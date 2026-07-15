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
    public void Blazor_page_routes_are_ui_entries_not_http_endpoints()
    {
        // T1.7 — a Blazor @page route (BlazorEntryExtractor stamps HandlerMethod "<component>" on a
        // .razor file) is an interactive UI entry, NOT an HTTP REST endpoint. Rendering it as
        // HttpEndpoint polluted the "N/M endpoints anonymous" security surface with UI pages. A real
        // API endpoint alongside it must still read as HttpEndpoint.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("WebApp", @"C:\repo\src\WebApp\WebApp.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Detections.Add(new EndpointDetection("GET", "/checkout", "Checkout", "<component>", [], [])
        {
            ExtractorName = "BlazorEntryExtractor", SourceFile = @"C:\repo\src\WebApp\Pages\Checkout.razor", LineNumber = 1,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/orders", "OrdersApi", "GetAsync", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\WebApp\OrdersApi.cs", LineNumber = 12,
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
            Projects = [new ProjectInfo("Catalog.API", @"C:\repo\src\Catalog.API\Catalog.API.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items", "CatalogApi", "GetAllItemsV1", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Catalog.API\CatalogApi.cs", LineNumber = 21,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items", "CatalogApi", "GetAllItems", [], [])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Catalog.API\CatalogApi.cs", LineNumber = 26,
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
        Assert.Contains(catalogItems, e => e.Title.Contains("GetAllItemsV1", StringComparison.Ordinal));
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
    public void NoiseFilter_IsProductionEntrySource_excludes_test_and_sample_sources()
    {
        // Entry points must come from production code only — a library's (or app's) test-project handlers
        // and sample-app handlers/endpoints must not surface as application entry points (MediatR audit:
        // samples/MediatR.Examples + MediatR.Tests handlers were leaking 18 phantom entries).
        var projects = ImmutableArray.Create(
            new ProjectInfo("Orders.Core", @"C:\repo\src\Orders.Core\Orders.Core.csproj", "C#", ["net10.0"], [], []),
            new ProjectInfo("Orders.Tests", @"C:\repo\test\Orders.Tests\Orders.Tests.csproj", "C#", ["net10.0"], [], []));
        var noise = new NoiseFilter(new ProjectClassifier(projects));

        Assert.True(noise.IsProductionEntrySource(@"C:\repo\src\Orders.Core\Endpoints\OrderEndpoints.cs"));
        Assert.False(noise.IsProductionEntrySource(@"C:\repo\test\Orders.Tests\PingHandler.cs"));
        Assert.False(noise.IsProductionEntrySource(@"C:\repo\samples\Demo\PingHandler.cs"));
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

    [Fact]
    public void Entry_target_falls_back_to_owning_controller_for_view_actions()
    {
        // W8: a view-returning controller action (no service call, no MediatR send) gets the owning
        // controller type as its target — honest + more useful than a blank drill-in hint.
        // Ocelot's GET /configuration is the canonical case.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Web", @"C:\repo\src\Web\Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Web.Controllers.FileConfigurationController", new TypeDiscovery
        {
            Id = "Web.Controllers.FileConfigurationController",
            Name = "FileConfigurationController",
            Namespace = "Web.Controllers",
            FilePath = @"C:\repo\src\Web\Controllers\FileConfigurationController.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Presentation,
        });
        model.Detections.Add(new EndpointDetection("GET", "/configuration",
            "FileConfigurationController", "Get", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Web\Controllers\FileConfigurationController.cs",
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
            Projects = [new ProjectInfo("Catalog.Api", @"C:\repo\src\Catalog.Api\Catalog.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Catalog.Api.CatalogApi", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogApi", Name = "CatalogApi",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
        });
        model.Types.TryAdd("Catalog.Api.CatalogContext", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogContext", Name = "CatalogContext",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogContext.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Types.TryAdd("Catalog.Api.CatalogItem", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogItem", Name = "CatalogItem",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogItem.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Detections.Add(new EfEntityDetection("CatalogItem", "CatalogContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Catalog.Api\CatalogItem.cs", LineNumber = 5,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items/{id}/pic",
            "CatalogApi", "GetItemPictureById", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Catalog.Api\CatalogApi.cs",
            LineNumber = 46,
        });
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "GetItemPictureById",
            "Catalog.Api.CatalogContext", "FindAsync",
            @"C:\repo\src\Catalog.Api\CatalogApi.cs:50"));

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
            Projects = [new ProjectInfo("Catalog.Api", @"C:\repo\src\Catalog.Api\Catalog.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Catalog.Api.CatalogApi", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogApi", Name = "CatalogApi",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
        });
        model.Types.TryAdd("Catalog.Api.CatalogContext", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogContext", Name = "CatalogContext",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogContext.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Types.TryAdd("Catalog.Api.CatalogItem", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogItem", Name = "CatalogItem",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogItem.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Catalog.Api.ImageStore", new TypeDiscovery
        {
            Id = "Catalog.Api.ImageStore", Name = "ImageStore",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\ImageStore.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Detections.Add(new EfEntityDetection("CatalogItem", "CatalogContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Catalog.Api\CatalogItem.cs", LineNumber = 5,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/items/{id}/pic",
            "CatalogApi", "GetItemPictureById", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Catalog.Api\CatalogApi.cs",
            LineNumber = 46,
        });
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "GetItemPictureById",
            "Catalog.Api.CatalogContext", "FindAsync",
            @"C:\repo\src\Catalog.Api\CatalogApi.cs:50"));
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "GetItemPictureById",
            "Catalog.Api.ImageStore", "ReadImageAsync",
            @"C:\repo\src\Catalog.Api\CatalogApi.cs:52"));

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
            Projects = [new ProjectInfo("Catalog.Api", @"C:\repo\src\Catalog.Api\Catalog.Api.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Catalog.Api.CatalogApi", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogApi", Name = "CatalogApi",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogApi.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
        });
        model.Types.TryAdd("Catalog.Api.CatalogContext", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogContext", Name = "CatalogContext",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogContext.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        });
        model.Types.TryAdd("Catalog.Api.CatalogType", new TypeDiscovery
        {
            Id = "Catalog.Api.CatalogType", Name = "CatalogType",
            Namespace = "Catalog.Api", FilePath = @"C:\repo\src\Catalog.Api\CatalogType.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Detections.Add(new EfEntityDetection("CatalogType", "CatalogContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\Catalog.Api\CatalogType.cs", LineNumber = 5,
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/catalog/catalogtypes",
            "CatalogApi", "<lambda>", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Catalog.Api\CatalogApi.cs",
            LineNumber = 77,
            HandlerBody = "async (CatalogContext context) => await context.CatalogTypes.OrderBy(x => x.Type).ToListAsync()",
        });
        model.CallEdges.Add(new CallEdge(
            "Catalog.Api.CatalogApi", "<lambda> GET /api/catalog/catalogtypes",
            "Catalog.Api.CatalogContext", "ToListAsync",
            @"C:\repo\src\Catalog.Api\CatalogApi.cs:77"));

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
            Projects = [new ProjectInfo("Orders.Domain", @"C:\repo\src\Orders.Domain\Orders.Domain.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Domain.Order", new TypeDiscovery
        {
            Id = "Orders.Domain.Order",
            Name = "Order",
            Namespace = "Orders.Domain",
            FilePath = @"C:\repo\src\Orders.Domain\Order.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("Orders.Domain.OrderItem", new TypeDiscovery
        {
            Id = "Orders.Domain.OrderItem",
            Name = "OrderItem",
            Namespace = "Orders.Domain",
            FilePath = @"C:\repo\src\Orders.Domain\OrderItem.cs",
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
            SourceFile = @"C:\repo\src\Orders.Domain\Order.cs",
            LineNumber = 5,
        });
        model.Detections.Add(new EfEntityDetection("OrderItem", "OrdersDbContext", false, ["Id"])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Orders.Domain\OrderItem.cs",
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
            Projects = [new ProjectInfo("Orders.Domain", @"C:\repo\src\Orders.Domain\Orders.Domain.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Orders.Domain.Order", new TypeDiscovery
        {
            Id = "Orders.Domain.Order",
            Name = "Order",
            Namespace = "Orders.Domain",
            FilePath = @"C:\repo\src\Orders.Domain\Order.cs",
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
            FilePath = @"C:\repo\src\Orders.Domain\OrderItem.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Detections.Add(new EfEntityDetection("Order", "OrdersDbContext", true, ["Id"])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Orders.Domain\Order.cs",
            LineNumber = 5,
        });
        model.Detections.Add(new EfEntityDetection("OrderItem", "OrdersDbContext", false, ["Id"])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Orders.Domain\OrderItem.cs",
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
            Projects = [new ProjectInfo("Web", @"C:\repo\src\Web\Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Detections.Add(new EndpointDetection("GET", "/products", "?", "<lambda>", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Web\Program.cs",
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
            Projects = [new ProjectInfo("Web", @"C:\repo\src\Web\Web.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Types.TryAdd("Web.Controllers.FileConfigurationController", new TypeDiscovery
        {
            Id = "Web.Controllers.FileConfigurationController",
            Name = "FileConfigurationController",
            Namespace = "Web.Controllers",
            FilePath = @"C:\repo\src\Web\Controllers\FileConfigurationController.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Presentation,
        });
        model.Detections.Add(new EndpointDetection("GET", "/configuration",
            "FileConfigurationController", "Get", [], [])
        {
            ExtractorName = "test",
            SourceFile = @"C:\repo\src\Web\Controllers\FileConfigurationController.cs",
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
