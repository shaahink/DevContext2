using System.Reflection;

namespace DevContext.Core.Utilities;

/// <summary>
/// Scans assemblies for <see cref="DiscoveryAssemblyAttribute"/> and discovers all
/// <see cref="IDiscoveryExtractor"/> implementations, sorted by <see cref="ExtractorOrderAttribute"/>.
/// </summary>
/// <remarks>
/// Enables the "implement + mark + test" workflow:
///   implement <see cref="IDiscoveryExtractor"/>,
///   mark the assembly with <c>[DiscoveryAssembly]</c>,
///   add a test.
///
/// All discovered extractors must have a parameterless constructor since the registry
/// uses <see cref="Activator.CreateInstance(System.Type)"/> to instantiate them.
/// </remarks>
public static class ExtractorRegistry
{
    /// <summary>
    /// Discovers all <see cref="IDiscoveryExtractor"/> implementations across assemblies
    /// marked with <see cref="DiscoveryAssemblyAttribute"/>.
    /// </summary>
    /// <returns>A read-only list of extractors sorted by <see cref="ExtractorOrderAttribute.Order"/>,
    /// or an empty list if no assemblies are marked with <c>[DiscoveryAssembly]</c>.</returns>
    public static IReadOnlyList<IDiscoveryExtractor> DiscoverExtractors()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.IsDefined(typeof(DiscoveryAssemblyAttribute)));

        if (!assemblies.Any())
            return Array.Empty<IDiscoveryExtractor>();

        var extractors = new List<IDiscoveryExtractor>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
.Where(t => !t.IsAbstract && !t.IsInterface && typeof(IDiscoveryExtractor).IsAssignableFrom(t));

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is IDiscoveryExtractor extractor)
                {
                    extractors.Add(extractor);
                }
            }
        }

        return extractors
            .OrderBy(e =>
            {
                var attr = e.GetType().GetCustomAttribute<ExtractorOrderAttribute>(false);
                return attr?.Order ?? 100;
            })
            .ToArray();
    }

    /// <summary>
    /// Discovers all <see cref="IDiscoveryExtractor"/> implementations from a specific assembly.
    /// The assembly must be marked with <see cref="DiscoveryAssemblyAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>A read-only list of extractors sorted by <see cref="ExtractorOrderAttribute.Order"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="assembly"/>
    /// is not marked with <c>[DiscoveryAssembly]</c>.</exception>
    public static IReadOnlyList<IDiscoveryExtractor> DiscoverExtractors(Assembly assembly)
    {
        if (!assembly.IsDefined(typeof(DiscoveryAssemblyAttribute)))
            throw new InvalidOperationException(
                $"Assembly '{assembly.GetName().Name}' is not marked with [DiscoveryAssemblyAttribute]. " +
                "Only assemblies decorated with this attribute contain discovery extractors.");

        var extractors = new List<IDiscoveryExtractor>();

        var types = assembly.GetTypes()
.Where(t => !t.IsAbstract && !t.IsInterface && typeof(IDiscoveryExtractor).IsAssignableFrom(t));

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is IDiscoveryExtractor extractor)
            {
                extractors.Add(extractor);
            }
        }

        return extractors
            .OrderBy(e =>
            {
                var attr = e.GetType().GetCustomAttribute<ExtractorOrderAttribute>(false);
                return attr?.Order ?? 100;
            })
            .ToArray();
    }
}
