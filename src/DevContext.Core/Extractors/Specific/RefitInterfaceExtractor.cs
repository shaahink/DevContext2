using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects Refit HTTP client interfaces — interfaces whose methods carry
/// <c>[Get]</c>/<c>[Post]</c>/<c>[Put]</c>/<c>[Delete]</c>/<c>[Patch]</c> attributes.
/// Produces <see cref="RefitRouteDetection"/> entries for cross-project YARP/gateway
/// ServiceLink joins (M1.8).</summary>
[ExtractorOrder(60)]
public sealed class RefitInterfaceExtractor : IDiscoveryExtractor
{
    public string Name => "RefitInterfaceExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Refit],
        ["refit-route-detections"],
        ["model.Detections"],
        "Scans syntax trees for Refit HTTP client interfaces ([Get]/[Post]/etc. attributes)");

    private static readonly HashSet<string> HttpMethodAttributes = new(StringComparer.Ordinal)
    {
        "Get", "Post", "Put", "Delete", "Patch", "Head", "Options",
    };

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Refit);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

            SyntaxTree syntaxTree;
            try { syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct); }
            catch { continue; }

            var root = await syntaxTree.GetRootAsync(ct);

            foreach (var ifaceDecl in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();

                var ifaceName = ifaceDecl.Identifier.ValueText;

                foreach (var method in ifaceDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    ct.ThrowIfCancellationRequested();
                    var attrs = method.AttributeLists.SelectMany(a => a.Attributes);
                    (string? httpMethod, string? routeTemplate) = (null, null);

                    foreach (var attr in attrs)
                    {
                        var attrName = attr.Name.ToString();
                        // Match [Get("/path")], [Get(...)], [Refit.Get("/path")]
                        var shortName = attrName.Contains('.')
                            ? attrName[(attrName.LastIndexOf('.') + 1)..]
                            : attrName;

                        if (HttpMethodAttributes.Contains(shortName))
                        {
                            httpMethod = shortName.ToUpperInvariant();
                            if (attr.ArgumentList?.Arguments.Count > 0)
                            {
                                var firstArg = attr.ArgumentList.Arguments[0].ToString().Trim('"');
                                routeTemplate = firstArg;
                            }
                            break;
                        }
                    }

                    if (httpMethod is not null && routeTemplate is not null)
                    {
                        model.Detections.Add(new RefitRouteDetection(
                            ifaceName, method.Identifier.ValueText, httpMethod, routeTemplate)
                        {
                            ExtractorName = Name,
                            SourceFile = filePath,
                            LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            Confidence = 0.8f,
                        });
                    }
                }
            }
        }
    }
}
