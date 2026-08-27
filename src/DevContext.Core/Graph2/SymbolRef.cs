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

    /// <summary>F1 (#33) — set when a Tier-B semantic bind CONTRADICTED this ref's syntactic text
    /// (the scope guessed one type, Roslyn measured another). A contradicted ref is unresolvable by
    /// name: <see cref="SymbolTable.Resolve"/> must never re-run the name ladder on text a real
    /// semantic bind has already disproved — that would resurrect the disproved guess. <see cref="Text"/>
    /// keeps the original spelling for diagnostics only.</summary>
    public bool Contradicted { get; init; }
}
