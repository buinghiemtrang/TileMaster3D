#!/usr/bin/env python3
"""Read-only coverage checks matching DecompileTargets.java's class/header output."""
import collections
import pathlib
import re
import sys


def targets(path):
    classes = collections.defaultdict(collections.Counter)
    for line in pathlib.Path(path).read_text().splitlines():
        rva, name = line.split('\t')
        cls, _ = name.split('$$', 1)
        classes[cls][(name, int(rva))] += 1
    if not classes:
        raise ValueError('empty targets')
    return classes


def coverage(target, directory, kind):
    expected = targets(target)
    rename = None
    if kind == 'named':
        target_path = pathlib.Path(target)
        names = {}
        for line in (target_path.parent / 'names.tsv').read_text().splitlines():
            parts = line.split('\t')
            if len(parts) == 2:
                names.setdefault(int(parts[0]), parts[1])
        helpers = {'0237fa00': 'il2cpp_init_metadata', '0237fcb0': 'ThrowNullReferenceException',
                   '0237fca4': 'il2cpp_object_new', '0237f99c': 'il2cpp_wbarrier'}
        mapping = {}
        for line in target_path.with_name(target_path.stem + '-veneers.tsv').read_text().splitlines():
            address, dest = line.split('\t')
            if address in helpers:
                mapping[address] = helpers[address]
            elif dest != '?':
                rva = int(dest, 16) & 0xffffffff
                mapping[address] = names.get(rva, 'native_%x' % rva)
        for address, name in helpers.items():
            mapping.setdefault(address, name)
        def rename(text):
            return re.sub(r'func_0x([0-9a-f]{8})',
                          lambda match: mapping.get(match[1], match[0]).replace('$$', '.'), text)
    suffix = '.named.c' if kind == 'named' else '.c'
    directory = pathlib.Path(directory)
    actual = {p.name for p in directory.glob('*' + suffix)
              if kind == 'named' or not p.name.endswith('.named.c')}
    wanted = {cls + suffix for cls in expected}
    errors = []
    if actual != wanted:
        errors.append('missing=%d extra=%d classes' % (len(wanted - actual), len(actual - wanted)))
    matched = 0
    for cls, functions in expected.items():
        path = directory / (cls + suffix)
        if not path.is_file():
            continue
        content = path.read_text()
        headers = collections.Counter((name, int(rva, 16)) for name, rva in
                                      re.findall(r'^// (.+)  RVA: 0x([0-9a-f]+)$', content, re.M))
        if headers != functions:
            errors.append('%s: function headers differ' % path.name)
        elif rename and content != rename((directory / (cls + '.c')).read_text()):
            errors.append('%s: named content differs from complete raw transformation' % path.name)
        else:
            matched += sum(headers.values())
    print('%s: %d/%d functions, %d/%d classes%s' %
          (kind, matched, sum(sum(v.values()) for v in expected.values()),
           len(actual & wanted), len(wanted), '; ' + '; '.join(errors[:5]) if errors else ''))
    return not errors


def main():
    mode, *args = sys.argv[1:]
    if mode in ('raw', 'named'):
        return coverage(*args, mode)
    if mode == 'addresses':
        target, directory = args
        addresses = set()
        for cls in targets(target):
            addresses.update(re.findall(r'func_0x([0-9a-f]+)',
                                        (pathlib.Path(directory) / (cls + '.c')).read_text()))
        for address in sorted(addresses):
            print(address)
        return True
    if mode == 'veneers':
        addr, veneer = map(pathlib.Path, args)
        wanted = addr.read_text().splitlines()
        rows = [line.split('\t') for line in veneer.read_text().splitlines()]
        valid = all(len(row) == 2 and re.fullmatch(r'\?|[0-9a-f]+', row[1]) for row in rows)
        return valid and [row[0] for row in rows] == wanted
    raise ValueError('unknown mode: ' + mode)


if __name__ == '__main__':
    try:
        sys.exit(0 if main() else 1)
    except (OSError, ValueError) as error:
        print(error, file=sys.stderr)
        sys.exit(1)
