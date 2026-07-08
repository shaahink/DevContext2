# REPORT
**BlazorWebAssemblyXrefGenerator**

Style: SampleCollection
_1 project  ·  2 HttpEndpoint  ·  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst + swagger + blazor + controllers + minimal-apis + identity + efcore + razor-pages + signalr + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 939 |
| Projects | 142 |
| Nodes | 50 |
| Edges | 29 |
| Entries | 2 |
| With target | 0/2 |
| Verified edges | 72% |
| Analyzed in | 13.2s |

## Top Flows

1. **GET /** *(HttpEndpoint)*
2. **GET /not-found** *(HttpEndpoint)*

### Trace 1: GET /

TRACE  GET /
       BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1
       BlazorWebAssemblyXrefGenerator
▸ ENTRY  GET /  (BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1)
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: GET /not-found

TRACE  GET /not-found
       BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1
       BlazorWebAssemblyXrefGenerator
▸ ENTRY  GET /not-found  (BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1)
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_3 info · 6 notable · 1 warning_

### **WARNING**: Auth surface: 0 protected, 2 unannotated of 2 API endpoints
*(Risk)*

- 2 no auth annotation

### **NOTABLE**: 2/2 endpoints anonymous
*(Risk)*

- GET /
- GET /not-found

### **NOTABLE**: Config without defaults: 2 consumed keys have no appsettings default
*(Risk)*

- APPLICATIONINSIGHTS_CONNECTION_STRING
- OTEL_EXPORTER_OTLP_ENDPOINT

### **NOTABLE**: Internal hubs: 1 heavily-referenced internal types
*(Topology)*

- LoginLogoutEndpointRouteBuilderExtensions (26 refs)

### **NOTABLE**: Extension seats: AddHttpClient (8 impls) · AddAuthentication (7 impls) · AddDbContext (4 impls)
*(Wiring)*

- AddHttpClient (8 impls)
- AddAuthentication (7 impls)
- AddDbContext (4 impls)

### **NOTABLE**: Most depended-upon: Aspire.ServiceDefaults (10 dependents) · MauiBlazorWeb.Shared (8 dependents) · BlazorWebAssemblySignalRApp.Shared (8 dependents)
*(Topology)*

- Aspire.ServiceDefaults (10 dependents)
- MauiBlazorWeb.Shared (8 dependents)
- BlazorWebAssemblySignalRApp.Shared (8 dependents)

### **NOTABLE**: Multi-implementation interfaces: IWeatherService (3 impls) · IMovieService (2 impls)
*(Wiring)*

- IWeatherService (3 impls)
- IMovieService (2 impls)

### _INFO_: Entry targets resolved 0/2 (0%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: DI: 538 Extension · 90 Scoped · 49 Singleton · 15 Transient (692 total)
*(Wiring)*

### _INFO_: Routing surface: 2 routes exposed
*(Shape)*

- GET /
- GET /not-found

MAP  BlazorWebAssemblyXrefGenerator     (1 project)

STACK  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst, net5.0, net6.0, net7.0, net8.0, net8.0-android;net8.0-ios;net8.0-maccatalyst, net9.0, net9.0-android;net9.0-ios;net9.0-maccatalyst, netcoreapp3.1, netstandard2.1 · Minimal APIs · Controllers · EF Core

STYLE  SampleCollection  (confidence moderate)
       evidence: 142 projects but .sln only covers 1 — multi-sample directory

       per service:
         BlazorSample: Unknown
         BlazorWebAppAreaOfStaticSsrComponents: Unknown
         BlazorWebAppMovies: Unknown [EF Core]
         BlazorWebAppSpreadOutStaticSsrComponents: Unknown
         BlazorWebAppWinAuthServer: Unknown
         WASMBrowserApp: Unknown
         BlazorSample: Unknown
         BlazorSample: Unknown
         BlazorSample: Unknown
         BlazorSample: Unknown
         BlazorSample: Unknown
         BlazorSample: Unknown
         BlazorWebAppMovies: Unknown [EF Core]
         WASMBrowserApp: Unknown
         BlazorSample: Unknown
         BlazorWebAppAreaOfStaticSsrComponents: Unknown
         BlazorWebAppMovies: Unknown [EF Core]
         BlazorWebAppSpreadOutStaticSsrComponents: Unknown
         BlazorWebAppWinAuthServer: Unknown
         WASMBrowserApp: Unknown
         BlazorSignalRApp: Unknown
         Backend: Unknown [EF Core]
         BlazorApp: Unknown
         BlazorWebAppEntra: Unknown
         MinimalApiJwt: Unknown
         BlazorWebAppEntra: Gateway [YARP]
         MinimalApiJwt: Unknown
         BlazorWebAppOidc: Unknown
         MinimalApiJwt: Unknown
         BlazorWebAppOidc: Gateway [YARP]
         MinimalApiJwt: Unknown
         BlazorWebAppOidcServer: Unknown
         MinimalApiJwt: Unknown
         Backend: Unknown [EF Core]
         Backend: Unknown [EF Core]
         QRGenerator: Unknown
         MauiBlazorWeb: Unknown
         MauiBlazorWeb.Web: Web App [EF Core]
         BlazorWebAssemblySignalRApp.Client: Unknown
         BlazorWebAssemblySignalRApp.Server: Unknown
         BlazorWebAssemblySignalRApp.Server: Unknown
         BlazorWebAssemblySignalRApp.Server: Unknown
         BlazorWebAssemblySignalRApp.Server: Unknown
         BlazorSignalRApp: Unknown
         Backend: Unknown [EF Core]
         BlazorApp: Unknown
         BlazorWebAppOidc: Unknown
         MinimalApiJwt: Unknown
         BlazorWebAppOidc: Gateway [YARP]
         MinimalApiJwt: Unknown
         BlazorWebAppOidcServer: Unknown
         MinimalApiJwt: Unknown
         Backend: Unknown [EF Core]
         Backend: Unknown [EF Core]
         MauiBlazorWeb.Maui: Unknown
         MauiBlazorWeb.Web: Web App
         BlazorSignalRApp: Unknown
         Backend: Unknown [EF Core]
         BlazorApp: Unknown
         BlazorWebAppEntra: Unknown
         MinimalApiJwt: Unknown
         BlazorWebAppEntra: Gateway [YARP]
         MinimalApiJwt: Unknown
         BlazorWebAppOidc: Unknown
         MinimalApiJwt: Unknown
         BlazorWebAppOidc: Gateway [YARP]
         MinimalApiJwt: Unknown
         BlazorWebAppOidcServer: Unknown
         MinimalApiJwt: Unknown
         Backend: Unknown [EF Core]
         Backend: Unknown [EF Core]
         MauiBlazorWeb: Unknown
         MauiBlazorWeb.Web: Web App
         BlazorApp: Unknown [EF Core]
         qr: Unknown
         BlazorApp: Unknown [EF Core]
         BlazorApp: Unknown [EF Core]
         MauiBlazorWeb: Unknown
         MauiBlazorWeb.Web: Web App [EF Core]

TOPOLOGY (depends-on)
   BlazorWebAssemblyXrefGenerator

ENTRY POINTS
   HTTP (2)
      GET /  (BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1)
      GET /not-found  (BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1)

PACKAGES
   Web/API:  Azure.Extensions.AspNetCore.DataProtection.Blobs 1.5.1, Azure.Extensions.AspNetCore.DataProtection.Keys 1.6.1, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0, Microsoft.AspNetCore.Authentication.Negotiate 10.0.0, Microsoft.AspNetCore.Authentication.OpenIdConnect 10.0.0, Microsoft.AspNetCore.Authorization 10.0.2, Microsoft.AspNetCore.Components.Authorization 10.0.2, Microsoft.AspNetCore.Components.CustomElements 10.0.0 … (27 total)
   ORM/Data:  Microsoft.EntityFrameworkCore.InMemory 10.0.0, Microsoft.EntityFrameworkCore.Sqlite 10.0.2, Microsoft.EntityFrameworkCore.SqlServer 10.0.0, Microsoft.EntityFrameworkCore.Tools 10.0.0
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.12.0, OpenTelemetry.Extensions.Hosting 1.12.0, OpenTelemetry.Instrumentation.Http 1.12.0, OpenTelemetry.Instrumentation.Runtime 1.12.0
   Other:  Aspire.Hosting.AppHost 9.5.0, Microsoft.Authentication.WebAssembly.Msal 10.0.0, Microsoft.Extensions.Azure 1.13.0, Microsoft.Extensions.Caching.Memory 10.0.0, Microsoft.Extensions.Http 10.0.0, Microsoft.Extensions.Http.Resilience 9.9.0, Microsoft.Extensions.Logging.Configuration 10.0.0, Microsoft.Extensions.Logging.Debug 10.0.0 … (18 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 821ms |
| GenericExtraction | 566ms |
| SignalSealing | 0ms |
| SpecificExtraction | 2980ms |
| Compression | 32ms |
| **Total** | **13245ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1929ms | 0 | 0 |
| EndpointExtractor | 1045ms | 0 | 2286 |
| BlazorEntryExtractor | 974ms | 0 | 2282 |
| SyntaxStructureExtractor | 562ms | 237 | 1043 |
| DiRegistrationExtractor | 556ms | 0 | 1043 |
| ProgramCsFlowExtractor | 530ms | 0 | 351 |
| FileTreeExtractor | 513ms | 0 | 0 |
| RazorPagesExtractor | 243ms | 0 | 277 |
| InMemoryEventBusExtractor | 218ms | 0 | 241 |
| EfCoreExtractor | 216ms | 0 | 241 |
| SignalRHubExtractor | 202ms | 0 | 223 |
| ControllerActionExtractor | 198ms | 0 | 202 |
| SolutionDiscovery | 176ms | 0 | 0 |
| BodyFactsExtractor | 170ms | 0 | 0 |
| SourceBodyExtractor | 166ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 28 | 8 |
| Resolves | 1 | 0 |

_939 files · 142 projects_
