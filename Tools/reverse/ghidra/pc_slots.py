"""Resolve IL2CPP metadata slots loaded PC-relatively in ARM code:
    ldr rX,[<literal>]      ; literal word = offset
    ldr rX,[pc,rX]          ; slot = address_of_this_ldr + 8 + offset, word at slot = metadata usage address
The usage address (minus the 0x10000 image base) is looked up in script.json (strings, types, methods).

usage: python3 -I pc_slots.py <pcldr_ghidra_hex>:<literal_ghidra_hex> ...   (env WORK; Ghidra must not have the project open)
Example: method of the second AddListener in an Awake, a string literal passed to LogWarning. Not game specific.
"""
import json, os, re, subprocess, sys

here = os.path.dirname(os.path.abspath(__file__))
work = os.environ.get('WORK', os.path.expanduser('~/Desktop/royal-smash-work'))
pairs = [tuple(int(x, 16) for x in a.split(':')) for a in sys.argv[1:]]


def words(rvas):
    out = subprocess.run([os.path.join(here, 'read_floats.sh')] + [hex(r) for r in rvas], capture_output=True, text=True).stdout
    return [int(re.search(r'\(0x([0-9a-f]+)\)', l).group(1), 16) for l in out.strip().split('\n') if l.startswith('RVA')]


lits = words([lit - 0x10000 for _, lit in pairs])
slots = [(pc + 8 + w) & 0xffffffff for (pc, _), w in zip(pairs, lits)]
vals = words([s - 0x10000 for s in slots])
d = json.load(open(work + '/il2cpp/script.json'))
names = {m['Address']: json.dumps(m['Value'], ensure_ascii=False) for m in d['ScriptString']}
for k in ('ScriptMetadata', 'ScriptMetadataMethod'):
    for m in d[k]:
        names[m['Address']] = m['Name']
for (pc, _), v in zip(pairs, vals):
    print('%x' % pc, names.get(v - 0x10000, '?%x' % v))
