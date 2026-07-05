using DevContext.Cli.Services;
using DevContext.Mcp;
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

    services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(b => b.AddSerilog(dispose: true)));
    services.AddSingleton(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("DevContext.Mcp"));

    services.AddDevContextServices(".");

    services.AddSingleton<McpSessionManager>();

    services.AddSingleton(sp => new DevContextTools(
        sp.GetRequiredService<McpSessionManager>(),
        sp.GetRequiredService<ILoggerFactory>().CreateLogger<DevContextTools>()));

    var toolsInstance = services.BuildServiceProvider().GetRequiredService<DevContextTools>();

    services.AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "devcontext",
            Version = DevContext.Core.DevContextVersion.Display,
        };
    })
    .WithTools(toolsInstance)
    .WithStdioServerTransport();

    var provider = services.BuildServiceProvider();
    var server = provider.GetRequiredService<ModelContextProtocol.Server.McpServer>();
    var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DevContext.Mcp");

    logger.LogInformation("DevContext MCP server starting (stdio)");

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
