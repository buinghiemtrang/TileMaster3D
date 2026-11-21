"""Name the IL2CPP metadata-usage slots used by pseudo-C (types, methods, strings, fields).

usage: python -I meta_slots.py <Class.clean.c ...>   (env WORK, GHIDRA)
Finds `&UNK_<a> + _UNK_<b>` pairs, resolves them with ResolveMetaSlots.java, maps the slot VA through
script.json ScriptMetadata/ScriptMetadataMethod/ScriptString, and prints `<a>_<b>  name`.
Output is cached next to each file as <Class>.slots.tsv.
"""
import json, os, re, subprocess, sys, tempfile
WORK = os.environ.get('WORK', os.path.expanduser('~/Desktop/royal-smash-work'))
GHIDRA = os.environ.get('GHIDRA', os.path.expanduser('~/tools/ghidra_12.1.4_PUBLIC'))
HERE = os.path.dirname(os.path.abspath(__file__))
pairs = {}
for f in sys.argv[1:]:
    for m in re.finditer(r'&UNK_([0-9a-f]{8}) \+ _UNK_([0-9a-f]{8})', open(f, encoding='utf8').read()):
        pairs[m.group(1) + '_' + m.group(2)] = (m.group(1), m.group(2))
tmp = tempfile.mkdtemp()
pf, of = os.path.join(tmp, 'p.txt'), os.path.join(tmp, 'o.tsv')
open(pf, 'w').write('\n'.join('%s %s' % v for v in pairs.values()))
r = subprocess.run([GHIDRA + '/support/analyzeHeadless', WORK + '/ghidra/project', 'royal', '-process', 'libil2cpp.so', '-noanalysis',
                '-scriptPath', HERE, '-postScript', 'ResolveMetaSlots.java', pf, of], cwd=WORK + '/ghidra', capture_output=True); sys.stderr.write(r.stdout.decode()[-600:] if not os.path.exists(of) else "")
d = json.load(open(WORK + '/il2cpp/script.json'))
names = {}
for k in ('ScriptMetadata', 'ScriptMetadataMethod'):
    for m in d[k]: names[m['Address']] = m['Name']
for m in d.get('ScriptString', []): names[m['Address']] = 'string ' + json.dumps(m['Value'], ensure_ascii=False)
for line in open(of):
    k, _, va = line.rstrip('\n').split('\t')
    print(k, names.get(int(va), '?%x' % int(va)))
