namespace DevContext.Core.Graph;

/// <summary>The orientation artifact: architecture, topology, packages, entry inventory, cross-cutting — no code.</summary>
public sealed record MapModel
{
    public string Style { get; init; } = "Unknown";
    public float StyleConfidence { get; init; }
    public string? StyleEvidence { get; init; }
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    public ImmutableArray<ProjectNode> Topology { get; init; } = [];
    public ImmutableArray<PackageGroup> Packages { get; init; } = [];
    public ImmutableArray<string> Aggregates { get; init; } = [];
    public ImmutableArray<string> PipelineBehaviors { get; init; } = [];
    /// <summary>App vs Library — decides whether the entry-point Map or the public-surface view renders (G3).</summary>
    public Archetype Archetype { get; init; } = Archetype.App;
    /// <summary>The capability-grouped public API, when <see cref="Archetype"/> is Library.</summary>
    public LibrarySurface? Surface { get; init; }
    /// <summary>When the analysed set is a partial closure of the owning solution (e.g. pointing at one
    /// microservice of many), a human-readable scope descriptor — keyed so the Map never claims a
    /// whole-system style from a single-service slice (Iteration 4 / Critical 3).</summary>
    public string? ScopeNote { get; init; }
}

public sealed record ProjectNode(string Name, ImmutableArray<string> DependsOn);

public sealed record PackageGroup(string Label, ImmutableArray<string> Packages);

public sealed class MapBuilder
{
    public static MapModel Build(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var archetype = ArchetypeDetector.Detect(model, entries);
        var topology = BuildTopology(model);
        return new MapModel
        {
            Style = model.DetectedStyle.ToString(),
            StyleConfidence = model.StyleConfidence,
            StyleEvidence = model.StyleDetectedVia,
            Entries = entries,
            Topology = topology,
            Packages = BuildPackages(model),
            Aggregates = BuildAggregates(model),
            PipelineBehaviors = BuildPipelineBehaviors(model),
            Archetype = archetype,
            Surface = archetype == Archetype.Library ? LibrarySurfaceBuilder.Build(model) : null,
            ScopeNote = BuildScopeNote(model, topology.Length),
        };
    }

    /// <summary>When fewer projects were analysed than the owning solution declares, describe the partial
    /// closure so the Map can stamp its scope (Iteration 4 / Critical 3). Null when the full solution was
    /// analysed or no solution was resolved.</summary>
    private static string? BuildScopeNote(DiscoveryModel model, int analyzedProjectCount)
    {
        var slnCount = model.Solution?.ProjectPaths.Length ?? 0;
        // Decide partial from the RAW discovered count vs the raw .sln count (consistent bases). Require a
        // clear gap (≤ 75%) so a whole-solution run that discovers a few fewer than the .sln lists (failed
        // loads, etc.) isn't falsely stamped — guards eShop whole-solution staying "Microservices".
        if (slnCount <= 0 || model.Projects.Length >= slnCount * 3 / 4 || analyzedProjectCount <= 0) return null;
        var slnName = model.Solution?.Name ?? "solution";
        return $"{analyzedProjectCount}-project closure of {slnCount}-project {slnName}";
    }

    private static ImmutableArray<ProjectNode> BuildTopology(DiscoveryModel model)
    {
        var classifier = new ProjectClassifier(model.Projects);
        var scoped = model.Solution is { ProjectPaths.Length: > 0 } sln
            ? sln.ProjectPaths.Select(p => Path.GetFileNameWithoutExtension(p)).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : null;

        // ProjectReferences come through as raw ".../X.csproj" relative paths; reduce to project
        // names so the topology reads "A ── B" (and so the name-based scope filter actually matches —
        // it previously dropped every dependency for solution-scoped repos). Test/benchmark projects
        // are excluded, consistent with the graph's NoiseFilter.
        return
        [
            .. model.Projects
                .Where(p => !classifier.IsInTestProject(p.FilePath))
                .Where(p => scoped is null || scoped.Contains(p.Name))
                .OrderBy(p => p.Name)
                .Select(p => new ProjectNode(p.Name,
                    [.. p.ProjectReferences
                        .Select(r => Path.GetFileNameWithoutExtension(r) ?? "")
                        .Where(r => r.Length > 0 && (scoped is null || scoped.Contains(r)))
                        .OrderBy(r => r)]))
        ];
    }

    private static ImmutableArray<PackageGroup> BuildPackages(DiscoveryModel model)
    {
        // Dedup by name, keep highest version
        var best = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pkg in model.Projects.SelectMany(p => p.PackageReferences))
        {
            if (!best.TryGetValue(pkg.Name, out var existing)
                || CompareVersions(pkg.Version, existing) > 0)
            {
                best[pkg.Name] = pkg.Version;
            }
        }

        // Group by category
        var groups = new Dictionary<string, List<string>>();
        foreach (var (name, version) in best.OrderBy(kv => kv.Key))
        {
            var cat = CategorizePackage(name);
            if (!groups.TryGetValue(cat, out var list))
                groups[cat] = list = [];
            // Strip unevaluated MSBuild-variable versions ($(TemplateOrchardPackageVersion) etc.) — show
            // just the package name rather than leak the build variable (Iteration 4 / Low 16).
            var showVersion = !string.IsNullOrEmpty(version) && !version.Contains("$(", StringComparison.Ordinal);
            list.Add(showVersion ? $"{name} {version}" : name);
        }

        var order = new[] { "Web/API", "ORM/Data", "Mediator/CQRS", "Messaging", "Validation",
            "Logging", "Testing", "Cloud", "Utilities", "Other" };
        var result = ImmutableArray.CreateBuilder<PackageGroup>();
        foreach (var cat in order)
        {
            if (groups.TryGetValue(cat, out var pkgs) && pkgs.Count > 0)
                result.Add(new PackageGroup(cat, [.. pkgs]));
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<string> BuildAggregates(DiscoveryModel model)
        => [.. model.Detections
            .OfType<EfEntityDetection>()
            .Where(d => d.IsAggregate)
            .Select(d => d.EntityType)
            .Distinct()
            .OrderBy(n => n)];

    private static ImmutableArray<string> BuildPipelineBehaviors(DiscoveryModel model)
    {
        var behaviors = new HashSet<string>(StringComparer.Ordinal);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            // Direct registration: services.AddTransient(typeof(IPipelineBehavior<,>), typeof(X))
            if (di.ServiceType.Contains("IPipelineBehavior", StringComparison.Ordinal))
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add(impl);
            }
            if (di.ExtensionsUsed.Contains("AddOpenBehavior") || di.ServiceType == "AddOpenBehavior")
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add(impl);
            }
            // AddMediatR fluent config: the lambda body may contain AddOpenBehavior(typeof(X)) calls
            if (di.ImplementationType is { Length: > 0 } body
                && body.Contains("AddOpenBehavior", StringComparison.Ordinal))
            {
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(body,
                    @"AddOpenBehavior\s*\(\s*typeof\s*\(\s*(\w+)",
                    System.Text.RegularExpressions.RegexOptions.Compiled))
                {
                    if (m.Groups[1].Value is { Length: > 0 } name && name != "?")
                        behaviors.Add(name);
                }
            }
        }
        return [.. behaviors.OrderBy(b => b)];
    }

    /// <summary>Strips typeof(…) / nameof(…) / generic arity suffix to get a raw type name.</summary>
    private static string CleanTypeRef(string expr)
    {
        var s = expr.AsSpan().Trim();
        if (s.StartsWith("typeof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        else if (s.StartsWith("nameof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        var generic = s.IndexOf('<');
        if (generic > 0) s = s.Slice(0, generic);
        return s.ToString().Trim();
    }

    private static int CompareVersions(string a, string b)
    {
        if (TryParseMajorMinor(a, out var aMajor, out var aMinor)
            && TryParseMajorMinor(b, out var bMajor, out var bMinor))
        {
            var cmp = aMajor.CompareTo(bMajor);
            return cmp != 0 ? cmp : aMinor.CompareTo(bMinor);
        }
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseMajorMinor(string version, out int major, out int minor)
    {
        major = 0; minor = 0;
        if (string.IsNullOrEmpty(version)) return false;
        var parts = version.Split('.');
        return parts.Length >= 2
            && int.TryParse(parts[0], out major)
            && int.TryParse(parts[1], out minor);
    }

    private static string CategorizePackage(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("aspnet") || n.Contains("microsoft.asp") || n.StartsWith("swashbuckle")
            || n.Contains("fastendpoints") || n.Contains("minimalapi")) return "Web/API";
        if (n.Contains("entityframework") || n.Contains("ef.") || n.Contains("efcore")
            || n.Contains("dapper") || n.Contains("sqlite") || n.Contains("sqlserver")
            || n.Contains("npgsql") || n.Contains("mysql") || n.Contains("cosmos")) return "ORM/Data";
        if (n.Contains("mediatr")) return "Mediator/CQRS";
        if (n.Contains("masstransit") || n.Contains("nservicebus") || n.Contains("rabbitmq")
            || n.Contains("azure.messaging") || n.Contains("amqp")) return "Messaging";
        if (n.Contains("fluentvalidation")) return "Validation";
        if (n.Contains("serilog") || n.Contains("nlog") || n.Contains("log4net")
            || n.Contains("opentelemetry") || n.Contains("applicationinsights")) return "Logging";
        if (n.Contains("xunit") || n.Contains("nunit") || n.Contains("mstest")
            || n.Contains("moq") || n.Contains("nsubstitute") || n.Contains("bogus")
            || n.Contains("fluentassertions") || n.Contains("shouldly")
            || n.Contains("testcontainers") || n.Contains("coverlet")) return "Testing";
        if (n.Contains("azure.") || n.Contains("amazon.") || n.Contains("aws.")) return "Cloud";
        if (n.Contains("polly") || n.Contains("automapper") || n.Contains("scrutor")
            || n.Contains("humanizer") || n.Contains("newtonsoft")
            || n.Contains("refit") || n.Contains("restsharp")
            || n.Contains("swagger")) return "Utilities";
        return "Other";
    }
}
