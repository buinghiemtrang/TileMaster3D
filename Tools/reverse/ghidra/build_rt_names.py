"""Build rt_names.json: names for the non-managed functions that pseudo-C calls (IL2CPP runtime helpers, libc imports).

usage: python -I build_rt_names.py <WORK> [rt_overrides.json]
Needs in <WORK>/ghidra: elf_names.json (elf_names.py), helper_names.tsv (pha 6 step 2), rt-out/rt.c (decompile of every
called address, see bulk_postprocess.py --rt) and rt_counts.json. Writes rt_names.json {rva hex: name}.
Order of evidence (first hit wins, nothing is guessed):
  1. overrides file (names a human derived and documented, e.g. from an exported il2cpp_* wrapper)
  2. helper_names.tsv (the 8 helpers named by DefineHelpers.java)
  3. ELF dynsym exports and PLT imports (elf_names.json)
  4. a body that is only a jump/call to a named PLT slot or named function: inherits that name (a thunk)
  5. Ghidra names it `thunk_FUN_<addr>` (a veneer onto another function): inherits the target's name
  6. its body is identical (after masking addresses) to an exported il2cpp_* function in rt-out/../api-out/api.c:
     it IS that function (exact, e.g. il2cpp_value_box)
Everything else stays `native_<rva>` / `func_0x<addr>`. Prints coverage by number of call sites.
Does not depend on the game (ARMv7 Ghidra image base 0x10000 is a constant here).
"""
import json, os, re, sys

WORK = sys.argv[1]
G = os.path.join(WORK, 'ghidra')
BASE = 0x10000
elf = json.load(open(os.path.join(G, 'elf_names.json')))
cnt = {int(k): v for k, v in json.load(open(os.path.join(G, 'rt_counts.json'))).items()}
names = {}
if len(sys.argv) > 2 and os.path.exists(sys.argv[2]):
    for k, v in json.load(open(sys.argv[2])).items():
        if not k.startswith('_'):
            names[k.lower().lstrip('0x')] = v
for l in open(os.path.join(G, 'helper_names.tsv'), encoding='utf8'):
    a, n = l.rstrip('\n').split('\t')
    names.setdefault('%x' % (int(a, 16) - BASE), n)
for k, v in elf.items():
    names.setdefault(k, v)

# thunks: function bodies that only forward to something we can name
body = {}
for p in re.split(r'(?m)^// rt\$\$r', open(os.path.join(G, 'rt-out', 'rt.c'), encoding='utf8').read())[1:]:
    h, _, b = p.partition('\n')
    body[int(h.split()[0], 16)] = b
call_re = re.compile(r'\(\*\(code \*\)&?(?:LAB|UNK|SUB|DAT)_([0-9a-f]{8})\)|\bfunc_0x([0-9a-f]{8})\b|\bFUN_([0-9a-f]{8})\b')
for _ in range(3):  # a thunk can point at a thunk
    for rva, b in body.items():
        if '%x' % rva in names:
            continue
        stmts = [s.strip() for s in b.split('\n') if s.strip() and not s.strip().startswith(('/*', '//', '*', 'void ', 'int ', 'undefined', 'uint ', 'char ', '{', '}', 'return;'))]
        if len(stmts) != 1:
            continue
        m = call_re.search(stmts[0])
        if m:
            tgt = int(next(g for g in m.groups() if g), 16) - BASE
            if '%x' % tgt in names and '%x' % tgt != '%x' % rva:
                names['%x' % rva] = names['%x' % tgt]


# 6. same body as an exported function
def norm(b):
    b = re.sub(r'/\*.*?\*/', '', b, flags=re.S)
    b = b.strip()
    b = b.split('\n', 1)[1] if '\n' in b else b          # drop the signature line (function name differs)
    b = re.sub(r'\b(?:FUN|thunk_FUN|SUB|LAB|UNK|DAT|PTR)_[0-9a-f]{8}\b|\bfunc_0x[0-9a-f]{8}\b|\b_UNK_[0-9a-f]{8}\b', 'X', b)
    b = re.sub(r'0x1a[0-9a-f]{5,6}\b', 'A', b)
    return re.sub(r'\s+', ' ', b).strip()


api_path = os.path.join(G, 'api-out', 'api.c')
if os.path.exists(api_path):
    exp = {}
    for p in re.split(r'(?m)^// api\$\$', open(api_path, encoding='utf8').read())[1:]:
        h, _, b = p.partition('\n')
        if len(b) > 300:                                   # short bodies collide too easily
            exp.setdefault(norm(b), h.split()[0])
    for rva, b in body.items():
        if '%x' % rva not in names and len(b) > 300 and norm(b) in exp:
            names['%x' % rva] = exp[norm(b)]

# 5. thunk_FUN_<addr> headers (run again after 8 so aliases of exception factories get the name too)
def thunk_pass():
    for _ in range(3):
        for rva, b in body.items():
            if '%x' % rva in names:
                continue
            m = re.search(r'^\S[^\n]*\bthunk_FUN_([0-9a-f]{8})\(', re.sub(r'/\*.*?\*/', '', b[:600], flags=re.S), re.M)
            if m and '%x' % (int(m.group(1), 16) - BASE) in names:
                names['%x' % rva] = names['%x' % (int(m.group(1), 16) - BASE)]


thunk_pass()

# 8. exception factories: the body names the type with C strings ("System", "<X>Exception", read by ReadCStrings.java
#    into rt-out/rt_strings.tsv): il2cpp_raise_<X> when it never returns (raises), else il2cpp_get_<X>.
#    Also writes rt-out/rt.strings.c: rt.c with every `_UNK_<lit> + 0x<pc>` C-string address replaced by the string.
str_path = os.path.join(G, 'rt-out', 'rt_strings.tsv')
if os.path.exists(str_path):
    strs = {}
    for l in open(str_path, encoding='utf8'):
        p = l.rstrip('\n').split('\t', 2)
        if len(p) == 3 and p[1] == 'S':
            strs[p[0]] = p[2]
        elif len(p) == 2:            # older format: key<TAB>string
            strs[p[0]] = p[1]
    lit_re = re.compile(r'_UNK_([0-9a-f]{8}) \+ 0x([0-9a-f]{6,8})\b')
    for rva, b in body.items():
        if '%x' % rva in names:
            continue
        found = [strs.get(a + '_' + c) for a, c in lit_re.findall(b)]
        exc = sorted(set(s for s in found if s and re.fullmatch(r'[A-Z]\w*Exception', s)))
        if len(exc) == 1 and 'System' in found:
            noreturn = 'halt_baddata' in b or 'Truncating control flow' in b
            names['%x' % rva] = ('il2cpp_raise_' if noreturn else 'il2cpp_get_') + exc[0]
    src = open(os.path.join(G, 'rt-out', 'rt.c'), encoding='utf8').read()
    src = lit_re.sub(lambda m: json.dumps(strs[m.group(1) + '_' + m.group(2)]) if m.group(1) + '_' + m.group(2) in strs else m.group(0), src)
    open(os.path.join(G, 'rt-out', 'rt.strings.c'), 'w', encoding='utf8', newline='\n').write(src)
    thunk_pass()

# 7. Ghidra already knows the name (import thunks such as __cxa_begin_catch): header is not an auto-generated name
sig_re = re.compile(r'^[A-Za-z_][\w \*]*?[ \*]([A-Za-z_]\w*)\(', re.M)
for rva, b in body.items():
    if '%x' % rva in names:
        continue
    m = sig_re.search(re.sub(r'/\*.*?\*/', '', b[:600], flags=re.S))
    if m and not re.match(r'(FUN|thunk_FUN|SUB|LAB|UNK|DAT|PTR|func|native|undefined)', m.group(1)):
        names['%x' % rva] = m.group(1)

tot = sum(cnt.values())
hit = sum(c for r, c in cnt.items() if '%x' % r in names)
print('%d names; %d of %d distinct targets; %.1f%% of %d call sites' % (len(names), sum(1 for r in cnt if '%x' % r in names), len(cnt), 100.0 * hit / tot, tot))
left = sorted(((c, r) for r, c in cnt.items() if '%x' % r not in names), reverse=True)[:25]
print('top unresolved:', ', '.join('%x(%d)' % (r, c) for c, r in left))

# fallback: one stable token per function. `native_<rva>` and `func_0x<rva+base>` of the same function, and every
# thunk_FUN_ alias of it, all print as rt_<rva of the real body>; read the body in rt-out/rt.strings.c (rt$$r<rva>).
for rva, b in body.items():
    if '%x' % rva in names:
        continue
    m = re.search(r'^\S[^\n]*\bthunk_FUN_([0-9a-f]{8})\(', re.sub(r'/\*.*?\*/', '', b[:600], flags=re.S), re.M)
    tgt = int(m.group(1), 16) - BASE if m else rva
    names['%x' % rva] = names.get('%x' % tgt, 'rt_%x' % tgt)
for rva in cnt:
    names.setdefault('%x' % rva, 'rt_%x' % rva)
json.dump(names, open(os.path.join(G, 'rt_names.json'), 'w'), indent=0)
