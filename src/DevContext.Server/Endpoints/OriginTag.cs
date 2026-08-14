namespace DevContext.Server.Endpoints;

/// <summary>
/// F5 (Prism D4.5) — classifies a request's tool-call origin. Program.cs stashes the verdict in
/// <see cref="ItemKey"/> before <c>UseGrpcWeb</c> rewrites the content-type; RecordToolCall
/// reads it back.
///
/// N4.1 — the content-type rule this replaced was WRONG, and measured wrong: it assumed "the
/// desktop app calls over gRPC-web, the MCP sidecar over native gRPC", but the sidecar wraps its
/// channel in <c>GrpcWebHandler</c> too (DevContext.Mcp/Program.cs), so BOTH sides arrive as
/// gRPC-web and every agent call was tagged "ui". The MCP page's feed default-filters to agents,
/// so real agent traffic had been rendering as nothing at all. Headers captured off the wire
/// 2026-08-14 (dev server, one page load + one real devcontext-mcp session):
///
///   app      ct=application/grpc-web+proto  ua=Mozilla/5.0 ... HeadlessChrome/149
///                                           x-user-agent=connect-es/2.1.2  origin=http://localhost:4200
///   sidecar  ct=application/grpc-web        ua=grpc-dotnet/2.83.0 (.NET 10.0.9; ...)
///                                           x-user-agent=(none)             origin=(none)
///
/// So the discriminator is the CLIENT, not the framing: three independent browser signals, any
/// one of which means "ui". The default leans "ui", because a stranger inflating the agent
/// numbers is a worse failure than under-counting: this is the input to a status card whose
/// whole job is to stop claiming things it did not measure.
/// </summary>
public static class OriginTag
{
    public const string ItemKey = "DevContext.Origin";

    /// <summary>The desktop app's own gRPC-web traffic.</summary>
    public const string Ui = "ui";

    /// <summary>Native gRPC — the MCP sidecar, i.e. an agent. N4.1's status card counts these.</summary>
    public const string Agent = "agent";

    /// <summary>Classify one request by who sent it. Framing is deliberately not consulted.</summary>
    public static string FromRequest(string? userAgent, string? xUserAgent, string? origin)
    {
        // A browser XHR to another port always carries Origin; the app's client library always
        // stamps x-user-agent; and a browser UA says Mozilla. Any one of the three is decisive.
        if (!string.IsNullOrWhiteSpace(origin)) return Ui;
        if (Contains(xUserAgent, "connect-es")) return Ui;
        if (Contains(userAgent, "Mozilla")) return Ui;

        // No browser signal at all: a native gRPC client — the MCP sidecar, or something else
        // driving this server the way an agent host would.
        return Agent;
    }

    private static bool Contains(string? haystack, string needle)
        => haystack?.Contains(needle, StringComparison.OrdinalIgnoreCase) == true;
}
