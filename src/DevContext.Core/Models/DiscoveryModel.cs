using System.Collections.Concurrent;

namespace DevContext.Core.Models;

public sealed class DiscoveryModel
{
    public SolutionInfo? Solution { get; internal set; }
    public ImmutableArray<ProjectInfo> Projects { get; internal set; } = [];
    public ArchitectureSignals Architecture { get; } = new();
    public ArchitectureStyle DetectedStyle { get; internal set; } = ArchitectureStyle.Unknown;
    public float StyleConfidence { get; internal set; }
    public string? StyleDetectedVia { get; internal set; }
    public ConcurrentDictionary<string, TypeDiscovery> Types { get; } = new();
    public ConcurrentBag<Detection> Detections { get; } = [];
    public ConcurrentBag<CallEdge> CallEdges { get; } = [];
    public HashSet<string> PrunedTypeIds { get; } = [];
    public List<string> PruningNotes { get; } = [];
    public ConcurrentDictionary<string, ConcurrentBag<InclusionReason>> Provenance { get; } = new();
    public ConcurrentBag<DiagnosticEntry> Diagnostics { get; } = [];
    public TokenBudget Budget { get; internal set; } = TokenBudget.Default;

    public void AddProvenance(string itemId, InclusionReason reason)
        => Provenance.GetOrAdd(itemId, _ => []).Add(reason);

    public void AddDiagnostic(DiagnosticLevel level, string source, string message)
        => Diagnostics.Add(new(level, source, message, DateTimeOffset.UtcNow));
}

public sealed record SolutionInfo(
    string FilePath,
    string Name,
    ImmutableArray<string> ProjectPaths
);
