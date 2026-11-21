#!/bin/bash
# show.sh <decomp dir> <Class> <Method> [overload index]: print pseudocode without noise
D=$1; n=${4:-1}
f=$(command ls "$D" | grep -F "$2\$\$$3@" | sed -n "${n}p")
echo "=================== $f"
grep -v -E '^/\* WARNING|^\s*(undefined[0-9]*|float|int|long|uint|ulong|bool|byte|char|ushort|short|void \*|Unity\w*|Core\w*|System\w*|Foundation\w*|float[234]_il2cpp)( \*)? +[A-Za-z_0-9]+( \[[0-9]+\])?;$' "$D/$f" | grep -v '^\s*$'
