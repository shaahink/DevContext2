namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects CLI command handlers — classes extending <c>Command&lt;TSettings&gt;</c>
/// (Spectre.Console.Cli) or implementing <c>ICommand</c> (System.CommandLine).
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
        "Scans for Command<TSettings> subclasses and ICommand implementations");

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
                    if (name == "RootCommand" || name == "Command")
                    {
                        isCommand = true;
                        break;
                    }
                    // Check interfaces
                    if (name == "ICommand" || name.Contains("ICommand"))
                    {
                        isCommand = true;
                        break;
                    }
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
