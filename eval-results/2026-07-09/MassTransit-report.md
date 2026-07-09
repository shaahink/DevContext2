# REPORT
**MassTransit**

Style: Microservices
_32 projects  ·  31 MessageConsumer  ·  net8.0, net8.0;net9.0;net10.0 + azure-servicebus + dapper + hangfire + quartz + masstransit + efcore + redis + signalr_

## Stats

| Metric | Value |
|--------|-------|
| Files | 5502 |
| Projects | 59 |
| Nodes | 13031 |
| Edges | 11526 |
| ServiceLinks | 12 |
| Entries | 31 |
| With target | 31/31 |
| Verified edges | 50% |
| Analyzed in | 61.9s |

## Top Flows

1. **AllConsumer** → `AllConsumer` *(MessageConsumer)*
2. **BatchConsumer** → `BatchConsumer` *(MessageConsumer)*
3. **CancelScheduledMessageConsumer** → `CancelScheduledMessageConsumer` *(MessageConsumer)*
4. **ConnectionConsumer** → `ConnectionConsumer` *(MessageConsumer)*
5. **FinalizeJobConsumer** → `FinalizeJobConsumer` *(MessageConsumer)*
6. **GroupConsumer** → `GroupConsumer` *(MessageConsumer)*
7. **GroupManagementConsumer** → `GroupManagementConsumer` *(MessageConsumer)*
8. **MediatorRequestHandler** → `MediatorRequestHandler` *(MessageConsumer)*
9. **MessageHandlerConsumer** → `MessageHandlerConsumer` *(MessageConsumer)*
10. **PauseScheduledMessageConsumer** → `PauseScheduledMessageConsumer` *(MessageConsumer)*

### Trace 1: AllConsumer

TRACE  AllConsumer
       src/MassTransit.SignalR/Consumers/AllConsumer.cs:12
       MassTransit.SignalR
▸ ENTRY  AllConsumer  (src/MassTransit.SignalR/Consumers/AllConsumer.cs:12)
   └─ call AllConsumer  (src/MassTransit.SignalR/Consumers/AllConsumer.cs:12)
          public class AllConsumer<THub> :
          IConsumer<All<THub>>
          where THub : Hub

---

### Trace 2: BatchConsumer

TRACE  BatchConsumer
       src/MassTransit/Consumers/Batching/BatchConsumer.cs:14
       MassTransit
▸ ENTRY  BatchConsumer  (src/MassTransit/Consumers/Batching/BatchConsumer.cs:14)
   └─ call BatchConsumer  (src/MassTransit/Consumers/Batching/BatchConsumer.cs:14)
          public class BatchConsumer<TMessage> :
          IConsumer<TMessage>
          where TMessage : class

---

### Trace 3: CancelScheduledMessageConsumer

TRACE  CancelScheduledMessageConsumer
       src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/CancelScheduledMessageConsumer.cs:8
       MassTransit.QuartzIntegration
▸ ENTRY  CancelScheduledMessageConsumer  (src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/CancelScheduledMessageConsumer.cs:8)
   └─ call CancelScheduledMessageConsumer  (src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/CancelScheduledMessageConsumer.cs:8)
          public class CancelScheduledMessageConsumer :
          IConsumer<CancelScheduledMessage>,
          IConsumer<CancelScheduledRecurringMessage>
      └─ call ConsumeContext  (src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/CancelScheduledMessageConsumer.cs:21) [approx]
             public interface ConsumeContext<out T> :
             ConsumeContext
             where T : class
         └─ di ConsumeContext [approx]
                public interface ConsumeContext<out T> :
                ConsumeContext
                where T : class
            (stopped at depth 3; 1 branch omitted)

---

## Insights

_4 info · 5 notable_

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- ServiceBusTopicConfigurator
- AmazonSqsBrokerTopology
- IStateMachineSagaTestHarness
- PublishContextAsyncPipe
- DelayedMessageSchedulerSpecification

### **NOTABLE**: Internal hubs: 1 heavily-referenced internal types
*(Topology)*

- Message (1 refs)

### **NOTABLE**: Extension seats: AddMassTransit (42 impls) · AddMassTransitTestHarness (19 impls) · AddOptions (8 impls)
*(Wiring)*

- AddMassTransit (42 impls)
- AddMassTransitTestHarness (19 impls)
- AddOptions (8 impls)

### **NOTABLE**: Event flow: 190 published, 31 consumed, 181 orphan
*(Wiring)*

- 181 orphan events (published, no internal consumer)
- MassTransit.JobService.Messages.SubmitJobCommand ← SubmitJob
- MassTransit.SqlTransport.Tests.ClientUpdatedEvent ← Should_support_routing_key
- MassTransit.SqlTransport.Tests.ClientDeletedEvent ← Should_support_routing_key
- 12 cross-service event flows

### **NOTABLE**: Potential SPOFs: 2 services are sole providers for ≥2 consumers
*(Risk)*

- MassTransit: 7 downstream 3 upstream
- MassTransit.MongoDbIntegration: 2 downstream 0 upstream

### _INFO_: Entry targets resolved 31/31 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 8 feature areas
*(Shape)*

- Courier/Consumers (9 entries)
- Consumers (5 entries)
- MassTransit.QuartzIntegration (4 entries)
- JobService (4 entries)
- MassTransit.HangfireIntegration (3 entries)

### _INFO_: Public surface: 1733 interfaces, 6240 classes (8098 total public types)
*(Shape)*

- 1733 interfaces
- 6240 classes

### _INFO_: Most depended-upon: MassTransit (24 dependents) · MassTransit (22 dependents) · MassTransit.TestFramework (20 dependents)
*(Topology)*

- MassTransit (24 dependents)
- MassTransit (22 dependents)
- MassTransit.TestFramework (20 dependents)

LIBRARY  MassTransit     (4531 public types)

ENTRY API
   annotate  [ConfigureConsumeTopology]   (ConfigureConsumeTopologyAttribute.cs)
      Specify whether the message type should be used to configure the broker topology for the consumer.
   annotate  [EntityName]   (EntityNameAttribute.cs)
      Specify the EntityName used for this message contract if configured.
   annotate  [ExcludeFromConfigureEndpoints]   (ExcludeFromConfigureEndpointsAttribute.cs)
      When added to a consuming type (consumer, saga, activity, etc), prevents MassTransit from configuring endpoint for it...
   annotate  [ExcludeFromImplementedTypes]   (ExcludeFromImplementedTypesAttribute.cs)
      Typically added to base messages types, such as IMessage, IEvent, etc.
   annotate  [ExcludeFromTopology]   (ExcludeFromTopologyAttribute.cs)
      When added to a message type (class, record, or interface), prevents MassTransit from creating an exchange or topic o...
   annotate  [FaultEntityName]   (FaultEntityNameAttribute.cs)
      Specify the EntityName used for the Fault version of this message contract, overriding the configured if configured.
   annotate  [HashCleanup]   (HashCleanupAttribute.cs)
   annotate  [Indexed]   (IndexedAttribute.cs)
      Specifies a property that should be indexed by the in-memory saga repository
   annotate  [MaybeNullWhen]   (NullableAttributes.cs)
   annotate  [MemberNotNullWhen]   (NullableAttributes.cs)
   annotate  [MessageUrn]   (MessageUrnAttribute.cs)
      Specify the message type name for this message type
   annotate  [RecurringScheduleDateTimeInterval]   (RecurringScheduleDateTimeIntervalAttribute.cs)

ABSTRACTIONS
   IConsumer (interface)  — 328 implementors
   IFilter (interface)  — 173 implementors
   ITypeConverter (interface)  — 168 implementors
   CorrelatedBy (interface)  — 104 implementors
   MassTransitStateMachine (class)  — 80 implementors
   IPipe (interface)  — 78 implementors
   IStateMachineActivity (interface)  — 77 implementors
   ISpecification (interface)  — 74 implementors
   IPipeSpecification (interface)  — 73 implementors
   SagaStateMachineInstance (interface)  — 70 implementors

GENERATORS
   analyzer    AsyncMethodAnalyzer
   analyzer    CancellationTokenOverloadMethodAnalyzer
   analyzer    MessageContractAnalyzer
   code-fixer  CancellationTokenOverloadMethodFixer
   code-fixer  MessageContractCodeFixProvider

PUBLIC SURFACE
   Automatonymous
      Activity (interface)
      Behavior (interface)
         A behavior is a chain of activities invoked by a state
      EventObserver (interface)
      StateObserver (interface)
   MassTransit
      AbstractUriException (class)
      ActionSagaClassMap (class):  ActionSagaClassMap
      ActiveJob (class):  Equals, GetHashCode
         Active Jobs are allocated a concurrency slot, and are valid until the deadline is reached, after which they may be au...
      ActiveMqBusFactory (class):  Create, CreateMessageTopology
      ActiveMqBusFactoryConfiguratorExtensions (class):  CreateUsingActiveMq, UsingActiveMq
      ActiveMqConfigureEndpointCallbackExtensions (class):  AddActiveMqConfigureEndpointCallback, AddActiveMqConfigureEndpointsCallback
      ActiveMqConnectException (class):  ActiveMqConnectException
      ActiveMqConnectionException (class):  ActiveMqConnectionException
      ActiveMqDeferMessageExtensions (class):  Defer
      ActiveMqEndpointAddress (struct):  ActiveMqEndpointAddress, GetQueryStringOptions, ParseLeft
      ActiveMqHostAddress (struct):  ActiveMqHostAddress, ParseLeft
      ActiveMqHostConfigurationExtensions (class):  Host, ReceiveEndpoint
      ActiveMqHostSettings (interface):  CreateConnection
         Settings to configure a ActiveMQ host explicitly without requiring the fluent interface
      ActiveMqMessageContext (interface)
      ActiveMqPublishTopologyConfigurationExtensions (class):  AddPublishMessageTypes, AddPublishMessageTypesFromNamespaceContaining
      ActiveMqSendContext (interface)
      ActiveMqSendContextExtensions (class):  SetPriority, TrySetPriority
      ActiveMqTransportConfigurationException (class):  ActiveMqTransportConfigurationException
      ActiveMqTransportException (class):  ActiveMqTransportException
      ActiveMqTransportOptions (class):  ActiveMqTransportOptions
      ActivityCompensationException (class):  ActivityCompensationException
      ActivityConfigurationObserver (class):  ActivityConfigurationObserver, ActivityConfigured, CompensateActivityConfigured, Disconnect, Dispose, ExecuteActivityConfigured
      ActivityDefinition (class):  Configure, GetCompensateEndpointName
      ActivityExecutionException (class):  ActivityExecutionException
      ActivityExecutionFaultedException (class):  ActivityExecutionFaultedException
      ActivityObserverConfigurationExtensions (class):  ConnectActivityObserver
      AgentExtensions (class):  Stop
      AmazonS3ClientExtensions (class):  CreateMessageDataRepository
      AmazonS3MessageDataRepositorySelectorExtensions (class):  AmazonS3
      AmazonSqsBusFactory (class):  Create, CreateMessageTopology
      AmazonSqsBusFactoryConfiguratorExtensions (class):  CreateUsingAmazonSqs, LocalstackHost, UseDefaultHost, UsingAmazonSqs
      AmazonSqsConfigureEndpointCallbackExtensions (class):  AddAmazonSqsConfigureEndpointCallback, AddAmazonSqsConfigureEndpointsCallback
      AmazonSqsConnectException (class):  AmazonSqsConnectException
      AmazonSqsConnectionException (class):  AmazonSqsConnectionException
      AmazonSqsEndpointAddress (struct):  AmazonSqsEndpointAddress, GetQueryStringOptions, IsFifo, ParseLeft
      AmazonSqsHostAddress (struct):  AmazonSqsHostAddress, ParseLeft
      AmazonSqsHostConfigurationExtensions (class):  Host, ReceiveEndpoint
      AmazonSqsHostSettings (interface):  CreateConnection
         Settings to configure a AmazonSQS host explicitly without requiring the fluent interface
      AmazonSqsMessageContext (interface)
      AmazonSqsMessageSchedulerExtensions (class):  AddAmazonSqsMessageScheduler, UseAmazonSqsMessageScheduler
      AmazonSqsPublishTopologyConfigurationExtensions (class):  AddPublishMessageTypes, AddPublishMessageTypesFromNamespaceContaining
      AmazonSqsSendContext (interface)
      AmazonSqsSendContextExtensions (class):  SetDeduplicationId, SetDelay, SetGroupId, TrySetDeduplicationId, TrySetDelay, TrySetGroupId
      AmazonSqsTransportConfigurationException (class):  AmazonSqsTransportConfigurationException
      AmazonSqsTransportException (class):  AmazonSqsTransportException
      AmazonSqsTransportOptions (class)
      ArrayMessageBody (class):  ArrayMessageBody, GetBytes, GetStream, GetString
      AsyncBusHandle (class):  AsyncBusHandle, DisposeAsync
      AsyncDelegatePipe (class):  AsyncDelegatePipe, Probe, Send
      AsyncSerializerWrapper (class):  AsyncSerializerWrapper, SerializeAsync
      AuditConfigurationExtensions (class):  ConnectConsumeAuditObserver, ConnectSendAuditObservers
      AzureBusFactory (class):  CreateMessageTopology, CreateUsingServiceBus
      AzureCosmosEmulatorConstants (class)
      AzureFunctionsBusConfigurationExtensions (class):  AddMassTransitForAzureFunctions, ConfigureApplicationInsights, IsMissingCredentials
      AzureFunctionsEventHubConfigurationExtensions (class):  AddMassTransitEventHub
      AzureServiceBusTestHarnessOptions (class)
      AzureServiceBusTransportOptions (class)
      AzureStorageConfigurationExtensions (class):  CreateMessageDataRepository
      AzureTableAuditStoreConfiguratorExtensions (class):  ConfigureAuditStore, UseAzureTableAuditStore
      AzureTableJobServiceConfigurationExtensions (class):  UseAzureTableSagaRepository
      AzureTableRepositoryRegistrationExtensions (class):  AzureTableRepository, SetAzureTableSagaRepositoryProvider
      Base64MessageBody (class):  Base64MessageBody, GetBytes, GetStream, GetString
         Converts a binary-only message body to Base64 so that it can be used with non-binary message transports.
      Batch (interface)
         A batch of messages which are delivered to a consumer all at once
      BatchConsumerExtensions (class):  Batch, Consumer
      BatchOptions (class):  BatchOptions, Configure, DefaultConfigurationCallback, GroupBy, SetConcurrencyLimit, SetConfigurationCallback, SetMessageLimit, SetTimeLimit, SetTimeLimitStart, Validate
         Batch options are applied to a consumer to configure the size and time limits for each batch.
      BatchProduceExtensions (class):  PublishBatch, SendBatch
      BehaviorContext (interface):  CreateProxy, Init, Raise
         A behavior context is an event context delivered to a behavior, including the state instance
      BehaviorContextProxy (class):  BehaviorContextProxy, CreateProxy, Init, Raise, SetCompleted
      BehaviorExceptionContext (interface):  CreateProxy
         An exceptional behavior context
      BehaviorExceptionContextProxy (class):  BehaviorExceptionContextProxy, CreateProxy
      BindConfigurationExtensions (class):  UseBind
      BindContext (interface)
         The binding of a value to the context, which is a fancy form of Tuple
      BuilderStateMachine (class)
      Bus (class)
         used to get access to the bus factories
      BusControlExtensions (class):  DeployAsync, Start, StartAsync, Stop, StopAsync
      BusControlHealthExtensions (class):  WaitForHealthStatus
      BusDepotExtensions (class):  Start, Stop
      BusFactoryExtensions (class):  Build
      BusHandle (interface):  StopAsync
         Returned once a bus has been started.
      BusHandleExtensions (class):  Stop, StopAsync
      BusHealthResult (class):  BusHealthResult, Degraded, Healthy, Unhealthy
      BusInstance (class):  CheckHealth, ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectEndpointConfigurationObserver, ConnectPublishObserver, ConnectReceiveEndpoint, ConnectReceiveEndpointObserver, ConnectReceiveObserver, ConnectRequestPipe, ConnectSendObserver, GetPublishSendEndpoint, GetSendEndpoint, Probe, Publish
         When configuring multiple bus instances in a single container (MultiBus), this base class should be used as a the bas...
      BusReady (interface)
      BytesMessageBody (class):  BytesMessageBody, GetBytes, GetStream, GetString
      Cached (class):  Cached
      CachedConvention (interface):  TryGetEndpointAddress
      CachedType (interface):  CachedType, Method1, Method2
      Callback (class):  Callback, GetResult
      CancelScheduledSendExtensions (class):  CancelScheduledSend
      CircuitBreakerConfigurationExtensions (class):  UseCircuitBreaker
      ClientFactoryContext (interface):  GetRequestEndpoint
         The client factory context, which contains multiple interfaces and properties used by clients
      ClientFactoryExtensions (class):  ConnectClientFactory, CreateClientFactory, CreateRequestClient
      CommandException (class):  CommandException
      CommandExtensions (class):  SendCommand
      CompensateActivityContext (interface)
      CompensateContext (interface):  Compensated, CreateActivityContext, Failed
      CompensationResult (interface):  Evaluate, IsFailed
      CompletedActivityOptions (interface):  SetLog, SetVariable, SetVariables
      CompositeEventStatus (struct):  CompareTo, CompositeEventStatus, Equals, GetHashCode, IsSet, Set
      ConcurrencyException (class):  ConcurrencyException
      ConcurrencyLimitConfigurationExtensions (class):  UseConcurrencyLimit
      ConcurrencyLimitExtensions (class):  SetConcurrencyLimit
      ConcurrentMessageLimitExtensions (class):  UseConcurrentMessageLimit
      ConfigurationException (class):  ConfigurationException
      ConfigureConsumeTopologyAttribute (class):  ConfigureConsumeTopologyAttribute
         Specify whether the message type should be used to configure the broker topology for the consumer.
      ConfigureTestHarnessUsageTelemetryOptions (class):  Configure
      ConnectHandle (interface):  Disconnect
         A connect handle is returned by a non-asynchronous resource that supports disconnection (such as removing an observer...
      ConnectionException (class):  ConnectionException
      ConsumeContext (interface):  AddConsumeTask, HasMessageType, NotifyConsumed, NotifyFaulted, Respond, RespondAsync, TryGetMessage
      ConsumeContextActivatorExtensions (class):  CreateInstance, GetServiceOrCreateInstance
      ConsumeContextAsyncPipe (class):  ConsumeContextAsyncPipe, Probe, Send
      ConsumeContextEndpointExtensions (class):  GetEndpoint, GetFaultEndpoint, GetReceiveFaultEndpoint, GetResponseEndpoint
      ConsumeContextExecuteExtensions (class):  ToPipe
      ConsumeContextNotAvailableException (class):  ConsumeContextNotAvailableException
      ConsumeContextPipe (class):  ConsumeContextPipe, Probe, Send
      ConsumeContextSchedulerExtensions (class):  ScheduleSend
      ConsumeContextSelfSchedulerExtensions (class):  ScheduleSend
      ConsumeRetryContext (interface):  CreateNext, NotifyPendingFaults
      ConsumeTopology (class):  ApplyConventionsToMessageTopology, ConnectConsumeTopologyConfigurationObserver, CreateTemporaryQueueName, GetMessageTopology, MessageTopologyCreated, ShrinkToFit, TryAddConvention, Validate
      ConsumerCanceledException (class):  ConsumerCanceledException
      ConsumerConsumeContext (interface)
         A consumer and consume context mixed together, carrying both a consumer and the message consume context.
      ConsumerConvention (class):  Register, Remove
         Used to register conventions for consumer message types
      ConsumerDefinition (class):  Configure, GetEndpointName
         A consumer definition defines the configuration for a consumer, which can be used by the automatic registration code ...
      ConsumerException (class):  ConsumerException
      ConsumerExtensions (class):  ConnectConsumer, Consumer
      ConsumerFaultContext (interface)
      ConsumerMessageException (class):  ConsumerMessageException
      ConsumerPipeConfiguratorExtensions (class):  UseFilter
      ContainerActivityExtensions (class):  Activity
      ContextFilterConfigurationExtensions (class):  UseContextFilter
      ConventionException (class):  ConventionException
      CorrelatedBy (interface)
         Used to identify a message as correlated so that the CorrelationId can be returned
      CorrelatedEventRegistration (class):  CorrelatedEventRegistration, RegisterCorrelation
      CorrelatedFaultEventRegistration (class):  CorrelatedFaultEventRegistration, RegisterCorrelation
      CorrelationIdConventionExtensions (class):  UseCorrelationId
      CosmosConcurrencyException (class):  CosmosConcurrencyException
      CosmosRepositoryConfigurationExtensions (class):  AddCosmosClientFactory, AddNewtonsoftCosmosClientFactory, CosmosRepository, SetCosmosSagaRepositoryProvider
      CourierContext (interface)
      CourierException (class):  CourierException
      CourierHostConfiguratorExtensions (class):  CompensateActivityHost, ExecuteActivityHost
      CreateAgentPipe (class):  CreateAgentPipe, Probe, Send
      DapperSagaRepository (class):  Create
      DapperSagaRepositoryRegistrationExtensions (class):  DapperRepository, SetDapperSagaRepositoryProvider
      DataEventRegistration (class):  DataEventRegistration, Declare
      DeadLetterExtensions (class):  UseDeadLetter
      DefaultEndpointNameFormatter (class):  CompensateActivity, Consumer, DefaultEndpointNameFormatter, ExecuteActivity, GetTemporaryQueueName, Message, Saga, SanitizeName, TemporaryEndpoint
         The default endpoint name formatter, which simply trims the words Consumer, Activity, and Saga from the type name.
      DefaultInstanceStateAccessor (class):  CreateDefaultAccessor, DefaultInstanceStateAccessor, Get, GetStateExpression, Probe, Set
         The default state accessor will attempt to find and use a single State property on the instance type.
      DefaultStopContext (class):  DefaultStopContext
      DelayedMessageSchedulerConfigurationExtensions (class):  UseDelayedMessageScheduler
      DelayedMessageSchedulerRegistrationExtensions (class):  AddDelayedMessageScheduler
      DelayedRedeliveryExtensions (class):  Factory, UseDelayedRedelivery
      DelegateConfigurationExtensions (class):  UseExecute, UseExecuteAsync
      DelegatePipe (class):  DelegatePipe, Probe, Send
      DelegatePipeConfiguratorExtensions (class):  UseSendExecute, UseSendExecuteAsync
      DependencyInjectionExtensions (class):  AddGenericRequestClient, CreateRequestClient, RegisterInMemorySagaRepository, UseMessageScope, UseServiceScope
      DependencyInjectionFilterExtensions (class):  UseCompensateActivityFilter, UseConsumeFilter, UseExecuteActivityFilter, UsePublishFilter, UseSendFilter
      DependencyInjectionHostingExtensions (class):  UseMassTransit, UseMediator
      DependencyInjectionReceiveEndpointExtensions (class):  CompensateActivityHost, ConnectConsumer, Consumer, ExecuteActivityHost, Saga, StateMachineSaga
      DependencyInjectionRegistrationExtensions (class):  AddHostedService, AddInstrumentation, AddMassTransit, AddMediator, AddUsageTracker, RemoveHostedService, RemoveMassTransitHostedService, ReplaceScoped
         Standard registration extensions, which are used to configure consumers, sagas, and activities on receive endpoints f...
      DependencyInjectionTestingExtensions (class):  AddConsumerContainerTestHarness, AddConsumerTestHarness, AddMassTransitInMemoryTestHarness, AddMassTransitTestHarness, AddMassTransitTextWriterLogger, AddSagaContainerTestHarness, AddSagaStateMachineContainerTestHarness, AddSagaStateMachineTestHarness, AddSagaTestHarness, AddTelemetryListener, GetTestMethodInfo, RegisterConsumerTestHarnesses, RegisterSagaTestHarnesses, SetTestTimeouts
      DependencyInjectionTransactionExtensions (class):  AddTransactionalBus, AddTransactionalEnlistmentBus
      DispatchConfigurationExtensions (class):  UseDispatch
      DuplicateKeyPipeConfigurationException (class):  DuplicateKeyPipeConfigurationException
      DynamoDbSagaConcurrencyException (class):  DynamoDbSagaConcurrencyException
      DynamoDbSagaRepositoryOptions (class):  DynamoDbSagaRepositoryOptions, FormatSagaKey
      DynamoDbSagaRepositoryRegistrationExtensions (class):  DynamoDbRepository, SetDynamoDbSagaRepositoryProvider
      EmptyMessageBody (class):  GetBytes, GetStream, GetString
      EmptyServiceProvider (class):  GetService
      EncryptedSerializerConfigurationExtensions (class):  UseEncryptedSerializerV2WithFallback
      EndpointConfigurationObserver (class):  EndpointConfigurationObserver, EndpointConfigured
      EndpointConvention (class):  Map, TryGetDestinationAddress
      EndpointConventionCache (class):  CreateDefaultConvention, EndpointConventionCache, GetOrAdd, TryGetEndpointAddress
      EndpointConventionExtensions (class):  Send
      EndpointException (class):  EndpointException
      EndpointHealthResult (struct):  Degraded, EndpointHealthResult, Healthy, Unhealthy
      EndpointNotFoundException (class):  EndpointNotFoundException
      EntityFrameworkAuditStoreConfiguratorExtensions (class):  ConfigureAuditStore, UseEntityFrameworkCoreAuditStore
      EntityFrameworkCoreJobServiceConfigurationExtensions (class):  UseEntityFrameworkCoreSagaRepository
      EntityFrameworkCoreSagaRepositoryRegistrationExtensions (class):  CreateEntityFrameworkSagaRepository, EntityFrameworkRepository, SetEntityFrameworkSagaRepositoryProvider, UseMySql, UseOracle, UsePostgres, UseSqlServer, UseSqlite
      EntityFrameworkOutboxConfigurationExtensions (class):  AddEntityFrameworkOutbox, AddInboxStateEntity, AddOutboxMessageEntity, AddOutboxStateEntity, AddTransactionalOutboxEntities, ConfigureInboxStateEntity, ConfigureOutboxMessageEntity, ConfigureOutboxStateEntity, UseEntityFrameworkOutbox, UseMySql, UseOracle, UsePostgres, UseSqlServer, UseSqlite
      EntityFrameworkOutboxOptions (class)
      EntityFrameworkSagaRepositoryRegistrationExtensions (class):  EntityFrameworkRepository, SetEntityFrameworkSagaRepositoryProvider
      EntityNameAttribute (class):  EntityNameAttribute
         Specify the EntityName used for this message contract if configured.
      EqualityComparer (class):  Equals, GetHashCode
      Event (interface)
      EventActivities (interface):  GetStateActivityBinders
      EventActivityBinder (interface):  Add, Catch, If, IfAsync, IfElse, IfElseAsync, Retry
      EventCorrelation (interface)
      EventDataContext (interface)
      EventExecutionException (class):  EventExecutionException
      EventExtensions (class):  PublishEvent
      EventHubConnectionException (class):  EventHubConnectionException, IsExceptionTransient
      EventHubConsumeContext (interface)
      EventHubConsumeContextExtensions (class):  Offset, OffsetString, SequenceNumber
      EventHubEndpointAddress (struct):  EventHubEndpointAddress, ParseLeft
      EventHubEndpointConnectorExtensions (class):  ConnectEventHubEndpoint
      EventHubFactoryConfiguratorExtensions (class):  ReceiveEndpoint
      EventHubIntegrationExtensions (class):  GetCurrentProducerProvider, UsingEventHub
      EventHubProducerExtensions (class):  GetProducer
      EventHubSendContext (interface)
      EventHubSerializerConfigurationExtensions (class):  UseJsonSerializer, UseRawJsonSerializer
      EventHubTestHarnessExtensions (class):  GetProducer
      EventObservable (class):  ExecuteFault, Method4, Method5, Method6, PostExecute, PreExecute
      EventRegistration (interface):  RegisterCorrelation
      ExceptionActivityBinder (interface):  Add, Catch, If, IfAsync, IfElse, IfElseAsync
      ExceptionConsumeContext (interface)
      ExceptionConsumerConsumeContext (interface)
      ExceptionInfo (interface)
         An exception information that is serializable
      ExceptionInfoException (class):  ExceptionInfoException
      ExceptionReceiveContext (interface)
      ExceptionSagaConsumeContext (interface)
      ExcludeFromConfigureEndpointsAttribute (class)
         When added to a consuming type (consumer, saga, activity, etc), prevents MassTransit from configuring endpoint for it...
      ExcludeFromImplementedTypesAttribute (class)
         Typically added to base messages types, such as IMessage, IEvent, etc.
      ExcludeFromTopologyAttribute (class)
         When added to a message type (class, record, or interface), prevents MassTransit from creating an exchange or topic o...
      ExecuteActivityContext (interface)
      ExecuteActivityDefinition (class):  Configure, GetExecuteEndpointName
      ExecuteContext (interface):  Completed, CompletedWithVariables, CreateActivityContext, Faulted, FaultedWithVariables, ReviseItinerary, Terminate
      ExecutionResult (interface):  Evaluate, IsFaulted
      Factory (struct):  ActivateType
      Fault (interface)
         A faulted message, published when a message consumer fails to process the message
      FaultEntityNameAttribute (class):  FaultEntityNameAttribute
         Specify the EntityName used for the Fault version of this message contract, overriding the configured if configured.
      FaultPipe (class):  FaultPipe, Probe, Send
      FaultedActivityOptions (interface):  SetVariable, SetVariables
      FilterConfigurationExtensions (class):  UseFilter, UseFilters
      ForkConfigurationExtensions (class):  UseFork
      ForwardExtensions (class):  Forward
      Future (class):  CompletePending, CompletePendingRequest, CreateFutureRequest, CreateFutureRoutingSlip, CreateResponseEvent, DuringAnyWhen, FaultPendingRequest, FaultPendingRoutingSlip, FormatEventName, GetFault, GetResult, SetFaulted, SetResult
         A future is a deterministic, durable service that given a command, executes any number of requests, routing slips, fu...
      FutureDefinition (class):  Configure, GetEndpointName
         A future definition defines the configuration for a future, which can be used by the automatic registration code to c...
      FutureExtensions (class):  AddSubscription, InitializeFuture, SetFault, SetResult
      FutureLocation (struct):  FutureLocation
      FutureMessage (class):  FutureMessage, HasMessageType
      FutureNotFoundException (class):  FutureNotFoundException
      FutureRequestHandle (interface):  OnResponseReceived
      FutureResponseHandle (interface)
      FutureResultConfiguratorExtensions (class):  SetCompleted
      FutureRoutingSlipHandle (interface)
      FutureState (class):  HasFaults, HasPending, HasResults, HasSubscriptions, HasVariables
      FutureStateExtensions (class):  AddSubscription, CreateFutureMessage, GetCommand, SelectResults, SetCompleted, SetFault, SetFaulted, SetResult, ToObject, TryGetFault, TryGetResult
      FutureSubscription (class):  Equals, FutureSubscription, GetHashCode
      FutureVariableExtensions (class):  SetVariable, TryGetVariable
      GlobalTopology (class):  ConfigureJobSagaCorrelation, ConfigureRoutingSlipCorrelation, GlobalTopology, IsConsumableMessageType, MarkMessageTypeNotConsumable, SeparatePublishFromSend
         This represents the global topology configuration, which is delegated to by all topology instances, unless for some r...
      Handle (class):  Handle, ReadyOrNot, StopAsync
      HandlerExtensions (class):  ConnectHandler, ConnectRequestHandler, Handler
      HandlerRegistrationConfiguratorExtensions (class):  AddHandler
      HangfireEndpointOptions (class)
      HangfireIntegrationExtensions (class):  UseHangfireScheduler
      HangfireRegistrationExtensions (class):  AddHangfireConsumers, ConfigureHangfireConsumers
      HangfireSchedulerOptions (class)
      HeaderValue (struct):  HeaderValue, IsSimpleValue, IsStringValue
      Headers (interface):  Get, GetAll, TryGetHeader
         Headers are values outside of a message body transferred with the message.
      HealthReportExtensions (class):  ToJsonString
      Host (class)
      HostConnectReceiveEndpointExtensions (class):  ConnectReceiveEndpoint, ConnectResponseEndpoint
      HostInfo (interface)
         The host where an event or otherwise was produced a routing slip
      HostReady (interface)
      HostReceiveEndpointHandle (interface):  StopAsync
         Returned when a receive endpoint is connected
      HostedServiceConfigurationExtensions (class):  AddMassTransitHostedService
         These are the updated extensions compatible with the container registration code.
      IActiveMqBusFactoryConfigurator (interface):  EnableArtemisCompatibility, Host, Publish, Send, SetConsumerEndpointQueueNameFormatter, SetTemporaryQueueNameFormatter, SetTemporaryQueueNamePrefix
      IActiveMqBusTopology (interface):  GetDestinationAddress, GetSendSettings, Publish, Send
      IActiveMqConsumeTopology (interface):  Apply, Bind, GetMessageTopology
      IActiveMqConsumeTopologyConfigurator (interface):  AddSpecification, GetMessageTopology
      IActiveMqConsumeTopologySpecification (interface):  Apply
      IActiveMqHostConfigurator (interface):  EnableAsyncSend, EnableOptimizeAcknowledge, FailoverHosts, Password, SetPrefetchPolicy, SetQueuePrefetchPolicy, TransportOptions, UseSsl, Username
      IActiveMqMessageConsumeTopology (interface)
      IActiveMqMessageConsumeTopologyConfigurator (interface):  Apply, Bind
      IActiveMqMessagePublishTopology (interface):  Apply, GetBrokerTopology, GetSendSettings
      IActiveMqMessagePublishTopologyConfigurator (interface)
      IActiveMqMessageSendTopology (interface)
      IActiveMqMessageSendTopologyConfigurator (interface)
      IActiveMqPublishTopology (interface):  GetMessageTopology, GetPublishBrokerTopology
      IActiveMqPublishTopologyConfigurator (interface):  GetMessageTopology
      IActiveMqQueueBindingConfigurator (interface)
      IActiveMqQueueConfigurator (interface)
         Configures a queue/exchange pair in ActiveMQ
      IActiveMqQueueEndpointConfigurator (interface)
      IActiveMqReceiveEndpointConfigurator (interface):  Bind, ConfigureConnection, ConfigureSession
         Configure a receiving ActiveMQ endpoint
      IActiveMqSendTopology (interface):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      IActiveMqSendTopologyConfigurator (interface)
      IActiveMqTopicBindingConfigurator (interface)
         Used to configure the binding of an exchange (to either a queue or another exchange)
      IActiveMqTopicConfigurator (interface)
         Configures an exchange for ActiveMQ
      IActivity (interface)
         An Activity implements the execute and compensate methods for an activity
      IActivityConfigurationObserver (interface):  ActivityConfigured, CompensateActivityConfigured, ExecuteActivityConfigured
      IActivityConfigurationObserverConnector (interface):  ConnectActivityConfigurationObserver
      IActivityDefinition (interface):  Configure, GetCompensateEndpointName
      IActivityFactory (interface):  Compensate, Execute
      IActivityObserver (interface):  CompensateFail, ExecuteFault, PostCompensate, PostExecute, PreCompensate, PreExecute
      IActivityObserverConnector (interface):  ConnectActivityObserver
      IActivityRegistrationConfigurator (interface):  CompensateEndpoint, Endpoints, ExcludeFromConfigureEndpoints, ExecuteEndpoint
      IAgent (interface):  Stop
         An agent can be supervised, and signals when it has completed
      IAmazonSqsBusFactoryConfigurator (interface):  Host, OverrideDefaultBusEndpointQueueName, Publish, Send
      IAmazonSqsBusTopology (interface):  GetDestinationAddress, GetSendSettings, Publish, Send
      IAmazonSqsConsumeTopology (interface):  Apply, Bind, GetMessageTopology
      IAmazonSqsConsumeTopologyConfigurator (interface):  AddSpecification, GetMessageTopology
      IAmazonSqsConsumeTopologySpecification (interface):  Apply
      IAmazonSqsHostConfigurator (interface):  AccessKey, AllowTransportHeader, Config, Credentials, EnableScopedTopics, Scope, SecretKey
      IAmazonSqsMessageConsumeTopology (interface)
      IAmazonSqsMessageConsumeTopologyConfigurator (interface):  Apply, Subscribe
      IAmazonSqsMessagePublishTopology (interface):  Apply, GetBrokerTopology, GetPublishSettings
      IAmazonSqsMessagePublishTopologyConfigurator (interface)
      IAmazonSqsMessageSendTopology (interface)
      IAmazonSqsMessageSendTopologyConfigurator (interface)
      IAmazonSqsPublishTopology (interface):  GetMessageTopology, GetPublishBrokerTopology
      IAmazonSqsPublishTopologyConfigurator (interface):  GetMessageTopology
      IAmazonSqsQueueConfigurator (interface)
         Configures a queue/exchange pair in AmazonSQS
      IAmazonSqsQueueEndpointConfigurator (interface)
      IAmazonSqsQueueSubscriptionConfigurator (interface)
      IAmazonSqsReceiveEndpointConfigurator (interface):  ConfigureClient, ConfigureConnection, DisableMessageOrdering, Subscribe
         Configure a receiving AmazonSQS endpoint
      IAmazonSqsSendTopology (interface):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      IAmazonSqsSendTopologyConfigurator (interface)
      IAmazonSqsTopicConfigurator (interface):  GetEndpointAddress
         Configures an exchange for AmazonSQS
      IAmazonSqsTopicSubscriptionConfigurator (interface)
         Used to configure the binding of an exchange (to either a queue or another exchange)
      IAsyncBusHandle (interface)
      IAsyncPipeContextHandle (interface):  CreateCanceled, CreateFaulted, Created, Faulted
         Supports the asynchronous notification of a PipeContext becoming available (this is a future of a future, basically)
      IAzureTableSagaRepositoryConfigurator (interface):  ConnectionFactory, KeyFormatter
      IBatchConfigurator (interface):  Consumer
         Batching is an experimental feature, and may be changed at any time in the future.
      IBehavior (interface):  Execute, Faulted
         A behavior is a chain of activities invoked by a state
      IBindConfigurator (interface):  Source
      IBus (interface)
         A bus is a logical element that includes a local endpoint and zero or more receive endpoints
      IBusControl (interface):  CheckHealth, StartAsync, StopAsync
      IBusDepot (interface):  Start, Stop
      IBusFactory (interface):  CreateBusEndpointConfiguration
      IBusFactoryConfigurator (interface):  AddDeserializer, AddSerializer, ClearSerialization, Message, Publish, Send
      IBusFactorySelector (interface)
         Use one of the selector extension methods to create a bus instance for the selected transport.
      IBusObserver (interface):  CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted
         Used to observe events produced by the bus
      IBusObserverConnector (interface):  ConnectBusObserver
      IBusOutboxConfigurator (interface):  DisableDeliveryService
      IBusRegistrationConfigurator (interface):  AddBus, AddConfigureEndpointsCallback, AddRider, SetBusFactory, SetRequestClientFactory
         Configures the container registration, and supports creation of a bus or a mediator.
      IBusRegistrationContext (interface):  ConfigureEndpoints, GetConfigureReceiveEndpoints
      IBusTopology (interface):  Message, Publish, Send, TryGetPublishAddress
      ICircuitBreakerConfigurator (interface)
         Configure the settings on the circuit breaker
      IClientFactory (interface):  CreateRequest, CreateRequestClient
         The client factory is used to create request clients
      ICompensateActivity (interface):  Compensate
      ICompensateActivityConfigurator (interface):  ActivityLog, Log, RoutingSlip
         Configure the execution of the activity and arguments with some tasty middleware.
      ICompensateActivityFactory (interface):  Compensate
      ICompensateActivityLogConfigurator (interface)
         Configure the execution of the activity and arguments with some tasty middleware.
      ICompensateLogConfigurator (interface)
         Configure the execution of the activity and arguments with some tasty middleware.
      IConfigureReceiveEndpoint (interface):  Configure
         Implement this interface, and register the implementation in the container as the interface type to apply configurati...
      IConnectionMultiplexerFactory (interface):  GetConnectionMultiplexer
      IConsumeConfigurator (interface)
         The base configuration interface for a consumer, handler, or instance that can consume messages.
      IConsumeMessageObserver (interface):  ConsumeFault, PostConsume, PreConsume
         Intercepts the ConsumeContext
      IConsumeMessageObserverConnector (interface):  ConnectConsumeMessageObserver
         Supports connection of a message observer to the pipeline
      IConsumeObserver (interface):  ConsumeFault, PostConsume, PreConsume
         Intercepts the ConsumeContext
      IConsumeObserverConnector (interface):  ConnectConsumeObserver
         Supports connection of a consume observer
      IConsumePipeConfigurator (interface):  AddPipeSpecification, AddPrePipeSpecification
      IConsumePipeConnector (interface):  ConnectConsumePipe
      IConsumeTopology (interface):  CreateTemporaryQueueName, GetMessageTopology
      IConsumeTopologyConfigurator (interface):  GetMessageTopology, TryAddConvention
      IConsumer (interface):  Consume
         Defines a class that is a consumer of a message.
      IConsumerConfigurationObserver (interface):  ConsumerConfigured, ConsumerMessageConfigured
      IConsumerConfigurationObserverConnector (interface):  ConnectConsumerConfigurationObserver
      IConsumerConfigurator (interface):  ConsumerMessage, Message
      IConsumerDefinition (interface):  Configure, GetEndpointName
      IConsumerFactory (interface):  Send
         Maps an instance of a consumer to one or more Consume methods for the specified message type The whole purpose for th...
      IConsumerMessageConfigurator (interface):  Message
      IConsumerRegistrationConfigurator (interface):  Endpoint, ExcludeFromConfigureEndpoints
      ICosmosClientFactory (interface):  GetCosmosClient
         An interface of a factory for creating Cosmos DB clients
      ICosmosCollectionIdFormatter (interface):  Saga
      ICosmosSagaRepositoryConfigurator (interface):  ConfigureEmulator, ConfigureItemRequestOptions, ConfigureLinqSerializerOptions, ConfigureQueryRequestOptions, SetCollectionIdFormatter, UseClientFactory, UseNewtonsoftJson
      IDapperSagaRepositoryConfigurator (interface)
      IDbMessageConsumeTopologyConfigurator (interface):  Apply
      IDeadLetterQueueNameFormatter (interface):  FormatDeadLetterQueueName
      IDefinition (interface)
      IDispatchConfigurator (interface):  Pipe
      IDynamoDbSagaRepositoryConfigurator (interface):  ContextFactory
      IEndpointConfigurationObserver (interface):  EndpointConfigured
      IEndpointConfigurationObserverConnector (interface):  ConnectEndpointConfigurationObserver
      IEndpointConfigurator (interface)
      IEndpointConventionCache (interface):  TryGetEndpointAddress
      IEndpointDefinition (interface):  Configure, GetEndpointName
         Defines an endpoint in a transport-independent way
      IEndpointNameFormatter (interface):  CompensateActivity, Consumer, ExecuteActivity, Message, Saga, SanitizeName, TemporaryEndpoint
      IEndpointRegistrationConfigurator (interface):  AddConfigureEndpointCallback
      IEndpointSettings (interface):  ConfigureEndpoint
      IEntityFrameworkBusOutboxConfigurator (interface)
      IEntityFrameworkOutboxConfigurator (interface):  DisableInboxCleanupService, UseBusOutbox
      IEntityFrameworkSagaRepository (interface):  AddSagaClassMap, GetDbContext
      IEntityFrameworkSagaRepositoryConfigurator (interface):  AddDbContext, CustomizeQuery, DatabaseFactory, ExistingDbContext, SetOptimisticConcurrency
      IEntityNameFormatter (interface):  FormatEntityName
         Used to build entity names for the publish topology
      IEntityNameValidator (interface):  IsValidEntityName, ThrowIfInvalidEntityName
      IErrorQueueNameFormatter (interface):  FormatErrorQueueName
      IEventCorrelationConfigurator (interface):  CorrelateBy, CorrelateById, OnMissingInstance, SelectId, SetSagaFactory
      IEventHubEndpointConnector (interface):  ConnectEventHubEndpoint
      IEventHubFactoryConfigurator (interface):  AddSerializer, ConfigureProducerOptions, Host, ReceiveEndpoint, Storage
      IEventHubProducer (interface):  Produce
      IEventHubProducerConfigurator (interface):  AddSerializer
      IEventHubProducerProvider (interface):  GetProducer
      IEventHubReceiveEndpointConfigurator (interface):  OnPartitionClosing, OnPartitionInitializing
      IEventHubReceiveEndpointContext (interface)
      IEventHubRider (interface):  GetProducerProvider
      IEventObserver (interface):  ExecuteFault, PostExecute, PreExecute
      IEventReceiver (interface):  Handle, HandleConsumer, HandleExecuteActivity, HandleSaga
         Called by an Azure Function to handle messages using configured consumers, sagas, or activities
      IExceptionConfigurator (interface):  Handle, Ignore
      IExceptionFilter (interface):  Match
         Filter exceptions for policies that act based on an exception
      IExecuteActivity (interface):  Execute
      IExecuteActivityArgumentsConfigurator (interface)
         Configure the execution of the activity and arguments with some tasty middleware.
      IExecuteActivityConfigurator (interface):  ActivityArguments, Arguments, RoutingSlip
         Configure the execution of the activity and arguments with some tasty middleware.
      IExecuteActivityDefinition (interface):  Configure, GetExecuteEndpointName
      IExecuteActivityFactory (interface):  Execute
         A factory that creates an execute activity and then invokes the pipe for the activity context
      IExecuteActivityRegistrationConfigurator (interface):  Endpoint, ExcludeFromConfigureEndpoints
      IExecuteArgumentsConfigurator (interface)
         Configure the execution of the activity and arguments with some tasty middleware.
      IFilter (interface):  Send
         A filter is a functional node in a pipeline, connected by pipes to other filters.
      IFilterObserver (interface):  PostSend, PreSend, SendFault
      IFilterObserverConnector (interface):  ConnectObserver
      IFutureDefinition (interface):  Configure, GetEndpointName
      IFutureFaultConfigurator (interface):  SetFaultedUsingFactory, SetFaultedUsingInitializer
      IFutureRegistrationConfigurator (interface):  Endpoint, ExcludeFromConfigureEndpoints, Repository
      IFutureRequestConfigurator (interface):  OnRequestFaulted, SetRequestAddressProvider, TrackPendingRequest, UsingRequestFactory, UsingRequestInitializer, WhenFaulted
      IFutureRequestDefinition (interface)
      IFutureResponseConfigurator (interface):  CompletePendingRequest, WhenReceived
      IFutureResultConfigurator (interface):  SetCompletedUsingFactory, SetCompletedUsingInitializer
      IFutureRoutingSlipConfigurator (interface):  BuildItinerary, BuildUsingItineraryPlanner, OnRoutingSlipCompleted, OnRoutingSlipFaulted, TrackPendingRoutingSlip, WhenRoutingSlipCompleted, WhenRoutingSlipFaulted
      IGlobalTopology (interface):  IsConsumableMessageType, MarkMessageTypeNotConsumable, SeparatePublishFromSend
      IGroupKeyProvider (interface):  TryGetKey
      IHandlerConfigurationObserver (interface):  HandlerConfigured
      IHandlerConfigurationObserverConnector (interface):  ConnectHandlerConfigurationObserver
      IHandlerConfigurator (interface)
         Configure a message handler, including specifying filters that are executed around the handler itself
      IHealthCheckOptionsConfigurator (interface)
      IInMemoryBusFactoryConfigurator (interface):  Host, Publish
      IInMemoryBusTopology (interface):  Publish
      IInMemoryConsumeTopology (interface):  Apply, GetMessageTopology
      IInMemoryConsumeTopologyConfigurator (interface):  AddSpecification, Bind, GetMessageTopology
      IInMemoryHostConfigurator (interface)
      IInMemoryMessageConsumeTopology (interface)
      IInMemoryMessageConsumeTopologyConfigurator (interface):  Apply, Bind
      IInMemoryMessagePublishTopology (interface):  Apply
      IInMemoryMessagePublishTopologyConfigurator (interface)
      IInMemoryPublishTopology (interface):  GetMessageTopology
      IInMemoryPublishTopologyConfigurator (interface):  GetMessageTopology
      IInMemoryReceiveEndpointConfigurator (interface):  Bind
      IInstanceConfigurator (interface)
      IItineraryBuilder (interface):  AddActivitiesFromSourceItinerary, AddActivity, AddSubscription, AddVariable, SetVariables
      IItineraryPlanner (interface):  PlanItinerary
         Implement to build a routing slip.
      IJobConsumer (interface):  Run
         Defines a message consumer which runs a job asynchronously, without waiting, which is monitored by Conductor services...
      IJobDistributionStrategy (interface):  IsJobSlotAvailable
         Used by the to determine if a job slot should be allocated to a job
      IJobSagaOptionsConfigurator (interface)
      IJobSagaRegistrationConfigurator (interface):  Endpoints, JobAttemptEndpoint, JobEndpoint, JobTypeEndpoint, UseRepositoryRegistrationProvider
      IJobServiceConfigurator (interface)
      IJobServiceRegistrationConfigurator (interface):  Endpoint, Options
      IKafkaApiConfigurator (interface)
      IKafkaFactoryConfigurator (interface):  ConfigureApi, ConfigureSocket, Host, OAuthBearerTokenRefreshHandler, SetHeadersDeserializer, SetHeadersSerializer, SetSerializationFactory, TopicEndpoint
      IKafkaFetchConfigurator (interface)
      IKafkaHostConfiguration (interface):  Build, CreateSendTransportContext, CreateSpecification
      IKafkaHostConfigurator (interface):  CancellationDelay, UseSasl, UseSsl
      IKafkaProducerConfigurator (interface):  SetHeadersSerializer, SetKeySerializer, SetStatisticsHandler, SetValueSerializer
      IKafkaRider (interface)
      IKafkaSaslConfigurator (interface)
      IKafkaSocketConfigurator (interface)
      IKafkaSslConfigurator (interface)
      IKafkaTopicEndpointConnector (interface):  ConnectTopicEndpoint
      IKafkaTopicReceiveEndpointConfigurator (interface):  ConfigureFetch, CreateIfMissing, SetHeadersDeserializer, SetKeyDeserializer, SetOffsetsCommittedHandler, SetStatisticsHandler, SetValueDeserializer, UseIsolationLevel
      ILatestConfigurator (interface)
      ILoadSagaRepository (interface):  Load
      IMartenSagaRepositoryConfigurator (interface):  Connection
      IMediatorConfigurator (interface)
      IMediatorRegistrationConfigurator (interface):  ConfigureMediator
      IMediatorRegistrationContext (interface)
      IMessageConfigurationObserver (interface):  MessageConfigured
      IMessageConsumeTopology (interface):  Apply
         The message-specific Consume topology, which may be configured or otherwise setup for use with the Consume specificat...
      IMessageConsumeTopologyConfigurator (interface):  Add, AddDelegate, AddOrUpdateConvention, TryAddConvention, UpdateConvention
         Configures the Consuming of a message type, allowing filters to be applied on Consume.
      IMessageCorrelationId (interface):  TryGetCorrelationId
      IMessageData (interface)
      IMessageDataRepository (interface):  Get, Put
         Storage of large message data that can be stored and retrieved separate of the message body.
      IMessageDeserializer (interface):  Deserialize, GetMessageBody
      IMessageEntityNameFormatter (interface):  FormatEntityName
         Used to build entity names for the publish topology
      IMessageFilterConfigurator (interface):  Exclude, Include
         Configures a message filter, for including and excluding message types
      IMessagePublishTopology (interface):  Apply, TryGetPublishAddress
         The message-specific publish topology, which may be configured or otherwise setup for use with the publish specificat...
      IMessagePublishTopologyConfigurator (interface):  Add, AddDelegate, AddOrUpdateConvention, TryAddConvention
         Configures the Publishing of a message type, allowing filters to be applied on Publish.
      IMessageReceiver (interface):  Handle, HandleConsumer, HandleExecuteActivity, HandleSaga
         Called by an Azure Function to handle messages using configured consumers, sagas, or activities
      IMessageScheduler (interface):  CancelScheduledPublish, CancelScheduledSend, SchedulePublish, ScheduleSend
         A message scheduler is able to schedule a message for delivery.
      IMessageSendTopology (interface):  Apply
         The message-specific send topology, which may be configured or otherwise setup for use with the send specification.
      IMessageSendTopologyConfigurator (interface):  Add, AddDelegate, AddOrUpdateConvention, TryAddConvention, TryGetConvention, UpdateConvention
         Configures the sending of a message type, allowing filters to be applied on send.
      IMessageSerializer (interface):  GetMessageBody
         A message serializer is responsible for serializing a message.
      IMessageTopology (interface):  GetMessageTopology
      IMessageTypeFilterConfigurator (interface):  Exclude, Include
         Configures a message filter, for including and excluding message types
      IMessageTypeSelector (interface):  GetMessageTopology
      IMissingInstanceConfigurator (interface):  Discard, Execute, ExecuteAsync, Fault
      IMissingInstanceRedeliveryConfigurator (interface):  OnRedeliveryLimitReached
      IMongoDbBusOutboxConfigurator (interface)
      IMongoDbOutboxConfigurator (interface):  ClientFactory, CollectionNameFormatter, DatabaseFactory, DisableInboxCleanupService, UseBusOutbox
      IMongoDbSagaRepositoryConfigurator (interface):  ClassMap, ClientFactory, CollectionNameFormatter, DatabaseFactory
      INewIdFormatter (interface):  Format
      INewIdGenerator (interface):  Next, NextGuid, NextSequentialGuid
      INewIdParser (interface):  Parse
      INotifyJobContext (interface):  NotifyCanceled, NotifyCompleted, NotifyFaulted, NotifyJobProgress, NotifyStarted
      IObjectDeserializer (interface):  DeserializeObject, SerializeObject
      IObserverConfigurator (interface)
         Configure a message handler, including specifying filters that are executed around the handler itself
      IOutboxConfigurator (interface)
      IOutboxOptionsConfigurator (interface)
      IPartitioner (interface):  GetPartitioner, Send
      IPipe (interface):  Send
      IPipeConfigurator (interface):  AddPipeSpecification
         Configures a pipe with specifications
      IPipeConnectorSpecification (interface):  Connect
      IPipeContextSource (interface):  Send
         A source provides the context which is sent to the specified pipe.
      IPostgresSqlHostConfigurator (interface)
      IProbeSite (interface):  Probe
         To support the introspection of code, this interface is used to gain information about the bus.
      IProcessIdProvider (interface):  GetProcessId
      IPropertyCollection (interface):  Get, TryGet
      IPublishEndpoint (interface):  Publish
         A publish endpoint lets the underlying transport determine the actual endpoint to which the message is sent.
      IPublishEndpointProvider (interface):  GetPublishSendEndpoint
      IPublishObserver (interface):  PostPublish, PrePublish, PublishFault
         Observes messages as they are published via a publish endpoint.
      IPublishObserverConnector (interface):  ConnectPublishObserver
         Connect an observer that is notified when a message is sent to an endpoint
      IPublishPipeConfigurator (interface):  AddPipeSpecification
      IPublishPipelineConfigurator (interface):  ConfigurePublish
      IPublishTopology (interface):  GetMessageTopology, TryGetPublishAddress
      IPublishTopologyConfigurator (interface):  AddMessagePublishTopology, GetMessageTopology, TryAddConvention
      IQuerySagaRepository (interface):  Find
      IRabbitMqBatchPublishConfigurator (interface)
      IRabbitMqBusFactoryConfigurator (interface):  Host, OverrideDefaultBusEndpointQueueName, Publish, Send
      IRabbitMqBusTopology (interface):  GetDestinationAddress, Publish, Send
      IRabbitMqClusterConfigurator (interface):  Node
      IRabbitMqConsumeTopology (interface):  Apply, GetMessageTopology
      IRabbitMqConsumeTopologyConfigurator (interface):  AddSpecification, Bind, BindQueue, GetMessageTopology
      IRabbitMqExchangeBindingConfigurator (interface):  SetBindingArgument
         Used to configure the binding of an exchange (to either a queue or another exchange)
      IRabbitMqExchangeConfigurator (interface):  SetExchangeArgument
         Configures an exchange for RabbitMQ
      IRabbitMqExchangeToExchangeBindingConfigurator (interface):  Bind
         Used to bind additional exchanges to an already bound exchange
      IRabbitMqHostConfigurator (interface):  ConnectionName, ContinuationTimeout, Heartbeat, MaxMessageSize, Password, RequestedChannelMax, RequestedConnectionTimeout, RequestedFrameMax, UseCluster, UseSsl, Username
      IRabbitMqMessageConsumeTopology (interface)
      IRabbitMqMessageConsumeTopologyConfigurator (interface):  Apply, Bind
      IRabbitMqMessagePublishTopology (interface):  Apply, GetBrokerTopology, GetSendSettings
      IRabbitMqMessagePublishTopologyConfigurator (interface):  BindAlternateExchangeQueue, BindQueue
      IRabbitMqMessageSendTopology (interface)
      IRabbitMqMessageSendTopologyConfigurator (interface)
      IRabbitMqPublishTopology (interface):  GetMessageTopology, GetPublishBrokerTopology
      IRabbitMqPublishTopologyConfigurator (interface):  GetMessageTopology
      IRabbitMqQueueBindingConfigurator (interface)
      IRabbitMqQueueConfigurator (interface):  EnablePriority, SetQueueArgument, SetQuorumQueue
         Configures a queue/exchange pair in RabbitMQ
      IRabbitMqQueueEndpointConfigurator (interface)
      IRabbitMqReceiveEndpointConfigurator (interface):  Bind, BindDeadLetterQueue, ConfigureChannel, ConfigureConnection, OverrideConsumerTag, SetDeliveryAcknowledgementTimeout, Stream
         Configure a receiving RabbitMQ endpoint
      IRabbitMqSendTopology (interface):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      IRabbitMqSendTopologyConfigurator (interface)
      IRabbitMqSslConfigurator (interface):  AllowPolicyErrors, EnforcePolicyErrors
         Configures SSL/TLS for RabbitMQ.
      IRabbitMqStreamConfigurator (interface):  FromFirst, FromLast, FromOffset, FromTimestamp
      IReceiveConfigurator (interface):  ReceiveEndpoint
      IReceiveConnector (interface):  ConnectReceiveEndpoint
      IReceiveEndpoint (interface):  Start, Stop
         A service endpoint has an inbound transport that pushes messages to consumers
      IReceiveEndpointConfigurator (interface):  AddDeserializer, AddEndpointSpecification, AddSerializer, ClearSerialization, ConfigureMessageTopology
         Configure a receiving endpoint
      IReceiveEndpointConnector (interface):  ConnectReceiveEndpoint
      IReceiveEndpointDependencyConnector (interface):  AddDependency
      IReceiveEndpointDependentConnector (interface):  AddDependent
      IReceiveEndpointObserver (interface):  Completed, Faulted, Ready, Stopping
         Used to observe the events signaled by a receive endpoint
      IReceiveEndpointObserverConnector (interface):  ConnectReceiveEndpointObserver
      IReceiveEndpointSpecification (interface):  Configure
         Specification for configuring a receive endpoint
      IReceiveObserver (interface):  ConsumeFault, PostConsume, PostReceive, PreReceive, ReceiveFault
         An observer that can monitor a receive endpoint to track message consumption at the endpoint level.
      IReceiveObserverConnector (interface):  ConnectReceiveObserver
      IReceivePipeConfigurator (interface)
      IReceivePipelineConfigurator (interface):  ConfigureDeadLetter, ConfigureError, ConfigureReceive, ConfigureTransport
      IReceiveTransportObserver (interface):  Completed, Faulted, Ready
         Used to observe the events signaled by a receive endpoint
      IReceiveTransportObserverConnector (interface):  ConnectReceiveTransportObserver
      IRecurringJobScheduleConfigurator (interface)
         Configure the optional settings of a recurring job
      IRecurringMessageScheduler (interface):  CancelScheduledRecurringSend, PauseScheduledRecurringSend, ResumeScheduledRecurringSend, ScheduleRecurringPublish, ScheduleRecurringSend
         A message scheduler is able to schedule a message for delivery.
      IRedeliveryConfigurator (interface)
      IRedisSagaRepositoryConfigurator (interface):  ConnectionFactory, DatabaseConfiguration, SelectDatabase
      IRegisterActivity (interface):  Register
      IRegisterConsumer (interface):  Register
      IRegisterExecuteActivity (interface):  Register
      IRegisterFuture (interface):  Register
      IRegisterSaga (interface):  Register
      IRegisterTestHarness (interface):  RegisterTestHarness
      IRegistrationBusFactory (interface):  CreateBus
      IRegistrationConfigurator (interface):  AddActivity, AddConsumer, AddEndpoint, AddExecuteActivity, AddFuture, AddRequestClient, AddSaga, AddSagaRepository, AddSagaStateMachine, SetDefaultRequestTimeout, SetEndpointNameFormatter, SetSagaRepositoryProvider
      IRegistrationContext (interface):  ConfigureActivity, ConfigureActivityCompensate, ConfigureActivityExecute, ConfigureConsumer, ConfigureConsumers, ConfigureExecuteActivity, ConfigureFuture, ConfigureSaga, ConfigureSagas
         Registration contains the consumers and sagas that have been registered, allowing them to be configured on one or mor...
      IRequestClient (interface):  Create, GetResponse
         A request client, which is used to send a request, as well as get one or more response types from that request.
      IRequestConfigurator (interface)
      IRequestPipeConfigurator (interface)
      IRequestPipeConnector (interface):  ConnectRequestPipe
         Connect a request pipe to the pipeline
      IRequestSendEndpoint (interface):  Send
      IRequestStateMachineMissingInstanceConfigurator (interface):  Apply
      IRescueConfigurator (interface)
      IRetryConfigurator (interface):  SetRetryPolicy
      IRetryObserver (interface):  PostCreate, PostFault, PreRetry, RetryComplete, RetryFault
      IRetryObserverConnector (interface):  ConnectRetryObserver
      IRetryPolicy (interface):  CreatePolicyContext, IsHandled
         A retry policy determines how exceptions are handled, and whether or not the remaining filters should be retried
      IRiderFactoryConfigurator (interface)
      IRiderRegistrationConfigurator (interface):  SetRiderFactory, TryAddScoped
      IRiderRegistrationContext (interface):  GetRegistrations
      IRoutingSlipBuilder (interface):  Build
      IRoutingSlipConfigurator (interface)
         Configure a message handler, including specifying filters that are executed around the handler itself
      IRoutingSlipExecutor (interface):  Execute
      ISaga (interface)
         Interface that specifies a class is usable as a saga instance, including the ability to get and set the CorrelationId...
      ISagaConfigurationObserver (interface):  SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
      ISagaConfigurationObserverConnector (interface):  ConnectSagaConfigurationObserver
      ISagaConfigurator (interface):  Message, SagaMessage
      ISagaDefinition (interface):  Configure, GetEndpointName
      ISagaFactory (interface):  Create, Send
         Creates a saga instance when an existing saga instance is missing
      ISagaMessageConfigurator (interface):  Message
      ISagaPolicy (interface):  Existing, Missing, PreInsertInstance
      ISagaQuery (interface):  GetFilter
         A saga query is used when a LINQ expression is accepted to query the saga repository storage to get zero or more saga...
      ISagaQueryFactory (interface):  TryCreateQuery
         Used to create a saga query from the message consume context
      ISagaRegistrationConfigurator (interface):  Endpoint, ExcludeFromConfigureEndpoints, Repository
      ISagaRepository (interface):  Send, SendQuery
         A saga repository is used by the service bus to dispatch messages to sagas
      ISagaRepositoryRegistrationConfigurator (interface)
      ISagaVersion (interface)
         For saga repositories that use an incrementing version
      IScheduleConfigurator (interface)
      IScheduleMessageProvider (interface):  CancelScheduledSend, ScheduleSend
      IScopedClientFactory (interface):  CreateRequest, CreateRequestClient
         A scoped client factory
      ISendEndpoint (interface):  Send
      ISendEndpointProvider (interface):  GetSendEndpoint
         The Send Endpoint Provider is used to retrieve endpoints using addresses.
      ISendObserver (interface):  PostSend, PreSend, SendFault
         Observes messages as they are sent to transports.
      ISendObserverConnector (interface):  ConnectSendObserver
         Connect an observer that is notified when a message is sent to an endpoint
      ISendPipeConfigurator (interface):  AddPipeSpecification
      ISendPipelineConfigurator (interface):  ConfigureSend
      ISendTopology (interface):  GetMessageTopology
      ISendTopologyConfigurator (interface):  AddMessageSendTopology, TryAddConvention
      ISerialization (interface):  GetMessageDeserializer, GetMessageSerializer, TryGetMessageDeserializer, TryGetMessageSerializer
      ISerializerFactory (interface):  CreateDeserializer, CreateSerializer
      IServiceBusBusFactoryConfigurator (interface):  Host, OverrideDefaultBusEndpointQueueName, Publish, Send, SetNamespaceSeparatorTo, SetNamespaceSeparatorToTilde, SetNamespaceSeparatorToUnderscore, SubscriptionEndpoint
      IServiceBusBusTopology (interface):  Publish, Send
      IServiceBusConsumeTopology (interface):  Apply, GetMessageTopology
      IServiceBusConsumeTopologyConfigurator (interface):  AddSpecification, GetMessageTopology, Subscribe
      IServiceBusEndpointConfigurator (interface)
      IServiceBusEndpointEntityConfigurator (interface)
      IServiceBusEntityConfigurator (interface)
      IServiceBusHostConfigurator (interface)
      IServiceBusMessageConsumeTopology (interface)
      IServiceBusMessageConsumeTopologyConfigurator (interface):  Apply, Subscribe
      IServiceBusMessageEntityConfigurator (interface):  EnableDuplicateDetection
      IServiceBusMessagePublishTopology (interface):  Apply, GetSendSettings, GetSubscriptionConfigurator
      IServiceBusMessagePublishTopologyConfigurator (interface)
      IServiceBusMessageSendTopology (interface)
      IServiceBusMessageSendTopologyConfigurator (interface)
      IServiceBusNamedKeyTokenProviderConfigurator (interface)
      IServiceBusPublishTopology (interface):  FormatSubscriptionName, GenerateSubscriptionName, GetMessageTopology, GetPublishBrokerTopology
      IServiceBusPublishTopologyConfigurator (interface):  GetMessageTopology
      IServiceBusQueueConfigurator (interface):  GetCreateQueueOptions
      IServiceBusQueueEndpointConfigurator (interface):  EnableDuplicateDetection
      IServiceBusReceiveEndpointConfigurator (interface):  Subscribe
         Configure an Azure Service Bus receive endpoint
      IServiceBusSendTopology (interface):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      IServiceBusSendTopologyConfigurator (interface):  GetMessageTopology
      IServiceBusSubscriptionConfigurator (interface):  GetCreateSubscriptionOptions
      IServiceBusSubscriptionEndpointConfigurator (interface)
         Configure an Azure Service Bus receive endpoint
      IServiceBusTokenProviderConfigurator (interface)
      IServiceBusTopicConfigurator (interface)
      IServiceInstanceConfigurator (interface):  AddSpecification
      ISetPropertyCollection (interface):  Set, SetMany
      ISetScopedConsumeContext (interface):  PushContext
      ISharedAccessSignatureTokenProviderConfigurator (interface)
      ISpecification (interface):  Validate
         A specification, that can be validated as part of a configurator, is used to allow nesting and chaining of specificat...
      ISqlBusFactoryConfigurator (interface):  Host, OverrideDefaultBusEndpointQueueName, Publish, Send
      ISqlBusTopology (interface):  Publish, Send
      ISqlConsumeTopology (interface):  Apply, GetMessageTopology
      ISqlConsumeTopologyConfigurator (interface):  AddSpecification, GetMessageTopology, Subscribe
      ISqlHostConfigurator (interface):  UseLicense, UseLicenseFile
      ISqlMessageConsumeTopology (interface)
      ISqlMessageConsumeTopologyConfigurator (interface):  Subscribe
      ISqlMessagePublishTopology (interface):  Apply, GetBrokerTopology, GetSendSettings
      ISqlMessagePublishTopologyConfigurator (interface)
      ISqlMessageSendTopology (interface)
      ISqlMessageSendTopologyConfigurator (interface)
      ISqlPublishTopology (interface):  GetMessageTopology, GetPublishBrokerTopology
      ISqlPublishTopologyConfigurator (interface):  GetMessageTopology
      ISqlQueueConfigurator (interface)
         Configure a database transport queue
      ISqlQueueEndpointConfigurator (interface)
      ISqlReceiveEndpointConfigurator (interface):  ConfigureClient, SetReceiveMode, Subscribe
         Configure a database transport receive endpoint
      ISqlSendTopology (interface):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      ISqlSendTopologyConfigurator (interface)
      ISqlServerSqlHostConfigurator (interface)
      ISqlTopicConfigurator (interface)
         Configures a topic for the database transport
      ISqlTopicSubscriptionConfigurator (interface)
         Configures the topic subscription for the receive endpoint
      ISqlTopicToTopicBindingConfigurator (interface):  Subscribe
      IStateAccessor (interface):  Get, GetStateExpression, Set
      IStateMachineActivity (interface):  Execute, Faulted
      IStateMachineActivitySelector (interface):  OfInstanceType, OfType
      IStateMachineEventActivitiesBuilder (interface):  CommitActivities, Ignore, When
      IStateMachineFaultedActivitySelector (interface):  OfInstanceType, OfType
      IStateMachineModifier (interface):  AfterLeave, AfterLeaveAny, Apply, BeforeEnter, BeforeEnterAny, CompositeEvent, During, DuringAny, Event, Finally, Initially, InstanceState, Name, OnUnhandledEvent, State
      IStateObserver (interface):  StateChanged
      ISubscriptionEndpointConnector (interface):  ConnectSubscriptionEndpoint
      ISupervisor (interface):  Add
         A supervisor with a set of agents (a supervisor is also an agent)
      ITickProvider (interface)
      ITimeoutConfigurator (interface)
      ITopicProducer (interface):  Produce
         Kafka messages are a combination of headers, a key, and a value.
      ITopicProducerProvider (interface):  GetProducer
      ITransactionConfigurator (interface)
      ITransactionalOutboxConfigurator (interface)
      ITransformConfigurator (interface):  Default, Set, Transform
      ITransformSpecificationConfigurator (interface):  Get
      ITransportConfigurator (interface)
      ITransportSequenceNumber (interface)
      IVisitable (interface):  Accept
         Used to visit the state machine structure, so it can be displayed, etc.
      IWorkerIdProvider (interface):  GetWorkerId
      InMemoryBus (class):  Create, CreateMessageTopology
      InMemoryConfigurationExtensions (class):  AddDelayProvider, CreateUsingInMemory, UsingInMemory
      InMemoryEndpointAddress (struct):  GetQueryStringOptions, InMemoryEndpointAddress, ParseLeft
      InMemoryHostAddress (struct):  InMemoryHostAddress, ParseLeft
      InMemoryOutboxConfigurationExtensions (class):  AddInMemoryInboxOutbox, UseInMemoryInboxOutbox, UseInMemoryOutbox
      InMemoryPublishTopologyConfigurationExtensions (class):  AddPublishMessageTypes, AddPublishMessageTypesFromNamespaceContaining
      InMemorySagaRepository (class):  Find, InMemorySagaRepository, Load, Probe, Send, SendQuery
      InMemorySagaRepositoryRegistrationExtensions (class):  InMemoryRepository, SetInMemorySagaRepositoryProvider
      InVar (class)
         Variables, which can be used for message initialization
      InboxCleanupServiceOptions (class)
      IndexedAttribute (class)
         Specifies a property that should be indexed by the in-memory saga repository
      InitialIfNullStateAccessor (class):  Get, GetStateExpression, InitialIfNullStateAccessor, Probe, Set
      InitiatedBy (interface)
         Specifies that the message type TMessage starts a new saga.
      InitiatedByOrOrchestrates (interface)
         Specifies that a class implementing ISaga consumes TMessage as part of the saga
      InlineFilterConfigurationExtensions (class):  UseInlineFilter
      InstanceExtensions (class):  ConnectInstance, Instance
         Extensions for subscribing object instances.
      InstrumentationConfigurationExtensions (class):  UseInstrumentation
      IntStateAccessor (class):  Get, GetStateExpression, IntStateAccessor, Probe, Set
         Accesses the current state as a string property
      InterceptConfigurationExtensions (class):  UseIntercept
      IntrospectionExtensions (class):  GetProbeResult
      InvalidCompensationAddressException (class):  InvalidCompensationAddressException
      InvalidLicenseException (class):  InvalidLicenseException
      InvalidLicenseFormatException (class):  InvalidLicenseFormatException
      JobAlreadyExistsException (class):  JobAlreadyExistsException
      JobAttemptSaga (class)
         Each attempt to run a job is tracked by this state
      JobAttemptStateMachine (class):  JobAttemptStateMachine
      JobAttemptStateMachineBehaviorExtensions (class):  GetJobAttemptSagaAddress, GetJobSagaAddress, GetRetryDelay, ScheduleJobStatusCheck, SendCancelJobAttempt, SendCheckJobStatus, SendJobAttemptFaulted, SendJobAttemptStartTimeout, SendStartJob
      JobConsumerOptions (class):  JobConsumerOptions, SetHeartbeatInterval, SetRejectedJobDelay, Validate
      JobContext (interface):  SaveJobState, SetJobProgress, TryGetJobState
      JobNotFoundException (class):  JobNotFoundException
      JobOptions (class):  JobOptions, SetConcurrentJobLimit, SetGlobalConcurrentJobLimit, SetInstanceProperties, SetJobCancellationTimeout, SetJobTimeout, SetJobTypeName, SetJobTypeProperties, SetProgressBuffer, SetRetry, Validate
         JobOptions contains the options used to configure the job consumer and related components
      JobSaga (class)
         Individual turnout jobs are tracked by this state
      JobSagaBusConfigurationExtensions (class):  SetPartitionedReceiveMode, UseJobSagaPartitionKeyFormatters
      JobSagaOptions (class):  JobSagaOptions, Validate
      JobServiceConfigurationExtensions (class):  ConfigureJobService, ConfigureJobServiceEndpoints
      JobServiceContainerConfigurationExtensions (class):  ConfigureSagaRepositories
      JobServiceEventExtensions (class):  GetJob
      JobServiceExtensions (class):  CancelJob, CreateSubmitJobCommand, FinalizeJob, GetJobState, RetryJob, SubmitJob
      JobServiceOptions (class):  JobServiceOptions, Validate
      JobServiceRegistrationExtensions (class):  AddJobSagaStateMachines, IsJobServiceEndpoint, SetJobConsumerOptions, TryAddJobDistributionStrategy
      JobServiceStoppingException (class):  JobServiceStoppingException
      JobStateMachine (class):  JobStateMachine
      JobStateMachineBehaviorExtensions (class):  CalculateNextStartDate, CancelCurrentJobAttempt, ClearJobState, ClearNextStartDate, DetermineNextStartDate, FinalizeJobAttempts, GetJobAttemptSagaAddress, GetJobTypeSagaAddress, InitializeJob, NotifyJobCompleted, NotifyJobFaulted, PublishJobCanceled, PublishJobStarted, RequestJobSlot, RequestRetryJobSlot
      JobTypeInfo (interface)
      JobTypeInstance (class)
      JobTypeSaga (class):  JobTypeSaga
         Every job type has one entry in this state machine
      JobTypeStateMachine (class):  JobTypeStateMachine
      JobTypeStateMachineBehaviorExtensions (class):  GetJobDistributionStrategyOrUseDefault, IsSlotAvailable, SetConcurrentLimit
      JsonMessageBody (interface):  GetJsonElement
         If the incoming message is in a JSON format, use this to unwrap the JSON document from any transport-specific encapsu...
      JsonSerializerConfigurationExtensions (class):  ConfigureJsonSerializerOptions, SetMessageSerializerOptions, UseJsonDeserializer, UseJsonSerializer
      KafkaConfigurationExtensions (class):  GetConsumerConfig, GetProducerConfig
      KafkaConfiguratorExtensions (class):  Host, TopicEndpoint
      KafkaConnectionException (class):  IsExceptionTransient, KafkaConnectionException
      KafkaConsumeContext (interface)
      KafkaConsumeContextExtensions (class):  GetKey, IsPartitionEof, Offset, Partition
      KafkaDependencyInjectionTestingExtensions (class):  ConfigureKafkaTestOptions
      KafkaIntegrationExtensions (class):  UsingKafka
      KafkaProducerConfiguratorExtensions (class):  AsAsyncOverSync, SetKeySerializer, SetValueSerializer
      KafkaProducerRegistrationExtensions (class):  AddProducer, GetProducer
      KafkaSendContext (interface)
      KafkaTestHarnessExtensions (class):  GetProducer
      KafkaTestHarnessOptions (class)
      KafkaTopicAddress (struct):  KafkaTopicAddress, ParseLeft
      KafkaTopicProducerProviderExtensions (class):  GetProducer
      KebabCaseEndpointNameFormatter (class):  KebabCaseEndpointNameFormatter, SanitizeName
         Formats the endpoint names using kebab-case (dashed snake case) SubmitOrderConsumer -> submit-order OrderState -> ord...
      KillSwitchConfigurationExtensions (class):  UseKillSwitch
      KillSwitchOptionsSpecification (class):  Apply, KillSwitchOptionsSpecification, Validate
      LatestConfigurationExtensions (class):  UseLatest
      LegacySerializationExtensions (class):  ClearMessageDeserializers
      Limit (class):  Limit
      LogContext (class):  ConfigureCurrentLogContext, ConfigureCurrentLogContextIfNull, CreateDefaultLogContext, CreateLogContext, Define, DefineMessage, LogContext, SetCurrentIfNull
      MartenSagaRepositoryRegistrationExtensions (class):  MartenRepository, SetMartenSagaRepositoryProvider
      MassTransitApplicationException (class):  ImportExceptionData, MassTransitApplicationException
         For use by application developers to include additional data elements along with the exception, which will be transfe...
      MassTransitBus (class):  CheckHealth, ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectEndpointConfigurationObserver, ConnectPublishObserver, ConnectReceiveEndpoint, ConnectReceiveEndpointObserver, ConnectReceiveObserver, ConnectRequestPipe, ConnectSendObserver, GetPublishSendEndpoint, GetSendEndpoint, MassTransitBus, Probe
      MassTransitException (class):  MassTransitException
      MassTransitExceptionExtensions (class):  ContainsFailure, ThrowIfContainsFailure
      MassTransitHealthCheckOptionsExtensions (class):  ConfigureHealthCheckOptions
      MassTransitHostOptions (class)
         If present in the container, these options will be used by the MassTransit hosted service.
      MassTransitHostedService (class):  DisposeAsync, MassTransitHostedService, StartAsync, StopAsync
      MassTransitMongoDbConventions (class):  IsMassTransitClass, IsSagaClass, MassTransitMongoDbConventions, RegisterClass
      MassTransitStateMachine (class):  Accept, BindActivitiesToState, BindEveryTransitionEvent, BindTransitionEvents, CompositeEvent, ConnectEventObserver, ConnectStateObserver, CreateRegistration, DeclareDataEvent, DeclareEvent, DeclarePropertyBasedEvent, DeclareState, DeclareTriggerEvent, DefaultUnhandledEventCallback, GetBackingFields
         A MassTransit state machine adds functionality on top of Automatonymous supporting things like request/response, and ...
      MediatorConfigurationExtensions (class):  CreateMediator
      MediatorRequestExtensions (class):  SendRequest
      MessageBody (interface):  GetBytes, GetStream, GetString
      MessageConsumeTopology (class):  Add, AddDelegate, AddOrUpdateConvention, Apply, MessageConsumeTopology, TryAddConvention, UpdateConvention, Validate
      MessageContext (interface)
         The message context includes the headers that are transferred with the message
      MessageCorrelation (class):  UseCorrelationId
      MessageData (interface)
         MessageData is used when a property size may be larger than what should be sent via the message transport.
      MessageDataConfiguratorExtensions (class):  UseMessageData
      MessageDataDefaults (class):  MessageDataDefaults
      MessageDataException (class):  MessageDataException
      MessageDataExtensions (class):  GetBytes, GetString, PutBytes, PutObject, PutStream, PutString
      MessageDataNotFoundException (class):  MessageDataNotFoundException
      MessageDataRepositorySelectorExtensions (class):  AzureStorage
      MessageDefaults (class)
      MessageEntityNameFormatter (class):  FormatEntityName, InitializeEntityNameFromAttributeIfSpecified, MessageEntityNameFormatter
      MessageException (class):  MessageException
      MessageHeaders (class)
      MessageInitializerException (class):  MessageInitializerException
      MessageLockExpiredException (class):  MessageLockExpiredException
      MessageNameFormatterEntityNameFormatter (class):  FormatEntityName, MessageNameFormatterEntityNameFormatter
      MessageNotAcknowledgedException (class):  MessageNotAcknowledgedException
         Thrown when a message is not acknowledged by the broker
      MessageNotConsumedException (class):  MessageNotConsumedException
      MessagePackConfigurationExtensions (class):  UseMessagePackDeserializer, UseMessagePackSerializer
      MessageRedeliveryContext (interface):  ScheduleRedelivery
         Used to reschedule delivery of the current message
      MessageRetryConfigurationExtensions (class):  UseMessageRetry
      MessageRetryLimitExceededException (class):  MessageRetryLimitExceededException
      MessageReturnedException (class):  MessageReturnedException
         Published when a RabbitMQ channel is closed and the message was not confirmed by the broker.
      MessageSchedulerBusExtensions (class):  CreateDelayedMessageScheduler, CreateMessageScheduler
      MessageSchedulerContext (interface):  ScheduleSend
      MessageSchedulerExtensions (class):  UseMessageScheduler, UsePublishMessageScheduler
      MessageSchedulerRegistrationExtensions (class):  AddMessageScheduler, AddPublishMessageScheduler
      MessageSessionSagaRepositoryConfigurationExtensions (class):  MessageSessionRepository, SetMessageSessionSagaRepositoryProvider
      MessageTimeToLiveExpiredException (class):  MessageTimeToLiveExpiredException
      MessageTypeCache (class):  CheckIfTemporaryMessageType, CheckIfValidMessageType, GetDiagnosticAddress, GetMessageTypeNames, GetMessageTypes, GetOrAdd, GetProperties, InvalidMessageTypeReason, IsTemporaryMessageType, IsValidMessageType, MessageTypeCache, PropertyListFactory
      MessageTypeSelector (class):  GetMessageTopology, MessageTypeSelector
      MessageTypeSelectorFactory (struct):  ActivateType
      MessageUrn (class):  Deconstruct, ForType, ForTypeString, GetMessageName, GetMessageNameFromAttribute, GetMessageNameFromType, GetUrnForType, MessageUrn, ValueFactory
      MessageUrnAttribute (class):  FormatUrn, MessageUrnAttribute
         Specify the message type name for this message type
      MessageUrnCache (class):  MessageUrnCache
      MessageUrnEntityNameFormatter (class):  FormatEntityName
         This is the simplest thing, it uses the built-in URN for a message type as the entity name, which can include illegal...
      MetricsContext (interface):  Populate
      MetricsContextExtensions (class):  AddMetricTags, SetMetricTags
      MissingInstanceRedeliveryExtensions (class):  Redeliver
      MongoDbAuditStoreConfiguratorExtensions (class):  ConfigureAuditStore, UseMongoDbAuditStore
      MongoDbConcurrencyException (class):  MongoDbConcurrencyException
      MongoDbMessageDataConfigurationExtensions (class):  MongoDb
      MongoDbOutboxConfigurationExtensions (class):  AddMongoDbOutbox, UseMongoDbOutbox
      MongoDbSagaRepositoryConfiguratorExtensions (class):  ClassMap, CollectionNameFormatter, Database
      MongoDbSagaRepositoryRegistrationExtensions (class):  MongoDbRepository, SetMongoDbSagaRepositoryProvider
      MongoDbSaveEventException (class):  GetObjectData, MongoDbSaveEventException
      NHibernateSagaRepositoryRegistrationExtensions (class):  NHibernateRepository, SetNHibernateSagaRepositoryProvider
      NHibernateStatePropertyExtensions (class):  CompositeEventProperty, StateProperty
      NServiceBusSerializerConfigurationExtensions (class):  SupportNServiceBusJsonDeserializer, SupportNServiceBusXmlDeserializer, UseNServiceBusJsonSerializer, UseNServiceBusXmlSerializer
      NewId (struct):  CompareTo, Equals, FromByteArray, FromGuid, FromSequentialByteArray, FromSequentialGuid, GetFormatterArray, GetHashCode, GetSequentialFormatterArray, NewId, Next, NextGuid, NextSequentialGuid, SetGenerator, SetProcessIdProvider
         A NewId is a type that fits into the same space as a Guid/Uuid/unique identifier, but is guaranteed to be both unique...
      NewIdExtensions (class):  ToNewId, ToNewIdFromSequential
      NewIdGenerator (class):  NewIdGenerator, Next, NextGuid, NextSequentialGuid, UpdateTimestamp
      NewtonsoftBsonConfigurationExtensions (class):  ConfigureBsonDeserializer, ConfigureBsonSerializer, UseBsonDeserializer, UseBsonSerializer
      NewtonsoftJsonConfigurationExtensions (class):  ConfigureNewtonsoftJsonDeserializer, ConfigureNewtonsoftJsonSerializer, UseNewtonsoftJsonDeserializer, UseNewtonsoftJsonSerializer
      NewtonsoftRawJsonConfigurationExtensions (class):  AddNewtonsoftRawJsonSerializer, UseNewtonsoftRawJsonDeserializer, UseNewtonsoftRawJsonSerializer
      NewtonsoftRawXmlConfigurationExtensions (class):  UseRawXmlDeserializer, UseRawXmlSerializer
      NewtonsoftXmlConfigurationExtensions (class):  UseXmlDeserializer, UseXmlSerializer
      NonTransitionEventObserver (class):  ExecuteFault, NonTransitionEventObserver, PostExecute, PreExecute
      NotAcceptedStateMachineException (class):  NotAcceptedStateMachineException
      NotImplementedByDesignException (class):  NotImplementedByDesignException
      ObserverExtensions (class):  ConnectObserver, ConnectRequestObserver, Observer
      ObserverRegistrationExtensions (class):  AddBusObserver, AddConsumeObserver, AddEventObserver, AddPublishObserver, AddReceiveEndpointObserver, AddReceiveObserver, AddSendObserver, AddStateObserver
      Observes (interface)
      OneTimeContext (interface):  Evict
      OneTimeContextPayload (class):  Evict, OneTimeContextPayload, RunOnce, RunOneTime, SetException, SetResult, TryTake
      OneTimeSetupMethod (class):  OneTimeSetupMethod, SetPayload, SetupPayload
      Orchestrates (interface)
         Specifies that a class implementing ISaga consumes TMessage as part of the saga
      OutboxDeliveryServiceOptions (class)
      PartitionKeyConsumeContext (interface)
      PartitionKeyConventionExtensions (class):  UsePartitionKeyFormatter
      PartitionKeyExtensions (class):  PartitionKey, SetPartitionKey, TrySetPartitionKey
      PartitionKeySendContext (interface)
      PartitionerConfigurationExtensions (class):  CreatePartitioner, UseConsumerPartitioner, UseMessagePartitioner, UsePartitioner, UseSagaPartitioner
      PayloadException (class):  PayloadException
      PayloadFactoryException (class):  PayloadFactoryException
      PayloadNotFoundException (class):  PayloadNotFoundException
      PerformanceCounterExtensions (class):  EnableStatsdPerformanceCounters
      Pipe (class):  AddCallback, Empty, Execute, ExecuteAsync, New, ToPipe
      PipeConfigurationException (class):  PipeConfigurationException
      PipeContext (interface):  AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, TryGetPayload
         The base context for all pipe types, includes the payload side-banding of data with the payload, as well as the cance...
      PipeContextHandle (interface)
         A handle to a PipeContext instance (of type ), which can be disposed once it is no longer needed (or can no longer be...
      PipeExtensions (class):  GetPayload, IsEmpty, IsNotEmpty, OneTimeSetup
      PipeFactoryException (class):  PipeFactoryException
      PipelineException (class):  PipelineException
      PostgresBusFactoryConfiguratorExtensions (class):  UsingPostgres
      PostgresHostConfigurationExtensions (class):  UsePostgres
      PostgresSqlTransportConfigurationExtensions (class):  AddPostgresMigrationHostedService
      PrefixEntityNameFormatter (class):  FormatEntityName, PrefixEntityNameFormatter
      ProbeContext (interface):  Add, CreateScope, Set
         Passed to a probe site to inspect it for interesting things
      ProbeContextExtensions (class):  CreateConsumerFactoryScope, CreateFilterScope, CreateMessageScope
      ProduceException (class):  ProduceException
      ProduceExtensions (class):  Produce
      ProgressBufferSettings (class):  ProgressBufferSettings
         Specifies the settings for the progress buffer, which defers updating the job progress until the thresholds (steps or...
      Provider (class)
      PublishContext (interface)
      PublishContextAsyncPipe (class):  Probe, PublishContextAsyncPipe, Send
      PublishContextPipe (class):  Probe, PublishContextPipe, Send
      PublishEndpointExtensions (class):  Publish
      PublishEndpointRecurringSchedulerExtensions (class):  CancelScheduledRecurringSend, PauseScheduledRecurringSend, ResumeScheduledRecurringSend, ScheduleRecurringSend
      PublishEventContext (class):  PublishEventContext
      PublishException (class):  PublishException
      PublishExecuteExtensions (class):  Publish, ToPipe
      PublishExtensions (class):  Publish, PublishAsync, Uplift
      PushPipe (class):  Probe, PushPipe, Send
      Quartz (class)
      QuartzEndpointOptions (class)
      QuartzIntegrationExtensions (class):  GetDefaultConfiguration, UseInMemoryScheduler
      QuartzRegistrationExtensions (class):  AddQuartzConsumers, ConfigureQuartzConsumers
      QuartzSchedulerOptions (class)
      RabbitMqAddressException (class):  RabbitMqAddressException
      RabbitMqBasicConsumeContext (interface)
         Contains the context of the BasicConsume call received by the BasicConsumer bound to the inbound RabbitMQ channel
      RabbitMqBusFactory (class):  Create, CreateMessageTopology
      RabbitMqBusFactoryConfiguratorExtensions (class):  CreateUsingRabbitMq, UsingRabbitMq
      RabbitMqConfigureEndpointCallbackExtensions (class):  AddRabbitMqConfigureEndpointCallback, AddRabbitMqConfigureEndpointsCallback
      RabbitMqConnectionException (class):  IsExceptionTransient, RabbitMqConnectionException
      RabbitMqConsumeContextExtensions (class):  GetRabbitMqTimestamp
      RabbitMqDeferMessageExtensions (class):  Defer
      RabbitMqDependencyInjectionTestingExtensions (class):  ConfigureRabbitMqTestOptions
      RabbitMqEndpointAddress (struct):  GetDelaySettings, GetQueryStringOptions, ParseLeft, RabbitMqEndpointAddress, ToShortAddress
      RabbitMqHostAddress (struct):  ParseLeft, RabbitMqHostAddress
      RabbitMqHostConfigurationExtensions (class):  Host, ReceiveEndpoint
      RabbitMqHostSettings (interface):  Refresh
         Settings to configure a RabbitMQ host explicitly without requiring the fluent interface
      RabbitMqPublishTopologyConfigurationExtensions (class):  AddPublishMessageTypes, AddPublishMessageTypesFromNamespaceContaining
      RabbitMqRequestClientExtensions (class):  CreateReplyToClientFactory, SetRabbitMqReplyToRequestClientFactory
      RabbitMqSendContext (interface)
      RabbitMqSendContextExtensions (class):  SetAwaitAck, SetHeader, SetPriority, SetStreamFilterValue, SetTransportHeader, TrySetAwaitAck, TrySetPriority, TrySetStreamFilterValue
      RabbitMqSslOptions (class)
      RabbitMqTestHarnessOptions (class)
      RabbitMqTransportOptions (class):  RabbitMqTransportOptions
      RateLimitConfigurationExtensions (class):  UseRateLimit
      RateLimitExtensions (class):  SetRateLimit
      RawJsonSerializerConfigurationExtensions (class):  AddRawJsonSerializer, UseRawJsonDeserializer, UseRawJsonSerializer
      RawStateAccessor (class):  Get, GetStateExpression, Probe, RawStateAccessor, Set
      ReceiveContext (interface):  AddReceiveTask, NotifyConsumed, NotifyFaulted
         The receive context is sent from the transport when a message is ready to be processed from the transport.
      ReceiveContextBodyExtensions (class):  GetBody, GetBodyStream
      ReceiveContextExtensions (class):  GetConversationId, GetCorrelationId, GetEndpointAddress, GetFaultAddress, GetHeaderId, GetInitiatorId, GetMessageEncoding, GetMessageId, GetMessageTypes, GetRequestId, GetResponseAddress, GetSentTime, GetSourceAddress, GetTimestamp
      ReceiveEndpointCompleted (interface)
      ReceiveEndpointConfigurationExtensions (class):  ReceiveEndpoint
      ReceiveEndpointConfiguratorDependencyExtensions (class):  AddDependency
      ReceiveEndpointDependency (class):  Completed, Faulted, Ready, ReceiveEndpointDependency, Stopping
      ReceiveEndpointDependent (class):  Completed, Faulted, Ready, ReceiveEndpointDependent, Stopping
      ReceiveEndpointEvent (interface)
      ReceiveEndpointFaulted (interface)
      ReceiveEndpointHandle (interface):  Stop
         A handle to an active endpoint
      ReceiveEndpointReady (interface)
      ReceiveEndpointStopping (interface)
      ReceiveFault (interface)
         Published when a message fails to deserialize at the endpoint
      ReceivePipeConfigurationExtensions (class):  ConfigureDefaultDeadLetterTransport, ConfigureDefaultErrorTransport, DiscardFaultedMessages, DiscardSkippedMessages, RethrowFaultedMessages, ThrowOnSkippedMessages
      ReceiveTransportCompleted (interface)
      ReceiveTransportEvent (interface)
      ReceiveTransportFaulted (interface)
      ReceiveTransportReady (interface)
      RecurringJobConsumerExtensions (class):  AddOrUpdateRecurringJob, CancelRecurringJob, FinalizeRecurringJob, RunRecurringJob, ScheduleJob
      RecurringJobException (class):  RecurringJobException
      RecurringJobScheduleConfiguratorExtensions (class):  At, DailyAt, Every, Monthly, SetTimeZone, ValidateDayOfMonth, ValidateHour, ValidateMinute, ValidateMonth, ValidateSecond, ValidateYear, Weekly, Yearly
      RedeliverExtensions (class):  Redeliver
      RedisSagaConcurrencyException (class):  RedisSagaConcurrencyException
      RedisSagaRepositoryOptions (class):  FormatLockKey, FormatSagaKey, RedisSagaRepositoryOptions
      RedisSagaRepositoryRegistrationExtensions (class):  RedisRepository, SetRedisSagaRepositoryProvider
      RegisterActivity (class):  Register
      RegisterConsumer (class):  Register
      RegisterExecuteActivity (class):  Register
      RegisterFuture (class):  Register
      RegisterSaga (class):  Register
      RegisterSagaStateMachine (class):  Register
      RegistrationConfiguratorExtensions (class):  AddActivity, AddConsumer, AddExecuteActivity, AddFuture, AddSaga, AddSagaStateMachine
      RegistrationContextExtensions (class):  ConfigureActivity, ConfigureActivityCompensate, ConfigureActivityExecute, ConfigureConsumer, ConfigureConsumers, ConfigureEndpoints, ConfigureExecuteActivity, ConfigureSaga, ConfigureSagas, ConfigureServiceEndpoints
      RegistrationExtensions (class):  AddActivities, AddActivitiesFromNamespaceContaining, AddActivity, AddConsumer, AddConsumers, AddConsumersFromNamespaceContaining, AddExecuteActivity, AddFuture, AddFutureRequestConsumer, AddFutures, AddFuturesFromNamespaceContaining, AddSaga, AddSagaStateMachine, AddSagaStateMachines, AddSagaStateMachinesFromNamespaceContaining
      RegistrationForConsumer (class):  RegisterTestHarness
      RegistrationForSaga (class):  RegisterTestHarness
      RegistrationForSagaStateMachine (class):  RegisterTestHarness
      ReplyToEndpointDefinition (class):  Configure, GetEndpointName, ReplyToEndpointDefinition
      Request (class):  GenerateRequestId, GetRequestId, SetRequestId, SetSendContextHeaders
      RequestCanceledException (class):  FormatMessage, RequestCanceledException
      RequestConsumerFuture (class):  RequestConsumerFuture
      RequestEventExtensions (class):  RequestCompleted, RequestFaulted, RequestStarted
      RequestException (class):  RequestException
      RequestExtensions (class):  Request
      RequestFaultException (class):  GetObjectData, RequestFaultException
      RequestHandle (interface):  Cancel, GetResponse
         A request handle manages the client-side request, and allows the request to be configured, response types added, etc.
      RequestSettings (interface)
         The request settings include the address of the request handler, as well as the timeout to use for requests.
      RequestTimeout (struct):  After, Equals, GetHashCode, Or, RequestTimeout
         A timeout, which can be a default (none) or a valid TimeSpan > 0, includes factory methods to make it "cute"
      RequestTimeoutException (class):  FormatMessage, RequestTimeoutException
      RescueConfigurationExtensions (class):  UseRescue
      RespondAsyncExecuteExtensions (class):  RespondAsync
      RespondExtensions (class):  Respond, RespondAsync
      Response (interface):  Deconstruct, Is, Response
         The base response type, which can be used to pattern match, via deconstruct, to the accepted response types.
      ResponseEndpointDefinition (class):  ResponseEndpointDefinition
         Specifies a temporary endpoint, with the prefix "response"
      ResponseExtensions (class):  Deconstruct, IsResponseAccepted
      Result (class):  Result, ToString
      Retry (class):  All, CreatePolicy, Except, Exponential, Filter, Immediate, Incremental, Interval, Intervals, Selected
      RetryConfigurationExtensions (class):  Exponential, Factory, Immediate, Incremental, Interval, Intervals, None, UseMessageRetry, UseRetry
      RetryConfigurator (class):  Build, ConnectRetryObserver, RetryConfigurator, SetRetryPolicy, Validate
      RetryContext (interface):  CanRetry, PreRetry, RetryFaulted
         The base context of a retry
      RetryContextExtensions (class):  GetRedeliveryCount, GetRetryAttempt, GetRetryCount
      RetryPolicyContext (interface):  CanRetry, Cancel, RetryFaulted
         An initial context acquired to begin a retry filter
      RetrySpecification (class):  Build, ConnectRetryObserver, RetrySpecification, SetRetryPolicy, Validate
      RiderReady (interface)
      RoutingKeyConsumeContext (interface)
      RoutingKeyConventionExtensions (class):  UseRoutingKeyFormatter
      RoutingKeyExtensions (class):  RoutingKey, SetRoutingKey, TrySetRoutingKey
      RoutingKeySendContext (interface)
      RoutingSlipArgumentException (class):  RoutingSlipArgumentException
      RoutingSlipBuilder (class):  AddActivitiesFromSourceItinerary, AddActivity, AddActivityException, AddActivityLog, AddCompensateLog, AddSubscription, AddVariable, Build, GetObjectAsDictionary, RoutingSlipBuilder, SetVariables, SetVariablesFromDictionary
         A RoutingSlipBuilder is used to create a routing slip with proper validation that the resulting RoutingSlip is valid.
      RoutingSlipEventConsumerConfigurationExtensions (class):  RoutingSlipActivityEventConsumers, RoutingSlipEventConsumers
      RoutingSlipEventExtensions (class):  GetArgument, GetDictionaryValue, GetResult, GetVariable
      RoutingSlipException (class):  RoutingSlipException
      RoutingSlipExtensions (class):  Execute, GetNextCompensateAddress, GetNextExecuteAddress, RanToCompletion
      RoutingSlipRequestFaultedException (class):  RoutingSlipRequestFaultedException
      SagaClassMap (class):  Configure
      SagaConsumeContext (interface):  SetCompleted
         Consume context including the saga instance consuming the message
      SagaDefinition (class):  Configure, GetEndpointName
         A saga definition defines the configuration for a saga, which can be used by the automatic registration code to confi...
      SagaException (class):  FormatMessage, SagaException
      SagaExtensions (class):  ConnectSaga, Saga
      SagaPipeConfiguratorExtensions (class):  UseFilter
      SagaQueryExpressionPropertyExtensions (class):  TryGetPropertyValue
      SagaStateMachine (interface):  IsCompleted
      SagaStateMachineException (class):  SagaStateMachineException
      SagaStateMachineExtensions (class):  CreateSagaFilter, CreateSagaQuery
      SagaStateMachineInstance (interface)
         An Automatonymous state machine instance that is usable as a saga by MassTransit must implement this interface.
      SagaStateMachineReceiveEndpointExtensions (class):  ConnectStateMachineSaga, StateMachineSaga
      Schedule (interface):  GetDelay, GetTokenId, SetTokenId
         Holds the state of a scheduled message
      ScheduleDateTimeExtensions (class):  Schedule
      SchedulePublishExtensions (class):  SchedulePublish
      ScheduleSettings (interface)
         The schedule settings, including the default delay for the message
      ScheduleTimeSpanExtensions (class):  Schedule, Unschedule
      ScheduledMessage (interface)
      ScheduledRecurringMessage (interface)
      ScheduledRedeliveryConfigurationExtensions (class):  Factory, UseScheduledRedelivery
      SchedulingExtensions (class):  GetQuartzNextScheduled, GetQuartzPreviousSent, GetQuartzScheduleGroup, GetQuartzScheduleId, GetQuartzScheduled, GetQuartzSent, GetSchedulingTokenId
      SelectedEventObserver (class):  ExecuteFault, PostExecute, PreExecute, SelectedEventObserver
      SendByConventionExtensions (class):  GetDestinationAddress, Send, SendAsync
      SendCallbackExtensions (class):  Send, SendAsync
      SendCommandContext (class):  SendCommandContext
      SendConsumeContextExecuteExtensions (class):  Send
      SendConsumeContextExtensions (class):  Send
      SendContext (interface):  CreateProxy
         The SendContext is used to tweak the send to the endpoint
      SendContextAsyncPipe (class):  Probe, Send, SendContextAsyncPipe
      SendContextExtensions (class):  ApplyRedeliveryOptions, GetOriginalMessageId, ReplaceMessageId, SetExceptionHeaders, SetHostHeaders, StartNewConversation, TransferConsumeContextHeaders
      SendContextPipe (class):  Probe, Send, SendContextPipe
      SendEndpointExtensions (class):  Send
      SendEndpointRecurringSchedulerExtensions (class):  CancelScheduledRecurringSend, PauseScheduledRecurringSend, ResumeScheduledRecurringSend, ScheduleRecurringSend
      SendException (class):  SendException
      SendExecuteExtensions (class):  Send, ToPipe
      SendExtensions (class):  Send, SendAsync
      SendHeaders (interface):  Set
      SendHeadersExtensions (class):  CopyFrom
      SendTuple (struct):  Deconstruct, SendTuple
         Combines a message and a pipe which can be used to send/publish the message
      SerializerConfigurationExtensions (class):  UseEncryptedSerializer, UseEncryptedSerializerV2, UseEncryption
      SerializerContext (interface):  GetMessageSerializer, IsSupportedMessageType, ToDictionary, TryGetMessage
      SerializerContextExtensions (class):  DeserializeDictionary, GetHeader, GetValue, SerializeDictionary, ToDictionary, TryGetHeader, TryGetValue
      ServiceBusBatchingExtensions (class):  SetServiceBusSessionBatchOptions
      ServiceBusBrokerTopologyExtensions (class):  LogResult
      ServiceBusBusFactoryConfiguratorExtensions (class):  Host, NamedKey, ReceiveEndpoint, SharedAccessSignature
      ServiceBusBusTopologyExtensions (class):  GetServiceBusBusTopology
      ServiceBusConfigurationExtensions (class):  AddSubscriptionEndpointConnector, CreateUsingAzureServiceBus, UsingAzureServiceBus
      ServiceBusConfigureEndpointCallbackExtensions (class):  AddServiceBusConfigureEndpointCallback, AddServiceBusConfigureEndpointsCallback
      ServiceBusConnectionException (class):  IsExceptionTransient, ServiceBusConnectionException
      ServiceBusDependencyInjectionTestingExtensions (class):  ConfigureServiceBusTestOptions
      ServiceBusEndpointAddress (struct):  GetQueryStringOptions, ParseLeft, ServiceBusEndpointAddress
      ServiceBusHostAddress (struct):  ParseLeft, ServiceBusHostAddress
      ServiceBusHostSettings (interface)
         The host settings used to configure the service bus connection
      ServiceBusMessageContext (interface)
         The context of a Message from AzureServiceBus - gives access to the transport message when requested.
      ServiceBusMessageContextExtensions (class):  ReplyToSessionId, SessionId
      ServiceBusMessageSchedulerBusExtensions (class):  AddServiceBusMessageScheduler, CreateServiceBusMessageScheduler
      ServiceBusPublishTopologyConfigurationExtensions (class):  AddPublishMessageTypes, AddPublishMessageTypesFromNamespaceContaining
      ServiceBusReceivePipeConfiguratorExtensions (class):  ConfigureDeadLetterQueueDeadLetterTransport, ConfigureDeadLetterQueueErrorTransport
      ServiceBusScheduleMessageExtensions (class):  UseDelayedMessageScheduler, UseServiceBusMessageScheduler
      ServiceBusSendContext (interface)
      ServiceBusSendContextExtensions (class):  SetLabel, SetReplyTo, SetReplyToSessionId, SetScheduledEnqueueTime, SetSessionId
      ServiceBusSessionBatchOptions (class):  SetMaxConcurrentSessions, SetMessageLimitPerSession, SetSessionIdleTimeout, SetTimeLimit, SetTimeLimitStart
      ServiceBusSessionIdConventionExtensions (class):  UseSessionIdFormatter
      ServiceInstanceConfigurationExtensions (class):  ServiceInstance
      ServiceInstanceOptions (class):  EnableJobServiceEndpoints, ServiceInstanceOptions, SetEndpointNameFormatter
      SetSerializerSendConventionExtensions (class):  UseSerializer
      ShutDownException (class):  ShutDownException
      SnakeCaseEndpointNameFormatter (class):  SanitizeName, SnakeCaseEndpointNameFormatter
         Formats the endpoint name using snake case.
      SqlBusFactory (class):  Create, CreateMessageTopology
      SqlConfigureEndpointCallbackExtensions (class):  AddSqlConfigureEndpointCallback, AddSqlConfigureEndpointsCallback
      SqlEndpointAddress (struct):  GetQueryStringOptions, ParseLeft, SqlEndpointAddress
      SqlEndpointAddressException (class):  SqlEndpointAddressException
      SqlHostAddress (struct):  GetQueryStringOptions, GetVirtualHostAndArea, IsValidSymbol, SqlHostAddress
         The database host address is composed of specific parts db://localhost/virtual_host_name.scope db://localhost/.scope ...
      SqlHostSettings (interface):  CreateConnectionContextFactory, GetLicenseInfo
         Settings to configure a DbTransport host explicitly without requiring the fluent interface
      SqlMessageContext (interface)
      SqlPublishTopologyConfigurationExtensions (class):  AddPublishMessageTypes, AddPublishMessageTypesFromNamespaceContaining
      SqlReceiveEndpointConfigurationExtensions (class):  ReceiveEndpoint
      SqlScheduleMessageExtensions (class):  AddSqlMessageScheduler, UseDbMessageScheduler, UseDelayedMessageScheduler, UseSqlMessageScheduler
      SqlSendContext (interface)
      SqlSendContextExtensions (class):  SetPriority, TrySetPriority
      SqlServerBusFactoryConfiguratorExtensions (class):  UsingSqlServer
      SqlServerDbTransportConfigurationExtensions (class):  AddSqlServerMigrationHostedService
      SqlServerHostConfigurationExtensions (class):  UseSqlServer
      SqlTopologyException (class):  SqlTopologyException
      SqlTransportMigrationOptions (class)
      SqlTransportOptions (class)
      State (interface):  AddSubstate, Bind, HasState, Ignore, IsStateOf, Raise
      StateAccessorExtensions (class):  GetState
      StateAccessorIndex (class):  CreateStateArray, StateAccessorIndex
      StateMachine (interface):  ConnectEventObserver, ConnectStateObserver, GetEvent, GetState, IsCompositeEvent, NextEvents, RaiseEvent
         A state machine definition
      StateMachineEvent (class):  StateMachineEvent
      StateMachineExtensions (class):  TransitionToState
      StateMachineIntrospectionExtensions (class):  NextEvents
      StateMachineRegistration (interface):  Declare
      StateMachineRequest (class):  EventFilter, GenerateRequestId, GetRequestId, Method1, Method12, Method2, Method22, SetRequestId, SetSendContextHeaders, StateMachineRequest
      StateMachineRequestExtensions (class):  CancelRequestTimeout, Request
      StateMachineSagaSpecification (class):  StateMachineSagaSpecification, Validate
      StateMachineSchedule (class):  GetDelay, GetTokenId, SetTokenId, StateMachineSchedule
      StateMachineState (class):  Accept, AddSubstate, Bind, CompareTo, Equals, GetHashCode, GetStateEvents, HasState, Ignore, IsRealEvent, IsStateOf, Probe, Raise, StateMachineState, ToString
      StateMachineVisitor (interface):  Visit
      StateObservable (class):  Method4, Method5, Method6, StateChanged
      StateRegistration (class):  Declare, StateRegistration
      StaticEntityNameFormatter (class):  FormatEntityName, StaticEntityNameFormatter
      StopContext (interface)
         The context associated with stopping an agent
      StopSupervisorContext (interface)
      StringMessageBody (class):  GetBytes, GetStream, GetString, StringMessageBody
      StringStateAccessor (class):  Get, GetStateExpression, Probe, Set, StringStateAccessor
         Accesses the current state as a string property
      SupervisorExtensions (class):  AddActiveContext, AddAsyncContext, AddContext, CreateAgent
      TagListMetricsContext (class):  AddTag, Populate, TagListMetricsContext
      TemporaryEndpointDefinition (class):  Configure, GetEndpointName, TemporaryEndpointDefinition
         Specifies a temporary endpoint, with the prefix "response"
      TestHarnessOptions (class)
      ThenExtensions (class):  Execute, ExecuteAsync, Then, ThenAsync
      TimeSpanContextScheduleExtensions (class):  ScheduleSend
      TimeSpanScheduleExtensions (class):  ScheduleSend
      TimeSpanSchedulePublishExtensions (class):  SchedulePublish
      TimeoutConfiguratorExtensions (class):  UseTimeout
      TopicEndpointConnectorExtensions (class):  ConnectTopicEndpoint
      TransactionConfiguratorExtensions (class):  UseTransaction
      TransactionContext (interface):  Commit, Rollback
      TransactionContextExtensions (class):  CreateTransactionScope
      TransformConfigurationExtensions (class):  UseTransform
      TransformContext (interface)
         Context used by a message transform
      TransitionExtensions (class):  Finalize, TransitionTo
      TransportException (class):  TransportException
      TransportUnavailableException (class):  TransportUnavailableException
      TriggerEventRegistration (class):  Declare, TriggerEventRegistration
      TypeAdapter (class):  TryGetEndpointAddress
      TypeCache (class):  GetOrAdd, GetShortName, Method1, Method2, TypeCache
      UncorrelatedEventCorrelation (class):  UncorrelatedEventCorrelation, Validate
      UncorrelatedEventRegistration (class):  RegisterCorrelation, UncorrelatedEventRegistration
      UncorrelatedFaultEventRegistration (class):  RegisterCorrelation, UncorrelatedFaultEventRegistration
      UnhandledEventBehaviorContext (class):  Ignore, Throw, UnhandledEventBehaviorContext
      UnhandledEventContext (interface):  Ignore, Throw
         The context of an unhandled event in the state machine
      UnhandledEventException (class):  UnhandledEventException
      UnknownEventException (class):  UnknownEventException
      UnknownStateException (class):  UnknownStateException
      UsageTelemetryOptions (class)
      UsageTelemetryOptionsExtensions (class):  ConfigureUsageTelemetryOptions, DisableUsageTelemetry
      ValidationResult (interface)
         Reports information about the configuration before configuring so that corrections can be made without allocating res...
      ValidationResultExtensions (class):  Failure, Success, Warning, WithParentKey
      ValueFactoryException (class):  ValueFactoryException
      Visitable (interface)
         Used to visit the state machine structure, so it can be displayed, etc.
   MassTransit.ActiveMqTransport
      ActiveMqConnectionContext (class):  ActiveMqConnectionContext, CreateSession, DisposeAsync, GetTemporaryQueue, GetTemporaryTopic, IsVirtualTopicConsumer, TryGetTemporaryEntity, TryRemoveTemporaryEntity
      ActiveMqConsumerReceiveEndpointContext (class):  ActiveMqConsumerReceiveEndpointContext, AddConsumeAgent, AddSendAgent, ConvertException, Probe
      ActiveMqDeadLetterTransport (class):  ActiveMqDeadLetterTransport, Send
      ActiveMqErrorTransport (class):  ActiveMqErrorTransport, Send
      ActiveMqHeaderProvider (class):  ActiveMqHeaderProvider, GetAll, TryGetHeader
      ActiveMqHost (class):  ActiveMqHost, ConnectReceiveEndpoint
      ActiveMqHostSettingsExtensions (class):  ToDescription
      ActiveMqMessageBody (class):  ActiveMqMessageBody, GetBytes, GetStream, GetString
      ActiveMqMessageNameFormatter (class):  ActiveMqMessageNameFormatter, GetMessageName
      ActiveMqMoveTransport (class):  CloneMessage
      ActiveMqPublishTransportProvider (class):  ActiveMqPublishTransportProvider, GetPublishTransport
      ActiveMqReceiveContext (class):  ActiveMqReceiveContext, GetTransportProperties
      ActiveMqReceiveEndpointContext (interface)
      ActiveMqReceiveLockContext (class):  ActiveMqReceiveLockContext, Complete, Faulted, ValidateLockStatus
      ActiveMqSendTransportContext (class):  ActiveMqSendTransportContext, CreateSendContext, GetAgentHandles, Probe, Send, SetResponseTo
      ActiveMqSendTransportProvider (class):  ActiveMqSendTransportProvider, GetSendTransport, NormalizeAddress
      ActiveMqSessionContext (class):  ActiveMqSessionContext, CreateBytesMessage, CreateMessage, CreateMessageConsumer, CreateTextMessage, DeleteQueue, DeleteTopic, DisposeAsync, GetDestination, GetQueue, GetTemporaryDestination, GetTopic, SendAsync
      ActiveMqTransportPropertyNames (class)
      ArtemisConsumerEndpointQueueNameFormatter (class):  Format
      CachedMessageProducer (class):  CachedMessageProducer, Close, CloseAsync, CreateBytesMessage, CreateBytesMessageAsync, CreateMapMessage, CreateMapMessageAsync, CreateMessage, CreateMessageAsync, CreateObjectMessage, CreateObjectMessageAsync, CreateStreamMessage, CreateStreamMessageAsync, CreateTextMessage, CreateTextMessageAsync
      ConnectionContext (interface):  CreateSession, GetTemporaryQueue, GetTemporaryTopic, IsVirtualTopicConsumer, TryGetTemporaryEntity, TryRemoveTemporaryEntity
      ConnectionContextFactory (class):  ConnectionContextFactory, CreateActiveContext, CreateConnection, CreateContext, CreateSharedConnection
      ConnectionContextSupervisor (class):  ConnectionContextSupervisor, CreatePublishTransport, CreateSendTransport, NormalizeAddress
      DeadLetterSettings (interface):  GetBrokerTopology
      EntitySettings (interface)
      ErrorSettings (interface):  GetBrokerTopology
      IActiveMqConsumerEndpointQueueNameFormatter (interface):  Format
      IActiveMqHost (interface)
      IConnectionContextSupervisor (interface):  CreatePublishTransport, CreateSendTransport, NormalizeAddress
         Attaches a connection context to the value (shared, of course)
      ISessionContextSupervisor (interface)
         Creates and caches a session on the connection
      MessageProducerCache (class):  GetMessageProducer, GetMessageProducerFromFactory, MessageProducerCache
      NmsExtensions (class):  GetGroupId, GetGroupSequence, SetGroupId, SetGroupSequence, Start, Stop
      PrimitiveMapHeaders (class):  Get, GetAll, GetEnumerator, PrimitiveMapHeaders, Set, TryGetHeader
      ReceiveSendEndpointProvider (class):  ConnectSendObserver, GetSendEndpoint, ReceiveSendEndpointProvider
      ReceiveSettings (interface):  GetInputAddress
         Specify the receive settings for a receive transport
      ReplyToPipe (class):  ReplyToPipe
      ReplyToSendEndpoint (class):  ReplyToSendEndpoint
      ScopeSessionContext (class):  CreateBytesMessage, CreateMessage, CreateMessageConsumer, CreateTextMessage, DeleteQueue, DeleteTopic, GetDestination, GetQueue, GetTemporaryDestination, GetTopic, ScopeSessionContext, SendAsync
      ScopeSessionContextFactory (class):  CreateActiveContext, CreateContext, CreateSession, CreateSharedSession, ScopeSessionContextFactory
      SendSettings (interface):  GetBrokerTopology, GetSendAddress
      SessionContext (interface):  CreateBytesMessage, CreateMessage, CreateMessageConsumer, CreateTextMessage, DeleteQueue, DeleteTopic, GetDestination, GetQueue, GetTemporaryDestination, GetTopic, SendAsync
      SessionContextFactory (class):  CreateActiveContext, CreateContext, CreateSession, CreateSharedSession, SessionContextFactory
      SessionContextSupervisor (class):  SessionContextSupervisor
      SharedConnectionContext (class):  CreateSession, GetTemporaryQueue, GetTemporaryTopic, IsVirtualTopicConsumer, SharedConnectionContext, TryGetTemporaryEntity, TryRemoveTemporaryEntity
      SharedSessionContext (class):  CreateBytesMessage, CreateMessage, CreateMessageConsumer, CreateTextMessage, DeleteQueue, DeleteTopic, GetDestination, GetQueue, GetTemporaryDestination, GetTopic, SendAsync, SharedSessionContext
      TransportActiveMqSendContext (class):  ReadPropertiesFrom, TransportActiveMqSendContext, WritePropertiesTo
      TransportHeaderExtensions (class):  SetHeaders
   MassTransit.ActiveMqTransport.Configuration
      ActiveMqBusConfiguration (class):  ActiveMqBusConfiguration, ConnectBusObserver, ConnectEndpointConfigurationObserver
      ActiveMqBusFactoryConfigurator (class):  ActiveMqBusFactoryConfigurator, CreateBusEndpointConfiguration, EnableArtemisCompatibility, Host, Publish, ReceiveEndpoint, Send, SetConsumerEndpointQueueNameFormatter, SetTemporaryQueueNameFormatter, SetTemporaryQueueNamePrefix, Validate
      ActiveMqEndpointConfiguration (class):  ActiveMqEndpointConfiguration, CreateEndpointConfiguration
      ActiveMqHostConfiguration (class):  ActiveMqHostConfiguration, ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, ReceiveEndpoint
      ActiveMqHostConfigurator (class):  ActiveMqHostConfigurator, EnableAsyncSend, EnableOptimizeAcknowledge, FailoverHosts, Password, SetPrefetchPolicy, SetQueuePrefetchPolicy, TransportOptions, UseSsl, Username
      ActiveMqQueueBindingConfigurator (class)
      ActiveMqQueueConfigurator (class)
      ActiveMqQueueReceiveSettings (class):  ActiveMqQueueReceiveSettings, GetInputAddress
      ActiveMqReceiveEndpointBuilder (class):  ActiveMqReceiveEndpointBuilder, BuildTopology, ConnectConsumePipe, CreateDeadLetterTransport, CreateErrorTransport, CreateReceiveEndpointContext
      ActiveMqReceiveEndpointConfiguration (class):  ActiveMqReceiveEndpointConfiguration, Bind, Build, ConfigureConnection, ConfigureSession, CreateActiveMqReceiveEndpointContext, CreateReceiveEndpointContext, FormatInputAddress, Validate
      ActiveMqRegistrationBusFactory (class):  ActiveMqRegistrationBusFactory, CreateBus
      ActiveMqTopicBindingConfigurator (class):  ActiveMqTopicBindingConfigurator
      ActiveMqTopicConfigurator (class):  ActiveMqTopicConfigurator
      ActiveMqTopologyConfiguration (class):  ActiveMqTopologyConfiguration, Validate
      AmqpHostSettings (class):  AmqpHostSettings
      ConfigurationHostSettings (class):  CreateConnection, FormatBrokerAddress, FormatHostAddress, GetHostName, GetPort, GetQueryString, IsFailoverArgument, ToString
      ConsumerConsumeTopicTopologySpecification (class):  Apply, ConsumerConsumeTopicTopologySpecification, Validate
      ConsumerConsumeTopologySpecification (class):  Apply, ConsumerConsumeTopologySpecification, Validate
         Used to by a Consumer virtual destination to the receive endpoint, via an additional message consumer
      EntityConfigurator (class):  GetEndpointAddress
      IActiveMqBusConfiguration (interface):  CreateEndpointConfiguration
      IActiveMqEndpointConfiguration (interface)
      IActiveMqHostConfiguration (interface):  ApplyEndpointDefinition, CreateReceiveEndpointConfiguration
      IActiveMqReceiveEndpointConfiguration (interface):  Build
      IActiveMqTopologyConfiguration (interface)
      InvalidActiveMqConsumeTopologySpecification (class):  Apply, InvalidActiveMqConsumeTopologySpecification, Validate
      OpenWireHostSettings (class):  OpenWireHostSettings
   MassTransit.ActiveMqTransport.Middleware
      ActiveMqConsumer (class):  ActiveMqConsumer, HandleMessage
         Receives messages from ActiveMQ, pushing them to the InboundPipe of the service endpoint.
      ActiveMqConsumerFilter (class):  ActiveMqConsumerFilter, CreateConsumer, CreateConsumerSupervisor, GetReceiveEntityName, Probe, Send
         A filter that uses the model context to create a basic consumer and connect it to the model
      CombinedDeliveryMetrics (class):  CombinedDeliveryMetrics
      ConfigureActiveMqTopologyFilter (class):  Configure, ConfigureActiveMqTopologyFilter, ConfigureTopology, Declare, Probe, Send
         Configures the broker with the supplied topology once the model is created, to ensure that the exchanges, queues, and...
      ConfigureTopologyContext (interface)
      ConsumerSupervisor (class):  ConsumerSupervisor
      RemoveAutoDeleteAgent (class):  Delete, DeleteAutoDelete, RemoveAutoDeleteAgent
   MassTransit.ActiveMqTransport.Topology
      ActiveMqBrokerTopology (class):  ActiveMqBrokerTopology, Probe
      ActiveMqBusTopology (class):  ActiveMqBusTopology, GetDestinationAddress, GetSendSettings, Publish, Send
      ActiveMqConsumeTopology (class):  ActiveMqConsumeTopology, AddSpecification, Apply, Bind, CreateTemporaryQueueName, GetMessageTopology, Validate
      ActiveMqDeadLetterSettings (class):  ActiveMqDeadLetterSettings, GetBrokerTopology
      ActiveMqEntityNameValidator (class):  IsValidEntityName, ThrowIfInvalidEntityName
      ActiveMqErrorSettings (class):  ActiveMqErrorSettings, GetBrokerTopology
      ActiveMqHostEqualityComparer (class):  Equals, GetHashCode
      ActiveMqMessageConsumeTopology (class):  ActiveMqMessageConsumeTopology, Apply, Bind, Validate
      ActiveMqMessagePublishTopology (class):  ActiveMqMessagePublishTopology, Apply, GetBrokerTopology, GetSendSettings, TryGetPublishAddress
      ActiveMqMessageSendTopology (class)
      ActiveMqPublishTopology (class):  ActiveMqPublishTopology, GetMessageTopology, GetPublishBrokerTopology
      ActiveMqQueueSendSettings (class):  ActiveMqQueueSendSettings, GetBrokerTopology, GetSendAddress, GetSettingStrings, ToString
      ActiveMqSendTopology (class):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      ActiveMqTopicSendSettings (class):  ActiveMqTopicSendSettings, GetBrokerTopology, GetSendAddress, GetSettingStrings, ToString
      BrokerTopology (interface)
      BrokerTopologyBuilder (class):  BindConsumer, CreateQueue, CreateTopic, GetNextId
      Cached (class)
      Consumer (interface)
         The exchange to queue binding details to declare the binding to ActiveMQ
      ConsumerEntity (class):  ConsumerEntity, ToString
      ConsumerEntityEqualityComparer (class):  Equals, GetHashCode
      ConsumerHandle (interface)
      ExchangeEntityEqualityComparer (class):  Equals, GetHashCode
      IActiveMqTemporaryQueueNameFormatter (interface):  Format
      IBrokerTopologyBuilder (interface):  BindConsumer, CreateQueue, CreateTopic
      IMessageExchangeTypeSelector (interface):  GetExchangeType
         Used to select the exchange type for a published message
      IMessageRoutingKeyFormatter (interface):  FormatRoutingKey
      IPublishEndpointBrokerTopologyBuilder (interface):  CreateImplementedBuilder
         A builder for creating the topology when publishing a message
      IReceiveEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so that the items added by it can be combined toge...
      ImplementedBuilder (class):  BindConsumer, CreateImplementedBuilder, CreateQueue, CreateTopic, ImplementedBuilder
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector, ImplementsMessageType
      NameEqualityComparer (class):  Equals, GetHashCode
      PrefixTemporaryQueueNameFormatter (class):  Format, PrefixTemporaryQueueNameFormatter
      PublishEndpointBrokerTopologyBuilder (class):  BuildBrokerTopology, CreateImplementedBuilder, PublishEndpointBrokerTopologyBuilder
      Queue (interface)
         The queue details used to declare the queue to ActiveMQ
      QueueEntity (class):  QueueEntity, ToString
      QueueEntityEqualityComparer (class):  Equals, GetHashCode
      QueueHandle (interface)
      ReceiveEndpointBrokerTopologyBuilder (class):  BuildTopologyLayout
      Topic (interface)
         The exchange details used to declare the exchange to ActiveMQ
      TopicEntity (class):  ToString, TopicEntity
      TopicHandle (interface)
   MassTransit.Agents
      ActivePipeContext (class):  ActivePipeContext, DisposeAsync, Faulted
         An active reference to a pipe context, which is managed by an existing pipe context handle.
      ActivePipeContextAgent (class):  ActivePipeContextAgent, DisposeAsync, Faulted, ToString
         An Agent Provocateur that uses a context handle for the activate state of the agent
      ActivePipeContextHandle (interface):  Faulted
         An active, in-use reference to a pipe context.
      AsyncPipeContextAgent (class):  AsyncPipeContextAgent, CreateCanceled, CreateFaulted, Created, DisposeAsync, Faulted, Stop, ToString
         A PipeContext, which as an agent can be Stopped, which disposes of the context making it unavailable
      AsyncPipeContextFilter (class):  AsyncPipeContextFilter, Probe, Send
         Completes the AsyncPipeContextAgent when the context is sent to the pipe, and doesn't return until the agent completes
      AsyncPipeContextHandle (class):  AsyncPipeContextHandle, CreateCanceled, CreateFaulted, Created, DisposeAsync, Faulted
         An asynchronously pipe context handle, which can be completed.
      AsyncPipeContextPipe (class):  AsyncPipeContextPipe, Probe, Send
         Completes the AsyncPipeContextAgent when the context is sent to the pipe, and doesn't return until the agent completes
      ConstantPipeContextHandle (class):  ConstantPipeContextHandle, DisposeAsync
      IActivePipeContextAgent (interface)
         An active use of a pipe context as an agent.
      IAsyncPipeContextAgent (interface)
      IPipeContextAgent (interface)
      IPipeContextFactory (interface):  CreateActiveContext, CreateContext
         Used to create the actual context, and the active context usages
      PipeContextAgent (class):  DisposeAsync, PipeContextAgent
         A PipeContext, which as an agent can be Stopped, which disposes of the context making it unavailable
      PipeContextSupervisor (class):  ActiveAndActualAgentsCompleted, CreateActiveContext, GetContext, PipeContextSupervisor, Probe, Send
         Maintains a cached context, which is created upon first use, and recreated whenever a fault is propagated to the usage.
   MassTransit.AmazonS3.MessageData
      AmazonS3MessageDataRepository (class):  AmazonS3MessageDataRepository, CreateFaulted, Get, ParseFilePath, PostCreate, PostStart, PostStop, PreStart, PreStop, Put, StartFaulted, StopFaulted
   MassTransit.AmazonSqsTransport
      AmazonSqsClientContext (class):  AmazonSqsClientContext, ChangeMessageVisibility, CreateQueue, CreateQueueSubscription, CreateTopic, DeleteMessage, DeleteQueue, DeleteQueueSubscription, DeleteTopic, GetQueueInfo, Publish, PurgeQueue, ReceiveMessages, SendMessage, SubscriptionAttributesEqual
      AmazonSqsConnectionContext (class):  AmazonSqsConnectionContext, CreateClientContext, DisposeAsync, GetQueue, GetQueueByName, GetTopic, GetTopicByName, RemoveQueueByName, RemoveTopicByName
      AmazonSqsHeaderProvider (class):  AmazonSqsHeaderProvider, GetAll, TryGetHeader
      AmazonSqsHost (class):  AmazonSqsHost, ConnectReceiveEndpoint
      AmazonSqsMessageNameFormatter (class):  AmazonSqsMessageNameFormatter, CreateMessageName, GetMessageName
      AmazonSqsMessageSendContext (class):  AmazonSqsMessageSendContext, ReadPropertiesFrom, WritePropertiesTo
      AmazonSqsPublishTransportProvider (class):  AmazonSqsPublishTransportProvider, GetPublishTransport
      AmazonSqsReceiveContext (class):  AmazonSqsReceiveContext, GetTransportProperties
      AmazonSqsReceiveLockContext (class):  AmazonSqsReceiveLockContext, Complete, Faulted, RenewMessageVisibility, ValidateLockStatus
      AmazonSqsSendTransportProvider (class):  AmazonSqsSendTransportProvider, GetSendTransport, NormalizeAddress
      AmazonSqsTransportPropertyNames (class)
      AmazonWebServiceResponseExtensions (class):  EnsureSuccessfulResponse
      BatchEntry (class):  BatchEntry, SetCompleted, SetFaulted
      BatchSettings (interface)
      Batcher (class):  DisposeAsync, Execute, ExecuteBatch, ReadBatch, WaitForBatch
      ClientBatchSettings (class):  ClientBatchSettings
      ClientContext (interface):  ChangeMessageVisibility, CreateQueue, CreateQueueSubscription, CreateTopic, DeleteMessage, DeleteQueue, DeleteTopic, GetQueueInfo, Publish, PurgeQueue, ReceiveMessages, SendMessage
      ClientContextBatchSettings (class):  ClientContextBatchSettings, GetBatchSettings
      ClientContextCacheDefaults (class):  ClientContextCacheDefaults, CreateCache
      ClientContextFactory (class):  ClientContextFactory, CreateActiveContext, CreateClientContext, CreateContext, CreateSharedClientContext
      ClientContextSupervisor (class):  ClientContextSupervisor
      Connection (class):  Connection, Dispose
      ConnectionContext (interface):  CreateClientContext, GetQueue, GetQueueByName, GetTopic, GetTopicByName, RemoveQueueByName, RemoveTopicByName
      ConnectionContextFactory (class):  ConnectionContextFactory, CreateActiveContext, CreateConnection, CreateContext, CreateSharedConnection
      ConnectionContextSupervisor (class):  ConnectionContextSupervisor, CreatePublishTransport, CreateSendTransport, CreateTransport, NormalizeAddress
      DeadLetterSettings (interface):  GetBrokerTopology
      DeleteBatcher (class):  DeleteBatcher
      EntitySettings (interface)
      ErrorSettings (interface):  GetBrokerTopology
      IAmazonSqsHost (interface)
      IBatcher (interface):  Execute
      IClientContextSupervisor (interface)
         Creates and caches a model on the connection
      IConnection (interface)
      IConnectionContextSupervisor (interface):  CreatePublishTransport, CreateSendTransport, NormalizeAddress
      PublishBatchSettings (class):  GetBatchSettings, PublishBatchSettings
      PublishBatcher (class):  PublishBatcher
      PublishSettings (interface)
      QueueCache (class):  CreateMissingQueue, DisposeAsync, Get, GetByName, GetExistingQueue, QueueCache, RemoveByName
      QueueInfo (class):  Delete, DisposeAsync, QueueHasTopicPermission, QueueInfo, Send, UpdatePolicy
      QueueSendTransportContext (class):  CreateSendContext, GetAgentHandles, Probe, QueueSendTransportContext, Send
      QueueSqsReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConvertException, Probe, QueueSqsReceiveEndpointContext
      ReceiveSettings (interface):  GetInputAddress
         Specify the receive settings for a receive transport
      ScopeClientContext (class):  ChangeMessageVisibility, CreateQueue, CreateQueueSubscription, CreateTopic, DeleteMessage, DeleteQueue, DeleteTopic, Dispose, GetQueueInfo, Publish, PurgeQueue, ReceiveMessages, ScopeClientContext, SendMessage
      ScopeClientContextFactory (class):  CreateActiveContext, CreateClientContext, CreateContext, CreateSharedModel, ScopeClientContextFactory
      SendBatcher (class):  SendBatcher
      SendSettings (interface):  GetBrokerTopology
      SharedClientContext (class):  ChangeMessageVisibility, CreateQueue, CreateQueueSubscription, CreateTopic, DeleteMessage, DeleteQueue, DeleteTopic, GetQueueInfo, Publish, PurgeQueue, ReceiveMessages, SendMessage, SharedClientContext
      SharedConnectionContext (class):  CreateClientContext, GetQueue, GetQueueByName, GetTopic, GetTopicByName, RemoveQueueByName, RemoveTopicByName, SharedConnectionContext
      SnsHeaderValueConverter (class):  AlwaysCopy, CreateMessageAttributeValue, SnsHeaderValueConverter, TryConvert
      SnsPublishBatchSettings (class):  SnsPublishBatchSettings
      SqsDeadLetterTransport (class):  Send, SqsDeadLetterTransport
      SqsErrorTransport (class):  Send, SqsErrorTransport
      SqsHeaderValueConverter (class):  AlwaysCopy, CreateMessageAttributeValue, SqsHeaderValueConverter, TryConvert
      SqsMessageBody (class):  GetJsonElement, SqsMessageBody
      SqsMoveTransport (class):  CopyReceivedMessageHeaders
      SqsReceiveEndpointContext (interface)
      TopicCache (class):  Clear, CreateMissingTopic, DisposeAsync, Get, GetByName, LoadExistingTopics, LoadExistingTopicsLazy, RemoveByName, ResetLoadExistingTopics, TopicCache
      TopicInfo (class):  DisposeAsync, Publish, TopicInfo
      TopicSendTransportContext (class):  CreateSendContext, GetAgentHandles, Probe, Send, TopicSendTransportContext
   MassTransit.AmazonSqsTransport.Configuration
      AmazonSqsBusConfiguration (class):  AmazonSqsBusConfiguration, ConnectBusObserver, ConnectEndpointConfigurationObserver
      AmazonSqsBusFactoryConfigurator (class):  AmazonSqsBusFactoryConfigurator, CreateBusEndpointConfiguration, Host, OverrideDefaultBusEndpointQueueName, Publish, ReceiveEndpoint, Send, Validate
      AmazonSqsEndpointConfiguration (class):  AmazonSqsEndpointConfiguration, CreateEndpointConfiguration
      AmazonSqsHostConfiguration (class):  AmazonSqsHostConfiguration, ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, ReceiveEndpoint
      AmazonSqsHostConfigurator (class):  AccessKey, AllowTransportHeader, AmazonSqsHostConfigurator, Config, Credentials, EnableScopedTopics, Scope, SecretKey, SetBasicCredentials
      AmazonSqsQueueConfigurator (class):  AmazonSqsQueueConfigurator
      AmazonSqsQueueSubscriptionConfigurator (class)
      AmazonSqsReceiveEndpointBuilder (class):  AmazonSqsReceiveEndpointBuilder, BuildTopology, ConnectConsumePipe, CreateDeadLetterTransport, CreateErrorTransport, CreateReceiveEndpointContext
      AmazonSqsReceiveEndpointConfiguration (class):  AmazonSqsReceiveEndpointConfiguration, Build, ConfigureClient, ConfigureConnection, CreateReceiveEndpointContext, CreateSqsReceiveEndpointContext, DisableMessageOrdering, FormatInputAddress, Subscribe, Validate
      AmazonSqsRegistrationBusFactory (class):  AmazonSqsRegistrationBusFactory, CreateBus
      AmazonSqsTopicConfigurator (class):  AmazonSqsTopicConfigurator
      AmazonSqsTopicSubscriptionConfigurator (class):  AmazonSqsTopicSubscriptionConfigurator
      AmazonSqsTopologyConfiguration (class):  AmazonSqsTopologyConfiguration, Validate
      ConfigurationHostSettings (class):  ConfigurationHostSettings, CreateConnection, FormatHostAddress, GetImmutableCredentials, ToString
      ConsumerConsumeTopologySpecification (class):  Apply, ConsumerConsumeTopologySpecification, Validate
         Used to by a TopicSubscription destination to the receive endpoint, via an additional message consumer
      EntityConfigurator (class):  GetEndpointAddress
      IAmazonSqsBusConfiguration (interface):  CreateEndpointConfiguration
      IAmazonSqsEndpointConfiguration (interface)
      IAmazonSqsHostConfiguration (interface):  ApplyEndpointDefinition, CreateReceiveEndpointConfiguration
      IAmazonSqsReceiveEndpointConfiguration (interface):  Build
      IAmazonSqsTopologyConfiguration (interface)
      InvalidAmazonSqsConsumeTopologySpecification (class):  Apply, InvalidAmazonSqsConsumeTopologySpecification, Validate
   MassTransit.AmazonSqsTransport.Middleware
      AmazonSqsConsumerFilter (class):  AmazonSqsConsumerFilter, Probe, Send
         A filter that uses the model context to create a basic consumer and connect it to the model
      AmazonSqsMessageReceiver (class):  AmazonSqsMessageReceiver, Consume, GetQueueAttributes, HandleMessage, ReceiveMessages
         Receives messages from AmazonSQS, pushing them to the InboundPipe of the service endpoint.
      ConfigureAmazonSqsTopologyFilter (class):  AnyAutoDelete, Configure, ConfigureAmazonSqsTopologyFilter, ConfigureTopology, Declare, Probe, Send
         Configures the broker with the supplied topology once the model is created, to ensure that the exchanges, queues, and...
      ConfigureTopologyContext (interface)
      FifoChannelExecutorPool (class):  DisposeAsync, FifoChannelExecutorPool, MessageGroupIdProvider, Push, Run
      PurgeOnStartupFilter (class):  Probe, PurgeIfRequested, PurgeOnStartupFilter, Send
         Purges the queue on startup, only once per filter instance
      RemoveAmazonSqsTopologyAgent (class):  Delete, DeleteAutoDelete, RemoveAmazonSqsTopologyAgent
   MassTransit.AmazonSqsTransport.Topology
      AmazonSnsTopicNameValidator (class):  IsValidEntityName, ThrowIfInvalidEntityName
      AmazonSqsBrokerTopology (class):  AmazonSqsBrokerTopology, Probe
      AmazonSqsBusTopology (class):  AmazonSqsBusTopology, GetDestinationAddress, GetSendSettings, Publish, Send
      AmazonSqsConsumeTopology (class):  AddSpecification, AmazonSqsConsumeTopology, Apply, Bind, GetMessageTopology, Validate
      AmazonSqsEntityNameValidator (class):  IsValidEntityName, ThrowIfInvalidEntityName
      AmazonSqsHostEqualityComparer (class):  Equals, GetHashCode
      AmazonSqsMessageConsumeTopology (class):  AmazonSqsMessageConsumeTopology, Apply, Subscribe, Validate
      AmazonSqsMessagePublishTopology (class):  AmazonSqsMessagePublishTopology, Apply, GetBrokerTopology, GetEndpointAddress, GetPublishSettings, TryGetPublishAddress
      AmazonSqsMessageSendTopology (class)
      AmazonSqsPublishTopology (class):  AmazonSqsPublishTopology, GetMessageTopology, GetPublishBrokerTopology
      AmazonSqsSendTopology (class):  AmazonSqsSendTopology, BuildEntityName, GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      BrokerTopology (interface)
      BrokerTopologyBuilder (class):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, GetNextId
      Cached (class)
      ConsumerEntityEqualityComparer (class):  Equals, GetHashCode
      IBrokerTopologyBuilder (interface):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription
      IPublishEndpointBrokerTopologyBuilder (interface)
         A builder for creating the topology when publishing a message
      IReceiveEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so that the items added by it can be combined toge...
      ISendEndpointBrokerTopologyBuilder (interface)
         A builder for creating the topology when publishing a message
      NameEqualityComparer (class):  Equals, GetHashCode
      PublishEndpointBrokerTopologyBuilder (class):  BuildBrokerTopology
      Queue (interface)
         The queue details used to declare the queue to AmazonSQS
      QueueDeadLetterSettings (class):  GetBrokerTopology, QueueDeadLetterSettings
      QueueEntity (class):  QueueEntity, ToString
      QueueEntityEqualityComparer (class):  Equals, GetHashCode
      QueueErrorSettings (class):  GetBrokerTopology, QueueErrorSettings
      QueueHandle (interface)
      QueueReceiveSettings (class):  GetInputAddress, QueueReceiveSettings
      QueueSendSettings (class):  GetBrokerTopology, GetSettingStrings, QueueSendSettings, ToString
      QueueSubscription (interface)
         The topic to queue binding details to declare the binding to AmazonSQS
      QueueSubscriptionEntity (class):  QueueSubscriptionEntity, ToString
      QueueSubscriptionHandle (interface)
      ReceiveEndpointBrokerTopologyBuilder (class):  BuildTopologyLayout
      SendEndpointBrokerTopologyBuilder (class):  BuildBrokerTopology
      Topic (interface)
         The exchange details used to declare the exchange to AmazonSQS
      TopicEntity (class):  EnsureRawDeliveryIsSet, ToString, TopicEntity
      TopicEntityEqualityComparer (class):  Equals, GetHashCode
      TopicHandle (interface)
      TopicPublishSettings (class):  GetSendAddress, GetSettingStrings, ToString, TopicPublishSettings
      TopicSubscription (interface)
         The topic to queue binding details to declare the binding to AmazonSQS
      TopicSubscriptionEntity (class):  ToString, TopicSubscriptionEntity
      TopicSubscriptionHandle (interface)
   MassTransit.Analyzers
      CommonExpressions (class):  GetAllInterfaces, GetAllTypes, GetContractProperties, HasMessageContract, ImplementsInterface, ImplementsType, InheritsFromType, InitializeProducerMethods, IsActivator, IsArray, IsClassOrInterface, IsCollection, IsDictionary, IsEnumerable, IsImmutableArray
      PropertyNameEqualityComparer (class):  Equals, GetHashCode
   MassTransit.Analyzers.Helpers
      NodeList (class):  Add, Contains, Index, NodeList
      NodeListExtensions (class):  Add
      NodeTable (class):  NodeTable
      OperationExtensions (class):  GetParameters, GetReceiverType, IsStaticMember
      TypeConversionHelper (class):  CanConvert, IsMessageData, IsTask, TypeConversionHelper
   MassTransit.Audit
      IConsumeMetadataFactory (interface):  CreateAuditMetadata
      IMessageAuditStore (interface):  StoreMessage
         Used to store message audits that are observed
      ISendMetadataFactory (interface):  CreateAuditMetadata
      MessageAuditMetadata (class)
   MassTransit.Audit.MetadataFactories
      DefaultConsumeMetadataFactory (class):  CreateAuditMetadata, CreateMetadata
      DefaultSendMetadataFactory (class):  CreateAuditMetadata, CreateMetadata
   MassTransit.Audit.Observers
      AuditConsumeObserver (class):  AuditConsumeObserver, ConsumeFault, PostConsume, PreConsume
      AuditPublishObserver (class):  AuditPublishObserver, PostPublish, PrePublish, PublishFault
      AuditSendObserver (class):  AuditSendObserver, PostSend, PreSend, SendFault
   MassTransit.AzureCosmos
      AzureCosmosSerializerExtensions (class):  GetSagaRenameSettings, GetSerializerOptions
      CosmosAuthSettings (class):  CosmosAuthSettings
      CosmosClientFactory (class):  CreateClient
      NewtonsoftJsonCosmosClientFactory (class):  CreateClient, Dispose, GetCosmosClient, GetSerializerSettingsIfNeeded, NewtonsoftJsonCosmosClientFactory
         Creates a single instance of the using a fixed endpoint and key for all clients, regardless of client name.
      SagaRenamePropertyNamingPolicy (class):  ConvertName, SagaRenamePropertyNamingPolicy
      SystemTextJsonCosmosClientFactory (class):  CreateClient, Dispose, GetCosmosClient, GetSerializerOptions, SystemTextJsonCosmosClientFactory
         Creates a single instance of the using a fixed endpoint and key for all clients, regardless of client name.
   MassTransit.AzureCosmos.Saga
      CosmosDatabaseContext (class):  CosmosDatabaseContext, GetItemRequestOptions, GetLinqSerializerOptions
      CosmosSagaRepository (class):  Create
      CosmosSagaRepositoryContext (class):  Add, CosmosSagaRepositoryContext, CreateSagaConsumeContext, Delete, Discard, GetItemRequestOptions, Insert, Load, Query, Save, Undo, Update
      CosmosSagaRepositoryContextFactory (class):  CosmosSagaRepositoryContextFactory, Execute, ExecuteAsyncMethod, Probe, Send, SendQuery
      DatabaseContext (interface):  GetItemRequestOptions, GetLinqSerializerOptions
      DefaultCollectionIdFormatter (class):  DefaultCollectionIdFormatter, Saga
      KebabCaseCollectionIdFormatter (class):  KebabCaseCollectionIdFormatter, Saga
         Formats the saga collection names using kebab case naming SimpleSaga -> simple-saga
      NewtonsoftJsonCosmosSerializer (class):  FromStream, NewtonsoftJsonCosmosSerializer, ToStream
         The default Cosmos JSON.NET serializer.
      PropertyRenameSerializerContractResolver (class):  IsRenamed, PropertyRenameSerializerContractResolver, RenameProperty
      QueryableExtensions (class):  QueryAsync
      SagaETag (class):  SagaETag
      SystemTextJsonCosmosSerializer (class):  FromStream, SystemTextJsonCosmosSerializer, ToStream
         The default serializer, using System.Text.Json
   MassTransit.AzureServiceBusTransport
      AzureServiceBusSendContext (class):  AzureServiceBusSendContext, ReadPropertiesFrom, SetScheduledMessageId, TryGetScheduledMessageId, TryGetSequenceNumber, WritePropertiesTo
      AzureServiceBusTransportPropertyNames (class)
      ClientContext (interface):  CloseAsync, NotifyFaulted, OnMessageAsync, OnSessionAsync, ShutdownAsync, StartAsync
         The client context is used to access the queue/subscription/topic client.
      ClientContextFactory (class):  CreateActiveContext, CreateClientContext, CreateContext, CreateSharedContext
      ClientContextSupervisor (class):  ClientContextSupervisor
      ClientSettings (interface):  GetInputAddress
      ConnectionContext (interface):  CreateMessageSender, CreateQueue, CreateQueueProcessor, CreateQueueSessionProcessor, CreateSubscriptionProcessor, CreateSubscriptionSessionProcessor, CreateTopic, CreateTopicSubscription, DeleteTopicSubscription
         Service Bus Connection Context
      ConnectionContextFactory (class):  ConnectionContextFactory, CreateActiveContext, CreateConnection, CreateContext, CreateSharedConnection, HasSharedAccess
      ConnectionContextSupervisor (class):  ConnectionContextSupervisor, CreateClientContextSupervisor, CreatePublishTransport, CreateSendEndpointContextSupervisor, CreateSendTransport, NormalizeAddress
      Defaults (class):  GetCreateQueueOptions, GetCreateTopicOptions
      DelegateSessionIdFormatter (class):  DelegateSessionIdFormatter, FormatSessionId
      EmptySessionIdFormatter (class):  FormatSessionId
      IClientContextSupervisor (interface)
      IConnectionContextSupervisor (interface):  CreateClientContextSupervisor, CreatePublishTransport, CreateSendEndpointContextSupervisor, CreateSendTransport, NormalizeAddress
      IMessageSessionIdFormatter (interface):  FormatSessionId
      IReceiver (interface):  Start
      ISendEndpointContextSupervisor (interface)
      IServiceBusHost (interface):  ConnectSubscriptionEndpoint
         An Azure ServiceBus Host, which caches the messaging factory and namespace manager
      IServiceBusMessageReceiver (interface):  Handle
      ISessionIdFormatter (interface):  FormatSessionId
      MessageLockContext (interface):  Abandon, Complete, DeadLetter
      MessageReceiver (class):  CreateMessageReceiver, Dispose, Handle, HandleConsumer, HandleExecuteActivity, HandleSaga, MessageReceiver
      MessageSendEndpointContext (class):  CancelScheduledSend, MessageSendEndpointContext, ScheduleSend, Send
      MessageSessionContext (interface):  GetStateAsync, RenewLockAsync, SetStateAsync
         A context for a message consumed within a message session
      MessageSessionIdFormatter (class):  FormatSessionId, MessageSessionIdFormatter
      MessageSessionSagaRepository (class):  Create
      MessageSessionSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, Insert, Load, MessageSessionSagaRepositoryContext, ReadSagaState, Save, Undo, Update, WriteSagaState
      MessageSessionSagaRepositoryContextFactory (class):  MessageSessionSagaRepositoryContextFactory, Probe, Send, SendQuery
      NamespaceContext (interface)
         A service bus namespace which has the appropriate messaging factories available
      QueueClientContext (class):  CloseAsync, DisposeAsync, NotifyFaulted, OnMessageAsync, OnSessionAsync, QueueClientContext, ShutdownAsync, StartAsync
      QueueClientContextFactory (class):  QueueClientContextFactory
      ReceiveSettings (interface):  GetCreateQueueOptions
      Receiver (class):  OnMessage, Receiver, Start
      ScheduledMessageToken (class):  ScheduledMessageToken
      SendEndpointContext (interface):  CancelScheduledSend, ScheduleSend, Send
      SendEndpointContextFactory (class):  CreateActiveContext, CreateContext, CreateSendEndpointContext, CreateSharedContext, SendEndpointContextFactory
      SendEndpointContextSupervisor (class):  SendEndpointContextSupervisor
      SendSettings (interface):  GetBrokerTopology
         The settings for sending to an endpoint
      ServiceBusConnectionContext (class):  CreateMessageSender, CreateQueue, CreateQueueAsync, CreateQueueProcessor, CreateQueueSessionProcessor, CreateSubscriptionAsync, CreateSubscriptionProcessor, CreateSubscriptionSessionProcessor, CreateTopic, CreateTopicAsync, CreateTopicSubscription, DeleteSubscriptionAsync, DeleteTopicSubscription, DisposeAsync, GetProcessorOptions
      ServiceBusEntityReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConvertException, Probe, ServiceBusEntityReceiveEndpointContext
      ServiceBusHeaderProvider (class):  GetAll, ServiceBusHeaderProvider, TryGetHeader
      ServiceBusHost (class):  ConnectReceiveEndpoint, ConnectSubscriptionEndpoint, ServiceBusHost
      ServiceBusInstance (class):  ConnectSubscriptionEndpoint, ServiceBusInstance
      ServiceBusMessageBody (class):  GetBytes, GetStream, GetString, ServiceBusMessageBody
      ServiceBusMessageLockContext (class):  Abandon, Complete, DeadLetter, ServiceBusMessageLockContext
      ServiceBusMessageNameFormatter (class):  GetMessageName, ServiceBusMessageNameFormatter
      ServiceBusMessageReceiver (class):  Handle, ServiceBusMessageReceiver
      ServiceBusMessageSessionContext (class):  GetStateAsync, RenewLockAsync, ServiceBusMessageSessionContext, SetStateAsync
      ServiceBusPublishTransportProvider (class):  GetPublishTransport, ServiceBusPublishTransportProvider
      ServiceBusQueueDeadLetterTransport (class):  Send, ServiceBusQueueDeadLetterTransport
      ServiceBusQueueErrorTransport (class):  Send, ServiceBusQueueErrorTransport
      ServiceBusQueueMoveTransport (class)
      ServiceBusReceiveContext (class):  GetTransportProperties, ServiceBusReceiveContext
      ServiceBusReceiveEndpointContext (interface)
      ServiceBusReceiveLockContext (class):  Complete, Faulted, ServiceBusReceiveLockContext, ValidateLockStatus
      ServiceBusRegistrationBusFactory (class):  CreateBus, ServiceBusRegistrationBusFactory
      ServiceBusSendTransportContext (class):  CancelScheduledSend, CopyIncomingIdentifiersIfPresent, CreateMessage, CreateSendContext, GetAgentHandles, IsCancelScheduledSend, Probe, ScheduleSend, Send, ServiceBusSendTransportContext
      ServiceBusSendTransportProvider (class):  GetSendTransport, NormalizeAddress, ServiceBusSendTransportProvider
      ServiceBusSessionMessageLockContext (class):  Abandon, Complete, DeadLetter, ServiceBusSessionMessageLockContext
      SessionReceiver (class):  OnSession, SessionReceiver, Start
      SharedClientContext (class):  CloseAsync, NotifyFaulted, OnMessageAsync, OnSessionAsync, SharedClientContext, ShutdownAsync, StartAsync
      SharedConnectionContext (class):  CreateMessageSender, CreateQueue, CreateQueueProcessor, CreateQueueSessionProcessor, CreateSubscriptionProcessor, CreateSubscriptionSessionProcessor, CreateTopic, CreateTopicSubscription, DeleteTopicSubscription, SharedConnectionContext
      SharedSendEndpointContext (class):  CancelScheduledSend, ScheduleSend, Send, SharedSendEndpointContext
      StreamExtensions (class):  ReadAsBytes
      SubscriptionClientContext (class):  CloseAsync, DisposeAsync, NotifyFaulted, OnMessageAsync, OnSessionAsync, ShutdownAsync, StartAsync, SubscriptionClientContext
      SubscriptionClientContextFactory (class):  SubscriptionClientContextFactory
      SubscriptionSettings (interface)
   MassTransit.AzureServiceBusTransport.Configuration
      Factory (class):  Create
      HostSettings (class):  HostSettings
      IServiceBusBusConfiguration (interface):  CreateEndpointConfiguration
      IServiceBusConsumeTopologySpecification (interface):  Apply
      IServiceBusEndpointConfiguration (interface)
      IServiceBusEntityEndpointConfiguration (interface):  Build
      IServiceBusHostConfiguration (interface):  ApplyEndpointDefinition, CreateReceiveEndpointConfiguration, CreateSubscriptionEndpointConfiguration, SetNamespaceSeparatorTo, SetNamespaceSeparatorToTilde, SetNamespaceSeparatorToUnderscore, SubscriptionEndpoint
      IServiceBusReceiveEndpointConfiguration (interface)
      IServiceBusSubscriptionEndpointConfiguration (interface)
      IServiceBusTopologyConfiguration (interface)
      ISessionIdMessageSendTopologyConvention (interface):  SetFormatter
      ISessionIdSendTopologyConvention (interface)
      InvalidServiceBusConsumeTopologySpecification (class):  Apply, InvalidServiceBusConsumeTopologySpecification, Validate
      NamedKeyTokenProviderConfigurator (class)
      QueueBrokeredMessageReceiverConfiguration (class):  Build, QueueBrokeredMessageReceiverConfiguration
      ServiceBusBusConfiguration (class):  ConnectBusObserver, ConnectEndpointConfigurationObserver, ServiceBusBusConfiguration
      ServiceBusEndpointConfiguration (class):  CreateEndpointConfiguration, ServiceBusEndpointConfiguration
      ServiceBusEndpointEntityConfigurator (class)
      ServiceBusEntityConfigurator (class)
      ServiceBusEntityReceiveEndpointConfiguration (class):  Validate, ValidateSettings
      ServiceBusHostConfiguration (class):  ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, CreateSubscriptionEndpointConfiguration, ReceiveEndpoint, ServiceBusHostConfiguration, SetNamespaceSeparatorTo, SetNamespaceSeparatorToTilde, SetNamespaceSeparatorToUnderscore, SubscriptionEndpoint
      ServiceBusMessageEntityConfigurator (class):  EnableDuplicateDetection
      ServiceBusQueueConfigurator (class):  GetCreateQueueOptions, GetQueueAddress, ServiceBusQueueConfigurator, Validate
      ServiceBusReceiveEndpointConfiguration (class):  Build, CreateDeadLetterTransport, CreateErrorTransport, CreateReceiveEndpointContext, CreateServiceBusReceiveEndpointContext, EnableDuplicateDetection, FormatInputAddress, ServiceBusReceiveEndpointConfiguration, Subscribe, Validate
      ServiceBusSubscriptionConfigurator (class):  GetCreateSubscriptionOptions, ServiceBusSubscriptionConfigurator, Validate
      ServiceBusSubscriptionEndpointConfiguration (class):  Build, CreateReceiveEndpointContext, CreateServiceBusReceiveEndpointContext, FormatInputAddress, ServiceBusSubscriptionEndpointConfiguration, Validate
      ServiceBusTopicConfigurator (class):  GetCreateTopicOptions, ServiceBusTopicConfigurator, Validate
      ServiceBusTopologyConfiguration (class):  ServiceBusTopologyConfiguration, Validate
      SessionIdMessageSendTopologyConvention (class):  SessionIdMessageSendTopologyConvention, SetFormatter, TryGetMessageSendTopology, TryGetMessageSendTopologyConvention
      SessionIdSendTopologyConvention (class):  SessionIdSendTopologyConvention, TryGetMessageSendTopologyConvention
      SharedAccessSignatureTokenProviderConfigurator (class)
      SubscriptionBrokeredMessageReceiverConfiguration (class):  Build, SubscriptionBrokeredMessageReceiverConfiguration
      SubscriptionConsumeTopologySpecification (class):  Apply, SubscriptionConsumeTopologySpecification, Validate
         Used to bind an exchange to the consuming queue's exchange
   MassTransit.AzureServiceBusTransport.Middleware
      ConfigureServiceBusTopologyFilter (class):  Configure, ConfigureServiceBusTopologyFilter, ConfigureTopology, Create, Delete, Probe, Send
      ConfigureTopologyContext (interface)
      DeadLetterQueueExceptionFilter (class):  Probe, Send
         Moves a faulted message to the dead-letter queue, rather than the _error queue
      DeadLetterQueueFilter (class):  Probe, Send
         Moves a message to the dead-letter queue, rather than the _skipped queue
      MessageReceiverFilter (class):  MessageReceiverFilter, Probe, Send
         Creates a message receiver and receives messages from the input queue of the endpoint
      MessageSessionReceiverFilter (class):  MessageSessionReceiverFilter, Probe
         Creates a message session receiver
      RemoveServiceBusTopologyAgent (class):  Delete, RemoveServiceBusTopologyAgent, RemoveSubscriptions
      ServiceBusMessageSchedulerFilter (class):  Probe, SchedulerFactory, Send
         Adds the service bus message scheduler filter
      ServiceBusSendContextFilter (class):  Probe, Send, ServiceBusSendContextFilter
      SetSessionIdFilter (class):  Probe, Send, SetSessionIdFilter
   MassTransit.AzureServiceBusTransport.Topology
      BaseClientSettings (class):  GetInputAddress
      BrokerTopology (interface)
      BrokerTopologyBuilder (class):  BrokerTopologyBuilder, BuildBrokerTopology, CreateQueue, CreateQueueSubscription, CreateSubscription, CreateTopic, CreateTopicSubscription, GetNextId
      Cached (class)
      EntityNameFormatter (class):  FormatDeadLetterPath, FormatErrorPath, FormatSubQueuePath, FormatSubscriptionPath
      IBrokerTopologyBuilder (interface):  CreateQueue, CreateQueueSubscription, CreateSubscription, CreateTopic, CreateTopicSubscription
      IPublishEndpointBrokerTopologyBuilder (interface):  CreateImplementedBuilder
         A builder for creating the topology when publishing a message
      IReceiveEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so that the items added by it can be combined toge...
      ISendEndpointBrokerTopologyBuilder (interface)
         A builder for creating the topology when publishing a message
      ISubscriptionEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so that the items added by it can be combined toge...
      ImplementedBuilder (class):  CreateImplementedBuilder, CreateQueue, CreateQueueSubscription, CreateSubscription, CreateTopic, CreateTopicSubscription, ImplementedBuilder
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector, ImplementsMessageType
      ImplementedTypeAdapter (class):  Apply, ImplementedTypeAdapter
      NameEqualityComparer (class):  Equals, GetHashCode
      PublishEndpointBrokerTopologyBuilder (class):  CreateImplementedBuilder, PublishEndpointBrokerTopologyBuilder
      Queue (interface)
         The queue details used to declare the queue to Azure Service Bus
      QueueEntity (class):  QueueEntity, ToString
      QueueEntityEqualityComparer (class):  Equals, GetHashCode
      QueueHandle (interface)
      QueueSendSettings (class):  GetBrokerTopology, QueueSendSettings
      QueueSubscription (interface)
         The exchange to queue binding details to declare the binding to RabbitMQ
      QueueSubscriptionEntity (class):  QueueSubscriptionEntity, ToString
      QueueSubscriptionEntityEqualityComparer (class):  Equals, GetHashCode
      QueueSubscriptionHandle (interface)
      ReceiveEndpointBrokerTopologyBuilder (class)
      ReceiveEndpointSettings (class):  GetCreateQueueOptions, ReceiveEndpointSettings
      SendEndpointBrokerTopologyBuilder (class)
      ServiceBusBrokerTopology (class):  GetSubscriptionValues, Probe, ServiceBusBrokerTopology
      ServiceBusBusTopology (class):  Publish, Send, ServiceBusBusTopology
      ServiceBusConsumeTopology (class):  AddSpecification, Apply, GetMessageTopology, ServiceBusConsumeTopology, Subscribe, Validate
      ServiceBusEntityNameValidator (class):  IsValidEntityName, ThrowIfInvalidEntityName
      ServiceBusMessageConsumeTopology (class):  Apply, ServiceBusMessageConsumeTopology, Subscribe, Validate
      ServiceBusMessagePublishTopology (class):  AddImplementedMessageConfigurator, Apply, EnableDuplicateDetection, GetSendSettings, GetSubscriptionConfigurator, ServiceBusMessagePublishTopology, TryGetPublishAddress
      ServiceBusMessageSendTopology (class)
      ServiceBusPublishTopology (class):  FormatSubscriptionName, GenerateSubscriptionName, GetMessageTopology, GetPublishBrokerTopology, ServiceBusPublishTopology
      ServiceBusSendTopology (class):  GetCreateQueueOptions, GetCreateTopicOptions, GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      ServiceBusSubscriptionNameValidator (class):  IsValidEntityName, ThrowIfInvalidEntityName
      SetSessionIdMessageSendTopology (class):  Apply, SetSessionIdMessageSendTopology
      Subscription (interface)
         A subscription, as defined
      SubscriptionEndpointBrokerTopologyBuilder (class)
      SubscriptionEndpointSettings (class):  SubscriptionEndpointSettings
      SubscriptionEntity (class):  SubscriptionEntity, ToString
      SubscriptionEntityEqualityComparer (class):  Equals, GetHashCode
      SubscriptionHandle (interface)
      Topic (interface)
         The exchange details used to declare the exchange to Azure Service Bus
      TopicEntity (class):  ToString, TopicEntity
      TopicEntityEqualityComparer (class):  Equals, GetHashCode
      TopicHandle (interface)
      TopicSendSettings (class):  GetBrokerTopology, TopicSendSettings
      TopicSubscription (interface)
         A subscription that forwards to another topic
      TopicSubscriptionEntity (class):  ToString, TopicSubscriptionEntity
      TopicSubscriptionEntityEqualityComparer (class):  Equals, GetHashCode
      TopicSubscriptionHandle (interface)
   MassTransit.AzureStorage.MessageData
      AzureStorageMessageDataRepository (class):  AzureStorageMessageDataRepository, CreateFaulted, Get, PostCreate, PostStart, PostStop, PreStart, PreStop, Put, SetBlobExpiration, StartFaulted, StopFaulted
      IBlobNameGenerator (interface):  GenerateBlobName
      NewIdBlobNameGenerator (class):  GenerateBlobName
   MassTransit.AzureTable
      AuditRecord (class):  AuditRecord, SanitizePartitionKey
      AzureTableAuditStore (class):  AzureTableAuditStore, StoreMessage
      DatabaseContext (interface)
      DefaultPartitionKeyFormatter (class):  Format
      ICloudTableProvider (interface):  GetCloudTable
      IEntityConverter (interface):  GetDictionary, GetObject
      IPartitionKeyFormatter (interface):  Format
      ISagaKeyFormatter (interface):  Format
   MassTransit.AzureTable.Saga
      AzureTableDatabaseContext (class):  AzureTableDatabaseContext
      AzureTableSagaRepository (class):  Create
      AzureTableSagaRepositoryContext (class):  Add, AzureTableSagaRepositoryContext, CreateSagaConsumeContext, Delete, Discard, Insert, Load, Save, TableInsert, Undo, Update
      AzureTableSagaRepositoryContextFactory (class):  AzureTableSagaRepositoryContextFactory, Execute, Probe, Send, SendQuery
      ConstCloudTableProvider (class):  ConstCloudTableProvider, GetCloudTable
      ConstPartitionSagaKeyFormatter (class):  ConstPartitionSagaKeyFormatter, Format
      ConstRowSagaKeyFormatter (class):  ConstRowSagaKeyFormatter, Format
      CosmosTableSagaRepositoryContext (class):  CosmosTableSagaRepositoryContext, Load
      DelegateCloudTableProvider (class):  DelegateCloudTableProvider, GetCloudTable
      EntityConverter (class):  EntityConverter, GetDictionary, GetObject
      EntityConverterFactory (class):  CreateConverter, CreatePropertyConverter
      EntityPropertyConverter (class):  EntityPropertyConverter, FromEntity, ToEntity
      EntityPropertyTypeConverter (class):  EntityPropertyTypeConverter, IsSupported, TryConvert
      IEntityPropertyConverter (interface):  FromEntity, ToEntity
      ObjectEntityPropertyConverter (class):  FromEntity, ObjectEntityPropertyConverter, ToEntity
      SagaETag (class):  SagaETag
      ValueTypeEntityPropertyConverter (class):  FromEntity, ToEntity, ValueTypeEntityPropertyConverter
   MassTransit.Batching
      BatchCollector (class):  Add, BatchCollector, Collect, Complete, DisposeAsync, Probe, Remove
      BatchConsumer (class):  Add, BatchConsumer, Consume, Deliver, ForceComplete, GetMessageBatchInOrder, IsReadyToDeliver, RemoveCanceledMessage, TimeLimitExpired
      BatchConsumerFactory (class):  BatchConsumerFactory, DisposeAsync, Probe, Send
      BatchEntry (struct):  BatchEntry, Unregister
      IBatchCollector (interface):  Collect, Complete
      MessageBatch (class):  GetEnumerator, MessageBatch
   MassTransit.Caching
      CacheSettings (class):  CacheSettings, CurrentTime
      GreenCache (class):  Add, AddIndex, Clear, Connect, GetAll, GetIndex, GreenCache
      ICache (interface):  Add, AddIndex, Clear, GetAll, GetIndex
      ICacheValueObserver (interface):  CacheCleared, ValueAdded, ValueRemoved
         Observes behavior within the cache
      IConnectCacheValueObserver (interface):  Connect
      IIndex (interface):  Get, Remove
         An index is used to access items in the cache quickly
      INode (interface):  GetValue
      INotifyValueUsed (interface)
         If a cached value implments this interface, the cache will attach itself to the event so the value can signal usage t...
      IPendingValue (interface):  CreateValue, SetValue
         A pending Get on an index, which has yet to be processed.
      TestCacheSettings (class):  TestCacheSettings
      TestTimeProvider (class):  Now, TestTimeProvider
   MassTransit.Clients
      BusClientFactoryContext (class):  BusClientFactoryContext, ConnectConsumePipe, ConnectRequestPipe, GetRequestEndpoint
      ClientFactory (class):  ClientFactory, CreateRequest, CreateRequestClient, DisposeAsync
      ClientRequestHandle (class):  AcceptResponse, AddPipeSpecification, Cancel, CancelAndDispose, ClientRequestHandle, Dispose, DisposeTimer, Fail, FaultHandler, GetResponse, HandleFault, Probe, Response, Send, SendRequest
      FaultHandlerConnectHandle (class):  Disconnect, Dispose, FaultHandlerConnectHandle, TrySetCanceled, TrySetException
         The fault handler for the request client
      HandlerConnectHandle (interface):  TrySetCanceled, TrySetException
      HostReceiveEndpointClientFactoryContext (class):  DisposeAsync, HostReceiveEndpointClientFactoryContext
      MessageResponse (class):  DeserializeObject, MessageResponse
         A result from a request
      PublishRequestSendEndpoint (class):  PublishRequestSendEndpoint
      ReceiveEndpointClientFactoryContext (class):  ConnectConsumePipe, ConnectRequestPipe, GetRequestEndpoint, ReceiveEndpointClientFactoryContext
      ReceiveEndpointPublishRequestSendEndpoint (class):  ReceiveEndpointPublishRequestSendEndpoint
      ReceiveEndpointSendRequestSendEndpoint (class):  ReceiveEndpointSendRequestSendEndpoint
      RequestClient (class):  Create, GetResponse, GetResponseInternal, RequestClient
      RequestSendEndpoint (class):  Send
      ResponseHandlerConfigurator (class):  AddPipeSpecification, AsyncMessageHandler, Connect, ConnectHandlerConfigurationObserver, MessageHandler, ResponseHandlerConfigurator
         Connects a handler to the inbound pipe of the receive endpoint
      ResponseHandlerConnectHandle (class):  Disconnect, Dispose, GetTask, ResponseHandlerConnectHandle, TrySetCanceled, TrySetException
         A connection to a request which handles a result, and completes the Task when it's received
      ScopedClientFactory (class):  CreateRequest, CreateRequestClient, ScopedClientFactory
      SendRequestSendEndpoint (class):  SendRequestSendEndpoint
   MassTransit.Components
      RequestState (class)
      RequestStateMachine (class):  InitializeInstance, RedeliverOnMissingInstance, RequestStateMachine
         Tracks a request, which was sent to a saga, and the saga deferred until some operation is completed, after which it w...
   MassTransit.Configuration
      ActivityConfigurationObservable (class):  ActivityConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, Method4, Method5, Method6
      ActivityDefinitionRegistrar (class):  Register
      ActivityPipeConfiguratorExtensions (class):  AddPipeSpecification
      ActivityRegistrar (class):  Register
      AsyncConsumerConvention (class):  GetConsumerMessageConvention
      AsyncConsumerMessageConvention (class):  GetMessageTypes
         A default convention that looks for IConsumerOfT message types
      AsyncDelegatePipeSpecification (class):  Apply, AsyncDelegatePipeSpecification, Validate
      AzureTableSagaRepositoryConfigurator (class):  ConnectionFactory, KeyFormatter, Register, Validate
      AzureTableSagaRepositoryRegistrationProvider (class):  AzureTableSagaRepositoryRegistrationProvider, Configure
      BaseHostConfiguration (class):  Build, ConnectConsumeObserver, ConnectEndpointConfigurationObserver, ConnectPublishObserver, ConnectReceiveEndpointContext, ConnectReceiveObserver, ConnectSendObserver, CreateReceiveEndpointConfiguration, ReceiveEndpoint, Validate
      Batch (class):  Apply, Batch, BatchFactory, Validate
      BatchConfigurator (class):  BatchConfigurator, Consumer
      BatchConnectHandle (class):  BatchConnectHandle, Disconnect, Dispose
      BatchConsumerConvention (class):  GetConsumerMessageConvention
      BatchConsumerInterfaceType (class):  BatchConsumerInterfaceType, GetConsumerConnector, GetInstanceConnector
         A batch consumer
      BatchConsumerMessageConnector (class):  ConnectConsumer, CreateConsumerMessageSpecification
      BatchConsumerMessageConvention (class):  GetMessageTypes
         A convention that looks for IConsumerOfBatchOfT message types
      BatchConsumerMessageSpecification (class):  AddPipeSpecification, BatchConsumerMessageSpecification, Build, BuildMessagePipe, ConnectConsumerConfigurationObserver, Message, TryGetMessageSpecification, Validate
         Configures the pipe for a consumer/message combination within a consumer configuration block.
      BatchMessageConnectorFactory (class):  BatchMessageConnectorFactory, CreateConsumerConnector, CreateInstanceConnector
      BehaviorContextRetryConfigurator (class):  BehaviorContextRetryConfigurator, ConnectRetryObserver, GetRetryPolicy, SetRetryPolicy
      BindConfigurator (class):  BindConfigurator, Source
      BindPipeSpecification (class):  AddPipeSpecification, Apply, BindPipeSpecification, Validate
      Builder (class):  AddFilter, Builder, ContextProvider, CreateDelegatedBuilder, CreateImplementedBuilder, FilterContext, InputContextProvider, MergeContext
      BuilderProxy (class):  AddFilter, BuilderProxy
      BusFactoryConfigurator (class):  ActivityConfigured, AddDeserializer, AddPipeSpecification, AddPrePipeSpecification, AddSerializer, ClearSerialization, CompensateActivityConfigured, ConfigurePublish, ConfigureSend, ConnectActivityConfigurationObserver, ConnectBusObserver, ConnectConsumeObserver, ConnectConsumerConfigurationObserver, ConnectEndpointConfigurationObserver, ConnectHandlerConfigurationObserver
      BusFactorySelector (class)
      BusRegistrationContext (class):  BusRegistrationContext, ConfigureEndpoints, ConfigureTheEndpoints, GetConfigureReceiveEndpoints, NoFilter
      Cached (class)
      CachedConnector (interface):  CachedConnector, Connect
      CachedValue (class):  CachedValue
      CancelScheduledMessageConsumerDefinition (class):  CancelScheduledMessageConsumerDefinition
      ChildBuilder (class):  AddFilter, ChildBuilder, CreateDelegatedBuilder
      ChildSpecificationPipeBuilder (class):  AddFilter, ChildSpecificationPipeBuilder, CreateDelegatedBuilder, CreateImplementedBuilder
      CircuitBreakerPipeSpecification (class):  Apply, CircuitBreakerPipeSpecification, IntervalTimeout, Validate
      CombinedEndpointDefinition (class):  Configure, GetEndpointName
      CompensateActivityEndpointDefinition (class):  CompensateActivityEndpointDefinition
      CompensateActivityHostConfigurator (class):  ActivityLog, AddPipeSpecification, CompensateActivityHostConfigurator, Configure, ConnectActivityObserver, Log, RoutingSlip, Validate
      CompensateActivityLogConfigurator (class):  AddPipeSpecification, CompensateActivityLogConfigurator
      CompensateContextRedeliveryPipeSpecification (class):  Apply, CompensateContextRedeliveryPipeSpecification, ConnectRetryObserver, Factory, SetRetryPolicy, Validate
      CompensateContextRetryPipeSpecification (class):  Apply, CompensateContextRetryPipeSpecification, ConnectRetryObserver, Factory, SetRetryPolicy, Validate
      CompensateContextTimeoutSpecification (class):  Apply, Factory, Validate
      CompensateLogConfigurator (class):  AddPipeSpecification, CompensateLogConfigurator
      CompositeFilter (class):  Matches
      CompositePredicate (class):  Add, DoesNotMatcheAny, MatchesAll, MatchesAny, MatchesNone
      ConcurrencyLimitConfigurationObserver (class):  ConcurrencyLimitConfigurationObserver, MessageConfigured, Method4, Method5, Method6
         Adds a concurrency limit filter for each message type configured on the consume pipe
      ConcurrencyLimitConsumePipeSpecification (class):  Apply, ConcurrencyLimitConsumePipeSpecification, Validate
         Adds a concurrency limit filter to the message pipe.
      ConcurrencyLimitConsumerConfigurationObserver (class):  ConcurrencyLimitConsumerConfigurationObserver, ConsumerConfigured, ConsumerMessageConfigured
         Configures a concurrency limit for a consumer, on the consumer configurator, which is constrained to the message type...
      ConcurrencyLimitHandlerConfigurationObserver (class):  ConcurrencyLimitHandlerConfigurationObserver, HandlerConfigured
         Configures a concurrency limit for a handler, on the handler configurator, which is constrained to the message type f...
      ConcurrencyLimitPipeSpecification (class):  Apply, ConcurrencyLimitPipeSpecification, Validate
         Configures a concurrency limit on the pipe.
      ConcurrencyLimitSagaConfigurationObserver (class):  ConcurrencyLimitSagaConfigurationObserver, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
         Configures a concurrency limit for a consumer, on the consumer configurator, which is constrained to the message type...
      ConfigurationObserver (class):  ActivityConfigured, BatchConsumerConfigured, CompensateActivityConfigured, ConsumerConfigured, ConsumerMessageConfigured, ExecuteActivityConfigured, HandlerConfigured, NotifyObserver, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
         Combines the separate configuration observers into a single observer that is for each message type, called once, to c...
      ConfiguratorPipeConnectorSpecification (class):  AddPipeSpecification, ConfiguratorPipeConnectorSpecification, Connect, Validate
      ConfigureReceiveEndpoint (class):  Configure, ConfigureReceiveEndpoint
      ConfigureReceiveEndpointDelegate (class):  Configure, ConfigureReceiveEndpointDelegate
      ConfigureReceiveEndpointDelegateProvider (class):  Configure, ConfigureReceiveEndpointDelegateProvider
      ConfigureSagaRepository (class):  Configure
      ConnectReceiveEndpointObserverBusObserver (class):  ConnectReceiveEndpointObserverBusObserver, CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted
      ConnectionMultiplexerFactory (class):  ConnectionMultiplexerFactory, DisposeAsync, GetConnectionMultiplexer
      ConstructorSagaInstanceFactory (class):  ConstructorSagaInstanceFactory
         Creates a saga instance using the constructor, via a compiled expression.
      ConsumeContextRescuePipeSpecification (class):  Apply, ConsumeContextRescuePipeSpecification, Validate
      ConsumeContextRetryPipeSpecification (class):  Apply, ConnectRetryObserver, ConsumeContextRetryPipeSpecification, SetRetryPolicy, Validate
      ConsumeMessageConnectorFactory (class):  ConsumeMessageConnectorFactory, CreateConsumerConnector, CreateInstanceConnector
      ConsumeMessageFilterConfigurator (class):  ConsumeMessageFilterConfigurator, Exclude, Include, Match
      ConsumePipeConfiguration (class):  ConsumePipeConfiguration
      ConsumePipeSpecification (class):  ActivityConfigured, AddPipeSpecification, AddPrePipeSpecification, BuildConsumePipe, CompensateActivityConfigured, ConnectActivityConfigurationObserver, ConnectConsumePipeSpecificationObserver, ConnectConsumerConfigurationObserver, ConnectHandlerConfigurationObserver, ConnectSagaConfigurationObserver, ConsumePipeSpecification, ConsumerConfigured, ConsumerMessageConfigured, CreateConsumePipeSpecification, CreateMessageSpecification
      ConsumePipeSpecificationObservable (class):  MessageSpecificationCreated, Method4, Method5, Method6
      ConsumeTopologyConfigurationObservable (class):  MessageTopologyCreated, Method4, Method5, Method6
      ConsumeTransformSpecification (class):  Apply
      ConsumerConfigurationObservable (class):  ConsumerConfigured, ConsumerMessageConfigured, Method4, Method5, Method6
      ConsumerConfigurator (class):  AddPipeSpecification, Configure, ConnectConsumerConfigurationObserver, ConsumerConfigurator, ConsumerMessage, Message, Options, SelectOptions, TryGetOptions, Validate
      ConsumerConnector (class):  ConnectConsumer, ConsumerConnector, Consumes, CreateConsumerSpecification
      ConsumerConnectorCache (class):  Connect, ConsumerConnectorCache, GetOrAdd
      ConsumerConsumeContextRescuePipeSpecification (class):  Apply, ConsumerConsumeContextRescuePipeSpecification, Validate
      ConsumerConventionCache (class):  ConsumerConventionCache, GetConventions, Remove, TryAdd
      ConsumerDefinitionRegistrar (class):  Register
      ConsumerEndpointDefinition (class):  ConsumerEndpointDefinition
      ConsumerFactoryConfiguratorExtensions (class):  HasProtectedDefaultConstructor, Validate, ValidateConsumer
      ConsumerFilterSpecification (class):  Apply, ConsumerFilterSpecification, Validate
      ConsumerInterfaceType (class):  ConsumerInterfaceType, GetConsumerConnector, GetInstanceConnector
         A standard asynchronous consumer message type, defined by IConsumer
      ConsumerMessageConfigurator (class):  AddPipeSpecification, ConsumerMessageConfigurator
      ConsumerMessageConnector (class):  ConnectConsumer, ConsumerMessageConnector, CreateConsumerMessageSpecification
      ConsumerMessageSpecification (class):  AddPipeSpecification, Build, BuildMessagePipe, ConnectConsumerConfigurationObserver, ConsumerMessageSpecification, Message, TryGetMessageSpecification, Validate
         Configures the pipe for a consumer/message combination within a consumer configuration block.
      ConsumerMessageSplitFilterSpecification (class):  Apply, ConsumerMessageSplitFilterSpecification, Validate
      ConsumerMetadataCache (class):  ConsumerMetadataCache
      ConsumerPipeSpecificationProxy (class):  Apply, ConsumerPipeSpecificationProxy, Validate
      ConsumerRegistrar (class):  Register
      ConsumerSpecification (class):  AddPipeSpecification, ConfigureMessagePipe, ConnectConsumerConfigurationObserver, ConsumerMessage, ConsumerSpecification, GetMessageSpecification, Message, Validate
      ConsumerSplitFilterSpecification (class):  Apply, ConsumerSplitFilterSpecification, Validate
      ContextFilterPipeSpecification (class):  Apply, ContextFilterPipeSpecification, Validate
      ContextPipeConfigurator (class):  AddPipeSpecification, Context, ContextPipeConfigurator, InputContext
      CorrelatedByCorrelationIdSelector (class):  TryGetSetCorrelationId
      CorrelatedByEventCorrelationBuilder (class):  Build, CorrelatedByEventCorrelationBuilder
      CorrelatedByFaultEventCorrelationBuilder (class):  Build, CorrelatedByFaultEventCorrelationBuilder
      CorrelatedSagaMessageConnector (class):  CorrelatedSagaMessageConnector
         Connects a message that has an exact CorrelationId to the saga instance to the saga repository.
      CorrelationIdMessageSendTopologyConvention (class):  CorrelationIdMessageSendTopologyConvention, SetCorrelationId, TryGetMessageCorrelationId, TryGetMessageSendTopology, TryGetMessageSendTopologyConvention
      CorrelationIdSendTopologyConvention (class):  CorrelationIdSendTopologyConvention, TryGetMessageSendTopologyConvention
         Looks for a property that can be used as a CorrelationId message header, and applies a filter to set it on message se...
      CosmosSagaRepositoryConfigurator (class):  ConfigureEmulator, ConfigureItemRequestOptions, ConfigureLinqSerializerOptions, ConfigureQueryRequestOptions, CosmosSagaRepositoryConfigurator, DatabaseContextFactory, Register, RegisterNewtonsoftJsonClientFactory, RegisterSystemTextJsonClientFactory, SetCollectionIdFormatter, UseClientFactory, UseNewtonsoftJson, Validate
      CosmosSagaRepositoryRegistrationProvider (class):  Configure, CosmosSagaRepositoryRegistrationProvider
      DapperSagaRepositoryRegistrationProvider (class):  Configure, DapperSagaRepositoryRegistrationProvider
      DeadLetterPipeSpecification (class):  Apply, DeadLetterPipeSpecification, Validate
      DefaultActivityDefinition (class)
      DefaultConsumerDefinition (class)
         A default consumer definition, used if no definition is found for the consumer type
      DefaultEndpointDefinition (class):  Configure, GetEndpointName
         Base/Default endpoint definition, not used apparently
      DefaultExecuteActivityDefinition (class)
      DefaultSagaDefinition (class)
      DefaultSagaFactory (class):  Create, Send
         Creates a saga instance using the default factory method
      DefaultTransportConfiguration (class):  DefaultTransportConfiguration, GetConcurrentMessageLimit, Validate
      DelayedMessageSchedulerSpecification (class):  Apply, Validate
      DelayedRedeliveryConfigurationObserver (class):  DelayedRedeliveryConfigurationObserver, Method7, Method8, Method9
      DelayedRedeliveryConsumerConfigurationObserver (class):  ConsumerConfigured, ConsumerMessageConfigured, DelayedRedeliveryConsumerConfigurationObserver
         Configures scheduled message redelivery for a consumer, on the consumer configurator, which is constrained to the mes...
      DelayedRedeliveryHandlerConfigurationObserver (class):  DelayedRedeliveryHandlerConfigurationObserver, HandlerConfigured
         Configures a message retry for a handler, on the handler configurator, which is constrained to the message types for ...
      DelayedRedeliveryPipeSpecification (class):  Apply, Validate
      DelayedRedeliverySagaConfigurationObserver (class):  DelayedRedeliverySagaConfigurationObserver, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
         Configures scheduled message redelivery for a saga, on the saga configurator, which is constrained to the message typ...
      DelegateEndpointDefinition (class):  Configure, DelegateEndpointDefinition, GetEndpointName
      DelegatePipeSpecification (class):  Apply, DelegatePipeSpecification, Validate
      DelegatePublishTopologyConfigurationObserver (class):  DelegatePublishTopologyConfigurationObserver, MessageTopologyCreated
      DelegateSendTopologyConfigurationObserver (class):  DelegateSendTopologyConfigurationObserver, MessageTopologyCreated
      DependencyInjectionActivityRegistrationExtensions (class):  RegisterActivity
      DependencyInjectionConsumerRegistrationExtensions (class):  RegisterConsumer
      DependencyInjectionContainerRegistrar (class):  AddDefinition, AddEndpointDefinition, DependencyInjectionContainerRegistrar, GetConfigureReceiveEndpoints, GetDefinition, GetEndpointDefinition, GetEndpointNameFormatter, GetOrAddRegistration, GetRegistrations, RegisterEndpointNameFormatter, RegisterRequestClient, RegisterScopedClientFactory, TryGetRegistration
      DependencyInjectionEndpointRegistrationExtensions (class):  RegisterEndpoint
      DependencyInjectionExecuteActivityRegistrationExtensions (class):  RegisterExecuteActivity
      DependencyInjectionFutureRegistrationExtensions (class):  RegisterFuture
      DependencyInjectionHandlerRegistrationExtensions (class):  RegisterHandler
      DependencyInjectionJobServiceRegistrationExtensions (class):  RegisterJobService
      DependencyInjectionMediatorContainerRegistrar (class):  DependencyInjectionMediatorContainerRegistrar, GetRegistrations
      DependencyInjectionRiderContainerRegistrar (class):  DependencyInjectionRiderContainerRegistrar, GetRegistrations
      DependencyInjectionSagaRegistrationExtensions (class):  RegisterSaga
      DependencyInjectionSagaStateMachineRegistrationExtensions (class):  RegisterSaga, RegisterSagaStateMachine
      DispatchPipeSpecification (class):  Apply, DispatchPipeSpecification, Pipe, Validate
      DynamoDbSagaRepositoryConfigurator (class):  ContextFactory, DynamoDbSagaRepositoryConfigurator, Register, Validate
      DynamoDbSagaRepositoryRegistrationProvider (class):  Configure, DynamoDbSagaRepositoryRegistrationProvider
      EmptyPipe (class):  Probe, Send
      Endpoint (class):  Endpoint
      EndpointConfiguration (class):  ActivityConfigured, AddDeserializer, AddPipeSpecification, AddPrePipeSpecification, AddSerializer, ClearSerialization, CompensateActivityConfigured, ConfigureDeadLetter, ConfigureError, ConfigurePublish, ConfigureReceive, ConfigureSend, ConfigureTransport, ConnectActivityConfigurationObserver, ConnectConsumerConfigurationObserver
      EndpointConfigurationObservable (class):  EndpointConfigured, Method4, Method5, Method6
      EndpointDefinitionExtensions (class):  Combine
      EndpointRegistrar (class):  EndpointRegistrar, Register
      EndpointRegistrationConfigurator (class):  AddConfigureEndpointCallback, EndpointRegistrationConfigurator
      EndpointSettings (class):  AddConfigureEndpointCallback, ConfigureEndpoint, EndpointSettings
      EntityFrameworkBusOutboxConfigurator (class):  Configure, DisableDeliveryService, EntityFrameworkBusOutboxConfigurator
      EntityFrameworkOutboxConfigurator (class):  Configure, DisableInboxCleanupService, EntityFrameworkOutboxConfigurator, UseBusOutbox
      EntityFrameworkSagaRepositoryConfigurator (class):  AddDbContext, CheckContextConstructors, CreateOptimisticLockStrategy, CreatePessimisticLockStrategy, CustomizeQuery, DatabaseFactory, DbContextOptionsFactory, EntityFrameworkSagaRepositoryConfigurator, ExistingDbContext, Register, SetOptimisticConcurrency, Validate
      EntityFrameworkSagaRepositoryRegistrationProvider (class):  Configure, EntityFrameworkSagaRepositoryRegistrationProvider
      EventMissingInstanceConfigurator (class):  Discard, Execute, ExecuteAsync, Fault
      ExceptionSpecification (class):  Handle, Ignore, Match
      ExecuteActivityArgumentsConfigurator (class):  AddPipeSpecification, ExecuteActivityArgumentsConfigurator
      ExecuteActivityDefinitionRegistrar (class):  Register
      ExecuteActivityEndpointDefinition (class):  ExecuteActivityEndpointDefinition
      ExecuteActivityHostConfigurator (class):  ActivityArguments, AddPipeSpecification, Arguments, Configure, ConnectActivityObserver, ExecuteActivityHostConfigurator, RoutingSlip, Validate
      ExecuteActivityRegistrar (class):  Register
      ExecuteArgumentsConfigurator (class):  AddPipeSpecification, ExecuteArgumentsConfigurator
      ExecuteContextRedeliveryPipeSpecification (class):  Apply, ConnectRetryObserver, ExecuteContextRedeliveryPipeSpecification, Factory, SetRetryPolicy, Validate
      ExecuteContextRetryPipeSpecification (class):  Apply, ConnectRetryObserver, ExecuteContextRetryPipeSpecification, Factory, SetRetryPolicy, Validate
      ExecuteContextTimeoutSpecification (class):  Apply, Factory, Validate
      Factory (class):  Create
      FilterPipe (class):  FilterPipe, Probe, Send
      FilterPipeSpecification (class):  Apply, FilterPipeSpecification, Validate
         Adds an arbitrary filter to the pipe
      ForkPipeSpecification (class):  Apply, ForkPipeSpecification, Validate
         Adds a fork to the pipe
      FutureDefinitionRegistrar (class):  Register
      FutureEndpointDefinition (class):  FutureEndpointDefinition
      FutureFaultConfigurator (class):  FutureFaultConfigurator, SetFaultedUsingFactory, SetFaultedUsingInitializer
      FutureRegistrar (class):  Register
      FutureRequestConfigurator (class):  FutureRequestConfigurator, OnRequestFaulted, OnResponseReceived, Send, SendRange, SetFaulted, SetRequestAddressProvider, TrackPendingRequest, UsingRequestFactory, UsingRequestInitializer, Validate, WhenFaulted
      FutureResponseConfigurator (class):  CompletePendingRequest, FutureResponseConfigurator, GetResultConfigurator, OnResponseReceived, SetCompletedUsingFactory, SetCompletedUsingInitializer, SetResult, Validate, WhenReceived
      FutureResultConfigurator (class):  FutureResultConfigurator, SetCompletedUsingFactory, SetCompletedUsingInitializer
      FutureRoutingSlipConfigurator (class):  BuildItinerary, BuildUsingItineraryPlanner, Execute, FutureRoutingSlipConfigurator, GetTrackingNumber, HasFault, HasResult, OnRoutingSlipCompleted, OnRoutingSlipFaulted, RoutingSlipFaultedValueProvider, TrackPendingRoutingSlip, Validate, WhenRoutingSlipCompleted, WhenRoutingSlipFaulted
      GroupKeyProvider (class):  GroupKeyProvider, TryGetKey
      HandlerConfigurationObservable (class):  HandlerConfigured, Method4, Method5, Method6
      HandlerConfigurator (class):  AddPipeSpecification, Configure, ConnectHandlerConfigurationObserver, HandlerConfigurator, Validate
         Connects a handler to the inbound pipe of the receive endpoint
      HandlerConnector (class):  ConnectHandler, ConnectRequestHandler
         Connects a message handler to a pipe
      HandlerConnectorCache (class):  HandlerConnectorCache
      HandlerPipeSpecification (class):  Apply, HandlerPipeSpecification, Validate
         Adds a message handler to the consuming pipe builder
      HangfireEndpointDefinition (class):  Configure, GetEndpointName, HangfireEndpointDefinition
      HasValueTypeSagaQueryPropertySelector (class):  HasValueTypeSagaQueryPropertySelector, TryGetProperty
      IActivityRegistrar (interface):  Register
      IActivityRegistration (interface):  AddConfigureAction, Configure, ConfigureCompensate, ConfigureExecute, GetDefinition
         An activity, which must be configured on two separate receive endpoints
      IBuildPipeConfigurator (interface):  Build
      IBusConfiguration (interface)
         The configuration of a bus
      IBusInstanceSpecification (interface):  Configure
      ICompensateTransformSpecification (interface)
      IConfigureSagaRepository (interface):  Configure
      IConsumePipeConfiguration (interface)
      IConsumePipeSpecification (interface):  BuildConsumePipe, CreateConsumePipeSpecification, GetMessageSpecification
      IConsumePipeSpecificationObserver (interface):  MessageSpecificationCreated
      IConsumePipeSpecificationObserverConnector (interface):  ConnectConsumePipeSpecificationObserver
      IConsumeTopologyConfigurationObserver (interface):  MessageTopologyCreated
      IConsumeTopologyConfigurationObserverConnector (interface):  ConnectConsumeTopologyConfigurationObserver
      IConsumeTopologyConvention (interface)
         A convention that is applies to a message type on Consume, if applicable to the message type.
      IConsumeTransformSpecification (interface)
      IConsumerConnector (interface):  ConnectConsumer, CreateConsumerSpecification
         Interface implemented by objects that tie an inbound pipeline together with consumers (by means of calling a consumer...
      IConsumerConnectorCache (interface)
      IConsumerConvention (interface):  GetConsumerMessageConvention
         A consumer convention is used to find message types inside a consumer class.
      IConsumerMessageConnector (interface):  ConnectConsumer, CreateConsumerMessageSpecification
      IConsumerMessageConvention (interface):  GetMessageTypes
         A convention that returns connectors for message types that are defined in the consumer type.
      IConsumerMessageSpecification (interface):  Build, BuildMessagePipe, TryGetMessageSpecification
      IConsumerMetadataCache (interface)
      IConsumerRegistrar (interface):  Register
      IConsumerRegistration (interface):  AddConfigureAction, Configure, GetConsumerRegistrationConfigurator, GetDefinition
      IConsumerSpecification (interface):  ConfigureMessagePipe, GetMessageSpecification
         A consumer specification, that can be modified
      IContainerRegistrar (interface):  AddDefinition, AddEndpointDefinition, GetOrAddRegistration, GetRegistrations, RegisterEndpointNameFormatter, RegisterRequestClient, RegisterScopedClientFactory
      IContainerSelector (interface):  GetConfigureReceiveEndpoints, GetDefinition, GetEndpointDefinition, GetEndpointNameFormatter, GetRegistrations, TryGetRegistration
         Used to pull configuration from the container, scoped to the bus, multi-bus, or mediator
      IConventionTypeFactory (interface):  Create
      ICorrelationIdMessageSendTopologyConvention (interface):  SetCorrelationId, TryGetMessageCorrelationId
      ICorrelationIdSelector (interface):  TryGetSetCorrelationId
      IEndpointConfiguration (interface)
      IEndpointRegistrar (interface):  Register
      IEndpointRegistration (interface):  GetDefinition
      IEventCorrelationBuilder (interface):  Build
      IExecuteActivityRegistrar (interface):  Register
      IExecuteActivityRegistration (interface):  AddConfigureAction, Configure, GetDefinition
         An execute activity, which doesn't have compensation
      IExecuteTransformSpecification (interface)
      IFutureRegistrar (interface):  Register
      IFutureRegistration (interface):  Configure, GetDefinition
      IHandlerConnector (interface):  ConnectHandler, ConnectRequestHandler
         Connects a message handler to the ConsumePipe
      IHandlerConnectorCache (interface)
      IHealthCheckOptions (interface)
      IHostConfiguration (interface):  Build, ConnectReceiveEndpointContext, CreateReceiveEndpointConfiguration
      IInstanceConnector (interface):  ConnectInstance, CreateConsumerSpecification
      IInstanceConnectorCache (interface)
      IInstanceMessageConnector (interface):  ConnectInstance, CreateConsumerMessageSpecification
      IJobServiceRegistration (interface):  AddConfigureAction, AddReceiveEndpointDependency, Configure
      IMessageConnectorFactory (interface):  CreateConsumerConnector, CreateInstanceConnector
      IMessageConsumePipeConfigurator (interface)
         Configures the Consuming of a message type, allowing filters to be applied on Consume.
      IMessageConsumePipeSpecification (interface):  AddParentMessageSpecification, BuildMessagePipe, GetMessageSpecification
      IMessageConsumeTopologyConvention (interface):  TryGetMessageConsumeTopology, TryGetMessageConsumeTopologyConvention
      IMessageDataRepositorySelector (interface)
         Use one of the selector extension methods to create a instance for the selected repository implementation.
      IMessageFabricConsumeTopologyBuilder (interface)
      IMessageFabricPublishTopologyBuilder (interface):  CreateImplementedBuilder
      IMessageFabricTopologyBuilder (interface):  ExchangeBind, ExchangeDeclare, QueueBind, QueueDeclare
      IMessageInterfaceType (interface):  GetConsumerConnector, GetInstanceConnector
      IMessagePublishPipeSpecification (interface):  AddParentMessageSpecification, BuildMessagePipe, GetMessageSpecification
      IMessagePublishTopologyConvention (interface):  TryGetMessagePublishTopology, TryGetMessagePublishTopologyConvention
      IMessageSendPipeSpecification (interface):  AddParentMessageSpecification, BuildMessagePipe, GetMessageSpecification
      IMessageSendTopologyConvention (interface):  TryGetMessageSendTopology, TryGetMessageSendTopologyConvention
      IMessageTopologyConfigurationObserver (interface):  MessageTopologyCreated
         Observes the configuration of message-specific topology
      IMessageTopologyConfigurationObserverConnector (interface):  ConnectMessageTopologyConfigurationObserver
      IMessageTopologyConfigurator (interface):  GetMessageTopology, SetEntityName, SetEntityNameFormatter
      IMessageTypeTopologyConfigurator (interface)
      IObserverConnector (interface):  ConnectObserver, ConnectRequestObserver
         Connects a message handler to the ConsumePipe
      IObserverConnectorCache (interface)
      IOptions (interface)
      IOptionsSet (interface):  Options, SelectOptions, TryGetOptions
      IPartitionKeyMessageSendTopologyConvention (interface):  SetFormatter
      IPartitionKeySendTopologyConvention (interface)
      IPipeBuilder (interface):  AddFilter
         A pipe builder constructs a pipe by adding filter to the end of the chain, after while the builder completes the pipe...
      IPipeSpecification (interface):  Apply
         Configures a pipe builder (typically by adding filters), but allows late binding to the pipe builder with pre-validat...
      IProxy (interface):  Configure
      IPublishPipeConfiguration (interface):  CreatePipe
      IPublishPipeSpecification (interface):  GetMessageSpecification
      IPublishPipeSpecificationObserver (interface):  MessageSpecificationCreated
      IPublishPipeSpecificationObserverConnector (interface):  ConnectPublishPipeSpecificationObserver
      IPublishTopologyConfigurationObserver (interface):  MessageTopologyCreated
      IPublishTopologyConfigurationObserverConnector (interface):  ConnectPublishTopologyConfigurationObserver
      IPublishTopologyConvention (interface)
         A convention that is applies to a message type on Publish, if applicable to the message type.
      IReceiveEndpointBuilder (interface)
      IReceiveEndpointConfiguration (interface):  CreateReceiveEndpointContext, CreateReceivePipe
      IReceivePipeConfiguration (interface):  CreatePipe
      IRedeliveryPipeSpecification (interface)
      IRegistration (interface)
      IRegistrationCache (interface)
      IRoutingKeyMessageSendTopologyConvention (interface):  SetFormatter
      IRoutingKeySendTopologyConvention (interface)
      ISagaConnector (interface):  ConnectSaga, CreateSagaSpecification
      ISagaConnectorCache (interface)
      ISagaConnectorFactory (interface):  CreateMessageConnector
      ISagaMessageConnector (interface):  ConnectSaga, CreateSagaMessageSpecification
      ISagaMessageSpecification (interface):  BuildConsumerPipe, BuildMessagePipe, GetMessageSpecification
      ISagaMetadataCache (interface)
      ISagaQueryPropertySelector (interface):  TryGetProperty
      ISagaRegistrar (interface):  Register
      ISagaRegistration (interface):  AddConfigureAction, Configure, GetDefinition
      ISagaRepositoryDecoratorRegistration (interface):  DecorateSagaRepository
      ISagaRepositoryRegistrationProvider (interface):  Configure
      ISagaSpecification (interface):  ConfigureMessagePipe, GetMessageSpecification
         A consumer specification, that can be modified
      ISendPipeConfiguration (interface):  CreatePipe
      ISendPipeSpecification (interface):  GetMessageSpecification
      ISendPipeSpecificationObserver (interface):  MessageSpecificationCreated
      ISendPipeSpecificationObserverConnector (interface):  ConnectSendPipeSpecificationObserver
      ISendTopologyConfigurationObserver (interface):  MessageTopologyCreated
      ISendTopologyConfigurationObserverConnector (interface):  ConnectSendTopologyConfigurationObserver
      ISendTopologyConvention (interface)
         A convention that is applies to a message type on send, if applicable to the message type.
      ISendTransformSpecification (interface)
      ISerializationConfiguration (interface):  AddDeserializer, AddSerializer, Clear, CreateSerializationConfiguration, CreateSerializerCollection
      ISetSerializerMessageSendTopologyConvention (interface):  SetSerializer
      ISpecificationPipeBuilder (interface):  CreateDelegatedBuilder, CreateImplementedBuilder
      ISpecificationPipeSpecification (interface):  Apply
      IStateMachineInterfaceType (interface):  GetConnector
      ITopologyConfiguration (interface)
      ITopologyConventionCache (interface):  GetOrAdd
         A convention cache for type specified, which converts to the generic type requested
      ITopologyPipeBuilder (interface):  CreateDelegatedBuilder
         A pipe builder used by topologies, which indicates whether the message type is either delegated (called from a sub-sp...
      ITransportConfiguration (interface):  GetConcurrentMessageLimit
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector, ImplementsMessageType
      ImplementedTypeAdapter (class):  Apply, ImplementedTypeAdapter, Validate
      InMemoryCompensateContextOutboxSpecification (class):  Apply, Factory, InMemoryCompensateContextOutboxSpecification, Validate
      InMemoryExecuteContextOutboxSpecification (class):  Apply, Factory, InMemoryExecuteContextOutboxSpecification, Validate
      InMemoryOutboxConfigurationObserver (class):  ActivityConfigured, BatchConsumerConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, InMemoryOutboxConfigurationObserver, MessageConfigured, Method4, Method5, Method6
      InMemoryOutboxConsumerConfigurationObserver (class):  ConsumerConfigured, ConsumerMessageConfigured, InMemoryOutboxConsumerConfigurationObserver
      InMemoryOutboxHandlerConfigurationObserver (class):  HandlerConfigured, InMemoryOutboxHandlerConfigurationObserver
         Configures a message retry for a handler, on the handler configurator, which is constrained to the message types for ...
      InMemoryOutboxSagaConfigurationObserver (class):  InMemoryOutboxSagaConfigurationObserver, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
      InMemoryOutboxSpecification (class):  Apply, Factory, InMemoryOutboxSpecification, Validate
      InMemorySagaRepositoryRegistrationProvider (class):  Configure
      InMemoryTestHarnessRegistrationBusFactory (class):  CreateBus, InMemoryTestHarnessRegistrationBusFactory
      InitiatedByOrOrchestratesSagaConnectorFactory (class):  CreateMessageConnector, InitiatedByOrOrchestratesSagaConnectorFactory
      InitiatedBySagaConnectorFactory (class):  CreateMessageConnector, InitiatedBySagaConnectorFactory
      InlineFilterPipeSpecification (class):  Apply, InlineFilterPipeSpecification, Validate
         Adds an arbitrary filter to the pipe
      InstanceCache (class)
      InstanceConfigurator (class):  AddPipeSpecification, Configure, ConnectConsumerConfigurationObserver, ConsumerMessage, InstanceConfigurator, Message, Options, SelectOptions, TryGetOptions, Validate
      InstanceConnector (class):  ConnectInstance, Consumes, CreateConsumerSpecification, InstanceConnector
      InstanceConnectorCache (class):  GetInstanceConnector, InstanceConnectorCache
      InstanceEndpointDefinition (class):  Configure, GetEndpointName, InstanceEndpointDefinition
         Instance-specific address for a service endpoint
      InstanceJobServiceSettings (class):  ApplyConfiguration, InstanceJobServiceSettings
      InstanceMessageConnector (class):  ConnectInstance, CreateConsumerMessageSpecification, InstanceMessageConnector
         Connects a consumer instance to the inbound pipeline for the specified message type.
      InterceptPipeSpecification (class):  Apply, InterceptPipeSpecification, Validate
         Adds a fork to the pipe
      JobAttemptSagaDefinition (class):  JobAttemptSagaDefinition
      JobConsumerConvention (class):  GetConsumerMessageConvention
      JobConsumerMessageConnector (class):  ConfigureInstanceStartJobConsumer, ConnectConsumer, ConnectFinalizeJobConsumer, ConnectStartJobConsumer, ConnectSubmitJobConsumer, CreateConsumerMessageSpecification, CreateJobPipe, JobConsumerMessageConnector
      JobConsumerMessageConvention (class):  GetMessageTypes
      JobConsumerMessageSpecification (class):  AddPipeSpecification, Build, BuildMessagePipe, ConnectConsumerConfigurationObserver, JobConsumerMessageSpecification, Message, TryGetMessageSpecification, Validate
      JobInterfaceType (class):  GetConsumerConnector, GetInstanceConnector, JobInterfaceType
         A job consumer
      JobMessageConnectorFactory (class):  CreateConsumerConnector, CreateInstanceConnector, JobMessageConnectorFactory
      JobSagaDefinition (class):  JobSagaDefinition
      JobSagaSettings (interface)
         Settings used by the job service sagas
      JobSagaSettingsConfigurator (interface)
      JobServiceConfigurator (class):  ConfigureJobServiceEndpoints, JobServiceConfigurator, OnConfigureEndpoint, Validate
      JobServiceConsumerConfigurationObserver (class):  ConsumerConfigured, ConsumerMessageConfigured, JobServiceConsumerConfigurationObserver
      JobServiceEndpointConfigurationObserver (class):  EndpointConfigured, JobServiceEndpointConfigurationObserver
      JobServiceEndpointDefinition (class):  Configure, GetEndpointName, JobServiceEndpointDefinition
      JobTypeSagaDefinition (class):  JobTypeSagaDefinition
      Last (class):  Probe, Send
      LastPipe (class):  LastPipe, Probe, Send
         The last pipe in a pipeline is always an end pipe that does nothing and returns synchronously
      LatestPipeSpecification (class):  Apply, Validate
         Configures the Latest filter
      MartenSagaRepositoryConfigurator (class):  Connection, Factory, MartenSagaRepositoryConfigurator, Register, Validate
      MartenSagaRepositoryRegistrationProvider (class):  Configure, MartenSagaRepositoryRegistrationProvider
      MartenSagaRepositoryStoreOptionsConfigurator (class):  Configure, MartenSagaRepositoryStoreOptionsConfigurator
      MassTransitEventCorrelationConfigurator (class):  Build, CorrelateBy, CorrelateById, MassTransitEventCorrelationConfigurator, OnMissingInstance, SelectId, SetSagaFactory
      MassTransitHealthCheckOptions (class):  MassTransitHealthCheckOptions
      MediatorConfiguration (class):  ConnectConsumeObserver, ConnectPublishObserver, ConnectSendObserver, MediatorConfiguration
      MediatorRegistrationContext (class):  ConfigureActivity, ConfigureActivityCompensate, ConfigureActivityExecute, ConfigureConsumer, ConfigureConsumers, ConfigureExecuteActivity, ConfigureFuture, ConfigureSaga, ConfigureSagas, GetService, MediatorRegistrationContext, PushContext
      MessageConsumePipeSpecification (class):  AddParentMessageSpecification, AddPipeSpecification, Apply, BuildMessagePipe, FilterContext, GetMessageSpecification, MergeContext, MessageConsumePipeSpecification, Validate
      MessageConsumeTopologyPipeSpecification (class):  Apply, MessageConsumeTopologyPipeSpecification, Validate
      MessageCorrelationIdEventCorrelationBuilder (class):  Build, MessageCorrelationIdEventCorrelationBuilder
      MessageCorrelationIdFaultEventCorrelationBuilder (class):  Build, MessageCorrelationIdFaultEventCorrelationBuilder
      MessageDataRepositorySelector (class):  MessageDataRepositorySelector
      MessageObserverConnector (class):  ConnectObserver, ConnectRequestObserver
         Connects a message handler to a pipe
      MessagePublishPipeSpecification (class):  AddImplementedMessageSpecification, AddParentMessageSpecification, AddPipeSpecification, Apply, BuildMessagePipe, FilterContext, GetMessageSpecification, MergeContext, MessagePublishPipeSpecification, Validate
      MessagePublishPipeSplitFilterSpecification (class):  Apply, MessagePublishPipeSplitFilterSpecification, Validate
      MessagePublishTopologyPipeSpecification (class):  Apply, MessagePublishTopologyPipeSpecification, Validate
      MessageRetryConfigurationObserver (class):  ActivityConfigured, BatchConsumerConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, Factory, MessageConfigured, MessageRetryConfigurationObserver, Method4, Method5, Method6
      MessageRetryConsumerConfigurationObserver (class):  BatchConsumerConfigured, ConsumerConfigured, ConsumerMessageConfigured, Factory, MessageRetryConsumerConfigurationObserver
         Configures a message retry for a consumer, on the consumer configurator, which is constrained to the message types fo...
      MessageRetryHandlerConfigurationObserver (class):  Factory, HandlerConfigured, MessageRetryHandlerConfigurationObserver
         Configures a message retry for a handler, on the handler configurator, which is constrained to the message types for ...
      MessageRetrySagaConfigurationObserver (class):  Factory, MessageRetrySagaConfigurationObserver, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
         Configures a message retry for a saga, on the saga configurator, which is constrained to the message types for that s...
      MessageSchedulerPipeSpecification (class):  Apply, MessageSchedulerPipeSpecification, Validate
      MessageScopeConfigurationObserver (class):  ActivityConfigured, BatchConsumerConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, MessageConfigured, MessageScopeConfigurationObserver, Method4, Method5, Method6
      MessageSendPipeSpecification (class):  AddImplementedMessageSpecification, AddParentMessageSpecification, AddPipeSpecification, Apply, BuildMessagePipe, FilterContext, GetMessageSpecification, MergeContext, MessageSendPipeSpecification, Validate
      MessageSendPipeSplitFilterSpecification (class):  Apply, MessageSendPipeSplitFilterSpecification, Validate
      MessageSendTopologyPipeSpecification (class):  Apply, MessageSendTopologyPipeSpecification, Validate
      MessageSessionSagaRepositoryRegistrationProvider (class):  Configure
      MessageTopologyConfigurationObservable (class):  MessageTopologyCreated, Method4, Method5, Method6
      MessageTypeFilterConfigurator (class):  Exclude, Include, Match, MessageTypeFilterConfigurator
      MissingInstanceRedeliveryConfigurator (class):  Build, ConnectRetryObserver, MissingInstanceRedeliveryConfigurator, OnRedeliveryLimitReached, SetRetryPolicy, Validate
      MongoDbBusOutboxConfigurator (class):  Configure, DisableDeliveryService, MongoDbBusOutboxConfigurator
      MongoDbConfigurator (class):  ClientFactory, CollectionNameFormatter, DatabaseFactory, Validate
      MongoDbGuidRepresentationExtensions (class):  MongoDbGuidRepresentationExtensions
      MongoDbOutboxConfigurator (class):  Configure, DisableInboxCleanupService, MongoDbOutboxConfigurator, RegisterClassMaps, RegisterCollectionFactory, UseBusOutbox
      MongoDbSagaRepositoryConfigurator (class):  ClassMap, MongoDbSagaRepositoryConfigurator, Register, Validate
      MongoDbSagaRepositoryRegistrationProvider (class):  Configure, MongoDbSagaRepositoryRegistrationProvider
      NHibernateSagaRepositoryRegistrationProvider (class):  Configure
      NamedEndpointDefinition (class):  GetEndpointName, NamedEndpointDefinition
      NotDefaultValueTypeSagaQueryPropertySelector (class):  NotDefaultValueTypeSagaQueryPropertySelector, TryGetProperty
      NotSupportedSagaRepository (class):  Find, Load, Probe
      ObserverConfigurator (class):  AddPipeSpecification, Configure, ObserverConfigurator, Validate
      ObserverConnectorCache (class):  ObserverConnectorCache
      ObserverPipeSpecification (class):  Apply, ObserverPipeSpecification, Validate
         Adds a message handler to the consuming pipe builder
      ObservesSagaConnectorFactory (class):  CreateMessageConnector, GetFilterExpression, ObservesSagaConnectorFactory
      OptionsSet (class):  Options, OptionsSet, SelectOptions, TryGetOptions
      OrchestratesSagaConnectorFactory (class):  CreateMessageConnector, OrchestratesSagaConnectorFactory
      OutboxConsumePipeSpecificationObserver (class):  ActivityConfigured, AddScopedFilter, CompensateActivityConfigured, ConsumerConfigured, ConsumerMessageConfigured, ExecuteActivityConfigured, OutboxConsumePipeSpecificationObserver, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
      ParentConsumePipeSpecificationObserver (class):  MessageSpecificationCreated, ParentConsumePipeSpecificationObserver
      ParentPublishPipeSpecificationObserver (class):  MessageSpecificationCreated, ParentPublishPipeSpecificationObserver
      ParentSendPipeSpecificationObserver (class):  MessageSpecificationCreated, ParentSendPipeSpecificationObserver
      PartitionConsumerSpecification (class):  Apply, PartitionConsumerSpecification, Validate
      PartitionKeyMessageSendTopologyConvention (class):  PartitionKeyMessageSendTopologyConvention, SetFormatter, TryGetMessageSendTopology, TryGetMessageSendTopologyConvention
      PartitionKeySendTopologyConvention (class):  PartitionKeySendTopologyConvention, TryGetMessageSendTopologyConvention
      PartitionMessageConfigurationObserver (class):  BatchConsumerConfigured, MessageConfigured, Method4, Method5, Method6, PartitionMessageConfigurationObserver
      PartitionMessageSpecification (class):  Apply, PartitionMessageSpecification, Validate
      PartitionSagaSpecification (class):  Apply, PartitionSagaSpecification, Validate
      PartitionerPipeSpecification (class):  Apply, PartitionerPipeSpecification, Validate
      PauseScheduledMessageConsumerDefinition (class):  PauseScheduledMessageConsumerDefinition
      PauseScheduledRecurringMessageConsumerDefinition (class):  PauseScheduledRecurringMessageConsumerDefinition
      PipeBuilder (class):  AddFilter, Build, Method1, Method2, PipeBuilder
      PipeConfigurator (class):  AddPipeSpecification, Build, Method1, Method2, PipeConfigurator, Validate
      PropertyCorrelationIdSelector (class):  PropertyCorrelationIdSelector, TryGetSetCorrelationId
      PropertySagaInstanceFactory (class):  PropertySagaInstanceFactory
         Creates a saga instance using the constructor, via a compiled expression.
      Proxy (class):  Apply, Configure, Proxy, TryGetPublishAddress
      PublishMessageSchedulerPipeSpecification (class):  Apply, Validate
      PublishPipeConfiguration (class):  CreatePipe, PublishPipeConfiguration
      PublishPipeSpecification (class):  AddPipeSpecification, ConnectPublishPipeSpecificationObserver, CreateMessageSpecification, FilterContext, GetMessageSpecification, MergeContext, PublishPipeSpecification, Validate
      PublishPipeSpecificationObservable (class):  MessageSpecificationCreated, Method4, Method5, Method6
      PublishToSendTopologyConfigurationObserver (class):  MessageTopologyCreated, PublishToSendTopologyConfigurationObserver
      PublishTopologyConfigurationObservable (class):  MessageTopologyCreated, Method4, Method5, Method6
      QuartzEndpointDefinition (class):  Configure, GetEndpointName, QuartzEndpointDefinition
      QuerySagaMessageConnector (class):  QuerySagaMessageConnector
      RateLimitPipeSpecification (class):  Apply, RateLimitPipeSpecification, Validate
      ReceiveContextRescuePipeSpecification (class):  Apply, ReceiveContextRescuePipeSpecification, Validate
      ReceiveEndpointBuilder (class):  ConnectConsumePipe, ReceiveEndpointBuilder
      ReceiveEndpointConfiguration (class):  AddDependency, AddDependent, AddEndpointSpecification, ConfigureMessageTopology, ConnectReceiveEndpointObserver, CreateReceiveEndpointContext, CreateReceivePipe, Validate
      ReceivePipeConfiguration (class):  AddPipeSpecification, CreateDeadLetterPipe, CreateErrorPipe, CreatePipe, ReceivePipeConfiguration, Validate
      ReceivePipeDispatcherConfiguration (class):  Build, ConnectReceiveEndpointObserver, ReceivePipeDispatcherConfiguration
      ReceiverConfiguration (class):  AddDependency, AddDependent, AddEndpointSpecification, ConfigureMessageTopology, ConnectReceiveEndpointObserver, Validate
      RedeliverRequestStateMachineSpecification (class):  Apply, RedeliverRequestStateMachineSpecification
      RedeliveryRetryPipeSpecification (class):  Apply, ConnectRetryObserver, Factory, RedeliveryRetryPipeSpecification, SetRetryPolicy, Validate
      RedisSagaRepositoryConfigurator (class):  ConnectionFactory, DatabaseConfiguration, RedisSagaRepositoryConfigurator, Register, SelectDatabase, SelectDefaultDatabase, Validate
      RedisSagaRepositoryRegistrationProvider (class):  Configure, RedisSagaRepositoryRegistrationProvider
      RegistrationConfigurator (class):  Add, AddActivity, AddConsumer, AddEndpoint, AddExecuteActivity, AddFuture, AddRequestClient, AddSaga, AddSagaRepository, AddSagaStateMachine, Clear, Complete, Contains, CopyTo, GetEnumerator
         Used for registration of consumers and sagas
      RegistrationContext (class):  ConfigureActivity, ConfigureActivityCompensate, ConfigureActivityExecute, ConfigureConsumer, ConfigureConsumers, ConfigureExecuteActivity, ConfigureFuture, ConfigureSaga, ConfigureSagas, GetService, PushContext, RegistrationContext
      RegistrationContextEndpointDefinition (class):  Configure, GetEndpointName, RegistrationContextEndpointDefinition
      RegistrationServiceCollectionExtensions (class):  RegisterLoadSagaRepository, RegisterQuerySagaRepository, RegisterSagaRepository
      RescuePipeSpecification (class):  AddPipeSpecification, Apply, RescuePipeSpecification, Validate
      ResumeScheduledMessageConsumerDefinition (class):  ResumeScheduledMessageConsumerDefinition
      ResumeScheduledRecurringMessageConsumerDefinition (class):  ResumeScheduledRecurringMessageConsumerDefinition
      RetryPipeSpecification (class):  Apply, ConnectRetryObserver, RetryPipeSpecification, SetRetryPolicy, Validate
      Rider (class)
      RiderRegistrationContext (class):  ConfigureActivity, ConfigureActivityCompensate, ConfigureActivityExecute, ConfigureConsumer, ConfigureConsumers, ConfigureExecuteActivity, ConfigureFuture, ConfigureSaga, ConfigureSagas, GetRegistrations, GetService, PushContext, RiderRegistrationContext
      RoutingKeyMessageSendTopologyConvention (class):  RoutingKeyMessageSendTopologyConvention, SetFormatter, TryGetMessageSendTopology, TryGetMessageSendTopologyConvention
      RoutingKeySendTopologyConvention (class):  RoutingKeySendTopologyConvention, TryGetMessageSendTopologyConvention
      RoutingSlipConfigurator (class):  AddPipeSpecification, Build, RoutingSlipConfigurator, Validate
      SagaConfigurationObservable (class):  Method4, Method5, Method6, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured
      SagaConfigurator (class):  AddPipeSpecification, Configure, ConnectSagaConfigurationObserver, Message, Options, SagaConfigurator, SagaMessage, SelectOptions, TryGetOptions, Validate
      SagaConnector (class):  ConnectSaga, CreateSagaSpecification, Initiates, InitiatesOrOrchestrates, Observes, Orchestrates, SagaConnector
      SagaConnectorCache (class):  SagaConnectorCache
         Caches the saga connectors for the saga
      SagaConsumeContextRescuePipeSpecification (class):  Apply, SagaConsumeContextRescuePipeSpecification, Validate
      SagaDefinitionRegistrar (class):  Register
      SagaEndpointDefinition (class):  SagaEndpointDefinition
      SagaFilterSpecification (class):  Apply, SagaFilterSpecification, Validate
      SagaInterfaceType (class):  GetInitiatedByConnector, GetInitiatedByOrOrchestratesConnector, GetObservesConnector, GetOrchestratesConnector, SagaInterfaceType
      SagaMessageConfigurator (class):  AddPipeSpecification, SagaMessageConfigurator
      SagaMessageConnector (class):  ConnectSaga, CreateSagaMessageSpecification
      SagaMessageSpecification (class):  AddPipeSpecification, BuildConsumerPipe, BuildMessagePipe, ConnectSagaConfigurationObserver, GetMessageSpecification, Message, SagaMessageSpecification, Validate
         Configures the pipe for a Saga/message combination within a Saga configuration block.
      SagaMessageSplitFilterSpecification (class):  Apply, SagaMessageSplitFilterSpecification, Validate
      SagaMetadataCache (class):  GenerateFactoryMethodAsynchronously, GeneratePropertyFactoryMethodAsynchronously, GetActivatorSagaInstanceFactoryMethod, GetInitiatingOrOrchestratingTypes, GetInitiatingTypes, GetObservingTypes, GetOrchestratingTypes, SagaMetadataCache
      SagaPipeSpecificationProxy (class):  Apply, SagaPipeSpecificationProxy, Validate
      SagaQueryPropertySelector (class):  SagaQueryPropertySelector, TryGetProperty
      SagaRegistrar (class):  Register
      SagaRepositoryRegistrationProvider (class):  Configure
      SagaSpecification (class):  AddPipeSpecification, ConfigureMessagePipe, ConnectSagaConfigurationObserver, GetMessageSpecification, Message, SagaMessage, SagaSpecification, Validate
      SagaSplitFilterSpecification (class):  Apply, SagaSplitFilterSpecification, Validate
      ScheduleMessageConsumerDefinition (class):  ScheduleMessageConsumerDefinition
      ScheduleRecurringMessageConsumerDefinition (class):  ScheduleRecurringMessageConsumerDefinition
      ScheduledRedeliveryConfigurationObserver (class):  ActivityConfigured, BatchConsumerConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, MessageConfigured, Method4, Method5, Method6, ScheduledRedeliveryConfigurationObserver
      ScheduledRedeliveryConsumerConfigurationObserver (class):  ConsumerConfigured, ConsumerMessageConfigured, ScheduledRedeliveryConsumerConfigurationObserver
         Configures scheduled message redelivery for a consumer, on the consumer configurator, which is constrained to the mes...
      ScheduledRedeliveryHandlerConfigurationObserver (class):  HandlerConfigured, ScheduledRedeliveryHandlerConfigurationObserver
         Configures a message retry for a handler, on the handler configurator, which is constrained to the message types for ...
      ScheduledRedeliveryPipeSpecification (class):  Apply, Validate
      ScheduledRedeliverySagaConfigurationObserver (class):  SagaConfigured, SagaMessageConfigured, ScheduledRedeliverySagaConfigurationObserver, StateMachineSagaConfigured
         Configures scheduled message redelivery for a saga, on the saga configurator, which is constrained to the message typ...
      ScopedCompensateActivityPipeSpecificationObserver (class):  ActivityConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, ScopedCompensateActivityPipeSpecificationObserver
      ScopedConsumePipeSpecificationObserver (class):  AddScopedFilter, ConsumerConfigured, ConsumerMessageConfigured, SagaConfigured, SagaMessageConfigured, ScopedConsumePipeSpecificationObserver, StateMachineSagaConfigured
      ScopedExecuteActivityPipeSpecificationObserver (class):  ActivityConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, ScopedExecuteActivityPipeSpecificationObserver
      ScopedFilterSpecificationObserver (class):  AddScopedFilter, MessageSpecificationCreated, ScopedFilterSpecificationObserver
      SendMessageFilterConfigurator (class):  Exclude, Include, Match, SendMessageFilterConfigurator
      SendPipeConfiguration (class):  CreatePipe, SendPipeConfiguration
      SendPipeSpecification (class):  AddPipeSpecification, ConnectSendPipeSpecificationObserver, CreateMessageSpecification, GetMessageSpecification, SendPipeSpecification, Validate
      SendPipeSpecificationObservable (class):  MessageSpecificationCreated, Method4, Method5, Method6
      SendTopologyConfigurationObservable (class):  MessageTopologyCreated, Method4, Method5, Method6
      SendTransformSpecification (class):  Apply
      SerializationConfiguration (class):  AddDeserializer, AddSerializer, AddSystemTextJson, Clear, CreateCollection, CreateSerializationConfiguration, CreateSerializerCollection, SerializationConfiguration, Validate
      ServiceBusBusFactoryConfigurator (class):  CreateBusEndpointConfiguration, EnableDuplicateDetection, Host, OverrideDefaultBusEndpointQueueName, Publish, ReceiveEndpoint, Send, ServiceBusBusFactoryConfigurator, SetNamespaceSeparatorTo, SetNamespaceSeparatorToTilde, SetNamespaceSeparatorToUnderscore, SubscriptionEndpoint
      ServiceBusHostConfigurator (class):  IsMissingCredentials, ParseEndpoint, ServiceBusHostConfigurator
      ServiceBusMessageSchedulerSpecification (class):  Apply, Validate
      ServiceBusReceiveEndpointBuilder (class):  BuildTopology, ClientContextFactory, ConnectConsumePipe, CreateReceiveEndpointContext, GenerateSubscriptionName, ServiceBusReceiveEndpointBuilder
      ServiceBusSubscriptionEndpointBuilder (class):  BuildTopology, ClientContextFactory, CreateReceiveEndpointContext, ServiceBusSubscriptionEndpointBuilder
      ServiceCollectionBusConfigurator (class):  AddBus, AddConfigureEndpointsCallback, AddMassTransitComponents, AddRider, CreateBus, DefaultClientFactory, ServiceCollectionBusConfigurator, SetBusFactory, SetRequestClientFactory
      ServiceCollectionMediatorConfigurator (class):  AddMassTransitComponents, ConfigureMediator, MediatorFactory, ServiceCollectionMediatorConfigurator
      ServiceCollectionRiderConfigurator (class):  ServiceCollectionRiderConfigurator, SetRiderFactory, TryAddScoped
      ServiceInstanceConfigurator (class):  AddSpecification, ConnectEndpointConfigurationObserver, Options, ReceiveEndpoint, SelectOptions, ServiceInstanceConfigurator, TryGetOptions
      SetCorrelationIdSelector (class):  SetCorrelationIdSelector, TryGetSetCorrelationId
      SetPartitionKeyMessageSendTopology (class):  Apply, SetPartitionKeyMessageSendTopology
      SetRoutingKeyMessageSendTopology (class):  Apply, SetRoutingKeyMessageSendTopology
      SetSerializerMessageSendTopologyConvention (class):  SetSerializer, TryGetMessageSendTopology, TryGetMessageSendTopologyConvention
      Settings (class):  Settings
      SettingsEndpointDefinition (class):  Configure, GetEndpointName
      SpecificationPipeBuilder (class):  AddFilter, Build, CreateDelegatedBuilder, CreateImplementedBuilder, SpecificationPipeBuilder
      SplitFilterPipeSpecification (class):  Apply, SplitFilterPipeSpecification, Validate
         Adds an arbitrary filter to the pipe
      StateMachineEventActivitiesBuilder (class):  AfterLeave, AfterLeaveAny, Apply, BeforeEnter, BeforeEnterAny, CommitActivities, CompositeEvent, During, DuringAny, Event, Finally, Ignore, Initially, InstanceState, Name
      StateMachineEventConnectorFactory (class):  CreateMessageConnector, StateMachineEventConnectorFactory
      StateMachineInterfaceType (class):  GetConnector, StateMachineInterfaceType
      StateMachineModifier (class):  AfterLeave, AfterLeaveAny, Apply, BeforeEnter, BeforeEnterAny, CompositeEvent, During, DuringAny, Event, Finally, Initially, InstanceState, Name, OnUnhandledEvent, State
      StateMachineRequestConfigurator (class):  StateMachineRequestConfigurator
      StateMachineSagaMessageConnector (class):  StateMachineSagaMessageConnector
      StateMachineScheduleConfigurator (class):  StateMachineScheduleConfigurator
      SystemTextJsonMessageSerializerFactory (class):  CreateDeserializer, CreateSerializer, SystemTextJsonMessageSerializerFactory
      SystemTextJsonRawMessageSerializerFactory (class):  CreateDeserializer, CreateSerializer, SystemTextJsonRawMessageSerializerFactory
      TempSagaRepository (class):  Find, Load, Probe, Send, SendQuery, TempSagaRepository
      TestHarnessRegistrationConfigurator (class):  Add, AddActivity, AddBus, AddConfigureEndpointsCallback, AddConsumer, AddEndpoint, AddExecuteActivity, AddFuture, AddRequestClient, AddRider, AddSaga, AddSagaRepository, AddSagaStateMachine, Clear, Contains
      TimeoutConfigurationObserver (class):  ActivityConfigured, BatchConsumerConfigured, CompensateActivityConfigured, ExecuteActivityConfigured, MessageConfigured, Method4, Method5, Method6, TimeoutConfigurationObserver
      TimeoutConsumerConfigurationObserver (class):  ConsumerConfigured, ConsumerMessageConfigured, TimeoutConsumerConfigurationObserver
      TimeoutHandlerConfigurationObserver (class):  HandlerConfigured, TimeoutHandlerConfigurationObserver
      TimeoutSagaConfigurationObserver (class):  SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured, TimeoutSagaConfigurationObserver
      TimeoutSpecification (class):  Apply, Factory, Validate
      TopologyConsumePipeSpecificationObserver (class):  MessageSpecificationCreated, TopologyConsumePipeSpecificationObserver
      TopologyConventionCache (class):  GetOrAdd, TopologyConventionCache
      TopologyPublishPipeSpecificationObserver (class):  MessageSpecificationCreated, TopologyPublishPipeSpecificationObserver
      TopologySendPipeSpecificationObserver (class):  MessageSpecificationCreated, TopologySendPipeSpecificationObserver
      TransactionPipeSpecification (class):  Apply, TransactionPipeSpecification, Validate
      TransformSpecification (class):  Default, Set, Transform, Validate
      TransformSpecificationConfigurator (class):  Get
      TransportConfiguration (class):  GetConcurrentMessageLimit, TransportConfiguration, Validate
      TransportRegistrationBusFactory (class):  ConnectBusObservers, ConnectConsumeObservers, ConnectPublishObservers, ConnectReceiveEndpointObservers, ConnectReceiveObservers, ConnectSendObservers, CreateBus
      UntypedConsumerConfigurator (class):  Configure, ConnectConsumerConfigurationObserver, UntypedConsumerConfigurator, Validate
      ValidateSpecification (class):  Configure, Validate, ValidateSpecification
      ValueTypeGroupKeyProvider (class):  TryGetKey, ValueTypeGroupKeyProvider
   MassTransit.Consumer
      DefaultConstructorConsumerFactory (class):  Probe, Send
      DelegateConsumerFactory (class):  DelegateConsumerFactory, Probe, Send
      InstanceConsumerFactory (class):  InstanceConsumerFactory, Probe, Send
         Retains a reference to an existing message consumer, and uses it to send consumable messages for processing.
      ObjectConsumerFactory (class):  ObjectConsumerFactory, Probe, Send
   MassTransit.Context
      BaseConsumeContext (class):  AddConsumeTask, AddOrUpdatePayload, ConnectSendObserver, ConsumeTask, GetOrAddPayload, GetSendEndpoint, HasMessageType, HasPayloadType, NotifyConsumed, NotifyFaulted, Respond, RespondAsync, RespondInternal, TryGetMessage, TryGetPayload
      BaseCourierContext (class):  Method4, Method5, Method6
      BatchConsumeContext (class):  BatchConsumeContext, Method4, Method5, Method6, NotifyConsumed, NotifyFaulted
      BindContextProxy (class):  AddOrUpdatePayload, BindContextProxy, GetOrAddPayload, HasPayloadType, TryGetPayload
         The BindContext
      Cached (class)
      CompensateContextProxy (class):  CompensateContextProxy, Compensated, CreateActivityContext, Failed
      CompensateContextScope (class):  CompensateContextScope, Compensated, CreateActivityContext, Failed
      ConsumeContextProxy (class):  AddConsumeTask, AddOrUpdatePayload, ConsumeContextProxy, GetOrAddPayload, HasMessageType, HasPayloadType, Method1, Method2, Method3, NotifyConsumed, NotifyFaulted, TryGetMessage, TryGetPayload
         A consume context proxy creates a payload scope, such that anything added to the payload of the context is only added...
      ConsumeContextScope (class):  AddOrUpdatePayload, ConsumeContextScope, GetOrAddPayload, HasPayloadType, Method1, Method2, Method3, NotifyConsumed, NotifyFaulted, TryGetPayload
      ConsumeMessageSchedulerContext (class):  CancelScheduledPublish, CancelScheduledSend, ConsumeMessageSchedulerContext, SchedulePublish, ScheduleSend
      ConsumerConsumeContextProxy (class):  ConsumerConsumeContextProxy, Method4, Method5, Method6
         A consumer instance merged with a message consume context
      ConsumerConsumeContextScope (class):  ConsumerConsumeContextScope, Method4, Method5, Method6
         A consumer instance merged with a message consume context
      CorrelationIdConsumeContextProxy (class):  CorrelationIdConsumeContextProxy, Method4, Method5, Method6
         A consumer instance merged with a message consume context
      CourierContextProxy (class):  Method4, Method5, Method6
      CourierContextScope (class):  Method4, Method5, Method6
      DefaultSagaConsumeContext (class):  DefaultSagaConsumeContext, Method4, Method5, Method6, SetCompleted
      DeserializerConsumeContext (class):  AddConsumeTask, AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, TryGetPayload
      ExecuteContextProxy (class):  Completed, CompletedWithVariables, CreateActivityContext, ExecuteContextProxy, Faulted, FaultedWithVariables, ReviseItinerary, Terminate
      ExecuteContextScope (class):  Completed, CompletedWithVariables, CreateActivityContext, ExecuteContextScope, Faulted, FaultedWithVariables, ReviseItinerary, Terminate
      HostCompensateActivityContext (class):  HostCompensateActivityContext, Method7, Method8, Method9
      HostExecuteActivityContext (class):  HostExecuteActivityContext, Method7, Method8, Method9
      IPendingFault (interface):  Notify
      IPublishEndpointConverter (interface):  Publish, PublishInitializer
         Calls the generic version of the IPublishEndpoint.Send method with the object's type
      IResponseEndpointConverter (interface):  Respond
         Calls the generic version of the ISendEndpoint.Send method with the object's type
      ISendEndpointConverter (interface):  Send, SendInitializer
         Calls the generic version of the ISendEndpoint.Send method with the object's type
      MediatorConsumeContext (class):  HasMessageType, MediatorConsumeContext, NotifyConsumed, NotifyFaulted, TryGetMessage
      MediatorSendMessageContext (class):  MediatorSendMessageContext
      MessageConsumeContext (class):  AddConsumeTask, AddOrUpdatePayload, ConnectPublishObserver, ConnectSendObserver, ConsumeTask, GetOrAddPayload, GetSendEndpoint, HasMessageType, HasPayloadType, MessageConsumeContext, NotifyConsumed, NotifyFaulted, Publish, Respond, RespondAsync
      MessageSendContext (class):  CreateProxy, GetMessageBody, MessageSendContext, ReadPropertiesFrom, WritePropertiesTo
      MissingConsumeContext (class):  AddConsumeTask, AddOrUpdatePayload, ConnectPublishObserver, ConnectSendObserver, GetOrAddPayload, GetSendEndpoint, HasMessageType, HasPayloadType, NotifyConsumed, NotifyFaulted, Publish, Respond, RespondAsync, TryGetMessage, TryGetPayload
      NoLockReceiveContext (class):  Complete, Faulted, NoLockReceiveContext, ValidateLockStatus
      PendingFault (class):  Notify, PendingFault
      PendingFaultCollection (class):  Add, Notify, PendingFaultCollection
      PropertyNames (class)
      PublishContextProxy (class):  CreateProxy, PublishContextProxy
      PublishEndpointConverter (class):  Publish, PublishInitializer
         Converts the object message type to the generic type T and publishes it on the endpoint specified.
      PublishEndpointConverterCache (class):  CreateConverter, CreateTypeConverter, Publish, PublishInitializer
         Caches the converters that allow a raw object to be published using the object's type through the generic Send method.
      ReceiveContextProxy (class):  AddOrUpdatePayload, AddReceiveTask, GetOrAddPayload, HasPayloadType, NotifyConsumed, NotifyFaulted, TryGetPayload
      ResponseEndpointConverter (class):  Respond
         Converts the object type message to the appropriate generic type and invokes the send method with that generic overload.
      ResponseEndpointConverterCache (class):  CreateConverter, CreateTypeConverter, Respond
         Caches the converters that allow a raw object to be published using the object's type through the generic Send method.
      RetryCompensateContext (class):  CreateNext, NotifyPendingFaults, RetryCompensateContext
      RetryCompensationResult (class):  Evaluate, IsFailed, RetryCompensationResult
      RetryExecuteContext (class):  CreateNext, NotifyPendingFaults, RetryExecuteContext
      RetryExecutionResult (class):  Evaluate, IsFaulted, RetryExecutionResult
      SagaConsumeContextProxy (class):  Method4, Method5, Method6, SagaConsumeContextProxy, SetCompleted
         A consumer instance merged with a message consume context
      ScheduleMessageRedeliveryContext (class):  ScheduleMessageRedeliveryContext, ScheduleRedelivery
         Used to schedule message redelivery using the message scheduler
      SendContextProxy (class):  CreateProxy, SendContextProxy
      SendContextScope (class):  AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, SendContextScope, TryGetPayload
      SendEndpointConverter (class):  Send, SendInitializer
         Converts the object type message to the appropriate generic type and invokes the send method with that generic overload.
      SendEndpointConverterCache (class):  CreateConverter, CreateTypeConverter, Send, SendInitializer
         Caches the converters that allow a raw object to be published using the object's type through the generic Send method.
      SystemTransactionContext (class):  Commit, Dispose, Rollback, SystemTransactionContext
      TransportReceiveContext (interface):  GetTransportProperties
      TransportSendContext (interface):  ReadPropertiesFrom, WritePropertiesTo
   MassTransit.Contracts
      CircuitBreakerClosed (interface)
      CircuitBreakerOpened (interface)
      CommandContext (interface)
      ConcurrencyLimitUpdated (interface)
         Published when the concurrency limit of a filter is updated.
      EventContext (interface)
      RequestCompleted (interface)
         Published by the saga when a request is completed, so that waiting requests can be completed, or redelivered to the s...
      RequestFaulted (interface)
      RequestStarted (interface)
         Published when a saga starts to process a request, but a subsequent operation (such as another request) is pending.
      RequestTimeoutExpired (interface)
      SetConcurrencyLimit (interface)
         Sets the concurrency limit of a concurrency limit filter
      SetRateLimit (interface)
         Set the rate limit of the RateLimitFilter
   MassTransit.Contracts.JobService
      AllocateJobSlot (interface)
      CancelJob (interface)
      CancelJobAttempt (interface)
      CompleteJob (interface)
      FaultJob (interface)
      FinalizeJob (interface)
      FinalizeJobAttempt (interface)
      GetJobAttemptStatus (interface)
      GetJobState (interface)
      JobAttemptCanceled (interface)
      JobAttemptCompleted (interface)
      JobAttemptFaulted (interface)
      JobAttemptStarted (interface)
      JobAttemptStatus (interface)
      JobCanceled (interface)
         Published when a job is canceled
      JobCancellationReasons (class)
      JobCompleted (interface)
         Published when a job completes
      JobFaulted (interface)
         Published when a job faults
      JobRetryDelayElapsed (interface)
      JobSlotAllocated (interface)
      JobSlotReleased (interface)
      JobSlotUnavailable (interface)
      JobSlotWaitElapsed (interface)
      JobStarted (interface)
         Event published when a node starts processing a job
      JobState (interface)
      JobStatusCheckRequested (interface)
         Signals that the time to supervise a job has expired, and the instance should be checked
      JobSubmissionAccepted (interface)
      JobSubmitted (interface)
      RecurringJobSchedule (interface)
      RetryJob (interface)
      RunJob (interface)
         Run a scheduled job immediately, vs waiting for the next scheduled job time
      SaveJobState (interface)
      SetConcurrentJobLimit (interface)
         When the bus is started, the current job limit for a job type is published along with the instance address.
      SetJobProgress (interface)
      StartJob (interface)
      StartJobAttempt (interface)
      SubmitJob (interface)
   MassTransit.Courier
      ActivityFactoryCache (class)
      CompensateActivityHost (class):  CompensateActivityHost, Probe, Send
      DefaultConstructorCompensateActivityFactory (class)
      DefaultConstructorExecuteActivityFactory (class)
      ExecuteActivityHost (class):  ExecuteActivityHost, Probe, Send
      FactoryMethodActivityFactory (class):  Compensate, Execute, FactoryMethodActivityFactory, Probe
      FactoryMethodCompensateActivityFactory (class):  Compensate, FactoryMethodCompensateActivityFactory, Probe
      FactoryMethodExecuteActivityFactory (class):  Execute, FactoryMethodExecuteActivityFactory, Probe
      HostCompensateContext (class):  Compensated, CreateActivityContext, Failed, HostCompensateContext
      HostExecuteContext (class):  Completed, CompletedWithVariables, CreateActivityContext, Faulted, FaultedWithVariables, HostExecuteContext, ReviseItinerary, Terminate
      IRoutingSlipEventPublisher (interface):  PublishRoutingSlipActivityCompensated, PublishRoutingSlipActivityCompensationFailed, PublishRoutingSlipActivityCompleted, PublishRoutingSlipActivityFaulted, PublishRoutingSlipCompleted, PublishRoutingSlipFaulted, PublishRoutingSlipRevised, PublishRoutingSlipTerminated
      IRoutingSlipSendEndpointTarget (interface):  AddSubscription
      MessageEnvelopeContextAdapter (class):  MessageEnvelopeContextAdapter, Probe, Send
      RoutingSlipBuilderSendEndpoint (class):  ConnectSendObserver, RoutingSlipBuilderSendEndpoint, Send
      RoutingSlipEventPublisher (class):  PublishEvent, PublishRoutingSlipActivityCompensated, PublishRoutingSlipActivityCompensationFailed, PublishRoutingSlipActivityCompleted, PublishRoutingSlipActivityFaulted, PublishRoutingSlipCompleted, PublishRoutingSlipFaulted, PublishRoutingSlipRevised, PublishRoutingSlipTerminated, PublishSubscriptionEvent, RoutingSlipEventPublisher
      RoutingSlipExecutor (class):  Execute, RoutingSlipExecutor
      RoutingSlipRequestInfo (struct):  RoutingSlipRequestInfo
      RoutingSlipRequestProxy (class):  Consume
      RoutingSlipRequestVariableNames (class)
      RoutingSlipResponseProxy (class):  CanRetry, Consume
      RoutingSlipSendContext (class):  GetMessageEnvelope, RoutingSlipSendContext
      SanitizedRoutingSlip (class):  GetActivityArguments, GetCompensateLogData, SanitizedRoutingSlip
         A sanitized routing slip is one that has been read from and ensured to be safe for use, cleaning up any missing or nu...
   MassTransit.Courier.Contracts
      Activity (interface)
      ActivityException (interface)
         Capture the exception information thrown by an activity
      ActivityLog (interface)
         Message contract for storing activity log data
      CompensateLog (interface)
      RoutingSlip (interface)
         A RoutingSlip is the transport-level interface that is used to carry the details of a message routing slip over the n...
      RoutingSlipActivityCompensated (interface)
      RoutingSlipActivityCompensationFailed (interface)
      RoutingSlipActivityCompleted (interface)
      RoutingSlipActivityFaulted (interface)
      RoutingSlipCompensationFailed (interface)
      RoutingSlipCompleted (interface)
         Published when a routing slip completes
      RoutingSlipFaulted (interface)
         Published when a routing slip faults (after compensation)
      RoutingSlipRevised (interface)
         Published when a routing slip is revised during execution
      RoutingSlipTerminated (interface)
         Published when a routing slip is terminated
      Subscription (interface)
         A routing slip subscription defines a specific endpoint where routing slip events should be sent (not published).
   MassTransit.Courier.Messages
      RoutingSlipActivity (class):  RoutingSlipActivity
      RoutingSlipActivityCompensatedMessage (class):  RoutingSlipActivityCompensatedMessage
      RoutingSlipActivityCompensationFailedMessage (class):  RoutingSlipActivityCompensationFailedMessage
      RoutingSlipActivityCompletedMessage (class):  RoutingSlipActivityCompletedMessage
      RoutingSlipActivityException (class):  RoutingSlipActivityException
      RoutingSlipActivityFaultedMessage (class):  RoutingSlipActivityFaultedMessage
      RoutingSlipActivityLog (class):  RoutingSlipActivityLog
      RoutingSlipCompensateLog (class):  RoutingSlipCompensateLog
      RoutingSlipCompensationFailedMessage (class):  RoutingSlipCompensationFailedMessage
      RoutingSlipCompletedMessage (class):  RoutingSlipCompletedMessage
      RoutingSlipFaultedMessage (class):  RoutingSlipFaultedMessage
      RoutingSlipRevisedMessage (class):  RoutingSlipRevisedMessage
      RoutingSlipRoutingSlip (class):  RoutingSlipRoutingSlip
      RoutingSlipSubscription (class):  RoutingSlipSubscription
      RoutingSlipTerminatedMessage (class):  RoutingSlipTerminatedMessage
   MassTransit.Courier.Results
      BaseExecutionResult (class):  Evaluate, IsFaulted, SetVariable, SetVariables
      CompensatedCompensationResult (class):  CompensatedCompensationResult, Evaluate, HasMoreCompensations, IsFailed, SetVariable, SetVariables, SkipLast
      CompletedExecutionResult (class):  CompletedExecutionResult, Evaluate, HasNextActivity, SetLog
      FailedCompensationResult (class):  Evaluate, FailedCompensationResult, IsFailed
      FaultedExecutionResult (class):  CreateRoutingSlipBuilder, Evaluate, FaultedExecutionResult, HasCompensationLogs, IsFaulted
      ReviseItineraryExecutionResult (class):  ReviseItineraryExecutionResult
      TerminateExecutionResult (class):  TerminateExecutionResult
   MassTransit.DapperIntegration.Configuration
      DapperSagaRepositoryConfigurator (class):  DapperSagaRepositoryConfigurator, Register, Validate
   MassTransit.DapperIntegration.Saga
      DapperDatabaseContext (class):  Commit, DapperDatabaseContext, DeleteAsync, DisposeAsync, GetTableName, InsertAsync, LoadAsync, QueryAsync, UpdateAsync
      DapperOptions (class):  DapperOptions
      DapperSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, DapperSagaRepositoryContext, Delete, Discard, Insert, Load, Query, Save, Undo, Update
      DapperSagaRepositoryContextFactory (class):  CreateDatabaseContext, DapperSagaRepositoryContextFactory, Execute, ExecuteAsyncMethod, Probe, Send, SendQuery
      DatabaseContext (interface):  Commit, DeleteAsync, InsertAsync, LoadAsync, QueryAsync, UpdateAsync
      SqlExpressionVisitor (class):  AndAlsoVisit, CreateFromExpression, EqualVisit, LambdaVisit, MemberAccessVisit
      WhereStatementHelper (class):  GetWhereStatementAndParametersFromExpression
   MassTransit.DependencyInjection
      BaseConsumeScopeProvider (class)
      Bind (class):  Bind, Create, Equals, GetHashCode
         Bind is used to store types bound to their owner, such as an IBusControl to an IMyBus.
      BusInstanceBuilder (class):  BusInstanceBuilder, CreateImplementation, CreateTypeFromInterface, GetBusInstanceType, GetGetMethodBuilder, GetModuleBuilderForType, GetSetMethodBuilder
      BusScopedBusContext (class):  BusScopedBusContext
      Cached (class)
      CombinedDisposable (class):  CombinedDisposable, Dispose
      CompensateActivityScopeProvider (class):  CompensateActivityScopeProvider, CreatedActivityScopeContextFactory, CreatedScopeContextFactory, ExistingActivityScopeContextFactory, ExistingScopeContextFactory, GetActivityScope, GetScope, PipeContextFactory, Probe
      ConsumeContextScopedBusContext (class):  ConsumeContextScopedBusContext
      ConsumeScopeProvider (class):  ConsumeScopeProvider, CreatedScopeContextFactory, ExistingScopeContextFactory, GetScope, PipeContextFactory, Probe
      CreatedCompensateActivityScopeContext (class):  CreatedCompensateActivityScopeContext, DisposeAsync, GetService
      CreatedCompensateScopeContext (class):  CreatedCompensateScopeContext, DisposeAsync, GetService
      CreatedConsumeScopeContext (class):  CreateInstance, CreatedConsumeScopeContext, DisposeAsync, GetService, PushConsumeContext
      CreatedConsumerConsumeScopeContext (class):  CreatedConsumerConsumeScopeContext, DisposeAsync
      CreatedExecuteActivityScopeContext (class):  CreatedExecuteActivityScopeContext, DisposeAsync, GetService
      CreatedExecuteScopeContext (class):  CreatedExecuteScopeContext, DisposeAsync, GetService
      DependencyInjectionFilterScopeContext (class):  DependencyInjectionFilterScopeContext, DisposeAsync
      DependencyInjectionLoadSagaRepository (class):  DependencyInjectionLoadSagaRepository
      DependencyInjectionLoadSagaRepositoryContextFactory (class):  DependencyInjectionLoadSagaRepositoryContextFactory, Execute, Probe
      DependencyInjectionQuerySagaRepository (class):  DependencyInjectionQuerySagaRepository
      DependencyInjectionQuerySagaRepositoryContextFactory (class):  DependencyInjectionQuerySagaRepositoryContextFactory, Execute, Probe
      DependencyInjectionSagaRepository (class):  DependencyInjectionSagaRepository, Probe, Send, SendQuery
      DependencyInjectionSagaRepositoryContextFactory (class):  DependencyInjectionSagaRepositoryContextFactory, Probe, Send, SendQuery
      ExecuteActivityScopeProvider (class):  CreatedActivityScopeContextFactory, CreatedScopeContextFactory, ExecuteActivityScopeProvider, ExistingActivityScopeContextFactory, ExistingScopeContextFactory, GetActivityScope, GetScope, PipeContextFactory, Probe
      ExistingCompensateActivityScopeContext (class):  DisposeAsync, ExistingCompensateActivityScopeContext, GetService
      ExistingCompensateScopeContext (class):  DisposeAsync, ExistingCompensateScopeContext, GetService
      ExistingConsumeScopeContext (class):  CreateInstance, DisposeAsync, ExistingConsumeScopeContext, GetService, PushConsumeContext
      ExistingConsumerConsumeScopeContext (class):  DisposeAsync, ExistingConsumerConsumeScopeContext
      ExistingExecuteActivityScopeContext (class):  DisposeAsync, ExistingExecuteActivityScopeContext, GetService
      ExistingExecuteScopeContext (class):  DisposeAsync, ExistingExecuteScopeContext, GetService
      FilterScopeProvider (class):  Create, FilterScopeProvider, Probe
         Used by Send/Publish filters to send within either a scoped endpoint/request client context or within the consume con...
      GenericRequestClient (class):  Create, GenericRequestClient, GetRequestClient, GetResponse
      IBusInstanceBuilder (interface):  GetBusInstanceType
      IBusInstanceBuilderCallback (interface):  GetResult
      ICompensateActivityScopeContext (interface):  GetService
      ICompensateActivityScopeProvider (interface):  GetActivityScope, GetScope
      ICompensateScopeContext (interface):  GetService
      IConsumeScopeContext (interface):  CreateInstance, GetService, PushConsumeContext
      IConsumeScopeProvider (interface):  GetScope
         Provides container scope for the consumer, either at the general level or the message-specific level.
      IConsumerConsumeScopeContext (interface)
      IExecuteActivityScopeContext (interface):  GetService
      IExecuteActivityScopeProvider (interface):  GetActivityScope, GetScope
      IExecuteScopeContext (interface):  GetService
      IFilterScopeContext (interface)
      IFilterScopeProvider (interface):  Create
      IRegistrationRiderFactory (interface):  CreateRider
      IScopedBusContextProvider (interface)
      IScopedConsumeContextProvider (interface):  GetContext, PushContext
      LegacySetScopedConsumeContext (class):  LegacySetScopedConsumeContext, PushContext
      MessageHandlerConsumer (class):  Consume, MessageHandlerConsumer
      MessageHandlerConsumerDefinition (class):  Configure, GetEndpointName
      MessageHandlerMethod (class):  MessageHandlerMethod
      NoopScope (class):  Dispose, NoopScope
      PushedContext (class):  Dispose, PushedContext
      RequestHandlerConsumer (class):  Consume, RequestHandlerConsumer
      RequestHandlerMethod (class):  RequestHandlerMethod
      ScopeCompensateActivityFactory (class):  Compensate, Probe, ScopeCompensateActivityFactory
         A factory to create an activity from Autofac, that manages the lifetime scope of the activity
      ScopeConsumerFactory (class):  Probe, ScopeConsumerFactory, Send
      ScopeExecuteActivityFactory (class):  Execute, Probe, ScopeExecuteActivityFactory
         A factory to create an activity from Autofac, that manages the lifetime scope of the activity
      ScopedBusContext (interface)
      ScopedBusContextProvider (class):  ScopedBusContextProvider
         Captures the bus context for the current scope as a scoped provider, so that it can be resolved by components at runt...
      ScopedClientFactoryContext (class):  ConnectConsumePipe, ConnectRequestPipe, GetRequestEndpoint, ScopedClientFactoryContext
      ScopedConsumeContextProvider (class):  GetContext, PopContext, PushContext
         Captures the for the current message as a scoped provider, so that it can be resolved by components at runtime (since...
      ScopedConsumePublishEndpointProvider (class):  ConnectPublishObserver, GetPublishSendEndpoint, ScopedConsumePublishEndpointProvider
      ScopedConsumeSendEndpointProvider (class):  ConnectSendObserver, GetSendEndpoint, ScopedConsumeSendEndpointProvider
      ScopedMediator (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectPublishObserver, ConnectRequestPipe, CreateRequest, CreateRequestClient, GetPublishSendEndpoint, Publish, PublishInternal, ScopedMediator
      ScopedPublishEndpointProvider (class):  ConnectPublishObserver, GetPublishSendEndpoint, ScopedPublishEndpointProvider
      ScopedRequestSendEndpoint (class):  ScopedRequestSendEndpoint, Send
      ScopedSendEndpoint (class):  ScopedSendEndpoint
      ScopedSendEndpointProvider (class):  ConnectSendObserver, GetSendEndpoint, ScopedSendEndpointProvider
      ScopedSendPipeAdapter (class):  ScopedSendPipeAdapter
      SetScopedConsumeContext (class):  PushContext, SetScopedConsumeContext
      TransactionalScopedBusContextProvider (class):  TransactionalScopedBusContextProvider
      TypedScopedConsumeContextProvider (class):  PushContext, TypedScopedConsumeContextProvider
      ValidateMassTransitHostOptions (class):  Validate
   MassTransit.DependencyInjection.Registration
      ActivityRegistration (class):  ActivityRegistration, AddConfigureAction, Configure, ConfigureCompensate, ConfigureExecute, GetActivityDefinition, GetDefinition
      ActivityRegistrationConfigurator (class):  ActivityRegistrationConfigurator, CompensateEndpoint, Endpoints, ExcludeFromConfigureEndpoints, ExecuteEndpoint
      Cached (class)
      CachedRegistration (interface):  Register
      ConsumerRegistration (class):  AddConfigureAction, Configure, ConsumerRegistration, GetConsumerDefinition, GetConsumerRegistrationConfigurator, GetDefinition
         A consumer registration represents a single consumer, which will be resolved from the container using the scope provi...
      ConsumerRegistrationConfigurator (class):  ConsumerRegistrationConfigurator, Endpoint, ExcludeFromConfigureEndpoints
      DefaultBusInstance (class):  Connect, ConnectReceiveEndpoint, DefaultBusInstance, GetRider
      DefaultFutureDefinition (class)
      EndpointRegistration (class):  EndpointRegistration, GetDefinition
      ExecuteActivityRegistration (class):  AddConfigureAction, Configure, ExecuteActivityRegistration, GetActivityDefinition, GetDefinition
      ExecuteActivityRegistrationConfigurator (class):  Endpoint, ExcludeFromConfigureEndpoints, ExecuteActivityRegistrationConfigurator
      FutureRegistration (class):  Configure, FutureRegistration, GetDefinition, GetFutureDefinition
      FutureRegistrationConfigurator (class):  Endpoint, ExcludeFromConfigureEndpoints, FutureRegistrationConfigurator, Repository
      FutureRequestConsumerDefinition (class)
      IConsumerFactoryDecoratorRegistration (interface):  DecorateConsumerFactory
      IRegistrationFilter (interface):  Matches
      IRegistrationFilterConfigurator (interface):  Exclude, Include
         Specify the consumer, saga, and activity types to include/exclude
      JobSagaRegistrationConfigurator (class):  Endpoints, JobAttemptEndpoint, JobEndpoint, JobSagaRegistrationConfigurator, JobTypeEndpoint, UseRepositoryRegistrationProvider
      JobServiceRegistration (class):  AddConfigureAction, AddReceiveEndpointDependency, Configure, ConfigureJobConsumerEndpoint, GetJobServiceSettings, JobServiceRegistration
      JobServiceRegistrationConfigurator (class):  Endpoint, JobServiceRegistrationConfigurator, Options
      RegistrationBusFactory (class):  CreateBus, RegistrationBusFactory
      RegistrationCache (class):  GetOrAdd, RegistrationCache
      RegistrationFilter (class):  Matches, RegistrationFilter
      RegistrationFilterConfigurator (class):  Exclude, Include, RegistrationFilterConfigurator
      RequestClientRegistrationCache (class):  Factory, Register
      RequestConsumerFutureDefinition (class):  RequestConsumerFutureDefinition
      RequestConsumerFutureEndpointDefinition (class):  Configure, GetEndpointName, RequestConsumerFutureEndpointDefinition
      SagaRegistration (class):  AddConfigureAction, Configure, GetDefinition, GetSagaDefinition, SagaRegistration
         A saga registration represents a single saga, which will use the container for the scope provider, as well as to reso...
      SagaRegistrationConfigurator (class):  Endpoint, ExcludeFromConfigureEndpoints, Repository, SagaRegistrationConfigurator
      SagaRepositoryRegistrationConfigurator (class):  Add, Clear, Contains, CopyTo, GetEnumerator, IndexOf, Insert, Remove, RemoveAt, SagaRepositoryRegistrationConfigurator
      SagaStateMachineRegistration (class):  AddConfigureAction, Configure, GetDefinition, GetSagaDefinition, SagaStateMachineRegistration
         A saga state machine represents a state machine and instance, which will use the container to resolve, as well as the...
   MassTransit.DependencyInjection.Testing
      ConsumerContainerTestHarnessRegistration (class):  ConsumerContainerTestHarnessRegistration, DecorateConsumerFactory
      ConsumerTestHarnessRegistration (class):  ConsumerTestHarnessRegistration, DecorateConsumerFactory
      ContainerTestHarness (class):  Cancel, ContainerTestHarness, DisposeAsync, ForceInactive, GetConsumerAddress, GetConsumerEndpoint, GetConsumerHarness, GetExecuteActivityAddress, GetExecuteActivityEndpoint, GetHandlerAddress, GetHandlerEndpoint, GetRequestClient, GetSagaAddress, GetSagaEndpoint, GetSagaHarness
      ContainerTestHarnessBusObserver (class):  ContainerTestHarnessBusObserver, CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted
      SagaContainerTestHarnessRegistration (class):  DecorateSagaRepository, SagaContainerTestHarnessRegistration
      SagaTestHarnessRegistration (class):  DecorateSagaRepository, SagaTestHarnessRegistration
   MassTransit.DynamoDbIntegration.Saga
      DatabaseContext (interface):  Add, Delete, Insert, Load, Update
      DynamoDbDatabaseContext (class):  Add, BuildPutItemOperationConfig, BuildUpdateItemOperationConfig, Delete, Dispose, DynamoDbDatabaseContext, GetDynamoDbSaga, GetExpirationEpochSeconds, Insert, Load, Save, Update
      DynamoDbSaga (class):  DynamoDbSaga, ToDocument
      DynamoDbSagaRepository (class):  Create
      DynamoDbSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, Dispose, DynamoDbSagaRepositoryContext, Insert, Load, Save, Undo, Update
      DynamoDbSagaRepositoryContextFactory (class):  DynamoDbSagaRepositoryContextFactory, Execute, Probe, Send, SendQuery
   MassTransit.EntityFrameworkCoreIntegration
      BusOutboxDeliveryService (class):  BusOutboxDeliveryService, DeliverOutbox, DeliverOutboxMessages, IsTransientFailure, RemoveOutbox, RollbackTransaction
      DbContextOutboxConsumeContext (class):  AddSend, DbContextOutboxConsumeContext, LoadOutboxMessages, NotifyOutboxMessageDelivered, RemoveOutboxMessages, SetConsumed, SetDelivered
      DbTransactionContext (interface)
         Implemented when a filter/context has already started and is managing the transaction
      EntityFrameworkConsumeContextScopedBusContext (class):  EntityFrameworkConsumeContextScopedBusContext
      EntityFrameworkOutboxContextFactory (class):  EntityFrameworkOutboxContextFactory, Probe, Send
      EntityFrameworkOutboxExtensions (class):  AddSend
      EntityFrameworkSagaRepository (class):  CreateOptimistic, CreatePessimistic, CreateRepository
      EntityFrameworkScopedBusContext (class):  AddSend, Dispose, EntityFrameworkScopedBusContext, GetService, WasCommitted
      EntityFrameworkScopedBusContextProvider (class):  Dispose, EntityFrameworkScopedBusContextProvider
      FutureSagaDbContext (class):  FutureSagaDbContext
      FutureStateMap (class):  FutureStateMap
      ILoadQueryExecutor (interface):  Load
      ILockStatementFormatter (interface):  AppendColumn, Complete, Create, CreateOutboxStatement
      ILockStatementProvider (interface):  GetOutboxStatement, GetRowLockStatement
      ISagaClassMap (interface):  Configure
      ISagaDbContextFactory (interface):  Create, CreateScoped, ReleaseAsync
         Creates a DbContext for the saga repository
      ISagaRepositoryLockStrategy (interface):  CreateLockContext, Load
      InboxCleanupService (class):  CleanUpInboxState, InboxCleanupService
         The Inbox Cleanup Service is responsible for removing entries after the expiration window timeout has elapsed.
      InboxState (class)
      JobAttemptSagaMap (class):  JobAttemptSagaMap
      JobSagaMap (class):  JobSagaMap
      JobServiceSagaDbContext (class):  JobServiceSagaDbContext
      JobTypeSagaMap (class):  JobTypeSagaMap
      JsonValueComparer (class):  DoEquals, DoGetHashCode, DoGetSnapshot, Json, JsonValueComparer
      JsonValueConverter (class):  Deserialize, JsonValueConverter, Serialize
      MySqlLockStatementFormatter (class):  AppendColumn, Complete, Create, CreateOutboxStatement
      MySqlLockStatementProvider (class):  MySqlLockStatementProvider
      OptimisticFutureSagaDbContext (class):  OptimisticFutureSagaDbContext
      OptimisticJobServiceSagaDbContext (class):  OptimisticJobServiceSagaDbContext
      OracleLockStatementFormatter (class):  AppendColumn, Complete, Create, CreateOutboxStatement, FormatTableName
      OracleLockStatementProvider (class):  OracleLockStatementProvider
      OutboxMessage (class):  Deserialize, DeserializerHeaders, DeserializerProperties
      OutboxMessageStaticData (class)
      OutboxState (class)
         Used by the sweeper to track the state of an outbox, to ensure that it is properly locked across sweeper instances to...
      PostgresLockStatementFormatter (class):  AppendColumn, Complete, Create, CreateOutboxStatement, FormatTableName
      PostgresLockStatementProvider (class):  PostgresLockStatementProvider
      SagaDbContext (class)
      SqlLockStatementProvider (class):  FormatLockStatement, GetOutboxStatement, GetRowLockStatement, GetSchemaAndTableNameAndColumnName, SqlLockStatementProvider
      SqlServerLockStatementFormatter (class):  AppendColumn, Complete, Create, CreateOutboxStatement, FormatTableName, SqlServerLockStatementFormatter
      SqlServerLockStatementProvider (class):  SqlServerLockStatementProvider
      SqliteLockStatementFormatter (class):  AppendColumn, Complete, Create, CreateOutboxStatement
      SqliteLockStatementProvider (class):  SqliteLockStatementProvider
      ValueConversionExtensions (class):  HasJsonConversion
   MassTransit.EntityFrameworkCoreIntegration.Audit
      AuditDbContext (class):  AuditDbContext
      AuditMapping (class):  AuditMapping, Configure
      AuditRecord (class)
      EntityFrameworkAuditStore (class):  EntityFrameworkAuditStore, StoreMessage
   MassTransit.EntityFrameworkCoreIntegration.Configuration
      EntityFrameworkSagaRepository (class):  AddSagaClassMap, CreateOptionsBuilder, EntityFrameworkSagaRepository, GetDbContext
      RepositorySagaDbContext (class):  RepositorySagaDbContext
   MassTransit.EntityFrameworkCoreIntegration.Saga
      ContainerSagaDbContextFactory (class):  ContainerSagaDbContextFactory, Create, CreateScoped, ReleaseAsync
      DbContextSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, DbContextSagaRepositoryContext, Delete, Discard, Dispose, Insert, Load, Query, Save, Undo, Update
      DelegateSagaDbContextFactory (class):  Create, CreateScoped, DelegateSagaDbContextFactory, ReleaseAsync
      EntityFrameworkSagaRepositoryContextFactory (class):  EntityFrameworkSagaRepositoryContextFactory, Execute, ExecuteAsyncMethod, Probe, Send, SendQuery, WithinTransaction
      OptimisticLoadQueryExecutor (class):  Load, OptimisticLoadQueryExecutor
      OptimisticSagaLockContext (class):  Load, OptimisticSagaLockContext
         Defers loading the sagas until the transaction is started
      OptimisticSagaRepositoryLockStrategy (class):  CreateLockContext, Load, OptimisticSagaRepositoryLockStrategy
      PessimisticLoadQueryExecutor (class):  GetLockStatement, Load, PessimisticLoadQueryExecutor
      PessimisticSagaLockContext (class):  Load, PessimisticSagaLockContext
         Queries the list of saga ids prior to the transaction, and then loads/locks them individually
      PessimisticSagaRepositoryLockStrategy (class):  CreateLockContext, Load, PessimisticSagaRepositoryLockStrategy
      SagaLockContext (interface):  Load
   MassTransit.EntityFrameworkIntegration
      EntityFrameworkSagaRepository (class):  CreateOptimistic, CreatePessimistic, CreateRepository
      ILoadQueryExecutor (interface):  Load
      ILoadQueryProvider (interface):  GetQueryable
      ILockStatementProvider (interface):  GetRowLockStatement
      ISagaClassMap (interface):  Configure
      ISagaDbContextFactory (interface):  Create, CreateScoped, Release
         Creates a DbContext for the saga repository
      ISagaRepositoryLockStrategy (interface):  CreateLockContext, Load
      SagaClassMap (class):  Configure
      SagaDbContext (class):  ContainsSagaConfiguration, Set
      SqlLockStatementProvider (class):  FormatLockStatement, GetRowLockStatement, GetSchemaAndTableName, SqlLockStatementProvider
      SqlServerLockStatementProvider (class):  SqlServerLockStatementProvider
   MassTransit.EntityFrameworkIntegration.Audit
      AuditDbContext (class):  AuditDbContext
      AuditMapping (class):  AuditMapping
      AuditRecord (class)
      EntityFrameworkAuditStore (class):  EntityFrameworkAuditStore, StoreMessage
   MassTransit.EntityFrameworkIntegration.Saga
      ContainerSagaDbContextFactory (class):  ContainerSagaDbContextFactory, Create, CreateScoped, Release
      CustomSagaLoadQueryProvider (class):  CustomSagaLoadQueryProvider, GetQueryable
      DbContextSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, DbContextSagaRepositoryContext, Delete, Discard, Dispose, Insert, Load, Query, Save, Undo, Update
      DefaultSagaLoadQueryProvider (class):  GetQueryable
      DelegateSagaDbContextFactory (class):  Create, CreateScoped, DelegateSagaDbContextFactory, Release
      EntityFrameworkSagaRepositoryContextFactory (class):  EntityFrameworkSagaRepositoryContextFactory, Execute, ExecuteAsyncMethod, Probe, Send, SendQuery, WithinTransaction
      OptimisticLoadQueryExecutor (class):  Load, OptimisticLoadQueryExecutor
      OptimisticSagaLockContext (class):  Load, OptimisticSagaLockContext
         Defers loading the sagas until the transaction is started
      OptimisticSagaRepositoryLockStrategy (class):  CreateLockContext, Load, OptimisticSagaRepositoryLockStrategy
      PessimisticLoadQueryExecutor (class):  GetLockStatement, Load, PessimisticLoadQueryExecutor
      PessimisticSagaLockContext (class):  Load, PessimisticSagaLockContext
         Queries the list of saga ids prior to the transaction, and then loads/locks them individually
      PessimisticSagaRepositoryLockStrategy (class):  CreateLockContext, Load, PessimisticSagaRepositoryLockStrategy
      SagaLockContext (interface):  Load
   MassTransit.EventHubIntegration
      BatchSendPipe (class):  BatchSendPipe, Probe, Send
      CachedEventHubProducer (class):  CachedEventHubProducer, ConnectSendObserver, DisposeAsync, Produce
      ConfigureTopologyContext (interface)
      ConnectionContext (interface):  CreateEventHubClient
      ConnectionContextFactory (class):  ConnectionContextFactory, CreateActiveContext, CreateConnectionContext, CreateContext, CreateSharedConnectionContext
      ConnectionContextSupervisor (class):  ConnectionContextSupervisor
      ConsumeContextEventHubProducerProvider (class):  ConnectSendObserver, ConsumeContextEventHubProducerProvider, GetProducer
      ConsumeSendPipeAdapter (class):  ConsumeSendPipeAdapter, Probe, Send
      EventDataHeaderProvider (class):  EventDataHeaderProvider, GetAll, TryGetHeader
      EventDataReceiveContext (class):  EventDataReceiveContext
      EventDataReceiver (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectPublishObserver, ConnectReceiveObserver, ConnectSendObserver, EventDataReceiver, Handle, Probe
      EventHubConnectionContext (class):  CreateEventHubClient, EventHubConnectionContext
      EventHubDataReceiver (class):  EventHubDataReceiver, GetBytes, Handle, HandleError, HandleMessage
      EventHubHeaderProvider (class):  EventHubHeaderProvider, GetAll, TryGetHeader
      EventHubMessageSendContext (class):  EventHubMessageSendContext
      EventHubProcessorContext (class):  EventHubProcessorContext, GetClient, ReleaseClient
      EventHubProducer (class):  ConnectSendObserver, DisposeAsync, EventHubProducer, Produce
      EventHubProducerCache (class):  EventHubProducerCache, GetProducer, GetProducerFromFactory
      EventHubProducerContext (class):  CreateBatch, DisposeAsync, EventHubProducerContext, Produce
      EventHubProducerProvider (class):  ConnectSendObserver, CreateProducer, EventHubProducerProvider, GetProducer, NormalizeAddress
      EventHubProducerSendTransportContext (class):  CreateContext, CreateSendContext, EventHubProducerSendTransportContext, GetAgentHandles, Probe, Send
      EventHubReceiveContext (class):  EventHubReceiveContext
      EventHubReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConvertException, EventHubReceiveEndpointContext
      EventHubReceiveLockContext (class):  Complete, EventHubReceiveLockContext, Faulted, ValidateLockStatus
      EventHubRider (class):  CheckEndpointHealth, ConnectEventHubEndpoint, EventHubRider, GetProducerProvider, Reset, Start
      EventHubSendTransportContext (interface):  CreateContext, GetAgentHandles, Send
      EventReceiver (class):  CreateEventDataReceiver, Dispose, EventReceiver, Handle, HandleConsumer, HandleExecuteActivity, HandleSaga
      Handle (class):  Handle, ReadyOrNot, StopAsync
      IConnectionContextSupervisor (interface)
      IEventDataReceiver (interface):  Handle
      IEventHubDataReceiver (interface)
      IEventHubProducerCache (interface):  GetProducer
      IProcessorContextSupervisor (interface)
      IProcessorLockContext (interface):  Canceled, Complete, Faulted, Pending
      IProducerContextSupervisor (interface)
      PartitionCheckpointData (class):  Close, PartitionCheckpointData, Pending
      ProcessorClientBuilderContext (interface):  OnPartitionClosing, OnPartitionInitializing
      ProcessorContext (interface):  GetClient, ReleaseClient
      ProcessorContextFactory (class):  CreateActiveContext, CreateContext, CreateProcessor, CreateSharedConnection, ProcessorContextFactory
      ProcessorContextSupervisor (class):  ProcessorContextSupervisor
      ProcessorLockContext (class):  Canceled, Complete, DisposeAsync, Faulted, OnPartitionClosing, OnPartitionInitializing, Pending, ProcessorLockContext
      Producer (class):  ConnectSendObserver, Produce, Producer
      ProducerContext (interface):  CreateBatch, Produce
      ProducerContextFactory (class):  CreateActiveContext, CreateContext, CreateProcessor, CreateSharedConnection, ProducerContextFactory
      ProducerContextSupervisor (class):  ProducerContextSupervisor
      ReceiveSettings (interface)
      RiderAgent (class):  RiderAgent
      SendPipe (class):  Probe, Send, SendPipe
      SharedConnectionContext (class):  CreateEventHubClient, SharedConnectionContext
      SharedProcessorContext (class):  GetClient, ReleaseClient, SharedProcessorContext
      SharedProducerContext (class):  CreateBatch, DisposeAsync, Produce, SharedProducerContext
   MassTransit.EventHubIntegration.Activities
      FaultedProduceActivity (class):  Accept, Execute, Faulted, FaultedProduceActivity, Probe
      ProduceActivity (class):  Accept, Execute, Faulted, Probe, ProduceActivity
      ProducerFactoryExtensions (class)
   MassTransit.EventHubIntegration.Checkpoints
      BatchCheckpointer (class):  BatchCheckpointer, Checkpoint, DisposeAsync, Pending, ReadBatch, TryCheckpoint, WaitForBatch
      ICheckpointer (interface):  Pending
      IPendingConfirmation (interface):  Canceled, Checkpoint, Complete, Faulted
      PartitionOffset (struct):  PartitionOffset, ToString
      PendingConfirmation (class):  Canceled, Checkpoint, Complete, Faulted, PendingConfirmation
      PendingConfirmationCollection (class):  Add, Cancel, Canceled, Complete, Dispose, Faulted, PendingConfirmationCollection
   MassTransit.EventHubIntegration.Configuration
      EventDataReceiverConfiguration (class):  Build, EventDataReceiverConfiguration
      EventHubBusInstanceSpecification (class):  Configure, EventHubBusInstanceSpecification, Validate
      EventHubFactoryConfigurator (class):  AddSerializer, Build, ConfigureProducerOptions, ConfigureSend, ConnectReceiveEndpointObserver, ConnectSendObserver, CreateSendTransportContext, CreateSpecification, EventHubFactoryConfigurator, Host, ReceiveEndpoint, Storage, ThrowIfHostIsAlreadyConfigured, ThrowIfStorageIsAlreadyConfigured, Validate
      EventHubProducerSpecification (class):  AddSerializer, ConfigureSend, ConnectSendObserver, CreateSendTransportContext, EventHubProducerSpecification, Validate
      EventHubReceiveEndpointBuilder (class):  CreateReceiveEndpointContext, EventHubReceiveEndpointBuilder
      EventHubReceiveEndpointConfigurator (class):  Build, CreateBlobClient, CreateEventHubReceiveContext, CreateEventProcessorClient, CreateReceiveEndpointContext, EventHubReceiveEndpointConfigurator, OnPartitionClosing, OnPartitionInitializing
      EventHubReceiveEndpointSpecification (class):  ConnectReceiveEndpointObserver, CreateReceiveEndpoint, EventHubReceiveEndpointSpecification, Validate
      EventHubRegistrationRiderFactory (class):  CreateRider, EventHubRegistrationRiderFactory
      HostSettings (class)
      IEventHubHostConfiguration (interface):  Build, CreateSendTransportContext, CreateSpecification
      IEventHubProducerSpecification (interface):  CreateSendTransportContext
      IEventHubReceiveEndpointSpecification (interface):  CreateReceiveEndpoint
      IHostSettings (interface)
      IStorageSettings (interface)
      StorageSettings (class)
   MassTransit.EventHubIntegration.Middleware
      EventHubBlobContainerFactoryFilter (class):  CreateBlobIfNotExistsAsync, EventHubBlobContainerFactoryFilter, Probe, Send
      EventHubConsumerFilter (class):  EventHubConsumerFilter, Probe, Send
   MassTransit.Events
      BusReadyEvent (class):  BusReadyEvent
      FaultEvent (class):  FaultEvent, GetExceptions
      FaultExceptionInfo (class):  FaultExceptionInfo, UpdateData
      HostReadyEvent (class):  HostReadyEvent
      ReceiveEndpointCompletedEvent (class):  ReceiveEndpointCompletedEvent
      ReceiveEndpointFaultedEvent (class):  ReceiveEndpointFaultedEvent
      ReceiveEndpointReadyEvent (class):  ReceiveEndpointReadyEvent
      ReceiveEndpointStoppingEvent (class):  ReceiveEndpointStoppingEvent
      ReceiveFaultEvent (class):  ReceiveFaultEvent
      ReceiveTransportCompletedEvent (class):  ReceiveTransportCompletedEvent
      ReceiveTransportFaultedEvent (class):  ReceiveTransportFaultedEvent
      ReceiveTransportReadyEvent (class):  ReceiveTransportReadyEvent
   MassTransit.ExceptionFilters
      CompositeExceptionFilter (class):  CompositeExceptionFilter, Match, Probe
      CompositeFilter (class):  Matches
      CompositePredicate (class):  Add, DoesNotMatchAny, MatchesAll, MatchesAny, MatchesNone
   MassTransit.Futures
      BuildRoutingSlipExecutor (class):  BuildRoutingSlipExecutor, Execute
      Default (class)
      FutureFault (class):  DefaultFactory, FutureFault, SetFaulted, Validate
      FutureRequest (class):  DefaultFactory, FutureRequest, PublishAddressProvider, SendRequest, Validate
      FutureResult (class):  SetResult, Validate
      FutureSubscriptionExtensions (class):  SendMessageToSubscriptions
      IFutureStateMachineConfigurator (interface):  CompletePendingRequest, CreateResponseEvent, DuringAnyWhen, SetFaulted, SetResult
      IRoutingSlipExecutor (interface):  Execute
      PlanRoutingSlipExecutor (class):  Execute
   MassTransit.Futures.Contracts
      Get (interface)
         Sent by a client to get a future value by type/ID
   MassTransit.HangfireIntegration
      BusRegistrationContextComponentResolver (class):  BusRegistrationContextComponentResolver
      DefaultHangfireComponentResolver (class):  DefaultHangfireComponentResolver
      ForwardScheduledMessagePipe (class):  ForwardScheduledMessagePipe, Probe, Send
      HangfireBusObserver (class):  CreateFaulted, HangfireBusObserver, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted
         Used to start and stop an in-memory scheduler using Hangfire
      HangfireMessageHeaders (class)
      HangfireRecurringScheduledMessageData (class):  Create
      HangfireScheduledMessageData (class)
      HashCleanupAttribute (class):  OnPerformed, OnPerforming
      HashedHangfireScheduledMessageData (class):  Create
      IHangfireComponentResolver (interface)
      IHashedScheduleId (interface)
      IMassTransitJobActivatorFactory (interface):  ActivateJob
      MassTransitJobActivator (class):  ActivateJob, CreateJobFactory, MassTransitJobActivator
      MassTransitJobActivatorFactory (class):  ActivateJob, CreateConstructor, CreateDefaultConstructor, CreateServiceBusConstructor, MassTransitJobActivatorFactory, NewJob
      MessageDataMessageContext (class):  ConvertDateTime, ConvertIdToGuid, ConvertToUri, Get, GetAll, GetEnumerator, GetHeaders, MessageDataMessageContext, TryGetHeader
      PauseScheduledRecurringMessageConsumer (class):  Consume, PauseScheduledRecurringMessageConsumer
      RecurringScheduleDateTimeIntervalAttribute (class):  OnCreated, OnCreating, OnPerformed, OnPerforming
      ResumeScheduledRecurringMessageConsumer (class):  Consume, ResumeScheduledRecurringMessageConsumer
      ScheduleJob (class):  ScheduleJob, SendMessage
      ScheduleMessageConsumer (class):  Consume, ScheduleMessageConsumer, TryRemoveJob
      ScheduleRecurringMessageConsumer (class):  Consume, ScheduleRecurringMessageConsumer
      SchedulerBusObserver (class):  CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, SchedulerBusObserver, StartFaulted, StopFaulted
         Used to start and stop an in-memory scheduler using Hangfire
   MassTransit.InMemoryTransport
      AdvanceTimeOperation (class):  AdvanceTimeOperation
      Context (class):  Context
      DelayOperation (class):  DelayOperation
      DuplicateKeyComparer (class):  Compare
      FutureDelay (class):  Cancel, Complete, Dispose, FutureDelay
      IInMemoryDelayProvider (interface):  Advance, Delay
      IInMemoryHost (interface)
      IInMemoryTransportProvider (interface):  CreatePublishTransport, CreateSendTransport, NormalizeAddress
      IOperation (interface)
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector, ImplementsMessageType
      InMemoryBusInstance (class):  Advance, Delay, InMemoryBusInstance
      InMemoryBusTopology (class):  InMemoryBusTopology, Publish
      InMemoryConsumeContext (interface)
      InMemoryConsumeTopology (class):  AddSpecification, Apply, Bind, GetMessageTopology, InMemoryConsumeTopology, Validate
      InMemoryDelayProvider (class):  Advance, CancelPendingDelays, Delay, DelayChannelReader, DisposeAsync, GetDelayTimeout, InMemoryDelayProvider
      InMemoryDeliveryContext (class):  Delivered, InMemoryDeliveryContext, WasAlreadyDelivered
      InMemoryHost (class):  ConnectReceiveEndpoint, InMemoryHost
         Caches InMemory transport instances so that they are only created and used once
      InMemoryMessageConsumeTopology (class):  Apply, Bind, InMemoryMessageConsumeTopology, Validate
      InMemoryMessageDeadLetterTransport (class):  InMemoryMessageDeadLetterTransport, Send
      InMemoryMessageErrorTransport (class):  InMemoryMessageErrorTransport, Send
      InMemoryMessageMoveTransport (class)
      InMemoryMessagePublishTopology (class):  AddImplementedMessageConfigurator, Apply, InMemoryMessagePublishTopology, TryGetPublishAddress
      InMemoryPublishTopology (class):  GetMessageTopology, InMemoryPublishTopology
      InMemoryPublishTransportProvider (class):  GetPublishTransport, InMemoryPublishTransportProvider
      InMemoryReceiveContext (class):  InMemoryReceiveContext
      InMemoryReceiveEndpointContext (interface):  ConfigureTopology
      InMemoryReceiveTransport (class):  ConnectPublishObserver, ConnectReceiveObserver, ConnectReceiveTransportObserver, ConnectSendObserver, InMemoryReceiveTransport, Probe, Start
         Support in-memory message queue that is not durable, but supports parallel delivery of messages based on TPL usage.
      InMemorySendContext (class):  InMemorySendContext, ReadPropertiesFrom, WritePropertiesTo
      InMemorySendTransportContext (class):  CreateSendContext, InMemorySendTransportContext, Probe, Send, SetHeaders
      InMemorySendTransportProvider (class):  GetSendTransport, InMemorySendTransportProvider, NormalizeAddress
      InMemoryTransportContext (interface)
      InMemoryTransportMessage (class):  InMemoryTransportMessage
      InMemoryTransportProvider (class):  ApplyTopologyToMessageFabric, CreatePublishTransport, CreateSendTransport, InMemoryTransportProvider, NormalizeAddress, Probe
      PropertyNames (class)
      ReceiveTransportAgent (class):  Deliver, Probe, ReceiveTransportAgent, Startup, Stop
      TransportInMemoryReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConfigureTopology, ConvertException, TransportInMemoryReceiveEndpointContext
      TypeAdapter (class):  Apply, TypeAdapter
   MassTransit.InMemoryTransport.Configuration
      ExchangeBindingConsumeTopologySpecification (class):  Apply, ExchangeBindingConsumeTopologySpecification, Validate
         Used to bind an exchange to the consuming queue's exchange
      IInMemoryBusConfiguration (interface):  CreateEndpointConfiguration
      IInMemoryConsumeTopologySpecification (interface):  Apply
      IInMemoryEndpointConfiguration (interface)
      IInMemoryHostConfiguration (interface):  ApplyEndpointDefinition, CreateReceiveEndpointConfiguration
      IInMemoryReceiveEndpointConfiguration (interface):  Build
      IInMemoryTopologyConfiguration (interface)
      InMemoryBusConfiguration (class):  ConnectBusObserver, ConnectEndpointConfigurationObserver, InMemoryBusConfiguration
      InMemoryBusFactoryConfigurator (class):  CreateBusEndpointConfiguration, Host, InMemoryBusFactoryConfigurator, Publish, ReceiveEndpoint
      InMemoryEndpointConfiguration (class):  CreateEndpointConfiguration, InMemoryEndpointConfiguration
      InMemoryHostConfiguration (class):  ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, InMemoryHostConfiguration, ReceiveEndpoint
      InMemoryReceiveEndpointBuilder (class):  ConnectConsumePipe, CreateReceiveEndpointContext, InMemoryReceiveEndpointBuilder
      InMemoryReceiveEndpointConfiguration (class):  Bind, Build, CreateInMemoryReceiveEndpointContext, CreateReceiveEndpointContext, InMemoryReceiveEndpointConfiguration
      InMemoryRegistrationBusFactory (class):  CreateBus, InMemoryRegistrationBusFactory
      InMemoryTopologyConfiguration (class):  InMemoryTopologyConfiguration, Validate
      InvalidInMemoryConsumeTopologySpecification (class):  Apply, InvalidInMemoryConsumeTopologySpecification, Validate
   MassTransit.Initializers
      Cached (class):  CreateMessageFactory
      IHeaderInitializer (interface):  Apply
         Initialize a message header
      IInitializerVariable (interface):  GetValue
      IMessageFactory (interface):  Create
         Creates the message type
      IMessageInitializer (interface):  Create, Initialize, InitializeMessage
         A message initializer that doesn't use the input
      IMessageInitializerCache (interface):  GetInitializer
      IMessageInitializerFactory (interface):  CreateMessageInitializer
      IPropertyConverter (interface):  Convert
         A message property converter, which is async, and has access to the context
      IPropertyInitializer (interface):  Apply
         A message initializer that uses the input
      IPropertyProvider (interface):  GetProperty
         Returns the property from the input
      IPropertyProviderFactory (interface):  TryGetPropertyConverter, TryGetPropertyProvider
      ITypeConverter (interface):  TryConvert
         A synchronous property type conversion, which may or may not succeed.
      InitializeContext (interface):  CreateInputContext, CreateMessageContext, TryGetParent
         Message initialization context, which includes the message being initialized and the input being used to initialize t...
      InitializerSendContextPipe (class):  InitializerSendContextPipe, Probe, Send
      MessageFactoryCache (class)
      MessageInitializer (class):  AddConvention, Create, Initialize, InitializeMessage, MessageInitializer, PrepareSendTuple
      MessageInitializerCache (class):  CreateMessageInitializer, GetInitializer, Initialize, InitializeMessage, MessageInitializerCache
      TaskInitializerExtensions (class):  Select
   MassTransit.Initializers.Contexts
      BaseInitializeContext (class):  BaseInitializeContext, CreateMessageContext, TryGetParent
      DynamicInitializeContext (class):  CreateInputContext, CreateMessageContext, DynamicInitializeContext, TryGetParent
      ScopeInitializeContext (class):  CreateMessageContext, ScopeInitializeContext, TryGetParent
   MassTransit.Initializers.Conventions
      CacheFactory (class):  Create
      Cached (interface)
      CachedValue (class):  CachedValue
      ConventionTypeCache (class):  ConventionTypeCache, GetOrAdd
      DefaultInitializerConvention (class):  DefaultInitializerConvention, TryGetHeaderInitializer, TryGetHeadersInitializer, TryGetPropertyInitializer
      DictionaryInitializerConvention (class):  DictionaryInitializerConvention, TryGetHeaderInitializer, TryGetHeadersInitializer, TryGetPropertyInitializer
      IConventionTypeCache (interface):  GetOrAdd
         A convention cache for type specified, which converts to the generic type requested
      IConventionTypeCacheFactory (interface):  Create
      IInitializerConvention (interface):  TryGetHeaderInitializer, TryGetHeadersInitializer, TryGetPropertyInitializer
      IMessageInitializerConvention (interface)
      IMessageInputInitializerConvention (interface)
      InitializerConvention (class):  TryGetHeaderInitializer, TryGetHeadersInitializer, TryGetPropertyInitializer
   MassTransit.Initializers.Factories
      DynamicMessageFactory (class):  Create
      HeaderAdapter (class):  Apply, HeaderAdapter
      HeaderInitializerInspector (class):  Apply, HeaderInitializerInspector
      IHeaderInitializerInspector (interface):  Apply
      IMessageInitializerBuilder (interface):  Add, IsInputPropertyUsed, SetInputPropertyUsed
      IPropertyInitializerInspector (interface):  Apply
      InputHeaderInitializerInspector (class):  Apply, InputHeaderInitializerInspector
      MessageInitializerBuilder (class):  Add, Build, IsInputPropertyUsed, MessageInitializerBuilder, SetInputPropertyUsed
      MessageInitializerFactory (class):  CreateHeaderInspector, CreateHeaderInspectors, CreateInputHeaderInspectors, CreateMessageInitializer, CreatePropertyInspectors, MessageInitializerFactory
      PropertyAdapter (class):  Apply, PropertyAdapter
      PropertyInitializerInspector (class):  Apply, PropertyInitializerInspector
   MassTransit.Initializers.HeaderInitializers
      CopyHeaderInitializer (class):  Apply, CopyHeaderInitializer
         Set a header to a constant value from the input
      DictionaryCopyHeaderInitializer (class):  Apply, DictionaryCopyHeaderInitializer
      ProviderHeaderInitializer (class):  Apply, ApplyAsync, ProviderHeaderInitializer
         Set a message property using the property provider for the property value
      SetHeaderInitializer (class):  Apply, SetHeaderInitializer
         Set a header to a constant value from the input
      SetStringHeaderInitializer (class):  Apply, SetStringHeaderInitializer
         Set a header to a constant value from the input
   MassTransit.Initializers.PropertyConverters
      ArrayPropertyConverter (class):  ArrayPropertyConverter, Convert, ConvertSync
      DictionaryKeyPropertyConverter (class):  Convert, ConvertSync, DictionaryKeyPropertyConverter
      DictionaryPropertyConverter (class):  Convert, ConvertSync, DictionaryPropertyConverter
      FromNullablePropertyConverter (class):  Convert, FromNullablePropertyConverter
      InitializePropertyConverter (class):  Convert, InitializePropertyConverter
      ListPropertyConverter (class):  Convert, ConvertSync, ListPropertyConverter
      MessageDataPropertyConverter (class):  Convert, MessageDataPropertyConverter
      StatePropertyConverter (class):  Convert, StatePropertyConverter
      TaskPropertyConverter (class):  Convert, TaskPropertyConverter
         Converts a to {T} by awaiting the result
      ToNullablePropertyConverter (class):  Convert, ToNullablePropertyConverter
      ToObjectPropertyConverter (class):  Convert
      TypePropertyConverter (class):  Convert, TypePropertyConverter
         Calls the property type converter, returning either the result or default.
      VariablePropertyConverter (class):  Convert, VariablePropertyConverter
   MassTransit.Initializers.PropertyInitializers
      CopyAsyncObjectPropertyInitializer (class):  Apply, CopyAsyncObjectPropertyInitializer
      CopyObjectPropertyInitializer (class):  Apply, CopyObjectPropertyInitializer
      CopyPropertyInitializer (class):  Apply, CopyPropertyInitializer
         Set a message property by copying the input property (of the same type), regardless of whether the input property val...
      DictionaryCopyPropertyInitializer (class):  Apply, DictionaryCopyPropertyInitializer
         Gets the dictionary entry for the property (if present), and sets the message property to the value
      ProviderPropertyInitializer (class):  Apply, ProviderPropertyInitializer
         Set a message property using the property provider for the property value
   MassTransit.Initializers.PropertyProviders
      ArrayProperty (class):  ArrayProperty, IsSupportedType, TryGetConverter, TryGetProvider
      ArrayResult (class):  ArrayResult, TryGetConverter, TryGetProvider
      AsyncPropertyProvider (class):  AsyncPropertyProvider, GetProperty
         Awaits a property, returning the property value.
      ConstantPropertyProvider (class):  ConstantPropertyProvider, GetProperty
         Returns a constant value for the property
      Convert (class):  Convert, TryGetConverter, TryGetProvider
      Converter (interface):  Convert
      DictionaryProperty (class):  DictionaryProperty, IsSupportedType, TryGetConverter, TryGetProvider
      DictionaryResult (class):  DictionaryResult, TryGetConverter, TryGetProvider
      FromNullablePropertyProvider (class):  FromNullablePropertyProvider, GetProperty
      IProviderFactory (interface):  TryGetConverter, TryGetProvider
      InitializerResult (class):  TryGetConverter, TryGetProvider
      InputDictionaryPropertyProvider (class):  GetProperty, InputDictionaryPropertyProvider
         Copies the input property, as-is, for the property value
      InputPropertyProvider (class):  GetProperty, InputPropertyProvider
         Copies the input property, as-is, for the property value
      ListResult (class):  ListResult, TryGetConverter, TryGetProvider
      Matching (class):  TryGetConverter, TryGetProvider
      MessageDataResult (class):  MessageDataResult, TryGetConverter, TryGetProvider
      NullableProperty (class):  NullableProperty, TryGetConverter, TryGetProvider
      NullableResult (class):  NullableResult, TryGetConverter, TryGetProvider
      ObjectConverter (class):  Convert, ObjectConverter
      ObjectPropertyProvider (class):  CreateConverter, GetProperty, ObjectPropertyProvider
      PropertyConverterPropertyProvider (class):  GetProperty, PropertyConverterPropertyProvider
      PropertyProviderFactory (class):  CreateProviderFactory, TryGetPropertyConverter, TryGetPropertyProvider
         For an input type, builds the property providers for the requested result types
      StateProperty (class):  StateProperty, TryGetConverter, TryGetProvider
      TaskProperty (class):  TaskProperty, TryGetConverter, TryGetProvider
         The property on the input is a Task
      TaskPropertyProvider (class):  GetProperty, TaskPropertyProvider
      TaskResult (class):  TaskResult, TryGetConverter, TryGetProvider
      ToNullablePropertyProvider (class):  GetProperty, ToNullablePropertyProvider
      VariableProperty (class):  TryGetConverter, TryGetProvider, VariableProperty
      VariablePropertyProvider (class):  GetProperty, VariablePropertyProvider
         Copies the input property, as-is, for the property value
   MassTransit.Initializers.TypeConverters
      BooleanTypeConverter (class):  TryConvert
      ByteTypeConverter (class):  TryConvert
      Cached (class)
      DateTimeOffsetTypeConverter (class):  TryConvert
      DateTimeTypeConverter (class):  TryConvert
      DecimalTypeConverter (class):  TryConvert
      DoubleTypeConverter (class):  TryConvert
      EnumTypeConverter (class):  TryConvert
      ExceptionTypeConverter (class):  TryConvert
      FromNullableTypeConverter (class):  FromNullableTypeConverter, TryConvert
      GuidTypeConverter (class):  TryConvert
      ITypeConverterCache (interface):  TryGetTypeConverter
      IntTypeConverter (class):  TryConvert
      LongTypeConverter (class):  TryConvert
      ShortTypeConverter (class):  TryConvert
      StateTypeConverter (class):  TryConvert
      StringTypeConverter (class):  TryConvert
      TimeSpanTypeConverter (class):  TryConvert
      ToNullableTypeConverter (class):  ToNullableTypeConverter, TryConvert
      ToObjectTypeConverter (class):  TryConvert
      TypeConverterCache (class):  AddSupportedTypes, TryGetTypeConverter, TypeConverterCache
      UriTypeConverter (class):  TryConvert
      VersionTypeConverter (class):  TryConvert
   MassTransit.Initializers.Variables
      Context (class):  Context
      IdContext (interface)
      IdVariable (class):  GetValue, IdVariable
      TimestampContext (interface)
      TimestampVariable (class):  GetValue, TimestampVariable
         Used to set timestamp(s) in a message, which is the same regardless of how many times it is used within the same init...
   MassTransit.Internals.Caching
      Bucket (class):  Add, Bucket, Clear
      CacheMetrics (struct):  Hit, Miss
      CacheOptions (class):  CacheOptions
      CacheValue (class):  CacheValue, CreateValue, Evict, GetValue, SetException, SetResult, TryTake
      IBucket (interface):  Add
      ICache (interface):  Clear, Get, GetOrAdd, Remove
      ICachePolicy (interface):  CheckValue, CreateValue, IsValid
      ICacheValue (interface):  Evict, GetValue
      IPendingValue (interface):  CreateValue, SetValue
      ITimeToLiveCacheValue (interface)
      IValueTracker (interface):  Add, Clear, ReBucket
      MassTransitCache (class):  Clear, Get, GetOrAdd, GetValues, MassTransitCache, Remove, TryGetValue
      PendingValue (class):  CreateValue, DefaultMissingValueFactory, PendingValue, SetValue
      TimeToLiveCachePolicy (class):  CheckValue, CreateValue, IsValid, TimeToLiveCachePolicy
      TimeToLiveCacheValue (class):  TimeToLiveCacheValue
      UsageCachePolicy (class):  CheckValue, CreateValue, IsValid
      ValueFactoryException (class):  ValueFactoryException
      ValueTracker (class):  Add, Clear, ReBucket, ValueTracker
   MassTransit.Internals.GraphValidation
      AdjacencyList (class):  AddEdge, AdjacencyList, GetEdges, GetNode
      CyclicGraphException (class):  CyclicGraphException
      DependencyGraph (class):  Add, DefaultNodeFactory, DependencyGraph, EnsureGraphIsAcyclic, GetItemsInOrder
      DependencyGraphNode (class):  DependencyGraphNode
      Edge (struct):  CompareTo, Edge
      ITarjanNodeProperties (interface)
      ITopologicalSortNodeProperties (interface)
      Node (class):  CompareTo, Equals, GetHashCode
      NodeList (class):  GetEnumerator, Index, NodeList
         Maintains a list of nodes for a given set of instances of T
      NodeTable (class):  NodeTable
         Maintains an index of nodes so that regular ints can be used to execute algorithms against objects with int-compare s...
      Tarjan (class):  Compute, Tarjan
      TopologicalSort (class):  Sort, TopologicalSort
   MassTransit.Introspection
      IProbeResultBuilder (interface):  Build
      ProbeResult (interface)
         The result of a probe
      ProbeResultBuilder (class):  Build, ProbeResultBuilder
      Result (class):  Result
      ScopeProbeContext (class):  Add, CreateScope, Set, SetVariablesFromDictionary
   MassTransit.JobService
      ConsumeJobContext (class):  Cancel, ConsumeJobContext, DisposeAsync, Notify, NotifyCanceled, NotifyCompleted, NotifyConsumed, NotifyFaulted, NotifyJobProgress, NotifyStarted, SaveJobState, SetJobProgress, TryGetJobState
      ConsumerJobHandle (class):  Cancel, ConsumerJobHandle, DisposeAsync
      DefaultJobDistributionStrategy (class):  IsJobSlotAvailable
      FaultJobContext (class):  FaultJobContext, NotifyConsumed, NotifyFaulted
      FinalizeJobConsumer (class):  Consume, FinalizeJobConsumer
      IJobService (interface):  BusStarted, ConfigureSuperviseJobConsumer, GetJobTypeId, RegisterJobType, StartJob, Stop, TryGetJob, TryRemoveJob
      IJobTypeRegistration (interface):  PublishConcurrentJobLimit, PublishHeartbeat, PublishJobInstanceStopped
      JobHandle (interface):  Cancel
         A JobHandle contains the JobContext, Task, and provides access to the job control
      JobMetadataCache (class):  GenerateHashGuid, GenerateJobTypeId, GenerateJobTypeName, GenerateRecurringJobId
      JobProgressBuffer (class):  Flush, JobProgressBuffer, ReadUpdate, Update, WaitForUpdate
      JobService (class):  Add, BusStarted, ConfigureSuperviseJobConsumer, GetJobTypeId, JobService, RegisterJobType, StartJob, Stop, TryGetJob, TryRemoveJob
      JobServiceBusObserver (class):  CreateFaulted, JobServiceBusObserver, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted
      JobServiceSettings (interface)
         Settings relevant to the job consumer endpoints and the service instance
      JobTypeRegistration (class):  JobTypeRegistration, PublishConcurrentJobLimit, PublishHeartbeat, PublishJobInstanceStopped, PublishSetConcurrentJobLimit
      ProgressUpdate (struct):  ProgressUpdate
      StartJobConsumer (class):  Consume, StartJobConsumer
      SubmitJobConsumer (class):  Consume, PublishJobSubmitted, SubmitJobConsumer
         Handles the command
      SuperviseJobConsumer (class):  Consume, SuperviseJobConsumer
   MassTransit.JobService.Messages
      AllocateJobSlotCommand (class)
      CancelJobAttemptCommand (class)
      CancelJobCommand (class)
      CompleteJobCommand (class)
      FaultJobCommand (class)
      FinalizeJobAttemptCommand (class)
      FinalizeJobCommand (class)
      GetJobAttemptStatusRequest (class)
      GetJobStateRequest (class)
      JobAttemptCanceledEvent (class)
      JobAttemptCompletedEvent (class)
      JobAttemptFaultedEvent (class)
      JobAttemptStartedEvent (class)
      JobAttemptStatusResponse (class)
      JobCanceledEvent (class)
      JobCompletedEvent (class)
      JobFaultedEvent (class)
      JobRetryDelayElapsedEvent (class)
      JobSlotAllocatedResponse (class)
      JobSlotReleasedEvent (class)
      JobSlotUnavailableResponse (class)
      JobSlotWaitElapsedEvent (class)
      JobStartedEvent (class)
      JobStateResponse (class):  JobStateResponse
      JobStatusCheckRequestedEvent (class)
      JobSubmissionAcceptedResponse (class)
      JobSubmittedEvent (class)
      RecurringJobScheduleInfo (class):  Validate
      RetryJobCommand (class)
      RunJobCommand (class)
      SaveJobStateCommand (class)
      SetConcurrentJobLimitCommand (class)
      SetJobProgressCommand (class)
      StartJobAttemptCommand (class)
      StartJobCommand (class)
      SubmitJobCommand (class)
   MassTransit.JobService.Scheduling
      CronExpression (class):  AddToSet, AdjustDayToNearestWeekday, BuildExpression, CalculateDaysOfMonth, CalculateNearestWeekdayForDaysOfMonth, CalculateNearestWeekdayForLastDay, CheckIncrementRange, CheckNext, ClearExpressionFields, CronExpression, Equals, FindNextWhiteSpace, GetDayOfWeekNumber, GetExpressionSummary, GetHashCode
      CronExpressionConstants (class)
      CronExpressionSummary (struct):  CronExpressionSummary, GetExpressionSetSummary, ToString
      CronField (class):  Add, Contains, CronField, GetEnumerator
      Defaults (class)
      NextFireTimeCursor (struct):  Deconstruct, NextFireTimeCursor
      SortedSetExtensions (class)
      TimeZoneUtil (class):  ConvertTime, FindTimeZoneById, GetUtcOffset, TimeZoneUtil
      ValueAndPosition (struct):  Deconstruct, ValueAndPosition
   MassTransit.KafkaIntegration
      AsyncTopicProducer (class):  AsyncTopicProducer, ConnectSendObserver, Produce, ProduceInternal
      ClientContext (interface)
      ClientContextFactory (class):  ClientContextFactory, CreateActiveContext, CreateClientContext, CreateContext, CreateSharedClientContext
      ClientContextSupervisor (class):  ClientContextSupervisor
      ConfigureTopologyContext (interface)
      ConsumeContextTopicProducerProvider (class):  ConnectSendObserver, ConsumeContextTopicProducerProvider, GetProducer
      ConsumeSendPipeAdapter (class):  ConsumeSendPipeAdapter, Probe, Send
      ConsumerContext (interface):  CreateConsumer
      ConsumerContextFactory (class):  ConsumerContextFactory, CreateActiveContext, CreateConsumer, CreateContext, CreateSharedConnection
      ConsumerContextSupervisor (class):  ConsumerContextSupervisor
      ConsumerLockContext (class):  Canceled, Complete, ConsumerLockContext, DisposeAsync, Faulted, OnAssigned, OnPartitionLost, OnUnAssigned, Pending
      Handle (class):  Handle, ReadyOrNot, StopAsync
      IClientContextSupervisor (interface)
      IConsumerContextSupervisor (interface)
      IConsumerLockContext (interface):  Canceled, Complete, Faulted, Pending
      IKafkaMessageConsumer (interface)
      IKafkaMessageReceiver (interface)
      IProducerContextSupervisor (interface)
      KafkaClientContext (class):  KafkaClientContext
      KafkaConsumeContext (class):  KafkaConsumeContext
      KafkaConsumerBuilderContext (interface):  OnAssigned, OnPartitionLost, OnUnAssigned
      KafkaConsumerContext (class):  CreateConsumer, KafkaConsumerContext
      KafkaHeaderProvider (class):  GetAll, KafkaHeaderProvider, TryGetHeader
      KafkaMessageConsumer (class):  Consume, Handle, HandleKafkaError, KafkaMessageConsumer, OnAssigned, OnPartitionLost, OnUnAssigned
      KafkaMessageContext (class):  GetEndpointAddress, KafkaMessageContext
      KafkaMessageExtensions (class):  Deserialize, DeserializeKey, DeserializeValue
      KafkaMessageSendContext (class):  KafkaMessageSendContext
      KafkaProducerContext (class):  Dispose, KafkaProducerContext, OnError, Produce
      KafkaReceiveContext (class):  KafkaReceiveContext
      KafkaReceiveEndpointBuilder (class):  CreateReceiveEndpointContext, KafkaReceiveEndpointBuilder
      KafkaReceiveEndpointContext (interface):  GetInputAddress
      KafkaReceiveLockContext (class):  Complete, Faulted, KafkaReceiveLockContext, ValidateLockStatus
      KafkaRider (class):  CheckEndpointHealth, ConnectSendObserver, ConnectTopicEndpoint, GetProducer, KafkaRider, Reset, Start
      KafkaSendTransportContext (interface):  CreateContext, GetAgentHandles, Send
      KafkaSerializationContext (class):  GetMessageSerializer, IsSupportedMessageType, KafkaSerializationContext, ToDictionary, TryGetMessage
      KafkaTopicOptions (class):  KafkaTopicOptions, Validate
      KafkaTopicSendTransportContext (class):  CreateContext, CreateMessage, CreateSendContext, GetAgentHandles, KafkaTopicSendTransportContext, Probe, Send
      KeyedTopicProducer (class):  ConnectSendObserver, KeyedTopicProducer, Produce
      PartitionCheckpointData (class):  Close, Lost, PartitionCheckpointData, Pending
      ProducerContext (interface):  Produce
      ProducerContextFactory (class):  CreateActiveContext, CreateContext, CreateProducer, CreateSharedConnection, ProducerContextFactory
      ProducerContextSupervisor (class):  ProducerContextSupervisor
      ReceiveSettings (interface)
      RiderAgent (class):  RiderAgent
      ScopedProducePipeAdapter (class):  Probe, ScopedProducePipeAdapter, Send
      ScopedTopicProducer (class):  ConnectSendObserver, Produce, ScopedTopicProducer
      SendPipe (class):  Probe, Send, SendPipe
      SetKeyPipe (class):  Probe, Send, SetKeyPipe
      SharedClientContext (class):  SharedClientContext
      SharedConsumerContext (class):  CreateConsumer, SharedConsumerContext
      SharedProducerContext (class):  Dispose, Produce, SharedProducerContext
      TopicKafkaReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConvertException, GetInputAddress, Probe, TopicKafkaReceiveEndpointContext
      TopicProducer (class):  ConnectSendObserver, DisposeAsync, Produce, TopicProducer
      TopicProducerProvider (class):  ConnectSendObserver, CreateProducer, GetProducer, NormalizeAddress, TopicProducerProvider
   MassTransit.KafkaIntegration.Activities
      FaultedProduceActivity (class):  Accept, Execute, Faulted, FaultedProduceActivity, Probe
      ProduceActivity (class):  Accept, Execute, Faulted, Probe, ProduceActivity
   MassTransit.KafkaIntegration.Caching
      CachedTopicProducer (class):  CachedTopicProducer, ConnectSendObserver, DisposeAsync, Produce
      ICachedTopicProducer (interface)
      ITopicProducerCache (interface):  GetProducer
      TopicProducerCache (class):  CreateProducer, GetProducer, TopicProducerCache
   MassTransit.KafkaIntegration.Checkpoints
      BatchCheckpointer (class):  BatchCheckpointer, Checkpoint, DisposeAsync, Pending, ReadBatch, TryCheckpoint, WaitForBatch
      ICheckpointer (interface):  Pending
      IPendingConfirmation (interface):  Canceled, Complete, Faulted
      PendingConfirmation (class):  Canceled, Complete, Faulted, PendingConfirmation
      PendingConfirmationCollection (class):  Add, Cancel, Canceled, Complete, Dispose, Faulted, PendingConfirmationCollection
   MassTransit.KafkaIntegration.Configuration
      IKafkaConsumerSpecification (interface):  CreateReceiveEndpoint
      IKafkaProducerRegistration (interface):  Register
      IKafkaProducerSpecification (interface):  CreateSendTransportContext
      KafkaApiConfigurator (class):  KafkaApiConfigurator
      KafkaBusInstanceSpecification (class):  Configure, KafkaBusInstanceSpecification, Validate
      KafkaConsumerSpecification (class):  ConnectReceiveEndpointObserver, CreateReceiveEndpoint, KafkaConsumerSpecification, Validate
      KafkaFactoryConfigurator (class):  Build, ConfigureApi, ConfigureSend, ConfigureSocket, ConnectReceiveEndpointObserver, ConnectSendObserver, CreateSendTransportContext, CreateSpecification, Host, KafkaFactoryConfigurator, OAuthBearerTokenRefreshHandler, SetHeadersDeserializer, SetHeadersSerializer, SetSerializationFactory, TopicEndpoint
      KafkaFetchConfigurator (class):  KafkaFetchConfigurator
      KafkaHostConfigurator (class):  CancellationDelay, KafkaHostConfigurator, UseSasl, UseSsl
      KafkaProducerRegistration (class):  KafkaProducerRegistration, Register
      KafkaProducerSpecification (class):  ConnectSendObserver, CreateSendTransportContext, KafkaProducerSpecification, SetHeadersSerializer, SetKeySerializer, SetStatisticsHandler, SetValueSerializer, Validate
      KafkaRegistrationRiderFactory (class):  CreateRider, KafkaRegistrationRiderFactory
      KafkaSaslConfigurator (class):  KafkaSaslConfigurator
      KafkaSocketConfigurator (class):  KafkaSocketConfigurator
      KafkaSslConfigurator (class):  KafkaSslConfigurator
      KafkaTopicReceiveEndpointConfiguration (class):  Build, ConfigureFetch, CreateIfMissing, CreateReceiveEndpointContext, CreateReceiveKafkaEndpointContext, KafkaTopicReceiveEndpointConfiguration, SetHeadersDeserializer, SetKeyDeserializer, SetOffsetsCommittedHandler, SetStatisticsHandler, SetValueDeserializer, UseIsolationLevel, Validate
   MassTransit.KafkaIntegration.Middleware
      CombinedDeliveryMetrics (class):  CombinedDeliveryMetrics
      ConfigureKafkaTopologyFilter (class):  ConfigureKafkaTopologyFilter, CreateTopic, Probe, Send
      ConsumerSupervisor (class):  ConsumerSupervisor
      KafkaConsumerFilter (class):  CreateConsumerSupervisor, KafkaConsumerFilter, Probe, Send
   MassTransit.KafkaIntegration.Serializers
      DefaultKafkaSerializerFactory (class):  GetDeserializer, GetSerializer
      DeserializerTypes (class):  TryGet
      DictionaryHeadersDeserializer (class):  Deserialize
      DictionaryHeadersSerialize (class):  DictionaryHeadersSerialize
      DictionaryHeadersSerializer (class):  DictionaryHeadersSerializer, Serialize
      IHeadersDeserializer (interface):  Deserialize
      IHeadersSerializer (interface):  Serialize
      IKafkaSerializerFactory (interface):  GetDeserializer, GetSerializer
      KafkaHeaderValueConverter (class):  AlwaysCopy, CreateHeaderValue, KafkaHeaderValueConverter, Serialize, TryConvert
      MassTransitAsyncJsonSerializer (class):  SerializeAsync
      MassTransitJsonDeserializer (class):  Deserialize
      MassTransitJsonSerializer (class):  Serialize
      SerializerTypes (class):  TryGet
   MassTransit.Licensing
      LicenseContact (class)
      LicenseCustomer (class)
      LicenseFeature (class)
      LicenseFile (class)
      LicenseInfo (class)
      LicenseProduct (class)
      LicenseReader (class):  ExtractPayload, Load, LoadFromFile
      LicenseSettings (class):  LicenseSettings
   MassTransit.Logging
      BusLogContext (class):  BusLogContext, CreateLogContext
      Cached (class)
      Consumer (class)
      Courier (class)
      DiagnosticActivityExtensions (class):  AddConsumeContextTags, SetTag
      DiagnosticHeaders (class)
      EnabledLogger (struct):  EnabledLogger, Log
      Exceptions (class)
      ILogContext (interface):  CreateLogContext
         Used to provide access to logging and diagnostic services
      LogCategoryName (class)
      LogContextActivityExtensions (class):  GetParentActivityContext, PopulateSendActivity, StartActivity, StartCompensateActivity, StartConsumerActivity, StartExecuteActivity, StartGenericActivity, StartHandlerActivity, StartOutboxDeliverActivity, StartOutboxSendActivity, StartReceiveActivity, StartSagaActivity, StartSagaStateMachineActivity, StartSendActivity
      LogContextInstrumentationExtensions (class):  AddCustomTags, CleanupLabel, Configure, FormatTypeName, GetActivityTypeLabel, GetArgumentTypeLabel, GetConsumerTypeLabel, GetEndpointLabel, GetLogTypeLabel, GetMessageTypeLabel, StartActivityCompensateInstrument, StartActivityExecuteInstrument, StartConsumeInstrument, StartHandlerInstrument, StartOutboxDeliveryInstrument
      Messaging (class)
      OperationName (class)
      RabbitMq (class)
      Saga (class)
      SagaLogExtensions (class):  LogAdded, LogCreated, LogFault, LogInsert, LogInsertFault, LogRemoved, LogUsed
      SingleLoggerFactory (class):  AddProvider, CreateLogger, Dispose, SingleLoggerFactory
      StartedActivity (struct):  AddExceptionEvent, SetTag, StartedActivity, Stop, Update
      StartedInstrument (struct):  AddException, StartedInstrument, Stop
      TestDisposable (class):  Dispose
      TextWriterLogger (class):  BeginScope, IsEnabled, Log, TextWriterLogger
      TextWriterLoggerFactory (class):  AddProvider, CreateLogger, Dispose, TextWriterLoggerFactory
      TextWriterLoggerOptions (class):  Disable, IsEnabled, TextWriterLoggerOptions
      Transport (class)
   MassTransit.MartenIntegration.Saga
      MartenSagaRepository (class):  Create
      MartenSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, Insert, Load, MartenSagaRepositoryContext, Query, Save, Undo, Update
      MartenSagaRepositoryContextFactory (class):  Execute, ExecuteAsyncMethod, MartenSagaRepositoryContextFactory, Probe, Send, SendQuery
   MassTransit.Mediator
      IMediator (interface)
      IScopedMediator (interface)
      MassTransitMediator (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectPublishObserver, ConnectRequestPipe, ConnectSendObserver, CreateRequest, CreateRequestClient, DisposeAsync, GetPublishSendEndpoint, MassTransitMediator, Publish, PublishInternal, Send
         Sends messages directly to the , without serialization
      MediatorRequestHandler (class):  Consume
         A Mediator request handler base class that provides a simplified overridable method with a Task (void) return type
      Request (interface)
         A mediator interface, signaling the response type expected by a request
   MassTransit.Mediator.Contexts
      FaultContext (class):  FaultContext
      MediatorClientFactoryContext (class):  ConnectConsumePipe, ConnectRequestPipe, GetRequestEndpoint, MediatorClientFactoryContext
      MediatorPipe (class):  MediatorPipe, Probe, Send
      MediatorPublishSendEndpoint (class):  ConnectPublishObserver, MediatorPublishSendEndpoint
      MediatorReceiveContext (class):  AddReceiveTask, MediatorReceiveContext, NotifyConsumed, NotifyFaulted
      MediatorRequestSendEndpoint (class):  MediatorRequestSendEndpoint
      MediatorSendEndpoint (class):  ConnectPublishObserver, ConnectSendObserver, CreateSendContext, GetPublishSendEndpoint, GetSendEndpoint, MediatorSendEndpoint, Send, SendMessage
      MediatorSerializationContext (class):  GetMessageSerializer, MediatorSerializationContext, ToDictionary, TryGetMessage
      PublishPipeAdapter (class):  Probe, PublishPipeAdapter, Send
   MassTransit.MessageData
      EncryptedMessageDataRepository (class):  EncryptedMessageDataRepository, Get, Put
      FileSystemMessageDataRepository (class):  FileSystemMessageDataRepository, GenerateFilePath, Get, ParseFilePath, Put, VerifyDirectory
      IInlineMessageData (interface):  Set
      IMessageDataReference (interface)
      InMemoryMessageDataId (class):  InMemoryMessageDataId
      InMemoryMessageDataRepository (class):  Get, InMemoryMessageDataRepository, Put
   MassTransit.MessageData.Configuration
      CourierMessageDataConfigurationObserver (class):  ActivityConfigured, CompensateActivityConfigured, CourierMessageDataConfigurationObserver, ExecuteActivityConfigured, MessageConfigured, Method4, Method5, Method6
      GetMessageDataObjectArrayTransformConfiguration (class):  Apply, GetMessageDataObjectArrayTransformConfiguration
      GetMessageDataObjectDictionaryTransformConfiguration (class):  Apply, GetMessageDataObjectDictionaryTransformConfiguration
      GetMessageDataObjectTransformConfiguration (class):  Apply, GetMessageDataObjectTransformConfiguration
      GetMessageDataTransformConfiguration (class):  Apply, GetMessageDataTransformConfiguration
      GetMessageDataTransformSpecification (class):  AddMessageDataProperties, Apply, GetMessageDataTransformSpecification, IsUnknownObjectType, TryGetConsumeTopology, TryGetConverter
      IMessageDataTransformConfiguration (interface):  Apply
      MessageDataRepositorySelectorExtensions (class):  Encrypted, FileSystem, InMemory
      PutMessageDataObjectArrayTransformConfiguration (class):  Apply, PutMessageDataObjectArrayTransformConfiguration
      PutMessageDataObjectDictionaryTransformConfiguration (class):  Apply, PutMessageDataObjectDictionaryTransformConfiguration
      PutMessageDataObjectTransformConfiguration (class):  Apply, PutMessageDataObjectTransformConfiguration
      PutMessageDataTransformConfiguration (class):  Apply, PutMessageDataTransformConfiguration
      PutMessageDataTransformSpecification (class):  AddMessageDataProperties, Apply, IsUnknownObjectType, PutMessageDataTransformSpecification, TryGetConverter, TryGetSendTopology
   MassTransit.MessageData.Conventions
      Factory (class):  Create, Factory
      IMessageDataMessageConsumeTopologyConvention (interface)
      IMessageDataMessageSendTopologyConvention (interface)
      MessageDataConsumeTopologyConvention (class):  MessageDataConsumeTopologyConvention, TryGetMessageConsumeTopologyConvention
      MessageDataMessageConsumeTopology (class):  Apply, MessageDataMessageConsumeTopology
      MessageDataMessageConsumeTopologyConvention (class):  MessageDataMessageConsumeTopologyConvention, TryGetMessageConsumeTopology, TryGetMessageConsumeTopologyConvention
      MessageDataMessageSendTopology (class):  Apply, MessageDataMessageSendTopology
      MessageDataMessageSendTopologyConvention (class):  MessageDataMessageSendTopologyConvention, TryGetMessageSendTopology, TryGetMessageSendTopologyConvention
      MessageDataSendTopologyConvention (class):  MessageDataSendTopologyConvention, TryGetMessageSendTopologyConvention
   MassTransit.MessageData.Converters
      ByteArrayMessageDataConverter (class):  Convert
      MessageDataConverter (class)
      StreamMessageDataConverter (class):  Convert
      StringMessageDataConverter (class):  Convert
      SystemTextJsonObjectMessageDataConverter (class):  Convert, SystemTextJsonObjectMessageDataConverter
   MassTransit.MessageData.PropertyProviders
      BytesMessageDataReader (class):  GetMessageData
      GetMessageDataPropertyProvider (class):  GetMessageDataPropertyProvider, GetProperty
      IMessageDataReader (interface):  GetMessageData
      MessageDataReaderFactory (class):  CreateReader
      ObjectMessageDataReader (class):  GetMessageData, ObjectMessageDataReader
      PutMessageDataPropertyProvider (class):  GetProperty, Put, PutMessageDataPropertyProvider
      StreamMessageDataReader (class):  GetMessageData
      StringMessageDataReader (class):  GetMessageData
   MassTransit.MessageData.Values
      BytesInlineMessageData (class):  BytesInlineMessageData, GetValue, Set
      DeserializedMessageData (class):  DeserializedMessageData
         When a message data property is deserialized, this is used as a placeholder for the actual message data accessor whic...
      EmptyMessageData (class):  EmptyMessageData, NoValue
      GetMessageData (class):  GetMessageData, GetValue
         Gets the message data when accessed via Value, using the specified repository and converter.
      InlineMessageData (class):  InlineMessageData, Set
      PutMessageData (class):  PutMessageData
         Message data that needs to be stored in the repository when the message is sent.
      StoredMessageData (class):  StoredMessageData
         MessageData that has been stored by the repository, has a valid address, and is ready to be serialized.
      StringInlineMessageData (class):  Set, StringInlineMessageData
   MassTransit.Metadata
      Activation (class):  Activate
      BusHostInfo (class):  BusHostInfo, GetAssemblyFileVersion, GetAssemblyInformationalVersion, GetUsefulProcessName
      Cached (class)
      CachedType (interface):  ActivateType, ImplementsType
      Factory (struct):  ActivateType
      HostMetadataCache (class):  GetCommitHash
      IActivationType (interface):  ActivateType
      IImplementedMessageType (interface):  ImplementsMessageType
      IImplementedMessageTypeCache (interface):  EnumerateImplementedTypes
      IMessageDataConverter (interface):  Convert
      INServiceBusTypeMetadataCache (interface)
      ITypeMetadataCache (interface)
      ImplementedMessageTypeCache (class):  EnumerateImplementedTypes, GetMessageTypes, ImplementedMessageTypeCache, Method1, Method2, Method3
      ImplementedType (struct):  ImplementedType
      NServiceBusTypeCache (class):  GetMessageTypeNames, NServiceBusTypeCache
      RegistrationMetadata (class):  IsActivityOrDefinition, IsConsumer, IsConsumerOrDefinition, IsFutureOrDefinition, IsSaga, IsSagaOrDefinition, IsSagaStateMachineOrDefinition
      TypeAdapter (class):  ActivateType, ImplementsType, Method1, Method2, Method3, TypeAdapter
      TypeMetadataCache (class):  GetImplementationType, GetMessageTypeNames, GetMessageTypes, GetProperties, GetShortName, IsTemporaryMessageType, IsValidMessageDataType, IsValidMessageType, TypeMetadataCache
   MassTransit.Middleware
      Agent (class):  Agent, SetNotReady, SetReady, Stop
         An Agent Provocateur that simply exists, out of context
      AsyncDelegateFilter (class):  AsyncDelegateFilter, Probe, Send
      BasePipeContext (class):  AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, TryGetPayload
         The base for a pipe context, with the underlying support for managing payloads (out-of-band data that is carried alon...
      BindPipe (class):  BindPipe, Probe, Send
      Cache (class)
      CircuitBreakerFilter (class):  CircuitBreakerFilter, Close, ClosePartially, Open, Probe, Send
      CommandContextConverter (class):  TryConvert
      CompensateActivityFactoryFilter (class):  CompensateActivityFactoryFilter, Probe, Send
      CompensateActivityFilter (class):  CompensateActivityFilter, Probe, Send
         Compensates an activity as part of an activity execute host pipe
      ConcurrencyLimitFilter (class):  ConcurrencyLimitFilter, Dispose, Probe, Send
         Limits the concurrency of the next section of the pipeline based on the concurrency limit specified.
      ConcurrencyLimiter (class):  ConcurrencyLimiter, Consume, Release, Wait
         A concurrency limiter (using a semaphore) which can be shared, and adjusted using a management endpoint.
      ConsumeConcurrencyLimitFilter (class):  ConsumeConcurrencyLimitFilter, Probe, Send
         A concurrency limit filter that is shared by multiple message types, so that a consumer accepting those various types...
      ConsumeContextConverterFactory (class):  GetConverter
      ConsumeContextMessageTypeFilter (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectMessagePipe, ConsumeContextMessageTypeFilter, CreateOutputPipe, GetMessagePipe, Probe, Send
         Converts ConsumeContext to ConsumeContextT for a given message type type.
      ConsumeContextOutputMessageTypeFilter (class):  ConnectConsumeMessageObserver, ConnectPipe, ConsumeContextOutputMessageTypeFilter, Probe, Send, SendToOutput
         Converts an inbound context type to a pipe context type post-dispatch
      ConsumePipe (class):  BuildMessagePipe, ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectRequestPipe, ConsumePipe, Probe, Send
      ConsumerMergePipe (class):  ConsumerMergePipe, Probe, Send
         Merges the out-of-band consumer back into the pipe
      ConsumerMessageFilter (class):  ConsumerMessageFilter, Probe, Send
         Consumes a message via Consumer, resolved through the consumer factory and notifies the context that the message was ...
      ConsumerMessageMergePipe (class):  ConsumerMessageMergePipe, Probe, Send
         Merges the out-of-band consumer back into the context
      ConsumerSplitFilter (class):  ConsumerSplitFilter, Probe, Send
         Splits a context item off the pipe and carries it out-of-band to be merged once the next filter has completed
      Context (class):  Context
      ContextFilter (class):  ContextFilter, Probe, Send
         A content filter applies a delegate to the message context, and uses the result to either accept the message or disca...
      ContextPartitioner (class):  ContextPartitioner, Probe, Send
      Converter (class):  TryConvert
      CopyContextPipe (class):  CopyContextPipe, Probe, Send
      CorrelatedSagaFilter (class):  CorrelatedSagaFilter, Probe, Send
         Sends the message through the repository using the specified saga policy.
      CorrelationIdMessageFilter (class):  CorrelationIdMessageFilter, Probe, Send
         Extracts the CorrelationId from the message where there is a one-to-one correlation identifier in the message (such a...
      DeadLetterFilter (class):  DeadLetterFilter, Probe, Send
         If a message was neither delivered to a consumer nor caused a fault (which was notified already) then this filter wil...
      DeadLetterTransportFilter (class):  Probe, Send
         Moves a message received to a transport without any deserialization
      DelaySendPipe (class):  DelaySendPipe, Probe, Send
      DelayedMessageRedeliveryContext (class):  DelayedMessageRedeliveryContext, ScheduleRedelivery
      DelayedMessageRedeliveryFilter (class):  DelayedMessageRedeliveryFilter, Probe, Send
         Uses a delayed exchange in ActiveMQ to delay a message retry
      DelayedMessageSchedulerFilter (class):  Probe, SchedulerFactory, Send
      DelegateFilter (class):  DelegateFilter, Probe, Send
      DeserializeFilter (class):  DeserializeFilter, Probe, Send
         Performs the deserialization of a message ReceiveContext and passes the resulting ConsumeContext to the output pipe.
      DiscardDeadLetterFilter (class):  Probe, Send
         Simply ignores/discards the not-consumed message
      DiscardErrorTransportFilter (class):  Probe, Send
         Discard the error instead of moving it to the error transport.
      DynamicFilter (class):  ConnectObserver, ConnectPipe, DynamicFilter, Probe, Send
         Dispatches an inbound pipe to one or more output pipes based on a dispatch type.
      DynamicRouter (class):  ConnectObserver, ConnectPipe, DynamicRouter, Probe, Send
         A dynamic router is a pipe on which additional pipes can be connected and context is routed through the pipe based up...
      EmptyPipe (class):  Probe, Send
      ErrorTransportFilter (class):  Probe, Send
         In the case of an exception, the message is moved to the destination transport.
      EventContextConverter (class):  TryConvert
      ExecuteActivityFactoryFilter (class):  ExecuteActivityFactoryFilter, Probe, Send
      ExecuteActivityFilter (class):  ExecuteActivityFilter, Probe, Send
         Executes an activity as part of an activity execute host pipe
      FaultDeadLetterFilter (class):  Probe, Send
      FilterPipe (class):  FilterPipe, Probe, Send
      ForkFilter (class):  ForkFilter, Probe, Send
         Forks a single pipe into two pipes, which are executed concurrently
      FutureRequestPipe (class):  FutureRequestPipe, Probe, Send
      FutureResultPipe (class):  FutureResultPipe, Probe, Send
      GenerateFaultFilter (class):  GenerateFault, Probe, Send
         Generates and publishes a event for the exception
      Handle (class):  Disconnect, Dispose, Handle
      HandlerMessageFilter (class):  HandlerMessageFilter, Probe, Send
         Consumes a message via a message handler and reports the message as consumed or faulted
      IConcurrencyLimiter (interface):  Release, Wait
      IConsumeContextMessageTypeFilter (interface):  ConnectMessagePipe
      IConsumeContextOutputMessageTypeFilter (interface):  ConnectPipe
      IConsumerMessageFilter (interface)
         Adapts a consumer to consume the message type
      IDynamicFilter (interface)
      IDynamicRouter (interface)
         A dynamic router is a pipe on which additional pipes can be connected and context is routed through the pipe based up...
      IHashGenerator (interface):  Hash
         Generates a hash of the input data for partitioning purposes
      IKeyPipeConnector (interface):  ConnectPipe
         Supports connecting a pipe using a key, which is a method of dispatching to different pipes based on context.
      ILatestFilter (interface)
         Maintains the latest context to be passed through the filter
      IMessagePipe (interface):  BuildMessagePipe, Send
      IMessagePublishPipe (interface)
      IMessageSendPipe (interface)
      IOutboxContextFactory (interface):  Send
      IOutputPipeFilter (interface)
      IPipeConnector (interface):  ConnectPipe
         The intent is to connect a pipe of a specific type to a pipe of a different type, for which there is a provider that ...
      IPipeContextConverter (interface):  TryConvert
         Converts the input context to the output context
      IPipeContextConverterFactory (interface):  GetConverter
      IPipeRouter (interface)
      IRequestIdTeeFilter (interface)
      ISagaMessageFilter (interface)
         Adapts a consumer to consume the message type
      ITeeFilter (interface)
      InMemoryOutboxFilter (class):  InMemoryOutboxFilter, Method1, Method2, Method3, Probe, Send
      InitiatedByOrOrchestratesSagaMessageFilter (class):  Probe, Send
         Dispatches the ConsumeContext to the consumer method for the specified message type
      InitiatedBySagaMessageFilter (class):  Probe, Send
         Dispatches the ConsumeContext to the consumer method for the specified message type
      InlineFilter (class):  InlineFilter, Probe, Send
      InstanceMessageFilter (class):  InstanceMessageFilter, Probe, Send
         Consumes a message via an existing class instance
      InterceptFilter (class):  InterceptFilter, Probe, Send
         Intercepts the pipe and executes an adjacent pipe prior to executing the next filter in the main pipe
      InternalOutboxExtensions (class):  SkipOutbox
      JobConsumerMessageFilter (class):  JobConsumerMessageFilter, Probe, RunJob, Send
         Converts the ConsumeContext to a JobContext, and executes the job
      KeyFilter (class):  ConnectPipe, KeyFilter, Probe, RemovePipe, Send
         Handles the registration of requests and connecting them to the consume pipe
      Last (class):  Probe, Send
      LastPipe (class):  LastPipe, Probe, Send
         The last pipe in a pipeline is always an end pipe that does nothing and returns synchronously
      LatestFilter (class):  GetLatest, LatestFilter, Probe, Send
         Retains the last value that was sent through the filter, usable as a source to a join pipe
      MergePipe (class):  MergePipe, Probe, Send
      MessagePipe (class):  BuildMessagePipe, CreateMessagePipe, MessagePipe, Probe, Send
      MessagePublishPipe (class):  MessagePublishPipe, Probe, Send
         Converts an inbound context type to a pipe context type post-dispatch
      MessageSchedulerFilter (class):  MessageSchedulerFilter, Probe, SchedulerFactory, Send
         Adds the scheduler to the consume context, so that it can be used for message redelivery
      MessageSendPipe (class):  MessageSendPipe, Probe, Send
         Converts an inbound context type to a pipe context type post-dispatch
      MessageSplitFilter (class):  MessageSplitFilter, Probe, Send
         Splits a context item off the pipe and carries it out-of-band to be merged once the next filter has completed
      MethodConsumerMessageFilter (class):  Probe, Send
         Dispatches the ConsumeContext to the consumer method for the specified message type
      MissingInstanceRedeliveryPipe (class):  MissingInstanceRedeliveryPipe, Probe, Send
      MissingSagaPipe (class):  MissingSagaPipe, Probe, Send
         Dispatches a missing saga message to the saga policy, calling Add if necessary
      Murmur3UnsafeHashGenerator (class):  Hash, Rotl32
      ObserverMessageFilter (class):  ObserverMessageFilter, Probe, Send
         Consumes a message via a message handler and reports the message as consumed or faulted
      ObservesSagaMessageFilter (class):  Probe, Send
         Dispatches the ConsumeContext to the consumer method for the specified message type
      OrchestratesSagaMessageFilter (class):  Probe, Send
         Dispatches the ConsumeContext to the consumer method for the specified message type
      OutboxConsumeContext (interface):  LoadOutboxMessages, NotifyOutboxMessageDelivered, RemoveOutboxMessages, SetConsumed, SetDelivered
      OutboxConsumeFilter (class):  OutboxConsumeFilter, Probe, Send
         Sends the message through the outbox
      OutboxConsumeOptions (class)
      OutboxMessageContext (interface)
      OutboxMessageHeaderProvider (class):  GetAll, OutboxMessageHeaderProvider, TryGetHeader
      OutboxMessagePipe (class):  DeliverOutboxMessages, OutboxMessagePipe, Probe, Send
      OutboxMessageSendPipe (class):  OutboxMessageSendPipe, Probe, Send
      OutboxSendContext (interface):  AddSend
         Used by the new outbox construct
      OutputPipeFilter (class):  ConnectObserver, ConnectPipe, OutputPipeFilter, Probe, Send, SendToOutput
         Converts an inbound context type to a pipe context type post-dispatch
      Partition (class):  DisposeAsync, Partition, Probe, Send
      PartitionFilter (class):  PartitionFilter, Probe, Send
      Partitioner (class):  DisposeAsync, GetPartitioner, Partitioner, Probe, Send
      PayloadFilter (class):  PayloadFilter, Probe, Send
      PipeContextConverterFactory (class):  GetConverter
      PipeContextSourceBindFilter (class):  PipeContextSourceBindFilter, Probe, Send
         Binds a context to the pipe using a .
      PipeRouter (class):  PipeRouter
      ProxyPipeContext (class):  AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, TryGetPayload
         The base for any pipe context proxy, optimized to avoid member access
      PublishMessageSchedulerFilter (class):  Probe, SchedulerFactory, Send
         Adds the scheduler to the consume context, so that it can be used for message redelivery
      PublishPipe (class):  Probe, PublishPipe, Send
      QuerySagaFilter (class):  Probe, QuerySagaFilter, Send
         Creates a filter to send a query to the saga repository using the query factory and saga policy provided.
      RateLimitFilter (class):  Dispose, Probe, RateLimitFilter, Reset, Send
         Limits the number of calls through the filter to a specified count per time interval specified.
      ReceiveEndpointDependencyFilter (class):  Probe, ReceiveEndpointDependencyFilter, Send
      ReceivePipe (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectRequestPipe, Probe, ReceivePipe, Send
      RedeliveryRetryFilter (class):  Probe, RedeliveryRetryFilter, Send
         Uses the message redelivery mechanism, if available, to delay a retry without blocking message delivery
      RequestIdFilter (class):  ConnectPipe, Probe, RemovePipe, RequestIdFilter, Send
         Handles the registration of requests and connecting them to the consume pipe
      RequestIdTeeFilter (class):  ConnectKeyFilter, ConnectPipe, RequestIdTeeFilter
      RescueFilter (class):  Probe, RescueFilter, Send
         Rescue catches an exception, and if the exception matches the exception filter, passes control to the rescue pipe.
      RethrowErrorTransportFilter (class):  Probe, Send
      RetryBusObserver (class):  CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, RetryBusObserver, StartFaulted, StopFaulted
      RetryFilter (class):  Attempt, Probe, RetryFilter, Send
         Uses a retry policy to handle exceptions, retrying the operation in according with the policy
      SagaMergePipe (class):  Probe, SagaMergePipe, Send
         Merges the out-of-band message back into the pipe
      SagaMessageMergePipe (class):  Probe, SagaMessageMergePipe, Send
         Merges the out-of-band Saga back into the context
      SagaMessageSplitFilter (class):  Probe, SagaMessageSplitFilter, Send
         Splits a context item off the pipe and carries it out-of-band to be merged once the next filter has completed
      SagaSplitFilter (class):  Probe, SagaSplitFilter, Send
         Splits a context item off the pipe and carries it out-of-band to be merged once the next filter has completed
      ScheduleMessageRedeliveryFilter (class):  Probe, ScheduleMessageRedeliveryFilter, Send
         Adds the scheduler to the consume context, so that it can be used for message redelivery
      ScopeCompensateFilter (class):  Probe, ScopeCompensateFilter, Send
      ScopeConsumeFilter (class):  Probe, ScopeConsumeFilter, Send
      ScopeExecuteFilter (class):  Probe, ScopeExecuteFilter, Send
      ScopeMessageFilter (class):  Probe, ScopeMessageFilter, Send
      ScopePipeContext (class):  AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, TryGetPayload
      ScopedCompensateFilter (class):  Probe, ScopedCompensateFilter, Send
      ScopedConsumeFilter (class):  Probe, ScopedConsumeFilter, Send
      ScopedExecuteFilter (class):  Probe, ScopedExecuteFilter, Send
      ScopedFilter (class):  Probe, ScopedFilter, Send
      SendPipe (class):  Probe, Send, SendPipe
      SendQuerySagaPipe (class):  Probe, Send, SendQuerySagaPipe
      SendSagaPipe (class):  Probe, Send, SendSagaPipe
      SetCorrelationIdFilter (class):  Probe, Send, SetCorrelationIdFilter
         Sets the CorrelationId header uses the supplied implementation.
      SetPartitionKeyFilter (class):  Probe, Send, SetPartitionKeyFilter
      SetRoutingKeyFilter (class):  Probe, Send, SetRoutingKeyFilter
      SetSerializerFilter (class):  Probe, Send, SetSerializerFilter
         Sets the CorrelationId header uses the supplied implementation.
      SplitFilter (class):  Probe, Send, SplitFilter
      StateMachineSagaMessageFilter (class):  Probe, Send, StateMachineSagaMessageFilter
         Dispatches the ConsumeContext to the consumer method for the specified message type
      Supervisor (class):  Add, Remove, SetReady, Supervisor
         Supervises a set of agents, allowing for graceful Start, Stop, and Ready state management
      TeeFilter (class):  ConnectKeyFilter, ConnectPipe, Probe, Send, TeeFilter
         Connects multiple output pipes to a single input pipe
      TimeoutFilter (class):  Probe, Send, TimeoutFilter
      TransactionFilter (class):  Probe, Send, TransactionFilter
      TransformFilter (class):  Probe, Send, TransformFilter
         Applies a transform to the message
      TransportReadyFilter (class):  Probe, Send, TransportReadyFilter
   MassTransit.Middleware.CircuitBreaker
      CircuitBreakerEventExtensions (class):  PublishCircuitBreakerClosed, PublishCircuitBreakerOpened
      CircuitBreakerSettings (interface)
      Closed (class)
      ClosedBehavior (class):  ClosedBehavior, PostSend, PreSend, Probe, Reset, SendFault, TripThresholdExceeded
         Represents a closed, normally operating circuit breaker state
      HalfOpenBehavior (class):  HalfOpenBehavior, PostSend, PreSend, Probe, SendFault
         Executes until the success count is met.
      ICircuitBreaker (interface):  Close, ClosePartially, Open
         Provides access to a circuit breaker from a state object
      ICircuitBreakerBehavior (interface):  PostSend, PreSend, SendFault
      OpenBehavior (class):  GetTimer, OpenBehavior, PartiallyCloseCircuit, PostSend, PreSend, Probe, SendFault
         Represents a circuit that is unavailable, with a timer waiting to partially close the circuit.
      Opened (class):  Opened
   MassTransit.Middleware.InMemoryOutbox
      Batch (class):  Batch, DiscardPendingActions, ExecutePendingActions, NotifyConsumed, NotifyFaulted
      InMemoryOutboxCompensateContext (class):  Compensated, CreateActivityContext, Failed, InMemoryOutboxCompensateContext
      InMemoryOutboxConsumeContext (class):  Add, DiscardPendingActions, ExecutePendingActions, InMemoryOutboxConsumeContext, Method1, Method2, Method3, NotifyConsumed, NotifyFaulted
      InMemoryOutboxCourierContextProxy (class)
      InMemoryOutboxDeferredMethod (class):  Dispose, InMemoryOutboxDeferredMethod, Run
      InMemoryOutboxDeferredMethodCollection (class):  Add, Discard, Execute, InMemoryOutboxDeferredMethodCollection
      InMemoryOutboxExecuteContext (class):  Completed, CompletedWithVariables, CreateActivityContext, Faulted, FaultedWithVariables, InMemoryOutboxExecuteContext, ReviseItinerary, Terminate
      InMemoryOutboxMessageSchedulerContext (class):  AddCancelMessage, AddScheduledMessage, CancelAllScheduledMessages, CancelScheduledPublish, CancelScheduledSend, ExecutePendingActions, InMemoryOutboxMessageSchedulerContext, SchedulePublish, ScheduleSend
      InMemoryOutboxPublishEndpointProvider (class):  ConnectPublishObserver, GetPublishSendEndpoint, InMemoryOutboxPublishEndpointProvider
      InMemoryOutboxReceiveContext (class):  InMemoryOutboxReceiveContext
      InMemoryOutboxSendEndpointProvider (class):  ConnectSendObserver, GetSendEndpoint, InMemoryOutboxSendEndpointProvider
      OutboxContext (interface):  Add, DiscardPendingActions, ExecutePendingActions
         The context for an outbox instance as part of consume context.
      OutboxSendEndpoint (class):  ConnectSendObserver, CreateSendContext, OutboxSendEndpoint, Send
   MassTransit.Middleware.Outbox
      BusOutboxNotification (class):  BusOutboxNotification, Delivered, WaitForDelivery
      EqualityComparer (class):  Equals, GetHashCode
      IBusOutboxNotification (interface):  Delivered, WaitForDelivery
      InMemoryInboxMessage (class):  AddOutboxMessage, GetOutboxMessages, InMemoryInboxMessage, MarkInUse, Release, RemoveOutboxMessages
      InMemoryInboxMessageKey (struct):  InMemoryInboxMessageKey
      InMemoryOutboxConsumeContext (class):  AddSend, InMemoryOutboxConsumeContext, LoadOutboxMessages, NotifyOutboxMessageDelivered, RemoveOutboxMessages, SetConsumed, SetDelivered
      InMemoryOutboxContextFactory (class):  InMemoryOutboxContextFactory, Probe, Send
      InMemoryOutboxMessage (class):  Deserialize, DeserializerHeaders, DeserializerProperties
      InMemoryOutboxMessageRepository (class):  InMemoryOutboxMessageRepository, Lock, MarkInUse, Release
      OutboxConsumeContextProxy (class):  AddSend, GetService, LoadOutboxMessages, NotifyOutboxMessageDelivered, RemoveOutboxMessages, SetConsumed, SetDelivered
      OutboxMessageStaticData (class)
      OutboxPublishEndpointProvider (class):  ConnectPublishObserver, GetPublishSendEndpoint, OutboxPublishEndpointProvider
      OutboxReceiveContext (class):  OutboxReceiveContext
      OutboxSendEndpoint (class):  AddSend, ConnectSendObserver, CreateSendContext, OutboxSendEndpoint, Send
      OutboxSendEndpointPipe (class):  OutboxSendEndpointPipe
      OutboxSendEndpointProvider (class):  ConnectSendObserver, GetSendEndpoint, OutboxSendEndpointProvider
   MassTransit.Middleware.Rescue
      RescueExceptionConsumeContext (class):  RescueExceptionConsumeContext
      RescueExceptionConsumerConsumeContext (class):  RescueExceptionConsumerConsumeContext
      RescueExceptionReceiveContext (class):  RescueExceptionReceiveContext
      RescueExceptionSagaConsumeContext (class):  RescueExceptionSagaConsumeContext, SetCompleted
   MassTransit.Middleware.Timeout
      TimeoutCompensateContext (class):  Compensated, CreateActivityContext, Failed, TimeoutCompensateContext
      TimeoutConsumeContext (class):  NotifyConsumed, NotifyFaulted, TimeoutConsumeContext
      TimeoutCourierContextProxy (class)
      TimeoutExecuteContext (class):  Completed, CompletedWithVariables, CreateActivityContext, Faulted, FaultedWithVariables, ReviseItinerary, Terminate, TimeoutExecuteContext
   MassTransit.MongoDbIntegration
      BusOutboxDeliveryService (class):  AbortTransaction, BusOutboxDeliveryService, DeliverOutbox, DeliverOutboxMessages, GetOutboxes, ProcessOutboxes, RemoveOutbox
      CollectionNameFormatterExtensions (class):  GetCollection
      ICollectionNameFormatter (interface):  Collection, Saga
      InboxCleanupService (class):  CleanUpInboxState, EnsureIndexCreated, InboxCleanupService, RemoveInboxStates
         The Inbox Cleanup Service is responsible for removing entries after the expiration window timeout has elapsed.
      MongoDbCollectionContext (interface):  DeleteMany, DeleteOne, Find, FindOneAndReplace, InsertOne, Lock
         Encapsulates so that it can be including an existing session/transaction
      MongoDbConsumeContextScopedBusContext (class):  MongoDbConsumeContextScopedBusContext
      MongoDbContext (interface):  AbortTransaction, BeginTransaction, CommitTransaction, GetCollection, StartSession
      MongoDbScopedBusContext (class):  AddSend, Dispose, GetService, MongoDbScopedBusContext, StartOutboxTransaction, WasCommitted
      MongoDbScopedBusContextProvider (class):  Dispose, MongoDbScopedBusContextProvider
      NoMongoDbSessionContext (class):  AbortTransaction, BeginTransaction, CommitTransaction, Dispose, GetCollection, NoMongoDbSessionContext, StartSession
         No transaction support, since no client is available
      NoSessionMongoDbCollectionContext (class):  DeleteMany, DeleteOne, Find, FindOneAndReplace, InsertOne, Lock, NoSessionMongoDbCollectionContext
      SagaConvention (class):  Apply
      TransactionMongoDbCollectionContext (class):  DeleteMany, DeleteOne, Find, FindOneAndReplace, InsertOne, Lock, TransactionMongoDbCollectionContext
      TransactionMongoDbContext (class):  AbortTransaction, BeginTransaction, CommitTransaction, Dispose, GetCollection, StartSession, TransactionMongoDbContext
         Allows the creation of a session with a transaction
   MassTransit.MongoDbIntegration.Audit
      AuditDocument (class)
      AuditHeaders (class):  AuditHeaders
      MongoDbAuditStore (class):  MongoDbAuditStore, StoreMessage
   MassTransit.MongoDbIntegration.Courier
      IRoutingSlipEventPersister (interface):  Persist
      RoutingSlipEventPersister (class):  Persist, RoutingSlipEventPersister
   MassTransit.MongoDbIntegration.Courier.Consumers
      RoutingSlipActivityCompensatedConsumer (class):  Consume, RoutingSlipActivityCompensatedConsumer
      RoutingSlipActivityCompensationFailedConsumer (class):  Consume, RoutingSlipActivityCompensationFailedConsumer
      RoutingSlipActivityCompletedConsumer (class):  Consume, RoutingSlipActivityCompletedConsumer
      RoutingSlipActivityFaultedConsumer (class):  Consume, RoutingSlipActivityFaultedConsumer
      RoutingSlipCompensationFailedConsumer (class):  Consume, RoutingSlipCompensationFailedConsumer
      RoutingSlipCompletedConsumer (class):  Consume, RoutingSlipCompletedConsumer
      RoutingSlipFaultedConsumer (class):  Consume, RoutingSlipFaultedConsumer
      RoutingSlipRevisedConsumer (class):  Consume, RoutingSlipRevisedConsumer
      RoutingSlipTerminatedConsumer (class):  Consume, RoutingSlipTerminatedConsumer
   MassTransit.MongoDbIntegration.Courier.Documents
      ActivityExceptionDocument (class):  ActivityExceptionDocument
      ExceptionInfoDocument (class):  ExceptionInfoDocument
      HostDocument (class):  HostDocument
      RoutingSlipDocument (class):  RoutingSlipDocument
   MassTransit.MongoDbIntegration.Courier.Events
      RoutingSlipActivityCompensatedDocument (class):  RoutingSlipActivityCompensatedDocument
      RoutingSlipActivityCompensationFailedDocument (class):  RoutingSlipActivityCompensationFailedDocument
      RoutingSlipActivityCompletedDocument (class):  RoutingSlipActivityCompletedDocument
      RoutingSlipActivityFaultedDocument (class):  RoutingSlipActivityFaultedDocument
      RoutingSlipCompensationFailedDocument (class):  RoutingSlipCompensationFailedDocument
      RoutingSlipCompletedDocument (class):  RoutingSlipCompletedDocument
      RoutingSlipEventDocument (class)
      RoutingSlipFaultedDocument (class):  RoutingSlipFaultedDocument
      RoutingSlipRevisedDocument (class):  RoutingSlipRevisedDocument
      RoutingSlipTerminatedDocument (class):  RoutingSlipTerminatedDocument
   MassTransit.MongoDbIntegration.MessageData
      IFileNameGenerator (interface):  GenerateFileName
      IMessageDataResolver (interface):  GetAddress, GetObjectId
      MessageDataResolver (class):  GetAddress, GetObjectId
      MongoDbMessageDataRepository (class):  BuildGridFsUploadOptions, Get, MongoDbMessageDataRepository, Put
      NewIdFileNameGenerator (class):  GenerateFileName
   MassTransit.MongoDbIntegration.Outbox
      InboxState (class)
      MongoDbOutboxConsumeContext (class):  AddSend, LoadOutboxMessages, MongoDbOutboxConsumeContext, NotifyOutboxMessageDelivered, RemoveOutboxMessages, SetConsumed, SetDelivered
      MongoDbOutboxContextFactory (class):  AbortTransaction, MongoDbOutboxContextFactory, Probe, Send
      MongoDbOutboxExtensions (class):  AddSend
      OutboxMessage (class):  Deserialize, DeserializerHeaders, DeserializerProperties
      OutboxMessageStaticData (class)
      OutboxState (class)
         Used by the sweeper to track the state of an outbox, to ensure that it is properly locked across sweeper instances to...
   MassTransit.MongoDbIntegration.Saga
      DefaultCollectionNameFormatter (class):  Collection, DefaultCollectionNameFormatter, Saga
      DotCaseCollectionNameFormatter (class):  Collection, DotCaseCollectionNameFormatter, Pluralize, Saga
         Formats the saga collection names using dot naming with Pluralize SimpleSaga -> simple.sagas
      MongoDbSagaRepository (class):  Create
      MongoDbSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, Insert, Load, MongoDbSagaRepositoryContext, Query, Save, Undo, Update
      MongoDbSagaRepositoryContextFactory (class):  Execute, ExecuteAsyncMethod, MongoDbSagaRepositoryContextFactory, Probe, Send, SendQuery
   MassTransit.Monitoring
      BusHealthCheck (class):  BusHealthCheck, CheckHealthAsync
      ConfigureBusHealthCheckServiceOptions (class):  Configure, ConfigureBusHealthCheckServiceOptions
      ConfigureDefaultInstrumentationOptions (class):  Configure
      Endpoint (class):  Endpoint, ToString
      EndpointDictionary (class):  EndpointDictionary, ToString
      InstrumentationOptions (class)
   MassTransit.Monitoring.Performance
      BasePerformanceCounters (class):  Initialize
      BuiltInCounters (class)
      Cached (class)
      ConsumerPerformanceCounter (class):  Consumed, ConsumerPerformanceCounter, Dispose, Faulted
         Tracks the consumption and failure of a consumer processing messages.
      ConsumerPerformanceCounterCache (class):  ConsumerPerformanceCounterCache, GetCounter
      ConsumerPerformanceCounters (class):  Install
      Consumers (class)
      Counter (class):  Counter
      CounterCategory (class):  CounterCategory
      CounterData (class)
      Counters (class)
      IConsumerPerformanceCounter (interface):  Consumed, Faulted
      ICounterFactory (interface):  Create
      IMessagePerformanceCounter (interface):  ConsumeFaulted, Consumed, PublishFaulted, Published, SendFaulted, Sent
      IPerformanceCounter (interface):  Increment, IncrementBy, Set
      IPerformanceCounterInstaller (interface):  Install
      ISendPerformanceCounter (interface):  Faulted, Sent
      MessagePerformanceCounter (class):  ConsumeFaulted, Consumed, Dispose, MessagePerformanceCounter, PublishFaulted, Published, SendFaulted, Sent
      MessagePerformanceCounterCache (class):  Counter
      MessagePerformanceCounters (class):  Install
      Messages (class)
      PerformanceCounterBusObserver (class):  CreateFaulted, PerformanceCounterBusObserver, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted
      PerformanceCounterPublishObserver (class):  PerformanceCounterPublishObserver, PostPublish, PrePublish, PublishFault
         An observer that updates the performance counters using the bus events generated.
      PerformanceCounterReceiveObserver (class):  ConsumeFault, PerformanceCounterReceiveObserver, PostConsume, PostReceive, PreReceive, ReceiveFault
         An observer that updates the performance counters using the bus events generated.
      PerformanceCounterSendObserver (class):  PerformanceCounterSendObserver, PostSend, PreSend, SendFault
         An observer that updates the performance counters using the bus events generated.
   MassTransit.Monitoring.Performance.Null
      NullCounterFactory (class):  Create
      NullPerformanceCounter (class):  Dispose, Increment, IncrementBy, Set
   MassTransit.Monitoring.Performance.StatsD
      StatsDConfiguration (class):  Defaults, StatsDConfiguration
      StatsDCounterFactory (class):  Create, StatsDCounterFactory
      StatsDPerformanceCounter (class):  Dispose, Increment, IncrementBy, Set, StatsDPerformanceCounter
   MassTransit.NHibernateIntegration
      BinaryGuidType (class):  Assemble, DeepCopy, Disassemble, Equals, GetHashCode, NullSafeGet, NullSafeSet, Replace
      CompositeEventStatusUserType (class):  Assemble, DeepCopy, Disassemble, Equals, GetHashCode, NullSafeGet, NullSafeSet, Replace
         Used to map a CompositeEventStatus property to an int for storage by NHibernate.
      IntStateUserTypeConverter (class):  Get, IntStateUserTypeConverter, Set
         Converts a State to an int, based on the given ordered array of states, for storage using NHibernate.
      NHibernateSagaRepository (class):  Create
      NHibernateSessionFactoryProvider (class):  ApplyDatabaseIntegration, CreateConfiguration, CreateModelMapper, CreateSessionFactory, GetSessionFactory, NHibernateSessionFactoryProvider, UpdateSchema
         Makes it easy to create an NHibernate Session factory using the mappings for sagas and such.
      SagaClassMapping (class)
         A base class that can be used to define an NHibernate saga map, which automatically sets the saga to not be lazy and ...
      SqlServerSessionFactoryProvider (class):  Integrate, SqlServerSessionFactoryProvider
      StateMachineStateUserType (class):  Assemble, DeepCopy, Disassemble, Equals, GetConverter, GetHashCode, NullSafeGet, NullSafeSet, Replace, SaveAsInt32, SaveAsString, SetStateUserTypeConverter
      StateUserTypeConverter (interface):  Get, Set
      StringStateUserTypeConverter (class):  Get, Set, StringStateUserTypeConverter
         The default storage
      UriUserType (class):  Assemble, DeepCopy, Disassemble, Equals, GetHashCode, NullSafeGet, NullSafeSet, Replace
         An NHibernate user type for storing a Uri
   MassTransit.NHibernateIntegration.Saga
      NHibernateSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, Insert, Load, NHibernateSagaRepositoryContext, Query, Save, Undo, Update
      NHibernateSagaRepositoryContextFactory (class):  Execute, ExecuteAsyncMethod, NHibernateSagaRepositoryContextFactory, Probe, Send, SendQuery
   MassTransit.NewIdFormatters
      Base32Formatter (class):  Base32Formatter, ConvertLongToBase32, Format
      DashedHexFormatter (class):  DashedHexFormatter, Format, HexToChar
      HexFormatter (class):  Format, HexFormatter, HexToChar
      ZBase32Formatter (class):  ConvertLongToBase32, Format, ZBase32Formatter
   MassTransit.NewIdParsers
      Base32Parser (class):  Base32Parser, ConvertLongToBase16, Parse
      ZBase32Parser (class):  ZBase32Parser
   MassTransit.NewIdProviders
      BestPossibleWorkerIdProvider (class):  GetWorkerId
      CurrentProcessIdProvider (class):  GetProcessId
      DateTimeTickProvider (class)
      HostNameHashWorkerIdProvider (class):  GetNetworkAddress, GetWorkerId
      NetworkAddressWorkerIdProvider (class):  GetNetworkAddress, GetWorkerId
      StopwatchTickProvider (class):  StopwatchTickProvider
   MassTransit.Observables
      ActivityObservable (class):  CompensateFail, ExecuteFault, PostCompensate, PostExecute, PreCompensate, PreExecute
      BusObservable (class):  CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted
      Cached (class)
      ConsumeMessageObservable (class):  ConsumeFault, PostConsume, PreConsume
      ConsumeObservable (class):  ConsumeFault, PostConsume, PreConsume
      ConsumeObserverConverter (class):  ConsumeFault, PostConsume, PreConsume
         Converts the object message type to the generic type T and publishes it on the endpoint specified.
      FilterObservable (class):  Method4, Method5, Method6, PostSend, PreSend, SendFault
      IConsumeObserverConverter (interface):  ConsumeFault, PostConsume, PreConsume
         Calls the generic version of the IPublishEndpoint.Send method with the object's type
      IRetryFaultObserver (interface):  RetryFault
      PublishObservable (class):  PostPublish, PostSend, PrePublish, PreSend, PublishFault, SendFault
      ReceiveEndpointObservable (class):  Completed, Faulted, Ready, Stopping
      ReceiveObservable (class):  ConsumeFault, PostConsume, PostReceive, PreReceive, ReceiveFault
      ReceiveTransportObservable (class):  Completed, Faulted, Ready
      RetryFaultObserver (class):  RetryFault
      RetryFaultObserverCache (class):  CreateConverter, CreateTypeConverter, RetryFault
      RetryObservable (class):  PostCreate, PostFault, PreRetry, RetryComplete, RetryFault
      SendObservable (class):  PostSend, PreSend, SendFault
   MassTransit.Payloads
      IPayloadCache (interface):  AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, TryGetPayload
         The context properties
      ListPayloadCache (class):  AddOrUpdatePayload, GetOrAddPayload, HasPayloadType, ListPayloadCache, TryGetPayload
   MassTransit.QuartzIntegration
      CancelScheduledMessageConsumer (class):  CancelScheduledMessageConsumer, Consume
      ForwardScheduledMessagePipe (class):  ForwardScheduledMessagePipe, Probe, Send
      JobDataMessageContext (class):  ConvertDateTime, ConvertIdToGuid, ConvertToUri, Get, GetAll, GetEnumerator, GetHeaders, JobDataMessageContext, TryGetHeader
      MassTransitJobFactory (class):  MassTransitJobFactory, NewJob, ReturnJob
      PauseScheduledMessageConsumer (class):  Consume, PauseScheduledMessageConsumer
      QuartzBusObserver (class):  CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, QuartzBusObserver, StartFaulted, StopFaulted
         Used by container-based Quartz configurations, to start/stop Quartz along with the bus.
      QuartzConstants (class)
      QuartzTimeAdjustment (class):  AdvanceTime, Dispose, GetNow, GetUtcNow, QuartzTimeAdjustment
      ResumeScheduledMessageConsumer (class):  Consume, ResumeScheduledMessageConsumer
      ScheduleMessageConsumer (class):  Consume, EnsureJobExists, PopulateTrigger, ScheduleMessageConsumer, ToString
      ScheduledMessageJob (class):  Execute, ScheduledMessageJob
      SchedulerBusObserver (class):  CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, SchedulerBusObserver, StartFaulted, StopFaulted
         Used to start and stop an in-memory scheduler using Quartz
   MassTransit.RabbitMqTransport
      AmqpTimestampExtensions (class):  SetAmqpTimestamp, TryConvert
      ChannelContext (interface):  BasicAck, BasicCancel, BasicConsume, BasicNack, BasicPublishAsync, BasicQos, ExchangeBind, ExchangeDeclare, ExchangeDeclarePassive, NotifyFaulted, QueueBind, QueueDeclare, QueueDeclarePassive, QueuePurge
         With a connection, and a channel from RabbitMQ, this context is passed forward to allow the channel to be configured ...
      ChannelContextFactory (class):  ChannelContextFactory, CreateActiveContext, CreateChannel, CreateContext, CreateSharedChannel
      ChannelContextSupervisor (class):  ChannelContextSupervisor
      ConnectionContext (interface):  CreateChannel, CreateChannelContext
         A RabbitMQ connection
      ConnectionContextFactory (class):  ConnectionContextFactory, CreateActiveContext, CreateConnection, CreateContext, CreateSharedConnection
      ConnectionContextSupervisor (class):  ConnectionContextSupervisor, CreatePublishTransport, CreateSendTransport, NormalizeAddress
      DeadLetterSettings (interface):  GetBrokerTopology
      DelaySettings (interface):  GetBrokerTopology, GetSendAddress
      EntitySettings (interface)
      ErrorSettings (interface):  GetBrokerTopology
      IChannelContextSupervisor (interface)
         Attaches a channel context to the value
      IConnectionContextSupervisor (interface):  CreatePublishTransport, CreateSendTransport, NormalizeAddress
      IRabbitMqEndpointResolver (interface)
      IRabbitMqHost (interface)
      MessageExchangeTypeSelector (class):  GetExchangeType, MessageExchangeTypeSelector
      MoveTransportHeaders (class):  Get, GetAll, GetEnumerator, MoveTransportHeaders, Set, TryGetHeader
      OperationInterruptedExceptionExtensions (class):  ChannelShouldBeClosed
      RabbitMqAddressExtensions (class):  ApplySslOptions, GetConnectionFactory, GetHostSettings, GetRabbitMqHostTopology, GetReceiveSettings, IsReplyToAddress, UriDecode
      RabbitMqBasicConsumer (class):  HandleBasicCancelAsync, HandleBasicCancelOkAsync, HandleBasicConsumeOkAsync, HandleBasicDeliverAsync, HandleChannelShutdown, HandleChannelShutdownAsync, RabbitMqBasicConsumer
         Receives messages from RabbitMQ, pushing them to the InboundPipe of the service endpoint.
      RabbitMqChannelContext (class):  BasicAck, BasicCancel, BasicConsume, BasicNack, BasicPublishAsync, BasicQos, DisposeAsync, ExchangeBind, ExchangeDeclare, ExchangeDeclarePassive, NotifyFaulted, QueueBind, QueueDeclare, QueueDeclarePassive, QueuePurge
      RabbitMqConnectionContext (class):  CreateChannel, CreateChannelContext, DisposeAsync, RabbitMqConnectionContext
      RabbitMqDeadLetterTransport (class):  RabbitMqDeadLetterTransport, Send
      RabbitMqDeliveryMetrics (interface)
      RabbitMqErrorTransport (class):  RabbitMqErrorTransport, Send
      RabbitMqExchangeNames (class)
      RabbitMqExtensions (class):  Cleanup, ToDescription
      RabbitMqHeaderProvider (class):  GetAll, RabbitMqHeaderProvider, TryGetHeader
      RabbitMqHeaders (class)
      RabbitMqHost (class):  ConnectReceiveEndpoint, RabbitMqHost
      RabbitMqLogMessages (class)
      RabbitMqMessageNameFormatter (class):  GetMessageName, RabbitMqMessageNameFormatter
      RabbitMqMessageSendContext (class):  RabbitMqMessageSendContext, ReadPropertiesFrom, WritePropertiesTo
      RabbitMqMoveTransport (class)
      RabbitMqPublishTransportProvider (class):  GetPublishTransport, RabbitMqPublishTransportProvider
      RabbitMqQueueReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConvertException, Probe, RabbitMqQueueReceiveEndpointContext
      RabbitMqReceiveContext (class):  GetTransportProperties, RabbitMqReceiveContext
      RabbitMqReceiveEndpointContext (interface)
      RabbitMqReceiveLockContext (class):  Complete, Faulted, RabbitMqReceiveLockContext, ValidateLockStatus
      RabbitMqSendTransportContext (class):  CopyIncomingPropertiesIfPresent, CreateSendContext, GetAgentHandles, Probe, RabbitMqSendTransportContext, Send, SetHeaders
      RabbitMqSendTransportProvider (class):  GetSendTransport, NormalizeAddress, RabbitMqSendTransportProvider
      RabbitMqTransportPropertyNames (class)
      ReceiveSendEndpointProvider (class):  ConnectSendObserver, GetSendEndpoint, ReceiveSendEndpointProvider
      ReceiveSettings (interface):  GetInputAddress
         Specify the receive settings for a receive transport
      ReplyToPipe (class):  ReplyToPipe
      ReplyToSendEndpoint (class):  ReplyToSendEndpoint
      ScopeChannelContext (class):  BasicAck, BasicCancel, BasicConsume, BasicNack, BasicPublishAsync, BasicQos, Dispose, ExchangeBind, ExchangeDeclare, ExchangeDeclarePassive, NotifyFaulted, QueueBind, QueueDeclare, QueueDeclarePassive, QueuePurge
      SendSettings (interface):  GetBrokerTopology, GetSendAddress
      SequentialEndpointResolver (class):  All, SequentialEndpointResolver
      SharedChannelContext (class):  BasicAck, BasicCancel, BasicConsume, BasicNack, BasicPublishAsync, BasicQos, ExchangeBind, ExchangeDeclare, ExchangeDeclarePassive, NotifyFaulted, QueueBind, QueueDeclare, QueueDeclarePassive, QueuePurge, SharedChannelContext
      SharedChannelContextFactory (class):  CreateActiveContext, CreateChannel, CreateContext, CreateSharedChannel, SharedChannelContextFactory
      SharedConnectionContext (class):  CreateChannel, CreateChannelContext, SharedConnectionContext
   MassTransit.RabbitMqTransport.Configuration
      BatchSettings (interface)
      ClusterNode (struct):  ClusterNode, Parse, ToString
      ConfigurationBatchSettings (class):  ConfigurationBatchSettings
      ConfigurationHostSettings (class):  ConfigurationHostSettings, ConfigureBatch, FormatHostAddress, Refresh
      ExchangeBindingConsumeTopologySpecification (class):  Apply, Bind, ExchangeBindingConsumeTopologySpecification, Validate
         Used to bind an exchange to the consuming queue's exchange
      ExchangeBindingPublishTopologySpecification (class):  Apply, ExchangeBindingPublishTopologySpecification, Validate
         Used to bind an exchange to the sending
      ExchangeToExchangeBindingConsumeTopologySpecification (class):  Apply, Bind, ExchangeToExchangeBindingConsumeTopologySpecification, Validate
         Used to bind an exchange to the consuming queue's exchange
      ExchangeToQueueBindingConsumeTopologySpecification (class):  Apply, ExchangeToQueueBindingConsumeTopologySpecification, Validate
         Used to bind an exchange to the consuming queue's exchange
      ExchangeToQueueBindingPublishTopologySpecification (class):  Apply, ExchangeToQueueBindingPublishTopologySpecification, Validate
         Used to declare an exchange and queue, and bind them together.
      IRabbitMqBusConfiguration (interface):  CreateEndpointConfiguration
      IRabbitMqConsumeTopologySpecification (interface):  Apply
      IRabbitMqEndpointConfiguration (interface)
      IRabbitMqHostConfiguration (interface):  ApplyEndpointDefinition, CreateReceiveEndpointConfiguration
      IRabbitMqPublishTopologySpecification (interface):  Apply
      IRabbitMqReceiveEndpointConfiguration (interface):  Build
      IRabbitMqTopologyConfiguration (interface)
      InvalidRabbitMqConsumeTopologySpecification (class):  Apply, InvalidRabbitMqConsumeTopologySpecification, Validate
      QueueBindingConfigurator (class):  SetBindingArgument
      RabbitMqBatchPublishConfigurator (class):  RabbitMqBatchPublishConfigurator
      RabbitMqBusConfiguration (class):  ConnectBusObserver, ConnectEndpointConfigurationObserver, RabbitMqBusConfiguration
      RabbitMqBusFactoryConfigurator (class):  CreateBusEndpointConfiguration, EnablePriority, Host, OverrideDefaultBusEndpointQueueName, Publish, RabbitMqBusFactoryConfigurator, ReceiveEndpoint, Send, SetExchangeArgument, SetQueueArgument, SetQuorumQueue, Validate
      RabbitMqClusterConfigurator (class):  GetEndpointResolver, Node, RabbitMqClusterConfigurator
      RabbitMqEndpointConfiguration (class):  CreateEndpointConfiguration, RabbitMqEndpointConfiguration
      RabbitMqExchangeBindingConfigurator (class):  SetBindingArgument
      RabbitMqExchangeConfigurator (class):  GetEndpointAddress, RabbitMqExchangeConfigurator, SetExchangeArgument
      RabbitMqHostConfiguration (class):  ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, RabbitMqHostConfiguration, ReceiveEndpoint, Validate
      RabbitMqHostConfigurator (class):  ConnectionName, ContinuationTimeout, GetVirtualHost, Heartbeat, MaxMessageSize, Password, RabbitMqHostConfigurator, RequestedChannelMax, RequestedConnectionTimeout, RequestedFrameMax, UseCluster, UseSsl, Username
      RabbitMqQueueConfigurator (class):  EnablePriority, GetEndpointAddress, SetQueueArgument, SetQuorumQueue
      RabbitMqReceiveEndpointBuilder (class):  BuildTopology, ConnectConsumePipe, CreateDeadLetterTransport, CreateErrorTransport, CreateReceiveEndpointContext, RabbitMqReceiveEndpointBuilder
      RabbitMqReceiveEndpointConfiguration (class):  Bind, BindDeadLetterQueue, Build, ConfigureChannel, ConfigureConnection, CreateRabbitMqReceiveEndpointContext, CreateReceiveEndpointContext, EnablePriority, FormatInputAddress, OverrideConsumerTag, RabbitMqReceiveEndpointConfiguration, SetDeliveryAcknowledgementTimeout, SetExchangeArgument, SetQueueArgument, SetQuorumQueue
      RabbitMqReceiveSettings (class):  GetInputAddress, RabbitMqReceiveSettings
      RabbitMqRegistrationBusFactory (class):  CreateBus, RabbitMqRegistrationBusFactory
      RabbitMqSslConfigurator (class):  AllowPolicyErrors, EnforcePolicyErrors, RabbitMqSslConfigurator
      RabbitMqStreamConfigurator (class):  FromFirst, FromLast, FromOffset, FromTimestamp, RabbitMqStreamConfigurator
      RabbitMqTopologyConfiguration (class):  RabbitMqTopologyConfiguration, Validate
   MassTransit.RabbitMqTransport.Contracts
      PrefetchCountUpdated (interface)
         Published/Returned when the prefetch count of a receive endpoint is updated
   MassTransit.RabbitMqTransport.Middleware
      ConfigureRabbitMqTopologyFilter (class):  Bind, Configure, ConfigureRabbitMqTopologyFilter, ConfigureTopology, Declare, Probe, Send
         Configures the broker with the supplied topology once the channel is created, to ensure that the exchanges, queues, a...
      ConfigureTopologyContext (interface)
      PrefetchCountFilter (class):  PrefetchCountFilter, Probe, Send, SetPrefetchCount
         Prepares a queue for receiving messages using the ReceiveSettings specified.
      PurgeOnStartupFilter (class):  Probe, PurgeIfRequested, PurgeOnStartupFilter, Send
         Purges the queue on startup, only once per filter instance
      RabbitMqConsumerFilter (class):  Probe, RabbitMqConsumerFilter, Send
         A filter that uses the channel context to create a basic consumer and connect it to the channel
   MassTransit.RabbitMqTransport.Topology
      BrokerTopology (interface)
      BrokerTopologyBuilder (class):  BuildBrokerTopology, ExchangeBind, ExchangeDeclare, GetNextId, QueueBind, QueueDeclare
      Cached (class)
      Exchange (interface)
         The exchange details used to declare the exchange to RabbitMQ
      ExchangeBindingEntity (class):  ExchangeBindingEntity, ToString
      ExchangeBindingEntityEqualityComparer (class):  Equals, GetHashCode
      ExchangeBindingHandle (interface)
      ExchangeEntity (class):  ExchangeEntity, ToString
      ExchangeEntityEqualityComparer (class):  Equals, GetHashCode
      ExchangeHandle (interface)
      ExchangeToExchangeBinding (interface)
         The exchange to exchange binding details to declare the binding to RabbitMQ
      ExchangeToQueueBinding (interface)
         The exchange to queue binding details to declare the binding to RabbitMQ
      FanoutExchangeTypeSelector (class):  GetExchangeType
      IBrokerTopologyBuilder (interface):  ExchangeBind, ExchangeDeclare, QueueBind, QueueDeclare
      IExchangeTypeSelector (interface):  GetExchangeType
         During a topology build, this will determine the exchange type for a message, given the exchange name (entity name) a...
      IMessageExchangeTypeSelector (interface):  GetExchangeType
         Used to select the exchange type for a published message
      IPublishEndpointBrokerTopologyBuilder (interface):  CreateImplementedBuilder
         A builder for creating the topology when publishing a message
      IReceiveEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so that the items added by it can be combined toge...
      ImplementedBuilder (class):  CreateImplementedBuilder, ExchangeBind, ExchangeDeclare, ImplementedBuilder, QueueBind, QueueDeclare
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector, ImplementsMessageType
      ImplementedTypeAdapter (class):  Apply, ImplementedTypeAdapter
      NameEqualityComparer (class):  Equals, GetHashCode
      PublishEndpointBrokerTopologyBuilder (class):  CreateImplementedBuilder, PublishEndpointBrokerTopologyBuilder
      Queue (interface)
         The queue details used to declare the queue to RabbitMQ
      QueueBindingEntity (class):  QueueBindingEntity, ToString
      QueueBindingEntityEqualityComparer (class):  Equals, GetHashCode
      QueueBindingHandle (interface)
      QueueEntity (class):  QueueEntity, ToString
      QueueEntityEqualityComparer (class):  Equals, GetHashCode
      QueueHandle (interface)
      RabbitMqBrokerTopology (class):  Probe, RabbitMqBrokerTopology
      RabbitMqBusTopology (class):  GetDestinationAddress, Publish, RabbitMqBusTopology, Send
      RabbitMqConsumeTopology (class):  AddSpecification, Apply, Bind, BindQueue, GetMessageTopology, RabbitMqConsumeTopology, Validate
      RabbitMqDeadLetterSettings (class):  GetBrokerTopology, RabbitMqDeadLetterSettings
      RabbitMqDelaySettings (class):  RabbitMqDelaySettings
      RabbitMqEntityNameValidator (class):  IsValidEntityName, ThrowIfInvalidEntityName
      RabbitMqErrorSettings (class):  GetBrokerTopology, RabbitMqErrorSettings
      RabbitMqMessageConsumeTopology (class):  Apply, Bind, RabbitMqMessageConsumeTopology, Validate
      RabbitMqMessagePublishTopology (class):  AddImplementedMessageConfigurator, Apply, ApplyBrokerTopology, BindAlternateExchangeQueue, BindQueue, GetBrokerTopology, GetSendSettings, RabbitMqMessagePublishTopology, SetExchangeArgument, TryGetPublishAddress
      RabbitMqMessageSendTopology (class)
      RabbitMqPublishTopology (class):  GetMessageTopology, GetPublishBrokerTopology, RabbitMqPublishTopology
      RabbitMqSendSettings (class):  BindToExchange, BindToQueue, GetBrokerTopology, GetSendAddress, GetSettingStrings, RabbitMqSendSettings, SetQueueArgument, ToString
      RabbitMqSendTopology (class):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings, RabbitMqSendTopology
      ReceiveEndpointBrokerTopologyBuilder (class)
      TopologyLayoutExtensions (class):  LogResult
   MassTransit.RedisIntegration.Saga
      DatabaseContext (interface):  Add, Delete, Insert, Load, Update
      RedisDatabaseContext (class):  Add, Delete, DisposeAsync, Get, Insert, Load, Lock, Put, RedisDatabaseContext, Update
      RedisSagaRepository (class):  Create, SelectDefaultDatabase
      RedisSagaRepositoryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, DisposeAsync, Insert, Load, RedisSagaRepositoryContext, Save, Undo, Update
      RedisSagaRepositoryContextFactory (class):  Execute, Probe, RedisSagaRepositoryContextFactory, Send, SendQuery
      SagaLock (class):  DisposeAsync, Lock, SagaLock
   MassTransit.RetryPolicies
      BaseRetryContext (class):  PreRetry, RetryFaulted
      BaseRetryPolicyContext (class):  CanRetry, Cancel, CreateCancellationToken, Dispose, RetryFaulted
      ConsumeContextRetryContext (class):  CanRetry, ConsumeContextRetryContext, PreRetry, RetryFaulted
      ConsumeContextRetryPolicy (class):  ConsumeContextRetryPolicy, CreatePolicyContext, IsHandled, Probe
      ConsumeContextRetryPolicyContext (class):  CanRetry, Cancel, ConsumeContextRetryPolicyContext, Dispose, RetryFaulted
      ExponentialRetryContext (class):  CanRetry, ExponentialRetryContext
      ExponentialRetryPolicy (class):  CalculateIntervals, CreatePolicyContext, ExponentialRetryPolicy, GetRetryInterval, IsHandled, Probe, ToString
      ExponentialRetryPolicyContext (class):  ExponentialRetryPolicyContext
      ImmediateRetryContext (class):  CanRetry, ImmediateRetryContext
      ImmediateRetryPolicy (class):  CreatePolicyContext, ImmediateRetryPolicy, IsHandled, Probe
      ImmediateRetryPolicyContext (class):  ImmediateRetryPolicyContext
      IncrementalRetryContext (class):  CanRetry, IncrementalRetryContext
      IncrementalRetryPolicy (class):  CreatePolicyContext, IncrementalRetryPolicy, IsHandled, Probe
      IncrementalRetryPolicyContext (class):  IncrementalRetryPolicyContext
      InlinePipeContext (class):  InlinePipeContext
      IntervalRetryContext (class):  CanRetry, IntervalRetryContext
      IntervalRetryPolicy (class):  CreatePolicyContext, IntervalRetryPolicy, IsHandled, Probe, ToString
      IntervalRetryPolicyContext (class):  IntervalRetryPolicyContext
      MessageRetryPolicyExtensions (class):  Attempt, Retry
      NoRetryContext (class):  CanRetry, NoRetryContext
      NoRetryPolicy (class):  CreatePolicyContext, IsHandled, NoRetryPolicy, Probe, ToString
      NoRetryPolicyContext (class):  CanRetry, NoRetryPolicyContext
      PipeRetryExtensions (class):  Attempt, Retry
      RedeliveryRetryConsumeContext (class):  CreateNext, RedeliveryRetryConsumeContext
      RetryConsumeContext (class):  CreateNext, NotifyConsumed, NotifyFaulted, NotifyPendingFaults, RetryConsumeContext
      RetryConsumerConsumeContext (class):  CreateNext, RetryConsumerConsumeContext
      RetrySagaConsumeContext (class):  CreateNext, RetrySagaConsumeContext, SetCompleted
   MassTransit.RetryPolicies.ExceptionFilters
      AllExceptionFilter (class):  Match, Probe
      FilterExceptionFilter (class):  FilterExceptionFilter, Match, Probe
      HandleExceptionFilter (class):  HandleExceptionFilter, Match, Probe
      IgnoreExceptionFilter (class):  IgnoreExceptionFilter, Match, Probe
   MassTransit.Saga
      AnyExistingSagaPolicy (class):  AnyExistingSagaPolicy, Existing, Missing, PreInsertInstance
         Sends the message to any existing saga instances, failing silently if no saga instances are found.
      DefaultSagaRepositoryQueryContext (class):  Add, CreateSagaConsumeContext, DefaultSagaRepositoryQueryContext, Delete, Discard, GetEnumerator, Insert, Load, Query, Save, Undo, Update
      ExpressionSagaQueryFactory (class):  ExpressionSagaQueryFactory, Probe, TryCreateQuery
         Creates a saga query using the specified filter expression
      FactoryMethodSagaFactory (class):  Create, FactoryMethodSagaFactory, Send
         Creates a saga instance using the default factory method
      IIndexedSagaProperty (interface):  Add, Remove, Select, Where
         For the in-memory saga repository, this maintains an index of saga properties for fast searching
      ILoadSagaRepositoryContextFactory (interface):  Execute
      IPropertyExpressionPropertyValue (interface):  GetValue
      IQuerySagaRepositoryContextFactory (interface):  Execute
      ISagaConsumeContextFactory (interface):  CreateSagaConsumeContext
         Creates the as needed by the .
      ISagaRepositoryContextFactory (interface):  Send, SendQuery
      InMemorySagaConsumeContext (class):  Dispose, InMemorySagaConsumeContext
      InMemorySagaConsumeContextFactory (class):  CreateSagaConsumeContext
      InMemorySagaRepositoryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, Dispose, InMemorySagaRepositoryContext, Insert, Load, Query, Save, Undo, Update
      InMemorySagaRepositoryContextFactory (class):  Execute, ExecuteAsyncMethod, InMemorySagaRepositoryContextFactory, Probe, Send, SendQuery
         Supports the InMemorySagaRepository
      IndexedSagaDictionary (class):  Add, BuildIndices, GetRightValue, HasIndexFor, IndexedSagaDictionary, MarkInUse, Release, Remove, Select, Where
      IndexedSagaProperty (class):  Add, GetGetMethod, IndexedSagaProperty, Remove, Select, Where
         A dictionary index of the sagas
      LoadSagaRepository (class):  Load, LoadSagaRepository, Probe
         The modern query saga repository, which can be used with any storage engine.
      LoadSagaRepositoryContext (interface):  Load
      LoadedSagaRepositoryQueryContext (class):  Add, CreateSagaConsumeContext, Delete, Discard, GetEnumerator, Insert, Load, LoadedSagaRepositoryQueryContext, Query, Save, Undo, Update
         For queries that load the actual saga instances
      NewOrExistingSagaPolicy (class):  Existing, Missing, NewOrExistingSagaPolicy, PreInsertInstance
         Creates a new or uses an existing saga instance
      NewSagaPolicy (class):  Existing, Missing, NewSagaPolicy, PreInsertInstance
         Accepts a message to a saga that does not already exist, throwing an exception if an existing saga instance is specif...
      NotImplementedSagaRepositoryContextFactory (class):  Execute, NotImplementedSagaRepositoryContextFactory, Probe
      PropertyExpressionPropertyValue (class):  GetValue
      PropertyExpressionSagaQueryFactory (class):  CreateExpression, Probe, PropertyExpressionSagaQueryFactory, TryCreateQuery
      QuerySagaRepository (class):  Find, Probe, QuerySagaRepository
         The modern query saga repository, which can be used with any storage engine.
      QuerySagaRepositoryContext (interface):  Query
      SagaConsumeContextFactory (class):  CreateSagaConsumeContext
      SagaFilterExpressionConverter (class):  Convert, EvaluateMemberAccess, RemoveMessageParameter, SagaFilterExpressionConverter
      SagaInstance (class):  Equals, GetHashCode, MarkInUse, Release, Remove, SagaInstance
      SagaQuery (class):  GetFilter, SagaQuery
      SagaRepository (class):  Find, Load, Probe, SagaRepository, Send, SendQuery
         The modern saga repository, which can be used with any storage engine.
      SagaRepositoryContext (interface):  Add, Delete, Discard, Insert, Load, Save, Undo, Update
      SagaRepositoryQueryContext (interface)
      StateExpressionVisitor (class):  Combine, StateExpressionVisitor
   MassTransit.SagaStateMachine
      ActionActivity (class):  Accept, ActionActivity, Execute, Faulted, Probe
      ActivityBehavior (class):  Accept, ActivityBehavior, Execute, Faulted, Probe
      ActivityBehaviorBuilder (class):  ActivityBehaviorBuilder, Add, CreateBehavior
      AllStateEventFilter (class):  Filter
      AsyncActivity (class):  Accept, AsyncActivity, Execute, Faulted, Probe
      AsyncFactoryActivity (class):  Accept, AsyncFactoryActivity, Execute, Faulted, Probe
      AsyncFaultedActionActivity (class):  Accept, AsyncFaultedActionActivity, Execute, Faulted, Probe
      Behavior (class):  Empty, Faulted
         A behavior is invoked by a state when an event is raised on the instance and embodies the activities that are execute...
      Cached (class)
      CachedConfigurator (interface):  Faulted
      CancelRequestTimeoutActivity (class):  Accept, CancelRequestTimeoutActivity, Execute, Faulted, Probe
      CatchActivityBinder (class):  Bind, CatchActivityBinder, IsStateTransitionEvent
         Creates a compensation activity with the compensation behavior
      CatchBehaviorBuilder (class):  Add, CatchBehaviorBuilder, CreateBehavior
      CatchExceptionActivityBinder (class):  Add, Catch, CatchExceptionActivityBinder, GetBinder, GetStateActivityBinders, If, IfAsync, IfElse, IfElseAsync
      CatchFaultActivity (class):  Accept, CatchFaultActivity, Execute, Faulted, Probe
         Catches an exception of a specific type and compensates using the behavior
      CompleteRequestActivity (class):  Accept, Execute, Faulted, Probe
      CompletedEvent (class)
      CompositeEventActivity (class):  Accept, CompositeEventActivity, Execute, Faulted, Probe, RaiseCompositeEvent
      ConditionActivity (class):  Accept, ConditionActivity, Execute, Faulted, Probe
      ConditionExceptionActivity (class):  Accept, ConditionExceptionActivity, Execute, Faulted, Probe
      ConditionalActivityBinder (class):  Bind, ConditionalActivityBinder, GetBehavior, IsStateTransitionEvent
      ConditionalExceptionActivityBinder (class):  Bind, ConditionalExceptionActivityBinder, GetBehavior, IsStateTransitionEvent
      ContainerFactoryActivity (class):  Accept, Execute, Faulted, Probe
      ContextMessageFactory (class):  ContextMessageFactory, GetMessage, Use
      DataBehavior (class):  Accept, DataBehavior, Execute, Faulted, Probe
         Splits apart the data from the behavior so it can be invoked properly.
      DataConverterActivity (class):  Accept, DataConverterActivity, Execute, Faulted, Probe
      DataEventActivityBinder (class):  Add, Catch, CreateConditionalActivityBinder, CreateStateActivityBinder, DataEventActivityBinder, GetBinder, GetStateActivityBinders, If, IfAsync, IfElse, IfElseAsync, Retry
      Edge (class):  Edge, Equals, GetHashCode, ToString
      EmptyBehavior (class):  Accept, Execute, Faulted, Probe
      EventCorrelationExpressionConverter (class):  Convert, EvaluateConsumeContextAccess, EventCorrelationExpressionConverter, RemoveMessageParameter
      ExceptionTypeCache (class):  Faulted, GetOrAdd
      ExecuteActivityBinder (class):  Bind, ExecuteActivityBinder, IsStateTransitionEvent
         Routes event activities to an activities
      ExecuteOnFaultedActivity (class):  Accept, Execute, ExecuteOnFaultedActivity, Faulted, Probe
      ExecuteOnFaultedBehavior (class):  Accept, Execute, ExecuteOnFaultedBehavior, Faulted, Probe
      ExpressionCorrelationSagaQueryFactory (class):  ExpressionCorrelationSagaQueryFactory, Probe, TryCreateQuery
      FactoryActivity (class):  Accept, Execute, FactoryActivity, Faulted, Probe
      FaultRequestActivity (class):  Accept, Execute, Faulted, Probe
      FaultedActionActivity (class):  Accept, Execute, Faulted, FaultedActionActivity, Probe
      FaultedBehavior (class):  Accept, Execute, Faulted, Probe
      FaultedContainerFactoryActivity (class):  Accept, Execute, Faulted, Probe
      FaultedEvent (class)
      FaultedPublishActivity (class):  Accept, Execute, Faulted, FaultedPublishActivity, Probe
      FaultedRequestActivity (class):  Accept, Execute, Faulted, FaultedRequestActivity
      FaultedRespondActivity (class):  Accept, Execute, Faulted, FaultedRespondActivity, Probe
      FaultedScheduleActivity (class):  Accept, Execute, Faulted, FaultedScheduleActivity, Probe, Schedule
      FaultedSendActivity (class):  Accept, Execute, Faulted, FaultedSendActivity, Probe, Send
      FaultedUnscheduleActivity (class):  Accept, Execute, Faulted, FaultedUnscheduleActivity, Probe
      GraphStateMachineExtensions (class):  GetGraph
      GraphStateMachineVisitor (class):  AddCurrentEdge, CreateEventVertex, CreateStateVertex, GetEventVertex, GetStateVertex, GraphStateMachineVisitor, InspectCompositeEventActivity, InspectTransitionActivity, Visit
      IActivityBinder (interface):  Bind, IsStateTransitionEvent
      IBehaviorBuilder (interface):  Add
      ICompositeEventStatusAccessor (interface):  Get, Set
      IStateEventFilter (interface):  Filter
      IgnoreEventActivityBinder (class):  Bind, IgnoreEventActivityBinder, IsStateTransitionEvent
      IntCompositeEventStatusAccessor (class):  Get, IntCompositeEventStatusAccessor, Probe, Set
      LastBehavior (class):  Accept, Execute, Faulted, LastBehavior, Probe
         The last behavior either completes the last activity in the behavior or throws the exception if a compensation is in ...
      LastCatchBehavior (class):  Accept, Execute, Faulted, LastCatchBehavior, Probe
      MessageEvent (class):  Accept, Equals, GetHashCode, MessageEvent, Probe, ToString
      MessageEventCorrelation (class):  GetSagaPolicy, IncludesInitial, MessageEventCorrelation, Validate
      MessageFactory (class):  Create
      PublishActivity (class):  Accept, Execute, Faulted, Probe, PublishActivity
      RequestActivity (class):  Accept, Execute, Faulted, RequestActivity
      RequestActivityImpl (class):  Probe
      RequestCompletedActivity (class):  Accept, Execute, Faulted, Probe, RequestCompletedActivity
         Publishes the event, used by the request state machine to track pending requests for a saga instance.
      RequestFaultedActivity (class):  Accept, Execute, Faulted, Probe
         Publishes the event, used by the request state machine to track pending requests for a saga instance.
      RequestStartedActivity (class):  Accept, Execute, Faulted, Probe
         Publishes the event, used by the request state machine to track pending requests for a saga instance.
      RequestStateMessagePipe (class):  Probe, RequestStateMessagePipe, Send
      RespondActivity (class):  Accept, Execute, Faulted, Probe, RespondActivity
      RetryActivity (class):  Accept, Execute, Faulted, Probe, RetryActivity
      RetryActivityBinder (class):  Bind, IsStateTransitionEvent, RetryActivityBinder
      ScheduleActivity (class):  Accept, Execute, Faulted, Probe, ScheduleActivity
      SelectedStateEventFilter (class):  Filter, SelectedStateEventFilter
      SendActivity (class):  Accept, Execute, Faulted, Probe, SendActivity
      SendRequestPipe (class):  Probe, Send, SendRequestPipe
         Handles the sending of a request to the endpoint specified
      SlimActivity (class):  Accept, Execute, Faulted, Probe, SlimActivity
         Adapts an Activity to a Data Activity context
      StateMachineActivitySelector (class):  OfInstanceType, OfType, StateMachineActivitySelector
      StateMachineFaultedActivitySelector (class):  OfInstanceType, OfType, StateMachineFaultedActivitySelector
      StateMachineGraph (class):  StateMachineGraph
      StructCompositeEventStatusAccessor (class):  Get, Probe, Set, StructCompositeEventStatusAccessor
      TaskMessageFactory (class):  GetMessage, TaskMessageFactory, Use
      TimeoutExpired (class):  TimeoutExpired
      TransitionActivity (class):  Accept, Execute, Faulted, Probe, RaiseAfterLeaveEvents, RaiseBeforeEnterEvents, RaiseCurrentStateLeaveEvents, Transition, TransitionActivity
      TriggerEvent (class):  Accept, CompareTo, Equals, GetHashCode, Probe, ToString, TriggerEvent
      TriggerEventActivityBinder (class):  Add, Catch, CreateConditionalActivityBinder, GetBinder, GetStateActivityBinders, If, IfAsync, IfElse, IfElseAsync, Retry, TriggerEventActivityBinder
      UnscheduleActivity (class):  Accept, Execute, Faulted, Probe, UnscheduleActivity
      Vertex (class):  Equals, GetHashCode, ToString, Vertex
   MassTransit.Scheduling
      BaseScheduleMessageProvider (class):  CancelScheduledSend, ScheduleSend
      Cached (class)
      CancelScheduledMessage (interface)
      CancelScheduledMessageCommand (class):  CancelScheduledMessageCommand
      CancelScheduledRecurringMessage (interface)
      CancelScheduledRecurringMessageCommand (class):  CancelScheduledRecurringMessageCommand
      ClientContextPipe (class):  ClientContextPipe, Probe, Send
      DefaultRecurringSchedule (class)
      DelayedScheduleMessageProvider (class):  CancelScheduledSend, DelayedScheduleMessageProvider, ScheduleSend
      EndpointRecurringMessageScheduler (class):  CancelScheduledRecurringSend, CreateCommand, EndpointRecurringMessageScheduler, GetPublishAddress, PauseScheduledRecurringSend, ResumeScheduledRecurringSend, Schedule, ScheduleRecurringPublish, ScheduleRecurringSend
      EndpointScheduleMessageProvider (class):  EndpointScheduleMessageProvider
      IMessageSchedulerConverter (interface):  ScheduleRecurringSend, ScheduleSend
         Calls the generic version of the ISendEndpoint.Send method with the object's type
      IScheduleTokenIdCache (interface):  TryGetTokenId
      MessageScheduler (class):  CancelScheduledPublish, CancelScheduledSend, GetPublishAddress, MessageScheduler, SchedulePublish, ScheduleSend
      MessageSchedulerConverter (class):  ScheduleRecurringSend, ScheduleSend
         Converts the object type message to the appropriate generic type and invokes the send method with that generic overload.
      MessageSchedulerConverterCache (class):  CreateConverter, CreateTypeConverter, ScheduleRecurringSend, ScheduleSend
         Caches the converters that allow a raw object to be published using the object's type through the generic Send method.
      PauseScheduledRecurringMessage (interface)
      PauseScheduledRecurringMessageCommand (class):  PauseScheduledRecurringMessageCommand
      PublishRecurringMessageScheduler (class):  CancelScheduledRecurringSend, CreateCommand, GetPublishAddress, PauseScheduledRecurringSend, PublishRecurringMessageScheduler, ResumeScheduledRecurringSend, Schedule, ScheduleRecurringPublish, ScheduleRecurringSend
      PublishScheduleMessageProvider (class):  PublishScheduleMessageProvider
      RecurringSchedule (interface)
      ResumeScheduledRecurringMessage (interface)
      ResumeScheduledRecurringMessageCommand (class):  ResumeScheduledRecurringMessageCommand
      ScheduleMessage (interface)
      ScheduleMessageCommand (class):  ScheduleMessageCommand
      ScheduleMessageContextPipe (class):  Probe, ScheduleMessageContextPipe, Send
         For remote endpoint schedulers, used to invoke the pipe and manage the ScheduledMessageId
      ScheduleRecurringMessage (interface)
      ScheduleRecurringMessageCommand (class):  ScheduleRecurringMessageCommand, ToString
      ScheduleRecurringMessageContextPipe (class):  Probe, ScheduleRecurringMessageContextPipe, Send
      ScheduleSendPipe (class):  ScheduleSendPipe
         For transport-based schedulers, used to invoke the pipe and manage the ScheduledMessageId, as well as set the transpo...
      ScheduleTokenId (class):  UseTokenId
      ScheduleTokenIdCache (class):  GetTokenId, ScheduleTokenIdCache, TryGetTokenId
         A cache of convention-based CorrelationId mappers, used unless overridden by some mystical force
      ScheduledMessageHandle (class):  ScheduledMessageHandle
      ScheduledRecurringMessageHandle (class):  ScheduledRecurringMessageHandle
      ServiceBusScheduleMessageProvider (class):  CancelScheduledSend, ScheduleSend, ServiceBusScheduleMessageProvider
      SqlScheduleMessageProvider (class):  CancelScheduledSend, RetryUsingContext, RetryUsingHostConfiguration, ScheduleSend, SqlScheduleMessageProvider
   MassTransit.Serialization
      AesCryptoStreamProvider (class):  AesCryptoStreamProvider, CreateAes, CreateDecryptor, CreateEncryptor, GetDecryptStream, GetEncryptStream, Probe
      AesCryptoStreamProviderV2 (class):  AesCryptoStreamProviderV2, CreateAes, CreateDecryptor, CreateEncryptor, GenerateIv, GetDecryptStream, GetEncryptStream, Probe
      BaseSerializerContext (class):  DeserializeObject, GetMessageSerializer, IsSupportedMessageType, SerializeObject, ToDictionary, TryGetMessage
      BodyConsumeContext (class):  BodyConsumeContext, HasMessageType, TryGetMessage
      BsonMessageSerializer (class):  BsonMessageSerializer, GetMessageBody
      CamelCaseDictionaryExtensions (class):  TryGetValueCamelCase
      ConstantSecureKeyProvider (class):  ConstantSecureKeyProvider, GetKey, Probe
      CopyBodySerializer (class):  CopyBodySerializer, GetMessageBody
         Copies the body of the receive context to the send context unmodified
      CryptoStreamProviderExtensions (class):  GetDecryptStream, GetEncryptStream, SetEncryptionKeyId
      DeserializeVariableExtensions (class):  TryGetValue
      DictionarySendHeaders (class):  DictionarySendHeaders, Get, GetAll, GetEnumerator, Set, TryGetHeader
      DisposingCryptoStream (class)
      EmptyHeaders (class):  EmptyHeaders, Get, GetAll, GetEnumerator, TryGetHeader
      EncryptMessageBody (class):  EncryptMessageBody, GetBytes, GetStream, GetString
      EncryptMessageBodyV2 (class):  EncryptMessageBodyV2, GetBytes, GetStream, GetString
      EncryptedFallbackMessageDeserializerV2 (class):  Deserialize, EncryptedFallbackMessageDeserializerV2, GetMessageBody, Probe, ShouldRetry
      EncryptedFallbackSerializerFactoryV2 (class):  CreateDeserializer, CreateSerializer, EncryptedFallbackSerializerFactoryV2
      EncryptedMessageDeserializer (class):  Deserialize, EncryptedMessageDeserializer, GetMessageBody, Probe
      EncryptedMessageDeserializerV2 (class):  Deserialize, EncryptedMessageDeserializerV2, GetMessageBody, Probe
      EncryptedMessageSerializer (class):  EncryptedMessageSerializer, GetMessageBody
      EncryptedMessageSerializerV2 (class):  EncryptedMessageSerializerV2, GetMessageBody
      EnvelopeMessageContext (class):  ConvertIdToGuid, ConvertToUri, EnvelopeMessageContext, GetHeaders
      ForwardMessagePipe (class):  ForwardMessagePipe, Probe, Send
      ICryptoStreamProvider (interface):  GetDecryptStream, GetEncryptStream
         Provides a crypto stream for the purpose of encrypting or decrypting
      ICryptoStreamProviderV2 (interface):  GetDecryptStream, GetEncryptStream
      ISecureKeyProvider (interface):  GetKey
      ISymmetricKeyProvider (interface):  TryGetKey
         Returns the symmetric key used to encrypt or decrypt messages
      InternalMessagePackResolver (class)
      JobPropertyCollection (class):  ContainsKey, Get, GetEnumerator, Set, SetMany, TryGet, TryGetValue
      JsonEnvelopeHeaders (class):  Deserialize, Get, GetAll, GetEnumerator, JsonEnvelopeHeaders, TryGetHeader
         The headers stored in the message envelope
      JsonExtensions (class):  Merge, MergeInto
      JsonMessageContext (class)
         While not used by MassTransit, this helper class can be used to deserialize a JSON message.
      JsonMessageContextHeaderConverter (class):  CanConvert, ReadJson, WriteJson
      JsonMessageEnvelope (class):  JsonMessageEnvelope, Update
      MassTransitMessagePackFormatterResolver (class):  GetFormatter, MassTransitMessagePackFormatterResolver, TryGetMappedType, TryGetNonGenericMappedType, TryGetOpenGenericMappedType
      MemoryMessageBody (class):  GetBytes, GetStream, GetString, MemoryMessageBody
      MessageEnvelope (interface)
      MessageIdMessageHeader (class):  Get, GetAll, GetEnumerator, MessageIdMessageHeader, TryGetHeader
      MessagePackEnvelope (class):  MessagePackEnvelope
      MessagePackMessageBody (class):  GetBytes, GetStream, GetString, MessagePackMessageBody
      MessagePackMessageBodySerializer (class):  GetMessageBody, MessagePackMessageBodySerializer, OverrideMessage
      MessagePackMessageSerializer (class):  Deserialize, DeserializeMessageBuffer, DeserializeObject, EnsureObjectBufferFormatIsByteArray, GetMessageBody, InternalDeserializeObject, Probe, SerializeObject
      MessagePackMessageSerializerContext (class):  GetMessageSerializer, MessagePackMessageSerializerContext, ToDictionary, TryGetMessage
      MessagePackSerializerFactory (class):  CreateDeserializer, CreateSerializer, MessagePackSerializerFactory
      NServiceBusHeaderAdapter (class):  GetEndpointAddress, GetHostInfo, GetMessageTypes, GetSentTime, NServiceBusHeaderAdapter
      NServiceBusHostInfo (class):  NServiceBusHostInfo
      NServiceBusJsonMessageDeserializer (class):  Deserialize, GetMessageBody, NServiceBusJsonMessageDeserializer, Probe
      NServiceBusJsonMessageSerializer (class):  GetMessageBody
      NServiceBusJsonSerializerFactory (class):  CreateDeserializer, CreateSerializer
      NServiceBusMessageHeaders (class)
      NServiceBusSendContextHeaderExtensions (class):  SetNServiceBusHeaders
      NServiceBusXmlMessageBody (class):  GetBytes, GetStream, GetString, NServiceBusXmlMessageBody
      NServiceBusXmlMessageDeserializer (class):  Deserialize, GetMessageBody, NServiceBusXmlMessageDeserializer, Probe
      NServiceBusXmlMessageSerializer (class):  GetMessageBody, NServiceBusXmlMessageSerializer
      NServiceBusXmlSerializerFactory (class):  CreateDeserializer, CreateSerializer
      NewtonsoftBsonBodyMessageSerializer (class):  GetMessageBody, NewtonsoftBsonBodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftBsonMessageBody (class):  GetBytes, GetStream, GetString, NewtonsoftBsonMessageBody
      NewtonsoftBsonMessageDeserializer (class):  Deserialize, GetMessageBody, NewtonsoftBsonMessageDeserializer, Probe
      NewtonsoftBsonSerializerContext (class):  GetMessageSerializer, NewtonsoftBsonSerializerContext
      NewtonsoftBsonSerializerFactory (class):  CreateDeserializer, CreateSerializer
      NewtonsoftEncryptedBodyMessageSerializer (class):  GetMessageBody, NewtonsoftEncryptedBodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftEncryptedSerializerContext (class):  GetMessageSerializer, NewtonsoftEncryptedSerializerContext
      NewtonsoftEncryptedSerializerFactory (class):  CreateDeserializer, CreateSerializer, NewtonsoftEncryptedSerializerFactory
      NewtonsoftEncryptedV2BodyMessageSerializer (class):  GetMessageBody, NewtonsoftEncryptedV2BodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftEncryptedV2SerializerContext (class):  GetMessageSerializer, NewtonsoftEncryptedV2SerializerContext
      NewtonsoftEncryptedV2SerializerFactory (class):  CreateDeserializer, CreateSerializer, NewtonsoftEncryptedV2SerializerFactory
      NewtonsoftJsonBodyMessageSerializer (class):  GetMessageBody, NewtonsoftJsonBodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftJsonMessageBody (class):  GetBytes, GetStream, GetString, NewtonsoftJsonMessageBody
      NewtonsoftJsonMessageDeserializer (class):  Deserialize, GetMessageBody, NewtonsoftJsonMessageDeserializer, Probe
      NewtonsoftJsonMessageSerializer (class):  GetMessageBody, NewtonsoftJsonMessageSerializer
      NewtonsoftJsonObjectMessageBody (class):  GetBytes, GetStream, GetString, NewtonsoftJsonObjectMessageBody
      NewtonsoftJsonSerializerContext (class):  GetMessageSerializer, NewtonsoftJsonSerializerContext
      NewtonsoftJsonSerializerFactory (class):  CreateDeserializer, CreateSerializer
      NewtonsoftObjectDeserializer (class):  DeserializeObject, NewtonsoftObjectDeserializer, SerializeObject
      NewtonsoftRawBsonBodyMessageSerializer (class):  GetMessageBody, NewtonsoftRawBsonBodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftRawBsonMessageBody (class):  GetBytes, GetStream, GetString, NewtonsoftRawBsonMessageBody
      NewtonsoftRawBsonSerializerContext (class):  GetMessageSerializer, NewtonsoftRawBsonSerializerContext
      NewtonsoftRawJsonBodyMessageSerializer (class):  GetMessageBody, NewtonsoftRawJsonBodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftRawJsonMessageBody (class):  GetBytes, GetStream, GetString, NewtonsoftRawJsonMessageBody
      NewtonsoftRawJsonMessageDeserializer (class):  Deserialize, GetMessageBody, NewtonsoftRawJsonMessageDeserializer, Probe
      NewtonsoftRawJsonMessageSerializer (class):  GetMessageBody, NewtonsoftRawJsonMessageSerializer
      NewtonsoftRawJsonSerializerContext (class):  GetMessageSerializer, IsSupportedMessageType, NewtonsoftRawJsonSerializerContext
      NewtonsoftRawJsonSerializerFactory (class):  CreateDeserializer, CreateSerializer, NewtonsoftRawJsonSerializerFactory
      NewtonsoftRawXmlBodyMessageSerializer (class):  GetMessageBody, NewtonsoftRawXmlBodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftRawXmlMessageBody (class):  GetBytes, GetStream, GetString, NewtonsoftRawXmlMessageBody
      NewtonsoftRawXmlMessageDeserializer (class):  Deserialize, GetMessageBody, NewtonsoftRawXmlMessageDeserializer, Probe
      NewtonsoftRawXmlSerializerContext (class):  GetMessageSerializer, IsSupportedMessageType, NewtonsoftRawXmlSerializerContext
      NewtonsoftRawXmlSerializerFactory (class):  CreateDeserializer, CreateSerializer, NewtonsoftRawXmlSerializerFactory
      NewtonsoftSerializerContext (class):  GetMessageToken, ToDictionary, TryGetMessage
      NewtonsoftXmlBodyMessageSerializer (class):  GetMessageBody, NewtonsoftXmlBodyMessageSerializer, Overlay
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      NewtonsoftXmlJsonMessageSerializer (class):  CreateDeserializer, CreateSerializer, NewtonsoftXmlJsonMessageSerializer
      NewtonsoftXmlMessageBody (class):  GetBytes, GetStream, GetString, NewtonsoftXmlMessageBody
      NewtonsoftXmlMessageDeserializer (class):  Deserialize, GetMessageBody, NewtonsoftXmlMessageDeserializer, Probe
      NewtonsoftXmlMessageSerializer (class):  GetMessageBody, NewtonsoftXmlMessageSerializer, Serialize
      NewtonsoftXmlSerializerContext (class):  GetMessageSerializer, NewtonsoftXmlSerializerContext
      NewtonsoftXmlSerializerFactory (class):  CreateDeserializer, CreateSerializer
      NotSupportedMessageBody (class):  GetBytes, GetStream, GetString
      ObjectDeserializer (class):  Deserialize, Serialize
      RawMessageContext (class):  GetHostInfo, GetSentTime, RawMessageContext
      RawMessageSerializer (class)
      RawXmlMessageSerializer (class):  GetMessageBody, RawXmlMessageSerializer
      ReadOnlyDictionaryHeaders (class):  Get, GetAll, GetEnumerator, ReadOnlyDictionaryHeaders, TryGetHeader
         When a message envelope is deserialized, encapsulate the headers such that objects can be deserialized from the body ...
      Serialization (class):  GetMessageDeserializer, GetMessageSerializer, Probe, Serialization, TryGetMessageDeserializer, TryGetMessageSerializer
      SerializedMessage (interface)
         The content of a serialized message
      SerializedMessageBody (class)
      SymmetricKey (interface)
      SystemTextJsonBodyMessageSerializer (class):  GetMessageBody, Merge, MergeArray, MergeObject, Overlay, SystemTextJsonBodyMessageSerializer, ToNode
         Used to serialize an existing deserialized message when a message is forwarded, scheduled, etc.
      SystemTextJsonExtensions (class):  GetObject, Transform
      SystemTextJsonMessageBody (class):  GetBytes, GetStream, GetString, SystemTextJsonMessageBody
      SystemTextJsonMessageSerializer (class):  Deserialize, DeserializeObject, GetMessageBody, Probe, SerializeObject, SystemTextJsonMessageSerializer
      SystemTextJsonObjectMessageBody (class):  GetBytes, GetStream, GetString, SystemTextJsonObjectMessageBody
      SystemTextJsonRawMessageBody (class):  GetBytes, GetStream, GetString, SystemTextJsonRawMessageBody
      SystemTextJsonRawMessageSerializer (class):  Deserialize, GetMessageBody, Probe, SystemTextJsonRawMessageSerializer
      SystemTextJsonRawSerializerContext (class):  GetMessageSerializer, IsSupportedMessageType, SystemTextJsonRawSerializerContext
      SystemTextJsonSerializerContext (class):  GetJsonElement, GetMessageSerializer, SystemTextJsonSerializerContext, ToDictionary, TryGetMessage
      TransportHeaderFilter (class):  Get, GetAll, GetEnumerator, TransportHeaderFilter, TryGetHeader
   MassTransit.Serialization.JsonConverters
      BaseJsonConverter (class):  CanConvert, CreateMissingConverter, ReadJson
      ByteArrayConverter (class):  CanConvert, GetByteArray, ReadByteArray, ReadJson, WriteJson
      CachedConverter (class):  Deserialize, ResolveContract
      CaseInsensitiveDictionary (class):  CaseInsensitiveDictionary
      CaseInsensitiveDictionaryJsonConverter (class):  CanConvert, Read, Write, WriteJson
      CaseInsensitiveDictionaryStringObjectJsonConverter (class):  Read, ReadArray, ReadObject, ReadPropertyValue, Write, WriteValue
      CustomMessageTypeJsonConverter (class):  CustomMessageTypeJsonConverter, Read, Write
      InterfaceJsonConverter (class):  Read, Write
      InterfaceProxyConverter (class):  WriteJson
      InternalTypeConverter (class):  WriteJson
      JsonContractResolver (class):  ContractDictionaryKeyResolver, JsonContractResolver
      JsonConverterFactoryExtensions (class):  Add
      JsonMessageContextHostInfoConverter (class):  CanConvert, ReadJson, WriteJson
      ListJsonConverter (class):  CanConvert, WriteJson
      MessageDataConverter (class):  Read, Write
      MessageDataObjectConverter (class):  Read, Write
      MessageDataReference (class)
      NewtonsoftMessageDataJsonConverter (class):  WriteJson
      NewtonsoftObjectMessageDataConverter (class):  Convert
      ObjectConverter (class):  Deserialize, ObjectConverter
      StringDecimalConverter (class):  WriteJson
      StringDecimalJsonConverter (class):  Read, Write
      SystemTextJsonConverterFactory (class):  CanConvert, CreateConverter, IsConvertibleInterfaceType, SystemTextJsonConverterFactory
      SystemTextJsonMessageDataConverter (class):  CanConvert, CreateConverter
      SystemTextMessageDataReference (class)
      TypeMappingJsonConverter (class):  Read, Write
      UriDictionarySystemTextJsonConverter (class):  Read, Write
   MassTransit.Serialization.MessagePackFormatters
      FormatterProxyInfo (struct)
      InterfaceConcreteMapFormatter (class):  Deserialize, Serialize
      InterfaceMessagePackFormatter (class):  Deserialize, GetFormatterProxyInfoFromType, InterfaceMessagePackFormatter, Serialize
      MessageDataFormatter (class):  Deserialize, Serialize
   MassTransit.SignalR
      HubLifetimeManagerOptions (class):  HubLifetimeManagerOptions
      IHubLifetimeManagerOptions (interface)
      MassTransitHubLifetimeManager (class):  AddGroupAsyncCore, AddToGroupAsync, MassTransitHubLifetimeManager, OnConnectedAsync, OnDisconnectedAsync, RemoveFromGroupAsync, RemoveGroupAsyncCore, SendAllAsync, SendAllExceptAsync, SendConnectionAsync, SendConnectionsAsync, SendGroupAsync, SendGroupExceptAsync, SendGroupsAsync, SendUserAsync
      MassTransitSignalRConfigurationExtensions (class):  AddSignalRHub, GetMassTransitHubLifetimeManager, RegisterConsumers
   MassTransit.SignalR.Configuration.Definitions
      AllConsumerDefinition (class):  AllConsumerDefinition
      ConnectionConsumerDefinition (class):  ConnectionConsumerDefinition
      GroupConsumerDefinition (class):  GroupConsumerDefinition
      GroupManagementConsumerDefinition (class):  GroupManagementConsumerDefinition
      HubConsumerDefinition (class):  Configure, GetEndpointName
      UserConsumerDefinition (class):  UserConsumerDefinition
   MassTransit.SignalR.Consumers
      AllConsumer (class):  AllConsumer, Consume, Handle
      ConnectionConsumer (class):  ConnectionConsumer, Consume, Handle
      GroupConsumer (class):  Consume, GroupConsumer, Handle
      GroupManagementConsumer (class):  Consume, GroupManagementConsumer
      UserConsumer (class):  Consume, Handle, UserConsumer
   MassTransit.SignalR.Contracts
      Ack (interface)
      All (interface)
      Connection (interface)
      Group (interface)
      GroupManagement (interface)
      User (interface)
   MassTransit.SignalR.Scoping
      DependencyInjectionHubLifetimeScopeProvider (class):  CreateScope, DependencyInjectionHubLifetimeScopeProvider
      HubLifetimeScope (class):  DisposeAsync, HubLifetimeScope
      IHubLifetimeScope (interface)
      IHubLifetimeScopeProvider (interface):  CreateScope
   MassTransit.SignalR.Utils
      ConcurrentHashSet (class):  Add, Clear, ConcurrentHashSet, Contains, Dispose, Remove, ToArray
      IMassTransitFeature (interface)
      MassTransitFeature (class)
      MassTransitSubscriptionManager (class):  AddSubscription, RemoveSubscription
      SerializedHubMessageExtensions (class):  ToProtocolDictionary, ToSerializedHubMessage
   MassTransit.SqlTransport
      ClientContext (interface):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, DeadLetterQueue, DeleteMessage, DeleteScheduledMessage, MoveMessage, Publish, PurgeQueue, ReceiveMessages, RenewLock, Send, TouchQueue, Unlock
      ClientContextSupervisor (class):  ClientContextSupervisor
      ConnectionContext (interface):  CreateClientContext, CreateConnection, DelayUntilMessageReady, Query
      ConnectionContextFactory (class):  CreateActiveContext, CreateContext, CreateSharedConnection
      ConnectionContextSupervisor (class):  ConnectionContextSupervisor, CreatePublishTransport, CreateSendTransport, NormalizeAddress
      DeadLetterSettings (interface):  GetBrokerTopology
      Defaults (class)
      EntitySettings (interface)
      ErrorSettings (interface):  GetBrokerTopology
      HostInfoCache (class)
      IClientContextSupervisor (interface)
      IConnectionContextSupervisor (interface):  CreatePublishTransport, CreateSendTransport, NormalizeAddress
      IQueueNotificationListener (interface):  MessageReady
      ISqlHost (interface)
      ISqlTransportConnection (interface)
      ISqlTransportDatabaseMigrator (interface):  CreateDatabase, CreateInfrastructure, CreateSchemaIfNotExist, DeleteDatabase
      MessageDelivery (class)
      MessageLockContext (interface):  Abandon, Complete, DeadLetter
      NotificationContext (interface):  ConnectNotificationSink
      QueueSendTransportContext (class):  CopyIncomingIdentifiersIfPresent, CreateSendContext, GetAgentHandles, Probe, QueueSendTransportContext, Send
      QueueSqlReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConvertException, Probe, QueueSqlReceiveEndpointContext
      ReceiveSettings (interface)
         Specify the receive settings for a receive transport
      ScopeClientContext (class):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, DeadLetterQueue, DeleteMessage, DeleteScheduledMessage, MoveMessage, Publish, PurgeQueue, ReceiveMessages, RenewLock, ScopeClientContext, Send, TouchQueue
      ScopeClientContextFactory (class):  CreateActiveContext, CreateClientContext, CreateContext, CreateSharedClientContext, ScopeClientContextFactory
      SendSettings (interface):  GetBrokerTopology, GetSendAddress
      SharedClientContext (class):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, DeadLetterQueue, DeleteMessage, DeleteScheduledMessage, MoveMessage, Publish, PurgeQueue, ReceiveMessages, RenewLock, Send, SharedClientContext, TouchQueue
      SharedClientContextFactory (class):  CreateActiveContext, CreateClientContext, CreateContext, CreateScopeContext, SharedClientContextFactory
      SharedConnectionContext (class):  CreateClientContext, CreateConnection, DelayUntilMessageReady, Query, SharedConnectionContext
      SqlClientContext (class):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, DeadLetterQueue, DeleteMessage, DeleteScheduledMessage, MoveMessage, Publish, PurgeQueue, ReceiveMessages, RenewLock, Send, TouchQueue, Unlock
      SqlHeaderProvider (class):  GetAll, SqlHeaderProvider, TryGetHeader
      SqlHost (class):  ConnectReceiveEndpoint, SqlHost
      SqlLogMessages (class)
      SqlMessageSendContext (class):  ReadPropertiesFrom, SqlMessageSendContext, WritePropertiesTo
      SqlPublishTransportProvider (class):  GetPublishTransport, SqlPublishTransportProvider
      SqlQueueDeadLetterTransport (class):  Send, SqlQueueDeadLetterTransport
      SqlQueueErrorTransport (class):  Send, SqlQueueErrorTransport
      SqlQueueMoveTransport (class)
      SqlReceiveContext (class):  GetTransportProperties, SqlReceiveContext
      SqlReceiveEndpointContext (interface)
      SqlReceiveLockContext (class):  Complete, Expired, Faulted, RenewLock, ScheduleRedelivery, SqlReceiveLockContext, ValidateLockStatus
      SqlSendTransportProvider (class):  GetSendTransport, NormalizeAddress, SqlSendTransportProvider
      SqlTransportMessage (class):  DeserializeHeaders, GetHeaders, GetHost, GetTransportHeaders, ToUri
      SqlTransportMigrationHostedService (class):  SqlTransportMigrationHostedService, StartAsync, StopAsync
      SqlTransportPropertyNames (class)
      TopicSendTransportContext (class):  CreateSendContext, GetAgentHandles, Probe, Send, TopicSendTransportContext
   MassTransit.SqlTransport.Configuration
      ConfigurationSqlHostSettings (class):  CreateConnectionContextFactory, FormatHostAddress, GetLicenseInfo, UriDecode, Validate
      ISqlBusConfiguration (interface):  CreateEndpointConfiguration
      ISqlConsumeTopologySpecification (interface):  Apply
      ISqlEndpointConfiguration (interface)
      ISqlHostConfiguration (interface):  ApplyEndpointDefinition, CreateReceiveEndpointConfiguration
      ISqlPublishTopologySpecification (interface):  Apply
      ISqlReceiveEndpointConfiguration (interface):  Build
      ISqlTopologyConfiguration (interface)
      InvalidSqlConsumeTopologySpecification (class):  Apply, InvalidSqlConsumeTopologySpecification, Validate
      QueueSubscriptionConsumeTopologySpecification (class):  Apply, QueueSubscriptionConsumeTopologySpecification, Validate
      SqlBusConfiguration (class):  ConnectBusObserver, ConnectEndpointConfigurationObserver, SqlBusConfiguration
      SqlBusFactoryConfigurator (class):  CreateBusEndpointConfiguration, Host, OverrideDefaultBusEndpointQueueName, Publish, ReceiveEndpoint, Send, SqlBusFactoryConfigurator, Validate
      SqlEndpointConfiguration (class):  CreateEndpointConfiguration, SqlEndpointConfiguration
      SqlHostConfiguration (class):  ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, ReceiveEndpoint, SqlHostConfiguration, Validate
      SqlHostConfigurator (class):  UseLicense, UseLicenseFile
      SqlMessageSchedulerSpecification (class):  Apply, Validate
      SqlQueueConfigurator (class)
      SqlQueueSubscriptionConfigurator (class)
      SqlReceiveEndpointBuilder (class):  BuildTopology, ConnectConsumePipe, CreateDeadLetterTransport, CreateErrorTransport, CreateReceiveEndpointContext, SqlReceiveEndpointBuilder
      SqlReceiveEndpointConfiguration (class):  Build, ConfigureClient, CreateDbReceiveEndpointContext, CreateReceiveEndpointContext, FormatInputAddress, IsValidEntityName, SetReceiveMode, SqlReceiveEndpointConfiguration, Subscribe, Validate
      SqlReceiveSettings (class):  GetInputAddress, SqlReceiveSettings
      SqlRegistrationBusFactory (class):  CreateBus, SqlRegistrationBusFactory
      SqlTopicConfigurator (class):  GetEndpointAddress, SqlTopicConfigurator
      SqlTopicSubscriptionConfigurator (class)
      SqlTopologyConfiguration (class):  SqlTopologyConfiguration, Validate
      TopicSubscriptionPublishTopologySpecification (class):  Apply, TopicSubscriptionPublishTopologySpecification, Validate
         Used to bind an exchange to the sending
   MassTransit.SqlTransport.Middleware
      ConfigureSqlTopologyFilter (class):  ConfigureSqlTopologyFilter, ConfigureTopology, CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, Probe, Send
         Configures the broker with the supplied topology once the model is created, to ensure that the exchanges, queues, and...
      ConfigureTopologyContext (interface)
      OrderedChannelExecutorPool (class):  DisposeAsync, OrderedChannelExecutorPool, PartitionKeyProvider, Push, Run
      PurgeOnStartupFilter (class):  Probe, PurgeIfRequested, PurgeOnStartupFilter, Send
         Purges the queue on startup, only once per filter instance
      SqlConsumerFilter (class):  Probe, Send, SqlConsumerFilter
         A filter that uses the model context to create a basic consumer and connect it to the model
      SqlMessageReceiver (class):  Consume, HandleMessage, MessageHandled, ReceiveMessages, SqlMessageReceiver, WaitForPollingIntervalOrMessageHandled
         Receives messages from AmazonSQS, pushing them to the InboundPipe of the service endpoint.
      SqlMessageSchedulerFilter (class):  Probe, SchedulerFactory, Send
         Adds the service bus message scheduler filter
   MassTransit.SqlTransport.PostgreSql
      EnumParameter (class):  AddParameter, EnumParameter
      IPostgresSqlTransportConnection (interface):  CreateCommand
      JsonParameter (class):  AddParameter, JsonParameter
      MaintenanceAgent (class):  MaintenanceAgent, PerformMaintenance
      NotificationAgent (class):  GetCancellationTokenForQueue, ListenForNotifications, NotificationAgent, OnConnectionOnNotification, RemoveTokenForQueue
      PostgresClientContext (class):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, DeadLetterQueue, DeleteMessage, DeleteScheduledMessage, MoveMessage, PostgresClientContext, Publish, PurgeQueue, ReceiveMessages, RenewLock, Send, TouchQueue
      PostgresConnectionContextFactory (class):  PostgresConnectionContextFactory
      PostgresDatabaseMigrator (class):  CreateDatabase, CreateDatabaseIfNotExist, CreateInfrastructure, CreateSchemaIfNotExist, DeleteDatabase, GrantAccess, PostgresDatabaseMigrator
      PostgresDbConnectionContext (class):  CreateClientContext, CreateConnection, DelayUntilMessageReady, DisposeAsync, PostgresDbConnectionContext, Query
      PostgresSqlHostConfigurator (class):  PostgresSqlHostConfigurator
      PostgresSqlHostSettings (class):  CreateConnectionContextFactory, GetDataSource, ParseHost, PostgresSqlHostSettings
      PostgresSqlTransportConnection (class):  Close, CreateBuilder, CreateCommand, DisposeAsync, GetAdminMigrationPrincipal, GetDatabaseAdminConnection, GetDatabaseConnection, GetSystemDatabaseConnection, Open, PostgresSqlTransportConnection
      SqlStatements (class)
      UriTypeHandler (class):  Parse, SetValue
   MassTransit.SqlTransport.PostgreSql.Helpers
      NotifyChannel (class):  SanitizeSchemaName
   MassTransit.SqlTransport.SqlServer
      ISqlServerSqlTransportConnection (interface)
      MaintenanceAgent (class):  Execute, MaintenanceAgent, PerformMaintenance
      SqlServerClientContext (class):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, DeadLetterQueue, DeleteMessage, DeleteScheduledMessage, Execute, MoveMessage, Publish, PurgeQueue, Query, QuerySingle, ReceiveMessages, RenewLock
      SqlServerConnectionContextFactory (class):  SqlServerConnectionContextFactory
      SqlServerDatabaseMigrator (class):  CreateDatabase, CreateDatabaseIfNotExist, CreateInfrastructure, CreateSchemaIfNotExist, DeleteDatabase, GrantAccess, SqlServerDatabaseMigrator
      SqlServerDbConnectionContext (class):  CreateClientContext, CreateConnection, DelayUntilMessageReady, DisposeAsync, IsTransient, Query, SqlServerDbConnectionContext
      SqlServerSqlHostConfigurator (class):  SqlServerSqlHostConfigurator
      SqlServerSqlHostSettings (class):  CreateConnectionContextFactory, FormatDataSource, GetConnectionString, ParseDataSource, ParseHost, SqlServerSqlHostSettings, TrimHost
      SqlServerSqlTransportConnection (class):  Close, CreateBuilder, DisposeAsync, GetDatabaseAdminConnection, GetDatabaseConnection, GetSystemDatabaseConnection, Open, ParseDataSource, SqlServerSqlTransportConnection
      SqlServerSqlTransportOptionsExtensions (class):  FormatDataSource
      UriTypeHandler (class):  Parse, SetValue
   MassTransit.SqlTransport.Topology
      BrokerTopology (interface)
      BrokerTopologyBuilder (class):  BuildBrokerTopology, CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, GetNextId
      IBrokerTopologyBuilder (interface):  CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription
      IPublishEndpointBrokerTopologyBuilder (interface):  CreateImplementedBuilder
         A builder for creating the topology when publishing a message
      IReceiveEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so that the items added by it can be combined toge...
      ImplementedBuilder (class):  CreateImplementedBuilder, CreateQueue, CreateQueueSubscription, CreateTopic, CreateTopicSubscription, ImplementedBuilder
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector, ImplementsMessageType
      ImplementedTypeAdapter (class):  Apply, ImplementedTypeAdapter
      NameEqualityComparer (class):  Equals, GetHashCode
      PublishEndpointBrokerTopologyBuilder (class):  CreateImplementedBuilder
      Queue (interface)
      QueueEntity (class):  QueueEntity, ToString
      QueueEntityEqualityComparer (class):  Equals, GetHashCode
      QueueHandle (interface)
      QueueSendSettings (class):  GetBrokerTopology, GetSendAddress, QueueSendSettings
      QueueSubscriptionEntity (class):  QueueSubscriptionEntity, ToString
      QueueSubscriptionEntityEqualityComparer (class):  Equals, GetHashCode
      QueueSubscriptionHandle (interface)
      ReceiveEndpointBrokerTopologyBuilder (class):  ReceiveEndpointBrokerTopologyBuilder
      SqlBrokerTopology (class):  Probe, SqlBrokerTopology
      SqlBusTopology (class):  Publish, Send, SqlBusTopology
      SqlConsumeTopology (class):  AddSpecification, Apply, GetMessageTopology, SqlConsumeTopology, Subscribe, Validate
      SqlMessageConsumeTopology (class):  Apply, SqlMessageConsumeTopology, Subscribe, Validate
      SqlMessageNameFormatter (class):  GetMessageName, SqlMessageNameFormatter
      SqlMessagePublishTopology (class):  AddImplementedMessageConfigurator, Apply, GetBrokerTopology, GetSendSettings, SqlMessagePublishTopology, TryGetPublishAddress
      SqlMessageSendTopology (class)
      SqlPublishTopology (class):  GetMessageTopology, GetPublishBrokerTopology, SqlPublishTopology
      SqlSendTopology (class):  GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      Topic (interface)
      TopicEntity (class):  ToString, TopicEntity
      TopicHandle (interface)
      TopicSendSettings (class):  GetBrokerTopology, GetSendAddress, TopicSendSettings
      TopicSubscriptionEntity (class):  ToString, TopicSubscriptionEntity
      TopicSubscriptionEntityEqualityComparer (class):  Equals, GetHashCode
      TopicSubscriptionHandle (interface)
      TopicToQueueSubscription (interface)
         The exchange to queue binding details to declare the binding to RabbitMQ
      TopicToTopicSubscription (interface)
         The exchange to exchange binding details to declare the binding to RabbitMQ
      TopologyLayoutExtensions (class):  LogResult
   MassTransit.Testing
      ActiveMqTestHarness (class):  ActiveMqTestHarness, Clean, CleanUpQueue, CleanUpVirtualHost, ConfigureHostSettings, DeleteDestination, GetBrokerEntities, GetDestinationName, GetHostSettings
      ActivityTestHarness (class):  ActivityTestHarness, BuildQueueName, ConfigureBus, GetActivityName
      ActivityTestHarnessExtensions (class):  Activity, ExecuteActivity
      AmazonSqsTestHarness (class):  AmazonSqsTestHarness, CleanUpQueue, CleanUpVirtualHost, ConfigureHostSettings, GetHostSettings
      AsyncElementListExtensions (class):  Any, Count, Deconstruct, First, FirstOrDefault, Select, Take
      AsyncTestHarness (class):  Cancel, Dispose, ForceInactive, GetConsumeObserver, GetTask
      AzureFunctionsTestExtensions (class):  AddAzureFunctionsTestComponents, HandleConsumer
      AzureServiceBusTestHarness (class):  AzureServiceBusTestHarness, Clean, CreateManagementClient
      AzureServiceBusTestHarnessHostedService (class):  AzureServiceBusTestHarnessHostedService, Clean, StartAsync, StopAsync
      BusTestHarness (class):  Clean, CreateRequestClient, GetSendEndpoint, Handled, HandledByConsumer, Handler, Start, Stop, SubscribeHandler
         A bus text fixture includes a single bus instance with one or more receiving endpoints.
      Consumer (class):  Consume, Consumer
      ConsumerConfigurator (class):  Configure, Connect, ConsumerConfigurator
      ConsumerTestHarness (class):  ConsumerTestHarness
      ConsumerTestHarnessExtensions (class):  Consumer
      Conversation (class):  Conversation
      ConversationConsumer (class):  ConversationConsumer
      ConversationThread (class):  ConversationThread
      ExecuteActivityTestHarness (class):  BuildQueueName, ConfigureBus, ExecuteActivityTestHarness, GetActivityName
      ExtensionMethodsForBuses (class):  CreateBusActivityMonitor, CreateBusActivityMonitorInternal
      ExtensionMethodsForSagas (class):  ShouldContainSaga, ShouldNotContainSaga
      FaultOf (class):  Consume, FaultOf
      FilterSet (class):  All, Any, None, NotAny
      HandlerTestHarness (class):  ConfigureReceiveEndpoint, HandleMessage, HandlerTestHarness
      HandlerTestHarnessExtensions (class):  Handler
      IAsyncElementList (interface):  Any, Select, SelectAsync
      IAsyncListElement (interface)
      IBaseTestHarness (interface):  Cancel, ForceInactive
      IConsumedMessage (interface)
      IConsumerConfigurator (interface):  Configure, Connect
      IConsumerTestHarness (interface)
      IPublishedMessage (interface)
      IPublishedMessageList (interface):  Any, Select, SelectAsync
      IReceivedMessage (interface)
      IReceivedMessageList (interface):  Any, Select, SelectAsync
      ISagaInstance (interface)
      ISagaList (interface):  Any, Contains, Select, SelectAsync
      ISagaStateMachineTestHarness (interface):  Exists
      ISagaTestHarness (interface):  Exists, Match, NotExists
      ISentMessage (interface)
      ISentMessageList (interface):  Any, Select, SelectAsync
      IStateMachineSagaTestHarness (interface)
      ITestHarness (interface):  GetConsumerAddress, GetConsumerEndpoint, GetConsumerHarness, GetExecuteActivityAddress, GetExecuteActivityEndpoint, GetHandlerAddress, GetHandlerEndpoint, GetRequestClient, GetSagaAddress, GetSagaEndpoint, GetSagaHarness, GetSagaStateMachineHarness, GetTask, Start
      InMemoryTestHarness (class):  ConnectRequestClient, InMemoryTestHarness
      InterceptPipe (class):  InterceptPipe, Probe, Send
      InterceptPolicy (class):  Existing, InterceptPolicy, Missing, PreInsertInstance
      InterceptPolicyPipe (class):  InterceptPolicyPipe, Probe, Send
      KafkaTestHarnessHostedService (class):  CreateTopics, DeleteTopics, KafkaTestHarnessHostedService, StartAsync, StopAsync
      MediatorTestHarness (class):  CreateMediator, CreateRequestClient, MediatorTestHarness, Start
      MessageEventType (class)
      MultiTestConsumer (class):  Configure, Connect, Consume, Fault, MultiTestConsumer
      Of (class):  Consume, Of
      OutputTimelineOptions (class):  GetTypeName, IncludeAddress, IncludeNamespace, Now
      PublishedMessage (class):  PublishedMessage
      PublishedMessageFilter (class):  Any, None
      PublishedMessageFilterSet (class):  Add
      PublishedMessageList (class):  Add, Any, PublishedMessageList, Select, SelectAsync
      RabbitMqTestHarness (class):  Clean, CleanUpVirtualHost, ConfigureHostSettings, GetHostSettings, GetVirtualHostEntities, RabbitMqTestHarness
      RabbitMqTestHarnessHostedService (class):  CleanVirtualHost, ConfigureVirtualHost, EnsureVirtualHostExists, GetHttpClient, GetUriBuilder, GetVirtualHostEntities, RabbitMqTestHarnessHostedService, StartAsync, StopAsync
      ReceivedMessage (class):  ReceivedMessage
      ReceivedMessageFilter (class):  Any, None
      ReceivedMessageFilterSet (class):  Add
      ReceivedMessageList (class):  Add, Any, ReceivedMessageList, Select, SelectAsync
      SagaTestHarness (class):  SagaTestHarness
      SagaTestHarnessExtensions (class):  Saga
      SentMessage (class):  SentMessage
      SentMessageFilter (class):  Any, None
      SentMessageFilterSet (class):  Add
      SentMessageList (class):  Add, Any, Select, SelectAsync, SentMessageList
      ServiceProviderTestExtensions (class):  GetTask, GetTasks
      SpanInfo (class)
      StateMachineSagaTestHarnessExtensions (class):  ContainsInState, ShouldContainSagaInState, StateMachineSaga
      TelemetryMonitorExtensions (class):  GetTestMethodInfo, Wait
      TestActivityListener (class):  ActivityStarted, ActivityStopped, DisposeAsync, FormatDetailsColumn, GenerateOutput, GetSpan, Sample, TestActivityListener
      TestSagaRepositoryDecorator (class):  Probe, Send, SendQuery, TestSagaRepositoryDecorator
      TestingServiceProviderExtensions (class):  AddOrUpdateSagaInstance, AddSagaInstance, AddTaskCompletionSource, ConnectPublishHandler, GetTestHarness, RestartHostedServices, StartTestHarness, Stop, TryRemoveSagaInstance
      TimelineExtensions (class):  AddConsumers, OutputTimeline
      TraceInfo (class):  TraceInfo
      TrackedActivity (class):  ActivityStarted, ActivityStopped, DisposeAsync, GetSpan, OnTimeout, Sample, TrackedActivity
   MassTransit.Testing.Implementations
      AsyncElementList (class):  Any, Select, SelectAsync
      AsyncInactivityObserver (class):  AsyncInactivityObserver, CheckSourceActivity, Connected, ForceInactive, NoActivity, TimeoutTask
      BaseBusActivityIndicatorConnectable (class):  ConnectConditionObserver
      BaseSagaTestHarness (class):  Exists, Match, NotExists
      BusActivityConsumeIndicator (class):  BusActivityConsumeIndicator, ConsumeFault, PostConsume, PreConsume, Signal
      BusActivityMonitor (class):  AwaitBusInactivity, Signal
         Signalable resource which monitors bus activity.
      BusActivityPublishIndicator (class):  BusActivityPublishIndicator, PostPublish, PrePublish, PublishFault, Signal, SignalInactivity
         An activity indicator for publish endpoints.
      BusActivityReceiveIndicator (class):  BusActivityReceiveIndicator, ConsumeFault, PostConsume, PostReceive, PreReceive, ReceiveFault, Signal, SignalInactivity
         An activity indicator for receive endpoint queues.
      BusActivitySendIndicator (class):  BusActivitySendIndicator, PostSend, PreSend, SendFault, Signal, SignalInactivity
         An activity indicator for send endpoints.
      BusTestConsumeObserver (class):  BusTestConsumeObserver, ConsumeFault, PostConsume, PreConsume
      BusTestPublishObserver (class):  BusTestPublishObserver, PostPublish, PrePublish, PublishFault
      BusTestReceiveObserver (class):  BusTestReceiveObserver, ConsumeFault, PostConsume, PostReceive, PreReceive, ReceiveFault
      BusTestSendObserver (class):  BusTestSendObserver, PostSend, PreSend, SendFault
      ConditionExpression (class):  AddConditionBlock, CheckCondition, ClearAllConditions, ConditionExpression, ConditionUpdated
         A collection of blocks of conditions that must occur to signal a resource.
      IBusActivityMonitor (interface):  AwaitBusInactivity
         Represents a monitor for bus activity, allowing awaiting an inactive bus state.
      ICondition (interface)
         Represents a boolean condition
      IConditionObserver (interface):  ConditionUpdated
         Represents an observer on a change in boolean condition state.
      IInactivityObservationSource (interface):  ConnectInactivityObserver
      IInactivityObserver (interface):  Connected, ForceInactive, NoActivity
      IObservableCondition (interface):  ConnectConditionObserver
         Represents a boolean condition which may be observed.
      ISignalResource (interface):  Signal
         Represents a resource which may be signaled.
      InMemoryTestHarnessBusInstance (class):  Connect, ConnectReceiveEndpoint, GetRider, InMemoryTestHarnessBusInstance
      InactivityTestObserver (class):  ConnectInactivityObserver, Dispose, OnActivityTimeout, RestartTimer
      RegistrationConsumerTestHarness (class):  RegistrationConsumerTestHarness
      RegistrationSagaStateMachineTestHarness (class):  Exists, RegistrationSagaStateMachineTestHarness
      RegistrationSagaTestHarness (class):  RegistrationSagaTestHarness
      SagaInstance (class):  SagaInstance
      SagaList (class):  Add, Any, Contains, SagaList, Select, SelectAsync
      StateMachineSagaTestHarness (class):  Exists, StateMachineSagaTestHarness
      TestConsumeMessageObserver (class):  ConsumeFault, PostConsume, PreConsume, TestConsumeMessageObserver
      TestConsumeObserver (class):  ConsumeFault, PostConsume, PreConsume, TestConsumeObserver
      TestConsumerFactoryDecorator (class):  Probe, Send, TestConsumerFactoryDecorator
      TestDecoratorPipe (class):  Probe, Send, TestDecoratorPipe
      TestReceiveEndpointObserver (class):  Completed, Faulted, Ready, Stopping, TestReceiveEndpointObserver
   MassTransit.Topology
      CorrelatedByMessageCorrelationId (class):  TryGetCorrelationId
      DefaultDeadLetterQueueNameFormatter (class):  FormatDeadLetterQueueName
      DefaultErrorQueueNameFormatter (class):  FormatErrorQueueName
      DelegateMessageCorrelationId (class):  DelegateMessageCorrelationId, TryGetCorrelationId
      EntityCollection (class):  EntityCollection, Get, GetEnumerator, GetOrAdd
      EntityHandle (interface)
      IMessageTypeSelector (interface):  GetMessageTopology
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector, ImplementsMessageType
      MessagePublishTopology (class):  Add, AddDelegate, AddOrUpdateConvention, Apply, IsMessageTypeExcluded, MessagePublishTopology, TryAddConvention, TryGetPublishAddress, Validate
      MessageSendTopology (class):  Add, AddDelegate, AddOrUpdateConvention, Apply, MessageSendTopology, TryAddConvention, TryGetConvention, UpdateConvention, Validate
      MessageTopology (class):  ConnectMessageTopologyConfigurationObserver, GetMessageTopology, MessageTopology, OnMessageTopologyCreated, SetEntityName, SetEntityNameFormatter
      MessageTypeSelector (class):  GetMessageTopology, MessageTypeSelector
      MessageTypeSelectorFactory (struct):  ActivateType
      NamedEntityCollection (class):  GetOrAdd, NamedEntityCollection
      NullableDelegateMessageCorrelationId (class):  NullableDelegateMessageCorrelationId, TryGetCorrelationId
      NullablePropertyMessageCorrelationId (class):  NullablePropertyMessageCorrelationId, TryGetCorrelationId
      PropertyMessageCorrelationId (class):  PropertyMessageCorrelationId, TryGetCorrelationId
      PublishTopology (class):  AddMessagePublishTopology, ApplyConventionsToMessageTopology, ConnectPublishTopologyConfigurationObserver, GetMessageTopology, MessageTopologyCreated, PublishTopology, TryAddConvention, TryGetPublishAddress, Validate
      SendTopology (class):  AddMessageSendTopology, ApplyConventionsToMessageTopology, ConnectSendTopologyConfigurationObserver, GetMessageTopology, MessageTopologyCreated, SendTopology, TryAddConvention, Validate
      SetCorrelationIdMessageSendTopology (class):  Apply, SetCorrelationIdMessageSendTopology
      SetSerializerMessageSendTopology (class):  Apply, SetSerializerMessageSendTopology
   MassTransit.Transactions
      BaseTransactionalBus (class):  Add, ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectEndpointConfigurationObserver, ConnectPublishObserver, ConnectReceiveEndpoint, ConnectReceiveEndpointObserver, ConnectReceiveObserver, ConnectRequestPipe, ConnectSendObserver, GetPublishSendEndpoint, GetSendEndpoint, Probe, Publish
      ITransactionalBus (interface):  Release
      TransactionalBus (class):  Add, Release, TransactionalBus
      TransactionalBusPublishEndpointProvider (class):  ConnectPublishObserver, GetPublishSendEndpoint, TransactionalBusPublishEndpointProvider
      TransactionalBusSendEndpoint (class):  ConnectSendObserver, CreateSendContext, Send, TransactionalBusSendEndpoint
      TransactionalEnlistmentBus (class):  Add, ClearTransaction, GetOrCreateEnlistment, Release, TransactionCompleted, TransactionalEnlistmentBus
      TransactionalEnlistmentNotification (class):  Add, Commit, DiscardPendingActions, ExecutePendingActions, InDoubt, Prepare, Rollback, TransactionalEnlistmentNotification
   MassTransit.Transformation
      ConsumeTransformContext (class):  ConsumeTransformContext
         Sits in front of the consume context and allows the inbound message to be transformed.
      DelegatePropertyProvider (class):  DelegatePropertyProvider, GetProperty
         Copies the input property, as-is, for the property value
      MessageTransformConvention (class):  Add, MessageTransformConvention, TryGetHeaderInitializer, TryGetHeadersInitializer, TryGetPropertyInitializer
      MessageTransformPropertyContext (class):  MessageTransformPropertyContext
      PropertyTransformContext (class):  PropertyTransformContext
         For nested types transformed.
      ReplaceMessageFactory (class):  Create
      SendTransformContext (class):  SendTransformContext
         Sits in front of the consume context and allows the inbound message to be transformed.
      TransformPropertyContext (interface)
         A transform property context, which includes the , as well as the current input property value, if present.
      TransformPropertyConverter (class):  Convert, TransformPropertyConverter
      TransformPropertyInitializer (class):  Apply, TransformPropertyInitializer
         Set a message property using the property provider for the property value
   MassTransit.Transports
      ActiveDispatch (struct):  ActiveDispatch, Complete
      AddressEqualityComparer (class):  Equals, GetHashCode
      BaseHost (class):  AddReceiveEndpoint, AddRider, CheckHealth, ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectEndpointConfigurationObserver, ConnectPublishObserver, ConnectReceiveEndpoint, ConnectReceiveEndpointObserver, ConnectReceiveObserver, ConnectSendObserver, GetRider, Probe, Start, Stop
      BaseReceiveContext (class):  AddReceiveTask, Cancel, Dispose, NotifyConsumed, NotifyFaulted
      BaseReceiveEndpointContext (class):  AddConsumeAgent, AddSendAgent, ConnectPublishObserver, ConnectReceiveEndpointObserver, ConnectReceiveObserver, ConnectReceiveTransportObserver, ConnectSendObserver, ConvertException, CreateReceivePipeDispatcher, Probe, Reset
      BaseSendTransportContext (class):  ConnectSendObserver, CreateSendContext, GetAgentHandles
      BusDepot (class):  BusDepot, Start, Stop
      BusTopology (class):  Message, Publish, Send, TryGetPublishAddress
      CachedSendEndpoint (class):  CachedSendEndpoint, ConnectSendObserver, CreateSendContext, DisposeAsync, Send
      ConsumeSendEndpoint (class):  ConsumeSendEndpoint, ConsumeTask, Send
         Intercepts the ISendEndpoint and makes it part of the current consume context
      ConsumeSendPipeAdapter (class):  ConsumeSendPipeAdapter
      ConsumerAgent (class):  CancelPendingConsumers, HandleDeliveryComplete, SetConsumeTask
      ConsumerReceiveEndpointDispatcher (class):  Create
      DefaultMessageNameFormatter (class):  CreateMessageName, DefaultMessageNameFormatter, GetMessageName
      DelegatePartitionKeyFormatter (class):  DelegatePartitionKeyFormatter, FormatPartitionKey
      DelegateRoutingKeyFormatter (class):  DelegateRoutingKeyFormatter, FormatRoutingKey
      DeliveryMetrics (interface)
      DictionaryHeaderProvider (class):  DictionaryHeaderProvider, GetAll, TryGetHeader
         A simple in-memory header collection for use with the in memory transport
      DictionarySendHeaderProvider (class):  DictionarySendHeaderProvider, GetAll, TryGetHeader
         A simple in-memory header collection for use with the in memory transport
      DictionaryTransportSetHeaderAdapter (class):  DictionaryTransportSetHeaderAdapter, IsHeaderIncluded, Set, TrimHeaderIfLengthExceedsLimit
      EndpointAddressExtensions (class):  GetDiagnosticEndpointName, GetEndpointName
      EndpointHandle (class):  EndpointHandle, IsUnrecoverable, SetFaulted, SetReady, Start, Stop
      ExecuteActivityReceiveEndpointDispatcher (class):  Create
      FaultContext (class):  FaultContext
      Handle (class):  Handle, Start, StopAsync
      HealthResultReceiveEndpointObserver (class):  Completed, Faulted, HealthResultReceiveEndpointObserver, Ready, Stopping
      HostConfigurationRetryExtensions (class):  Retry
      HostHandle (interface):  Stop
      HostRiderHandle (interface):  StopAsync
      IBusInstance (interface):  Connect, GetRider
      IConsumePipe (interface)
      IDeadLetterTransport (interface):  Send
         If present, can be used to move the to the dead letter queue
      IDispatchMetrics (interface):  GetMetrics
      IErrorTransport (interface):  Send
         If present, can be used to move the to the error queue
      IHeaderProvider (interface):  GetAll, TryGetHeader
         Used to read a header from a transport message
      IHeaderValueConverter (interface):  TryConvert
      IHost (interface):  AddReceiveEndpoint, AddRider, CheckHealth, GetRider, Start
      IMessageNameFormatter (interface):  GetMessageName
         Used to format a message type into a MessageName, which can be used as a valid queue name on the transport
      IMessagePartitionKeyFormatter (interface):  FormatPartitionKey
      IMessageRoutingKeyFormatter (interface):  FormatRoutingKey
      IPartitionKeyFormatter (interface):  FormatPartitionKey
      IPublishPipe (interface):  Send
      IPublishTransportProvider (interface):  GetPublishTransport
      IReceiveEndpointCollection (interface):  Add, CheckEndpointHealth, Start, StartEndpoints, StopEndpoints
      IReceiveEndpointDependency (interface)
      IReceiveEndpointDependent (interface)
      IReceiveEndpointDispatcher (interface):  Dispatch
      IReceiveEndpointDispatcherFactory (interface):  CreateConsumerReceiver, CreateExecuteActivityReceiver, CreateReceiver, CreateSagaReceiver
      IReceivePipe (interface)
      IReceivePipeDispatcher (interface):  Dispatch
         Dispatches a prepared to a .
      IReceiveTransport (interface):  Start
      IRider (interface)
      IRiderCollection (interface):  Add, CheckEndpointHealth, Get, StartRider, StartRiders
      IRiderControl (interface):  CheckEndpointHealth, Start
      IRoutingKeyFormatter (interface):  FormatRoutingKey
      ISendContextPipe (interface):  Send
      ISendEndpointCache (interface):  GetSendEndpoint
      ISendPipe (interface)
      ISendTransport (interface):  CreateSendContext, Send
      ISendTransportProvider (interface):  GetSendTransport, NormalizeAddress
      ITransportSendEndpoint (interface):  CreateSendContext
      ITransportSetHeaderAdapter (interface):  Set
      ITransportSupervisor (interface):  AddConsumeAgent, AddSendAgent
      ITypeReceiveEndpointDispatcherFactory (interface):  Create
      JsonTransportHeaders (class):  Get, GetAll, GetEnumerator, JsonTransportHeaders, TryGetHeader
         The context headers are sourced from the IContextHeaderProvider, with the use of a Json deserializer to convert data ...
      Lock (struct):  Lock
      MessagePartitionKeyFormatter (class):  FormatPartitionKey, MessagePartitionKeyFormatter
      MessageRoutingKeyFormatter (class):  FormatRoutingKey, MessageRoutingKeyFormatter
      Metrics (class):  Metrics
      MultiBusInstance (class):  Connect, ConnectReceiveEndpoint, FormatBusName, GetRider, MultiBusInstance
      Observer (class):  Completed, Faulted, Observer, Ready
      PendingReceiveLockContext (class):  Cancel, Complete, Enqueue, Execute, Faulted, TryDequeue, ValidateLockStatus
      PipeAdapter (class):  PipeAdapter, Probe, Send
      PublishEndpoint (class):  ConnectPublishObserver, Publish, PublishEndpoint, PublishInternal
         The publish endpoint delivers messages to the topic/exchange/whatever based upon the publish topology of the broker, ...
      PublishEndpointProvider (class):  ConnectPublishObserver, CreateSendEndpoint, GetPublishSendEndpoint, PublishEndpointProvider
      PublishSendPipeAdapter (class):  Probe, PublishSendPipeAdapter, Send
      ReadOnlyDictionaryHeaderProvider (class):  GetAll, ReadOnlyDictionaryHeaderProvider, TryGetHeader
      ReadyEvent (class):  ReadyEvent
      ReceiveEndpoint (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectPublishObserver, ConnectReceiveEndpointObserver, ConnectReceiveObserver, ConnectRequestPipe, ConnectSendObserver, GetPublishSendEndpoint, GetSendEndpoint, IsStarted, Probe, ReceiveEndpoint, Start, Stop
         A receive endpoint is called by the receive transport to push messages to consumers.
      ReceiveEndpointCollection (class):  Add, CheckEndpointHealth, ConnectConsumeMessageObserver, ConnectReceiveEndpointObserver, Probe, ReceiveEndpointCollection, Remove, Start, StartEndpoint, StartEndpoints, StopEndpoints
      ReceiveEndpointContext (interface):  AddConsumeAgent, AddSendAgent, ConvertException, CreateReceivePipeDispatcher, Reset
         The context of a receive endpoint
      ReceiveEndpointDispatcher (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectPublishObserver, ConnectReceiveObserver, ConnectSendObserver, Dispatch, GetMetrics, Probe, ReceiveEndpointDispatcher
      ReceiveEndpointDispatcherFactory (class):  CreateConsumerReceiver, CreateExecuteActivityReceiver, CreateMessageReceiver, CreateReceiver, CreateSagaReceiver, DisposeAsync, ReceiveEndpointDispatcherFactory
      ReceiveEndpointDispatcherReceiveContext (class):  IsRedelivered, ReceiveEndpointDispatcherReceiveContext
      ReceiveEndpointLoggingExtensions (class):  GetMessageId, LogCanceled, LogConsumed, LogConsumerCompleted, LogFaulted, LogMoved, LogRetry, LogScheduled, LogSent, LogSkipped, LogTransportDupe, LogTransportFaulted
      ReceiveLockContext (interface):  Complete, Faulted, ValidateLockStatus
         Encapsulates a transport lock
      ReceivePipeDispatcher (class):  ConnectConsumeMessageObserver, ConnectConsumeObserver, ConnectConsumePipe, ConnectReceiveObserver, ConnectRequestPipe, Dispatch, DispatchComplete, GetMetrics, Probe, ReceivePipeDispatcher, StartDispatch
      ReceiveTransport (class):  ConnectPublishObserver, ConnectReceiveObserver, ConnectReceiveTransportObserver, ConnectSendObserver, Probe, ReceiveTransport, Start
      ReceiveTransportAgent (class):  NotifyFaulted, ReceiveTransportAgent, Run, RunTransport, Stop
      ReceiveTransportHandle (interface):  Stop
         A handle to an active transport
      ReceiveTransportObserverExtensions (class):  NotifyCompleted, NotifyFaulted, NotifyReady
      RiderCollection (class):  Add, CheckEndpointHealth, Get, Remove, RiderCollection, StartRider, StartRiders
      RiderHandle (interface):  StopAsync
      SagaReceiveEndpointDispatcher (class):  Create
      SendContextPipeAdapter (class):  Probe, Send
      SendEndpoint (class):  ConnectSendObserver, CreateSendContext, DisposeAsync, Send, SendEndpoint
      SendEndpointCache (class):  GetSendEndpoint, GetSendEndpointFromFactory, SendEndpointCache
         Caches SendEndpoint instances by address (ignoring the query string entirely, case insensitive)
      SendEndpointCacheDefaults (class):  SendEndpointCacheDefaults
      SendEndpointPipe (class):  Probe, Send, SendEndpointPipe
      SendEndpointProvider (class):  ConnectSendObserver, CreateSendEndpoint, GetSendEndpoint, SendEndpointProvider
      SendEndpointProxy (class):  ConnectSendObserver, CreateSendContext, Send
         Generalized proxy for ISendEndpoint to intercept pipe/context
      SendPipe (class):  Probe, Send, SendPipe
      SendTransport (class):  ConnectSendObserver, CreateSendContext, DisposeAsync, Send, SendTransport
      SendTransportContext (interface):  CreateSendContext, GetAgentHandles, Send
      SimpleHeaderValueConverter (class):  TryConvert
      StartHostHandle (class):  EndpointsReady, ReadyOrNot, RidersReady, StartHostHandle, Stop
      StartObserver (class):  Completed, ConnectEndpointHandle, Faulted, Ready, StartObserver, Stopping
      StringHeaderValueConverter (class):  TryConvert
      SupervisorStoppingContext (class):  SupervisorStoppingContext
      TransportBusInstance (class):  Connect, ConnectReceiveEndpoint, GetRider, GetRiderName, TransportBusInstance
      TransportLogMessages (class)
      TransportPipeContextSupervisor (class):  AddConsumeAgent, AddSendAgent
      TransportSetHeaderAdapter (class):  IsHeaderIncluded, Set, TransportSetHeaderAdapter
      TransportSetHeaderAdapterExtensions (class):  Set, SetExceptionHeaders, SetHostHeaders, ToString, TryGetInt
      TransportStartExtensions (class):  OnTransportStartup
      TransportStoppingContext (class):  TransportStoppingContext
      WaitForConnectionPipe (class):  Probe, Send, WaitForConnectionPipe
   MassTransit.Transports.Components
      FilterSpecification (class):  Build
      IKillSwitch (interface):  Restart, Started, Stop
      IKillSwitchState (interface)
      KillSwitch (class):  CompensateFail, Completed, ConsumeFault, ExecuteFault, Faulted, KillSwitch, PostCompensate, PostConsume, PostExecute, PreCompensate, PreConsume, PreExecute, Ready, Restart, RestartReceiveEndpoint
         The KillSwitch monitors a receive endpoint, stopping and restarting as necessary in the presence of exceptions.
      KillSwitchOptions (class):  KillSwitchOptions, SetActivationThreshold, SetExceptionFilter, SetRestartTimeout, SetTrackingPeriod, SetTripThreshold, Validate
      RestartingKillSwitchState (class):  ConsumeFault, PostConsume, PreConsume, Probe, RestartingKillSwitchState
      StartedKillSwitchState (class):  Activate, ConsumeFault, LogThreshold, PostConsume, PreConsume, Probe, Reset, StartedKillSwitchState, TripThresholdExceeded
      StoppedKillSwitchState (class):  Activate, ConsumeFault, PostConsume, PreConsume, Probe, Restart, StoppedKillSwitchState
   MassTransit.Transports.Fabric
      ArrayExtensions (class):  Copy, Shuffle
      Counter (class):  Add
      DeliveryContext (interface):  Delivered, WasAlreadyDelivered
      Gauge (class):  Add, NotifyZeroActivity, Remove
      Handle (class):  Disconnect, Handle
      IMessageExchange (interface)
      IMessageFabric (interface):  ExchangeBind, ExchangeDeclare, GetExchange, GetQueue, QueueBind, QueueDeclare
      IMessageFabricObserver (interface):  ConsumerConnected, ExchangeBindingCreated, ExchangeDeclared, QueueBindingCreated, QueueDeclared
      IMessageFabricObserverConnector (interface):  ConnectMessageFabricObserver
      IMessageQueue (interface):  ConnectMessageReceiver
      IMessageReceiver (interface):  Deliver
         Receives messages from a queue
      IMessageSink (interface):  Deliver
      IMessageSource (interface):  Connect
      IReceiverLoadBalancer (interface):  SelectReceiver
      ImplementedBuilder (class):  CreateImplementedBuilder, ExchangeBind, ExchangeDeclare, ImplementedBuilder, QueueBind, QueueDeclare
      MessageDirectExchange (class):  Connect, Deliver, MessageDirectExchange, Probe, ToString
      MessageFabric (class):  ConnectMessageFabricObserver, ExchangeBind, ExchangeDeclare, GetExchange, GetOrAddExchange, GetOrAddQueue, GetQueue, MessageFabric, Probe, QueueBind, QueueDeclare, ValidateBinding
      MessageFabricConsumeTopologyBuilder (class):  ExchangeBind, ExchangeDeclare, MessageFabricConsumeTopologyBuilder, QueueBind, QueueDeclare
      MessageFabricObservable (class):  ConsumerConnected, ExchangeBindingCreated, ExchangeDeclared, QueueBindingCreated, QueueDeclared
      MessageFabricPublishTopologyBuilder (class):  CreateImplementedBuilder, ExchangeBind, ExchangeDeclare, MessageFabricPublishTopologyBuilder, QueueBind, QueueDeclare
      MessageFanOutExchange (class):  Connect, Deliver, MessageFanOutExchange, Probe, ToString
      MessageQueue (class):  ConnectMessageReceiver, Deliver, DeliverWithDelay, MessageQueue, Probe, StartDispatcher
      MessageReceiverCollection (class):  Connect, Disconnect, MessageReceiverCollection, Next, Probe, TryGetReceiver
      MessageTopicExchange (class):  Connect, Deliver, MessageTopicExchange, Probe, ToString
      Metric (class)
      QueueMetric (class):  QueueMetric
      Receiver (class):  Receiver
      RoundRobinReceiverLoadBalancer (class):  BuildList, RoundRobinReceiverLoadBalancer, SelectReceiver
      SingleReceiverLoadBalancer (class):  SelectReceiver, SingleReceiverLoadBalancer
      TopicNode (class):  Add, Bind, Deliver, GetChild, Probe, TopicNode
      TopologyHandle (interface):  Disconnect
   MassTransit.UsageTelemetry
      BusUsageTelemetry (class)
      EndpointUsageTelemetry (class)
      HostUsageTelemetry (class)
      MassTransitUsageTelemetry (class)
      RiderUsageTelemetry (class)
   MassTransit.UsageTracking
      CloudEnvironmentInfo (struct):  CloudEnvironmentInfo
      IUsageTelemetrySource (interface):  Update
      IUsageTracker (interface):  PreConfigureBus, PreConfigureRider
      MassTransitUsageTelemetryExtensions (class):  AddEndpoint, MapEndpointType
      UsageTelemetryBusObserver (class):  CreateFaulted, PostCreate, PostStart, PostStop, PreStart, PreStop, StartFaulted, StopFaulted, UsageTelemetryBusObserver
      UsageTelemetryConfigurationObserver (class):  ActivityConfigured, CompensateActivityConfigured, ConsumerConfigured, ConsumerMessageConfigured, ExecuteActivityConfigured, HandlerConfigured, SagaConfigured, SagaMessageConfigured, StateMachineSagaConfigured, Update, UsageTelemetryConfigurationObserver
      UsageTelemetryEndpointConfigurationObserver (class):  EndpointConfigured, Update, UsageTelemetryEndpointConfigurationObserver
      UsageTelemetrySerializerContext (class)
      UsageTracker (class):  DetectCloudEnvironment, DisposeAsync, PostCreateBus, PostStartBus, PreConfigureBus, PreConfigureRider, ReportUsageTelemetry, UsageTracker
   MassTransit.Util
      ActionFuture (class):  ActionFuture, Run
      ActiveRequest (struct):  ActiveRequest, Callback, Complete, Dispose
      AssemblyTypeCache (class):  Clear, FailedAssemblies, FindTypes, FindTypesInNamespace, ForAssembly, ThrowIfAnyTypeScanFailures
         Caches assemblies and assembly types to avoid repeated assembly scanning
      AwaitAdapter (class):  GetResult, OnCompleted
      BaseFuture (class)
      Cached (class):  GetCanceledTask
      ChannelExecutor (class):  ChannelExecutor, DisposeAsync, Push, PushWithWait, ReadFromChannel, Run
      ChartRow (class):  ChartRow, GetColumn
      ChartTable (class):  Add, CalculateRange, ChartTable, GetRows
      Connectable (class):  All, Connect, Connectable, Disconnect, ForEach, ForEachAsync, Method1, Method2, Method3
         Maintains a collection of connections of the generic type
      ConvertObject (class):  AddPropertyToDictionary, ToDictionary
      DisposableAction (class):  DisposableAction, Dispose
      DisposeAsyncExtensions (class):  DisposeAsync
      EmptyConnectHandle (class):  Disconnect, Dispose
         A do-nothing connect handle, simply to satisfy
      ExceptionUtil (class):  ExceptionUtil, GetExceptionHeaderDetail, GetExceptionHeaderDictionary, GetMessage, GetStackTrace
      FormatUtil (class)
      FuncFuture (class):  FuncFuture, Run
      Future (class):  Future, Run
      Handle (class):  Disconnect, Dispose, Handle, Method1, Method2
      IChannelExecutorPool (interface):  Push, Run
      IFuture (interface):  Run
      LambdaEqualityComparer (class):  Equals, GetHashCode, LambdaEqualityComparer
      LambdaEqualityComparerExtensions (class):  Distinct, Except
      Line (class):  Line
      MultipleConnectHandle (class):  Disconnect, Dispose, MultipleConnectHandle
      NoSynchronizationDispatcher (class):  NoSynchronizationDispatcher, WaitForCompletion
      ObservableObserver (class):  ObservableObserver, OnCompleted, OnError, OnNext, Subscribe
      PartitionChannelExecutorPool (class):  DisposeAsync, GetChannelExecutor, PartitionChannelExecutorPool, Push, Run
      PendingTaskCollection (class):  Add, Completed, PendingTaskCollection, Remove
      Recycle (class):  Recycle
         Recycles a supervisor once it is stopped, replacing it with a new one
      RequestRateAlgorithm (class):  Add, BeginRequest, ChangeRateLimit, ChangeRequestCount, Dispose, Remove, RequestRateAlgorithm, Reset, Run, RunRequest, RunResultSet
      RequestRateAlgorithmOptions (class)
      RollingTimer (class):  Dispose, Reset, Restart, RollingTimer, Set, Start, StartInternal, Stop
         Thread safe timer that allows efficient restarts by rolling the due time further into the future.
      RunFuture (struct):  Run, RunFuture
      ScheduledWork (struct):  Execute, ScheduledWork
      SingleThreadedDictionary (class):  Clear, ContainsKey, GetEnumerator, SingleThreadedDictionary, TryAdd, TryGetValue, TryRemove
      SingleThreadedSynchronizationContext (class):  AddWork, Dispose, ErrorAndGetExceptionForShutdownTimeout, Post, Run, Send, ShutDown, SingleThreadedSynchronizationContext, TryTake
      SingleThreadedSynchronizationDispatcher (class):  SingleThreadedSynchronizationDispatcher, WaitForCompletion
      SynchronizationDispatcher (class):  FromCurrentSynchronizationContext, WaitForCompletion
      SynchronousFuture (class):  Run, SynchronousFuture
      TaskAwaitAdapter (class):  GetResult, GetResultOfT, OnCompleted, TaskAwaitAdapter
      TaskExecutor (class):  DisposeAsync, MultipleReader, Push, Run, SingleReader, TaskExecutor
         The successor to , now with a more optimized execution pipeline resulting in lower memory usage and reduced overhead.
      TaskFuture (class):  Run, TaskFuture
      TaskUtil (class):  Await, Cancel, Cancelled, ContinueOnSameSynchronizationContext, Default, Faulted, GetTask, InitializeExecutionEnvironment, RegisterIfCanBeCanceled, RegisterTask, SetCompleted, SetSynchronizationContext
      TextTable (class):  AddColumns, AddRow, CalculateColumnWidths, Configure, Create, EnableCount, GetNumberAlignment, HideRowSeparator, OutputTo, SetColumn, SetRightNumberAlignment, TextTable, ToString, Write
         Generates a monospaced text table, useful in trace output formats.
      TextTableOptions (class)
      TypeSet (class):  AllTypes, FindTypes, TypeSet
         Access to a set of exported .Net Type's as defined in a scanning operation
      WindowsFormsSynchronizationDispatcher (class):  GetIfApplicable, IsApplicable, WaitForCompletion, WindowsFormsSynchronizationDispatcher
      WpfSynchronizationDispatcher (class):  GetIfApplicable, IsApplicable, WaitForCompletion, WpfSynchronizationDispatcher
   MassTransit.Util.Scanning
      AssemblyFinder (class):  FindAssemblies
      AssemblyScanRecord (class):  ToString
      AssemblyScanTypeInfo (class):  AssemblyScanTypeInfo, FindTypes, SelectGroups, SelectLists, SelectTypes
      AssemblyScanner (class):  AssembliesAndExecutablesFromApplicationBaseDirectory, AssembliesAndExecutablesFromPath, AssembliesFromApplicationBaseDirectory, AssembliesFromPath, Assembly, AssemblyContainingType, Contains, Exclude, ExcludeFileNameStartsWith, ExcludeNamespace, ExcludeNamespaceContainingType, ExcludeType, FindTheCallingAssembly, HasAssemblies, Include
      AssemblyTypeList (class):  Add, AllTypes, SelectTypes
      IAssemblyScanner (interface):  AssembliesAndExecutablesFromApplicationBaseDirectory, AssembliesAndExecutablesFromPath, AssembliesFromApplicationBaseDirectory, AssembliesFromPath, Assembly, AssemblyContainingType, Exclude, ExcludeFileNameStartsWith, ExcludeNamespace, ExcludeNamespaceContainingType, ExcludeType, Include, IncludeFileNameStartsWith, IncludeNamespace, IncludeNamespaceContainingType
      TypeQuery (class):  Find, TypeQuery
   MassTransit.Visualizer
      StateMachineGraphvizGenerator (class):  CreateDotFile, StateMachineGraphvizGenerator, VertexStyler
      StateMachineMermaidGenerator (class):  CreateMermaidFile, FormatVertex, GetVertexLabel, StateMachineMermaidGenerator
   MassTransit.Visualizer.Abstractions
      StateMachineGenerator (class):  CreateAdjacencyGraph, StateMachineGenerator
   global
      MaybeNullWhenAttribute (class):  MaybeNullWhenAttribute
      MemberNotNullWhenAttribute (class):  MemberNotNullWhenAttribute
   INTERNAL  (60 types in *.Internal — available on request)

CONSUMER PATHS
   annotate  →  [ConfigureConsumeTopology] on a partial class/member
   annotate  →  [EntityName] on a partial class/member
   annotate  →  [ExcludeFromConfigureEndpoints] on a partial class/member
   annotate  →  [ExcludeFromImplementedTypes] on a partial class/member
   annotate  →  [ExcludeFromTopology] on a partial class/member
   annotate  →  [FaultEntityName] on a partial class/member

ENTRY POINTS
   Bus (31)
      AllConsumer  → AllConsumer  (src/MassTransit.SignalR/Consumers/AllConsumer.cs:12)
      BatchConsumer  → BatchConsumer  (src/MassTransit/Consumers/Batching/BatchConsumer.cs:14)
      CancelScheduledMessageConsumer  → CancelScheduledMessageConsumer  (src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/CancelScheduledMessageConsumer.cs:8)
      ConnectionConsumer  → ConnectionConsumer  (src/MassTransit.SignalR/Consumers/ConnectionConsumer.cs:11)
      FinalizeJobConsumer  → FinalizeJobConsumer  (src/MassTransit/JobService/JobService/FinalizeJobConsumer.cs:10)
      GroupConsumer  → GroupConsumer  (src/MassTransit.SignalR/Consumers/GroupConsumer.cs:12)
      GroupManagementConsumer  → GroupManagementConsumer  (src/MassTransit.SignalR/Consumers/GroupManagementConsumer.cs:8)
      MediatorRequestHandler  → MediatorRequestHandler  (src/MassTransit.Abstractions/Mediator/MediatorRequestHandler.cs:31)
      MessageHandlerConsumer  → MessageHandlerConsumer  (src/MassTransit/DependencyInjection/DependencyInjection/MessageHandlerConsumer.cs:72)
      PauseScheduledMessageConsumer  → PauseScheduledMessageConsumer  (src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/PauseScheduledMessageConsumer.cs:8)
      PauseScheduledRecurringMessageConsumer  → PauseScheduledRecurringMessageConsumer  (src/Scheduling/MassTransit.HangfireIntegration/HangfireIntegration/PauseScheduledRecurringMessageConsumer.cs:10)
      RequestHandlerConsumer  → RequestHandlerConsumer  (src/MassTransit/DependencyInjection/DependencyInjection/RequestHandlerConsumer.cs:82)
      ResumeScheduledMessageConsumer  → ResumeScheduledMessageConsumer  (src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/ResumeScheduledMessageConsumer.cs:8)
      ResumeScheduledRecurringMessageConsumer  → ResumeScheduledRecurringMessageConsumer  (src/Scheduling/MassTransit.HangfireIntegration/HangfireIntegration/ResumeScheduledMessageConsumer.cs:10)
      RoutingSlipActivityCompensatedConsumer  → RoutingSlipActivityCompensatedConsumer  (src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consumers/RoutingSlipActivityCompensatedConsumer.cs:8)
      RoutingSlipActivityCompensationFailedConsumer  → RoutingSlipActivityCompensationFailedConsumer  (src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consumers/RoutingSlipActivityCompensationFailedConsumer.cs:8)
      RoutingSlipActivityCompletedConsumer  → RoutingSlipActivityCompletedConsumer  (src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consumers/RoutingSlipActivityCompletedConsumer.cs:8)
      RoutingSlipActivityFaultedConsumer  → RoutingSlipActivityFaultedConsumer  (src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consumers/RoutingSlipActivityFaultedConsumer.cs:8)
      RoutingSlipCompensationFailedConsumer  → RoutingSlipCompensationFailedConsumer  (src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consumers/RoutingSlipCompensationFailedConsumer.cs:8)
      RoutingSlipCompletedConsumer  → RoutingSlipCompletedConsumer  (src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consumers/RoutingSlipCompletedConsumer.cs:8)
      … and 11 more (bus entries)

PACKAGES
   Web/API:  Microsoft.ApplicationInsights.AspNetCore 2.23.0, Microsoft.AspNetCore.SignalR.Core 1.1.0
   ORM/Data:  Dapper 2.1.79, dapper.contrib 2.0.78, EntityFramework 6.5.2, Microsoft.Azure.Cosmos 3.61.0, Microsoft.EntityFrameworkCore.Relational 9.0.1, Npgsql 8.0.7
   Messaging:  Apache.NMS.AMQP 2.4.0, Azure.Messaging.EventHubs.Processor 5.12.2, Azure.Messaging.ServiceBus 7.20.1, RabbitMQ.Client 7.2.1
   Cloud:  Azure.Data.Tables 12.11.0, Azure.Identity 1.21.0, Azure.Storage.Blobs 12.28.0, Microsoft.Azure.WebJobs.Extensions.EventHubs 6.5.3, Microsoft.Azure.WebJobs.Extensions.ServiceBus 5.17.0
   Utilities:  Newtonsoft.Json 13.0.4, Newtonsoft.Json.Bson 1.0.3
   Other:  Apache.NMS.ActiveMQ 2.2.0, AWSSDK.DynamoDBv2 4.0.17.7, AWSSDK.S3 4.0.20.4, AWSSDK.SimpleNotificationService 4.0.2.26, AWSSDK.SQS 4.0.2.24, Confluent.Kafka 2.14.2, Hangfire.Core 1.8.23, Iesi.Collections 4.1.1 … (36 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus Activity)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 2275ms |
| GenericExtraction | 24453ms |
| SignalSealing | 0ms |
| SpecificExtraction | 4855ms |
| Compression | 597ms |
| **Total** | **61894ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 24450ms | 8135 | 204 |
| DiRegistrationExtractor | 24441ms | 12 | 204 |
| CallGraphExtractor | 4016ms | 0 | 0 |
| ProgramCsFlowExtractor | 2417ms | 0 | 0 |
| SourceBodyExtractor | 1435ms | 0 | 0 |
| ProjectStructure | 1301ms | 0 | 0 |
| BodyFactsExtractor | 819ms | 0 | 0 |
| EventBusExtractor | 809ms | 0 | 1696 |
| FileTreeExtractor | 786ms | 0 | 0 |
| InMemoryEventBusExtractor | 532ms | 0 | 1130 |
| MediatRExtractor | 528ms | 0 | 1122 |
| EfCoreExtractor | 520ms | 0 | 1117 |
| SignalRHubExtractor | 517ms | 0 | 1117 |
| IndirectWiringDetector | 216ms | 0 | 833 |
| SolutionDiscovery | 182ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 10318 | 5074 |
| Sends | 20 | 0 |
| Raises | 525 | 247 |
| Consumes | 44 | 0 |
| ReadsWrites | 5 | 2 |
| Resolves | 602 | 493 |
| ServiceLink | 12 | 0 |

_5502 files · 59 projects_
