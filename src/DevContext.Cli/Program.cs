using DevContext.Cli;
using DevContext.Cli.Commands;

var services = new ServiceCollection();
services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(b => b.AddSerilog(dispose: true)));
services.AddDevContextServices(".");

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.Settings.ApplicationName = "devcontext";
    config.Settings.ApplicationVersion = DevContextVersion.Display;

    config.AddCommand<AnalyzeCommand>("analyze")
        .WithDescription("Analyze a .NET project and produce structured context")
        .WithExample(new[] { "analyze", "." })
        .WithExample(new[] { "analyze", "./src/MyApp.sln", "--focus", "POST /api/orders" });

    config.AddCommand<QueryCommand>("query")
        .WithDescription("Query the analysis graph (JSON-first)")
        .WithExample(new[] { "query", "entrypoints", "--path", "." })
        .WithExample(new[] { "query", "trace", "--focus", "POST /api/orders" });

    config.AddCommand<InitCommand>("init")
        .WithDescription("Create devcontext.json in the current directory");

    config.AddCommand<ScenariosCommand>("scenarios")
        .WithDescription("(removed — scenario is derived from focus)");

    config.AddCommand<VersionCommand>("version")
        .WithDescription("Show version information");

    config.PropagateExceptions();
});

return await app.RunAsync(args);
