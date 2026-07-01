# PLAN — Desktop V2 (the world-class .NET repo lens)

> **Branch:** `feat/desktop-v2` (off `develop` after merging `feat/library-surface-fv-polly`)
> **Started:** 2026-06-30
> **Last session:** 2026-06-30 — R0–R5 remediation + P4 Trace view complete
> **Resume:** Read `docs/dev/HANDOVER-DESKTOP-V2.md` first. Then `git checkout feat/desktop-v2` → verify gates (§9) → pick up at §8 known gaps or continue master plan P4 trace-graph + P5 engine.

---

## 0. State snapshot (what we have today)

### 0.1 Git

- **Branch:** `feat/desktop-v2` created from `develop` at merge commit (includes all library-surface + desktop-redo spike)
- **Working tree:** clean
- `docs/dev/PLAN-DESKTOP-V2.md` — this file, tracked on the branch

### 0.2 Builds green

| Gate | Status |
|---|---|
| `dotnet build DevContext.slnx` | **0 warnings, 0 errors** |
| `dotnet test tests/DevContext.Server.Tests` | **12/12 passed** |
| `dotnet test DevContext.slnx` | **382/384 passed** (2 skipped — SurfaceQualityTests flaky, not blocking) |
| `pnpm lint` (from `src/DevContext.App`) | **All files pass** |
| `pnpm test` (from `src/DevContext.App`) | **4/4 passed** |
| `pnpm build` (from `src/DevContext.App`) | **Successful** (Angular prod build — slow on first run) |

### 0.3 Server audit result (actual state vs plan assumptions)

**The PLAN-DESKTOP-V2 §7 said several server RPCs were scaffolded/UNIMPLEMENTED. The audit found they are ALL implemented:**

| Area | Status | File |
|---|---|---|
| EngineHostCache (warm reuse) | **DONE** | `src/DevContext.Server/Sessions/EngineHostCache.cs` |
| EngineRunner uses EngineHostCache | **DONE** | `src/DevContext.Server/Sessions/EngineRunner.cs:38` |
| Session eviction (LRU + idle-TTL) | **DONE** | `src/DevContext.Server/Sessions/AnalysisSessionManager.cs:52-79` |
| CloseSession RPC (idempotent) | **DONE** | `src/DevContext.Server/Endpoints/DevContextGrpcService.cs:74-78` |
| ServerOptions from config | **DONE** | `src/DevContext.Server/Sessions/ServerOptions.cs` + `Program.cs:8-10` |
| Central error mapping | **DONE** | `src/DevContext.Server/Endpoints/DevContextGrpcService.cs:239-269` |
| Clone progress wired to stream | **DONE** | `src/DevContext.Server/Sessions/EngineRunner.cs:84-85` |
| SearchNodes RPC | **DONE** | `src/DevContext.Server/Endpoints/DevContextGrpcService.cs:144-166` |
| GetStats RPC | **DONE** | `src/DevContext.Server/Endpoints/DevContextGrpcService.cs:168-182` |
| Render RPC | **DONE** | `src/DevContext.Server/Endpoints/DevContextGrpcService.cs:184-204` |
| Structured GetMap | **DONE** | `src/DevContext.Server/Endpoints/DevContextGrpcService.cs:90-96` |
| Session management + eviction tests | **DONE** | `tests/DevContext.Server.Tests/SessionManagementTests.cs` |
| Integration tests (all RPCs) | **DONE** | `tests/DevContext.Server.Tests/AnalyzeFlowTests.cs` |

**ONLY server gap:** `AnalyzeRequest.cleanup` (auto/keep) — the proto field exists but is never read. `AnalysisSession.DisposeAsync` always cleans clones. Fix is small (add `cleanup` to `AnalyzeSpec`, honor in `AnalysisSession.DisposeAsync`). See §P0.1.

### 0.4 Frontend spike state (what we're rebuilding from)

The spike in `src/DevContext.App/src/app/` has:

- **Infrastructure (keep):** `core/grpc/**`, `data-access/**`, `core/theme/**`, `state/*.store.ts`, `models/**`, `ui/icon`, `ui/badge`, `ui/button`, `ui/card`, `ui/panel`, `ui/search-field`, `ui/segmented`, `ui/sheet`, `ui/spinner`, `ui/tabs`, `ui/text-input`, `ui/toast`, `ui/toolbar`
- **Shell (replace):** `app.ts` (bare `<router-outlet>`), `features/workspace/workspace.ts` (single-page conditional layout with inline status bar)
- **Old IA features (replace/repurpose):** `entries-panel`, `map-panel`, `trace-panel`, `launcher`, `llm-export`, `stats-drawer`, `source-bar`, `node-detail`, `overview`, `health-dashboard`, `shortcuts`, `vibe-switcher`, `story-panel`, `entity-cross-ref`, `impact-panel`, `repo-browser`
- **Drop entirely:** `ui/command-palette/**`, `ui/drawer/**`, `ui/graph-canvas/**` (de-hardcode colors), `ui/kbd-hint/**`
- **Stores (keep pattern, reshape):** `session.store.ts`, `trace.store.ts`, `connection.store.ts`, `github.store.ts`, `recent.store.ts`
- **Routing:** Currently `''` → `/home` → `Workspace`. Will become activity-bar routing.

### 0.5 Key decisions (locked)

| Decision | Choice |
|---|---|
| Base branch | `feat/desktop-v2` off `develop` (all latest merged) |
| Foldering | **In-place** — delete old features, build new under `src/app/shell/` + `src/app/features/{source,overview,entries,browse,trace,document,stats,cache}/` |
| Topology rendering | **Simple boxes/arrows** (no dagre graph in Overview) |
| Command palette | **Dropped** |
| Graph-as-nav | **Dropped** — basic graph is entry/trace aid only |
| Cytoscape | **Removed** — tiny path graph in Trace is pure SVG/Tailwind |
| Cleanup | All decisions from PLAN-DESKTOP-V2 §2 (Tauri v2, Angular 22 zoneless, Tailwind v4, lucide, signal stores, gRPC-Web, etc.) |

---

## 1. Phases overview

```
P0 ── Shell & system spine (foundation)
 │
 ├─ P0.1 ── Server: fix AnalyzeRequest.cleanup (only missing server item)
 │
 P1 ── Intake + Overview + Cache
 │
 P2 ── Entry Points + Browse/filter
 │
 P3 ── Document/Export + Stats + choreographed progress
 │
 P4 ── Trace (right-sized) + vibes/polish + a11y
 │
 P5 ── Engine: indirect-wiring breadth + eval (parallelizable)
```

Each phase is a **deliverable chunk** that ends with a green gate and is independently shippable.

---

## 2. P0 — Shell & system spine

**Goal:** New fixed-grid app shell with activity-bar navigation, routes, status bar, and operational infrastructure. Delete the old workspace.ts framing.

### P0 deliverables (in order)

#### P0.0 — Shell grid + routes (the IA reset)

1. **Create `src/app/shell/` directory** with:
   - `app-shell.ts` — the fixed CSS-grid component:
     ```
     grid-template-rows: 36px 1fr 24px
     grid-template-columns: 48px var(--sidebar-width) 1fr
     ```
     TitleBar (top row, full width), ActivityBar (left 48px, row 2), Sidebar (contextual, col 2, row 2), Main `<router-outlet>` (col 3, row 2), StatusBar (bottom row, full width).
   - `title-bar.ts` — custom Tauri drag region, app title + controls
   - `activity-bar.ts` — icon rail with route links: Source, Overview, Entry Points, Browse, Trace, Document, Stats, Cache
   - `status-bar.ts` — binds to `ActivityService`, shows op label + stage + percent
   - `sidebar.ts` — contextual sidebar per-route (content projected by each route component)

2. **Update `app.config.ts` routes:**
   ```typescript
   { path: '', component: SourceView },         // launcher when no session
   { path: 'overview', component: OverviewView },
   { path: 'entries', component: EntriesView },
   { path: 'browse', component: BrowseView },
   { path: 'trace', component: TraceView },
   { path: 'document', component: DocumentView },
   { path: 'stats', component: StatsView },
   { path: 'cache', component: CacheView },
   ```
   Route guard: `/overview`…`/stats` require a session; redirect to `/` with hint.

3. **Update `app.ts`** to use `AppShell` instead of bare `<router-outlet>`.

4. **Scroll laws (per §4 of original plan):**
   - Shell root: `display:grid; height:100vh; overflow:hidden`
   - `html,body { height:100%; overflow:hidden }` (already in `styles.css`)
   - Only leaf content regions get `overflow:auto`
   - Every flex/grid ancestor gets `min-h-0` / `min-w-0`

5. **Delete `features/workspace/workspace.ts`** and its route.

**Exit gate:** App boots to the shell; empty routes render placeholder text; activity bar navigates between them; no body scroll; `pnpm check` + `dotnet build DevContext.slnx` green.

#### P0.1 — ActivityService + OperationController

1. **Create `src/app/core/activity/`:**
   - `operation-controller.ts` — wraps each cancellable op:
     ```typescript
     class OperationController {
       state: Signal<'idle'|'running'|'cancelling'|'done'|'error'>
       cancel(): void
       run<T>(fn: (ct: CancellationToken) => Promise<T>): Promise<T>
     }
     ```
     New analyze cancels prior (port WPF `CancellableOperation` pattern).

   - `activity.service.ts` — signal store:
     ```typescript
     class ActivityService {
       label: Signal<string>         // "Analyzing DntSite…"
       stage: Signal<string>         // "Deep analysis"
       percent: Signal<number>       // 0-100
       state: Signal<'idle'|'busy'|'error'>
       cancel(): void
     }
     ```
     The ONE busy truth. StatusBar binds to this.

2. **Wire `ActivityService`** into `SessionStore.analyze()` — the analyze stream updates it.

3. **Wire into the status bar** and verify a fake op renders correctly.

**Exit gate:** Status bar reflects a fake operation; cancel fires; no duplicate spinners anywhere.

#### P0.2 — Vibe token extension

1. **Extend vibe tokens in `src/styles.css`** — add density, border-width, motion tokens:
   ```css
   --vibe-density: normal | compact
   --vibe-border-width: 1px | 2px
   --vibe-motion: normal | reduced
   --vibe-sidebar-width: 240px
   --vibe-activity-bar-width: 48px
   --vibe-title-bar-height: 36px
   --vibe-status-bar-height: 24px
   ```

2. **Add `modern-light` vibe** to `core/theme/vibes/` + register in `vibes/index.ts`.

3. **De-hardcode `ui/graph-canvas` colors** — read from `ThemeService.palette()` CSS vars. (Later: remove Cytoscape entirely in P4.)

**Exit gate:** `pnpm check` green; vibe switcher still works with all 3 vibes (modern, terminal, hacker) + new modern-light; `ui/` components use vibe tokens only.

#### P0.3 — Delete drop-list items

1. **Delete `ui/command-palette/`** — entire directory
2. **Delete `ui/drawer/`** — entire directory
3. **Delete `ui/kbd-hint/`** — entire directory
4. **Remove command palette wiring** from `workspace.ts` (the whole file is deleted in P0.0, but verify no other references remain)
5. **Remove `⌘K` handler** from any surviving components

**Exit gate:** No import of deleted modules anywhere; `pnpm check` green.

### P0 server fix

#### P0-S1 — Wire `AnalyzeRequest.cleanup`

1. **Add `cleanup` to `AnalyzeSpec`:**
   - File: `src/DevContext.Server/Sessions/AnalysisContracts.cs`
   - Add: `string? Cleanup` (values: `"auto"`, `"keep"`, or null)

2. **Read `cleanup` in `DevContextGrpcService.Analyze`:**
   - File: `src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17-38`
   - After line 25 (`request.NoRoslyn`), add: `request.HasCleanup ? request.Cleanup : null`

3. **Store `cleanup` in `AnalysisSession`:**
   - File: `src/DevContext.Server/Sessions/AnalysisSession.cs`
   - Add `_cleanup` field set from spec
   - In `DisposeAsync`: only clean clone if `_cleanup != "keep"`

4. **Pass `cleanup` through `EngineRunner` → `EngineResult`:**
   - The `EngineResult` already carries `GitClonePath` — just add `Cleanup` field
   - `AnalysisSessionManager.AnalyzeAsync` passes it to `AnalysisSession` constructor

**Exit gate:** `dotnet test tests/DevContext.Server.Tests` green (12/12).

### P0 exit criteria (full)

- [ ] Shell grid renders with activity bar + router outlet + status bar
- [ ] Navigate between all 8 routes (empty views)
- [ ] Status bar shows fake operation; cancel works
- [ ] No body scroll; viewport fixed at 1024x720
- [ ] `pnpm check` green (lint + test + build)
- [ ] `dotnet build DevContext.slnx` 0/0
- [ ] `dotnet test tests/DevContext.Server.Tests` green
- [ ] Server `cleanup` honored
- [ ] `ui/command-palette`, `ui/drawer`, `ui/kbd-hint` deleted; no leftover imports

---

## 3. P1 — Intake + Overview + Cache

**Goal:** Full source intake (port WPF `ConfigPanel.razor`), structured Overview from `GetMap`, Cache page with clone/recents management.

### P1.0 — Source intake (port ConfigPanel.razor)

1. **Create `features/source/`** — replace old `launcher` + `source-bar`:
   - `source-view.ts` — the full intake form (route `/`)
   - `repo-input.ts` — URL/path input with 400ms debounced `git ls-remote` validation + status pill (checking/valid/private/not-found/rate-limited/no-git)
   - `repo-picker.ts` — Tauri folder + `.sln` picker via `@tauri-apps/plugin-dialog`
   - `recent-chips.ts` — recent repos as chips (from `recent.store`)
   - `advanced-options.ts` — collapsible advanced: depth (1-10), detail (signature/salient/full), no-roslyn, dry-run, format, diagnostics
   - `clone-control.ts` — truthful clone path + size display; auto-clean on exit / keep choice

2. **Wire intake → `SessionStore.analyze()`** with the full `AnalyzeRequest` shape.

3. **Delete old `features/launcher/`** and `features/source-bar/` once replaced.

**Exit gate:** Clone GitHub repo + local folder from intake; see progress in status bar; clone listed in Cache (even if Cache page is placeholder); cancel mid-analyze.

### P1.1 — Overview view (structured GetMap)

1. **Create `features/overview/`** — route `/overview`:
   - `overview-view.ts` — layout: sidebar (section jump-list) + main (structured Map)
   - `archetype-card.ts` — App/Library archetype with confidence + evidence
   - `style-card.ts` — architectural style with confidence
   - `library-surface.ts` — when IsLibrary: surface groups + extension points
   - `topology-dag.ts` — simple boxes/arrows (pure SVG/Tailwind, no Cytoscape): project nodes → dependency arrows
   - `package-stack.ts` — package groups
   - `cross-cutting.ts` — pipeline behaviors, aggregates
   - `scope-note.ts` — scope note when present

2. **Use `DevContextApi.getMap(handle)`** — the structured `MapResponse` already has all fields.

**Exit gate:** Analyze a repo → Overview shows all cards; topology DAG renders correctly; sidebar jump-list scrolls to sections.

### P1.2 — Cache page

1. **Create `features/cache/`** — route `/cache`:
   - `cache-view.ts` — two sections: clones + analysis-cache
   - `clones-list.ts` — table: path, size, age, keep/auto, delete button
   - `cache-footprint.ts` — persistent analysis-cache size + clear button
   - `recents-manage.ts` — recent entries with remove (from `recent.store`)
   - Cache hit-rate display on re-analysis

2. **Wire clone tracking:**
   - `SessionStore` tracks clones per session
   - Cleanup runs on close (honor `cleanup` field)
   - Surface in Cache page via `session.store`

**Exit gate:** Clone a repo → Cache page shows clone entry with size/age; delete button removes clone; recents manageable; cache hit-rate shown.

### P1 exit criteria

- [ ] Intake form accepts URL/path + Tauri picker
- [ ] Debounced validation with status pill
- [ ] Advanced options toggle + honored in analyze
- [ ] Clone control shows path/size; auto-clean/keep works
- [ ] Overview shows structured Map with all cards
- [ ] Simple topology DAG renders
- [ ] Cache page: clones listed, deletable, recents manageable
- [ ] Cancel mid-analyze works cleanly
- [ ] `pnpm check` green; `dotnet build` 0/0; server tests green

---

## 4. P2 — Entry Points + Browse/filter

### P2.0 — Entry Points view

1. **Create `features/entries/`** — route `/entries`:
   - `entries-view.ts` — layout: sidebar (kind filter + search) + main (entry board)
   - `entry-board.ts` — grouped by kind (HTTP, Bus Consumers, Hosted Services, Scheduled Jobs, Domain Events, Public API)
   - `entry-card.ts` — each row: method/route → resolved target, "approx" badge if syntactic
   - `entry-graph-toggle.ts` — optional basic graph (entry → handler) as toggleable mini-view
   - Click entry → navigate to `/trace?focus=X`

2. **Wire `ListEntryPoints` RPC** — already implemented and tested.

**Exit gate:** Entry board shows entries grouped by kind; click → traces; approx badges visible.

### P2.1 — Browse view

1. **Create `features/browse/`** — route `/browse`:
   - `browse-view.ts` — layout: sidebar (kind/tag facets + search) + main (node table)
   - `node-table.ts` — filterable list/table: title, kind, tags, in-degree, out-degree
   - `node-detail-card.ts` — row click → detail card (from `GetNode`): file:line, tags, degree
   - `neighbor-panel.ts` — Calls-out / Called-by / Find-usages tabs (from `GetNeighbors` with direction `out|in|usages`)

2. **Wire `SearchNodes`, `GetNode`, `GetNeighbors`** — all already implemented.

3. **Delete old `features/node-detail/`** and `features/entity-cross-ref/` once replaced.

**Exit gate:** Browse the graph by text filter + kind/tag facets; inspect any node with neighbor directions; "approx" indicators visible.

### P2 exit criteria

- [ ] Entry points grouped by kind with approx badges
- [ ] Click entry → trace route
- [ ] Browse table with text filter + kind/tag facets
- [ ] Node detail card with file:line, degree, tags
- [ ] Calls-out / Called-by / Find-usages working
- [ ] `pnpm check` green; `dotnet build` 0/0

---

## 5. P3 — Document/Export + Stats + choreographed progress

### P3.0 — Document/Export view

1. **Create `features/document/`** — route `/document`:
   - `document-view.ts` — layout: section rail (sticky sidebar with toggles) + rendered Human view
   - `section-rail.ts` — section jump-list with include toggles (port WPF `SectionSelectionModel`)
   - `human-view.ts` — rendered markdown/HTML as navigable document
   - `llm-export.ts` — LLM/export panel: live token total vs budget slider; copy/save via Tauri
   - Section toggles drive the LLM/export view — toggling a section recalculates tokens against budget

2. **Wire `Render` RPC** with `sections[]`, `format`, `include_diagnostics`.

3. **Delete old `features/llm-export/`** once replaced.

**Exit gate:** Toggle sections → token total updates; click section in rail → scrolls to it; copy/save exports; LLM view reflects toggles.

### P3.1 — Dual Stats page

1. **Create `features/stats/`** — route `/stats`:
   - **Run stats** (from `GetStats`): stage waterfall, per-extractor timings + types/detections, scorer funnel, cache hit-rate, seam coverage (edges per seam + approx count), nodes/edges, wiring coverage = entriesWithTarget/entries
   - **Code stats** (insight): projects/files, type/member counts, entry-points by kind, public-surface size, library abstractions by implementor count, package stack

2. **Wire `GetStats` RPC** — already implemented.

3. **Delete old `features/stats-drawer/`** and `features/health-dashboard/` once replaced.

**Exit gate:** Stats page shows run + code stats; numbers match CLI output; per-extractor timings visible.

### P3.2 — Choreographed progress (stage stepper)

1. **Create `ui/stage-stepper/`** — renders the 7 engine stages as a stepper:
   - DiscoveryAndCacheWarmup → GenericExtraction → SignalSealing → SpecificExtraction → Scoring → Compression → Rendering
   - Each stage lights up as `ProgressEvent` streams in
   - Percent per stage shown

2. **Wire into the Analyze stream:**
   - `SessionStore` exposes `progressEvents: Signal<ProgressEvent[]>` from the streaming Analyze
   - Stage stepper reads from it
   - Stats "code" panel fills in live during analysis

3. **Wire clone progress** — already streamed via `CloneProgress` → `ProgressEvent`.

**Exit gate:** Analyze a repo → stage stepper lights up sequentially; Stats code panel fills live; clone phase shown when cloning.

### P3 exit criteria

- [ ] Document view with section toggles + token budget
- [ ] LLM/export with live token total + budget slider
- [ ] Copy/save export (markdown + JSON)
- [ ] Run stats page with stage waterfall + extractor timings + cache hit-rate
- [ ] Code stats page with type/member counts + entry-point breakdown
- [ ] Stage stepper lights up during analysis
- [ ] Live Stats code panel fills during analysis
- [ ] `pnpm check` green; `dotnet build` 0/0; server tests green

---

## 6. P4 — Trace (right-sized) + vibes/polish + a11y

### P4.0 — Trace view (right-sized)

1. **Create `features/trace/`** — route `/trace`:
   - `trace-view.ts` — layout: focus picker + narrative tree + small path graph
   - `focus-picker.ts` — searchable entry+symbol combobox (port the search from old ConfigPanel)
   - `narrative-tree.ts` — recursive tree component: seam · title · file:line · salient · pipeline behaviors · approx/resolved badges
   - `path-graph.ts` — small, legible SVG of the current trace path only (no Cytoscape — pure SVG/Tailwind)
   - Instant re-trace on focus change (analyze once, render many)

2. **Wire `GetTrace` RPC** — already implemented.

3. **Delete old `features/trace-panel/`, `features/story-panel/`, `ui/graph-canvas/`** once replaced.

**Exit gate:** Trace any entry → narrative tree renders; focus picker filters entries/symbols; path graph syncs with tree; depth/detail controls work.

### P4.1 — Vibes + polish

1. **Add Terminal-variant vibes:** CLI green-on-black (exists), Hacker (exists)
2. **Add light mode to all vibes** (modern-light already added in P0.2)
3. **Vibe switcher** — runtime switch with persistence (already partially done in `vibe-switcher`)
4. **High-contrast mode** — a11y-focused theme
5. **Reduced motion** — respect `prefers-reduced-motion`

### P4.2 — a11y pass

1. **Keyboard navigation** — Tab order through all interactive elements; focus rings visible
2. **Screen reader** — aria-labels on activity bar, sidebar, status bar
3. **axe-core smoke test** — run axe against each route

### P4 exit criteria

- [ ] Trace view with narrative tree + small path graph
- [ ] Focus picker filters + instant re-trace
- [ ] 4 vibes (modern, terminal, hacker) × 2 modes (dark, light) all working
- [ ] High-contrast mode
- [ ] Keyboard nav works; focus rings visible
- [ ] axe smoke clean
- [ ] `pnpm check` green; `dotnet build` 0/0

---

## 7. P5 — Engine: indirect-wiring breadth + eval

**Parallelizable with P2–P4.** See original PLAN-DESKTOP-V2 §8.

- Broaden wiring coverage: MassTransit, Hangfire, Quartz, channels, minimal-API, SignalR, gRPC services, Scrutor, `IOptions<T>`, EF interceptors
- Eval loop: `eval-repos.json` → capture → benchmark → audit → gate
- Generalize metric to wiring coverage (`entriesWithTarget/entries`, seams labeled)
- Feed engine weak points back

---

## 8. File map (where things go)

| Area | Location |
|---|---|
| App shell | `src/DevContext.App/src/app/shell/{app-shell,title-bar,activity-bar,sidebar,status-bar}.ts` |
| Routes | `src/DevContext.App/src/app/app.config.ts` |
| Activity/Operation | `src/DevContext.App/src/app/core/activity/{activity.service,operation-controller}.ts` |
| Vibe/Tailwind | `src/DevContext.App/src/styles.css`, `src/DevContext.App/src/app/core/theme/**` |
| RPC client (reuse) | `src/DevContext.App/src/app/core/grpc/**`, `src/DevContext.App/src/app/data-access/devcontext-api.ts` |
| Stores (keep + extend) | `src/DevContext.App/src/app/state/{session,trace,connection,github,recent}.store.ts` |
| Views (new) | `src/DevContext.App/src/app/features/{source,overview,entries,browse,trace,document,stats,cache}/**` |
| UI primitives (keep) | `src/DevContext.App/src/app/ui/{badge,button,card,icon,panel,search-field,segmented,sheet,spinner,tabs,text-input,toast,toolbar}/**` |
| UI primitives (new) | `src/DevContext.App/src/app/ui/stage-stepper/` |
| UI primitives (delete) | `command-palette/`, `drawer/`, `kbd-hint/`, `graph-canvas/` (later) |
| Features (delete) | `workspace/`, `launcher/`, `source-bar/`, `llm-export/`, `stats-drawer/`, `health-dashboard/`, `node-detail/`, `entity-cross-ref/`, `trace-panel/`, `story-panel/`, `map-panel/`, `entries-panel/`, `impact-panel/`, `repo-browser/`, `shortcuts/` (as each is replaced) |
| Server | `src/DevContext.Server/Endpoints/DevContextGrpcService.cs`, `Sessions/**`, `Mapping/ProtoMapper.cs` |
| Proto | `proto/devcontext/v1/devcontext.proto` (additive only) |
| Drops | `ui/command-palette/**`, `ui/drawer/**`, `ui/kbd-hint/**`, Cytoscape dependency |

---

## 9. Gates (every phase, every commit)

```powershell
# .NET
dotnet build DevContext.slnx -clp:ErrorsOnly        # 0 warnings
dotnet test tests/DevContext.Server.Tests             # 12/12 (or more)
dotnet test DevContext.slnx                            # all green

# App (from src/DevContext.App)
pnpm check                                            # lint + test + build
pnpm gen:proto                                        # after proto changes
node --experimental-strip-types scripts/grpcweb-smoke.mts  # live smoke

# Contract
buf lint                                              # after proto changes
```

**Shell invariant:** Body never scrolls; no element overflows viewport at 1024x720.

---

## 10. Gotchas (carried from the spike)

- proto3 `optional` throws on null in generated C# — guard every nullable in `ProtoMapper`
- Run server as literal `dotnet <dll>` under `concurrently` (not `dotnet run`)
- `RepoUrl.Parse` eats relative paths — pass absolute local paths
- `.NET` is warnings-as-errors + nullable + analyzers
- Tailwind v4 CSS-first: vibes via `@theme inline` + `var(--vibe-*)` + `[data-vibe][data-theme]`. Never hardcode colors.
- Node 24+, pnpm 10 (corepack); `lucide` core (not `lucide-angular`)
- gRPC-Web + CORS: permissive localhost policy — smoke test proves transport

---

## 11. Session log

### 2026-06-30 (session 1) — P0–P3
Initial implementation: shell, 8 routes, ActivityService, Source/Overview/Cache/Entries/Browse/Document/Stats views, stage stepper, server cleanup fix.

### 2026-06-30 (session 2) — R0–R5 remediation + P4 Trace
Read `PLAN-DESKTOP-V2-REMEDIATION.md` (21 findings). Fixed all: ViewFrame migration, nav gating, honest validation, cleanup wiring, busy disables, secondary RPC error handling, real Trace narrative tree, dead code deletion, kind map dedup, vibe light themes + high-contrast, ThemeService/ConnectionStore init, Cytoscape removal, debounce, stats auto-load.
**Current state:** All 8 views functional. Remaining: P4 trace-graph, vibe switcher UI, a11y audit, Tauri picker/save, toast mount.

### P0 — Shell & system spine (COMPLETE)
- `src/app/shell/app-shell.ts` — fixed CSS-grid (36px/1fr/24px rows, 48px/240px/1fr cols) + activity bar + sidebar slot + `<router-outlet>`
- `src/app/shell/title-bar.ts` — Tauri drag region + connection dot
- `src/app/shell/status-bar.ts` — single busy truth from ActivityService
- `src/app/core/activity/activity.service.ts` — one global activity state
- `src/app/core/activity/operation-controller.ts` — cancellable op wrapper (abort → cancel)
- `src/app/app.config.ts` — 8 lazy-loaded routes + wildcard redirect
- `src/app/app.ts` — uses AppShell component
- `src/app/state/session.store.ts` — wired to ActivityService; `busy`/`ready` restored as proper computed booleans
- `src/styles.css` — added `overflow:hidden` to html/body, `prefers-reduced-motion` MQ, border-width token
- `src/DevContext.Server/` — wired `AnalyzeRequest.cleanup` through AnalyzeSpec → EngineResult → AnalysisSession.DisposeAsync
- Deleted: `ui/command-palette/`, `ui/drawer/`, `ui/kbd-hint/`, 13 old spike feature dirs

### P1 — Intake + Overview + Cache (COMPLETE)
- `features/source/source-view.ts` — full intake form: URL/path input, 400ms debounced validation (status pill), recents chips, advanced options (depth/detail/noRoslyn/dryRun/cleanup), stage stepper during analysis
- `features/overview/overview-view.ts` — structured GetMap: archetype, stats, topology (boxes/arrows), packages, aggregates, pipeline, library surface, extension points, scope note
- `features/cache/cache-view.ts` — analysis stats + coverage %, recent repos with delete, clone management info

### P2 — Entry Points + Browse (COMPLETE)
- `features/entries/entries-view.ts` — grouped by kind (HTTP/Bus/Hosted/Scheduled/Domain/API), text filter, kind filter, approx badges, click-to-trace → `/trace`
- `features/browse/browse-view.ts` — SearchNodes-driven node list, text filter, node detail card (kind/degree/location), neighbor directions (out/in/usages), approx badges

### P3 — Document + Stats + Progress (COMPLETE)
- `features/document/document-view.ts` — Render RPC, section rail with include toggles, token budget slider, copy, markdown view
- `features/stats/stats-view.ts` — dual Stats page: run stats (stages, extractors, cache, funnel) + code stats (graph, corpus, seams)
- `ui/stage-stepper/stage-stepper.ts` — 7-stage progress indicator (Discover→Render), shown during analyze in source view

### Current state
- `dotnet build DevContext.slnx` — 0/0
- `dotnet test tests/DevContext.Server.Tests` — 12/12
- `pnpm check` — lint, test (4/4), build all green
- Bundle: 405KB initial, 104KB transfer

### Remaining
- **P4** — Trace (right-sized) + vibes/polish + a11y
- **P5** — Engine: indirect-wiring breadth + eval

---

## 12. Resume instruction

1. Read this file in full — note P0–P3 already complete (§11 session log)
2. Confirm green: `dotnet build DevContext.slnx -clp:ErrorsOnly; pushd src\DevContext.App; pnpm check; popd`
3. Pick the next phase (P4 or P5) — each is self-contained
4. Work through deliverables in order
5. Keep every change green against §9 gates
6. Commit at each phase exit gate
