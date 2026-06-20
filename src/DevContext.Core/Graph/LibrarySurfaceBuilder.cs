namespace DevContext.Core.Graph;

/// <summary>Builds the <see cref="LibrarySurface"/> from a library's public types/methods — namespace/
/// type grouping first, with builder/DI-style extension methods surfaced as extension points. Test
/// projects are excluded, consistent with the graph's <see cref="NoiseFilter"/>.</summary>
public static class LibrarySurfaceBuilder
{
    private const int MaxMembersPerType = 15;
    private static readonly string[] ExtensionVerbs =
        ["Add", "Use", "Register", "With", "Configure", "Map"];

    public static LibrarySurface Build(DiscoveryModel model)
    {
        var classifier = new ProjectClassifier(model.Projects);
        var publicTypes = model.Types.Values
            .Where(t => t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
            .Where(t => !classifier.IsInTestProject(t.FilePath))
            .ToList();

        var groups = publicTypes
            .GroupBy(t => t.Namespace)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new SurfaceGroup(g.Key,
                [.. g.OrderBy(t => t.Name, StringComparer.Ordinal).Select(ToSurfaceType)]))
            .ToImmutableArray();

        var extensionPoints = publicTypes
            .SelectMany(t => PublicMethods(t)
                .Where(m => m.IsStatic && ExtensionVerbs.Any(v =>
                    m.Name.StartsWith(v, StringComparison.Ordinal)))
                .Select(m => $"{t.Name}.{m.Name}"))
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToImmutableArray();

        return new LibrarySurface(groups, extensionPoints);
    }

    private static SurfaceType ToSurfaceType(TypeDiscovery t)
        => new(t.Name, t.Kind,
            [.. PublicMethods(t).Select(m => m.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal).Take(MaxMembersPerType)]);

    private static IEnumerable<MethodSignature> PublicMethods(TypeDiscovery t)
        => t.Methods.Where(m =>
            m.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public
            && !m.Name.StartsWith('.')                              // ctors / static ctors
            && !m.Name.StartsWith("get_", StringComparison.Ordinal)
            && !m.Name.StartsWith("set_", StringComparison.Ordinal)
            && !m.Name.StartsWith("add_", StringComparison.Ordinal)
            && !m.Name.StartsWith("remove_", StringComparison.Ordinal));
}
