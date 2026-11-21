"""Annotate a cleaned pseudo-C file with names for IL2CPP metadata slots, writing <Class>.ann.c.

usage: python -I annotate_pseudoc.py <Class.clean.c ...>      (env WORK, GHIDRA; runs meta_slots.py)
`*(undefined4*)(&UNK_a + _UNK_b)` becomes `<Type or Method$... or string "...">`; a double deref becomes `<<...>>`.
Also drops the il2cpp_init_metadata prologue lines (`*pcVarN = 1`, repeated init calls) to shorten the output.
"""
import os, re, subprocess, sys
HERE = os.path.dirname(os.path.abspath(__file__))
for f in sys.argv[1:]:
    slots = {}
    out = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'meta_slots.py'), f], capture_output=True, text=True).stdout
    for l in out.splitlines():
        k, n = l.split(' ', 1); slots[k] = n
    t = open(f, encoding='utf8').read()
    # `*(T *)(&UNK_a + _UNK_b)` (one deref) and `**(T **)(...)` (two) for any int-like T
    t = re.sub(r'(\*+)\((?:undefined\d|u?int|u?short|code) \*+\)\(&UNK_([0-9a-f]{8}) \+ _UNK_([0-9a-f]{8})\)',
               lambda m: ('<<' if len(m.group(1)) > 1 else '<') + slots.get(m.group(2) + '_' + m.group(3), '?') + ('>>' if len(m.group(1)) > 1 else '>'), t)
    t = '\n'.join(l for l in t.split('\n') if 'il2cpp_init_metadata(<' not in l and not re.match(r'\s*(char \*pcVar\d+;|\*pcVar\d+ = .\\x01.;|pcVar\d+ = &UNK_)', l))
    open(f.replace('.clean.c', '.ann.c'), 'w', encoding='utf8').write(t)
