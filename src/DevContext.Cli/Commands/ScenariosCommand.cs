namespace DevContext.Cli.Commands;

public sealed class ScenariosCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("Name").Centered())
            .AddColumn("Display Name")
            .AddColumn("Description")
            .AddColumn(new TableColumn("Required Sections").LeftAligned());

        foreach (var (name, scenario) in ScenarioRegistry.BuiltIn)
        {
            table.AddRow(
                $"[bold]{name}[/]",
                scenario.DisplayName,
                scenario.Description ?? "",
                string.Join(", ", scenario.RequiredSections));
        }

        AnsiConsole.Write(new Panel(table).Header("Available Scenarios").Border(BoxBorder.Heavy));
        return 0;
    }
}
