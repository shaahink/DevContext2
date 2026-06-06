using System.Text;

namespace DevContext.Core.Rendering;

/// <summary>Renders the discovery model as a human-readable Markdown document.</summary>
public sealed class MarkdownRenderer : IContextRenderer
{
    public string Format => "markdown";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var sb = new StringBuilder();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        AppendHeader(sb, model, options);
        AppendArchitecture(sb, model);
        AppendSignals(sb, model);
        AppendProjects(sb, model);
        AppendProfileAndTokens(sb, model, options);
        sb.AppendLine("---");

        if (ShouldRender(SectionNames.ArchitectureOverview, options))
            AppendArchitectureOverview(sb, model);
        if (!options.FocusPoints.IsDefaultOrEmpty && options.FocusPoints.Length > 0)
            AppendEntryPoints(sb, model, options);
        if (ShouldRender(SectionNames.Endpoints, options))
            AppendEndpoints(sb, model);
        if (ShouldRender(SectionNames.CallGraph, options))
            AppendCallGraphAvailability(sb, model);
        if (ShouldRender(SectionNames.MediatRHandlers, options))
            AppendMediatRHandlers(sb, model);
        if (ShouldRender(SectionNames.DataModel, options))
            AppendEfEntities(sb, model);
        if (ShouldRender(SectionNames.MessageConsumers, options))
            AppendMessageConsumers(sb, model);
        if (ShouldRender(SectionNames.NonObviousWiring, options))
            AppendNonObviousWiring(sb, model);
        if (ShouldRender(SectionNames.RelatedTypes, options))
            AppendRelatedTypesByLayer(sb, model);

        if (options.IncludeDiagnostics)
            AppendDiagnostics(sb, model);

        AppendFooter(sb, model, options, sw);

        var content = sb.ToString();
        var estimatedTokens = Math.Max(1, content.Length / 4);

        return new ValueTask<RenderedContext>(new RenderedContext(
            content, estimatedTokens, [.. model.AppliedCompressions], sw.Elapsed, "2.0"));
    }

    private static void AppendHeader(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        var scenarioName = options.ScenarioDisplayName ?? "Architecture Overview";
        var entryName = model.Solution?.Name ?? "project";

        sb.AppendLine($"## DevContext -- {scenarioName} on {entryName}");
        sb.AppendLine();
    }

    private static void AppendArchitecture(StringBuilder sb, DiscoveryModel model)
    {
        var style = model.DetectedStyle != ArchitectureStyle.Unknown
            ? $"{model.DetectedStyle} ({model.StyleConfidence:P0} confidence)"
            : "Not detected";

        sb.AppendLine($"**Architecture**: {style}");
    }

    private static void AppendSignals(StringBuilder sb, DiscoveryModel model)
    {
        var signals = model.Architecture.All.Values.Where(s => s.Detected).ToList();
        if (signals.Count == 0)
        {
            sb.AppendLine("**Signals**: none");
            return;
        }

        var signalList = string.Join(" \u00b7 ", signals.Select(s => s.Key));
        sb.AppendLine($"**Signals**: {signalList}");
    }

    private static void AppendProjects(StringBuilder sb, DiscoveryModel model)
    {
        var count = model.Projects.Length;
        var names = count > 0
            ? string.Join(", ", model.Projects.Select(p => p.Name))
            : "none";

        sb.AppendLine($"**Projects**: {count} -- {names}");
    }

    private static void AppendProfileAndTokens(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        var activeTypes = model.Types.Values.Count(t => !t.IsPruned);
        sb.AppendLine($"**Profile**: focused | **Tokens**: ~{options.EstimatedTokens} (budget {model.Budget.MaxTokens}) | **Types**: {activeTypes} in output");
        sb.AppendLine();
    }

    private static void AppendArchitectureOverview(StringBuilder sb, DiscoveryModel model)
    {
        sb.AppendLine("## Architecture overview");
        sb.AppendLine();

        if (model.Projects.Length == 0)
        {
            sb.AppendLine("No projects discovered.");
            sb.AppendLine();
            return;
        }

        foreach (var project in model.Projects)
        {
            sb.AppendLine($"- {project.Name}");
        }

        sb.AppendLine();
    }

    private static void AppendEntryPoints(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        if (options.FocusPoints.IsDefaultOrEmpty || options.FocusPoints.Length == 0) return;

        sb.AppendLine("## Entry points");
        sb.AppendLine();

        foreach (var focus in options.FocusPoints)
        {
            var type = model.Types.Values.FirstOrDefault(t =>
                t.Name == focus.TypeName || t.Id.EndsWith("." + focus.TypeName, StringComparison.Ordinal));
            if (type is null)
            {
                // Show unresolved focus point with Levenshtein suggestions
                sb.AppendLine($"⚠ `--around {focus.TypeName}` not found.");
                if (focus.TypeName is not null)
                {
                    var matches = model.Types.Values
                        .Select(t => t.Name)
                        .Distinct()
                        .Select(n => (Name: n, Dist: LevenshteinDistance(focus.TypeName, n)))
                        .Where(x => x.Dist <= 3 && x.Dist > 0)
                        .OrderBy(x => x.Dist)
                        .Take(3)
                        .Select(x => $"`{x.Name}`")
                        .ToList();
                    if (matches.Count > 0)
                        sb.AppendLine($"  Did you mean: {string.Join(", ", matches)}?");
                }
                sb.AppendLine();
                continue;
            }

            sb.AppendLine($"### `{type.Name}` ({type.Kind}, {type.Layer})");
            sb.AppendLine($"> `{type.Namespace}.{type.Name}` — {type.FilePath}");
            sb.AppendLine();

            if (type.ImplementedInterfaces.Length > 0)
                sb.AppendLine($"**Implements**: {string.Join(", ", type.ImplementedInterfaces.Select(i => $"`{i}`"))}");

            if (type.BaseTypes.Length > 0 && type.BaseTypes[0] != "object")
                sb.AppendLine($"**Extends**: `{type.BaseTypes[0]}`");

            var ctors = type.Methods.Where(m => m.Name == ".ctor" || m.Name == type.Name).ToList();
            if (ctors.Any() && ctors[0].ParameterTypes.Length > 0)
            {
                var deps = ctors[0].ParameterTypes.Zip(ctors[0].ParameterNames, (t2, n) => $"`{t2} {n}`");
                sb.AppendLine($"**Depends on**: {string.Join(", ", deps)}");
            }

            var publicMethods = type.Methods
                .Where(m => m.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public
                    && m.Name != ".ctor" && m.Name != type.Name)
                .Take(8)
                .ToList();
            if (publicMethods.Any())
            {
                sb.AppendLine();
                sb.AppendLine("**Methods**:");
                foreach (var method in publicMethods)
                {
                    var paramStr = string.Join(", ", method.ParameterTypes.Zip(
                        method.ParameterNames, (t, n) => $"{t} {n}"));
                    sb.AppendLine($"- `{method.ReturnType} {method.Name}({paramStr})`");
                }
            }

            sb.AppendLine();
        }
    }

    private static void AppendEndpoints(StringBuilder sb, DiscoveryModel model)
    {
        sb.AppendLine("## Endpoints");
        sb.AppendLine();

        var allEndpoints = model.Detections.OfType<EndpointDetection>()
            .Where(e => !IsFrameworkEndpoint(e))
            .ToList();

        if (allEndpoints.Count == 0)
        {
            sb.AppendLine("No endpoints detected.");
            sb.AppendLine();
            return;
        }

        var hasGroupPrefix = allEndpoints.Any(e => e.GroupPrefix is not null);

        // Build project lookup from source file paths
        var projectByDir = model.Projects
            .Select(p => (Dir: Path.GetDirectoryName(p.FilePath) ?? "", Name: p.Name))
            .Where(p => !string.IsNullOrEmpty(p.Dir))
            .OrderByDescending(p => p.Dir.Length)
            .ToList();

        string ProjectForFile(string filePath) =>
            projectByDir.FirstOrDefault(kv => filePath.StartsWith(kv.Dir, StringComparison.OrdinalIgnoreCase)).Name
            ?? Path.GetFileName(Path.GetDirectoryName(filePath)) ?? "?";

        // Group endpoints by project
        var byProject = allEndpoints.GroupBy(ep => ProjectForFile(ep.SourceFile)).OrderBy(g => g.Key);

        // Collect MediatR handler types for linkage
        var mediatRTypes = model.Detections.OfType<MediatRHandlerDetection>()
            .Select(m => m.HandlerType)
            .ToHashSet();

        var header = hasGroupPrefix
            ? "| Method | Route | Group | Handler | Auth | Source |"
            : "| Method | Route | Handler | Auth | Source |";
        var sep = hasGroupPrefix
            ? "|--------|-------|-------|---------|------|--------|"
            : "|--------|-------|---------|------|--------|";

        foreach (var projectGroup in byProject)
        {
            sb.AppendLine($"**{projectGroup.Key}** ({projectGroup.Count()} endpoints)");
            sb.AppendLine(header);
            sb.AppendLine(sep);

            foreach (var ep in projectGroup)
            {
                var auth = FormatAuth(ep);
                var handler = FormatHandler(ep);

                // C3: MediatR linkage — check if handler type is a known MediatR handler
                if (mediatRTypes.Contains(ep.HandlerType))
                {
                    var mediatR = model.Detections.OfType<MediatRHandlerDetection>()
                        .FirstOrDefault(m => m.HandlerType == ep.HandlerType);
                    if (mediatR is not null)
                        handler += $" → `{mediatR.RequestType}`";
                }

                var source = $"{Path.GetFileName(ep.SourceFile)}:{ep.LineNumber}";

                if (hasGroupPrefix)
                {
                    var group = ep.GroupPrefix ?? "-";
                    sb.AppendLine($"| {ep.HttpMethod} | {ep.RouteTemplate} | {group} | {handler} | {auth} | {source} |");
                }
                else
                {
                    sb.AppendLine($"| {ep.HttpMethod} | {ep.RouteTemplate} | {handler} | {auth} | {source} |");
                }
            }
            sb.AppendLine();
        }
    }

    private static bool ShouldRender(string sectionName, RenderOptions options)
    {
        if (options.RequiredSections.IsDefaultOrEmpty) return true;
        return options.RequiredSections.Contains(sectionName);
    }

    private static int LevenshteinDistance(string a, string b) => StringHelpers.LevenshteinDistance(a, b);

    private static string FormatAuth(EndpointDetection ep)
    {
        return ep.AuthAttributes.Length > 0 ? string.Join(", ", ep.AuthAttributes) : "-";
    }

    private static bool IsFrameworkEndpoint(EndpointDetection ep)
    {
        // Filter routes from known framework/infrastructure files
        var fileName = Path.GetFileName(ep.SourceFile);
        if (fileName is "OpenApi.Extensions.cs" or "Extensions.cs")
            return true;

        // Filter known framework route patterns
        var route = ep.RouteTemplate;
        if (route is "/" or "/health" or "/alive")
            return true;

        return false;
    }

    private static string FormatHandler(EndpointDetection ep)
    {
        var handler = ep.HandlerMethod;
        if (handler is "<lambda>" or "<anonymous>")
        {
            var handlerText = ep.HandlerType;
            // Truncate very long lambda bodies to a compact reference
            if (handlerText.Length > 80)
                return $"λ {Path.GetFileName(ep.SourceFile)}:{ep.LineNumber}";
            return handlerText.Length > 40 ? handlerText[..37] + "..." : handlerText;
        }
        return $"{ep.HandlerType}.{handler}";
    }

    private static string FormatImplementation(string implementationType, ImmutableArray<string> extensionsUsed)
    {
        // Filter unresolvable extension args
        if (implementationType == "?" || string.IsNullOrEmpty(implementationType))
            return extensionsUsed.Length > 0
                ? $"({string.Join(", ", extensionsUsed.Take(3))})"
                : "-";

        // Truncate delegate bodies (lambdas with => or { )
        if (implementationType.Length > 80)
        {
            var firstLine = implementationType.Split('\n')[0];
            return firstLine.Length > 60
                ? firstLine[..57] + "..."
                : firstLine + "...";
        }

        return implementationType;
    }

    private static void AppendCallGraphAvailability(StringBuilder sb, DiscoveryModel model)
    {
        sb.AppendLine("## Call graph");
        sb.AppendLine();
        sb.AppendLine("Not available in focused profile. Re-run with `--profile debug` to enable call graph extraction and BFS reachability analysis from entry points.");
        sb.AppendLine();
    }

    private static void AppendMediatRHandlers(StringBuilder sb, DiscoveryModel model)
    {
        var handlers = model.Detections.OfType<MediatRHandlerDetection>().ToList();
        if (handlers.Count == 0) return;

        sb.AppendLine("## MediatR Handlers");
        sb.AppendLine();

        sb.AppendLine("| Kind | Request | Response | Handler |");
        sb.AppendLine("|------|---------|----------|---------|");
        foreach (var h in handlers)
        {
            sb.AppendLine($"| {h.Kind} | {h.RequestType} | {h.ResponseType} | {h.HandlerType} |");
        }

        sb.AppendLine();
    }

    private static void AppendEfEntities(StringBuilder sb, DiscoveryModel model)
    {
        var entities = model.Detections.OfType<EfEntityDetection>().ToList();
        if (entities.Count == 0) return;

        sb.AppendLine("## Data model (EF Core)");
        sb.AppendLine();

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

    private static void AppendNonObviousWiring(StringBuilder sb, DiscoveryModel model)
    {
        var wiring = model.Detections.OfType<IndirectWiringDetection>().ToList();
        var workers = model.Detections.OfType<BackgroundWorkerDetection>().ToList();
        var middleware = model.Detections.OfType<MiddlewareDetection>().ToList();
        var diRegs = model.Detections.OfType<DiRegistrationDetection>().ToList();

        if (wiring.Count == 0 && workers.Count == 0 && middleware.Count == 0 && diRegs.Count == 0)
            return;

        sb.AppendLine("## Non-obvious wiring");
        sb.AppendLine();

        if (wiring.Count > 0)
        {
            sb.AppendLine("### Indirect wiring");
            sb.AppendLine();
            sb.AppendLine("| Kind | Caller | Target |");
            sb.AppendLine("|------|--------|--------|");
            foreach (var w in wiring)
                sb.AppendLine($"| {w.Kind} | {w.CallerType}.{w.CallerMethod} | {w.TargetType ?? "unknown"} |");
            sb.AppendLine();
        }

        if (workers.Count > 0)
        {
            sb.AppendLine("### Background workers");
            sb.AppendLine();
            foreach (var w in workers)
                sb.AppendLine($"- {w.ImplementationType} ({w.Kind})");
            sb.AppendLine();
        }

        if (middleware.Count > 0)
        {
            sb.AppendLine("### Middleware pipeline");
            sb.AppendLine();

            var grouped = middleware
                .GroupBy(m => m.MiddlewareType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Kind = g.First().Kind,
                    Sources = g.Select(m => Path.GetFileName(m.SourceFile)).Distinct()
                })
                .OrderBy(m => m.Count)
                .ToList();

            sb.AppendLine("| Type | Kind | Count | Sources |");
            sb.AppendLine("|------|------|-------|---------|");
            foreach (var m in grouped)
            {
                var sources = string.Join(", ", m.Sources);
                sb.AppendLine($"| {m.Type} | {m.Kind} | {m.Count} | {sources} |");
            }
            sb.AppendLine();
        }

        if (diRegs.Count > 0)
        {
            sb.AppendLine("### DI registrations");
            sb.AppendLine();
            sb.AppendLine("| Lifetime | Service | Implementation | Source |");
            sb.AppendLine("|----------|---------|----------------|--------|");
            foreach (var d in diRegs)
            {
                // Skip ? implementations — unresolvable extension args are noise
                var impl = FormatImplementation(d.ImplementationType, d.ExtensionsUsed);
                var source = $"{Path.GetFileName(d.SourceFile)}:{d.LineNumber}";
                sb.AppendLine($"| {d.Lifetime} | {d.ServiceType} | {impl} | {source} |");
            }
            sb.AppendLine();
        }
    }

    private static void AppendRelatedTypesByLayer(StringBuilder sb, DiscoveryModel model)
    {
        var hasDetections = model.Detections.Count > 0;

        if (hasDetections)
        {
            sb.AppendLine("## Related types grouped by layer");
            sb.AppendLine();

            var typedTypes = model.Types.Values
                .Where(t => !t.IsPruned)
                .GroupBy(t => t.Layer)
                .OrderBy(g => g.Key.ToString());

            var hasContent = false;

            foreach (var group in typedTypes)
            {
                var typeList = string.Join(", ", group.Select(t => t.Name));
                sb.AppendLine($"- **{group.Key}**: {typeList}");
                hasContent = true;
            }

            if (!hasContent)
                sb.AppendLine("No types discovered.");

            sb.AppendLine();
        }
        else
        {
            // Library mode: no detections — emit a compact namespace-summary instead of flat wall
            sb.AppendLine("## Types by namespace");
            sb.AppendLine();

            var hasContent = false;
            var survivingTypes = model.Types.Values.Where(t => !t.IsPruned).ToList();

            foreach (var nsGroup in survivingTypes
                .GroupBy(t => t.Namespace)
                .OrderBy(g => g.Key))
            {
                var types = nsGroup.ToList();
                var publicCount = types.Count(t => t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public);
                var totalCount = types.Count;
                var publicTypes = types
                    .Where(t => t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
                    .Select(t => t.Name)
                    .ToList();

                sb.Append($"- **{nsGroup.Key}** — {totalCount} types");
                if (publicCount > 0 && publicCount < totalCount)
                    sb.Append($" ({publicCount} public)");
                sb.AppendLine();

                if (publicCount > 0 && publicTypes.Count <= 10)
                {
                    sb.AppendLine($"  Public: {string.Join(", ", publicTypes)}");
                }
                else if (publicCount > 10)
                {
                    var sample = string.Join(", ", publicTypes.Take(10));
                    sb.AppendLine($"  Public ({publicCount}): {sample} ...");
                }
                hasContent = true;
            }

            if (!hasContent)
                sb.AppendLine("No types discovered.");

            sb.AppendLine();
        }
    }

    private static void AppendDiagnostics(StringBuilder sb, DiscoveryModel model)
    {
        sb.AppendLine("## Diagnostics");
        sb.AppendLine();

        var diagnostics = model.Diagnostics.ToList();
        if (diagnostics.Count == 0)
        {
            sb.AppendLine("No diagnostics recorded.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("| Level | Source | Message |");
        sb.AppendLine("|-------|--------|---------|");
        foreach (var diag in diagnostics)
            sb.AppendLine($"| {diag.Level} | {diag.Source} | {diag.Message} |");

        sb.AppendLine();

        if (model.PruningNotes.Count > 0)
        {
            sb.AppendLine("### Pruning notes");
            sb.AppendLine();
            foreach (var note in model.PruningNotes)
                sb.AppendLine($"- {note}");
            sb.AppendLine();
        }
    }

    private static void AppendFooter(StringBuilder sb, DiscoveryModel model,
        RenderOptions options, System.Diagnostics.Stopwatch sw)
    {
        var typesTotal = model.Types.Count;
        var typesSurviving = model.Types.Values.Count(t => !t.IsPruned);
        var prunedCount = typesTotal - typesSurviving;

        var compressionSummary = model.AppliedCompressions
            .Where(r => r.TokensBefore != r.TokensAfter)
            .Select(r =>
            {
                var pct = r.TokensBefore > 0
                    ? (r.TokensBefore - r.TokensAfter) * 100 / r.TokensBefore
                    : 0;
                return $"{r.StrategyName}(−{pct}%)";
            })
            .ToList();

        var compressionText = compressionSummary.Count > 0
            ? $" | Compression: {string.Join(" · ", compressionSummary)}"
            : "";

        sb.AppendLine("---");
        sb.AppendLine($"*Generated in {sw.Elapsed.TotalMilliseconds:F1}ms | "
            + $"{typesTotal} types ({typesSurviving} active, {prunedCount} pruned)"
            + compressionText
            + " | Schema v2.0*");

        // Usage hints (only when output is broad and no focus points)
        if (typesSurviving > 50 && options.FocusPoints.IsDefaultOrEmpty)
        {
            sb.AppendLine();
            sb.AppendLine("> 💡 Narrow this output with `--around TypeName` or `--around TypeName:MethodName` for focused context.");
        }
    }
}
