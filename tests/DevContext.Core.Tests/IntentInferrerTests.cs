namespace DevContext.Core.Tests;

public sealed class IntentInferrerTests
{
    [Theory]
    [InlineData("debug why is this endpoint failing", "deep-dive")]
    [InlineData("add new crud endpoint for orders", "overview")]
    [InlineData("architecture overview structure", "overview")]
    [InlineData("di injection reflection", "audit")]
    [InlineData("middleware pipeline interceptor", "audit")]
    [InlineData("event message publish bus", "deep-dive")]
    [InlineData("trace call graph", "deep-dive")]
    public void Infer_KeywordMatching_ReturnsExpectedScenario(string task, string expected)
    {
        var (scenario, _) = IntentInferrer.Infer(task);
        Assert.Equal(expected, scenario);
    }

    [Fact]
    public void Infer_NoMatch_ReturnsOverview()
    {
        var (scenario, _) = IntentInferrer.Infer("zxyzwqv");
        Assert.Equal("overview", scenario);
    }
}
