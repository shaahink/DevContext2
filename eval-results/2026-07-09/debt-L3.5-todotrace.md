# REPORT
**TodoApp**

Style: MinimalApi
_6 projects  ·  12 HttpEndpoint  ·  net9.0 + blazor + minimal-apis + identity + efcore_

## Stats

| Metric | Value |
|--------|-------|
| Files | 40 |
| Projects | 7 |
| Nodes | 123 |
| Edges | 81 |
| Entries | 12 |
| With target | 11/12 |
| Verified edges | 69% |
| Analyzed in | 2.7s |

## Top Flows

1. **POST /todos/** → `TodoDbContext` *(HttpEndpoint)*
2. **DELETE /todos/{id}** → `TodoDbContext` *(HttpEndpoint)*
3. **GET /auth/signin/{provider}** → `AuthClient` *(HttpEndpoint)*
4. **GET /todos/** → `TodoDbContext` *(HttpEndpoint)*
5. **GET /todos/{id}** → `TodoDbContext` *(HttpEndpoint)*
6. **POST /auth/login** → `AuthClient` *(HttpEndpoint)*
7. **POST /auth/register** → `AuthClient` *(HttpEndpoint)*
8. **PUT /todos/{id}** → `TodoDbContext` *(HttpEndpoint)*
9. **GET /auth/login/{provider}** → `AuthApi` *(HttpEndpoint)*
10. **POST /auth/logout** → `AuthApi` *(HttpEndpoint)*

### Trace 1: POST /todos/

TRACE  POST /todos/
       Todo.Api/Todos/TodoApi.cs:38
       Todo.Api
▸ ENTRY  POST /todos/  (Todo.Api/Todos/TodoApi.cs:38)
   └─ call <lambda> POST /todos/  (Todo.Api/Todos/TodoApi.cs:38)
      ├─ call TodoDbContext  (Todo.Api/Todos/TodoApi.cs:9) [approx]
      │      public class TodoDbContext(DbContextOptions<TodoDbContext> options) : IdentityDbContext<TodoUser>(options)
      │      public DbSet<Todo> Todos => Set<Todo>();
      │      protected override void OnModelCreating(ModelBuilder builder)
      └─ call Todo  (Todo.Api/Todos/TodoApi.cs:12) [approx]
             public class Todo
             public int Id { get; set; }
             [Required]

TOUCHES  Todo
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: DELETE /todos/{id}

TRACE  DELETE /todos/{id}
       Todo.Api/Todos/TodoApi.cs:67
       Todo.Api
▸ ENTRY  DELETE /todos/{id}  (Todo.Api/Todos/TodoApi.cs:67)
   └─ call <lambda> DELETE /todos/{id}  (Todo.Api/Todos/TodoApi.cs:67)
      └─ call TodoDbContext  (Todo.Api/Todos/TodoApi.cs:3) [approx]
             public class TodoDbContext(DbContextOptions<TodoDbContext> options) : IdentityDbContext<TodoUser>(options)
             public DbSet<Todo> Todos => Set<Todo>();
             protected override void OnModelCreating(ModelBuilder builder)
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

### Trace 3: GET /auth/signin/{provider}

TRACE  GET /auth/signin/{provider}
       Todo.Web/Server/AuthApi.cs:64
       Todo.Web.Server
▸ ENTRY  GET /auth/signin/{provider}  (Todo.Web/Server/AuthApi.cs:64)
   └─ call <lambda> GET /auth/signin/{provider}  (Todo.Web/Server/AuthApi.cs:64)
      └─ call AuthClient  (Todo.Web/Server/AuthApi.cs:19) [approx]
             public class AuthClient(HttpClient client)
             public async Task<string?> GetTokenAsync(UserInfo userInfo)
             var response = await client.PostAsJsonAsync("users/login", userInfo);
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_6 info · 3 notable · 1 warning_

### **WARNING**: 6/12 endpoints anonymous, incl. 3 POST/PUT/DELETE
*(Risk)*

- GET /auth/signin/{provider}
- GET /auth/login/{provider}
- POST /auth/login
- POST /auth/register
- POST /users/token/{provider}

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

- GET /auth/signin/{provider}
- GET /auth/login/{provider}
- POST /auth/logout
- POST /auth/login
- POST /auth/register

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

- Todo.Api (1 entities)
- TodoDbContext (1 entities)

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
      DELETE /todos/{id}  → TodoDbContext  (Todo.Api/Todos/TodoApi.cs:67)
      GET /auth/login/{provider}  → AuthApi  (Todo.Web/Server/AuthApi.cs:55)
      GET /auth/signin/{provider}  → AuthClient  (Todo.Web/Server/AuthApi.cs:64)
      GET /todos/  → TodoDbContext  (Todo.Api/Todos/TodoApi.cs:24)
      GET /todos/{id}  → TodoDbContext  (Todo.Api/Todos/TodoApi.cs:29)
      POST /auth/login  → AuthClient  (Todo.Web/Server/AuthApi.cs:27)
      POST /auth/logout  → AuthApi  (Todo.Web/Server/AuthApi.cs:40)
      POST /auth/register  → AuthClient  (Todo.Web/Server/AuthApi.cs:14)
      POST /todos/  → TodoDbContext  (Todo.Api/Todos/TodoApi.cs:38)
      POST /users/token/{provider}  → UsersApi  (Todo.Api/Users/UsersApi.cs:24)
      PUT /todos/{id}  → TodoDbContext  (Todo.Api/Todos/TodoApi.cs:52)
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
| DiscoveryAndCacheWarmup | 74ms |
| GenericExtraction | 222ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1123ms |
| Compression | 15ms |
| **Total** | **2725ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1013ms | 0 | 0 |
| SyntaxStructureExtractor | 218ms | 43 | 49 |
| DiRegistrationExtractor | 212ms | 0 | 49 |
| ProgramCsFlowExtractor | 195ms | 0 | 6 |
| EndpointExtractor | 106ms | 0 | 17 |
| SourceBodyExtractor | 54ms | 0 | 0 |
| EfCoreExtractor | 38ms | 0 | 6 |
| ProjectStructure | 32ms | 0 | 0 |
| BlazorEntryExtractor | 31ms | 0 | 3 |
| BodyFactsExtractor | 28ms | 0 | 0 |
| DependencyExtractor | 27ms | 0 | 0 |
| LayerClassifier | 27ms | 0 | 0 |
| InMemoryEventBusExtractor | 23ms | 0 | 2 |
| FileTreeExtractor | 23ms | 0 | 0 |
| SolutionDiscovery | 15ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 51 | 16 |
| ReadsWrites | 9 | 9 |
| Resolves | 21 | 0 |

_40 files · 7 projects_
