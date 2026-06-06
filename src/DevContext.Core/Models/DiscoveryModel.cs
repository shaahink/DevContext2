using System.Collections.Concurrent;

namespace DevContext.Core.Models;

/// <summary>Represents the result of running the discovery pipeline, containing all extracted data.</summary>
public sealed class DiscoveryModel
{
    /// <summary>Information about the solution file, if found.</summary>
    public SolutionInfo? Solution { get; internal set; }
    /// <summary>Projects discovered in the solution.</summary>
    public ImmutableArray<ProjectInfo> Projects { get; internal set; } = [];
    /// <summary>Architecture signals detected during extraction.</summary>
    public ArchitectureSignals Architecture { get; } = new();
    /// <summary>The detected overall architecture style.</summary>
    public ArchitectureStyle DetectedStyle { get; internal set; } = ArchitectureStyle.Unknown;
    /// <summary>Confidence level of the detected architecture style.</summary>
    public float StyleConfidence { get; internal set; }
    /// <summary>Indicates which detector or signal identified the architecture style.</summary>
    public string? StyleDetectedVia { get; internal set; }
    /// <summary>All discovered types, keyed by fully qualified name.</summary>
    public ConcurrentDictionary<string, TypeDiscovery> Types { get; } = new();
    /// <summary>All extracted detections (endpoints, handlers, entities, etc.).</summary>
    public ConcurrentBag<Detection> Detections { get; } = [];
    /// <summary>Call edges discovered during call graph extraction.</summary>
    public ConcurrentBag<CallEdge> CallEdges { get; } = [];
    /// <summary>Set of type IDs that have been pruned from the model.</summary>
    public HashSet<string> PrunedTypeIds { get; } = [];
    /// <summary>Human-readable notes explaining why types were pruned.</summary>
    public List<string> PruningNotes { get; } = [];
    /// <summary>Tracks why each item was included (provenance tracking).</summary>
    public ConcurrentDictionary<string, ConcurrentBag<InclusionReason>> Provenance { get; } = new();
    /// <summary>Diagnostic entries recorded during the pipeline run.</summary>
    public ConcurrentBag<DiagnosticEntry> Diagnostics { get; } = [];
    /// <summary>Token budget configuration for the model.</summary>
    public TokenBudget Budget { get; internal set; } = TokenBudget.Default;
    /// <summary>Compression results recorded sequentially during the compression stage.</summary>
    public List<CompressionResult> AppliedCompressions { get; } = [];

    /// <summary>Records a provenance reason for why a specific item was included.</summary>
    public void AddProvenance(string itemId, InclusionReason reason)
        => Provenance.GetOrAdd(itemId, _ => []).Add(reason);

    /// <summary>Adds a diagnostic entry to the model.</summary>
    public void AddDiagnostic(DiagnosticLevel level, string source, string message)
        => Diagnostics.Add(new(level, source, message, DateTimeOffset.UtcNow));
}

/// <summary>Describes a solution file and its constituent project paths.</summary>
public sealed record SolutionInfo(
    string FilePath,
    string Name,
    ImmutableArray<string> ProjectPaths
);
