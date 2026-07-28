# Verdict differ: cell-by-cell, between two graph-truth verdict directories.
# Usage: python compare-verdicts.py <before-dir> <after-dir> [label]
# "Nothing moved" is Batch D's whole acceptance, so it needs a mechanical check, not an eyeball.
import json, io, os, sys

CHECKS = ["transport", "handler-join", "hub-sanity", "entry-target", "style", "sln-scope", "dup-name"]


def load(d):
    out = {}
    if not os.path.isdir(d):
        return out
    for f in os.listdir(d):
        if not f.endswith(".json"):
            continue
        v = json.load(io.open(os.path.join(d, f), encoding="utf-8-sig"))
        repo = f[:-5]
        if v.get("error"):
            out[repo] = {c: v.get("error", "ERR").upper() for c in CHECKS}
        else:
            out[repo] = {c: v["checks"].get(c, {}).get("verdict", "?") for c in CHECKS}
    return out


before_dir, after_dir = sys.argv[1], sys.argv[2]
label = sys.argv[3] if len(sys.argv) > 3 else ""
before, after = load(before_dir), load(after_dir)

moved, only_after, only_before = [], [], []
for repo in sorted(set(before) | set(after)):
    if repo not in before:
        only_after.append(repo)
        continue
    if repo not in after:
        only_before.append(repo)
        continue
    for c in CHECKS:
        b, a = before[repo][c], after[repo][c]
        if b != a:
            moved.append((repo, c, b, a))

print("== %s ==" % label)
print("compared %d repos present in both" % len(set(before) & set(after)))
if moved:
    print("MOVED CELLS (%d):" % len(moved))
    for repo, c, b, a in moved:
        print("  %-24s %-13s %s -> %s" % (repo, c, b, a))
else:
    print("MOVED CELLS: none")
if only_after:
    print("only in after (%d): %s" % (len(only_after), ", ".join(only_after)))
if only_before:
    print("only in before (%d): %s" % (len(only_before), ", ".join(only_before)))
