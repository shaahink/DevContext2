namespace DevContext.Core.Tests;

public sealed class IntentInferrerTests
{
    [Theory]
    [InlineData("debug why is this endpoint failing", "debug-endpoint")]
    [InlineData("add new crud endpoint for orders", "add-similar-feature")]
    [InlineData("middleware pipeline interceptor", "modify-middleware")]
    [InlineData("event message publish bus", "trace-message-flow")]
    [InlineData("architecture overview structure", "architecture")]
    [InlineData("di injection reflection", "harden-di")]
    public void Infer_KeywordMatching_ReturnsExpectedScenario(string task, string expected)
    {
        var (scenario, _) = IntentInferrer.Infer(task);
        Assert.Equal(expected, scenario);
    }

    [Fact]
    public void Infer_NoMatch_ReturnsArchitecture()
    {
        var (scenario, _) = IntentInferrer.Infer("zxyzwqv");
        Assert.Equal("architecture", scenario);
    }
}
