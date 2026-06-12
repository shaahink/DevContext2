## DevContext — Overview on CleanArch

**Architecture**: CleanArchitecture (100% confidence)
**Signals**: minimal-apis · mediatr · efcore
**Projects**: 4 — Web, Infrastructure, Domain, Application
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 4 in output

---
## Architecture overview

```text
└── Web
    ├── Application
    │   └── Domain
    └── Infrastructure
```

## Endpoints

**Web** (1 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| GET | /products | λ Program.cs:13 | - | Program.cs:13 |

## MediatR Handlers

| Kind | Request | Response | Handler |
|------|---------|----------|---------|
| Command | GetProductsQuery | List<Product> | GetProductsHandler |

## Data model (EF Core)

### `AppDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `Product` | — | Id |
| `Product` | ✓ | Id |

## Middleware pipeline

| Type | Kind | Count | Sources |
|------|------|-------|---------|
| MapGet | MapX | 1 | Program.cs |

## DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Extension | AddDbContext | AddDbContext → options =>... | Program.cs:8 |
| Extension | AddMediatR | AddMediatR → cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductsHandler).Assembly) | Program.cs:7 |

## Related types

- **Application**: GetProductsHandler, GetProductsQuery
- **Domain**: Product
- **Infrastructure**: AppDbContext

---
*Generated in {elapsed}ms | 4 types (4 active, 0 pruned) | Compression: TrivialMemberCompressor(−4%) | Schema v1.1*