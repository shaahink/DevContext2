namespace DevContext.Core.Tests;

public sealed class CliCommandExtractorTests
{
    [Fact]
    public async Task SpectreCommand_WithSettings_DetectsCliCommand()
    {
        var result = await RunExtractorOnSourceAsync(
            "DeployCommand.cs",
            """
            public sealed class DeployCommand : Command<DeploySettings>
            {
                public override int Execute(CommandContext context, DeploySettings settings) => 0;
            }
            """);

        var detections = result.Detections.OfType<CliCommandDetection>().ToList();
        Assert.Contains(detections, d => d.CommandType == "DeployCommand" && d.SettingsType == "DeploySettings");
    }

    [Fact]
    public async Task SystemCommandLine_RootCommand_WithUsing_DetectsCliCommand()
    {
        // E7 positive: System.CommandLine's bare `Command`/`RootCommand` base is only trustworthy when
        // the file actually imports System.CommandLine (PowerToys.DSC's BaseCommand : Command shape).
        var result = await RunExtractorOnSourceAsync(
            "BaseCommand.cs",
            """
            using System.CommandLine;

            public abstract class BaseCommand : Command
            {
                protected BaseCommand(string name) : base(name) { }
            }
            """);

        var detections = result.Detections.OfType<CliCommandDetection>().ToList();
        Assert.Contains(detections, d => d.CommandType == "BaseCommand");
    }

    [Fact]
    public async Task BareCommandBase_WithoutSystemCommandLineUsing_IsNotDetected()
    {
        // E7 negative: PowerToys' Command Palette extension SDK has its own unrelated `Command` base
        // (Microsoft.CommandPalette.Extensions.Toolkit.Command) for UI action items — a bare-name match
        // with no System.CommandLine import must not be classified as a CLI command.
        var result = await RunExtractorOnSourceAsync(
            "InvokableCommand.cs",
            """
            namespace Microsoft.CommandPalette.Extensions.Toolkit;

            public abstract class InvokableCommand : Command, IInvokableCommand
            {
                public virtual object Invoke() => null;
            }
            """);

        Assert.Empty(result.Detections.OfType<CliCommandDetection>());
    }

    [Fact]
    public async Task WpfICommand_ViewModelHelper_IsNotDetected()
    {
        // E7 negative: WPF's System.Windows.Input.ICommand (Execute/CanExecute) is the MVVM
        // RelayCommand/AsyncCommand/ButtonClickCommand pattern — not a CLI seam. System.CommandLine has
        // no public ICommand interface, so this was pure false-positive noise (PowerToys "CLI (20)").
        var result = await RunExtractorOnSourceAsync(
            "ButtonClickCommand.cs",
            """
            using System;
            using System.Windows.Input;

            public sealed class ButtonClickCommand : ICommand
            {
                public event EventHandler CanExecuteChanged;
                public bool CanExecute(object parameter) => true;
                public void Execute(object parameter) { }
            }
            """);

        Assert.Empty(result.Detections.OfType<CliCommandDetection>());
    }

    [Fact]
    public async Task InterfaceNameContainingICommand_IsNotDetected()
    {
        // E7 negative: the old check matched ANY base-list entry containing the substring "ICommand" —
        // sweeping in unrelated interfaces like IInvokableCommand/ICommandProvider from the Command
        // Palette extension SDK.
        var result = await RunExtractorOnSourceAsync(
            "InvokablePage.cs",
            """
            public abstract class InvokablePage : IInvokableCommand
            {
                public object Invoke() => null;
            }
            """);

        Assert.Empty(result.Detections.OfType<CliCommandDetection>());
    }

    [Fact]
    public async Task CommandAttribute_WithVerb_DetectsCommandAndSettings()
    {
        // Batch B (DC8 probe answer): GitVersion's new CLI declares every command with its OWN
        // [Command("output", ...)] attribute against its OWN ICommand<TSettings>, wired by a source
        // generator. E7 refuses the interface name — correctly — so the verb string is the evidence.
        var result = await RunExtractorOnSourceAsync(
            "OutputCommand.cs",
            """
            namespace GitVersion.Commands;

            [Command("output", "Outputs the version object.")]
            public class OutputCommand(ILogger<OutputCommand> logger) : ICommand<OutputSettings>
            {
                public Task<int> InvokeAsync(OutputSettings settings, CancellationToken ct = default)
                    => Task.FromResult(0);
            }
            """);

        var detection = Assert.Single(result.Detections.OfType<CliCommandDetection>());
        Assert.Equal("OutputCommand", detection.CommandType);
        Assert.Equal("output", detection.CommandName);
        Assert.Equal("OutputSettings", detection.SettingsType);
        Assert.Equal("InvokeAsync", detection.ExecuteMethod);
    }

    [Fact]
    public async Task CommandAttribute_WithoutVerbString_IsNotDetected()
    {
        // The verb string is what makes the attribute unambiguous. A bare [Command] on an
        // ICommand-ish type is exactly the name-only evidence E7 exists to reject.
        var result = await RunExtractorOnSourceAsync(
            "ToolbarAction.cs",
            """
            [Command]
            public sealed class ToolbarAction : ICommand
            {
                public void Execute(object parameter) { }
            }
            """);

        Assert.Empty(result.Detections.OfType<CliCommandDetection>());
    }

    private static async Task<DiscoveryModel> RunExtractorOnSourceAsync(string fileName, string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(fileName, source);

        var cache = new FakeAnalysisCache(fs);
        var allFiles = new List<string>();
        await foreach (var f in fs.EnumerateFilesAsync("", "*", SearchOption.AllDirectories))
            allFiles.Add(f);

        var analysis = new SharedAnalysisContext { AllSourceFiles = allFiles };

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.CliCommands));
        model.Architecture.Seal();

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions { MaxOutputTokens = 8000 },
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = new NullLogger<DiscoveryContext>(),
        };

        var extractor = new CliCommandExtractor();
        await extractor.ExtractAsync(ctx, model, CancellationToken.None);
        return model;
    }
}
