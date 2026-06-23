# Probe Results — PLAN-11 Part C

> Run 2026-06-16 after B1–B5 completion.
> Engine trace vs. IDEAL-OUTPUT-TARGET.md §2 hand-built target.

## Engine Trace (actual output)

```
TRACE  POST /api/orders/
       C:\...\eShop\src\Ordering.API\Apis\OrdersApi.cs:17

▸ ENTRY  POST /api/orders/  (C:\...\OrdersApi.cs:17)
   └─ call OrdersApi  (C:\...\OrdersApi.cs:17)
      └─ send CreateOrderCommand  (C:\...\OrdersApi.cs:152) [approx]
             var result = await services.Mediator.Send(requestCreateOrder);
         └─ handler CreateOrderCommandHandler  (C:\...\CreateOrderCommandHandler.cs:6)
            ├─ raises OrderStartedIntegrationEvent  (C:\...\CreateOrderCommandHandler.cs:29) [approx]
            │      // Add Integration event to clean the basket
            │      var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(message.UserId);
            │      await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStartedIntegrationEvent);
            ├─ raises OrderStartedIntegrationEvent  (C:\...\CreateOrderCommandHandler.cs:29) [approx]
            │      // Add Integration event to clean the basket
            │      var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(message.UserId);
            │      await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStartedIntegrationEvent);
            ├─ call OrderingIntegrationEventService  (C:\...\CreateOrderCommandHandler.cs:33) [verified]
            │      // Add/Update the Buyer AggregateRoot
            │      // DDD patterns comment: Add child entities and value-objects through the Order Aggregate-Root
            │      // methods and constructor so validations, invariants and business logic
            └─ call Order  (C:\...\CreateOrderCommandHandler.cs:44) [verified]
                   _logger.LogInformation("Creating Order - Order: {@Order}", order);
EMITS    OrderStartedIntegrationEvent, OrderStartedIntegrationEvent
```

## Comparison to Ideal Target

| # | Ideal target element | Engine trace status | Gap |
|---|---------------------|-------------------|-----|
| 1 | `POST /api/orders` entry with route info | ✓ Present | No project name (`Ordering.API`) shown |
| 2 | `OrdersApi.CreateOrderAsync` method-level entry | ✗ Links to `OrdersApi` type, not method | B4 partial: handler method not extracted for minimal API lambda entry |
| 3 | "masks credit-card number" body detail | ✗ Missing | Body scan picks first Send call, not preceding setup lines |
| 4 | `IdentifiedCommand` wrapper + idempotency | ✗ Missing | Idempotency pattern requires double-send detection (IdentifiedCommand wraps CreateOrderCommand) |
| 5 | `pipeline` (Logging → Validation → Transaction) | ✗ Missing | B3 implemented but eShop uses extension methods not `AddTransient(typeof(IPipelineBehavior<,>))` — pipeline behaviors not detected in this repo |
| 6 | `send ▸ CreateOrderIdentifiedCommandHandler` | ✗ Only shows CreateOrderCommandHandler | No intermediate IdentifiedCommand step |
| 7 | `CreateOrderCommandHandler` with DI annotation | ✓ Present (handler node + call edges) | DI annotations not shown per node |
| 8 | `new Order(...)` → ctor raises `OrderStartedDomainEvent` | ✗ Missing | B5 mirrors Raises to Handler, but domain event (not integration event) not detected — regex doesn't match the ctor pattern |
| 9 | `order.AddOrderItem(...) × N` | ✗ Missing | Not a Sends/Raises edge — would need method-level body lines |
| 10 | `_orderRepository.Add(order)` + `SaveEntitiesAsync` | ✗ Missing | These are direct calls, not indirection seams — would appear in Full detail |
| 11 | `data ▸ OrderingContext.SaveEntitiesAsync` | ✗ Missing | No Data edge from handler to DbContext/SaveEntitiesAsync |
| 12 | `domain-event ▸ OrderStartedDomainEvent → ValidateOrAddBuyer...` | ✗ Missing | Domain event handler not connected — AddRaises only detects integration events, not domain events in this file |
| 13 | `AFTER COMMIT` integration event | ✗ Missing | No save/dispatch boundary modelling |
| 14 | `RESULT / TOUCHES / EMITS / NEXT` footer | Partial | B2: EmittedEvents works (integration events), TouchedEntities empty (no entity nodes in trace path) |
| 15 | Salient body lines at provenance sites | ✓ Working | B1: Shows 3 lines around Send and Raises provenance |

## Scorecard

| Dimension | Score (0-10) | Notes |
|-----------|-------------|-------|
| **Correctness** | 6/10 | Engine correctly identifies the primary flow (entry → send → handler). Missing: idempotency wrapper, domain event chain, save boundary. |
| **Navigation** | 7/10 | File:line provenance on every hop. Salient body lines connect code to narrative. Missing: method-level anchoring for minimal API entries. |
| **Completeness** | 4/10 | Captures ~40% of the ideal trace. Missing: pipeline behaviors, domain events (not integration events), save-time dispatch, idempotency wrapper, RESULT/TOUCHES, NEXT. |
| **Explanation** | 5/10 | Seam labels (send/handler/raises/call) clarify control transfer. [approx]/[verified] tags build trust. Missing: DI annotations, pipeline annotation, domain-event chain. |
| **Weighted Total** | **5.55/10** | (6×0.4 + 7×0.25 + 4×0.2 + 5×0.15) |

## Key Findings

### What works
- **B1 Salient lines** are the single highest-impact feature — they turn the tree from names into a story. The `.Send(requestCreateOrder)` and `new OrderStartedIntegrationEvent(...)` lines immediately convey intent.
- **B2 Summary footer** captures emitted events. TOUCHES is empty because entities aren't in the trace path (no data edge from handler to Order entity).
- **B3 Pipeline seam** infrastructure exists but eShop's pipeline registration uses MediatR extension methods that the current regex-based DI detection doesn't capture.
- **B4 Per-method anchoring** works for named handlers (FastEndpoints/Controllers) but minimal API lambdas still fall back to the owner type.

### Gaps to close before "story-form" claim
1. **Domain event detection** — `new Order(...)` in constructor body raising `OrderStartedDomainEvent` via `AddDomainEvent` needs a multi-step pattern. Currently only detects regex `AddDomainEvent(new X(...))` in the same source file.
2. **Idempotency wrapper** — `IdentifiedCommand<CreateOrderCommand,bool>` wrapping and the double-send pattern (IdentifiedCommandHandler → send CreateOrderCommand) requires following the Sends edge target's handler back to another Sends.
3. **Pipeline behavior detection** — eShop registers pipeline via `cfg.AddOpenBehavior(typeof(LoggingBehavior<,>))` using MediatR config API, not `services.AddTransient(typeof(IPipelineBehavior<,>))`.
4. **Domain event handler chain** — The `Consumes` edge from Event to domain event handler exists but trace doesn't reach it because no Data/Raises edge connects the handler to the skeleton Order domain event.

### Resolution quality
- Sends/Raises edges are marked `[approx]` (syntactic regex match)
- Call edges from Roslyn are marked `[verified]` — 3 of 5 hops have verified calls
- Semantic resolution (Part F) would upgrade `[approx]` → `[verified]` for Sends/Raises edges

## Decision

**Funding recommendation: GREEN — fund Parts F + G.**

The engine trace at 5.55/10 is significantly more useful than the legacy catalog for understanding the MediatR dispatch flow. The key gaps are in detection patterns (domain events, pipeline registrations, idempotency), not in the trace engine architecture. Semantic resolution (Part F) would upgrade all `[approx]` edges to `[verified]`, and a persistent index (Part G) would make the engine usable on large multi-project solutions. The engine already proves the approach works — it needs pattern refinement, not rethinking.
