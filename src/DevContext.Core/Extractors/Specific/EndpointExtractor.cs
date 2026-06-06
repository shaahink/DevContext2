using System.Collections.Immutable;
using DevContext.Core.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

[ExtractorOrder(10)]
public sealed class EndpointExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> MapMethods = HttpConstants.MapMethods;
    private static readonly ImmutableArray<string> HttpVerbAttributes = HttpConstants.HttpVerbAttributes;

    public string Name => "EndpointExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;

    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MinimalApis, ArchitectureSignals.Keys.FastEndpoints, ArchitectureSignals.Keys.Controllers],
        ["endpoint-detections"],
        ["model.Detections"],
        "Detects HTTP endpoints: Minimal API Map* calls, FastEndpoints, MVC controllers");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MinimalApis)
        || currentModel.Architecture.Has(ArchitectureSignals.Keys.FastEndpoints)
        || currentModel.Architecture.Has(ArchitectureSignals.Keys.Controllers);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var detectedKeys = new HashSet<string>();

        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            await ScanFile(filePath, context, model, detectedKeys, ct);
        }
    }

    private static async Task ScanFile(
        string filePath, DiscoveryContext context, DiscoveryModel model,
        HashSet<string> detectedKeys, CancellationToken ct)
    {
        SyntaxTree syntaxTree;
        try
        {
            syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
        }
        catch
        {
            return;
        }

        var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
        var allInvocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        // Phase 1b pre-scan: MapGroup + NewVersionedApi chain detection — resolve group prefixes
        var groupPrefixes = ExtractGroupPrefixes(root);

        // Phase 1: Find direct MapGet/MapPost/etc calls (app.MapGet("/route", handler))
        foreach (var invocation in allInvocations)
        {
            ct.ThrowIfCancellationRequested();
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (!MapMethods.Contains(memberAccess.Name.Identifier.ValueText)) continue;

            // Check if this is a call on a MapGroup variable (e.g. api.MapGet(...))
            var groupPrefix = memberAccess.Expression is IdentifierNameSyntax groupVar
                && groupPrefixes.TryGetValue(groupVar.Identifier.ValueText, out var gp)
                ? gp
                : null;

            AddEndpoint(invocation, memberAccess, filePath, detectedKeys, model, groupPrefix);
        }

        // Phase 2: Find extension methods that take IEndpointRouteBuilder/WebApplication
        // and scan their bodies for Map* calls (catches MapTodoEndpoints, etc.)
        var extMethods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => IsEndpointExtension(m));

        foreach (var extMethod in extMethods)
        {
            // Scan for MapGroup calls within the extension method body for prefix resolution
            var extGroupPrefixes = ExtractGroupPrefixes(extMethod);
            var extInvocations = extMethod.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in extInvocations)
            {
                ct.ThrowIfCancellationRequested();
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
                if (!MapMethods.Contains(memberAccess.Name.Identifier.ValueText)) continue;

                var groupPrefix = memberAccess.Expression is IdentifierNameSyntax groupVar
                    && extGroupPrefixes.TryGetValue(groupVar.Identifier.ValueText, out var gp)
                    ? gp
                    : null;

                AddEndpoint(invocation, memberAccess, filePath, detectedKeys, model, groupPrefix);
            }
        }

        // Phase 3: Detect FastEndpoints-style classes with HTTP verb attributes
        var fastEndpointClasses = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => DerivesFromFastEndpoint(c));

        foreach (var cls in fastEndpointClasses)
        {
            ct.ThrowIfCancellationRequested();
            var httpAttr = cls.AttributeLists
                .SelectMany(a => a.Attributes)
                .FirstOrDefault(a => HttpVerbAttributes.Contains(a.Name.ToString()));

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

        // Phase 4: FastEndpoints Configure() method pattern
        // Classes deriving from Endpoint<T> define Configure() with HTTP verb method calls like Post("/route")
        foreach (var cls in fastEndpointClasses)
        {
            ct.ThrowIfCancellationRequested();

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

    private static bool DerivesFromFastEndpoint(ClassDeclarationSyntax cls)
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

    private static bool IsEndpointExtension(MethodDeclarationSyntax method)
    {
        if (method.ParameterList.Parameters.Count == 0) return false;
        var firstType = method.ParameterList.Parameters[0].Type?.ToString() ?? "";
        return firstType.Contains("WebApplication")
            || firstType.Contains("IEndpointRouteBuilder")
            || firstType.Contains("RouteGroupBuilder");
    }

    private static void AddEndpoint(
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax memberAccess,
        string filePath,
        HashSet<string> detectedKeys,
        DiscoveryModel model,
        string? groupPrefix = null)
    {
        var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        if (!detectedKeys.Add($"{filePath}:{line}")) return;

        var methodName = memberAccess.Name.Identifier.ValueText;
        var httpMethod = HttpConstants.MapMethodToVerb.TryGetValue(methodName, out var verb) ? verb : "UNKNOWN";

        var routeArg = invocation.ArgumentList.Arguments
            .FirstOrDefault(a => a.Expression is LiteralExpressionSyntax)
            ?.Expression as LiteralExpressionSyntax;
        var routeTemplate = routeArg?.Token.ValueText ?? "/";

        // Combine group prefix if present, then normalize leading slash
        var fullRoute = groupPrefix is not null
            ? $"{groupPrefix}/{routeTemplate}".Replace("//", "/")
            : routeTemplate;
        fullRoute = NormalizeRoute(fullRoute);

        var handlerArg = FindHandler(invocation);
        var handlerInfo = handlerArg?.ToString() ?? "?";
        var handlerMethod = handlerArg switch
        {
            LambdaExpressionSyntax => "<lambda>",
            AnonymousMethodExpressionSyntax => "<anonymous>",
            IdentifierNameSyntax id => id.Identifier.ValueText,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
            _ => "<lambda>"
        };

        model.Detections.Add(new EndpointDetection(httpMethod, fullRoute, handlerInfo, handlerMethod, [], [], groupPrefix)
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = filePath,
            LineNumber = line,
        });
    }

    private static Dictionary<string, string> ExtractGroupPrefixes(SyntaxNode root)
    {
        var prefixes = new Dictionary<string, string>();
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (memberAccess.Name.Identifier.ValueText != "MapGroup") continue;

            var prefixArg = invocation.ArgumentList.Arguments.FirstOrDefault();
            var prefix = prefixArg?.Expression is LiteralExpressionSyntax lit
                ? lit.Token.ValueText
                : null;
            if (prefix is null) continue;

            var variableName = FindAssignedVariable(invocation);
            if (variableName is null) continue;

            prefixes[variableName] = prefix;
        }

        // Resolve multi-level chains
        foreach (var key in prefixes.Keys.ToList())
        {
            var resolved = prefixes[key];
            foreach (var (varName, varPrefix) in prefixes)
            {
                if (resolved != varPrefix && resolved.Contains(varName))
                    resolved = resolved.Replace(varName, varPrefix);
            }
            prefixes[key] = resolved;
        }

        return prefixes;
    }

    private static string? FindAssignedVariable(InvocationExpressionSyntax invocation)
    {
        // Walk up chained calls: app.MapGroup("x").HasApiVersion(1,0) → outermost invocation
        var outermost = invocation;
        while (outermost.Parent is MemberAccessExpressionSyntax chainAccess
            && chainAccess.Parent is InvocationExpressionSyntax chainInvocation)
        {
            outermost = chainInvocation;
        }

        // Case 1: var x = outer.MapGroup(...) or var x = outer.MapGroup(...).Chain(...)
        if (outermost.Parent is EqualsValueClauseSyntax eq
            && eq.Parent is VariableDeclaratorSyntax decl)
            return decl.Identifier.ValueText;

        // Case 2: x = outer.MapGroup(...) (assignment to existing variable)
        if (outermost.Parent is AssignmentExpressionSyntax assign
            && assign.Left is IdentifierNameSyntax id)
            return id.Identifier.ValueText;

        return null;
    }

    private static ExpressionSyntax? FindHandler(InvocationExpressionSyntax invocation)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count == 0) return null;

        // If there's only one argument, it's the handler (no route string)
        if (args.Count == 1)
        {
            var expr = args[0].Expression;
            if (expr is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax
                or IdentifierNameSyntax or MemberAccessExpressionSyntax)
                return expr;
            return null;
        }

        // Handler is the last argument that looks like a delegate
        for (int i = args.Count - 1; i >= 0; i--)
        {
            var expr = args[i].Expression;
            if (expr is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax
                or IdentifierNameSyntax or MemberAccessExpressionSyntax)
                return expr;
        }

        return args[^1].Expression;
    }

    private static string NormalizeRoute(string route)
    {
        if (string.IsNullOrEmpty(route) || route == "/") return route;
        if (!route.StartsWith('/'))
            route = "/" + route;
        return route;
    }
}
