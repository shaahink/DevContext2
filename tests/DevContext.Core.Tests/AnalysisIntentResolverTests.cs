namespace DevContext.Core.Tests;

public sealed class AnalysisIntentResolverTests
{
    [Fact]
    public void NoFocus_ReturnsOverview()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput());
        Assert.Equal("overview", result.Scenario.Name);
        Assert.Equal(ExtractionProfile.Focused, result.Profile);
        Assert.Empty(result.FocusPoints);
        Assert.Contains("Overview map", result.Explanation);
    }

    [Fact]
    public void WithFocus_ReturnsDeepDive()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "MyService" });
        Assert.Equal("deep-dive", result.Scenario.Name);
        Assert.Equal(ExtractionProfile.Debug, result.Profile);
        Assert.Single(result.FocusPoints);
        Assert.Equal(FocusKind.Type, result.FocusPoints[0].Kind);
        Assert.Contains("MyService", result.Explanation);
    }

    [Fact]
    public void TypeMethodFocus_ParsesCorrectly()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "MyService:ProcessOrder" });
        Assert.Single(result.FocusPoints);
        Assert.Equal(FocusKind.Method, result.FocusPoints[0].Kind);
        Assert.Equal("MyService", result.FocusPoints[0].TypeName);
        Assert.Equal("ProcessOrder", result.FocusPoints[0].MethodName);
    }

    [Fact]
    public void EndpointFocus_WithVerb_ParsesCorrectly()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "GET /api/orders" });
        Assert.Single(result.FocusPoints);
        Assert.Equal(FocusKind.Endpoint, result.FocusPoints[0].Kind);
        Assert.Equal("GET", result.FocusPoints[0].HttpMethod);
        Assert.Equal("/api/orders", result.FocusPoints[0].Route);
    }

    [Fact]
    public void EndpointFocus_BareRoute_ParsesCorrectly()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "/api/products" });
        Assert.Single(result.FocusPoints);
        Assert.Equal(FocusKind.Endpoint, result.FocusPoints[0].Kind);
        Assert.Null(result.FocusPoints[0].HttpMethod);
        Assert.Equal("/api/products", result.FocusPoints[0].Route);
    }

    [Fact]
    public void EndpointFocus_RouteWithSlashButNoLeadingSlash_GetsNormalized()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "api/orders" });
        Assert.Single(result.FocusPoints);
        Assert.Equal("/api/orders", result.FocusPoints[0].Route);
    }

    [Fact]
    public void TraceAlias_MapsToDeepDive()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { ExplicitScenario = "trace", Focus = "Foo" });
        Assert.Equal("deep-dive", result.Scenario.Name);
    }

    [Fact]
    public void AuditAlias_ProducesWarning()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { ExplicitScenario = "audit" });
        Assert.Equal("overview", result.Scenario.Name);
        Assert.Contains(result.Warnings, w => w.Contains("deprecated"));
    }

    [Fact]
    public void DepthOverridesCallDepth()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "Foo", Depth = 7 });
        Assert.Equal(7, result.Scenario.Pruning.MaxCallDepth);
        Assert.Equal(2, result.Scenario.Pruning.MaxPathDistance); // depth > 2 => 2
    }

    [Fact]
    public void ShallowDepth_ClampsPathDistance()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "Foo", Depth = 1 });
        Assert.Equal(1, result.Scenario.Pruning.MaxCallDepth);
        Assert.Equal(1, result.Scenario.Pruning.MaxPathDistance); // depth <= 2 => 1
    }

    [Fact]
    public void ExplicitProfile_IsRespected()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { ExplicitProfile = "full" });
        Assert.Equal(ExtractionProfile.Full, result.Profile);
    }

    [Fact]
    public void ExplicitScenario_DeepDiveNoFocus_ProducesWarning()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { ExplicitScenario = "deep-dive" });
        Assert.Contains(result.Warnings, w => w.Contains("deep-dive without --focus"));
    }

    [Fact]
    public void UnknownExplicitScenario_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            AnalysisIntentResolver.Resolve(new IntentInput { ExplicitScenario = "nonsense" }));
    }

    [Fact]
    public void DepthClamped_Below1()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "Foo", Depth = 0 });
        Assert.Equal(1, result.Scenario.Pruning.MaxCallDepth);
    }

    [Fact]
    public void DepthClamped_Above10()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "Foo", Depth = 20 });
        Assert.Equal(10, result.Scenario.Pruning.MaxCallDepth);
    }

    [Fact]
    public void PostEndpoint_VerbParsed()
    {
        var result = AnalysisIntentResolver.Resolve(new IntentInput { Focus = "POST /api/users" });
        Assert.Equal("POST", result.FocusPoints[0].HttpMethod);
        Assert.Equal("/api/users", result.FocusPoints[0].Route);
    }
}
