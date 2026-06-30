# Ideal Output Target

> What DevContext *should* produce, designed from the output backwards — independent of the
> current CLI flags, desktop layout, and section model. Grounded in real target repos
> (`eval-repos/`). This is the artifact we score ourselves against; the engine and surface are
> whatever it takes to produce it.
>
> Companion to `DESIGN-PHILOSOPHY.md`. Where the philosophy states principles, this states the
> concrete deliverable.

## The core reframe: catalog → trace

Today the tool emits ~15 **disconnected catalogs** (endpoints table, DI table, entities table,
call graph, event flow…). Each is individually correct; nothing connects them. The reader gets
an *inventory*, not an explanation of how the system *works*, and has to re-assemble a story the
tool already had all the pieces for.

The ideal output is built from two artifacts:

- **The Map** — orientation, no code. "What is this and where do things live."
- **The Trace** — an entry point followed *down the wiring*, with indirection bridged.
  "How does this operation actually work."

Everything else (token budget, sections, focus, depth) is a dial on these two.

---

## 1. What we analyze (archetypes)

The unifying abstraction is the **entry point** plus the **wiring between entry points**.
What counts as an entry differs by archetype, but the model is the same.

| # | Archetype | Examples in `eval-repos/` | Entry points are… | Primary artifact |
|---|-----------|---------------------------|-------------------|------------------|
| A | **Layered / CQRS web app** (Clean/DDD, MediatR, EF, events) | eShop `Ordering.API`, VerticalSlice | HTTP endpoints, message/integration-event consumers, domain-event handlers, hosted services | **Trace per entry** + Map |
| B | **Simple web API** (Minimal API / controllers, direct EF) | TodoApi | HTTP endpoints | Trace (shallow) + Map |
| C | **Microservice constellation** (Aspire-orchestrated) | eShop (whole) | Per-service entries + the integration-event bus *between* services | Map of services + cross-service event topology; drill into one service |
| D | **Library / framework** (no app entry points) | AutoMapper | Public API surface (types + public methods) | **Surface map**, not traces |

Future (same model, different entry kinds): Blazor/desktop apps (component lifecycle + event
handlers), CLI tools (commands), Functions (triggers).

**Design consequence.** "App" and "library" are *different products of the same engine*. Forcing
one output shape on both is why AutoMapper output is useless today. Archetype is auto-detected
(no application entries + packable library → surface mode).

---

## 2. The Trace (the centerpiece)

A Trace starts at one entry point and follows execution **down the wiring**, bridging the
indirection that a naive call graph drops. Each hop is labelled by its **seam kind** so the
reader knows *how* control actually transfers.

Seam kinds:

| Label | Meaning | How it's resolved |
|-------|---------|-------------------|
| `call` | direct method call | semantic call edge |
| `di` | interface → concrete impl | DI registration, else single implementor, else `ambiguous` |
| `send` | MediatR `Send`/`Publish` → handler | join request/notification type → `IRequestHandler<>` / `INotificationHandler<>` |
| `pipeline` | cross-cutting wrapper around every `send` | MediatR `IPipelineBehavior<>` registrations |
| `domain-event` | aggregate raises event, dispatched at save | detect UoW dispatch convention, join event type → handler |
| `integration-event` | published to bus, consumed elsewhere (often cross-service) | join published event type → consumer, across projects |
| `data` | EF write/read → tables | `DbSet` + `IEntityTypeConfiguration` |

### Real ideal Trace — `POST /api/orders` (eShop Ordering.API)

Hand-built from the actual source. This is the target.

```
TRACE  POST api/orders  —  "create an order"
       eShop · Ordering.API   (Minimal API, v1.0, group "api/orders")

▸ ENTRY  OrdersApi.CreateOrderAsync(requestId, CreateOrderRequest, OrderServices)
         Apis/OrdersApi.cs:17 → :118
         1. masks the credit-card number
         2. builds CreateOrderCommand from the request body            :141
         3. wraps it in IdentifiedCommand<CreateOrderCommand,bool>      :146  (idempotency via x-requestid)
         4. dispatches it ──▼

  send ▸ MediatR.Send(IdentifiedCommand<CreateOrderCommand,bool>)
  pipeline   every Send is wrapped:  Logging → Validation → Transaction   Application/Behaviors/*.cs
             validator for this request: CreateOrderCommandValidator      Application/Validations/CreateOrderCommandValidator.cs
             Transaction = one EF transaction; integration events published only after it commits

  send ▸ CreateOrderIdentifiedCommandHandler : IdentifiedCommandHandler<CreateOrderCommand,bool>
         Application/Commands/CreateOrderCommandHandler.cs:57
         • dedupes by request id (di: IRequestManager → RequestManager); duplicate → returns true, stops here
         • first time → re-dispatches the inner command ──▼

  send ▸ CreateOrderCommandHandler.Handle(CreateOrderCommand)
         Application/Commands/CreateOrderCommandHandler.cs:29
         di:  IOrderRepository → OrderRepository · IOrderingIntegrationEventService → OrderingIntegrationEventService
              IIdentityService → IdentityService · IMediator · ILogger
         body:
           1. new OrderStartedIntegrationEvent(userId)
              └ integration-event ▸ AddAndSaveEventAsync(evt)   — queued in outbox, published after commit
           2. new Order(userId, userName, address, card…)       Domain/AggregatesModel/OrderAggregate/Order.cs:52
              └ ctor raises  OrderStartedDomainEvent             Order.cs:170  (queued on the aggregate)
           3. order.AddOrderItem(...) × N                        Order.cs:71   (aggregate-guarded; merges dup products)
           4. _orderRepository.Add(order)                        call → OrderRepository.Add
           5. await UnitOfWork.SaveEntitiesAsync() ──▼           Infrastructure/OrderingContext.cs:47

  data ▸ OrderingContext.SaveEntitiesAsync   (DbContext, IUnitOfWork)
         • FIRST dispatches queued domain events, THEN EF SaveChanges (one tx)   MediatorExtension.cs:19
         • writes → ordering.orders, ordering.orderitems (+ owned Address)        EntityConfigurations/OrderEntityTypeConfiguration.cs

  domain-event ▸ OrderStartedDomainEvent  →  ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler
         Application/DomainEventHandlers/ValidateOrAddBuyer…cs
         • creates/validates the Buyer aggregate + payment method for the user (writes in the same tx)

◇ AFTER COMMIT — published to the event bus:
   integration-event ▸ OrderStartedIntegrationEvent  →  consumed by Basket.API  (clears the user's basket)   [cross-service]

RESULT  200 OK (empty)  ·  failure → 400 BadRequest
TOUCHES aggregates: Order, OrderItem, Address(VO), Buyer   ·   tables: ordering.orders, ordering.orderitems
EMITS   OrderStartedDomainEvent (in-proc)  ·  OrderStartedIntegrationEvent (cross-service)
NEXT    order lifecycle continues: SetAwaitingValidationStatus → SetStockConfirmedStatus → SetPaidStatus → SetShippedStatus,
        each raising its own domain + integration events  (see trace: order-lifecycle)
```

Why this is the target and not today's output:

- **The four most important hops are invisible to the current syntactic call graph.** `Mediator.Send`
  (×2 incl. the idempotency wrapper) lands on a leaf; `mediator.Publish(domainEvent)` is pulled off
  EF's change tracker generically (`MediatorExtension.cs:19`) so *nothing textual* connects
  `new Order(...)` to the buyer-validation handler; the integration event crosses a process
  boundary. These are **joins the tool can already source** (it has the MediatR map, the event
  detections, the DI map) — it just doesn't assemble them.
- It reads as a **story**, depth-bounded, with file:line provenance — exactly what an LLM needs to
  modify the flow correctly.
- It shows cross-cutting behavior **once** (the pipeline), not smeared across every row.

### Dials on a Trace

- **depth** — how many `send`/`call` hops to follow (framework boundary auto-stops).
- **fan-out / detail** — at each node: signature only, or include the salient body lines (as above),
  or full source. Maps to the old `focused`/`debug`/`full` without the user learning those words.
- **budget** — a Trace is naturally bounded; budget trims depth × fan-out, and the cut is shown
  ("stopped at depth 4; 3 deeper branches omitted").

---

## 3. The Map (orientation, no code)

Produced when no entry is chosen. Answers "what is this." Real sketch for the Ordering service:

```
MAP  eShop · Ordering.API     (1 microservice in a 24-project Aspire solution)

STACK  .NET 9 · Minimal APIs (Asp.Versioning) · MediatR (CQRS) · EF Core (SQL Server)
       FluentValidation · RabbitMQ event bus · DDD aggregates
STYLE  Clean/DDD layering — API → Application → Domain → Infrastructure
       (confidence high; evidence: project refs + folder roles + MediatR + AggregatesModel)

TOPOLOGY (depends-on)
   Ordering.API ─┬─ Ordering.Domain
                 ├─ Ordering.Infrastructure ── Ordering.Domain
                 └─ EventBus · IntegrationEventLogEF · ServiceDefaults(Aspire)

ENTRY POINTS
   HTTP (7)   POST api/orders         → CreateOrderCommand
              POST api/orders/draft   → CreateOrderDraftCommand
              PUT  api/orders/cancel  → CancelOrderCommand
              PUT  api/orders/ship    → ShipOrderCommand
              GET  api/orders/{id}    → OrderQueries (read side)
              GET  api/orders         → OrderQueries
              GET  api/orders/cardtypes → OrderQueries
   Bus (5)    integration-event consumers: GracePeriodConfirmed, OrderPaymentSucceeded,
              OrderStockConfirmed, OrderStockRejected, OrderPaymentFailed
   Domain (7) domain-event handlers: OrderStarted→Buyer validation, →AwaitingValidation,
              →StockConfirmed, →Paid, →Shipped, →Cancelled, BuyerPaymentVerified

CROSS-CUTTING
   MediatR pipeline (every command):  Logging → Validation(FluentValidation) → Transaction
   Persistence:  OrderingContext (IUnitOfWork) — dispatches domain events then EF SaveChanges
   Aggregates:   Order(root) · OrderItem · Address(VO) · Buyer(root) · PaymentMethod

→ drill in:  trace POST /api/orders        → list all:  trace --all
```

At the **constellation** level (archetype C), the Map zooms out to services + the integration-event
bus *between* them (who publishes `OrderStartedIntegrationEvent`, who consumes it) — the wiring that
no single project reveals.

The Map deliberately omits bodies. The only "sections" left to toggle are *which facets of the map*
to show (topology / entries / cross-cutting / data model) — a far smaller surface than today's 12
section checkboxes.

---

## 4. Library archetype — IMPLEMENTED (FluentValidation, Polly, CommunityToolkit.Mvvm)

No application entries → a ranked **surface map**, organized by how a consumer uses the library, not a
call stack. Shipped: archetype auto-detection (sample/benchmark apps don't flip a library to App) →
`LibrarySurfaceRenderer`. Worked examples + gates: `eval-results/{FluentValidation,Polly}/BENCHMARK.md`,
`eval/expectations/{fluentvalidation,polly}.json` (all `expected`, green).

Real output (FluentValidation), build-free (syntax + `///` docs only):

```
LIBRARY  FluentValidation     (92 public types)

ENTRY API                       (ranked: register → build → derive/implement → extend, with /// summary)
   register  ServiceCollectionExtensions.AddValidatorsFromAssembly      "Adds all validators in specified assembly"
   derive    AbstractValidator                                          "Base class for object validators."
   extend    DefaultValidatorExtensions                                 "…the default set of validators."  (this IRuleBuilder)
ABSTRACTIONS                    (seats consumers implement/derive, by implementor count)
   AbstractValidator (class) · IPropertyValidator (interface) · IValidationRule (interface) · …
PUBLIC SURFACE                  (by namespace · docs · *.Internal demoted)
   FluentValidation · FluentValidation.Results · FluentValidation.Validators · …
   INTERNAL  (15 types in *.Internal — available on request)
CONSUMER PATHS
   wire into DI → AddValidatorsFromAssembly(...)   ·   build one → derive AbstractValidator
PACKAGES                        (runtime only — test/benchmark/sample deps excluded)
```

Builder-shaped libraries get a `build` entry: Polly's `ResiliencePipelineBuilder` (a `*Builder` with a
public `Build()`) leads its surface. **Source-generator libraries** (e.g. CommunityToolkit.Mvvm) lead with
their marker attributes as an `annotate` tier (`[ObservableProperty]`, `[RelayCommand]`) and get a
`GENERATORS` section listing the source generators / analyzers / code fixers they ship. Traces are off by
default for libraries (a public method's "call stack down" is library internals, rarely what a consumer
wants); available on demand for a chosen method.

---

## 5. What the engine must produce (deltas from today)

1. **Unified `EntryPoint` model** — endpoints, event/message consumers, domain-event handlers,
   hosted services, (library) public API — one list, today they are separate detection tables.
2. **Indirection-bridged graph** — the trace must follow `call · di · send · pipeline ·
   domain-event · integration-event · data`. The join data already exists in separate extractors
   (MediatR map, event detections, DI map, EF config); the missing work is *assembling* it into one
   traversable graph rather than rendering each piece alone.
3. **Trustworthy resolution** — the foundational fork: the current call graph is syntactic +
   string-heuristic (first-impl-wins, locals unresolved, no overloads). Entry-rooted traces need
   Roslyn `SemanticModel` symbol resolution (hybrid: semantic where the workspace compiles,
   syntactic fallback otherwise). Decide before building.
4. **Entry-rooted assembly + rendering** — DFS from the entry, depth + fan-out caps, framework-
   boundary stop, revisit dedup, salient-body extraction (not full dump), seam labelling, budget
   trimming with a visible "what was cut."
5. **Archetype detection** — app vs library vs constellation selects the artifact shape.

---

## 6. What this means for the surface (options, rethought)

Free to merge/relayout. The natural shape that falls out of the two artifacts:

- **One target input** (folder / repo / sln).
- **One derived choice:** pick an entry → **Trace**; pick none → **Map**. No "mode toggle" — the
  artifact is derived from whether an entry is selected. (Resolves the overview/deep-dive/profile
  vocabulary soup.)
- **Entry picker** autocompletes from the entry inventory: routes (`POST /api/orders`), consumers,
  public methods. (Replaces `--around` directory-proximity, which models the wrong thing, and the
  deprecated `--task` free-text/NLU, which the philosophy already rejects.)
- **Dials:** depth · detail · budget.
- **Later — filter as search:** a structured full-text filter over the unified model ("orders",
  "payment", "IOrderRepository") that returns matching entries/types and their traces. This is the
  honest version of "narrow it down" — search over a model, not pseudo-NLU.

### How this resolves the open design tensions

| Tension (from the feature assessment) | Resolution here |
|---|---|
| Intent/`--task` NLU in limbo | Removed; replaced by entry picker + later structured search |
| "Two situations, one dial" half-realized | Realized: Map vs Trace **derived** from entry presence |
| `--around` = directory proximity (wrong model) | Focus = entry-rooted **graph slice**, not folder hops |
| Section model leaks (12 toggles, focus-gated source) | Artifact-first; "sections" shrink to map facets + trace detail |
| Output is a catalog, not a narrative | The Trace **is** the narrative; catalogs become supporting detail |
| JSON second-class | Trace/Map are structured graphs → JSON is a first-class serialization of the same model |

---

## 7. Validation probe (gates semantic resolution + persistent index)

The engine (Parts A–E) now produces real Map + Trace output — syntactic resolution, all
indirection seams bridged (send · di · domain-event · integration-event), `Calls` marked
`[approx]`. The question is: **does the syntactic trace meaningfully help an LLM vs raw files
vs the old catalog**, or do we need semantic precision (Part F) before it's useful?

The probe (defined in `docs/dev/reports/probe-kit.md` after Part D is executed):

1. Run the engine on the target entry to produce the **real trace** (not the hand-built §2 target).
2. Give a fresh, context-free LLM session a realistic task (e.g. "add a per-line discount to orders")
   with **(a)** the engine trace, **(b)** raw source files, and **(c)** the legacy catalog output.
3. Compare answer quality on: correctness (does discount logic work end-to-end?), navigation speed
   (how fast does the LLM find the right files?), completeness (validation, persistence, events?),
   and explanation quality (does the LLM understand the MediatR flow?).

If the trace wins → fund Parts F (SemanticSymbolResolver) + G (Persistent Index).
If not → syntactic precision is insufficient; semantic resolution is prerequisite, or the approach
needs rethinking. Record the result in `docs/dev/reports/` and update this section.
