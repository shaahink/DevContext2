using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DevContext.Roslyn.Services;

public sealed class MSBuildRoslynWorkspaceProvider : IRoslynWorkspaceProvider
{
    private readonly string _solutionPath;
    private readonly ILogger<MSBuildRoslynWorkspaceProvider> _logger;
    private readonly TimeSpan _timeout;
    private IRoslynWorkspace? _cached;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public MSBuildRoslynWorkspaceProvider(
        string solutionPath,
        ILogger<MSBuildRoslynWorkspaceProvider> logger,
        TimeSpan? timeout = null)
    {
        _solutionPath = solutionPath;
        _logger = logger;
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public async Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct)
    {
        if (_cached != null) return _cached;

        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_cached != null) return _cached;

            _logger.LogInformation("Opening MSBuild workspace for {Solution}", _solutionPath);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeout);
            var token = cts.Token;

            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(_solutionPath, progress: null, cancellationToken: token).ConfigureAwait(false);

            _cached = new MSBuildRoslynWorkspace(workspace, solution);
            _logger.LogInformation("MSBuild workspace loaded with {ProjectCount} projects",
                solution.Projects.Count());
            return _cached;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("MSBuild workspace timed out for {Solution}", _solutionPath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load MSBuild workspace for {Solution}", _solutionPath);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed class MSBuildRoslynWorkspace : IRoslynWorkspace
    {
        private readonly MSBuildWorkspace _workspace;
        private readonly Solution _solution;

        public MSBuildRoslynWorkspace(MSBuildWorkspace workspace, Solution solution)
        {
            _workspace = workspace;
            _solution = solution;
        }

        public Task<ImmutableArray<DevContext.Core.Contracts.ProjectInfo>> GetProjectsAsync(CancellationToken ct)
        {
            var projects = _solution.Projects.Select(p => new DevContext.Core.Contracts.ProjectInfo(
                p.Name,
                p.FilePath ?? p.Name,
                p.Language,
                [],
                p.ProjectReferences.Select(r => r.ProjectId.ToString()).ToImmutableArray(),
                p.MetadataReferences
                    .OfType<PortableExecutableReference>()
                    .Select(r => new PackageReferenceInfo(
                        Path.GetFileNameWithoutExtension(r.FilePath ?? "unknown"), ""))
                    .ToImmutableArray()
            )).ToImmutableArray();

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
}
