"""Rewrite Ghidra's second spelling of an IL2CPP literal-pool address into the first one, in place.

Ghidra sometimes prints `_UNK_b + 0xa` (constant folded) where it elsewhere prints `&UNK_a + _UNK_b`.
annotate_pseudoc.py / meta_slots.py only understand the second form. On Windows (Ghidra 12.1.4) the folded form shows up
in ~15% of the places; on Ubuntu in ~1%. This makes both spell the same, so run it on every raw <Class>.c first.

usage: python -I normalize_unk.py <Class.c> [more.c ...]
Does not depend on the game (ARMv7 literal pools: address = a + b).
"""
import re, sys

# `_UNK_b + 0xa` -> `&UNK_a + _UNK_b`  (a padded to 8 hex digits; only when a is a plausible address)
FOLDED = re.compile(r'\b_UNK_([0-9a-f]{8}) \+ 0x([0-9a-f]{5,8})\b')
# `(char *)(&UNK_a + _UNK_b)` in an assignment -> `&UNK_a + _UNK_b`
CASTED = re.compile(r'= \((?:char|byte|undefined\d?|int|uint|code) \*+\)\((&UNK_[0-9a-f]{8} \+ _UNK_[0-9a-f]{8})\);')


def fix(text):
    text = FOLDED.sub(lambda m: '&UNK_%s + _UNK_%s' % (m.group(2).zfill(8), m.group(1)), text)
    return CASTED.sub(r'= \1;', text)


if __name__ == '__main__':
    for p in sys.argv[1:]:
        s = open(p, encoding='utf8').read()
        t = fix(s)
        if t != s:
            open(p, 'w', encoding='utf8', newline='\n').write(t)
