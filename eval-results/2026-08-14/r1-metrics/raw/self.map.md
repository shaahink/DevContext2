MAP  DevContext     (5 projects)
SCOPE  analyzed DevContext.slnx - 1 of 14 solutions in this repo; analyze 
another by naming its solution - style/topology are local to this slice, not the
whole system

STACK  net10.0 ú Minimal APIs

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 6 project(s); no MediatR

       per service:
         DevContext.Cli: CLI [CLI]
         DevContext.Mcp: Unknown
         DevContext.Server: gRPC Service [gRPC]

TOPOLOGY (depends-on)
   DevContext.Contracts
   DevContext.Core
   DevContext.Cli ÄÄ DevContext.Core
   DevContext.Mcp ÄÄ DevContext.Contracts
   DevContext.Server ÄÄ DevContext.Cli, DevContext.Contracts, DevContext.Core

CROSS-SERVICE
  gRPC (1)
    [gRPC] DevContext.Mcp  DevContext.Server  
(C:\Code\DevContext2-engine\src\DevContext.Mcp\DevContextTools.cs:54C:\Code\Dev
Context2-engine\src\DevContext.Server\Endpoints\DevContextGrpcService.cs:17)

ENTRY POINTS
   HTTP (1)
      GET /health   Program  (src/DevContext.Server/Program.cs:73)
   gRPC (26)
      DevContextService.Analyze   ProtoMapper.ToSummary  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.CloseSession   AnalysisSessionManager.CloseSessionAsync
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ConfigLookup   DevContextGrpcService  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.FindTestsFor   TestHeuristics.IsLikelyTestMethod  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetContext   ContextPackBuilder.Build  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetContextPack   ContextPackBuilder.BuildMulti  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetFlowIndex   ProtoMapper.ToFlowIndexResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetGraphFacets   ServiceMapProjection.Project  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetImpact   ProtoMapper.ToImpactResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetInterestingPoints   
ProtoMapper.ToInterestingPointsResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetMap   ProtoMapper.ToMapResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetNeighbors   ProtoMapper.ToNeighborsResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetNode   ProtoMapper.ToNodeResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetSeam   ProtoMapper.ToSeamResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetStats   ProtoMapper.ToStatsResponse  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetTrace   EntryPointResolver.Resolve  
(src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ListEntryPoints   ProtoMapper.ToProto  
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

