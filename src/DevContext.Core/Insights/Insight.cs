using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Insights;

public enum InsightCategory { Shape, Risk, Wiring, Data, Topology, Coverage }
public enum Severity { Info, Notable, Warning }

/// <summary>D6 typed action kind — determines how the UI navigates on an insight chip or action button.</summary>
public enum TypedActionKind { None, Focus, Node, Filter }

/// <summary>D6 typed action — a resolvable navigation target from an insight card. Every action carries
/// a Kind that tells the UI which route to take (Focus → GetTrace, Node → selectNode, Filter → deckKind).
/// When Kind is None, the chip renders as plain text with no link.</summary>
public sealed record TypedAction(TypedActionKind Kind, string Target)
{
    public static readonly TypedAction None = new(TypedActionKind.None, "");
    public static TypedAction? Focus(string? entryKey) => entryKey is not null ? new(TypedActionKind.Focus, entryKey) : null;
    public static TypedAction? Node(string? nodeId) => nodeId is not null ? new(TypedActionKind.Node, nodeId) : null;
    public static TypedAction? Filter(string? kind) => kind is not null ? new(TypedActionKind.Filter, kind) : null;
    public bool IsNone => Kind == TypedActionKind.None;
}

/// <summary>A structured finding about a repo — linkable, ranked, and honest.
/// L4.1: every insight carries confidence + why-it-matters + a suggested action so renderers
/// (CLI/desktop/MCP) can expose action buttons without hard-coding per-source logic.
/// M2.3 (D6): primary action replaced by <see cref="PrimaryAction"/> typed union; evidence chips
/// get <see cref="EvidenceActions"/> parallel array for resolvable links only.</summary>
public sealed record Insight(
    string Id,
    InsightCategory Category,
    Severity Severity,
    string Title,
    ImmutableArray<string> Evidence,
    string? JumpOff,
    double Confidence = 0.5,
    string? ConfidenceBasis = null,
    string? WhyItMatters = null)
{
    /// <summary>D6 — the primary action button on the insight card. Null means no action button.</summary>
    public TypedAction? PrimaryAction { get; init; }

    /// <summary>D6 — typed actions for evidence chips, parallel to <see cref="Evidence"/>. A null entry
    /// means the corresponding evidence chip renders as plain text (no link).</summary>
    public ImmutableArray<TypedAction?> EvidenceActions { get; init; } = [];

    /// <summary>Convenience factory — new fields default to honest neutral values.</summary>
    public static Insight Create(string id, InsightCategory category, Severity severity,
        string title, IEnumerable<string>? evidence = null, string? jumpOff = null,
        double confidence = 0.5, string? confidenceBasis = null,
        string? whyItMatters = null, TypedAction? action = null,
        ImmutableArray<TypedAction?>? evidenceActions = null)
        => new(id, category, severity, title,
            evidence?.ToImmutableArray() ?? [],
            jumpOff,
            confidence, confidenceBasis, whyItMatters)
        {
            PrimaryAction = action,
            EvidenceActions = evidenceActions ?? [],
        };
}

/// <summary>A source of insights — catalog-registered, pure post-graph computation.</summary>
public interface IInsightSource
{
    string Id { get; }
    InsightCategory Category { get; }
    IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries);
}
