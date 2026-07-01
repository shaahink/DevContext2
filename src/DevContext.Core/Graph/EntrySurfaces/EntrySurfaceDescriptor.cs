namespace DevContext.Core.Graph.EntrySurfaces;

/// <summary>The role an entry surface plays in archetype classification.</summary>
public enum SurfaceRole
{
    /// <summary>This entry surface counts toward the App archetype — e.g. HTTP, gRPC, Blazor.</summary>
    AppEntry,
    /// <summary>When self-sourced, this signal means the repo IS the framework/library.</summary>
    FrameworkLibrary,
    /// <summary>Overrides App/Library — repo is a reverse proxy gateway.</summary>
    Gateway,
}

/// <summary>
/// A declarative descriptor for one entry surface shape. Encodes the signal key, packages,
/// SDK hints, self-name patterns, archetype role, and render label in one place.
/// Every scattered table (PackageSignalMap, ProjectNameSignalMap, LibraryFrameworkSignals,
/// AppEntryKinds, GroupLabel) becomes a projection of <see cref="EntrySurfaceCatalog.All"/>.
/// </summary>
public sealed record EntrySurfaceDescriptor(
    string            SignalKey,
    EntryPointKind?   Kind,
    string            RenderLabel,
    SurfaceRole       Role,
    ImmutableArray<string> Packages,
    ImmutableArray<string> SdkHints,
    ImmutableArray<string> SelfNamePatterns);
