using System.Text;

namespace DevContext.Core.Rendering;

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
        AppendProfileAndTokens(sb, model);
        sb.AppendLine("---");

        AppendArchitectureOverview(sb, model);
        AppendEntrySection(sb, model);
        AppendRelatedTypesByLayer(sb, model);

        if (options.IncludeDiagnostics)
        {
            AppendDiagnostics(sb, model);
        }

        AppendFooter(sb, model, sw, options);

        var content = sb.ToString();
        var estimatedTokens = Math.Max(1, content.Length / 4);

        return new ValueTask<RenderedContext>(new RenderedContext(
            content, estimatedTokens, [], sw.Elapsed, "2.0"));
    }

    private static void AppendHeader(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        var scenarioName = "Unknown";
        var entryName = model.Solution?.Name ?? "Unknown";

        sb.AppendLine($"## DevContext \u2014 {scenarioName} on {entryName}");
        sb.AppendLine();
    }

    private static void AppendArchitecture(StringBuilder sb, DiscoveryModel model)
    {
        var style = model.DetectedStyle != ArchitectureStyle.Unknown
            ? $"{model.DetectedStyle} ({model.StyleConfidence:P0})"
            : "Not detected";

        sb.AppendLine($"**Architecture**: {style}");
    }

    private static void AppendSignals(StringBuilder sb, DiscoveryModel model)
    {
        var signals = model.Architecture.All;
        if (signals.Count == 0)
        {
            sb.AppendLine("**Signals**: none");
            return;
        }

        var signalList = string.Join(", ",
            signals.Where(s => s.Value.Detected).Select(s => s.Key));
        sb.AppendLine($"**Signals**: {signalList}");
    }

    private static void AppendProjects(StringBuilder sb, DiscoveryModel model)
    {
        var count = model.Projects.Length;
        var names = count > 0
            ? string.Join(", ", model.Projects.Select(p => p.Name))
            : "none";

        sb.AppendLine($"**Projects**: {count} \u2014 {names}");
    }

    private static void AppendProfileAndTokens(StringBuilder sb, DiscoveryModel model)
    {
        sb.AppendLine($"**Profile**: {model.Budget.MaxTokens} | **Tokens**: {model.Budget.MaxTokens - model.Budget.SafetyMargin}/{model.Budget.MaxTokens}");
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

        var layerGroups = model.Projects
            .GroupBy(InferProjectLayer)
            .OrderBy(g => g.Key.ToString());

        foreach (var group in layerGroups)
        {
            var projectList = string.Join(", ", group.Select(p => p.Name));
            sb.AppendLine($"- **{group.Key}**: {projectList}");
        }

        sb.AppendLine();
    }

    private static void AppendEntrySection(StringBuilder sb, DiscoveryModel model)
    {
        sb.AppendLine("## Entry section with endpoints");
        sb.AppendLine();

        var endpoints = model.Detections.OfType<EndpointDetection>().ToList();
        if (endpoints.Count == 0)
        {
            sb.AppendLine("No endpoints detected.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("| Method | Route | Handler |");
        sb.AppendLine("|--------|-------|---------|");
        foreach (var ep in endpoints)
        {
            sb.AppendLine($"| {ep.HttpMethod} | {ep.RouteTemplate} | {ep.HandlerType} |");
        }

        sb.AppendLine();
    }

    private static ArchitectureLayer InferProjectLayer(ProjectInfo project)
    {
        var lower = project.Name.ToLowerInvariant();
        if (lower.Contains("domain")) return ArchitectureLayer.Domain;
        if (lower.Contains("application")) return ArchitectureLayer.Application;
        if (lower.Contains("infrastructure")) return ArchitectureLayer.Infrastructure;
        if (lower.Contains("persistence")) return ArchitectureLayer.Persistence;
        if (lower.Contains("presentation") || lower.Contains("ui") || lower.Contains("web")) return ArchitectureLayer.Presentation;
        if (lower.Contains("api")) return ArchitectureLayer.Api;
        if (lower.Contains("shared") || lower.Contains("common")) return ArchitectureLayer.Shared;
        if (lower.Contains("test") || lower.Contains("spec")) return ArchitectureLayer.Testing;
        return ArchitectureLayer.Unknown;
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
        {
            sb.AppendLine("No types discovered.");
        }

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
        {
            sb.AppendLine($"| {diag.Level} | {diag.Source} | {diag.Message} |");
        }

        sb.AppendLine();
    }

    private static void AppendFooter(StringBuilder sb, DiscoveryModel model,
        System.Diagnostics.Stopwatch sw, RenderOptions options)
    {
        var typesTotal = model.Types.Count;
        var typesSurviving = model.Types.Values.Count(t => !t.IsPruned);
        var prunedCount = typesTotal - typesSurviving;
        var compressNotes = string.Join("; ", model.PruningNotes);

        sb.AppendLine($"---");
        sb.AppendLine($"*Generated in {sw.Elapsed.TotalMilliseconds:F1}ms | "
            + $"{typesTotal} types ({typesSurviving} active, {prunedCount} pruned)"
            + (compressNotes.Length > 0 ? $" | {compressNotes}" : "")
            + $" | Schema v2.0*");
    }
}
