using System.Text.Json;

using DevContext.Core.Contracts;
using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

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
    /// <summary>Total number of <see cref="CreationOp"/> whose <c>Type</c> was resolved via semantic binding.</summary>
    public int CreationOpsResolved { get; init; }
    /// <summary>Total number of <see cref="InvocationOp"/> generic args resolved via semantic binding.</summary>
    public int GenericArgsResolved { get; init; }
    /// <summary>Total number of <see cref="InvocationOp"/> argument types (inline <c>new X()</c>/<c>Adapt&lt;T&gt;()</c>
    /// dispatch arguments) resolved via semantic binding.</summary>
    public int ArgTypesResolved { get; init; }
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
    /// <summary>Wall ms loading framework (TPA) metadata references — first analysis in a process pays it.</summary>
    public double FrameworkRefsMs { get; init; }
    /// <summary>Wall ms resolving NuGet metadata references (assets.json parse + dll probe + load).</summary>
    public double NuGetRefsMs { get; init; }
    /// <summary>Wall ms collecting syntax trees from the analysis cache.</summary>
    public double CollectTreesMs { get; init; }
    /// <summary>Wall ms in <c>CSharpCompilation.Create</c> (lazy — near zero by design).</summary>
    public double CreateMs { get; init; }
    /// <summary>Wall ms semantically binding the BodyFacts demand set.</summary>
    public double BindMs { get; init; }

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
    /// <summary>The invocation verbs whose <c>Args[0]</c> a seam detector can actually consume
    /// (<see cref="Seams.SeamDetectorHelpers.ResolveArgTarget"/> call sites). Built from the detectors'
    /// own verb catalogs so the bind demand can never drift from what detection reads. Binding every
    /// argument of every invocation instead was the measured big-repo wall (DntSite: 390k arg binds,
    /// 79.7s of an 81.8s SemanticLite stage).</summary>
    internal static readonly HashSet<string> ArgDemandVerbs = BuildArgDemandVerbs();

    private static HashSet<string> BuildArgDemandVerbs()
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        set.UnionWith(Seams.MediatRDispatchDetector.Verbs);
        set.UnionWith(Seams.BusPublishDetector.Verbs);
        set.UnionWith(Seams.DomainEventRaiseDetector.RaiseVerbs);
        return set;
    }

    /// <summary>Framework reference assemblies loaded once from the TPA.</summary>
    private static readonly Lazy<(ImmutableArray<MetadataReference> Refs, HashSet<string> Names)> FrameworkRefs = new(() =>
    {
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrEmpty(tpa)) return ([], new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var refs = ImmutableArray.CreateBuilder<MetadataReference>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in tpa.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
            try
            {
                refs.Add(MetadataReference.CreateFromFile(path));
                names.Add(Path.GetFileNameWithoutExtension(path));
            }
            catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "metadata-ref", ex); }
        }
        return (refs.ToImmutable(), names);
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
        IReadOnlyList<SyntaxTree>? extraTrees = null,
        CancellationToken ct = default)
    {
        var result = new SemanticLiteResult();
        if (projects.Count == 0) return result;

        // Force the (process-wide, lazy) framework refs under their own clock so the NuGet timing
        // below doesn't silently absorb the first-run TPA load.
        var swFw = System.Diagnostics.Stopwatch.StartNew();
        _ = FrameworkRefs.Value;
        swFw.Stop();

        var swNuGet = System.Diagnostics.Stopwatch.StartNew();
        var (nugetRefs, assetsProjects, degradedProjects) = ResolveNuGetMetadataRefs(projects, rootPath);
        swNuGet.Stop();
        result = result with
        {
            ProjectsWithAssets = assetsProjects,
            ProjectsDegraded = degradedProjects,
            FrameworkRefsMs = swFw.Elapsed.TotalMilliseconds,
            NuGetRefsMs = swNuGet.Elapsed.TotalMilliseconds,
        };
        var swTrees = System.Diagnostics.Stopwatch.StartNew();
        var allTrees = new List<SyntaxTree>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
            if (!seenPaths.Add(path)) continue;

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
                perProject[owner] = perProject.TryGetValue(owner, out var c) ? c + 1 : 1;
            }
            catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "syntax-parse", ex); }
        }

        foreach (var (_, name) in projectDirs)
            if (!perProject.ContainsKey(name))
                result = result with { ProjectsSkipped = result.ProjectsSkipped + 1 };

        // Batch A: extra virtual trees (Blazor @code) join THE compilation so component bodies get
        // real semantic upgrades — there is no second compilation to fold them into anymore.
        if (extraTrees is not null)
            foreach (var tree in extraTrees)
                if (!string.IsNullOrEmpty(tree.FilePath) && seenPaths.Add(tree.FilePath))
                    allTrees.Add(tree);
        swTrees.Stop();
        result = result with { CollectTreesMs = swTrees.Elapsed.TotalMilliseconds };

        if (allTrees.Count == 0 || nugetRefs.Length == 0 && FrameworkRefs.Value.Refs.Length == 0)
            return result with
            {
                UpgradedBodyFacts = bodyFacts.ToImmutableArray(),
                TreeCount = allTrees.Count,
                ReferenceCount = nugetRefs.Length,
            };

        CSharpCompilation? compilation = null;
        var swCreate = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var allRefs = ImmutableArray.CreateBuilder<MetadataReference>();
            allRefs.AddRange(FrameworkRefs.Value.Refs);
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
        swCreate.Stop();

        var swBind = System.Diagnostics.Stopwatch.StartNew();
        var upgraded = bodyFacts.ToImmutableArray();
        var varDeclsResolved = 0;
        var receiversResolved = 0;
        var creationOpsResolved = 0;
        var genericArgsResolved = 0;
        var argTypesResolved = 0;
        if (bodyFacts.Count > 0)
            (upgraded, varDeclsResolved, receiversResolved, creationOpsResolved, genericArgsResolved, argTypesResolved) =
                UpgradeBodyFacts(bodyFacts, compilation, allTrees, ct);
        swBind.Stop();

        result = result with
        {
            Compilation = compilation,
            UpgradedBodyFacts = upgraded,
            VarDeclsResolved = varDeclsResolved,
            ReceiversResolved = receiversResolved,
            CreationOpsResolved = creationOpsResolved,
            GenericArgsResolved = genericArgsResolved,
            ArgTypesResolved = argTypesResolved,
            TreeCount = allTrees.Count,
            ReferenceCount = nugetRefs.Length,
            CompilationBuilt = true,
            CreateMs = swCreate.Elapsed.TotalMilliseconds,
            BindMs = swBind.Elapsed.TotalMilliseconds,
        };

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
            catch (Exception ex)
            {
                degradedProjects++;
                PipelineDiagnostics.Swallowed("SemanticLitePopulator", "assets-json", ex);
            }
        }

        var fwNames = FrameworkRefs.Value.Names;

        var refs = ImmutableArray.CreateBuilder<MetadataReference>();
        foreach (var (name, (_, path)) in best)
        {
            if (fwNames.Contains(name)) continue;
            try { refs.Add(MetadataReference.CreateFromFile(path)); }
            catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "metadata-ref", ex); }
        }
        return (refs.ToImmutable(), assetsProjects, degradedProjects);
    }

    /// <summary>Ranks a <c>lib/&lt;tfm&gt;/x.dll</c> path so the newest compatible target framework wins
    /// when a package ships several. Higher is better; unknown TFMs score lowest.
    /// Parses <c>netMAJOR.MINOR</c> generically (net5.0–net99.0) so the repo's own TFM
    /// never falls off the end of a hard-coded list.</summary>
    internal static int TfmScore(string libRelativePath)
    {
        var s = libRelativePath.AsSpan();
        var idx = s.LastIndexOf("/net".AsSpan(), StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var tfm = s.Slice(idx + 4);
            if (tfm.Length > 0 && tfm[0] >= '0' && tfm[0] <= '9')
            {
                var dotIdx = tfm.IndexOf('.');
                if (dotIdx > 0
                    && dotIdx + 1 < tfm.Length
                    && tfm[dotIdx + 1] >= '0' && tfm[dotIdx + 1] <= '9'
                    && int.TryParse(tfm.Slice(0, dotIdx), out var major))
                {
                    var minorEnd = dotIdx + 1;
                    while (minorEnd < tfm.Length && tfm[minorEnd] >= '0' && tfm[minorEnd] <= '9')
                        minorEnd++;
                    if (int.TryParse(tfm.Slice(dotIdx + 1, minorEnd - dotIdx - 1), out var minor))
                        return major * 10 + minor;
                }
            }
        }
        var p = libRelativePath.ToLowerInvariant();
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
    private static (ImmutableArray<BodyFacts> Facts, int VarDecls, int Receivers, int Creations, int GenericArgs, int ArgTypes) UpgradeBodyFacts(
        IReadOnlyList<BodyFacts> facts,
        CSharpCompilation compilation,
        List<SyntaxTree> allTrees,
        CancellationToken ct = default)
    {
        var treeIndex = new Dictionary<string, SyntaxTree>(StringComparer.OrdinalIgnoreCase);
        foreach (var tree in allTrees)
            treeIndex[tree.FilePath] = tree;

        // Split pass-throughs from the bind demand set, keeping every body at its original index so
        // the parallel pass below cannot reorder output (results land in disjoint slots).
        var results = new BodyFacts[facts.Count];
        var demand = new List<(int Index, BodyFacts Body, SyntaxTree Tree)>();
        for (var i = 0; i < facts.Count; i++)
        {
            var body = facts[i];
            if (string.IsNullOrEmpty(body.File)
                || !treeIndex.TryGetValue(body.File, out var tree)
                || !HasBindDemand(body))
                results[i] = body;
            else
                demand.Add((i, body, tree));
        }

        var varDeclsResolved = 0;
        var receiversResolved = 0;
        var creationOpsResolved = 0;
        var genericArgsResolved = 0;
        var argTypesResolved = 0;

        // The bind is the measured wall of big-repo analysis (DntSite: 79.7s of an 81.8s SemanticLite
        // stage, serial) and is CPU-bound. Parallel BY TREE — each task binds one file's bodies against
        // its own GetSemanticModel, per-file semantic-model isolation; no
        // SemanticModel is ever shared across threads.
        var parallelOpts = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount };
        Parallel.ForEach(demand.GroupBy(d => d.Tree), parallelOpts, group =>
        {
            // Razor virtual trees: op lines are #line-MAPPED (true razor lines) but the line-based
            // node relookup below reads the VIRTUAL tree text — a mismatch that could bind the wrong
            // node. Skip the upgrade for these bodies (their facts stay honestly syntactic).
            if (group.Key.FilePath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var (index, body, _) in group) results[index] = body;
                return;
            }

            SemanticModel? semanticModel;
            try { semanticModel = compilation.GetSemanticModel(group.Key); }
            catch (Exception ex) { semanticModel = null; PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-model", ex); }

            var localVarDecls = 0;
            var localReceivers = 0;
            var localCreations = 0;
            var localGenericArgs = 0;
            var localArgTypes = 0;

            foreach (var (index, body, tree) in group)
            {
                if (semanticModel is null)
                {
                    results[index] = body;
                    continue;
                }

                results[index] = UpgradeOneBody(body, tree, semanticModel,
                    ref localVarDecls, ref localReceivers, ref localCreations, ref localGenericArgs, ref localArgTypes);
            }

            Interlocked.Add(ref varDeclsResolved, localVarDecls);
            Interlocked.Add(ref receiversResolved, localReceivers);
            Interlocked.Add(ref creationOpsResolved, localCreations);
            Interlocked.Add(ref genericArgsResolved, localGenericArgs);
            Interlocked.Add(ref argTypesResolved, localArgTypes);
        });

        return (ImmutableArray.Create(results), varDeclsResolved, receiversResolved, creationOpsResolved, genericArgsResolved, argTypesResolved);
    }

    /// <summary>Upgrades a single body's ops against the (task-local) semantic model. Pure over its
    /// inputs apart from the count refs; identical op-by-op logic to the pre-parallel serial loop.</summary>
    private static BodyFacts UpgradeOneBody(
        BodyFacts body, SyntaxTree tree, SemanticModel semanticModel,
        ref int varDeclsResolved, ref int receiversResolved, ref int creationOpsResolved,
        ref int genericArgsResolved, ref int argTypesResolved)
    {
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
                        // F1 (#33): a contradiction UNRESOLVES the ref — an honest change, not a resolution.
                        if (merged.Tier == ResolutionTier.Semantic) varDeclsResolved++;
                    }
                    break;
                }
                case InvocationOp inv:
                {
                    var newInv = inv;
                    var invChanged = false;

                    // (a) Receiver type — gates dispatch detection (ISender/IMediator etc.).
                    if (inv.ReceiverType is not { Tier: ResolutionTier.Semantic })
                    {
                        var bound = TryBindReceiverType(inv, tree, semanticModel);
                        var merged = MergeSemantic(inv.ReceiverType, bound, inv.Line, tree.FilePath);
                        if (merged is not null)
                        {
                            newInv = newInv with { ReceiverType = merged };
                            invChanged = true;
                            if (merged.Tier == ResolutionTier.Semantic) receiversResolved++;
                        }
                    }

                    // (b) Generic type arguments (e.g. Adapt<T>, Map<T>) — bound directly (assembly-independent).
                    if (newInv.GenericArgs.Length > 0)
                    {
                        var gargs = newInv.GenericArgs;
                        var gargsChanged = false;
                        for (var gi = 0; gi < gargs.Length; gi++)
                        {
                            if (gargs[gi].Tier == ResolutionTier.Semantic) continue;
                            var bound = TryBindGenericArg(inv, gi, tree, semanticModel);
                            var merged = MergeSemantic(gargs[gi], bound, inv.Line, tree.FilePath);
                            if (merged is not null)
                            {
                                gargs = gargs.SetItem(gi, merged);
                                gargsChanged = true;
                                if (merged.Tier == ResolutionTier.Semantic) genericArgsResolved++;
                            }
                        }
                        if (gargsChanged) { newInv = newInv with { GenericArgs = gargs }; invChanged = true; }
                    }

                    // (c) Argument types — the inline dispatch target `sender.Send(new XCommand(..))` /
                    //     `sender.Send(request.Adapt<XCommand>())`, where there is no `var` local to carry
                    //     the type. Binding the argument expression (or its mapping generic arg) makes the
                    //     dispatched contract verified. Demand-scoped to what detection consumes: only
                    //     Args[0] of a dispatch/publish/raise verb is ever read (ResolveArgTarget call
                    //     sites), so only that is bound.
                    if (!newInv.Args.IsDefaultOrEmpty
                        && ArgDemandVerbs.Contains(newInv.MethodName)
                        && newInv.Args[0].Type is not { Tier: ResolutionTier.Semantic })
                    {
                        var bound = TryBindArgType(inv, 0, tree, semanticModel);
                        var merged = MergeSemantic(newInv.Args[0].Type, bound, inv.Line, tree.FilePath);
                        if (merged is not null)
                        {
                            newInv = newInv with { Args = newInv.Args.SetItem(0, newInv.Args[0] with { Type = merged }) };
                            invChanged = true;
                            if (merged.Tier == ResolutionTier.Semantic) argTypesResolved++;
                        }
                    }

                    if (invChanged) { ops = ops.SetItem(i, newInv); changed = true; }
                    break;
                }
                case CreationOp cr when cr.Type is not { Tier: ResolutionTier.Semantic }:
                {
                    var bound = TryBindCreationType(cr, tree, semanticModel);
                    var merged = MergeSemantic(cr.Type, bound, cr.Line, tree.FilePath);
                    if (merged is not null)
                    {
                        ops = ops.SetItem(i, cr with { Type = merged });
                        changed = true;
                        if (merged.Tier == ResolutionTier.Semantic) creationOpsResolved++;
                    }
                    break;
                }
            }
        }

        return changed ? body with { Ops = ops } : body;
    }

    /// <summary>Binds the type of argument <paramref name="argIndex"/> of the invocation at <c>inv.Line</c>
    /// whose method name matches <c>inv.MethodName</c>. Uses <see cref="BindExpressionType"/> so an inline
    /// <c>new XCommand(..)</c> binds via the object-creation type and an inline <c>expr.Adapt&lt;T&gt;()</c>
    /// binds via its generic type argument (assembly-independent). Returns null when nothing resolves.</summary>
    private static (string Short, string Fqn)? TryBindArgType(
        InvocationOp inv, int argIndex, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var invocation = RelocateInvocation(root, tree, inv);
            if (invocation is null || argIndex >= invocation.ArgumentList.Arguments.Count) return null;

            return BindExpressionType(invocation.ArgumentList.Arguments[argIndex].Expression, model);
        }
        catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-bind", ex); }
        return null;
    }

    /// <summary>E1.2 (backlog #12) — relocates the syntax node an op was built from. Ops carry their OWN
    /// <see cref="BodyOp.Span"/>, so the node is found exactly, including when several ops share a line.
    /// The old line-span relocation stays as the fallback for any op produced without a span (or against a
    /// tree the span does not fit): it returns the innermost node containing the WHOLE line, which is the
    /// enclosing statement whenever the statement fits on one line — the defect itself.</summary>
    private static SyntaxNode? Relocate(SyntaxNode root, SyntaxTree tree, BodyOp op)
    {
        if (op.Span is { } span && root.FullSpan.Contains(span))
            return root.FindNode(span, getInnermostNodeForTie: true);

        var lines = tree.GetText().Lines;
        if (lines.Count == 0) return null;
        return root.FindNode(lines[Math.Clamp(op.Line - 1, 0, lines.Count - 1)].Span);
    }

    /// <summary>Relocates the invocation an <see cref="InvocationOp"/> was built from. With an exact span
    /// the relocated node IS the invocation; the method-name checks below only matter on the line-span
    /// fallback, where the node is a statement that may contain several calls.</summary>
    private static InvocationExpressionSyntax? RelocateInvocation(SyntaxNode root, SyntaxTree tree, InvocationOp op)
    {
        var node = Relocate(root, tree, op);
        if (node is null) return null;
        if (node is InvocationExpressionSyntax exact && MethodNameOf(exact) == op.MethodName) return exact;

        return node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>()
                   .FirstOrDefault(x => MethodNameOf(x) == op.MethodName)
            ?? node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                   .FirstOrDefault(x => MethodNameOf(x) == op.MethodName)
            ?? node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
    }

    /// <summary>The simple (unqualified) method name of an invocation expression.</summary>
    private static string MethodNameOf(InvocationExpressionSyntax inv) => inv.Expression switch
    {
        MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
        SimpleNameSyntax sn => sn.Identifier.ValueText,
        _ => "",
    };

    /// <summary>Binds the type of an object-creation expression at the given line and returns its
    /// <c>(short, fqn)</c> type when the semantic model resolves it to a real named type; null otherwise.</summary>
    private static (string Short, string Fqn)? TryBindCreationType(
        CreationOp creation, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var node = Relocate(root, tree, creation);

            var expr = node?.AncestorsAndSelf()
                           .FirstOrDefault(n => n is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax)
                           as ExpressionSyntax;
            if (expr is null) return null;
            return NamedType(model.GetTypeInfo(expr).Type);
        }
        catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-bind", ex); }
        return null;
    }

    /// <summary>Binds the type of a generic argument at index <c>argIndex</c> of an invocation and
    /// returns its <c>(short, fqn)</c> type when the semantic model resolves it; null otherwise.</summary>
    private static (string Short, string Fqn)? TryBindGenericArg(
        InvocationOp inv, int argIndex, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var invocation = RelocateInvocation(root, tree, inv);
            if (invocation is null) return null;

            var name = invocation.Expression switch
            {
                GenericNameSyntax gen => gen,
                MemberAccessExpressionSyntax ma => ma.Name as GenericNameSyntax,
                _ => null,
            };
            if (name is null || argIndex >= name.TypeArgumentList.Arguments.Count) return null;

            var arg = name.TypeArgumentList.Arguments[argIndex];
            return NamedType(model.GetTypeInfo(arg).Type);
        }
        catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-bind", ex); }
        return null;
    }

    private static bool HasBindDemand(BodyFacts body)
    {
        foreach (var op in body.Ops)
        {
            if (op is LocalDeclOp) return true;
            if (op is InvocationOp) return true;
            if (op is CreationOp) return true;
        }
        return false;
    }

    /// <summary>Applies Law R2 to a single ref: given the existing (syntactic) ref and a semantic bind
    /// <c>(short, fqn)</c>, returns the upgraded ref — or null if nothing should change.
    /// <list type="bullet">
    /// <item>existing null → <b>fill</b>: new Semantic ref (short Text for detector matching, bound FQN in Resolved).</item>
    /// <item>existing short name equals bound short name → <b>confirm</b>: same Text, tier→Semantic, Resolved set.</item>
    /// <item>short names disagree → <b>unresolve</b> (F1, #33): Roslyn measured a DIFFERENT type than
    /// the scope guessed, so the syntactic text is disproved at this site. The old behaviour — return
    /// null and let the wrong guess survive — was the smoking gun behind
    /// <c>AppDbContext::ConfigureAwait</c>: the name ladder re-resolved the disproved text and the
    /// binder consumed it as fact. The ref is marked <see cref="SymbolRef.Contradicted"/> so
    /// <see cref="SymbolTable.Resolve"/> never re-runs the ladder on it; it is deliberately NOT
    /// re-pointed at the bound type (never re-point a resolution this tier — when two witnesses
    /// disagree, say unknown).</item>
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

        return existing with
        {
            Resolved = null,
            Candidates = [],
            Tier = ResolutionTier.Unresolved,
            Contradicted = true,
        };
    }

    /// <summary>Binds the initializer of a local declaration and returns its <c>(short, fqn)</c> type when
    /// the semantic model resolves it to a real (non-error) named type; null otherwise.
    /// <para>Two strategies, in order: (1) bind the whole initializer expression (covers <c>new X()</c> and
    /// mapping calls when the mapping package is actually referenced); (2) when the initializer is a mapping
    /// call (<c>expr.Adapt&lt;T&gt;()</c>, <c>Map&lt;T&gt;()</c>, <c>Create&lt;T&gt;()</c>) with a single explicit
    /// generic type argument, bind THAT TYPE ARGUMENT directly. Strategy 2 is Roslyn name-resolution of the
    /// type <c>T</c> in its lexical context — it succeeds even when the mapping extension method's assembly is
    /// missing from disk (a partially-restored repo), which is the common eShop <c>Adapt&lt;Command&gt;</c>
    /// dispatch case. This is a genuine semantic bind (the type symbol is real and in-scope), not a name guess,
    /// so it earns the Semantic tier honestly.</para></summary>
    private static (string Short, string Fqn)? TryBindLocalDeclType(
        LocalDeclOp local, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var node = Relocate(root, tree, local);

            var decl = node?.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
            if (decl is null) return null;

            foreach (var variable in decl.Declaration.Variables)
            {
                if (!string.Equals(variable.Identifier.ValueText, local.Name, StringComparison.Ordinal))
                    continue;
                var init = variable.Initializer?.Value;
                if (init is null) continue;

                return BindExpressionType(init, model);
            }
        }
        catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-bind", ex); }
        return null;
    }

    /// <summary>Binds an expression to its <c>(short, fqn)</c> named type using two strategies, in order:
    /// (1) bind the whole expression (covers <c>new X()</c> and mapping calls when the mapping package is
    /// referenced); (2) when the expression is a mapping call (<c>expr.Adapt&lt;T&gt;()</c>, <c>Map&lt;T&gt;()</c>,
    /// <c>Create&lt;T&gt;()</c>) with a single explicit generic type argument, bind THAT TYPE ARGUMENT directly.
    /// Strategy 2 is Roslyn name-resolution of the type <c>T</c> in its lexical context — it succeeds even when
    /// the mapping extension method's assembly is missing from disk (a partially-restored repo), which is the
    /// common eShop <c>Adapt&lt;Command&gt;</c> dispatch case. Both are genuine semantic binds (real, in-scope
    /// type symbols), not name guesses, so they earn the Semantic tier honestly.</summary>
    private static (string Short, string Fqn)? BindExpressionType(ExpressionSyntax expr, SemanticModel model)
    {
        var whole = NamedType(model.GetTypeInfo(expr).Type);
        if (whole is not null) return whole;
        return TryBindMappingGenericArg(expr, model);
    }

    /// <summary>Mapping methods whose single generic type argument is the produced type (mirrors
    /// <c>BodyFactExtractor</c>'s <c>MappingMethods</c>). The type argument is bound directly so the produced
    /// type survives even when the mapping library assembly is not on disk.</summary>
    private static readonly HashSet<string> MappingMethods = new(StringComparer.Ordinal)
    {
        "Adapt", "Map", "MapTo", "MapFrom", "Create", "CreateFrom",
    };

    /// <summary>When <paramref name="expr"/> (possibly awaited) is a mapping call with a single explicit
    /// generic type argument, binds that type argument to its <c>(short, fqn)</c> named type; null otherwise.</summary>
    private static (string Short, string Fqn)? TryBindMappingGenericArg(ExpressionSyntax expr, SemanticModel model)
    {
        while (expr is AwaitExpressionSyntax ae) expr = ae.Expression;
        if (expr is not InvocationExpressionSyntax inv) return null;

        var name = inv.Expression switch
        {
            MemberAccessExpressionSyntax ma => ma.Name as GenericNameSyntax,
            GenericNameSyntax gn => gn,
            _ => null,
        };
        if (name is null
            || !MappingMethods.Contains(name.Identifier.ValueText)
            || name.TypeArgumentList.Arguments.Count != 1)
            return null;

        return NamedType(model.GetTypeInfo(name.TypeArgumentList.Arguments[0]).Type);
    }

    /// <summary>Binds the receiver expression of an invocation and returns its <c>(short, fqn)</c> type
    /// when the semantic model resolves it to a real (non-error) named type; null otherwise.</summary>
    private static (string Short, string Fqn)? TryBindReceiverType(
        InvocationOp inv, SyntaxTree tree, SemanticModel model)
    {
        try
        {
            var root = tree.GetRoot();
            var invocation = RelocateInvocation(root, tree, inv);
            if (invocation is null) return null;

            var receiver = GetReceiverSyntax(invocation);
            if (receiver is null) return null;
            return NamedType(model.GetTypeInfo(receiver).Type);
        }
        catch (Exception ex) { PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-bind", ex); }
        return null;
    }

    /// <summary>Projects a resolved <see cref="ITypeSymbol"/> to <c>(shortName, fullyQualified)</c>,
    /// or null for null/error/unnamed types. Interface receivers (e.g. <c>ISender</c>) keep their
    /// declared short name so detector short-name catalogs still match. The FQN is the
    /// <see cref="SymbolCanon"/> canonical (open-generic, arity-suffixed, nested chain) so a semantic
    /// bind of <c>IdentifiedCommand&lt;T, R&gt;</c> lands on the same node id its declaration produced.</summary>
    private static (string Short, string Fqn)? NamedType(ITypeSymbol? type)
    {
        if (type is null || type is IErrorTypeSymbol || type is not INamedTypeSymbol named) return null;
        var shortName = named.Name;
        if (string.IsNullOrEmpty(shortName)) return null;
        return (shortName, SymbolCanon.ForSymbol(named));
    }

    private static ExpressionSyntax? GetReceiverSyntax(InvocationExpressionSyntax invocation)
        => invocation.Expression is MemberAccessExpressionSyntax memberAccess
            ? memberAccess.Expression
            : invocation.Expression;
}
