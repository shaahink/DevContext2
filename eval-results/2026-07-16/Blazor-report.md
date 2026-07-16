# REPORT
**BlazorWebAssemblyXrefGenerator**

Style: SampleCollection
_1 project  ·  2 UiEntry  ·  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst + swagger + blazor + controllers + minimal-apis + identity + efcore + razor-pages + signalr + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 939 |
| Projects | 142 |
| Nodes | 52 |
| Edges | 30 |
| Entries | 2 |
| With target | 0/2 |
| Deep spine (>=2) | 0/2 (0%) |
| Verified edges | 70% |
| Analyzed in | 12.3s |

## Top Flows

1. **GET /** *(UiEntry)*
2. **GET /not-found** *(UiEntry)*

### Trace 1: GET /

TRACE  GET /
       BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1
       BlazorWebAssemblyXrefGenerator
▸ ENTRY  GET /  (BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1)

---

### Trace 2: GET /not-found

TRACE  GET /not-found
       BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1
       BlazorWebAssemblyXrefGenerator
▸ ENTRY  GET /not-found  (BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1)

---

## Insights

_3 info · 3 notable_

### **NOTABLE**: Config without defaults: 2 consumed keys have no appsettings default
*(Risk)*

- APPLICATIONINSIGHTS_CONNECTION_STRING
- OTEL_EXPORTER_OTLP_ENDPOINT

### **NOTABLE**: Most depended-upon: Aspire.ServiceDefaults (10 dependents) · MauiBlazorWeb.Shared (8 dependents) · BlazorWebAssemblySignalRApp.Shared (8 dependents)
*(Topology)*

- Aspire.ServiceDefaults (10 dependents)
- MauiBlazorWeb.Shared (8 dependents)
- BlazorWebAssemblySignalRApp.Shared (8 dependents)

### **NOTABLE**: Multi-implementation interfaces: IWeatherService (3 impls) · IMovieService (2 impls)
*(Wiring)*

- IWeatherService (3 impls)
- IMovieService (2 impls)

### _INFO_: Entry targets resolved 0/2 (0%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 538 Extension · 90 Scoped · 49 Singleton · 15 Transient (692 total)
*(Wiring)*

### _INFO_: Wiring hubs: Console (6) · MovieContext (5)
*(Wiring)*

- Console (6)
- MovieContext (5)

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
         MauiBlazorWeb: MAUI App [.NET MAUI]
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
         MauiBlazorWeb.Maui: MAUI App [.NET MAUI]
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
         MauiBlazorWeb: MAUI App [.NET MAUI]
         MauiBlazorWeb.Web: Web App
         BlazorApp: Unknown [EF Core]
         Aspire.AppHost: Aspire AppHost [Aspire]
         Aspire.AppHost: Aspire AppHost [Aspire]
         qr: Unknown
         BlazorApp: Unknown [EF Core]
         Aspire.AppHost: Aspire AppHost [Aspire]
         BlazorApp: Unknown [EF Core]
         Aspire.AppHost: Aspire AppHost [Aspire]
         Aspire.AppHost: Aspire AppHost [Aspire]
         MauiBlazorWeb: MAUI App [.NET MAUI]
         MauiBlazorWeb.Web: Web App [EF Core]

TOPOLOGY (depends-on)
   BlazorWebAssemblyXrefGenerator

ENTRY POINTS
   UI (2)
      GET /  (BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1)
      GET /not-found  (BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1)

PACKAGES
   Web/API:  Azure.Extensions.AspNetCore.DataProtection.Blobs 1.5.1, Azure.Extensions.AspNetCore.DataProtection.Keys 1.6.1, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0, Microsoft.AspNetCore.Authentication.Negotiate 10.0.0, Microsoft.AspNetCore.Authentication.OpenIdConnect 10.0.0, Microsoft.AspNetCore.Authorization 10.0.2, Microsoft.AspNetCore.Components.Authorization 10.0.2, Microsoft.AspNetCore.Components.CustomElements 10.0.0 … (27 total)
   ORM/Data:  Microsoft.EntityFrameworkCore.InMemory 10.0.0, Microsoft.EntityFrameworkCore.Sqlite 10.0.2, Microsoft.EntityFrameworkCore.SqlServer 10.0.0, Microsoft.EntityFrameworkCore.Tools 10.0.0
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.12.0, OpenTelemetry.Extensions.Hosting 1.12.0, OpenTelemetry.Instrumentation.Http 1.12.0, OpenTelemetry.Instrumentation.Runtime 1.12.0
   Other:  Aspire.Hosting.AppHost 9.5.0, Microsoft.Authentication.WebAssembly.Msal 10.0.0, Microsoft.Extensions.Azure 1.13.0, Microsoft.Extensions.Caching.Memory 10.0.0, Microsoft.Extensions.Http 10.0.0, Microsoft.Extensions.Http.Resilience 9.9.0, Microsoft.Extensions.Logging.Configuration 10.0.0, Microsoft.Extensions.Logging.Debug 10.0.0 … (18 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "GET /not-found")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 1334ms |
| GenericExtraction | 677ms |
| SignalSealing | 0ms |
| SpecificExtraction | 3138ms |
| Compression | 34ms |
| **Total** | **12341ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| BlazorEntryExtractor | 1623ms | 0 | 2286 |
| CallGraphExtractor | 1509ms | 0 | 0 |
| EndpointExtractor | 1014ms | 0 | 1293 |
| FileTreeExtractor | 829ms | 0 | 0 |
| ProgramCsFlowExtractor | 673ms | 237 | 1081 |
| SyntaxStructureExtractor | 602ms | 237 | 744 |
| DiRegistrationExtractor | 594ms | 0 | 708 |
| RazorPagesExtractor | 325ms | 0 | 175 |
| SolutionDiscovery | 296ms | 0 | 0 |
| SignalRHubExtractor | 231ms | 0 | 107 |
| ControllerActionExtractor | 216ms | 0 | 104 |
| EfCoreExtractor | 210ms | 0 | 103 |
| ProjectStructure | 202ms | 0 | 0 |
| InMemoryEventBusExtractor | 199ms | 0 | 101 |
| BodyFactsExtractor | 148ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 29 | 9 |
| Resolves | 1 | 0 |

_939 files · 142 projects_
