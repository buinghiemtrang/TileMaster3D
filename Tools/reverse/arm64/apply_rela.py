"""Apply plain (non-APS2) ELF64 aarch64 RELA relocations to a copy of a .so.

For static analysis only (Ghidra, gen_sigs, field_offsets): RELATIVE relocs are
written as base(0)+addend, ABS64/GLOB_DAT to locally defined symbols as S+A,
imports stay untouched. DT_RELA/DT_RELASZ/DT_RELACOUNT are left in place: most
readers ignore them for a DYN file loaded at base 0 because the slot values
now equal what the loader would write.

argv: src.so dst.so
"""
import struct
import sys

R_AARCH64_ABS64, R_AARCH64_GLOB_DAT, R_AARCH64_JUMP_SLOT, R_AARCH64_RELATIVE = 257, 1025, 1026, 1027


def main(src, dst):
    data = bytearray(open(src, "rb").read())
    e_phoff, = struct.unpack_from("<Q", data, 0x20)
    e_phentsize, e_phnum = struct.unpack_from("<HH", data, 0x36)
    loads, dyn_off = [], None
    for i in range(e_phnum):
        p_type, _, p_offset, p_vaddr, _, p_filesz, _, _ = struct.unpack_from(
            "<IIQQQQQQ", data, e_phoff + i * e_phentsize)
        if p_type == 1:
            loads.append((p_vaddr, p_offset, p_filesz))
        elif p_type == 2:
            dyn_off = p_offset

    def v2o(va):
        for vaddr, off, size in loads:
            if vaddr <= va < vaddr + size:
                return off + va - vaddr
        return None

    tags, pos = {}, dyn_off
    while True:
        tag, val = struct.unpack_from("<qQ", data, pos)
        if tag == 0:
            break
        tags.setdefault(tag, val)
        pos += 16
    if 0x60000011 in tags:
        raise SystemExit("APS2 (DT_ANDROID_RELA) present: use arm64/unpack_aps2.py instead")
    symtab, strtab = tags[6], tags[5]

    def sym_value(idx):
        st_name, st_info, st_other, st_shndx, st_value, st_size = struct.unpack_from(
            "<IBBHQQ", data, v2o(symtab) + idx * 24)
        return st_value if st_shndx != 0 else None

    stats = {"relative": 0, "abs_local": 0, "import": 0, "other": 0, "unmapped": 0}
    tables = [(tags.get(7), tags.get(8))]           # DT_RELA, DT_RELASZ
    if tags.get(20) == 7:                          # DT_PLTREL == DT_RELA
        tables.append((tags.get(23), tags.get(2)))  # DT_JMPREL, DT_PLTRELSZ
    for va, size in tables:
        if not va or not size:
            continue
        base = v2o(va)
        for k in range(size // 24):
            r_offset, r_info, r_addend = struct.unpack_from("<QQq", data, base + k * 24)
            rtype, rsym = r_info & 0xFFFFFFFF, r_info >> 32
            off = v2o(r_offset)
            if off is None:
                stats["unmapped"] += 1
                continue
            if rtype == R_AARCH64_RELATIVE:
                struct.pack_into("<Q", data, off, r_addend & 0xFFFFFFFFFFFFFFFF)
                stats["relative"] += 1
            elif rtype in (R_AARCH64_ABS64, R_AARCH64_GLOB_DAT, R_AARCH64_JUMP_SLOT):
                sv = sym_value(rsym)
                if sv is None:
                    stats["import"] += 1
                else:
                    struct.pack_into("<Q", data, off, (sv + r_addend) & 0xFFFFFFFFFFFFFFFF)
                    stats["abs_local"] += 1
            else:
                stats["other"] += 1
    open(dst, "wb").write(data)
    print(stats)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
