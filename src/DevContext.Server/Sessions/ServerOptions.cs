namespace DevContext.Server.Sessions;

public sealed record ServerOptions
{
    public string Urls { get; init; } = "http://127.0.0.1:5179";
    public int SessionCapacity { get; init; } = 5;
    public TimeSpan SessionIdleTimeout { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>Where THIS server persists analysis snapshots. Null keeps the process-wide default
    /// (DEVCONTEXT_CACHE_ROOT if set, else the per-user cache) — the env override still exists and
    /// still works; this is an owned second way in.
    ///
    /// It exists because an environment variable cannot be owned by two hosts in one process, and
    /// the server test assembly runs five xUnit collections concurrently that each wanted their own
    /// cache root. Whoever constructed last won, so an analysis could persist into a neighbour's
    /// root — measured 2026-07-29 (G5 s18) as a 5-in-15 failure rate on
    /// AnalyzeCacheTruthTests.A_rehydrate_reports_the_originals_instant_not_its_own, which then
    /// found nothing in the root it owned. A host that is handed its root cannot lose it.</summary>
    public string? SnapshotCacheRoot { get; init; }
}
