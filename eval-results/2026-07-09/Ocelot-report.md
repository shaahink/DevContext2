# REPORT
**Ocelot**

Style: SampleCollection
_9 projects  ·  3 HttpEndpoint  ·  net8.0;net9.0 + controllers + minimal-apis + polly + fluentvalidation + gateway_

## Stats

| Metric | Value |
|--------|-------|
| Files | 790 |
| Projects | 26 |
| Nodes | 1494 |
| Edges | 1219 |
| Entries | 3 |
| With target | 3/3 |
| Verified edges | 55% |
| Analyzed in | 25.2s |

## Top Flows

1. **POST /configuration** → `Set` *(HttpEndpoint)*
2. **GET /configuration** → `Get` *(HttpEndpoint)*
3. **DELETE /outputcache/{region}** → `IOcelotCache` *(HttpEndpoint)*

### Trace 1: POST /configuration

TRACE  POST /configuration
       src/Ocelot/Administration/FileConfigurationController.cs:37
       Ocelot
▸ ENTRY  POST /configuration  (src/Ocelot/Administration/FileConfigurationController.cs:37)
   └─ call FileConfigurationController.Post  (src/Ocelot/Administration/FileConfigurationController.cs:37)
          [HttpPost]
          public async Task<IActionResult> Post([FromBody] FileConfiguration fileConfiguration)
          try
      ├─ call IFileConfigurationSetter  (src/Ocelot/Administration/FileConfigurationController.cs:42) [approx]
      │      public interface IFileConfigurationSetter
      │      Task<Response> Set(FileConfiguration config);
      │  └─ di FileAndInternalConfigurationSetter [approx]
      │         public class FileAndInternalConfigurationSetter : IFileConfigurationSetter
      │         private readonly IInternalConfigurationRepository _internalConfigRepo;
      │         private readonly IInternalConfigurationCreator _configCreator;
      └─ call Set  (src/Ocelot/Administration/FileConfigurationController.cs:42) [verified]
             public async Task<Response> Set(FileConfiguration fileConfig)
             var response = await _repo.Set(fileConfig);
             if (response.IsError)
         ├─ call IFileConfigurationRepository  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:26) [approx]
         │      public interface IFileConfigurationRepository
         │      Task<Response<FileConfiguration>> Get();
         │      Task<Response> Set(FileConfiguration fileConfiguration);
         │  └─ di ConsulFileConfigurationRepository  (src/Ocelot.Provider.Consul/OcelotBuilderExtensions.cs:57) [×2 impls]
         │         public class ConsulFileConfigurationRepository : IFileConfigurationRepository
         │         private readonly IOcelotCache<FileConfiguration> _cache;
         │         private readonly string _configurationKey;
         ├─ call IInternalConfigurationCreator  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:33) [approx]
         │      public interface IInternalConfigurationCreator
         │      Task<Response<IInternalConfiguration>> Create(FileConfiguration fileConfiguration);
         │  └─ di FileInternalConfigurationCreator [approx]
         │         public class FileInternalConfigurationCreator : IInternalConfigurationCreator
         │         private readonly IConfigurationValidator _configurationValidator;
         │         private readonly IConfigurationCreator _configCreator;
         ├─ call IInternalConfigurationRepository  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:37) [approx]
         │      public interface IInternalConfigurationRepository
         │      Response<IInternalConfiguration> Get();
         │      Response AddOrReplace(IInternalConfiguration internalConfiguration);
         │  └─ di InMemoryInternalConfigurationRepository [approx]
         │         /// <summary>
         │         /// Register as singleton.
         │         /// </summary>
         ├─ call AddOrReplace  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:37) [verified]
         │      public Response AddOrReplace(IInternalConfiguration internalConfiguration)
         │      lock (LockObject)
         │      _internalConfiguration = internalConfiguration;
         │  ├─ call IOcelotConfigurationChangeTokenSource  (src/Ocelot/Configuration/Repository/InMemoryInternalConfigurationRepository.cs:33) [approx]
         │  │      /// <summary>
         │  │      /// <see cref="IChangeToken" /> source which is activated when Ocelot's configuration is changed.
         │  │      /// </summary>
         │  │  └─ di OcelotConfigurationChangeTokenSource [approx]
         │  │         public class OcelotConfigurationChangeTokenSource : IOcelotConfigurationChangeTokenSource
         │  │         private readonly OcelotConfigurationChangeToken _changeToken = new();
         │  │         public IChangeToken ChangeToken => _changeToken;
         │  └─ call Activate  (src/Ocelot/Configuration/Repository/InMemoryInternalConfigurationRepository.cs:33) [verified]
         │         public void Activate()
         │         _changeToken.Activate();
         │     ├─ call OcelotConfigurationChangeToken  (src/Ocelot/Configuration/ChangeTracking/OcelotConfigurationChangeTokenSource.cs:13) [approx]
         │     │      public class OcelotConfigurationChangeToken : IChangeToken
         │     │      public const double PollingIntervalSeconds = 1;
         │     │      private readonly ICollection<CallbackWrapper> _callbacks = new List<CallbackWrapper>();
         │     └─ call OcelotConfigurationChangeToken.Activate  (src/Ocelot/Configuration/ChangeTracking/OcelotConfigurationChangeTokenSource.cs:13) [verified]
         │            public void Activate()
         │            lock (_lock)
         │            _timeChanged = DateTime.UtcNow;
         │        (stopped at depth 5; 1 branch omitted)
         ├─ call Create  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:33) [verified]
         │      public async Task<Response<IInternalConfiguration>> Create(FileConfiguration fileConfiguration)
         │      var response = await _configurationValidator.IsValid(fileConfiguration);
         │      if (response.Data.IsError)
         │  (2 more branches omitted beyond fan-out)
         │  ├─ call IConfigurationValidator  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:32) [approx]
         │  │      public interface IConfigurationValidator
         │  │      Task<Response<ConfigurationValidationResult>> IsValid(FileConfiguration configuration);
         │  │  └─ di FileConfigurationFluentValidator  (src/Ocelot/DependencyInjection/Features.cs:19)
         │  │         /// <summary>Validation of a <see cref="FileConfiguration"/> objects.</summary>
         │  │         public partial class FileConfigurationFluentValidator : AbstractValidator<FileConfiguration>, IConfigurationValidator
         │  │         private readonly List<ServiceDiscoveryFinderDelegate> _serviceDiscoveryFinderDelegates;
         │  ├─ call IRoutesCreator  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:39) [approx]
         │  │      public interface IRoutesCreator
         │  │      IReadOnlyList<Route> Create(FileConfiguration fileConfiguration);
         │  │      /// <summary>
         │  │  └─ di StaticRoutesCreator [approx]
         │  │         public class StaticRoutesCreator : IRoutesCreator
         │  │         private readonly ILoadBalancerOptionsCreator _loadBalancerOptionsCreator;
         │  │         private readonly IClaimsToThingCreator _claimsToThingCreator;
         │  ├─ call IAggregatesCreator  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:41) [approx]
         │  │      public interface IAggregatesCreator
         │  │      List<Route> Create(FileConfiguration fileConfiguration, IReadOnlyList<Route> routes);
         │  │  └─ di AggregatesCreator [approx]
         │  │         public class AggregatesCreator : IAggregatesCreator
         │  │         private readonly IUpstreamTemplatePatternCreator _creator;
         │  │         private readonly IUpstreamHeaderTemplatePatternCreator _headerCreator;
         │  ├─ call IDynamicsCreator  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:43) [approx]
         │  │      public interface IDynamicsCreator
         │  │      IReadOnlyList<Route> Create(FileConfiguration fileConfiguration);
         │  │      /// <summary>
         │  │  └─ di DynamicRoutesCreator [approx]
         │  │         public class DynamicRoutesCreator : IDynamicsCreator
         │  │         private readonly IAuthenticationOptionsCreator _authOptionsCreator;
         │  │         private readonly ICacheOptionsCreator _cacheOptionsCreator;
         │  ├─ call IConfigurationCreator  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:50) [approx]
         │  │      public interface IConfigurationCreator
         │  │      InternalConfiguration Create(FileConfiguration configuration, Route[] routes);
         │  │  └─ di ConfigurationCreator [approx]
         │  │         public class ConfigurationCreator : IConfigurationCreator
         │  │         private readonly IAuthenticationOptionsCreator _authOptionsCreator;
         │  │         private readonly IServiceProviderConfigurationCreator _serviceProviderConfigCreator;
         │  ├─ call Create  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:50) [verified]
         │  │      public InternalConfiguration Create(FileConfiguration configuration, Route[] routes)
         │  │      var adminPath = _adminPath?.Path;
         │  │      var globalConfiguration = configuration.GlobalConfiguration ?? new();
         │  │  (12 more branches omitted beyond fan-out)
         │  │  ├─ call IAuthenticationOptionsCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:51) [approx]
         │  │  │      public interface IAuthenticationOptionsCreator
         │  │  │      AuthenticationOptions Create(FileAuthenticationOptions options);
         │  │  │      AuthenticationOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call IServiceProviderConfigurationCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:52) [approx]
         │  │  │      public interface IServiceProviderConfigurationCreator
         │  │  │      ServiceProviderConfiguration Create(FileGlobalConfiguration globalConfiguration);
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call ILoadBalancerOptionsCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:53) [approx]
         │  │  │      public interface ILoadBalancerOptionsCreator
         │  │  │      LoadBalancerOptions Create(FileLoadBalancerOptions options);
         │  │  │      LoadBalancerOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call IQoSOptionsCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:54) [approx]
         │  │  │      public interface IQoSOptionsCreator
         │  │  │      QoSOptions Create(FileQoSOptions options);
         │  │  │      QoSOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call IHttpHandlerOptionsCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:55) [approx]
         │  │  │      public interface IHttpHandlerOptionsCreator
         │  │  │      HttpHandlerOptions Create(FileHttpHandlerOptions options);
         │  │  │      HttpHandlerOptions Create(FileRoute route, FileGlobalConfiguration globalConfiguration);
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call IVersionCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:56) [approx]
         │  │  │      public interface IVersionCreator
         │  │  │      Version Create(string downstreamHttpVersion);
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call IVersionPolicyCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:57) [approx]
         │  │  │      /// <summary>
         │  │  │      /// Defines conversions from version policy strings to <see cref="HttpVersionPolicy"/> enumeration values.
         │  │  │      /// </summary>
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  └─ call IMetadataCreator  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:58) [approx]
         │  │         /// <summary>
         │  │         /// This interface describes the creation of metadata options.
         │  │         /// </summary>
         │  │     (stopped at depth 5; 1 branch omitted)
         │  ├─ call DynamicRoutesCreator.Create  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:43) [verified]
         │  │      public IReadOnlyList<Route> Create(FileConfiguration fileConfiguration)
         │  │      Route CreateRoute(FileDynamicRoute route)
         │  │      => SetUpDynamicRoute(route, fileConfiguration.GlobalConfiguration);
         │  │  ├─ call FileConfiguration.Select  (src/Ocelot/Configuration/Creator/DynamicRoutesCreator.cs:48) [approx]
         │  │  └─ call SetUpDynamicRoute  (src/Ocelot/Configuration/Creator/DynamicRoutesCreator.cs:47) [verified]
         │  │         private Route SetUpDynamicRoute(FileDynamicRoute dynamicRoute, FileGlobalConfiguration globalConfiguration)
         │  │         // The old RateLimitRule property takes precedence over the new RateLimitOptions property for backward compatibility, thus, override forcibly
         │  │         if (dynamicRoute.RateLimitRule != null)
         │  │     (stopped at depth 5; 38 branches omitted)
         │  └─ call Create  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:41) [verified]
         │         public List<Route> Create(FileConfiguration fileConfiguration, IReadOnlyList<Route> routes)
         │         return fileConfiguration.Aggregates
         │         .Select(aggregate => SetUpAggregateRoute(routes, aggregate, fileConfiguration.GlobalConfiguration))
         │     ├─ call List  (src/Ocelot/Configuration/Creator/AggregatesCreator.cs:20) [verified]
         │     ├─ call SetUpAggregateRoute  (src/Ocelot/Configuration/Creator/AggregatesCreator.cs:20) [verified]
         │     │      private Route SetUpAggregateRoute(IEnumerable<Route> routes, FileAggregateRoute aggregateRoute, FileGlobalConfiguration globalConfiguration)
         │     │      var applicableRoutes = new List<DownstreamRoute>();
         │     │      var allRoutes = routes.SelectMany(x => x.DownstreamRoute);
         │     │  (stopped at depth 5; 6 branches omitted)
         │     └─ call FileConfiguration.Select  (src/Ocelot/Configuration/Creator/AggregatesCreator.cs:19) [approx]
         └─ call Set  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:26) [verified]
                public async Task<Response> Set(FileConfiguration ocelotConfiguration)
                var json = JsonConvert.SerializeObject(ocelotConfiguration, Formatting.Indented);
                var bytes = Encoding.UTF8.GetBytes(json);
            ├─ call IOcelotCache  (src/Ocelot.Provider.Consul/ConsulFileConfigurationRepository.cs:73) [approx]
            │      public interface IOcelotCache<T>
            │      /// <summary>
            │      /// Adds the specified <paramref name="value"/> to the cache.
            └─ call DefaultMemoryCache.AddOrUpdate  (src/Ocelot.Provider.Consul/ConsulFileConfigurationRepository.cs:73) [verified]
                   public T AddOrUpdate(string key, T value, string region, TimeSpan ttl)
                   if (_memoryCache.TryGetValue(key, out var cached))
                   _memoryCache.Remove(key);
               └─ call DefaultMemoryCache.Add  (src/Ocelot/Cache/DefaultMemoryCache.cs:35) [verified]
                      public bool Add(string key, T value, string region, TimeSpan ttl)
                      if (ttl.TotalMilliseconds <= 0)
                      return false;
                  (stopped at depth 5; 1 branch omitted)
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: GET /configuration

TRACE  GET /configuration
       src/Ocelot/Administration/FileConfigurationController.cs:24
       Ocelot
▸ ENTRY  GET /configuration  (src/Ocelot/Administration/FileConfigurationController.cs:24)
   └─ call FileConfigurationController.Get  (src/Ocelot/Administration/FileConfigurationController.cs:24)
          [HttpGet]
          public async Task<IActionResult> Get()
          var response = await _repo.Get();
      ├─ call IFileConfigurationRepository  (src/Ocelot/Administration/FileConfigurationController.cs:27) [approx]
      │      public interface IFileConfigurationRepository
      │      Task<Response<FileConfiguration>> Get();
      │      Task<Response> Set(FileConfiguration fileConfiguration);
      │  └─ di ConsulFileConfigurationRepository  (src/Ocelot.Provider.Consul/OcelotBuilderExtensions.cs:57) [×2 impls]
      │         public class ConsulFileConfigurationRepository : IFileConfigurationRepository
      │         private readonly IOcelotCache<FileConfiguration> _cache;
      │         private readonly string _configurationKey;
      └─ call Get  (src/Ocelot/Administration/FileConfigurationController.cs:27) [verified]
             public async Task<Response<FileConfiguration>> Get()
             var config = _cache.Get(_configurationKey, _configurationKey);
             if (config != null)
         ├─ call IOcelotCache  (src/Ocelot.Provider.Consul/ConsulFileConfigurationRepository.cs:42) [approx]
         │      public interface IOcelotCache<T>
         │      /// <summary>
         │      /// Adds the specified <paramref name="value"/> to the cache.
         └─ call DefaultMemoryCache.Get  (src/Ocelot.Provider.Consul/ConsulFileConfigurationRepository.cs:42) [verified]
                public T Get(string key, string region)
                if (TryGetValue(key, region, out T value))
                return value;
            └─ call DefaultMemoryCache.TryGetValue  (src/Ocelot/Cache/DefaultMemoryCache.cs:41) [verified]
                   public bool TryGetValue(string key, string region, out T value)
                   return _memoryCache.TryGetValue(key, out value);
RESULT   200 OK · failure → 404 Not Found

---

### Trace 3: DELETE /outputcache/{region}

TRACE  DELETE /outputcache/{region}
       src/Ocelot/Administration/OutputCacheController.cs:20
       Ocelot
▸ ENTRY  DELETE /outputcache/{region}  (src/Ocelot/Administration/OutputCacheController.cs:20)
   └─ call OutputCacheController.Delete  (src/Ocelot/Administration/OutputCacheController.cs:20)
          [HttpDelete]
          [Route("{region}")]
          public IActionResult Delete(string region)
      ├─ call IOcelotCache  (src/Ocelot/Administration/OutputCacheController.cs:24) [approx]
      │      public interface IOcelotCache<T>
      │      /// <summary>
      │      /// Adds the specified <paramref name="value"/> to the cache.
      └─ call DefaultMemoryCache.ClearRegion  (src/Ocelot/Administration/OutputCacheController.cs:24) [verified]
             public void ClearRegion(string region)
             if (_regions.TryGetValue(region, out var keys))
             foreach (var key in keys)
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

## Insights

_2 info · 8 notable_

### **NOTABLE**: Config without defaults: 2 consumed keys have no appsettings default
*(Risk)*

- Jwt:Audience
- Jwt:Issuer

### **NOTABLE**: Internal hubs: 1 heavily-referenced internal types
*(Topology)*

- CallbackWrapper (1 refs)

### **NOTABLE**: Extension seats: AddKubernetes (6 impls) · AddOcelot (4 impls) · AddButterfly (4 impls)
*(Wiring)*

- AddKubernetes (6 impls)
- AddOcelot (4 impls)
- AddButterfly (4 impls)

### **NOTABLE**: Missing validation: 3/3 endpoints have no FluentValidation validator
*(Risk)*

- DELETE /outputcache/{region} → IOcelotCache
- POST /configuration → Set
- GET /configuration → Get

### **NOTABLE**: Most depended-upon: Ocelot (9 dependents) · Ocelot.Samples.Web (8 dependents) · Ocelot (7 dependents)
*(Topology)*

- Ocelot (9 dependents)
- Ocelot.Samples.Web (8 dependents)
- Ocelot (7 dependents)

### **NOTABLE**: Auth surface: 3 protected, 0 unannotated of 3 API endpoints
*(Risk)*

- 3 protected
- DELETE /outputcache/{region}
- POST /configuration

### **NOTABLE**: Event flow: 1 published, 0 consumed, 1 orphan
*(Wiring)*

- 1 orphan events (published, no internal consumer)
- Ocelot.LoadBalancer.StickySession ← Update
- 0/1 events consumed

### **NOTABLE**: Multi-implementation interfaces: ILoadBalancerCreator (4 impls) · IDownstreamRouteProvider (3 impls) · IOcelotTracer (3 impls)
*(Wiring)*

- ILoadBalancerCreator (4 impls)
- IDownstreamRouteProvider (3 impls)
- IOcelotTracer (3 impls)

### _INFO_: Entry targets resolved 3/3 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Routing surface: 3 routes exposed
*(Shape)*

- DELETE /outputcache/{region}
- POST /configuration
- GET /configuration

MAP  Ocelot     (9 projects)

STACK  net8.0;net9.0 · Minimal APIs · Controllers · FluentValidation

STYLE  SampleCollection  (confidence moderate)
       evidence: 14/23 projects are samples/demos/docs — no unified architecture

       per service:
         Ocelot.Samples.Basic: Unknown
         Ocelot.Samples.Configuration: Unknown
         Ocelot.Samples.GraphQL: Unknown
         Ocelot.Samples.Metadata: Unknown
         Ocelot.Samples.Web: Web App
         Ocelot.Benchmarks: Unknown
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
   Ocelot.Benchmarks ── Ocelot
   Ocelot.Cache.CacheManager ── Ocelot
   Ocelot.Provider.Consul ── Ocelot
   Ocelot.Provider.Eureka ── Ocelot
   Ocelot.Provider.Kubernetes ── Ocelot
   Ocelot.Provider.Polly ── Ocelot
   Ocelot.Tracing.Butterfly ── Ocelot
   Ocelot.Tracing.OpenTracing ── Ocelot

ENTRY POINTS
   HTTP (3)
      DELETE /outputcache/{region}  → IOcelotCache  (src/Ocelot/Administration/OutputCacheController.cs:20)
      GET /configuration  → Get  (src/Ocelot/Administration/FileConfigurationController.cs:24)
      POST /configuration  → Set  (src/Ocelot/Administration/FileConfigurationController.cs:37)

PACKAGES
   Web/API:  Butterfly.Client.AspNetCore 0.0.8, Microsoft.AspNetCore.Authentication.JwtBearer 9.0.11, Microsoft.AspNetCore.MiddlewareAnalysis 9.0.11, Microsoft.AspNetCore.Mvc.NewtonsoftJson 9.0.11, Microsoft.AspNetCore.TestHost 9.0.11, Microsoft.ServiceFabric.AspNetCore.Kestrel 7.1.2493, Serilog.AspNetCore 9.0.0, Swashbuckle.AspNetCore 9.0.6
   Validation:  FluentValidation 12.1.1
   Logging:  Serilog 4.3.0
   Testing:  coverlet.collector 6.0.4, Moq 4.20.72, Shouldly 4.3.0, xunit 2.9.3, xunit.runner.visualstudio 3.1.5, Xunit.SkippableFact 1.5.23
   Utilities:  GraphQL.NewtonsoftJson 8.8.0, Polly 8.6.5, Polly.Testing 8.6.5
   Other:  BenchmarkDotNet 0.15.8, Butterfly.Client 0.0.8, CacheManager.Core 2.0.0, CacheManager.Microsoft.Extensions.Configuration 2.0.0, CacheManager.Serialization.Json 2.0.0, Consul 1.7.14.9, GraphQL 8.8.0, IPAddressRange 6.3.0 … (41 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 703ms |
| GenericExtraction | 5336ms |
| SignalSealing | 0ms |
| SpecificExtraction | 4386ms |
| Compression | 129ms |
| **Total** | **25168ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 5332ms | 885 | 240 |
| DiRegistrationExtractor | 5328ms | 0 | 240 |
| CallGraphExtractor | 2319ms | 0 | 0 |
| EndpointExtractor | 2060ms | 0 | 50 |
| ProgramCsFlowExtractor | 656ms | 0 | 20 |
| ControllerActionExtractor | 627ms | 0 | 48 |
| InMemoryEventBusExtractor | 620ms | 0 | 48 |
| GrpcClientExtractor | 612ms | 0 | 48 |
| ProjectStructure | 501ms | 0 | 0 |
| SourceBodyExtractor | 445ms | 0 | 0 |
| BodyFactsExtractor | 378ms | 0 | 0 |
| IndirectWiringDetector | 174ms | 0 | 29 |
| FileTreeExtractor | 136ms | 0 | 0 |
| SolutionDiscovery | 61ms | 0 | 0 |
| DependencyExtractor | 27ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 1050 | 487 |
| Raises | 1 | 0 |
| Resolves | 168 | 63 |

_790 files · 26 projects_
