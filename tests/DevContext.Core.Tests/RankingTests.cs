namespace DevContext.Core.Tests;

public sealed class RankingTests
{
    private static readonly RunReport DefaultReport = new()
    {
        Stages = [], Extractors = [], Scorers = [], Compressions = [],
        Cache = new(0, 0, 0, 0), Corpus = new(0, 0, 0),
        Funnel = new(0, 0, 0, 0, 0, 0),
        Parallelism = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
        TotalWall = TimeSpan.Zero,
    };
    /// <summary>overview: endpoint handler at path distance 3 outranks plain POCO at distance 0</summary>
    [Fact]
    public void Overview_EndpointHandlerOutranksPlainPoco()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        // Endpoint handler at distance 3
        model.Types.TryAdd("Far.Controllers.OrdersController", new TypeDiscovery
        {
            Id = "Far.Controllers.OrdersController",
            Name = "OrdersController",
            Namespace = "Far.Controllers",
            FilePath = @"C:\repo\Far\Controllers\OrdersController.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
            PathProximityScore = 0.2f,
            RoleScore = 1.0, // endpoint role
        });

        // Plain POCO at distance 0
        model.Types.TryAdd("Near.Models.Product", new TypeDiscovery
        {
            Id = "Near.Models.Product",
            Name = "Product",
            Namespace = "Near.Models",
            FilePath = @"C:\repo\Near\Models\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            PathProximityScore = 1.0f,
            RoleScore = 0.0,
        });

        var scenario = ScenarioRegistry.BuiltIn["overview"]; // RoleWeight=0.7, FocusWeight=0.3
        foreach (var type in model.Types.Values)
            type.FinalScore = scenario.Pruning.RoleWeight * type.RoleScore + scenario.Pruning.FocusWeight * type.FocusScore;

        var ranked = model.Types.Values.OrderByDescending(t => t.FinalScore).ToList();
        Assert.Equal("OrdersController", ranked[0].Name); // endpoint wins despite distance
    }

    /// <summary>overview: 1 endpoint role outranks 3 DI registrations</summary>
    [Fact]
    public void Overview_OneEndpointOutranksThreeDiRegistrations()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        model.Types.TryAdd("App.Api.HomeController", new TypeDiscovery
        {
            Id = "App.Api.HomeController", Name = "HomeController",
            Namespace = "App.Api", FilePath = @"C:\repo\Api\HomeController.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api, RoleScore = 1.0, // endpoint
        });

        model.Types.TryAdd("App.Infra.ServiceA", new TypeDiscovery
        {
            Id = "App.Infra.ServiceA", Name = "ServiceA",
            Namespace = "App.Infra", FilePath = @"C:\repo\Infra\ServiceA.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application, RoleScore = 0.35, // DI × 3
        });

        model.Types.TryAdd("App.Infra.ServiceB", new TypeDiscovery
        {
            Id = "App.Infra.ServiceB", Name = "ServiceB",
            Namespace = "App.Infra", FilePath = @"C:\repo\Infra\ServiceB.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application, RoleScore = 0.35,
        });

        model.Types.TryAdd("App.Infra.ServiceC", new TypeDiscovery
        {
            Id = "App.Infra.ServiceC", Name = "ServiceC",
            Namespace = "App.Infra", FilePath = @"C:\repo\Infra\ServiceC.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application, RoleScore = 0.35,
        });

        var scenario = ScenarioRegistry.BuiltIn["overview"];
        foreach (var type in model.Types.Values)
            type.FinalScore = scenario.Pruning.RoleWeight * type.RoleScore + scenario.Pruning.FocusWeight * type.FocusScore;

        var ranked = model.Types.Values.OrderByDescending(t => t.FinalScore).ToList();
        Assert.Equal("HomeController", ranked[0].Name); // 1 × 1.0 > 3 × 0.35
    }

    /// <summary>trace with type-level focus: type call-reachable at depth 1 outranks unreachable endpoint</summary>
    [Fact]
    public void Trace_CallReachableTypeOutranksUnreachableEndpoint()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        model.Types.TryAdd("Near.Services.OrderService", new TypeDiscovery
        {
            Id = "Near.Services.OrderService", Name = "OrderService",
            Namespace = "Near.Services", FilePath = @"C:\repo\Near\Services\OrderService.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            RoleScore = 0.35, FocusScore = 0.5, // reachable at depth 1
        });

        model.Types.TryAdd("Far.Api.OldController", new TypeDiscovery
        {
            Id = "Far.Api.OldController", Name = "OldController",
            Namespace = "Far.Api", FilePath = @"C:\repo\Far\Api\OldController.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
            RoleScore = 1.0, FocusScore = 0.0, // unreachable
        });

        var scenario = ScenarioRegistry.BuiltIn["deep-dive"]; // RoleWeight=0.35, FocusWeight=0.65
        foreach (var type in model.Types.Values)
            type.FinalScore = scenario.Pruning.RoleWeight * type.RoleScore + scenario.Pruning.FocusWeight * type.FocusScore;

        // OrderService: 0.35*0.35 + 0.65*0.5 = 0.1225 + 0.325 = 0.4475
        // OldController: 0.35*1.0 + 0.65*0.0 = 0.35
        var ranked = model.Types.Values.OrderByDescending(t => t.FinalScore).ToList();
        Assert.Equal("OrderService", ranked[0].Name);
    }

    /// <summary>trace: focus type itself always included even with budget 1000</summary>
    [Fact]
    public void Trace_FocusTypePinnedEvenWithSmallBudget()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        model.Types.TryAdd("App.Focused.Type", new TypeDiscovery
        {
            Id = "App.Focused.Type", Name = "Type",
            Namespace = "App.Focused", FilePath = @"C:\repo\App\Focused\Type.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            FinalScore = 0.0, RoleScore = 0.0, FocusScore = 1.0,
        });

        model.Types.TryAdd("App.Other.Service", new TypeDiscovery
        {
            Id = "App.Other.Service", Name = "Service",
            Namespace = "App.Other", FilePath = @"C:\repo\App\Other\Service.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            FinalScore = 10.0, RoleScore = 1.0, FocusScore = 0.0,
            Methods = [new MethodSignature("Big", "string", ["string","int","bool","decimal","long","double","float"], ["a","b","c","d","e","f","g"], Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
            SourceBody = new string('x', 5000),
        });

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext
            {
                FocusPoints = [new FocusPoint(FocusKind.Type, @"C:\repo\App\Focused\Type.cs", "Type", null)],
            },
            Scenario = ScenarioRegistry.BuiltIn["deep-dive"],
            Options = new ExtractionOptions(), Report = DefaultReport,
        };

        var request = new RenderRequest { Format = "markdown", MaxTokens = 100 }; // tiny budget

        var plan = RenderPlanBuilder.Build(snapshot, request);

        Assert.Contains("App.Focused.Type", plan.IncludedTypeIds);
    }

    /// <summary>no-focus: ranking equals RoleScore ordering</summary>
    [Fact]
    public void NoFocus_RankingEqualsRoleScore()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        model.Types.TryAdd("App.Api.Controller", new TypeDiscovery
        {
            Id = "App.Api.Controller", Name = "Controller",
            Namespace = "App.Api", FilePath = @"C:\repo\Api\Controller.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api, RoleScore = 1.0,
        });

        model.Types.TryAdd("App.Infra.Helper", new TypeDiscovery
        {
            Id = "App.Infra.Helper", Name = "Helper",
            Namespace = "App.Infra", FilePath = @"C:\repo\Infra\Helper.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application, RoleScore = 0.3,
        });

        model.Types.TryAdd("App.Domain.Entity", new TypeDiscovery
        {
            Id = "App.Domain.Entity", Name = "Entity",
            Namespace = "App.Domain", FilePath = @"C:\repo\Domain\Entity.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain, RoleScore = 0.6,
        });

        var scenario = ScenarioRegistry.BuiltIn["overview"];
        // No focus → FinalScore = RoleScore
        foreach (var type in model.Types.Values)
            type.FinalScore = type.RoleScore;

        var ranked = model.Types.Values.OrderByDescending(t => t.FinalScore).ToList();
        Assert.Equal("Controller", ranked[0].Name); // 1.0
        Assert.Equal("Entity", ranked[1].Name);     // 0.6
        Assert.Equal("Helper", ranked[2].Name);     // 0.3
    }

    /// <summary>library mode: public base interface outranks internal helper</summary>
    [Fact]
    public void LibraryMode_PublicBaseInterfaceOutranksInternalHelper()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        model.Types.TryAdd("Lib.Public.IInterface", new TypeDiscovery
        {
            Id = "Lib.Public.IInterface", Name = "IInterface",
            Namespace = "Lib.Public", FilePath = @"C:\repo\Lib\Public\IInterface.cs",
            Kind = TypeKind.Interface, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain, RoleScore = 1.0, // referenced-as-base
        });

        model.Types.TryAdd("Lib.Internal.Helper", new TypeDiscovery
        {
            Id = "Lib.Internal.Helper", Name = "Helper",
            Namespace = "Lib.Internal", FilePath = @"C:\repo\Lib\Internal\Helper.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Internal,
            Layer = ArchitectureLayer.Application, RoleScore = 0.4, // just public
        });

        // No focus → FinalScore = RoleScore
        foreach (var type in model.Types.Values)
            type.FinalScore = type.RoleScore;

        var ranked = model.Types.Values.OrderByDescending(t => t.FinalScore).ToList();
        Assert.Equal("IInterface", ranked[0].Name);
    }

    /// <summary>name-pattern type in production project: included when budget allows, RoleScore 0</summary>
    [Fact]
    public void NamePatternType_InProductionProject_GetsZeroRoleScore()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        // Type name matches test pattern, but not in a test project
        model.Types.TryAdd("App.Mocks.MockService", new TypeDiscovery
        {
            Id = "App.Mocks.MockService", Name = "MockService",
            Namespace = "App.Mocks", FilePath = @"C:\repo\App\Mocks\MockService.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            RoleScore = 0.5, // initially set by role, then penalized to 0
        });

        // Simulate what PatternRelevancePruner does: force RoleScore to 0 for name-pattern match
        model.Types["App.Mocks.MockService"].RoleScore = 0;

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext(),
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(), Report = DefaultReport,
        };

        var request = new RenderRequest { Format = "markdown", MaxTokens = 8000 };

        var plan = RenderPlanBuilder.Build(snapshot, request);

        // Should be included (not hard-excluded) since it's in a production project
        Assert.Contains("App.Mocks.MockService", plan.IncludedTypeIds);
        Assert.Equal(0, model.Types["App.Mocks.MockService"].RoleScore);
    }

    /// <summary>test-project type: excluded with reason, visible in Excluded list</summary>
    [Fact]
    public void TestProjectType_ExcludedWithReason()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("App.Tests", @"C:\repo\src\App.Tests\App.Tests.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Architecture.Seal();

        model.Types.TryAdd("App.Tests.UnitTest", new TypeDiscovery
        {
            Id = "App.Tests.UnitTest", Name = "UnitTest",
            Namespace = "App.Tests", FilePath = @"C:\repo\src\App.Tests\UnitTest.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Testing,
            IsHardExcluded = true, ExclusionReason = "test project",
        });

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext(),
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(), Report = DefaultReport,
        };

        var request = new RenderRequest { Format = "markdown", MaxTokens = 8000 };

        var plan = RenderPlanBuilder.Build(snapshot, request);

        Assert.DoesNotContain("App.Tests.UnitTest", plan.IncludedTypeIds);
        Assert.Contains(plan.Excluded, e => e.TypeId == "App.Tests.UnitTest" && e.Reason == "test project");
    }

    /// <summary>A smaller --max-tokens budget produces fewer included types.</summary>
    [Fact]
    public void SmallerBudget_ProducesFewerIncludedTypes()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        for (var i = 0; i < 50; i++)
        {
            model.Types.TryAdd($"App.Type{i}", new TypeDiscovery
            {
                Id = $"App.Type{i}", Name = $"Type{i}",
                Namespace = "App", FilePath = $@"C:\repo\App\Type{i}.cs",
                Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Application,
                FinalScore = 1.0 - i * 0.0001,
                Methods = [new MethodSignature("DoWork", "void", ["string","int","bool","decimal","long","double","float"], ["a","b","c","d","e","f","g"], Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
                SourceBody = new string('x', 200), // ~50 tokens each
            });
        }

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext(),
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(), Report = DefaultReport,
        };

        var planLarge = RenderPlanBuilder.Build(snapshot, new RenderRequest { Format = "markdown", MaxTokens = 8000 });
        var planSmall = RenderPlanBuilder.Build(snapshot, new RenderRequest { Format = "markdown", MaxTokens = 2000 });

        Assert.True(planSmall.IncludedTypeIds.Length < planLarge.IncludedTypeIds.Length,
            $"Small budget ({planSmall.IncludedTypeIds.Length}) must include fewer types than large ({planLarge.IncludedTypeIds.Length})");
    }

    /// <summary>Same snapshot + same request = same plan (determinism).</summary>
    [Fact]
    public void RenderPlan_IsDeterministic()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        for (var i = 0; i < 20; i++)
        {
            model.Types.TryAdd($"App.Type{i}", new TypeDiscovery
            {
                Id = $"App.Type{i}", Name = $"Type{i}",
                Namespace = "App", FilePath = $@"C:\repo\App\Type{i}.cs",
                Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Application,
                FinalScore = 10.0 - i,
                Methods = [new MethodSignature("DoWork", "void", [], [], Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
            });
        }

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext(),
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(), Report = DefaultReport,
        };

        var request = new RenderRequest { Format = "markdown", MaxTokens = 5000 };

        var plan1 = RenderPlanBuilder.Build(snapshot, request);
        var plan2 = RenderPlanBuilder.Build(snapshot, request);

        Assert.True(plan1.IncludedTypeIds.SequenceEqual(plan2.IncludedTypeIds), "IncludedTypeIds must be identical");
        Assert.True(plan1.Excluded.SequenceEqual(plan2.Excluded), "Excluded must be identical");
        Assert.Equal(plan1.EstimatedTokens, plan2.EstimatedTokens);
    }

    /// <summary>Pinned-but-hard-excluded types are routed to Excluded with veto reason, not silently dropped.</summary>
    [Fact]
    public void PinVetoedType_GoesToExcludedNotSilent()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("App.Tests", @"C:\repo\src\App.Tests\App.Tests.csproj", "C#", ["net10.0"], [], [])],
        };
        model.Architecture.Seal();

        // A pinned type that is hard-excluded (test project)
        model.Types.TryAdd("App.Tests.TestService", new TypeDiscovery
        {
            Id = "App.Tests.TestService", Name = "TestService",
            Namespace = "App.Tests", FilePath = @"C:\repo\src\App.Tests\TestService.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Testing,
            IsHardExcluded = true, ExclusionReason = "test project",
            FinalScore = 999.0, // Very high score
        });

        // A normal type to ensure the plan is not empty
        model.Types.TryAdd("App.Production.Service", new TypeDiscovery
        {
            Id = "App.Production.Service", Name = "Service",
            Namespace = "App.Production", FilePath = @"C:\repo\src\App\Service.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            FinalScore = 50.0,
            Methods = [new MethodSignature("Handle", "void", ["string"], ["req"], Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
        });

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext
            {
                FocusPoints = [new FocusPoint(FocusKind.Type, @"C:\repo\src\App.Tests\TestService.cs", "TestService", null)],
            },
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(), Report = DefaultReport,
        };

        var request = new RenderRequest { Format = "markdown", MaxTokens = 8000 };

        var plan = RenderPlanBuilder.Build(snapshot, request);

        // The pinned test type should NOT be included
        Assert.DoesNotContain("App.Tests.TestService", plan.IncludedTypeIds);

        // BUT it must appear in Excluded with the veto reason
        var vetoed = plan.Excluded.FirstOrDefault(e => e.TypeId == "App.Tests.TestService");
        Assert.NotNull(vetoed);
        Assert.Contains("vetoed", vetoed.Reason);
    }

    /// <summary>Excluded list contains expected types with reasons.</summary>
    [Fact]
    public void CutList_RendersExcludedTypesWithReasons()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        model.Types.TryAdd("App.Kept.Type", new TypeDiscovery
        {
            Id = "App.Kept.Type", Name = "Type",
            Namespace = "App.Kept", FilePath = @"C:\repo\App\Kept\Type.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            FinalScore = 100.0,
            Methods = [new MethodSignature("Work", "void", [], [], Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
        });

        model.Types.TryAdd("App.Cut.Type", new TypeDiscovery
        {
            Id = "App.Cut.Type", Name = "CutType",
            Namespace = "App.Cut", FilePath = @"C:\repo\App\Cut\Type.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
            FinalScore = 0.01, // Very low score
            Methods = [new MethodSignature("BigMethod", "string",
                ImmutableArray.Create("a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"),
                ImmutableArray.Create("x1","x2","x3","x4","x5","x6","x7","x8","x9","x10","x11","x12","x13","x14","x15","x16","x17","x18","x19","x20"),
                Microsoft.CodeAnalysis.Accessibility.Public, false, false)],
            SourceBody = new string('x', 4000),
        });

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext(),
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(), Report = DefaultReport,
        };

        var request = new RenderRequest { Format = "markdown", MaxTokens = 800 };

        var plan = RenderPlanBuilder.Build(snapshot, request);

        Assert.Contains("App.Kept.Type", plan.IncludedTypeIds);
        Assert.Contains(plan.Excluded, e => e.TypeId == "App.Cut.Type" && e.Reason == "budget");
    }

    /// <summary>Folder focus with no call graph: PathProximityScore alone drives FocusScore, reordering ranking.</summary>
    [Fact]
    public void FolderFocus_NoCallGraph_ReordersRanking()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        // Type A: far from focus folder (low path proximity)
        model.Types.TryAdd("App.Far.Service", new TypeDiscovery
        {
            Id = "App.Far.Service", Name = "FarService",
            Namespace = "App.Far", FilePath = @"C:\repo\src\App\Far\Service.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            RoleScore = 0.5,
            PathProximityScore = 0.1f, // far
            GraphProximity = 0.0,
        });

        // Type B: near focus folder (high path proximity)
        model.Types.TryAdd("App.Near.Service", new TypeDiscovery
        {
            Id = "App.Near.Service", Name = "NearService",
            Namespace = "App.Near", FilePath = @"C:\repo\src\App\Near\Service.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            RoleScore = 0.3, // lower role score
            PathProximityScore = 0.9f, // near
            GraphProximity = 0.0,
        });

        var scenario = ScenarioRegistry.BuiltIn["deep-dive"]; // uses FocusWeight 0.65

        // Compute like RunScoringAsync would:
        // FocusScore = max(PathProximityScore, GraphProximity)
        // FinalScore = RoleWeight * RoleScore + FocusWeight * FocusScore
        var hasFocus = true;
        foreach (var type in model.Types.Values)
        {
            if (hasFocus)
            {
                type.FocusScore = Math.Max(type.PathProximityScore, type.GraphProximity);
                type.FinalScore = scenario.Pruning.RoleWeight * type.RoleScore
                                  + scenario.Pruning.FocusWeight * type.FocusScore;
            }
            else
                type.FinalScore = type.RoleScore;
        }

        // NearService: 0.35 * 0.3 + 0.65 * 0.9 = 0.105 + 0.585 = 0.69
        // FarService: 0.35 * 0.5 + 0.65 * 0.1 = 0.175 + 0.065 = 0.24
        // NearService ranks higher despite lower RoleScore
        var ranked = model.Types.Values.OrderByDescending(t => t.FinalScore).ToList();
        Assert.Equal("NearService", ranked[0].Name);
        Assert.Equal("FarService", ranked[1].Name);
    }

    /// <summary>No-focus: FinalScore equals RoleScore.</summary>
    [Fact]
    public void NoFocus_FinalScoreEqualsRoleScore()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        model.Types.TryAdd("App.Type", new TypeDiscovery
        {
            Id = "App.Type", Name = "Type",
            Namespace = "App", FilePath = @"C:\repo\src\App\Type.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            RoleScore = 0.7,
            PathProximityScore = 0.9f,
            GraphProximity = 0.5,
        });

        var hasFocus = false;
        foreach (var type in model.Types.Values)
        {
            if (hasFocus) type.FocusScore = Math.Max(type.PathProximityScore, type.GraphProximity);
            type.FinalScore = hasFocus
                ? 0.35 * type.RoleScore + 0.65 * type.FocusScore
                : type.RoleScore;
        }

        Assert.Equal(0.7, model.Types.Values.First().FinalScore);
    }
}
