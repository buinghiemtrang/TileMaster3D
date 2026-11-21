import json,sys
d=json.load(open(sys.argv[1]))['normalized_result']
T=d['types']; imgs={i['name']:i for i in d['images']}
im=imgs['Core.dll']; idx={}
for i in range(im['type_start'],im['type_start']+im['type_count']):
    t=T[i]
    if t['declaring_type']==-1: idx[(t['namespace']+'.' if t['namespace'] else '')+t['name']]=i
from collections import defaultdict
F=defaultdict(list)
for f in d['fields']: F[f['declaring_type']].append(f['name'])
for name in sys.argv[2:]:
    i=idx[name]; t=T[i]
    ms=[m['name'] for m in d['methods'][t['method_start']:t['method_start']+t['method_count']]] if t['method_count'] else []
    ms=[m for m in dict.fromkeys(ms) if not m.startswith(('get_','set_','.c','<'))]
    fs=[f for f in F[i] if not f.startswith('<')]
    print(f"\n## {name}\n F: {', '.join(fs)[:900]}\n M: {', '.join(ms)[:1100]}")
