using DevContext.Core.Graph;
using Xunit;

namespace DevContext.Core.Tests;

/// <summary>T3.4 — the config-key scan is extracted so it can be cached per session (the latency fix).
/// These lock the scan's behaviour: it finds literal keys across the indexer / GetValue / GetSection /
/// GetConnectionString patterns behind graph nodes, and is deterministic (same input → same output).
/// F3 (BUG-BACKLOG #34): the model-aware overload merges Options-pattern bindings detected at
/// extraction time into the same catalog — including from files that own NO graph node, which the
/// syntax scan skips wholesale.</summary>
public sealed class ConfigScannerTests
{
    private static (CodeGraph Graph, string File) BuildGraphOverTempSource(string source)
    {
        var (graph, files) = BuildGraphOverTempSources(("Settings.cs", source, true));
        return (graph, files[0]);
    }

    /// <summary>Multi-file harness (F3): writes each source into one temp dir; files flagged
    /// <c>ownsNode</c> get a Type node in the graph, the rest exist on disk only — the
    /// composition-root shape the syntax scan cannot see.</summary>
    private static (CodeGraph Graph, string[] Files) BuildGraphOverTempSources(
        params (string Name, string Source, bool OwnsNode)[] sources)
    {
        var dir = Path.Combine(Path.GetTempPath(), "devctx-cfg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        var g = new CodeGraphBuilder();
        var files = new string[sources.Length];
        for (var i = 0; i < sources.Length; i++)
        {
            var file = Path.Combine(dir, sources[i].Name);
            File.WriteAllText(file, sources[i].Source);
            files[i] = file;
            if (!sources[i].OwnsNode) continue;
            var typeName = Path.GetFileNameWithoutExtension(sources[i].Name);
            g.AddNode(new GraphNode(NodeId.ForType($"Ns.{typeName}"), typeName, NodeKind.Type)
            { FilePath = file, Project = "Api", LineNumber = 1 });
        }
        return (g.Build(), files);
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
    public void Scan_merges_options_bindings_from_a_file_the_syntax_scan_never_visits()
    {
        // F3 (#34): the Book2Course shape. The Options binding lives in a composition-root file that
        // owns NO graph node, so the syntax scan skips it wholesale — the extraction-time detection
        // is the only way its key reaches the catalog, labelled with its own PatternType.
        var (graph, files) = BuildGraphOverTempSources(
            ("Settings.cs", """
                public class Settings
                {
                    public void Load(IConfiguration configuration)
                    {
                        var a = configuration["ConnectionStrings:Db"];
                    }
                }
                """, true),
            ("DependencyInjection.cs", """
                public static class DependencyInjection
                {
                    public static IServiceCollection AddPipeline(this IServiceCollection services)
                    {
                        services.AddOptions<QueueDrainOptions>()
                            .BindConfiguration(QueueDrainOptions.SectionName);
                        return services;
                    }
                }
                """, false));
        try
        {
            var model = new DiscoveryModel();
            model.Detections.Add(new OptionsBindingDetection("QueueDrainOptions", "Pipeline:Queue:Drain", "BindConfiguration")
            { ExtractorName = "DiRegistrationExtractor", SourceFile = files[1], LineNumber = 5 });

            var bindings = ConfigScanner.Scan(graph, model);

            Assert.Contains(bindings, b => b.Key == "ConnectionStrings:Db"); // syntax rows survive
            var options = Assert.Single(bindings, b => b.Key == "Pipeline:Queue:Drain");
            Assert.Equal(ConfigScanner.OptionsBindingPatternType, options.PatternType);
            Assert.Equal(files[1], options.FilePath);
            Assert.Equal(5, options.LineNumber);
        }
        finally { Directory.Delete(Path.GetDirectoryName(files[0])!, recursive: true); }
    }

    [Fact]
    public void Scan_keeps_one_row_when_both_paths_catch_the_same_binding_site()
    {
        // Configure<T>(cfg.GetSection("X")) is one binding: the syntax scan sees the literal
        // GetSection, extraction sees the Options shape. The catalog must not count it twice,
        // and the Options row (which knows the bound type's pattern) wins the collision.
        var (graph, files) = BuildGraphOverTempSources(
            ("Startup.cs", """
                public static class Startup
                {
                    public static void Wire(IServiceCollection services, IConfiguration configuration)
                    {
                        services.Configure<MediaOptions>(configuration.GetSection("Pipeline:Media"));
                    }
                }
                """, true));
        try
        {
            var model = new DiscoveryModel();
            model.Detections.Add(new OptionsBindingDetection("MediaOptions", "Pipeline:Media", "Configure")
            { ExtractorName = "DiRegistrationExtractor", SourceFile = files[0], LineNumber = 5 });

            var bindings = ConfigScanner.Scan(graph, model);

            var row = Assert.Single(bindings, b => b.Key == "Pipeline:Media");
            Assert.Equal(ConfigScanner.OptionsBindingPatternType, row.PatternType);
            Assert.Equal("Api", row.Service); // service attributed from the file's graph node
        }
        finally { Directory.Delete(Path.GetDirectoryName(files[0])!, recursive: true); }
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
