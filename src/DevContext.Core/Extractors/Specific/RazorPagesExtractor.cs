using System.Text.RegularExpressions;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects Razor Pages from .cshtml @page directives and PageModel subclasses.
/// Produces <see cref="EndpointDetection"/> entries so the HTTP entry-point builder surfaces them.</summary>
[ExtractorOrder(59)]
public sealed class RazorPagesExtractor : IDiscoveryExtractor
{
    public string Name => "RazorPagesExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.RazorPages],
        ["razor-page-detections"],
        ["model.Detections"],
        "Scans .cshtml files for @page directives and .cs files for PageModel handler methods");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.RazorPages);

    private static readonly Regex PageDirective = new(
        @"@page\s+(?:""([^""]+)""|'([^']+)'|\s*$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private static readonly Regex HandlerMethod = new(
        @"On(Get|Post|Put|Delete|Patch)(?:Async)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        // Pass 1: scan .cshtml files for @page directives (markup lives in AllContentFiles — text-scanned)
        var pageRoutes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in context.Analysis.AllContentFiles)
        {
            ct.ThrowIfCancellationRequested();
            if (!filePath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)) continue;

            var pageName = Path.GetFileNameWithoutExtension(filePath);
            if (pageName.StartsWith('_')) continue;

            string text;
            try { text = await context.Cache.GetTextAsync(filePath, ct); }
            catch { continue; }

            var match = PageDirective.Match(text);
            if (!match.Success) continue;

            var route = match.Groups[1].Success ? match.Groups[1].Value
                : match.Groups[2].Success ? match.Groups[2].Value
                : "/" + pageName;

            if (!route.StartsWith("/")) route = "/" + route;
            pageRoutes[pageName] = route;
        }

        // Pass 2: scan .cs files for PageModel subclasses
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

                var isPageModel = false;
                foreach (var bt in baseList.Types)
                {
                    if (bt.Type.ToString() == "PageModel")
                    {
                        isPageModel = true;
                        break;
                    }
                }
                if (!isPageModel) continue;

                var className = classDecl.Identifier.ValueText;
                var methods = new List<string>();
                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    var methodName = method.Identifier.ValueText;
                    if (HandlerMethod.IsMatch(methodName))
                        methods.Add(methodName);
                }

                var primaryMethod = methods.FirstOrDefault() ?? "OnGet";
                var route = pageRoutes.GetValueOrDefault(className, "/" + className);

                model.Detections.Add(new EndpointDetection(
                    "GET", route, className, primaryMethod,
                    ImmutableArray<string>.Empty, methods.ToImmutableArray())
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.85f,
                });
            }
        }

        // Pass 3: standalone .cshtml files with @page but no PageModel
        foreach (var (pageName, route) in pageRoutes)
        {
            if (model.Detections.OfType<EndpointDetection>()
                .Any(d => d.HandlerType == pageName && d.ExtractorName == Name))
                continue;

            model.Detections.Add(new EndpointDetection(
                "GET", route, pageName, "<page>",
                ImmutableArray<string>.Empty, ImmutableArray<string>.Empty)
            {
                ExtractorName = Name,
                SourceFile = $"{pageName}.cshtml",
                LineNumber = 1,
                Confidence = 0.8f,
            });
        }
    }
}
