using DevContext.Server.Endpoints;

namespace DevContext.Server.Tests;

/// <summary>
/// F5 (Prism D4.5) — ui/agent classification. The stash-then-read mechanics live in Program.cs;
/// the classification truth is here.
///
/// N4.1 — these rows are HEADERS CAPTURED OFF THE WIRE (2026-08-14, dev server: one page load
/// plus one real devcontext-mcp session driven over stdio), not a guess about who frames what.
/// The rule they replaced classified by content-type on the premise that the sidecar spoke
/// native gRPC; it wraps its channel in GrpcWebHandler, so both sides said "grpc-web" and every
/// agent call was tagged "ui" — with the MCP feed defaulting to agents-only, real agent traffic
/// rendered as an empty page.
/// </summary>
public sealed class OriginTagTests
{
    private const string BrowserUa =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/149.0.7827.55 Safari/537.36";
    private const string SidecarUa = "grpc-dotnet/2.83.0 (.NET 10.0.9; CLR 10.0.9; net10.0; windows; x64)";

    [Fact]
    public void The_desktop_app_is_ui()
    {
        // Exactly what the app sends: browser UA, connect-es, and an Origin.
        Assert.Equal("ui", OriginTag.FromRequest(BrowserUa, "connect-es/2.1.2", "http://localhost:4200"));
    }

    [Fact]
    public void The_MCP_sidecar_is_an_agent()
    {
        // Exactly what devcontext-mcp sends — over gRPC-web, which is why framing cannot decide.
        Assert.Equal("agent", OriginTag.FromRequest(SidecarUa, null, null));
    }

    [Theory]
    // Any ONE browser signal is decisive, so a stray missing header cannot promote the app to
    // "agent" and inflate the numbers the status card reports.
    [InlineData(null, null, "tauri://localhost")]
    [InlineData(null, "connect-es/2.1.2", null)]
    [InlineData(BrowserUa, null, null)]
    public void Any_single_browser_signal_means_ui(string? userAgent, string? xUserAgent, string? origin)
        => Assert.Equal("ui", OriginTag.FromRequest(userAgent, xUserAgent, origin));

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("", "", "")]
    [InlineData("Grpc.Net.Client/2.83", null, "   ")]
    public void A_client_with_no_browser_signal_at_all_is_an_agent(string? userAgent, string? xUserAgent, string? origin)
        => Assert.Equal("agent", OriginTag.FromRequest(userAgent, xUserAgent, origin));
}
