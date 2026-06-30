MAP  Ocelot.Samples     (16 projects)

STACK  net8.0;net9.0;net10.0 · Minimal APIs · Controllers · FluentValidation

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, MinimalApi=yes(conf=0.8)

TOPOLOGY (depends-on)
   Ocelot
   Ocelot.Samples.Basic ── Ocelot, Ocelot.Samples.Web
   Ocelot.Samples.Configuration ── Ocelot, Ocelot.Samples.Web
   Ocelot.Samples.Eureka.ApiGateway ── Ocelot.Samples.Web
   Ocelot.Samples.Eureka.DownstreamService ── Ocelot.Samples.Web
   Ocelot.Samples.GraphQL ── Ocelot, Ocelot.Samples.Web
   Ocelot.Samples.Kubernetes.ApiGateway ── Ocelot.Samples.Web
   Ocelot.Samples.Kubernetes.DownstreamService ── Ocelot.Samples.Web
   Ocelot.Samples.Metadata ── Ocelot, Ocelot.Samples.Web
   Ocelot.Samples.OpenTracing ── Ocelot.Samples.Web
   Ocelot.Samples.ServiceDiscovery.ApiGateway ── Ocelot, Ocelot.Samples.Web
   Ocelot.Samples.ServiceDiscovery.DownstreamService ── Ocelot.Samples.Web
   Ocelot.Samples.ServiceFabric.ApiGateway ── Ocelot, Ocelot.Samples.Web
   Ocelot.Samples.ServiceFabric.DownstreamService ── Ocelot.Samples.Web
   Ocelot.Samples.Web
   Ocelot.Samples.WebSocket ── Ocelot, Ocelot.Samples.Web

ENTRY POINTS
   HTTP (3)
      DELETE /outputcache/{region}  → DefaultMemoryCache.ClearRegion  (src/Administration/OutputCacheController.cs:20)
      POST /configuration  → FileAndInternalConfigurationSetter.SetAsync  (src/Administration/FileConfigurationController.cs:41)
      GET /configuration  (src/Administration/FileConfigurationController.cs:23)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Authentication.JwtBearer 10.0.9, Microsoft.AspNetCore.MiddlewareAnalysis 10.0.9, Microsoft.AspNetCore.Mvc.NewtonsoftJson 10.0.9, Microsoft.AspNetCore.TestHost 10.0.9, Microsoft.ServiceFabric.AspNetCore.Kestrel 8.4.268, Serilog.AspNetCore 10.0.0, Swashbuckle.AspNetCore 10.2.1
   Validation:  FluentValidation 12.1.1
   Logging:  Serilog 4.3.1
   Testing:  coverlet.MTP 10.0.1, Moq 4.20.72, Shouldly 4.3.0, xunit.v3.mtp-v2 4.0.0-pre.128
   Utilities:  GraphQL.NewtonsoftJson 8.8.4, Ocelot.QualityOfService.Polly 25.0.0-beta.2
   Other:  BenchmarkDotNet 0.15.8, GraphQL 8.8.4, IPAddressRange 6.3.0, Jaeger 1.0.3, Microsoft.Extensions.Caching.Memory 10.0.9, Microsoft.Extensions.Configuration.EnvironmentVariables 10.0.9, Microsoft.Extensions.Configuration.FileExtensions 10.0.9, Microsoft.Extensions.Configuration.Json 10.0.9 … (26 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
