# REPORT
**Aggregator**

Style: SampleCollection
_2 projects  ·  6 GrpcService  ·  net10.0 + minimal-apis + grpc_

## Stats

| Metric | Value |
|--------|-------|
| Files | 6 |
| Projects | 2 |
| Nodes | 17 |
| Edges | 10 |
| Entries | 6 |
| With target | 6/6 |
| Deep spine (>=2) | 6/6 (100%) |
| Verified edges | 80% |
| Analyzed in | 2.2s |

## Top Flows

1. **Counter.AccumulateCount** → `IncrementingCounter` *(GrpcService)*
2. **Counter.IncrementCount** → `IncrementingCounter` *(GrpcService)*
3. **Aggregator.AccumulateCount** → `AggregatorService` *(GrpcService)*
4. **Aggregator.SayHellos** → `AggregatorService` *(GrpcService)*
5. **Greeter.SayHello** → `GreeterService` *(GrpcService)*
6. **Greeter.SayHellos** → `GreeterService` *(GrpcService)*

### Trace 1: Counter.AccumulateCount

TRACE  Counter.AccumulateCount
       Server/Services/CounterService.cs:27
       Server
▸ ENTRY  Counter.AccumulateCount  (Server/Services/CounterService.cs:27)
   └─ call CounterService.AccumulateCount  (Server/Services/CounterService.cs:27)
          public override async Task<CounterReply> AccumulateCount(IAsyncStreamReader<CounterRequest> requestStream, ServerCallContext context)
          await foreach (var message in requestStream.ReadAllAsync())
          _logger.LogInformation($"Incrementing count by {message.Count}");
      ├─ call IncrementingCounter  (Server/Services/CounterService.cs:52) [approx]
      │      public class IncrementingCounter
      │      public void Increment(int amount)
      │      Count += amount;
      └─ call IncrementingCounter.Increment  (Server/Services/CounterService.cs:52) [verified]
             public void Increment(int amount)
             Count += amount;

---

### Trace 2: Counter.IncrementCount

TRACE  Counter.IncrementCount
       Server/Services/CounterService.cs:27
       Server
▸ ENTRY  Counter.IncrementCount  (Server/Services/CounterService.cs:27)
   └─ call CounterService.IncrementCount  (Server/Services/CounterService.cs:27)
          public override Task<CounterReply> IncrementCount(Empty request, ServerCallContext context)
          _logger.LogInformation("Incrementing count by 1");
          _counter.Increment(1);
      ├─ call IncrementingCounter  (Server/Services/CounterService.cs:41) [approx]
      │      public class IncrementingCounter
      │      public void Increment(int amount)
      │      Count += amount;
      └─ call IncrementingCounter.Increment  (Server/Services/CounterService.cs:41) [verified]
             public void Increment(int amount)
             Count += amount;

---

### Trace 3: Aggregator.AccumulateCount

TRACE  Aggregator.AccumulateCount
       Server/Services/AggregatorService.cs:27
       Server
▸ ENTRY  Aggregator.AccumulateCount  (Server/Services/AggregatorService.cs:27)
   └─ call AggregatorService.AccumulateCount  (Server/Services/AggregatorService.cs:27)
          public override async Task<CounterReply> AccumulateCount(IAsyncStreamReader<CounterRequest> requestStream, ServerCallContext context)
          // Forward the call on to the counter service
          using (var call = _counterClient.AccumulateCount())

---

## Insights

_2 info_

### _INFO_: Entry targets resolved 6/6 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 4 Extension · 1 Singleton (5 total)
*(Wiring)*

MAP  Aggregator     (2 projects)

STACK  net10.0 · Minimal APIs

STYLE  SampleCollection  (confidence high)
       evidence: 2/2 projects are samples/demos/docs — no unified architecture

       per service:
         Client: Unknown
         Server: Unknown

TOPOLOGY (depends-on)
   Client
   Server

ENTRY POINTS
   gRPC (6)
      Aggregator.AccumulateCount  → AggregatorService  (Server/Services/AggregatorService.cs:27)
      Aggregator.SayHellos  → AggregatorService  (Server/Services/AggregatorService.cs:27)
      Counter.AccumulateCount  → IncrementingCounter  (Server/Services/CounterService.cs:27)
      Counter.IncrementCount  → IncrementingCounter  (Server/Services/CounterService.cs:27)
      Greeter.SayHello  → GreeterService  (Server/Services/GreeterService.cs:26)
      Greeter.SayHellos  → GreeterService  (Server/Services/GreeterService.cs:26)

PACKAGES
   Web/API:  Grpc.AspNetCore, OpenTelemetry.Instrumentation.AspNetCore
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.Http
   Other:  Google.Protobuf 3.31.1, Grpc.Net.Client, Grpc.Tools 2.80.0

→ drill in:  --focus "<entry>"   (e.g. --focus "Counter.IncrementCount")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 73ms |
| GenericExtraction | 191ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1319ms |
| Compression | 15ms |
| **Total** | **2174ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1290ms | 0 | 0 |
| SyntaxStructureExtractor | 187ms | 4 | 8 |
| ProgramCsFlowExtractor | 186ms | 0 | 8 |
| DiRegistrationExtractor | 183ms | 0 | 5 |
| ProjectStructure | 30ms | 0 | 0 |
| DependencyExtractor | 26ms | 0 | 0 |
| LayerClassifier | 26ms | 0 | 0 |
| SolutionDiscovery | 25ms | 0 | 0 |
| EndpointExtractor | 23ms | 0 | 5 |
| BodyFactsExtractor | 18ms | 0 | 0 |
| FileTreeExtractor | 12ms | 0 | 0 |
| GrpcServiceExtractor | 8ms | 0 | 5 |
| InMemoryEventBusExtractor | 7ms | 0 | 2 |
| SourceBodyExtractor | 6ms | 0 | 0 |
| GrpcClientExtractor | 6ms | 0 | 2 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 10 | 2 |

_6 files · 2 projects_
