using System.Collections.Immutable;

using DevContext.Core.Graph;

using Xunit.Abstractions;

namespace DevContext.Core.Tests;

/// <summary>N2.2 ACCEPTANCE (audit §5 N2, owner decision 2 = FULL convergence) — the pack the
/// Studio's Types tab composes, proven on a REAL cloned library rather than the 4-file fixture.
/// <para>The claim under test is the whole point of N2: <b>one pipeline, two faces</b>. A human
/// scoping FluentValidation has no entry points to click — it is a library, its Map is a public
/// SURFACE — so before N2.1 the Studio could only offer a card whose entryIds BuildMulti refused,
/// and the human's pack came back empty while `get_context` answered the same question fine. This
/// test drives the human path end to end at real-repo scale: namespace-qualified type names, taken
/// off <c>Map.Surface</c> (the exact source and the exact key the Types tab uses —
/// <c>typeFocus(namespace, name)</c> in scope-picker.ts), packed through <c>BuildMulti</c>, and it
/// asserts the pack has USAGE and VERIFIED counts — content, not just a non-empty response.</para>
/// <para>Eval-tier because it analyzes a ~700-file clone (~30s); skips silently when the clone is
/// absent, the same contract <c>BudgetIndependenceTests</c> uses.</para></summary>
[Trait("Category", "Eval")]
public sealed class LibraryPackEvalTests(ITestOutputHelper output)
{
    [Fact]
    public async Task FluentValidation_pack_composed_from_types_has_usage_and_verified_counts()
    {
        var repo = ContextPackFixture.RepoPath(Path.Combine("eval-repos", "FluentValidation", "src", "FluentValidation"));
        if (!Directory.Exists(repo)) return; // eval repo not cloned — skip silently

        var (builder, snapshot) = await ContextPackFixture.AnalyzeAsync(repo);

        // The library face of the Map. If this is null the Types tab has nothing to show either,
        // and the rest of the test would be asserting against a fiction.
        var surface = snapshot.Map?.Surface;
        Assert.NotNull(surface);

        // Same key the picker sends: namespace-qualified, because short type names collide across
        // namespaces (typeFocus() in scope-picker.ts).
        var query = new GraphQuery(snapshot.Graph!, snapshot.Entries, snapshot.Map);
        var candidates = surface!.Groups
            .SelectMany(g => g.Types.Select(t => g.Namespace is { Length: > 0 } ns ? $"{ns}.{t.Name}" : t.Name))
            .Distinct(StringComparer.Ordinal)
            .Select(f => (Focus: f, Usages: query.ResolveEntry(f) is { } e ? query.FindUsages(e.Node).Length : -1))
            .ToList();
        Assert.Contains(candidates, c => c.Usages >= 0);   // the surface is addressable at all

        // MEASURED, and it is a property of libraries rather than of this clone: `usage` is the
        // INBOUND direction, and a library's outermost public types (DefaultValidatorExtensions,
        // DefaultValidatorOptions…) are consumed from OUTSIDE the repo, so they have zero in-repo
        // usages and their usage card is legitimately empty. Ranking by member count — the obvious
        // proxy for "important type" — lands on exactly those and measures nothing. So the pack is
        // scoped to the types that DO have in-repo consumers, which is the case the acceptance is
        // about, and the count is printed so the choice is visible rather than assumed.
        var focuses = candidates
            .Where(c => c.Usages > 0)
            .OrderByDescending(c => c.Usages)
            .Take(3)
            .Select(c => c.Focus)
            .ToImmutableArray();
        Assert.False(focuses.IsEmpty, "no public type on this surface has an in-repo consumer");
        output.WriteLine($"surface: {candidates.Count} public types, {candidates.Count(c => c.Usages > 0)} with in-repo usages");
        output.WriteLine($"focuses: {string.Join(", ", candidates.Where(c => focuses.Contains(c.Focus)).Select(c => $"{c.Focus} ({c.Usages} usages)"))}");

        ContextCardSpec[] specs =
        [
            new("signatures", "Signatures", focuses),
            new("usage", "Usage", focuses),
        ];
        var pack = builder.BuildMulti(specs, ContextPackBuilder.DefaultBudgetTokens);

        // 1. THE N2.1 CONVERGENCE, at real-repo scale: a bare namespace-qualified TYPE — never a
        //    declared entry point — resolves and produces sections. This is the exact input that
        //    returned an empty pack before, with no error to explain it.
        Assert.NotEmpty(pack.Cards);
        foreach (var card in pack.Cards)
            Assert.False(card.Sections.IsDefaultOrEmpty, $"card '{card.Title}' resolved no sections");

        // 2. USAGE joined the card types (audit wire item 5): the `usage` card carries a real
        //    usage section with content, not a title echo.
        var usage = Assert.Single(pack.Cards.Where(c => c.Type == "usage"));
        var usageSection = Assert.Single(usage.Sections.Where(s => s.Section == "usage"));
        Assert.False(string.IsNullOrWhiteSpace(usageSection.Content));
        Assert.True(usageSection.Tokens > 0, "usage section priced at 0 tokens");

        // 3. VERIFIED COUNTS: the pack's provenance is real. Verified = resolved semantically or by
        //    a detection join; a pack that is all-approx is a pack the reader cannot trust, and the
        //    Studio renders these per card (N1.1).
        var verified = pack.Cards.SelectMany(c => c.Sections).Sum(s => s.Verified);
        var approx = pack.Cards.SelectMany(c => c.Sections).Sum(s => s.Approx);
        Assert.True(verified > 0, $"pack has no semantically-verified provenance (verified={verified}, approx={approx})");

        // 4. The staleness ledger covers what was assembled — one verdict per section, computed by
        //    the server over the sections it actually built (N1.1 wire item 4), and a clean clone
        //    on an unmodified checkout must not read as stale.
        var sectionKeys = pack.Cards.SelectMany(c => c.Sections).Select(s => s.Section).Distinct().ToHashSet(StringComparer.Ordinal);
        Assert.Equal(sectionKeys.Count, pack.Verification.Length);
        Assert.All(pack.Verification, v => Assert.Contains(v.Section, sectionKeys));

        // 5. HONESTY-NOTE PARITY (N2.2 itself), on a real repo: the note and the token header are
        //    the same arithmetic. A pack that met its promise says nothing; one that did not says
        //    which kind of under-fill it is, and only a content-exhausted one offers focuses.
        var fillPct = pack.TotalTokens * 100L / ContextPackBuilder.DefaultBudgetTokens;
        Assert.Equal(fillPct < ContextPackBuilder.FillPromisePercent, pack.FillNote is not null);
        if (pack.FillNote is not null)
        {
            Assert.Contains($"fill {fillPct}%", pack.FillNote, StringComparison.Ordinal);
            if (pack.FillNote.Contains("raise the budget", StringComparison.Ordinal))
                Assert.Empty(pack.SuggestedFocuses);
        }
        else
        {
            Assert.Empty(pack.SuggestedFocuses);
        }

        output.WriteLine($"cards={pack.Cards.Length} sections={sectionKeys.Count} tokens={pack.TotalTokens}/"
            + $"{ContextPackBuilder.DefaultBudgetTokens} (fill {fillPct}%) verified={verified} approx={approx}");
        output.WriteLine($"usage section: {usageSection.Tokens} tok, {usageSection.SourceLocations.Length} source locations");
        output.WriteLine($"fillNote: {pack.FillNote ?? "(none — pack met its fill promise)"}");
        foreach (var s in pack.SuggestedFocuses) output.WriteLine($"  suggested: {s.Focus} [{s.Kind}] depth={s.Depth}");
        output.WriteLine(pack.AssembledMarkdown.Length > 1200 ? pack.AssembledMarkdown[..1200] : pack.AssembledMarkdown);
    }
}
