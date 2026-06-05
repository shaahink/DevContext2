namespace DevContext.Core.Contracts;

public interface IRoslynWorkspace
{
    Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(CancellationToken ct);
    Task<ImmutableArray<DocumentInfo>> GetDocumentsAsync(CancellationToken ct);
}

public sealed record ProjectInfo(
    string Name,
    string FilePath,
    string Language,
    ImmutableArray<string> TargetFrameworks,
    ImmutableArray<string> ProjectReferences,
    ImmutableArray<PackageReferenceInfo> PackageReferences
);

public sealed record PackageReferenceInfo(
    string Name,
    string Version
);

public sealed record DocumentInfo(
    string FilePath,
    string? DocumentCategory
);

public interface IRoslynWorkspaceProvider
{
    Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct);
}
