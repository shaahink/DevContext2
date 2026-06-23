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
        var requestId = NodeId.ForType("CreateOrderCommand");
        var handlerId = NodeId.ForType("Orders.Api.CreateOrderCommandHandler");
        Assert.True(graph.Contains(requestId));
        Assert.True(graph.Contains(handlerId));
        Assert.Contains(graph.OutEdges(requestId), e => e.Kind == EdgeKind.Handles && e.To == handlerId);
    }

    [Fact]
    public void Background_workers_become_entry_points()
    {
        // DntSite audit: 24 DNTScheduler jobs were detected but never surfaced as entries.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Web", @"C:\repo\src\Web\Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Web.Jobs.BackupDatabaseJob", new TypeDiscovery
        {
            Id = "Web.Jobs.BackupDatabaseJob", Name = "BackupDatabaseJob", Namespace = "Web.Jobs",
            FilePath = @"C:\repo\src\Web\Jobs\BackupDatabaseJob.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Infrastructure,
        });
        model.Detections.Add(new BackgroundWorkerDetection("DNTScheduler", "BackupDatabaseJob", BackgroundWorkerKind.HostedService)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Web\SchedulersConfig.cs", LineNumber = 18,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var worker = entries.Single(e => e.Title == "BackupDatabaseJob");
        Assert.Equal(EntryPointKind.ScheduledJob, worker.Kind); // DNTScheduler → Scheduled
        Assert.Contains(graph.OutEdges(worker.Node), e =>
            e.Kind == EdgeKind.Calls && e.To == NodeId.ForType("Web.Jobs.BackupDatabaseJob"));
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

    [Fact]
    public void B1_event_consumers_adds_events_and_consumes_edges()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommandHandler", Name = "CreateOrderCommandHandler",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("Orders.Api.ValidateBuyerHandler", new TypeDiscovery
        {
            Id = "Orders.Api.ValidateBuyerHandler", Name = "ValidateBuyerHandler",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\ValidateBuyerHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        model.Detections.Add(new MediatRHandlerDetection("OrderStartedDomainEvent", "void", "ValidateBuyerHandler", MediatRKind.Notification)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\ValidateBuyerHandler.cs", LineNumber = 15,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var eventId = NodeId.ForType("OrderStartedDomainEvent");
        var handlerId = NodeId.ForType("Orders.Api.ValidateBuyerHandler");
        Assert.True(graph.Contains(eventId));
        Assert.True(graph.Contains(handlerId));
        Assert.Contains(graph.OutEdges(eventId), e => e.Kind == EdgeKind.Consumes && e.To == handlerId);
    }

    [Fact]
    public void B1_di_resolves_adds_resolve_edges()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.IOrderRepository", new TypeDiscovery
        {
            Id = "Orders.Api.IOrderRepository", Name = "IOrderRepository",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\IOrderRepository.cs",
            Kind = TypeKind.Interface, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Orders.Api.OrderRepository", new TypeDiscovery
        {
            Id = "Orders.Api.OrderRepository", Name = "OrderRepository",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\OrderRepository.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
            ImplementedInterfaces = ["IOrderRepository"],
        });

        model.Detections.Add(new DiRegistrationDetection("IOrderRepository", "OrderRepository", "Scoped", [], DiRegistrationShape.DirectBinding)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\Program.cs", LineNumber = 5,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var svcId = NodeId.ForType("Orders.Api.IOrderRepository");
        var implId = NodeId.ForType("Orders.Api.OrderRepository");
        Assert.Contains(graph.OutEdges(svcId), e => e.Kind == EdgeKind.Resolves && e.To == implId);
    }

    [Fact]
    public void B1_aggregate_nodes_tagged()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.Order", new TypeDiscovery
        {
            Id = "Orders.Api.Order", Name = "Order",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\Domain\Order.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });

        model.Detections.Add(new EfEntityDetection("Order", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\Domain\Order.cs", LineNumber = 5,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var orderNode = graph.Node(NodeId.ForType("Orders.Api.Order"));
        Assert.NotNull(orderNode);
        Assert.Contains("aggregate", orderNode!.Tags);
    }
}
