using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class TraceBuilderTests
{
    [Fact]
    public void Build_dedups_edges_mirrored_on_type_and_handler_twin()
    {
        // A Handler node and its Type twin can carry the SAME Raises edge (GraphBuilder mirrors it
        // onto both). The trace must show the event once, not twice.
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /x");
        var typeId = NodeId.ForType("App.Handler");
        var handlerId = NodeId.ForHandler("App.Handler");
        var eventId = NodeId.ForEvent("App.ThingHappened");

        g.AddNode(new GraphNode(entryId, "POST /x", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(typeId, "Handler", NodeKind.Type));
        g.AddNode(new GraphNode(handlerId, "Handler", NodeKind.Handler));
        g.AddNode(new GraphNode(eventId, "ThingHappened", NodeKind.Event));

        g.AddEdge(new GraphEdge(entryId, handlerId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(typeId, eventId, EdgeKind.Raises));     // mirrored onto the type twin
        g.AddEdge(new GraphEdge(handlerId, eventId, EdgeKind.Raises));  // and onto the handler

        var trace = new TraceBuilder(g.Build())
            .Build(new EntryPoint(EntryPointKind.HttpEndpoint, "POST /x", entryId), new TraceOptions { MaxDepth = 6 });

        Assert.Single(trace.EmittedEvents);                       // EMITS shows the event once
        var handlerStep = Assert.Single(trace.Root.Children);     // entry → handler
        Assert.Equal(1, handlerStep.Children.Count(c => c.Node.Id == eventId)); // raises shown once
    }
}
