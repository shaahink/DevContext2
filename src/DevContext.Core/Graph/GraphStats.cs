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
    public static (ImmutableArray<SeamStat> Seams, int EntriesWithTarget, int EntriesWithDeepSpine, double DeepSpineRatio) Compute(
        CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var byKind = new Dictionary<EdgeKind, (int Count, int Approx)>();
        foreach (var node in graph.Nodes)
        {
            foreach (var e in graph.OutEdges(node.Id))
            {
                byKind.TryGetValue(e.Kind, out var c);
                byKind[e.Kind] = (c.Count + 1, c.Approx + (e.Resolution == Resolution.Syntactic ? 1 : 0));
            }
        }

        var seams = byKind
            .OrderBy(kv => kv.Key)
            .Select(kv => new SeamStat(kv.Key.ToString(), kv.Value.Count, kv.Value.Approx))
            .ToImmutableArray();

        var withTarget = entries.IsDefaultOrEmpty
            ? 0
            : entries.Count(e => !string.IsNullOrEmpty(e.Target));

        // G10.1 RE-MEASURED 2026-08-02, 11 poles (eval-results/2026-08-02/G10/threshold-grid.txt):
        // THIS RATIO IS SATURATED. It reads 1.000 on CleanArchitecture, MediatR, dotnet-podcasts,
        // self and DntSite, 0.982 on eShop (107/109) and 0.961 on wolverine (49/51). A >=2-step
        // spine means "the entry reaches one thing", which was a real distinction when entries
        // routinely resolved to nothing and is now true of very nearly every entry that exists — so
        // the number the report prints as coverage ("Deep spine (>=2) | 107/109 (98%)",
        // ReportRenderer) is the same on every repo and separates none of them.
        //
        // The bar is NOT raised here. Where a useful one sits is a question about the step
        // distribution, which no surface currently exposes, and inventing a 3 or a 4 to make the row
        // look discriminating would be re-calibrating a shipped metric by eye. Tracked as a
        // conductor bug; measured here so the next reader does not mistake 100% for good news.
        var flows = graph.Flows;
        var totalEntries = entries.IsDefaultOrEmpty ? 0 : entries.Length;
        var deepCount = totalEntries == 0
            ? 0
            : flows.Count(f => f.Steps.Length >= 2);
        var ratio = totalEntries == 0 ? 0.0 : (double)deepCount / totalEntries;

        return (seams, withTarget, deepCount, ratio);
    }
}
