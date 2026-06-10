using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Populates SourceBody for each non-pruned type with its declaration source text.</summary>
[ExtractorOrder(40)]
public sealed class SourceBodyExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "SourceBodyExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Deep;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["model.Types[*].SourceBody"],
        "Populates SourceBody for each non-pruned type with its declaration source text");
    /// <summary>Only runs in Full extraction profile.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.Options.Profile == ExtractionProfile.Full;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var perTypeCap = context.ActiveScenario.Compression.PerTypeCharCap;
        var fileGroups = model.Types.Values
            .Where(t => !t.IsPruned)
            .GroupBy(t => t.FilePath, StringComparer.OrdinalIgnoreCase);

        foreach (var group in fileGroups)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(group.Key, ct);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {group.Key}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var typeDecls = root.DescendantNodes().OfType<TypeDeclarationSyntax>();

            foreach (var typeDecl in typeDecls)
            {
                var ns = typeDecl.Ancestors()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .FirstOrDefault()
                    ?.Name
                    .ToString() ?? "global";
                var fullName = $"{ns}.{typeDecl.Identifier.ValueText}";

                var type = group.FirstOrDefault(t => t.Id == fullName);
                if (type == null) continue;

                var sourceText = typeDecl.ToFullString();
                if (perTypeCap > 0 && sourceText.Length > perTypeCap)
                    sourceText = sourceText[..perTypeCap];

                type.SourceBody = sourceText;
            }
        }
    }
}
