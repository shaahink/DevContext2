namespace DevContext.Core.Tests;

public sealed class OutputSelfCheckTests
{
    private static readonly RenderOptions DefaultRenderOptions = new(
        IncludeProvenance: false,
        IncludeDiagnostics: false,
        EstimatedTokens: 8000);

    // ── budget-respected ──────────────────────────────────────────────

    [Fact]
    public void BudgetRespected_WhenUnderBudget_Passes()
    {
        var rendered = new RenderedContext("content", 500, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = 8000 } };
        var opts = DefaultRenderOptions with { EstimatedTokens = 8000 };

        var results = OutputSelfCheck.Check(rendered, model, opts);

        var r = results.First(x => x.CheckId == "budget-respected");
        Assert.True(r.Passed);
    }

    [Fact]
    public void BudgetRespected_WhenOverBudget_Fails()
    {
        var rendered = new RenderedContext("content", 9000, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = 8000 } };
        var opts = DefaultRenderOptions with { EstimatedTokens = 8000 };

        var results = OutputSelfCheck.Check(rendered, model, opts);

        var r = results.First(x => x.CheckId == "budget-respected");
        Assert.False(r.Passed);
    }

    [Fact]
    public void BudgetRespected_WithinSafetyMargin_Passes()
    {
        var rendered = new RenderedContext("content", 8300, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = 8000 } };
        var opts = DefaultRenderOptions with { EstimatedTokens = 8000 };

        var results = OutputSelfCheck.Check(rendered, model, opts);

        var r = results.First(x => x.CheckId == "budget-respected");
        Assert.True(r.Passed);
    }

    // ── no-empty-sections ─────────────────────────────────────────────

    [Fact]
    public void NoEmptySections_EmptySection_Fails()
    {
        var content = """
            ## Architecture overview
            N-Layer

            ## Endpoints


            ## Call graph
            None
            """;
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "no-empty-sections");
        Assert.False(r.Passed);
        Assert.Contains("Endpoints", r.Detail);
    }

    [Fact]
    public void NoEmptySections_AllFilled_Passes()
    {
        var content = """
            ## Architecture overview
            N-Layer

            ## Endpoints
            | Method | Route |
            |--------|-------|
            | GET | /api |

            ## Call graph
            None
            """;
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "no-empty-sections");
        Assert.True(r.Passed);
    }

    // ── sections-known ────────────────────────────────────────────────

    [Fact]
    public void SectionsKnown_KnownHeaders_Passes()
    {
        var content = """
            ## Architecture overview
            Content

            ## Endpoints
            Content
            """;
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "sections-known");
        Assert.True(r.Passed);
    }

    [Fact]
    public void SectionsKnown_UnknownHeader_Fails()
    {
        var content = """
            ## Architecture overview
            Content

            ## Random Unknown Section
            Content
            """;
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "sections-known");
        Assert.False(r.Passed);
        Assert.Contains("Random Unknown Section", r.Detail);
    }

    [Fact]
    public void SectionsKnown_DevContextTitle_IsExcluded()
    {
        var content = """
            ## DevContext — overview on MyApp
            Content
            """;
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "sections-known");
        Assert.True(r.Passed);
    }

    // ── detections-sourced ────────────────────────────────────────────

    [Fact]
    public void DetectionsSourced_AllSourced_Passes()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api", "Handler", "Get",
            [], [], null)
        {
            ExtractorName = "Test",
            SourceFile = "Program.cs",
            LineNumber = 10
        });

        var rendered = new RenderedContext("{}", 10, [], TimeSpan.Zero, "1.0");
        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "detections-sourced");
        Assert.True(r.Passed);
    }

    [Fact]
    public void DetectionsSourced_EmptySourceFile_Fails()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api", "Handler", "Get",
            [], [], null)
        {
            ExtractorName = "Test",
            SourceFile = "",
            LineNumber = 10
        });

        var rendered = new RenderedContext("{}", 10, [], TimeSpan.Zero, "1.0");
        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "detections-sourced");
        Assert.False(r.Passed);
    }

    [Fact]
    public void DetectionsSourced_ZeroLineNumber_Fails()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api", "Handler", "Get",
            [], [], null)
        {
            ExtractorName = "Test",
            SourceFile = "Program.cs",
            LineNumber = 0
        });

        var rendered = new RenderedContext("{}", 10, [], TimeSpan.Zero, "1.0");
        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "detections-sourced");
        Assert.False(r.Passed);
    }

    // ── no-duplicate-detections ───────────────────────────────────────

    [Fact]
    public void NoDuplicateDetections_UniqueKeys_Passes()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api/a", "A", "GetA",
            [], [], null)
        {
            ExtractorName = "Test", SourceFile = "Program.cs", LineNumber = 10
        });
        model.Detections.Add(new EndpointDetection("POST", "/api/b", "B", "PostB",
            [], [], null)
        {
            ExtractorName = "Test", SourceFile = "Program.cs", LineNumber = 20
        });

        var rendered = new RenderedContext("{}", 10, [], TimeSpan.Zero, "1.0");
        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "no-duplicate-detections");
        Assert.True(r.Passed);
    }

    [Fact]
    public void NoDuplicateDetections_SameKey_Fails()
    {
        var model = new DiscoveryModel();
        var det1 = new EndpointDetection("GET", "/api/x", "X", "Get",
            [], [], null)
        {
            ExtractorName = "Test", SourceFile = "Program.cs", LineNumber = 10
        };
        var det2 = new EndpointDetection("GET", "/api/x", "X", "Get",
            [], [], null)
        {
            ExtractorName = "Other", SourceFile = "Program.cs", LineNumber = 10
        };
        model.Detections.Add(det1);
        model.Detections.Add(det2);

        var rendered = new RenderedContext("{}", 10, [], TimeSpan.Zero, "1.0");
        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "no-duplicate-detections");
        Assert.False(r.Passed);
    }

    [Fact]
    public void NoDuplicateDetections_DifferentLine_Passes()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api/x", "X", "Get",
            [], [], null)
        {
            ExtractorName = "Test", SourceFile = "Program.cs", LineNumber = 10
        });
        model.Detections.Add(new EndpointDetection("GET", "/api/x", "X", "Get",
            [], [], null)
        {
            ExtractorName = "Test", SourceFile = "Program.cs", LineNumber = 20
        });

        var rendered = new RenderedContext("{}", 10, [], TimeSpan.Zero, "1.0");
        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "no-duplicate-detections");
        Assert.True(r.Passed);
    }

    // ── no-dynamic-routes ─────────────────────────────────────────────

    [Fact]
    public void NoDynamicRoutes_CleanContent_Passes()
    {
        var rendered = new RenderedContext("## Endpoints\n| GET | /api/users |",
            100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "no-dynamic-routes");
        Assert.True(r.Passed);
    }

    [Fact]
    public void NoDynamicRoutes_ContainsDynamic_Fails()
    {
        var rendered = new RenderedContext("## Endpoints\n| GET | <dynamic> |",
            100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "no-dynamic-routes");
        Assert.False(r.Passed);
    }

    // ── json-schema-valid ─────────────────────────────────────────────

    [Fact]
    public void JsonSchemaValid_ValidJson_Passes()
    {
        var content = """
            {
              "schemaVersion": "1.0",
              "solution": null,
              "architecture": null,
              "signals": [],
              "projects": { "count": 0, "names": [] },
              "typesSummary": { "found": 0, "inOutput": 0, "prunedPercent": 0.0 },
              "detections": [],
              "maxTokens": 8000
            }
            """;
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "json-schema-valid");
        Assert.True(r.Passed);
    }

    [Fact]
    public void JsonSchemaValid_InvalidJson_Fails()
    {
        var content = """{ invalid json @@@ }""";
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "json-schema-valid");
        Assert.False(r.Passed);
    }

    // ── html-well-formed ──────────────────────────────────────────────

    [Fact]
    public void HtmlWellFormed_ValidHtml_Passes()
    {
        var content = "<article class='dc-report'><section id='test'><h2>Test</h2><p>ok</p></section></article>";
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "html-well-formed");
        Assert.True(r.Passed);
    }

    [Fact]
    public void HtmlWellFormed_UnclosedTag_Fails()
    {
        var content = "<article><section><h2>Test</h2><p>broken</section>";
        var rendered = new RenderedContext(content, 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "html-well-formed");
        Assert.False(r.Passed);
    }

    [Fact]
    public void HtmlWellFormed_NonHtml_NotApplicable()
    {
        var rendered = new RenderedContext("## Markdown content", 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "html-well-formed");
        Assert.True(r.Passed);
        Assert.Contains("N/A", r.Detail);
    }

    // ── funnel-consistent ─────────────────────────────────────────────

    [Fact]
    public void FunnelConsistent_EmptyModel_NotApplicable()
    {
        var model = new DiscoveryModel();
        var rendered = new RenderedContext("", 0, [], TimeSpan.Zero, "1.0");

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "funnel-consistent");
        Assert.True(r.Passed);
        Assert.Contains("N/A", r.Detail);
    }

    [Fact]
    public void FunnelConsistent_BalancedModel_Passes()
    {
        var model = new DiscoveryModel();
        var type = new TypeDiscovery
        {
            Id = "t1",
            Name = "MyType",
            Namespace = "ns",
            FilePath = "f.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            IsHardExcluded = true
        };
        model.Types["t1"] = type;
        model.PrunedTypeIds.Add("t1");

        var rendered = new RenderedContext("", 0, [], TimeSpan.Zero, "1.0");

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "funnel-consistent");
        Assert.True(r.Passed);
    }

    [Fact]
    public void FunnelConsistent_ImbalancedModel_Fails()
    {
        var model = new DiscoveryModel();
        var type = new TypeDiscovery
        {
            Id = "t1",
            Name = "MyType",
            Namespace = "ns",
            FilePath = "f.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            // IsHardExcluded stays false by default
        };
        model.Types["t1"] = type;
        // PrunedTypeIds has "t1" but type.IsHardExcluded is false — creates imbalance
        model.PrunedTypeIds.Add("t1");

        var rendered = new RenderedContext("", 0, [], TimeSpan.Zero, "1.0");

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions);

        var r = results.First(x => x.CheckId == "funnel-consistent");
        Assert.False(r.Passed);
    }

    // ── deterministic (test-only) ─────────────────────────────────────

    [Fact]
    public void Deterministic_WhenRequested_ReturnsNotApplicable()
    {
        var rendered = new RenderedContext("content", 100, [], TimeSpan.Zero, "1.0");
        var model = new DiscoveryModel();

        var results = OutputSelfCheck.Check(rendered, model, DefaultRenderOptions, includeTestOnly: true);

        var r = results.First(x => x.CheckId == "deterministic");
        Assert.True(r.Passed); // NotApplicable returns Pass with "N/A" detail
        Assert.Contains("N/A", r.Detail);
    }

    // ── BuildStableKey ────────────────────────────────────────────────

    [Fact]
    public void BuildStableKey_DifferentEndpoints_DifferentKeys()
    {
        var a = new EndpointDetection("GET", "/api/a", "A", "Get", [], [], null)
            { ExtractorName = "T", SourceFile = "f.cs", LineNumber = 1 };
        var b = new EndpointDetection("POST", "/api/a", "A", "Post", [], [], null)
            { ExtractorName = "T", SourceFile = "f.cs", LineNumber = 1 };

        var keyA = OutputSelfCheck.BuildStableKey(a);
        var keyB = OutputSelfCheck.BuildStableKey(b);

        Assert.NotEqual(keyA, keyB);
    }

    [Fact]
    public void BuildStableKey_SameEndpoints_SameKey()
    {
        var a = new EndpointDetection("GET", "/api/x", "X", "Get", [], [], null)
            { ExtractorName = "T", SourceFile = "f.cs", LineNumber = 10 };
        var b = new EndpointDetection("GET", "/api/x", "X", "Get", [], [], null)
            { ExtractorName = "T", SourceFile = "f.cs", LineNumber = 10 };

        var keyA = OutputSelfCheck.BuildStableKey(a);
        var keyB = OutputSelfCheck.BuildStableKey(b);

        Assert.Equal(keyA, keyB);
    }

    [Fact]
    public void BuildStableKey_DiRegistrations_DifferentKeys()
    {
        var a = new DiRegistrationDetection("ISvc", "ImplA", "Scoped",
            [], DiRegistrationShape.DirectBinding, null)
            { ExtractorName = "T", SourceFile = "s.cs", LineNumber = 1 };
        var b = new DiRegistrationDetection("ISvc", "ImplB", "Scoped",
            [], DiRegistrationShape.DirectBinding, null)
            { ExtractorName = "T", SourceFile = "s.cs", LineNumber = 1 };

        var keyA = OutputSelfCheck.BuildStableKey(a);
        var keyB = OutputSelfCheck.BuildStableKey(b);

        Assert.NotEqual(keyA, keyB);
    }
}
