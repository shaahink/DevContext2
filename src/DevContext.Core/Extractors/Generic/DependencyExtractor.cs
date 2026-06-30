namespace DevContext.Core.Extractors.Generic;

/// <summary>Detects NuGet package references to emit architecture signals and builds the project dependency graph.</summary>
[ExtractorOrder(10)]
public sealed class DependencyExtractor : IDiscoveryExtractor
{
    private static readonly FrozenDictionary<string, string> PackageSignalMap = new Dictionary<string, string>
    {
        ["MediatR"] = ArchitectureSignals.Keys.MediatR,
        ["Mediator"] = ArchitectureSignals.Keys.MediatR,
        ["Microsoft.EntityFrameworkCore"] = ArchitectureSignals.Keys.EfCore,
        ["Microsoft.AspNetCore.OpenApi"] = ArchitectureSignals.Keys.MinimalApis,
        ["MassTransit"] = ArchitectureSignals.Keys.MassTransit,
        ["Dapper"] = ArchitectureSignals.Keys.Dapper,
        ["FluentValidation"] = ArchitectureSignals.Keys.FluentValidation,
        ["Microsoft.EntityFrameworkCore.SqlServer"] = ArchitectureSignals.Keys.EfCore,
        ["Microsoft.EntityFrameworkCore.Sqlite"] = ArchitectureSignals.Keys.EfCore,
        ["Microsoft.EntityFrameworkCore.InMemory"] = ArchitectureSignals.Keys.EfCore,
        ["Npgsql.EntityFrameworkCore.PostgreSQL"] = ArchitectureSignals.Keys.EfCore,
        ["Pomelo.EntityFrameworkCore.MySql"] = ArchitectureSignals.Keys.EfCore,
        ["Hangfire"] = ArchitectureSignals.Keys.Hangfire,
        ["Scrutor"] = ArchitectureSignals.Keys.Scrutor,
        ["Refit"] = ArchitectureSignals.Keys.Refit,
        ["Aspire.Hosting"] = ArchitectureSignals.Keys.Aspire,
        ["FastEndpoints"] = ArchitectureSignals.Keys.FastEndpoints,
        ["Serilog"] = ArchitectureSignals.Keys.Serilog,
        ["Polly"] = ArchitectureSignals.Keys.Polly,
        ["AutoMapper"] = ArchitectureSignals.Keys.AutoMapper,
        ["Swashbuckle.AspNetCore"] = ArchitectureSignals.Keys.Swagger,
        ["Microsoft.AspNetCore.Identity"] = ArchitectureSignals.Keys.Identity,
        ["NLog"] = ArchitectureSignals.Keys.NLog,
        ["Quartz"] = ArchitectureSignals.Keys.Quartz,
        ["StackExchange.Redis"] = ArchitectureSignals.Keys.Redis,
        ["AspNetCore.HealthChecks"] = ArchitectureSignals.Keys.HealthChecks,
        ["Microsoft.WindowsAppSDK"] = ArchitectureSignals.Keys.DesktopUi,
        ["CommunityToolkit.WinUI"] = ArchitectureSignals.Keys.DesktopUi,
        ["CommunityToolkit.Mvvm"] = ArchitectureSignals.Keys.DesktopUi,
        ["Ocelot"] = ArchitectureSignals.Keys.Gateway,
        ["Microsoft.ReverseProxy"] = ArchitectureSignals.Keys.Gateway,
    }.ToFrozenDictionary();

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

        foreach (var projectInfo in model.Projects)
        {
            ct.ThrowIfCancellationRequested();

            foreach (var pkg in projectInfo.PackageReferences)
            {
                if (TryMatchSignal(pkg.Name, out var signalKey, out var matchedKey))
                {
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

                    // Detect minimal APIs from project SDK
                    var sdk = doc.Root?.Attribute("Sdk")?.Value;
                    if (sdk == "Microsoft.NET.Sdk.Web")
                    {
                        model.Architecture.Register(FeatureSignal.CreateDetected(
                            ArchitectureSignals.Keys.MinimalApis, confidence: 0.8f, via: "ProjectSdk", sdk));
                    }

                    // Detect WPF desktop apps from SDK + UseWPF property
                    var outputType = projectInfo.OutputType;
                    var isWinExe = outputType is { } ot && ot.Contains("WinExe", StringComparison.OrdinalIgnoreCase);
                    if (sdk == "Microsoft.NET.Sdk.WindowsDesktop"
                        || (sdk == "Microsoft.NET.Sdk" && isWinExe))
                    {
                        model.Architecture.Register(FeatureSignal.CreateDetected(
                            ArchitectureSignals.Keys.DesktopUi, confidence: 0.9f, via: "ProjectSdk", sdk ?? outputType ?? "WinExe"));
                    }

                    var packageRefs = doc.Descendants("PackageReference")
                        .Select(r => r.Attribute("Include")?.Value ?? r.Attribute("Update")?.Value ?? "")
                        .Where(v => !string.IsNullOrEmpty(v));

                    foreach (var pkgName in packageRefs)
                    {
                        if (TryMatchSignal(pkgName, out var extraSignalKey, out var matchedKey2))
                        {
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

        // ── F2: Code-based signal detection ────────────────────────────────
        // A framework's own source doesn't reference itself as a NuGet package.
        // Detect signals from project/solution names for self-referencing repos.
        foreach (var projectInfo in model.Projects)
        {
            ct.ThrowIfCancellationRequested();
            if (TryMatchSignalFromProjectName(projectInfo.Name, out var signalKey, out var matchedName))
                model.Architecture.Register(FeatureSignal.CreateDetected(
                    signalKey, confidence: 0.7f, via: "ProjectName", matchedName));
        }

        context.Analysis.ProjectGraph = new ProjectDependencyGraph(adjacency);
    }

    private static readonly Dictionary<string, string> ProjectNameSignalMap = new()
    {
        ["Microsoft.AspNetCore.SignalR"] = ArchitectureSignals.Keys.SignalR,
        ["Grpc"] = ArchitectureSignals.Keys.Grpc,
        ["MassTransit"] = ArchitectureSignals.Keys.MassTransit,
        ["Yarp"] = ArchitectureSignals.Keys.Gateway,
        ["ReverseProxy"] = ArchitectureSignals.Keys.Gateway,
        ["Orleans"] = ArchitectureSignals.Keys.Orleans,
        ["Microsoft.Orleans"] = ArchitectureSignals.Keys.Orleans,
        ["HotChocolate"] = ArchitectureSignals.Keys.GraphQL,
        ["GreenDonut"] = ArchitectureSignals.Keys.GraphQL,
        ["xunit"] = ArchitectureSignals.Keys.Testing,
        ["nunit"] = ArchitectureSignals.Keys.Testing,
    };

    private static bool TryMatchSignalFromProjectName(string projectName, out string signalKey, out string matchedKey)
    {
        foreach (var (pattern, key) in ProjectNameSignalMap)
        {
            if (projectName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)
                || projectName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
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
