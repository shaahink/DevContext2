using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Insights;

public enum InsightCategory { Shape, Risk, Wiring, Data, Topology, Coverage }
public enum Severity { Info, Notable, Warning }

/// <summary>What a consumer should do about this insight. Each maps to a UI or MCP affordance.</summary>
public enum InsightAction { None, Trace, Usages, Export }

/// <summary>A structured finding about a repo — linkable, ranked, and honest.
/// L4.1: every insight carries confidence + why-it-matters + a suggested action so renderers
/// (CLI/desktop/MCP) can expose action buttons without hard-coding per-source logic.</summary>
public sealed record Insight(
    string Id,
    InsightCategory Category,
    Severity Severity,
    string Title,
    ImmutableArray<string> Evidence,
    string? JumpOff,
    double Confidence = 0.5,
    string? ConfidenceBasis = null,
    string? WhyItMatters = null,
    InsightAction Action = InsightAction.None,
    string? ActionTarget = null)
{
    /// <summary>Convenience factory — new fields default to honest neutral values.</summary>
    public static Insight Create(string id, InsightCategory category, Severity severity,
        string title, IEnumerable<string>? evidence = null, string? jumpOff = null,
        double confidence = 0.5, string? confidenceBasis = null,
        string? whyItMatters = null, InsightAction action = InsightAction.None,
        string? actionTarget = null)
        => new(id, category, severity, title,
            evidence?.ToImmutableArray() ?? [],
            jumpOff,
            confidence, confidenceBasis, whyItMatters,
            action, actionTarget);
}

/// <summary>A source of insights — catalog-registered, pure post-graph computation.</summary>
public interface IInsightSource
{
    string Id { get; }
    InsightCategory Category { get; }
    IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries);
}
