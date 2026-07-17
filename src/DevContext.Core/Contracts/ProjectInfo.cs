namespace DevContext.Core.Contracts;

/// <summary>Information about a project discovered in the workspace.</summary>
public sealed record ProjectInfo(
    string Name,
    string FilePath,
    string Language,
    ImmutableArray<string> TargetFrameworks,
    ImmutableArray<string> ProjectReferences,
    ImmutableArray<PackageReferenceInfo> PackageReferences,
    string? OutputType = null,
    bool IsPackable = false,
    string? Sdk = null,
    bool IsToolPackaged = false
);

/// <summary>Information about a NuGet package reference.</summary>
public sealed record PackageReferenceInfo(
    string Name,
    string Version
);
