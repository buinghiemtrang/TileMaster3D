"""Resolve IL2CPP literal-pool slots (strings, type handles, method infos) written as `**(undefined4 **)(_UNK_<v> + 0x<a>)` in pseudo-C.

usage: python -I literal_string.py <v> <a> [<v> <a> ...]     (hex, as printed; env WORK)
Method (verified on BrokenPieceFader): V = word at RVA(v) [RVA = ghidra address - 0x10000];
pointer = word at RVA(V + a); string address = pointer - 0x10000 -> script.json ScriptString.
Needs read_floats.sh (Ghidra headless), so Ghidra must not have the project open.
"""
import json, os, re, subprocess, sys
here = os.path.dirname(os.path.abspath(__file__))
work = os.environ.get('WORK', os.path.expanduser('~/Desktop/royal-smash-work'))
args = sys.argv[1:]
pairs = [(int(args[i], 16), int(args[i + 1], 16)) for i in range(0, len(args), 2)]
def words(rvas):
    out = subprocess.run([os.path.join(here, 'read_floats.sh')] + [hex(r) for r in rvas], capture_output=True, text=True).stdout
    return [int(re.search(r'\(0x([0-9a-f]+)\)', l).group(1), 16) for l in out.strip().split('\n')]
vs = words([v - 0x10000 for v, _ in pairs])
ps = words([vv + a - 0x10000 for vv, (_, a) in zip(vs, pairs)])
d = json.load(open(work + '/il2cpp/script.json'))
names = {m['Address']: json.dumps(m['Value'], ensure_ascii=False) for m in d['ScriptString']}
for k in ('ScriptMetadata', 'ScriptMetadataMethod'):
    for m in d[k]:
        names[m['Address']] = m['Name']
for (v, a), p in zip(pairs, ps):
    print('_UNK_%08x + 0x%x' % (v, a), names.get(p - 0x10000, '?%x' % (p - 0x10000)))
