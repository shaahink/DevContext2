using DevContext.Core.Observers;

using Xunit.Abstractions;

namespace DevContext.Core.Tests;

/// <summary>
/// T7.3 — stage-waterfall honesty. On shamshir the observed stages summed to ~25s of a 51s wall:
/// semantic-lite, graph assembly, insights and fingerprinting ran between/after the observed
/// stages, invisible in the report (fingerprinting even ran AFTER the wall clock stopped).
/// Every post-extraction phase must land in a named waterfall row, and the rows must account
/// for (nearly) the whole measured wall.
/// </summary>
public sealed class StageWaterfallTests
{
    private readonly ITestOutputHelper _output;

    public StageWaterfallTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    public async Task Waterfall_names_every_pipeline_phase_and_accounts_for_the_wall()
    {
        var repoPath = FixturePath("tests/fixtures/CompositionApp");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var fs = new RealFileSystem();
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);

        var collector = new RunReportCollector();
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = new ExtractionOptions
            {
                MaxOutputTokens = 8000,
                OutputFormat = OutputFormat.Markdown,
                AllowRoslyn = true,
            },
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new CompositeDiscoveryObserver(collector),
            FileSystem = fs,
            Cache = new AnalysisCache(fs),
            Analysis = new SharedAnalysisContext(),
            Logger = loggerFactory.CreateLogger("Waterfall"),
        };

        var pipeline = TestPipeline.Build(loggerFactory);
        var snapshot = await pipeline.AnalyzeAsync(ctx);
        var report = snapshot.Report;

        var stageNames = report.Stages.Select(s => s.Stage).ToList();
        _output.WriteLine($"stages: {string.Join(", ", stageNames)}");

        // The formerly-invisible phases each land in a named row.
        Assert.Contains("SemanticLite", stageNames);
        Assert.Contains("GraphAssembly", stageNames);
        Assert.Contains("Insights", stageNames);
        Assert.Contains("Snapshot", stageNames);
        // And the pre-existing rows are still there.
        Assert.Contains("DiscoveryAndCacheWarmup", stageNames);
        Assert.Contains("GenericExtraction", stageNames);
        Assert.Contains("SpecificExtraction", stageNames);
        Assert.Contains("Compression", stageNames);

        // Coverage: named rows account for (nearly) the whole wall. The ≥95% product bar is
        // asserted on a real repo in the T7.3 evidence run; here 85% guards the structure
        // without flaking on a small fixture where inter-stage glue is proportionally larger.
        var stageSumMs = report.Stages.Sum(s => s.Elapsed.TotalMilliseconds);
        var totalMs = report.TotalWall.TotalMilliseconds;
        _output.WriteLine($"stage sum {stageSumMs:F0}ms of wall {totalMs:F0}ms = {stageSumMs / totalMs:P1}");
        Assert.True(totalMs > 0, "TotalWall not measured");
        Assert.True(stageSumMs >= totalMs * 0.85,
            $"Waterfall accounts for only {stageSumMs / totalMs:P1} of the wall ({stageSumMs:F0}ms of {totalMs:F0}ms) — a phase is running outside the named rows.");
    }

    private static string FixturePath(string relativePath)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "DevContext.slnx")))
        {
            var parent = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
            if (parent == dir) break;
            dir = parent;
        }
        return Path.GetFullPath(Path.Combine(dir ?? ".", relativePath));
    }
}
