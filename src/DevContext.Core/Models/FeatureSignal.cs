namespace DevContext.Core.Models;

public sealed record FeatureSignal(
    string Key,
    bool Detected,
    float Confidence,
    string DetectedVia,
    ImmutableArray<string> Evidence
)
{
    public static FeatureSignal CreateDetected(string key, float confidence = 1.0f,
        string via = "PackageReference", params string[] evidence)
        => new(key, true, confidence, via, [.. evidence]);
}
