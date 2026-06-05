namespace DevContext.Core.Models;

public sealed record PropertySignature(
    string Name,
    string PropertyType,
    Microsoft.CodeAnalysis.Accessibility Accessibility,
    bool IsStatic,
    bool IsReadOnly,
    bool HasGetter,
    bool HasSetter
);
