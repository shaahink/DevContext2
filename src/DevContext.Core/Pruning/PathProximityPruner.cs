namespace DevContext.Core.Pruning;

/// <summary>Computes path-proximity score for every type based on directory distance from focus points. Score ∈ [0,1].</summary>
public sealed class PathProximityPruner : IPruner
{
    /// <summary>Gets the name of this pruner.</summary>
    public string Name => "PathProximityPruner";
    /// <summary>Gets the execution order.</summary>
    public int Order => 10;

    public ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var focusPoints = context.Analysis.FocusPoints;
        var maxDistance = context.ActiveScenario.Pruning.MaxPathDistance;

        // Separate resolved (has file path) and unresolved focus points
        var resolvedFps = focusPoints.Where(fp => !string.IsNullOrEmpty(fp.FilePath)).ToList();

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            if (focusPoints.Count == 0) continue;

            var minDistance = int.MaxValue;

            // Path-based: directory distance from resolved focus points
            foreach (var fp in resolvedFps)
            {
                var dist = ComputeDirectoryDistance(type.FilePath, fp.FilePath);
                if (dist < minDistance) minDistance = dist;
            }

            // Name-based fallback for unresolved focus points (type not found by name match)
            if (minDistance == int.MaxValue)
            {
                foreach (var fp in focusPoints)
                {
                    if (fp.TypeName is null) continue;
                    // Boost types whose name or namespace contains the focus type name
                    if (type.Name.Contains(fp.TypeName, StringComparison.OrdinalIgnoreCase)
                        || (type.Namespace?.Contains(fp.TypeName, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        var dist = 1; // near but not exact match
                        if (string.Equals(type.Name, fp.TypeName, StringComparison.OrdinalIgnoreCase))
                            dist = 0; // exact name match
                        if (dist < minDistance) minDistance = dist;
                    }
                }
            }

            type.PathProximityScore = minDistance switch
            {
                0 => 1.0f,
                int.MaxValue => 0.0f,
                _ => Math.Max(0.0f, 1.0f - (float)minDistance / Math.Max(maxDistance, 1)),
            };
        }

        return default;
    }

    private static int ComputeDirectoryDistance(string pathA, string pathB)
    {
        var dirA = Path.GetDirectoryName(pathA);
        var dirB = Path.GetDirectoryName(pathB);

        if (string.IsNullOrEmpty(dirA) || string.IsNullOrEmpty(dirB)) return int.MaxValue;

        var partsA = dirA.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var partsB = dirB.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var common = 0;
        var minLen = Math.Min(partsA.Length, partsB.Length);
        for (var i = 0; i < minLen; i++)
        {
            if (string.Equals(partsA[i], partsB[i], StringComparison.OrdinalIgnoreCase))
                common++;
            else
                break;
        }

        return (partsA.Length - common) + (partsB.Length - common);
    }
}
