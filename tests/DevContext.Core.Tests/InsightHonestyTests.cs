using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;

namespace DevContext.Core.Tests;

/// <summary>T6.3 — insight honesty riders pinned: writes-only validation counting, VM-View
/// self-suppression, internal-hubs ref floor, no CLI flags in surface-neutral copy.</summary>
public sealed class InsightHonestyTests
{
    private static readonly CodeGraph EmptyGraph = new(
        new Dictionary<NodeId, GraphNode>(),
        new Dictionary<NodeId, ImmutableArray<GraphEdge>>());

    private static EntryPoint HttpEntry(string method, string route, string? target = "Handler.Cmd") => new(
        EntryPointKind.HttpEndpoint, $"{method} {route}", new NodeId(NodeKind.EntryPoint, $"{method} {route}"))
    {
        HttpMethod = method,
        Route = route,
        Target = target,
    };

    private static DiscoveryModel ModelWithFluentValidation()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(new FeatureSignal(
            ArchitectureSignals.Keys.FluentValidation, true, 1.0f, "test", []));
        return model;
    }

    [Fact]
    public void MissingValidation_counts_write_endpoints_only()
    {
        // Audit A11: "every WRITE endpoint needs a validator" was evidenced by GET endpoints —
        // eShop rendered "43/56" with reads inflating both numbers.
        var model = ModelWithFluentValidation();
        var entries = new[]
        {
            HttpEntry("GET", "/items"),
            HttpEntry("GET", "/items/{id}"),
            HttpEntry("POST", "/items"),
        }.ToImmutableArray();

        var insight = new UnvalidatedEndpointsSource().Compute(model, EmptyGraph, entries).Single();

        Assert.StartsWith("Missing validation: 1/1 write endpoints", insight.Title);
        Assert.DoesNotContain(insight.Evidence, e => e.StartsWith("GET"));
    }

    [Fact]
    public void MissingValidation_silent_when_only_reads_exist()
    {
        var model = ModelWithFluentValidation();
        var entries = new[] { HttpEntry("GET", "/items") }.ToImmutableArray();

        Assert.Empty(new UnvalidatedEndpointsSource().Compute(model, EmptyGraph, entries));
    }

    [Fact]
    public void VmView_self_suppresses_when_no_call_edges_bind_the_sides()
    {
        // T6.0 shamshir: "ViewModel-View: 0 VMs + 6 Views (0 call edges)" on a trading engine —
        // naming coincidences, not an MVVM layer. Zero binding edges → no card.
        var model = new DiscoveryModel();
        model.Types["T1"] = MakeType("TradeView", Microsoft.CodeAnalysis.Accessibility.Public);
        model.Types["T2"] = MakeType("RiskViewModel", Microsoft.CodeAnalysis.Accessibility.Public);

        var insights = new DesktopArchetypeSource().Compute(model, EmptyGraph, []).ToList();

        Assert.DoesNotContain(insights, i => i.Id == "desktop.vm-view-wiring");
    }

    [Fact]
    public void InternalHubs_requires_at_least_three_refs()
    {
        // "heavily-referenced ... (1 refs)" rendered verbatim on shamshir (T6.0 S1.10).
        var model = new DiscoveryModel();
        model.Types["Pub"] = MakeType("PublicApi", Microsoft.CodeAnalysis.Accessibility.Public);
        model.Types["Hub"] = MakeType("OnceUsed", Microsoft.CodeAnalysis.Accessibility.Internal);
        model.CallEdges.Add(new CallEdge("Lib.Caller", "M", "Lib.OnceUsed", "Go", "f.cs:1"));

        var insights = new LibraryArchetypeSource().Compute(model, EmptyGraph, []).ToList();

        Assert.DoesNotContain(insights, i => i.Id == "lib.internal-hubs");
    }

    private static TypeDiscovery MakeType(string name, Microsoft.CodeAnalysis.Accessibility accessibility) => new()
    {
        Id = $"Lib.{name}",
        Name = name,
        Namespace = "Lib",
        FilePath = $@"C:\repo\src\{name}.cs",
        Kind = TypeKind.Class,
        Accessibility = accessibility,
        Layer = ArchitectureLayer.Application,
    };

    [Fact]
    public void CoverageHonesty_carries_no_cli_flag_copy()
    {
        var entries = new[] { HttpEntry("GET", "/x") }.ToImmutableArray();
        var insight = new CoverageHonestySource().Compute(new DiscoveryModel(), EmptyGraph, entries).Single();

        Assert.DoesNotContain("--focus", insight.Title);
    }

    // ----- config.missing-defaults (F3, BUG-BACKLOG #34) — first-ever coverage -----

    private static OptionsBindingDetection OptionsBinding(string optionsType, string section) => new(
        optionsType, section, "BindConfiguration")
    {
        ExtractorName = "DiRegistrationExtractor",
        SourceFile = @"C:\repo\src\Pipeline\DependencyInjection.cs",
        LineNumber = 73,
    };

    [Fact]
    public void ConfigDefaults_counts_options_bound_sections()
    {
        // F3: AddOptions<T>().BindConfiguration(Const) consumed a section the regex path could not
        // see, so "N consumed keys" under-declared. The extraction-time OptionsBindingDetection is
        // the ONE source both the config catalog and this count read.
        var model = new DiscoveryModel();
        model.Detections.Add(OptionsBinding("QueueDrainOptions", "Pipeline:Queue:Drain"));

        var insight = new ConfigDefaultsSource().Compute(model, EmptyGraph, []).Single();

        Assert.Contains("1 consumed", insight.Title);
        Assert.Contains("Pipeline:Queue:Drain", insight.Evidence);
    }

    [Fact]
    public void ConfigDefaults_still_counts_literal_reads_alongside_options_bindings()
    {
        var model = new DiscoveryModel();
        model.Types["T1"] = MakeType("Notifier", Microsoft.CodeAnalysis.Accessibility.Public);
        model.Types["T1"].SourceBody = """var url = _config["Notify:Url"];""";
        model.Detections.Add(OptionsBinding("QueueDrainOptions", "Pipeline:Queue:Drain"));

        var insight = new ConfigDefaultsSource().Compute(model, EmptyGraph, []).Single();

        Assert.Contains("2 consumed", insight.Title);
        Assert.Contains("Notify:Url", insight.Evidence);
        Assert.Contains("Pipeline:Queue:Drain", insight.Evidence);
    }

    [Fact]
    public void ConfigDefaults_headline_names_its_blind_spot()
    {
        // The drive's honesty requirement: where under-declaring remains possible, the HEADLINE
        // says what the count cannot see — not just confidenceBasis. (DRIVE.md F3: "the catalog
        // under-declares and doesn't say so".)
        var model = new DiscoveryModel();
        model.Detections.Add(OptionsBinding("QueueDrainOptions", "Pipeline:Queue:Drain"));

        var insight = new ConfigDefaultsSource().Compute(model, EmptyGraph, []).Single();

        Assert.Contains("computed keys", insight.Title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConfigDefaults_count_is_the_full_missing_count_not_the_evidence_cap()
    {
        // The old code did Take(8) BEFORE counting, so any repo with more than eight missing keys
        // reported exactly 8 — an under-declared count wearing a confident headline.
        var model = new DiscoveryModel();
        for (var i = 0; i < 10; i++)
            model.Detections.Add(OptionsBinding($"Opt{i}", $"Section:{i:D2}"));

        var insight = new ConfigDefaultsSource().Compute(model, EmptyGraph, []).Single();

        Assert.Contains("10 consumed", insight.Title);
        Assert.Equal(8, insight.Evidence.Length); // evidence stays capped; the COUNT is honest
    }
}
