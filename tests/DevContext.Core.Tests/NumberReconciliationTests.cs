using DevContext.Core.Graph;
using DevContext.Core.Insights;

using Microsoft.CodeAnalysis;

namespace DevContext.Core.Tests;

/// <summary>Batch E (R2 §2.E item 2) — one counting function per stat. Each test pins a number that two
/// surfaces used to compute for themselves, and states which reading is the true one.</summary>
public sealed class NumberReconciliationTests
{
    [Fact]
    public void Confidence_ledger_reports_only_countable_facts()
    {
        // The retired member was OverallConfidence = verified/total*0.7 + (total-approx)/total*0.3.
        // The home chip printed it under the label "verified" while the chip's own tooltip printed
        // VerifiedEdgePct — one word, two numbers. Everything here is now a count, or a ratio of two
        // counts that travel beside it, so the chip and its explanation cannot disagree.
        var g = new CodeGraphBuilder();
        var a = NodeId.ForType("App.A");
        var b = NodeId.ForType("App.B");
        var c = NodeId.ForType("App.C");
        g.AddNode(new GraphNode(a, "A", NodeKind.Type));
        g.AddNode(new GraphNode(b, "B", NodeKind.Type));
        g.AddNode(new GraphNode(c, "C", NodeKind.Type));
        g.AddEdge(new GraphEdge(a, b, EdgeKind.Calls) { Resolution = Resolution.Semantic });
        g.AddEdge(new GraphEdge(a, c, EdgeKind.Calls) { Resolution = Resolution.Syntactic });

        var ledger = ConfidenceLedger.Compute(g.Build(), []);

        Assert.Equal(2, ledger.TotalEdges);
        Assert.Equal(0.5, ledger.VerifiedEdgePct);   // 1 of 2 semantic
        Assert.Equal(0.5, ledger.ApproxEdgePct);     // 1 of 2 syntactic
        // Every ratio is reproducible from counts the ledger also carries.
        Assert.Equal(ledger.VerifiedEdgePct, Math.Round(1d / ledger.TotalEdges, 2));
    }

    [Fact]
    public void Public_type_count_excludes_the_librarys_own_tests_and_samples()
    {
        // The library page counted the real surface while the lib.public-surface INSIGHT counted every
        // public type in the model — tests and samples included — and both appeared on the same screen.
        // One definition now answers for both; this pins WHICH definition won and why: a library's
        // contract is not what its own tests declare.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Lib", @"C:/repo/src/Lib/Lib.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Lib.Tests", @"C:/repo/tests/Lib.Tests/Lib.Tests.csproj", "C#", ["net10.0"], [],
                    [new PackageReferenceInfo("xunit", "2.9.0")]),
            ],
        };
        AddType(model, "Lib.PublicApi", @"C:/repo/src/Lib/PublicApi.cs");
        AddType(model, "Lib.Tests.PublicApiTests", @"C:/repo/tests/Lib.Tests/PublicApiTests.cs");
        AddType(model, "Lib.Samples.DemoApp", @"C:/repo/samples/Demo/DemoApp.cs");

        var surface = LibrarySurfaceBuilder.PublicSurfaceTypes(model);

        Assert.Equal(["Lib.PublicApi"], surface.Select(t => t.Id));
    }

    private static void AddType(DiscoveryModel model, string id, string filePath)
        => model.Types[id] = new TypeDiscovery
        {
            Id = id,
            Name = id[(id.LastIndexOf('.') + 1)..],
            Namespace = id[..id.LastIndexOf('.')],
            FilePath = filePath,
            Kind = DevContext.Core.Models.TypeKind.Class,
            Accessibility = Accessibility.Public,
            Layer = ArchitectureLayer.Unknown,
        };
}
