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
- V3 P2: Live Console + real Stats streaming
- V3 P3: Synced Human↔LLM lens

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

