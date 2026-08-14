LIBRARY  FastEndpoints     (1189 public types)

ENTRY API
   annotate  [A2ASkill]   (A2ASkillAttribute.cs)
      opt an endpoint in to being exposed as an A2A (agent-to-agent) skill.
   annotate  [AllowFileUploads]   (AllowFileUploadsAttribute.cs)
      enable file uploads with multipart/form-data content type
   annotate  [BindFrom]   (BindFromAttribute.cs)
      use this attribute to specify the name of route param, query param, or 
form field if it's different from the name of ...
   annotate  [DontBind]   (DontBindAttribute.cs)
      you can prevent one or more binding sources from supplying values for a 
dto property decorated with this attribute.
   annotate  [DontInject]   (DontInjectAttribute.cs)
      endpoint properties marked with this attribute will disable property 
injection for that property
   annotate  [DontRegister]   (DontRegisterAttribute.cs)
      classes marked with this attribute will be skipped during assembly 
scanning for auto registration
   annotate  [FormField]   (DontBindAttribute.cs)
      disables all other binding sources for a dto property except form fields.
   annotate  [From]   (FromAttribute.cs)
      properties decorated with this attribute will have their values auto bound
from the relevant claim of the current use...
   annotate  [FromBody]   (FromBodyAttribute.cs)
      properties decorated with this attribute will have their values auto bound
from the incoming request's json body.
   annotate  [FromClaim]   (FromClaimAttribute.cs)
      properties decorated with this attribute will have their values auto bound
from the relevant claim of the current use...
   annotate  [FromCookie]   (FromCookieAttribute.cs)
      properties decorated with this attribute will have their values auto bound
from the relevant cookie of the current re...
   annotate  [FromForm]   (FromFormAttribute.cs)
      if a request dto property is decorated with this attribute, that property 
will be bound from complex multipart form d...

ABSTRACTIONS
   Endpoint (class)  - 217 implementors
   EndpointWithoutRequest (class)  - 78 implementors
   Validator (class)  - 42 implementors
   ICommandHandler (interface)  - 35 implementors
   ICommand (interface)  - 30 implementors
   IEvent (interface)  - 21 implementors
   IEventHandler (interface)  - 19 implementors
   IPreProcessor (interface)  - 13 implementors
   IJobStorageProvider (interface)  - 12 implementors
   IJobStorageRecord (interface)  - 12 implementors

GENERATORS
   generator   AccessControlGenerator
   generator   DiscoveredTypesGenerator
   generator   GenericProcessorTypesGenerator
   generator   ReflectionGenerator
   generator   ServiceRegistrationGenerator

PUBLIC SURFACE
   Admin.Login
      AdminLogin (class)
      Endpoint (class):  Configure, Endpoint, HandleAsync
      Endpoint_V1 (class):  Configure, Endpoint_V1
      Endpoint_V2 (class):  Configure, HandleAsync
      Request (class)
         the admin login request
      Response (class)
      Validator (class):  Validator
   Contracts.Dtos
      MyDto (class)
   Customers.Create
      Endpoint (class):  Configure, Endpoint, HandleAsync
      Request (class)
   Customers.CreateWithPropertiesDI
      Endpoint (class):  Configure, HandleAsync
      Request (class)
   Customers.EventHandlers
      SendOrderConfirmation (class):  HandleAsync
   Customers.List.Recent
      Endpoint (class):  Configure, ExecuteAsync
      Endpoint_V1 (class):  Configure
      Endpoint_V2 (class):  Configure
      Endpoint_V3 (class):  Configure
      Response (class)
   Customers.Login
      Endpoint (class):  HandleAsync
   Customers.Update
      Endpoint (class):  Configure, HandleAsync
      Request (class)
   Customers.UpdateWithHeader
      Endpoint (class):  Configure, HandleAsync
      Request (record)
   FastEndpoints
      AllowFileUploadsAttribute (class)
         enable file uploads with multipart/form-data content type
      AntiforgeryMiddleware (class):  Invoke
      BaseCommandExecutor (class)
      BaseEndpoint (class):  Configure, Routes, Verbs
         the base class all fast endpoints inherit from
      BaseHandlerExecutor (class):  Bind, HandlerExecMethodAttributes
      BindFromAttribute (class):  BindFromAttribute
         use this attribute to specify the name of route param, query param, or 
form field if it's different from the name of ...
      BinderContext (struct):  BinderContext, CreateScope, Resolve, TryResolve
         binder context supplied to custom request binders.
      BinderExtensions (class):  AppendEscaped, TryParseByteArray, 
TryParseCollection, TryParseObject
      BindingOptions (class):  ValueParserFor, ValueParserWhen
         request binding options
      BufferedResponseStream (class):  Flush, FlushAsync, Read, Seek, SetLength,
Write, WriteAsync
      ClientStreamCommandExecutor (class):  ExecuteClientStream
      ClientStreamHandlerExecutor (class)
      . and 330 more (the structured surface lists them all)
   FastEndpoints.A2A
      A2AArgumentExtractor (class):  Extract, GetRequestedSkill
      A2AEndpointExtensions (class):  A2ASkill
         opt-in extension methods for exposing FastEndpoints endpoints as A2A 
skills.
      A2AInvocationResultMapper (class):  BuildFaultedError, BuildHttpError, 
BuildResponsePart, BuildSuccess, BuildValidationError, IsJsonMediaType, Map
      A2AJsonRpcEndpoint (class):  Error, HandleAsync, IsValidJsonRpcId, 
TryGetRequestedA2AVersion
      A2AMessage (class)
      A2AMessageSendParams (class)
         A2A v1 SendMessage params.
      A2AMessageValidator (class):  EnsureActualOutputModeIsAccepted, 
EnsureInputModesAreSupported, EnsureRequestedOutputModesAreSupported, 
GetInputModes, GetOutputModes, Validate
      A2AOptions (class)
         configuration for / .
      A2APart (class)
      A2AProvider (class)
         agent-card provider block.
      A2ARpcException (class)
      A2ASendMessageConfiguration (class)
      . and 16 more (the structured surface lists them all)
   FastEndpoints.Agents
      AgentCatalogUniqueness (class):  FormatEndpointTypeNames
      AgentEndpointCatalog (class):  AgentEndpointCatalog, EnsureUnique, 
FromEndpoints, GetVisible, ResolveVisible
      AgentEndpointFeature (class):  AgentEndpointFeature
      AgentHttpContextFactory (class):  Create
      AgentInputPropertyRules (class)
      AgentJsonPropertyNames (class):  BuildSchemaNameMapCore, 
TryGetJsonPropertyName
      AgentJsonSerializerOptions (class)
      AgentPublishedNameResolver (class):  FormatEndpointType, IsValid, 
IsValidChar, SanitizeGenerated, ValidateExplicit
      AgentRequest (class)
      AgentRequestBuilder (class):  AddQueryValues, Build, 
BuildNestedPropertySpec, BuildPropertySpec, BuildRequest, BuildRequestSpec, 
BuildRouteSegments, CombineQueryKey, GetQueryBindingKind, GetScalarValue, 
IsQueryFirstVerb, NormalizeRouteParameter, ParseRouteParameters, ResolvePath, 
ResolvePathSegment
      AgentRequestPropertySpec (record)
      AgentRequestSpec (record)
      . and 12 more (the structured surface lists them all)
   FastEndpoints.AspVersioning
      Extensions (class):  AddVersioning, ApiVersion, WithVersionSet
      VersionSets (class):  CreateApi
         a container for globally holding the instances for the application
   FastEndpoints.ClientGen
      CsClientGenProcessor (class):  Process
      Extensions (class):  ExportSwaggerJsonAndExitAsync, GenerateCSharpClient, 
GenerateClientsAndExitAsync, GenerateTypeScriptClient, MapCSharpClientEndpoint, 
MapTypeScriptClientEndpoint
      LoggingExtensions (class):  ApiClientGenerationStarting, 
ApiClientGenerationSuccessful, ExportingSwaggerJson, SwaggerJsonExportSuccessful
      Runner (class)
      TsClientGenProcessor (class):  Process
   FastEndpoints.ClientGen.Kiota
      ClientGenConfig (class)
      ClientGenerator (class)
      Extensions (class):  ExportSwaggerJson, ExportSwaggerJsonAndExitAsync, 
GenerateApiClientsAndExitAsync, GenerateClient, IsApiClientGenerationMode, 
IsNotGenerationMode, IsSwaggerJsonExportMode, MapApiClientEndpoint
      LoggingExtensions (class):  ApiClientGenerationSuccessful, 
ClientArchiveCreationSuccessful, ExportingSwaggerSpec, 
StartingApiClientGeneration, SwaggerSpecExportSuccessful, 
ZippingGeneratedClientFile
      SwaggerJsonExportConfig (class)
         settings for exporting a swagger document as json
   FastEndpoints.Generator
      Extensions (class)
      FullTypeComparer (class):  Equals, FullTypeComparer, GetHashCode
      Match (struct)
      Match (struct)
      MatchComparer (class):  Equals, GetHashCode, MatchComparer
      MatchComparer (class):  Equals, GetHashCode, MatchComparer
      Permission (struct):  GetAclHash
      PermissionComparer (class):  Equals, GetHashCode, PermissionComparer
      Registration (struct):  Registration
      TypeCollector (class)
      TypeData (struct)
      TypeDataComparer (class):  Equals, GetHashCode, TypeDataComparer
      . and 2 more (the structured surface lists them all)
   FastEndpoints.JobsQueues
      LoggingExtensions (class):  CommandExecutionCritical, 
CommandParallelExecutionWarning, JobCancelledManually, StorageGetJobResultError,
StorageMarkAsCompleteError, StorageOnExecutionFailureError, 
StoragePurgeStaleJobsError, StorageRetrieveError, StorageStoreJobResultError
   FastEndpoints.Mcp
      EndpointMcpToolSource (class):  BuildFaultedResult, BuildHttpErrorResult, 
BuildSuccessResult, BuildTool, BuildToolCatalog, BuildTools, 
BuildValidationErrorResult, BuildVisibleProtocolTools, GetRootPropertyNames, 
GetVisibleTools, ResolveCallerContext, ResolveVisibleTool
         builds instances for every FastEndpoints endpoint that opted-in via 
McpTool(...) or [McpTool].
      Extensions (class):  AddMcp, CallToolAsync, ListToolsAsync, 
ResolveCallerContext, UseMcp
         entry-point extensions for wiring FastEndpoints-as-MCP into the ASP.NET
Core host.
      McpEndpointExtensions (class):  McpTool
         opt-in extension methods for exposing FastEndpoints endpoints as MCP 
tools.
      McpOptions (class)
         user-facing configuration for the FastEndpoints MCP bridge.
      McpStructuredContentMapper (class):  MatchesSchemaType, 
TryApplyOutputSchema, TryExtractStructuredContent, TryMap, ValidateJsonObject, 
ValidateJsonValue
      McpToolAttribute (class):  McpToolAttribute, ResolveConfiguredHints
         opt an endpoint in to being exposed as a Model Context Protocol (MCP) 
tool.
      McpToolBehaviorHints (class)
         advisory behavioral hints for an MCP tool.
      McpToolInfo (class)
         opt-in metadata attached to a FastEndpoints endpoint so the 
FastEndpoints.Mcp addon exposes it as a Model Context Pro...
      McpToolSchemaFactory (class):  BuildInputSchema, 
DisallowAdditionalInputProperties, EnrichInputSchemaWithValidation, 
EnsureObjectRootSchema, GetRootSchemaType, HasObjectRootSchema, 
NormalizeRootObjectSchema, RemoveNonClientInputProperties, RemoveRequiredEntry, 
RemoveSchemaProperties, RemoveToHeaderOutputProperties, 
ResolveSerializerOptions, TryBuildOutputSchema, TryResolveValidator
      ToolRegistration (record)
   FastEndpoints.Messaging
      Types (class)
   FastEndpoints.Messaging.Remote
      LoggingExtensions (class):  EventHubExceptionReceiverFault, 
NoSubscribersWarning, QueueOverflowWarning, RestoreSubscriberIDsError, 
SubscriberConnected
      ObjectFactoryExtensions (class)
   FastEndpoints.Messaging.Remote.Core
      LoggingExtensions (class):  EmptyTrackingIdWarning, 
EventExecutionCompletionObservedCritical, EventExecutionTaskCritical, 
EventExecutorTaskTerminatedCritical, EventReceiverTaskTerminatedCritical, 
HandlerExecutionCritical, RemoteConnectionConfigured, SetEventCritical, 
StorageGetNextBatchError, StorageMarkAsCompleteError, StoreEventError, 
StoreEventsError, StreamReceiveTrace, SubscriberExceptionReceiverFault, 
SubscriberRegistered
   FastEndpoints.OData
      ODataBinder (class):  BindAsync
      ODataEndpoint (class):  Configure, HandleAsync
         implement this abstract class and override the ExecuteAsync() method to
create OData endpoints.
      ODataMemberInfo (class):  GetCustomAttributes, IsDefined
      ODataParamInfo (class):  GetCustomAttributes, GetCustomAttributesData
      ODataResult (class)
   FastEndpoints.OpenApi
      AutoTagOverride (class)
         represents an auto-tag override value.
      ChildValidatorAdapterAccessor (class):  CompileContextFactory, 
CompileGetValidator, Create, Get, Missing
      ChildValidatorResolver (class):  ApplyChildValidator, 
ApplyRulesFromIncludedValidators, Dispose, GetIncludedValidators, 
TryCreateChildValidator, TryGetIncludedValidator
      ComplexQueryParameterExpander (class):  Add, GetDontBindRequiredness
      ConstructorDefaultExtensions (class):  CreateConstructorDefaultMap
      DocumentOptions (class):  AddAuth
         options for the openapi document
      DocumentPathNormalizer (class):  Apply, CollectPathRenames, 
MergePathItems, NormalizeParameterNames, RenamePaths
      DocumentSchemaHelpers (class):  EnsureComponentSchemas, 
FormFileArraySchema, FormFileBinarySchema, GetOperationVariantSourceRefId, 
IsFormFileCollectionRef, IsFormFileRef, RewriteFormFileSchema, RewriteSchemaRef,
RewriteSchemaRefs, VariantReferencesRefId
      DocumentSchemaNormalizer (class):  Normalize, RemoveFrameworkSchemas
      DocumentSecurityTransformer (class):  AddSecuritySchemes, Apply, 
FixOperationSecurity
      DocumentTagTransformer (class):  Apply, Cleanup
      DocumentTransformer (class):  TransformAsync
      . and 60 more (the structured surface lists them all)
   FastEndpoints.OpenApi.Kiota
      ClientGenConfig (class)
      ClientGenerator (class)
      Extensions (class):  ExecuteGenerationModeAsync, ExportOpenApiJson, 
GenerateClient, MapApiClientEndpoint, NormalizeOutputFileName, 
SafeDeleteDirectory, ValidateClientGenerationConfig, 
ValidateOpenApiJsonExportConfig
      GenerationWorkspace (class):  Create, CreateRootPath, Dispose, 
GenerationWorkspace
      LoggingExtensions (class):  ApiClientGenerationSuccessful, 
ClientArchiveCreationSuccessful, ExportingOpenApiSpec, 
OpenApiSpecExportSuccessful, StartingApiClientGeneration, 
ZippingGeneratedClientFile
      OpenApiJsonExportConfig (class)
         settings for exporting an openapi document as json
   FastEndpoints.OpenApi.ValidationProcessor
      FluentValidationRule (class)
      RuleContext (class):  TryGetPropertySchema
   . and 102 more namespaces (the structured surface lists them all)

CONSUMER PATHS
   annotate    [A2ASkill] on a partial class/member
   annotate    [AllowFileUploads] on a partial class/member
   annotate    [BindFrom] on a partial class/member
   annotate    [DontBind] on a partial class/member
   annotate    [DontInject] on a partial class/member
   annotate    [DontRegister] on a partial class/member

ENTRY POINTS
   HTTP (158)
      DELETE /release-versioning/endpoint-b   EndpointB_V1_Delete  
(TestHarness/Web/[Features]/TestCases/Swagger/ReleaseVersioning/Endpoints.cs:74)
      DELETE inventory/manage/delete/{itemID}   Endpoint  
(TestHarness/Web/[Features]/Inventory/Manage/Delete/Endpoint.cs:7)
      GET /   ClientGenerator  (Src/ClientGen.Kiota/Extensions.cs:33)
      GET /_test_url_cache_   IEndpoint.GetTestUrlCache  
(Src/Library/Main/MainExtensions.cs:234)
      GET /api/ping   ResponseSender.OkAsync  
(TestHarness/OpenApi.Kiota/PingEndpoint.cs:9)
      GET /apiary/ver0/status   ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:270)
      GET /customer/login   Endpoint  
(TestHarness/Web/[Features]/Customers/Login/Endpoint.cs:5)
      GET /inventory/list/recent/{CategoryID}   Endpoint  
(TestHarness/Web/[Features]/Inventory/List/Recent/Endpoint.cs:6)
      GET /release-versioning/endpoint-a   EndpointA  
(TestHarness/Web/[Features]/TestCases/Swagger/ReleaseVersioning/Endpoints.cs:9)
      GET /release-versioning/endpoint-a [HandleAsync]   EndpointA_V1  
(TestHarness/Web/[Features]/TestCases/Swagger/ReleaseVersioning/Endpoints.cs:21)
      GET /release-versioning/endpoint-b   EndpointB  
(TestHarness/Web/[Features]/TestCases/Swagger/ReleaseVersioning/Endpoints.cs:49)
      GET /release-versioning/endpoint-b [HandleAsync]   EndpointB_V1  
(TestHarness/Web/[Features]/TestCases/Swagger/ReleaseVersioning/Endpoints.cs:61)
      GET /sales/orders/retrieve/{@id}   Endpoint  
(TestHarness/Web/[Features]/Sales/Orders/Retrieve/Endpoint.cs:9)
      GET /swagger-review/auto-tag-override   ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:13)
      GET /swagger-review/bindfrom-query-get   ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:340)
      GET /swagger-review/catch-all/{*slug}   ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:289)
      GET /swagger-review/cookie-get   ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:561)
      GET /swagger-review/default-route-value/{id=5}   ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:1094)
      GET /swagger-review/duplicate-query-naming-policy   
ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:307)
      GET /swagger-review/duplicate-x402-header   ResponseSender.OkAsync  
(TestHarness/Web/[Features]/TestCases/Swagger/Review/Endpoints.cs:619)
      . and 138 more (http entries)
   Background (1)
      EventHubInitializer  
(Src/Messaging/Messaging.Remote/Server/HandlerServerExtensions.cs:42)
   CLI (1)
      FastEndpoints.Generator.Cli (Main)  (Src/Generator.Cli/Program.cs:11)

PACKAGES
   Web/API:  Grpc.AspNetCore.Server 2.80.0, 
Microsoft.AspNetCore.Authentication.JwtBearer 8.0.26, 
Microsoft.AspNetCore.JsonPatch.SystemTextJson 10.0.7, Microsoft.AspNetCore.OData
9.4.1, Microsoft.AspNetCore.OpenApi 10.0.7, ModelContextProtocol.AspNetCore 
1.2.0, NSwag.AspNetCore 14.7.1, NSwag.Generation.AspNetCore 14.7.1 . (9 total)
   ORM/Data:  Microsoft.EntityFrameworkCore 10.0.7, 
Microsoft.EntityFrameworkCore.InMemory 10.0.7
   Validation:  FluentValidation 12.1.1
   Other:  Asp.Versioning.Http 8.1.1, Asp.Versioning.Mvc.ApiExplorer 8.1.1, 
Grpc.Net.Client 2.80.0, Grpc.Net.Client.Web 2.80.0, MessagePack 3.1.4, 
Microsoft.CodeAnalysis.Analyzers 5.3.0, Microsoft.CodeAnalysis.CSharp [4.11.0], 
Microsoft.Extensions.DependencyInjection.Abstractions 10.0.7 . (15 total)

 drill in:  trace a focused type   (e.g. trace AdminLogin)

