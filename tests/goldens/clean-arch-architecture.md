## DevContext -- Architecture Overview on CleanArch

**Architecture**: MinimalApi (80% confidence)
**Signals**: minimal-apis · mediatr · efcore
**Projects**: 4 -- Web, Infrastructure, Domain, Application
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 4 in output

---
## Architecture overview

- Web
- Infrastructure
- Domain
- Application

## Endpoints

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| GET | /products | async (IMediator mediator) =>
{
    var products = await mediator.Send(new GetProductsQuery());
    return Results.Ok(products);
}.<lambda> | - |

## MediatR Handlers

| Kind | Request | Response | Handler |
|------|---------|----------|---------|
| Command | GetProductsQuery | List<Product> | GetProductsHandler |

## Non-obvious wiring

### Middleware pipeline

| Order | Type | Kind |
|-------|------|------|
| 1 | MapGet | MapX |

### DI registrations

| Lifetime | Service | Implementation |
|----------|---------|----------------|
| Extension | AddDbContext | options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")) |
| Extension | AddMediatR | cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductsHandler).Assembly) |

## Related types grouped by layer

- **Application**: GetProductsHandler, GetProductsQuery
- **Domain**: Product
- **Infrastructure**: AppDbContext

---
*Generated in {elapsed}ms | 4 types (4 active, 0 pruned) | Schema v2.0*