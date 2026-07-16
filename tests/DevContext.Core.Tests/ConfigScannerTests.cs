using DevContext.Core.Graph;
using Xunit;

namespace DevContext.Core.Tests;

/// <summary>T3.4 — the config-key scan is extracted so it can be cached per session (the latency fix).
/// These lock the scan's behaviour: it finds literal keys across the indexer / GetValue / GetSection /
/// GetConnectionString patterns behind graph nodes, and is deterministic (same input → same output).</summary>
public sealed class ConfigScannerTests
{
    private static (CodeGraph Graph, string File) BuildGraphOverTempSource(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "devctx-cfg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "Settings.cs");
        File.WriteAllText(file, source);

        var g = new CodeGraphBuilder();
        var id = NodeId.ForType("Ns.Settings");
        g.AddNode(new GraphNode(id, "Settings", NodeKind.Type) { FilePath = file, Project = "Api", LineNumber = 1 });
        return (g.Build(), file);
    }

    [Fact]
    public void Scan_finds_literal_keys_across_patterns()
    {
        var source = """
            public class Settings
            {
                public void Load(IConfiguration configuration)
                {
                    var a = configuration["ConnectionStrings:Db"];
                    var b = configuration.GetValue<int>("Timeout");
                    var c = configuration.GetSection("Features");
                    var d = configuration.GetConnectionString("Redis");
                }
            }
            """;
        var (graph, file) = BuildGraphOverTempSource(source);
        try
        {
            var bindings = ConfigScanner.Scan(graph);
            var keys = bindings.Select(b => b.Key).ToHashSet();

            Assert.Contains("ConnectionStrings:Db", keys);
            Assert.Contains("Timeout", keys);
            Assert.Contains("Features", keys);
            Assert.Contains("Redis", keys);
            Assert.All(bindings, b => Assert.Equal(file, b.FilePath));
            Assert.All(bindings, b => Assert.Equal("Api", b.Service));
            Assert.Contains(bindings, b => b.PatternType == "GetValue");
            Assert.Contains(bindings, b => b.PatternType == "GetConnectionString");
        }
        finally { Directory.Delete(Path.GetDirectoryName(file)!, recursive: true); }
    }

    [Fact]
    public void Scan_is_deterministic_and_safe_when_files_are_missing()
    {
        // A node whose file does not exist must be skipped, not throw — the cache is computed once
        // and callers rely on it never faulting mid-session.
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForType("Ns.Gone"), "Gone", NodeKind.Type)
        { FilePath = "C:/definitely/not/here/Gone.cs", LineNumber = 1 });
        var graph = g.Build();

        var first = ConfigScanner.Scan(graph);
        var second = ConfigScanner.Scan(graph);

        Assert.Empty(first);
        Assert.Equal(first.Count, second.Count);
    }
}
