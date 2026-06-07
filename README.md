# DevContext — .NET Codebase Analysis for LLM Context

**DevContext** is a static analysis CLI tool that extracts structured context from .NET codebases for use with LLMs (Large Language Models). It analyzes your project's architecture, endpoints, dependencies, data models, and middleware pipeline — then prunes and compresses the output to fit a token budget.

```bash
# Quick start
dotnet tool install -g DevContext.Cli
devcontext . --scenario architecture --max-tokens 8000
```

---

## Why DevContext?

LLMs need focused, structured context to be useful for coding tasks. Pointing an LLM at an entire codebase wastes tokens on irrelevant details and buries what matters. DevContext solves this by:

- **Discovering** what's in your codebase (endpoints, handlers, entities, DI wiring, middleware)
- **Pruning** irrelevant types (test noise, unrelated code, framework internals)
- **Compressing** output to fit within a token budget
- **Scoping** by scenario (architecture overview, debug an endpoint, trace a message flow)

The tool is pure static analysis — no LLM calls, no runtime agents, no network access.

---

## Quick Start

```bash
# Install as a .NET global tool
dotnet tool install -g DevContext.Cli

# Run from any .NET project/solution directory
devcontext .

# Focus on a specific endpoint
devcontext . --scenario debug-endpoint --around CreateOrderHandler

# Save to file
devcontext . --scenario architecture --format markdown -o output.md

# See what extractors would run
devcontext . --dry-run
```

---

## Scenarios

| Scenario | What it shows | Best for |
|---|---|---|
| `architecture` | Projects, signals, endpoints, handlers, entities, wiring, related types | New team members, codebase overview, PR context |
| `debug-endpoint` | Focused view of one endpoint + its handler chain, dependencies, entities, call graph | Debugging a failing endpoint, understanding a handler |
| `add-similar-feature` | Existing endpoint patterns + handler structure + related types | Copying an existing pattern for a new feature |
| `modify-middleware` | Middleware pipeline, DI registrations, indirect wiring | Adding/modifying middleware, understanding pipeline |
| `trace-message-flow` | MediatR handlers, message consumers, event bus, data model | Tracing events through the system |
| `harden-di` | Indirect wiring, reflection, service locator, DI registrations | Security audit, DI hardening |

---

## Profiles

| Profile | Details |
|---|---|
| `quick` | Fast scan, minimal output (~2000 tokens) |
| `focused` | Balanced extraction with pruning (default) |
| `debug` | Adds call graph extraction (BFS from entry points) |
| `full` | Full analysis with source body extraction |

---

## Command Reference

```bash
devcontext [PATH] [OPTIONS]

Arguments:
  [PATH]                  Root path (.sln, .csproj, or directory)

Options:
  -s, --scenario <NAME>   architecture | debug-endpoint | add-similar-feature |
                          modify-middleware | trace-message-flow | harden-di
  -p, --profile <NAME>    quick | focused | debug | full
  -a, --around <PATH>     Focus on a specific type/method (e.g. OrdersController:CreateOrder)
  -t, --task <TEXT>       Free-text intent → auto-selects scenario + profile
      --max-tokens <N>    Token budget (default 8000)
  -o, --output <FILE>     Write to file (default stdout)
      --format <FMT>      markdown (default) | json
      --include-provenance  Show why each type was included
      --include-diagnostics Show pruning notes and warnings
      --no-roslyn         Disable Roslyn workspace loading
      --metrics           Show extraction performance metrics
      --dry-run           Plan extractors without running
      --verbose           Info-level logging
      --trace             Debug-level logging

Commands:
  devcontext init          Create devcontext.json in current directory
  devcontext scenarios     List all scenarios with descriptions
  devcontext dry-run [PATH] Plan-only mode (alias for --dry-run)
  devcontext version       Show version + commit hash
```

---

## Signals (Detected Architecture Features)

DevContext detects 30+ architecture signals from NuGet packages and code patterns:

| Signal | Detected From |
|---|---|
| `minimal-apis` | `Microsoft.NET.Sdk.Web`, `Microsoft.AspNetCore.OpenApi` |
| `controllers` | Base type inheritance from `ControllerBase` |
| `fast-endpoints` | `FastEndpoints` NuGet package |
| `mediatr` | `MediatR` package |
| `efcore` | `Microsoft.EntityFrameworkCore` and providers |
| `fluentvalidation` | `FluentValidation` package |
| `serilog` | `Serilog` package |
| `automapper` | `AutoMapper` package or project reference |
| `polly` | `Polly` package |
| `swagger` | `Swashbuckle.AspNetCore` package |
| `masstransit` | `MassTransit` package |
| `aspire` | `Microsoft.Aspire` package |
| And 20+ more | NLog, Quartz, Redis, HealthChecks, Hangfire, etc. |

Each signal gates scenario-specific extractors — no point scanning for MediatR handlers if the codebase doesn't use MediatR.

---

## Output Sections

| Section | Content | Example |
|---|---|---|
| Header | Scenario name, architecture style, signals, project count, token budget | `MinimalApi (100%) · controllers · mediatr · efcore` |
| Architecture overview | Project list by name | `- Catalog.API / - Ordering.API / - WebApp` |
| Entry points | Inline type definition for `--around` focus | Constructor deps, implemented interfaces, public methods |
| Endpoints | Per-project grouped HTTP endpoints with routes, handler chains, auth | `POST /api/orders → CreateOrderCommand` |
| Call graph | BFS call tree from focused handler (debug profile) | `CreateOrderHandler.Handle ├─ _repo.Add └─ _eventService.Publish` |
| MediatR Handlers | Command/query handlers with request/response types | `CreateOrderCommand → bool → CreateOrderCommandHandler` |
| Data model (EF Core) | Per-DbContext entities, aggregate roots, key properties | `OrderingContext: Order, OrderItem, Buyer` |
| Message consumers | Event bus subscribers | `MassTransit · OrderCreated · EmailConsumer` |
| Middleware pipeline | Deduplicated middleware with source and count | `UseAuthorization ×3 (Program.cs)` |
| DI registrations | Compact service registrations with source attribution | `Scoped · IOrderRepository · OrderRepository · Extensions.cs:16` |
| Related types | Surviving types grouped by architecture layer | `Api: CreateOrderHandler, Api: OrderController` |

---

## Architecture

```
CLI (Spectre.Console)
  → ProjectRootResolver: finds .sln / walk-up / folder mode
  → DiscoveryPipeline:
      Stage 1 — File tree, solution, project structure
      Stage 2 — Generic extractors (parallel): dependencies, syntax, DI, middleware
      [Signals sealed]
      Stage 3 — Specific extractors (sequential): endpoints, MediatR, EF Core, etc.
      Stage 4 — Pruning: path proximity → call reachability → pattern relevance → token budget
      Stage 5 — Compression: trivial member → boilerplate → deduplication → namespace grouping
      Stage 6 — Render: markdown or JSON
```

Three projects:
- **`DevContext.Core`** — Contracts, pipeline, extractors, pruning, compression, rendering. Zero Spectre/Serilog dependencies.
- **`DevContext.Roslyn`** — Roslyn workspace integration (loaded on demand for deep analysis).
- **`DevContext.Cli`** — Composition root. Spectre.Console CLI, DI wiring, Serilog logging.

---

## Configuration

Create `devcontext.json` in your project root for persistent settings:

```json
{
  "$schema": "https://devcontext.dev/schemas/v2/config.json",
  "defaultProfile": "focused",
  "defaultScenario": "debug-endpoint",
  "maxOutputTokens": 6000,
  "excludePatterns": [".git", "bin", "obj", "Migrations"],
  "entryPaths": ["src/Api"],
  "profiles": {
    "quick": { "profile": "quick", "maxOutputTokens": 2000, "noRoslyn": true }
  }
}
```

---

## Development

```bash
# Build
dotnet build

# Test (144+ tests)
dotnet test tests/DevContext.Core.Tests

# Run against a benchmark repo
dotnet run --project src/DevContext.Cli -- analyze eval-repos/TodoApi --scenario architecture

# Format
dotnet format --verify-no-changes
```

### Project structure

```
src/DevContext.Core/       # Core library (~73 files)
  Contracts/               # All interfaces (IDiscoveryExtractor, IObserver, etc.)
  Extractors/              # Generic (always run) + Specific (signal-gated)
  Models/                  # Data model (signals, detections, types, scenarios)
  Pipeline/                # DiscoveryPipeline orchestrator
  Pruning/                 # 4 pruning strategies
  Compression/             # 6 compression strategies
  Rendering/               # Markdown + JSON renderers
  Observers/               # Metrics, recording, composite observer
  Resolvers/               # Focus point parser, project root resolver
  Constants/               # HttpConstants, SectionNames

src/DevContext.Roslyn/     # Roslyn workspace integration
src/DevContext.Cli/        # CLI commands, Spectre.Console observer

tests/DevContext.Core.Tests/  # ~144 tests across 30+ test files
eval-repos/                   # Cloned benchmark repos (gitignored)
docs/                         # ADRs, iteration docs, benchmark reports
```

---

## License

MIT
