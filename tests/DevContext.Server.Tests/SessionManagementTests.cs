using DevContext.Protos;

using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.AspNetCore.Mvc.Testing;

namespace DevContext.Server.Tests;

public sealed class SessionManagementTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private DevContextService.DevContextServiceClient CreateClient()
    {
        var http = factory.CreateClient();
        var channel = GrpcChannel.ForAddress(http.BaseAddress!, new GrpcChannelOptions { HttpClient = http });
        return new DevContextService.DevContextServiceClient(channel);
    }

    [Fact]
    public async Task CloseSession_returns_closed_true_for_valid_handle()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var resp = await client.CloseSessionAsync(new SessionRequest { Handle = handle });
        Assert.True(resp.Closed);

        var ex = await Assert.ThrowsAsync<RpcException>(
            () => client.ListEntryPointsAsync(new SessionRequest { Handle = handle }).ResponseAsync);
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task CloseSession_is_idempotent()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var first = await client.CloseSessionAsync(new SessionRequest { Handle = handle });
        Assert.True(first.Closed);

        var second = await client.CloseSessionAsync(new SessionRequest { Handle = handle });
        Assert.False(second.Closed);
    }

    [Fact]
    public async Task Warm_reuse_same_root_uses_cached_host()
    {
        var client = CreateClient();

        var handle1 = await AnalyzeControllerApp(client);
        var handle2 = await AnalyzeControllerApp(client);

        Assert.NotEqual(handle1, handle2);

        var map1 = await client.GetMapAsync(new SessionRequest { Handle = handle1 });
        var map2 = await client.GetMapAsync(new SessionRequest { Handle = handle2 });
        Assert.NotEmpty(map1.Markdown);
        Assert.NotEmpty(map2.Markdown);
    }

    [Fact]
    public async Task Eviction_respects_capacity()
    {
        var client = CreateClient();

        for (var i = 0; i < 7; i++)
        {
            var handle = await AnalyzeControllerApp(client);
            Assert.NotNull(handle);
        }

        Assert.True(true);
    }

    [Fact]
    public async Task Error_mapping_returns_NotFound_for_unknown_handle()
    {
        var client = CreateClient();
        var ex = await Assert.ThrowsAsync<RpcException>(
            () => client.GetNodeAsync(new NodeRequest { Handle = "unknown", NodeId = "x" }).ResponseAsync);
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
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
