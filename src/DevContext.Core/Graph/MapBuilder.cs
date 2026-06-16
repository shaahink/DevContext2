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
}

public sealed record ProjectNode(string Name, ImmutableArray<string> DependsOn);

public sealed record PackageGroup(string Label, ImmutableArray<string> Packages);

public sealed class MapBuilder
{
    public static MapModel Build(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        return new MapModel
        {
            Style = model.DetectedStyle.ToString(),
            StyleConfidence = model.StyleConfidence,
            StyleEvidence = model.StyleDetectedVia,
            Entries = entries,
            Topology = BuildTopology(model),
            Packages = BuildPackages(model),
            Aggregates = BuildAggregates(model),
            PipelineBehaviors = [],
        };
    }

    private static ImmutableArray<ProjectNode> BuildTopology(DiscoveryModel model)
    {
        var scoped = model.Solution is { ProjectPaths.Length: > 0 } sln
            ? sln.ProjectPaths.Select(p => Path.GetFileNameWithoutExtension(p)).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : null;

        return
        [
            .. model.Projects
                .Where(p => scoped is null || scoped.Contains(p.Name))
                .OrderBy(p => p.Name)
                .Select(p => new ProjectNode(p.Name,
                    [.. p.ProjectReferences
                        .Where(r => scoped is null || scoped.Contains(r))
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
            list.Add(string.IsNullOrEmpty(version) ? name : $"{name} {version}");
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
