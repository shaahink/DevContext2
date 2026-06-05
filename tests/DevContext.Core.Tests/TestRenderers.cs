namespace DevContext.Core.Tests;

public sealed class TestMarkdownRenderer : IContextRenderer
{
    public string Format => "markdown";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Test Analysis");
        if (model.Solution != null)
            sb.AppendLine($"Solution: {model.Solution.Name}");
        sb.AppendLine($"Types: {model.Types.Count}");
        sb.AppendLine($"Detections: {model.Detections.Count}");

        var estimatedTokens = sb.Length / 4;
        return new ValueTask<RenderedContext>(new RenderedContext(sb.ToString(), estimatedTokens, [], TimeSpan.Zero, "2.0"));
    }
}

public sealed class TestJsonRenderer : IContextRenderer
{
    public string Format => "json";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new { types = model.Types.Count, detections = model.Detections.Count });
        return new ValueTask<RenderedContext>(new RenderedContext(json, json.Length / 4, [], TimeSpan.Zero, "2.0"));
    }
}
