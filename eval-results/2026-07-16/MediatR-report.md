# REPORT
**MediatR**

Style: SampleCollection
_12 projects  ·  1 DomainEventHandler  ·  net10.0, net6.0 + mediatr_

## Stats

| Metric | Value |
|--------|-------|
| Files | 152 |
| Projects | 15 |
| Nodes | 430 |
| Edges | 354 |
| Entries | 1 |
| With target | 1/1 |
| Deep spine (>=2) | 1/1 (100%) |
| Verified edges | 87% |
| Analyzed in | 6.7s |

## Top Flows

1. **NotificationHandler** → `NotificationHandler` *(DomainEventHandler)*

### Trace 1: NotificationHandler

TRACE  NotificationHandler
       src/MediatR/INotificationHandler.cs:25
       MediatR
▸ ENTRY  NotificationHandler  (src/MediatR/INotificationHandler.cs:25)
   └─ call NotificationHandler  (src/MediatR/INotificationHandler.cs:25)
          /// <summary>
          /// Wrapper class for a synchronous notification handler
          /// </summary>

---

## Insights

_2 info · 3 notable_

### **NOTABLE**: Most depended-upon: MediatR (5 dependents)
*(Topology)*

- MediatR (5 dependents)

### **NOTABLE**: Multi-implementation interfaces: IStreamPipelineBehavior<StreamPing, Pong> (2 impls)
*(Wiring)*

- IStreamPipelineBehavior<StreamPing, Pong> (2 impls)

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- IRequestExceptionHandler
- TaskWhenAllPublisher
- ConnectionExceptionHandler
- Publisher
- Program

### _INFO_: Entry targets resolved 1/1 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 103 Extension · 40 Singleton · 16 Transient · 5 Scoped (164 total)
*(Wiring)*

MAP  MediatR     (12 projects)

STACK  net10.0, net6.0, netstandard2.0, netstandard2.0;net8.0;net9.0;net10.0 · MediatR (CQRS)

STYLE  SampleCollection  (confidence moderate)
       evidence: 10/13 projects are samples/demos/docs — no unified architecture

       per service:
         MediatR.Examples.AspNetCore: Unknown
         MediatR.Examples.Autofac: Unknown
         MediatR.Examples.DryIoc: Unknown
         MediatR.Examples.Lamar: Unknown
         MediatR.Examples.LightInject: Unknown
         MediatR.Examples.PublishStrategies: Unknown
         MediatR.Examples.SimpleInjector: Unknown
         MediatR.Examples.Stashbox: Unknown
         MediatR.Examples.Windsor: Unknown
         MediatR.Benchmarks: Unknown

TOPOLOGY (depends-on)
   MediatR.Examples ── MediatR
   MediatR
   MediatR.Benchmarks ── MediatR
   MediatR.Contracts
   MediatR.Examples.AspNetCore ── MediatR.Examples
   MediatR.Examples.Autofac ── MediatR.Examples
   MediatR.Examples.DryIoc ── MediatR.Examples
   MediatR.Examples.Lamar ── MediatR.Examples
   MediatR.Examples.LightInject ── MediatR.Examples
   MediatR.Examples.PublishStrategies ── MediatR, MediatR.Examples
   MediatR.Examples.SimpleInjector ── MediatR.Examples
   MediatR.Examples.Stashbox ── MediatR.Examples

ENTRY POINTS
   Domain (1)
      NotificationHandler  → NotificationHandler  (src/MediatR/INotificationHandler.cs:25)

CROSS-CUTTING
   MediatR pipeline (every command):  ConstrainedBehavior → ConstructorTestBehavior → GenericPipelineBehavior → InnerBehavior → ListResponseBehavior → Open2Behavior → OuterBehavior → TimeoutBehavior

PACKAGES
   Mediator/CQRS:  MediatR.Contracts [2.0.1, 3.0.0)
   Testing:  Shouldly 4.2.1, xunit 2.6.1, xunit.runner.visualstudio 2.5.3
   Utilities:  Scrutor 6.1.0
   Other:  Autofac 8.4.0, Autofac.Extensions.DependencyInjection 10.0.0, BenchmarkDotNet 0.13.10, BenchmarkDotNet.Diagnostics.Windows 0.13.10, Castle.Windsor 5.1.2, Castle.Windsor.Extensions.DependencyInjection 5.1.2, DryIoc.Microsoft.DependencyInjection 8.0.0-preview-04, IsExternalInit 1.0.3 … (26 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "NotificationHandler")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 314ms |
| GenericExtraction | 1163ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1871ms |
| Compression | 40ms |
| **Total** | **6690ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1844ms | 0 | 0 |
| SyntaxStructureExtractor | 1160ms | 348 | 164 |
| ProgramCsFlowExtractor | 891ms | 0 | 164 |
| DiRegistrationExtractor | 888ms | 0 | 164 |
| ProjectStructure | 231ms | 0 | 0 |
| SourceBodyExtractor | 153ms | 0 | 0 |
| BodyFactsExtractor | 115ms | 0 | 0 |
| SolutionDiscovery | 40ms | 0 | 0 |
| FileTreeExtractor | 39ms | 0 | 0 |
| MediatRExtractor | 25ms | 0 | 183 |
| InMemoryEventBusExtractor | 24ms | 0 | 164 |
| DependencyExtractor | 21ms | 0 | 0 |
| LayerClassifier | 17ms | 0 | 0 |
| IndirectWiringDetector | 12ms | 0 | 102 |
| AntiPatternDetector | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 231 | 35 |
| Sends | 85 | 3 |
| Handles | 1 | 0 |
| Consumes | 1 | 0 |
| Resolves | 36 | 7 |

_152 files · 15 projects_
