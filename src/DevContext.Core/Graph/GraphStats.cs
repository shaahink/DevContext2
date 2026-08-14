using DevContext.Core.Models;

namespace DevContext.Core.Graph;

/// <summary>
/// Computes the graph-shaped run stats — per-seam edge counts with their resolution split, and how many
/// entry points resolved a dispatch target. This is the single source for both the CLI <c>--stats</c>
/// view and the desktop stats page, replacing the meaningless type-funnel on the Map/Trace path. It's a
/// genuine quality dashboard: as detection and (later) semantic resolution improve, "approx" shrinks and
/// entry-target coverage rises — visible progress, run over run.
/// </summary>
public static class GraphStats
{
    /// <summary>Tallies every out-edge by kind (with the syntactic/approx share) and counts entries that
    /// resolved a target. Cheap — one pass over the graph's adjacency.</summary>
    // R1.1 (#24, 2026-08-14): the deep-spine ratio that used to ride this tuple is RETIRED.
    // Re-measured on 12 poles against the post-E1 graph
    // (eval-results/2026-08-14/r1-metrics/threshold-grid-post-e1.txt): 1.000 on eight of them
    // (TodoApi, VerticalSlice, CleanArchitecture, MediatR, dotnet-podcasts, eshop-microservices,
    // Hangfire, self), 0.982 on eShop and 0.994 on FastEndpoints, and 0 only on the two poles with
    // zero entries, where it was a divide-by-zero artifact rather than a measurement. Identical
    // shape to the 2026-08-02 grid, so E1's 8x lift in Semantic share moved it not at all: ">=2
    // steps" means "the entry reaches one thing", which is now true of essentially every entry that
    // exists. It shipped as a coverage row ("Deep spine (>=2) | 107/109 (98%)") that reads the same
    // on every repo. The bar was NOT raised to make it separate again — where a useful one sits is
    // a question about the step DISTRIBUTION, which no surface exposes, and inventing a 3 or a 4 by
    // eye is not a calibration. Its design origin was proposal-loom.md:312 ("entries with >=2-deep
    // spine >=70% on non-CQRS repos"), a build-time acceptance bar that has been met with room to
    // spare on every pole measured since.
    public static (ImmutableArray<SeamStat> Seams, int EntriesWithTarget) Compute(
        CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        // V1.1 (#25): the split is TALLIED per tier from EdgeConfidence — the one definition — and
        // carried on the row. It used to record only Count and Approx, which left every consumer to
        // infer "verified = Count - Approx"; that inference counts Resolution.Join (the enum's
        // DEFAULT, i.e. every edge nobody labelled) as Roslyn-verified, and it is the reason the CLI
        // and the desktop app printed opposite verdicts for the same edge.
        var byKind = new Dictionary<EdgeKind, (int Count, int Verified, int Joined, int Approx)>();
        foreach (var node in graph.Nodes)
        {
            foreach (var e in graph.OutEdges(node.Id))
            {
                byKind.TryGetValue(e.Kind, out var c);
                var tier = EdgeConfidence.TierOf(e);
                byKind[e.Kind] = (
                    c.Count + 1,
                    c.Verified + (tier == EdgeTier.Verified ? 1 : 0),
                    c.Joined + (tier == EdgeTier.Joined ? 1 : 0),
                    c.Approx + (tier == EdgeTier.Approximate ? 1 : 0));
            }
        }

        var seams = byKind
            .OrderBy(kv => kv.Key)
            .Select(kv => new SeamStat(kv.Key.ToString(), kv.Value.Count, kv.Value.Verified, kv.Value.Joined, kv.Value.Approx))
            .ToImmutableArray();

        var withTarget = entries.IsDefaultOrEmpty
            ? 0
            : entries.Count(e => !string.IsNullOrEmpty(e.Target));

        return (seams, withTarget);
    }
}
