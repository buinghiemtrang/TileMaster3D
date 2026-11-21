#!/bin/bash
# Print 4 floats at each RVA of libil2cpp.so (RVA = Ghidra address in the pseudo-C minus 0x10000).
# usage: read_floats.sh 0x1f5d464 0x1f62aa0 ...      env: GHIDRA (install dir), WORK (royal-smash-work)
GHIDRA=${GHIDRA:-$HOME/tools/ghidra_12.1.4_PUBLIC}
WORK=${WORK:-$HOME/Desktop/royal-smash-work}
HERE=$(cd "$(dirname "$0")" && pwd)
cd "$WORK/ghidra" || exit 1
"$GHIDRA/support/analyzeHeadless" project royal -process libil2cpp.so -noanalysis -scriptPath "$HERE" \
  -postScript ReadFloats.java "$@" 2>&1 | grep -E 'ReadFloats.java> RVA' | sed 's/INFO  ReadFloats.java> //; s/ (GhidraScript)//'
