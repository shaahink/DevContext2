using System.Collections.Immutable;

namespace DevContext.Core.Graph.Facets;

/// <summary>Declarative facet: gate + compute → composable per-archetype value packs.
/// One facet = one file + one eval check. Registered in a FacetCatalog, consumed by
/// MapRenderer, desktop, and MCP via GraphQuery.</summary>
public sealed record FacetDescriptor(
    string Id,
    string SectionKey,
    int Cap,
    Func<FacetContext, FacetResult> Compute);

/// <summary>Input passed to every facet's Compute function — the data snapshots available
/// after graph assembly.</summary>
public sealed record FacetContext(
    DiscoveryModel Model,
    CodeGraph Graph,
    ImmutableArray<EntryPoint> Entries);

/// <summary>The rendered facet result — one or more line groups with an optional cap disclosure.</summary>
public sealed record FacetResult(
    ImmutableArray<string> Lines,
    bool Truncated,
    int TotalCount)
{
    public static FacetResult Empty => new([], false, 0);
    public static FacetResult Single(string line) => new([line], false, 1);
}
