## DevContext -- Add Similar Feature on CleanArch

**Architecture**: CleanArchitecture (100% confidence)
**Signals**: minimal-apis · mediatr · efcore
**Projects**: 4 -- Web, Infrastructure, Domain, Application
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 4 in output

---
## Endpoints

**Web** (1 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| GET | /products | λ Program.cs:13 | - | Program.cs:13 |

## MediatR Handlers

| Kind | Request | Response | Handler |
|------|---------|----------|---------|
| Command | GetProductsQuery | List<Product> | GetProductsHandler |

## Related types grouped by layer

- **Application**: GetProductsHandler, GetProductsQuery
- **Domain**: Product
- **Infrastructure**: AppDbContext

---
*Generated in {elapsed}ms | 4 types (4 active, 0 pruned) | Compression: TrivialMemberCompressor(−4%) | Schema v2.0*