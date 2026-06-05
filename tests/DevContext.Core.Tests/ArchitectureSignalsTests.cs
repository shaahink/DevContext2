namespace DevContext.Core.Tests;

public sealed class ArchitectureSignalsTests
{
    [Fact]
    public void Has_ReturnsFalse_ForUnknownKey()
    {
        var signals = new ArchitectureSignals();
        Assert.False(signals.Has("nonexistent"));
    }

    [Fact]
    public void Get_ReturnsNull_ForUnknownKey()
    {
        var signals = new ArchitectureSignals();
        Assert.Null(signals.Get("nonexistent"));
    }

    [Fact]
    public void All_ReflectsRegisteredSignals()
    {
        var signals = new ArchitectureSignals();
        signals.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));
        signals.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore));

        Assert.Equal(2, signals.All.Count);
    }

    [Fact]
    public void Keys_Constants_AreCorrect()
    {
        Assert.Equal("minimal-apis", ArchitectureSignals.Keys.MinimalApis);
        Assert.Equal("controllers", ArchitectureSignals.Keys.Controllers);
        Assert.Equal("mediatr", ArchitectureSignals.Keys.MediatR);
        Assert.Equal("efcore", ArchitectureSignals.Keys.EfCore);
    }
}
