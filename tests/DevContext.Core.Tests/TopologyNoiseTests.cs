using DevContext.Core.Graph;
using DevContext.Core.Insights;

namespace DevContext.Core.Tests;

/// <summary>
/// T1.9 — topology noise. Tests/samples/benchmarks are classified by PROJECT (not path regex) and kept
/// out of the service topology: the service diagram/count (runnable projects), most-depended-upon, and
/// dead-code consume only production projects. A library's samples must not out-rank the library
/// (MediatR.Examples over MediatR), and test projects must not render as service cards.
/// </summary>
public sealed class TopologyNoiseTests
{
    private static PackageReferenceInfo Pkg(string name) => new(name, "1.0.0");

    private static ProjectInfo Prod(string name, params string[] refs)
        => new(name, $@"C:\repo\src\{name}\{name}.csproj", "C#", ["net10.0"], [.. refs], [], "Exe");

    [Fact]
    public void ProjectClassifier_separates_production_from_test_benchmark_sample()
    {
        var prod = Prod("Ordering.API");
        var testByName = new ProjectInfo("Ordering.UnitTests", @"C:\repo\test\Ordering.UnitTests\Ordering.UnitTests.csproj",
            "C#", ["net10.0"], [], []);
        var testByPkg = new ProjectInfo("Some.Verifications", @"C:\repo\src\Some.Verifications\Some.Verifications.csproj",
            "C#", ["net10.0"], [], [Pkg("xunit")]);
        var bench = new ProjectInfo("MediatR.Benchmarks", @"C:\repo\benchmarks\MediatR.Benchmarks\MediatR.Benchmarks.csproj",
            "C#", ["net10.0"], [], [Pkg("BenchmarkDotNet")]);
        var sample = new ProjectInfo("MediatR.Examples", @"C:\repo\samples\MediatR.Examples\MediatR.Examples.csproj",
            "C#", ["net10.0"], [], []);

        Assert.True(ProjectClassifier.IsProductionProject(prod));
        Assert.False(ProjectClassifier.IsProductionProject(testByName));
        Assert.False(ProjectClassifier.IsProductionProject(testByPkg));
        Assert.False(ProjectClassifier.IsProductionProject(bench));
        Assert.False(ProjectClassifier.IsProductionProject(sample));   // classified by its /samples/ dir
    }

    [Fact]
    public void MostDepended_names_the_library_not_its_samples()
    {
        // MediatR-shaped: the library, a sample that references it, and several consumers referencing BOTH.
        // Without the fix the sample (referenced by every consumer) out-ranks the library; with it, the
        // sample can never be the answer, so the real library surfaces.
        var lib = Prod("MediatR");
        var sample = new ProjectInfo("MediatR.Examples", @"C:\repo\samples\MediatR.Examples\MediatR.Examples.csproj",
            "C#", ["net10.0"], [@"C:\repo\src\MediatR\MediatR.csproj"], []);
        var model = new DiscoveryModel
        {
            Projects =
            [
                lib, sample,
                Prod("ConsumerA", @"C:\repo\src\MediatR\MediatR.csproj", @"C:\repo\samples\MediatR.Examples\MediatR.Examples.csproj"),
                Prod("ConsumerB", @"C:\repo\src\MediatR\MediatR.csproj", @"C:\repo\samples\MediatR.Examples\MediatR.Examples.csproj"),
                Prod("ConsumerC", @"C:\repo\src\MediatR\MediatR.csproj", @"C:\repo\samples\MediatR.Examples\MediatR.Examples.csproj"),
            ],
        };

        var graph = new CodeGraphBuilder().Build();
        var insight = new TopologyChokepointSource().Compute(model, graph, []).Single();

        Assert.Contains("MediatR (", insight.Title);
        Assert.DoesNotContain("Examples", insight.Title);
    }
}
