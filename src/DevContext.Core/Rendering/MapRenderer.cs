using DevContext.Core.Graph;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Rendering;

/// <summary>Input for the Map renderer — the snapshot-derived map model plus the user's render request.</summary>
public sealed record MapRenderContext(
    MapModel Map,
    AnalysisSnapshot Snapshot,
    string Format,
    RenderRequest Request);

/// <summary>Renders the <see cref="MapModel"/> as markdown, HTML, or JSON (PLAN-10 B4).</summary>
public sealed class MapRenderer
{
    /// <summary>Renders the map in the IDEAL-OUTPUT-TARGET §3 layout.</summary>
    public ValueTask<RenderedContext> RenderAsync(MapRenderContext ctx, CancellationToken ct)
        => throw new NotImplementedException("MapRenderer stub — fill in Part B4");
}
