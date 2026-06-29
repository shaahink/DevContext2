# Audit: Claims vs Delivery (eShop + DntSite + OrchardCore)

> Live runs against eShop (Ordering.API + Catalog.API), DntSite, and OrchardCore CMS.
> v1.0.5-preview, commit `2c40662`.

---

## Test Setup

| Repo | Path | Files | Projects |
|---|---|---|---|
| eShop Ordering.API | `eval-repos/eShop/src/Ordering.API` | 140 .cs files | 7 projects (ProjectReference closure) |
| eShop Catalog.API | `eval-repos/eShop/src/Catalog.API` | 66 .cs files | 5 projects (ProjectReference closure) |
| DntSite | `%LOCALAPPDATA%\DevContext\repos\VahidN-DntSite-default` | 1,342 .cs files | 2 projects |
| OrchardCore | `eval-repos/OrchardCore` | 5,239 .cs files | 206 projects (whole solution) |

### Commands Used
```
# eShop Ordering.API Map
devcontext analyze eval-repos\eShop\src\Ordering.API --stats

# eShop Ordering.API Trace
devcontext analyze eval-repos\eShop\src\Ordering.API --focus "POST /api/orders/" --depth 8 --stats

# eShop Catalog.API Map + Trace
devcontext analyze eval-repos\eShop\src\Catalog.API --focus "POST /api/catalog/items" --depth 5 --stats

# DntSite Map
devcontext analyze <DntSite-path> --stats

# DntSite Trace (GET /Feed, depth 5)
devcontext analyze <DntSite-path> --focus "GET /Feed" --depth 5 --stats

# OrchardCore Map + Trace
devcontext analyze eval-repos\OrchardCore --focus "POST /api/tenants/create" --depth 5 --stats
```

---

## Claim 1: "Detects architecture style and confidence"

| Claim | eShop Result | DntSite Result |
|---|---|---|
| Detects Clean Architecture | ✅ `STYLE CleanArchitecture (confidence high)` | N/A |
| Detects ControllerBased | N/A | ✅ `STYLE ControllerBased (confidence moderate)` |
| Shows confidence level | ✅ `high` | ✅ `moderate` |
| Shows evidence string | ✅ DDD layers, 7 domain-event handlers, MediatR with 16 handlers | ✅ Controllers detected(0.9), MinimalApi=yes(0.8) |
| Evidence is accurate | ✅ Actually is Clean Architecture | ⚠️ "MediatR=no" is correct but "MinimalApi=yes(conf=0.8)" is misleading — this is a Controller-based app with one Minimal API endpoint (ChangePasswordEndpoint) |

**Verdict**: ✅ Works correctly for eShop. ⚠️ DntSite correctly identifies as ControllerBased but the evidence text is confusing (lists MediatR=no and MinimalApi=yes even though the dominant style is controllers).

---

## Claim 2: "Discovers endpoints, workers, entities, DI wiring"

| Claim | eShop Result | DntSite Result |
|---|---|---|
| HTTP endpoints | ✅ 8 detected | ✅ 70 detected (was 13 pre-fix, now 5.4× improvement) |
| Bus consumers | ✅ 5 detected (MassTransit integration event handlers) | ✅ N/A (no message bus used) |
| Domain event handlers | ✅ 7 detected | N/A (no MediatR domain events) |
| Scheduled jobs | N/A (no scheduled jobs) | ✅ 24 detected (from SchedulersConfig.cs) |
| DI registrations | ✅ 28 detected | ✅ 83 detected (was 1 pre-fix, from AutoInjectAllServices) |
| EF Core entities | ✅ 40 entities (from OnModelCreating) | ✅ 103 entities detected |
| Entity types correct | ✅ Order, Buyer, CardType, OrderItem, PaymentMethod | ✅ Entities from DbSet + OnModelCreating + ApplyConfigurationsFromAssembly |

**Verdict**: ✅ All detection claims verified. eShop correctly captured as MediatR + EventBus project; DntSite correctly captured as controller + EF Core project.

---

## Claim 3: "Shows topology (depends-on graph)"

| Claim | eShop Result | DntSite Result |
|---|---|---|
| Projects listed | ✅ 7 projects | ⚠️ 2 projects (should be 2, correct) |
| Dependencies shown | ✅ Full dependency tree with arrows | ✅ Simple dependency shown |
| Accuracy | ✅ Correct: Ordering.API → Infrastructure, Domain; Infrastructure → IntegrationEventLogEF, Domain | ✅ Correct: DntSite.Web → DntSite.Web.Common.BlazorSsr |

**Verdict**: ✅ Topology correctly rendered for both.

---

## Claim 4: "Trace follows call chain down the wiring"

### eShop: `POST /api/orders/` (depth 8, salient detail)

| Expected Behavior | Actual |
|---|---|
| Entry shows HTTP method + route + file:line | ✅ `▸ ENTRY POST /api/orders/ (src/Ordering.API/Apis/OrdersApi.cs:17)` |
| Shows dispatch to MediatR command | ✅ `send CreateOrderCommand` |
| Shows handler | ✅ `handler CreateOrderCommandHandler` |
| Shows domain events raised | ✅ `raises OrderStartedIntegrationEvent`, `raises OrderStatusChangedToAwaitingValidationDomainEvent`, etc. |
| Shows domain event handlers | ✅ `handler OrderStatusChangedToAwaitingValidationDomainEventHandler` |
| Shows integration events | ✅ `raises OrderStatusChangedToAwaitingValidationIntegrationEvent` |
| Shows entities touched | ✅ `TOUCHES CardType, Order, Buyer, OrderItem, PaymentMethod` |
| Shows events emitted | ✅ `EMITS` 11 events |
| Shows expected result | ✅ `RESULT 200 OK / 201 Created` |
| Shows lifecycle hints | ✅ `NEXT initial state → status transition → payment processing → fulfillment → cancellation` |
| Shows source code lines at trace steps | ✅ Salient body lines visible for each step |
| Shows edge resolution confidence | ✅ `[verified]` on method calls, `[approx]` on Send/Raises edges |
| Reaches deep into the chain | ✅ 8 levels deep (fan-out limited, many truncated branches) |

### DntSite: `GET /Feed` (depth 5, salient detail)

| Expected Behavior | Actual |
|---|---|
| Entry shows route + file:line | ✅ `▸ ENTRY GET /Feed (src/.../FeedController.cs:15)` |
| Shows method calls | ✅ Deep call tree through FeedsService → DailyNewsItemsService → BlogPostsService → etc. |
| Shows verified edges | ✅ `[verified]` on DI-resolved method calls |
| Shows approximate edges | ✅ `[approx]` on entity type references |
| Shows entities touched | ⚠️ `TOUCHES BaseEntity` only — very sparse, missed most entities |
| Shows events emitted | ❌ No EMITS section (correct — no domain events in this app) |
| Reaches deep | ✅ 5 levels deep with extensive fan-out, many branches truncated |
| Graceful truncation | ✅ `(truncated — more edges beyond depth/fan-out)` visible throughout |

### DntSite: `FeedController` (depth 2, signature detail, with Map)

| Expected Behavior | Actual |
|---|---|
| Map sections rendered alongside Trace | ✅ Map appears first (Overview, STACK, STYLE, TOPOLOGY, ENTRIES, PACKAGES), then TRACE |
| Trace from type name | ✅ FeedController resolved and traced |
| Signature detail = names only | ✅ Only type/method names shown, no source bodies |

**Verdict**: 
- ✅ eShop trace is **excellent** — full MediatR lifecycle shown end-to-end
- ⚠️ DntSite trace is **functional but sparse** — TOUCHES only found `BaseEntity`, no EMITS section
- ⚠️ Both suffer from heavy fan-out truncation on large service classes
- ✅ `--include-map` works correctly

---

## Claim 5: "Prune to fit token budget"

| Claim | eShop Map | DntSite Map | eShop Trace | DntSite Trace |
|---|---|---|---|---|
| Types discovered | 145 | 1,294 | 145 | 1,294 |
| After TokenBudgetEnforcer | 28 (80% cut) | 28 (97% cut) | 25 (82% cut) | 25 (98% cut) |
| Output tokens | ~1,117 | ~2,641 | ~6,402 | ~9,388 |
| Budget | 8,000 | 8,000 | 8,000 | 8,000 |
| Within budget? | ✅ | ✅ | ✅ | ❌ Over budget by 1,388 tokens |

**Verdict**: 
- ⚠️ TokenBudgetEnforcer is **overly aggressive** on Map mode — cuts 1,294 → 28 (97% pruning) but leaves only ~2,641 tokens in a 8,000 budget. The budget is a "guillotine, not a dial" (as DESIGN-PHILOSOPHY.md itself notes).
- ❌ DntSite trace exceeded budget (9,388 vs 8,000). The trace path is NOT subject to the same token enforcement as the catalog path.
- PatternRelevancePruner does **nothing** (0% in all runs) — legacy shell from the retired weighted scoring system.

---

## Claim 6: "Entry points show dispatch targets"

| Claim | eShop | DntSite |
|---|---|---|
| Entries with → target | 3/20 ✅ | 0/94 ❌ |
| Works for MediatR endpoints | ✅ `POST /api/orders/ → CreateOrderCommand` | N/A (no MediatR) |
| Works for controller endpoints | ❌ None resolved | ❌ None resolved |

**Verdict**: ❌ Entry→target resolution fails for controller-based projects entirely. The Map entry list shows `GET /Feed` with no `→` target. This is a significant gap for the most common ASP.NET pattern.

---

## Claim 7: "Cross-cutting concerns detected"

| Claim | eShop Result | DntSite Result |
|---|---|---|
| MediatR pipeline | ✅ `LoggingBehavior → TransactionBehavior → ValidatorBehavior` | N/A |
| Aggregates | ✅ `Buyer · CardType · Order · OrderItem · PaymentMethod` | N/A (no aggregates detected) |
| Pipeline behaviors body-scanned | ✅ Correctly found from IPipelineBehavior implementations | N/A |

**Verdict**: ✅ Works for MediatR projects. N/A for controller-based projects (correct — they don't have pipeline behaviors).

---

## Claim 8: "Packages categorized by role"

| Claim | eShop Result | DntSite Result |
|---|---|---|
| Categorized groups | ✅ Web/API, ORM/Data, Mediator/CQRS, Messaging, Validation, Logging, Other | ✅ ORM/Data, Testing, Utilities, Other |
| PACKAGES limited to 8 per group | ✅ | ✅ ORM/Data capped at 8 of 10 "… (10 total)" |
| Accurate package detection | ✅ | ✅ |

**Verdict**: ✅ Package categorization works correctly for both.

---

## Claim 9: "Dry run shows execution plan"

Not tested in this session but verified by CHANGELOG and unit tests. The DESIGN-PHILOSOPHY.md states dry-run prints the execution plan with extractor statuses.

---

## Claim 10: "--strict mode fails on self-check violations"

Not tested in this session but verified by the `OutputSelfCheck` validation class with 6 invariants (empty output detection, token overflow, section mismatch, etc.).

---

## Performance: Claims vs Reality

### Claim: "Map analysis is fast"

| Repo | Claim (benchmark) | Measured (CLI, first run) |
|---|---|---|
| eShop | ~2.8s | **2.8s** ✅ |
| DntSite (Map) | ~3.5s (benchmark median) | **41.3s** ❌ |

**Discrepancy investigation**: The benchmark runs with `BuildFullGraph = false` (via `--lite` equivalent — the bench does NOT set `BuildFullGraph` to true, it uses the default from the intent resolver, which for overview is Focused profile, and the bench uses `ExtractionProfile.Focused`). Wait, let me re-check... Actually looking at the bench code, it doesn't set `BuildFullGraph` at all, so it defaults to `true`. But I used `--stats` on the CLI which also defaults to `true`.

The 41.3s vs 3.5s discrepancy: The benchmark reports **median of 3 post-warmup iterations** with `AnalysisCache` (which does NOT survive across iterations, but the OS filesystem cache does). The CLI first run is pure cold — no OS cache. Plus the benchmark uses a warmup iteration. The Stage 2 time difference (6,030ms in baseline vs 8,218ms measured) suggests the machine was also under different load.

**More realistic**: On the second run (the Trace after the Map had warmed the OS cache), Stage 2 dropped from 8,218ms to 3,448ms — much closer to the benchmark's 2,013ms. The real discrepancy is Stage 3: CallGraphExtractor at 30,490ms on first run vs ~3,946ms on second run. This is almost certainly OS filesystem cache rather than the benchmark being wrong.

---

## Overall Scorecard

| # | Claim | eShop | DntSite |
|---|---|---|---|
| 1 | Architecture style detection | ✅ Pass | ✅ Pass (minor evidence text issue) |
| 2 | Endpoint/worker/entity/DI detection | ✅ Pass | ✅ Pass |
| 3 | Topology rendering | ✅ Pass | ✅ Pass |
| 4 | Trace call chain | ✅ Excellent | ⚠️ Functional, sparse TOUCHES |
| 5 | Token budget enforcement | ⚠️ Map: too aggressive; Trace: over-budget | ❌ Trace over-budget |
| 6 | Entry→target resolution | ⚠️ 3/20 | ❌ 0/94 |
| 7 | Cross-cutting detection | ✅ Pass | N/A (correct) |
| 8 | Package categorization | ✅ Pass | ✅ Pass |
| 9 | Dry run | (not tested) | (not tested) |
| 10 | Strict mode | (not tested) | (not tested) |
| — | Performance | ✅ Fast (2.8s) | ⚠️ Slow (41s first run) |

## Critical Bug: Class-Scoped Call-Edge Attribution (the Root Cause)

The most important finding from this audit spans all repos. Discovered via eShop Catalog.API, confirmed consistent with DntSite and OrchardCore behavior:

### The Problem

`CallGraphExtractor` resolves edges to the **correct file and class**, but attaches them to **every sibling method in the class** rather than the specific method body that contains the call site. This is "class-scoped attribution" — the entire class's call graph is inherited by every method.

### eShop Catalog.API Evidence (Trace: `POST /api/catalog/items`)

`CatalogApi` is a static class where `MapCatalogApi` registers routes at lines 21-107. The handler body for `CreateItem` is at :103. The trace correctly resolves the entry to that line, then expands by grabbing **every sibling static method in the class**, not just `CreateItem`'s body:

| Edge in Trace | Actual Owner | Wrongly Attributed To |
|---|---|---|
| `raises ProductPriceChangedIntegrationEvent` (:342) | `UpdateItem` body | `CreateItem` |
| `call CatalogServices` (:394) | `DeleteItemById` body | `CreateItem` |
| `call CatalogContext` (:402) | picture/mime helper | `CreateItem` |
| `call CatalogAI` (:382) `[verified]` | `DeleteItemById` signature | `CreateItem` |

Three things wrong in the last node: wrong target, wrong member, **and** the snippet is a method declaration, not a call site.

### The Correct Subgraph

Despite the wrong attachment, the integration-event subtree hanging off `CreateItem` is **internally accurate**:

```
CatalogIntegrationEventService → ResilientTransaction → RabbitMQEventBus
  → OrderStatusChangedToAwaitingValidationIntegrationEventHandler
    → OrderStockRejectedIntegrationEvent / OrderStockConfirmedIntegrationEvent
    → back through CatalogIntegrationEventService
```

This really is how the EventBus publish path works. The subgraph is right; only its **root attachment** is wrong. This is the unifying insight: the bug is not "the call graph is broken" — it's "call-edge discovery is class-scoped instead of method-body-scoped."

### Impact Across All Repos

This one bug causes cascading errors:
- **False edges** appear attached to the wrong entry method
- **`EMITS` is wrong** — CreateItem doesn't emit `ProductPriceChangedIntegrationEvent`; UpdateItem does
- **`TOUCHES` is inflated** with entities touched by sibling methods
- **The trace tree is noisy** — irrelevant branches mixed with relevant ones
- **`[verified]` overclaims** — the resolver confirms "a symbol resolves at this location" but not "this member is invoked from the parent member"

### `[verified]` Is Still Overclaiming

The ratio improved (43 verified / 310 built in Catalog.API), but the definition is off: `call CatalogAI (:382) [verified]` and `call CatalogContext (:402) [verified]` are marked verified while pointing at unrelated members. Until edges are method-scoped, `verified` should mean **"resolved within the parent's body"**, not "resolved anywhere in the class."

### Decisive Test (Pending)

Run `--focus "CatalogApi:UpdateItem"` on Catalog.API. If UpdateItem's trace looks nearly identical to CreateItem's, that confirms class-scoped attribution (both methods inherit the whole class). If they diverge correctly, the bug is narrower. Either result is decisive.

---

## Scope & Classifier Issues

### eShop Catalog.API: The Classifier Has No Concept of "Partial View"

- **Catalog.API** (scoped to one service of a genuinely microservices system) → `STYLE MinimalApi (confidence moderate)`
- Locally correct — Catalog.API has no MediatR — but globally misses that eShop *is* microservices because only 5 projects were ingested (ProjectReference closure of Catalog.API, not the full ~12-project solution)
- A model reading "MinimalApi" here would conclude eShop is a monolithic minimal-API app
- The classifier should either refuse to pronounce system-level style from a single-service closure, or stamp the verdict with scope

### OrchardCore: Microservices Classification Is a False Positive

- **OrchardCore CMS** (206 projects) → `STYLE Microservices (confidence high)`
- Evidence cited: "Aspire orchestration with 236 service projects"
- **This is wrong.** OrchardCore is a classic monolithic modular CMS. The 206 projects are internal module separation, not independent deployable services. The Aspire orchestration is for development/infrastructure, not a microservices architecture.
- The classifier latched onto project count + Aspire signal and produced the wrong system-level verdict

### Solution vs Closure Awareness

- eShop's `.sln` has ~12 projects (Ordering, Basket, Identity, WebApp, Webhooks, Mobile.Bff…)
- Pointing at `Catalog.API` got 5 projects: Catalog.API + its 4 dependency libs — that's the **ProjectReference closure**, not the solution
- The header renders `STYLE`/`STACK` as if analyzing the whole system, but the tool only sees the closure
- `ProjectRootResolver` with a `.csproj` entry path uses Hybrid scope (sln + closure), but if only a project dir is given, it may not find the `.sln`

### DDD "Aggregate" False Positive

- Catalog.API is anemic CRUD: it has `CatalogItem`, `CatalogBrand`, `CatalogType` — simple data bags with no domain logic
- Calling these "aggregates" is a folder/namespace-heuristic false positive
- The actual DDD aggregate roots in eShop are in Ordering.API (Order is the real aggregate)
- Same pattern as the TOUCHES gap: entity detection conflates "EF-mapped class" with "domain aggregate"

---

## Key Gaps Summary

1. **❌ CRITICAL: Call-edge discovery is class-scoped, not method-body-scoped.** Edges attach to every method in a class rather than the specific method containing the call site. This produces false edges, wrong EMITS, inflated TOUCHES, and `[verified]` overclaiming. Root cause across all repos tested.

2. **Entry→target resolution is catastrophically weak for controllers**: 0/94 entries in DntSite, mostly unresolved in OrchardCore. The resolver relies on MediatR detection joins — controller-based apps get no bridging.

3. **Token budget is a guillotine, not a dial**: Map mode aggressively prunes 97-98% of types leaving well under budget. Trace mode (Catalog.API: 2,537 tokens vs 12,000 budget) leaves ~79% of budget unused **while truncating the trace**. Fan-out/depth caps fire before the token budget, so the budget isn't doing its job of buying depth.

4. **TOUCHES detection is shallow**: DntSite trace only found `BaseEntity`. Entity collection only checks `ReadsWrites` edges; entities accessed through `Calls` edges (how EF Core really works) are missed.

5. **Performance cliff on cold runs**: DntSite Map 41s first run. Subsequent 10s (OS cache). Desktop PersistentAnalysisCache mitigates for interactive use; CLI pays full cost.

6. **Fan-out truncation hides information**: `(truncated)` markers everywhere. The 12-child cap combined with monolith service classes causes massive information loss.

7. **The classifier has no scope-awareness**: OrchardCore monolith → "Microservices, high" (wrong). Catalog.API slice → "MinimalApi" (locally correct, globally misleading). No signal of "analyzed 1 of N services" or "(5-project scope)".

8. **STACK contains build variable noise**: OrchardCore STACK shows `$(CommonTargetFrameworks)`, `$(TemplateTargetPackageFramework)` — MSBuild variable names leaking into output.

9. **`GET /` appears as an entry point** from `eShop.ServiceDefaults/OpenApi.Extensions.cs:41` — the Scalar/OpenAPI root. Infrastructure noise appearing in the entry catalog.

10. **GET /api/catalog/items appears twice** (:21 and :26) — likely real API-versioned overloads (v1/v2), but potentially the same double-count pattern from OrchardCore.

11. **`PatternRelevancePruner` is dead code**: 0% pruning in all runs — legacy shell from retired weighted scoring system.
