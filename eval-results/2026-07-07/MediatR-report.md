# REPORT
**MediatR**

Style: CleanArchitecture
_12 projects  ·  1 DomainEventHandler  ·  net10.0, net6.0 + mediatr_

## Stats

| Metric | Value |
|--------|-------|
| Files | 152 |
| Projects | 15 |
| Nodes | 240 |
| Edges | 114 |
| Entries | 1 |
| With target | 1/1 |
| Verified edges | 82% |
| Analyzed in | 3.5s |

## Top Flows

1. **NotificationHandler** → `NotificationHandler` *(DomainEventHandler)*

### Trace 1: NotificationHandler

TRACE  NotificationHandler
       src/MediatR/INotificationHandler.cs:25

▸ ENTRY  NotificationHandler  (src/MediatR/INotificationHandler.cs:25)
   └─ call NotificationHandler  (src/MediatR/INotificationHandler.cs:25)
          /// <summary>
          /// Wrapper class for a synchronous notification handler
          /// </summary>

---

## Insights

_2 info · 5 notable_

### **NOTABLE**: Internal hubs: 4 heavily-referenced internal types
*(Topology)*

- StreamRequestHandlerWrapperImpl (2 refs)
- StreamRequestHandlerWrapper (1 refs)
- StreamRequestHandlerBase (1 refs)
- Handler (1 refs)

### **NOTABLE**: Extension seats: AddMediatR (38 impls) · IStreamPipelineBehavior<StreamPing, Pong> (2 impls) · typeof(IPipelineBehavior<,>) (2 impls)
*(Wiring)*

- AddMediatR (38 impls)
- IStreamPipelineBehavior<StreamPing, Pong> (2 impls)
- typeof(IPipelineBehavior<,>) (2 impls)

### **NOTABLE**: Most depended-upon: MediatR.Examples (9 dependents) · MediatR (5 dependents)
*(Topology)*

- MediatR.Examples (9 dependents)
- MediatR (5 dependents)

### **NOTABLE**: Event flow: 4 published, 1 consumed, 4 orphan
*(Wiring)*

- 4 orphan events (published, no internal consumer)
- MediatR.Tests.Pinged ← Program.Main
- MediatR.Benchmarks.Pinged ← Benchmarks
- MediatR.Examples.Pinged ← Runner.Run
- 1/4 events consumed

### **NOTABLE**: Multi-implementation interfaces: IStreamPipelineBehavior<StreamPing, Pong> (2 impls)
*(Wiring)*

- IStreamPipelineBehavior<StreamPing, Pong> (2 impls)

### _INFO_: Entry targets resolved 1/1 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Public surface: 23 interfaces, 291 classes (325 total public types)
*(Shape)*

- 23 interfaces
- 291 classes

MAP  MediatR     (12 projects)

STACK  net10.0, net6.0, netstandard2.0, netstandard2.0;net8.0;net9.0;net10.0 · MediatR (CQRS)

STYLE  CleanArchitecture  (confidence moderate)
       evidence: 22 domain-event handlers; MediatR with 65 handlers

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

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 281ms |
| GenericExtraction | 1022ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1462ms |
| Compression | 25ms |
| **Total** | **3509ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1433ms | 0 | 0 |
| SyntaxStructureExtractor | 1019ms | 348 | 164 |
| DiRegistrationExtractor | 774ms | 0 | 164 |
| ProgramCsFlowExtractor | 278ms | 0 | 0 |
| ProjectStructure | 212ms | 0 | 0 |
| SourceBodyExtractor | 75ms | 0 | 0 |
| FileTreeExtractor | 34ms | 0 | 0 |
| SolutionDiscovery | 32ms | 0 | 0 |
| MediatRExtractor | 25ms | 0 | 183 |
| InMemoryEventBusExtractor | 24ms | 0 | 164 |
| DependencyExtractor | 16ms | 0 | 0 |
| LayerClassifier | 14ms | 0 | 0 |
| IndirectWiringDetector | 14ms | 0 | 103 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 31 | 5 |
| Sends | 5 | 5 |
| Handles | 1 | 0 |
| Raises | 4 | 4 |
| Consumes | 1 | 0 |
| Resolves | 72 | 7 |

_152 files · 15 projects_
