using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class GraphBuilderTests
{
    // ── T2.3 target quality: entry-target resolution over a hand-built graph ──────────────────────
    private static GraphNode Type(NodeId id, string title, params string[] tags) =>
        new(id, title, NodeKind.Type) { FilePath = "o.cs", Tags = [.. tags] };
    private static GraphNode Member(NodeId id, string title) =>
        new(id, title, NodeKind.Member) { FilePath = "o.cs" };

    [Fact]
    public void EntryTarget_mutating_verb_prefers_a_non_getter_service_call()
    {
        // POST /api/system/reset calls Orchestrator.GetAll() (getter, first) AND Orchestrator.Reset() —
        // the mutating verb must resolve to Reset, not the getter (audit A7 / T2.3).
        var b = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /api/system/reset");
        var handlerId = NodeId.ForMember("Api.SystemController", "Reset");
        var orchType = NodeId.ForType("Api.BacktestOrchestrator");
        var getAll = NodeId.ForMember("Api.BacktestOrchestrator", "GetAll");
        var reset = NodeId.ForMember("Api.BacktestOrchestrator", "Reset");
        b.AddNode(new GraphNode(entryId, "POST /api/system/reset", NodeKind.EntryPoint));
        b.AddNode(Member(handlerId, "SystemController.Reset"));
        b.AddNode(Type(orchType, "BacktestOrchestrator", RoleTags.Service));
        b.AddNode(Member(getAll, "BacktestOrchestrator.GetAll"));
        b.AddNode(Member(reset, "BacktestOrchestrator.Reset"));
        b.AddEdge(new GraphEdge(entryId, handlerId, EdgeKind.Calls));
        b.AddEdge(new GraphEdge(handlerId, getAll, EdgeKind.Calls)); // getter first
        b.AddEdge(new GraphEdge(handlerId, reset, EdgeKind.Calls));
        var graph = b.Build();

        var entry = new EntryPoint(EntryPointKind.HttpEndpoint, "POST /api/system/reset", entryId)
        { HttpMethod = "POST", HandlerNode = handlerId };
        Assert.Equal("BacktestOrchestrator.Reset", GraphBuilder.ResolveEntryTarget(graph, entry));
    }

    [Fact]
    public void TypeFocus_trace_groups_by_member_not_a_flat_wall_of_callees()
    {
        // A Type focus (Orchestrator) must open with its OWN members as branches, each expanding to its
        // call spine — not a flat aggregation of every member's callees behind "(N branches omitted)" (T2.4).
        var b = new CodeGraphBuilder();
        var orchType = NodeId.ForType("Demo.Orchestrator");
        var op1 = NodeId.ForMember("Demo.Orchestrator", "Op1");
        var op2 = NodeId.ForMember("Demo.Orchestrator", "Op2");
        var helper1 = NodeId.ForMember("Demo.Helper1", "Run");
        var helper2 = NodeId.ForMember("Demo.Helper2", "Run");
        b.AddNode(new GraphNode(orchType, "Orchestrator", NodeKind.Type) { FilePath = "o.cs" });
        b.AddNode(new GraphNode(op1, "Orchestrator.Op1", NodeKind.Member) { FilePath = "o.cs", LineNumber = 3 });
        b.AddNode(new GraphNode(op2, "Orchestrator.Op2", NodeKind.Member) { FilePath = "o.cs", LineNumber = 4 });
        b.AddNode(new GraphNode(helper1, "Helper1.Run", NodeKind.Member) { FilePath = "h.cs" });
        b.AddNode(new GraphNode(helper2, "Helper2.Run", NodeKind.Member) { FilePath = "h.cs" });
        b.AddEdge(new GraphEdge(op1, helper1, EdgeKind.Calls));
        b.AddEdge(new GraphEdge(op2, helper2, EdgeKind.Calls));
        var graph = b.Build();

        var entry = new EntryPoint(EntryPointKind.PublicApi, "Orchestrator", orchType);
        var trace = new TraceBuilder(graph).Build(entry, new TraceOptions { MaxDepth = 6, MaxFanOut = 12 });

        // The root's branches are the type's members (not the flat Helper callees).
        var childTitles = trace.Root.Children.Select(c => c.Node.Title).OrderBy(t => t).ToArray();
        Assert.Equal(new[] { "Orchestrator.Op1", "Orchestrator.Op2" }, childTitles);
        // Each member expands to its own callee.
        var op1Step = trace.Root.Children.Single(c => c.Node.Title == "Orchestrator.Op1");
        Assert.Contains(op1Step.Children, gc => gc.Node.Title == "Helper1.Run");
    }

    [Fact]
    public void EntryTarget_labels_direct_data_access_when_only_the_dbcontext_is_called()
    {
        // An action whose only meaningful collaborator is a DbContext has no service layer — say so
        // ("direct data access (TradingDbContext)") rather than resolving to nothing (T2.3).
        var b = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /api/data/save");
        var handlerId = NodeId.ForMember("Api.DataController", "Save");
        var dbType = NodeId.ForType("Api.TradingDbContext");
        b.AddNode(new GraphNode(entryId, "POST /api/data/save", NodeKind.EntryPoint));
        b.AddNode(Member(handlerId, "DataController.Save"));
        b.AddNode(Type(dbType, "TradingDbContext", RoleTags.DataStore));
        b.AddEdge(new GraphEdge(entryId, handlerId, EdgeKind.Calls));
        b.AddEdge(new GraphEdge(handlerId, dbType, EdgeKind.Calls));
        var graph = b.Build();

        var entry = new EntryPoint(EntryPointKind.HttpEndpoint, "POST /api/data/save", entryId)
        { HttpMethod = "POST", HandlerNode = handlerId };
        Assert.Equal("direct data access (TradingDbContext)", GraphBuilder.ResolveEntryTarget(graph, entry));
    }

    [Fact]
    public void DiResolve_wired_only_from_a_test_project_is_tagged_test_only()
    {
        // Shamshir: ITradeRepository → SqliteTradeRepository was wired only from InProcessEngineSmokeTests.
        // The Resolves edge must be tagged test-only (rendered "[test-only registration]") so it is not
        // mistaken for the production binding (T2.1). Uses NoiseFilter, not a path regex.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Engine", @"C:/repo/src/Engine/Engine.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Engine.Tests", @"C:/repo/test/Engine.Tests/Engine.Tests.csproj", "C#", ["net10.0"], [], []),
            ],
        };
        model.Types.TryAdd("Engine.ITradeRepository", new TypeDiscovery
        {
            Id = "Engine.ITradeRepository", Name = "ITradeRepository", Namespace = "Engine",
            FilePath = @"C:/repo/src/Engine/ITradeRepository.cs", Kind = TypeKind.Interface,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Engine.SqliteTradeRepository", new TypeDiscovery
        {
            Id = "Engine.SqliteTradeRepository", Name = "SqliteTradeRepository", Namespace = "Engine",
            FilePath = @"C:/repo/src/Engine/SqliteTradeRepository.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Infrastructure,
            ImplementedInterfaces = ["ITradeRepository"],
        });
        model.Detections.Add(new DiRegistrationDetection("ITradeRepository", "SqliteTradeRepository", "Scoped", [])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/test/Engine.Tests/InProcessEngineSmokeTests.cs",
            LineNumber = 89,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var resolves = Assert.Single(graph.AllEdges, e => e.Kind == EdgeKind.Resolves);
        Assert.Contains(RoleTags.TestOnlyDi, resolves.Tags);
    }

    [Fact]
    public void Build_joins_endpoint_and_handler_and_filters_test_code()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Orders.Api", @"C:/repo/src/Orders.Api/Orders.Api.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Orders.Tests", @"C:/repo/src/Orders.Tests/Orders.Tests.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        model.Types.TryAdd("Orders.Api.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommandHandler", Name = "CreateOrderCommandHandler",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("Orders.Tests.OrderHandlerTests", new TypeDiscovery
        {
            Id = "Orders.Tests.OrderHandlerTests", Name = "OrderHandlerTests",
            Namespace = "Orders.Tests", FilePath = @"C:/repo/src/Orders.Tests/OrderHandlerTests.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Testing,
        });

        model.Detections.Add(new EndpointDetection("POST", "/api/orders", "OrdersApi", "CreateOrderAsync", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Orders.Api/OrdersApi.cs", LineNumber = 10,
        });
        model.Detections.Add(new MediatRHandlerDetection("CreateOrderCommand", "bool", "CreateOrderCommandHandler", MediatRKind.Command)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Orders.Api/CreateOrderCommandHandler.cs", LineNumber = 20,
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
    public void Blazor_page_routes_are_ui_entries_not_http_endpoints()
    {
        // T1.7 — a Blazor @page route (BlazorEntryExtractor stamps HandlerMethod "<component>" on a
        // .razor file) is an interactive UI entry, NOT an HTTP REST endpoint. Rendering it as
        // HttpEndpoint polluted the "N/M endpoints anonymous" security surface with UI pages. A real
        // API endpoint alongside it must still read as HttpEndpoint.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("WebApp", @"C:/repo/src/WebApp/WebApp.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Detections.Add(new EndpointDetection("GET", "/checkout", "Checkout", "<component>", [], [])
        {
            ExtractorName = "BlazorEntryExtractor", SourceFile = @"C:/repo/src/WebApp/Pages/Checkout.razor", LineNumber = 1,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/orders", "OrdersApi", "GetAsync", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/WebApp/OrdersApi.cs", LineNumber = 12,
        });

        var scope = SolutionScope.FromModel(model);
        var (_, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var page = entries.Single(e => e.Route == "/checkout");
        Assert.Equal(EntryPointKind.UiEntry, page.Kind);
        Assert.Null(page.HttpMethod);   // a UI page carries no REST verb

        var api = entries.Single(e => e.Route == "/api/orders");
        Assert.Equal(EntryPointKind.HttpEndpoint, api.Kind);
    }

    [Fact]
    public void Same_route_endpoints_are_disambiguated_not_merged()
    {
        // T1.7 — an API-version pair (eShop `GET /items` v1/v2 → GetAllItemsV1 / GetAllItems) shares
        // verb+route but is two real endpoints. They must yield TWO entries with DISTINCT titles and node
        // ids (the deck fired NG0955 dup-keys when they collapsed onto one), disambiguated by the action.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Catalog.API", @"C:/repo/src/Catalog.API/Catalog.API.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items", "CatalogApi", "GetAllItemsV1", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Catalog.API/CatalogApi.cs", LineNumber = 21,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items", "CatalogApi", "GetAllItems", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Catalog.API/CatalogApi.cs", LineNumber = 26,
        });

        var scope = SolutionScope.FromModel(model);
        var (_, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var catalogItems = entries.Where(e => e.Route == "/api/catalog/items").ToList();
        Assert.Equal(2, catalogItems.Count);                              // never merged
        Assert.Equal(2, catalogItems.Select(e => e.Node).Distinct().Count()); // distinct node ids → no dup-key
        Assert.Equal(2, catalogItems.Select(e => e.Title).Distinct().Count()); // distinct titles
        // D5.3 — detections are canonically ordered (file, then line), so the FIRST endpoint in
        // source order (line 21, GetAllItemsV1) keeps the bare route title and the collision
        // (line 26) carries the [action] suffix — deterministically, not by bag arrival order.
        Assert.Contains(catalogItems, e => e.Title.Contains("[GetAllItems]", StringComparison.Ordinal));
        Assert.Equal("GET /api/catalog/items",
            catalogItems.Single(e => e.Provenance!.EndsWith(":21", StringComparison.Ordinal)).Title);
    }

    [Fact]
    public void Http_entries_group_by_route_feature_not_namespace()
    {
        // T1.6 — feature areas for HTTP endpoints come from the route's first meaningful segment
        // (skipping api/version/param), not the shared handler namespace that collapses everything into
        // one "Api (N entries)" module row. Versioned routes fold into the same feature.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("TradingEngine.Web", @"C:/repo/src/TradingEngine.Web/TradingEngine.Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Detections.Add(new EndpointDetection("GET", "/api/runs/{id}", "RunsApi", "GetRun", [], [])
        { ExtractorName = "test", SourceFile = @"C:/repo/src/TradingEngine.Web/Api/RunsApi.cs", LineNumber = 10 });
        model.Detections.Add(new EndpointDetection("POST", "/api/runs", "RunsApi", "CreateRun", [], [])
        { ExtractorName = "test", SourceFile = @"C:/repo/src/TradingEngine.Web/Api/RunsApi.cs", LineNumber = 20 });
        model.Detections.Add(new EndpointDetection("GET", "/api/v2/strategies", "StrategiesApi", "List", [], [])
        { ExtractorName = "test", SourceFile = @"C:/repo/src/TradingEngine.Web/Api/StrategiesApi.cs", LineNumber = 5 });

        var scope = SolutionScope.FromModel(model);
        var (_, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        Assert.Equal(["runs"], entries.Where(e => e.Route!.Contains("/runs")).Select(e => e.GroupPath).Distinct());
        Assert.Equal("strategies", entries.Single(e => e.Route == "/api/v2/strategies").GroupPath); // version segment skipped
    }

    [Fact]
    public void Background_workers_become_entry_points()
    {
        // DntSite audit: 24 DNTScheduler jobs were detected but never surfaced as entries.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Web", @"C:/repo/src/Web/Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Web.Jobs.BackupDatabaseJob", new TypeDiscovery
        {
            Id = "Web.Jobs.BackupDatabaseJob", Name = "BackupDatabaseJob", Namespace = "Web.Jobs",
            FilePath = @"C:/repo/src/Web/Jobs/BackupDatabaseJob.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Infrastructure,
        });
        model.Detections.Add(new BackgroundWorkerDetection("DNTScheduler", "BackupDatabaseJob", BackgroundWorkerKind.HostedService)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Web/SchedulersConfig.cs", LineNumber = 18,
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
            new ProjectInfo("Orders.Core", @"C:/repo/src/Orders.Core/Orders.Core.csproj", "C#", ["net10.0"], [], []));
        var noise = new NoiseFilter(new ProjectClassifier(projects));

        var spec = new TypeDiscovery
        {
            Id = "Orders.Core.Specifications.OrderByIdSpec", Name = "OrderByIdSpec",
            Namespace = "Orders.Core.Specifications",
            FilePath = @"C:/repo/src/Orders.Core/Specifications/OrderByIdSpec.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        };

        Assert.True(noise.IsProductionCode(spec));
    }

    [Fact]
    public void NoiseFilter_IsProductionEntrySource_excludes_test_and_sample_sources()
    {
        // Entry points must come from production code only — a library's (or app's) test-project handlers
        // and sample-app handlers/endpoints must not surface as application entry points (MediatR audit:
        // samples/MediatR.Examples + MediatR.Tests handlers were leaking 18 phantom entries).
        var projects = ImmutableArray.Create(
            new ProjectInfo("Orders.Core", @"C:/repo/src/Orders.Core/Orders.Core.csproj", "C#", ["net10.0"], [], []),
            new ProjectInfo("Orders.Tests", @"C:/repo/test/Orders.Tests/Orders.Tests.csproj", "C#", ["net10.0"], [], []));
        var noise = new NoiseFilter(new ProjectClassifier(projects));

        Assert.True(noise.IsProductionEntrySource(@"C:/repo/src/Orders.Core/Endpoints/OrderEndpoints.cs"));
        Assert.False(noise.IsProductionEntrySource(@"C:/repo/test/Orders.Tests/PingHandler.cs"));
        Assert.False(noise.IsProductionEntrySource(@"C:/repo/samples/Demo/PingHandler.cs"));
    }

    [Fact]
    public void SolutionScope_scopes_to_resolved_solution_projects()
    {
        var model = new DiscoveryModel
        {
            Solution = new SolutionInfo(@"C:/repo/AppA.sln", "AppA", [@"C:/repo/src/AppA/AppA.csproj"]),
            Projects =
            [
                new ProjectInfo("AppA", @"C:/repo/src/AppA/AppA.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("AppB", @"C:/repo/src/AppB/AppB.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        var scope = SolutionScope.FromModel(model);

        Assert.Equal("AppA", scope.SolutionName);
        Assert.True(scope.Contains(@"C:/repo/src/AppA/Service.cs"));   // in the resolved solution
        Assert.False(scope.Contains(@"C:/repo/src/AppB/Service.cs"));  // independent solution → excluded
    }

    [Fact]
    public void B1_event_consumers_adds_events_and_consumes_edges()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:/repo/src/Orders.Api/Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Orders.Api.CreateOrderCommandHandler", Name = "CreateOrderCommandHandler",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("Orders.Api.ValidateBuyerHandler", new TypeDiscovery
        {
            Id = "Orders.Api.ValidateBuyerHandler", Name = "ValidateBuyerHandler",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/ValidateBuyerHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        model.Detections.Add(new MediatRHandlerDetection("OrderStartedDomainEvent", "void", "ValidateBuyerHandler", MediatRKind.Notification)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Orders.Api/ValidateBuyerHandler.cs", LineNumber = 15,
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
            Projects = [new ProjectInfo("Orders.Api", @"C:/repo/src/Orders.Api/Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.IOrderRepository", new TypeDiscovery
        {
            Id = "Orders.Api.IOrderRepository", Name = "IOrderRepository",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/IOrderRepository.cs",
            Kind = TypeKind.Interface, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Orders.Api.OrderRepository", new TypeDiscovery
        {
            Id = "Orders.Api.OrderRepository", Name = "OrderRepository",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/OrderRepository.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
            ImplementedInterfaces = ["IOrderRepository"],
        });

        model.Detections.Add(new DiRegistrationDetection("IOrderRepository", "OrderRepository", "Scoped", [], DiRegistrationShape.DirectBinding)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Orders.Api/Program.cs", LineNumber = 5,
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
    public void C5_multi_host_di_binding_cites_the_focus_hosts_own_registration()
    {
        // bitwarden: Api's trace resolved CurrentContext to bitwarden_license/src/Sso/Startup.cs:46 —
        // an arbitrary (bag-order) pick among N hosts' registrations. All sites must ride the one edge
        // deterministically; the trace cites the focus host's own site, or "[×N hosts]" when it has none.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Api", @"C:/repo/src/Api/Api.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Sso", @"C:/repo/bitwarden_license/src/Sso/Sso.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Notifications", @"C:/repo/src/Notifications/Notifications.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Core", @"C:/repo/src/Core/Core.csproj", "C#", ["net10.0"], [], []),
            ],
        };
        model.Types.TryAdd("Core.ICurrentContext", new TypeDiscovery
        {
            Id = "Core.ICurrentContext", Name = "ICurrentContext", Namespace = "Core",
            FilePath = @"C:/repo/src/Core/ICurrentContext.cs", Kind = TypeKind.Interface,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Core.CurrentContext", new TypeDiscovery
        {
            Id = "Core.CurrentContext", Name = "CurrentContext", Namespace = "Core",
            FilePath = @"C:/repo/src/Core/CurrentContext.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Infrastructure,
            ImplementedInterfaces = ["ICurrentContext"],
        });
        // Same binding registered by TWO hosts. The Sso site sorts FIRST ordinally (bitwarden_license < src),
        // reproducing the audit's wrong cite if determinism alone were the fix.
        model.Detections.Add(new DiRegistrationDetection("ICurrentContext", "CurrentContext", "Scoped", [], DiRegistrationShape.DirectBinding)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/bitwarden_license/src/Sso/Startup.cs", LineNumber = 46,
        });
        model.Detections.Add(new DiRegistrationDetection("ICurrentContext", "CurrentContext", "Scoped", [], DiRegistrationShape.DirectBinding)
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Api/Startup.cs", LineNumber = 120,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var resolves = Assert.Single(graph.AllEdges, e => e.Kind == EdgeKind.Resolves);
        Assert.Equal(2, resolves.RegistrationSites.Length);
        Assert.Equal(@"C:/repo/bitwarden_license/src/Sso/Startup.cs:46", resolves.Provenance); // deterministic first
        Assert.Equal(["Sso", "Api"], resolves.RegistrationProjects.ToArray());

        // Focus host Api: its OWN registration is cited, no host-count annotation.
        var apiTrace = new TraceBuilder(graph).Build(
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /accounts", resolves.From) { Project = "Api" });
        var apiStep = Assert.Single(apiTrace.Root.Children, s => s.Seam == SeamKind.Resolve);
        Assert.Equal(@"C:/repo/src/Api/Startup.cs:120", apiStep.Provenance);
        Assert.Equal(0, apiStep.DiHostCount);

        // Focus host Notifications registers nothing: deterministic first + honest "[×2 hosts]".
        var foreignTrace = new TraceBuilder(graph).Build(
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /hub", resolves.From) { Project = "Notifications" });
        var foreignStep = Assert.Single(foreignTrace.Root.Children, s => s.Seam == SeamKind.Resolve);
        Assert.Equal(@"C:/repo/bitwarden_license/src/Sso/Startup.cs:46", foreignStep.Provenance);
        Assert.Equal(2, foreignStep.DiHostCount);
    }

    [Fact]
    public void B1_aggregate_nodes_tagged()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Api", @"C:/repo/src/Orders.Api/Orders.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Api.Order", new TypeDiscovery
        {
            Id = "Orders.Api.Order", Name = "Order",
            Namespace = "Orders.Api", FilePath = @"C:/repo/src/Orders.Api/Domain/Order.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });

        model.Detections.Add(new EfEntityDetection("Order", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Orders.Api/Domain/Order.cs", LineNumber = 5,
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

    [Fact]
    public void Entry_target_falls_back_to_owning_controller_for_view_actions()
    {
        // W8: a view-returning controller action (no service call, no MediatR send) gets the owning
        // controller type as its target — honest + more useful than a blank drill-in hint.
        // Ocelot's GET /configuration is the canonical case.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Web", @"C:/repo/src/Web/Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Web.Controllers.FileConfigurationController", new TypeDiscovery
        {
            Id = "Web.Controllers.FileConfigurationController",
            Name = "FileConfigurationController",
            Namespace = "Web.Controllers",
            FilePath = @"C:/repo/src/Web/Controllers/FileConfigurationController.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Presentation,
        });
        model.Detections.Add(new EndpointDetection("GET", "/configuration",
            "FileConfigurationController", "Get", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Web/Controllers/FileConfigurationController.cs",
            LineNumber = 23,
        });

        var scope = SolutionScope.FromModel(model);
        var (_, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = Assert.Single(entries);
        Assert.Equal("GET /configuration", entry.Title);
        Assert.NotNull(entry.Target);
        Assert.Equal("FileConfigurationController", entry.Target);
    }

    [Fact]
    public void Entry_target_never_points_at_a_raw_data_access_call()
    {
        // E6: eShop's GetItemPictureById does nothing but `context.CatalogItems.FindAsync(id)` — the
        // resolved target used to be "CatalogContext.FindAsync", the single most-read column pointing at
        // noise. A DataStore-tagged callee must never be chosen as the target; the owning-type fallback
        // (honest, not a wrong specific claim) applies instead.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Catalog.Api", @"C:/repo/src/Catalog.Api/Catalog.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Catalog.Api.CatalogApi", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogApi", Name = "CatalogApi",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
        });
        model.Types.TryAdd("Catalog.Api.CatalogContext", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogContext", Name = "CatalogContext",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogContext.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Types.TryAdd("Catalog.Api.CatalogItem", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogItem", Name = "CatalogItem",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogItem.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Detections.Add(new EfEntityDetection("CatalogItem", "CatalogContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Catalog.Api/CatalogItem.cs", LineNumber = 5,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items/{id}/pic",
            "CatalogApi", "GetItemPictureById", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Catalog.Api/CatalogApi.cs",
            LineNumber = 46,
        });
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "GetItemPictureById",
            "Catalog.Api.CatalogContext", "FindAsync",
            @"C:/repo/src/Catalog.Api/CatalogApi.cs:50"));

        var scope = SolutionScope.FromModel(model);
        var (_, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = Assert.Single(entries);
        Assert.NotEqual("CatalogContext.FindAsync", entry.Target);
    }

    [Fact]
    public void Entry_target_prefers_real_collaborator_over_data_access_noise()
    {
        // E6 positive counterpart: when a handler calls both a DbContext method AND a genuine in-scope
        // collaborator, the collaborator must win — the noise filter shouldn't blank out a real target
        // that happens to sit alongside a data-access call.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Catalog.Api", @"C:/repo/src/Catalog.Api/Catalog.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Catalog.Api.CatalogApi", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogApi", Name = "CatalogApi",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
        });
        model.Types.TryAdd("Catalog.Api.CatalogContext", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogContext", Name = "CatalogContext",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogContext.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Types.TryAdd("Catalog.Api.CatalogItem", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogItem", Name = "CatalogItem",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogItem.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Catalog.Api.ImageStore", new TypeDiscovery
        {
            Id = "Catalog.Api.ImageStore", Name = "ImageStore",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/ImageStore.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Detections.Add(new EfEntityDetection("CatalogItem", "CatalogContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Catalog.Api/CatalogItem.cs", LineNumber = 5,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items/{id}/pic",
            "CatalogApi", "GetItemPictureById", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Catalog.Api/CatalogApi.cs",
            LineNumber = 46,
        });
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "GetItemPictureById",
            "Catalog.Api.CatalogContext", "FindAsync",
            @"C:/repo/src/Catalog.Api/CatalogApi.cs:50"));
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "GetItemPictureById",
            "Catalog.Api.ImageStore", "ReadImageAsync",
            @"C:/repo/src/Catalog.Api/CatalogApi.cs:52"));

        var scope = SolutionScope.FromModel(model);
        var (_, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = Assert.Single(entries);
        Assert.Equal("ImageStore.ReadImageAsync", entry.Target);
    }

    [Fact]
    public void Entry_target_for_lambda_with_only_noise_calls_reports_inline_count()
    {
        // E6: a minimal-API lambda whose only calls are data-access noise (e.g. eShop's
        // /catalogtypes/-shaped inline handlers) has nothing honest to name as "the target" — say so
        // ("inline (N calls)") instead of naming the whole registration type, which reads as a real handler.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Catalog.Api", @"C:/repo/src/Catalog.Api/Catalog.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Catalog.Api.CatalogApi", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogApi", Name = "CatalogApi",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
        });
        model.Types.TryAdd("Catalog.Api.CatalogContext", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogContext", Name = "CatalogContext",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogContext.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Types.TryAdd("Catalog.Api.CatalogType", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogType", Name = "CatalogType",
            Namespace = "Catalog.Api", FilePath = @"C:/repo/src/Catalog.Api/CatalogType.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Detections.Add(new EfEntityDetection("CatalogType", "CatalogContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:/repo/src/Catalog.Api/CatalogType.cs", LineNumber = 5,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/catalogtypes",
            "CatalogApi", "<lambda>", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Catalog.Api/CatalogApi.cs",
            LineNumber = 77,
            HandlerBody = "async (CatalogContext context) => await context.CatalogTypes.OrderBy(x => x.Type).ToListAsync()",
        });
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "<lambda> GET /api/catalog/catalogtypes",
            "Catalog.Api.CatalogContext", "ToListAsync",
            @"C:/repo/src/Catalog.Api/CatalogApi.cs:77"));

        var scope = SolutionScope.FromModel(model);
        var (_, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = Assert.Single(entries);
        // E6 — updated L7.1: PlainCallDetector now identifies the lambda's dependency on
        // CatalogContext (an in-solution data-store type), so the entry target reflects the real
        // service type instead of the generic "inline (N calls)" fallback. This is more honest:
        // the reader sees exactly which DbContext the endpoint touches.
        Assert.Equal("CatalogContext", entry.Target);
    }

    [Fact]
    public void Entity_navigation_property_creates_entity_relation_edge()
    {
        // A-F14: OrderItem has a property of type Order (a known entity), so we create
        // an EntityRelation edge OrderItem → Order (BelongsTo direction).
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Domain", @"C:/repo/src/Orders.Domain/Orders.Domain.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Domain.Order", new TypeDiscovery
        {
            Id = "Orders.Domain.Order",
            Name = "Order",
            Namespace = "Orders.Domain",
            FilePath = @"C:/repo/src/Orders.Domain/Order.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Orders.Domain.OrderItem", new TypeDiscovery
        {
            Id = "Orders.Domain.OrderItem",
            Name = "OrderItem",
            Namespace = "Orders.Domain",
            FilePath = @"C:/repo/src/Orders.Domain/OrderItem.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            Properties =
            [
                new PropertySignature("Order", "Order", Microsoft.CodeAnalysis.Accessibility.Public,
                    false, false, true, true),
            ],
        });
        model.Detections.Add(new EfEntityDetection("Order", "OrdersDbContext", true, ["Id"])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Orders.Domain/Order.cs",
            LineNumber = 5,
        });
        model.Detections.Add(new EfEntityDetection("OrderItem", "OrdersDbContext", false, ["Id"])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Orders.Domain/OrderItem.cs",
            LineNumber = 5,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var orderItemId = NodeId.ForType("Orders.Domain.OrderItem");
        var orderId = NodeId.ForType("Orders.Domain.Order");
        Assert.Contains(graph.OutEdges(orderItemId, EdgeKind.EntityRelation),
            e => e.To == orderId);

        var orderNode = graph.Node(orderId);
        Assert.NotNull(orderNode);
        Assert.Contains("aggregate", orderNode!.Tags);
        Assert.Contains("entity", orderNode.Tags);

        var orderItemNode = graph.Node(orderItemId);
        Assert.NotNull(orderItemNode);
        Assert.Contains("entity", orderItemNode!.Tags);
        Assert.DoesNotContain("aggregate", orderItemNode.Tags);
    }

    [Fact]
    public void Entity_collection_navigation_creates_entity_relation_edge()
    {
        // A-F14: Order has ICollection<OrderItem> → creates EntityRelation OrderItem→Order
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Orders.Domain", @"C:/repo/src/Orders.Domain/Orders.Domain.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Domain.Order", new TypeDiscovery
        {
            Id = "Orders.Domain.Order",
            Name = "Order",
            Namespace = "Orders.Domain",
            FilePath = @"C:/repo/src/Orders.Domain/Order.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            Properties =
            [
                new PropertySignature("Items", "ICollection<OrderItem>",
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false, true, false),
            ],
        });
        model.Types.TryAdd("Orders.Domain.OrderItem", new TypeDiscovery
        {
            Id = "Orders.Domain.OrderItem",
            Name = "OrderItem",
            Namespace = "Orders.Domain",
            FilePath = @"C:/repo/src/Orders.Domain/OrderItem.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Detections.Add(new EfEntityDetection("Order", "OrdersDbContext", true, ["Id"])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Orders.Domain/Order.cs",
            LineNumber = 5,
        });
        model.Detections.Add(new EfEntityDetection("OrderItem", "OrdersDbContext", false, ["Id"])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Orders.Domain/OrderItem.cs",
            LineNumber = 5,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var orderItemId = NodeId.ForType("Orders.Domain.OrderItem");
        var orderId = NodeId.ForType("Orders.Domain.Order");
        Assert.Contains(graph.OutEdges(orderItemId, EdgeKind.EntityRelation),
            e => e.To == orderId);
    }

    [Fact]
    public void Http_entry_point_node_stamps_line_number()
    {
        // MCP blind-drive audit (2026-07-11) bug #2: EntryPoint graph nodes never carried LineNumber,
        // so read_source fell back to the whole file for every entry. HttpEntryPointBuilder is one of
        // 11 builders with the same gap — this pins the fix on the most common (HTTP) case.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Web", @"C:/repo/src/Web/Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Detections.Add(new EndpointDetection("GET", "/products", "?", "<lambda>", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Web/Program.cs",
            LineNumber = 13,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = Assert.Single(entries);
        var node = graph.Node(entry.Node);
        Assert.NotNull(node);
        Assert.Equal(13, node!.LineNumber);
    }

    [Fact]
    public void Flow_carries_resolved_target_matching_enriched_entry()
    {
        // MCP blind-drive audit (2026-07-11) bug #3: top_flows always reported target: null because
        // ComputeFlows ran over the pre-enrichment entries — graph.Flows[i].Entry.Target was frozen
        // before EnrichEntryTargets ran. Pins that graph.Flows carries the SAME enriched Target as the
        // returned entry inventory (used by `entrypoints`, which already worked).
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Web", @"C:/repo/src/Web/Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Web.Controllers.FileConfigurationController", new TypeDiscovery
        {
            Id = "Web.Controllers.FileConfigurationController",
            Name = "FileConfigurationController",
            Namespace = "Web.Controllers",
            FilePath = @"C:/repo/src/Web/Controllers/FileConfigurationController.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Presentation,
        });
        model.Detections.Add(new EndpointDetection("GET", "/configuration",
            "FileConfigurationController", "Get", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:/repo/src/Web/Controllers/FileConfigurationController.cs",
            LineNumber = 23,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = Assert.Single(entries);
        Assert.NotNull(entry.Target);

        var flow = Assert.Single(graph.Flows);
        Assert.Equal(entry.Target, flow.Entry.Target);
    }
}
