MAP  Clean.Architecture     (6 projects)
SCOPE  analyzed Clean.Architecture.slnx - 1 of 4 solutions in this repo; analyze
another by naming its solution - style/topology are local to this slice, not the
whole system
       not analyzed - 2 runnable apps outside this solution:
         MinimalClean.Architecture.AspireHost: Unknown
         MinimalClean.Architecture.Web: Web App [EF Core]

STACK  net10.0, net9.0 ú Minimal APIs ú FastEndpoints ú MediatR (CQRS) ú EF Core
ú DDD aggregates

STYLE  CleanArchitecture  (confidence moderate)
       evidence: DDD folder layers: Application, Infrastructure, Core; 2 
domain-event handlers; MediatR with 2 handlers

       per service:
         Clean.Architecture.AspireHost: Unknown
         Clean.Architecture.Web: Web App [EF Core]

TOPOLOGY (depends-on)
   Clean.Architecture.Core
   Clean.Architecture.ServiceDefaults
   Clean.Architecture.UseCases ÄÄ Clean.Architecture.Core
   Clean.Architecture.Infrastructure ÄÄ Clean.Architecture.Core, 
Clean.Architecture.UseCases
   Clean.Architecture.Web ÄÄ Clean.Architecture.Infrastructure, 
Clean.Architecture.ServiceDefaults, Clean.Architecture.UseCases
   Clean.Architecture.AspireHost ÄÄ Clean.Architecture.ServiceDefaults, 
Clean.Architecture.Web

ENTRY POINTS
   HTTP (5)
      DELETE /Contributors/{ContributorId:int}   Delete  
(src/Clean.Architecture.Web/Contributors/Delete.cs:19)
      GET /Contributors   ListContributorsQuery  
(src/Clean.Architecture.Web/Contributors/List.cs:14)
      GET /Contributors/{ContributorId:int}   GetById  
(src/Clean.Architecture.Web/Contributors/GetById.cs:18)
      POST /Contributors   Create  
(src/Clean.Architecture.Web/Contributors/Create.cs:25)
      PUT /Contributors/{ContributorId:int}   Update  
(src/Clean.Architecture.Web/Contributors/Update.cs:20)
   Domain (2)
      ContributorDeletedHandler  
(src/Clean.Architecture.Core/ContributorAggregate/Handlers/ContributorDeletedHan
dler.cs:6)
      ContributorNameUpdatedEmailNotificationHandler  
(src/Clean.Architecture.Core/ContributorAggregate/Handlers/ContributorNameUpdate
dEmailNotificationHandler.cs:6)

CROSS-CUTTING
   Aggregates:   Cart ú Contributor ú GuestUser ú Order ú Product ú Project

PACKAGES
   Web/API:  Ardalis.Result.AspNetCore 10.1.0, AspNetCore.Localizer.Json 1.0.4, 
FastEndpoints 8.1.0, FastEndpoints.ApiExplorer 2.3.0, FastEndpoints.Swagger 
8.1.0, FastEndpoints.Swagger.Swashbuckle 2.3.0, 
Microsoft.AspNetCore.Mvc.NewtonsoftJson 10.0.7, Microsoft.AspNetCore.Mvc.Testing
10.0.7 . (11 total)
   ORM/Data:  Ardalis.Specification.EntityFrameworkCore 9.3.1, 
Aspire.Hosting.SqlServer 13.3.5, Microsoft.EntityFrameworkCore.Design 10.0.7, 
Microsoft.EntityFrameworkCore.InMemory 10.0.7, 
Microsoft.EntityFrameworkCore.Relational 10.0.7, 
Microsoft.EntityFrameworkCore.Sqlite 10.0.7, 
Microsoft.EntityFrameworkCore.SqlServer 10.0.7, 
Microsoft.EntityFrameworkCore.Tools 10.0.7 . (9 total)
   Validation:  FluentValidation 12.1.1, 
FluentValidation.DependencyInjectionExtensions 12.1.1
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.3, 
OpenTelemetry.Extensions.Hosting 1.15.3, OpenTelemetry.Instrumentation.Http 
1.15.1, OpenTelemetry.Instrumentation.Runtime 1.15.1, 
Serilog.Sinks.ApplicationInsights 5.0.1, Serilog.Sinks.OpenTelemetry 4.2.0
   Testing:  coverlet.collector 10.0.0, NSubstitute 5.3.0, Shouldly 4.3.0, 
Testcontainers 4.11.0, Testcontainers.MsSql 4.11.0, xunit.runner.visualstudio 
3.1.5, xunit.v3 3.2.2
   Cloud:  Azure.Identity 1.21.0
   Utilities:  Newtonsoft.Json 13.0.4
   Other:  Ardalis.GuardClauses 5.0.0, Ardalis.HttpClientTestExtensions 4.2.0, 
Ardalis.ListStartupServices 1.1.4, Ardalis.Result 10.1.0, Ardalis.SharedKernel 
5.0.0, Ardalis.SmartEnum 8.2.0, Ardalis.Specification 9.3.1, 
Aspire.Hosting.AppHost 13.2.3 . (26 total)

 drill in:  trace a focused entry   (e.g. trace "GET /Contributors")

