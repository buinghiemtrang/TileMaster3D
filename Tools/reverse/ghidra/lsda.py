# lsda.py <so> <rvaHex> [...]: print the C++ exception call-site table (try ranges -> landing pads) of the function containing each RVA.
# ARM EHABI: .ARM.exidx -> .ARM.extab -> generic model (personality, unwind words, LSDA). Not game specific.
import struct,sys
d=open(sys.argv[1],'rb').read()
shoff=struct.unpack_from('<I',d,0x20)[0]; shentsize,shnum,shstrndx=struct.unpack_from('<HHH',d,0x2e)
secs={}
raw=[struct.unpack_from('<10I',d,shoff+i*shentsize) for i in range(shnum)]
stroff=raw[shstrndx][4]
for r in raw:
    e=d.index(b'\0',stroff+r[0]); secs[d[stroff+r[0]:e].decode()]=r
ex=secs['.ARM.exidx']; exaddr,exoff,exsize=ex[3],ex[4],ex[5]
def w(a):
    return struct.unpack_from('<I',d,exoff+a-exaddr)[0] if exaddr<=a<exaddr+exsize else None
def rd_addr(a):
    for r in raw:
        if r[3]<=a<r[3]+r[5] and r[1]!=8: return struct.unpack_from('<I',d,r[4]+a-r[3])[0]
def rdb(a,n):
    for r in raw:
        if r[3]<=a<r[3]+r[5] and r[1]!=8: return d[r[4]+a-r[3]:r[4]+a-r[3]+n]
def prel31(a):
    v=rd_addr(a)&0x7fffffff
    if v&0x40000000: v-=0x80000000
    return (a+v)&0xffffffff
n=exsize//8
ents=[(prel31(exaddr+i*8),exaddr+i*8) for i in range(n)]
def uleb(b,i):
    r=0;s=0
    while True:
        c=b[i];i+=1;r|=(c&0x7f)<<s;s+=7
        if not c&0x80:return r,i
def sleb(b,i):
    r=0;s=0
    while True:
        c=b[i];i+=1;r|=(c&0x7f)<<s;s+=7
        if not c&0x80:
            if c&0x40: r-=1<<s
            return r,i
for arg in sys.argv[2:]:
    t=int(arg,16)
    lo=0;hi=n-1;idx=-1
    while lo<=hi:
        m=(lo+hi)//2
        if ents[m][0]<=t: idx=m;lo=m+1
        else: hi=m-1
    fn,ea=ents[idx]; nxt=ents[idx+1][0] if idx+1<n else fn
    data=rd_addr(ea+4)
    print('func %x-%x exidx data=%08x'%(fn,nxt,data))
    if data==1: print(' cantunwind'); continue
    if data&0x80000000: print(' inline compact (no LSDA)'); continue
    x=prel31(ea+4); first=rd_addr(x)
    if first&0x80000000:
        idxp=(first>>24)&0xf
        cnt=(first>>16)&0xff if idxp in(1,2) else 0
        lsda=x+4+4*cnt if idxp in (1,2) else x+4
        # compact pr1/pr2: descriptors follow opcodes; fall through to scan for gxx data below
        print(' compact personality %d, extra words %d'%(idxp,cnt))
        p=x+4+4*cnt
        # descriptor list: ( encoded landing, lsda...) -- try to find the LSDA as the word after
        print(' words:',[hex(rd_addr(p+4*k)) for k in range(6)])
        continue
    pers=prel31(x); cnt=(rd_addr(x+4)>>24)&0xff
    p=x+8+4*cnt
    print(' personality %x, unwind words %d, lsda at %x'%(pers,cnt,p))
    # Large async methods can have a call-site table longer than 512 bytes.
    # Read the containing section remainder; its encoded length bounds parsing.
    section=next(r for r in raw if r[3]<=p<r[3]+r[5] and r[1]!=8)
    b=rdb(p,section[3]+section[5]-p)
    lpe=b[0]; i=1
    if lpe!=0xff: _,i=uleb(b,i)
    tte=b[i]; i+=1
    if tte!=0xff: _,i=uleb(b,i)
    cse=b[i]; i+=1
    ln,i=uleb(b,i); end=i+ln
    while i<end:
        s,i=uleb(b,i); l,i=uleb(b,i); lp,i=uleb(b,i); a,i=uleb(b,i)
        print('  try %x-%x -> lp %s action %d'%(fn+s,fn+s+l,hex(fn+lp) if lp else 'none',a))
