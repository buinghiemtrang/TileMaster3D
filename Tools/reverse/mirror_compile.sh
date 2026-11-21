#!/bin/bash
# Compile / run validators on a MIRROR of the project so the user's open editor is left alone.
# usage: mirror_compile.sh compile <name>            -> batch compile of the mirror, prints error summary
#        mirror_compile.sh run <Class.Method> <name> -> executeMethod (no -quit) on the mirror
# The mirror (WORK/mirror/royal-smash) is refreshed from REPO with rsync (Assets, Packages, ProjectSettings, Library).
# Not game specific.
REPO=${REPO:-$HOME/Desktop/royal-smash}
WORK=${WORK:-$HOME/Desktop/royal-smash-work}
MIRROR=${MIRROR:-$WORK/mirror/royal-smash}
EDITOR=${EDITOR:-$HOME/Unity/Hub/Editor/6000.0.62f1/Editor/Unity}
mkdir -p "$MIRROR"
[ -z "$NO_SYNC" ] && for d in Assets Packages ProjectSettings; do
  rsync -a --delete "$REPO/$d/" "$MIRROR/$d/"
done
# Library is copied once: re-copying it while the user's editor is working made the mirror's service list load with null entries.
if [ -n "$FRESH_LIBRARY" ]; then rm -rf "$MIRROR/Library"; elif [ ! -d "$MIRROR/Library" ] || [ -n "$RESET_LIBRARY" ]; then rsync -a --delete "$REPO/Library/" "$MIRROR/Library/"; fi
if ps -eo args | grep -F "$MIRROR" | grep -v grep | grep -i unity >/dev/null; then echo "Unity already running on the mirror"; exit 2; fi
case "$1" in
  compile)
    LOG=$WORK/unity-$2.log
    "$EDITOR" -batchmode -nographics -quit -projectPath "$MIRROR" -logFile "$LOG"
    echo "exit $?  log: $LOG"
    grep "error CS" "$LOG" | sort -u | sed -E 's/\(([0-9]+),[0-9]+\): error (CS[0-9]+): (.{0,150}).*/:\1 \2 \3/' | head -${MAXERR:-60}
    echo "errors: $(grep -c 'error CS' "$LOG")" ;;
  run)
    LOG=$WORK/$3.log
    NG=-nographics; [ -n "$GRAPHICS" ] && NG=""   # GRAPHICS=1 renders with the real GPU/X display (needed for screenshots)
    BM=-batchmode; [ -n "$WINDOWED" ] && { BM=""; NG=""; export UIFLOW_WINDOWED=1; }   # WINDOWED=1 opens a real editor window (exact Game view)
    "$EDITOR" $BM $NG -projectPath "$MIRROR" -executeMethod "$2" -logFile "$LOG"
    echo "exit $?  log: $LOG" ;;
esac
