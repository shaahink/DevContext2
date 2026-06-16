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
    /// <summary>Role importance score ∈ [0,1] for legacy render path ordering.</summary>
    public double RoleScore { get; set; }
    /// <summary>Combined ranking score used by legacy RenderPlanBuilder.</summary>
    public double FinalScore { get; set; }
    /// <summary>Relevance score used by compression/truncation.</summary>
    public double RelevanceScore { get; set; }
    /// <summary>Reason why this type was excluded, only set by hard-irrelevance rules.</summary>
    public string? ExclusionReason { get; set; }
    /// <summary>Scorer says this type should never be shown (test project noise).</summary>
    public bool IsHardExcluded { get; set; }
    /// <summary>Proximity score based on directory distance from focus points (0.0 to 1.0). Deprecated — PLAN-10 E1.</summary>
    public float PathProximityScore { get; set; }
    /// <summary>Reachability score via BFS over type-collapsed call graph. Deprecated — PLAN-10 E1.</summary>
    public double GraphProximity { get; set; }
    /// <summary>Focus proximity score ∈ [0,1] encompassing path distance and call-graph reachability. Deprecated — PLAN-10 E1.</summary>
    public double FocusScore { get; set; }
}
