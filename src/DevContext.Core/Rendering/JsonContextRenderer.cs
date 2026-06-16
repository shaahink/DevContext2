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
            Signals = [.. model.Architecture.All
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(kvp => new SignalOutput(kvp.Key, kvp.Value.Confidence, kvp.Value.Detected))],
            Projects = new ProjectsOutput(
                model.Projects.Length,
                [.. model.Projects.OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name)]),
            TypesSummary = new TypesOutput(total, inOutput, prunedPercent),
            Detections = [.. model.Detections.OrderBy(d => d.GetType().Name, StringComparer.Ordinal).ThenBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber)],
            Diagnostics = options.IncludeDiagnostics ? [.. model.Diagnostics] : null,
            PruningSummary = model.PruningNotes.Count > 0
                ? string.Join("; ", model.PruningNotes)
                : null,
            PruningNotes = options.IncludeDiagnostics && model.PruningNotes.Count > 0
                ? [.. model.PruningNotes]
                : null,
            MaxTokens = options.EstimatedTokens,
            RunReport = options.Report,
        };
    }
}
