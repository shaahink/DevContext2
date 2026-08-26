using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(45)]
public sealed class DiRegistrationExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableHashSet<string> LifetimeMethods =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "AddSingleton", "AddScoped", "AddTransient");

    public string Name => "DiRegistrationExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;

    public ExtractorCapabilities Capabilities => new(
        [], ["di-registrations"],
        ["model.Detections"],
        "Cheap syntax matching for services.AddSingleton/AddScoped/AddTransient and AddX extension methods, "
        + "incl. Options-pattern config bindings (AddOptions<T>().BindConfiguration / Configure<T>)");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        // Two-phase, output-preserving parallelism (P5): parse + build per-file detection lists in
        // parallel, then commit to model.Detections single-threaded in source-file order. model.Detections
        // is a ConcurrentBag (add is thread-safe), but its ORDER fed the old CallGraphExtractor's diMap (retired in Batch A)
        // (last-write-wins by key), so committing serially in source order keeps the output identical.
        var files = context.Analysis.AllSourceFiles;
        var perFile = new List<Detection>[files.Count];

        // F3 (BUG-BACKLOG #34): section names in Options bindings are idiomatically string consts
        // (`BindConfiguration(QueueDrainOptions.SectionName)`), so they resolve through the same
        // project-wide const index F2 used for group prefixes. Built once, read concurrently —
        // syntax trees are cached, so the extra pass is cheap.
        var stringConsts = await FastEndpointsHelper.BuildRouteConstIndex(context, ct);

        var opts = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount };
        await Parallel.ForEachAsync(Enumerable.Range(0, files.Count), opts, async (i, innerCt) =>
        {
            var filePath = files[i];

            // Use shared syntax node cache — populated first by SyntaxStructureExtractor (thread-safe Lazy).
            FileSyntaxNodes nodes;
            try
            {
                var tree = await context.Cache.GetSyntaxTreeAsync(filePath, innerCt);
                nodes = await context.Analysis.GetOrParseSyntaxNodesAsync(filePath, async () =>
                {
                    var root = await tree.GetRootAsync(innerCt).ConfigureAwait(false);
                    return new FileSyntaxNodes(
                        [.. root.DescendantNodes().OfType<TypeDeclarationSyntax>()],
                        [.. root.DescendantNodes().OfType<InvocationExpressionSyntax>()]
                    );
                });
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning(ex, "Failed to parse syntax for DI registrations: {Path}", filePath);
                return;
            }

            perFile[i] = BuildDetections(filePath, nodes, stringConsts);
        });

        // Phase 2: commit in source-file order (identical ordering to the prior serial loop).
        foreach (var list in perFile)
        {
            if (list is null) continue;
            foreach (var detection in list)
                model.Detections.Add(detection);
        }
    }

    private List<Detection> BuildDetections(
        string filePath, FileSyntaxNodes nodes, IReadOnlyDictionary<string, string> stringConsts)
    {
        var detections = new List<Detection>();

        foreach (var invocation in nodes.Invocations)
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                continue;

            var methodName = memberAccess.Name.Identifier.ValueText;
            if (!IsServicesChain(memberAccess)) continue;

            var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            // F3 (BUG-BACKLOG #34): Options-pattern config bindings. Runs beside the branches below
            // (the generic Add* branch still records the AddOptions registration itself); what was
            // missing was the SECTION KEY the chained binding call names, which is the fact the
            // config catalog and the missing-defaults insight both need.
            if (TryDetectOptionsBinding(invocation, memberAccess, methodName, stringConsts) is { } binding)
            {
                detections.Add(new OptionsBindingDetection(
                    OptionsType: binding.OptionsType,
                    SectionKey: binding.SectionKey,
                    BindingMethod: binding.BindingMethod)
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                });
            }

            // Batch B (DC3) — the transport client REGISTRATION names its target. This runs beside
            // (not instead of) the branches below: AddHttpClient<TI,TImpl> is still a DI binding (C6),
            // and the generic Add* branch still records the registration itself. What was missing was
            // the type argument + address, which the generic branch throws away (it prefers args[0],
            // i.e. the configuration lambda).
            if (TransportForMethod(methodName) is { } transport
                && memberAccess.Name is GenericNameSyntax clientGeneric
                && clientGeneric.TypeArgumentList.Arguments.Count >= 1)
            {
                // AddHttpClient<TInterface, TImpl>: the CLIENT is the implementation (the interface is
                // the port). Single type arg: that type is the client.
                var typeArgs = clientGeneric.TypeArgumentList.Arguments;
                detections.Add(new TransportClientDetection(
                    Transport: transport,
                    ClientType: typeArgs[^1].ToString(),
                    Address: ExtractConfiguredAddress(invocation))
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                });
            }

            if (LifetimeMethods.Contains(methodName))
            {
                var lifetime = methodName.ToLowerInvariant() switch
                {
                    "addsingleton" => "Singleton",
                    "addscoped" => "Scoped",
                    "addtransient" => "Transient",
                    _ => "Unknown",
                };

                var args = invocation.ArgumentList.Arguments;
                string serviceType;
                string implementationType;
                DiRegistrationShape shape = DiRegistrationShape.DirectBinding;
                string? factorySummary = null;

                if (args.Count >= 2)
                {
                    serviceType = args[0].Expression?.ToString() ?? "?";
                    implementationType = args[1].Expression?.ToString() ?? "?";
                    (shape, factorySummary) = ClassifyShape(args[1].Expression);
                }
                else if (args.Count == 1)
                {
                    serviceType = args[0].Expression?.ToString() ?? "?";
                    implementationType = serviceType;
                    (shape, factorySummary) = ClassifyShape(args[0].Expression);
                }
                else if (memberAccess.Name is GenericNameSyntax genericName)
                {
                    var typeArgs = genericName.TypeArgumentList.Arguments;
                    serviceType = typeArgs.Count >= 1 ? typeArgs[0].ToString() : "?";
                    implementationType = typeArgs.Count >= 2 ? typeArgs[1].ToString() : serviceType;
                    shape = typeArgs.Count >= 2 ? DiRegistrationShape.DirectBinding : DiRegistrationShape.SelfRegistration;
                }
                else
                {
                    serviceType = "?";
                    implementationType = "?";
                }

                var extensions = ExtractExtensionMethods(invocation);

                detections.Add(new DiRegistrationDetection(
                    ServiceType: serviceType,
                    ImplementationType: implementationType,
                    Lifetime: lifetime,
                    ExtensionsUsed: extensions,
                    Shape: shape,
                    FactorySummary: factorySummary)
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                });
            }
            // C6 (Prism D1.2f): AddHttpClient<TInterface, TImpl> is a real interface→impl binding —
            // podcasts' IFeedClient/FeedClient. Without this it fell into the generic Add* branch and
            // the graph only knew the pair through the weaker single-implementor fallback.
            else if (methodName == "AddHttpClient"
                && memberAccess.Name is GenericNameSyntax httpGeneric
                && httpGeneric.TypeArgumentList.Arguments.Count == 2)
            {
                var typeArgs = httpGeneric.TypeArgumentList.Arguments;
                detections.Add(new DiRegistrationDetection(
                    ServiceType: typeArgs[0].ToString(),
                    ImplementationType: typeArgs[1].ToString(),
                    Lifetime: "HttpClient",
                    ExtensionsUsed: ["AddHttpClient"],
                    Shape: DiRegistrationShape.DirectBinding)
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                });
            }
            else if (methodName.StartsWith("Add") && methodName.Length > 3)
            {
                var args = invocation.ArgumentList.Arguments;
                var argTypes = args
                    .Select(a => a.Expression?.ToString() ?? "?")
                    .ToImmutableArray();

                // For generic extensions like AddHostedService<T>, extract T from type arguments
                var implType = argTypes.Length > 0 ? argTypes[0] : "?";
                if (implType == "?" && invocation.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name is GenericNameSyntax genericName)
                {
                    var typeArgs = genericName.TypeArgumentList.Arguments;
                    implType = typeArgs.Count >= 1 ? typeArgs[0].ToString() : "?";
                }

                detections.Add(new DiRegistrationDetection(
                    ServiceType: methodName,
                    ImplementationType: implType,
                    Lifetime: "Extension",
                    ExtensionsUsed: [methodName])
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                });
            }
            else if (methodName.StartsWith("Auto") || methodName.StartsWith("Scan"))
            {
                // Bulk auto-registration patterns: AutoInjectAllServices, Scan, RegisterAssemblyTypes, etc.
                detections.Add(new DiRegistrationDetection(
                    ServiceType: methodName,
                    ImplementationType: "*",
                    Lifetime: "Bulk",
                    ExtensionsUsed: [methodName],
                    Shape: DiRegistrationShape.InlineFactory,
                    FactorySummary: "[bulk auto-registration]")
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                });
            }
        }

        return detections;
    }

    /// <summary>F3 (BUG-BACKLOG #34) — recognises an Options-pattern configuration binding and the
    /// section key it names. Two shapes: <c>AddOptions&lt;T&gt;()</c> followed on the fluent chain by
    /// <c>.BindConfiguration(section)</c> or <c>.Bind(cfg.GetSection(section))</c>, and
    /// <c>Configure&lt;T&gt;(cfg.GetSection(section))</c> (incl. the named-options overload; a lone
    /// string argument to a Configure&lt;T&gt; helper counts too). Returns null when no chained call
    /// binds configuration or when the section argument does not resolve — a computed section is
    /// never guessed (the F2 rule).</summary>
    private static (string OptionsType, string SectionKey, string BindingMethod)? TryDetectOptionsBinding(
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax memberAccess,
        string methodName,
        IReadOnlyDictionary<string, string> stringConsts)
    {
        switch (methodName)
        {
            case "AddOptions" when memberAccess.Name is GenericNameSyntax optionsGeneric
                && optionsGeneric.TypeArgumentList.Arguments.Count >= 1:
            {
                var optionsType = optionsGeneric.TypeArgumentList.Arguments[0].ToString();

                // Walk outward along the fluent chain the AddOptions call anchors.
                SyntaxNode current = invocation;
                while (current.Parent is MemberAccessExpressionSyntax chainAccess
                    && ReferenceEquals(chainAccess.Expression, current)
                    && chainAccess.Parent is InvocationExpressionSyntax chained)
                {
                    var chainedName = chainAccess.Name.Identifier.ValueText;
                    var args = chained.ArgumentList.Arguments;

                    if (chainedName == "BindConfiguration" && args.Count >= 1
                        && ResolveSectionArgument(args[0].Expression, stringConsts) is { } bound)
                        return (optionsType, bound, "BindConfiguration");

                    if (chainedName == "Bind" && args.Count >= 1
                        && SectionFromGetSection(args[0].Expression, stringConsts) is { } section)
                        return (optionsType, section, "Bind");

                    current = chained;
                }
                return null;
            }

            case "Configure" when memberAccess.Name is GenericNameSyntax configureGeneric
                && configureGeneric.TypeArgumentList.Arguments.Count == 1:
            {
                var optionsType = configureGeneric.TypeArgumentList.Arguments[0].ToString();
                var args = invocation.ArgumentList.Arguments;

                // A GetSection argument names the section unambiguously, in any position (the
                // named-options overload puts the name first).
                foreach (var arg in args)
                    if (SectionFromGetSection(arg.Expression, stringConsts) is { } section)
                        return (optionsType, section, "Configure");

                // A LONE string argument can only be the section (a name-first overload has two
                // arguments, so a named-options name never lands here).
                if (args.Count == 1
                    && ResolveSectionArgument(args[0].Expression, stringConsts) is { } lone)
                    return (optionsType, lone, "Configure");

                return null;
            }

            default:
                return null;
        }
    }

    /// <summary>Unwraps <c>cfg.GetSection(section)</c> / <c>GetRequiredSection(section)</c> and
    /// resolves the section argument; null for any other expression.</summary>
    private static string? SectionFromGetSection(
        ExpressionSyntax? expr, IReadOnlyDictionary<string, string> stringConsts)
        => expr is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax ma } call
           && ma.Name.Identifier.ValueText is "GetSection" or "GetRequiredSection"
           && call.ArgumentList.Arguments.Count >= 1
            ? ResolveSectionArgument(call.ArgumentList.Arguments[0].Expression, stringConsts)
            : null;

    /// <summary>Resolves a section-name argument to its string value. A literal answers directly;
    /// <c>nameof(X)</c> is a compile-time constant readable from syntax (the eShop idiom,
    /// <c>BindConfiguration(nameof(BackgroundTaskOptions))</c>); a const reference resolves through
    /// the project-wide const index — bare (<c>BindConfiguration(SectionName)</c>, the enclosing
    /// type's own const) or qualified (<c>QueueDrainOptions.SectionName</c>). Returns null for a
    /// genuine expression: a computed section is never guessed. Mirrors
    /// <c>EndpointExtractor.ResolveGroupPrefixArgument</c> (F2), which set the rule.</summary>
    private static string? ResolveSectionArgument(
        ExpressionSyntax? expression, IReadOnlyDictionary<string, string> stringConsts)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax { Token.Value: string literal }:
                return literal;

            // nameof(X) / nameof(Ns.X) — the value is the final identifier's text, by definition.
            case InvocationExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" }
            } nameofCall when nameofCall.ArgumentList.Arguments.Count == 1:
                return nameofCall.ArgumentList.Arguments[0].Expression switch
                {
                    IdentifierNameSyntax id => id.Identifier.ValueText,
                    MemberAccessExpressionSyntax nested => nested.Name.Identifier.ValueText,
                    _ => null,
                };

            // BindConfiguration(SectionName) — a const on the type the call is written in.
            case IdentifierNameSyntax bare:
            {
                var owner = expression.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                return owner is not null
                    && stringConsts.TryGetValue(
                        $"{owner.Identifier.ValueText}.{bare.Identifier.ValueText}", out var own)
                    ? own
                    : null;
            }

            // BindConfiguration(QueueDrainOptions.SectionName) — the index's own key shape.
            case MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax type, Name: { } member }:
                return stringConsts.TryGetValue(
                    $"{type.Identifier.ValueText}.{member.Identifier.ValueText}", out var qualified)
                    ? qualified
                    : null;

            default:
                return null;
        }
    }

    /// <summary>Batch B — maps a client-registration method to the transport it configures, or null
    /// when the method registers no transport.</summary>
    private static string? TransportForMethod(string methodName) => methodName switch
    {
        "AddGrpcClient" => TransportKinds.Grpc,
        "AddHttpClient" => TransportKinds.Http,
        "AddRefitClient" => TransportKinds.Refit,
        _ => null,
    };

    /// <summary>Batch B — finds the address a client registration was configured with. The literal can
    /// sit in the registration's own configuration lambda
    /// (<c>o =&gt; o.Address = new("http://basket-api")</c>) or on a chained builder call
    /// (<c>.ConfigureHttpClient(c =&gt; c.BaseAddress = ...)</c>), so the whole fluent chain is scanned.
    /// Only scheme-bearing literals count — that skips the version/policy strings those same chains
    /// carry. Returns null when the address comes from configuration rather than a literal.</summary>
    private static string? ExtractConfiguredAddress(InvocationExpressionSyntax invocation)
    {
        SyntaxNode outermost = invocation;
        while (outermost.Parent is MemberAccessExpressionSyntax chainAccess
            && ReferenceEquals(chainAccess.Expression, outermost)
            && chainAccess.Parent is InvocationExpressionSyntax chained)
        {
            outermost = chained;
        }

        foreach (var literal in outermost.DescendantNodes().OfType<LiteralExpressionSyntax>())
        {
            if (literal.Token.Value is string text && text.Contains("://", StringComparison.Ordinal))
                return text;
        }
        return null;
    }

    private static bool IsServicesChain(MemberAccessExpressionSyntax memberAccess)
    {
        var expr = memberAccess.Expression;

        while (expr is InvocationExpressionSyntax innerInvocation)
        {
            if (innerInvocation.Expression is MemberAccessExpressionSyntax innerAccess)
            {
                expr = innerAccess.Expression;
            }
            else
            {
                return false;
            }
        }

        var target = expr.ToString();
        return string.Equals(target, "Services", StringComparison.OrdinalIgnoreCase)
            || target.EndsWith(".Services", StringComparison.OrdinalIgnoreCase)
            || target.EndsWith(".services", StringComparison.Ordinal);
    }

    private static ImmutableArray<string> ExtractExtensionMethods(InvocationExpressionSyntax invocation)
    {
        var extensions = new List<string>();
        var current = invocation;

        while (current.Expression is MemberAccessExpressionSyntax ma)
        {
            if (ma.Name.Identifier.ValueText != "AddSingleton" &&
                ma.Name.Identifier.ValueText != "AddScoped" &&
                ma.Name.Identifier.ValueText != "AddTransient" &&
                ma.Name.Identifier.ValueText.StartsWith("Add"))
            {
                extensions.Add(ma.Name.Identifier.ValueText);
            }

            if (ma.Expression is InvocationExpressionSyntax inner)
            {
                current = inner;
            }
            else
            {
                break;
            }
        }

        return extensions.ToImmutableArray();
    }

    private static (DiRegistrationShape Shape, string? Summary) ClassifyShape(ExpressionSyntax? expr)
    {
        if (expr is not LambdaExpressionSyntax lambda)
            return (DiRegistrationShape.DirectBinding, null);

        var body = lambda.Body;

        // Expression body: sp => sp.GetRequiredService<T>()
        if (body is ExpressionSyntax exprBody)
        {
            if (exprBody is InvocationExpressionSyntax inv
                && inv.Expression is MemberAccessExpressionSyntax ma
                && (ma.Name.Identifier.ValueText == "GetRequiredService"
                    || ma.Name.Identifier.ValueText == "GetService"))
            {
                return (DiRegistrationShape.ForwardingAlias,
                    $"alias → {ma.Name.Identifier.ValueText}");
            }

            var summary = BuildFactorySummary(exprBody);
            return (DiRegistrationShape.InlineFactory, summary);
        }

        // Block body: sp => { ... }
        if (body is BlockSyntax block)
        {
            var summary = BuildFactorySummary(block);
            return (DiRegistrationShape.InlineFactory, summary);
        }

        return (DiRegistrationShape.InlineFactory, "[factory]");
    }

    private static string? BuildFactorySummary(SyntaxNode body)
    {
        // Check if body itself is an object creation
        if (body is ObjectCreationExpressionSyntax bodyCreation)
        {
            var typeName = bodyCreation.Type.ToString();
            return $"[factory: new {typeName}]";
        }

        // Walk descendants for object creations
        foreach (var creation in body.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            var typeName = creation.Type.ToString();
            var deps = creation.ArgumentList?.Arguments
                .Select(a => a.Expression.ToString())
                .Where(s => !s.StartsWith("sp.") && !s.StartsWith("\""))
                .ToList() ?? [];

            if (deps.Count > 0)
                return $"[factory: new {typeName}({string.Join(", ", deps.Take(3))})]";

            return $"[factory: new {typeName}]";
        }

        // Detect File.ReadAllText, File.Exists patterns
        if (body.ToString().Contains("File.ReadAllText") || body.ToString().Contains("File.Exists"))
            return "[factory: reads from disk]";

        // Detect foreach bulk registration
        if (body.ToString().Contains("foreach"))
            return "[factory: bulk registration]";

        return "[factory]";
    }
}
