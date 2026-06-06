# DevContext v2 — Iteration 5 Delivered & Current State

**Generated**: 2026-06-06  
**Branch**: `develop`  
**Tests**: 137 passing · 0 failing · 0 warnings · format clean  

---

## 1. Iteration 5 Delivery Summary

| Phase | Commit | What Changed | Tests |
|---|---|---|---|
| **1.1** | `56ae7ee` | **Versioned Minimal API** — MapGroup prefix resolution in extension method bodies. `ExtractGroupPrefixes` helper refactored to work on any `SyntaxNode`. `FindAssignedVariable` walks up chained calls (`vApi.MapGroup("api/catalog").HasApiVersion(1,0)` → variable `api`). eShop routes now show `api/catalog/items` instead of `/items`. | 135 |
| **1.2** | `88c1a1c` | **Project-reference signals** — `DependencyExtractor` now scans `ProjectReference` XML elements in addition to `PackageReference`. Referenced project names (without `.csproj`) are checked against `PackageSignalMap`. AutoMapper signal (and others) now fire when referenced via project-to-project refs. | 136 |
| **1.3** | `67b8908` | **TokenBudgetEnforcer accuracy** — Added `ParameterNames` to char count estimate. Changed divisor from 3 to 4 (matching renderer's `chars/4`). Prevents budget underestimation that caused 112K token output with 20K budget. | 136 |
| **2.1** | `ccc0ef4` | **Library-mode renderer** — When no detections are present (library/project mode), emits a compact **namespace-summary** instead of the flat type wall. Shows `{namespace} — N types (M public)`, lists public types. Dedicated test. | 137 |

---

## 2. Benchmark Results (Iteration 5)

### eShop (Reference Architecture — 24 projects)

| Metric | Iteration 4 | Iteration 5 | Change |
|---|---|---|---|
| Runtime | 4.5s | **3.2s** | ↓ 29% |
| Endpoints | 30 (no prefix) | **30 (with prefix)** | Routes show `api/catalog/items` |
| Controller actions | 14 | 14 | — |
| MediatR handlers | 18 | 18 | — |
| Detections | 183 | 183 | — |
| Types pruned | 352/517 | **439/517** | Kept more (better estimate) |
| Tokens | 6676 | **7114** | — |
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
| Project references at 0.9 confidence (not 1.0) | Project reference is less definitive than direct NuGet package reference — the project might not be the actual library |
| `chars/4` divisor matches renderer | Avoids mismatch between budget enforcement and actual output cost |
| Library-mode namespace summary | Flat type walls are useless for LLM context; namespace grouping with public type listing is more actionable |

### Remaining Gaps (Deferred to Future Iterations)

| Gap | Priority | Estimated Effort |
|---|---|---|
| **Shared tree walk** — Merge SyntaxStructure + DiRegistration to eliminate 95%+ runtime | P1 | Large refactor |
| **gRPC endpoint detection** — `MapGrpcService<T>()` | P2 | Small |
| **Controller convention routes** — IdentityServer `AddControllersWithViews` patterns | P2 | Small |
| **Duplicate type deduplication** — Merge overlapping class names across projects | P3 | Medium |
| **Route base path from configuration** — Resolve full route paths from appsettings/di config | P3 | Medium |
