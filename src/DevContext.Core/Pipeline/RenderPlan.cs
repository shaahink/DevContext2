namespace DevContext.Core.Pipeline;

/// <summary>The lens: which types render, in what order, truncated how, and what was cut.</summary>
public sealed record RenderPlan
{
    public required ImmutableArray<string> IncludedTypeIds { get; init; }
    public required ImmutableArray<PlannedExclusion> Excluded { get; init; }
    public required ImmutableArray<string> Sections { get; init; }
    public required int PerTypeCharCap { get; init; }
    public required int EstimatedTokens { get; init; }
    public required int MaxTokens { get; init; }
}

/// <summary>Records a type that was excluded from rendering and why.</summary>
public sealed record PlannedExclusion(string TypeId, string TypeName, double Score, string Reason);
