# REPORT
**Clean.Architecture**

Style: VerticalSlices
_6 projects  ·  5 HttpEndpoint, 2 DomainEventHandler  ·  net10.0, net9.0 + minimal-apis + mediatr + serilog + efcore + aspire + fast-endpoints_

## Stats

| Metric | Value |
|--------|-------|
| Files | 362 |
| Projects | 22 |
| Nodes | 328 |
| Edges | 156 |
| Entries | 7 |
| With target | 7/7 |
| Deep spine (>=2) | 7/7 (100%) |
| Verified edges | 57% |
| Analyzed in | 9.0s |

## Top Flows

1. **GET /Contributors** → `ListContributorsQuery` *(HttpEndpoint)*
2. **DELETE /Contributors/{ContributorId:int}** → `Delete` *(HttpEndpoint)*
3. **GET /Contributors/{ContributorId:int}** → `GetById` *(HttpEndpoint)*
4. **POST /Contributors** → `Create` *(HttpEndpoint)*
5. **PUT /Contributors/{ContributorId:int}** → `Update` *(HttpEndpoint)*
6. **ContributorDeletedHandler** → `ContributorDeletedHandler` *(DomainEventHandler)*
7. **ContributorNameUpdatedEmailNotificationHandler** → `ContributorNameUpdatedEmailNotificationHandler` *(DomainEventHandler)*

### Trace 1: GET /Contributors

TRACE  GET /Contributors
       src/Clean.Architecture.Web/Contributors/List.cs:14
       Clean.Architecture.Web
▸ ENTRY  GET /Contributors  (src/Clean.Architecture.Web/Contributors/List.cs:14)
   └─ call List.HandleAsync  (src/Clean.Architecture.Web/Contributors/List.cs:14)
          public override async Task HandleAsync(ListContributorsRequest request, CancellationToken cancellationToken)
          var result = await _mediator.Send(new ListContributorsQuery(request.Page, request.PerPage));
          if (!result.IsSuccess)
      ├─ send ListContributorsQuery  (src/Clean.Architecture.Web/Contributors/List.cs:51) [verified]
      │      public record ListContributorsQuery(int? Page = 1, int? PerPage = Constants.DEFAULT_PAGE_SIZE)
      │      : IQuery<Result<PagedResult<ContributorDto>>>;
      └─ call AddLinkHeader  (src/Clean.Architecture.Web/Contributors/List.cs:59) [verified]
             private void AddLinkHeader(int page, int perPage, int totalPages)
             var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
             string Link(string rel, int p) => $"<{baseUrl}?page={p}&per_page={perPage}>; rel=\"{rel}\"";
         ├─ call List  (src/Clean.Architecture.Web/Contributors/List.cs:73) [verified]
         │      public class List(IMediator mediator) : Endpoint<ListContributorsRequest, ContributorListResponse, ListContributorsMapper>
         │      private readonly IMediator _mediator = mediator;
         │      public override void Configure()
         │  ├─ send ListContributorsQuery  (src/Clean.Architecture.Web/Contributors/List.cs:51) [verified]
         │  │      public record ListContributorsQuery(int? Page = 1, int? PerPage = Constants.DEFAULT_PAGE_SIZE)
         │  │      : IQuery<Result<PagedResult<ContributorDto>>>;
         │  └─ call AddLinkHeader  (src/Clean.Architecture.Web/Contributors/List.cs:59) [verified]
         │         private void AddLinkHeader(int page, int perPage, int totalPages)
         │         var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
         │         string Link(string rel, int p) => $"<{baseUrl}?page={p}&per_page={perPage}>; rel=\"{rel}\"";
         │     (stopped at depth 4; 3 branches omitted)
         ├─ call List.Link  (src/Clean.Architecture.Web/Contributors/List.cs:79) [verified]
         └─ call List.Add  (src/Clean.Architecture.Web/Contributors/List.cs:79) [verified]
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: DELETE /Contributors/{ContributorId:int}

TRACE  DELETE /Contributors/{ContributorId:int}
       src/Clean.Architecture.Web/Contributors/Delete.cs:19
       Clean.Architecture.Web
▸ ENTRY  DELETE /Contributors/{ContributorId:int}  (src/Clean.Architecture.Web/Contributors/Delete.cs:19)
   └─ call Delete.HandleAsync  (src/Clean.Architecture.Web/Contributors/Delete.cs:19)
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

### Trace 3: GET /Contributors/{ContributorId:int}

TRACE  GET /Contributors/{ContributorId:int}
       src/Clean.Architecture.Web/Contributors/GetById.cs:18
       Clean.Architecture.Web
▸ ENTRY  GET /Contributors/{ContributorId:int}  (src/Clean.Architecture.Web/Contributors/GetById.cs:18)
   └─ call GetById.HandleAsync  (src/Clean.Architecture.Web/Contributors/GetById.cs:18)
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_5 info · 3 notable · 1 warning_

### **WARNING**: 5/5 endpoints anonymous, incl. 3 POST/PUT/DELETE
*(Risk)*

- PUT /Contributors/{ContributorId:int}
- GET /Contributors
- GET /Contributors/{ContributorId:int}
- DELETE /Contributors/{ContributorId:int}
- POST /Contributors

### **NOTABLE**: Config without defaults: 2 consumed keys have no appsettings default
*(Risk)*

- APPLICATIONINSIGHTS_CONNECTION_STRING
- OTEL_EXPORTER_OTLP_ENDPOINT

### **NOTABLE**: Multi-implementation interfaces: IEmailSender (2 impls) · IListProjectsShallowQueryService (2 impls) · IListIncompleteItemsQueryService (2 impls)
*(Wiring)*

- IEmailSender (2 impls)
- IListProjectsShallowQueryService (2 impls)
- IListIncompleteItemsQueryService (2 impls)

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- ContributorNameUpdatedEvent
- PagedResult
- ContributorStatus
- GetContributorByIdRequest
- UpdateContributorValidator

### _INFO_: Entry targets resolved 7/7 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 52 Extension · 33 Scoped · 1 Singleton (86 total)
*(Wiring)*

### _INFO_: Entry surface: 5 HTTP · 2 DomainEventHandler
*(Shape)*

- 5 HTTP
- 2 DomainEventHandler

### _INFO_: Most depended-upon: Clean.Architecture.ServiceDefaults (2 dependents) · Clean.Architecture.Core (2 dependents) · Clean.Architecture.UseCases (2 dependents)
*(Topology)*

- Clean.Architecture.ServiceDefaults (2 dependents)
- Clean.Architecture.Core (2 dependents)
- Clean.Architecture.UseCases (2 dependents)

### _INFO_: Data map: 9 entities across 2 scopes
*(Data)*

- AppDbContext (1 entities)
- Clean.Architecture.Core (1 entities)

MAP  Clean.Architecture     (6 projects)

STACK  net10.0, net9.0 · Minimal APIs · FastEndpoints · MediatR (CQRS) · EF Core · DDD aggregates

STYLE  VerticalSlices  (confidence high)
       evidence: FastEndpoints detected; MediatR with 6 handlers

       per service:
         Clean.Architecture.AspireHost: Unknown
         Clean.Architecture.Web: Web App [EF Core]
         MinimalClean.Architecture.AspireHost: Unknown
         MinimalClean.Architecture.Web: Web App [EF Core]
         NimblePros.SampleToDo.AspireHost: Unknown
         NimblePros.SampleToDo.Web: Web App [EF Core, FluentValidation]

TOPOLOGY (depends-on)
   Clean.Architecture.Core
   Clean.Architecture.ServiceDefaults
   Clean.Architecture.UseCases ── Clean.Architecture.Core
   Clean.Architecture.Infrastructure ── Clean.Architecture.Core, Clean.Architecture.UseCases
   Clean.Architecture.Web ── Clean.Architecture.Infrastructure, Clean.Architecture.ServiceDefaults, Clean.Architecture.UseCases
   Clean.Architecture.AspireHost ── Clean.Architecture.ServiceDefaults, Clean.Architecture.Web

ENTRY POINTS
   HTTP (5)
      DELETE /Contributors/{ContributorId:int}  → Delete  (src/Clean.Architecture.Web/Contributors/Delete.cs:19)
      GET /Contributors  → ListContributorsQuery  (src/Clean.Architecture.Web/Contributors/List.cs:14)
      GET /Contributors/{ContributorId:int}  → GetById  (src/Clean.Architecture.Web/Contributors/GetById.cs:18)
      POST /Contributors  → Create  (src/Clean.Architecture.Web/Contributors/Create.cs:25)
      PUT /Contributors/{ContributorId:int}  → Update  (src/Clean.Architecture.Web/Contributors/Update.cs:20)
   Domain (2)
      ContributorDeletedHandler  → ContributorDeletedHandler  (src/Clean.Architecture.Core/ContributorAggregate/Handlers/ContributorDeletedHandler.cs:6)
      ContributorNameUpdatedEmailNotificationHandler  → ContributorNameUpdatedEmailNotificationHandler  (src/Clean.Architecture.Core/ContributorAggregate/Handlers/ContributorNameUpdatedEmailNotificationHandler.cs:6)

CROSS-CUTTING
   Aggregates:   Cart · Contributor · GuestUser · Order · Product · Project

PACKAGES
   Web/API:  Ardalis.Result.AspNetCore 10.1.0, AspNetCore.Localizer.Json 1.0.4, FastEndpoints 8.1.0, FastEndpoints.ApiExplorer 2.3.0, FastEndpoints.Swagger 8.1.0, FastEndpoints.Swagger.Swashbuckle 2.3.0, Microsoft.AspNetCore.Mvc.NewtonsoftJson 10.0.7, Microsoft.AspNetCore.Mvc.Testing 10.0.7 … (11 total)
   ORM/Data:  Ardalis.Specification.EntityFrameworkCore 9.3.1, Aspire.Hosting.SqlServer 13.3.5, Microsoft.EntityFrameworkCore.Design 10.0.7, Microsoft.EntityFrameworkCore.InMemory 10.0.7, Microsoft.EntityFrameworkCore.Relational 10.0.7, Microsoft.EntityFrameworkCore.Sqlite 10.0.7, Microsoft.EntityFrameworkCore.SqlServer 10.0.7, Microsoft.EntityFrameworkCore.Tools 10.0.7 … (9 total)
   Validation:  FluentValidation 12.1.1, FluentValidation.DependencyInjectionExtensions 12.1.1
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.3, OpenTelemetry.Extensions.Hosting 1.16.0, OpenTelemetry.Instrumentation.Http 1.15.1, OpenTelemetry.Instrumentation.Runtime 1.15.1, Serilog.Sinks.ApplicationInsights 5.0.1, Serilog.Sinks.OpenTelemetry 4.2.0
   Testing:  coverlet.collector 10.0.0, NSubstitute 5.3.0, Shouldly 4.3.0, Testcontainers 4.11.0, Testcontainers.MsSql 4.11.0, xunit.runner.visualstudio 3.1.5, xunit.v3 3.2.2
   Cloud:  Azure.Identity 1.21.0
   Utilities:  Newtonsoft.Json 13.0.4
   Other:  Ardalis.GuardClauses 5.0.0, Ardalis.HttpClientTestExtensions 4.2.0, Ardalis.ListStartupServices 1.1.4, Ardalis.Result 10.1.0, Ardalis.SharedKernel 5.0.0, Ardalis.SmartEnum 8.2.0, Ardalis.Specification 9.3.1, Aspire.Hosting.AppHost 13.2.3 … (27 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "GET /Contributors")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 400ms |
| GenericExtraction | 429ms |
| SignalSealing | 0ms |
| SpecificExtraction | 2706ms |
| Compression | 54ms |
| **Total** | **9015ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 2227ms | 0 | 0 |
| EndpointExtractor | 471ms | 0 | 75 |
| SyntaxStructureExtractor | 417ms | 381 | 90 |
| ProgramCsFlowExtractor | 415ms | 208 | 90 |
| DiRegistrationExtractor | 399ms | 0 | 87 |
| SourceBodyExtractor | 197ms | 0 | 0 |
| FileTreeExtractor | 146ms | 0 | 0 |
| ProjectStructure | 138ms | 0 | 0 |
| BodyFactsExtractor | 123ms | 0 | 0 |
| SolutionDiscovery | 109ms | 0 | 0 |
| EfCoreExtractor | 99ms | 0 | 52 |
| MediatRExtractor | 85ms | 0 | 47 |
| InMemoryEventBusExtractor | 84ms | 0 | 47 |
| IndirectWiringDetector | 36ms | 0 | 34 |
| DependencyExtractor | 29ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 98 | 53 |
| Sends | 24 | 0 |
| Handles | 2 | 0 |
| Consumes | 2 | 0 |
| ReadsWrites | 16 | 13 |
| Resolves | 13 | 0 |
| EntityRelation | 1 | 1 |

_362 files · 22 projects_
