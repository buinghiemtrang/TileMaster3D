"""Mechanical fixes for AssetRipper/ILSpy decompilation artifacts in stub scripts.

usage: python -I fix_decompile.py <Assets/Scripts dir> [unity_log_for_operator_pairs]
Idempotent: safe to run repeatedly.
"""
import os, re, sys

root = sys.argv[1]
log = sys.argv[2] if len(sys.argv) > 2 else None

TYPE = r'[\w.]+(?:<[^<>]*(?:<[^<>]*>[^<>]*)*>)?(?:\[\])*\??'
rules = [
    # `[In] ref T x` is how the decompiler prints `in T x`
    (re.compile(r'\[In\]\s*ref\s+'), 'in '),
    # optional `in` parameter with a default: give it `= default`
    (re.compile(r'\[Optional\]\s*\[DefaultParameterValue\(null\)\]\s*in\s+(' + TYPE + r')\s+(\w+)'), r'in \1 \2 = default'),
    # not available in Unity's BCL
    (re.compile(r'MethodImplOptions\.AggressiveOptimization'), '(MethodImplOptions)512'),
    # attributes that are invalid where the decompiler placed them / compiler-reserved
    (re.compile(r'\[NonVersionable\]\s*'), ''),
    (re.compile(r'\[(?:Microsoft\.CodeAnalysis\.)?Embedded\]\s*'), ''),
    # `out T* p` assigned null is printed as `Unsafe.As<T*, @null>(ref p) = null`
    (re.compile(r'System\.Runtime\.CompilerServices\.Unsafe\.As<[^;]*?, @null>\(ref (\w+)\) = null;'), r'\1 = null;'),
    # null-ref argument passed to an `in` parameter
    (re.compile(r'ref \*\(' + TYPE + r'\*\)null'), 'default'),
    # C# 9: ref structs cannot implement interfaces (pattern-based Dispose still works)
    (re.compile(r'(ref struct \w+(?:<[^>]*>)?)\s*:\s*IDisposable\b'), r'\1'),
]
# Sentry ships internalized copies of BCL libraries; once split into their own
# assemblies their types must be public for Sentry to see them.
public_dirs = ('Sentry.System.',)
public_rx = re.compile(r'^(\t?)internal ((?:(?:sealed|static|abstract|readonly|ref|partial|unsafe) )*(?:class|struct|interface|enum|delegate)\b)', re.M)
# Foundation.Unmanaged: the `unmanaged` constraint is lost as `struct`
unmanaged_dirs = ('Foundation.Unmanaged',)
unmanaged_rx = re.compile(r'(where\s+\w+\s*:\s*)struct\b')

changed = 0
for d, _, files in os.walk(root):
    for f in files:
        p = os.path.join(d, f)
        if f == 'EmbeddedAttribute.cs' and re.search(r'[\\/]Microsoft[\\/]CodeAnalysis$', d):
            os.remove(p)
            if os.path.exists(p + '.meta'): os.remove(p + '.meta')
            changed += 1
            continue
        if not f.endswith('.cs'): continue
        src = open(p, encoding='utf8', errors='surrogateescape').read()
        new = src
        for rx, rep in rules:
            new = rx.sub(rep, new)
        rel = os.path.relpath(p, root)
        if rel.split(os.sep)[0] in unmanaged_dirs:
            new = unmanaged_rx.sub(r'\1unmanaged', new)
        if rel.split(os.sep)[0].startswith(public_dirs):
            new = public_rx.sub(r'\1public \2', new)
        if new != src:
            open(p, 'w', encoding='utf8', errors='surrogateescape', newline='').write(new)
            changed += 1
print('files changed:', changed)

# CS0216: operator X defined without its pair -> add the pair as a stub
if log:
    pair = {'==': '!=', '!=': '==', '<': '>', '>': '<', '<=': '>=', '>=': '<='}
    err = re.compile(r"^(.*?\.cs)\((\d+),\d+\): error CS0216: The operator '[^']*operator (\S+)\(([^)]*)\)' requires a matching operator '(\S+)'")
    todo = {}
    for line in open(log, encoding='utf8', errors='ignore'):
        m = err.match(line.strip())
        if m: todo.setdefault(m.group(1), set()).add((int(m.group(2)), m.group(3), m.group(5)))
    proj = os.path.dirname(os.path.dirname(os.path.abspath(root)))
    for rel, items in todo.items():
        p = os.path.join(proj, rel.replace('\\', os.sep))
        if not os.path.exists(p): continue
        lines = open(p, encoding='utf8', errors='surrogateescape').read().split('\n')
        for ln, op, want in sorted(items, reverse=True):
            decl = lines[ln - 1]
            m = re.search(r'public static (\S+) operator ' + re.escape(op) + r'\((.*)\)', decl)
            if not m: continue
            if re.search(r'operator ' + re.escape(want) + r'\(' + re.escape(m.group(2)) + r'\)', '\n'.join(lines)): continue
            indent = re.match(r'\s*', decl).group(0)
            stub = [indent + f'public static {m.group(1)} operator {want}({m.group(2)})', indent + '{', indent + '\treturn default;', indent + '}', '']
            # insert before the declaration (attributes above it stay with the original)
            i = ln - 1
            while i > 0 and lines[i - 1].strip().startswith('['): i -= 1
            lines[i:i] = stub
        open(p, 'w', encoding='utf8', errors='surrogateescape', newline='').write('\n'.join(lines))
        print('operator pairs added in', rel)

# ---- log-driven fixes for constructors / readonly fields ----
if log:
    proj = os.path.dirname(os.path.dirname(os.path.abspath(root)))
    index = {}
    def find_class(name):
        if not index:
            for d, _, fs in os.walk(root):
                for f in fs:
                    if f.endswith('.cs'): index.setdefault(f[:-3], []).append(os.path.join(d, f))
        return index.get(name.split('<')[0], [])

    def split_top(s):
        out, depth, cur = [], 0, ''
        for ch in s:
            if ch in '<([': depth += 1
            if ch in '>)]': depth -= 1
            if ch == ',' and depth == 0: out.append(cur); cur = ''
            else: cur += ch
        if cur.strip(): out.append(cur)
        return out

    def base_args(base):
        bare = base.split('<')[0].split('.')[-1]
        best = None
        for p in find_class(bare):
            src = open(p, encoding='utf8', errors='surrogateescape').read()
            for m in re.finditer(r'(?:public|protected|internal)[\w\s]*\b' + bare + r'\(([^)]*)\)\s*(?::|\{|\n)', src):
                params = [x for x in split_top(m.group(1)) if x.strip()]
                if best is None or len(params) < len(best): best = params
        if best is None: return None
        args = []
        for prm in best:
            prm = re.sub(r'\[[^\]]*\]', '', prm).split('=')[0].strip()
            toks = prm.split()
            mod = toks[0] if toks[0] in ('in', 'ref', 'out', 'params') else ''
            typ = ' '.join(toks[1 if mod else 0:-1])
            if mod in ('ref', 'out'): return None   # needs a real variable; fix by hand
            args.append(f'({typ})default')
        return args

    ctor_err = re.compile(r"^(.*?\.cs)\((\d+),\d+\): error CS(7036|1729): .*?'([\w.<>, ]+?)(?:\.\w+)?(?:\.\w+\(.*\))?'")
    ro_err = re.compile(r"^(.*?\.cs)\((\d+),\d+\): error CS0191")
    edits = {}
    for line in open(log, encoding='utf8', errors='ignore'):
        line = line.strip()
        m = ctor_err.match(line) or ro_err.match(line)
        if m: edits.setdefault(m.group(1), set()).add((int(m.group(2)), line))
    for rel, items in edits.items():
        p = os.path.join(proj, rel.replace('\\', os.sep))
        if not os.path.exists(p): continue
        lines = open(p, encoding='utf8', errors='surrogateescape').read().split('\n')
        for ln, msg in sorted(items, reverse=True):
            l = lines[ln - 1]
            ind = re.match(r'\s*', l).group(0)
            if 'CS0191' in msg:
                fm = re.search(r'(\w+)\s*=', l)
                if fm:
                    fld = fm.group(1)
                    for i, x in enumerate(lines):
                        if re.search(r'\breadonly\b[^=;(]*\b' + fld + r'\s*[;=]', x):
                            lines[i] = x.replace('readonly ', '', 1)
                continue
            cm = re.search(r'class (\w+)(?:<[^>]*>)?\s*:\s*([\w.]+(?:<[^>]*>)?)', l)
            if cm:   # class without ctor: add a private one that calls base
                args = base_args(cm.group(2))
                if args is None: continue
                j = ln
                while lines[j].strip() != '{': j += 1
                lines[j + 1:j + 1] = [ind + f'\tprivate {cm.group(1)}()', ind + f'\t\t: base({", ".join(args)})', ind + '\t{', ind + '\t}', '']
                continue
            km = re.search(r'\b(\w+)\s*\([^)]*\)\s*$', l)
            if km and not lines[ln].strip().startswith(':'):   # ctor without base call
                # find the base type from the enclosing class declaration
                for i in range(ln - 1, -1, -1):
                    cm = re.search(r'class ' + km.group(1) + r'(?:<[^>]*>)?\s*:\s*([\w.]+(?:<[^>]*>)?)', lines[i])
                    if cm: break
                else: continue
                args = base_args(cm.group(1))
                if args is None: continue
                lines.insert(ln, ind + f'\t: base({", ".join(args)})')
        open(p, 'w', encoding='utf8', errors='surrogateescape', newline='').write('\n'.join(lines))
        print('ctor/readonly fixes in', rel)
