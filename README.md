# DevContext — .NET Codebase Analysis for LLM Context

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](global.json)
[![Tests](https://img.shields.io/badge/tests-221%20passing-brightgreen)](tests/)

**DevContext** is a static analysis CLI and desktop tool that extracts structured context from .NET codebases for use with LLMs. It discovers endpoints, background workers, EF Core entities, DI registrations, middleware pipelines, call graphs, and anti-patterns — then prunes and compresses the output to fit a token budget.

```bash
# Quick start
dotnet tool install -g DevContext.Cli
devcontext . --scenario overview --max-tokens 8000
```

## Table of Contents

- [Features](#features)
- [Why DevContext?](#why-devcontext)
- [Before vs After](#before-vs-after)
- [Quick Start](#quick-start)
- [Desktop UI](#desktop-ui)
- [CLI Reference](docs/cli-reference.md)
- [Example Scenarios](#example-scenarios)
- [Output Sections](#output-sections)
- [Configuration](#configuration)
- [Architecture](#architecture)
- [Development](#development)
- [License](#license)

---

## Features

**UI simplification** — two clear modes and explicit section checkboxes:

| Concept | How it works |
|---------|-------------|
| 2 modes | Overview (whole-codebase) or Trace (entry-point focused) |
| 9 section checkboxes | Check what you want — profile derived automatically |
| Profile auto-derived | Call graph checked → Debug, Source code checked → Full, neither → Focused |
| `--scenario audit` | Deprecated — maps to Overview with a warning |
| `--task` field | Free-text intent (`"trace the order handler"`) auto-selects mode/profile |

**Extractor capabilities** — 70 endpoints, 24 background workers, 83 DI registrations, correct architecture classification on real-world codebases.

---

## Why DevContext?

LLMs need focused, structured context to be useful for coding tasks. Pointing an LLM at an entire codebase wastes tokens on irrelevant details and buries what matters. DevContext solves this by:

- **Discovering** what's in your codebase (endpoints, handlers, entities, DI wiring, background workers, middleware)
- **Pruning** irrelevant types (test noise, unrelated code, framework internals)
- **Compressing** output to fit within a token budget
- **Scoping** by entry point (`--around`) or natural-language intent (`--task`)

The tool is pure static analysis — no LLM calls, no runtime agents, no network access.

---

## Before vs After

**Before**: You dump source files into an LLM. It gets 50,000 tokens of noise. You spend half your budget on `using` directives and closing braces.

**After**: DevContext produces a structured, pruned, 8,000-token overview that an LLM can immediately act on:

```markdown
## Endpoints (70 found)

| Method | Route | Handler | Source |
|--------|-------|---------|--------|
| GET | /Feed | FeedController.Index | FeedController.cs:15 |
| GET | /blog/rss.xml | FeedController.SiteFeed | FeedController.cs:92 |
| GET | /rss.xml | FeedController.SiteFeed | FeedController.cs:92 |
| ... | ... | ... | ... |
| GET | /Exports/{type}/{name}.pdf | ExportsController.Get | ExportsController.cs:13 |

## Background workers (24 found)

DotNetVersionCheckJob · BackupDatabaseJob · DailyNewsletterJob
FullTextSearchWriterJob · ThumbnailsServiceJob · DraftsJob ...

## DI registrations

| Bulk | AutoInjectAllServices | [bulk auto-registration] | ServicesRegistry.cs:23 |
| Singleton | IXmlRepository → DataProtectionKeyService | DataProtectionConfig.cs:18 |

## Anti-patterns detected
| Severity | Pattern | Description | Source |
|---|---|---|---|
| high | ServiceLocator | IServiceScopeFactory.CreateScope() | BacktestOrchestrator.cs:117 |
```

**Architecture**: ControllerBased (80%) · **Signals**: controllers · efcore · **Projects**: 3  
**24 background jobs** · **70 endpoints** · **83 DI registrations** · all in ~6,000 tokens

---

## Quick Start

```bash
# Install as a .NET global tool
dotnet tool install -g DevContext.Cli

# Run from any .NET project/solution directory (Overview mode)
devcontext .

# Trace a specific entry point with natural language intent
devcontext . --task "trace the order submission handler"

# Focus on a specific type or method
devcontext . --around FeedController

# Save to file
devcontext . --scenario overview --format markdown -o output.md

# See extractor timing (debug profile for call graph)
devcontext . --scenario trace --profile debug --around FeedController:Posts

# Plan only — see what extractors would run
devcontext . --dry-run
```

---

## Desktop UI

DevContext includes a cross-platform desktop app (Blazor Hybrid on WPF) for interactive analysis:

```
┌─────────────────────────────────────────────────────────────┐
│  Source  [path/to/project or github.com/user/repo]           │
│                                                             │
│  Intent  [trace the order submission handler]  (optional)    │
│                                                             │
│  Mode    [ Overview ]  [ Trace ]                             │
│                                                             │
│  Entry point  [FeedController]        (Trace mode)           │
│                                                             │
│  Sections                                                    │
│    ☑ Architecture overview     ☐ Call graph (+Roslyn)        │
│    ☑ Endpoints                 ☐ Message consumers          │
│    ☑ MediatR Handlers          ☐ Source code (+tokens)      │
│    ☑ Data model                ...                          │
│    ☑ DI / Wiring                                            │
│                                                             │
│  Token budget  [━━━━━━━━━━○━━━━] 8000                        │
│                                                             │
│  Symbol focus  [Namespace.Class:Method]   (Overview mode)    │
│                                                             │
│  Output  [ Markdown ] [ JSON ]     ▸ Advanced                │
│                                                             │
│  [         Clone & Analyze         ]                         │
└─────────────────────────────────────────────────────────────┘
```

See the [Desktop UI guide](docs/desktop-ui.md) for full details.

---

## Example Scenarios

| Example | Mode | Entry point | What it shows |
|---------|------|-------------|---------------|
| [Architecture overview](docs/examples/architecture.md) | Overview | — | Project tree, 70 endpoints, entities, DI wiring, middleware |
| [Trace an endpoint](docs/examples/trace.md) | Trace | `FeedController:SiteFeed` | Call graph, entry point details, source code, anti-patterns |
| [DI hardening audit](docs/examples/harden-di.md) | Overview + Debug | — | Service locators, reflection activation, manual wiring detection |
| [Intent inference](docs/examples/intent.md) | Auto | — | How `--task` auto-selects mode and profile from natural language |

---

## Output Sections

| Section | Content | Gated by |
|---------|---------|----------|
| Header | Architecture style, signals, projects, token budget | Always |
| Architecture overview | Project dependency tree (ASCII) | `☑ Architecture overview` |
| Entry points | Inline type definition for `--around` focus | When focus point provided |
| Endpoints | Per-project HTTP endpoint table with routes, auth, source locations | `☑ Endpoints` |
| Call graph | BFS call tree from entry points | `☑ Call graph` (+ profile: debug) |
| MediatR Handlers | Command/query handlers with request/response types | `☑ MediatR Handlers` |
| Data model (EF Core) | Per-DbContext entities, aggregate roots, migrations summary | `☑ Data model` |
| Message consumers | Event bus / in-memory event consumers | `☑ Message consumers` |
| Non-obvious wiring | Indirect wiring, middleware pipeline, DI registrations, background workers | `☑ DI / Wiring` |
| Anti-patterns | FireAndForget, ServiceLocator, CaptiveDependencies, etc. | When detected |
| Source code | Full type declarations for entry point + call chain | `☑ Source code` (+ profile: full) |
| Related types | Surviving types grouped by layer | `☑ Related types` |
| Diagnostics | Pruning notes, warnings, pipeline events | `--include-diagnostics` |

---

## Configuration

Create `devcontext.json` in your project root for persistent settings:

```json
{
  "$schema": "https://devcontext.dev/schemas/v2/config.json",
  "defaultScenario": "overview",
  "maxOutputTokens": 6000,
  "excludePatterns": [".git", "bin", "obj", "Migrations"],
  "entryPaths": ["src/Api"]
}
```

See the [configuration guide](docs/configuration.md) for all available options.

---

## Architecture

```
CLI (Spectre.Console) / Desktop (Blazor Hybrid + WPF)
  → ProjectRootResolver: finds .sln / walk-up / folder mode
  → DiscoveryPipeline:
      Stage 1 — File tree, solution, project structure
      Stage 2 — Generic extractors (parallel): dependencies, syntax, DI, middleware
      [Signals sealed — controllers, efcore, mediatr, etc.]
      Stage 3 — Specific extractors (sequential): endpoints, controllers, EF Core, call graph, etc.
      Stage 4 — Pruning: path proximity → call reachability → pattern relevance → token budget
      Stage 5 — Compression: trivial member → boilerplate → deduplication → namespace grouping
      Stage 6 — Render: markdown or JSON
```

Four projects:
- **`DevContext.Core`** — Contracts, pipeline, extractors, pruning, compression, rendering. Zero UI dependencies.
- **`DevContext.Roslyn`** — Roslyn workspace integration (loaded on demand for deep analysis).
- **`DevContext.Cli`** — CLI tool. Spectre.Console + Serilog. `dotnet tool install -g DevContext.Cli`.
- **`DevContext.Desktop`** — Desktop app. Blazor Hybrid on WPF. In-process engine for real cancellation and progress.

---

## Development

```bash
# Build
dotnet build

# Test (221 tests)
dotnet test tests/DevContext.Core.Tests
dotnet test tests/DevContext.Desktop.Tests

# Run CLI against a local project
dotnet run --project src/DevContext.Cli -- analyze C:\path\to\project --scenario overview

# Run the desktop app
dotnet run --project src/DevContext.Desktop

# Regenerate golden test files after output format changes
$env:UPDATE_GOLDENS=1; dotnet test tests/DevContext.Core.Tests
```

### Project structure

```
src/DevContext.Core/       # Core library
  Contracts/               # All interfaces (IDiscoveryExtractor, IObserver, etc.)
  Extractors/              # Generic (always run) + Specific (signal-gated, 20 total)
  Models/                  # Data model (signals, detections, types, scenarios)
  Pipeline/                # DiscoveryPipeline orchestrator
  Pruning/                 # 4 pruning strategies
  Compression/             # 6 compression strategies
  Rendering/               # Markdown + JSON renderers
  Resolvers/               # Focus point parser, project root resolver

src/DevContext.Roslyn/     # Roslyn workspace integration
src/DevContext.Cli/        # CLI commands, Spectre.Console observer
src/DevContext.Desktop/    # Blazor Hybrid desktop app

tests/DevContext.Core.Tests/     # 157 unit + golden tests
tests/DevContext.Desktop.Tests/  # 64 ViewModel tests
docs/                            # ADRs, configuration, examples, benchmarks
```

---

## License

MIT
