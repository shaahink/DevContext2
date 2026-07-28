using System.Collections.Immutable;

using DevContext.Core.Insights;
using DevContext.Server.Mapping;

using Proto = DevContext.Protos;

namespace DevContext.Server.Tests;

/// <summary>
/// S10 — the wire spelling of <see cref="Insight.Severity"/> is part of the contract, and it is
/// lowercase.
///
/// It used to be whatever <c>Severity.ToString()</c> produced ("Warning"), which is a spelling no
/// consumer agreed on: the desktop app keyed every severity lookup on "warning" and the MCP server
/// filtered on "WARNING". Both guesses were wrong and BOTH FAILED SILENTLY — the app filed
/// warnings under its "Know this" heading and never once rendered the "Act on this" group, gave
/// security warnings the info-blue border, and dropped every engine warning from Home's "Needs
/// attention"; the MCP's <c>stats.warnings</c> array came back empty on every repo ever analysed.
///
/// A stringly-typed enum needs a spelling something enforces, or each new consumer re-guesses.
/// That is what these tests are.
/// </summary>
public sealed class ProtoMapperSeverityTests
{
    private static Proto.StatsResponse Map(params Severity[] severities)
    {
        var insights = severities
            .Select((s, i) => Insight.Create($"test.{i}", InsightCategory.Risk, s, $"title {i}"))
            .ToImmutableArray();

        return ProtoMapper.ToStatsResponse(
            report: null, graph: null,
            nodeCount: 0, edgeCount: 0, entryCount: 0,
            seams: [], entriesWithTarget: 0, totalWallMs: 0,
            insights: insights,
            entries: []);
    }

    [Fact]
    public void Severity_reaches_the_wire_lowercase()
    {
        var resp = Map(Severity.Warning, Severity.Notable, Severity.Info);

        Assert.Equal(["warning", "notable", "info"], resp.Insights.Select(i => i.Severity));
    }

    /// <summary>Every member of the enum, so a new severity cannot land with a capital letter.</summary>
    [Fact]
    public void Every_severity_is_lowercase_on_the_wire()
    {
        var all = Enum.GetValues<Severity>();

        var resp = Map(all);

        Assert.All(resp.Insights, i => Assert.Equal(i.Severity.ToLowerInvariant(), i.Severity));
        Assert.Equal(all.Length, resp.Insights.Count);
    }

    /// <summary>
    /// The exact predicate the MCP's <c>stats.warnings</c> uses. This is the assertion that would
    /// have caught the empty-array bug the day it shipped.
    /// </summary>
    [Fact]
    public void Mcp_warning_filter_matches_a_warning()
    {
        var resp = Map(Severity.Warning, Severity.Info);

        var warnings = resp.Insights.Where(i => i.Severity == "warning").Select(i => i.Title).ToArray();

        Assert.Single(warnings);
    }
}
