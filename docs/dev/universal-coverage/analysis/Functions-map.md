MAP  DotNetWorker.Extensions     (22 projects)

STACK  net10.0, net48, net6.0;net7.0;net8.0;net9.0;net10.0;netstandard2.0, net6.0;net8.0, net6.0;net8.0;net9.0;net10.0;netstandard2.0, net8.0, net8.0;net9.0;net10.0, net9.0, netstandard2.0, netstandard2.0;net472, netstandard2.0;net6.0 · EF Core

STYLE  NLayer  (confidence moderate)
       evidence: EF Core + 51 projects; folder roles: Application, Core

TOPOLOGY (depends-on)
   Worker.Extensions.Abstractions
   Worker.Extensions.Http ── Worker.Extensions.Abstractions
   Worker.Extensions.CosmosDB ── Worker.Extensions.Abstractions
   Worker.Extensions.EventHubs ── Worker.Extensions.Abstractions
   Worker.Extensions.Http.AspNetCore ── Worker.Extensions.Abstractions, Worker.Extensions.Http, Worker.Extensions.Http.AspNetCore.Analyzers
   Worker.Extensions.Http.AspNetCore.Analyzers
   Worker.Extensions.Rpc ── Worker.Extensions.Abstractions
   Worker.Extensions.Storage ── Worker.Extensions.Storage.Blobs, Worker.Extensions.Storage.Queues
   Worker.Extensions.Storage.Blobs ── Worker.Extensions.Abstractions
   Worker.Extensions.Storage.Queues ── Worker.Extensions.Abstractions
   Worker.Extensions.Tables ── Worker.Extensions.Abstractions
   Worker.Extensions.Timer ── Worker.Extensions.Abstractions
   E2EApp ── Worker.Extensions.Abstractions, Worker.Extensions.CosmosDB, Worker.Extensions.EventHubs, Worker.Extensions.Http, Worker.Extensions.Storage, Worker.Extensions.Tables, Worker.Extensions.Timer
   E2EAspNetCoreApp ── Worker.Extensions.Http, Worker.Extensions.Http.AspNetCore
   Worker.Extensions.EventGrid ── Worker.Extensions.Abstractions
   Worker.Extensions.Kafka ── Worker.Extensions.Abstractions
   Worker.Extensions.RabbitMQ ── Worker.Extensions.Abstractions
   Worker.Extensions.SendGrid ── Worker.Extensions.Abstractions
   Worker.Extensions.ServiceBus ── Worker.Extensions.Abstractions, Worker.Extensions.Rpc
   Worker.Extensions.Shared
   Worker.Extensions.SignalRService ── Worker.Extensions.Abstractions
   Worker.Extensions.Warmup ── Worker.Extensions.Abstractions

PACKAGES
   Web/API:  Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore 2.1.0
   ORM/Data:  Microsoft.Azure.Cosmos 3.60.0, Microsoft.Azure.Functions.Worker.Extensions.CosmosDB 4.11.0, Microsoft.EntityFrameworkCore 8.0.10, Microsoft.EntityFrameworkCore.Design 8.0.10, Microsoft.EntityFrameworkCore.SqlServer 8.0.10
   Messaging:  Azure.Messaging.EventGrid 4.29.0, Azure.Messaging.EventHubs 5.12.2, Azure.Messaging.ServiceBus 7.20.1, Microsoft.Azure.Functions.Worker.Extensions.RabbitMQ 2.0.3
   Logging:  Microsoft.ApplicationInsights.PerfCounterCollector 2.23.0, Microsoft.ApplicationInsights.WorkerService 2.23.0, Microsoft.Azure.Functions.Worker.ApplicationInsights 2.0.0, OpenTelemetry 1.13.1, OpenTelemetry.Api 1.15.3, OpenTelemetry.Extensions.Hosting 1.13.1
   Testing:  Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit 1.1.2, Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.XUnit 1.1.2, Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing.XUnit 1.1.2, Moq 4.20.71, xunit 2.9.0, xunit.runner.visualstudio 3.1.5
   Cloud:  Azure.Core 1.44.1, Azure.Data.Tables 12.9.0, Azure.Identity 1.17.0, Azure.Storage.Blobs 12.23.0, Azure.Storage.Queues 12.21.0, Microsoft.Azure.Core.NewtonsoftJson 2.0.0, Microsoft.Azure.Functions.Worker 2.1.0, Microsoft.Azure.Functions.Worker.Core 1.20.0 … (26 total)
   Utilities:  Newtonsoft.Json 13.0.3
   Other:  @(PackageReference), AwesomeAssertions 9.3.0, AwesomeAssertions.Analyzers 9.0.8, Google.Protobuf 3.32.1, Grpc.Core 2.46.6, Grpc.Net.Client 2.55.0, Grpc.Net.ClientFactory 2.65.0, Grpc.Tools 2.72.0 … (55 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
