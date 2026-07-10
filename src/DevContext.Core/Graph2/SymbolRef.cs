namespace DevContext.Core.Graph2;

public sealed record SymbolRef
{
    public required string Text { get; init; }
    public required RefSite Site { get; init; }
    public SymbolId? Resolved { get; init; }
    public ResolutionTier Tier { get; init; } = ResolutionTier.Unresolved;
    public ImmutableArray<SymbolId> Candidates { get; init; } = [];
}
