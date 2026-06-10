using Microsoft.CodeAnalysis;

namespace DevContext.Roslyn.Services;

public sealed class RoslynWorkspaceProvider : IRoslynWorkspaceProvider
{
    private readonly string _solutionPath;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<RoslynWorkspaceProvider> _logger;
    private IRoslynWorkspace? _cached;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RoslynWorkspaceProvider(string solutionPath, IFileSystem fileSystem, ILogger<RoslynWorkspaceProvider> logger)
    {
        _solutionPath = solutionPath;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct)
    {
        if (_cached != null) return _cached;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cached != null) return _cached;

            _logger.LogInformation("Opening Roslyn workspace for {Solution}", _solutionPath);
            var workspace = new AdhocWorkspace();

            var slnContent = await _fileSystem.ReadAllTextAsync(_solutionPath, ct);
            var slnPath = Path.GetDirectoryName(_solutionPath) ?? "";
            var projects = ParseProjectPaths(slnContent);

            foreach (var project in projects)
            {
                ct.ThrowIfCancellationRequested();
                var csprojPath = Path.Combine(slnPath, project);
                if (!_fileSystem.FileExists(csprojPath)) continue;

                try
                {
                    var csprojContent = await _fileSystem.ReadAllTextAsync(csprojPath, ct);
                    var projectName = Path.GetFileNameWithoutExtension(csprojPath);
                    var doc = System.Xml.Linq.XDocument.Parse(csprojContent);

                    var tfm = doc.Descendants("TargetFramework").FirstOrDefault()?.Value
                           ?? doc.Descendants("TargetFrameworks").FirstOrDefault()?.Value
                           ?? "net10.0";

                    var projectInfo = Microsoft.CodeAnalysis.ProjectInfo.Create(
                        ProjectId.CreateNewId(),
                        VersionStamp.Create(),
                        projectName,
                        projectName,
                        LanguageNames.CSharp,
                        csprojPath);

                    workspace.AddProject(projectInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load project {Project}", csprojPath);
                }
            }

            _cached = new RoslynWorkspace(workspace);
            _logger.LogInformation("Roslyn workspace loaded with {ProjectCount} projects", workspace.CurrentSolution.Projects.Count());
            return _cached;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load Roslyn workspace for {Solution}", _solutionPath);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static ImmutableArray<string> ParseProjectPaths(string slnContent)
    {
        var projects = new List<string>();
        var lines = slnContent.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("Project("))
            {
                var parts = trimmed.Split(',');
                if (parts.Length >= 2)
                {
                    var path = parts[1].Trim().Trim('"');
                    if (!string.IsNullOrEmpty(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        projects.Add(path);
                    }
                }
            }
        }
        return projects.ToImmutableArray();
    }
}
