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
            json, estimatedTokens, [], sw.Elapsed, "2.0"));
    }

    private static DevContextOutput BuildOutput(DiscoveryModel model, RenderOptions options)
    {
        return new DevContextOutput
        {
            SchemaVersion = "2.0",
            Solution = model.Solution is not null
                ? new SolutionOutput(
                    model.Solution.Name,
                    model.Solution.FilePath,
                    [.. model.Solution.ProjectPaths])
                : null,
            Architecture = new ArchitectureOutput(
                model.DetectedStyle.ToString(),
                model.StyleConfidence,
                model.StyleDetectedVia ?? ""),
            Signals = [.. model.Architecture.All
                .Where(kvp => kvp.Value.Detected)
                .Select(kvp => new SignalOutput(kvp.Key, kvp.Value.Confidence, kvp.Value.DetectedVia))],
            TypesSummary = new TypesSummaryOutput(
                model.Types.Count,
                model.Types.Values.Count(t => !t.IsPruned),
                model.Types.Values.Count(t => t.IsPruned)),
            Detections = [.. model.Detections.Select(ToDetectionOutput)],
            Diagnostics = options.IncludeDiagnostics
                ? [.. model.Diagnostics.Select(d => new DiagnosticOutput(d.Level.ToString(), d.Source, d.Message))]
                : [],
        };
    }

    private static DetectionOutput ToDetectionOutput(Detection detection)
    {
        return new DetectionOutput(
            detection.GetType().Name,
            detection.SourceFile,
            detection.LineNumber,
            detection.Confidence);
    }

    private sealed record DevContextOutput
    {
        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; init; } = "2.0";

        [JsonPropertyName("solution")]
        public SolutionOutput? Solution { get; init; }

        [JsonPropertyName("architecture")]
        public ArchitectureOutput Architecture { get; init; } = new("Unknown", 0, "");

        [JsonPropertyName("signals")]
        public IReadOnlyList<SignalOutput> Signals { get; init; } = [];

        [JsonPropertyName("typesSummary")]
        public TypesSummaryOutput TypesSummary { get; init; } = new(0, 0, 0);

        [JsonPropertyName("detections")]
        public IReadOnlyList<DetectionOutput> Detections { get; init; } = [];

        [JsonPropertyName("diagnostics")]
        public IReadOnlyList<DiagnosticOutput> Diagnostics { get; init; } = [];
    }

    private sealed record SolutionOutput(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("filePath")] string FilePath,
        [property: JsonPropertyName("projects")] IReadOnlyList<string> Projects);

    private sealed record ArchitectureOutput(
        [property: JsonPropertyName("style")] string Style,
        [property: JsonPropertyName("confidence")] float Confidence,
        [property: JsonPropertyName("detectedVia")] string DetectedVia);

    private sealed record SignalOutput(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("confidence")] float Confidence,
        [property: JsonPropertyName("detectedVia")] string DetectedVia);

    private sealed record TypesSummaryOutput(
        [property: JsonPropertyName("total")] int Total,
        [property: JsonPropertyName("active")] int Active,
        [property: JsonPropertyName("pruned")] int Pruned);

    private sealed record DetectionOutput(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("sourceFile")] string SourceFile,
        [property: JsonPropertyName("lineNumber")] int LineNumber,
        [property: JsonPropertyName("confidence")] float Confidence);

    private sealed record DiagnosticOutput(
        [property: JsonPropertyName("level")] string Level,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("message")] string Message);
}
