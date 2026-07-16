# REPORT
**Ocelot**

Style: SampleCollection
_2 projects  ·  3 HttpEndpoint, 1 HostedService  ·  net8.0;net9.0;net10.0 + controllers + minimal-apis + serilog + fluentvalidation + gateway_

## Stats

| Metric | Value |
|--------|-------|
| Files | 754 |
| Projects | 21 |
| Nodes | 1540 |
| Edges | 1283 |
| Entries | 4 |
| With target | 4/4 |
| Deep spine (>=2) | 3/4 (75%) |
| Verified edges | 58% |
| Analyzed in | 22.8s |

## Top Flows

1. **POST /configuration** → `FileAndInternalConfigurationSetter.SetAsync` *(HttpEndpoint)*
2. **DELETE /outputcache/{region}** → `IOcelotCache` *(HttpEndpoint)*
3. **GET /configuration** → `IFileConfigurationRepository` *(HttpEndpoint)*
4. **TPoller** → `TPoller` *(HostedService)*

### Trace 1: POST /configuration

TRACE  POST /configuration
       src/Administration/FileConfigurationController.cs:42
       Ocelot
▸ ENTRY  POST /configuration  (src/Administration/FileConfigurationController.cs:42)
   └─ call FileConfigurationController.Post  (src/Administration/FileConfigurationController.cs:42)
          [HttpPost]
          public async Task<ActionResult> Post([FromBody] FileConfiguration configuration, CancellationToken cancellationToken)
          try
      ├─ call IFileConfigurationSetter  (src/Administration/FileConfigurationController.cs:47) [approx]
      │      /// <summary>
      │      /// Feature: <see cref="Features.AddOcelotConfigurationRepository(IServiceCollection)"/>.
      │      /// </summary>
      │  └─ di FileAndInternalConfigurationSetter  (src/DependencyInjection/Features.cs:34)
      │         /// <summary>
      │         /// Features: <see cref="Features.AddOcelotConfigurationRepository(IServiceCollection)"/> and <see cref="IOcelotBuilder.AddConfigurationPoller()"/>.
      │         /// </summary>
      └─ call SetAsync  (src/Administration/FileConfigurationController.cs:47) [verified]
             public async Task SetAsync(FileConfiguration configuration, CancellationToken cancellationToken = default)
             await _repo.SetAsync(configuration, cancellationToken);
             var config = await _configCreator.Create(configuration);
         ├─ call IFileConfigurationRepository  (src/Configuration/Repository/FileAndInternalConfigurationSetter.cs:33) [approx]
         │      /// <summary>
         │      /// Features: <see cref="Features.AddOcelotConfigurationRepository(IServiceCollection)"/> and <see cref="IOcelotBuilder.AddConfigurationPoller()"/>.
         │      /// </summary>
         │  ├─ di TRepository  (src/DependencyInjection/OcelotBuilder.cs:203)
         │  └─ di DiskFileConfigurationRepository  (src/DependencyInjection/Features.cs:34)
         │         public class DiskFileConfigurationRepository : IFileConfigurationRepository, IDisposable
         │         private FileInfo _ocelotFile;
         │         private FileInfo _environmentFile;
         ├─ call IInternalConfigurationCreator  (src/Configuration/Repository/FileAndInternalConfigurationSetter.cs:35) [approx]
         │      public interface IInternalConfigurationCreator
         │      Task<Response<IInternalConfiguration>> Create(FileConfiguration fileConfiguration);
         │  └─ di FileInternalConfigurationCreator [approx]
         │         public class FileInternalConfigurationCreator : IInternalConfigurationCreator
         │         private readonly IConfigurationValidator _configurationValidator;
         │         private readonly IConfigurationCreator _configCreator;
         ├─ call IInternalConfigurationRepository  (src/Configuration/Repository/FileAndInternalConfigurationSetter.cs:39) [approx]
         │      public interface IInternalConfigurationRepository
         │      IInternalConfiguration Get();
         │      string AddOrReplace(IInternalConfiguration configuration);
         │  └─ di InMemoryInternalConfigurationRepository  (src/DependencyInjection/Features.cs:34)
         │         public class InMemoryInternalConfigurationRepository : IInternalConfigurationRepository
         │         #if NET9_0_OR_GREATER
         │         private static readonly Lock Locker = new();
         ├─ call AddOrReplace  (src/Configuration/Repository/FileAndInternalConfigurationSetter.cs:39) [verified]
         │      public string AddOrReplace(IInternalConfiguration internalConfiguration)
         │      lock (Locker)
         │      _internalConfiguration = internalConfiguration;
         │  ├─ call IOcelotConfigurationChangeTokenSource  (src/Configuration/Repository/InMemoryInternalConfigurationRepository.cs:36) [approx]
         │  │      /// <summary>
         │  │      /// <see cref="IChangeToken" /> source which is activated when Ocelot's configuration is changed.
         │  │      /// </summary>
         │  │  └─ di OcelotConfigurationChangeTokenSource [approx]
         │  │         public class OcelotConfigurationChangeTokenSource : IOcelotConfigurationChangeTokenSource
         │  │         private readonly OcelotConfigurationChangeToken _changeToken = new();
         │  │         public IChangeToken ChangeToken => _changeToken;
         │  └─ call Activate  (src/Configuration/Repository/InMemoryInternalConfigurationRepository.cs:36) [verified]
         │         public void Activate()
         │         _changeToken.Activate();
         │     ├─ call OcelotConfigurationChangeToken  (src/Configuration/ChangeTracking/OcelotConfigurationChangeTokenSource.cs:13) [approx]
         │     │      public class OcelotConfigurationChangeToken : IChangeToken
         │     │      public const double PollingIntervalSeconds = 1;
         │     │      private readonly List<CallbackWrapper> _callbacks = new();
         │     └─ call OcelotConfigurationChangeToken.Activate  (src/Configuration/ChangeTracking/OcelotConfigurationChangeTokenSource.cs:13) [verified]
         │            public void Activate()
         │            lock (_lock)
         │            _timeChanged = DateTime.UtcNow;
         │        (stopped at depth 5; 1 branch omitted)
         └─ call Create  (src/Configuration/Repository/FileAndInternalConfigurationSetter.cs:35) [verified]
                public async Task<Response<IInternalConfiguration>> Create(FileConfiguration fileConfiguration)
                var response = await _configurationValidator.IsValid(fileConfiguration);
                if (response.Data.IsError)
            (2 more branches omitted beyond fan-out)
            ├─ call IConfigurationValidator  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:32) [approx]
            │      public interface IConfigurationValidator
            │      Task<Response<ConfigurationValidationResult>> IsValid(FileConfiguration configuration);
            │  └─ di FileConfigurationFluentValidator  (src/DependencyInjection/Features.cs:21)
            │         /// <summary>Validation of a <see cref="FileConfiguration"/> objects.</summary>
            │         public partial class FileConfigurationFluentValidator : AbstractValidator<FileConfiguration>, IConfigurationValidator
            │         private readonly List<ServiceDiscoveryFinderDelegate> _serviceDiscoveryFinderDelegates;
            ├─ call IRoutesCreator  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:39) [approx]
            │      public interface IRoutesCreator
            │      IReadOnlyList<Route> Create(FileConfiguration fileConfiguration);
            │      /// <summary>
            │  └─ di StaticRoutesCreator [approx]
            │         public class StaticRoutesCreator : IRoutesCreator
            │         private readonly ILoadBalancerOptionsCreator _loadBalancerOptionsCreator;
            │         private readonly IClaimsToThingCreator _claimsToThingCreator;
            ├─ call IAggregatesCreator  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:41) [approx]
            │      public interface IAggregatesCreator
            │      List<Route> Create(FileConfiguration fileConfiguration, IReadOnlyList<Route> routes);
            │  └─ di AggregatesCreator [approx]
            │         public class AggregatesCreator : IAggregatesCreator
            │         private readonly IUpstreamTemplatePatternCreator _creator;
            │         private readonly IUpstreamHeaderTemplatePatternCreator _headerCreator;
            ├─ call IDynamicsCreator  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:43) [approx]
            │      public interface IDynamicsCreator
            │      IReadOnlyList<Route> Create(FileConfiguration fileConfiguration);
            │      /// <summary>
            │  └─ di DynamicRoutesCreator [approx]
            │         public class DynamicRoutesCreator : IDynamicsCreator
            │         private readonly IAuthenticationOptionsCreator _authOptionsCreator;
            │         private readonly ICacheOptionsCreator _cacheOptionsCreator;
            ├─ call IConfigurationCreator  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:50) [approx]
            │      public interface IConfigurationCreator
            │      InternalConfiguration Create(FileConfiguration configuration, Route[] routes);
            │  └─ di ConfigurationCreator [approx]
            │         public class ConfigurationCreator : IConfigurationCreator
            │         private readonly IAuthenticationOptionsCreator _authOptionsCreator;
            │         private readonly IServiceProviderConfigurationCreator _serviceProviderConfigCreator;
            ├─ call Create  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:50) [verified]
            │      public InternalConfiguration Create(FileConfiguration configuration, Route[] routes)
            │      var adminPath = _adminPath?.Path;
            │      var globalConfiguration = configuration.GlobalConfiguration ?? new();
            │  (12 more branches omitted beyond fan-out)
            │  ├─ call IAuthenticationOptionsCreator  (src/Configuration/Creator/ConfigurationCreator.cs:51) [approx]
            │  │      public interface IAuthenticationOptionsCreator
            │  │      AuthenticationOptions Create(FileAuthenticationOptions options);
            │  │      AuthenticationOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call IServiceProviderConfigurationCreator  (src/Configuration/Creator/ConfigurationCreator.cs:52) [approx]
            │  │      public interface IServiceProviderConfigurationCreator
            │  │      ServiceProviderConfiguration Create(FileGlobalConfiguration globalConfiguration);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call ILoadBalancerOptionsCreator  (src/Configuration/Creator/ConfigurationCreator.cs:53) [approx]
            │  │      public interface ILoadBalancerOptionsCreator
            │  │      LoadBalancerOptions Create(FileLoadBalancerOptions options);
            │  │      LoadBalancerOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call IQoSOptionsCreator  (src/Configuration/Creator/ConfigurationCreator.cs:54) [approx]
            │  │      public interface IQoSOptionsCreator
            │  │      QoSOptions Create(FileQoSOptions options);
            │  │      QoSOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call IHttpHandlerOptionsCreator  (src/Configuration/Creator/ConfigurationCreator.cs:55) [approx]
            │  │      public interface IHttpHandlerOptionsCreator
            │  │      HttpHandlerOptions Create(FileHttpHandlerOptions options);
            │  │      HttpHandlerOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call IVersionCreator  (src/Configuration/Creator/ConfigurationCreator.cs:56) [approx]
            │  │      public interface IVersionCreator
            │  │      Version Create(string downstreamHttpVersion);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call IVersionPolicyCreator  (src/Configuration/Creator/ConfigurationCreator.cs:57) [approx]
            │  │      /// <summary>
            │  │      /// Defines conversions from version policy strings to <see cref="HttpVersionPolicy"/> enumeration values.
            │  │      /// </summary>
            │  │  (stopped at depth 5; 1 branch omitted)
            │  └─ call IMetadataCreator  (src/Configuration/Creator/ConfigurationCreator.cs:58) [approx]
            │         /// <summary>
            │         /// This interface describes the creation of metadata options.
            │         /// </summary>
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call DynamicRoutesCreator.Create  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:43) [verified]
            │      public IReadOnlyList<Route> Create(FileConfiguration fileConfiguration)
            │      Route CreateRoute(FileDynamicRoute route)
            │      => SetUpDynamicRoute(route, fileConfiguration.GlobalConfiguration);
            │  ├─ call FileConfiguration.Select  (src/Configuration/Creator/DynamicRoutesCreator.cs:48) [approx]
            │  └─ call SetUpDynamicRoute  (src/Configuration/Creator/DynamicRoutesCreator.cs:47) [verified]
            │         private Route SetUpDynamicRoute(FileDynamicRoute dynamicRoute, FileGlobalConfiguration globalConfiguration)
            │         // The old RateLimitRule property takes precedence over the new RateLimitOptions property for backward compatibility, thus, override forcibly
            │         if (dynamicRoute.RateLimitRule != null)
            │     (stopped at depth 5; 38 branches omitted)
            └─ call Create  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:41) [verified]
                   public List<Route> Create(FileConfiguration fileConfiguration, IReadOnlyList<Route> routes)
                   return fileConfiguration.Aggregates
                   .Select(aggregate => SetUpAggregateRoute(routes, aggregate, fileConfiguration.GlobalConfiguration))
               ├─ call List  (src/Configuration/Creator/AggregatesCreator.cs:20) [verified]
               ├─ call SetUpAggregateRoute  (src/Configuration/Creator/AggregatesCreator.cs:20) [verified]
               │      private Route SetUpAggregateRoute(IEnumerable<Route> routes, FileAggregateRoute aggregateRoute, FileGlobalConfiguration globalConfiguration)
               │      var applicableRoutes = new List<DownstreamRoute>();
               │      var allRoutes = routes.SelectMany(x => x.DownstreamRoute);
               │  (stopped at depth 5; 5 branches omitted)
               └─ call FileConfiguration.Select  (src/Configuration/Creator/AggregatesCreator.cs:19) [approx]
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: DELETE /outputcache/{region}

TRACE  DELETE /outputcache/{region}
       src/Administration/OutputCacheController.cs:20
       Ocelot
▸ ENTRY  DELETE /outputcache/{region}  (src/Administration/OutputCacheController.cs:20)
   └─ call OutputCacheController.Delete  (src/Administration/OutputCacheController.cs:20)
          [HttpDelete]
          [Route("{region}")]
          public IActionResult Delete(string region)
      ├─ call IOcelotCache  (src/Administration/OutputCacheController.cs:24) [approx]
      │      public interface IOcelotCache<T>
      │      /// <summary>
      │      /// Adds the specified <paramref name="value"/> to the cache.
      │  └─ di DefaultMemoryCache [approx]
      │         public class DefaultMemoryCache<T> : IOcelotCache<T>
      │         private readonly IMemoryCache _memoryCache;
      │         private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _regions;
      └─ call DefaultMemoryCache.ClearRegion  (src/Administration/OutputCacheController.cs:24) [verified]
             public void ClearRegion(string region)
             if (region.IsNotEmpty() && _regions.TryGetValue(region, out var keys))
             foreach (var key in keys)
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

### Trace 3: GET /configuration

TRACE  GET /configuration
       src/Administration/FileConfigurationController.cs:24
       Ocelot
▸ ENTRY  GET /configuration  (src/Administration/FileConfigurationController.cs:24)
   └─ call FileConfigurationController.Get  (src/Administration/FileConfigurationController.cs:24)
          [HttpGet]
          public async Task<ActionResult> Get(CancellationToken cancellationToken)
          FileConfiguration configuration;
      └─ call IFileConfigurationRepository  (src/Administration/FileConfigurationController.cs:30) [approx]
             /// <summary>
             /// Features: <see cref="Features.AddOcelotConfigurationRepository(IServiceCollection)"/> and <see cref="IOcelotBuilder.AddConfigurationPoller()"/>.
             /// </summary>
         ├─ di TRepository  (src/DependencyInjection/OcelotBuilder.cs:203)
         └─ di DiskFileConfigurationRepository  (src/DependencyInjection/Features.cs:34)
                public class DiskFileConfigurationRepository : IFileConfigurationRepository, IDisposable
                private FileInfo _ocelotFile;
                private FileInfo _environmentFile;
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_4 info · 6 notable_

### **NOTABLE**: Auth surface: 3 protected, 0 unannotated of 3 API endpoints
*(Risk)*

- 3 protected
- DELETE /outputcache/{region}
- POST /configuration

### **NOTABLE**: Config without defaults: 2 consumed keys have no appsettings default
*(Risk)*

- Jwt:Audience
- Jwt:Issuer

### **NOTABLE**: Missing validation: 2/2 write endpoints have no FluentValidation validator
*(Risk)*

- DELETE /outputcache/{region} → IOcelotCache
- POST /configuration → SetAsync

### **NOTABLE**: Most depended-upon: Ocelot (5 dependents) · Ocelot (5 dependents) · Ocelot (2 dependents)
*(Topology)*

- Ocelot (5 dependents)
- Ocelot (5 dependents)
- Ocelot (2 dependents)

### **NOTABLE**: Event wiring: 1 events (0 integration), 0 cross-service, 1 orphan
*(Wiring)*

- StickySession ← Ocelot (no consumer)

### **NOTABLE**: Multi-implementation interfaces: ILoadBalancerCreator (4 impls) · IDownstreamRouteProvider (3 impls) · IOcelotLoggerFactory (3 impls)
*(Wiring)*

- ILoadBalancerCreator (4 impls)
- IDownstreamRouteProvider (3 impls)
- IOcelotLoggerFactory (3 impls)

### _INFO_: Routing surface: 3 routes exposed
*(Shape)*

- DELETE /outputcache/{region}
- POST /configuration
- GET /configuration

### _INFO_: Entry targets resolved 4/4 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 111 Singleton · 91 Extension · 7 Transient (209 total)
*(Wiring)*

### _INFO_: Entry surface: 3 HTTP · 1 hosted
*(Shape)*

- 3 HTTP
- 1 hosted

GATEWAY  Ocelot     (2 projects)

STACK  net8.0;net9.0;net10.0 · Minimal APIs · Controllers · FluentValidation

STYLE  SampleCollection  (confidence moderate)
       evidence: 16/17 projects are samples/demos/docs — no unified architecture

       per service:
         Ocelot.Benchmarks: Unknown
         Ocelot.Samples.Basic: Unknown
         Ocelot.Samples.Configuration: Unknown
         Ocelot.Samples.GraphQL: Unknown
         Ocelot.Samples.Metadata: Unknown
         Ocelot.Samples.OpenTracing: Unknown
         Ocelot.Samples.Web: Web App
         Ocelot.Samples.WebSocket: Unknown
         Ocelot.Samples.Eureka.ApiGateway: Unknown
         Ocelot.Samples.Eureka.DownstreamService: Unknown
         Ocelot.Samples.Kubernetes.ApiGateway: Unknown
         Ocelot.Samples.Kubernetes.DownstreamService: Unknown
         Ocelot.Samples.ServiceDiscovery.ApiGateway: Unknown
         Ocelot.Samples.ServiceDiscovery.DownstreamService: Unknown
         Ocelot.Samples.ServiceFabric.ApiGateway: Unknown
         Ocelot.Samples.ServiceFabric.DownstreamService: Unknown

TOPOLOGY (depends-on)
   Ocelot
   Ocelot.Benchmarks ── Ocelot, Ocelot.Testing

ROUTES
   Get /identityserverexample  →  localhost/
   Get /posts  →  jsonplaceholder.typicode.com/posts
   Get /posts/{postId}  →  jsonplaceholder.typicode.com/posts/{postId}
   Get /posts/{postId}/comments  →  jsonplaceholder.typicode.com/posts/{postId}/comments
   Get /comments  →  jsonplaceholder.typicode.com/comments
   Post /posts  →  jsonplaceholder.typicode.com/posts
   Put /posts/{postId}  →  jsonplaceholder.typicode.com/posts/{postId}
   Patch /posts/{postId}  →  jsonplaceholder.typicode.com/posts/{postId}
   Delete /posts/{postId}  →  jsonplaceholder.typicode.com/posts/{postId}
   Get /products  →  jsonplaceholder.typicode.com/api/products
   Get /products/{productId}  →  jsonplaceholder.typicode.com/api/products/{productId}
   Post /products  →  jsonplaceholder.typicode.com/api/products
   Put /products/{productId}  →  jsonplaceholder.typicode.com/api/products/{productId}
   Get /posts/  →  jsonplaceholder.typicode.com/posts
   GET /list-post  →  jsonplaceholder.typicode.com/posts

ENTRY POINTS
   HTTP (3)
      DELETE /outputcache/{region}  → IOcelotCache  (src/Administration/OutputCacheController.cs:20)
      GET /configuration  → IFileConfigurationRepository  (src/Administration/FileConfigurationController.cs:24)
      POST /configuration  → FileAndInternalConfigurationSetter.SetAsync  (src/Administration/FileConfigurationController.cs:42)
   Background (1)
      TPoller  → TPoller  (src/DependencyInjection/OcelotBuilder.cs:203)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Authentication.JwtBearer 10.0.10, Microsoft.AspNetCore.MiddlewareAnalysis 10.0.10, Microsoft.AspNetCore.Mvc.NewtonsoftJson 10.0.10, Microsoft.AspNetCore.TestHost 10.0.10, Microsoft.ServiceFabric.AspNetCore.Kestrel 8.6.235, Serilog.AspNetCore 10.0.0, Swashbuckle.AspNetCore 10.2.3
   Validation:  FluentValidation 12.1.1
   Logging:  Serilog 4.4.0
   Testing:  coverlet.MTP 10.0.1, Moq 4.20.72, Shouldly 4.3.0, xunit.v3.mtp-v2 4.0.0-pre.128
   Utilities:  GraphQL.NewtonsoftJson 8.8.4, Ocelot.QualityOfService.Polly 25.0.0-beta.2
   Other:  BenchmarkDotNet 0.16.0-preview.1, GraphQL 8.8.4, IPAddressRange 6.3.0, Jaeger 1.0.3, Microsoft.Extensions.Caching.Memory 10.0.10, Microsoft.Extensions.Configuration.EnvironmentVariables 10.0.10, Microsoft.Extensions.Configuration.FileExtensions 10.0.10, Microsoft.Extensions.Configuration.Json 10.0.10 … (26 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /configuration")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 647ms |
| GenericExtraction | 4662ms |
| SignalSealing | 0ms |
| SpecificExtraction | 4335ms |
| Compression | 158ms |
| **Total** | **22824ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| ProgramCsFlowExtractor | 4657ms | 858 | 233 |
| SyntaxStructureExtractor | 4642ms | 858 | 209 |
| DiRegistrationExtractor | 4638ms | 1 | 209 |
| EndpointExtractor | 2167ms | 0 | 34 |
| CallGraphExtractor | 2159ms | 0 | 0 |
| SourceBodyExtractor | 684ms | 0 | 0 |
| ControllerActionExtractor | 577ms | 0 | 31 |
| InMemoryEventBusExtractor | 575ms | 0 | 22 |
| BodyFactsExtractor | 533ms | 0 | 0 |
| GrpcClientExtractor | 506ms | 0 | 15 |
| ProjectStructure | 412ms | 0 | 0 |
| IndirectWiringDetector | 178ms | 0 | 12 |
| FileTreeExtractor | 150ms | 0 | 0 |
| SolutionDiscovery | 78ms | 0 | 0 |
| DependencyExtractor | 26ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 1122 | 479 |
| Raises | 1 | 0 |
| Resolves | 160 | 60 |

_754 files · 21 projects_
