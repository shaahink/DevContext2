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
}
