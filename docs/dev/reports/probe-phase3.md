# Phase 3 Re-Probe — Complete & Honest Traces

> Run 2026-06-28, after Iteration 3 / Phase 3 (branch `feature/iter3-complete-honest-traces`).
> Satisfies the **periodic LLM probe** checkpoint (`docs/product/ACCEPTANCE.md` — re-run after Phases 1, 3, 8) and
> the Iteration-3 guide's re-probe gate. Records the trace-completeness delta (the substrate the scored
> A/B consumes); the scored fresh-agent run is the human-in-the-loop step in §4.

## 1. What Phase 3 changed

Iteration 1 made an aggregate's ctor raise only *its* event (not all six), so the right domain-event path
was no longer drowned. Phase 3 makes the trace **follow** that path to its handler, count entities reached
via calls, show cross-cutting once, and be honest about cuts.

## 2. Evidence — `POST /api/orders/` (eShop Ordering.API)

### BEFORE Phase 3 (post Iter 1–2)
```
send CreateOrderCommand
   handler CreateOrderCommandHandler
      raises OrderStartedIntegrationEvent        (integration event only)
      call OrderingIntegrationEventService.AddAndSaveEventAsync → ... outbox
      call Order.AddOrderItem
      call OrderRepository.Add → data Order        <-- STOPS; the domain event is never followed
TOUCHES  CardType, Order, OrderItem, PaymentMethod   (no Buyer)
```
The domain-event path (`OrderStartedDomainEvent → ValidateOrAddBuyer…Handler`) — the thing the probe
agent had to find by hand — was **missing**. `data Order` dead-ended (the ctor's raise was on a member
the trace couldn't reach), and the pipeline/truncation were invisible.

### AFTER Phase 3
```
send CreateOrderCommand
      pipeline ▸ LoggingBehavior → ValidatorBehavior → TransactionBehavior   (Step 3, once)
   handler CreateOrderCommandHandler
      raises OrderStartedIntegrationEvent → ... outbox
      call OrderRepository.Add → data Order
         call Order.AddOrderStartedDomainEvent           (Step 2: ctor bridge)
            raises OrderStartedDomainEvent               (variable-form raise now detected)
               consumes ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler
                  call BuyerRepository.FindAsync / Add    (the buyer-validation logic)
                  (stopped at depth N; K branches omitted)   (Step 4: honest cut)
TOUCHES  CardType, Order, OrderItem, Buyer, PaymentMethod    (Step 1: Buyer via Calls)
EMITS    OrderStartedIntegrationEvent, IntegrationEventLogEntry, OrderStartedDomainEvent, ...
```

| Completeness element | Before | After |
|---|---|---|
| Domain-event → handler path (Step 2) | missing | **present** (`OrderStartedDomainEvent → ValidateOrAddBuyer…Handler`) |
| TOUCHES via Calls / EF access (Step 1) | no `Buyer` | **`Buyer`** added |
| Cross-cutting pipeline (Step 3) | invisible | **once** under the send (incl. `TransactionBehavior`) |
| Honest truncation (Step 4) | silent | **explicit** (`stopped at depth N; K branches omitted`) |
| Sibling fabrications (Iter 1 guard) | gone | still gone (no Shipped/Cancelled) |

### How the chain was fixed (two real gaps)
1. **Reachability** — `new Order(...)` is an object creation, not a call, so `data Order` lands on
   `Type:Order`. The controlled bridge was extended to expand an entity/aggregate Type to its **constructor**
   member, surfacing the ctor's domain-event raise (scoped by entity/aggregate tag — non-domain ctors
   aren't expanded).
2. **Detection** — eShop's `AddOrderStartedDomainEvent` raises via a **variable**
   (`var e = new OrderStartedDomainEvent(...); this.AddDomainEvent(e);`), which the inline-only
   `AddDomainEvent(new X(` regex missed (while the six *other* events use the inline form). `AddRaises` now
   resolves the variable to its `new EventType()` like `AddSends` does. This is the single event the trace
   needed — the others stay member-scoped and hidden.

## 3. Known caveat (deferred to Iteration 4)

DntSite `GET /Feed/News` TOUCHES is still **empty**: DntSite uses EF Core but its entities aren't detected
by `EfCoreExtractor` (custom base class / Gridify pattern — **0 aggregates** detected vs eShop's 5). The
Step-1 mechanism (collect entities reached via Calls, resolving a Member callee to its owning entity Type)
is correct and validated on eShop; surfacing DntSite's entities needs an `EfCoreExtractor` improvement,
which is Iteration 4 (honest Map & detection), not Iteration 3.

## 4. Scored A/B protocol (human-in-the-loop)

Per `IDEAL-OUTPUT-TARGET.md §7`: conditions **C** (repo + tools, no trace) vs **D** (repo + tools + the
fixed trace) on the task **"add a per-line discount to orders"** (eShop `Ordering.API`), plus a controller
repo + a 2nd task. Score on the rubric (Correctness 40 / Navigation 25 / Completeness 20 / Explanation 15).

**Target:** the fixed trace reduces a tool-using agent's cost — moving from "primer" (the Iteration-0
result, 5.55/10, named gaps: method anchoring, domain-event chain, pipeline) toward "accelerator". Phase 1
closed the anchoring + fabrication gaps; **Phase 3 closes the domain-event chain + pipeline + honest-cut
gaps** — the remaining named completeness gaps from the original probe. Run the scored A/B with a genuinely
fresh agent (no eShop context) and append the scorecard below.

### Scorecard (fill after run)

| Condition | Correctness | Navigation | Completeness | Explanation | Weighted |
|---|---|---|---|---|---|
| C (repo + tools) | | | | | |
| D (repo + tools + trace) | | | | | |

## 5. Status

Phase-3 trace substrate **passes** the completeness + honesty bar (locked by
`TraceQualityTests.Orders_trace_is_complete_and_honest`). The scored fresh-agent A/B is queued; re-run
again after Iteration 8 (MCP) per `ACCEPTANCE.md`.
