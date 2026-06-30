MAP  unknown     (29 projects)

STACK  net8.0;net9.0;net10.0, netstandard2.0, netstandard2.0;net8.0;net9.0;net10.0, netstandard2.1 · EF Core · MassTransit

STYLE  NLayer  (confidence moderate)
       evidence: EF Core + 29 projects; folder roles: Infrastructure, Core

TOPOLOGY (depends-on)
   MassTransit ── MassTransit.Abstractions
   MassTransit.Abstractions
   MassTransit.Azure.ServiceBus.Core ── MassTransit
   MassTransit.Newtonsoft ── MassTransit
   MassTransit.WebJobs.ServiceBusIntegration ── MassTransit, MassTransit.Azure.ServiceBus.Core
   MassTransit.ActiveMqTransport ── MassTransit
   MassTransit.AmazonS3 ── MassTransit
   MassTransit.AmazonSqsTransport ── MassTransit
   MassTransit.Analyzers
   MassTransit.Azure.Cosmos ── MassTransit
   MassTransit.Azure.Storage ── MassTransit
   MassTransit.Azure.Table ── MassTransit
   MassTransit.DapperIntegration ── MassTransit
   MassTransit.EntityFrameworkCoreIntegration ── MassTransit
   MassTransit.EntityFrameworkIntegration ── MassTransit
   MassTransit.HangfireIntegration ── MassTransit, MassTransit.Newtonsoft
   MassTransit.Interop.NServiceBus ── MassTransit, MassTransit.Newtonsoft
   MassTransit.KafkaIntegration ── MassTransit
   MassTransit.MartenIntegration ── MassTransit
   MassTransit.MessagePack ── MassTransit, MassTransit.Abstractions
   MassTransit.NHibernateIntegration ── MassTransit
   MassTransit.QuartzIntegration ── MassTransit
   MassTransit.RabbitMqTransport ── MassTransit
   MassTransit.RedisIntegration ── MassTransit
   MassTransit.SignalR ── MassTransit
   MassTransit.SqlTransport.PostgreSql ── MassTransit
   MassTransit.SqlTransport.SqlServer ── MassTransit
   MassTransit.StateMachineVisualizer ── MassTransit
   MassTransit.WebJobs.EventHubsIntegration ── MassTransit, MassTransit.Azure.ServiceBus.Core, MassTransit.WebJobs.ServiceBusIntegration

ENTRY POINTS
   Bus (26)
      AddConsumer  (Scheduling/MassTransit.QuartzIntegration/Configuration/QuartzRegistrationExtensions.cs:32)
      AddMassTransit  (Transports/MassTransit.WebJobs.ServiceBusIntegration/Configuration/AzureFunctionsBusConfigurationExtensions.cs:36)
      AddMediator  (MassTransit/Configuration/DependencyInjection/DependencyInjectionHostingExtensions.cs:82)
      AllConsumer  (MassTransit.SignalR/Consumers/AllConsumer.cs:12)
      BatchConsumer  (MassTransit/Consumers/Batching/BatchConsumer.cs:14)
      CancelScheduledMessageConsumer  (Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/CancelScheduledMessageConsumer.cs:8)
      ConnectionConsumer  (MassTransit.SignalR/Consumers/ConnectionConsumer.cs:11)
      FinalizeJobConsumer  (MassTransit/JobService/JobService/FinalizeJobConsumer.cs:10)
      GroupConsumer  (MassTransit.SignalR/Consumers/GroupConsumer.cs:12)
      GroupManagementConsumer  (MassTransit.SignalR/Consumers/GroupManagementConsumer.cs:8)
      MediatorRequestHandler  (MassTransit.Abstractions/Mediator/MediatorRequestHandler.cs:31)
      MessageHandlerConsumer  (MassTransit/DependencyInjection/DependencyInjection/MessageHandlerConsumer.cs:72)
      PauseScheduledMessageConsumer  (Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/PauseScheduledMessageConsumer.cs:8)
      PauseScheduledRecurringMessageConsumer  (Scheduling/MassTransit.HangfireIntegration/HangfireIntegration/PauseScheduledRecurringMessageConsumer.cs:10)
      RequestHandlerConsumer  (MassTransit/DependencyInjection/DependencyInjection/RequestHandlerConsumer.cs:82)
      ResumeScheduledMessageConsumer  (Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/ResumeScheduledMessageConsumer.cs:8)
      ResumeScheduledRecurringMessageConsumer  (Scheduling/MassTransit.HangfireIntegration/HangfireIntegration/ResumeScheduledMessageConsumer.cs:10)
      RoutingSlipRequestProxy  (MassTransit/Courier/RoutingSlipRequestProxy.cs:8)
      RoutingSlipResponseProxy  (MassTransit/Courier/RoutingSlipResponseProxy.cs:11)
      ScheduleMessageConsumer  (Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/ScheduleMessageConsumer.cs:16)
      … and 6 more (bus entries — use --focus for a drill-in)

PACKAGES
   Web/API:  Microsoft.ApplicationInsights.AspNetCore, Microsoft.AspNetCore.SignalR.Core
   ORM/Data:  Dapper, dapper.contrib, EntityFramework, Microsoft.Azure.Cosmos, Microsoft.EntityFrameworkCore.Relational, Npgsql
   Messaging:  Apache.NMS.AMQP, Azure.Messaging.ServiceBus, RabbitMQ.Client
   Testing:  NUnit, NUnit.Analyzers
   Cloud:  Azure.Data.Tables, Azure.Identity, Azure.Storage.Blobs, Microsoft.Azure.WebJobs.Extensions.EventHubs, Microsoft.Azure.WebJobs.Extensions.ServiceBus
   Utilities:  Newtonsoft.Json, Newtonsoft.Json.Bson
   Other:  Apache.NMS.ActiveMQ, AWSSDK.S3, AWSSDK.SimpleNotificationService, AWSSDK.SQS, Confluent.Kafka, Hangfire.Core, Iesi.Collections, Marten … (35 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
