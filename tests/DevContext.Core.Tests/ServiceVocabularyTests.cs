using DevContext.Core.Extractors.Generic;
using DevContext.Core.Graph;
using DevContext.Core.Graph2;

namespace DevContext.Core.Tests;

/// <summary>
/// R3 D-4 (G6.1) — ONE VOCABULARY FOR "SERVICE".
///
/// <para>The Atlas page shows the same repo three ways: an Architecture canvas fed by the ServiceMap
/// facet, a per-service breakdown fed by <see cref="ArchitectureStyleDetector.DetectPerServiceStyles"/>,
/// and a hub radar. The first two used to answer "which projects are services?" with two independent
/// predicates: the canvas asked <see cref="ServiceBoundaryInference.RunnableProjects"/> (production AND
/// runnable), while the breakdown asked <c>IsRunnableService</c> plus a second skip list of its own —
/// including an infrastructure filter keyed on the project NAME containing "shared"/"common"/
/// ".eventbus", and a test filter keyed on the FILE PATH rather than on the project.</para>
///
/// <para>On eShop the two happened to agree (12 = 12), which is exactly why this was invisible. These
/// tests pin the invariant instead of the coincidence: the per-service rows ARE the runnable-production
/// set, no more and no less.</para>
/// </summary>
public sealed class ServiceVocabularyTests
{
    private const string Root = @"C:\repo\src";

    private static PackageReferenceInfo Pkg(string name) => new(name, "1.0.0");

    private static ProjectInfo Proj(string name, string relDir, string? outputType,
        params PackageReferenceInfo[] pkgs)
        => new(name, $@"{Root}\{relDir}\{name}.csproj", "C#", ["net10.0"], [], [.. pkgs], outputType);

    private static string[] PerServiceRows(DiscoveryModel model)
        => [.. ArchitectureStyleDetector.DetectPerServiceStyles(model)
            .Select(s => s.ProjectName).OrderBy(n => n, StringComparer.Ordinal)];

    private static string[] RunnableSet(DiscoveryModel model)
        => [.. ServiceBoundaryInference
            .RunnableProjects(SolutionScope.FromModel(model), model.SamplesAreTheProduct)
            .Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal)];

    /// <summary>The load-bearing one. RED before G6.1: `Shop.Common` is a runnable production project,
    /// so the canvas drew it, but the breakdown dropped it because its NAME contains "common".</summary>
    [Fact]
    public void Per_service_rows_are_exactly_the_runnable_production_set()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                Proj("Shop.Api", "Shop.Api", "Exe", Pkg("Microsoft.AspNetCore.App")),
                Proj("Shop.Worker", "Shop.Worker", "Exe"),
                // A runnable host whose NAME merely contains an infrastructure word. The canvas has
                // always drawn it; the breakdown used to silently drop it.
                Proj("Shop.Common", "Shop.Common", "Exe", Pkg("Microsoft.AspNetCore.App")),
                Proj("Shop.SharedHost", "Shop.SharedHost", "Exe"),
                Proj("Shop.EventBus", "Shop.EventBus", "Exe"),
                // Not runnable — a library is not a service on either surface.
                Proj("Shop.Domain", "Shop.Domain", null),
            ],
        };

        Assert.Equal(RunnableSet(model), PerServiceRows(model));
        Assert.Contains("Shop.Common", PerServiceRows(model));
        Assert.DoesNotContain("Shop.Domain", PerServiceRows(model));
    }

    /// <summary>The exclusions that ARE real live in the one predicate, so both surfaces inherit them.
    /// A test host, a benchmark harness and a sample app are not services anywhere.</summary>
    [Fact]
    public void Non_production_runnables_are_absent_from_both_surfaces()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                Proj("Shop.Api", "Shop.Api", "Exe", Pkg("Microsoft.AspNetCore.App")),
                Proj("Shop.FunctionalTests", "Shop.FunctionalTests", "Exe", Pkg("xunit"), Pkg("Microsoft.AspNetCore.App")),
                Proj("Shop.Benchmarks", "Shop.Benchmarks", "Exe", Pkg("BenchmarkDotNet")),
                new ProjectInfo("Shop.Sample", @"C:\repo\samples\Shop.Sample\Shop.Sample.csproj",
                    "C#", ["net10.0"], [], [], "Exe"),
            ],
        };

        Assert.Equal(RunnableSet(model), PerServiceRows(model));
        Assert.Equal(["Shop.Api"], PerServiceRows(model));
    }

    /// <summary>The Aspire AppHost is a member of the set — with the orchestrator's style, not a
    /// membership exemption. It was the one project the old code let through the infrastructure
    /// filter by early-returning above it.</summary>
    [Fact]
    public void The_apphost_is_a_service_with_the_orchestrator_style()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                Proj("Shop.AppHost", "Shop.AppHost", "Exe"),
                Proj("Shop.Api", "Shop.Api", "Exe", Pkg("Microsoft.AspNetCore.App")),
            ],
        };

        Assert.Equal(RunnableSet(model), PerServiceRows(model));
        var appHost = ArchitectureStyleDetector.DetectPerServiceStyles(model)
            .Single(s => s.ProjectName == "Shop.AppHost");
        Assert.Equal("Aspire AppHost", appHost.Style);
    }

    /// <summary>T8 shape: in a samples-only repo the sample hosts ARE the product, and both surfaces
    /// must say so together — the flag travels through the one predicate.</summary>
    [Fact]
    public void Samples_are_the_product_moves_both_surfaces_together()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Sample.Api", @"C:\repo\samples\Sample.Api\Sample.Api.csproj",
                    "C#", ["net10.0"], [], [Pkg("Microsoft.AspNetCore.App")], "Exe"),
            ],
            SamplesAreTheProduct = true,
        };

        Assert.Equal(RunnableSet(model), PerServiceRows(model));
        Assert.Equal(["Sample.Api"], PerServiceRows(model));
    }

    // ── G6.3 — the boundary the scope pick draws, and what it costs ────────────────────────────
    //
    // G6.1 was right and incomplete. Making the breakdown obey `RunnableProjects(scope, …)` also made
    // it obey the SOLUTION scope, and a repo that declares several solutions is analysed one at a
    // time. dotnet-podcasts keeps its two MAUI clients in sibling solutions: the engine parses their
    // csprojs (the mobile TFM triple is read from there) but the analysed solution does not list
    // them — so the only surface that had ever named them, the per-service rollup, went silent and
    // the ratcheted `maui-present` eval expectation went red.
    //
    // The fix is NOT to let non-services back into the service list. It is to give the boundary a
    // name: same style detector, a separate population, and no surface calls them services.

    /// <summary>A MAUI client in a sibling solution is NOT a service — and is NOT invisible either.
    /// RED before G6.3 on both halves: the app was absent from every list the engine produced.</summary>
    [Fact]
    public void A_runnable_app_outside_the_analysed_solution_is_named_but_never_called_a_service()
    {
        var model = MultiSolutionRepo();

        // Half 1 — the service vocabulary is unchanged: services are the analysed solution's
        // runnable production projects, and the mobile client is not one of them.
        Assert.Equal(RunnableSet(model), PerServiceRows(model));
        Assert.Equal(["Shop.Api"], PerServiceRows(model));

        // Half 2 — and the app is still named, with the style the same detector gives it.
        var outside = ArchitectureStyleDetector.DetectOutsideScopeApps(model);
        var mobile = Assert.Single(outside);
        Assert.Equal("Shop.Mobile", mobile.ProjectName);
        Assert.Equal("MAUI App", mobile.Style);
        Assert.Contains(".NET MAUI", mobile.Stack);
    }

    /// <summary>The two lists are disjoint, and the outside list obeys the SAME exclusions — a test
    /// host in a sibling solution is not an app you were "missing".</summary>
    [Fact]
    public void The_two_populations_never_overlap_and_share_one_production_rule()
    {
        var model = MultiSolutionRepo();
        var services = PerServiceRows(model);
        var outside = ArchitectureStyleDetector.DetectOutsideScopeApps(model)
            .Select(s => s.ProjectName).ToArray();

        Assert.Empty(services.Intersect(outside, StringComparer.OrdinalIgnoreCase));
        // Shop.Mobile.Tests is runnable and out of scope — but it is a test project, so it is neither.
        Assert.DoesNotContain("Shop.Mobile.Tests", outside);
        Assert.DoesNotContain("Shop.Mobile.Tests", services);
        // A library outside the solution is not an app.
        Assert.DoesNotContain("Shop.Mobile.Shared", outside);
    }

    /// <summary>A single-solution repo has no boundary to report — the list stays empty rather than
    /// growing a section that says "nothing".</summary>
    [Fact]
    public void A_repo_whose_solution_holds_everything_reports_no_outside_apps()
    {
        var model = new DiscoveryModel
        {
            Projects = [Proj("Shop.Api", "Shop.Api", "Exe", Pkg("Microsoft.AspNetCore.App"))],
            Solution = new SolutionInfo(@"C:\repo\Shop.sln", "Shop", [@"src\Shop.Api\Shop.Api.csproj"]),
        };

        Assert.Equal(["Shop.Api"], PerServiceRows(model));
        Assert.Empty(ArchitectureStyleDetector.DetectOutsideScopeApps(model));
    }

    /// <summary>dotnet-podcasts in miniature: one resolved solution holding the web service, a mobile
    /// client + its library + its test host discovered on disk but listed in a sibling solution.</summary>
    private static DiscoveryModel MultiSolutionRepo() => new()
    {
        Projects =
        [
            Proj("Shop.Api", "Shop.Api", "Exe", Pkg("Microsoft.AspNetCore.App")),
            // The mobile triple is the MAUI evidence — package-free, exactly like podcasts' clients.
            new ProjectInfo("Shop.Mobile", $@"{Root}\Shop.Mobile\Shop.Mobile.csproj", "C#",
                ["net10.0-android", "net10.0-ios", "net10.0-maccatalyst"], [], [], "Exe"),
            new ProjectInfo("Shop.Mobile.Shared", $@"{Root}\Shop.Mobile.Shared\Shop.Mobile.Shared.csproj",
                "C#", ["net10.0-android"], [], [], null),
            new ProjectInfo("Shop.Mobile.Tests", $@"{Root}\Shop.Mobile.Tests\Shop.Mobile.Tests.csproj",
                "C#", ["net10.0"], [], [Pkg("xunit")], "Exe"),
        ],
        // The analysed solution lists ONE of the four.
        Solution = new SolutionInfo(@"C:\repo\Shop.sln", "Shop", [@"src\Shop.Api\Shop.Api.csproj"]),
    };
}
