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
    /// <summary>Number of syntax trees fed into the semantic-lite compilation.</summary>
    public int TreeCount { get; init; }
    /// <summary>Number of NuGet metadata references resolved from assets.json.</summary>
    public int ReferenceCount { get; init; }
    /// <summary>True when a <see cref="CSharpCompilation"/> was successfully constructed.</summary>
    public bool CompilationBuilt { get; init; }
    /// <summary>Diagnostic note when Tier B degraded (exception type/message); empty on success.</summary>
    public string DegradeReason { get; init; } = "";
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

        var (nugetRefs, assetsProjects, degradedProjects) = ResolveNuGetMetadataRefs(projects, rootPath);
        result = result with { ProjectsWithAssets = assetsProjects, ProjectsDegraded = degradedProjects };
        var allTrees = new List<SyntaxTree>();
        var fileToProject = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        // Map each source file to its most-specific owning project. Projects nest (a solution `src/` root
        // whose children live in `src/Services/Basket/…`), so a naive per-project prefix scan would add the
        // same tree twice — CSharpCompilation.Create rejects duplicate trees. Assign each file to the
        // project with the LONGEST matching directory prefix, then add every tree exactly once.
        var projectDirs = new List<(string Dir, string Name)>();
        foreach (var proj in projects)
        {
            var projDir = Path.GetDirectoryName(proj.FilePath);
            if (projDir is not null) projectDirs.Add((projDir, proj.Name));
        }
        projectDirs.Sort((a, b) => b.Dir.Length - a.Dir.Length);

        var perProject = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var path in cache.KnownFilePaths)
        {
            if (ct.IsCancellationRequested) break;
            if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;
            if (fileToProject.ContainsKey(path)) continue;

            string? owner = null;
            foreach (var (dir, name) in projectDirs)
            {
                if (path.StartsWith(dir, StringComparison.OrdinalIgnoreCase)) { owner = name; break; }
            }
            if (owner is null) continue;

            try
            {
                var tree = cache.GetSyntaxTreeAsync(path, ct).AsTask().GetAwaiter().GetResult();
                allTrees.Add(tree);
                fileToProject[path] = owner;
                perProject[owner] = perProject.TryGetValue(owner, out var c) ? c + 1 : 1;
            }
            catch { }
        }

        foreach (var (_, name) in projectDirs)
            if (!perProject.ContainsKey(name))
                result = result with { ProjectsSkipped = result.ProjectsSkipped + 1 };

        if (allTrees.Count == 0 || nugetRefs.Length == 0 && FrameworkRefs.Value.Length == 0)
            return result with
            {
                UpgradedBodyFacts = bodyFacts.ToImmutableArray(),
                TreeCount = allTrees.Count,
                ReferenceCount = nugetRefs.Length,
            };

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
        catch (Exception ex)
        {
            return result with
            {
                UpgradedBodyFacts = bodyFacts.ToImmutableArray(),
                TreeCount = allTrees.Count,
                ReferenceCount = nugetRefs.Length,
                DegradeReason = $"{ex.GetType().Name}: {ex.Message}",
            };
        }

        var upgraded = bodyFacts.ToImmutableArray();
        var varDeclsResolved = 0;
        var receiversResolved = 0;
        if (bodyFacts.Count > 0)
            (upgraded, varDeclsResolved, receiversResolved) =
                UpgradeBodyFacts(bodyFacts, compilation, allTrees, fileToProject);

        result = result with
        {
            Compilation = compilation,
            UpgradedBodyFacts = upgraded,
            VarDeclsResolved = varDeclsResolved,
            ReceiversResolved = receiversResolved,
            TreeCount = allTrees.Count,
            ReferenceCount = nugetRefs.Length,
            CompilationBuilt = true,
        };

        sw.Stop();
        return result;
    }

    /// <summary>Reads <c>obj/project.assets.json</c> for each project and resolves NuGet DLL paths
    /// from the package folders and library file listings. Returns metadata references for the
    /// compilation plus per-project tier routing counts (assets-present vs degraded-to-Tier-A).
    /// De-duplicates by assembly name (one target framework per assembly) so the compilation never
    /// sees two references with the same identity — which would poison semantic binding.</summary>
    private static (ImmutableArray<MetadataReference> Refs, int AssetsProjects, int DegradedProjects)
        ResolveNuGetMetadataRefs(IReadOnlyList<ProjectInfo> projects, string rootPath)
    {
        // assembly simple name → (tfm score, absolute dll path). Highest score wins.
        var best = new Dictionary<string, (int Score, string Path)>(StringComparer.OrdinalIgnoreCase);
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

                            var asmName = Path.GetFileNameWithoutExtension(fp);
                            var score = TfmScore(fp);
                            if (best.TryGetValue(asmName, out var cur) && cur.Score >= score)
                                continue;

                            foreach (var folder in packageFolders)
                            {
                                var full = Path.Combine(folder, relPath, fp.Replace('/', Path.DirectorySeparatorChar));
                                if (!File.Exists(full)) continue;
                                best[asmName] = (score, full);
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

        var refs = ImmutableArray.CreateBuilder<MetadataReference>();
        foreach (var (_, path) in best.Values)
        {
            try { refs.Add(MetadataReference.CreateFromFile(path)); }
            catch { }
        }
        return (refs.ToImmutable(), assetsProjects, degradedProjects);
    }

    /// <summary>Ranks a <c>lib/&lt;tfm&gt;/x.dll</c> path so the newest compatible target framework wins
    /// when a package ships several. Higher is better; unknown TFMs score lowest.</summary>
    private static int TfmScore(string libRelativePath)
    {
        var p = libRelativePath.ToLowerInvariant();
        if (p.Contains("/net9.")) return 90;
        if (p.Contains("/net8.")) return 80;
        if (p.Contains("/net7.")) return 70;
        if (p.Contains("/net6.")) return 60;
        if (p.Contains("/net5.")) return 50;
        if (p.Contains("/netcoreapp")) return 40;
        if (p.Contains("/netstandard2.1")) return 31;
        if (p.Contains("/netstandard2.0")) return 30;
        if (p.Contains("/netstandard")) return 20;
        if (p.Contains("/net4")) return 10;
        return 1;
    }

    /// <summary>For each <see cref="BodyFacts"/> in the demand set (bodies with a dispatch/creation/local
    /// worth binding), uses <see cref="SemanticModel"/> to upgrade <see cref="LocalDeclOp.InferredFrom"/>
    /// and <see cref="InvocationOp.ReceiverType"/> to <see cref="ResolutionTier.Semantic"/>. Two modes:
    /// <list type="bullet">
    /// <item><b>Fill</b> — the syntactic field was null; a real bind supplies it (short name for detector
    /// matching, bound FQN in <c>Resolved</c>).</item>
    /// <item><b>Confirm</b> — a syntactic field exists at a lower tier and the bind's short name agrees;
    /// the tier is upgraded to Semantic (identity unchanged, so the graph node never drifts — only the
    /// resolution tier improves, which the assembler turns into a <c>verified</c> edge).</item>
    /// </list>
    /// Law R2: only upgrades, never downgrades and never re-points an existing resolution to a different
    /// short name. Returns the upgraded facts plus per-kind upgrade counts for tier-routing stats.</summary>
    private static (ImmutableArray<BodyFacts> Facts, int VarDecls, int Receivers) UpgradeBodyFacts(
        IReadOnlyList<BodyFacts> facts,
        CSharpCompilation compilation,
        List<SyntaxTree> allTrees,
        Dictionary<string, string?> fileToProject)
    {
        var treeIndex = new Dictionary<string, SyntaxTree>(StringComparer.OrdinalIgnoreCase);
        foreach (var tree in allTrees)
            treeIndex[tree.FilePath] = tree;

        var modelCache = new Dictionary<SyntaxTree, SemanticModel?>();
        var varDeclsResolved = 0;
        var receiversResolved = 0;
        var upgraded = ImmutableArray.CreateBuilder<BodyFacts>();

        foreach (var body in facts)
        {
            if (string.IsNullOrEmpty(body.File)
                || !treeIndex.TryGetValue(body.File, out var tree)
                || !HasBindDemand(body))
            {
                upgraded.Add(body);
                continue;
            }

            if (!modelCache.TryGetValue(tree, out var semanticModel))
            {
                try { semanticModel = compilation.GetSemanticModel(tree); }
                catch { semanticModel = null; }
                modelCache[tree] = semanticModel;
            }

            if (semanticModel is null)
            {
                upgraded.Add(body);
                continue;
            }

            var ops = body.Ops;
            var changed = false;

            for (var i = 0; i < ops.Length; i++)
            {
                switch (ops[i])
                {
                    case LocalDeclOp local when local.InferredFrom is not { Tier: ResolutionTier.Semantic }:
                    {
                        var bound = TryBindLocalDeclType(local, tree, semanticModel);
                        var merged = MergeSemantic(local.InferredFrom, bound, local.Line, tree.FilePath);
                        if (merged is not null)
                        {
                            ops = ops.SetItem(i, local with { InferredFrom = merged });
                            changed = true;
                            varDeclsResolved++;
                        }
                        break;
                    }
                    case InvocationOp inv when inv.ReceiverType is not { Tier: ResolutionTier.Semantic }:
                    {
                        var bound = TryBindReceiverType(inv, tree, semanticModel);
                        var merged = MergeSemantic(inv.ReceiverType, bound, inv.Line, tree.FilePath);
                        if (merged is not null)
                        {
                            ops = ops.SetItem(i, inv with { ReceiverType = merged });
                            changed = true;
                            receiversResolved++;
                        }
                        break;
                    }
                }
            }

            upgraded.Add(changed ? body with { Ops = ops } : body);
        }

        return (upgraded.ToImmutable(), varDeclsResolved, receiversResolved);
    }

    /// <summary>Demand set (design §6 "bind lazily, only for members that own seam matches or ambiguous
    /// refs"): a body is worth binding only if it declares locals or invokes something with a receiver —
    /// the shapes that own dispatch/creation seams. Pure getters/branches with none are skipped so the
    /// semantic model is never materialised for them.</summary>
    private static bool HasBindDemand(BodyFacts body)
    {
        foreach (var op in body.Ops)
        {
            if (op is LocalDeclOp) return true;
            if (op is InvocationOp { ReceiverText: not null }) return true;
        }
        return false;
    }

    /// <summary>Applies Law R2 to a single ref: given the existing (syntactic) ref and a semantic bind
    /// <c>(short, fqn)</c>, returns the upgraded ref — or null if nothing should change.
    /// <list type="bullet">
    /// <item>existing null → <b>fill</b>: new Semantic ref (short Text for detector matching, bound FQN in Resolved).</item>
    /// <item>existing short name equals bound short name → <b>confirm</b>: same Text, tier→Semantic, Resolved set.</item>
    /// <item>short names disagree → no change (never re-point a resolution this tier; left for a later pass).</item>
    /// </list></summary>
    private static SymbolRef? MergeSemantic(SymbolRef? existing, (string Short, string Fqn)? bound, int line, string file)
    {
        if (bound is not { } b) return null;
        var resolved = new SymbolId(SymbolKind.Type, b.Fqn);

        if (existing is null)
            return new SymbolRef
            {
                Text = b.Short,
                Site = new RefSite { File = file, Line = line, Project = "" },
                Resolved = resolved,
                Tier = ResolutionTier.Semantic,
            };

        if (string.Equals(existing.Text, b.Short, StringComparison.Ordinal))
            return existing with { Resolved = resolved, Tier = ResolutionTier.Semantic };

        return null;
    }

    /// <summary>Binds the initializer of a local declaration and returns its <c>(short, fqn)</c> type when
    /// the semantic model resolves it to a real (non-error) named type; null otherwise.</summary>
    private static (string Short, string Fqn)? TryBindLocalDeclType(
        LocalDeclOp local, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var span = tree.GetText().Lines[Math.Max(0, local.Line - 1)].Span;
            var node = root.FindNode(span);

            var decl = node?.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
            if (decl is null) return null;

            foreach (var variable in decl.Declaration.Variables)
            {
                if (!string.Equals(variable.Identifier.ValueText, local.Name, StringComparison.Ordinal))
                    continue;
                if (variable.Initializer?.Value is null) continue;
                return NamedType(model.GetTypeInfo(variable.Initializer.Value).Type);
            }
        }
        catch { }
        return null;
    }

    /// <summary>Binds the receiver expression of an invocation and returns its <c>(short, fqn)</c> type
    /// when the semantic model resolves it to a real (non-error) named type; null otherwise.</summary>
    private static (string Short, string Fqn)? TryBindReceiverType(
        InvocationOp inv, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var span = tree.GetText().Lines[Math.Max(0, inv.Line - 1)].Span;
            var node = root.FindNode(span);

            var invocation = node?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation is null) return null;

            var receiver = GetReceiverSyntax(invocation);
            if (receiver is null) return null;
            return NamedType(model.GetTypeInfo(receiver).Type);
        }
        catch { }
        return null;
    }

    /// <summary>Projects a resolved <see cref="ITypeSymbol"/> to <c>(shortName, fullyQualified)</c>,
    /// or null for null/error/unnamed types. Interface receivers (e.g. <c>ISender</c>) keep their
    /// declared short name so detector short-name catalogs still match.</summary>
    private static (string Short, string Fqn)? NamedType(ITypeSymbol? type)
    {
        if (type is null || type is IErrorTypeSymbol || type is not INamedTypeSymbol named) return null;
        var fqn = named.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        var shortName = named.Name;
        if (string.IsNullOrEmpty(shortName) || string.IsNullOrEmpty(fqn)) return null;
        return (shortName, fqn);
    }

    private static ExpressionSyntax? GetReceiverSyntax(InvocationExpressionSyntax invocation)
        => invocation.Expression is MemberAccessExpressionSyntax memberAccess
            ? memberAccess.Expression
            : invocation.Expression;
}
