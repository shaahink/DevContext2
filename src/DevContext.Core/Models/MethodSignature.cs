namespace DevContext.Core.Models;

public sealed record MethodSignature(
    string Name,
    string ReturnType,
    ImmutableArray<string> ParameterTypes,
    ImmutableArray<string> ParameterNames,
    Microsoft.CodeAnalysis.Accessibility Accessibility,
    bool IsStatic,
    bool IsAsync
);
