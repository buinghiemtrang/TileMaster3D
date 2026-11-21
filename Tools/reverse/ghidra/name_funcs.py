"""Replace every remaining `func_0x........` in *.ann.c with the managed method name.

usage: python -I name_funcs.py <ghidra dir> [Class...]
Needs (all in <ghidra dir>): func_names.json built by build_func_names.py (veneer address -> name).
Names that stay `native_<rva>` are unnamed native/unknown targets (RVA is kept so they can be looked up).
Output: <Class>.named.c next to the .ann.c (the .ann.c is untouched).
"""
import glob, json, os, re, sys
d = sys.argv[1]
names = json.load(open(os.path.join(d, 'func_names.json')))
want = set(sys.argv[2:])
pat = re.compile(r'func_0x([0-9a-f]{8})')
for f in glob.glob(os.path.join(d, 'pseudoc', '*.ann.c')):
    base = os.path.basename(f)[:-6]
    if '.named' in base:
        continue
    if want and not any(base == w or base.startswith(w + '.') or base.startswith(w + '<') for w in want):
        continue
    txt = open(f).read()
    out = pat.sub(lambda m: names.get(m.group(1), m.group(0)).replace('$$', '.'), txt)
    open(os.path.join(d, 'pseudoc', base + '.named.c'), 'w').write(out)
