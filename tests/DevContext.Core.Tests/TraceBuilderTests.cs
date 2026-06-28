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
}
