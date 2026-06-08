using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Walks syntax trees to build a BFS-depth-limited call graph for Debug and Full extraction profiles.</summary>
[ExtractorOrder(30)]
public sealed class CallGraphExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "CallGraphExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Deep;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;
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
        var allEdges = new ConcurrentBag<CallEdge>();

        // Build DI resolution map: interface/abstract type → concrete implementation
        // Key: short type name, Value: short implementation name
        var diMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
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
        var interfaceImplMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in model.Types)
        {
            var type = kv.Value;
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

        // Build short-name → fully-qualified-name map from model.Types
        var fqnMap = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in model.Types)
        {
            if (!fqnMap.ContainsKey(kv.Value.Name))
                fqnMap[kv.Value.Name] = kv.Key;
        }

        await foreach (var filePath in EnumerateSourceFilesAsync(context, ct))
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var typeDecls = root.DescendantNodes().OfType<TypeDeclarationSyntax>();

            foreach (var typeDecl in typeDecls)
            {
                var callerType = GetTypeFullName(typeDecl);
                if (string.IsNullOrEmpty(callerType)) continue;

                // Build field map for this type: fieldName → declaredType
                var fieldMap = BuildFieldMap(typeDecl, diMap);

                foreach (var methodDecl in typeDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    var callerMethod = methodDecl.Identifier.ValueText;

                    foreach (var invocation in methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        var (calleeType, calleeMethod) = ResolveCallee(invocation, callerType, fieldMap, diMap, interfaceImplMap, fqnMap);
                        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                        allEdges.Add(new CallEdge(
                            callerType, callerMethod,
                            calleeType, calleeMethod,
                            $"{filePath}:{lineNumber}"));
                    }
                }

                foreach (var ctorDecl in typeDecl.Members.OfType<ConstructorDeclarationSyntax>())
                {
                    var callerMethod = ctorDecl.Identifier.ValueText;

                    foreach (var invocation in ctorDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        var (calleeType, calleeMethod) = ResolveCallee(invocation, callerType, fieldMap, diMap, interfaceImplMap, fqnMap);
                        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                        allEdges.Add(new CallEdge(
                            callerType, callerMethod,
                            calleeType, calleeMethod,
                            $"{filePath}:{lineNumber}"));
                    }
                }
            }
        }

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

        model.AddDiagnostic(DiagnosticLevel.Info, Name,
            $"Built call graph: {includedEdges.Count} edges at depth ≤ {maxDepth}");
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
            foreach (var type in model.Types.Values.Where(t => !t.IsPruned))
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

        return map;
    }

    private static (string Type, string Method) ResolveCallee(InvocationExpressionSyntax invocation,
        string callerType, Dictionary<string, string> fieldMap, Dictionary<string, string> diMap,
        Dictionary<string, string> interfaceImplMap, Dictionary<string, string> fqnMap)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.ValueText;
            var target = memberAccess.Expression.ToString();
            var resolved = ResolveType(target, callerType, fieldMap, diMap, interfaceImplMap, fqnMap);
            return (resolved, methodName);
        }

        if (invocation.Expression is IdentifierNameSyntax simpleName)
        {
            var resolved = ResolveType("this", callerType, fieldMap, diMap, interfaceImplMap, fqnMap);
            return (resolved, simpleName.Identifier.ValueText);
        }

        return ("unknown", invocation.Expression?.ToString() ?? "?");
    }

    private static string ResolveType(string target, string callerType, Dictionary<string, string> fieldMap,
        Dictionary<string, string> diMap, Dictionary<string, string> interfaceImplMap,
        Dictionary<string, string> fqnMap)
    {
        // Same-class method calls and base calls resolve to the containing type
        if (target is "this" or "base")
            return callerType;

        var fieldName = target.StartsWith("this.", StringComparison.Ordinal)
            ? target["this.".Length..]
            : target;

        if (fieldMap.TryGetValue(fieldName, out var declaredType))
        {
            var shortType = StripGenerics(declaredType);

            // Try DI map (e.g. IBacktestCommandService → BacktestOrchestrator from AddSingleton)
            if (diMap.TryGetValue(shortType, out var impl))
                shortType = impl;
            // Fallback: interface→implementation from type hierarchy
            else if (interfaceImplMap.TryGetValue(shortType, out var impl2))
                shortType = impl2;

            // Resolve to fully qualified name
            if (fqnMap.TryGetValue(shortType, out var fqn))
                return fqn;

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

    private static async IAsyncEnumerable<string> EnumerateSourceFilesAsync(
        DiscoveryContext context, [EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var file in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            yield return file;
        }
    }
}
