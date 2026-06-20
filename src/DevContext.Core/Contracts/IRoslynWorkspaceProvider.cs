namespace DevContext.Core.Contracts;

/// <summary>Provides access to a Roslyn workspace for semantic analysis of projects and documents.</summary>
public interface IRoslynWorkspace
{
    /// <summary>Gets the list of projects in the workspace.</summary>
    Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(CancellationToken ct);
    /// <summary>Gets the list of documents in the workspace.</summary>
    Task<ImmutableArray<DocumentInfo>> GetDocumentsAsync(CancellationToken ct);
}

/// <summary>Information about a project discovered in the workspace.</summary>
public sealed record ProjectInfo(
    string Name,
    string FilePath,
    string Language,
    ImmutableArray<string> TargetFrameworks,
    ImmutableArray<string> ProjectReferences,
    ImmutableArray<PackageReferenceInfo> PackageReferences,
    string? OutputType = null,
    bool IsPackable = false
);

/// <summary>Information about a NuGet package reference.</summary>
public sealed record PackageReferenceInfo(
    string Name,
    string Version
);

/// <summary>Information about a document within a project.</summary>
public sealed record DocumentInfo(
    string FilePath,
    string? DocumentCategory
);

/// <summary>Factory for creating a Roslyn workspace from a given context.</summary>
public interface IRoslynWorkspaceProvider
{
    /// <summary>Creates or retrieves a Roslyn workspace instance for analysis.</summary>
    Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct);
}
