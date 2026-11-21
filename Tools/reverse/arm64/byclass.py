"""Group per-function pseudo-C (ApplySigsAndDecompile output, <name>@<addr>.c) by top-level class.
argv: targets.tsv (addr<TAB>Ns.Outer/Inner$$Method), decomp dir, out dir, class_index.tsv
Writes <out>/<Ns.Outer>.c with a header per function and class_index.tsv: class<TAB>functions<TAB>lines.
"""
import os, sys, collections
tsv, dec, out, idx = sys.argv[1:5]
os.makedirs(out, exist_ok=True)
by_addr = {}
for f in os.listdir(dec):
    if '@' in f and f.endswith('.c'):
        by_addr[int(f.rsplit('@', 1)[1][:-2], 16)] = f
groups = collections.defaultdict(list)
for line in open(tsv):
    a, name = line.rstrip('\n').split('\t')[:2]
    cls = name.split('$$')[0].split('/')[0]
    groups[cls].append((int(a, 16), name))
with open(idx, 'w') as w:
    for cls, items in sorted(groups.items()):
        lines = 0
        with open(os.path.join(out, cls + '.c'), 'w') as o:
            for a, name in items:
                f = by_addr.get(a)
                body = open(os.path.join(dec, f)).read() if f else '// (no pseudo-C)\n'
                o.write(f'\n// ===== {name}  RVA 0x{a - 0x100000:x}  Ghidra 0x{a:x}\n{body}')
                lines += body.count('\n')
        w.write(f'{cls}\t{len(items)}\t{lines}\n')
print(len(groups), 'classes')
