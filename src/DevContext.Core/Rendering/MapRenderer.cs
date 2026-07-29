using System.Text;

using DevContext.Core.Extractors.Generic;
using DevContext.Core.Graph;
using DevContext.Core.Graph.EntrySurfaces;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Rendering;

public sealed record MapRenderContext(
    MapModel Map,
    AnalysisSnapshot Snapshot,
    string Format,
    RenderRequest Request);

public static class MapRenderer
{
    public static ValueTask<RenderedContext> RenderAsync(MapRenderContext ctx, CancellationToken ct)
    {
        var model = ctx.Snapshot.Model;
        var sections = new List<NarrativeSection>();

        // Identity preamble (header + stack + style) is one always-present block.
        Add(sections, "Overview", sb =>
        {
            AppendMapHeader(sb, ctx, model);
            AppendStack(sb, ctx);
            AppendStyle(sb, ctx.Map);
        });
        var basePath = ctx.Snapshot.RootPath;
        // L7.2 — archetype-specific section renders before Topology for Desktop/Worker/Blazor/Library
        Add(sections, "Archetype", sb => AppendArchetypeView(sb, ctx.Map));
        Add(sections, "Topology", sb => AppendTopology(sb, ctx.Map));
        Add(sections, "Routes", sb => AppendGatewayRoutes(sb, ctx.Map));
        Add(sections, "Cross-service", sb => AppendServiceLinks(sb, ctx));
        Add(sections, "Event wiring", sb => AppendEventWiring(sb, ctx));
        Add(sections, "Entry points", sb => AppendEntryPoints(sb, ctx.Map, basePath));
        // A5 (Prism D1.1e) — render backstop: an App map with zero entries is never a dead end.
        Add(sections, "Backstop", sb => AppendNoEntriesBackstop(sb, ctx));
        Add(sections, "Cross-cutting", sb => AppendCrossCutting(sb, ctx.Map));
        Add(sections, "Packages", sb => AppendPackages(sb, ctx.Map));
        Add(sections, "Footer", sb => AppendFooter(sb, ctx));

        return new ValueTask<RenderedContext>(NarrativeSections.ToRenderedContext(sections));
    }

    /// <summary>Builds one fragment into its own buffer; skips empty blocks so the desktop only
    /// lists sections that actually rendered content.</summary>
    private static void Add(List<NarrativeSection> sections, string key, Action<StringBuilder> build)
    {
        var sb = new StringBuilder();
        build(sb);
        if (sb.Length > 0)
            sections.Add(new NarrativeSection(key, sb.ToString()));
    }

    private static void AppendMapHeader(StringBuilder sb, MapRenderContext ctx, DiscoveryModel model)
    {
        var sln = model.Solution?.Name ?? "unknown";
        var projCount = ctx.Map.Topology.Length;
        var label = ctx.Map.Archetype switch
        {
            Archetype.Gateway => "GATEWAY",
            Archetype.Desktop => "DESKTOP APP",
            Archetype.Worker  => "WORKER",
            Archetype.Blazor  => "BLAZOR APP",
            Archetype.Library => "LIBRARY",
            Archetype.CliTool => "CLI TOOL",
            _ => "MAP",
        };
        sb.AppendLine($"{label}  {sln}     ({projCount} project{(projCount != 1 ? "s" : "")})");
        if (ctx.Map.ScopeNote is { Length: > 0 } scope)
            sb.AppendLine($"SCOPE  {scope} — style/topology are local to this slice, not the whole system");
        AppendOutsideScopeApps(sb, ctx.Map);
        sb.AppendLine();
    }

    /// <summary>R3 D-4 (G6.3) — say what the scope pick cost. The SCOPE line already names the solution
    /// analysed and how many the repo declares; it never said that RUNNABLE APPS were among the ones you
    /// are not seeing. dotnet-podcasts keeps its two MAUI clients in sibling solutions, so once the
    /// per-service rollup started obeying the scope (G6.1) the only surface that had ever mentioned them
    /// went silent. These rows carry the same style vocabulary as the services and are deliberately NOT
    /// under "per service:" — a service is a project the canvas draws, and the canvas does not draw these.</summary>
    private static void AppendOutsideScopeApps(StringBuilder sb, MapModel map)
    {
        if (map.OutsideScopeApps.IsDefaultOrEmpty) return;
        var n = map.OutsideScopeApps.Length;
        sb.AppendLine($"       not analyzed — {n} runnable app{(n != 1 ? "s" : "")} outside this solution:");
        foreach (var app in map.OutsideScopeApps)
        {
            var stackStr = app.Stack.Length > 0 ? $" [{string.Join(", ", app.Stack)}]" : "";
            sb.AppendLine($"         {app.ProjectName}: {app.Style}{stackStr}");
        }
    }

    /// <summary>A5 (Prism D1.1e) — no dead maps. An App-archetype map with ZERO entries renders the
    /// public surface when one exists (the MapBuilder backstop built <c>Map.Surface</c>), else an honest
    /// console view naming the runnable executables. Before this, Newtonsoft rendered 209 tokens and
    /// GitVersion 485 — confidently empty lenses on 600+-file repos.</summary>
    private static void AppendNoEntriesBackstop(StringBuilder sb, MapRenderContext ctx)
    {
        if (ctx.Map.Archetype != Archetype.App || !ctx.Map.Entries.IsDefaultOrEmpty) return;

        if (ctx.Map.Surface is { } surface && (surface.Groups.Length > 0 || surface.EntryApi.Length > 0))
        {
            sb.AppendLine("NOTE: no entry points detected — showing the public surface instead");
            sb.AppendLine();
            LibrarySurfaceRenderer.AppendEntryApi(sb, surface);
            LibrarySurfaceRenderer.AppendAbstractions(sb, surface);
            LibrarySurfaceRenderer.AppendSurface(sb, surface);
            return;
        }

        var consoleExes = ctx.Snapshot.Model.Projects
            .Where(p => p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true
                && ProjectClassifier.IsProductionProject(p))
            .OrderBy(p => p.Name)
            .ToList();
        if (consoleExes.Count == 0) return;
        sb.AppendLine("CONSOLE VIEW");
        foreach (var exe in consoleExes)
            sb.AppendLine($"   {exe.Name}  ({exe.OutputType})");
        // Surface-neutral phrasing (T6.3): this markdown renders on CLI, desktop AND MCP —
        // "trace" is the verb every surface has; a flag name is only true on one of them.
        sb.AppendLine("   NOTE: no entry points detected — trace a focused type (e.g. trace \"<TypeName>\")");
        sb.AppendLine();
    }

    private static void AppendStack(StringBuilder sb, MapRenderContext ctx)
    {
        var model = ctx.Snapshot.Model;
        var signals = model.Architecture.All;
        var parts = new List<string>();

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
        if (ctx.Map.Aggregates.Length > 0)
            parts.Add("DDD aggregates");

        if (parts.Count > 0)
        {
            sb.AppendLine("STACK  " + string.Join(" · ", parts));
            sb.AppendLine();
        }
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

    private static void AppendStyle(StringBuilder sb, MapModel map)
    {
        if (string.IsNullOrEmpty(map.StyleEvidence)) return;
        var confidence = map.StyleConfidence switch
        {
            >= 0.8f => "high",
            >= 0.5f => "moderate",
            _ => "low",
        };
        sb.AppendLine($"STYLE  {map.Style}  (confidence {confidence})");
        sb.AppendLine($"       evidence: {map.StyleEvidence}");

        // M1.9 / D5 — per-service style rollup
        if (!map.ServiceStyles.IsDefaultOrEmpty)
        {
            sb.AppendLine();
            sb.AppendLine("       per service:");
            foreach (var svc in map.ServiceStyles)
            {
                var stackStr = svc.Stack.Length > 0
                    ? $" [{string.Join(", ", svc.Stack)}]"
                    : "";
                sb.AppendLine($"         {svc.ProjectName}: {svc.Style}{stackStr}");
            }
        }
        sb.AppendLine();
    }

    /// <summary>L7.2 — renders the archetype-specific entry-point view for Desktop, Worker, Blazor, and Library.</summary>
    private static void AppendArchetypeView(StringBuilder sb, MapModel map)
    {
        var view = map.ArchetypeView;
        if (view is not { IsRelevant: true }) return;

        sb.AppendLine(view.SectionLabel);

        if (!view.Groups.IsDefaultOrEmpty)
        {
            foreach (var group in view.Groups.Take(MaxArchetypeGroups))
            {
                var layerTag = group.Layer is { } l ? $" [{l}]" : "";
                sb.AppendLine($"   {group.Project}{layerTag} ({group.Entries.Length})");
                foreach (var entry in group.Entries)
                {
                    var target = entry.Target is { Length: > 0 } t ? $"  \u2192 {t}" : "";
                    sb.AppendLine($"      {entry.Title}{target}");
                }
            }
        }
        else
        {
            sb.AppendLine($"   (no {view.Archetype.ToString().ToLowerInvariant()}-specific entries detected)");
        }

        sb.AppendLine();
    }

    /// <summary>Max archetype entry groups shown.</summary>
    private const int MaxArchetypeGroups = 20;

    private static void AppendTopology(StringBuilder sb, MapModel map)
    {
        if (map.Topology.IsDefaultOrEmpty) return;

        // W4: rank projects by "most-depended-on" (reverse dependency count) so the
        // framework roots and shared libraries surface first — not alphabetical noise.
        var dependedOn = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var proj in map.Topology)
        {
            if (!dependedOn.ContainsKey(proj.Name)) dependedOn[proj.Name] = 0;
            foreach (var dep in proj.DependsOn)
            {
                var key = dep;
                dependedOn[key] = dependedOn.GetValueOrDefault(key) + 1;
            }
        }

        var ranked = map.Topology
            .OrderByDescending(p => dependedOn.GetValueOrDefault(p.Name))
            .ThenBy(p => p.Name)
            .ToList();

        var shown = ranked.Take(MaxTopologyProjects).ToList();
        var omitted = ranked.Count - shown.Count;

        sb.AppendLine("TOPOLOGY (depends-on)");
        foreach (var proj in shown)
        {
            if (proj.DependsOn.Length == 0)
                sb.AppendLine($"   {proj.Name}");
            else
            {
                var deps = string.Join(", ", proj.DependsOn);
                sb.AppendLine($"   {proj.Name} ── {deps}");
            }
        }
        if (omitted > 0)
            sb.AppendLine($"   … and {omitted} more projects (trace a focused entry for a scoped slice)");
        sb.AppendLine();
    }

    private static void AppendGatewayRoutes(StringBuilder sb, MapModel map)
    {
        if (map.Routes.IsDefaultOrEmpty) return;

        var shown = map.Routes.Take(MaxRoutes).ToList();
        var omitted = map.Routes.Length - shown.Count;

        sb.AppendLine("ROUTES");
        foreach (var route in shown)
        {
            sb.Append($"   {route.UpstreamMethods} {route.UpstreamTemplate}");
            sb.Append($"  →  {route.DownstreamHosts}{route.DownstreamTemplate}");
            sb.AppendLine();
        }
        if (omitted > 0)
            sb.AppendLine($"   … and {omitted} more routes");
        sb.AppendLine();
    }

    /// <summary>Renders ServiceLink edges — cross-project seams between runnable services
    /// (bus, gRPC, HTTP via gateway). M1.6-M1.8.</summary>
    private static void AppendServiceLinks(StringBuilder sb, MapRenderContext ctx)
    {
        var graph = ctx.Snapshot.Graph;
        if (graph is null) return;

        var serviceLinks = graph.AllEdges
            .Where(e => e.Kind == EdgeKind.ServiceLink)
            .ToList();
        if (serviceLinks.Count == 0) return;

        var byTag = serviceLinks
            .GroupBy(e => e.Tags.FirstOrDefault() ?? "unknown")
            .ToList();

        sb.AppendLine("CROSS-SERVICE");
        foreach (var group in byTag.OrderBy(g => g.Key))
        {
            var label = group.Key switch
            {
                ServiceLinkTags.BusPublishConsume => "bus",
                ServiceLinkTags.Grpc => "gRPC",
                ServiceLinkTags.HttpViaGateway => "http/via gateway",
                ServiceLinkTags.RefitDirect => "refit/direct",
                ServiceLinkTags.HttpDirect => "http/direct",
                ServiceLinkTags.AspireReference => "apphost reference",
                _ => group.Key,
            };
            sb.AppendLine($"  {label} ({group.Count()})");
        }

        // List individual links with provenance
        foreach (var sl in serviceLinks.OrderBy(e => TagsLabel(e.Tags)).ThenBy(e => e.Provenance))
        {
            var tag = sl.Tags.FirstOrDefault() switch
            {
                ServiceLinkTags.BusPublishConsume => "bus",
                ServiceLinkTags.Grpc => "gRPC",
                ServiceLinkTags.HttpViaGateway => "http",
                ServiceLinkTags.RefitDirect => "refit",
                ServiceLinkTags.HttpDirect => "http",
                ServiceLinkTags.AspireReference => "apphost",
                _ => "?",
            };
            var fromNode = graph.Node(sl.From);
            var toNode = graph.Node(sl.To);
            var fromName = fromNode?.Title ?? sl.From.ToString();
            var toName = toNode?.Title ?? sl.To.ToString();
            sb.Append($"    [{tag}] {fromName} → {toName}");
            if (sl.Provenance is { Length: > 0 } prov)
                sb.Append($"  ({prov})");
            sb.AppendLine();
        }
        sb.AppendLine();
    }

    /// <summary>T2.6 — the event board, rendered from the single <see cref="CodeGraph.EventWiring"/>
    /// projection (the same rows the insight and flow markers use). Integration events only: cross-service
    /// hops first (the integration contract), then the rest. Domain events are in-process and stay out.</summary>
    private static void AppendEventWiring(StringBuilder sb, MapRenderContext ctx)
    {
        var wiring = ctx.Snapshot.Graph?.EventWiring ?? [];
        var integration = wiring.Where(w => w.IsIntegration).ToList();
        if (integration.Count == 0) return;

        var crossService = integration.Count(w => w.IsCrossService);
        sb.AppendLine($"EVENT WIRING  ({integration.Count} integration events, {crossService} cross-service)");
        foreach (var w in integration
            .OrderByDescending(w => w.IsCrossService)
            .ThenBy(w => w.EventName, StringComparer.Ordinal))
        {
            var pubs = w.Publishers.Select(p => p.Service).OfType<string>().Distinct(StringComparer.Ordinal).ToList();
            var cons = w.Consumers.Select(c => c.Service).OfType<string>().Distinct(StringComparer.Ordinal).ToList();
            var pubStr = pubs.Count > 0 ? string.Join(", ", pubs) : "(external)";
            var conStr = cons.Count > 0 ? string.Join(", ", cons) : "(no consumer)";
            var marker = w.IsCrossService ? "→" : "·";
            sb.AppendLine($"  {w.EventName}: {pubStr} {marker} {conStr}");
        }
        sb.AppendLine();
    }

    private static string TagsLabel(ImmutableArray<string> tags) => tags.FirstOrDefault() ?? "z-unknown";

    private static void AppendEntryPoints(StringBuilder sb, MapModel map, string? basePath)
    {
        if (map.Entries.IsDefaultOrEmpty) return;
        sb.AppendLine("ENTRY POINTS");

        // W4: cap each kind group at MaxEntriesPerKind, ranked production-first (entries
        // with resolved targets → those without), then by title. Groups beyond the cap get
        // an explicit "… and N more" disclosure.
        var byKind = map.Entries.GroupBy(e => e.Kind).OrderBy(g => g.Key);
        foreach (var group in byKind)
        {
            var list = group
                .OrderByDescending(e => e.Target is not null)
                .ThenBy(e => e.Title)
                .ToList();
            var shown = list.Take(MaxEntriesPerKind).ToList();
            var omitted = list.Count - shown.Count;

            sb.AppendLine($"   {GroupLabel(group.Key)} ({list.Count})");
            foreach (var ep in shown)
                sb.AppendLine($"      {ep.Title}{Target(ep)}{Where(ep, basePath)}");
            if (omitted > 0)
                sb.AppendLine($"      … and {omitted} more ({GroupLabel(group.Key).ToLowerInvariant()} entries — trace one for a drill-in)");
        }
        sb.AppendLine();
    }

    private static string Target(EntryPoint ep)
        => string.IsNullOrEmpty(ep.Target) ? "" : $"  → {ep.Target}";

    /// <summary>Short "(repo/relative/File.cs:line)" — repo-relative (like traces), not the absolute
    /// machine path, so the Map's entry list matches the trace's source locations.</summary>
    private static string Where(EntryPoint ep, string? basePath)
    {
        if (ep.Provenance is not { Length: > 0 } p) return "";
        return $"  ({PathDisplay.RelativeProvenance(basePath, p)})";
    }

    private static void AppendCrossCutting(StringBuilder sb, MapModel map)
    {
        var parts = new List<string>();

        if (map.PipelineBehaviors.Length > 0)
        {
            parts.Add("MediatR pipeline (every command):  "
                + string.Join(" → ", map.PipelineBehaviors));
        }

        if (map.Aggregates.Length > 0)
        {
            parts.Add("Aggregates:   " + string.Join(" · ", map.Aggregates.Take(10))
                + (map.Aggregates.Length > 10 ? $" ... ({map.Aggregates.Length} total)" : ""));
        }

        if (parts.Count > 0)
        {
            sb.AppendLine("CROSS-CUTTING");
            foreach (var part in parts)
                sb.AppendLine($"   {part}");
            sb.AppendLine();
        }
    }

    private static void AppendPackages(StringBuilder sb, MapModel map)
    {
        if (map.Packages.IsDefaultOrEmpty) return;
        sb.AppendLine("PACKAGES");
        foreach (var group in map.Packages)
        {
            var shown = group.Packages.Take(MaxPackagesPerGroup).ToList();
            var line = string.Join(", ", shown);
            if (group.Packages.Length > MaxPackagesPerGroup)
                line += $" … ({group.Packages.Length} total)";
            sb.AppendLine($"   {group.Label}:  {line}");
        }
        sb.AppendLine();
    }

    /// <summary>Cap per-group package lists so PACKAGES stays a signal, not a manifest dump (G9).</summary>
    private const int MaxPackagesPerGroup = 8;
    /// <summary>Max topology projects shown — ranked by most-depended-on, remainder disclosed (W4).</summary>
    private const int MaxTopologyProjects = 50;
    /// <summary>Max entries shown per kind group — ranked production-first, remainder disclosed (W4).</summary>
    private const int MaxEntriesPerKind = 20;
    /// <summary>Max gateway routes shown, remainder disclosed (W4).</summary>
    private const int MaxRoutes = 30;

    // T3.8 — the drill-in example is derived from THIS repo's own top-scored entry, not a hardcoded
    // eShop route ("POST /api/orders/") that resolves on no other repo (audit C5/D).
    private static void AppendFooter(StringBuilder sb, MapRenderContext ctx)
    {
        var example = ctx.Map.Entries.IsDefaultOrEmpty
            ? "<TypeName>"
            : ctx.Map.Entries
                .OrderByDescending(e => e.Score)
                .Select(e => e.Route is not null && e.HttpMethod is not null ? $"{e.HttpMethod} {e.Route}" : e.Title)
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? "<TypeName>";
        // Surface-neutral phrasing (T6.3): the same footer ships over CLI, desktop and MCP.
        sb.AppendLine($"→ drill in:  trace a focused entry   (e.g. trace \"{example}\")");
    }

    internal static string GroupLabelForKind(EntryPointKind kind) => GroupLabel(kind);

    private static readonly FrozenDictionary<EntryPointKind, string> KindLabels = EntrySurfaceCatalog.All
        .Where(d => d.Kind is not null)
        .GroupBy(d => d.Kind!.Value)
        .ToFrozenDictionary(g => g.Key, g => g.First().RenderLabel);

    private static string GroupLabel(EntryPointKind kind)
        => KindLabels.GetValueOrDefault(kind, kind.ToString());
}
