## DevContext — Add Similar Feature on MinimalApiProject

**Architecture**: MinimalApi (100% confidence)
**Signals**: dapper · minimal-apis · mediatr · efcore
**Projects**: 3 — Infrastructure, Api, Core
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 5 in output

---
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

## Related types grouped by layer

- **Api**: Order, CreateOrderCommand, CreateOrderHandler
- **Infrastructure**: OrderRepository, IOrderRepository

---
*Generated in {elapsed}ms | 7 types (5 active, 2 pruned) | Compression: TrivialMemberCompressor(−12%) · StructuralDeduplicator(−13%) | Schema v2.0.0*