using System.Text.Json;

using DevContext.Mcp;
using DevContext.Protos;

using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Server.Tests;

/// <summary>
/// G1.4 (R4 §1 item 7) — what <c>analyze</c> tells the agent it just did.
///
/// Before this checkpoint the tool read the server's event stream for one field and threw the rest
/// away: it kept <c>Result.Handle</c> and returned <c>{handle, status:"ready", hint}</c> — the same
/// eleven words whether the call had spent eight minutes analysing or two milliseconds reusing an
/// open session, with the AnalysisSummary the server had already computed discarded at the loop
/// (measured: eval-results/2026-07-29/mcp-r4-g14-before/analyze-honesty-first.json).
///
/// The Error arm was dropped the same way, which is the sharper half: a failed analysis fell
/// through to the generic "did not return a handle", so the server's own reason — "Git is not
/// installed", "Repository not found" — never reached the caller.
/// </summary>
public sealed class McpAnalyzeEnvelopeTests
{
    private static DevContextTools ToolsFor(AnalyzeEvent evt) => new(
        new DevContextService.DevContextServiceClient(
            new McpStubCallInvoker(rpc => rpc == "Analyze" ? evt : null)),
        NullLogger<DevContextTools>.Instance);

    private static AnalyzeEvent Analyzed(bool cached) => new()
    {
        Result = new AnalyzeResult
        {
            Handle = "handle-1",
            Cached = cached,
            Summary = new AnalysisSummary
            {
                Label = "eShop",
                Archetype = "Service",
                Projects = 12,
                Nodes = 4210,
                Edges = 8801,
                Entries = 96,
                EntriesWithTarget = 88,
                ElapsedMs = 128_400,
                Explanation = "full analysis",
            },
        },
    };

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    [Fact]
    public async Task The_summary_the_server_computed_reaches_the_agent()
    {
        var root = Parse(await ToolsFor(Analyzed(cached: false)).Analyze("C:/repos/eShop"));

        var summary = root.GetProperty("summary");
        Assert.Equal("eShop", summary.GetProperty("label").GetString());
        Assert.Equal("Service", summary.GetProperty("archetype").GetString());
        Assert.Equal(4210, summary.GetProperty("nodes").GetInt32());
        Assert.Equal(8801, summary.GetProperty("edges").GetInt32());
        Assert.Equal(96, summary.GetProperty("entries").GetInt32());
        Assert.Equal(128_400, summary.GetProperty("elapsedMs").GetInt64());
    }

    /// <summary>The flag is the server's answer, passed through — not a guess made here.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Cached_reports_what_the_server_said(bool cached)
    {
        var root = Parse(await ToolsFor(Analyzed(cached)).Analyze("C:/repos/eShop"));

        Assert.Equal(cached, root.GetProperty("cached").GetBoolean());
    }

    /// <summary>
    /// And the note has to move with it. A fixed sentence would be no more honest than the fixed
    /// hint it replaces: the point is that a caller can tell which of the two things happened.
    /// </summary>
    [Fact]
    public async Task The_note_distinguishes_a_cache_hit_from_an_analysis()
    {
        var fresh = Parse(await ToolsFor(Analyzed(cached: false)).Analyze("C:/repos/eShop"))
            .GetProperty("note").GetString()!;
        var reused = Parse(await ToolsFor(Analyzed(cached: true)).Analyze("C:/repos/eShop"))
            .GetProperty("note").GetString()!;

        Assert.NotEqual(fresh, reused);
        Assert.Contains("can take minutes", fresh, StringComparison.Ordinal);
        Assert.Contains("snapshotted", fresh, StringComparison.Ordinal);
        Assert.Contains("no analysis ran", reused, StringComparison.Ordinal);
    }

    /// <summary>
    /// The server said why it failed. Saying "did not return a handle" instead is the tool
    /// substituting its own guess for an answer it was given.
    /// </summary>
    [Fact]
    public async Task A_streamed_error_surfaces_the_servers_reason()
    {
        var tools = ToolsFor(new AnalyzeEvent
        {
            Error = new AnalyzeError { Code = "GitNotInstalled", Message = "Git is not installed. Install Git to clone GitHub repositories." },
        });

        var root = Parse(await tools.Analyze("github.com/dotnet/eShop"));

        Assert.Contains("Git is not installed", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
        Assert.Contains("GitNotInstalled", root.GetProperty("hint").GetString()!, StringComparison.Ordinal);
        Assert.False(root.TryGetProperty("handle", out _));
    }

    /// <summary>A stream that ends with neither result nor error keeps the old honest fallback.</summary>
    [Fact]
    public async Task An_empty_stream_still_says_so()
    {
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(_ => null)),
            NullLogger<DevContextTools>.Instance);

        var root = Parse(await tools.Analyze("C:/repos/NotARepo"));

        Assert.Contains("did not return a handle", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
    }
}
