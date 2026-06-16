namespace DevContext.Core.Utilities;

/// <summary>Shared helper methods for extractors.</summary>
internal static class ExtractorHelpers
{
    /// <summary>Iterates all source files registered in the analysis cache with cancellation support.</summary>
    public static async IAsyncEnumerable<string> EnumerateSourceFilesAsync(
        DiscoveryContext context, [EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var file in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    /// <summary>Checks whether a file path belongs to a test project.</summary>
    public static bool IsTestFile(string filePath)
    {
        var lower = filePath.ToLowerInvariant();
        return lower.Contains("\\test", StringComparison.Ordinal) || lower.Contains("\\tests\\", StringComparison.Ordinal) || lower.Contains("/tests/", StringComparison.Ordinal)
            || lower.EndsWith("test.cs", StringComparison.OrdinalIgnoreCase)
            || lower.EndsWith("tests.cs", StringComparison.OrdinalIgnoreCase);
    }
}
