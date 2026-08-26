using System.Collections.Concurrent;

using DevContext.Core.Graph2;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Models;

/// <summary>Cached pre-parsed syntax nodes for a single file.</summary>
public sealed record FileSyntaxNodes(
    ImmutableArray<TypeDeclarationSyntax> TypeDeclarations,
    ImmutableArray<InvocationExpressionSyntax> Invocations
);

/// <summary>Aggregates analysis data shared across pipeline stages.</summary>
public sealed class SharedAnalysisContext
{
    /// <summary>All .cs source file paths discovered in the project. INVARIANT: C# compilation units only —
    /// every consumer parses these with <c>CSharpSyntaxTree.ParseText</c>. Razor markup (.razor/.cshtml)
    /// is text-only and lives in <see cref="AllContentFiles"/>; never add it here or it gets parsed as C#
    /// and folded into the semantic compilation (huge perf hit on Razor-heavy repos + garbage trees).</summary>
    public IReadOnlyList<string> AllSourceFiles { get; set; } = [];
    /// <summary>Razor markup file paths (.razor / .cshtml). Consumed as TEXT (regex/@page scanning) by the
    /// Razor Pages and Blazor extractors — never Roslyn-parsed as C#. The ONE sanctioned C# path is
    /// <c>RazorCodeVirtualizer</c> (C1): it extracts the <c>@code</c>/<c>@functions</c> block text and
    /// parses ONLY that, cached in <see cref="RazorVirtualTrees"/> — the markup still never sees Roslyn.</summary>
    public IReadOnlyList<string> AllContentFiles { get; set; } = [];
    /// <summary>All .csproj project file paths discovered.</summary>
    public IReadOnlyList<string> AllProjectFiles { get; set; } = [];
    /// <summary>Focus points extracted from user input to guide extraction.</summary>
    public IReadOnlyList<FocusPoint> FocusPoints { get; set; } = [];
    /// <summary>Raw unresolved focus points as parsed from user input (before type resolution).</summary>
    public IReadOnlyList<FocusPoint> UnresolvedFocusPoints { get; set; } = [];
    /// <summary>Project dependency graph showing project-to-project references.</summary>
    public ProjectDependencyGraph? ProjectGraph { get; set; }
    /// <summary>Maps project names to their inferred architecture layer.</summary>
    public IReadOnlyDictionary<string, ArchitectureLayer> ProjectLayerMap { get; set; }
        = FrozenDictionary<string, ArchitectureLayer>.Empty;
    /// <summary>Call graph mapping methods to their call edges.</summary>
    public CallGraph? CallGraph { get; set; }
    /// <summary>F1 (#33): the ONE <see cref="SymbolTable"/> the graph world resolved through (scoped
    /// to the analysed solution, built pre-compression), kept so post-analysis consumers — the
    /// dogfood truth sweep foremost — judge declarations with the SAME index the builders and the
    /// INV-C oracle used. The legacy catalog's compression mutates <see cref="TypeDiscovery"/>
    /// members afterwards (TrivialMemberCompressor strips ToString/Equals/GetHashCode and
    /// auto-properties), so an oracle re-derived from the compressed model disagrees with the
    /// graph's own admission decisions. Null when the full graph was not built.</summary>
    public SymbolTable? GraphSymbols { get; set; }
    /// <summary>Shared cache of pre-parsed syntax nodes per file. Populated once, read by all Stage 2 extractors.</summary>
    public ConcurrentDictionary<string, Lazy<Task<FileSyntaxNodes>>> SyntaxNodeCache { get; } = new();

    /// <summary>Get or lazily populates the parsed syntax nodes for a given file path.</summary>
    public async Task<FileSyntaxNodes> GetOrParseSyntaxNodesAsync(string filePath, Func<Task<FileSyntaxNodes>> factory)
    {
        var lazy = SyntaxNodeCache.GetOrAdd(filePath, _ => new Lazy<Task<FileSyntaxNodes>>(factory));
        return await lazy.Value;
    }

    /// <summary>Content-keyed cache of structured body facts per file (Loom L2, <c>facts-v1</c>). Built
    /// once from the shared parse by <c>BodyFactsExtractor</c>; read by seam detectors. Never re-parses —
    /// the factory walks the syntax tree already memoised in <see cref="IAnalysisCache"/>.</summary>
    public ConcurrentDictionary<string, Lazy<Task<ImmutableArray<BodyFacts>>>> BodyFactsCache { get; } = new();

    /// <summary>Gets or lazily builds the body facts for a given file path (facts-v1 cache).</summary>
    public async Task<ImmutableArray<BodyFacts>> GetOrBuildBodyFactsAsync(
        string filePath, Func<Task<ImmutableArray<BodyFacts>>> factory)
    {
        var lazy = BodyFactsCache.GetOrAdd(filePath, _ => new Lazy<Task<ImmutableArray<BodyFacts>>>(factory));
        return await lazy.Value;
    }

    /// <summary>All body facts gathered across files after the BodyFacts pass (facts-v1). Empty until
    /// <c>BodyFactsExtractor</c> runs; consumed by the seam detectors (L2) and the assembler (L2.3).</summary>
    public IReadOnlyList<BodyFacts> AllBodyFacts { get; set; } = [];

    /// <summary>C1: virtual C# trees built from <c>.razor</c> <c>@code</c> blocks (null = the file has no
    /// extractable C#). Keyed by razor path; built once via <c>RazorCodeVirtualizer</c>, shared by the
    /// structure and call-graph extractors. Not persisted — rebuilt from source on a cache miss.</summary>
    public ConcurrentDictionary<string, Lazy<Task<Microsoft.CodeAnalysis.SyntaxTree?>>> RazorVirtualTrees { get; } = new();

    /// <summary>Gets or lazily builds the razor virtual tree for a given file path (C1).</summary>
    public async Task<Microsoft.CodeAnalysis.SyntaxTree?> GetOrBuildRazorVirtualTreeAsync(
        string filePath, Func<Task<Microsoft.CodeAnalysis.SyntaxTree?>> factory)
    {
        var lazy = RazorVirtualTrees.GetOrAdd(filePath, _ => new Lazy<Task<Microsoft.CodeAnalysis.SyntaxTree?>>(factory));
        return await lazy.Value;
    }
}

/// <summary>Represents the dependency graph of projects within the solution.</summary>
public sealed class ProjectDependencyGraph
{
    /// <summary>Adjacency list mapping project names to their referenced project names.</summary>
    public IReadOnlyDictionary<string, ImmutableArray<string>> AdjacencyList { get; }
    /// <summary>Creates a project dependency graph from an adjacency dictionary.</summary>
    public ProjectDependencyGraph(Dictionary<string, ImmutableArray<string>> adjacency)
    {
        AdjacencyList = adjacency.ToFrozenDictionary();
    }
}

/// <summary>Represents a call graph mapping methods to their outgoing call edges.</summary>
public sealed class CallGraph
{
    /// <summary>Edges grouped by caller key (Type.Method).</summary>
    public IReadOnlyDictionary<string, ImmutableArray<CallEdge>> Edges { get; }
    /// <summary>Creates a call graph from an edges dictionary.</summary>
    public CallGraph(Dictionary<string, ImmutableArray<CallEdge>> edges)
    {
        Edges = edges.ToFrozenDictionary();
    }
}

/// <summary>Represents a single call edge between a caller and callee method.</summary>
public sealed record CallEdge(
    string CallerType,
    string CallerMethod,
    string CalleeType,
    string CalleeMethod,
    string? CallSiteLocation
)
{
    /// <summary>How the callee was resolved — semantic (Roslyn symbol) carries [verified] into the trace,
    /// syntactic stays [approx]. Defaults to syntactic for callers that don't resolve symbolically.</summary>
    public Graph.Resolution Resolution { get; init; } = Graph.Resolution.Syntactic;
}
