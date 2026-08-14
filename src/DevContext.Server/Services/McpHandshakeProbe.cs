using System.Diagnostics;
using System.Text.Json;

namespace DevContext.Server.Services;

/// <summary>What one real MCP round trip established. <see cref="Error"/> is empty iff Ok.</summary>
public sealed record McpHandshakeResult(
    bool Ok,
    string Command,
    string ServerName,
    string ServerVersion,
    string ProtocolVersion,
    IReadOnlyList<string> ToolNames,
    long ElapsedMs,
    string Error);

/// <summary>The four facts a successful <c>initialize</c> + <c>tools/list</c> pair yields.</summary>
internal sealed record HandshakeFacts(
    string ServerName,
    string ServerVersion,
    string ProtocolVersion,
    IReadOnlyList<string> ToolNames);

/// <summary>
/// N4.1 (STUDIO-MCP audit §4, Room 2) — the handshake test.
///
/// Everything else the MCP page can show is inference: the binary exists, some agent called
/// once, a stream is attached. This spawns the executable a host would spawn and speaks the
/// protocol a host would speak — <c>initialize</c>, <c>notifications/initialized</c>,
/// <c>tools/list</c> — over stdio, and reports what came back. If this is green, a host config
/// pointing at that path works; nothing weaker proves that.
/// </summary>
public static class McpHandshakeProbe
{
    /// <summary>The MCP protocol revision the desktop asks for. Servers may negotiate down.</summary>
    private const string ClientProtocolVersion = "2024-11-05";

    public static async Task<McpHandshakeResult> RunAsync(
        McpBinaryProbe binary,
        TimeSpan timeout,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        if (!binary.Found)
        {
            return Failed(string.Empty, sw,
                $"No {McpBinaryLocator.BaseName} executable found beside the server, on PATH, or in this repo's build output. " +
                $"Build it with: dotnet build src/DevContext.Mcp");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        Process? process = null;
        try
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = binary.Path,
                WorkingDirectory = Path.GetDirectoryName(binary.Path) ?? Environment.CurrentDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null)
                return Failed(binary.Path, sw, $"Could not start {binary.Path}.");

            // The child writes its own log file (Serilog), so stdout carries JSON-RPC only.
            // stderr is drained and dropped anyway: a full pipe would deadlock the child
            // mid-handshake, and the diagnosis a human needs is in that log file.
            _ = process.StandardError.ReadToEndAsync(CancellationToken.None);

            var facts = await ConverseAsync(
                process.StandardInput, process.StandardOutput, timeoutCts.Token).ConfigureAwait(false);

            return new McpHandshakeResult(
                Ok: true,
                Command: binary.Path,
                ServerName: facts.ServerName,
                ServerVersion: facts.ServerVersion,
                ProtocolVersion: facts.ProtocolVersion,
                ToolNames: facts.ToolNames,
                ElapsedMs: sw.ElapsedMilliseconds,
                Error: string.Empty);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            return Failed(binary.Path, sw,
                $"{Path.GetFileName(binary.Path)} did not answer within {timeout.TotalSeconds:0}s. " +
                ExitHint(process));
        }
        catch (Exception ex)
        {
            return Failed(binary.Path, sw, $"{ex.GetType().Name}: {ex.Message} {ExitHint(process)}".TrimEnd());
        }
        finally
        {
            KillQuietly(process);
            process?.Dispose();
        }
    }

    /// <summary>
    /// The conversation itself, over any reader/writer pair — which is what makes it testable
    /// without a process. Throws <see cref="InvalidOperationException"/> with a message written
    /// for the person reading the page, not for a log.
    /// </summary>
    internal static async Task<HandshakeFacts> ConverseAsync(
        TextWriter toServer,
        TextReader fromServer,
        CancellationToken ct)
    {
        // Escaped rather than a raw literal: the request ends in three closing braces, which a
        // raw interpolated string reads as an interpolation delimiter.
        await SendAsync(toServer,
            "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{\"protocolVersion\":\""
            + ClientProtocolVersion
            + "\",\"capabilities\":{},\"clientInfo\":{\"name\":\"devcontext-desktop\",\"version\":\"1.0\"}}}",
            ct).ConfigureAwait(false);

        var initialize = await ReadResultAsync(fromServer, id: 1, step: "initialize", ct).ConfigureAwait(false);
        var protocolVersion = initialize.TryGetProperty("protocolVersion", out var pv) ? pv.GetString() ?? "" : "";
        var serverName = "";
        var serverVersion = "";
        if (initialize.TryGetProperty("serverInfo", out var info))
        {
            serverName = info.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
            serverVersion = info.TryGetProperty("version", out var v) ? v.GetString() ?? "" : "";
        }

        await SendAsync(toServer, """{"jsonrpc":"2.0","method":"notifications/initialized"}""", ct).ConfigureAwait(false);
        await SendAsync(toServer, """{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}""", ct).ConfigureAwait(false);

        var list = await ReadResultAsync(fromServer, id: 2, step: "tools/list", ct).ConfigureAwait(false);
        if (!list.TryGetProperty("tools", out var tools) || tools.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("tools/list answered without a tools array.");

        var names = new List<string>();
        foreach (var tool in tools.EnumerateArray())
        {
            if (tool.TryGetProperty("name", out var name) && name.GetString() is { Length: > 0 } s)
                names.Add(s);
        }

        return new HandshakeFacts(serverName, serverVersion, protocolVersion, names);
    }

    private static async Task SendAsync(TextWriter toServer, string json, CancellationToken ct)
    {
        // One JSON object per line is the stdio transport's framing.
        await toServer.WriteLineAsync(json.AsMemory(), ct).ConfigureAwait(false);
        await toServer.FlushAsync(ct).ConfigureAwait(false);
    }

    /// <summary>Read lines until the response with this id arrives; anything else is skipped.</summary>
    private static async Task<JsonElement> ReadResultAsync(TextReader fromServer, int id, string step, CancellationToken ct)
    {
        while (true)
        {
            var line = await fromServer.ReadLineAsync(ct).ConfigureAwait(false);
            if (line is null)
                throw new InvalidOperationException($"The process closed its output before answering {step}.");
            if (line.Length == 0) continue;

            JsonDocument doc;
            try { doc = JsonDocument.Parse(line); }
            catch (JsonException) { continue; } // not JSON-RPC — ignore rather than fail the probe
            using (doc)
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) continue;
                if (!root.TryGetProperty("id", out var idProp) || !idProp.TryGetInt32(out var got) || got != id) continue;

                if (root.TryGetProperty("error", out var error))
                {
                    var message = error.TryGetProperty("message", out var m) ? m.GetString() : null;
                    throw new InvalidOperationException($"{step} returned an error: {message ?? error.ToString()}");
                }

                if (!root.TryGetProperty("result", out var result))
                    throw new InvalidOperationException($"{step} answered without a result.");

                return result.Clone();
            }
        }
    }

    private static McpHandshakeResult Failed(string command, Stopwatch sw, string error)
        => new(false, command, string.Empty, string.Empty, string.Empty, [], sw.ElapsedMilliseconds, error);

    private static string ExitHint(Process? process)
    {
        try
        {
            if (process is { HasExited: true })
                return $"The process exited with code {process.ExitCode}; see %LOCALAPPDATA%/DevContext/logs/mcp-*.log.";
        }
        catch (InvalidOperationException) { /* never started */ }

        return string.Empty;
    }

    private static void KillQuietly(Process? process)
    {
        try
        {
            if (process is { HasExited: false })
                process.Kill(entireProcessTree: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
        {
            // Already gone, or never started.
        }
    }
}
