using DevContext.Core.Extractors.Generic;
using DevContext.Core.Utilities;

namespace DevContext.Core.Graph;

/// <summary>The orientation artifact: architecture, topology, packages, entry inventory, cross-cutting — no code.</summary>
public sealed record MapModel
{
    public string Style { get; init; } = "Unknown";
    public float StyleConfidence { get; init; }
    public string? StyleEvidence { get; init; }
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    public ImmutableArray<ProjectNode> Topology { get; init; } = [];
    public ImmutableArray<PackageGroup> Packages { get; init; } = [];
    public ImmutableArray<string> Aggregates { get; init; } = [];
    public ImmutableArray<string> PipelineBehaviors { get; init; } = [];
    /// <summary>App vs Library — decides whether the entry-point Map or the public-surface view renders (G3).</summary>
    public Archetype Archetype { get; init; } = Archetype.App;
    /// <summary>The capability-grouped public API, when <see cref="Archetype"/> is Library.</summary>
    public LibrarySurface? Surface { get; init; }
    /// <summary>L7.2 — archetype-specific entry-point view (desktop/worker/library/blazor).</summary>
    public ArchetypeView? ArchetypeView { get; init; }
    /// <summary>When the analysed set is a partial closure of the owning solution (e.g. pointing at one
    /// microservice of many), a human-readable scope descriptor — keyed so the Map never claims a
    /// whole-system style from a single-service slice (Iteration 4 / Critical 3).</summary>
    public string? ScopeNote { get; init; }
    /// <summary>Gateway routes from ocelot.json / YARP config (W7).</summary>
    public ImmutableArray<GatewayRoute> Routes { get; init; } = [];
    /// <summary>Per-service style assessment (M1.9 / D5). One entry per runnable web project.</summary>
    public ImmutableArray<PerServiceStyle> ServiceStyles { get; init; } = [];
    /// <summary>R3 D-4 (G6.3) — runnable apps that live outside the analysed solution. Never services;
    /// the companion to <see cref="ScopeNote"/>, which says a choice was made but not what it cost.</summary>
    public ImmutableArray<PerServiceStyle> OutsideScopeApps { get; init; } = [];
    /// <summary>M1.2 — the solution-wide stack tags (runtime, web framework, CQRS, data, validation,
    /// messaging, aggregates), in render order. Previously computed inside <c>MapRenderer</c> and
    /// therefore reachable only through the markdown, while <c>MapResponse.stack</c> shipped empty to
    /// three consumers. One fact, one owner: the renderer and the wire now read the same list.</summary>
    public ImmutableArray<string> Stack { get; init; } = [];
}

public sealed record ProjectNode(string Name, ImmutableArray<string> DependsOn)
{
    public string? Layer { get; init; }
    public string? Feature { get; init; }
}

public sealed record PackageGroup(string Label, ImmutableArray<string> Packages);

public sealed class MapBuilder
{
    public static MapModel Build(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var archetype = ArchetypeDetector.Detect(model, entries);
        var topology = BuildTopology(model, graph);
        var aggregates = BuildAggregates(model);
        var archetypeView = archetype is Archetype.Desktop or Archetype.Worker
            or Archetype.Blazor or Archetype.Library or Archetype.CliTool
            ? new ArchetypeProjection().Project(graph,
                new ProjectionOptions { Archetype = archetype })
            : null;
        return new MapModel
        {
            Style = model.DetectedStyle.ToString(),
            StyleConfidence = model.StyleConfidence,
            StyleEvidence = model.StyleDetectedVia,
            Entries = entries,
            Topology = topology,
            Packages = BuildPackages(model.Projects),
            Aggregates = aggregates,
            Stack = BuildStack(model, aggregates),
            PipelineBehaviors = BuildPipelineBehaviors(model),
            Archetype = archetype,
            Surface = BuildSurface(model, archetype, entries),
            ArchetypeView = archetypeView,
            ScopeNote = BuildScopeNote(model, topology.Length),
            Routes = [.. model.GatewayRoutes],
            ServiceStyles = model.PerServiceStyles,
            OutsideScopeApps = model.OutsideScopeApps,
        };
    }

    /// <summary>M1.2 — the solution-wide stack tags, in render order. This list used to live inside
    /// <c>MapRenderer.AppendStack</c>, which meant the only way to see it was to read the markdown:
    /// <c>MapResponse.stack</c> had three consumers (identity strip, Atlas chip header, MCP overview)
    /// and no writer. Same order, same words as the markdown STACK line — the renderer now joins THIS.</summary>
    internal static ImmutableArray<string> BuildStack(DiscoveryModel model, ImmutableArray<string> aggregates)
    {
        var signals = model.Architecture.All;
        var parts = ImmutableArray.CreateBuilder<string>();

        // Runtime. E5 (Prism D1.4b): a multi-targeting library's TFM matrix summarizes to the newest
        // few + a count — Newtonsoft's raw `net20, net35, net40, …` ×9 dump was unreadable.
        var tfms = model.Projects
            .SelectMany(p => p.TargetFrameworks)
            .Where(f => !f.Contains("$(", StringComparison.Ordinal)) // drop unevaluated MSBuild vars (Low 16)
            .Distinct()
            .OrderBy(f => f)
            .ToList();
        if (tfms.Count > 0) parts.Add(SummarizeTfms(tfms));

        // Web framework
        if (signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) && ma.Detected)
            parts.Add("Minimal APIs");
        if (signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var ctrl) && ctrl.Detected)
            parts.Add("Controllers");
        if (signals.TryGetValue(ArchitectureSignals.Keys.FastEndpoints, out var fe) && fe.Detected)
            parts.Add("FastEndpoints");

        // CQRS / Mediator — light from handler evidence too (not just the package signal), so a scoped
        // sub-project whose handlers are present reads consistently with the resolved STYLE (G7 residual).
        // B6: a repo that declares its own IRequestHandler<,> hand-rolled the pattern — branding it
        // "MediatR" is a name-only match (podcasts has zero MediatR references).
        switch (ArchitectureStyleDetector.GetMediatREvidence(model))
        {
            case MediatREvidenceKind.Package: parts.Add("MediatR (CQRS)"); break;
            case MediatREvidenceKind.HandRolled: parts.Add("CQRS (hand-rolled mediator)"); break;
        }

        // Data
        if (signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var ef) && ef.Detected)
            parts.Add("EF Core");

        // Validation
        if (signals.TryGetValue(ArchitectureSignals.Keys.FluentValidation, out var fv) && fv.Detected)
            parts.Add("FluentValidation");

        // Messaging
        if (signals.TryGetValue(ArchitectureSignals.Keys.MassTransit, out var mt) && mt.Detected)
            parts.Add("MassTransit");
        if (signals.TryGetValue(ArchitectureSignals.Keys.NServiceBus, out var nsb) && nsb.Detected)
            parts.Add("NServiceBus");

        // Aggregates
        if (aggregates.Length > 0)
            parts.Add("DDD aggregates");

        return parts.ToImmutable();
    }

    /// <summary>E5: ≤3 distinct TFMs render verbatim (poles unchanged); a matrix shows the two most
    /// modern + a count. Modernity ranks family first (net5+ core &gt; netcoreapp &gt; netstandard &gt;
    /// classic net4x), then version — so "net6.0, netstandard2.0 +3 more TFMs", never a net20-first dump.</summary>
    internal static string SummarizeTfms(IReadOnlyList<string> tfms)
    {
        if (tfms.Count <= 3) return string.Join(", ", tfms);

        var ranked = tfms.OrderByDescending(TfmRank).ThenBy(t => t, StringComparer.Ordinal).ToList();
        return $"{ranked[0]}, {ranked[1]} +{tfms.Count - 2} more TFMs";
    }

    private static double TfmRank(string tfm)
    {
        // Strip a platform suffix ("net8.0-android" → "net8.0") for scoring; the display keeps it.
        var dash = tfm.IndexOf('-');
        var core = dash > 0 ? tfm[..dash] : tfm;

        static double Version(string s, string prefix)
            => double.TryParse(s[prefix.Length..], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;

        if (core.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase))
            return 1000 + Version(core, "netstandard");
        if (core.StartsWith("netcoreapp", StringComparison.OrdinalIgnoreCase))
            return 2000 + Version(core, "netcoreapp");
        if (core.StartsWith("net", StringComparison.OrdinalIgnoreCase))
        {
            var v = Version(core, "net");
            // "net10.0"/"net6.0" (dotted) is modern .NET; "net48"/"net462" (no dot) is classic Framework.
            return core.Contains('.') ? 3000 + v : v;
        }
        return 0;
    }

    /// <summary>When fewer projects were analysed than the owning solution declares, describe the partial
    /// closure so the Map can stamp its scope (Iteration 4 / Critical 3). Null when the full solution was
    /// analysed or no solution was resolved.</summary>
    private static string? BuildScopeNote(DiscoveryModel model, int analyzedProjectCount)
    {
        var slnCount = model.Solution?.ProjectPaths.Length ?? 0;
        // Decide partial from the RAW discovered count vs the raw .sln count (consistent bases). Require a
        // clear gap (≤ 75%) so a whole-solution run that discovers a few fewer than the .sln lists (failed
        // loads, etc.) isn't falsely stamped — guards eShop whole-solution staying "Microservices".
        var closure = slnCount > 0 && model.Projects.Length < slnCount * 3 / 4 && analyzedProjectCount > 0
            ? $"{analyzedProjectCount}-project closure of {slnCount}-project {model.Solution?.Name ?? "solution"}"
            : null;

        // Batch C (DC6) — the OTHER partiality: the repo declares several solutions and this analysis
        // covered one of them. Both notes ride the same Map/proto/UI field, because to a reader they are
        // the same sentence: what you are looking at is a slice, and here is which one.
        var multi = model.ScopeNote is { IsPartial: true } n ? n.Text : null;

        return (closure, multi) switch
        {
            (null, null) => null,
            (not null, null) => closure,
            (null, not null) => multi,
            _ => $"{multi}; {closure}",
        };
    }

    /// <summary>A5 (Prism D1.1e) — the render backstop's data source. The Library archetype always gets
    /// its surface; an App map with ZERO entries also builds one, so the renderer can show the public
    /// surface instead of a dead 19-line map (audit: Newtonsoft 209 tokens, GitVersion 485). Returns
    /// null when the built surface has no content (renderer then falls back to the console view).</summary>
    private static LibrarySurface? BuildSurface(DiscoveryModel model, Archetype archetype, ImmutableArray<EntryPoint> entries)
    {
        if (archetype == Archetype.Library)
            return LibrarySurfaceBuilder.Build(model);
        if (archetype == Archetype.App && entries.IsDefaultOrEmpty)
        {
            var surface = LibrarySurfaceBuilder.Build(model);
            return surface.Groups.Length > 0 || surface.EntryApi.Length > 0 ? surface : null;
        }
        return null;
    }

    private static ImmutableArray<ProjectNode> BuildTopology(DiscoveryModel model, CodeGraph graph)
    {
        var classifier = new ProjectClassifier(model.Projects);
        // PathText first (H1): solution-relative paths can arrive '\'-separated, which off-Windows
        // GetFileNameWithoutExtension reads as one big file name — the scope filter then matches
        // nothing and the topology renders empty.
        var scoped = model.Solution is { ProjectPaths.Length: > 0 } sln
            ? sln.ProjectPaths.Select(p => Path.GetFileNameWithoutExtension(PathText.Normalize(p))).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : null;

        var layerCounts = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        var featureCounts = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Nodes)
        {
            if (node.Project is not { } proj) continue;
            if (node.Layer is { } l)
            {
                if (!layerCounts.TryGetValue(proj, out var lc))
                    layerCounts[proj] = lc = new();
                lc[l] = lc.GetValueOrDefault(l) + 1;
            }
            if (node.Feature is { } f)
            {
                if (!featureCounts.TryGetValue(proj, out var fc))
                    featureCounts[proj] = fc = new();
                fc[f] = fc.GetValueOrDefault(f) + 1;
            }
        }

        string? Dominant(Dictionary<string, int> counts) =>
            counts is { Count: > 0 } ? counts.OrderByDescending(kv => kv.Value).First().Key : null;

        var perProjectLayer = layerCounts.ToDictionary(kv => kv.Key, kv => Dominant(kv.Value), StringComparer.OrdinalIgnoreCase);
        var perProjectFeature = featureCounts.ToDictionary(kv => kv.Key, kv => Dominant(kv.Value), StringComparer.OrdinalIgnoreCase);

        // ProjectReferences come through as raw ".../X.csproj" relative paths; reduce to project
        // names so the topology reads "A ── B" (and so the name-based scope filter actually matches —
        // it previously dropped every dependency for solution-scoped repos).
        // E2 (Prism D1.1b): the topology is production-only — the same filters as the service list.
        // SE.Redis rendered .github/docs/docker holder csproj and tests/RedisConfigs as topology nodes
        // (all NoTargets-SDK holders). NOTE: no raw IsTestPath filter here — our own fixture repos live
        // under tests/fixtures/ and absolute-path matching would empty their topology; classification +
        // the holder rule cover the audit cases. Dependency names are filtered to the kept set so
        // excluded projects don't linger as edge text.
        var kept = model.Projects
            .Where(p => !classifier.IsInTestProject(p.FilePath))
            .Where(p => classifier.IsProduction(p))
            .Where(p => scoped is null || scoped.Contains(p.Name))
            .ToList();
        var keptNames = kept.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        // E3 (Prism D1.4a): duplicate short names render indistinguishably (`Messages` ×6,
        // `AppHost` ×2) — a duplicated name gets a parent-directory qualifier, extended upward
        // until the rows are distinct.
        var displayNames = DisambiguateNames(kept);
        return
        [
            .. kept
                .OrderBy(p => displayNames[p])
                .Select(p => new ProjectNode(displayNames[p],
                    [.. p.ProjectReferences
                        .Select(r => Path.GetFileNameWithoutExtension(PathText.Normalize(r)) ?? "")
                        .Where(r => r.Length > 0 && keptNames.Contains(r) && (scoped is null || scoped.Contains(r)))
                        .OrderBy(r => r)])
                {
                    Layer = perProjectLayer.GetValueOrDefault(p.Name),
                    Feature = perProjectFeature.GetValueOrDefault(p.Name),
                })
        ];
    }

    /// <summary>E3: maps each kept project to its display name — the bare name when unique, else
    /// `Name (dir)` where dir is the nearest ancestor directory segment that isn't just the project
    /// name again, widened one segment at a time until the duplicate group is fully distinct.</summary>
    private static Dictionary<ProjectInfo, string> DisambiguateNames(List<ProjectInfo> kept)
    {
        static string[] AncestorSegments(ProjectInfo p)
        {
            var segments = new List<string>();
            var dir = PathText.DirOf(p.FilePath);
            while (!string.IsNullOrEmpty(dir))
            {
                var seg = PathText.NameOf(dir);
                if (string.IsNullOrEmpty(seg)) break;
                // src/Messages/Messages.csproj — the name-echo segment disambiguates nothing.
                if (!seg.Equals(p.Name, StringComparison.OrdinalIgnoreCase)) segments.Add(seg);
                dir = PathText.DirOf(dir);
            }
            return [.. segments];
        }

        var result = new Dictionary<ProjectInfo, string>();
        foreach (var group in kept.GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
        {
            var members = group.ToList();
            if (members.Count == 1)
            {
                result[members[0]] = members[0].Name;
                continue;
            }

            var ancestors = members.ToDictionary(p => p, AncestorSegments);
            for (var depth = 1; depth <= ancestors.Values.Max(a => a.Length); depth++)
            {
                string Qualified(ProjectInfo p) =>
                    $"{p.Name} ({string.Join("/", ancestors[p].Take(depth).Reverse())})";
                if (members.Select(Qualified).Distinct(StringComparer.OrdinalIgnoreCase).Count() == members.Count)
                {
                    foreach (var p in members) result[p] = Qualified(p);
                    break;
                }
            }
            // Pathologically identical paths: fall back to the bare name rather than looping forever.
            foreach (var p in members) result.TryAdd(p, p.Name);
        }
        return result;
    }

    /// <summary>Groups NuGet package references (dedup by name, highest version) by category. Takes an
    /// explicit project set so callers can scope it — the Library surface passes only runtime projects.</summary>
    internal static ImmutableArray<PackageGroup> BuildPackages(IEnumerable<ProjectInfo> projects)
    {
        // Dedup by name, keep highest version
        var best = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pkg in projects.SelectMany(p => p.PackageReferences))
        {
            if (!best.TryGetValue(pkg.Name, out var existing)
                || CompareVersions(pkg.Version, existing) > 0)
            {
                best[pkg.Name] = pkg.Version;
            }
        }

        // Group by category
        var groups = new Dictionary<string, List<string>>();
        foreach (var (name, version) in best.OrderBy(kv => kv.Key))
        {
            var cat = CategorizePackage(name);
            if (!groups.TryGetValue(cat, out var list))
                groups[cat] = list = [];
            // Strip unevaluated MSBuild-variable versions ($(TemplateOrchardPackageVersion) etc.) — show
            // just the package name rather than leak the build variable (Iteration 4 / Low 16).
            var showVersion = !string.IsNullOrEmpty(version) && !version.Contains("$(", StringComparison.Ordinal);
            list.Add(showVersion ? $"{name} {version}" : name);
        }

        var order = new[] { "Web/API", "ORM/Data", "Mediator/CQRS", "Messaging", "Validation",
            "Logging", "Testing", "Cloud", "Utilities", "Other" };
        var result = ImmutableArray.CreateBuilder<PackageGroup>();
        foreach (var cat in order)
        {
            if (groups.TryGetValue(cat, out var pkgs) && pkgs.Count > 0)
                result.Add(new PackageGroup(cat, [.. pkgs]));
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<string> BuildAggregates(DiscoveryModel model)
        => [.. model.Detections
            .OfType<EfEntityDetection>()
            .Where(d => d.IsAggregate)
            .Select(d => d.EntityType)
            .Distinct()
            .OrderBy(n => n)];

    private static ImmutableArray<string> BuildPipelineBehaviors(DiscoveryModel model)
    {
        var behaviors = new HashSet<string>(StringComparer.Ordinal);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            // Direct registration: services.AddTransient(typeof(IPipelineBehavior<,>), typeof(X))
            if (di.ServiceType.Contains("IPipelineBehavior", StringComparison.Ordinal))
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add(impl);
            }
            if (di.ExtensionsUsed.Contains("AddOpenBehavior") || di.ServiceType == "AddOpenBehavior")
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add(impl);
            }
            // AddMediatR fluent config: the lambda body may contain AddOpenBehavior(typeof(X)) calls
            if (di.ImplementationType is { Length: > 0 } body
                && body.Contains("AddOpenBehavior", StringComparison.Ordinal))
            {
                var pos = 0;
                while ((pos = body.IndexOf("AddOpenBehavior", pos, StringComparison.Ordinal)) >= 0)
                {
                    pos += "AddOpenBehavior".Length;
                    var rest = body[pos..];
                    var bp = 0;
                    while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                    if (bp < rest.Length && rest[bp] == '(') bp++;
                    while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                    if (bp + "typeof".Length <= rest.Length
                        && rest.AsSpan(bp, "typeof".Length).SequenceEqual("typeof"))
                    {
                        bp += "typeof".Length;
                        while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                        if (bp < rest.Length && rest[bp] == '(') bp++;
                        while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                        var start = bp;
                        while (bp < rest.Length && (char.IsLetterOrDigit(rest[bp]) || rest[bp] == '_')) bp++;
                        if (bp > start)
                        {
                            var name = rest[start..bp];
                            if (name.Length > 0 && name != "?")
                                behaviors.Add(name);
                        }
                    }
                }
            }
        }
        return [.. behaviors.OrderBy(b => b)];
    }

    /// <summary>Strips typeof(…) / nameof(…) / generic arity suffix to get a raw type name.</summary>
    private static string CleanTypeRef(string expr)
    {
        var s = expr.AsSpan().Trim();
        if (s.StartsWith("typeof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        else if (s.StartsWith("nameof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        var generic = s.IndexOf('<');
        if (generic > 0) s = s.Slice(0, generic);
        return s.ToString().Trim();
    }

    private static int CompareVersions(string a, string b)
    {
        if (TryParseMajorMinor(a, out var aMajor, out var aMinor)
            && TryParseMajorMinor(b, out var bMajor, out var bMinor))
        {
            var cmp = aMajor.CompareTo(bMajor);
            return cmp != 0 ? cmp : aMinor.CompareTo(bMinor);
        }
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseMajorMinor(string version, out int major, out int minor)
    {
        major = 0; minor = 0;
        if (string.IsNullOrEmpty(version)) return false;
        var parts = version.Split('.');
        return parts.Length >= 2
            && int.TryParse(parts[0], out major)
            && int.TryParse(parts[1], out minor);
    }

    private static string CategorizePackage(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("aspnet") || n.Contains("microsoft.asp") || n.StartsWith("swashbuckle")
            || n.Contains("fastendpoints") || n.Contains("minimalapi")) return "Web/API";
        if (n.Contains("entityframework") || n.Contains("ef.") || n.Contains("efcore")
            || n.Contains("dapper") || n.Contains("sqlite") || n.Contains("sqlserver")
            || n.Contains("npgsql") || n.Contains("mysql") || n.Contains("cosmos")) return "ORM/Data";
        if (n.Contains("mediatr")) return "Mediator/CQRS";
        if (n.Contains("masstransit") || n.Contains("nservicebus") || n.Contains("rabbitmq")
            || n.Contains("azure.messaging") || n.Contains("amqp")) return "Messaging";
        if (n.Contains("fluentvalidation")) return "Validation";
        if (n.Contains("serilog") || n.Contains("nlog") || n.Contains("log4net")
            || n.Contains("opentelemetry") || n.Contains("applicationinsights")) return "Logging";
        if (n.Contains("xunit") || n.Contains("nunit") || n.Contains("mstest")
            || n.Contains("moq") || n.Contains("nsubstitute") || n.Contains("bogus")
            || n.Contains("fluentassertions") || n.Contains("shouldly")
            || n.Contains("testcontainers") || n.Contains("coverlet")) return "Testing";
        if (n.Contains("azure.") || n.Contains("amazon.") || n.Contains("aws.")) return "Cloud";
        if (n.Contains("polly") || n.Contains("automapper") || n.Contains("scrutor")
            || n.Contains("humanizer") || n.Contains("newtonsoft")
            || n.Contains("refit") || n.Contains("restsharp")
            || n.Contains("swagger")) return "Utilities";
        return "Other";
    }
}
