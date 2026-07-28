namespace DevContext.Server.Sessions;

public sealed class AnalysisSession(string handle, EngineResult engine) : IAsyncDisposable
{
    private GraphQuery? _query;

    public string Handle { get; } = handle;
    public EngineResult Engine { get; } = engine;
    public AnalysisSnapshot Snapshot => Engine.Snapshot;

    // M3.1 — session metadata for server-of-record
    public string RepoPath { get; init; } = "";
    public string CommitSha { get; init; } = "";
    /// <summary>R3 D-D — the solution this session was scoped to (<c>--sln</c>), or null for the
    /// scored default. Part of the session's IDENTITY: one repo at one HEAD has as many honest
    /// analyses as it declares solutions, which is the rule the snapshot cache already keys on.</summary>
    public string? Sln { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public int CallCount { get; set; }
    public long TokenTotal { get; set; }

    // G4 — repo root for path relativization (D4)
    public string RepoRoot => RepoPath;

    public GraphQuery Query => _query ??= new GraphQuery(Snapshot.Graph!, Snapshot.Entries, Snapshot.Map);

    // T3.4 — the config-key scan reads + syntax-parses every node-bearing file (10.5s on shamshir), so
    // it is computed once per session and reused. config() filters this list in-memory (≤500ms warm).
    private IReadOnlyList<ConfigBindingInfo>? _configBindings;
    public IReadOnlyList<ConfigBindingInfo> ConfigBindings() => _configBindings ??= ConfigScanner.Scan(Snapshot.Graph!);

    // T7.4 (audit B11) — the flow atlas (≤100 shallow traces + hub degrees) computed once per
    // session; the app used to re-derive it with ~100 GetTrace + ~10 GetNode RPCs on every boot.
    private FlowIndexResult? _flowIndex;
    public FlowIndexResult FlowIndex() => _flowIndex ??= FlowIndexBuilder.Build(Query, Snapshot.Entries);

    public async Task<string> RenderMapMarkdownAsync(CancellationToken ct)
    {
        var rendered = await Engine.Pipeline
            .RenderAsync(Snapshot, BuildRequest(entry: null, depth: null, TraceDetail.Salient), ct)
            .ConfigureAwait(false);
        return rendered.Content;
    }

    /// <summary>Batch E — renders the markdown for a trace the caller ALREADY built. Passing the built
    /// trace in is what makes GetTrace one walk instead of two, and it is what makes the tree it returns
    /// and the document it returns the same trace rather than two independently-shaped ones.</summary>
    public async Task<string> RenderTraceMarkdownAsync(string focus, int depth, TraceDetail detail,
        Core.Graph.Trace? prebuilt, CancellationToken ct)
    {
        var rendered = await Engine.Pipeline
            .RenderAsync(Snapshot, BuildRequest(focus, depth, detail) with { PrebuiltTrace = prebuilt }, ct)
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
