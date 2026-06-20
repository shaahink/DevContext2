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
)
{
    /// <summary>True when a closure scan set was resolved (project/subfolder input under the Hybrid policy).</summary>
    public bool IsScoped => !ScopeProjectDirs.IsDefaultOrEmpty;

    /// <summary>
    /// The root to feed discovery. For a resolved closure we anchor at the <b>solution directory</b> so
    /// the solution/style/topology resolve correctly; <see cref="ScopeProjectDirs"/> then narrows the
    /// source-file walk to the closure. Otherwise the resolved root.
    /// </summary>
    public string EffectiveRootPath =>
        IsScoped && SolutionFilePath is { } sln && Path.GetDirectoryName(sln) is { Length: > 0 } dir
            ? dir
            : RootPath;
}

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
