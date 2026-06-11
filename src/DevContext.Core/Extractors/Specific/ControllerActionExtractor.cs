using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects MVC controller actions and their route templates via syntax tree analysis.</summary>
[ExtractorOrder(25)]
public sealed class ControllerActionExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> HttpVerbs =
        ["HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch"];

    private static readonly ImmutableArray<string> VerbNamePrefixes =
        ["Get", "Post", "Put", "Delete", "Patch"];

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
                var hasActionToken = controllerRoute?.Contains("[action]", StringComparison.OrdinalIgnoreCase) == true;

                foreach (var member in classDecl.Members)
                {
                    if (member is not MethodDeclarationSyntax method) continue;

                    var actionRoutes = ExtractActionRoutes(method);
                    var hasVerb = HasHttpVerbAttribute(method);

                    // Skip methods that have neither [Route] nor HTTP verb attributes,
                    // unless the controller uses [action] token convention routing
                    if (actionRoutes.Length == 0 && !hasVerb && !hasActionToken)
                        continue;

                    // Skip methods that are [NonAction] decorated
                    if (HasNonActionAttribute(method)) continue;

                    var httpMethod = ExtractHttpMethod(method);
                    if (httpMethod == null && actionRoutes.Length == 0)
                    {
                        // Convention-routed actions rely on method name for HTTP verb inference
                        if (hasActionToken)
                            httpMethod = InferHttpVerbFromMethodName(method.Identifier.ValueText);
                        else
                            continue;
                    }

                    // Infer verb from method name when only [Route] is present
                    if (httpMethod == null && actionRoutes.Length > 0)
                        httpMethod = InferHttpVerbFromMethodName(method.Identifier.ValueText);

                    var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    var authAttrs = ExtractAuthAttributes(method);
                    var paramTypes = method.ParameterList.Parameters
                        .Select(p => p.Type?.ToString() ?? "?")
                        .ToImmutableArray();

                    if (actionRoutes.Length == 0)
                    {
                        // Action has [HttpGet] etc. but no [Route] — combine with controller route
                        var combinedRoute = CombineRoutes(controllerName, method.Identifier.ValueText, controllerRoute, null);
                        model.Detections.Add(new EndpointDetection(
                            HttpMethod: httpMethod!,
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
                    else
                    {
                        foreach (var actionRoute in actionRoutes)
                        {
                            var combinedRoute = CombineRoutes(controllerName, method.Identifier.ValueText, controllerRoute, actionRoute);
                            model.Detections.Add(new EndpointDetection(
                                HttpMethod: httpMethod!,
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
            if (attrName == "ApiController"
                || attrName == "ApiControllerAttribute"
                || attrName.EndsWith(".ApiController", StringComparison.Ordinal)
                || attrName.EndsWith(".ApiControllerAttribute", StringComparison.Ordinal)) return true;
        }

        return false;
    }

    private static bool HasHttpVerbAttribute(MethodDeclarationSyntax method)
    {
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            var name = attrName.Contains('<') ? attrName[..attrName.IndexOf('<')] : attrName;
            foreach (var verb in HttpVerbs)
                if (name == verb || name.EndsWith("." + verb, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static bool HasNonActionAttribute(MethodDeclarationSyntax method)
    {
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            if (attrName is "NonAction" or "NonActionAttribute") return true;
        }
        return false;
    }

    private static string InferHttpVerbFromMethodName(string methodName)
    {
        foreach (var prefix in VerbNamePrefixes)
        {
            if (methodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return prefix.ToUpperInvariant();
        }
        return "GET"; // ASP.NET default for [ApiController] actions
    }

    private static string? ExtractHttpMethod(MethodDeclarationSyntax method)
    {
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();

            foreach (var verb in HttpVerbs)
            {
                if (attrName == verb || attrName.EndsWith("." + verb, StringComparison.Ordinal))
                {
                    return verb[4..].ToUpperInvariant();
                }
            }

            var genericIndex = attrName.IndexOf('<');
            if (genericIndex > 0)
            {
                var baseName = attrName[..genericIndex];
                foreach (var verb in HttpVerbs)
                {
                    if (baseName == verb || baseName.EndsWith("." + verb, StringComparison.Ordinal))
                        return verb[4..].ToUpperInvariant();
                }
            }
        }

        return null;
    }

    private static bool IsRouteAttribute(string attrName)
    {
        return attrName == "Route"
            || attrName == "RouteAttribute"
            || attrName.EndsWith(".Route", StringComparison.Ordinal)
            || attrName.EndsWith(".RouteAttribute", StringComparison.Ordinal);
    }

    private static string? ExtractControllerRoute(ClassDeclarationSyntax classDecl,
        string controllerName)
    {
        foreach (var attr in classDecl.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            if (IsRouteAttribute(attrName))
            {
                var template = ExtractRouteTemplate(attr);
                return template ?? GetConventionControllerRoute(controllerName);
            }
        }

        return GetConventionControllerRoute(controllerName);
    }

    private static string GetConventionControllerRoute(string controllerName)
    {
        if (controllerName.EndsWith("Controller", StringComparison.Ordinal))
            return controllerName[..^10];
        return controllerName;
    }

    private static ImmutableArray<string> ExtractActionRoutes(MethodDeclarationSyntax method)
    {
        var routes = new List<string>();
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();
            if (IsRouteAttribute(attrName))
            {
                var template = ExtractRouteTemplate(attr);
                if (template is not null)
                    routes.Add(template);
            }
        }
        return routes.ToImmutableArray();
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

    private static string CombineRoutes(string controllerName, string actionName,
        string? controllerRoute, string? actionRoute)
    {
        // Expand tokens in controller and action routes
        var ctrl = ExpandRouteTokens(controllerRoute ?? "[controller]", controllerName, actionName);
        var act = ExpandRouteTokens(actionRoute ?? "", controllerName, actionName);

        // Absolute action route overrides controller route entirely
        if (act.StartsWith('/') || act.StartsWith("~/"))
            return act.TrimStart('~');

        ctrl = ctrl.Trim('/');
        act = act.Trim('/');

        if (string.IsNullOrEmpty(ctrl)) return "/" + act;
        if (string.IsNullOrEmpty(act)) return "/" + ctrl;

        return "/" + ctrl + "/" + act;
    }

    private static string ExpandRouteTokens(string template, string controllerName, string actionName)
    {
        var shortName = controllerName.EndsWith("Controller", StringComparison.Ordinal)
            ? controllerName[..^10] : controllerName;
        return template
            .Replace("[controller]", shortName, StringComparison.OrdinalIgnoreCase)
            .Replace("[action]", actionName, StringComparison.OrdinalIgnoreCase);
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
