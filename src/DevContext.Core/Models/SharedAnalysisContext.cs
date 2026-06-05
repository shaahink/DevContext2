using System.Collections.Frozen;

namespace DevContext.Core.Models;

public sealed class SharedAnalysisContext
{
    public IReadOnlyList<string> AllSourceFiles { get; set; } = [];
    public IReadOnlyList<string> AllProjectFiles { get; set; } = [];
    public IReadOnlyList<FocusPoint> FocusPoints { get; set; } = [];
    public ProjectDependencyGraph? ProjectGraph { get; set; }
    public IReadOnlyDictionary<string, ArchitectureLayer> ProjectLayerMap { get; set; }
        = FrozenDictionary<string, ArchitectureLayer>.Empty;
    public CallGraph? CallGraph { get; set; }
}

public sealed class ProjectDependencyGraph
{
    public IReadOnlyDictionary<string, ImmutableArray<string>> AdjacencyList { get; }
    public ProjectDependencyGraph(Dictionary<string, ImmutableArray<string>> adjacency)
    {
        AdjacencyList = adjacency.ToFrozenDictionary();
    }
}

public sealed class CallGraph
{
    public IReadOnlyDictionary<string, ImmutableArray<CallEdge>> Edges { get; }
    public CallGraph(Dictionary<string, ImmutableArray<CallEdge>> edges)
    {
        Edges = edges.ToFrozenDictionary();
    }
}

public sealed record CallEdge(
    string CallerType,
    string CallerMethod,
    string CalleeType,
    string CalleeMethod,
    string? CallSiteLocation
);
