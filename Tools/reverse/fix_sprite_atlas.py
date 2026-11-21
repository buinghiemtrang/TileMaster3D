"""Re-bind sprites that AssetRipper exported without their sprite atlas texture.

usage: python -I fix_sprite_atlas.py <apk_dir> <project> [--apply]

In the build, sprites packed in a SpriteAtlas keep `m_RD.texture: {fileID: 0}`; the engine binds
them at runtime from the atlas' m_RenderDataMap. AssetRipper exports the atlas textures
(sactx-*.png) but not the SpriteAtlas objects, so in the editor those sprites draw as white quads.
This reads every SpriteAtlas from the game files (needs UnityPy) and writes its render data into
each matching sprite's m_RD: texture (the exported sactx png), textureRect, textureRectOffset,
atlasRectOffset, settingsRaw, uvTransform and downscaleMultiplier. Sprites are matched through
m_RenderDataKey. Default is a dry run; --apply writes the files.
"""
import os, re, sys, glob
import UnityPy

UnityPy.config.FALLBACK_UNITY_VERSION = os.environ.get('UNITY_VERSION', '6000.0.62f1')  # set UNITY_VERSION for another game

apk, proj = sys.argv[1], sys.argv[2]
apply = '--apply' in sys.argv


def guid_str(k):
    # Unity prints each 32-bit word of a GUID least significant nibble first
    return ''.join(''.join('0123456789abcdef'[(k[f'data[{i}]'] >> (4 * j)) & 15] for j in range(8)) for i in range(4))


def unity_files(root):
    for d, _, fs in os.walk(root):
        for f in fs:
            if f.endswith(('.bundle', '.assets', '.split0')) or re.fullmatch(r'[0-9a-f]{32}', f):
                yield os.path.join(d, f)


# (guid, fileID) -> render data with the atlas texture name
render = {}
for path in unity_files(apk):
    try:
        env = UnityPy.load(path)
    except Exception:
        continue
    for obj in env.objects:
        if obj.type.name != 'SpriteAtlas':
            continue
        t = obj.read_typetree()
        files = obj.assets_file.objects
        for key, data in t['m_RenderDataMap']:
            tex = data['texture']
            if tex['m_FileID'] != 0 or tex['m_PathID'] not in files:
                continue
            data = dict(data)
            data['textureName'] = files[tex['m_PathID']].read_typetree()['m_Name']
            data['atlas'] = t['m_Name']
            render[(guid_str(key[0]), key[1])] = data
print(f'atlas render data: {len(render)} sprites')

# exported atlas textures: name -> guid
tex_guid = {}
for meta in sorted(glob.glob(os.path.join(proj, 'Assets', '**', 'sactx-*.png.meta'), recursive=True)):
    name = os.path.basename(meta)[:-len('.png.meta')]
    g = re.search(r'^guid:\s*([0-9a-f]{32})', open(meta).read(), re.M)
    if g:
        tex_guid.setdefault(name, g.group(1))


def fmt(v):
    return ('%.9g' % v) if isinstance(v, float) else str(v)


def patch(text, d, guid):
    """Rewrite the render-data fields inside the m_RD block (up to m_AtlasRD)."""
    start = text.index('\n  m_RD:\n')
    end = text.index('\n  m_AtlasRD:', start)
    rd = text[start:end]
    r = d['textureRect']
    rd = re.sub(r'(\n    texture: )\{fileID: 0\}', rf'\1{{fileID: 2800000, guid: {guid}, type: 3}}', rd, count=1)
    rd = re.sub(r'(\n    textureRect:\n      serializedVersion: 2\n)      x: .*\n      y: .*\n      width: .*\n      height: .*',
                lambda m: m.group(1) + f'      x: {fmt(r["x"])}\n      y: {fmt(r["y"])}\n      width: {fmt(r["width"])}\n      height: {fmt(r["height"])}', rd, count=1)
    o, a, u = d['textureRectOffset'], d['atlasRectOffset'], d['uvTransform']
    # The atlas stores the trim offset relative to the SOURCE texture; for sprites cut from a larger texture it equals the sprite's own
    # m_Rect origin, and Unity's sliced Image then reads it as padding (drawn shifted/shrunk). Inside m_Rect the offset is 0.
    mr = re.search(r'\n  m_Rect:\n    serializedVersion: 2\n    x: (\S+)\n    y: (\S+)\n', text)
    if mr and abs(o['x'] - float(mr.group(1))) < 0.01 and abs(o['y'] - float(mr.group(2))) < 0.01:
        o = {'x': 0, 'y': 0}
    rd = re.sub(r'\n    textureRectOffset: .*', f'\n    textureRectOffset: {{x: {fmt(o["x"])}, y: {fmt(o["y"])}}}', rd, count=1)
    rd = re.sub(r'\n    atlasRectOffset: .*', f'\n    atlasRectOffset: {{x: {fmt(a["x"])}, y: {fmt(a["y"])}}}', rd, count=1)
    rd = re.sub(r'\n    settingsRaw: .*', f'\n    settingsRaw: {d["settingsRaw"]}', rd, count=1)
    rd = re.sub(r'\n    uvTransform: .*', f'\n    uvTransform: {{x: {fmt(u["x"])}, y: {fmt(u["y"])}, z: {fmt(u["z"])}, w: {fmt(u["w"])}}}', rd, count=1)
    rd = re.sub(r'\n    downscaleMultiplier: .*', f'\n    downscaleMultiplier: {fmt(d["downscaleMultiplier"])}', rd, count=1)
    return text[:start] + rd + text[end:]


key_re = re.compile(r'm_RenderDataKey:\n\s+([0-9a-f]{32}): (-?\d+)')
fixed, unbound, no_tex, by_atlas = 0, 0, set(), {}
for f in glob.glob(os.path.join(proj, 'Assets', '**', '*.asset'), recursive=True):
    raw = open(f, 'rb').read()
    text = raw.decode('utf8', errors='surrogateescape')
    if '\nSprite:\n' not in text or '\n  m_RD:\n' not in text:
        continue
    m = key_re.search(text)
    rd_tex = re.search(r'\n  m_RD:\n    serializedVersion: \d+\n    texture: \{fileID: 0\}', text)
    if not m or not rd_tex:
        continue
    d = render.get((m.group(1), int(m.group(2))))
    if d is None:
        unbound += 1
        continue
    guid = tex_guid.get(d['textureName'])
    if guid is None:
        no_tex.add(d['textureName'])
        continue
    by_atlas[d['atlas']] = by_atlas.get(d['atlas'], 0) + 1
    fixed += 1
    if apply:
        open(f, 'wb').write(patch(text, d, guid).encode('utf8', errors='surrogateescape'))

print(f'sprites re-bound: {fixed} {by_atlas}')
print(f'sprites without texture and not in any atlas: {unbound}')
for n in sorted(no_tex):
    print('  atlas texture not exported:', n)
if not apply:
    print('dry run; add --apply to write')
