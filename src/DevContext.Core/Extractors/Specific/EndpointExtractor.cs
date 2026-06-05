using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

[ExtractorOrder(10)]
public sealed class EndpointExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> MapMethods = ["MapGet", "MapPost", "MapPut", "MapDelete", "MapPatch"];

    public string Name => "EndpointExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;

    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MinimalApis], ["endpoint-detections"],
        ["model.Detections"],
        "Detects minimal API endpoints via direct Map* calls and extension method bodies");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MinimalApis);

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

        // Phase 1: Find direct MapGet/MapPost/etc calls (app.MapGet("/route", handler))
        foreach (var invocation in allInvocations)
        {
            ct.ThrowIfCancellationRequested();
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (!MapMethods.Contains(memberAccess.Name.Identifier.ValueText)) continue;
            AddEndpoint(invocation, memberAccess, filePath, detectedKeys, model);
        }

        // Phase 2: Find extension methods that take IEndpointRouteBuilder/WebApplication
        // and scan their bodies for Map* calls (catches MapTodoEndpoints, etc.)
        var extMethods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => IsEndpointExtension(m));

        foreach (var extMethod in extMethods)
        {
            var extInvocations = extMethod.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in extInvocations)
            {
                ct.ThrowIfCancellationRequested();
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
                if (!MapMethods.Contains(memberAccess.Name.Identifier.ValueText)) continue;
                AddEndpoint(invocation, memberAccess, filePath, detectedKeys, model);
            }
        }
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
        DiscoveryModel model)
    {
        var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        if (!detectedKeys.Add($"{filePath}:{line}")) return;

        var methodName = memberAccess.Name.Identifier.ValueText;
        var httpMethod = methodName switch
        {
            "MapGet" => "GET",
            "MapPost" => "POST",
            "MapPut" => "PUT",
            "MapDelete" => "DELETE",
            "MapPatch" => "PATCH",
            _ => "UNKNOWN",
        };

        var routeArg = invocation.ArgumentList.Arguments
            .FirstOrDefault(a => a.Expression is LiteralExpressionSyntax)
            ?.Expression as LiteralExpressionSyntax;
        var routeTemplate = routeArg?.Token.ValueText ?? "/";

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

        model.Detections.Add(new EndpointDetection(httpMethod, routeTemplate, handlerInfo, handlerMethod, [], [])
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = filePath,
            LineNumber = line,
        });
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
}
