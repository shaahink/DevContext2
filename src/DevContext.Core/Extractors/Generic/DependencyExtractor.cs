namespace DevContext.Core.Extractors.Generic;

using DevContext.Core.Graph.EntrySurfaces;

/// <summary>Detects NuGet package references to emit architecture signals and builds the project dependency graph.</summary>
[ExtractorOrder(10)]
public sealed class DependencyExtractor : IDiscoveryExtractor
{
    private static readonly FrozenDictionary<string, string> PackageSignalMap = BuildPackageSignalMap();
    private static readonly FrozenDictionary<string, string> ProjectNameSignalMap = BuildProjectNameSignalMap();
    private static readonly FrozenDictionary<string, string> SdkSignalMap = BuildSdkSignalMap();
    private static readonly ImmutableArray<EntrySurfaceDescriptor> AppEntryDescriptors = EntrySurfaceCatalog.All
        .Where(d => d.Kind is not null && d.Role == SurfaceRole.AppEntry)
        .ToImmutableArray();

    private static FrozenDictionary<string, string> BuildPackageSignalMap()
    {
        var map = new Dictionary<string, string>();
        foreach (var d in EntrySurfaceCatalog.All)
        {
            if (d.SignalKey.Length == 0) continue;
            foreach (var pkg in d.Packages)
                map[pkg] = d.SignalKey;
        }
        return map.ToFrozenDictionary();
    }

    private static FrozenDictionary<string, string> BuildProjectNameSignalMap()
    {
        var map = new Dictionary<string, string>();
        foreach (var d in EntrySurfaceCatalog.All)
        {
            if (d.SignalKey.Length == 0) continue;
            foreach (var pattern in d.SelfNamePatterns)
                map[pattern] = d.SignalKey;
        }
        return map.ToFrozenDictionary();
    }

    private static FrozenDictionary<string, string> BuildSdkSignalMap()
    {
        var map = new Dictionary<string, string>();
        foreach (var d in EntrySurfaceCatalog.All)
        {
            if (d.SignalKey.Length == 0) continue;
            foreach (var sdk in d.SdkHints)
                map[sdk] = d.SignalKey;
        }
        return map.ToFrozenDictionary();
    }

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "DependencyExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [.. PackageSignalMap.Values],
        ["model.Architecture", "context.Analysis.ProjectGraph"],
        "Detects NuGet package references and builds project dependency graph");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var adjacency = new Dictionary<string, ImmutableArray<string>>();

        // ── F2: Code-based signal detection (MUST run first) ────────────────
        // A framework's own source doesn't reference itself as a NuGet package.
        // Detect signals from project/solution names for self-referencing repos.
        // These self-source keys are remembered so PackageReference-based detection
        // below cannot override them — a framework repo's own extension libraries
        // that reference the core package via NuGet must not flip the archetype.
        var selfSourcedKeys = new HashSet<string>();
        foreach (var projectInfo in model.Projects)
        {
            ct.ThrowIfCancellationRequested();
            if (TryMatchSignalFromProjectName(projectInfo.Name, out var signalKey, out var matchedName))
            {
                model.Architecture.Register(FeatureSignal.CreateDetected(
                    signalKey, confidence: 0.7f, via: "ProjectName", matchedName));
                selfSourcedKeys.Add(signalKey);
            }
        }

        foreach (var projectInfo in model.Projects)
        {
            ct.ThrowIfCancellationRequested();

            foreach (var pkg in projectInfo.PackageReferences)
            {
                if (TryMatchSignal(pkg.Name, out var signalKey, out var matchedKey))
                {
                    if (selfSourcedKeys.Contains(signalKey)
                        || Graph.ProjectClassifier.IsSamplePath(projectInfo.FilePath)
                        || Graph.ProjectClassifier.IsTestPath(projectInfo.FilePath))
                        continue;

                    model.Architecture.Register(FeatureSignal.CreateDetected(
                        signalKey, confidence: 1.0f, via: "PackageReference", matchedKey));
                }
            }

            var refs = projectInfo.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r))
                .Where(r => !string.IsNullOrEmpty(r))
                .ToImmutableArray();

            adjacency[projectInfo.Name] = refs;

            var csprojPath = projectInfo.FilePath;
            if (!string.IsNullOrEmpty(csprojPath) && context.Cache.KnownFilePaths.Contains(csprojPath))
            {
                try
                {
                    var doc = await context.Cache.GetXmlAsync(csprojPath, ct);

                    // Detect signals from project SDK via catalog
                    var sdk = doc.Root?.Attribute("Sdk")?.Value;
                    if (sdk is not null
                        && SdkSignalMap.TryGetValue(sdk, out var sdkSignalKey)
                        && !selfSourcedKeys.Contains(sdkSignalKey))
                    {
                        model.Architecture.Register(FeatureSignal.CreateDetected(
                            sdkSignalKey, confidence: 0.9f, via: "ProjectSdk", sdk));
                    }

                    // Detect WPF desktop apps from SDK (WinExe + Microsoft.NET.Sdk requires extra check)
                    var outputType = projectInfo.OutputType;
                    var isWinExe = outputType is { } ot && ot.Contains("WinExe", StringComparison.OrdinalIgnoreCase);
                    if (sdk == "Microsoft.NET.Sdk" && isWinExe
                        && !selfSourcedKeys.Contains(ArchitectureSignals.Keys.DesktopUi))
                    {
                        model.Architecture.Register(FeatureSignal.CreateDetected(
                            ArchitectureSignals.Keys.DesktopUi, confidence: 0.9f, via: "ProjectSdk", outputType ?? "WinExe"));
                    }

                    var packageRefs = doc.Descendants("PackageReference")
                        .Select(r => r.Attribute("Include")?.Value ?? r.Attribute("Update")?.Value ?? "")
                        .Where(v => !string.IsNullOrEmpty(v));

                    foreach (var pkgName in packageRefs)
                    {
                        if (TryMatchSignal(pkgName, out var extraSignalKey, out var matchedKey2))
                        {
                            if (selfSourcedKeys.Contains(extraSignalKey)
                                || Graph.ProjectClassifier.IsSamplePath(csprojPath)
                                || Graph.ProjectClassifier.IsTestPath(csprojPath))
                                continue;

                            model.Architecture.Register(FeatureSignal.CreateDetected(
                                extraSignalKey, confidence: 1.0f, via: "PackageReference", matchedKey2));
                        }
                    }

                    // Also check ProjectReference elements for signal-bearing projects
                    var projectRefs = doc.Descendants("ProjectReference")
                        .Select(r => r.Attribute("Include")?.Value ?? "")
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Select(v => Path.GetFileNameWithoutExtension(v))
                        .Where(v => !string.IsNullOrEmpty(v));

                    foreach (var projName in projectRefs)
                    {
                        if (PackageSignalMap.TryGetValue(projName, out var projSignalKey))
                        {
                            if (selfSourcedKeys.Contains(projSignalKey)
                                || Graph.ProjectClassifier.IsSamplePath(csprojPath)
                                || Graph.ProjectClassifier.IsTestPath(csprojPath))
                                continue;

                            model.Architecture.Register(FeatureSignal.CreateDetected(
                                projSignalKey, confidence: 0.9f, via: "ProjectReference", projName));
                        }
                    }
                }
                catch (Exception ex)
                {
                    model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {csprojPath}: {ex.Message}");
                }
            }
        }

        context.Analysis.ProjectGraph = new ProjectDependencyGraph(adjacency);
    }

    private static bool TryMatchSignalFromProjectName(string projectName, out string signalKey, out string matchedKey)
    {
        foreach (var (pattern, key) in ProjectNameSignalMap)
        {
            if (projectName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            {
                signalKey = key;
                matchedKey = pattern;
                return true;
            }
        }
        signalKey = null!;
        matchedKey = null!;
        return false;
    }

    /// <summary>Matches a package name against the signal map with exact and prefix strategies.</summary>
    private static bool TryMatchSignal(string packageName, out string signalKey, out string matchedKey)
    {
        if (PackageSignalMap.TryGetValue(packageName, out signalKey!))
        {
            matchedKey = packageName;
            return true;
        }
        foreach (var (key, value) in PackageSignalMap)
        {
            if (packageName.StartsWith(key, StringComparison.OrdinalIgnoreCase))
            {
                signalKey = value;
                matchedKey = key;
                return true;
            }
        }
        signalKey = null!;
        matchedKey = null!;
        return false;
    }
}
