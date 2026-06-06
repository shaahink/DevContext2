## DevContext -- Architecture Overview on project

**Architecture**: MinimalApi (100% confidence)
**Signals**: minimal-apis · efcore
**Projects**: 7 -- AspireShop.AppHost, AspireShop.BasketService, AspireShop.CatalogDb, AspireShop.CatalogDbManager, AspireShop.CatalogService, AspireShop.Frontend, AspireShop.ServiceDefaults
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 16 in output

---
## Architecture overview

- AspireShop.AppHost
- AspireShop.BasketService
- AspireShop.CatalogDb
- AspireShop.CatalogDbManager
- AspireShop.CatalogService
- AspireShop.Frontend
- AspireShop.ServiceDefaults

## Endpoints

**AspireShop.CatalogDbManager** (1 endpoints)
| Method | Route | Group | Handler | Auth | Source |
|--------|-------|-------|---------|------|--------|
| POST | /reset-db | - | λ Program.cs:24 | - | Program.cs:24 |

**AspireShop.CatalogService** (3 endpoints)
| Method | Route | Group | Handler | Auth | Source |
|--------|-------|-------|---------|------|--------|
| GET | /api/v1/catalog/items/{catalogItemId:int}/image | /api/v1/catalog | λ CatalogApi.cs:47 | - | CatalogApi.cs:47 |
| GET | /api/v1/catalog/items/type/all/brand/{catalogBrandId:int} | /api/v1/catalog | λ CatalogApi.cs:19 | - | CatalogApi.cs:19 |
| GET | /api/v1/catalog/items/type/all | /api/v1/catalog | λ CatalogApi.cs:14 | - | CatalogApi.cs:14 |

## Data model (EF Core)

### `CatalogDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<OnModelCreating>` | — | — |
| `CatalogBrand` | ✓ | Id |
| `CatalogItem` | ✓ | Id |
| `CatalogType` | ✓ | Id |

### `Initial`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

## Non-obvious wiring

### Background workers

- sp => sp.GetRequiredService<CatalogDbInitializer>() (HostedService)

### Middleware pipeline

| Type | Kind | Count | Sources |
|------|------|-------|---------|
| UseStaticFiles | UseX | 1 | Program.cs |
| UseAntiforgery | UseX | 1 | Program.cs |
| UseHttpsRedirection | UseX | 1 | Program.cs |
| MapPost | MapX | 1 | Program.cs |
| MapGrpcService | MapX | 1 | Program.cs |
| UseExceptionHandler | UseX | 2 | Program.cs |

### DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Extension | AddHealthChecks | (AddHealthChecks) | Extensions.cs:86 |
| Extension | AddCheck | "self" | Extensions.cs:86 |
| Extension | AddOpenTelemetry | (AddOpenTelemetry) | Extensions.cs:78 |
| Extension | AddOpenTelemetry | (AddOpenTelemetry) | Extensions.cs:45 |
| Extension | AddServiceDiscovery | (AddServiceDiscovery) | Extensions.cs:23 |
| Extension | AddRazorComponents | (AddRazorComponents) | Program.cs:19 |
| Singleton | BasketServiceClient | BasketServiceClient | Program.cs:16 |
| Extension | AddGrpcServiceReference | $"{(isHttps ? "https" : "http")}://basketservice" | Program.cs:16 |
| Extension | AddHttpServiceReference | "https+http://catalogservice" | Program.cs:12 |
| Extension | AddHttpForwarderWithServiceDiscovery | (AddHttpForwarderWithServiceDiscovery) | Program.cs:10 |
| Extension | AddOpenApi | (AddOpenApi) | Program.cs:12 |
| Extension | AddProblemDetails | (AddProblemDetails) | Program.cs:11 |
| Extension | AddEndpointsApiExplorer | (AddEndpointsApiExplorer) | Program.cs:10 |
| Extension | AddHealthChecks | (AddHealthChecks) | Program.cs:17 |
| Extension | AddCheck | "DbInitializer" | Program.cs:17 |
| Extension | AddHostedService | sp => sp.GetRequiredService<CatalogDbInitializer>() | Program.cs:16 |
| Singleton | CatalogDbInitializer | CatalogDbInitializer | Program.cs:15 |
| Extension | AddOpenTelemetry | (AddOpenTelemetry) | Program.cs:12 |
| Transient | IBasketRepository | RedisBasketRepository | Program.cs:11 |
| Extension | AddGrpcHealthChecks | (AddGrpcHealthChecks) | Program.cs:10 |
| Extension | AddGrpc | (AddGrpc) | Program.cs:9 |

## Related types grouped by layer

- **Domain**: CustomerBasket, Order, BasketItem
- **Unknown**: RedisBasketRepository, CatalogDbContextModelSnapshot, Extensions, Initial, CatalogServiceClient, BasketService, GrpcServiceHealthCheck, CatalogBrand, CatalogDbContext, BasketServiceClient, CatalogApi, CatalogItem, IBasketRepository

---
*Generated in 22.0ms | 22 types (16 active, 6 pruned) | Compression: TrivialMemberCompressor(−16%) · BoilerplateCompressor(−8%) · StructuralDeduplicator(−7%) | Schema v2.0*
