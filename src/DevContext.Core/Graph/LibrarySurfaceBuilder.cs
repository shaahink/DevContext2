namespace DevContext.Core.Graph;

/// <summary>
/// Builds the <see cref="LibrarySurface"/> from a library's public types/methods. Produces a ranked
/// <c>ENTRY API</c> (marker attributes → extension front-doors → builders → abstract seats → fluent DSL),
/// the <c>ABSTRACTIONS</c> seats consumers implement/derive, a <c>GENERATORS</c> section (source
/// generators / analyzers / code fixers), the namespace-grouped public surface (with <c>*.Internal</c> and
/// tooling namespaces demoted and XML-doc one-liners), deterministic consumer-path recipes, and runtime-only
/// packages. Build-free: everything is derived from syntactic <see cref="TypeDiscovery"/> data (base types /
/// interfaces / attributes / WP1 doc summaries + extension flags). Test/sample/exe projects are excluded,
/// consistent with the graph's <see cref="NoiseFilter"/>.
/// </summary>
public static class LibrarySurfaceBuilder
{
    private const int MaxMembersPerType = 15;
    private const int MaxEntryApi = 12;
    private const int MaxAbstractions = 10;
    private const int MaxConsumerPaths = 6;
    private const string AttributeSuffix = "Attribute";

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
            .Where(t => !ProjectClassifier.IsTestPath(t.FilePath))
            .Where(t => !IsUnder(nonLibraryDirs, t.FilePath))
            // Stable order: model.Types is a ConcurrentDictionary (nondeterministic enumeration), so order
            // by FQN here — every downstream grouping/dedup then produces a byte-deterministic surface.
            .OrderBy(t => t.Id, StringComparer.Ordinal)
            .ToList();

        // Roslyn tooling (source generators / analyzers / code fixers) gets its own GENERATORS section
        // rather than cluttering the runtime API surface.
        var generators = BuildGenerators(publicTypes);
        var toolingNames = generators.Select(g => g.Name).ToHashSet(StringComparer.Ordinal);
        var surfaceTypes = publicTypes.Where(t => !toolingNames.Contains(t.Name)).ToList();

        var mainTypes = surfaceTypes.Where(t => !IsDemotedNamespace(t.Namespace)).ToList();
        var internalTypes = surfaceTypes.Where(t => IsDemotedNamespace(t.Namespace)).ToList();

        var abstractions = BuildAbstractions(model, mainTypes);
        var entryApi = BuildEntryApi(mainTypes, abstractions, hasGenerators: generators.Length > 0);

        return new LibrarySurface(GroupByNamespace(mainTypes), BuildExtensionPoints(publicTypes))
        {
            EntryApi = entryApi,
            Abstractions = abstractions,
            Internals = GroupByNamespace(internalTypes),
            ConsumerPaths = BuildConsumerPaths(entryApi),
            Generators = generators,
            Packages = BuildRuntimePackages(model, classifier),
        };
    }

    private static bool IsDemotedNamespace(string ns) => IsInternalNamespace(ns) || IsToolingNamespace(ns);

    private static bool IsToolingNamespace(string ns)
        => ns.Contains(".SourceGenerators", StringComparison.Ordinal)
            || ns.Contains(".CodeFixers", StringComparison.Ordinal);

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
                .GroupBy(a => a.Name, StringComparer.Ordinal)
                .Select(g => g.First())
                .OrderByDescending(a => a.ImplementorCount)
                .ThenBy(a => a.Name, StringComparer.Ordinal)
                .Take(MaxAbstractions)
        ];
    }

    /// <summary>Ranked "how do I use this": (0) marker attributes (source-gen libraries), (1) framework-seat
    /// extension front-doors, (2) primary fluent builders, (3) abstract seats, (4) fluent-DSL extension
    /// classes. Deterministic, weight-free — tiered then name-ordered.</summary>
    private static ImmutableArray<SurfaceEntry> BuildEntryApi(List<TypeDiscovery> mainTypes, ImmutableArray<SurfaceAbstraction> abstractions, bool hasGenerators)
    {
        var ranked = new List<(int Tier, string Sort, SurfaceEntry Entry)>();

        // Tier 0: marker attributes — the consumer API of an attribute-driven (source-gen) library.
        if (hasGenerators)
            foreach (var t in mainTypes.Where(IsMarkerAttribute))
                ranked.Add((0, t.Name, new SurfaceEntry($"[{t.Name[..^AttributeSuffix.Length]}]", "annotate",
                    OneLine(t.XmlDoc), ShortLocation(t.FilePath))));

        foreach (var t in mainTypes)
            foreach (var m in PublicMethods(t).Where(IsFrameworkFrontDoor))
                ranked.Add((1, $"{t.Name}.{m.Name}",
                    new SurfaceEntry($"{t.Name}.{m.Name}", "register", OneLine(m.XmlDoc), ShortLocation(t.FilePath))));

        foreach (var t in mainTypes.Where(IsBuilderType))
            ranked.Add((2, t.Name, new SurfaceEntry(t.Name, "build", OneLine(t.XmlDoc), ShortLocation(t.FilePath))));

        foreach (var a in abstractions.Take(4))
        {
            var t = mainTypes.FirstOrDefault(x => x.Name == a.Name);
            var kind = a.Kind == TypeKind.Interface ? "implement" : "derive";
            ranked.Add((3, a.Name, new SurfaceEntry(a.Name, kind, OneLine(t?.XmlDoc),
                t is null ? null : ShortLocation(t.FilePath))));
        }

        foreach (var t in mainTypes)
            if (PublicMethods(t).Any(m => m.IsExtension && !IsFrameworkFrontDoor(m)))
                ranked.Add((4, t.Name, new SurfaceEntry(t.Name, "extend", OneLine(t.XmlDoc), ShortLocation(t.FilePath))));

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

    /// <summary>A primary fluent builder — a <c>*Builder</c> class exposing a public <c>Build()</c>
    /// (e.g. Polly's <c>ResiliencePipelineBuilder</c>). Excludes <c>*BuilderBase</c>/<c>*BuilderExtensions</c>
    /// and predicate builders that have no <c>Build</c>.</summary>
    private static bool IsBuilderType(TypeDiscovery t)
        => t.Kind == TypeKind.Class
            && t.Name.EndsWith("Builder", StringComparison.Ordinal)
            && PublicMethods(t).Any(m => m.Name == "Build");

    private static bool IsMarkerAttribute(TypeDiscovery t)
        => t.Kind == TypeKind.Class
            && t.Name.Length > AttributeSuffix.Length
            && t.Name.EndsWith(AttributeSuffix, StringComparison.Ordinal);

    /// <summary>Detects the Roslyn tooling a library ships — source generators (IIncrementalGenerator /
    /// ISourceGenerator), analyzers (DiagnosticAnalyzer / DiagnosticSuppressor), and code fixers
    /// (CodeFixProvider) — by base type / interface / [Generator] attribute. Build-free.</summary>
    private static ImmutableArray<SurfaceGenerator> BuildGenerators(IEnumerable<TypeDiscovery> publicTypes) =>
    [
        .. publicTypes
            .Select(t => (Type: t, Kind: GeneratorKind(t)))
            .Where(x => x.Kind is not null)
            .Select(x => (Order: GeneratorOrder(x.Kind!), Gen: new SurfaceGenerator(x.Type.Name, x.Kind!, OneLine(x.Type.XmlDoc))))
            .GroupBy(x => x.Gen.Name, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(x => x.Order).ThenBy(x => x.Gen.Name, StringComparer.Ordinal)
            .Select(x => x.Gen)
    ];

    private static string? GeneratorKind(TypeDiscovery t)
    {
        if (t.Kind != TypeKind.Class) return null;
        if (t.ImplementedInterfaces.Any(i => i is "IIncrementalGenerator" or "ISourceGenerator")
            || t.Attributes.Any(a => a is "Generator" or "GeneratorAttribute"))
            return "generator";
        if (t.BaseTypes.Any(b => b is "DiagnosticAnalyzer" or "DiagnosticSuppressor"))
            return "analyzer";
        if (t.BaseTypes.Any(b => b is "CodeFixProvider"))
            return "code-fixer";
        return null;
    }

    private static int GeneratorOrder(string kind) => kind switch { "generator" => 0, "analyzer" => 1, _ => 2 };

    private static ImmutableArray<string> BuildConsumerPaths(ImmutableArray<SurfaceEntry> entryApi)
    {
        var paths = new List<string>();
        foreach (var e in entryApi)
        {
            var recipe = e.Kind switch
            {
                "annotate" => $"annotate  →  {e.Title} on a partial class/member",
                "register" => $"wire into DI  →  {e.Title}(...)",
                "build" => $"build  →  new {e.Title}()…Build()",
                "derive" => $"extend  →  derive {e.Title}",
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
