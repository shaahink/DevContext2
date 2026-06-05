namespace DevContext.Core.Resolvers;

/// <summary>Parses user-provided focus point strings (file paths, type:method, etc.) into structured focus points.</summary>
public static class FocusPointParser
{
    /// <summary>Parses a focus point from a user-provided input string.</summary>
    public static FocusPoint? Parse(string input, IFileSystem fs)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // Type:Method pattern
        var colonParts = input.Split(':');
        if (colonParts.Length == 2 && !input.Contains("\\") && !input.Contains("/"))
        {
            return new FocusPoint(FocusKind.Method, "", colonParts[0], colonParts[1]);
        }

        // Check if it's an existing file
        if (fs.FileExists(input))
        {
            return new FocusPoint(FocusKind.File, fs.GetFullPath(input), null, null);
        }

        // Check if it's an existing directory
        if (fs.DirectoryExists(input))
        {
            return new FocusPoint(FocusKind.Folder, fs.GetFullPath(input), null, null);
        }

        // Type name (no path separators)
        if (!input.Contains("\\") && !input.Contains("/"))
        {
            return new FocusPoint(FocusKind.Type, "", input, null);
        }

        return null;
    }
}
