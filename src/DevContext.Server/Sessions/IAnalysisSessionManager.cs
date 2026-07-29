namespace DevContext.Server.Sessions;

public interface IAnalysisSessionManager
{
    Task<AnalysisOutcome> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct);

    AnalysisSession? Get(string handle);

    Task<bool> CloseSessionAsync(string handle);

    // M3.1 — server-of-record: repo+HEAD keyed lookup. R3 D-D adds the analyzed solution, because a
    // multi-solution repo has one honest analysis per solution and they must not answer for each other.
    AnalysisSession? TryGetByRepo(string repoPath, string commitSha, string? sln = null);

    // M3.1 — session list for MCP page
    IReadOnlyList<AnalysisSession> ListSessions();
}
