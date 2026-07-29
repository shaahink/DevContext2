using System.Text.Json;

using DevContext.Mcp;
using DevContext.Protos;

using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Server.Tests;

/// <summary>
/// G1.3 (R4 §1 item 4) — which session a handle-less MCP call lands on.
///
/// The server orders <c>ListSessions</c> by LastAccess and bumps LastAccess on EVERY access
/// (AnalysisSessionManager.Get), so <c>Sessions[0]</c> means "whatever repo anyone touched most
/// recently" — the desktop app opening repo A silently retargeted an agent's next handle-less call
/// from repo B, and the answer came back about the wrong codebase with nothing to notice.
/// Measured before the fix: analyze(A), analyze(B), an explicit-handle call on A, then a handle-less
/// overview() went to A (eval-results/2026-07-29/mcp-r4-g13-before/retarget.txt).
///
/// status() is the probe here because it echoes the handle it resolved.
/// </summary>
public sealed class McpHandleScopeTests
{
    private const string TouchedByAnotherClient = "handle-A";
    private const string OursFromAnalyze = "handle-B";

    /// <summary>Sessions come back in the server's LastAccess order — A first, as if another client
    /// just touched it.</summary>
    private static DevContextTools Tools() => new(
        new DevContextService.DevContextServiceClient(new McpStubCallInvoker(rpc => rpc switch
        {
            "ListSessions" => McpErrorEnvelopeTests.Sessions(TouchedByAnotherClient, OursFromAnalyze),
            "Analyze" => new AnalyzeEvent { Result = new AnalyzeResult { Handle = OursFromAnalyze } },
            _ => null,
        })),
        NullLogger<DevContextTools>.Instance);

    private static string? Handle(string json)
        => JsonDocument.Parse(json).RootElement.TryGetProperty("handle", out var h) ? h.GetString() : null;

    [Fact]
    public async Task A_handle_less_call_goes_to_the_session_this_client_analyzed()
    {
        var tools = Tools();

        var analyzed = Handle(await tools.Analyze("C:/repos/B"));
        var resolved = Handle(await tools.Status());

        Assert.Equal(OursFromAnalyze, analyzed);
        Assert.Equal(OursFromAnalyze, resolved);
    }

    /// <summary>
    /// With nothing of our own and more than one session open, picking one is a guess. Name them
    /// instead — that is the defect, not the tiebreak.
    /// </summary>
    [Fact]
    public async Task With_no_session_of_our_own_it_refuses_to_guess_between_two()
    {
        var tools = Tools();

        using var doc = JsonDocument.Parse(await tools.Status());

        var error = doc.RootElement.GetProperty("error").GetString()!;
        Assert.Contains("analyzed nothing", error, StringComparison.Ordinal);
        Assert.Contains(TouchedByAnotherClient, error, StringComparison.Ordinal);
        Assert.Contains(OursFromAnalyze, error, StringComparison.Ordinal);
    }

    /// <summary>One open session is unambiguous — attaching to a session the desktop app made is a
    /// real workflow and still works.</summary>
    [Fact]
    public async Task One_open_session_still_resolves_without_a_handle()
    {
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(rpc =>
                rpc == "ListSessions" ? McpErrorEnvelopeTests.Sessions(TouchedByAnotherClient) : null)),
            NullLogger<DevContextTools>.Instance);

        Assert.Equal(TouchedByAnotherClient, Handle(await tools.Status()));
    }

    /// <summary>An explicit handle always wins — the default is a default, not a redirect.</summary>
    [Fact]
    public async Task An_explicit_handle_is_never_overridden()
    {
        var tools = Tools();
        await tools.Analyze("C:/repos/B");

        Assert.Equal(TouchedByAnotherClient, Handle(await tools.Status(TouchedByAnotherClient)));
    }

    /// <summary>Closing our session gives the memory up — a closed handle must not be resurrected.</summary>
    [Fact]
    public async Task Closing_our_session_forgets_it()
    {
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(rpc => rpc switch
            {
                "ListSessions" => McpErrorEnvelopeTests.Sessions(TouchedByAnotherClient, OursFromAnalyze),
                "Analyze" => new AnalyzeEvent { Result = new AnalyzeResult { Handle = OursFromAnalyze } },
                "CloseSession" => new CloseResponse { Closed = true },
                _ => null,
            })),
            NullLogger<DevContextTools>.Instance);

        await tools.Analyze("C:/repos/B");
        await tools.CloseSession(OursFromAnalyze);

        using var doc = JsonDocument.Parse(await tools.Status());
        Assert.Contains("analyzed nothing", doc.RootElement.GetProperty("error").GetString()!, StringComparison.Ordinal);
    }
}
