namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects CLI command handlers — classes extending <c>Command&lt;TSettings&gt;</c>/
/// <c>AsyncCommand&lt;TSettings&gt;</c> (Spectre.Console.Cli), or a bare <c>Command</c>/<c>RootCommand</c>
/// base gated on the file importing System.CommandLine (E7: bare-name bases and any
/// <c>ICommand</c>-ish interface also collide with unrelated frameworks — WPF's
/// <c>System.Windows.Input.ICommand</c> MVVM pattern, the Command Palette extension SDK's own
/// <c>Command</c>/<c>IInvokableCommand</c> — so those are never trusted on name alone).
/// Produces <see cref="CliCommandDetection"/> entries.</summary>
[ExtractorOrder(65)]
public sealed class CliCommandExtractor : IDiscoveryExtractor
{
    public string Name => "CliCommandExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.CliCommands],
        ["cli-command-detections"],
        ["model.Detections"],
        "Scans for Command<TSettings> subclasses and System.CommandLine-gated Command/RootCommand bases");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.CliCommands);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

            Microsoft.CodeAnalysis.SyntaxTree syntaxTree;
            try { syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct); }
            catch { continue; }

            var root = await syntaxTree.GetRootAsync(ct);

            // E7: System.CommandLine's bare `Command`/`RootCommand` base names collide with unrelated
            // frameworks' own types of the same name (e.g. the Command Palette extension SDK's own
            // `Command` base for UI action items) — require the file to actually import System.CommandLine
            // before trusting a bare-name match.
            var hasSystemCommandLineUsing = root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>()
                .Any(u => u.Name?.ToString() is { } n
                    && (n == "System.CommandLine" || n.StartsWith("System.CommandLine.", StringComparison.Ordinal)));

            foreach (var classDecl in root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();
                var baseList = classDecl.BaseList;
                if (baseList is null) continue;

                string? settingsType = null;
                var isCommand = false;

                foreach (var bt in baseList.Types)
                {
                    var name = bt.Type.ToString();
                    if (name.StartsWith("Command<", StringComparison.Ordinal))
                    {
                        isCommand = true;
                        settingsType = name[8..^1];
                        break;
                    }
                    if (name.StartsWith("AsyncCommand<", StringComparison.Ordinal))
                    {
                        isCommand = true;
                        settingsType = name[13..^1];
                        break;
                    }
                    // E7: bare "Command"/"RootCommand" only means System.CommandLine when the file
                    // actually imports it — otherwise it's as likely the Command Palette extension SDK's
                    // own unrelated Command base (Microsoft.CommandPalette.Extensions.Toolkit.Command).
                    if ((name == "RootCommand" || name == "Command") && hasSystemCommandLineUsing)
                    {
                        isCommand = true;
                        break;
                    }
                    // Note: WPF's System.Windows.Input.ICommand (Execute/CanExecute, used pervasively by
                    // MVVM RelayCommand/AsyncCommand/ButtonClickCommand view-model helpers) is NOT a CLI
                    // marker — System.CommandLine has no public ICommand interface to detect from, so a
                    // bare "ICommand" match here was pure false-positive noise (E7), not a real seam.
                }

                if (!isCommand) continue;

                var className = classDecl.Identifier.ValueText;

                var executeMethod = "Execute";
                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    if (method.Identifier.ValueText is "Execute" or "ExecuteAsync" or "Invoke" or "InvokeAsync")
                    {
                        executeMethod = method.Identifier.ValueText;
                        break;
                    }
                }

                model.Detections.Add(new CliCommandDetection(
                    className, settingsType ?? "object", executeMethod)
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.85f,
                });
            }
        }
    }
}
