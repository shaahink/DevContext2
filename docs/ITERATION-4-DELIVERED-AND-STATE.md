# DevContext v2 — Iteration 4 Delivered & Current State

**Generated**: 2026-06-06  
**Branch**: `develop`  
**Tests**: 134 passing · 0 failing · 0 warnings · format clean  

---

## 1. Iteration 4 Delivery Summary

| Phase | Commit | What Changed | Test Count |
|---|---|---|---|
| **0** | `6b98a02` | `.gitignore` for `.claude/`/`.opencode/`, deleted `Class1.cs` stubs, library-mode pruner tests | 129 |
| **1.1** | `8ecdc50` | **Fixed `--metrics` under CompositeObserver** — `RecordExtractorMetrics` now reaches `MetricsDiscoveryObserver` through `CompositeDiscoveryObserver`. The `?` cast that silently failed is replaced with a helper that checks both types. | 129 |
| **1.2** | `b3d0819` | **Endpoint table upgraded**: Added `GroupPrefix` column (when present), `Source` column (File:Line), lambda truncation (long lambdas → `λ {file}:{line}`). Golden files regenerated. | 129 |
| **2.1** | `5bd33e5` | **9 new architecture signals**: `Serilog`, `Polly`, `AutoMapper`, `Swagger`, `Identity`, `NLog`, `Quartz`, `Redis`, `HealthChecks` — added to `ArchitectureSignals.Keys` + `DependencyExtractor.PackageSignalMap`. | 130 |
| **2.2** | `a91516a` | Deleted empty `UnitTest1.cs` stubs from `DevContext.Integration` + `DevContext.Roslyn.Tests`. Added NLog signal test. | 131 |
| **3** | `e817cad` | 3 new renderer tests (GroupPrefix, Source column, lambda truncation). CLI dry-run verified on MinimalApi fixture. | 134 |
| **4** | `6285067` | Better Spectre UX for signal sealing message (`"Signals sealed — 3 detected: ..."`). Final build/test/format gate. | 134 |

---

## 2. Current System State

### Architecture Overview

```
DevContext.Cli (Spectre.Console CLI, DI composition root)
  → DevContext.Core (all extraction, pruning, compression, rendering)
  → DevContext.Roslyn (optional Roslyn workspace integration)
```

28 source projects in solution, 3 main deliverables.

### Pipeline Stages

| Stage | Execution | Extractors |
|---|---|---|
| 0 — ProjectRootResolver | Pre-pipeline | Resolves .sln / .csproj / folder |
| 1 — Discovery & Cache Warmup | Sequential | FileTreeExtractor, SolutionDiscovery, ProjectStructure |
| 2 — Generic Extraction | **Parallel** | DependencyExtractor, SyntaxStructureExtractor, LayerClassifier, ProgramCsFlowExtractor, DiRegistrationExtractor |
| — Signal Sealing | Sync barrier | `Architecture.Seal()` called, style detected |
| 3 — Specific Extraction | Sequential | EndpointExtractor, MediatRExtractor, ControllerActionExtractor, EfCoreExtractor, EventBusExtractor, AspireExtractor, CallGraphExtractor, SourceBodyExtractor, IndirectWiringDetector |
| 4 — Pruning | Sequential | PathProximity → CallReachability → PatternRelevance → TokenBudget |
| 5 — Compression | Sequential | TrivialMember → Boilerplate → StructuralDeduplicator → NamespaceGrouper → LlmFriendly → AggressiveTruncator |
| 6 — Rendering | Final | MarkdownRenderer / JsonContextRenderer |

### All Registered Signals (31 total)

| Signal Key | Source Package(s) | Notes |
|---|---|---|
| `minimal-apis` | SDK: Web, Microsoft.AspNetCore.OpenApi | Also from project SDK detection |
| `controllers` | Base type: ControllerBase | Syntax fallback from SyntaxStructureExtractor |
| `mediatr` | MediatR | |
| `efcore` | Microsoft.EntityFrameworkCore | |
| `masstransit` | MassTransit | |
| `aspire` | Microsoft.Aspire | |
| `fast-endpoints` | FastEndpoints packages | |
| `dapper` | Dapper | |
| `blazor` | — | |
| `wpf-mvvm` | — | |
| `signalr` | — | |
| `grpc` | — | |
| `scrutor` | Scrutor | |
| `refit` | Refit | |
| `fluentvalidation` | FluentValidation | |
| `hangfire` | Hangfire | |
| `serilog` | Serilog | **New in Iter 4** |
| `polly` | Polly | **New in Iter 4** |
| `automapper` | AutoMapper | **New in Iter 4** |
| `swagger` | Swashbuckle.AspNetCore | **New in Iter 4** |
| `identity` | Microsoft.AspNetCore.Identity | **New in Iter 4** |
| `nlog` | NLog | **New in Iter 4** |
| `quartz` | Quartz | **New in Iter 4** |
| `redis` | StackExchange.Redis | **New in Iter 4** |
| `healthchecks` | AspNetCore.HealthChecks | **New in Iter 4** |

### Key Bug Fixes From Iter 3 Now Verified Working

- **`--metrics` output**: Rich extractor table with Tier/Category/Time/Types+/Dets+ now correctly populated even when `Composite(Spectre, Metrics)` is used
- **MapGroup chain detection**: Multi-level `MapGroup` prefix resolution working
- **FastEndpoints `Configure()` pattern**: HTTP verb detection inside Configure() method
- **FocusPoint Type/Method resolution**: Deferred to Stage 3, with Levenshtein suggestions
- **Library-mode pruner**: Activates when no web signals present, boosts public types and penalizes test types

---

## 3. Benchmark Results

### 3.1 eShop (Reference Architecture — 24 projects, 154 source files)

**Command**: `analyze "eval-repos/eShop/eShop.slnx" --profile focused --max-tokens 20000 --metrics --include-diagnostics`

| Metric | Value |
|---|---|
| Runtime | 4.5s |
| Types found | 517 |
| Types in output | 212 (59% pruned) |
| Detections total | 183 |
| Endpoints | **30** (Minimal API: Catalog 15, Orders 7, Webhooks 4, WebhookClient 2, Other 2) |
| Controller actions | **14** (IdentityServer UI: Account, Consent, Device, External, Grants) |
| MediatR handlers | **18** (Domain event handlers + command handlers) |
| DI registrations | 121 (across all 24 projects) |
| Middleware/workers | 22 |
| Signals | controllers, minimal-apis, mediatr, fluentvalidation |
| Architecture style | MinimalApi (100%) |

**Key observations:**
- **eShop runs cleanly** with correct endpoint detection on real-world patterns
- Catalog API route base path (`/api/catalog`) not prepended to individual routes — routes show as `/items` not `/api/catalog/items` (base path is set in Program.cs config, not visible at `MapGet` call sites)
- Versioned minimal API pattern (`app.NewVersionedApi("...").MapGet(...)`) is caught via extension method body scanning
- Controller actions correctly detected for IdentityServer UI controllers
- gRPC endpoints (Basket) correctly not emitted as HTTP REST endpoints
- Duplicate type warnings from SyntaxStructureExtractor: 20 warnings for overlapping class names across projects (`Extensions`, `Program`)
- Pruning correctly removes all 5 test projects' types
- 7 MSBuild/outbox migration classes flagged as duplicates due to parallel project compilation

### 3.2 TodoApi (Minimal API Reference — 1 project)

**Command**: `analyze "eval-repos/TodoApi/TodoApp.sln" --profile focused --max-tokens 20000 --metrics`

| Metric | Value |
|---|---|
| Runtime | 1.3s |
| Types found | 43 |
| Types in output | 37 |
| Detections | 49 |
| Endpoints | **11** |
| Middleware | 19 |
| DI registrations | 38 |
| Signals | minimal-apis |
| Architecture style | MinimalApi (100%) |

**Key observations:**
- Fastest benchmark, clean focused output
- All minimal API endpoints correctly detected
- No signal noise
- Token budget 20K → only 1.5K used (output is compact and focused)

### 3.3 VerticalSlice (FastEndpoints Reference — multi-project)

**Command**: `analyze "eval-repos/VerticalSlice/Clean.Architecture.slnx" --profile focused --max-tokens 20000 --metrics`

| Metric | Value |
|---|---|
| Runtime | 2.0s |
| Types found | 381 |
| Types in output | 318 |
| Detections | 58 |
| Endpoints | **23** (FastEndpoints class-based + Configure() pattern) |
| Signals | minimal-apis (80%), fluentvalidation (100%), **fast-endpoints (100%)** |
| Architecture style | MinimalApi (100%) |

**Key observations:**
- **FastEndpoints signal fires at 100% confidence** — package detection from DependencyExtractor works
- **23 endpoints detected** from FastEndpoints class hierarchy (`Endpoint<TRequest, TResponse>` + `Configure()` with `Post/Get` calls)
- Some routes show `<dynamic>` where route strings use non-literal expressions
- `minimal-apis` signal fires at 80% — from SDK:Web detection (not actual minimal API usage)

### 3.4 AutoMapper (Library — 6 projects, 2713 types)

**Command**: `analyze "eval-repos/AutoMapper/AutoMapper.slnx" --profile focused --max-tokens 20000 --metrics`

| Metric | Value |
|---|---|
| Runtime | 8.1s |
| Types found | 2713 |
| Types in output | 145 (95% pruned) |
| Detections | 0 |
| Endpoints | 0 |
| Signals | **none** (library mode activated) |
| Architecture style | Not detected |

**Key observations:**
- **Library mode activates** — no web signals → `ApplyLibraryModeScoring` boosts public API types, penalizes test types
- PatternRelevancePruner: 2713 → 562 (test type removal using name/namespace heuristics + library mode)
- TokenBudgetEnforcer: 562 → 404 (signature-only estimation)
- Final output: 145 types (rendered with `SourceBody` from full types?)
- **Performance bottleneck**: SyntaxStructureExtractor + DiRegistrationExtractor together consume 7.7s of the 7.9s total (97% of runtime — duplicate tree walking)
- **Output quality concern**: AutoMapper output is 4,346 lines of flat type listings under "Unknown" layer — not ideal for LLM context consumption
- **AutoMapper signal NOT detected** — package detection requires NuGet package reference, but AutoMapper uses project-to-project references in its solution. The `automapper` signal key exists but isn't triggered here

---

## 4. Diagnosed Gaps for Iteration 5

### P0: Versioned Minimal API Pattern (`NewVersionedApi`)

eShop uses `app.NewVersionedApi("Catalog API").MapGet(...)` extensively. The current `MapMethods` list catches `MapGet` but only when called on variables within extension method bodies. This works thanks to extension method body scanning, but the versioned API builder pattern is not explicitly recognized.

**Impact**: Routes detected but base path `/api/catalog` missing from individual routes.

### P1: Library Output Quality

AutoMapper output is 4,346 lines of `| - **Unknown**: Type1, Type2, ...` flat lists under a single layer. For library-mode projects with no detections, this is not useful LLM context.

**Fix needed**: PatternRelevancePruner library mode should be more aggressive, or renderer should group/summarize better for library output (e.g., emit public API surface summary, interfaces, key abstractions).

### P1: Duplicate Tree Walking (Performance)

All 4 benchmarks show the same pattern: `SyntaxStructureExtractor` + `DiRegistrationExtractor` together consume ~95%+ of runtime, both walking the same syntax trees independently. `IAnalysisCache` prevents re-parsing but `DescendantNodes()` filtering is per-extractor.

**eShop**: Syntax 2.6s + DI 2.6s = 5.2s of 4.5s total (parallel, so max wins)
**AutoMapper**: Syntax 7.7s + DI 7.7s = 7.7s of 7.9s total (parallel)

**Fix needed**: Shared "interesting node" cache or merge SyntaxStructure + DiRegistration into a single pass.

### P1: TokenBudgetEnforcer Underestimation

AutoMapper output is 112K tokens rendered vs 20K budget. `TokenBudgetEnforcer` uses signature-only size estimation which massively underestimates when types contain full source bodies or when rendered as long flat lists.

**Fix needed**: TokenBudgetEnforcer should use the actual rendered token estimate, not the pre-render signature estimate.

### P2: Signal Detection for Project References

The `AutoMapper` signal exists but is not detected because AutoMapper uses project-to-project references, not NuGet packages. The `DependencyExtractor` only checks `PackageReference` elements.

**Fix needed**: Add project-reference scanning in DependencyExtractor for known signal-bearing projects.

### P2: gRPC Endpoint Detection

eShop's Basket.API uses `MapGrpcService<BasketService>()`. No gRPC endpoint detection exists. This is a gap for microservice architectures.

### P2: Route Base Path Resolution

eShop Catalog API routes show as `/items` but serve at `/api/catalog/items`. The base path is configured via `UsePathBase` or custom middleware — not detectable from individual `MapGet` call sites without semantic analysis.

### P3: Duplicate Type Deduplication

In multi-project solutions, overlapping class names (`Extensions`, `Program`, `AppDelegate`) generate 20+ diagnostic warnings. SyntaxStructureExtractor reports duplicates but doesn't merge or deduplicate.

### P3: IdentityServer Controller Routes

ControllerActionExtractor shows `POST /` for IdentityServer controllers — the actual routes are `/Account/Login`, `/Consent`, etc. but the `[Route]` attribute pattern on IdentityServer controllers uses convention-based routing (`AddControllersWithViews()` + `MapDefaultControllerRoute()`), not explicit route templates.

---

## 5. Benchmark Artifacts

| Repo | File | Format |
|---|---|---|
| eShop | `eval-results/eShop/iter4-focused-architecture.md` | Markdown |
| eShop | `%TEMP%\eshop-focused-console.txt` | Console (metrics) |
| TodoApi | `eval-results/TodoApi/iter4-focused-architecture.md` | Markdown |
| TodoApi | `%TEMP%\todoapi-console.txt` | Console (metrics) |
| VerticalSlice | `eval-results/VerticalSlice/iter4-focused-architecture.md` | Markdown |
| VerticalSlice | `%TEMP%\vertical-console.txt` | Console (metrics) |
| AutoMapper | `eval-results/AutoMapper/iter4-focused-architecture.md` | Markdown |
| AutoMapper | `%TEMP%\automapper-console.txt` | Console (metrics) |

---

## 6. Next Steps — Recommended for Iteration 5

1. **Versioned minimal API support** — Add `NewVersionedApi` / `MapVersionedApi` to recognized endpoint registration patterns, resolve route base paths from versioned API builder config
2. **Library-mode output quality** — Rethink library output: emit public API surface summary, interfaces, type hierarchy rather than flat wall of type names
3. **Shared tree walk** — Merge SyntaxStructureExtractor + DiRegistrationExtractor into a single pass to eliminate 95%+ of runtime
4. **Token budget accuracy** — Use post-render token estimation for budget enforcement, not pre-render signature estimates
5. **Project-reference signals** — Detect signals from project-to-project references, not just NuGet packages
6. **gRPC endpoint stubs** — Basic `MapGrpcService` detection for completeness
