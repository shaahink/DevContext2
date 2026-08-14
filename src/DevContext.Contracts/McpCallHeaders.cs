namespace DevContext.Protos;

/// <summary>
/// N4.3 (STUDIO-MCP audit §4, Room 2) — the header names the MCP sidecar stamps on every gRPC
/// call it makes and the server reads back onto <c>ToolCallEvent</c>.
///
/// <para>They live HERE, beside the generated contract, because they are part of the same wire
/// agreement: the sidecar (DevContext.Mcp) and the server (DevContext.Server) reference this
/// project and nothing else in common. Two string literals in two projects is how a header
/// quietly stops matching after a rename — the same species of drift as the tool list this
/// checkpoint deleted.</para>
/// </summary>
public static class McpCallHeaders
{
    /// <summary>The MCP verb the agent called: <c>trace</c>, <c>get_context</c>, <c>analyze</c>.</summary>
    public const string Tool = "x-mcp-tool";

    /// <summary>
    /// A short summary of the arguments the agent sent, base64 of UTF-8.
    ///
    /// <para>Encoded because gRPC metadata values on a non-binary key are ASCII by spec and this
    /// carries repo text — a focus is whatever a user typed, in whatever script their codebase
    /// uses. The <c>-b64</c> suffix is in the name so a reader of the wire is never guessing.</para>
    /// </summary>
    public const string Args = "x-mcp-args-b64";

    /// <summary>The one argument identifying what the call was about, base64 of UTF-8.</summary>
    public const string PrimaryArg = "x-mcp-arg1-b64";
}
