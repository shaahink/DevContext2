# DevContext Desktop — UI/UX Redesign Brief

> For an LLM to design a new shell, navigation, component structure, feature flow, and visual language.
> This doc describes WHAT exists and WHAT is planned. Not HOW to build it.

---

## 1. App Identity

DevContext is a desktop tool that analyzes .NET repos and produces structured understanding:
- **Map**: full architecture (projects, packages, seams, topology)
- **Entries**: all entry points (HTTP endpoints, message consumers, hosted services, jobs)
- **Trace**: call-chain tree from any entry point, annotated by seam kind (call/send/handler/raises)
- **Graph**: Cytoscape call-graph from trace tree root, interactive, node click = re-trace
- **Lens**: 50/50 split — Human pane (trace tree + node detail) + LLM pane (auto-rendered markdown from Render RPC)
- **Insights**: severity-ranked findings (warning/notable/info) from engine analysis
- **Export**: structured LLM context doc with per-section toggles, copy, token count
- **Console**: live streaming boot-log during analysis → settles into RunReport (funnel, stages, timings)

**Stack**: Angular 22 (zoneless, signals, standalone components), Tailwind CSS v4, lucide icons, gRPC-Web to .NET server over port 5179. Tauri v2 shell (native window, custom titlebar, `decorations: false`). Windows app with WebView2.

---

## 2. Server Data (What the Engine Provides)

### Analyze (streaming)
- Input: `{path, depth?, detail?, noRoslyn?, cleanup?}`
- Stream: `ProgressEvent{stage, percent, message}` → final `AnalyzeResult{handle, summary}` or error
- `AnalysisSummary`: `{label, projects, nodes, edges, entries, entriesWithTarget, elapsedMs, explanation, warnings[], isLibrary, archetype}`

### Map (stateless query)
- `MapResponse`: `{markdown, style, styleConfidence, archetype, isLibrary, projectCount, topology[], packages[], aggregates[], pipelineBehaviors[], stack[], scopeNote}`

### Entries (stateless query)
- `EntryPoint`: `{kind, title, nodeId, httpMethod?, route?, provenance?, project?, target?}`
- Note: NO `filePath` or `line` field. NO `auth` field.

### Trace (stateless query)
- Input: `{handle, focus, depth, detail}` → `TraceResponse: {found, root?, markdown, touchedEntities[], emittedEvents[]}`
- `TraceNode`: `{nodeId, title, kind, seam, depth, provenance, resolution, truncated, omitted, salient?, tags[], children[]}`

### Node (stateless query)
- `NodeResponse`: `{nodeId, title, kind, tags[], filePath, outDegree, inDegree, found}`
- Note: NO `loc`, `complexity`, `fanIn`, `fanOut`, `instability` fields.

### Neighbors (stateless query)
- `NeighborsResponse`: `{edges[]}` where `Edge: {from, to, kind, resolution, provenance?, otherTitle}`

### Render (stateless query)
- Input: `{handle, focus?, depth?, detail?, format?, sections[]}` → `RenderResponse: {content, estimatedTokens, sections[]}`

### Search (stateless query)
- `searchNodes(handle, query, limit)` → `SearchResponse: {nodes[]}`

### Stats (query, lazy-loaded after analysis)
- `StatsResponse`: `{totalWallMs, stages[], seams[], extractors[], cache?, corpus?, funnel?, insights[]}`
- `Insight`: `{title, severity, detail?, evidence[]}` — rendered in Insights view

### Other RPCs
- `Ping` → `{ready, version}`
- `CloseSession(handle)`

**Key constraint**: Analyze once, query many. All view RPCs (Map, Entries, Trace, Node, Neighbors, Render, Search, Stats) operate on the same immutable snapshot.

### Edge & Graph Structures

The engine builds an internal call-graph. The UI accesses it via two surfaces:

**Trace (tree projection)** — `TraceNode` is one root-to-leaves path through the graph, flattened to a tree. Seam = relationship kind between parent and this node. Each node has:
```
TraceNode { nodeId, title, kind, seam, depth, provenance?, resolution,
            truncated, omitted, salient?, tags[], pipeline[], children[] }
```
Seam values observed: `call`, `send`, `handler`, `raises`, `consumes`, `data`, `di`, `pipeline`, `resolve`.  
Resolution values: `"Syntactic"` (regex/pattern — approx) or `"Semantic"` (Roslyn — verified).

**Neighbors (local graph expansion)** — From any nodeId, get incoming and outgoing edges:
```
Edge { from, to, kind, resolution, provenance?, otherTitle }
```
GetNeighbors RPC takes `direction: 'out' | 'in' | 'usages'`. Edges include `otherTitle` — the human-readable name of the other side, so you don't need a second Node RPC to render edge labels.

**Node detail (single node)** — `NodeResponse { found, nodeId, title, kind, tags[], filePath, outDegree, inDegree }`. Note: NO `loc`, `complexity`, `fanIn`, `fanOut`, `instability` — these metrics are not in the proto.

**Map topology (project-level)** — `MapResponse.topology[]` is `ProjectNode { name, dependsOn[] }` — a separate project-dependency view of the graph, at project granularity (not method-level).

**GraphStat (from StatsResponse)** — `{ nodes, edges, entries, entriesWithTarget }` — aggregate counts only, not structural graph data.

**Key constraint**: There is no RPC to fetch "the whole graph." The graph exists only as projections: trace tree (depth-limited path from a focus), neighbors (one-hop expansion from a node), or topology (project-level). Cytoscape builds from flattened `TraceNode` tree up to `maxDepth=4`.

---

## 3. Frontend State (Signal Stores)

All injectable, all `providedIn: 'root'`. Views read signals — never call RPCs directly.

### WorkspaceStore — multi-tab container
```typescript
// Built but NOT wired into shell template
MAX_TABS = 6
tabs: TabState[]          // per-repo sessions
activeId: string | null
activeTab: computed       // derived from activeId
atCap: computed           // tabs.length >= 6

TabState { id, path, label, session: TabSessionSlice, trace: TabTraceSlice, route, controller }

TabSessionSlice {
  status: 'idle'|'cloning'|'analyzing'|'ready'|'error'
  error, handle, summary, mapResponse, mapMarkdown,
  entryGroups: EntryGroupVm[], stats: StatsResponse | null,
  statsError, statsLoading, progress: {stage, percent, message},
  consoleLog: LogLine[]
}

TabTraceSlice {
  focus: string | null, depth: 6, detail: 'salient',
  error, loading, found, tree: TraceNodeVm | null,
  markdown, touched[], emitted[],
  selectedNodeId, nodeDetail: NodeDetailVm | null, neighbors: EdgeVm[]
}
```

### SessionStore — facade over active tab's session slice
```typescript
status, error, handle, summary, mapResponse, mapMarkdown, entryGroups, stats, statsError, statsLoading, progress, consoleLog
busy, ready, entryCount, insights, insightCount
async analyze(spec: AnalyzeSpec)
cancel(), refreshStats()
```

### TraceStore — facade over active tab's trace slice
```typescript
focus, depth, detail, error, loading, found, tree, markdown, touched, emitted, selectedNodeId, nodeDetail, neighbors, active
async trace(handle, focus), setDepth(n), setDetail(d), clear(), selectNode(nodeId)
```

### Other stores
- **NodeStore**: `{open, nodeId, node, neighbors, loading, error}` — for node card sheet (overlay). Not per-tab.
- **ConnectionStore**: `{online, checked, version}` — polls Ping every 5s
- **RecentStore**: `{recents: RecentRepo[]}` — localStorage, last 10 repos
- **PrefsStore**: `{defaultDepth:6, defaultDetail:'salient', useRoslyn:true, autoCleanup:true}` — localStorage, schema-versioned

### View Models (models/view-models.ts)
```typescript
EntryVm { kind, title, nodeId, httpMethod?, route?, target?, provenance?, focus }
EntryGroupVm { kind, label, entries[] }
TraceNodeVm { id, title, kind, seam, depth, provenance?, resolution, truncated, omitted, salient?, tags[], children[] }
NodeDetailVm { id, title, kind, tags[], filePath?, outDegree, inDegree }
EdgeVm { from, to, kind, resolution, provenance?, otherTitle }

KIND_LABELS: { HttpEndpoint:'HTTP', MessageConsumer:'Bus consumers', HostedService:'Hosted services', ScheduledJob:'Scheduled jobs', DomainEventHandler:'Domain events', PublicApi:'Public API' }
KIND_ICONS: { HttpEndpoint:'webhook', MessageConsumer:'arrow-right', HostedService:'play', ScheduledJob:'refresh', DomainEventHandler:'dot', PublicApi:'network' }
```

---

## 4. Current Shell Structure

```
workspace-shell.ts
  ├─ app-header        (fixed, z-40, h-11, logo + repo dropdown + "New" + server dot + win controls)
  ├─ main area         (calc: 100vh - header - footer)
  │   ├─ navigation-rail  (w-14, left, icon+label, 7 items, requiresSession items hidden when not ready)
  │   └─ <router-outlet>
  ├─ app-footer        (fixed, z-40, h-7, session stats + vibe cycler)
  ├─ app-palette       (Ctrl+K overlay, entries + nodes + actions)
  └─ ? help overlay    (keyboard shortcuts)
```

**7 rail items**: Overview, Entries, Trace, Graph, Insights, Export, Settings. Icons `home`, `list`, `arrow-right`, `network`, `lightbulb`, `file-text`, `settings` — but `home`, `list`, `lightbulb` are NOT in the icon registry (render blank).

**8 routes**: `/` `/overview` `/entries` `/trace` `/graph` `/insights` `/export` `/settings` — all lazy-loaded. No explicit `path:''`.

**tab-strip.ts**: Complete multi-tab UI component (status dots, close ×, + button, Ctrl+T/W/1-6 shortcuts, auto-analyze on first activation) — **NOT wired** into workspace-shell.

### Key headers/footer details
- Header uses `bg-base/80 backdrop-blur-lg shadow-sm` (web-page feel)
- Header has `<ng-content select="[analyze]"/>` slot — nothing projects into it
- Header has repo-label dropdown (recent repos + "New analysis…")
- Header "New" button calls `closeTab() + navigateByUrl('/')`
- Header window controls use dynamic `import('@tauri-apps/api/window')` — hidden when `!window.__TAURI__`
- Footer shows: session stats (entries/nodes/edges/wired%), connection dot + version, theme vibe cycler, progress % during analysis
- Tauri config: `decorations: false`, min 900×600, resizable

---

## 5. Current Feature UI (one component per feature)

### SectionLanding (on /overview when no session)
- Path input (text) + Analyze button + Recents list + Advanced options (depth, detail, noRoslyn, cleanup)

### SectionConsole (on /overview when analyzing/ready)
- Mode `boot`: live scrolling log from ProgressEvents
- Mode `report`: stages chart, funnel bar, cache stats, extractors table

### SectionEntries (on /entries)
- Filter bar: SearchField + kind chips + "approx"/"has target" quick-filters + count badges
- Table: Method (badge) · Route (mono) · Target (mono+arrow) · Kind · hover actions (Trace/NodeCard/Copy)
- Sortable columns, sort persisted in URL (`?sort=col&dir=asc`)
- Keyboard: ↑↓ move, Enter trace, n node card, Ctrl+C copy
- Empty states: "Analyze a repo first" (no session) / "No entries match — clear filters" (filtered)

### SectionTrace (on /trace)
- Focus input with dropdown suggestions (debounced 150ms blur, populated from entry list)
- Depth + Detail selectors, Clear button
- Trace tree: recursive `app-trace-node` with seam-colored chips, [approx]/[verified] badges
- `traceStore.selectNode()` wired to node clicks in tree
- Deep-linkable: `/trace?focus=X`

### SectionGraph (on /graph)
- Requires an active trace. Seeds from `traceStore.tree()` root.
- Depth selector (1-4), Expand/Shrink toggle. Cytoscape canvas via `app-graph-canvas`.
- Node click → `selectNode()` + `trace()` — re-populates both
- If no trace active: "Select an entry and trace it to visualize the call graph." (no session guard — shows confusing text when no repo analyzed)

### SectionLens (on /trace, below SectionTrace)
- 50/50 split: Human (node detail card + trace tree) / LLM (markdown from Render RPC, debounced 250ms)
- Human pane shows `nodeDetail()` when a node is selected (card with kind, file, tags, in/out degree)
- LLM pane auto-refreshes on `focus` change. Copy button + Ctrl+C global shortcut.
- `selectNode()` wired to trace-node clicks

### SectionArchitecture (on /overview)
- Projects topology, Pipeline Behaviors, Aggregates, Packages, Seams (bar chart)
- All wrapped in SectionCard. Has empty states for no session / no data.

### SectionIdentity (on /overview)
- Archetype badge, Style + confidence, Scope note, 5 stat cells (nodes/edges/entries/wired/coverage%), Stack chips
- Wrapped in SectionCard. Has empty state for no session.

### SectionStats (on /overview)
- Pipeline stages (bar chart), Seams (grid), Extractors (table), Cache + Corpus + Funnel cards
- Loading skeleton, error state with retry. Wrapped in SectionCard.

### InsightsView (on /insights)
- Insight cards per severity (warning/notable/info) grouped by severity. Evidence chips. Detail text.
- Error state with retry, loading state, empty state with CTA. Semantic color tokens.

### NodeCard (overlay sheet, opens from entries row action or palette)
- Node detail: kind, filePath, tags, in/out degree, incoming edges ("Called by"), outgoing edges ("Calls")
- Trace + Copy ID buttons. Loading: spinner with "Loading…" (should be skeleton). Error: text + retry + copy details.

### SettingsView (on /settings)
- Left tabs: Appearance (vibes/themes) · Analysis (depth/detail/roslyn toggle/cleanup toggle, reads from PrefsStore) · Storage (static paths only — no Tauri file commands wired) · Server (status dot + port) · About (version, links, privacy)

### SectionExport (modal overlay)
- Opens from /export page or narrative-canvas button. Section toggle sidebar + markdown preview.
- Copy + token count. Re-render button. No pack presets (Onboarding/Trace/Review).

### Palette (Ctrl+K)
- Overlay, 560px, single input. Actions + View shortcuts + Entries (top 10, inline gRPC call, no debounce) + Nodes (from searchNodes, top 8, no kind icons, no debounce). No Recents. No Tab verb cycling. "No results" text doesn't include query.

---

## 6. Graph Capabilities (Cytoscape)

- `graph-canvas.ts` (272 lines): uses `cytoscape` + `cytoscape-dagre` for layout
- Input: `TraceNodeVm` tree root + `maxDepth` (1-4). Builds Cytoscape elements from tree flattened to depth.
- Seam colors mapped to node classes (entry/send/handle/raise/consume/data/resolve/pipeline/call)
- Theme-responsive: edge/background colors bind to current theme via `ThemeService.vibe()` effect
- Emits `nodeSelected(nodeId)` output on node click
- DAGRE layout, `elk` alternative not used
- Double-click: currently no handler. Graph is read-only projection.
- No MiniGraph, no neighborhood expansion, no fullscreen, no zoom-to-fit on new data

---

## 7. Known Issues (23 gaps)

### Unshipped (built, not wired)
1. **Tab strip not wired** — `tab-strip.ts` complete, never imported into workspace-shell
2. **Tab shortcuts missing from `?` help** — no Ctrl+T/W/1-6 in help overlay
3. **shortLabel() bug** — uses `split(' ')[0]`, not last path segment. Breaks on paths with spaces.
4. **No duplicate-path guard** — analyzing same path creates duplicate tab instead of switching
5. **No Ctrl+Tab MRU** — MRU tab cycling not implemented
6. **No explicit `/` route** in app.config.ts

### Missing Features (spec says must exist)
7. **Auth column** on entries (BLOCKED — proto has no auth fields)
8. **file:line column** on entries (BLOCKED — proto has no source location)
9. **Storage tab file operations** (BLOCKED — needs Tauri plugin @tauri-apps/plugin-fs)
10. **Export pack presets** (Onboarding/Trace/Review quick-select buttons)
11. **Nav rail disabled items hidden** — spec says visible+disabled with tooltip. Currently filtered out.
12. **Nav rail no count badges** — spec says badges on Entries/Insights
13. **Nav rail icon gaps** — `home`, `list`, `lightbulb` not in icon registry

### Bugs
14. **Palette no debounce** on node search (fires per keystroke ≥2 chars, spec says 150ms)
15. **Palette no Tab verb cycling** (Trace/Node/Usages/Copy)
16. **Palette no Recents section**
17. **Palette no kind icons** on node results
18. **Palette wrong "no results" text** (should include query)
19. **Graph empty state** wrong — "Select an entry and trace it" even when no repo analyzed
20. **Export page no loading/error states**
21. **NodeCard text loading** — spec says skeleton, not "Loading..."

### Code quality
22. **Duplicated helpers** — `ms()`, `num()`, `pct()`, `fmtK()`, etc duplicated between section-stats and section-console
23. **Palette rebuilds entire list** per keystroke — should separate static from search-dependent items

---

## 8. Current UX Problems (Flow-Level)

1. **4-click per entry exploration**: User clicks entry row (traces) → navigates to /trace (or g+t) → sees tree + lens → navigates back to /entries (or g+e). Trace and entries live on different pages.

2. **Graph is blank on arrival**: Graph seeds from `traceStore.tree()` which is null until user traces something. No auto-seed from entry list or topology.

3. **Lens is hidden**: Lens is only on /trace page. Not visible from /entries or /graph. Selection from entries table populates trace Store but user must switch pages to see the lens output.

4. **Page-thinking dominates**: 7 rail items, 8 routes. Each capability IS a route. The mental model is "I need to go to the Trace page" not "I have a selection and the trace/lens/graph are views of it."

5. **Visual web-page feel**: Cards with `rounded-md border shadow-sm` wrappers on every section. Header has `backdrop-blur`. Rounded corners everywhere. Heavy hover backgrounds. Not IDE-native.

6. **Tab strip invisible**: Full multi-tab infrastructure exists (WorkspaceStore, TabStrip, SessionStore/TraceStore facades) but tab strip is orphaned — zero multi-repo workflows possible.

7. **No undo/redo**: No history stack. User traces one thing, clicks a node, traces another — can't go back.

8. **No breadcrumb**: No visible path showing current exploration context.

---

## 9. Available Icons (lucide, via Icon component registry)

```
activity, arrow-right, arrow-up, boxes, check, chevron-right, circle, code,
copy, database, dot, download, file-text, folder-open, globe, info, laptop,
layers, loader, map, moon, network, palette, play, plug, refresh, search,
settings, square, sun, webhook, x, zap
```

Components use `<app-icon [name]="iconName" [size]="16" />`.

---

## 10. UI Component Library (existing, reusable)

- `SectionCard` — card wrapper with id, title, subtitle. Used everywhere. Has `rounded-md border shadow-sm`.
- `Icon` — lucide icons by name
- `Button` — variant (primary/secondary/ghost), size (sm/md)
- `Badge` — variant (accent/warn/success/default), rounded pill
- `Sheet` — slide-in overlay from right (used by NodeCard)
- `GraphCanvas` — Cytoscape wrapper
- `StatCell` — value + label cell
- `SearchField` — two-way-bound search input
- `NodeLink` — clickable mono node identifier (used in trace trees)
- `Toast` — toast notification service
- `TraceNodeComponent` — recursive component for trace trees. New `nodeSelected` output. Seam-colored chips.
- `Spinner`, `SectionCard` (to be removed/replaced)

---

## 11. Tauri Shell Context

- `tauri.conf.json`: `decorations: false` — no OS title bar. App provides custom window controls.
- Title bar: 30px, `data-tauri-drag-region` on left portion
- Window controls: minimize/maximize/close via `@tauri-apps/api/window`. Dynamic import with `window.__TAURI__` guard.
- Min size: 900×600. Resizable. Default 1280×820.
- Tauri v2, `@tauri-apps/api: ^2.11.1`

---

## 12. Styling Approach

- Tailwind CSS v4 with custom design tokens in `src/styles.css`
- Dark-first. `data-vibe` attribute on `<html>` for theme switching (hacker/terminal/default vibes)
- Custom color variables: `--ink`, `--ink-muted`, `--ink-subtle`, `--accent`, `--bg-base`, `--bg-surface`, `--bg-elevated`, `--line`, `--surface-2`, `--danger`, `--warn`, `--success`
- Base font: 13px UI, monospace for code elements
- `text-2xs` density for labels and metadata

---

## 13. Route Table (current)

| Route | Page Component | Content |
|-------|---------------|---------|
| `/` `/overview` | OverviewPage | Landing (no session), Console, Identity, Architecture, Stats |
| `/entries` | EntriesPage | SectionEntries (full table) |
| `/trace` | TracePage | SectionTrace + SectionLens |
| `/graph` | GraphPage | SectionGraph |
| `/insights` | InsightsPage | InsightsView |
| `/export` | ExportPage | SectionExport (modal) |
| `/settings` | SettingsView | 5-tab settings |

All lazy-loaded. State in stores (not lost on route switch). URL state: `/trace?focus=X`, `/entries?sort=col&dir=asc&kind=X&q=query`.
