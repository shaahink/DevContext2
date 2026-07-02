using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Insights;

public enum InsightCategory { Shape, Risk, Wiring, Data, Topology, Coverage }
public enum Severity { Info, Notable, Warning }

/// <summary>A structured finding about a repo — linkable, ranked, and honest.</summary>
public sealed record Insight(
    string Id,
    InsightCategory Category,
    Severity Severity,
    string Title,
    ImmutableArray<string> Evidence,
    string? JumpOff)
{
    /// <summary>Convenience factory.</summary>
    public static Insight Create(string id, InsightCategory category, Severity severity,
        string title, IEnumerable<string>? evidence = null, string? jumpOff = null)
        => new(id, category, severity, title,
            evidence?.ToImmutableArray() ?? [],
            jumpOff);
}

/// <summary>A source of insights — catalog-registered, pure post-graph computation.</summary>
public interface IInsightSource
{
    string Id { get; }
    InsightCategory Category { get; }
    IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries);
}
