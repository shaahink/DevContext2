# Phase 1 Re-Probe — Member-Origin Edge Correctness

> Run 2026-06-28, after Iteration 1 / Phase 1 (commit `4f457a3`, landed on `develop` via PR #1 `c84f088`).
> Satisfies the **periodic LLM probe** checkpoint in `docs/ACCEPTANCE.md` ("re-run the before/after probe
> after Phases 1, 3, and 8"). Companion to the archived `docs/archive/reports/probe-kit.md` (the full
> LLM-in-the-loop protocol) and `IDEAL-OUTPUT-TARGET.md` §7.
>
> This report records the **trace-quality delta** the member-origin fix produced (the substrate the LLM
> probe consumes). The full fresh-agent A/B run (inputs a–d, scored on the rubric) is a human-in-the-loop
> step; the protocol to reproduce it is in §4.

## 1. What Phase 1 changed (the hypothesis under test)

Before Phase 1, call/raise/send/data edges attached to the **Type** node, and a Member node inherited
**all** of its Type's edges. A trace anchored on one method therefore showed *every sibling method's*
wiring — fabricated `[verified]` edges, wrong EMITS/TOUCHES, and (per the original probe) it could also
**miss the real path**. Phase 1 makes edges originate from the **method** that contains them and replaces
the inherit-everything traversal with a controlled handler bridge.

**Claim being probed:** a member-anchored trace now shows *only that method's* wiring, so it stops
misleading a tool-using agent (moves the trace from "primer" toward "accelerator").

## 2. Evidence A — sibling divergence (`eval-repos/eShop/src/Catalog.API`)

Two sibling methods of one class, same command, `--depth 4`.

### BEFORE (broken) — byte-identical, headed by the *type*
```
TRACE  CatalogApi                      TRACE  CatalogApi
> ENTRY CatalogApi                     > ENTRY CatalogApi
   raises ProductPriceChangedIntegrationEvent   <-- belongs to UpdateItem
   data CatalogType                    (identical)
   call CatalogServices  ".gif"=>...   <-- DeleteItemById's body (sibling)
   call CatalogIntegrationEventService -> ResilientTransaction -> RabbitMQEventBus
TOUCHES CatalogType, CatalogItem, CatalogBrand
EMITS   ProductPriceChangedIntegrationEvent, IntegrationEventLogEntry
```
`--focus CatalogApi:CreateItem` and `:UpdateItem` produced **identical** output. The method was ignored;
CreateItem fabricated UpdateItem's price-changed event and DeleteItemById's delete chain.

### AFTER (fixed) — divergent, headed by the *method*
```
TRACE  CatalogApi.CreateItem                    TRACE  CatalogApi.UpdateItem
> ENTRY CatalogApi.CreateItem                   > ENTRY CatalogApi.UpdateItem
   data CatalogType                                raises ProductPriceChangedIntegrationEvent  [correct]
   call CatalogAI.GetEmbeddingAsync                data CatalogItem
   call CatalogServices.Add                        call CatalogServices.SingleOrDefaultAsync
   call CatalogContext.SaveChangesAsync            call CatalogIntegrationEventService
                                                     .SaveEventAndCatalogContextChangesAsync
                                                      -> SaveEventAsync -> raises IntegrationEventLogEntry
                                                     .PublishThroughEventBusAsync
                                                      -> RabbitMQEventBus.PublishAsync   [genuine, this method]
TOUCHES CatalogType, CatalogItem, CatalogBrand   TOUCHES CatalogItem, CatalogBrand, CatalogType
EMITS   (none)                                   EMITS  ProductPriceChangedIntegrationEvent,
                                                        IntegrationEventLogEntry
```
- Headers name the method. The traces **differ**.
- CreateItem (only adds an item) no longer raises `ProductPriceChangedIntegrationEvent`; its EMITS is empty.
- UpdateItem genuinely raises it and reaches the bus (its method *does* call `PublishThroughEventBusAsync`).

## 3. Evidence B — the eShop CQRS spine (`eval-repos/eShop/src/Ordering.API`)

`--focus "POST /api/orders/" --depth 6`.

### BEFORE (broken) — sibling sends + every Order domain event
```
> ENTRY POST /api/orders/
   call OrdersApi.CreateOrderAsync
      send CreateOrderCommand   -> handler CreateOrderCommandHandler -> raises OrderStartedIntegrationEvent
      send CancelOrderCommand   <-- FABRICATED (CancelOrderAsync sibling)
      send ShipOrderCommand     <-- FABRICATED (ShipOrderAsync sibling)
      data Order
         raises OrderStatusChangedToAwaitingValidationDomainEvent
         raises OrderStatusChangedToStockConfirmedDomainEvent
         raises OrderStatusChangedToPaidDomainEvent
         raises OrderShippedDomainEvent      <-- FABRICATED (Order.SetShippedStatus)
         raises OrderCancelledDomainEvent    <-- FABRICATED (Order.SetCancelledStatus)
EMITS  OrderStartedIntegrationEvent, ...6 domain events..., IntegrationEventLogEntry   (7 events)
```

### AFTER (fixed) — only CreateOrderAsync's wiring; spine intact via the handler bridge
```
> ENTRY POST /api/orders/
   call OrdersApi.CreateOrderAsync
      send CreateOrderCommand
         handler CreateOrderCommandHandler           [Type node -> bridges to its Handle member]
            raises OrderStartedIntegrationEvent       [genuine, Handle body]
            call OrderingIntegrationEventService.AddAndSaveEventAsync   [outbox write]
               call IntegrationEventLogService.SaveEventAsync
                  raises IntegrationEventLogEntry
            call Order.AddOrderItem
            call OrderRepository.Add -> OrderingContext.SaveEntitiesAsync -> SaveChangesAsync
      data CardType
EMITS  OrderStartedIntegrationEvent, IntegrationEventLogEntry            (2 events)
```
- **Gone:** `CancelOrderCommand`/`ShipOrderCommand` (sibling methods) and the five unrelated Order
  domain-event raises (raised in unrelated `Order` methods, dumped wholesale by the old type-level
  `data Order` edge). EMITS dropped 7 -> 2.
- **Survives:** the genuine `send -> handler -> raises OrderStartedIntegrationEvent -> outbox write`
  spine, reached because the controlled bridge expands the handler Type's `Handle` member.
- **Honest absence:** the RabbitMQ publish is *not* shown here — Ordering's `AddAndSaveEventAsync` only
  writes the outbox; the publish is a deferred `PublishEventsThroughEventBusAsync` reached via a
  transaction-pipeline behavior (Iteration 3). Showing it would be the very fabrication Phase 1 removes.
  (Contrast Catalog `UpdateItem`, whose method genuinely calls the bus and still shows it — §2.)

## 4. Full LLM A/B protocol (human-in-the-loop, for the periodic probe)

Reuse the archived task (`docs/archive/reports/probe-kit.md`): **"Add a per-line discount to orders"** on
`eval-repos/eShop` `Ordering.API`. Give a *fresh* agent the task with each input variant and score on the
rubric (Correctness 40 / Navigation 25 / Completeness 20 / Explanation 15):

- **(a) trace only** — `analyze eval-repos/eShop/src/Ordering.API --focus "POST /api/orders/" --depth 8`
- **(b) raw source files** — the ~15–25 relevant files
- **(c) repo + tools** — agent with file tools, no trace
- **(d) repo + tools + trace** — the accelerator hypothesis

**Acceptance target:** with the *fixed* trace, (d) should reduce a tool-using agent's cost vs (c) — moving
the trace from "primer" (cheap orientation) to "accelerator". The prior run scored the *old* trace
5.55/10 with "method-level anchoring missing" and "sibling/fabrication" noise as named gaps (§probe-results
items #2, #8–#12) — Phase 1 closes the anchoring + fabrication gaps; domain-event chain + pipeline remain
Iteration 3.

> Note: this checkpoint records the substrate delta. Run the scored A/B with a genuinely fresh agent
> (no eShop context) and append the scorecard below when executed.

## 5. Qualitative scorecard delta (substrate, pre-LLM-run)

| Dimension | Old probe (2026-06-16) | Phase-1 substrate | Why |
|---|---|---|---|
| Correctness | 6/10 | improved | No fabricated sibling edges; `[verified]` is now method-scoped |
| Navigation | 7/10 | improved | Method-level anchoring (`ENTRY CatalogApi.CreateItem`), not the bare type |
| Completeness | 4/10 | unchanged here | Domain-event chain + pipeline are Iteration 3 (intentionally out of Phase 1) |
| Explanation | 5/10 | improved | Trace no longer asserts wiring a method doesn't have; honest about deferred bus |

## 6. Status

Phase-1 trace substrate **passes** the divergence + no-fabrication bar (locked by
`TraceQualityTests.Sibling_methods_produce_divergent_traces_no_fabricated_edges` and
`Orders_trace_keeps_the_real_spine_and_drops_sibling_edges`). The scored fresh-agent A/B is queued for the
next human-in-the-loop session; re-run again after Iteration 3 (complete + honest traces) per
`ACCEPTANCE.md`.
