namespace DevContext.Core.Models;

/// <summary>Describes a property signature including name, type, accessibility, and accessor availability.</summary>
public sealed record PropertySignature(
    string Name,
    string PropertyType,
    Microsoft.CodeAnalysis.Accessibility Accessibility,
    bool IsStatic,
    bool IsReadOnly,
    bool HasGetter,
    bool HasSetter
);
