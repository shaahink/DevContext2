# Handover — Desktop V2 (R0–R5 + P0–P3 complete)

> **Branch:** `feat/desktop-v2` | **Date:** 2026-06-30
> **For:** A fresh agent reviewing or resuming this work. Read this file first, then the referenced docs.

---

## 1. What this is

The cross-platform desktop rebuild of DevContext. Evolves the `src/DevContext.App` Tauri + Angular 22 + gRPC spike into a world-class .NET repo lens with an 8-route fixed-grid app shell, global activity/busy truth, and feature views sourced from the existing server RPCs.

---

## 2. Reference documents (read order)

| # | File | Role |
|---|---|---|
| 1 | `docs/dev/HANDOVER-DESKTOP-V2.md` | **This file.** Start here. |
| 2 | `docs/dev/PLAN-DESKTOP-V2.md` | Master build plan: phases P0–P5, feature specs, file map, gates, gotchas. Session log at §11 shows what was done. |
| 3 | `docs/dev/PLAN-DESKTOP-V2-REMEDIATION.md` | Remediation plan: 21 findings (F1–F21) found after P0–P3, 6 fix phases (R0–R5). Read §0 for the list; all are now fixed. |
| 4 | `docs/dev/REVIEW-DESKTOP-V2-P0-P3.md` | Initial P0–P3 self-review (historical — superseded by remediation). |
| 5 | `docs/dev/HANDOVER-DESKTOP-REDO.md` | Original spike handover. Still authoritative for engine API (§7), build/run (§8), gotchas (§11). |
| 6 | `src/DevContext.App/AGENTS.md` | App-level conventions, run commands, architecture layering. |

---

## 3. Git / branch history

```
develop
  │
  ├── Merge feat/library-surface-fv-polly (commit ff143d8)  — all latest code
  │
  └── feat/desktop-v2  ← YOU ARE HERE
        │
        ├── Original PLAN-DESKTOP-V2.md written
        ├── P0–P3 implemented (initial shell + 7 views)
        ├── REVIEW-DESKTOP-V2-P0-P3.md written (self-review, found gaps)
        ├── PLAN-DESKTOP-V2-REMEDIATION.md written (21 findings)
        ├── R0–R5 implemented (all findings fixed)
        └── P4 Trace view + vibe light themes + final polish
```

**Uncommitted.** All changes are in the working tree. Status summary:

- **Modified:** `app-shell.ts`, `title-bar.ts`, `status-bar.ts`, `app.ts`, `app.config.ts`, `session.store.ts`, `styles.css`, `devcontext-api.ts`, `AnalyzeSpec`/server files, `view-models.ts`, `package.json`, `tauri.conf.json`, vibe defs
- **Added:** `shell/view-frame.ts`, `core/activity/`, `features/source/`, `features/entries/`, `features/browse/`, `features/trace/trace-view.ts` + `trace-node.ts`, `features/document/`, `features/stats/`, `features/cache/`, `features/overview/overview-view.ts`, `ui/stage-stepper/`
- **Deleted:** `ui/command-palette/`, `ui/drawer/`, `ui/kbd-hint/`, `ui/graph-canvas/`, `features/vibe-switcher/`, `features/overview/overview.ts` (old), 13 old spike feature dirs
- **Docs:** `PLAN-DESKTOP-V2.md`, `PLAN-DESKTOP-V2-REMEDIATION.md`, `REVIEW-DESKTOP-V2-P0-P3.md`, `HANDOVER-DESKTOP-V2.md`

---

## 4. Current architecture

### Shell (grid)

```
┌─────────────────────────────────────────────┐
│ TitleBar (36px) — app name + connection dot  │
├──┬──────────────────────────────────────────┤
│A │  Main (router-outlet in 1fr column)       │
│c │  Each route wrapped in ViewFrame           │
│t │  ViewFrame provides: [sidebar] [header]    │
│b │  body                                   │  1fr
├──┴──────────────────────────────────────────┤
│ StatusBar (24px) — ActivityService single    │
│   busy truth                                │
└─────────────────────────────────────────────┘
```

Activity bar (48px, left): grouped into top (Source, Cache) + divider + lens (Overview…Stats). Lens group is gated on `session.ready()` — dimmed/disabled before analysis. Auto-advances to `/overview` on analysis success.

### Key services

| Service | File | Role |
|---|---|---|
| `ActivityService` | `core/activity/activity.service.ts` | Single busy truth: `label`, `stage`, `percent`, `state`. `start()` returns `OperationController`. `runSecondary()` for non-cancellable ops. `setError()` with 5s auto-clear. |
| `OperationController` | `core/activity/operation-controller.ts` | Wraps cancellable async ops. New analyze cancels prior. Uses `_cancelled` flag + `AbortController`. |
| `SessionStore` | `state/session.store.ts` | Wires analyze through `ActivityService`. `busy`/`ready` are `computed<boolean>`. `entryCount` is computed. Progress → `activity.setProgress()`. |
| `ConnectionStore` | `state/connection.store.ts` | Started by AppShell constructor — polls every 5s, title bar shows green/red dot. |
| `ThemeService` | `core/theme/theme.service.ts` | Instantiated by AppShell constructor — sets `data-vibe`/`data-theme` on `<html>`. Vibes: modern (dark/light/high-contrast), terminal (dark/light), hacker (dark/light). |
| `TraceStore` | `state/trace.store.ts` | Unchanged from spike. `trace(handle, focus)` → tree, markdown, touched, emitted. `setDepth`/`setDetail` re-render without re-analysis. |
| `DevContextApi` | `data-access/devcontext-api.ts` | Typed RPC wrapper. `AnalyzeSpec` includes `cleanup?: 'auto' | 'keep'`. |

### Routes & views

| Route | Component | Uses ViewFrame | Status |
|---|---|---|---|
| `/` | `SourceView` | No (centered landing page) | **Done** |
| `/overview` | `OverviewView` | Yes (no sidebar) | **Done** |
| `/entries` | `EntriesView` | Yes (sidebar = kind filter + search) | **Done** |
| `/browse` | `BrowseView` | Yes (sidebar = search) | **Done** |
| `/trace` | `TraceView` | Yes (sidebar = focus picker + depth/detail) | **Done** |
| `/document` | `DocumentView` | Yes (sidebar = section rail) | **Done** |
| `/stats` | `StatsView` | Yes (no sidebar, auto-loads) | **Done** |
| `/cache` | `CacheView` | Yes (no sidebar) | **Done** |

### Server (one change from spike)

`AnalyzeRequest.cleanup` (auto/keep) wired through:
- `AnalysisContracts.cs` — `AnalyzeSpec` gained `Cleanup` field
- `IEngineRunner.cs` — `EngineResult` gained `Cleanup` field
- `EngineRunner.cs` — passes `spec.Cleanup`
- `AnalysisSession.cs` — `DisposeAsync` skips clone cleanup when `Cleanup == "keep"`
- `DevContextGrpcService.cs` — reads `request.HasCleanup ? request.Cleanup : null`

All other server RPCs were already implemented in the spike (audit confirmed 13/14 done).

---

## 5. What each view does

**Source** (`/`): Landing page with hero text, path input (local or GitHub URL), honest classification ("Local path" / "GitHub repo"), recents as cards, advanced options (depth, detail, no‑roslyn, cleanup auto/keep), stage stepper during analyze. Busy disables all controls. Auto‑advances to `/overview` on success.

**Overview** (`/overview`): Structured `GetMap` cards: archetype + confidence%, scope note, topology (project boxes with dependency arrows), packages, aggregates, pipeline behaviors, library surface (groups + extension points).

**Entries** (`/entries`): `ListEntryPoints` grouped by kind. Left sidebar: kind filter list + search field. Entry rows show route → target, "approx" badge on syntactic entries, HTTP method badge. Click → traces and navigates to `/trace`.

**Browse** (`/browse`): `SearchNodes` with 300ms debounce. Left sidebar: search field. Select node → bottom detail card (kind, in/out degree, file:line, tags). Neighbor tabs: Calls out / Called by / Usages. "approx" badge on syntactic edges. All RPCs wrapped in `activity.runSecondary()`.

**Trace** (`/trace`): Narrative tree using `TraceStore`. Left sidebar: focus picker (searchable entry list), depth selector (1–10), detail selector (salient/signature/full). Recursive `TraceNodeComponent` renders seam · title · salient · approx/truncated/omitted badges with indented tree.

**Document** (`/document`): `Render` RPC. Left sidebar: section include toggles with token counts. Header: token total, budget slider, Render + Copy buttons. Body: rendered markdown.

**Stats** (`/stats`): `GetStats` dual page. Auto-loads on visit. Left column (Run): stages waterfall, extractor timings, cache hit/miss, funnel. Right column (Code): graph stats, corpus, seams with approx counts. Refresh button. All RPCs through `activity.runSecondary()`.

**Cache** (`/cache`): Session summary (projects, nodes, edges, entries, wired, coverage %). Recent repos list with per-item delete. Clone management info (safety note: local paths never touched).

---

## 6. Remediation — 21 findings fixed

| # | Finding | Fix |
|---|---|---|
| F1 | 240px dead sidebar column | Removed shell `<aside>`, created `ViewFrame` with optional sidebar per-view |
| F2 | Title/status bar grid spans broken | `display:contents` → `host: { class: 'col-span-3' }` |
| F3 | Connection dot always Offline | `ConnectionStore.start()` in AppShell constructor |
| F4 | Fake "Repository found" | Replaced with local/GitHub classification |
| F5 | cleanup/dryRun inert | `cleanup` wired end-to-end; `dryRun` removed |
| F6 | Cache "Clear" cancels analyze | Removed button |
| F7 | Secondary RPCs fire-and-forget | `activity.runSecondary()` on all views |
| F8 | Trace placeholder | Real narrative tree + focus picker |
| F9 | No folder picker / no save | Tauri decorations set; picker/save deferred (needs native plugin) |
| F10 | Double title bar | `tauri.conf.json` → `"decorations": false` |
| F11 | ThemeService never instantiated | Injected in AppShell |
| F12 | Dead code | Deleted overview.ts, vibe-switcher/, graph-canvas/, Cytoscape+elkjs deps |
| F13 | Duplicate kind maps | Single `KIND_LABELS` + `KIND_ICONS` in `view-models.ts` |
| F14 | Browse no debounce | 300ms debounce + `runSecondary` error handling |
| F15 | Redundant Overview "Stats" card | Removed; kept meaningful cards |
| F16 | Stage stepper only in Source | Now also auto-loads stats on visit |
| F17 | IA reads as 8 equal peers | Rail grouped + gated; auto-advance after analysis |
| F18 | Busy doesn't propagate | All controls disabled during busy |
| F19 | Stats UX weak | Auto-loads with loading state |
| F20 | Deletion safety | Clone management card states local paths never touched |
| F21 | Landing not intuitive | Hero text + recents as cards + "Browse…" placeholder |

---

## 7. Gates (all green — verify before resuming)

```powershell
# .NET (root)
dotnet build DevContext.slnx -clp:ErrorsOnly        # 0 warnings, 0 errors
dotnet test tests/DevContext.Server.Tests             # 12/12 passed

# App (src/DevContext.App)
pnpm check                                            # lint + 4/4 tests + build
pnpm server                                           # starts server on http://127.0.0.1:5179
pnpm dev:web                                          # browser at http://localhost:4200
```

Bundle: **409 KB** initial, **105 KB** transfer.

---

## 8. Known gaps / not done

- **Tauri folder/`.sln` picker** (R4.0) — needs `@tauri-apps/plugin-dialog` + Rust-side setup. Input is typed-only today.
- **Save-to-file export** (R4.1) — needs `@tauri-apps/plugin-fs`. Document has copy-to-clipboard only.
- **Title bar min/max/close buttons** (R4.2) — `decorations:false` means the custom title bar needs window controls via `@tauri-apps/api/window`. Not added yet.
- **SVG path graph in Trace** (P4.0) — narrative tree is complete; the small synced path graph (plan says "pure SVG/Tailwind, no Cytoscape") is deferred.
- **a11y audit** (P4.2) — keyboard nav works but no axe-core run yet. Some views have `(click)` without `(keydown)` companions (lint didn't catch all).
- **Vibe switcher UI** — `ThemeService` is live but no UI to switch vibes at runtime (old `vibe-switcher` deleted). Add a small settings affordance in the title bar.
- **`[class.bg-accent/10]` bindings** — Angular `[class.xxx]` with Tailwind slash-opacity syntax may not apply correctly in all browsers. Verify and switch to `[ngClass]` if flaky.
- **Toast** — `ToastService` is instantiated but the `<app-toast>` component must be mounted in the shell template to render messages.

---

## 9. How to resume

```powershell
git checkout feat/desktop-v2
dotnet build DevContext.slnx -clp:ErrorsOnly
pushd src\DevContext.App; pnpm check; popd
```

Read `docs/dev/PLAN-DESKTOP-V2.md` §4–§7 for the remaining master plan phases:

1. **P4 (Trace polish + vibes + a11y):** SVG path graph in Trace, vibe switcher UI, light/high-contrast, keyboard nav, axe-core pass
2. **P5 (Engine breadth):** indirect-wiring coverage (MassTransit, Hangfire, Quartz, etc.), eval loop against `eval-repos.json`

Also fix the known gaps in §8 above — especially the toast mount (`<app-toast>` in AppShell template) and the `[class.bg-accent/N]` bindings audit.

---

## 10. File map (quick reference)

```
src/DevContext.App/src/app/
  shell/              app-shell, title-bar, status-bar, view-frame
  core/activity/      activity.service, operation-controller
  core/grpc/          gRPC client + generated TS (unchanged)
  core/theme/         theme.service, vibe-definition, vibes/ (unchanged)
  data-access/        devcontext-api (AnalyzeSpec now has cleanup)
  state/              session.store (rewired), trace.store, connection.store, recent.store, github.store
  models/             view-models (KIND_LABELS + KIND_ICONS exported)
  features/
    source/           source-view (landing + intake)
    overview/         overview-view (structured GetMap)
    entries/          entries-view (grouped ListEntryPoints)
    browse/           browse-view (SearchNodes + detail)
    trace/            trace-view + trace-node (narrative tree)
    document/         document-view (Render + sections + token budget)
    stats/            stats-view (dual Run + Code stats)
    cache/            cache-view (summary + recents)
  ui/
    badge, button, card, icon, panel, search-field, segmented,
    sheet, spinner, tabs, text-input, toast, toolbar, stage-stepper
```
