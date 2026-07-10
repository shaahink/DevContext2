using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

[ExtractorOrder(10)]
public sealed class EndpointExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> MapMethods = HttpConstants.MapMethods;

    public string Name => "EndpointExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;

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

        // Project-wide const-string index so FastEndpoints routes referenced as `SomeRequest.Route`
        // resolve across files (G2).
        var routeConsts = await FastEndpointsHelper.BuildRouteConstIndex(context, ct);

        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            await ScanFile(filePath, context, model, detectedKeys, routeConsts, ct);
        }
    }

    private static async Task ScanFile(
        string filePath, DiscoveryContext context, DiscoveryModel model,
        HashSet<string> detectedKeys, IReadOnlyDictionary<string, string> routeConsts, CancellationToken ct)
    {
        SyntaxTree syntaxTree;
        try
        {
            syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
        }
        catch (Exception ex)
        {
            context.Logger.LogWarning(ex, "Failed to parse syntax tree: {Path}", filePath);
            return;
        }

        var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
        var allInvocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        // Global auth signal (E1): AddAuthorization(o => o.FallbackPolicy/DefaultPolicy =
        // ...RequireAuthenticatedUser()) protects every endpoint that carries no other auth metadata.
        // Detected once per analysis so AnonymousEndpointsSource never calls a covered endpoint "anonymous".
        DetectGlobalAuthFallback(root, filePath, detectedKeys, model);

        // Phase 1b pre-scan: MapGroup + NewVersionedApi chain detection — resolve group prefixes and
        // group-level auth conventions (RequireAuthorization/AllowAnonymous applied to the group, either
        // chained onto MapGroup(...) or as a later statement on the group variable — E1).
        var groupPrefixes = ExtractGroupPrefixes(root);
        var groupAuth = ExtractGroupAuth(root, groupPrefixes);

        // Phase 1: Find direct MapGet/MapPost/etc calls (app.MapGet("/route", handler))
        foreach (var invocation in allInvocations)
        {
            ct.ThrowIfCancellationRequested();
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (!MapMethods.Contains(memberAccess.Name.Identifier.ValueText)) continue;

            // Check if this is a call on a MapGroup variable (e.g. api.MapGet(...))
            var groupVarName = memberAccess.Expression is IdentifierNameSyntax groupVar ? groupVar.Identifier.ValueText : null;
            var groupPrefix = groupVarName is not null && groupPrefixes.TryGetValue(groupVarName, out var gp) ? gp : null;
            var groupAuthAttrs = groupVarName is not null && groupAuth.TryGetValue(groupVarName, out var ga) ? ga : [];

            AddEndpoint(invocation, memberAccess, filePath, detectedKeys, model, groupPrefix, groupAuthAttrs);
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
            var extGroupAuth = ExtractGroupAuth(extMethod, extGroupPrefixes);
            var extInvocations = extMethod.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in extInvocations)
            {
                ct.ThrowIfCancellationRequested();
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
                if (!MapMethods.Contains(memberAccess.Name.Identifier.ValueText)) continue;

                var groupVarName = memberAccess.Expression is IdentifierNameSyntax groupVar ? groupVar.Identifier.ValueText : null;
                var groupPrefix = groupVarName is not null && extGroupPrefixes.TryGetValue(groupVarName, out var gp) ? gp : null;
                var groupAuthAttrs = groupVarName is not null && extGroupAuth.TryGetValue(groupVarName, out var ga) ? ga : [];

                AddEndpoint(invocation, memberAccess, filePath, detectedKeys, model, groupPrefix, groupAuthAttrs);
            }
        }

        // Phase 3+4: FastEndpoints-style class detection
        var fastEndpointClasses = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => FastEndpointsHelper.DerivesFromFastEndpoint(c))
            .ToList();

        FastEndpointsHelper.DetectClassAttributeEndpoints(fastEndpointClasses, filePath, detectedKeys, model);
        FastEndpointsHelper.DetectConfigureMethodEndpoints(fastEndpointClasses, filePath, detectedKeys, model, routeConsts);
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
        string? groupPrefix = null,
        ImmutableArray<string> groupAuthAttributes = default)
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

        // Endpoint's own chain (`.MapGet(...).RequireAuthorization()`/`.AllowAnonymous()`) always wins;
        // an endpoint with no auth call of its own inherits whatever the group decided (E1).
        var ownAuthAttrs = ExtractAuthFromChain(invocation);
        var authAttrs = ownAuthAttrs.Length > 0
            ? ownAuthAttrs
            : (groupAuthAttributes.IsDefault ? [] : groupAuthAttributes);
        var handlerArg = FindHandler(invocation);
        var handlerMethod = handlerArg switch
        {
            LambdaExpressionSyntax => "<lambda>",
            AnonymousMethodExpressionSyntax => "<anonymous>",
            IdentifierNameSyntax id => id.Identifier.ValueText,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
            _ => "<lambda>"
        };

        // HandlerType must name the *owning type* so the graph can resolve the handler — not the method
        // name (G1). A bare identifier (CreateOrderAsync) is a method group on the enclosing type; a
        // qualified reference (OrdersApi.CreateOrderAsync) names its type as the member-access qualifier;
        // a lambda/anonymous handler carries its source text (the graph keys off "=>" to anchor a
        // per-endpoint node). Previously this stored handlerArg.ToString() for all shapes, so a
        // method-group endpoint recorded the method name as the type and never resolved.
        var handlerInfo = handlerArg switch
        {
            IdentifierNameSyntax => invocation.Ancestors().OfType<TypeDeclarationSyntax>()
                .FirstOrDefault()?.Identifier.ValueText ?? handlerArg.ToString(),
            MemberAccessExpressionSyntax ma => ma.Expression.ToString(),
            null => "?",
            _ => handlerArg.ToString(),
        };

        // Capture the handler's OWN body + line so the graph anchors a per-endpoint node on exactly
        // what this route runs — not the shared registration type. Two shapes carry a body:
        //   • inline lambda/anonymous (minimal API) — the lambda source itself (G5);
        //   • a method-group reference to a method in the SAME class (eShop's MapPost("/",
        //     CreateOrderAsync)) — the referenced method's body, found syntactically. This makes
        //     per-endpoint dispatch precise: GET methods that only query resolve to no command,
        //     instead of the type-level route heuristic guessing one (G1b).
        var handlerLine = 0;
        string? handlerBody = null;
        if (handlerArg is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax)
        {
            handlerLine = handlerArg.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            handlerBody = handlerArg.ToString();
        }
        else if (handlerArg is IdentifierNameSyntax idName)
        {
            var method = invocation.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault()
                ?.Members.OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == idName.Identifier.ValueText);
            if (method is not null)
            {
                handlerLine = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                handlerBody = method.ToString();
            }
        }

        model.Detections.Add(new EndpointDetection(httpMethod, fullRoute, handlerInfo, handlerMethod, authAttrs, [], groupPrefix, handlerLine, handlerBody)
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = filePath,
            LineNumber = line,
        });
    }

    private static ImmutableArray<string> ExtractAuthFromChain(InvocationExpressionSyntax mapInvocation)
    {
        var auth = new List<string>();
        SyntaxNode node = mapInvocation;

        while (node.Parent is MemberAccessExpressionSyntax ma && ma.Parent is InvocationExpressionSyntax chainInvoke)
        {
            var attr = AuthAttributeForCall(ma.Name.Identifier.ValueText, chainInvoke);
            if (attr is not null) auth.Add(attr);
            node = chainInvoke;
        }

        return [.. auth];
    }

    private static string? AuthAttributeForCall(string methodName, InvocationExpressionSyntax call)
    {
        if (methodName == "RequireAuthorization")
        {
            var policyArg = call.ArgumentList.Arguments.FirstOrDefault();
            return policyArg?.Expression is LiteralExpressionSyntax lit
                ? $"[Authorize({lit.Token.ValueText})]"
                : "[Authorize]";
        }
        if (methodName == "AllowAnonymous") return "[AllowAnonymous]";
        return null;
    }

    /// <summary>
    /// Group-level auth conventions (E1): a group builder can be given `RequireAuthorization`/
    /// `AllowAnonymous` either chained directly onto its `MapGroup(...)` call, or as a separate later
    /// statement on the group variable (`group.RequireAuthorization(pb => ...)`) — the common shape for
    /// custom policy builders. Member endpoints registered on that group inherit these unless they
    /// specify their own auth chain.
    /// </summary>
    private static Dictionary<string, ImmutableArray<string>> ExtractGroupAuth(
        SyntaxNode root, Dictionary<string, string> groupPrefixes)
    {
        var groupAuth = new Dictionary<string, List<string>>();

        void AddAuth(string varName, string? attr)
        {
            if (attr is null) return;
            if (!groupAuth.TryGetValue(varName, out var list))
            {
                list = [];
                groupAuth[varName] = list;
            }
            list.Add(attr);
        }

        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            var methodName = memberAccess.Name.Identifier.ValueText;

            // Standalone statement on a known group variable — `group.RequireAuthorization(...)`.
            if ((methodName == "RequireAuthorization" || methodName == "AllowAnonymous")
                && memberAccess.Expression is IdentifierNameSyntax idExpr
                && groupPrefixes.ContainsKey(idExpr.Identifier.ValueText))
            {
                AddAuth(idExpr.Identifier.ValueText, AuthAttributeForCall(methodName, invocation));
                continue;
            }

            // Chained directly onto the MapGroup(...) call — `app.MapGroup("/x").RequireAuthorization()`.
            if (methodName != "MapGroup") continue;
            var varName = FindAssignedVariable(invocation);
            if (varName is null || !groupPrefixes.ContainsKey(varName)) continue;

            foreach (var attr in ExtractAuthFromChain(invocation))
                AddAuth(varName, attr);
        }

        return groupAuth.ToDictionary(kv => kv.Key, kv => (ImmutableArray<string>)[.. kv.Value]);
    }

    /// <summary>
    /// Detects an app-wide fallback/default authorization policy
    /// (`AddAuthorization(o => o.FallbackPolicy = ...RequireAuthenticatedUser())`). When present, an
    /// endpoint with no auth metadata of its own is protected by default — the insight layer must not
    /// call it "anonymous" (E1).
    /// </summary>
    private static void DetectGlobalAuthFallback(
        SyntaxNode root, string filePath, HashSet<string> detectedKeys, DiscoveryModel model)
    {
        if (detectedKeys.Contains("global-auth-fallback-policy")) return;

        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (memberAccess.Name.Identifier.ValueText != "AddAuthorization") continue;

            var lambda = invocation.ArgumentList.Arguments
                .Select(a => a.Expression)
                .OfType<LambdaExpressionSyntax>()
                .FirstOrDefault();
            if (lambda is null) continue;

            var setsAuthRequiredDefault = lambda.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Any(a =>
                    a.Left is MemberAccessExpressionSyntax lma
                    && lma.Name.Identifier.ValueText is "FallbackPolicy" or "DefaultPolicy"
                    && a.Right.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                        .Any(i => i.Expression is MemberAccessExpressionSyntax rma
                            && rma.Name.Identifier.ValueText == "RequireAuthenticatedUser"));

            if (!setsAuthRequiredDefault) continue;

            detectedKeys.Add("global-auth-fallback-policy");
            var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            model.Detections.Add(new GlobalAuthPolicyDetection(true)
            {
                ExtractorName = "EndpointExtractor",
                SourceFile = filePath,
                LineNumber = line,
            });
            return;
        }
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
