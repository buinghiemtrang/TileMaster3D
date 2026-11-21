"""Extract compiled shaders (GLSL source + render state) from the game's Unity files.

usage: python -I extract_shaders.py <apk_dir> <out_dir> [--all] [--only NAME ...]

Needs UnityPy (pip install UnityPy). The Android build only ships GLES3 programs, which Unity
stores as GLSL source text, LZ4-compressed inside Shader.compressedBlob.

For every shader writes <out_dir>/<Shader_Name>/:
  shader.txt        ShaderLab skeleton: Properties, SubShader/Pass tags, render state, variant list
  state.json        the parsed form as read from the asset (for anything shader.txt leaves out)
  pN_<pass>/vNNN.glsl   one file per compiled variant, keywords in the header comment
Engine shaders (URP, TMP, built-in, Hidden/*) are skipped unless --all is given.
"""
import os, re, sys, json, struct, hashlib
import lz4.block
import UnityPy

UnityPy.config.FALLBACK_UNITY_VERSION = os.environ.get('UNITY_VERSION', '6000.0.62f1')  # set UNITY_VERSION for another game

ENGINE = re.compile(r'^(Hidden/|Universal Render Pipeline/|Sprites/|Skybox/|Legacy Shaders/|Particles/Standard'
                    r'|UI/Default$|TextMeshPro/(Distance Field|Mobile/Distance Field|Sprite)$)')

BLEND = ['Zero', 'One', 'DstColor', 'SrcColor', 'OneMinusDstColor', 'SrcAlpha', 'OneMinusSrcColor',
         'DstAlpha', 'OneMinusDstAlpha', 'SrcAlphaSaturate', 'OneMinusSrcAlpha']
BLENDOP = ['Add', 'Sub', 'RevSub', 'Min', 'Max']
CMP = ['Disabled', 'Never', 'Less', 'Equal', 'LEqual', 'Greater', 'NotEqual', 'GEqual', 'Always']
CULL = ['Off', 'Front', 'Back']
STENCILOP = ['Keep', 'Zero', 'Replace', 'IncrSat', 'DecrSat', 'Invert', 'IncrWrap', 'DecrWrap']
PROPTYPE = ['Color', 'Vector', 'Float', 'Range', '2D', 'Int']
PROPFLAGS = [(1, 'HideInInspector'), (2, 'PerRendererData'), (4, 'NoScaleOffset'), (8, 'Normal'),
             (16, 'HDR'), (32, 'Gamma'), (64, 'NonModifiableTextureData'), (128, 'MainTexture'), (256, 'MainColor')]
TEXDIM = {1: 'Any', 2: '2D', 3: '3D', 4: 'Cube', 5: '2DArray', 6: 'CubeArray'}


def unity_files(root):
    for d, _, fs in os.walk(root):
        for f in sorted(fs):
            if f.endswith('.bundle') or f.endswith('.assets') or f.endswith('.split0') \
                    or f in ('globalgamemanagers', 'level0') or re.fullmatch(r'[0-9a-f]{32}', f):
                yield os.path.join(d, f)


def val(v, table=None):
    """A ShaderLab state value: either a constant or a [_Property] reference."""
    if v['name'] != '<noninit>':
        return '[' + v['name'] + ']'
    x = v['val']
    if table is not None:
        i = int(x)
        return table[i] if 0 <= i < len(table) else str(x)
    return ('%g' % x)


def fmt_state(s):
    out = []
    b = s['rtBlend0']
    src, dst = val(b['srcBlend'], BLEND), val(b['destBlend'], BLEND)
    srca, dsta = val(b['srcBlendAlpha'], BLEND), val(b['destBlendAlpha'], BLEND)
    if (src, dst) != ('One', 'Zero') or (srca, dsta) != ('One', 'Zero'):
        out.append(f'Blend {src} {dst}' + ('' if (srca, dsta) == (src, dst) else f', {srca} {dsta}'))
    op, opa = val(b['blendOp'], BLENDOP), val(b['blendOpAlpha'], BLENDOP)
    if op != 'Add' or opa != 'Add':
        out.append(f'BlendOp {op}' + ('' if opa == op else f', {opa}'))
    cm = b['colMask']
    if cm['name'] != '<noninit>' or int(cm['val']) != 15:
        m = int(cm['val'])
        out.append('ColorMask ' + (val(cm) if cm['name'] != '<noninit>'
                                   else (''.join(c for c, bit in zip('RGBA', (8, 4, 2, 1)) if m & bit) or '0')))
    out.append('ZWrite ' + (val(s['zWrite']) if s['zWrite']['name'] != '<noninit>'
                            else ('On' if s['zWrite']['val'] else 'Off')))
    out.append('ZTest ' + val(s['zTest'], CMP))
    out.append('Cull ' + val(s['culling'], CULL))
    if s['offsetFactor']['val'] or s['offsetUnits']['val'] or s['offsetFactor']['name'] != '<noninit>':
        out.append(f"Offset {val(s['offsetFactor'])}, {val(s['offsetUnits'])}")
    if s['alphaToMask']['val'] or s['alphaToMask']['name'] != '<noninit>':
        out.append('AlphaToMask ' + val(s['alphaToMask']))
    ref, op = s['stencilRef'], s['stencilOp']
    stencil_used = ref['name'] != '<noninit>' or ref['val'] or op['comp']['name'] != '<noninit>' \
        or int(op['comp']['val']) not in (0, 8) or int(op['pass']['val']) != 0 or op['pass']['name'] != '<noninit>'
    if stencil_used:
        out.append('Stencil {')
        out.append(f"    Ref {val(ref)}  ReadMask {val(s['stencilReadMask'])}  WriteMask {val(s['stencilWriteMask'])}")
        out.append(f"    Comp {val(op['comp'], CMP)}  Pass {val(op['pass'], STENCILOP)}  "
                   f"Fail {val(op['fail'], STENCILOP)}  ZFail {val(op['zFail'], STENCILOP)}")
        out.append('}')
    return out


def tags(t):
    return ' '.join(f'"{k}"="{v}"' for k, v in t['tags'])


def fmt_prop(p):
    attrs = ''.join(f'[{n}] ' for bit, n in PROPFLAGS if p['m_Flags'] & bit)
    attrs += ''.join(f'[{a}] ' for a in p['m_Attributes'])
    d = [p[f'm_DefValue[{i}]'] for i in range(4)]
    ty = p['m_Type']
    if ty == 3:
        kind, default = f'Range({d[1]:g}, {d[2]:g})', f'{d[0]:g}'
    elif ty == 4:
        kind = TEXDIM.get(p['m_DefTexture']['m_TexDim'], '2D')
        default = f'"{p["m_DefTexture"]["m_DefaultName"]}" {{}}'
    elif ty in (0, 1):
        kind, default = PROPTYPE[ty], '(' + ','.join(f'{x:g}' for x in d) + ')'
    else:
        kind, default = PROPTYPE[ty] if ty < len(PROPTYPE) else str(ty), f'{d[0]:g}'
    return f'{attrs}{p["m_Name"]} ("{p["m_Description"]}", {kind}) = {default}'


def blob_entries(t):
    """Decompress the GLES3 blob and return its entries as raw bytes, indexed like m_BlobIndex."""
    pi = 0  # only one platform (GLES3x = 9) in this build
    blob = bytes(t['compressedBlob'])
    segs = []
    for off, cl, dl in zip(t['offsets'][pi], t['compressedLengths'][pi], t['decompressedLengths'][pi]):
        segs.append(lz4.block.decompress(blob[off:off + cl], uncompressed_size=dl))
    head = segs[0]
    n = struct.unpack_from('<i', head, 0)[0]
    entries = []
    for i in range(n):
        off, ln, seg = struct.unpack_from('<iii', head, 4 + 12 * i)
        entries.append(segs[seg][off:off + ln])
    return entries


def glsl_of(entry):
    for marker in (b'#ifdef VERTEX', b'#version'):
        i = entry.find(marker)
        if i >= 4:
            ln = struct.unpack_from('<i', entry, i - 4)[0]
            if 0 < ln <= len(entry) - i:
                return entry[i:i + ln].decode('utf8', errors='replace')
    return None


def safe(name):
    return re.sub(r'[^\w.-]+', '_', name).strip('_') or 'unnamed'


def dump_shader(t, out_root, src):
    pf = t['m_ParsedForm']
    name = pf['m_Name']
    entries = blob_entries(t)
    kwnames = pf.get('m_KeywordNames', [])
    d = os.path.join(out_root, safe(name))
    os.makedirs(d, exist_ok=True)

    lines = [f'// source: {src}', f'Shader "{name}" {{', '    Properties {']
    lines += ['        ' + fmt_prop(p) for p in pf['m_PropInfo']['m_Props']]
    lines.append('    }')
    nvar = 0
    for si, ss in enumerate(pf['m_SubShaders']):
        lines.append(f'    SubShader {{  // #{si}')
        if ss['m_Tags']['tags']: lines.append(f'        Tags {{ {tags(ss["m_Tags"])} }}')
        if ss.get('m_LOD'): lines.append(f'        LOD {ss["m_LOD"]}')
        for pi, p in enumerate(ss['m_Passes']):
            st = p['m_State']
            if p['m_Type'] == 1:
                lines.append(f'        UsePass "{p["m_UseName"]}"')
                continue
            if p['m_Type'] == 2:
                lines.append(f'        GrabPass {{ "{p["m_TextureName"]}" }}')
                continue
            pname = st['m_Name']
            lines.append(f'        Pass {{  // #{pi}')
            if pname: lines.append(f'            Name "{pname}"')
            if st['m_Tags']['tags']: lines.append(f'            Tags {{ {tags(st["m_Tags"])} }}')
            lines += ['            ' + x for x in fmt_state(st)]
            # one GLES3 program holds both stages; group subprograms by blob index
            progs = {}
            for stage in ('progVertex', 'progFragment', 'progGeometry', 'progHull', 'progDomain'):
                sp = p.get(stage)
                if not sp: continue
                for tier in sp.get('m_PlayerSubPrograms', []) + [sp.get('m_SubPrograms', [])]:
                    for sub in tier:
                        kws = [kwnames[k] if k < len(kwnames) else f'#{k}' for k in sub.get('m_KeywordIndices', [])]
                        kws += [f'local#{k}' for k in sub.get('m_LocalKeywordIndices', [])]
                        progs.setdefault(sub['m_BlobIndex'], set()).add(' '.join(sorted(kws)))
            pdir = os.path.join(d, f'p{si}.{pi}_{safe(pname) if pname else "pass"}')
            os.makedirs(pdir, exist_ok=True)
            seen = {}
            for k, (bi, kwsets) in enumerate(sorted(progs.items())):
                code = glsl_of(entries[bi]) if bi < len(entries) else None
                kwtxt = ' | '.join(sorted(kwsets)) or '<none>'
                if code is None:
                    lines.append(f'            // v{k:03}: blob {bi} has no GLSL  [{kwtxt}]')
                    continue
                h = hashlib.sha1(code.encode()).hexdigest()
                if h in seen:
                    lines.append(f'            // v{k:03}: same as {seen[h]}  [{kwtxt}]')
                    continue
                fn = f'v{k:03}.glsl'
                seen[h] = fn
                with open(os.path.join(pdir, fn), 'w', encoding='utf8') as f:
                    f.write(f'// {name} / subshader {si} pass {pi} {pname}\n// keywords: {kwtxt}\n\n{code}')
                lines.append(f'            // v{k:03}: [{kwtxt}]')
                nvar += 1
            lines.append('        }')
        lines.append('    }')
    if pf.get('m_FallbackName'): lines.append(f'    Fallback "{pf["m_FallbackName"]}"')
    if pf.get('m_CustomEditorName'): lines.append(f'    CustomEditor "{pf["m_CustomEditorName"]}"')
    lines.append('}')
    with open(os.path.join(d, 'shader.txt'), 'w', encoding='utf8') as f:
        f.write('\n'.join(lines) + '\n')
    pf_json = dict(pf)
    with open(os.path.join(d, 'state.json'), 'w', encoding='utf8') as f:
        json.dump(pf_json, f, indent=1, default=str)
    return d, nvar


def main():
    apk, out = sys.argv[1], sys.argv[2]
    want_all = '--all' in sys.argv
    only = set(sys.argv[sys.argv.index('--only') + 1:]) if '--only' in sys.argv else None
    done = {}  # name -> blob hash, to skip identical copies in several bundles
    for path in unity_files(apk):
        try:
            env = UnityPy.load(path)
        except Exception as e:
            print('skip', path, e); continue
        for obj in env.objects:
            if obj.type.name != 'Shader': continue
            t = obj.read_typetree()
            name = t['m_ParsedForm']['m_Name']
            if only is not None and name not in only: continue
            if only is None and not want_all and ENGINE.match(name): continue
            h = hashlib.sha1(bytes(t['compressedBlob'])).hexdigest()
            if name in done and h in done[name]: continue
            outname = name if name not in done else f'{name}__{len(done[name]) + 1}'
            done.setdefault(name, set()).add(h)
            t['m_ParsedForm']['m_Name'] = outname
            d, n = dump_shader(t, out, os.path.relpath(path, apk))
            t['m_ParsedForm']['m_Name'] = name
            print(f'{n:5} variants  {outname}')


if __name__ == '__main__':
    main()
