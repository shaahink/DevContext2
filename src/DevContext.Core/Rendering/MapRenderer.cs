using System.Text;

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
        var sb = new StringBuilder();
        var model = ctx.Snapshot.Model;

        AppendMapHeader(sb, ctx, model);
        AppendStack(sb, ctx);
        AppendStyle(sb, ctx.Map);
        AppendTopology(sb, ctx.Map);
        AppendEntryPoints(sb, ctx.Map);
        AppendCrossCutting(sb, ctx.Map);
        AppendPackages(sb, ctx.Map);
        AppendFooter(sb);

        var content = sb.ToString();
        var tokens = content.Length / 4;
        return new ValueTask<RenderedContext>(new RenderedContext(content, tokens, [], TimeSpan.Zero, "2.0"));
    }

    private static void AppendMapHeader(StringBuilder sb, MapRenderContext ctx, DiscoveryModel model)
    {
        var sln = model.Solution?.Name ?? "unknown";
        var projCount = ctx.Map.Topology.Length;
        sb.AppendLine($"MAP  {sln}     ({projCount} project{(projCount != 1 ? "s" : "")})");
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

        // CQRS / Mediator
        if (signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected)
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

    private static void AppendEntryPoints(StringBuilder sb, MapModel map)
    {
        if (map.Entries.IsDefaultOrEmpty) return;
        sb.AppendLine("ENTRY POINTS");

        var byKind = map.Entries.GroupBy(e => e.Kind).OrderBy(g => g.Key);
        foreach (var group in byKind)
        {
            var list = group.ToList();
            sb.Append($"   {GroupLabel(group.Key)} ({list.Count})");

            if (list.Count <= 10)
            {
                sb.AppendLine();
                foreach (var ep in list)
                    sb.AppendLine($"      {ep.Title}  {(ep.Provenance is { } p ? $"({p})" : "")}");
            }
            else
            {
                sb.AppendLine();
                foreach (var ep in list.Take(10))
                    sb.AppendLine($"      {ep.Title}");
                sb.AppendLine($"      ... and {list.Count - 10} more");
            }
        }
        sb.AppendLine();
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
            sb.AppendLine($"   {group.Label}:  {string.Join(", ", group.Packages)}");
        }
        sb.AppendLine();
    }

    private static void AppendFooter(StringBuilder sb)
    {
        sb.AppendLine("→ drill in:  trace <entry>        → list all:  trace --all");
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
