# Fill empty `event` add/remove accessors whose compiler-generated backing field m_<Name> exists.
import re,sys,glob,os,shutil
P,B,R=sys.argv[1:4]; os.makedirs(B,exist_ok=True)
pat=re.compile(r'(public (?:static )?event [^\n]+? (\w+)\n(\t+)\{\n(?:\t+\[CompilerGenerated\]\n)?\t+add\n\t+\{\n)(\t+\}\n(?:\t+\[CompilerGenerated\]\n)?\t+remove\n\t+\{\n)(\t+\})')
filled=[];skipped=[]
for f in glob.glob(P+'/**/*.cs',recursive=True):
    s=open(f,errors='ignore').read()
    if 'add\n' not in s: continue
    def rep(m):
        name=m.group(2); ind=m.group(3)
        if re.search(r'\bm_'+re.escape(name)+r';',s):
            filled.append(f'{os.path.relpath(f,P)}:{name}')
            return m.group(1)+f'{ind}\t\tm_{name} += value;\n'+m.group(4)+f'{ind}\t\tm_{name} -= value;\n'+m.group(5)
        skipped.append(f'{os.path.relpath(f,P)}:{name}'); return m.group(0)
    s2=pat.sub(rep,s)
    if s2!=s:
        shutil.copy(f,os.path.join(B,os.path.relpath(f,P).replace('/','__'))); open(f,'w').write(s2)
open(R,'w').write('FILLED\n'+'\n'.join(filled)+'\nSKIPPED\n'+'\n'.join(skipped)+'\n')
print('filled',len(filled),'skipped',len(skipped))
