# REPORT
**DotNetWorker**

Style: Unknown
_15 projects  ·  2 HostedService  ·  net10.0, net48 + azure-servicebus + azure-functions + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 858 |
| Projects | 66 |
| Nodes | 1012 |
| Edges | 695 |
| Entries | 2 |
| With target | 2/2 |
| Deep spine (>=2) | 2/2 (100%) |
| Verified edges | 55% |
| Analyzed in | 16.1s |

## Top Flows

1. **ApplicationInsightsValidationService** → `ApplicationInsightsValidationService` *(HostedService)*
2. **WorkerHostedService** → `WorkerHostedService` *(HostedService)*

### Trace 1: ApplicationInsightsValidationService

TRACE  ApplicationInsightsValidationService
       src/DotNetWorker.ApplicationInsights/FunctionsApplicationInsightsExtensions.cs:67
       DotNetWorker.ApplicationInsights
▸ ENTRY  ApplicationInsightsValidationService  (src/DotNetWorker.ApplicationInsights/FunctionsApplicationInsightsExtensions.cs:67)
   └─ call ApplicationInsightsValidationService  (src/DotNetWorker.ApplicationInsights/FunctionsApplicationInsightsExtensions.cs:67)
          /// <summary>
          /// This services is for a singular purpose: trigger validation of <see cref="FunctionsApplicationInsightsOptions" /> on startup.
          /// </summary>

---

### Trace 2: WorkerHostedService

TRACE  WorkerHostedService
       src/DotNetWorker.Core/Hosting/ServiceCollectionExtensions.cs:73
       DotNetWorker.Core
▸ ENTRY  WorkerHostedService  (src/DotNetWorker.Core/Hosting/ServiceCollectionExtensions.cs:73)
   └─ call WorkerHostedService  (src/DotNetWorker.Core/Hosting/ServiceCollectionExtensions.cs:73)
          internal class WorkerHostedService : IHostedService
          private readonly IWorker _worker;
          private readonly IWorkerDiagnostics _diagnostics;

---

## Insights

_5 info · 2 notable_

### **NOTABLE**: Config without defaults: 3 consumed keys have no appsettings default
*(Risk)*

- AzureWebJobsEventHubSender
- ENVIRONMENT
- FunctionAppUrl

### **NOTABLE**: Internal hubs: 5 heavily-referenced internal types
*(Topology)*

- TestLoggerProvider (152 refs)
- FunctionMetadataGenerator (98 refs)
- EnumerableBinaryTestClass (75 refs)
- GrpcWorker (26 refs)
- ParameterBinder (20 refs)

### _INFO_: Public surface: 26 interfaces, 514 classes (551 total public types)
*(Shape)*

- 26 interfaces
- 514 classes

### _INFO_: Entry targets resolved 2/2 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 55 Extension · 28 Singleton · 1 Scoped · 1 Transient (85 total)
*(Wiring)*

### _INFO_: Most depended-upon: Worker.Extensions.Abstractions (16 dependents) · DotNetWorker (5 dependents) · DotNetWorker.Core (4 dependents)
*(Topology)*

- Worker.Extensions.Abstractions (16 dependents)
- DotNetWorker (5 dependents)
- DotNetWorker.Core (4 dependents)

### _INFO_: Wiring hubs: FunctionContext (100) · IFunctionsWorkerApplicationBuilder (25) · ResolveExtensionCopyLocal (16) · FunctionsResourceDetector (15) · IFunctionsApplication (14)
*(Wiring)*

- FunctionContext (100)
- IFunctionsWorkerApplicationBuilder (25)
- ResolveExtensionCopyLocal (16)
- FunctionsResourceDetector (15)
- IFunctionsApplication (14)

LIBRARY  DotNetWorker     (183 public types)

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
      … and 69 more (use --format json for the full surface)
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
   … and 6 more namespaces (use --format json for the full surface)

CONSUMER PATHS
   annotate  →  [Binding] on a partial class/member
   annotate  →  [BindingCapabilities] on a partial class/member
   annotate  →  [BindingPropertyName] on a partial class/member
   annotate  →  [BlobInput] on a partial class/member
   annotate  →  [BlobOutput] on a partial class/member
   annotate  →  [BlobTrigger] on a partial class/member

ENTRY POINTS
   Background (2)
      ApplicationInsightsValidationService  → ApplicationInsightsValidationService  (src/DotNetWorker.ApplicationInsights/FunctionsApplicationInsightsExtensions.cs:67)
      WorkerHostedService  → WorkerHostedService  (src/DotNetWorker.Core/Hosting/ServiceCollectionExtensions.cs:73)

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
| DiscoveryAndCacheWarmup | 534ms |
| GenericExtraction | 1328ms |
| SignalSealing | 0ms |
| SpecificExtraction | 3835ms |
| Compression | 111ms |
| **Total** | **16149ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 3152ms | 0 | 0 |
| SyntaxStructureExtractor | 1323ms | 1008 | 87 |
| ProgramCsFlowExtractor | 1191ms | 0 | 2 |
| DiRegistrationExtractor | 1183ms | 0 | 87 |
| SourceBodyExtractor | 772ms | 0 | 0 |
| CliCommandExtractor | 673ms | 0 | 274 |
| AzureFunctionsExtractor | 450ms | 0 | 274 |
| InMemoryEventBusExtractor | 447ms | 0 | 258 |
| BodyFactsExtractor | 331ms | 0 | 0 |
| FileTreeExtractor | 220ms | 0 | 0 |
| ProjectStructure | 191ms | 0 | 0 |
| IndirectWiringDetector | 141ms | 0 | 129 |
| SolutionDiscovery | 117ms | 0 | 0 |
| DependencyExtractor | 36ms | 0 | 0 |
| LayerClassifier | 21ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 649 | 280 |
| Resolves | 46 | 31 |

_858 files · 66 projects_
