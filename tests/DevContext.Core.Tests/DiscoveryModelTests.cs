namespace DevContext.Core.Tests;

public sealed class DiscoveryModelTests
{
    [Fact]
    public void AddProvenance_AppendsMultipleReasons()
    {
        var model = new DiscoveryModel();
        model.AddProvenance("item1", new InclusionReason("reason1", "src", 1.0f));
        model.AddProvenance("item1", new InclusionReason("reason2", "src", 0.5f));

        Assert.Equal(2, model.Provenance["item1"].Count);
    }

    [Fact]
    public void AddDiagnostic_Appends()
    {
        var model = new DiscoveryModel();
        model.AddDiagnostic(DiagnosticLevel.Warning, "TestExtractor", "test message");

        var diag = Assert.Single(model.Diagnostics);
        Assert.Equal(DiagnosticLevel.Warning, diag.Level);
        Assert.Equal("TestExtractor", diag.Source);
        Assert.Equal("test message", diag.Message);
    }

    [Fact]
    public async Task Detections_AreThreadSafe()
    {
        var model = new DiscoveryModel();
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            model.Detections.Add(new EndpointDetection("GET", $"/test/{i}", "H", "M", [], [])
            {
                ExtractorName = "Test",
                SourceFile = "test.cs",
                LineNumber = i
            });
        }));

        await Task.WhenAll(tasks);
        Assert.Equal(10, model.Detections.Count);
    }

    [Fact]
    public void DefaultBudget_Is8000()
    {
        var model = new DiscoveryModel();
        Assert.Equal(8000, model.Budget.MaxTokens);
    }
}
