namespace DevContext.Core.Graph;

/// <summary>
/// Builds the <see cref="LibrarySurface"/> from a library's public types/methods. Produces a ranked
/// <c>ENTRY API</c> (extension front-doors → abstract seats → fluent DSL), the <c>ABSTRACTIONS</c> seats
/// consumers implement/derive, the namespace-grouped public surface (with <c>*.Internal</c> demoted and
/// XML-doc one-liners), deterministic consumer-path recipes, and runtime-only packages. Build-free:
/// everything is derived from syntactic <see cref="TypeDiscovery"/> data (incl. WP1 doc summaries +
/// extension-method flags). Test projects are excluded, consistent with the graph's <see cref="NoiseFilter"/>.
/// </summary>
public static class LibrarySurfaceBuilder
{
    private const int MaxMembersPerType = 15;
    private const int MaxEntryApi = 12;
    private const int MaxAbstractions = 10;
    private const int MaxConsumerPaths = 6;

    private static readonly string[] ExtensionVerbs =
        ["Add", "Use", "Register", "With", "Configure", "Map"];

    // Framework "seat" types whose extension methods are the canonical wire-up front doors.
    private static readonly string[] FrameworkSeats =
        ["IServiceCollection", "IApplicationBuilder", "IEndpointRouteBuilder", "IHostBuilder",
         "IHostApplicationBuilder", "WebApplicationBuilder", "IMvcBuilder", "IServiceProvider"];

    public static LibrarySurface Build(DiscoveryModel model)
    {
        var classifier = new ProjectClassifier(model.Projects);
        var nonLibraryDirs = NonLibraryProjectDirs(model.Projects);
        var publicTypes = model.Types.Values
            .Where(t => t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
            .Where(t => !classifier.IsInTestProject(t.FilePath))
            .Where(t => !ProjectClassifier.IsSamplePath(t.FilePath))
            .Where(t => !IsUnder(nonLibraryDirs, t.FilePath))
            .ToList();

        var mainTypes = publicTypes.Where(t => !IsInternalNamespace(t.Namespace)).ToList();
        var internalTypes = publicTypes.Where(t => IsInternalNamespace(t.Namespace)).ToList();

        var abstractions = BuildAbstractions(model, mainTypes);
        var entryApi = BuildEntryApi(mainTypes, abstractions);

        return new LibrarySurface(GroupByNamespace(mainTypes), BuildExtensionPoints(publicTypes))
        {
            EntryApi = entryApi,
            Abstractions = abstractions,
            Internals = GroupByNamespace(internalTypes),
            ConsumerPaths = BuildConsumerPaths(entryApi),
            Packages = BuildRuntimePackages(model, classifier),
        };
    }

    private static bool IsInternalNamespace(string ns)
        => ns.EndsWith(".Internal", StringComparison.Ordinal)
            || ns.Contains(".Internal.", StringComparison.Ordinal)
            || ns.EndsWith(".Internals", StringComparison.Ordinal);

    private static ImmutableArray<SurfaceGroup> GroupByNamespace(IEnumerable<TypeDiscovery> types) =>
    [
        .. types
            .GroupBy(t => t.Namespace)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new SurfaceGroup(g.Key,
                [.. g.OrderBy(t => t.Name, StringComparer.Ordinal).Select(ToSurfaceType)]))
    ];

    // Real extension methods (this-param) OR the legacy static+verb-prefix heuristic — keeps the
    // existing AutoMapper contract while now catching DSL extensions that lack an Add/Use/With prefix.
    private static ImmutableArray<string> BuildExtensionPoints(IEnumerable<TypeDiscovery> publicTypes) =>
    [
        .. publicTypes
            .SelectMany(t => PublicMethods(t)
                .Where(m => m.IsExtension
                    || (m.IsStatic && ExtensionVerbs.Any(v => m.Name.StartsWith(v, StringComparison.Ordinal))))
                .Select(m => $"{t.Name}.{m.Name}"))
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
    ];

    /// <summary>Interfaces / base classes consumers implement or derive — those with at least one in-repo
    /// implementor (so a library's real seats surface, not its 30 marker interfaces).</summary>
    private static ImmutableArray<SurfaceAbstraction> BuildAbstractions(DiscoveryModel model, List<TypeDiscovery> mainTypes)
    {
        var implCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var t in model.Types.Values)
            foreach (var b in t.BaseTypes.Concat(t.ImplementedInterfaces))
            {
                var key = StripGenerics(b);
                implCounts[key] = implCounts.GetValueOrDefault(key) + 1;
            }

        return
        [
            .. mainTypes
                .Where(t => t.Kind is TypeKind.Interface or TypeKind.Class)
                .Where(t => implCounts.GetValueOrDefault(t.Name) > 0)
                .Select(t => new SurfaceAbstraction(t.Name, t.Kind, implCounts.GetValueOrDefault(t.Name)))
                .OrderByDescending(a => a.ImplementorCount)
                .ThenBy(a => a.Name, StringComparer.Ordinal)
                .Take(MaxAbstractions)
        ];
    }

    /// <summary>Ranked "how do I use this": (0) framework-seat extension front-doors, (1) abstract seats,
    /// (2) fluent-DSL extension classes. Deterministic, weight-free — tiered then name-ordered.</summary>
    private static ImmutableArray<SurfaceEntry> BuildEntryApi(List<TypeDiscovery> mainTypes, ImmutableArray<SurfaceAbstraction> abstractions)
    {
        var ranked = new List<(int Tier, string Sort, SurfaceEntry Entry)>();

        foreach (var t in mainTypes)
            foreach (var m in PublicMethods(t).Where(IsFrameworkFrontDoor))
                ranked.Add((0, $"{t.Name}.{m.Name}",
                    new SurfaceEntry($"{t.Name}.{m.Name}", "register", OneLine(m.XmlDoc), ShortLocation(t.FilePath))));

        foreach (var a in abstractions.Take(4))
        {
            var t = mainTypes.FirstOrDefault(x => x.Name == a.Name);
            var kind = a.Kind == TypeKind.Interface ? "implement" : "derive";
            ranked.Add((1, a.Name, new SurfaceEntry(a.Name, kind, OneLine(t?.XmlDoc),
                t is null ? null : ShortLocation(t.FilePath))));
        }

        foreach (var t in mainTypes)
            if (PublicMethods(t).Any(m => m.IsExtension && !IsFrameworkFrontDoor(m)))
                ranked.Add((2, t.Name, new SurfaceEntry(t.Name, "extend", OneLine(t.XmlDoc), ShortLocation(t.FilePath))));

        return
        [
            .. ranked
                .GroupBy(e => e.Entry.Title, StringComparer.Ordinal)
                .Select(g => g.OrderBy(e => e.Tier).First())
                .OrderBy(e => e.Tier).ThenBy(e => e.Sort, StringComparer.Ordinal)
                .Select(e => e.Entry)
                .Take(MaxEntryApi)
        ];
    }

    private static bool IsFrameworkFrontDoor(MethodSignature m)
        => m.IsExtension && m.ExtendedType is { } ext
            && FrameworkSeats.Contains(StripGenerics(ext), StringComparer.Ordinal);

    private static ImmutableArray<string> BuildConsumerPaths(ImmutableArray<SurfaceEntry> entryApi)
    {
        var paths = new List<string>();
        foreach (var e in entryApi)
        {
            var recipe = e.Kind switch
            {
                "register" => $"wire into DI  →  {e.Title}(...)",
                "derive" => $"build one  →  derive {e.Title}",
                "implement" => $"contract  →  implement {e.Title}",
                "extend" => $"configure  →  {e.Title}.*",
                _ => null,
            };
            if (recipe is not null && !paths.Contains(recipe))
                paths.Add(recipe);
            if (paths.Count >= MaxConsumerPaths)
                break;
        }
        return [.. paths];
    }

    private static ImmutableArray<PackageGroup> BuildRuntimePackages(DiscoveryModel model, ProjectClassifier classifier)
        => MapBuilder.BuildPackages(model.Projects.Where(p =>
            !classifier.IsInTestProject(p.FilePath) && !IsExe(p) && !IsBenchmarkOrSample(p.Name)
            && !ProjectClassifier.IsSamplePath(p.FilePath)));

    // A library's surface is its library projects — not its exe benchmarks/samples. Collect those
    // project directories so their (public) types are kept out of the surface.
    private static ImmutableArray<string> NonLibraryProjectDirs(ImmutableArray<ProjectInfo> projects) =>
    [
        .. projects
            .Where(p => IsExe(p) || IsBenchmarkOrSample(p.Name))
            .Select(p => System.IO.Path.GetDirectoryName(p.FilePath))
            .Where(d => !string.IsNullOrEmpty(d))
            .Select(d => d!.Replace('\\', '/').TrimEnd('/'))
    ];

    private static bool IsUnder(ImmutableArray<string> dirs, string filePath)
    {
        var norm = filePath.Replace('\\', '/').TrimEnd('/');
        foreach (var d in dirs)
            if (norm.StartsWith(d + "/", StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static bool IsExe(ProjectInfo p)
        => p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsBenchmarkOrSample(string name)
        => name.Contains("Benchmark", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Sample", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Example", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Demo", StringComparison.OrdinalIgnoreCase);

    private static SurfaceType ToSurfaceType(TypeDiscovery t)
        => new(t.Name, t.Kind,
            [.. PublicMethods(t).Select(m => m.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal).Take(MaxMembersPerType)])
        {
            Doc = OneLine(t.XmlDoc),
        };

    private static IEnumerable<MethodSignature> PublicMethods(TypeDiscovery t)
        => t.Methods.Where(m =>
            m.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public
            && !m.Name.StartsWith('.')                              // ctors / static ctors
            && !m.Name.StartsWith("get_", StringComparison.Ordinal)
            && !m.Name.StartsWith("set_", StringComparison.Ordinal)
            && !m.Name.StartsWith("add_", StringComparison.Ordinal)
            && !m.Name.StartsWith("remove_", StringComparison.Ordinal));

    private static string StripGenerics(string typeName)
    {
        var i = typeName.IndexOf('<');
        return i > 0 ? typeName[..i] : typeName;
    }

    private static string? OneLine(string? doc)
    {
        if (string.IsNullOrWhiteSpace(doc)) return null;
        var text = doc.Trim();
        var dot = text.IndexOf(". ", StringComparison.Ordinal);
        if (dot > 0) text = text[..(dot + 1)];
        return text.Length > 120 ? text[..117] + "..." : text;
    }

    private static string ShortLocation(string filePath)
        => string.IsNullOrEmpty(filePath) ? "" : System.IO.Path.GetFileName(filePath);
}
