Overview map (no focus).
Analyzing project...

MAP  TodoApp     (6 projects)

STACK  Minimal APIs · EF Core

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 6 project(s); no MediatR

TOPOLOGY (depends-on)
   Todo.Api ── TodoApp.ServiceDefaults
   Todo.Web.Client ── Todo.Web.Shared
   Todo.Web.Server ── Todo.Web.Client, Todo.Web.Shared, TodoApp.ServiceDefaults
   Todo.Web.Shared
   TodoApp.AppHost ── Todo.Api, Todo.Web.Server
   TodoApp.ServiceDefaults

ENTRY POINTS
   HTTP (11)
      GET /auth/signin/{provider}  (Todo.Web/Server/AuthApi.cs:64)
      GET /auth/login/{provider}  (Todo.Web/Server/AuthApi.cs:55)
      POST /auth/logout  (Todo.Web/Server/AuthApi.cs:40)
      POST /auth/login  (Todo.Web/Server/AuthApi.cs:27)
      POST /auth/register  (Todo.Web/Server/AuthApi.cs:14)
      POST /users/token/{provider}  (Todo.Api/Users/UsersApi.cs:24)
      DELETE /todos/{id}  (Todo.Api/Todos/TodoApi.cs:67)
      PUT /todos/{id}  (Todo.Api/Todos/TodoApi.cs:52)
      POST /todos/  (Todo.Api/Todos/TodoApi.cs:38)
      GET /todos/{id}  (Todo.Api/Todos/TodoApi.cs:29)
      GET /todos/  (Todo.Api/Todos/TodoApi.cs:24)

PACKAGES
   Web/API:  AspNet.Security.OAuth.GitHub, Auth0.AspNetCore.Authentication, 
Microsoft.AspNetCore.Authentication.Google, 
Microsoft.AspNetCore.Authentication.MicrosoftAccount, 
Microsoft.AspNetCore.Components.WebAssembly, 
Microsoft.AspNetCore.Components.WebAssembly.Server, 
Microsoft.AspNetCore.Identity.EntityFrameworkCore, 
Microsoft.AspNetCore.Mvc.Testing . (11 total)
   ORM/Data:  Microsoft.EntityFrameworkCore.Design, 
Microsoft.EntityFrameworkCore.InMemory, Microsoft.EntityFrameworkCore.Sqlite
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, 
OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.Http, 
OpenTelemetry.Instrumentation.Runtime
   Testing:  coverlet.collector, xunit, xunit.runner.visualstudio
   Other:  Microsoft.Extensions.Http, Microsoft.Extensions.Http.Resilience, 
Microsoft.Extensions.ServiceDiscovery, 
Microsoft.Extensions.ServiceDiscovery.Yarp, Microsoft.NET.Test.Sdk, 
Microsoft.OpenApi, MiniValidation, Yarp.ReverseProxy

 drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 40 files · 166 nodes · 90 edges · 11 entries · ~551 tokens · 2.1s 
stage2 x2.8 stage3 x1.1
┌──────────┬──────────────────────┐
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │     TodoApp.sln      │
│   Time   │        2194ms        │
│  Tokens  │  ~551 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.127 │
└──────────┴──────────────────────┘
