namespace DevContext.Core.Pruning;

/// <summary>Prunes types whose directory distance from focus points exceeds the configured maximum.</summary>
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

        if (focusPoints.Count == 0)
        {
            foreach (var type in model.Types.Values)
            {
                type.PathProximityScore = 0.5f;
            }

            return default;
        }

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            var minDistance = int.MaxValue;

            foreach (var fp in focusPoints)
            {
                var dist = ComputeDirectoryDistance(type.FilePath, fp.FilePath);
                if (dist < minDistance) minDistance = dist;
            }

            type.PathProximityScore = minDistance == 0
                ? 1.0f
                : Math.Max(0.0f, 1.0f - (float)minDistance / Math.Max(maxDistance, 1));

            if (minDistance > maxDistance)
            {
                type.IsPruned = true;
            }
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
