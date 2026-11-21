"""Remap script GUIDs in Unity YAML assets from removed stub scripts to the real package scripts.

usage: python -I remap_guids.py <project> <stub_map.tsv> <removed_stubs_dir> [--apply]

stub_map.tsv: guid<TAB>fullname<TAB>path  (from scan_scripts.py, taken before the stubs were removed)
Only stubs whose assembly folder now lives in <removed_stubs_dir> are remapped.
Real scripts are scanned in <project>/Library/PackageCache.
"""
import os, re, sys, collections

proj, stub_tsv, removed = sys.argv[1], sys.argv[2], sys.argv[3]
apply = '--apply' in sys.argv
removed_asms = set(os.listdir(removed))

stubs = {}  # guid -> fullname
for line in open(stub_tsv, encoding='utf8'):
    g, full, p = line.rstrip('\n').split('\t')
    parts = re.split(r'[\\/]', p)
    asm = parts[parts.index('Scripts') + 1] if 'Scripts' in parts else None
    if asm in removed_asms:
        stubs[g] = full

ns_re = re.compile(r'^\s*namespace\s+([\w.]+)', re.M)
skip = re.compile(r'[\\/](Tests?|Samples~|Documentation~|Editor)[\\/]', re.I)
real = collections.defaultdict(list)  # fullname -> [(guid, path)]
pc = os.environ.get('PKGCACHE') or os.path.join(proj, 'Library', 'PackageCache')
for d, _, files in os.walk(pc):
    for f in files:
        if not f.endswith('.cs'): continue
        p = os.path.join(d, f)
        if skip.search(p): continue
        meta = p + '.meta'
        if not os.path.exists(meta): continue
        g = re.search(r'^guid:\s*([0-9a-f]{32})', open(meta, encoding='utf8', errors='ignore').read(), re.M)
        if not g: continue
        m = ns_re.search(open(p, encoding='utf8', errors='ignore').read())
        full = (m.group(1) + '.' if m else '') + os.path.splitext(f)[0]
        real[full].append((g.group(1), p))

# which stub guids are actually referenced?
yaml_ext = {'.prefab', '.unity', '.asset', '.mat', '.controller', '.overrideController', '.anim',
            '.playable', '.mask', '.signal', '.preset', '.lighting', '.spriteatlas', '.spriteatlasv2',
            '.physicMaterial', '.guiskin', '.fontsettings', '.mixer', '.renderTexture', '.flare'}
guid_re = re.compile(r'guid: ([0-9a-f]{32})')
files = []
for d, _, fs in os.walk(os.path.join(proj, 'Assets')):
    for f in fs:
        if os.path.splitext(f)[1] in yaml_ext:
            files.append(os.path.join(d, f))

used = collections.Counter()
for p in files:
    try: txt = open(p, encoding='utf8', errors='ignore').read()
    except OSError: continue
    for g in guid_re.findall(txt):
        if g in stubs: used[g] += 1

mapping, missing, ambiguous = {}, [], []
for g, n in used.items():
    full = stubs[g]
    cands = real.get(full, [])
    if len(cands) == 1:
        mapping[g] = cands[0][0]
    elif len(cands) > 1:
        rt = [c for c in cands if re.search(r'[\\/]Runtime[\\/]', c[1])]
        if len(rt) == 1: mapping[g] = rt[0][0]
        else: ambiguous.append((full, n, [c[1] for c in cands]))
    else:
        missing.append((full, n))

print(f'stub guids referenced: {len(used)} ({sum(used.values())} refs); mapped: {len(mapping)}; '
      f'missing: {len(missing)}; ambiguous: {len(ambiguous)}')
for full, n in sorted(missing, key=lambda x: -x[1]): print('  MISSING', n, full)
for full, n, c in ambiguous: print('  AMBIGUOUS', n, full, c)

if apply and mapping:
    changed = 0
    rx = re.compile(r'guid: (' + '|'.join(mapping) + r')')
    for p in files:
        raw = open(p, 'rb').read()
        txt = raw.decode('utf8', errors='surrogateescape')
        new = rx.sub(lambda m: 'guid: ' + mapping[m.group(1)], txt)
        if new != txt:
            open(p, 'wb').write(new.encode('utf8', errors='surrogateescape'))
            changed += 1
    print('files changed:', changed)
