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
);
