# Handover — Go-To Program Implementation (2026-07-02)

> Branch: `go-to/implement-iterations` · Base: `develop` @ `7228d1e`
> Round-1 session (DeepSeek): 19 commits · I1–I5 engine + shell skeletons
> Round-2 session (this session): 10 commits · R2.1–R2.10 wire fix + all missing faces + TS build fix
> Gate: `dotnet build` 0w · `dotnet test --filter Category!=Eval` 385/0 · **`pnpm check` fully green (lint + tests + build)**

---

## R2 session — what was delivered (10 commits)

### Defects fixed (the "everything was marked DONE but nothing worked" bugs)

| Finding | What | How |
|---------|------|-----|
| **F1** | Insights computed but never serialized — died in snapshot | Added `Insight` type chain: KernelJsonRenderer → devcontext.proto → ProtoMapper → gRPC handler → StatsResponse |
| **F2** | `insights-view.ts` called non-existent `store.lastStats()` | Added `_stats`/`_statsError`/`_statsLoading` signals to SessionStore, fetch after analyze, expose `stats()` + `lastStats()` |
| **F3** | insights-view read phantom fields (`projectCount`, `entryTargetRatio`, `seamCounts`) from AnalysisSummary | Full rewrite of insights-view — renders real `Insight[]` cards grouped by category with severity colors, evidence chips, coverage bar, Engine drawer, empty/loading/error triad |
| **F4** | Insights route orphaned — not in rail or palette | Added to LENS_ITEMS with count badge, palette entry |
| **F5** | No NodeLink primitive — every name was dead text | Created `ui/node-link/node-link.ts` component, wired into entries-view (targets), trace-node (titles), node-card (neighbors), document-view (markdown linkify) |
| **F6** | Dead Tailwind class `ml-[calc({{depth()}}*20px)]` in trace-node | Removed — nested `border-l-2 border-line pl-3` provides indentation |
| **F7** | Connection dot binary with no tooltip | 3-state (online/connecting/offline) with server version tooltip via ConnectionStore.version |
| **F8** | Stats fetch failure swallowed silently | `_statsError` signal + retry button + `refreshStats()` method |

### New faces built (zero → functional)

| Face | Spec | Status |
|------|------|--------|
| **Graph view** | UI-UX §5 — seeded from entry nodes via GetNode+GetNeighbors, seam filter chips, NodeCard via NodeLink | Route + rail item + component |
| **Settings view** | UI-UX §7 — Appearance (vibes), Analysis (depth/detail/Roslyn), Storage (I8 paths), Server (status/port), About (I9 version/privacy/GitHub links) | Route + rail item (always accessible, not session-gated) |
| **Entries table** | UI-UX §4 — sortable columns (Route, Target resolved-first, Kind), hover row actions (Trace/NodeCard/Copy), has-target/approx filter chips, filtered/total counter, keyboard Enter→trace | Rewrote entries-view |

### Existing faces upgraded

| Face | What changed |
|------|-------------|
| **Trace** | Fixed F6, added focus breadcrumb (history stack with back), honest-empty hint when focus resolves but root has no out-edges |
| **Palette** | Added Graph, Browse, Document, Settings entries for Ctrl+K |
| **Overview** | Added top-3 notable insights section (severity >= Notable per I3 spec) |
| **Export/Document** | Added Onboarding/Trace/Review pack presets that auto-select section toggles, conservative markdown linkify for entry titles |
| **Connection store** | Captures server version from PingResponse for Settings About panel |
| **devcontext-api.ts** | `ping()` now returns `{ ready, version }` instead of bare boolean |

### Pre-existing TS build errors fixed (from round-1 session)
These shipped as "green" in round-1 but `pnpm build` failed on them:

| File | Error | Fix |
|------|-------|-----|
| `node-card.ts:27` | `n.line` doesn't exist on `NodeResponse` | Removed — show just `filePath` |
| `node-card.ts:41-53` | `neigh.incoming`/`neigh.outgoing` don't exist | Filter `neigh.edges` by direction via computed signals |
| `palette.ts:83` | `r.results` doesn't exist on `SearchResponse` | Changed to `r.nodes` |
| `app-shell.ts:91` | `s.label?.split` — `?.` on required string | Removed `?.` |
| `node.store.ts:26` | `'both'` not a valid `NeighborDirection` | Two calls (`'out'` + `'in'`), merge edges via `create(NeighborsResponseSchema)` |

---

## Review checklist (for the next agent)

Before continuing to engine work, verify the desktop faces actually render live data:

1. **`pnpm dev` or `pnpm dev:web`** — launch the app
2. **Analyze a known-good repo** (e.g. `C:\code\DevContext2\eval-repos\eShop` or any local .NET solution)
3. **Check each R2-delivered face:**
   - [ ] Insights view shows cards (not "undefined projects") — proves F1-F3 are fixed
   - [ ] Rail shows Insights count badge
   - [ ] Click entry target in Entries → NodeLink opens Node Card — proves R2.2 works
   - [ ] Graph view loads and shows nodes
   - [ ] Settings → About shows engine version
   - [ ] Overview shows notable insights at top
   - [ ] Trace breadcrumb back button works
   - [ ] Document → click a preset → sections auto-select → render
4. **Known caveat:** Entry targets pass display names (`"OrderController"`) as `nodeId` to NodeLink/NodeStore. `NodeStore.show()` calls `GetNode` which may fail on display names vs graph node IDs. If Node Cards don't open from entry targets, the fix is to resolve target names to node IDs in the proto or store layer.

---

## Engine state & next items

### Immediate (after desktop smoke test passes)

```
E1 (remaining insight sources) → I8 (caching/snapshots) → I10 (workspace tabs) → I9 (release readiness)
```

| ID | What | Guide |
|----|------|-------|
| **E1** | 6 remaining insight sources: `wiring.hubs`, `graph.orphans`, `wiring.external-events`, `data.busiest-aggregate`, `topology.chokepoint`, `wiring.multi-impl` | `ITERATION-I3-insights.md` — each is one file + one unit test + two eval expectations (positive + negative) |
| **E2** | Pattern-zoo corpus (`tests/fixtures/PatternZoo/`) — covers modern C# through seam scanners | `ITERATION-I1-trust.md` §I1.5 |
| **E3** | Full W9 deletion — migrate eval `json-*` checks, delete `PatternRelevancePruner.cs`, `TokenBudgetEnforcer.cs`, `RenderPlanBuilder`, `FinalScore`, `OutputSelfCheck` | `ITERATION-I2-cli-kernel.md` §I2.2 |
| **E4** | Remaining facets: F3 message matrix (producers→consumers), F1 auth surface (unblocks Entries Auth column), F2 middleware, F4 data map, F5 talks-to, F6 config, F7 interesting points, F8 DI health, F9-F12 | `ITERATION-I5-facet-menu.md` |
| **E5** | Benchmark expansion — clone/register 8 missing-archetype repos, run suite, ratchet expectations | `ITERATION-I7-benchmark-audit.md` |
| **I8** | Caching & storage — repo-hash snapshot cache, clone consolidation, Settings→Storage face backend | `ITERATION-I8-caching-storage.md` |
| **I10** | Workspace tabs — multi-session WorkspaceStore, 32px tab strip, VS Code-grade interactions | `ITERATION-I10-workspace-tabs.md` |
| **I9** | Release readiness — about/updates/logs/errors, CLI polish floor (exit codes, stdout/stderr, --quiet, completions) | `ITERATION-I9-release-readiness.md` |
| **A** | Harder repos — F14 EF depth, F15 build intelligence (bug-grade CPM + Directory.Build.props) | `ADDENDUM-A-harder-repos.md` |

### E1 insight sources are the highest-leverage next step
They become automatically visible in the desktop Insights view (R2.1 already wired the full chain). Each source adds one Insight subclass + one eval check. The 6 remaining:

1. `wiring.hubs` — degree outlier over production types (topology)
2. `graph.orphans` — public types with zero in-edges, not DI-registered, not entries (likely dead code)
3. `wiring.external-events` — consumed event types minus produced (external contracts)
4. `data.busiest-aggregate` — entity with most domain event raises
5. `topology.chokepoint` — most-depended-upon project
6. `wiring.multi-impl` — interfaces with multiple DI registrations

---

## Do-not-regress anchors

```
BudgetIndependenceTests · TraceQualityTests sibling-divergence Facts
GraphBuilderSpanTests (3) · NoiseFilterTests · ArchetypeDetectorTests
```

All green (385/0). Any wire/engine change must keep them green.

## Docs to maintain (same-commit rule)

- `docs/product/cli-reference.md` — update with `--stats` insights section, `query` ops
- `docs/product/desktop-ui.md` — update with Insights, Graph, Settings sections
- `docs/dev/go-to-program/README.md` — tracker status column
- `docs/dev/go-to-program/UNIFIED-TRACKER.md` — per-item status
- `docs/dev/go-to-program/PROGRESS-LOG.md` — append after every session

## Where to work

Worktree: `C:\Code\DevContext2-goto-audit` · Branch: `go-to/implement-iterations`
Do not touch `C:\Code\DevContext2` (main checkout, different branch).
Addendum docs sourced from `C:\Code\DevContext2-addendum` (branch `docs/go-to-program-addendum`) — already merged.

## Resume protocol (cold start)

### Warm-start prompt — paste this into the next agent session

```
You are resuming the DevContext go-to program.

Worktree: C:\Code\DevContext2-goto-audit
Branch:   go-to/implement-iterations (tracks origin/go-to/implement-iterations)
Commit:   f927e7a "docs(handover): complete round-2 handover"
Base:     develop @ 7228d1e

Do NOT touch C:\Code\DevContext2 (main checkout, different branch).
Addendum docs source: C:\Code\DevContext2-addendum (branch docs/go-to-program-addendum) — already merged into this worktree.

Read in this order:
  1. docs/dev/go-to-program/HANDOVER.md       (this file — review checklist, caveats, next items)
  2. docs/dev/go-to-program/README.md          (iteration tracker)
  3. docs/dev/go-to-program/PROGRESS-LOG.md    (what happened in prior sessions)
  4. docs/dev/go-to-program/ITERATION-R2-verify-and-finish.md  (plan for remaining engine items E1-E5)
  5. docs/dev/go-to-program/UI-UX-GUIDELINES.md                (design contract for any UI work)

Verify baseline before editing:
  dotnet build DevContext.slnx                            # must be 0 warnings
  dotnet test  DevContext.slnx --filter "Category!=Eval"   # must be 385/0 green
  Set-Location src/DevContext.App; pnpm check              # lint + test + build — must be green

Do-not-regress anchors:
  BudgetIndependenceTests · TraceQualityTests sibling-divergence Facts
  GraphBuilderSpanTests (3) · NoiseFilterTests · ArchetypeDetectorTests

What to do first:
  A. Desktop smoke test (verify R2 faces render live data):
     pnpm dev   # launch desktop
     Analyze a known-good .NET repo (e.g. eval-repos/eShop)
     Check: Insights cards show real data · rail badge has count · Entry target NodeLink opens NodeCard
            Graph loads · Settings>About shows version · Overview shows top insights · Trace breadcrumb works

  B. If smoke test passes → next item is E1 (remaining 6 engine insight sources).
     Each source: 1 Insight subclass + 1 unit test + 1 positive eval check + 1 negative.
     Guide: docs/dev/go-to-program/ITERATION-I3-insights.md
     Sources become visible in Insights view automatically (R2.1 closed the wire).

  C. After E1 → I8 (caching/snapshots) → I10 (workspace tabs) → I9 (release readiness).
     Full delivery order in HANDOVER.md §Engine state & next items.

Delivery rules:
  - One commit per item: type(scope): what · update README/UNIFIED-TRACKER status same commit
  - Append PROGRESS-LOG.md after every session (date · changed · verified · next)
  - Docs move with code: cli-reference.md for CLI changes, desktop-ui.md for UI changes
  - No screenshot required (user waived this rule for R2)
  - eval-repos/ must be populated before running eval/gates.ps1 (junction to C:\code\DevContext2\eval-repos)
```

### Manual cold-start commands

```
git -C C:/Code/DevContext2-goto-audit checkout go-to/implement-iterations
git -C C:/Code/DevContext2-goto-audit pull

# Read in order:
docs/dev/go-to-program/HANDOVER.md
docs/dev/go-to-program/README.md
docs/dev/go-to-program/PROGRESS-LOG.md
docs/dev/go-to-program/ITERATION-R2-verify-and-finish.md
docs/dev/go-to-program/UI-UX-GUIDELINES.md

# Verify baseline:
dotnet build DevContext.slnx                            # 0w
dotnet test DevContext.slnx --filter "Category!=Eval"    # 385/0
Set-Location src/DevContext.App; pnpm check              # lint + test + build green

# Pick next item from HANDOVER.md §Engine state & next items
```
