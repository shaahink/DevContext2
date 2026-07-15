using Api.Services;

using HotChocolate;
using HotChocolate.Types;

namespace Api.GraphQl;

/// <summary>A HotChocolate GraphQL query resolver. The resolver field delegates to an injected domain
/// service — the callee the seeded Map-mode call graph must reach (T1.1).</summary>
[QueryType]
public sealed class CatalogQuery
{
    private readonly ICatalogLookupService _catalog;

    public CatalogQuery(ICatalogLookupService catalog) => _catalog = catalog;

    public string GetProduct(int id) => _catalog.FindProduct(id);
}
