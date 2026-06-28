namespace DevContext.Core.Graph;

/// <summary>
/// The single place a focus string resolves to an <see cref="EntryPoint"/> — shared by the
/// CLI/pipeline render branch and the desktop picker so both behave identically. Resolution order:
/// (1) an entry in the inventory whose title/route matches; (2) any drillable Type node by name (a
/// typed <c>--focus</c>, e.g. <c>OrderService</c> or <c>OrderService:Process</c>), producing a
/// synthetic <see cref="EntryPointKind.PublicApi"/> entry rooted on that node so a trace can walk its
/// out-edges. Before the Type+tags collapse this lived inline in the pipeline and matched the old
/// Handler/Service node kinds; now every class is one Type node, so the node filter is Type/EntryPoint.
/// </summary>
public static class EntryPointResolver
{
    /// <summary>Resolves <paramref name="focus"/> against the entry inventory, then the graph. Null when
    /// nothing matches (caller renders the Map).</summary>
    public static EntryPoint? Resolve(IReadOnlyList<EntryPoint> entries, CodeGraph graph, string? focus)
    {
        if (string.IsNullOrWhiteSpace(focus)) return null;
        var f = focus.Trim();

        var byTitle = entries.FirstOrDefault(e =>
            string.Equals(e.Title, f, StringComparison.OrdinalIgnoreCase));
        if (byTitle is not null) return byTitle;

        return ResolveFromNode(graph, f);
    }

    /// <summary>Finds a Type/EntryPoint node by short name or FQN suffix. For a "Type:Method" focus it
    /// anchors on the <b>Member</b> node when one exists (member-origin: this is what makes two sibling
    /// methods produce different traces); it falls back to the Type node otherwise (a method with no
    /// wiring). For a bare type it prefers the node with the most out-edges so a focus that matches both a
    /// bare class and a richer twin lands on the one that actually goes somewhere.</summary>
    private static EntryPoint? ResolveFromNode(CodeGraph graph, string focus)
    {
        var name = focus;
        string? method = null;
        var colon = name.IndexOf(':');
        if (colon > 0)
        {
            method = name[(colon + 1)..].Trim();
            name = name[..colon];
        }

        GraphNode? best = null;
        foreach (var node in graph.Nodes)
        {
            if (node.Kind is not (NodeKind.Type or NodeKind.EntryPoint)) continue;
            if (!string.Equals(node.Title, name, StringComparison.OrdinalIgnoreCase)
                && !node.Id.Key.EndsWith("." + name, StringComparison.OrdinalIgnoreCase)) continue;

            // "Type:Method" → anchor on the Member node that originates this method's edges, so the trace
            // shows only this method's wiring (sibling methods diverge). Prefer the first candidate Type
            // that actually declares the member.
            if (method is { Length: > 0 } && node.Kind is NodeKind.Type
                && graph.Node(NodeId.ForMember(node.Id.Key, method)) is { } memberNode)
            {
                return new EntryPoint(EntryPointKind.PublicApi, memberNode.Title, memberNode.Id)
                {
                    Provenance = memberNode.FilePath,
                };
            }

            if (best is null || graph.OutEdges(node.Id).Length > graph.OutEdges(best.Id).Length)
                best = node;
        }

        return best is null
            ? null
            : new EntryPoint(EntryPointKind.PublicApi, best.Title, best.Id) { Provenance = best.FilePath };
    }
}
