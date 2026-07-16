using System.Collections.Immutable;

using DevContext.Core.Analysis;
using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>T4.5 gate — verify_context flips stale when a spine file is edited mid-session.
/// End-to-end over a TEMP COPY of the ControllerApp fixture: full analyze captures fingerprints,
/// a pack is built, the file behind the spine is edited/deleted, the verifier reports it.</summary>
public sealed class ContextPackVerifierTests
{
    [Fact]
    public async Task Verify_flips_stale_when_a_spine_file_changes_mid_session()
    {
        var fixture = RepoPath(Path.Combine("tests", "fixtures", "ControllerApp"));
        Assert.True(Directory.Exists(fixture), $"fixture missing: {fixture}");

        var work = Path.Combine(Path.GetTempPath(), "dc-t45-" + Guid.NewGuid().ToString("N"));
        CopyTree(fixture, work);
        try
        {
            var fs = new RealFileSystem();
            var rootResult = await ProjectRootResolver.ResolveAsync(work, fs, CancellationToken.None);
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
                Logger = loggerFactory.CreateLogger("VerifierGate"),
            };

            var snapshot = await TestPipeline.Build(loggerFactory).AnalyzeAsync(ctx);
            Assert.NotNull(snapshot.Graph);
            Assert.NotEmpty(snapshot.FileFingerprints);

            var query = new GraphQuery(snapshot.Graph!, snapshot.Entries, snapshot.Map);
            var builder = new ContextPackBuilder(query, snapshot);
            var focus = snapshot.Entries[0].Title;
            var pack = builder.Build(focus);
            Assert.True(pack.Found);

            var verifier = new ContextPackVerifier(snapshot);

            // Fresh session — nothing on disk moved, nothing may be stale.
            var fresh = verifier.Verify(pack.Sections);
            Assert.DoesNotContain(fresh, s => s.Stale);

            // Edit the file behind the spine (any fingerprinted file the pack cites).
            var citedRel = pack.Sections
                .SelectMany(s => s.SourceLocations)
                .Select(l => l.Contains(':') ? l[..l.LastIndexOf(':')] : l)
                .First();
            var citedAbs = Path.GetFullPath(Path.Combine(rootResult.EffectiveRootPath, citedRel));
            File.AppendAllText(citedAbs, "\n// mid-session edit\nclass MidSessionEdit { }\n");

            var afterEdit = verifier.Verify(pack.Sections);
            var staleSections = afterEdit.Where(s => s.Stale).ToList();
            Assert.NotEmpty(staleSections);
            var delta = staleSections.SelectMany(s => s.Changed).First(c => c.File == citedRel);
            Assert.Equal("modified", delta.Status);
            Assert.True(delta.LineDelta >= 3, $"line delta should count the appended lines, got {delta.LineDelta}");

            // Delete it — the verdict names the loss.
            File.Delete(citedAbs);
            var afterDelete = verifier.Verify(pack.Sections);
            Assert.Contains(afterDelete.SelectMany(s => s.Changed), c => c.File == citedRel && c.Status == "deleted");
        }
        finally
        {
            Directory.Delete(work, recursive: true);
        }
    }

    private static void CopyTree(string from, string to)
    {
        Directory.CreateDirectory(to);
        foreach (var dir in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(to, Path.GetRelativePath(from, dir)));
        foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(to, Path.GetRelativePath(from, file)));
    }

    private static string RepoPath(string relativePath)
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
