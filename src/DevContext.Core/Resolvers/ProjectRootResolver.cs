namespace DevContext.Core.Resolvers;

public sealed class ProjectRootResolver
{
    public async Task<ProjectRootResult> ResolveAsync(string inputPath, IFileSystem fs, CancellationToken ct = default)
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

        var slnFiles = await fs.EnumerateFilesAsync(fullPath, "*.sln", SearchOption.TopDirectoryOnly, ct).ToListAsync2(ct);
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
            var parentSlns = await fs.EnumerateFilesAsync(parent, "*.sln", SearchOption.TopDirectoryOnly, ct).ToListAsync2(ct);
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
                var nestedSlns = await fs.EnumerateFilesAsync(dir, "*.sln", SearchOption.TopDirectoryOnly, ct).ToListAsync2(ct);
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
}
