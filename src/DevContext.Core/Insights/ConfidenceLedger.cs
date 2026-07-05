using System.Collections.Immutable;

using DevContext.Core.Graph;

namespace DevContext.Core.Insights;

/// <summary>L4.3 — Decomposes "X% confidence" into honest per-aspect breakdowns
/// so a human or agent can judge trustworthiness without reading engine internals.</summary>
public sealed record ConfidenceLedger(
    double OverallConfidence,
    double VerifiedEdgePct,
    double ApproxEdgePct,
    int TotalEdges,
    ImmutableArray<SeamConfidence> PerSeam,
    double AuthCoveragePct,
    int EndpointsWithAuth,
    int TotalEndpoints,
    double EntryTargetPct,
    int EntriesWithTarget,
    int TotalEntries)
{
    /// <summary>Computes the ledger from a built graph and entry inventory.</summary>
    public static ConfidenceLedger Compute(CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var edgeList = new List<GraphEdge>();
        foreach (var node in graph.Nodes)
            edgeList.AddRange(graph.OutEdges(node.Id));

        var totalEdges = edgeList.Count;
        if (totalEdges == 0)
            return new ConfidenceLedger(0, 0, 0, 0, [], 0, 0, 0, 0, 0, 0);

        var verified = edgeList.Count(e => e.Resolution == Resolution.Semantic);
        var approx = edgeList.Count(e => e.Resolution == Resolution.Syntactic || e.Confidence < 1.0f);

        var perSeam = edgeList
            .GroupBy(e => e.Kind)
            .Select(g =>
            {
                var sVerified = g.Count(e => e.Resolution == Resolution.Semantic);
                var sApprox = g.Count(e => e.Resolution == Resolution.Syntactic || e.Confidence < 1.0f);
                return new SeamConfidence(g.Key.ToString(), g.Count(), sVerified, sApprox);
            })
            .OrderByDescending(s => s.Total)
            .ToImmutableArray();

        var httpEntries = entries.Where(e => e.Kind == EntryPointKind.HttpEndpoint).ToList();
        var endpointsWithAuth = httpEntries.Count(e => !e.AuthAttributes.IsDefaultOrEmpty);
        var totalEndpoints = httpEntries.Count;

        var entriesWithTarget = entries.Count(e => e.Target is not null);
        var totalEntries = entries.Length;

        var overall = totalEdges > 0
            ? (double)verified / totalEdges * 0.7 + (double)(totalEdges - approx) / totalEdges * 0.3
            : 0;

        return new ConfidenceLedger(
            Math.Round(overall, 2),
            Math.Round(totalEdges > 0 ? (double)verified / totalEdges : 0, 2),
            Math.Round(totalEdges > 0 ? (double)approx / totalEdges : 0, 2),
            totalEdges,
            perSeam,
            Math.Round(totalEndpoints > 0 ? (double)endpointsWithAuth / totalEndpoints : 0, 2),
            endpointsWithAuth,
            totalEndpoints,
            Math.Round(totalEntries > 0 ? (double)entriesWithTarget / totalEntries : 0, 2),
            entriesWithTarget,
            totalEntries);
    }
}

/// <summary>Per-seam confidence breakdown.</summary>
public sealed record SeamConfidence(
    string Seam,
    int Total,
    int Verified,
    int Approx);
