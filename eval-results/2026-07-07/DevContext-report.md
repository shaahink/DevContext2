# REPORT
**DevContext**

Style: CleanArchitecture
_7 projects  ·  1 HttpEndpoint, 4 UiEntry, 32 GrpcService, 3 CliCommand  ·  net10.0, net10.0-windows10.0.19041.0 + blazor + controllers + desktop-ui + minimal-apis + serilog + grpc + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 323 |
| Projects | 22 |
| Nodes | 406 |
| Edges | 57 |
| Entries | 40 |
| With target | 40/40 |
| Verified edges | 91% |
| Analyzed in | 15.1s |

## Top Flows

1. **GET /health** → `Program` *(HttpEndpoint)*
2. **Proto.DevContextService.AnalysisCodeToGrpc** → `DevContextGrpcService` *(GrpcService)*
3. **Proto.DevContextService.Analyze** → `DevContextGrpcService` *(GrpcService)*
4. **Proto.DevContextService.CloseSession** → `DevContextGrpcService` *(GrpcService)*
5. **Proto.DevContextService.CompleteCall** → `DevContextGrpcService` *(GrpcService)*
6. **Proto.DevContextService.ConfigLookup** → `DevContextGrpcService` *(GrpcService)*
7. **Proto.DevContextService.Error** → `DevContextGrpcService` *(GrpcService)*
8. **Proto.DevContextService.FindTestsFor** → `DevContextGrpcService` *(GrpcService)*
9. **Proto.DevContextService.GetContext** → `DevContextGrpcService` *(GrpcService)*
10. **Proto.DevContextService.GetImpact** → `DevContextGrpcService` *(GrpcService)*

### Trace 1: GET /health

TRACE  GET /health
       src/DevContext.Server/Program.cs:42

▸ ENTRY  GET /health  (src/DevContext.Server/Program.cs:42)
   └─ call <lambda> GET /health  (src/DevContext.Server/Program.cs:42)
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: Proto.DevContextService.AnalysisCodeToGrpc

TRACE  Proto.DevContextService.AnalysisCodeToGrpc
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16
       DevContext.Server
▸ ENTRY  Proto.DevContextService.AnalysisCodeToGrpc  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
   └─ call DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
          public sealed class DevContextGrpcService(
          IAnalysisSessionManager sessions,
          McpObservabilityService mcpObs,

---

### Trace 3: Proto.DevContextService.Analyze

TRACE  Proto.DevContextService.Analyze
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16
       DevContext.Server
▸ ENTRY  Proto.DevContextService.Analyze  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
   └─ call DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
          public sealed class DevContextGrpcService(
          IAnalysisSessionManager sessions,
          McpObservabilityService mcpObs,

---

## Insights

_5 info · 4 notable · 1 warning_

### **WARNING**: Auth surface: 0 protected, 1 unannotated of 1 API endpoints
*(Risk)*

- 1 no auth annotation

### **NOTABLE**: 1/1 endpoints anonymous
*(Risk)*

- GET /health

### **NOTABLE**: ViewModel-View: 4 VMs + 1 Views (0 call edges)
*(Wiring)*

- 4 ViewModels
- 1 Views

### **NOTABLE**: Extension seats: AddMediatR (2 impls) · AddDevContextServices (2 impls) · AddLogging (2 impls)
*(Wiring)*

- AddMediatR (2 impls)
- AddDevContextServices (2 impls)
- AddLogging (2 impls)

### **NOTABLE**: Event flow: 2 published, 0 consumed, 2 orphan
*(Wiring)*

- 2 orphan events (published, no internal consumer)
- X ← GraphBuilder.AddRaises
- TIntegrationEvent ← GraphBuilder
- 0/2 events consumed

### _INFO_: Entry targets resolved 40/40 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 4 feature areas
*(Shape)*

- Endpoints (32 entries)
- ViewModels (4 entries)
- Commands (3 entries)
- global (1 entries)

### _INFO_: Routing surface: 1 routes exposed
*(Shape)*

- GET /health

### _INFO_: Public surface: 17 interfaces, 283 classes (437 total public types)
*(Shape)*

- 17 interfaces
- 283 classes

### _INFO_: Most depended-upon: Core (4 dependents) · DevContext.Core (3 dependents) · DevContext.Core (2 dependents)
*(Topology)*

- Core (4 dependents)
- DevContext.Core (3 dependents)
- DevContext.Core (2 dependents)

MAP  DevContext     (7 projects)

STACK  net10.0, net10.0-windows10.0.19041.0 · Minimal APIs · Controllers · MediatR (CQRS)

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure, Api, Core; MediatR with 2 handlers

       per service:
         DevContext.Benchmarks: Unknown
         DevContext.Cli: Unknown
         DevContext.Desktop: Unknown
         DevContext.Mcp: Unknown
         DevContext.Server: gRPC Service [gRPC]
         Web: CQRS [MediatR]
         Api: Unknown
         App: CQRS [MediatR, FluentValidation]
         Api: CQRS [MediatR]

TOPOLOGY (depends-on)
   DevContext.Core
   DevContext.Cli ── DevContext.Core
   DevContext.Contracts
   DevContext.Benchmarks ── DevContext.Cli, DevContext.Core
   DevContext.Desktop ── DevContext.Cli, DevContext.Core
   DevContext.Mcp ── DevContext.Contracts
   DevContext.Server ── DevContext.Cli, DevContext.Contracts, DevContext.Core

ENTRY POINTS
   HTTP (1)
      GET /health  → Program  (src/DevContext.Server/Program.cs:42)
   UI (4)
      [RelayCommand] MainViewModel.AnalyzeAsync  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:319)
      [RelayCommand] MainViewModel.ResetToScenarioDefaults  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:601)
      [RelayCommand] MainViewModel.SelectRecent  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:314)
      [RelayCommand] MainViewModel.SetFormat  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:311)
   gRPC (32)
      Proto.DevContextService.AnalysisCodeToGrpc  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.Analyze  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.CloseSession  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.CompleteCall  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.ConfigLookup  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.Error  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.FindTestsFor  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetContext  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetImpact  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetInterestingPoints  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetMap  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetNeighbors  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetNode  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetStats  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.GetTrace  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.IsLikelyTestMethod  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.ListEntryPoints  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.ListSessions  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.MapException  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      Proto.DevContextService.ObserveToolCalls  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:16)
      … and 12 more (grpc entries — use --focus for a drill-in)
   CLI (3)
      AnalyzeCommand —settings AnalyzeSettings  → AnalyzeCommand  (src/DevContext.Cli/Commands/AnalyzeCommand.cs:10)
      QueryCommand —settings QuerySettings  → QueryCommand  (src/DevContext.Cli/Commands/QueryCommand.cs:14)
      ReportCommand —settings ReportSettings  → ReportCommand  (src/DevContext.Cli/Commands/ReportCommand.cs:11)

PACKAGES
   Web/API:  Grpc.AspNetCore 2.*, Grpc.AspNetCore.Web 2.*, Microsoft.AspNetCore.Components.WebView.Wpf 10.*, Microsoft.AspNetCore.Mvc.Testing 10.0.*, Microsoft.AspNetCore.OpenApi 10.0.0
   ORM/Data:  Dapper 2.1.35, Microsoft.EntityFrameworkCore 10.0.0-preview.3.25171.5
   Mediator/CQRS:  MediatR 12.0.0
   Validation:  FluentValidation 11.5.0
   Logging:  Serilog 4.3.1, Serilog.Extensions.Logging 10.0.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0
   Testing:  coverlet.collector 10.0.1, NSubstitute 5.*, xunit 2.9.3, xunit.runner.visualstudio 3.1.5
   Other:  BenchmarkDotNet 0.15.8, CommunityToolkit.Mvvm 8.*, Google.Protobuf 3.*, Grpc.Core.Api 2.*, Grpc.Net.Client 2.*, Grpc.Net.Client.Web 2.*, Grpc.Tools 2.*, LibGit2Sharp 0.30.* … (19 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 11565ms |
| GenericExtraction | 550ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1952ms |
| Compression | 66ms |
| **Total** | **15065ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| FileTreeExtractor | 8749ms | 0 | 0 |
| SolutionDiscovery | 2783ms | 0 | 0 |
| EndpointExtractor | 1026ms | 0 | 31 |
| CallGraphExtractor | 920ms | 0 | 0 |
| SyntaxStructureExtractor | 548ms | 466 | 54 |
| DiRegistrationExtractor | 547ms | 0 | 54 |
| GrpcClientExtractor | 443ms | 0 | 20 |
| ControllerActionExtractor | 426ms | 0 | 20 |
| GrpcServiceExtractor | 417ms | 0 | 17 |
| CliCommandExtractor | 414ms | 0 | 15 |
| MediatRExtractor | 397ms | 0 | 16 |
| InMemoryEventBusExtractor | 396ms | 0 | 14 |
| BlazorEntryExtractor | 343ms | 0 | 14 |
| IndirectWiringDetector | 338ms | 0 | 13 |
| DesktopEntryExtractor | 243ms | 0 | 14 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 40 | 0 |
| Raises | 3 | 3 |
| Resolves | 14 | 2 |

_323 files · 22 projects_
