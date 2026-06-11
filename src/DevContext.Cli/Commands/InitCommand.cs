namespace DevContext.Cli.Commands;

public sealed class InitCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var path = DevContextConfig.DefaultPath;
        if (File.Exists(path))
        {
            AnsiConsole.MarkupLine("[yellow]devcontext.json already exists[/]");
            return 0;
        }

        var config = new
        {
            defaultProfile = "focused",
            defaultScenario = "overview",
            maxOutputTokens = 6000,
            excludePatterns = new[] { ".git", "bin", "obj", "Migrations" },
            entryPaths = new[] { "src/Api" }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
        AnsiConsole.MarkupLine($"[green]Created {path}[/]");
        return 0;
    }
}
