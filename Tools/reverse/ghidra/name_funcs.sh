#!/bin/bash
# Give every remaining func_0x........ in pseudoc/*.ann.c its managed name -> pseudoc/<Class>.named.c (read these).
# usage: name_funcs.sh [Class...]     env: GHIDRA, WORK. Resolves only addresses not yet in func_names.json.
GHIDRA=${GHIDRA:-$HOME/tools/ghidra_12.1.4_PUBLIC}
WORK=${WORK:-$HOME/Desktop/royal-smash-work}
HERE=$(cd "$(dirname "$0")" && pwd)
cd "$WORK/ghidra" || exit 1
[ -f func_names.json ] || echo '{}' > func_names.json
python3 -I - <<'P'
import glob, json, re
known = json.load(open('func_names.json'))
seen = set()
for f in glob.glob('pseudoc/*.c'):
    seen.update(re.findall(r'func_0x([0-9a-f]{8})', open(f).read()))
open('allfuncs.txt', 'w').write('\n'.join(sorted(a for a in seen if a not in known)) + '\n')
P
if [ -s allfuncs.txt ]; then
  "$GHIDRA/support/analyzeHeadless" project royal -process libil2cpp.so -noanalysis -scriptPath "$HERE" \
    -postScript ResolveVeneers.java "$PWD/allfuncs.txt" "$PWD/allfuncs.tsv" 2>&1 | grep -E "ERROR|Exception"
  python3 -I "$HERE/build_func_names.py" . ../il2cpp/script.json --merge
fi
python3 -I "$HERE/name_funcs.py" . "$@"
