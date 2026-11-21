"""Unpack AG-packed normal maps exported by AssetRipper into standard RGB normal maps.

Mobile builds store normal maps packed (x in alpha, y in green, R=G=B). AssetRipper
writes them to PNG as-is, and Unity's NormalMap importer then treats RGB as xyz,
which gives noisy lighting. This rewrites such PNGs as R=x, G=y, B=z, A=255.

Usage: python -I fix_packed_normals.py <Assets> <backup_dir> [--apply]
Only textures imported as NormalMap (textureType: 1) whose RGB channels are gray
are touched. Needs Pillow. Not game specific.
"""
import os
import re
import shutil
import sys

from PIL import Image


def is_packed(im):
    small = im.convert('RGBA').resize((32, 32))
    px = list(small.getdata())
    gray = sum(1 for r, g, b, a in px if abs(r - g) < 4 and abs(g - b) < 4) / len(px)
    return gray > 0.95


def unpack(im):
    im = im.convert('RGBA')
    out = Image.new('RGBA', im.size)
    src = im.load()
    dst = out.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            _, g, _, a = src[x, y]
            nx = a / 255 * 2 - 1
            ny = g / 255 * 2 - 1
            nz = max(0.0, 1 - nx * nx - ny * ny) ** 0.5
            dst[x, y] = (a, g, round((nz + 1) / 2 * 255), 255)
    return out


def main():
    root, backup = sys.argv[1], sys.argv[2]
    apply = '--apply' in sys.argv
    for d, _, files in os.walk(root):
        for f in files:
            if not f.endswith('.png.meta'):
                continue
            meta = os.path.join(d, f)
            with open(meta, encoding='utf-8', errors='ignore') as h:
                if not re.search(r'^  textureType: 1$', h.read(), re.M):
                    continue
            png = meta[:-5]
            im = Image.open(png)
            if not is_packed(im):
                print('skip (not packed)', png)
                continue
            print('unpack', png)
            if apply:
                dest = os.path.join(backup, os.path.relpath(png, root))
                os.makedirs(os.path.dirname(dest), exist_ok=True)
                if not os.path.exists(dest):
                    shutil.copy2(png, dest)
                unpack(im).save(png)
    if not apply:
        print('dry run; add --apply to write')


if __name__ == '__main__':
    main()
