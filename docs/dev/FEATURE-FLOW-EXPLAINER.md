# DevContext Desktop — Feature & Flow Explainer

> For LLM review. Describes the app's identity, current features, technology wiring, user flows,
> and proposed UX improvements. Written so any LLM can understand and recommend meaningful changes.

---

## 1. App Identity — What Is This?

DevContext is a **desktop devtool for .NET repositories**. It analyzes a .NET solution and produces:

| Output | What it is | Where shown |
|--------|-----------|-------------|
| **Map** | Full architecture map — projects, packages, seams, topology | Overview page, Architecture section |
| **Entries** | All entry points into the app (HTTP endpoints, message consumers, hosted services) | Entries page (sortable filterable table) |
| **Trace** | Call-chain tree from any entry point, annotated with seam kinds (call/send/handle) | Trace page (interactive tree + focus search) |
| **Lens** | Side-by-side Human (trace tree + node detail) and LLM (auto-rendered markdown) | Trace page (below trace section) |
| **Graph** | Cytoscape call-graph from the current trace root | Graph page (interactive canvas) |
| **Insights** | Severity-ranked findings (warnings/notables/info) from engine analysis | Insights page |
| **Export** | Full LLM-context document with section toggles, copy, token count | Export page (modal overlay) |
| **Console** | Live streaming boot-log during analysis → collapses into RunReport on completion | Overview page |

The app's tagline: **"The devtool lens for any .NET repository. Instant architecture understanding."**

It's a nerdy, keyboard-first, signal-based Angular desktop app — dark-first, Tailwind CSS v4, lucide icons, zoneless change detection. No jQuery. No Bootstrap. No Material. House style: `text-xs` density, `text-2xs uppercase` headers, `max-w-4xl` content, mono mono everywhere.

---

## 2. Technology — How It's Wired

```
┌──────────────────────────────────────────────────────────────────────┐
│  BROWSER / TAURI WEBVIEW                                            │
│  Angular 22 (zoneless, signals, standalone components)              │
│                                                                      │
│  src/app/                                                            │
│    core/           gRPC-Web client (ConnectRPC via @bufbuild)       │
│    data-access/    DevContextApi — typed wrapper over gRPC client   │
│    state/          Signal stores:                                    │
│                      WorkspaceStore  — tabs, sessions, traces       │
│                      SessionStore    — facade over active tab       │
│                      TraceStore      — facade over active tab       │
│                      ConnectionStore — server ping + version        │
│                      NodeStore       — node card sheet state        │
│                      RecentStore     — localStorage recents         │
│                      PrefsStore      — localStorage prefs (new)     │
│    models/         View models + proto→vm mappers                   │
│    ui/             Dumb components: Icon, Button, Badge, Sheet,     │
│                      GraphCanvas (Cytoscape), SectionCard, etc.     │
│    features/       Smart components: pages, sections, node-card,    │
│                      palette, settings                              │
│    shell/          App layout: header, footer, nav rail              │
│                      (tab-strip built but not wired)                │
├──────────────────────────────────────────────────────────────────────┤
│  gRPC-Web ──── port 5179 ──── DevContext.Server (.NET)              │
│                                  │                                   │
│                                  └─── DevContext.Core engine         │
│                                       (analyzer, graph builder,     │
│                                        trace builder, extractors)    │
└──────────────────────────────────────────────────────────────────────┘
```

**Key architectural rules:**
- Analyze once, query many. `Analyze` returns a session handle. Map/Trace/Node/Neighbors are cheap render-time RPCs over the same snapshot.
- Store-per-domain: each domain has its own injectable signal store.
- Views are projections of stores — switching routes never re-fetches.
- `WorkspaceStore` holds 1-6 `TabState` objects, each with its own `session` + `trace` slices. `SessionStore` and `TraceStore` are computed facades reading from `workspace.activeTab()`.

---

## 3. Current User Flows

### Flow 1: First analysis
```
App opens → Landing page (/)
  → User types path (local dir, .sln, or github URL)
  → User clicks "Analyze" or picks from recents
  → Console shows live streaming boot-log (ProgressEvents)
  → Analysis completes → Console settles into RunReport (funnel, stages, timings)
  → Overview page shows Identity + Architecture + Stats sections
  → All rail items become active
```

### Flow 2: Exploring entries (current — 4 hops per entry)
```
Click "Entries" rail (or g+e)
  → Entries page: table with kind filter chips, search, sortable columns
  → Click a row → traces that entry, user is still on /entries
  → BUT lens is on /trace page — user must navigate to see it
  → Navigate to /trace (or g+t) → see trace tree + lens
  → Navigate back to /entries (or g+e) → repeat
  = 4 interactions per entry exploration
```

### Flow 3: Tracing deep
```
On /trace page:
  → Focus input: type to search entries, pick from dropdown
  → OR: come from entries page click (focus auto-populated)
  → Trace tree renders → collapsible tree with seam badges
  → Lens shows Human pane (tree + node detail) + LLM pane (markdown)
  → Ctrl+C copies LLM context to clipboard
  → Click trace node → selectNode RPC → lens shows node detail card
```

### Flow 4: Switching repos (current)
```
Header "New" button → closes current tab → navigates to landing
  → User types new path → analyzes
  = 3 steps, full re-navigation
```

### Flow 5: Graph exploration
```
On /graph page:
  → If a trace is active → canvas seeds from trace tree root
  → Depth slider (1-4), Expand/Shrink button
  → Click node → traces that node (re-populates trace + lens on /trace)
  → Currently must manually switch back to /trace to see results
```

---

## 4. Suggested UX Flows (For Review)

### Proposed Flow A: Multi-Tab Workspace (I10)

```
Header:     ◆ DevContext  [📁 eShop ▾]         New  ● online  ─□✕
Tab strip:  [◆ eShop ×] [TodoApi ×] [DntSite ×]                    [+]

Each tab = independent repo session. Switch = instant. Route + view state preserved per tab.

New interactions:
  Ctrl+T        → new blank tab at Landing page
  Ctrl+W        → close active tab
  Ctrl+1-6      → jump to tab
  Ctrl+Tab      → MRU cycle (last two tabs)       [not yet built]
  Click recents → starts analysis in NEW tab or switches to existing tab with same path
  "New" button  → closes current tab, lands at overview

Infrastructure: ALL built (workspace.store, session.store, trace.store, tab-strip.ts).
Only missing: <app-tab-strip> not placed in workspace-shell.ts template.

Expected output: 3 repos open simultaneously — switch instantly without losing view state.
Each tab's analysis runs independently. Footer shows per-tab progress.
```

### Proposed Flow B: Persistent Lens Panel

```
Shell has toggleable right panel (Ctrl+Shift+L):

┌─ Active View ──────────────────┬─ Lens (toggleable) ────────────────┐
│ /entries                       │ Human: trace tree or node detail   │
│ GET /api/orders  → ○ 📋       │ ├ call OrderRepo                    │
│ POST /api/order  → ○ 📋       │ └ call PaymentGateway               │
│ Click row → lens updates here  │────────────────────────────────────│
│ (no navigation!)               │ LLM: auto-rendered markdown        │
│                                │ [Copy] 2.3K tok                    │
└────────────────────────────────┴────────────────────────────────────┘

Benefits:
  - 4-click entry exploration → 1-click (75% reduction)
  - Lens works on ANY page: entries, graph, insights, overview
  - Keyboard arrow-down through entries → lens updates in real time
  - Toggleable when you want full-width view

Implementation: Move lens from trace-page to workspace-shell, right panel.
Gated on session.ready(). State from TraceStore (already global).
```

### Proposed Flow C: One-Click Re-Analyze From Anywhere

```
Header dropdown: [📁 eShop ▾]
  ┌───────────────────────────┐
  │ CURRENT                  │
  │ eShop                     │
  ├───────────────────────────┤
  │ RECENT                   │
  │ 📁 DntSite                │   ← click = analyze in new tab
  │ 📁 eShop                  │   ← or switch to existing tab
  ├───────────────────────────┤
  │ ▶ New analysis…          │   ← new tab at landing
  │ ↻ Re-analyze this repo   │   ← re-run current tab  [not yet built]
  └───────────────────────────┘

Footer: "eShop · 94 entries · 512 nodes    ● v1.2.3  terminal"
        ↑ click repo name = same dropdown         ↑ click = cycle vibe

Re-analyze entry: not yet in dropdown. Needs "Re-analyze this repo" item that
calls session.analyze() with same path + prefs defaults.
```

### Proposed Flow D: Graph Seeded From Context

```
Currently: /graph is blank until you trace something first.

Proposed: Graph auto-seeds from:
  - If trace store has active focus → seeds from trace root
  - If entries page has selected row (focus persisted in store) → seeds from that entry
  - If nothing selected → seeds from top-3 "interesting" entry points (from topology)
  - User can still manually select any entry to seed from

Implementation: In section-graph, add computed that reads traceStore.focus().
If no trace, read session.entryGroups and pick first HTTP endpoint as seed.
```

### Proposed Flow E: Quick Trace Popover on Hover

```
On Entries page, hovering a row shows a mini trace popover:

┌ Entries ────────────────────────────────────────────┐
│ GET /api/orders → OrderSvc → ○ 📋                   │
│ ┌ Popover ────────────────────────────────────┐     │
│ │ OrderController                             │     │
│ │ ├ call OrderRepo.GetById   [verified]       │     │
│ │ └ call PaymentGateway.Charge [approx]       │     │
│ │ [Pin] [Full trace → /trace?focus=...]       │     │
│ └─────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────┘

Implementation: 200ms hover delay → fetch trace at depth=2 → render inline.
Shift+click pins popover. "Full trace" navigates to /trace?focus=X.
Cache last 5 traces in a Map. Only if lens panel is closed.
```

---

## 5. Request for Review

The DevContext team should review and provide feedback on:

1. **Tab strip wiring** — is the tab strip placement correct (between header and content)?
   Are the keyboard shortcuts right? Should we auto-analyze restored tabs or stay lazy?

2. **Persistent lens** — is a toggleable right panel the right approach? Or should the lens
   remain page-specific? Should it auto-show when a trace is active, or stay hidden until toggled?

3. **Graph seeding** — should the graph be a passive canvas seeded from trace, or should it
   have its own exploration mode (BFS from any node)?

4. **Quick trace popover vs persistent lens** — which is more useful for rapid entry scanning?
   The popover is lower-cost (no layout change) but the lens is more powerful (full markdown render).

5. **Any missing flows?** The current app has no concept of:
   - Per-entry "notes" or annotations
   - Comparing two traces side-by-side
   - Exporting a graph image
   - Filtering the trace tree by seam kind
   - Searching within rendered LLM context
   - Persisting window size/position
   Are any of these priorities?

6. **Rail UX** — should disabled items stay visible (with tooltip) or stay hidden? Current
   approach: hidden. Spec says: visible but disabled.

---

## Appendix: Current Route Table

| Route | Page | Content | URL State |
|-------|------|---------|-----------|
| `/` / `/overview` | OverviewPage | Landing (no session), Console, Identity, Architecture, Stats | — |
| `/entries` | EntriesPage | Full entries table | `?sort=&dir=&kind=&q=` |
| `/trace` | TracePage | Trace tree + Synced Lens | `?focus=X` |
| `/graph` | GraphPage | Cytoscape call graph | — |
| `/insights` | InsightsPage | Insight cards + engine drawer | — |
| `/export` | ExportPage | Section toggle sidebar + markdown preview + copy | — |
| `/settings` | SettingsView | 5-tab settings (Appearance, Analysis, Storage, Server, About) | — |

All routes lazy-loaded. State preserved in stores (not URL — except entries/trace).

## Appendix: Store Inventory

| Store | What it holds | Persisted |
|-------|--------------|-----------|
| WorkspaceStore | `TabState[]` — up to 6 tabs, each with session + trace slices, route, controller | localStorage (paths + labels + routes only) |
| SessionStore | Facade — reads from `workspace.activeTab().session` | No |
| TraceStore | Facade — reads from `workspace.activeTab().trace` | No |
| ConnectionStore | `online`, `version`, `checked` — from periodic Ping RPC | No |
| NodeStore | `open`, `nodeId`, `node`, `neighbors`, `loading`, `error` — for sheet | No |
| RecentStore | `RecentRepo[]` — path + label + accessedAt (max 10) | localStorage |
| PrefsStore | `depth`, `detail`, `useRoslyn`, `autoCleanup` | localStorage (schema v1) |
