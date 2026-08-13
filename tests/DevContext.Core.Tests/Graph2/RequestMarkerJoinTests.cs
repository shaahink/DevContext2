using DevContext.Core.Extractors.Specific;
using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Tests.Graph2;

/// <summary>E1.4 — a REQUEST MARKER is not a handler, and its type argument is the RESPONSE.
///
/// <para>Measured on eShop before this fixture existed: <c>CreateOrderCommand : IRequest&lt;bool&gt;</c>
/// produced a <c>MediatRHandlerDetection</c> whose <c>RequestType</c> was <c>"bool"</c>, so the graph
/// minted <c>Type:bool</c> — tagged <c>command</c>, in no project — and drew
/// <c>Type:bool --Handles--&gt; CreateOrderCommand</c>. The same path minted <c>Type:R</c> from
/// <c>IdentifiedCommand&lt;T, R&gt; : IRequest&lt;R&gt;</c>: a GENERIC PARAMETER as a graph node. The
/// request itself was also tagged <c>handler</c>, because the join's handler-side node is the one that
/// carries <see cref="RoleTags.Handler"/>.</para>
///
/// <para>Why it matters beyond tidiness: <c>Type:bool</c> was one of the TWO members of
/// <c>impact(up)</c> for <c>CreateOrderCommandHandler</c> on eShop — half of the engine's answer to
/// "what depends on this handler" was a boolean. <c>IRequest&lt;TResponse&gt;</c> means the class IS
/// the request; there is no handler to point at, so there is no edge to draw.</para></summary>
public sealed class RequestMarkerJoinTests
{
    private static readonly HashSet<string> NoDerivedHandlers = new(StringComparer.Ordinal);

    [Theory]
    [InlineData("IRequest<bool>", "bool", MediatRKind.Command)]
    [InlineData("IQuery<OrderDto>", "OrderDto", MediatRKind.Query)]
    [InlineData("ICommand<int>", "int", MediatRKind.Command)]
    public void A_request_marker_names_itself_as_the_request_and_its_type_arg_as_the_response(
        string iface, string expectedResponse, MediatRKind expectedKind)
    {
        var parsed = MediatRExtractor.TryParseHandlerType(iface, NoDerivedHandlers);

        Assert.NotNull(parsed);
        Assert.Equal("<self>", parsed!.Value.RequestType);
        Assert.Equal(expectedResponse, parsed.Value.ResponseType);
        Assert.Equal(expectedKind, parsed.Value.Kind);
    }

    [Fact]
    public void A_generic_parameter_never_becomes_the_request_type()
    {
        // IdentifiedCommand<T, R> : IRequest<R> — the eShop shape that minted Type:R.
        var parsed = MediatRExtractor.TryParseHandlerType("IRequest<R>", NoDerivedHandlers);

        Assert.NotNull(parsed);
        Assert.Equal("<self>", parsed!.Value.RequestType);
        Assert.Equal("R", parsed.Value.ResponseType);
    }

    [Fact]
    public void A_real_handler_interface_is_untouched()
    {
        var parsed = MediatRExtractor.TryParseHandlerType("IRequestHandler<CreateOrderCommand, bool>", NoDerivedHandlers);

        Assert.NotNull(parsed);
        Assert.Equal("CreateOrderCommand", parsed!.Value.RequestType);
        Assert.Equal("bool", parsed.Value.ResponseType);
    }

    [Fact]
    public void A_request_marker_detection_draws_no_edge_and_leaves_the_request_untagged_as_a_handler()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:/repo/src/Orders.Api/Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.CreateOrderCommand", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommand", Name = "CreateOrderCommand",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/CreateOrderCommand.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("Orders.Api.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommandHandler", Name = "CreateOrderCommandHandler",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        // What `CreateOrderCommand : IRequest<bool>` now yields...
        model.Detections.Add(new MediatRHandlerDetection("<self>", "bool", "CreateOrderCommand", MediatRKind.Command)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Orders.Api/CreateOrderCommand.cs", LineNumber = 3,
        });
        // ...beside the REAL join, which must survive untouched.
        model.Detections.Add(new MediatRHandlerDetection("CreateOrderCommand", "bool", "CreateOrderCommandHandler", MediatRKind.Command)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Orders.Api/CreateOrderCommandHandler.cs", LineNumber = 20,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var requestId = NodeId.ForType("Orders.Api.CreateOrderCommand");
        var handlerId = NodeId.ForType("Orders.Api.CreateOrderCommandHandler");

        // The real join is intact.
        Assert.Contains(graph.OutEdges(requestId), e => e.Kind == EdgeKind.Handles && e.To == handlerId);

        // No phantom node was minted for the marker, under any spelling.
        Assert.DoesNotContain(graph.Nodes, n => n.Id.Key.Contains("<self>", StringComparison.Ordinal));
        Assert.DoesNotContain(graph.Nodes, n => n.Id == NodeId.ForType("bool"));

        // Nothing "handles" the request except by the real join, and the request is not a handler.
        Assert.DoesNotContain(graph.AllEdges, e => e.Kind == EdgeKind.Handles && e.To == requestId);
        var request = Assert.Single(graph.Nodes, n => n.Id == requestId);
        Assert.DoesNotContain(RoleTags.Handler, request.Tags);
        Assert.Contains("command", request.Tags);
    }
}
