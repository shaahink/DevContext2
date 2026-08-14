using System.Text.Json;
using System.Text.Json.Nodes;

using DevContext.Protos;
using DevContext.Server.Services;

using Grpc.Core;
using Grpc.Net.Client;

namespace DevContext.Server.Tests;

/// <summary>
/// N4.2 (STUDIO-MCP audit §4, Room 2 "setup that works") — write-config-for-me, asserted ON DISK.
///
/// The failure this replaces was a response that sounded right: the page advertised a config the
/// user had to assemble by hand around a placeholder path. So every assertion here reads the file
/// back and parses it — a result that names <c>.mcp.json</c> while nothing is there, or that
/// silently ate the user's other MCP servers, is the exact class of defect the audit found.
/// </summary>
public sealed class McpConfigWriteTests(ServerTestFactory factory)
    : IClassFixture<ServerTestFactory>, IDisposable
{
    private readonly List<string> _tempRoots = [];

    /// <summary>A probe result standing in for a machine that has the binary.</summary>
    private static readonly McpBinaryProbe FoundBinary =
        new(true, OperatingSystem.IsWindows() ? @"C:\Apps\DevContext\devcontext-mcp.exe" : "/opt/devcontext/devcontext-mcp", "bundle");

    // ---------------------------------------------------------------- the writer, directly

    [Theory]
    [InlineData("claude", ".mcp.json", "mcpServers", false)]
    [InlineData("cursor", ".cursor/mcp.json", "mcpServers", false)]
    [InlineData("vscode", ".vscode/mcp.json", "servers", true)]
    public void Write_CreatesEachHostsProjectConfig_NamingTheResolvedBinary(
        string hostId, string relativePath, string serversKey, bool wantsStdioType)
    {
        var root = NewTempRoot();
        var target = McpConfigWriter.TargetFor(hostId);
        Assert.NotNull(target);

        var result = McpConfigWriter.Write(root, target!, FoundBinary);

        Assert.Equal("created", result.Action);
        Assert.Equal(relativePath, result.RelativePath);
        Assert.True(File.Exists(result.Path), $"the result named {result.Path} — nothing is there");

        var entry = ServerEntry(result.Path, serversKey);
        Assert.Equal(FoundBinary.Path, (string?)entry["command"]);
        Assert.Equal(wantsStdioType ? "stdio" : null, (string?)entry["type"]);
        // The command the response reports IS the command in the file — the page shows one string.
        Assert.Equal(FoundBinary.Path, result.Command);
    }

    [Fact]
    public void Write_MergesIntoAnExistingConfig_KeepingEveryOtherServerAndKey()
    {
        var root = NewTempRoot();
        var path = Path.Combine(root, ".mcp.json");
        File.WriteAllText(path, """
            {
              "mcpServers": {
                "github": { "command": "gh-mcp", "args": ["--stdio"] }
              },
              "somethingElseTheHostReads": 7
            }
            """);

        var result = McpConfigWriter.Write(root, McpConfigWriter.TargetFor("claude")!, FoundBinary);

        Assert.Equal("updated", result.Action);
        var document = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var servers = document["mcpServers"]!.AsObject();
        Assert.Equal("gh-mcp", (string?)servers["github"]!["command"]);
        Assert.Equal("--stdio", (string?)servers["github"]!["args"]!.AsArray()[0]);
        Assert.Equal(FoundBinary.Path, (string?)servers["devcontext"]!["command"]);
        Assert.Equal(7, (int?)document["somethingElseTheHostReads"]);
    }

    [Fact]
    public void Write_ReportsUnchanged_AndLeavesTheUsersFormattingAlone_WhenItAlreadySaysThis()
    {
        var root = NewTempRoot();
        var target = McpConfigWriter.TargetFor("cursor")!;
        var first = McpConfigWriter.Write(root, target, FoundBinary);
        Assert.Equal("created", first.Action);

        // Reformat by hand — same meaning, different bytes. A second write must not touch it.
        var handFormatted = File.ReadAllText(first.Path).Replace("\n", "\r\n", StringComparison.Ordinal) + "\r\n";
        File.WriteAllText(first.Path, handFormatted);

        var second = McpConfigWriter.Write(root, target, FoundBinary);

        Assert.Equal("unchanged", second.Action);
        Assert.Equal(handFormatted, File.ReadAllText(first.Path));
    }

    [Fact]
    public void Write_RefusesToNameABinaryThatIsNotThere()
    {
        var root = NewTempRoot();
        var missing = new McpBinaryProbe(false, string.Empty, string.Empty);

        var ex = Assert.Throws<InvalidOperationException>(
            () => McpConfigWriter.Write(root, McpConfigWriter.TargetFor("claude")!, missing));

        Assert.Contains("devcontext-mcp", ex.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(root, ".mcp.json")), "a refused write must leave no file behind");
    }

    [Fact]
    public void Write_RefusesAConfigItCannotParse_RatherThanOverwritingIt()
    {
        var root = NewTempRoot();
        var path = Path.Combine(root, ".mcp.json");
        const string theirs = "{ this is not json";
        File.WriteAllText(path, theirs);

        Assert.Throws<InvalidOperationException>(
            () => McpConfigWriter.Write(root, McpConfigWriter.TargetFor("claude")!, FoundBinary));

        Assert.Equal(theirs, File.ReadAllText(path));
    }

    [Fact]
    public void Write_RefusesWhenTheServersKeyIsNotAnObject()
    {
        var root = NewTempRoot();
        var path = Path.Combine(root, ".vscode", "mcp.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        const string theirs = """{ "servers": "nope" }""";
        File.WriteAllText(path, theirs);

        Assert.Throws<InvalidOperationException>(
            () => McpConfigWriter.Write(root, McpConfigWriter.TargetFor("vscode")!, FoundBinary));

        Assert.Equal(theirs, File.ReadAllText(path));
    }

    [Fact]
    public void Snippet_IsTheSameJsonAFreshWriteProduces()
    {
        var root = NewTempRoot();
        var target = McpConfigWriter.TargetFor("claude")!;

        var written = McpConfigWriter.Write(root, target, FoundBinary);
        var snippet = McpConfigWriter.SnippetFor(target, FoundBinary);

        // One composer. The string the user copies and the file the button writes are the same
        // bytes (the file gets a trailing newline; nothing else differs).
        Assert.Equal(File.ReadAllText(written.Path).TrimEnd('\n'), snippet);
    }

    [Fact]
    public void SnippetFor_SaysPlaceholderOutLoud_WhenNoBinaryWasFound()
    {
        var snippet = McpConfigWriter.SnippetFor(
            McpConfigWriter.TargetFor("claude")!, new McpBinaryProbe(false, string.Empty, string.Empty));

        // Angle brackets cannot be mistaken for a path anyone has — unlike the old hard-coded
        // C:/path/to/DevContext2/... which read like a real machine's.
        Assert.Contains("<absolute path to devcontext-mcp", snippet, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- over the wire

    [Fact]
    public async Task GetMcpStatus_CarriesAHostCardPerSupportedHost_WithTheProbedPathInTheSnippet()
    {
        var client = CreateClient();
        var status = await client.GetMcpStatusAsync(new GetMcpStatusRequest());

        Assert.Equal(McpConfigWriter.Targets.Count, status.Hosts.Count);
        Assert.Equal(McpConfigWriter.Targets.Select(t => t.Id), status.Hosts.Select(h => h.Id));

        foreach (var host in status.Hosts)
        {
            Assert.False(string.IsNullOrWhiteSpace(host.Label));
            Assert.EndsWith(".json", host.RelativePath, StringComparison.Ordinal);

            // PARSED, not substring-matched: on Windows the command is a backslash path and JSON
            // escapes every separator, so a Contains() against the raw path passes or fails for
            // reasons that have nothing to do with what the snippet means.
            var target = McpConfigWriter.TargetFor(host.Id)!;
            var command = (string?)JsonNode.Parse(host.Snippet)!
                .AsObject()[target.ServersKey]!.AsObject()["devcontext"]!.AsObject()["command"];

            // MEASURED, not assumed: whatever the probe on THIS machine found is what the snippet
            // says. Both branches are real states (CI has the dev build; a bare box has neither).
            if (status.McpBinaryFound) Assert.Equal(status.McpBinaryPath, command);
            else Assert.StartsWith("<absolute path to", command, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task WriteMcpConfig_WritesIntoTheAnalyzedRepo_OrSaysWhyItCannot()
    {
        var client = CreateClient();
        var (handle, root) = await AnalyzeATempCopy(client);
        var found = (await client.GetMcpStatusAsync(new GetMcpStatusRequest())).McpBinaryFound;

        if (!found)
        {
            // No binary on this machine: the RPC must refuse rather than write a config naming a
            // path that does not exist. That refusal is the checkpoint's point, so assert it.
            var refused = await Assert.ThrowsAsync<RpcException>(
                () => client.WriteMcpConfigAsync(new WriteMcpConfigRequest { Handle = handle, Host = "claude" }).ResponseAsync);
            Assert.Contains("devcontext-mcp", refused.Status.Detail, StringComparison.Ordinal);
            return;
        }

        var resp = await client.WriteMcpConfigAsync(new WriteMcpConfigRequest { Handle = handle, Host = "claude" });

        Assert.Equal("created", resp.Action);
        Assert.Equal(".mcp.json", resp.RelativePath);
        Assert.Equal(Path.GetFullPath(Path.Combine(root, ".mcp.json")), Path.GetFullPath(resp.Path));
        Assert.True(File.Exists(resp.Path), $"the response named {resp.Path} — nothing is there");
        Assert.Equal(resp.Command, (string?)ServerEntry(resp.Path, "mcpServers")["command"]);
        Assert.True(File.Exists(resp.Command), "the config must name a binary that is actually there");
    }

    [Fact]
    public async Task WriteMcpConfig_RejectsAnUnknownHost()
    {
        var client = CreateClient();
        var (handle, _) = await AnalyzeATempCopy(client);

        var ex = await Assert.ThrowsAsync<RpcException>(
            () => client.WriteMcpConfigAsync(new WriteMcpConfigRequest { Handle = handle, Host = "emacs" }).ResponseAsync);

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task WriteMcpConfig_RejectsAnUnknownHandle()
    {
        var client = CreateClient();

        var ex = await Assert.ThrowsAsync<RpcException>(
            () => client.WriteMcpConfigAsync(new WriteMcpConfigRequest { Handle = "not-a-session", Host = "claude" }).ResponseAsync);

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    // ---------------------------------------------------------------- plumbing

    private static JsonObject ServerEntry(string path, string serversKey)
    {
        var document = JsonNode.Parse(File.ReadAllText(path), documentOptions: new JsonDocumentOptions())!.AsObject();
        return document[serversKey]!.AsObject()["devcontext"]!.AsObject();
    }

    private DevContextService.DevContextServiceClient CreateClient()
    {
        var http = factory.CreateClient();
        var channel = GrpcChannel.ForAddress(http.BaseAddress!, new GrpcChannelOptions { HttpClient = http });
        return new DevContextService.DevContextServiceClient(channel);
    }

    private string NewTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "devcontext-mcpconfig-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        _tempRoots.Add(root);
        return root;
    }

    private async Task<(string Handle, string Root)> AnalyzeATempCopy(
        DevContextService.DevContextServiceClient client)
    {
        var root = NewTempRoot();
        CopyDirectory(FixturePath("ControllerApp"), root);

        string? handle = null;
        using var call = client.Analyze(new AnalyzeRequest { Path = root, NoRoslyn = true });
        await foreach (var evt in call.ResponseStream.ReadAllAsync())
        {
            if (evt.EventCase == AnalyzeEvent.EventOneofCase.Result) handle = evt.Result.Handle;
        }
        Assert.NotNull(handle);
        return (handle!, root);
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(source, target, StringComparison.OrdinalIgnoreCase));
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(source, target, StringComparison.OrdinalIgnoreCase), overwrite: true);
    }

    private static string FixturePath(string name) => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures", name));

    public void Dispose()
    {
        foreach (var root in _tempRoots)
        {
            try
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
            catch (IOException) { /* temp dir — OS cleans up */ }
            catch (UnauthorizedAccessException) { /* same */ }
        }
    }
}
