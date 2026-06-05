namespace DevContext.Core.Models;

public enum FocusKind { File, Folder, Type, Method }

public sealed record FocusPoint(
    FocusKind Kind,
    string FilePath,
    string? TypeName,
    string? MethodName
);
