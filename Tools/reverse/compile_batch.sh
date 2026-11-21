#!/bin/bash
# Batch compile the Unity project and print a summary of errors.
# usage: compile_batch.sh [N]      env: EDITOR (Unity binary), REPO, WORK. Fails if an editor already has the project open.
EDITOR=${EDITOR:-$HOME/Unity/Hub/Editor/6000.0.62f1/Editor/Unity}
REPO=${REPO:-$HOME/Desktop/royal-smash}
WORK=${WORK:-$HOME/Desktop/royal-smash-work}
N=${1:-$(date +%H%M%S)}
if ps -eo comm,args | grep -E "^Unity .*$REPO" >/dev/null; then echo "Unity editor has the project open; close it first"; exit 2; fi
LOG=$WORK/unity-compile-$N.log
"$EDITOR" -batchmode -nographics -quit -projectPath "$REPO" -logFile "$LOG"
echo "log: $LOG  (exit $?)"
grep "error CS" "$LOG" | sort -u | sed -E 's/\(([0-9]+),[0-9]+\): error (CS[0-9]+): (.{0,150}).*/:\1 \2 \3/' | head -${MAXERR:-60}
echo "errors: $(grep -c 'error CS' "$LOG")"
