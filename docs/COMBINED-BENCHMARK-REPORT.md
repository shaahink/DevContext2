# DevContext v2 — Combined Benchmark Report

**Generated**: 2026-06-06  
**Branch**: develop  
**Tests**: 144 passing · 0 warnings · format clean  

---

## 1. Benchmark Matrix

| Repo | Type | Source | Signals Expected |
|---|---|---|---|
| **eShop** | Microservice (24 proj) | dotnet/eShop | controllers, minimal-apis, mediatr, efcore, fluentvalidation |
| **TodoApi** | Minimal API (7 proj) | dotnet/TodoApi | minimal-apis, efcore |
| **VerticalSlice** | FastEndpoints (22 proj) | ardalis/CleanArchitecture | minimal-apis, fluentvalidation, fast-endpoints, efcore |
| **AutoMapper** | Library (6 proj) | AutoMapper/AutoMapper | automapper |
| **AspireShop** | Aspire app (5 proj) | dotnet/aspire-samples | minimal-apis, efcore, aspire |
| **BlazorSignalR** | Blazor + SignalR | dotnet/blazor-samples | minimal-apis |

---

## 2. Results Per Repo

### eShop (Reference Microservice)

**Command**: `--profile focused --max-tokens 20000`

| Metric | Value |
|---|---|
| Runtime | 4.1s |
| Types found | 517 |
| Types in output | 28 (95% pruned) |
| Detections | 183 |
| Endpoints | 30 HTTP + 14 controller |
| MediatR handlers | 18 |
| DI registrations | 121 |
| Signals | controllers, minimal-apis, mediatr, efcore, fluentvalidation |
| Architecture | MinimalApi (100%) |

**Scenarios tested**: architecture (28 types), debug-endpoint (20 types), trace-message-flow (MediatR + Data model sections)

**Key output features**: Per-project endpoint grouping (Catalog.API 16 end., Identity.API 14, Ordering.API 7, Webhooks.API 4, WebhookClient 2), Data model section with EF entities, middleware dedup with counts, compact DI table with Source attribution.

### TodoApi (Minimal API Reference)

**Command**: `--scenario debug-endpoint --around TodoApi --profile focused --max-tokens 20000`

| Metric | Value |
|---|---|
| Runtime | 1.8s |
| Types found | 43 |
| Types in output | 20 (MaxSurviving=20 enforced) |
| Endpoints | 11 |
| Entry points | TodoApi class shown with namespace, deps, methods |
| Signals | minimal-apis, efcore |

### VerticalSlice (FastEndpoints)

**Command**: `--profile focused --max-tokens 20000`

| Metric | Value |
|---|---|
| Runtime | 1.7s |
| Types found | 381 |
| Types in output | 25 |
| Endpoints | 23 (FastEndpoints) |
| Signals | minimal-apis, efcore, fluentvalidation, fast-endpoints (100%) |
| Architecture | MinimalApi (80%) |

### AutoMapper (Library Mode)

**Command**: `--profile focused --max-tokens 20000`

| Metric | Value |
|---|---|
| Runtime | 2.5s |
| Types found | 2713 |
| Types in output | 167 (94% pruned) |
| Detections | 0 (library mode) |
| Signals | automapper (90%) — via ProjectReference |
| Output format | Compact namespace summary (34 namespaces, 34 public types listed) |

### AspireShop (Aspire + Minimal APIs)

**Command**: `--profile focused --max-tokens 20000`

| Metric | Value |
|---|---|
| Runtime | 0.43s |
| Types found | 22 |
| Types in output | 22 |
| Detections | 38 |
| Signals | minimal-apis (100%), efcore (100%) |

### BlazorSignalR (Blazor + SignalR)

**Command**: `--profile focused --max-tokens 20000`

| Metric | Value |
|---|---|
| Runtime | 0.36s |
| Types found | 1 |
| Types in output | 1 |
| Detections | 12 |
| Signals | minimal-apis (80%) |
| Output format | All sections rendered (RequiredSections enforcement) |

---

## 3. Feature Coverage Matrix

| Feature | eShop | TodoApi | VerticalSlice | AutoMapper | AspireShop | BlazorSignalR |
|---|---|---|---|---|---|---|
| Endpoint detection | ✅ 30+14 | ✅ 11 | ✅ 23 FE | N/A | ✅ minimal | N/A |
| MediatR handlers | ✅ 18 | ✅ 0 | ✅ 0 | N/A | ✅ 0 | N/A |
| Per-project grouping | ✅ 6 groups | ✅ 3 groups | ✅ 3 groups | N/A | ✅ 2 groups | N/A |
| Auth attributes | ✅ (on controllers) | ✅ (method-level) | ✅ | N/A | ✅ | N/A |
| Entry points (--around) | ✅ | ✅ | ✅ | N/A | ✅ | N/A |
| Data model (EF) | ✅ 7 DbContexts | N/A | N/A | N/A | ✅ | N/A |
| Middleware dedup | ✅ grouped | ✅ 6 items | ✅ | N/A | ✅ | ✅ |
| Library mode | N/A | N/A | N/A | ✅ compact | N/A | N/A |
| RequiredSections | ✅ enforced | ✅ | ✅ | ✅ | ✅ | ✅ |
| MaxSurvivingTypes | ✅ 30 | ✅ 20 | ✅ 25 | N/A | ✅ 30 | ✅ 20 |
| Profile warning | ✅ | ✅ | N/A | N/A | N/A | N/A |
| --around hint | ✅ | ✅ | ✅ | N/A | ✅ | N/A |

---

## 4. Signal Detection Summary

| Signal | eShop | TodoApi | Vertical | AutoMapper | AspireShop | Blazor |
|---|---|---|---|---|---|---|
| minimal-apis | ✅ 100% | ✅ 100% | ✅ 80% | — | ✅ 100% | ✅ 80% |
| controllers | ✅ 90% | — | — | — | — | — |
| mediatr | ✅ 100% | — | — | — | — | — |
| efcore | ✅ 100% | ✅ 100% | ✅ 100% | — | ✅ 100% | — |
| fluentvalidation | ✅ 100% | — | ✅ 100% | — | — | — |
| fast-endpoints | — | — | ✅ 100% | — | — | — |
| automapper | — | — | — | ✅ 90% | — | — |

**Strongest profile**: eShop (5 simultaneous signals, 183 detections, 44 endpoints)
**Library mode**: AutoMapper (0 endpoints, compact namespace summary)

---

## 5. Performance Benchmarks

| Repo | Runtime | Types/s | Bottleneck |
|---|---|---|---|
| eShop | 4.1s | 126 | SyntaxStructure + DiRegistration (parallel) |
| TodoApi | 0.8s | 54 | Fast pipeline |
| VerticalSlice | 1.7s | 224 | SyntaxStructure + DiRegistration |
| AutoMapper | 2.5s | 1085 | Shared cache reduces walk time |
| AspireShop | 0.43s | 51 | Small codebase |
| BlazorSignalR | 0.36s | 3 | Tiny codebase |

---

## 6. Key Findings

1. **eShop** is the most comprehensive test — covers 5 signals, 44 endpoints, 183 detections. All features (per-project grouping, EF section, middleware dedup, auth attributes) render correctly.

2. **AutoMapper library mode** works correctly — 0 detections, 94% pruned, namespace-summary instead of flat wall. automapper(90%) signal fires via ProjectReference detection.

3. **Per-project endpoint grouping** makes eShop output much more readable. `Catalog.API` (16 endpoints), `Identity.API` (14), `Ordering.API` (7), `Webhooks.API` (4), `WebhookClient` (2) — each grouped with project header.

4. **Scenario enforcement** working across all repos — `architecture` includes all sections, `debug-endpoint` (20 types), `trace-message-flow` (MediatR + Data model).

5. **3 bug fixes deployed during this iteration**: `_indent` crash fix, `MaxSurvivingTypes` enforcement, `Scenario.DisableExtractors` wiring, dead `_styleDetector` removal.
