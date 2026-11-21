#!/bin/bash
# Offline C# compile check for one Unity assembly, using Unity's own Bee .rsp (same sources/refs/defines).
# Usage: asm_check.sh <AssemblyName> [outdir]   e.g. asm_check.sh Assembly-CSharp
# Does NOT touch Library or the project; output dll goes to outdir (default /tmp/asmcheck_$USER/<asm>).
# Note: source list = files known at Unity's last compile (new .cs files are not included).
set -uo pipefail
P=${REPO:?set REPO=Unity project root}
CSC=${CSC:-$(dirname "${EDITOR:?set EDITOR=path to Unity editor binary}")/Data/DotNetSdkRoslyn/csc.dll}
A=${1:?assembly name}
OUT=${2:-/tmp/asmcheck_$USER/$A}
mkdir -p "$OUT"
RSP=$(ls -t "$P"/Library/Bee/artifacts/*.dag/"$A".rsp 2>/dev/null | head -1)
[ -z "$RSP" ] && { echo "no rsp for $A"; exit 2; }
command grep -v -E '^-(out|refout|generatedfilesout|errorlog|pdb|doc):' "$RSP" > "$OUT/check.rsp"
echo "-out:\"$OUT/$A.dll\"" >> "$OUT/check.rsp"
cd "$P"
dotnet "$CSC" /nologo /warn:0 @"$OUT/check.rsp" > "$OUT/build.log" 2>&1
rc=$?
n=$(command grep -c "error CS" "$OUT/build.log")
command grep "error CS" "$OUT/build.log" | sort -u | head -40
echo "asm_check $A: exit=$rc errors=$n log=$OUT/build.log"
exit $rc
