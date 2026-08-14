# Desktop UI Guide

The DevContext desktop app is an **Angular 22 (zoneless, signals) + Tauri 2** client over the same
engine as the CLI: the UI talks gRPC-Web to `DevContext.Server`, which analyzes once and serves every
view from the immutable snapshot. There is no WPF shell any more — if a doc mentions
`DevContext.Desktop`, BlazorWebView, or a Human/LLM/Stats tab layout, it is describing the retired app.

Source: `src/DevContext.App` (see its `AGENTS.md` for build/run). Contract:
`proto/devcontext/v1/devcontext.proto`. Run for development: `pnpm dev:web` (browser) or `pnpm dev`
(Tauri window); agents use `scripts/start-dev-bg.ps1`.

---

## Shell

```
┌ titlebar ─ tabs (one per repo session) ─ window controls ───────────────┐
├ activity bar │                                              │           │
│  Home        │                 routed page                  │ inspector │
│  Explore     │                                              │ (Explore) │
│  Atlas       │                                              │           │
│  Insights    │                                              │           │
│  MCP         │                                              │           │
│  Context     │                                              │           │
│  Settings    │                                              │           │
├ trail bar (recently visited nodes) ──────────────────────────────────────┤
└ statusbar ─ connection · session stats · clickable segments ────────────┘
```

- **Tabs** (`shell/tab-strip.ts`): one tab per analyzed repo, VS Code-style (32px strip, close on
  hover, middle-click close, active underline). Titlebar **New** always creates a tab; it never
  replaces or cancels others. Closing a tab that is still cloning/analyzing asks first.
- **Activity bar** (`shell/activity-bar.ts`): Home `h` · Explore `e` · Atlas `a` · Insights `i` ·
  MCP `m` · Context `c` · Settings `s`. Session-scoped pages are disabled until a repo is analyzed.
  Explore and Insights badges show live entry/insight counts.
- **Trail bar**: breadcrumb of visited nodes, grouped by flow; feeds Context Studio's
  "From current trail" seed.
- `?` opens the full keyboard-shortcut overlay. Dark theme is the default; a Paper light theme and
  system-follow live in Settings.

## Pages

| Route | What it shows |
|---|---|
| `/` **Home** | Repo hero: service-map hero (runnables, gateway/bus lanes), entries sparkbar, wiring-health %, freshness chip, top flows, "needs attention" insights, onboarding links (trace a detected flow, open Atlas, point your agent at the MCP). |
| `/explore` **Workbench** | The main graph workbench: stage canvas (Cytoscape) + **lenses** — Service / Layer / Feature / Flow (`?lens=`), plus a virtualized **Table lens** (toolbar toggle, `Shift+E`, CSV export). Left: entry deck. Right: **Inspector** with Overview / Code tabs (PrismJS-highlighted `read_source`), graph-adjacent insights, neighbors, and trail. Omnibox (`Ctrl+K`) searches nodes/entries; node hover shows a peek card. |
| `/atlas` **Atlas** | The printable architecture read: service diagram → top flows as stepper strips → event wiring board (publisher/event/consumer) → per-service cards (style badge, stack tags) → cross-cutting (pipeline behaviors, packages) → hub radar. Export copies a markdown one-pager. |
| `/insights` **Insights** | The full insight list (risk/wiring/topology/coverage…), filterable, each with evidence and deep links into Explore. |
| `/mcp` **MCP** | Agent hand-off page: live server/session status and copyable MCP client config. |
| `/context` **Context Studio** | Build an LLM context pack: scope picker (services → entries tree + omnibox + presets like "I'm changing this endpoint"), composition cards (flow, signatures, bodies, DI wiring, entities, contracts, identity…), budget slider (1k–16k tokens) with per-card server-measured meters, intent selector (trace/explain/review), markdown/plain format, Copy / **Save to repo** of the **server-assembled** pack (`Ctrl+E` from anywhere). Save writes `.devcontext/packs/<slug>.md` into the analyzed repo (gitignored by default, via the `SavePackFile` RPC — the app has no file system of its own) and shows the copyable "point your agent here" line for `CLAUDE.md`; Home's *Point your agent here* tile routes here. |
| `/settings` **Settings** | Theme (dark/light/system), storage (cache location/size, clones), app info. |

## The model — analyze once, query many

Analyzing a repo (local path or GitHub URL) creates a **session**; every page is a cheap query over
that session's frozen snapshot — switching lenses, tracing entries, or building context packs never
re-analyzes. Re-analyze explicitly from the source bar (stale chip appears when the repo changed on
disk). Multiple sessions coexist as tabs; the MCP server exposes the same sessions to agents.

## Where behavior lives (for contributors)

| Concern | File(s) |
|---|---|
| Routes | `src/app/app.config.ts` |
| Shell/tabs/rail/statusbar | `src/app/shell/*` |
| Graph canvas + lenses | `src/app/ui/graph-canvas/*`, `src/app/features/pages/workbench-page.ts` |
| Inspector (code pane, insights) | `src/app/features/inspector/*` |
| Context Studio | `src/app/features/context-studio/*` |
| Signal stores | `src/app/state/*` |
| gRPC client + generated types | `src/app/core/`, `src/app/core/grpc/gen/**` (`pnpm gen:proto`) |

Gate: `pnpm check` (lint + vitest + build). UI drive gate:
`node src/DevContext.App/scripts/ui-audit-drive.mjs` (server + ng serve required).
