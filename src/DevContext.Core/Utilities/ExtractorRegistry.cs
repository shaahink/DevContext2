using System.Reflection;

namespace DevContext.Core.Utilities;

/// <summary>
/// Scans assemblies for <see cref="DiscoveryAssemblyAttribute"/> and discovers all
/// <see cref="IDiscoveryExtractor"/> implementations, sorted by <see cref="ExtractorOrderAttribute"/>.
/// Replaces hardcoded registration lists so that adding an extractor requires only
/// "implement IDiscoveryExtractor, mark assembly with [DiscoveryAssembly], add test."
/// </summary>
public static class ExtractorRegistry
{
    /// <summary>
    /// Discovers all IDiscoveryExtractor implementations across assemblies
    /// marked with [DiscoveryAssembly].
    /// </summary>
    public static IReadOnlyList<IDiscoveryExtractor> DiscoverExtractors()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetCustomAttribute<DiscoveryAssemblyAttribute>() != null);

        var extractors = new List<IDiscoveryExtractor>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => typeof(IDiscoveryExtractor).IsAssignableFrom(t));

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
    /// Discovers all IDiscoveryExtractor implementations from a specific assembly.
    /// </summary>
    public static IReadOnlyList<IDiscoveryExtractor> DiscoverExtractors(Assembly assembly)
    {
        var extractors = new List<IDiscoveryExtractor>();

        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => typeof(IDiscoveryExtractor).IsAssignableFrom(t));

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
