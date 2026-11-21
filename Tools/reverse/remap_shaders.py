"""Point materials at real shaders instead of AssetRipper's dummy shaders, then drop the dummies.

usage: python -I remap_shaders.py <project> <shader_root>... [--apply]

A dummy is any .shader under <project>/Assets containing "//DummyShaderTextExporter".
Real shaders are the other .shader files under <project>/Assets plus every <shader_root>
(e.g. the editor's BuiltInPackages/com.unity.render-pipelines.universal), matched by the
name in `Shader "..."`. Default is a dry run; --apply rewrites the YAML references and
deletes the dummy .shader/.meta pairs that got a replacement.
"""
import os, re, sys, collections

args = [a for a in sys.argv[1:] if not a.startswith('--')]
proj, roots = args[0], args[1:]
apply = '--apply' in sys.argv

name_re = re.compile(r'^\s*Shader\s+"([^"]+)"', re.M)
guid_re = re.compile(r'^guid:\s*([0-9a-f]{32})', re.M)
skip = re.compile(r'[\\/](Tests?|Samples~|Documentation~|ShaderLibrary~?)[\\/]', re.I)


def shaders(root):
    for d, _, fs in os.walk(root):
        for f in fs:
            if not f.endswith('.shader'): continue
            p = os.path.join(d, f)
            if skip.search(p) or not os.path.exists(p + '.meta'): continue
            txt = open(p, encoding='utf8', errors='ignore').read()
            m = name_re.search(txt)
            g = guid_re.search(open(p + '.meta', encoding='utf8', errors='ignore').read())
            if m and g:
                yield p, m.group(1), g.group(1), '//DummyShaderTextExporter' in txt


assets = os.path.join(proj, 'Assets')
dummies = {}                           # guid -> (name, path)
real = collections.defaultdict(set)    # name -> {guid}
real_path = {}
for root in [assets] + roots:
    for p, name, g, dummy in shaders(root):
        if dummy and root == assets:
            dummies[g] = (name, p)
        elif not dummy:
            real[name].add(g); real_path[g] = p

yaml_ext = {'.prefab', '.unity', '.asset', '.mat', '.controller', '.anim', '.playable', '.preset',
            '.spriteatlas', '.spriteatlasv2', '.shadervariants', '.renderTexture', '.lighting'}
files = [os.path.join(d, f) for d, _, fs in os.walk(assets) for f in fs if os.path.splitext(f)[1] in yaml_ext]

used = collections.Counter()
ref_re = re.compile(r'guid: ([0-9a-f]{32})')
for p in files:
    for g in ref_re.findall(open(p, encoding='utf8', errors='ignore').read()):
        if g in dummies: used[g] += 1

mapping, unmapped = {}, []
for g, (name, p) in sorted(dummies.items(), key=lambda x: x[1][0]):
    cands = real.get(name, set())
    if len(cands) == 1:
        mapping[g] = next(iter(cands))
        print(f'  MAP  {used[g]:4} refs  {name}  ->  {os.path.relpath(real_path[mapping[g]], proj) if real_path[mapping[g]].startswith(proj) else real_path[mapping[g]]}')
    else:
        unmapped.append((name, used[g], len(cands)))
for name, n, c in unmapped:
    print(f'  KEEP {n:4} refs  {name}' + (f'  (ambiguous: {c} real shaders)' if c > 1 else ''))
print(f'dummies: {len(dummies)}; mapped: {len(mapping)} ({sum(used[g] for g in mapping)} refs); kept: {len(unmapped)}')

if apply and mapping:
    rx = re.compile(r'guid: (' + '|'.join(mapping) + r')')
    changed = 0
    for p in files:
        raw = open(p, 'rb').read()
        txt = raw.decode('utf8', errors='surrogateescape')
        new = rx.sub(lambda m: 'guid: ' + mapping[m.group(1)], txt)
        if new != txt:
            open(p, 'wb').write(new.encode('utf8', errors='surrogateescape'))
            changed += 1
    for g in mapping:
        p = dummies[g][1]
        os.remove(p); os.remove(p + '.meta')
    print('files changed:', changed, '; dummy shaders removed:', len(mapping))
