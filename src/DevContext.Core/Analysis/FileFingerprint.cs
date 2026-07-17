using System.Collections.Immutable;
using System.Security.Cryptography;

namespace DevContext.Core.Analysis;

/// <summary>T4.5 — content fingerprint of an analyzed source file, captured at analyze time so
/// verify_context can tell whether disk has drifted from what the graph describes.</summary>
public readonly record struct FileFingerprint(string Sha256, int LineCount);

public static class FileFingerprinter
{
    /// <summary>Fingerprints every distinct file in <paramref name="filePaths"/> that is readable
    /// NOW. Files that vanish between extraction and snapshot simply get no fingerprint — the
    /// verifier reports them as unverifiable rather than guessing.</summary>
    public static ImmutableDictionary<string, FileFingerprint> Capture(IEnumerable<string> filePaths)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, FileFingerprint>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in filePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Read(path) is { } fp)
                builder[path] = fp;
        }
        return builder.ToImmutable();
    }

    /// <summary>One file's fingerprint from disk, or null when unreadable.</summary>
    public static FileFingerprint? Read(string path)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            var lines = 1;
            foreach (var b in bytes)
                if (b == (byte)'\n')
                    lines++;
            return new FileFingerprint(Convert.ToHexString(SHA256.HashData(bytes)), lines);
        }
        catch
        {
            return null;
        }
    }
}
