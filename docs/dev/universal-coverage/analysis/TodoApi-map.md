MAP  TodoApp     (6 projects)

STACK  Minimal APIs · EF Core

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 6 project(s); no MediatR

TOPOLOGY (depends-on)
   Todo.Web.Shared
   TodoApp.ServiceDefaults
   Todo.Api ── TodoApp.ServiceDefaults
   Todo.Web.Client ── Todo.Web.Shared
   Todo.Web.Server ── Todo.Web.Client, Todo.Web.Shared, TodoApp.ServiceDefaults
   TodoApp.AppHost ── Todo.Api, Todo.Web.Server

ENTRY POINTS
   HTTP (11)
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

PACKAGES
   Web/API:  AspNet.Security.OAuth.GitHub, Auth0.AspNetCore.Authentication, Microsoft.AspNetCore.Authentication.Google, Microsoft.AspNetCore.Authentication.MicrosoftAccount, Microsoft.AspNetCore.Components.WebAssembly, Microsoft.AspNetCore.Components.WebAssembly.Server, Microsoft.AspNetCore.Identity.EntityFrameworkCore, Microsoft.AspNetCore.Mvc.Testing … (11 total)
   ORM/Data:  Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.InMemory, Microsoft.EntityFrameworkCore.Sqlite
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime
   Testing:  coverlet.collector, xunit, xunit.runner.visualstudio
   Other:  Microsoft.Extensions.Http, Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.ServiceDiscovery, Microsoft.Extensions.ServiceDiscovery.Yarp, Microsoft.NET.Test.Sdk, Microsoft.OpenApi, MiniValidation, Yarp.ReverseProxy

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
