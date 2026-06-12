namespace DevContext.Core.Models;

/// <summary>Defines what kind of entity a focus point targets.</summary>
public enum FocusKind { File, Folder, Type, Method, Endpoint }

/// <summary>Represents a user-specified focus point (file, folder, type, method, or endpoint route) to guide extraction.</summary>
public sealed record FocusPoint(
    FocusKind Kind,
    string FilePath,
    string? TypeName,
    string? MethodName,
    string? HttpMethod = null,
    string? Route = null
);
