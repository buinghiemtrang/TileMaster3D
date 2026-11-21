"""Annotate ListInstr.java output: append the managed method name to every `bl/b/blx 0x...` whose target is a method.

usage: python3 -I annotate_listing.py <analyzeHeadless log containing ListInstr lines>   (env WORK, default ~/Desktop/royal-smash-work)
Ghidra addresses = RVA + 0x10000 (ARMv7 image base used in this project). Not game specific.
"""
import json, os, re, sys

work = os.environ.get('WORK', os.path.expanduser('~/Desktop/royal-smash-work'))
d = json.load(open(work + '/il2cpp/script.json'))
names = {m['Address'] + 0x10000: m['Name'] for m in d['ScriptMethod']}
for line in open(sys.argv[1], encoding='utf-8', errors='replace'):
    if 'ListInstr.java>' not in line:
        continue
    line = line.split('ListInstr.java> ')[1].replace(' (GhidraScript)', '').rstrip()
    m = re.search(r'\b(bl|b|blx)\s+0x([0-9a-f]+)', line)
    if m and int(m.group(2), 16) in names:
        line += '   ; ' + names[int(m.group(2), 16)]
    print(line)
