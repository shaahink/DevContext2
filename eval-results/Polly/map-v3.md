Overview map (no focus).
Analyzing project...

LIBRARY  Polly     (188 public types)

STYLE  MinimalApi

ENTRY API
   register  PollyServiceCollectionExtensions.AddResiliencePipeline   
(PollyServiceCollectionExtensions.cs)
      Adds a resilience pipeline that handles to service collection.
   register  PollyServiceCollectionExtensions.AddResiliencePipelineRegistry   
(PollyServiceCollectionExtensions.cs)
      Adds and to the service collection that allows configuring and retrieving 
resilience pipelines using the key.
   register  PollyServiceCollectionExtensions.AddResiliencePipelines   
(PollyServiceCollectionExtensions.cs)
      Allows deferred addition of one or more resilience pipelines to the 
service collection.
   derive    AsyncPolicy   (AsyncPolicy.ContextAndKeys.cs)
   derive    Policy   (Policy.ContextAndKeys.cs)
   derive    ResilienceStrategy   (ResilienceStrategy.cs)
      Base class for all proactive resilience strategies.
   derive    ResilienceStrategyOptions   (ResilienceStrategyOptions.cs)
      The options associated with the individual resilience strategy.
   extend    AdvancedCircuitBreakerSyntax   (AdvancedCircuitBreakerSyntax.cs)
      Fluent API for defining a Circuit Breaker .
   extend    AdvancedCircuitBreakerTResultSyntax   
(AdvancedCircuitBreakerTResultSyntax.cs)
      Fluent API for defining a Circuit Breaker .
   extend    AsyncAdvancedCircuitBreakerSyntax   
(AsyncAdvancedCircuitBreakerSyntax.cs)
      Fluent API for defining a Circuit Breaker .
   extend    AsyncAdvancedCircuitBreakerTResultSyntax   
(AsyncAdvancedCircuitBreakerTResultSyntax.cs)
      Fluent API for defining a Circuit Breaker .
   extend    AsyncCircuitBreakerSyntax   (AsyncCircuitBreakerSyntax.cs)
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
      AsyncAdvancedCircuitBreakerTResultSyntax (class):  
AdvancedCircuitBreakerAsync
         Fluent API for defining a Circuit Breaker .
      AsyncCircuitBreakerSyntax (class):  CircuitBreakerAsync
         Fluent API for defining a Circuit Breaker .
      AsyncCircuitBreakerTResultSyntax (class):  CircuitBreakerAsync
         Fluent API for defining a Circuit Breaker .
      AsyncFallbackSyntax (class):  FallbackAsync
         Fluent API for defining a Fallback .
      AsyncFallbackTResultSyntax (class):  FallbackAsync
         Fluent API for defining an async Fallback policy governing executions 
returning TResult.
      AsyncPolicy (class):  ExecuteAndCaptureAsync, ExecuteAsync, WithPolicyKey,
WrapAsync
      AsyncRetrySyntax (class):  RetryAsync, RetryForeverAsync, 
WaitAndRetryAsync, WaitAndRetryForeverAsync
         Fluent API for defining a .
      AsyncRetryTResultSyntax (class):  RetryAsync, RetryForeverAsync, 
WaitAndRetryAsync, WaitAndRetryForeverAsync
         Fluent API for defining an .
      CircuitBreakerResiliencePipelineBuilderExtensions (class):  
AddCircuitBreaker
         Circuit breaker extensions for .
      CircuitBreakerSyntax (class):  CircuitBreaker
         Fluent API for defining a Circuit Breaker .
      CircuitBreakerTResultSyntax (class):  CircuitBreaker
         Fluent API for defining a Circuit Breaker .
      Context (class):  Add, Clear, Contains, ContainsKey, Context, CopyTo, 
GetEnumerator, Remove, TryGetValue
         Context that carries with a single execution through a Policy.
      DelegateResult (class):  DelegateResult
         The captured outcome of executing an individual FuncTResult.
      ExceptionPredicates (class):  FirstMatchOrDefault
         A collection of predicates used to define whether a policy handles a 
given .
      ExecutionRejectedException (class)
         Exception thrown when a policy rejects execution of a delegate.
      FallbackResiliencePipelineBuilderExtensions (class):  AddFallback
         Extensions for adding fallback to .
      FallbackSyntax (class):  Fallback
         Fluent API for defining a Fallback policy.
      FallbackTResultSyntax (class):  Fallback
         Fluent API for defining a Fallback policy governing executions 
returning TResult.
      HedgingResiliencePipelineBuilderExtensions (class):  AddHedging
         Extensions for adding hedging to .
      IAsyncPolicy (interface):  ExecuteAndCaptureAsync, ExecuteAsync, 
WithPolicyKey
         An interface defining all executions available on a non-generic, 
asynchronous policy.
      IAsyncPolicyExtensions (class):  AsAsyncPolicy
         Contains extensions methods on .
      IAsyncPolicyPolicyWrapExtensions (class):  WrapAsync
         Defines extensions for configuring instances on an or .
      ISyncPolicy (interface):  Execute, ExecuteAndCapture, WithPolicyKey
         An interface defining all executions available on a non-generic, 
synchronous policy.
      ISyncPolicyExtensions (class):  AsPolicy
         Contains extensions methods on .
      ISyncPolicyPolicyWrapExtensions (class):  Wrap
         Defines extensions for configuring instances on an or .
      IsPolicy (interface)
         A marker interface identifying Polly policies of all types, and 
containing properties common to all policies.
      LegacySupport (class):  SetProperties
         Legacy support for older versions of Polly.
      Outcome (class):  FromException, FromExceptionAsValueTask, FromResult, 
FromResultAsValueTask, ThrowIfException, ToString
         Produces instances of .
      Policy (class):  Bulkhead, BulkheadAsync, Cache, CacheAsync, Execute, 
ExecuteAndCapture, Handle, HandleInner, HandleResult, NoOp, NoOpAsync, 
RateLimit, RateLimitAsync, Timeout, TimeoutAsync
      PolicyBase (class)
      PolicyBuilder (class):  Equals, GetHashCode, GetType, Or, OrInner, 
OrResult, ToString
         Builder class that holds the list of current exception predicates.
      PolicyResult (class):  Failure, Successful
         The captured result of executing a policy.
      PollyServiceCollectionExtensions (class):  AddResiliencePipeline, 
AddResiliencePipelineRegistry, AddResiliencePipelines
         Provides extension methods for registering resilience pipelines using 
the .
      PredicateBuilder (class):  Build, Handle, HandleInner, HandleResult
         Defines a builder for creating exception predicates.
      PredicateResult (class):  False, True
         Class that represents the results that can be used in predicates.
      RateLimiterResiliencePipelineBuilderExtensions (class):  
AddConcurrencyLimiter, AddRateLimiter
         Extensions for adding rate limiting to .
      ResilienceContext (class)
         A context assigned to a single execution of .
      ResilienceContextCreationArguments (struct):  
ResilienceContextCreationArguments
         Arguments used by the when creating .
      ResilienceContextPool (class):  Get, Return
         The pool of instances.
      ResiliencePipeline (class):  Execute, ExecuteAsync, ExecuteOutcomeAsync
      ResiliencePipelineBuilder (class):  Build, ResiliencePipelineBuilder
         A builder that is used to create an instance of .
      ResiliencePipelineBuilderBase (class)
         A builder that is used to create an instance of .
      ResiliencePipelineBuilderExtensions (class):  AddPipeline, AddStrategy
         Extensions for .
      ResiliencePipelineConversionExtensions (class):  AsAsyncPolicy, 
AsSyncPolicy
         Extensions for conversion of resilience strategies to policies.
      ResilienceProperties (class):  GetValue, Set, TryGetValue
         Represents a collection of custom resilience properties.
      ResiliencePropertyKey (struct):  ResiliencePropertyKey, ToString
         Represents a key used by .
      ResilienceStrategy (class)
         Base class for all proactive resilience strategies.
      ResilienceStrategyOptions (class)
         The options associated with the individual resilience strategy.
      ResultPredicates (class):  AnyMatch
         A collection of predicates used to define whether a policy handles a 
given value.
      RetryResiliencePipelineBuilderExtensions (class):  AddRetry
         Extensions for adding retries to .
      RetrySyntax (class):  Retry, RetryForever, WaitAndRetry, 
WaitAndRetryForever
         Fluent API for defining a Retry .
      RetryTResultSyntax (class):  Retry, RetryForever, WaitAndRetry, 
WaitAndRetryForever
         Fluent API for defining a Retry .
      StrategyBuilderContext (class)
         The context used for building an individual resilience strategy.
      TelemetryResiliencePipelineBuilderExtensions (class):  ConfigureTelemetry
         The telemetry extensions for the .
      TimeoutResiliencePipelineBuilderExtensions (class):  AddTimeout
         Extensions for adding timeout to .
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
         Defines a ttl strategy which will cache items until the specified 
point-in-time.
      AsyncCachePolicy (class)
         A cache policy that can be applied to the results of delegate 
executions.
      AsyncSerializingCacheProvider (class):  AsyncSerializingCacheProvider, 
PutAsync, TryGetAsync
         Defines an which serializes objects of any type in and out of an 
underlying cache which caches as type .
      CachePolicy (class)
         A cache policy that can be applied to the results of delegate 
executions.
      CacheProviderExtensions (class):  AsyncFor, For, WithSerializer
         Class that provides helper methods for configuring CacheProviders.
      ContextualTtl (class):  GetTtl
         Defines a ttl strategy which will cache items for a TimeSpan which may 
be influenced by data in the execution context.
      DefaultCacheKeyStrategy (class):  GetCacheKey
         The default cache key strategy for .
      IAsyncCacheProvider (interface):  PutAsync, TryGetAsync
         Defines methods for classes providing asynchronous cache functionality 
for Polly s.
      ICacheItemSerializer (interface):  Deserialize, Serialize
         Defines operations for serializing and deserializing values being 
placed in caches by instances.
      ICacheKeyStrategy (interface):  GetCacheKey
         Defines how a should get a string cache key from an execution .
      ICachePolicy (interface)
         Defines properties and methods common to all Cache policies.
      ISyncCacheProvider (interface):  Put, TryGet
         Defines methods for classes providing synchronous cache functionality 
for Polly s.
      ITtlStrategy (interface):  GetTtl
         Defines a strategy for providing time-to-live durations for cacheable 
results.
      NonSlidingTtl (class):  GetTtl
         Represents an expiring at an absolute time, not with sliding 
expiration.
      RelativeTtl (class):  GetTtl, RelativeTtl
         Defines a ttl strategy which will cache items for the specified time.
      ResultTtl (class):  GetTtl, ResultTtl
         Defines a ttl strategy which can calculate a duration to cache items 
dynamically based on the execution context and r...
      SerializingCacheProvider (class):  Put, SerializingCacheProvider, TryGet
         Defines an which serializes objects of any type in and out of an 
underlying cache which caches as type .
      SlidingTtl (class):  GetTtl, SlidingTtl
         Defines a ttl strategy which will cache items with a sliding ttl.
      Ttl (struct):  Ttl
         Represents a time-to-live for a given cache item.
   Polly.CircuitBreaker
      AsyncCircuitBreakerPolicy (class):  Isolate, Reset
         A circuit-breaker policy that can be applied to async delegates.
      BreakDurationGeneratorArguments (struct):  BreakDurationGeneratorArguments
         Represents arguments used to generate a dynamic break duration for a 
circuit breaker.
      BrokenCircuitException (class):  BrokenCircuitException, GetObjectData
         Exception thrown when a circuit is broken.
      CircuitBreakerManualControl (class):  CircuitBreakerManualControl, 
CloseAsync, IsolateAsync
         Allows manual control of the circuit-breaker.
      CircuitBreakerPolicy (class):  Isolate, Reset
         A circuit-breaker policy that can be applied to delegates.
      CircuitBreakerPredicateArguments (struct):  
CircuitBreakerPredicateArguments
         Arguments used by predicate.
      CircuitBreakerStateProvider (class)
         Allows retrieval of the circuit breaker state.
      CircuitBreakerStrategyOptions (class):  CircuitBreakerStrategyOptions
      ICircuitBreakerPolicy (interface):  Isolate, Reset
         Defines properties and methods common to all circuit-breaker policies.
      IsolatedCircuitException (class):  IsolatedCircuitException
         Exception thrown when a circuit is isolated (held open) by manual 
override.
      OnCircuitClosedArguments (struct):  OnCircuitClosedArguments
         Arguments used by event.
      OnCircuitHalfOpenedArguments (struct):  OnCircuitHalfOpenedArguments
         Arguments used by event.
      OnCircuitOpenedArguments (struct):  OnCircuitOpenedArguments
         Arguments used by event.
   Polly.DependencyInjection
      AddResiliencePipelineContext (class):  EnableReloads, GetOptions, 
OnPipelineDisposed
         Represents the context for adding a resilience pipeline with the 
specified key.
      AddResiliencePipelinesContext (class):  AddResiliencePipeline
         Represents the context for configuring resilience pipelines with the 
specified key.
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
         Represents the options for configuring a fallback resilience strategy 
with a specific result type.
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
      RateLimiterRejectedException (class):  GetObjectData, 
RateLimiterRejectedException
         Exception thrown when a rate limiter rejects an execution.
      RateLimiterStrategyOptions (class):  RateLimiterStrategyOptions
         Options for the rate limiter strategy.
   Polly.Registry
      ConfigureBuilderContext (class):  AddReloadToken, OnPipelineDisposed
         The context used by .
      ConfigureBuilderContextExtensions (class):  EnableReloads
         Extensions for .
      IConcurrentPolicyRegistry (interface):  AddOrUpdate, GetOrAdd, TryAdd, 
TryRemove, TryUpdate
         Represents a collection of policies keyed by which can be updated and 
consumed in a thread-safe manner.
      IPolicyRegistry (interface):  Add, Clear, Remove
         Represents a collection of policies keyed by .
      IReadOnlyPolicyRegistry (interface):  ContainsKey, Get, TryGet
         Represents a read-only collection of policies keyed by .
      PolicyRegistry (class):  Add, AddOrUpdate, Clear, ContainsKey, Get, 
GetEnumerator, GetOrAdd, PolicyRegistry, Remove, TryAdd, TryGet, TryRemove, 
TryUpdate
         Stores a registry of and policy pairs.
      ResiliencePipelineProvider (class):  GetPipeline, TryGetPipeline
         Represents a provider for resilience pipelines that are accessible by .
      ResiliencePipelineRegistry (class):  Dispose, DisposeAsync, 
GetOrAddPipeline, ResiliencePipelineRegistry, TryAddBuilder, TryGetPipeline
         Represents a registry of resilience pipelines and builders that are 
accessible by .
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
         Represents the arguments used by for determining whether a retry should
be performed.
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
         Arguments used by the behavior chaos strategy to execute a user's 
delegate custom action.
      ChaosBehaviorStrategyOptions (class):  ChaosBehaviorStrategyOptions
         Represents the options for the behavior chaos strategy.
      OnBehaviorInjectedArguments (struct):  OnBehaviorInjectedArguments
         Arguments used by the behavior chaos strategy to notify that a custom 
behavior was injected.
   Polly.Simmy.Fault
      ChaosFaultStrategyOptions (class):  ChaosFaultStrategyOptions
         Represents the options for the fault chaos strategy.
      FaultGenerator (class):  AddException, FaultGenerator
         A generator for creating faults (exceptions) using registered delegate 
functions.
      FaultGeneratorArguments (struct):  FaultGeneratorArguments
         Arguments used by the fault chaos strategy to ge the fault that is 
going to be injected.
      OnFaultInjectedArguments (struct):  OnFaultInjectedArguments
         Arguments used by the fault chaos strategy to notify that an fault was 
injected.
   Polly.Simmy.Latency
      ChaosLatencyStrategyOptions (class):  ChaosLatencyStrategyOptions
         Represents the options for the latency chaos strategy.
      LatencyGeneratorArguments (struct):  LatencyGeneratorArguments
         Arguments used by the .
      OnLatencyInjectedArguments (struct):  OnLatencyInjectedArguments
         Arguments used by the latency chaos strategy to notify that a latency 
was injected.
   Polly.Simmy.Outcomes
      ChaosOutcomeStrategyOptions (class):  ChaosOutcomeStrategyOptions
         Represents the options for the outcome chaos strategy.
      OnOutcomeInjectedArguments (struct):  OnOutcomeInjectedArguments
         Arguments used by the outcome chaos strategy to notify that an outcome 
was injected.
      OutcomeGenerator (class):  AddException, AddResult, OutcomeGenerator
         Generator that produces outcomes such as exceptions or results.
      OutcomeGeneratorArguments (struct):  OutcomeGeneratorArguments
         Arguments used by .
   Polly.Telemetry
      EnrichmentContext (struct):  EnrichmentContext
         Enrichment context used when reporting resilience events.
      ExecutionAttemptArguments (struct):  ExecutionAttemptArguments
         Arguments that encapsulate the execution attempt for retries or 
hedging.
      MeteringEnricher (class):  Enrich
         Enricher used to enrich the metrics with additional information.
      PipelineExecutedArguments (struct):  PipelineExecutedArguments
         Arguments that indicate the pipeline execution started.
      PipelineExecutingArguments (struct)
         Arguments that indicate the pipeline execution started.
      ResilienceEvent (struct):  ResilienceEvent, ToString
         Represents a resilience event that has been reported.
      ResilienceStrategyTelemetry (class):  Report, SetTelemetrySource
         Resilience telemetry is used by individual resilience strategies to 
report some important events.
      ResilienceTelemetrySource (class):  ResilienceTelemetrySource
         The source of resilience telemetry events.
      SeverityProviderArguments (struct):  SeverityProviderArguments
         Arguments used by .
      TelemetryEventArguments (struct):  TelemetryEventArguments
         Represents the information about the resilience event.
      TelemetryListener (class):  Write
         Listener of resilience telemetry events.
      TelemetryOptions (class):  TelemetryOptions
         The options that are used to configure the telemetry that is produced 
by the resilience strategies.
   Polly.Testing
      ResiliencePipelineDescriptor (class)
         Describes the resilience pipeline.
      ResiliencePipelineExtensions (class):  GetPipelineDescriptor
         The test-related extensions for and .
      ResilienceStrategyDescriptor (class)
         This class provides additional information about a .
   Polly.Tests
      StrongNameTests (class):  Tests_Are_Strong_Named
   Polly.Timeout
      AsyncTimeoutPolicy (class)
         A timeout policy which can be applied to async delegates.
      ITimeoutPolicy (interface)
         Defines properties and methods common to all Timeout policies.
      OnTimeoutArguments (struct):  OnTimeoutArguments
         Arguments used by the timeout strategy to notify that a timeout 
occurred.
      TimeoutGeneratorArguments (struct):  TimeoutGeneratorArguments
         Arguments used by the timeout strategy to retrieve a timeout for 
current execution.
      TimeoutPolicy (class)
         A timeout policy which can be applied to delegates.
      TimeoutRejectedException (class):  GetObjectData, TimeoutRejectedException
         Exception thrown when a delegate executed through a timeout resilience 
strategy does not complete, before the configu...
      TimeoutStrategyOptions (class):  TimeoutStrategyOptions
         Represents the options for the timeout strategy.
   Polly.Utilities
      ExceptionExtensions (class):  RethrowWithOriginalStackTraceIfDiffersFrom
         Contains extension methods on the class.
      SystemClock (class):  Reset
         Time related delegates used to support different compilation targets 
and to improve testability of the code.
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
   wire into DI  →  
PollyServiceCollectionExtensions.AddResiliencePipelineRegistry(...)
   wire into DI  →  PollyServiceCollectionExtensions.AddResiliencePipelines(...)
   build one  →  derive AsyncPolicy
   build one  →  derive Policy
   build one  →  derive ResilienceStrategy

PACKAGES
   Other:  Microsoft.Bcl.AsyncInterfaces, Microsoft.Bcl.TimeProvider, 
Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Options, 
System.ComponentModel.Annotations, System.Diagnostics.DiagnosticSource, 
System.Threading.RateLimiting, System.Threading.Tasks.Extensions … (9 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus 
PollyServiceCollectionExtensions)

analyzed 794 files · 389 nodes · 38 edges · 0 entries · ~6840 tokens · 9.1s 
stage2 ×2.1 stage3 ×1.7
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │      Polly.slnx      │
│   Time   │        9738ms        │
│  Tokens  │ ~6840 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.135 │
╰──────────┴──────────────────────╯
