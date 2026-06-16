namespace DevContext.Core.Models;

public sealed record RepoUrl(string Owner, string Repo, string? Ref)
{
    public string ClonePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext", "repos", $"{Owner}-{Repo}-{Ref ?? "default"}");

    public bool IsValid => !string.IsNullOrEmpty(Owner) && !string.IsNullOrEmpty(Repo);

    /// <summary>Parses a GitHub URL or shorthand into a RepoUrl.</summary>
    public static RepoUrl? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var url = input.Trim();

        // Shorthand with protocol prefix: "github.com/user/repo" (no https://)
        if (url.StartsWith("github.com/", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        // Shorthand: "user/repo"
        if (url.Where(c => c == '/').Take(2).Count() == 1 && !url.Contains("://", StringComparison.Ordinal) && !url.Contains(' '))
        {
            var parts = url.Split('/');
            if (parts.Length == 2 && !string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
                return new RepoUrl(parts[0].Trim(), parts[1].Trim().Replace(".git", ""), null);
            return null;
        }

        // Full URL: https://github.com/user/repo
        if (!url.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase)
            && !url.StartsWith("http://github.com/", StringComparison.OrdinalIgnoreCase))
            return null;

        var path = url.Replace("https://github.com/", "", StringComparison.OrdinalIgnoreCase)
                      .Replace("http://github.com/", "", StringComparison.OrdinalIgnoreCase)
                      .TrimEnd('/');

        // Extract ref from /tree/ref or /blob/ref
        string? refName = null;
        if (path.Contains("/tree/", StringComparison.Ordinal))
        {
            var idx = path.IndexOf("/tree/", StringComparison.Ordinal);
            refName = path[(idx + "/tree/".Length)..].Trim('/');
            path = path[..idx];
        }
        else if (path.Contains("/blob/", StringComparison.Ordinal))
        {
            var idx = path.IndexOf("/blob/", StringComparison.Ordinal);
            refName = path[(idx + "/blob/".Length)..].Trim('/');
            path = path[..idx];
        }

        var segs = path.Split('/');
        if (segs.Length < 2) return null;

        return new RepoUrl(segs[0], segs[1].Replace(".git", ""), refName);
    }

    public string ToDisplay() => Ref is not null
        ? $"github.com/{Owner}/{Repo} · {Ref}"
        : $"github.com/{Owner}/{Repo}";
}
