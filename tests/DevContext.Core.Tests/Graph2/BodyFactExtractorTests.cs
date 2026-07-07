using DevContext.Core.Graph2;

using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Tests.Graph2;

/// <summary>L2.1 — structured BodyFacts extraction over the shared parse (the regex funeral's first
/// coffin). Every op knows its enclosing member + line by construction; string literals never enter.</summary>
public sealed class BodyFactExtractorTests
{
    private static ImmutableArray<BodyFacts> Extract(string code)
        => BodyFactExtractor.Extract(CSharpSyntaxTree.ParseText(code), "/src/Test.cs", "Test.Proj");

    private static BodyFacts Member(string code, string memberName)
        => Extract(code).Single(b => b.MemberName == memberName);

    [Fact]
    public void Member_id_encodes_type_fqn_and_param_count()
    {
        const string code = "namespace A.B; class C { public void M(int x, int y) { } }";
        var facts = Extract(code);
        var m = Assert.Single(facts);
        Assert.Equal(SymbolKind.Member, m.Member.Kind);
        Assert.Equal("A.B.C::M(2)", m.Member.Canonical);
        Assert.Equal("Test.Proj", m.Project);
        Assert.Equal("/src/Test.cs", m.File);
    }

    [Fact]
    public void Captures_invocation_with_receiver_type_from_lambda_param()
    {
        // Verbatim shape of CheckoutBasketEndpoints.AddRoutes — the minimal-API lambda receiver.
        const string code = """
            namespace Basket.API.Basket.CheckoutBasket;
            public class CheckoutBasketEndpoints
            {
                public void AddRoutes(IEndpointRouteBuilder app)
                {
                    app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, ISender sender) =>
                    {
                        var command = request.Adapt<CheckoutBasketCommand>();
                        var result = await sender.Send(command);
                        return result;
                    });
                }
            }
            """;
        var body = Member(code, "AddRoutes");
        var send = body.Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Send");
        Assert.Equal("sender", send.ReceiverText);
        Assert.Equal("ISender", send.ReceiverType?.Text);
        Assert.Equal("command", Assert.Single(send.Args).Text);
    }

    [Fact]
    public void Infers_local_type_from_Adapt_generic()
    {
        const string code = """
            namespace N; class C { void M(object request) { var command = request.Adapt<CheckoutBasketCommand>(); } }
            """;
        var body = Assert.Single(Extract(code));
        var local = body.Ops.OfType<LocalDeclOp>().Single(l => l.Name == "command");
        Assert.Equal("CheckoutBasketCommand", local.InferredFrom?.Text);
        Assert.Null(local.DeclaredType); // `var`
    }

    [Fact]
    public void Infers_local_type_from_new_creation()
    {
        const string code = "namespace N; class C { void M() { var x = new CreateOrderCommand(1); } }";
        var body = Assert.Single(Extract(code));
        var local = body.Ops.OfType<LocalDeclOp>().Single(l => l.Name == "x");
        Assert.Equal("CreateOrderCommand", local.InferredFrom?.Text);
    }

    [Fact]
    public void Infers_local_type_from_same_type_method_return()
    {
        // The BasketCheckoutEventHandler.Consume pattern: `var command = MapToCreateOrderCommand(evt)`.
        const string code = """
            namespace N;
            class C
            {
                void Consume(object message)
                {
                    var command = MapToCreateOrderCommand(message);
                }
                private CreateOrderCommand MapToCreateOrderCommand(object message) => new CreateOrderCommand(message);
            }
            """;
        var body = Member(code, "Consume");
        var local = body.Ops.OfType<LocalDeclOp>().Single(l => l.Name == "command");
        Assert.Equal("CreateOrderCommand", local.InferredFrom?.Text);
    }

    [Fact]
    public void Captures_generic_args_and_line_numbers()
    {
        const string code = """
            namespace N;
            class C
            {
                void M(object x)
                {
                    var e = x.Adapt<BasketCheckoutEvent>();
                }
            }
            """;
        var body = Assert.Single(Extract(code));
        var adapt = body.Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Adapt");
        Assert.Equal("BasketCheckoutEvent", Assert.Single(adapt.GenericArgs).Text);
        Assert.Equal(6, adapt.Line); // 1-based line of the invocation
    }

    [Fact]
    public void String_literals_never_enter_the_op_stream()
    {
        const string code = """
            namespace N; class C { void M() { var s = "sender.Send(NotACommand)"; } }
            """;
        var body = Assert.Single(Extract(code));
        Assert.DoesNotContain(body.Ops.OfType<InvocationOp>(), i => i.MethodName == "Send");
        Assert.DoesNotContain(body.Ops.OfType<IdentifierUseOp>(), i => i.Identifier == "NotACommand");
    }

    [Fact]
    public void Facts_version_is_v1()
        => Assert.Equal("facts-v1", BodyFactsVersion.Version);

    [Fact]
    public async Task Body_facts_cache_builds_once_and_is_reused()
    {
        var ctx = new SharedAnalysisContext();
        var calls = 0;
        ImmutableArray<BodyFacts> Factory()
        {
            Interlocked.Increment(ref calls);
            return BodyFactExtractor.Extract(
                CSharpSyntaxTree.ParseText("namespace N; class C { void M() {} }"), "/src/C.cs", "N");
        }

        var first = await ctx.GetOrBuildBodyFactsAsync("/src/C.cs", () => Task.FromResult(Factory()));
        var second = await ctx.GetOrBuildBodyFactsAsync("/src/C.cs", () => Task.FromResult(Factory()));

        Assert.Equal(1, calls); // content-keyed cache — factory runs once
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Pipeline_extractor_populates_facts_from_the_shared_parse_and_is_output_neutral()
    {
        const string endpointSource = """
            namespace Basket.API.Basket.CheckoutBasket;
            public class CheckoutBasketEndpoints
            {
                public void AddRoutes(IEndpointRouteBuilder app)
                {
                    app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, ISender sender) =>
                    {
                        var command = request.Adapt<CheckoutBasketCommand>();
                        var result = await sender.Send(command);
                        return result;
                    });
                }
            }
            """;
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Basket.API\CheckoutBasketEndpoints.cs", endpointSource);

        var ctx = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo")
            .WithOptions(new ExtractionOptions { BuildFullGraph = true })
            .Build();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Basket.API\CheckoutBasketEndpoints.cs"];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\Basket.API\Basket.API.csproj"];

        var model = new DiscoveryModel();
        var extractor = new BodyFactsExtractor();
        Assert.True(extractor.ShouldRun(ctx, model));
        await extractor.ExtractAsync(ctx, model, default);

        Assert.NotEmpty(ctx.Analysis.AllBodyFacts);
        var addRoutes = ctx.Analysis.AllBodyFacts.Single(b => b.MemberName == "AddRoutes");
        Assert.Contains(addRoutes.Ops.OfType<InvocationOp>(), i => i.MethodName == "Send");
        Assert.Equal("Basket.API", addRoutes.Project); // file→project stamping

        // Output-neutral: the model the graph builds from is untouched until the assembler consumes facts (L2.3).
        Assert.Empty(model.Detections);
        Assert.Empty(model.Diagnostics);
    }
}
