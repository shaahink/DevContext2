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
