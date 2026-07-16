using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class TraceBuilderTests
{
    [Fact]
    public void One_node_per_class_carries_incoming_handles_and_outgoing_raises()
    {
        // Regression guard for the Type+tags collapse: one node per class. The handler that handles a
        // request also raises an event — the incoming Handles edge and the outgoing Raises edge land on
        // the SAME node (no Type/Handler twin), and the trace shows each step once.
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /x");
        var requestId = NodeId.ForType("App.DoThing");
        var handlerId = NodeId.ForType("App.DoThingHandler");
        var eventId = NodeId.ForType("App.ThingHappened");

        g.AddNode(new GraphNode(entryId, "POST /x", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(requestId, "DoThing", NodeKind.Type) { Tags = [RoleTags.Command] });
        g.AddNode(new GraphNode(handlerId, "DoThingHandler", NodeKind.Type) { Tags = [RoleTags.Handler] });
        g.AddNode(new GraphNode(eventId, "ThingHappened", NodeKind.Type) { Tags = [RoleTags.DomainEvent] });

        g.AddEdge(new GraphEdge(entryId, requestId, EdgeKind.Sends));
        g.AddEdge(new GraphEdge(requestId, handlerId, EdgeKind.Handles));
        g.AddEdge(new GraphEdge(handlerId, eventId, EdgeKind.Raises));

        var trace = new TraceBuilder(g.Build())
            .Build(new EntryPoint(EntryPointKind.HttpEndpoint, "POST /x", entryId), new TraceOptions { MaxDepth = 6 });

        Assert.Single(trace.EmittedEvents);                          // EMITS shows the event once
        var sendStep = Assert.Single(trace.Root.Children);           // entry → send request
        Assert.Equal(requestId, sendStep.Node.Id);
        var handleStep = Assert.Single(sendStep.Children);           // request → handle handler
        Assert.Equal(handlerId, handleStep.Node.Id);
        Assert.Equal(1, handleStep.Children.Count(c => c.Node.Id == eventId)); // raises shown once
    }

    [Fact]
    public void AddNode_merges_role_tags_and_keeps_first_declaration()
    {
        // AddTypeNodes seeds the declaration (FilePath/SourceBody); a later join adds a role tag.
        // The merge must keep the declaration and union the tags onto the one node.
        var g = new CodeGraphBuilder();
        var id = NodeId.ForType("App.Order");
        g.AddNode(new GraphNode(id, "Order", NodeKind.Type) { FilePath = "Order.cs", SourceBody = "class Order {}" });
        g.AddNode(new GraphNode(id, "Order", NodeKind.Type) { Tags = [RoleTags.Entity, RoleTags.Aggregate] });

        var node = g.Build().Node(id)!;
        Assert.Equal("Order.cs", node.FilePath);
        Assert.Equal("class Order {}", node.SourceBody);
        Assert.Contains(RoleTags.Entity, node.Tags);
        Assert.Contains(RoleTags.Aggregate, node.Tags);
    }

    [Fact]
    public void Salient_ShowsCalleesOwnMethod_NotCallerLineOrSiblingMethod()
    {
        // Regression guard for E3: the old implementation took lines from the CALLER's body (or, on
        // the common Type-body fallback, the whole parent type's body) around the edge's provenance
        // line — indexing an absolute file line number into a substring that does NOT start at line 1
        // of the file, silently landing on unrelated code. The fix looks up the callee's own
        // declaration by member name, which needs no file-line↔offset mapping at all. Here the
        // provenance line deliberately points at a line that falls inside the sibling DeleteOrder
        // method within OrderService's SourceBody — the exact shape that used to leak wrong code.
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var callerId = NodeId.ForMember("App.OrdersController", "Post");
        var calleeId = NodeId.ForMember("App.OrderService", "CreateOrder");
        var serviceTypeId = NodeId.ForType("App.OrderService");

        var serviceSourceBody = @"public class OrderService
{
    private readonly IRepo _repo;

    public void CreateOrder(Order o)
    {
        _repo.Save(o);
    }

    public void DeleteOrder(int id)
    {
        _repo.Delete(id);
    }
}";

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(callerId, "OrdersController.Post", NodeKind.Member)
        {
            SourceBody = "public IActionResult Post() => _service.CreateOrder(new Order());",
        });
        g.AddNode(new GraphNode(serviceTypeId, "OrderService", NodeKind.Type) { SourceBody = serviceSourceBody });
        g.AddNode(new GraphNode(calleeId, "OrderService.CreateOrder", NodeKind.Member));

        g.AddEdge(new GraphEdge(entryId, callerId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls) { Provenance = "OrdersController.cs:11" });

        var trace = new TraceBuilder(g.Build())
            .Build(new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId), new TraceOptions { MaxDepth = 6 });

        var callStep = trace.Root.Children.Single();
        var calleeStep = callStep.Children.Single(c => c.Node.Id == calleeId);

        Assert.Contains(calleeStep.Salient, l => l.Contains("CreateOrder"));
        Assert.DoesNotContain(calleeStep.Salient, l => l.Contains("DeleteOrder"));
        Assert.DoesNotContain(calleeStep.Salient, l => l.Contains("_service.CreateOrder"));
    }

    [Fact]
    public void Salient_ForTypeCallee_ShowsOwnDeclarationOpeningLines()
    {
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var serviceTypeId = NodeId.ForType("App.OrderService");

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(serviceTypeId, "OrderService", NodeKind.Type)
        {
            SourceBody = "public class OrderService\n{\n    public OrderService(IRepo repo) { }\n}",
        });
        g.AddEdge(new GraphEdge(entryId, serviceTypeId, EdgeKind.Resolves));

        var trace = new TraceBuilder(g.Build())
            .Build(new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId), new TraceOptions { MaxDepth = 6 });

        var step = trace.Root.Children.Single();
        Assert.Contains(step.Salient, l => l.Contains("class OrderService"));
    }

    [Fact]
    public void Salient_MemberNotDeclaredInTypeBody_ReturnsEmpty_NoCrash()
    {
        // A framework/inherited method the type doesn't declare itself (e.g. object.ToString()) has no
        // local text to show — the snippet must be honestly empty, never an unrelated sibling method.
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var callerId = NodeId.ForMember("App.OrdersController", "Post");
        var calleeId = NodeId.ForMember("App.OrderService", "ToString");
        var serviceTypeId = NodeId.ForType("App.OrderService");

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(callerId, "OrdersController.Post", NodeKind.Member));
        g.AddNode(new GraphNode(serviceTypeId, "OrderService", NodeKind.Type)
        {
            SourceBody = "public class OrderService\n{\n    public void CreateOrder() { }\n}",
        });
        g.AddNode(new GraphNode(calleeId, "OrderService.ToString", NodeKind.Member));

        g.AddEdge(new GraphEdge(entryId, callerId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls));

        var trace = new TraceBuilder(g.Build())
            .Build(new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId), new TraceOptions { MaxDepth = 6 });

        var calleeStep = trace.Root.Children.Single().Children.Single(c => c.Node.Id == calleeId);
        Assert.Empty(calleeStep.Salient);
    }

    // T3.3 — token budget shaping.
    private static TraceStep Step(string title, params TraceStep[] children)
    {
        var node = new GraphNode(NodeId.ForType("Ns." + title), title, NodeKind.Type);
        return new TraceStep(node, SeamKind.Call, 0) { Children = [.. children] };
    }

    private static int CountNodes(TraceStep s)
    {
        var n = 1;
        foreach (var c in s.Children) n += CountNodes(c);
        return n;
    }

    private static int TotalOmitted(TraceStep s)
    {
        var sum = s.Omitted;
        foreach (var c in s.Children) sum += TotalOmitted(c);
        return sum;
    }

    [Fact]
    public void ShapeToBudget_cuts_to_budget_names_omissions_and_keeps_the_root()
    {
        // 10 children, each with one grandchild → 21 nodes.
        var root = Step("Entry", Enumerable.Range(0, 10)
            .Select(i => Step($"Child{i}", Step($"Grand{i}"))).ToArray());
        var trace = new Trace(new EntryPoint(EntryPointKind.HttpEndpoint, "Entry", NodeId.ForEntry("Entry")), root);
        var full = CountNodes(trace.Root);
        Assert.Equal(21, full);

        var shaped = TraceBuilder.ShapeToBudget(trace, budgetTokens: 20);
        var kept = CountNodes(shaped.Root);

        Assert.Equal("Entry", shaped.Root.Node.Title);            // root always survives
        Assert.True(kept < full, $"expected < {full} nodes, got {kept}");
        Assert.True(TotalOmitted(shaped.Root) > 0);               // the cut is named, not silent
        Assert.Equal(full, kept + TotalOmitted(shaped.Root));     // every node is kept xor counted omitted
    }

    [Fact]
    public void ShapeToBudget_zero_is_unlimited_and_unchanged()
    {
        var root = Step("Entry", Step("A", Step("A1")), Step("B"));
        var trace = new Trace(new EntryPoint(EntryPointKind.HttpEndpoint, "Entry", NodeId.ForEntry("Entry")), root);

        var unlimited = TraceBuilder.ShapeToBudget(trace, 0);
        Assert.Equal(CountNodes(trace.Root), CountNodes(unlimited.Root));
        Assert.Equal(0, TotalOmitted(unlimited.Root));
    }
}
