using System.Collections.Concurrent;
using System.Diagnostics;

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
    /// <summary>Runs when the full graph is built (default) or in Debug/Full. This is the expensive
    /// pass (semantic resolution over a CSharpCompilation); the <c>--lite</c> opt-out turns it off via
    /// BuildFullGraph=false, leaving Calls edges unresolved (Map + indirection seams still work).</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.Options.BuildFullGraph
            || context.Options.Profile is ExtractionProfile.Debug or ExtractionProfile.Full;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var maxDepth = context.ActiveScenario.Pruning.MaxCallDepth;
        var allEdges = new ConcurrentBag<CallEdge>();

        // Build DI resolution map: interface/abstract type → concrete implementation
        // Key: short type name, Value: short implementation name
        var diMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // Stable order: model.Detections is a ConcurrentBag filled by several parallel extractors, so its
        // enumeration order is nondeterministic and this map is "last write wins" by service short-name.
        // Order by source location so the winning implementation — and the resolved edges — are stable.
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>()
            .OrderBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber))
        {
            if (!string.IsNullOrEmpty(di.ImplementationType)
                && di.ImplementationType != "?"
                && !di.ImplementationType.StartsWith("sp =>")
                && !di.ImplementationType.StartsWith("_ =>")
                && !di.ImplementationType.StartsWith("(")
                && !di.ImplementationType.Contains("GetRequiredService"))
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
        // Iterate in a STABLE order: model.Types is a ConcurrentDictionary whose enumeration order is
        // nondeterministic, and these maps resolve short-name collisions "first wins". Ordering by FQN
        // makes the winner — and therefore the resolved call edges — deterministic across runs.
        foreach (var kv in model.Types.OrderBy(kv => kv.Key, StringComparer.Ordinal))
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

        // Pass 1: collect every parsed tree so one compilation can see the whole solution.
        var swParse = Stopwatch.StartNew();
        var trees = new List<(string Path, SyntaxTree Tree)>();
        await foreach (var filePath in ExtractorHelpers.EnumerateSourceFilesAsync(context, ct))
        {
            ct.ThrowIfCancellationRequested();
            try { trees.Add((filePath, await context.Cache.GetSyntaxTreeAsync(filePath, ct))); }
            catch { model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}"); }
        }
        swParse.Stop();

        // Build a best-effort semantic compilation. Source types always bind; external package types
        // may be error types, but intra-solution receiver resolution — the calls a trace follows — works
        // regardless. Any failure degrades cleanly to the syntactic field/DI-map heuristic.
        var swCompile = Stopwatch.StartNew();
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
            model.AddDiagnostic(DiagnosticLevel.Info, Name,
                $"Semantic compilation unavailable; using syntactic resolution ({ex.GetType().Name}).");
        }
        swCompile.Stop();

        // Pass 2: resolve call edges, preferring the SemanticModel and falling back to syntax.
        // Parallel across files — semantic binding (the dominant cost, measured) is CPU-bound and
        // Roslyn's GetSemanticModel is safe to call concurrently on one Compilation; edges land in a
        // ConcurrentBag. (perf P2: parallelized the bind loop.)
        var swBind = Stopwatch.StartNew();
        var semanticEdges = 0;
        var parallelOpts = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount };

        // Binds one file: resolves every invocation to a call edge; reports discovered callee types
        // (for focus-scoped frontier expansion). Pure reads of the shared maps → thread-safe.
        void BindOne(string filePath, SyntaxTree syntaxTree, ConcurrentBag<string>? calleesOut)
        {
            var root = syntaxTree.GetRoot(ct);

            SemanticModel? semanticModel = null;
            if (compilation is not null)
            {
                try { semanticModel = compilation.GetSemanticModel(syntaxTree); }
                catch { semanticModel = null; }
            }

            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var callerType = GetTypeFullName(typeDecl);
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
                        if (resolution == Graph.Resolution.Semantic) Interlocked.Increment(ref semanticEdges);

                        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        allEdges.Add(new CallEdge(
                            callerType, callerMethod, calleeType, calleeMethod, $"{filePath}:{lineNumber}")
                        {
                            Resolution = resolution,
                        });
                        calleesOut?.Add(calleeType);
                    }
                }
            }
        }

        // Focus-scoped binding (perf P1): a focused trace only needs files reachable from the focus, so
        // seed from the focus type/route and bind the reachable frontier round-by-round (≤ maxDepth call
        // hops). Falls back to a full parallel bind when there's no focus or the seed can't be resolved —
        // correctness-preserving: the downstream BFS prunes by depth either way.
        var treeByPath = new Dictionary<string, SyntaxTree>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in trees) treeByPath[t.Path] = t.Tree;

        var seedFiles = ResolveFocusSeedFiles(context, model);
        if (context.Analysis.FocusPoints.Count > 0 && seedFiles.Count > 0)
        {
            var typeToFile = BuildTypeToFile(model);
            var bound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var frontier = new HashSet<string>(seedFiles, StringComparer.OrdinalIgnoreCase);

            // The trace doesn't only follow CALL edges — it crosses Send→Handler / scheduled-job seams.
            // A focused trace from an endpoint reaches its MediatR handler (and everything the handler
            // calls — e.g. eShop Order → IntegrationEventLogEF) via a Send seam, not a call. So seed the
            // closure with those seam-landing type files too; otherwise their call edges are never bound
            // and the trace silently truncates. Bounded (handlers/jobs are a small fraction of files).
            foreach (var f in SeamLandingFiles(model, typeToFile)) frontier.Add(f);

            // Bind the FULL transitive closure of files reachable from the focus (not just maxDepth call
            // hops): the trace can walk to its own depth (≤10) and includes deep cross-project seams, so
            // binding too shallow silently drops them (e.g. eShop Order → IntegrationEventLogEF). The
            // closure from one entry is small even in a large repo; the cap only guards pathological fan-out.
            const int MaxRounds = 16;
            for (var round = 0; round < MaxRounds && frontier.Count > 0; round++)
            {
                var toBind = frontier.Where(f => !bound.Contains(f) && treeByPath.ContainsKey(f)).ToList();
                foreach (var f in toBind) bound.Add(f);

                var callees = new ConcurrentBag<string>();
                Parallel.ForEach(toBind, parallelOpts, f => BindOne(f, treeByPath[f], callees));

                var next = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in callees)
                    if (typeToFile.TryGetValue(c, out var cf) && !bound.Contains(cf))
                        next.Add(cf);
                frontier = next;
            }

            model.AddDiagnostic(DiagnosticLevel.Info, Name,
                $"Focus-scoped call graph: bound {bound.Count} of {trees.Count} files from the focus seed.");
        }
        else
        {
            Parallel.ForEach(trees, parallelOpts, t => BindOne(t.Path, t.Tree, null));
        }
        swBind.Stop();

        var swBfs = Stopwatch.StartNew();
        var adjacency = new Dictionary<string, List<CallEdge>>();
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
        var bfsDepth = new Dictionary<string, int>();
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

        foreach (var edge in includedEdges)
            model.CallEdges.Add(edge);

        var callGraphAdj = new Dictionary<string, ImmutableArray<CallEdge>>();
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

        swBfs.Stop();
        var resolver = compilation is not null ? $"semantic ({semanticEdges} verified)" : "syntactic";
        model.AddDiagnostic(DiagnosticLevel.Info, Name,
            $"Built call graph: {includedEdges.Count} edges at depth ≤ {maxDepth}; resolver: {resolver}; "
            + $"phases: parse {swParse.ElapsedMilliseconds}ms · compile {swCompile.ElapsedMilliseconds}ms · "
            + $"bind {swBind.ElapsedMilliseconds}ms · bfs {swBfs.ElapsedMilliseconds}ms ({trees.Count} files)");
    }

    /// <summary>Files the focus points to — the seed for focus-scoped binding (perf P1). Type/Method
    /// focus → the declaring type's file(s); Endpoint focus → the matching endpoint's file + handler
    /// type file. Empty when the focus can't be tied to a source file (→ full bind fallback).</summary>
    private static HashSet<string> ResolveFocusSeedFiles(DiscoveryContext context, DiscoveryModel model)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fp in context.Analysis.FocusPoints)
        {
            if (!string.IsNullOrEmpty(fp.TypeName))
            {
                foreach (var t in model.Types.Values)
                    if ((string.Equals(t.Name, fp.TypeName, StringComparison.OrdinalIgnoreCase)
                            || t.Id.EndsWith("." + fp.TypeName, StringComparison.OrdinalIgnoreCase))
                        && !string.IsNullOrEmpty(t.FilePath))
                        files.Add(t.FilePath);
            }

            if (fp.Kind == FocusKind.Endpoint && !string.IsNullOrEmpty(fp.Route))
            {
                foreach (var ep in model.Detections.OfType<EndpointDetection>())
                {
                    if (!RouteMatches(ep, fp)) continue;
                    if (!string.IsNullOrEmpty(ep.SourceFile)) files.Add(ep.SourceFile);
                    foreach (var t in model.Types.Values)
                        if (string.Equals(t.Name, ep.HandlerType, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(t.FilePath))
                            files.Add(t.FilePath);
                }
            }
        }
        return files;
    }

    private static bool RouteMatches(EndpointDetection ep, FocusPoint fp)
    {
        static string Norm(string r) => "/" + r.Trim('/');
        var routeOk = string.Equals(Norm(ep.RouteTemplate), Norm(fp.Route!), StringComparison.OrdinalIgnoreCase);
        var verbOk = string.IsNullOrEmpty(fp.HttpMethod)
            || string.Equals(ep.HttpMethod, fp.HttpMethod, StringComparison.OrdinalIgnoreCase);
        return routeOk && verbOk;
    }

    /// <summary>Maps every type's FQN (Id) and short name to its source file, for frontier expansion.</summary>
    private static Dictionary<string, string> BuildTypeToFile(DiscoveryModel model)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // Stable order (model.Types enumeration is nondeterministic) so the short-name "first wins"
        // TryAdd below picks the same file each run → deterministic focus-closure frontier expansion.
        foreach (var kv in model.Types.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(kv.Value.FilePath)) continue;
            map[kv.Key] = kv.Value.FilePath;
            map.TryAdd(kv.Value.Name, kv.Value.FilePath);
        }
        return map;
    }

    /// <summary>Files of types a trace can jump to across a non-call seam (MediatR handlers, scheduled
    /// jobs / hosted services). Seeding the focus closure with these ensures their call edges are bound,
    /// so a send→handler→… chain (incl. cross-project) isn't silently truncated by focus-scoping.</summary>
    private static HashSet<string> SeamLandingFiles(DiscoveryModel model, Dictionary<string, string> typeToFile)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Add(string? typeName)
        {
            if (!string.IsNullOrEmpty(typeName) && typeToFile.TryGetValue(StripGenerics(typeName), out var f))
                files.Add(f);
        }
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>()) Add(h.HandlerType);
        foreach (var w in model.Detections.OfType<BackgroundWorkerDetection>()) Add(w.ImplementationType);
        return files;
    }

    private static HashSet<string> GetStartKeys(DiscoveryContext context, DiscoveryModel model)
    {
        var startKeys = new HashSet<string>();

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

        foreach (var field in typeDecl.Members.OfType<FieldDeclarationSyntax>())
        {
            var fieldType = field.Declaration.Type.ToString();
            foreach (var variable in field.Declaration.Variables)
            {
                map[variable.Identifier.ValueText] = fieldType;
            }
        }

        foreach (var prop in typeDecl.Members.OfType<PropertyDeclarationSyntax>())
        {
            var propType = prop.Type.ToString();
            map[prop.Identifier.ValueText] = propType;
        }

        // Add constructor parameters (they behave like fields for resolution)
        foreach (var ctor in typeDecl.Members.OfType<ConstructorDeclarationSyntax>())
        {
            foreach (var param in ctor.ParameterList.Parameters)
            {
                var paramType = param.Type?.ToString();
                if (paramType is not null)
                    map[param.Identifier.ValueText] = paramType;
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

        // Method parameters (e.g. a handler's `Handle(CreateOrderCommand request)` argument).
        foreach (var method in typeDecl.Members.OfType<MethodDeclarationSyntax>())
        {
            foreach (var param in method.ParameterList.Parameters)
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

    /// <summary>Builds the "Namespace.Name" form that matches <see cref="GetTypeFullName"/> / TypeDiscovery.Id.</summary>
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
                    var match = collisions.FirstOrDefault(c => callerNs is not null && c.StartsWith(callerNs + "."));
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

    private static string GetTypeFullName(TypeDeclarationSyntax typeDecl)
    {
        var ns = typeDecl.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()
            ?.Name
            .ToString();
        return ns != null ? $"{ns}.{typeDecl.Identifier.ValueText}" : typeDecl.Identifier.ValueText;
    }
}
