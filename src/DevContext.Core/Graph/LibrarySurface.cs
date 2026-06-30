namespace DevContext.Core.Graph;

/// <summary>
/// The capability-grouped public API of a <see cref="Archetype.Library"/> — what a Map's entry-point
/// inventory is to an app (assessment G3, design §4). Built by <see cref="LibrarySurfaceBuilder"/> over
/// the library's public types/methods; rendered by <c>LibrarySurfaceRenderer</c>.
/// </summary>
public sealed record LibrarySurface(
    ImmutableArray<SurfaceGroup> Groups,
    ImmutableArray<string> ExtensionPoints)
{
    /// <summary>Ranked "how do I use this" front doors — extension front-doors, abstract seats, key types.</summary>
    public ImmutableArray<SurfaceEntry> EntryApi { get; init; } = [];
    /// <summary>Interfaces / base classes consumers implement or derive, with in-repo implementor counts.</summary>
    public ImmutableArray<SurfaceAbstraction> Abstractions { get; init; } = [];
    /// <summary>Internal-by-convention namespaces (e.g. <c>*.Internal</c>) demoted out of the main surface.</summary>
    public ImmutableArray<SurfaceGroup> Internals { get; init; } = [];
    /// <summary>Deterministic usage recipes derived from the entries and seats.</summary>
    public ImmutableArray<string> ConsumerPaths { get; init; } = [];
    /// <summary>Roslyn tooling this library ships — source generators / analyzers / code fixers.</summary>
    public ImmutableArray<SurfaceGenerator> Generators { get; init; } = [];
    /// <summary>Runtime NuGet packages (test / benchmark / sample project deps excluded).</summary>
    public ImmutableArray<PackageGroup> Packages { get; init; } = [];
}

/// <summary>Public types grouped by namespace.</summary>
public sealed record SurfaceGroup(string Namespace, ImmutableArray<SurfaceType> Types);

/// <summary>One public type and its public members.</summary>
public sealed record SurfaceType(string Name, TypeKind Kind, ImmutableArray<string> Members)
{
    /// <summary>The type's XML doc <c>&lt;summary&gt;</c> one-liner, if any.</summary>
    public string? Doc { get; init; }
}

/// <summary>A ranked entry-API front door (extension front-door, abstract seat, or key type).</summary>
public sealed record SurfaceEntry(string Title, string Kind, string? Doc, string? Location);

/// <summary>An interface or base class consumers implement or derive, with its in-repo implementor count.</summary>
public sealed record SurfaceAbstraction(string Name, TypeKind Kind, int ImplementorCount);

/// <summary>A Roslyn tooling type the library ships: a source generator, analyzer, or code fixer.</summary>
public sealed record SurfaceGenerator(string Name, string Kind, string? Doc);
