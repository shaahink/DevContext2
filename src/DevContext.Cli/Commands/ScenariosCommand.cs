namespace DevContext.Cli.Commands;

public sealed class ScenariosCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("Name").Centered())
            .AddColumn("Display Name")
            .AddColumn("When")
            .AddColumn("Description");

        // Only show the two user-facing modes — expert overrides via --scenario
        var display = new (string Name, string When)[]
        {
            ("overview", "No starting point"),
            ("deep-dive", "Requires --focus"),
        };

        foreach (var (name, when) in display)
        {
            if (!ScenarioRegistry.BuiltIn.TryGetValue(name, out var scenario))
                continue;

            table.AddRow(
                $"[bold]{name}[/]",
                scenario.DisplayName,
                when,
                scenario.Description ?? "");
        }

        AnsiConsole.Write(new Panel(table).Header("Available Scenarios").Border(BoxBorder.Heavy));
        return 0;
    }
}
