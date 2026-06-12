## DevContext — Overview on MinimalApiProject

**Architecture**: MinimalApi (100% confidence)
**Signals**: dapper · minimal-apis · mediatr · efcore
**Projects**: 3 — Infrastructure, Api, Core
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 7 in output

---
## Architecture overview

```text
└── Api
    └── Core
└── Infrastructure
```

## Endpoints

**Api** (2 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| POST | /orders | λ Program.cs:15 | - | Program.cs:15 |
| GET | /orders | λ Program.cs:9 | - | Program.cs:9 |

## MediatR Handlers

| Kind | Request | Response | Handler |
|------|---------|----------|---------|
| Command | CreateOrderCommand | int | CreateOrderHandler |

## Middleware pipeline

| Type | Kind | Count | Sources |
|------|------|-------|---------|
| MapPost | MapX | 1 | Program.cs |
| MapGet | MapX | 1 | Program.cs |

## DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Extension | AddMediatR | AddMediatR → cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly) | Program.cs:5 |

## Related types

- **Api**: Order, CreateOrderCommand, CreateOrderHandler, GetOrdersQuery
- **Infrastructure**: OrderRepository, IOrderRepository
- **Unknown**: Order

---
*Generated in {elapsed}ms | 7 types (7 active, 0 pruned) | Compression: TrivialMemberCompressor(−12%) · StructuralDeduplicator(−13%) | Schema v1.0*