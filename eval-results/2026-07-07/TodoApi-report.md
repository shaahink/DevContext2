# REPORT
**TodoApp**

Style: MinimalApi
_6 projects  ·  12 HttpEndpoint  ·  net9.0 + blazor + minimal-apis + identity + efcore_

## Stats

| Metric | Value |
|--------|-------|
| Files | 40 |
| Projects | 7 |
| Nodes | 164 |
| Edges | 57 |
| Entries | 12 |
| With target | 11/12 |
| Verified edges | 70% |
| Analyzed in | 1.9s |

## Top Flows

1. **GET /todos/{id}** → `TodoApi` *(HttpEndpoint)*
2. **POST /todos/** → `TodoApi` *(HttpEndpoint)*
3. **DELETE /todos/{id}** → `TodoApi` *(HttpEndpoint)*
4. **GET /auth/login/{provider}** → `AuthApi` *(HttpEndpoint)*
5. **GET /auth/signin/{provider}** → `AuthApi` *(HttpEndpoint)*
6. **GET /todos/** → `TodoApi` *(HttpEndpoint)*
7. **POST /auth/login** → `AuthApi` *(HttpEndpoint)*
8. **POST /auth/logout** → `AuthApi` *(HttpEndpoint)*
9. **POST /auth/register** → `AuthApi` *(HttpEndpoint)*
10. **POST /users/token/{provider}** → `UsersApi` *(HttpEndpoint)*

### Trace 1: GET /todos/{id}

TRACE  GET /todos/{id}
       Todo.Api/Todos/TodoApi.cs:29

▸ ENTRY  GET /todos/{id}  (Todo.Api/Todos/TodoApi.cs:29)
   └─ call <lambda> GET /todos/{id}  (Todo.Api/Todos/TodoApi.cs:29)
      └─ data Todo [approx]
             public class Todo
             public int Id { get; set; }
             [Required]
         └─ data TodoDbContext  (Todo.Api/TodoDbContext.cs:6)
                public class TodoDbContext(DbContextOptions<TodoDbContext> options) : IdentityDbContext<TodoUser>(options)
                public DbSet<Todo> Todos => Set<Todo>();
                protected override void OnModelCreating(ModelBuilder builder)

TOUCHES  Todo
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: POST /todos/

TRACE  POST /todos/
       Todo.Api/Todos/TodoApi.cs:38

▸ ENTRY  POST /todos/  (Todo.Api/Todos/TodoApi.cs:38)
   └─ call <lambda> POST /todos/  (Todo.Api/Todos/TodoApi.cs:38)
      └─ data Todo [approx]
             public class Todo
             public int Id { get; set; }
             [Required]
         └─ data TodoDbContext  (Todo.Api/TodoDbContext.cs:6)
                public class TodoDbContext(DbContextOptions<TodoDbContext> options) : IdentityDbContext<TodoUser>(options)
                public DbSet<Todo> Todos => Set<Todo>();
                protected override void OnModelCreating(ModelBuilder builder)

TOUCHES  Todo
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 3: DELETE /todos/{id}

TRACE  DELETE /todos/{id}
       Todo.Api/Todos/TodoApi.cs:67

▸ ENTRY  DELETE /todos/{id}  (Todo.Api/Todos/TodoApi.cs:67)
   └─ call <lambda> DELETE /todos/{id}  (Todo.Api/Todos/TodoApi.cs:67)
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

## Insights

_6 info · 3 notable · 1 warning_

### **WARNING**: 6/12 endpoints anonymous, incl. 3 POST/PUT/DELETE
*(Risk)*

- GET /
- GET /auth/signin/{provider}
- GET /auth/login/{provider}
- POST /auth/login
- POST /auth/register

### **NOTABLE**: Config without defaults: 2 consumed keys have no appsettings default
*(Risk)*

- APPLICATIONINSIGHTS_CONNECTION_STRING
- OTEL_EXPORTER_OTLP_ENDPOINT

### **NOTABLE**: Extension seats: AddOptions (3 impls) · AddAuthentication (2 impls) · AddHttpClient (2 impls)
*(Wiring)*

- AddOptions (3 impls)
- AddAuthentication (2 impls)
- AddHttpClient (2 impls)

### **NOTABLE**: Auth surface: 6 protected, 6 unannotated of 12 API endpoints
*(Risk)*

- 6 protected
- POST /auth/logout
- DELETE /todos/{id}
- 6 no auth annotation

### _INFO_: Entry targets resolved 11/12 (91%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: DI: 33 Extension · 5 Singleton · 5 Scoped (43 total)
*(Wiring)*

### _INFO_: Routing surface: 8 routes exposed
*(Shape)*

- GET /
- GET /auth/signin/{provider}
- GET /auth/login/{provider}
- POST /auth/logout
- POST /auth/login

### _INFO_: Public surface: 0 interfaces, 33 classes (34 total public types)
*(Shape)*

- 0 interfaces
- 33 classes

### _INFO_: Most depended-upon: Todo.Api (2 dependents) · Todo.Web.Shared (2 dependents) · TodoApp.ServiceDefaults (1 dependents)
*(Topology)*

- Todo.Api (2 dependents)
- Todo.Web.Shared (2 dependents)
- TodoApp.ServiceDefaults (1 dependents)

### _INFO_: Data map: 2 entities across 2 scopes
*(Data)*

- TodoDbContext (1 entities)
- Todo (1 entities)

MAP  TodoApp     (6 projects)

STACK  net9.0 · Minimal APIs · EF Core

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 6 project(s); no MediatR

       per service:
         Todo.Api: Web API [EF Core]
         Todo.Web.Server: Gateway [YARP]

TOPOLOGY (depends-on)
   Todo.Web.Shared
   TodoApp.ServiceDefaults
   Todo.Api ── TodoApp.ServiceDefaults
   Todo.Web.Client ── Todo.Web.Shared
   Todo.Web.Server ── Todo.Web.Client, Todo.Web.Shared, TodoApp.ServiceDefaults
   TodoApp.AppHost ── Todo.Api, Todo.Web.Server

ENTRY POINTS
   HTTP (12)
      DELETE /todos/{id}  → TodoApi  (Todo.Api/Todos/TodoApi.cs:67)
      GET /auth/login/{provider}  → AuthApi  (Todo.Web/Server/AuthApi.cs:55)
      GET /auth/signin/{provider}  → AuthApi  (Todo.Web/Server/AuthApi.cs:64)
      GET /todos/  → TodoApi  (Todo.Api/Todos/TodoApi.cs:24)
      GET /todos/{id}  → TodoApi  (Todo.Api/Todos/TodoApi.cs:29)
      POST /auth/login  → AuthApi  (Todo.Web/Server/AuthApi.cs:27)
      POST /auth/logout  → AuthApi  (Todo.Web/Server/AuthApi.cs:40)
      POST /auth/register  → AuthApi  (Todo.Web/Server/AuthApi.cs:14)
      POST /todos/  → TodoApi  (Todo.Api/Todos/TodoApi.cs:38)
      POST /users/token/{provider}  → UsersApi  (Todo.Api/Users/UsersApi.cs:24)
      PUT /todos/{id}  → TodoApi  (Todo.Api/Todos/TodoApi.cs:52)
      GET /  (Todo.Web/Server/App.razor:1)

PACKAGES
   Web/API:  AspNet.Security.OAuth.GitHub 9.4.0, Auth0.AspNetCore.Authentication 1.5.0, Microsoft.AspNetCore.Authentication.Google 9.0.8, Microsoft.AspNetCore.Authentication.MicrosoftAccount 9.0.8, Microsoft.AspNetCore.Components.WebAssembly 9.0.8, Microsoft.AspNetCore.Components.WebAssembly.Server 9.0.8, Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.8, Microsoft.AspNetCore.Mvc.Testing 9.0.8 … (11 total)
   ORM/Data:  Microsoft.EntityFrameworkCore.Design 9.0.8, Microsoft.EntityFrameworkCore.InMemory 9.0.8, Microsoft.EntityFrameworkCore.Sqlite 9.0.8
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.12.0, OpenTelemetry.Extensions.Hosting 1.12.0, OpenTelemetry.Instrumentation.Http 1.12.0, OpenTelemetry.Instrumentation.Runtime 1.12.0
   Testing:  coverlet.collector 6.0.4, xunit 2.9.3, xunit.runner.visualstudio 3.1.3
   Other:  Microsoft.Extensions.Http 9.0.8, Microsoft.Extensions.Http.Resilience 9.7.0, Microsoft.Extensions.ServiceDiscovery 9.5.0, Microsoft.Extensions.ServiceDiscovery.Yarp 9.5.0, Microsoft.NET.Test.Sdk 17.14.1, Microsoft.OpenApi 1.6.22, MiniValidation 0.9.2, Yarp.ReverseProxy 2.3.0

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 187ms |
| GenericExtraction | 289ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1038ms |
| Compression | 22ms |
| **Total** | **1936ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 927ms | 0 | 0 |
| SyntaxStructureExtractor | 286ms | 43 | 49 |
| DiRegistrationExtractor | 284ms | 0 | 49 |
| ProgramCsFlowExtractor | 246ms | 0 | 6 |
| ProjectStructure | 130ms | 0 | 0 |
| EndpointExtractor | 107ms | 0 | 17 |
| BlazorEntryExtractor | 106ms | 0 | 17 |
| SourceBodyExtractor | 36ms | 0 | 0 |
| SolutionDiscovery | 34ms | 0 | 0 |
| EfCoreExtractor | 26ms | 0 | 5 |
| FileTreeExtractor | 19ms | 0 | 0 |
| InMemoryEventBusExtractor | 18ms | 0 | 2 |
| DependencyExtractor | 12ms | 0 | 0 |
| LayerClassifier | 12ms | 0 | 0 |
| IndirectWiringDetector | 9ms | 0 | 2 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 21 | 4 |
| ReadsWrites | 15 | 13 |
| Resolves | 21 | 0 |

_40 files · 7 projects_
