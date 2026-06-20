namespace DevContext.Core.Resolvers;

/// <summary>Resolves the project root directory from a user-provided input path using solution/project file discovery.</summary>
public sealed class ProjectRootResolver
{
    /// <summary>Resolves the project root, finding .sln or .csproj files by walking up and down the
    /// directory tree, then computes the Hybrid scan set (closure for project/subfolder input,
    /// whole-solution otherwise) via <see cref="ScopeResolver"/>.</summary>
    public static async Task<ProjectRootResult> ResolveAsync(string inputPath, IFileSystem fs, CancellationToken ct = default)
    {
        var baseResult = await ResolveBaseAsync(inputPath, fs, ct);
        var (dirs, anchor) = await ScopeResolver.ResolveAsync(baseResult, fs, ct);
        return baseResult with { ScopeProjectDirs = dirs, AnchorProjectPath = anchor };
    }

    private static async Task<ProjectRootResult> ResolveBaseAsync(string inputPath, IFileSystem fs, CancellationToken ct)
    {
        var fullPath = fs.GetFullPath(inputPath);

        if (fs.FileExists(fullPath))
        {
            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            if (ext is ".sln" or ".slnx")
            {
                var dir = fs.GetDirectoryName(fullPath)!;
                return new ProjectRootResult(dir, fullPath, [fullPath], ResolutionMethod.ExplicitSln, null);
            }
            if (ext == ".csproj")
            {
                var dir = fs.GetDirectoryName(fullPath)!;
                return new ProjectRootResult(dir, null, [fullPath], ResolutionMethod.ExplicitCsproj, null);
            }
        }

        if (!fs.DirectoryExists(fullPath))
        {
            throw new DirectoryNotFoundException($"Path not found: {fullPath}");
        }

        var slnFiles = await FindSolutionsAsync(fs, fullPath, ct);
        if (slnFiles.Count > 0)
        {
            return new ProjectRootResult(fullPath, slnFiles[0], slnFiles.ToImmutableArray(),
                ResolutionMethod.DirectoryContainsSln, null);
        }

        var current = fullPath;
        for (int i = 0; i < 5; i++)
        {
            var parent = fs.GetDirectoryName(current);
            if (parent == null) break;
            var parentSlns = await FindSolutionsAsync(fs, parent, ct);
            if (parentSlns.Count > 0)
            {
                return new ProjectRootResult(fullPath, parentSlns[0], parentSlns.ToImmutableArray(),
                    ResolutionMethod.WalkedUp, $"walked up {i + 1} levels");
            }
            current = parent;
        }

        current = fullPath;
        for (int i = 0; i < 3; i++)
        {
            var dirs = fs.EnumerateDirectories(current, "*", SearchOption.TopDirectoryOnly).ToList();
            foreach (var dir in dirs)
            {
                var nestedSlns = await FindSolutionsAsync(fs, dir, ct);
                if (nestedSlns.Count > 0)
                {
                    return new ProjectRootResult(fullPath, nestedSlns[0], nestedSlns.ToImmutableArray(),
                        ResolutionMethod.WalkedDown, $"walked down {i + 1} levels to {dir}");
                }
            }
            var next = dirs.FirstOrDefault();
            if (next == null) break;
            current = next;
        }

        var csprojFiles = await fs.EnumerateFilesAsync(fullPath, "*.csproj", SearchOption.AllDirectories, ct).ToListAsync2(ct);
        if (csprojFiles.Count > 0)
        {
            return new ProjectRootResult(fullPath, null, csprojFiles.ToImmutableArray(),
                ResolutionMethod.FolderMode, $"no .sln found, {csprojFiles.Count} .csproj files");
        }

        return new ProjectRootResult(fullPath, null, [], ResolutionMethod.FolderMode,
            "no .sln or .csproj found, folder mode");
    }

    /// <summary>Finds solution files in a directory — both legacy <c>.sln</c> and XML <c>.slnx</c>.
    /// <c>.sln</c> is listed first so single-<c>.sln</c> repos keep their existing primary selection;
    /// on Windows the <c>*.sln</c> glob can also match <c>.slnx</c>, so duplicates are dropped.</summary>
    private static async Task<List<string>> FindSolutionsAsync(IFileSystem fs, string dir, CancellationToken ct)
    {
        var result = new List<string>();
        foreach (var pattern in new[] { "*.sln", "*.slnx" })
        {
            foreach (var file in await fs.EnumerateFilesAsync(dir, pattern, SearchOption.TopDirectoryOnly, ct).ToListAsync2(ct))
                if (!result.Contains(file)) result.Add(file);
        }
        return result;
    }
}
