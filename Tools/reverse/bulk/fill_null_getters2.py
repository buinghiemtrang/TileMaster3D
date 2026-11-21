# Fill `X Name => null/default;` getters whose native body is `ldr r0,[r0,#off]; bx lr` (verified ARM),
# ARMv7 only (reads 32-bit ELF, matches ARM32 instructions). Env: WORK, REPO, ABI (default armeabi-v7a).
# mapping #off to the instance field at that offset in dump.cs. usage: fill_null_getters2.py [--write]
import re,sys,json,struct,glob,os,shutil
W=os.environ['WORK'].rstrip('/')+'/'
P=os.environ['REPO'].rstrip('/')+'/Assets/Scripts/'
WRITE='--write' in sys.argv
so=open(W+'apk/base/lib/'+os.environ.get('ABI','armeabi-v7a')+'/libil2cpp.so','rb').read()
phoff=struct.unpack_from('<I',so,0x1c)[0]; phnum=struct.unpack_from('<H',so,0x2c)[0]
segs=[]
for i in range(phnum):
    t,off,va,pa,fs,ms,fl,al=struct.unpack_from('<8I',so,phoff+i*32)
    if t==1: segs.append((va,off,fs))
def rd(va):
    for v,o,fs in segs:
        if v<=va<v+fs: return struct.unpack_from('<I',so,o+va-v)[0]
addr={}
for m in json.load(open(W+'il2cpp/script.json'))['ScriptMethod']:
    addr[m['Name']]=m['Address']
dump=open(W+'il2cpp/dump.cs',errors='ignore').read()
# class -> {offset: fieldname}
cls_fields={}
for m in re.finditer(r'\n// Namespace: ([^\n]*)\n(?:\[[^\n]*\]\n)*[^\n]*?(?:class|struct) (\w+)[^\n]*\n\{(.*?)\n\}',dump,re.S):
    ns,cn,body=m.groups()
    full=(ns+'.' if ns else '')+cn
    offs={}
    for f in re.finditer(r'\n\t(?:private|protected|public|internal)[^\n(]*? (\w+); // 0x([0-9A-F]+)',body):
        if ' static ' in f.group(0) or ' const ' in f.group(0): continue
        offs[int(f.group(2),16)]=f.group(1)
    cls_fields.setdefault(full,offs)
getter=re.compile(r'(public|protected|internal|private)( override| virtual| new)? ([\w<>\[\], .]+?) (\w+) => (null|default);')
filled=[];skipped=[]
for f in glob.glob(P+'**/*.cs',recursive=True):
    s=open(f,errors='ignore').read()
    if '=> null;' not in s and '=> default;' not in s: continue
    ns=re.search(r'namespace ([\w.]+)',s); ns=ns.group(1) if ns else ''
    changed=False
    def rep(m):
        global changed
        name=m.group(4)
        # find enclosing class name: last 'class X' before position
        pre=s[:m.start()]
        cm=list(re.finditer(r'(?:class|struct) (\w+)',pre))
        if not cm: return m.group(0)
        cn=cm[-1].group(1); full=(ns+'.' if ns else '')+cn
        key=f'{full}$$get_{name}'
        a=addr.get(key)
        if a is None: skipped.append(key+' noaddr'); return m.group(0)
        i0=rd(a); i1=rd(a+4)
        # ldr r0,[r0,#imm12]: 0xE590 0xxx  with Rn=0 Rt=0 ; bx lr = 0xE12FFF1E
        if i0 is None or (i0 & 0xFFFFF000)!=0xE5900000 or i1!=0xE12FFF1E:
            skipped.append(key+' notsimple'); return m.group(0)
        off=i0&0xFFF
        fld=cls_fields.get(full,{}).get(off)
        if not fld or not re.search(r'\b'+re.escape(fld)+r';',s):
            skipped.append(f'{key} off={off:#x} fld={fld}'); return m.group(0)
        filled.append(f'{key} -> {fld}')
        return m.group(0).replace(f'=> {m.group(5)};',f'=> {fld};')
    s2=getter.sub(rep,s)
    if s2!=s and WRITE:
        b=W+'backup/null-getters2/'+os.path.relpath(f,P); os.makedirs(os.path.dirname(b),exist_ok=True); shutil.copy(f,b); open(f,'w').write(s2)
open(W+'reports/null-getters2.txt','w').write('FILLED\n'+'\n'.join(filled)+'\nSKIPPED\n'+'\n'.join(skipped)+'\n')
print('filled',len(filled),'skipped',len(skipped))
