import json,sys,collections,re
d=json.load(open(sys.argv[1]))['normalized_result']
T=d['types']; imgs=d['images']
own=lambda n: n in('Core.dll','Assembly-CSharp.dll') or n.startswith(('RoyalSmash.','BackendMulti.','Foundation.'))
tot_t=tot_m=0
for im in imgs:
    if not own(im['name']): continue
    ts=T[im['type_start']:im['type_start']+im['type_count']]
    m=sum(t['method_count'] for t in ts); tot_t+=len(ts); tot_m+=m
    if im['name'] in('Core.dll','Assembly-CSharp.dll') or im['name'].startswith('RoyalSmash.'):
        ns=collections.Counter(); nm=collections.Counter()
        for t in ts:
            if t['declaring_type']==-1 and not t['name'].startswith('<'):
                ns[t['namespace'] or '(global)']+=1; nm[t['namespace'] or '(global)']+=t['method_count']
        print(f"\n### {im['name']}: {len(ts)} types, {m} methods")
        for k,v in sorted(ns.items(), key=lambda x:-nm[x[0]])[:int(sys.argv[2])]:
            print(f"  {k}: {v} types / {nm[k]} methods")
print(f"\nTOTAL own: {tot_t} types, {tot_m} methods of {len(T)} / {len(d['methods'])}")
