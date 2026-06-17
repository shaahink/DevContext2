using System.Collections.Immutable;

using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Rendering;

/// <summary>Shared detection queries used by all renderers. Eliminates duplicate <c>.OfType&lt;T&gt;()</c> chains.</summary>
public static class RenderingQueries
{
    /// <summary>Returns endpoint detections, filtering known framework endpoints.</summary>
    public static IReadOnlyList<EndpointDetection> GetEndpoints(DiscoveryModel model, RenderOptions options)
    {
        var endpoints = model.Detections.OfType<EndpointDetection>()
            .Where(e => !IsFrameworkEndpoint(e));

        endpoints = FilterEndpointsByFocusPoints(endpoints, options);
        return endpoints.ToList();
    }

    private static IEnumerable<EndpointDetection> FilterEndpointsByFocusPoints(IEnumerable<EndpointDetection> endpoints, RenderOptions options)
    {
        if (options.FocusPoints.IsDefaultOrEmpty)
            return endpoints;

        var focusDirs = options.FocusPoints
            .Where(fp => fp.FilePath is not null)
            .Select(fp => Path.GetDirectoryName(fp.FilePath)?.Replace('\\', '/') ?? "")
            .Where(d => d.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (focusDirs.Count == 0)
            return endpoints;

        return endpoints.Where(e =>
        {
            if (e.SourceFile is null) return true;
            var dir = Path.GetDirectoryName(e.SourceFile)?.Replace('\\', '/') ?? "";
            return focusDirs.Any(fd =>
                dir.StartsWith(fd, StringComparison.OrdinalIgnoreCase)
                || fd.StartsWith(dir, StringComparison.OrdinalIgnoreCase)
                || AreSiblingDirectories(dir, fd));
        });
    }

    /// <summary>Checks whether an endpoint is a known framework/infrastructure route.</summary>
    public static bool IsFrameworkEndpoint(EndpointDetection ep)
    {
        var fileName = Path.GetFileName(ep.SourceFile);
        if (fileName is "OpenApi.Extensions.cs" or "Extensions.cs")
            return true;

        var route = ep.RouteTemplate;
        if (route is "/" or "/health" or "/alive")
            return true;

        return false;
    }

    /// <summary>Returns all MediatR handler detections.</summary>
    public static IReadOnlyList<MediatRHandlerDetection> GetMediatRHandlers(DiscoveryModel model)
        => model.Detections.OfType<MediatRHandlerDetection>().ToList();

    /// <summary>Returns MediatR handler type names as a hash set for quick lookup.</summary>
    public static ISet<string> GetMediatRHandlerTypes(DiscoveryModel model)
        => model.Detections.OfType<MediatRHandlerDetection>()
            .Select(m => m.HandlerType)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>Returns EF entity detections (excluding migrations and OnModelCreating synthetic entries).</summary>
    public static IReadOnlyList<EfEntityDetection> GetEfEntities(DiscoveryModel model)
        => model.Detections.OfType<EfEntityDetection>()
            .Where(e => !string.Equals(e.DbContextType, "Migrations", StringComparison.Ordinal)
                        && !string.Equals(e.EntityType, "<OnModelCreating>", StringComparison.Ordinal))
            .ToList();

    /// <summary>Returns the count of EF Core migration detections.</summary>
    public static int GetMigrationCount(DiscoveryModel model)
        => model.Detections.OfType<EfEntityDetection>()
            .Count(e => string.Equals(e.DbContextType, "Migrations", StringComparison.Ordinal));

    /// <summary>Returns message consumer detections.</summary>
    public static IReadOnlyList<MessageConsumerDetection> GetMessageConsumers(DiscoveryModel model)
        => model.Detections.OfType<MessageConsumerDetection>().ToList();

    /// <summary>Returns indirect wiring detections.</summary>
    public static IReadOnlyList<IndirectWiringDetection> GetIndirectWiring(DiscoveryModel model)
        => model.Detections.OfType<IndirectWiringDetection>().ToList();

    /// <summary>Returns background worker detections.</summary>
    public static IReadOnlyList<BackgroundWorkerDetection> GetBackgroundWorkers(DiscoveryModel model)
        => model.Detections.OfType<BackgroundWorkerDetection>().ToList();

    /// <summary>Returns middleware pipeline detections.</summary>
    public static IReadOnlyList<MiddlewareDetection> GetMiddlewarePipeline(DiscoveryModel model)
        => model.Detections.OfType<MiddlewareDetection>().ToList();

    /// <summary>Returns DI registration detections.</summary>
    public static IReadOnlyList<DiRegistrationDetection> GetDiRegistrations(DiscoveryModel model)
        => model.Detections.OfType<DiRegistrationDetection>().ToList();

    /// <summary>Returns anti-pattern detections.</summary>
    public static IReadOnlyList<AntiPatternDetection> GetAntiPatterns(DiscoveryModel model)
        => model.Detections.OfType<AntiPatternDetection>().ToList();

    /// <summary>Returns event flow detections.</summary>
    public static IReadOnlyList<EventFlowDetection> GetEventFlows(DiscoveryModel model)
        => model.Detections.OfType<EventFlowDetection>().ToList();

    /// <summary>Formats the DI shape consistently across renderers.</summary>
    public static string FormatDiShape(DiRegistrationDetection d)
    {
        var impl = FormatImplementation(d.ImplementationType, d.ExtensionsUsed);
        return d.Shape switch
        {
            DiRegistrationShape.ForwardingAlias => $"{impl} (alias)",
            DiRegistrationShape.InlineFactory when d.FactorySummary is not null => d.FactorySummary,
            DiRegistrationShape.DirectBinding => string.Equals(d.ServiceType, d.ImplementationType
, StringComparison.Ordinal) ? impl
                : $"{d.ServiceType} → {impl}",
            _ => impl,
        };
    }

    private static string FormatImplementation(string implementationType, ImmutableArray<string> extensionsUsed)
    {
        if (string.Equals(implementationType, "?", StringComparison.Ordinal) || string.IsNullOrEmpty(implementationType))
            return extensionsUsed.Length > 0
                ? $"({string.Join(", ", extensionsUsed.Take(3))})"
                : "-";

        if (implementationType.Length > 80)
        {
            var firstLine = implementationType.Split('\n')[0];
            return firstLine.Length > 60
                ? firstLine[..57] + "..."
                : firstLine + "...";
        }

        return implementationType;
    }

    private static bool AreSiblingDirectories(string dirA, string dirB)
    {
        var parentA = Path.GetDirectoryName(dirA)?.Replace('\\', '/') ?? "";
        var parentB = Path.GetDirectoryName(dirB)?.Replace('\\', '/') ?? "";
        return string.Equals(parentA, parentB, StringComparison.OrdinalIgnoreCase);
    }
}
