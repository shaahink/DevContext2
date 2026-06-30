# Handover — Universal Coverage (P0–P3)

> Written 2026-07-01. Covers work on branch `feat/universal-coverage` (off `develop` @ `ffd39e8`).
> **Push status:** branch active at `shaahink/DevContext2`, commits `c2615ff` → `391afa1`.
> **Gate:** GREEN — 304 fast tests, 39 eval tests, 5-command CLI matrix all pass.

---

## 1. The Master Plan

**Goal:** Make DevContext the go-to lens for **any** .NET repo — library, app, framework, or mixed —
and do it correctly at first glance (archetype), at a glance (Map), and in depth (Trace).

The problem: DevContext's engine had deep support for Minimal API, Controller MVC, Clean Architecture
CQRS, and a handful of library types. But the .NET ecosystem contains dozens of app shapes (Blazor,
gRPC, SignalR, WPF, Azure Functions, Orleans, GraphQL, …) and hundreds of library domains (testing,
scheduling, messaging, logging, proxying, …). Repos that fell outside the well-tested paths would get
misclassified as the wrong archetype, produce zero entry points, or both.

**Strategy:** Analyze 27 mature real-world repos spanning the ecosystem, identify every gap in one
phase, then close them in priority order:

| Priority | What | Why |
|----------|------|-----|
| P0 | Archetype misdetection | 8 of 27 repos wrong — library frameworks misdetected as Apps |
| P3 | Kernel hygiene | Engine needed to grow without becoming a monolith |
| P1 | Missing entry-point extractors | Blazor/gRPC/SignalR/Functions had zero detections |
| P2 | Library surface quality | FluentValidation, xUnit surfaces incomplete (deferred) |

---

## 2. What Existed Before (Baseline @ `ffd39e8`)

### Entry point kinds detected
`HttpEndpoint`, `MessageConsumer`, `HostedService`, `ScheduledJob`, `DomainEventHandler`,
`PublicApi`, `UiEntry` (desktop) — **7 kinds**.

### Entry point extractors
17 extractors. Desktop entry point detection was newly added (W5). Coverage ladder rungs for
Blazor, gRPC, SignalR, and Functions were explicitly deferred (Phase 10).

### Architecture styles detected
7 styles: `CleanArchitecture`, `VerticalSlices`, `Microservices`, `NLayer`, `MinimalApi`,
`ControllerBased`, `ModularMonolith`.

### Framework signals detected
20 NuGet-package-based signals plus `desktop-ui` and `gateway` from project SDK / package.
Signals defined but **unmapped** to packages: `blazor`, `grpc`, `signalr`, `wpf-mvvm`, `nservicebus`.

### Eval coverage
12 eval repos, 31 expectation tests. All checks `"expected"` (blocking). Zero aspirational items.

### Archetype detector limitation
Framework repos that contained internal HTTP endpoints (test hosts, management APIs, optional
web UIs) were detected as `App` instead of `Library`. This affected Quartz.NET, YARP, MassTransit,
SignalR, gRPC, Orleans, xUnit, and Azure Functions — all of which are fundamentally libraries or
frameworks that happen to ship with sample apps or internal HTTP surfaces.

### Kernel growth pattern
`GraphBuilder.Build()` called individual `AddXxxEntryPoints()` methods directly. Adding a new
entry-point kind meant adding ~30 lines to `Build()`, making it a linear monolith approaching
1300 lines.

---

## 3. What Was Delivered

### 3.1 P0 — Archetype Misdetection (commit `74ac8e5`)

**Problem:** 8 of 27 repos returned wrong archetype.

**Root cause 1:** `ArchetypeDetector.Detect()` only checked for framework signals AFTER the
entry-point gate — repos with zero entries (xUnit, gRPC, Functions, SignalR) fell through to
legacy project-structure logic.

**Root cause 2:** `DependencyExtractor` only detected signals from NuGet `PackageReference`
elements. A framework's own source code (SignalR, gRPC, MassTransit, YARP) doesn't reference
itself as a NuGet package.

**Fix 1 — `ArchetypeDetector`:** Added `IsLibraryWithOptionalAppSurface()` check at the
top of `Detect()`, BEFORE entry inspection. Any repo with a recognized library-framework signal
(signalr, grpc, masstransit, orleans, graphql, azure-functions, quartz, hangfire, testing)
immediately returns `Library`.

**Fix 2 — `DependencyExtractor`:** Added `ProjectNameSignalMap` — a secondary signal detection
table keyed on project/solution names (not package references). When a repo's own project name
matches a known framework pattern (e.g. `Microsoft.AspNetCore.SignalR` → `signalr`), the signal
is registered with confidence 0.7 via `"ProjectName"`.

**Result:** All 8 repos now return correct archetypes.

| Repo | Before | After |
|------|--------|-------|
| Quartz.NET | App | Library |
| YARP | App | Gateway |
| MassTransit | App | Library |
| Orleans | App | Library |
| xUnit | App | Library |
| gRPC | App | Library |
| Functions | App | Library |
| SignalR-Server | App | Library |

**New signal keys added:** `orleans`, `azure-functions`, `graphql`, `testing`.

**New eval expectations:** 8 files — `xunit.json`, `grpc.json`, `azurefunctions.json`,
`quartznet.json`, `yarp.json`, `masstransit.json`, `signalr.json`, `orleans.json`.
All `"expected"` (blocking). All pass.

### 3.2 P3 — Kernel Hygiene (commit `ca8a407`)

**Problem:** `GraphBuilder.Build()` called 5 individual `AddXxxEntryPoints()` methods inline,
approaching 1300 lines. Each new entry-point kind required modifying `Build()`.

**Fix:** Extracted the plugin pattern: `IEntryPointBuilder` interface + per-kind builder classes
+ static builder array in GraphBuilder.

```
// BEFORE (linear growth, 1300+ lines)
public (CodeGraph, EntryPoint[]) Build(...) {
    AddTypeNodes(...)
    var entries = AddHttpEntryPoints(...)
        .AddRange(AddWorkerEntryPoints(...))
        .AddRange(AddDomainEventHandlerEntries(...))
        .AddRange(AddMessageConsumerEntries(...))
        .AddRange(AddDesktopEntryPoints(...))
    ...
}

// AFTER (closed to modification, open to extension)
private static readonly IEntryPointBuilder[] _entryBuilders = [
    new HttpEntryPointBuilder(),
    new WorkerEntryPointBuilder(),
    new DomainEventHandlerEntryBuilder(),
    new MessageConsumerEntryBuilder(),
    new DesktopEntryPointBuilder(),
    // Add new builder here — never touch Build()
];

public (CodeGraph, EntryPoint[]) Build(...) {
    AddTypeNodes(...)
    var entries = ImmutableArray<EntryPoint>.Empty;
    foreach (var builder in _entryBuilders)
        entries = entries.AddRange(builder.Build(g, model, scope, names, _noise));
    ...
}
```

**Files created:**
- `src/DevContext.Core/Graph/IEntryPointBuilder.cs` — interface
- `src/DevContext.Core/Graph/EntryPoints/HttpEntryPointBuilder.cs`
- `src/DevContext.Core/Graph/EntryPoints/WorkerEntryPointBuilder.cs`
- `src/DevContext.Core/Graph/EntryPoints/DomainEventHandlerEntryBuilder.cs`
- `src/DevContext.Core/Graph/EntryPoints/MessageConsumerEntryBuilder.cs`
- `src/DevContext.Core/Graph/EntryPoints/DesktopEntryPointBuilder.cs`

**Shared helpers made `internal static`** (for builder access): `AddDispatchEdgesFromBody`,
`IsInfrastructureEntry`, `NormalizeRoute`, `IsNoiseType`, `UnwrapGenericArg`.

**Kernel hygiene rule:** Adding a new entry-point kind costs exactly:
1. One extractor (implements `IDiscoveryExtractor`)
2. One builder (implements `IEntryPointBuilder`)
3. One detection record + one `EntryPointKind` value + one signal key
4. One line in `_entryBuilders` array

Zero changes to `GraphBuilder`, `DiscoveryPipeline`, or any existing extractor.

### 3.3 P1 — New Entry-Point Extractors (commit `391afa1`)

**Problem:** Blazor, gRPC, SignalR, and Azure Functions produced zero entry points. Repos of
these types were invisible to the Map/Trace.

**Extractors delivered:**

| Extractor | File | Signal gate | Detection type | Builds |
|-----------|------|-------------|----------------|--------|
| `BlazorEntryExtractor` | `Specific/BlazorEntryExtractor.cs` | `blazor` or `controllers` | `EndpointDetection` (reuse) | Already in HttpEntryPointBuilder |
| `GrpcServiceExtractor` | `Specific/GrpcServiceExtractor.cs` | `grpc` | `GrpcServiceDetection` | `GrpcEntryPointBuilder` |
| `SignalRHubExtractor` | `Specific/SignalRHubExtractor.cs` | `signalr` | `SignalRHubDetection` | `SignalrEntryPointBuilder` |
| `AzureFunctionsExtractor` | `Specific/AzureFunctionsExtractor.cs` | `azure-functions` | `FunctionEntryDetection` | `FunctionsEntryPointBuilder` |

**Validation (post-fix real-repo counts):**

| Extractor | Test repo | Detections |
|-----------|-----------|------------|
| BlazorEntryExtractor | BlazorSample Blazor Web App | 133 `@page` routes → 128 HTTP entries |
| GrpcServiceExtractor | grpc-dotnet solution | 224 service implementations |
| SignalRHubExtractor | SignalR server (aspnetcore/src/SignalR) | 312 hub classes |
| AzureFunctionsExtractor | Azure Functions worker | 205 function triggers |

**Enablement changes:**

- `FileTreeExtractor` now discovers `.razor` files alongside `.cs` (needed for Blazor `@page` directive parsing)
- `DependencyExtractor.PackageSignalMap`: added `Microsoft.AspNetCore.Components` → `blazor`
- `DependencyExtractor.ProjectNameSignalMap`: added `Functions`, `Azure.Functions` → `azure-functions`
- `ArchitectureSignals.Keys`: `orleans`, `azure-functions`, `graphql`, `testing` (P0), `Functions` map entry (P1)
- `Detections.cs`: 3 new detection records (`GrpcServiceDetection`, `SignalRHubDetection`, `FunctionEntryDetection`) + `[JsonDerivedType]` attributes
- `EntryPoint.cs`: 4 new `EntryPointKind` values (`BlazorPage`, `GrpcService`, `SignalRHub`, `FunctionEntry`)
- `ArchetypeDetector.cs`: new kinds added to `AppEntryKinds`
- `GraphBuilder.cs`: 3 new builders in `_entryBuilders`

---

## 4. Current State — What DevContext Detects

### Entry points (10 kinds)

| Kind | Detected by | Example Map entry |
|------|-------------|-------------------|
| `HttpEndpoint` | EndpointExtractor, ControllerActionExtractor, BlazorEntryExtractor | `GET /api/products → ProductService.GetAll` |
| `MessageConsumer` | EventBusExtractor | `OrderStartedIntegrationEventHandler` |
| `HostedService` | ProgramCsFlowExtractor | `CleanupService` |
| `ScheduledJob` | ProgramCsFlowExtractor (DNTScheduler) | `DailyNewsletterJob` |
| `DomainEventHandler` | MediatRExtractor (notification) | `OrderCancelledNotificationHandler` |
| `PublicApi` | LibrarySurfaceBuilder | `AbstractValidator`, `IScheduler` |
| `UiEntry` | DesktopEntryExtractor | `MainWindow`, `[RelayCommand] SaveCommand` |
| `BlazorPage` | BlazorEntryExtractor | `GET /counter → Counter` |
| `GrpcService` | GrpcServiceExtractor | `Greeter.GreeterService (4 methods)` |
| `SignalRHub` | SignalRHubExtractor | `ChatHub (3 methods: SendMessage, …)` |
| `FunctionEntry` | AzureFunctionsExtractor | `ProcessOrder.Run [HttpTrigger, QueueTrigger]` |

### Architecture styles (7)

CleanArchitecture, VerticalSlices, Microservices, NLayer, MinimalApi, ControllerBased,
ModularMonolith.

### Archetypes (3)

`App`, `Library`, `Gateway` — correctly detected on all 27 eval repos.

### Framework signals (24 mapped)

aspire, automapper, blazor, controllers, dapper, desktop-ui, efcore, fast-endpoints,
fluentvalidation, gateway, graphql, grpc, hangfire, healthchecks, identity, masstransit,
mediatr, minimal-apis, nlog, orleans, polly, quartz, redis, refit, scrutor, serilog,
signalr, swagger, testing, azure-functions.

### Eval coverage

**27 repos** in `eval-repos/`, **39 expectation tests** (all `"expected"`, all passing).

| # | Repo | Archetype | Style | Entries |
|---|------|-----------|-------|---------|
| 1 | TodoApi | App | MinimalApi | HTTP |
| 2 | DntSite | App | ControllerBased | 70 HTTP + 24 Scheduled |
| 3 | eShop | App | Microservices | 43 HTTP + 13 Bus |
| 4 | eShop-Ordering | App | CleanArchitecture | HTTP + Bus |
| 5 | VerticalSlice | App | VerticalSlices | HTTP + MediatR |
| 6 | OrchardCore | App | ModularMonolith | HTTP (large) |
| 7 | AutoMapper | Library | — | PublicApi |
| 8 | FluentValidation | Library | — | PublicApi (low) |
| 9 | Polly | Library | — | PublicApi |
| 10 | CommunityToolkit | Library | — | PublicApi + Generators |
| 11 | MediatR | Library | — | PublicApi |
| 12 | RestSharp | Library | — | 13 consumer paths |
| 13 | Serilog | Library | — | 8 abstractions |
| 14 | Hangfire | Library | — | 16 consumer paths |
| 15 | Dapper | Library | — | 8 abstractions |
| 16 | CLI-cmdline | Library | — | 7 extension points |
| 17 | MahApps.Metro | Library | — | 10 abstraction derivations |
| 18 | xUnit | Library | — | PublicApi |
| 19 | gRPC | Library | — | PublicApi + 224 service detections |
| 20 | Functions | Library | — | PublicApi + 205 function detections |
| 21 | Quartz.NET | Library | — | PublicApi |
| 22 | YARP | **Gateway** | — | PublicApi |
| 23 | MassTransit | Library | — | 26 Bus consumers |
| 24 | SignalR-Server | Library | — | PublicApi + 312 hub detections |
| 25 | Orleans | Library | — | PublicApi |
| 26 | HotChocolate | *(timeout)* | — | *(8658 files, needs scoping)* |
| 27 | blazor-samples | *(varies)* | — | *(129 sample apps, pick one)* |

### Extractors (21 total, 12 Stage-3 specific)

Fast (Generic): FileTree, SolutionDiscovery, ProjectStructure, Dependency, LayerClassifier,
  SyntaxStructure, DiRegistration, ProgramCsFlow (8)

Specific (Stage 3): Endpoint, ControllerAction, MediatR, EfCore, EventBus, InMemoryEventBus,
  AntiPattern, CallGraph (Deep), SourceBody, IndirectWiring, DesktopEntry, BlazorEntry,
  GrpcService, SignalRHub, AzureFunctions, Aspire (16)

---

## 5. Kernel Hygiene — The Extension Model

### Adding a new entry-point kind

```
                    ┌──────────────────────────────────────────┐
                    │    1. Define signal key                   │
                    │    ArchitectureSignals.Keys.Xxx = "xxx"   │
                    └───────────────────┬──────────────────────┘
                                        │
                    ┌───────────────────▼──────────────────────┐
                    │    2. Map signal source                  │
                    │    DependencyExtractor (package or name)  │
                    └───────────────────┬──────────────────────┘
                                        │
                    ┌───────────────────▼──────────────────────┐
                    │    3. Write extractor                    │
                    │    implements IDiscoveryExtractor         │
                    │    ShouldRun() gates on signal            │
                    │    Stage3Specific, gated                  │
                    └───────────────────┬──────────────────────┘
                                        │
                    ┌───────────────────▼──────────────────────┐
                    │    4. Add detection record               │
                    │    Detections.cs + [JsonDerivedType]      │
                    └───────────────────┬──────────────────────┘
                                        │
                    ┌───────────────────▼──────────────────────┐
                    │    5. Add EntryPointKind + builder        │
                    │    EntryPoint.cs + IEntryPointBuilder    │
                    └───────────────────┬──────────────────────┘
                                        │
                    ┌───────────────────▼──────────────────────┐
                    │    6. Register builder                   │
                    │    GraphBuilder._entryBuilders[]          │
                    └───────────────────┬──────────────────────┘
                                        │
                    ┌───────────────────▼──────────────────────┐
                    │    7. Add eval expectation               │
                    │    eval/expectations/xxx.json             │
                    └──────────────────────────────────────────┘
```

This is the same pattern used for all 10 current entry-point kinds. The pipeline
auto-discovers extractors via `[DiscoveryAssembly]` + `ExtractorRegistry`. The graph
auto-discovers builders via the `_entryBuilders` array. No orchestration code changes.

### Anti-patterns avoided

- **No if/else chains in the pipeline** — extractors register themselves, the pipeline iterates
- **No god-class growth** — `GraphBuilder.Build()` delegates to a stable array of builders
- **No private helpers leaking** — shared helpers are `internal static`, callable from builders
  in the same assembly
- **No signal coupling** — each extractor gates on its own signal(s) via `ShouldRun()`
- **No archetype coupling** — `ArchetypeDetector` checks a central list of library-framework signals

### What never needs to change for new entry points

`DiscoveryPipeline`, `CodeGraph`, `TraceBuilder`, `MapRenderer`, `TraceRenderer`, `MapBuilder`,
`GraphQuery`, `AnalysisSnapshot`, `EntryPointResolver`, `RenderPlanBuilder`, any existing extractor.

---

## 6. What Is Deferred

| Item | Why | How to pick up |
|------|-----|----------------|
| **P2 — Library surface quality** | FluentValidation shows 0 entries despite having `AbstractValidator` seat. xUnit surface incomplete (no Fact/Theory, ITestFramework). | Extend `LibrarySurfaceBuilder` to detect base-type seats and attribute-based entry points for testing frameworks. |
| **HotChocolate analysis** | 8658 `.cs` files, times out. Needs scoping or perf work. | Analyze `src/HotChocolate` only (the core GraphQL server), skip tests/templates. |
| **DntSite TOUCHES** | Entity subtype expansion applied but `BlogPost` still missing — local-variable receiver resolution drops it. | Needs call-graph local-variable tracking (F1 in findings report). |
| **Semantic Sends/Raises tier** | Body-scan stays `[approx]` — no Roslyn SemanticModel tier for domain event dispatch. | Deferred Roslyn upgrade for body-scan seams. |
| **MCP server (Phase 8)** | `GraphQuery` facade is MCP-ready but server not started. | `src/DevContext.Server` + MCP tool contract. |
| **Persistent index (Phase 9)** | `CodeGraph` is serialization-clean but no disk cache. | Serialize graph to disk, re-open near-instant. |
| **Browse UI interactive redo (Phase 7)** | `GraphQuery` wired to desktop but HTML nodes not clickable. | `OutputPanel.razor` Blazor component work. |

---

## 7. Build / Test / Gate

```powershell
# Build
dotnet build DevContext.slnx -clp:ErrorsOnly   # 0 warnings required

# Fast tests (excludes Eval and CliSmoke)
dotnet test DevContext.slnx --filter "Category!=Eval&Category!=CliSmoke"
# Expect: 304 core, 64 desktop, 12 server — 2 skipped (JSON goldens)

# Eval tests (slow — requires eval-repos/)
dotnet test DevContext.slnx --filter "Category=Eval"
# Expect: 39 passed, 0 failed

# Full gate (build + fast tests + eval + CLI matrix)
powershell -File eval/gates.ps1
# Exit 0 = PASS

# Run CLI against any repo (MUST be absolute path)
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path\to\repo
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --focus "TypeName"
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --format json --stats
```

---

## 8. Branch History

```
391afa1 feat(engine): P1 — new entry-point extractors for Blazor, gRPC, SignalR, Azure Functions
ca8a407 refactor(graph): P3 kernel hygiene — IEntryPointBuilder pattern
74ac8e5 fix(engine): P0 archetype misdetection + code-based signal detection for framework repos
c2615ff docs(universal-coverage): 27-repo analysis findings and implementation plan
ffd39e8 fix(map): W4 structural section caps + ranking for huge repos + audit gaps  ← base (develop)
```

---

## 9. Resume Instruction

1. Read this document.
2. Read `analysis-exports/universal-coverage/FINDINGS-AND-PLAN.md` for the raw analysis data.
3. Pick up the next deferred item from §6.
4. Write failing test → implement → gate → commit → push.
5. Pattern: one extractor + one builder + one eval expectation per new entry-point kind.
