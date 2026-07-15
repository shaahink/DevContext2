using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>
/// MCP blind-drive audit (2026-07-11) bug #1: trace()/get_context() failed on bare route strings
/// ("/products") because EntryPointResolver only matched the full "METHOD /route" title. An agent
/// that just read a route off `entrypoints` would naturally try the bare route next.
/// </summary>
public sealed class EntryPointResolverTests
{
    private static EntryPoint HttpEntry(string method, string route) =>
        new(EntryPointKind.HttpEndpoint, $"{method} {route}", NodeId.ForEntry($"{method} {route}"))
        {
            HttpMethod = method,
            Route = route,
        };

    [Fact]
    public void Resolve_matches_bare_route_when_unambiguous()
    {
        var entries = new[] { HttpEntry("GET", "/products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/products");

        Assert.NotNull(resolved);
        Assert.Equal("GET /products", resolved!.Title);
    }

    [Fact]
    public void Resolve_bare_route_ignores_leading_and_trailing_slash_differences()
    {
        var entries = new[] { HttpEntry("GET", "/api/Products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/api/Products/");

        Assert.NotNull(resolved);
        Assert.Equal("GET /api/Products", resolved!.Title);
    }

    [Fact]
    public void Resolve_prefers_GET_when_route_is_ambiguous_across_verbs()
    {
        var entries = new[]
        {
            HttpEntry("DELETE", "/api/Products"),
            HttpEntry("POST", "/api/Products"),
            HttpEntry("GET", "/api/Products"),
        };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/api/Products");

        Assert.NotNull(resolved);
        Assert.Equal("GET /api/Products", resolved!.Title);
    }

    [Fact]
    public void Resolve_bare_route_falls_back_to_first_when_no_GET_present()
    {
        var entries = new[]
        {
            HttpEntry("DELETE", "/api/Products"),
            HttpEntry("POST", "/api/Products"),
        };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/api/Products");

        Assert.NotNull(resolved);
        Assert.Equal("DELETE /api/Products", resolved!.Title);
    }

    [Fact]
    public void Resolve_still_matches_exact_title_first()
    {
        var entries = new[] { HttpEntry("GET", "/products"), HttpEntry("POST", "/products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "POST /products");

        Assert.NotNull(resolved);
        Assert.Equal("POST /products", resolved!.Title);
    }

    [Fact]
    public void Resolve_unknown_bare_route_returns_null()
    {
        var entries = new[] { HttpEntry("GET", "/products") };
        var graph = new CodeGraphBuilder().Build();

        var resolved = EntryPointResolver.Resolve(entries, graph, "/unknown");

        Assert.Null(resolved);
    }
}
