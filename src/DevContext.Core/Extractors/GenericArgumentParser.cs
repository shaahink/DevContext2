namespace DevContext.Core.Extractors;

/// <summary>Shared generic argument parsing used by MediatR and EventBus extractors.</summary>
public static class GenericArgumentParser
{
    /// <summary>Extracts generic type arguments from a type name like <c>IRequestHandler&lt;T,Q&gt;</c>.</summary>
    public static string[] ExtractGenericArguments(string typeName)
    {
        var openBracket = typeName.IndexOf('<');
        if (openBracket < 0) return [];

        var closeBracket = typeName.LastIndexOf('>');
        if (closeBracket <= openBracket) return [];

        var inner = typeName.Substring(openBracket + 1, closeBracket - openBracket - 1);
        return SplitGenericArgs(inner);
    }

    /// <summary>Extracts the base name before generics, e.g. <c>IRequestHandler</c> from <c>IRequestHandler&lt;T&gt;</c>.</summary>
    public static string? ExtractGenericBaseName(string typeName)
    {
        var openBracket = typeName.IndexOf('<');
        return openBracket < 0 ? typeName : typeName[..openBracket];
    }

    /// <summary>Splits comma-separated generic arguments respecting nested angle brackets.</summary>
    public static string[] SplitGenericArgs(string args)
    {
        var depth = 0;
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var ch in args)
        {
            switch (ch)
            {
                case '<':
                    depth++;
                    current.Append(ch);
                    break;
                case '>':
                    depth--;
                    current.Append(ch);
                    break;
                case ',' when depth == 0:
                    parts.Add(current.ToString().Trim());
                    current.Clear();
                    break;
                default:
                    current.Append(ch);
                    break;
            }
        }

        if (current.Length > 0)
            parts.Add(current.ToString().Trim());

        return [.. parts];
    }
}
