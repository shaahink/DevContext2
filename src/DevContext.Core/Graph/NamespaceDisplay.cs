namespace DevContext.Core.Graph;

/// <summary>Display fallback for the synthetic <c>global</c> namespace (T2.7, audit A7). A type declared in
/// the global namespace is keyed with namespace <c>global</c> (<c>SyntaxStructureExtractor</c>) and member id
/// <c>global.Type::…</c> (<c>BodyFactExtractor</c>, T2.5) so the graph has a stable identity — but the literal
/// "global" must never reach the user as a group/service label. Fall back namespace → project/top-folder;
/// strip the <c>global.</c> prefix from a rendered FQN. Ids keep it internally; only the DISPLAY changes.</summary>
public static class NamespaceDisplay
{
    /// <summary>The synthetic namespace assigned to global-namespace types.</summary>
    public const string GlobalNamespace = "global";

    /// <summary>True when the namespace is the synthetic global sentinel or empty.</summary>
    public static bool IsGlobal(string? ns)
        => string.IsNullOrEmpty(ns) || string.Equals(ns, GlobalNamespace, StringComparison.Ordinal);

    /// <summary>A user-facing group label: the namespace, else the supplied fallback (project name or top
    /// folder), else "app" — never the literal "global".</summary>
    public static string Label(string? ns, string? fallback)
        => IsGlobal(ns) ? (string.IsNullOrWhiteSpace(fallback) ? "app" : fallback!.Trim()) : ns!;

    /// <summary>The immediate containing folder name of a file path, used as a "top folder" fallback label
    /// when a type has no namespace to group under.</summary>
    public static string? FolderLabel(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return null;
        var dir = Path.GetDirectoryName(filePath.Replace('\\', '/'));
        if (string.IsNullOrEmpty(dir)) return null;
        var name = Path.GetFileName(dir.TrimEnd('/'));
        return string.IsNullOrEmpty(name) ? null : name;
    }
}
