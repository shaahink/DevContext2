# DevContext Gap Analysis: VahidN-DntSite-default

**Date**: 2026-06-11
**Tool version**: v2.0.0 (`simplify-ui` branch)
**Repo analyzed**: `C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default`
**Actual project**: ASP.NET Core 10 Blazor SSR + API Controllers, SQLite + EF Core, Lucene.NET, 23 scheduled jobs, no MediatR

---

## Test Matrix

| Run | Scenario | Profile | Focus | Tokens | Time | Output |
|-----|----------|---------|-------|--------|------|--------|
| A | overview | focused | — | ~2,131 | 35s | `docs/report-overview.md` |
| B | deep-dive | focused | FeedController | ~2,408 | 16s | `docs/report-trace-feed.md` |
| C | deep-dive | debug | FeedController | ~3,930 | 18s | `docs/report-trace-debug.md` |
| D | overview | focused | — | ~2,018 | 10s | `docs/report-overview-metrics.md` |

---

## Gap Summary

| Severity | Description |
|----------|------------|
| **Critical** | 80% of controller endpoints missed — 13 found vs ~55 actual |
| **Critical** | Architecture classified as MinimalApi — actually Blazor SSR + Controllers |
| **Critical** | DbContext entities not extracted — shows `<Migration>` entries instead of `DbSet<T>` entities |
| **High** | DI registrations nearly empty — 1 found vs 100+ actual |
| **High** | Background workers not detected — 23 DNTScheduler jobs + 1 BackgroundService invisible |
| **Medium** | Related types capped at 40 for 1289-type project |
| **Medium** | Route tokens `[controller]`/`[action]` not expanded to actual values |
| **Medium** | Multiple `[Route]` on one action — only first read, rest dropped |
| **Low** | No Blazor awareness — .razor files, SSR, interactive components invisible |
| **Observation** | Middleware pipeline detection works fully (11/11 detected) |
| **Observation** | Call graph works correctly in debug profile (477 edges) |
| **Observation** | Indirect wiring detection active in Trace mode (12 detections) |

---

## Gap 1 — Architecture Misclassification: Critical

**What the tool says**: `MinimalApi (80% confidence)`  
**What it actually is**: Blazor SSR Web App with API Controllers + Blazor interactive server

**Why**: The `ArchitectureStyleDetector` weights the `minimal-apis` signal (triggered by `Microsoft.NET.Sdk.Web`) at 80% even when the `controllers` signal is at 90% confidence. The project uses `app.MapControllers()` which is part of Minimal API endpoint routing, but the architecture is clearly controller-based.

**Impact**: All downstream LLM context is framed as "this is a Minimal API project" which is misleading for the reader.

**Fix**: When `controllers` signal confidence exceeds `minimal-apis` confidence, classify as `ControllerBased` or `WebApp` rather than `MinimalApi`.

---

## Gap 2 — Controller Endpoint Detection: Critical

**Found**: 13 endpoints across 5 controllers  
**Actual**: ~55 endpoints across 10 controllers

| Controller | Actions | Route style | Found? |
|-----------|---------|-------------|--------|
| UploadFileController | 7 | `[HttpPost(template: "...")]` | **7 of 7** |
| FtsController | 2 | `[HttpGet/HttpPost(template: "...")]` | **2 of 2** |
| JavaScriptErrorsReportController | 1 | `[HttpPost(template: "...")]` | **1 of 1** |
| OpenSearchController | 1 | `[HttpGet(template: "...")]` | **1 of 1** |
| WelcomeController | 1 | `[HttpGet(template: "...")]` | **1 of 1** |
| FeedController | ~19 | `[Route("...")]` only | **0 of 19** |
| ProjectsFeedsController | ~11 | `[Route("...")]` only | **0 of 11** |
| FileController | ~12 | `[Route("...")]` only | **0 of 12** |
| ExportsController | 2 | `[Route("...")]` only | **0 of 2** |
| SitemapController | 1 | `[Route("...")]` only | **0 of 1** |

**Root cause**: `ControllerActionExtractor.ExtractHttpMethod()` (in `src/DevContext.Core/Extractors/Specific/ControllerActionExtractor.cs`) only checks for `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`, `[HttpPatch]`. If none found, it returns `null` and the action is skipped.

All 5 missed controllers use **bare `[Route(...)]`** on their actions without an HTTP verb overlay. ASP.NET Core infers GET for these (per `[ApiController]` convention), but the extractor doesn't implement this inference.

**Secondary issues**:
- `[controller]` and `[action]` tokens in `[Route]` templates are not expanded (e.g. `[controller]/[action]` is output literally rather than `Feed/Posts`)
- Multiple `[Route]` attributes on one action — only the first is read, the rest are silently dropped (e.g. `FeedController.SiteFeed()` has 9 alternate routes)

**Fix**:
1. When `ExtractHttpMethod()` returns null but the method has a `[Route]` attribute, infer HTTP verb from method name prefix (Get/Post/Put/Delete/Patch) or default to GET
2. Expand `[controller]`, `[action]`, `[area]` tokens in route templates
3. Support multiple `[Route]` attributes per action

---

## Gap 3 — EF Core Entity Extraction: Critical

**What the tool shows**:
- `ApplicationDbContext`: entity = `<OnModelCreating>` (singular)
- 30 migration classes (e.g. `V2024_04_19_1424`) each rendered as a separate table with entity = `<Migration>`

**What it should show**: 30+ `DbSet<T>` entity types (BlogPost, BlogComment, News, Survey, Course, etc.) grouped under `ApplicationDbContext`, plus a single line noting 30 migrations exist.

**Root cause**: `EfCoreExtractor` is detecting migration classes as DbContext-like objects and emitting them individually. Meanwhile, actual `DbSet<T>` properties on `ApplicationDbContext` are either not being matched by the extractor's pattern or are classified incorrectly.

**Fix**: 
1. Verify `EfCoreExtractor` correctly identifies `DbSet<T>` properties via Roslyn symbol analysis (not just string matching). The current regex/string approach may fail on `DbSet<BlogPost>` when `BlogPost` is in a different namespace.
2. Filter out migration classes (those inheriting `Migration`) from the DbContext entity listing.
3. Show migrations as a single summary entry, not as individual data model tables.

---

## Gap 4 — DI Registration Detection: High

**Found**: 1 (EfDbLoggerProvider singleton)  
**Actual**: 100+ services registered via `services.AutoInjectAllServices()`

**The DntSite DI pattern**:

```csharp
// ServicesConfigs/ServicesRegistry/ServicesConfig.cs
services.AutoInjectAllServices();  // from DNTCommon.Web.Core
```

This scans assemblies and auto-registers all classes implementing interfaces. The project has no explicit `AddScoped<ISomeService, SomeService>()` calls except for a handful in `Program.cs`.

**Root cause**: `DiRegistrationExtractor` only pattern-matches on `Services.AddSingleton<T>()`, `AddScoped<T>()`, `AddTransient<T>()` with a Roslyn syntax walker. It has no awareness of library-specific auto-registration conventions like `AutoInjectAllServices()`, `Scan()`, or `RegisterAssemblyTypes()`.

**Fix (two-phase)**:
1. Detect `AutoInjectAllServices`, `Scan`, `RegisterAssemblyTypes`, and similar bulk-registration calls as a special kind of DI detection with a summary (e.g. "X services auto-registered via AutoInjectAllServices()")
2. Follow up by walking the assembly types to list the actual registrations (requires more work — could be a separate extractor)

---

## Gap 5 — Background Worker Detection: High

**Found**: 0 background workers  
**Actual**: 23 scheduled jobs + 1 BackgroundService

**The DntSite scheduling pattern**:
```csharp
services.AddDNTScheduler(options => {
    options.AddJob<BackupDatabaseJob>();
    options.AddJob<DailyNewsletterJob>();
    // ... 21 more
});
```

And one standard `AddHostedService<StartupIndexingService>()`.

**Root cause**: `ProgramCsFlowExtractor` detects `AddHostedService<T>()` patterns but not `AddDNTScheduler()` or other scheduling library conventions (`AddQuartz`, `AddHangfire`, etc.).

**Fix**: 
1. Add `AddDNTScheduler(...)` pattern recognition with extraction of the job type arguments
2. Consider detecting `AddQuartz`, `AddHangfire`, and similar library scheduling patterns

---

## Gap 6 — Route Token Expansion: Medium

**Observed**: Routes reported as `[controller]/[action]` literally rather than `Feed/Posts`.

**Impact**: The route is technically correct as an ASP.NET Core template, but it's less readable for LLMs and humans.

**Fix**: Resolve `[controller]` to the controller class name (minus "Controller" suffix) and `[action]` to the method name. Also support `[area]` if present.

---

## Gap 7 — Related Types Pruning: Medium

**Observed**: Only 27 types shown in Related Types section out of 1289 total. The pruning cap (`MaxSurvivingTypes = 40` for overview) is too low for a project this size.

**Impact**: Services like `FeedsService`, `BlogPostsService`, `FullTextSearchService` that are critical to understanding the architecture are invisible.

**Fix**: Consider raising the default `MaxSurvivingTypes` for overview from 40 to 80–100, or implementing a smarter selection that prioritizes types referenced by detected endpoints/handlers/consumers.

---

## Gap 8 — Multiple Route Attributes: Medium

**Observed**: `FeedController.SiteFeed()` has 9 `[Route]` attributes (RSS, Atom, plain, different formats). Only the first is read.

**Impact**: Alternate URL patterns for content feeds are invisible.

**Fix**: Collect all `[Route]` attributes on a method and emit them all.

---

## Gap 9 — No Blazor Awareness: Low

**Observed**: No mention of Blazor SSR, interactive server components, or `.razor` files anywhere in the report.

**Impact**: The tool treats the project as a pure API backend, missing the fact that 80% of the codebase is Blazor UI components.

**Fix**: Add a Blazor-specific extractor that:
1. Detects `.razor` files and counts them
2. Reports render modes (`AddInteractiveServerRenderMode`, etc.)
3. Lists key component types (pages, layouts, etc.)
This is a larger feature request — the tool currently focuses on backend architecture.

---

## What Worked Well

| Dimension | Result |
|-----------|--------|
| **Middleware pipeline** | 11/11 middleware entries detected with correct ordering |
| **Call graph** (debug profile) | 477 edges at depth 5, correctly traces FeedController → services → DB |
| **Indirect wiring** (trace mode) | 12 detections: service locators, Activator.CreateInstance, assembly scanning |
| **File tree discovery** | 1289 types discovered, all .cs files indexed |
| **Project structure** | 3 projects correctly identified with dependencies |
| **Package/signal mapping** | efcore, controllers, minimal-apis signals correctly detected |
| **No .sln detection** | Correctly reported "No .sln file found" as a diagnostic |
| **Performance** | 10–35 seconds for 1289 types is reasonable |
| **Compression** | Token reduction visible (~2900→2200 chars) |

---

## Recommendations by Priority

### P0 — Fix immediately (high impact, relatively isolated)
1. **Controller endpoint inference**: Add HTTP verb inference for bare `[Route]` attributes (method name prefix matching)
2. **Architecture classification**: When `controllers` > `minimal-apis` confidence, classify correctly
3. **EF Core entity extraction**: Fix DbSet entity detection; filter migrations from data model table

### P1 — High value
4. **Route token expansion**: Expand `[controller]`/`[action]` to actual names
5. **DI auto-registration**: Detect `AutoInjectAllServices` etc. as bulk registration
6. **Multiple route attributes**: Collect all `[Route]` attributes per action
7. **Background worker detection**: Add `AddDNTScheduler` and similar patterns

### P2 — Nice to have
8. **Related types pruning**: Increase cap or prioritize types
9. **Blazor awareness**: Detect .razor files and render modes

---

## Metrics from Tool Run D

```
Pipeline: 10.2s total
├─ DiscoveryAndCacheWarmup: 193ms (FileTree: 106ms, Solution: 32ms, ProjectStructure: 26ms)
├─ GenericExtraction: 7,090ms
│  ├─ DiRegistrationExtractor: 7,069ms (1,289t, 12d)  ⚠ slowest
│  └─ SyntaxStructureExtractor: 7,081ms (1,289t, 12d)  ⚠ slowest
├─ SignalSealing: 10ms (controllers 90%, minimal-apis 80%, efcore 100%)
├─ SpecificExtraction: 2,850ms
│  ├─ EndpointExtractor: 1,570ms (0t, 1d)
│  ├─ ControllerActionExtractor: 313ms (0t, 12d)    ← only 12 detections
│  └─ EfCoreExtractor: 613ms (0t, 32d)              ← 31 migrations + 1 OnModelCreating
├─ Pruning: 52ms (1289→40 types)
├─ Compression: 15ms
└─ Rendering: 34ms (2,018 tokens)

Total detections: 57
Types active: 40 (of 1289)
```

**Note**: `DiRegistrationExtractor` and `SyntaxStructureExtractor` consume 70% of total pipeline time (14s combined). These two run in parallel (Stage 2), so max wall time is the slower one (~7s). Still, 14s CPU for 7s wall time is significant.
