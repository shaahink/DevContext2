using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors;

/// <summary>Helper for detecting FastEndpoints-style endpoints via syntax analysis (Phases 3 and 4).</summary>
public static class FastEndpointsHelper
{
    public static void DetectClassAttributeEndpoints(
        IEnumerable<ClassDeclarationSyntax> fastEndpointClasses,
        string filePath,
        HashSet<string> detectedKeys,
        DiscoveryModel model)
    {
        foreach (var cls in fastEndpointClasses)
        {
            var httpAttr = cls.AttributeLists
                .SelectMany(a => a.Attributes)
                .FirstOrDefault(a => HttpConstants.HttpVerbAttributes.Contains(a.Name.ToString()));

            if (httpAttr == null) continue;

            var httpMethod = httpAttr.Name.ToString() switch
            {
                "HttpGet" => "GET",
                "HttpPost" => "POST",
                "HttpPut" => "PUT",
                "HttpDelete" => "DELETE",
                "HttpPatch" => "PATCH",
                _ => "UNKNOWN"
            };

            var routeArg = httpAttr.ArgumentList?.Arguments
                .FirstOrDefault()
                ?.Expression as LiteralExpressionSyntax;
            var route = routeArg?.Token.ValueText ?? "/";

            var line = cls.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            if (!detectedKeys.Add($"{filePath}:{line}")) continue;

            model.Detections.Add(new EndpointDetection(httpMethod, route, cls.Identifier.ValueText, cls.Identifier.ValueText, [], [])
            {
                ExtractorName = "EndpointExtractor",
                SourceFile = filePath,
                LineNumber = line,
            });
        }
    }

    /// <summary>Builds a project-wide <c>"TypeName.Field" → literal value</c> index of string-constant
    /// fields, so a FastEndpoints route referenced as <c>SomeRequest.Route</c> resolves even when the
    /// request type lives in a different file from the endpoint (G2). Syntax trees are cached, so this
    /// extra pass over the corpus is cheap.</summary>
    public static async Task<Dictionary<string, string>> BuildRouteConstIndex(
        DiscoveryContext context, CancellationToken ct)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            SyntaxTree tree;
            try { tree = await context.Cache.GetSyntaxTreeAsync(filePath, ct); }
            catch { continue; }
            var root = await tree.GetRootAsync(ct).ConfigureAwait(false);
            foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                foreach (var field in type.Members.OfType<FieldDeclarationSyntax>())
                {
                    foreach (var v in field.Declaration.Variables)
                    {
                        if (v.Initializer?.Value is LiteralExpressionSyntax { Token.Value: string s })
                            map[$"{type.Identifier.ValueText}.{v.Identifier.ValueText}"] = s;
                    }
                }
            }
        }
        return map;
    }

    public static void DetectConfigureMethodEndpoints(
        IEnumerable<ClassDeclarationSyntax> fastEndpointClasses,
        string filePath,
        HashSet<string> detectedKeys,
        DiscoveryModel model,
        IReadOnlyDictionary<string, string>? routeConsts = null)
    {
        foreach (var cls in fastEndpointClasses)
        {
            var configure = cls.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == "Configure");
            if (configure is null) continue;

            foreach (var invocation in configure.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var methodName = (invocation.Expression as IdentifierNameSyntax)?.Identifier.Text
                              ?? (invocation.Expression as MemberAccessExpressionSyntax)?.Name.Identifier.Text;

                if (methodName is not ("Get" or "Post" or "Put" or "Delete" or "Patch" or "Options"))
                    continue;

                var httpMethod = methodName switch
                {
                    "Get" => "GET",
                    "Post" => "POST",
                    "Put" => "PUT",
                    "Delete" => "DELETE",
                    "Patch" => "PATCH",
                    "Options" => "OPTIONS",
                    _ => "UNKNOWN",
                };

                // Route is often a const/static string (FastEndpoints idiom:
                // Post(CreateContributorRequest.Route) where Route = "/Contributors") rather than an
                // inline literal — resolve those to the actual pattern instead of "<dynamic>" (G2).
                var routeExpr = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                var route = ResolveRouteValue(routeExpr, cls.SyntaxTree.GetRoot(), routeConsts) ?? "<dynamic>";

                var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                if (!detectedKeys.Add($"{filePath}:{line}")) continue;

                model.Detections.Add(new EndpointDetection(
                    httpMethod, route,
                    cls.Identifier.ValueText, "HandleAsync", [], [])
                {
                    ExtractorName = "EndpointExtractor",
                    SourceFile = filePath,
                    LineNumber = line,
                });
            }
        }
    }

    /// <summary>Resolves a FastEndpoints route argument to its string value. Handles an inline literal,
    /// and a const/static string field referenced as <c>Type.Route</c> or bare <c>Route</c> (resolved
    /// from a string-literal field initializer in the same file). Returns null when it can't (e.g. an
    /// interpolated or cross-file expression) so the caller falls back to "&lt;dynamic&gt;".</summary>
    private static string? ResolveRouteValue(ExpressionSyntax? expr, SyntaxNode root,
        IReadOnlyDictionary<string, string>? index) => expr switch
    {
        LiteralExpressionSyntax lit => lit.Token.ValueText,
        // Type.Route — prefer the cross-file const index ("Type.Route"), then a same-file field lookup.
        MemberAccessExpressionSyntax ma when ma.Expression is IdentifierNameSyntax t
            && index is not null
            && index.TryGetValue($"{t.Identifier.ValueText}.{ma.Name.Identifier.ValueText}", out var v) => v,
        MemberAccessExpressionSyntax ma => FindConstStringValue(root, ma.Name.Identifier.ValueText),
        IdentifierNameSyntax id => FindConstStringValue(root, id.Identifier.ValueText),
        _ => null,
    };

    /// <summary>Finds a field named <paramref name="fieldName"/> with a string-literal initializer
    /// anywhere in the file (const string Route = "/x"). File-scoped lookup by name is enough for the
    /// one-request-type-per-file FastEndpoints layout.</summary>
    private static string? FindConstStringValue(SyntaxNode root, string fieldName)
    {
        foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            foreach (var v in field.Declaration.Variables)
            {
                if (v.Identifier.ValueText == fieldName
                    && v.Initializer?.Value is LiteralExpressionSyntax { Token.Value: string s })
                    return s;
            }
        }
        return null;
    }

    public static bool DerivesFromFastEndpoint(ClassDeclarationSyntax cls)
    {
        if (cls.BaseList == null) return false;
        return cls.BaseList.Types.Any(t =>
        {
            var name = t.Type.ToString();
            return name == "EndpointWithoutRequest"
                || name.StartsWith("Endpoint<", StringComparison.Ordinal)
                || name.StartsWith("Endpoint", StringComparison.Ordinal);
        });
    }
}
