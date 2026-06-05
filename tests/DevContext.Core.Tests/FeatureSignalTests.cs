namespace DevContext.Core.Tests;

public sealed class FeatureSignalTests
{
    [Fact]
    public void CreateDetected_WithKey_SetsProperties()
    {
        var signal = FeatureSignal.CreateDetected("mediatr", 0.9f, "PackageReference", "MediatR 12.0");

        Assert.Equal("mediatr", signal.Key);
        Assert.True(signal.Detected);
        Assert.Equal(0.9f, signal.Confidence);
        Assert.Equal("PackageReference", signal.DetectedVia);
        Assert.Contains("MediatR 12.0", signal.Evidence);
    }

    [Fact]
    public void ArchitectureSignals_RegisterAndRead_Works()
    {
        var signals = new ArchitectureSignals();
        var signal = FeatureSignal.CreateDetected("efcore", 1.0f, "PackageReference", "Microsoft.EntityFrameworkCore");

        signals.Register(signal);

        Assert.True(signals.Has("efcore"));
        Assert.NotNull(signals.Get("efcore"));
    }

    [Fact]
    public void ArchitectureSignals_Seal_PreventsRegistration()
    {
        var signals = new ArchitectureSignals();
        signals.Seal();

        Assert.Throws<InvalidOperationException>(() =>
            signals.Register(FeatureSignal.CreateDetected("test", 1.0f)));
    }

    [Fact]
    public void ArchitectureSignals_HigherConfidence_Wins()
    {
        var signals = new ArchitectureSignals();
        signals.Register(FeatureSignal.CreateDetected("test", 0.5f, "low"));
        signals.Register(FeatureSignal.CreateDetected("test", 0.9f, "high"));

        Assert.Equal("high", signals.Get("test")?.DetectedVia);
    }
}
