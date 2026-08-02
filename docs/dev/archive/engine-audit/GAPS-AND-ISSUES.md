# DevContext Cross-Repo Analysis — Gaps and Issues

> Generated 2026-06-30 from branch `feat/desktop-v2` (commit `ff143d8`) against
> 4 fresh repos: **Serilog** (library), **Ocelot** (API gateway), **Files** (desktop),
> **aspnetcore** (.NET framework internals). Each was analyzed with both Map and Trace.

---

## Summary of Findings

| Severity | Count | Category |
|----------|-------|----------|
| **Critical** | 3 | Token budget exceeds limit, Noise filter leaks test entries, Library traces are empty |
| **High** | 4 | Desktop apps misclassified, Map entry-point starvation, Map→Trace dispatch fragility, Map over-budget renders |
| **Medium** | 4 | MSBuild-variable token leak, Ocelot entry→target low, Serilog DI edges missing, Files call graph unused |
| **Low** | 2 | RESULT inference too aggressive, Framework-boundary detection weak on large repos |

---

## Critical Issues

### C1 — Token Budget Enforcement is Broken

**Evidence:** The `TokenBudgetEnforcer` reports 99% cuts across almost every run (247→25, 827→7, 1185→10, 13884→20). Yet the aspnetcore Map output was **21,071 tokens** against an 8,000 budget — 2.6x over. Ocelot Map was only **640 tokens** — the budget enforcer is cutting too aggressively, leaving the output sparse while the budget headroom is unused.

| Repo | Types Before | Types After | Cut % | Output Tokens | Budget | Over? |
|------|-------------|-------------|-------|---------------|--------|-------|
| Serilog Map | 247 | 25 | 89% | ~3,651 | 8,000 | Under |
| Ocelot Map | 827 | 7 | 99% | ~640 | 8,000 | **Severely under** |
| Files Map | 1,185 | 10 | 99% | ~3,668 | 8,000 | Under |
| aspnetcore Map | 13,884 | 20 | 99% | ~21,071 | 8,000 | **2.6x over** |

**Impact:** Ocelot has 31 controller actions + 34 endpoints detected but the map shows **only 3 entries**. The budget enforcer is cutting before the renderer can use the headroom. Conversely, aspnetcore massively overruns the budget.

**Locus:** `Scoring/TokenBudgetEnforcer`, the "stored tokens" count is mismatched with the renderer's actual token consumption. Type count (e.g., 20) may not correspond to rendered token count (e.g., 21,071). The budget is applied at scoring time per-type, but the renderer adds per-entry, topology, and package lines that the scorer doesn't account for.

**Suggested fix:** Replace the type-count-based budget model with a render-time token tracker that operates on the rendered sections themselves (per P2 in DESIGN-PHILOSOPHY.md). Budget scoring should not destroy types before rendering — it should inform a render-time cut list.

---

### C2 — Noise Filter Does Not Exclude Test Projects from Entry Inventory on aspnetcore

**Evidence:** The aspnetcore Map shows 518 HTTP entries. Sampling the output:
- `GET /api/get/{id}` from `src/Http/Routing/test/testassets/RoutingWebSite/...` (lines 413-415)
- `GET /` from `src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs` (line 455)
- `POST /_ready/{token}` from `src/Components/Testing/src/Infrastructure/ServerFixture.cs` (line 456)
- `GET /oidc/authorize` from `src/Components/test/testassets/Components.TestServer/...` (line 458)
- `GET /parallel-abort` from `src/Servers/Kestrel/stress/Program.cs` (line 467)

**These are ALL test projects.** The `NoiseFilter.IsProductionEntrySource` is supposed to gate these, but it's not working for the aspnetcore project structure. The HANDOVER-LIBRARY-SUPPORT (commit `05de28d`) fixed this for MediatR using `IsInTestProject`, but aspnetcore's test projects may use a different path convention (`testassets/`, `stress/`, `Testing/src/`).

**Impact:** 518 entries shown; probably 400+ are test noise. Makes the Map useless for orientation. An LLM given this output would think aspnetcore *is* a test framework.

**Locus:** `Graph.NoiseFilter.IsProductionEntrySource`, `GraphBuilder.AddHttpEntryPoints`

---

### C3 — Library Traces Are Empty (No Call Graph for Library Types)

**Evidence:**
- Serilog `--focus Log` → depth 0, 4 lines: just `▸ ENTRY Log`
- Serilog `--focus LoggerConfiguration` → depth 0, 4 lines: just `▸ ENTRY LoggerConfiguration`
- Despite 46 Calls edges detected in the graph (for Log trace), none were followed.

**Root cause:** The trace traverses edges from the entry node. For a library type that has no HTTP/bus/handler entry edges, the trace walker starts from the Type node itself. The call edges exist (46 detected for Serilog) but the trace builder's `OutEdgesWithTwin` bridge expects *handler entry members* or *constructor members* — not general method call edges from a library type.

**Impact:** `--focus` on library types is useless. The Map says "→ drill in: --focus `<TypeName>`" (e.g., `--focus IDestructuringPolicy`) but following that advice produces a 4-line empty trace.

**Locus:** `Graph.TraceBuilder.OutEdgesWithTwin` — the controlled bridge needs a library-aware path that surfaces method-out-edges for library types.

---

## High-Priority Issues

### H1 — Desktop Apps Are Misclassified as Libraries

**Evidence:** Files (a WinUI 3 desktop file manager) is classified as `LIBRARY` with 127 public types. The Map shows ENTRY API, ABSTRACTIONS, PUBLIC SURFACE, and CONSUMER PATHS — all library-surface sections. Zero entries detected.

**Root cause:** `ArchetypeDetector` sees no HTTP endpoints, no MediatR, no message bus → classifies as Library. The PRODUCT-DIRECTION §4 entry-point ladder lists Hosted services, Blazor components, console `Main`, and desktop apps at rungs 4-7 — none are implemented.

**Impact:** DevContext is useless for desktop .NET apps. A WinUI/WPF/Avalonia app shows as a "library" — fundamentally wrong archetype.

**Locus:** `Graph.ArchetypeDetector.cs`, no desktop-app extractor exists.

---

### H2 — Map Entry-Point Starvation (Token Budget Cuts Entries Before Render)

**Evidence:** Ocelot produced 31 controller action detections + 34 endpoint detections. Yet the Map shows only 3 entries. The TokenBudgetEnforcer cut 827 types to 7 — the entries were likely among the 820 types cut. The 3 surviving entries are all from the Ocelot.Samples.Web project, not from the core Ocelot library.

**Impact:** The Map's ENTRY POINTS section is the most valuable orientation artifact — having only 3 entries from 65+ detections makes it almost useless.

**Locus:** Token budget enforcer precedence: entries should be promoted above the budget cut, or the budget should be render-time per section.

---

### H3 — Map→Trace Dispatch Falls Back to Map for Library Focus

**Evidence:** Files `--focus IAppService` produced the full LIBRARY map (227 lines), not a trace. The CLI printed "Overview map (no focus)" in its header despite `--focus` being passed. The focus resolves to a type, but the library render path overrides trace rendering.

**Impact:** When the archetype is Library, `--focus` effectively does nothing — the user gets the library surface again instead of a trace.

**Locus:** `Pipeline.DiscoveryPipeline.RenderAsync` — the library render path (`LibrarySurfaceRenderer`) is chosen before checking for `--focus`. The dispatch needs to accommodate library-trace mode.

---

### H4 — Map Output on aspnetcore is Unreviewably Large

**Evidence:** aspnetcore Map is 934 lines, 21,071 tokens — way over the 8,000 budget. The TOPOLOGY section alone runs from lines 8–404 (397 lines of project tree). The ENTRY POINTS section runs from lines 405–751 (347 lines, 518 entries). No human or LLM can usefully consume this.

**Impact:** The primary use case ("paste into LLM") fails completely for large repos. The output is too long and too noisy to be useful.

**Locus:** Token budget enforcement as discussed in C1. Also, the Map renderer should truncate topology to top-level projects only, and collapse entry points beyond a reasonable limit.

---

## Medium-Priority Issues

### M1 — MSBuild Variable Tokens Leak into STACK

**Evidence:** aspnetcore STACK shows `net472` — an MSBuild variable like `$(NetFrameworkMinimum)` that should have been filtered by the Phase 4 fix. The fix in `MapRenderer` and `MapBuilder.BuildPackages` strips `$(...)` tokens, but `net472` is a resolved TFM, not a variable token. However, the STACK shows mixed frameworks (net11.0, net472, netstandard2.0) which is technically correct for the aspnetcore repo (multi-targeting), so this may be accurate — but the net472 entry is distracting.

Actually, the real issue here is that `net472` IS a valid TFM for the repo (many projects target it for compat). So this may not be a bug. Let me reclassify.

**Reclassified as LOW.** The STACK is technically correct for a multi-TFM repo.

---

### M2 — Ocelot Entry→Target Resolution Only 2/3

**Evidence:** Ocelot Map shows "3 entries · 2/3 → target". The `GET /configuration` entry has no target:
```
GET /configuration  (src/Administration/FileConfigurationController.cs:23)
```
Compared to the POST:
```
POST /configuration  → FileAndInternalConfigurationSetter.SetAsync  (src/Administration/FileConfigurationController.cs:41)
```

The `GET` handler likely returns a view or calls a different service. The `ResolveEntryTarget` fallback to `ResolvePrimaryCall` (Phase 2 fix) works for the POST but not the GET.

**Impact:** 1 in 3 entries has no target — the drill-in hint is broken for that entry.

**Locus:** `GraphBuilder.ResolveEntryTarget` — needs to handle controller actions that return views (no service call) by falling back to the controller type itself.

---

### M3 — Serilog DI Registration Edges Missing

**Evidence:** Serilog Map shows 1 edge (Resolves) despite having `LoggerConfiguration` with extension methods that register services. The `DiRegistrationExtractor` ran but found 0 detections — the DI registrations are likely in extension method bodies, not in `Add*` convention methods.

**Impact:** No DI graph for Serilog means no understanding of how sinks/enrichers are wired.

**Locus:** `Extractors/Generic/DiRegistrationExtractor.cs` — may not detect registrations in extension method bodies or `UseSerilog()`-style host builder extensions.

---

### M4 — Files Call Graph Edges Not Utilized

**Evidence:** Files `--focus MainWindow` produced 165 Calls edges in the graph, but the trace is empty (depth 0). The call graph extractor built edges, but the trace builder didn't follow them because MainWindow has no entry-point edges (it's a desktop app, not an HTTP handler).

**Related to H1 and C3.** Once desktop entry points are added, traces should work.

---

## Low-Priority Issues

### L1 — RESULT Inference on Simple Endpoints May Be Too Aggressive

**Evidence:** Ocelot `DELETE /outputcache/{region}` shows:
```
RESULT   200 OK / 204 No Content · failure → 404 Not Found
```
This is inferred from the controller action's return type, not from actual HTTP annotations. For a DELETE, 204 is correct, but 404 is assumed by convention.

**Impact:** Minor. The RESULT inference is a nice-to-have heuristic. False positives are possible.

---

### L2 — Framework Boundary Detection Weak on Large Repos

**Evidence:** aspnetcore traces show connections to test-specific DbContexts (`Microsoft.AspNetCore.Identity.EntityFrameworkCore.Test.CustomDbContext`) instead of production types. The framework boundary (`Microsoft.*`, `System.*`) stop rule fires, but test-project types aren't filtered before the trace starts.

**Impact:** Trace TOUCHES include test entities, confusing the picture.

**Locus:** `TraceBuilder` framework boundary detection — should also filter test-project types.

---

## Archetype Detection Analysis

| Repo | Expected Archetype | Detected Archetype | Correct? |
|------|-------------------|--------------------|----------|
| Serilog | Library | LIBRARY | Yes |
| Ocelot | App (API Gateway) | MAP (ControllerBased) | Yes |
| Files | App (Desktop) | LIBRARY | **No** |
| aspnetcore | App (Framework) | MAP (NLayer) | Partially* |

*aspnetcore is the ASP.NET framework itself — it's a library suite with many sample/test apps. NLayer is a reasonable detection but not ideal.

**Root cause for Files:** `ArchetypeDetector` only checks for HTTP, Bus, Domain, and Worker entry points. Desktop apps have none of these. A WinUI/WPF app needs a new entry-point kind (Window/Page/View lifecycle, Command handlers, App.OnLaunched).

---

## Token Budget Analysis

The `PatternRelevancePruner` shows **0% delta** across all runs — it never changes the type count. Only the `TokenBudgetEnforcer` cuts. The relevance scoring pipeline (which should promote entry-point-related types) is not affecting the budget.

| Run | Pre-budget types | Post-budget types | PatternRelevancePruner delta |
|-----|-----------------|-------------------|------------------------------|
| Serilog | 247 | 25 | 0% |
| Ocelot | 827 | 7 | 0% |
| Files | 1,185 | 10 | 0% |
| aspnetcore | 13,884 | 20 | 0% |

**The scorer funnel is a no-op for relevance.** Only the budget enforcer does anything, and it does a flat cut with no awareness of entry-point importance.

---

## Scaling Analysis

| Repo | Files | Projects | Analysis Time | Nodes | Edges | Tokens |
|------|-------|----------|---------------|-------|-------|--------|
| Serilog | 214 | ~5 | 6.5s | 125 | 1 | 3,651 |
| Ocelot | 741 | 16 | 21.3s | 638 | 335 | 640 |
| Files | 1,117 | ~12 | 20.6s | 1,169 | 92 | 3,668 |
| aspnetcore | 9,959 | 395 | 143.4s | 66,274 | 13,498 | 21,071 |

**Analysis time scales roughly linearly with file count.** The CallGraphExtractor dominates Stage 3 for large repos (29-37s for aspnetcore). The SyntaxStructureExtractor dominates Stage 2 (26-64s). Entry-scoped binding (Phase 6) helps for focused traces but Map mode binds all files.

---

## Output Quality Summary

| Repo | Artifact | Quality | Key Issue |
|------|----------|---------|-----------|
| Serilog Map | `map.md` | **Good** | Library surface is comprehensive, ranked, documented. |
| Serilog Trace (Log) | `trace-log.md` | **Fail** | Empty trace (4 lines). Can't follow library wiring. |
| Serilog Trace (LoggerConfiguration) | `trace-loggerconfiguration.md` | **Fail** | Empty trace (4 lines). |
| Ocelot Map | `map.md` | **Poor** | Only 3/65+ entries shown. Token budget starved the output. |
| Ocelot Trace (POST /configuration) | `trace-post-configuration.md` | **Excellent** | Deep, verified edges. Honest truncation. 277 lines. |
| Ocelot Trace (DELETE /outputcache) | `trace-delete-outputcache.md` | **Good** | Short but correct. RESULT section present. |
| Files Map | `map.md` | **Fail** | Misclassified as library. No app entries. |
| Files Trace (IAppService) | `trace-appservice.md` | **Fail** | Fell back to Library map instead of trace. |
| Files Trace (MainWindow) | `trace-mainwindow.md` | **Fail** | Empty trace (4 lines). |
| aspnetcore Map | `map.md` | **Fail** | 934 lines, 2.6x over budget, test entries flooding. Not usable. |
| aspnetcore Trace (POST /login) | `trace-post-login.md` | **Poor** | 10 lines, connects to test DbContext, not production. |
| aspnetcore Trace (WebApplication) | `trace-webapplication.md` | **Fail** | Empty trace (4 lines). |

---

## Key Files Referenced

| Issue | File |
|-------|------|
| Token budget | `src/DevContext.Core/Scoring/TokenBudgetEnforcer.cs` |
| Noise filter | `src/DevContext.Core/Graph/NoiseFilter.cs` |
| Archetype detection | `src/DevContext.Core/Graph/ArchetypeDetector.cs` |
| Entry→target resolution | `src/DevContext.Core/Graph/GraphBuilder.cs` (ResolveEntryTarget) |
| Trace builder (controlled bridge) | `src/DevContext.Core/Graph/TraceBuilder.cs` (OutEdgesWithTwin) |
| Library surface renderer | `src/DevContext.Core/Rendering/LibrarySurfaceRenderer.cs` |
| Render dispatch | `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` (RenderAsync) |
| DI registration extractor | `src/DevContext.Core/Extractors/Generic/DiRegistrationExtractor.cs` |

---

## Action Items (Priority Order)

1. **[C1]** Fix token budget: render-time tracking instead of pre-render type-count cut
2. **[C2]** Fix noise filter: exclude test-project/testassets/stress entries from aspnetcore-scale repos
3. **[H1]** Add desktop-app archetype: Window/Page/View lifecycle entry points
4. **[C3]** Fix library traces: OutEdgesWithTwin bridge for library type call edges
5. **[H2]** Promote entry points above token budget cut (or render-time per-section budget)
6. **[H3]** Fix Map→Trace dispatch: allow `--focus` to produce trace in library mode
7. **[M2]** Fix entry→target for controller actions returning views (no service call)
8. **[M3]** Improve DI extraction for extension-method-based registration
