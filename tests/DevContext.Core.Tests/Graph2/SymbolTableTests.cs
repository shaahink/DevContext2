using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

public sealed class SymbolTableTests
{
    private static TypeDiscovery T(string id, string name, string ns, string filePath)
        => new()
        {
            Id = id,
            Name = name,
            Namespace = ns,
            FilePath = filePath,
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        };

    private static Func<string, string?> ProjectMapper =
        f => f.StartsWith("/src/Basket/", StringComparison.Ordinal) ? "Basket.API"
           : f.StartsWith("/src/Ordering/", StringComparison.Ordinal) ? "Ordering.API"
           : f.StartsWith("/src/Catalog/", StringComparison.Ordinal) ? "Catalog.API"
           : null;

    [Fact]
    public void Resolves_fqn_as_declared()
    {
        var table = new SymbolTable([T("Basket.API.CheckoutBasketCommand", "CheckoutBasketCommand", "Basket.API", "/src/Basket/Checkout.cs")], ProjectMapper);
        var r = table.Resolve(new SymbolRef { Text = "Basket.API.CheckoutBasketCommand", Site = Ref("") });
        Assert.Equal(ResolutionTier.Declared, r.Tier);
        Assert.Equal("Basket.API.CheckoutBasketCommand", r.Resolved?.Canonical);
    }

    [Fact]
    public void Resolves_unique_short_name_as_global_unique()
    {
        var table = new SymbolTable([T("Basket.API.CheckoutBasketCommand", "CheckoutBasketCommand", "Basket.API", "/src/Basket/Checkout.cs")], ProjectMapper);
        var r = table.Resolve(new SymbolRef { Text = "CheckoutBasketCommand", Site = Ref("") });
        Assert.Equal(ResolutionTier.GlobalUnique, r.Tier);
        Assert.Equal("Basket.API.CheckoutBasketCommand", r.Resolved?.Canonical);
    }

    [Fact]
    public void Resolves_project_scoped_when_file_context()
    {
        var table = new SymbolTable(
        [
            T("Basket.API.Models.Product", "Product", "Basket.API.Models", "/src/Basket/Models/Product.cs"),
            T("Ordering.API.Models.Product", "Product", "Ordering.API.Models", "/src/Ordering/Models/Product.cs"),
        ], ProjectMapper);
        var r = table.Resolve(new SymbolRef { Text = "Product", Site = Ref("/src/Basket/Controllers/Checkout.cs") });
        Assert.Equal(ResolutionTier.ProjectScoped, r.Tier);
        Assert.Equal("Basket.API.Models.Product", r.Resolved?.Canonical);
    }

    [Fact]
    public void Detects_ambiguity_when_short_name_has_multiple_global_matches()
    {
        var table = new SymbolTable(
        [
            T("Basket.API.Models.Product", "Product", "Basket.API.Models", "/src/Basket/Models/Product.cs"),
            T("Ordering.API.Models.Product", "Product", "Ordering.API.Models", "/src/Ordering/Models/Product.cs"),
        ], ProjectMapper);
        var r = table.Resolve(new SymbolRef { Text = "Product", Site = Ref("/src/Shared/Utils.cs") });
        Assert.Equal(ResolutionTier.Ambiguous, r.Tier);
        Assert.Null(r.Resolved);
        Assert.Equal(2, r.Candidates.Length);
    }

    [Fact]
    public void Unresolved_unknown_name_returns_unresolved_tier()
    {
        var table = new SymbolTable([T("Basket.API.CheckoutBasketCommand", "CheckoutBasketCommand", "Basket.API", "/src/Basket/Checkout.cs")], ProjectMapper);
        var r = table.Resolve(new SymbolRef { Text = "DoesNotExist", Site = Ref("") });
        Assert.Equal(ResolutionTier.Unresolved, r.Tier);
        Assert.Null(r.Resolved);
        Assert.Empty(r.Candidates);
    }

    [Fact]
    public void AmbiguityReport_counts_ambiguous_short_names()
    {
        var table = new SymbolTable(
        [
            T("Basket.API.Models.Product", "Product", "Basket.API.Models", "/src/Basket/Models/Product.cs"),
            T("Ordering.API.Models.Product", "Product", "Ordering.API.Models", "/src/Ordering/Models/Product.cs"),
            T("Ordering.API.Models.Order", "Order", "Ordering.API.Models", "/src/Ordering/Models/Order.cs"),
        ], ProjectMapper);
        var report = table.AmbiguityReport;
        Assert.Equal(1, report.AmbiguousShortNameCount);
        Assert.Equal(2, report.CandidateCounts["Product"]);
    }

    [Fact]
    public void RazorPages_scenario_same_short_name_different_single_project_resolves_correctly()
    {
        var table = new SymbolTable(
        [
            T("Sample1.Models.Student", "Student", "Sample1.Models", "/samples/Sample1/Models/Student.cs"),
            T("Sample2.Models.Student", "Student", "Sample2.Models", "/samples/Sample2/Models/Student.cs"),
        ], f => f.StartsWith("/samples/Sample1/", StringComparison.Ordinal) ? "Sample1"
               : f.StartsWith("/samples/Sample2/", StringComparison.Ordinal) ? "Sample2"
               : null);

        var r1 = table.Resolve(new SymbolRef { Text = "Student", Site = Ref("/samples/Sample1/Controllers/HomeController.cs") });
        Assert.Equal(ResolutionTier.ProjectScoped, r1.Tier);
        Assert.Equal("Sample1.Models.Student", r1.Resolved?.Canonical);

        var r2 = table.Resolve(new SymbolRef { Text = "Student", Site = Ref("/samples/Sample2/Pages/Index.cshtml.cs") });
        Assert.Equal(ResolutionTier.ProjectScoped, r2.Tier);
        Assert.Equal("Sample2.Models.Student", r2.Resolved?.Canonical);
    }

    [Fact]
    public void RazorPages_scenario_ambiguous_when_no_project_context()
    {
        var table = new SymbolTable(
        [
            T("Sample1.Models.Student", "Student", "Sample1.Models", "/samples/Sample1/Models/Student.cs"),
            T("Sample2.Models.Student", "Student", "Sample2.Models", "/samples/Sample2/Models/Student.cs"),
        ], ProjectMapper);
        var r = table.Resolve(new SymbolRef { Text = "Student", Site = Ref("/unknown/SomeFile.cs") });
        Assert.Equal(ResolutionTier.Ambiguous, r.Tier);
        Assert.Null(r.Resolved);
        Assert.Equal(2, r.Candidates.Length);
    }

    [Fact]
    public void Empty_text_is_unresolved()
    {
        var table = new SymbolTable([], null);
        var r = table.Resolve(new SymbolRef { Text = "", Site = Ref("") });
        Assert.Equal(ResolutionTier.Unresolved, r.Tier);
    }

    // ── L3.2: Law R2 — a real semantic bind is never downgraded or re-ambiguated ──

    [Fact]
    public void R2_preserves_semantic_bind_to_known_fqn()
    {
        var table = new SymbolTable([T("Basket.API.CheckoutBasketCommand", "CheckoutBasketCommand", "Basket.API", "/src/Basket/Checkout.cs")], ProjectMapper);
        var bound = new SymbolRef
        {
            Text = "CheckoutBasketCommand",
            Site = Ref(""),
            Resolved = new SymbolId(SymbolKind.Type, "Basket.API.CheckoutBasketCommand"),
            Tier = ResolutionTier.Semantic,
        };
        var r = table.Resolve(bound);
        Assert.Equal(ResolutionTier.Semantic, r.Tier);
        Assert.Equal("Basket.API.CheckoutBasketCommand", r.Resolved?.Canonical);
    }

    [Fact]
    public void R2_maps_semantic_bind_onto_in_scope_node_without_downgrading()
    {
        // The bound FQN (Roslyn display) differs from the graph's node id, but the short name is unique —
        // map onto the in-scope id so identity matches the syntactic path, keeping the Semantic tier.
        var table = new SymbolTable([T("Basket.API.CheckoutBasketCommand", "CheckoutBasketCommand", "Basket.API", "/src/Basket/Checkout.cs")], ProjectMapper);
        var bound = new SymbolRef
        {
            Text = "CheckoutBasketCommand",
            Site = Ref(""),
            Resolved = new SymbolId(SymbolKind.Type, "global::Basket.API.CheckoutBasketCommand"),
            Tier = ResolutionTier.Semantic,
        };
        var r = table.Resolve(bound);
        Assert.Equal(ResolutionTier.Semantic, r.Tier);
        Assert.Equal("Basket.API.CheckoutBasketCommand", r.Resolved?.Canonical);
    }

    [Fact]
    public void R2_semantic_bind_arbitrates_a_would_be_ambiguous_short_name()
    {
        // "Product" collides across two projects: a bare syntactic ref is Ambiguous (Law R1). But a real
        // semantic bind resolved it — that ref keeps its Semantic resolution instead of being re-ambiguated.
        var table = new SymbolTable(
        [
            T("Basket.API.Models.Product", "Product", "Basket.API.Models", "/src/Basket/Models/Product.cs"),
            T("Ordering.API.Models.Product", "Product", "Ordering.API.Models", "/src/Ordering/Models/Product.cs"),
        ], ProjectMapper);
        var bound = new SymbolRef
        {
            Text = "Product",
            Site = Ref("/src/Shared/Utils.cs"),
            Resolved = new SymbolId(SymbolKind.Type, "Ordering.API.Models.Product"),
            Tier = ResolutionTier.Semantic,
        };
        var r = table.Resolve(bound);
        Assert.Equal(ResolutionTier.Semantic, r.Tier);
        Assert.Equal("Ordering.API.Models.Product", r.Resolved?.Canonical);
        Assert.Empty(r.Candidates);
    }

    [Fact]
    public void IsKnownFqn_returns_true_for_registered_fqn()
    {
        var table = new SymbolTable([T("Basket.API.CheckoutBasketCommand", "CheckoutBasketCommand", "Basket.API", "/src/Basket/Checkout.cs")], ProjectMapper);
        Assert.True(table.IsKnownFqn("Basket.API.CheckoutBasketCommand"));
        Assert.False(table.IsKnownFqn("Basket.API.Unknown"));
    }

    [Fact]
    public void IsAmbiguous_returns_true_when_multiple_fqns_share_short_name()
    {
        var table = new SymbolTable(
        [
            T("Basket.API.Models.Product", "Product", "Basket.API.Models", "/src/Basket/Models/Product.cs"),
            T("Ordering.API.Models.Product", "Product", "Ordering.API.Models", "/src/Ordering/Models/Product.cs"),
        ], ProjectMapper);
        Assert.True(table.IsAmbiguous("Product"));
        Assert.False(table.IsAmbiguous("UniqueType"));
        Assert.False(table.IsAmbiguous("DoesNotExist"));
    }

    [Fact]
    public void GetNamespace_returns_ns_from_registered_fqn()
    {
        var table = new SymbolTable([T("Basket.API.CheckoutBasketCommand", "CheckoutBasketCommand", "Basket.API", "/src/Basket/Checkout.cs")], ProjectMapper);
        Assert.Equal("Basket.API", table.GetNamespace("Basket.API.CheckoutBasketCommand"));
    }

    [Fact]
    public void GetNamespace_extracts_namespace_for_unregistered_fqn()
    {
        var table = new SymbolTable([], null);
        var result = table.GetNamespace("DevContext.Core.Graph.GraphBuilder");
        Assert.Equal("DevContext.Core.Graph", result);
    }

    [Fact]
    public void GetNamespace_returns_fqn_when_no_dot_in_unregistered_name()
    {
        var table = new SymbolTable([], null);
        Assert.Equal("GraphBuilder", table.GetNamespace("GraphBuilder"));
    }

    [Fact]
    public void Empty_types_does_not_throw()
    {
        var table = new SymbolTable([]);
        Assert.NotNull(table.AmbiguityReport);
        Assert.Equal(0, table.AmbiguityReport.AmbiguousShortNameCount);
    }

    [Fact]
    public void Null_types_does_not_throw()
    {
        var table = new SymbolTable(null!);
        Assert.NotNull(table.AmbiguityReport);
        Assert.Equal(0, table.AmbiguityReport.AmbiguousShortNameCount);
    }

    private static RefSite Ref(string file, string project = "") => new() { File = file, Line = 1, Project = project };
}
