using System.Text.RegularExpressions;

namespace DevContext.Core.Graph;

/// <summary>One config key-usage site: where a literal configuration key is read in source.</summary>
public readonly record struct ConfigBindingInfo(
    string Key, string FilePath, int LineNumber, string NodeId, string PatternType, string Service);

/// <summary>
/// T3.4 — scans the source files behind graph nodes for literal configuration-key usage
/// (IConfiguration indexer / GetValue / GetSection / GetConnectionString / GetRequiredSection).
/// Extracted from the gRPC ConfigLookup handler so the result can be computed ONCE and cached per
/// session: the scan reads and regex-matches every node-bearing file (10.5s on a 2.8k-node repo),
/// so re-running it on every config() call was the latency bug. The scan is pure and deterministic.
/// </summary>
public static class ConfigScanner
{
    private static readonly Regex ConfigKeyRegex = new(
        @"(?:\bIConfiguration\b|\bConfiguration\b|(?<!\w)(?:_config|_configuration|_cfg|_conf|_c)\b|(?<!\w)(?:cfg|conf)\b(?=\s*\.\s*\[))\s*(?:\[""([^""]+)""\]|\.GetValue<[^>]+>\(""([^""]+)""\)|\.GetSection\(""([^""]+)""\)|\.GetConnectionString\(""([^""]+)""\)|\.GetRequiredSection\(""([^""]+)""\))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Full unfiltered scan over every file that owns a graph node. Callers filter by key
    /// in-memory afterward — this is the expensive part worth caching.</summary>
    public static IReadOnlyList<ConfigBindingInfo> Scan(CodeGraph graph)
    {
        var result = new List<ConfigBindingInfo>();

        var filesByPath = graph.Nodes
            .Where(n => n.FilePath is not null)
            .GroupBy(n => n.FilePath!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var (filePath, nodes) in filesByPath)
        {
            if (!File.Exists(filePath)) continue;
            string[] lines;
            try { lines = File.ReadAllLines(filePath); }
            catch { continue; }

            var service = nodes.Count > 0 ? nodes[0].Project ?? "" : "";

            for (var i = 0; i < lines.Length; i++)
            {
                foreach (Match m in ConfigKeyRegex.Matches(lines[i]))
                {
                    string? key = null;
                    var patternType = "Indexer";
                    for (var g = 1; g <= 5; g++)
                    {
                        if (m.Groups[g].Success)
                        {
                            key = m.Groups[g].Value;
                            patternType = g switch
                            {
                                1 => "Indexer", 2 => "GetValue", 3 => "GetSection",
                                4 => "GetConnectionString", 5 => "GetRequiredSection", _ => "Indexer",
                            };
                            break;
                        }
                    }
                    if (key is null) continue;

                    string? nodeId = null;
                    foreach (var node in nodes)
                        if (node.LineNumber is { } ln && ln == i + 1)
                        {
                            nodeId = node.Id.ToString();
                            break;
                        }

                    result.Add(new ConfigBindingInfo(key, filePath, i + 1, nodeId ?? "", patternType, service));
                }
            }
        }

        return result;
    }
}
