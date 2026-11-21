"""Read IL2CPP MetadataRegistration.fieldOffsets from a relocated libil2cpp.so.

argv: relocated .so, REA evidence bundle (inspect_il2cpp_metadata with --binary),
output JSON. Output maps "Namespace.Type" -> [[field, offset, type_name], ...].
"""
import json
import struct
import sys


def main(so_path, ev_path, out_path):
    data = open(so_path, "rb").read()
    ev = json.load(open(ev_path))
    rec = [r for r in ev["records"] if r.get("operation") == "inspect_il2cpp_metadata"][0] if "records" in ev else ev
    n = rec["normalized_result"]
    e_phoff, = struct.unpack_from("<Q", data, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", data, 0x36)
    loads = []
    for i in range(e_phnum):
        p_type, _, p_offset, p_vaddr, _, p_filesz, _, _ = struct.unpack_from(
            "<IIQQQQQQ", data, e_phoff + i * e_phentsize)
        if p_type == 1:
            loads.append((p_vaddr, p_offset, p_filesz))

    def v2o(va):
        for vaddr, off, size in loads:
            if vaddr <= va < vaddr + size:
                return off + va - vaddr
        raise ValueError(hex(va))

    def q(va):
        return struct.unpack_from("<Q", data, v2o(va))[0]

    types_field = int(n["binary"]["types_field"], 16)
    # types_field points at typesCount: typesCount, types, methodSpecsCount, methodSpecs,
    # fieldOffsetsCount, fieldOffsets, ...
    count, table = q(types_field + 0x20), q(types_field + 0x28)
    types, fields, ftypes = n["types"], n["fields"], n["field_types"]
    assert count == len(types), (count, len(types))

    by_type = {}
    for idx, f in enumerate(fields):
        by_type.setdefault(f["declaring_type"], []).append(idx)

    def full_name(i):
        t = types[i]
        name = t["name"]
        while t["declaring_type"] != -1:
            t = types[t["declaring_type"]]
            name = t["name"] + "/" + name
        return (t["namespace"] + "." if t["namespace"] else "") + name

    out = {}
    for ti in range(count):
        ptr = q(table + ti * 8)
        idxs = by_type.get(ti, [])
        if not ptr or not idxs:
            continue
        rows = []
        for k, fi in enumerate(idxs):
            off = struct.unpack_from("<i", data, v2o(ptr + k * 4))[0]
            rows.append([fields[fi]["name"], off, ftypes[fi].get("name")])
        out[full_name(ti)] = rows
    json.dump(out, open(out_path, "w"))
    print("types with offsets:", len(out))


if __name__ == "__main__":
    main(*sys.argv[1:4])
