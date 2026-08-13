using System.Collections.Immutable;

using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>N2.2 — the CompositionApp pack fixture, shared by the pack test classes that need a
/// real analyzed graph (ledger + honesty). It was private to ContextPackLedgerTests; a second
/// copy is how two tests start disagreeing about what "the fixture" is.</summary>
internal static class ContextPackFixture
{
    public static async Task<(ContextPackBuilder Builder, ImmutableArray<string> EntryIds)> BuildAsync()
    {
        var repoPath = RepoPath(Path.Combine("tests", "fixtures", "CompositionApp"));
        Assert.True(Directory.Exists(repoPath), $"fixture missing: {repoPath}");
        var (builder, snapshot) = await AnalyzeAsync(repoPath);
        Assert.False(snapshot.Entries.IsDefaultOrEmpty);
        return (builder, snapshot.Entries.Select(e => e.Node.ToString()).ToImmutableArray());
    }

    /// <summary>N2.2 — the same analyze+builder construction against an ARBITRARY repo path, so an
    /// eval-scale test can pack a real cloned library. The snapshot rides back because a library's
    /// scope lives in <c>Map.Surface</c>, not in <c>Entries</c>.</summary>
    public static async Task<(ContextPackBuilder Builder, AnalysisSnapshot Snapshot)> AnalyzeAsync(string repoPath)
    {
        var fs = new RealFileSystem();
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = new ExtractionOptions { MaxOutputTokens = 8000, OutputFormat = OutputFormat.Markdown, AllowRoslyn = true },
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = new AnalysisCache(fs),
            Analysis = new SharedAnalysisContext(),
            Logger = loggerFactory.CreateLogger("PackFixture"),
        };

        var snapshot = await TestPipeline.Build(loggerFactory).AnalyzeAsync(ctx);
        Assert.NotNull(snapshot.Graph);

        var query = new GraphQuery(snapshot.Graph!, snapshot.Entries, snapshot.Map);
        return (new ContextPackBuilder(query, snapshot), snapshot);
    }

    public static string RepoPath(string relativePath)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "DevContext.slnx")))
        {
            var parent = Path.GetDirectoryName(dir);
            if (parent == dir) break;
            dir = parent;
        }
        return Path.Combine(dir ?? ".", relativePath);
    }
}
