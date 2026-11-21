#!/usr/bin/env python3
"""Disassemble ARM (A32) code and read literals straight from the il2cpp .so, no Ghidra needed.

  arm_dis.py dis   <libil2cpp.so> <script.json> <start_rva> <end_rva>   # annotated listing
  arm_dis.py words <libil2cpp.so> <rva> [<rva> ...]                      # 4 words + floats at each RVA

RVA = address in the pseudo-C minus 0x10000 (Ghidra image base). `dis` prints method names on bl/b
(from il2cpp script.json) and resolves `ldr r, [pc, #n]` / `vldr s, [pc, #n]` literals as hex + float.
Capstone runs with skipdata so literal pools sitting between functions do not stop the listing.
Needs: pip install capstone pyelftools. No dependency on the game; paths are arguments.

Do not save this file (or any scratch script) as `dis.py` next to the files you run: it shadows the
standard-library `dis` module and breaks `import inspect` inside pyelftools.
"""
import json
import re
import struct
import sys

from elftools.elf.elffile import ELFFile


def load_segments(path):
    f = open(path, 'rb')
    elf = ELFFile(f)
    segs = [(s['p_vaddr'], s['p_offset'], s['p_filesz']) for s in elf.iter_segments() if s['p_type'] == 'PT_LOAD']

    def read(addr, n):
        for base, off, size in segs:
            if base <= addr and addr + n <= base + size:
                f.seek(off + addr - base)
                return f.read(n)
        return b''

    return read


def cmd_words(so, rvas):
    read = load_segments(so)
    for a in rvas:
        r = int(a, 16)
        b = read(r, 16)
        if len(b) < 16:
            print(a, 'unmapped')
            continue
        w = struct.unpack('<4I', b)
        fl = struct.unpack('<4f', b)
        print(a, ' '.join('%08x' % x for x in w), ' | ', ' '.join('%.9g' % x for x in fl))


def cmd_dis(so, script_json, start, end):
    from capstone import CS_ARCH_ARM, CS_MODE_ARM, Cs
    read = load_segments(so)
    names = {m['Address']: m['Name'] for m in json.load(open(script_json, encoding='utf-8'))['ScriptMethod']}
    md = Cs(CS_ARCH_ARM, CS_MODE_ARM)
    md.skipdata = True
    for ins in md.disasm(read(start, end - start), start):
        if ins.mnemonic == '.byte':
            continue
        note = ''
        if ins.mnemonic.startswith('b') and ins.op_str.startswith('#0x'):
            note = names.get(int(ins.op_str[1:], 16), '')
        m = re.search(r'\[pc(?:, #(-?0x[0-9a-f]+|-?\d+))?\]', ins.op_str)
        if m:
            a = ins.address + 8 + int(m[1] or '0', 0)
            b = read(a, 4)
            if len(b) == 4:
                note += ' literal@%x=%08x float=%.10g' % (a, struct.unpack('<I', b)[0], struct.unpack('<f', b)[0])
        print('%08x: %-12s %-45s %s' % (ins.address, ins.mnemonic, ins.op_str, note))


def main():
    sys.stdout.reconfigure(encoding='utf-8')
    if len(sys.argv) >= 5 and sys.argv[1] == 'dis':
        cmd_dis(sys.argv[2], sys.argv[3], int(sys.argv[4], 16), int(sys.argv[5], 16))
    elif len(sys.argv) >= 4 and sys.argv[1] == 'words':
        cmd_words(sys.argv[2], sys.argv[3:])
    else:
        print(__doc__)
        sys.exit(2)


if __name__ == '__main__':
    main()
