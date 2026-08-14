using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>
/// MCP blind-drive audit (2026-07-11) bug #1: trace()/get_context() failed on bare route strings
/// ("/products") because EntryPointResolver only matched the full "METHOD /route" title. An agent
/// that just read a route off `entrypoints` would naturally try the bare route next.
/// </summary>
public sealed class EntryPointResolverTests
{
    private static EntryPoint HttpEntry(string method, string route) =>
        new(EntryPointKind.HttpEndpoint, $"{method} {route}", NodeId.ForEntry($"{method} {route}"))
        {
            HttpMethod = method,
            Route = route,
        };

    [Fact]
    public void Resolve_matches_bare_route_when_unambiguous()
    {
        var entries = new[] { HttpEntry("GET", "/products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/products");

        Assert.NotNull(resolved);
        Assert.Equal("GET /products", resolved!.Title);
    }

    [Fact]
    public void Resolve_bare_route_ignores_leading_and_trailing_slash_differences()
    {
        var entries = new[] { HttpEntry("GET", "/api/Products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/api/Products/");

        Assert.NotNull(resolved);
        Assert.Equal("GET /api/Products", resolved!.Title);
    }

    [Fact]
    public void Resolve_prefers_GET_when_route_is_ambiguous_across_verbs()
    {
        var entries = new[]
        {
            HttpEntry("DELETE", "/api/Products"),
            HttpEntry("POST", "/api/Products"),
            HttpEntry("GET", "/api/Products"),
        };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/api/Products");

        Assert.NotNull(resolved);
        Assert.Equal("GET /api/Products", resolved!.Title);
    }

    [Fact]
    public void Resolve_bare_route_falls_back_to_first_when_no_GET_present()
    {
        var entries = new[]
        {
            HttpEntry("DELETE", "/api/Products"),
            HttpEntry("POST", "/api/Products"),
        };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/api/Products");

        Assert.NotNull(resolved);
        Assert.Equal("DELETE /api/Products", resolved!.Title);
    }

    [Fact]
    public void Resolve_still_matches_exact_title_first()
    {
        var entries = new[] { HttpEntry("GET", "/products"), HttpEntry("POST", "/products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "POST /products");

        Assert.NotNull(resolved);
        Assert.Equal("POST /products", resolved!.Title);
    }

    [Fact]
    public void Resolve_unknown_bare_route_returns_null()
    {
        var entries = new[] { HttpEntry("GET", "/products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/unknown");

        Assert.Null(resolved);
    }

    // ── G1.2 (R4 item 2) — the member tier ───────────────────────────────────
    // MEASURED on FluentValidation before this tier existed: `resolve("RuleFor")` listed
    // Member:FluentValidation.AbstractValidator`1::RuleFor while get_context(focus:"RuleFor")
    // answered "No context could be built" — two tools in one menu disagreeing about the same name.

    [Fact]
    public void Resolve_matches_a_bare_member_name()
    {
        var g = new CodeGraphBuilder();
        var memberId = NodeId.ForMember("App.OrderService", "CreateOrder");
        g.AddNode(new GraphNode(memberId, "CreateOrder", NodeKind.Member));
        var graph = g.Build();

        var resolved = EntryPointResolver.Resolve([], graph, "CreateOrder");

        Assert.NotNull(resolved);
        Assert.Equal(memberId, resolved!.Node);
        Assert.Equal(EntryPointKind.PublicApi, resolved.Kind);
    }

    [Fact]
    public void Resolve_prefers_a_type_over_a_same_named_member()
    {
        // The member tier runs LAST, so adding it can never take a focus away from the type it
        // used to resolve to — that is what keeps every existing --focus and every golden unmoved.
        var g = new CodeGraphBuilder();
        var typeId = NodeId.ForType("App.Validate");
        var memberId = NodeId.ForMember("App.OrderService", "Validate");
        g.AddNode(new GraphNode(typeId, "Validate", NodeKind.Type));
        g.AddNode(new GraphNode(memberId, "Validate", NodeKind.Member));
        var graph = g.Build();

        var resolved = EntryPointResolver.Resolve([], graph, "Validate");

        Assert.NotNull(resolved);
        Assert.Equal(typeId, resolved!.Node);
    }

    [Fact]
    public void Resolve_picks_the_most_connected_member_when_a_name_repeats()
    {
        var g = new CodeGraphBuilder();
        var lonely = NodeId.ForMember("App.Unused", "Validate");
        var busy = NodeId.ForMember("App.OrderService", "Validate");
        var calleeId = NodeId.ForType("App.OrderRepository");
        g.AddNode(new GraphNode(lonely, "Validate", NodeKind.Member));
        g.AddNode(new GraphNode(busy, "Validate", NodeKind.Member));
        g.AddNode(new GraphNode(calleeId, "OrderRepository", NodeKind.Type));
        g.AddEdge(new GraphEdge(busy, calleeId, EdgeKind.Calls));
        var graph = g.Build();

        var resolved = EntryPointResolver.Resolve([], graph, "Validate");

        Assert.NotNull(resolved);
        Assert.Equal(busy, resolved!.Node);
    }

    [Fact]
    public void Resolve_member_tier_is_deterministic_when_degrees_tie()
    {
        // Determinism seal: equal-degree matches resolve on the ordinal-least key, not on whichever
        // node the graph happens to enumerate first.
        var a = NodeId.ForMember("App.Alpha", "Validate");
        var z = NodeId.ForMember("App.Zulu", "Validate");

        var forward = new CodeGraphBuilder();
        forward.AddNode(new GraphNode(a, "Validate", NodeKind.Member));
        forward.AddNode(new GraphNode(z, "Validate", NodeKind.Member));

        var reverse = new CodeGraphBuilder();
        reverse.AddNode(new GraphNode(z, "Validate", NodeKind.Member));
        reverse.AddNode(new GraphNode(a, "Validate", NodeKind.Member));

        Assert.Equal(a, EntryPointResolver.Resolve([], forward.Build(), "Validate")!.Node);
        Assert.Equal(a, EntryPointResolver.Resolve([], reverse.Build(), "Validate")!.Node);
    }

    [Fact]
    public void Resolve_breaks_a_zero_out_degree_type_tie_on_in_degree()
    {
        // MEASURED on FluentValidation: an INTERFACE has no out-edges by construction, so on a
        // library — where the front doors are interfaces — every same-named candidate tied at 0 and
        // the winner was whichever the graph enumerated first. get_context("IValidator") rooted on
        // the non-generic IValidator (3 in-edges) while resolve("IValidator") answered IValidator`1
        // (9). Out-degree still decides first; this only defines what happens when it ties.
        var g = new CodeGraphBuilder();
        var bare = NodeId.ForType("App.IValidator");
        var used = NodeId.ForType("App.IValidator`1");
        var callerId = NodeId.ForMember("App.Runner", "Run");
        g.AddNode(new GraphNode(bare, "IValidator", NodeKind.Type));
        g.AddNode(new GraphNode(used, "IValidator", NodeKind.Type));
        g.AddNode(new GraphNode(callerId, "Runner.Run", NodeKind.Member));
        g.AddEdge(new GraphEdge(callerId, used, EdgeKind.Calls));
        var graph = g.Build();

        var resolved = EntryPointResolver.Resolve([], graph, "IValidator");

        Assert.NotNull(resolved);
        Assert.Equal(used, resolved!.Node);
    }

    [Fact]
    public void Resolve_ranks_same_named_types_on_rolled_degree_not_their_own_edges()
    {
        // The FluentValidation case in miniature. Since the Type+tags collapse a type's collaborations
        // hang off its MEMBERS, so comparing the types' own edge counts compares 0 against 0 — the
        // non-generic IValidator (3 users, all direct) beat IValidator`1 (9 users, all through members)
        // because it happened to be enumerated first. GraphQuery.ResolveNodeId has rolled since C3.
        var g = new CodeGraphBuilder();
        var bare = NodeId.ForType("App.IValidator");
        var generic = NodeId.ForType("App.IValidator`1");
        var genericMember = NodeId.ForMember("App.IValidator`1", "Validate");
        var callerA = NodeId.ForMember("App.CallerA", "Run");
        var callerB = NodeId.ForMember("App.CallerB", "Run");
        foreach (var (id, title, kind) in new (NodeId, string, NodeKind)[]
        {
            (bare, "IValidator", NodeKind.Type), (generic, "IValidator", NodeKind.Type),
            (genericMember, "Validate", NodeKind.Member),
            (callerA, "CallerA.Run", NodeKind.Member), (callerB, "CallerB.Run", NodeKind.Member),
        })
            g.AddNode(new GraphNode(id, title, kind));

        // The bare type is used once, directly. The generic type is used twice — through its member.
        g.AddEdge(new GraphEdge(callerA, bare, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerA, genericMember, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerB, genericMember, EdgeKind.Calls));
        var graph = g.Build();

        var resolved = EntryPointResolver.Resolve([], graph, "IValidator");

        Assert.NotNull(resolved);
        Assert.Equal(generic, resolved!.Node);
    }

    [Fact]
    public void Resolve_still_prefers_out_degree_over_in_degree()
    {
        // The rule this resolver has always had ("lands on the one that actually goes somewhere")
        // is untouched — in-degree only speaks when out-degree ties.
        var g = new CodeGraphBuilder();
        var goesSomewhere = NodeId.ForType("App.Alpha.Service");
        var muchUsed = NodeId.ForType("App.Zulu.Service");
        var other = NodeId.ForType("App.Other");
        g.AddNode(new GraphNode(goesSomewhere, "Service", NodeKind.Type));
        g.AddNode(new GraphNode(muchUsed, "Service", NodeKind.Type));
        g.AddNode(new GraphNode(other, "Other", NodeKind.Type));
        g.AddEdge(new GraphEdge(goesSomewhere, other, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(other, muchUsed, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(NodeId.ForMember("App.Runner", "Run"), muchUsed, EdgeKind.Calls));
        var graph = g.Build();

        var resolved = EntryPointResolver.Resolve([], graph, "Service");

        Assert.NotNull(resolved);
        Assert.Equal(goesSomewhere, resolved!.Node);
    }

    [Fact]
    public void Resolve_unknown_bare_symbol_still_returns_null()
    {
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForMember("App.OrderService", "CreateOrder"), "CreateOrder", NodeKind.Member));

        var resolved = EntryPointResolver.Resolve([], g.Build(), "NoSuchSymbol");

        Assert.Null(resolved);
    }

    // ── T1.3 (BUG-BACKLOG #6) — the nodeId tier ──────────────────────────────
    // Every other tool in the menu takes a nodeId, and trace's own did-you-mean envelope hands
    // nodeIds back, so an agent reaches this resolver with a "Kind:Key" string on its first try.
    // Before the tier, that string was split at the FIRST colon and the KIND PREFIX was read as a
    // type name. Two measured consequences, both silent:
    //   TodoApi 2026-08-13 — trace("EntryPoint:GET /") and trace("Type:Todo.Web.Server.ExternalProviders")
    //     answered found:false while the same nodes' bare titles traced 5 and 2 steps.
    //   Hangfire 2026-07-29 — on a graph carrying a node whose Title is literally "Type" (bug #7's
    //     mis-bound node), it matched THAT node: found:true, 0 steps, rendered "Entry: Type: Type".

    [Fact]
    public void Resolve_accepts_a_Type_nodeId()
    {
        var g = new CodeGraphBuilder();
        var id = NodeId.ForType("Todo.Web.Server.ExternalProviders");
        g.AddNode(new GraphNode(id, "ExternalProviders", NodeKind.Type));

        var resolved = EntryPointResolver.Resolve([], g.Build(), "Type:Todo.Web.Server.ExternalProviders");

        Assert.NotNull(resolved);
        Assert.Equal(id, resolved!.Node);
    }

    [Fact]
    public void Resolve_accepts_a_Member_nodeId()
    {
        // MEASURED on Hangfire: trace("Member:Hangfire.BackgroundJobClient::Create") answered
        // "No entry or node matched" and offered back candidates that routed into the phantom.
        var g = new CodeGraphBuilder();
        var id = NodeId.ForMember("Hangfire.BackgroundJobClient", "Create");
        g.AddNode(new GraphNode(NodeId.ForType("Hangfire.BackgroundJobClient"), "BackgroundJobClient", NodeKind.Type));
        g.AddNode(new GraphNode(id, "Create", NodeKind.Member));

        var resolved = EntryPointResolver.Resolve([], g.Build(), "Member:Hangfire.BackgroundJobClient::Create");

        Assert.NotNull(resolved);
        Assert.Equal(id, resolved!.Node);
    }

    [Fact]
    public void Resolve_of_a_nodeId_never_lands_on_a_node_titled_like_its_kind()
    {
        // The phantom, reproduced: a graph that carries a node titled "Type" (BUG-BACKLOG #7) plus
        // the node the caller actually asked for. The wrong answer here is not an exception — it is
        // a confident EntryPoint on the "Type" node, which traces to found:true with zero steps.
        var g = new CodeGraphBuilder();
        var phantom = NodeId.ForType("System.Type");
        var wanted = NodeId.ForType("Hangfire.BackgroundJobClient");
        g.AddNode(new GraphNode(phantom, "Type", NodeKind.Type));
        g.AddNode(new GraphNode(wanted, "BackgroundJobClient", NodeKind.Type));
        g.AddEdge(new GraphEdge(wanted, NodeId.ForType("Hangfire.JobStorage"), EdgeKind.Calls));

        var resolved = EntryPointResolver.Resolve([], g.Build(), "Type:Hangfire.BackgroundJobClient");

        Assert.NotNull(resolved);
        Assert.Equal(wanted, resolved!.Node);
        Assert.NotEqual("Type", resolved.Title);
    }

    [Fact]
    public void Resolve_of_a_nodeId_returns_the_inventory_entry_when_the_node_is_one()
    {
        // A round-tripped nodeId must not downgrade an HTTP endpoint into a synthetic PublicApi:
        // the entry carries the route, verb and provenance every downstream render reads.
        var entry = HttpEntry("GET", "/todos/");
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(entry.Node, entry.Title, NodeKind.EntryPoint));

        var resolved = EntryPointResolver.Resolve([entry], g.Build(), $"EntryPoint:{entry.Node.Key}");

        Assert.Same(entry, resolved);
    }

    [Fact]
    public void Resolve_of_a_nodeId_whose_key_is_unknown_returns_null()
    {
        // A miss is the honest answer — found:false plus candidates, not a confident empty tree.
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForType("App.Real"), "Real", NodeKind.Type));

        var resolved = EntryPointResolver.Resolve([], g.Build(), "Type:App.NoSuchThing");

        Assert.Null(resolved);
    }

    [Fact]
    public void Resolve_still_reads_an_ordinary_Type_colon_Method_focus_as_a_member()
    {
        // The tier must not swallow the documented "Type:Method" form — only kind PREFIXES.
        var g = new CodeGraphBuilder();
        var typeId = NodeId.ForType("App.OrderService");
        var memberId = NodeId.ForMember("App.OrderService", "Process");
        g.AddNode(new GraphNode(typeId, "OrderService", NodeKind.Type));
        g.AddNode(new GraphNode(memberId, "Process", NodeKind.Member));

        var resolved = EntryPointResolver.Resolve([], g.Build(), "OrderService:Process");

        Assert.NotNull(resolved);
        Assert.Equal(memberId, resolved!.Node);
    }
}
