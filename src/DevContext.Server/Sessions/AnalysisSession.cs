namespace DevContext.Server.Sessions;

public sealed class AnalysisSession(string handle, EngineResult engine) : IAsyncDisposable
{
    private GraphQuery? _query;

    public string Handle { get; } = handle;
    public EngineResult Engine { get; } = engine;
    public AnalysisSnapshot Snapshot => Engine.Snapshot;

    public GraphQuery Query => _query ??= new GraphQuery(Snapshot.Graph!, Snapshot.Entries, Snapshot.Map);

    public async Task<string> RenderMapMarkdownAsync(CancellationToken ct)
    {
        var rendered = await Engine.Pipeline
            .RenderAsync(Snapshot, BuildRequest(entry: null, depth: null, TraceDetail.Salient), ct)
            .ConfigureAwait(false);
        return rendered.Content;
    }

    public async Task<string> RenderTraceMarkdownAsync(string focus, int depth, TraceDetail detail, CancellationToken ct)
    {
        var rendered = await Engine.Pipeline
            .RenderAsync(Snapshot, BuildRequest(focus, depth, detail), ct)
            .ConfigureAwait(false);
        return rendered.Content;
    }

    public async Task<RenderedContext> RenderAsync(string? focus, int? depth, TraceDetail detail,
        string format, ImmutableArray<string> sections, bool includeDiagnostics, CancellationToken ct)
    {
        var req = new RenderRequest
        {
            Format = format,
            MaxTokens = Snapshot.Options.MaxOutputTokens,
            Sections = sections.IsDefaultOrEmpty ? Snapshot.Scenario.RequiredSections : sections,
            Entry = focus,
            Depth = depth,
            Detail = detail,
            IncludeDiagnostics = includeDiagnostics,
        };
        return await Engine.Pipeline.RenderAsync(Snapshot, req, ct).ConfigureAwait(false);
    }

    private RenderRequest BuildRequest(string? entry, int? depth, TraceDetail detail) => new()
    {
        Format = "markdown",
        MaxTokens = Snapshot.Options.MaxOutputTokens,
        Sections = Snapshot.Scenario.RequiredSections,
        Entry = entry,
        Depth = depth,
        Detail = detail,
    };

    public async ValueTask DisposeAsync()
    {
        if (Engine.Cleanup == "keep") return;
        if (Engine.GitClonePath is { } clone)
            GitCloneService.Cleanup(clone);
    }
}
