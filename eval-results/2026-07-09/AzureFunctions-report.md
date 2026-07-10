# REPORT
**DotNetWorker.Extensions**

Style: Unknown
_22 projects  ·  net10.0, net48 + azure-servicebus + azure-functions + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 857 |
| Projects | 66 |
| Nodes | 818 |
| Edges | 561 |
| Entries | 0 |
| With target | 0/0 |
| Verified edges | 48% |
| Analyzed in | 17.8s |

## Top Flows

_No entries found._

## Insights

_4 info · 2 notable_

### **NOTABLE**: Config without defaults: 3 consumed keys have no appsettings default
*(Risk)*

- AzureWebJobsEventHubSender
- ENVIRONMENT
- FunctionAppUrl

### **NOTABLE**: Extension seats: AddOptions (10 impls) · AddFunctionsWorkerCore (3 impls) · AddHostedService (2 impls)
*(Wiring)*

- AddOptions (10 impls)
- AddFunctionsWorkerCore (3 impls)
- AddHostedService (2 impls)

### _INFO_: DI: 55 Extension · 28 Singleton · 1 Scoped · 1 Transient (85 total)
*(Wiring)*

### _INFO_: Public surface: 26 interfaces, 513 classes (550 total public types)
*(Shape)*

- 26 interfaces
- 513 classes

### _INFO_: Most depended-upon: Worker.Extensions.Abstractions (16 dependents) · DotNetWorker (5 dependents) · DotNetWorker.Core (4 dependents)
*(Topology)*

- Worker.Extensions.Abstractions (16 dependents)
- DotNetWorker (5 dependents)
- DotNetWorker.Core (4 dependents)

### _INFO_: Wiring hubs: FunctionContext (100) · IFunctionsWorkerApplicationBuilder (25) · ResolveExtensionCopyLocal (16) · FunctionsResourceDetector (15) · IServiceCollection (13)
*(Wiring)*

- FunctionContext (100)
- IFunctionsWorkerApplicationBuilder (25)
- ResolveExtensionCopyLocal (16)
- FunctionsResourceDetector (15)
- IServiceCollection (13)

LIBRARY  DotNetWorker.Extensions     (183 public types)

ENTRY API
   annotate  [Binding]   (BindingAttribute.cs)
   annotate  [BindingCapabilities]   (BindingCapabilitiesAttribute.cs)
      Specifies a binding type's capabilities.
   annotate  [BindingPropertyName]   (BindingPropertyNameAttribute.cs)
      Specifies the binding property name that is used when generating function metadata.
   annotate  [BlobInput]   (BlobInputAttribute.cs)
   annotate  [BlobOutput]   (BlobOutputAttribute.cs)
   annotate  [BlobTrigger]   (BlobTriggerAttribute.cs)
   annotate  [ConverterFallbackBehavior]   (ConverterFallbackBehaviorAttribute.cs)
      An attribute that specifies if converter fallback is allowed or disallowed.
   annotate  [CosmosDBInput]   (CosmosDBInputAttribute.cs)
   annotate  [CosmosDBOutput]   (CosmosDBOutputAttribute.cs)
   annotate  [CosmosDBTrigger]   (CosmosDBTriggerAttribute.cs)
   annotate  [DefaultValue]   (DefaultValueAttribute.cs)
   annotate  [EventGridOutput]   (EventGridOutputAttribute.cs)

ABSTRACTIONS
   IInputConverter (interface)  — 25 implementors
   OutputBindingAttribute (class)  — 12 implementors
   TriggerBindingAttribute (class)  — 12 implementors
   IFunctionsWorkerMiddleware (interface)  — 7 implementors
   InputBindingAttribute (class)  — 6 implementors
   WorkerExtensionStartup (class)  — 5 implementors
   FunctionDefinition (class)  — 4 implementors
   BindingAttribute (class)  — 3 implementors
   FunctionContext (class)  — 3 implementors
   HttpRequestData (class)  — 3 implementors

GENERATORS
   generator   ExtensionStartupRunnerGenerator
      Generates a class with a method which has code to call the "Configure" method of each of the participating extension'...
   generator   FunctionExecutorGenerator
   generator   FunctionMetadataProviderGenerator
   analyzer    AsyncVoidAnalyzer
   analyzer    BindingTypeNotSupported
   analyzer    DeferredBindingAttributeNotSupported
   analyzer    HttpResultAttributeExpectedAnalyzer
   analyzer    IterableBindingTypeExpectedForBlobContainerPath
   analyzer    RegistrationExpectedInASPNetIntegration
      Analyzer to verify whether expected registration is present for ASP.NET Core Integration.
   analyzer    WebJobsAttributesNotSupported
   code-fixer  AsyncVoidCodeFixProvider
   code-fixer  CodeFixForHttpResultAttribute
   code-fixer  CodeFixForRegistrationInASPNetCoreIntegration

PUBLIC SURFACE
   Azure.Functions.Sdk
      ExtensionReference (class):  TryGetFromModule
         Represents an Azure Functions extension reference.
      FunctionsAssemblyResolver (class):  Resolve
         An assembly resolver that skips trusted platform assemblies.
      FunctionsAssemblyScanner (class):  FromTaskItems, FunctionsAssemblyScanner, GetWebJobsReferences, ShouldScanPackage
         Class to help with scanning functions related assemblies.
      LockFileExtensions (class)
         Extensions for NuGet lock files.
      MonoExtensions (class):  CheckTypeInheritance, GetReflectionFullName
         Extensions for Mono.Cecil types.
      TaskItemExtensions (class)
         Extensions for .
      WebJobsReference (class):  FromModule
         Represents an Azure Functions WebJobs reference.
   Azure.Functions.Sdk.Tasks
      FuncSdkLog (class):  Execute
      GenerateWorkerConfig (class):  Execute, GenerateWorkerConfig
         Generates a worker config file for dotnet-isolated functions.
   Azure.Functions.Sdk.Tasks.Extensions
      ResolveExtensionPackages (class):  Cancel, Dispose, Execute, ResolveExtensionPackages
      ValidateExtensionPackages (class):  Execute
      WriteExtensionMetadata (class):  Cancel, Dispose, Execute, WriteExtensionMetadata
      WriteExtensionProject (class):  Execute, WriteExtensionProject
   Azure.Functions.Sdk.Tasks.Inner
      ResolveExtensionCopyLocal (class):  Execute
         Resolves the set of extension assemblies that should be copied locally, excluding those that are part of the Azure Fu...
   Azure.Functions.Sdk.Tasks.Publish
      CreateFuncZipFile (class):  CreateFuncZipFile, Execute
         Creates a zip file for a folder.
      ZipDeploy (class):  Cancel, Dispose, Execute, ZipDeploy
   Azure.Functions.Sdk.ZipDeploy
      DeploymentClient (class):  ZipDeployAsync
      DeploymentException (class):  DeploymentException
      StatusResult (class):  ParseAsync, ThrowFailure
      ZipDeployRequest (record):  CreateRequestMessage, GetUri
   Microsoft.Azure.Functions.Worker
      BindingContext (class)
         Exposes binding infomation for a given function context.
      BindingMetadata (class)
         Contains metadata about an Azure Functions binding.
      BlobInputAttribute (class):  BlobInputAttribute
      BlobOutputAttribute (class):  BlobOutputAttribute
      BlobStorageExtensionStartup (class):  Configure
      BlobTriggerAttribute (class):  BlobTriggerAttribute
      CosmosDBExtensionOptions (class)
      CosmosDBInputAttribute (class):  CosmosDBInputAttribute
      CosmosDBOutputAttribute (class):  CosmosDBOutputAttribute
      CosmosDBTriggerAttribute (class):  CosmosDBTriggerAttribute
      CosmosExtensionStartup (class):  Configure
      EventGridOutputAttribute (class):  EventGridOutputAttribute
      EventGridTriggerAttribute (class)
      EventHubOutputAttribute (class):  EventHubOutputAttribute
         Attribute used to specify that the attribute target should output its data to Event Hubs.
      EventHubTriggerAttribute (class):  EventHubTriggerAttribute
         Attribute used to mark a function that should be triggered by Event Hubs messages.
      ExponentialBackoffRetryAttribute (class):  ExponentialBackoffRetryAttribute
         Defines an exponential backoff retry strategy, where the delay between retries will get progressively larger, limited...
      FixedDelayRetryAttribute (class):  FixedDelayRetryAttribute
         Defines a retry strategy where a fixed delay is used between retries.
      FunctionAttribute (class):  FunctionAttribute
      FunctionContext (class)
         Encapsulates the information about a function execution.
      FunctionContextBindingFeatureExtensions (class):  BindInputAsync, GetInvocationResult, GetOutputBindings
         FunctionContext extension methods for binding data.
      FunctionContextExtensions (class):  GetHttpContext
         Provides extension methods to work with .
      FunctionContextHttpRequestExtensions (class):  GetHttpRequestDataAsync, GetHttpResponseData
         FunctionContext extensions for http trigger function invocations.
      FunctionContextLoggerExtensions (class):  GetLogger
         extensions for .
      FunctionDefinition (class)
         A representation of a function.
      FunctionInputConverterException (class):  FunctionInputConverterException
         The exception that is thrown when input conversion for the function invocation fails.
      FunctionInvocation (class)
         A representation of a single function invocation.
      FunctionParameter (class):  FunctionParameter
         Represents a parameter defined by the target function.
      FunctionWorkerException (class):  FunctionWorkerException
         The exception that is thrown when an error occurs during function invocation in the .NET isolated model.
      FunctionsApplicationInsightsExtensions (class):  ConfigureFunctionsApplicationInsights
      FunctionsDebugger (class):  Enable
         Isolated debugging utility.
      FunctionsWorkerApplicationBuilderExtensions (class):  ConfigureBlobStorageExtension, ConfigureCosmosDBExtension, ConfigureCosmosDBExtensionOptions, ConfigureTablesExtension, UseCosmosDBWorkerSerializer, UseWorkerSerializer
         Provides extension methods to work with a .
      FunctionsWorkerHost (class):  RunDefault, RunDefaultAsync
         Hosting helpers to work with Azure Functions Workers
      GrpcHttpClientBuilderExtensions (class):  ConfigureForFunctionsHostGrpc
         Extensions for registering RPC services to communicate with the functions host.
      HttpResultAttribute (class)
         Attribute used to mark an HTTP Response on an HTTP Trigger function with multiple output bindings.
      HttpTriggerAttribute (class):  HttpTriggerAttribute
         Attribute used to mark a function that should be invoked over HTTP.
      IAutoConfigureStartup (interface):  Configure
         Represents an interface for auto-configuration of an .
      IFunctionActivator (interface):  CreateInstance
         Defines a service that will be used to create function class instances.
      IFunctionsWorkerApplicationBuilder (interface):  Use
         Represents a builder for a Functions Worker Application.
      IInvocationFeatures (interface):  Get, Set
         Represents a collection of invocation features.
      InputBindingData (class)
         A type representing the input binding data.
      InputConverterCollection (class):  Clear, GetEnumerator, Register, RegisterAt
         A collection of input converters.
      InvocationResult (class)
         A type wrapping the result of a function invocation.
      KafkaHeader (class):  GetValueAsString
         Represents a single Kafka record header (key-value pair where value is raw bytes).
      KafkaOutputAttribute (class):  KafkaOutputAttribute
      KafkaRecord (class)
         Represents a raw Apache Kafka record with full metadata.
      KafkaTimestamp (class)
         Represents the timestamp of a Kafka record.
      KafkaTriggerAttribute (class):  KafkaTriggerAttribute
      OutputBindingData (class)
         A type representing an output binding entry.
      QueueOutputAttribute (class):  QueueOutputAttribute
      QueueTriggerAttribute (class):  QueueTriggerAttribute
      RabbitMQOutputAttribute (class):  RabbitMQOutputAttribute
      RabbitMQTriggerAttribute (class):  RabbitMQTriggerAttribute
      RetryAttribute (class):  RetryAttribute
      RetryContext (class)
         Exposes information about retry activity for the event that triggered the current function invocation.
      ScheduleStatus (class)
         Represents a timer schedule status.
      SendGridOutputAttribute (class):  SendGridOutputAttribute
         Initializes a new instance of the class.
      ServiceBusExtensionStartup (class):  Configure
      ServiceBusMessageActions (class):  AbandonMessageAsync, CompleteMessageAsync, DeadLetterMessageAsync, DeferMessageAsync, RenewMessageLockAsync
      ServiceBusOutputAttribute (class):  ServiceBusOutputAttribute
      ServiceBusSessionMessageActions (class):  GetSessionStateAsync, ReleaseSession, RenewSessionLockAsync, SetSessionStateAsync
         Converter to bind to type parameters.
      ServiceBusTriggerAttribute (class):  ServiceBusTriggerAttribute
      SignalRConnectionInfo (class)
         Contains necessary information for a SignalR client to connect to SignalR Service.
      SignalRConnectionInfoInputAttribute (class)
         Provides for a SignalR client to connect to SignalR Service.
      SignalREndpoint (class)
         Represents an endpoint of SignalR Service.
      SignalREndpointConnectionInfo (class)
         Contains metadata of an SignalR endpoint and connection information for a client to connect to the SignalR Service.
      SignalREndpointsInputAttribute (class):  SignalREndpointsInputAttribute
         Binds a list of to the parameter.
      SignalRGroupAction (class):  SignalRGroupAction
         An action to manage a group.
      SignalRInvocationContext (class)
         The context for a SignalR invocation.
      SignalRMessageAction (class):  SignalRMessageAction
         An action to send a message to SignalR clients.
      SignalRNegotiationContext (class)
         The context of SignalR client negotiation.
      SignalRNegotiationInputAttribute (class):  SignalRNegotiationInputAttribute
         Provides to a parameter, which provides information to choose an available SignalR endpoint and corresponding connect...
      SignalROutputAttribute (class):  SignalROutputAttribute
         Binds to a to send a message.
      SignalRTriggerAttribute (class):  SignalRTriggerAttribute
         Binds to a to mark a function that should be triggered by messages sent from SignalR clients.
      TableExtensionStartup (class):  Configure
         Table extension startup.
      TableInputAttribute (class):  TableInputAttribute
         Attribute used to configure a parameter as the input target for the Azure Storage Tables binding.
      TableOutputAttribute (class):  TableOutputAttribute
         Attribute used to configure the source of the Azure Storage Tables output binding.
      TimerInfo (class)
         Provides access to timer schedule information for jobs triggered by .
      TimerTriggerAttribute (class):  TimerTriggerAttribute
         Attribute used to mark a function that should be invoked periodically based on a timer schedule.
      TraceContext (class)
         The trace context for the current invocation.
      WarmupTrigger (class)
         Attribute used to mark a function that should be invoked during the warmup stage of the Function App.
      WorkerOptions (class)
         An options class for configuring the worker.
   Microsoft.Azure.Functions.Worker.Builder
      FunctionsApplication (class):  CreateBuilder
         The application used to configure an Azure Functions worker.
      FunctionsApplicationBuilder (class):  Build, ConfigureContainer, Use
         A builder for Azure Functions worker applications.
      FunctionsApplicationBuilderAspNetCoreExtensions (class):  ConfigureFunctionsWebApplication
         ASP.NET Core extensions for .
   Microsoft.Azure.Functions.Worker.Context.Features
      FunctionInputBindingResult (class):  FunctionInputBindingResult
         A type representing the result of binding function inputs.
      IFunctionInputBindingFeature (interface):  BindFunctionInputAsync
         Provides a mechanism to bind function inputs.
   Microsoft.Azure.Functions.Worker.Converters
      ConversionResult (struct):  Failed, Success, Unhandled
         A type representing the result of function input conversion operation.
      ConverterContext (class)
         A type defining the information needed for an input conversion operation.
      ConverterContextExtensions (class):  TryGetBindingAttribute
         Provides extension methods to work with an instance.
      ConverterFallbackBehaviorAttribute (class):  ConverterFallbackBehaviorAttribute
         An attribute that specifies if converter fallback is allowed or disallowed.
      IInputConverter (interface):  ConvertAsync
         Provides a mechanism for input conversion.
      InputConverterAttribute (class):  InputConverterAttribute
         An attribute that can specify a type of to use for function input conversion.
      SupportedTargetTypeAttribute (class):  SupportedTargetTypeAttribute
         An attribute that can specify a target type supported by function input conversion.
   Microsoft.Azure.Functions.Worker.Core
      CollectionModelBindingData (class)
         A representation of Collection of Microsoft.Azure.WebJobs.ParameterBindingData
      ModelBindingData (class)
         A representation of a Microsoft.Azure.WebJobs.ParameterBindingData.
      WorkerExtensionStartup (class):  Configure
         The base type for worker extension startup implementations.
      WorkerExtensionStartupAttribute (class):  WorkerExtensionStartupAttribute
         An assembly level attribute to inform that this assembly contains a worker extension startup implementation type.
      WorkerExtensionStartupCodeExecutorInfoAttribute (class):  WorkerExtensionStartupCodeExecutorInfoAttribute
         An assembly level attribute to inform that this assembly contains information about extension startup code executor t...
   Microsoft.Azure.Functions.Worker.Core.FunctionMetadata
      DefaultFunctionMetadata (class)
         Local representation of FunctionMetadata
      DefaultRetryOptions (class)
         Function execution retry policy to use on invocation failures.
      IFunctionMetadata (interface)
         Public interface that represents properties in function metadata.
      IFunctionMetadataManager (interface):  GetFunctionMetadataAsync
         Manages function metadata, providing functionality that combines metadata from the registered provider and metadata t...
      IFunctionMetadataProvider (interface):  GetFunctionMetadataAsync
         Returns function metadata information from an app.
      IFunctionMetadataTransformer (interface):  Transform
         Defines a contract for transforming instances for Azure Functions.
      IRetryOptions (interface)
         Function execution retry policy to use on invocation failures.
   Microsoft.Azure.Functions.Worker.Core.Invocation
      FunctionActivatorExtensions (class):  CreateInstance
         Contains extension methods to work with instances.
   Microsoft.Azure.Functions.Worker.Diagnostics
      ActivityAttributes (class)
      CapabilityFlags (class)
      ExceptionAttributes (class)
      FunctionContextKeys (class)
      IBaggagePropagator (interface):  SetBaggage
         Propagates baggage items within the current context.
      InternalKeys (class)
      KnownAttributes (class)
      OTelAttributes_1_17_0 (class)
      OTelAttributes_1_37_0 (class)
   Microsoft.Azure.Functions.Worker.Extensions.Abstractions
      BindingAttribute (class):  BindingAttribute
      BindingCapabilitiesAttribute (class):  BindingCapabilitiesAttribute
         Specifies a binding type's capabilities.
      BindingPropertyNameAttribute (class):  BindingPropertyNameAttribute
         Specifies the binding property name that is used when generating function metadata.
      DefaultValueAttribute (class):  DefaultValueAttribute
      ExtensionInformationAttribute (class):  ExtensionInformationAttribute
      ISupportCardinality (interface)
      InputBindingAttribute (class):  InputBindingAttribute
      KnownBindingCapabilities (class)
         Class containing constant values representing known binding capabilities.
      OutputBindingAttribute (class):  OutputBindingAttribute
      SupportsDeferredBindingAttribute (class)
         Specifies if a converter supports deferred binding when generating function metadata.
      TriggerBindingAttribute (class):  TriggerBindingAttribute
   Microsoft.Azure.Functions.Worker.Extensions.Http.Converters
      IFromBodyConversionFeature (interface):  ConvertAsync
         Defines an interface for model binding conversion used when binding to the body of an HTTP request.
   Microsoft.Azure.Functions.Worker.Extensions.Rpc
      FunctionsGrpcOptions (class)
         Contains options for the functions gRPC invocation.
      RpcServiceCollectionExtensions (class):  AddWorkerRpc
         extensions for RPC.
   Microsoft.Azure.Functions.Worker.Http
      FromBodyAttribute (class):  FromBodyAttribute
         Specifies that a parameter should be bound using the HTTP request body when using the .
      HttpCookie (class):  HttpCookie
         Http response cookie object
      HttpCookies (class):  Append, CreateNew
         Provides functionality to help interact with HTTP cookies.
      HttpHeadersCollection (class):  HttpHeadersCollection
         A collection of HTTP Headers
      HttpRequestData (class):  CreateResponse, HttpRequestData
         A representation of the HTTP request sent by the host.
      HttpRequestDataExtensions (class):  CreateResponse, ReadAsString, ReadAsStringAsync, ReadFromJsonAsync
         Provides extension methods to work with an instance.
      HttpResponseData (class):  CreateResponse, HttpResponseData
         A representation of the outgoing HTTP response.
      HttpResponseDataExtensions (class):  WriteAsJsonAsync, WriteBytes, WriteBytesAsync, WriteString, WriteStringAsync
         Provides extension methods to work with an instance.
      IHttpCookie (interface)
         A representation of an HTTP cookie
      IHttpRequestDataFeature (interface):  GetHttpRequestDataAsync
         A representation of the HTTP request sent by the host
   Microsoft.Azure.Functions.Worker.Invocation
      IFunctionExecutor (interface):  ExecuteAsync
         Provides a mechanism to execute function code.
   Microsoft.Azure.Functions.Worker.Middleware
      IFunctionsWorkerMiddleware (interface):  Invoke
         Represents a middleware to be used in the worker execution pipeline.
   Microsoft.Azure.Functions.Worker.OpenTelemetry
      ConfigureFunctionsOpenTelemetry (class):  UseFunctionsWorkerDefaults
      FunctionsResourceDetector (class):  Detect
   Microsoft.Azure.Functions.Worker.Sdk
      CustomAttributeExtensions (class):  GetAllDefinedProperties
      ExtensionReference (class)
      ExtensionsMetadata (class)
   Microsoft.Azure.Functions.Worker.Sdk.Analyzers
      BindingTypeCodeRefactoringProvider (class):  ComputeRefactoringsAsync
   Microsoft.Azure.Functions.Worker.Sdk.Tasks
      EnhanceExtensionsMetadata (class):  Execute
      GenerateFunctionMetadata (class):  Execute
   Microsoft.Azure.Functions.Worker.SignalRService
      ServerlessHub (class)
   Microsoft.Extensions.DependencyInjection
      ServiceCollectionExtensions (class):  AddFunctionsWorkerCore, AddFunctionsWorkerDefaults
         Azure Functions extensions for .
      SignalRServiceDependencyInjectionExtensions (class):  AddServerlessHub
   Microsoft.Extensions.Hosting
      CoreWorkerHostBuilderExtensions (class):  ConfigureFunctionsWorker, InvokeAutoGeneratedConfigureMethods
         Provides extension methods to work with a .
      FunctionsHostBuilderExtensions (class):  ConfigureFunctionsWebApplication
         Provides extension methods to work with a .
      MiddlewareWorkerApplicationBuilderExtensions (class):  UseDefaultWorkerMiddleware, UseFunctionExecutionMiddleware, UseMiddleware, UseOutputBindingsMiddleware, UseWhen
         Provides extension methods to work with Worker Middleware against a .
      WorkerHostBuilderExtensions (class):  ConfigureFunctionsWorkerDefaults
         Provides extension methods to work with a .
   Microsoft.Extensions.Logging
      FunctionsLoggerExtensions (class):  LogMetric
         Extensions for .
   Microsoft.NET.Sdk.Functions.Http
      HttpResponseMessageWrapper (class):  GetHeader, GetResponseBodyAsync, HttpResponseMessageWrapper
      IHttpClient (interface):  GetAsync, PostAsync
      IHttpResponse (interface):  GetHeader, GetResponseBodyAsync
         A response to an HTTP request
   Microsoft.NET.Sdk.Functions.MSBuild.Tasks
      CreateZipFileTask (class):  Execute
   Microsoft.NET.Sdk.Functions.Tasks
      StringMessages (class)
      ZipDeployTask (class):  Execute

CONSUMER PATHS
   annotate  →  [Binding] on a partial class/member
   annotate  →  [BindingCapabilities] on a partial class/member
   annotate  →  [BindingPropertyName] on a partial class/member
   annotate  →  [BlobInput] on a partial class/member
   annotate  →  [BlobOutput] on a partial class/member
   annotate  →  [BlobTrigger] on a partial class/member

PACKAGES
   ORM/Data:  Microsoft.Azure.Cosmos 3.60.0
   Messaging:  Azure.Messaging.EventGrid 4.29.0, Azure.Messaging.EventHubs 5.12.2, Azure.Messaging.ServiceBus 7.20.1
   Logging:  Microsoft.ApplicationInsights.PerfCounterCollector 2.23.0, OpenTelemetry 1.13.1, OpenTelemetry.Api 1.15.3, OpenTelemetry.Extensions.Hosting 1.13.1
   Cloud:  Azure.Core 1.44.1, Azure.Data.Tables 12.8.3, Azure.Identity 1.17.0, Azure.Storage.Blobs 12.23.0, Azure.Storage.Queues 12.21.0, Microsoft.Azure.Functions.Worker 2.1.0, Microsoft.Azure.Functions.Worker.Core 1.20.0, Microsoft.Azure.Functions.Worker.Extensions.Abstractions 1.3.0 … (12 total)
   Utilities:  Newtonsoft.Json 13.0.3
   Other:  @(PackageReference), Google.Protobuf 3.32.1, Grpc.Core 2.46.6, Grpc.Net.Client 2.55.0, Grpc.Net.ClientFactory 2.65.0, Grpc.Tools 2.72.0, Microsoft.Bcl.AsyncInterfaces 10.0.0, Microsoft.Bcl.HashCode 6.0.0 … (31 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus ExtensionReference)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 327ms |
| GenericExtraction | 1433ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1501ms |
| Compression | 108ms |
| **Total** | **17755ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 1428ms | 1007 | 85 |
| DiRegistrationExtractor | 1233ms | 0 | 85 |
| SourceBodyExtractor | 879ms | 0 | 0 |
| CliCommandExtractor | 611ms | 0 | 274 |
| BodyFactsExtractor | 445ms | 0 | 0 |
| InMemoryEventBusExtractor | 430ms | 0 | 274 |
| AzureFunctionsExtractor | 415ms | 0 | 274 |
| CallGraphExtractor | 379ms | 0 | 0 |
| ProgramCsFlowExtractor | 223ms | 0 | 0 |
| IndirectWiringDetector | 164ms | 0 | 51 |
| ProjectStructure | 135ms | 0 | 0 |
| FileTreeExtractor | 121ms | 0 | 0 |
| SolutionDiscovery | 66ms | 0 | 0 |
| DependencyExtractor | 39ms | 0 | 0 |
| LayerClassifier | 32ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 550 | 292 |
| Resolves | 11 | 0 |

_857 files · 66 projects_
