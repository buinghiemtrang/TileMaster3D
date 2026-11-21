"""Map every method of the C# stubs to its RVA and to its pseudo-C, and tell whether it is still an empty stub.

usage: python -I stub_index.py build <Assets/Scripts> <dump.cs> <pseudoc dir> <out.tsv>
       python -I stub_index.py show  <out.tsv> <Ns.Class or file substring> [--todo]
build: writes one row per method:
  status  stub file:line  Ns.Type  method(signature)  RVA  pseudo-C file:line
  status = todo (empty body / single return default/null/false/0 / throw), done (has real code), abstract (no body),
           nobody (extern/interface), norva (no RVA in dump.cs: abstract, generic-only, or not compiled).
  The pseudo-C location is the `// Ns.Type$$Method  RVA: 0x..` header inside <pseudoc>/*.named.c
  (falls back to the raw .c when no .named.c exists).
show:  prints the rows of one class (or of every class in a file), --todo only the empty ones, plus a count.
Matching key: namespace + type (nested as Outer.Inner) + method name + parameter types (modifiers and spaces removed),
so overloads map one to one. Compiler-generated classes (<Foo>d__12, <>c) have no stub; their pseudo-C sits next to the
parent class and must be folded into async/yield/lambdas when translating.
Does not depend on the game.
"""
import glob, os, re, sys


def unescape(s):
    """AssetRipper writes compiler-generated names with _003C/_003E for < and > (e.g. _003CFoo_003Ed__12)."""
    return s.replace('_003C', '<').replace('_003E', '>')


def norm_params(p):
    out = []
    depth, cur = 0, ''
    for ch in p:
        if ch in '<([':
            depth += 1
        elif ch in '>)]':
            depth -= 1
        if ch == ',' and depth == 0:
            out.append(cur)
            cur = ''
        else:
            cur += ch
    if cur.strip():
        out.append(cur)
    res = []
    for a in out:
        a = re.sub(r'\[[^\]]*\]\s*', '', a.strip())                 # attributes
        a = re.sub(r'^(this|ref|out|in|params|readonly|scoped)\s+', '', a)
        a = re.sub(r'^(ref|out|in)\s+', '', a)
        a = a.split('=')[0].strip()                                  # default values
        parts = a.rsplit(None, 1)
        typ = parts[0] if len(parts) == 2 else a
        typ = unescape(typ)
        # C# tuple syntax (string name, object value) -> ValueTuple<string,object> as dump.cs prints it
        for _ in range(3):
            typ = re.sub(r'\(([^()]*)\)', lambda m: 'ValueTuple<%s>' % ','.join(
                x.strip().rsplit(None, 1)[0] if len(x.strip().split()) > 1 else x.strip() for x in m.group(1).split(',')), typ)
        typ = re.sub(r'\s+', '', typ)
        typ = re.sub(r'([\w>\]]+)\?', r'Nullable<\1>', typ)  # int? -> Nullable<int>
        res.append(re.sub(r'\b\w+\.', '', typ))      # drop namespace / outer-type qualifiers, also inside generics
    return ','.join(res)


SIG = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:(?:public|private|protected|internal|static|virtual|override|abstract|sealed|'
                 r'extern|unsafe|new|async|partial|readonly)\s+)*([\w<>\[\],\s\.\?\(\)]+?)\s+([\w\.<>]+?)(?:<[^>()]*>)?\s*\((.*)\)\s*'
                 r'(?:where\s+.*)?(\{.*|;)?\s*$')
TYPE = re.compile(r'\b(class|struct|interface|enum|record)\s+(\w+)')


def strip_generic(t):
    """UnsafeArray<T>.Enumerator -> UnsafeArray.Enumerator (keeps compiler-generated <Foo>d__12)."""
    prev = None
    while prev != t:
        prev = t
        t = re.sub(r'(\w)<[^<>]*>', r'\1', t)
    return t


def parse_dump(path):
    rva = {}
    top_ns = {}            # top-level type name -> namespace; used for nested types (printed with an empty namespace)
    ns, typ, pending = '', '', None
    gen_key, gen_rva, gen_shared = None, None, False   # open generic method (RVA -1): take an instance's RVA
    for line in open(path, encoding='utf8', errors='replace'):
        if line.startswith('// Namespace:'):
            ns = line.split(':', 1)[1].strip()
            continue
        m = re.match(r'^(?:\w+ )*(?:class|struct|interface|enum) ([^\s:]+(?:<[^:{]*?>)?)', line)
        if m and not line.startswith('\t'):
            typ = strip_generic(m.group(1).strip())
            gen_key = None
            if '.' not in typ:
                top_ns[typ] = ns
            continue
        if gen_key is not None and line.startswith('\t|-'):
            m = re.match(r'^\t\|-RVA: 0x([0-9A-F]+)', line)
            if m:
                gen_rva = int(m.group(1), 16)
            elif gen_rva is not None and not line.startswith('\t|-RVA'):
                shared = '__Il2CppFullySharedGenericType' in line
                if gen_key not in rva or (shared and not gen_shared):
                    rva[gen_key] = gen_rva
                    gen_shared = shared
                gen_rva = None
            continue
        m = re.match(r'^\t// RVA: (0x[0-9A-F]+|-1)', line)
        if m:
            pending = -1 if m.group(1) == '-1' else int(m.group(1), 16)
            gen_key = None
            continue
        if pending is not None and line.startswith('\t') and '(' in line:
            s = line.strip()
            m = re.match(r'^(?:[\w\[\]<>,\.\s\?]+?\s)?([\w\.<>`\|]+?)(?:<[^()]*>)?\(([^)]*)\)', s.split(' { }')[0])
            if m:
                name = m.group(1).split(' ')[-1]
                if name not in ('.ctor', '.cctor'):
                    name = name.split('.')[-1]          # explicit interface impl: System.IDisposable.Dispose -> Dispose
                key_ns = ns if ns or '.' not in typ else top_ns.get(typ.split('.')[0], '')
                key = (key_ns, typ, name, norm_params(m.group(2)))
                if pending == -1:      # open generic: the GenericInstMethod block that follows lists instance RVAs
                    gen_key, gen_rva, gen_shared = key, None, False
                else:
                    rva[key] = pending
            pending = None
    return rva


def parse_pseudoc(pd):
    cache = os.path.join(pd, '_rva_index.tsv')       # rebuilt when any pseudo-C file is newer
    newest = max((os.path.getmtime(f) for f in glob.glob(os.path.join(pd, '*.c'))), default=0)
    if os.path.exists(cache) and os.path.getmtime(cache) >= newest:
        return {int(a, 16): w for a, w in (l.rstrip('\n').split('\t') for l in open(cache, encoding='utf8'))}
    loc = {}
    files = glob.glob(os.path.join(pd, '*.named.c'))
    named_stems = set(f[:-len('.named.c')] for f in files)
    files += [f for f in glob.glob(os.path.join(pd, '*.c'))
              if not f.endswith(('.clean.c', '.ann.c', '.named.c')) and f[:-2] not in named_stems]
    for f in files:
        for i, line in enumerate(open(f, encoding='utf8', errors='replace'), 1):
            if line.startswith('// ') and '  RVA: 0x' in line:
                loc.setdefault(int(line.rsplit('0x', 1)[1], 16), '%s:%d' % (os.path.basename(f), i))
    with open(cache, 'w', encoding='utf8') as o:
        for a, w in loc.items():
            o.write('%x\t%s\n' % (a, w))
    return loc


def body_status(body):
    b = re.sub(r'//[^\n]*|/\*.*?\*/', '', body, flags=re.S).strip()[1:-1].strip()
    if not b:
        return 'todo'
    if re.fullmatch(r'(return\s+(default(\([^)]*\))?|null|false|true|0|0f|0L|0u|0\.0|0\.0f|string\.Empty|""|new\s+[\w<>\.,\s\[\]]+\(\))?\s*;|throw\s+[^;]*;)', b):
        return 'todo'
    return 'done'


def parse_stubs(root):
    rows = []
    for f in glob.glob(os.path.join(root, '**', '*.cs'), recursive=True):
        src = open(f, encoding='utf8', errors='replace').read()
        lines = src.split('\n')
        ns = ''
        stack = []          # (type name, brace depth at which it opened)
        depth = 0
        i = 0
        while i < len(lines):
            line = lines[i]
            s = line.strip()
            m = re.match(r'^namespace\s+([\w\.]+)', s)
            if m:
                ns = m.group(1)
            tm = TYPE.search(s) if not s.startswith('//') else None
            if tm and '(' not in s.split(tm.group(0))[0] and not s.startswith(('return', 'var ', 'new ')):
                stack.append([tm.group(2), depth + 1, False])   # name, depth inside its braces, opened yet
            sm = SIG.match(line) if (stack and '(' in s and not s.startswith(('if', 'for', 'while', 'switch', 'return', 'using',
                                                                                'catch', 'lock', 'else', '//', 'new ', 'throw'))) else None
            if sm and sm.group(2) not in ('if', 'for', 'while', 'switch', 'catch', 'using', 'lock', 'foreach', 'return', 'new') \
                    and '=' not in s.split('(')[0]:
                typ = unescape('.'.join(t[0] for t in stack))
                name = unescape(sm.group(2)).split('.')[-1]
                sig = '%s(%s)' % (name, re.sub(r'\s+', ' ', sm.group(3)))
                # body: from the first '{' after the signature to its matching '}'
                j, text = i, ''
                while j < len(lines) and '{' not in lines[j] and ';' not in lines[j]:
                    j += 1
                if j < len(lines) and '{' in lines[j] and not (';' in lines[j] and lines[j].index(';') < lines[j].index('{')):
                    d, k, buf = 0, j, []
                    while k < len(lines):
                        buf.append(lines[k])
                        d += lines[k].count('{') - lines[k].count('}')
                        if d <= 0:
                            break
                        k += 1
                    body = '\n'.join(buf)
                    body = body[body.index('{'):]
                    status = body_status(body)
                    for ln in lines[i:k + 1]:
                        depth += ln.count('{') - ln.count('}')
                    rows.append((status, '%s:%d' % (os.path.relpath(f, root), i + 1), ns, typ, name, norm_params(sm.group(3)), sig))
                    i = k + 1
                    while stack and stack[-1][2] and depth < stack[-1][1]:
                        stack.pop()
                    continue
                rows.append(('abstract' if 'abstract' in s else 'nobody', '%s:%d' % (os.path.relpath(f, root), i + 1), ns,
                             typ, name, norm_params(sm.group(3)), sig))
            depth += line.count('{') - line.count('}')
            for t in stack:
                if depth >= t[1]:
                    t[2] = True
            while stack and stack[-1][2] and depth < stack[-1][1]:
                stack.pop()
            i += 1
    return rows


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')   # file names use full-width ＜＞ on Windows; -I ignores PYTHONIOENCODING
    if sys.argv[1] == 'build':
        root, dump, pd, out = sys.argv[2:6]
        rva = parse_dump(dump)
        loc = parse_pseudoc(pd)
        rows = parse_stubs(root)
        n = {'todo': 0, 'done': 0, 'norva': 0}
        with open(out, 'w', encoding='utf8') as o:
            o.write('status\tstub\ttype\tmethod\trva\tpseudoc\n')
            for status, where, ns, typ, name, params, sig in rows:
                if name == typ.split('.')[-1]:
                    name = '.ctor'
                r = rva.get((ns, typ, name, params))
                if r is None and '.' in typ:         # nested type printed before its outer type: namespace unknown
                    r = rva.get(('', typ, name, params))
                if r is None and status in ('todo', 'done'):
                    status = 'norva' if status == 'todo' else status
                n[status] = n.get(status, 0) + 1
                o.write('\t'.join([status, where, (ns + '.' if ns else '') + typ, sig,
                                   '0x%X' % r if r is not None else '', loc.get(r, '') if r is not None else '']) + '\n')
        print('%d methods: %s' % (len(rows), ', '.join('%s %d' % kv for kv in sorted(n.items()))))
    elif sys.argv[1] == 'show':
        tsv, key = sys.argv[2], sys.argv[3]
        todo = '--todo' in sys.argv
        k = 0
        for line in open(tsv, encoding='utf8'):
            p = line.rstrip('\n').split('\t')
            if p[0] == 'status' or not (p[2] == key or key in p[1]):
                continue
            if todo and p[0] != 'todo':
                continue
            print('%-6s %-10s %-60s %s' % (p[0], p[4], p[3][:60], p[5] or '-'))
            k += 1
        print(k, 'rows')


if __name__ == '__main__':
    main()
