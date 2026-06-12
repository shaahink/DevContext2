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

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            if (focusPoints.Count == 0) continue; // no focus — path component stays 0

            var minDistance = int.MaxValue;

            foreach (var fp in focusPoints)
            {
                var dist = ComputeDirectoryDistance(type.FilePath, fp.FilePath);
                if (dist < minDistance) minDistance = dist;
            }

            type.PathProximityScore = minDistance == 0
                ? 1.0f
                : Math.Max(0.0f, 1.0f - (float)minDistance / Math.Max(maxDistance, 1));
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
