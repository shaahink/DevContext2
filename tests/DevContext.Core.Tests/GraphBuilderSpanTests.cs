using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>Tests for span-bound variable resolution in the Sends/Raises body-scan seams.
/// Proves and then guards the I1.1 fix: variable search must stay inside the enclosing method span.</summary>
public sealed class GraphBuilderSpanTests
{
    [Fact]
    public void Send_of_parameter_does_not_steal_sibling_methods_new()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Dispatcher.Api", @"C:\repo\src\Dispatcher.Api\Dispatcher.Api.csproj",
                    "C#", ["net10.0"], [], []),
            ],
        };
        model.Types.TryAdd("Dispatcher.Api.Dispatcher", new TypeDiscovery
        {
            Id = "Dispatcher.Api.Dispatcher",
            Name = "Dispatcher",
            Namespace = "Dispatcher.Api",
            FilePath = @"C:\repo\src\Dispatcher.Api\Dispatcher.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            SourceBody = """
                public class Dispatcher
                {
                    private readonly IMediator _m;
                    public Task A() => _m.Send(new AlphaCommand());
                    public Task B(BetaCommand cmd) => _m.Send(cmd);
                }
                """,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        // B's Send(cmd) must NOT produce a Sends edge to AlphaCommand
        var bMemberId = NodeId.ForMember("Dispatcher.Api.Dispatcher", "B");
        var alphaCommandId = NodeId.ForType("AlphaCommand");
        var allSendsFromB = graph.OutEdges(bMemberId, EdgeKind.Sends);
        Assert.DoesNotContain(allSendsFromB, edge => edge.To == alphaCommandId);
    }

    [Fact]
    public void Raises_of_parameter_does_not_steal_sibling_methods_new()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Domain.Api", @"C:\repo\src\Domain.Api\Domain.Api.csproj",
                    "C#", ["net10.0"], [], []),
            ],
        };
        model.Types.TryAdd("Domain.Api.Aggregate", new TypeDiscovery
        {
            Id = "Domain.Api.Aggregate",
            Name = "Aggregate",
            Namespace = "Domain.Api",
            FilePath = @"C:\repo\src\Domain.Api\Aggregate.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            SourceBody = """
                public class Aggregate
                {
                    public Aggregate() { AddDomainEvent(new StartedEvent()); }
                    public void Apply(AppliedEvent evt) { AddDomainEvent(evt); }
                }
                """,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        // Apply's AddDomainEvent(evt) must NOT produce a Raises edge to StartedEvent
        var applyMemberId = NodeId.ForMember("Domain.Api.Aggregate", "Apply");
        var startedEventId = NodeId.ForType("StartedEvent");
        var raisesFromApply = graph.OutEdges(applyMemberId, EdgeKind.Raises);
        Assert.DoesNotContain(raisesFromApply, edge => edge.To == startedEventId);
    }

    [Fact]
    public void Send_of_parameter_type_resolves_via_method_signature()
    {
        // I1.2: B(BetaCommand cmd) has no in-span new, but cmd's type IS BetaCommand
        // from the method signature — the fallback should resolve it.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Handler.Api", @"C:\repo\src\Handler.Api\Handler.Api.csproj",
                    "C#", ["net10.0"], [], []),
            ],
        };
        model.Types.TryAdd("Handler.Api.Handler", new TypeDiscovery
        {
            Id = "Handler.Api.Handler",
            Name = "Handler",
            Namespace = "Handler.Api",
            FilePath = @"C:\repo\src\Handler.Api\Handler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            Methods =
            [
                new MethodSignature("Handle",
                    "Task",
                    ["BetaCommand"],
                    ["cmd"],
                    Microsoft.CodeAnalysis.Accessibility.Public,
                    false, false),
            ],
            SourceBody = """
                public class Handler
                {
                    private readonly IMediator _m;
                    public Task Handle(BetaCommand cmd) => _m.Send(cmd);
                }
                """,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        // Handle's Send(cmd) should resolve to BetaCommand via param-type fallback
        var handleMemberId = NodeId.ForMember("Handler.Api.Handler", "Handle");
        var betaCmdId = NodeId.ForType("BetaCommand");
        var sendsFromHandle = graph.OutEdges(handleMemberId, EdgeKind.Sends);
        Assert.Contains(sendsFromHandle, edge => edge.To == betaCmdId);
    }
}
