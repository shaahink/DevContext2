using System.Diagnostics;
using System.Text.Json;

using DevContext.Core.Contracts;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph2;

/// <summary>Result of running the Tier B <see cref="SemanticLitePopulator"/> over a solution.</summary>
public sealed record SemanticLiteResult
{
    /// <summary>Count of projects where <c>project.assets.json</c> was found and used for NuGet resolution.</summary>
    public int ProjectsWithAssets { get; init; }
    /// <summary>Count of projects that degraded to Tier A (no assets.json).</summary>
    public int ProjectsDegraded { get; init; }
    /// <summary>Skipped (no source files).</summary>
    public int ProjectsSkipped { get; init; }
    /// <summary>Total number of <see cref="LocalDeclOp"/> whose <c>InferredFrom</c> was null but was resolved via semantic binding.</summary>
    public int VarDeclsResolved { get; init; }
    /// <summary>Total number of <see cref="InvocationOp"/> whose <c>ReceiverType</c> was resolved via semantic binding.</summary>
    public int ReceiversResolved { get; init; }
    /// <summary>The upgraded body facts with semantic resolution applied (null if no compilation was built).</summary>
    public ImmutableArray<BodyFacts> UpgradedBodyFacts { get; init; }
    /// <summary>The built compilation (null if degraded entirely).</summary>
    public CSharpCompilation? Compilation { get; init; }

    public SemanticLiteResult() { UpgradedBodyFacts = []; }
}

/// <summary>Builds <c>CSharpCompilation</c> objects from parsed trees + framework references +
/// NuGet dlls resolved via <c>obj/project.assets.json</c> (Tier B of the Loom design §6).
/// When assets.json is missing, degrades per-project to Tier A with a Coverage note.
///
/// For each <see cref="BodyFacts"/> member, uses <see cref="SemanticModel"/> to upgrade
/// <see cref="LocalDeclOp.InferredFrom"/> (the <c>var x = expr.Adapt&lt;T&gt;()</c> pattern) and
/// <see cref="InvocationOp.ReceiverType"/> from null/guessed to <see cref="ResolutionTier.Semantic"/>.
/// Law R2 applies: only upgrades (Syntactic → Semantic), never downgrades.</summary>
public static class SemanticLitePopulator
{
    /// <summary>Framework reference assemblies loaded once from the TPA.</summary>
    private static readonly Lazy<ImmutableArray<MetadataReference>> FrameworkRefs = new(() =>
    {
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrEmpty(tpa)) return [];
        var refs = ImmutableArray.CreateBuilder<MetadataReference>();
        foreach (var path in tpa.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
            try { refs.Add(MetadataReference.CreateFromFile(path)); }
            catch { }
        }
        return refs.ToImmutable();
    });

    /// <summary>Runs the semantic-lite populator. Builds one <c>CSharpCompilation</c> from all
    /// project trees + framework references + NuGet dlls (when assets.json is present).
    /// Then upgrades <see cref="BodyFacts"/> with semantic binding results for the demand set.
    /// Degrades per-project when assets.json is missing; the result records tier routing.
    /// Returns the upgraded facts + stats.</summary>
    public static SemanticLiteResult Populate(
        IReadOnlyList<ProjectInfo> projects,
        IReadOnlyList<BodyFacts> bodyFacts,
        IAnalysisCache cache,
        string rootPath,
        CancellationToken ct = default)
    {
        var result = new SemanticLiteResult();
        if (projects.Count == 0) return result;

        var sw = Stopwatch.StartNew();

        var nugetRefs = ResolveNuGetMetadataRefs(projects, rootPath, result);
        var allTrees = new List<SyntaxTree>();
        var fileToProject = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var proj in projects)
        {
            if (ct.IsCancellationRequested) break;
            var projDir = Path.GetDirectoryName(proj.FilePath);
            if (projDir is null) continue;

            var projectTreesAdded = 0;
            foreach (var path in cache.KnownFilePaths)
            {
                if (!path.StartsWith(projDir, StringComparison.OrdinalIgnoreCase)) continue;
                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    var tree = cache.GetSyntaxTreeAsync(path, ct).AsTask().GetAwaiter().GetResult();
                    allTrees.Add(tree);
                    fileToProject[path] = proj.Name;
                    projectTreesAdded++;
                }
                catch { }
            }

            if (projectTreesAdded == 0)
                result = result with { ProjectsSkipped = result.ProjectsSkipped + 1 };
        }

        if (allTrees.Count == 0 || nugetRefs.Length == 0 && FrameworkRefs.Value.Length == 0)
            return result with { UpgradedBodyFacts = bodyFacts.ToImmutableArray() };

        CSharpCompilation? compilation = null;
        try
        {
            var allRefs = ImmutableArray.CreateBuilder<MetadataReference>();
            allRefs.AddRange(FrameworkRefs.Value);
            allRefs.AddRange(nugetRefs);

            compilation = CSharpCompilation.Create("DevContextSemanticsLite",
                allTrees,
                allRefs.ToImmutable(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }
        catch (Exception)
        {
            return result with { UpgradedBodyFacts = bodyFacts.ToImmutableArray() };
        }

        var upgraded = bodyFacts.ToImmutableArray();
        if (bodyFacts.Count > 0)
            upgraded = UpgradeBodyFacts(bodyFacts, compilation, allTrees, fileToProject, result);

        result = result with
        {
            Compilation = compilation,
            UpgradedBodyFacts = upgraded,
        };

        sw.Stop();
        return result;
    }

    /// <summary>Reads <c>obj/project.assets.json</c> for each project and resolves NuGet DLL paths
    /// from the package folders and library file listings. Returns metadata references for the
    /// compilation. Uses the `packageFolders` and `libraries` sections of the lock file.
    /// Where assets.json is missing, that project degrades to Tier A.</summary>
    private static ImmutableArray<MetadataReference> ResolveNuGetMetadataRefs(
        IReadOnlyList<ProjectInfo> projects, string rootPath, SemanticLiteResult result)
    {
        var dllPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var assetsProjects = 0;
        var degradedProjects = 0;

        foreach (var proj in projects)
        {
            var projDir = Path.GetDirectoryName(proj.FilePath);
            if (projDir is null) continue;
            var assetsPath = Path.Combine(projDir, "obj", "project.assets.json");
            if (!File.Exists(assetsPath))
            {
                degradedProjects++;
                continue;
            }

            try
            {
                var json = File.ReadAllText(assetsPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var packageFolders = new List<string>();
                if (root.TryGetProperty("packageFolders", out var pf))
                {
                    foreach (var folder in pf.EnumerateObject())
                        packageFolders.Add(folder.Name);
                }

                if (root.TryGetProperty("libraries", out var libraries))
                {
                    foreach (var lib in libraries.EnumerateObject())
                    {
                        if (!lib.Value.TryGetProperty("type", out var typeEl) || typeEl.GetString() != "package")
                            continue;
                        if (!lib.Value.TryGetProperty("path", out var pathEl))
                            continue;
                        if (!lib.Value.TryGetProperty("files", out var filesEl))
                            continue;

                        var relPath = pathEl.GetString() ?? "";
                        foreach (var filePath in filesEl.EnumerateArray())
                        {
                            var fp = filePath.GetString();
                            if (fp is null || !fp.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (!fp.Contains("lib/", StringComparison.OrdinalIgnoreCase))
                                continue;

                            foreach (var folder in packageFolders)
                            {
                                var full = Path.Combine(folder, relPath, fp.Replace('/', Path.DirectorySeparatorChar));
                                if (!File.Exists(full)) continue;
                                dllPaths.Add(full);
                                break;
                            }
                        }
                    }
                }

                assetsProjects++;
            }
            catch
            {
                degradedProjects++;
            }
        }

        result = result with
        {
            ProjectsWithAssets = assetsProjects,
            ProjectsDegraded = degradedProjects,
        };

        var refs = ImmutableArray.CreateBuilder<MetadataReference>();
        foreach (var dllPath in dllPaths)
        {
            try { refs.Add(MetadataReference.CreateFromFile(dllPath)); }
            catch { }
        }
        return refs.ToImmutable();
    }

    /// <summary>For each <see cref="BodyFacts"/>, uses <see cref="SemanticModel"/> to upgrade
    /// <see cref="LocalDeclOp.InferredFrom"/> and <see cref="InvocationOp.ReceiverType"/>.
    /// Law R2: only upgrades (Syntactic → Semantic), never downgrades.</summary>
    private static ImmutableArray<BodyFacts> UpgradeBodyFacts(
        IReadOnlyList<BodyFacts> facts,
        CSharpCompilation compilation,
        List<SyntaxTree> allTrees,
        Dictionary<string, string?> fileToProject,
        SemanticLiteResult result)
    {
        var treeIndex = new Dictionary<string, SyntaxTree>(StringComparer.OrdinalIgnoreCase);
        foreach (var tree in allTrees)
            treeIndex[tree.FilePath] = tree;

        var varDeclsResolved = 0;
        var receiversResolved = 0;
        var upgraded = ImmutableArray.CreateBuilder<BodyFacts>();

        foreach (var body in facts)
        {
            if (string.IsNullOrEmpty(body.File) || !treeIndex.TryGetValue(body.File, out var tree))
            {
                upgraded.Add(body);
                continue;
            }

            SemanticModel? semanticModel = null;
            try { semanticModel = compilation.GetSemanticModel(tree); }
            catch { }

            if (semanticModel is null)
            {
                upgraded.Add(body);
                continue;
            }

            var ops = body.Ops;
            var changed = false;

            for (var i = 0; i < ops.Length; i++)
            {
                var op = ops[i];

                if (op is LocalDeclOp local && local.InferredFrom is null)
                {
                    var upgradedRef = TryResolveLocalDeclType(local, tree, semanticModel);
                    if (upgradedRef is not null)
                    {
                        ops = ops.SetItem(i, local with { InferredFrom = upgradedRef });
                        changed = true;
                        Interlocked.Increment(ref varDeclsResolved);
                    }
                }
                else if (op is InvocationOp inv && inv.ReceiverType is null)
                {
                    var upgradedRef = TryResolveReceiverType(inv, tree, semanticModel);
                    if (upgradedRef is not null)
                    {
                        ops = ops.SetItem(i, inv with { ReceiverType = upgradedRef });
                        changed = true;
                        Interlocked.Increment(ref receiversResolved);
                    }
                }
                else if (op is InvocationOp inv2 && inv2.ReceiverType is { Tier: < ResolutionTier.Semantic } r)
                {
                    var upgradedRef = TryResolveReceiverType(inv2, tree, semanticModel);
                    if (upgradedRef is not null && upgradedRef.Resolved is not null)
                    {
                        ops = ops.SetItem(i, inv2 with { ReceiverType = r with { Resolved = upgradedRef.Resolved, Tier = ResolutionTier.Semantic } });
                        changed = true;
                        Interlocked.Increment(ref receiversResolved);
                    }
                }
            }

            upgraded.Add(changed ? body with { Ops = ops } : body);
        }

        return upgraded.ToImmutable();
    }

    /// <summary>Uses the <see cref="SemanticModel"/> to find the local declaration on the given line
    /// and resolve the type of its initializer. Returns a <see cref="SymbolRef"/> at
    /// <see cref="ResolutionTier.Semantic"/> when the type can be determined.</summary>
    private static SymbolRef? TryResolveLocalDeclType(
        LocalDeclOp local, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var lineSpan = tree.GetText().Lines[Math.Max(0, local.Line - 1)];
            var span = lineSpan.Span;
            var node = root.FindNode(span);

            var decl = node?.AncestorsAndSelf()
                .OfType<LocalDeclarationStatementSyntax>()
                .FirstOrDefault();
            if (decl is null) return null;

            foreach (var variable in decl.Declaration.Variables)
            {
                if (!string.Equals(variable.Identifier.ValueText, local.Name, StringComparison.Ordinal))
                    continue;
                if (variable.Initializer?.Value is null) continue;

                var typeInfo = model.GetTypeInfo(variable.Initializer.Value);
                if (typeInfo.Type is null || typeInfo.Type is IErrorTypeSymbol) continue;

                var canon = typeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                return new SymbolRef
                {
                    Text = canon,
                    Site = new RefSite
                    {
                        File = tree.FilePath,
                        Line = local.Line,
                        Project = "",
                    },
                    Resolved = new SymbolId(SymbolKind.Type, canon),
                    Tier = ResolutionTier.Semantic,
                };
            }
        }
        catch { }
        return null;
    }

    /// <summary>Uses the <see cref="SemanticModel"/> to find the invocation on the given line
    /// and resolve the receiver's type. Returns a <see cref="SymbolRef"/> at
    /// <see cref="ResolutionTier.Semantic"/> when the type can be determined.</summary>
    private static SymbolRef? TryResolveReceiverType(
        InvocationOp inv, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var lineSpan = tree.GetText().Lines[Math.Max(0, inv.Line - 1)];
            var span = lineSpan.Span;
            var node = root.FindNode(span);

            var invocation = node?.AncestorsAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();
            if (invocation is null) return null;

            var receiver = GetReceiverSyntax(invocation);
            if (receiver is null) return null;

            var typeInfo = model.GetTypeInfo(receiver);
            if (typeInfo.Type is null || typeInfo.Type is IErrorTypeSymbol) return null;

            var canon = typeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
            return new SymbolRef
            {
                Text = canon,
                Site = new RefSite
                {
                    File = tree.FilePath,
                    Line = inv.Line,
                    Project = "",
                },
                Resolved = new SymbolId(SymbolKind.Type, canon),
                Tier = ResolutionTier.Semantic,
            };
        }
        catch { }
        return null;
    }

    private static ExpressionSyntax? GetReceiverSyntax(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            return memberAccess.Expression;
        if (invocation.Expression is MemberBindingExpressionSyntax) { }
        return invocation.Expression;
    }
}
