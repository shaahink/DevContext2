# DevContext — .NET codebase context for humans and LLMs

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](global.json)
[![CI](https://github.com/shaahink/DevContext2/actions/workflows/ci.yml/badge.svg)](https://github.com/shaahink/DevContext2/actions/workflows/ci.yml)

**DevContext is the answer to "what IS this .NET solution?"** — point it at a folder, repo URL, `.sln`, or `.csproj`; it reads the code once, models it, and produces the most relevant context for whatever you're doing — sized for an LLM prompt, readable by a human, and honest about how it got there.

| | CLI | Desktop |
|---|-----|---------|
| **Platform** | Linux, macOS, Windows | Windows 10+ (build 19041+) |
| **Requires** | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | Nothing — self-contained `.exe` |
| **Download** | `dotnet tool install -g DevContext.Cli` | [GitHub Releases](https://github.com/shaahink/DevContext2/releases) |

## The 30-second demo

```bash
dotnet tool install -g DevContext.Cli
devcontext analyze . --focus DiscoveryPipeline:RunAsync --depth 3
```

```
## DevContext — Slice on DevContext2

**Architecture**: CleanArchitecture (100% confidence)
**Signals**: controllers · efcore · mediatr · minimal-apis
**Projects**: 4 — DevContext.Cli, DevContext.Core, DevContext.Desktop, DevContext.Roslyn
**Types**: 38 in output

## Endpoints
| Method | Route | Handler | Source |
|--------|-------|---------|--------|
| POST | /api/analyze | AnalyzeController.Analyze | AnalyzeController.cs:15 |
| GET  | /api/health  | HealthController.Check  | HealthController.cs:8  |
  [...]
analyzed 412 files · 38 types kept of 167 · 7,842/8,000 tokens · 1.9s stage2 ×3.1 stage3 ×2.4
```

## There are exactly two situations

1. **You don't know the repo** → `devcontext analyze .` produces an orientation map (architecture, endpoints, data model, DI wiring). No starting point exists — by definition — so DevContext shows you the whole picture.
2. **You know where you're standing** → `devcontext analyze . --focus TypeName:Method` slices from that point *down the wiring* (endpoint → handler → MediatR → entities → events). The **Depth** dial controls how far to follow.

That's the entire surface. No natural-language input, no query-box pretense — just Focus + Depth, with smart defaults and visual adjustment after.

## How it decides what to show

DevContext doesn't delete. Every type gets three normalized scores ∈ [0,1]:

| Score | Meaning |
|-------|---------|
| **RoleScore** | How load-bearing is this type? Endpoint (1.0) > MediatR handler (0.8) > DI registration (0.35) |
| **FocusScore** | How close to your focus point? Path distance + call-graph reachability via BFS |
| **FinalScore** | Weighted blend per mode: overview (0.7×Role + 0.3×Focus), slice (0.35×Role + 0.65×Focus) |

The highest-scoring types fill the token budget. Everything past the line is listed in the cut list: *"12 types pruned: …"*. You can turn the dial after analysis (`--max-tokens`, section checkboxes) and immediately see what enters and leaves — no re-analysis. Run `--stats` to see the full scoring funnel, extractor timing, cache hits, and parallel speedup.

→ [Full design philosophy](docs/DESIGN-PHILOSOPHY.md)

## What it extracts

| Detection | What it finds |
|-----------|---------------|
| **Endpoints** | Minimal API `Map*` calls, FastEndpoints, MVC controller actions |
| **MediatR handlers** | `IRequestHandler<T,Q>`, commands, queries, notifications |
| **Message consumers** | MassTransit `IConsumer<T>`, NServiceBus, in-memory `IEventHandler<T>` |
| **EF Core entities** | DbContext, DbSet properties, aggregate roots, key properties |
| **DI registrations** | `AddSingleton`/`AddScoped`/`AddTransient`, extension methods, factory delegates |
| **Background workers** | `IHostedService`, `BackgroundService`, Quartz jobs |
| **Middleware pipeline** | `Use*` calls in Program.cs, registration order |
| **Indirect wiring** | `Activator.CreateInstance`, service locator, reflection scanning |
| **Aspire resources** | `AddProject`, `AddRedis`, `AddPostgres`, `WithReference` |
| **Anti-patterns** | Fire-and-forget, `IServiceScopeFactory`, `new` outside DI, `CancellationToken.None` |
| **Event flow** | Publish/Subscribe pairs, handler implementations |
| **Architecture style** | Evidence-based: Microservices, CleanArchitecture, NLayer, MinimalApi, VerticalSlices |

## Quickstart

**CLI:**
```bash
dotnet tool install -g DevContext.Cli
devcontext analyze .                              # Overview map (no focus)
devcontext analyze . --focus OrderService         # Slice from a type
devcontext analyze . --focus "GET /api/orders"    # Slice from an endpoint route
devcontext analyze . --focus Foo:Bar --depth 3    # With explicit depth
devcontext analyze . --stats                      # Full nerd view (timing, funnel, cache)
devcontext analyze . --format json --strict       # JSON output with runReport
```

**Desktop:** Download `DevContext.Desktop.zip` from [Releases](https://github.com/shaahink/DevContext2/releases), unzip, run `DevContext.Desktop.exe`. Three tabs after analysis: Human (HTML), LLM (markdown), Stats (timing waterfall, extractor grid, token funnel, parallel speedup). Section checkboxes and token slider re-render instantly — no re-analysis.

## Honest roadmap

**What's solid today:**
- Full .NET static analysis — endpoints, entities, DI, handlers, events, anti-patterns
- Score-then-budget ranking with inspectable cut list
- Analyze-once-render-many (Plan 1): snapshot + lens, sub-100ms re-renders
- `--stats` everywhere + Stats tab on desktop
- Self-validating output (`--strict` mode, eval suite over real repos)

**Known limits (tracked as aspirational eval checks):**
- Architecture-style detection on hybrid repos (Aspire microservices, VerticalSlice) — currently biased toward per-service endpoint style rather than topology. [Issue](docs/DETECTION-GUIDE.md#5-architecture-style-detection-the-known-weak-spot)
- `<dynamic>` route placeholders in FastEndpoints — route resolution needs improvement
- AutoMapper library-mode type retention — scoring weights tuned for web apps, not pure libraries

**Deliberately deferred:**
- **Beyond .NET** — the pipeline is language-agnostic by design; TS/other languages are roadmap-only.
- **LLM-value benchmark** — an honest harness measuring "does an LLM answer codebase questions better with DevContext output than with a raw file dump?" stays out of the README until it exists.
- **Persistent snapshot cache** — serialize `AnalysisSnapshot` to disk keyed by content hash for instant re-open.

## Development

```bash
dotnet build DevContext.slnx
dotnet test                                      # 288 tests
$env:UPDATE_GOLDENS=1; dotnet test               # Regenerate goldens
```

## License

MIT
