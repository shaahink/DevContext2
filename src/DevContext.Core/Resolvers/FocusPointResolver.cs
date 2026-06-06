namespace DevContext.Core.Resolvers;

/// <summary>Resolves Type and Method focus points by looking up model.Types after Stage 2 population.</summary>
public static class FocusPointResolver
{
    /// <summary>Resolves unresolved Type/Method focus points against the populated model.Types dictionary.</summary>
    public static IReadOnlyList<FocusPoint> Resolve(
        IReadOnlyList<FocusPoint> unresolved, DiscoveryModel model)
    {
        var resolved = new List<FocusPoint>(unresolved.Count);

        foreach (var fp in unresolved)
        {
            if (fp.Kind is FocusKind.Type or FocusKind.Method)
            {
                // Look up the type name in model.Types. Types are keyed by fully-qualified name,
                // so search by matching the TypeName against type.Name or type.Id
                var match = FindType(fp.TypeName, model);
                if (match is not null)
                {
                    resolved.Add(new FocusPoint(fp.Kind, match.FilePath, fp.TypeName, fp.MethodName));
                }
                else
                {
                    // Fall back to folder proximity with a warning (emitted by pipeline)
                    resolved.Add(fp);
                }
            }
            else
            {
                resolved.Add(fp);
            }
        }

        return resolved.ToImmutableArray();
    }

    private static TypeDiscovery? FindType(string? typeName, DiscoveryModel model)
    {
        if (string.IsNullOrEmpty(typeName)) return null;

        // Try exact match on Id (fully-qualified name ending with .TypeName)
        foreach (var (id, type) in model.Types)
        {
            if (id.EndsWith("." + typeName, StringComparison.Ordinal)
                || id == typeName
                || type.Name == typeName)
            {
                return type;
            }
        }

        return null;
    }
}
