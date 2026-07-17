using DevContext.Core.Graph;
using DevContext.Core.Utilities;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Tests;

/// <summary>C1 (Prism D2): .razor @code blocks into the call graph — the virtualizer extracts ONLY
/// the @code/@functions/@inject C#, never the markup, and #line-maps it back to true razor lines.</summary>
public sealed class RazorCodeVirtualizerTests
{
    // ── Block extraction ─────────────────────────────────────────────────────

    [Fact]
    public void ExtractCodeBlocks_finds_a_code_block_and_skips_markup()
    {
        const string razor = """
            @page "/discover"
            <div class="x">@item.Name</div>
            @code {
                private int count;
                public void Increment() { count++; }
            }
            """;
        var blocks = RazorCodeVirtualizer.ExtractCodeBlocks(razor);
        var block = Assert.Single(blocks);
        var content = razor[block.ContentStart..block.ContentEnd];
        Assert.Contains("Increment", content);
        Assert.DoesNotContain("div", content);
    }

    [Fact]
    public void ExtractCodeBlocks_braces_inside_strings_comments_and_chars_do_not_break_balance()
    {
        const string razor = """
            @code {
                // a } in a line comment
                /* and a } in a block comment */
                private string a = "}";
                private string b = @"}}";
                private string c = $"{x}";
                private char d = '}';
                public void M() { }
            }
            <p>after</p>
            """;
        var blocks = RazorCodeVirtualizer.ExtractCodeBlocks(razor);
        var block = Assert.Single(blocks);
        var content = razor[block.ContentStart..block.ContentEnd];
        Assert.Contains("public void M()", content);
        Assert.DoesNotContain("after", content);
    }

    [Fact]
    public void ExtractCodeBlocks_functions_alias_and_multiple_blocks()
    {
        const string razor = """
            @functions {
                public int A() => 1;
            }
            <p>markup</p>
            @code {
                public int B() => 2;
            }
            """;
        var blocks = RazorCodeVirtualizer.ExtractCodeBlocks(razor);
        Assert.Equal(2, blocks.Count);
    }

    [Fact]
    public void ExtractCodeBlocks_unbalanced_block_is_skipped_not_swallowed()
    {
        const string razor = """
            @code {
                public void M() {
            """;
        Assert.Empty(RazorCodeVirtualizer.ExtractCodeBlocks(razor));
    }

    [Fact]
    public void ExtractCodeBlocks_email_like_at_code_is_not_a_directive()
    {
        Assert.Empty(RazorCodeVirtualizer.ExtractCodeBlocks("<a href=\"mailto:x@code.dev\">x@code{y}</a>"));
    }

    // ── Virtual source ───────────────────────────────────────────────────────

    [Fact]
    public void BuildVirtualSource_wraps_code_in_component_class_with_inject_properties()
    {
        const string razor = """
            @page "/discover"
            @inject PodcastService PodcastService
            <h1>Discover</h1>
            @code {
                protected override async Task OnInitializedAsync()
                {
                    var shows = await PodcastService.GetShows(10, null);
                }
            }
            """;
        var source = RazorCodeVirtualizer.BuildVirtualSource(
            @"C:\repo\src\Web\Pages\DiscoverPage.razor", razor, "Podcast.Pages.Pages");
        Assert.NotNull(source);
        Assert.Contains("namespace Podcast.Pages.Pages;", source);
        Assert.Contains("public partial class DiscoverPage : Microsoft.AspNetCore.Components.ComponentBase", source);
        Assert.Contains("public PodcastService PodcastService { get; set; }", source);
        Assert.Contains("OnInitializedAsync", source);
        Assert.DoesNotContain("<h1>", source); // markup never parsed, never emitted

        // The virtual source must be structurally valid C# (no parse errors from the wrapping).
        var tree = CSharpSyntaxTree.ParseText(source);
        Assert.DoesNotContain(tree.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
    }

    [Fact]
    public void BuildVirtualSource_line_directives_map_invocations_to_true_razor_lines()
    {
        const string razor = """
            @page "/discover"
            @inject PodcastService PodcastService
            <h1>Discover</h1>
            @code {
                protected override async Task OnInitializedAsync()
                {
                    await PodcastService.GetShows(10, null);
                }
            }
            """;
        var path = @"C:\repo\src\Web\Pages\DiscoverPage.razor";
        var source = RazorCodeVirtualizer.BuildVirtualSource(path, razor, "Podcast.Pages.Pages");
        var tree = CSharpSyntaxTree.ParseText(source!, path: path);
        var invocation = tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().Single();
        var mapped = invocation.GetLocation().GetMappedLineSpan();
        Assert.Equal(path, mapped.Path);
        Assert.Equal(7, mapped.StartLinePosition.Line + 1); // the razor line of GetShows(...)
    }

    [Fact]
    public void BuildVirtualSource_honors_inherits_and_inherited_usings()
    {
        const string razor = """
            @inherits LayoutComponentBase
            @using My.Own.Ns
            @code {
                public void M() { }
            }
            """;
        var source = RazorCodeVirtualizer.BuildVirtualSource(
            @"C:\repo\src\Web\Shared\MainLayout.razor", razor, "Web.Shared",
            inheritedUsings: ["Podcast.Pages.Data"]);
        Assert.Contains("class MainLayout : LayoutComponentBase", source);
        Assert.Contains("using Podcast.Pages.Data;", source);
        Assert.Contains("using My.Own.Ns;", source);
    }

    [Fact]
    public void BuildVirtualSource_markup_only_component_returns_null()
    {
        Assert.Null(RazorCodeVirtualizer.BuildVirtualSource(
            @"C:\repo\src\Web\App.razor", "<Router AppAssembly=\"@typeof(App).Assembly\" />", "Web"));
    }

    [Fact]
    public void BuildVirtualSource_inject_only_component_still_produces_the_type()
    {
        var source = RazorCodeVirtualizer.BuildVirtualSource(
            @"C:\repo\src\Web\Widget.razor", "@inject IClock Clock\n<p>@Clock.Now</p>", "Web");
        Assert.NotNull(source);
        Assert.Contains("public IClock Clock { get; set; }", source);
    }

    // ── End-to-end: structure + call graph over a fake Blazor repo ───────────

    private static (DiscoveryContext Ctx, DiscoveryModel Model) BuildBlazorRepo()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Podcast.Pages\Podcast.Pages.csproj",
            "<Project Sdk=\"Microsoft.NET.Sdk.Razor\"></Project>");
        fs.AddFile(@"C:\repo\src\Podcast.Pages\_Imports.razor", """
            @using Microsoft.AspNetCore.Components
            @using Podcast.Pages.Data
            """);
        fs.AddFile(@"C:\repo\src\Podcast.Pages\Pages\DiscoverPage.razor", """
            @page "/discover"
            @inject PodcastService PodcastService
            <h1>Discover</h1>
            @code {
                protected override async Task OnInitializedAsync()
                {
                    await PodcastService.GetShows(10, null);
                }
            }
            """);
        fs.AddFile(@"C:\repo\src\Podcast.Pages\Data\PodcastService.cs", """
            namespace Podcast.Pages.Data;

            public sealed class PodcastService
            {
                public Task<string[]> GetShows(int max, string? term) => Task.FromResult(System.Array.Empty<string>());
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo")
            .BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Podcast.Pages\Data\PodcastService.cs"];
        ctx.Analysis.AllContentFiles =
            [@"C:\repo\src\Podcast.Pages\_Imports.razor", @"C:\repo\src\Podcast.Pages\Pages\DiscoverPage.razor"];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\Podcast.Pages\Podcast.Pages.csproj"];
        ctx.Analysis.FocusPoints = [];
        return (ctx, new DiscoveryModel());
    }

    [Fact]
    public async Task SyntaxStructureExtractor_discovers_the_component_type_from_the_code_block()
    {
        var (ctx, model) = BuildBlazorRepo();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);

        // Namespace = csproj name + folder path (Blazor's default), so a .razor.cs partial would merge.
        Assert.True(model.Types.TryGetValue("Podcast.Pages.Pages.DiscoverPage", out var component));
        Assert.Equal(@"C:\repo\src\Podcast.Pages\Pages\DiscoverPage.razor", component!.FilePath);
        Assert.Contains(component.Methods, m => m.Name == "OnInitializedAsync");
        Assert.Contains(component.Properties, p => p.Name == "PodcastService");
    }

    [Fact]
    public async Task CallGraphExtractor_binds_component_code_calls_with_razor_line_provenance()
    {
        var (ctx, model) = BuildBlazorRepo();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);

        // The Blazor page detection seeds the entry-scoped bind, exactly as BlazorEntryExtractor emits it.
        model.Detections.Add(new EndpointDetection(
            "GET", "/discover", "DiscoverPage", "<component>", [], [])
        {
            ExtractorName = "BlazorEntryExtractor",
            SourceFile = @"C:\repo\src\Podcast.Pages\Pages\DiscoverPage.razor",
            LineNumber = 1,
        });

        await new CallGraphExtractor().ExtractAsync(ctx, model, default);

        var edge = Assert.Single(model.CallEdges, e =>
            e.CallerMethod == "OnInitializedAsync" && e.CalleeMethod == "GetShows");
        Assert.Equal("Podcast.Pages.Pages.DiscoverPage", edge.CallerType);
        Assert.EndsWith("PodcastService", edge.CalleeType);
        // #line honesty: the call site is the true razor line, in the razor file.
        Assert.Contains(@"DiscoverPage.razor:7", edge.CallSiteLocation);
    }

    [Fact]
    public async Task GraphBuilder_links_ui_entry_to_lifecycle_member_and_resolves_a_service_target()
    {
        var (ctx, model) = BuildBlazorRepo();
        model.Projects =
        [
            new ProjectInfo("Podcast.Pages", @"C:\repo\src\Podcast.Pages\Podcast.Pages.csproj", "C#", ["net10.0"], [], []),
        ];
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);
        model.Detections.Add(new EndpointDetection(
            "GET", "/discover", "DiscoverPage", "<component>", [], [])
        {
            ExtractorName = "BlazorEntryExtractor",
            SourceFile = @"C:\repo\src\Podcast.Pages\Pages\DiscoverPage.razor",
            LineNumber = 1,
        });
        await new CallGraphExtractor().ExtractAsync(ctx, model, default);

        var scope = SolutionScope.FromModel(model);
        var (graph, entries) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        var entry = Assert.Single(entries);
        Assert.Equal(EntryPointKind.UiEntry, entry.Kind);
        // The entry links at MEMBER level (the lifecycle method), so target/reach/trace all light up.
        Assert.NotNull(entry.HandlerNode);
        Assert.Contains("OnInitializedAsync", entry.HandlerNode!.Value.Key);
        Assert.Equal("PodcastService.GetShows", entry.Target);
        Assert.True(entry.Reach > 0, $"UI entry reach must be > 0, was {entry.Reach}");
    }
}
