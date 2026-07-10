# REPORT
**BlazorWebAssemblyXrefGenerator**

Style: Microservices
_1 project  ·  2 HttpEndpoint  ·  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst + swagger + blazor + controllers + minimal-apis + identity + efcore + razor-pages + signalr + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 939 |
| Projects | 142 |
| Nodes | 360 |
| Edges | 1 |
| Entries | 2 |
| With target | 0/2 |
| Verified edges | 100% |
| Analyzed in | 24.9s |

## Top Flows

1. **GET /** *(HttpEndpoint)*
2. **GET /not-found** *(HttpEndpoint)*

### Trace 1: GET /

TRACE  GET /
       BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1

▸ ENTRY  GET /  (BlazorWebAssemblyXrefGenerator/Pages/Home.razor:1)
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: GET /not-found

TRACE  GET /not-found
       BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1

▸ ENTRY  GET /not-found  (BlazorWebAssemblyXrefGenerator/Pages/NotFound.razor:1)
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_3 info · 6 notable · 1 warning_

### **WARNING**: Auth surface: 0 protected, 2 unannotated of 2 endpoints
*(Risk)*

- 2 no auth annotation

### **NOTABLE**: 2/2 endpoints anonymous
*(Risk)*

- GET /not-found
- GET /

### **NOTABLE**: Possible dead code: 4 public types with zero inbound references
*(Wiring)*

- Migrations
- BlazorApp.Models.MovieContext
- Backend.Models.TodoContext
- BlazorWebAppMovies.Data.BlazorWebAppMoviesContext

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

### _INFO_: Routing surface: 2 routes exposed
*(Shape)*

- GET /not-found
- GET /

### _INFO_: Public surface: 14 interfaces, 191 classes (205 total public types)
*(Shape)*

- 14 interfaces
- 191 classes

MAP  BlazorWebAssemblyXrefGenerator     (1 project)

STACK  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst, net5.0, net6.0, net7.0, net8.0, net8.0-android;net8.0-ios;net8.0-maccatalyst, net9.0, net9.0-android;net9.0-ios;net9.0-maccatalyst, netcoreapp3.1, netstandard2.1 · Minimal APIs · Controllers · EF Core

STYLE  Microservices  (confidence high)
       evidence: Aspire orchestration with 124 service projects

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
| DiscoveryAndCacheWarmup | 2538ms |
| GenericExtraction | 2837ms |
| SignalSealing | 0ms |
| SpecificExtraction | 19021ms |
| Compression | 29ms |
| **Total** | **24889ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| BlazorEntryExtractor | 17029ms | 0 | 2286 |
| SyntaxStructureExtractor | 2833ms | 237 | 1043 |
| DiRegistrationExtractor | 2824ms | 0 | 1043 |
| ProgramCsFlowExtractor | 2677ms | 0 | 351 |
| CallGraphExtractor | 1987ms | 0 | 0 |
| ProjectStructure | 1804ms | 0 | 0 |
| EndpointExtractor | 1025ms | 0 | 243 |
| FileTreeExtractor | 552ms | 0 | 0 |
| RazorPagesExtractor | 497ms | 0 | 110 |
| EfCoreExtractor | 307ms | 0 | 102 |
| ControllerActionExtractor | 288ms | 0 | 95 |
| InMemoryEventBusExtractor | 248ms | 0 | 87 |
| SignalRHubExtractor | 223ms | 0 | 64 |
| IndirectWiringDetector | 218ms | 0 | 64 |
| SolutionDiscovery | 178ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Resolves | 1 | 0 |

_939 files · 0 projects_
