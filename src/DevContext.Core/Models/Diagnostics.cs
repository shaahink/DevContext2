namespace DevContext.Core.Models;

/// <summary>Severity level of a diagnostic entry.</summary>
public enum DiagnosticLevel
{
    Info,
    Warning,
    Error
}

/// <summary>Represents a diagnostic entry recorded during pipeline execution.</summary>
public sealed record DiagnosticEntry(
    DiagnosticLevel Level,
    string Source,
    string Message,
    DateTimeOffset Timestamp
);
