namespace DevContext.Core.Graph;

/// <summary>
/// The single place a focus string resolves to an <see cref="EntryPoint"/> — shared by the
/// CLI/pipeline render branch and the desktop picker so both behave identically. Resolution order:
/// (1) an entry in the inventory whose title/route matches; (2) any drillable Type node by name (a
/// typed <c>--focus</c>, e.g. <c>OrderService</c> or <c>OrderService:Process</c>), producing a
/// synthetic <see cref="EntryPointKind.PublicApi"/> entry rooted on that node so a trace can walk its
/// out-edges. Before the Type+tags collapse this lived inline in the pipeline and matched the old
/// Handler/Service node kinds; now every class is one Type node, so the node filter is Type/EntryPoint.
/// (3) R4 item 2 — a bare MEMBER name (<c>RuleFor</c>), so a symbol an agent read off
/// <c>resolve</c>/<c>find</c> can be handed straight back as a focus.
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

        // Bare route ("/products", no HTTP verb prefix): match HttpEndpoint entries by Route so an
        // agent that just learned the route from `entrypoints` doesn't need a round-trip through
        // resolve()/find() first. A single match resolves directly; an ambiguous route (several verbs
        // on the same path) prefers GET — the natural "explore this endpoint" default — over failing.
        if (f.StartsWith('/'))
        {
            var routeMatches = entries.Where(e =>
                e.Kind == EntryPointKind.HttpEndpoint && e.Route is { } r &&
                string.Equals(GraphBuilder.NormalizeRoute(r), GraphBuilder.NormalizeRoute(f), StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (routeMatches.Count == 1) return routeMatches[0];
            if (routeMatches.Count > 1)
                return routeMatches.FirstOrDefault(e =>
                    string.Equals(e.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    ?? routeMatches[0];
        }

        return ResolveFromNode(graph, f) ?? ResolveFromMember(graph, f);
    }

    /// <summary>Picks the most-connected of several same-named types: out-degree first (the "goes
    /// somewhere" rule this resolver has always had), then in-degree, then the ordinal-least key — so a
    /// tie is broken by evidence and, failing that, deterministically, never by enumeration order.
    /// Degrees are ROLLED (the type's own edges plus its members'), matching C3's rule for the other
    /// resolver: a type's collaborations hang off its members, so its own edge count is usually 0.</summary>
    private static GraphNode PickMostConnected(CodeGraph graph, List<GraphNode> candidates)
    {
        var degrees = RolledDegrees(graph, candidates);
        var best = candidates[0];
        for (var i = 1; i < candidates.Count; i++)
        {
            var (co, ci) = degrees[candidates[i].Id];
            var (bo, bi) = degrees[best.Id];
            var better = co != bo ? co > bo
                : ci != bi ? ci > bi
                : string.CompareOrdinal(candidates[i].Id.Key, best.Id.Key) < 0;
            if (better) best = candidates[i];
        }
        return best;
    }

    /// <summary>Rolled (out, in) degree per candidate, in ONE pass over the graph: each candidate's own
    /// edges, plus every member whose owning type is that candidate. Edges that stay inside the type are
    /// not collaborations and do not count — the same exclusion GraphQuery.RolledEdges makes.</summary>
    private static Dictionary<NodeId, (int Out, int In)> RolledDegrees(CodeGraph graph, List<GraphNode> candidates)
    {
        var degrees = new Dictionary<NodeId, (int Out, int In)>();
        var byKey = new Dictionary<string, NodeId>(StringComparer.Ordinal);
        foreach (var c in candidates)
        {
            degrees[c.Id] = (graph.OutEdges(c.Id).Length, graph.InEdges(c.Id).Length);
            if (c.Kind == NodeKind.Type) byKey[c.Id.Key] = c.Id;
        }
        if (byKey.Count == 0) return degrees;

        foreach (var node in graph.Nodes)
        {
            if (node.Kind != NodeKind.Member) continue;
            var ownerKey = Graph2.SymbolCanon.OwnerTypeOf(node.Id.Key);
            if (!byKey.TryGetValue(ownerKey, out var ownerId)) continue;

            var (o, i) = degrees[ownerId];
            foreach (var e in graph.OutEdges(node.Id))
                if (!string.Equals(OwnerTypeKey(e.To), ownerKey, StringComparison.Ordinal)) o++;
            foreach (var e in graph.InEdges(node.Id))
                if (!string.Equals(OwnerTypeKey(e.From), ownerKey, StringComparison.Ordinal)) i++;
            degrees[ownerId] = (o, i);
        }
        return degrees;
    }

    /// <summary>The type a node key belongs to — the member's owner, or the key itself.</summary>
    private static string OwnerTypeKey(NodeId id)
        => id.Kind == NodeKind.Member ? Graph2.SymbolCanon.OwnerTypeOf(id.Key) : id.Key;

    /// <summary>R4 item 2 (G1.2) — the member tier: a bare symbol name with no type qualifier.
    /// MEASURED on FluentValidation before this existed: the graph holds
    /// <c>Member:FluentValidation.AbstractValidator`1::RuleFor</c>, <c>resolve("RuleFor")</c> lists it,
    /// and <c>get_context(focus:"RuleFor")</c> still answered "No context could be built" — because
    /// this resolver stopped at Type/EntryPoint nodes. Runs LAST so a type never loses to a
    /// same-named member. Picks the most-connected match (the tier above's rule), tie-broken on the
    /// ordinal-least node key so the choice is deterministic; the pack names which symbol it landed on.</summary>
    private static EntryPoint? ResolveFromMember(CodeGraph graph, string focus)
    {
        // A qualified focus ("Type:Method") already had its chance above; only the bare form lands here.
        if (focus.Contains(':')) return null;

        GraphNode? best = null;
        foreach (var node in graph.Nodes)
        {
            if (node.Kind != NodeKind.Member) continue;
            if (!string.Equals(node.Title, focus, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(Graph2.SymbolCanon.MemberNameOf(node.Id.Key), focus, StringComparison.OrdinalIgnoreCase))
                continue;

            // A member has no members of its own, so its own edges ARE its degree — no rolling needed.
            if (best is null || IsBetterMember(graph, node, best))
                best = node;
        }

        return best is null
            ? null
            : new EntryPoint(EntryPointKind.PublicApi, best.Title, best.Id) { Provenance = best.FilePath };
    }

    private static bool IsBetterMember(CodeGraph graph, GraphNode candidate, GraphNode incumbent)
    {
        var candidateOut = graph.OutEdges(candidate.Id).Length;
        var incumbentOut = graph.OutEdges(incumbent.Id).Length;
        if (candidateOut != incumbentOut) return candidateOut > incumbentOut;

        var candidateIn = graph.InEdges(candidate.Id).Length;
        var incumbentIn = graph.InEdges(incumbent.Id).Length;
        if (candidateIn != incumbentIn) return candidateIn > incumbentIn;

        return string.CompareOrdinal(candidate.Id.Key, incumbent.Id.Key) < 0;
    }

    /// <summary>Finds a Type/EntryPoint node by short name or FQN suffix. For a "Type:Method" focus it
    /// anchors on the <b>Member</b> node when one exists (member-origin: this is what makes two sibling
    /// methods produce different traces); it falls back to the Type node otherwise (a method with no
    /// wiring). For a bare type it prefers the node with the most out-edges so a focus that matches both a
    /// bare class and a richer twin lands on the one that actually goes somewhere.
    /// <para>G1.2 — that preference is now measured on ROLLED degree (the type's own edges plus its
    /// members'), which is what <see cref="GraphQuery.ResolveNodeId"/> has ranked on since C3. Since the
    /// Type+tags collapse a type's collaborations hang off its MEMBERS, so comparing the types' own edges
    /// compared 0 against 0 and the winner was whichever the graph enumerated first. MEASURED on
    /// FluentValidation: <c>get_context(focus:"IValidator")</c> rooted on the non-generic
    /// <c>IValidator</c> (3) while <c>resolve("IValidator")</c> answered <c>IValidator`1</c> (9) — two
    /// tools, one name, different symbols, and get_context picked the emptier one.</para></summary>
    private static EntryPoint? ResolveFromNode(CodeGraph graph, string focus)
    {
        var name = focus;
        string? method = null;
        var sep = name.IndexOf("::", StringComparison.Ordinal);   // full member-key form
        var colon = sep >= 0 ? sep : name.IndexOf(':');
        if (colon > 0)
        {
            method = name[(colon + (sep >= 0 ? 2 : 1))..].Trim();
            name = name[..colon];
        }

        List<GraphNode>? candidates = null;
        foreach (var node in graph.Nodes)
        {
            if (node.Kind is not (NodeKind.Type or NodeKind.EntryPoint)) continue;
            var keyMatches = node.Kind == NodeKind.Type
                ? Graph2.SymbolCanon.TypeIdMatches(node.Id.Key, name, StringComparison.OrdinalIgnoreCase)
                : node.Id.Key.EndsWith("." + name, StringComparison.OrdinalIgnoreCase);
            if (!string.Equals(node.Title, name, StringComparison.OrdinalIgnoreCase) && !keyMatches) continue;

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

            (candidates ??= []).Add(node);
        }

        if (candidates is null) return null;
        // One match is the common case and costs nothing; rolling degrees up is only worth its one
        // pass over the graph when a name is genuinely contested.
        var best = candidates.Count == 1 ? candidates[0] : PickMostConnected(graph, candidates);
        return new EntryPoint(EntryPointKind.PublicApi, best.Title, best.Id) { Provenance = best.FilePath };
    }
}
