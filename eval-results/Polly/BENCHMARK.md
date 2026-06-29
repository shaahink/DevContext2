# Polly — Library Surface BENCHMARK (hand-built target)

> Target DevContext output for Polly, hand-built from a source read at pinned SHA
> `7a1d10f47e2ec667ceada49deb6bdd9a765753bd`. The *Polly instance* of the canonical Library Benchmark
> Format. Build-free constraint applies (syntax + `///` only; inherited-from-external members not enumerated).
>
> Shape exercised: **fluent builder + strategy options + DI extension** (modern-lib shape #2), plus the
> "library with sample apps" trap that the App archetype must not fall into.

```
LIBRARY  Polly   (resilience & transient-fault library · ~188 public types)
         TFMs: multi-targeted (netstandard2.0 + net8.0)            archetype: Library

ENTRY API  (ranked, /// summary + file)
   register  PollyServiceCollectionExtensions.AddResiliencePipeline(this IServiceCollection)
                "Adds a resilience pipeline … to the service collection."
   build     ResiliencePipelineBuilder        new ResiliencePipelineBuilder().AddRetry(..).AddTimeout(..).Build()   [v8 fluent builder]
   extend    *ResiliencePipelineBuilderExtensions   AddRetry · AddTimeout · AddCircuitBreaker · AddHedging · AddFallback   (this ResiliencePipelineBuilder)
   derive    ResilienceStrategyOptions / ResilienceStrategy    custom-strategy seats

ABSTRACTIONS / SEATS
   Policy / AsyncPolicy           v7 policy seats (24 implementors each)
   ResilienceStrategy             v8 strategy seat
   ResilienceStrategyOptions      options seat
   IsPolicy · ITtlStrategy · IAsyncCacheProvider · ISyncCacheProvider    pluggable contracts

PUBLIC SURFACE  (by namespace · docs · benchmarks/samples excluded)
   Polly                          ResiliencePipeline(Builder), Policy, PolicyBuilder, Outcome, Context, *Syntax (v7 fluent), *ResiliencePipelineBuilderExtensions
   Polly.Retry / .CircuitBreaker / .Timeout / .Fallback / .RateLimiting   strategy options + policies + arguments
   Polly.Registry                 ResiliencePipelineRegistry, IPolicyRegistry
   Polly.Simmy(.*)                chaos strategies (fault/latency/outcome/behavior)
   Polly.Telemetry                ResilienceStrategyTelemetry, TelemetryOptions

CONSUMER PATHS
   "build a pipeline"  → new ResiliencePipelineBuilder().AddRetry(options).Build()
   "wire into DI"      → services.AddResiliencePipeline(key, builder => builder.AddRetry(...))
   "custom strategy"   → derive ResilienceStrategyOptions / ResilienceStrategy

PACKAGES  (runtime only — NO test/sample deps: Refit, RestSharp, Flurl, xunit, NSubstitute excluded)
   Microsoft.Bcl.AsyncInterfaces, Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Options,
   System.Threading.RateLimiting, System.Diagnostics.DiagnosticSource, …
```

## What makes this the target

- **Library, not App.** Polly ships a `samples/Chaos` Minimal-API demo + sample/benchmark exe projects; the
  archetype must ignore those (a library's samples are not the library) and render the surface, not an app Map.
- **The v8 builder front door** (`ResiliencePipelineBuilder` + `AddRetry`/`AddTimeout` extensions) and the DI
  registration (`AddResiliencePipeline`) lead the ENTRY API.
- **Benchmarks/samples excluded** from PUBLIC SURFACE and PACKAGES — only real Polly library API remains.

## Gates (`eval/expectations/polly.json`)
`archetype=Library` · `ENTRY API`/`ABSTRACTIONS`/`CONSUMER PATHS` present · `ResiliencePipelineBuilder` +
`AddResiliencePipeline` surfaced · no sample packages (`Refit`/`RestSharp`) · no benchmark types
(`Polly.Benchmarks`). All `expected` (the engine produces them after the It2 sample/benchmark-exclusion fix).
