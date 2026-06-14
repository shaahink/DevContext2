using System.Text.RegularExpressions;

namespace DevContext.Core.Compression;

/// <summary>Formats source bodies for LLM consumption by normalizing whitespace and condensing doc comments.</summary>
public sealed partial class LlmFriendlyFormatter : ICompressionStrategy
{
    /// <summary>Gets the name of this compression strategy.</summary>
    public string Name => "LlmFriendlyFormatter";
    /// <summary>Gets the execution order.</summary>
    public int Order => 50;

    private static readonly Regex DocCommentRegex = MyRegex();

    private static readonly Regex SummaryTagRegex = new(
        @"<summary>(.*?)</summary>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ValueTask<CompressionResult> CompressAsync(DiscoveryModel model, CompressionOptions options, CancellationToken ct)
    {
        var tokensBefore = EstimateTotalTokens(model);
        var notes = new List<string>();
        var formattedCount = 0;

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();
            if (type.IsHardExcluded) continue;
            if (string.IsNullOrEmpty(type.SourceBody)) continue;

            var body = type.SourceBody;

            body = NormalizeWhitespace(body);
            body = StripDocCommentsPreserveSummary(body);

            type.SourceBody = body;

            var sectionTokens = EstimateTokenCount(body);
            if (sectionTokens > 0)
            {
                type.SourceBody = $"{body}\n<!-- ~{sectionTokens} tokens -->";
            }

            formattedCount++;
        }

        var tokensAfter = EstimateTotalTokens(model);
        notes.Add($"Formatted {formattedCount} type source bodies for LLM consumption");

        return new ValueTask<CompressionResult>(new CompressionResult(
            Name, tokensBefore, tokensAfter, notes));
    }

    private static string NormalizeWhitespace(string text)
    {
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Replace('\t', ' ').TrimEnd();
        }

        return string.Join("\n", lines);
    }

    private static string StripDocCommentsPreserveSummary(string text)
    {
        return DocCommentRegex.Replace(text, match =>
        {
            var content = match.Groups[1].Value.Trim();
            if (SummaryTagRegex.IsMatch(content))
            {
                var summaryMatch = SummaryTagRegex.Match(content);
                return $"/// {summaryMatch.Groups[0].Value}";
            }

            return string.Empty;
        });
    }

    private static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return Math.Max(1, text.Length / 4);
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
            chars += type.SourceBody?.Length ?? 0;
        }

        return Math.Max(1, chars / 4);
    }

    [GeneratedRegex(@"^\s*///\s*(.*?)$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
