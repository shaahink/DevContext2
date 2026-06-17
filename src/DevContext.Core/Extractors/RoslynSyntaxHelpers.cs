using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors;

/// <summary>Shared Roslyn syntax helpers for namespace and type name resolution.</summary>
public static class RoslynSyntaxHelpers
{
    /// <summary>Returns the namespace of a type declaration, or "global" if none.</summary>
    public static string GetNamespace(TypeDeclarationSyntax typeDecl)
        => typeDecl.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()
            ?.Name
            .ToString() ?? "global";

    /// <summary>Returns the fully qualified type name as "Namespace.TypeName".</summary>
    public static string GetTypeFullName(TypeDeclarationSyntax typeDecl)
    {
        var ns = GetNamespace(typeDecl);
        return !string.Equals(ns, "global", StringComparison.Ordinal) ? $"{ns}.{typeDecl.Identifier.ValueText}" : typeDecl.Identifier.ValueText;
    }

    /// <summary>Returns "Namespace.TypeName.MethodName" for a member inside a type.</summary>
    public static string? GetTypeMemberFullName(SyntaxNode node, string memberName)
    {
        var type = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (type is null) return null;
        var ns = GetNamespace(type);
        return !string.Equals(ns, "global", StringComparison.Ordinal) ? $"{ns}.{type.Identifier.ValueText}.{memberName}" : null;
    }
}
