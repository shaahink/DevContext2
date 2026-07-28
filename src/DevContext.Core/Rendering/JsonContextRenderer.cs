using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevContext.Core.Rendering;

/// <summary>Renders the discovery model as a structured JSON document.</summary>
public sealed class JsonContextRenderer : IContextRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // Batch D: detections are polymorphic, and the ONE declaration of that lives in
        // DetectionPolymorphism. This render used to rely on an attribute list on Detection that the
        // snapshot cache overrode with a different discriminator — same hierarchy, two wire formats.
        TypeInfoResolver = Models.DetectionPolymorphism.Resolver(),
    };

    /// <summary>Gets the format identifier ("json").</summary>
    public string Format => "json";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var output = BuildOutput(model, options);
        var json = JsonSerializer.Serialize(output, JsonOptions);
        var estimatedTokens = Math.Max(1, json.Length / 4);

        return new ValueTask<RenderedContext>(new RenderedContext(
            json, estimatedTokens, [.. model.AppliedCompressions], sw.Elapsed, "1.1"));
    }

    private static DevContextOutput BuildOutput(DiscoveryModel model, RenderOptions options)
    {
        var total = model.Types.Count;
        int inOutput;
        double prunedPercent;

        if (options.Plan is { } plan)
        {
            inOutput = plan.IncludedTypeIds.Length;
            prunedPercent = total > 0 ? Math.Round((double)(total - inOutput) / total * 100, 1) : 0;
        }
        else
        {
            inOutput = model.Types.Values.Count(t => !t.IsHardExcluded);
            prunedPercent = total > 0 ? Math.Round((double)(total - inOutput) / total * 100, 1) : 0;
        }

        return new DevContextOutput
        {
            SchemaVersion = "1.1",
            GeneratedAt = DateTime.UtcNow,
            Solution = model.Solution is not null
                ? new SolutionOutput(
                    model.Solution.Name,
                    model.Solution.FilePath,
                    [.. model.Solution.ProjectPaths])
                : null,
            Architecture = new ArchitectureOutput(
                model.DetectedStyle.ToString(),
                model.StyleConfidence),
            Archetype = model.Archetype ?? "App",
            Signals = [.. model.Architecture.All
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new SignalOutput(kvp.Key, kvp.Value.Confidence, kvp.Value.Detected))],
            Projects = new ProjectsOutput(
                model.Projects.Length,
                [.. model.Projects.OrderBy(p => p.Name).Select(p => p.Name)]),
            TypesSummary = new TypesOutput(total, inOutput, prunedPercent),
            Detections = [.. model.Detections.OrderBy(d => d.GetType().Name).ThenBy(d => d.SourceFile).ThenBy(d => d.LineNumber)],
            Diagnostics = options.IncludeDiagnostics ? [.. model.Diagnostics] : null,
            PruningSummary = model.PruningNotes.Count > 0
                ? string.Join("; ", model.PruningNotes)
                : null,
            PruningNotes = options.IncludeDiagnostics && model.PruningNotes.Count > 0
                ? [.. model.PruningNotes]
                : null,
            MaxTokens = options.EstimatedTokens,
            RunReport = options.Report,
            EventWiring = BuildEventWiring(options.Snapshot?.Graph),
        };
    }

    /// <summary>T2.6 — surfaces the graph's event-wiring projection into the JSON contract. The counts are
    /// the deterministic gate handle (e.g. eShop's ≥8 integration events); the rows let a JSON-only
    /// consumer render the same board without holding the graph.</summary>
    private static EventWiringOutput? BuildEventWiring(Graph.CodeGraph? graph)
    {
        if (graph is null || graph.EventWiring.IsDefaultOrEmpty) return null;
        var wiring = graph.EventWiring;
        var events = wiring.Select(w => new EventWireOutput(
            w.EventName,
            w.IsIntegration,
            w.IsCrossService,
            [.. w.Publishers.Select(p => p.Service ?? p.Title).Distinct(StringComparer.Ordinal)],
            [.. w.Consumers.Select(c => c.Service ?? c.Title).Distinct(StringComparer.Ordinal)]))
            .OrderBy(e => e.Event, StringComparer.Ordinal)
            .ToList();
        return new EventWiringOutput(
            wiring.Length,
            wiring.Count(w => w.IsIntegration),
            wiring.Count(w => w.IsCrossService),
            wiring.Count(w => w.IsOrphan),
            events);
    }
}
