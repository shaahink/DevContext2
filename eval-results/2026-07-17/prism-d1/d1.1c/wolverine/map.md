LIBRARY  wolverine     (1322 public types)

ENTRY API
   register  AncillaryWolverineOptionsMartenExtensions.ProcessEventsWithWolverineHandlersInStrictOrder   (AncillaryWolverineOptionsMartenExtensions.cs)
      Create a subscription for Marten events to be processed in strict order by Wolverine
   register  AncillaryWolverineOptionsMartenExtensions.PublishEventsToWolverine   (AncillaryWolverineOptionsMartenExtensions.cs)
      Relay events captured by Marten to Wolverine message publishing
   register  AncillaryWolverineOptionsMartenExtensions.SubscribeToEvents   (AncillaryWolverineOptionsMartenExtensions.cs)
      Register a custom subscription that will process a batch of Marten events at a time with a user defined action
   register  AncillaryWolverineOptionsMartenExtensions.SubscribeToEventsWithServices   (AncillaryWolverineOptionsMartenExtensions.cs)
      Add a subscription built by the IoC container to a separate Marten IDocumentStore
   register  AncillaryWolverineOptionsPolecatExtensions.ProcessEventsWithWolverineHandlersInStrictOrder   (AncillaryWolverineOptionsPolecatExtensions.cs)
      Create a subscription for Polecat events to be processed in strict order by Wolverine for an ancillary store
   register  AncillaryWolverineOptionsPolecatExtensions.PublishEventsToWolverine   (AncillaryWolverineOptionsPolecatExtensions.cs)
      Relay events captured by Polecat to Wolverine message publishing for an ancillary store
   register  AncillaryWolverineOptionsPolecatExtensions.SubscribeToEvents   (AncillaryWolverineOptionsPolecatExtensions.cs)
      Register a custom subscription that will process a batch of Polecat events at a time with a user defined action for a...
   register  AncillaryWolverineOptionsPolecatExtensions.SubscribeToEventsWithServices   (AncillaryWolverineOptionsPolecatExtensions.cs)
      Add a subscription built by the IoC container to a separate Polecat IDocumentStore
   register  DeadLettersEndpointExtensions.MapDeadLettersEndpoints   (DeadLettersEndpointExtensions.cs)
      Add endpoints to manage the Wolverine database-backed deal letter queue for this application.
   register  HandlerStubbingExtensions.ClearAllWolverineStubs   (HandlerStubbingExtensions.cs)
      Clears out any registered message handler stubs in this Wolverine application and restores the system to its original...
   register  HandlerStubbingExtensions.StubWolverineMessageHandling   (HandlerStubbingExtensions.cs)
      Register stubbed behavior within the Wolverine application for any messages of the TRequest type Use this to efficien...
   register  HandlerStubbingExtensions.WolverineStubs   (HandlerStubbingExtensions.cs)
      Configure stubbed message handlers within an application using Wolverine.

ABSTRACTIONS
   Saga (class)  — 77 implementors
   IListener (interface)  — 43 implementors
   Endpoint (class)  — 38 implementors
   ISender (interface)  — 37 implementors
   IWolverineExtension (interface)  — 25 implementors
   IAgentCommand (interface)  — 22 implementors
   IMessageSerializer (interface)  — 21 implementors
   IContinuation (interface)  — 20 implementors
   IHandlerPolicy (interface)  — 20 implementors
   ListenerConfiguration (class)  — 20 implementors

PUBLIC SURFACE
   DeepMiddlewareUsage
      Participant (class)
      Trainer (class)
      TrainerGet (class):  Get
      TrainerMiddleware (class):  LoadAsync
      TrainerResponse (record)
      UserId (record)
      UserIdMiddleWare (class):  Load
   Google.Cloud.PubSub.V1
      SubscriptionNameExtensions (class):  WithAssignedNodeNumber
   Helpdesk.Api.Incidents
      AcknowledgeResolution (record)
      AssignAgentToIncident (record)
      PrioritiseIncident (record)
      RecordAgentResponseToIncident (record)
      RecordCustomerResponseToIncident (record)
      ResolveIncident (record)
   IntegrationTests
      MessageController (class):  Enqueue, Invoke
      Servers (class)
      XunitLogger (class):  BeginScope, IsEnabled, Log, XunitLogger
         Logger that writes to xUnit test output
      XunitLoggerProvider (class):  CreateLogger, Dispose, XunitLoggerProvider
         Logger provider that writes to xUnit test output
      XunitLoggingExtensions (class):  AddXunitLogging
         Extension methods for configuring xUnit logging
   Internal.Generated.WolverineHandlers
      GET_api_test (class):  GET_api_test, Handle
      GET_api_trainer (class):  GET_api_trainer, Handle
      POST_api_items (class):  Handle, POST_api_items
   Module1
      IInterfaceMessage (interface)
      IMessageAbstraction (interface)
      IModuleService (interface)
      Module1Extension (class):  Configure
      ServiceFromModule (class)
      TalkCommand (class):  Execute
   Module2
      MessageHandler (class):  Handle
      Module2Message1 (record)
      Module2Message2 (record)
      Module2Message3 (record)
      Module2Message4 (record)
   MultiTenantedEfCoreWithPostgreSQL
      Program (class):  Main
   MultiTenantedEfCoreWithSqlServer
      Program (class):  Main
   OrderExtension
      CreateOrder (class)
      OrderHandler (class):  HandleAsync
      ShipOrder (class)
   OtelMessages
      InitialCommand (record)
      InitialPost (class):  InitialPost
      LocalMessage1 (record)
      LocalMessage2 (record)
      LocalMessage3 (record)
      LocalMessage4 (record)
      MessagingConstants (class)
      RabbitMessage1 (class)
      RabbitMessage2 (class)
      RabbitMessage3 (class)
      TcpMessage1 (record)
      TcpMessage2 (record)
   OtelWebApi
      InitialCommandHandler (class):  Handle
   PolecatIncidentService
      AgentAssignedToIncident (record)
      AgentRespondedToIncident (record)
      ArchiveIncident (record)
      ArchiveIncidentHandler (class):  Handle
      CategoriseIncident (record)
      CategoriseIncidentEndpoint (class):  Post, Validate
      CategoriseIncidentPayload (record)
      CategoriseViaAsParameters (record)
      CategoriseViaAsParametersEndpoint (class):  Post
      CloseIncident (record)
      CloseIncidentEndpoint (class):  Handle
      Contact (record)
      … and 15 more (use --format json for the full surface)
   SharedPersistenceModels
      TestingOverrides (class)
   SharedPersistenceModels.Items
      ApproveItem1 (record)
      ApproveItem1Handler (class):  Handle
      ApproveItem2 (record)
      ApproveItem2Handler (class):  Handle, LoadAsync
      ApproveItem3 (record)
      ApproveItem3Handler (class):  Handle
      Entity (class):  Publish
      IDomainEvent (interface)
      IEntity (interface)
      Item (class):  Approve
      ItemApproved (record)
      ItemEndpoints (class):  GetAll, GetItem, GetItem2
      … and 7 more (use --format json for the full surface)
   SharedPersistenceModels.Orders
      ApproveOrder (record)
      CreditLimitExceeded (record)
      CreditReserved (record)
      Order (class):  Handle, Start
      OrderApproved (record)
      OrderCompleted (record)
      OrderCreated (record)
      OrderPlaced (record)
      OrderRejected (record)
      OrdersDbContext (class):  OrdersDbContext
      PlaceOrder (record)
      RejectOrder (record)
      … and 3 more (use --format json for the full surface)
   StartupStyleTarget
      CreateItemRequest (record)
      Program (class):  Main
      Startup (class):  Configure, ConfigureServices
      TestEndpoint (class):  Get, Post
   Subscriber1
      Subscriber1Handlers (class):  Handle
      Subscriber2Handlers (class):  Handle
      Worker (class):  Worker
   Subscriber2
      Worker (class):  Worker
   Wolverine
      ApplyItemException (class):  DeadLetter, DeadLetterAndAckOthers, DeadLetterAndReplayOthers
         Thrown from a batch message handler that already knows which item(s) in the batch are "poison" (deterministically bad...
      AssignmentWaiter (class):  AssignmentWaiter, ExpectRunningAgents, HasReached, StartAsync
      BrokerName (record)
         Just identifies a named broker for the case of needing to connect a single Wolverine application to multiple message ...
      ConfiguredMessageExtensions (class):  DelayedFor, ScheduleToGroup, ScheduledAt, ToDestination, ToEndpoint, ToTopic, WithDeliveryOptions, WithGroupId, WithTenantId
      DeliveryMessage (class):  ApplyAsync, DeliveryMessage
      DeliveryOptions (class):  RequireResponse, WithHeader
         Optional customizations and metadata for how a message should be delivered
      DurabilitySettings (class):  ToDescription
      Envelope (class):  Envelope, Equals, ForPersistedHandled, ForPing, GetDataAsync, GetHashCode, IsExpired, IsPing, IsScheduledForLater, ScheduleAt, ScheduleDelayed, SetMessageType, SetMetricsTag, ToString, TryGetHeader
      EnvelopeConstants (class)
      ErrorReport (class):  ErrorReport
      ExceptionInfo (record):  From
         Wire-stable, serializable snapshot of an exception.
      ExpectationExpression (class):  ExpectationExpression, RespondWith
      … and 58 more (use --format json for the full surface)
   Wolverine.AdminApi
      WolverineAdminApiExtensions (class):  MapWolverineAdminApiEndpoints
   Wolverine.AmazonSns
      AmazonSnsListenerConfiguration (class)
      AmazonSnsSubscriberConfiguration (class):  ConfigureTopicCreation, InteropWith, InteropWithCloudEvents, SendRawJsonMessage, SubscribeSqsQueue, UseInterop, UseMassTransitInterop, UseNServiceBusInterop
      AmazonSnsTransportExtensions (class):  AddNamedAmazonSnsBroker, ConfigureAmazonSnsTransport, ToSnsTopic, ToSnsTopicOnNamedBroker, UseAmazonSnsTransport, UseAmazonSnsTransportLocally, UseAmazonSnsTransportLocallyAsNamedBroker
      ISnsEnvelopeMapper (interface):  BuildMessageBody, ReadEnvelopeData, ToAttributes
         Pluggable strategy for reading and writing data to Amazon SNS topics
      SnsEndpointUri (class):  Topic
         Builds canonical Wolverine endpoint values for AWS SNS transport endpoints.
      WolverineSnsTransportException (class):  WolverineSnsTransportException
   Wolverine.AmazonSqs
      AmazonSqsListenerConfiguration (class):  CircuitBreaker, ConfigureDeadLetterQueue, ConfigureQueueCreation, DisableDeadLetterQueueing, InteropWith, InteropWithCloudEvents, ReceiveRawJsonMessage, ReceiveSnsTopicMessage, UseMassTransitInterop, UseNServiceBusInterop
      AmazonSqsMessageRoutingConvention (class):  QueueNameForSender
      AmazonSqsSubscriberConfiguration (class):  ConfigureQueueCreation, EnableFairQueueMessageGroups, InteropWith, InteropWithCloudEvents, SendRawJsonMessage, UseInterop, UseMassTransitInterop, UseNServiceBusInterop
      AmazonSqsTransportExtensions (class):  AddNamedAmazonSqsBroker, ConfigureAmazonSqsTransport, ListenToSqsQueue, ListenToSqsQueueOnNamedBroker, PublishToShardedAmazonSqsQueues, ToSqsQueue, ToSqsQueueOnNamedBroker, UseAmazonSqsTransport, UseAmazonSqsTransportLocally, UseAmazonSqsTransportLocallyAsNamedBroker, UseShardedAmazonSqsQueues
      CreateQueueRequestExtensions (class):  MaximumMessageSize, MessageRetentionPeriod, ReceiveMessageWaitTimeSeconds, VisibilityTimeout
      DateTimeOffsetHelper (class):  ToDateTimeOffset, ToWireFormattedString
      DefaultSqsEnvelopeMapper (class):  BuildMessageBody, DetermineGroupId, ReadEnvelopeData, ToAttributes
      ISqsEnvelopeMapper (interface):  BuildMessageBody, DetermineGroupId, ReadEnvelopeData, ToAttributes
         Pluggable strategy for reading and writing data to Amazon SQS queues
      SqsEndpointUri (class):  Queue
         Builds canonical Wolverine endpoint values for AWS SQS transport endpoints.
      WolverineSqsTransportException (class):  WolverineSqsTransportException
   Wolverine.Attributes
      AlwaysPublishResponseAttribute (class):  Modify
         Decorate any handler method or class if you always want any response (the "T" in IMessageBus.InvokeAsync()) to be *al...
      AuditAttribute (class):  AuditAttribute
         Marks a member on a message type to be audited in message activity logging, Open Telemetry activity tags, and in perf...
      DeliverWithinAttribute (class):  DeliverWithinAttribute, Modify
         Directs Wolverine that any message of this type must be delivered within the supplied number of seconds, or it should...
      ExcludeFromServiceCapabilitiesAttribute (class)
         When applied at the assembly level, tells Wolverine to exclude all message types and handlers from this assembly when...
      IdempotentAttribute (class):  Modify
         Adds idempotency checks to this message handler ONLY use this for message handlers that do not use transactional midd...
      InteropMessageAttribute (class):  InteropMessageAttribute
         This attribute helps Wolverine "know" that when interoperating with another messaging tool like NServiceBus or MassTr...
      LocalQueueAttribute (class):  LocalQueueAttribute
         Directs Wolverine to send this to the named local queue when using IMessageContext.Enqueue(message)
      MaximumAttemptsAttribute (class):  MaximumAttemptsAttribute, Modify
         Specify the maximum number of attempts to process a received message
      MessageIdentityAttribute (class):  GetName, MessageIdentityAttribute
         Used to override Wolverine's default behavior for identifying a message type.
      MessageTimeoutAttribute (class):  MessageTimeoutAttribute, Modify
      MiddlewareAttribute (class):  MiddlewareAttribute, Modify
         Attach one or more Wolverine conventional middleware types
      ModifyChainAttribute (class):  Modify
         Base class to use for applying middleware or other alterations to generic IChains (either RouteChain or HandlerChain)
      … and 24 more (use --format json for the full surface)
   Wolverine.AzureServiceBus
      AzureServiceBusConfiguration (class):  AddTenantByConnectionString, AddTenantByNamespace, AzureServiceBusConfiguration, ConfigureQueue, DeleteAllExistingObjectsOnStartup, EnableDeadLetterQueueRecovery, EnableWolverineControlQueues, SystemQueuesAreEnabled, TenantIdBehavior, UseConventionalRouting, UseTopicAndSubscriptionConventionalRouting
      AzureServiceBusEmulatorExtensions (class):  DeleteAllAzureServiceBusObjectsAsync, UseAzureServiceBusEmulator
         Helpers for connecting Wolverine to a locally running Azure Service Bus emulator.
      AzureServiceBusEndpointUri (class):  Queue, Subscription, Topic
         Builds canonical Wolverine endpoint values for Azure Service Bus transport endpoints.
      AzureServiceBusMessageRoutingConvention (class):  QueueNameForSender
      AzureServiceBusQueueListenerConfiguration (class):  AzureServiceBusQueueListenerConfiguration, CircuitBreaker, ConfigureDeadLetterQueue, ConfigureProcessor, ConfigureQueue, DisableDeadLetterQueueing, ExclusiveNodeWithSessions, InteropWith, MaximumMessagesToReceive, MaximumWaitTime, RequireSessions, UseMassTransitInterop, UseNServiceBusInterop
      AzureServiceBusQueueSubscriberConfiguration (class):  AzureServiceBusQueueSubscriberConfiguration, ConfigureQueue, InteropWith, RequireSessions, UseMassTransitInterop, UseNServiceBusInterop
      AzureServiceBusSubscriptionListenerConfiguration (class):  AzureServiceBusSubscriptionListenerConfiguration, CircuitBreaker, ConfigureProcessor, ConfigureSubscription, ConfigureSubscriptionRule, ConfigureTopic, ExclusiveNodeWithSessions, InteropWith, MaximumMessagesToReceive, MaximumWaitTime, RequireSessions, UseMassTransitInterop, UseNServiceBusInterop
      AzureServiceBusTenant (class):  AzureServiceBusTenant, Compile
      AzureServiceBusTopicBroadcastingRoutingConvention (class):  SubscriptionNameForListener, TopicNameForListener, TopicNameForSender
      AzureServiceBusTopicSubscriberConfiguration (class):  AzureServiceBusTopicSubscriberConfiguration, ConfigureTopic, InteropWith, UseMassTransitInterop, UseNServiceBusInterop
      AzureServiceBusTransport (class):  AzureServiceBusTransport, BuildHealthCheck, BuildListenerForSubscription, ConnectAsync, DeleteAllObjectsAsync, DescribeEndpoint, DiagnosticColumns, DisposeAsync, ReplyEndpoint, SanitizeIdentifier, WithManagementClientAsync, WithServiceBusClientAsync
      AzureServiceBusTransportExtensions (class):  AddNamedAzureServiceBusBroker, ConfigureAzureServiceBus, DeleteAllAzureServiceBusObjectsAsync, ListenToAzureServiceBusQueue, ListenToAzureServiceBusQueueOnNamedBroker, ListenToAzureServiceBusSubscription, ListenToAzureServiceBusSubscriptionOnNamedBroker, PublishToShardedAzureServiceBusQueues, ToAzureServiceBusQueue, ToAzureServiceBusQueueOnNamedBroker, ToAzureServiceBusTopic, ToAzureServiceBusTopicOnNamedBroker, UseAzureServiceBus, UseShardedAzureServiceBusQueues
      … and 2 more (use --format json for the full surface)
   … and 129 more namespaces (use --format json for the full surface)
   INTERNAL  (148 types in *.Internal — available on request)

CONSUMER PATHS
   wire into DI  →  AncillaryWolverineOptionsMartenExtensions.ProcessEventsWithWolverineHandlersInStrictOrder(...)
   wire into DI  →  AncillaryWolverineOptionsMartenExtensions.PublishEventsToWolverine(...)
   wire into DI  →  AncillaryWolverineOptionsMartenExtensions.SubscribeToEvents(...)
   wire into DI  →  AncillaryWolverineOptionsMartenExtensions.SubscribeToEventsWithServices(...)
   wire into DI  →  AncillaryWolverineOptionsPolecatExtensions.ProcessEventsWithWolverineHandlersInStrictOrder(...)
   wire into DI  →  AncillaryWolverineOptionsPolecatExtensions.PublishEventsToWolverine(...)

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
   Bus (20)
      ApproveItem1Handler  → ApproveItem1Handler  (src/Persistence/SharedPersistenceModels/Items/ApproveItem1.cs:9)
      ApproveItem2Handler  → ApproveItem2Handler  (src/Persistence/SharedPersistenceModels/Items/ApproveItem2.cs:15)
      ApproveItem3Handler  → ApproveItem3Handler  (src/Persistence/SharedPersistenceModels/Items/ApproveItem3.cs:10)
      ArchiveIncidentHandler  → ArchiveIncidentHandler  (src/Persistence/Polecat/PolecatIncidentService/ArchiveIncident.cs:11)
      BadMessageHandler  → BadMessageHandler  (src/Http/StaticCodeGenDemonstrator/Program.cs:57)
      CommitToSprintHandler  → CommitToSprintHandler  (src/Persistence/EFCore/DomainEventsWithEfCore/BackLogService/UseEventPublisher/Code.cs:131)
      ContinueTripHandler  → ContinueTripHandler  (src/Persistence/LoadTesting/Publisher.cs:94)
      HttpHandler  → HttpHandler  (src/Http/Wolverine.Http/HttpHandler.cs:75)
      MarkItemReady2Handler  → MarkItemReady2Handler  (src/Persistence/OrderEventSourcingSample/Order.cs:283)
      MarkItemReadyHandler  → MarkItemReadyHandler  (src/Persistence/OrderEventSourcingSample/Alternatives/Signatures.cs:25)
      MessageHandler  → MessageHandler  (src/Persistence/DuplicateMessageSending/Program.cs:97)
      RepairRequestedHandler  → RepairRequestedHandler  (src/Persistence/LoadTesting/Repairs/RepairRequestedHandler.cs:23)
      StartAndTriggerApprovalHandler  → StartAndTriggerApprovalHandler  (src/Persistence/SharedPersistenceModels/Items/StartNewItem.cs:28)
      StartNewItemHandler  → StartNewItemHandler  (src/Persistence/SharedPersistenceModels/Items/StartNewItem.cs:12)
      StartOrderHandler  → StartOrderHandler  (src/Persistence/SharedPersistenceModels/Orders/Order.cs:47)
      StartTripHandler  → StartTripHandler  (src/Persistence/LoadTesting/Trips/StartTripHandler.cs:24)
      TestMessagesHandler  → TestMessagesHandler  (src/Transports/Kafka/BatchMessaging/Program.cs:51)
      TodoCreatedHandler  → TodoCreatedHandler  (src/Http/NSwagDemonstrator/Endpoints.cs:61)
      TripMessageHandler  → TripMessageHandler  (src/Persistence/LoadTesting/Trips/TripMessageHandler.cs:54)
      ValidatedMarkItemReadyHandler  → ValidatedMarkItemReadyHandler  (src/Persistence/OrderEventSourcingSample/Order.cs:369)
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
   Web/API:  Grpc.AspNetCore 2.76.0, Marten.AspNetCore 9.15.1, Microsoft.AspNetCore.OpenApi 9.0.5, Microsoft.AspNetCore.SignalR.Client 9.0.8, OpenTelemetry.Instrumentation.AspNetCore 1.15.2, protobuf-net.Grpc.AspNetCore 1.2.2, Swashbuckle.AspNetCore 6.5.0
   ORM/Data:  Microsoft.Azure.Cosmos 3.46.1, Microsoft.EntityFrameworkCore 10.0.2, Microsoft.EntityFrameworkCore.Design 10.0.2, Microsoft.EntityFrameworkCore.Relational 10.0.2, Microsoft.EntityFrameworkCore.SqlServer 10.0.0, MySqlConnector 2.4.0, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0-rc.2, Weasel.EntityFrameworkCore 9.16.2 … (11 total)
   Messaging:  Azure.Messaging.ServiceBus 7.20.1, RabbitMQ.Client 7.1.2
   Validation:  FluentValidation 12.0.0
   Logging:  OpenTelemetry 1.15.3, OpenTelemetry.Api 1.15.3, OpenTelemetry.Exporter.Jaeger, OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.3, OpenTelemetry.Extensions.Hosting 1.15.3, OpenTelemetry.Instrumentation.Http 1.15.1
   Cloud:  Azure.Identity 1.17.0, Azure.Storage.Blobs 12.27.0, Microsoft.Azure.SignalR 1.32.0
   Utilities:  Newtonsoft.Json 13.0.3
   Other:  Asp.Versioning.Abstractions [10.0.0,11.0.0), Asp.Versioning.Http [10.0.0,11.0.0), AWSSDK.S3 4.0.22.1, AWSSDK.SimpleNotificationService 4.0.2.14, AWSSDK.SQS 4.0.2.14, Confluent.SchemaRegistry.Serdes.Avro 2.14.0, Confluent.SchemaRegistry.Serdes.Json 2.14.0, DotPulsar 5.1.2 … (48 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus AncillaryWolverineOptionsMartenExtensions)
