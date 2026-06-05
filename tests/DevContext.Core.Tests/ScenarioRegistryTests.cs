namespace DevContext.Core.Tests;

public sealed class ScenarioRegistryTests
{
    [Fact]
    public void BuiltIn_ContainsAllScenarios()
    {
        Assert.Equal(6, ScenarioRegistry.BuiltIn.Count);
        Assert.Contains("architecture", ScenarioRegistry.BuiltIn.Keys);
        Assert.Contains("debug-endpoint", ScenarioRegistry.BuiltIn.Keys);
        Assert.Contains("add-similar-feature", ScenarioRegistry.BuiltIn.Keys);
        Assert.Contains("modify-middleware", ScenarioRegistry.BuiltIn.Keys);
        Assert.Contains("trace-message-flow", ScenarioRegistry.BuiltIn.Keys);
        Assert.Contains("harden-di", ScenarioRegistry.BuiltIn.Keys);
    }

    [Fact]
    public void ArchitectureScenario_HasCorrectRequiredSections()
    {
        var scenario = ScenarioRegistry.BuiltIn["architecture"];
        Assert.Contains("Architecture overview", scenario.RequiredSections);
        Assert.Contains("Related types", scenario.RequiredSections);
    }

    [Fact]
    public void DebugEndpointScenario_UsesAggressiveTruncation()
    {
        var scenario = ScenarioRegistry.BuiltIn["debug-endpoint"];
        Assert.True(scenario.Compression.AggressiveTruncation);
    }
}
