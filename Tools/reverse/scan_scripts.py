"""Scan .cs files under a root and emit `guid<TAB>fullname<TAB>path` for each script with a .meta.
fullname = namespace + '.' + file stem (Unity binds MonoScript by file name == class name)."""
import os, re, sys
root, out = sys.argv[1], sys.argv[2]
ns_re = re.compile(r'^\s*namespace\s+([\w.]+)', re.M)
rows = []
for d, _, files in os.walk(root):
    for f in files:
        if not f.endswith('.cs'): continue
        p = os.path.join(d, f)
        meta = p + '.meta'
        if not os.path.exists(meta): continue
        g = re.search(r'^guid:\s*([0-9a-f]{32})', open(meta, encoding='utf8', errors='ignore').read(), re.M)
        if not g: continue
        src = open(p, encoding='utf8', errors='ignore').read()
        m = ns_re.search(src)
        name = os.path.splitext(f)[0]
        full = (m.group(1) + '.' if m else '') + name
        rows.append((g.group(1), full, p))
with open(out, 'w', encoding='utf8') as fo:
    for r in rows: fo.write('\t'.join(r) + '\n')
print(len(rows), 'scripts')
