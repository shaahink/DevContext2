using System.Text.Json.Serialization.Metadata;

using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Analysis;

/// <summary>
/// The persisted form of an <see cref="AnalysisSnapshot"/> (J2, Prism D2.0b). The live snapshot is
/// NOT directly serializable — <see cref="SharedAnalysisContext"/> carries Roslyn syntax-node caches
/// (cyclic, process-bound), <see cref="CodeGraph"/>/<see cref="CallGraph"/>/<see cref="ProjectDependencyGraph"/>
/// are constructor-built classes, and <see cref="DiscoveryModel"/> is a mutable accumulator. This DTO
/// pins the SERIALIZABLE SUBSET: everything render/query paths consume after analysis.
///
/// Deliberately NOT persisted:
/// - <c>Options</c> — every load site replaces it with the current request's options.
/// - <c>Analysis.SyntaxNodeCache</c>/<c>BodyFactsCache</c>/<c>AllBodyFacts</c> — analysis-time only;
///   consumed before the snapshot exists, never at render time.
/// - Dry-run fields — dry runs are never cached (<see cref="SnapshotCacheService.SaveAsync"/> refuses).
/// </summary>
public sealed record PersistedSnapshot
{
    public required PersistedModel Model { get; init; }
    public required Scenario Scenario { get; init; }
    public required RunReport Report { get; init; }
    public required PersistedAnalysis Analysis { get; init; }
    public PersistedGraph? Graph { get; init; }
    public MapModel? Map { get; init; }
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    public ImmutableArray<Insight> Insights { get; init; } = [];
    public string RootPath { get; init; } = "";
    public string Explanation { get; init; } = "";
    public ImmutableArray<string> Warnings { get; init; } = [];
    public DateTimeOffset? AnalyzedAtUtc { get; init; }
    public string? GitHead { get; init; }
    public Dictionary<string, FileFingerprint> FileFingerprints { get; init; } = [];
}

/// <summary>Flattened <see cref="DiscoveryModel"/>: concurrent containers become arrays, the sealed
/// <see cref="ArchitectureSignals"/> becomes its signal list, and the get-only per-type Tags bags
/// travel as a parallel id→tags map.</summary>
public sealed record PersistedModel
{
    public SolutionInfo? Solution { get; init; }
    public SolutionScopeNote? ScopeNote { get; init; }
    public ImmutableArray<ProjectInfo> Projects { get; init; } = [];
    public FeatureSignal[] Signals { get; init; } = [];
    public ArchitectureStyle DetectedStyle { get; init; }
    public float StyleConfidence { get; init; }
    public string? StyleDetectedVia { get; init; }
    public ImmutableArray<PerServiceStyle> PerServiceStyles { get; init; } = [];
    public string? Archetype { get; init; }
    public bool SamplesAreTheProduct { get; init; }
    public TypeDiscovery[] Types { get; init; } = [];
    public Dictionary<string, string[]> TypeTags { get; init; } = [];
    public Detection[] Detections { get; init; } = [];
    public CallEdge[] CallEdges { get; init; } = [];
    public string[] PrunedTypeIds { get; init; } = [];
    public string[] PruningNotes { get; init; } = [];
    public Dictionary<string, InclusionReason[]> Provenance { get; init; } = [];
    public DiagnosticEntry[] Diagnostics { get; init; } = [];
    public CompressionResult[] AppliedCompressions { get; init; } = [];
    public GatewayRoute[] GatewayRoutes { get; init; } = [];
    public SwallowedFailure[] ExtractionFailures { get; init; } = [];
}

/// <summary>The render-consumed subset of <see cref="SharedAnalysisContext"/> (file lists, focus
/// points, and the three derived graphs), with the constructor-built graph classes flattened to
/// plain adjacency dictionaries.</summary>
public sealed record PersistedAnalysis
{
    public string[] AllSourceFiles { get; init; } = [];
    public string[] AllContentFiles { get; init; } = [];
    public string[] AllProjectFiles { get; init; } = [];
    public FocusPoint[] FocusPoints { get; init; } = [];
    public FocusPoint[] UnresolvedFocusPoints { get; init; } = [];
    public Dictionary<string, string[]>? ProjectAdjacency { get; init; }
    public Dictionary<string, ArchitectureLayer> ProjectLayerMap { get; init; } = [];
    public Dictionary<string, CallEdge[]>? CallGraphEdges { get; init; }
}

/// <summary>Flattened <see cref="CodeGraph"/>: node list + flat edge list (out-adjacency and the
/// derived inverse adjacency are rebuilt by the <see cref="CodeGraph"/> constructor on load).</summary>
public sealed record PersistedGraph
{
    public GraphNode[] Nodes { get; init; } = [];
    public GraphEdge[] Edges { get; init; } = [];
    public bool IsSparseGraph { get; init; }
    public int HubScopeNodeCount { get; init; }
    public ImmutableArray<LayerViolation> LayerViolations { get; init; } = [];
    public ImmutableArray<Flow> Flows { get; init; } = [];
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    public ImmutableArray<EventWire> EventWiring { get; init; } = [];
}

/// <summary>Maps <see cref="AnalysisSnapshot"/> ⇄ <see cref="PersistedSnapshot"/>. Lives in Core on
/// purpose: rehydration writes <see cref="DiscoveryModel"/>'s internal setters and re-seals
/// <see cref="ArchitectureSignals"/>, which only this assembly may do.</summary>
public static class SnapshotPersistence
{
    public static PersistedSnapshot FromSnapshot(AnalysisSnapshot s) => new()
    {
        Model = FromModel(s.Model),
        Scenario = s.Scenario,
        Report = s.Report,
        Analysis = FromAnalysis(s.Analysis),
        Graph = s.Graph is null ? null : FromGraph(s.Graph),
        Map = s.Map,
        Entries = s.Entries,
        Insights = s.Insights,
        RootPath = s.RootPath,
        Explanation = s.Explanation,
        Warnings = s.Warnings,
        AnalyzedAtUtc = s.AnalyzedAtUtc,
        GitHead = s.GitHead,
        FileFingerprints = s.FileFingerprints.ToDictionary(kv => kv.Key, kv => kv.Value),
    };

    /// <summary>Rehydrates a snapshot. <c>Options</c> comes back as a bare default — every load site
    /// replaces it with the current request's options (<c>snapshot with { Options = … }</c>).</summary>
    public static AnalysisSnapshot ToSnapshot(PersistedSnapshot p) => new()
    {
        Model = ToModel(p.Model),
        Analysis = ToAnalysis(p.Analysis),
        Scenario = p.Scenario,
        Options = new ExtractionOptions(),
        Report = p.Report,
        RootPath = p.RootPath,
        Explanation = p.Explanation,
        Warnings = p.Warnings,
        AnalyzedAtUtc = p.AnalyzedAtUtc,
        GitHead = p.GitHead,
        FileFingerprints = p.FileFingerprints.ToImmutableDictionary(
            kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase),
        Graph = p.Graph is null ? null : ToGraph(p.Graph),
        Map = p.Map,
        Entries = p.Entries,
        Insights = p.Insights,
    };

    private static PersistedModel FromModel(DiscoveryModel m) => new()
    {
        Solution = m.Solution,
        ScopeNote = m.ScopeNote,
        Projects = m.Projects,
        Signals = [.. m.Architecture.All.Values],
        DetectedStyle = m.DetectedStyle,
        StyleConfidence = m.StyleConfidence,
        StyleDetectedVia = m.StyleDetectedVia,
        PerServiceStyles = m.PerServiceStyles,
        Archetype = m.Archetype,
        SamplesAreTheProduct = m.SamplesAreTheProduct,
        Types = [.. m.Types.Values],
        TypeTags = m.Types.Values.Where(t => !t.Tags.IsEmpty)
            .ToDictionary(t => t.Id, t => t.Tags.ToArray()),
        Detections = [.. m.Detections],
        CallEdges = [.. m.CallEdges],
        PrunedTypeIds = [.. m.PrunedTypeIds],
        PruningNotes = [.. m.PruningNotes],
        Provenance = m.Provenance.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()),
        Diagnostics = [.. m.Diagnostics],
        AppliedCompressions = [.. m.AppliedCompressions],
        GatewayRoutes = [.. m.GatewayRoutes],
        ExtractionFailures = [.. m.ExtractionFailures],
    };

    private static DiscoveryModel ToModel(PersistedModel p)
    {
        var m = new DiscoveryModel
        {
            Solution = p.Solution,
            ScopeNote = p.ScopeNote,
            Projects = p.Projects,
            DetectedStyle = p.DetectedStyle,
            StyleConfidence = p.StyleConfidence,
            StyleDetectedVia = p.StyleDetectedVia,
            PerServiceStyles = p.PerServiceStyles,
            Archetype = p.Archetype,
            SamplesAreTheProduct = p.SamplesAreTheProduct,
        };
        foreach (var sig in p.Signals) m.Architecture.Register(sig);
        m.Architecture.Seal();
        foreach (var t in p.Types)
        {
            // ConcurrentBag enumerates newest-first for same-thread adds, so re-add REVERSED to
            // keep the rehydrated enumeration order identical to the persisted one.
            if (p.TypeTags.TryGetValue(t.Id, out var tags))
                for (var i = tags.Length - 1; i >= 0; i--) t.Tags.Add(tags[i]);
            m.Types[t.Id] = t;
        }
        // SealableBag preserves insertion order (D5.3), so re-add in the persisted (sealed) order.
        foreach (var d in p.Detections) m.Detections.Add(d);
        foreach (var ce in p.CallEdges) m.CallEdges.Add(ce);
        m.PrunedTypeIds.UnionWith(p.PrunedTypeIds);
        m.PruningNotes.AddRange(p.PruningNotes);
        foreach (var kv in p.Provenance)
            for (var i = kv.Value.Length - 1; i >= 0; i--) m.AddProvenance(kv.Key, kv.Value[i]);
        for (var i = p.Diagnostics.Length - 1; i >= 0; i--) m.Diagnostics.Add(p.Diagnostics[i]);
        m.AppliedCompressions.AddRange(p.AppliedCompressions);
        m.GatewayRoutes.AddRange(p.GatewayRoutes);
        m.ExtractionFailures.AddRange(p.ExtractionFailures);
        return m;
    }

    private static PersistedAnalysis FromAnalysis(SharedAnalysisContext a) => new()
    {
        AllSourceFiles = [.. a.AllSourceFiles],
        AllContentFiles = [.. a.AllContentFiles],
        AllProjectFiles = [.. a.AllProjectFiles],
        FocusPoints = [.. a.FocusPoints],
        UnresolvedFocusPoints = [.. a.UnresolvedFocusPoints],
        ProjectAdjacency = a.ProjectGraph?.AdjacencyList
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()),
        ProjectLayerMap = a.ProjectLayerMap.ToDictionary(kv => kv.Key, kv => kv.Value),
        CallGraphEdges = a.CallGraph?.Edges.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()),
    };

    private static SharedAnalysisContext ToAnalysis(PersistedAnalysis p) => new()
    {
        AllSourceFiles = p.AllSourceFiles,
        AllContentFiles = p.AllContentFiles,
        AllProjectFiles = p.AllProjectFiles,
        FocusPoints = p.FocusPoints,
        UnresolvedFocusPoints = p.UnresolvedFocusPoints,
        ProjectGraph = p.ProjectAdjacency is null
            ? null
            : new ProjectDependencyGraph(p.ProjectAdjacency
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray())),
        ProjectLayerMap = p.ProjectLayerMap.ToFrozenDictionary(),
        CallGraph = p.CallGraphEdges is null
            ? null
            : new CallGraph(p.CallGraphEdges
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray())),
    };

    private static PersistedGraph FromGraph(CodeGraph g) => new()
    {
        Nodes = [.. g.Nodes],
        Edges = [.. g.AllEdges],
        IsSparseGraph = g.IsSparseGraph,
        HubScopeNodeCount = g.HubScopeNodeCount,
        LayerViolations = g.LayerViolations,
        Flows = g.Flows,
        Entries = g.Entries,
        EventWiring = g.EventWiring,
    };

    private static CodeGraph ToGraph(PersistedGraph p)
    {
        var nodes = p.Nodes.ToDictionary(n => n.Id);
        var outEdges = p.Edges.GroupBy(e => e.From)
            .ToDictionary(gr => gr.Key, gr => gr.ToImmutableArray());
        return new CodeGraph(nodes, outEdges)
        {
            IsSparseGraph = p.IsSparseGraph,
            HubScopeNodeCount = p.HubScopeNodeCount,
            LayerViolations = p.LayerViolations,
            Flows = p.Flows,
            Entries = p.Entries,
            EventWiring = p.EventWiring,
        };
    }
}
