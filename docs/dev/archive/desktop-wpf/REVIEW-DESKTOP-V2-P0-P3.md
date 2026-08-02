# Review — Desktop V2 P0–P3

> **Branch:** `feat/desktop-v2` | **Date:** 2026-06-30
> **Read with:** `docs/dev/PLAN-DESKTOP-V2.md` (the build plan)
> **For:** A fresh agent/reviewer. Resume at the end of this file.

---

## 1. What the plan asked for (P0–P3)

The plan lives at `docs/dev/PLAN-DESKTOP-V2.md`. It defines 5 deliverable phases to rebuild the DevContext desktop app inside the existing `src/DevContext.App` skeleton (Tauri + Angular 22 + gRPC), replacing the old spike's single `Workspace` component with an 8-route fixed-grid app shell and new feature views.

### P0 — Shell & system spine
- Fixed CSS-grid app-shell with title bar, activity bar (icon rail), sidebar slot, status bar
- 8 lazy-loaded routes (Source, Overview, Entries, Browse, Trace, Document, Stats, Cache)
- `ActivityService` (single busy truth) + `OperationController` (cancellable ops)
- Extend vibe tokens, add `overflow:hidden` scroll laws, `prefers-reduced-motion`
- Delete command-palette, drawer, kbd-hint, old `workspace.ts` framing, ⌘K handler
- Server: wire `AnalyzeRequest.cleanup` (auto/keep) — the only missing server item from the spike audit

### P1 — Intake + Overview + Cache
- **Source intake:** port WPF `ConfigPanel.razor` — URL/path input, debounced validation with status pill, Tauri folder picker (deferred), recents chips, advanced options (depth, detail, no-roslyn, dry-run, format, cleanup auto/keep)
- **Overview:** structured `GetMap` — archetype card, style + confidence, project topology (simple boxes/arrows), packages, aggregates, pipeline behaviors, library surface, scope note
- **Cache:** clone management, cache footprint, recents management, hit-rate display

### P2 — Entry Points + Browse/filter
- **Entry Points:** `ListEntryPoints` grouped by kind (HTTP, Bus Consumers, Hosted Services, Scheduled Jobs, Domain Events, Public API), kind filter, text search, approx badges, click → trace
- **Browse:** `SearchNodes`-driven node table, text filter, kind/tag facets, node detail card (`GetNode`), Calls-out/Called-by/Find-usages (`GetNeighbors` direction out/in/usages)

### P3 — Document/Export + Stats + choreographed progress
- **Document:** `Render` RPC, section rail with include toggles, live token total vs budget slider, LLM/export view, copy/save
- **Stats:** dual page — Run (`GetStats` stage waterfall, per-extractor timings, cache hit-rate, funnel, seams) + Code (graph nodes/edges/entries/wired, corpus files/projects, seams)
- **Stage stepper:** 7-stage progress indicator that lights up during analysis, Stats code panel fills live

---

## 2. What was actually built

### Shell & routing

| File | Role |
|---|---|
| `src/app/shell/app-shell.ts` | Fixed CSS-grid: TitleBar (36px) \| ActivityBar (48px) + Sidebar (240px) + `<router-outlet>` \| StatusBar (24px). Activity bar renders 8 icon-rail links with `routerLinkActive`. |
| `src/app/shell/title-bar.ts` | Tauri drag region + connection dot (green/red from `ConnectionStore`) |
| `src/app/shell/status-bar.ts` | Binds `ActivityService` (busy dot + label + % + stage) or session summary when ready |
| `src/app/app.config.ts` | 8 lazy routes + wildcard redirect to `/` |
| `src/app/app.ts` | Root component uses `<app-app-shell />` |

### Activity infrastructure

| File | Role |
|---|---|
| `src/app/core/activity/activity.service.ts` | Signal store: `label`, `stage`, `percent`, `state` (idle/busy/error). `start()` returns `OperationController` and sets busy. `setProgress()` from analyze stream. `setError()` with 5s auto-clear. |
| `src/app/core/activity/operation-controller.ts` | Wraps cancellable ops. `run(fn)` accepts abort signal. `cancel()` aborts and sets `_cancelled` flag. New analyze cancels prior via `_cts?.abort()`. |

### Session store (rewired)

`src/app/state/session.store.ts`:
- Injects `ActivityService` — `analyze()` calls `activity.start()`, progress callbacks call `activity.setProgress()`
- `busy` = `computed(() => this._status() === 'analyzing' \|\| this._status() === 'cloning')` (boolean)
- `ready` = `computed(() => this._status() === 'ready')` (boolean)
- `entryCount` = `computed(() => ...)` (number)
- `cancel()` delegates to `activity.controller?.cancel()`

### 8 routed views

| Route | File | Status |
|---|---|---|
| `/` | `features/source/source-view.ts` | **Built** — URL/path input, 400ms debounced validation (ping-based, shows checking/valid/no-git), recents chips, advanced options (depth 1–10, detail salient/signature/full, no-roslyn checkbox, dry-run checkbox, cleanup auto/keep radio), stage stepper during analyze |
| `/overview` | `features/overview/overview-view.ts` | **Built** — structured GetMap cards: archetype + confidence%, project count/style, topology (boxes with dependency arrows), packages, aggregates, pipeline behaviors, library surface (groups + extension points), scope note. Uses `DecimalPipe` for confidence %. |
| `/entries` | `features/entries/entries-view.ts` | **Built** — grouped by kind, text filter + kind toggle, approx badges, click → navigates to `/trace` with `TraceStore.trace()`. |
| `/browse` | `features/browse/browse-view.ts` | **Built** — `SearchNodes` via `effect()` on query signal, select node → detail card (kind, in/out degree, file:line, tags), neighbor tabs (out/in/usages), approx badges on syntactic edges. Maps proto responses to `NodeDetailVm`/`EdgeVm` shapes. |
| `/trace` | `features/trace/trace-view.ts` | **Placeholder** — empty "coming in P4" message |
| `/document` | `features/document/document-view.ts` | **Built** — Render button calls `api.render()`, section rail with include toggles, budget slider (0–10000), live token total (`totalTokens()`), copy-to-clipboard, markdown content pane |
| `/stats` | `features/stats/stats-view.ts` | **Built** — Load Stats button calls `api.getStats()`, dual-column: Run (stages waterfall, extractor timings, cache hit/miss, funnel) + Code (graph nodes/edges/entries/wired, corpus files/C#/projects, seams with approx). |
| `/cache` | `features/cache/cache-view.ts` | **Built** — session summary stats + wiring coverage %, recent repos list with per-item delete button, clone management info |

### Progress

| File | Role |
|---|---|
| `src/app/ui/stage-stepper/stage-stepper.ts` | 7-stage indicator (Discover → Structure → Seal → Deep → Score → Compress → Render). Each stage lights up when `activity.percent()` exceeds its threshold. Shown in source view during analysis. |

### Server

| File | Change |
|---|---|
| `src/DevContext.Server/Sessions/AnalysisContracts.cs` | `AnalyzeSpec` gained `Cleanup` field |
| `src/DevContext.Server/Sessions/IEngineRunner.cs` | `EngineResult` gained `Cleanup` field |
| `src/DevContext.Server/Sessions/EngineRunner.cs` | Passes `spec.Cleanup` to `EngineResult` |
| `src/DevContext.Server/Sessions/AnalysisSession.cs` | `DisposeAsync` skips clone cleanup when `Cleanup == "keep"` |
| `src/DevContext.Server/Endpoints/DevContextGrpcService.cs` | Reads `request.HasCleanup ? request.Cleanup : null` into `AnalyzeSpec` |

### Vibe tokens

`src/styles.css`:
- Added `--vibe-border-width: 1px` token
- Added `overflow: hidden` to `html, body` (scroll laws)
- Added `@media (prefers-reduced-motion: reduce)` query that zeroes animation/transition durations

### Deleted

- `ui/command-palette/`, `ui/drawer/`, `ui/kbd-hint/` — entire directories
- 13 old spike feature directories: `workspace/`, `source-bar/`, `launcher/`, `map-panel/`, `trace-panel/`, `story-panel/`, `entries-panel/`, `node-detail/`, `entity-cross-ref/`, `llm-export/`, `stats-drawer/`, `health-dashboard/`, `impact-panel/`, `repo-browser/`, `shortcuts/`

### Kept (unchanged infrastructure)

`core/grpc/`, `data-access/`, `core/theme/`, `state/connection.store.ts`, `state/trace.store.ts`, `state/github.store.ts`, `state/recent.store.ts`, `models/`, all `ui/` primitives (icon, badge, button, card, panel, search-field, segmented, sheet, spinner, tabs, text-input, toast, toolbar), `ui/graph-canvas/` (still present but unused — P4 drops it), `features/overview/overview.ts` (dead code — P4 deletes), `features/vibe-switcher/` (dead code — P4 integrates or deletes)

---

## 3. Plan vs delivered delta

| Plan item | Delivered | Delta |
|---|---|---|
| Tauri folder/`.sln` picker | Deferred | Requires `@tauri-apps/plugin-dialog` — not yet wired. Input accepts typed paths only. |
| Debounced `git ls-remote` validation | Simplified | Validates via server Ping instead of real git-ls-remote. Shows checking → valid/no-git states. |
| Clone path + size display in intake | Not built | The advanced options show cleanup auto/keep choice. Actual clone path/size tracking not wired. |
| Cache hit-rate on re-analysis | Partial | Cache page shows summary stats + coverage %. Dynamic hit-rate display from GetStats not wired. |
| "Analyze once, render many" | Partial | Focus/option changes do re-fetch via RPCs but don't re-analyze (correct). No explicit render-only path. |
| Session guard redirect to `/` | Not built | Routes `/overview`…`/stats` don't guard against missing session — they show "Analyze a repo" empty state instead. Acceptable UX, but plan asked for redirect with hint. |
| Tauri save dialog for export | Not built | Document view has copy-to-clipboard only. |
| Stage stepper in Stats code panel | Partial | Stage stepper shown in Source view during analyze. Stats page requires manual "Load Stats" click — doesn't fill live. |

---

## 4. Architecture rules (self-check)

| Rule from plan §1 | Held? |
|---|---|
| **Nothing blocks. Everything is cancellable.** | Yes — `OperationController` wraps analyze; cancel aborts; new analyze cancels prior. |
| **One truth for "busy."** | Yes — only `ActivityService` drives the status bar. |
| **The shell never moves.** | Yes — `display:grid; height:100vh; overflow:hidden`; only `<main>` scrolls. |
| **Reactivity is fine-grained.** | Yes — signals all the way down; no `OnPropertyChanged(string.Empty)`. |
| **Waiting is a feature.** | Partial — stage stepper exists; Stats don't fill live during analyze. |
| **Truthful.** | Yes — `AnalyzeRequest.cleanup` honored; clone controls visible in advanced options. |

---

## 5. Gates (all green)

```powershell
dotnet build DevContext.slnx -clp:ErrorsOnly        # 0 warnings, 0 errors
dotnet test tests/DevContext.Server.Tests             # 12/12 passed
```
```powershell
# From src/DevContext.App
pnpm check                                            # lint pass, 4/4 tests, build success
```
Bundle: **405 KB** initial (plan budget: cold start ≤ 2s — Tauri WebView2, sufficient).

---

## 6. How to smoke test

```powershell
# Terminal 1
pushd src\DevContext.App; pnpm server

# Terminal 2
pushd src\DevContext.App; pnpm dev:web
# Open http://localhost:4200
```

1. **Shell:** Grid renders with title bar (connection dot), activity bar (8 icons), empty main area, status bar ("Ready")
2. **Intake:** Enter a local `.sln` path or `C:\Code\DevContext2` → click Analyze → stage stepper lights up, status bar shows progress
3. **After analysis:** Status bar shows "X entries". Navigate via activity bar to Overview, Entries, Browse, Stats, Cache, Document
4. **Cancel:** Click Analyze again during analysis → previous cancels
5. **Navigate:** Click any activity bar icon → route changes, main area updates
6. **Recents:** After first analysis, recents chips appear in Source view. Click one → re-analyzes.

---

## 7. Known gaps / deferred to P4

- **Trace view** — placeholder; needs narrative tree + small SVG path graph (no Cytoscape), focus picker
- **Vibe polish** — `modern-light` CSS exists but switcher doesn't render it; terminal/hacker need visual pass
- **a11y** — no keyboard nav audit, no axe-core run
- **Live stats during analyze** — stats page needs to subscribe to progress stream
- **Tauri folder picker** — `@tauri-apps/plugin-dialog` not integrated
- **Session route guard** — routes don't redirect to `/` when no session
- **Save-to-file** — document view only copies to clipboard
- **Old dead code** — `features/overview/overview.ts`, `features/vibe-switcher/`, `ui/graph-canvas/` still present
- **Cytoscape** — still in `package.json`; unused (P4 removes)

---

## 8. Resume for a new session

```powershell
git checkout feat/desktop-v2
dotnet build DevContext.slnx -clp:ErrorsOnly
pushd src\DevContext.App; pnpm check; popd
```

Read `docs/dev/PLAN-DESKTOP-V2.md` §4–§7 for P4 scope (Trace + vibes/polish + a11y). Read §11 for this session log. Start at P4.0 (Trace view).
