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

        AppendHeader(sb, model);
        AppendArchitecture(sb, model);
        AppendSignals(sb, model);
        AppendProjects(sb, model);
        AppendProfileAndTokens(sb, model, options);
        sb.AppendLine("---");

        AppendArchitectureOverview(sb, model);
        AppendEndpoints(sb, model);
        AppendMediatRHandlers(sb, model);
        AppendNonObviousWiring(sb, model);
        AppendRelatedTypesByLayer(sb, model);

        if (options.IncludeDiagnostics)
            AppendDiagnostics(sb, model);

        AppendFooter(sb, model, sw);

        var content = sb.ToString();
        var estimatedTokens = Math.Max(1, content.Length / 4);

        return new ValueTask<RenderedContext>(new RenderedContext(
            content, estimatedTokens, model.PruningNotes.Select(n => new CompressionResult("Pruning", 0, 0, [n])).ToArray(), sw.Elapsed, "2.0"));
    }

    private static void AppendHeader(StringBuilder sb, DiscoveryModel model)
    {
        var scenarioName = "Architecture Overview";
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

    private static void AppendEndpoints(StringBuilder sb, DiscoveryModel model)
    {
        sb.AppendLine("## Endpoints");
        sb.AppendLine();

        var endpoints = model.Detections.OfType<EndpointDetection>().ToList();
        if (endpoints.Count == 0)
        {
            sb.AppendLine("No endpoints detected.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("| Method | Route | Handler | Auth |");
        sb.AppendLine("|--------|-------|---------|------|");
        foreach (var ep in endpoints)
        {
            var auth = ep.AuthAttributes.Length > 0 ? string.Join(", ", ep.AuthAttributes) : "-";
            sb.AppendLine($"| {ep.HttpMethod} | {ep.RouteTemplate} | {ep.HandlerType}.{ep.HandlerMethod} | {auth} |");
        }

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
            sb.AppendLine("| Order | Type | Kind |");
            sb.AppendLine("|-------|------|------|");
            foreach (var m in middleware.OrderBy(m => m.PipelineOrder))
                sb.AppendLine($"| {m.PipelineOrder} | {m.MiddlewareType} | {m.Kind} |");
            sb.AppendLine();
        }

        if (diRegs.Count > 0)
        {
            sb.AppendLine("### DI registrations");
            sb.AppendLine();
            sb.AppendLine("| Lifetime | Service | Implementation |");
            sb.AppendLine("|----------|---------|----------------|");
            foreach (var d in diRegs)
                sb.AppendLine($"| {d.Lifetime} | {d.ServiceType} | {d.ImplementationType} |");
            sb.AppendLine();
        }
    }

    private static void AppendRelatedTypesByLayer(StringBuilder sb, DiscoveryModel model)
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
        System.Diagnostics.Stopwatch sw)
    {
        var typesTotal = model.Types.Count;
        var typesSurviving = model.Types.Values.Count(t => !t.IsPruned);
        var prunedCount = typesTotal - typesSurviving;

        var compressionCount = model.PruningNotes.Count(n =>
            n.Contains("TrivialMember") || n.Contains("Boilerplate") ||
            n.Contains("Deduplicator") || n.Contains("NamespaceGrouper") ||
            n.Contains("LlmFriendly") || n.Contains("AggressiveTruncator") ||
            n.Contains("kept") || n.Contains("TokenBudget"));

        var pruningDetail = compressionCount > 0
            ? $" | Compressed: {compressionCount} strategies"
            : "";

        sb.AppendLine("---");
        sb.AppendLine($"*Generated in {sw.Elapsed.TotalMilliseconds:F1}ms | "
            + $"{typesTotal} types ({typesSurviving} active, {prunedCount} pruned)"
            + pruningDetail
            + " | Schema v2.0*");
    }
}
