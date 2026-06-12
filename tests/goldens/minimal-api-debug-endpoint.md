## DevContext — Trace on MinimalApiProject

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

## Call graph

Not available in current profile. Re-run with `--profile debug` to enable call graph extraction and BFS reachability analysis from entry points.

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

---
*Generated in {elapsed}ms | 7 types (5 active, 2 pruned) | Compression: TrivialMemberCompressor(−12%) · StructuralDeduplicator(−13%) | Schema v1.0*