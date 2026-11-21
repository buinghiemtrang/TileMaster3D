"""Map ARM veneer targets (ResolveVeneers.java output) to managed method names from dump.cs.

usage: python -I build_veneer_names.py <dump.cs> <veneers.tsv> <out veneer_names.json>

veneers.tsv: `<ghidra address hex>\t<target RVA hex or ?>` written by ResolveVeneers.java.
The JSON maps the 8-digit Ghidra address (as it appears in FUN_xxxxxxxx / thunk_FUN_xxxxxxxx) to
`Class.Method`; targets without a managed method get `native_<rva>`.
"""
import json, re, sys

dump, veneers, out = sys.argv[1:4]
by_rva = {}
cls = None
pending = None
for line in open(dump, encoding='utf8', errors='replace'):
    line = line.rstrip('\n')
    m = re.match(r'^(?:\[.*\]\s*)?[\w\s]*\b(?:class|struct|interface)\s+([\w.<>`,]+)', line)
    if m and '// TypeDefIndex' in line:
        cls = m.group(1)
        continue
    m = re.match(r'^\s*// RVA: 0x([0-9A-F]+)', line)
    if m:
        pending = int(m.group(1), 16)
        continue
    if pending is not None and cls and '(' in line and not line.strip().startswith('//'):
        name = re.search(r'([\w.<>$`|]+)\s*(?:<[^()]*>)?\(', line)
        by_rva.setdefault(pending, f'{cls}.{name.group(1) if name else "?"}')
        pending = None

names = {}
for line in open(veneers):
    addr, target = line.rstrip('\n').split('\t')
    if target == '?':
        continue
    rva = int(target, 16)
    names[addr] = by_rva.get(rva, 'native_%x' % rva).replace(' ', '')
json.dump(names, open(out, 'w'), indent=0)
print(len(names), 'veneers,', sum(not v.startswith('native_') for v in names.values()), 'with a managed name')
