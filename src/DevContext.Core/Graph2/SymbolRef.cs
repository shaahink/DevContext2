namespace DevContext.Core.Graph2;

public sealed record SymbolRef
{
    public required string Text { get; init; }
    public required RefSite Site { get; init; }
    public SymbolId? Resolved { get; init; }
    public ResolutionTier Tier { get; init; } = ResolutionTier.Unresolved;
    public ImmutableArray<SymbolId> Candidates { get; init; } = [];
    /// <summary>Use-site generic arity of the referenced type (<c>IdentifiedCommand&lt;T, R&gt;</c> → 2;
    /// bare mentions → 0). <see cref="Text"/> stays the bare base name for detector catalogs; the arity
    /// lets <see cref="SymbolTable"/> pick the structurally-matching declaration instead of erasing it.</summary>
    public int Arity { get; init; }
}
