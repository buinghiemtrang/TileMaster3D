#!/bin/bash
# Run an editor static method in batch mode WITHOUT -quit (for validators that exit by themselves).
# usage: run_editor_method.sh <Class.Method> <logname>      env: EDITOR, REPO, WORK
EDITOR=${EDITOR:-$HOME/Unity/Hub/Editor/6000.0.62f1/Editor/Unity}
REPO=${REPO:-$HOME/Desktop/royal-smash}
WORK=${WORK:-$HOME/Desktop/royal-smash-work}
if ps -eo comm,args | grep -E "^Unity .*$REPO" >/dev/null; then echo "Unity editor has the project open; close it first"; exit 2; fi
LOG=$WORK/${2:-unity-run}.log
"$EDITOR" -batchmode -nographics -projectPath "$REPO" -executeMethod "$1" -logFile "$LOG"
echo "exit $?  log: $LOG"
