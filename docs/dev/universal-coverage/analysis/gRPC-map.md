MAP  Grpc.DotNet     (30 projects)

STACK  net10.0, net10.0;net9.0;net8.0;net462, net4.7.2, net462;net8.0;net9.0;net10.0, net462;netstandard2.0, net462;netstandard2.0;net8.0;net9.0;net10.0, net462;netstandard2.0;netstandard2.1, net462;netstandard2.0;netstandard2.1;net8.0;net9.0;net10.0, net8.0, net8.0;net9.0;net10.0, net9.0, netstandard2.0, netstandard2.0;netstandard2.1;net8.0;net9.0;net10.0 · Minimal APIs · Controllers

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, MinimalApi=yes(conf=0.8)

TOPOLOGY (depends-on)
   Grpc.AspNetCore.Server ── Grpc.Net.Common
   Grpc.Net.Client ── Grpc.Net.Common
   Grpc.Core.Api
   Grpc.AspNetCore ── Grpc.AspNetCore.Server.ClientFactory
   Grpc.AspNetCore.Web
   Grpc.Net.Common ── Grpc.Core.Api
   Grpc.HealthCheck ── Grpc.Core.Api
   Grpc.Net.Client.Web
   Grpc.Net.ClientFactory ── Grpc.Net.Client
   Grpc.AspNetCore.Server.ClientFactory ── Grpc.AspNetCore.Server, Grpc.Net.ClientFactory
   Grpc.AspNetCore.Server.Reflection ── Grpc.AspNetCore.Server, Grpc.Reflection
   Grpc.Auth ── Grpc.Core.Api
   Grpc.Reflection ── Grpc.Core.Api
   InteropTestsGrpcWebClient ── Grpc.Net.Client, Grpc.Net.Client.Web
   BenchmarkWorkerWebsite ── Grpc.AspNetCore
   dotnet-grpc
   FunctionalTestsWebsite ── Grpc.AspNetCore, Grpc.AspNetCore.Web, Grpc.HealthCheck
   Grpc.AspNetCore.HealthChecks ── Grpc.AspNetCore.Server, Grpc.HealthCheck
   Grpc.AspNetCore.Microbenchmarks ── Grpc.AspNetCore
   Grpc.StatusProto ── Grpc.Core.Api
   GrpcAspNetCoreServer ── Grpc.AspNetCore.Server, Grpc.AspNetCore.Web
   GrpcClient ── Grpc.Net.Client
   GrpcCoreServer ── Grpc.Net.Common
   InteropTestsClient ── Grpc.Auth, Grpc.Net.Client, Grpc.Net.Client.Web
   InteropTestsGrpcWebWebsite ── InteropTestsGrpcWebClient
   InteropTestsNativeServer
   InteropTestsWebsite ── Grpc.AspNetCore, Grpc.AspNetCore.Web
   LinkerTestsClient ── Grpc.Net.Client, Grpc.Net.ClientFactory
   LinkerTestsWebsite ── Grpc.AspNetCore.Server
   QpsWorker ── Grpc.AspNetCore.Server, Grpc.AspNetCore.Server.Reflection, Grpc.Net.Client

PACKAGES
   Web/API:  Grpc.AspNetCore, Grpc.AspNetCore.HealthChecks, Grpc.AspNetCore.Server, Grpc.AspNetCore.Server.Reflection, Grpc.AspNetCore.Web, Microsoft.AspNetCore.Authentication.Certificate, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Components.WebAssembly … (15 total)
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.Http
   Testing:  Moq, NUnit, NUnit3TestAdapter
   Utilities:  Newtonsoft.Json
   Other:  BenchmarkDotNet, CommandLineParser, Google.Api.CommonProtos, Google.Apis.Auth, Google.Protobuf, Grpc.Auth, Grpc.Core, Grpc.Core.Api … (32 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
