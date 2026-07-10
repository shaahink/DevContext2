Overview map (no focus).
Analyzing project...

MAP  DevContext     (6 projects)

STACK  net10.0, net10.0-windows10.0.19041.0 ú Minimal APIs ú Controllers ú 
MediatR (CQRS)

STYLE  ModularMonolith  (confidence high)
       evidence: 9 module-like sub-projects: devcontext.benchmarks, 
devcontext.cli, devcontext.contracts, devcontext.core, devcontext.desktop, 
devcontext.server, devcontext.core.tests, devcontext.desktop.tests, 
devcontext.server.tests

TOPOLOGY (depends-on)
   DevContext.Core
   DevContext.Cli ÄÄ DevContext.Core
   DevContext.Contracts
   DevContext.Benchmarks ÄÄ DevContext.Cli, DevContext.Core
   DevContext.Desktop ÄÄ DevContext.Cli, DevContext.Core
   DevContext.Server ÄÄ DevContext.Cli, DevContext.Contracts, DevContext.Core

ENTRY POINTS
   HTTP (1)
      GET /health   Program  (src/DevContext.Server/Program.cs:36)
   UI (4)
      [RelayCommand] MainViewModel.AnalyzeAsync   MainViewModel  
(src/DevContext.Desktop/ViewModels/MainViewModel.cs:319)
      [RelayCommand] MainViewModel.ResetToScenarioDefaults   MainViewModel  
(src/DevContext.Desktop/ViewModels/MainViewModel.cs:601)
      [RelayCommand] MainViewModel.SelectRecent   MainViewModel  
(src/DevContext.Desktop/ViewModels/MainViewModel.cs:314)
      [RelayCommand] MainViewModel.SetFormat   MainViewModel  
(src/DevContext.Desktop/ViewModels/MainViewModel.cs:311)
   gRPC (1)
      Proto.DevContextService.DevContextGrpcService (20 methods: Analyze, 
RunAnalysisAsync, CloseSession)   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:12)
   CLI (5)
      AnalyzeCommand -settings AnalyzeSettings   AnalyzeCommand  
(src/DevContext.Cli/Commands/AnalyzeCommand.cs:10)
      InitCommand -settings object   InitCommand  
(src/DevContext.Cli/Commands/InitCommand.cs:3)
      QueryCommand -settings QuerySettings   QueryCommand  
(src/DevContext.Cli/Commands/QueryCommand.cs:11)
      ScenariosCommand -settings object   ScenariosCommand  
(src/DevContext.Cli/Commands/ScenariosCommand.cs:3)
      VersionCommand -settings object   VersionCommand  
(src/DevContext.Cli/Commands/VersionCommand.cs:3)

PACKAGES
   Web/API:  Grpc.AspNetCore 2.*, Grpc.AspNetCore.Web 2.*, 
Microsoft.AspNetCore.Components.WebView.Wpf 10.*, 
Microsoft.AspNetCore.Mvc.Testing 10.0.*, Microsoft.AspNetCore.OpenApi 10.0.0
   ORM/Data:  Dapper 2.1.35, Microsoft.EntityFrameworkCore 
10.0.0-preview.3.25171.5
   Mediator/CQRS:  MediatR 12.0.0
   Validation:  FluentValidation 11.5.0
   Logging:  Serilog 4.3.1, Serilog.Extensions.Logging 10.0.0, 
Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.*
   Testing:  coverlet.collector 10.0.1, NSubstitute 5.*, xunit 2.9.3, 
xunit.runner.visualstudio 3.1.5
   Other:  BenchmarkDotNet 0.15.8, CommunityToolkit.Mvvm 8.*, Google.Protobuf 
3.*, Grpc.Core.Api 2.*, Grpc.Net.Client 2.*, Grpc.Tools 2.*, LibGit2Sharp 
0.30.*, Microsoft.CodeAnalysis.CSharp 5.3.0 . (17 total)

 drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 294 files ú 331 nodes ú 25 edges ú 11 entries ú 11/11 target ú ~744 
tokens ú 12.9s stage2 x2.6 stage3 x2.2

                                    Insights                                    
ÚÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Sev  ³ Category ³ Title                        ³ Evidence                    ³
ÃÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ NOTE ³ Risk     ³ 1/1 endpoints anonymous      ³ GET /health                 ³
³ NOTE ³ Wiring   ³ Possible dead code: 5 public ³ RunRow, SolutionInfo,       ³
³      ³          ³ types with zero inbound      ³ BlazorEntryExtractor        ³
³      ³          ³ references                   ³                             ³
³ NOTE ³ Wiring   ³ Multi-implementation         ³ ? (6 impls), " (4 impls),   ³
³      ³          ³ interfaces: ? (6 impls) ú "  ³ rootPath (2 impls)          ³
³      ³          ³ (4 impls) ú rootPath (2      ³                             ³
³      ³          ³ impls)                       ³                             ³
³ INFO ³ Coverage ³ Entry targets resolved 11/11 ³                             ³
³      ³          ³ (100%) - use --focus for     ³                             ³
³      ³          ³ deeper traces                ³                             ³
³ INFO ³ Wiring   ³ DI: 20 Singleton ú 16        ³                             ³
³      ³          ³ Extension ú 2 Scoped (38     ³                             ³
³      ³          ³ total)                       ³                             ³
³ INFO ³ Shape    ³ Entry surface: 5 CLI ú 4 UI  ³ 5 CLI, 4 UI, 1 HTTP         ³
³      ³          ³ ú 1 HTTP ú 1 GrpcService     ³                             ³
³ INFO ³ Topology ³ Most depended-upon: Core (4  ³ Core (4 dependents),        ³
³      ³          ³ dependents) ú                ³ DevContext.Core (3          ³
³      ³          ³ DevContext.Core (3           ³ dependents),                ³
³      ³          ³ dependents) ú                ³ DevContext.Core (2          ³
³      ³          ³ DevContext.Core (2           ³ dependents)                 ³
³      ³          ³ dependents)                  ³                             ³
ÀÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
                           Stage Timing                            
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Stage                   ³    Time ³ Bar                         ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ DiscoveryAndCacheWarmup ³  9016ms ³ ÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛ ³
³ GenericExtraction       ³   658ms ³ ÛÛ                          ³
³ SignalSealing           ³     1ms ³                             ³
³ SpecificExtraction      ³  2229ms ³ ÛÛÛÛÛÛ                      ³
³ Compression             ³    60ms ³                             ³
³ Total                   ³ 12935ms ³                             ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

                                   Extractors                                   
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Name                     ³   Time ³ +Types ³ +Dets ³ Status                  ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ FileTreeExtractor        ³ 6456ms ³      0 ³     0 ³ ran                     ³
³ SolutionDiscovery        ³ 2351ms ³      0 ³     0 ³ ran                     ³
³ CallGraphExtractor       ³ 1320ms ³      0 ³     0 ³ ran                     ³
³ EndpointExtractor        ³  905ms ³      0 ³    31 ³ ran                     ³
³ SyntaxStructureExtractor ³  654ms ³    418 ³    45 ³ ran                     ³
³ DiRegistrationExtractor  ³  653ms ³      0 ³    45 ³ ran                     ³
³ SourceBodyExtractor      ³  397ms ³      0 ³     0 ³ ran                     ³
³ MediatRExtractor         ³  384ms ³      0 ³    28 ³ ran                     ³
³ InMemoryEventBusExtracto ³  346ms ³      0 ³    25 ³ ran                     ³
³ r                        ³        ³        ³       ³                         ³
³ ProgramCsFlowExtractor   ³  346ms ³      0 ³     7 ³ ran                     ³
³ GrpcServiceExtractor     ³  332ms ³      0 ³    25 ³ ran                     ³
³ CliCommandExtractor      ³  325ms ³      0 ³    24 ³ ran                     ³
³ ControllerActionExtracto ³  323ms ³      0 ³    24 ³ ran                     ³
³ r                        ³        ³        ³       ³                         ³
³ DesktopEntryExtractor    ³  310ms ³      0 ³    25 ³ ran                     ³
³ IndirectWiringDetector   ³  253ms ³      0 ³    16 ³ ran                     ³
³ ProjectStructure         ³  186ms ³      0 ³     0 ³ ran                     ³
³ BlazorEntryExtractor     ³   31ms ³      0 ³     0 ³ ran                     ³
³ DependencyExtractor      ³   20ms ³      0 ³     0 ³ ran                     ³
³ LayerClassifier          ³   17ms ³      0 ³     0 ³ ran                     ³
³ AntiPatternDetector      ³    0ms ³      0 ³     0 ³ skipped: gated by       ³
³                          ³        ³        ³       ³ ShouldRun               ³
³ AspireExtractor          ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs aspire            ³
³ AwsLambdaExtractor       ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs aws-lambda        ³
³ AzureFunctionsExtractor  ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs azure-functions   ³
³ EfCoreExtractor          ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs efcore            ³
³ EventBusExtractor        ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs masstransit or    ³
³                          ³        ³        ³       ³ nservicebus             ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

         Graph Seams         
ÚÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄ¿
³ Seam     ³ Edges ³ Approx ³
ÃÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄ´
³ Calls    ³    11 ³      0 ³
³ Raises   ³     3 ³      3 ³
³ Resolves ³    11 ³      2 ³
ÀÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÙ
331 nodes ú 25 edges ú 11/11 entries  target
cache 86% hit ú 294 files ú 0 projects
ÚÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³  Metric  ³        Value         ³
ÃÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ Solution ³   DevContext.slnx    ³
³   Time   ³       13567ms        ³
³  Tokens  ³  ~744 (budget 8000)  ³
³ Version  ³ v1.0.5-preview.0.244 ³
ÀÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
