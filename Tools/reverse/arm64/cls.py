import json,sys,collections,re
d=json.load(open(sys.argv[1]))['normalized_result']
T=d['types']; imgs={i['name']:i for i in d['images']}
mode=sys.argv[2]
def tl(img):
    im=imgs[img]; r=range(im['type_start'],im['type_start']+im['type_count'])
    return [(i,T[i]) for i in r if T[i]['declaring_type']==-1 and not T[i]['name'].startswith('<')]
if mode=='nslist':
    c=collections.Counter(); m=collections.Counter()
    for i,t in tl('Core.dll'):
        ns=t['namespace'] or '(global)'
        top='.'.join(ns.split('.')[:2]); c[top]+=1; m[top]+=t['method_count']
    for k in sorted(c,key=lambda k:-m[k]): print(f"{k}: {c[k]}t/{m[k]}m")
else:
    img,pat=sys.argv[3],sys.argv[4]; mn=int(sys.argv[5])
    rows=[(t['namespace'],t['name'],t['method_count']) for i,t in tl(img) if re.fullmatch(pat,t['namespace'] or '(global)') and t['method_count']>=mn]
    for ns,n,mc in sorted(rows,key=lambda r:(r[0],-r[2])): print(f"{mc:4d} {ns}.{n}")
