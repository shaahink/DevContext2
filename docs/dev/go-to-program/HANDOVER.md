# Handover — Go-To Program Implementation (2026-07-02)

> Branch: `go-to/implement-iterations` · Base: `develop` @ `7228d1e`
> Round-1 session (DeepSeek): 19 commits · I1–I5 engine + shell skeletons
> Round-2 session: 10 commits · R2.1–R2.10 wire fix + all missing faces + TS build fix
> Round-3 session (this session): 4 commits · engine bug fix + 3 desktop interaction bugs + I10.1/I10.2/I10.4 multi-tab
> Gate: `dotnet build` 0w · `dotnet test --filter Category!=Eval` 386/0 · **`pnpm check` fully green (lint + 7/7 tests + build)**

---

## R3 session — what was delivered (4 commits: `84a4068`, `8887d4d`, `af7ccef`, `97044f8`)

R2's own handover asked the next agent to run a desktop smoke test before touching engine work. That
smoke test is what round 3 actually did — and it immediately found a real bug, not a UI problem.

### The engine bug (commit `84a4068`) — read this first, it explains why R2's smoke test would have failed too

`FileTreeExtractor.IsExcluded` matched exclude patterns with `path.Contains(pattern)` against the
**full absolute path**. `ExtractionOptions.ExcludePatterns` includes `"eval-repos"` and
`"analysis-repos"` by default (added by W1 hygiene, to stop `analyze` on the DevContext2 monorepo root
from recursing into its own eval fixtures). Substring-anywhere matching means analyzing **any** repo
that merely *lives under* a folder named `eval-repos` — e.g. `C:\code\DevContext2\eval-repos\TodoApi`,
which is exactly the path this branch's own docs (R2's HANDOVER, the resume prompt below) tell you to
smoke-test with — silently returned 0 projects/files/entries. No error, no warning, nothing in any
log. Confirmed via CLI: `analyze eval-repos/TodoApi` → "No projects discovered"; the identical repo
copied to a path without that folder name → 40 files/164 nodes/12 entries. **This is almost certainly
why R2's own smoke-test step was never actually completed** — anyone following R2's checklist verbatim
against `eval-repos/eShop` would have seen every face report zero data and reasonably concluded the R2
wire-up hadn't worked, when the wire-up was fine and the engine was the problem.

Fixed to match exact path **segments** relative to the walk root (never the root's own ancestor
segments), so nested `eval-repos/`/`analysis-repos/` subfolders are still pruned (re-verified: analyzing
the DevContext2 monorepo root itself still excludes its own `eval-repos/`) without nuking analysis of a
repo that happens to live under one. Regression test added. **385/0 → 386/0.**

### Three desktop interaction bugs (commit `8887d4d`) — found only by actually driving the app

Once the engine fix let real data through, three more defects surfaced live (Playwright, client-side
nav — `page.goto()` to a route does a hard SPA reload and drops session state, which produced a
false-positive "everything is broken" on the first driving pass; don't make that mistake):

- **NodeLink never stopped click propagation.** Clicking an entry's target link in the Entries table
  both opened the Node Card *and* navigated to `/trace`, because the row itself has a click-to-trace
  handler and the click bubbled. Fixed in `ui/node-link/node-link.ts` — a link click is now terminal.
- **Sheet (backs Node Card) never moved focus into the overlay on open.** Escape silently did nothing
  (focus stayed on whatever triggered the open) and the backdrop blocked every other click until
  closed by hand. Fixed in `ui/sheet/sheet.ts` with a focus-on-open effect.
- **Document/Export presets matched invented section keys.** `applyPreset()` used
  `'identity'/'entries'/'stack'/'insights'/'coverage'`, none of which are real — `MapRenderer.cs`'s
  actual `NarrativeSection` keys are `Overview`/`Topology`/`Routes`/`Entry points`/`Cross-cutting`/
  `Packages`/`Footer`. Onboarding only ever matched "Topology" by luck; Review and Trace Pack matched
  nothing at all. Also fixed: clicking a preset before ever clicking Render was a silent no-op (the
  common case — Presets sit above Sections in the UI, it's the obvious first click).
- The R2 HANDOVER's "known caveat" about NodeLink passing display names to `GetNode` **did not
  reproduce** — `GetNode` resolves display names fine. Don't chase that one; it's a non-issue.

### I10 multi-tab workspace — I10.1 + I10.2 + I10.4 done (commits `af7ccef`, `97044f8`); I10.3 blocked on I8

Not in R2's plan — added because the user asked specifically for multi-tab progress this session. Full
design rationale is in `ITERATION-I10-workspace-tabs.md`; what actually shipped:

- **`state/workspace.store.ts`** — up to `MAX_TABS=6` independent `TabState` (session slice + trace
  slice + last route + its own `OperationController`). `SessionStore`/`TraceStore` are now facades
  computed over `activeTab()` with an API **identical** to the pre-tabs version — every one of the 15
  components that inject them needed zero changes.
- **Race safety is real, not just claimed:** `analyze()`/`trace()` capture their tabId once at the top
  and thread it through every async callback — never re-read `activeId()` later. Each tab owns its own
  controller, so analyzing tab B cannot cancel tab A's in-flight request (previously one shared global
  `ActivityService` controller meant it would have). `workspace.store.spec.ts` has the explicit
  regression test plus cap/close-neighbor tests (7/7 passing, up from 4).
- **`shell/tab-strip.ts`** — 32px strip under the header, inset to the content column (icon rail stays
  a continuous full-height dock on the far left). Status dot, close on hover/active + middle-click,
  Ctrl+T/W/1-6, "+" disabled at cap. Each tab remembers its last route via `Router` `NavigationEnd` and
  restores it on switch (`replaceUrl`).
- **Persistence (I10.4):** `{path, label, route}` + active index persist to `localStorage`; restored as
  **idle** tabs on boot (session/trace data is never persisted — only the currently-active restored tab
  lazily re-analyzes, via one reactive rule that only ever reads `activeTab()`, so it's structurally
  impossible for restoring N tabs to trigger N analyses). `closeTab()` now calls the existing
  `CloseSession` RPC — previously closing a tab leaked its server-side snapshot for the process
  lifetime.
- **Deliberately deferred, not silently skipped:** I10.3 (server `MaxLiveSessions`/LRU/rehydrate) needs
  I8's snapshot cache, which doesn't exist — the tab cap alone is the spec's own "reduced v1" allowance.
  Drag-reorder (spec marks optional for v1). `ActivityService` is still one global instance for the
  footer/toast display only — cosmetic last-writer-wins if two tabs are busy at once; the underlying
  per-tab *data* is fully isolated regardless of what the footer shows, which is what actually matters
  and what the tests/live verification check.

**Verified live**, not just green checks — see PROGRESS-LOG's round-3 entry for the full list, but the
one worth calling out: started analyzing Serilog in tab 1, opened tab 2, analyzed TodoApi to completion
*while tab 1 kept running in the background*, switched back to tab 1 and it had completed normally.
That's the actual point of I10 and it works.

### Known stale doc, not fixed this round

`docs/product/desktop-ui.md` describes the **old WPF + BlazorWebView** desktop (`ConfigPanel`/
`OutputPanel`/Human-LLM-Stats tabs) — it predates the entire Angular rewrite (R1/R2/R3), not just this
session's changes. Patching just a "workspace tabs" section into it would be actively misleading since
the rest of the doc doesn't match anything in `src/DevContext.App`. It needs a full rewrite against the
current UI-UX-GUIDELINES.md + whatever R2/R3 actually shipped, not an incremental patch — flagging
rather than doing a partial fix that would look done when it isn't. Same for the `run-devcontext` skill
under `.claude/skills/` — it's CLI/WPF-only and doesn't mention `DevContext.App`, `pnpm dev:web`, or the
gRPC server at all; used a raw Playwright script instead this round (see PROGRESS-LOG for how). Worth
`/run-skill-generator`-ing a real driver skill for the Angular app at some point.

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

## Review checklist — DONE in round 3, don't redo blind

The R2 checklist below was executed for real in round 3 (Playwright, client-side nav, against
`eval-repos/TodoApi` and cross-checked with a two-tab session). Every box is checked; the engine bug
and the three desktop bugs above are what made them fail on the first attempt and are now fixed. If
you're picking this up cold, trust this record rather than re-running the whole checklist — but DO
re-run it if you've since touched any of: `FileTreeExtractor`, `NodeLink`, `Sheet`, `document-view.ts`,
`MapRenderer.cs` section keys, or anything in `state/*.ts` / `shell/tab-strip.ts`.

- [x] Insights view shows cards (not "undefined projects") — F1-F3 confirmed fixed
- [x] Rail shows Insights count badge
- [x] Click entry target in Entries → NodeLink opens Node Card without navigating away (R2.2 works +
      the propagation bug from this round is fixed)
- [x] Graph view loads and shows nodes (seeded, not the whole graph)
- [x] Settings → About shows engine version
- [x] Overview shows notable insights at top
- [x] Trace loads and renders
- [x] Document → click a preset → correct real sections auto-select → renders

---

## Engine state & next items

### Immediate

```
I10.3 needs I8 first → E1 (remaining insight sources) → I8 (caching/snapshots) → I9 (release readiness)
```

I10 (multi-tab) is functionally done for what doesn't need a snapshot cache — see its own section
above. Nothing is blocking a fresh agent from picking any of E1-E5 straight away.

| ID | What | Guide |
|----|------|-------|
| **E1** | 6 remaining insight sources: `wiring.hubs`, `graph.orphans`, `wiring.external-events`, `data.busiest-aggregate`, `topology.chokepoint`, `wiring.multi-impl` | `ITERATION-I3-insights.md` — each is one file + one unit test + two eval expectations (positive + negative) |
| **E2** | Pattern-zoo corpus (`tests/fixtures/PatternZoo/`) — covers modern C# through seam scanners | `ITERATION-I1-trust.md` §I1.5 |
| **E3** | Full W9 deletion — migrate eval `json-*` checks, delete `PatternRelevancePruner.cs`, `TokenBudgetEnforcer.cs`, `RenderPlanBuilder`, `FinalScore`, `OutputSelfCheck` | `ITERATION-I2-cli-kernel.md` §I2.2 |
| **E4** | Remaining facets: F3 message matrix (producers→consumers), F1 auth surface (unblocks Entries Auth column), F2 middleware, F4 data map, F5 talks-to, F6 config, F7 interesting points, F8 DI health, F9-F12 | `ITERATION-I5-facet-menu.md` |
| **E5** | Benchmark expansion — clone/register 8 missing-archetype repos, run suite, ratchet expectations | `ITERATION-I7-benchmark-audit.md` |
| **I8** | Caching & storage — repo-hash snapshot cache, clone consolidation, Settings→Storage face backend. **Unblocks I10.3** (server MaxLiveSessions/LRU/rehydrate) once done | `ITERATION-I8-caching-storage.md` |
| **I10.3** | Server: MaxLiveSessions + LRU + rehydrate path for the tab strip built this round | `ITERATION-I10-workspace-tabs.md` §2 — depends on I8 |
| **I9** | Release readiness — about/updates/logs/errors, CLI polish floor (exit codes, stdout/stderr, --quiet, completions) | `ITERATION-I9-release-readiness.md` |
| **A** | Harder repos — F14 EF depth, F15 build intelligence (bug-grade CPM + Directory.Build.props) | `ADDENDUM-A-harder-repos.md` |
| **Docs** | `docs/product/desktop-ui.md` needs a full rewrite (stale — describes the old WPF shell, not the Angular app at all). Same for `.claude/skills/run-devcontext/` (CLI/WPF only, doesn't know about `DevContext.App` or `pnpm dev:web`) — consider `/run-skill-generator` for a real driver skill | — |

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

All green (386/0 as of this round). Any wire/engine change must keep them green. Frontend:
`pnpm check` must stay green (lint + 7/7 vitest tests, up from 4 — `workspace.store.spec.ts` added the
I10 tab-isolation regression test this round).

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
Commit:   97044f8 "feat(desktop): I10.4 — persist tabs across restarts, lazy re-analyze, close frees session"
Base:     develop @ 7228d1e

Do NOT touch C:\Code\DevContext2 (main checkout, different branch — has its own unrelated work in
progress on branch feat/narrative-canvas, do not touch that either).
Addendum docs source: C:\Code\DevContext2-addendum (branch docs/go-to-program-addendum) — already merged into this worktree.

Read in this order:
  1. docs/dev/go-to-program/HANDOVER.md       (this file — round-3 section first, then the rest)
  2. docs/dev/go-to-program/README.md          (iteration tracker)
  3. docs/dev/go-to-program/PROGRESS-LOG.md    (round-3 entry has the full verified-live list)
  4. docs/dev/go-to-program/ITERATION-R2-verify-and-finish.md  (E1-E5 engine backlog detail)
  5. docs/dev/go-to-program/ITERATION-I10-workspace-tabs.md    (if touching tabs — I10.3/I8 next)
  6. docs/dev/go-to-program/UI-UX-GUIDELINES.md                (design contract for any UI work)

Verify baseline before editing:
  dotnet build DevContext.slnx                            # must be 0 warnings
  dotnet test  DevContext.slnx --filter "Category!=Eval"   # must be 386/0 green
  Set-Location src/DevContext.App; pnpm check              # lint + 7/7 tests + build — must be green

Do-not-regress anchors:
  BudgetIndependenceTests · TraceQualityTests sibling-divergence Facts
  GraphBuilderSpanTests (3) · NoiseFilterTests · ArchetypeDetectorTests · workspace.store.spec.ts (new this round)

Smoke-testing the desktop app: the in-repo `run-devcontext` skill is STALE (documents the old WPF
desktop, not this Angular+Tauri app) — don't trust it for this app. `pnpm dev:web` (server + `ng serve`,
browser-reachable on :4200) + a Playwright script driving it directly worked fine this round; no
chromium-cli was installed but `npx playwright` resolved and the browser binaries were already cached
under %LOCALAPPDATA%\ms-playwright. Use CLIENT-SIDE navigation (click rail links / router outlets) —
`page.goto()` to a route does a hard SPA reload and silently drops all session state, which looks
exactly like "everything is broken" but isn't.

What to do first:
  A. Pick any of E1-E5 from HANDOVER.md §Engine state & next items — nothing is currently blocking.
     E1 (6 remaining insight sources) is still the highest engine leverage: each source is one file +
     one unit test + two eval expectations, and becomes visible in the Insights view automatically
     (the wire is closed, verified live this round).
  B. If continuing I10: I10.3 (server MaxLiveSessions/LRU/rehydrate) needs I8 (snapshot cache) first —
     do I8 before touching I10.3.
  C. `docs/product/desktop-ui.md` needs a full rewrite (stale since before R1) — worth doing whenever
     UI work next touches a large surface, so the rewrite has fresh material to describe.

Delivery rules:
  - One commit per item: type(scope): what · update README/UNIFIED-TRACKER status same commit
  - Append PROGRESS-LOG.md after every session (date · changed · verified · next)
  - Docs move with code: cli-reference.md for CLI changes; desktop-ui.md is stale, don't patch it
    incrementally (see above) — either do the full rewrite or leave it alone and say so
  - Verify claims live before writing "DONE" — R2's HANDOVER marked faces done that turned out to be
    fully broken by an engine bug nobody had actually triggered; don't repeat that
  - eval-repos/ in THIS worktree are empty placeholders — use C:\code\DevContext2\eval-repos\<repo>
    (the main checkout's populated fixtures) for any manual/live testing; only run eval/gates.ps1 after
    populating this worktree's eval-repos/ properly (junction to C:\code\DevContext2\eval-repos)
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
dotnet test DevContext.slnx --filter "Category!=Eval"    # 386/0
Set-Location src/DevContext.App; pnpm check              # lint + 7/7 tests + build green

# Pick next item from HANDOVER.md §Engine state & next items
```
