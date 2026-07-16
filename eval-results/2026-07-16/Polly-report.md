# REPORT
**Polly**

Style: MinimalApi
_7 projects  ·  net10.0, net10.0;net9.0;net8.0 + minimal-apis + polly_

## Stats

| Metric | Value |
|--------|-------|
| Files | 797 |
| Projects | 21 |
| Nodes | 1207 |
| Edges | 893 |
| Entries | 0 |
| With target | 0/0 |
| Verified edges | 59% |
| Analyzed in | 21.4s |

## Top Flows

_No entries found._

## Insights

_3 info · 1 notable_

### **NOTABLE**: Most depended-upon: Polly.RateLimiting (5 dependents) · Polly.Core (4 dependents) · Polly.Extensions (4 dependents)
*(Topology)*

- Polly.RateLimiting (5 dependents)
- Polly.Core (4 dependents)
- Polly.Extensions (4 dependents)

### _INFO_: Public surface: 23 interfaces, 446 classes (519 total public types)
*(Shape)*

- 23 interfaces
- 446 classes

### _INFO_: DI: 58 Extension · 2 Singleton (60 total)
*(Wiring)*

### _INFO_: Wiring hubs: PolicyBuilder (160) · StubCacheProvider (108) · Policy (38) · Func (38) · ServiceCollection (29)
*(Wiring)*

- PolicyBuilder (160)
- StubCacheProvider (108)
- Policy (38)
- Func (38)
- ServiceCollection (29)

LIBRARY  Polly     (187 public types)

ENTRY API
   register  PollyServiceCollectionExtensions.AddResiliencePipeline   (PollyServiceCollectionExtensions.cs)
      Adds a resilience pipeline that handles to service collection.
   register  PollyServiceCollectionExtensions.AddResiliencePipelineRegistry   (PollyServiceCollectionExtensions.cs)
      Adds and to the service collection that allows configuring and retrieving resilience pipelines using the key.
   register  PollyServiceCollectionExtensions.AddResiliencePipelines   (PollyServiceCollectionExtensions.cs)
      Allows deferred addition of one or more resilience pipelines to the service collection.
   build     PredicateBuilder   (PredicateBuilder.cs)
      Defines a builder for creating exception predicates.
   build     ResiliencePipelineBuilder   (ResiliencePipelineBuilder.cs)
      A builder that is used to create an instance of .
   derive    AsyncPolicy   (AsyncPolicy.ContextAndKeys.cs)
   derive    Policy   (Policy.ContextAndKeys.cs)
   derive    ResilienceStrategy   (ResilienceStrategy.cs)
      Base class for all proactive resilience strategies.
   derive    ResilienceStrategyOptions   (ResilienceStrategyOptions.cs)
      The options associated with the individual resilience strategy.
   extend    AdvancedCircuitBreakerSyntax   (AdvancedCircuitBreakerSyntax.cs)
      Fluent API for defining a Circuit Breaker .
   extend    AdvancedCircuitBreakerTResultSyntax   (AdvancedCircuitBreakerTResultSyntax.cs)
      Fluent API for defining a Circuit Breaker .
   extend    AsyncAdvancedCircuitBreakerSyntax   (AsyncAdvancedCircuitBreakerSyntax.cs)
      Fluent API for defining a Circuit Breaker .

ABSTRACTIONS
   AsyncPolicy (class)  — 24 implementors
   Policy (class)  — 24 implementors
   ResilienceStrategy (class)  — 22 implementors
   ResilienceStrategyOptions (class)  — 15 implementors
   IsPolicy (interface)  — 11 implementors
   ITtlStrategy (interface)  — 7 implementors
   ChaosStrategy (class)  — 6 implementors
   ExecutionRejectedException (class)  — 6 implementors
   IAsyncCacheProvider (interface)  — 6 implementors
   ISyncCacheProvider (interface)  — 6 implementors

PUBLIC SURFACE
   Polly
      AdvancedCircuitBreakerSyntax (class):  AdvancedCircuitBreaker
         Fluent API for defining a Circuit Breaker .
      AdvancedCircuitBreakerTResultSyntax (class):  AdvancedCircuitBreaker
         Fluent API for defining a Circuit Breaker .
      AsyncAdvancedCircuitBreakerSyntax (class):  AdvancedCircuitBreakerAsync
         Fluent API for defining a Circuit Breaker .
      AsyncAdvancedCircuitBreakerTResultSyntax (class):  AdvancedCircuitBreakerAsync
         Fluent API for defining a Circuit Breaker .
      AsyncCircuitBreakerSyntax (class):  CircuitBreakerAsync
         Fluent API for defining a Circuit Breaker .
      AsyncCircuitBreakerTResultSyntax (class):  CircuitBreakerAsync
         Fluent API for defining a Circuit Breaker .
      AsyncFallbackSyntax (class):  FallbackAsync
         Fluent API for defining a Fallback .
      AsyncFallbackTResultSyntax (class):  FallbackAsync
         Fluent API for defining an async Fallback policy governing executions returning TResult.
      AsyncPolicy (class):  ExecuteAndCaptureAsync, ExecuteAsync, WithPolicyKey, WrapAsync
      AsyncRetrySyntax (class):  RetryAsync, RetryForeverAsync, WaitAndRetryAsync, WaitAndRetryForeverAsync
         Fluent API for defining a .
      AsyncRetryTResultSyntax (class):  RetryAsync, RetryForeverAsync, WaitAndRetryAsync, WaitAndRetryForeverAsync
         Fluent API for defining an .
      CircuitBreakerResiliencePipelineBuilderExtensions (class):  AddCircuitBreaker
         Circuit breaker extensions for .
      … and 46 more (use --format json for the full surface)
   Polly.Bulkhead
      AsyncBulkheadPolicy (class):  Dispose
         A bulkhead-isolation policy which can be applied to delegates.
      BulkheadPolicy (class):  Dispose
         A bulkhead-isolation policy which can be applied to delegates.
      BulkheadRejectedException (class):  BulkheadRejectedException
         Exception thrown when a bulkhead's semaphore and queue are full.
      IBulkheadPolicy (interface)
         Defines properties and methods common to all bulkhead policies.
   Polly.Caching
      AbsoluteTtl (class):  AbsoluteTtl
         Defines a ttl strategy which will cache items until the specified point-in-time.
      AsyncCachePolicy (class)
         A cache policy that can be applied to the results of delegate executions.
      AsyncSerializingCacheProvider (class):  AsyncSerializingCacheProvider, PutAsync, TryGetAsync
         Defines an which serializes objects of any type in and out of an underlying cache which caches as type .
      CachePolicy (class)
         A cache policy that can be applied to the results of delegate executions.
      CacheProviderExtensions (class):  AsyncFor, For, WithSerializer
         Class that provides helper methods for configuring CacheProviders.
      ContextualTtl (class):  GetTtl
         Defines a ttl strategy which will cache items for a TimeSpan which may be influenced by data in the execution context.
      DefaultCacheKeyStrategy (class):  GetCacheKey
         The default cache key strategy for .
      IAsyncCacheProvider (interface):  PutAsync, TryGetAsync
         Defines methods for classes providing asynchronous cache functionality for Polly s.
      ICacheItemSerializer (interface):  Deserialize, Serialize
         Defines operations for serializing and deserializing values being placed in caches by instances.
      ICacheKeyStrategy (interface):  GetCacheKey
         Defines how a should get a string cache key from an execution .
      ICachePolicy (interface)
         Defines properties and methods common to all Cache policies.
      ISyncCacheProvider (interface):  Put, TryGet
         Defines methods for classes providing synchronous cache functionality for Polly s.
      … and 7 more (use --format json for the full surface)
   Polly.CircuitBreaker
      AsyncCircuitBreakerPolicy (class):  Isolate, Reset
         A circuit-breaker policy that can be applied to async delegates.
      BreakDurationGeneratorArguments (struct):  BreakDurationGeneratorArguments
         Represents arguments used to generate a dynamic break duration for a circuit breaker.
      BrokenCircuitException (class):  BrokenCircuitException, GetObjectData
         Exception thrown when a circuit is broken.
      CircuitBreakerManualControl (class):  CircuitBreakerManualControl, CloseAsync, IsolateAsync
         Allows manual control of the circuit-breaker.
      CircuitBreakerPolicy (class):  Isolate, Reset
         A circuit-breaker policy that can be applied to delegates.
      CircuitBreakerPredicateArguments (struct):  CircuitBreakerPredicateArguments
         Arguments used by predicate.
      CircuitBreakerStateProvider (class)
         Allows retrieval of the circuit breaker state.
      CircuitBreakerStrategyOptions (class):  CircuitBreakerStrategyOptions
      ICircuitBreakerPolicy (interface):  Isolate, Reset
         Defines properties and methods common to all circuit-breaker policies.
      IsolatedCircuitException (class):  IsolatedCircuitException
         Exception thrown when a circuit is isolated (held open) by manual override.
      OnCircuitClosedArguments (struct):  OnCircuitClosedArguments
         Arguments used by event.
      OnCircuitHalfOpenedArguments (struct):  OnCircuitHalfOpenedArguments
         Arguments used by event.
      … and 1 more (use --format json for the full surface)
   Polly.DependencyInjection
      AddResiliencePipelineContext (class):  EnableReloads, GetOptions, OnPipelineDisposed
         Represents the context for adding a resilience pipeline with the specified key.
      AddResiliencePipelinesContext (class):  AddResiliencePipeline
         Represents the context for configuring resilience pipelines with the specified key.
   Polly.Fallback
      AsyncFallbackPolicy (class)
         A fallback policy that can be applied to asynchronous delegates.
      FallbackActionArguments (struct):  FallbackActionArguments
         Arguments used by .
      FallbackPolicy (class)
         A fallback policy that can be applied to delegates.
      FallbackPredicateArguments (struct):  FallbackPredicateArguments
         Represents arguments used in fallback handling scenarios.
      FallbackStrategyOptions (class):  FallbackStrategyOptions
         Represents the options for configuring a fallback resilience strategy with a specific result type.
      IFallbackPolicy (interface)
         Defines properties and methods common to all Fallback policies.
      OnFallbackArguments (struct):  OnFallbackArguments
         Represents arguments used in fallback handling scenarios.
   Polly.Hedging
      HedgingActionGeneratorArguments (struct):  HedgingActionGeneratorArguments
         Represents arguments used in the hedging resilience strategy.
      HedgingDelayGeneratorArguments (struct):  HedgingDelayGeneratorArguments
         Arguments used by hedging delay generator.
      HedgingPredicateArguments (struct):  HedgingPredicateArguments
         Represents arguments used in hedging handling scenarios.
      HedgingStrategyOptions (class):  HedgingStrategyOptions
         Hedging strategy options.
      OnHedgingArguments (struct):  OnHedgingArguments
         Represents arguments used by the on-hedging event.
   Polly.Hedging.Utils
      ExecutionInfo (record)
   Polly.NoOp
      AsyncNoOpPolicy (class)
         A noop policy that can be applied to asynchronous delegates.
      INoOpPolicy (interface)
         Defines properties and methods common to all NoOp policies.
      NoOpPolicy (class)
         A no op policy that can be applied to delegates.
   Polly.RateLimit
      AsyncRateLimitPolicy (class)
         A rate-limit policy that can be applied to asynchronous delegates.
      IRateLimitPolicy (interface)
         Defines properties and methods common to all RateLimit policies.
      RateLimitPolicy (class)
         A rate-limit policy that can be applied to synchronous delegates.
      RateLimitRejectedException (class):  RateLimitRejectedException
         Exception thrown when a delegate executed through a is rate-limited.
   Polly.RateLimiting
      OnRateLimiterRejectedArguments (struct):  OnRateLimiterRejectedArguments
         The arguments used by the .
      RateLimiterArguments (struct):  RateLimiterArguments
         The arguments used by the delegate.
      RateLimiterRejectedException (class):  GetObjectData, RateLimiterRejectedException
         Exception thrown when a rate limiter rejects an execution.
      RateLimiterStrategyOptions (class):  RateLimiterStrategyOptions
         Options for the rate limiter strategy.
   Polly.Registry
      ConfigureBuilderContext (class):  AddReloadToken, OnPipelineDisposed
         The context used by .
      ConfigureBuilderContextExtensions (class):  EnableReloads
         Extensions for .
      IConcurrentPolicyRegistry (interface):  AddOrUpdate, GetOrAdd, TryAdd, TryRemove, TryUpdate
         Represents a collection of policies keyed by which can be updated and consumed in a thread-safe manner.
      IPolicyRegistry (interface):  Add, Clear, Remove
         Represents a collection of policies keyed by .
      IReadOnlyPolicyRegistry (interface):  ContainsKey, Get, TryGet
         Represents a read-only collection of policies keyed by .
      PolicyRegistry (class):  Add, AddOrUpdate, Clear, ContainsKey, Get, GetEnumerator, GetOrAdd, PolicyRegistry, Remove, TryAdd, TryGet, TryRemove, TryUpdate
         Stores a registry of and policy pairs.
      ResiliencePipelineProvider (class):  GetPipeline, TryGetPipeline
         Represents a provider for resilience pipelines that are accessible by .
      ResiliencePipelineRegistry (class):  Dispose, DisposeAsync, GetOrAddPipeline, ResiliencePipelineRegistry, TryAddBuilder, TryGetPipeline
         Represents a registry of resilience pipelines and builders that are accessible by .
      ResiliencePipelineRegistryOptions (class)
         An options class used by .
   Polly.Retry
      AsyncRetryPolicy (class)
         A retry policy that can be applied to asynchronous delegates.
      IRetryPolicy (interface)
         Defines properties and methods common to all Retry policies.
      OnRetryArguments (struct):  OnRetryArguments
         Represents the arguments used by for handling the retry event.
      RetryDelayGeneratorArguments (struct):  RetryDelayGeneratorArguments
         Represents the arguments used by for generating the next retry delay.
      RetryPolicy (class)
         A retry policy that can be applied to synchronous delegates.
      RetryPredicateArguments (struct):  RetryPredicateArguments
         Represents the arguments used by for determining whether a retry should be performed.
      RetryStrategyOptions (class):  RetryStrategyOptions
   Polly.Simmy
      ChaosBehaviorPipelineBuilderExtensions (class):  AddChaosBehavior
         Extension methods for adding chaos behaviors to a .
      ChaosFaultPipelineBuilderExtensions (class):  AddChaosFault
         Extension methods for adding chaos fault strategy to a .
      ChaosLatencyPipelineBuilderExtensions (class):  AddChaosLatency
         Extension methods for adding chaos latency strategies to a .
      ChaosOutcomePipelineBuilderExtensions (class):  AddChaosOutcome
         Extension methods for adding chaos outcome strategy to a .
      ChaosStrategy (class)
         Base class for chaos strategies.
      ChaosStrategyOptions (class)
         The options associated with the .
      EnabledGeneratorArguments (struct):  EnabledGeneratorArguments
         Defines the arguments for the .
      InjectionRateGeneratorArguments (struct):  InjectionRateGeneratorArguments
         Defines the arguments for the .
   Polly.Simmy.Behavior
      BehaviorGeneratorArguments (struct):  BehaviorGeneratorArguments
         Arguments used by the behavior chaos strategy to execute a user's delegate custom action.
      ChaosBehaviorStrategyOptions (class):  ChaosBehaviorStrategyOptions
         Represents the options for the behavior chaos strategy.
      OnBehaviorInjectedArguments (struct):  OnBehaviorInjectedArguments
         Arguments used by the behavior chaos strategy to notify that a custom behavior was injected.
   Polly.Simmy.Fault
      ChaosFaultStrategyOptions (class):  ChaosFaultStrategyOptions
         Represents the options for the fault chaos strategy.
      FaultGenerator (class):  AddException, FaultGenerator
         A generator for creating faults (exceptions) using registered delegate functions.
      FaultGeneratorArguments (struct):  FaultGeneratorArguments
         Arguments used by the fault chaos strategy to ge the fault that is going to be injected.
      OnFaultInjectedArguments (struct):  OnFaultInjectedArguments
         Arguments used by the fault chaos strategy to notify that an fault was injected.
   Polly.Simmy.Latency
      ChaosLatencyStrategyOptions (class):  ChaosLatencyStrategyOptions
         Represents the options for the latency chaos strategy.
      LatencyGeneratorArguments (struct):  LatencyGeneratorArguments
         Arguments used by the .
      OnLatencyInjectedArguments (struct):  OnLatencyInjectedArguments
         Arguments used by the latency chaos strategy to notify that a latency was injected.
   Polly.Simmy.Outcomes
      ChaosOutcomeStrategyOptions (class):  ChaosOutcomeStrategyOptions
         Represents the options for the outcome chaos strategy.
      OnOutcomeInjectedArguments (struct):  OnOutcomeInjectedArguments
         Arguments used by the outcome chaos strategy to notify that an outcome was injected.
      OutcomeGenerator (class):  AddException, AddResult, OutcomeGenerator
         Generator that produces outcomes such as exceptions or results.
      OutcomeGeneratorArguments (struct):  OutcomeGeneratorArguments
         Arguments used by .
   Polly.Telemetry
      EnrichmentContext (struct):  EnrichmentContext
         Enrichment context used when reporting resilience events.
      ExecutionAttemptArguments (struct):  ExecutionAttemptArguments
         Arguments that encapsulate the execution attempt for retries or hedging.
      MeteringEnricher (class):  Enrich
         Enricher used to enrich the metrics with additional information.
      PipelineExecutedArguments (struct):  PipelineExecutedArguments
         Arguments that indicate the pipeline execution started.
      PipelineExecutingArguments (struct)
         Arguments that indicate the pipeline execution started.
      ResilienceEvent (struct):  ResilienceEvent, ToString
         Represents a resilience event that has been reported.
      ResilienceStrategyTelemetry (class):  Report, SetTelemetrySource
         Resilience telemetry is used by individual resilience strategies to report some important events.
      ResilienceTelemetrySource (class):  ResilienceTelemetrySource
         The source of resilience telemetry events.
      SeverityProviderArguments (struct):  SeverityProviderArguments
         Arguments used by .
      TelemetryEventArguments (struct):  TelemetryEventArguments
         Represents the information about the resilience event.
      TelemetryListener (class):  Write
         Listener of resilience telemetry events.
      TelemetryOptions (class):  TelemetryOptions
         The options that are used to configure the telemetry that is produced by the resilience strategies.
   Polly.Testing
      ResiliencePipelineDescriptor (class)
         Describes the resilience pipeline.
      ResiliencePipelineExtensions (class):  GetPipelineDescriptor
         The test-related extensions for and .
      ResilienceStrategyDescriptor (class)
         This class provides additional information about a .
   Polly.Timeout
      AsyncTimeoutPolicy (class)
         A timeout policy which can be applied to async delegates.
      ITimeoutPolicy (interface)
         Defines properties and methods common to all Timeout policies.
      OnTimeoutArguments (struct):  OnTimeoutArguments
         Arguments used by the timeout strategy to notify that a timeout occurred.
      TimeoutGeneratorArguments (struct):  TimeoutGeneratorArguments
         Arguments used by the timeout strategy to retrieve a timeout for current execution.
      TimeoutPolicy (class)
         A timeout policy which can be applied to delegates.
      TimeoutRejectedException (class):  GetObjectData, TimeoutRejectedException
         Exception thrown when a delegate executed through a timeout resilience strategy does not complete, before the configu...
      TimeoutStrategyOptions (class):  TimeoutStrategyOptions
         Represents the options for the timeout strategy.
   Polly.Utilities
      ExceptionExtensions (class):  RethrowWithOriginalStackTraceIfDiffersFrom
         Contains extension methods on the class.
      SystemClock (class):  Reset
         Time related delegates used to support different compilation targets and to improve testability of the code.
      TaskHelper (class)
         Task helpers.
   Polly.Wrap
      AsyncPolicyWrap (class)
      IPolicyWrap (interface)
         Defines properties and methods common to all PolicyWrap policies.
      IPolicyWrapExtension (class):  GetPolicies, GetPolicy
         Extension methods for IPolicyWrap.
      PolicyWrap (class)

CONSUMER PATHS
   wire into DI  →  PollyServiceCollectionExtensions.AddResiliencePipeline(...)
   wire into DI  →  PollyServiceCollectionExtensions.AddResiliencePipelineRegistry(...)
   wire into DI  →  PollyServiceCollectionExtensions.AddResiliencePipelines(...)
   build  →  new PredicateBuilder()…Build()
   build  →  new ResiliencePipelineBuilder()…Build()
   extend  →  derive AsyncPolicy

PACKAGES
   Other:  Microsoft.Bcl.AsyncInterfaces 6.0.0, Microsoft.Bcl.TimeProvider 8.0.0, Microsoft.Extensions.Logging.Abstractions 8.0.0, Microsoft.Extensions.Options 8.0.0, System.ComponentModel.Annotations 4.5.0, System.Diagnostics.DiagnosticSource 8.0.0, System.Threading.RateLimiting 8.0.0, System.Threading.Tasks.Extensions 4.5.4 … (9 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus PollyServiceCollectionExtensions)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 613ms |
| GenericExtraction | 4289ms |
| SignalSealing | 0ms |
| SpecificExtraction | 2390ms |
| Compression | 167ms |
| **Total** | **21440ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| ProgramCsFlowExtractor | 4286ms | 762 | 61 |
| SyntaxStructureExtractor | 4279ms | 762 | 60 |
| DiRegistrationExtractor | 4271ms | 0 | 60 |
| EndpointExtractor | 1831ms | 0 | 77 |
| BodyFactsExtractor | 549ms | 0 | 0 |
| CallGraphExtractor | 500ms | 0 | 0 |
| InMemoryEventBusExtractor | 500ms | 0 | 76 |
| SourceBodyExtractor | 498ms | 0 | 0 |
| ProjectStructure | 388ms | 0 | 0 |
| IndirectWiringDetector | 187ms | 0 | 76 |
| FileTreeExtractor | 146ms | 0 | 0 |
| SolutionDiscovery | 71ms | 0 | 0 |
| DependencyExtractor | 22ms | 0 | 0 |
| LayerClassifier | 19ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 874 | 363 |
| Resolves | 19 | 6 |

_797 files · 21 projects_
