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
        => new(name, $@"C:/repo/src/{name}/{name}.csproj", "C#", ["net10.0"], [.. refs], [], "Exe");

    [Fact]
    public void ProjectClassifier_separates_production_from_test_benchmark_sample()
    {
        var prod = Prod("Ordering.API");
        var testByName = new ProjectInfo("Ordering.UnitTests", @"C:/repo/test/Ordering.UnitTests/Ordering.UnitTests.csproj",
            "C#", ["net10.0"], [], []);
        var testByPkg = new ProjectInfo("Some.Verifications", @"C:/repo/src/Some.Verifications/Some.Verifications.csproj",
            "C#", ["net10.0"], [], [Pkg("xunit")]);
        var bench = new ProjectInfo("MediatR.Benchmarks", @"C:/repo/benchmarks/MediatR.Benchmarks/MediatR.Benchmarks.csproj",
            "C#", ["net10.0"], [], [Pkg("BenchmarkDotNet")]);
        var sample = new ProjectInfo("MediatR.Examples", @"C:/repo/samples/MediatR.Examples/MediatR.Examples.csproj",
            "C#", ["net10.0"], [], []);

        Assert.True(ProjectClassifier.IsProductionProject(prod));
        Assert.False(ProjectClassifier.IsProductionProject(testByName));
        Assert.False(ProjectClassifier.IsProductionProject(testByPkg));
        Assert.False(ProjectClassifier.IsProductionProject(bench));
        Assert.False(ProjectClassifier.IsProductionProject(sample));   // classified by its /samples/ dir
    }

    [Fact]
    public void ProjectClassifier_excludes_holders_build_tooling_and_toys()
    {
        // D1.1b (audit A2/A3/E2) — the StackExchange.Redis + GitVersion shapes:
        // NoTargets/Traversal SDK holders, Cake/Nuke build exes, and toys/ aux hosts
        // are never production, so they leave topology, per-service rows, and archetype evidence.
        var holderHub = new ProjectInfo(".github", @"C:/repo/.github/.github.csproj",
            "C#", ["net6.0"], [], [], Sdk: "Microsoft.Build.NoTargets/3.3.0");
        var traversal = new ProjectInfo("Build", @"C:/repo/Build.csproj",
            "C#", [], [], [], OutputType: "Exe", Sdk: "Microsoft.Build.Traversal/3.0.2");
        var cakeBuild = new ProjectInfo("docker", @"C:/repo/build/docker/docker.csproj",
            "C#", ["net10.0"], [], [Pkg("Cake.Http"), Pkg("Cake.Json")], OutputType: "Exe");
        var nukeBuild = new ProjectInfo("build", @"C:/repo/build/build.csproj",
            "C#", ["net10.0"], [], [Pkg("Nuke.Common")], OutputType: "Exe");
        var toyHost = new ProjectInfo("KestrelRedisServer", @"C:/repo/toys/KestrelRedisServer/KestrelRedisServer.csproj",
            "C#", ["net10.0"], [], [], OutputType: "Exe");

        Assert.True(ProjectClassifier.IsHolderProject(holderHub));
        Assert.True(ProjectClassifier.IsHolderProject(traversal));
        Assert.True(ProjectClassifier.IsBuildToolingProject(cakeBuild));
        Assert.True(ProjectClassifier.IsBuildToolingProject(nukeBuild));
        Assert.True(ProjectClassifier.IsSamplePath(toyHost.FilePath));
        Assert.False(ProjectClassifier.IsProductionProject(holderHub));
        Assert.False(ProjectClassifier.IsProductionProject(traversal));
        Assert.False(ProjectClassifier.IsProductionProject(cakeBuild));
        Assert.False(ProjectClassifier.IsProductionProject(nukeBuild));
        Assert.False(ProjectClassifier.IsProductionProject(toyHost));
        // toys stay production in a samples-only repo (SamplesAreTheProduct waiver), holders never do.
        Assert.True(ProjectClassifier.IsProductionProject(toyHost, samplesAreTheProduct: true));
        Assert.False(ProjectClassifier.IsProductionProject(holderHub, samplesAreTheProduct: true));
    }

    [Fact]
    public void BuildTooling_closes_over_project_references()
    {
        // GitVersion shape: artifacts/publish/release are Cake Frosting exes that reference only
        // build/common — the project that holds the Cake packages. The closure must catch them.
        var common = new ProjectInfo("common", @"C:/repo/build/common/common.csproj",
            "C#", ["net10.0"], [], [Pkg("Cake.Coverlet")]);
        var artifacts = new ProjectInfo("artifacts", @"C:/repo/build/artifacts/artifacts.csproj",
            "C#", ["net10.0"], [@"../common/common.csproj"], [], OutputType: "Exe");
        var app = Prod("GitVersion.App");
        var classifier = new ProjectClassifier([common, artifacts, app]);

        Assert.True(classifier.IsBuildTooling(common));
        Assert.True(classifier.IsBuildTooling(artifacts));   // via the reference, no direct marker
        Assert.False(classifier.IsBuildTooling(app));
        Assert.False(classifier.IsProduction(artifacts, samplesAreTheProduct: false));
        Assert.True(classifier.IsProduction(app, samplesAreTheProduct: false));
    }

    [Fact]
    public void Archetype_library_when_only_blockers_are_holders_and_build_tooling()
    {
        // SE.Redis shape: the lib + toys/ exes + a root Traversal Build.csproj "Exe". Before D1.1b the
        // Traversal exe (no lib reference) blocked the Library verdict.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("StackExchange.Redis", @"C:/repo/src/StackExchange.Redis/StackExchange.Redis.csproj",
                    "C#", ["net10.0"], [], [], IsPackable: true),
                new ProjectInfo("Build", @"C:/repo/Build.csproj",
                    "C#", [], [], [], OutputType: "Exe", Sdk: "Microsoft.Build.Traversal/3.0.2"),
                new ProjectInfo("TestConsole", @"C:/repo/toys/TestConsole/TestConsole.csproj",
                    "C#", ["net10.0"], [], [], OutputType: "Exe"),
            ],
        };
        model.Types.TryAdd("StackExchange.Redis.ConnectionMultiplexer",
            PublicTypeAt("StackExchange.Redis.ConnectionMultiplexer", @"C:/repo/src/StackExchange.Redis/ConnectionMultiplexer.cs"));

        Assert.Equal(Archetype.Library, ArchetypeDetector.Detect(model, []));
    }

    private static TypeDiscovery PublicTypeAt(string id, string file) => new()
    {
        Id = id, Name = id, Namespace = "Lib", FilePath = file,
        Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
        Layer = ArchitectureLayer.Application,
    };

    [Fact]
    public void MostDepended_names_the_library_not_its_samples()
    {
        // MediatR-shaped: the library, a sample that references it, and several consumers referencing BOTH.
        // Without the fix the sample (referenced by every consumer) out-ranks the library; with it, the
        // sample can never be the answer, so the real library surfaces.
        var lib = Prod("MediatR");
        var sample = new ProjectInfo("MediatR.Examples", @"C:/repo/samples/MediatR.Examples/MediatR.Examples.csproj",
            "C#", ["net10.0"], [@"C:/repo/src/MediatR/MediatR.csproj"], []);
        var model = new DiscoveryModel
        {
            Projects =
            [
                lib, sample,
                Prod("ConsumerA", @"C:/repo/src/MediatR/MediatR.csproj", @"C:/repo/samples/MediatR.Examples/MediatR.Examples.csproj"),
                Prod("ConsumerB", @"C:/repo/src/MediatR/MediatR.csproj", @"C:/repo/samples/MediatR.Examples/MediatR.Examples.csproj"),
                Prod("ConsumerC", @"C:/repo/src/MediatR/MediatR.csproj", @"C:/repo/samples/MediatR.Examples/MediatR.Examples.csproj"),
            ],
        };

        var graph = new CodeGraphBuilder().Build();
        var insight = new TopologyChokepointSource().Compute(model, graph, []).Single();

        Assert.Contains("MediatR (", insight.Title);
        Assert.DoesNotContain("Examples", insight.Title);
    }
}
