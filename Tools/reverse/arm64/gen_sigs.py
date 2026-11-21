"""Build Ghidra struct and signature specs from IL2CPP metadata + relocated binary.

argv: global-metadata.dat, relocated libil2cpp.so, REA evidence bundle (with --binary),
      out dir, seed base (hex, Ghidra load offset of an ELF linked at 0).
Writes structs.tsv (struct, size, offset, field, token) and sigs.tsv
(address, name, ret token, params "name:token;...") for every linked method.
Tokens: v void, b bool, i1/u1/i2/u2/i4/u4/i8/u8, f float, d double,
p void*, P:<struct> pointer to struct, F2/F3/F4 float vectors.
"""
import json
import re
import struct
import sys

METHOD_SIZE = None  # derived from metadata section (v31: 36, v39: 30)
STATIC = 0x10
PRIM = {
    "void": "v", "bool": "b", "byte": "u1", "sbyte": "i1", "short": "i2", "ushort": "u2",
    "char": "u2", "int": "i4", "uint": "u4", "long": "i8", "ulong": "u8", "float": "f",
    "double": "d", "System.IntPtr": "u8", "System.UIntPtr": "u8", "nint": "u8", "nuint": "u8",
}
FLOATS = {
    "UnityEngine.Vector2": "F2", "UnityEngine.Vector3": "F3", "UnityEngine.Vector4": "F4",
    "UnityEngine.Quaternion": "F4", "UnityEngine.Color": "F4", "UnityEngine.Rect": "F4",
    "Unity.Mathematics.float2": "F2", "Unity.Mathematics.float3": "F3",
    "Unity.Mathematics.float4": "F4", "Unity.Mathematics.quaternion": "F4",
}
SMALL_INT = {"UnityEngine.LayerMask": "i4", "UnityEngine.Color32": "u4",
             "UnityEngine.Vector2Int": "u8"}


def sanitize(name):
    return re.sub(r"[^A-Za-z0-9_]", "_", name)


def main(meta_path, so_path, ev_path, out_dir, seed_base):
    seed_base = int(seed_base, 16)
    meta = open(meta_path, "rb").read()
    so = open(so_path, "rb").read()
    ev = json.load(open(ev_path))
    recs = ev["records"] if "records" in ev else [ev]
    n = [r for r in recs if r.get("operation") == "inspect_il2cpp_metadata"][0]["normalized_result"]
    types, fields, ftypes = n["types"], n["fields"], n["field_types"]
    methods, maddr = n["methods"], n["method_addresses"]
    rtypes, params, ptypes = n["return_types"], n["parameters"], n["parameter_types"]
    sect = {s["name"]: s for s in n["sections"]}

    e_phoff, = struct.unpack_from("<Q", so, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", so, 0x36)
    loads = []
    for i in range(e_phnum):
        p_type, _, p_offset, p_vaddr, _, p_filesz, _, _ = struct.unpack_from(
            "<IIQQQQQQ", so, e_phoff + i * e_phentsize)
        if p_type == 1:
            loads.append((p_vaddr, p_offset, p_filesz))

    def v2o(va):
        for vaddr, off, size in loads:
            if vaddr <= va < vaddr + size:
                return off + va - vaddr
        raise ValueError(hex(va))

    def q(va):
        return struct.unpack_from("<Q", so, v2o(va))[0]

    tf = int(n["binary"]["types_field"], 16)  # typesCount
    off_tab, size_cnt, size_tab = q(tf + 0x28), q(tf + 0x30), q(tf + 0x38)
    assert size_cnt == len(types)

    def full_name(i):
        t = types[i]
        name = t["name"]
        while t["declaring_type"] != -1:
            t = types[t["declaring_type"]]
            name = t["name"] + "/" + name
        return (t["namespace"] + "." if t["namespace"] else "") + name

    names = [full_name(i) for i in range(len(types))]
    by_type = {}
    for idx, f in enumerate(fields):
        by_type.setdefault(f["declaring_type"], []).append(idx)

    # Instance layout: increasing offsets inside instance_size, in declaration order.
    layouts, enum_of = {}, {}
    for ti, fidx in by_type.items():
        inst_size = struct.unpack_from("<I", so, v2o(q(size_tab + ti * 8)))[0]
        ptr = q(off_tab + ti * 8)
        if not ptr:
            continue
        rows, last = [], -1
        for k, fi in enumerate(fidx):
            off = struct.unpack_from("<i", so, v2o(ptr + k * 4))[0]
            fname, ftype = fields[fi]["name"], ftypes[fi].get("name") or "?"
            if fname == "value__":
                enum_of[names[ti]] = PRIM.get(ftype, "i4")
            if 0x10 <= off < inst_size and off > last:
                rows.append((off, fname, ftype))
                last = off
        layouts[ti] = (inst_size, rows)

    struct_names = {names[ti]: sanitize(names[ti]) for ti in layouts}

    def token(tname):
        if tname is None:
            return "p"
        if tname in PRIM:
            return PRIM[tname]
        if tname in FLOATS:
            return FLOATS[tname]
        if tname in SMALL_INT:
            return SMALL_INT[tname]
        if tname in enum_of:
            return enum_of[tname]
        if tname in struct_names and not tname.endswith("[]"):
            return "P:" + struct_names[tname]
        return "p"

    with open(f"{out_dir}/structs.tsv", "w") as w:
        for ti, (size, rows) in layouts.items():
            sn = struct_names[names[ti]]
            for off, fname, ftype in rows:
                tok = token(ftype)
                if tok.startswith("P:"):
                    tok = "p"  # keep struct graph flat; pointers stay void*
                w.write(f"{sn}\t{size}\t{off}\t{fname}\t{tok}\n")

    pstart = sect["methods"]["offset"]
    msize = sect["methods"]["size"] // sect["methods"]["count"]
    flags_off = msize - 8  # flags, iflags, slot, parameterCount are the last 4 uint16
    params_by_method = {}
    for pi, p in enumerate(params):
        params_by_method.setdefault(p["method"], []).append(pi)
    written = 0
    with open(f"{out_dir}/sigs.tsv", "w") as w:
        for mi, m in enumerate(methods):
            a = maddr[mi]
            if a.get("state") != "code":
                continue
            flags, = struct.unpack_from("<H", meta, pstart + mi * msize + flags_off)
            ti = m["declaring_type"]
            plist = []
            if not flags & STATIC:
                cls = names[ti]
                plist.append("this:" + ("P:" + struct_names[cls] if cls in struct_names else "p"))
            for pi in params_by_method.get(mi, []):
                pname = sanitize(params[pi]["name"]) or f"p{pi}"
                plist.append(f"{pname}:{token(ptypes[pi].get('name'))}")
            plist.append("method:p")
            addr = int(a["rva"], 16) + seed_base
            ret = token(rtypes[mi].get("name"))
            w.write(f"0x{addr:x}\t{names[ti]}$${m['name']}\t{ret}\t{';'.join(plist)}\n")
            written += 1
    print("structs:", len(layouts), "signatures:", written)


if __name__ == "__main__":
    main(*sys.argv[1:6])
