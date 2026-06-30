# Universal Coverage — 27-Repo Analysis & Plan

> Branch: `feat/universal-coverage` (off `develop` @ `ffd39e8`)
> Analyzed: 2026-06-30

## Analysis Matrix (27 repos, 20 small + 7 huge)

| # | Repo | Type | Arch | Style | Entries | Verdict |
|---|------|------|------|-------|---------|--------|
| 1 | TodoApi | Minimal API App | App ✓ | MinimalApi ✓ | 1 map (HTTP) | OK |
| 2 | DntSite | Controller Web App | App ✓ | ControllerBased ✓ | 70 HTTP + 24 Sched | OK |
| 3 | eShop | Microservices | App ✓ | Microservices ✓ | 43 HTTP + 13 Bus | OK |
| 4 | eShop-Ordering | Single-service | App ✓ | CleanArchitecture ✓ | 1 map | OK |
| 5 | VerticalSlice | VSA+FastEndpoints | App ✓ | VerticalSlices ✓ | 1 map | OK |
| 6 | OrchardCore | Modular Monolith | App ✓ | ModularMonolith ✓ | 1 map | OK |
| 7 | AutoMapper | Library | Library ✓ | NLayer | 1 map (PUBLIC API) | OK |
| 8 | FluentValidation | Library | Library ✓ | Unknown | 0 entries (surface visible) | LOW — 6 detections |
| 9 | Polly | Library | Library ✓ | MinimalApi (mis-flagged) | 1 map (PUBLIC API) | OK* (style is noise for Library) |
| 10 | CommunityToolkit | Library (src-gen) | Library ✓ | Unknown | 1 map (GENERATORS) | OK |
| 11 | MediatR | Library | Library ✓ | CleanArchitecture (mis) | 1 map (PUBLIC API) | OK* |
| 12 | RestSharp | HTTP client library | Library ✓ | Unknown | 13 (annotate+derive+implement+extend) | OK |
| 13 | Serilog | Logging library | Library ✓ | Unknown | 8 (implement+extend) | OK |
| 14 | Hangfire | Job library | Library ✓ | Unknown | 16 (register+implement+derive+extend) | OK |
| 15 | Dapper | Micro-ORM | Library ✓ | NLayer | 8 (derive+implement+extend) | OK |
| 16 | CLI-cmdline | CLI framework | Library ✓ | Unknown | 7 (derive+extend) | OK |
| 17 | MahApps.Metro | WPF UI library | Library ✓ | Unknown | 10 (derive+extend) | OK |
| 18 | xUnit | **Testing framework** | **MAP ✗** | Unknown | **0** | **FAIL — misdetected as App** |
| 19 | gRPC | **gRPC framework** | **MAP ✗** | ControllerBased ✗ | **0** | **FAIL — misdetected as App** |
| 20 | Functions | **Azure Functions lib** | **MAP ✗** | NLayer ✗ | **0** | **FAIL — misdetected as App** |
| 21 | Quartz.NET | **Scheduling library** | **MAP ✗** | MinimalApi ✗ | 51 HTTP (internal mgmt API) | **FAIL — wrong archetype, wrong entries** |
| 22 | YARP | **Reverse proxy lib** | **MAP ✗** | ControllerBased ✗ | 1 HTTP (internal) | **FAIL — wrong archetype** |
| 23 | MassTransit | Message bus framework | **MAP ✗** | NLayer | 26 Bus | **FAIL — wrong archetype** |
| 24 | SignalR-Server | Real-time framework | **MAP ✗** | MinimalApi ✗ | **0** | **FAIL — wrong archetype** |
| 25 | Orleans | Actor framework | **MAP ✗** | Microservices ✗ | 0 (token budget exceeded) | **FAIL — wrong archetype** |
| 26 | HotChocolate | GraphQL framework | *(timed out)* | — | — | *(timeout — 8658 files)* |

## Systemic Failures

### F1: Archetype Misdetection (8 repos)
The `ArchetypeDetector` misdetects libraries that have internal test hosts, sample apps, or management APIs as Apps.

| Root Cause | Repos affected |
|---|---|
| Internal HTTP endpoints (management API) | Quartz.NET, YARP |
| Test hosts w/ controllers | gRPC, Functions |
| No distinction between framework + consumer code | xUnit, MassTransit, SignalR, Orleans |

**Fix:** Signal-based library confidence boost. When a repo has **no application entry-point kind** (no HttpEndpoint, no HostedService, no BlazorPage) but has abstraction-heavy public API surface, boost library confidence above app confidence.

### F2: Signal Detection — Only via NuGet Packages
The `DependencyExtractor` only detects signals from NuGet `PackageReference` elements. A framework's own source code (SignalR, gRPC, MassTransit, YARP) doesn't reference itself as a package → signals are not detected.

| Missing signal | Repo | Why |
|---|---|---|
| gateway | YARP | YARP's own source doesn't reference `Microsoft.ReverseProxy` as NuGet |
| signalr | SignalR-Server | Own source, no self-reference |
| grpc | gRPC | Own source, no self-reference |
| masstransit | MassTransit | Own source, no self-reference |

**Fix:** Add **code-based signal detection** in DependencyExtractor — check if the repo's own project/solution NAME matches a known framework pattern.

### F3: Zero Entry Points (3 repos)
xUnit, gRPC, Functions have zero consumer-facing entry points surfaced.

| Repo | What should be detected | Missing extractor |
|---|---|---|
| xUnit | `[Fact]`, `[Theory]` attributes, `ITestFramework`, assertion classes | No test-framework extractor |
| gRPC | `Greeter.GreeterBase` subclasses, `.proto` service defs, `MapGrpcService<T>` | No gRPC extractor |
| Functions | `[Function]`, `[HttpTrigger]`, `[TimerTrigger]` attributes | No Functions extractor |

### F4: Library Surface — Empty for Some Repos
FluentValidation shows only 6 detections and 0 entries. The library surface builder works for most libraries but is inconsistent:
- FluentValidation: 0 entries, 6 detections (expected: DI registration + AbstractValidator seat)
- xUnit: 0 entries (expected: ITestFramework, FactAttribute, assertions)

### F5: Token Budget Overrun on Huge Repos
Orleans (3034 files): 236K tokens produced vs 8000 budget → MAP header only, no entry inventory.

---

## Gap Prioritization (Engineering Plan)

### P0 — Archetype Misdetection Fixes (1-2 days)

| Fix | What changes |
|---|---|
| **Boost library confidence when no app entries** | `ArchetypeDetector` — after GraphBuilder runs, if no EntryPoint of kind HttpEndpoint/HostedService/ScheduledJob exists, library archetype gets +0.3 confidence boost |
| **Gate archetype detection on real app signals** | Require at least 1 application entry point for App archetype |
| **Code-based signal detection** | `DependencyExtractor` — check project/solution name against known framework patterns: `SignalR` → signalr, `Grpc` → grpc, `MassTransit` → masstransit, `YARP` / `ReverseProxy` → gateway |

### P1 — Missing Entry Point Extractors (3-4 days)

| Extractor | Gating signal | Detects | Repo validated by |
|---|---|---|---|
| **gRPC extractors** (new) | grpc | `XxxBase` subclass, `.proto` service methods, `MapGrpcService<T>` | grpc-dotnet |
| **SignalR extractor** (new) | signalr | `Hub`/`Hub<T>` subclasses, `MapHub<T>` | SignalR-Server |
| **Functions extractor** (new) | functions | `[Function]`/`[HttpTrigger]`/`[TimerTrigger]` attributes | Azure Functions |
| **Blazor extractor** (new) | blazor | `@page` directives, component classes | blazor-samples / DntSite |

### P2 — Library Archetype Quality (1-2 days)

| Fix | What changes |
|---|---|
| **Boost Library surface builder for all libraries** | Ensure at least ABSTRACTIONS + ENTRY API sections always appear |
| **xUnit/FluentValidation surface fixes** | Extend `LibrarySurfaceBuilder` to detect test-framework entry points (Fact/Theory attributes) and validation-framework seats (AbstractValidator) from base types |
| **Gateway signal → Library archetype** | YARP is a library, not an app — gateway signal should bias toward Library |

### P3 — Kernel Hygiene Refactoring (2-3 days)

| Refactoring | File | New pattern |
|---|---|---|
| **IEntryPointBuilder** | GraphBuilder.cs | Extract entry-point building to per-kind strategy classes |
| **IEdgeBuilder** | GraphBuilder.cs | Extract edge building to per-kind strategy classes |
| **IStyleScorer** | ArchitectureStyleDetector.cs | Extract style scoring to per-style strategy classes |
| **Code-based signal detection** | DependencyExtractor.cs | Add project-name-based signal inference alongside package-based |

---

## Failing Test Plan

### New Eval Expectation Files

Each new eval repo gets an expectation file. These will ALL FAIL until the engine fixes are implemented.

#### `eval/expectations/xunit.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected", "note": "xUnit is a test framework library, not an app" }
```

#### `eval/expectations/grpc.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected" },
{ "id": "signal-grpc", "type": "signal-present", "value": "grpc", "status": "expected" }
```

#### `eval/expectations/signalr.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected" },
{ "id": "signal-signalr", "type": "signal-present", "value": "signalr", "status": "expected" }
```

#### `eval/expectations/functions.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected" }
```

#### `eval/expectations/quartznet.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected" }
```

#### `eval/expectations/yarp.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected" },
{ "id": "signal-gateway", "type": "signal-present", "value": "gateway", "status": "expected" }
```

#### `eval/expectations/masstransit.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected" },
{ "id": "bus-entries", "type": "output-contains", "format": "markdown", "value": "Bus", "status": "expected" }
```

#### `eval/expectations/orleans.json`
```json
{ "id": "archetype-library", "type": "json-equals", "path": "$.archetype", "value": "Library", "status": "expected" }
```

### New Trace Quality Tests

```csharp
// DesktopEntryExtractor_MahApps_DetectsWindowEntry
// Verifies MahApps.Metro's Window subclasses are detected as desktop entries

// SignalrHub_HubMethods_Detected  
// Verifies SignalR hub methods appear as entries

// GrpcService_ServiceBase_Detected
// Verifies gRPC service implementations appear as entries

// BlazorRoute_PageDirective_Detected
// Verifies @page "/route" directives produce entry points

// AzureFunction_FunctionAttribute_Detected
// Verifies [Function] attributed methods produce entry points
```

---

## Implementation Order

1. **P0** archetype fixes (make 8 misdetected repos pass archetype=Library)
2. **P3** kernel hygiene refactoring (before adding more extractors)
3. **P2** library surface quality (improve existing library detection)
4. **P1** new entry point extractors (gRPC, SignalR, Functions, Blazor)
5. Write failing tests → implement fixes → ratchet aspirational→expected
6. Re-eval all 27 repos
7. Run full gate
