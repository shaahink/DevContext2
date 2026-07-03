# AGENTS.md — DevContext Desktop UI worktree

You are in `C:\Code\DevContext2-ui` on branch `feat/ui-iteration`.
**Mission:** Build the live console, synced lens, facet views, and release polish for the desktop app.
You work ONLY in `src/DevContext.App/` — zero C# changes needed.

## Start here (every session)
1. Read this file — the work items below are ordered.
2. `docs/dev/PLAN-DESKTOP-V3.md` — the spec for P2 Live Console and P3 Synced Lens.
3. `docs/dev/go-to-program/UI-UX-GUIDELINES.md` — design contract for all UI work.
4. `docs/dev/go-to-program/ITERATION-I5-facet-menu.md` — facet specs (for U3).
5. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.

## Work items (do NOT skip order — each builds on the prior)

### U1 — Live Console (V3 P2) ✅ DONE
Stream engine `ProgressEvent`s as a scrolling boot-log. Settle into the RunReport on completion.
- Done: `workspace.store.ts` — `LogLine` type + `consoleLog` in `TabSessionSlice`. `session.store.ts` — appends progress events to `consoleLog` signal. New `section-console.ts` (boot-log + RunReport). Wired into `narrative-canvas.ts`.
- Gate: analyzing a repo shows live streaming log; after completion = readable report with funnel.

### U2 — Synced Lens (V3 P3) ✅ DONE
Single selection → Human pane + LLM pane side-by-side. Auto-render on selection via existing `Render` RPC.
- Done: `section-lens.ts` — persistent 50/50 split. Human pane (node detail + trace tree). LLM pane (auto-rendered markdown). Debounced render on `TraceStore.focus` change. Copy button + Ctrl+C shortcut. Wired into `narrative-canvas.ts` after trace section.
- Gate: pick any entry — both panes show it. Copy is one keystroke. Zero navigation.

### U3 — Facet views (E4 facets, UI-side only — engine must deliver E4 first) ⬜ BLOCKED
- F1 auth surface: add Auth column to Entries table (data comes from engine facet)
- F3 message matrix: producers→consumers table
- F4 data map, F5 talks-to, F8 DI health cards
- Gate: each facet renders real data from engine RPCs.

### U4 — Release UI polish (I9 UI side) ✅ DONE
- About section: version from ConnectionStore, server status dot, GitHub + issues + releases links, privacy note.
- Error telemetry hardening: audited all 34 catch sites. Fixed `palette.ts` swallow comment, added toast to `node.store.ts` failures, added toast to `node-card.ts` clipboard failures. Audit confirms no truly swallowed user-facing errors remain.
- Updates panel: "Check updates" link to GitHub Releases in About section.
- Gate: about shows real version; force an RPC error = toast appears.

### U5 — Workspace navigation + polish ✅ DONE
Navigation rail + routed views replacing single-page scroll. Entries table sorting/keyboard nav. Palette entry/node results. Keyboard shortcuts.
- Done: `shell/navigation-rail.ts` — left sidebar with icon+label navigation. `shell/workspace-shell.ts` — header + rail + router-outlet + footer, `g+key` view nav, `?` help overlay. `app.config.ts` — 8 lazy-loaded routes. `features/pages/` — overview, entries, trace, graph, insights, export page wrappers. `section-entries.ts` — sortable columns, arrow-key nav, row actions. `palette.ts` — entry search top 10, stale routes removed.
- Gate: clicking rail items navigates views; sort headers work; Ctrl+K palette searches entries; `?` shows shortcuts.

### I11 — Focus Workspace (unified context-thinking shell) ⬜ SUPERSEDED — see F below
**Spec:** `docs/dev/go-to-program/ITERATION-I11-focus-workspace.md`
**Gaps:** `docs/dev/GAP-TRACKER.md` (23 gaps — closed by the F0-F7 fixes below, rest carried into F's waterfall)
**Flow explainer:** `docs/dev/FEATURE-FLOW-EXPLAINER.md` (for LLM context)

I11 was synthesized from two independent proposals (C, CT) but was itself superseded by a third
("F proposal", `docs/dev/briefs/ui-ux-redesign-proposal-fable.md`) before implementation started —
see that doc's §11 for the diff. Kept here for history; do not resume I11 directly, resume **F**.

### F — Fable Workbench Redesign (active — supersedes I11) 🔶 IN PROGRESS
**Branch:** `feat/fable-redesign-skeleton` · **Spec:** `docs/dev/briefs/ui-ux-redesign-proposal-fable.md`
**Handoff:** `docs/dev/briefs/fable-skeleton-HANDOFF.md` (file-by-file risk notes for the skeleton)

Same "one workbench, one selection, one trail" redesign as I11 aimed for, but with an exact design
system (Graphite tokens), a named cancellation architecture (LatestGate), and client-side derived
insights (Flow Atlas) that I11 left unspecified. Executed as a waterfall W0→W7 (proposal §10) —
**do not start Wn+1 until Wn's gate passes.**
- W0 (design tokens) — **done**. Graphite palette + `@layer components` vocabulary in `styles.css`;
  Inter/JetBrains Mono bundled locally (`public/assets/fonts`, zero remote font requests, verified);
  `ThemeService` — turned out to already exist pre-branch and already implement "data-theme alongside
  data-vibe" with vibes as accent remaps, so nothing new was built there; 6 new `ui/` primitives
  (Skeleton, Meter, SeamChip, KindIcon, EmptyState, Ticker); `/styleguide` dev route (isDevMode()-gated,
  confirmed tree-shaken from prod builds). Along the way: fixed a real bug where `trace-node.ts`'s own
  stale seam-color map never matched the wire's actual values (every seam chip, old and new routes,
  was silently falling back to gray) — it now uses `SeamChip`.
- W1 (shell skeleton) — **done**. `titlebar.ts`/`activity-bar.ts`/`statusbar/statusbar.ts` replace
  `header/app-header.ts`/`navigation-rail.ts`/`footer/app-footer.ts` (30/48px-wide/22px, no `fixed`
  positioning — normal document flow, the old `calc(100vh - ...)` height math is gone). `tab-strip.ts`
  finally wired in (GAP-T1 — it was fully built, just never imported); `shortLabel` fixed (GAP-T3);
  MRU stack + Ctrl+Tab (GAP-T5). Activity bar: registry-safe icons (GAP-S7 — three rail items had been
  rendering with literally no icon this whole time), count badges (GAP-S6), disabled-visible+tooltip
  (GAP-S5). `offline-banner.ts` (live-verified: kill the server, banner+red dots appear within one poll
  cycle, clear on revive). `core/webview-shortcuts.service.ts` intercepts Ctrl+P/Ctrl+R/F5/Ctrl+F/zoom
  (§7.3) — Ctrl+R does a real light re-analyze instead of reloading (verified), full focus-restore
  after is deferred. `/atlas` stub route. The activity bar deliberately still shows all 7 old items
  (Overview/Entries/Trace/Graph/Insights/Export/Settings) rather than collapsing to the proposal's
  eventual 5-icon layout — that collapse assumes `/explore` has already absorbed Entries/Trace/Graph,
  which is a W4 cutover, not a W1 one; collapsing now would make working pages nav-undiscoverable.
- W2 (component build) — **done, blended with W4** (see note below). `entry-deck`, `stage`,
  `inspector`, `trail-bar`, `audit-table` all exist, wired into a real `/explore` route with real
  store data (ahead of spec — normally W2 stays store-free until W4; this project already decided in
  W1/W2 to skip that intermediate store-free stage, since it would mean building each component
  twice). Not built: `mini-map` (thumbnail for tree mode) — low value, skipped; `node-peek` — spec
  itself defers this to W7.
- W3 (state/RPC hardening) — **done**. `LatestGate` (`core/rpc-call.ts`) threads through
  `TraceStore.trace()`/`selectNode()` (keys `${tabId}:trace`/`${tabId}:node`) and the omnibox's node
  search (key `'search'`), all abort-on-supersede; `TrailStore`/`AtlasStore` fully built (not just
  skeletons — `AtlasStore` already computes `topFlows`/`hubs`/`eventWiring`/`reachedBy`, pulled ahead
  of its nominal W5 slot since the indexer needed those shapes anyway); `SessionStore` has the
  duplicate-path guard (GAP-T4); `PrefsStore` has `dockLevel`. Analyze-stream cancel sweep audited
  and confirmed complete (`OperationController` + `WorkspaceStore.closeTab` + the trace/atlas
  tab-close effects) — no gap found, no code needed.
- W4 (the great wiring) — **6 of 9 checkpoints done**, each its own commit, each `pnpm check` green,
  each verified live via headless Chrome (playwright, `channel: 'chrome'`) against a real analyzed
  repo (`tests/fixtures/MinimalApiProject`) — not just lint/build. Full narrative + every bug found
  during verification: `docs/dev/go-to-program/PROGRESS-LOG.md`'s "W3 remainder + W4 partial" entry.
  Done: **omnibox live** (replaces Palette entirely — GAP-B1-B5, C2 closed); **stage altitudes live**
  (System topology graph, Node List/Graph + out/in/usages toggle, Flow dblclick "re-trace" — which
  turned out to need a client-side `TraceStore.reroot()` instead of a fresh `GetTrace`, see below);
  **workbench URL state** (`?focus&view&kind&q`) **+ Esc-ladder** (cancel trace → close overlay →
  deselect node → clear focus → clear filter) **+ `p`/Alt+←→ shortcuts**; **inspector LLM section**
  migrated to the Render RPC (was raw `trace.markdown()`); **audit table overlay** (Shift+E, replaces
  the standalone entries page as an overlay). Remaining for the W4 gate:
  1. **Export drawer** (Ctrl+E): section toggles + Onboarding/Flow-Review/Full/From-Trail presets,
     porting `section-export.ts`'s render logic; "From Trail" renders each `TrailStore.pins()` step
     via the Render RPC, concatenated, tokens summed.
  2. **Home page assembly**: identity strip, Top Flows card row (`AtlasStore.topFlows()` — already
     computed), insight headline row, run report — restyled card-free. Console during analysis stays
     as-is.
  3. **Atlas page assembly**: map markdown (prose-zone), topology graph (`GraphCanvas` `topology`
     mode — already built in checkpoint 3, just needs binding here too), packages/pipeline list. Since
     `AtlasStore.eventWiring()`/`hubs()` already exist (pulled ahead in W3), surface them here too
     instead of stubbing to W5 — near-zero incremental cost.
  4. **Route cutover**: make `/entries` `/trace` `/graph` redirect into `/explore` (preserving
     `?focus` where present); `/overview` redirect to `/` (Home). Delete
     `section-entries`/`-trace`/`-graph`/`-lens`/`-export` + their standalone pages
     (`entries-page`/`trace-page`/`graph-page`) + `SectionCard`, once nothing references them (grep
     first — `styleguide-page.ts` may still demo some primitives independently).
  5. Full manual gate sweep per proposal §10's W4 table (flows A-E walked end-to-end, deep links land
     traced, deleting files broke nothing, kill-server-mid-trace degrades correctly).
- W5-W7 — not started. Note that `AtlasStore`'s W5 data layer (topFlows/hubs/eventWiring/reachedBy)
  already exists from W3 — W5 itself is now mostly "bind it to Home/Atlas/Inspector," which the W4
  remainder above is already starting to do opportunistically. Inspector's "Reached by N flows" line
  and Insights-for-selection section were deliberately left for W5 proper (see PROGRESS-LOG) even
  though the data exists, to keep the W4 gate meaning something.
- **Notable deviation from the spec, discovered by hand, not from docs:** `GetTrace`'s `focus` param
  only resolves registered entry-point keys (e.g. `"POST /orders"`) — passing a raw internal graph
  node id (e.g. `Member:Foo.<lambda>`) comes back `found: false`, confirmed against the live server.
  So "double-click any graph node → re-trace from it" (proposal §2) is implemented as a **client-side
  re-root** (`TraceStore.reroot()`, finds the node inside the tree already loaded and redisplays that
  subtree) rather than a fresh `GetTrace` call — the proposal implies zero engine changes are needed
  for this feature but doesn't explain how; this is the how. Depth is capped at whatever was already
  fetched relative to the original root. `NeighborsRequest`/`NodeRequest` (used by `selectNode`/
  `getNode`), by contrast, DO accept arbitrary node ids fine — only `GetTrace`'s focus resolution is
  this restrictive.
- Known pre-existing bug noticed in passing, NOT fixed (out of scope): `section-console.ts`
  tracks its `@for` by `line.timestamp` (`Date.now()`) — two progress events landing in the same
  millisecond collide (`NG0955` console warning, harmless but real). Worth a look whenever that file
  is next touched (it may not survive the W4 route cutover anyway).
- **Playwright verification tip for whoever resumes:** the first headless Chrome launch immediately
  after a `pnpm check` sometimes hits a `<vite-error-overlay>` intercepting clicks — `ng serve` (still
  running) and `pnpm check`'s own one-shot `ng build` likely contend over `.angular/cache`. A bare
  retry of the identical script always passed clean in this session. Don't dismiss an overlay that
  survives a retry, but don't chase one that only appears once either.
- Gate for resuming: `pnpm check` green (real exit code) → pick up the W4 remainder above, starting
  with the export drawer.

## Verify loop
```powershell
# From C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # lint + vitest tests (27 as of this writing — check current) + build — must be GREEN
pnpm server         # start .NET server (separate terminal)
pnpm dev:web        # start Angular dev server → http://localhost:4200
```

## Hard rules
- **No C# changes** — you don't need the engine worktree. If a face needs data the kernel can't answer, document it and move on. The engine agent fills the gap.
- **TypeScript only:** `src/DevContext.App/src/app/**`
- **pnpm check green** before every commit.
- Append `PROGRESS-LOG.md` after every session.

## Resume protocol (cold start)
```
git -C C:/Code/DevContext2-ui checkout feat/ui-iteration
git -C C:/Code/DevContext2-ui pull

# Verify baseline
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check

# Pick the first work item whose Status != DONE in this file
# Do Step 0 (reproduce) first, then execute
```

To resume the **F — Fable Workbench Redesign** specifically (the active redesign track):
```
git -C C:/Code/DevContext2-ui checkout feat/fable-redesign-skeleton
git -C C:/Code/DevContext2-ui pull
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check > check.log; echo $LASTEXITCODE   # real exit code, never pipe to tail
```
Then read `docs/dev/briefs/ui-ux-redesign-proposal-fable.md` §10 (the waterfall) — W0-W3 are done, W4
is 6/9 checkpoints done. Pick up the W4 remainder in this order: export drawer (Ctrl+E) → Home page
assembly → Atlas page assembly → route cutover + deletion → full manual gate sweep. See this file's
"F — Fable Workbench Redesign" section above for exactly what each remaining item needs, and
`docs/dev/go-to-program/PROGRESS-LOG.md`'s latest entry for the bugs already found/fixed and the
`vite-error-overlay` retry gotcha when Playwright-verifying a fresh change.

For each remaining item: implement → `pnpm check` green → verify live (start `pnpm server` +
`pnpm dev:web`, drive it with a headless Chrome via `playwright`, `channel: 'chrome'`, against
`tests/fixtures/MinimalApiProject` — see recent commits for smoke-script examples, none are kept in
the repo) → commit → append a PROGRESS-LOG.md entry → move to the next item. One commit per
checkpoint, same discipline as the W4 commits already on this branch.

---

# AGENTS.md — DevContext Engine worktree

You are in `C:\Code\DevContext2-engine` on branch `feat/engine-iteration`.
**Mission:** Deliver E2 (pattern-zoo), E5 (benchmark expansion), I8 (snapshot cache), I10.3 (server LRU),
A-F14/F15 (harder repos), and I9 (release readiness engine side).
You work in `src/DevContext.Core/**`, `tests/**`, `src/DevContext.Cli/**`, `src/DevContext.Server/**` —
**zero TypeScript changes needed.**

## Start here (every session)
1. Read this file — work items below are ordered by dependency.
2. `docs/dev/go-to-program/HANDOVER.md` — round-1/2/3 delivery summary + engine state section.
3. `docs/dev/go-to-program/PROGRESS-LOG.md` — last entry has unified-iteration-1 summary.
4. `docs/dev/go-to-program/ITERATION-I1-trust.md` — §I1.5 for E2 pattern-zoo.
5. `docs/dev/go-to-program/ITERATION-I7-benchmark-audit.md` — E5 benchmark expansion.
6. `docs/dev/go-to-program/ITERATION-I8-caching-storage.md` — I8 snapshot cache.
7. `docs/dev/go-to-program/ITERATION-I10-workspace-tabs.md` — I10.3 server LRU (depends on I8).
8. `docs/dev/go-to-program/ADDENDUM-A-harder-repos.md` — A-F14/F15 harder repos.
9. `docs/dev/go-to-program/ITERATION-I9-release-readiness.md` — I9 release readiness.

## Work items (ordered — each builds on the prior; I10.3 is blocked until I8 done)

### E2 — Pattern-zoo corpus  **DONE**
- Locus: `tests/fixtures/PatternZoo/PatternZoo/` (9 fixture files) + `tests/DevContext.Core.Tests/PatternZooTests.cs` (13 tests).
- Also shipped I1.3 (conjunction gate for bare-verb fallback) + I1.5 (string literal stripping in GraphBuilder).
- Gate: `PatternZooTests` green (13/0); all existing seam tests still green (369/0).

### E5 — Benchmark expansion
Clone 8 missing-archetype repos → register in `eval-repos.json` + `eval/expectations/` → capture Map+Trace.
- Locus: `eval-repos.json`, `eval/expectations/*.json`
- Archetypes: CLI, Worker, gRPC, Blazor, MAUI/Avalonia, classic MVC, serverless, 2nd library.
- Gate: `dotnet test --filter Category=Eval` green with new expectations.

### I8 — Caching & storage
Repo-hash snapshot cache → instant re-opens. Settings→Storage backend.
- Locus: new cache service in `src/DevContext.Core/Analysis/`, wire through DI.
- Gate: analyze same repo twice → second run near-instant from cache.

### I10.3 — Server MaxLiveSessions + LRU + rehydrate  **DONE**
- Locus: `src/DevContext.Server/Sessions/EngineRunner.cs` — cache-hit rehydration before analysis.
- Gate: server checks I8 snapshot cache before analysis; cache hit → instant EngineResult from cached snapshot.

### A-F14 — EF depth tracking  **DONE**
Entity relationship depth analysis (entity→aggregate root distance).
- Locus: `src/DevContext.Core/Graph/GraphBuilder.cs` — `AddEntityNavigationEdges()` + `ExtractInnerEntityNameWithDir()`
  `src/DevContext.Core/Graph/TraceBuilder.cs` — `AnnotateEntityDepths()`
  `src/DevContext.Core/Graph/CodeGraph.cs` — `EdgeKind.EntityRelation`
- Gate: entity navigation relationship tests green; TOUCHES annotated with chain depth.

### A-F15 — Build intelligence  **DONE**
CPM detection + Directory.Build.props fix (bug-grade: CPM packages not detected).
- Locus: `src/DevContext.Core/Resolvers/CsprojReader.cs` — `ResolveCpmVersions()`, `ParsePackageReferencesCpmAware()`, `ResolveOutputType()`, `ResolveTargetFrameworks()`, `ResolveIsPackable()` with ancestor-chain walking.
- Gate: CPM fixture project; `CsprojReaderCpmTests` (12 tests) green; all existing tests green (381/0).

### I9 — Release readiness (engine side)  **DONE** (CLI exit codes + --quiet)
CLI polish: exit codes, `--quiet`, stdout/stderr separation, completions.
- Locus: `src/DevContext.Cli/Settings/AnalyzeSettings.cs`, `src/DevContext.Cli/Commands/AnalyzeCommand.cs`
- Gate: `--strict` returns exit code 2 on invariant fail; `--quiet` prints nothing on success.

## Verify loop
```powershell
# From C:/Code/DevContext2-engine
dotnet build DevContext.slnx                             # 0 warnings (analyzer warnings = errors)
dotnet test  DevContext.slnx --filter "Category!=Eval"   # must be green (383/0 as of I9)
powershell -File eval/gates.ps1                          # full gate (needs populated eval-repos/)
```

## Hard rules
- **No TypeScript changes** — you work in the engine. The UI agent handles desktop.
- **Reform in place; never rewrite extractors.**
- **Do-not-regress anchors:** `BudgetIndependenceTests`, `TraceQualityTests` sibling-divergence Facts,
  `GraphBuilderSpanTests`, `NoiseFilterTests`, `ArchetypeDetectorTests`.
- **Docs move with code, same commit.** Update `docs/product/cli-reference.md` for CLI changes.
- **One wire contract:** anything a face shows must exist as `GraphQuery` op / kernel JSON first.

### Before running eval/gates.ps1
`eval-repos/` in this worktree may be empty. Junction to the populated copy:
```powershell
New-Item -ItemType Junction -Path C:\Code\DevContext2-engine\eval-repos -Target C:\Code\DevContext2\eval-repos
```

## Resume protocol (cold start)
```
git -C C:/Code/DevContext2-engine checkout feat/engine-iteration
git -C C:/Code/DevContext2-engine pull

# Pre-flight
dotnet build C:/Code/DevContext2-engine/DevContext.slnx
dotnet test C:/Code/DevContext2-engine/DevContext.slnx --filter "Category!=Eval"

# Ensure eval-repos junction exists (see above)

# Pick the first work item whose Status != DONE in this file
# Do Step 0 (reproduce) first, then execute. Commit per item.
```
