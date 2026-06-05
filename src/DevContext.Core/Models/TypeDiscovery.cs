using System.Collections.Concurrent;

namespace DevContext.Core.Models;

/// <summary>Represents a discovered type (class, interface, struct, record, enum) in the codebase.</summary>
public sealed class TypeDiscovery
{
    /// <summary>Fully qualified type ID (e.g. "Namespace.ClassName").</summary>
    public required string Id { get; init; }
    /// <summary>Short type name.</summary>
    public required string Name { get; init; }
    /// <summary>Namespace containing the type.</summary>
    public required string Namespace { get; init; }
    /// <summary>Source file path where the type is declared.</summary>
    public required string FilePath { get; init; }
    /// <summary>Kind of the type (Class, Interface, Struct, Record, Enum, Delegate).</summary>
    public required TypeKind Kind { get; init; }
    /// <summary>Accessibility level of the type.</summary>
    public required Microsoft.CodeAnalysis.Accessibility Accessibility { get; init; }
    /// <summary>Architecture layer this type belongs to.</summary>
    public required ArchitectureLayer Layer { get; init; }
    /// <summary>Methods declared in this type.</summary>
    public ImmutableArray<MethodSignature> Methods { get; set; } = [];
    /// <summary>Properties declared in this type.</summary>
    public ImmutableArray<PropertySignature> Properties { get; set; } = [];
    /// <summary>Base types this type extends.</summary>
    public ImmutableArray<string> BaseTypes { get; init; } = [];
    /// <summary>Interfaces implemented by this type.</summary>
    public ImmutableArray<string> ImplementedInterfaces { get; init; } = [];
    /// <summary>Attributes applied to this type.</summary>
    public ImmutableArray<string> Attributes { get; init; } = [];
    /// <summary>Full source body text of the type declaration (populated by SourceBodyExtractor).</summary>
    public string? SourceBody { get; set; }
    /// <summary>Tags added by compressors and other pipeline stages.</summary>
    public ConcurrentBag<string> Tags { get; } = [];
    /// <summary>Whether this type has been pruned from the model.</summary>
    public bool IsPruned { get; set; }
    /// <summary>Proximity score based on directory distance from focus points (0.0 to 1.0).</summary>
    public float PathProximityScore { get; set; }
    /// <summary>Relevance score based on detection patterns and call graph reachability.</summary>
    public float RelevanceScore { get; set; }
}
