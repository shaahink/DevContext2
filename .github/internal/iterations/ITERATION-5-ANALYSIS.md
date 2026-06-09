# DevContext v2 — Iteration 5 Analysis

**Date**: 2026-06-05  
**Branch**: `develop`  
**State**: post-iteration-4, 134 tests green  
**Method**: full read of iter4 benchmark outputs (eShop, VerticalSlice, AutoMapper, TodoApi) + source audit of EndpointExtractor, MarkdownRenderer, PatternRelevancePruner, TokenBudgetEnforcer, LayerClassifier

---

## 1. Output Quality Audit — Section by Section

Reading the actual rendered markdown from all four benchmarks reveals that every section has specific, addressable problems. These are not bugs in the pipeline — they are limitations in detection logic, rendering decisions, and relevance scoring that degrade usefulness for an LLM.

---

### 1.1 Header Block

**Current state (eShop)**:
```
**Architecture**: MinimalApi (100% confidence)
**Signals**: controllers · minimal-apis · mediatr · fluentvalidation
```

**Problems**:

**P0 — Architecture style is wrong for eShop.** eShop is a microservices application with Aspire AppHost. `ArchitectureStyleDetector` assigns `MinimalApi (100% confidence)` because it scores the presence of `minimal-apis` signal. But minimal APIs being present in each service does not make the *solution* a MinimalApi architecture. The correct style is `Microservices` — 24 projects, Aspire AppHost, independent deployment units, service discovery.

**P1 — Aspire signal absent.** eShop has `eShop.AppHost` referencing `Microsoft.Aspire.*` packages but the `aspire` signal does not appear in the signals list. `DependencyExtractor` may not match the Aspire package prefix. This also means `AspireExtractor` never runs.

**P1 — Architecture style wrong for VerticalSlice.** Shows `MinimalApi (80% confidence)` for a Clean Architecture solution using FastEndpoints. The `fast-endpoints` + layered project structure (`Core`, `Infrastructure`, `UseCases`, `Web`) should score `CleanArchitecture` higher. Instead, the SDK:Web detection fires `minimal-apis` at 80% and that wins.

**Signal confidence display**: Signals are shown without confidence levels (e.g., `minimal-apis · fluentvalidation`). A consuming LLM can't tell the difference between `minimal-apis (100%, PackageReference)` and `minimal-apis (80%, SDK heuristic)`. The header should show high-confidence signals with confidence ≥ 0.9 separately from lower-confidence inferences.

---

### 1.2 Architecture Overview Section

**Current state**:
```
## Architecture overview

- Basket.API
- Catalog.API
- ClientApp
- eShop.AppHost
...
```

This is a flat list of project names. The LLM already received the project names in the header (`**Projects**: 24 -- Basket.API, Catalog.API...`). The section is **100% duplicate information** with no added value.

**What it should be**: A layer-grouped, optionally dependency-aware view of the solution structure. For eShop: group by service vs shared library vs AppHost vs tests. For VerticalSlice: show the Clean Architecture rings. For TodoApi: show the single-project flat structure.

The `context.Analysis.ProjectLayerMap` already has layer classifications per project — this data exists, it's just not used in the renderer.

**Proposed structure for layer-grouped overview**:
```
## Architecture overview

**Api**: Basket.API, Catalog.API, Ordering.API, Webhooks.API, WebhookClient, Identity.API
**Presentation**: WebApp, WebAppComponents, ClientApp, HybridApp
**Infrastructure**: EventBus, EventBusRabbitMQ, IntegrationEventLogEF, eShop.ServiceDefaults, eShop.AppHost
**Domain**: Ordering.Domain, Ordering.Infrastructure
**Testing** (excluded from output): Basket.UnitTests, Catalog.FunctionalTests, ...
```

This is ~6 lines instead of 24 and conveys the actual architecture shape.

---

### 1.3 Endpoints Section

**Overall quality**: The endpoints section is now the *most improved* section from iteration 3/4. Real routes, handlers, sources. But several problems remain.

**P1 — All IdentityServer controller routes show `/`.**
The eShop endpoint table shows 14 controller actions, all with route `/`:
```
| POST | / | AccountController.Login | - | AccountController.cs:59 |
| GET  | / | AccountController.Login | - | AccountController.cs:39 |
```

`ControllerActionExtractor` detects `[HttpPost]`/`[HttpGet]` attributes on methods but if there's no explicit `[Route]` at the action level, the route defaults to `/`. IdentityServer controllers use convention-based routing (`AddControllersWithViews()` + `MapDefaultControllerRoute()`), where the route is `/{Controller}/{Action}`. The extractor should derive the conventional route when no explicit template is present: `AccountController.Login` → `/Account/Login`.

**P1 — Duplicate endpoints in VerticalSlice.**
`GetByIdEndpoint.HandleAsync` appears twice in the VerticalSlice output (two different projects both have a `GetByIdEndpoint` class). Similarly `List.HandleAsync`, `Delete.HandleAsync`, `Create.HandleAsync` appear 2+ times.

The endpoint detection key is `{filePath}:{line}` — different files with the same class name generate different entries. The deduplication should happen at the output level: group by `(Method, Route, HandlerType)` and show unique endpoints, noting if multiple projects share the same endpoint pattern.

**P1 — `<dynamic>` routes dominate VerticalSlice.**
17 of 23 FastEndpoints show `<dynamic>`. FastEndpoints routes like:
```csharp
public override void Configure() {
    Get(ProductsApiRoute.GetById);  // constant in another file
}
```
...produce `<dynamic>` because `ProductsApiRoute.GetById` is a field reference, not a string literal. This is expected and can't be fully resolved syntactically. But the display could be improved: show the field name as the route reference rather than the generic `<dynamic>`.

```
| GET | ProductsApiRoute.GetById | GetByIdEndpoint.HandleAsync | ... |
```

This gives the LLM a clue about where to look for the actual route value.

**P2 — Lambda handler display**.
eShop shows `λ WebHooksApi.cs:66` for lambda handlers. This is better than `<lambda>` but still not useful. A short preview of the lambda signature would help:
- Current: `λ WebHooksApi.cs:66`
- Better: `(int id, ...) => ...` (first 40 chars of lambda body/params)

---

### 1.4 Non-Obvious Wiring Section

**The DI registrations table is the worst section in the output.** This is the biggest quality problem.

**Problem 1 — Multi-line lambda cells.**
eShop contains registrations like:
```csharp
services.AddSingleton<IAppEnvironmentService>(serviceProvider =>
{
    var requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
    var fixUriService = serviceProvider.GetRequiredService<IFixUriService>();
    // ... 10 more lines
    return aes;
});
```

This renders as a 15-line table cell. The markdown table is broken and unreadable. The implementation column needs hard truncation at ~60 characters.

**Problem 2 — Duplicate registrations across projects.**
`AddHealthChecks`, `AddOpenTelemetry`, `AddServiceDiscovery` appear 3+ times each in eShop (once per service project). Each project's `Program.cs` has the same service defaults registration. These should be deduplicated: if the same `(ServiceType, Lifetime)` pair appears 3+ times, collapse to one entry with a "×N projects" note.

**Problem 3 — `Extension` lifetime + `?` implementation entries are noise.**
94 of the 121 eShop DI entries are `| Extension | AddSomething | ? |`. These tell the LLM nothing: `| Extension | AddOpenTelemetry | ? |` — there's no service type, no implementation, just an extension method call. These should be filtered out or collapsed to a summary line: `• N extension method registrations (AddOpenTelemetry, AddServiceDiscovery, AddHealthChecks, ...)`

**Problem 4 — Ordering and grouping.**
The DI table dumps registrations in source-scan order across all 24 projects with no grouping. An LLM reading this needs to understand which project's registration it's reading. Group by project or at minimum sort by service type.

**Middleware duplicates:**
`UseDefaultOpenApi` appears 3 times (Order: 1), `UseExceptionHandler` twice, `UseAntiforgery` twice, `UseStaticFiles` 3 times, `UseHsts` twice. All from different projects. These need cross-project deduplication.

**Middleware OrderX numbers are meaningless cross-project.**
The `PipelineOrder` field (1, 2, 3...) is assigned per-file by `ProgramCsFlowExtractor` counting the position of each Use/Map call within that file. Across projects this becomes `UseExceptionHandler` with order 1 appearing next to `UseIdentityServer` with order 4, with no indication they're from different programs. The project name should be shown.

---

### 1.5 Related Types Section

**Current output (eShop)**:
```
- **Api**: OrderStatusChangedToCancelledIntegrationEvent, CreateOrderCommand, ... [50 types in one line]
- **Domain**: ClientRequestEntityTypeConfiguration, OrderDetailViewModel, ... [35 types]
- **Unknown**: HttpRequestExceptionEx, WebNavigatedEventArgsConverter, ... [90+ types]
```

**Problems**:

1. **Not useful as comma-separated lists.** 50+ names in one bullet point is not scannable. An LLM can parse it but it provides zero contextual signal about what each type *is*.

2. **Unknown layer dominates.** In eShop, 90+ types are under `Unknown` — mostly the Hybrid/MAUI app types. In AutoMapper, 100% of types are `Unknown`. The layer classification is not robust enough for real-world projects.

3. **No type kind shown.** Knowing `OrderRepository` is a `class` in Infrastructure means little. Knowing it implements `IOrderRepository`, is `Scoped`, and has 5 methods is the useful context.

4. **Token waste.** Listing 50 names with no additional context wastes ~200 tokens conveying information the LLM can't act on.

**Proposed improvement**: Show per-type: name + kind indicator + key relationship.
```
- **Infrastructure** (4): BuyerRepository (class, impl: IBuyerRepository), RequestManager (class), OrderRepository (class, impl: IOrderRepository), MediatorExtension (static class)
```
This is more tokens per type but far more useful context. Limit to top N types per layer by relevance score.

---

### 1.6 AutoMapper (Library Mode) Output

AutoMapper output is:
- 145 types all under `Unknown` layer as a flat comma-separated list
- No architecture, no signals, no endpoints
- Architecture: "Not detected"

This output provides essentially zero value as LLM context for a developer working on the AutoMapper codebase. Problems:

1. **Library mode scoring doesn't produce a useful output shape.** The `PatternRelevancePruner` library mode boosts public types and penalizes test types, but the *renderer* doesn't change its behavior based on this. It still outputs a flat "Related types" list with no grouping by interface/abstraction.

2. **145 types is still too many.** With 2713 types and 20K budget, the budget allows 145 types. But 145 type names in a flat list have less LLM value than 20 types with their full signatures and key relationships.

3. **No public API surface summary.** A library like AutoMapper has a clear public API: `IMapper`, `IMapperConfigurationExpression`, `Profile`, `CreateMap`, etc. The output should surface this as a structured "Public API" section rather than a flat list.

4. **Test types still leaking.** `Dto11`, `TestProblem`, `DifferentProfiles`, `Example`, `PrimitiveExample` appear in output. The `IsTestOrNoiseType` check catches `TestSuffixes` and `TestNamespaceSegments` but `DifferentProfiles`, `ConventionsNameSplitMember` etc. are test-only types not caught by name patterns.

---

## 2. Relevance and Pruning Analysis

### 2.1 Detection-to-Type Matching Problem

`PatternRelevancePruner` builds a detection lookup by type *name* (not fully qualified): `detectionsByTypeName[type.Name]`. The `EndpointDetection.HandlerType` for a lambda endpoint is the raw lambda expression string (e.g., `async (int id, IOrderService svc) => ...`) — this never matches any type name.

For minimal API lambda endpoints, zero types get the endpoint relevance boost even though endpoint-related types (request/response classes, services) are highly relevant. The boost only fires when the handler is a named method on a named class (`ControllerName.ActionName`).

**Impact on eShop**: 30 minimal API endpoints detected, but many have lambda handlers. Those endpoints boost nobody. The relevance scoring is essentially working on controller action endpoints only.

**Fix needed**: When an endpoint has a lambda handler, extract the *parameter types* from the lambda signature as the relevance-boosted types. `async (int id, IOrderService svc) =>` → boost `IOrderService` (the service being called).

### 2.2 Layer Classification Failures

`LayerClassifier` misclassifies several patterns:

- **`UseCases` layer** (used in Clean.Architecture.UseCases) is not in the classifier. Hits `Unknown`.
- **`ServiceDefaults` projects** (eShop.ServiceDefaults, NimblePros.ServiceDefaults) are classified as `Infrastructure` because of the Aspire package. This is roughly correct but noisy.
- **`AppHost` projects** classified as `Infrastructure` — but they're really orchestration/hosting, not infrastructure in the DDD sense.
- **Mobile/MAUI projects** (`HybridApp`, `ClientApp`) classified as `Presentation` — correct by convention, but they contain ViewModels, Services, Models all mixed together.
- **eShop's ordering service** has both `Ordering.Domain` and `Ordering.Infrastructure` — Domain gets classified correctly, Infrastructure gets classified correctly, but `Ordering.API` maps to `Api` even though it contains `Models`, `Application`, `Infrastructure` sub-folders.

### 2.3 Pruning Effectiveness

For eShop (`focused` profile, 20K budget):
- 517 types found
- PatternRelevancePruner: removes 29 test project types
- TokenBudgetEnforcer: removes 136 types for budget
- Result: 212 types in output (59% pruned)

212 types at 20K budget is a lot. The output is 20K tokens of mostly flat type listings. With better scoring, we should be able to get 80+ types in output with richer per-type context.

The core problem: `PathProximityScore` is 0 for all types when no `--around` argument is given (no focus points). With no focus points, `RelevanceScore` from detections is the only differentiator. But most types in the "Related types" section have `RelevanceScore = 0` (no detection references them). `TokenBudgetEnforcer` then sorts by `0 + 0 = 0` for all of them, effectively cutting arbitrarily (insertion order).

**Fix needed**: When no focus points are provided, assign a default relevance weight based on:
1. Layer priority (Domain + Application > Infrastructure > Api > Unknown)
2. Accessibility (`Public` > `Internal` > `Private`)
3. Type kind (interfaces > abstract classes > concrete classes > records > enums)
4. Whether the type appears in other types' base lists or interfaces
5. Method count (more API surface → higher relevance)

---

## 3. Performance Analysis

| Repo | Runtime | SyntaxStructure | DiRegistration | Total extract |
|---|---|---|---|---|
| TodoApi | 1.3s | ~0.3s | ~0.3s | dominated by startup |
| VerticalSlice | 2.0s | ~0.6s | ~0.6s | parallel |
| eShop | 4.5s | ~2.6s | ~2.6s | parallel (same trees) |
| AutoMapper | 8.1s | ~7.7s | ~7.7s | parallel (same trees) |

The duplicate tree-walking problem is confirmed. `SyntaxStructureExtractor` and `DiRegistrationExtractor` both call `context.Cache.GetSyntaxTreeAsync` for every `.cs` file and then call `root.DescendantNodes()`. The trees are cached (parsed once), but `DescendantNodes()` is re-evaluated per extractor.

For AutoMapper: 2713 types means 500+ source files each iterated by 4 generic extractors = significant redundant work.

**The shared node cache** would address this. A single pass that emits: types, methods, DI registrations, class declarations → each downstream extractor queries the cache instead of re-walking.

---

## 4. Summary: Ranked Output Quality Problems

| ID | Problem | Repos affected | Impact | Fixability |
|---|---|---|---|---|
| **Q1** | DI registrations table: multi-line lambdas, duplicates, `Extension /?` noise | eShop, Vertical | Severe — makes section unreadable | Easy (render-time truncation + dedup) |
| **Q2** | Architecture overview is duplicate project list | All | High — wastes tokens, no new info | Easy (use ProjectLayerMap) |
| **Q3** | Architecture style wrong (eShop=Microservices, Vertical=CleanArch) | eShop, Vertical | High — wrong context framing | Medium (style scorer changes) |
| **Q4** | Controller actions all show route `/` | eShop | High — endpoint table misleading | Medium (conventional route derivation) |
| **Q5** | Related types: flat comma-separated list | All | High — not scannable by LLM | Medium (type kind + key relation per entry) |
| **Q6** | Duplicate endpoints across projects | Vertical | Medium — LLM sees same endpoint twice | Easy (dedup by method+route+handler) |
| **Q7** | Middleware duplicate across projects | eShop | Medium — UseX appears 3× | Easy (dedup by type+kind) |
| **Q8** | No focus-less relevance baseline | All (no --around) | Medium — arbitrary type cuts | Medium (layer/type/accessibility scoring) |
| **Q9** | `<dynamic>` route shows field reference name | Vertical | Medium — field name is more useful | Easy (show field name instead) |
| **Q10** | Library output has no structure | AutoMapper | High — 145 names useless | Hard (rethink library renderer) |
| **Q11** | Lambda endpoints don't boost parameter types | eShop | Medium — relevant services not promoted | Medium (parse lambda params) |
| **Q12** | Aspire signal not detected in eShop | eShop | Low-medium (AspireExtractor never runs) | Easy (check package name prefix) |
| **Q13** | `UseCases` layer not classified | Vertical | Low (maps to Unknown) | Trivial (add to LayerClassifier) |
| **Q14** | Signal confidence not shown in header | All | Low | Trivial |

---

## 5. Key Code Observations

### EndpointExtractor (`EndpointExtractor.cs`)

- Good: phases 1-4 well structured, MapGroup chain resolution working
- Gap: `FindHandler` returns the raw argument expression for lambdas — the handler info stored in `EndpointDetection.HandlerType` for a lambda is the entire lambda expression string. This is what causes the unreadable 80+ char `HandlerType` values.
- Gap: handler resolution for `IdentifierNameSyntax` (method reference like `GetOrder`) correctly returns the name, but the `HandlerType` is also set to the raw expression string. Separate `HandlerType` (the class that owns it) from `HandlerMethod` (the method name) more cleanly.
- Gap: FE route field references (`ProductsApiRoute.GetById`) produce `<dynamic>` but could produce `ProductsApiRoute.GetById` as the route template value.

### MarkdownRenderer (`MarkdownRenderer.cs`)

- `AppendRelatedTypesByLayer`: simple `string.Join(", ", names)` — no type context, no limit, no order by relevance.
- `AppendNonObviousWiring`: no deduplication of DI or middleware entries, no truncation of implementation strings, no grouping by project.
- `AppendArchitectureOverview`: just lists project names — `model.Projects.Select(p => p.Name)`.
- `AppendProfileAndTokens`: hardcodes `"focused"` as the profile text regardless of actual profile.
- No "Public API Surface" section for library-mode output.

### PatternRelevancePruner (`PatternRelevancePruner.cs`)

- Detection lookup is by type name (not FQ): `detectionsByTypeName[type.Name]`
- Lambda endpoint `HandlerType` is a raw expression string → never matches any type
- Library mode scoring runs *after* test type pruning — correct order
- Library mode is triggered by `!HasWebSignals(model)` — but eShop has web signals so this never fires even though half the types are MAUI viewmodels that are not web-relevant
- `TestPrefixes`/`TestSuffixes` miss many test-like names: `Example`, `DifferentProfiles`, `TestProblem`, `Bug_*` suffix classes

### TokenBudgetEnforcer (`TokenBudgetEnforcer.cs`)

- `EstimateTokenCost` counts chars of names/signatures + SourceBody, divides by 3
- This does NOT account for how the type will be rendered — the DI table rows, middleware rows, and flat type name listing have very different token costs per type
- When `SourceBody` is populated (Full profile), a 200-line class body drastically inflates the per-type estimate
- No minimum floor for "types that have detections should always survive regardless of budget" — a highly relevant type with long signatures could be cut before an irrelevant type with a short name

### LayerClassifier (`LayerClassifier.cs`)

- Path/name heuristics are good but miss: `UseCases`, `Contracts`, `Abstractions`, `Host`, `AppHost`, `Services` (ambiguous — could be Application or Infrastructure)
- `ClassifyByPackages` checks FastEndpoints → `Api`, MediatR → `Application`, EFCore → `Persistence`. This is correct but limited. Many projects have multiple packages.
- No concept of project *role*: AppHost/Aspire orchestration projects should be a separate layer (`Orchestration`)
- The `Testing` layer is correctly classified but types from Testing layer are supposed to be pruned — however the type-to-layer assignment goes through `SharedAnalysisContext.ProjectLayerMap[project.Name]` and the type's layer is set during `SyntaxStructureExtractor`. If a project isn't in the map yet (concurrency timing), types get `Unknown`.

### ArchitectureStyleDetector

Not read yet but the eShop/VerticalSlice misclassification suggests the scorer overweights the `minimal-apis` signal. The `Microservices` style score should trigger when: many projects + AppHost/Aspire + each project has its own `Program.cs`. The `CleanArchitecture` style should trigger when: layered project names (Domain, Application, Infrastructure, Web/Api) + MediatR.
