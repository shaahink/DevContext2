using DevContext.Core.Extractors.Specific;
using DevContext.Core.Models;

namespace DevContext.Core.Graph;

/// <summary>An Aspire infrastructure resource (a database, cache or broker declared in the AppHost).</summary>
/// <param name="Name">The resource's runtime name, e.g. "catalogdb".</param>
/// <param name="ResourceType">The resource method's suffix, e.g. "Postgres" for AddPostgres.</param>
internal readonly record struct AspireStore(string Name, string ResourceType);

/// <summary>Batch B (DC3) — resolves the ADDRESS a transport client was registered with to the project
/// that serves it. Two independent answers, tried in order:
/// <list type="number">
/// <item>the Aspire AppHost's own resource table — <c>AddProject&lt;Projects.Basket_API&gt;("basket-api")</c>
/// says in the repo's own words that the host "basket-api" is the project Basket.API;</item>
/// <item>a normalized project-name match, so repos with no AppHost still join
/// ("catalog-api" and "Catalog.API" normalize alike).</item>
/// </list>
/// A host matching two projects is AMBIGUOUS and resolves to nothing — Batch A's rule (never let a
/// name-shaped guess pick a silent winner) applies here too.</summary>
internal sealed class ServiceAddressBook
{
    private readonly Dictionary<string, string> _byResource;      // aspire resource name / AppHost local → project
    private readonly Dictionary<string, string> _byNormalized;    // normalized project name → project
    private readonly Dictionary<string, string> _grpcServices;    // gRPC service name → serving project
    private readonly HashSet<string> _ambiguousNormalized;

    private ServiceAddressBook(
        Dictionary<string, string> byResource,
        Dictionary<string, string> byNormalized,
        Dictionary<string, string> grpcServices,
        HashSet<string> ambiguousNormalized,
        Dictionary<string, AspireStore> storeResources)
    {
        _byResource = byResource;
        _byNormalized = byNormalized;
        _grpcServices = grpcServices;
        _ambiguousNormalized = ambiguousNormalized;
        StoreResources = storeResources;
    }

    /// <summary>Aspire resources that are infrastructure rather than analyzed projects, keyed by both
    /// their runtime name and the AppHost local they were assigned to.</summary>
    public IReadOnlyDictionary<string, AspireStore> StoreResources { get; }

    /// <summary>Aspire PROJECT resources keyed by runtime name and AppHost local — the topology join.</summary>
    public IReadOnlyDictionary<string, string> ProjectResources => _byResource;

    public static ServiceAddressBook Build(DiscoveryModel model, SolutionScope scope)
    {
        var byNormalized = new Dictionary<string, string>(StringComparer.Ordinal);
        var ambiguous = new HashSet<string>(StringComparer.Ordinal);
        foreach (var project in model.Projects)
        {
            var key = Normalize(project.Name);
            if (key.Length == 0) continue;
            if (byNormalized.TryGetValue(key, out var existing))
            {
                if (!string.Equals(existing, project.Name, StringComparison.Ordinal))
                    ambiguous.Add(key);
                continue;
            }
            byNormalized[key] = project.Name;
        }

        var byResource = new Dictionary<string, string>(StringComparer.Ordinal);
        var stores = new Dictionary<string, AspireStore>(StringComparer.Ordinal);
        foreach (var resource in model.Detections.OfType<AspireResourceDetection>()
            .OrderBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber))
        {
            if (resource.ResourceType == "Project")
            {
                // Projects.Basket_API → Basket.API: the generated name substitutes '_' for the
                // separators, so only the normalized forms are comparable.
                if (resource.ProjectRef is not { Length: > 0 } projectRef) continue;
                var key = Normalize(projectRef);
                if (key.Length == 0 || ambiguous.Contains(key)) continue;
                if (!byNormalized.TryGetValue(key, out var project)) continue;
                Register(byResource, resource.ResourceName, project);
                Register(byResource, resource.VariableName, project);
            }
            else
            {
                var store = new AspireStore(resource.ResourceName, resource.ResourceType);
                Register(stores, resource.ResourceName, store);
                Register(stores, resource.VariableName, store);
            }
        }

        var grpcServices = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var server in model.Detections.OfType<GrpcServiceDetection>()
            .OrderBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber))
        {
            if (!scope.Contains(server.SourceFile)) continue;
            if (scope.ProjectForFile(server.SourceFile) is not { Length: > 0 } project) continue;
            grpcServices.TryAdd(server.ServiceName, project);
        }

        return new ServiceAddressBook(byResource, byNormalized, grpcServices, ambiguous, stores);
    }

    /// <summary>Resolves an address host ("basket-api", "catalog-api") to an analyzed project, or null
    /// when the host names nothing in this solution (an external service) or names it ambiguously.</summary>
    public string? ResolveHost(string host)
    {
        if (_byResource.TryGetValue(host, out var viaResource)) return viaResource;
        var key = Normalize(host);
        if (key.Length == 0 || _ambiguousNormalized.Contains(key)) return null;
        return _byNormalized.TryGetValue(key, out var viaName) ? viaName : null;
    }

    /// <summary>Resolves a generated gRPC client type (<c>Basket.BasketClient</c>) to the project that
    /// implements the service — the join for clients registered without a literal address.</summary>
    public string? ResolveGrpcClientType(string clientType)
    {
        var leaf = clientType[(clientType.LastIndexOf('.') + 1)..];
        if (leaf.EndsWith("Client", StringComparison.Ordinal))
            leaf = leaf[..^"Client".Length];
        if (leaf.Length == 0) return null;
        return _grpcServices.TryGetValue(leaf, out var project) ? project : null;
    }

    /// <summary>Extracts the host from a configured address, including .NET service-discovery schemes
    /// (<c>https+http://catalog-api</c>) and ports (<c>http://basket-api:8080</c>). Null when the
    /// registration configured no literal address.</summary>
    public static string? ExtractHost(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;
        var schemeEnd = address.IndexOf("://", StringComparison.Ordinal);
        var rest = schemeEnd >= 0 ? address[(schemeEnd + 3)..] : address;
        var hostEnd = rest.IndexOfAny(['/', ':', '?']);
        var host = hostEnd >= 0 ? rest[..hostEnd] : rest;
        return host.Length == 0 ? null : host;
    }

    /// <summary>True when a host that resolved to no analyzed project still names a real service
    /// somewhere else — as opposed to a loopback address or an unsubstituted configuration
    /// placeholder, which name nothing and must not become nodes.</summary>
    public static bool IsExternalHost(string host)
    {
        if (host is "localhost" or "127.0.0.1" or "0.0.0.0" or "[::1]") return false;
        foreach (var c in host)
        {
            if (!char.IsLetterOrDigit(c) && c is not ('.' or '-' or '_')) return false;
        }
        return host.Any(char.IsLetter);
    }

    private static void Register<T>(Dictionary<string, T> map, string? key, T value)
    {
        if (key is { Length: > 0 } && key != "?") map.TryAdd(key, value);
    }

    /// <summary>Reduces a name to comparable form: "Catalog.API", "catalog-api" and "Catalog_Api" all
    /// become "catalogapi".</summary>
    private static string Normalize(string name)
    {
        var buffer = new char[name.Length];
        var length = 0;
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c)) buffer[length++] = char.ToLowerInvariant(c);
        }
        return new string(buffer, 0, length);
    }
}
