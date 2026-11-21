"""Build func_names.json: every func_0x........ seen in pseudoc/*.c -> managed method name.

usage: python -I build_func_names.py <ghidra dir> <script.json> [--merge]   (--merge keeps names already in func_names.json)
Steps (pha 6): grep addresses to allfuncs.txt, run ResolveVeneers.java allfuncs.txt allfuncs.tsv, then this script.
Covers generic instances too (script.json ScriptMethod), unlike build_veneer_names.py (dump.cs only).
"""
import json, os, sys
d, sj = sys.argv[1:3]
by = {}
for m in json.load(open(sj))['ScriptMethod']:
    by.setdefault(m['Address'], m['Name'])
out = os.path.join(d, 'func_names.json')
res = json.load(open(out)) if '--merge' in sys.argv and os.path.exists(out) else {}
for l in open(os.path.join(d, 'allfuncs.tsv')):
    a, t = l.rstrip('\n').split('\t')
    if t == '?': continue
    rva = int(t, 16) & 0xffffffff
    res[a] = by.get(rva, 'native_%x' % rva)
json.dump(res, open(out, 'w'), indent=0)
print(len(res), 'resolved,', sum(not v.startswith('native_') for v in res.values()), 'with managed name')
