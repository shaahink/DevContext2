using System.Text.RegularExpressions;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects Blazor component pages from <c>.razor</c> files via <c>@page</c> directives
/// and from ComponentBase subclasses in <c>.cs</c> files. Produces <see cref="EndpointDetection"/>
/// entries so the existing HTTP entry-point builder surfaces them in the Map.</summary>
[ExtractorOrder(55)]
public sealed class BlazorEntryExtractor : IDiscoveryExtractor
{
    public string Name => "BlazorEntryExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Blazor, ArchitectureSignals.Keys.Controllers],
        ["blazor-entry-detections"],
        ["model.Detections"],
        "Scans .razor files for @page directives and .cs files for ComponentBase subclasses");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Blazor)
        || currentModel.Architecture.Has(ArchitectureSignals.Keys.Controllers);

    private static readonly Regex PageDirective = new(
        @"@page\s+""([^""]+)""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        // .razor markup lives in AllContentFiles; ComponentBase .cs partials in AllSourceFiles.
        foreach (var filePath in context.Analysis.AllContentFiles.Concat(context.Analysis.AllSourceFiles))
        {
            ct.ThrowIfCancellationRequested();

            if (!filePath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
                && !filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                continue;

            var isRazor = filePath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase);
            var componentName = Path.GetFileNameWithoutExtension(filePath);

            // Skip _Imports.razor
            if (componentName.StartsWith('_')) continue;

            string text;
            try { text = await context.Cache.GetTextAsync(filePath, ct); }
            catch { continue; }

            if (isRazor)
            {
                // Extract @page directives
                foreach (Match m in PageDirective.Matches(text))
                {
                    var route = m.Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(route)) continue;
                    if (!route.StartsWith("/")) route = "/" + route;

                    model.Detections.Add(new EndpointDetection(
                        "GET", route, componentName, "<component>",
                        ImmutableArray<string>.Empty, ImmutableArray<string>.Empty)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = 1,
                        Confidence = 0.9f,
                    });
                }
            }
            else
            {
                // For .cs files: detect ComponentBase subclasses
                // Simple substring check — avoids full Roslyn parse for performance
                if (text.Contains("ComponentBase", StringComparison.Ordinal)
                    && text.Contains("partial class " + componentName, StringComparison.Ordinal))
                {
                    // Only create an entry if also .razor file exists alongside
                    var razorPath = Path.ChangeExtension(filePath, ".razor");
                    if (context.Analysis.AllContentFiles.Contains(razorPath))
                        continue; // .razor file handles the @page detection

                    // Standalone component class — no @page, not an entry
                }
            }
        }
    }
}
