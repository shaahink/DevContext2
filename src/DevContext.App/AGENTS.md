# DevContext Desktop (Tauri + Angular)

A cross-platform desktop client for DevContext. The UI (Angular 22, zoneless, signals) talks to
`DevContext.Server` over **gRPC-Web**; the server wraps the unchanged `DevContext.Core` engine.
Tauri provides the native shell (OS WebView — no bundled Chromium).

## Prerequisites

- **Node 24+** (`nvm use 24` — Angular 22 requires ≥ 22.22.3 / 24)
- **pnpm** (`corepack enable`)
- **.NET 10 SDK** (for the server)
- **Rust** + platform toolchain (for the Tauri shell): Windows needs VS Build Tools (VC.Tools) + WebView2 (preinstalled on Win11)

## Install

```bash
pnpm install
```

## Run

```bash
pnpm dev        # desktop: starts the .NET server + `tauri dev` (native window)
pnpm dev:web    # browser:  starts the .NET server + `ng serve` -> http://localhost:4200
pnpm server     # just the .NET server (http://127.0.0.1:5179)
```

The UI polls `Ping`/`/health`; the connection dot in the source bar shows server readiness.

## Test & checks

```bash
pnpm test       # Vitest unit/component tests (one-shot)
pnpm lint       # angular-eslint (flat config)
pnpm build      # production build
pnpm check      # lint + test + build (the local gate)
```

Live gRPC-Web smoke (drives the real server with the same client the app uses):

```bash
pnpm server     # in one terminal
node --experimental-strip-types scripts/grpcweb-smoke.mts   # in another
```

## Contract / codegen

The gRPC contract lives at `proto/devcontext/v1/devcontext.proto` (repo root) — the single source of
truth. It generates **C#** (server, via `Grpc.Tools` in `DevContext.Contracts`) and **TypeScript**
(this app, via buf):

```bash
pnpm gen:proto  # regenerate src/app/core/grpc/gen/** after editing the .proto
```

## Architecture (clear layering)

```
src/app/
  core/        transport + generated gRPC client (DEVCONTEXT_CLIENT), config
  data-access/ DevContextApi — typed wrapper over the gRPC client
  state/       signal stores: SessionStore, TraceStore, ConnectionStore
  models/      view models + proto -> view mappers
  ui/          dumb presentational components: Icon, GraphCanvas (Cytoscape)
  features/    smart components: source-bar, entries-panel, map-panel, trace-panel, node-detail, workspace
```

- **Analyze once, query many.** `Analyze` returns a session handle; Map/Trace/Node/Neighbors are
  cheap render-time queries over the same immutable snapshot — never a re-analysis.
- **Zoneless + signals** throughout; no `zone.js`.
- **Styling**: Tailwind CSS v4 design tokens (`src/styles.css`), dark-first. Icons via `lucide`.

## Server lifecycle

In development the server runs separately (`pnpm dev` orchestrates it). For packaged builds the Tauri
shell (`src-tauri/src/lib.rs`) spawns and kills the server when `DEVCONTEXT_SERVER_DLL` is set
(bundled self-contained sidecar — see P5 in the plan).

## M6 verification — drive the new UI surfaces

After `pnpm dev:web`, analyze the dogfood repo and verify:

### Home page (`/`)
- [ ] ServiceMapHero renders deterministically — gateway (YarpApiGateway) on left column, core services in center, bus/broker rail underneath
- [ ] Three tiles populate: Entries sparkbar (colored per-service bars), Wiring health (%, color-coded), Freshness ("Analyzed in Xs", stale/current chip)
- [ ] Onboarding row shows [Trace checkout] link (auto-detected from entries), [Open atlas], [Point your agent here]
- [ ] Top flows have service-colored chips per entry
- [ ] "Needs attention" section links to insights or explore with correct params

### Atlas page (`/atlas`)
- [ ] 6 sections render in order: service diagram → top flows (flow steppers) → event wiring board → per-service cards → cross-cutting → hub radar
- [ ] Flow steppers show horizontal strips with node count, depth, cross-service count
- [ ] Event wiring board table shows publisher/event/consumer rows
- [ ] Service cards show style badge + stack tags per service
- [ ] Cross-cutting shows pipeline behaviors + packages
- [ ] Hub radar rows click → navigate to Explore with node view
- [ ] Export one-pager button copies markdown to clipboard (shows "Copied!" for 2s)

### Bug check
- [ ] No browser console errors
- [ ] No missing-data indicators on empty states
- [ ] All router links work (click service card → Explore, click flow stepper → Explore, Hub radar → Explore)
- [ ] Identity strip still works (stale chip, re-analyze, confidence ledger collapsible)

## M7 planning — design tokens + explore lenses

### Key files to read before M7
1. `docs/dev/briefs/proposal-meridian.md` §M7
2. `docs/dev/briefs/meridian-agent-playbook.md` §UI-Explore, §UI-Chrome, §UI-Table lens
3. `src/styles.css` — current Tailwind v4 + @theme inline + CSS vars
4. `src/app/core/theme/theme.service.ts` — vibe switching, ThemePalette
5. `src/app/features/pages/workbench-page.ts` — current explore page with Esc-ladder
6. `src/app/features/explorer/stage.ts` — graph canvas (Cytoscape, topology/trace/neighbors)
7. `src/app/features/explorer/inspector.ts` — right-side inspector panel
8. `src/app/state/trail.store.ts` — breadcrumb/trail system

### M7 checkpoints (proposed order)
| # | What | Component(s) | Notes |
|---|------|-------------|-------|
| 7.0 | Design-token pass: min body 12px, icons 14–16px, per-kind color coding, contrast audit | `styles.css`, `theme.service.ts`, all templates | Sweeps all surfaces; kills the "squint" problem |
| 7.1 | Graph↔code binding: node select → inspector Code tab with full member; trace step → edge highlight + code line scroll; Esc ladder (selection→focus→altitude) | `workbench-page.ts`, `stage.ts`, `inspector.ts` | Code pane becomes first-class citizen |
| 7.2 | Lenses: Service/Layer/Feature; per-page default lens; lane sheet for layers | `workbench-page.ts`, `stage.ts`, new `lens-switcher.ts` | System canvas repeated everywhere is banned (A12) |
| 7.3 | Trail dedupe/group/cap; entry deck legibility | `trail.store.ts`, `entry-deck.ts` | Trail must not grow unbounded |
| 7.4 | Chrome pass + feedback affordances (VS Code bar test) | `workspace-shell.ts`, `activity-bar.ts`, omnibox | Bar height ≥40px, hit targets ≥28px, every action confirms visibly |
| 7.5 | Table lens v2 (virtualized, archetype-default columns, relationship chips, row expand) | new `features/table-lens/` | Data-dense, CSV export |

