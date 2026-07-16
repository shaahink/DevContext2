# DevContext — .NET codebase context for humans and LLMs

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](global.json)
[![CI](https://github.com/shaahink/DevContext2/actions/workflows/ci.yml/badge.svg)](https://github.com/shaahink/DevContext2/actions/workflows/ci.yml)
[![Angular](https://img.shields.io/badge/Angular-22-DD0031?logo=angular)](src/DevContext.App)
[![Tauri](https://img.shields.io/badge/Tauri-2-FFC131?logo=tauri)](src/DevContext.App/src-tauri)

> **Point it at any .NET repo and it gives you a Map (what's here) and a Trace (how things connect) — sized for an LLM prompt, readable by a human, and honest about how it got there.**

<p align="center">
  <a href="docs/screenshots/01-home.png">
    <img src="docs/screenshots/01-home.png" alt="DevContext Home — After analyzing eShop microservices" width="85%">
  </a>
  <br>
  <em>Home page after analyzing a 7-service eShop microservices solution — identity strip, service map, topology tiles, and onboarding</em>
</p>

## One engine, four surfaces

Everything is powered by a single analysis engine (`DevContext.Core`): analyze a solution once, then
query it from whichever surface fits your workflow.

| Surface | What it's for | Get it |
|---------|---------------|--------|
| **CLI** (`devcontext`) | Scriptable Map/Trace in your terminal; JSON output for pipelines | `dotnet tool install -g DevContext.Cli` (Linux/macOS/Windows, needs [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)) |
| **Desktop app** | Interactive exploration: graph, table lens, insights, Context Studio | Windows installer from [Releases](https://github.com/shaahink/DevContext2/releases) (needs the [.NET 10 runtime](https://dotnet.microsoft.com/download/dotnet/10.0)), or build from source — see [Quickstart](#quickstart) |
| **MCP server** (24 tools) | Let AI agents (Claude Code, Cursor, VS Code, …) query your codebase | Build + register — see [docs/product/mcp-reference.md](docs/product/mcp-reference.md) |
| **gRPC server** | Analyze-once, query-many backend that powers the app and MCP | Started automatically by the app/MCP; standalone via `dotnet run --project src/DevContext.Server` |

---

## What is DevContext?

DevContext turns any .NET solution into a **structured, queryable code graph** — not just syntax trees, but semantic understanding of architecture, wiring, entry points, data flow, and dependency injection. Use it to:

- **Onboard to an unfamiliar repo** in under a minute
- **Feed precise context to an LLM** — no token waste, no hallucinated code
- **Trace request flows** end-to-end, from HTTP endpoint through handlers, events, and database
- **Export LLM-ready context packs** with scope picker, token budget, and intent presets

<p align="center">
  <a href="docs/screenshots/02-atlas.png"><img src="docs/screenshots/02-atlas.png" alt="Atlas" width="45%"></a>
  <a href="docs/screenshots/03-explore.png"><img src="docs/screenshots/03-explore.png" alt="Explore" width="45%"></a>
  <br>
  <em>Atlas one-pager (left) and Explore workbench with entry deck + trace (right)</em>
</p>

---

## Features

### 🗺 Map & Trace Engine

Analyze any .NET solution once, then query it a hundred ways. The Map shows **what's in the repo** (architecture style, project topology, entry points, packages). The Trace walks **how things connect** — from an endpoint through MediatR handlers, MassTransit consumers, EF Core entities, and DI wiring.

<p align="center">
  <a href="docs/screenshots/04-graph.png"><img src="docs/screenshots/04-graph.png" alt="Graph view" width="45%"></a>
  <a href="docs/screenshots/05-code-inspector.png"><img src="docs/screenshots/05-code-inspector.png" alt="Code Inspector" width="45%"></a>
  <br>
  <em>Interactive graph visualization (left) and source inspector with PrismJS syntax highlighting (right)</em>
</p>

### 🔬 Explore Workbench

The Explore view is your central workbench: an **entry deck** showing all entry points (HTTP endpoints, bus consumers, background services), an interactive **trace/inspector** panel, and an **interactive graph** that renders the call graph as a Cytoscape dagre layout. Switch between Service, Layer, Feature, and Flow lenses to understand the codebase from different angles.

### 📊 Table Lens & Insights

The **Table Lens** gives you a CDK-virtualized spreadsheet of every entry point with archetype columns, filterable and sortable. Export to CSV. The **Insights** page surfaces architecture findings grouped by severity — dependency violations, wiring gaps, pattern deviations.

<p align="center">
  <a href="docs/screenshots/06-table-lens.png"><img src="docs/screenshots/06-table-lens.png" alt="Table Lens" width="45%"></a>
  <a href="docs/screenshots/07-insights.png"><img src="docs/screenshots/07-insights.png" alt="Insights" width="45%"></a>
  <br>
  <em>Table lens — CDK-virtualized entry audit (left) and architecture insights (right)</em>
</p>

### 🧠 Context Studio

Assemble LLM-ready context packs with the **Context Studio** — a three-pane tool:
- **Scope picker** (left): browse services → entries as a tree, search with omnibox, use presets like *"I'm changing this endpoint"*
- **Composition view** (center): ordered cards for flows, signatures, bodies, DI wiring, config, entities, contracts, tests — drag to reorder, toggle body on/off
- **Budget panel** (right): token budget slider with per-card meter, intent selector (trace / explain / review), format (markdown / plain), Copy / Save with toast feedback

Every pack opens with an identity header (repo, archetype, analyzed-at, git HEAD) and carries
**per-section provenance** — which files each section came from and how it was resolved.

<p align="center">
  <a href="docs/screenshots/08-context-studio.png"><img src="docs/screenshots/08-context-studio.png" alt="Context Studio" width="45%"></a>
  <a href="docs/screenshots/09-export.png"><img src="docs/screenshots/09-export.png" alt="Export" width="45%"></a>
  <br>
  <em>Context Studio with scope picker + composition (left) and export/copy with token budget (right)</em>
</p>

### 🔌 MCP Integration

DevContext ships a built-in **MCP server** exposing **24 tools** for AI agent integration — from `overview` and `trace` to budget-priced `get_context` packs and `verify_context` staleness checks. The desktop UI provides a full MCP management page — status card, configuration snippets, sessions table, live log feed, and a "Try a tool" sandbox. Setup + full tool catalog: [docs/product/mcp-reference.md](docs/product/mcp-reference.md).

<p align="center">
  <a href="docs/screenshots/10-mcp.png"><img src="docs/screenshots/10-mcp.png" alt="MCP" width="85%"></a>
  <br>
  <em>MCP management page — status, config, sessions, live feed, try-a-tool sandbox</em>
</p>

---

## What it detects

| Detection | Finds |
|-----------|-------|
| **Endpoints** | Minimal API, FastEndpoints, MVC controller actions |
| **MediatR handlers** | `IRequestHandler<T,Q>`, commands, queries, notifications |
| **Message consumers** | MassTransit `IConsumer<T>`, NServiceBus, in-memory handlers |
| **EF Core entities** | DbContext, DbSet, `OnModelCreating` config, aggregate roots |
| **DI registrations** | `AddSingleton`/`AddScoped`/`AddTransient`, factory delegates |
| **Background workers** | `IHostedService`, `BackgroundService`, Quartz jobs |
| **Middleware pipeline** | `Use*` calls in Program.cs, registration order |
| **Cross-service links** | Bus-publish (MassTransit/RabbitMQ), gRPC, HTTP between services |
| **Architecture style** | Evidence-driven: Microservices, CleanArchitecture, NLayer, MinimalApi, etc. |
| **Call graph** | Roslyn syntactic call edges between solution types |

---

## How the trace engine works

The trace is a **structural traversal** over a typed CodeGraph — nodes and edges built at analyze time by joining detections into a connected graph. Each edge carries provenance (`file:line`), resolution (`Join`/`Syntactic`/`Semantic`), and confidence.

| Edge | Priority | Built from |
|------|----------|-----------|
| **Sends** | 0 (highest) | `.Send(new XCommand())` / `.Publish(new XEvent())` |
| **Handles** | 1 | MediatR handler join |
| **Raises** | 2 | `AddDomainEvent(new XEvent())` |
| **Consumes** | 3 | Event → handler join |
| **ReadsWrites** | 4 | EF entity + body reference |
| **Resolves** | 5 | DI registration + single-implementor |
| **WrappedBy** | 6 | `IPipelineBehavior` |
| **Calls** | 7 (lowest) | Roslyn call graph |

Depth limit (default 6), fan-out cap (12), framework boundary detection, revisit guard, and cycle breaking keep traces focused. [Full design →](docs/product/TRACE-ENGINE-DESIGN.md)

---

## Quickstart

### CLI

```bash
dotnet tool install -g DevContext.Cli
devcontext analyze .                              # Map (architecture overview)
devcontext analyze . --focus OrderService          # Trace from a type
devcontext analyze . --focus "GET /api/orders"     # Trace from an endpoint
devcontext analyze . --depth 6 --detail salient    # Full trace with source context
devcontext analyze . --stats                       # Timing, funnel, cache
devcontext analyze . --format json --strict        # JSON with validation
```

Full flag reference: [docs/product/cli-reference.md](docs/product/cli-reference.md) · configuration: [docs/product/configuration.md](docs/product/configuration.md)

### Desktop app

**Install (Windows):** download the installer (`DevContext_*_x64-setup.exe`) from [Releases](https://github.com/shaahink/DevContext2/releases) — it bundles the analysis server as a sidecar (installers attach to releases starting with the next tag). The app spawns the server via `dotnet`, so the [.NET 10 runtime](https://dotnet.microsoft.com/download/dotnet/10.0) must be installed.

**Or build from source** — requires the .NET 10 SDK, Node 22 + [pnpm](https://pnpm.io), and (for the native window) the [Tauri 2 prerequisites](https://v2.tauri.app/start/prerequisites/) on Windows:

```bash
dotnet build DevContext.slnx
cd src/DevContext.App
pnpm install
pnpm dev:web        # browser mode at http://localhost:4200 (server auto-started)
pnpm dev            # or: native Tauri window
pnpm tauri build    # or: build the installer yourself (bundles the server sidecar)
```

UI guide: [docs/product/desktop-ui.md](docs/product/desktop-ui.md)

### MCP (AI agents)

```bash
dotnet build DevContext.slnx
```

then register the built server with your MCP client:

```json
{
  "mcpServers": {
    "devcontext": {
      "command": "C:/path/to/DevContext2/src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe"
    }
  }
}
```

The MCP server auto-spawns the gRPC backend. All 24 tools, session model, and per-client snippets: [docs/product/mcp-reference.md](docs/product/mcp-reference.md)

---

## Documentation

| Doc | What's in it |
|-----|--------------|
| [docs/product/cli-reference.md](docs/product/cli-reference.md) | Every `devcontext analyze` flag, verified against source |
| [docs/product/configuration.md](docs/product/configuration.md) | `devcontext.json` schema and precedence |
| [docs/product/mcp-reference.md](docs/product/mcp-reference.md) | MCP setup + all 24 tools |
| [docs/product/desktop-ui.md](docs/product/desktop-ui.md) | Desktop app tour, page by page |
| [docs/product/TRACE-ENGINE-DESIGN.md](docs/product/TRACE-ENGINE-DESIGN.md) | Trace engine internals: edges, priorities, caps |
| [docs/product/DETECTION-GUIDE.md](docs/product/DETECTION-GUIDE.md) | What each detector finds and its provenance |
| [docs/product/examples/](docs/product/examples/) | Real Map/Trace output on eShop, DntSite, and more |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Build, test, gate battery, conventions |

---

## AI Agent Context

Designed for coding agents (Claude Code, Copilot, Cursor, Cline, etc.):

| File | For |
|------|-----|
| `AGENTS.md` | **Read this first** — cold-start order, architecture, gate battery, worktree discipline |
| `docs/dev/DEVELOPER-PIPELINE.md` | The full developer pipeline: build, test, run, bench, eval |
| `docs/product/AGENT-REFERENCE.md` | Engine internals: ANALYZE→RENDER pipeline, Graph2, contracts |
| `docs/dev/CODE-MAP.md` | Source-verified module map — "where do I change X?" |
| `proto/devcontext/v1/devcontext.proto` | gRPC contract — single source of truth for server ⇄ app ⇄ MCP |

### Architecture

```
DevContext.Core (kernel)   —  analysis pipeline, Graph2 identity spine, BodyFacts, renderers
├── DevContext.Cli          —  `devcontext` dotnet tool
├── DevContext.Contracts    —  proto → C# gRPC codegen
├── DevContext.Server       —  gRPC-Web backend (analyze once, query many)
├── DevContext.Mcp          —  MCP server (24 tools, stdio → gRPC proxy)
DevContext.App (Angular 22) —  Tauri 2 desktop, zoneless, signals — talks gRPC-Web to Server
```

### Gate battery (green before every commit)

```powershell
dotnet build DevContext.slnx                              # 0 warnings / 0 errors
dotnet test DevContext.slnx --filter "Category!=Eval"     # fast unit + integration
dotnet test DevContext.slnx --filter "Category=Truth"     # truth gates
powershell -File scripts/loom-guards.ps1                  # banned patterns + truth gate
cd src/DevContext.App; pnpm check                          # app: lint + test + build
```

---

## Development

```bash
dotnet build DevContext.slnx
dotnet test DevContext.slnx --filter "Category!=Eval"

# Desktop (browser mode):
cd src/DevContext.App
pnpm dev:web                              # Angular @ :4200 + .NET server

# Or with background services (AI agent friendly):
powershell -File scripts/start-dev-bg.ps1
node --experimental-strip-types scripts/capture-readme.mts --no-spawn
powershell -File scripts/start-dev-bg.ps1 -Kill
```

---

## License

MIT

---

<p align="center">
  <a href="docs/screenshots/12-home-full.png"><img src="docs/screenshots/12-home-full.png" alt="DevContext — Start" width="50%"></a>
  <br>
  <em>DevContext at rest — ready to analyze</em>
</p>
