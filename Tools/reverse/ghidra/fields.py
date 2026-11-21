"""Print instance field offsets of a class from dump.cs.   usage: python -I fields.py <dump.cs> <ClassName>   (Core.SandTerrain, or nested Core.SandTerrain.Stamp)"""
import re, sys
dump, want = sys.argv[1], sys.argv[2].split('.')[-1]
lines = open(dump, encoding='utf8', errors='replace').read().split('\n')
inside = False
for i, l in enumerate(lines):
    m = re.match(r'^(?:\[.*\]\s*)?[\w\s]*\b(?:class|struct)\s+([\w.<>`,]+)', l)
    if m and '// TypeDefIndex' in l:
        name = m.group(1)
        inside = name.split('.')[-1] == want and (name == want or name.endswith('.' + want))
        if inside: print('//', l.strip())
        continue
    if inside:
        if l.startswith('\t// Fields'): continue
        if l.startswith('\t// Methods') or l.startswith('\t// Properties'): break
        f = re.match(r'^\t(?:public|private|protected|internal)[\w\s]*?\s([\w<>\[\],.`]+)\s+(\w+); // (0x[0-9A-F]+)$', l)
        if f and 'static' not in l.split(f.group(1))[0] and 'const' not in l.split(f.group(1))[0]:
            print('%s +0x%x %s %s' % (f.group(3), int(f.group(3), 16) , f.group(1), f.group(2)))
