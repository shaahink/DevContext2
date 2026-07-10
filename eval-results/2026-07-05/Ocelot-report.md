# REPORT
**Ocelot**

Style: ControllerBased
_9 projects  ·  3 HttpEndpoint  ·  net8.0;net9.0 + controllers + minimal-apis + polly + fluentvalidation + gateway_

## Stats

| Metric | Value |
|--------|-------|
| Files | 790 |
| Projects | 26 |
| Nodes | 701 |
| Edges | 392 |
| Entries | 3 |
| With target | 3/3 |
| Verified edges | 75% |
| Analyzed in | 9.0s |

## Top Flows

1. **POST /configuration** → `FileAndInternalConfigurationSetter.Set` *(HttpEndpoint)*
2. **GET /configuration** → `ConsulFileConfigurationRepository.Get` *(HttpEndpoint)*
3. **DELETE /outputcache/{region}** → `DefaultMemoryCache.ClearRegion` *(HttpEndpoint)*

### Trace 1: POST /configuration

TRACE  POST /configuration
       src/Ocelot/Administration/FileConfigurationController.cs:37

▸ ENTRY  POST /configuration  (src/Ocelot/Administration/FileConfigurationController.cs:37)
   └─ call FileConfigurationController.Post  (src/Ocelot/Administration/FileConfigurationController.cs:37)
          [HttpPost]
          public async Task<IActionResult> Post([FromBody] FileConfiguration fileConfiguration)
          try
      └─ call FileAndInternalConfigurationSetter.Set  (src/Ocelot/Administration/FileConfigurationController.cs:42) [verified]
             public async Task<Response> Set(FileConfiguration fileConfig)
             var response = await _repo.Set(fileConfig);
             if (response.IsError)
         ├─ call ConsulFileConfigurationRepository.Set  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:26) [verified]
         │      public async Task<Response> Set(FileConfiguration ocelotConfiguration)
         │      var json = JsonConvert.SerializeObject(ocelotConfiguration, Formatting.Indented);
         │      var bytes = Encoding.UTF8.GetBytes(json);
         │  └─ call DefaultMemoryCache.AddOrUpdate  (src/Ocelot.Provider.Consul/ConsulFileConfigurationRepository.cs:73) [verified]
         │         public T AddOrUpdate(string key, T value, string region, TimeSpan ttl)
         │         if (_memoryCache.TryGetValue(key, out var cached))
         │         _memoryCache.Remove(key);
         │     └─ call DefaultMemoryCache.Add  (src/Ocelot/Cache/DefaultMemoryCache.cs:35) [verified]
         │            public bool Add(string key, T value, string region, TimeSpan ttl)
         │            if (ttl.TotalMilliseconds <= 0)
         │            return false;
         │        (stopped at depth 5; 1 branch omitted)
         ├─ call FileInternalConfigurationCreator.Create  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:33) [verified]
         │      public async Task<Response<IInternalConfiguration>> Create(FileConfiguration fileConfiguration)
         │      var response = await _configurationValidator.IsValid(fileConfiguration);
         │      if (response.Data.IsError)
         │  ├─ call FileConfigurationFluentValidator.IsValid  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:32) [verified]
         │  │      public async Task<Response<ConfigurationValidationResult>> IsValid(FileConfiguration configuration)
         │  │      var validateResult = await ValidateAsync(configuration);
         │  │      if (validateResult.IsValid)
         │  │  └─ call FileConfigurationFluentValidator.ValidateAsync  (src/Ocelot/Configuration/Validator/FileConfigurationFluentValidator.cs:82) [approx]
         │  ├─ call StaticRoutesCreator.Create  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:39) [verified]
         │  │      public IReadOnlyList<Route> Create(FileConfiguration fileConfiguration)
         │  │      Route CreateRoute(FileRoute route)
         │  │      => SetUpRoute(route, SetUpDownstreamRoute(route, fileConfiguration.GlobalConfiguration));
         │  │  ├─ call StaticRoutesCreator.SetUpRoute  (src/Ocelot/Configuration/Creator/StaticRoutesCreator.cs:69) [verified]
         │  │  │      private Route SetUpRoute(FileRoute fileRoute, DownstreamRoute downstreamRoute)
         │  │  │      var upstreamTemplatePattern = _upstreamTemplatePatternCreator.Create(fileRoute); // TODO It should be downstreamRoute.UpstreamPathTemplate
         │  │  │      var upstreamHeaderTemplates = _upstreamHeaderTemplatePatternCreator.Create(fileRoute); // TODO It should be downstreamRoute.UpstreamHeaders
         │  │  │  (stopped at depth 5; 3 branches omitted)
         │  │  ├─ call StaticRoutesCreator.SetUpDownstreamRoute  (src/Ocelot/Configuration/Creator/StaticRoutesCreator.cs:69) [verified]
         │  │  │      private DownstreamRoute SetUpDownstreamRoute(FileRoute fileRoute, FileGlobalConfiguration globalConfiguration)
         │  │  │      var requestIdKey = _requestIdKeyCreator.Create(fileRoute, globalConfiguration);
         │  │  │      var upstreamTemplatePattern = _upstreamTemplatePatternCreator.Create(fileRoute);
         │  │  │  (stopped at depth 5; 52 branches omitted)
         │  │  └─ call FileConfiguration.Select  (src/Ocelot/Configuration/Creator/StaticRoutesCreator.cs:70) [approx]
         │  ├─ call AggregatesCreator.Create  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:41) [verified]
         │  │      public List<Route> Create(FileConfiguration fileConfiguration, IReadOnlyList<Route> routes)
         │  │      return fileConfiguration.Aggregates
         │  │      .Select(aggregate => SetUpAggregateRoute(routes, aggregate, fileConfiguration.GlobalConfiguration))
         │  │  ├─ call FileConfiguration.Select  (src/Ocelot/Configuration/Creator/AggregatesCreator.cs:19) [approx]
         │  │  └─ call AggregatesCreator.SetUpAggregateRoute  (src/Ocelot/Configuration/Creator/AggregatesCreator.cs:20) [verified]
         │  │         private Route SetUpAggregateRoute(IEnumerable<Route> routes, FileAggregateRoute aggregateRoute, FileGlobalConfiguration globalConfiguration)
         │  │         var applicableRoutes = new List<DownstreamRoute>();
         │  │         var allRoutes = routes.SelectMany(x => x.DownstreamRoute);
         │  │     (stopped at depth 5; 4 branches omitted)
         │  ├─ call DynamicRoutesCreator.Create  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:43) [verified]
         │  │      public IReadOnlyList<Route> Create(FileConfiguration fileConfiguration)
         │  │      Route CreateRoute(FileDynamicRoute route)
         │  │      => SetUpDynamicRoute(route, fileConfiguration.GlobalConfiguration);
         │  │  ├─ call DynamicRoutesCreator.SetUpDynamicRoute  (src/Ocelot/Configuration/Creator/DynamicRoutesCreator.cs:47) [verified]
         │  │  │      private Route SetUpDynamicRoute(FileDynamicRoute dynamicRoute, FileGlobalConfiguration globalConfiguration)
         │  │  │      // The old RateLimitRule property takes precedence over the new RateLimitOptions property for backward compatibility, thus, override forcibly
         │  │  │      if (dynamicRoute.RateLimitRule != null)
         │  │  │  (stopped at depth 5; 27 branches omitted)
         │  │  └─ call FileConfiguration.Select  (src/Ocelot/Configuration/Creator/DynamicRoutesCreator.cs:48) [approx]
         │  └─ call ConfigurationCreator.Create  (src/Ocelot/Configuration/Creator/FileInternalConfigurationCreator.cs:50) [verified]
         │         public InternalConfiguration Create(FileConfiguration configuration, Route[] routes)
         │         var adminPath = _adminPath?.Path;
         │         var globalConfiguration = configuration.GlobalConfiguration ?? new();
         │     (2 more branches omitted beyond fan-out)
         │     ├─ call AuthenticationOptionsCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:51) [verified]
         │     │      public AuthenticationOptions Create(FileAuthenticationOptions options)
         │     │      => new(options);
         │     │  (stopped at depth 5; 2 branches omitted)
         │     ├─ call ServiceProviderConfigurationCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:52) [verified]
         │     │      public ServiceProviderConfiguration Create(FileGlobalConfiguration globalConfiguration)
         │     │      var port = globalConfiguration?.ServiceDiscoveryProvider?.Port ?? 0;
         │     │      var scheme = globalConfiguration?.ServiceDiscoveryProvider?.Scheme ?? "http";
         │     │  (stopped at depth 5; 9 branches omitted)
         │     ├─ call LoadBalancerOptionsCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:53) [verified]
         │     │      public LoadBalancerOptions Create(FileLoadBalancerOptions options)
         │     │      => new(options);
         │     │  (stopped at depth 5; 1 branch omitted)
         │     ├─ call QoSOptionsCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:54) [verified]
         │     │      public QoSOptions Create(FileQoSOptions options)
         │     │      => new(options ?? new());
         │     │  (stopped at depth 5; 2 branches omitted)
         │     ├─ call HttpHandlerOptionsCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:55) [verified]
         │     │      public HttpHandlerOptions Create(FileHttpHandlerOptions options)
         │     │      options ??= new();
         │     │      var hasTracer = _tracer != null;
         │     │  (stopped at depth 5; 1 branch omitted)
         │     ├─ call HttpVersionCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:56) [verified]
         │     │      public Version Create(string downstreamHttpVersion)
         │     │      if (!Version.TryParse(downstreamHttpVersion, out var version))
         │     │      version = new Version(1, 1);
         │     ├─ call HttpVersionPolicyCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:57) [verified]
         │     │      public HttpVersionPolicy Create(string downstreamHttpVersionPolicy) => downstreamHttpVersionPolicy switch
         │     │      VersionPolicies.RequestVersionExact => HttpVersionPolicy.RequestVersionExact,
         │     │      VersionPolicies.RequestVersionOrHigher => HttpVersionPolicy.RequestVersionOrHigher,
         │     └─ call DefaultMetadataCreator.Create  (src/Ocelot/Configuration/Creator/ConfigurationCreator.cs:58) [verified]
         │            public MetadataOptions Create(IDictionary<string, string> metadata, FileGlobalConfiguration globalConfiguration)
         │            metadata ??= new Dictionary<string, string>();
         │            globalConfiguration.Metadata ??= new Dictionary<string, string>();
         │        (stopped at depth 5; 7 branches omitted)
         └─ call InMemoryInternalConfigurationRepository.AddOrReplace  (src/Ocelot/Configuration/Setter/FileAndInternalConfigurationSetter.cs:37) [verified]
                public Response AddOrReplace(IInternalConfiguration internalConfiguration)
                lock (LockObject)
                _internalConfiguration = internalConfiguration;
            └─ call OcelotConfigurationChangeTokenSource.Activate  (src/Ocelot/Configuration/Repository/InMemoryInternalConfigurationRepository.cs:33) [verified]
                   public void Activate()
                   _changeToken.Activate();
               └─ call OcelotConfigurationChangeToken.Activate  (src/Ocelot/Configuration/ChangeTracking/OcelotConfigurationChangeTokenSource.cs:13) [verified]
                      public void Activate()
                      lock (_lock)
                      _timeChanged = DateTime.UtcNow;
                  (stopped at depth 5; 1 branch omitted)
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: GET /configuration

TRACE  GET /configuration
       src/Ocelot/Administration/FileConfigurationController.cs:24

▸ ENTRY  GET /configuration  (src/Ocelot/Administration/FileConfigurationController.cs:24)
   └─ call FileConfigurationController.Get  (src/Ocelot/Administration/FileConfigurationController.cs:24)
          [HttpGet]
          public async Task<IActionResult> Get()
          var response = await _repo.Get();
      └─ call ConsulFileConfigurationRepository.Get  (src/Ocelot/Administration/FileConfigurationController.cs:27) [verified]
             public async Task<Response<FileConfiguration>> Get()
             var config = _cache.Get(_configurationKey, _configurationKey);
             if (config != null)
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

▸ ENTRY  DELETE /outputcache/{region}  (src/Ocelot/Administration/OutputCacheController.cs:20)
   └─ call OutputCacheController.Delete  (src/Ocelot/Administration/OutputCacheController.cs:20)
          [HttpDelete]
          [Route("{region}")]
          public IActionResult Delete(string region)
      └─ call DefaultMemoryCache.ClearRegion  (src/Ocelot/Administration/OutputCacheController.cs:24) [verified]
             public void ClearRegion(string region)
             if (_regions.TryGetValue(region, out var keys))
             foreach (var key in keys)
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

## Insights

_3 info · 6 notable_

### **NOTABLE**: Downstream wiring: 10 target services detected
*(Wiring)*

- RateLimitOptionsCreator.MergeHeaderRules
- FileRoute.DeepCopy
- DefaultMemoryCache.ClearRegion
- FileConfigurationFluentValidator.IsValid
- StaticRoutesCreator.Create

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- IEnumerableExtensions
- DownstreamRequestInitialiserMiddleware
- IConfigurationCreator
- IAddHeadersToRequest
- DownstreamRouteExtensions

### **NOTABLE**: Internal hubs: 1 heavily-referenced internal types
*(Topology)*

- CallbackWrapper (1 refs)

### **NOTABLE**: Extension seats: AddKubernetes (6 impls) · AddOcelot (4 impls) · AddButterfly (4 impls)
*(Wiring)*

- AddKubernetes (6 impls)
- AddOcelot (4 impls)
- AddButterfly (4 impls)

### **NOTABLE**: Most depended-upon: Ocelot (9 dependents) · Ocelot.Samples.Web (8 dependents) · Ocelot (7 dependents)
*(Topology)*

- Ocelot (9 dependents)
- Ocelot.Samples.Web (8 dependents)
- Ocelot (7 dependents)

### **NOTABLE**: Auth surface: 3 protected, 0 unannotated of 3 endpoints
*(Risk)*

- 3 protected

### _INFO_: Entry targets resolved 3/3 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Routing surface: 3 routes exposed
*(Shape)*

- DELETE /outputcache/{region}
- POST /configuration
- GET /configuration

### _INFO_: Public surface: 104 interfaces, 690 classes (800 total public types)
*(Shape)*

- 104 interfaces
- 690 classes

GATEWAY  Ocelot     (9 projects)

STACK  net8.0;net9.0 · Minimal APIs · Controllers · FluentValidation

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, MinimalApi=yes(conf=0.9)

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
      DELETE /outputcache/{region}  → DefaultMemoryCache.ClearRegion  (src/Ocelot/Administration/OutputCacheController.cs:20)
      GET /configuration  → ConsulFileConfigurationRepository.Get  (src/Ocelot/Administration/FileConfigurationController.cs:24)
      POST /configuration  → FileAndInternalConfigurationSetter.Set  (src/Ocelot/Administration/FileConfigurationController.cs:37)

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
| DiscoveryAndCacheWarmup | 540ms |
| GenericExtraction | 3755ms |
| SignalSealing | 0ms |
| SpecificExtraction | 3301ms |
| Compression | 98ms |
| **Total** | **9049ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 3752ms | 885 | 240 |
| DiRegistrationExtractor | 3747ms | 0 | 240 |
| EndpointExtractor | 1730ms | 0 | 50 |
| CallGraphExtractor | 1566ms | 0 | 0 |
| ProgramCsFlowExtractor | 490ms | 0 | 20 |
| ProjectStructure | 416ms | 0 | 0 |
| InMemoryEventBusExtractor | 396ms | 0 | 48 |
| ControllerActionExtractor | 384ms | 0 | 48 |
| SourceBodyExtractor | 250ms | 0 | 0 |
| IndirectWiringDetector | 128ms | 0 | 32 |
| FileTreeExtractor | 66ms | 0 | 0 |
| SolutionDiscovery | 54ms | 0 | 0 |
| DependencyExtractor | 19ms | 0 | 0 |
| LayerClassifier | 13ms | 0 | 0 |
| BlazorEntryExtractor | 4ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 223 | 34 |
| Sends | 1 | 1 |
| Resolves | 168 | 63 |

_790 files · 0 projects_
