namespace DevContext.Server.Sessions;

/// <summary>What the caller asked us to analyze. The transport layer builds this from the gRPC
/// request; the engine layer turns it into the canonical DevContext analyze recipe.</summary>
public sealed record AnalyzeSpec(string Path, string? Focus, int? Depth, string? Detail, bool NoRoslyn);

/// <summary>A coarse progress tick streamed back while the engine runs.</summary>
public sealed record AnalysisProgress(string Stage, double Percent, string Message);

/// <summary>A failure the user can act on (bad path, git not installed, private repo). Carries a
/// stable <see cref="Code"/> so the transport layer can surface it without string-matching.</summary>
public sealed class AnalysisException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
