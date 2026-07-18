using DevContext.Core.Graph;
using DevContext.Core.Models;

using Microsoft.CodeAnalysis;

using TypeKind = DevContext.Core.Models.TypeKind;

namespace DevContext.Core.Tests;

/// <summary>D5.3 determinism rider — parallel extractor arrival order must never reach an anchor
/// pick. Pins: (1) SealableBag sealed order is insertion-order-independent, (2) the model's
/// canonical detection/call-edge order is total for value-distinct items, (3) NameResolver's
/// short-name collision pick no longer depends on the (per-process-randomized) enumeration order
/// of ConcurrentDictionary.Values.</summary>
public class DeterministicOrderTests
{
    private static EndpointDetection Endpoint(string file, int line, string route) => new(
        "GET", route, "Handler", "Handle", [], [])
    {
        ExtractorName = "test",
        SourceFile = file,
        LineNumber = line,
    };

    [Fact]
    public void Sealed_order_is_independent_of_insertion_order()
    {
        var items = Enumerable.Range(0, 200)
            .Select(i => Endpoint($"src/File{i % 7}.cs", i % 13, $"/route/{i}"))
            .ToList();

        var forward = new DiscoveryModel();
        foreach (var d in items) forward.Detections.Add(d);
        forward.SealDeterministicOrder();

        var reversed = new DiscoveryModel();
        foreach (var d in Enumerable.Reverse(items)) reversed.Detections.Add(d);
        reversed.SealDeterministicOrder();

        Assert.Equal(forward.Detections.ToList(), reversed.Detections.ToList());
    }

    [Fact]
    public void Sealed_order_is_independent_of_parallel_interleaving()
    {
        var items = Enumerable.Range(0, 500)
            .Select(i => Endpoint($"src/File{i % 11}.cs", i % 17, $"/r/{i}"))
            .ToList();

        var sequential = new DiscoveryModel();
        foreach (var d in items) sequential.Detections.Add(d);
        sequential.SealDeterministicOrder();

        var parallel = new DiscoveryModel();
        Parallel.ForEach(items, d => parallel.Detections.Add(d));
        parallel.SealDeterministicOrder();

        Assert.Equal(sequential.Detections.ToList(), parallel.Detections.ToList());
    }

    [Fact]
    public void Call_edge_sealed_order_is_total_and_insertion_independent()
    {
        var edges = new List<CallEdge>
        {
            new("B", "M", "C", "N", "b.cs:2"),
            new("A", "M", "C", "N", "a.cs:1"),
            new("A", "M", "B", "N", "a.cs:9"),
            new("A", "M", "B", "N", "a.cs:9") { Resolution = Resolution.Semantic },
        };

        var forward = new DiscoveryModel();
        foreach (var e in edges) forward.CallEdges.Add(e);
        forward.SealDeterministicOrder();

        var reversed = new DiscoveryModel();
        foreach (var e in Enumerable.Reverse(edges)) reversed.CallEdges.Add(e);
        reversed.SealDeterministicOrder();

        Assert.Equal(forward.CallEdges.ToList(), reversed.CallEdges.ToList());
        Assert.Equal("A", forward.CallEdges.First().CallerType);
    }

    [Fact]
    public void Clear_then_readd_preserves_new_insertion_order()
    {
        // The SemanticLite call-edge upgrade replaces the bag contents with an order-preserving
        // list; the replace must not reorder (the pre-D5.3 ConcurrentBag enumerated LIFO).
        var bag = new SealableBag<int>();
        bag.Add(3); bag.Add(1);
        bag.Clear();
        bag.Add(5); bag.Add(4); bag.Add(6);
        Assert.Equal([5, 4, 6], bag.ToList());
    }

    private static TypeDiscovery Type(string ns, string name, string file) => new()
    {
        Id = $"{ns}.{name}",
        Name = name,
        Namespace = ns,
        FilePath = file,
        Kind = TypeKind.Class,
        Accessibility = Accessibility.Public,
        Layer = ArchitectureLayer.Unknown,
    };

    [Fact]
    public void Graph_nodes_and_edges_enumerate_in_insertion_order()
    {
        // The frozen graph must expose the BUILDER's insertion order, never FrozenDictionary's
        // hash-layout order (randomized per process — the rendered ServiceLink pair-order flap).
        var b = new CodeGraphBuilder();
        var zeta = NodeId.ForService("Zeta");
        var alpha = NodeId.ForService("Alpha");
        var mid = NodeId.ForService("Mid");
        b.AddNode(new GraphNode(zeta, "Zeta", NodeKind.Service));
        b.AddNode(new GraphNode(alpha, "Alpha", NodeKind.Service));
        b.AddNode(new GraphNode(mid, "Mid", NodeKind.Service));
        b.AddEdge(new GraphEdge(zeta, alpha, EdgeKind.ServiceLink) { Provenance = "z:1" });
        b.AddEdge(new GraphEdge(zeta, mid, EdgeKind.ServiceLink) { Provenance = "z:2" });
        b.AddEdge(new GraphEdge(alpha, mid, EdgeKind.ServiceLink) { Provenance = "a:1" });

        var g = b.Build();

        Assert.Equal(["Zeta", "Alpha", "Mid"], g.Nodes.Select(n => n.Title).ToArray());
        Assert.Equal(["z:1", "z:2", "a:1"], g.AllEdges.Select(e => e.Provenance ?? "").ToArray());
    }

    [Fact]
    public void Name_resolver_collision_pick_is_enumeration_order_independent()
    {
        var types = new[]
        {
            Type("Zebra.Fixtures", "CipherService", "z/CipherService.cs"),
            Type("App.Services", "CipherService", "a/CipherService.cs"),
            Type("App.Other", "Unrelated", "a/Unrelated.cs"),
        };

        var forward = new NameResolver(types);
        var reversed = new NameResolver(types.Reverse());

        Assert.Equal(forward.Resolve("CipherService"), reversed.Resolve("CipherService"));
    }
}
