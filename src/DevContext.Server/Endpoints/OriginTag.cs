namespace DevContext.Server.Endpoints;

/// <summary>
/// F5 (Prism D4.5) — classifies a request's tool-call origin: the desktop app calls over
/// gRPC-web (<c>application/grpc-web*</c>), the MCP sidecar over native gRPC
/// (<c>application/grpc</c>). The classification MUST run before <c>UseGrpcWeb</c>: that
/// middleware rewrites the request content-type to <c>application/grpc</c> so the standard
/// gRPC endpoint can serve it — sniffing afterwards tagged every app RPC "agent" (the
/// audit's F5: GetFlowIndex/GetStats in the "agents only" feed). Program.cs stashes the
/// verdict in <see cref="ItemKey"/> pre-rewrite; RecordToolCall reads it back.
/// </summary>
public static class OriginTag
{
    public const string ItemKey = "DevContext.Origin";

    /// <summary>The desktop app's own gRPC-web traffic.</summary>
    public const string Ui = "ui";

    /// <summary>Native gRPC — the MCP sidecar, i.e. an agent. N4.1's status card counts these.</summary>
    public const string Agent = "agent";

    public static string FromContentType(string? contentType)
        => contentType?.Contains("grpc-web", StringComparison.OrdinalIgnoreCase) == true ? Ui : Agent;
}
