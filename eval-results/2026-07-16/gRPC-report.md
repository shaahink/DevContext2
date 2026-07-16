# REPORT
**Grpc.DotNet**

Style: SampleCollection
_30 projects  ·  6 GrpcService  ·  net10.0, net10.0;net9.0;net8.0;net462 + blazor + controllers + minimal-apis + razor-pages + grpc + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 828 |
| Projects | 106 |
| Nodes | 1347 |
| Edges | 1130 |
| Entries | 6 |
| With target | 6/6 |
| Deep spine (>=2) | 6/6 (100%) |
| Verified edges | 66% |
| Analyzed in | 24.2s |

## Top Flows

1. **ServerReflection.ServerReflectionInfo** → `ResponseStreamWrapper.MoveNext` *(GrpcService)*
2. **ServerReflection.ServerReflectionInfo** → `ResponseStreamWrapper.MoveNext` *(GrpcService)*
3. **Health.Check** → `HealthServiceImpl.Check` *(GrpcService)*
4. **Health.Check** → `HealthServiceImpl.Check` *(GrpcService)*
5. **Health.Watch** → `HealthServiceImpl.Watch` *(GrpcService)*
6. **Health.Watch** → `HealthServiceImpl.Watch` *(GrpcService)*

### Trace 1: ServerReflection.ServerReflectionInfo

TRACE  ServerReflection.ServerReflectionInfo
       src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:26
       Grpc.Reflection
▸ ENTRY  ServerReflection.ServerReflectionInfo  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:26)
   ├─ call ReflectionV1ServiceImpl.ServerReflectionInfo  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:26)
   │      public override async Task ServerReflectionInfo(IAsyncStreamReader<ServerReflectionRequest> requestStream, IServerStreamWriter<ServerReflectionResponse> responseStream, ServerCallContext context)
   │      while (await requestStream.MoveNext().ConfigureAwait(false))
   │      var response = ProcessRequest(requestStream.Current);
   │  ├─ call IAsyncStreamReader  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:58) [approx]
   │  │      /// <summary>
   │  │      /// A stream of messages to be read.
   │  │      /// Messages can be awaited <c>await reader.MoveNext()</c>, that returns <c>true</c>
   │  ├─ call IServerStreamWriter  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:61) [approx]
   │  │      /// <summary>
   │  │      /// A writable stream of messages that is used in server-side handlers.
   │  │      /// </summary>
   │  ├─ call IClientStreamWriter.WriteAsync  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:61) [verified]
   │  ├─ call ReflectionV1ServiceImpl.ProcessRequest  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:60) [verified]
   │  │      ServerReflectionResponse ProcessRequest(ServerReflectionRequest request)
   │  │      var requestCase = request.MessageRequestCase switch
   │  │      ServerReflectionRequest.MessageRequestOneofCase.FileByFilename => ReflectionServiceCore.RequestCase.FileByFilename,
   │  │  ├─ call ReflectionV1ServiceImpl.ToResponse  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:83) [verified]
   │  │  │      static ServerReflectionResponse ToResponse(ReflectionServiceCore.Response coreResponse)
   │  │  │      switch (coreResponse.Case)
   │  │  │      case ReflectionServiceCore.ResponseCase.FileDescriptorResponse:
   │  │  └─ call ReflectionServiceCore.ProcessRequest  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:82) [verified]
   │  │         public Response ProcessRequest(RequestCase requestCase, string? stringValue)
   │  │         switch (requestCase)
   │  │         case RequestCase.FileByFilename:
   │  │     ├─ call ReflectionServiceCore.CreateErrorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:93) [verified]
   │  │     │      static Response CreateErrorResponse(StatusCode status, string message)
   │  │     │      return Response.Error((int)status, message);
   │  │     │  └─ call Response.Error  (src/Grpc.Reflection/ReflectionServiceCore.cs:134) [verified]
   │  │     │         public static Response Error(int errorCode, string errorMessage)
   │  │     │         => new Response(ResponseCase.ErrorResponse, errorCode: errorCode, errorMessage: errorMessage);
   │  │     ├─ call ReflectionServiceCore.ListServices  (src/Grpc.Reflection/ReflectionServiceCore.cs:91) [verified]
   │  │     │      Response ListServices()
   │  │     │      return Response.Services(services);
   │  │     │  └─ call Response.Services  (src/Grpc.Reflection/ReflectionServiceCore.cs:129) [verified]
   │  │     │         public static Response Services(IEnumerable<string> serviceNames)
   │  │     │         => new Response(ResponseCase.ListServicesResponse, serviceNames: serviceNames);
   │  │     ├─ call ReflectionServiceCore.FileContainingSymbol  (src/Grpc.Reflection/ReflectionServiceCore.cs:89) [verified]
   │  │     │      Response FileContainingSymbol(string symbol)
   │  │     │      FileDescriptor? file = symbolRegistry.FileContainingSymbol(symbol);
   │  │     │      if (file is null)
   │  │     │  ├─ call ReflectionServiceCore.CreateFileDescriptorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:116) [verified]
   │  │     │  │      Response CreateFileDescriptorResponse(FileDescriptor file)
   │  │     │  │      var transitiveDependencies = new HashSet<FileDescriptor>();
   │  │     │  │      CollectTransitiveDependencies(file, transitiveDependencies);
   │  │     │  │  (stopped at depth 5; 2 branches omitted)
   │  │     │  ├─ call ReflectionServiceCore.CreateErrorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:113) [verified]
   │  │     │  │      static Response CreateErrorResponse(StatusCode status, string message)
   │  │     │  │      return Response.Error((int)status, message);
   │  │     │  │  (stopped at depth 5; 1 branch omitted)
   │  │     │  └─ call SymbolRegistry.FileContainingSymbol  (src/Grpc.Reflection/ReflectionServiceCore.cs:110) [verified]
   │  │     │         public FileDescriptor? FileContainingSymbol(string symbol)
   │  │     │         filesBySymbol.TryGetValue(symbol, out var file);
   │  │     │         return file;
   │  │     └─ call ReflectionServiceCore.FileByFilename  (src/Grpc.Reflection/ReflectionServiceCore.cs:87) [verified]
   │  │            Response FileByFilename(string filename)
   │  │            FileDescriptor? file = symbolRegistry.FileByName(filename);
   │  │            if (file is null)
   │  │        ├─ call ReflectionServiceCore.CreateFileDescriptorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:105) [verified]
   │  │        │      Response CreateFileDescriptorResponse(FileDescriptor file)
   │  │        │      var transitiveDependencies = new HashSet<FileDescriptor>();
   │  │        │      CollectTransitiveDependencies(file, transitiveDependencies);
   │  │        │  (stopped at depth 5; 2 branches omitted)
   │  │        ├─ call ReflectionServiceCore.CreateErrorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:102) [verified]
   │  │        │      static Response CreateErrorResponse(StatusCode status, string message)
   │  │        │      return Response.Error((int)status, message);
   │  │        │  (stopped at depth 5; 1 branch omitted)
   │  │        └─ call SymbolRegistry.FileByName  (src/Grpc.Reflection/ReflectionServiceCore.cs:99) [verified]
   │  │               public FileDescriptor? FileByName(string filename)
   │  │               filesByName.TryGetValue(filename, out var file);
   │  │               return file;
   │  └─ call MoveNext  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:58) [verified]
   │         public async Task<bool> MoveNext(CancellationToken cancellationToken)
   │         var result = await _inner.MoveNext(cancellationToken);
   │         if (!result && !_disposed)
   │     └─ call IAsyncStreamReader  (src/Grpc.AspNetCore.Server.ClientFactory/ContextPropagationInterceptor.cs:251) [approx]
   │            /// <summary>
   │            /// A stream of messages to be read.
   │            /// Messages can be awaited <c>await reader.MoveNext()</c>, that returns <c>true</c>
   └─ call ReflectionServiceImpl.ServerReflectionInfo  (src/Grpc.Reflection/ReflectionServiceImpl.cs:26)
          public override async Task ServerReflectionInfo(IAsyncStreamReader<ServerReflectionRequest> requestStream, IServerStreamWriter<ServerReflectionResponse> responseStream, ServerCallContext context)
          while (await requestStream.MoveNext().ConfigureAwait(false))
          var response = ProcessRequest(requestStream.Current);
      ├─ call IAsyncStreamReader  (src/Grpc.Reflection/ReflectionServiceImpl.cs:58) [approx]
      │      /// <summary>
      │      /// A stream of messages to be read.
      │      /// Messages can be awaited <c>await reader.MoveNext()</c>, that returns <c>true</c>
      ├─ call IServerStreamWriter  (src/Grpc.Reflection/ReflectionServiceImpl.cs:61) [approx]
      │      /// <summary>
      │      /// A writable stream of messages that is used in server-side handlers.
      │      /// </summary>
      ├─ call IClientStreamWriter.WriteAsync  (src/Grpc.Reflection/ReflectionServiceImpl.cs:61) [verified]
      ├─ call ReflectionServiceImpl.ProcessRequest  (src/Grpc.Reflection/ReflectionServiceImpl.cs:60) [verified]
      │      ServerReflectionResponse ProcessRequest(ServerReflectionRequest request)
      │      var requestCase = request.MessageRequestCase switch
      │      ServerReflectionRequest.MessageRequestOneofCase.FileByFilename => ReflectionServiceCore.RequestCase.FileByFilename,
      │  ├─ call ReflectionServiceImpl.ToResponse  (src/Grpc.Reflection/ReflectionServiceImpl.cs:83) [verified]
      │  │      static ServerReflectionResponse ToResponse(ReflectionServiceCore.Response coreResponse)
      │  │      switch (coreResponse.Case)
      │  │      case ReflectionServiceCore.ResponseCase.FileDescriptorResponse:
      │  └─ call ReflectionServiceCore.ProcessRequest  (src/Grpc.Reflection/ReflectionServiceImpl.cs:82) [verified]
      │         public Response ProcessRequest(RequestCase requestCase, string? stringValue)
      │         switch (requestCase)
      │         case RequestCase.FileByFilename:
      │     (stopped at depth 3; 4 branches omitted)
      └─ call MoveNext  (src/Grpc.Reflection/ReflectionServiceImpl.cs:58) [verified]
             public async Task<bool> MoveNext(CancellationToken cancellationToken)
             var result = await _inner.MoveNext(cancellationToken);
             if (!result && !_disposed)
         (stopped at depth 2; 1 branch omitted)

---

### Trace 2: ServerReflection.ServerReflectionInfo

TRACE  ServerReflection.ServerReflectionInfo
       src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:26
       Grpc.Reflection
▸ ENTRY  ServerReflection.ServerReflectionInfo  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:26)
   ├─ call ReflectionV1ServiceImpl.ServerReflectionInfo  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:26)
   │      public override async Task ServerReflectionInfo(IAsyncStreamReader<ServerReflectionRequest> requestStream, IServerStreamWriter<ServerReflectionResponse> responseStream, ServerCallContext context)
   │      while (await requestStream.MoveNext().ConfigureAwait(false))
   │      var response = ProcessRequest(requestStream.Current);
   │  ├─ call IAsyncStreamReader  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:58) [approx]
   │  │      /// <summary>
   │  │      /// A stream of messages to be read.
   │  │      /// Messages can be awaited <c>await reader.MoveNext()</c>, that returns <c>true</c>
   │  ├─ call IServerStreamWriter  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:61) [approx]
   │  │      /// <summary>
   │  │      /// A writable stream of messages that is used in server-side handlers.
   │  │      /// </summary>
   │  ├─ call IClientStreamWriter.WriteAsync  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:61) [verified]
   │  ├─ call ReflectionV1ServiceImpl.ProcessRequest  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:60) [verified]
   │  │      ServerReflectionResponse ProcessRequest(ServerReflectionRequest request)
   │  │      var requestCase = request.MessageRequestCase switch
   │  │      ServerReflectionRequest.MessageRequestOneofCase.FileByFilename => ReflectionServiceCore.RequestCase.FileByFilename,
   │  │  ├─ call ReflectionV1ServiceImpl.ToResponse  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:83) [verified]
   │  │  │      static ServerReflectionResponse ToResponse(ReflectionServiceCore.Response coreResponse)
   │  │  │      switch (coreResponse.Case)
   │  │  │      case ReflectionServiceCore.ResponseCase.FileDescriptorResponse:
   │  │  └─ call ReflectionServiceCore.ProcessRequest  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:82) [verified]
   │  │         public Response ProcessRequest(RequestCase requestCase, string? stringValue)
   │  │         switch (requestCase)
   │  │         case RequestCase.FileByFilename:
   │  │     ├─ call ReflectionServiceCore.CreateErrorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:93) [verified]
   │  │     │      static Response CreateErrorResponse(StatusCode status, string message)
   │  │     │      return Response.Error((int)status, message);
   │  │     │  └─ call Response.Error  (src/Grpc.Reflection/ReflectionServiceCore.cs:134) [verified]
   │  │     │         public static Response Error(int errorCode, string errorMessage)
   │  │     │         => new Response(ResponseCase.ErrorResponse, errorCode: errorCode, errorMessage: errorMessage);
   │  │     ├─ call ReflectionServiceCore.ListServices  (src/Grpc.Reflection/ReflectionServiceCore.cs:91) [verified]
   │  │     │      Response ListServices()
   │  │     │      return Response.Services(services);
   │  │     │  └─ call Response.Services  (src/Grpc.Reflection/ReflectionServiceCore.cs:129) [verified]
   │  │     │         public static Response Services(IEnumerable<string> serviceNames)
   │  │     │         => new Response(ResponseCase.ListServicesResponse, serviceNames: serviceNames);
   │  │     ├─ call ReflectionServiceCore.FileContainingSymbol  (src/Grpc.Reflection/ReflectionServiceCore.cs:89) [verified]
   │  │     │      Response FileContainingSymbol(string symbol)
   │  │     │      FileDescriptor? file = symbolRegistry.FileContainingSymbol(symbol);
   │  │     │      if (file is null)
   │  │     │  ├─ call ReflectionServiceCore.CreateFileDescriptorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:116) [verified]
   │  │     │  │      Response CreateFileDescriptorResponse(FileDescriptor file)
   │  │     │  │      var transitiveDependencies = new HashSet<FileDescriptor>();
   │  │     │  │      CollectTransitiveDependencies(file, transitiveDependencies);
   │  │     │  │  (stopped at depth 5; 2 branches omitted)
   │  │     │  ├─ call ReflectionServiceCore.CreateErrorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:113) [verified]
   │  │     │  │      static Response CreateErrorResponse(StatusCode status, string message)
   │  │     │  │      return Response.Error((int)status, message);
   │  │     │  │  (stopped at depth 5; 1 branch omitted)
   │  │     │  └─ call SymbolRegistry.FileContainingSymbol  (src/Grpc.Reflection/ReflectionServiceCore.cs:110) [verified]
   │  │     │         public FileDescriptor? FileContainingSymbol(string symbol)
   │  │     │         filesBySymbol.TryGetValue(symbol, out var file);
   │  │     │         return file;
   │  │     └─ call ReflectionServiceCore.FileByFilename  (src/Grpc.Reflection/ReflectionServiceCore.cs:87) [verified]
   │  │            Response FileByFilename(string filename)
   │  │            FileDescriptor? file = symbolRegistry.FileByName(filename);
   │  │            if (file is null)
   │  │        ├─ call ReflectionServiceCore.CreateFileDescriptorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:105) [verified]
   │  │        │      Response CreateFileDescriptorResponse(FileDescriptor file)
   │  │        │      var transitiveDependencies = new HashSet<FileDescriptor>();
   │  │        │      CollectTransitiveDependencies(file, transitiveDependencies);
   │  │        │  (stopped at depth 5; 2 branches omitted)
   │  │        ├─ call ReflectionServiceCore.CreateErrorResponse  (src/Grpc.Reflection/ReflectionServiceCore.cs:102) [verified]
   │  │        │      static Response CreateErrorResponse(StatusCode status, string message)
   │  │        │      return Response.Error((int)status, message);
   │  │        │  (stopped at depth 5; 1 branch omitted)
   │  │        └─ call SymbolRegistry.FileByName  (src/Grpc.Reflection/ReflectionServiceCore.cs:99) [verified]
   │  │               public FileDescriptor? FileByName(string filename)
   │  │               filesByName.TryGetValue(filename, out var file);
   │  │               return file;
   │  └─ call MoveNext  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:58) [verified]
   │         public async Task<bool> MoveNext(CancellationToken cancellationToken)
   │         var result = await _inner.MoveNext(cancellationToken);
   │         if (!result && !_disposed)
   │     └─ call IAsyncStreamReader  (src/Grpc.AspNetCore.Server.ClientFactory/ContextPropagationInterceptor.cs:251) [approx]
   │            /// <summary>
   │            /// A stream of messages to be read.
   │            /// Messages can be awaited <c>await reader.MoveNext()</c>, that returns <c>true</c>
   └─ call ReflectionServiceImpl.ServerReflectionInfo  (src/Grpc.Reflection/ReflectionServiceImpl.cs:26)
          public override async Task ServerReflectionInfo(IAsyncStreamReader<ServerReflectionRequest> requestStream, IServerStreamWriter<ServerReflectionResponse> responseStream, ServerCallContext context)
          while (await requestStream.MoveNext().ConfigureAwait(false))
          var response = ProcessRequest(requestStream.Current);
      ├─ call IAsyncStreamReader  (src/Grpc.Reflection/ReflectionServiceImpl.cs:58) [approx]
      │      /// <summary>
      │      /// A stream of messages to be read.
      │      /// Messages can be awaited <c>await reader.MoveNext()</c>, that returns <c>true</c>
      ├─ call IServerStreamWriter  (src/Grpc.Reflection/ReflectionServiceImpl.cs:61) [approx]
      │      /// <summary>
      │      /// A writable stream of messages that is used in server-side handlers.
      │      /// </summary>
      ├─ call IClientStreamWriter.WriteAsync  (src/Grpc.Reflection/ReflectionServiceImpl.cs:61) [verified]
      ├─ call ReflectionServiceImpl.ProcessRequest  (src/Grpc.Reflection/ReflectionServiceImpl.cs:60) [verified]
      │      ServerReflectionResponse ProcessRequest(ServerReflectionRequest request)
      │      var requestCase = request.MessageRequestCase switch
      │      ServerReflectionRequest.MessageRequestOneofCase.FileByFilename => ReflectionServiceCore.RequestCase.FileByFilename,
      │  ├─ call ReflectionServiceImpl.ToResponse  (src/Grpc.Reflection/ReflectionServiceImpl.cs:83) [verified]
      │  │      static ServerReflectionResponse ToResponse(ReflectionServiceCore.Response coreResponse)
      │  │      switch (coreResponse.Case)
      │  │      case ReflectionServiceCore.ResponseCase.FileDescriptorResponse:
      │  └─ call ReflectionServiceCore.ProcessRequest  (src/Grpc.Reflection/ReflectionServiceImpl.cs:82) [verified]
      │         public Response ProcessRequest(RequestCase requestCase, string? stringValue)
      │         switch (requestCase)
      │         case RequestCase.FileByFilename:
      │     (stopped at depth 3; 4 branches omitted)
      └─ call MoveNext  (src/Grpc.Reflection/ReflectionServiceImpl.cs:58) [verified]
             public async Task<bool> MoveNext(CancellationToken cancellationToken)
             var result = await _inner.MoveNext(cancellationToken);
             if (!result && !_disposed)
         (stopped at depth 2; 1 branch omitted)

---

### Trace 3: Health.Check

TRACE  Health.Check
       src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:30
       Grpc.AspNetCore.HealthChecks
▸ ENTRY  Health.Check  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:30)
   ├─ call HealthServiceIntegration.Check  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:30)
   │      public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
   │      if (!_grpcHealthCheckOptions.UseHealthChecksCache)
   │      return GetHealthCheckResponseAsync(request.Service, throwOnNotFound: true, context.CancellationToken);
   │  ├─ call HealthServiceImpl  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:60) [approx]
   │  │      /// <summary>
   │  │      /// Implementation of a simple Health service. Useful for health checking.
   │  │      ///
   │  ├─ call HealthServiceImpl.Check  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:60) [verified]
   │  │      public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
   │  │      HealthCheckResponse response = GetHealthCheckResponse(request.Service, throwOnNotFound: true);
   │  │      return Task.FromResult(response);
   │  │  └─ call HealthServiceImpl.GetHealthCheckResponse  (src/Grpc.HealthCheck/HealthServiceImpl.cs:116) [verified]
   │  │         private HealthCheckResponse GetHealthCheckResponse(string service, bool throwOnNotFound)
   │  │         HealthCheckResponse response;
   │  │         lock (statusLock)
   │  └─ call GetHealthCheckResponseAsync  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:56) [verified]
   │         private async Task<HealthCheckResponse> GetHealthCheckResponseAsync(string service, bool throwOnNotFound, CancellationToken cancellationToken)
   │         // Match Check behavior from Grpc.HealthCheck.HealthServiceImpl.
   │         HealthCheckResponse.Types.ServingStatus status;
   │     ├─ call GrpcHealthChecksOptions  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:148) [approx]
   │     │      /// <summary>
   │     │      /// Contains options for the gRPC health checks service.
   │     │      /// </summary>
   │     ├─ call HealthChecksStatusHelpers.GetStatus  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:178) [verified]
   │     │      public static (HealthCheckResponse.Types.ServingStatus status, int resultCount) GetStatus(IEnumerable<KeyValuePair<string, HealthReportEntry>> results)
   │     │      var resultCount = 0;
   │     │      var resolvedStatus = HealthCheckResponse.Types.ServingStatus.Unknown;
   │     ├─ call ServiceMapping.Predicate  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:173) [verified]
   │     │      [Obsolete($"This member is obsolete and will be removed in the future. Use {nameof(HealthCheckPredicate)} to map service names to .NET health checks.")]
   │     │      public Func<HealthResult, bool>? Predicate { get; }
   │     ├─ call IGrpcCall.Where  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:170) [verified]
   │     ├─ call ServiceMapping.HealthCheckPredicate  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:157) [verified]
   │     │      public Func<HealthCheckMapContext, bool>? HealthCheckPredicate { get; }
   │     └─ call TryGetServiceMapping  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:148) [verified]
   │            internal bool TryGetServiceMapping(string name, [NotNullWhen(true)] out ServiceMapping? serviceMapping)
   │            return _mappings.TryGetValue(name, out serviceMapping);
   │        ├─ call ServiceMappingKeyedCollection  (src/Grpc.AspNetCore.HealthChecks/ServiceMappingCollection.cs:49) [approx]
   │        │      private sealed class ServiceMappingKeyedCollection : KeyedCollection<string, ServiceMapping>
   │        │      protected override string GetKeyForItem(ServiceMapping item)
   │        │      return item.Name;
   │        └─ call ServiceMappingKeyedCollection.TryGetValue  (src/Grpc.AspNetCore.HealthChecks/ServiceMappingCollection.cs:49) [approx]
   └─ call HealthServiceImpl.Check  (src/Grpc.HealthCheck/HealthServiceImpl.cs:35)
          public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
          HealthCheckResponse response = GetHealthCheckResponse(request.Service, throwOnNotFound: true);
          return Task.FromResult(response);
      (stopped at depth 1; 1 branch omitted)

---

## Insights

_4 info · 3 notable_

### **NOTABLE**: Config without defaults: 3 consumed keys have no appsettings default
*(Risk)*

- enableCertAuth
- enableGrpcWeb
- Http3Port

### **NOTABLE**: Internal hubs: 5 heavily-referenced internal types
*(Topology)*

- Assert (178 refs)
- ExecutionContextLogger (101 refs)
- ResponseStreamWrapper (44 refs)
- GrpcProtocolHelpers (41 refs)
- HttpContextServerCallContext (39 refs)

### **NOTABLE**: Most depended-upon: Grpc.AspNetCore (5 dependents) · Grpc.Core.Api (5 dependents) · Grpc.Net.Client (5 dependents)
*(Topology)*

- Grpc.AspNetCore (5 dependents)
- Grpc.Core.Api (5 dependents)
- Grpc.Net.Client (5 dependents)

### _INFO_: Public surface: 20 interfaces, 488 classes (518 total public types)
*(Shape)*

- 20 interfaces
- 488 classes

### _INFO_: Entry targets resolved 6/6 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 272 Extension · 59 Singleton · 7 Transient · 6 Scoped (344 total)
*(Wiring)*

### _INFO_: Wiring hubs: List (108) · ServiceCollection (67) · IAsyncStreamReader (66) · IServerStreamWriter (62) · CallInvoker (23)
*(Wiring)*

- List (108)
- ServiceCollection (67)
- IAsyncStreamReader (66)
- IServerStreamWriter (62)
- CallInvoker (23)

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
      … and 4 more (use --format json for the full surface)
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
      … and 35 more (use --format json for the full surface)
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
   … and 8 more namespaces (use --format json for the full surface)

CONSUMER PATHS
   wire into DI  →  GrpcClientServiceExtensions.AddGrpcClient(...)
   wire into DI  →  GrpcEndpointRouteBuilderExtensions.MapGrpcService(...)
   wire into DI  →  GrpcHealthChecksEndpointRouteBuilderExtensions.MapGrpcHealthChecksService(...)
   wire into DI  →  GrpcHealthChecksServiceExtensions.AddGrpcHealthChecks(...)
   wire into DI  →  GrpcReflectionEndpointRouteBuilderExtensions.MapGrpcReflectionService(...)
   wire into DI  →  GrpcReflectionServiceExtensions.AddGrpcReflection(...)

ENTRY POINTS
   gRPC (6)
      Health.Check  → HealthServiceImpl.Check  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:30)
      Health.Check  → HealthServiceImpl.Check  (src/Grpc.HealthCheck/HealthServiceImpl.cs:35)
      Health.Watch  → HealthServiceImpl.Watch  (src/Grpc.AspNetCore.HealthChecks/Internal/HealthServiceIntegration.cs:30)
      Health.Watch  → HealthServiceImpl.Watch  (src/Grpc.HealthCheck/HealthServiceImpl.cs:35)
      ServerReflection.ServerReflectionInfo  → ResponseStreamWrapper.MoveNext  (src/Grpc.Reflection/ReflectionV1ServiceImpl.cs:26)
      ServerReflection.ServerReflectionInfo  → ResponseStreamWrapper.MoveNext  (src/Grpc.Reflection/ReflectionServiceImpl.cs:26)

PACKAGES
   Web/API:  Grpc.AspNetCore, Grpc.AspNetCore.Web, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Components.WebAssembly, Microsoft.AspNetCore.Components.WebAssembly.DevServer, Microsoft.AspNetCore.Components.WebAssembly.Server
   Other:  Google.Api.CommonProtos 2.16.0, Google.Apis.Auth 1.70.0, Google.Protobuf 3.31.1, Grpc.Tools 2.80.0, Microsoft.Bcl.AsyncInterfaces 8.0.0, Microsoft.Extensions.Http, Microsoft.Extensions.Logging.Abstractions, System.Diagnostics.DiagnosticSource 6.0.1 … (11 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus GrpcClientServiceExtensions)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 1832ms |
| GenericExtraction | 4519ms |
| SignalSealing | 0ms |
| SpecificExtraction | 5432ms |
| Compression | 122ms |
| **Total** | **24164ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| ProgramCsFlowExtractor | 4515ms | 879 | 437 |
| SyntaxStructureExtractor | 4479ms | 879 | 344 |
| DiRegistrationExtractor | 4476ms | 0 | 344 |
| CallGraphExtractor | 3197ms | 0 | 0 |
| EndpointExtractor | 2229ms | 0 | 167 |
| ProjectStructure | 1509ms | 0 | 0 |
| SourceBodyExtractor | 1340ms | 0 | 0 |
| BlazorEntryExtractor | 819ms | 0 | 163 |
| CliCommandExtractor | 756ms | 0 | 66 |
| RazorPagesExtractor | 680ms | 0 | 161 |
| InMemoryEventBusExtractor | 626ms | 0 | 158 |
| GrpcServiceExtractor | 617ms | 0 | 157 |
| ControllerActionExtractor | 571ms | 0 | 113 |
| BodyFactsExtractor | 546ms | 0 | 0 |
| GrpcClientExtractor | 459ms | 0 | 100 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 1068 | 380 |
| Resolves | 62 | 6 |

_828 files · 106 projects_
