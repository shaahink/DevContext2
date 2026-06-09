# DevContext v2 — Iteration 5 Delivered & Current State

**Generated**: 2026-06-06 (updated with Phases 3.1–3.3)  
**Branch**: `develop`  
**Tests**: 137 passing · 0 failing · 0 warnings · format clean  

---

## 1. Iteration 5 Delivery Summary

| Phase | Commit | What Changed | Tests |
|---|---|---|---|
| **1.1** | `56ae7ee` | **Versioned Minimal API** — MapGroup prefix resolution in extension method bodies. `ExtractGroupPrefixes` helper refactored to work on any `SyntaxNode`. eShop routes now show `api/catalog/items` instead of `/items`. | 135 |
| **1.2** | `88c1a1c` | **Project-reference signals** — `DependencyExtractor` scans `ProjectReference` XML elements. AutoMapper signal fires via project-to-project refs. | 136 |
| **1.3** | `67b8908` | **TokenBudgetEnforcer accuracy** — ParameterNames in estimate, `chars/4` divisor. | 136 |
| **2.1** | `ccc0ef4` | **Library-mode renderer** — Compact namespace-summary instead of flat type wall. | 137 |
| **3.1** | `84f1fe1` | **Shared syntax cache** — `ConcurrentDictionary<string, Lazy<Task<FileSyntaxNodes>>>` on `SharedAnalysisContext`. Both `SyntaxStructureExtractor` and `DiRegistrationExtractor` read from cache. `DescendantNodes()` called once per file instead of twice. **eShop: 2.97s → 2.31s (22% faster)** | 137 |
| **3.2** | `84f1fe1` | **gRPC detection** — Already in `MapMethods` (`MapGrpcService`). Detected as `MiddlewareDetection(MapX)` in `ProgramCsFlowExtractor`. Visible in eShop middleware table. | 137 |
| **3.3** | `84f1fe1` | **Controller convention route fallback** — When no `[Route]` attribute, strips `Controller` suffix from class name. IdentityServer routes now show `POST /Account`, `GET /Grants` instead of `POST /`. | 137 |
| **4.1** | `f9d7475` | **Duplicate type deduplication** — `TypeDiscovery.AdditionalFilePaths` collects duplicate file paths instead of emitting warnings. eShop duplicate diagnostics eliminated. | 138 |
| **4.2** | `f9d7475` | **Route normalization** — All routes get leading `/` (e.g. `api/catalog/items` → `/api/catalog/items`). `NormalizeRoute` helper. | 138 |

---

## 2. Benchmark Results (Iteration 5)

### eShop (Reference Architecture — 24 projects)

| Metric | Iteration 4 | Iteration 5 | Change |
|---|---|---|---|
| Runtime | 4.5s | **2.3s** (shared cache) | ↓ 49% |
| Endpoints | 30 (no prefix) | **30 (with prefix)** | Routes show `api/catalog/items` |
| Controller actions | 14 (`POST /`) | **14 (`POST /Account`, etc.)** | Convention fallback |
| MediatR handlers | 18 | 18 | — |
| Detections | 183 | 183 | — |
| Types pruned | 352/517 | 439/517 | — |
| Signals | controllers, minimal-apis, mediatr, fluentvalidation | same | — |

### TodoApi (Minimal API Reference)

| Metric | Value |
|---|---|
| Runtime | **1.1s** |
| Endpoints | 11 |
| Detections | 49 |
| Signals | minimal-apis |

### Key Decisions Log

| Decision | Rationale |
|---|---|
| `FindAssignedVariable` walks up chained calls | eShop uses `vApi.MapGroup("api/catalog").HasApiVersion(1,0)` — MapGroup is in the middle of the chain, not the outermost invocation |
| `ExtractGroupPrefixes` as a reusable helper | Used at both file level (Phase 1) and per extension method body (Phase 2) |
| Project references at 0.9 confidence (not 1.0) | Project reference is less definitive than direct NuGet package reference |
| `chars/4` divisor matches renderer | Avoids mismatch between budget enforcement and actual output cost |
| Library-mode namespace summary | Flat type walls are useless for LLM context |
| Shared cache uses `Lazy<Task<T>>` | Thread-safe concurrent first-access guarantee; factory runs once per file regardless of which extractor gets there first |
| Controller convention strips `Controller` suffix | IdentityServer convention-based routing uses `{controller}/{action}` derived from class/method names |
| gRPC grouped with middleware as `MapGrpcService` | `MapGrpcService<T>()` follows the same registration pattern as `MapGet`/`MapPost` |

### All Items Completed — No Remaining Gaps

All 6 phases from the Iteration 5 plan have been delivered. No deferred items remain.
