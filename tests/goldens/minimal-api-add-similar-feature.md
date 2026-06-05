## DevContext -- Architecture Overview on MinimalApiProject

**Architecture**: MinimalApi (100% confidence)
**Signals**: dapper · minimal-apis · mediatr · efcore
**Projects**: 3 -- Infrastructure, Api, Core
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 5 in output

---
## Architecture overview

- Infrastructure
- Api
- Core

## Endpoints

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| POST | /orders | async (CreateOrderCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/orders/{id}", id);
}.<lambda> | - |
| GET | /orders | async (IMediator mediator) =>
{
    var orders = await mediator.Send(new GetOrdersQuery());
    return Results.Ok(orders);
}.<lambda> | - |

## MediatR Handlers

| Kind | Request | Response | Handler |
|------|---------|----------|---------|
| Command | CreateOrderCommand | int | CreateOrderHandler |

## Non-obvious wiring

### Middleware pipeline

| Order | Type | Kind |
|-------|------|------|
| 101 | MapGet | MapX |
| 102 | MapPost | MapX |

### DI registrations

| Lifetime | Service | Implementation |
|----------|---------|----------------|
| Extension | AddMediatR | cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly) |

## Related types grouped by layer

- **Api**: Order, CreateOrderCommand, CreateOrderHandler
- **Infrastructure**: OrderRepository
- **Unknown**: IOrderRepository

---
*Generated in {elapsed}ms | 7 types (5 active, 2 pruned) | Schema v2.0*