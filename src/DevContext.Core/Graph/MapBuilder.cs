namespace DevContext.Core.Graph;

/// <summary>The orientation artifact: architecture, topology, packages, entry inventory, cross-cutting — no code.</summary>
public sealed record MapModel
{
    /// <summary>Detected architecture style.</summary>
    public string Style { get; init; } = "Unknown";
    /// <summary>Style confidence 0..1.</summary>
    public float StyleConfidence { get; init; }
    /// <summary>Human-readable evidence for the style (not a name-substring guess).</summary>
    public string? StyleEvidence { get; init; }
    /// <summary>All entry points, grouped/ordered for display by the renderer.</summary>
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    /// <summary>Project dependency tree.</summary>
    public ImmutableArray<ProjectNode> Topology { get; init; } = [];
    /// <summary>NuGet packages, grouped for compact display.</summary>
    public ImmutableArray<PackageGroup> Packages { get; init; } = [];
    /// <summary>Aggregate roots (from EfEntityDetection.IsAggregate).</summary>
    public ImmutableArray<string> Aggregates { get; init; } = [];
    /// <summary>MediatR pipeline behaviors (cross-cutting, shown once).</summary>
    public ImmutableArray<string> PipelineBehaviors { get; init; } = [];
}

/// <summary>A project and the projects it references (for the dependency tree).</summary>
public sealed record ProjectNode(string Name, ImmutableArray<string> DependsOn);

/// <summary>NuGet packages grouped for compact display.</summary>
public sealed record PackageGroup(string Label, ImmutableArray<string> Packages);

/// <summary>
/// Builds the <see cref="MapModel"/> — the no-code orientation view. Everything here is structural and
/// already-parsed; there is no scoring. Skeleton: the agent fills each facet in P1. The entry inventory
/// is passed through from GraphBuilder as-is. <c>Build</c> is static for now (matching the
/// <c>ArchitectureStyleDetector.Detect</c> convention); make it instance once it gains collaborators
/// (e.g. an evidence-based style detector) in P1.
/// </summary>
public sealed class MapBuilder
{
    /// <summary>Assembles the map from the discovery model, graph, and entry inventory.</summary>
    public static MapModel Build(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        // TODO(agent, P1): replace model.DetectedStyle with EVIDENCE-based style detection
        //   (reference direction + folder roles + aggregate/MediatR presence), not project-name substrings.
        // TODO(agent, P1): Topology from model.Projects' ProjectReferences; scope to ONE solution.
        // TODO(agent, P1): Packages — surface model.Projects[].PackageReferences (already parsed, never shown today).
        // TODO(agent, P1): Aggregates from EfEntityDetection.IsAggregate; PipelineBehaviors from a small new detector.
        return new MapModel
        {
            Style = model.DetectedStyle.ToString(),
            StyleConfidence = model.StyleConfidence,
            StyleEvidence = model.StyleDetectedVia,
            Entries = entries,
        };
    }
}
