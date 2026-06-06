using DevContext.Core.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects MVC controller actions and their route templates via syntax tree analysis.</summary>
[ExtractorOrder(25)]
public sealed class ControllerActionExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> HttpVerbs =
        ["HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch"];

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "ControllerActionExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Controllers], ["endpoint-detections"],
        ["model.Detections"],
        "Walks syntax trees to detect MVC controller actions and their route templates");
    /// <summary>Only runs when the Controllers signal has been detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Controllers);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classes)
            {
                ct.ThrowIfCancellationRequested();

                if (!IsController(classDecl)) continue;

                var controllerName = classDecl.Identifier.ValueText;
                var controllerRoute = ExtractControllerRoute(classDecl, controllerName);

                foreach (var member in classDecl.Members)
                {
                    if (member is not MethodDeclarationSyntax method) continue;

                    var httpMethod = ExtractHttpMethod(method);
                    if (httpMethod == null) continue;

                    var actionRoute = ExtractActionRoute(method);
                    var combinedRoute = CombineRoutes(controllerRoute, actionRoute);
                    var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    var authAttrs = ExtractAuthAttributes(method);
                    var paramTypes = method.ParameterList.Parameters
                        .Select(p => p.Type?.ToString() ?? "?")
                        .ToImmutableArray();

                    model.Detections.Add(new EndpointDetection(
                        HttpMethod: httpMethod,
                        RouteTemplate: combinedRoute,
                        HandlerType: controllerName,
                        HandlerMethod: method.Identifier.ValueText,
                        AuthAttributes: authAttrs,
                        ParameterTypes: paramTypes)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }
            }
        }
    }

    private static bool IsController(ClassDeclarationSyntax classDecl)
    {
        if (classDecl.BaseList == null) return false;

        foreach (var baseType in classDecl.BaseList.Types)
        {
            var typeName = baseType.Type.ToString();
            if (typeName is "ControllerBase" or "Controller") return true;
            if (typeName.StartsWith("Controller<", StringComparison.Ordinal)) return true;
        }

        foreach (var attr in classDecl.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            if (attrName == "ApiController" || attrName == "ApiControllerAttribute") return true;
        }

        return false;
    }

    private static string? ExtractHttpMethod(MethodDeclarationSyntax method)
    {
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();

            if (HttpVerbs.Contains(attrName))
            {
                return attrName[4..].ToUpperInvariant();
            }

            var genericIndex = attrName.IndexOf('<');
            if (genericIndex > 0)
            {
                var baseName = attrName[..genericIndex];
                if (HttpVerbs.Contains(baseName))
                    return baseName[4..].ToUpperInvariant();
            }
        }

        return null;
    }

    private static string? ExtractControllerRoute(ClassDeclarationSyntax classDecl,
        string controllerName)
    {
        foreach (var attr in classDecl.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            if (attrName is "Route" or "RouteAttribute")
            {
                return ExtractRouteTemplate(attr);
            }
        }

        // Convention fallback: ControllerName → /ControllerName
        if (controllerName.EndsWith("Controller", StringComparison.Ordinal))
            return "/" + controllerName[..^10];
        return "/" + controllerName;
    }

    private static string? ExtractActionRoute(MethodDeclarationSyntax method)
    {
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            if (attrName is "Route" or "RouteAttribute")
            {
                return ExtractRouteTemplate(attr);
            }
        }

        return null;
    }

    private static string? ExtractRouteTemplate(AttributeSyntax attr)
    {
        if (attr.ArgumentList == null) return null;

        var arg = attr.ArgumentList.Arguments.FirstOrDefault();
        if (arg == null) return null;

        if (arg.Expression is LiteralExpressionSyntax lit)
            return lit.Token.ValueText;

        if (arg.Expression is InvocationExpressionSyntax inv
            && inv.Expression is IdentifierNameSyntax ins
            && ins.Identifier.ValueText == "nameof"
            && inv.ArgumentList.Arguments.Count == 1)
        {
            return inv.ArgumentList.Arguments[0].Expression.ToString();
        }

        return arg.Expression.ToString();
    }

    private static string CombineRoutes(string? controllerRoute, string? actionRoute)
    {
        var controller = controllerRoute?.Trim('/') ?? "";
        var action = actionRoute?.Trim('/') ?? "";

        if (string.IsNullOrEmpty(controller)) return "/" + action;
        if (string.IsNullOrEmpty(action)) return "/" + controller;

        return "/" + controller + "/" + action;
    }

    private static ImmutableArray<string> ExtractAuthAttributes(MethodDeclarationSyntax method)
    {
        var result = new List<string>();

        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            if (attrName.Contains("Authorize"))
                result.Add(attrName);
        }

        return [.. result];
    }
}
