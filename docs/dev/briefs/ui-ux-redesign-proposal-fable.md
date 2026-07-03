# DevContext Desktop — UI/UX Redesign Proposal ("F proposal") · v2

> **Status: PROPOSAL · EXECUTABLE SPEC** · Authored 2026-07-03 · Response to `ui-ux-redesign-brief.md`
> Third independent proposal, after the C and CT proposals that were synthesized into
> `go-to-program/ITERATION-I11-focus-workspace.md`. Diffed against I11 in §11.
> v2 adds: full design system (colors/themes/type/motion), CSS framework verdict,
> reactivity & cancellation architecture, status-bar insight ticker, Tauri platform
> practices, complete feature coverage matrix, and an agent-executable **waterfall**
> plan (skeleton → components → wiring).
>
> Every feature names the RPC data it is built from — nothing requires an engine
> change unless explicitly marked **[ENGINE]**.

---

## 0. Agent Execution Protocol (read first, every stage)

This plan will be executed by cold agents across many sessions. Global rules:

```powershell
# Entry ritual — every session
git -C C:/Code/DevContext2-ui pull
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # must be green BEFORE starting; capture the REAL exit code:
                    #   pnpm check > check.log; echo $LASTEXITCODE   — never pipe to tail
```

1. **Waterfall discipline.** Stages W0→W7 run in order. Do not start Wn+1 until Wn's
   gate passes. Within a stage, tasks are numbered — execute in order, tick them in
   `docs/dev/go-to-program/PROGRESS-LOG.md` as you go.
2. **Reachability rule.** A component "exists" only when it is reachable from
   `app.config.ts` routes or a wired overlay. This repo has previously shipped work
   into a dead component tree (see AGENTS.md history) — never again. After creating a
   component, prove reachability in the gate.
3. **No new dependencies** unless this doc lists them (§7.4 lists the allowed set).
   If you believe one is needed, stop and record the proposal in PROGRESS-LOG instead.
4. **No scope drift.** Restyling outside the stage's listed files is a defect, not a bonus.
5. **Exit ritual.** `pnpm check` green (real exit code) → manual gate checklist below the
   stage → PROGRESS-LOG entry (stage, tasks done, deviations) → one commit per stage,
   message `feat(ui): W<n> — <stage name>`.

---

## 1. Overall UI/UX Vision

### One workbench, one selection, one trail

The mental model today is *pages*: "I go to Entries, then Trace, then Graph." The
engine's model is **one immutable snapshot, many projections**. The UI must mirror it:

1. **A repo snapshot is a workspace** (a tab). Everything in a tab is a view of one
   analysis result.
2. **Selection is the cursor.** Exactly one "current focus" per tab (`TraceStore.focus`
   — already exists). The entry deck moves it, the trace tree refines it, the graph
   re-aims it, the Inspector describes it, the LLM pane renders it. Nothing navigates;
   everything *reacts*.
3. **Exploration leaves a trail, and the trail is the product.** Every selection pushes
   onto a per-tab trail: simultaneously the breadcrumb, the undo/redo stack, and the
   **seed for Export**. Pinned steps become the context pack. Understanding accumulates
   instead of evaporating on page switches.

### The feel (and the wrong feel we are killing)

What was wrong: `rounded-md border shadow-sm` cards around everything, `backdrop-blur`
header, heavy hover fills, web-page whitespace — a dashboard cosplaying as a tool.
What we want: **VS Code's chrome discipline + Telegram's warmth in prose zones.**
Density where you scan (deck, tree); air where you read (LLM pane, insights, Home
digest). Everything keyboard-first, everything cancellable, nothing ever blanks.

### Trust principle: never show fabricated data

The proto gives real `inDegree`/`outDegree` — show them. It does not give LOC or
cyclomatic complexity — show nothing until it does. No hash-faked placeholders (§11 #1).

---

## 2. Key Screens / Main Areas + Core Flows

| Surface | Route | What it is |
|---------|-------|------------|
| **Start** | `/` (no session) | Native folder picker + recents + advanced options. VS Code Welcome vibe. |
| **Home** | `/` (session ready) | Console during analysis → digest: identity strip, **Top Flows**, insight headlines, run report. |
| **Workbench** | `/explore` | Entry Deck │ Stage │ Inspector. Absorbs Entries + Trace + Graph + Lens. |
| **Atlas** | `/atlas` | Architecture: map markdown, project topology graph, packages, pipeline behaviors, **Event Wiring Board**, Hub Radar. |
| **Insights** | `/insights` | Severity-ranked digest; every insight links back into the Workbench. |
| Omnibox | overlay (Ctrl+K / Ctrl+P) | Single search surface: entries, nodes, actions, recents, verbs. |
| Export drawer | overlay (Ctrl+E) | Section toggles + presets + **From Trail** pack. |
| Settings | `/settings` | Same 5 tabs, restyled; Storage tab goes live via Tauri fs (§7). |

### The Workbench

```
┌ TitleBar 30px ─────────────────────────────────────────────────────────────────┐
│ ◆ DevContext   [ eShop — search or jump…  Ctrl+K ]              ●  ─  □  ✕    │
├ TabStrip 32px ─────────────────────────────────────────────────────────────────┤
│ ● eShop ×   ○ TodoApi ×                                                  [+]  │
├ Trail 22px (hidden until first selection) ─────────────────────────────────────┤
│ ⌂ › GET /api/orders › OrderService.Process › PricingService    ⟲ ⟳   ◈ 3 pins │
├───┬──────────────────┬───────────────────────────────┬─────────────────────────┤
│ A │ ENTRY DECK       │ STAGE   [System|Flow|Node]    │ INSPECTOR               │
│ c │ /filter…    94   │         [Tree ⇄ Graph]        │ ▾ Details               │
│ t │ [HTTP][Bus][Job] │                               │   OrderService.Process  │
│ i │                  │ OrdersController              │   Service · Orders.cs   │
│ v │ GET /orders    ● │ ├─ call  OrderService         │   in 9 · out 18         │
│ i │ POST /orders  ⚠  │ │   ├─ send  [Bus] Pricing    │ ▾ Call stack (depth 2)  │
│ t │ GET /customers   │ │   └─ data  OrderRepo        │ ▸ Insights (2)          │
│ y │ GET /report      │ └─ call  AuthService          │ ▾ LLM context  2.3k ⧉   │
│   │ …                │                    ┌────────┐ │   The GET /orders…      │
│ B │                  │ seam chips · depth │mini-map│ │ ▸ Trail                 │
│ a │                  │                    └────────┘ │                         │
├───┴──────────────────┴───────────────────────────────┴─────────────────────────┤
│ eShop · 94 entries · 512 nodes · 89% wired │ ▸ atlas 42/94 │ ⚡ ticker │ ● v1.2 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

- **Entry Deck** (left, 260px, virtual-scrolled): flat listbox, not a table. `j/k`
  scrub, `/` filters, kind chips with counts, insight badges on referenced rows.
  Selection auto-traces (debounced 150ms, cancels in-flight). `Shift+E` opens the full
  sortable audit table (today's section-entries) as an overlay.
- **Stage** (center): one canvas, three **altitudes** — the graph is never blank:
  - **System** — project topology from `MapResponse.topology[]`. Available the moment
    analysis completes. Click a project → deck filters to it (`EntryPoint.project`).
  - **Flow** — current trace as Tree or Graph (toggle). Seam chips, depth, detail.
  - **Node** — one-hop `GetNeighbors` neighborhood (out/in/usages toggle).
  - Double-click any graph node anywhere → re-trace from it.
- **Inspector** (right, dock 0/30/40/100%): selection-driven collapsible sections —
  Details (kind, **file path via `getNode(entry.nodeId).filePath`** — partially
  unblocks GAP-S2 client-side, tags, real in/out degree), Call Stack (compact tree,
  depth 2), Insights-for-selection, LLM context (Render RPC, 250ms debounce, token
  count, copy — works from the 28px collapsed strip), Trail (steps + pins).

### Core flows (click accounting)

- **A. Cold start → oriented.** Native folder picker → Console streams boot log →
  Home digest answers "what is this?": archetype/style badges, stat strip, Top Flows
  ranked and clickable, top-3 insights. Click a flow → Workbench, traced.
- **B. Sweep.** `j/k` down the deck; stage + Inspector + LLM pane track live. Twenty
  entries surveyed in twenty keystrokes, zero page switches (today: ~4 clicks + 2 page
  switches each).
- **C. Deep dive.** Click tree node → Inspector describes it, trail extends → Node
  altitude → spot odd caller → double-click → re-trace. `Ctrl+Z` walks the whole
  excursion back.
- **D. Impact question.** Omnibox → type entity → Tab-cycle verb to "Usages" → direct
  callers on stage; Inspector shows "Reached by N flows" (§3.4). "What breaks if I
  touch this?" in four keystrokes.
- **E. Ship it.** Pin twice (`p`) during C → `Ctrl+E` → "From Trail" → identity header
  + pinned flows via Render RPC + insight notes → one copy, summed `estimatedTokens`.

---

## 3. New Insights & Features (each grounded in existing RPCs)

### 3.1 Flow Atlas — background flow indexing ★ the enabler
After analysis, a low-priority queue shallow-traces every entry (`getTrace` depth 2–3,
detail salient, 4 concurrent, pausable, cancelled on tab close, cap 100). Per-entry
`FlowStat`: breadth (node count), boundary crossings (`seam ∈ {send,consumes,raises,
handler}`), data touches (`seam=data` + `touchedEntities[]`), `emittedEvents[]`,
confidence (% `resolution="Semantic"`). Progress in the status bar ("atlas 42/94").
Pure client; N stateless queries against the immutable snapshot.

### 3.2 Top Flows (Home)
Rank atlas by breadth × boundary crossings; top ~7 cards: route + one-line shape
("crosses messaging, touches 3 entities, 92% verified") → click → Workbench. The
highest-leverage "I just cloned this repo" feature.

### 3.3 Event Wiring Board (Atlas)
Join atlas `emittedEvents[]` (publishers) with `MessageConsumer`/`DomainEventHandler`
entries (subscribers) by event-name match. Three-column board: *publishing flow →
event → consuming entry*; both sides click through to traces. Heuristic join → rows
badged `[approx]`, consistent with existing resolution vocabulary.

### 3.4 Impact lens — "Reached by N flows"
Invert the atlas: node/entity → entry points whose traces reach it. Inspector Details
line + omnibox verb. Complements `GetNeighbors(usages)` (direct callers) with
flow-level reachability. Caveat shown while atlas coverage < 100%.

### 3.5 Confidence Ledger
Per-flow and repo-wide verified% from `resolution`; subtle meter on trace headers +
Home line. One click filters the tree to `[approx]` edges — "show me where the map
might be wrong."

### 3.6 Unwired Entries
`summary.entries − summary.entriesWithTarget` counts them; per-entry `target == null`
locates them. Insight card + deck quick-filter (exists) + Home count.

### 3.7 Hub Radar
Batch `getNode` over atlas node IDs → real degrees → top hubs on Atlas ("everything
passes through `UnitOfWork.Commit`"). Labeled "among mapped flows" — no whole-graph
RPC exists and we don't pretend otherwise.

### 3.8 Pinning & Context Packs (Trail → Export)
`p` pins the selection. Export presets — **Onboarding** (Identity+Architecture+Entries),
**Flow Review** (current trace, depth 2), **Full**, and **From Trail** (each pinned
focus rendered via `Render{focus,sections}`, concatenated with headers, tokens summed).

### 3.9 (Stretch) Snapshot diff
Re-analyze same path → client-side diff of `entryGroups` + summary: "+3 endpoints,
−1 consumer, wired 87→91%". Deferred to W7.

---

## 4. Design System — "Graphite" (opinionated)

### 4.1 CSS framework verdict: keep Tailwind CSS v4 — and here is the discipline

**Decision: Tailwind v4, CSS-first `@theme` tokens, plus a small `@layer components`
vocabulary. No component library.** Reasons: (a) already in the repo — zero migration
burn; (b) v4 tokens are plain CSS custom properties, so theming = swapping variables
under `data-theme`/`data-vibe`, no build ceremony; (c) utilities co-located in
templates are the fastest medium for *agents* — no naming negotiation, no orphaned
stylesheets; (d) zero runtime cost in a desktop WebView.

Rejected: **Angular Material / PrimeNG** (widget aesthetics fight the IDE feel; you
spend the budget de-theming them), **SCSS design system** (agents drift without
co-located styles), **CSS-in-JS** (not idiomatic Angular, runtime cost).

Guardrail for agent drift — exactly one shared vocabulary in `styles.css`:

```css
@layer components {
  .panel      { /* bg-surface + 1px line border-right/left, radius 0 */ }
  .list-row   { /* h-7, px-2, text-xs, hover wash, selected: 2px left accent + accent-dim bg */ }
  .chip       { /* h-5, px-1.5, text-2xs, 1px line border, radius 3px */ }
  .kbd        { /* mono text-2xs, bg-elevated, radius 3px, px-1 */ }
  .section-h  { /* text-2xs font-medium text-ink-muted, twisty inline, h-6 */ }
  .prose-zone { /* 13.5px, leading-relaxed, max-w-[68ch] */ }
  .hairline   { /* 2px indeterminate progress bar, accent, top-edge absolute */ }
}
```
Anything more specific stays as utilities in the component template. New `@layer
components` classes require a PROGRESS-LOG note.

### 4.2 Color — dark-first "Graphite" theme (exact values)

Declared in `@theme` as `--color-*`; consumed as `bg-base`, `text-ink`, etc.

| Token | Value | Use |
|-------|-------|-----|
| `bg-base` | `#16181d` | window, titlebar, statusbar |
| `bg-surface` | `#1b1e24` | panels (deck, stage, inspector) |
| `bg-elevated` | `#222630` | overlays: omnibox, drawer, peek, toasts |
| `bg-hover` | `#ffffff0a` | interactive hover wash (never full fills) |
| `line` | `#ffffff12` | the only structural separator (1px) |
| `line-strong` | `#ffffff1f` | focused panel edge, drag handles |
| `ink` | `#dfe2e8` | primary text |
| `ink-muted` | `#99a0ac` | secondary text, icons |
| `ink-subtle` | `#6b7280` | tertiary, disabled, placeholders |
| `accent` | `#8b93ff` | selection, active nav, primary buttons, links |
| `accent-hover` | `#a0a7ff` | |
| `accent-ink` | `#10122b` | text on accent fills |
| `accent-dim` | `#8b93ff26` | selection wash (15%) |
| `ok` | `#7bd88f` | verified, success, connection dot |
| `warn` | `#e5c07b` | approx, warnings |
| `danger` | `#e06c75` | errors, destructive |
| `info` | `#6cb2eb` | informational insights |

The accent is a violet-blue: distinctive (not VS Code azure), a quiet nod to .NET
purple without corporate heaviness, and it survives dimming to 15% washes.

**Seam palette** (trace chips + graph node classes; One-Dark-adjacent hues chosen to
read "IDE-native" and stay distinguishable on `bg-surface`):

| Seam | Color | Rationale |
|------|-------|-----------|
| entry | `#8b93ff` accent | roots are landmarks |
| call | `#99a0ac` ink-muted | most common — must recede |
| send | `#e5c07b` amber | leaving the process should glow warm |
| consumes | `#56b6c2` teal | arriving pairs against send |
| handler | `#c678dd` violet | |
| raises | `#d19a66` orange | fires-and-forgets read hotter than calls |
| data | `#6cb2eb` blue | |
| di | `#808693` gray | plumbing recedes |
| pipeline | `#d16d9e` pink | |
| resolve | `#5ac8fa` cyan | |

Resolution: verified→`ok`, approx→`warn`, truncated→`ink-subtle`. Severity:
warning→`warn`, notable→`accent`, info→`ink-muted`.

**Themes & vibes.** `data-theme="graphite"` (default, above) and `data-theme="paper"`
(light, W7): base `#f6f7f9`, surface `#ffffff`, line `#00000014`, ink `#23262c`,
accent darkened to `#5b63d6` for contrast; follows OS via `prefers-color-scheme`
unless user overrides in Settings. Existing **vibes** (default/terminal/hacker) are
kept as *accent remaps over Graphite* — terminal: accent `#7bd88f` + mono UI font;
hacker: as today — the statusbar vibe cycler stays. Fun is a feature.

### 4.3 Typography

- **UI:** Inter Variable, **bundled locally** (woff2 in `/assets/fonts` — a desktop
  app never fetches fonts; also keeps Windows/macOS/Linux pixel-identical).
  Fallback `system-ui`.
- **Code/mono:** JetBrains Mono (bundled, subset to Latin) for node IDs, routes,
  trace titles, token counts. Fallback `Cascadia Code, Consolas, monospace`.
- **Scale:** 11px labels (`text-2xs`) · 12px dense/meta · **13px UI base** · 15px
  section titles · 18px page titles. Chrome line-height 1.45; `.prose-zone` 1.6.
- **Numbers:** `font-variant-numeric: tabular-nums` on all stat cells, tickers,
  token counts — no jitter while values stream.

### 4.4 Shape, elevation, motion

- **Radius:** 0 on panels/structural; 3px controls (buttons/inputs/chips); 6px
  floating overlays. Square bones, softened touch points.
- **Elevation = honesty:** resting panels never cast shadows (1px `line` separates
  them). Only things that truly float — omnibox, drawer, peek, toasts, menus — get
  the single shadow `0 8px 24px #00000059` on `bg-elevated`.
- **Motion:** 120ms ease-out for state (hover, selection, section collapse); 200ms
  for overlays (drawer slide, omnibox fade+2px rise); **transform/opacity only**,
  never animate layout; `prefers-reduced-motion` collapses all to 0ms. No bounce.
- **Focus:** `:focus-visible` → 1px accent outline, 2px offset. Native scrollbars.

---

## 5. Reactivity & Cancellation Architecture

The app must *feel* alive: nothing blocks, nothing blanks, everything abortable.

### 5.1 RPC layer: epoch + abort (`core/rpc-call.ts`, new)

Every store RPC goes through one wrapper:

```typescript
// Semantics: switchMap for signals. Latest call wins, losers are dropped AND aborted.
runLatest(key, epochRef, fn: (signal: AbortSignal) => Promise<T>): Promise<T | STALE>
```
- Stores stamp an epoch per key (`trace`, `render`, `node`, `search`); responses
  carrying a stale epoch are discarded — no flicker from out-of-order returns.
- The `AbortSignal` reaches the transport (grpc-web unary supports abort via the
  underlying fetch/XHR; the analyze *stream* handle exposes `.cancel()` — wire both).
- Tab close → abort all in-flight for that tab + `CloseSession(handle)` (exists).
- Atlas indexer (§3.1) is a cooperative queue: `pause()`, `resume()`, `cancel()`;
  auto-pauses while a user-initiated trace is in flight (user latency > indexing).

### 5.2 Content-preserving loading (the biggest feel win)

Never unmount → spinner → remount. Policy table:

| Surface | While loading | On error |
|---------|--------------|----------|
| Stage tree/graph | keep last content at 60% opacity + `.hairline` on top edge | inline error row + Retry, last content stays dimmed |
| Inspector sections | per-section skeleton blocks (shimmer 1.2s) on first load; dim+hairline on refresh | section-scoped error + Retry |
| LLM pane | previous markdown dimmed + token badge pulses | error line + Retry (keeps last render) |
| Entry deck | skeleton rows on first load only | banner row |
| NodeCard / Peek | skeleton (fixes GAP-B8) | text + retry + copy details (exists) |
| Analysis | Console stream (exists) + statusbar segment | error state (exists) |

Selection echo is **instant**: on `j/k` the Inspector Details fills from local
`EntryVm` metadata synchronously; RPC-backed sections follow when they land.

### 5.3 Cancellation surfaces (visible, not just internal)

- Analysis: Cancel button (exists) + statusbar segment shows `✕` on hover.
- Trace in flight > 400ms: hairline + `Esc` cancels (Esc-ladder layer 0).
- Atlas: statusbar segment click → popover with Pause/Resume/Cancel.
- Omnibox search: superseded per keystroke via `runLatest` (150ms debounce).

### 5.4 Degraded states

- `ConnectionStore.online == false` → statusbar dot goes `danger`, a 24px inline
  banner appears under the tabstrip ("Engine offline — retrying…"); auto-clears on
  reconnect (5s Ping poll exists). Views keep last data; actions disable, never hide.
- Debounce policy: deck scrub 150ms · LLM render 250ms · omnibox 150ms · filter 100ms.

---

## 6. StatusBar — the useful-insight footer (full spec)

22px, `bg-base`, top edge carries `.hairline` while anything streams. Segments left→right
(all segments are flat buttons, VS Code-style):

| Segment | Content | Click |
|---------|---------|-------|
| Context | `eShop · 94 entries · 512 nodes · 89% wired` | → Home |
| Task | during analysis: `▰▰▱ parsing 62% — OrderService.cs`; during atlas: `atlas 42/94`; during trace: micro-spinner | → Console / atlas popover (pause/cancel) |
| **Ticker** ⚡ | rotating one-liners, 6s cadence, pause on hover, ← → cycle | → the relevant view |
| Connection | `● v1.2.3` (`ok`/`danger` dot) | → Settings·Server |
| Vibe | current vibe name | cycles vibe (exists — keep) |

**Ticker content sources, in priority order:**
1. *During analysis* — facts as they stream from `ProgressEvent` + partial summary:
   "12 projects found" → "MediatR pipeline detected" → "94 entry points". The boot
   wait becomes the first moment of insight delivery.
2. *After analysis* — top `Insight` headlines by severity ("⚠ Missing auth on
   POST /orders — view"), then atlas discoveries ("GET /report touches 3 entities
   across 2 boundaries"), then unwired-entry count (§3.6).
3. *Idle filler* — keyboard tips ("`?` shows every shortcut", "`p` pins to your
   trail"), shown at most 1-in-4 rotations, never repeating within a session.

Implementation: `TickerService` with a priority queue of `{text, icon, link, ttl}`;
sources push, service rotates. Deduplicates, persists "seen tips" in PrefsStore.

---

## 7. Tauri Platform Practices (cross-platform correctness)

### 7.1 Engine lifecycle — the server becomes a sidecar
Today the .NET server on :5179 is started by hand. Ship it as a Tauri **sidecar**
(`externalBin`): spawn on app start with `--port <free>` (portpicker in Rust, pass to
the webview via injected config — kills port-collision), health-poll `Ping` until
`ready:true` (Start page shows "Starting analysis engine…" state), kill child on
`RunEvent::ExitRequested` (never orphan a process), restart with 1s/5s/15s backoff on
crash + statusbar `danger` dot while down. Dev mode (`pnpm dev:web` + manual server)
keeps working via a `VITE`-style env fallback to :5179.

### 7.2 Window correctness
- **No white flash:** window starts `visible: false`, `backgroundColor: '#16181d'`
  in `tauri.conf.json`; Angular calls `getCurrentWindow().show()` after first render.
- **State restore:** `tauri-plugin-window-state` (size/pos/maximized across launches).
- **Custom titlebar (decorations:false) done right:** `data-tauri-drag-region` only on
  non-interactive strips (v2 gives double-click-maximize on drag regions for free);
  interactive children (omnibox trigger, dropdown, window buttons) must NOT carry it.
  macOS later: overlay traffic lights (`titleBarStyle: Overlay`) — keep left 80px clear
  behind a `--titlebar-inset` token instead of hardcoding.
- Min 960×640. Test at Windows 125%/150% scaling before every gate (WebView2 DPI).

### 7.3 WebView keyboard interception (silent killers)
WebView2 answers `Ctrl+P` (print), `Ctrl+R`/`F5` (reload — **destroys all tab state**),
`Ctrl+F` (find), `Ctrl+ +/-` (zoom). W1 installs a capture-phase keydown handler that
`preventDefault()`s these and reroutes: Ctrl+P→omnibox, Ctrl+R→re-analyze (deliberate,
with focus restore), F5→nothing, Ctrl+F→deck filter, zoom keys→nothing.

### 7.4 Plugins (the allowed dependency list)

| Plugin | Enables | Stage |
|--------|---------|-------|
| `dialog` | **native folder picker on Start/landing** — the raw text path input dies | W4 |
| `fs` (scoped) | Settings·Storage real file ops — unblocks GAP-S3 | W6 |
| `opener` | "Reveal in Explorer" / "Open file" from Inspector `filePath` | W6 |
| `single-instance` | second launch focuses window; path arg → new tab | W6 |
| `window-state` | §7.2 | W6 |
| `clipboard-manager` | reliable copy (LLM pane, node IDs) — `navigator.clipboard` is flaky in WebView2 without focus | W4 |
| npm: `@angular/cdk` (Scrolling only) | virtual scroll for deck/audit table (PublicApi libraries → thousands of rows) | W2 |

**Security:** per-plugin capabilities scoped minimally; CSP in `tauri.conf.json`:
`default-src 'self'; connect-src http://localhost:* ws://localhost:*; style-src 'self'
'unsafe-inline'; font-src 'self'` — no remote content, fonts local (§4.3).

---

## 8. Component & Skeleton Structure

### 8.1 Component tree (target)

```
workspace-shell                        // CSS grid rows: 30/32/22?/1fr/22 (trail row collapses)
├─ shell/titlebar/titlebar.ts          // drag region, omnibox TRIGGER (not a second search),
│                                      //   repo dropdown, server dot, window controls (Tauri-guarded)
├─ shell/tab-strip.ts                  // EXISTS — wired in W1. Ctrl+T/W/1-6, MRU Ctrl+Tab
├─ shell/trail-bar.ts                  // breadcrumb + ⟲⟳ + pin count; hidden when empty
├─ shell/activity-bar.ts               // 48px icons: map(Home) layers(Explore) boxes(Atlas)
│                                      //   info(Insights) settings — badges, disabled-visible
├─ shell/offline-banner.ts             // §5.4
├─ <router-outlet>
│  ├─ pages/start-page.ts              // folder picker (dialog plugin), recents, advanced opts
│  ├─ pages/home-page.ts               // console | digest (identity, top-flows, insights, report)
│  ├─ pages/workbench-page.ts          // 3-col grid + 4px drag handles (pointer events, min widths)
│  │  ├─ explorer/entry-deck.ts        //   cdk-virtual-scroll listbox, j/k, chips, insight badges
│  │  ├─ explorer/audit-table.ts       //   Shift+E overlay = today's sortable section-entries
│  │  ├─ explorer/stage.ts             //   altitude switch + toolbar (seams/depth/detail)
│  │  │  ├─ explorer/trace-tree.ts     //     progressive twisties (replaces trace-node.ts)
│  │  │  ├─ ui/graph-canvas.ts         //     EXISTS — gains topology + neighbors builders,
│  │  │  │                             //     zoom-to-fit on data change, dblclick→re-trace
│  │  │  └─ explorer/mini-map.ts       //     thumbnail when tree mode
│  │  └─ inspector/inspector.ts        //   dock levels; sections:
│  │     ├─ inspector/details-section.ts        // + file path via getNode, + reached-by (§3.4)
│  │     ├─ inspector/callstack-section.ts
│  │     ├─ inspector/insights-section.ts
│  │     ├─ inspector/llm-section.ts            // migrates section-lens render/debounce/copy
│  │     └─ inspector/trail-section.ts
│  ├─ pages/atlas-page.ts              // map md (prose-zone), topology, packages, pipeline
│  │  ├─ atlas/event-board.ts          //   §3.3
│  │  └─ atlas/hub-radar.ts            //   §3.7
│  ├─ pages/insights-page.ts
│  ├─ pages/styleguide-page.ts         // DEV-ONLY route: token sheet + every ui/* component
│  └─ settings/settings-view.ts        // + theme (graphite/paper/system), + dock pref, + storage live
├─ features/omnibox/omnibox.ts         // THE single search surface
├─ features/export/export-drawer.ts    // Ctrl+E, presets + From Trail
├─ features/peek/node-peek.ts          // 200ms hover card (real fields only)
├─ shell/statusbar/statusbar.ts        // §6 segments + ticker
└─ ui/*                                // Icon Button Badge Sheet StatCell SearchField NodeLink
                                       //   Toast + NEW: Skeleton, Meter, Ticker, KindIcon,
                                       //   SeamChip, EmptyState. SectionCard DELETED.
```

### 8.2 Stores

```typescript
// WorkspaceStore — per-tab additions
TabTrailSlice { steps: TrailStep[]; cursor: number; pins: TrailStep[] }
  // TrailStep {kind:'entry'|'node'|'insight', id, title, focus, ts} — cap 50 FIFO
mruStack: string[]                                    // Ctrl+Tab

// NEW TrailStore — facade: push(), undo(), redo(), pin(), jumpTo(i)
// NEW AtlasStore — facade over TabAtlasSlice:
TabAtlasSlice { flows: Map<focus,FlowStat>; status; indexed; total;
                eventWiring: computed; reachedBy: computed; hubs: computed }
// NEW TickerService — §6
// core/rpc-call.ts — runLatest epoch/abort wrapper — §5.1
// SessionStore — duplicate-path guard; save/restore focus across re-analyze
// PrefsStore  — + dockLevel, theme ('graphite'|'paper'|'system'), seenTips[]
// TraceStore  — unchanged API; internals move onto runLatest
```

### 8.3 Routes & URL state

```
/           → StartPage | HomePage (one route, two states)
/explore    → WorkbenchPage    ?focus=X&view=system|tree|graph|node&kind=&q=
/atlas /insights /settings
/styleguide → dev-only (guarded by isDevMode())
/entries /trace /graph /overview → redirects (deep-link compat)
```

### 8.4 Keyboard map (conflict-free — see §11 #5 on I11's clash)

| Key | Action |
|-----|--------|
| Ctrl+K / Ctrl+P | Omnibox (Tab cycles verbs: Trace · Node · Usages · Copy) |
| Ctrl+T / Ctrl+W / **Ctrl+1-6** | tab new / close / **jump** — tabs own the digits |
| Ctrl+Tab / Ctrl+Shift+Tab | MRU cycle |
| Ctrl+Shift+L | Inspector toggle (0 ↔ last); presets via omnibox `>dock`; drag for custom |
| Ctrl+E | Export drawer |
| Ctrl+Z / Ctrl+Y · Alt+←/→ | trail undo / redo |
| Ctrl+R | re-analyze with focus restore (intercepted from WebView, §7.3) |
| j/k · / · Enter · Shift+E | deck: scrub · filter · pin-selection · audit table |
| v t / v g / v s / v n | stage: tree · graph · system · node |
| p | pin selection to trail |
| g then o/e/a/i/s · ? · Esc | view nav · help overlay · **Esc-ladder**: cancel in-flight trace → close overlay → unpin peek → deselect node → clear focus → clear filter |

---

## 9. Feature Coverage Matrix (nothing gets dropped)

Every existing feature, every gap, every new feature → its waterfall stage.

### Existing features (must survive the redesign)

| Feature | Lands in | Notes |
|---------|----------|-------|
| Landing path input + recents + advanced opts | W4 Start page | input → native picker (§7.4) |
| Console boot-log + RunReport (stages/funnel/cache/extractors) | W4 Home | restyled card-free |
| Identity / Architecture / Stats sections | W4 Home + Atlas | Atlas absorbs architecture+stats detail |
| Entries table (sortable, URL sort, kbd nav, quick-filters) | W2/W4 audit-table overlay | deck is primary; table kept for auditing |
| Trace tree + seam chips + approx/verified | W2 trace-tree | progressive twisties |
| Graph (Cytoscape+dagre, theme-reactive, node click) | W2/W4 stage | + zoom-to-fit, dblclick, altitudes |
| Lens 50/50 (human/LLM, 250ms render, Ctrl+C) | W4 inspector llm-section | |
| Insights view (severity groups, evidence, retry) | W4 insights-page | + badges on deck rows (W5) |
| NodeCard sheet | W4 | skeleton loading (W7 polish if slipped) |
| Palette (entries/nodes/actions) | W4 omnibox | + debounce/verbs/recents/icons/empty-text |
| Export modal (toggles, copy, tokens, re-render) | W4 export-drawer | + presets + From Trail |
| Settings 5 tabs + vibes | W4/W6 | + theme picker; Storage live in W6 |
| Multi-tab infra (WorkspaceStore, tab-strip, facades) | W1 | finally wired |
| ConnectionStore polling / RecentStore / PrefsStore | W1/W3 | + offline banner §5.4 |
| Deep links (`/trace?focus=`, entries sort URL) | W4 | via redirects §8.3 |
| Toast service, footer vibe cycler | W1 | vibe cycler stays in statusbar |

### The 23 gaps

| Gap | Stage | Gap | Stage |
|-----|-------|-----|-------|
| T1 tabs wired | W1 | B1 palette debounce | W4 |
| T2 shortcuts in `?` help | W7 | B2 Tab verbs | W4 |
| T3 shortLabel bug | W1 | B3 recents in omnibox | W4 |
| T4 duplicate-path guard | W3 | B4 kind icons | W4 |
| T5 Ctrl+Tab MRU | W1 | B5 empty-text w/ query | W4 |
| N1 `/` route | W1 | B6 graph empty state | W4 (altitudes kill it) |
| S1 auth column | **[ENGINE]** blocked | B7 export states | W4 |
| S2 file:line | **partial now**: `getNode().filePath` in Inspector (W4); line# **[ENGINE]** | B8 NodeCard skeleton | W7 |
| S3 storage file ops | W6 (fs plugin) | C1 dedupe helpers | W7 |
| S4 export presets | W4 | C2 palette perf | W4 (static/search split) |
| S5 disabled rail visible | W1 | S6 rail badges | W1 |
| S7 icon registry names | W1 | | |

### New in this proposal
Flow Atlas W5 · Top Flows W5 · Event Board W5 · Impact lens W5 · Confidence W5 ·
Unwired W5 · Hub Radar W5 · Trail W4 · Context Packs W4 · Ticker W5 · Sidecar W6 ·
Paper theme W7 · Snapshot diff W7 (stretch).

---

## 10. The Waterfall Plan (agent-executable)

Skeleton first; components in isolation; then wiring into the shell. Each stage:
**Entry** (precondition) → numbered tasks → **Gate** (verification). One commit per
stage. `pnpm check` with real exit codes at entry and exit — always.

### W0 — Design System Foundations
**Entry:** `pnpm check` green.
1. `styles.css`: full `@theme` token set from §4.2 (Graphite), seam/severity palettes,
   motion tokens, `@layer components` vocabulary (§4.1), reduced-motion collapse.
2. Bundle Inter Variable + JetBrains Mono into `/assets/fonts`; `@font-face`; remove
   any remote font reference.
3. `ThemeService`: `data-theme` (graphite/paper/system-follow) alongside existing
   `data-vibe`; vibes become accent remaps (§4.2).
4. `pages/styleguide-page.ts` + dev-only route: token sheet, type ramp, every `ui/*`
   component rendered in all states. This page is how every later stage proves visuals.
5. New `ui/` primitives: `Skeleton`, `Meter`, `SeamChip`, `KindIcon`, `EmptyState`, `Ticker`.
**Gate:** `/styleguide` renders tokens + primitives; app still boots with old shell
unbroken; grep finds no `shadow-sm|rounded-md` in `ui/*` primitives; fonts load offline.

### W1 — Shell Skeleton (the strong shell everything bolts into)
**Entry:** W0 gate.
1. `workspace-shell` regrid: titlebar 30 / tabstrip 32 / trail 22 (collapsible) /
   main / statusbar 22. All regions exist from day one — placeholders where needed.
2. Titlebar: rename from app-header; solid `bg-base`, no blur/shadow; drag-region
   hygiene (§7.2); omnibox *trigger* input (opens overlay; the overlay arrives W4 —
   until then it opens the existing palette).
3. Wire `tab-strip` (T1); fix `shortLabel` (T3); MRU stack + Ctrl+Tab (T5).
4. Activity-bar: rename from navigation-rail; 48px icons-only (map/layers/boxes/info/
   settings — registry-safe, S7); count badges (S6); disabled-visible + tooltip (S5);
   3px left-accent active indicator.
5. Statusbar: rename from footer; 22px; segment layout per §6 with ticker slot
   (static placeholder until W5); vibe cycler moves in.
6. Routes: explicit `/` (N1); stub `workbench/atlas` pages ("under construction" +
   link to old routes); keep old routes serving old components — **the app remains
   fully usable on the old pages throughout W1–W3**.
7. WebView shortcut interception (§7.3).
8. `shell/offline-banner.ts` driven by ConnectionStore.
**Gate:** tabs keyboardable (Ctrl+T/W/1-6/Tab); chrome reads IDE (no blur/shadows/
radius on panels); all routes reachable; old features still work; Ctrl+P/Ctrl+R/F5 no
longer trigger WebView defaults; kill the server → banner appears, dot reddens, revive
→ clears.

### W2 — Component Build (in isolation, proven on /styleguide)
**Entry:** W1 gate. Components take VM inputs / emit outputs — no store access yet;
each gets a styleguide entry with mock data. This is the parallelizable stage: tasks
1–7 are independent (safe to split across agents).
1. `entry-deck` (cdk virtual scroll, j/k, chips, badges) + `audit-table` (port of
   section-entries table).
2. `trace-tree` (progressive twisties, seam chips, resolution badges, 12px indent).
3. `stage` shell (altitude/mode toolbar, seam filter chips, depth/detail, empty states
   per altitude) + `mini-map`.
4. `graph-canvas` upgrades: `buildFromTopology(ProjectNode[])`,
   `buildFromNeighbors(EdgeVm[])`, zoom-to-fit on data change, dblclick output.
5. `inspector` + five sections (skeleton-first loading states baked in).
6. `trail-bar`, `omnibox` (UI only), `export-drawer` (UI only), `node-peek`.
7. Home digest components: identity strip, top-flow card, insight headline row.
**Gate:** styleguide shows every component in loading/empty/error/populated states;
`pnpm check` green; zero routes changed; no store imports inside new components
(grep `inject(SessionStore|TraceStore` in `features/explorer|inspector` → 0 hits).

### W3 — State & RPC Hardening (no UI change)
**Entry:** W2 gate.
1. `core/rpc-call.ts` `runLatest` (§5.1); migrate TraceStore/NodeStore/search onto it.
2. Analyze-stream cancel + tab-close abort + `CloseSession` sweep.
3. `TabTrailSlice` + `TrailStore` (push/undo/redo/pin/jumpTo, cap 50).
4. `AtlasStore` skeleton (slice, queue with pause/resume/cancel — indexer arrives W5).
5. SessionStore: duplicate-path guard (T4); focus save/restore across re-analyze.
6. PrefsStore: `dockLevel`, `theme`, `seenTips[]` (schema-versioned migration).
7. Unit tests: epoch drop, abort on supersede, trail undo/redo truncation, dup-guard.
**Gate:** `pnpm check` green with new tests; rapid-fire trace calls in the running app
produce exactly one final tree (verify via network tab — aborted requests visible).

### W4 — The Great Wiring (skeleton + components + stores become the product)
**Entry:** W3 gate.
1. `workbench-page`: deck│stage│inspector grid, drag handles, dock levels
   (Ctrl+Shift+L), URL state (`?focus&view&kind&q`), selection loop:
   j/k → debounced trace (runLatest) → tree/graph → inspector → LLM render.
   Instant selection echo (§5.2). Esc-ladder.
2. Stage altitudes live: System (topology — **graph never blank**), Flow, Node
   (neighbors + direction toggle); project-click → deck filter; dblclick → re-trace.
3. Trail live: push on selection, trail-bar breadcrumb, Ctrl+Z/Y, Alt+arrows, `p` pins.
4. Inspector sections on real data — incl. file path via `getNode` (S2 partial) and
   LLM section migration (render/debounce/copy via clipboard plugin).
5. Omnibox live: absorbs palette; debounce (B1), Tab verbs (B2), recents (B3), kind
   icons (B4), query-in-empty (B5), static/search split (C2).
6. Export drawer live: toggles + Onboarding/Flow-Review/Full presets (S4) + From
   Trail (§3.8); loading/error states (B7).
7. Home assembled: console (analyzing) ⇄ digest (ready; top-flows slot shows entry
   list until W5 ranks it); Start page with native folder picker (dialog plugin).
8. Atlas assembled: map prose-zone, topology canvas, packages/pipeline (event board
   + hubs arrive W5). Insights page restyled + links into workbench.
9. Cut over routes: `/explore` real; old routes → redirects (§8.3); **delete**
   section-entries/-trace/-graph/-lens/-export + their pages + SectionCard.
**Gate:** flows A–E (§2) walked end-to-end; deep links `/trace?focus=X` land traced;
deleting files broke nothing (`pnpm check`); graph shows topology with zero traces;
kill server mid-trace → dimmed content + banner, no crash; audit table via Shift+E.

### W5 — Derived Insight Layer
**Entry:** W4 gate.
1. Atlas indexer live (§3.1): idle-queue, 4 concurrent, pause-on-user-trace,
   statusbar progress segment with pause/cancel popover.
2. Top Flows ranking on Home (§3.2). 3. Event Wiring Board (§3.3).
4. Impact lens in Inspector + omnibox verb (§3.4). 5. Confidence meters + approx
   filter (§3.5). 6. Unwired entries surfacing (§3.6). 7. Hub Radar (§3.7).
8. TickerService + statusbar ticker content (§6): analysis facts, insight headlines,
   atlas discoveries, rotating tips (seenTips in prefs).
**Gate:** open eShop-class repo → atlas completes; Home ranks flows; board joins ≥1
publisher/consumer pair; ticker rotates 3 source types; atlas cancels cleanly on tab
close (no orphan RPCs in network tab); user trace pauses indexing.

### W6 — Tauri Hardening
**Entry:** W5 gate.
1. Sidecar engine (§7.1): spawn/health/kill/backoff; dynamic port; dev-mode fallback.
2. No-flash startup + window-state plugin (§7.2). 3. single-instance (+ path arg →
   new tab). 4. fs plugin → Settings·Storage live (S3). 5. opener → Reveal/Open from
   Inspector. 6. CSP + capability scoping (§7.4). 7. DPI pass at 125%/150%.
**Gate:** cold-launch the packaged app with no manual server → lands on Start ready;
quit → engine process gone (Task Manager); relaunch restores window bounds; second
launch focuses first; Storage tab lists real paths/sizes.

### W7 — Polish & Acceptance
**Entry:** W6 gate.
1. node-peek wired to every NodeLink (200ms hover, Ctrl to pin). 2. NodeCard skeleton
   (B8). 3. Helper dedupe into `core/format.ts` (C1). 4. `?` help overlay = full §8.4
   map (T2). 5. Paper light theme + system-follow (§4.2). 6. Reduced-motion audit.
7. Snapshot diff (§3.9) if time. 8. Full acceptance sweep (below).
**Final gate:**

```
┌ pnpm check                      │ lint 0/0 · tests green · build 0w/0e (real exit code)
├ Feel                            │ zero shadows/radius on panels · 1px lines only ·
│                                 │ native scrollbars · content never blanks on load
├ Reactive                        │ j/k sweep 20 entries: no flicker, no stale trees ·
│                                 │ Esc cancels a slow trace · atlas pause/resume works
├ Never-blank graph               │ fresh session → System topology on stage
├ Trail                           │ dive 5 steps → Ctrl+Z ×5 → Ctrl+Y ×5 → breadcrumb exact
├ Export                          │ From Trail pack = pinned flows, token sum correct
├ Ticker                          │ rotates facts during analysis, insights after
├ Tauri                           │ sidecar lifecycle · no white flash · shortcuts safe
├ Coverage matrix §9              │ every row shipped or explicitly [ENGINE]-blocked
└ All 23 gaps                     │ closed except S1 (auth) and S2 line# — engine-blocked
```

---

## 11. Deltas vs I11 (for synthesis)

**Adopted from I11 unchanged:** three-region IDE shell, chrome-first phase ordering,
visual rules direction, Esc ladder, progressive tree, seam chips, export presets,
Shift+E audit table, quick peek, dock levels, redirect table.

| # | I11 | This proposal | Why |
|---|-----|---------------|-----|
| 1 | Hash-faked "nerd metrics" (~LOC, ~CC) | Real degrees only; nothing fabricated | Fake numbers in a trust tool poison the real ones beside them. |
| 2 | Graph exists only after a trace | Stage altitudes; System topology renders immediately | Kills blank-graph structurally; gives Map data a visual home. |
| 3 | History stack = undo only | Trail = breadcrumb + undo + pins + export seed | One concept instead of three; exploration produces an artifact. |
| 4 | Titlebar search AND Ctrl+K palette | One omnibox; titlebar is a trigger | Two search surfaces = two ranking behaviors to maintain. |
| 5 | **Ctrl+1-6 tabs AND Ctrl+1-4 dock — conflict in I11 §6** | Tabs own digits; dock = Ctrl+Shift+L + drag + omnibox | Spec bug; must be resolved either way. |
| 6 | Shift+wheel graph timeline | Cut | Low discoverability, high cost; Ctrl+Z covers it. |
| 7 | Architecture = restyled page | Atlas = architecture + event board + hubs + topology | The brief asks for new engine-derived insight surfaces. |
| 8 | — | Flow Atlas + Top Flows + impact lens + confidence + ticker | All client-side over existing RPCs. |
| 9 | — | Design system with exact tokens, CSS verdict, styleguide route | Agents need pinned values, not adjectives. |
| 10 | — | Reactivity/cancellation architecture (runLatest, content-preserving loading) | "Feels alive" is engineering, not styling. |
| 11 | — | Tauri: sidecar lifecycle, shortcut interception, no-flash, plugins | Desktop correctness was unspecified in I11. |
| 12 | Phases mutate live pages early | W1–W3 keep old UI fully working; cutover is one stage (W4) | Safer for multi-agent iteration; always a shippable app. |
