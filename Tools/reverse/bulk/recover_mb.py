#!/usr/bin/env python3
"""Recover MonoBehaviour/ScriptableObject .asset data from the original bundle raw bytes.

AssetRipper left some assets empty (layout mismatch). This tool reads the raw object bytes from
data.unity3d (no typetree in release build), decodes them using the serialized fields of the C#
classes found in PROJ/Assets/Scripts (field order = declaration order, base class first), and
rewrites the .asset YAML after the `m_EditorClassIdentifier:` line.

Usage: recover_mb.py [--write] [--dump] NAME [NAME ...]    (NAME = m_Name of asset; dry run unless --write)
"""
import os, re, sys, struct, glob, shutil, collections
import UnityPy

W = os.environ['WORK']
PROJ = os.environ['REPO']
BUNDLE = W + '/apk/base/assets/bin/Data/data.unity3d'
SCRIPTS = PROJ + '/Assets/Scripts'
BACKUP = W + '/backup/asset-recover'

PRIM = {'int': 'i', 'uint': 'I', 'short': 'h', 'ushort': 'H', 'byte': 'B', 'sbyte': 'b', 'long': 'q', 'ulong': 'Q',
        'float': 'f', 'double': 'd', 'bool': '?', 'char': 'H', 'Int32': 'i', 'Single': 'f', 'Boolean': '?'}
BUILTIN = {'Vector2': ['x', 'y'], 'Vector3': ['x', 'y', 'z'], 'Vector4': ['x', 'y', 'z', 'w'],
           'Quaternion': ['x', 'y', 'z', 'w'], 'Color': ['r', 'g', 'b', 'a'], 'Rect': ['x', 'y', 'width', 'height'],
           'Vector2Int': ['x', 'y'], 'Vector3Int': ['x', 'y', 'z']}
OBJ_ROOTS = {'ScriptableObject', 'MonoBehaviour', 'Object', 'Sprite', 'Texture2D', 'Texture', 'GameObject', 'Component',
             'Transform', 'RectTransform', 'AudioClip', 'Material', 'Mesh', 'Font', 'TextAsset', 'Shader',
             'AnimationClip', 'RuntimeAnimatorController', 'Behaviour', 'Image', 'TMP_FontAsset', 'VideoClip',
             'PhysicsMaterial', 'AnimatorController', 'Canvas', 'Camera', 'ParticleSystem', 'Button', 'Graphic',
             'MaskableGraphic', 'Selectable', 'UIBehaviour', 'Renderer', 'SpriteRenderer', 'Collider'}


# ---------------------------------------------------------------- C# index
class Cls:
    def __init__(s, name, kind, tparams, bases, body, ns, file):
        s.name, s.kind, s.tparams, s.bases, s.body, s.ns, s.file = name, kind, tparams, bases, body, ns, file
        s._fields = None


def strip_comments(t):
    t = re.sub(r'/\*.*?\*/', '', t, flags=re.S)
    return re.sub(r'//[^\n]*', '', t)


def match_brace(t, i):
    d = 0
    n = len(t)
    while i < n:
        c = t[i]
        if c == '"':
            i += 1
            while i < n and t[i] != '"':
                i += 2 if t[i] == '\\' else 1
        elif c == '{':
            d += 1
        elif c == '}':
            d -= 1
            if d == 0:
                return i
        i += 1
    return n - 1


TYPE_RE = re.compile(r'\b(class|struct|enum|interface)\s+(\w+)\s*(<[^>{]*>)?\s*(?::\s*([^{]+?))?\s*(?:where[^{]+)?\{')
INDEX = collections.defaultdict(list)


def index_file(path):
    t = strip_comments(open(path, encoding='utf-8-sig', errors='replace').read())
    nsm = re.search(r'\bnamespace\s+([\w.]+)', t)
    ns = nsm.group(1) if nsm else ''

    def walk(text, base):
        pos = 0
        while True:
            m = TYPE_RE.search(text, pos)
            if not m:
                return
            end = match_brace(text, m.end() - 1)
            body = text[m.end():end]
            tps = [x.strip() for x in (m.group(3) or '')[1:-1].split(',') if x.strip()]
            bases = [b.strip() for b in re.split(r',(?![^<]*>)', m.group(4) or '') if b.strip()]
            c = Cls(m.group(2), m.group(1), tps, bases, body, ns, path)
            INDEX[c.name].append(c)
            walk(body, base)
            pos = end + 1
    walk(t, '')


def build_index():
    for p in glob.glob(SCRIPTS + '/**/*.cs', recursive=True):
        index_file(p)


def find_cls(name, ns=None):
    name = name.split('.')[-1]
    c = INDEX.get(name, [])
    if ns:
        c2 = [x for x in c if x.ns == ns]
        if c2:
            c = c2
    return c[0] if c else None


def split_generic(s):
    out, d, cur = [], 0, ''
    for ch in s:
        if ch == '<':
            d += 1
        if ch == '>':
            d -= 1
        if ch == ',' and d == 0:
            out.append(cur.strip())
            cur = ''
        else:
            cur += ch
    if cur.strip():
        out.append(cur.strip())
    return out


class T:  # parsed type ref
    def __init__(s, name, args=(), arr=False):
        s.name, s.args, s.arr = name, list(args), arr

    def __repr__(s):
        return s.name + ('<' + ','.join(map(repr, s.args)) + '>' if s.args else '') + ('[]' if s.arr else '')


def parse_type(s):
    s = s.strip().rstrip('?')
    if s.endswith('[]'):
        return T('', [parse_type(s[:-2])], True)
    m = re.match(r'([\w.]+)\s*(?:<(.*)>)?$', s)
    if not m:
        raise ValueError('type? ' + s)
    return T(m.group(1).split('.')[-1], [parse_type(a) for a in split_generic(m.group(2) or '')])


def subst(t, env):
    if t.name in env and not t.args:
        return env[t.name]
    return T(t.name, [subst(a, env) for a in t.args], t.arr)


def members(body):
    """Split depth-1 statements of a class body -> list of (attrs, text) for field candidates."""
    out, i, n = [], 0, len(body)
    cur = ''
    d = 0
    while i < n:
        c = body[i]
        if c == '"':
            j = i + 1
            while j < n and body[j] != '"':
                j += 2 if body[j] == '\\' else 1
            cur += body[i:j + 1]
            i = j + 1
            continue
        if c == '{':
            e = match_brace(body, i)
            if re.search(r'=(?!=|>)', cur) and '=>' not in cur and not re.search(r'\)\s*$', cur.strip()):
                cur += body[i:e + 1]  # initializer
            else:
                cur = ''  # method / property / nested type body: discard
            i = e + 1
            if cur == '':
                continue
            continue
        if c == ';':
            out.append(cur.strip())
            cur = ''
        else:
            cur += c
        i += 1
    return out


def is_serialized(mods_attrs, is_public, is_static, is_const, is_ro):
    if is_static or is_const:
        return False
    if re.search(r'\bNonSerialized\b', mods_attrs):
        return False
    if re.search(r'\bSerializeField\b|\bSerializeReference\b', mods_attrs):
        return True
    return is_public and not is_ro


def own_fields(c):
    """[(name, type, isref)] for class c (own only, in order)."""
    if c._fields is not None:
        return c._fields
    res = []
    for st in members(c.body):
        if not st:
            continue
        attrs = ' '.join(re.findall(r'\[[^\]]*\]', st))
        decl = re.sub(r'\[[^\]]*\]', ' ', st).strip()
        if '=>' in decl:
            continue
        if re.match(r'(delegate|event|using|namespace)\b', decl) or '(' in decl.split('=')[0]:
            continue
        head = decl.split('=')[0].strip()
        toks = re.findall(r'[\w.]+(?:<[^;=]*>)?(?:\[\])?\??|\S', head)
        # modifiers
        mods = []
        while toks and toks[0] in ('public', 'private', 'protected', 'internal', 'static', 'readonly', 'const', 'new',
                                   'volatile', 'unsafe'):
            mods.append(toks.pop(0))
        if len(toks) < 2:
            continue
        tname = toks[0]
        names = [x for x in ''.join(toks[1:]).split(',') if x]
        pub = 'public' in mods
        if not is_serialized(attrs, pub, 'static' in mods, 'const' in mods, 'readonly' in mods):
            continue
        try:
            ty = parse_type(tname)
        except Exception:
            continue
        for nm in names:
            res.append((nm.strip(), ty, bool(re.search(r'\bSerializeReference\b', attrs))))
    c._fields = res
    return res


def all_fields(c, env):
    """Fields incl. bases. env: generic param map. Returns [(name, type(subst), isref)]"""
    out = []
    for b in c.bases:
        try:
            bt = parse_type(b)
        except Exception:
            continue
        bc = find_cls(bt.name)
        if bc and bc.kind in ('class', 'struct'):
            benv = {p: subst(a, env) for p, a in zip(bc.tparams, bt.args)}
            out += all_fields(bc, benv)
    for n, ty, r in own_fields(c):
        out.append((n, subst(ty, env), r))
    return out


def is_unity_obj(c, depth=0):
    if c is None or depth > 12:
        return False
    for b in c.bases:
        bn = parse_type(b).name
        if bn in OBJ_ROOTS:
            return True
        if is_unity_obj(find_cls(bn), depth + 1):
            return True
    return False


# ---------------------------------------------------------------- reader
class Rd:
    def __init__(s, b, pos=0):
        s.b, s.p = b, pos

    def u(s, fmt):
        sz = struct.calcsize('<' + fmt)
        v = struct.unpack_from('<' + fmt, s.b, s.p)[0]
        s.p += sz
        return v

    def align(s):
        s.p = (s.p + 3) & ~3

    def string(s):
        n = s.u('i')
        v = s.b[s.p:s.p + n].decode('utf-8', 'replace')
        s.p += n
        s.align()
        return v


class Ptr:
    def __init__(s, f, p):
        s.f, s.p = f, p


class Ref:
    def __init__(s, rid):
        s.rid = rid


class Struct:
    def __init__(s, cls, items):
        s.cls, s.items = cls, items


class Reg:  # references registry collector
    pass


def read_value(rd, ty, isref, trace):
    if ty.arr:
        n = rd.u('i')
        if n < 0 or n > 100000:
            raise ValueError('bad array count %d at %d (%s)' % (n, rd.p, trace))
        et = ty.args[0]
        v = [read_value(rd, et, isref, trace + '[]') for _ in range(n)]
        rd.align()
        return v
    nm = ty.name
    if isref and nm not in ('List',):
        return Ref(rd.u('q'))
    if nm in PRIM:
        v = rd.u(PRIM[nm])
        if nm in ('bool', 'byte', 'sbyte', 'short', 'ushort', 'char', 'Boolean'):
            rd.align()
        return v
    if nm == 'string':
        return rd.string()
    if nm == 'List':
        n = rd.u('i')
        if n < 0 or n > 100000:
            raise ValueError('bad list count %d at %d (%s)' % (n, rd.p, trace))
        v = [read_value(rd, ty.args[0], isref, trace + '[]') for _ in range(n)]
        return v
    if nm in BUILTIN:
        return Struct(nm, [(k, rd.u('f')) for k in BUILTIN[nm]]) if nm not in ('Vector2Int', 'Vector3Int') else \
            Struct(nm, [(k, rd.u('i')) for k in BUILTIN[nm]])
    if nm == 'LayerMask':
        return Struct(nm, [('m_Bits', rd.u('I'))])
    c = find_cls(nm)
    if nm in OBJ_ROOTS and (c is None or nm == 'Object'):
        return Ptr(rd.u('i'), rd.u('q'))
    if c is None:
        raise ValueError('unknown type %s (%s)' % (nm, trace))
    if c.kind == 'enum':
        return rd.u('i')
    if c.kind in ('class', 'struct') and is_unity_obj(c) or nm in OBJ_ROOTS:
        f = rd.u('i')
        p = rd.u('q')
        return Ptr(f, p)
    env = dict(zip(c.tparams, ty.args))
    items = []
    for fn, ft, fr in all_fields(c, env):
        items.append((fn, read_value(rd, ft, fr, trace + '.' + fn)))
    rd.align()
    return Struct(nm, items)


def read_object(raw, scls, script_ns=None):
    """raw: MonoBehaviour bytes. returns (header dict, fields, registry list)"""
    rd = Rd(raw)
    rd.p = 12 + 4 + 12
    name = rd.string()
    c = scls
    items = []
    for fn, ft, fr in all_fields(c, {}):
        items.append((fn, read_value(rd, ft, fr, fn)))
    reg = None
    if rd.p < len(raw):
        ver = rd.u('i')
        reg = []
        if ver == 2:
            n = rd.u('i')
            for _ in range(n):
                rid = rd.u('q')
                cn, ns, asm = rd.string(), rd.string(), rd.string()
                data = None
                if cn:
                    rc = find_cls(cn, ns)
                    if rc is None:
                        raise ValueError('registry class not found %s.%s' % (ns, cn))
                    env = {}
                    data = Struct(cn, [(fn, read_value(rd, ft, fr, cn + '.' + fn)) for fn, ft, fr in all_fields(rc, env)])
                    rd.align()
                reg.append((rid, cn, ns, asm, data))
        else:
            raise ValueError('registry version %d' % ver)
    return name, items, reg, rd.p


# ---------------------------------------------------------------- yaml
def fnum(v):
    if isinstance(v, float):
        import math
        from decimal import Decimal
        if v != v or math.isinf(v):
            return {True: 'nan'}.get(v != v, 'Infinity' if v > 0 else '-Infinity')
        for p in range(1, 10):
            s = '%.*g' % (p, v)
            if struct.unpack('<f', struct.pack('<f', float(s)))[0] == v:
                break
        if abs(v) != 0 and (abs(v) < 1e-5 or abs(v) >= 1e15):
            m, e = ('%.*e' % (p - 1, v)).split('e')
            return m + 'e' + ('-' if e[0] == '-' else '+') + e[1:].lstrip('+-').rjust(2, '0')
        s = format(Decimal(s), 'f')
        if '.' in s:
            s = s.rstrip('0').rstrip('.')
        return s
    return str(v)


def yscalar(s):
    if s == '':
        return ''
    if re.fullmatch(r'[A-Za-z_][\w \-./()]*', s) and not s.endswith(' ') and s.lower() not in (
            'true', 'false', 'null', 'yes', 'no', 'on', 'off', 'y', 'n', '~'):
        return s
    if all(32 <= ord(c) < 127 for c in s):
        return "'" + s.replace("'", "''") + "'"
    return '"' + ''.join(c if 32 <= ord(c) < 127 and c not in '"\\' else
                         {'"': '\\"', '\\': '\\\\', '\n': '\\n'}.get(c, '\\u%04X' % ord(c) if ord(c) < 0x10000
                         else '\\U%08X' % ord(c)) for c in s) + '"'


class Ctx:
    def __init__(s, resolve):
        s.resolve = resolve


def ptr_yaml(p, ctx):
    if p.p == 0:
        return '{fileID: 0}'
    if p.f != 0:
        return '{fileID: %d, pathID: %d}  # UNRESOLVED external' % (p.f, p.p)
    return ctx.resolve(p.p)


def emit(key, v, ind, ctx, out):
    pad = ' ' * ind
    if isinstance(v, Ptr):
        out.append('%s%s: %s' % (pad, key, ptr_yaml(v, ctx)))
    elif isinstance(v, Ref):
        out.append('%s%s:' % (pad, key))
        out.append('%s  rid: %d' % (pad, v.rid))
    elif isinstance(v, Struct):
        if not v.items:
            out.append('%s%s: {}' % (pad, key))
        else:
            out.append('%s%s:' % (pad, key))
            for k, x in v.items:
                emit(k, x, ind + 2, ctx, out)
    elif isinstance(v, list):
        if not v:
            out.append('%s%s: []' % (pad, key))
        else:
            out.append('%s%s:' % (pad, key))
            for x in v:
                emit_item(x, ind, ctx, out)
    elif isinstance(v, bool):
        out.append('%s%s: %d' % (pad, key, int(v)))
    elif isinstance(v, str):
        out.append(('%s%s: %s' % (pad, key, yscalar(v))).rstrip() if v else '%s%s:' % (pad, key))
    else:
        out.append('%s%s: %s' % (pad, key, fnum(v)))


def emit_item(x, ind, ctx, out):
    pad = ' ' * ind
    tmp = []
    if isinstance(x, Struct):
        if not x.items:
            out.append(pad + '- {}')
            return
        emit(x.items[0][0], x.items[0][1], ind + 2, ctx, tmp)
        for k, y in x.items[1:]:
            emit(k, y, ind + 2, ctx, tmp)
        tmp[0] = pad + '- ' + tmp[0].lstrip()
        out += tmp
    elif isinstance(x, Ptr):
        out.append('%s- %s' % (pad, ptr_yaml(x, ctx)))
    elif isinstance(x, list):
        emit('', x, ind + 2, ctx, tmp)
        out.append(pad + '- ' + tmp[0].lstrip(': ').strip() if False else pad + '- []')
    elif isinstance(x, str):
        out.append('%s- %s' % (pad, yscalar(x)))
    elif isinstance(x, bool):
        out.append('%s- %d' % (pad, int(x)))
    else:
        out.append('%s- %s' % (pad, fnum(x)))


def to_yaml(items, reg, ctx):
    out = []
    for k, v in items:
        emit(k, v, 2, ctx, out)
    if reg is not None:
        out.append('  references:')
        out.append('    version: 2')
        out.append('    RefIds:')
        for rid, cn, ns, asm, data in reg:
            out.append('    - rid: %d' % rid)
            if not cn:
                out.append('      type: {class:, ns:, asm:}')
                out.append('      data: {}')
            else:
                out.append('      type: {class: %s, ns: %s, asm: %s}' % (cn, ns, asm))
                if not data.items:
                    out.append('      data: {}')
                else:
                    out.append('      data:')
                    for k, v in data.items:
                        emit(k, v, 8, ctx, out)
    return out


# ---------------------------------------------------------------- project glue
def asset_index():
    """name -> list of (path, guid, scriptguid)"""
    idx = collections.defaultdict(list)
    for p in glob.glob(PROJ + '/Assets/**/*.asset', recursive=True):
        idx[os.path.basename(p)[:-6]].append(p)
    return idx


def guid_of(path):
    m = re.search(r'guid: (\w+)', open(path + '.meta').read())
    return m.group(1)


def script_guid_to_cls():
    """m_Script guid -> class (via .cs.meta)"""
    g = {}
    for p in glob.glob(SCRIPTS + '/**/*.cs.meta', recursive=True):
        m = re.search(r'guid: (\w+)', open(p).read())
        if m:
            g[m.group(1)] = p[:-5]
    return g


def main():
    args = sys.argv[1:]
    write = '--write' in args
    names = [a for a in args if not a.startswith('--')]
    build_index()
    env = UnityPy.load(BUNDLE)
    byname, byid = {}, {}
    for o in env.objects:
        if o.assets_file.name != 'resources.assets' or o.type.name != 'MonoBehaviour':
            byid_key = (o.assets_file.name, o.path_id)
            byid[byid_key] = (o, None)
            continue
        raw = o.get_raw_data()
        try:
            l = struct.unpack_from('<i', raw, 28)[0]
            nm = raw[32:32 + l].decode()
        except Exception:
            nm = None
        byname.setdefault(nm, []).append(o)
        byid[(o.assets_file.name, o.path_id)] = (o, nm)
    aidx = asset_index()
    sg = script_guid_to_cls()

    def resolve(pid):
        o, nm = byid.get(('resources.assets', pid), (None, None))
        if o is None:
            return '{fileID: 0}  # UNRESOLVED pathID %d' % pid
        cands = aidx.get(nm, []) if nm else []
        if not cands:
            try:
                nm2 = o.read().m_Name if o.type.name != 'MonoBehaviour' else None
            except Exception:
                nm2 = None
            return '{fileID: 0}  # UNRESOLVED pathID %d (%s %s)' % (pid, o.type.name, nm2 or nm)
        # disambiguate by script pptr
        if len(cands) > 1:
            raw = o.get_raw_data()
            spid = struct.unpack_from('<q', raw, 20)[0]
            good = [c for c in cands]
            sel = None
            for c in good:
                sc = re.search(r'm_Script: .*guid: (\w+)', open(c).read(2000))
                if sc and sg.get(sc.group(1)) and unity_script_pid(env, sg[sc.group(1)], spid):
                    sel = c
                    break
            cands = [sel] if sel else cands
        return '{fileID: 11400000, guid: %s, type: 2}' % guid_of(cands[0])

    ctx = Ctx(resolve)
    for name in names:
        objs = byname.get(name)
        if not objs:
            print('NOT FOUND in bundle:', name)
            continue
        paths = aidx.get(name)
        if not paths or len(paths) != 1:
            print('asset file ambiguous/missing for', name, paths)
            continue
        apath = paths[0]
        text = open(apath).read()
        sm = re.search(r'm_Script: \{fileID: 11500000, guid: (\w+)', text)
        cspath = sg.get(sm.group(1))
        cname = os.path.basename(cspath)[:-3]
        c = find_cls(cname)
        o = objs[0]
        raw = o.get_raw_data()
        try:
            nm, items, reg, end = read_object(raw, c)
        except Exception as e:
            print('FAIL', name, cname, e)
            continue
        status = 'OK' if end == len(raw) else 'LEFTOVER %d/%d' % (end, len(raw))
        lines = to_yaml(items, reg, ctx)
        head = text.split('m_EditorClassIdentifier:')[0] + 'm_EditorClassIdentifier:\n'
        new = head + '\n'.join(lines) + '\n'
        print('%s [%s] %s consumed=%d/%d unresolved=%d' % (name, cname, status, end, len(raw), new.count('UNRESOLVED')))
        if '--dump' in args:
            print(new)
        if write and status == 'OK':
            bk = os.path.join(BACKUP, os.path.basename(apath))
            if not os.path.exists(bk):
                shutil.copy2(apath, bk)
            open(apath, 'w').write(new)
            print('  written', apath)


def unity_script_pid(env, cspath, pid):
    # script PPtr pathID within resources.assets -> MonoScript m_ClassName matches the .cs file name
    for o in env.objects:
        if o.path_id == pid and o.assets_file.name == 'resources.assets':
            try:
                return o.read().m_ClassName == os.path.basename(cspath)[:-3]
            except Exception:
                return False
    return False


if __name__ == '__main__':
    main()
