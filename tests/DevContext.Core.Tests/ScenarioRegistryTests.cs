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
    public void DeepDiveScenario_DisplayName_IsSlice()
    {
        var scenario = ScenarioRegistry.BuiltIn["deep-dive"];
        Assert.Equal("Slice", scenario.DisplayName);
    }

    [Fact]
    public void DeepDiveScenario_default_pruning_config_is_pinned()
    {
        var p = ScenarioRegistry.BuiltIn["deep-dive"].Pruning;
        Assert.Equal(3, p.MaxPathDistance);
        Assert.Equal(5, p.MaxCallDepth);
        Assert.Equal(25, p.MaxSurvivingTypes);
        Assert.Equal(0.35, p.RoleWeight);
        Assert.Equal(0.65, p.FocusWeight);
    }

    [Fact]
    public void OverviewScenario_default_pruning_config_is_pinned()
    {
        var p = ScenarioRegistry.BuiltIn["overview"].Pruning;
        Assert.Equal(2, p.MaxPathDistance);
        Assert.Equal(40, p.MaxSurvivingTypes);
        Assert.Equal(0.7, p.RoleWeight);
        Assert.Equal(0.3, p.FocusWeight);
    }
}
