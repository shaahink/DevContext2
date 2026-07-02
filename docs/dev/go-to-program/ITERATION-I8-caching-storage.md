# Iteration I8 — Caching & storage (repo-hash snapshots, clone consolidation, user control)

> **Status: NOT STARTED** · Addendum to the fixed plan (complements, does not modify, I1–I7) ·
> Depends on: I2 (kernel serialization — the snapshot format IS the kernel JSON records) ·
> Unblocks: I10 (tab rehydration), the V5 GitHub-URL path, instant desktop re-opens.
> One session engine-side + one settings-UI slice.

## Goal

Analyze a repo once per commit. Re-opening it — CLI, desktop, any tab — is near-instant from a disk
snapshot, honestly stamped. Clones and caches live in one user-visible, user-controlled place
(Settings → Storage).

## Design

### Cache identity (the tricky bit — get this right first)

```
repoKey    = SHA256(normalized absolute root path)            // stable per checkout location
versionKey = git repo:  HEAD sha
             + (dirty ? "-dirty-" + SHA256(porcelain status text
                                           + per-changed-file (path, mtime, size)) : "")
             non-git:   "manifest-" + SHA256(all *.sln/*.csproj (path, mtime, size))
engineKey  = DevContextVersion + snapshot schema version      // engine upgrade ⇒ cold
```

- Use `git` plumbing via existing `GitCloneService`-style shelling (`git -C <root> rev-parse HEAD`,
  `git status --porcelain`); **fall back to manifest hashing when git is absent or errors** — never
  fail an analysis because hashing failed (log Info, run uncached).
- Dirty-tree runs ARE cached (keyed by the dirty digest) — devs live in dirty trees; two consecutive
  runs without edits must hit.
- **Pitfall:** mtime-only digests miss same-second edits — include size, accept the rare false hit;
  document it. Do NOT hash full file contents at key time (that's an analysis-scale cost).

### Snapshot store (L2 — the new layer)

```
%LOCALAPPDATA%/DevContext/
  cache/<repoKey>/<versionKey>.snap.json.gz     // + meta.json (path label, engineKey, sizes, lastUsed)
  clones/<owner>-<repo>/                        // consolidated GitCloneService target
  logs/
```

Contents: the **queryable parts only** — `CodeGraph` + `MapModel` + entries + insights + seam stats +
scope/diagnostics summary. All already serialization-clean; reuse the I2 source-gen JSON context
(one contract — the snapshot is a saved kernel wire document). **Not persisted in v1:** source bodies,
parse trees, the Roslyn compilation (risk/size; note as v2 with the persistent-index work). Gzip via
`GZipStream`; **VOTE:** JSON.gz over MessagePack v1 — debuggable, zero new deps; revisit if >5s load
on OrchardCore-scale graphs.

### Flow

`analyze` → compute keys → hit (keys match + engineKey match) → load snapshot → serve Map/entries/
insights/graph queries instantly, stamped `from cache · <sha-short> · re-analyze available` in
Overview/CLI header → miss → full analysis → **write-behind** (background task; a failed write is a
warning, never an analysis failure).

**Honesty rule:** anything that needs the full model (a new deep trace at a different profile,
`--strict` self-check re-run) triggers transparent re-analysis with progress — the snapshot never
pretends to be the full model. `TraceBuilder` over the persisted `CodeGraph` works for normal traces
(the graph has the edges); only profile-changing operations re-analyze.

### Eviction & limits

LRU by `lastUsed` in meta; default cap 2 GB total / 10 versions per repo (keep newest per repo
always). Sweep on server boot + after each write. All caps configurable (prefs + `devcontext.json`).

### Clone consolidation

`GitCloneService` targets `clones/<owner>-<repo>`; if the folder exists → `fetch` + checkout ref
instead of re-clone. Default flips to **keep clones** (they're now managed + visible); `--keep`
becomes a no-op with notice. Cache and clone lifetimes are independent (clearing cache keeps clones).

## Faces

- **CLI:** `devcontext cache list` (repo · versions · size · last used) / `clear [repo]` / `path`;
  `analyze --no-cache` (force fresh) / `--cache-only` (fail if cold — CI use). Header line prints the
  cache stamp.
- **Server/desktop:** cache check inside the analyze RPC (client code unchanged — it just gets fast
  results + a `fromCache` flag on the summary for the stamp). New small RPCs: `GetStorageInfo`,
  `ClearCache(repoKey?)`.
- **Settings → Storage** (spec in UI-UX-GUIDELINES §7): cache location (read-only path + open folder),
  per-repo rows (label · versions · size · clear), total usage bar vs cap, clone folder row (path +
  open + keep-clones toggle), "Clear all" with confirm.

## Docs & goldens

`cli-reference.md` cache section (same-commit rule) · `desktop-ui.md` Storage group · eval: two new
checks — same-commit second run is `fromCache:true` and byte-identical Map; `--no-cache` forces miss.
Bench note: record warm-hit time for DntSite in `benchmarks/results/` (target < 2s).

## Gate

Gates green · DntSite: analyze → re-analyze hits cache < 2s with identical Map (paste timings) ·
dirty-edit → miss → re-hit · engine version bump → miss (test by faking engineKey) · settings Storage
screenshot · cache clear removes files.
