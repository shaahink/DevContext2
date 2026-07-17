# REPORT
**GettingStarted**

Style: Unknown
_1 project  ·  1 MessageConsumer, 1 HostedService  ·  net5.0 + masstransit_

## Stats

| Metric | Value |
|--------|-------|
| Files | 3 |
| Projects | 1 |
| Nodes | 16 |
| Edges | 10 |
| Entries | 2 |
| With target | 2/2 |
| Deep spine (>=2) | 2/2 (100%) |
| Verified edges | 100% |
| Analyzed in | 1.8s |

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

_4 info_

### _INFO_: Entry targets resolved 2/2 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: Message flow: 0 producers, 1 consumers, 0 message types
*(Wiring)*

- 1 consumers

### _INFO_: Entry surface: 1 hosted · 1 Bus
*(Shape)*

- 1 hosted
- 1 Bus

### _INFO_: Event wiring: 2 events (2 integration), 0 cross-service, 0 orphan
*(Wiring)*

- <registration> → GettingStarted
- Message → GettingStarted

WORKER  GettingStarted     (1 project)

STACK  net5.0 · MassTransit

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         GettingStarted: Worker Service [Worker, MassTransit]

WORKER VIEW
   GettingStarted [Application] (1)
      Worker  → Worker

TOPOLOGY (depends-on)
   GettingStarted

EVENT WIRING  (2 integration events, 0 cross-service)
  <registration>: (external) · GettingStarted
  Message: GettingStarted · GettingStarted

ENTRY POINTS
   Bus (1)
      MessageConsumer  → MessageConsumer  (GettingStarted/MessageConsumer.cs:12)
   Background (1)
      Worker  → Worker  (GettingStarted/Program.cs:28)

PACKAGES
   Messaging:  MassTransit 8.0.0, MassTransit.RabbitMQ 8.0.0
   Other:  Microsoft.Extensions.Hosting 5.0.0

→ drill in:  --focus "<entry>"   (e.g. --focus "Worker")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 85ms |
| GenericExtraction | 170ms |
| SignalSealing | 0ms |
| SpecificExtraction | 969ms |
| Compression | 17ms |
| **Total** | **1755ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 960ms | 0 | 0 |
| ProgramCsFlowExtractor | 166ms | 4 | 3 |
| DiRegistrationExtractor | 165ms | 4 | 2 |
| SyntaxStructureExtractor | 164ms | 4 | 0 |
| SolutionDiscovery | 38ms | 0 | 0 |
| ProjectStructure | 31ms | 0 | 0 |
| LayerClassifier | 21ms | 0 | 0 |
| DependencyExtractor | 21ms | 0 | 0 |
| BodyFactsExtractor | 13ms | 0 | 0 |
| FileTreeExtractor | 12ms | 0 | 0 |
| InMemoryEventBusExtractor | 5ms | 0 | 4 |
| EventBusExtractor | 4ms | 0 | 4 |
| SourceBodyExtractor | 3ms | 0 | 0 |
| IndirectWiringDetector | 3ms | 0 | 1 |
| AntiPatternDetector | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 3 | 0 |
| Raises | 1 | 0 |
| Consumes | 4 | 0 |
| Resolves | 2 | 0 |

_3 files · 1 projects_
