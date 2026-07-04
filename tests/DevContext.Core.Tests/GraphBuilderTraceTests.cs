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

        // Member-origin (Phase 1): a call edge originates from the caller METHOD's Member node and lands
        // on the callee METHOD's Member node (both carried on CallEdge), so a method-anchored trace
        // descends method-to-method instead of inheriting every sibling's edges.
        var callerMemberId = NodeId.ForMember("Orders.Api.OrderService", "ProcessOrder");
        var calleeMemberId = NodeId.ForMember("Orders.Api.OrderRepository", "Save");
        Assert.Contains(graph.OutEdges(callerMemberId), e => e.Kind == EdgeKind.Calls && e.To == calleeMemberId);

        // ...and the old Type→Type folded edge no longer exists (that fold was the fabrication bug).
        var callerTypeId = NodeId.ForType("Orders.Api.OrderService");
        Assert.DoesNotContain(graph.OutEdges(callerTypeId), e => e.Kind == EdgeKind.Calls);
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
        var eventId = NodeId.ForType("OrderStartedDomainEvent");
        Assert.True(graph.Contains(eventId), "Event node should exist");
        Assert.Contains(graph.OutEdges(orderId), e => e.Kind == EdgeKind.Raises && e.To == eventId);
    }

    [Fact]
    public void C1_raises_fires_for_a_genuine_event_constructor()
    {
        // Positive counterpart to the substring/gate tests below: a plain `new TEvent(...)` (no
        // AddDomainEvent/RaiseDomainEvent wrapper) for a type in the model-derived event set must still
        // raise.
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
            SourceBody = "public async Task Handle() { var evt = new OrderStartedIntegrationEvent(orderId); await _service.AddAndSaveEventAsync(evt); }",
        });
        model.Types.TryAdd("Orders.Api.OrderStartedIntegrationEvent", new TypeDiscovery
        {
            Id = "Orders.Api.OrderStartedIntegrationEvent", Name = "OrderStartedIntegrationEvent",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\OrderStartedIntegrationEvent.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            BaseTypes = ["IntegrationEvent"],
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var handlerId = NodeId.ForType("Orders.Api.CreateOrderCommandHandler");
        var eventId = NodeId.ForType("Orders.Api.OrderStartedIntegrationEvent");
        Assert.Contains(graph.OutEdges(handlerId), e => e.Kind == EdgeKind.Raises && e.To == eventId);
    }

    [Fact]
    public void C1_raises_does_not_fire_for_mediatr_command_construction()
    {
        // E5: a MediatR command implements IRequest<T> — BuildEventTypeNameSet (shared with the Sends
        // conjunction gate) treats IRequest as an event-ish marker, so a plain `new CreateOrderCommand(...)`
        // construction used to be misread as "raises CreateOrderCommand", duplicating the real Sends seam.
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
            SourceBody = "public async Task CreateOrderAsync() { var cmd = new CreateOrderCommand(items); await _mediator.Send(cmd); }",
        });
        model.Types.TryAdd("Orders.Api.CreateOrderCommand", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommand", Name = "CreateOrderCommand",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\CreateOrderCommand.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            ImplementedInterfaces = ["IRequest<bool>"],
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var apiId = NodeId.ForType("Orders.Api.OrdersApi");
        var commandId = NodeId.ForType("Orders.Api.CreateOrderCommand");
        Assert.DoesNotContain(graph.OutEdges(apiId), e => e.Kind == EdgeKind.Raises && e.To == commandId);
        // The genuine seam (Sends) must still be present — this isn't a case of losing the edge entirely.
        Assert.Contains(graph.OutEdges(apiId), e => e.Kind == EdgeKind.Sends && e.To == commandId);
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
        var requestId = NodeId.ForType("CreateOrderCommand");
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

        var entityId = NodeId.ForType("Orders.Api.Order");
        var ctxId = NodeId.ForType("Orders.Api.OrderingContext");
        Assert.Contains(graph.OutEdges(entityId), e => e.Kind == EdgeKind.ReadsWrites && e.To == ctxId);
    }

    [Fact]
    public void C1_data_edges_reject_entity_name_as_prefix_of_longer_identifier()
    {
        // E4: a plain substring match let "CardType" match inside "CardTypeId" (a DTO property name),
        // fabricating a ReadsWrites edge to an entity the body never actually touches.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:\repo\src\Orders.Api\Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.Handler", new TypeDiscovery
        {
            Id = "Orders.Api.Handler", Name = "Handler",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\Handler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            SourceBody = "var requestId = request.CardTypeId;",
        });
        model.Types.TryAdd("Orders.Api.CardType", new TypeDiscovery
        {
            Id = "Orders.Api.CardType", Name = "CardType",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\Domain\CardType.cs",
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
        model.Detections.Add(new EfEntityDetection("CardType", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\Domain\CardType.cs", LineNumber = 5,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var handlerId = NodeId.ForType("Orders.Api.Handler");
        var cardTypeId = NodeId.ForType("Orders.Api.CardType");
        Assert.DoesNotContain(graph.OutEdges(handlerId), e => e.Kind == EdgeKind.ReadsWrites && e.To == cardTypeId);
    }

    [Fact]
    public void C1_data_edges_are_member_scoped_not_leaked_to_sibling_method()
    {
        // E4 (eShop shape): OrdersApi.CreateOrderAsync only carries a CardTypeId property — it never
        // touches the CardType entity — while the sibling OrdersApi.GetCardTypesAsync genuinely returns
        // CardType. Each method's own data edge must land on its own Member node, not bleed into the
        // other's.
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
            SourceBody = "public static class OrdersApi { "
                + "public static async Task CreateOrderAsync(CreateOrderRequest request) { var cmd = new CreateOrderCommand(request.CardTypeId); } "
                + "public static async Task<CardType> GetCardTypesAsync(IOrderQueries orderQueries) { return await orderQueries.GetCardTypeAsync(); } "
                + "}",
        });
        model.Types.TryAdd("Orders.Api.CardType", new TypeDiscovery
        {
            Id = "Orders.Api.CardType", Name = "CardType",
            Namespace = "Orders.Api", FilePath = @"C:\repo\src\Orders.Api\Domain\CardType.cs",
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
        model.Detections.Add(new EfEntityDetection("CardType", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Orders.Api\Domain\CardType.cs", LineNumber = 5,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var createOrderMemberId = NodeId.ForMember("Orders.Api.OrdersApi", "CreateOrderAsync");
        var getCardTypesMemberId = NodeId.ForMember("Orders.Api.OrdersApi", "GetCardTypesAsync");
        var cardTypeId = NodeId.ForType("Orders.Api.CardType");

        Assert.DoesNotContain(graph.OutEdges(createOrderMemberId), e => e.Kind == EdgeKind.ReadsWrites && e.To == cardTypeId);
        Assert.Contains(graph.OutEdges(getCardTypesMemberId), e => e.Kind == EdgeKind.ReadsWrites && e.To == cardTypeId);
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

        var entry = entries.FirstOrDefault(e => e.Title == "POST /api/orders");
        Assert.NotNull(entry);

        // Verify graph structure: entry → Sends → request → Handles → handler → Raises → event → Consumes → handler
        var trace = new TraceBuilder(graph).Build(entry!, new Graph.TraceOptions { MaxDepth = 6 });
        Assert.Equal(SeamKind.Entry, trace.Root.Seam);

        // At minimum, verify the graph has the expected nodes and edges
        Assert.True(graph.Contains(NodeId.ForType("CreateOrderCommand")));
        Assert.True(graph.Contains(NodeId.ForType("Orders.Api.CreateOrderCommandHandler")));
        Assert.Contains(graph.OutEdges(NodeId.ForType("CreateOrderCommand")),
            e => e.Kind == EdgeKind.Handles);
        Assert.True(graph.Contains(NodeId.ForType("OrderStartedDomainEvent")));
        Assert.Contains(graph.OutEdges(NodeId.ForType("OrderStartedDomainEvent")),
            e => e.Kind == EdgeKind.Consumes);
    }
}
