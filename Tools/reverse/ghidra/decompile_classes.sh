#!/bin/bash
# One step for pha 6 step 3: decompile classes, clean them and name the metadata slots.
# usage: decompile_classes.sh Class [Class...]     (names as in script.json, without namespace is fine; nested/generic classes are included)
# env: GHIDRA (install dir), WORK (royal-smash-work). Needs DefineHelpers.java to have been run once (pha 6 step 2).
GHIDRA=${GHIDRA:-$HOME/tools/ghidra_12.1.4_PUBLIC}
WORK=${WORK:-$HOME/Desktop/royal-smash-work}
HERE=$(cd "$(dirname "$0")" && pwd)
cd "$WORK/ghidra" || exit 1
python3 -I "$HERE/make_ghidra_inputs.py" ../il2cpp/script.json . "$@" || exit 1
"$GHIDRA/support/analyzeHeadless" project royal -process libil2cpp.so -noanalysis -scriptPath "$HERE" \
  -postScript DecompileTargets.java "$PWD/targets.tsv" "$PWD/pseudoc" 2>&1 | grep -E "decompiled .* functions"
cd pseudoc
for f in *.c; do
  case "$f" in *.clean.c|*.ann.c|*.named.c) continue;; esac
  [ "$f" -nt "${f%.c}.ann.c" ] || continue
  python3 -I "$HERE/clean_pseudoc.py" "$f" >/dev/null 2>&1
  python3 -I "$HERE/annotate_pseudoc.py" "${f%.c}.clean.c"
done
