# REPORT
**Clean.Architecture**

Style: VerticalSlices
_6 projects  ·  5 HttpEndpoint, 2 DomainEventHandler  ·  net10.0, net9.0 + minimal-apis + mediatr + serilog + efcore + aspire + fast-endpoints_

## Stats

| Metric | Value |
|--------|-------|
| Files | 362 |
| Projects | 22 |
| Nodes | 649 |
| Edges | 139 |
| Entries | 7 |
| With target | 7/7 |
| Verified edges | 29% |
| Analyzed in | 2.3s |

## Top Flows

1. **POST /Contributors** → `CreateContributorRequest` *(HttpEndpoint)*
2. **DELETE /Contributors/{ContributorId:int}** → `Delete` *(HttpEndpoint)*
3. **GET /Contributors** → `ListContributorsMapper` *(HttpEndpoint)*
4. **GET /Contributors/{ContributorId:int}** → `GetById` *(HttpEndpoint)*
5. **PUT /Contributors/{ContributorId:int}** → `Update` *(HttpEndpoint)*
6. **ContributorNameUpdatedEmailNotificationHandler** → `ContributorNameUpdatedEmailNotificationHandler` *(DomainEventHandler)*
7. **ContributorDeletedHandler** → `ContributorDeletedHandler` *(DomainEventHandler)*

### Trace 1: POST /Contributors

TRACE  POST /Contributors
       src/Clean.Architecture.Web/Contributors/Create.cs:25

▸ ENTRY  POST /Contributors  (src/Clean.Architecture.Web/Contributors/Create.cs:25)
   └─ call CreateContributorRequest  (src/Clean.Architecture.Web/Contributors/Create.cs:25)
          public class CreateContributorRequest
          public const string Route = "/Contributors";
          [Required]
      └─ data PhoneNumber [approx]
             public class PhoneNumber(string countryCode, string number, string? extension) : ValueObject
             public static PhoneNumber Unknown { get; } = new PhoneNumber(String.Empty, String.Empty, String.Empty);
             public string CountryCode { get; private set; } = countryCode;
         └─ data Migrations  (src/Clean.Architecture.Infrastructure/Data/Migrations/20231218143922_PhoneNumber.cs:8)
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: DELETE /Contributors/{ContributorId:int}

TRACE  DELETE /Contributors/{ContributorId:int}
       src/Clean.Architecture.Web/Contributors/Delete.cs:19

▸ ENTRY  DELETE /Contributors/{ContributorId:int}  (src/Clean.Architecture.Web/Contributors/Delete.cs:19)
   └─ call Delete  (src/Clean.Architecture.Web/Contributors/Delete.cs:19)
          public class Delete
          : Endpoint<DeleteContributorRequest,
          Results<NoContent,
      ├─ send DeleteContributorCommand  (src/Clean.Architecture.Web/Contributors/Delete.cs:42) [approx]
      │      public record DeleteContributorCommand(ContributorId ContributorId) : ICommand<Result>;
      └─ call ContributorId.From  (src/Clean.Architecture.Web/Contributors/Delete.cs:47) [verified]
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

### Trace 3: GET /Contributors

TRACE  GET /Contributors
       src/Clean.Architecture.Web/Contributors/List.cs:14

▸ ENTRY  GET /Contributors  (src/Clean.Architecture.Web/Contributors/List.cs:14)
   └─ call ListContributorsMapper  (src/Clean.Architecture.Web/Contributors/List.cs:14)
          public sealed class ListContributorsMapper
          : Mapper<ListContributorsRequest, ContributorListResponse, UseCases.PagedResult<ContributorDto>>
          public override ContributorListResponse FromEntity(UseCases.PagedResult<ContributorDto> e)
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_5 info · 3 notable · 2 warning_

### **WARNING**: 5/5 endpoints anonymous, incl. 3 POST/PUT/DELETE
*(Risk)*

- PUT /Contributors/{ContributorId:int}
- GET /Contributors
- GET /Contributors/{ContributorId:int}
- DELETE /Contributors/{ContributorId:int}
- POST /Contributors

### **WARNING**: Auth surface: 0 protected, 5 unannotated of 5 endpoints
*(Risk)*

- 5 no auth annotation

### **NOTABLE**: Downstream wiring: 10 target services detected
*(Wiring)*

- Delete.Delete
- Delete.AllowAnonymous
- Delete.Summary
- Delete.Tags
- Delete.Description

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- LoggerConfigs
- ServiceConfigs
- DataSchemaConstants
- CreateContributorHandler
- GetContributorByIdMapper

### **NOTABLE**: Extension seats: AddDbContext (8 impls) · AddMediator (4 impls) · IEmailSender (2 impls)
*(Wiring)*

- AddDbContext (8 impls)
- AddMediator (4 impls)
- IEmailSender (2 impls)

### _INFO_: Entry targets resolved 7/7 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 3 feature areas
*(Shape)*

- Contributors (5 entries)
- ContributorAggregate/Handlers (1 entries)
- NimblePros/SampleToDo/Core/ContributorAggregate/Handlers (1 entries)

### _INFO_: Routing surface: 5 routes exposed
*(Shape)*

- PUT /Contributors/{ContributorId:int}
- GET /Contributors
- GET /Contributors/{ContributorId:int}
- DELETE /Contributors/{ContributorId:int}
- POST /Contributors

### _INFO_: Public surface: 13 interfaces, 294 classes (376 total public types)
*(Shape)*

- 13 interfaces
- 294 classes

### _INFO_: Most depended-upon: Clean.Architecture.ServiceDefaults (2 dependents) · Clean.Architecture.Core (2 dependents) · Clean.Architecture.UseCases (2 dependents)
*(Topology)*

- Clean.Architecture.ServiceDefaults (2 dependents)
- Clean.Architecture.Core (2 dependents)
- Clean.Architecture.UseCases (2 dependents)

MAP  Clean.Architecture     (6 projects)

STACK  net10.0, net9.0 · Minimal APIs · FastEndpoints · MediatR (CQRS) · EF Core · DDD aggregates

STYLE  VerticalSlices  (confidence high)
       evidence: FastEndpoints detected; MediatR with 6 handlers

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
      GET /Contributors  → ListContributorsMapper  (src/Clean.Architecture.Web/Contributors/List.cs:14)
      GET /Contributors/{ContributorId:int}  → GetById  (src/Clean.Architecture.Web/Contributors/GetById.cs:18)
      POST /Contributors  → CreateContributorRequest  (src/Clean.Architecture.Web/Contributors/Create.cs:25)
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
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.3, OpenTelemetry.Extensions.Hosting 1.15.3, OpenTelemetry.Instrumentation.Http 1.15.1, OpenTelemetry.Instrumentation.Runtime 1.15.1, Serilog.Sinks.ApplicationInsights 5.0.1, Serilog.Sinks.OpenTelemetry 4.2.0
   Testing:  coverlet.collector 10.0.0, NSubstitute 5.3.0, Shouldly 4.3.0, Testcontainers 4.11.0, Testcontainers.MsSql 4.11.0, xunit.runner.visualstudio 3.1.5, xunit.v3 3.2.2
   Cloud:  Azure.Identity 1.21.0
   Utilities:  Newtonsoft.Json 13.0.4
   Other:  Ardalis.GuardClauses 5.0.0, Ardalis.HttpClientTestExtensions 4.2.0, Ardalis.ListStartupServices 1.1.4, Ardalis.Result 10.1.0, Ardalis.SharedKernel 5.0.0, Ardalis.SmartEnum 8.2.0, Ardalis.Specification 9.3.1, Aspire.Hosting.AppHost 13.2.3 … (27 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 137ms |
| GenericExtraction | 252ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1510ms |
| Compression | 24ms |
| **Total** | **2260ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1268ms | 0 | 0 |
| SyntaxStructureExtractor | 248ms | 381 | 90 |
| DiRegistrationExtractor | 246ms | 0 | 90 |
| EndpointExtractor | 239ms | 0 | 75 |
| ProgramCsFlowExtractor | 147ms | 0 | 4 |
| SourceBodyExtractor | 87ms | 0 | 0 |
| EfCoreExtractor | 63ms | 0 | 52 |
| InMemoryEventBusExtractor | 55ms | 0 | 47 |
| ProjectStructure | 52ms | 0 | 0 |
| MediatRExtractor | 52ms | 0 | 47 |
| FileTreeExtractor | 50ms | 0 | 0 |
| SolutionDiscovery | 30ms | 0 | 0 |
| DependencyExtractor | 23ms | 0 | 0 |
| IndirectWiringDetector | 18ms | 0 | 30 |
| LayerClassifier | 17ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 48 | 28 |
| Sends | 7 | 7 |
| Handles | 2 | 0 |
| Consumes | 2 | 0 |
| ReadsWrites | 66 | 63 |
| Resolves | 13 | 0 |
| EntityRelation | 1 | 1 |

_362 files · 0 projects_
