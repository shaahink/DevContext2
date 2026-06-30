MAP  Quartz     (16 projects)

STACK  net10.0, net472;netstandard2.0, net8.0, net8.0;net9.0, net9.0 · Minimal APIs

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 18 project(s); no MediatR

TOPOLOGY (depends-on)
   Quartz
   Quartz.Jobs ── Quartz
   Quartz.Plugins ── Quartz.Jobs
   Quartz.AspNetCore ── Quartz.HttpClient
   Quartz.HttpClient ── Quartz
   Quartz.Dashboard ── Quartz.AspNetCore
   Quartz.Plugins.TimeZoneConverter ── Quartz
   _build
   Quartz.Benchmark ── Quartz.Jobs
   Quartz.Examples ── Quartz, Quartz.Jobs, Quartz.Plugins
   Quartz.Examples.AspNetCore ── Quartz.AspNetCore, Quartz.Dashboard, Quartz.Plugins, Quartz.Plugins.TimeZoneConverter
   Quartz.Examples.HttpClient ── Quartz.HttpClient
   Quartz.Examples.Worker ── Quartz
   Quartz.Extensions.Redis ── Quartz
   Quartz.Serialization.Newtonsoft ── Quartz
   Quartz.Server ── Quartz.Plugins

ENTRY POINTS
   HTTP (51)
      DELETE /  → EndpointHelper.ExecuteWithOkResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/SchedulerEndpoints.cs:51)
      DELETE /  → EndpointHelper.ExecuteWithOkResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/JobEndpoints.cs:54)
      DELETE /  → EndpointHelper.ExecuteWithOkResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/CalendarEndpoints.cs:27)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/TriggerEndpoints.cs:51)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/TriggerEndpoints.cs:48)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/TriggerEndpoints.cs:45)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/TriggerEndpoints.cs:27)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/TriggerEndpoints.cs:24)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/TriggerEndpoints.cs:21)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/TriggerEndpoints.cs:18)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/SchedulerEndpoints.cs:45)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/SchedulerEndpoints.cs:24)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/SchedulerEndpoints.cs:21)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/SchedulerEndpoints.cs:18)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/JobEndpoints.cs:66)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/JobEndpoints.cs:63)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/JobEndpoints.cs:30)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/JobEndpoints.cs:27)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/JobEndpoints.cs:24)
      GET /  → EndpointHelper.ExecuteWithJsonResponse  (src/Quartz.AspNetCore/AspNetCore/HttpApi/Endpoints/JobEndpoints.cs:21)
      … and 31 more (http entries — use --focus for a drill-in)
   Background (1)
      Worker  (src/Quartz.Examples.Worker/Program.cs:24)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Mvc.Testing, Microsoft.AspNetCore.SignalR.Client, NSwag.AspNetCore, OpenTelemetry.Instrumentation.AspNetCore
   ORM/Data:  Microsoft.Data.SQLite, MySqlConnector, Npgsql, Npgsql.DependencyInjection, System.Data.SQLite.Core, Testcontainers.MySql
   Logging:  log4net, NLog.Extensions.Logging, OpenTelemetry, OpenTelemetry.Exporter.Console, OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime … (12 total)
   Testing:  FluentAssertions, FluentAssertions.Analyzers, NUnit, NUnit.Analyzers, NUnit3TestAdapter, Testcontainers.FirebirdSql, Testcontainers.MsSql, Testcontainers.Oracle … (11 total)
   Utilities:  Newtonsoft.Json
   Other:  BenchmarkDotNet, FakeItEasy, FirebirdSql.Data.FirebirdClient, MELT, Microsoft.CodeAnalysis.BannedApiAnalyzers, Microsoft.Data.SqlClient, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Hosting … (24 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
