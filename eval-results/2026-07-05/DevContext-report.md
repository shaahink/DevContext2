# REPORT
**DevContext**

Style: CleanArchitecture
_7 projects  ·  1 HttpEndpoint, 4 UiEntry, 23 GrpcService, 3 CliCommand  ·  net10.0, net10.0-windows10.0.19041.0 + blazor + controllers + desktop-ui + minimal-apis + serilog + grpc + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 314 |
| Projects | 22 |
| Nodes | 380 |
| Edges | 46 |
| Entries | 31 |
| With target | 31/31 |
| Verified edges | 89% |
| Analyzed in | 10.4s |

## Top Flows

1. **GET /health** → `Program` *(HttpEndpoint)*
2. **Proto.DevContextService.AnalysisCodeToGrpc** → `DevContextGrpcService` *(GrpcService)*
3. **Proto.DevContextService.Analyze** → `DevContextGrpcService` *(GrpcService)*
4. **Proto.DevContextService.CloseSession** → `DevContextGrpcService` *(GrpcService)*
5. **Proto.DevContextService.Error** → `DevContextGrpcService` *(GrpcService)*
6. **Proto.DevContextService.GetContext** → `DevContextGrpcService` *(GrpcService)*
7. **Proto.DevContextService.GetImpact** → `DevContextGrpcService` *(GrpcService)*
8. **Proto.DevContextService.GetInterestingPoints** → `DevContextGrpcService` *(GrpcService)*
9. **Proto.DevContextService.GetMap** → `DevContextGrpcService` *(GrpcService)*
10. **Proto.DevContextService.GetNeighbors** → `DevContextGrpcService` *(GrpcService)*

### Trace 1: GET /health

TRACE  GET /health
       src/DevContext.Server/Program.cs:38

▸ ENTRY  GET /health  (src/DevContext.Server/Program.cs:38)
   └─ call <lambda> GET /health  (src/DevContext.Server/Program.cs:38)
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: Proto.DevContextService.AnalysisCodeToGrpc

TRACE  Proto.DevContextService.AnalysisCodeToGrpc
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12
       DevContext.Server
▸ ENTRY  Proto.DevContextService.AnalysisCodeToGrpc  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
   └─ call DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
          public sealed class DevContextGrpcService(
          IAnalysisSessionManager sessions,
          ILogger<DevContextGrpcService> logger)

---

### Trace 3: Proto.DevContextService.Analyze

TRACE  Proto.DevContextService.Analyze
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12
       DevContext.Server
▸ ENTRY  Proto.DevContextService.Analyze  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
   └─ call DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
          public sealed class DevContextGrpcService(
          IAnalysisSessionManager sessions,
          ILogger<DevContextGrpcService> logger)

---

## Insights

_5 info · 4 notable · 1 warning_

### **WARNING**: Auth surface: 0 protected, 1 unannotated of 1 endpoints
*(Risk)*

- 1 no auth annotation

### **NOTABLE**: 1/1 endpoints anonymous
*(Risk)*

- GET /health

### **NOTABLE**: ViewModel-View: 4 VMs + 1 Views (0 call edges)
*(Wiring)*

- 4 ViewModels
- 1 Views

### **NOTABLE**: Downstream wiring: 6 target services detected
*(Wiring)*

- DevContextGrpcService
- MainViewModel
- QueryCommand
- ReportCommand
- AnalyzeCommand

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- NamespaceGrouper
- OrleansGrainEntryPointBuilder
- AspireRelationshipDetection
- MapRenderContext
- RenderedContext

### _INFO_: Command tree: 3 CLI commands, 3 top-level groups
*(Shape)*

- AnalyzeCommand (1 commands)
- QueryCommand (1 commands)
- ReportCommand (1 commands)

### _INFO_: Parameter inventory: ~1.0 params per command (avg)
*(Data)*

- 3 commands

### _INFO_: Entry targets resolved 31/31 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 4 feature areas
*(Shape)*

- Endpoints (23 entries)
- ViewModels (4 entries)
- Commands (3 entries)
- global (1 entries)

### _INFO_: Routing surface: 1 routes exposed
*(Shape)*

- GET /health

MAP  DevContext     (7 projects)

STACK  net10.0, net10.0-windows10.0.19041.0 · Minimal APIs · Controllers · MediatR (CQRS)

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure, Api, Core; MediatR with 2 handlers

TOPOLOGY (depends-on)
   DevContext.Core
   DevContext.Cli ── DevContext.Core
   DevContext.Contracts
   DevContext.Benchmarks ── DevContext.Cli, DevContext.Core
   DevContext.Desktop ── DevContext.Cli, DevContext.Core
   DevContext.Mcp ── DevContext.Cli, DevContext.Core
   DevContext.Server ── DevContext.Cli, DevContext.Contracts, DevContext.Core

ENTRY POINTS
   HTTP (1)
      GET /health  → Program  (src/DevContext.Server/Program.cs:38)
   UI (4)
      [RelayCommand] MainViewModel.AnalyzeAsync  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:319)
      [RelayCommand] MainViewModel.ResetToScenarioDefaults  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:601)
      [RelayCommand] MainViewModel.SelectRecent  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:314)
      [RelayCommand] MainViewModel.SetFormat  → MainViewModel  (src/DevContext.Desktop/ViewModels/MainViewModel.cs:311)
   gRPC (23)
      Proto.DevContextService.AnalysisCodeToGrpc  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.Analyze  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.CloseSession  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.Error  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetContext  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetImpact  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetInterestingPoints  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetMap  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetNeighbors  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetNode  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetStats  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.GetTrace  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.ListEntryPoints  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.MapException  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.ParseDetail  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.Ping  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.Render  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.Require  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.ResolveNode  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      Proto.DevContextService.RunAnalysisAsync  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
      … and 3 more (grpc entries — use --focus for a drill-in)
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
   Other:  BenchmarkDotNet 0.15.8, CommunityToolkit.Mvvm 8.*, Google.Protobuf 3.*, Grpc.Core.Api 2.*, Grpc.Net.Client 2.*, Grpc.Tools 2.*, LibGit2Sharp 0.30.*, Microsoft.CodeAnalysis.CSharp 5.3.0 … (18 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 6836ms |
| GenericExtraction | 549ms |
| SignalSealing | 0ms |
| SpecificExtraction | 2073ms |
| Compression | 63ms |
| **Total** | **10358ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| FileTreeExtractor | 4915ms | 0 | 0 |
| SolutionDiscovery | 1885ms | 0 | 0 |
| CallGraphExtractor | 1054ms | 0 | 0 |
| EndpointExtractor | 1016ms | 0 | 30 |
| SyntaxStructureExtractor | 546ms | 451 | 55 |
| DiRegistrationExtractor | 545ms | 0 | 55 |
| CliCommandExtractor | 506ms | 0 | 27 |
| ControllerActionExtractor | 344ms | 0 | 22 |
| InMemoryEventBusExtractor | 342ms | 0 | 19 |
| GrpcServiceExtractor | 332ms | 0 | 19 |
| MediatRExtractor | 305ms | 0 | 18 |
| DesktopEntryExtractor | 262ms | 0 | 15 |
| SourceBodyExtractor | 212ms | 0 | 0 |
| BlazorEntryExtractor | 169ms | 0 | 12 |
| ProgramCsFlowExtractor | 165ms | 0 | 7 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 31 | 0 |
| Raises | 3 | 3 |
| Resolves | 12 | 2 |

_314 files · 0 projects_
