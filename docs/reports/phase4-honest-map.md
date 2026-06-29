# Phase 4 Capability Report — Honest Map & Detection

> 2026-06-28, Iteration 4 / Phase 4 (branch `feature/iter4-honest-map`).
> Covers the 4 honest-Map fixes + trace-noise polish.

## OrchardCore evidence

| Check | Result |
|---|---|
| **Style** | `ModularMonolith (high)` — **not Microservices**. Already correct on develop (requires Aspire AppHost, which OrchardCore lacks). |
| **STACK `$(` noise** | **GONE.** Before: `$(CommonTargetFrameworks), $(TemplateTargetPackageFramework)` in STACK, `$(TemplateOrchardPackageVersion)` in PACKAGES. After: `netstandard2.0 · Minimal APIs · Controllers`; packages show clean versions or name-only. |
| **Fix 1b (suppression)** | Not needed here (OrchardCore is a whole-solution analysis, 202 projects = sln-count → not partial). |

## Catalog.API evidence (the partial-closure gate)

| Check | Before | After |
|---|---|---|
| **Scope stamp** | none (`MAP eShop (5 projects)`) | `SCOPE 5-project closure of 24-project eShop — style/topology are local...` |
| **Aggregates** | `CatalogBrand · CatalogItem · CatalogType` | **None** — CRUD types dropped. `DDD aggregates` removed from STACK. |
| **STACK `$(`** | none (eShop uses clean TFMs) | still clean — `output-not-contains "$("` passes. |
| **Local style** | `MinimalApi (moderate)` | `MinimalApi (moderate)` — the scope stamp makes it honest now. |

## eShop whole-solution (regression guard)

| Check | Result |
|---|---|
| **Style** | `Microservices (high)` — **preserved.** The partial-closure suppression correctly did NOT fire (19 analysed projects ≥ 24*3/4=18 threshold). |
| **Aggregates** | Was `Buyer · CardType · Order · OrderItem · PaymentMethod` → Now **`Buyer · Order`** only. Genuine aggregates (those implementing `IAggregateRoot`) kept; child entities dropped. Fix 4. |
| **STACK `$(`** | `output-not-contains "$("` passes. |

## DntSite TOUCHES diagnosis (deferred item)

**Finding:** 115 EfEntityDetections (entities ARE detected). The `GET /Feed/News` trace touches types like
`BlogPost`, `DailyNewsItem` via method calls (e.g. `call BlogPost.MapToNewsWhatsNewItemModel`). The
TOUCHES gap is that these call-edge callees' owning Type nodes resolve to a DIFFERENT FQN than the
EfEntityDetection-created entity Type nodes — the entity nodes are tagged entity, but `ResolveEntityNode`
(the Iteration-3 Calls→entity resolution) looks up the callee's owning Type by the call-graph FQN,
which doesn't match the short-name-resolved entity node key. **Root cause: FQN-canonicalization mismatch**
between detection-resolution and call-graph resolution — a focused FQN-canonicalization fix is needed,
but it risks destabilising the graph joins that all iterations depend on. **Tracked as a follow-up; the
mechanism (unified FQN resolution in AddEntityNodes / NameResolver) is the natural Iteration-5 enhancement.**

## Trace-noise polish (folded in)

| Before | After |
|---|---|
| `call ProductsController.NotFound`, `.Ok`, `CreatedAtAction` in controller traces | **Gone** — self-call noise filter in `AddCallEdges` (known ASP.NET result helpers + `nameof`). |
| `call Buyer.nameof`, `call IntegrationEventLogService.nameof` in any trace | **Gone** — `nameof` in the noise set. |

## Gate status

`pwsh -File eval/gates.ps1` **PASS** (catalog.json + eshop.json ratchet + ArchitectureStyleDetectorTests partial-closure tests + TraceQualityTests noise assertions all green).
