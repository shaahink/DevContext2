using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class MapBuilderTests
{
    private static CodeGraph EmptyGraph => new(
        new Dictionary<NodeId, GraphNode>(),
        new Dictionary<NodeId, ImmutableArray<GraphEdge>>());
    [Fact]
    public void Build_packages_dedup_and_group()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#",
                    ["net10.0"], [],
                    [new PackageReferenceInfo("MediatR", "12.0.0"), new PackageReferenceInfo("Serilog", "4.0.0")]),
                new ProjectInfo("Orders.Infrastructure", @"C:\repo\src\Orders.Infrastructure\Orders.Infrastructure.csproj", "C#",
                    ["net10.0"], [],
                    [new PackageReferenceInfo("MediatR", "11.0.0"), new PackageReferenceInfo("Dapper", "2.1.0")]),
            ],
        };

        var map = MapBuilder.Build(model, EmptyGraph, []);

        Assert.NotEmpty(map.Packages);
        // MediatR should appear once with the highest version (12.0.0)
        var mediatrGroup = map.Packages.First(g => string.Equals(g.Label, "Mediator/CQRS", StringComparison.Ordinal));
        Assert.Single(mediatrGroup.Packages);
        Assert.Contains("MediatR 12.0.0", mediatrGroup.Packages[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Build_topology_is_scoped()
    {
        var model = new DiscoveryModel
        {
            Solution = new SolutionInfo(@"C:\repo\App.sln", "App",
                [@"C:\repo\src\A\A.csproj", @"C:\repo\src\B\B.csproj"]),
            Projects =
            [
                new ProjectInfo("A", @"C:\repo\src\A\A.csproj", "C#", ["net10.0"], ["B"], []),
                new ProjectInfo("B", @"C:\repo\src\B\B.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("C", @"C:\repo\src\C\C.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        var map = MapBuilder.Build(model, EmptyGraph, []);

        Assert.Equal(2, map.Topology.Length);
        var a = map.Topology.First(n => string.Equals(n.Name, "A", StringComparison.Ordinal));
        Assert.Single(a.DependsOn);
        Assert.Equal("B", a.DependsOn[0]);
    }

    [Fact]
    public void Build_aggregates_from_ef_detection()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EfEntityDetection("Order", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = "Order.cs", LineNumber = 1,
        });
        model.Detections.Add(new EfEntityDetection("OrderItem", "OrderingContext", false, ["Id"])
        {
            ExtractorName = "test", SourceFile = "OrderItem.cs", LineNumber = 1,
        });

        var map = MapBuilder.Build(model, EmptyGraph, []);

        Assert.Single(map.Aggregates);
        Assert.Equal("Order", map.Aggregates[0]);
    }

    [Fact]
    public void Build_passes_entries_and_style_through()
    {
        var model = new DiscoveryModel();
        model.DetectedStyle = ArchitectureStyle.MinimalApi;
        model.StyleConfidence = 0.9f;
        model.StyleDetectedVia = "Minimal APIs + 1 project(s); no MediatR";

        var entries = ImmutableArray.Create(new EntryPoint(EntryPointKind.HttpEndpoint, "GET /todos",
            NodeId.ForEntry("GET /todos")));

        var map = MapBuilder.Build(model, EmptyGraph, entries);

        Assert.Equal("MinimalApi", map.Style);
        Assert.Equal(0.9f, map.StyleConfidence);
        Assert.Equal("Minimal APIs + 1 project(s); no MediatR", map.StyleEvidence);
        Assert.Single(map.Entries);
        Assert.Equal("GET /todos", map.Entries[0].Title);
    }
}
