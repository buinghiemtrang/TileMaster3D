"""Name runtime targets that are really managed code: IL2CPP `_inline` copies of small methods.

usage: python -I name_inline_copies.py <WORK> [--apply]
IL2CPP emits a private copy (`<Method>_inline`) of methods marked AggressiveInlining when another assembly calls them.
The copy is not registered in metadata, so script.json has no name for it and bulk_postprocess.py prints it as
`rt_<rva>` (e.g. rt_1c6b278 = Vector3.Normalize: metadata init, SQRT(x*x+y*y+z*z), compare with epsilon).
Its body is the same as the registered method's, so this script hashes every decompiled managed function in
<WORK>/ghidra/pseudoc/*.c (raw files), with addresses masked, and gives each `rt_<rva>` whose body in
rt-out/rt.c matches exactly one managed name the name `<Ns.Class.Method>_inline`.
Writes the new names into rt_names.json (kept: every earlier name) and, with --apply, rewrites `rt_<rva>` in all
pseudoc/*.named.c. Bodies shorter than 120 characters after masking are skipped (too many accidental twins).
Does not depend on the game.
"""
import glob, hashlib, json, os, re, sys

WORK = sys.argv[1]
G = os.path.join(WORK, 'ghidra')
PD = os.path.join(G, 'pseudoc')
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import normalize_unk

MASK = re.compile(r'\b(?:FUN|thunk_FUN|SUB|LAB|UNK|DAT|PTR|_UNK|code_r0x)_?[0-9a-f]{6,8}\b|\bfunc_0x[0-9a-f]{8}\b|'
                  r'\b0x1[0-9a-f]{6}\b|\b0x[0-9a-f]{7,8}\b')
HEAD = re.compile(r'(?m)^// (.+?)  RVA: 0x([0-9a-f]+)\n')


def norm(b):
    b = normalize_unk.fix(b)
    b = re.sub(r'/\*.*?\*/', '', b, flags=re.S).strip()
    b = b.split('\n', 1)[1] if '\n' in b else ''     # drop the signature (its name differs)
    b = MASK.sub('X', b)
    return re.sub(r'\s+', ' ', b).strip()


def h(s):
    return hashlib.blake2b(s.encode('utf8', 'replace'), digest_size=12).digest()


# 1. managed corpus
corpus = {}
files = [f for f in glob.glob(os.path.join(PD, '*.c')) if not f.endswith(('.clean.c', '.ann.c', '.named.c'))]
for i, f in enumerate(files):
    parts = HEAD.split(open(f, encoding='utf8', errors='replace').read())
    for k in range(1, len(parts) - 2, 3):
        body = parts[k + 2]
        if body.startswith('// decompile failed') or body.startswith('// no function'):
            continue
        n = norm(body)
        if len(n) < 120:
            continue
        key = h(n)
        name = parts[k].replace('$$', '.')
        old = corpus.get(key)
        if old is None:
            corpus[key] = name
        elif old != name:
            corpus[key] = ''        # ambiguous: two different managed methods share the body
    if i % 5000 == 4999:
        print('  corpus', i + 1, 'files', len(corpus), 'bodies', flush=True)
print('corpus:', len(corpus), 'distinct bodies from', len(files), 'files', flush=True)

# 2. runtime targets
rt_path = os.path.join(G, 'rt_names.json')
names = json.load(open(rt_path))
body = {}
for p in re.split(r'(?m)^// rt\$\$r', open(os.path.join(G, 'rt-out', 'rt.c'), encoding='utf8').read())[1:]:
    head, _, b = p.partition('\n')
    body[head.split()[0]] = b
new = {}
amb = 0
for rva, b in body.items():
    if not names.get(rva, 'rt_').startswith('rt_'):
        continue
    n = norm(b)
    if len(n) < 120:
        continue
    m = corpus.get(h(n))
    if m:
        new[rva] = m + '_inline'
    elif m == '':
        amb += 1
# aliases (thunk_FUN_ of a renamed body) carry the target's rt_ token: follow them
for rva, v in list(names.items()):
    if v.startswith('rt_') and v[3:] in new:
        new[rva] = new[v[3:]]
print('named', len(new), 'runtime targets as _inline copies;', amb, 'ambiguous (left as rt_)', flush=True)
old_tokens = {}
for rva, v in new.items():
    old_tokens[names.get(rva, 'rt_' + rva)] = v
    names[rva] = v
json.dump(names, open(rt_path, 'w'), indent=0)

# 3. rewrite the named pseudo-C
if '--apply' in sys.argv and old_tokens:
    tok = re.compile(r'\brt_([0-9a-f]+)\b')
    changed = 0
    for f in glob.glob(os.path.join(PD, '*.named.c')):
        t = open(f, encoding='utf8').read()
        u = tok.sub(lambda m: old_tokens.get(m.group(0), m.group(0)), t)
        if u != t:
            open(f, 'w', encoding='utf8', newline='\n').write(u)
            changed += 1
    print('rewrote', changed, 'named files', flush=True)
