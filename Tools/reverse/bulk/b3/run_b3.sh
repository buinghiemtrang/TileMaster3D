#!/bin/bash
# B3 pipeline: Ghidra extract per group -> veneers -> names -> Codex translate per group (sequential).
set -euo pipefail
# Env: WORK, REPO, GHIDRA (install dir), GPROJ (Ghidra project name), GAME (display name), GRPS (group list), PY (python)
W=${WORK:?}
P=${REPO:?}
G=${GHIDRA:?}/support/analyzeHeadless
PY=${PY:-python3}
T=$(cd "$(dirname "$0")" && pwd)
CHECK=$T/b3_coverage.py
LOG=$W/logs/b3-pipeline.log
GRPS=${GRPS:?space-separated group names, each with WORK/ghidra/b3-<group>.tsv}
trap 'rc=$?; echo "$(date +%T) pipeline failed rc=$rc line=$LINENO" >> "$LOG"; exit "$rc"' ERR
cd "$W/ghidra"
shopt -s nullglob
for g in $GRPS; do
  out=$W/ghidra/b3/$g
  target=$W/ghidra/b3-$g.tsv
  mkdir -p "$out"
  if ! "$PY" "$CHECK" raw "$target" "$out" >> "$LOG" 2>&1; then
    existing=("$out"/*.c)
    if (( ${#existing[@]} )); then
      echo "$(date +%T) incomplete/ambiguous raw extract $g; inspect files before retry" >> "$LOG"
      exit 1
    fi
    echo "$(date +%T) extract $g" >> "$LOG"
    "$G" "$W/ghidra/project" "${GPROJ:?}" -process libil2cpp.so -readOnly -noanalysis -scriptPath "$P/Tools/reverse/ghidra" -postScript DecompileTargets.java "$target" "$out" > "$W/logs/ghidra-b3-$g.log" 2>&1
    "$PY" "$CHECK" raw "$target" "$out" >> "$LOG" 2>&1
  fi
  if ! "$PY" "$CHECK" named "$target" "$out" >> "$LOG" 2>&1; then
    "$PY" "$CHECK" addresses "$target" "$out" > "$W/ghidra/b3-$g.addr"
    "$G" "$W/ghidra/project" "${GPROJ:?}" -process libil2cpp.so -readOnly -noanalysis -scriptPath "$P/Tools/reverse/ghidra" -postScript ResolveVeneers.java "$W/ghidra/b3-$g.addr" "$W/ghidra/b3-$g-veneers.tsv" > "$W/logs/ghidra-b3-$g-veneers.log" 2>&1
    "$PY" "$CHECK" veneers "$W/ghidra/b3-$g.addr" "$W/ghidra/b3-$g-veneers.tsv"
    raw=()
    for file in "$out"/*.c; do
      [[ "$file" == *.named.c ]] || raw+=("$file")
    done
    "$PY" "$T/../name_c.py" names.tsv "b3-$g-veneers.tsv" "${raw[@]}" >> "$LOG" 2>&1
    "$PY" "$CHECK" named "$target" "$out" >> "$LOG" 2>&1
  fi
  echo "$(date +%T) extraction verified $g" >> "$LOG"
done
echo "$(date +%T) all extraction done" >> "$LOG"
[ "${1:-}" = "extract-only" ] && exit 0
# Translation is the only stage that requires Codex.
codex_bins=("$HOME"/.vscode/extensions/openai.chatgpt-*/bin/linux-x86_64/codex)
if (( ${#codex_bins[@]} == 0 )); then
  echo "$(date +%T) codex executable not found" >> "$LOG"
  exit 1
fi
C=${codex_bins[${#codex_bins[@]}-1]}
report_complete() {
  "$PY" - "$1" "$2" <<'PY'
import pathlib, re, sys
path = pathlib.Path(sys.argv[1])
text = path.read_text() if path.is_file() else ''
expected = len({line.split('\t', 1)[1].split('$$', 1)[0]
                for line in pathlib.Path(sys.argv[2]).read_text().splitlines()})
match = re.match(r'Tiến độ: ([1-9][0-9]*)/([1-9][0-9]*) ', text)
sys.exit(0 if match and int(match[1]) == int(match[2]) == expected else 1)
PY
}
for g in $GRPS; do
  if report_complete "$W/reports/b3-$g.md" "$W/ghidra/b3-$g.tsv"; then continue; fi
  sed -e "s#__PROJ__#$P#; s#__WORK__#$W#; s#__GROUP__#$g#g; s#__GAME__#${GAME:-the game}#g" "$T/prompt_template.txt" > "$W/logs/prompt-b3-$g.txt"
  echo "$(date +%T) codex start $g" >> "$LOG"
  rc=0
  "$C" exec --skip-git-repo-check -s workspace-write -m gpt-6.1-sol -c model_reasoning_effort="low" -C "$P" --add-dir "$P" --add-dir "$W" -o "$W/logs/codex-b3-last-$g.txt" "$(cat "$W/logs/prompt-b3-$g.txt")" < /dev/null > "$W/logs/codex-b3-$g.log" 2>&1 || rc=$?
  echo "$(date +%T) codex end $g rc=$rc" >> "$LOG"
  if grep -qiE "hit your usage limit|usage limit reached|rate_limit_exceeded" "$W/logs/codex-b3-$g.log"; then echo "$(date +%T) codex limit hit, stop" >> "$LOG"; exit 2; fi
  if (( rc != 0 )); then exit "$rc"; fi
  report_complete "$W/reports/b3-$g.md" "$W/ghidra/b3-$g.tsv"
done
echo "$(date +%T) pipeline done" >> "$LOG"
