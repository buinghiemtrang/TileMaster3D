# Fill empty Trigger*/SubscribeTo*/UnsubscribeFrom* bodies in *Events*.cs following the generated EventsData pattern.
import re,sys,glob,os,shutil
P,B,R=sys.argv[1:4]; os.makedirs(B,exist_ok=True)
sig=re.compile(r'^\t\tpublic void ((Trigger|SubscribeTo|UnsubscribeFrom)(\w+?))(Event)?\(([^)]*)\)\s*$')
done=[];skipped=[]
def argnames(params):
    out=[]
    for p in [x.strip() for x in params.split(',') if x.strip()]:
        p=re.sub(r'=.*$','',p).strip(); out.append(p.split()[-1])
    return out
for f in glob.glob(P+'/**/*Events*.cs',recursive=True):
    s=open(f,errors='ignore').read(); L=s.split('\n')
    fields=set(re.findall(r'private (?:event )?(?:System\.)?Action(?:<[^;=]*>)? (\w+);',s))
    has_data='eventsData' in s and re.search(r'\b\w*EventsData eventsData;',s)
    ch=False
    for i,l in enumerate(L):
        m=sig.match(l)
        if not m or i+2>=len(L) or L[i+1].strip()!='{' or L[i+2].strip()!='}': continue
        full,kind,name,ev,params=m.group(1)+(m.group(4) or ''),m.group(2),m.group(3),m.group(4),m.group(5)
        args=argnames(params); body=None
        cand=[c for c in (f'm_On{name}',f'On{name}',f'm_{name}',f'_on{name}',f'on{name}') if c in fields]
        if cand and not has_data:
            fld=cand[0]
            if kind=='Trigger': body=f'{fld}?.Invoke({", ".join(args)});'
            elif kind=='SubscribeTo' and len(args)==1: body=f'{fld} += {args[0]};'
            elif kind=='UnsubscribeFrom' and len(args)==1: body=f'{fld} -= {args[0]};'
        elif has_data:
            body=f'eventsData.{full}({", ".join(args)});'
        if body:
            L[i+1]='\t\t{'; L[i+2]='\t\t\t'+body+'\n\t\t}'; ch=True; done.append(f'{os.path.relpath(f,P)}:{full}')
        else: skipped.append(f'{os.path.relpath(f,P)}:{full}')
    if ch:
        shutil.copy(f,os.path.join(B,os.path.relpath(f,P).replace('/','__'))); open(f,'w').write('\n'.join(L))
open(R,'w').write('FILLED\n'+'\n'.join(done)+'\nSKIPPED\n'+'\n'.join(skipped)+'\n')
print('filled',len(done),'skipped',len(skipped))
