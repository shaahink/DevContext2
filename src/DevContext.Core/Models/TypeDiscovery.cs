using System.Collections.Concurrent;

namespace DevContext.Core.Models;

public sealed class TypeDiscovery
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Namespace { get; init; }
    public required string FilePath { get; init; }
    public required TypeKind Kind { get; init; }
    public required Microsoft.CodeAnalysis.Accessibility Accessibility { get; init; }
    public required ArchitectureLayer Layer { get; init; }
    public ImmutableArray<MethodSignature> Methods { get; init; } = [];
    public ImmutableArray<PropertySignature> Properties { get; init; } = [];
    public ImmutableArray<string> BaseTypes { get; init; } = [];
    public ImmutableArray<string> ImplementedInterfaces { get; init; } = [];
    public ImmutableArray<string> Attributes { get; init; } = [];
    public string? SourceBody { get; set; }
    public ConcurrentBag<string> Tags { get; } = [];
    public bool IsPruned { get; set; }
    public float PathProximityScore { get; set; }
    public float RelevanceScore { get; set; }
}
