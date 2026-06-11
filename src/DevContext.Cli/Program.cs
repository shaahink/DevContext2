using DevContext.Cli.Commands;

var services = new ServiceCollection();
services.AddSingleton<IFileSystem>(_ => new RealFileSystem());
services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(b => b.AddSerilog(dispose: true)));
services.AddSingleton<IRoslynWorkspaceProvider>(_ => new NullRoslynProvider());

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.Settings.ApplicationName = "devcontext";
    config.Settings.ApplicationVersion = "2.0.0";

    config.AddCommand<AnalyzeCommand>("analyze")
        .WithDescription("Analyze a .NET project and produce structured context")
        .WithExample(new[] { "analyze", "." })
        .WithExample(new[] { "analyze", "./src/MyApp.sln", "--scenario", "overview" })
        .WithExample(new[] { "analyze", "--task", "debug why is this endpoint failing" });

    config.AddCommand<InitCommand>("init")
        .WithDescription("Create devcontext.json in the current directory");

    config.AddCommand<ScenariosCommand>("scenarios")
        .WithDescription("List available analysis scenarios");

    config.AddCommand<VersionCommand>("version")
        .WithDescription("Show version information");

    config.PropagateExceptions();
});

return await app.RunAsync(args);
