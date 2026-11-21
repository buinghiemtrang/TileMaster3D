# usage: name_c.py names.tsv veneers.tsv file.c...  -> writes file.named.c
import sys,re
names_tsv,ven,*files=sys.argv[1:]
by={}
for l in open(names_tsv,errors='replace'):
    p=l.rstrip('\n').split('\t')
    if len(p)==2: by.setdefault(int(p[0]),p[1])
# NOTE: helper addresses below are PixelFlow 0.37.1 (ARMv7). Re-derive them for each build (see ghidra/examples).
helpers={'0237fa00':'il2cpp_init_metadata','0237fcb0':'ThrowNullReferenceException','0237fca4':'il2cpp_object_new','0237f99c':'il2cpp_wbarrier'}
m={}
for l in open(ven):
    a,t=l.rstrip('\n').split('\t')
    if a in helpers: m[a]=helpers[a]; continue
    if t=='?': continue
    r=int(t,16)&0xffffffff
    m[a]=by.get(r,'native_%x'%r)
for a,v in helpers.items(): m.setdefault(a,v)
pat=re.compile(r'func_0x([0-9a-f]{8})')
for f in files:
    s=open(f).read()
    s=pat.sub(lambda x:m.get(x.group(1),x.group(0)).replace('$$','.'),s)
    open(f[:-2]+'.named.c','w').write(s)
print(len(m),'mapped', sum(not v.startswith('native_') for v in m.values()),'named')
