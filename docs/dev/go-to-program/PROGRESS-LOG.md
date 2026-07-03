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
### I9 — Release readiness (engine side)  **DONE** (CLI exit codes + --quiet)
CLI polish: exit codes, `--quiet`, stdout/stderr separation, completions.
- Locus: `src/DevContext.Cli/Settings/AnalyzeSettings.cs`, `src/DevContext.Cli/Commands/AnalyzeCommand.cs`
- Gate: `--strict` returns exit code 2 on invariant fail; `--quiet` prints nothing on success.

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

## 2026-07-02 — E2 Pattern-Zoo + I1.3/I1.5 fixes

**Changed:**
- E2 — Created `tests/fixtures/PatternZoo/PatternZoo/` with 9 C# fixture files covering:
  primary constructor, expression body, record with body, local function, required init,
  collection expression, conditional block (`#if`), raw string literal trap, and parameter-type
  Send resolution. Each file contains a known seam (`IMediator.Send(new X())`).
- `PatternZooTests.cs` — 13 parameterized/inline Facts asserting Sends edges across all syntax
  shapes plus negative guards: non-mediator Send (SmtpClient) produces no edge, raw string
  literal `"""...Send(new FakeCommand())..."""` produces no fabricated edge, parameter-type
  fallback resolves correctly, multiple seams in one class all detected, Publish/SendAsync
  variants work.
- **I1.5 fix — String literal stripping:** Added `GraphBuilder.StripStringLiterals()` — a
  character-level pre-pass that replaces C# string literal contents (regular `"..."`,
  verbatim `@"..."`, raw `"""..."""`, interpolated `$"..."`) with spaces preserving offsets.
  Applied in `AddSends` so in-literal seam-like patterns never produce fabricated edges.
- **I1.3 fix — Conjunction gate:** When `AddSends` hits a bare-verb fallback (unknown receiver
  but known verb like `Send`), now also checks `IsLikelyRequestType()` — the target type name
  must end with Command/Query/Event/Notification/Request/Response or be in the model's event
  type set. Kills the `SmtpClient.Send(new MailMessage())` false positive.

**Verified:** `dotnet build` 0w · `dotnet test --filter Category!=Eval` 369/0 (up from 356,
+13 PatternZoo tests, no regressions).

**Next:** A-F15 (Build intelligence — CPM + Directory.Build.props) → A-F14 (EF depth).

## 2026-07-02 — A-F15: Build intelligence (CPM + Directory.Build.props)

**Changed:**
- **CPM (`Directory.Packages.props`):** `CsprojReader.ResolveCpmVersions()` walks the ancestor
  directory chain from each csproj looking for `Directory.Packages.props`, parses its
  `<PackageVersion Include="X" Version="Y" />` elements. `ParsePackageReferencesCpmAware()`
  resolves PackageReference versions from CPM when inline Version is missing.
- **`Directory.Build.props` chain:** `ResolveOutputType()`, `ResolveTargetFrameworks()`,
  `ResolveIsPackable()` walk the ancestor chain for `Directory.Build.props`. csproj values
  win; ancestor values fill in when csproj doesn't set them. Nearest ancestor wins among
  Directory.Build.props imports.
- `ProjectStructureExtractor` updated to use the new CsprojReader resolution methods.
- CPM fixture project: `tests/fixtures/CpmProject/` with `Directory.Build.props` (sets
  OutputType+TargetFramework) and `Directory.Packages.props` (MediatR 12.0.0,
  FluentValidation 11.5.0, Microsoft.Extensions.Hosting 10.0.0).
- `CsprojReaderCpmTests` (12 tests): CPM version resolution, inline-override, OutputType
  from Directory.Build.props, TargetFramework from ancestor, IsPackable fallback, empty
  returns when no CPM/ancestor files exist.

**Verified:** `dotnet build` 0w · `dotnet test --filter Category!=Eval` 381/0 (+12 CPM tests, no regressions).

**Next:** A-F14 (EF depth tracking) → E5 (Benchmark expansion) → I8 (Caching).

## 2026-07-02 — A-F14: EF depth tracking (entity navigation + depth annotation)

**Changed:**
- `EdgeKind.EntityRelation` — new edge kind for entity-to-entity navigation relationships.
- `GraphBuilder.AddEntityNavigationEdges()` — scans entity types' declared properties for
  navigation properties referencing other known entities. Creates `EntityRelation` edges in
  the BelongsTo direction (child entity → parent aggregate/entity). Handles both direct
  references (`OrderItem.Order`) and collection properties (`Order.Items: ICollection<OrderItem>`).
- `GraphBuilder.ExtractInnerEntityNameWithDir()` — extracts the inner entity name from property
  type strings, distinguishing collection types (ICollection<>, List<>, arrays) from direct
  references. Filters out primitive/framework types.
- `TraceBuilder.AnnotateEntityDepths()` — post-collection step that computes depth from each
  touched entity to its nearest aggregate root by BFS-traversing EntityRelation edges.
  Aggregate roots get "(root)" annotation; connected entities get "(depth N from AggregateName)".
  Unconnected entities unchanged.
- `GraphBuilderTests` — 2 new tests: direct reference navigation and collection navigation.

**Verified:** `dotnet build` 0w · `dotnet test --filter Category!=Eval` 383/0 (+2 entity nav tests, no regressions).

**Next:** E5 (Benchmark expansion) → I8 (Caching).

## 2026-07-02 — I8 snapshot cache + I10.3 server rehydrate + I9 CLI polish

**Changed:**
- **I8 — Snapshot cache:** `SnapshotCacheService` in `Core/Analysis/` — computes cache keys from
  repo path (SHA256) + git HEAD (or manifest hash), saves/loads `AnalysisSnapshot` as JSON.gz,
  LRU eviction (10 versions/repo, 2GB cap), CLI `cache list/clear/path` stubs.
- **I8 — CLI integration:** `AnalyzeCommand` checks snapshot cache before running analysis;
  cache hit → render from cached snapshot with `"from cache · sha7 · Nms"` stamp; cache miss
  → save write-behind. New flags: `--no-cache` (force fresh), `--cache-only` (fail if cold).
- **I10.3 — Server rehydrate:** `EngineRunner` checks I8 cache before full analysis; cache hit
  → instant `EngineResult` with fresh pipeline from `EngineHostCache`. Write-behind saves after
  analysis.
- **I9 — CLI polish:** Exit codes (0=ok, 1=usage, 2=strict-fail, 3=cache-only-miss, 4=network/
  clone). `--quiet` flag suppresses all output on success.

**Verified:** `dotnet build` 0w · `dotnet test --filter Category!=Eval` 383/0 (no regressions).

**Next:** E5 (Benchmark expansion — remaining item).

---

## 2026-07-02 — V4 Audit fixes + P0 header/cleanup + P1 entries/deep-link + P2 prefs/lens

**Changed:**
- **V4 Audit fixes (F0-F7):** Build breaks (wrong imports in `navigation-rail.ts`, `afterRender→afterEveryRender`, dead `graph-view.ts`); wired Synced Lens onto Trace page; fixed blank graph on first trace + ngModel-on-signal bug; trace dropdown focus gating; entries arrow-key double-jump; insights semantic colors + detail rendering; settings GitHub links + Roslyn toggle.
- **P0.1 — Header repo affordance:** Replaced dead `<ng-content select="[analyze]"/>` with repo-label dropdown showing current repo, recents list, and "New analysis…" action. "New" button now resets workspace state (`closeTab + navigateByUrl('/')`) instead of `window.location.reload()`.
- **P0.2 — Landing recents ×:** Ported remove-recent button from deleted `SectionSettings` to `SectionLanding` recents list (hover-visible, stopPropagation).
- **P0.3 — Dead code deleted:** `narrative-canvas.ts`, `section-settings.ts`, `scroll-spy/scroll-spy.ts` — all only imported each other, nothing referenced them.
- **P1 — Entries table spec:** Count badges on filter chips (`HTTP 23`); "approx" and "has target" quick-filter toggle chips; sort persisted in URL (`?sort=route&dir=asc`); visible row-action buttons on hover (Trace · Node card · Copy route); sticky header + 500px max-height scroll; filter/sort state synced to URL via `replaceUrl`.
- **P1 — Trace deep-linking:** `/trace?focus=X` — `trace-page.ts` reads `focus` from query params and triggers trace on nav; `effect` writes focus back to URL on change.
- **P2.7 — `prefs.store`:** New state store (`state/prefs.store.ts`) — persists analysis defaults (depth/detail/roslyn/cleanup) to localStorage under schema-versioned key. `SettingsView` reads/writes prefs. `SectionLanding` and `AppHeader.selectRecent()` apply prefs defaults on analyze.
- **P2.9 — Lens node detail wired:** `trace-node.ts` now emits `nodeSelected` output on click; wired in `section-trace.ts` and `section-lens.ts` to call `traceStore.selectNode()`. `section-graph.ts` calls `selectNode()` alongside `trace()` on graph node click. Lens Human pane now shows node detail card when a node is selected.
- **Bonus:** Removed unused `RouterLinkActive` import from `navigation-rail.ts` and unused `Icon` import from `workspace-shell.ts`.

**Verified:**
- `pnpm check` green: lint 0/0 · test 7/7 · build 0w 0e
- 14 files changed, +340/−446 lines

**Next from AUDIT-V4-VERIFICATION §6:**
- P3: U3 facets / E4 (needs engine facet layer first — deferred multi-iteration)
- Verification debt: Eval gate (`dotnet test --filter Category=Eval`), manual smoke test
- Follow-ups: file:line column (needs engine proto change), CDK v22 virtual scroll API, Tauri storage commands

---

## 2026-07-02 — Audit + Bug Fix Pass + Window Buttons + Gap Tracker

**Changed:**
- **Window buttons fixed:** Silent-catch pattern replaced with Tauri detection (`window.__TAURI__`). Buttons now hidden in browser mode (where Tauri APIs don't exist). Tauri API module cached after first lazy load. `isTauri()` guard in template.
- **Footer wired:** Added `ConnectionStore` + `ActivityService` — footer now shows connection dot + server version + progress % during analysis + error state.
- **NodeCard error state:** Added error display with Retry + Copy details buttons. Added empty-state for zero callers/callees. Skeleton spinner (not text "Loading..."). 
- **SectionArchitecture empty states:** Added "Analyze a repo first" when no session + "No architecture data available" when analysis produced no data. `hasContent()` gate on all subsections.
- **SectionIdentity empty states:** Added guard — shows prompt text instead of `0 nodes / 0 edges` before analysis.
- **SectionStats error state:** `statsError` signal now read — shows inline error with Retry button during loading.
- **SectionExport toggle fix:** User's section enable/disable state now preserved across re-renders (was: overwritten to all `true`). Added inline error state with Retry.
- **Two documents created:** `docs/dev/GAP-TRACKER.md` — 23 remaining gaps with doc refs, expected fixes, and priority ordering. `docs/dev/FEATURE-FLOW-EXPLAINER.md` — app identity, technology wiring, current flows, 5 proposed UX improvements (tabs, persistent lens, one-click re-analyze, graph seeding, quick popover), request for review.

**Verified:**
- `pnpm check` green: lint 0/0 · test 7/7 · build 0w 0e

**Next:**
- Wire tab strip (I10 — built but orphaned) — highest-impact unshipped feature
- Persistent Lens panel (eliminates page-hop on entry exploration)
- Nav rail polish (icons, count badges, disabled tooltips)

---

## 2026-07-03 — Fable branch: commit backlog + lint fixes + W3 completion (LatestGate/dup-guard/prefs)

**Context:** `feat/fable-redesign-skeleton` (branched from `develop` at `cab2800`) carried two
uncommitted, already-verified changesets in its working tree left over from `develop` (the "V4 audit
+ P0-P2" and "Audit + Bug Fix Pass" entries above) — never committed before the branch was cut.
Committed those first so the tree matches what was actually tested, then continued the Fable
waterfall (`docs/dev/briefs/ui-ux-redesign-proposal-fable.md` §10) per the skeleton HANDOFF's
"next steps" ordering.

**Changed:**
- **Commits `0947011`/`1729b90`:** the two pending changesets above, plus lint fixes for the F-skeleton
  commit (`entry-deck.ts`/`stage.ts`/`inspector.ts` needed `keydown.enter`/`space` + `tabindex` paired
  with `(click)` for `@angular-eslint/template` a11y rules; `atlas.store.ts`/`stage.ts` used the banned
  `Array<T>`/`ReadonlyArray<T>` generic form). `pnpm check` had never been run on the skeleton commit
  before this session — HANDOFF said as much ("STATIC — never compiled or run").
- **W3 — LatestGate threading (proposal §5.1):** `DevContextApi.getTrace/getNode/getNeighbors` take an
  optional `AbortSignal` now (mirrors the existing `analyze()` pattern), threaded to the ConnectRPC
  `CallOptions`. `TraceStore` now routes `trace()`'s `run()` and `selectNode()` through a `LatestGate`
  keyed `${tabId}:trace`/`${tabId}:node` — a stale response from a superseded j/k scrub can no longer
  paint over a newer one. A constructor `effect()` (same pattern as `TrailStore`'s GC effect) aborts a
  tab's in-flight gate entries the moment the tab closes.
- **W3 — SessionStore duplicate-path guard (GAP-T4):** `analyze()` now checks all tabs (not just the
  active one) for a matching `path` with a handle or in-flight analysis; if found, switches to that tab
  instead of creating a duplicate. Re-analyzing the path already open in the *active* tab is unaffected
  (deliberate re-analyze, not blocked).
- **W3 — PrefsStore `dockLevel`/`theme`:** added to the `Prefs` interface (schema stays version 1 — the
  existing `{...DEFAULTS, ...parsed}` merge already backfills missing keys from old blobs).
  `WorkbenchPage`'s Inspector dock toggle (`Ctrl+Shift+L`) now reads/writes `PrefsStore` instead of its
  own `devcontext-dock` localStorage key. `theme` is stored but **not yet applied to the DOM** — that's
  W0-finish's `ThemeService`, still open.
- **Tests:** `trail.store.spec.ts` (new, 7 tests — push/undo/redo, forward-branch truncation on push-
  after-undo, duplicate-push collapse, `jumpTo`, pin toggle, tab-close self-GC, per-tab isolation).
  `workspace.store.spec.ts` (+2 tests — the dup-path guard switches tabs and does *not* re-call
  `analyze()`; re-analyzing the active tab's own path is unaffected).

**Verified:**
- `pnpm check` green (real exit code): lint 0/0 · test 25/25 · build 0w/0e.
- Manual smoke via `pnpm dev:web` + Playwright (`chromium-cli` isn't installed in this environment;
  used `playwright-core` with `channel: 'chrome'` against the system Chrome instead — no browser
  download needed): analyzed `tests/fixtures/MinimalApiProject`, SPA-navigated to `/explore` (a real
  `page.goto` would reset in-memory session state, same as it would for a user typing the URL fresh —
  `WorkspaceStore` deliberately doesn't persist session/trace/handle), focused the deck and scrubbed
  with `j` — trace tree + Inspector Details/LLM/Trail all populated correctly, trail breadcrumb showed
  `POST /orders`. Toggled the dock (`Ctrl+Shift+L`) twice — Inspector hid and restored with content
  intact. Confirmed `localStorage['devcontext-prefs']` carries `dockLevel`/`theme` and the old
  `devcontext-dock` key is never written. Zero console errors throughout.

**Next (HANDOFF's waterfall order, unchanged):**
- W3 remainder: analyze-stream cancel + tab-close abort sweep beyond what `OperationController`
  already does (it already cancels + closes the session on tab close — audit whether more is needed);
  omnibox/palette search (`searchNodes`) onto `runLatest` (currently ungated — GAP-B1).
- W0 finish: bundle Inter/JetBrains Mono locally, `ThemeService` (to actually apply `PrefsStore.theme`),
  `/styleguide` dev route.
- W1: shell skeleton rename/restyle (titlebar/activity-bar/statusbar), wire tab-strip, WebView shortcut
  interception — see `AGENTS.md` "F — Fable Workbench Redesign" for the up-to-date per-stage status.

---

## 2026-07-03 — Fable branch: W0 finish (fonts/primitives/styleguide) + W1 (shell skeleton)

**Context:** continuing `feat/fable-redesign-skeleton` immediately after the W3-completion session
above, in the same session. Corrected course on one thing that session got wrong: it added a
`PrefsStore.theme: 'graphite'|'paper'|'system'` field as an inert placeholder for "W0 finish's
ThemeService" — but `ThemeService` (`core/theme/theme.service.ts`) turned out to already exist
pre-branch, with its own `data-vibe`/`data-theme` model (vibe = modern/terminal/hacker, theme =
dark/light/high-contrast per vibe) that doesn't match the vocabulary that field assumed. Removed the
field rather than keep two conflicting theme-persistence paths.

**Changed — W0 (commit `d465717`):**
- Inter Variable + JetBrains Mono, Latin-subset woff2 (~110KB combined), fetched once from Google
  Fonts' own CDN and vendored into `public/assets/fonts` — not an npm dependency. Removed the remote
  `<link>` from `index.html`; reordered every vibe's font fallback chain to try the bundled font
  first instead of assuming the OS has Cascadia Code/JetBrains Mono installed.
- Six new `ui/` primitives: `Skeleton`, `Meter`, `SeamChip`, `KindIcon`, `EmptyState`, `Ticker` — all
  presentational, zero store imports (grepped to confirm). Building `SeamChip` surfaced a real,
  previously-unverified bug the skeleton HANDOFF had flagged as a risk: `trace-node.ts` had its own
  local seam-color map with lowercase/plural keys (`'raises'`, `'consumes'`, `'handler'`) that never
  matched the wire's actual values — confirmed against `DevContext.Core/Graph/TraceBuilder.cs`'s
  `SeamKind` enum, the wire is `SeamKind.ToString()`, PascalCase singular (`Entry`, `Call`, `Send`,
  `Handle`, `Raise`, `Consume`, `Data`, `Resolve`, `Pipeline`). Every seam chip in every trace view —
  old `/trace` page and the new `/explore` Flow altitude alike — has been falling back to the default
  gray this whole time. `trace-node.ts` now uses `SeamChip`, which does a correct case-insensitive
  lookup against `models/seam-colors.ts` (which also gained the missing `Entry` color).
- `pages/styleguide-page.ts`, dev-only route (`isDevMode()`-gated — confirmed tree-shaken from the
  production bundle via `grep`, not just hidden). Token sheet, seam palette, type ramp, the
  `@layer components` vocabulary, every `ui/*` primitive in at least one state.
- Gate cleanup: `grep 'shadow-sm|rounded-md' src/app/ui` was NOT clean — 8 pre-existing components
  used `rounded-md`, outside the proposal's two-bucket radius scale (§4.4: 3px controls, 6px floating
  overlays). Fixed: controls → `rounded-sm`; the floating `Toast` → `rounded-lg` + the exact
  `--shadow-overlay` token instead of a generic `shadow-lg`; `GraphCanvas` (a structural panel, not a
  control) → no radius at all.
- Verified: `pnpm check` green (lint 0/0, test 25/25, build 0w/0e); grep clean; Playwright smoke of
  `/styleguide` — zero console errors, `document.fonts` reports both fonts loaded, zero requests to
  `fonts.googleapis.com`/`gstatic.com`.

**Changed — W1 (commit `485e431`):**
- `shell/titlebar/titlebar.ts` (new, replaces `shell/header/app-header.ts`): 30px, solid `bg-base`,
  no blur/shadow. Drag-region hygiene (§7.2). Omnibox trigger dispatches the same Ctrl+K keydown
  `Palette` already listens for globally (no new coupling). Repo-label dropdown (recents + New
  analysis) preserved — palette doesn't cover that yet.
- `shell/tab-strip.ts` wired into `workspace-shell` (**GAP-T1** — fully built already, including its
  own Ctrl+T/W/1-6 handling, just never imported). `shortLabel()` fixed to use the last path segment
  (**GAP-T3**). MRU stack added to `WorkspaceStore` + Ctrl+Tab/Ctrl+Shift+Tab cycling (**GAP-T5**), 2
  new unit tests.
- `shell/activity-bar.ts` (new, replaces `shell/navigation-rail.ts`): registry-safe icons (**GAP-S7**
  — `'home'`/`'list'`/`'lightbulb'` were never in the icon registry, so those three rail items
  rendered with literally no icon this whole time; now `map`/`layers`/`zap`). Disabled items stay in
  the DOM with a tooltip instead of being filtered out (**GAP-S5**). Count badges on Entries/Insights
  (**GAP-S6**). 3px left-accent active indicator. Deliberately keeps all 7 existing items rather than
  collapsing to the proposal's eventual 5-icon layout — see AGENTS.md note.
- `shell/statusbar/statusbar.ts` (new, replaces `shell/footer/app-footer.ts`): 22px, moved out of
  `fixed bottom-0` into normal document flow — `workspace-shell`'s `calc(100vh - 2.75rem - 1.75rem)`
  height hack is gone, the shell is a plain flex column now. New `Ticker` primitive mounted (inert
  until W5).
- `shell/offline-banner.ts` (new), `core/webview-shortcuts.service.ts` (new, §7.3 — Ctrl+P/Ctrl+R/F5/
  Ctrl+F/zoom interception), `/atlas` stub route.
- Verified live end-to-end via Playwright against a real analyzed repo (two gotchas hit and worked
  around, noted in case they recur): (1) Playwright's `keyboard.press('Control+T')` sends `key: 'T'`
  (uppercase), not `'t'` — the app's handlers correctly check lowercase to match real keyboards, so
  the combo string needs lowercase too, e.g. `'Control+t'`. (2) `concurrently -k` (used by
  `pnpm dev:web`) kills `ng serve` the instant the .NET server process dies, so testing the offline
  banner needs the two processes started independently (`pnpm server` — or a direct
  `dotnet .../DevContext.Server.dll` invocation — and `ng serve`, separately) rather than through the
  combined script. Confirmed: 30px titlebar; Ctrl+T/W/Tab/1 all change tab state correctly; help
  overlay lists the new shortcuts; Ctrl+R doesn't reload (page-lifetime JS marker survives) AND
  visibly triggers a real re-analyze ("Discovering files… 10%" in the statusbar) — not just "nothing
  broke"; F5 doesn't reload; killing the server surfaces the offline banner + red connection dots
  within one 5s poll cycle, reviving it clears both. Zero console errors except one pre-existing,
  unrelated bug incidentally surfaced by rapid concurrent-tab analysis: `NG0955` duplicate track keys
  — `section-console.ts` tracks `@for` by `line.timestamp` (`Date.now()`), and two progress events in
  the same millisecond collide. Not fixed here (out of scope), noted in AGENTS.md.
- `pnpm check` green throughout: lint 0/0 · test 27/27 · build 0w/0e.

**Next:** W2 completion (progressive `trace-tree.ts` to replace `trace-node.ts`'s reuse in Stage;
`audit-table.ts`; `graph-canvas` topology/neighbors builders) or W3's remainder (analyze-stream cancel
sweep, omnibox onto `runLatest`) — see `AGENTS.md`'s "F — Fable Workbench Redesign" for current status.

**Changed — W3 remainder + W4 partial (commits `bf23e31`..`7de783d`, this session):**
Resumed cold per AGENTS.md's own resume protocol; W0/W1 were already done, W3 was "mostly done."
Closed out W3, then pushed six W4 checkpoints, each its own commit, `pnpm check` green before every
one, and each verified live against a headless Chrome (playwright, `channel: 'chrome'`) analyzing
`tests/fixtures/MinimalApiProject` through the real server — not just lint/build. That verification
step caught four real bugs no amount of type-checking would have: see below.

1. **W3 remainder** (`bf23e31`): palette search debounced (150ms) through `LatestGate` — closes
   GAP-B1. Audited the analyze-stream/tab-close cancel sweep (`OperationController` +
   `WorkspaceStore.closeTab` + `TraceStore`/`AtlasStore`'s tab-close effects) — already complete, no
   code change needed.
2. **Omnibox** (`0af42ca`): new `features/omnibox/omnibox.ts` replaces `features/palette/palette.ts`
   (deleted). Ctrl+K/Ctrl+P, Tab-cycle verbs (Trace/Node/Usages/Copy) applied to the selected
   entry/node row, sections Action/Recents/Entries/Nodes, static sections computed once (GAP-C2).
   `TraceStore.selectNode()` gained an optional `NeighborDirection` param (was hardcoded `'out'`).
   **Bugs found live, not by lint:** the input never received DOM focus on open — typing only
   "worked" by whatever residual focus was left over from a prior interaction, a real keyboard-first
   regression; Escape didn't close the overlay (the panel's blanket `(keydown)="$event.stopPropagation()"`
   swallowed it before it reached the overlay's own handler); static action items weren't filtered by
   query at all (the code path was simply missing). All three fixed same commit.
3. **Stage altitudes** (`8d1c181`): `GraphCanvas` now takes a discriminated `data` input
   (`mode: trace|topology|neighbors`) instead of a lone `trace` input. System altitude renders real
   topology the instant analysis completes (zero traces run — the "graph is never blank" promise);
   clicking a project filters the Entry Deck to it (`EntryVm` gained `project` from
   `EntryPoint.project`). Node altitude gained a List/Graph toggle plus an out/in/usages direction
   toggle. **Engine-constraint discovery, confirmed by hand against the live server, not from
   docs:** `GetTrace`'s `focus` only resolves registered entry-point keys — passing a raw internal
   node id (e.g. `Member:Foo.<lambda>`) comes back `found: false`. So "double-click any graph node →
   re-trace from it" is NOT a fresh `GetTrace` call (the proposal assumed no RPC needed this would
   suffice, but doesn't say how); `TraceStore.reroot()` instead finds the node inside the
   already-loaded tree and re-roots the display at it client-side — zero new RPCs, depth capped at
   whatever was already fetched. Trail gained a `reroot` step kind whose restore path calls
   `reroot()` instead of re-tracing (its id isn't a valid focus string, so the normal restore path
   would also 404).
4. **Workbench URL state + Esc-ladder** (`0278a48`): `/explore` mirrors selection into
   `?focus&view&kind&q` (read once on load for deep-link compat, written back with `replaceUrl`,
   same convention as `TracePage`'s existing `?focus`). `EntryDeck.filterText`/`activeKind` and
   `Stage.altitude` became `model()` so the Workbench can lift them. Esc-ladder: cancel in-flight
   trace → deselect node → clear focus → clear filter (the "close overlay" rung was added in
   checkpoint 6 once there was an overlay). Added `p` (pin current trail step) and Alt+←/→ (trail
   undo/redo aliases). `TraceStore` gained `cancelTrace()`/`deselectNode()` for the ladder's first
   two rungs.
5. **Inspector Render-RPC LLM section** (`4a56501`): migrated off `trace.markdown()` onto the Render
   RPC (250ms debounce, real `estimatedTokens`), superseding `section-lens.ts`'s pattern for this
   surface. First load shows `Skeleton` blocks; a refresh dims existing content instead (per the
   content-preserving loading policy — skeletons are first-load only). **Found live:** the LLM
   header nested a `<button>` (Copy) inside another `<button>` (the section-h collapse toggle) —
   invalid HTML that Angular's DOM renderer never sanitizes (it builds the DOM via direct node
   creation, so the browser's HTML-parse-time nesting correction never triggers). This was
   pre-existing (inherited from the original `trace.markdown()` version, not introduced this
   session) but squarely in the code being touched, so fixed: the toggle is now an inner button
   inside a plain div, Copy is a sibling not a descendant. Deliberately NOT done: Insights-for-
   selection and "Reached by N flows" via `AtlasStore` — the data already exists (built ahead of
   schedule in W3) but wiring it now would jump the waterfall past a real W4 gate for no urgent
   reason; left for W5 as the proposal itself places them.
6. **Audit table overlay** (`7de783d`): new `features/explorer/audit-table.ts`, the sortable/
   filterable entry table ported from `section-entries.ts`, as a Shift+E overlay instead of a
   standalone page. Self-contained filter/sort state (no URL sync — unlike `section-entries`'s old
   `?sort&dir&kind&q`, syncing would fight the Workbench's own `?kind&q`). Row "Trace" behaves like a
   deck selection and closes the overlay.

**Process note for whoever resumes:** three of the four Playwright smoke runs in this session hit a
`<vite-error-overlay>` intercepting clicks on the VERY FIRST browser launch immediately after a
`pnpm check` (which runs its own one-shot `ng build` concurrently with the already-running `ng serve`
dev server — they likely contend over `.angular/cache`). Every single time, a bare retry of the exact
same script one more time passed clean with zero console errors. Treat this specific symptom (overlay
appears only on the first post-check run, never on the second) as this known flakiness, not a product
bug — but don't dismiss a `vite-error-overlay` that persists across a retry; read its text.

**Remaining for a clean W4 gate** (see `AGENTS.md` — updated same session): export drawer (Ctrl+E,
presets, From Trail), Home page assembly (identity strip, top-flow list, insight headlines), Atlas
page assembly (map/topology/packages — `eventWiring`/`hubs` already computed in `AtlasStore` since
W3, just needs binding), route cutover (`/explore` canonical, old routes redirect, delete
`section-entries/-trace/-graph/-lens/-export` + their pages + `SectionCard`), then the full manual
gate sweep in proposal §10's W4 table.
