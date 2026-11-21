"""Print an .ann.c file without IL2CPP plumbing so the logic can be read.

usage: python -I slim_pseudoc.py <Class.ann.c> [FunctionNameSubstring]
Removes class-init and null-check blocks, generic-sharing noise (0xbd flag checks, rgctx helpers), write-barrier
epilogues, boxing helpers for string.Format arguments and blank lines. With a second argument prints only the
functions whose header line contains it.
"""
import re, sys
s = open(sys.argv[1], encoding='utf8').read()
if len(sys.argv) > 2:
    parts = re.split(r'(?m)^(?=// \S+\$\$)', s)
    s = ''.join(p for p in parts if p.startswith('//') and sys.argv[2] in p.split('\n', 1)[0])
s = re.sub(r'\n\s*if \(\*\(int \*\)\([^\n]*\+ 0x74\) == 0\) \{\n\s*il2cpp_class_init\(\);\n(\s*[^\n]*\n)?\s*\}', '', s)
s = re.sub(r'\n\s*if \([^\n]*== (?:\(int \*\))?0(?:x0)?\) \{\n\s*il2cpp_throw_null\(\);\n\s*\}', '', s)
s = re.sub(r'\n\s*if \(\*pcVar\d+ == .\\0.\) \{\n\s*\}', '', s)
s = re.sub(r'\n\s*if \(\*\(int \*\)\(_UNK_01a70f84[^\n]*\n(?:[^\n]*\n){0,8}?\s*\}\n', '\n', s)
s = re.sub(r'\n\s*if \(\(\*\(ushort \*\)\([^\n]*0xbd\) & 1\) == 0\) \{\n[^\n]*\n\s*\}', '', s)
out = []
for l in s.split('\n'):
    if re.search(r'il2cpp_helper_05640|func_0x01a05794|func_0x01a05624\(u|func_0x01a0563c\(', l):
        continue
    if l.strip() and not re.match(r'\s*(undefined\d* |u?int |float |char |bool |code |ulonglong |longlong |double )\**\w+(, \w+)*;$', l):
        out.append(l)
print('\n'.join(out))
