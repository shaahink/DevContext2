using System.Collections.Immutable;
using System.Text.Json;
using System.Text.RegularExpressions;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>M2.2 — Detects configuration keys consumed in C# source that have no default value
/// in appsettings*.json. Scans source for IConfiguration access patterns and compares against
/// declared configuration keys.</summary>
public sealed class ConfigDefaultsSource : IInsightSource
{
    public string Id => "config.missing-defaults";
    public InsightCategory Category => InsightCategory.Risk;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var consumedKeys = FindConsumedConfigKeys(model);
        if (consumedKeys.Count == 0) yield break;

        var appsettingsKeys = ParseAppsettingsKeys(model);
        var missingDefaults = consumedKeys
            .Where(k => !appsettingsKeys.Contains(k))
            .Take(8)
            .OrderBy(k => k)
            .ToList();

        if (missingDefaults.Count == 0) yield break;

        yield return Insight.Create(Id, Category, Severity.Notable,
            $"Config without defaults: {missingDefaults.Count} consumed keys have no appsettings default",
            missingDefaults,
            confidence: 0.5,
            confidenceBasis: "Config key extraction is regex-based — may miss Bind()/Options pattern keys. appsettings parsing is best-effort.",
            whyItMatters: "A config key consumed with no default fails at runtime — document or provide a default for every consumed key.");
    }

    private static readonly Regex ConfigKeyPattern = new(
        @"(?:\bIConfiguration\b|\bConfiguration\b|(?<!\w)(?:_config|_configuration|_cfg|_conf|_c)\b|(?<!\w)(?:cfg|conf)\b(?=\s*\.\s*\[))\s*(?:\[""([^""]+)""\]|\.GetValue<[^>]+>\(""([^""]+)"")|\.GetSection\(""([^""]+)"")|\.GetConnectionString\(""([^""]+)"")|\.GetRequiredSection\(""([^""]+)"")",
        RegexOptions.Compiled);

    private static HashSet<string> FindConsumedConfigKeys(DiscoveryModel model)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in model.Types.Values)
        {
            var body = type.SourceBody;
            if (body is null) continue;
            foreach (Match m in ConfigKeyPattern.Matches(body))
            {
                for (var i = 1; i <= 5; i++)
                {
                    if (m.Groups[i].Success)
                    {
                        result.Add(m.Groups[i].Value);
                        break;
                    }
                }
            }
        }
        return result;
    }

    private static HashSet<string> ParseAppsettingsKeys(DiscoveryModel model)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var scannedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var proj in model.Projects)
        {
            if (proj.FilePath is null) continue;
            var dir = Path.GetDirectoryName(proj.FilePath);
            if (dir is null || !scannedDirs.Add(dir)) continue;

            if (!Directory.Exists(dir)) continue;

            var configFiles = Directory.GetFiles(dir, "appsettings*.json", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(dir, "hostsettings*.json", SearchOption.AllDirectories))
                .ToList();

            foreach (var fp in configFiles)
            {
                try
                {
                    var text = File.ReadAllText(fp);
                    using var doc = JsonDocument.Parse(text);
                    CollectKeys(doc.RootElement, "", result, maxDepth: 5);
                }
                catch { }
            }
        }

        // Also scan solution-root-level config files (shared settings)
        if (model.Projects.Length > 0 && model.Projects[0].FilePath is { } firstProj)
        {
            var slnDir = Path.GetDirectoryName(Path.GetDirectoryName(firstProj));
            if (slnDir is not null && Directory.Exists(slnDir) && scannedDirs.Add(slnDir))
            {
                var slnConfigFiles = Directory.GetFiles(slnDir, "appsettings*.json", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(slnDir, "hostsettings*.json", SearchOption.AllDirectories));
                foreach (var fp in slnConfigFiles)
                {
                    try
                    {
                        var text = File.ReadAllText(fp);
                        using var doc = JsonDocument.Parse(text);
                        CollectKeys(doc.RootElement, "", result, maxDepth: 5);
                    }
                    catch { }
                }
            }
        }

        return result;
    }

    private static void CollectKeys(JsonElement element, string prefix, HashSet<string> keys, int maxDepth)
    {
        if (maxDepth <= 0) return;
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}:{prop.Name}";
                    keys.Add(key);
                    if (prop.Value.ValueKind != JsonValueKind.Object && prop.Value.ValueKind != JsonValueKind.Array)
                        continue;
                    CollectKeys(prop.Value, key, keys, maxDepth - 1);
                }
                break;
            case JsonValueKind.Array:
                var idx = 0;
                foreach (var item in element.EnumerateArray())
                {
                    CollectKeys(item, $"{prefix}:{idx}", keys, maxDepth - 1);
                    idx++;
                }
                break;
        }
    }
}
