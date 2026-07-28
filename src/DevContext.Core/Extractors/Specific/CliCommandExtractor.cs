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
        // B4 (Prism D1.1d): also run for parser-less CLI tools — a repo whose production console exes
        // carry PackAsTool/ToolCommandName evidence (GitVersion) gets Main-method entries below even
        // though no CLI-parser package ever fires the CliCommands signal.
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.CliCommands)
            || currentModel.Projects.Any(Graph.ArchetypeDetector.IsCliToolCandidate);

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
            // Evaluated only when a class actually carries a bare Command/RootCommand base, which is
            // rare: this extractor visits every file in any repo with CLI-tool evidence, and walking
            // a whole syntax tree per file to find using directives is pure cost everywhere else.
            bool? systemCommandLineImport = null;
            bool HasSystemCommandLineUsing() => systemCommandLineImport ??= root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>()
                .Any(u => u.Name?.ToString() is { } n
                    && (n == "System.CommandLine" || n.StartsWith("System.CommandLine.", StringComparison.Ordinal)));

            foreach (var classDecl in root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();

                string? settingsType = null;
                var isCommand = false;

                // Batch B (DC8) — an attribute carrying a verb name, [Command("output", ...)], is
                // strong framework-agnostic evidence: the same shape as endpoint attributes, and a
                // string nobody writes by accident. GitVersion's new CLI declares every command this
                // way against its OWN ICommand<T> + a source generator, so E7's refusal to trust
                // ICommand-ish bases (which stands — see below) left the whole tool invisible.
                var commandVerb = FindCommandAttributeVerb(classDecl);
                if (commandVerb is not null) isCommand = true;

                foreach (var bt in classDecl.BaseList?.Types ?? default)
                {
                    var name = bt.Type.ToString();
                    // A generic ICommand<TSettings> never MAKES a class a command — but once the
                    // attribute above says it is one, its type argument names the settings type.
                    if (isCommand && settingsType is null && name.StartsWith("ICommand<", StringComparison.Ordinal))
                    {
                        settingsType = name["ICommand<".Length..^1];
                        continue;
                    }
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
                    if ((name == "RootCommand" || name == "Command") && HasSystemCommandLineUsing())
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
                    className, settingsType ?? "object", executeMethod, commandVerb)
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.85f,
                });
            }
        }

        await AddMainEntryFallbackAsync(context, model, ct);
    }

    /// <summary>Batch B — the verb from a <c>[Command("output", ...)]</c>-shaped attribute, or null.
    /// The string argument is required: a bare <c>[Command]</c> is exactly the ambiguous, name-only
    /// evidence E7 refuses.</summary>
    private static string? FindCommandAttributeVerb(
        Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl)
    {
        foreach (var attributeList in classDecl.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                var leaf = name[(name.LastIndexOf('.') + 1)..];
                if (leaf is not ("Command" or "CommandAttribute")) continue;
                var firstArgument = attribute.ArgumentList?.Arguments.FirstOrDefault();
                if (firstArgument?.Expression is
                        Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax literal
                    && literal.Token.Value is string verb && verb.Length > 0)
                {
                    return verb;
                }
            }
        }
        return null;
    }

    /// <summary>B4 (Prism D1.1d) — plain <c>Main()</c> becomes an entry. For every production console
    /// exe with CLI-tool evidence (see <see cref="Graph.ArchetypeDetector.IsCliToolCandidate"/>) that
    /// produced NO parser-based command detections, emit one detection for its program entry point
    /// (top-level statements or a static <c>Main</c>). GitVersion read "App with 0 entries" and an
    /// empty map because its CLI detection was package-gated.</summary>
    private async ValueTask AddMainEntryFallbackAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var classifier = new Graph.ProjectClassifier(model.Projects);
        var coveredDirs = model.Detections.OfType<CliCommandDetection>()
            .Select(d => Path.GetDirectoryName(d.SourceFile) ?? "")
            .Where(d => d.Length > 0)
            .ToList();

        foreach (var proj in model.Projects)
        {
            ct.ThrowIfCancellationRequested();
            if (proj.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) != true
                || proj.OutputType.Contains("WinExe", StringComparison.OrdinalIgnoreCase)) continue;
            if (!Graph.ArchetypeDetector.IsCliToolCandidate(proj)) continue;
            if (classifier.IsInTestProject(proj.FilePath)
                || !classifier.IsProduction(proj, model.SamplesAreTheProduct)) continue;

            var projDir = (Path.GetDirectoryName(proj.FilePath) ?? "").Replace('\\', '/');
            if (projDir.Length == 0) continue;
            // Skip projects that already own parser-based command detections.
            if (coveredDirs.Any(d => d.Replace('\\', '/').StartsWith(projDir, StringComparison.OrdinalIgnoreCase)))
                continue;

            // Find the program entry file: prefer Program.cs, else the first file with top-level
            // statements or a static Main.
            var projFiles = context.Analysis.AllSourceFiles
                .Where(f => f.Replace('\\', '/').StartsWith(projDir + "/", StringComparison.OrdinalIgnoreCase)
                    && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => Path.GetFileName(f).Equals("Program.cs", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ToList();

            foreach (var file in projFiles)
            {
                Microsoft.CodeAnalysis.SyntaxTree tree;
                try { tree = await context.Cache.GetSyntaxTreeAsync(file, ct); }
                catch { continue; }
                var root = await tree.GetRootAsync(ct);

                var topLevel = root.DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax>()
                    .FirstOrDefault();
                var mainMethod = root.DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.ValueText == "Main"
                        && m.Modifiers.Any(t => t.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword));
                if (topLevel is null && mainMethod is null) continue;

                var line = (topLevel as Microsoft.CodeAnalysis.SyntaxNode ?? mainMethod)!
                    .GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new CliCommandDetection(proj.Name, "", "Main")
                {
                    ExtractorName = Name,
                    SourceFile = file,
                    LineNumber = line,
                    Confidence = 0.7f,
                });
                break; // one entry per project
            }
        }
    }
}
