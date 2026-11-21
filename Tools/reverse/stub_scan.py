"""Find the empty stubs that stub_index.py misses, merge them with the index and write one classified to-do list.

usage: python -I stub_scan.py <Assets/Scripts> <dump.cs> <pseudoc dir> <stub_index.tsv> <out todo-all.tsv> [batches.tsv]

What it adds to the index (source = scan):
  expr    expression-bodied member `=> default/null/0/false/throw ...;` (methods and property getters)
  getter  property accessor `get { return default; }` / `get => null;`
  tail    a "done" body of at most 3 statements whose last one is `return default/null/false/0;` or `throw`
  ctor    type whose dump.cs .ctor/.cctor has non-trivial pseudo-C while the C# file has neither a ctor nor any field
          initializer (file-level check; nested types share the file of their outer type)
  rofield `readonly List/Dictionary/HashSet/Queue/Stack<..>` field declared without `= new`
Every scan row is kept only when the original pseudo-C body at that RVA is non-trivial (it contains a call, a store, or
returns something other than 0), except rofield which has no RVA of its own (checked through the type's .ctor).
Index rows with status todo are copied as source = index.
Action column: mock | real-lib | translate, from the namespace (lists MOCK / REAL below; edit them for another game).
Optional batches.tsv: translate rows grouped into batches of 50-200 methods by class, in ORDER below.
Output columns: assembly  type  method  rva  source  pseudoc  action   (assembly = first folder under Assets/Scripts)
Depends on the game only through the MOCK / REAL / ORDER lists.
"""
import os, re, sys
from collections import Counter, OrderedDict

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import stub_index as si

MOCK = ('BackendMulti', 'RoyalSmash.Backend', 'RoyalSmash.Ads', 'RoyalSmash.IAP', 'RoyalSmash.Support',
        'RoyalSmash.Retention', 'Foundation.Analytics', 'Foundation.SDKManagement', 'Sentry', 'Facebook', 'Firebase',
        'AppsFlyer', 'AppsFlyerSDK', 'AppLovin', 'MaxSdk', 'Helpshift', 'Metica', 'Google',
        'AFMiniJSON', 'HSMiniJSON')
REAL = ('MongoDB', 'Jint', 'Acornima', 'MessagePack', 'Nerdbank', 'System', 'Microsoft', 'Spine')
# Sirenix (Odin) and SRDebugger/SRF/SROptions: translate from pseudo-C (user decision 2026-10-30)
# translate batches: (label, type prefix); first match wins, rest goes to "Other"
ORDER = [('Foundation', 'Foundation.'), ('Core-Gameplay', 'Core.Gameplay'), ('Core-UI', 'Core.UI'),
         ('Core-Shop', 'Core.Shop'), ('Core-Notifications', 'Core.Notifications'),
         ('Core-CDN', 'Core.CDNAssetManagement'), ('Core-Analytics', 'Core.Analytics'),
         ('Core-other', 'Core.'), ('RoyalSmash', 'RoyalSmash.'), ('Sirenix', 'Sirenix.'),
         ('SRDebugger', 'SRDebugger'), ('SRF', 'SRF'), ('SROptions', 'SROptions'), ('Other', '')]
LIVEOPS = 'Core.LiveOps'

RET_DEF = r'(default(\([^)]*\))?|null|false|0|0f|0L|0u|0\.0f?|string\.Empty|"")'
EXPR = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:(?:public|private|protected|internal|static|virtual|override|sealed|new|'
                  r'unsafe|readonly)\s+)*[\w<>\[\],\.\?\s]+?\s+([\w\.]+)\s*(\([^)]*\))?\s*=>\s*(?:' + RET_DEF +
                  r'|throw\b[^;]*)\s*;')
GETTER = re.compile(r'\bget\s*(?:\{\s*return\s+' + RET_DEF + r'\s*;\s*\}|=>\s*' + RET_DEF + r'\s*;|\{\s*throw\b[^}]*\})')
PROP = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:(?:public|private|protected|internal|static|virtual|override|sealed|new|'
                  r'unsafe|readonly)\s+)*[\w<>\[\],\.\?\s]+?\s+([\w\.]+)\s*(?:\{|$)')
ROFIELD = re.compile(r'\breadonly\s+(?:[\w\.]+\.)?(List|Dictionary|HashSet|Queue|Stack|SortedDictionary|'
                     r'LinkedList)<.*>\s+(\w+)\s*;')
FIELD_INIT = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:(?:public|private|protected|internal|static|readonly|const)\s+)+'
                        r'[\w<>\[\],\.\?\s]+?\s+\w+\s*=[^=>]')
TRIVIAL = re.compile(r'il2cpp_(class_init|throw_null|runtime_class_init|codegen_initialize)|^\s*(if \(\*pcVar|piVar\d+ = <|'
                     r'iVar\d+ = \*piVar)')


def action(t):
    for p in MOCK:
        if t == p or t.startswith(p + '.') or '.' not in p and t.startswith(p):   # SDK prefixes: AppsFlyerSupport, MaxSdkBase
            return 'mock'
    for p in REAL:
        if t == p or t.startswith(p + '.'):
            return 'real-lib'
    return 'translate'


class Pseudo:
    def __init__(self, pd):
        self.pd, self.cache = pd, {}

    def body(self, where):
        f, ln = where.rsplit(':', 1)
        if f not in self.cache:
            try:
                self.cache[f] = open(os.path.join(self.pd, f), encoding='utf8', errors='replace').read().split('\n')
            except OSError:
                self.cache[f] = []
        lines, out = self.cache[f], []
        for l in lines[int(ln):]:
            if l.startswith('// ') and '  RVA: 0x' in l:
                break
            out.append(l)
        return out

    def nontrivial(self, where):
        if not where:
            return False
        started = False
        for l in self.body(where):
            s = l.strip()
            if s == '{':
                started = True
                continue
            if not started or not s or s in ('}', 'return;') or TRIVIAL.search(l):
                continue
            if re.match(r'^(undefined\d*|int|uint|char|bool|float|double|longlong|ulonglong|short|ushort|byte|void|code)'
                        r'\s*\**\s*\w+;$', s):
                continue
            m = re.match(r'^return\s+(.*);$', s)
            if m:
                if m.group(1).strip() not in ('0', '0.0', '(undefined4)0', 'param_1'):
                    return True
                continue
            if re.search(r'\w\s*\(', s) and not s.startswith(('if (', 'while (', '} while')) or re.search(r'[^=!<>]=[^=]', s):
                return True
        return False


def scan(root, rva, loc, ps, index_keys):
    rows = []
    for dirpath, _, files in os.walk(root):
        for fn in files:
            if not fn.endswith('.cs'):
                continue
            f = os.path.join(dirpath, fn)
            rel = os.path.relpath(f, root)
            lines = open(f, encoding='utf8', errors='replace').read().split('\n')
            ns, stack, depth, prop = '', [], 0, None
            has_ctor, has_init, types = False, False, []
            for i, line in enumerate(lines):
                s = line.strip()
                m = re.match(r'^namespace\s+([\w\.]+)', s)
                if m:
                    ns = m.group(1)
                tm = si.TYPE.search(s) if not s.startswith('//') else None
                if tm and tm.group(1) in ('class', 'struct', 'record') and '(' not in s.split(tm.group(0))[0] \
                        and not s.startswith(('return', 'var ', 'new ')):
                    stack.append([tm.group(2), depth + 1, False])
                    types.append((ns, '.'.join(t[0] for t in stack)))
                typ = si.unescape('.'.join(t[0] for t in stack))
                if stack and re.match(r'^\s*(?:(?:public|private|protected|internal|static)\s+)*' + re.escape(stack[-1][0])
                                      + r'\s*\(', line):
                    has_ctor = True
                if FIELD_INIT.match(line) and '(' not in line.split('=')[0]:
                    has_init = True

                def add(kind, name, params, where_line):
                    key = (ns, typ, name, params)
                    r = rva.get(key) or rva.get(('', typ, name, params))
                    if r is None or r in index_keys:
                        return
                    w = loc.get(r, '')
                    if ps.nontrivial(w):
                        rows.append((rel, (ns + '.' if ns else '') + typ, '%s(%s)' % (name, params), r, 'scan:' + kind,
                                     w, '%s:%d' % (rel, where_line + 1)))
                if stack:
                    em = EXPR.match(line)
                    if em and em.group(1) not in ('if', 'return'):
                        name = em.group(1).split('.')[-1]
                        if em.group(2) is not None:
                            add('expr', name, si.norm_params(em.group(2)[1:-1]), i)
                        else:
                            add('expr', 'get_' + name, '', i)
                    pm = PROP.match(line)
                    if pm and '(' not in s and '=' not in s and not s.startswith(('return', 'else', 'get', 'set')):
                        prop = pm.group(1).split('.')[-1]
                    if prop and GETTER.search(line):
                        add('getter', 'get_' + prop, '', i)
                        prop = None
                    rm = ROFIELD.search(line)
                    if rm and '=' not in line:
                        r = rva.get((ns, typ, '.ctor', '')) or rva.get(('', typ, '.ctor', ''))
                        rows.append((rel, (ns + '.' if ns else '') + typ, 'field ' + rm.group(2), r, 'scan:rofield',
                                     loc.get(r, '') if r else '', '%s:%d' % (rel, i + 1)))
                depth += line.count('{') - line.count('}')
                for t in stack:
                    if depth >= t[1]:
                        t[2] = True
                while stack and stack[-1][2] and depth < stack[-1][1]:
                    stack.pop()
            if not has_ctor and not has_init:
                for tns, t in types:
                    for name in ('.ctor', '.cctor'):
                        r = rva.get((tns, t, name, '')) or rva.get(('', t, name, ''))
                        if r and r not in index_keys and ps.nontrivial(loc.get(r, '')):
                            rows.append((rel, (tns + '.' if tns else '') + t, name + '()', r, 'scan:ctor',
                                         loc.get(r, ''), rel))
    return rows


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    root, dump, pd, index, out = sys.argv[1:6]
    batches = sys.argv[6] if len(sys.argv) > 6 else None
    rva = si.parse_dump(dump)
    loc = si.parse_pseudoc(pd)
    ps = Pseudo(pd)
    allrows, index_keys = [], set()
    for line in open(index, encoding='utf8'):
        p = line.rstrip('\n').split('\t')
        if p[0] == 'status':
            continue
        r = int(p[4], 16) if p[4] else None
        if r is not None and p[0] == 'todo':
            index_keys.add(r)
        if p[0] == 'todo':
            allrows.append((p[1].split('\\')[0].split('/')[0], p[2], p[3], r, 'index', p[5]))
    files = {}
    for line in open(index, encoding='utf8'):
        p = line.rstrip('\n').split('\t')
        if p[0] != 'done' or not p[4]:
            continue
        f, ln = p[1].rsplit(':', 1)
        if f not in files:
            files[f] = open(os.path.join(root, f), encoding='utf8', errors='replace').read().split('\n')
        lines, k = files[f], int(ln) - 1
        while k < len(lines) and '{' not in lines[k]:
            k += 1
        d, buf = 0, []
        while k < len(lines) and len(buf) < 12:
            buf.append(lines[k])
            d += lines[k].count('{') - lines[k].count('}')
            if d <= 0:
                break
            k += 1
        if d > 0 or not buf:
            continue
        b = re.sub(r'//[^\n]*', '', '\n'.join(buf))
        b = b[b.index('{') + 1:b.rindex('}')]
        stm = [x.strip() for x in b.split(';') if x.strip()]
        if 2 <= len(stm) <= 3 and '{' not in b and re.fullmatch(r'(return\s+' + RET_DEF + r'|throw\b.*)', stm[-1], re.S):
            if ps.nontrivial(p[5]):
                allrows.append((p[1].replace('\\', '/').split('/')[0], p[2], p[3], int(p[4], 16), 'scan:tail', p[5]))
    for rel, t, m, r, src, w, _ in scan(root, rva, loc, ps, index_keys):
        allrows.append((rel.split('\\')[0].split('/')[0], t, m, r, src, w))
    seen, uniq = set(), []
    for row in allrows:
        k = (row[1], row[2], row[3])
        if k not in seen:
            seen.add(k)
            uniq.append(row)
    with open(out, 'w', encoding='utf8') as o:
        o.write('assembly\ttype\tmethod\trva\tsource\tpseudoc\taction\n')
        for a, t, m, r, src, w in uniq:
            o.write('\t'.join([a, t, m, '0x%X' % r if r else '', src, w, action(t)]) + '\n')
    by_act = Counter(action(r[1]) for r in uniq)
    by_src = Counter(r[4] for r in uniq)
    by_top = Counter((r[1].split('.')[0] if '.' in r[1] else '(global)', action(r[1])) for r in uniq)
    print('rows', len(uniq), dict(by_act))
    print('source', dict(by_src))
    for (top, act), n in sorted(by_top.items(), key=lambda kv: -kv[1])[:40]:
        print('  %-28s %-9s %d' % (top, act, n))
    if batches:
        groups = OrderedDict((lab, OrderedDict()) for lab, _ in ORDER)
        groups['LiveOps'] = OrderedDict()
        for row in uniq:
            if action(row[1]) != 'translate':
                continue
            t = row[1]
            lab = 'LiveOps' if t.startswith(LIVEOPS) else next(l for l, p in ORDER if t.startswith(p))
            groups[lab].setdefault(t, []).append(row)
        with open(batches, 'w', encoding='utf8') as o:
            o.write('batch\tgroup\tmethods\tclasses\tfirst_class\tlast_class\n')
            n = 0
            for lab, classes in groups.items():
                cur, cls = [], []
                def flush():
                    nonlocal n, cur, cls
                    if cur:
                        n += 1
                        o.write('%03d\t%s\t%d\t%d\t%s\t%s\n' % (n, lab, len(cur), len(cls), cls[0], cls[-1]))
                    cur, cls = [], []
                for t, rows in classes.items():
                    if cur and len(cur) + len(rows) > 200 and len(cur) >= 50:
                        flush()
                    cur += rows
                    cls.append(t)
                flush()
        print('batches', n)


if __name__ == '__main__':
    main()
