namespace DevContext.Roslyn.Services;

using Microsoft.CodeAnalysis;

public sealed class RoslynWorkspace : IRoslynWorkspace
{
    private readonly Workspace _workspace;
    private readonly Solution _solution;

    public RoslynWorkspace(Workspace workspace)
    {
        _workspace = workspace;
        _solution = workspace.CurrentSolution;
    }

    public Task<ImmutableArray<DevContext.Core.Contracts.ProjectInfo>> GetProjectsAsync(CancellationToken ct)
    {
        var projects = _solution.Projects.Select(p =>
        {
            return new DevContext.Core.Contracts.ProjectInfo(
                p.Name,
                p.FilePath ?? p.Name,
                p.Language,
                ImmutableArray<string>.Empty,
                p.ProjectReferences.Select(r => r.ProjectId.ToString()).ToImmutableArray(),
                p.MetadataReferences
                    .OfType<PortableExecutableReference>()
                    .Select(r => new PackageReferenceInfo(
                        Path.GetFileNameWithoutExtension(r.FilePath ?? "unknown"), ""))
                    .ToImmutableArray());
        }).ToImmutableArray();

        return Task.FromResult(projects);
    }

    public Task<ImmutableArray<DevContext.Core.Contracts.DocumentInfo>> GetDocumentsAsync(CancellationToken ct)
    {
        var docs = _solution.Projects
            .SelectMany(p => p.Documents)
            .Select(d => new DevContext.Core.Contracts.DocumentInfo(
                d.FilePath ?? d.Name,
                d.Folders.Count > 0 ? d.Folders[^1] : null))
            .ToImmutableArray();

        return Task.FromResult(docs);
    }
}
