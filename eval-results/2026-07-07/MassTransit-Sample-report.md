# REPORT
**GettingStarted**

Style: Unknown
_1 project  ·  1 MessageConsumer, 1 HostedService  ·  net5.0 + masstransit_

## Stats

| Metric | Value |
|--------|-------|
| Files | 3 |
| Projects | 1 |
| Nodes | 14 |
| Edges | 9 |
| Entries | 2 |
| With target | 2/2 |
| Verified edges | 100% |
| Analyzed in | 1.2s |

## Top Flows

1. **MessageConsumer** → `MessageConsumer` *(MessageConsumer)*
2. **Worker** → `Worker` *(HostedService)*

### Trace 1: MessageConsumer

TRACE  MessageConsumer
       GettingStarted/MessageConsumer.cs:12

▸ ENTRY  MessageConsumer  (GettingStarted/MessageConsumer.cs:12)
   └─ call MessageConsumer  (GettingStarted/MessageConsumer.cs:12)
          public class MessageConsumer :
          IConsumer<Message>
          readonly ILogger<MessageConsumer> _logger;

---

### Trace 2: Worker

TRACE  Worker
       GettingStarted/Program.cs:28

▸ ENTRY  Worker  (GettingStarted/Program.cs:28)
   └─ call Worker  (GettingStarted/Program.cs:28)
          public class Worker : BackgroundService
          readonly IBus _bus;
          public Worker(IBus bus)

---

## Insights

_4 info_

### _INFO_: Entry targets resolved 2/2 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Public surface: 0 interfaces, 4 classes (4 total public types)
*(Shape)*

- 0 interfaces
- 4 classes

### _INFO_: Message flow: 0 producers, 1 consumers, 0 message types
*(Wiring)*

- 1 consumers

### _INFO_: Entry surface: 1 hosted · 1 Bus
*(Shape)*

- 1 hosted
- 1 Bus

MAP  GettingStarted     (1 project)

STACK  net5.0 · MassTransit

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

TOPOLOGY (depends-on)
   GettingStarted

ENTRY POINTS
   Bus (1)
      MessageConsumer  → MessageConsumer  (GettingStarted/MessageConsumer.cs:12)
   Background (1)
      Worker  → Worker  (GettingStarted/Program.cs:28)

PACKAGES
   Messaging:  MassTransit 8.0.0, MassTransit.RabbitMQ 8.0.0
   Other:  Microsoft.Extensions.Hosting 5.0.0

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 62ms |
| GenericExtraction | 128ms |
| SignalSealing | 0ms |
| SpecificExtraction | 722ms |
| Compression | 12ms |
| **Total** | **1185ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 712ms | 0 | 0 |
| ProgramCsFlowExtractor | 125ms | 4 | 3 |
| DiRegistrationExtractor | 124ms | 4 | 2 |
| SyntaxStructureExtractor | 124ms | 4 | 0 |
| SolutionDiscovery | 27ms | 0 | 0 |
| ProjectStructure | 23ms | 0 | 0 |
| DependencyExtractor | 13ms | 0 | 0 |
| LayerClassifier | 13ms | 0 | 0 |
| FileTreeExtractor | 9ms | 0 | 0 |
| InMemoryEventBusExtractor | 5ms | 0 | 4 |
| IndirectWiringDetector | 4ms | 0 | 4 |
| EventBusExtractor | 4ms | 0 | 4 |
| SourceBodyExtractor | 3ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 3 | 0 |
| Consumes | 4 | 0 |
| Resolves | 2 | 0 |

_3 files · 1 projects_
