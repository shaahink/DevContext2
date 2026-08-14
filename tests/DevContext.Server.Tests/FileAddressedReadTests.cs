using DevContext.Protos;

using Grpc.Core;
using Grpc.Net.Client;

namespace DevContext.Server.Tests;

/// <summary>
/// M1.1 items 2+3 — the two FILE-ADDRESSED reads, end to end over the real service on a real
/// snapshot. Every other browse RPC is node-first; these two take a PATH, which is the whole
/// reason they need a test at this altitude: a path parameter is the one place a query API can
/// quietly become "read any file on this machine", and a cap the caller cannot see is a cap that
/// lies. Both are asserted here, not read off a comment.
/// </summary>
public sealed class FileAddressedReadTests(ServerTestFactory factory)
    : IClassFixture<ServerTestFactory>
{
    private DevContextService.DevContextServiceClient CreateClient()
    {
        var http = factory.CreateClient();
        var channel = GrpcChannel.ForAddress(http.BaseAddress!, new GrpcChannelOptions { HttpClient = http });
        return new DevContextService.DevContextServiceClient(channel);
    }

    [Fact]
    public async Task FileMode_ReturnsTheWholeFile_AndSaysSo()
    {
        var client = CreateClient();
        var (handle, path) = await AFileInTheSnapshot(client);

        var resp = await client.ReadSourceAsync(new ReadSourceRequest
        {
            SessionId = handle,
            Mode = ReadSourceMode.File,
            FilePath = path,
        });

        Assert.True(resp.TotalLines > 0);
        Assert.Equal(1, resp.StartLine);
        Assert.Equal(resp.TotalLines, resp.EndLine);
        Assert.False(resp.Truncated);
        // The content really is the file, not a window that happens to report the file's length.
        Assert.Equal(resp.TotalLines, resp.Content.Split('\n').Length);
    }

    [Fact]
    public async Task FileMode_CapFires_AndTheResponseAdmitsIt()
    {
        var client = CreateClient();
        var (handle, path) = await AFileInTheSnapshot(client);

        var resp = await client.ReadSourceAsync(new ReadSourceRequest
        {
            SessionId = handle,
            Mode = ReadSourceMode.File,
            FilePath = path,
            MaxLines = 2,
        });

        Assert.Equal(2, resp.EndLine);
        Assert.Equal(2, resp.Content.Split('\n').Length);
        Assert.True(resp.Truncated);
        Assert.True(resp.TotalLines > resp.EndLine, "the cap must report the size it cut from");
    }

    [Fact]
    public async Task FileMode_RefusesAPathOutsideTheAnalyzedRoot()
    {
        var client = CreateClient();
        var (handle, _) = await AFileInTheSnapshot(client);

        // A real, definitely-readable file that is not in the snapshot's root.
        var outside = typeof(FileAddressedReadTests).Assembly.Location;

        var resp = await client.ReadSourceAsync(new ReadSourceRequest
        {
            SessionId = handle,
            Mode = ReadSourceMode.File,
            FilePath = outside,
        });

        Assert.Contains("Refused", resp.Content, StringComparison.Ordinal);
        Assert.Equal(0, resp.TotalLines);
    }

    [Fact]
    public async Task FileMode_AcceptsARepoRelativePath()
    {
        var client = CreateClient();
        var (handle, absolute) = await AFileInTheSnapshot(client);
        var root = Path.GetDirectoryName(absolute)!;
        var relative = Path.GetFileName(absolute);

        var viaAbsolute = await client.ReadSourceAsync(new ReadSourceRequest
        {
            SessionId = handle, Mode = ReadSourceMode.File, FilePath = absolute,
        });
        var viaRelative = await client.ReadSourceAsync(new ReadSourceRequest
        {
            SessionId = handle,
            Mode = ReadSourceMode.File,
            FilePath = Path.GetRelativePath(FixturePath("ControllerApp"), Path.Combine(root, relative)),
        });

        Assert.Equal(viaAbsolute.TotalLines, viaRelative.TotalLines);
        Assert.Equal(viaAbsolute.Content, viaRelative.Content);
    }

    [Fact]
    public async Task FileOverlay_ReturnsTheWiringSitesInThatFile_OrderedByLine()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        // The overlay is only interesting on a file the graph has edges FOR, so find one rather
        // than hard-coding a fixture file name that an extractor change could invalidate.
        FileOverlayResponse? found = null;
        foreach (var path in await FilesInSnapshot(client, handle))
        {
            var resp = await client.GetFileOverlayAsync(new FileOverlayRequest { Handle = handle, FilePath = path });
            if (resp.Sites.Count > 0) { found = resp; break; }
        }

        Assert.NotNull(found);
        Assert.Equal(found!.Sites.Count, found.PlacedSites + found.UnplacedSites);
        Assert.All(found.Sites, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Kind));
            Assert.False(string.IsNullOrWhiteSpace(s.ToTitle));
            Assert.False(string.IsNullOrWhiteSpace(s.ToNodeId));
        });
        var lines = found.Sites.Select(s => s.Line).ToArray();
        Assert.Equal(lines.OrderBy(l => l).ToArray(), lines);
    }

    [Fact]
    public async Task FileOverlay_RefusesAPathOutsideTheAnalyzedRoot()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var resp = await client.GetFileOverlayAsync(new FileOverlayRequest
        {
            Handle = handle,
            FilePath = typeof(FileAddressedReadTests).Assembly.Location,
        });

        Assert.Empty(resp.Sites);
        Assert.Equal(0, resp.PlacedSites);
        Assert.Equal(0, resp.UnplacedSites);
    }

    /// <summary>A node-bearing source file from the analyzed snapshot, with its handle.</summary>
    private static async Task<(string Handle, string Path)> AFileInTheSnapshot(
        DevContextService.DevContextServiceClient client)
    {
        var handle = await AnalyzeControllerApp(client);
        var files = await FilesInSnapshot(client, handle);
        Assert.NotEmpty(files);
        return (handle, files[0]);
    }

    private static async Task<IReadOnlyList<string>> FilesInSnapshot(
        DevContextService.DevContextServiceClient client, string handle)
    {
        var nodes = await client.SearchNodesAsync(new SearchRequest { Handle = handle, Query = "a", Limit = 200 });
        var paths = new List<string>();
        foreach (var n in nodes.Nodes)
        {
            var detail = await client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = n.NodeId });
            if (detail.HasFilePath && !paths.Contains(detail.FilePath, StringComparer.OrdinalIgnoreCase))
                paths.Add(detail.FilePath);
        }
        return paths;
    }

    private static async Task<string> AnalyzeControllerApp(DevContextService.DevContextServiceClient client)
    {
        var fixture = FixturePath("ControllerApp");
        string? handle = null;
        using var call = client.Analyze(new AnalyzeRequest { Path = fixture, NoRoslyn = true });
        await foreach (var evt in call.ResponseStream.ReadAllAsync())
        {
            if (evt.EventCase == AnalyzeEvent.EventOneofCase.Result)
                handle = evt.Result.Handle;
        }
        Assert.NotNull(handle);
        return handle!;
    }

    private static string FixturePath(string name) => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures", name));
}
