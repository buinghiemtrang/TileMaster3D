"""Print selected functions from a Ghidra pseudo-C file.

usage: python -I show_function.py <Class.clean.c> <method> [<method> ...]
(method = the part after `$$` in the header comment, e.g. BuildMesh, .ctor, OnEnable)
"""
import re,sys
t=open(sys.argv[1]).read()
parts=re.split(r'(?m)^(// \S+\$\$\S+\s+RVA: 0x[0-9a-f]+)\n',t)
d={parts[i].split('$$')[1].split()[0]:parts[i]+'\n'+parts[i+1] for i in range(1,len(parts),2)}
for k in sys.argv[2:]:
    s=re.sub(r'(?m)^\s*/\* WARNING.*\n','',d[k]); s=re.sub(r'\n{2,}','\n',s); print(s)
