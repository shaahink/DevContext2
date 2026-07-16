using System.Collections.Immutable;

using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>T4.6 gate — pack assembly correctness on the CompositionApp fixture: a real analyzed
/// repo must produce a multi-card pack whose contracts differ from signatures, whose archetype
/// header is filled, and which ships no empty sections/cards and no HTML comment markers.</summary>
public sealed class ContextPackAssemblyTests
{
    [Fact]
    public async Task CompositionApp_pack_assembles_verifiably()
    {
        var repoPath = RepoPath(Path.Combine("tests", "fixtures", "CompositionApp"));
        Assert.True(Directory.Exists(repoPath), $"fixture missing: {repoPath}");

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
            Logger = loggerFactory.CreateLogger("PackAssembly"),
        };

        var snapshot = await TestPipeline.Build(loggerFactory).AnalyzeAsync(ctx);
        Assert.NotNull(snapshot.Graph);
        Assert.False(snapshot.Entries.IsDefaultOrEmpty);

        var query = new GraphQuery(snapshot.Graph!, snapshot.Entries, snapshot.Map);
        var builder = new ContextPackBuilder(query, snapshot);
        var entryIds = snapshot.Entries.Select(e => e.Node.ToString()).ToImmutableArray();

        var pack = builder.BuildMulti(
            [
                new ContextCardSpec("flow", "Flow", entryIds),
                new ContextCardSpec("signatures", "Member signatures", entryIds),
                new ContextCardSpec("contracts", "Contracts and interfaces", entryIds),
                new ContextCardSpec("entities", "Entities", entryIds),
            ],
            totalBudget: 8000);

        var md = pack.AssembledMarkdown;

        // T4.1 — identity header: repo named, archetype filled (never `_Archetype: _`).
        Assert.Contains("— Context Pack", md, StringComparison.Ordinal);
        Assert.DoesNotContain("_Archetype: _", md, StringComparison.Ordinal);

        // T4.6 — contracts is its own selection, not a signatures copy. CompositionApp's DI
        // spine reaches IPriceService/IAddonService, so the card must exist and differ.
        var signatures = pack.Cards.Single(c => c.Type == "signatures");
        var contracts = pack.Cards.Single(c => c.Type == "contracts");
        var signaturesText = string.Join("\n", signatures.Sections.Select(s => s.Content));
        var contractsText = string.Join("\n", contracts.Sections.Select(s => s.Content));
        Assert.NotEqual(signaturesText, contractsText);
        Assert.Contains("(interface)", contractsText, StringComparison.Ordinal);

        // T4.6 — no empty sections/cards rendered; no HTML comment markers in the human copy.
        Assert.DoesNotContain(", 0 tok_", md, StringComparison.Ordinal);
        Assert.DoesNotContain("<!--", md, StringComparison.Ordinal);
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
