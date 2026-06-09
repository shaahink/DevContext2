# Example: Architecture Overview of a Multi-Project Solution

**Scenario**: `architecture` — Get a comprehensive structural overview of any .NET solution.

**Command**:
```bash
devcontext analyze ./eShop.slnx --scenario architecture --profile focused
```

---

## What the output looks like

### Project Dependency Tree

```markdown
└── Web.Shopping.HttpAggregator
└── Auxiliary
└── Ordering.API
    ├── Ordering.Domain
    └── Ordering.Infrastructure
        ├── EventBus
        └── EventBusRabbitMQ
└── Webhooks.API
├── Identity.API
├── WebhookClient
├── WebStatus
├── Catalog.API
├── Basket.API
├── Payment.API
├── Web.Shopping.HttpAggregator
├── OrderProcessor.Worker
└── Ordering.SignalrHub
```

### Detected Architecture Signals

```
controllers · minimal-apis · mediatr · efcore · fluentvalidation
```

### Endpoints (16 in Catalog.API alone)

| Method | Route | Handler | Source |
|---|---|---|---|
| GET | /api/catalog/items | GetAllItems.GetAllItems | CatalogApi.cs:26 |
| GET | /api/catalog/items/{id:int} | GetItemById.GetItemById | CatalogApi.cs:36 |
| POST | /api/catalog/items | CreateItem.CreateItem | CatalogApi.cs:103 |
| PUT | /api/catalog/items/{id:int} | UpdateItem.UpdateItem | CatalogApi.cs:98 |
| DELETE | /api/catalog/items/{id:int} | DeleteItemById.DeleteItemById | CatalogApi.cs:107 |

### MediatR Handlers

| Kind | Request | Response | Handler |
|---|---|---|---|
| Command | CreateOrderCommand | bool | CreateOrderCommandHandler |
| Command | CancelOrderCommand | bool | CancelOrderCommandHandler |
| Command | ShipOrderCommand | bool | ShipOrderCommandHandler |

### EF Core Data Model

**OrderingContext**:

| Entity | Aggregate Root | Key Properties |
|---|---|---|
| Order | ✓ | Id |
| OrderItem | | Id |
| Buyer | ✓ | Id |
| CardType | ✓ | Id |

### Middleware Pipeline

| Type | Kind | Count | Sources |
|---|---|---|---|
| UseRouting | UseX | 1 | Program.cs |
| UseAuthentication | UseX | 1 | Program.cs |

### Anti-Patterns Detected

| Severity | Pattern | Description | Source |
|---|---|---|---|
| high | FireAndForget | Discard assignment to `NotifyAsync` | HooksRepository.cs:18 |
| high | FireAndForget | `_ = Task.Factory.StartNew(...)` without await | RabbitMQEventBus.cs:229 |
| high | ServiceLocator | IServiceScopeFactory.CreateScope() | MigrateDbContextExtensions.cs:33 |
| high | ServiceLocator | IServiceScopeFactory.CreateAsyncScope() | RabbitMQEventBus.cs:190 |
| medium | CancellationTokenNone | `CancellationToken.None` | App.xaml.cs:120 |
| medium | NewOutsideDI | `new ExternalProvider(...)` | AccountController.cs:223 |
| medium | UnboundedCollection | `ConcurrentDictionary<...> _onChangeSubscriptions` | HooksRepository.cs:8 |

---

## What this tells you

In 30 seconds, an LLM can see:

- **Architecture**: 18+ microservices with Domain/API/Infrastructure layering
- **Communication patterns**: MediatR for in-process commands, RabbitMQ for cross-service events
- **Data layer**: EF Core per service, SQLite in some, PostgreSQL in others
- **Issues**: Fire-and-forget in the event bus (`RabbitMQEventBus.cs:229`), service locator in infrastructure code, unbounded collections in `HooksRepository`
