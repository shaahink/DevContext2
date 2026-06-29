namespace DevContext.Core.Models;

/// <summary>Describes a method signature including name, return type, parameters, and modifiers.</summary>
public sealed record MethodSignature(
    string Name,
    string ReturnType,
    ImmutableArray<string> ParameterTypes,
    ImmutableArray<string> ParameterNames,
    Microsoft.CodeAnalysis.Accessibility Accessibility,
    bool IsStatic,
    bool IsAsync
)
{
    /// <summary>True when this is a C# extension method (static, first parameter has the <c>this</c> modifier).</summary>
    public bool IsExtension { get; init; }
    /// <summary>For an extension method, the type it extends (the first parameter's declared type); otherwise null.</summary>
    public string? ExtendedType { get; init; }
    /// <summary>The <c>&lt;summary&gt;</c> text from the method's leading XML doc comment, whitespace-collapsed; null if none.</summary>
    public string? XmlDoc { get; init; }
}
