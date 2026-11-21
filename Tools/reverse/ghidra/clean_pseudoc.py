"""Strip IL2CPP boilerplate from Ghidra pseudo-C so the game logic is easier to read.

usage: python -I clean_pseudoc.py <in.c> [<out.c>]

Removes: lazy metadata init blocks (il2cpp_init_metadata), class static-constructor checks
(il2cpp_class_init), GC write barriers (il2cpp_wbarrier), null-check throws (il2cpp_throw_null) and
decompiler WARNING comments. If <in dir>/../veneer_names.json exists, remaining `FUN_xxxxxxxx` /
`thunk_FUN_xxxxxxxx` calls are replaced by the managed method they jump to.
Addresses in the output are Ghidra addresses (RVA + 0x10000).
"""
import json, os, re, sys

t = open(sys.argv[1], encoding='utf8').read()
t = re.sub(r"(?m)^[ \t]*/\* WARNING.*\n", "", t)
# lazy init of IL2CPP metadata: if (*flag == '\0') { il2cpp_init_metadata(..); ...; *flag = 1; }
t = re.sub(r"\n[ \t]*\w+ = \(char \*\)\([^\n]*\);\n[ \t]*if \(\*\w+ == '\\0'\) \{\n(?:[ \t]*il2cpp_init_metadata\([^\n]*\);\n)+[ \t]*\*\w+ = '\\x01';\n[ \t]*\}", "", t)
# class init: if (*(int *)(klass + 0x74) == 0) { il2cpp_class_init(); }   (the result may be assigned)
t = re.sub(r"\n[ \t]*if \(\*\(int \*\)\([^\n]*\+ 0x74\) == 0\) \{\n[ \t]*(?:\w+ = )?il2cpp_class_init\([^\n]*\);\n(?:[ \t]*\w+ = \w+;\n)?[ \t]*\}", "", t)
t = re.sub(r"\n[ \t]*if \(\w+ == 0\) \{\n[ \t]*il2cpp_throw_null\(\);\n[ \t]*\}", "", t)
t = re.sub(r"\n[ \t]*il2cpp_wbarrier\([^\n]*\);", "", t)
vn = os.path.join(os.path.dirname(os.path.abspath(sys.argv[1])), '..', 'veneer_names.json')
if os.path.exists(vn):
    names = json.load(open(vn))
    t = re.sub(r"(?:thunk_)?FUN_([0-9a-f]{8})", lambda m: names.get(m.group(1), m.group(0)).replace('.', '__'), t)
t = re.sub(r"\n{3,}", "\n\n", t)
open(sys.argv[2] if len(sys.argv) > 2 else sys.argv[1].replace('.c', '.clean.c'), 'w', encoding='utf8').write(t)
