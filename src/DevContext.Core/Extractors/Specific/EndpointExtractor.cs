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

        // Repo-wide caller-prefix index so a group's prefix crosses the extension-method boundary
        // (B3): `var shows = app.MapGroup("/shows"); shows.MapShowsApi();` in Program.cs must reach
        // the `group.MapGet("/", …)` calls inside MapShowsApi in another file. Composed only when
        // every observed call site agrees on one prefix — an ambiguous or mixed-receiver method
        // keeps its routes bare rather than guessing.
        var extensionCallerPrefixes = await BuildExtensionCallerPrefixIndex(context, routeConsts, ct);

        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            await ScanFile(filePath, context, model, detectedKeys, routeConsts, extensionCallerPrefixes, ct);
        }
    }

    private static async Task ScanFile(
        string filePath, DiscoveryContext context, DiscoveryModel model,
        HashSet<string> detectedKeys, IReadOnlyDictionary<string, string> routeConsts,
        IReadOnlyDictionary<string, CallerGroupContext> extensionCallerPrefixes, CancellationToken ct)
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
        var groupPrefixes = ExtractGroupPrefixes(root, routeConsts);
        var groupAuth = ExtractGroupAuth(root, groupPrefixes);

        // Phase 2 FIRST (B3): extension methods that take IEndpointRouteBuilder/WebApplication/
        // RouteGroupBuilder, scanned with per-method scope. It runs before the whole-file pass because
        // AddEndpoint dedups on file:line and the per-method resolution is strictly better scoped: the
        // method's own group vars can't collide with same-named vars elsewhere in the file, and the
        // receiver parameter can be seeded with the caller's composed prefix from the repo-wide index.
        var extMethods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => IsEndpointExtension(m));

        foreach (var extMethod in extMethods)
        {
            // Seed the receiver parameter with the caller's group prefix (B3) — only for group-capable
            // receiver types; a WebApplication receiver can never be a RouteGroupBuilder at a call site,
            // so an index hit there is a same-name different method.
            Dictionary<string, string>? seed = null;
            Dictionary<string, ImmutableArray<string>>? authSeed = null;
            var firstParam = extMethod.ParameterList.Parameters[0];
            var firstParamType = firstParam.Type?.ToString() ?? "";
            if ((firstParamType.Contains("RouteGroupBuilder") || firstParamType.Contains("IEndpointRouteBuilder"))
                && extensionCallerPrefixes.TryGetValue(extMethod.Identifier.ValueText, out var callerContext))
            {
                seed = new Dictionary<string, string> { [firstParam.Identifier.ValueText] = callerContext.Prefix };

                // The group's authorization travels with its prefix (F2) — the routes are in this file,
                // the policy that guards them is in the caller's.
                if (!callerContext.Auth.IsDefaultOrEmpty)
                {
                    authSeed = new Dictionary<string, ImmutableArray<string>>
                    {
                        [firstParam.Identifier.ValueText] = callerContext.Auth,
                    };
                }
            }

            var extGroupPrefixes = ExtractGroupPrefixes(extMethod, routeConsts, seed);
            var extGroupAuth = ExtractGroupAuth(extMethod, extGroupPrefixes, authSeed);
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

        // Phase 1: direct MapGet/MapPost/etc calls (app.MapGet("/route", handler)) anywhere else in
        // the file — extension-method endpoints were already claimed above with better-scoped prefixes.
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
    /// <param name="seed">Auth conventions entering this scope from outside — the extension-method
    /// receiver parameter carrying its caller's group policy (B3/F2). A sub-group declared off that
    /// receiver inherits it too: `var sub = app.MapGroup("/x")` inside a method whose `app` is the
    /// admin group is still the admin surface.</param>
    private static Dictionary<string, ImmutableArray<string>> ExtractGroupAuth(
        SyntaxNode root,
        Dictionary<string, string> groupPrefixes,
        IReadOnlyDictionary<string, ImmutableArray<string>>? seed = null)
    {
        var groupAuth = new Dictionary<string, List<string>>();

        if (seed is not null)
        {
            foreach (var (name, attributes) in seed)
                groupAuth[name] = [.. attributes];

            // Carry it to group vars declared off a seeded receiver, transitively.
            var receivers = GroupReceivers(root);
            foreach (var (varName, receiver) in receivers)
            {
                var origin = receiver;
                var guard = new HashSet<string>(StringComparer.Ordinal);
                while (origin is not null && guard.Add(origin))
                {
                    if (seed.TryGetValue(origin, out var inherited))
                    {
                        if (!groupAuth.TryGetValue(varName, out var list))
                            groupAuth[varName] = list = [];
                        foreach (var attribute in inherited)
                        {
                            if (!list.Contains(attribute)) list.Add(attribute);
                        }
                        break;
                    }
                    origin = receivers.TryGetValue(origin, out var next) ? next : null;
                }
            }
        }

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

    /// <summary>Maps group-builder variable names to their FULL route prefixes within one scope (a file
    /// root or a single extension method). Nesting composes through the MapGroup receiver — `var v1 =
    /// api.MapGroup("/v1")` where `api` is itself a group var resolves to "/api/v1" (B3). An optional
    /// <paramref name="seed"/> pre-binds names that enter the scope from outside — the extension-method
    /// receiver parameter carrying its caller's composed prefix. Unresolvable receivers (`app`,
    /// `NewVersionedApi(...)` chains) contribute nothing: the var keeps its own literal, as before.</summary>
    private static Dictionary<string, string> ExtractGroupPrefixes(
        SyntaxNode root,
        IReadOnlyDictionary<string, string> routeConsts,
        IReadOnlyDictionary<string, string>? seed = null)
    {
        var defs = new Dictionary<string, (string? Receiver, string Prefix)>();
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (memberAccess.Name.Identifier.ValueText != "MapGroup") continue;

            var prefixArg = invocation.ArgumentList.Arguments.FirstOrDefault();
            var prefix = ResolveGroupPrefixArgument(prefixArg?.Expression, routeConsts);
            if (prefix is null) continue;

            var variableName = FindAssignedVariable(invocation);
            if (variableName is null) continue;

            var receiver = memberAccess.Expression is IdentifierNameSyntax recv
                ? recv.Identifier.ValueText
                : null;
            defs[variableName] = (receiver, prefix);
        }

        var resolved = new Dictionary<string, string>();
        string Resolve(string name, HashSet<string> visiting)
        {
            if (resolved.TryGetValue(name, out var done)) return done;
            var (receiver, prefix) = defs[name];
            var full = prefix;
            if (receiver is not null && visiting.Add(name))
            {
                if (defs.ContainsKey(receiver) && !visiting.Contains(receiver))
                    full = CombinePrefix(Resolve(receiver, visiting), prefix);
                else if (seed is not null && seed.TryGetValue(receiver, out var seedPrefix))
                    full = CombinePrefix(seedPrefix, prefix);
            }
            resolved[name] = full;
            return full;
        }
        foreach (var key in defs.Keys)
            Resolve(key, []);

        // Seed names with no local re-definition resolve to the seed value itself
        // (`group.MapGet("/", …)` directly on the receiver parameter).
        if (seed is not null)
        {
            foreach (var (name, prefix) in seed)
                resolved.TryAdd(name, prefix);
        }

        return resolved;
    }

    /// <summary>Resolves a `MapGroup(...)` prefix argument to its route string. A literal answers
    /// directly; a const-string reference resolves through the project-wide const index — either bare
    /// (`MapGroup(GroupPrefix)`, the enclosing type's own const) or qualified
    /// (`MapGroup(AdminEndpoints.GroupPrefix)`). Returns null for a genuine expression, which keeps
    /// the previous behaviour: the group contributes nothing rather than being guessed at.
    /// <para>
    /// This is load-bearing beyond the route text. A group whose prefix does not resolve is never
    /// registered as a group variable at all, so <see cref="ExtractGroupAuth"/> — which gates on
    /// <c>groupPrefixes.ContainsKey</c> — also loses the group's `RequireAuthorization`, and every
    /// endpoint on it reads as un-annotated. Naming the group's prefix with a constant is the
    /// idiomatic way to write it, and it used to cost the whole surface its auth.
    /// </para></summary>
    private static string? ResolveGroupPrefixArgument(
        ExpressionSyntax? expression, IReadOnlyDictionary<string, string> routeConsts)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax { Token.Value: string literal }:
                return literal;

            // `MapGroup(GroupPrefix)` — a const on the type the call is written in.
            case IdentifierNameSyntax bare:
            {
                var owner = expression.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                return owner is not null
                    && routeConsts.TryGetValue(
                        $"{owner.Identifier.ValueText}.{bare.Identifier.ValueText}", out var own)
                    ? own
                    : null;
            }

            // `MapGroup(AdminEndpoints.GroupPrefix)` — the index's own key shape.
            case MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax type, Name: { } member }:
                return routeConsts.TryGetValue(
                    $"{type.Identifier.ValueText}.{member.Identifier.ValueText}", out var qualified)
                    ? qualified
                    : null;

            default:
                return null;
        }
    }

    /// <summary>Group variable → the identifier it was built from, for `var sub = parent.MapGroup(...)`.
    /// Only the identifier-receiver shape is recorded; anything else has no name to inherit from.</summary>
    private static Dictionary<string, string> GroupReceivers(SyntaxNode root)
    {
        var receivers = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (memberAccess.Name.Identifier.ValueText != "MapGroup") continue;
            if (memberAccess.Expression is not IdentifierNameSyntax receiver) continue;
            if (FindAssignedVariable(invocation) is not { } variableName) continue;
            receivers[variableName] = receiver.Identifier.ValueText;
        }
        return receivers;
    }

    /// <summary>Composes a parent group prefix with a child's own prefix, normalizing the seam slash.</summary>
    private static string CombinePrefix(string parent, string child)
        => NormalizeRoute($"{parent}/{child}".Replace("//", "/"));

    /// <summary>What a group builder carries at the call site of an endpoint extension method: its
    /// composed route prefix, and the auth conventions applied to the group. Both cross the
    /// extension-method boundary together — a group is one surface, and its authorization is a
    /// property of that surface, not of the file the routes happen to be written in.</summary>
    /// <remarks>Equality is by VALUE over both members. <see cref="ImmutableArray{T}"/> compares by
    /// reference, so the compiler-generated equality would make two call sites that agree perfectly
    /// look like disagreement — and the agreement rule below would then silently drop the group.</remarks>
    private readonly record struct CallerGroupContext(string Prefix, ImmutableArray<string> Auth)
    {
        public bool Equals(CallerGroupContext other) =>
            string.Equals(Prefix, other.Prefix, StringComparison.Ordinal)
            && Auth.AsSpan().SequenceEqual(other.Auth.AsSpan(), StringComparer.Ordinal);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Prefix, StringComparer.Ordinal);
            foreach (var attribute in Auth) hash.Add(attribute, StringComparer.Ordinal);
            return hash.ToHashCode();
        }
    }

    /// <summary>Builds the repo-wide method-name → caller-group-context index for B3. Records every
    /// member call whose receiver is a resolved group builder — `shows.MapShowsApi()` (group variable)
    /// and `app.MapGroup("/shows").MapShowsApi()` (inline chain) both index MapShowsApi → "/shows".
    /// A name survives only when (a) every prefixed call site agrees on ONE prefix AND one set of auth
    /// conventions and (b) it is never also called on a plain identifier receiver (`app.MapXApi()`),
    /// where composing would be a guess — single-hop honesty: an extension calling another extension
    /// does not forward its seeded context.
    /// <para>
    /// The auth half is what makes the group's `RequireAuthorization` reach routes registered in
    /// another file (2026-08-26 unseen drive, F2). `var admin = app.MapGroup(Prefix).RequireAuthorization(p);
    /// admin.MapAdminLedgerEndpoints();` puts the policy in one file and the routes in another; before
    /// this carried, every one of those routes read as un-annotated and the insight layer called the
    /// most protected surface in the app anonymous.
    /// </para></summary>
    private static async Task<IReadOnlyDictionary<string, CallerGroupContext>> BuildExtensionCallerPrefixIndex(
        DiscoveryContext context, IReadOnlyDictionary<string, string> routeConsts, CancellationToken ct)
    {
        var prefixed = new Dictionary<string, HashSet<CallerGroupContext>>(StringComparer.Ordinal);
        var unprefixed = new HashSet<string>(StringComparer.Ordinal);

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
                continue; // the main scan logs the parse failure
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var groupPrefixes = ExtractGroupPrefixes(root, routeConsts);
            var groupAuth = ExtractGroupAuth(root, groupPrefixes);

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
                var name = memberAccess.Name.Identifier.ValueText;
                if (name == "MapGroup" || MapMethods.Contains(name)) continue;

                CallerGroupContext? context2 = memberAccess.Expression switch
                {
                    IdentifierNameSyntax id when groupPrefixes.TryGetValue(id.Identifier.ValueText, out var p) =>
                        new CallerGroupContext(
                            p,
                            groupAuth.TryGetValue(id.Identifier.ValueText, out var a) ? a : []),

                    // `app.MapGroup("/x").RequireAuthorization().MapXApi()` — the group is never named,
                    // so its conventions have to be read off the chain itself.
                    InvocationExpressionSyntax chain when ChainMapGroupPrefix(chain, groupPrefixes, routeConsts) is { } cp =>
                        new CallerGroupContext(cp, [.. ExtractAuthFromChain(chain)]),

                    _ => null,
                };

                if (context2 is { } resolved)
                {
                    if (!prefixed.TryGetValue(name, out var set))
                        prefixed[name] = set = [];
                    set.Add(resolved);
                }
                else if (memberAccess.Expression is IdentifierNameSyntax)
                {
                    unprefixed.Add(name);
                }
            }
        }

        return prefixed
            .Where(kv => kv.Value.Count == 1 && !unprefixed.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value.First(), StringComparer.Ordinal);
    }

    /// <summary>Walks INWARD through a fluent chain receiver to the innermost `MapGroup("literal")`
    /// link — `app.MapGroup("/x").RequireAuthorization().MapXApi()` yields "/x", composed with the
    /// MapGroup receiver's own prefix when that receiver is a resolved group variable. Returns null
    /// when the chain carries no resolvable MapGroup prefix (e.g. `NewVersionedApi(...)` chains).</summary>
    private static string? ChainMapGroupPrefix(
        InvocationExpressionSyntax chain,
        Dictionary<string, string> groupPrefixes,
        IReadOnlyDictionary<string, string> routeConsts)
    {
        var current = chain;
        while (true)
        {
            if (current.Expression is not MemberAccessExpressionSyntax memberAccess) return null;
            if (memberAccess.Name.Identifier.ValueText == "MapGroup")
            {
                var prefixArg = current.ArgumentList.Arguments.FirstOrDefault();
                if (ResolveGroupPrefixArgument(prefixArg?.Expression, routeConsts) is not { } own) return null;
                return memberAccess.Expression is IdentifierNameSyntax recv
                    && groupPrefixes.TryGetValue(recv.Identifier.ValueText, out var parent)
                    ? CombinePrefix(parent, own)
                    : own;
            }
            if (memberAccess.Expression is InvocationExpressionSyntax inner)
            {
                current = inner;
                continue;
            }
            return null;
        }
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
