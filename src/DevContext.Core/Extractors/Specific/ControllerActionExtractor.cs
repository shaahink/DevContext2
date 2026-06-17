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
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
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
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();
                if (!IsController(classDecl)) continue;

                ProcessControllerActions(classDecl, model, filePath);
            }
        }
    }

    private void ProcessControllerActions(ClassDeclarationSyntax classDecl, DiscoveryModel model, string filePath)
    {
        var controllerName = classDecl.Identifier.ValueText;
        var controllerRoute = ExtractControllerRoute(classDecl, controllerName);
        var hasActionToken = controllerRoute?.Contains("[action]", StringComparison.OrdinalIgnoreCase) == true;

        foreach (var member in classDecl.Members)
        {
            if (member is not MethodDeclarationSyntax method) continue;

            var actionRoutes = ExtractActionRoutes(method);
            var (hasVerb, httpMethod) = ExtractHttpVerb(method);

            if (actionRoutes.Length == 0 && !hasVerb && !hasActionToken)
                continue;

            if (HasNonActionAttribute(method)) continue;
            if (httpMethod == null && actionRoutes.Length == 0)
            {
                if (hasActionToken)
                    httpMethod = InferHttpVerbFromMethodName(method.Identifier.ValueText);
                else
                    continue;
            }

            if (httpMethod == null && actionRoutes.Length > 0)
                httpMethod = InferHttpVerbFromMethodName(method.Identifier.ValueText);

            var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var authAttrs = ExtractAuthAttributes(method);
            var paramTypes = method.ParameterList.Parameters
                .Select(p => p.Type?.ToString() ?? "?")
                .ToImmutableArray();

            if (actionRoutes.Length == 0)
            {
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
            if (string.Equals(attrName, "ApiController"
, StringComparison.Ordinal) || string.Equals(attrName, "ApiControllerAttribute"
, StringComparison.Ordinal) || attrName.EndsWith(".ApiController", StringComparison.Ordinal)
                || attrName.EndsWith(".ApiControllerAttribute", StringComparison.Ordinal)) return true;
        }

        return false;
    }

    /// <summary>Single pass over method attributes to determine whether it has an HTTP verb and extract its HTTP method.</summary>
    private static (bool HasVerb, string? HttpMethod) ExtractHttpVerb(MethodDeclarationSyntax method)
    {
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attrName = attr.Name.ToString();

            foreach (var verb in HttpVerbs)
            {
                var isMatch = string.Equals(attrName, verb, StringComparison.Ordinal) || attrName.EndsWith("." + verb, StringComparison.Ordinal);
                if (isMatch)
                    return (true, verb[4..].ToUpperInvariant());
            }

            var genericIndex = attrName.IndexOf('<');
            if (genericIndex > 0)
            {
                var baseName = attrName[..genericIndex];
                foreach (var verb in HttpVerbs)
                {
                    if (string.Equals(baseName, verb, StringComparison.Ordinal) || baseName.EndsWith("." + verb, StringComparison.Ordinal))
                        return (true, verb[4..].ToUpperInvariant());
                }
            }
        }

        return (false, null);
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


    private static bool IsRouteAttribute(string attrName)
    {
        return string.Equals(attrName, "Route"
, StringComparison.Ordinal) || string.Equals(attrName, "RouteAttribute"
, StringComparison.Ordinal) || attrName.EndsWith(".Route", StringComparison.Ordinal)
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
            && string.Equals(ins.Identifier.ValueText, "nameof"
, StringComparison.Ordinal) && inv.ArgumentList.Arguments.Count == 1)
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
        if (act.StartsWith('/') || act.StartsWith("~/", StringComparison.Ordinal))
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
            if (attrName.Contains("Authorize", StringComparison.Ordinal))
                result.Add(attrName);
        }

        return [.. result];
    }
}
