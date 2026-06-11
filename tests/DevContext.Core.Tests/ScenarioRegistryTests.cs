namespace DevContext.Core.Tests;

public sealed class ScenarioRegistryTests
{
    [Fact]
    public void BuiltIn_ContainsAllScenarios()
    {
        Assert.Equal(2, ScenarioRegistry.BuiltIn.Count);
        Assert.Contains("overview", ScenarioRegistry.BuiltIn.Keys);
        Assert.Contains("deep-dive", ScenarioRegistry.BuiltIn.Keys);
    }

    [Fact]
    public void OverviewScenario_HasAllSections()
    {
        var scenario = ScenarioRegistry.BuiltIn["overview"];
        Assert.Equal("overview", scenario.Name);
        Assert.Equal(7, scenario.RequiredSections.Length);
        Assert.Contains("Architecture overview", scenario.RequiredSections);
        Assert.Contains("Related types", scenario.RequiredSections);
        Assert.Contains("Endpoints", scenario.RequiredSections);
    }

    [Fact]
    public void DeepDiveScenario_UsesAggressiveTruncation()
    {
        var scenario = ScenarioRegistry.BuiltIn["deep-dive"];
        Assert.True(scenario.Compression.AggressiveTruncation);
    }

    [Fact]
    public void DeepDiveScenario_DisplayName_IsTrace()
    {
        var scenario = ScenarioRegistry.BuiltIn["deep-dive"];
        Assert.Equal("Trace", scenario.DisplayName);
    }
}
