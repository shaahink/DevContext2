# REPORT
**GettingStarted**

Style: Unknown
_1 project  ·  1 MessageConsumer, 1 HostedService  ·  net5.0 + masstransit_

## Stats

| Metric | Value |
|--------|-------|
| Files | 3 |
| Projects | 1 |
| Nodes | 15 |
| Edges | 10 |
| Entries | 2 |
| With target | 2/2 |
| Verified edges | 100% |
| Analyzed in | 1.7s |

## Top Flows

1. **MessageConsumer** → `MessageConsumer` *(MessageConsumer)*
2. **Worker** → `Worker` *(HostedService)*

### Trace 1: MessageConsumer

TRACE  MessageConsumer
       GettingStarted/MessageConsumer.cs:12
       GettingStarted
▸ ENTRY  MessageConsumer  (GettingStarted/MessageConsumer.cs:12)
   └─ call MessageConsumer  (GettingStarted/MessageConsumer.cs:12)
          public class MessageConsumer :
          IConsumer<Message>
          readonly ILogger<MessageConsumer> _logger;

---

### Trace 2: Worker

TRACE  Worker
       GettingStarted/Program.cs:28
       GettingStarted
▸ ENTRY  Worker  (GettingStarted/Program.cs:28)
   └─ call Worker  (GettingStarted/Program.cs:28)
          public class Worker : BackgroundService
          readonly IBus _bus;
          public Worker(IBus bus)
      └─ raises Message  (GettingStarted/Worker.cs:22) [verified]
             public class Message
             public string Text { get; set; }
         └─ consumes MessageConsumer  (GettingStarted/MessageConsumer.cs:12)
                public class MessageConsumer :
                IConsumer<Message>
                readonly ILogger<MessageConsumer> _logger;
EMITS    Message

---

## Insights

_5 info_

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

### _INFO_: Event flow: 1 published, 2 consumed, 0 orphan
*(Wiring)*

- 2/1 events consumed

WORKER  GettingStarted     (1 project)

STACK  net5.0 · MassTransit

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

WORKER VIEW
   GettingStarted [Application] (1)
      Worker

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
| DiscoveryAndCacheWarmup | 82ms |
| GenericExtraction | 178ms |
| SignalSealing | 0ms |
| SpecificExtraction | 841ms |
| Compression | 17ms |
| **Total** | **1660ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 831ms | 0 | 0 |
| ProgramCsFlowExtractor | 175ms | 4 | 3 |
| SyntaxStructureExtractor | 174ms | 4 | 0 |
| DiRegistrationExtractor | 174ms | 4 | 2 |
| SolutionDiscovery | 33ms | 0 | 0 |
| ProjectStructure | 31ms | 0 | 0 |
| DependencyExtractor | 21ms | 0 | 0 |
| LayerClassifier | 21ms | 0 | 0 |
| FileTreeExtractor | 14ms | 0 | 0 |
| BodyFactsExtractor | 12ms | 0 | 0 |
| EventBusExtractor | 6ms | 0 | 4 |
| InMemoryEventBusExtractor | 5ms | 0 | 1 |
| SourceBodyExtractor | 4ms | 0 | 0 |
| IndirectWiringDetector | 3ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 3 | 0 |
| Raises | 1 | 0 |
| Consumes | 4 | 0 |
| Resolves | 2 | 0 |

_3 files · 1 projects_
