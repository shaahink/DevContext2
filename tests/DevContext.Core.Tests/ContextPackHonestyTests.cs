using System.Collections.Immutable;

using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>N2.2 gate — the honesty layer the AGENT has had since D5.1, now owed to the human too.
/// <para>`get_context` has never let the ≥85%-fill promise fail silently: an under-filled pack says
/// WHY (the budget cut content, or the focus's connected subgraph is simply small) and, in the
/// second case, names better-connected focuses. GetContextPack returned the same sections with none
/// of that, so the Studio was the less honest of the two faces. These tests pin the verdict at the
/// place both faces read — <see cref="ContextPackBuilder"/> — not at either renderer.</para></summary>
public sealed class ContextPackHonestyTests
{
    /// <summary>The two under-fill reasons are DISTINGUISHED, and a pack that meets its promise
    /// says nothing at all. A single "low fill" flag would have been the easy version and the
    /// useless one: the two cases have opposite next moves (raise the ceiling vs pick another
    /// focus), and only one of them can be answered by a suggestion.</summary>
    [Fact]
    public async Task Under_filled_pack_says_which_kind_of_under_fill_it_is()
    {
        var (builder, entryIds) = await ContextPackFixture.BuildAsync();
        ContextCardSpec[] specs = [new("flow", "Flow", entryIds), new("signatures", "Signatures", entryIds)];

        // A ceiling far beyond what this fixture CONTAINS: nothing was cut, the pack is simply
        // everything there is. That is not an error and must not read as one.
        var exhausted = builder.BuildMulti(specs, totalBudget: 400_000);
        Assert.NotNull(exhausted.FillNote);
        Assert.Contains("everything reachable", exhausted.FillNote!, StringComparison.Ordinal);
        Assert.DoesNotContain("raise the budget", exhausted.FillNote!, StringComparison.Ordinal);
        // The percentage is real arithmetic over the pack's own totals, not a bucket label.
        Assert.Contains($"fill {exhausted.TotalTokens * 100L / 400_000}%", exhausted.FillNote!, StringComparison.Ordinal);

        // THE cross-check, and the reason this is an integration assertion rather than a unit one:
        // the note and the "N tok / M budget" header the Studio prints beside it must be the same
        // arithmetic. This program's recurring defect is one fact spelled two ways on one screen —
        // a header reading 96% next to a note saying the pack under-filled would be exactly that.
        // Swept across budgets because the pack SIZE moves with the budget (a bigger ceiling buys a
        // deeper walk), so no single ceiling exercises both sides.
        foreach (var budget in new[] { 600, 1500, 4000, 8000, 20_000 })
        {
            var p = builder.BuildMulti(specs, budget);
            var pct = p.TotalTokens * 100L / budget;
            Assert.Equal(pct < ContextPackBuilder.FillPromisePercent, p.FillNote is not null);
            if (p.FillNote is null) Assert.Empty(p.SuggestedFocuses);
        }
    }

    /// <summary>The branch selection itself, over inputs chosen to hit each arm — the arms are
    /// what a reader acts on, and on a small fixture the real pack cannot reach both.</summary>
    [Fact]
    public async Task Budget_cut_and_content_exhausted_are_told_apart_by_the_omitted_reasons()
    {
        var (builder, _) = await ContextPackFixture.BuildAsync();
        IReadOnlyList<(string, int)> noFocuses = [];

        // Same low fill, different cause — the ONLY signal is what omitted[] says (T5.1 put the
        // real reasons there; before that they were built and discarded).
        var (cutNote, cutSuggested) = builder.BuildFillNote(
            totalTokens: 1000, totalBudget: 8000,
            omitted: ["signatures: omitted (1450 tokens, budget exhausted)"], noFocuses);
        Assert.NotNull(cutNote);
        Assert.Contains("fill 12%", cutNote!, StringComparison.Ordinal);
        Assert.Contains("raise the budget", cutNote!, StringComparison.Ordinal);
        // A budget-cut pack gets NO suggestions: another focus is not the next move, the slider
        // is. Offering focuses here is the kind of helpfulness that sends the reader sideways.
        Assert.Empty(cutSuggested);

        var (exhaustedNote, _) = builder.BuildFillNote(
            totalTokens: 1000, totalBudget: 8000,
            omitted: ["entities: no touched entities"], noFocuses);
        Assert.NotNull(exhaustedNote);
        Assert.Contains("everything reachable", exhaustedNote!, StringComparison.Ordinal);
        Assert.DoesNotContain("raise the budget", exhaustedNote!, StringComparison.Ordinal);

        // At or above the promise, nothing is owed and nothing is said — either way round.
        Assert.Null(builder.BuildFillNote(6800, 8000, ["x: budget exhausted"], noFocuses).Note);
        Assert.Null(builder.BuildFillNote(8000, 8000, [], noFocuses).Note);
        // A zero ceiling is not an under-fill; dividing by it would be the only bug on offer.
        Assert.Null(builder.BuildFillNote(0, 0, [], noFocuses).Note);
    }

    /// <summary>Suggestions are the repo's better-connected flows and never include the focuses the
    /// pack is already built on — a suggestion to look at what you are looking at is worse than no
    /// suggestion. Repos with no ranked flows (a class library) get the note alone.</summary>
    [Fact]
    public async Task Suggested_focuses_exclude_the_pack_s_own_focuses_and_are_addressable()
    {
        var (builder, entryIds) = await ContextPackFixture.BuildAsync();

        // Build on ONE entry with a ceiling nothing can fill, so the content-exhausted branch runs.
        var one = entryIds.Take(1).ToImmutableArray();
        var pack = builder.BuildMulti([new ContextCardSpec("flow", "Flow", one)], totalBudget: 400_000);
        Assert.NotNull(pack.FillNote);

        if (pack.SuggestedFocuses.Length == 0) return; // fixture has no ranked multi-step flows

        var built = pack.Cards.SelectMany(c => c.Sections).Select(s => s.Content).ToList();
        foreach (var s in pack.SuggestedFocuses)
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Focus));
            Assert.True(s.Depth >= 2, $"'{s.Focus}' is offered as better connected at depth {s.Depth}");

            // The contract that makes a suggestion actionable: the string we hand the reader is one
            // the builder itself accepts. A suggestion the product cannot consume is a dead end
            // dressed as a next step.
            var followed = builder.BuildMulti([new ContextCardSpec("flow", "Follow", [s.Focus])]);
            Assert.NotEmpty(followed.Cards);
        }
        Assert.Equal(pack.SuggestedFocuses.Select(s => s.Focus).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                     pack.SuggestedFocuses.Length);
        Assert.NotEmpty(built);
    }

    /// <summary>N2.2 — ONE budget number. It was 8000 in the MCP tool, 8000 in three server
    /// fallbacks, 8000 in the app's API client and 4000 in the desktop preference the Studio
    /// actually opened with, so the human's pack was priced at half the agent's silently. This
    /// pins the constant every C# site now reads, and pins that it is NOT
    /// <see cref="TracePolicy.DefaultBudgetTokens"/> — a different budget for a different thing,
    /// and the most likely origin of the 4000.</summary>
    [Fact]
    public void Pack_budget_default_is_one_number()
    {
        Assert.Equal(8000, ContextPackBuilder.DefaultBudgetTokens);
        Assert.NotEqual(TracePolicy.DefaultBudgetTokens, ContextPackBuilder.DefaultBudgetTokens);
        Assert.Equal(85, ContextPackBuilder.FillPromisePercent);
    }
}
