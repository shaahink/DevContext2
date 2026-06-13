using System.Collections.Immutable;
using System.Text;

using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Rendering;

/// <summary>Renders the discovery model as semantic HTML for the Desktop Human View.</summary>
public sealed class HtmlContextRenderer : IContextRenderer
{
    public string Format => "html";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var sb = new StringBuilder();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var sectionTokens = new List<SectionTokenRecord>();
        var navLinks = new List<(string Id, string Label)>();

        var includedIds = options.Plan is { } plan
            ? new HashSet<string>(plan.IncludedTypeIds, StringComparer.Ordinal)
            : null;

        sb.AppendLine("<article class='dc-report'>");
        var preLen = sb.Length;
        RenderHeader(sb, model, options);
        TrackSection(sectionTokens, "Header", preLen, sb.Length);

        // Build nav while rendering sections
        RenderSection(sb, options, SectionNames.ArchitectureOverview, "Architecture overview", navLinks,
            () => RenderArchitectureOverview(sb, model, options));
        if (!options.FocusPoints.IsDefaultOrEmpty && options.FocusPoints.Length > 0)
            RenderSection(sb, options, "entry-points", "Entry points", navLinks,
                () => RenderEntryPoints(sb, model, options));
        RenderSection(sb, options, SectionNames.Endpoints, "Endpoints", navLinks,
            () => RenderEndpoints(sb, model));
        RenderSection(sb, options, SectionNames.CallGraph, "Call graph", navLinks,
            () => RenderCallGraph(sb, model, options));
        RenderSection(sb, options, SectionNames.MediatRHandlers, "MediatR Handlers", navLinks,
            () => RenderMediatRHandlers(sb, model));
        RenderSection(sb, options, SectionNames.DataModel, "Data model", navLinks,
            () => RenderDataModel(sb, model));
        RenderSection(sb, options, SectionNames.MessageConsumers, "Message consumers", navLinks,
            () => RenderMessageConsumers(sb, model));
        RenderSection(sb, options, SectionNames.IndirectWiring, "Indirect wiring", navLinks,
            () => RenderIndirectWiring(sb, model));
        RenderSection(sb, options, SectionNames.BackgroundWorkers, "Background workers", navLinks,
            () => RenderBackgroundWorkers(sb, model));
        RenderSection(sb, options, SectionNames.MiddlewarePipeline, "Middleware pipeline", navLinks,
            () => RenderMiddlewarePipeline(sb, model));
        RenderSection(sb, options, SectionNames.DiRegistrations, "DI registrations", navLinks,
            () => RenderDiRegistrations(sb, model));
        RenderAntiPatterns(sb, model);
        RenderEventFlow(sb, model);
        RenderSection(sb, options, SectionNames.RelatedTypes, "Related types", navLinks,
            () => RenderRelatedTypes(sb, model, includedIds));
        if (options.IncludeDiagnostics)
            RenderSection(sb, options, "diagnostics", "Diagnostics", navLinks,
                () => RenderDiagnostics(sb, model, options));

        RenderFooter(sb, model, sw, includedIds);

        // Insert nav after header if there are links
        var navHtml = BuildNav(navLinks);
        var content = sb.ToString();
        if (navHtml is not null)
            content = content.Replace("</header>", $"</header>{navHtml}");

        sb.Clear();
        sb.Append(content);
        sb.AppendLine("</article>");

        var final = sb.ToString();
        var tokens = Math.Max(1, final.Length / 4);
        return new ValueTask<RenderedContext>(new RenderedContext(
            final, tokens, [.. model.AppliedCompressions], sw.Elapsed, "1.1"));
    }

    private static string? BuildNav(List<(string Id, string Label)> links)
    {
        if (links.Count <= 1) return null;
        var sb = new StringBuilder();
        sb.AppendLine("<nav class='dc-nav'><span class='dc-nav-title'>Jump to:</span>");
        foreach (var (id, label) in links)
            sb.AppendLine($"<a href='#dc-{id}'>{System.Net.WebUtility.HtmlEncode(label)}</a>");
        sb.AppendLine("</nav>");
        return sb.ToString();
    }

    private static void RenderSection(StringBuilder sb, RenderOptions options, string sectionId, string label,
        List<(string, string)> navLinks, Action render)
    {
        if (!ShouldRender(sectionId, options)) return;
        render();
        navLinks.Add((sectionId, label));
    }

    private static void TrackSection(List<SectionTokenRecord> records, string name, int prevLength, int currentLength)
    {
        if (currentLength <= prevLength) return;
        var chars = currentLength - prevLength;
        var tokens = Math.Max(1, chars / 4);
        records.Add(new SectionTokenRecord(name, tokens, tokens, false));
    }

    private static bool ShouldRender(string sectionName, RenderOptions options)
    {
        if (options.RequiredSections.IsDefaultOrEmpty) return true;
        return options.RequiredSections.Contains(sectionName);
    }

    private static string RelPath(string? fullPath)
    {
        if (fullPath is null) return "";
        var name = System.IO.Path.GetFileName(fullPath);
        return System.Net.WebUtility.HtmlEncode(name);
    }

    private static string FullPathAttr(string? fullPath)
        => fullPath is not null ? $" title=\"{System.Net.WebUtility.HtmlEncode(fullPath)}\"" : "";

    private static string AuthBadge(EndpointDetection ep)
        => ep.AuthAttributes.Length > 0
            ? " <span class='dc-auth'>🔒</span>"
            : "";

    // ── Render methods ─────────────────────────────────────────────────────

    private static void RenderHeader(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        var name = options.ScenarioDisplayName ?? "Overview";
        var style = model.DetectedStyle != ArchitectureStyle.Unknown
            ? $"{model.DetectedStyle} ({model.StyleConfidence:P0})"
            : "Not detected";
        var signals = string.Join(" · ",
            model.Architecture.All.Values.Where(s => s.Detected).Select(s => s.Key));
        var profile = options.ProfileDisplayName ?? "focused";

        sb.AppendLine("<header class='dc-header'>");
        sb.AppendLine($"<h1 class='dc-title'>DevContext — {System.Net.WebUtility.HtmlEncode(name)}</h1>");
        sb.AppendLine("<div class='dc-meta'>");
        sb.AppendLine($"<span class='dc-badge dc-badge-style'>{System.Net.WebUtility.HtmlEncode(style)}</span>");
        sb.AppendLine($"<span class='dc-meta-text'>Signals: {System.Net.WebUtility.HtmlEncode(signals)}</span>");
        sb.AppendLine($"<span class='dc-meta-text'>Profile: {System.Net.WebUtility.HtmlEncode(profile)}</span>");
        sb.AppendLine($"<span class='dc-meta-text'>Tokens: ~{options.EstimatedTokens:N0}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine("</header>");
    }

    private static void RenderArchitectureOverview(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.ArchitectureOverview}'>");
        sb.AppendLine("<h2 class='dc-h2'>Architecture overview</h2>");
        if (model.Projects.Length == 0)
        {
            sb.AppendLine("<p>No projects discovered.</p>");
        }
        else if (options.ProjectGraph is not null && options.ProjectGraph.AdjacencyList.Count > 0)
        {
            sb.AppendLine("<pre class='dc-tree'>");
            var graph = options.ProjectGraph.AdjacencyList;
            var roots = graph.Keys.Where(k => !graph.Values.Any(v => v.Contains(k))).OrderBy(k => k).ToList();
            if (roots.Count == 0) roots = graph.Keys.OrderBy(k => k).ToList();
            var visited = new HashSet<string>();
            foreach (var root in roots)
                WriteTree(sb, root, graph, visited, "", true);
            sb.AppendLine("</pre>");
        }
        else
        {
            sb.AppendLine("<ul class='dc-projects'>");
            foreach (var p in model.Projects) sb.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(p.Name)}</li>");
            sb.AppendLine("</ul>");
        }
        sb.AppendLine("</section>");
    }

    private static void WriteTree(StringBuilder sb, string key, IReadOnlyDictionary<string, ImmutableArray<string>> graph,
        HashSet<string> visited, string indent, bool isLast)
    {
        if (!visited.Add(key)) return;
        var conn = isLast ? "└── " : "├── ";
        sb.AppendLine($"{System.Net.WebUtility.HtmlEncode(indent + conn + key)}");
        if (!graph.TryGetValue(key, out var children) || children.Length == 0) return;
        for (int i = 0; i < children.Length; i++)
            WriteTree(sb, children[i], graph, visited, indent + (isLast ? "    " : "│   "), i == children.Length - 1);
    }

    private static void RenderEntryPoints(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        sb.AppendLine("<section class='dc-section' id='dc-entry-points'>");
        sb.AppendLine("<h2 class='dc-h2'>Entry points</h2>");
        foreach (var fp in options.FocusPoints)
        {
            sb.AppendLine($"<div class='dc-entry'><code>{System.Net.WebUtility.HtmlEncode(fp.TypeName ?? fp.FilePath ?? "?")}</code>");
            if (fp.FilePath is not null)
                sb.AppendLine($"<span class='dc-path'{FullPathAttr(fp.FilePath)}>{RelPath(fp.FilePath)}</span>");
            sb.AppendLine("</div>");
        }
        sb.AppendLine("</section>");
    }

    private static void RenderEndpoints(StringBuilder sb, DiscoveryModel model)
    {
        var endpoints = model.Detections.OfType<EndpointDetection>()
            .Where(e => !e.SourceFile?.EndsWith("ChangePasswordEndpoint.cs") is not false).ToList();
        if (endpoints.Count == 0) return;

        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.Endpoints}'>");
        sb.AppendLine($"<h2 class='dc-h2'>Endpoints ({endpoints.Count} found)</h2>");
        sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'>");
        sb.AppendLine("<thead><tr><th>Method</th><th>Route</th><th>Handler</th><th>Source</th></tr></thead><tbody>");
        foreach (var ep in endpoints.OrderBy(e => e.HandlerType).ThenBy(e => e.HandlerMethod))
        {
            var src = System.Net.WebUtility.HtmlEncode($"{RelPath(ep.SourceFile)}{(ep.LineNumber > 0 ? $":{ep.LineNumber}" : "")}");
            sb.Append($"<tr><td class='dc-method dc-method-{ep.HttpMethod.ToLowerInvariant()}'>");
            sb.Append(ep.HttpMethod);
            sb.Append("</td>");
            sb.Append($"<td class='dc-route'><code>{System.Net.WebUtility.HtmlEncode(ep.RouteTemplate)}</code></td>");
            sb.Append($"<td class='dc-handler'>{System.Net.WebUtility.HtmlEncode($"{ep.HandlerType}.{ep.HandlerMethod}")}{AuthBadge(ep)}</td>");
            sb.Append($"<td class='dc-src'{FullPathAttr(ep.SourceFile)}>{src}</td></tr>");
            sb.AppendLine();
        }
        sb.AppendLine("</tbody></table></div>");
        sb.AppendLine("</section>");
    }

    private static void RenderCallGraph(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        if (options.CallGraph is null || options.CallGraph.Edges.Count == 0)
        {
            sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.CallGraph}'><h2 class='dc-h2'>Call graph</h2>");
            sb.AppendLine("<p class='dc-note'>Not available in current profile. Use Trace mode with an entry point and Debug profile.</p>");
            sb.AppendLine("</section>");
            return;
        }
        var callerKeys = options.FocusPoints
            .Where(fp => fp.TypeName is not null)
            .Select(fp => $"{fp.TypeName}.{fp.FilePath}")
            .Where(k => options.CallGraph.Edges.ContainsKey(k))
            .ToList();
        if (callerKeys.Count == 0)
            callerKeys = options.CallGraph.Edges.Keys.Take(3).ToList();

        sb.AppendLine("<section class='dc-section'>");
        sb.AppendLine("<h2 class='dc-h2'>Call graph</h2>");
        sb.AppendLine("<div class='dc-call-graph'>");
        var visited = new HashSet<string>();
        foreach (var key in callerKeys)
        {
            if (!options.CallGraph.Edges.TryGetValue(key, out _)) continue;
            sb.AppendLine($"<details class='dc-call-node' open><summary>{System.Net.WebUtility.HtmlEncode(key)}</summary><ul>");
            visited.Clear();
            visited.Add(key);
            RenderCallNode(sb, options.CallGraph, key, visited, 0, 5);
            sb.AppendLine("</ul></details>");
        }
        sb.AppendLine("</div></section>");
    }

    private static void RenderCallNode(StringBuilder sb, CallGraph graph, string key, HashSet<string> visited, int depth, int max)
    {
        if (depth >= max || !graph.Edges.TryGetValue(key, out var edges)) return;
        foreach (var edge in edges.Take(10))
        {
            var calleeKey = $"{edge.CalleeType}.{edge.CalleeMethod}";
            var loc = edge.CallSiteLocation is not null ? $" <span class='dc-path'>{System.Net.WebUtility.HtmlEncode(edge.CallSiteLocation)}</span>" : "";
            sb.Append($"<li><code>{System.Net.WebUtility.HtmlEncode(calleeKey)}</code>{loc}");
            if (visited.Add(calleeKey))
            {
                sb.AppendLine("<ul>");
                RenderCallNode(sb, graph, calleeKey, visited, depth + 1, max);
                sb.AppendLine("</ul>");
            }
            sb.AppendLine("</li>");
        }
    }

    private static void RenderMediatRHandlers(StringBuilder sb, DiscoveryModel model)
    {
        var handlers = model.Detections.OfType<MediatRHandlerDetection>().ToList();
        if (handlers.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.MediatRHandlers}'>");
        sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'><thead><tr><th>Kind</th><th>Request</th><th>Response</th><th>Handler</th></tr></thead><tbody>");
        foreach (var h in handlers.OrderBy(h => h.Kind.ToString()).ThenBy(h => h.HandlerType))
            sb.AppendLine($"<tr><td class='dc-kind'>{System.Net.WebUtility.HtmlEncode(h.Kind.ToString())}</td><td><code>{System.Net.WebUtility.HtmlEncode(h.RequestType ?? "?")}</code></td><td><code>{System.Net.WebUtility.HtmlEncode(h.ResponseType ?? "?")}</code></td><td><code>{System.Net.WebUtility.HtmlEncode(h.HandlerType)}</code></td></tr>");
        sb.AppendLine("</tbody></table></div></section>");
    }

    private static void RenderDataModel(StringBuilder sb, DiscoveryModel model)
    {
        var entities = model.Detections.OfType<EfEntityDetection>()
            .Where(e => e.DbContextType != "Migrations" && e.EntityType != "<OnModelCreating>").ToList();
        var migrationCount = model.Detections.OfType<EfEntityDetection>().Count(e => e.DbContextType == "Migrations");
        if (entities.Count == 0 && migrationCount == 0) return;

        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.DataModel}'>");
        if (entities.Count > 0)
        {
            foreach (var group in entities.GroupBy(e => e.DbContextType).OrderBy(g => g.Key))
            {
                sb.AppendLine($"<h3 class='dc-h3'>{System.Net.WebUtility.HtmlEncode(group.Key)}</h3>");
                sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'><thead><tr><th>Entity</th><th>Aggregate</th><th>Keys</th></tr></thead><tbody>");
                foreach (var e in group.OrderBy(e => e.EntityType))
                {
                    var agg = e.IsAggregate ? "✓" : "—";
                    var keys = e.KeyProperties.Length > 0 ? string.Join(", ", e.KeyProperties) : "—";
                    sb.AppendLine($"<tr><td><code>{System.Net.WebUtility.HtmlEncode(e.EntityType)}</code></td><td>{agg}</td><td>{System.Net.WebUtility.HtmlEncode(keys)}</td></tr>");
                }
                sb.AppendLine("</tbody></table></div>");
            }
        }
        if (migrationCount > 0)
            sb.AppendLine($"<p class='dc-note'>{migrationCount} EF Core migrations found.</p>");
        sb.AppendLine("</section>");
    }

    private static void RenderMessageConsumers(StringBuilder sb, DiscoveryModel model)
    {
        var consumers = model.Detections.OfType<MessageConsumerDetection>().ToList();
        if (consumers.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.MessageConsumers}'>");
        sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'><thead><tr><th>Bus</th><th>Message</th><th>Consumer</th></tr></thead><tbody>");
        foreach (var c in consumers.OrderBy(c => c.BusKind).ThenBy(c => c.MessageType))
            sb.AppendLine($"<tr><td>{System.Net.WebUtility.HtmlEncode(c.BusKind)}</td><td><code>{System.Net.WebUtility.HtmlEncode(c.MessageType)}</code></td><td><code>{System.Net.WebUtility.HtmlEncode(c.ConsumerType)}</code></td></tr>");
        sb.AppendLine("</tbody></table></div></section>");
    }

    private static void RenderIndirectWiring(StringBuilder sb, DiscoveryModel model)
    {
        var wiring = model.Detections.OfType<IndirectWiringDetection>().ToList();
        if (wiring.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.IndirectWiring}'><h2 class='dc-h2'>Indirect wiring</h2>");
        sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'><thead><tr><th>Kind</th><th>Caller</th><th>Target</th></tr></thead><tbody>");
        foreach (var w in wiring)
            sb.AppendLine($"<tr><td class='dc-wiring-{w.Kind.ToString().ToLowerInvariant()}'>{System.Net.WebUtility.HtmlEncode(w.Kind.ToString())}</td><td><code>{System.Net.WebUtility.HtmlEncode(w.CallerType)}.{System.Net.WebUtility.HtmlEncode(w.CallerMethod)}</code></td><td>{System.Net.WebUtility.HtmlEncode(w.TargetType ?? "unknown")}</td></tr>");
        sb.AppendLine("</tbody></table></div></section>");
    }

    private static void RenderBackgroundWorkers(StringBuilder sb, DiscoveryModel model)
    {
        var workers = model.Detections.OfType<BackgroundWorkerDetection>().ToList();
        if (workers.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.BackgroundWorkers}'>");
        sb.AppendLine("<ul class='dc-worker-list'>");
        foreach (var w in workers)
            sb.AppendLine($"<li><span class='dc-worker-kind'>{System.Net.WebUtility.HtmlEncode(w.Kind.ToString())}</span> <code>{System.Net.WebUtility.HtmlEncode(w.ImplementationType)}</code></li>");
        sb.AppendLine("</ul></section>");
    }

    private static void RenderMiddlewarePipeline(StringBuilder sb, DiscoveryModel model)
    {
        var middleware = model.Detections.OfType<MiddlewareDetection>().ToList();
        if (middleware.Count == 0) return;
        var grouped = middleware.GroupBy(m => m.MiddlewareType)
            .Select(g => new { Type = g.Key, Count = g.Count(), Kind = g.First().Kind, Sources = g.Select(m => System.IO.Path.GetFileName(m.SourceFile)).Distinct() })
            .OrderBy(m => m.Count).ToList();

        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.MiddlewarePipeline}'><h2 class='dc-h2'>Middleware pipeline</h2>");
        sb.AppendLine("<ol class='dc-pipeline'>");
        foreach (var m in grouped)
        {
            var src = string.Join(", ", m.Sources);
            sb.AppendLine($"<li><code>{System.Net.WebUtility.HtmlEncode(m.Type)}</code> <span class='dc-pipeline-count'>×{m.Count}</span> <span class='dc-pipeline-src'>{System.Net.WebUtility.HtmlEncode(src)}</span></li>");
        }
        sb.AppendLine("</ol></section>");
    }

    private static void RenderDiRegistrations(StringBuilder sb, DiscoveryModel model)
    {
        var regs = model.Detections.OfType<DiRegistrationDetection>().ToList();
        if (regs.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.DiRegistrations}'><h2 class='dc-h2'>DI registrations</h2>");
        sb.AppendLine("<div class='dc-di-grid'>");
        foreach (var d in regs)
        {
            var lt = d.Lifetime.ToLowerInvariant();
            var impl = d.ServiceType == d.ImplementationType || d.ImplementationType == "?"
                ? System.Net.WebUtility.HtmlEncode(d.ImplementationType)
                : $"{System.Net.WebUtility.HtmlEncode(d.ServiceType)} → {System.Net.WebUtility.HtmlEncode(d.ImplementationType)}";
            var src = $"{System.Net.WebUtility.HtmlEncode(System.IO.Path.GetFileName(d.SourceFile))}:{d.LineNumber}";
            var shape = d.Shape switch
            {
                DiRegistrationShape.ForwardingAlias => "alias",
                DiRegistrationShape.InlineFactory => "factory",
                _ => d.Lifetime == "Bulk" ? "bulk" : ""
            };
            var shapeTag = shape.Length > 0 ? $"<span class='dc-di-shape'>{shape}</span>" : "";
            sb.AppendLine($"<div class='dc-di-card dc-di-{lt}'><span class='dc-di-lt'>{System.Net.WebUtility.HtmlEncode(d.Lifetime)}</span> <code>{impl}</code> {shapeTag}<span class='dc-di-src'>{src}</span></div>");
        }
        sb.AppendLine("</div></section>");
    }

    private static void RenderAntiPatterns(StringBuilder sb, DiscoveryModel model)
    {
        var patterns = model.Detections.OfType<AntiPatternDetection>().ToList();
        if (patterns.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-antipatterns'><h2 class='dc-h2'>Anti-patterns detected</h2>");
        foreach (var g in patterns.GroupBy(p => System.IO.Path.GetFileName(p.SourceFile)).OrderBy(g => g.Key))
        {
            sb.AppendLine($"<details class='dc-ap-file'><summary>{System.Net.WebUtility.HtmlEncode(g.Key)} ({g.Count()})</summary><ul class='dc-ap-list'>");
            foreach (var p in g.OrderByDescending(p => p.Severity))
                sb.AppendLine($"<li class='dc-ap-{p.Severity.ToLowerInvariant()}'><strong>{System.Net.WebUtility.HtmlEncode(p.Pattern)}</strong>: {System.Net.WebUtility.HtmlEncode(p.Description)} <span class='dc-path'>line {p.LineNumber}</span></li>");
            sb.AppendLine("</ul></details>");
        }
        sb.AppendLine("</section>");
    }

    private static void RenderEventFlow(StringBuilder sb, DiscoveryModel model)
    {
        var events = model.Detections.OfType<EventFlowDetection>().ToList();
        if (events.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-eventflow'><h2 class='dc-h2'>Event flow</h2>");
        sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'><thead><tr><th>Event</th><th>Direction</th><th>Target</th></tr></thead><tbody>");
        foreach (var e in events.OrderBy(e => e.EventType))
            sb.AppendLine($"<tr><td><code>{System.Net.WebUtility.HtmlEncode(e.EventType)}</code></td><td>{System.Net.WebUtility.HtmlEncode(e.Kind)}</td><td><code>{System.Net.WebUtility.HtmlEncode(e.Target)}</code></td></tr>");
        sb.AppendLine("</tbody></table></div></section>");
    }

    private static void RenderRelatedTypes(StringBuilder sb, DiscoveryModel model, HashSet<string>? includedIds)
    {
        var types = includedIds is not null
            ? model.Types.Values.Where(t => includedIds.Contains(t.Id)).ToList()
            : model.Types.Values.Where(t => !t.IsHardExcluded).ToList();
        if (types.Count == 0) return;
        sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.RelatedTypes}'><h2 class='dc-h2'>Related types</h2>");
        sb.AppendLine("<div class='dc-types-grid'>");
        foreach (var group in types.GroupBy(t => t.Layer).OrderBy(g => g.Key))
        {
            sb.AppendLine($"<div class='dc-type-layer'><h4 class='dc-h4'>{System.Net.WebUtility.HtmlEncode(group.Key.ToString())}</h4><ul class='dc-type-list'>");
            foreach (var t in group.OrderBy(t => t.Name).Take(15))
                sb.AppendLine($"<li><code>{System.Net.WebUtility.HtmlEncode(t.Name)}</code></li>");
            sb.AppendLine("</ul></div>");
        }
        sb.AppendLine("</div></section>");
    }

    private static void RenderDiagnostics(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        sb.AppendLine($"<section class='dc-section' id='dc-diagnostics'><h2 class='dc-h2'>Diagnostics</h2>");
        if (model.Diagnostics.IsEmpty && model.PruningNotes.Count == 0 && options.Plan?.Excluded.IsDefaultOrEmpty != false)
        {
            sb.AppendLine("<p>No diagnostics recorded.</p></section>"); return;
        }

        if (!model.Diagnostics.IsEmpty)
        {
            sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'><thead><tr><th>Level</th><th>Source</th><th>Message</th></tr></thead><tbody>");
            foreach (var d in model.Diagnostics)
                sb.AppendLine($"<tr class='dc-diag-{d.Level.ToString().ToLowerInvariant()}'><td class='dc-diag-level'>{System.Net.WebUtility.HtmlEncode(d.Level.ToString())}</td><td>{System.Net.WebUtility.HtmlEncode(d.Source)}</td><td>{System.Net.WebUtility.HtmlEncode(d.Message)}</td></tr>");
            sb.AppendLine("</tbody></table></div>");
        }

        if (options.Plan is { Excluded.Length: > 0 } plan)
        {
            sb.AppendLine("<details class='dc-budget-cuts'><summary>Budget cuts — what almost made it</summary><ul>");
            foreach (var ex in plan.Excluded.OrderByDescending(e => e.Score).Take(20))
                sb.AppendLine($"<li><code>{System.Net.WebUtility.HtmlEncode(ex.TypeName)}</code> ({ex.Score:F3}) — {System.Net.WebUtility.HtmlEncode(ex.Reason)}</li>");
            sb.AppendLine("</ul></details>");
        }

        if (model.PruningNotes.Count > 0)
        {
            sb.AppendLine("<details class='dc-pruning'><summary>Pruning notes</summary><ul>");
            foreach (var note in model.PruningNotes)
                sb.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(note)}</li>");
            sb.AppendLine("</ul></details>");
        }
        sb.AppendLine("</section>");
    }

    private static void RenderFooter(StringBuilder sb, DiscoveryModel model, System.Diagnostics.Stopwatch sw, HashSet<string>? includedIds)
    {
        var active = includedIds is not null
            ? includedIds.Count
            : model.Types.Values.Count(t => !t.IsHardExcluded);
        var total = model.Types.Count;
        sb.AppendLine("<footer class='dc-footer'>");
        sb.AppendLine($"Generated in {sw.Elapsed.TotalMilliseconds:F1}ms · {total} types ({active} active, {total - active} pruned) · Schema v1.1");
        sb.AppendLine("</footer>");
    }
}
