namespace ConsoleBackstop.Lib;

/// <summary>Fixture (Prism D1.1e / audit A5): a public surface that the render backstop must show
/// when the App-archetype map has zero entries.</summary>
public sealed class TextKit
{
    /// <summary>Normalizes whitespace runs to single spaces.</summary>
    public static string Collapse(string input) => string.Join(' ',
        input.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Truncates to <paramref name="max"/> characters with an ellipsis.</summary>
    public static string Truncate(string input, int max)
        => input.Length <= max ? input : input[..max] + "...";
}

/// <summary>Builds URL slugs from titles.</summary>
public sealed class SlugBuilder
{
    /// <summary>Lower-cases and hyphenates a title.</summary>
    public string Build(string title) => title.ToLowerInvariant().Replace(' ', '-');
}
