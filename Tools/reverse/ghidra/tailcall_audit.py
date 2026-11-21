"""Classify tail calls (TailCalls.java output) whose target name is absent from the caller's .named.c body.

For each one, compare the calls made by the target's pseudo-C with the caller's: >= 60% shared means the decompiler copied the
target body into the caller (INLINED, nothing lost); otherwise DROPPED? (the call block may be missing from the pseudo-C; read the
assembly before the branch with ListInstr.java + annotate_listing.py).

usage: python3 -I tailcall_audit.py <tailcalls.tsv> <pseudoc dir> <script.json> [name-prefix ...]
Not game specific.
"""
import collections, glob, json, os, re, sys

tsv, pseudoc, script = sys.argv[1:4]
prefixes = tuple(sys.argv[4:]) or ('',)
d = json.load(open(script))
by_addr = {m['Address']: m['Name'] for m in d['ScriptMethod']}
bodies = {}
hdr = re.compile(r'^// (.*?\$\$.*?)  RVA: 0x([0-9a-f]+)', re.M)
for f in glob.glob(os.path.join(pseudoc, '*.named.c')):
    t = open(f, encoding='utf-8', errors='replace').read()
    ms = list(hdr.finditer(t))
    for i, m in enumerate(ms):
        bodies[m.group(1)] = t[m.start():ms[i + 1].start() if i + 1 < len(ms) else len(t)]
call_re = re.compile(r'([A-Za-z_][\w.<>, ]*?(?:__|\.)[A-Za-z_]\w*)\s*\(')


def calls(body):
    out = set()
    for c in call_re.findall(body):
        c = c.strip()
        if c.startswith(('il2cpp', 'rt_', 'FUN_')) or 'TypeInfo' in c:
            continue
        out.add(re.split(r'__|\.', c)[-1])
    return out


stats = collections.Counter()
for line in open(tsv):
    label, start, branch, target = line.rstrip('\n').split('\t')[:4]
    if not label.startswith(prefixes):
        continue
    callee = by_addr.get(int(target, 16) - 0x10000)
    caller_body = bodies.get(label)
    if callee is None or caller_body is None:
        continue
    short = callee.split('$$')[-1]
    if short in caller_body or re.sub(r'[<>`.]', '_', short) in caller_body:
        continue
    target_calls = calls(bodies.get(callee, ''))
    if not target_calls:
        verdict = 'TRIVIAL'
    else:
        ratio = len(target_calls & calls(caller_body)) / len(target_calls)
        verdict = 'INLINED' if ratio >= 0.6 else 'DROPPED?'
    stats[verdict] += 1
    print('%s\t%s\t%s\t%s' % (verdict, label, callee, branch))
print(dict(stats), file=sys.stderr)
