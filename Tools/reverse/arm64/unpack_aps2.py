"""Apply Android packed (APS2) RELA relocations to a copy of an ELF64 aarch64 .so.

Only for static analysis: RELATIVE relocations are written as base(0)+addend,
symbol relocations to locally defined symbols as S+A, imports stay untouched.
The DT_ANDROID_RELA/RELASZ tags are neutralised so readers treat the file as
already relocated. Format: bionic linker/linker_reloc_iterators.h.
"""
import struct
import sys

DT_ANDROID_RELA, DT_ANDROID_RELASZ = 0x60000011, 0x60000012
R_AARCH64_ABS64, R_AARCH64_GLOB_DAT, R_AARCH64_RELATIVE = 257, 1025, 1027
G_BY_INFO, G_BY_OFFSET, G_BY_ADDEND, G_HAS_ADDEND = 1, 2, 4, 8


def sleb(buf, pos):
    result = shift = 0
    while True:
        b = buf[pos]
        pos += 1
        result |= (b & 0x7F) << shift
        shift += 7
        if not b & 0x80:
            if b & 0x40:
                result -= 1 << shift
            return result, pos


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
        tags.setdefault(tag, (val, pos))
        pos += 16
    rela_va, rela_tag_pos = tags[DT_ANDROID_RELA]
    rela_sz, relasz_tag_pos = tags[DT_ANDROID_RELASZ]
    symtab, strtab = tags[6][0], tags[5][0]

    buf = bytes(data[v2o(rela_va):v2o(rela_va) + rela_sz])
    assert buf[:4] == b"APS2", buf[:4]
    pos = 4
    count, pos = sleb(buf, pos)
    r_offset, pos = sleb(buf, pos)
    r_info = r_addend = 0
    done = 0
    stats = {"relative": 0, "symbol": 0, "import": 0, "unmapped": 0, "other": 0}
    while done < count:
        gsize, pos = sleb(buf, pos)
        gflags, pos = sleb(buf, pos)
        if gflags & G_BY_OFFSET:
            gdelta, pos = sleb(buf, pos)
        if gflags & G_BY_INFO:
            r_info, pos = sleb(buf, pos)
        if gflags & G_HAS_ADDEND and gflags & G_BY_ADDEND:
            d, pos = sleb(buf, pos)
            r_addend += d
        elif not gflags & G_HAS_ADDEND:
            r_addend = 0
        for _ in range(gsize):
            if gflags & G_BY_OFFSET:
                r_offset += gdelta
            else:
                d, pos = sleb(buf, pos)
                r_offset += d
            if not gflags & G_BY_INFO:
                r_info, pos = sleb(buf, pos)
            if gflags & G_HAS_ADDEND and not gflags & G_BY_ADDEND:
                d, pos = sleb(buf, pos)
                r_addend += d
            done += 1
            rtype, rsym = r_info & 0xFFFFFFFF, r_info >> 32
            off = v2o(r_offset)
            if off is None:
                stats["unmapped"] += 1
                continue
            if rtype == R_AARCH64_RELATIVE:
                struct.pack_into("<Q", data, off, r_addend & 0xFFFFFFFFFFFFFFFF)
                stats["relative"] += 1
            elif rtype in (R_AARCH64_ABS64, R_AARCH64_GLOB_DAT):
                st_value, = struct.unpack_from("<Q", data, v2o(symtab) + rsym * 24 + 8)
                shndx, = struct.unpack_from("<H", data, v2o(symtab) + rsym * 24 + 6)
                if shndx:
                    struct.pack_into("<Q", data, off, (st_value + r_addend) & 0xFFFFFFFFFFFFFFFF)
                    stats["symbol"] += 1
                else:
                    stats["import"] += 1
            else:
                stats["other"] += 1
    # DT_DEBUG(21) entries are ignored by static readers.
    struct.pack_into("<qQ", data, rela_tag_pos, 21, 0)
    struct.pack_into("<qQ", data, relasz_tag_pos, 21, 0)
    open(dst, "wb").write(data)
    print(f"relocations={count}", stats)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
