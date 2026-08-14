using System.Threading.Channels;

using DevContext.Protos;
using DevContext.Server.Endpoints;
using DevContext.Server.Services;

namespace DevContext.Server.Tests;

/// <summary>
/// N4.1 (STUDIO-MCP audit §3.D/§4 Room 2) — the MCP page performed status instead of measuring
/// it: a green dot that meant "the gRPC server answered", a Start/Stop pair that flipped a
/// global mute and touched no MCP process, and a header claiming a binary that the installer
/// never shipped. These pin the three things that replaced it — where the binary probe LOOKS
/// and in what order, that agent traffic is counted whether or not anyone is watching, and that
/// the handshake reads a real tools/list answer (and says so plainly when it cannot).
/// </summary>
public sealed class McpStatusMeasurementTests
{
    private const string Exe = "devcontext-mcp.exe";

    // -----------------------------------------------------------------------
    // Binary probe
    // -----------------------------------------------------------------------

    [Fact]
    public void Probe_prefers_the_binary_beside_the_server()
    {
        // The shipped shell runs the server out of resources/server/, which is where N4.2
        // publishes the MCP exe: a bundled copy must win over whatever else is on the machine.
        var bundled = Path.Combine("C:", "app", "resources", "server", Exe);
        var onPath = Path.Combine("C:", "tools", Exe);

        var probe = McpBinaryLocator.Probe(
            baseDirectory: Path.Combine("C:", "app", "resources", "server"),
            pathEnv: Path.Combine("C:", "tools"),
            fileExists: p => p == bundled || p == onPath,
            executableName: Exe);

        Assert.True(probe.Found);
        Assert.Equal("bundle", probe.Source);
        Assert.Equal(Path.GetFullPath(bundled), probe.Path);
    }

    [Fact]
    public void Probe_falls_back_to_a_PATH_entry()
    {
        var onPath = Path.Combine("C:", "tools", Exe);
        var pathEnv = string.Join(Path.PathSeparator, Path.Combine("C:", "nothing"), Path.Combine("C:", "tools"));

        var probe = McpBinaryLocator.Probe(
            baseDirectory: Path.Combine("C:", "app"),
            pathEnv: pathEnv,
            fileExists: p => p == onPath,
            executableName: Exe);

        Assert.True(probe.Found);
        Assert.Equal("path", probe.Source);
        Assert.Equal(Path.GetFullPath(onPath), probe.Path);
    }

    [Fact]
    public void Probe_finds_this_repos_build_output_from_the_servers_bin_directory()
    {
        // Developers run the server out of src/DevContext.Server/bin/Debug/net10.0; the exe they
        // just built sits in the sibling project's bin. Without this the page would read
        // "not found" on the machine of every person working on it.
        var root = Path.Combine("C:", "repo");
        var solution = Path.Combine(root, "DevContext.slnx");
        var built = Path.Combine(root, "src", "DevContext.Mcp", "bin", "Debug", "net10.0", Exe);

        var probe = McpBinaryLocator.Probe(
            baseDirectory: Path.Combine(root, "src", "DevContext.Server", "bin", "Debug", "net10.0"),
            pathEnv: null,
            fileExists: p => p == solution || p == built,
            executableName: Exe);

        Assert.True(probe.Found);
        Assert.Equal("dev-build", probe.Source);
        Assert.Equal(Path.GetFullPath(built), probe.Path);
    }

    [Fact]
    public void Probe_reports_not_found_rather_than_guessing_a_path()
    {
        var probe = McpBinaryLocator.Probe(
            baseDirectory: Path.Combine("C:", "app"),
            pathEnv: Path.Combine("C:", "tools"),
            fileExists: _ => false,
            executableName: Exe);

        Assert.False(probe.Found);
        Assert.Equal(string.Empty, probe.Path);
        Assert.Equal(string.Empty, probe.Source);
    }

    // -----------------------------------------------------------------------
    // Agent-call measurement
    // -----------------------------------------------------------------------

    [Fact]
    public void Agent_calls_are_counted_with_nobody_watching()
    {
        // The whole point of killing the mute: what the server knows about agent traffic is not
        // a function of whether a page is open. No Subscribe() call in this test.
        var svc = new McpObservabilityService();

        svc.Notify(Event(OriginTag.Agent, "get_context", 1_000));
        svc.Notify(Event(OriginTag.Agent, "trace", 2_000));

        Assert.Equal(0, svc.ObserverCount);
        Assert.Equal(2, svc.AgentCallCount);
        Assert.Equal(2_000, svc.LastAgentCallAtUtcMs);
        Assert.Equal("trace", svc.LastAgentTool);
    }

    [Fact]
    public void The_apps_own_traffic_does_not_count_as_an_agent_call()
    {
        // One page render logged 163 UI-origin calls (T6.10). If those counted, "last agent
        // call" would say "just now" on a machine no agent has ever touched.
        var svc = new McpObservabilityService();

        svc.Notify(Event(OriginTag.Ui, "GetMap", 5_000));

        Assert.Equal(0, svc.AgentCallCount);
        Assert.Equal(0, svc.LastAgentCallAtUtcMs);
        Assert.Equal(string.Empty, svc.LastAgentTool);
    }

    [Fact]
    public void Events_reach_a_subscribed_observer()
    {
        var svc = new McpObservabilityService();
        var channel = Channel.CreateUnbounded<ToolCallEvent>();
        using var subscription = svc.Subscribe("obs-1", channel.Writer);

        svc.Notify(Event(OriginTag.Agent, "find", 7_000));

        Assert.Equal(1, svc.ObserverCount);
        Assert.True(channel.Reader.TryRead(out var received));
        Assert.Equal("find", received!.Tool);
    }

    private static ToolCallEvent Event(string origin, string tool, long timestampUtcMs) => new()
    {
        SessionHandle = "h",
        Tool = tool,
        SessionRepo = "C:/repo",
        Bytes = 400,
        EstTokens = 100,
        ElapsedMs = 5,
        TimestampUtcMs = timestampUtcMs,
        Origin = origin,
    };

    // -----------------------------------------------------------------------
    // Handshake conversation
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handshake_reads_serverInfo_and_the_tool_menu()
    {
        var server = new StringReader(string.Join('\n', [
            """{"jsonrpc":"2.0","id":1,"result":{"protocolVersion":"2024-11-05","serverInfo":{"name":"devcontext","version":"1.0.0"},"capabilities":{}}}""",
            """{"jsonrpc":"2.0","method":"notifications/message","params":{"level":"info"}}""",
            """{"jsonrpc":"2.0","id":2,"result":{"tools":[{"name":"analyze"},{"name":"map"},{"name":"get_context"}]}}""",
        ]));
        var client = new StringWriter();

        var facts = await McpHandshakeProbe.ConverseAsync(client, server, CancellationToken.None);

        Assert.Equal("devcontext", facts.ServerName);
        Assert.Equal("1.0.0", facts.ServerVersion);
        Assert.Equal("2024-11-05", facts.ProtocolVersion);
        Assert.Equal(["analyze", "map", "get_context"], facts.ToolNames);

        // The three messages a host sends, in order — an unnotified server may refuse tools/list.
        var sent = client.ToString();
        Assert.Contains("\"method\":\"initialize\"", sent, StringComparison.Ordinal);
        Assert.Contains("\"method\":\"notifications/initialized\"", sent, StringComparison.Ordinal);
        Assert.Contains("\"method\":\"tools/list\"", sent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handshake_surfaces_a_JSON_RPC_error_instead_of_reporting_green()
    {
        var server = new StringReader(
            """{"jsonrpc":"2.0","id":1,"error":{"code":-32600,"message":"unsupported protocol version"}}""");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => McpHandshakeProbe.ConverseAsync(new StringWriter(), server, CancellationToken.None));

        Assert.Contains("unsupported protocol version", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handshake_says_so_when_the_process_closes_its_output()
    {
        // A binary that starts and dies (no server to proxy, bad runtime) used to be
        // indistinguishable from a healthy one, because nothing ever spoke to it.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => McpHandshakeProbe.ConverseAsync(new StringWriter(), new StringReader(""), CancellationToken.None));

        Assert.Contains("closed its output", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handshake_without_a_binary_fails_with_the_build_instruction()
    {
        var result = await McpHandshakeProbe.RunAsync(
            new McpBinaryProbe(false, string.Empty, string.Empty), TimeSpan.FromSeconds(1), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Empty(result.ToolNames);
        Assert.Contains("dotnet build src/DevContext.Mcp", result.Error, StringComparison.Ordinal);
    }
}
