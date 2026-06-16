using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class GraphBuilderTests
{
    [Fact]
    public void Build_joins_endpoint_and_handler_and_filters_test_code()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Orders.Tests", @"C:\repo\src\Orders.Tests\Orders.Tests.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        model.Types.TryAdd("Orders.Api.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommandHandler", Name = "CreateOrderCommandHandler",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("Orders.Tests.OrderHandlerTests", new TypeDiscovery
        {
            Id = "Orders.Tests.OrderHandlerTests", Name = "OrderHandlerTests",
            Namespace = "Orders.Tests", FilePath = @"C:\repo\src\Orders.Tests\OrderHandlerTests.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Testing,
        });

        model.Detections.Add(new EndpointDetection("POST", "/api/orders", "OrdersApi", "CreateOrderAsync", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\OrdersApi.cs", LineNumber = 10,
        });
        model.Detections.Add(new MediatRHandlerDetection("CreateOrderCommand", "bool", "CreateOrderCommandHandler", MediatRKind.Command)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\CreateOrderCommandHandler.cs", LineNumber = 20,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        // Production type is a node; test-project type is filtered out (structurally, not by name).
        Assert.True(graph.Contains(NodeId.ForType("Orders.Api.CreateOrderCommandHandler")));
        Assert.False(graph.Contains(NodeId.ForType("Orders.Tests.OrderHandlerTests")));

        // Endpoint became an entry point.
        Assert.Single(entries);
        Assert.Equal("POST /api/orders", entries[0].Title);

        // MediatR detection became Request --Handles--> Handler, with the handler key FQN-resolved.
        var requestId = NodeId.ForRequest("CreateOrderCommand");
        var handlerId = NodeId.ForHandler("Orders.Api.CreateOrderCommandHandler");
        Assert.True(graph.Contains(requestId));
        Assert.True(graph.Contains(handlerId));
        Assert.Contains(graph.OutEdges(requestId), e => e.Kind == EdgeKind.Handles && e.To == handlerId);
    }

    [Fact]
    public void NoiseFilter_keeps_production_Spec_types()
    {
        // Regression: the old name-suffix heuristic dropped DDD *Spec types as "tests".
        var projects = ImmutableArray.Create(
            new ProjectInfo("Orders.Core", @"C:\repo\src\Orders.Core\Orders.Core.csproj", "C#", ["net10.0"], [], []));
        var noise = new NoiseFilter(new ProjectClassifier(projects));

        var spec = new TypeDiscovery
        {
            Id = "Orders.Core.Specifications.OrderByIdSpec", Name = "OrderByIdSpec",
            Namespace = "Orders.Core.Specifications",
            FilePath = @"C:\repo\src\Orders.Core\Specifications\OrderByIdSpec.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        };

        Assert.True(noise.IsProductionCode(spec));
    }

    [Fact]
    public void SolutionScope_scopes_to_resolved_solution_projects()
    {
        var model = new DiscoveryModel
        {
            Solution = new SolutionInfo(@"C:\repo\AppA.sln", "AppA", [@"C:\repo\src\AppA\AppA.csproj"]),
            Projects =
            [
                new ProjectInfo("AppA", @"C:\repo\src\AppA\AppA.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("AppB", @"C:\repo\src\AppB\AppB.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        var scope = SolutionScope.FromModel(model);

        Assert.Equal("AppA", scope.SolutionName);
        Assert.True(scope.Contains(@"C:\repo\src\AppA\Service.cs"));   // in the resolved solution
        Assert.False(scope.Contains(@"C:\repo\src\AppB\Service.cs"));  // independent solution → excluded
    }
}
