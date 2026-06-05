namespace DevContext.Core.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class DiscoveryAssemblyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ExtractorOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
