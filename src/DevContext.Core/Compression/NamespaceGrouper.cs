namespace DevContext.Core.Compression;

public sealed class NamespaceGrouper : ICompressionStrategy
{
    public string Name => "NamespaceGrouper";
    public int Order => 40;

    public ValueTask<CompressionResult> CompressAsync(DiscoveryModel model, CompressionOptions options, CancellationToken ct)
    {
        var tokensBefore = EstimateTotalTokens(model);
        var notes = new List<string>();

        var groups = new Dictionary<string, List<TypeDiscovery>>();

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();
            if (type.IsPruned) continue;

            var ns = string.IsNullOrEmpty(type.Namespace) ? "(global)" : type.Namespace;
            if (!groups.TryGetValue(ns, out var bucket))
            {
                groups[ns] = bucket = [];
            }

            bucket.Add(type);
        }

        foreach (var kvp in groups)
        {
            ct.ThrowIfCancellationRequested();

            var ns = kvp.Key;
            var typesInNs = kvp.Value;

            typesInNs.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            foreach (var type in typesInNs)
            {
                type.Tags.Add($"ns-group:{ns}");
            }

            notes.Add($"Namespace '{ns}' contains {typesInNs.Count} type(s)");
        }

        var tokensAfter = EstimateTotalTokens(model);
        return new ValueTask<CompressionResult>(new CompressionResult(
            Name, tokensBefore, tokensAfter, notes));
    }

    private static int EstimateTotalTokens(DiscoveryModel model)
    {
        var chars = 0;
        foreach (var type in model.Types.Values)
        {
            if (type.IsPruned) continue;
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
