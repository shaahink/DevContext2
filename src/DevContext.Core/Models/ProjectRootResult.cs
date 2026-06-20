namespace DevContext.Core.Models;

/// <summary>Result of resolving a project root directory from a user-provided input path.</summary>
public sealed record ProjectRootResult(
    string RootPath,
    string? SolutionFilePath,
    ImmutableArray<string> EntryCandidates,
    ResolutionMethod Method,
    string? ResolutionNote,
    ImmutableArray<string> ScopeProjectDirs = default,
    string? AnchorProjectPath = null
);

/// <summary>Describes how the project root was resolved from the input path.</summary>
public enum ResolutionMethod
{
    ExplicitSln,
    ExplicitCsproj,
    DirectoryContainsSln,
    WalkedUp,
    WalkedDown,
    FolderMode
}
