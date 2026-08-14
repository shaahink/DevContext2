using System.Collections.Immutable;

using DevContext.Core.Graph;

namespace DevContext.Core.Insights;

/// <summary>L4.3 — Decomposes "X% confidence" into honest per-aspect breakdowns
/// so a human or agent can judge trustworthiness without reading engine internals.
/// <para>Batch E (R2 §2.E item 2): the <c>OverallConfidence</c> member is GONE. It was
/// <c>verified/total * 0.7 + (total-approx)/total * 0.3</c> — a blend of two of the other fields with
/// invented weights, i.e. not a countable fact about the repo. The home chip printed it under the
/// label "verified" while the chip's own tooltip printed <see cref="VerifiedEdgePct"/> for the same
/// word, so the number and its explanation disagreed on every repo that isn't 100% semantic. Every
/// member below is now a COUNT or a ratio of two counts that appear beside it.</para></summary>
public sealed record ConfidenceLedger(
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
            return new ConfidenceLedger(0, 0, 0, [], 0, 0, 0, 0, 0, 0);

        // V1.1 (#25): both counts come from EdgeConfidence, the one definition. `approx` used to be
        // `Syntactic || Confidence < 1.0f` — a FOURTH spelling of the word, and one that could put a
        // single edge in two buckets at once (a Semantic seam target ships at 0.95, so it counted as
        // verified AND as approx). Confidence is a separate axis and stays out of the tier.
        var verified = edgeList.Count(EdgeConfidence.IsVerified);
        var approx = edgeList.Count(EdgeConfidence.IsApproximate);

        var perSeam = edgeList
            .GroupBy(e => e.Kind)
            .Select(g =>
            {
                var sVerified = g.Count(EdgeConfidence.IsVerified);
                var sApprox = g.Count(EdgeConfidence.IsApproximate);
                var sJoined = g.Count(EdgeConfidence.IsJoined);
                return new SeamConfidence(g.Key.ToString(), g.Count(), sVerified, sApprox) { Joined = sJoined };
            })
            .OrderByDescending(s => s.Total)
            .ToImmutableArray();

        var httpEntries = entries.Where(e => e.Kind == EntryPointKind.HttpEndpoint).ToList();
        var endpointsWithAuth = httpEntries.Count(e => !e.AuthAttributes.IsDefaultOrEmpty);
        var totalEndpoints = httpEntries.Count;

        var entriesWithTarget = entries.Count(e => e.Target is not null);
        var totalEntries = entries.Length;

        return new ConfidenceLedger(
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

/// <summary>Per-seam confidence breakdown. V1.1 (#25): <see cref="Verified"/>, <see cref="Joined"/>
/// and <see cref="Approx"/> are the three <see cref="Graph.EdgeTier"/> counts and partition
/// <see cref="Total"/> exactly — nothing here is a subtraction.</summary>
public sealed record SeamConfidence(
    string Seam,
    int Total,
    int Verified,
    int Approx)
{
    /// <summary>Edges derived by joining two detections — neither Roslyn-verified nor a string guess.</summary>
    public int Joined { get; init; }
}
