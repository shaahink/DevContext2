using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Tests;

/// <summary>T4.1 — a context pack must identify itself (repo, analyzed-at, git HEAD, focus) and
/// locate every node with a repo-relative file:line, so a human or agent can verify it against
/// the source it claims to describe.</summary>
public sealed class ContextPackBuilderTests
{
    private static RunReport DefaultReport => new()
    {
        Stages = [], Extractors = [], Scorers = [], Compressions = [],
        Cache = new(0, 0, 0, 0), Corpus = new(0, 0, 0),
        Funnel = new(0, 0, 0, 0, 0, 0),
        Parallelism = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
        TotalWall = TimeSpan.Zero,
    };

    /// <summary>Entry → controller member (file+line) → service member (file, NO line: the
    /// trailing-colon shape) with a type SourceBody so the trace carries salient lines.</summary>
    private static (GraphQuery Query, AnalysisSnapshot Snapshot) Arrange(
        DateTimeOffset? analyzedAt = null, string? gitHead = null)
    {
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var callerId = NodeId.ForMember("App.OrdersController", "Post");
        var calleeId = NodeId.ForMember("App.OrderService", "CreateOrder");
        var serviceTypeId = NodeId.ForType("App.OrderService");

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(callerId, "OrdersController.Post", NodeKind.Member)
        {
            FilePath = @"C:\repo\src\App\OrdersController.cs",
            LineNumber = 11,
            SourceBody = "public IActionResult Post() => _service.CreateOrder(new Order());",
        });
        g.AddNode(new GraphNode(serviceTypeId, "OrderService", NodeKind.Type)
        {
            SourceBody = "public class OrderService\n{\n    public void CreateOrder(Order o)\n    {\n        _repo.Save(o);\n    }\n}",
        });
        g.AddNode(new GraphNode(calleeId, "OrderService.CreateOrder", NodeKind.Member)
        {
            FilePath = @"C:\repo\src\App\OrderService.cs",
        });

        g.AddEdge(new GraphEdge(entryId, callerId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls));

        var graph = g.Build();
        var entries = ImmutableArray.Create(
            new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId));
        return (new GraphQuery(graph, entries), MakeSnapshot(graph, entries, analyzedAt, gitHead));
    }

    private static AnalysisSnapshot MakeSnapshot(
        CodeGraph graph, ImmutableArray<EntryPoint> entries,
        DateTimeOffset? analyzedAt = null, string? gitHead = null, string rootPath = @"C:\repo") => new()
    {
        Model = new DiscoveryModel(),
        Analysis = new SharedAnalysisContext(),
        Scenario = ScenarioRegistry.BuiltIn["overview"],
        Options = new ExtractionOptions(),
        Report = DefaultReport,
        RootPath = rootPath,
        Graph = graph,
        Entries = entries,
        AnalyzedAtUtc = analyzedAt,
        GitHead = gitHead,
    };

    [Fact]
    public void Pack_header_names_repo_focus_analyzed_at_and_head()
    {
        var (query, snapshot) = Arrange(
            analyzedAt: new DateTimeOffset(2026, 7, 16, 14, 30, 0, TimeSpan.Zero),
            gitHead: "abc1234def5678900000000000000000000000ff");

        var pack = new ContextPackBuilder(query, snapshot).Build("POST /orders");

        Assert.True(pack.Found);
        // The audit's `# ` empty title (snapshot.Explanation is never populated) must be gone:
        // the title names the repo and the focus.
        Assert.Contains("# repo — POST /orders", pack.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("# \n", pack.Content, StringComparison.Ordinal);
        Assert.Contains("analyzed 2026-07-16 14:30 UTC", pack.Content, StringComparison.Ordinal);
        Assert.Contains("HEAD abc1234", pack.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Pack_header_omits_head_and_timestamp_when_unknown()
    {
        var (query, snapshot) = Arrange();

        var pack = new ContextPackBuilder(query, snapshot).Build("POST /orders");

        Assert.Contains("# repo — POST /orders", pack.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("HEAD", pack.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("analyzed", pack.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Signature_locations_are_repo_relative_file_line_with_no_trailing_colon()
    {
        var (query, snapshot) = Arrange();

        var pack = new ContextPackBuilder(query, snapshot).Build("POST /orders");

        Assert.Contains("Location: src/App/OrdersController.cs:11", pack.Content, StringComparison.Ordinal);
        // No declaration line known → path only, never `file:` with a dangling colon.
        Assert.Contains("Location: src/App/OrderService.cs", pack.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("OrderService.cs:", pack.Content, StringComparison.Ordinal);
        // Absolute machine paths never leak into a pack.
        Assert.DoesNotContain(@"C:\repo", pack.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/repo", pack.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Salient_body_headings_carry_their_location()
    {
        var (query, snapshot) = Arrange();

        var pack = new ContextPackBuilder(query, snapshot).Build("POST /orders");

        Assert.Contains("### OrderService.CreateOrder — src/App/OrderService.cs", pack.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Contracts_section_selects_contract_shapes_not_a_signature_copy()
    {
        // T4.6 (audit C2) — the contracts card was a verbatim duplicate of signatures. Now it
        // selects only contract-shaped spine types: message contracts (role tags), interfaces,
        // DTOs — never a plain service class, and never entities.
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var callerId = NodeId.ForMember("App.OrdersController", "Post");
        var commandId = NodeId.ForType("App.CreateOrderCommand");
        var ifaceId = NodeId.ForType("App.IOrderRepository");
        var serviceId = NodeId.ForType("App.OrderService");

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(callerId, "OrdersController.Post", NodeKind.Member));
        g.AddNode(new GraphNode(commandId, "CreateOrderCommand", NodeKind.Type)
        {
            Tags = [RoleTags.Command],
            FilePath = @"C:\repo\src\App\CreateOrderCommand.cs",
            LineNumber = 5,
            SourceBody = "public record CreateOrderCommand(OrderDto Order) : ICommand<OrderResult>;",
        });
        g.AddNode(new GraphNode(ifaceId, "IOrderRepository", NodeKind.Type)
        {
            SourceBody = "public interface IOrderRepository\n{\n    Task Save(Order o);\n}",
        });
        g.AddNode(new GraphNode(serviceId, "OrderService", NodeKind.Type)
        {
            SourceBody = "public class OrderService\n{\n}",
        });

        g.AddEdge(new GraphEdge(entryId, callerId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerId, commandId, EdgeKind.Sends));
        g.AddEdge(new GraphEdge(callerId, ifaceId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerId, serviceId, EdgeKind.Calls));

        var graph = g.Build();
        var entries = ImmutableArray.Create(
            new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId));
        var snapshot = MakeSnapshot(graph, entries);

        var pack = new ContextPackBuilder(new GraphQuery(graph, entries), snapshot).Build("POST /orders");

        var contracts = pack.Sections.Single(s => s.Section == "contracts").Content;
        var signatures = pack.Sections.Single(s => s.Section == "signatures").Content;
        Assert.Contains("`CreateOrderCommand` (command) — src/App/CreateOrderCommand.cs:5", contracts, StringComparison.Ordinal);
        Assert.Contains("`IOrderRepository` (interface)", contracts, StringComparison.Ordinal);
        Assert.Contains("public record CreateOrderCommand(OrderDto Order) : ICommand<OrderResult>;", contracts, StringComparison.Ordinal);
        Assert.DoesNotContain("OrderService", contracts, StringComparison.Ordinal);
        Assert.NotEqual(signatures, contracts);
    }

    [Fact]
    public void Empty_sections_are_dropped_and_recorded_in_omitted()
    {
        // T4.6 (audit C2) — "Entities — 0 tok" must never ship; the pack says what it looked
        // for and found empty instead.
        var (query, snapshot) = Arrange();

        var pack = new ContextPackBuilder(query, snapshot).Build("POST /orders");

        Assert.DoesNotContain("## di_wiring", pack.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("## entities", pack.Content, StringComparison.Ordinal);
        Assert.Contains("di_wiring: empty — omitted", pack.Omitted);
        Assert.Contains("entities: empty — omitted", pack.Omitted);
        Assert.Contains("contracts: empty — omitted", pack.Omitted);
    }

    [Fact]
    public void Multi_pack_drops_empty_cards_and_ships_no_html_markers()
    {
        var (query, snapshot) = Arrange();
        var entryId = snapshot.Entries[0].Node.ToString();

        var pack = new ContextPackBuilder(query, snapshot).BuildMulti(
            [
                new ContextCardSpec("flow", "Flow", [entryId]),
                new ContextCardSpec("entities", "Entities", [entryId]),
            ],
            totalBudget: 4000);

        Assert.Single(pack.Cards);
        Assert.Equal("flow", pack.Cards[0].Type);
        Assert.Contains(pack.Omitted, o => o.StartsWith("entities", StringComparison.Ordinal));
        Assert.DoesNotContain("<!--", pack.AssembledMarkdown, StringComparison.Ordinal);
        Assert.DoesNotContain(", 0 tok_", pack.AssembledMarkdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Multi_pack_propagates_section_omission_reasons()
    {
        // T5.1 (audit R1) — BuildMulti built the per-section omission reasons and threw them
        // away, so the Studio's GetContextPack always reported an empty omitted[] while
        // silently cutting sections. The single-focus form carries the reasons verbatim.
        var (query, snapshot) = Arrange();
        var entryId = snapshot.Entries[0].Node.ToString();

        var pack = new ContextPackBuilder(query, snapshot).BuildMulti(
            [new ContextCardSpec("flow", "Flow", [entryId])],
            totalBudget: 4000);

        Assert.Contains("di_wiring: empty — omitted", pack.Omitted);
        Assert.Contains("entities: empty — omitted", pack.Omitted);
    }

    [Fact]
    public void Bodies_expand_to_full_text_and_mark_truncation_when_cut()
    {
        // T4.2 — the audit's 612/4000 under-fill: bodies fill the remaining budget spine-first.
        // Generous budget → the FULL member body ships (lines beyond the 3-line salient snippet);
        // tight budget → the snippet ships with a visible `… (+N lines)` marker, never a silent cut.
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var callerId = NodeId.ForMember("App.OrdersController", "Post");
        var calleeId = NodeId.ForMember("App.OrderService", "CreateOrder");
        var serviceTypeId = NodeId.ForType("App.OrderService");

        var longBody = "public class OrderService\n{\n    public void CreateOrder(Order o)\n    {\n"
            + string.Join("\n", Enumerable.Range(1, 40).Select(i => $"        var step{i} = DoTheThing{i}(o);"))
            + "\n    }\n}";

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(callerId, "OrdersController.Post", NodeKind.Member));
        g.AddNode(new GraphNode(serviceTypeId, "OrderService", NodeKind.Type) { SourceBody = longBody });
        g.AddNode(new GraphNode(calleeId, "OrderService.CreateOrder", NodeKind.Member));
        g.AddEdge(new GraphEdge(entryId, callerId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls));

        var graph = g.Build();
        var entries = ImmutableArray.Create(
            new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId));
        var builder = new ContextPackBuilder(new GraphQuery(graph, entries), MakeSnapshot(graph, entries));

        var generous = builder.Build("POST /orders", budgetTokens: 8000);
        Assert.Contains("var step10 = DoTheThing10(o);", generous.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("… (+", generous.Content, StringComparison.Ordinal);

        var tight = builder.Build("POST /orders", budgetTokens: 300);
        Assert.DoesNotContain("var step10 = DoTheThing10(o);", tight.Content, StringComparison.Ordinal);
        Assert.Contains("… (+", tight.Content, StringComparison.Ordinal);
        Assert.True(tight.TotalTokens <= 300, $"pack overflows its budget: {tight.TotalTokens}");
    }

    [Fact]
    public void Config_section_lists_spine_keys_with_binding_sites()
    {
        // T4.3 (R9) — the config card was a dead client stub. The section scans the spine's own
        // files for literal key reads and cites each binding site repo-relative.
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "dc-t43-" + Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var svcPath = Path.Combine(dir, "OrderService.cs");
            File.WriteAllText(svcPath,
                "public class OrderService\n{\n    public OrderService(IConfiguration config)\n    {\n"
                + "        var cs = config.GetConnectionString(\"Database\");\n"
                + "        var retries = config[\"Orders:MaxRetries\"];\n    }\n}");

            var g = new CodeGraphBuilder();
            var entryId = NodeId.ForEntry("POST /orders");
            var svcId = NodeId.ForType("App.OrderService");
            g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
            g.AddNode(new GraphNode(svcId, "OrderService", NodeKind.Type) { FilePath = svcPath });
            g.AddEdge(new GraphEdge(entryId, svcId, EdgeKind.Calls));

            var graph = g.Build();
            var entries = ImmutableArray.Create(
                new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId));
            var builder = new ContextPackBuilder(
                new GraphQuery(graph, entries), MakeSnapshot(graph, entries, rootPath: dir));

            var pack = builder.Build("POST /orders");

            var config = pack.Sections.Single(s => s.Section == "config").Content;
            Assert.Contains("`Database` (GetConnectionString) — OrderService.cs:5", config, StringComparison.Ordinal);
            Assert.Contains("`Orders:MaxRetries` (Indexer) — OrderService.cs:6", config, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Tests_section_lists_reaching_tests_and_says_best_effort()
    {
        // T4.3 (R9) — the tests card was a dead client stub. The section lists test methods whose
        // call closure reaches a spine member, and says it is a heuristic (0 rows ≠ untested).
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var calleeId = NodeId.ForMember("App.OrderService", "CreateOrder");
        var testId = NodeId.ForMember("App.Tests.OrderServiceTests", "CreateOrder_saves_the_order");

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(calleeId, "OrderService.CreateOrder", NodeKind.Member));
        g.AddNode(new GraphNode(testId, "OrderServiceTests.CreateOrder_saves_the_order", NodeKind.Member)
        {
            FilePath = @"C:\repo\tests\App.Tests\OrderServiceTests.cs",
            LineNumber = 34,
        });
        g.AddEdge(new GraphEdge(entryId, calleeId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(testId, calleeId, EdgeKind.Calls));

        var graph = g.Build();
        var entries = ImmutableArray.Create(
            new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId));
        var builder = new ContextPackBuilder(new GraphQuery(graph, entries), MakeSnapshot(graph, entries));

        var pack = builder.Build("POST /orders");

        var tests = pack.Sections.Single(s => s.Section == "tests").Content;
        Assert.Contains("`OrderServiceTests.CreateOrder_saves_the_order` — reaches `OrderService.CreateOrder` (distance 1)", tests, StringComparison.Ordinal);
        Assert.Contains("tests/App.Tests/OrderServiceTests.cs:34", tests, StringComparison.Ordinal);
        Assert.Contains("best-effort", tests, StringComparison.Ordinal);
    }

    [Fact]
    public void Sections_carry_provenance_footer_and_structured_tiers()
    {
        // T4.4 (R10) — each section says where it came from (repo-relative file:line set) and
        // how sure it is (verified = Semantic/Join, approx = Syntactic), both in the markdown
        // footer and as structured fields for the UI's provenance chips (T5.3).
        var (query, snapshot) = Arrange();

        var pack = new ContextPackBuilder(query, snapshot).Build("POST /orders");

        var signatures = pack.Sections.Single(s => s.Section == "signatures");
        Assert.Contains("_provenance:", signatures.Content, StringComparison.Ordinal);
        Assert.Contains("verified", signatures.Content, StringComparison.Ordinal);
        Assert.NotEmpty(signatures.SourceLocations);
        Assert.Contains("src/App/OrdersController.cs:11", signatures.SourceLocations);
        Assert.True(signatures.Verified + signatures.Approx > 0, "tier mix is empty");
        // Structured locations are repo-relative like everything else in the pack.
        Assert.DoesNotContain(signatures.SourceLocations, l => l.Contains(":\\") || l.StartsWith("C:/"));
    }

    [Fact]
    public void Multi_pack_header_names_repo_and_fills_archetype()
    {
        var (query, snapshot) = Arrange(
            analyzedAt: new DateTimeOffset(2026, 7, 16, 14, 30, 0, TimeSpan.Zero),
            gitHead: "abc1234def5678900000000000000000000000ff");
        var entryId = snapshot.Entries[0].Node.ToString();

        var pack = new ContextPackBuilder(query, snapshot).BuildMulti(
            [new ContextCardSpec("flow", "Flow", [entryId])], totalBudget: 4000);

        Assert.Contains("# repo — Context Pack", pack.AssembledMarkdown, StringComparison.Ordinal);
        Assert.Contains("HEAD abc1234", pack.AssembledMarkdown, StringComparison.Ordinal);
        // Audit C2's `_Archetype: _`: the archetype comes from the Map now, never the
        // unpopulated snapshot.Explanation — with no Map it says "unknown", not blank.
        Assert.DoesNotContain("_Archetype: _", pack.AssembledMarkdown, StringComparison.Ordinal);
        Assert.Contains("_Archetype: unknown_", pack.AssembledMarkdown, StringComparison.Ordinal);
    }
}
