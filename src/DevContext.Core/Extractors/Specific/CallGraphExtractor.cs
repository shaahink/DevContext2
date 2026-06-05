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

                foreach (var methodDecl in typeDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    var callerMethod = methodDecl.Identifier.ValueText;

                    foreach (var invocation in methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        var (calleeType, calleeMethod) = ResolveCallee(invocation);
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

    private static (string Type, string Method) ResolveCallee(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.ValueText;
            var target = memberAccess.Expression.ToString();
            return (target, methodName);
        }

        if (invocation.Expression is IdentifierNameSyntax simpleName)
        {
            return ("this", simpleName.Identifier.ValueText);
        }

        return ("unknown", invocation.Expression?.ToString() ?? "?");
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
