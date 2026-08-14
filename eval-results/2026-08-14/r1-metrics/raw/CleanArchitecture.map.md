MAP  CleanArchitecture     (8 projects)

STACK  net10.0 ú Minimal APIs ú MediatR (CQRS) ú EF Core ú FluentValidation

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure, Api; 1 
domain-event handlers; MediatR with 10 handlers

       per service:
         AppHost: Aspire AppHost [Aspire]
         Web: Web API [EF Core]
         TestAppHost: Unknown

TOPOLOGY (depends-on)
   Shared
   Application ÄÄ Domain
   Domain
   Infrastructure ÄÄ Application, Shared
   ServiceDefaults
   Web ÄÄ Application, Infrastructure, ServiceDefaults
   AppHost ÄÄ Shared, Web
   TestAppHost ÄÄ Shared

ENTRY POINTS
   HTTP (15)
      DELETE /   EndpointRouteBuilderExtensions  
(src/Web/Infrastructure/EndpointRouteBuilderExtensions.cs:60)
      DELETE /{id}   DeleteTodoItemCommand  (src/Web/Endpoints/TodoItems.cs:18)
      DELETE /{id} [DeleteTodoList]   DeleteTodoListCommand  
(src/Web/Endpoints/TodoLists.cs:18)
      GET /   GetTodosQuery  (src/Web/Endpoints/TodoLists.cs:15)
      GET / [GetWeatherForecasts]   GetWeatherForecastsQuery  
(src/Web/Endpoints/WeatherForecasts.cs:12)
      GET / [handler]   EndpointRouteBuilderExtensions  
(src/Web/Infrastructure/EndpointRouteBuilderExtensions.cs:24)
      PATCH /   EndpointRouteBuilderExtensions  
(src/Web/Infrastructure/EndpointRouteBuilderExtensions.cs:51)
      PATCH /UpdateDetail/{id}   UpdateTodoItemDetailCommand  
(src/Web/Endpoints/TodoItems.cs:17)
      POST /   CreateTodoItemCommand  (src/Web/Endpoints/TodoItems.cs:15)
      POST / [CreateTodoList]   CreateTodoListCommand  
(src/Web/Endpoints/TodoLists.cs:16)
      POST / [handler]   EndpointRouteBuilderExtensions  
(src/Web/Infrastructure/EndpointRouteBuilderExtensions.cs:33)
      POST /logout   Users  (src/Web/Endpoints/Users.cs:14)
      PUT /   EndpointRouteBuilderExtensions  
(src/Web/Infrastructure/EndpointRouteBuilderExtensions.cs:42)
      PUT /{id}   UpdateTodoItemCommand  (src/Web/Endpoints/TodoItems.cs:16)
      PUT /{id} [UpdateTodoList]   UpdateTodoListCommand  
(src/Web/Endpoints/TodoLists.cs:17)
   Domain (1)
      LogTodoItemCompleted  
(src/Application/TodoItems/EventHandlers/LogTodoItemCompleted.cs:6)

CROSS-CUTTING
   MediatR pipeline (every command):  AuthorizationBehaviour  
PerformanceBehaviour  UnhandledExceptionBehaviour  ValidationBehaviour

PACKAGES
   Web/API:  Azure.Extensions.AspNetCore.Configuration.Secrets 1.5.0, 
Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore 10.0.5, 
Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.5, 
Microsoft.AspNetCore.Mvc.Testing 10.0.5, Microsoft.AspNetCore.OpenApi 10.0.5, 
OpenTelemetry.Instrumentation.AspNetCore 1.15.1, Scalar.AspNetCore 2.13.22
   ORM/Data:  Aspire.Microsoft.EntityFrameworkCore.SqlServer 13.2.2, 
Aspire.Npgsql.EntityFrameworkCore.PostgreSQL 13.2.2, 
CommunityToolkit.Aspire.Hosting.SQLite 13.1.1, Microsoft.EntityFrameworkCore 
10.0.5, Microsoft.EntityFrameworkCore.Design 10.0.5, 
Microsoft.EntityFrameworkCore.Sqlite 10.0.5, 
Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 10.0.5, 
Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1
   Mediator/CQRS:  MediatR 14.1.0, MediatR.Contracts 2.0.1
   Validation:  FluentValidation.DependencyInjectionExtensions 12.1.1
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.2, 
OpenTelemetry.Extensions.Hosting 1.15.2, OpenTelemetry.Instrumentation.Http 
1.15.0, OpenTelemetry.Instrumentation.Runtime 1.15.0
   Testing:  coverlet.collector 8.0.1, Moq 4.20.72, nunit 4.5.1, NUnit.Analyzers
4.12.0, NUnit3TestAdapter 6.2.0, Reqnroll.NUnit 3.3.4, Shouldly 4.3.0
   Cloud:  Aspire.Hosting.Azure.AppContainers 13.2.2, 
Aspire.Hosting.Azure.PostgreSQL 13.2.2, Aspire.Hosting.Azure.Sql 13.2.2, 
Azure.Identity 1.21.0
   Utilities:  AutoMapper 16.1.1
   Other:  Ardalis.GuardClauses 5.0.0, Aspire.Hosting.AppHost 13.2.2, 
Aspire.Hosting.JavaScript 13.2.2, Aspire.Hosting.Testing 13.2.2, 
Microsoft.Build.Tasks.Core 18.4.0, Microsoft.Build.Utilities.Core 18.4.0, 
Microsoft.Extensions.ApiDescription.Server 10.0.5, Microsoft.Extensions.Hosting 
10.0.5 . (14 total)

 drill in:  trace a focused entry   (e.g. trace "POST /")

