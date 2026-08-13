# DevContext pre-release - desktop agent loop Phase Tracker

**Plan:** DevContext pre-release - desktop agent loop | **Branch:** `feat/pre-release-desktop` | **Design doc:** docs/dev/research/PRE-RELEASE-PLAN-2026-08-13.md

## Handoff (overwrite this block, ≤12 lines, no history)

N1 IS CLOSED - N1.2 landed (e448d64 + 366cc3a; evidence eval-results/2026-08-13/N1.2-pins-real.md).
START AT N2.1 (BuildMulti adopts ResolveEntry; `usage` joins CardTypeSections; picker Types tab).
Pins are REAL: ContextStudio.onTrailSeed() (context-studio.ts:481) reads trailStore.pins() first
and falls back to steps(). THE RULE N2/N3 MUST REUSE, MEASURED HERE: a seed resolves step.focus
against the LIVE session.entryGroups() and takes the RESOLVED entry's nodeId+title, never the
pinned step's - so a pin cannot carry a dead id across a re-analyze and needs NO invalidation
effect. Every step kind resolves now (a `node` step carries its trace's focus), reroot never can.
N3.1's "never opens empty" should call onTrailSeed, not re-derive seeding. Backlog #26 is under
"FIXED in N1.2"; 27 open, 7 high. New run bug #3: icon.ts renders an EMPTY span for a name its
REGISTRY lacks (box/edit/grip-vertical/lock still dead; bookmark+history added).
TEST LOOP (unchanged): `pnpm exec ng test --watch=false --include=<spec>`; plain `pnpm vitest run`
fails on TestBed.initTestEnvironment. Check the EXIT CODE, not filtered output - an escaped
apostrophe in an Angular expression is a lexer error and cost a build here. `pnpm check` green
this session: lint clean, 188/188, production build. Engine untouched by N1.2.
`conductor bg start -- pnpm <script>` dies on corepack; wrap in `pwsh -NoProfile -Command`.
Open bugs unchanged: #1 (negative budget for the last focus), #2 (eval stamp cache never hits).


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 16 |
| Done | 3 |
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
| N1.1 | Verified/approx rendered per card; verification ledger verifies the pack actually built (wire item 4, mechanism chosen with a stated reason); state lifecycle fixed (per-tab keying or handle-effect invalidation; budget/intent/format persisted); body toggles wired or deleted | DONE | d57b59d | eval-results/2026-08-13/N1.1-studio-truth.md |
| N1.2 | Pins real end-to-end: `p` pins from Explore; TrailStore.pins() has real readers; pinned steps seed the pack; the three advertising surfaces (inspector, trail bar, ticker) tell the truth | TODO | - | - |

### N2 — Pack convergence - one pipeline, two faces (owner decision 2: FULL)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N2.1 | BuildMulti adopts the ResolveEntry path (symbol-rooted cards); `usage` joins CardTypeSections; picker gains a Types tab (LibrarySurface list) + D-G row identity (target member + route tail + project) | TODO | - | - |
| N2.2 | Honesty-note parity with get_context (fill-rate note + suggested focuses in the rail); budget default reconciled to one stated number; ACCEPTANCE: a FluentValidation pack composed from types, with usage and verified counts, end to end | TODO | - | - |

### M1 — Hygiene + Reader prerequisites (proto/mapper shopping list)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| M1.1 | Proto/mapper shopping list: TraceNode structured file_path+line_number; ReadSource file mode (or GetFileSource) with caps; per-file edge overlay query on the wire; ProtoMapper stops dropping MultiImplCount/DiHostCount/TestOnly/OmittedNames | TODO | - | - |
| M1.2 | Hygiene: MapResponse.stack populated or its three consumers stop rendering it (bug filed either way); Layer/Feature lens slots hidden until data exists; createTab MAX_TABS lie fixed; dock resizer added; high-contrast theme selectable or removed | TODO | - | - |

### N3 — Loop joints - routes into Studio + repo-file hand-off (owner decision 3)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N3.1 | Send-to-Studio from Explore (selection/trail/pins), Insights cards, and NodeCard; Studio default state = proposed pack from current trail+pins (never opens empty after exploration); archetype preset on fresh sessions | TODO | - | - |
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
