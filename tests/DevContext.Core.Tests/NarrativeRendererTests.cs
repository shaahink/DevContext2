using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>Covers the section-aware Map/Trace narrative: renderers emit per-section fragments whose
/// concatenation reproduces the monolithic Content (so CLI/golden output is unchanged), and the
/// desktop can toggle them in sync across both views.</summary>
public sealed class NarrativeRendererTests
{
    private static RunReport DefaultReport => new()
    {
        Stages = [], Extractors = [], Scorers = [], Compressions = [],
        Cache = new(0, 0, 0, 0), Corpus = new(0, 0, 0),
        Funnel = new(0, 0, 0, 0, 0, 0),
        Parallelism = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
        TotalWall = TimeSpan.Zero,
    };

    private static CodeGraph EmptyGraph => new(
        new Dictionary<NodeId, GraphNode>(),
        new Dictionary<NodeId, ImmutableArray<GraphEdge>>());

    [Fact]
    public void Assembler_content_is_concatenation_of_fragments()
    {
        var sections = new List<NarrativeSection>
        {
            new("Overview", "MAP App\n\n"),
            new("Topology", "TOPOLOGY\n   A\n\n"),
        };

        var rc = NarrativeSections.ToRenderedContext(sections);

        Assert.Equal("MAP App\n\nTOPOLOGY\n   A\n\n", rc.Content);
        Assert.Equal(2, rc.Sections.Length);
        Assert.NotNull(rc.SectionFragments);
        Assert.Equal("MAP App\n\n", rc.SectionFragments!["Overview"]);
        Assert.Equal("TOPOLOGY\n   A\n\n", rc.SectionFragments!["Topology"]);
    }

    [Fact]
    public void WithExtraSection_appends_to_content_and_fragments()
    {
        var rc = NarrativeSections.ToRenderedContext([new NarrativeSection("Overview", "MAP App\n")]);

        var withTail = NarrativeSections.WithExtraSection(rc, "Diagnostics", "\nDIAGNOSTICS\n  x\n");

        Assert.EndsWith("\nDIAGNOSTICS\n  x\n", withTail.Content);
        Assert.Contains(withTail.Sections, s => s.Name == "Diagnostics");
        Assert.Equal("\nDIAGNOSTICS\n  x\n", withTail.SectionFragments!["Diagnostics"]);
    }

    [Fact]
    public void WithExtraSection_is_noop_for_empty_text()
    {
        var rc = NarrativeSections.ToRenderedContext([new NarrativeSection("Overview", "MAP App\n")]);

        var same = NarrativeSections.WithExtraSection(rc, "Diagnostics", "");

        Assert.Equal(rc.Content, same.Content);
        Assert.Equal(rc.Sections.Length, same.Sections.Length);
    }

    [Fact]
    public async Task MapRenderer_emits_section_fragments_that_rejoin_to_content()
    {
        var model = new DiscoveryModel
        {
            Solution = new SolutionInfo(@"C:\repo\App.sln", "App",
                [@"C:\repo\src\A\A.csproj", @"C:\repo\src\B\B.csproj"]),
            Projects =
            [
                new ProjectInfo("A", @"C:\repo\src\A\A.csproj", "C#", ["net10.0"], ["B"],
                    [new PackageReferenceInfo("MediatR", "12.0.0")]),
                new ProjectInfo("B", @"C:\repo\src\B\B.csproj", "C#", ["net10.0"], [], []),
            ],
            DetectedStyle = ArchitectureStyle.MinimalApi,
            StyleConfidence = 0.9f,
            StyleDetectedVia = "Minimal APIs",
        };
        var entries = ImmutableArray.Create(
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /todos", NodeId.ForEntry("GET /todos")));
        var map = MapBuilder.Build(model, EmptyGraph, entries);

        var snapshot = new AnalysisSnapshot
        {
            Model = model,
            Analysis = new SharedAnalysisContext(),
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(),
            Report = DefaultReport,
            Graph = EmptyGraph,
            Map = map,
            Entries = entries,
        };

        var request = new RenderRequest { Format = "markdown", MaxTokens = 8000 };
        var result = await MapRenderer.RenderAsync(new MapRenderContext(map, snapshot, "markdown", request), default);

        // Expected toggleable blocks are present as sections + fragments.
        Assert.Contains(result.Sections, s => s.Name == "Overview");
        Assert.Contains(result.Sections, s => s.Name == "Topology");
        Assert.Contains(result.Sections, s => s.Name == "Entry points");
        Assert.Contains(result.Sections, s => s.Name == "Packages");
        Assert.NotNull(result.SectionFragments);

        // Concatenating the fragments in section order reproduces the monolithic Content.
        var joined = string.Concat(result.Sections.Select(s => result.SectionFragments![s.Name]));
        Assert.Equal(result.Content, joined);

        // Sanity: the narrative still reads like a Map.
        Assert.Contains("MAP", result.Content);
        Assert.Contains("GET /todos", result.Content);
    }
}
