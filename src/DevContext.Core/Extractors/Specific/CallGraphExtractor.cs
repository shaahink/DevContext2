using System.Collections.Concurrent;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Walks syntax trees to build a BFS-depth-limited call graph for Debug and Full extraction profiles.</summary>
[ExtractorOrder(30)]
public sealed class CallGraphExtractor : IDiscoveryExtractor
{
    /// <summary>The runtime's reference assemblies, loaded once. Used to give the semantic compilation a
    /// reference set so BCL/framework symbols bind; intra-solution types resolve from source regardless.</summary>
    private static readonly Lazy<ImmutableArray<MetadataReference>> ReferenceAssemblies = new(() =>
    {
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrEmpty(tpa)) return [];
        var refs = ImmutableArray.CreateBuilder<MetadataReference>();
        foreach (var path in tpa.Split(System.IO.Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) continue;
            try { refs.Add(MetadataReference.CreateFromFile(path)); }
            catch { /* skip unreadable assembly */ }
        }
        return refs.ToImmutable();
    });

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "CallGraphExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Deep;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], ["call-graph"],
        ["model.CallEdges", "context.Analysis.CallGraph"],
        "Walks syntax trees using Roslyn to build a BFS-depth-limited call graph");
    /// <summary>Only runs in Debug or Full extraction profile.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.Options.Profile is ExtractionProfile.Debug or ExtractionProfile.Full;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var maxDepth = context.ActiveScenario.Pruning.MaxCallDepth;
        var (diMap, interfaceImplMap, fqnMap, fqnCollisions) = BuildResolutionMaps(model);
        var trees = await CollectSyntaxTreesAsync(context, model, Name, ct).ConfigureAwait(false);
        var compilation = CreateCompilation(trees, model, Name);

        var allEdges = new ConcurrentBag<CallEdge>();
        var semanticEdges = await ResolveCallEdgesAsync(trees, compilation, diMap, interfaceImplMap, fqnMap, fqnCollisions, allEdges, ct).ConfigureAwait(false);

        var includedEdges = FilterEdgesByBfsDepth(allEdges, context, model, maxDepth, ct);
        EmitCallGraphAndDiagnostic(includedEdges, context, model, compilation, semanticEdges, maxDepth, Name, ct);
    }

    private static (
        Dictionary<string, string> DiMap,
        Dictionary<string, string> InterfaceImplMap,
        Dictionary<string, string> FqnMap,
        Dictionary<string, List<string>> FqnCollisions)
        BuildResolutionMaps(DiscoveryModel model)
    {
        // Build DI resolution map: interface/abstract type → concrete implementation
        // Key: short type name, Value: short implementation name
        var diMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!string.IsNullOrEmpty(di.ImplementationType)
                && !string.Equals(di.ImplementationType, "?"
, StringComparison.Ordinal) && !di.ImplementationType.StartsWith("sp =>", StringComparison.Ordinal)
                && !di.ImplementationType.StartsWith("_ =>", StringComparison.Ordinal)
                && !di.ImplementationType.StartsWith('(')
                && !di.ImplementationType.Contains("GetRequiredService", StringComparison.Ordinal))
            {
                var svcShort = StripGenerics(di.ServiceType);
                var implShort = StripGenerics(di.ImplementationType);
                diMap[svcShort] = implShort;
            }
        }

        // Build interface→implementation map from model.Types (fallback when DI map misses)
        // and short-name → fully-qualified-name map (handles collisions by namespace-prefix preference)
        var interfaceImplMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var fqnMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var fqnCollisions = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var kv in model.Types)
        {
            var type = kv.Value;

            // Build fqnMap: first add wins, duplicates go to collision list
            if (!fqnMap.ContainsKey(type.Name))
                fqnMap[type.Name] = kv.Key;
            else
            {
                if (!fqnCollisions.ContainsKey(type.Name))
                    fqnCollisions[type.Name] = [fqnMap[type.Name]];
                fqnCollisions[type.Name].Add(kv.Key);
            }

            // Build interfaceImplMap
            if (type.ImplementedInterfaces is { Length: > 0 })
            {
                foreach (var iface in type.ImplementedInterfaces)
                {
                    var ifaceShort = StripGenerics(iface);
                    if (!interfaceImplMap.ContainsKey(ifaceShort))
                        interfaceImplMap[ifaceShort] = type.Name;
                }
            }
        }

        return (diMap, interfaceImplMap, fqnMap, fqnCollisions);
    }

    private static async Task<List<(string Path, SyntaxTree Tree)>> CollectSyntaxTreesAsync(
        DiscoveryContext context, DiscoveryModel model, string name, CancellationToken ct)
    {
        // Pass 1: collect every parsed tree so one compilation can see the whole solution.
        var trees = new List<(string Path, SyntaxTree Tree)>();
        await foreach (var filePath in ExtractorHelpers.EnumerateSourceFilesAsync(context, ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            try { trees.Add((filePath, await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false))); }
            catch { model.AddDiagnostic(DiagnosticLevel.Warning, name, $"Failed to parse {filePath}"); }
        }
        return trees;
    }

    private static CSharpCompilation? CreateCompilation(
        List<(string Path, SyntaxTree Tree)> trees, DiscoveryModel model, string name)
    {
        // Build a best-effort semantic compilation. Source types always bind; external package types
        // may be error types, but intra-solution receiver resolution — the calls a trace follows — works
        // regardless. Any failure degrades cleanly to the syntactic field/DI-map heuristic.
        CSharpCompilation? compilation = null;
        try
        {
            compilation = CSharpCompilation.Create("DevContextSemantics",
                trees.Select(t => t.Tree),
                ReferenceAssemblies.Value,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }
        catch (Exception ex)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, name,
                $"Semantic compilation unavailable; using syntactic resolution ({ex.GetType().Name}).");
        }
        return compilation;
    }

    private static async Task<int> ResolveCallEdgesAsync(
        List<(string Path, SyntaxTree Tree)> trees,
        CSharpCompilation? compilation,
        Dictionary<string, string> diMap,
        Dictionary<string, string> interfaceImplMap,
        Dictionary<string, string> fqnMap,
        Dictionary<string, List<string>> fqnCollisions,
        ConcurrentBag<CallEdge> allEdges,
        CancellationToken ct)
    {
        // Pass 2: resolve call edges, preferring the SemanticModel and falling back to syntax.
        var semanticEdges = 0;
        foreach (var (filePath, syntaxTree) in trees)
        {
            ct.ThrowIfCancellationRequested();
            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            SemanticModel? semanticModel = null;
            if (compilation is not null)
            {
                try { semanticModel = compilation.GetSemanticModel(syntaxTree); }
                catch { semanticModel = null; }
            }

            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var callerType = RoslynSyntaxHelpers.GetTypeFullName(typeDecl);
                if (string.IsNullOrEmpty(callerType)) continue;

                var fieldMap = BuildFieldMap(typeDecl, diMap);

                foreach (var member in typeDecl.Members)
                {
                    var callerMethod = member switch
                    {
                        MethodDeclarationSyntax m => m.Identifier.ValueText,
                        ConstructorDeclarationSyntax c => c.Identifier.ValueText,
                        _ => null,
                    };
                    if (callerMethod is null) continue;

                    foreach (var invocation in member.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        var (calleeType, calleeMethod, resolution) = ResolveCalleeSmart(
                            invocation, semanticModel, callerType, fieldMap,
                            diMap, interfaceImplMap, fqnMap, fqnCollisions);
                        if (resolution == Graph.Resolution.Semantic) semanticEdges++;

                        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        allEdges.Add(new CallEdge(
                            callerType, callerMethod, calleeType, calleeMethod, $"{filePath}:{lineNumber}")
                        {
                            Resolution = resolution,
                        });
                    }
                }
            }
        }
        return semanticEdges;
    }

    private static List<CallEdge> FilterEdgesByBfsDepth(
        ConcurrentBag<CallEdge> allEdges, DiscoveryContext context, DiscoveryModel model,
        int maxDepth, CancellationToken ct)
    {
        var adjacency = new Dictionary<string, List<CallEdge>>(StringComparer.Ordinal);
        foreach (var edge in allEdges)
        {
            var key = $"{edge.CallerType}.{edge.CallerMethod}";
            if (!adjacency.TryGetValue(key, out var list))
            {
                list = [];
                adjacency[key] = list;
            }
            list.Add(edge);
        }

        var startKeys = GetStartKeys(context, model);
        var bfsDepth = new Dictionary<string, int>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        var includedEdges = new List<CallEdge>();

        foreach (var key in startKeys)
        {
            if (bfsDepth.TryAdd(key, 0))
                queue.Enqueue(key);
        }

        while (queue.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var currentKey = queue.Dequeue();
            var depth = bfsDepth[currentKey];
            if (depth >= maxDepth) continue;

            if (!adjacency.TryGetValue(currentKey, out var edges)) continue;

            foreach (var edge in edges)
            {
                includedEdges.Add(edge);
                var calleeKey = $"{edge.CalleeType}.{edge.CalleeMethod}";
                if (bfsDepth.TryAdd(calleeKey, depth + 1))
                    queue.Enqueue(calleeKey);
            }
        }
        return includedEdges;
    }

    private static void EmitCallGraphAndDiagnostic(
        List<CallEdge> includedEdges, DiscoveryContext context, DiscoveryModel model,
        CSharpCompilation? compilation, int semanticEdges, int maxDepth, string name, CancellationToken ct)
    {
        foreach (var edge in includedEdges)
            model.CallEdges.Add(edge);

        var callGraphAdj = new Dictionary<string, ImmutableArray<CallEdge>>(StringComparer.Ordinal);
        foreach (var edge in includedEdges)
        {
            ct.ThrowIfCancellationRequested();
            var key = $"{edge.CallerType}.{edge.CallerMethod}";
            if (!callGraphAdj.TryGetValue(key, out var existing))
            {
                callGraphAdj[key] = [edge];
            }
            else
            {
                callGraphAdj[key] = existing.Add(edge);
            }
        }

        context.Analysis.CallGraph = new CallGraph(callGraphAdj);

        var resolver = compilation is not null ? $"semantic ({semanticEdges} verified)" : "syntactic";
        model.AddDiagnostic(DiagnosticLevel.Info, name,
            $"Built call graph: {includedEdges.Count} edges at depth ≤ {maxDepth}; resolver: {resolver}");
    }

    private static HashSet<string> GetStartKeys(DiscoveryContext context, DiscoveryModel model)
    {
        var startKeys = new HashSet<string>(StringComparer.Ordinal);

        if (context.Analysis.FocusPoints.Count > 0)
        {
            var focusFiles = context.Analysis.FocusPoints
                .Select(f => f.FilePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var type in model.Types.Values)
            {
                if (focusFiles.Contains(type.FilePath))
                {
                    foreach (var method in type.Methods)
                        startKeys.Add($"{type.Id}.{method.Name}");
                }
            }
        }

        if (startKeys.Count == 0)
        {
            foreach (var type in model.Types.Values)
            {
                foreach (var method in type.Methods)
                    startKeys.Add($"{type.Id}.{method.Name}");
            }
        }

        return startKeys;
    }

    private static Dictionary<string, string> BuildFieldMap(TypeDeclarationSyntax typeDecl, Dictionary<string, string> diMap)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        // Single pass over members to collect fields, properties, constructors, and method params
        foreach (var member in typeDecl.Members)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    foreach (var variable in field.Declaration.Variables)
                        map[variable.Identifier.ValueText] = field.Declaration.Type.ToString();
                    break;
                case PropertyDeclarationSyntax prop:
                    map[prop.Identifier.ValueText] = prop.Type.ToString();
                    break;
                case ConstructorDeclarationSyntax ctor:
                    foreach (var param in ctor.ParameterList.Parameters)
                    {
                        var ctorParamType = param.Type?.ToString();
                        if (ctorParamType is not null)
                            map[param.Identifier.ValueText] = ctorParamType;
                    }
                    break;
                case MethodDeclarationSyntax method:
                    foreach (var param in method.ParameterList.Parameters)
                    {
                        var methodParamType = param.Type?.ToString();
                        if (methodParamType is not null)
                            map.TryAdd(param.Identifier.ValueText, methodParamType);
                    }
                    break;
            }
        }

        // Add primary constructor parameters (C# 12 class/record)
        if (typeDecl.ParameterList is not null)
        {
            foreach (var param in typeDecl.ParameterList.Parameters)
            {
                var paramType = param.Type?.ToString();
                if (paramType is not null && !map.ContainsKey(param.Identifier.ValueText))
                    map[param.Identifier.ValueText] = paramType;
            }
        }

        // Lambda parameters with explicit types — minimal-API handlers register inline lambdas
        // like `(TodoDbContext db, CurrentUser owner) => ...`, where `db`/`owner` are the receivers
        // of the calls we want to follow. Without these, every minimal-API trace dead-ends at the
        // endpoint type. Fields/ctor params are authoritative, so only fill gaps (TryAdd).
        foreach (var lambda in typeDecl.DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>())
        {
            foreach (var param in lambda.ParameterList.Parameters)
            {
                var paramType = param.Type?.ToString();
                if (paramType is not null)
                    map.TryAdd(param.Identifier.ValueText, paramType);
            }
        }

        return map;
    }

    /// <summary>Resolves an invocation's callee, preferring Roslyn's SemanticModel (the receiver's real
    /// static type) and falling back to the syntactic field/DI-map heuristic. Semantic hits are tagged
    /// so the trace can show [verified] vs [approx].</summary>
    private static (string Type, string Method, Graph.Resolution Resolution) ResolveCalleeSmart(
        InvocationExpressionSyntax invocation, SemanticModel? model, string callerType,
        Dictionary<string, string> fieldMap, Dictionary<string, string> diMap,
        Dictionary<string, string> interfaceImplMap, Dictionary<string, string> fqnMap,
        Dictionary<string, List<string>> fqnCollisions)
    {
        if (model is not null)
        {
            try
            {
                var info = model.GetSymbolInfo(invocation);
                var method = (info.Symbol ?? info.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
                if (method?.ReceiverType is { } recv)
                {
                    var mapped = MapSemanticReceiver(recv, diMap, interfaceImplMap, fqnMap);
                    if (mapped is not null)
                        return (mapped, method.Name, Graph.Resolution.Semantic);
                }
                else if (invocation.Expression is MemberAccessExpressionSyntax ma
                    && model.GetTypeInfo(ma.Expression).Type is { } recvType)
                {
                    // The method didn't bind (external base member, no package ref) but the receiver's
                    // declared type often resolves from source — that's the type the trace follows.
                    var mapped = MapSemanticReceiver(recvType, diMap, interfaceImplMap, fqnMap);
                    if (mapped is not null)
                        return (mapped, ma.Name.Identifier.ValueText, Graph.Resolution.Semantic);
                }
            }
            catch { /* fall through to syntactic */ }
        }

        var (type, syntacticMethod) = ResolveCallee(invocation, callerType, fieldMap, diMap, interfaceImplMap, fqnMap, fqnCollisions);
        return (type, syntacticMethod, Graph.Resolution.Syntactic);
    }

    /// <summary>Maps a semantically-resolved receiver type to the solution type a trace should descend
    /// into: interfaces/abstracts route to their concrete impl (DI registration, else sole implementor);
    /// concrete source types pass through; external/framework receivers return null (dropped — the join
    /// seams cover meaningful external indirection like MediatR dispatch and EF entity access).</summary>
    private static string? MapSemanticReceiver(ITypeSymbol recv,
        Dictionary<string, string> diMap, Dictionary<string, string> interfaceImplMap,
        Dictionary<string, string> fqnMap)
    {
        var shortName = recv.Name;
        if (string.IsNullOrEmpty(shortName)) return null;

        // Interface/abstract → impl. Works even when recv is an unresolved error type (missing package
        // reference): only its name is needed, and the impl is a real solution type.
        if (diMap.TryGetValue(shortName, out var impl) || interfaceImplMap.TryGetValue(shortName, out impl))
            return fqnMap.TryGetValue(impl, out var implFqn) ? implFqn : impl;

        // Concrete solution type — keep it (the verified internal call).
        if (recv is INamedTypeSymbol named && named.Locations.Any(l => l.IsInSource))
            return FqnOf(named);

        return null;
    }

    /// <summary>Builds the "Namespace.Name" form that matches <see cref="RoslynSyntaxHelpers.GetTypeFullName"/> / TypeDiscovery.Id.</summary>
    private static string FqnOf(INamedTypeSymbol t)
    {
        var ns = t.ContainingNamespace is { IsGlobalNamespace: false } n ? n.ToDisplayString() : null;
        return ns is null ? t.Name : $"{ns}.{t.Name}";
    }

    private static (string Type, string Method) ResolveCallee(InvocationExpressionSyntax invocation,
        string callerType, Dictionary<string, string> fieldMap, Dictionary<string, string> diMap,
        Dictionary<string, string> interfaceImplMap, Dictionary<string, string> fqnMap,
        Dictionary<string, List<string>> fqnCollisions)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.ValueText;
            var target = memberAccess.Expression.ToString();

            // Handle chained calls: db.Todos.Where(...) — walk to root identifier
            if (memberAccess.Expression is MemberAccessExpressionSyntax chained)
            {
                var rootId = WalkToRootIdentifier(chained);
                if (!string.IsNullOrEmpty(rootId))
                {
                    var resolved = ResolveType(rootId, callerType, fieldMap, diMap, interfaceImplMap, fqnMap, fqnCollisions);
                    return (resolved, methodName);
                }
            }

            var resolvedSimple = ResolveType(target, callerType, fieldMap, diMap, interfaceImplMap, fqnMap, fqnCollisions);
            return (resolvedSimple, methodName);
        }

        if (invocation.Expression is IdentifierNameSyntax simpleName)
        {
            var resolved = ResolveType("this", callerType, fieldMap, diMap, interfaceImplMap, fqnMap, fqnCollisions);
            return (resolved, simpleName.Identifier.ValueText);
        }

        return ("unknown", invocation.Expression?.ToString() ?? "?");
    }

    /// <summary>Walks a member access chain to find the root identifier.
    /// e.g. 'db.Todos.Where(predicate)' → root 'db', or 'this._repo.Orders' → root '_repo'.</summary>
    private static string? WalkToRootIdentifier(ExpressionSyntax expr)
    {
        while (expr is MemberAccessExpressionSyntax ma)
        {
            expr = ma.Expression;
        }
        return expr switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            ThisExpressionSyntax => "this",
            _ => expr.ToString()
        };
    }

    private static string ResolveType(string target, string callerType, Dictionary<string, string> fieldMap,
        Dictionary<string, string> diMap, Dictionary<string, string> interfaceImplMap,
        Dictionary<string, string> fqnMap, Dictionary<string, List<string>> fqnCollisions)
    {
        if (target is "this" or "base")
            return callerType;

        var fieldName = target.StartsWith("this.", StringComparison.Ordinal)
            ? target["this.".Length..]
            : target;

        if (fieldMap.TryGetValue(fieldName, out var declaredType))
        {
            var shortType = StripGenerics(declaredType);

            if (diMap.TryGetValue(shortType, out var impl))
                shortType = impl;
            else if (interfaceImplMap.TryGetValue(shortType, out var impl2))
                shortType = impl2;

            // Resolve to FQN; prefer caller's namespace on collision
            if (fqnMap.TryGetValue(shortType, out var fqn))
            {
                if (fqnCollisions.TryGetValue(shortType, out var collisions))
                {
                    // Prefer type whose namespace matches the caller's
                    var callerNs = callerType is not null && callerType.LastIndexOf('.') > 0
                        ? callerType[..callerType.LastIndexOf('.')]
                        : null;
                    var match = collisions.FirstOrDefault(c => callerNs is not null && c.StartsWith(callerNs + ".", StringComparison.Ordinal));
                    return match ?? fqn;
                }
                return fqn;
            }

            return shortType;
        }

        return target;
    }

    private static string StripGenerics(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx > 0 ? typeName[..idx].TrimEnd() : typeName.TrimEnd();
    }
}
