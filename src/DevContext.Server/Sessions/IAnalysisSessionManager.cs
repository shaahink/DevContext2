namespace DevContext.Server.Sessions;

public interface IAnalysisSessionManager
{
    Task<AnalysisSession> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct);

    AnalysisSession? Get(string handle);

    Task<bool> CloseSessionAsync(string handle);

    // M3.1 — server-of-record: repo+HEAD keyed lookup
    AnalysisSession? TryGetByRepo(string repoPath, string commitSha);

    // M3.1 — session list for MCP page
    IReadOnlyList<AnalysisSession> ListSessions();
}
