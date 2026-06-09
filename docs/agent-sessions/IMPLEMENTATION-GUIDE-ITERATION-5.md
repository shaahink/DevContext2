# DevContext v2 — Iteration 5 Implementation Guide

**Audience**: New AI coding session. Read `docs/ITERATION-5-ANALYSIS.md` before this document.  
**Focus**: Output quality on real codebases. Every task here maps to a concrete problem in the iter4 benchmark outputs.  
**Branch target**: `develop`  
**Tests**: 134 passing going in. Do not reduce this.  
**Design reference**: `DevContext-v2-Design.md` (do not break contracts)

---

## 0. Orientation: What This Iteration Is About

Iteration 4 fixed *detection* — we can find endpoints, signals, types. Iteration 5 fixes *output* — what the LLM actually reads needs to be useful.

The four benchmark repos expose three categories of output failure:

**Category A — Wrong or misleading information** (eShop shows `MinimalApi` instead of `Microservices`; controller routes all show `/`)

**Category B — Noise overwhelming signal** (DI table has 121 entries including 94 useless `Extension/?` rows; middleware shows the same `UseDefaultOpenApi` 3× from 3 projects; eShop output has 50+ type names in one comma-separated line)

**Category C — Missing structure** (Architecture overview is a duplicate of the project list already in the header; Related types section has no type kind, no key relationships, no limit)

All 14 problems in `ITERATION-5-ANALYSIS.md §4` are addressed by tasks below, grouped to minimize context switching. The full PR sequence is at the end.

---

## Non-Negotiables (Same as Prior Iterations)

- `DevContext.Core` has zero deps on Spectre, Serilog, MSBuild workspace
- All async: `ConfigureAwait(false)`, CancellationToken everywhere
- Nullable enabled, zero `!` suppressions without a comment
- `TreatWarningsAsErrors` on — zero new warnings
- No generic extractors reading `model.Architecture.Has(...)` before signal seal
- All public API gets an XML doc comment
- Failing test written before the code change it covers

---

## GROUP A — Architecture Detection (Category A, highest credibility impact)

### Task A1: Fix `Aspire` signal detection in `DependencyExtractor`

**Problem**: eShop has `eShop.AppHost` referencing Aspire packages but the `aspire` signal is absent. This means `ArchitectureStyleDetector.ScoreMicroservices` never fires (it gates on the Aspire signal), so eShop is scored as `MinimalApi` instead of `Microservices`.

**Root cause**: Find out what package name eShop actually references. Aspire packages are named `Microsoft.Hosting.Aspire`, `Aspire.Hosting.*`, `Microsoft.Extensions.ServiceDiscovery`, etc. The current package map likely checks for `"Aspire"` in the name — verify the exact package name in the eval repo.

```bash
# Check what's in eShop AppHost csproj
grep -r "Aspire\|aspire" eval-repos/eShop/src/eShop.AppHost/ --include="*.csproj"
```

Add any missing Aspire package patterns to `DependencyExtractor.PackageSignalMap`. Common patterns:
- `Aspire.Hosting.*` — AppHost orchestration
- `Aspire.Client.*` — service client
- `Microsoft.Extensions.ServiceDiscovery` — Aspire service discovery
- `eShop.ServiceDefaults` — Aspire service defaults (project-to-project reference, needs different handling)

For project-to-project references that imply Aspire: if a project references a project named `*.ServiceDefaults` or `*.AppHost`, register the `aspire` signal.

**Test**: 
```csharp
[Fact]
public async Task DependencyExtractor_DetectsAspireSignal_FromHostingPackage()
{
    var fs = new FakeFileSystem();
    fs.AddFile("AppHost.csproj", """
        <Project Sdk="Microsoft.NET.Sdk">
          <ItemGroup>
            <PackageReference Include="Aspire.Hosting.AppHost" Version="9.0.0" />
          </ItemGroup>
        </Project>
        """);
    // ... assert aspire signal detected
}
```

---

### Task A2: Fix `ArchitectureStyleDetector` — Microservices and CleanArchitecture scoring

**File**: `src/DevContext.Core/Extractors/Generic/ArchitectureStyleDetector.cs`

**Problem 1**: `ScoreMicroservices` requires the `aspire` signal. If Aspire isn't detected (Task A1 not done yet, or non-Aspire microservices), microservices never scores. Add a project-count heuristic that doesn't require Aspire:

```csharp
private static void ScoreMicroservices(
    IReadOnlyDictionary<string, FeatureSignal> signals,
    int projectCount,
    string[] projectNames,
    Dictionary<ArchitectureStyle, (float, string)> scores)
{
    float score = 0f;
    var via = new List<string>();

    // Aspire is strong evidence of microservices
    if (signals.TryGetValue(ArchitectureSignals.Keys.Aspire, out var aspire) && aspire.Detected)
    {
        score += 0.6f;
        via.Add("Aspire");
    }

    // Many API projects is evidence of microservices
    var apiProjectCount = projectNames.Count(n => n.EndsWith(".api") || n.EndsWith("api") || n.EndsWith("service"));
    if (apiProjectCount >= 3)
    {
        score += Math.Min(0.4f, apiProjectCount * 0.1f);
        via.Add($"{apiProjectCount} API/service projects");
    }

    // Large project count without module naming
    if (projectCount >= 8 && score == 0)
        score += 0.2f;

    if (score >= 0.4f)
        scores[ArchitectureStyle.Microservices] = (Math.Min(score, 0.95f), string.Join("+", via));
}
```

**Problem 2**: `ScoreCleanArchitecture` requires the `mediatr` signal. VerticalSlice has MediatR but also FastEndpoints. The `ScoreVerticalSlices` only scores 0.7 for FastEndpoints alone — but `ScoreCleanArchitecture` with MediatR + 3 layers scores higher. Let's check VerticalSlice: it has `fast-endpoints` (100%) and `fluentvalidation` (100%) but is `minimal-apis` (80%) — MediatR is absent, so `CleanArchitecture` never scores.

Fix: `ScoreCleanArchitecture` should also trigger from layer-named projects alone, without requiring MediatR:
```csharp
private static void ScoreCleanArchitecture(...)
{
    var hasDomain = projectNames.Any(n => n.Contains("domain") || n.Contains(".core"));
    var hasApplication = projectNames.Any(n => n.Contains("application") || n.Contains("usecases") || n.Contains("use-cases"));
    var hasInfrastructure = projectNames.Any(n => n.Contains("infrastructure") || n.Contains("persistence"));
    
    var layerCount = (hasDomain ? 1 : 0) + (hasApplication ? 1 : 0) + (hasInfrastructure ? 1 : 0);
    if (layerCount < 2) return;
    
    var hasMediatR = signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected;
    var hasFastEndpoints = signals.TryGetValue(ArchitectureSignals.Keys.FastEndpoints, out var fe) && fe.Detected;
    
    // Layer structure is the primary signal; MediatR/FE are corroborating evidence
    var baseScore = 0.4f + layerCount * 0.15f;
    if (hasMediatR) baseScore += 0.2f;
    if (hasFastEndpoints) baseScore += 0.1f;  // FE + Clean Architecture is common
    
    scores[ArchitectureStyle.CleanArchitecture] = (Math.Min(baseScore, 0.95f), $"layers:{layerCount}+{(hasMediatR ? "MediatR" : hasFastEndpoints ? "FE" : "structure")}");
}
```

**Problem 3**: `ScoreMinimalApi` assigns the full signal confidence (1.0 for eShop) to MinimalApi style. But `minimal-apis` being present just means the project *uses* minimal API endpoints — it doesn't mean the *architecture* is "MinimalApi" as a style. MinimalApi as a style should be reserved for small, single-project, single-concern apps. Add a suppression condition:

```csharp
private static void ScoreMinimalApi(...)
{
    if (!signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) || !ma.Detected) return;
    
    // Don't score MinimalApi style if this looks like a larger solution
    // (microservices and clean architecture will score higher in those cases)
    var confidence = ma.Confidence * 0.8f;  // slightly reduce raw signal confidence
    
    // Downgrade if many projects — not a "minimal" architecture
    if (projectNames.Length >= 6)
        confidence *= 0.5f;
    
    if (confidence > 0.1f)
        scores[ArchitectureStyle.MinimalApi] = (confidence, $"Signal:{ArchitectureSignals.Keys.MinimalApis}");
}
```

**Tests**:
```csharp
[Fact]
public void Detect_ReturnsCleanArchitecture_WhenLayeredProjectsPresent()
// Project names: Core, Infrastructure, UseCases, Web + fast-endpoints signal

[Fact]
public void Detect_ReturnsMicroservices_WhenManyApiProjectsPresent()
// 8+ projects with API in name, Aspire signal

[Fact]
public void Detect_ReturnsMinimalApi_ForSmallSingleProject()
// 1-3 projects, minimal-apis signal → MinimalApi wins
```

---

### Task A3: Fix `LayerClassifier` — add missing layer patterns

**File**: `src/DevContext.Core/Extractors/Generic/LayerClassifier.cs`

Add to `ClassifyByProjectName` and `ClassifyByPath`:
```csharp
// UseCases → Application
if (lower.Contains("usecases") || lower.Contains("use-cases") || lower.Contains("use_cases"))
    return ArchitectureLayer.Application;

// Contracts / Abstractions → Shared
if (lower.Contains("contracts") || lower.Contains("abstractions") || lower.Contains("interfaces"))
    return ArchitectureLayer.Shared;

// Host / AppHost / Orchestration → new layer or Infrastructure
if (lower.Contains("apphost") || lower.Contains(".host") || lower.Contains("orchestrat"))
    return ArchitectureLayer.Infrastructure;  // or new Orchestration layer if enum allows
```

Also add path patterns for the above.

In `ClassifyByPath`, for AppHost detection, add check for `servicedefaults`:
```csharp
if (lower.Contains("servicedefaults") || lower.Contains("service-defaults"))
    return ArchitectureLayer.Shared;  // not Infrastructure — it's cross-cutting defaults
```

**Note**: Check if `ArchitectureLayer` enum has `Orchestration` or `Shared`. If neither fits, use `Shared` for ServiceDefaults/Contracts and `Infrastructure` for AppHost.

Add tests: verify each new pattern maps correctly.

---

## GROUP B — DI Registrations Section (Category B, most visible noise)

### Task B1: Truncate multi-line lambda implementations

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — `AppendNonObviousWiring`

The core problem: `DiRegistrationDetection.ImplementationType` stores the raw `args[1].Expression.ToString()` from `DiRegistrationExtractor`. For lambdas, this is the full multi-line expression. The fix should happen at **render time** (not extraction time, so the full text is preserved for JSON output).

In `MarkdownRenderer`:
```csharp
private static string TruncateImpl(string impl)
{
    // Collapse whitespace, then truncate
    var normalized = System.Text.RegularExpressions.Regex.Replace(impl.Trim(), @"\s+", " ");
    return normalized.Length > 60 ? normalized[..57] + "..." : normalized;
}
```

Apply in `AppendNonObviousWiring` for the DI registration table:
```csharp
sb.AppendLine($"| {d.Lifetime} | {TruncateImpl(d.ServiceType)} | {TruncateImpl(d.ImplementationType)} |");
```

The 60-char limit keeps table cells readable. The JSON output (`JsonContextRenderer`) should keep the full value.

---

### Task B2: Filter and deduplicate the DI registrations table

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs`

The DI registrations table in `AppendNonObviousWiring` currently dumps all `DiRegistrationDetection` entries. Apply three filters:

**Filter 1 — Remove `Extension` lifetime with `?` implementation** (these are zero-signal rows):
```csharp
// Skip pure extension method calls with no concrete implementation
if (d.Lifetime == "Extension" && (d.ImplementationType is "?" or "" || d.ImplementationType == d.ServiceType))
    continue;
```

**Filter 2 — Collapse duplicates** (same ServiceType + Lifetime appearing in multiple projects):
Group registrations by `(Lifetime, ServiceType)`. If a group has N > 1 entries with the same or similar service type, emit one row with a `×N` indicator:
```csharp
var diGroups = diRegs
    .GroupBy(d => $"{d.Lifetime}:{d.ServiceType}")
    .Select(g => (Registration: g.First(), Count: g.Count()))
    .OrderBy(x => x.Registration.Lifetime)
    .ToList();

foreach (var (reg, count) in diGroups)
{
    var countNote = count > 1 ? $" ×{count}" : "";
    sb.AppendLine($"| {reg.Lifetime}{countNote} | {TruncateImpl(reg.ServiceType)} | {TruncateImpl(reg.ImplementationType)} |");
}
```

**Filter 3 — Cap total DI rows** at a reasonable number. In a 24-project solution, 121 DI entries is noise. Apply a max of 40 rows, preferring `Singleton`, `Scoped`, `Transient` over `Extension`:
```csharp
const int MaxDiRows = 40;
var diRegs = model.Detections.OfType<DiRegistrationDetection>()
    .OrderBy(d => d.Lifetime == "Extension" ? 1 : 0)  // lifetime entries first
    .ThenBy(d => d.ServiceType)
    .Take(MaxDiRows)
    .ToList();
```

Show a footer note if rows were capped: `*... and N more DI registrations*`

---

### Task B3: Deduplicate middleware entries

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs`

In `AppendNonObviousWiring`, middleware list:
```csharp
// Deduplicate: same MiddlewareType from multiple projects → show once with ×N
var middlewareGroups = middleware
    .GroupBy(m => m.MiddlewareType)
    .Select(g => (Middleware: g.OrderBy(m => m.PipelineOrder).First(), Count: g.Count()))
    .OrderBy(x => x.Middleware.PipelineOrder)
    .ToList();

foreach (var (mw, count) in middlewareGroups)
{
    var countNote = count > 1 ? $" ×{count}" : "";
    sb.AppendLine($"| {mw.PipelineOrder} | {mw.MiddlewareType}{countNote} | {mw.Kind} |");
}
```

---

## GROUP C — Architecture Overview Section (Category C)

### Task C1: Replace flat project list with layer-grouped overview

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — `AppendArchitectureOverview`

Current: iterates `model.Projects` and outputs `- {project.Name}`. This is identical to the comma-separated project list in the header block.

**New behavior**: Use `context.Analysis.ProjectLayerMap` to group by layer. Show counts and project names per layer. Exclude Testing layer from the grouped view (or show as a single "Tests (N projects)" line).

```csharp
private static void AppendArchitectureOverview(StringBuilder sb, DiscoveryModel model, RenderOptions options)
{
    sb.AppendLine("## Architecture overview");
    sb.AppendLine();

    if (model.Projects.Length == 0)
    {
        sb.AppendLine("No projects discovered.");
        sb.AppendLine();
        return;
    }

    // Group by layer using the project layer map
    var layerMap = options.ProjectLayerMap;  // passed through RenderOptions
    
    if (layerMap.Count == 0)
    {
        // Fallback: just list projects (no layer info available)
        foreach (var p in model.Projects)
            sb.AppendLine($"- {p.Name}");
        sb.AppendLine();
        return;
    }

    var grouped = model.Projects
        .GroupBy(p => layerMap.TryGetValue(p.Name, out var l) ? l : ArchitectureLayer.Unknown)
        .OrderBy(g => LayerSortOrder(g.Key));

    foreach (var group in grouped)
    {
        if (group.Key == ArchitectureLayer.Testing)
        {
            sb.AppendLine($"- **Testing** ({group.Count()} projects — excluded from analysis)");
            continue;
        }
        var names = string.Join(", ", group.Select(p => p.Name));
        sb.AppendLine($"- **{group.Key}**: {names}");
    }
    sb.AppendLine();
}

private static int LayerSortOrder(ArchitectureLayer layer) => layer switch
{
    ArchitectureLayer.Domain => 0,
    ArchitectureLayer.Application => 1,
    ArchitectureLayer.Infrastructure => 2,
    ArchitectureLayer.Persistence => 3,
    ArchitectureLayer.Api => 4,
    ArchitectureLayer.Presentation => 5,
    ArchitectureLayer.Shared => 6,
    ArchitectureLayer.Unknown => 7,
    ArchitectureLayer.Testing => 8,
    _ => 9
};
```

**`RenderOptions` change**: Add `IReadOnlyDictionary<string, ArchitectureLayer> ProjectLayerMap`. Set in the pipeline when building `RenderOptions` from `context.Analysis.ProjectLayerMap`.

---

## GROUP D — Related Types Section (Category C + B)

### Task D1: Show type context, limit per layer, order by relevance

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — `AppendRelatedTypesByLayer`

**Current**: `string.Join(", ", group.Select(t => t.Name))` — 50+ names per line.

**New behavior**: Show each type with a compact context indicator. Limit to top N per layer by `RelevanceScore + PathProximityScore`.

```csharp
private static void AppendRelatedTypesByLayer(StringBuilder sb, DiscoveryModel model)
{
    sb.AppendLine("## Related types");
    sb.AppendLine();

    const int MaxPerLayer = 15;
    const int MaxLayersShown = 6;  // don't show Testing or extremely sparse layers

    var typedTypes = model.Types.Values
        .Where(t => !t.IsPruned && t.Layer != ArchitectureLayer.Testing)
        .GroupBy(t => t.Layer)
        .OrderBy(g => LayerSortOrder(g.Key))
        .Take(MaxLayersShown);

    var hasContent = false;

    foreach (var group in typedTypes)
    {
        var topTypes = group
            .OrderByDescending(t => t.RelevanceScore + t.PathProximityScore)
            .ThenBy(t => t.Name)
            .Take(MaxPerLayer)
            .ToList();

        var total = group.Count();
        var layerHeader = total > MaxPerLayer
            ? $"**{group.Key}** ({total} types, showing top {MaxPerLayer})"
            : $"**{group.Key}** ({total})";

        sb.AppendLine($"### {layerHeader}");
        sb.AppendLine();

        foreach (var type in topTypes)
        {
            var kindBadge = type.Kind switch
            {
                TypeKind.Interface => "interface",
                TypeKind.Struct => "struct",
                TypeKind.Enum => "enum",
                TypeKind.Delegate => "delegate",
                _ => "class"
            };

            var relations = new List<string>();
            if (type.ImplementedInterfaces.Length > 0)
                relations.Add($"impl: {string.Join(", ", type.ImplementedInterfaces.Take(2))}");
            if (type.BaseTypes.Length > 0 && type.BaseTypes[0] != "object")
                relations.Add($"extends: {type.BaseTypes[0]}");

            var methodCount = type.Methods.Length > 0 ? $", {type.Methods.Length} methods" : "";
            var relationStr = relations.Count > 0 ? $" — {string.Join("; ", relations)}" : "";

            sb.AppendLine($"- `{type.Name}` ({kindBadge}{methodCount}){relationStr}");
        }

        if (total > MaxPerLayer)
            sb.AppendLine($"  *...and {total - MaxPerLayer} more*");

        sb.AppendLine();
        hasContent = true;
    }

    if (!hasContent)
        sb.AppendLine("No types discovered.");
}
```

This produces output like:
```
### Api (47 types, showing top 15)

- `CatalogApi` (class, 14 methods)
- `OrdersApi` (class, 7 methods)
- `GetItemById` (class, 2 methods) — impl: IEndpointHandler
- `CreateOrderCommand` (class, 3 methods) — extends: IRequest<bool>
...and 32 more
```

Much more useful for an LLM than a flat comma list.

---

## GROUP E — Endpoint Table Improvements (Category A + B)

### Task E1: Deduplicate endpoints across projects

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — `AppendEndpoints`

Before rendering the endpoint table, deduplicate:
```csharp
var endpoints = model.Detections.OfType<EndpointDetection>()
    .GroupBy(ep => $"{ep.HttpMethod}:{ep.RouteTemplate}:{ep.HandlerType}")
    .Select(g => g.First())  // keep first occurrence
    .OrderBy(ep => ep.HttpMethod)
    .ThenBy(ep => ep.RouteTemplate)
    .ToList();
```

If two endpoints have the same method+route but different source files (cross-project duplicate), use the `SourceFile` as a tiebreaker.

---

### Task E2: Conventional route derivation for controller actions

**File**: `src/DevContext.Core/Extractors/Specific/ControllerActionExtractor.cs`

When a controller action has `[HttpGet]` or `[HttpPost]` without an explicit route template, derive the conventional route: `/{ControllerName}/{ActionName}` where `ControllerName` strips the `Controller` suffix.

```csharp
private static string DeriveConventionalRoute(string controllerName, string actionName)
{
    // Strip "Controller" suffix for route
    var controller = controllerName.EndsWith("Controller", StringComparison.Ordinal)
        ? controllerName[..^"Controller".Length]
        : controllerName;
    return $"/{controller}/{actionName}";
}
```

In the extractor, when no explicit route template is found on the method or class:
```csharp
var route = explicitRoute 
    ?? DeriveConventionalRoute(controllerName, methodName);
```

Mark it as conventional: add a `bool IsConventional` field to `EndpointDetection` (or encode in the route template as `/{Controller}/{Action}` with a note). If adding a field, update JSON serialization and rendering.

Simpler alternative without a new field: store the derived route as the template with a `(conv.)` suffix indicator in the route string. The renderer can strip the indicator. E.g., `route = "/Account/Login (conv.)"`.

---

### Task E3: Show field reference name for `<dynamic>` FE routes

**File**: `src/DevContext.Core/Extractors/Specific/EndpointExtractor.cs` — Phase 4 (FE Configure() detection)

When the route argument is not a string literal, check if it's a member access expression (field reference):
```csharp
var route = routeArg?.Expression switch
{
    LiteralExpressionSyntax lit => lit.Token.ValueText,
    MemberAccessExpressionSyntax ma => $"[{ma.Name.Identifier.Text}]",  // e.g., [GetById]
    IdentifierNameSyntax id => $"[{id.Identifier.Text}]",               // e.g., [ProductsRoute]
    _ => "<dynamic>"
};
```

Result: instead of `<dynamic>`, route shows `[GetById]` or `[ProductsApiRoute]`. The LLM can then look for that constant in the codebase.

---

## GROUP F — Relevance Improvements (Category B, affects all repos)

### Task F1: Default relevance scoring when no focus points given

**File**: `src/DevContext.Core/Pruning/PatternRelevancePruner.cs`

When no `FocusPoints` are present, all `PathProximityScore` values are 0. `TokenBudgetEnforcer` then sorts by `RelevanceScore` only — types not referenced by detections have score 0 and get cut arbitrarily.

Add a "default relevance" pass in `PatternRelevancePruner` that runs when `context.Analysis.FocusPoints.Count == 0`:

```csharp
private static void ApplyDefaultRelevanceBaseline(DiscoveryModel model)
{
    // When no focus points, give all types a baseline score based on
    // layer priority, accessibility, and structural importance
    foreach (var type in model.Types.Values)
    {
        if (type.IsPruned) continue;

        // Layer priority
        type.RelevanceScore += type.Layer switch
        {
            ArchitectureLayer.Domain => 0.5f,
            ArchitectureLayer.Application => 0.4f,
            ArchitectureLayer.Api => 0.4f,
            ArchitectureLayer.Infrastructure => 0.2f,
            ArchitectureLayer.Persistence => 0.2f,
            ArchitectureLayer.Presentation => 0.3f,
            ArchitectureLayer.Shared => 0.3f,
            ArchitectureLayer.Unknown => 0.1f,
            _ => 0.1f
        };

        // Accessibility: public types are more important for context
        if (type.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
            type.RelevanceScore += 0.2f;

        // Interfaces are more important than implementations for context
        if (type.Kind == TypeKind.Interface)
            type.RelevanceScore += 0.3f;

        // Richer types (more API surface) are more relevant
        var memberCount = type.Methods.Length + type.Properties.Length;
        type.RelevanceScore += Math.Min(0.3f, memberCount * 0.02f);
    }
}
```

Call this at the end of `PruneAsync` when `!context.Analysis.FocusPoints.Any()`. It runs after the detection-based boost pass so detection-referenced types still win.

---

### Task F2: Lambda endpoint handlers should boost parameter service types

**File**: `src/DevContext.Core/Pruning/PatternRelevancePruner.cs`

When `EndpointDetection.HandlerType` is a lambda expression (>60 chars or contains `async`, `=>`), the type name won't match anything in `model.Types`. But the lambda parameters often reference services that are relevant types.

**In the extractor** (`EndpointExtractor.cs`): when `handlerArg` is a `LambdaExpressionSyntax`, extract the parameter types:

```csharp
// In AddEndpoint, when handler is a lambda:
ImmutableArray<string> parameterTypes = [];
if (handlerArg is LambdaExpressionSyntax lambda)
{
    parameterTypes = lambda.ParameterList?.Parameters
        .Select(p => p.Type?.ToString() ?? "")
        .Where(t => !string.IsNullOrEmpty(t))
        .ToImmutableArray() ?? [];
}
```

Store in `EndpointDetection.ParameterTypes` (this field already exists in the record).

**In `PatternRelevancePruner`**: boost types that appear in endpoint parameter types:
```csharp
// In BuildDetectionLookup or a separate pass:
foreach (var ep in model.Detections.OfType<EndpointDetection>())
{
    foreach (var paramType in ep.ParameterTypes)
    {
        if (model.Types.TryGetValue(paramType, out _) || 
            model.Types.Values.Any(t => t.Name == paramType))
        {
            // boost this type
        }
    }
}
```

**Simpler approach** — add to `GetTypeNameEntry` a multi-result version for endpoint parameter types:
```csharp
// Yield one entry per parameter type for endpoints with lambdas
case EndpointDetection ep when ep.ParameterTypes.Length > 0:
    foreach (var pt in ep.ParameterTypes)
        yield return (pt, typeof(EndpointDetection));
    break;
```

Boost value for parameter types: 1.5f (less than handler boost of 5.0f, but still meaningful).

---

## GROUP G — Library Mode Output (AutoMapper — Category C)

### Task G1: Add "Public API Surface" section for library-mode output

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs`

When `model.Architecture.All.Count == 0` (no signals detected) — i.e., library mode — replace the generic "Related types grouped by layer" section with a structured "Public API Surface" section.

```csharp
private static void AppendLibraryApiSurface(StringBuilder sb, DiscoveryModel model)
{
    sb.AppendLine("## Public API Surface");
    sb.AppendLine();

    var surviving = model.Types.Values
        .Where(t => !t.IsPruned)
        .OrderByDescending(t => t.RelevanceScore + t.PathProximityScore)
        .ToList();

    // Interfaces first — they're the contracts
    var interfaces = surviving.Where(t => t.Kind == TypeKind.Interface).ToList();
    if (interfaces.Any())
    {
        sb.AppendLine("### Interfaces (contracts)");
        sb.AppendLine();
        foreach (var iface in interfaces.Take(20))
        {
            var methods = iface.Methods.Take(5).Select(m => $"`{m.Name}({string.Join(", ", m.ParameterTypes)})`");
            sb.AppendLine($"- `{iface.Name}` — {string.Join(", ", methods)}");
        }
        if (interfaces.Count > 20) sb.AppendLine($"*...and {interfaces.Count - 20} more interfaces*");
        sb.AppendLine();
    }

    // Abstract classes — key extensibility points
    var abstractClasses = surviving
        .Where(t => t.Kind == TypeKind.Class && t.Attributes.Any(a => a.Contains("abstract")))
        .ToList();
    // Note: need IsAbstract on TypeDiscovery — check if it exists or use attribute heuristic

    // Public classes, ordered by relevance
    var publicClasses = surviving
        .Where(t => t.Kind == TypeKind.Class && t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
        .Take(30)
        .ToList();
    if (publicClasses.Any())
    {
        sb.AppendLine("### Key classes");
        sb.AppendLine();
        foreach (var cls in publicClasses)
        {
            var impl = cls.ImplementedInterfaces.Length > 0
                ? $" : {string.Join(", ", cls.ImplementedInterfaces.Take(2))}"
                : "";
            var methodCount = cls.Methods.Length > 0 ? $" ({cls.Methods.Length} methods)" : "";
            sb.AppendLine($"- `{cls.Name}`{impl}{methodCount}");
        }
        sb.AppendLine();
    }
}
```

In `RenderAsync`, check if library mode and dispatch to `AppendLibraryApiSurface` instead of `AppendRelatedTypesByLayer`.

Library mode detection in renderer:
```csharp
bool isLibraryMode = !model.Architecture.All.Values.Any(s => s.Detected);
```

---

### Task G2: More aggressive test type filtering for library mode

**File**: `src/DevContext.Core/Pruning/PatternRelevancePruner.cs` — `IsTestOrNoiseType`

AutoMapper output still contains types like `Dto11`, `Example`, `TestProblem`, `DifferentProfiles`. These slip through because they don't match `TestSuffixes` or `TestNamespaceSegments`.

Add to `IsTestOrNoiseType`:
```csharp
// Short/meaningless names common in test fixtures
if (name.Length <= 4 && char.IsUpper(name[0]))  // A, Foo, Bar, etc.
    return true;

// Test-fixture naming patterns
if (name.StartsWith("Example", StringComparison.Ordinal))
    return true;
if (name.StartsWith("Sample", StringComparison.Ordinal) && name.Length < 15)
    return true;

// Dto suffixes
if (name.EndsWith("Dto", StringComparison.Ordinal) && name.Length < 8)
    return true;
```

**More robust**: In library mode, only keep types from the main library project (not test projects, not benchmarks). `CollectTestProjectNames` currently checks for `Tests`/`Test`/`Specs` suffixes. Add:
- `Benchmark` suffix → test/noise
- `TestApp` suffix → test/noise  
- `Sample` suffix → test/noise

---

## GROUP H — Renderer Polish

### Task H1: Fix hardcoded "focused" profile in header

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — `AppendProfileAndTokens`

```csharp
// Current (wrong):
sb.AppendLine($"**Profile**: focused | ...");

// Fix: use actual profile from RenderOptions
sb.AppendLine($"**Profile**: {options.Profile.ToString().ToLowerInvariant()} | ...");
```

`RenderOptions` needs to carry the `ExtractionProfile`. Check what fields it currently has. If `Profile` is missing, add it when `DiscoveryPipeline` builds `RenderOptions`.

### Task H2: Show signal confidence tiers in header

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — `AppendSignals`

Current: `minimal-apis · fluentvalidation`

New: show confidence for signals below 0.9:
```csharp
var signalList = string.Join(" · ", signals.Select(s =>
    s.Confidence >= 0.9f ? s.Key : $"{s.Key} ({s.Confidence:P0})"
));
```

Result: `minimal-apis · fast-endpoints · fluentvalidation (80%)` — the LLM can see which signals are inferred vs definitively detected.

### Task H3: Add project count summary to architecture overview header

In the header (not section), append a note about the Aspire/microservices nature when detected:
```
**Architecture**: Microservices (87% confidence — 5 API projects + Aspire)
```

The `StyleDetectedVia` string on `DiscoveryModel` already stores this — use it.

---

## Git Maintenance

### Branch naming
```
feature/A1-aspire-signal-detection
feature/A2-architecture-style-scorer
feature/A3-layer-classifier-additions
fix/B1-di-lambda-truncation
fix/B2-di-table-dedup-filter
fix/B3-middleware-dedup
feature/C1-arch-overview-layer-grouped
feature/D1-related-types-section-redesign
fix/E1-endpoint-dedup
fix/E2-controller-conventional-routes
fix/E3-dynamic-route-field-name
feature/F1-default-relevance-baseline
feature/F2-lambda-endpoint-param-boost
feature/G1-library-api-surface-section
fix/G2-library-test-type-filtering
fix/H1-H2-H3-renderer-polish
```

### Commit message conventions
```
fix(render): truncate multi-line lambda implementations in DI table
feat(render): replace flat project list with layer-grouped architecture overview
fix(style): downgrade MinimalApi confidence for multi-project solutions
feat(style): score CleanArchitecture from layer names without requiring MediatR
fix(endpoint): derive conventional routes for controller actions without explicit templates
feat(pruning): add default relevance baseline scoring when no focus points given
feat(library): add public API surface section for library-mode output
```

### PR rules
- One logical change per PR
- All 134+ tests must pass
- `dotnet format --verify-no-changes` must pass
- Run eval on at least one relevant benchmark and check output in eval-results/
- Commit eval markdown artifact to `eval/iteration-5-results` branch

### Order (do not skip steps or merge groups out of order)
```
Batch 1 (signal + detection, no rendering changes):
  PR 1: A1 Aspire signal detection
  PR 2: A3 LayerClassifier additions
  
Batch 2 (style detection, depends on signal fixes):
  PR 3: A2 ArchitectureStyleDetector scorer

Batch 3 (rendering — DI table noise, quick wins):
  PR 4: B1 DI lambda truncation
  PR 5: B2 DI table dedup + filter
  PR 6: B3 Middleware dedup

Batch 4 (rendering — structure improvements):
  PR 7: C1 Architecture overview layer-grouped
  PR 8: D1 Related types section redesign
  PR 9: H1+H2+H3 renderer polish

Batch 5 (endpoint quality):
  PR 10: E1 endpoint dedup
  PR 11: E2 controller conventional routes
  PR 12: E3 dynamic route field name

Batch 6 (relevance improvements):
  PR 13: F1 default relevance baseline
  PR 14: F2 lambda endpoint param boost

Batch 7 (library mode):
  PR 15: G2 library test type filtering (low risk, do first)
  PR 16: G1 library API surface section (renderer change, do last)
```

---

## Verification Checklist

Run after all batches merged:

```bash
# 1. Build + test + format
dotnet build && dotnet test && dotnet format --verify-no-changes

# 2. eShop — check architecture detection + section quality
dotnet run --project src/DevContext.Cli -- analyze eval-repos/eShop/eShop.slnx \
  --profile focused --max-tokens 20000 --include-diagnostics \
  -o eval-results/eShop/iter5-focused.md

# Assertions:
# - Architecture: Microservices (not MinimalApi)
# - aspire signal present
# - DI table: no multi-line cells, max 40 rows, no Extension/? entries
# - Middleware: no duplicates
# - Architecture overview: layer-grouped (Api, Domain, etc.), not flat list
# - Related types: shows type kind and relations, not flat comma list
# - Controller routes: /Account/Login not /

# 3. VerticalSlice — check CleanArch detection + FE routes
dotnet run --project src/DevContext.Cli -- analyze eval-repos/VerticalSlice/Clean.Architecture.slnx \
  --profile focused --max-tokens 20000 \
  -o eval-results/VerticalSlice/iter5-focused.md

# Assertions:
# - Architecture: CleanArchitecture (not MinimalApi)
# - No duplicate endpoints
# - FE routes show [FieldName] instead of <dynamic> where applicable
# - UseCases layer correctly classified as Application

# 4. TodoApi — regression test (shouldn't regress)
dotnet run --project src/DevContext.Cli -- analyze eval-repos/TodoApi/TodoApp.sln \
  --profile focused --max-tokens 8000 \
  -o eval-results/TodoApi/iter5-focused.md

# Assertions:
# - Architecture: MinimalApi (small, single project → should keep MinimalApi)
# - 11 endpoints detected (same as before)
# - DI table still shows registrations (not over-filtered)

# 5. AutoMapper — library output quality
dotnet run --project src/DevContext.Cli -- analyze eval-repos/AutoMapper/AutoMapper.slnx \
  --profile focused --max-tokens 20000 \
  -o eval-results/AutoMapper/iter5-focused.md

# Assertions:
# - "Public API Surface" section present (not "Related types grouped by layer")
# - Interfaces section shows IMapper, IConfigurationProvider, IProfileExpression etc.
# - No Dto11, Example, TestProblem type names in output
# - Architecture still "Not detected" (correct for library)

# 6. Piped output still clean
dotnet run --project src/DevContext.Cli -- analyze eval-repos/TodoApi/TodoApp.sln 2>/dev/null | head -5
# Should be "## DevContext --" not ANSI escape codes
```

---

## Key Architecture Invariants to Preserve

1. `DiscoveryModel.Types` never gains new public setters — `TypeDiscovery` is still only written by `SyntaxStructureExtractor` (via `TryAdd`)
2. `ArchitectureStyleDetector` is called between Stage 2 and Stage 3 by the pipeline orchestrator — do not turn it into an `IDiscoveryExtractor` (it would need to be Generic, but reads sealed signals)
3. `RenderOptions` additions (Profile, ProjectLayerMap) should have defaults that produce current behavior when not set — no breaking change for the JSON renderer
4. `EndpointDetection.ParameterTypes` already exists in the record — just populate it from lambda parameters

---

## Edge Cases to Test

- **Solution with zero signals** (pure library): library-mode output, no endpoint/MediatR sections
- **Solution with only `controllers` signal** (no minimal-apis): `AppendEndpoints` renders correctly with conventional routes
- **Single-project app** (TodoApi): MinimalApi style score survives the multi-project downgrade
- **FastEndpoints + CleanArchitecture layers**: both signals + layers → CleanArchitecture wins over VerticalSlices
- **DI table with exactly 40 rows**: no "... and N more" footer
- **DI table with 41+ rows**: footer note appears
- **All middleware entries deduplicated** (100% duplicate): still shows the type once, not zero times

---

*This document is the implementation reference for iteration 5. Source: `ITERATION-5-ANALYSIS.md` + iter4 benchmark audit. Design: `DevContext-v2-Design.md`.*
