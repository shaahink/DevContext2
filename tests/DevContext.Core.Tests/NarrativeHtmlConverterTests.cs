namespace DevContext.Core.Tests;

public sealed class NarrativeHtmlConverterTests
{
    // The desktop Human (HTML) view styles section headers as <h3>. Before the fix, IsSectionHeader only
    // recognized the app-Map headers, so the LibrarySurfaceRenderer sections rendered as plain text.
    [Theory]
    [InlineData("ENTRY API")]
    [InlineData("ABSTRACTIONS")]
    [InlineData("GENERATORS")]
    [InlineData("PUBLIC SURFACE")]
    [InlineData("CONSUMER PATHS")]
    public void Library_surface_headers_render_as_section_titles(string header)
    {
        var html = NarrativeHtmlConverter.Convert($"{header}\n   some content\n");

        Assert.Contains($"<h3 class='narrative-section-title'>{header}</h3>", html);
    }

    [Fact]
    public void App_map_headers_still_render_as_section_titles()
    {
        var html = NarrativeHtmlConverter.Convert("TOPOLOGY\n   A -> B\n");

        Assert.Contains("<h3 class='narrative-section-title'>TOPOLOGY</h3>", html);
    }
}
