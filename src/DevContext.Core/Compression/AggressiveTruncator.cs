namespace DevContext.Core.Compression;

/// <summary>Aggressively truncates type source bodies that exceed the per-type character cap.</summary>
public sealed class AggressiveTruncator : ICompressionStrategy
{
    /// <summary>Gets the name of this compression strategy.</summary>
    public string Name => "AggressiveTruncator";
    /// <summary>Gets the execution order.</summary>
    public int Order => 60;

    public ValueTask<CompressionResult> CompressAsync(DiscoveryModel model, CompressionOptions options, CancellationToken ct)
    {
        var tokensBefore = EstimateTotalTokens(model);
        var notes = new List<string>();
        var truncatedCount = 0;

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();
            if (type.IsPruned || type.IsHardExcluded) continue;
            if (string.IsNullOrEmpty(type.SourceBody)) continue;

            var cap = options.PerTypeCharCap;
            if (cap <= 0) continue;

            var originalLength = type.SourceBody.Length;
            if (originalLength <= cap) continue;

            type.SourceBody = TruncateBody(type.SourceBody, cap, out var truncatedLines);
            truncatedCount++;
            notes.Add($"Truncated '{type.Id}' from {originalLength} to {cap} chars ({truncatedLines} method lines truncated)");
        }

        var tokensAfter = EstimateTotalTokens(model);
        return new ValueTask<CompressionResult>(new CompressionResult(
            Name, tokensBefore, tokensAfter, notes));
    }

    private static string TruncateBody(string body, int charCap, out int truncatedLines)
    {
        truncatedLines = 0;

        var lines = body.Split('\n');
        var result = new List<string>(lines.Length);
        var charCount = 0;

        foreach (var line in lines)
        {
            var lineLength = line.Length + 1;

            if (charCount + lineLength <= charCap)
            {
                result.Add(line);
                charCount += lineLength;
                continue;
            }

            var remaining = charCap - charCount;
            if (remaining > 3)
            {
                result.Add(line[..Math.Min(remaining - 3, line.Length)]);
            }

            truncatedLines = 1;

            var remainingLines = 0;
            for (var i = lines.Length - 1; i >= result.Count; i--)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    remainingLines++;
            }

            result.Add($"// ... [{remainingLines} lines]");
            break;
        }

        return string.Join("\n", result);
    }

    private static int EstimateTotalTokens(DiscoveryModel model)
    {
        var chars = 0;
        foreach (var type in model.Types.Values)
        {
            if (type.IsPruned || type.IsHardExcluded) continue;
            chars += type.Name?.Length ?? 0;
            chars += type.Namespace?.Length ?? 0;
            chars += type.Methods.Sum(m => m.Name.Length + m.ReturnType.Length);
            chars += type.Properties.Sum(p => p.Name.Length + p.PropertyType.Length);
            chars += type.BaseTypes.Sum(b => b.Length);
            chars += type.ImplementedInterfaces.Sum(i => i.Length);
            chars += type.SourceBody?.Length ?? 0;
        }

        return Math.Max(1, chars / 4);
    }
}
