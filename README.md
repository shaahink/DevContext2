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
MAP  DevContext2     (4 projects)

STACK  Minimal APIs · MediatR (CQRS) · EF Core
STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Api, Application, Domain, Infrastructure;  ...
TOPOLOGY (depends-on)
   DevContext.Cli ── DevContext.Core, DevContext.Roslyn
   DevContext.Core ── DevContext.Roslyn
   DevContext.Desktop ── DevContext.Core
   DevContext.Roslyn

ENTRY POINTS
   HTTP (4)
      GET /api/health  (src/DevContext.Cli/HealthController.cs:15)
      POST /api/analyze  (src/DevContext.Cli/AnalyzeController.cs:22)
   Bus (2)
      OrderCreatedConsumer
      ...
→ drill in:  --focus "POST /api/analyze" or --focus <TypeName>
```

## There are exactly two situations

1. **You don't know the repo** → `devcontext analyze .` produces a **Map** (architecture style, tech stack, project topology, entry points, NuGet packages, aggregates). No starting point exists — by definition — so DevContext shows you the whole picture.
2. **You know where you're standing** → `devcontext analyze . --focus TypeName:Method` produces a **Trace** — a call-stack tree from that entry point *down the wiring* (endpoint → send → handler → entities → events). The **Depth** dial (`--depth 3`) controls how far to follow.

That's the entire surface. No natural-language input, no query-box pretense — just Focus + Depth, with smart defaults and visual adjustment after.

## The trace engine — how it decides

The trace is not a scored, ranked dump. It is a **structural traversal** over a CodeGraph — a typed node/edge model built at analyze time by joining detections (endpoints, MediatR handlers, EF entities, DI registrations, call edges) into a connected graph.

### Entry — where the trace starts

| Entry kind | Source detection | Example |
|---|---|---|
| `HttpEndpoint` | `EndpointDetection` | `POST /api/orders` |
| `MessageConsumer` | `MessageConsumerDetection` | `OrderCreatedConsumer` |
| `HostedService` | `BackgroundWorkerDetection` | `ScheduledJobWorker` |
| `ScheduledJob` | `BackgroundWorkerDetection` (timed) | Quartz job |
| `DomainEventHandler` | `MediatRHandlerDetection` (Notification) | `OrderShippedHandler` |
| `PublicApi` | `--focus` free-text match against Type/Handler/Service nodes | `OrdersController` |

### Traversal — how the trace walks the wiring

The trace visits nodes by following **typed edges** from the entry point forward, depth-first. Each edge carries a provenance (`file:line`), a resolution (`Join`/`Syntactic`/`Semantic`), and a confidence (`0..1`).

| Edge | Direction | Built from | Priority |
|---|---|---|---|
| **Sends** | caller → Request | `.Send(new XCommand())` / `.Publish(new XEvent())` body scan | 0 (highest) |
| **Handles** | Request → Handler | `MediatRHandlerDetection` join | 1 |
| **Raises** | handler → Event | `AddDomainEvent(new XEvent())` body scan | 2 |
| **Consumes** | Event → Handler | Notification `MediatRHandlerDetection` + `MessageConsumerDetection` join | 3 |
| **ReadsWrites** | handler → Entity/DataStore | `EfEntityDetection` + body reference scan | 4 |
| **Resolves** | interface → impl | `DiRegistrationDetection` + single-implementor fallback | 5 |
| **WrappedBy** | Request → pipeline behavior | `IPipelineBehavior` DI registration | 6 |
| **Calls** | type → type | Roslyn call graph (syntactic or semantic) | 7 (lowest) |

### Exit rules — what stops the trace

| Rule | Condition |
|---|---|
| **Depth limit** | `depth >= options.MaxDepth` (default 6) |
| **Fan-out cap** | `children >= options.MaxFanOut` (default 12) — excess marked `Truncated` |
| **Revisit guard** | Node already visited — cycle break, marked `Truncated` if followable edges remain |
| **Framework boundary** | Node title starts with `Microsoft.`, `System.`, or is `DbContext`, `ILogger`, `IMediator`, `ISender`, `IPublisher`, or contains `Mediator` |
| **No followable edges** | Node has zero out-edges of the followed kinds |

### Filters — what's excluded from the graph

| Filter | Rule |
|---|---|
| **Test projects** | Project name ends with `Tests`, `Test`, `Specs`, `IntegrationTests`, `FunctionalTests`; or references xUnit/NUnit/MSTest/Moq packages |
| **Generated code** | File path contains `/obj/`, `/bin/`, `/Migrations/`, or ends with `.g.cs` / `.Designer.cs` |
| **Noise types** | Name ends with `Exception`, or is `Task`, `ValueTask`, `List`, `Dictionary`, `Array`, `String`, `Object`, `Guid`, `CancellationToken` — caught by body-scan regexes but never real requests/events |
| **Self-calls** | `callerId == calleeId` — skipped |
| **Phantom nodes** | Call edges where either endpoint isn't a known solution type — skipped |
| **Lambda DI factories** | DI registrations with `sp =>`, `_ =>`, `(`, or `GetRequiredService` as impl — skipped |

### Edge ranking (deterministic, weight-free)

Edges are ranked structurally within each trace step:
1. **Sends** (0) — dispatch is the core story
2. **Handles** (1) — handler is the response
3. **Raises** (2) — events are important
4. **Consumes** (3) — event consumption
5. **ReadsWrites** (4) — data access
6. **Resolves** (5) — DI wiring
7. **WrappedBy** (6) — pipeline wrappers
8. **Calls** (7) — lowest priority, most likely framework noise

Uncertain edges (`Confidence != 1.0`) are ranked last within each priority tier.

### Twin-node resolution

Handler, Service, and Member nodes share their class's declaration. To avoid dead-ending after crossing an indirection seam (e.g., `Handles` edge lands on a Handler node), the trace builder follows out-edges from both the landed node and its **Type twin** — the same class with a different `NodeId`.

### Architecture style detection

Evidence-driven (not name-substring):

| Style | Signals | Confidence factors |
|---|---|---|
| **Microservices** | Aspire AppHost + ≥3 service projects | Capped below VerticalSlices |
| **CleanArchitecture** | MediatR + DDD folder roles (≥2 of Domain/Application/Infrastructure/Api) + aggregates + domain-event handlers | Up to 0.95 |
| **VerticalSlices** | FastEndpoints + MediatR | 0.70–0.85 |
| **NLayer** | EF Core + >2 projects, no strong DDD signals | 0.60 |
| **MinimalApi** | Minimal APIs, no MediatR | 0.65 (multi-project), 0.90 (single) |
| **ControllerBased** | Controllers present, no MediatR, dominant over MinimalApi | 0.55–0.70 |
| **ModularMonolith** | ≥2 projects with "module"/"bounded"/"context" in name | 0.55+ |

→ Full design: [TRACE-ENGINE-DESIGN.md](docs/product/TRACE-ENGINE-DESIGN.md) | [Trace rule reference](docs/product/TRACE-RULE-REFERENCE.md)

## What it extracts

| Detection | What it finds |
|-----------|---------------|
| **Endpoints** | Minimal API `Map*` calls, FastEndpoints, MVC controller actions |
| **MediatR handlers** | `IRequestHandler<T,Q>`, commands, queries, notifications |
| **Message consumers** | MassTransit `IConsumer<T>`, NServiceBus, in-memory `IEventHandler<T>` |
| **EF Core entities** | DbContext, DbSet properties, `OnModelCreating` config, aggregate roots |
| **DI registrations** | `AddSingleton`/`AddScoped`/`AddTransient`, extension methods, factory delegates, auto-registration |
| **Background workers** | `IHostedService`, `BackgroundService`, schedulers, Quartz jobs |
| **Middleware pipeline** | `Use*` calls in Program.cs, registration order |
| **Indirect wiring** | `Activator.CreateInstance`, service locator, reflection scanning |
| **Aspire resources** | `AddProject`, `AddRedis`, `AddPostgres`, `WithReference` |
| **Anti-patterns** | Fire-and-forget, `IServiceScopeFactory`, `new` outside DI, `CancellationToken.None` |
| **Event flow** | Publish/Subscribe pairs, handler implementations |
| **Architecture style** | Evidence-based: Microservices, CleanArchitecture, NLayer, MinimalApi, VerticalSlices, ControllerBased |
| **Call graph** | Roslyn semantic or syntactic call edges between solution types |

## Quickstart

**CLI:**
```bash
dotnet tool install -g DevContext.Cli
devcontext analyze .                              # Map (architecture overview)
devcontext analyze . --focus OrderService          # Trace from a type
devcontext analyze . --focus "GET /api/orders"     # Trace from an endpoint route
devcontext analyze . --focus Foo:Bar --depth 3     # Trace with explicit depth
devcontext analyze . --depth 6 --detail salient    # Full trace with source context
devcontext analyze . --stats                       # Nerd view (timing, funnel, cache)
devcontext analyze . --include-diagnostics         # Show graph assembly stats
devcontext analyze . --format json --strict        # JSON with runReport
```

**Desktop:** Download `DevContext.Desktop.zip` from [Releases](https://github.com/shaahink/DevContext2/releases), unzip, run `DevContext.Desktop.exe`. Three tabs: Human (HTML), LLM (markdown), Stats (timing waterfall, extractor grid, token funnel). Entry picker and depth/detail dials for trace control.

## Honest roadmap

**What's solid today:**
- Map + Trace engine over a connected CodeGraph (typed nodes/edges, all indirection seams bridged)
- Evidence-based architecture style detection (7 styles)
- All 12 detection extractors (endpoints → call graph)
- Analyze-once-render-many (snapshot + lens, sub-100ms re-renders)
- `--stats` everywhere + Stats tab on desktop
- Self-validating output (`--strict` mode, eval suite over real repos)

**Known limits:**
- Call edges are syntactic (regex-based) — marked `[approx]` in trace output. Semantic resolution (Roslyn `SemanticModel`) is designed behind `ISymbolResolver` and deferred to P3.
- Architecture-style detection on hybrid repos may misclassify — see [DETECTION-GUIDE.md](docs/product/DETECTION-GUIDE.md#5-architecture-style-detection-the-known-weak-spot).
- No persistent snapshot cache yet — each run re-analyses from source.

**Deliberately deferred:**
- **Beyond .NET** — the pipeline is language-agnostic by design; TS/other languages are roadmap-only.
- **LLM-value benchmark** — an honest harness measuring "does an LLM answer codebase questions better with DevContext output than with a raw file dump?" stays out of the README until it exists.
- **Persistent index** — serialize `CodeGraph` keyed by content hash for instant warm runs. Designed for (serialization-ready records, stable NodeId scheme) but not built.

## Development

```bash
dotnet build DevContext.slnx
dotnet test                                      # 288+ tests
$env:UPDATE_GOLDENS=1; dotnet test               # Regenerate goldens
```

## License

MIT
