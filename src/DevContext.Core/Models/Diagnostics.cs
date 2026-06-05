namespace DevContext.Core.Models;

public enum DiagnosticLevel
{
    Info,
    Warning,
    Error
}

public sealed record DiagnosticEntry(
    DiagnosticLevel Level,
    string Source,
    string Message,
    DateTimeOffset Timestamp
);
