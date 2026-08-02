TRACE  POST /configuration
       src/Administration/FileConfigurationController.cs:41

▸ ENTRY  POST /configuration  (src/Administration/FileConfigurationController.cs:41)
   └─ call FileConfigurationController.Post  (src/Administration/FileConfigurationController.cs:41)
      └─ call FileAndInternalConfigurationSetter.SetAsync  (src/Administration/FileConfigurationController.cs:46) [verified]
             }
             }
             }
         ├─ call FileInternalConfigurationCreator.Create  (src/Configuration/Repository/FileAndInternalConfigurationSetter.cs:35) [verified]
         │      }
         │      }
         │  ├─ call FileConfigurationFluentValidator.IsValid  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:32) [verified]
         │  │      return new ErrorResponse<IInternalConfiguration>(response.Data.Errors);
         │  │      }
         │  │  └─ call FileConfigurationFluentValidator.ValidateAsync  (src/Configuration/Validator/FileConfigurationFluentValidator.cs:82) [approx]
         │  │         var result = new ConfigurationValidationResult(true, errors.Cast<Error>().ToList());
         │  │         return new OkResponse<ConfigurationValidationResult>(result);
         │  ├─ call StaticRoutesCreator.Create  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:39) [verified]
         │  │      var dynamicRoute = _dynamicsCreator.Create(fileConfiguration);
         │  │      var mergedRoutes = routes
         │  │  ├─ call StaticRoutesCreator.SetUpRoute  (src/Configuration/Creator/StaticRoutesCreator.cs:69) [verified]
         │  │  │      }
         │  │  │      public virtual int CreateTimeout(FileRoute route, FileGlobalConfiguration global)
         │  │  │  ├─ call UpstreamTemplatePatternCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:160) [verified]
         │  │  │  │      {
         │  │  │  │      UpstreamHeaderTemplates = upstreamHeaderTemplates, // downstreamRoute.UpstreamHeaders
         │  │  │  │      UpstreamHost = fileRoute.UpstreamHost,
         │  │  │  │  (stopped at depth 6; 3 branches omitted)
         │  │  │  ├─ call UpstreamHeaderTemplatePatternCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:161) [verified]
         │  │  │  │      UpstreamHeaderTemplates = upstreamHeaderTemplates, // downstreamRoute.UpstreamHeaders
         │  │  │  │      UpstreamHost = fileRoute.UpstreamHost,
         │  │  │  │      UpstreamHttpMethod = upstreamHttpMethods,
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  └─ call FileRoute.ToHttpMethods  (src/Configuration/Creator/StaticRoutesCreator.cs:162) [approx]
         │  │  │         UpstreamHost = fileRoute.UpstreamHost,
         │  │  │         UpstreamHttpMethod = upstreamHttpMethods,
         │  │  │         UpstreamTemplatePattern = upstreamTemplatePattern,
         │  │  ├─ call StaticRoutesCreator.SetUpDownstreamRoute  (src/Configuration/Creator/StaticRoutesCreator.cs:69) [verified]
         │  │  │      }
         │  │  │      public virtual int CreateTimeout(FileRoute route, FileGlobalConfiguration global)
         │  │  │  (40 more branches omitted beyond fan-out)
         │  │  │  ├─ call RequestIdKeyCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:83) [verified]
         │  │  │  │      var authOptions = _authOptionsCreator.Create(fileRoute, globalConfiguration);
         │  │  │  │      var claimsToHeaders = _claimsToThingCreator.Create(fileRoute.AddHeadersToRequest);
         │  │  │  ├─ call UpstreamTemplatePatternCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:85) [verified]
         │  │  │  │      var claimsToHeaders = _claimsToThingCreator.Create(fileRoute.AddHeadersToRequest);
         │  │  │  │      var claimsToClaims = _claimsToThingCreator.Create(fileRoute.AddClaimsToRequest);
         │  │  │  │  (stopped at depth 6; 3 branches omitted)
         │  │  │  ├─ call AuthenticationOptionsCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:87) [verified]
         │  │  │  │      var claimsToClaims = _claimsToThingCreator.Create(fileRoute.AddClaimsToRequest);
         │  │  │  │      var claimsToQueries = _claimsToThingCreator.Create(fileRoute.AddQueriesToRequest);
         │  │  │  │  (stopped at depth 6; 2 branches omitted)
         │  │  │  ├─ call ClaimsToThingCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:89) [verified]
         │  │  │  │      var claimsToQueries = _claimsToThingCreator.Create(fileRoute.AddQueriesToRequest);
         │  │  │  │      var claimsToDownstreamPath = _claimsToThingCreator.Create(fileRoute.ChangeDownstreamPathTemplate);
         │  │  │  │  (stopped at depth 6; 2 branches omitted)
         │  │  │  ├─ call QoSOptionsCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:97) [verified]
         │  │  │  │      var httpHandlerOptions = _httpHandlerOptionsCreator.Create(fileRoute, globalConfiguration);
         │  │  │  │      var hAndRs = _headerFAndRCreator.Create(fileRoute, globalConfiguration);
         │  │  │  │  (stopped at depth 6; 2 branches omitted)
         │  │  │  ├─ call RateLimitOptionsCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:99) [verified]
         │  │  │  │      var hAndRs = _headerFAndRCreator.Create(fileRoute, globalConfiguration);
         │  │  │  │      var downstreamAddresses = _downstreamAddressesCreator.Create(fileRoute);
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call HttpHandlerOptionsCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:101) [verified]
         │  │  │  │      var downstreamAddresses = _downstreamAddressesCreator.Create(fileRoute);
         │  │  │  │      var lbOptions = _loadBalancerOptionsCreator.Create(fileRoute, globalConfiguration);
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call HeaderFindAndReplaceCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:103) [verified]
         │  │  │  │      var lbOptions = _loadBalancerOptionsCreator.Create(fileRoute, globalConfiguration);
         │  │  │  │      var lbKey = _routeKeyCreator.Create(fileRoute, lbOptions);
         │  │  │  │  (stopped at depth 6; 2 branches omitted)
         │  │  │  ├─ call DownstreamAddressesCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:105) [verified]
         │  │  │  │      var securityOptions = _securityOptionsCreator.Create(fileRoute.SecurityOptions, globalConfiguration);
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call LoadBalancerOptionsCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:107) [verified]
         │  │  │  │      var downstreamHttpVersion = _versionCreator.Create(fileRoute.DownstreamHttpVersion);
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call RouteKeyCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:108) [verified]
         │  │  │  │      var downstreamHttpVersion = _versionCreator.Create(fileRoute.DownstreamHttpVersion);
         │  │  │  │      var downstreamHttpVersionPolicy = _versionPolicyCreator.Create(fileRoute.DownstreamHttpVersionPolicy);
         │  │  │  │  (stopped at depth 6; 6 branches omitted)
         │  │  │  └─ call SecurityOptionsCreator.Create  (src/Configuration/Creator/StaticRoutesCreator.cs:110) [verified]
         │  │  │         var downstreamHttpVersionPolicy = _versionPolicyCreator.Create(fileRoute.DownstreamHttpVersionPolicy);
         │  │  │         var cacheOptions = _cacheOptionsCreator.Create(fileRoute, globalConfiguration, lbKey);
         │  │  │     (stopped at depth 6; 1 branch omitted)
         │  │  └─ call FileConfiguration.Select  (src/Configuration/Creator/StaticRoutesCreator.cs:70) [approx]
         │  │         public virtual int CreateTimeout(FileRoute route, FileGlobalConfiguration global)
         │  │         {
         │  ├─ call AggregatesCreator.Create  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:41) [verified]
         │  │      var mergedRoutes = routes
         │  │      .Union(aggregates)
         │  │      .Union(dynamicRoute)
         │  │  ├─ call FileConfiguration.Select  (src/Configuration/Creator/AggregatesCreator.cs:19) [approx]
         │  │  │      .ToList();
         │  │  │      }
         │  │  └─ call AggregatesCreator.SetUpAggregateRoute  (src/Configuration/Creator/AggregatesCreator.cs:20) [verified]
         │  │         }
         │  │         private Route SetUpAggregateRoute(IEnumerable<Route> routes, FileAggregateRoute aggregateRoute, FileGlobalConfiguration globalConfiguration)
         │  │     ├─ call FileAggregateRoute.Select  (src/Configuration/Creator/AggregatesCreator.cs:29) [approx]
         │  │     │      if (downstreamRoute == null)
         │  │     │      {
         │  │     │      return null;
         │  │     ├─ call UpstreamTemplatePatternCreator.Create  (src/Configuration/Creator/AggregatesCreator.cs:40) [verified]
         │  │     │      return new Route()
         │  │     │      {
         │  │     │      Aggregator = aggregateRoute.Aggregator,
         │  │     │  (stopped at depth 6; 3 branches omitted)
         │  │     ├─ call UpstreamHeaderTemplatePatternCreator.Create  (src/Configuration/Creator/AggregatesCreator.cs:41) [verified]
         │  │     │      {
         │  │     │      Aggregator = aggregateRoute.Aggregator,
         │  │     │      DownstreamRoute = applicableRoutes,
         │  │     │  (stopped at depth 6; 1 branch omitted)
         │  │     └─ call FileAggregateRoute.ToHttpMethods  (src/Configuration/Creator/AggregatesCreator.cs:42) [approx]
         │  │            Aggregator = aggregateRoute.Aggregator,
         │  │            DownstreamRoute = applicableRoutes,
         │  │            DownstreamRouteConfig = aggregateRoute.RouteKeysConfig,
         │  ├─ call DynamicRoutesCreator.Create  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:43) [verified]
         │  │      .Union(dynamicRoute)
         │  │      .ToArray();
         │  │  ├─ call DynamicRoutesCreator.SetUpDynamicRoute  (src/Configuration/Creator/DynamicRoutesCreator.cs:47) [verified]
         │  │  │      }
         │  │  │      public virtual int CreateTimeout(FileDynamicRoute route, FileGlobalConfiguration global)
         │  │  │  (15 more branches omitted beyond fan-out)
         │  │  │  ├─ call LoadBalancerOptionsCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:68) [verified]
         │  │  │  │      var authOptions = _authOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │      var version = _versionCreator.Create(dynamicRoute.DownstreamHttpVersion.IfEmpty(globalConfiguration.DownstreamHttpVersion));
         │  │  │  │      var versionPolicy = _versionPolicyCreator.Create(dynamicRoute.DownstreamHttpVersionPolicy.IfEmpty(globalConfiguration.DownstreamHttpVersionPolicy));
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call RouteKeyCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:69) [verified]
         │  │  │  │      var version = _versionCreator.Create(dynamicRoute.DownstreamHttpVersion.IfEmpty(globalConfiguration.DownstreamHttpVersion));
         │  │  │  │      var versionPolicy = _versionPolicyCreator.Create(dynamicRoute.DownstreamHttpVersionPolicy.IfEmpty(globalConfiguration.DownstreamHttpVersionPolicy));
         │  │  │  │      var scheme = dynamicRoute.DownstreamScheme.IfEmpty(globalConfiguration.DownstreamScheme);
         │  │  │  │  (stopped at depth 6; 6 branches omitted)
         │  │  │  ├─ call CacheOptionsCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:70) [verified]
         │  │  │  │      var versionPolicy = _versionPolicyCreator.Create(dynamicRoute.DownstreamHttpVersionPolicy.IfEmpty(globalConfiguration.DownstreamHttpVersionPolicy));
         │  │  │  │      var scheme = dynamicRoute.DownstreamScheme.IfEmpty(globalConfiguration.DownstreamScheme);
         │  │  │  │      var handlerOptions = _httpHandlerOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call AuthenticationOptionsCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:72) [verified]
         │  │  │  │      var handlerOptions = _httpHandlerOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │      var metadata = _metadataCreator.Create(dynamicRoute.Metadata, globalConfiguration);
         │  │  │  │      var qosOptions = _qosOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │  (stopped at depth 6; 2 branches omitted)
         │  │  │  ├─ call HttpVersionCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:73) [verified]
         │  │  │  │      var metadata = _metadataCreator.Create(dynamicRoute.Metadata, globalConfiguration);
         │  │  │  │      var qosOptions = _qosOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │      var rlOptions = _rateLimitOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  ├─ call FileDynamicRoute.IfEmpty  (src/Configuration/Creator/DynamicRoutesCreator.cs:73) [approx]
         │  │  │  │      var metadata = _metadataCreator.Create(dynamicRoute.Metadata, globalConfiguration);
         │  │  │  │      var qosOptions = _qosOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │      var rlOptions = _rateLimitOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  ├─ call HttpVersionPolicyCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:74) [verified]
         │  │  │  │      var qosOptions = _qosOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │      var rlOptions = _rateLimitOptionsCreator.Create(dynamicRoute, globalConfiguration);
         │  │  │  │      var timeout = CreateTimeout(dynamicRoute, globalConfiguration);
         │  │  │  ├─ call HttpHandlerOptionsCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:76) [verified]
         │  │  │  │      var timeout = CreateTimeout(dynamicRoute, globalConfiguration);
         │  │  │  │      var downstreamRoute = new DownstreamRouteBuilder()
         │  │  │  │      .WithAuthenticationOptions(authOptions)
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call DefaultMetadataCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:77) [verified]
         │  │  │  │      var downstreamRoute = new DownstreamRouteBuilder()
         │  │  │  │      .WithAuthenticationOptions(authOptions)
         │  │  │  │      .WithCacheOptions(cacheOptions)
         │  │  │  │  (stopped at depth 6; 7 branches omitted)
         │  │  │  ├─ call QoSOptionsCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:78) [verified]
         │  │  │  │      .WithAuthenticationOptions(authOptions)
         │  │  │  │      .WithCacheOptions(cacheOptions)
         │  │  │  │      .WithDownstreamHttpVersion(version)
         │  │  │  │  (stopped at depth 6; 2 branches omitted)
         │  │  │  ├─ call RateLimitOptionsCreator.Create  (src/Configuration/Creator/DynamicRoutesCreator.cs:79) [verified]
         │  │  │  │      .WithCacheOptions(cacheOptions)
         │  │  │  │      .WithDownstreamHttpVersion(version)
         │  │  │  │      .WithDownstreamHttpVersionPolicy(versionPolicy)
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  └─ call DynamicRoutesCreator.CreateTimeout  (src/Configuration/Creator/DynamicRoutesCreator.cs:80) [verified]
         │  │  │         .WithDownstreamHttpVersion(version)
         │  │  │         .WithDownstreamHttpVersionPolicy(versionPolicy)
         │  │  │         .WithDownstreamScheme(scheme)
         │  │  │     (stopped at depth 6; 2 branches omitted)
         │  │  └─ call FileConfiguration.Select  (src/Configuration/Creator/DynamicRoutesCreator.cs:48) [approx]
         │  │         public virtual int CreateTimeout(FileDynamicRoute route, FileGlobalConfiguration global)
         │  │         {
         │  └─ call ConfigurationCreator.Create  (src/Configuration/Creator/FileInternalConfigurationCreator.cs:50) [verified]
         │         }
         │     ├─ call AuthenticationOptionsCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:51) [verified]
         │     │      var httpHandlerOptions = _httpHandlerOptionsCreator.Create(globalConfiguration.HttpHandlerOptions);
         │     │      var version = _versionCreator.Create(globalConfiguration.DownstreamHttpVersion);
         │     │      var versionPolicy = _versionPolicyCreator.Create(globalConfiguration.DownstreamHttpVersionPolicy);
         │     │  (stopped at depth 5; 2 branches omitted)
         │     ├─ call ServiceProviderConfigurationCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:52) [verified]
         │     │      var version = _versionCreator.Create(globalConfiguration.DownstreamHttpVersion);
         │     │      var versionPolicy = _versionPolicyCreator.Create(globalConfiguration.DownstreamHttpVersionPolicy);
         │     │      var metadataOptions = _metadataCreator.Create(null, globalConfiguration);
         │     │  ├─ call ServiceProviderConfigurationBuilder.Build  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  ├─ call ServiceProviderConfigurationBuilder.WithNamespace  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  ├─ call ServiceProviderConfigurationBuilder.WithPollingInterval  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  ├─ call ServiceProviderConfigurationBuilder.WithConfigurationKey  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  ├─ call ServiceProviderConfigurationBuilder.WithToken  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  ├─ call ServiceProviderConfigurationBuilder.WithType  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  ├─ call ServiceProviderConfigurationBuilder.WithPort  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  ├─ call ServiceProviderConfigurationBuilder.WithHost  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │  │      .WithPort(port)
         │     │  │      .WithType(type)
         │     │  │      .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     │  └─ call ServiceProviderConfigurationBuilder.WithScheme  (src/Configuration/Creator/ServiceProviderConfigurationCreator.cs:19) [verified]
         │     │         .WithPort(port)
         │     │         .WithType(type)
         │     │         .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
         │     ├─ call LoadBalancerOptionsCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:53) [verified]
         │     │      var versionPolicy = _versionPolicyCreator.Create(globalConfiguration.DownstreamHttpVersionPolicy);
         │     │      var metadataOptions = _metadataCreator.Create(null, globalConfiguration);
         │     │      var rateLimitOptions = _rateLimitOptionsCreator.Create(globalConfiguration);
         │     │  (stopped at depth 5; 1 branch omitted)
         │     ├─ call QoSOptionsCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:54) [verified]
         │     │      var metadataOptions = _metadataCreator.Create(null, globalConfiguration);
         │     │      var rateLimitOptions = _rateLimitOptionsCreator.Create(globalConfiguration);
         │     │      var cacheOptions = _cacheOptionsCreator.Create(globalConfiguration.CacheOptions);
         │     │  (stopped at depth 5; 2 branches omitted)
         │     ├─ call HttpHandlerOptionsCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:55) [verified]
         │     │      var rateLimitOptions = _rateLimitOptionsCreator.Create(globalConfiguration);
         │     │      var cacheOptions = _cacheOptionsCreator.Create(globalConfiguration.CacheOptions);
         │     │  (stopped at depth 5; 1 branch omitted)
         │     ├─ call HttpVersionCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:56) [verified]
         │     │      var cacheOptions = _cacheOptionsCreator.Create(globalConfiguration.CacheOptions);
         │     │      return new InternalConfiguration(routes)
         │     ├─ call HttpVersionPolicyCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:57) [verified]
         │     │      return new InternalConfiguration(routes)
         │     │      {
         │     ├─ call DefaultMetadataCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:58) [verified]
         │     │      return new InternalConfiguration(routes)
         │     │      {
         │     │      AdministrationPath = adminPath,
         │     │  (stopped at depth 5; 7 branches omitted)
         │     ├─ call RateLimitOptionsCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:59) [verified]
         │     │      {
         │     │      AdministrationPath = adminPath,
         │     │      AuthenticationOptions = authOptions,
         │     │  (stopped at depth 5; 1 branch omitted)
         │     └─ call CacheOptionsCreator.Create  (src/Configuration/Creator/ConfigurationCreator.cs:60) [verified]
         │            AdministrationPath = adminPath,
         │            AuthenticationOptions = authOptions,
         │            CacheOptions = cacheOptions,
         │        (stopped at depth 5; 1 branch omitted)
         └─ call InMemoryInternalConfigurationRepository.AddOrReplace  (src/Configuration/Repository/FileAndInternalConfigurationSetter.cs:39) [verified]
            └─ call OcelotConfigurationChangeTokenSource.Activate  (src/Configuration/Repository/InMemoryInternalConfigurationRepository.cs:36) [verified]
                   }
                   }
               └─ call OcelotConfigurationChangeToken.Activate  (src/Configuration/ChangeTracking/OcelotConfigurationChangeTokenSource.cs:13) [verified]
                      }
                  └─ call CallbackWrapper.Invoke  (src/Configuration/ChangeTracking/OcelotConfigurationChangeToken.cs:31) [verified]
                         }
                         }
RESULT   200 OK / 201 Created · failure → 400 Bad Request
