"""Re-decompile functions that hit the per-function timeout, and merge the result back into the original file.

usage: python -I retry_failed.py prepare <pseudoc dir> <out.tsv>
           -> lists every `// decompile failed` function as targets (one pseudo-class per original file)
       run:  DecompileBulk.java <out.tsv> <tmp dir> 600     (longer timeout; any number of workers)
       python -I retry_failed.py merge <pseudoc dir> <tmp dir>
           -> replaces each failed block in the original raw .c with the new body (or keeps the failure note),
              and touches the file so `bulk_postprocess.py --only-new` redoes it.
The pseudo-class name is `retry@<original file name>` so DecompileBulk writes one tmp file per original file.
Does not depend on the game.
"""
import glob, os, re, sys

mode, pd = sys.argv[1], sys.argv[2]
blk = re.compile(r'^// (.+?)  RVA: 0x([0-9a-f]+)\n// decompile failed[^\n]*\n\n?', re.M)

if mode == 'prepare':
    n = 0
    with open(sys.argv[3], 'w', encoding='utf8') as o:
        for f in sorted(glob.glob(os.path.join(pd, '*.c'))):
            if f.endswith(('.clean.c', '.ann.c', '.named.c')):
                continue
            for m in blk.finditer(open(f, encoding='utf8').read()):
                o.write('%d\tretry@%s$$%s\n' % (int(m.group(2), 16), os.path.basename(f)[:-2], m.group(1).replace('$$', '::')))
                n += 1
    print(n, 'failed functions listed')
elif mode == 'merge':
    td = sys.argv[3]
    got = {}
    for f in glob.glob(os.path.join(td, '*.c')):
        t = open(f, encoding='utf8').read()
        parts = re.split(r'(?m)^// (.+?)  RVA: 0x([0-9a-f]+)\n', t)
        for i in range(1, len(parts) - 2, 3):
            body = parts[i + 2]
            if not body.startswith('// decompile failed'):
                got[int(parts[i + 1], 16)] = body
    fixed = 0
    for f in glob.glob(os.path.join(pd, '*.c')):
        if f.endswith(('.clean.c', '.ann.c', '.named.c')):
            continue
        t = open(f, encoding='utf8').read()
        if '// decompile failed' not in t:
            continue

        def rep(m):
            global fixed
            a = int(m.group(2), 16)
            if a in got:
                fixed += 1
                return '// %s  RVA: 0x%x\n%s' % (m.group(1), a, got[a])
            return m.group(0)
        u = blk.sub(rep, t)
        if u != t:
            open(f, 'w', encoding='utf8', newline='\n').write(u)
    print(fixed, 'functions merged;', len(got), 'decompiled in retry')
