using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
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
    /// <summary>All .cs source file paths discovered in the project.</summary>
    public IReadOnlyList<string> AllSourceFiles { get; set; } = [];
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
    /// <summary>Shared cache of pre-parsed syntax nodes per file. Populated once, read by all Stage 2 extractors.</summary>
    public ConcurrentDictionary<string, Lazy<Task<FileSyntaxNodes>>> SyntaxNodeCache { get; } = new();

    /// <summary>Gets or lazily populates the parsed syntax nodes for a given file path.</summary>
    public async Task<FileSyntaxNodes> GetOrParseSyntaxNodesAsync(string filePath, Func<Task<FileSyntaxNodes>> factory)
    {
        var lazy = SyntaxNodeCache.GetOrAdd(filePath, _ => new Lazy<Task<FileSyntaxNodes>>(factory));
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
);
