# DevContext v2 — Iteration 6: Scenario Usability Analysis & Plan

**Date**: 2026-06-05  
**State**: post-iteration-5, 138 tests green  
**Method**: Walk through 7 real developer scenarios, compare what each needs to what the tool currently produces, then derive concrete tasks.

---

## Part 1 — Scenario Analysis

The tool's purpose is to give an LLM the right context for a specific task. That means the output must change meaningfully based on *what the developer is trying to do*. Right now it doesn't — all scenarios produce near-identical output. This section documents what each scenario actually requires and the gap.

---

### Scenario 1: Debug a broken endpoint

**Developer intent**: "POST /api/orders is returning 500. I need to understand everything this endpoint touches."

**Typical command**:
```
devcontext . --scenario debug-endpoint --around CreateOrder
```

**What the developer needs in the output**:
1. The specific endpoint row: method, route, handler, auth, source line
2. The handler type definition: class fields, constructor dependencies (injected services)
3. The MediatR command it dispatches (if applicable) and its properties
4. The command handler: what it does (at least method signatures)
5. The DI registrations for the services this handler depends on
6. Any middleware that intercepts this endpoint (auth, rate limiting, exception handling)
7. The EF entities the handler touches (DbContext, DbSet, entity shape)
8. Hint: "Re-run with `--profile debug` to see full call graph"

**What the tool currently produces**:
- ❌ All 44 endpoints, not narrowed to the relevant one
- ❌ Scenario name: "Architecture Overview" (hardcoded, ignores `--scenario`)
- ❌ No `MaxSurvivingTypes=20` enforced — 439 types survive instead
- ❌ No inline handler type definition (only type name in Related types flat list)
- ❌ No connection endpoint → handler → services
- ❌ EF entities detected (`EfEntityDetection` populated by `EfCoreExtractor`) but **never rendered** — section does not exist in `MarkdownRenderer`
- ❌ `AuthAttributes` column always shows `-` — never populated by `EndpointExtractor`
- ❌ Profile: Focused → no call graph, but no warning to user to try `--profile debug`
- ✅ MediatR handlers table present (useful)
- ✅ Middleware pipeline present (useful)
- ✅ Source file:line in endpoint table (useful)

**Gap severity**: High. The user can find the endpoint row and the command handler in separate tables but can't see the connection, can't see the entity model, and gets 439 types of noise around the 10 types they actually care about.

---

### Scenario 2: Add a new feature similar to an existing one

**Developer intent**: "I need to add a `GET /api/catalog/items/{id}/reviews` endpoint. Show me how the existing `GetItemById` endpoint is structured so I can follow the same pattern."

**Typical command**:
```
devcontext . --scenario add-similar-feature --around GetItemById
```

**What the developer needs**:
1. The exact endpoint definition: route, handler class, source file
2. The handler class inline: what it extends/implements, constructor, key method signatures
3. The query/command it uses: what request type, what response type
4. The handler for that query: implementation structure
5. How the endpoint is registered: the `MapGroup` it belongs to, which extension method adds it
6. The pattern to replicate: names, file location, method shape

**What the tool currently produces**:
- ❌ ALL endpoints in a 44-row table — user must find their reference row manually
- ❌ No inline type definition for `GetItemById` — just a name in the Related types list
- ❌ No `--around` focus narrowing visible in output (focus parsing exists but doesn't change sections)
- ❌ No "Entry" section showing the focused type detail (RequiredSections: ["Entry", "Related types"] defined but not rendered)
- ❌ `MaxSurvivingTypes=40` not enforced — 439 types in output
- ✅ Endpoint row has source file:line so user can jump to it
- ✅ Group column shows the MapGroup prefix

**Gap severity**: Medium-high. The user can find the reference endpoint and jump to its source, but the output gives them 400+ types of noise and no structured view of the pattern to follow.

---

### Scenario 3: Trace a message/event flow

**Developer intent**: "When a user places an order, what chain of events happens? I need to trace it from POST /orders to the order confirmation being stored."

**Typical command**:
```
devcontext . --scenario trace-message-flow --around CreateOrder
```

**What the developer needs**:
1. The entry point: `POST /api/orders` → dispatches `CreateOrderCommand`
2. The command handler: `CreateOrderCommandHandler` — what domain methods it calls, what events it raises
3. Domain events raised: `OrderStartedDomainEvent` → which handlers handle it
4. Integration events: `OrderStatusChangedToSubmittedIntegrationEvent` → published to bus → consumed by what
5. Message consumers: `MessageConsumerDetection` entries — who receives the integration events
6. State machine / saga if applicable (e.g. in MassTransit)

**What the tool currently produces**:
- ❌ Scenario name: "Architecture Overview"
- ❌ `RequiredSections: ["Domain signals", "Call graph"]` — neither section exists in the renderer
- ❌ MediatR handlers table shows all handlers (useful) but as a flat unconnected list — no `CreateOrderCommand → OrderStartedDomainEvent → ValidateOrAddBuyerAggregate` chain
- ❌ `EventBusExtractor` detects `MessageConsumerDetection` but **nothing in `MarkdownRenderer` renders this detection type** — complete gap
- ❌ No "Domain events" section showing which notifications are raised by which commands
- ❌ No flow chain connecting the pieces

**Gap severity**: Critical. The `trace-message-flow` scenario produces output that does not contain the flow. The user gets a flat table of handlers with no connections shown. `MessageConsumerDetection` is detected but never displayed.

---

### Scenario 4: Modify the middleware pipeline

**Developer intent**: "I need to add rate limiting middleware. Show me the current pipeline so I know where to insert it."

**Typical command**:
```
devcontext . --scenario modify-middleware
```

**What the developer needs**:
1. The current middleware pipeline in order, clearly separated by project/application
2. What's already registered (UseRateLimiter? UseRouting? UseAuthentication?)
3. Which project each middleware belongs to (important in microservices)
4. The startup file that controls each application's pipeline
5. The DI registrations for existing middleware services

**What the tool currently produces**:
- ❌ Scenario name: "Architecture Overview"
- ❌ Middleware table mixes entries from ALL projects' Program.cs files with the same order numbers
- ❌ No project label — `UseExceptionHandler` appears twice with no indication which app it's from
- ❌ DI registrations are still noisy (still shows `Extension/?` entries in bulk)
- ✅ `RequiredSections: ["Entry", "Non-obvious wiring"]` — at least Non-obvious wiring is always rendered
- ✅ Order numbers are present (per-file, not globally meaningful)
- ✅ Source file:line would solve the "which project" problem — **not shown for middleware**

**Gap severity**: Medium. The middleware section is actually the best scenario match — the data is there, it just needs deduplication and per-project attribution.

---

### Scenario 5: Understand the data model before schema changes

**Developer intent**: "I'm adding a `reviews_count` column to the catalog items table. Show me the entity model and migration history."

**Typical command**:
```
devcontext . --scenario architecture --around CatalogContext
```

**What the developer needs**:
1. The `CatalogContext` class: which `DbSet<T>` properties it has
2. The entity types: `CatalogItem`, `CatalogBrand`, `CatalogType` — their properties and relationships
3. Entity configurations: what `IEntityTypeConfiguration<T>` classes configure the schema
4. Current migrations: what's in the latest migration

**What the tool currently produces**:
- ❌ **`EfEntityDetection` entries are populated by `EfCoreExtractor` but `MarkdownRenderer` has NO section for them** — complete silence
- ❌ No entity model section at all
- ❌ CatalogContext might appear in Related types as a name, but no DbSet properties shown
- ✅ `efcore` signal detected (shown in header)
- ✅ Source file:line on types (if they survive pruning)

**Gap severity**: Critical. The EF entity detection is implemented and working but the data is invisible. This is a pure renderer gap.

---

### Scenario 6: Security audit — find unprotected endpoints

**Developer intent**: "Before our security review, I need a list of all endpoints and whether they're protected by authorization."

**Typical command**:
```
devcontext . --scenario architecture
```

**What the developer needs**:
1. All endpoints with their auth attributes clearly populated
2. Which endpoints have `[Authorize]`, `[RequireAuthorization("policy")]`, `[AllowAnonymous]`
3. Which require specific roles or policies
4. Global auth filters vs per-endpoint

**What the tool currently produces**:
- ❌ Auth column always shows `-` — `EndpointExtractor` never populates `AuthAttributes`
- ❌ No distinction between "no auth attribute" and "explicitly anonymous"
- ❌ No global auth filter detection (e.g. `app.MapGroup("/api").RequireAuthorization()` applies to all routes in that group)
- ✅ Endpoints table structure exists (right format, wrong data)
- ✅ Auth column exists in the table definition

**Gap severity**: High. The endpoint table has an Auth column that has never had data in it. This is a pure extraction gap in `EndpointExtractor`.

---

### Scenario 7: Understand how to extend the pipeline behavior (CQRS behaviors)

**Developer intent**: "I need to add a caching pipeline behavior for our query handlers. Show me how the existing `LoggingBehavior` is structured and how behaviors are registered."

**Typical command**:
```
devcontext . --scenario add-similar-feature --around LoggingBehavior
```

**What the developer needs**:
1. The `LoggingBehavior` class definition: interface it implements, generic constraints, key methods
2. How it's registered in DI: `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))`
3. What other behaviors exist (ordering matters in MediatR)
4. The MediatR pipeline configuration: `AddMediatR`, `AddBehavior` calls

**What the tool currently produces**:
- ❌ DI registration for `LoggingBehavior` shows as `Extension | AddTransient | ?` — no type details
- ❌ No inline class definition for `LoggingBehavior` (just a name in Related types if it survived)
- ❌ No "Entry" section showing the focused type
- ✅ `LoggingBehavior` will appear in Related types if its relevance score is high enough
- ✅ `mediatr` signal detected

**Gap severity**: Medium. The developer can find the source file from Related types and open it manually, but the tool doesn't give them the class definition or DI wiring in a structured way.

---

## Part 2 — Cross-Scenario Gap Inventory

Synthesizing across all 7 scenarios, the gaps fall into 4 categories:

### Category 1 — Missing Renders (data exists, not displayed)

| Detection Type | Extractor | Status |
|---|---|---|
| `EfEntityDetection` | `EfCoreExtractor` | Detected, **never rendered** |
| `MessageConsumerDetection` | `EventBusExtractor` | Detected, **never rendered** |
| `AuthAttributes` on `EndpointDetection` | `EndpointExtractor` | Field exists, **never populated** |
| Scenario `DisplayName` | `ScenarioRegistry` | Defined, renderer hardcodes "Architecture Overview" |

### Category 2 — Scenario System Dead on Arrival

The scenario system has complete configuration in `ScenarioRegistry` but zero enforcement:

| Config field | Where defined | Who uses it |
|---|---|---|
| `MaxSurvivingTypes` | `PruningConfig` | Nobody — `TokenBudgetEnforcer` uses token budget only |
| `RequiredSections` | `Scenario` | Nobody — `MarkdownRenderer` always renders all sections |
| `MaxPathDistance` | `PruningConfig` | Nobody — `PathProximityPruner` reads `ExtractionOptions`, not `PruningConfig` |
| `MaxCallDepth` | `PruningConfig` | `CallGraphExtractor` but only in Debug/Full profile |
| `EnablePatternBoost` | `CompressionConfig` | Nobody |
| `AggressiveTruncation` | `CompressionConfig` | Nobody |

This means a user running `--scenario debug-endpoint` gets exactly the same output as `--scenario architecture`. The scenario name is cosmetic.

### Category 3 — Context Assembly (connect the dots)

The tool detects isolated facts but doesn't connect them:
- Endpoint `POST /api/orders` dispatches `CreateOrderCommand` (not shown)
- `CreateOrderCommand` is handled by `CreateOrderCommandHandler` (MediatR table shows this)
- `CreateOrderCommandHandler` depends on `IOrderRepository`, `IIntegrationEventService` (not shown)
- `IOrderRepository` is registered as `Scoped` → `OrderRepository` (DI table shows this if you find it)
- `OrderRepository` uses `OrderingContext` which has `DbSet<Order>` (EF table not shown)

Each piece of this chain is separately detectable. None of it is assembled as a chain.

### Category 4 — UX / CLI Discoverability

- `--around` is the most powerful feature for focused output, but there's no mention of it in `--dry-run` output or anywhere in the rendered context. Users who don't read the docs never use it.
- Profile implications are invisible: `--scenario debug-endpoint` silently produces no call graph on `Focused` profile.
- No "you got partial results because" guidance in the output.

---

## Part 3 — Iteration 6 Task Plan

Tasks are ordered by scenario impact, not technical complexity. Always: failing test first.

---

### GROUP A — Fix what's completely broken/silent (P0)

#### Task A1: Render EF entities section

**Why**: EfCoreExtractor is working. Data exists. Nothing shows it to the user. This is the fastest high-impact fix.

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs`

Add `AppendEfEntities` called after `AppendMediatRHandlers`:

```csharp
private static void AppendEfEntities(StringBuilder sb, DiscoveryModel model)
{
    var entities = model.Detections.OfType<EfEntityDetection>().ToList();
    if (entities.Count == 0) return;

    sb.AppendLine("## Data model (EF Core)");
    sb.AppendLine();

    // Group by DbContext
    var byContext = entities.GroupBy(e => e.DbContextType).OrderBy(g => g.Key);
    foreach (var group in byContext)
    {
        sb.AppendLine($"### `{group.Key}`");
        sb.AppendLine();
        sb.AppendLine("| Entity | Aggregate root | Key properties |");
        sb.AppendLine("|--------|---------------|----------------|");
        foreach (var e in group.OrderBy(e => e.EntityType))
        {
            var keys = e.KeyProperties.Length > 0 ? string.Join(", ", e.KeyProperties) : "—";
            var agg = e.IsAggregate ? "✓" : "—";
            sb.AppendLine($"| `{e.EntityType}` | {agg} | {keys} |");
        }
        sb.AppendLine();
    }
}
```

**Test**: Add golden test case with a DbContext in fixture, assert EF entities section appears.

---

#### Task A2: Render message consumer section

**Why**: `EventBusExtractor` detects `MessageConsumerDetection`. The `trace-message-flow` scenario is completely empty without this.

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs`

Add `AppendMessageConsumers` called after `AppendEfEntities`:

```csharp
private static void AppendMessageConsumers(StringBuilder sb, DiscoveryModel model)
{
    var consumers = model.Detections.OfType<MessageConsumerDetection>().ToList();
    if (consumers.Count == 0) return;

    sb.AppendLine("## Message consumers");
    sb.AppendLine();
    sb.AppendLine("| Bus | Message type | Consumer |");
    sb.AppendLine("|-----|-------------|---------|");
    foreach (var c in consumers.OrderBy(c => c.BusKind).ThenBy(c => c.MessageType))
        sb.AppendLine($"| {c.BusKind} | `{c.MessageType}` | `{c.ConsumerType}` |");
    sb.AppendLine();
}
```

---

#### Task A3: Populate `AuthAttributes` in EndpointExtractor

**Why**: Auth column has always been empty. Security audits are impossible.

**File**: `src/DevContext.Core/Extractors/Specific/EndpointExtractor.cs`

For Minimal API endpoints: look for chained calls after `MapGet/Post/etc`:
```csharp
// On app.MapGet("/route", handler).RequireAuthorization("Policy")
//                                  ^^^^^^^^^^^^^^^^^^^
// Look for method chains after the map call
private static ImmutableArray<string> ExtractAuthFromChain(InvocationExpressionSyntax mapInvocation)
{
    var auth = new List<string>();
    var node = mapInvocation.Parent;
    
    while (node is MemberAccessExpressionSyntax ma && ma.Parent is InvocationExpressionSyntax chainInvoke)
    {
        var methodName = ma.Name.Identifier.ValueText;
        if (methodName == "RequireAuthorization")
        {
            var policyArg = chainInvoke.ArgumentList.Arguments.FirstOrDefault();
            auth.Add(policyArg?.Expression is LiteralExpressionSyntax lit
                ? $"[Authorize({lit.Token.ValueText})]"
                : "[Authorize]");
        }
        else if (methodName == "AllowAnonymous")
        {
            auth.Add("[AllowAnonymous]");
        }
        node = chainInvoke.Parent;
    }
    
    return [.. auth];
}
```

For MapGroup auth: if a MapGroup has `.RequireAuthorization()` chained, all routes in that group inherit it. Store in `EndpointDetection.GroupAuth` (new nullable field) or encode in `AuthAttributes` as `[Authorize] (group)`.

For controller actions: scan method and class attributes for `[Authorize]`, `[AllowAnonymous]`:
```csharp
private static ImmutableArray<string> ExtractAuthAttributes(MemberDeclarationSyntax member, ClassDeclarationSyntax cls)
{
    var attrs = new List<string>();
    
    // Check method attributes
    var methodAttrs = member.AttributeLists.SelectMany(a => a.Attributes)
        .Where(a => a.Name.ToString() is "Authorize" or "AllowAnonymous" or "RequireAuthorization");
    
    // Check class-level attributes too (inherits to all actions)
    var classAttrs = cls.AttributeLists.SelectMany(a => a.Attributes)
        .Where(a => a.Name.ToString() is "Authorize" or "AllowAnonymous");
    
    // Class-level overrides only if method doesn't override it
    // ...
}
```

**Test**: fixture with `[Authorize]`, `[AllowAnonymous]`, and `.RequireAuthorization()` chain → assert auth column populated correctly.

---

#### Task A4: Use scenario display name in renderer header

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs:42`

One-line fix. The model needs to carry the active scenario name. Pass it through `RenderOptions`:

```csharp
// RenderOptions: add string ScenarioDisplayName = "Architecture Overview"
// In MarkdownRenderer.AppendHeader:
var scenarioName = options.ScenarioDisplayName;
sb.AppendLine($"## DevContext -- {scenarioName} on {entryName}");
```

In `DiscoveryPipeline.BuildRenderOptions`: populate from `context.ActiveScenario.DisplayName`.

---

### GROUP B — Enforce the scenario system (P0-P1)

The scenario config is complete. Nothing reads it. These tasks wire it up.

#### Task B1: Enforce `MaxSurvivingTypes` in `TokenBudgetEnforcer`

**File**: `src/DevContext.Core/Pruning/TokenBudgetEnforcer.cs`

After the token-budget pruning loop, add a count-based hard limit if configured in the scenario:

```csharp
// After existing token-budget loop:
var maxTypes = context.Options.MaxSurvivingTypes;  // new field on ExtractionOptions, set from scenario
if (maxTypes > 0)
{
    var survivors = model.Types.Values
        .Where(t => !t.IsPruned)
        .OrderByDescending(t => t.PathProximityScore + t.RelevanceScore)
        .ToList();
    
    if (survivors.Count > maxTypes)
    {
        foreach (var type in survivors.Skip(maxTypes))
        {
            type.IsPruned = true;
            model.PrunedTypeIds.Add(type.Id);
        }
        model.PruningNotes.Add($"TokenBudgetEnforcer: capped at {maxTypes} types (scenario limit)");
    }
}
```

Add `MaxSurvivingTypes` to `ExtractionOptions` (default 0 = unlimited). Populate from scenario in `AnalyzeCommand` before building context.

```csharp
// In AnalyzeCommand, after resolving scenario:
var scenario = scenarios[scenarioName];
var options = new ExtractionOptions
{
    // ...existing fields...
    MaxSurvivingTypes = scenario.Pruning.MaxSurvivingTypes
};
```

---

#### Task B2: Enforce `RequiredSections` in `MarkdownRenderer`

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs`

`RequiredSections` defines which sections are shown. Change from "always show all sections" to "show sections that match":

```csharp
// Add to RenderOptions:
public ImmutableArray<string> RequiredSections { get; init; } = [];  // empty = show all

// In RenderAsync:
bool ShouldShow(string sectionName) =>
    options.RequiredSections.IsEmpty || options.RequiredSections.Contains(sectionName);
```

Apply to each `Append*` call:
```csharp
if (ShouldShow("Endpoints")) AppendEndpoints(sb, model);
if (ShouldShow("MediatR Handlers")) AppendMediatRHandlers(sb, model);
if (ShouldShow("Data model")) AppendEfEntities(sb, model);
if (ShouldShow("Message consumers")) AppendMessageConsumers(sb, model);
if (ShouldShow("Non-obvious wiring")) AppendNonObviousWiring(sb, model);
if (ShouldShow("Related types")) AppendRelatedTypesByLayer(sb, model);
// Architecture overview and header are always shown
```

Map `RequiredSections` string → method: the section names in `ScenarioRegistry` must match exactly the strings used here. Fix the scenario configs now that section names are enforced:

```csharp
// ScenarioRegistry — fix section names to match renderer:
["debug-endpoint"] = new()
{
    RequiredSections = ["Endpoints", "MediatR Handlers", "Non-obvious wiring", "Related types"]
    // Removed: "Entry" (doesn't exist yet), "Call graph" (requires Deep)
},
["trace-message-flow"] = new()
{
    RequiredSections = ["MediatR Handlers", "Message consumers", "Data model"]
    // Was: "Domain signals", "Call graph" — neither exists
},
["modify-middleware"] = new()
{
    RequiredSections = ["Non-obvious wiring"]  // middleware pipeline is in Non-obvious wiring
},
["add-similar-feature"] = new()
{
    RequiredSections = ["Endpoints", "MediatR Handlers", "Related types"]
},
["harden-di"] = new()
{
    RequiredSections = ["Non-obvious wiring", "Related types"]
}
```

**Note**: Section name mismatches will silently produce empty output. Add a startup validation that all `RequiredSections` strings match a known section name.

---

#### Task B3: Propagate scenario pruning config to context

The `PruningConfig.MaxPathDistance` is defined but `PathProximityPruner` uses `ExtractionOptions` directly. Add a bridging step:

```csharp
// ExtractionOptions: add int MaxPathDistance = 0 (0 = unlimited)
// In AnalyzeCommand:
var options = new ExtractionOptions
{
    MaxPathDistance = scenario.Pruning.MaxPathDistance > 0 
        ? scenario.Pruning.MaxPathDistance 
        : int.MaxValue
};

// PathProximityPruner: respect the limit
if (context.Options.MaxPathDistance > 0 && distance > context.Options.MaxPathDistance)
    type.IsPruned = true;
```

---

### GROUP C — Context assembly for debug/feature scenarios (P1)

#### Task C1: Add "Entry" section for focus-point runs

When `--around` is specified and focus points are resolved to types, show an "Entry" section with the focused type's full definition inline. This is the highest-value change for `debug-endpoint` and `add-similar-feature`.

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs`

Add `AppendEntry` called before endpoints:

```csharp
private static void AppendEntry(StringBuilder sb, DiscoveryModel model, RenderOptions options)
{
    if (!options.FocusPoints.Any()) return;  // no --around → skip

    sb.AppendLine("## Entry points");
    sb.AppendLine();

    foreach (var focus in options.FocusPoints)
    {
        // Find the type in the model
        var type = model.Types.Values.FirstOrDefault(t =>
            t.Name == focus.TypeName || t.Id.EndsWith(focus.TypeName ?? ""));
        if (type is null) continue;

        sb.AppendLine($"### `{type.Name}` ({type.Kind}, {type.Layer})");
        sb.AppendLine($"> `{type.Namespace}.{type.Name}` — {type.FilePath}:{type.Methods.FirstOrDefault()?.LineNumber ?? 0}");
        sb.AppendLine();

        // Interfaces implemented
        if (type.ImplementedInterfaces.Length > 0)
            sb.AppendLine($"**Implements**: {string.Join(", ", type.ImplementedInterfaces.Select(i => $"`{i}`"))}");

        // Base type
        if (type.BaseTypes.Length > 0 && type.BaseTypes[0] != "object")
            sb.AppendLine($"**Extends**: `{type.BaseTypes[0]}`");

        // Constructor parameters (injected dependencies)
        var ctors = type.Methods.Where(m => m.Name == ".ctor" || m.Name == type.Name).ToList();
        if (ctors.Any())
        {
            var deps = ctors.First().ParameterTypes;
            if (deps.Length > 0)
                sb.AppendLine($"**Depends on**: {string.Join(", ", deps.Select(d => $"`{d}`"))}");
        }

        // Public methods (the API surface)
        var publicMethods = type.Methods
            .Where(m => m.Accessibility == "public" && m.Name != ".ctor" && m.Name != type.Name)
            .Take(8)
            .ToList();
        if (publicMethods.Any())
        {
            sb.AppendLine();
            sb.AppendLine("**Methods**:");
            foreach (var method in publicMethods)
            {
                var paramStr = string.Join(", ", method.ParameterTypes.Zip(
                    method.ParameterNames, (t2, n) => $"{t2} {n}"));
                sb.AppendLine($"- `{method.ReturnType} {method.Name}({paramStr})`");
            }
        }

        // Source body if available (Deep profile)
        if (type.SourceBody is not null)
        {
            sb.AppendLine();
            sb.AppendLine("```csharp");
            sb.AppendLine(type.SourceBody.Length > 500
                ? type.SourceBody[..500] + "\n// ... truncated"
                : type.SourceBody);
            sb.AppendLine("```");
        }

        sb.AppendLine();
    }
}
```

Add `FocusPoints` to `RenderOptions` — pass from `context.Analysis.FocusPoints`.

**Note**: `MethodSignature` needs `ParameterNames` (not just `ParameterTypes`). Check if that field exists; if not, add it to `MethodSignature` during `SyntaxStructureExtractor` parsing.

---

#### Task C2: Per-project endpoint grouping in the endpoint table

**File**: `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — `AppendEndpoints`

Add source project attribution. The source file path contains the project directory. Extract the project name from it using `model.Projects`:

```csharp
// Build a lookup: file path prefix → project name
var projectByPath = model.Projects
    .OrderByDescending(p => p.Directory?.Length ?? 0)  // longest match first
    .ToDictionary(p => p.Directory ?? "", p => p.Name);

string GetProjectForFile(string filePath) =>
    projectByPath.FirstOrDefault(kv => filePath.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Value
    ?? Path.GetFileNameWithoutExtension(filePath);
```

Group endpoints by project and render with project headers:

```csharp
var endpointsByProject = endpoints.GroupBy(ep => GetProjectForFile(ep.SourceFile));
foreach (var projectGroup in endpointsByProject.OrderBy(g => g.Key))
{
    sb.AppendLine($"**{projectGroup.Key}** ({projectGroup.Count()} endpoints)");
    sb.AppendLine("|...|");
    foreach (var ep in projectGroup) { /* row */ }
    sb.AppendLine();
}
```

This gives a developer working on `Ordering.API` just the ordering endpoints at the top, not mixed with catalog and webhooks.

---

#### Task C3: Endpoint → MediatR handler linkage note

For each endpoint whose handler name matches a pattern `{X}Async.{X}Async` (the eShop pattern where method and class share the name), or whose handler appears in `MediatRHandlerDetection.HandlerType`, add a `→ via MediatR` annotation:

```csharp
// In AppendEndpoints, for each endpoint row:
var mediatRLink = model.Detections.OfType<MediatRHandlerDetection>()
    .FirstOrDefault(mh => ep.HandlerMethod.Contains(mh.RequestType.Replace("Command", "").Replace("Query", "")));

if (mediatRLink is not null)
    handler += $" → `{mediatRLink.RequestType}`";
```

This gives: `CreateOrderAsync.CreateOrderAsync → CreateOrderCommand` in the Handler column — the developer can now see the dispatch chain without a call graph.

---

### GROUP D — DI noise cleanup (P1, still pending from iter 5)

#### Task D1: Final DI registration filtering

The E2E findings confirm the DI table is still 50% noise. Apply the filters from the iteration 5 guide (previously documented, not yet implemented):

```csharp
// In AppendNonObviousWiring:
var diRegs = model.Detections.OfType<DiRegistrationDetection>()
    .Where(d => !(d.Lifetime == "Extension" && (d.ImplementationType is "?" or "")))
    .GroupBy(d => $"{d.Lifetime}:{NormalizeType(d.ServiceType)}")
    .Select(g => (Reg: g.First(), Count: g.Count()))
    .OrderBy(x => x.Reg.Lifetime == "Extension" ? 1 : 0)
    .ThenBy(x => x.Reg.ServiceType)
    .Take(40)
    .ToList();

// Truncate implementation to 60 chars
static string Trunc(string s) {
    var norm = Regex.Replace(s.Trim(), @"\s+", " ");
    return norm.Length > 60 ? norm[..57] + "..." : norm;
}
```

Add source attribution per DI registration — the `Source` file from `DiRegistrationDetection` (if captured). If not captured, add it to the detection.

#### Task D2: Middleware deduplication + project label

```csharp
// Group middleware by type, keep one per unique (type, kind) pair
var middlewareGroups = middleware
    .GroupBy(m => $"{m.MiddlewareType}:{m.Kind}")
    .Select(g => {
        var first = g.First();
        return (Mw: first, Count: g.Count());
    })
    .OrderBy(x => x.Mw.PipelineOrder);

// Add source file to show which project
foreach (var (mw, count) in middlewareGroups)
{
    var countNote = count > 1 ? $" ×{count}" : "";
    var source = Path.GetFileNameWithoutExtension(mw.SourceFile ?? "");
    sb.AppendLine($"| {mw.PipelineOrder} | {mw.MiddlewareType}{countNote} | {mw.Kind} | {source} |");
}
```

---

### GROUP E — UX / discoverability (P2)

#### Task E1: Profile-scenario mismatch warning

When `--scenario debug-endpoint` or `--scenario trace-message-flow` is used with `Focused` profile (or less), add a diagnostic note and a "to get more" footer hint.

**File**: `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` — after stage 3

```csharp
// After Stage 3:
if (context.ActiveScenario.Name is "debug-endpoint" or "trace-message-flow"
    && context.Options.Profile < ExtractionProfile.Debug
    && context.Analysis.FocusPoints.Any())
{
    model.AddDiagnostic(DiagnosticLevel.Info, "Pipeline",
        $"Scenario '{context.ActiveScenario.Name}' benefits from call graph analysis. " +
        $"Re-run with '--profile debug' to enable call graph (current: {context.Options.Profile}).");
}
```

Also surface it in the rendered output footer as a hint line:
```markdown
---
*...footer...*
💡 Re-run with `--profile debug` to add call graph to this output.
```

---

#### Task E2: Show `--around` usage hint in dry-run and scenario output

When running with a scenario but no `--around`, append a usage hint to the output:
```markdown
---
💡 Narrow this output with `--around TypeName` or `--around TypeName:MethodName` for focused context.
   Example: `devcontext . --scenario debug-endpoint --around CreateOrderCommandHandler`
```

Only show when the output has > 50 surviving types and no focus points — i.e., when the output is broad.

---

#### Task E3: `--around` resolution failure message

When `--around SomeType` doesn't find the type in `model.Types`, surface a clear message in the rendered output (not just a diagnostic):

```markdown
## ⚠ Entry point not found

`--around CreateOrderHandler` did not match any type in 517 scanned types.

**Did you mean?**
- `CreateOrderCommandHandler` (Ordering.API, src/Ordering.API/Application/Commands/)
- `CreateOrderDraftCommandHandler` (Ordering.API)

Run `devcontext . --dry-run` to see all resolved types.
```

Levenshtein-based suggestions, top 3 within distance 3.

---

### GROUP F — Scenario configuration fixes

#### Task F1: Fix `ScenarioRegistry` section names

The current required sections reference sections that don't exist (`"Entry"`, `"Call graph"`, `"Domain signals"`). Fix to reference actual renderer section names:

```csharp
["architecture"] = new() {
    RequiredSections = ["Architecture overview", "Endpoints", "MediatR Handlers", 
                        "Data model", "Non-obvious wiring", "Related types"]
},
["debug-endpoint"] = new() {
    // Focus: endpoint + handler + services involved + entities
    RequiredSections = ["Endpoints", "MediatR Handlers", "Data model", 
                        "Non-obvious wiring", "Related types"]
},
["add-similar-feature"] = new() {
    // Focus: entry point + pattern context
    RequiredSections = ["Endpoints", "MediatR Handlers", "Related types"]
},
["modify-middleware"] = new() {
    // Focus: pipeline only
    RequiredSections = ["Non-obvious wiring"]
},
["trace-message-flow"] = new() {
    // Focus: message routing
    RequiredSections = ["MediatR Handlers", "Message consumers", "Data model"]
},
["harden-di"] = new() {
    // Focus: indirect wiring
    RequiredSections = ["Non-obvious wiring", "Related types"]
}
```

Also add `"Entry points"` as a valid section that appears for all scenarios when `--around` is given, regardless of `RequiredSections`.

---

## Part 4 — PR Execution Order

```
Batch 1 — Fix silent gaps (A1-A4): all independent, can be done in any order
  PR 1:  A1 EF entities section in renderer
  PR 2:  A2 Message consumers section in renderer
  PR 3:  A3 Auth attribute detection in EndpointExtractor
  PR 4:  A4 Scenario display name in renderer header

Batch 2 — Scenario system wiring (B1-B3): B3 depends on ExtractionOptions changes from B1
  PR 5:  B1 MaxSurvivingTypes enforcement in TokenBudgetEnforcer
  PR 6:  B2 RequiredSections enforcement in MarkdownRenderer
  PR 7:  B3 MaxPathDistance propagation + F1 ScenarioRegistry fixes (do together)

Batch 3 — Context assembly (C1-C3): C1 depends on RenderOptions changes from B2
  PR 8:  C2 Per-project endpoint grouping (independent)
  PR 9:  C3 Endpoint → MediatR linkage note (independent)
  PR 10: C1 Entry points section (depends on B2 RenderOptions + FocusPoints in RenderOptions)

Batch 4 — DI/Middleware noise (D1-D2): independent
  PR 11: D1 DI registration filtering + truncation
  PR 12: D2 Middleware dedup + project label

Batch 5 — UX (E1-E3): independent
  PR 13: E1 Profile-scenario mismatch warning
  PR 14: E2 --around usage hint
  PR 15: E3 --around resolution failure with suggestions
```

---

## Part 5 — Verification by Scenario

After all batches merged, verify each scenario against the relevant eval repo:

### S1 Debug endpoint (eShop)
```bash
devcontext eval-repos/eShop/eShop.slnx --scenario debug-endpoint --around CreateOrderCommandHandler --profile focused --max-tokens 8000
```
**Assert**:
- Header: "Debug Endpoint on ClientApp" (not "Architecture Overview")
- Entry points section present showing `CreateOrderCommandHandler` dependencies
- ≤20 surviving types (MaxSurvivingTypes=20)
- Sections: Endpoints, MediatR Handlers, Data model, Non-obvious wiring, Related types
- Endpoints: Auth column has real values (not all `-`)
- Footer hint: "Re-run with --profile debug for call graph"

### S2 Add similar feature (VerticalSlice)
```bash
devcontext eval-repos/VerticalSlice/Clean.Architecture.slnx --scenario add-similar-feature --around GetByIdEndpoint --max-tokens 8000
```
**Assert**:
- Header: "Add Similar Feature"
- Entry points section shows `GetByIdEndpoint` inline (kind, methods, dependencies)
- ≤40 types surviving
- No Middleware section, no MediatR Handlers section (not in RequiredSections)

### S3 Trace message flow (eShop)
```bash
devcontext eval-repos/eShop/eShop.slnx --scenario trace-message-flow --around CreateOrder --max-tokens 8000
```
**Assert**:
- Header: "Trace Message Flow"
- Message consumers section present (if EventBusExtractor found any)
- MediatR handlers section present
- Data model section present (EF entities)
- No Endpoints table (not in RequiredSections for this scenario)

### S4 Modify middleware (eShop)
```bash
devcontext eval-repos/eShop/eShop.slnx --scenario modify-middleware --max-tokens 4000
```
**Assert**:
- Header: "Modify Middleware"
- Only Non-obvious wiring section (+ Architecture overview)
- Middleware table: no duplicate UseExceptionHandler × 2
- Source project shown per middleware entry
- ≤25 types surviving

### S5 Architecture overview (TodoApi)
```bash
devcontext eval-repos/TodoApi/TodoApp.sln --scenario architecture --max-tokens 8000
```
**Assert**:
- All sections shown (architecture requires all)
- 11 endpoints with auth column populated
- No EF section (no efcore signal)
- No message consumers section (no MassTransit)
- Surviving types still compact

### S6 Harden DI (eShop)
```bash
devcontext eval-repos/eShop/eShop.slnx --scenario harden-di --max-tokens 8000
```
**Assert**:
- Header: "Harden DI"
- Only Non-obvious wiring + Related types sections
- IndirectWiringDetection entries present (if any found)
- ≤50 types

---

## Part 6 — New Ideas and Future Iteration Seeds

These don't fit iteration 6 scope but should be tracked:

**Dependency chain view** (iteration 7): For `--around SomeHandler`, build a mini dependency graph: `SomeHandler → depends on → [IOrderRepository (Scoped→OrderRepository), IEventBus (Singleton→RabbitMQEventBus)]`. Requires cross-referencing `DiRegistrationDetection` with constructor parameter types from `TypeDiscovery.Methods[.ctor]`. No Roslyn needed — pure model cross-referencing.

**Event flow chain** (iteration 7): For MediatR: `CreateOrderCommand → CreateOrderCommandHandler → raises OrderStartedDomainEvent → handled by ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler`. The chain is inferable from: command handlers (command type) + notification handlers (notification type). Build the chain from detection data alone.

**Multi-step `--around`** (iteration 7): `--around POST:/api/orders` as a shorthand to focus on a specific endpoint by method+route rather than type name. Route → endpoint detection → handler type → focus point.

**`--what` free-text search** (iteration 7): `devcontext . --what "why does order creation fail"` → uses `IntentInferrer` + extracts candidate type names from the question → auto-sets `--around` candidates.

**Auth policy graph** (iteration 8): When `[Authorize("OrdersFullAccess")]` is detected, find where `OrdersFullAccess` is defined in the policy registry, what claims/roles it requires. Produces a security overview section.

**Hot path detection** (iteration 8): Identify types that appear in multiple endpoint handlers (high fan-in) as "likely shared infrastructure". These are the most important types to understand for any change.

**`--since last-commit` mode** (iteration 9): Run against the diff between HEAD and previous commit. Only extract context for changed files and their direct dependencies. Extremely useful for PR review.

---

## Git Maintenance

Same conventions as prior iterations. Branch format: `feature/A1-ef-entities-renderer`, `fix/D1-di-registration-filtering`. One PR per task. All 138+ tests pass before merge. Run at least 2 scenario verification commands after each batch and commit eval artifacts to `eval/iteration-6-results`.

Commit convention examples:
```
feat(render): add EF Core entities section to MarkdownRenderer
feat(render): add message consumers section for trace-message-flow scenario
feat(endpoint): populate AuthAttributes from RequireAuthorization/Authorize chains
fix(render): use scenario display name in output header
feat(pruning): enforce MaxSurvivingTypes from scenario config in TokenBudgetEnforcer
feat(render): enforce RequiredSections per scenario in MarkdownRenderer
feat(render): add Entry points section for --around focus runs
fix(render): per-project endpoint grouping in endpoint table
fix(render): truncate DI registration lambda bodies, filter Extension/? noise
fix(render): deduplicate middleware by type with project source label
feat(pipeline): warn when profile insufficient for scenario (debug-endpoint + Focused)
```

---

*Source: iteration 5 E2E findings + 7-scenario usability walkthrough. Tests: 138. Next: iteration 7 should target dependency chain view and event flow chain assembly.*
