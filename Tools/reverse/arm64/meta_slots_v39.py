"""Name IL2CPP metadata-usage slots (v27+) found in .data of a relocated ARM64 libil2cpp.so.

Before runtime init every slot holds (usage << 29) | (index << 1) | 1:
1 TypeInfo, 2 Il2CppType (index into MetadataRegistration.types), 3 MethodDef,
4 FieldRef (metadata fieldRefs), 5 StringLiteral, 6 MethodRef (methodSpecs).

argv: relocated.so, global-metadata.dat, REA evidence bundle (with binary), out.tsv, ghidra base (hex)
out.tsv: ghidra_address<TAB>label   (feed to ghidra/ApplyLabels.java)
"""
import json
import re
import struct
import sys


def main(so_path, meta_path, ev_path, out_path, base):
    base = int(base, 16)
    so = open(so_path, "rb").read()
    meta = open(meta_path, "rb").read()
    ev = json.load(open(ev_path))
    n = [r for r in ev["records"] if r.get("operation") == "inspect_il2cpp_metadata"][0]["normalized_result"]
    types, methods, fields, sls = n["types"], n["methods"], n["fields"], n["string_literals"]
    sect = {s["name"]: s for s in n["sections"]}

    e_phoff, = struct.unpack_from("<Q", so, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", so, 0x36)
    loads = []
    for i in range(e_phnum):
        p_type, _, p_offset, p_vaddr, _, p_filesz, _, _ = struct.unpack_from("<IIQQQQQQ", so, e_phoff + i * e_phentsize)
        if p_type == 1:
            loads.append((p_vaddr, p_offset, p_filesz))

    def v2o(va):
        for vaddr, off, size in loads:
            if vaddr <= va < vaddr + size:
                return off + va - vaddr
        return None

    def q(va):
        return struct.unpack_from("<Q", so, v2o(va))[0]

    tf = int(n["binary"]["types_field"], 16)          # &typesCount
    types_cnt, types_arr = q(tf), q(tf + 8)
    specs_cnt, specs_arr = q(tf + 0x10), q(tf + 0x18)
    ginst_cnt, ginst_arr = q(tf - 0x20), q(tf - 0x18)   # MetadataRegistration.genericInsts

    def tname(t):
        T = types[t]
        name = T["name"]
        while T["declaring_type"] != -1:
            T = types[T["declaring_type"]]
            name = T["name"] + "." + name
        return (T["namespace"] + "." if T["namespace"] else "") + name

    def il2type_name(idx, depth=0):
        if idx >= types_cnt or depth > 4:
            return "?"
        p = q(types_arr + idx * 8)
        return il2type_at(p, depth)

    PRIM = {1: "void", 2: "bool", 3: "char", 4: "sbyte", 5: "byte", 6: "short", 7: "ushort", 8: "int", 9: "uint",
            0xa: "long", 0xb: "ulong", 0xc: "float", 0xd: "double", 0xe: "string", 0x18: "IntPtr", 0x19: "UIntPtr",
            0x1c: "object"}
    ptr_to_idx = {}

    def il2type_at(p, depth=0):
        o = v2o(p)
        if o is None:
            return "?"
        data, bits = struct.unpack_from("<QI", so, o)
        et = (bits >> 16) & 0xFF
        if et in PRIM:
            return PRIM[et]
        if et in (0x11, 0x12):                       # VALUETYPE, CLASS: data = TypeDefinitionIndex
            return tname(data) if data < len(types) else "?"
        if et == 0x1D:                               # SZARRAY: data = element Il2CppType*
            return il2type_at(data, depth + 1) + "[]"
        if et == 0x14:                               # ARRAY: data = Il2CppArrayType* (etype first)
            return il2type_at(q(data), depth + 1) + "[,]"
        if et == 0x15:                               # GENERICINST: data = Il2CppGenericClass* {type*, context}
            gen = il2type_at(q(data), depth + 1).split("`")[0]
            inst = q(data + 8)                       # class_inst: Il2CppGenericInst* {argc, argv}
            args = []
            if inst and v2o(inst) is not None:
                argc, argv = q(inst), q(inst + 8)
                for k in range(min(argc, 4)):
                    args.append(il2type_at(q(argv + k * 8), depth + 1))
            return gen + "<" + ",".join(args) + ">"
        if et in (0x13, 0x1E):                       # VAR, MVAR
            return "T"
        return "?"

    def mname(mi):
        m = methods[mi]
        return tname(m["declaring_type"]) + "$$" + m["name"]

    fields_by_type = {}
    for fi, f in enumerate(fields):
        fields_by_type.setdefault(f["declaring_type"], []).append(f["name"])
    fr = sect["fieldRefs"]
    fr_size = fr["size"] // fr["count"] if fr["count"] else 8

    def fieldref(idx):
        off = fr["offset"] + idx * fr_size
        if fr_size == 6:
            tidx, fidx = struct.unpack_from("<Hi", meta, off)
        else:
            tidx, fidx = struct.unpack_from("<ii", meta, off)
        p = q(types_arr + tidx * 8)
        data, bits = struct.unpack_from("<QI", so, v2o(p))
        t = data if ((bits >> 16) & 0xFF) in (0x11, 0x12) else None
        if t is None or t >= len(types):
            return "?"
        names = fields_by_type.get(t, [])
        return tname(t) + "." + (names[fidx] if fidx < len(names) else f"field{fidx}")

    def ginst(i):
        if i < 0 or i >= ginst_cnt:
            return ""
        inst = q(ginst_arr + i * 8)
        argc, argv = q(inst), q(inst + 8)
        return "<" + ",".join(il2type_at(q(argv + k * 8), 1) for k in range(min(argc, 4))) + ">"

    def methodref(idx):
        if idx >= specs_cnt:
            return "?"
        mdi, cls_inst, m_inst = struct.unpack_from("<iii", so, v2o(specs_arr + idx * 12))
        if not 0 <= mdi < len(methods):
            return "?"
        m = methods[mdi]
        return tname(m["declaring_type"]).split("`")[0] + ginst(cls_inst) + "$$" + m["name"] + ginst(m_inst)

    def sanitize(s, limit=160):
        s = re.sub(r"[^A-Za-z0-9_$.<>,\[\]]", "_", s)
        return s[:limit]

    limits = {1: types_cnt, 2: types_cnt, 3: len(methods), 4: fr["count"], 5: len(sls), 6: specs_cnt}
    data_sect = [(va, off, size) for va, off, size in loads]  # scan RW segments only
    rw = []
    for i in range(e_phnum):
        p_type, p_flags, p_offset, p_vaddr, _, p_filesz, _, _ = struct.unpack_from("<IIQQQQQQ", so, e_phoff + i * e_phentsize)
        if p_type == 1 and p_flags & 2:
            rw.append((p_vaddr, p_offset, p_filesz))
    out, seen, counts = [], {}, {}
    prefix = {1: "TI_", 2: "TY_", 3: "MD_", 4: "FR_", 5: "SL_", 6: "MR_"}
    for vaddr, off, size in rw:
        for k in range(0, size - 7, 8):
            v, = struct.unpack_from("<Q", so, off + k)
            if not v & 1 or v >> 32:
                continue
            usage, idx = (v >> 29) & 7, (v >> 1) & 0x0FFFFFFF
            if usage not in limits or idx >= limits[usage]:
                continue
            try:
                if usage in (1, 2):
                    name = il2type_name(idx)
                elif usage == 3:
                    name = mname(idx)
                elif usage == 4:
                    name = fieldref(idx)
                elif usage == 5:
                    name = '"' + sls[idx]["value"] + '"'
                else:
                    name = methodref(idx)
            except Exception:
                name = "?"
            if name.startswith("?"):
                continue
            label = prefix[usage] + sanitize(name)
            seen[label] = seen.get(label, 0) + 1
            if seen[label] > 1:
                label += f"_{seen[label]}"
            out.append((vaddr + k + base, label))
            counts[usage] = counts.get(usage, 0) + 1
    with open(out_path, "w") as w:
        for a, l in out:
            w.write(f"0x{a:x}\t{l}\n")
    print({prefix[k][:-1]: v for k, v in sorted(counts.items())}, "total", len(out))


if __name__ == "__main__":
    main(*sys.argv[1:6])
