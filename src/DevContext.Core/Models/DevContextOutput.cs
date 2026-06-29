namespace DevContext.Core.Models;

/// <summary>
/// Stable output schema for DevContext JSON rendering.
/// This is the public contract — internal DiscoveryModel can evolve independently.
/// Schema version is bumped on breaking changes.
/// </summary>
public sealed record DevContextOutput
{
    public string SchemaVersion { get; init; } = "1.1";
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public SolutionOutput? Solution { get; init; }
    public ArchitectureOutput? Architecture { get; init; }
    public string? Archetype { get; init; }
    public IReadOnlyList<SignalOutput> Signals { get; init; } = [];
    public ProjectsOutput Projects { get; init; } = new();
    public TypesOutput TypesSummary { get; init; } = new();
    public IReadOnlyList<object> Detections { get; init; } = [];
    public IReadOnlyList<DiagnosticEntry>? Diagnostics { get; init; }
    public string? PruningSummary { get; init; }
    public IReadOnlyList<string>? PruningNotes { get; init; }
    public int MaxTokens { get; init; }
    public RunReport? RunReport { get; init; }
}

public sealed record SolutionOutput(string Name, string FilePath, IReadOnlyList<string> ProjectPaths);
public sealed record ArchitectureOutput(string Style, float Confidence);
public sealed record SignalOutput(string Key, float Confidence, bool Detected);
public sealed record ProjectsOutput(int Count = 0, IReadOnlyList<string> Names = null!);
public sealed record TypesOutput(int Found = 0, int InOutput = 0, double PrunedPercent = 0);
