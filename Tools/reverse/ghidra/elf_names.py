"""Name functions of libil2cpp.so that are NOT managed methods, straight from the ELF (no guessing).

usage: python -I elf_names.py <libil2cpp.so> <out.json>      (needs `pip install pyelftools`)
Output: {"<rva hex, no 0x>": "name"} where rva = ELF virtual address (Ghidra address = rva + image base 0x10000).
Sources:
  1. .dynsym symbols that are defined (il2cpp_* C API, C++ runtime symbols). Thumb bit is cleared.
  2. .plt stubs: each stub (add ip,pc,#a / add ip,ip,#b / ldr pc,[ip,#c]!) jumps through a .got slot; the matching
     .rel.plt relocation names the imported function (memset, memcpy, __stack_chk_fail, sinf, ...).
     Entries are written as `<name>` (imports) so callers read `memset(p, 0, n)` instead of `native_1aba5ac`.
  3. Data imports (R_ARM_GLOB_DAT in .rel.dyn) such as `__stack_chk_guard`: keyed by .got slot address.
ARMv7 only (PLT decoding). Does not depend on the game.
"""
import json, struct, sys
from elftools.elf.elffile import ELFFile

src, out = sys.argv[1], sys.argv[2]
f = open(src, 'rb')
e = ELFFile(f)
names = {}
ds = e.get_section_by_name('.dynsym')
for s in ds.iter_symbols():
    if s['st_shndx'] != 'SHN_UNDEF' and s['st_value'] and s['st_info']['type'] in ('STT_FUNC', 'STT_OBJECT', 'STT_NOTYPE'):
        names.setdefault('%x' % (s['st_value'] & ~1), s.name)


def rot(imm8, r):
    v = imm8
    r = r * 2
    return ((v >> r) | (v << (32 - r))) & 0xffffffff if r else v


got_to_name = {}
for secname in ('.rel.plt', '.rel.dyn'):
    sec = e.get_section_by_name(secname)
    if not sec:
        continue
    for r in sec.iter_relocations():
        if r['r_info_sym']:
            sym = ds.get_symbol(r['r_info_sym'])
            if sym['st_shndx'] == 'SHN_UNDEF' and sym.name:
                got_to_name[r['r_offset']] = sym.name

plt = e.get_section_by_name('.plt')
data = plt.data()
base = plt['sh_addr']
plt_named = 0
# entries: 20-byte header, then 12 bytes each; decode instead of assuming, scan every 4 bytes for the 3-insn pattern
for off in range(0, len(data) - 12, 4):
    a, b, c = struct.unpack_from('<III', data, off)
    if (a & 0xfffff000) == 0xe28fc000 and (b & 0xfffff000) == 0xe28cc000 and (c & 0xfffff000) == 0xe5bcf000:
        pc = base + off + 8
        ip = pc + rot(a & 0xff, (a >> 8) & 0xf)
        ip = ip + rot(b & 0xff, (b >> 8) & 0xf)
        got = ip + (c & 0xfff)
        n = got_to_name.get(got)
        if n:
            names['%x' % (base + off)] = n
            plt_named += 1
json.dump(names, open(out, 'w'), indent=0)
print(len(names), 'names;', plt_named, 'plt stubs;', len([1 for k in got_to_name]), 'imports')
json.dump({'%x' % k: v for k, v in got_to_name.items()}, open(out.replace('.json', '.got.json'), 'w'), indent=0)
