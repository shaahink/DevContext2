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
    bool IsToolPackaged = false,
    ImmutableArray<string> Sdks = default,
    bool UsesWpf = false,
    bool UsesWinForms = false
)
{
    /// <summary>Batch D (R2 §2.D) — every SDK the csproj declares (root attribute, <c>&lt;Sdk Name&gt;</c>
    /// elements, and SDK-style <c>&lt;Import&gt;</c>), version suffixes stripped. This is the ONE SDK
    /// vocabulary: the scalar <c>Sdk</c> (root attribute only) that used to sit beside it is gone, because
    /// a record with two SDK fields lets a caller populate one and leave the other empty — which is
    /// precisely how the holder-project rule and the runnable-service rule could disagree about the same
    /// csproj. A snapshot persisted before this field existed rehydrates as EMPTY, never as a default
    /// (unusable) array — the schema bump is what stops a stale cache reading as "declares no SDK".</summary>
    public ImmutableArray<string> Sdks { get; init; } = Sdks.IsDefault ? [] : Sdks;

    /// <summary>True when the project declares exactly this SDK id (case-insensitive). Equality, not
    /// substring: the old <c>text.Contains("Microsoft.NET.Sdk.Web")</c> probe answered yes for a csproj
    /// that merely MENTIONED the SDK, and a substring rule here would let the base
    /// <c>Microsoft.NET.Sdk</c> answer the Web SDK's question. Version suffixes are already stripped by
    /// <see cref="Resolvers.CsprojReader.ParseSdks"/>, so <c>Microsoft.Build.NoTargets/3.3.0</c> matches
    /// <c>Microsoft.Build.NoTargets</c>.</summary>
    public bool HasSdk(string sdkId)
    {
        foreach (var sdk in Sdks)
            if (sdk.Equals(sdkId, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
}

/// <summary>Information about a NuGet package reference.</summary>
public sealed record PackageReferenceInfo(
    string Name,
    string Version
);
