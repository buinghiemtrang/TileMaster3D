#!/usr/bin/env python3
"""Usage: used_scripts_scan.py   (env REPO = Unity project root; was PixelFlow tools/stub_scan.py, renamed to avoid clash with Tools/reverse/stub_scan.py)
 [--depth N] <asset.prefab|.unity|.asset> ...
Recursively follows nested prefab / ScriptableObject guid references from the given assets (default depth 6) and
lists every C# script (MonoBehaviour/SO) used, with its stub-body count. Output sorted by stubs, then a per-assembly sum.
Stub = empty body `) { }` or `{ return default/null/false/0; }`."""
import os, re, sys, collections
PROJ = os.environ['REPO']
args = sys.argv[1:]
depth = 6
if args[:1] == ['--depth']:
    depth = int(args[1]); args = args[2:]
os.chdir(PROJ)
g2p = {}
for r, d, f in os.walk('Assets'):
    for n in f:
        if n.endswith('.meta'):
            try:
                m = re.search(r'^guid: (\w+)', open(os.path.join(r, n), errors='ignore').read(), re.M)
            except OSError:
                continue
            if m:
                g2p[m.group(1)] = os.path.join(r, n[:-5])


def stubs(p):
    s = open(p, errors='ignore').read()
    return len(re.findall(r'\)\s*\{\s*\}', s)) + len(re.findall(r'\{\s*return (default|null|false|0);\s*\}', s))


seen, scripts = set(), {}
frontier = [(a, 0) for a in args]
while frontier:
    a, dd = frontier.pop()
    if a in seen or not os.path.isfile(a):
        continue
    seen.add(a)
    try:
        t = open(a, errors='ignore').read()
    except OSError:
        continue
    for g in set(re.findall(r'guid: (\w{32})', t)):
        p = g2p.get(g)
        if not p:
            continue
        if p.endswith('.cs'):
            scripts[p] = scripts.get(p) or stubs(p)
        elif dd < depth and p.endswith(('.prefab', '.asset')):
            frontier.append((p, dd + 1))
rows = sorted(((e, p) for p, e in scripts.items() if e), reverse=True)
asm = collections.Counter()
for e, p in rows:
    asm[p.split('/')[2]] += e
print(f'{len(seen)} assets, {len(scripts)} scripts, {len(rows)} with stubs, {sum(e for e, _ in rows)} stub bodies')
for e, p in rows:
    print(e, p.replace('Assets/Scripts/', ''))
print('-- per assembly')
for a, e in asm.most_common():
    print(e, a)
