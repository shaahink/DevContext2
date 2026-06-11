namespace DevContext.Core.Tests;

public sealed class ArchitectureStyleDetectorTests
{
    private static ProjectInfo Project(string name) =>
        new(name, $"{name}.csproj", "C#", [], [], []);

    [Fact]
    public void ControllerBased_when_controllers_stronger_than_minimal_apis()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Controllers, 0.9f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Projects = [Project("WebApp")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.ControllerBased, style);
    }

    [Fact]
    public void MinimalApi_when_no_controllers_signal()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Projects = [Project("WebApp")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.MinimalApi, style);
    }

    [Fact]
    public void NLayer_when_efcore_and_multi_project()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects = [Project("Web"), Project("Core"), Project("Infrastructure")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.NLayer, style);
    }

    [Fact]
    public void CleanArchitecture_when_mediatr_and_named_layers()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Projects = [
            Project("MyApp.Domain"),
            Project("MyApp.Application"),
            Project("MyApp.Infrastructure")
        ];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.CleanArchitecture, style);
    }
}
