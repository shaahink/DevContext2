using System.Collections.Concurrent;

using DevContext.Core.Graph;

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
    /// <summary>Per-service (runnable-project) style assessment. M1.9 / D5 — one entry per
    /// runnable web service, each carrying its local style + stack tags.</summary>
    public ImmutableArray<PerServiceStyle> PerServiceStyles { get; internal set; } = [];
    /// <summary>The detected codebase archetype ("App" or "Library"), set at graph-assembly time.</summary>
    public string? Archetype { get; internal set; }
    /// <summary>T8 — true when every non-test/non-benchmark project lives under a sample path (a sample
    /// COLLECTION like dotnet/aspire-samples): the samples ARE the product, so sample-path suppression
    /// (entries, archetype, topology, library surface) must not apply. Computed root-relatively by the
    /// pipeline at graph-assembly time (<see cref="ProjectClassifier.SamplesAreTheProduct"/>).</summary>
    public bool SamplesAreTheProduct { get; internal set; }
    /// <summary>All discovered types, keyed by fully qualified name.</summary>
    public ConcurrentDictionary<string, TypeDiscovery> Types { get; } = new();

    private ImmutableArray<TypeDiscovery>? _orderedTypes;
    private int _orderedTypesCount = -1;
    /// <summary>D5.3 determinism — FQN-ordered view of <see cref="Types"/>. ConcurrentDictionary
    /// enumeration order is randomized per process (string-hash seeding), so any order-sensitive
    /// consumer (graph assembly's node adds and first-match picks, renders) must read THIS view.
    /// Cached; recomputed when the type count has moved since the last read.</summary>
    public ImmutableArray<TypeDiscovery> OrderedTypes
    {
        get
        {
            var count = Types.Count;
            if (_orderedTypes is not { } cached || _orderedTypesCount != count)
            {
                cached = [.. Types.Values.OrderBy(t => t.Id, StringComparer.Ordinal)];
                _orderedTypes = cached;
                _orderedTypesCount = count;
            }
            return cached;
        }
    }
    /// <summary>All extracted detections (endpoints, handlers, entities, etc.). Arrival order is
    /// nondeterministic while parallel extractors run; the pipeline calls
    /// <see cref="SealDeterministicOrder"/> once after extraction so first-match consumers
    /// (provenance anchor picks) read a stable sequence.</summary>
    public SealableBag<Detection> Detections { get; } = [];
    /// <summary>Call edges discovered during call graph extraction. Same ordering contract as
    /// <see cref="Detections"/>.</summary>
    public SealableBag<CallEdge> CallEdges { get; } = [];
    /// <summary>Set of type IDs that have been pruned from the model.</summary>
    public HashSet<string> PrunedTypeIds { get; } = [];
    /// <summary>Human-readable notes explaining why types were pruned.</summary>
    public List<string> PruningNotes { get; } = [];
    /// <summary>Tracks why each item was included (provenance tracking).</summary>
    public ConcurrentDictionary<string, ConcurrentBag<InclusionReason>> Provenance { get; } = new();
    /// <summary>Diagnostic entries recorded during the pipeline run.</summary>
    public ConcurrentBag<DiagnosticEntry> Diagnostics { get; } = [];
    /// <summary>Aggregated swallowed-failure counters (J1) — drained from PipelineDiagnostics at the
    /// end of the run; the health data stats and the waterfall render (J3).</summary>
    public List<SwallowedFailure> ExtractionFailures { get; } = [];
    /// <summary>Compression results recorded sequentially during the compression stage.</summary>
    public List<CompressionResult> AppliedCompressions { get; } = [];

    /// <summary>Gateway Routes extracted from ocelot.json / YARP config (W7).</summary>
    public List<GatewayRoute> GatewayRoutes { get; } = [];

    /// <summary>Records a provenance reason for why a specific item was included.</summary>
    public void AddProvenance(string itemId, InclusionReason reason)
        => Provenance.GetOrAdd(itemId, _ => []).Add(reason);

    /// <summary>Adds a diagnostic entry to the model.</summary>
    public void AddDiagnostic(DiagnosticLevel level, string source, string message)
        => Diagnostics.Add(new(level, source, message, DateTimeOffset.UtcNow));

    /// <summary>D5.3 determinism — sorts <see cref="Detections"/> and <see cref="CallEdges"/> into a
    /// canonical order. Called once by the pipeline after the last extraction stage: parallel
    /// extractors append in arrival order, and every downstream first-match pick (http ServiceLink
    /// provenance, [bus] seam citation site) would otherwise inherit a run-to-run flap. Ties beyond
    /// file/line/type fall back to the record's ToString — value-identical items are interchangeable,
    /// so only value-distinct items need the total order.</summary>
    public void SealDeterministicOrder()
    {
        Detections.Seal(static (a, b) =>
        {
            var c = string.CompareOrdinal(a.SourceFile, b.SourceFile);
            if (c != 0) return c;
            c = a.LineNumber.CompareTo(b.LineNumber);
            if (c != 0) return c;
            c = string.CompareOrdinal(a.GetType().Name, b.GetType().Name);
            if (c != 0) return c;
            return string.CompareOrdinal(a.ToString(), b.ToString());
        });
        CallEdges.Seal(static (a, b) =>
        {
            var c = string.CompareOrdinal(a.CallerType, b.CallerType);
            if (c != 0) return c;
            c = string.CompareOrdinal(a.CallerMethod, b.CallerMethod);
            if (c != 0) return c;
            c = string.CompareOrdinal(a.CalleeType, b.CalleeType);
            if (c != 0) return c;
            c = string.CompareOrdinal(a.CalleeMethod, b.CalleeMethod);
            if (c != 0) return c;
            c = string.CompareOrdinal(a.CallSiteLocation ?? "", b.CallSiteLocation ?? "");
            if (c != 0) return c;
            return ((int)a.Resolution).CompareTo((int)b.Resolution);
        });
    }
}

/// <summary>Describes a solution file and its constituent project paths.</summary>
public sealed record SolutionInfo(
    string FilePath,
    string Name,
    ImmutableArray<string> ProjectPaths
);

/// <summary>Per-service (runnable project) style assessment. M1.9 / D5 — each runnable web service
/// gets one entry with its local architecture style and stack tags.</summary>
public sealed record PerServiceStyle(
    string ProjectName,
    string Style,
    ImmutableArray<string> Stack
);
