namespace DevContext.Core.Models;

/// <summary>Categorizes the kind of a discovered type declaration.</summary>
public enum TypeKind
{
    Class,
    Interface,
    Struct,
    Record,
    Enum,
    Delegate
}
