Overview map (no focus).
Analyzing project...

MAP  DevContext     (5 projects)
SCOPE  analyzed DevContext.slnx - 1 of 6 solutions in this repo; analyze another
by naming its solution - style/topology are local to this slice, not the whole 
system

STACK  net10.0 · Minimal APIs

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 6 project(s); no MediatR

       per service:
         DevContext.Cli: CLI [CLI]
         DevContext.Mcp: Unknown
         DevContext.Server: gRPC Service [gRPC]

TOPOLOGY (depends-on)
   DevContext.Contracts
   DevContext.Core
   DevContext.Cli ── DevContext.Core
   DevContext.Mcp ── DevContext.Contracts
   DevContext.Server ── DevContext.Cli, DevContext.Contracts, DevContext.Core

CROSS-SERVICE
  gRPC (1)
    [gRPC] DevContext.Mcp  DevContext.Server  
(C:\Code\DevContext2-v11-before\src\DevContext.Mcp\DevContextTools.cs:54C:\Code
\DevContext2-v11-before\src\DevContext.Server\Endpoints\DevContextGrpcService.cs
:17)

ENTRY POINTS
   HTTP (1)
      GET /health   Program  (src/DevContext.Server/Program.cs:73)
   gRPC (26)
      DevContextService.Analyze   AnalysisOutcome.ConfigureAwait  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.CloseSession   AnalysisSessionManager.CloseSessionAsync
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ConfigLookup   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.FindTestsFor   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetContext   ContextPackBuilder.Build  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetContextPack   ContextPackBuilder.BuildMulti  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetFlowIndex   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetGraphFacets   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetImpact   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetInterestingPoints   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetMap   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetNeighbors   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetNode   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetSeam   NodeId  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetStats   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetTrace   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ListEntryPoints   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ListSessions   AnalysisSessionManager.ListSessions  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ObserveToolCalls   McpObservabilityService.Subscribe  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.Ping   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      . and 6 more (grpc entries - trace one for a drill-in)
   CLI (3)
      QueryCommand -settings QuerySettings   DiscoveryPipeline.AnalyzeAsync  
(src/DevContext.Cli/Commands/QueryCommand.cs:14)
      AnalyzeCommand -settings AnalyzeSettings  
(src/DevContext.Cli/Commands/AnalyzeCommand.cs:10)
      ReportCommand -settings ReportSettings  
(src/DevContext.Cli/Commands/ReportCommand.cs:11)

PACKAGES
   Web/API:  Grpc.AspNetCore 2.*, Grpc.AspNetCore.Web 2.*, 
Microsoft.AspNetCore.Mvc.Testing 10.0.*
   Logging:  Serilog 4.3.1, Serilog.Extensions.Logging 10.0.0, 
Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0
   Testing:  coverlet.collector 10.0.1, xunit 2.9.3, xunit.runner.visualstudio 
3.1.5, Xunit.SkippableFact 1.5.23
   Other:  BenchmarkDotNet 0.15.8, Google.Protobuf 3.*, Grpc.Core.Api 2.*, 
Grpc.Net.Client 2.*, Grpc.Net.Client.Web 2.*, Grpc.Tools 2.*, LibGit2Sharp 
0.30.*, Microsoft.CodeAnalysis.CSharp 5.3.0 . (16 total)

 drill in:  trace a focused entry   (e.g. trace "QueryCommand -settings 
QuerySettings")

analyzed 389 files · 1267 nodes · 1396 edges · 30 entries · 28/30 target · 
~1163 tokens · 14.1s stage2 x3.0 stage3 x2.2
┌──────────┬─────────────────────┐
│  Metric  │        Value        │
├──────────┼─────────────────────┤
│ Solution │     DevContext      │
│   Time   │       15184ms       │
│  Tokens  │ ~1163 (budget 8000) │
│ Version  │ v1.0.6-preview.0.65 │
└──────────┴─────────────────────┘
