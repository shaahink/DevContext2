using DevContext.Core.Graph;
using DevContext.Core.Graph2;
using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Tests;

public sealed class PatternZooTests
{
    private static ProjectInfo Pi => new("PatternZoo", @"C:\repo\PatternZoo\PatternZoo.csproj", "C#", ["net10.0"], [], []);
    private static GraphBuilder Builder => new(new SyntacticSymbolResolver(), new NoiseFilter(new ProjectClassifier([Pi])));

    private static (CodeGraph graph, ImmutableArray<EntryPoint> entries) Build(string sourceBody, ImmutableArray<MethodSignature>? methods = null)
    {
        var model = new DiscoveryModel { Projects = [Pi] };
        var filePath = @"C:\repo\PatternZoo\Handler.cs";
        model.Types.TryAdd("PatternZoo.Handler", new TypeDiscovery
        {
            Id = "PatternZoo.Handler",
            Name = "Handler",
            Namespace = "PatternZoo",
            FilePath = filePath,
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            SourceBody = sourceBody,
            Methods = methods ?? [],
        });

        // L2.3: extract BodyFacts from source body so the seam detectors (not the old regex methods) produce edges.
        // Wrap in namespace so BodyFactExtractor produces FQNs matching the model's type id.
        var wrappedSource = $"namespace PatternZoo {{ {sourceBody} }}";
        var parseOpts = CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG");
        var tree = CSharpSyntaxTree.ParseText(wrappedSource, parseOpts, path: filePath);
        // F1 (#33): unless a test pins its own signatures, the model DECLARES what the source
        // declares — INV-C refuses a member node of a type whose model entry does not vouch for
        // it, and a zoo fixture with `Methods = []` would drop every edge it means to assert.
        model.Types["PatternZoo.Handler"].Methods = methods ?? TestMethodSignatures.DeclaredIn(tree);
        var facts = BodyFactExtractor.Extract(tree, filePath, "PatternZoo");
        return Builder.Build(model, SolutionScope.FromModel(model), facts);
    }

    [Fact]
    public void Primary_constructor_handler_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public class Handler(IMediator mediator)
            {
                public Task Run() => mediator.Send(new PingCommand());
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var pingId = NodeId.ForType("PingCommand");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == pingId);
    }

    [Fact]
    public void Expression_bodied_method_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;
                public Task Run() => _m.Send(new RunCommand());
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var runCommandId = NodeId.ForType("RunCommand");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == runCommandId);
    }

    [Fact]
    public void Record_with_body_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public record Handler(IMediator Mediator)
            {
                public Task Execute() => Mediator.Send(new RecordCommand());
            }
            """);

        var execId = NodeId.ForMember("PatternZoo.Handler", "Execute");
        var recordCmdId = NodeId.ForType("RecordCommand");
        Assert.Contains(graph.OutEdges(execId, EdgeKind.Sends), e => e.To == recordCmdId);
    }

    [Fact]
    public void Local_function_inside_method_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;

                public Task Run()
                {
                    return Dispatch();

                    Task Dispatch() => _m.Send(new LocalCommand());
                }
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var localCmdId = NodeId.ForType("LocalCommand");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == localCmdId);
    }

    [Fact]
    public void Required_init_property_handler_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                public required IMediator Mediator { get; init; }

                public Task Run() => Mediator.Send(new InitCommand());
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var initCmdId = NodeId.ForType("InitCommand");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == initCmdId);
    }

    [Fact]
    public void Collection_expression_in_scope_does_not_break_seam_detection()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;

                public Task Run()
                {
                    string[] tags = ["urgent", "batch"];
                    return _m.Send(new CollectionCommand());
                }
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var collCmdId = NodeId.ForType("CollectionCommand");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == collCmdId);
    }

    [Fact]
    public void Conditional_block_still_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;

                public Task Run()
                {
            #if DEBUG
                    return _m.Send(new DebugCommand());
            #else
                    return Task.CompletedTask;
            #endif
                }
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var debugCmdId = NodeId.ForType("DebugCommand");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == debugCmdId);
    }

    [Fact]
    public void Raw_string_literal_containing_send_pattern_does_not_produce_fabricated_edge()
    {
        var sourceBody = "public class Handler\n" +
            "{\n" +
            "    private readonly IMediator _m;\n" +
            "    public Handler(IMediator m) => _m = m;\n" +
            "    public Task Run()\n" +
            "    {\n" +
            "        var sql = \"\"\"\n" +
            "            SELECT * FROM Commands\n" +
            "            WHERE Handler.Send(new FakeCommand()) IS NOT NULL\n" +
            "            \"\"\";\n" +
            "        return _m.Send(new RealCommand());\n" +
            "    }\n" +
            "}\n";

        var (graph, _) = Build(sourceBody);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var realCmdId = NodeId.ForType("RealCommand");
        var fakeCmdId = NodeId.ForType("FakeCommand");

        var sends = graph.OutEdges(runId, EdgeKind.Sends);

        // RealCommand must be present (the actual seam in the method)
        Assert.Contains(sends, e => e.To == realCmdId);

        // FakeCommand must NOT be present (it's inside a string literal, not real code)
        Assert.DoesNotContain(sends, e => e.To == fakeCmdId);
    }

    [Fact]
    public void Send_of_parameter_in_inline_method_resolves_correctly()
    {
        // I1.2: parameter-type fallback from method signature
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;
                public Task Handle(MyCommand cmd) => _m.Send(cmd);
            }
            """, methods:
        [
            new MethodSignature("Handle", "Task", ["MyCommand"], ["cmd"],
                Microsoft.CodeAnalysis.Accessibility.Public, false, false),
        ]);

        var handleId = NodeId.ForMember("PatternZoo.Handler", "Handle");
        var myCmdId = NodeId.ForType("MyCommand");
        Assert.Contains(graph.OutEdges(handleId, EdgeKind.Sends), e => e.To == myCmdId);
    }

    [Fact]
    public void Multiple_seams_in_same_class_are_all_detected()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;
                public Task Create() => _m.Send(new CreateCommand());
                public Task Update() => _m.Send(new UpdateCommand());
                public Task Delete() => _m.Send(new DeleteCommand());
            }
            """);

        var createId = NodeId.ForMember("PatternZoo.Handler", "Create");
        var updateId = NodeId.ForMember("PatternZoo.Handler", "Update");
        var deleteId = NodeId.ForMember("PatternZoo.Handler", "Delete");

        Assert.Contains(graph.OutEdges(createId, EdgeKind.Sends), e => e.To == NodeId.ForType("CreateCommand"));
        Assert.Contains(graph.OutEdges(updateId, EdgeKind.Sends), e => e.To == NodeId.ForType("UpdateCommand"));
        Assert.Contains(graph.OutEdges(deleteId, EdgeKind.Sends), e => e.To == NodeId.ForType("DeleteCommand"));
    }

    [Fact]
    public void Sends_with_async_suffix_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;
                public Task Run() => _m.SendAsync(new AsyncCommand());
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == NodeId.ForType("AsyncCommand"));
    }

    [Fact]
    public void Publish_produces_sends_edge()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                private readonly IMediator _m;
                public Handler(IMediator m) => _m = m;
                public Task Run() => _m.Publish(new MyNotification());
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        Assert.Contains(graph.OutEdges(runId, EdgeKind.Sends), e => e.To == NodeId.ForType("MyNotification"));
    }

    [Fact]
    public void Non_mediator_send_does_not_produce_fabricated_edge()
    {
        var (graph, _) = Build("""
            public class Handler
            {
                public void Run()
                {
                    var client = new SmtpClient();
                    client.Send(new MailMessage());
                }
            }
            """);

        var runId = NodeId.ForMember("PatternZoo.Handler", "Run");
        var sends = graph.OutEdges(runId, EdgeKind.Sends);
        Assert.DoesNotContain(sends, e => e.To == NodeId.ForType("MailMessage"));
    }
}
