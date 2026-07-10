# REPORT
**MinimalApiProject**

Style: CleanArchitecture
_3 projects  ·  2 HttpEndpoint  ·  net10.0 + minimal-apis_

## Stats

| Metric | Value |
|--------|-------|
| Files | 6 |
| Projects | 3 |
| Nodes | 16 |
| Edges | 8 |
| Entries | 2 |
| With target | 2/2 |
| Verified edges | 62% |
| Analyzed in | 1.6s |

## Top Flows

1. **POST /orders** → `CreateOrderCommand` *(HttpEndpoint)*
2. **GET /orders** → `GetOrdersQuery` *(HttpEndpoint)*

### Trace 1: POST /orders

TRACE  POST /orders
       src/Api/Program.cs:15
       Api
▸ ENTRY  POST /orders  (src/Api/Program.cs:15)
   └─ call <lambda> POST /orders  (src/Api/Program.cs:15)
      └─ send CreateOrderCommand  (src/Api/Program.cs:3) [approx]
             public record CreateOrderCommand(string ProductName, int Quantity) : IRequest<int>;
         └─ handler CreateOrderHandler  (src/Api/Handlers/CreateOrderHandler.cs:6)
                public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
                private readonly IOrderRepository _repository;
                public CreateOrderHandler(IOrderRepository repository)
            └─ call OrderRepository.AddAsync  (src/Api/Handlers/CreateOrderHandler.cs:24) [verified]
                   public Task AddAsync(Order order)
                   => Task.CompletedTask;
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: GET /orders

TRACE  GET /orders
       src/Api/Program.cs:9
       Api
▸ ENTRY  GET /orders  (src/Api/Program.cs:9)
   └─ call <lambda> GET /orders  (src/Api/Program.cs:9)
      └─ send GetOrdersQuery  (src/Api/Program.cs:3) [approx]
             public record GetOrdersQuery() : IRequest<List<Order>>;
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_4 info · 2 warning_

### **WARNING**: 2/2 endpoints anonymous, incl. 1 POST/PUT/DELETE
*(Risk)*

- POST /orders
- GET /orders

### **WARNING**: Auth surface: 0 protected, 2 unannotated of 2 API endpoints
*(Risk)*

- 2 no auth annotation

### _INFO_: Entry targets resolved 2/2 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Routing surface: 2 routes exposed
*(Shape)*

- POST /orders
- GET /orders

### _INFO_: Public surface: 1 interfaces, 4 classes (7 total public types)
*(Shape)*

- 1 interfaces
- 4 classes

### _INFO_: Most depended-upon: Core (2 dependents)
*(Topology)*

- Core (2 dependents)

MAP  MinimalApiProject     (3 projects)

STACK  net10.0 · Minimal APIs · MediatR (CQRS)

STYLE  CleanArchitecture  (confidence moderate)
       evidence: DDD folder layers: Infrastructure, Api, Core; MediatR with 1 handlers

       per service:
         Api: CQRS [MediatR]

TOPOLOGY (depends-on)
   Core
   Api ── Core
   Infrastructure ── Core

ENTRY POINTS
   HTTP (2)
      GET /orders  → GetOrdersQuery  (src/Api/Program.cs:9)
      POST /orders  → CreateOrderCommand  (src/Api/Program.cs:15)

PACKAGES
   Web/API:  Microsoft.AspNetCore.OpenApi 10.0.0
   ORM/Data:  Dapper 2.1.35, Microsoft.EntityFrameworkCore 10.0.0-preview.3.25171.5
   Mediator/CQRS:  MediatR 12.0.0

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 42ms |
| GenericExtraction | 175ms |
| SignalSealing | 0ms |
| SpecificExtraction | 957ms |
| Compression | 24ms |
| **Total** | **1640ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 936ms | 0 | 0 |
| ProgramCsFlowExtractor | 171ms | 7 | 3 |
| DiRegistrationExtractor | 166ms | 7 | 1 |
| SyntaxStructureExtractor | 162ms | 7 | 0 |
| ProjectStructure | 19ms | 0 | 0 |
| DependencyExtractor | 19ms | 0 | 0 |
| LayerClassifier | 18ms | 0 | 0 |
| EndpointExtractor | 16ms | 0 | 3 |
| BodyFactsExtractor | 15ms | 0 | 0 |
| SolutionDiscovery | 12ms | 0 | 0 |
| FileTreeExtractor | 7ms | 0 | 0 |
| MediatRExtractor | 5ms | 0 | 1 |
| InMemoryEventBusExtractor | 5ms | 0 | 1 |
| SourceBodyExtractor | 4ms | 0 | 0 |
| IndirectWiringDetector | 4ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 3 | 0 |
| Sends | 2 | 2 |
| Handles | 1 | 0 |
| Resolves | 2 | 1 |

_6 files · 3 projects_
