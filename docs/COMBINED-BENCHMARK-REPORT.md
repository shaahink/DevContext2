# DevContext v1.0 — Combined Benchmark Report

**Generated**: 2026-06-11 | **Version**: v1.0.0  
**Tests**: 221 passing (157 Core + 64 Desktop)

---

## Benchmarked Repos

| # | Repo | Description | Types | Size |
|---|------|-------------|-------|------|
| 1 | **DntSite** | Persian community/blog platform — Blazor SSR + 10 API controllers + SQLite + 23 DNTScheduler jobs | 1,289 types | Large real-world |
| 2 | **MinimalApiProject** | Test fixture — minimal API with MediatR + EF Core | ~20 types | Small fixture |
| 3 | **CleanArchProject** | Test fixture — Clean Architecture template | ~15 types | Small fixture |

---

## DntSite Results (Real-World ~1,289 Types)

### Accuracy

| Metric | Value | Notes |
|--------|-------|-------|
| Architecture classification | **ControllerBased (80%)** | ✅ Correct! Previously misclassified as MinimalApi |
| Signals detected | controllers (90%), minimal-apis (80%), efcore (100%) | ✅ All 3 correct |
| Endpoints found | **70** | ✅ All 10 controllers detected (was 13 in v2.0) |
| EF entities found | 2 (ApplicationDbContext) + 30 migrations grouped | Entities from `OnModelCreating` detected |
| Background workers | **24** | ✅ All 23 DNTScheduler jobs + 1 BackgroundService |
| DI registrations | **83** | ✅ `AutoInjectAllServices` bulk pattern detected |
| Indirect wiring issues | 12 | ManualServiceLocator + ReflectionActivation |
| Middleware entries | 11 | ✅ Full pipeline in order |

### Performance

| Scenario | Profile | Time | Tokens | Total Detections | Active Types |
|----------|---------|------|--------|-----------------|-------------|
| Overview | focused (default) | **8.3s** | 6,273 | 186 | 12 |
| Overview | debug | 23.2s | 6,272 | — | 12 |
| Trace (`FeedController`) | focused | **13.0s** | 5,990 | 198 | 11 |
| Trace (`FeedController`) | debug | ~18s | — | — | — |

### Pipeline Breakdown (Overview / Focused)

| Stage | Time | Dominant Extractor |
|-------|------|--------------------|
| Discovery & Cache Warmup | 136ms | FileTreeExtractor (68ms) |
| Generic Extraction | 5,403ms | DiRegistrationExtractor (5,395ms) + SyntaxStructureExtractor (5,398ms) |
| Signal Sealing | 9ms | — |
| Specific Extraction | 2,373ms | EndpointExtractor (1,234ms) |
| Pruning | 50ms | 1,289 → 40 types |
| Compression | 12ms | StructuralDeduplicator (−51%) |
| Rendering | 31ms | 6,273 tokens output |

**Bottleneck**: DiRegistrationExtractor + SyntaxStructureExtractor consume ~67% of total time (5.4s parallel). These parse every `.cs` file's syntax tree and walk invocation chains.

### Extractor Detection Counts (Overview / Focused)

| Extractor | Detections |
|-----------|-----------|
| ControllerActionExtractor | **69** (up from 12 in v2.0) |
| DiRegistrationExtractor | **83** (up from 12 in v2.0) |
| ProgramCsFlowExtractor | 39 (up from 11 in v2.0) |
| EfCoreExtractor | 33 |
| IndirectWiringDetector | 12 (Trace mode) |
| EndpointExtractor | 1 (Minimal API endpoint) |
| InMemoryEventBusExtractor | 0 |

---

## Fixture Results

### MinimalApiProject

| Metric | Value |
|--------|-------|
| Time | 606ms |
| Tokens | 381 |
| Architecture | MinimalApi (100%) ✅ |
| Signals | dapper, minimal-apis, mediatr, efcore |

### CleanArchProject

| Metric | Value |
|--------|-------|
| Time | 516ms |
| Tokens | 458 |
| Architecture | CleanArchitecture (100%) ✅ |
| Signals | minimal-apis, mediatr, efcore |

---

## Detection Coverage

| Dimension | DntSite (1,289 types) |
|-----------|----------------------|
| Architecture classification | ControllerBased (80%) |
| Controller endpoints detected | 70 (all 10 controllers) |
| Background workers detected | 24 (23 DNTScheduler + 1 BackgroundService) |
| DI registrations detected | 83 (incl. `AutoInjectAllServices` bulk pattern) |
| Indirect wiring detections | 12 (service locators, reflection activation) |
| Middleware pipeline entries | 11 (full pipeline in order) |
| EF Core entities | BaseEntity + 30 migrations grouped |


---

## Known Limitations

| Limitation | Impact |
|-----------|--------|
| DiRegistrationExtractor + SyntaxStructureExtractor consume 67% of pipeline time | Large repos may take 10s+ |
| EF entities from `ApplyConfigurationsFromAssembly` not statically detectable | Entity count shows 2 instead of 30+ (entities defined in separate EfConfig files) |
| Migrations listed as individual rows under "Migrations" group | 30 migration rows in output; could be summarized |
| No Blazor-specific extraction | `.razor` files, SSR components, interactive server setup not detected |
| `--around` filtering is proximity-based (not exact-match) | `--around FeedController` shows nearby controllers too |
