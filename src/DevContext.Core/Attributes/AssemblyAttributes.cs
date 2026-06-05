namespace DevContext.Core.Attributes;

/// <summary>Marks an assembly as containing discovery-related components.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class DiscoveryAssemblyAttribute : Attribute;

/// <summary>Specifies the execution order for an extractor (lower values run first).</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExtractorOrderAttribute(int order) : Attribute
{
    /// <summary>Gets the order value.</summary>
    public int Order { get; } = order;
}
