using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;

namespace DevContext.Core.Tests;

public sealed class AnonymousEndpointsSourceTests
{
    private static readonly CodeGraph EmptyGraph = new(
        new Dictionary<NodeId, GraphNode>(),
        new Dictionary<NodeId, ImmutableArray<GraphEdge>>());

    private static EntryPoint HttpEntry(string method, string route) => new(
        EntryPointKind.HttpEndpoint, $"{method} {route}", new NodeId(NodeKind.EntryPoint, $"{method} {route}"))
    {
        HttpMethod = method,
        Route = route,
    };

    private static EndpointDetection Endpoint(string method, string route, params string[] authAttrs) => new(
        method, route, "Handler", "Handler", [.. authAttrs], [])
    {
        ExtractorName = "Test",
        SourceFile = "Program.cs",
        LineNumber = 1,
    };

    [Fact]
    public void VerbCollision_OnSameRoute_ReportsOnlyTheGenuinelyAnonymousVerb()
    {
        // TodoApi shape (E1): GET /todos is protected, but a naive route-only dedup let the POST
        // detection (enumerated first) hide the GET's real (protected) status, or vice versa.
        var model = new DiscoveryModel();
        model.Detections.Add(Endpoint("GET", "/todos", "[Authorize]"));
        model.Detections.Add(Endpoint("POST", "/todos"));

        var entries = new[] { HttpEntry("GET", "/todos"), HttpEntry("POST", "/todos") }.ToImmutableArray();

        var insight = new AnonymousEndpointsSource().Compute(model, EmptyGraph, entries).Single();

        Assert.StartsWith("1/2 endpoints anonymous", insight.Title);
        Assert.Contains("POST /todos", insight.Evidence);
        Assert.DoesNotContain("GET /todos", insight.Evidence);
    }

    [Fact]
    public void GlobalFallbackPolicy_NoPerEndpointAuth_NeverSaysAnonymous()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(Endpoint("GET", "/orders"));
        model.Detections.Add(new GlobalAuthPolicyDetection(true)
        {
            ExtractorName = "Test",
            SourceFile = "Program.cs",
            LineNumber = 1,
        });

        var entries = new[] { HttpEntry("GET", "/orders") }.ToImmutableArray();

        var insight = new AnonymousEndpointsSource().Compute(model, EmptyGraph, entries).Single();

        Assert.DoesNotContain("anonymous", insight.Title, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not individually verifiable", insight.Title);
    }

    [Fact]
    public void GlobalFallbackPolicy_ExplicitAllowAnonymous_StillReportedAsAnonymous()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(Endpoint("GET", "/public", "[AllowAnonymous]"));
        model.Detections.Add(Endpoint("GET", "/private"));
        model.Detections.Add(new GlobalAuthPolicyDetection(true)
        {
            ExtractorName = "Test",
            SourceFile = "Program.cs",
            LineNumber = 1,
        });

        var entries = new[] { HttpEntry("GET", "/public"), HttpEntry("GET", "/private") }.ToImmutableArray();

        var insight = new AnonymousEndpointsSource().Compute(model, EmptyGraph, entries).Single();

        Assert.Contains("1/2 endpoints anonymous", insight.Title);
        Assert.Contains("GET /public", insight.Evidence);
        Assert.Contains("not individually verifiable", insight.Title);
    }

    [Fact]
    public void NoGlobalFallback_NoPerEndpointAuth_StillReportsAnonymous()
    {
        // Regression: without any global signal, absence of auth metadata is the only evidence we
        // have, so the original "anonymous" claim is honest and must be preserved.
        var model = new DiscoveryModel();
        model.Detections.Add(Endpoint("DELETE", "/orders/{id}"));

        var entries = new[] { HttpEntry("DELETE", "/orders/{id}") }.ToImmutableArray();

        var insight = new AnonymousEndpointsSource().Compute(model, EmptyGraph, entries).Single();

        Assert.Contains("anonymous", insight.Title, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("POST/PUT/DELETE", insight.Title);
    }

    [Fact]
    public void FullyAuthenticated_NoGlobalFallback_YieldsNoInsight()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(Endpoint("GET", "/orders", "[Authorize]"));

        var entries = new[] { HttpEntry("GET", "/orders") }.ToImmutableArray();

        Assert.Empty(new AnonymousEndpointsSource().Compute(model, EmptyGraph, entries));
    }
}
