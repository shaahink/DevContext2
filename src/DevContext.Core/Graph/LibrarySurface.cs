namespace DevContext.Core.Graph;

/// <summary>
/// The capability-grouped public API of a <see cref="Archetype.Library"/> — what a Map's entry-point
/// inventory is to an app (assessment G3, design §4). Built by <see cref="LibrarySurfaceBuilder"/> over
/// the library's public types/methods; rendered by <c>LibrarySurfaceRenderer</c>.
/// </summary>
public sealed record LibrarySurface(
    ImmutableArray<SurfaceGroup> Groups,
    ImmutableArray<string> ExtensionPoints);

/// <summary>Public types grouped by namespace.</summary>
public sealed record SurfaceGroup(string Namespace, ImmutableArray<SurfaceType> Types);

/// <summary>One public type and its public members.</summary>
public sealed record SurfaceType(string Name, TypeKind Kind, ImmutableArray<string> Members);
