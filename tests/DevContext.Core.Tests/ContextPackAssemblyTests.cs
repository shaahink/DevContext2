using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

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
                new ContextCardSpec("config", "Configuration", entryIds),
                new ContextCardSpec("tests", "Tests", entryIds),
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

        // T4.3 — config/tests are real server sections now (no client-stub "not traced" path).
        // CompositionApp reads no config and ships no tests, so both cards must be dropped
        // honestly: named in omitted[], never rendered empty.
        Assert.DoesNotContain(pack.Cards, c => c.Type is "config" or "tests");
        Assert.Contains(pack.Omitted, o => o.StartsWith("config", StringComparison.Ordinal));
        Assert.Contains(pack.Omitted, o => o.StartsWith("tests", StringComparison.Ordinal));

        // N0.1 (audit §3.F.4) — allocated_tokens is a measurement, not the ceiling echoed back
        // under a second label: a pack whose entries resolve to nothing allocated nothing.
        Assert.InRange(pack.AllocatedTokens, 1, 8000);
        var unresolved = builder.BuildMulti(
            [new ContextCardSpec("flow", "Flow", ["NoSuchEntry:Does.Not.Exist"])],
            totalBudget: 8000);
        Assert.Empty(unresolved.Cards);
        Assert.Equal(0, unresolved.AllocatedTokens);

        // N0.1 (audit §3.F.3) — the multi-entry merge must carry provenance, not just content.
        // Each entry's section text ends in its own `_provenance: N source sites · V verified ·
        // A approx_` footer, so a merged section's structured counts must be at least the sum of
        // the footers it printed. Before the fix the merge kept only the FIRST entry's
        // SourceLocations/Verified/Approx, so a card built from every entry said "12 verified"
        // in prose and reported 3 on the wire — one fact, two spellings.
        var mergedSections = pack.Cards
            .SelectMany(c => c.Sections.Select(s => (Card: c.Type, Section: s)))
            .Where(x => ProvenanceFooters(x.Section.Content).Count > 1)
            .ToList();
        Assert.NotEmpty(mergedSections);
        foreach (var (cardType, section) in mergedSections)
        {
            var footers = ProvenanceFooters(section.Content);
            var where = $"{cardType}/{section.Section}";
            Assert.True(
                section.Verified >= footers.Sum(f => f.Verified),
                $"{where}: content claims {footers.Sum(f => f.Verified)} verified across {footers.Count} merged entries, field says {section.Verified}");
            Assert.True(
                section.Approx >= footers.Sum(f => f.Approx),
                $"{where}: content claims {footers.Sum(f => f.Approx)} approx across {footers.Count} merged entries, field says {section.Approx}");
            Assert.NotEmpty(section.SourceLocations);
        }
    }

    /// <summary>N2.1 gate (audit §3.C, owner decision 2) — pack convergence: the multi-card path
    /// resolves a TYPE and a MEMBER through the same resolver `get_context` uses, and the `usage`
    /// section it has always built for a symbol root is now reachable from a card.
    /// <para>MEASURED before the change on this same fixture: both cards below came back dropped —
    /// `flow (…): no content for its entries — omitted` — because BuildMulti resolved ids against
    /// the entry inventory alone. `PriceService` is a plain class, not a declared entry.</para></summary>
    [Fact]
    public async Task Symbol_rooted_cards_resolve_and_carry_usage()
    {
        var (builder, _) = await BuildFixture();

        var pack = builder.BuildMulti(
            [
                // Bare type name — the spelling the resolver takes from an agent.
                new ContextCardSpec("flow", "Flow: PriceService", ["PriceService"]),
                // NodeId spelling — the one the desktop already sends for declared entries.
                new ContextCardSpec("usage", "Who uses IPriceService", ["Type:CompositionApp.Core.IPriceService"]),
                // Type:Member — the member-origin form, so a card can scope to one method.
                new ContextCardSpec("signatures", "RefreshAsync", ["PriceService:RefreshAsync"]),
                new ContextCardSpec("identity", "Repo identity", ["PriceService"]),
            ],
            totalBudget: 8000);

        Assert.DoesNotContain(pack.Omitted, o => o.Contains("no content for its entries", StringComparison.Ordinal));

        var flow = pack.Cards.Single(c => c.Type == "flow");
        Assert.NotEmpty(string.Join("", flow.Sections.Select(s => s.Content)).Trim());

        // The section that was built-but-unreachable: `usage` is the inbound direction, and
        // IPriceService is injected by PriceWorker and the controller.
        var usage = pack.Cards.Single(c => c.Type == "usage");
        var usageText = string.Join("\n", usage.Sections.Select(s => s.Content));
        Assert.Contains("usage", usage.Sections.Select(s => s.Section));
        // Inbound, with the caller named and the verb read from the caller's side.
        Assert.Contains("PriceHub", usageText, StringComparison.Ordinal);
        Assert.Contains(" calls it ", usageText, StringComparison.Ordinal);

        Assert.Contains(pack.Cards, c => c.Type == "signatures");

        // G1.2's honesty line reaches the Studio's pack too: a symbol-rooted pack says which
        // symbol it landed on and that the symbol is not a declared entry point.
        var identity = string.Join("\n", pack.Cards.Single(c => c.Type == "identity").Sections.Select(s => s.Content));
        Assert.Contains("Rooted on symbol:", identity, StringComparison.Ordinal);
        Assert.Contains("not a declared entry point", identity, StringComparison.Ordinal);

        // Budget accounting still measures rather than echoes (N0.1's rule) on this path.
        Assert.InRange(pack.AllocatedTokens, 1, 8000);
    }

    /// <summary>N2.1 — a declared entry keeps the focus spelling it always had, and the two spellings
    /// of one symbol collapse to ONE traced focus (one budget share), not two.</summary>
    [Fact]
    public async Task Declared_entries_and_symbols_share_one_resolution()
    {
        var (builder, snapshot) = await BuildFixture();
        var firstEntry = snapshot.Entries[0].Node.ToString();

        var pack = builder.BuildMulti(
            [
                new ContextCardSpec("flow", "Entry flow", [firstEntry]),
                new ContextCardSpec("signatures", "Same symbol, two spellings",
                    ["PriceService", "Type:CompositionApp.Web.Services.PriceService"]),
            ],
            totalBudget: 8000);

        Assert.Contains(pack.Cards, c => c.Type == "flow");
        var signatures = pack.Cards.Single(c => c.Type == "signatures");
        // One focus → one contributing entry → exactly one provenance footer. Two focuses would
        // have traced PriceService twice and merged its own content into itself.
        Assert.Single(ProvenanceFooters(string.Join("\n", signatures.Sections.Select(s => s.Content))));
    }

    private static async Task<(ContextPackBuilder Builder, AnalysisSnapshot Snapshot)> BuildFixture()
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
            Logger = loggerFactory.CreateLogger("PackConvergence"),
        };

        var snapshot = await TestPipeline.Build(loggerFactory).AnalyzeAsync(ctx);
        Assert.NotNull(snapshot.Graph);
        Assert.False(snapshot.Entries.IsDefaultOrEmpty);
        return (new ContextPackBuilder(new GraphQuery(snapshot.Graph!, snapshot.Entries, snapshot.Map), snapshot), snapshot);
    }

    /// <summary>N0.1 — the `_provenance:` footers `tokensAddSection` writes into a section's own
    /// text; one per contributing entry after a multi-entry merge.</summary>
    private static List<(int Sites, int Verified, int Approx)> ProvenanceFooters(string content)
    {
        var matches = Regex.Matches(
            content,
            @"_provenance: (\d+) source sites · (\d+) verified · (\d+) approx_");
        return [.. matches.Select(m => (
            int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
            int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
            int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture)))];
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
