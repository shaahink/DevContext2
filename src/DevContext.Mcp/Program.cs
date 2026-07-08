using DevContext.Mcp;
using DevContext.Protos;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext", "logs", "mcp-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var services = new ServiceCollection();
    services.AddLogging(b => b.AddSerilog(dispose: true));

    // M3.1 — connect to the DevContext server; spawn it if not running
    var serverEndpoint = "http://127.0.0.1:5179";
    var serverProcess = ServerShim.EnsureServerRunning(serverEndpoint);

    // gRPC-Web uses HTTP/1.1 transport — force version 1.1
    var httpInner = new SocketsHttpHandler { EnableMultipleHttp2Connections = false };
    var httpVersionHandler = new ForceHttp11Handler(httpInner);
    var channel = GrpcChannel.ForAddress(serverEndpoint, new GrpcChannelOptions
    {
        HttpHandler = new GrpcWebHandler(httpVersionHandler),
        ThrowOperationCanceledOnCancellation = true,
    });

    var client = new DevContextService.DevContextServiceClient(channel);
    services.AddSingleton(client);

    // Verify connectivity
    try
    {
        var ping = await client.PingAsync(new PingRequest());
        Log.Information("Connected to DevContext server v{Version}", ping.Version);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Cannot reach DevContext server at {Endpoint}. Is the server running?", serverEndpoint);
        return 1;
    }

    services.AddSingleton(sp => new DevContextTools(
        sp.GetRequiredService<DevContextService.DevContextServiceClient>(),
        sp.GetRequiredService<ILoggerFactory>().CreateLogger<DevContextTools>()));

    var toolsInstance = services.BuildServiceProvider().GetRequiredService<DevContextTools>();

    services.AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "devcontext",
            Version = "1.0.0", // M3.1 �?" version comes from server via Ping, static here
        };
    })
    .WithTools(toolsInstance)
    .WithCallToolHandler(UnknownToolHandler.Handle)
    .WithStdioServerTransport();

    var provider = services.BuildServiceProvider();
    var server = provider.GetRequiredService<ModelContextProtocol.Server.McpServer>();
    var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DevContext.Mcp");

    logger.LogInformation("DevContext MCP server starting (stdio → gRPC proxy to {Endpoint})", serverEndpoint);

    // M3.1 — if we spawned the server, clean it up on exit
    AppDomain.CurrentDomain.ProcessExit += (_, _) =>
    {
        serverProcess?.Kill(entireProcessTree: true);
        serverProcess?.Dispose();
    };

    await server.RunAsync();

    logger.LogInformation("DevContext MCP server stopped");
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "MCP server terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
