namespace DevContext.Cli.Commands;

public sealed class VersionCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var panel = new Panel(
            Align.Center(new Markup($"[bold yellow]DevContext[/] [white]v{DevContextVersion.Display}[/]")))
            .Border(BoxBorder.Rounded)
            .Padding(2, 1);
        AnsiConsole.Write(panel);
        return 0;
    }
}
