using DevContext.Protos;
using DevContext.Server.Sessions;

using Grpc.Core;
using Grpc.Net.Client;

namespace DevContext.Server.Tests;

/// <summary>
/// N3.2 (STUDIO-MCP §8.3, decision 3) — the repo-file hand-off, asserted on DISK. Save's whole
/// point is that the pack ends up somewhere an agent can be pointed at, so the only test that
/// proves it is one that reads the file back out of the repo afterwards: a response that says
/// ".devcontext/packs/x.md" while nothing was written is exactly the fabricated-confidence
/// failure the audit found elsewhere.
///
/// The analyze runs against a COPY of the fixture in a temp directory, because a passing test
/// must not leave a .devcontext/ inside tests/fixtures/.
/// </summary>
public sealed class PackFileHandoffTests(ServerTestFactory factory)
    : IClassFixture<ServerTestFactory>, IDisposable
{
    private readonly List<string> _tempRoots = [];

    private DevContextService.DevContextServiceClient CreateClient()
    {
        var http = factory.CreateClient();
        var channel = GrpcChannel.ForAddress(http.BaseAddress!, new GrpcChannelOptions { HttpClient = http });
        return new DevContextService.DevContextServiceClient(channel);
    }

    [Fact]
    public async Task SavePackFile_WritesThePackAndAGitignore_IntoTheAnalyzedRepo()
    {
        var client = CreateClient();
        var (handle, root) = await AnalyzeATempCopy(client);
        const string content = "# Checkout pack\n\nOrderController.Post — src/Api/OrderController.cs:42\n";

        var resp = await client.SavePackFileAsync(new SavePackFileRequest
        {
            Handle = handle,
            Slug = "Checkout flow",
            Content = content,
            Format = "markdown",
        });

        Assert.Equal(".devcontext/packs/checkout-flow.md", resp.RelativePath);
        Assert.True(File.Exists(resp.Path), $"the response named {resp.Path} — nothing is there");
        // Byte-for-byte: Save must serve what the composer was looking at, not a re-render.
        Assert.Equal(content, File.ReadAllText(resp.Path));
        Assert.Equal(Path.GetFullPath(Path.Combine(root, ".devcontext", "packs", "checkout-flow.md")),
            Path.GetFullPath(resp.Path));

        var gitignore = Path.Combine(root, ".devcontext", ".gitignore");
        Assert.True(File.Exists(gitignore), "decision 3 says gitignored BY DEFAULT");
        Assert.Contains("packs/", File.ReadAllText(gitignore), StringComparison.Ordinal);
        Assert.True(resp.Gitignored);

        // The hand-off line names the path that was written, so pasting it into CLAUDE.md is true.
        Assert.Contains(resp.RelativePath, resp.AgentLine, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SavePackFile_NeverEditsAGitignoreTheRepoAlreadyHas_AndSaysWhenItStoppedCovering()
    {
        var client = CreateClient();
        var (handle, root) = await AnalyzeATempCopy(client);

        var dir = Directory.CreateDirectory(Path.Combine(root, ".devcontext"));
        var gitignore = Path.Combine(dir.FullName, ".gitignore");
        const string mine = "# mine\nnotes.md\n";
        File.WriteAllText(gitignore, mine);

        var resp = await client.SavePackFileAsync(new SavePackFileRequest
        {
            Handle = handle, Slug = "pack", Content = "x", Format = "markdown",
        });

        Assert.Equal(mine, File.ReadAllText(gitignore));
        Assert.False(resp.Gitignored, "packs/ is not covered — the UI must not promise it is");
        Assert.True(File.Exists(resp.Path));
    }

    [Fact]
    public async Task SavePackFile_CannotBeTalkedIntoWritingOutsideThePacksDirectory()
    {
        var client = CreateClient();
        var (handle, root) = await AnalyzeATempCopy(client);

        var resp = await client.SavePackFileAsync(new SavePackFileRequest
        {
            Handle = handle,
            Slug = "../../../evil",
            Content = "no",
            Format = "markdown",
        });

        var packs = Path.GetFullPath(Path.Combine(root, ".devcontext", "packs"));
        Assert.StartsWith(packs, Path.GetFullPath(resp.Path), StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.GetFullPath(Path.Combine(root, "..", "..", "..", "evil.md"))));
    }

    [Fact]
    public async Task SavePackFile_FormatPicksTheExtension()
    {
        var client = CreateClient();
        var (handle, _) = await AnalyzeATempCopy(client);

        var json = await client.SavePackFileAsync(new SavePackFileRequest
        {
            Handle = handle, Slug = "pack", Content = "{}", Format = "json",
        });
        var plain = await client.SavePackFileAsync(new SavePackFileRequest
        {
            Handle = handle, Slug = "pack", Content = "x", Format = "plain",
        });

        Assert.EndsWith(".json", json.RelativePath, StringComparison.Ordinal);
        Assert.EndsWith(".txt", plain.RelativePath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SavePackFile_RejectsAnUnknownHandle()
    {
        var client = CreateClient();
        var ex = await Assert.ThrowsAsync<RpcException>(() => client.SavePackFileAsync(new SavePackFileRequest
        {
            Handle = "not-a-session", Slug = "pack", Content = "x", Format = "markdown",
        }).ResponseAsync);

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Theory]
    [InlineData("Checkout flow", "checkout-flow")]
    [InlineData("../../etc/passwd", "etc-passwd")]
    [InlineData("  ", "context-pack")]
    [InlineData("!!!", "context-pack")]
    [InlineData("Order.Service_v2", "order-service-v2")]
    public void Slugify_CollapsesEverythingThatCouldAddressAPath(string input, string expected)
        => Assert.Equal(expected, PackFileWriter.Slugify(input));

    private async Task<(string Handle, string Root)> AnalyzeATempCopy(
        DevContextService.DevContextServiceClient client)
    {
        var root = Path.Combine(Path.GetTempPath(), "devcontext-packfile-" + Guid.NewGuid().ToString("N"));
        _tempRoots.Add(root);
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
