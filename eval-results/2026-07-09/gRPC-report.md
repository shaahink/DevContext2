# REPORT
**Grpc.DotNet**

Style: SampleCollection
_30 projects  ·  74 GrpcService  ·  net10.0, net10.0;net9.0;net8.0;net462 + blazor + controllers + minimal-apis + razor-pages + grpc + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 828 |
| Projects | 106 |
| Nodes | 1354 |
| Edges | 1035 |
| Entries | 74 |
| With target | 74/74 |
| Verified edges | 63% |
| Analyzed in | 23.7s |

## Top Flows

1. **Command.Create** → `RemoveCommand` *(GrpcService)*
2. **Command.Create** → `RefreshCommand` *(GrpcService)*
3. **Command.Create** → `ListCommand` *(GrpcService)*
4. **Command.Create** → `AddUrlCommand` *(GrpcService)*
5. **Command.Create** → `AddFileCommand` *(GrpcService)*
6. **ChannelCredentialsConfigurator.SetCompositeCredentials** → `ClientFactoryCredentialsConfigurator` *(GrpcService)*
7. **ChannelCredentialsConfigurator.SetCompositeCredentials** → `DefaultChannelCredentialsConfigurator` *(GrpcService)*
8. **ChannelCredentialsConfigurator.SetInsecureCredentials** → `ClientFactoryCredentialsConfigurator` *(GrpcService)*
9. **ChannelCredentialsConfigurator.SetInsecureCredentials** → `DefaultChannelCredentialsConfigurator` *(GrpcService)*
10. **ChannelCredentialsConfigurator.SetSslCredentials** → `ClientFactoryCredentialsConfigurator` *(GrpcService)*

### Trace 1: Command.Create

TRACE  Command.Create
       src/dotnet-grpc/Commands/RemoveCommand.cs:26
       dotnet-grpc
▸ ENTRY  Command.Create  (src/dotnet-grpc/Commands/RemoveCommand.cs:26)
   ├─ call RemoveCommand  (src/dotnet-grpc/Commands/RemoveCommand.cs:26)
   │      internal sealed class RemoveCommand : CommandBase
   │      public RemoveCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call RefreshCommand  (src/dotnet-grpc/Commands/RefreshCommand.cs:26)
   │      internal sealed class RefreshCommand : CommandBase
   │      public RefreshCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call ListCommand  (src/dotnet-grpc/Commands/ListCommand.cs:27)
   │      internal sealed class ListCommand : CommandBase
   │      public ListCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call AddUrlCommand  (src/dotnet-grpc/Commands/AddUrlCommand.cs:26)
   │      internal sealed class AddUrlCommand : CommandBase
   │      public AddUrlCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   └─ call AddFileCommand  (src/dotnet-grpc/Commands/AddFileCommand.cs:26)
          internal sealed class AddFileCommand : CommandBase
          public AddFileCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
          : base(console, projectPath, httpClient) { }

---

### Trace 2: Command.Create

TRACE  Command.Create
       src/dotnet-grpc/Commands/RemoveCommand.cs:26
       dotnet-grpc
▸ ENTRY  Command.Create  (src/dotnet-grpc/Commands/RemoveCommand.cs:26)
   ├─ call RemoveCommand  (src/dotnet-grpc/Commands/RemoveCommand.cs:26)
   │      internal sealed class RemoveCommand : CommandBase
   │      public RemoveCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call RefreshCommand  (src/dotnet-grpc/Commands/RefreshCommand.cs:26)
   │      internal sealed class RefreshCommand : CommandBase
   │      public RefreshCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call ListCommand  (src/dotnet-grpc/Commands/ListCommand.cs:27)
   │      internal sealed class ListCommand : CommandBase
   │      public ListCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call AddUrlCommand  (src/dotnet-grpc/Commands/AddUrlCommand.cs:26)
   │      internal sealed class AddUrlCommand : CommandBase
   │      public AddUrlCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   └─ call AddFileCommand  (src/dotnet-grpc/Commands/AddFileCommand.cs:26)
          internal sealed class AddFileCommand : CommandBase
          public AddFileCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
          : base(console, projectPath, httpClient) { }

---

### Trace 3: Command.Create

TRACE  Command.Create
       src/dotnet-grpc/Commands/RemoveCommand.cs:26
       dotnet-grpc
▸ ENTRY  Command.Create  (src/dotnet-grpc/Commands/RemoveCommand.cs:26)
   ├─ call RemoveCommand  (src/dotnet-grpc/Commands/RemoveCommand.cs:26)
   │      internal sealed class RemoveCommand : CommandBase
   │      public RemoveCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call RefreshCommand  (src/dotnet-grpc/Commands/RefreshCommand.cs:26)
   │      internal sealed class RefreshCommand : CommandBase
   │      public RefreshCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call ListCommand  (src/dotnet-grpc/Commands/ListCommand.cs:27)
   │      internal sealed class ListCommand : CommandBase
   │      public ListCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   ├─ call AddUrlCommand  (src/dotnet-grpc/Commands/AddUrlCommand.cs:26)
   │      internal sealed class AddUrlCommand : CommandBase
   │      public AddUrlCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
   │      : base(console, projectPath, httpClient) { }
   └─ call AddFileCommand  (src/dotnet-grpc/Commands/AddFileCommand.cs:26)
          internal sealed class AddFileCommand : CommandBase
          public AddFileCommand(ConsoleService console, string? projectPath, HttpClient httpClient)
          : base(console, projectPath, httpClient) { }

---

## Insights

_4 info · 4 notable_

### **NOTABLE**: Config without defaults: 3 consumed keys have no appsettings default
*(Risk)*

- enableCertAuth
- enableGrpcWeb
- Http3Port

### **NOTABLE**: Internal hubs: 5 heavily-referenced internal types
*(Topology)*

- Assert (159 refs)
- GrpcProtocolHelpers (41 refs)
- HttpContextServerCallContext (39 refs)
- GrpcEventSource (38 refs)
- ExecutionContextLogger (35 refs)

### **NOTABLE**: Extension seats: AddGrpcClient (23 impls) · AddGrpc (13 impls) · AddLogging (9 impls)
*(Wiring)*

- AddGrpcClient (23 impls)
- AddGrpc (13 impls)
- AddLogging (9 impls)

### **NOTABLE**: Most depended-upon: Grpc.AspNetCore (5 dependents) · Grpc.Core.Api (5 dependents) · Grpc.Net.Client (5 dependents)
*(Topology)*

- Grpc.AspNetCore (5 dependents)
- Grpc.Core.Api (5 dependents)
- Grpc.Net.Client (5 dependents)

### _INFO_: Entry targets resolved 74/74 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: DI: 272 Extension · 59 Singleton · 7 Transient · 6 Scoped (344 total)
*(Wiring)*

### _INFO_: Public surface: 20 interfaces, 488 classes (518 total public types)
*(Shape)*

- 20 interfaces
- 488 classes

### _INFO_: Wiring hubs: List (108) · ServiceCollection (67) · IAsyncStreamReader (66) · IServerStreamWriter (62) · HealthServiceImpl (25)
*(Wiring)*

- List (108)
- ServiceCollection (67)
- IAsyncStreamReader (66)
- IServerStreamWriter (62)
- HealthServiceImpl (25)

LIBRARY  Grpc.DotNet     (167 public types)

ENTRY API
   register  GrpcClientServiceExtensions.AddGrpcClient   (GrpcClientServiceExtensions.cs)
      Adds the and related services to the and configures a binding between the type and a named .
   register  GrpcEndpointRouteBuilderExtensions.MapGrpcService   (GrpcEndpointRouteBuilderExtensions.cs)
      Maps incoming requests to the specified type.
   register  GrpcHealthChecksEndpointRouteBuilderExtensions.MapGrpcHealthChecksService   (GrpcHealthChecksEndpointRouteBuilderExtensions.cs)
      Maps incoming requests to the gRPC health checks service.
   register  GrpcHealthChecksServiceExtensions.AddGrpcHealthChecks   (GrpcHealthChecksServiceExtensions.cs)
      Adds gRPC health check services to the specified .
   register  GrpcReflectionEndpointRouteBuilderExtensions.MapGrpcReflectionService   (GrpcReflectionEndpointRouteBuilderExtensions.cs)
      Maps incoming requests to the gRPC reflection service.
   register  GrpcReflectionServiceExtensions.AddGrpcReflection   (GrpcReflectionServiceExtensions.cs)
      Adds gRPC reflection services to the specified .
   register  GrpcServicesExtensions.AddGrpc   (GrpcServiceExtensions.cs)
      Adds gRPC services to the specified .
   register  GrpcWebApplicationBuilderExtensions.UseGrpcWeb   (GrpcWebApplicationBuilderExtensions.cs)
      Adds gRPC-Web middleware to the specified .
   build     Builder   (ServerServiceDefinition.cs)
      Builder class for .
   derive    ChannelCredentials   (ChannelCredentials.cs)
      Client-side channel credentials.
   derive    ConfigObject   (ConfigObject.cs)
      Represents a configuration object.
   implement IAsyncStreamReader   (IAsyncStreamReader.cs)
      A stream of messages to be read.

ABSTRACTIONS
   Interceptor (class)  — 17 implementors
   IAsyncStreamReader (interface)  — 8 implementors
   ConfigObject (class)  — 7 implementors
   ChannelCredentials (class)  — 5 implementors
   ICompressionProvider (interface)  — 5 implementors
   IServerStreamWriter (interface)  — 5 implementors
   ServerCallContext (class)  — 5 implementors
   CallInvoker (class)  — 4 implementors
   ClientBase (class)  — 4 implementors
   IGrpcServiceActivator (interface)  — 4 implementors

PUBLIC SURFACE
   Common
      BenchmarkConfigurationHelpers (class):  CreateBindingAddress, CreateIPEndPoint
   FunctionalTestsWebsite
      Program (class):  CreateHostBuilder, Main
      Startup (class):  Configure, ConfigureServices
   FunctionalTestsWebsite.Infrastructure
      DynamicEndpointDataSource (class):  AddEndpoints, GetChangeToken
         This endpoint data source can be modified and will raise a change token event.
      DynamicService (class)
      IncrementingCounter (class):  Increment
      ScopedValueProvider (class)
      SingletonValueProvider (class)
      TransientValueProvider (class)
      ValueProvider (class):  GetNext
   FunctionalTestsWebsite.Services
      AnyService (class):  DoAny
      AuthorizedGreeter (class):  SayHello
      ChatterService (class):  Chat, ChatCore
      CounterService (class):  AccumulateCount, CounterService, IncrementCount
      DeflateCompressionService (class):  SayHello, WriteMessageWithoutCompression
      EchoService (class):  ClientStreamingEcho, Echo, EchoAbort, FullDuplexEcho, HalfDuplexEcho, NoOp, ServerStreamingEcho, ServerStreamingEchoAbort
         Written to as closely as possible mirror the behaviour of the C++ implementation in grpc/grpc-web: https://github.com...
      GreeterService (class):  GreeterService, SayHello, SayHelloSendLargeReply, SayHelloWithHttpContextAccessor, SayHellos, SayHellosCore, SayHellosSendHeadersFirst
      GzipCompressionService (class):  SayHello, WriteMessageWithoutCompression
      IssueService (class):  GetLibrary
      LifetimeService (class):  GetScopedValue, GetSingletonValue, GetTransientValue, LifetimeService
      NestedService (class):  NestedService, SayHello
      RacerService (class):  ReadySetGo
      SecondGreeterService (class):  SayHello
      SingletonCounterService (class):  Dispose, IncrementCount, SingletonCounterService
      StreamService (class):  BufferAllData, ClientStreamedData, EchoAllData, StreamService
      TesterService (class):  SayHelloBidirectionalStreaming, SayHelloBidirectionalStreamingError, SayHelloClientStreaming, SayHelloClientStreamingError, SayHelloServerStreaming, SayHelloServerStreamingError, SayHelloUnary, SayHelloUnaryError, TesterService
   Grpc.AspNetCore.ClientFactory
      GrpcContextPropagationOptions (class)
         Options used to configure gRPC call context propagation.
   Grpc.AspNetCore.HealthChecks
      GrpcHealthChecksOptions (class)
         Contains options for the gRPC health checks service.
      HealthCheckMapContext (class):  HealthCheckMapContext
         Context used to map health check registrations to a service.
      HealthResult (class):  HealthResult
         Represents the result of a single .
      ServiceMapping (class):  ServiceMapping
         Represents the mapping of health check results to a service exposed by the gRPC health checks API.
      ServiceMappingCollection (class):  Add, Clear, Contains, CopyTo, GetEnumerator, Map, MapService, Remove
         A collection of used to map health results to gRPC health checks services.
   Grpc.AspNetCore.Server
      GrpcActivatorHandle (struct):  GrpcActivatorHandle
         Handle to the activator instance.
      GrpcMethodMetadata (class):  GrpcMethodMetadata
         Metadata for a gRPC method endpoint.
      GrpcServiceOptions (class)
         Options used to configure service instances.
      IGrpcInterceptorActivator (interface):  Create, ReleaseAsync
         An interceptor activator abstraction.
      IGrpcServerBuilder (interface)
         A builder abstraction for configuring gRPC servers.
      IGrpcServiceActivator (interface):  Create, ReleaseAsync
         A activator abstraction.
      IServerCallContextFeature (interface)
         Provides access to the gRPC server call context for the current HTTP request.
      InterceptorCollection (class):  Add
         Represents the pipeline of interceptors to be invoked when processing a gRPC call.
      InterceptorRegistration (class)
         Representation of a registration of an in the server pipeline.
   Grpc.AspNetCore.Server.Model
      IServiceMethodProvider (interface):  OnServiceMethodDiscovery
         Defines a contract for specifying methods for .
      ServiceMethodProviderContext (class):  AddClientStreamingMethod, AddDuplexStreamingMethod, AddMethod, AddServerStreamingMethod, AddUnaryMethod
         A context for .
   Grpc.AspNetCore.Web
      DisableGrpcWebAttribute (class)
         Identifies an endpoint that doesn't support gRPC-Web.
      EnableGrpcWebAttribute (class)
         Identifies an endpoint that supports gRPC-Web.
      IGrpcWebEnabledMetadata (interface)
         Represents gRPC-Web metadata used during request processing.
   Grpc.Auth
      GoogleAuthInterceptors (class):  FromAccessToken, FromCredential
         Factory methods to create authorization interceptors for Google credentials.
      GoogleGrpcCredentials (class):  FromAccessToken, GetApplicationDefaultAsync, ToCallCredentials, ToChannelCredentials
         Factory/extension methods to create instances of and classes based on credential objects originating from Google auth...
   Grpc.Core
      AsyncClientStreamingCall (class):  AsyncClientStreamingCall, ConfigureAwait, Dispose, GetAwaiter, GetStatus, GetTrailers
         Return type for client streaming calls.
      AsyncDuplexStreamingCall (class):  AsyncDuplexStreamingCall, Dispose, GetStatus, GetTrailers
         Return type for bidirectional streaming calls.
      AsyncServerStreamingCall (class):  AsyncServerStreamingCall, Dispose, GetStatus, GetTrailers
         Return type for server streaming calls.
      AsyncStreamReaderExtensions (class):  MoveNext, ReadAllAsync
         Extension methods for .
      AsyncUnaryCall (class):  AsyncUnaryCall, ConfigureAwait, Dispose, GetAwaiter, GetStatus, GetTrailers
         Return type for single request - single response call.
      AuthContext (class):  AuthContext, FindPropertiesByName
         Authentication context for a call.
      AuthInterceptorContext (class):  AuthInterceptorContext
         Context for an RPC being intercepted by .
      AuthProperty (class):  Create
         A property of an .
      BindServiceMethodAttribute (class):  BindServiceMethodAttribute
         Specifies the location of the service bind method for a gRPC service.
      Builder (class):  AddMethod, Build, Builder
         Builder class for .
      CallCredentials (class):  Compose, FromInterceptor, InternalPopulateConfiguration
         Client-side call credentials.
      CallCredentialsConfiguratorBase (class):  SetAsyncAuthInterceptorCredentials, SetCompositeCredentials
         Base class for objects that can consume configuration from CallCredentials objects.
      CallInvoker (class):  AsyncClientStreamingCall, AsyncDuplexStreamingCall, AsyncServerStreamingCall, AsyncUnaryCall, BlockingUnaryCall
         Abstraction of client-side RPC invocation.
      CallOptions (struct):  CallOptions, WithCancellationToken, WithCredentials, WithDeadline, WithHeaders, WithPropagationToken, WithWaitForReady, WithWriteOptions
         Options for calls made by client.
      ChannelBase (class):  CreateCallInvoker, ShutdownAsync
         Base class for gRPC channel.
      ChannelCredentials (class):  ChannelCredentials, Create, InternalPopulateConfiguration
         Client-side channel credentials.
      ChannelCredentialsConfiguratorBase (class):  SetCompositeCredentials, SetInsecureCredentials, SetSslCredentials
         Base class for objects that can consume configuration from CallCredentials objects.
      ClientBase (class):  ClientBase, WithHost
         Generic base class for client-side stubs.
      ContextPropagationOptions (class):  ContextPropagationOptions
         Options for .
      ContextPropagationToken (class)
         Token for propagating context of server side handlers to child calls.
      DeserializationContext (class):  PayloadAsNewBuffer, PayloadAsReadOnlySequence
         Provides access to the payload being deserialized when deserializing messages.
      Entry (class):  Entry, ToString
         Metadata entry
      ExceptionExtensions (class):  ToRpcDebugInfo
         Extensions methods for converting to .
      IAsyncStreamReader (interface):  MoveNext
         A stream of messages to be read.
      IAsyncStreamWriter (interface):  WriteAsync
         A writable stream of messages.
      IClientStreamWriter (interface):  CompleteAsync
         Client-side writable stream of messages with Close capability.
      IMethod (interface)
         A non-generic representation of a remote method.
      IServerStreamWriter (interface)
         A writable stream of messages that is used in server-side handlers.
      KeyCertificatePair (class):  KeyCertificatePair
         Key certificate pair (in PEM encoding).
      Marshaller (class):  Marshaller
         Encapsulates the logic for serializing and deserializing messages.
      Marshallers (class):  Create
         Utilities for creating marshallers.
      Metadata (class):  Add, Clear, Contains, CopyTo, Get, GetAll, GetEnumerator, GetValue, GetValueBytes, IndexOf, Insert, Metadata, Remove, RemoveAt
         A collection of metadata entries that can be exchanged during a call.
      MetadataExtensions (class):  GetRpcStatus, SetRpcStatus
         Extension methods for using with .
      Method (class):  Method
         A description of a remote method.
      RpcException (class):  RpcException
         Thrown when remote procedure call fails.
      RpcExceptionExtensions (class):  GetRpcStatus
         Extension methods for getting from .
      RpcStatusExtensions (class):  ToRpcException
         Extensions methods for converting to .
      SerializationContext (class):  Complete, GetBufferWriter, SetPayloadLength
         Provides storage for payload when serializing a message.
      ServerCallContext (class):  CreatePropagationToken, WriteResponseHeadersAsync
         Context for a server-side call.
      ServerCallContextExtensions (class):  GetHttpContext
         Extension methods for ServerCallContext.
      ServerServiceDefinition (class):  BindService, CreateBuilder
         Stores mapping of methods to server call handlers.
      ServiceBinderBase (class):  AddMethod
         Allows binding server-side method implementations in alternative serving stacks.
      SslCredentials (class):  InternalPopulateConfiguration, SslCredentials
         Client-side SSL credentials.
      Status (struct):  Status, ToString
         Represents RPC result, which consists of and an optional detail string.
      VerifyPeerContext (class)
         Verification context for VerifyPeerCallback.
      VersionInfo (class)
         Provides info about current version of gRPC.
      WriteOptions (class):  WriteOptions
         Options for write operations.
   Grpc.Core.Interceptors
      CallInvokerExtensions (class):  Intercept
         Extends the CallInvoker class to provide the interceptor facility on the client side.
      ChannelExtensions (class):  Intercept
         Provides extension methods to make it easy to register interceptors on Channel objects.
      ClientInterceptorContext (struct):  ClientInterceptorContext
         Carries along the context associated with intercepted invocations on the client side.
      Interceptor (class):  AsyncClientStreamingCall, AsyncDuplexStreamingCall, AsyncServerStreamingCall, AsyncUnaryCall, BlockingUnaryCall, ClientStreamingServerHandler, DuplexStreamingServerHandler, ServerStreamingServerHandler, UnaryServerHandler
         Serves as the base class for gRPC interceptors.
   Grpc.Core.Utils
      GrpcPreconditions (class):  CheckArgument, CheckNotNull, CheckState
         Utility methods to simplify checking preconditions in the code.
   Grpc.HealthCheck
      HealthServiceImpl (class):  Check, ClearAll, ClearStatus, SetStatus, Watch
         Implementation of a simple Health service.
   Grpc.Net.Client
      GrpcChannel (class):  CreateCallInvoker, Dispose, ForAddress
         Represents a gRPC channel.
      GrpcChannelOptions (class):  GrpcChannelOptions
         An options class for configuring a .
   Grpc.Net.Client.Configuration
      ConfigObject (class)
         Represents a configuration object.
      HedgingPolicy (class):  HedgingPolicy
         The hedging policy for outgoing calls.
      LoadBalancingConfig (class):  LoadBalancingConfig
         Base type for load balancer policy configuration.
      MethodConfig (class):  MethodConfig
         Configuration for a method.
      MethodName (class):  MethodName
         The name of a method.
      PickFirstConfig (class):  PickFirstConfig
         Configuration for pick_first load balancer policy.
      RetryPolicy (class):  RetryPolicy
         The retry policy for outgoing calls.
      RetryThrottlingPolicy (class):  RetryThrottlingPolicy
         The retry throttling policy for a server.
      RoundRobinConfig (class):  RoundRobinConfig
         Configuration for pick_first load balancer policy.
      ServiceConfig (class):  ServiceConfig
         A represents information about a service.
   Grpc.Net.Client.Web
      GrpcWebHandler (class):  GrpcWebHandler
         A implementation that executes gRPC-Web request processing.
   Grpc.Net.ClientFactory
      CallOptionsContext (class)
         Context used to update for a gRPC call.
      GrpcClientFactory (class):  CreateClient
         A factory abstraction for a component that can create gRPC client instances with custom configuration for a given log...
      GrpcClientFactoryOptions (class)
         Options used to configure a gRPC client.
      InterceptorRegistration (class):  InterceptorRegistration
         Representation of a registration of an in the client pipeline.
   Grpc.Net.Compression
      GzipCompressionProvider (class):  CreateCompressionStream, CreateDecompressionStream, GzipCompressionProvider
         GZIP compression provider.
      ICompressionProvider (interface):  CreateCompressionStream, CreateDecompressionStream
         Provides a specific compression implementation to compress gRPC messages.
   Grpc.Reflection
      ReflectionServiceImpl (class):  ProcessRequest, ReflectionServiceImpl, ServerReflectionInfo, ToResponse
         Implementation of server reflection service (v1alpha).
      ReflectionV1ServiceImpl (class):  ProcessRequest, ReflectionV1ServiceImpl, ServerReflectionInfo, ToResponse
         Implementation of server reflection service (v1).
      Response (struct):  Error, FileDescriptor, Services
      SymbolRegistry (class):  FileByName, FileContainingSymbol, FromFiles
         Registry of protobuf symbols
   Grpc.Shared
      ServiceProvidersMiddleware (class):  InvokeAsync, ServiceProvidersMiddleware
   Grpc.Shared.TestAssets
      ClientOptions (class)
      CoreChannelWrapper (class):  CoreChannelWrapper, ShutdownAsync
      ExceptionAssert (class):  ThrowsAsync
      GrpcChannelWrapper (class):  GrpcChannelWrapper, ShutdownAsync
      IChannelWrapper (interface):  ShutdownAsync
      InteropClient (class):  InteropClient, Run, RunCancelAfterBeginAsync, RunCancelAfterFirstResponseAsync, RunClientCompressedStreamingAsync, RunClientCompressedUnary, RunClientStreamingAsync, RunComputeEngineCreds, RunCustomMetadataAsync, RunEmptyStreamAsync, RunEmptyUnary, RunJwtTokenCreds, RunLargeUnary, RunOAuth2AuthTokenAsync, RunPerRpcCredsAsync
      TestCredentials (class):  CreateSslCredentials, CreateSslServerCredentials
         SSL Credentials for testing.
   Grpc.Testing
      TestServiceImpl (class):  EmptyCall, FullDuplexCall, HalfDuplexCall, StreamingInputCall, StreamingOutputCall, UnaryCall
   InteropTestsClientGrpcWeb
      Program (class):  CreateHostBuilder, Main
      Startup (class):  Configure
   InteropTestsGrpcWebClient
      Program (class):  Main
   InteropTestsGrpcWebClient.Infrastructure
      InteropTestInvoker (class):  GetTestNames, InteropTestInvoker, RunTestAsync
      PageLoggerFactory (class):  AddProvider, CreateLogger, Dispose, PageLoggerFactory
   InteropTestsGrpcWebClient.Pages
      Index (class)
      Message (class)
   InteropTestsWebsite
      Program (class):  CreateHostBuilder, Main
      Startup (class):  Configure, ConfigureServices
   Microsoft.AspNetCore.Builder
      GrpcEndpointRouteBuilderExtensions (class):  MapGrpcService
         Provides extension methods for to add gRPC service endpoints.
      GrpcHealthChecksEndpointRouteBuilderExtensions (class):  MapGrpcHealthChecksService
         Provides extension methods for to add gRPC service endpoints.
      GrpcReflectionEndpointRouteBuilderExtensions (class):  MapGrpcReflectionService
         Provides extension methods for to add gRPC service endpoints.
      GrpcServiceEndpointConventionBuilder (class):  Add, Finally
         Builds conventions that will be used for customization of gRPC service instances.
      GrpcWebApplicationBuilderExtensions (class):  UseGrpcWeb
         Provides extension methods for to add gRPC-Web middleware.
      GrpcWebEndpointConventionBuilderExtensions (class):  DisableGrpcWeb, EnableGrpcWeb
         gRPC-Web extension methods for .
      GrpcWebOptions (class)
         Provides programmatic configuration for gRPC-Web.
   Microsoft.Extensions.DependencyInjection
      GrpcClientServiceExtensions (class):  AddGrpcClient
         Extensions methods to configure an for with gRPC.
      GrpcHealthChecksServiceExtensions (class):  AddGrpcHealthChecks
         Extension methods for the gRPC health checks services.
      GrpcHttpClientBuilderExtensions (class):  AddCallCredentials, AddInterceptor, ConfigureChannel, ConfigureGrpcClientCreator
         Extension methods for configuring an .
      GrpcReflectionServiceExtensions (class):  AddGrpcReflection
         Extension methods for the gRPC reflection services.
      GrpcServerHttpClientBuilderExtensions (class):  EnableCallContextPropagation
         Extension methods for configuring an .
      GrpcServicesExtensions (class):  AddGrpc, AddServiceOptions
         Extension methods for the gRPC services.
   QpsWorker.Infrastructure
      ClientRunner (class):  ClientRunner, GetStats, Start, StopAsync
         Helper methods to start client runners for performance testing.
      ClosedLoopInterarrivalTimer (class):  ClosedLoopInterarrivalTimer, WaitForNext, WaitForNextAsync
         Interarrival timer that doesn't wait at all.
      ConfigHelpers (class):  GetConfiguration
      GenericService (class):  DuplexStreamingServerMethod, GenericService
      Histogram (class):  AddObservation, GetSnapshot, Histogram
         Basic implementation of histogram based on grpc/support/histogram.h.
      IInterarrivalTimer (interface):  WaitForNext, WaitForNextAsync
      PoissonInterarrivalTimer (class):  PoissonInterarrivalTimer, WaitForNext, WaitForNextAsync
         Interarrival timer that generates Poisson process load.
      ServerRunner (class):  GetStats, ServerRunner, Start, StopAsync
      Snapshot (class):  Snapshot, ToString
      TimeStats (class):  GetSnapshot, TimeStats
         Snapshottable time statistics.
   QpsWorker.Services
      WorkerServiceImpl (class):  CoreCount, QuitWorker, RunClient, RunServer, WorkerServiceImpl
   global
      BenchmarkServiceImpl (class):  CreateResponse, StreamingBothWays, StreamingCall, StreamingFromClient, StreamingFromServer, UnaryCall

CONSUMER PATHS
   wire into DI  →  GrpcClientServiceExtensions.AddGrpcClient(...)
   wire into DI  →  GrpcEndpointRouteBuilderExtensions.MapGrpcService(...)
   wire into DI  →  GrpcHealthChecksEndpointRouteBuilderExtensions.MapGrpcHealthChecksService(...)
   wire into DI  →  GrpcHealthChecksServiceExtensions.AddGrpcHealthChecks(...)
   wire into DI  →  GrpcReflectionEndpointRouteBuilderExtensions.MapGrpcReflectionService(...)
   wire into DI  →  GrpcReflectionServiceExtensions.AddGrpcReflection(...)

ENTRY POINTS
   gRPC (74)
      CallCredentialsConfigurator.ResetPerCallCredentialState  → DefaultCallCredentialsConfigurator  (src/Grpc.Net.Client/Internal/DefaultCallCredentialsConfigurator.cs:23)
      CallCredentialsConfigurator.SetAsyncAuthInterceptorCredentials  → DefaultCallCredentialsConfigurator  (src/Grpc.Net.Client/Internal/DefaultCallCredentialsConfigurator.cs:23)
      CallCredentialsConfigurator.SetCompositeCredentials  → DefaultCallCredentialsConfigurator  (src/Grpc.Net.Client/Internal/DefaultCallCredentialsConfigurator.cs:23)
      Channel.CalculateHandlerContext  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.CreateCallInvoker  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.CreateChannelRetryThrottling  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.CreateInternalHttpInvoker  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.CreateMethodInfo  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.CreateServiceConfigMethods  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.DebuggerToString  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.Dispose  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.FinishActiveCall  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.ForAddress  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.ForAddress  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.ForAddress  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.ForAddress  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.GetActiveCalls  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.GetCachedGrpcMethodInfo  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.GetRandomNumber  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      Channel.IsHttpOrHttpsAddress  → GrpcChannel  (src/Grpc.Net.Client/GrpcChannel.cs:40)
      … and 54 more (grpc entries)

PACKAGES
   Web/API:  Grpc.AspNetCore, Grpc.AspNetCore.Web, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Components.WebAssembly, Microsoft.AspNetCore.Components.WebAssembly.DevServer, Microsoft.AspNetCore.Components.WebAssembly.Server
   Other:  Google.Api.CommonProtos 2.16.0, Google.Apis.Auth 1.70.0, Google.Protobuf 3.31.1, Grpc.Tools 2.80.0, Microsoft.Bcl.AsyncInterfaces 8.0.0, Microsoft.Extensions.Http, Microsoft.Extensions.Logging.Abstractions, System.Diagnostics.DiagnosticSource 6.0.1 … (11 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus GrpcClientServiceExtensions)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 1583ms |
| GenericExtraction | 4068ms |
| SignalSealing | 0ms |
| SpecificExtraction | 6684ms |
| Compression | 186ms |
| **Total** | **23738ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 4065ms | 879 | 405 |
| DiRegistrationExtractor | 4062ms | 1 | 405 |
| CallGraphExtractor | 3718ms | 0 | 0 |
| EndpointExtractor | 2959ms | 0 | 247 |
| ProgramCsFlowExtractor | 1842ms | 0 | 61 |
| ProjectStructure | 1238ms | 0 | 0 |
| CliCommandExtractor | 1159ms | 0 | 139 |
| SourceBodyExtractor | 984ms | 0 | 0 |
| RazorPagesExtractor | 924ms | 0 | 243 |
| GrpcServiceExtractor | 848ms | 0 | 241 |
| GrpcClientExtractor | 776ms | 0 | 225 |
| ControllerActionExtractor | 626ms | 0 | 168 |
| InMemoryEventBusExtractor | 613ms | 0 | 161 |
| BodyFactsExtractor | 598ms | 0 | 0 |
| BlazorEntryExtractor | 563ms | 0 | 146 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 946 | 379 |
| Resolves | 89 | 6 |

_828 files · 106 projects_
