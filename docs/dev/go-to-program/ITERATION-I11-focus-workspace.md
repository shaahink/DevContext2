# Iteration I11 — Focus Workspace (unified context-thinking shell)

> **Status: SPEC · READY** · Authored 2026-07-02 · Branch: `develop`
> **Depends on:** I4 (workspace shell) · I10 (tabs — wired per §1) · I8 (snapshot cache — nice to have)
> **Supersedes:** UI-UX-GUIDELINES §1 navigation table (3 routes collapse to 1), §4 entries (listbox replaces table), §5 graph/trace (folded into stage)
> **Complements:** GAP-TRACKER.md (all 23 gaps addressed)
> **Synthesis of:** C proposal (discipline: j/k scrub, history stack, progressive trace, esc ladder) + CT proposal (vision: context thinking, smart dock, quick peek, nerd mode, breadcrumb, Ctrl+Z/Y undo)

## Cold-Agent Resume Protocol

```powershell
# Start here every time
git -C C:/Code/DevContext2-ui checkout feat/ui-iteration
git -C C:/Code/DevContext2-ui pull
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # must be green before starting

# Read this spec, then execute phases in order — never skip.
# Each phase lists its gate command. Run it before moving on.
```

---

## 0. Philosophy — From Page-Thinking to Context-Thinking

**Before (current):**
```
User thinks: "I need to see entries → click Entries → click row → click Trace page → see tree"
Each feature IS a page. The user navigates between destinations.
4 clicks per entry exploration.
```

**After (target):**
```
User thinks: "I want to explore this repo"
One view. User moves a selection (j/k). Everything updates in place.
Context Panel, Stage, Breadcrumb, MiniGraph — all projections of one selection state.
1 click (or zero clicks with j/k) per entry exploration.
```

**Core rule: selection is the only state that moves.** `TraceStore.focus` already is global. The Context Panel reads it. The Stage reads it. The Breadcrumb reads it. Nothing else changes independently.

**Visual rule: this is an IDE, not a web page.** No card shadows. No rounded corners on panels. No heavy hover bg changes. Dividers separate sections, not borders with radius. Tight padding. Native OS scrollbars. Flat buttons, ghost by default.

---

## 1. Target Layout — The Complete Shell

```
┌── TitleBar (30px) ──────────────────────────────────────────────────────────────────────────────────┐
│  DevContext    [📁 eShop ▾]   [Search Everything… Ctrl+P]      [Analyze] [Settings]    ─  □  ✕     │
├── TabStrip (32px) ──────────────────────────────────────────────────────────────────────────────────┤
│  ⬤ eShop  ×   TodoApi  ×   DntSite  ×                                                         [+] │
├── Breadcrumb (22px) — only visible when hasHistory ─────────────────────────────────────────────────┤
│  eShop › OrdersController › GET /api/orders › OrderService.Process                                 │
├──────┬──────────────────────────────────────────────┬───────────────────────────────────────────────┤
│      │                                              │                                               │
│ Act- │          EXPLORER                            │          CONTEXT PANEL                        │
│ ivity│                                              │  (toggleable: Ctrl+Shift+L, Smart Dock 1-4)  │
│ Bar  │ ┌ EntryListbox ────┬── Stage ──────────────┐ │                                               │
│ (48px│ │ / filter…         │ [Trace │ Graph]       │ │  ┌ Details ─────────────────────────┐     │
│ icons│ │                   │ [call][send][handler] │ │  │ GET /api/orders                    │     │
│ only)│ │ GET /orders   HTTP│                       │ │  │ HTTP · GET                          │     │
│      │ │ POST /orders  HTTP│ OrderController       │ │  │ OrdersController.cs:42              │     │
│  ○ H │ │ GET /customers    │ ├ call OrderService    │ │  │ Auth: [anon]                        │     │
│  ◉ E │ │ GET /report       │ │  ├ PricingService    │ │  └────────────────────────────────────┘     │
│  ◎ A │ │                   │ │  └ Repository        │ │                                               │
│  ◎ I │ │                   │ └ call AuthService     │ │  ┌ Call Stack (depth 2) ─────────────┐     │
│  ◎ X │ │                   │                       │ │  │ OrderService.Process                │     │
│      │ │                   │ ▐▐ mini graph ▐▐ [exp]│ │  │ ├ PricingService.Calculate [approx] │     │
│  ⚙ S │ └───────────────────┴──────────────────────┘ │  │ └ Repository.Save [verified]         │     │
│      │                                              │  └────────────────────────────────────┘     │
│      │                                              │                                               │
│      │                                              │  ┌ Metrics ──────────────────────────┐     │
│      │                                              │  │ ~142 LOC · CC ~12                  │     │
│      │                                              │  │ Fan-in 9 · Fan-out 18              │     │
│      │                                              │  │ Instability 0.67                   │     │
│      │                                              │  └────────────────────────────────────┘     │
│      │                                              │                                               │
│      │                                              │  ┌ Insights ─────────────────────────┐     │
│      │                                              │  │ ⚠ Missing auth on POST /orders     │     │
│      │                                              │  │ ⓘ Possible N+1 in Process           │     │
│      │                                              │  └────────────────────────────────────┘     │
│      │                                              │                                               │
│      │                                              │  ┌ LLM ──────────────────────────────┐     │
│      │                                              │  │ The GET /orders endpoint…           │     │
│      │                                              │  │ [Copy] [Explain] 2.3K tok           │     │
│      │                                              │  └────────────────────────────────────┘     │
│───┴──────────────────────────────────────────────────┴───────────────────────────────────────────────┤
│ StatusBar (22px)                                                                                     │
│ eShop · 94 entries · 512 nodes · 89% wired      ● v1.2.3 online     terminal                        │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Panel sizing

| Dock level | Shortcut | Context Panel | Explorer |
|-----------|----------|---------------|----------|
| 0 — hidden | Ctrl+1 | 0% (collapsed to 28px token strip) | 100% |
| 1 — compact | Ctrl+2 | 30% (~360px at 1200w) | 70% |
| 2 — default | Ctrl+3 | 40% (~480px at 1200w) | 60% |
| 3 — focus | Ctrl+4 | 100% (fullscreen Context) | 0% |

Dock level persisted in `PrefsStore` per machine. Resize handles exist between panels as 4px draggable dividers. Drag sets a custom width; Ctrl+1-4 toggles between presets.

### Activity Bar (replaces Navigation Rail)

| Icon | Name | Route | Badge | Disabled when |
|------|------|-------|-------|---------------|
| `map` | Home | `/overview` | — | Never |
| `layers` | Explore | `/explore` | entry count | No session |
| `boxes` | Architecture | `/architecture` | — | No session |
| `info` | Insights | `/insights` | insight count | No session |
| `file-text` | Export | `/export` | — | No session |
| `settings` | Settings | `/settings` | — | Never |

Notes:
- Icons-only (no labels). Tooltip on hover shows name + shortcut (g+letter).
- Existing icon registry already has `map`, `layers`, `boxes`, `info`, `file-text`, `settings` — no new lucide imports needed (fixes GAP-S7 by using existing icons).
- Count badge: small circle (12px) with number inside, positioned top-right over icon. Only shows when count > 0 and session is ready. White text on accent bg.
- Disabled items: `opacity-40`, `pointer-events-none`, tooltip says "Analyze a repo first" (fixes GAP-S5).
- Active indicator: left border accent (3px line), not background fill.

---

## 2. Component Map — What Changes

### New Components (to create)

| File | Selector | Replaces | Lines (est) |
|------|----------|----------|-------------|
| `features/explorer/entry-listbox.ts` | `app-entry-listbox` | `section-entries.ts` table | ~120 |
| `features/explorer/progressive-trace-tree.ts` | `app-progressive-trace-tree` | `trace-node.ts` | ~100 |
| `features/explorer/mini-graph.ts` | `app-mini-graph` | (new) | ~50 |
| `features/explorer/seam-filter-chips.ts` | `app-seam-filter-chips` | (new) | ~40 |
| `features/explorer/stage.ts` | `app-stage` | `section-trace.ts` + `section-graph.ts` | ~80 |
| `features/context/context-panel.ts` | `app-context-panel` | `section-lens.ts` | ~180 |
| `features/context/quick-peek.ts` | `app-quick-peek` | (new) | ~80 |
| `shell/breadcrumb.ts` | `app-breadcrumb` | (new) | ~50 |
| `features/export/export-drawer.ts` | `app-export-drawer` | `section-export.ts` modal | ~140 |
| `features/context/nerd-metrics.ts` | (inline utility) | — | ~30 |

### Modified Components

| File | Changes |
|------|---------|
| `shell/workspace-shell.ts` | New layout: `TitleBar` + `TabStrip` + `Breadcrumb` + HorizSplit (ActivityBar \| Explorer \| ContextPanel) + `StatusBar`. Smart Dock resize handles. Add `ExportDrawer` to template. Update keyboard handler for all new shortcuts. Update help overlay with full keyboard map. |
| `shell/header/app-header.ts` → rename to `shell/titlebar/titlebar.ts` | Rename selector to `app-titlebar`. Reduce height to 30px. Remove `bg-base/80 backdrop-blur` — use solid `bg-base`. Remove shadow. Flatten buttons. Move "Search Everything" input into titlebar center (replaces dead `[analyze]` slot already removed). Add `Ctrl+P` shortcut to focus search. |
| `shell/navigation-rail.ts` → rename to `shell/activity-bar/activity-bar.ts` | Rename selector to `app-activity-bar`. 5 items (was 7). Icons only. Add count badges. Disabled items visible (fixes GAP-S5/S6). Active = left border accent. Fix `shortLabel`/last-segment bug (GAP-T3). |
| `shell/tab-strip.ts` | Wire into workspace-shell (GAP-T1). Add duplicate-path guard in SessionStore.analyze() (GAP-T4). MRU cycle with Ctrl+Tab (GAP-T5). |
| `features/palette/palette.ts` | Add debounce 150ms on node search (GAP-B1). Add Tab verb cycling (GAP-B2). Add Recents section (GAP-B3). Add kind icons on node results (GAP-B4). Fix "no results" text to include query (GAP-B5). Route names updated: "Go to Explore" replaces "Go to Entries"/"Go to Trace"/"Go to Graph". |
| `features/settings/settings-view.ts` | Smart Dock preference (numbered toggle 0-3). PrefsStore read/write already done. |
| `features/narrative/section-identity.ts` | Restyle: no SectionCard wrapper. Tight 1px divider separators. Edge-to-edge. Already has empty state. |
| `features/narrative/section-architecture.ts` | Promoted to own route `/architecture`. Restyle: no card. Edge-to-edge. Already has empty state. |
| `features/narrative/section-console.ts` | Restyle: no card. Tight padding. Keep `afterEveryRender` auto-scroll but guard mode check. |
| `features/narrative/section-stats.ts` | Restyle: no card. Already has error state. |
| `features/insights/insights-view.ts` | Restyle: no card wrappers per insight. Flat list with 1px dividers. Already has loading/error/empty states. |
| `features/node-card/node-card.ts` | Skeleton loading state instead of text "Loading..." (GAP-B8). Already has error state. |
| `shell/footer/app-footer.ts` | Rename to `shell/statusbar/statusbar.ts`, selector `app-statusbar`. Reduce height to 22px. Text-2xs throughout. Sections separated by thin vertical lines. Already has connection dot + progress. Add `Ctrl+R` hint for re-analyze. |

### Removed Files

| File | Why |
|------|-----|
| `features/narrative/section-card/` (directory) | Replaced by panel dividers. No component in the new design uses card wrappers. |
| `features/narrative/section-entries.ts` | Replaced by `EntryListbox` (inside Explorer) |
| `features/narrative/section-trace.ts` | Folded into `Stage` (inside Explorer) |
| `features/narrative/section-graph.ts` | Folded into `Stage` + `MiniGraph` |
| `features/narrative/section-lens.ts` | Replaced by `ContextPanel` |
| `features/narrative/section-export.ts` | Replaced by `ExportDrawer` (in shell, not a page) |
| `features/pages/entries-page.ts` | `/entries` redirects to `/explore` |
| `features/pages/trace-page.ts` | `/trace` redirects to `/explore` |
| `features/pages/graph-page.ts` | `/graph` redirects to `/explore` |
| `features/pages/export-page.ts` | `/export` stays but simplified — drawer trigger + section presets |

### Stores — Changed

| Store | Change |
|-------|--------|
| `WorkspaceStore` | Add `historyStack: HistoryEntry[]` per tab (type: `{focus: string, stageMode: 'trace'|'graph', expandedNodes: string[]}[]`). Methods: `pushHistory(tabId, entry)`, `undoHistory(tabId)`, `redoHistory(tabId)`. Cap 50 entries, FIFO eviction. Add `mruStack: string[]` for Ctrl+Tab MRU (GAP-T5). Add `lastAnalyzedPath` on TabState for focus-restore on re-analyze. |
| `SessionStore` | `analyze()` — duplicate-path guard before createTab (GAP-T4). Save last focus + route before re-analyze. After re-analyze completes, attempt to restore previous focus by matching entry route/title. |
| `PrefsStore` | Add `dockLevel: 0 | 1 | 2 | 3` (default 2). Add `lensAutoShow: boolean` (default true — shows on first trace, then respects toggle). Schema version unchanged (just new keys added). |
| `TraceStore` | (no change — already a global facade over active tab) |

---

## 3. New Route Table

```typescript
// app.config.ts
const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./shell/workspace-shell').then(m => m.WorkspaceShell),
    children: [
      { path: '', redirectTo: '/overview', pathMatch: 'full' },
      { path: 'overview', loadComponent: () => import('./features/pages/overview-page').then(m => m.OverviewPage) },
      { path: 'explore', loadComponent: () => import('./features/pages/explore-page').then(m => m.ExplorePage) },
      { path: 'architecture', loadComponent: () => import('./features/pages/architecture-page').then(m => m.ArchitecturePage) },
      { path: 'insights', loadComponent: () => import('./features/pages/insights-page').then(m => m.InsightsPage) },
      { path: 'export', loadComponent: () => import('./features/pages/export-page').then(m => m.ExportPage) },
      { path: 'settings', loadComponent: () => import('./features/settings/settings-view').then(m => m.SettingsView) },

      // Redirects — old routes kept alive for deep links
      { path: 'entries', redirectTo: '/explore' },
      { path: 'trace', redirectTo: '/explore' },
      { path: 'graph', redirectTo: '/explore' },
    ],
  },
];
```

URL state:

| URL | What it does |
|-----|-------------|
| `/explore` | Explorer with EntryListbox, no focus |
| `/explore?focus=GET%20/api/orders` | Explorer, auto-traces that focus, Context Panel populated |
| `/explore?focus=OrderService&view=graph` | Explorer, seeds graph from node, stage shows graph |
| `/trace?focus=GET%20/api/orders` | Redirects to `/explore?focus=GET%20/api/orders&view=trace` |
| `/entries?kind=HttpEndpoint&q=order` | Redirects to `/explore?kind=HttpEndpoint&q=order` |
| `/graph?focus=OrderService` | Redirects to `/explore?focus=OrderService&view=graph` |

---

## 4. Component Specs — Behavior & Contracts

### 4.1 EntryListbox (`features/explorer/entry-listbox.ts`)

```
Selector: app-entry-listbox
Inputs:  entries: EntryVm[], selectedIndex: number, entryFilter: string
Outputs: selectionChange: EntryVm, entryFilterChange: string

─────── Behavior ──────────────────────────────────────────────────────
- Renders as a flat list, NOT a table. No grid lines. No column headers.
- Each row: Method badge (compact) + Route/title (mono) + Kind icon.
  Example: [GET] /api/orders         ◎
  No target column. No actions column.
- Filter bar at top: borderless input, `/` focuses it.
- Kind filter chips ABOVE the list (same chips as current section-entries, with count badges).
  "has target" and "approx" quick-filter chips stay (from P1.5).
- j/k move selection. Home/End jump to first/last.
- Selection is shown as a subtle highlight bar (2px left accent, slight bg shift).
- Selected entry automatically triggers trace via output (debounced 150ms).
- Enter "pins" the selection (stops following j/k until Enter is pressed again).
- Shift+E: expands to a full overlay table (the current sortable section-entries table)
  for auditing. Esc or second Shift+E collapses overlay.
- Empty state (no entries, no session): "Analyze a repo to list its entry points."
- Empty state (filtered to zero): "No entries match — clear filters" with clickable text.
────────────────────────────────────────────────────────────────────────
```

### 4.2 ProgressiveTraceTree (`features/explorer/progressive-trace-tree.ts`)

```
Selector: app-progressive-trace-tree
Inputs:  root: TraceNodeVm | null, expandedNodes: Set<string>, activeSeams: Set<string>, maxDepth: number
Outputs: nodeClick: string, toggleExpand: string

─────── Behavior ──────────────────────────────────────────────────────
- Recursive. Replaces current trace-node.ts.
- Each node: twisty (▸/▾) + seam badge (colored chip, compact) + title (mono, NodeLink).
- Indentation: 12px per depth level (tighter than current 20px).
- Seam badge colors: reuse existing SEAM_COLORS from models/seam-colors.ts.
- Resolution badges: [approx] amber, [verified] green, [truncated] neutral.
  Inline, after title, 2xs size.
- Click node → nodeClick output (TraceStore.selectNode → Context Panel updates).
- Twisties: collapsed by default past depth 2. Expand with click or → key.
- Active seams filter: only nodes with seam in activeSeams set are rendered.
  Chips above tree toggle seams (SeamFilterChips component).
- Empty state (no tree): "Trace not found for this focus."
────────────────────────────────────────────────────────────────────────
```

### 4.3 Stage (`features/explorer/stage.ts`)

```
Selector: app-stage
Inputs:  (reads TraceStore directly — traceStore.tree, traceStore.focus)
Outputs: (writes TraceStore — setDepth, setDetail, selectNode)

─────── Behavior ──────────────────────────────────────────────────────
- Hosts ProgressiveTraceTree in "trace" mode, GraphCanvas in "graph" mode.
- Toggle between modes: segmented control OR keyboard (v t / v g).
- Trace mode:
  * SeamFilterChips above tree
  * Depth selector (dropdown, 1-10)
  * Detail selector (salient/signature/full)
  * Progress bar when traceStore.loading() is true
- Graph mode:
  * Depth selector (1-4, Cytoscape performance)
  * Expand/Shrink button
  * MiniGraph thumbnail visible when in trace mode (bottom-right, 200x150px)
  * Space key: graph expands to full stage (mode switch)
  * Click node in graph → selectNode → Context Panel updates, Breadcrumb extends
  * Double-click node → expand neighborhood (re-trace from that node at depth 1 more)
  * Middle mouse → pan (Cytoscape native)
  * Mouse wheel → zoom
  * Shift+wheel → timeline through expansion history (undo/redo graph state)
  * F → focus only on selected node's neighborhood
  * Ctrl+Z while graph active → collapse previous expansion
- Empty state (no trace active): "Select an entry above to explore."
────────────────────────────────────────────────────────────────────────
```

### 4.4 ContextPanel (`features/context/context-panel.ts`)

```
Selector: app-context-panel
Inputs:  (reads TraceStore + SessionStore directly — no inputs needed)
Outputs: —

─────── Behavior ──────────────────────────────────────────────────────
- Right panel. Content driven entirely by what is selected.
- No selection → shows search prompt: "Select an entry, node, or insight to inspect."
- Sections (collapsible, twisty headers):

  1. Details (always visible when selection exists)
     Shows based on selection type:
     * Entry selected: HTTP method · Route · Auth badge · File location (via getNode RPC)
     * Node selected: Kind · File path · Tags (chips) · In/Out degree
     * Insight selected: Severity badge · Title · Detail text · Evidence

  2. Call Stack (shown for entries/nodes, depth=3 initially)
     ProgressiveTraceTree rendered at compact depth.
     If selection is a trace node, highlights that node in the tree.
     Click a node in this tree → selectNode → entire Context Panel updates for that node.

  3. Metrics — "Nerd Mode" (shown for nodes, collapsible)
     LOC, CC, Fan-in, Fan-out, Instability.
     Data source: placeholder using deterministic hash of nodeId (see §7).
     Prefixed with ~ and italic to signal "estimated."
     When engine adds these fields to NodeResponse proto, remove prefix.

  4. Insights (shown when selection has related insights)
     List of insights that reference this entry/node.
     Click insight → Context Panel switches to insight detail view.
     "Trace source" button for each insight.

  5. LLM Context (always visible when trace is active)
     Auto-rendered markdown from Render RPC (debounced 250ms, same as section-lens today).
     Copy button (Ctrl+C works even when panel collapsed to token strip).
     Token count badge.
     Future: "Explain" / "Find risks" / "Suggest tests" quick-action buttons (placeholders, disabled).
     Collapsed mode: shows only token count + Copy button (28px strip).

- Smart Dock sizing: width controlled by dock level (0-3). Persisted in PrefsStore.
- Ctrl+Shift+L toggles between last non-zero dock level and level 0.
────────────────────────────────────────────────────────────────────────
```

### 4.5 QuickPeek (`features/context/quick-peek.ts`)

```
Selector: app-quick-peek
Inputs:  (none — controlled by a service/state: QuickPeekState)
Outputs: —

─────── Behavior ──────────────────────────────────────────────────────
- Floating overlay positioned near mouse cursor.
- Trigger: hover any linkable element (node, entry, package) for 200ms.
- Shows: Title · Kind · File location · Mini metadata (LOC, CC — from placeholder).
- Disappears on mouse-leave (no click needed).
- If Ctrl is held during hover, QuickPeek stays pinned until Ctrl is released or clicked.
- If user clicks while QuickPeek is visible, the selection moves (QuickPeek transitions
  into Context Panel — no duplicate fetch).
────────────────────────────────────────────────────────────────────────
```

### 4.6 Breadcrumb (`shell/breadcrumb.ts`)

```
Selector: app-breadcrumb
Inputs:  path: Crumb[]  (reads from WorkspaceStore.activeTab().historyStack)
Outputs: crumbClick: Crumb

─────── Behavior ──────────────────────────────────────────────────────
- Horizontal bar, 22px height, beneath TabStrip.
- Only visible when there's history (at least one selection made).
- Format: `eShop › OrdersController › GET /api/orders › OrderService`
- Each segment: clickable, navigates to that point in history.
- Middle-click: opens that point in a new tab (duplicate tab with same repo, that focus).
- Overflow: leftmost segment always visible, trailing segments truncated with '…' dropdown.
- Breadcrumb updates on: entry selection, node selection, insight click, trace step.
────────────────────────────────────────────────────────────────────────
```

### 4.7 ExportDrawer (`features/export/export-drawer.ts`)

```
Selector: app-export-drawer
Inputs:  open: boolean
Outputs: dismissed: void

─────── Behavior ──────────────────────────────────────────────────────
- Slides from right side of screen, 400px wide. Overlays content (doesn't push layout).
- Backdrop: backdrop-blur, click-to-close.
- Content: same section toggle sidebar + markdown preview + Copy + token count as current
  section-export.ts, but in a drawer format (not a modal).
- Section presets: 3 quick-select buttons:
  * "Onboarding" — Identity, Architecture, Entries
  * "Trace" — current trace with depth 2, Call Stack
  * "Review" — all sections, full detail
  Clicking a preset updates sectionData toggles and re-renders.
- Fixes GAP-S4 (pack presets).
- Ctrl+E opens drawer from anywhere. Ctrl+E or Esc or click-outside closes.
────────────────────────────────────────────────────────────────────────
```

### 4.8 TitleBar (`shell/titlebar/titlebar.ts`) — rename from AppHeader

```
Selector: app-titlebar
Changes from current app-header:
- Height: 2.75rem → 30px
- Background: solid bg-base (remove bg-base/80, backdrop-blur, shadow-sm)
- Logo: keep, but smaller (font-sm → font-xs mono). Click → navigateHome.
- Center: "Search Everything" input (new). 300px wide, borderless, placeholder text.
  Ctrl+P focuses it. Searches entries + nodes + packages + actions.
  This replaces the dead <ng-content select="[analyze]"/> (already removed in P0.1).
- Right: repo dropdown (existing), server dot (existing), window controls (existing).
  "New" button removed — access via dropdown "New analysis…" or Ctrl+Shift+N.
- Window controls: visible only when isTauri() (existing). Same icons.
- Analyze button: always visible, triggers session.analyze() on current repo path
  (re-analyze, Flow C). If no session, navigates to landing.
────────────────────────────────────────────────────────────────────────
```

### 4.9 ActivityBar (`shell/activity-bar/activity-bar.ts`) — rename from NavigationRail

```
Selector: app-activity-bar
Changes from current navigation-rail:
- Items reduced from 7 to 6 (see §1 table).
- Icons only (was icon + label). Tooltip on hover shows name + shortcut.
- Width: 48px (was w-14 = 56px).
- Active indicator: 3px left border accent (was text-accent color change).
- Count badges: positioned absolute top-right over icon. 12px circle.
  Badge for Explore: session.entryCount()
  Badge for Insights: session.insightCount()
  Both hidden when count is 0 or not ready.
- Disabled items: always visible, opacity-40, pointer-events-none, title="Analyze a repo first".
  (was: filtered out entirely). Fixes GAP-S5/S6.
- Icon names changed: home→map, list→layers, lightbulb→info. All exist in REGISTRY (fixes GAP-S7).
```

---

## 5. Data Flow — The Complete Loop

```
User presses j in EntryListbox
  │
  ├─ EntryListbox.selectedIndex += 1
  ├─ Output: selectionChange → ExplorerPage.onEntryChange(entry)
  │    │
  │    ├─ TraceStore.trace(handle, entry.focus)   [debounced 150ms]
  │    │   └─ TraceStore.loading = true
  │    │   └─ RPC: getTrace → TraceStore.tree updates
  │    │   └─ TraceStore.loading = false
  │    │
  │    ├─ (if tree changed) ContextPanel updates:
  │    │   └─ Details: from entry metadata (SessionStore.entryGroups)
  │    │   └─ Call Stack: ProgressiveTraceTree at depth 2
  │    │   └─ LLM: debounced Render RPC (250ms) → markdown
  │    │   └─ Metrics: placeholder nerdMetrics(entry.nodeId)
  │    │   └─ Insights: filter SessionStore.stats()?.insights
  │    │
  │    ├─ WorkspaceStore.pushHistory(tabId, {focus, stageMode, expandedNodes})
  │    ├─ Breadcrumb reads historyStack → updates path
  │    └─ URL: replaceUrl('/explore?focus=entry.focus')
  │
  └─ (nothing else moves)

User clicks node in ProgressiveTraceTree or Graph
  │
  ├─ TraceStore.selectNode(nodeId)
  │   ├─ RPCs: getNode + getNeighbors (parallel)
  │   └─ TraceStore.nodeDetail updates
  │   └─ TraceStore.neighbors updates
  │
  ├─ ContextPanel updates:
  │   └─ Details: from nodeDetail (kind, file, tags, degree)
  │   └─ Call Stack: stays (shows trace tree from current focus)
  │   └─ Metrics: nerdMetrics(nodeId)
  │   └─ LLM: unchanged (stays rendered for the trace focus)
  │
  ├─ WorkspaceStore.pushHistory(tabId, entry)
  └─ Breadcrumb extends: appends node title

User presses Ctrl+Z (undo)
  │
  ├─ WorkspaceStore.historyStack.pop(tabId)
  ├─ Restore previous entry: {focus, stageMode, expandedNodes}
  ├─ TraceStore.trace(handle, focus)   [sync, no debounce — we know the previous state]
  ├─ Wait for tree to load
  ├─ Restore expanded nodes: walk tree, expand matching nodes
  ├─ ContextPanel updates to match restored state
  ├─ Breadcrumb pops last segment
  └─ URL updates

User presses Esc
  │
  ├─ (layer 1) If Export drawer open → close
  ├─ (layer 2) If Quick Peek pinned → unpin
  ├─ (layer 3) If Context Panel has node selected (not trace root) → deselect node,
  │            Context Panel returns to entry-level view
  ├─ (layer 4) If trace active → clear focus, stage returns to empty/entry-picker
  ├─ (layer 5) If entries overlay (Shift+E) open → collapse to listbox
  └─ (layer 6) Nothing left to unwind
```

---

## 6. Keyboard Map — Full

### Global (always works)

| Key | Action | Handler |
|-----|--------|---------|
| `Ctrl+P` | Focus "Search Everything" in titlebar | TitleBar |
| `Ctrl+K` | Command palette | Palette |
| `Ctrl+Shift+N` | New blank tab | TabStrip |
| `Ctrl+T` | New blank tab (same as above) | TabStrip |
| `Ctrl+W` | Close active tab | TabStrip |
| `Ctrl+Tab` | MRU next tab | TabStrip |
| `Ctrl+Shift+Tab` | MRU previous tab | TabStrip |
| `Ctrl+1-6` | Jump to tab N | TabStrip |
| `Ctrl+Shift+L` | Toggle Context Panel | WorkspaceShell |
| `Ctrl+1-4` | Smart Dock resize (context: 0/30/40/100%) | WorkspaceShell |
| `Ctrl+E` | Open/close Export drawer | WorkspaceShell |
| `Ctrl+R` | Re-analyze current repo, restore focus | WorkspaceShell |
| `Ctrl+Z` / `Ctrl+Y` | Undo / Redo exploration | WorkspaceShell |
| `g` + letter | Navigate to view (g o/e/a/i/x/s) | WorkspaceShell |
| `?` | Help overlay | WorkspaceShell |
| `Esc` | Esc ladder (close drawer → deselect → clear → collapse) | WorkspaceShell |

### Explorer (when Explorer view has focus)

| Key | Action | Handler |
|-----|--------|---------|
| `j` / `k` | Move entry highlight up/down | EntryListbox |
| `/` | Focus entry filter | EntryListbox |
| `Enter` | Pin/unpin current selection | EntryListbox |
| `Shift+E` | Expand/collapse entries overlay (full table) | EntryListbox |
| `v t` / `v g` | Stage mode: trace tree / graph | Stage |
| `→` | Expand trace tree node (on focused node) | ProgressiveTraceTree |
| `←` | Collapse trace tree node | ProgressiveTraceTree |
| `Space` | Expand graph from selected node (full size) | Stage |
| `F` | Focus only selected node's neighborhood in graph | Stage |
| `.` | Re-center current view (scroll tree to root, fit graph) | Stage |
| `Ctrl+Z` (graph active) | Collapse previous graph expansion | Stage |
| `Alt+←` / `Alt+→` | Exploration history back / forward | WorkspaceShell |
| `Ctrl+C` (trace active) | Copy LLM context to clipboard | ContextPanel |

### Palette (when open)

| Key | Action |
|-----|--------|
| `↑` / `↓` | Move selection up/down |
| `Tab` | Cycle verb (Trace · Node · Usages · Copy) |
| `Enter` | Execute selected item |
| `Esc` | Close palette |

---

## 7. Placeholder Data — Nerd Metrics

File: `features/context/nerd-metrics.ts`

```typescript
export interface NerdMetrics {
  readonly loc: number;
  readonly cc: number; // cyclomatic complexity
  readonly fanIn: number;
  readonly fanOut: number;
  readonly instability: number; // fanOut / (fanIn + fanOut)
  readonly lastTouched: Date;
}

// Deterministic hash — same nodeId always produces the same placeholder values.
// Prefixed with ~ to signal "estimated" until engine provides real data.
function hashCode(s: string): number {
  let h = 0;
  for (let i = 0; i < s.length; i++) {
    h = ((h << 5) - h + s.charCodeAt(i)) | 0;
  }
  return h;
}

export function nerdMetrics(nodeId: string): NerdMetrics {
  const h = Math.abs(hashCode(nodeId));
  const fanIn = h % 20;
  const fanOut = (h >> 8) % 25;
  return {
    loc: 20 + (h % 480),
    cc: 1 + ((h >> 8) % 30),
    fanIn,
    fanOut,
    instability: fanIn + fanOut === 0 ? 0 : Math.round((fanOut / (fanIn + fanOut)) * 100) / 100,
    lastTouched: new Date(Date.now() - (h % 30) * 86400000),
  };
}
```

Display format in Context Panel:
```
~142 LOC · CC ~12
Fan-in ~9 · Fan-out ~18
Instability ~0.67
Last touched ~12d ago
```

All values prefixed with `~` and rendered in italic `text-ink-muted`. Tooltip on hover: "Engine doesn't provide these metrics yet — values are hash-based estimates."

When `NodeResponse` proto gains these fields, `toNodeDetailVm()` will extract them, and the `~` prefix + tooltip disappears automatically (check `nodeDetail.loc != null` to switch to real data).

---

## 8. History Stack — Data Structure

In `WorkspaceStore`, add to `TabState`:

```typescript
export interface HistoryEntry {
  readonly focus: string;           // TraceStore.focus at time of entry
  readonly stageMode: 'trace' | 'graph';
  readonly expandedNodes: readonly string[];  // flat list of expanded node IDs in ProgressiveTraceTree
  readonly breadcrumb: readonly string[];    // human-readable path segments for Breadcrumb display
  readonly timestamp: number;
}

// Per-tab: readonly historyStack: HistoryEntry[]
// Per-tab: historyIndex (cursor for undo/redo), -1 = at tip
```

Methods:
- `pushHistory(tabId, entry)` — adds to stack, removes forward entries if not at tip, caps at 50 (shift from front)
- `undoHistory(tabId)` — decrements cursor, returns previous entry (null if at start)
- `redoHistory(tabId)` — increments cursor, returns next entry (null if at tip)
- `clearHistory(tabId)` — resets stack and cursor

On tab switch, no history is lost — each tab maintains its own stack.

---

## 9. Visual Language — The IDE Rules

### What changes in `styles.css`

```css
/* Remove */
  No .shadow-* classes on panels (shadow-sm, shadow-lg, shadow-xl — all removed)
  No .rounded-* on structural elements (rounded-md, rounded-lg — removed from panels)
  No .backdrop-blur-* on structural elements (was in header)
  No #custom-scrollbar or scrollbar-width: thin (use native OS scrollbars)

/* Add */
  Divide lines: .divide-y .divide-panel (new color token: panel-border, a 1px semi-transparent line)
  Panel background: .bg-panel (slightly lighter than bg-base for content panels)
  Focus rings: .focus-ring (1px accent outline, 2px offset — minimal, not browser default)
  Selection highlight: .selected (2px left border accent, subtle bg shift — no bg-fill)
  Ghost button: .btn-ghost (no background, text color shift on hover — already in Button component)
  Tight spacing: py-1 (4px) instead of py-2 (8px), px-2 (8px) instead of px-3 (12px)
  Text density: text-xs for body, text-2xs for labels, text-sm for headers only
```

### Panel decoration pattern

Each panel (ActivityBar, Explorer columns, Context Panel) uses:
```
border-right: 1px solid var(--color-panel-border)   OR
border-left: 1px solid var(--color-panel-border)
```

No card borders. No shadows. No rounded corners on panels. The 1px divider is the only visual separation between zones.

### Element patterns

| Element | Style |
|---------|-------|
| Button | Ghost by default (no bg, text-ink-muted → hover:text-ink). Accent only for primary actions. |
| Input | Borderless (no bg, underline or focus border only). `bg-transparent border-b border-panel focus:border-accent`. |
| List item (entry, node) | No bg. Hover: 2px left border accent. Selected: left border stays, subtle bg shift (bg-panel). |
| Section header | text-2xs, no uppercase, font-medium, text-ink-muted. Collapse twisty inline. |
| Badge/chip | 0 radius. Thin border. Text-2xs. Compact padding. |
| Divider | 1px horizontal line, color panel-border. No margin (content spacing via py). |
| Scrollbar | OS native. No custom styling. |
| Focus ring | 1px accent outline, offset 2px. Visible on Tab, not on click (use `:focus-visible`). |
| Selection | Subtle. Left border accent. No full background fill. |

---

## 10. Implementation Phases (resumable)

### Phase 1 — Foundation: Wire Tabs + Restyle Chrome

**Entry check:** `pnpm check` green

**Tasks:**
1.1 Import `TabStrip` in `workspace-shell.ts` → add `<app-tab-strip />` between header and content
1.2 Adjust height calc: `calc(100vh - 30px - 32px - 22px)` (titlebar + tabstrip + statusbar)
1.3 Add duplicate-path guard in `SessionStore.analyze()` (GAP-T4)
1.4 Add `mruStack` and Ctrl+Tab/Ctrl+Shift+Tab handlers in TabStrip (GAP-T5)
1.5 Fix `shortLabel()` → use last path segment (GAP-T3)
1.6 Rename `app-header` → `app-titlebar`: 30px height, solid bg, remove shadow/blur, move files
1.7 Rename `app-footer` → `app-statusbar`: 22px height, vertical dividers
1.8 Rename `app-navigation-rail` → `app-activity-bar`: 5 items, icons only, 48px width
1.9 Add count badges to ActivityBar (GAP-S6)
1.10 Add disabled-item visibility + tooltip (GAP-S5)
1.11 Fix icon names: use existing REGISTRY icons (map, layers, boxes, info) — GAP-S7
1.12 Add explicit root route `/` redirect in app.config.ts (GAP-N1)

**Gate:** Open app → see tab strip with one tab. Ctrl+T creates new tab. Ctrl+W closes. ActivityBar shows 5 icons + Settings. Badge on Explore shows entry count. Disabled items are visible (opacity-40) with tooltip.

### Phase 2 — Explorer Unification

**Entry check:** Phase 1 gate passes

**Tasks:**
2.1 Create `EntryListbox` component — flat list, j/k navigation, debounced trace, kind chips, search
2.2 Create `ProgressiveTraceTree` component — twisties, depth-based collapse, seam badges inline
2.3 Create `SeamFilterChips` component — toggle chips for call/send/handler/raises/consumes
2.4 Create `Stage` component — hosts tree + graph, mode toggle, depth/detail controls
2.5 Create `MiniGraph` component — thumbnail canvas, click → expand
2.6 Create `ExplorePage` (`features/pages/explore-page.ts`) — 3-column layout (listbox | stage | placeholder)
2.7 Add `/explore` route to app.config.ts
2.8 Redirect `/entries`, `/trace`, `/graph` → `/explore`
2.9 Remove `section-entries.ts`, `section-trace.ts`, `section-graph.ts`, `section-lens.ts`
2.10 Update Palette "Go to" entries: Explore replaces three old routes (part of B1-B5 palette fixes)

**Gate:** Navigate to /explore → see entry listbox. j/k moves selection. Trace tree renders in stage. Stage toggles between trace and graph. Context Panel shows placeholder text. Old routes redirect correctly.

### Phase 3 — Context Panel

**Entry check:** Phase 2 gate passes

**Tasks:**
3.1 Create `ContextPanel` component — collapsible sections (Details, Call Stack, Metrics, Insights, LLM)
3.2 Wire Details section — switches content based on selection type (entry vs node vs insight)
3.3 Wire Call Stack section — ProgressiveTraceTree at compact depth, highlights selected node
3.4 Wire Metrics section — `nerdMetrics(nodeId)` placeholder
3.5 Wire Insights section — filter stats.insights by current selection
3.6 Wire LLM section — migrate existing SectionLens render/debounce/Ctrl+C logic
3.7 Smart Dock: resize handles, Ctrl+1-4 presets, PrefsStore persistence
3.8 Ctrl+Shift+L toggle (last non-zero level ↔ level 0)
3.9 Collapsed mode: token-count strip (28px wide) with Copy button only

**Gate:** Select an entry → Context Panel shows Details + Call Stack (2 levels) + LLM (rendered after 250ms). Switch to a graph node → Metrics section shows ~LOC, ~CC. Collapse panel with Ctrl+Shift+L → shows thin strip with token count + Copy. Ctrl+C from collapsed mode copies LLM content.

### Phase 4 — History, Breadcrumb, Undo

**Entry check:** Phase 3 gate passes

**Tasks:**
4.1 Add `HistoryEntry` type + `historyStack` + `historyIndex` to WorkspaceStore
4.2 Add `pushHistory(tabId, entry)`, `undoHistory()`, `redoHistory()` methods
4.3 Wire pushHistory into: every selection change, every trace, every node click
4.4 Create `Breadcrumb` component — reads historyStack, renders path
4.5 Place Breadcrumb in workspace-shell below TabStrip
4.6 Wire Ctrl+Z / Ctrl+Y → undo/redo history
4.7 Wire Alt+← / Alt+→ → same undo/redo (alias)
4.8 Implement Esc ladder: close popover → deselect node → clear focus → collapse entries

**Gate:** Trace an entry, click a node, click another node. Breadcrumb shows: `eShop › Entry › Node1 › Node2`. Press Ctrl+Z → breadcrumb pops, selection goes back to Node1, Context Panel shows Node1. Ctrl+Y redoes. Esc clears everything in layers.

### Phase 5 — Quick Peek, Export Drawer, Polish

**Entry check:** Phase 4 gate passes

**Tasks:**
5.1 Create `QuickPeek` component — floating overlay, 200ms hover delay, mouse-track position
5.2 Wire QuickPeek into every `NodeLink` usage (trace tree, entry list, context panel)
5.3 Create `ExportDrawer` component — slide from right, section toggles, presets, copy
5.4 Add 3 pack presets (Onboarding/Trace/Review) — fixes GAP-S4
5.5 Wire Ctrl+E → open ExportDrawer from anywhere
5.6 Remove old `features/pages/export-page.ts` route → drawer replaces it
5.7 Restyle remaining views without SectionCard: Overview, Architecture, Insights
5.8 Add skeleton loading to NodeCard (GAP-B8)
5.9 Fix Palette debounce (GAP-B1), Tab cycling (GAP-B2), recents (GAP-B3), icons (GAP-B4), text (GAP-B5)
5.10 Extract duplicated helpers from section-stats + section-console → shared file (GAP-C1)
5.11 Add section graph/chart page empty states (GAP-B6, GAP-B7 — Architecture page already has empty state from audit; Export drawer gets its own empty state)
5.12 Update `?` help overlay with ALL keyboard shortcuts (GAP-T2)
5.13 Ctrl+R re-analyze with focus restore

**Gate:** Hover any node/entry → Quick Peek shows metadata near cursor. Ctrl+E opens export drawer with 3 preset buttons. Ctrl+R re-analyzes and restores last focus. Palette has debounced search + Tab cycling + recents. `?` overlay shows full keyboard map. All gap tracker items are resolved.

---

## 11. Acceptance Criteria — Final Gate

```
┌──────────────────────────────┬────────────────────────────────────┐
│ Gate                         │ Expected                           │
├──────────────────────────────┼────────────────────────────────────┤
│ pnpm check                   │ lint 0/0 · test 7/7 · build 0w/0e │
├──────────────────────────────┼────────────────────────────────────┤
│ Tab strip visible            │ Wired. Ctrl+T/W/1-6 work.          │
│ ActivityBar 5 items + badges │ Icons only. Counts on Explore/     │
│                              │ Insights. Disabled with tooltip.   │
├──────────────────────────────┼────────────────────────────────────┤
│ Explorer: j/k scrub          │ Arrow keys move highlight. Trace   │
│                              │ RPC fires on change (debounced).   │
│                              │ Context Panel updates live.        │
├──────────────────────────────┼────────────────────────────────────┤
│ Stage: trace ↔ graph toggle  │ v t / v g switch mode. Graph seeds │
│                              │ from current selection.            │
├──────────────────────────────┼────────────────────────────────────┤
│ Context Panel: all sections  │ Details, Call Stack, Metrics,      │
│                              │ Insights, LLM. Updates on any      │
│                              │ selection change.                  │
├──────────────────────────────┼────────────────────────────────────┤
│ Smart Dock                   │ Ctrl+1-4 resize. Persisted.        │
│                              │ Ctrl+Shift+L toggle.               │
├──────────────────────────────┼────────────────────────────────────┤
│ Breadcrumb                   │ Shows exploration path. Click      │
│                              │ jumps to point. Adjust on undo.    │
├──────────────────────────────┼────────────────────────────────────┤
│ Undo/Redo                    │ Ctrl+Z/Y walks history stack.      │
├──────────────────────────────┼────────────────────────────────────┤
│ Esc ladder                   │ Closes in layers: drawer → node    │
│                              │ deselect → clear focus → collapse. │
├──────────────────────────────┼────────────────────────────────────┤
│ Quick Peek                   │ Hover 200ms → metadata overlay.    │
│                              │ Move away → gone.                  │
├──────────────────────────────┼────────────────────────────────────┤
│ Export drawer                │ Ctrl+E → slide-out. 3 presets.     │
│                              │ Copy works.                        │
├──────────────────────────────┼────────────────────────────────────┤
│ Re-analyze with restore      │ Ctrl+R → re-analyze, restore       │
│                              │ previous focus by route match.     │
├──────────────────────────────┼────────────────────────────────────┤
│ No web-page feel             │ Zero card shadows. No rounded      │
│                              │ panel corners. 1px dividers only.  │
│                              │ Native scrollbars. Borderless       │
│                              │ inputs. Ghost buttons.              │
├──────────────────────────────┼────────────────────────────────────┤
│ All 23 GAPs resolved         │ GAP-T1-T5, N1, S4-S7, B1-B8,       │
│                              │ C1-C2 all addressed. S1-S3          │
│                              │ remain BLOCKED (engine-dependent). │
└──────────────────────────────┴────────────────────────────────────┘
```

---

## 12. Verification Commands (per phase)

```powershell
# After every phase:
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check      # lint + test + build — MUST be green

# Manual smoke (after Phase 2+):
pnpm server     # terminal 1
pnpm dev:web    # terminal 2 → http://localhost:4200

# Walk through every step in the acceptance criteria table.
# Use real exit codes — never pipe through tail.
```

---

## Appendix A: Files Created/Modified/Deleted

### Created
- `features/explorer/entry-listbox.ts`
- `features/explorer/progressive-trace-tree.ts`
- `features/explorer/mini-graph.ts`
- `features/explorer/seam-filter-chips.ts`
- `features/explorer/stage.ts`
- `features/context/context-panel.ts`
- `features/context/quick-peek.ts`
- `features/context/nerd-metrics.ts`
- `shell/breadcrumb.ts`
- `features/export/export-drawer.ts`
- `features/pages/explore-page.ts`
- `features/pages/architecture-page.ts`
- `shell/titlebar/titlebar.ts`
- `shell/activity-bar/activity-bar.ts`
- `shell/statusbar/statusbar.ts`

### Modified
- `shell/workspace-shell.ts` — major: new layout, smart dock, help overlay, keyboard map
- `shell/tab-strip.ts` — wire + MRU + shortLabel fix
- `features/palette/palette.ts` — B1-B5 fixes + route name updates
- `features/settings/settings-view.ts` — dock level preference
- `features/node-card/node-card.ts` — skeleton loading
- `features/narrative/section-identity.ts` — restyle, no card
- `features/narrative/section-architecture.ts` — promoted to page, restyle
- `features/narrative/section-console.ts` — restyle, no card
- `features/narrative/section-stats.ts` — restyle, no card
- `features/insights/insights-view.ts` — restyle, no cards
- `features/narrative/section-landing.ts` — restyle, no SectionCard
- `features/pages/overview-page.ts` — restyle sections
- `state/workspace.store.ts` — historyStack, mruStack, lastAnalyzedPath
- `state/session.store.ts` — duplicate-path guard, focus restore
- `state/prefs.store.ts` — dockLevel, lensAutoShow
- `app.config.ts` — new routes + redirects

### Deleted
- `features/narrative/section-card/` (directory)
- `features/narrative/section-entries.ts`
- `features/narrative/section-trace.ts`
- `features/narrative/section-graph.ts`
- `features/narrative/section-lens.ts`
- `features/narrative/section-export.ts`
- `features/pages/entries-page.ts`
- `features/pages/trace-page.ts`
- `features/pages/graph-page.ts`
- `features/pages/export-page.ts`
- `shell/header/app-header.ts` → renamed to titlebar/titlebar.ts
- `shell/navigation-rail.ts` → renamed to activity-bar/activity-bar.ts
- `shell/footer/app-footer.ts` → renamed to statusbar/statusbar.ts
- `shell/scroll-spy/` (already deleted in P0.3)
- `features/narrative/narrative-canvas.ts` (already deleted in P0.3)

---

## Appendix B: Gap Tracker Cross-Reference

| Gap | Phase | Action |
|-----|-------|--------|
| T1 — Tab strip not wired | Phase 1 | Wire into workspace-shell |
| T2 — Tab shortcuts in help | Phase 5 | Update help overlay |
| T3 — shortLabel() bug | Phase 1 | Use last path segment |
| T4 — No duplicate-path guard | Phase 1 | SessionStore.analyze() check |
| T5 — No Ctrl+Tab MRU | Phase 1 | mruStack + handler |
| N1 — No `/` route | Phase 1 | Add explicit route |
| S1 — Auth column | Deferred | Blocked on engine F1 |
| S2 — file:line column | Deferred | Blocked on engine proto |
| S3 — Storage file ops | Deferred | Needs Tauri plugin |
| S4 — Export pack presets | Phase 5 | 3 preset buttons in drawer |
| S5 — Disabled rail tooltips | Phase 1 | ActivityBar shows disabled items |
| S6 — Nav count badges | Phase 1 | Badges on Explore/Insights |
| S7 — Missing icon registry | Phase 1 | Use existing icons (map/layers/boxes/info) |
| B1 — Palette no debounce | Phase 5 | debounceTime(150) |
| B2 — Palette no Tab verbs | Phase 5 | Tab cycling handler |
| B3 — Palette no Recents | Phase 5 | Add RecentStore section |
| B4 — Palette no kind icons | Phase 5 | KIND_ICONS on node results |
| B5 — Palette wrong no-results | Phase 5 | Include query in text |
| B6 — Graph empty state | Phase 5 | Stage handles "no session" |
| B7 — Export empty state | Phase 5 | Drawer handles all states |
| B8 — NodeCard skeleton | Phase 5 | Skeleton pulse blocks |
| C1 — Duplicate helpers | Phase 5 | Extract to shared file |
| C2 — Palette perf | Phase 5 | Separate static/search sections |
