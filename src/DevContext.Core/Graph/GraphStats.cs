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
    public static (ImmutableArray<SeamStat> Seams, int EntriesWithTarget) Compute(
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

        return (seams, withTarget);
    }
}
