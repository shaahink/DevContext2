using System.Text;

namespace DevContext.Core.Compression;

/// <summary>Deduplicates types that have identical structural shapes (same methods and properties).</summary>
public sealed class StructuralDeduplicator : ICompressionStrategy
{
    /// <summary>Gets the name of this compression strategy.</summary>
    public string Name => "StructuralDeduplicator";
    /// <summary>Gets the execution order.</summary>
    public int Order => 30;

    public ValueTask<CompressionResult> CompressAsync(DiscoveryModel model, CompressionOptions options, CancellationToken ct)
    {
        var tokensBefore = EstimateTotalTokens(model);
        var notes = new List<string>();
        var groups = new Dictionary<string, List<TypeDiscovery>>();
        var prunedCount = 0;

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();
            if (type.IsHardExcluded) continue;

            var key = ComputeShapeKey(type);
            if (!groups.TryGetValue(key, out var bucket))
            {
                groups[key] = bucket = [];
            }

            bucket.Add(type);
        }

        foreach (var kvp in groups)
        {
            ct.ThrowIfCancellationRequested();

            var bucket = kvp.Value;
            if (bucket.Count <= 1) continue;

            bucket.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            for (var i = 1; i < bucket.Count; i++)
            {
                var duplicate = bucket[i];
                duplicate.IsHardExcluded = true;
                duplicate.ExclusionReason = $"deduplicated (same shape as {bucket[0].Id})";
                model.PrunedTypeIds.Add(duplicate.Id);
                prunedCount++;
            }

            var kept = bucket[0];
            kept.Tags.Add($"similar-types:{bucket.Count}");
            notes.Add($"'{kept.Id}' has {bucket.Count} similar types (showing 1, deduplicated {bucket.Count - 1})");
        }

        var tokensAfter = EstimateTotalTokens(model);
        return new ValueTask<CompressionResult>(new CompressionResult(
            Name, tokensBefore, tokensAfter, notes));
    }

    private static string ComputeShapeKey(TypeDiscovery type)
    {
        var sb = new StringBuilder();
        sb.Append(type.Kind);
        sb.Append('|');

        var methodKeys = type.Methods
            .Select(m => $"{m.Name}:{m.ReturnType}:{string.Join(",", m.ParameterTypes)}")
            .OrderBy(k => k, StringComparer.Ordinal);
        foreach (var mk in methodKeys)
        {
            sb.Append(mk);
            sb.Append(';');
        }

        sb.Append('|');

        var propertyKeys = type.Properties
            .Select(p => $"{p.Name}:{p.PropertyType}")
            .OrderBy(k => k, StringComparer.Ordinal);
        foreach (var pk in propertyKeys)
        {
            sb.Append(pk);
            sb.Append(';');
        }

        return sb.ToString();
    }

    private static int EstimateTotalTokens(DiscoveryModel model)
    {
        var chars = 0;
        foreach (var type in model.Types.Values)
        {
            if (type.IsHardExcluded) continue;
            chars += type.Name?.Length ?? 0;
            chars += type.Namespace?.Length ?? 0;
            chars += type.Methods.Sum(m => m.Name.Length + m.ReturnType.Length);
            chars += type.Properties.Sum(p => p.Name.Length + p.PropertyType.Length);
            chars += type.BaseTypes.Sum(b => b.Length);
            chars += type.ImplementedInterfaces.Sum(i => i.Length);
        }

        return Math.Max(1, chars / 4);
    }
}
