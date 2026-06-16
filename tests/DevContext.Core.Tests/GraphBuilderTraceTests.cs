using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class GraphBuilderTraceTests
{
    [Fact]
    public void C1_call_edges_from_model_call_edges()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.OrderService", new TypeDiscovery
        {
            Id = "Orders.Api.OrderService", Name = "OrderService",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\OrderService.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            Methods = [new MethodSignature("ProcessOrder", "void", [], [], Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
        });
        model.Types.TryAdd("Orders.Api.OrderRepository", new TypeDiscovery
        {
            Id = "Orders.Api.OrderRepository", Name = "OrderRepository",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\OrderRepository.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
            Methods = [new MethodSignature("Save", "void", [], [], Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
        });
        model.CallEdges.Add(new CallEdge(
            "Orders.Api.OrderService", "ProcessOrder",
            "Orders.Api.OrderRepository", "Save",
            @"C:\repo\src\Orders.Api\OrderService.cs:25"));

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        // CallEdges use type-level nodes as fallback (member nodes not auto-created)
        var callerTypeId = NodeId.ForType("Orders.Api.OrderService");
        var calleeTypeId = NodeId.ForType("Orders.Api.OrderRepository");
        Assert.Contains(graph.OutEdges(callerTypeId), e => e.Kind == EdgeKind.Calls && e.To == calleeTypeId);
    }

    [Fact]
    public void C1_raises_from_domain_event_pattern()
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
            SourceBody = "public Order() { AddDomainEvent(new OrderStartedDomainEvent(this)); }",
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var orderId = NodeId.ForType("Orders.Api.Order");
        var eventId = NodeId.ForEvent("OrderStartedDomainEvent");
        Assert.True(graph.Contains(eventId), "Event node should exist");
        Assert.Contains(graph.OutEdges(orderId), e => e.Kind == EdgeKind.Raises && e.To == eventId);
    }

    [Fact]
    public void C1_sends_from_mediator_send_pattern()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.OrdersApi", new TypeDiscovery
        {
            Id = "Orders.Api.OrdersApi", Name = "OrdersApi",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\OrdersApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
            SourceBody = "var cmd = new CreateOrderCommand(items); await _mediator.Send(cmd);",
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var apiId = NodeId.ForType("Orders.Api.OrdersApi");
        var requestId = NodeId.ForRequest("CreateOrderCommand");
        Assert.True(graph.Contains(requestId), "Request node should exist");
        Assert.Contains(graph.OutEdges(apiId), e => e.Kind == EdgeKind.Sends && e.To == requestId);
    }

    [Fact]
    public void C1_data_edges_link_entity_to_data_store()
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
        model.Types.TryAdd("Orders.Api.OrderingContext", new TypeDiscovery
        {
            Id = "Orders.Api.OrderingContext", Name = "OrderingContext",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\OrderingContext.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
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

        var entityId = NodeId.ForEntity("Orders.Api.Order");
        var ctxId = NodeId.ForType("Orders.Api.OrderingContext");
        Assert.Contains(graph.OutEdges(entityId), e => e.Kind == EdgeKind.ReadsWrites && e.To == ctxId);
    }

    [Fact]
    public void Full_trace_path_entry_to_event()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Orders.Infrastructure", @"C:\repo\src\Orders.Infrastructure\Orders.Infrastructure.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        model.Types.TryAdd("Orders.Api.OrdersApi", new TypeDiscovery
        {
            Id = "Orders.Api.OrdersApi", Name = "OrdersApi",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\OrdersApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
            SourceBody = "var cmd = new CreateOrderCommand(items); await _mediator.Send(cmd);",
        });
        model.Types.TryAdd("Orders.Api.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommandHandler", Name = "CreateOrderCommandHandler",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            SourceBody = "public async Task Handle() { var order = new Order(); order.AddDomainEvent(new OrderStartedDomainEvent(order)); _repo.Add(order); }",
        });
        model.Types.TryAdd("Orders.Api.ValidateBuyerHandler", new TypeDiscovery
        {
            Id = "Orders.Api.ValidateBuyerHandler", Name = "ValidateBuyerHandler",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\ValidateBuyerHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        model.Detections.Add(new EndpointDetection("POST", "/api/orders", "OrdersApi", "CreateOrderAsync", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\OrdersApi.cs", LineNumber = 10,
        });
        model.Detections.Add(new MediatRHandlerDetection("CreateOrderCommand", "bool", "CreateOrderCommandHandler", MediatRKind.Command)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\CreateOrderCommandHandler.cs", LineNumber = 20,
        });
        model.Detections.Add(new MediatRHandlerDetection("OrderStartedDomainEvent", "void", "ValidateBuyerHandler", MediatRKind.Notification)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\ValidateBuyerHandler.cs", LineNumber = 15,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = entries.FirstOrDefault(e => string.Equals(e.Title, "POST /api/orders", StringComparison.Ordinal));
        Assert.NotNull(entry);

        // Verify graph structure: entry → Sends → request → Handles → handler → Raises → event → Consumes → handler
        var trace = new TraceBuilder(graph).Build(entry!, new Graph.TraceOptions { MaxDepth = 6 });
        Assert.Equal(SeamKind.Entry, trace.Root.Seam);

        // At minimum, verify the graph has the expected nodes and edges
        Assert.True(graph.Contains(NodeId.ForRequest("CreateOrderCommand")));
        Assert.True(graph.Contains(NodeId.ForHandler("Orders.Api.CreateOrderCommandHandler")));
        Assert.Contains(graph.OutEdges(NodeId.ForRequest("CreateOrderCommand")),
            e => e.Kind == EdgeKind.Handles);
        Assert.True(graph.Contains(NodeId.ForEvent("OrderStartedDomainEvent")));
        Assert.Contains(graph.OutEdges(NodeId.ForEvent("OrderStartedDomainEvent")),
            e => e.Kind == EdgeKind.Consumes);
    }
}
