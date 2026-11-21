"""Dump vtable slots of every type from IL2CPP metadata v39 (Il2CppTypeDefinition = 76 bytes).

Pseudo-C virtual call `(**(code **)(*obj + OFF))(obj, ..., *(obj->klass + OFF + 8))` -> slot = (OFF - VBASE) / 16.
VBASE for this build is printed by the self-check (System.Object slot 3 must be ToString).

argv: global-metadata.dat, REA evidence bundle, out.tsv [vbase hex, default 0x138]
out.tsv: Type<TAB>slot<TAB>offset<TAB>Method  (Method = DeclaringType$$Name)
"""
import json
import struct
import sys

TD = "<IIHHHHIIIIIIIIIHHHHHHHHII"   # 76 bytes, field widths from REA index_widths (typedef/type/container = 2)


def main(meta_path, ev_path, out_path, vbase="0x138"):
    vbase = int(vbase, 16)
    meta = open(meta_path, "rb").read()
    ev = json.load(open(ev_path))
    n = [r for r in ev["records"] if r.get("operation") == "inspect_il2cpp_metadata"][0]["normalized_result"]
    types, methods = n["types"], n["methods"]
    sect = {s["name"]: s for s in n["sections"]}
    td, vt = sect["typeDefinitions"], sect["vtableMethods"]
    assert td["size"] // td["count"] == struct.calcsize(TD) == 76

    def tname(t):
        T = types[t]
        name = T["name"]
        while T["declaring_type"] != -1:
            T = types[T["declaring_type"]]
            name = T["name"] + "." + name
        return (T["namespace"] + "." if T["namespace"] else "") + name

    bad = 0
    rows = []
    for ti in range(td["count"]):
        f = struct.unpack_from(TD, meta, td["offset"] + ti * 76)
        method_start, vtable_start, method_count, vtable_count = f[8], f[13], f[15], f[20]
        if types[ti]["method_count"] and method_start != types[ti]["method_start"]:
            bad += 1
        for k in range(vtable_count):
            enc, = struct.unpack_from("<I", meta, vt["offset"] + (vtable_start + k) * 4)
            usage, idx = (enc >> 29) & 7, (enc & 0x1FFFFFFE) >> 1
            if usage == 3 and idx < len(methods):
                m = methods[idx]
                name = tname(m["declaring_type"]) + "$$" + m["name"]
            elif enc == 0:
                name = "(abstract/empty)"
            else:
                name = f"(usage{usage}:{idx})"
            rows.append((tname(ti), k, vbase + k * 16, name))
    with open(out_path, "w") as w:
        for t, k, off, name in rows:
            w.write(f"{t}\t{k}\t0x{off:x}\t{name}\n")
    obj = [r for r in rows if r[0] == "System.Object"]
    print("layout mismatches:", bad, "rows:", len(rows))
    print("System.Object:", [(r[1], hex(r[2]), r[3]) for r in obj])


if __name__ == "__main__":
    main(*sys.argv[1:5])
