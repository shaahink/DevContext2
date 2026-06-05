namespace DevContext.Core.Models;

public sealed record ProjectRootResult(
    string RootPath,
    string? SolutionFilePath,
    ImmutableArray<string> EntryCandidates,
    ResolutionMethod Method,
    string? ResolutionNote
);

public enum ResolutionMethod
{
    ExplicitSln,
    ExplicitCsproj,
    DirectoryContainsSln,
    WalkedUp,
    WalkedDown,
    FolderMode
}
