# litann.py <annotated listing>: resolve PC-relative literal loads in an annotate_listing.py output.
# Follows 'ldr rX,[0xLIT]' + 'ldr rY,[pc,rX]' / 'add rY,pc,rX' and prints the string / TypeInfo / method behind the slot
# (the .so is unrelocated on disk, so the slot holds the raw usage RVA that script.json maps).
# Paths come from env WORK (default ~/Desktop/royal-smash-work) and ABI (default armeabi-v7a). Not game specific.
# Annotate an annotate_listing output with PC-relative literal loads (ldr rX,[0xLIT] ; ldr rY,[pc,rX]).
import json,os,re,struct,sys
work=os.environ.get('WORK',os.path.expanduser('~/Desktop/royal-smash-work'))
so=os.path.join(work,'apk','lib','lib',os.environ.get('ABI','armeabi-v7a'),'libil2cpp.so')
d=open(so,'rb').read()
phoff=struct.unpack_from('<I',d,0x1c)[0]; n=struct.unpack_from('<H',d,0x2c)[0]
segs=[struct.unpack_from('<6I',d,phoff+i*32) for i in range(n)]
segs=[(va,off,fs) for t,off,va,pa,fs,ms in segs if t==1]
def word(rva):
    for va,off,fs in segs:
        if va<=rva<va+fs: return struct.unpack_from('<I',d,off+rva-va)[0]
    return None
sj=json.load(open(os.path.join(work,'il2cpp','script.json'),encoding='utf-8'))
names={m['Address']:'str '+json.dumps(m['Value'],ensure_ascii=False) for m in sj['ScriptString']}
for k in ('ScriptMetadata','ScriptMetadataMethod'):
    for m in sj[k]: names[m['Address']]=m['Name']
lastlit={}
for line in open(sys.argv[1],encoding='utf-8',errors='replace'):
    line=line.rstrip('\n')
    m=re.match(r'\s*([0-9a-f]{7,8})\s+(\S+)\s+(.*)',line)
    note=''
    if m:
        addr=int(m.group(1),16); op=m.group(2); args=m.group(3)
        mm=re.match(r'(r\d+|lr|r1[0-2]),\[0x([0-9a-f]+)\]',args)
        if op=='ldr' and mm: lastlit[mm.group(1)]=int(mm.group(2),16)
        mm=re.match(r'(r\d+),\[pc,(r\d+)\]',args)
        if op=='ldr' and mm and mm.group(2) in lastlit:
            lit=lastlit[mm.group(2)]-0x10000; w=word(lit)
            if w is not None:
                slot=(addr-0x10000+8+w)&0xffffffff; v=word(slot)
                note='  ; '+names.get(slot, names.get(v,'?slot %x val %x'%(slot,v if v is not None else 0)))
        mm=re.match(r'(r\d+),pc,(r\d+)',args)
        if op=='add' and mm and mm.group(2) in lastlit:
            lit=lastlit[mm.group(2)]-0x10000; w=word(lit)
            if w is not None:
                slot=(addr-0x10000+8+w)&0xffffffff
                note='  ; &'+names.get(slot,'slot %x'%slot)
    print(line+note)
