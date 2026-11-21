"""Turn Il2CppDumper's script.json into plain TSV files for the Ghidra scripts.

usage: python -I make_ghidra_inputs.py <script.json> <out_dir> [Class ...]

names.tsv    : address<TAB>name for every method (applied to the program as labels/functions)
targets.tsv  : address<TAB>name of methods whose type is one of the given classes
               (name is `Class$$method`; nested types look like `Class.Nested$$method`)
"""
import json, os, sys, re

src, out = sys.argv[1], sys.argv[2]
classes = sys.argv[3:]
os.makedirs(out, exist_ok=True)
d = json.load(open(src, encoding='utf8'))
methods = d['ScriptMethod']
with open(os.path.join(out, 'names.tsv'), 'w', encoding='utf8') as f:
    for m in methods:
        f.write('%d\t%s\n' % (m['Address'], m['Name'].replace(' ', '-').replace('\t', '-')))
pat = re.compile(r'^(?:[\w.]+\.)?(?:%s)(?:[.<][^$]*)?\$\$' % '|'.join(re.escape(c) for c in classes)) if classes else None
n = 0
with open(os.path.join(out, 'targets.tsv'), 'w', encoding='utf8') as f:
    for m in methods:
        if pat and pat.match(m['Name']):
            f.write('%d\t%s\n' % (m['Address'], m['Name'])); n += 1
print(len(methods), 'names;', n, 'targets')
