using System.Text;

using DevContext.Core.Extractors.Generic;
using DevContext.Core.Graph;
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
        Add(sections, "Topology", sb => AppendTopology(sb, ctx.Map));
        Add(sections, "Entry points", sb => AppendEntryPoints(sb, ctx.Map, basePath));
        Add(sections, "Cross-cutting", sb => AppendCrossCutting(sb, ctx.Map));
        Add(sections, "Packages", sb => AppendPackages(sb, ctx.Map));
        Add(sections, "Footer", AppendFooter);

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
        sb.AppendLine($"MAP  {sln}     ({projCount} project{(projCount != 1 ? "s" : "")})");
        if (ctx.Map.ScopeNote is { Length: > 0 } scope)
            sb.AppendLine($"SCOPE  {scope} — style/topology are local to this slice, not the whole system");
        sb.AppendLine();
    }

    private static void AppendStack(StringBuilder sb, MapRenderContext ctx)
    {
        var model = ctx.Snapshot.Model;
        var signals = model.Architecture.All;
        var parts = new List<string>();

        // Runtime
        var tfms = model.Projects
            .SelectMany(p => p.TargetFrameworks)
            .Where(f => !f.Contains("$(", StringComparison.Ordinal)) // drop unevaluated MSBuild vars (Low 16)
            .Distinct()
            .OrderBy(f => f)
            .ToList();
        if (tfms.Count > 0) parts.Add(string.Join(", ", tfms));

        // Web framework
        if (signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) && ma.Detected)
            parts.Add("Minimal APIs");
        if (signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var ctrl) && ctrl.Detected)
            parts.Add("Controllers");
        if (signals.TryGetValue(ArchitectureSignals.Keys.FastEndpoints, out var fe) && fe.Detected)
            parts.Add("FastEndpoints");

        // CQRS / Mediator — light from handler evidence too (not just the package signal), so a scoped
        // sub-project whose handlers are present reads consistently with the resolved STYLE (G7 residual).
        if (ArchitectureStyleDetector.HasMediatREvidence(model))
            parts.Add("MediatR (CQRS)");

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
        sb.AppendLine();
    }

    private static void AppendTopology(StringBuilder sb, MapModel map)
    {
        if (map.Topology.IsDefaultOrEmpty) return;
        sb.AppendLine("TOPOLOGY (depends-on)");

        foreach (var proj in map.Topology)
        {
            if (proj.DependsOn.Length == 0)
            {
                sb.AppendLine($"   {proj.Name}");
            }
            else
            {
                var deps = string.Join(", ", proj.DependsOn);
                sb.AppendLine($"   {proj.Name} ── {deps}");
            }
        }
        sb.AppendLine();
    }

    private static void AppendEntryPoints(StringBuilder sb, MapModel map, string? basePath)
    {
        if (map.Entries.IsDefaultOrEmpty) return;
        sb.AppendLine("ENTRY POINTS");

        // The Map is the entry-discovery surface and the launch pad for tracing, so list ALL entries
        // (no "... and N more"). Each shows its dispatch target (route → command/handler) when the
        // graph resolved one, plus a short file:line.
        var byKind = map.Entries.GroupBy(e => e.Kind).OrderBy(g => g.Key);
        foreach (var group in byKind)
        {
            var list = group.ToList();
            sb.AppendLine($"   {GroupLabel(group.Key)} ({list.Count})");
            foreach (var ep in list)
                sb.AppendLine($"      {ep.Title}{Target(ep)}{Where(ep, basePath)}");
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

    private static void AppendFooter(StringBuilder sb)
    {
        sb.AppendLine("→ drill in:  --focus \"<entry>\"   (e.g. --focus \"POST /api/orders/\" or --focus <TypeName>)");
    }

    private static string GroupLabel(EntryPointKind kind) => kind switch
    {
        EntryPointKind.HttpEndpoint => "HTTP",
        EntryPointKind.MessageConsumer => "Bus",
        EntryPointKind.DomainEventHandler => "Domain",
        EntryPointKind.HostedService => "Background",
        EntryPointKind.ScheduledJob => "Scheduled",
        EntryPointKind.PublicApi => "Public API",
        _ => kind.ToString(),
    };
}
