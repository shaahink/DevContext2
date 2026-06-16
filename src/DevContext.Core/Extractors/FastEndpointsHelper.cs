using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors;

/// <summary>Helper for detecting FastEndpoints-style endpoints via syntax analysis (Phases 3 and 4).</summary>
public static class FastEndpointsHelper
{
    public static void DetectClassAttributeEndpoints(
        IEnumerable<ClassDeclarationSyntax> fastEndpointClasses,
        string filePath,
        ISet<string> detectedKeys,
        DiscoveryModel model)
    {
        foreach (var cls in fastEndpointClasses)
        {
            var httpAttr = cls.AttributeLists
                .SelectMany(a => a.Attributes)
                .FirstOrDefault(a => HttpConstants.HttpVerbAttributes.Contains(a.Name.ToString(), StringComparer.Ordinal));

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

    public static void DetectConfigureMethodEndpoints(
        IEnumerable<ClassDeclarationSyntax> fastEndpointClasses,
        string filePath,
        ISet<string> detectedKeys,
        DiscoveryModel model)
    {
        foreach (var cls in fastEndpointClasses)
        {
            var configure = cls.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => string.Equals(m.Identifier.Text, "Configure", StringComparison.Ordinal));
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

                var routeArg = invocation.ArgumentList.Arguments.FirstOrDefault();
                var route = routeArg?.Expression is LiteralExpressionSyntax lit
                    ? lit.Token.ValueText
                    : "<dynamic>";

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

    public static bool DerivesFromFastEndpoint(ClassDeclarationSyntax cls)
    {
        if (cls.BaseList == null) return false;
        return cls.BaseList.Types.Any(t =>
        {
            var name = t.Type.ToString();
            return string.Equals(name, "EndpointWithoutRequest"
, StringComparison.Ordinal) || name.StartsWith("Endpoint<", StringComparison.Ordinal)
                || name.StartsWith("Endpoint", StringComparison.Ordinal);
        });
    }
}
