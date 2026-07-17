MAP  wolverine     (125 projects)

STACK  net10.0, net7.0, net9.0, net9.0;net10.0 · Minimal APIs · Controllers · MediatR (CQRS) · EF Core · FluentValidation

STYLE  CleanArchitecture  (confidence moderate)
       evidence: DDD folder layers: Infrastructure, Api, Core; MediatR with 4 handlers

       per service:
         build: Unknown
         CodeGenTarget: Unknown
         CrazyStartingWebApp: Worker Service [Worker]
         DeepMiddlewareUsage: Unknown
         NSwagDemonstrator: Unknown
         SqlServerOutboxWebService: Unknown
         StartupStyleTarget: Unknown
         StaticCodeGenDemonstrator: Unknown
         DuplicateMessageSending: Worker Service [Worker]
         LoadTesting: Unknown
         MultiTenantedEfCoreWithPostgreSQL: Unknown [EF Core]
         MultiTenantedEfCoreWithSqlServer: Unknown [EF Core]
         OrderEventSourcingSample: Unknown
         PausingAndRestartingListener: Unknown [EF Core]
         ChaosSender: Unknown
         CommandBus: Unknown
         EncryptionDemo: Unknown
         FaultEventsDemo: Unknown
         InMemoryMediator: Unknown [EF Core]
         OptimizedArtifactWorkflowSample: Worker Service [Worker]
         OrderSagaSample: Unknown
         Quickstart: Unknown
         WebApiWithMarten: Unknown
         WolverineChat: Unknown
         Benchmarks: Unknown
         ConsoleApp: Unknown
         MetricsDemonstrator: Worker Service [Worker]
         Wolverine.AotSmoke: Unknown
         Wolverine.AotSmoke.Static: Unknown
         PolecatIncidentService: Unknown
         AppHost: Unknown
         Consumer: Worker Service [Worker]
         Publisher: Worker Service [Worker]
         AppHost: Unknown
         Pinger: Worker Service [Worker]
         Ponger: Worker Service [Worker]
         TeleHealth.Backend: Unknown
         TeleHealth.WebApi: Unknown
         DiagnosticsApp: Unknown
         ItemService: Unknown [EF Core]
         Client: Unknown
         Server: gRPC Service [gRPC]
         Client: Unknown
         Server: gRPC Service [gRPC]
         Client: Unknown
         Server: gRPC Service [gRPC]
         IncidentService: Unknown
         MartenAndRabbitEmailService: Worker Service [Worker]
         MartenAndRabbitIssueService: Unknown
         AppWithMiddleware: Unknown
         MultiTenantedTodoWebService: Unknown
         InventoryServer: gRPC Service [gRPC]
         OrderClient: Unknown
         OrderServer: gRPC Service [gRPC]
         Orders: Unknown
         RetailClient: Worker Service [Worker]
         Pinger: Worker Service [Worker]
         Ponger: Unknown
         Pinger: Worker Service [Worker]
         Ponger: gRPC Service [gRPC]
         Pinger: Worker Service [Worker]
         Ponger: gRPC Service [gRPC]
         Pinger: Worker Service [Worker]
         Ponger: Worker Service [Worker]
         ProcessManagerViaHandlers: Unknown
         Client: Unknown
         Server: gRPC Service [gRPC]
         RacerClient: Unknown
         RacerServer: gRPC Service [gRPC]
         BankingService: Unknown
         TodoWebService: Unknown
         OtelWebApi: Unknown
         OtelWebApiWolverineMarten: Unknown
         Subscriber1: Worker Service [Worker]
         Subscriber2: Worker Service [Worker]
         WebAppWithSQS: Unknown
         BatchMessaging: Unknown
         RabbitMqBootstrapping: Unknown
         BackLogService: Unknown

TOPOLOGY (depends-on)
   Wolverine
   Wolverine.Marten ── Wolverine.Postgresql
   Messages ── Wolverine.Grpc
   Messages
   Messages
   Messages ── Wolverine
   Messages
   Messages
   Messages
   Messages ── Wolverine.Grpc
   Wolverine.Http ── Wolverine
   Wolverine.RabbitMQ ── Wolverine, Wolverine.Newtonsoft
   Wolverine.Grpc ── Wolverine
   Wolverine.Http.Marten ── Wolverine.Http, Wolverine.Marten
   Wolverine.EntityFrameworkCore ── Wolverine.RDBMS
   Wolverine.RDBMS ── Wolverine
   Wolverine.FluentValidation ── Wolverine
   Contracts
   Wolverine.Newtonsoft ── Wolverine
   Wolverine.SqlServer ── Wolverine.RDBMS
   MartenAndRabbitMessages
   RacerContracts
   SharedPersistenceModels ── Wolverine.EntityFrameworkCore, Wolverine.Http
   TeleHealth.Common ── Wolverine.Marten
   Wolverine.AmazonSqs ── Wolverine, Wolverine.Newtonsoft
   Wolverine.Postgresql ── Wolverine.RDBMS
   DiagnosticsModule ── Wolverine
   Orders ── Messages, Wolverine.Marten, Wolverine.RabbitMQ
   Wolverine.AdminApi ── Wolverine
   Wolverine.FluentValidation.Grpc ── Wolverine.FluentValidation, Wolverine.Grpc
   Wolverine.Http.FluentValidation ── Wolverine.FluentValidation, Wolverine.Http
   Wolverine.Http.Polecat ── Wolverine.Http, Wolverine.Polecat
   Wolverine.Kafka ── Wolverine
   Wolverine.Polecat ── Wolverine.SqlServer
   Wolverine.SignalR ── Wolverine
   AppWithMiddleware ── Wolverine.FluentValidation, Wolverine.Marten, Wolverine.RabbitMQ
   BackLogService ── Wolverine.EntityFrameworkCore, Wolverine.Postgresql
   BankingService ── Wolverine, Wolverine.Http
   BatchMessaging ── Wolverine.Kafka
   build
   ChaosSender ── Wolverine.AdminApi, Wolverine.Marten, Wolverine.RabbitMQ
   Client ── Messages
   Client ── Messages
   Client ── Messages
   Client ── Messages
   CommandBus ── Wolverine, Wolverine.Http.Marten, Wolverine.Marten
   ConsoleApp ── Wolverine, Wolverine.ComplianceTests, Wolverine.RabbitMQ
   CrazyStartingWebApp ── Wolverine.Http
   DeepMiddlewareUsage ── Wolverine.Http.Marten
   DiagnosticsApp ── DiagnosticsModule, Wolverine.Http, Wolverine.Marten, Wolverine.RabbitMQ
   … and 75 more projects (use --focus for a scoped slice)

EVENT WIRING  (5 integration events, 0 cross-service)
  Ping: DuplicateMessageSending · (no consumer)
  ServiceBusMessage: Wolverine.AzureServiceBus · (no consumer)
  TMessage: Wolverine · (no consumer)
  TResponse: Wolverine · (no consumer)
  TestEvent: SqlServerOutboxWebService · (no consumer)

ENTRY POINTS
   HTTP (17)
      DELETE /  → WolverineRequiredException  (src/Http/Wolverine.Http/WolverineHttpEndpointRouteBuilderExtensions.cs:152)
      DELETE / [DeadLettersEndpointExtensions:55]  → IWolverineRuntime  (src/Http/Wolverine.Http/DeadLettersEndpointExtensions.cs:55)
      DELETE / [WolverineHttpEndpointRouteBuilderExtensions:95]  → WolverineRequiredException  (src/Http/Wolverine.Http/WolverineHttpEndpointRouteBuilderExtensions.cs:95)
      GET /api/nodes  → IWolverineRuntime  (src/Http/Wolverine.AdminApi/WolverineAdminApiExtensions.cs:21)
      GET /views/nodes  → WolverineAdminApiExtensions  (src/Http/Wolverine.AdminApi/WolverineAdminApiExtensions.cs:28)
      POST /  → WolverineRequiredException  (src/Http/Wolverine.Http/WolverineHttpEndpointRouteBuilderExtensions.cs:114)
      POST / [DeadLettersEndpointExtensions:37]  → DeadLetterEnvelopeIdsRequest  (src/Http/Wolverine.Http/DeadLettersEndpointExtensions.cs:37)
      POST / [WolverineHttpEndpointRouteBuilderExtensions:63]  → WolverineRequiredException  (src/Http/Wolverine.Http/WolverineHttpEndpointRouteBuilderExtensions.cs:63)
      POST /batch/{queue}  → HttpTransportExtensions  (src/Http/Wolverine.Http/Transport/HttpTransportExtensions.cs:23)
      POST /invoke  → HttpTransportExtensions  (src/Http/Wolverine.Http/Transport/HttpTransportExtensions.cs:27)
      POST /orders/itemready  → MartenOutbox.Enroll  (src/Persistence/OrderEventSourcingSample/Order.cs:76)
      POST /replay  → IWolverineRuntime  (src/Http/Wolverine.Http/DeadLettersEndpointExtensions.cs:44)
      POST /test  → TestMessagesHandler  (src/Transports/Kafka/BatchMessaging/Program.cs:33)
      PUT /  → WolverineRequiredException  (src/Http/Wolverine.Http/WolverineHttpEndpointRouteBuilderExtensions.cs:133)
      PUT / [WolverineHttpEndpointRouteBuilderExtensions:79]  → WolverineRequiredException  (src/Http/Wolverine.Http/WolverineHttpEndpointRouteBuilderExtensions.cs:79)
      GET /  (src/Persistence/OrderEventSourcingSample/Program.cs:47)
      POST /items/ready  (src/Persistence/OrderEventSourcingSample/Program.cs:46)
   Background (7)
      AzureServiceBusDeadLetterQueueListener  → AzureServiceBusDeadLetterQueueListener  (src/Transports/Azure/Wolverine.AzureServiceBus/AzureServiceBusConfiguration.cs:241)
      ClientHostedService  → ClientHostedService  (src/Http/CrazyStartingWebApp/Program.cs:19)
      DeadLetterQueueListener  → DeadLetterQueueListener  (src/Transports/RabbitMQ/Wolverine.RabbitMQ/Internal/RabbitMqTransportExpression.cs:334)
      DeadLetterQueueReplayer  → DeadLetterQueueReplayer  (src/Persistence/Wolverine.RavenDb/WolverineRavenDbExtensions.cs:37)
      HeartbeatBackgroundService  → HeartbeatBackgroundService  (src/Wolverine/WolverineOptionsExtensions.cs:37)
      Sender  → Sender  (src/Persistence/DuplicateMessageSending/Program.cs:48)
      SqsDeadLetterQueueListener  → SqsDeadLetterQueueListener  (src/Transports/AWS/Wolverine.AmazonSqs/Internal/AmazonSqsTransportConfiguration.cs:210)
   gRPC (4)
      WolverineTransport.Call  → WolverineRuntime.FindInvoker  (src/Wolverine.Grpc/Internals/WolverineGrpcTransportService.cs:13)
      WolverineTransport.Ping  → WolverineGrpcTransportService  (src/Wolverine.Grpc/Internals/WolverineGrpcTransportService.cs:13)
      WolverineTransport.Send  → EnvelopeSerializer.Deserialize  (src/Wolverine.Grpc/Internals/WolverineGrpcTransportService.cs:13)
      WolverineTransport.SendBatch  → EnvelopeSerializer.ReadMany  (src/Wolverine.Grpc/Internals/WolverineGrpcTransportService.cs:13)
   SignalR (1)
      WolverineHub (1 methods: ReceiveMessage)  → SignalRTransport.ReceiveAsync  (src/Transports/SignalR/Wolverine.SignalR/WolverineHub.cs:9)

PACKAGES
   Web/API:  Grpc.AspNetCore 2.76.0, Marten.AspNetCore 9.15.1, Microsoft.AspNetCore.Mvc.Testing 9.0.14, Microsoft.AspNetCore.OpenApi 9.0.5, Microsoft.AspNetCore.SignalR 1.2.9, Microsoft.AspNetCore.SignalR.Client 9.0.8, NSwag.AspNetCore 14.0.0, OpenTelemetry.Instrumentation.AspNetCore 1.15.2 … (11 total)
   ORM/Data:  Microsoft.Azure.Cosmos 3.46.1, Microsoft.EntityFrameworkCore 10.0.2, Microsoft.EntityFrameworkCore.Design 10.0.2, Microsoft.EntityFrameworkCore.InMemory 9.0.5, Microsoft.EntityFrameworkCore.Relational 10.0.2, Microsoft.EntityFrameworkCore.SqlServer 10.0.0, MySqlConnector 2.4.0, Npgsql 9.0.3 … (14 total)
   Messaging:  Aspire.Hosting.RabbitMQ 9.1.0, Azure.Messaging.ServiceBus 7.20.1, RabbitMQ.Client 7.1.2
   Validation:  FluentValidation 12.0.0
   Logging:  OpenTelemetry 1.15.3, OpenTelemetry.Api 1.15.3, OpenTelemetry.Exporter.Console 1.15.3, OpenTelemetry.Exporter.Jaeger, OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.3, OpenTelemetry.Extensions.Hosting 1.15.3, OpenTelemetry.Instrumentation.Http 1.15.1
   Testing:  coverlet.collector 6.0.4, FluentAssertions 6.12.2, Meziantou.Extensions.Logging.Xunit 1.0.15, NSubstitute 5.3.0, Shouldly 4.3.0, Testcontainers 4.12.0, Testcontainers.Kafka 4.12.0, Testcontainers.Nats 4.12.0 … (13 total)
   Cloud:  Azure.Identity 1.17.0, Azure.Storage.Blobs 12.27.0, Microsoft.Azure.SignalR 1.32.0
   Utilities:  Marten.Newtonsoft 9.15.1, Newtonsoft.Json 13.0.3, Refit 8.0.0, Refit.HttpClientFactory 8.0.0
   Other:  Alba 8.5.2, Apache.Avro 1.12.1, Asp.Versioning.Abstractions [10.0.0,11.0.0), Asp.Versioning.Http [10.0.0,11.0.0), Asp.Versioning.Mvc.ApiExplorer [10.0.0,11.0.0), Aspire.Hosting.Kafka 9.1.0, AWSSDK.S3 4.0.22.1, AWSSDK.SimpleNotificationService 4.0.2.14 … (70 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "WolverineTransport.Call")
