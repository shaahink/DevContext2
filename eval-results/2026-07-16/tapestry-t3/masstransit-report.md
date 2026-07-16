Overview map (no focus).
Analyzing project...

LIBRARY  MassTransit     (4531 public types)

ENTRY API
   annotate  [ConfigureConsumeTopology]   (ConfigureConsumeTopologyAttribute.cs)
      Specify whether the message type should be used to configure the broker 
topology for the consumer.
   annotate  [EntityName]   (EntityNameAttribute.cs)
      Specify the EntityName used for this message contract if configured.
   annotate  [ExcludeFromConfigureEndpoints]   
(ExcludeFromConfigureEndpointsAttribute.cs)
      When added to a consuming type (consumer, saga, activity, etc), prevents 
MassTransit from configuring endpoint for it...
   annotate  [ExcludeFromImplementedTypes]   
(ExcludeFromImplementedTypesAttribute.cs)
      Typically added to base messages types, such as IMessage, IEvent, etc.
   annotate  [ExcludeFromTopology]   (ExcludeFromTopologyAttribute.cs)
      When added to a message type (class, record, or interface), prevents 
MassTransit from creating an exchange or topic o...
   annotate  [FaultEntityName]   (FaultEntityNameAttribute.cs)
      Specify the EntityName used for the Fault version of this message 
contract, overriding the configured if configured.
   annotate  [HashCleanup]   (HashCleanupAttribute.cs)
   annotate  [Indexed]   (IndexedAttribute.cs)
      Specifies a property that should be indexed by the in-memory saga 
repository
   annotate  [MaybeNullWhen]   (NullableAttributes.cs)
   annotate  [MemberNotNullWhen]   (NullableAttributes.cs)
   annotate  [MessageUrn]   (MessageUrnAttribute.cs)
      Specify the message type name for this message type
   annotate  [RecurringScheduleDateTimeInterval]   
(RecurringScheduleDateTimeIntervalAttribute.cs)

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
         Active Jobs are allocated a concurrency slot, and are valid until the 
deadline is reached, after which they may be au...
      ActiveMqBusFactory (class):  Create, CreateMessageTopology
      ActiveMqBusFactoryConfiguratorExtensions (class):  CreateUsingActiveMq, 
UsingActiveMq
      ActiveMqConfigureEndpointCallbackExtensions (class):  
AddActiveMqConfigureEndpointCallback, AddActiveMqConfigureEndpointsCallback
      ActiveMqConnectException (class):  ActiveMqConnectException
      ActiveMqConnectionException (class):  ActiveMqConnectionException
      ActiveMqDeferMessageExtensions (class):  Defer
      ActiveMqEndpointAddress (struct):  ActiveMqEndpointAddress, 
GetQueryStringOptions, ParseLeft
      ActiveMqHostAddress (struct):  ActiveMqHostAddress, ParseLeft
      ActiveMqHostConfigurationExtensions (class):  Host, ReceiveEndpoint
      … and 1102 more (use --format json for the full surface)
   MassTransit.ActiveMqTransport
      ActiveMqConnectionContext (class):  ActiveMqConnectionContext, 
CreateSession, DisposeAsync, GetTemporaryQueue, GetTemporaryTopic, 
IsVirtualTopicConsumer, TryGetTemporaryEntity, TryRemoveTemporaryEntity
      ActiveMqConsumerReceiveEndpointContext (class):  
ActiveMqConsumerReceiveEndpointContext, AddConsumeAgent, AddSendAgent, 
ConvertException, Probe
      ActiveMqDeadLetterTransport (class):  ActiveMqDeadLetterTransport, Send
      ActiveMqErrorTransport (class):  ActiveMqErrorTransport, Send
      ActiveMqHeaderProvider (class):  ActiveMqHeaderProvider, GetAll, 
TryGetHeader
      ActiveMqHost (class):  ActiveMqHost, ConnectReceiveEndpoint
      ActiveMqHostSettingsExtensions (class):  ToDescription
      ActiveMqMessageBody (class):  ActiveMqMessageBody, GetBytes, GetStream, 
GetString
      ActiveMqMessageNameFormatter (class):  ActiveMqMessageNameFormatter, 
GetMessageName
      ActiveMqMoveTransport (class):  CloneMessage
      ActiveMqPublishTransportProvider (class):  
ActiveMqPublishTransportProvider, GetPublishTransport
      ActiveMqReceiveContext (class):  ActiveMqReceiveContext, 
GetTransportProperties
      … and 35 more (use --format json for the full surface)
   MassTransit.ActiveMqTransport.Configuration
      ActiveMqBusConfiguration (class):  ActiveMqBusConfiguration, 
ConnectBusObserver, ConnectEndpointConfigurationObserver
      ActiveMqBusFactoryConfigurator (class):  ActiveMqBusFactoryConfigurator, 
CreateBusEndpointConfiguration, EnableArtemisCompatibility, Host, Publish, 
ReceiveEndpoint, Send, SetConsumerEndpointQueueNameFormatter, 
SetTemporaryQueueNameFormatter, SetTemporaryQueueNamePrefix, Validate
      ActiveMqEndpointConfiguration (class):  ActiveMqEndpointConfiguration, 
CreateEndpointConfiguration
      ActiveMqHostConfiguration (class):  ActiveMqHostConfiguration, 
ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, 
ReceiveEndpoint
      ActiveMqHostConfigurator (class):  ActiveMqHostConfigurator, 
EnableAsyncSend, EnableOptimizeAcknowledge, FailoverHosts, Password, 
SetPrefetchPolicy, SetQueuePrefetchPolicy, TransportOptions, UseSsl, Username
      ActiveMqQueueBindingConfigurator (class)
      ActiveMqQueueConfigurator (class)
      ActiveMqQueueReceiveSettings (class):  ActiveMqQueueReceiveSettings, 
GetInputAddress
      ActiveMqReceiveEndpointBuilder (class):  ActiveMqReceiveEndpointBuilder, 
BuildTopology, ConnectConsumePipe, CreateDeadLetterTransport, 
CreateErrorTransport, CreateReceiveEndpointContext
      ActiveMqReceiveEndpointConfiguration (class):  
ActiveMqReceiveEndpointConfiguration, Bind, Build, ConfigureConnection, 
ConfigureSession, CreateActiveMqReceiveEndpointContext, 
CreateReceiveEndpointContext, FormatInputAddress, Validate
      ActiveMqRegistrationBusFactory (class):  ActiveMqRegistrationBusFactory, 
CreateBus
      ActiveMqTopicBindingConfigurator (class):  
ActiveMqTopicBindingConfigurator
      … and 14 more (use --format json for the full surface)
   MassTransit.ActiveMqTransport.Middleware
      ActiveMqConsumer (class):  ActiveMqConsumer, HandleMessage
         Receives messages from ActiveMQ, pushing them to the InboundPipe of the
service endpoint.
      ActiveMqConsumerFilter (class):  ActiveMqConsumerFilter, CreateConsumer, 
CreateConsumerSupervisor, GetReceiveEntityName, Probe, Send
         A filter that uses the model context to create a basic consumer and 
connect it to the model
      CombinedDeliveryMetrics (class):  CombinedDeliveryMetrics
      ConfigureActiveMqTopologyFilter (class):  Configure, 
ConfigureActiveMqTopologyFilter, ConfigureTopology, Declare, Probe, Send
         Configures the broker with the supplied topology once the model is 
created, to ensure that the exchanges, queues, and...
      ConfigureTopologyContext (interface)
      ConsumerSupervisor (class):  ConsumerSupervisor
      RemoveAutoDeleteAgent (class):  Delete, DeleteAutoDelete, 
RemoveAutoDeleteAgent
   MassTransit.ActiveMqTransport.Topology
      ActiveMqBrokerTopology (class):  ActiveMqBrokerTopology, Probe
      ActiveMqBusTopology (class):  ActiveMqBusTopology, GetDestinationAddress, 
GetSendSettings, Publish, Send
      ActiveMqConsumeTopology (class):  ActiveMqConsumeTopology, 
AddSpecification, Apply, Bind, CreateTemporaryQueueName, GetMessageTopology, 
Validate
      ActiveMqDeadLetterSettings (class):  ActiveMqDeadLetterSettings, 
GetBrokerTopology
      ActiveMqEntityNameValidator (class):  IsValidEntityName, 
ThrowIfInvalidEntityName
      ActiveMqErrorSettings (class):  ActiveMqErrorSettings, GetBrokerTopology
      ActiveMqHostEqualityComparer (class):  Equals, GetHashCode
      ActiveMqMessageConsumeTopology (class):  ActiveMqMessageConsumeTopology, 
Apply, Bind, Validate
      ActiveMqMessagePublishTopology (class):  ActiveMqMessagePublishTopology, 
Apply, GetBrokerTopology, GetSendSettings, TryGetPublishAddress
      ActiveMqMessageSendTopology (class)
      ActiveMqPublishTopology (class):  ActiveMqPublishTopology, 
GetMessageTopology, GetPublishBrokerTopology
      ActiveMqQueueSendSettings (class):  ActiveMqQueueSendSettings, 
GetBrokerTopology, GetSendAddress, GetSettingStrings, ToString
      … and 29 more (use --format json for the full surface)
   MassTransit.Agents
      ActivePipeContext (class):  ActivePipeContext, DisposeAsync, Faulted
         An active reference to a pipe context, which is managed by an existing 
pipe context handle.
      ActivePipeContextAgent (class):  ActivePipeContextAgent, DisposeAsync, 
Faulted, ToString
         An Agent Provocateur that uses a context handle for the activate state 
of the agent
      ActivePipeContextHandle (interface):  Faulted
         An active, in-use reference to a pipe context.
      AsyncPipeContextAgent (class):  AsyncPipeContextAgent, CreateCanceled, 
CreateFaulted, Created, DisposeAsync, Faulted, Stop, ToString
         A PipeContext, which as an agent can be Stopped, which disposes of the 
context making it unavailable
      AsyncPipeContextFilter (class):  AsyncPipeContextFilter, Probe, Send
         Completes the AsyncPipeContextAgent when the context is sent to the 
pipe, and doesn't return until the agent completes
      AsyncPipeContextHandle (class):  AsyncPipeContextHandle, CreateCanceled, 
CreateFaulted, Created, DisposeAsync, Faulted
         An asynchronously pipe context handle, which can be completed.
      AsyncPipeContextPipe (class):  AsyncPipeContextPipe, Probe, Send
         Completes the AsyncPipeContextAgent when the context is sent to the 
pipe, and doesn't return until the agent completes
      ConstantPipeContextHandle (class):  ConstantPipeContextHandle, 
DisposeAsync
      IActivePipeContextAgent (interface)
         An active use of a pipe context as an agent.
      IAsyncPipeContextAgent (interface)
      IPipeContextAgent (interface)
      IPipeContextFactory (interface):  CreateActiveContext, CreateContext
         Used to create the actual context, and the active context usages
      … and 2 more (use --format json for the full surface)
   MassTransit.AmazonS3.MessageData
      AmazonS3MessageDataRepository (class):  AmazonS3MessageDataRepository, 
CreateFaulted, Get, ParseFilePath, PostCreate, PostStart, PostStop, PreStart, 
PreStop, Put, StartFaulted, StopFaulted
   MassTransit.AmazonSqsTransport
      AmazonSqsClientContext (class):  AmazonSqsClientContext, 
ChangeMessageVisibility, CreateQueue, CreateQueueSubscription, CreateTopic, 
DeleteMessage, DeleteQueue, DeleteQueueSubscription, DeleteTopic, GetQueueInfo, 
Publish, PurgeQueue, ReceiveMessages, SendMessage, SubscriptionAttributesEqual
      AmazonSqsConnectionContext (class):  AmazonSqsConnectionContext, 
CreateClientContext, DisposeAsync, GetQueue, GetQueueByName, GetTopic, 
GetTopicByName, RemoveQueueByName, RemoveTopicByName
      AmazonSqsHeaderProvider (class):  AmazonSqsHeaderProvider, GetAll, 
TryGetHeader
      AmazonSqsHost (class):  AmazonSqsHost, ConnectReceiveEndpoint
      AmazonSqsMessageNameFormatter (class):  AmazonSqsMessageNameFormatter, 
CreateMessageName, GetMessageName
      AmazonSqsMessageSendContext (class):  AmazonSqsMessageSendContext, 
ReadPropertiesFrom, WritePropertiesTo
      AmazonSqsPublishTransportProvider (class):  
AmazonSqsPublishTransportProvider, GetPublishTransport
      AmazonSqsReceiveContext (class):  AmazonSqsReceiveContext, 
GetTransportProperties
      AmazonSqsReceiveLockContext (class):  AmazonSqsReceiveLockContext, 
Complete, Faulted, RenewMessageVisibility, ValidateLockStatus
      AmazonSqsSendTransportProvider (class):  AmazonSqsSendTransportProvider, 
GetSendTransport, NormalizeAddress
      AmazonSqsTransportPropertyNames (class)
      AmazonWebServiceResponseExtensions (class):  EnsureSuccessfulResponse
      … and 47 more (use --format json for the full surface)
   MassTransit.AmazonSqsTransport.Configuration
      AmazonSqsBusConfiguration (class):  AmazonSqsBusConfiguration, 
ConnectBusObserver, ConnectEndpointConfigurationObserver
      AmazonSqsBusFactoryConfigurator (class):  AmazonSqsBusFactoryConfigurator,
CreateBusEndpointConfiguration, Host, OverrideDefaultBusEndpointQueueName, 
Publish, ReceiveEndpoint, Send, Validate
      AmazonSqsEndpointConfiguration (class):  AmazonSqsEndpointConfiguration, 
CreateEndpointConfiguration
      AmazonSqsHostConfiguration (class):  AmazonSqsHostConfiguration, 
ApplyEndpointDefinition, Build, CreateReceiveEndpointConfiguration, 
ReceiveEndpoint
      AmazonSqsHostConfigurator (class):  AccessKey, AllowTransportHeader, 
AmazonSqsHostConfigurator, Config, Credentials, EnableScopedTopics, Scope, 
SecretKey, SetBasicCredentials
      AmazonSqsQueueConfigurator (class):  AmazonSqsQueueConfigurator
      AmazonSqsQueueSubscriptionConfigurator (class)
      AmazonSqsReceiveEndpointBuilder (class):  AmazonSqsReceiveEndpointBuilder,
BuildTopology, ConnectConsumePipe, CreateDeadLetterTransport, 
CreateErrorTransport, CreateReceiveEndpointContext
      AmazonSqsReceiveEndpointConfiguration (class):  
AmazonSqsReceiveEndpointConfiguration, Build, ConfigureClient, 
ConfigureConnection, CreateReceiveEndpointContext, 
CreateSqsReceiveEndpointContext, DisableMessageOrdering, FormatInputAddress, 
Subscribe, Validate
      AmazonSqsRegistrationBusFactory (class):  AmazonSqsRegistrationBusFactory,
CreateBus
      AmazonSqsTopicConfigurator (class):  AmazonSqsTopicConfigurator
      AmazonSqsTopicSubscriptionConfigurator (class):  
AmazonSqsTopicSubscriptionConfigurator
      … and 10 more (use --format json for the full surface)
   MassTransit.AmazonSqsTransport.Middleware
      AmazonSqsConsumerFilter (class):  AmazonSqsConsumerFilter, Probe, Send
         A filter that uses the model context to create a basic consumer and 
connect it to the model
      AmazonSqsMessageReceiver (class):  AmazonSqsMessageReceiver, Consume, 
GetQueueAttributes, HandleMessage, ReceiveMessages
         Receives messages from AmazonSQS, pushing them to the InboundPipe of 
the service endpoint.
      ConfigureAmazonSqsTopologyFilter (class):  AnyAutoDelete, Configure, 
ConfigureAmazonSqsTopologyFilter, ConfigureTopology, Declare, Probe, Send
         Configures the broker with the supplied topology once the model is 
created, to ensure that the exchanges, queues, and...
      ConfigureTopologyContext (interface)
      FifoChannelExecutorPool (class):  DisposeAsync, FifoChannelExecutorPool, 
MessageGroupIdProvider, Push, Run
      PurgeOnStartupFilter (class):  Probe, PurgeIfRequested, 
PurgeOnStartupFilter, Send
         Purges the queue on startup, only once per filter instance
      RemoveAmazonSqsTopologyAgent (class):  Delete, DeleteAutoDelete, 
RemoveAmazonSqsTopologyAgent
   MassTransit.AmazonSqsTransport.Topology
      AmazonSnsTopicNameValidator (class):  IsValidEntityName, 
ThrowIfInvalidEntityName
      AmazonSqsBrokerTopology (class):  AmazonSqsBrokerTopology, Probe
      AmazonSqsBusTopology (class):  AmazonSqsBusTopology, 
GetDestinationAddress, GetSendSettings, Publish, Send
      AmazonSqsConsumeTopology (class):  AddSpecification, 
AmazonSqsConsumeTopology, Apply, Bind, GetMessageTopology, Validate
      AmazonSqsEntityNameValidator (class):  IsValidEntityName, 
ThrowIfInvalidEntityName
      AmazonSqsHostEqualityComparer (class):  Equals, GetHashCode
      AmazonSqsMessageConsumeTopology (class):  AmazonSqsMessageConsumeTopology,
Apply, Subscribe, Validate
      AmazonSqsMessagePublishTopology (class):  AmazonSqsMessagePublishTopology,
Apply, GetBrokerTopology, GetEndpointAddress, GetPublishSettings, 
TryGetPublishAddress
      AmazonSqsMessageSendTopology (class)
      AmazonSqsPublishTopology (class):  AmazonSqsPublishTopology, 
GetMessageTopology, GetPublishBrokerTopology
      AmazonSqsSendTopology (class):  AmazonSqsSendTopology, BuildEntityName, 
GetDeadLetterSettings, GetErrorSettings, GetMessageTopology, GetSendSettings
      BrokerTopology (interface)
      … and 30 more (use --format json for the full surface)
   MassTransit.Analyzers
      CommonExpressions (class):  GetAllInterfaces, GetAllTypes, 
GetContractProperties, HasMessageContract, ImplementsInterface, ImplementsType, 
InheritsFromType, InitializeProducerMethods, IsActivator, IsArray, 
IsClassOrInterface, IsCollection, IsDictionary, IsEnumerable, IsImmutableArray
      PropertyNameEqualityComparer (class):  Equals, GetHashCode
   MassTransit.Analyzers.Helpers
      NodeList (class):  Add, Contains, Index, NodeList
      NodeListExtensions (class):  Add
      NodeTable (class):  NodeTable
      OperationExtensions (class):  GetParameters, GetReceiverType, 
IsStaticMember
      TypeConversionHelper (class):  CanConvert, IsMessageData, IsTask, 
TypeConversionHelper
   MassTransit.Audit
      IConsumeMetadataFactory (interface):  CreateAuditMetadata
      IMessageAuditStore (interface):  StoreMessage
         Used to store message audits that are observed
      ISendMetadataFactory (interface):  CreateAuditMetadata
      MessageAuditMetadata (class)
   MassTransit.Audit.MetadataFactories
      DefaultConsumeMetadataFactory (class):  CreateAuditMetadata, 
CreateMetadata
      DefaultSendMetadataFactory (class):  CreateAuditMetadata, CreateMetadata
   MassTransit.Audit.Observers
      AuditConsumeObserver (class):  AuditConsumeObserver, ConsumeFault, 
PostConsume, PreConsume
      AuditPublishObserver (class):  AuditPublishObserver, PostPublish, 
PrePublish, PublishFault
      AuditSendObserver (class):  AuditSendObserver, PostSend, PreSend, 
SendFault
   MassTransit.AzureCosmos
      AzureCosmosSerializerExtensions (class):  GetSagaRenameSettings, 
GetSerializerOptions
      CosmosAuthSettings (class):  CosmosAuthSettings
      CosmosClientFactory (class):  CreateClient
      NewtonsoftJsonCosmosClientFactory (class):  CreateClient, Dispose, 
GetCosmosClient, GetSerializerSettingsIfNeeded, 
NewtonsoftJsonCosmosClientFactory
         Creates a single instance of the using a fixed endpoint and key for all
clients, regardless of client name.
      SagaRenamePropertyNamingPolicy (class):  ConvertName, 
SagaRenamePropertyNamingPolicy
      SystemTextJsonCosmosClientFactory (class):  CreateClient, Dispose, 
GetCosmosClient, GetSerializerOptions, SystemTextJsonCosmosClientFactory
         Creates a single instance of the using a fixed endpoint and key for all
clients, regardless of client name.
   MassTransit.AzureCosmos.Saga
      CosmosDatabaseContext (class):  CosmosDatabaseContext, 
GetItemRequestOptions, GetLinqSerializerOptions
      CosmosSagaRepository (class):  Create
      CosmosSagaRepositoryContext (class):  Add, CosmosSagaRepositoryContext, 
CreateSagaConsumeContext, Delete, Discard, GetItemRequestOptions, Insert, Load, 
Query, Save, Undo, Update
      CosmosSagaRepositoryContextFactory (class):  
CosmosSagaRepositoryContextFactory, Execute, ExecuteAsyncMethod, Probe, Send, 
SendQuery
      DatabaseContext (interface):  GetItemRequestOptions, 
GetLinqSerializerOptions
      DefaultCollectionIdFormatter (class):  DefaultCollectionIdFormatter, Saga
      KebabCaseCollectionIdFormatter (class):  KebabCaseCollectionIdFormatter, 
Saga
         Formats the saga collection names using kebab case naming SimpleSaga ->
simple-saga
      NewtonsoftJsonCosmosSerializer (class):  FromStream, 
NewtonsoftJsonCosmosSerializer, ToStream
         The default Cosmos JSON.NET serializer.
      PropertyRenameSerializerContractResolver (class):  IsRenamed, 
PropertyRenameSerializerContractResolver, RenameProperty
      QueryableExtensions (class):  QueryAsync
      SagaETag (class):  SagaETag
      SystemTextJsonCosmosSerializer (class):  FromStream, 
SystemTextJsonCosmosSerializer, ToStream
         The default serializer, using System.Text.Json
   MassTransit.AzureServiceBusTransport
      AzureServiceBusSendContext (class):  AzureServiceBusSendContext, 
ReadPropertiesFrom, SetScheduledMessageId, TryGetScheduledMessageId, 
TryGetSequenceNumber, WritePropertiesTo
      AzureServiceBusTransportPropertyNames (class)
      ClientContext (interface):  CloseAsync, NotifyFaulted, OnMessageAsync, 
OnSessionAsync, ShutdownAsync, StartAsync
         The client context is used to access the queue/subscription/topic 
client.
      ClientContextFactory (class):  CreateActiveContext, CreateClientContext, 
CreateContext, CreateSharedContext
      ClientContextSupervisor (class):  ClientContextSupervisor
      ClientSettings (interface):  GetInputAddress
      ConnectionContext (interface):  CreateMessageSender, CreateQueue, 
CreateQueueProcessor, CreateQueueSessionProcessor, CreateSubscriptionProcessor, 
CreateSubscriptionSessionProcessor, CreateTopic, CreateTopicSubscription, 
DeleteTopicSubscription
         Service Bus Connection Context
      ConnectionContextFactory (class):  ConnectionContextFactory, 
CreateActiveContext, CreateConnection, CreateContext, CreateSharedConnection, 
HasSharedAccess
      ConnectionContextSupervisor (class):  ConnectionContextSupervisor, 
CreateClientContextSupervisor, CreatePublishTransport, 
CreateSendEndpointContextSupervisor, CreateSendTransport, NormalizeAddress
      Defaults (class):  GetCreateQueueOptions, GetCreateTopicOptions
      DelegateSessionIdFormatter (class):  DelegateSessionIdFormatter, 
FormatSessionId
      EmptySessionIdFormatter (class):  FormatSessionId
      … and 55 more (use --format json for the full surface)
   MassTransit.AzureServiceBusTransport.Configuration
      Factory (class):  Create
      HostSettings (class):  HostSettings
      IServiceBusBusConfiguration (interface):  CreateEndpointConfiguration
      IServiceBusConsumeTopologySpecification (interface):  Apply
      IServiceBusEndpointConfiguration (interface)
      IServiceBusEntityEndpointConfiguration (interface):  Build
      IServiceBusHostConfiguration (interface):  ApplyEndpointDefinition, 
CreateReceiveEndpointConfiguration, CreateSubscriptionEndpointConfiguration, 
SetNamespaceSeparatorTo, SetNamespaceSeparatorToTilde, 
SetNamespaceSeparatorToUnderscore, SubscriptionEndpoint
      IServiceBusReceiveEndpointConfiguration (interface)
      IServiceBusSubscriptionEndpointConfiguration (interface)
      IServiceBusTopologyConfiguration (interface)
      ISessionIdMessageSendTopologyConvention (interface):  SetFormatter
      ISessionIdSendTopologyConvention (interface)
      … and 21 more (use --format json for the full surface)
   MassTransit.AzureServiceBusTransport.Middleware
      ConfigureServiceBusTopologyFilter (class):  Configure, 
ConfigureServiceBusTopologyFilter, ConfigureTopology, Create, Delete, Probe, 
Send
      ConfigureTopologyContext (interface)
      DeadLetterQueueExceptionFilter (class):  Probe, Send
         Moves a faulted message to the dead-letter queue, rather than the 
_error queue
      DeadLetterQueueFilter (class):  Probe, Send
         Moves a message to the dead-letter queue, rather than the _skipped 
queue
      MessageReceiverFilter (class):  MessageReceiverFilter, Probe, Send
         Creates a message receiver and receives messages from the input queue 
of the endpoint
      MessageSessionReceiverFilter (class):  MessageSessionReceiverFilter, Probe
         Creates a message session receiver
      RemoveServiceBusTopologyAgent (class):  Delete, 
RemoveServiceBusTopologyAgent, RemoveSubscriptions
      ServiceBusMessageSchedulerFilter (class):  Probe, SchedulerFactory, Send
         Adds the service bus message scheduler filter
      ServiceBusSendContextFilter (class):  Probe, Send, 
ServiceBusSendContextFilter
      SetSessionIdFilter (class):  Probe, Send, SetSessionIdFilter
   MassTransit.AzureServiceBusTransport.Topology
      BaseClientSettings (class):  GetInputAddress
      BrokerTopology (interface)
      BrokerTopologyBuilder (class):  BrokerTopologyBuilder, 
BuildBrokerTopology, CreateQueue, CreateQueueSubscription, CreateSubscription, 
CreateTopic, CreateTopicSubscription, GetNextId
      Cached (class)
      EntityNameFormatter (class):  FormatDeadLetterPath, FormatErrorPath, 
FormatSubQueuePath, FormatSubscriptionPath
      IBrokerTopologyBuilder (interface):  CreateQueue, CreateQueueSubscription,
CreateSubscription, CreateTopic, CreateTopicSubscription
      IPublishEndpointBrokerTopologyBuilder (interface):  
CreateImplementedBuilder
         A builder for creating the topology when publishing a message
      IReceiveEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so 
that the items added by it can be combined toge...
      ISendEndpointBrokerTopologyBuilder (interface)
         A builder for creating the topology when publishing a message
      ISubscriptionEndpointBrokerTopologyBuilder (interface)
         A unique builder context should be created for each specification, so 
that the items added by it can be combined toge...
      ImplementedBuilder (class):  CreateImplementedBuilder, CreateQueue, 
CreateQueueSubscription, CreateSubscription, CreateTopic, 
CreateTopicSubscription, ImplementedBuilder
      ImplementedMessageTypeConnector (class):  ImplementedMessageTypeConnector,
ImplementsMessageType
      … and 41 more (use --format json for the full surface)
   MassTransit.AzureStorage.MessageData
      AzureStorageMessageDataRepository (class):  
AzureStorageMessageDataRepository, CreateFaulted, Get, PostCreate, PostStart, 
PostStop, PreStart, PreStop, Put, SetBlobExpiration, StartFaulted, StopFaulted
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
   … and 143 more namespaces (use --format json for the full surface)
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
      AllConsumer  → AllConsumer  
(src/MassTransit.SignalR/Consumers/AllConsumer.cs:12)
      BatchConsumer  → BatchConsumer  
(src/MassTransit/Consumers/Batching/BatchConsumer.cs:14)
      CancelScheduledMessageConsumer  → CancelScheduledMessageConsumer  
(src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/CancelScheduledM
essageConsumer.cs:8)
      ConnectionConsumer  → ConnectionConsumer  
(src/MassTransit.SignalR/Consumers/ConnectionConsumer.cs:11)
      FinalizeJobConsumer  → FinalizeJobConsumer  
(src/MassTransit/JobService/JobService/FinalizeJobConsumer.cs:10)
      GroupConsumer  → GroupConsumer  
(src/MassTransit.SignalR/Consumers/GroupConsumer.cs:12)
      GroupManagementConsumer  → GroupManagementConsumer  
(src/MassTransit.SignalR/Consumers/GroupManagementConsumer.cs:8)
      MediatorRequestHandler  → MediatorRequestHandler  
(src/MassTransit.Abstractions/Mediator/MediatorRequestHandler.cs:31)
      MessageHandlerConsumer  → MessageHandlerConsumer  
(src/MassTransit/DependencyInjection/DependencyInjection/MessageHandlerConsumer.
cs:72)
      PauseScheduledMessageConsumer  → PauseScheduledMessageConsumer  
(src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/PauseScheduledMe
ssageConsumer.cs:8)
      PauseScheduledRecurringMessageConsumer  → 
PauseScheduledRecurringMessageConsumer  
(src/Scheduling/MassTransit.HangfireIntegration/HangfireIntegration/PauseSchedul
edRecurringMessageConsumer.cs:10)
      RequestHandlerConsumer  → RequestHandlerConsumer  
(src/MassTransit/DependencyInjection/DependencyInjection/RequestHandlerConsumer.
cs:82)
      ResumeScheduledMessageConsumer  → ResumeScheduledMessageConsumer  
(src/Scheduling/MassTransit.QuartzIntegration/QuartzIntegration/ResumeScheduledM
essageConsumer.cs:8)
      ResumeScheduledRecurringMessageConsumer  → 
ResumeScheduledRecurringMessageConsumer  
(src/Scheduling/MassTransit.HangfireIntegration/HangfireIntegration/ResumeSchedu
ledMessageConsumer.cs:10)
      RoutingSlipActivityCompensatedConsumer  → 
RoutingSlipActivityCompensatedConsumer  
(src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consu
mers/RoutingSlipActivityCompensatedConsumer.cs:8)
      RoutingSlipActivityCompensationFailedConsumer  → 
RoutingSlipActivityCompensationFailedConsumer  
(src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consu
mers/RoutingSlipActivityCompensationFailedConsumer.cs:8)
      RoutingSlipActivityCompletedConsumer  → 
RoutingSlipActivityCompletedConsumer  
(src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consu
mers/RoutingSlipActivityCompletedConsumer.cs:8)
      RoutingSlipActivityFaultedConsumer  → RoutingSlipActivityFaultedConsumer  
(src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consu
mers/RoutingSlipActivityFaultedConsumer.cs:8)
      RoutingSlipCompensationFailedConsumer  → 
RoutingSlipCompensationFailedConsumer  
(src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consu
mers/RoutingSlipCompensationFailedConsumer.cs:8)
      RoutingSlipCompletedConsumer  → RoutingSlipCompletedConsumer  
(src/Persistence/MassTransit.MongoDbIntegration/MongoDbIntegration/Courier/Consu
mers/RoutingSlipCompletedConsumer.cs:8)
      … and 11 more (bus entries)
   Background (8)
      AzureServiceBusTestHarnessHostedService  → 
AzureServiceBusTestHarnessHostedService  
(src/Transports/MassTransit.Azure.ServiceBus.Core/Configuration/ServiceBusDepend
encyInjectionTestingExtensions.cs:31)
      BusOutboxDeliveryService  → BusOutboxDeliveryService  
(src/Persistence/MassTransit.MongoDbIntegration/Configuration/Configuration/Mong
oDbBusOutboxConfigurator.cs:50)
      InboxCleanupService  → InboxCleanupService  
(src/Persistence/MassTransit.MongoDbIntegration/Configuration/Configuration/Mong
oDbOutboxConfigurator.cs:59)
      KafkaTestHarnessHostedService  → KafkaTestHarnessHostedService  
(src/Transports/MassTransit.KafkaIntegration/Configuration/KafkaDependencyInject
ionTestingExtensions.cs:43)
      RabbitMqTestHarnessHostedService  → RabbitMqTestHarnessHostedService  
(src/Transports/MassTransit.RabbitMqTransport/Configuration/RabbitMqDependencyIn
jectionTestingExtensions.cs:31)
      SqlTransportMigrationHostedService  → SqlTransportMigrationHostedService  
(src/Transports/MassTransit.SqlTransport.SqlServer/Configuration/SqlServerDbTran
sportConfigurationExtensions.cs:39)
      BusOutboxDeliveryService<TDbContext>  
(src/Persistence/MassTransit.EntityFrameworkCoreIntegration/Configuration/Config
uration/EntityFrameworkBusOutboxConfigurator.cs:52)
      InboxCleanupService<TDbContext>  
(src/Persistence/MassTransit.EntityFrameworkCoreIntegration/Configuration/Config
uration/EntityFrameworkOutboxConfigurator.cs:74)

PACKAGES
   Web/API:  Microsoft.ApplicationInsights.AspNetCore 2.23.0, 
Microsoft.AspNetCore.SignalR.Core 1.1.0
   ORM/Data:  Dapper 2.1.79, dapper.contrib 2.0.78, EntityFramework 6.5.2, 
Microsoft.Azure.Cosmos 3.61.0, Microsoft.EntityFrameworkCore.Relational 9.0.1, 
Npgsql 8.0.7
   Messaging:  Apache.NMS.AMQP 2.4.0, Azure.Messaging.EventHubs.Processor 
5.12.2, Azure.Messaging.ServiceBus 7.20.1, RabbitMQ.Client 7.2.1
   Cloud:  Azure.Data.Tables 12.11.0, Azure.Identity 1.21.0, Azure.Storage.Blobs
12.28.0, Microsoft.Azure.WebJobs.Extensions.EventHubs 6.5.3, 
Microsoft.Azure.WebJobs.Extensions.ServiceBus 5.17.0
   Utilities:  Newtonsoft.Json 13.0.4, Newtonsoft.Json.Bson 1.0.3
   Other:  Apache.NMS.ActiveMQ 2.2.0, AWSSDK.DynamoDBv2 4.0.17.7, AWSSDK.S3 
4.0.20.4, AWSSDK.SimpleNotificationService 4.0.2.26, AWSSDK.SQS 4.0.2.24, 
Confluent.Kafka 2.14.2, Hangfire.Core 1.8.23, Iesi.Collections 4.1.1 … (36 
total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus Activity)

analyzed 5502 files · 13930 nodes · 13560 edges · 39 entries · 37/39 →target · 
~8305 tokens · 56.3s stage2 ×3.0 stage3 ×2.2
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │   MassTransit.sln    │
│   Time   │       56914ms        │
│  Tokens  │ ~8305 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.262 │
╰──────────┴──────────────────────╯
