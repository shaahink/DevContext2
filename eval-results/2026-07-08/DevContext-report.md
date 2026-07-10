# REPORT
**DevContext**

Style: CleanArchitecture
_7 projects  ·  1 HttpEndpoint, 4 UiEntry, 35 GrpcService, 3 CliCommand  ·  net10.0, net10.0-windows10.0.19041.0 + blazor + controllers + desktop-ui + minimal-apis + serilog + grpc + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 351 |
| Projects | 22 |
| Nodes | 1278 |
| Edges | 1231 |
| Entries | 43 |
| With target | 43/43 |
| Verified edges | 59% |
| Analyzed in | 48.2s |

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
10. **Proto.DevContextService.GetContextPack** → `DevContextGrpcService` *(GrpcService)*

### Trace 1: GET /health

TRACE  GET /health
       src/DevContext.Server/Program.cs:42
       DevContext.Server
▸ ENTRY  GET /health  (src/DevContext.Server/Program.cs:42)
   └─ call <lambda> GET /health  (src/DevContext.Server/Program.cs:42)
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: Proto.DevContextService.AnalysisCodeToGrpc

TRACE  Proto.DevContextService.AnalysisCodeToGrpc
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17
       DevContext.Server
▸ ENTRY  Proto.DevContextService.AnalysisCodeToGrpc  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
   └─ call DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
          public sealed class DevContextGrpcService(
          IAnalysisSessionManager sessions,
          McpObservabilityService mcpObs,

---

### Trace 3: Proto.DevContextService.Analyze

TRACE  Proto.DevContextService.Analyze
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17
       DevContext.Server
▸ ENTRY  Proto.DevContextService.Analyze  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
   └─ call DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
          public sealed class DevContextGrpcService(
          IAnalysisSessionManager sessions,
          McpObservabilityService mcpObs,

---

## Insights

_6 info · 3 notable · 1 warning_

### **WARNING**: Auth surface: 0 protected, 1 unannotated of 1 API endpoints
*(Risk)*

- 1 no auth annotation

### **NOTABLE**: 1/1 endpoints anonymous
*(Risk)*

- GET /health

### **NOTABLE**: ViewModel-View: 4 VMs + 2 Views (0 call edges)
*(Wiring)*

- 4 ViewModels
- 2 Views

### **NOTABLE**: Extension seats: AddMediatR (2 impls) · AddDevContextServices (2 impls) · AddLogging (2 impls)
*(Wiring)*

- AddMediatR (2 impls)
- AddDevContextServices (2 impls)
- AddLogging (2 impls)

### _INFO_: Entry targets resolved 43/43 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Command inventory: 16 ICommand implementations
*(Wiring)*

- FakeCommand
- VersionCommand
- PingCommand
- CollectionCommand
- RecordCommand

### _INFO_: Module map: 4 feature areas
*(Shape)*

- Endpoints (35 entries)
- ViewModels (4 entries)
- Commands (3 entries)
- global (1 entries)

### _INFO_: Routing surface: 1 routes exposed
*(Shape)*

- GET /health

### _INFO_: Public surface: 19 interfaces, 307 classes (495 total public types)
*(Shape)*

- 19 interfaces
- 307 classes

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
   gRPC (35)
      Proto.DevContextService.AnalysisCodeToGrpc  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.Analyze  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.CloseSession  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.CompleteCall  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.ConfigLookup  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.Error  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.FindTestsFor  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetContext  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetContextPack  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetGraphFacets  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetImpact  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetInterestingPoints  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetMap  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetNeighbors  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetNode  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetStats  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.GetTrace  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.IsLikelyTestMethod  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.ListEntryPoints  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      Proto.DevContextService.ListSessions  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      … and 15 more (grpc entries — use --focus for a drill-in)
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
   Testing:  coverlet.collector 10.0.1, NSubstitute 5.*, xunit 2.9.3, xunit.runner.visualstudio 3.1.5, Xunit.SkippableFact 1.5.23
   Other:  BenchmarkDotNet 0.15.8, CommunityToolkit.Mvvm 8.*, Google.Protobuf 3.*, Grpc.Core.Api 2.*, Grpc.Net.Client 2.*, Grpc.Net.Client.Web 2.*, Grpc.Tools 2.*, LibGit2Sharp 0.30.* … (19 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 20189ms |
| GenericExtraction | 781ms |
| SignalSealing | 0ms |
| SpecificExtraction | 2242ms |
| Compression | 114ms |
| **Total** | **48229ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| FileTreeExtractor | 16145ms | 0 | 0 |
| SolutionDiscovery | 3995ms | 0 | 0 |
| CallGraphExtractor | 1134ms | 0 | 0 |
| EndpointExtractor | 1101ms | 0 | 31 |
| SyntaxStructureExtractor | 777ms | 528 | 54 |
| DiRegistrationExtractor | 776ms | 1 | 54 |
| CliCommandExtractor | 607ms | 0 | 18 |
| InMemoryEventBusExtractor | 472ms | 0 | 22 |
| GrpcServiceExtractor | 471ms | 0 | 23 |
| ControllerActionExtractor | 454ms | 0 | 22 |
| MediatRExtractor | 447ms | 0 | 19 |
| GrpcClientExtractor | 446ms | 0 | 19 |
| DesktopEntryExtractor | 342ms | 0 | 15 |
| IndirectWiringDetector | 223ms | 0 | 13 |
| SourceBodyExtractor | 223ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 1210 | 506 |
| Sends | 7 | 0 |
| Resolves | 14 | 2 |

_351 files · 22 projects_
