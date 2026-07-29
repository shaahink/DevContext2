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
}
