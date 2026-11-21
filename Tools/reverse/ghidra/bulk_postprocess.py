"""Turn raw <Class>.c files into <Class>.named.c for many classes with a handful of Ghidra launches in total.

usage: python -I bulk_postprocess.py <WORK> <GHIDRA> [--only-new] [--slim] [--pseudoc DIR] [--project DIR]
  Reads  <WORK>/ghidra/pseudoc/<Class>.c            (from DecompileBulk.java or DecompileTargets.java)
  Writes <Class>.clean.c, <Class>.ann.c, <Class>.named.c next to them.
  --only-new  skips classes whose .named.c is already newer than the .c.
  --slim      deletes the intermediate .clean.c/.ann.c afterwards (keep raw .c + .named.c; for the whole .so).
  --project   Ghidra project dir (default <WORK>/ghidra/project); must not be open in another Ghidra.
Streams file by file (works for 30k+ classes / 1 GB+ of pseudo-C without holding it in memory).
Also names what script.json cannot: ELF imports/exports and PLT stubs (elf_names.py, needs `pip install pyelftools`),
runtime helpers (build_rt_names.py + <WORK>/ghidra/rt_overrides.json), and metadata cells without a name such as
`__stack_chk_guard` (Ghidra symbol) or `il2cpp_defaults` (<WORK>/ghidra/slot_overrides.json). Leftovers print as
`<DAT_0xxxxxxx>` / `native_<rva>` / `func_0x<addr>`.
Replaces, for lots of classes, decompile_classes.sh (clean + annotate) and name_funcs.sh. Those start Ghidra once per
class (meta_slots.py) which is too slow for thousands of classes, and call `analyzeHeadless` without `.bat`.
Does not depend on the game. Needs <WORK>/il2cpp/script.json and <WORK>/ghidra/{project,veneer_names.json}.
"""
import glob, json, os, re, runpy, shutil, subprocess, sys, tempfile

WORK, GHIDRA = sys.argv[1], sys.argv[2]
only_new = '--only-new' in sys.argv
slim = '--slim' in sys.argv
HERE = os.path.dirname(os.path.abspath(__file__))
GD = os.path.join(WORK, 'ghidra')
opt = lambda k, d: sys.argv[sys.argv.index(k) + 1] if k in sys.argv else d
PD = opt('--pseudoc', os.path.join(GD, 'pseudoc'))
PROJECT = opt('--project', os.path.join(GD, 'project'))
HEADLESS = os.path.join(GHIDRA, 'support', 'analyzeHeadless.bat' if os.name == 'nt' else 'analyzeHeadless')
BASE = 0x10000


def ghidra(script, *args):
    cmd = [HEADLESS, PROJECT, 'royal', '-process', 'libil2cpp.so', '-noanalysis', '-readOnly',
           '-scriptPath', HERE, '-postScript', script] + list(args)
    r = subprocess.run(cmd, cwd=GD, input=b'\n', capture_output=True)
    out = r.stdout.decode(errors='replace')
    for l in out.splitlines():
        if 'ERROR' in l or 'Exception' in l:
            print('  ghidra:', l[:200])
    return out


def run_script(name, *argv):
    old = sys.argv
    sys.argv = [name] + list(argv)
    try:
        runpy.run_path(os.path.join(HERE, name), run_name='__main__')
    finally:
        sys.argv = old


def rd(p):
    return open(p, encoding='utf8').read()


def wr(p, t):
    open(p, 'w', encoding='utf8', newline='\n').write(t)


# ---- select raw files
raw = [f for f in glob.glob(os.path.join(PD, '*.c')) if not f.endswith(('.clean.c', '.ann.c', '.named.c'))]
if only_new:
    raw = [f for f in raw if not (os.path.exists(f[:-2] + '.named.c') and os.path.getmtime(f[:-2] + '.named.c') >= os.path.getmtime(f))]
print(len(raw), 'raw files to process', flush=True)
if not raw:
    sys.exit(0)
stem = lambda f: f[:-2]

# ---- 1. normalize + clean, collect metadata slot pairs (streaming)
sys.path.insert(0, HERE)
import normalize_unk
pair_re = re.compile(r'&UNK_([0-9a-f]{8}) \+ _UNK_([0-9a-f]{8})')
pairs = set()
for i, f in enumerate(raw):
    s = rd(f)
    t = normalize_unk.fix(s)
    if t != s:
        wr(f, t)
    run_script('clean_pseudoc.py', f, stem(f) + '.clean.c')
    pairs.update(pair_re.findall(rd(stem(f) + '.clean.c')))
    if i % 2000 == 1999:
        print('  cleaned', i + 1, flush=True)
print('cleaned', len(raw), flush=True)

# ---- 2. metadata slots: one Ghidra run
tmp = tempfile.mkdtemp()
pf, of = os.path.join(tmp, 'p.txt'), os.path.join(tmp, 'o.tsv')
open(pf, 'w').write('\n'.join('%s %s' % v for v in pairs))
print(len(pairs), 'distinct slots -> Ghidra', flush=True)
ghidra('ResolveMetaSlots.java', pf, of)
d = json.load(open(os.path.join(WORK, 'il2cpp', 'script.json'), encoding='utf8'))
names = {}
for k in ('ScriptMetadata', 'ScriptMetadataMethod'):
    for m in d[k]:
        names[m['Address']] = m['Name']
for m in d.get('ScriptString', []):
    names[m['Address']] = 'string ' + json.dumps(m['Value'], ensure_ascii=False)
del d
slots, cells = {}, {}
for line in open(of, encoding='utf8'):
    k, _, va = line.rstrip('\n').split('\t')
    if int(va) in names:
        slots[k] = names[int(va)]
    else:
        cells[k] = int(va)
del names
if cells:  # cells script.json does not know: manual overrides, then Ghidra symbols (EXTERNAL imports), else DAT_
    ov = {}
    ovp = os.path.join(GD, 'slot_overrides.json')
    if os.path.exists(ovp):
        ov = {k.lower().lstrip('0x'): v for k, v in json.load(open(ovp)).items() if not k.startswith('_')}
    need = sorted(set(v for v in cells.values() if '%x' % v not in ov))
    sym = {}
    for i in range(0, len(need), 200):
        out = ghidra('SymbolsAt.java', *['%x' % v for v in need[i:i + 200]])
        for m in re.finditer(r'SYM ([0-9a-f]+) -> [0-9a-f]+ block=(\S+)((?: \[[^\]]+\])*)', out):
            nm = [n for n in re.findall(r'\[([^\]]+)\]', m.group(3)) if not re.match(r'(DAT|SUB|LAB|UNK|PTR|FUN)_', n)]
            if nm:
                sym[int(m.group(1), 16)] = nm[0]
    for k, v in cells.items():
        slots[k] = ov.get('%x' % v) or sym.get(v) or 'DAT_%08x' % (v + BASE)
    print(len(cells), 'slots without a script.json name;', len(set(cells.values())), 'distinct cells', flush=True)

# ---- 3. annotate (same two rules as annotate_pseudoc.py), collect func_0x addresses
deref_re = re.compile(r'(\*+)\((?:undefined\d|u?int|u?short|code) \*+\)\(&UNK_([0-9a-f]{8}) \+ _UNK_([0-9a-f]{8})\)')
drop_re = re.compile(r'\s*(char \*pcVar\d+;|\*pcVar\d+ = .\\x01.;|pcVar\d+ = &UNK_)')
fre = re.compile(r'func_0x([0-9a-f]{8})')


def sub(m):
    name = slots.get(m.group(2) + '_' + m.group(3))
    if name is None:  # not a metadata slot (e.g. a .bss init flag): keep the original expression
        return m.group(0)
    two = len(m.group(1)) > 1
    return ('<<' if two else '<') + name + ('>>' if two else '>')


seen = set()
cpairs = set()   # `&UNK_a + _UNK_b` left after annotation: not a metadata slot, often a C string (icall names)
for f in raw:
    t = deref_re.sub(sub, rd(stem(f) + '.clean.c'))
    t = '\n'.join(l for l in t.split('\n') if 'il2cpp_init_metadata(<' not in l and not drop_re.match(l))
    wr(stem(f) + '.ann.c', t)
    seen.update(fre.findall(t))
    cpairs.update(pair_re.findall(t))
    if slim:
        os.remove(stem(f) + '.clean.c')
del slots
print('annotated', len(raw), flush=True)

# ---- 3b. remaining address pairs that point at a C string (e.g. `il2cpp_resolve_icall("UnityEngine.X::Y(...)")`)
cstr = {}
if cpairs:
    open(pf, 'w').write('\n'.join('%s %s' % (b, a) for a, b in cpairs))   # ReadCStrings wants "<lit> <pc>"
    ghidra('ReadCStrings.java', pf, of)
    ns = 0
    for l in open(of, encoding='utf8'):
        k, kind, v = l.rstrip('\n').split('\t', 2)
        b, a = k.split('_')
        if kind == 'S' and len(v) >= 3:
            cstr[a + '_' + b] = json.dumps(v, ensure_ascii=False)
            ns += 1
        elif kind == 'A':   # plain data address (static variable, cache cell, init flag): Ghidra's DAT_ name
            cstr[a + '_' + b] = '&DAT_%08x' % int(v, 16)
    print(len(cpairs), 'non-slot address pairs;', ns, 'C strings;', len(cstr) - ns, 'data addresses', flush=True)
cast_re = re.compile(r'\((?:char|byte|undefined\d?|int|uint|code) \*\)\(&UNK_([0-9a-f]{8}) \+ _UNK_([0-9a-f]{8})\)')
bare_re = re.compile(r'&UNK_([0-9a-f]{8}) \+ _UNK_([0-9a-f]{8})')


def cstr_sub(m):
    return cstr.get(m.group(1) + '_' + m.group(2), m.group(0))

# ---- 4. func_0x -> managed names: one Ghidra run for the addresses not yet known
fn_path = os.path.join(GD, 'func_names.json')
known = json.load(open(fn_path)) if os.path.exists(fn_path) else {}
todo = sorted(x for x in seen if x not in known)
print(len(todo), 'new func_0x addresses', flush=True)
if todo:
    open(os.path.join(GD, 'allfuncs.txt'), 'w').write('\n'.join(todo) + '\n')
    ghidra('ResolveVeneers.java', os.path.join(GD, 'allfuncs.txt'), os.path.join(GD, 'allfuncs.tsv'))
    run_script('build_func_names.py', GD, os.path.join(WORK, 'il2cpp', 'script.json'), '--merge')
    known = json.load(open(fn_path))
rcnt = {}
for f in raw:
    t = fre.sub(lambda m: known.get(m.group(1), m.group(0)).replace('$$', '.'), rd(stem(f) + '.ann.c'))
    if cstr:
        t = bare_re.sub(cstr_sub, cast_re.sub(cstr_sub, t))
    wr(stem(f) + '.named.c', t)
    for x in re.findall(r'native_([0-9a-f]+)', t):
        rcnt[int(x, 16)] = rcnt.get(int(x, 16), 0) + 1
    for x in fre.findall(t):
        rcnt[int(x, 16) - BASE] = rcnt.get(int(x, 16) - BASE, 0) + 1
    if slim:
        os.remove(stem(f) + '.ann.c')
del known

# ---- 5. runtime helpers / libc imports that remain as native_<rva> or func_0x<addr>
elf_json = os.path.join(GD, 'elf_names.json')
if not os.path.exists(elf_json):
    lib = [os.path.join(r, 'libil2cpp.so') for r, _, fs in os.walk(os.path.join(WORK, 'apk', 'lib')) if 'libil2cpp.so' in fs]
    if lib:
        subprocess.run([sys.executable, '-I', os.path.join(HERE, 'elf_names.py'), lib[0], elf_json])
if os.path.exists(elf_json):
    old = os.path.join(GD, 'rt_counts.json')   # keep counts of earlier runs so rt_names covers every class
    if only_new and os.path.exists(old):
        for k, v in json.load(open(old)).items():
            rcnt[int(k)] = rcnt.get(int(k), 0) + v
    json.dump(rcnt, open(old, 'w'))
    with open(os.path.join(GD, 'rt.tsv'), 'w') as f:
        for r, c in sorted(rcnt.items(), key=lambda x: -x[1]):
            f.write('%d\trt$$r%x\n' % (r, r))
    shutil.rmtree(os.path.join(GD, 'rt-out'), ignore_errors=True)
    ghidra('DecompileBulk.java', os.path.join(GD, 'rt.tsv'), os.path.join(GD, 'rt-out'), '60', 'create')
    if not os.path.exists(os.path.join(GD, 'api-out', 'api.c')):   # exported il2cpp_* bodies, to match identical helpers
        with open(os.path.join(GD, 'api.tsv'), 'w') as f:
            for k, v in sorted(json.load(open(elf_json)).items(), key=lambda x: int(x[0], 16)):
                if v.startswith('il2cpp_') and int(k, 16) < 0x6000000:
                    f.write('%d\tapi$$%s\n' % (int(k, 16), v))
        ghidra('DecompileBulk.java', os.path.join(GD, 'api.tsv'), os.path.join(GD, 'api-out'), '60', 'create')
    # C strings the runtime bodies address (exception type names, messages) -> rt-out/rt_strings.tsv
    lit = set(re.findall(r'_UNK_([0-9a-f]{8}) \+ 0x([0-9a-f]{6,8})\b', rd(os.path.join(GD, 'rt-out', 'rt.c'))))
    open(os.path.join(GD, 'rt-out', 'rt_str_pairs.txt'), 'w').write('\n'.join('%s %s' % p for p in lit))
    ghidra('ReadCStrings.java', os.path.join(GD, 'rt-out', 'rt_str_pairs.txt'), os.path.join(GD, 'rt-out', 'rt_strings.tsv'))
    run_script('build_rt_names.py', WORK, os.path.join(GD, 'rt_overrides.json'))
    rt = json.load(open(os.path.join(GD, 'rt_names.json')))
    n1 = lambda m: rt.get(m.group(1).lstrip('0') or '0', m.group(0))
    n2 = lambda m: rt.get('%x' % (int(m.group(1), 16) - BASE), m.group(0))
    # labels the project already carries as il2cpp_helper_<last 5 hex digits of the Ghidra address 0x01aXXXXX>
    n3 = lambda m: rt.get('%x' % (0x1a00000 + int(m.group(1), 16) - BASE), m.group(0))
    for f in raw:
        p = stem(f) + '.named.c'
        t = rd(p)
        u = re.sub(r'il2cpp_helper_([0-9a-f]{5})\b', n3, fre.sub(n2, re.sub(r'native_([0-9a-f]+)', n1, t)))
        if u != t:
            wr(p, u)
else:
    print('no elf_names.json (pip install pyelftools): runtime helpers stay unnamed')
print('named', len(raw), flush=True)
