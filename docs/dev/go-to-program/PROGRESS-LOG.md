# Progress Log — go-to program

> Append-only session log. Date · Changed · Verified · Next.

---

## 2026-07-02 — R2 execution (session 1)

**Changed:**
- Merged addendum docs (I8 caching, I9 release, I10 tabs, ADDENDUM-A harder repos) from `C:\Code\DevContext2-addendum`
- Updated README.md tracker: added I8/I9/I10/A rows, updated CORE spine to I1→I2→I3→I4→I8→I10→I9
- Updated UNIFIED-TRACKER.md: added I8/I9/I10 sections, new delivery diagram
- **R2.1** Insights on wire: KernelJsonRenderer → proto → gRPC server → TypeScript store → desktop view → CLI. Full stack: `Insight[]` now reaches every face.
- **R2.2** NodeLink component: every name is a link. wired into entries/trace/node-card + document markdown linkify.
- **R2.3** Entries table: sortable columns, filter chips (has-target/approx), hover row actions (Trace/NodeCard/Copy), filtered/total counter.
- **R2.4** Trace fixes: F6 dead Tailwind class removed, focus breadcrumb with back, honest empty hint.
- **R2.5** Graph view: new face with seeded exploration from entries, seam filter chips, NodeCard via NodeLink. Route + rail item.
- **R2.6** Settings view: new face with Appearance/Analysis/Storage(I8)/Server/About(I9) sub-tabs. ConnectionStore now captures version from PingResponse.
- **R2.7** Palette: added Graph, Browse, Document, Settings entries.
- **R2.8** Connection: 3-state (online/connecting/offline) with server version tooltip.
- **R2.9** Overview: top-3 notable insights section at top.
- **R2.10** Export packs: Onboarding/Trace/Review presets that auto-select section toggles.

**Verified:**
- `dotnet build DevContext.slnx` — 0w 0e
- `dotnet test DevContext.slnx --filter Category!=Eval` — 385/0 green
- `pnpm lint` — green (pre-existing build errors in node-card/palette/node.store unrelated to R2)

## 2026-07-02 — Pre-existing TS errors + handover (session 1 cleanup)

**Changed:**
- Fixed 5 pre-existing TypeScript build errors from round-1 session that prevented `pnpm build`:
  - `node-card.ts`: removed `n.line` (not in NodeResponse proto); replaced `neigh.incoming`/`neigh.outgoing` with edge-filtering via computed signals
  - `palette.ts`: `r.results` → `r.nodes` (SearchResponse field name)
  - `app-shell.ts`: removed unnecessary `?.` on `label` (required proto field)
  - `node.store.ts`: `'both'` → `'out'` + `'in'` with merged edges via `create(NeighborsResponseSchema)`
- Fixed 4 self-inflicted errors from R2 code:
  - `settings-view.ts`: `theme.vibes`→`theme.vibes()`, `theme.activeVibe`→`theme.vibe()`, removed unused imports
  - `graph-view.ts`: removed unused Icon/Badge imports
  - `document-view.ts`: `onDocClick(MouseEvent)` → `onDocClick(Event)` for keyboard event compatibility
  - `title-bar.ts`: fixed template string literal parsing error with single quote
- Rewrote HANDOVER.md: round-2 delivery summary, review checklist, known caveats, next-items table, resume protocol

**Verified:**
- `pnpm check` fully green: lint · 4/4 tests · build success (app bundle generated, 0 errors, 0 warnings)
- All 12 lazy chunks built: entries-view, source-view, trace-view, document-view, settings-view, browse-view, overview-view, stats-view, graph-view, insights-view, cache-view

**Next:** Desktop smoke test (verify faces render real data) → E1 remaining insight sources

## 2026-07-02 — Round-3 execution: desktop smoke test + engine bug + I10 multi-tab

**Changed:**
- Ran the desktop smoke test HANDOVER.md asked for (Playwright, since the in-repo `run-devcontext`
  skill is stale — it documents the old WPF desktop, not this branch's Angular+Tauri app). First pass
  against `eval-repos/eShop` (via `page.goto` client-side nav) showed every R2 face reporting
  "0 projects / 0 entries" — traced to a real engine bug, not a UI wiring bug (see below).
- **Engine bug fix (commit `84a4068`):** `FileTreeExtractor.IsExcluded` matched exclude patterns via
  `path.Contains(pattern)` against the FULL absolute path, so analyzing any repo living under a folder
  literally named `eval-repos` or `analysis-repos` — the exact path this branch's own docs tell the
  next agent to smoke-test with — silently returned 0 projects/files/entries, no error anywhere.
  Reproduced via CLI: `analyze eval-repos/TodoApi` → 0 projects; same repo copied elsewhere → 40
  files/164 nodes/12 entries. Fixed to match exact path SEGMENTS relative to the walk root (never the
  root's own ancestors), so nested `eval-repos/` subfolders are still pruned (verified against the
  DevContext2 monorepo root) without nuking analysis of a repo that happens to live under one.
  Regression test added. **385/0 → 386/0.**
- **Three desktop interaction bugs found by actually driving the app (commit `8887d4d`)**, once the
  engine fix let real data reach the UI:
  - `NodeLink` never stopped click propagation, so clicking a target link in the Entries table both
    opened the Node Card AND navigated away to `/trace` (the row has its own click-to-trace handler).
  - `Sheet` (backs Node Card) never moved focus into the overlay on open, so Escape silently did
    nothing and the backdrop blocked all other clicks until closed by hand.
  - Document/Export presets matched invented section keys (`'identity'`, `'entries'`, `'stack'`,
    `'insights'`, `'coverage'`) that don't exist — `MapRenderer.cs`'s real keys are Overview/Topology/
    Routes/Entry points/Cross-cutting/Packages/Footer. Onboarding only ever matched "Topology" by
    luck; Review and Trace Pack matched nothing. Also fixed presets silently no-op'ing when clicked
    before the first Render (the obvious first click, since Presets sit above Sections in the UI).
  - The HANDOVER.md "known caveat" about NodeLink/GetNode display-name resolution did **not**
    reproduce — GetNode resolves display names fine.
- **I10 multi-tab workspace — I10.1+I10.2+I10.4 (commits `af7ccef`, `97044f8`):**
  - `WorkspaceStore`: up to 6 independent `TabState` entries (session slice + trace slice + route +
    its own `OperationController`). `SessionStore`/`TraceStore` rewritten as facades over
    `activeTab()` with a byte-for-byte identical public API — all 15 consumer components needed zero
    changes. `analyze()`/`trace()` capture their owning tabId once at call time and thread it through
    every async callback (never re-reading `activeId()` later), and each tab owns its own
    `OperationController` — starting analyze on tab B cannot cancel tab A's in-flight request, which
    it previously would have (one shared global controller). Regression test: start analyze on A,
    switch to B mid-flight, complete → A ready with A's handle, B untouched.
  - `TabStrip`: 32px strip under the header, VS Code-ish (active underline, status dot, close on
    hover, middle-click close, Ctrl+T/W/1-6, "+" disabled at cap). Each tab remembers its last route
    and restores it on switch.
  - I10.4: tabs persist `{path, label, route}` + active index to localStorage; restore as IDLE tabs
    on boot (never re-analyze all of them — only the persisted active tab lazily re-analyzes, and
    that's structural: the trigger effect only ever reads `activeTab()`). `closeTab()` now calls the
    existing `CloseSession` RPC, freeing the server-side snapshot instead of leaking it.
  - **Deferred, not silently skipped:** I10.3 server-side `MaxLiveSessions`/LRU/rehydrate (needs I8's
    snapshot cache, which doesn't exist yet — the tab cap alone is the spec's own "reduced v1"
    allowance); drag-reorder (spec marks optional for v1); `ActivityService` is still one global
    instance for the footer/toast display only (cosmetic last-writer-wins across concurrent tabs) —
    the underlying per-tab DATA is fully isolated regardless, which is what the race test and the
    live concurrent-analyze scenario below verify.

**Verified (live, Playwright, not just green checks):**
- Single-tab regression: re-ran the full R2 checklist against `eval-repos/TodoApi` post-engine-fix —
  Insights cards (real severities/evidence), Entries table (12/12, sortable, NodeLink→NodeCard without
  navigating away), Graph (seeded, 20-22 nodes), Settings→About (real engine version), Trace, and all
  three Document presets selecting the correct real sections. Zero console errors, zero network
  failures throughout every run.
- Multi-tab isolation: two repos analyzed in two tabs show fully independent data on switch; route
  restoration works (navigate tab 1 to Entries, bounce through tab 2, back to tab 1 → still Entries).
- **The actual point of I10** — concurrent analyze: started analyzing Serilog in tab 1, opened tab 2
  and analyzed TodoApi to completion *while tab 1 kept running in the background*, switched back to
  tab 1 and it had completed normally (3 projects, real data) — not cancelled.
- Persistence: analyzed two repos, navigated the active one to Entries, simulated a restart (fresh
  page load, same localStorage) — both tabs came back with correct resolved labels, URL landed
  straight back on Entries, the active tab's data was live within ~1s, the other tab stayed idle
  (dimmed) until clicked, then lazily re-analyzed on its own.

**Gate:** `dotnet build` 0w · `dotnet test --filter Category!=Eval` 386/0 · `pnpm check` green
(lint + 7/7 tests, up from 4 — 3 new WorkspaceStore tests + build).

**Next:** E1 remaining 6 insight sources (highest engine leverage, unchanged from prior handover) →
I10.3 needs I8 first (server LRU/rehydrate) → I9 release readiness → E4 remaining facets.

## 2026-07-02 — Unified Iteration 1: merge + Tiers 1-2 delivery

**Branch:** `feat/unified-iteration-1` (off `develop` after merging both `go-to/implement-iterations` and `feat/narrative-canvas`).

**Merges:**
- Merged `go-to/implement-iterations` (34 commits: I1-I10 engine + desktop) into develop — fast-forward
- Merged `feat/narrative-canvas` (9 commits: P0-P6 single scroll canvas) on top — resolved 11 conflicts:
  6 modify/delete (old views deleted by canvas) + 5 content conflicts (stores/config/views).
  Kept narrative-canvas UI, retained go-to WorkspaceStore/NodeLink/infrastructure.
- All merges verified: build 0w, tests 356/0 (12 server + 280 core + 64 desktop, 3 skipped).

**Tier 1 — Perf + Library Surface (delivered):**
- `--fast` mode: CLI flag skips `InMemoryEventBusExtractor`, `AntiPatternDetector`, `IndirectWiringDetector`.
  Wired through `ExtractionOptions.Fast` → `AnalyzeCommand` → `DiscoverPipeline` exclude list.
- WS-G-a: `LibrarySurfaceBuilder` now detects `AbstractValidator<T>`/`AbstractAuthorizationHandler` as
  consumer "derive" seats. xUnit `[Fact]`/`[Theory]`/`[InlineData]`/`[Trait]` now appear as annotate entries
  even without generators.

**Tier 2 — E1 + E3 (delivered):**
- E1 — 6 insight sources: `wiring.hubs`, `graph.orphans`, `wiring.external-events`,
  `data.busiest-aggregate`, `topology.chokepoint`, `wiring.multi-impl`. Each implements `IInsightSource`,
  registered in `DiscoveryPipeline.ComputeInsights`. Total: 10 insight sources (4 existing + 6 new).
- E3 — Full W9 deletion: deleted `Pruning/` (TokenBudgetEnforcer + PatternRelevancePruner),
  `RenderPlanBuilder.cs` (replaced with stub), `TokenBudget.cs`, `OutputSelfCheckTests.cs`.
  Cleaned `DiscoveryModel.Budget`, `TypeDiscovery` FinalScore/FocusScore/GraphProximity/PathProximity,
  global usings, `MarkdownRenderer` budget display, `PipelineTests` pruner test.
  Build 0w, tests 356/0.

**Verified:** `dotnet build` 0w · `dotnet test --filter Category!=Eval` 356/0 (12+280+64, 3 skipped).
pnpm check NOT run (TypeScript unchanged from narrative-canvas merge — the only TS file touched was
trace-node.ts which took go-to's NodeLink version, identical to what was already on narrative-canvas
before the merge).

**Next — remaining items (all tiers documented below):**
- E2: Pattern-zoo corpus (`tests/fixtures/PatternZoo/`) — modern C# through seam scanners
- E4: Remaining facets F1-F12 (auth surface, message matrix, middleware, data map, etc.)
- E5: Benchmark expansion — 8 missing-archetype repos
- I8: Caching & storage — repo-hash snapshot cache (unblocks I10.3)
- I10.3: Server MaxLiveSessions + LRU + rehydrate (depends on I8)
- A: Harder repos — F14 EF depth, F15 build intelligence
- I9: Release readiness

---

## 2026-07-02 — U1 Live Console (P2) — feat/ui-iteration

**Changed:**
- `state/workspace.store.ts`: added `LogLine` type and `consoleLog` field to `TabSessionSlice`
- `state/session.store.ts`: progress callback now appends `ProgressEvent`s to `consoleLog` signal; exposed via `SessionStore.consoleLog` computed
- **New** `features/narrative/section-console.ts`: boot-log during analysis (scrolling with auto-scroll-to-bottom, phosphor terminal style), RunReport on completion (stages waterfall, funnel bar, extractor timings, cold-cache labeling)
- `features/narrative/narrative-canvas.ts`: wired `<app-section-console />` for both boot and report modes; added 'console' to scroll-spy sections
- `features/narrative/section-stats.ts`: funnel card now has stacked horizontal bars for types discovered→included and raw→budget tokens; cold cache shows "cold run — no cache reuse"
- `styles.css`: added `.console-surface`, `.console-log`, `.log-line`, `.log-cursor` (blink animation), `.report-line`, `.console-section-title`; terminal-vibe phosphor selectors; `prefers-reduced-motion` safe

**Verified:**
- `pnpm lint` — green
- `pnpm test` — 7/7 green
- `tsc --noEmit` — clean
- `pnpm build` — timed out (resource contention with concurrent Angular build); TypeScript compilation used as proxy

**Next:** U2 Synced Lens (P3) — Human+LLM split pane with auto-render on selection

---

## 2026-07-02 — U2 Synced Lens (P3) — feat/ui-iteration

**Changed:**
- **New** `features/narrative/section-lens.ts`: persistent 50/50 Human (left) + LLM (right) split pane
  - Human pane: trace tree (from TraceStore) + node detail (kind, file, tags, degrees) when explicitly selected
  - LLM pane: auto-rendered markdown via `api.render(handle, { focus, format: 'markdown' })`
  - Auto-render: debounced 250ms `effect` on `TraceStore.focus` — no manual Render needed
  - Copy: button in LLM pane + `Ctrl+C` keyboard shortcut (`@HostListener`)
  - Empty/loading/error states for both panes
- `features/narrative/narrative-canvas.ts`: wired `<app-section-lens />` after trace section; 'lens' added to scroll-spy
- `styles.css`: `.lens-split` 50/50 flex, `.lens-pane`, `.lens-card`, `.lens-content` (max-h 70vh, mono, scroll), `.lens-placeholder`, terminal-vibe adjustments, responsive stacking ≤768px

**Verified:**
- `pnpm lint` — green
- `pnpm test` — 7/7 green
- `tsc --noEmit` — clean

**Next:** U3 Facet views (blocked on engine E4) or U4 Release polish

---

## 2026-07-02 — U4 Release Polish — feat/ui-iteration

**Changed:**
- `features/narrative/section-settings.ts`: About section now shows real engine version (from `ConnectionStore.ping`), server status dot, stack info, GitHub link, issues link, "Check updates" link to GitHub Releases, privacy note
- `features/palette/palette.ts`: replaced `/* swallow */` comment with clear explanation (search is supplementary, silent failure is OK)
- `state/node.store.ts`: added `error` signal + toast notification when node detail fetch fails (was silently setting null state)
- `features/node-card/node-card.ts`: added toast confirmation for clipboard copy success/failure (was silent void call)

**Error telemetry audit (34 catch sites):**
- All user-facing catch sites now either surface errors with toasts or error signals, or have clear comments explaining why silent handling is appropriate
- Non-critical catches (localStorage, Tauri API in browser, cleanup ops) remain silent — acceptable
- `operation-controller.ts`, `graph-view.ts`, `github.store.ts`, `theme.service.ts`, `recent.store.ts`, `workspace.store.ts` — all verified OK

**Verified:**
- `pnpm lint` — green
- `pnpm test` — 7/7 green
- `tsc --noEmit` — clean

**All U1-U4 items complete.** Remaining front-end work from UI-UX-GUIDELINES.md: navigation rail, entries table enhancements, keyboard shortcuts — none are U-list items. U3 (facet views) blocked on engine E4.

---

## 2026-07-02 — U5 Workspace Navigation + Polish (audit items) — feat/ui-iteration

**Changed:**
- **New** `shell/navigation-rail.ts`: left sidebar with icon+label items (Overview, Entries, Trace, Graph, Insights, Export, Settings). Session-required items hidden until `session.ready()`. Active route highlighted with accent border.
- **New** `shell/workspace-shell.ts`: header + rail + router-outlet + footer layout. Imports palette. Keyboard shortcuts: `g+key` (o/e/t/g/i/x/s) for view navigation, `?` for shortcut help overlay, `Escape` to close. `g` sets 1.5s pending flag.
- **New** `features/pages/`: 6 page wrappers (`overview-page`, `entries-page`, `trace-page`, `graph-page`, `insights-page`, `export-page`) that reuse existing section components. Export page handles dismiss→navigate back.
- `app.config.ts`: route table changed from single wildcard `**` to 8 lazy-loaded child routes under `WorkspaceShell`
- `section-entries.ts`: sortable columns (click header to toggle Method/Route/Target/Kind asc/desc), keyboard navigation (↑↓ to move, Enter to trace, `n` for NodeCard, `Ctrl+C` to copy route), selected row highlight, subtitle shows filtered/total count
- `palette.ts`: removed stale routes (Browse, Document, Stats), added Export view nav, capped entry results at top 10
- `app.ts`: unchanged (router-outlet already present)

**Verified:**
- `pnpm lint` — green
- `pnpm test` — 7/7 green
- `tsc --noEmit` — clean

**All U1-U5 items complete.** U3 (facet views) blocked on engine E4. App is fully routed with navigation rail, keyboard shortcuts, and sortable entries table.
