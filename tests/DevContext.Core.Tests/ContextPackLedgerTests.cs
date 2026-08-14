using System.Collections.Immutable;

using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>N1.1 gate — the two truths GetContextPack gained: the pack carries its OWN staleness
/// ledger (audit wire item 4: the ledger must describe the pack that was built, not a re-derived
/// one), and the Studio's per-card body toggle reaches the pack (audit §3.F.2).</summary>
public sealed class ContextPackLedgerTests
{
    [Fact]
    public async Task Pack_carries_a_ledger_for_its_own_sections_and_honors_exclude_bodies()
    {
        var (builder, entryIds) = await BuildFixtureAsync();

        ContextCardSpec[] Specs(bool excludeBodies) =>
        [
            new("flow", "Flow", entryIds),
            new("signatures", "Member signatures", entryIds),
            new("bodies", "Code bodies", entryIds, excludeBodies),
        ];

        var pack = builder.BuildMulti(Specs(excludeBodies: false), totalBudget: 8000);

        // ── wire item 4 ────────────────────────────────────────────────────────────────
        // The ledger's keys are exactly the pack's own section keys. The pre-N1.1 client
        // assembled this from one VerifyContext per focus, each handed the WHOLE budget and
        // each verifying every section of that focus — so it could report a section this pack
        // dropped (the `wanted` filter) and could miss the halves the proportional split built.
        var packSectionKeys = pack.Cards
            .SelectMany(c => c.Sections.Select(s => s.Section))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();
        var ledgerKeys = pack.Verification
            .Select(v => v.Section)
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();
        Assert.NotEmpty(packSectionKeys);
        Assert.Equal(packSectionKeys, ledgerKeys);

        // A ledger entry counts the files ITS section cites — the union across the cards that
        // share the key, never a whole-focus file set.
        foreach (var v in pack.Verification)
        {
            var cited = pack.Cards
                .SelectMany(c => c.Sections)
                .Where(s => string.Equals(s.Section, v.Section, StringComparison.OrdinalIgnoreCase))
                .SelectMany(s => s.SourceLocations)
                .Select(l => l.Contains(':') ? l[..l.LastIndexOf(':')] : l)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            Assert.Equal(cited, v.FilesChecked);
        }

        // The fixture is analyzed from disk moments earlier, so nothing has drifted: the flag
        // and the per-section verdicts must agree, in the honest direction.
        Assert.Equal(pack.Verification.Any(v => v.Stale), pack.AnyStale);
        Assert.False(pack.AnyStale);

        // ── exclude_bodies (§3.F.2) ────────────────────────────────────────────────────
        // The toggle was an icon and an opacity: the copied bytes carried every body while the
        // budget pill read "All bodies hidden". Hiding bodies now really cuts the section.
        var bodies = pack.Cards.Single(c => c.Type == "bodies");
        Assert.Contains(bodies.Sections, s => s.Section == ContextPackBuilder.BodiesSection);

        var hidden = builder.BuildMulti(Specs(excludeBodies: true), totalBudget: 8000);
        Assert.DoesNotContain(hidden.Cards, c => c.Type == "bodies");
        Assert.Contains(hidden.Omitted, o => o.Contains("code bodies hidden", StringComparison.Ordinal));
        Assert.DoesNotContain("## Code bodies", hidden.AssembledMarkdown, StringComparison.Ordinal);

        // …and the ledger follows the pack it describes: the bodies section is gone from both.
        Assert.DoesNotContain(hidden.Verification, v => v.Section == ContextPackBuilder.BodiesSection);
        Assert.True(hidden.TotalTokens < pack.TotalTokens,
            $"hiding bodies must shrink the pack: {hidden.TotalTokens} vs {pack.TotalTokens}");

        // The flag is scoped to cards that can carry bodies — everything else is untouched, which
        // is why the Studio renders the toggle only for these types.
        Assert.Equal(["bodies"], ContextPackBuilder.BodyCapableCardTypes);
        var flowUnaffected = builder.BuildMulti(
            [new ContextCardSpec("flow", "Flow", entryIds, ExcludeBodies: true)],
            totalBudget: 8000);
        var flowPlain = builder.BuildMulti(
            [new ContextCardSpec("flow", "Flow", entryIds)],
            totalBudget: 8000);
        Assert.Equal(flowPlain.TotalTokens, flowUnaffected.TotalTokens);
    }

    // N2.2 — the fixture moved to ContextPackFixture so the honesty tests build the same graph.
    private static Task<(ContextPackBuilder Builder, ImmutableArray<string> EntryIds)> BuildFixtureAsync()
        => ContextPackFixture.BuildAsync();
}
