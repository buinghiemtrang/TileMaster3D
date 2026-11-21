# Đếm type/method theo image và namespace từ REA evidence bundle (il2cpp metadata)
import json, sys, collections
src = sys.argv[1]; out = sys.argv[2]
b = json.load(open(src))
nr = [r for r in b['records'] if r['operation']=='inspect_il2cpp_metadata'][0]['normalized_result']
imgs, types, methods, addrs = nr['images'], nr['types'], nr['methods'], nr['method_addresses']
type_img = {}
for ii, im in enumerate(imgs):
    for t in range(im['type_start'], im['type_start']+im['type_count']): type_img[t] = ii
# top-level namespace (nested types take their outer's namespace)
def ns_of(t):
    while types[t]['declaring_type'] != -1: t = types[t]['declaring_type']
    return types[t]['namespace']
rows = []
img_stats = collections.defaultdict(lambda: [0,0,0,0])  # types, methods, code, nobody
ns_stats = collections.defaultdict(lambda: [0,0,0])
for ti, t in enumerate(types):
    ii = type_img[ti]; img_stats[ii][0]+=1
    ns_stats[(imgs[ii]['name'], ns_of(ti))][0]+=1
for mi, m in enumerate(methods):
    ti = m['declaring_type']; ii = type_img[ti]
    st = addrs[mi]['state']
    img_stats[ii][1]+=1
    if st=='code': img_stats[ii][2]+=1
    else: img_stats[ii][3]+=1
    k=(imgs[ii]['name'], ns_of(ti)); ns_stats[k][1]+=1
    if st=='code': ns_stats[k][2]+=1
with open(out,'w') as f:
    f.write('# image\ttypes\tmethods\tcode\tno_body\n')
    for ii,(a,bm,c,d) in sorted(img_stats.items(), key=lambda x:-x[1][1]):
        f.write(f'{imgs[ii]["name"]}\t{a}\t{bm}\t{c}\t{d}\n')
    f.write('\n# image\tnamespace\ttypes\tmethods\tcode\n')
    for (im,ns),(a,bm,c) in sorted(ns_stats.items(), key=lambda x:(x[0][0],-x[1][1])):
        f.write(f'{im}\t{ns or "(global)"}\t{a}\t{bm}\t{c}\n')
print('ok', out)
