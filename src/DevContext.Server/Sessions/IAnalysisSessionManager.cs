namespace DevContext.Server.Sessions;

public interface IAnalysisSessionManager
{
    Task<AnalysisSession> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct);

    AnalysisSession? Get(string handle);

    bool CloseSession(string handle);
}
