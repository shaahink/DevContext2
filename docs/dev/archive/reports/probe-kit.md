# Trace Engine Validation Probe

> Created 2026-06-16 after PLAN-10 Parts A–E build.
> Gates Parts F (SemanticSymbolResolver) + G (Persistent Index).

## Purpose

Measure whether the syntactic trace (indirection bridged, Calls marked `[approx]`) teaches an
LLM better than raw source files or the legacy catalog output for a realistic feature task.

## The task

**"Add a per-line discount to orders."** Support a percentage or fixed-amount discount per
`OrderItem`, apply it during order creation, persist it, and reflect it in the order total.

Target repo: `eval-repos/eShop` — specifically the `Ordering.API` microservice.

Success criteria for the LLM answer:
- Finds the right files (OrdersApi.cs, CreateOrderCommandHandler, Order aggregate, EF config)
- Understands the MediatR dispatch flow (entry → send → handler → domain event → consumer)
- Implements discount as a property on OrderItem with validation
- Applies it in handler logic before persisting
- Updates persistence (EF migration or JSON column)
- Raises or updates domain events appropriately

## Three inputs (to generate)

### (a) Engine Trace

```bash
devcontext analyze eval-repos/eShop --focus "POST /api/orders" --depth 10
```

Produces the indirection-bridged trace starting at the entry point, through
`MediatR.Send`, `CreateOrderIdentifiedCommandHandler`, `CreateOrderCommandHandler`,
domain events (`OrderStartedDomainEvent`), EF persistence, and integration events.
Calls edges are marked `[approx]` until Part F.

### (b) Raw source files

The relevant source files from `Ordering.API`, `Ordering.Application`,
`Ordering.Domain`, and `Ordering.Infrastructure` projects. ~15–25 files covering
the endpoint, handlers, aggregate, repository, EF configuration, and event types.

### (c) Legacy catalog output

```bash
devcontext analyze eval-repos/eShop --scenario overview
```

The old 15-section disconnected catalog: endpoints table, DI table, entities table,
call graph, related types by layer, etc. — no indirection bridging.

## Scoring rubric

| Dimension | Weight | Metric |
|-----------|--------|--------|
| Correctness | 40% | Does the LLM produce working code that implements a per-line discount? |
| Navigation | 25% | How quickly does the LLM find the right files and understand the flow? |
| Completeness | 20% | Does it cover validation, persistence, domain events, integration events? |
| Explanation | 15% | Does the LLM correctly explain the MediatR flow, aggregate boundaries, and event dispatch? |

## How to run

1. Generate the three inputs (a, b, c) by running the engine commands above.
2. Start a **fresh** LLM session with no prior knowledge of eShop.
3. Present the task + one input variant (randomize order across runs if possible).
4. Score each run on the rubric above.
5. Record results in a `probe-results.md` under `docs/reports/`.

## Decision

| Result | Action |
|--------|--------|
| Trace significantly outperforms both raw files and legacy | Fund Parts F + G immediately |
| Trace beats legacy but not raw files | Fund F (semantic needed for trust) |
| Trace does not beat raw files | Re-evaluate approach before investing more infrastructure |
