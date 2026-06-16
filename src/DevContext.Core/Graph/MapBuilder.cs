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
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => new ProjectNode(p.Name,
                    [.. p.ProjectReferences
                        .Where(r => scoped is null || scoped.Contains(r))
                        .Order(StringComparer.Ordinal)]))
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
        var groups = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var (name, version) in best.OrderBy(kv => kv.Key, StringComparer.Ordinal))
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
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

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
            && int.TryParse(parts[0], System.Globalization.CultureInfo.InvariantCulture, out major) && int.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, out minor);
    }

    private static string CategorizePackage(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("aspnet", StringComparison.Ordinal) || n.Contains("microsoft.asp", StringComparison.Ordinal) || n.StartsWith("swashbuckle", StringComparison.Ordinal)
            || n.Contains("fastendpoints", StringComparison.Ordinal) || n.Contains("minimalapi", StringComparison.Ordinal)) return "Web/API";
        if (n.Contains("entityframework", StringComparison.Ordinal) || n.Contains("ef.", StringComparison.Ordinal) || n.Contains("efcore", StringComparison.Ordinal)
            || n.Contains("dapper", StringComparison.Ordinal) || n.Contains("sqlite", StringComparison.Ordinal) || n.Contains("sqlserver", StringComparison.Ordinal)
            || n.Contains("npgsql", StringComparison.Ordinal) || n.Contains("mysql", StringComparison.Ordinal) || n.Contains("cosmos", StringComparison.Ordinal)) return "ORM/Data";
        if (n.Contains("mediatr", StringComparison.Ordinal)) return "Mediator/CQRS";
        if (n.Contains("masstransit", StringComparison.Ordinal) || n.Contains("nservicebus", StringComparison.Ordinal) || n.Contains("rabbitmq", StringComparison.Ordinal)
            || n.Contains("azure.messaging", StringComparison.Ordinal) || n.Contains("amqp", StringComparison.Ordinal)) return "Messaging";
        if (n.Contains("fluentvalidation", StringComparison.Ordinal)) return "Validation";
        if (n.Contains("serilog", StringComparison.Ordinal) || n.Contains("nlog", StringComparison.Ordinal) || n.Contains("log4net", StringComparison.Ordinal)
            || n.Contains("opentelemetry", StringComparison.Ordinal) || n.Contains("applicationinsights", StringComparison.Ordinal)) return "Logging";
        if (n.Contains("xunit", StringComparison.Ordinal) || n.Contains("nunit", StringComparison.Ordinal) || n.Contains("mstest", StringComparison.Ordinal)
            || n.Contains("moq", StringComparison.Ordinal) || n.Contains("nsubstitute", StringComparison.Ordinal) || n.Contains("bogus", StringComparison.Ordinal)
            || n.Contains("fluentassertions", StringComparison.Ordinal) || n.Contains("shouldly", StringComparison.Ordinal)
            || n.Contains("testcontainers", StringComparison.Ordinal) || n.Contains("coverlet", StringComparison.Ordinal)) return "Testing";
        if (n.Contains("azure.", StringComparison.Ordinal) || n.Contains("amazon.", StringComparison.Ordinal) || n.Contains("aws.", StringComparison.Ordinal)) return "Cloud";
        if (n.Contains("polly", StringComparison.Ordinal) || n.Contains("automapper", StringComparison.Ordinal) || n.Contains("scrutor", StringComparison.Ordinal)
            || n.Contains("humanizer", StringComparison.Ordinal) || n.Contains("newtonsoft", StringComparison.Ordinal)
            || n.Contains("refit", StringComparison.Ordinal) || n.Contains("restsharp", StringComparison.Ordinal)
            || n.Contains("swagger", StringComparison.Ordinal)) return "Utilities";
        return "Other";
    }
}
