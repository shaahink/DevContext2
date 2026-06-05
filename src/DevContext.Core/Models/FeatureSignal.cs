namespace DevContext.Core.Models;

/// <summary>Represents a detected feature signal (e.g., "mediatr", "efcore") with confidence and evidence.</summary>
public sealed record FeatureSignal(
    string Key,
    bool Detected,
    float Confidence,
    string DetectedVia,
    ImmutableArray<string> Evidence
)
{
    /// <summary>Creates a positively detected signal with the default confidence and evidence.</summary>
    public static FeatureSignal CreateDetected(string key, float confidence = 1.0f,
        string via = "PackageReference", params string[] evidence)
        => new(key, true, confidence, via, [.. evidence]);
}
