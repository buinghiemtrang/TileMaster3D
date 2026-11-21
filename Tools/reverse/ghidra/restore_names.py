"""Rename pseudo-C files between the Windows-safe spelling and the real class names.

Windows forbids  < > | : ? * "  in file names, but IL2CPP names use them (`Core.Obstacle.<>c__DisplayClass134_0`,
`<Foo>d__12`, `<<Bar>g__Run|1>d`). DecompileBulk.java (and the handoff zip made on Windows) therefore stores them as
the full-width twins  ＜ ＞ ｜ ： ？ ＊ ＂ . The scripts that expect the real names (name_funcs.sh, show_function.py,
decompile_classes.sh, `startswith(w + '<')` in name_funcs.py ...) only work on the real names, so on Linux run:

    python3 -I restore_names.py <dir> [--to-windows]

  default       full-width -> real characters   (use on Ubuntu after copying the Windows pseudoc/)
  --to-windows  real -> full-width              (use before zipping on Ubuntu for Windows)
Only touches *.c files directly inside <dir>. Prints a count. Skips (and reports) a target that already exists.
Does not depend on the game.
"""
import os, sys

PAIRS = {'＜': '<', '＞': '>', '｜': '|', '：': ':', '？': '?', '＊': '*', '＂': '"'}
d = sys.argv[1]
to_win = '--to-windows' in sys.argv
if os.name == 'nt' and not to_win:
    sys.exit('Windows cannot hold < > | : ? * " in file names: run this on Linux (or with --to-windows).')
table = str.maketrans({v: k for k, v in PAIRS.items()} if to_win else PAIRS)
done = clash = 0
for name in os.listdir(d):
    if not name.endswith('.c'):
        continue
    new = name.translate(table)
    if new == name:
        continue
    src, dst = os.path.join(d, name), os.path.join(d, new)
    if os.path.exists(dst):
        clash += 1
        print('exists, skipped:', new)
        continue
    os.rename(src, dst)
    done += 1
print('renamed', done, 'files;', clash, 'skipped')
