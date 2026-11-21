"""Build targets.tsv for DecompileBulk.java from script.json by namespace instead of by class name.

usage: python -I make_bulk_targets.py <script.json> <out_targets.tsv> --include P1,P2 [--exclude Q1,Q2] [--limit N]
  P/Q are prefixes of the class name part of `Ns.Class$$method` (e.g. `Core.`, `Foundation.Analytics`). The special
  prefix `@global` matches classes without a namespace. Rows are grouped by class (required by DecompileBulk).
  --limit N keeps only the first N methods (pilot runs).
Does not depend on the game.
"""
import json, sys, collections

src, out = sys.argv[1], sys.argv[2]
opt = {'--include': '', '--exclude': '', '--limit': '0'}
a = sys.argv[3:]
for k in range(0, len(a) - 1, 2):
    opt[a[k]] = a[k + 1]
inc = [p for p in opt['--include'].split(',') if p]
exc = [p for p in opt['--exclude'].split(',') if p]
limit = int(opt['--limit'])


def match(cls, pats):
    for p in pats:
        if p == '@global':
            if '.' not in cls.split('<')[0]:
                return True
        elif cls.startswith(p):
            return True
    return False


groups = collections.OrderedDict()
for m in json.load(open(src, encoding='utf8'))['ScriptMethod']:
    name = m['Name']
    if '$$' not in name:
        continue
    cls = name.split('$$')[0]
    if match(cls, inc) and not match(cls, exc):
        groups.setdefault(cls, []).append((m['Address'], name))
n = 0
with open(out, 'w', encoding='utf8') as f:
    for cls, ms in groups.items():
        for addr, name in ms:
            if limit and n >= limit:
                break
            f.write('%d\t%s\n' % (addr, name)); n += 1
print(n, 'methods in', len(groups), 'classes')
