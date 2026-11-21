"""Match the shipped Addressables catalog (dumped by Royal Smash > Addressables > Dump Shipped Catalog)
against the AssetRipper export and write the entries to recreate as an Addressables group.

usage: python -I addressables_entries.py <aa-catalog.txt> <Assets/AssetBundles dir> <out.json>
Each entry: {"path": "Assets/AssetBundles/<hash>/<rel>", "address": <primary key>, "labels": [other keys]}.
Only entries whose InternalId (an `Assets/...` path inside the bundle) exists under some <hash>/ folder are kept;
the rest live on the CDN or are sub-assets. Applied by Assets/Editor/AddressablesEntryImporter.cs.
Game-independent.
"""
import collections, json, os, re, sys

catalog, root, out = sys.argv[1:4]
disk = {}
stems = collections.defaultdict(list)  # path without extension -> files (AssetRipper re-encodes e.g. .wav -> .ogg)
dups = 0
for h in sorted(os.listdir(root)):
    p = os.path.join(root, h)
    if not os.path.isdir(p):
        continue
    for dp, _, fn in os.walk(p):
        for f in fn:
            if f.endswith('.meta'):
                continue
            rel = os.path.relpath(os.path.join(dp, f), p).replace(os.sep, '/')
            if rel in disk:
                dups += 1
            else:
                disk[rel] = 'Assets/AssetBundles/%s/%s' % (h, rel)
            stems[os.path.splitext(rel)[0]].append(disk.get(rel) or 'Assets/AssetBundles/%s/%s' % (h, rel))

entries = collections.OrderedDict()   # primary key -> dict
aliases = collections.defaultdict(list)  # primary key -> alias keys
cur = None
for line in open(catalog, encoding='utf8'):
    line = line.rstrip('\n')
    if not line.startswith('\t'):
        if ' -> ' in line:
            k, p = line.split(' -> ', 1)
            aliases[p].append(k)
        else:
            cur = line
            entries.setdefault(cur, {})
    elif line.startswith('\tInternalId: '):
        entries[cur]['internal'] = line[len('\tInternalId: '):]

res = {}
missing = 0
ext_fallback = 0
for key, e in entries.items():
    iid = e.get('internal', '')
    if not iid.startswith('Assets/'):
        continue
    path = disk.get(iid[len('Assets/'):])
    if not path:
        alt = stems.get(os.path.splitext(iid[len('Assets/'):])[0], [])
        if len(alt) == 1:
            path = alt[0]
            ext_fallback += 1
    if not path:
        missing += 1
        continue
    r = res.setdefault(path, {'path': path, 'address': key, 'labels': []})
    for k in aliases.get(key, []):
        if k not in r['labels']:
            r['labels'].append(k)
json.dump(list(res.values()), open(out, 'w', encoding='utf8'), indent=1)
print('%d entries written (%d matched by name with a different extension), %d catalog assets without a file on disk, %d duplicate relative paths' % (len(res), ext_fallback, missing, dups))
