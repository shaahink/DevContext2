# DevContext pre-release - desktop agent loop Phase Tracker

**Plan:** DevContext pre-release - desktop agent loop | **Branch:** `feat/pre-release-desktop` | **Design doc:** docs/dev/research/PRE-RELEASE-PLAN-2026-08-13.md

## Handoff (overwrite this block, ≤12 lines, no history)

N3 IS CLOSED. N3.2 landed and claimed (6efcef6 server + 032c9b8 app, evidence
eval-results/2026-08-14/N3.2-repo-file-handoff.md). New RPC SavePackFile writes
.devcontext/packs/<slug>.md into the analyzed repo + .devcontext/.gitignore holding packs/; Studio
Save is no longer a download and its strip shows the SERVER's path, the gitignore state, the
copyable CLAUDE.md line, and admits drift; Home's tile routes to /context. VERIFIED LIVE end to end
in the browser against a temp fixture copy, file read back off disk, preview and file both 1326
chars. Gates: lint 0, 256/256 tests (28 files), ng build 0, dotnet 0w/0e, contract-sweep PASS.
NEXT: N4.1 — but N4.3 needs Run A's T1 merged in; check that before sequencing N4.
TRAPS worth having (all measured this session): `conductor bg start -- pnpm ...` CANNOT spawn pnpm
here (MODULE_NOT_FOUND, exits 0s) — use pnpm.cmd, or run pnpm test / pnpm build separately in the
foreground (~40s/~90s). A SINGLE spec runs as `pnpm ng test --watch=false --include=<path>` —
`pnpm vitest run <file>` dies with "Need to call TestBed.initTestEnvironment() first". A git commit
whose message came from a PowerShell here-string failed twice with a lock error; -m and -F worked.
Still standing: lint any NEWLY ADDED spec; ContextStudio seeds itself on open (createStudio() clears
the proposal unless you pass {keepProposal:true}).


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 16 |
| Done | 9 |
| Claimed (unconfirmed) | 1 |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · DONE ✓ (confirmed) · BLOCKED · SKIPPED. Evidence = artifact path produced by a run this
phase (a code path is not evidence). Agent claims are marked DONE; engine confirms as DONE ✓.

### N0 — Truth batch - no-decision honesty fixes on Studio + MCP page

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N0.1 | Studio truth items: multi-entry merge preserves SourceLocations/Verified/Approx (§3.F.3); allocated_tokens no longer echoes budget (§3.F.4); Studio copy/save use the app's clipboard helper and toasts await outcome (§3.F.7) | DONE ✓ | 36bf916 | eval-results/2026-08-13/N0.1-studio-truth.md |
| N0.2 | MCP page truth items: status read no longer calls StartMcp (§3.F.9); snippet paths + copy-label fix (§3.F.10/11); feed totals respect the filter + wire timestamps (§3.F.12); sessions table renders the honesty fields so the shown age stops lying (§3.F.13); dead state deleted (§3.F.14) | DONE ✓ | 36bf916 | eval-results/2026-08-13/N0.2-mcp-truth.md |
| N0.3 | The §3.F inventory filed into BUG-BACKLOG.md as triaged bugs; spec smoke coverage exists for both pages (the three data-testids referenced by real specs) | DONE ✓ | 823de02 | eval-results/2026-08-13/N0.3-testid-coverage.txt |

### N1 — Studio truth pass + pins made real (owner decision 1: IMPLEMENT)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N1.1 | Verified/approx rendered per card; verification ledger verifies the pack actually built (wire item 4, mechanism chosen with a stated reason); state lifecycle fixed (per-tab keying or handle-effect invalidation; budget/intent/format persisted); body toggles wired or deleted | DONE ✓ | d57b59d | eval-results/2026-08-13/N1.1-studio-truth.md |
| N1.2 | Pins real end-to-end: `p` pins from Explore; TrailStore.pins() has real readers; pinned steps seed the pack; the three advertising surfaces (inspector, trail bar, ticker) tell the truth | DONE ✓ | e448d64 | eval-results/2026-08-13/N1.2-pins-real.md |

### N2 — Pack convergence - one pipeline, two faces (owner decision 2: FULL)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N2.1 | BuildMulti adopts the ResolveEntry path (symbol-rooted cards); `usage` joins CardTypeSections; picker gains a Types tab (LibrarySurface list) + D-G row identity (target member + route tail + project) | DONE ✓ | 104c9d0 | eval-results/2026-08-13/N2.1-pack-convergence.md |
| N2.2 | Honesty-note parity with get_context (fill-rate note + suggested focuses in the rail); budget default reconciled to one stated number; ACCEPTANCE: a FluentValidation pack composed from types, with usage and verified counts, end to end | DONE ✓ | aab7cf3 | eval-results/2026-08-13/N2.2-honesty-parity.md |

### M1 — Hygiene + Reader prerequisites (proto/mapper shopping list)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| M1.1 | Proto/mapper shopping list: TraceNode structured file_path+line_number; ReadSource file mode (or GetFileSource) with caps; per-file edge overlay query on the wire; ProtoMapper stops dropping MultiImplCount/DiHostCount/TestOnly/OmittedNames | DONE ✓ | a95d620 | eval-results/2026-08-13/M1.1-reader-prereqs.md |
| M1.2 | Hygiene: MapResponse.stack populated or its three consumers stop rendering it (bug filed either way); Layer/Feature lens slots hidden until data exists; createTab MAX_TABS lie fixed; dock resizer added; high-contrast theme selectable or removed | DONE ✓ | 7ccbf56 | eval-results/2026-08-13/M1-gate-red-lint-fix.md |

### N3 — Loop joints - routes into Studio + repo-file hand-off (owner decision 3)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N3.1 | Send-to-Studio from Explore (selection/trail/pins), Insights cards, and NodeCard; Studio default state = proposed pack from current trail+pins (never opens empty after exploration); archetype preset on fresh sessions | DONE | f427027 | eval-results/2026-08-14/N3.1-loop-joints.md |
| N3.2 | Save writes `.devcontext/packs/<slug>.md` (gitignored by default) + a copyable point-your-agent-here line for CLAUDE.md; Home's point-your-agent-here routes through Studio | TODO | - | - |

### N4 — MCP page rebuild - the observation deck (owner decision 4: full deck + ship binary)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N4.1 | Status that measures: binary fs probe; ObserverCount + last-agent-call-at on the wire and rendered; handshake = one real MCP tools/list round-trip shown; Start/Stop killed or renamed to what it does | TODO | - | - |
| N4.2 | Setup that works: devcontext-mcp ships in the Tauri bundle; snippets carry the resolved absolute path; write-config-for-me button per host | TODO | - | - |
| N4.3 | The catalog served: ListTools RPC (kills #4 structurally); page renders the curated described menu agents actually get (requires T1 merged in); feed keyed by MCP tool names (analyze wrapped, args digest, wire timestamps); rows deep-link — trace→Explore, get_context→replay-in-Studio | TODO | - | - |

### Z1 — Close-out: docs + backlog + README screenshot sync, full battery, push

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| Z1.1 | STUDIO-MCP-AUDIT statuses + DECISIONS.md (D-G settled by N2) + BUG-BACKLOG reconciled; PRE-RELEASE-PLAN §3 table updated for this run; full battery green; branch pushed | TODO | - | - |
| Z1.2 | README screenshot sync: docs/screenshots refreshed via the existing capture pipeline (screenshot-readme.mts / capture-readme.mts) against the post-N4 app — at minimum 08-context-studio, 09-export, 10-mcp plus any visibly changed page; README captions updated where the UI changed (agent-story claims untouched — engine Z1 owns those); committed and pushed | TODO | - | - |

## Dependencies

```
(none — stages run sequentially by plan order)
```
