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
    public void Member_id_prefixes_global_for_a_namespaceless_type()
    {
        // A global-namespace type must key its members "global.Type::..." to match SyntaxStructureExtractor's
        // TypeDiscovery.Id (namespace ?? "global"). Without the prefix the BodyFacts member id ("OrdersApi")
        // diverges from the graph node id ("global.OrdersApi"), orphaning a seam edge from the entry node —
        // eShop's namespaceless OrdersApi /draft flow (T2.5).
        const string code = "public static class OrdersApi { public void Handle(int x) { } }";
        var m = Assert.Single(Extract(code));
        Assert.Equal("global.OrdersApi::Handle(1)", m.Member.Canonical);
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
        Assert.Null(send.ReceiverMember); // bare-identifier receiver — ReceiverText already names it
    }

    [Fact]
    public void Chained_call_receiver_is_the_call_result_not_the_root_identifier()
    {
        // F1 (#33): in `x.Y().Z()` the receiver of Z is what Y() RETURNS, not x. RootIdentifier used
        // to walk THROUGH the invocation and hand Z the root's type — `_db.SaveChangesAsync()
        // .ConfigureAwait(false)` recorded receiver `_db` (AppDbContext) and the binder minted
        // `AppDbContext::ConfigureAwait`. The op must carry NO receiver type (Tier B may bind the
        // true one later) and its ReceiverText must NOT be the root identifier — and must not be
        // null either, or the call would masquerade as a self-call in the binder's bare arm.
        const string code = """
            namespace Shop;
            public class Worker
            {
                private Session _session;
                public void Run()
                {
                    _session.Begin().Commit();
                }
            }
            """;
        var body = Member(code, "Run");
        var commit = body.Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Commit");
        Assert.Null(commit.ReceiverType);
        Assert.NotNull(commit.ReceiverText);
        Assert.NotEqual("_session", commit.ReceiverText);

        // The INNER call is untouched: `_session` is its real receiver.
        var begin = body.Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Begin");
        Assert.Equal("_session", begin.ReceiverText);
        Assert.Equal("Session", begin.ReceiverType?.Text);
    }

    [Fact]
    public void Body_facts_carry_the_member_declaration_line()
    {
        // T2.2: the member's own decl line, from the syntax walk (no re-parse), so the assembler can stamp
        // file:line on member nodes and packs never render a bare trailing colon.
        const string code = """
            namespace N;
            class C
            {
                public void First() { }

                public void Second(int x) { }
            }
            """;
        var facts = Extract(code);
        Assert.Equal(4, facts.Single(b => b.MemberName == "First").DeclLine);
        Assert.Equal(6, facts.Single(b => b.MemberName == "Second").DeclLine);
    }

    [Fact]
    public void Captures_receiver_member_for_property_accessed_receiver()
    {
        // eShop OrdersApi shape: the sender is reached through an [AsParameters] record (services.Mediator).
        // The root identifier is "services"; the trailing ".Mediator" segment must be captured so dispatch
        // detection can recognise it as MediatR (T2.5).
        const string code = """
            namespace Ordering.Api;
            public static class OrdersApi
            {
                public static async Task Handle(CreateOrderDraftCommand command, OrderServices services)
                    => await services.Mediator.Send(command);
            }
            """;
        var send = Member(code, "Handle").Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Send");
        Assert.Equal("services", send.ReceiverText);
        Assert.Equal("Mediator", send.ReceiverMember);
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
    public void Facts_version_is_v2()
        // v2: F1 (#33) — chained-call receivers stopped rooting at the leading identifier.
        => Assert.Equal("facts-v2", BodyFactsVersion.Version);

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
        fs.AddFile(@"C:/repo/src/Basket.API/CheckoutBasketEndpoints.cs", endpointSource);

        var ctx = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:/repo")
            .WithOptions(new ExtractionOptions { BuildFullGraph = true })
            .Build();
        ctx.Analysis.AllSourceFiles = [@"C:/repo/src/Basket.API/CheckoutBasketEndpoints.cs"];
        ctx.Analysis.AllProjectFiles = [@"C:/repo/src/Basket.API/Basket.API.csproj"];

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

    [Fact]
    public void Multi_lambda_with_same_name_param_produces_correct_receiver_per_scope()
    {
        const string code = """
            namespace N;
            class C
            {
                void M()
                {
                    var items = new List<int>();
                    items.Select((IHandler x) => x.Execute());
                    var strings = new List<string>();
                    strings.Select((ILogger x) => x.Log("hello"));
                }
            }
            """;
        var addRoutes = Member(code, "M");
        var execute = addRoutes.Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Execute");
        var log = addRoutes.Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Log");

        Assert.Equal("x", execute.ReceiverText);
        Assert.Equal("IHandler", execute.ReceiverType?.Text);
        Assert.Equal("x", log.ReceiverText);
        Assert.Equal("ILogger", log.ReceiverType?.Text);
    }

    // ── D-3 / G5.2: a `this.`-qualified receiver is the same call as the unqualified one ──────────

    [Fact]
    public void This_qualified_field_receiver_resolves_like_the_unqualified_spelling()
    {
        // D-3 (G5.1 Defect 1): GitVersion's five verbs write every collaborator call as
        // `this.service.Call()`. RootIdentifier walked the member access down to the `this` TOKEN, so
        // ReceiverText was "this", which matches no entry in the type scope — 16 of 17 invocation
        // sites across those verbs produced no receiver type at all and the CLI joined no handler.
        // The unqualified spelling of the SAME call resolves fine, so the two must agree.
        const string code = """
            namespace N;
            class C
            {
                private readonly IWidget widget;
                void Qualified() { this.widget.Do(); }
                void Bare() { widget.Do(); }
            }
            """;
        var qualified = Member(code, "Qualified").Ops.OfType<InvocationOp>().Single();
        var bare = Member(code, "Bare").Ops.OfType<InvocationOp>().Single();

        Assert.Equal(bare.ReceiverText, qualified.ReceiverText);
        Assert.Equal("widget", qualified.ReceiverText);
        Assert.Equal("IWidget", qualified.ReceiverType?.Text);
        // The member is already named by ReceiverText — a chain segment would double-count it and
        // send the binder hunting for a property called "widget" on IWidget.
        Assert.Null(qualified.ReceiverMember);
    }

    [Fact]
    public void This_qualified_chain_keeps_its_trailing_segment()
    {
        // The chain case: `this.services.Mediator.Send(c)` must read exactly like
        // `services.Mediator.Send(c)` — root "services" for the scope lookup, trailing "Mediator" for
        // the receiver-chain hop and for dispatch detection.
        const string code = """
            namespace N;
            class C
            {
                private readonly OrderServices services;
                void M(object c) { this.services.Mediator.Send(c); }
            }
            """;
        var send = Member(code, "M").Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Send");
        Assert.Equal("services", send.ReceiverText);
        Assert.Equal("OrderServices", send.ReceiverType?.Text);
        Assert.Equal("Mediator", send.ReceiverMember);
    }

    [Fact]
    public void Bare_this_self_call_still_reports_this()
    {
        // The guard on the normalisation above: `this.Helper()` has no member to unwrap, and the call
        // binder's self-call arm depends on seeing "this" (it joins the caller type only when the type
        // DECLARES the method). Reporting "Helper" here would make a self-call look like a field call.
        const string code = "namespace N; class C { void M() { this.Helper(); } void Helper() { } }";
        var call = Member(code, "M").Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Helper");
        Assert.Equal("this", call.ReceiverText);
        Assert.Null(call.ReceiverMember);
    }

    [Fact]
    public void This_rooted_chain_through_a_call_is_left_alone()
    {
        // Second guard: `this.Make().Do()` — the segment before the call is the RESULT of a call,
        // not a declared member, so nothing the type scope can resolve. The D-3 normalisation must
        // only fire when the walk to `this` is pure member access. F1 (#33) tightened the contract:
        // Do's receiver is what Make() RETURNS, so the op records the raw receiver expression
        // (never "this", which would send it to the binder's declares-the-method self-call arm and
        // bind Do to the caller type on a name coincidence).
        const string code = "namespace N; class C { void M() { this.Make().Do(); } }";
        var call = Member(code, "M").Ops.OfType<InvocationOp>().Single(i => i.MethodName == "Do");
        Assert.Equal("this.Make()", call.ReceiverText);
        Assert.Null(call.ReceiverType);
    }
}
