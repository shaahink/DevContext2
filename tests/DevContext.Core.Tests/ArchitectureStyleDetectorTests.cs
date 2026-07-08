namespace DevContext.Core.Tests;

public sealed class ArchitectureStyleDetectorTests
{
    private static ProjectInfo Project(string name) =>
        new(name, $"{name}.csproj", "C#", [], [], []);

    [Fact]
    public void ControllerBased_when_controllers_stronger_than_minimal_apis()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Controllers, 0.9f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Projects = [Project("WebApp")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.ControllerBased, style);
    }

    [Fact]
    public void MinimalApi_when_no_controllers_signal()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Projects = [Project("WebApp")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.MinimalApi, style);
    }

    [Fact]
    public void NLayer_when_efcore_and_multi_project()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects = [Project("Web"), Project("Core"), Project("Infrastructure")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.NLayer, style);
    }

    [Fact]
    public void CleanArchitecture_when_mediatr_and_named_layers()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Projects = [
            Project("MyApp.Domain"),
            Project("MyApp.Application"),
            Project("MyApp.Infrastructure")
        ];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.CleanArchitecture, style);
    }

    [Fact]
    public void TodoApi_shape_is_MinimalApi_not_CleanArchitecture()
    {
        // Single project, Minimal API signal, no MediatR → MinimalApi
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.9f));
        model.Projects = [Project("TodoApi")];

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.MinimalApi, style);
    }

    [Fact]
    public void VerticalSlice_shape_is_CleanArchitecture_not_MinimalApi()
    {
        // MediatR + FastEndpoints + DDD folder roles + aggregates → CleanArchitecture or VerticalSlices
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.FastEndpoints, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects = [
            Project("CleanArchitecture.Api"),
            Project("CleanArchitecture.Core"),
            Project("CleanArchitecture.Infrastructure")
        ];
        // Add types in DDD folder conventions
        model.Types.TryAdd("CleanArchitecture.Core.ContributorAggregate.Contributor", new TypeDiscovery
        {
            Id = "CleanArchitecture.Core.ContributorAggregate.Contributor",
            Name = "Contributor",
            Namespace = "CleanArchitecture.Core.ContributorAggregate",
            FilePath = @"C:\repo\src\CleanArchitecture.Core\ContributorAggregate\Contributor.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("CleanArchitecture.UseCases.Contributors.List.Handler", new TypeDiscovery
        {
            Id = "CleanArchitecture.UseCases.Contributors.List.Handler",
            Name = "Handler",
            Namespace = "CleanArchitecture.UseCases.Contributors.List",
            FilePath = @"C:\repo\src\CleanArchitecture.UseCases\Contributors\List\Handler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Detections.Add(new MediatRHandlerDetection("ListContributorsQuery", "Result", "Handler", MediatRKind.Query)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\CleanArchitecture.UseCases\Contributors\List\Handler.cs", LineNumber = 10,
        });
        model.Detections.Add(new EfEntityDetection("Contributor", "AppDbContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\CleanArchitecture.Core\ContributorAggregate\Contributor.cs", LineNumber = 5,
        });

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        // Should be CleanArchitecture or VerticalSlices — NOT MinimalApi
        Assert.NotEqual(ArchitectureStyle.MinimalApi, style);
        Assert.True(style is ArchitectureStyle.CleanArchitecture or ArchitectureStyle.VerticalSlices,
            $"Expected CleanArchitecture or VerticalSlices, got {style} ({via})");
        Assert.True(confidence > 0.5f, $"Confidence {confidence} should be > 0.5");
    }

    [Fact]
    public void MediatR_handlers_in_code_outrank_MinimalApi_without_a_package_signal()
    {
        // eShop's Ordering.API scoped alone: the MediatR package signal isn't lit (package lives in a
        // sibling project), but the handler types are right here. The style must read CleanArchitecture
        // off those handlers, not fall through to MinimalApi (assessment G7). Detections are NOT used —
        // the style detector runs before the Stage-3 MediatR extractor — so this proves the type path.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.9f));
        model.Projects = [Project("Ordering.API")];
        model.Types.TryAdd("Ordering.API.Application.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Ordering.API.Application.CreateOrderCommandHandler",
            Name = "CreateOrderCommandHandler",
            Namespace = "Ordering.API.Application.Commands",
            FilePath = @"C:\repo\src\Ordering.API\Application\Commands\CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            ImplementedInterfaces = ["IRequestHandler<CreateOrderCommand, bool>"],
        });
        model.Types.TryAdd("Ordering.API.Application.OrderPaidDomainEventHandler", new TypeDiscovery
        {
            Id = "Ordering.API.Application.OrderPaidDomainEventHandler",
            Name = "OrderPaidDomainEventHandler",
            Namespace = "Ordering.API.Application.DomainEventHandlers",
            FilePath = @"C:\repo\src\Ordering.API\Application\DomainEventHandlers\OrderPaidDomainEventHandler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            ImplementedInterfaces = ["INotificationHandler<OrderPaidDomainEvent>"],
        });

        var (style, _, via) = ArchitectureStyleDetector.Detect(model);

        Assert.Equal(ArchitectureStyle.CleanArchitecture, style);
        Assert.Contains("domain-event handlers", via);
    }

    [Fact]
    public void EShop_shape_is_microservices_not_MinimalApi()
    {
        // Aspire + many projects → Microservices (with a .sln — real eShop has one)
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        var projectNames = new[] { "Ordering.API", "Ordering.Domain", "Ordering.Application",
            "Ordering.Infrastructure", "Basket.API", "Payment.API", "Catalog.API",
            "EventBus", "ServiceDefaults", "AppHost" };
        model.Projects = [.. projectNames.Select(Project)];
        model.Solution = new SolutionInfo("eShop.slnx", "eShop",
            [.. projectNames.Select(n => $"{n}.csproj")]);
        model.Detections.Add(new MediatRHandlerDetection("CreateOrderCommand", "bool", "CreateOrderCommandHandler", MediatRKind.Command)
        {
            ExtractorName = "test", SourceFile = "Ordering.Application/Commands/CreateOrderCommandHandler.cs", LineNumber = 20,
        });
        model.Detections.Add(new EfEntityDetection("Order", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"Ordering.Domain/AggregatesModel/Order.cs", LineNumber = 5,
        });

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.NotEqual(ArchitectureStyle.MinimalApi, style);
        Assert.True(style is ArchitectureStyle.Microservices or ArchitectureStyle.CleanArchitecture,
            $"Expected Microservices or CleanArchitecture, got {style} ({via})");
    }

    [Fact]
    public void Controllers_with_test_project_not_misread_as_NLayer()
    {
        // DntSite audit: a lone test project inflated projectCount, tripping the NLayer rule at repo-root.
        // Non-test count = 2 ⇒ NLayer (EfCore + >2) must not fire; controllers win.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Controllers, 0.9f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects =
        [
            new ProjectInfo("Web", @"C:\repo\src\Web\Web.csproj", "C#", [], [], []),
            new ProjectInfo("BlazorSsr", @"C:\repo\src\BlazorSsr\BlazorSsr.csproj", "C#", [], [], []),
            new ProjectInfo("App.Tests", @"C:\repo\tests\App.Tests\App.Tests.csproj", "C#", [], [], []),
        ];

        var (style, _, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.ControllerBased, style);
    }

    [Fact]
    public void Partial_closure_suppresses_system_level_microservices()
    {
        // Iteration 4 / Critical 3: pointing at one service of a larger solution analyses a partial
        // closure. Even with Aspire + an AppHost in the slice, the verdict must NOT pronounce the SYSTEM
        // architecture (Microservices) — it states a local style instead.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Projects =
        [
            Project("Catalog.API"), Project("Catalog.Domain"), Project("Catalog.Infrastructure"),
            Project("eShop.AppHost"), Project("eShop.ServiceDefaults"),
        ];
        // The solution declares 24 projects; we analysed only 5 → partial closure (5 < 24 * 0.75).
        model.Solution = new SolutionInfo("eShop.slnx", "eShop",
            [.. Enumerable.Range(0, 24).Select(i => $"p{i}.csproj")]);

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.NotEqual(ArchitectureStyle.Microservices, style);
        Assert.NotEqual(ArchitectureStyle.ModularMonolith, style);
    }

    [Fact]
    public void Whole_solution_keeps_microservices_despite_scope_field()
    {
        // Regression guard for the partial-closure suppression: when ~all solution projects are analysed
        // (not a partial closure), the Microservices verdict is preserved.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        var projects = new[]
        {
            "Ordering.API", "Basket.API", "Catalog.API", "Payment.API",
            "Identity.API", "Webhooks.API", "eShop.AppHost", "eShop.ServiceDefaults",
        };
        model.Projects = [.. projects.Select(Project)];
        // Solution declares the same set we analysed → NOT partial → Microservices kept.
        model.Solution = new SolutionInfo("eShop.slnx", "eShop", [.. projects.Select(p => $"{p}.csproj")]);

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.Microservices, style);
    }

    [Fact]
    public void ModularMonolith_detected_from_real_module_segment_naming()
    {
        // E8 positive: a genuine bounded-context naming convention ("Module" as a whole dot-separated
        // name segment) still triggers ModularMonolith.
        var model = new DiscoveryModel();
        model.Projects =
        [
            Project("Ordering.Module"), Project("Catalog.Module"), Project("Shipping.Module"),
        ];

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.ModularMonolith, style);
    }

    [Fact]
    public void ModularMonolith_not_claimed_from_product_name_substring_or_test_projects()
    {
        // E8 negative: DevContext's own self-analysis shape. Every project name contains "context" as a
        // SUBSTRING (the product is called "DevContext") but none of them is a bounded-context module by
        // naming convention, and a *.Tests project must never count towards the module tally even when it
        // does carry a real "Module" segment.
        var model = new DiscoveryModel();
        model.Projects =
        [
            new ProjectInfo("DevContext.Cli", @"C:\repo\src\DevContext.Cli\DevContext.Cli.csproj", "C#", [], [], []),
            new ProjectInfo("DevContext.Core", @"C:\repo\src\DevContext.Core\DevContext.Core.csproj", "C#", [], [], []),
            new ProjectInfo("DevContext.Server", @"C:\repo\src\DevContext.Server\DevContext.Server.csproj", "C#", [], [], []),
            new ProjectInfo("Catalog.Module.Tests", @"C:\repo\tests\Catalog.Module.Tests\Catalog.Module.Tests.csproj", "C#", [], [], []),
        ];

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.NotEqual(ArchitectureStyle.ModularMonolith, style);
    }

    [Fact]
    public void SampleCollection_when_majority_of_projects_are_in_sample_paths()
    {
        // E4 / L7.3: a repo where >50% of non-test projects live under sample/demo paths
        // is a sample collection, not Microservices, even if it has web-signal projects.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.9f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Gateway, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MassTransit, 1.0f));
        model.Projects =
        [
            new ProjectInfo("BlazorApp1", @"C:\repo\samples\BlazorApp1\BlazorApp1.csproj", "C#", [], [], []),
            new ProjectInfo("BlazorApp2", @"C:\repo\samples\BlazorApp2\BlazorApp2.csproj", "C#", [], [], []),
            new ProjectInfo("BlazorApp3", @"C:\repo\samples\BlazorApp3\BlazorApp3.csproj", "C#", [], [], []),
            new ProjectInfo("BlazorApp4", @"C:\repo\samples\BlazorApp4\BlazorApp4.csproj", "C#", [], [], []),
            new ProjectInfo("SharedLib", @"C:\repo\src\Shared\SharedLib.csproj", "C#", [], [], []),
        ];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.SampleCollection, style);
        Assert.True(confidence > 0.7f, $"Confidence {confidence} should be > 0.7");
        Assert.Contains("samples", via);
    }

    [Fact]
    public void SampleCollection_when_no_unifying_solution_and_many_projects()
    {
        // L7.3: a docs/examples repo with no .sln file and >3 projects → SampleCollection
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Controllers, 0.9f));
        model.Projects =
        [
            Project("ExampleA"), Project("ExampleB"), Project("ExampleC"), Project("ExampleD"),
        ];
        model.Solution = null;

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.SampleCollection, style);
    }

    [Fact]
    public void SampleCollection_outranks_Microservices()
    {
        // L7.3 guardrail: SampleCollection MUST never report as Microservices (E4 fix).
        // Even with strong microservices signals, sample-path dominance wins.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Gateway, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MassTransit, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects =
        [
            new ProjectInfo("DemoApi", @"C:\repo\samples\DemoApi\DemoApi.csproj", "C#", [], [], []),
            new ProjectInfo("DemoApi.Domain", @"C:\repo\samples\DemoApi\DemoApi.Domain.csproj", "C#", [], [], []),
            new ProjectInfo("DemoApi.Application", @"C:\repo\samples\DemoApi\DemoApi.Application.csproj", "C#", [], [], []),
            new ProjectInfo("DemoApi.Infrastructure", @"C:\repo\samples\DemoApi\DemoApi.Infrastructure.csproj", "C#", [], [], []),
        ];

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.NotEqual(ArchitectureStyle.Microservices, style);
        Assert.NotEqual(ArchitectureStyle.CleanArchitecture, style);
        Assert.Equal(ArchitectureStyle.SampleCollection, style);
    }

    [Fact]
    public void Non_sample_paths_do_not_trigger_SampleCollection()
    {
        // Regression: a normal microservices repo without sample paths must NOT be flagged.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        model.Projects =
        [
            new ProjectInfo("Ordering.API", @"C:\repo\src\Ordering.API\Ordering.API.csproj", "C#", [], [], []),
            new ProjectInfo("Basket.API", @"C:\repo\src\Basket.API\Basket.API.csproj", "C#", [], [], []),
            new ProjectInfo("AppHost", @"C:\repo\src\AppHost\AppHost.csproj", "C#", [], [], []),
        ];
        model.Solution = new SolutionInfo("eShop.slnx", "eShop",
            ["Ordering.API.csproj", "Basket.API.csproj", "AppHost.csproj"]);

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.NotEqual(ArchitectureStyle.SampleCollection, style);
    }

    [Fact]
    public void SampleCollection_when_many_projects_but_sln_covers_few()
    {
        // L7.4: multi-.sln sample directory (e.g. dotnet/blazor-samples). The resolver
        // walked down to find a single small .sln (1-2 projects) but the pipeline
        // collected 100+ projects from the top-level directory. This mismatch is a
        // strong signal of a multi-sample collection.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Gateway, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.9f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MassTransit, 1.0f));
        var projects = ImmutableArray.CreateBuilder<ProjectInfo>();
        for (int i = 0; i < 30; i++)
            projects.Add(new ProjectInfo($"App{i}", $@"C:\repo\10.0\App{i}\App{i}.csproj", "C#", [], [], []));
        model.Projects = projects.ToImmutable();
        model.Solution = new SolutionInfo("BlazorSample.sln", "BlazorSample",
            ["BlazorSample.csproj"]); // .sln covers only 1 project, but 30 analyzed

        var (style, _, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.SampleCollection, style);
        Assert.Contains("multi-sample", via);
    }

    [Fact]
    public void Normal_repo_with_sln_matching_projects_not_flagged()
    {
        // Regression: a normal repo where the .sln covers all projects must NOT be flagged.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.9f));
        model.Projects = [Project("MyApp"), Project("MyApp.Core"), Project("MyApp.Infra")];
        model.Solution = new SolutionInfo("MyApp.sln", "MyApp",
            ["MyApp.csproj", "MyApp.Core.csproj", "MyApp.Infra.csproj"]);

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.NotEqual(ArchitectureStyle.SampleCollection, style);
    }
}
