namespace DevContext.Core.Models;

/// <summary>Defines what kind of entity a focus point targets.</summary>
public enum FocusKind { File, Folder, Type, Method }

/// <summary>Represents a user-specified focus point (file, folder, type, or method) to guide extraction.</summary>
public sealed record FocusPoint(
    FocusKind Kind,
    string FilePath,
    string? TypeName,
    string? MethodName
);
