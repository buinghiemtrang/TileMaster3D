# Ước lượng khối lượng dịch theo nhóm (phân nhóm theo tên namespace/class, xấp xỉ).
# argv: REA evidence bundle, output tsv
import json, sys, re, collections, bisect

b = json.load(open(sys.argv[1]))
nr = [r for r in b['records'] if r['operation'] == 'inspect_il2cpp_metadata'][0]['normalized_result']
imgs, types, methods, addrs = nr['images'], nr['types'], nr['methods'], nr['method_addresses']

all_addr = sorted({int(a['rva'], 16) for a in addrs if a['state'] == 'code'})
def size_of(rva):
    i = bisect.bisect_right(all_addr, rva)
    sz = (all_addr[i] - rva) if i < len(all_addr) else 0
    return sz if sz <= 0x8000 else 0  # gap to next image / runtime code: not a method body

type_img = {}
for ii, im in enumerate(imgs):
    for t in range(im['type_start'], im['type_start'] + im['type_count']):
        type_img[t] = im['name']
def top(t):
    while types[t]['declaring_type'] != -1:
        t = types[t]['declaring_type']
    return t

EMBED_NS = {'SimpleJSON', 'DG.Tweening', 'MoreMountains.NiceVibrations', 'UnityEngine.SignInWithApple',
            'UnityEngine.Purchasing.Security', 'BMFontEditor', 'Fishtail.PlayTheBall.Vibration', 'FlyingWormConsole3',
            '__test.Scripts', 'aaaaa', '_FrameWork.Utilities.EditorHelper'}
EMBED_CLS = re.compile(r'^(Reporter|ReporterGUI|ReporterMessageReceiver|TestReporter|Images|ConsoleProDebug|NewBehaviourScript|MBezierTest|MultiKeyDictionary`3|UnitySourceGenerated.*|<PrivateImplementationDetails>|<Module>)$')
DEBUG_CLS = re.compile(r'^(GM\w*|DevAssistantGUI|MemoryProfile|LogEventItem)$')
SERVER = re.compile(r'^(GameTeam|GameMail|OwnHttp|_FrameWork\.Http|Authentication|Downloaded)$|Http|Server|Email|Login|Team|Ranking|Rank|Mail|Notice|Facebook|IAP\w*OrderValidate|OrderValidate|ClearAccount|Guest')
LIVEOPS = re.compile(r'RoyalPass|TreasureHunt|ShortBattlePass|BakingMaster|Festival|MonthRanking|LuckyPrize|RoadToRiches|PropIceBreaking|Recharge|ActivityEntrance|Activity|Pop_Gift|Gift|StarRanking|Victories|RegionRank|Turntable|Pig|Recall|CardSystem|Card|MagicCube|GamePark|SignBoard|Achive|Medal|Chest|StoreMultiple|UserLevelReward|SpecialBlock|Christmas|christmas')
MONET = re.compile(r'^(Advertisement|AdvertisingCustom|Function_Reward_Video|RewardAdView|FQDev\.StoreKit|Recharge)$|Ad[A-Z]|^Ad|Advert|IAP|Store|Purchase|LogEvent|SDK|UserLabel|Banner|Inter|RewardVideo|Consent|NoAds')
CORE = re.compile(r'^GameScene|Element|Booster|Prop|Magnet|MagicWand|Combo|Level|Energy|Fail|Victory|Guide|Novice|TeatherHand|Fuse|Unlock')

def group(ii_name, ns, cls):
    if ii_name == 'Assembly-CSharp-firstpass.dll':
        return '4 SDK wrapper (firstpass)'
    if ii_name.startswith('FQDev.'):
        return '7 FQDev in-house libs'
    if ii_name == 'OSA.dll':
        return '8 OSA (paid asset)'
    if ii_name != 'Assembly-CSharp.dll':
        return None
    full = (ns + '.' if ns else '') + cls
    if ns in EMBED_NS or EMBED_CLS.match(cls):
        return '9 embedded lib/debug tool in Assembly-CSharp'
    if DEBUG_CLS.match(cls):
        return '6 GM/debug panel'
    if CORE.search(ns) or (not ns and CORE.search(cls) and not LIVEOPS.search(cls) and not MONET.search(cls)):
        return '1 core gameplay'
    if SERVER.search(ns) or SERVER.search(cls):
        return '3 server-dependent (team/rank/mail/login/http)'
    if MONET.search(ns) or MONET.search(cls):
        return '5 ads/IAP/analytics (game side)'
    if LIVEOPS.search(ns) or LIVEOPS.search(cls):
        return '2 live-ops/meta features'
    return '0 framework/UI/common'

st = collections.defaultdict(lambda: [set(), 0, 0, 0, 0])  # types, methods, code, bytes, small(<=16B)
for mi, m in enumerate(methods):
    t = top(m['declaring_type'])
    g = group(type_img[t], types[t]['namespace'], types[t]['name'])
    if not g:
        continue
    s = st[g]
    s[0].add(t); s[1] += 1
    a = addrs[mi]
    if a['state'] == 'code':
        s[2] += 1
        sz = size_of(int(a['rva'], 16))
        s[3] += sz
        if sz <= 16: s[4] += 1
with open(sys.argv[2], 'w') as f:
    f.write('group\ttop_types\tmethods\twith_code\tcode_KB\tsmall<=16B\n')
    for g in sorted(st):
        s = st[g]
        line = f'{g}\t{len(s[0])}\t{s[1]}\t{s[2]}\t{s[3]//1024}\t{s[4]}'
        f.write(line + '\n'); print(line)
