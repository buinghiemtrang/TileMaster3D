"""Add missing asmdef references reported by the Unity compiler (CS0234 / CS0246 / CS0012).

usage: python -I fix_asmdef_refs.py <Assets/Scripts> <unity log> [--apply]
For each `<file>.cs(l,c): error CS0234/CS0246: ... namespace/type 'X' ...` it finds the asmdef that owns the file,
looks for the asmdef whose sources declare namespace X (or a type X), and lists/adds that reference.
Dry run by default. Game-independent. Dependency cycles are skipped and reported.
"""
import glob, json, os, re, sys
root, log = sys.argv[1], sys.argv[2]
apply = '--apply' in sys.argv
asmdefs = {}
for p in glob.glob(os.path.join(root, '*', '*.asmdef')):
    d = json.loads(open(p, encoding='utf-8-sig').read())
    asmdefs[d['name']] = (p, d)
owner_of_dir = {os.path.dirname(p): n for n, (p, d) in asmdefs.items()}
ns_index, type_index = {}, {}
for name, (p, d) in asmdefs.items():
    for f in glob.glob(os.path.join(os.path.dirname(p), '**', '*.cs'), recursive=True):
        t = open(f, encoding='utf-8', errors='ignore').read()
        for m in re.finditer(r'(?m)^\s*namespace\s+([\w.]+)', t):
            ns_index.setdefault(m.group(1), set()).add(name)
        for m in re.finditer(r'(?m)^\s*(?:public|internal)\s+(?:static\s+|sealed\s+|abstract\s+|partial\s+)*(?:class|struct|interface|enum)\s+(\w+)', t):
            type_index.setdefault(m.group(1), set()).add(name)

def owner(path):
    parts = path.replace('\\', '/').split('/')
    if 'Scripts' in parts:
        i = parts.index('Scripts')
        d = os.path.join(root, parts[i + 1])
        return owner_of_dir.get(d)

def reaches(a, b, seen=None):
    seen = seen or set()
    if a == b:
        return True
    if a in seen or a not in asmdefs:
        return False
    seen.add(a)
    return any(reaches(r, b, seen) for r in asmdefs[a][1].get('references', []))

wanted = {}
for line in open(log, encoding='utf-8', errors='ignore'):
    m = re.match(r'(Assets/Scripts/\S+?\.cs)\(\d+,\d+\): error CS(0234|0246|0012): (.*)', line)
    if not m:
        continue
    o = owner(m.group(1))
    if not o:
        continue
    q = re.search(r"'([\w.]+)'", m.group(3))
    if not q:
        continue
    x = q.group(1)
    cands = set()
    if m.group(2) == '0234':
        full = re.search(r"namespace '(\w+)' does not exist in the namespace '([\w.]+)'", m.group(3))
        if full:
            x = full.group(2) + '.' + full.group(1)
    if x in ns_index:
        cands = ns_index[x]
    elif x.split('.')[-1] in type_index:
        cands = type_index[x.split('.')[-1]]
    cands = {c for c in cands if c != o}
    if len(cands) == 1:
        wanted.setdefault(o, set()).add(next(iter(cands)))
    elif cands:
        print('ambiguous', o, x, sorted(cands))
for o, refs in sorted(wanted.items()):
    p, d = asmdefs[o]
    new = [r for r in sorted(refs) if r not in d.get('references', [])]
    ok = [r for r in new if not reaches(r, o)]
    for r in new:
        if r not in ok:
            print('cycle, skipped:', o, '->', r)
    if ok:
        print(o, '+', ok)
        if apply:
            d.setdefault('references', []).extend(ok)
            open(p, 'w', encoding='utf-8').write(json.dumps(d, indent=4) + '\n')
