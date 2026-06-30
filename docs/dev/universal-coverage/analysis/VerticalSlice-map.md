MAP  Clean.Architecture     (6 projects)

STACK  net9.0 · Minimal APIs · FastEndpoints · MediatR (CQRS) · EF Core · FluentValidation · DDD aggregates

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
      ContributorDeletedHandler  (src/Clean.Architecture.Core/ContributorAggregate/Handlers/ContributorDeletedHandler.cs:6)
      ContributorNameUpdatedEmailNotificationHandler  (src/Clean.Architecture.Core/ContributorAggregate/Handlers/ContributorNameUpdatedEmailNotificationHandler.cs:6)

CROSS-CUTTING
   Aggregates:   Cart · Contributor · GuestUser · Order · Product · Project

PACKAGES
   Web/API:  Ardalis.Result.AspNetCore, AspNetCore.Localizer.Json, FastEndpoints, FastEndpoints.ApiExplorer, FastEndpoints.Swagger, FastEndpoints.Swagger.Swashbuckle, Microsoft.AspNetCore.Mvc.NewtonsoftJson, Microsoft.AspNetCore.Mvc.Testing … (11 total)
   ORM/Data:  Ardalis.Specification.EntityFrameworkCore, Aspire.Hosting.SqlServer, Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.InMemory, Microsoft.EntityFrameworkCore.Relational, Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools … (9 total)
   Validation:  FluentValidation, FluentValidation.DependencyInjectionExtensions
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime, Serilog.Sinks.ApplicationInsights, Serilog.Sinks.OpenTelemetry
   Testing:  coverlet.collector, NSubstitute, Shouldly, Testcontainers, Testcontainers.MsSql, xunit.runner.visualstudio, xunit.v3
   Cloud:  Azure.Identity
   Utilities:  Newtonsoft.Json
   Other:  Ardalis.GuardClauses, Ardalis.HttpClientTestExtensions, Ardalis.ListStartupServices, Ardalis.Result, Ardalis.SharedKernel, Ardalis.SmartEnum, Ardalis.Specification, Aspire.Hosting.AppHost … (26 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
