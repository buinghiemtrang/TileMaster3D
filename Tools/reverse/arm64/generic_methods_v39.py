"""List generic method instantiations (method + generic args -> code RVA) of an IL2CPP v29+ ARM64 build.

Generic methods (Foo<T>) have no entry in the per-image method pointer tables, so gen_sigs/sigs.tsv
miss them. Their code is reached via:
  MetadataRegistration.genericMethodTable[i] = {genericMethodIndex, methodIndex, invokerIndex, adjustorThunkIndex}
  genericMethodIndex -> MetadataRegistration.methodSpecs[] = {methodDefinitionIndex, classIndexIndex, methodIndexIndex}
  methodIndex        -> CodeRegistration.genericMethodPointers[]
CodeRegistration is located by searching .data for (codeGenModulesCount, codeGenModules) whose array holds the
module addresses REA reported, then scanning backwards for a (count, ptr) pair whose count matches the table.

argv: relocated.so, REA evidence bundle (with binary), out.tsv
out.tsv: rva<TAB>ghidra_addr(rva+0x100000)<TAB>name   (name: Ns.Type<args>$$Method<args>; shared code shows __Il2CppFullySharedGenericType or object)
"""
import json
import struct
import sys

GHIDRA_BASE = 0x100000


def main(so_path, ev_path, out_path):
    so = open(so_path, "rb").read()
    ev = json.load(open(ev_path))
    n = [r for r in ev["records"] if r.get("operation") == "inspect_il2cpp_metadata"][0]["normalized_result"]
    types, methods = n["types"], n["methods"]

    e_phoff, = struct.unpack_from("<Q", so, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", so, 0x36)
    loads, rw = [], []
    for i in range(e_phnum):
        p_type, p_flags, p_offset, p_vaddr, _, p_filesz, _, _ = struct.unpack_from("<IIQQQQQQ", so, e_phoff + i * e_phentsize)
        if p_type == 1:
            loads.append((p_vaddr, p_offset, p_filesz))
            if p_flags & 2:
                rw.append((p_vaddr, p_offset, p_filesz))

    def v2o(va):
        for vaddr, off, size in loads:
            if vaddr <= va < vaddr + size:
                return off + va - vaddr
        return None

    def q(va):
        return struct.unpack_from("<Q", so, v2o(va))[0]

    tf = int(n["binary"]["types_field"], 16)          # &MetadataRegistration.typesCount
    types_cnt, types_arr = q(tf), q(tf + 8)
    specs_cnt, specs_arr = q(tf + 0x10), q(tf + 0x18)
    ginst_cnt, ginst_arr = q(tf - 0x20), q(tf - 0x18)
    gmt_cnt, gmt_arr = q(tf - 0x10), q(tf - 0x08)

    def tname(t):
        T = types[t]
        name = T["name"]
        while T["declaring_type"] != -1:
            T = types[T["declaring_type"]]
            name = T["name"] + "/" + name
        return (T["namespace"] + "." if T["namespace"] else "") + name

    PRIM = {1: "void", 2: "bool", 3: "char", 4: "sbyte", 5: "byte", 6: "short", 7: "ushort", 8: "int", 9: "uint",
            0xa: "long", 0xb: "ulong", 0xc: "float", 0xd: "double", 0xe: "string", 0x18: "IntPtr", 0x19: "UIntPtr",
            0x1c: "object"}

    def il2type_at(p, depth=0):
        o = v2o(p) if p else None
        if o is None or depth > 5:
            return "?"
        data, bits = struct.unpack_from("<QI", so, o)
        et = (bits >> 16) & 0xFF
        if et in PRIM:
            return PRIM[et]
        if et in (0x11, 0x12):
            return tname(data) if data < len(types) else "?"
        if et == 0x1D:
            return il2type_at(data, depth + 1) + "[]"
        if et == 0x14:
            return il2type_at(q(data), depth + 1) + "[,]"
        if et == 0x15:
            gen = il2type_at(q(data), depth + 1).split("`")[0]
            inst = q(data + 8)
            args = []
            if inst and v2o(inst) is not None:
                argc, argv = q(inst), q(inst + 8)
                args = [il2type_at(q(argv + k * 8), depth + 1) for k in range(min(argc, 6))]
            return gen + "<" + ",".join(args) + ">"
        if et in (0x13, 0x1E):
            return "T"
        return "?"

    def ginst(i):
        if i < 0 or i >= ginst_cnt:
            return ""
        inst = q(ginst_arr + i * 8)
        argc, argv = q(inst), q(inst + 8)
        return "<" + ",".join(il2type_at(q(argv + k * 8), 1) for k in range(min(argc, 6))) + ">"

    # --- CodeRegistration (v29+ layout, checked on TM3D v39) ---
    # REA's code_gen_modules_field = &codeGenModulesCount. Going back: -0x68 genericMethodPointersCount,
    # -0x60 genericMethodPointers (then adjustorThunks, invokers, unresolved*, interop, winrt, codeGenModules).
    cgm = int(n["binary"]["code_gen_modules_field"], 16)
    if q(cgm) != len(n["binary"]["code_gen_modules"]):
        sys.exit("code_gen_modules_field does not hold codeGenModulesCount")
    gmp_cnt, gmp_arr = q(cgm - 0x68), q(cgm - 0x60)
    table = [struct.unpack_from("<iiii", so, v2o(gmt_arr + i * 16))[:2] for i in range(gmt_cnt)]
    if max(mi for _, mi in table) >= gmp_cnt:
        sys.exit("genericMethodTable.methodIndex exceeds genericMethodPointersCount: layout mismatch")

    rows = []
    for gmi, mi in table:
        if not 0 <= gmi < specs_cnt or not 0 <= mi < gmp_cnt:
            continue
        mdi, cls_inst, m_inst = struct.unpack_from("<iii", so, v2o(specs_arr + gmi * 12))
        if not 0 <= mdi < len(methods):
            continue
        ptr = q(gmp_arr + mi * 8)
        if not ptr:
            continue
        m = methods[mdi]
        name = tname(m["declaring_type"]).split("`")[0] + ginst(cls_inst) + "$$" + m["name"] + ginst(m_inst)
        rows.append((ptr, name))
    with open(out_path, "w") as w:
        for ptr, name in rows:
            w.write(f"0x{ptr:x}\t0x{ptr + GHIDRA_BASE:x}\t{name}\n")
    print("genericMethodTable", gmt_cnt, "pointers", gmp_cnt, "rows", len(rows))


if __name__ == "__main__":
    main(*sys.argv[1:4])
