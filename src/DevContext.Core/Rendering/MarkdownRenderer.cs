using System.Text;

using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Rendering;

/// <summary>Renders the discovery model as a human-readable Markdown document.</summary>
public sealed class MarkdownRenderer : IContextRenderer
{
    public string Format => "markdown";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var sb = new StringBuilder();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var sectionTokens = new List<SectionTokenRecord>();

        var includedIds = options.Plan is { } plan
            ? new HashSet<string>(plan.IncludedTypeIds, StringComparer.Ordinal)
            : null;

        RenderHeaderSection(sb, model, options, sectionTokens, includedIds);

        if (ShouldRender(SectionNames.ArchitectureOverview, options))
            RenderArchitectureSection(sb, model, options, sectionTokens);

        if (!options.FocusPoints.IsDefaultOrEmpty && options.FocusPoints.Length > 0)
            RenderFocusSections(sb, model, options, sectionTokens);

        RenderFeatureSections(sb, model, options, sectionTokens);
        RenderWiringSections(sb, model, options, sectionTokens);
        RenderAnalysisSections(sb, model, sectionTokens);

        if (ShouldRender(SectionNames.RelatedTypes, options))
            RenderTypeSection(sb, model, options, includedIds, sectionTokens);

        if (options.IncludeDiagnostics)
            RenderDiagnosticSection(sb, model, options, sectionTokens);

        AppendFooter(sb, model, options, sw, includedIds);

        if (options.TokenView)
            AppendTokenAccounting(sb, sectionTokens);

        var content = sb.ToString();
        var estimatedTokens = Math.Max(1, content.Length / 4);

        return new ValueTask<RenderedContext>(new RenderedContext(
            content, estimatedTokens, [.. model.AppliedCompressions], sw.Elapsed, "1.1",
            SectionTokens: sectionTokens.Count > 0 ? sectionTokens : null));
    }

    private static void TrackSection(List<SectionTokenRecord> records, string name, int prevLength, int currentLength)
    {
        if (currentLength <= prevLength) return; // skip empty sections
        var chars = currentLength - prevLength;
        var tokens = Math.Max(1, chars / 4);
        records.Add(new SectionTokenRecord(name, tokens, tokens, false));
    }

    private static void AppendTokenAccounting(StringBuilder sb, List<SectionTokenRecord> sections)
    {
        sb.AppendLine();
        sb.AppendLine("<!-- TOKEN ACCOUNTING (strip before sending to LLM)");
        sb.AppendLine("┌────────────────────────────────┬──────────┬──────────┬──────────┐");
        sb.AppendLine("│ Section                        │  Tokens  │  Pct     │ Trunc?   │");
        sb.AppendLine("├────────────────────────────────┼──────────┼──────────┼──────────┤");
        var total = sections.Sum(s => s.CompressedTokens);
        foreach (var s in sections)
        {
            var pct = total > 0 ? s.CompressedTokens * 100 / total : 0;
            var trunc = s.WasTruncated ? "Yes" : "No";
            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"│ {s.SectionName,-30} │ {s.CompressedTokens,8:N0} │ {pct,7}% │ {trunc,-8} │");
        }
        sb.AppendLine("├────────────────────────────────┼──────────┼──────────┼──────────┤");
        sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"│ {"TOTAL",-30} │ {total,8:N0} │ {"100%",7} │ {sections.Count(s => s.WasTruncated),8} truncated │");
        sb.AppendLine("└────────────────────────────────┴──────────┴──────────┴──────────┘");
        sb.AppendLine("-->");
    }

    private static void RenderHeaderSection(StringBuilder sb, DiscoveryModel model, RenderOptions options, List<SectionTokenRecord> sectionTokens, HashSet<string>? includedIds)
    {
        var preLen = sb.Length;
        AppendHeader(sb, model, options);
        AppendArchitecture(sb, model);
        AppendSignals(sb, model);
        AppendProjects(sb, model);
        AppendProfileAndTokens(sb, model, options, includedIds);
        TrackSection(sectionTokens, "Header", preLen, sb.Length);
        sb.AppendLine("---");
    }

    private static void AppendHeader(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        var scenarioName = options.ScenarioDisplayName ?? "Architecture Overview";
        var entryName = model.Solution?.Name ?? "project";

        sb.AppendLine($"## DevContext — {scenarioName} on {entryName}");
        sb.AppendLine();
    }

    private static void AppendArchitecture(StringBuilder sb, DiscoveryModel model)
    {
        var style = model.DetectedStyle != ArchitectureStyle.Unknown
            ? $"{model.DetectedStyle} ({model.StyleConfidence:P0} confidence)"
            : "Not detected";

        sb.AppendLine($"**Architecture**: {style}");
    }

    private static void AppendSignals(StringBuilder sb, DiscoveryModel model)
    {
        var signals = model.Architecture.All.Values.Where(s => s.Detected).ToList();
        if (signals.Count == 0)
        {
            sb.AppendLine("**Signals**: none");
            return;
        }

        var signalList = string.Join(" \u00b7 ", signals.Select(s => s.Key));
        sb.AppendLine($"**Signals**: {signalList}");
    }

    private static void AppendProjects(StringBuilder sb, DiscoveryModel model)
    {
        var count = model.Projects.Length;
        var names = count > 0
            ? string.Join(", ", model.Projects.Select(p => p.Name))
            : "none";

        sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"**Projects**: {count} — {names}");
    }

    private static void AppendProfileAndTokens(StringBuilder sb, DiscoveryModel model, RenderOptions options, HashSet<string>? includedIds)
    {
        var activeTypes = includedIds is not null
            ? includedIds.Count
            : model.Types.Values.Count(t => !t.IsHardExcluded);
        sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"**Profile**: {options.ProfileDisplayName ?? "default"} | **Tokens**: ~{options.EstimatedTokens} (budget {model.Budget.MaxTokens}) | **Types**: {activeTypes} in output");
        sb.AppendLine();
    }

    private static void RenderArchitectureSection(StringBuilder sb, DiscoveryModel model, RenderOptions options, List<SectionTokenRecord> sectionTokens)
    {
        var preLen = sb.Length;
        AppendArchitectureOverview(sb, model, options);
        TrackSection(sectionTokens, "Architecture overview", preLen, sb.Length);
    }

    private static void AppendArchitectureOverview(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        sb.AppendLine("## Architecture overview");
        sb.AppendLine();

        if (model.Projects.Length == 0)
        {
            sb.AppendLine("No projects discovered.");
            sb.AppendLine();
            return;
        }

        // Render project dependency graph if available
        if (options.ProjectGraph is not null && options.ProjectGraph.AdjacencyList.Count > 0)
        {
            var graph = options.ProjectGraph.AdjacencyList;
            var roots = graph.Keys
                .Where(k => !graph.Values.Any(v => v.Contains(k, StringComparer.Ordinal)))
                .Order(StringComparer.Ordinal)
                .ToList();

            if (roots.Count == 0) roots = graph.Keys.Order(StringComparer.Ordinal).ToList();

            sb.AppendLine("```text");
            var visited = new HashSet<string>(StringComparer.Ordinal);
            foreach (var root in roots)
            {
                RenderProjectTree(sb, root, graph, visited, "", true);
            }
            sb.AppendLine("```");
        }
        else
        {
            foreach (var project in model.Projects)
                sb.AppendLine($"- {project.Name}");
        }

        sb.AppendLine();
    }

    private static void RenderProjectTree(StringBuilder sb, string project,
        IReadOnlyDictionary<string, ImmutableArray<string>> graph,
        HashSet<string> visited, string indent, bool isLast)
    {
        if (!visited.Add(project)) return;

        var connector = isLast ? "└── " : "├── ";
        sb.AppendLine($"{indent}{connector}{project}");

        var deps = graph.TryGetValue(project, out var d) ? d : ImmutableArray<string>.Empty;
        var childIndent = indent + (isLast ? "    " : "│   ");

        for (int i = 0; i < deps.Length; i++)
            RenderProjectTree(sb, deps[i], graph, visited, childIndent, i == deps.Length - 1);
    }

    private static void RenderFocusSections(StringBuilder sb, DiscoveryModel model, RenderOptions options, List<SectionTokenRecord> sectionTokens)
    {
        var preLen = sb.Length;
        AppendEntryPoints(sb, model, options);
        TrackSection(sectionTokens, "Entry points", preLen, sb.Length);

        preLen = sb.Length;
        AppendSourceBodies(sb, model, options);
        TrackSection(sectionTokens, "Source code", preLen, sb.Length);
    }

    private static void AppendEntryPoints(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        if (options.FocusPoints.IsDefaultOrEmpty || options.FocusPoints.Length == 0) return;

        sb.AppendLine("## Entry points");
        sb.AppendLine();

        foreach (var focus in options.FocusPoints)
        {
            var type = model.Types.Values.FirstOrDefault(t => string.Equals(t.Name, focus.TypeName, StringComparison.Ordinal) || t.Id.EndsWith("." + focus.TypeName, StringComparison.Ordinal));
            if (type is null)
            {
                AppendUnresolvedFocusPoint(sb, focus, model);
                continue;
            }

            AppendFocusTypeDetail(sb, type, model);
        }
    }

    private static void AppendUnresolvedFocusPoint(StringBuilder sb, FocusPoint focus, DiscoveryModel model)
    {
        sb.AppendLine($"⚠ `--around {focus.TypeName}` not found.");
        if (focus.TypeName is not null)
        {
            var matches = model.Types.Values
                .Select(t => t.Name)
                .Distinct(StringComparer.Ordinal)
                .Select(n => (Name: n, Dist: StringHelpers.LevenshteinDistance(focus.TypeName, n)))
                .Where(x => x.Dist <= 3 && x.Dist > 0)
                .OrderBy(x => x.Dist)
                .Take(3)
                .Select(x => $"`{x.Name}`")
                .ToList();
            if (matches.Count > 0)
                sb.AppendLine($"  Did you mean: {string.Join(", ", matches)}?");
        }
        sb.AppendLine();
    }

    private static void AppendFocusTypeDetail(StringBuilder sb, TypeDiscovery type, DiscoveryModel model)
    {
        sb.AppendLine($"### `{type.Name}` ({type.Kind}, {type.Layer})");
        sb.AppendLine($"> `{type.Namespace}.{type.Name}` — {type.FilePath}");
        sb.AppendLine();

        if (type.ImplementedInterfaces.Length > 0)
            sb.AppendLine($"**Implements**: {string.Join(", ", type.ImplementedInterfaces.Select(i => $"`{i}`"))}");

        if (type.BaseTypes.Length > 0 && !string.Equals(type.BaseTypes[0], "object", StringComparison.Ordinal))
            sb.AppendLine($"**Extends**: `{type.BaseTypes[0]}`");

        var ctors = type.Methods.Where(m => string.Equals(m.Name, ".ctor", StringComparison.Ordinal) || string.Equals(m.Name, type.Name, StringComparison.Ordinal)).ToList();
        if (ctors.Count != 0 && ctors[0].ParameterTypes.Length > 0)
        {
            var deps = ctors[0].ParameterTypes.Zip(ctors[0].ParameterNames, (t2, n) => $"`{t2} {n}`");
            sb.AppendLine($"**Depends on**: {string.Join(", ", deps)}");

            // Cross-reference with DI registrations
            var diRegs = model.Detections.OfType<DiRegistrationDetection>()
                .Where(d => !d.ServiceType.StartsWith("Add", StringComparison.Ordinal) && !string.Equals(d.ServiceType, "?", StringComparison.Ordinal))
                .GroupBy(d => d.ServiceType, StringComparer.Ordinal)
                .ToDictionary(g => g.Key.Trim(), g => g.First(), StringComparer.Ordinal);

            var resolved = new List<string>();
            foreach (var paramType in ctors[0].ParameterTypes)
            {
                var trimmed = paramType.Trim();
                if (diRegs.TryGetValue(trimmed, out var reg))
                {
                    var source = $"{Path.GetFileName(reg.SourceFile)}:{reg.LineNumber}";
                    resolved.Add($"`{trimmed}` → `{reg.ImplementationType}` ({source})");
                }
                else if (trimmed.StartsWith("ILogger", StringComparison.Ordinal))
                {
                    resolved.Add($"`{trimmed}` → framework-provided");
                }
            }
            if (resolved.Count > 0)
                sb.AppendLine($"**Resolved to**: {string.Join(", ", resolved)}");
        }

        var publicMethods = type.Methods
            .Where(m => m.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public
                && !string.Equals(m.Name, ".ctor", StringComparison.Ordinal) && !string.Equals(m.Name, type.Name, StringComparison.Ordinal))
            .Take(8)
            .ToList();
        if (publicMethods.Count != 0)
        {
            sb.AppendLine();
            sb.AppendLine("**Methods**:");
            foreach (var method in publicMethods)
            {
                var paramStr = string.Join(", ", method.ParameterTypes.Zip(
                    method.ParameterNames, (t, n) => $"{t} {n}"));
                sb.AppendLine($"- `{method.ReturnType} {method.Name}({paramStr})`");
            }
        }

        sb.AppendLine();
    }

    private static void AppendSourceBodies(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        if (options.FocusPoints.IsDefaultOrEmpty || options.FocusPoints.Length == 0) return;

        // Collect types from focus points (entry points)
        var typeSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var f in options.FocusPoints)
        {
            foreach (var t in model.Types.Values)
            {
                if (string.Equals(t.Name, f.TypeName, StringComparison.Ordinal) || t.Id.EndsWith("." + f.TypeName, StringComparison.Ordinal))
                {
                    typeSet.Add(t.Id);
                    break;
                }
            }
        }

        // Also collect types from call graph chain (visited nodes)
        if (options.CallGraph is not null)
        {
            foreach (var kv in options.CallGraph.Edges)
            {
                foreach (var edge in kv.Value)
                {
                    if (typeSet.Count >= 5) break;
                    foreach (var t in model.Types.Values)
                    {
                        if (string.Equals(t.Id, edge.CalleeType, StringComparison.Ordinal) || t.Id.EndsWith("." + edge.CalleeType, StringComparison.Ordinal))
                        {
                            typeSet.Add(t.Id);
                            break;
                        }
                    }
                }
                if (typeSet.Count >= 5) break;
            }
        }

        var typesWithBodies = model.Types.Values
            .Where(t => typeSet.Contains(t.Id) && t.SourceBody is not null)
            .Take(5)
            .ToList();

        if (typesWithBodies.Count == 0) return;

        sb.AppendLine("## Source code");
        sb.AppendLine();

        foreach (var type in typesWithBodies)
        {
            if (type.SourceBody is null) continue;

            var body = type.SourceBody;
            var cap = options.Plan?.PerTypeCharCap ?? int.MaxValue;
            if (cap > 0 && body.Length > cap)
            {
                body = TruncateBody(body, cap);
            }

            sb.AppendLine($"### {type.Name}.cs");
            sb.AppendLine("```csharp");
            sb.AppendLine(body);

            if (type.SourceBody.Length >= 5000)
                sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"// ... [{type.SourceBody.Length} total chars]");

            sb.AppendLine("```");
            sb.AppendLine();
        }
    }

    private static void RenderFeatureSections(StringBuilder sb, DiscoveryModel model, RenderOptions options, List<SectionTokenRecord> sectionTokens)
    {
        if (ShouldRender(SectionNames.Endpoints, options))
        {
            var preLen = sb.Length;
            AppendEndpoints(sb, model, options);
            TrackSection(sectionTokens, "Endpoints", preLen, sb.Length);
        }
        if (ShouldRender(SectionNames.CallGraph, options))
        {
            var preLen = sb.Length;
            AppendCallGraphAvailability(sb, model, options);
            TrackSection(sectionTokens, "Call graph", preLen, sb.Length);
        }
        if (ShouldRender(SectionNames.MediatRHandlers, options))
        {
            var preLen = sb.Length;
            AppendMediatRHandlers(sb, model);
            TrackSection(sectionTokens, "MediatR Handlers", preLen, sb.Length);
        }
        if (ShouldRender(SectionNames.DataModel, options))
        {
            var preLen = sb.Length;
            AppendEfEntities(sb, model);
            TrackSection(sectionTokens, "Data model", preLen, sb.Length);
        }
        if (ShouldRender(SectionNames.MessageConsumers, options))
        {
            var preLen = sb.Length;
            AppendMessageConsumers(sb, model);
            TrackSection(sectionTokens, "Message consumers", preLen, sb.Length);
        }
    }

    private static void AppendEndpoints(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        sb.AppendLine($"## {SectionNames.Endpoints}");
        sb.AppendLine();

        var allEndpoints = model.Detections.OfType<EndpointDetection>()
            .Where(e => !IsFrameworkEndpoint(e));

        allEndpoints = FilterEndpointsByFocusPoints(allEndpoints, options);

        var endpoints = allEndpoints.ToList();

        if (endpoints.Count == 0)
        {
            sb.AppendLine("No endpoints detected.");
            sb.AppendLine();
            return;
        }

        var hasGroupPrefix = endpoints.Exists(e => e.GroupPrefix is not null);

        // Build project lookup from source file paths
        var projectByDir = model.Projects
            .Select(p => (Dir: Path.GetDirectoryName(p.FilePath) ?? "", Name: p.Name))
            .Where(p => !string.IsNullOrEmpty(p.Dir))
            .OrderByDescending(p => p.Dir.Length)
            .ToList();

        string ProjectForFile(string filePath) =>
            projectByDir.FirstOrDefault(kv => filePath.StartsWith(kv.Dir, StringComparison.OrdinalIgnoreCase)).Name
            ?? Path.GetFileName(Path.GetDirectoryName(filePath)) ?? "?";

        // Group endpoints by project
        var byProject = endpoints.GroupBy(ep => ProjectForFile(ep.SourceFile), StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal);

        // Collect MediatR handler types for linkage
        var mediatRTypes = model.Detections.OfType<MediatRHandlerDetection>()
            .Select(m => m.HandlerType)
            .ToHashSet(StringComparer.Ordinal);

        var header = hasGroupPrefix
            ? "| Method | Route | Group | Handler | Auth | Source |"
            : "| Method | Route | Handler | Auth | Source |";
        var sep = hasGroupPrefix
            ? "|--------|-------|-------|---------|------|--------|"
            : "|--------|-------|---------|------|--------|";

        foreach (var projectGroup in byProject)
        {
            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"**{projectGroup.Key}** ({projectGroup.Count()} endpoints)");
            sb.AppendLine(header);
            sb.AppendLine(sep);

            foreach (var ep in projectGroup)
            {
                var auth = FormatAuth(ep);
                var handler = FormatHandler(ep);

                // C3: MediatR linkage — check if handler type is a known MediatR handler
                if (mediatRTypes.Contains(ep.HandlerType))
                {
                    var mediatR = model.Detections.OfType<MediatRHandlerDetection>()
                        .FirstOrDefault(m => string.Equals(m.HandlerType, ep.HandlerType, StringComparison.Ordinal));
                    if (mediatR is not null)
                        handler += $" → `{mediatR.RequestType}`";
                }

                var source = $"{Path.GetFileName(ep.SourceFile)}:{ep.LineNumber}";

                if (hasGroupPrefix)
                {
                    var group = ep.GroupPrefix ?? "-";
                    sb.AppendLine($"| {ep.HttpMethod} | {ep.RouteTemplate} | {group} | {handler} | {auth} | {source} |");
                }
                else
                {
                    sb.AppendLine($"| {ep.HttpMethod} | {ep.RouteTemplate} | {handler} | {auth} | {source} |");
                }
            }
            sb.AppendLine();
        }
    }

    private static IEnumerable<EndpointDetection> FilterEndpointsByFocusPoints(IEnumerable<EndpointDetection> endpoints, RenderOptions options)
    {
        if (options.FocusPoints.IsDefaultOrEmpty)
            return endpoints;

        var focusDirs = options.FocusPoints
            .Where(fp => fp.FilePath is not null)
            .Select(fp => Path.GetDirectoryName(fp.FilePath)?.Replace('\\', '/') ?? "")
            .Where(d => d.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (focusDirs.Count == 0)
            return endpoints;

        return endpoints.Where(e =>
        {
            if (e.SourceFile is null) return true;
            var dir = Path.GetDirectoryName(e.SourceFile)?.Replace('\\', '/') ?? "";
            return focusDirs.Any(fd =>
                dir.StartsWith(fd, StringComparison.OrdinalIgnoreCase)
                || fd.StartsWith(dir, StringComparison.OrdinalIgnoreCase)
                || AreSiblingDirectories(dir, fd));
        });
    }

    private static bool ShouldRender(string sectionName, RenderOptions options)
    {
        if (options.RequiredSections.IsDefaultOrEmpty) return true;
        return options.RequiredSections.Contains(sectionName, StringComparer.Ordinal);
    }

    private static string FormatAuth(EndpointDetection ep)
    {
        return ep.AuthAttributes.Length > 0 ? string.Join(", ", ep.AuthAttributes) : "-";
    }

    private static bool AreSiblingDirectories(string dirA, string dirB)
    {
        var parentA = Path.GetDirectoryName(dirA)?.Replace('\\', '/') ?? "";
        var parentB = Path.GetDirectoryName(dirB)?.Replace('\\', '/') ?? "";
        return string.Equals(parentA, parentB, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFrameworkEndpoint(EndpointDetection ep)
    {
        // Filter routes from known framework/infrastructure files
        var fileName = Path.GetFileName(ep.SourceFile);
        if (fileName is "OpenApi.Extensions.cs" or "Extensions.cs")
            return true;

        // Filter known framework route patterns
        var route = ep.RouteTemplate;
        if (route is "/" or "/health" or "/alive")
            return true;

        return false;
    }

    private static string FormatHandler(EndpointDetection ep)
    {
        var handler = ep.HandlerMethod;
        if (handler is "<lambda>" or "<anonymous>")
        {
            var handlerText = ep.HandlerType;
            // Truncate very long lambda bodies to a compact reference
            if (handlerText.Length > 80)
                return $"λ {Path.GetFileName(ep.SourceFile)}:{ep.LineNumber}";
            return handlerText.Length > 40 ? handlerText[..37] + "..." : handlerText;
        }
        return $"{ep.HandlerType}.{handler}";
    }

    private static string FormatImplementation(string implementationType, ImmutableArray<string> extensionsUsed)
    {
        // Filter unresolvable extension args
        if (string.Equals(implementationType, "?", StringComparison.Ordinal) || string.IsNullOrEmpty(implementationType))
            return extensionsUsed.Length > 0
                ? $"({string.Join(", ", extensionsUsed.Take(3))})"
                : "-";

        // Truncate delegate bodies (lambdas with => or { )
        if (implementationType.Length > 80)
        {
            var firstLine = implementationType.Split('\n')[0];
            return firstLine.Length > 60
                ? firstLine[..57] + "..."
                : firstLine + "...";
        }

        return implementationType;
    }

    private static string FormatDiShape(DiRegistrationDetection d)
    {
        var impl = FormatImplementation(d.ImplementationType, d.ExtensionsUsed);
        return d.Shape switch
        {
            DiRegistrationShape.ForwardingAlias => $"{impl} (alias)",
            DiRegistrationShape.InlineFactory when d.FactorySummary is not null => d.FactorySummary,
            DiRegistrationShape.DirectBinding => string.Equals(d.ServiceType, d.ImplementationType
, StringComparison.Ordinal) ? impl
                : $"{d.ServiceType} → {impl}",
            _ => impl,
        };
    }

    private static void AppendCallGraphAvailability(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        if (options.CallGraph is null || options.CallGraph.Edges.Count == 0)
        {
        sb.AppendLine($"## {SectionNames.CallGraph}");
            sb.AppendLine();
            sb.AppendLine("Not available in current profile. Re-run with `--profile debug` to enable call graph extraction and BFS reachability analysis from entry points.");
            sb.AppendLine();
            return;
        }

        // Determine which caller keys to focus on
        var callerKeys = options.FocusPoints.IsDefaultOrEmpty
            ? options.CallGraph.Edges.Keys.Take(5).ToList()
            : options.FocusPoints
                .Select(f => options.CallGraph.Edges.Keys.FirstOrDefault(k =>
                    k.StartsWith(f.TypeName ?? "", StringComparison.OrdinalIgnoreCase)))
                .Where(k => k is not null)
                .Cast<string>()
                .DefaultIfEmpty(options.CallGraph.Edges.Keys.FirstOrDefault() ?? "")
                .Take(3)
                .ToList();

        if (callerKeys.Count == 0 || callerKeys.TrueForAll(string.IsNullOrEmpty))
            callerKeys = options.CallGraph.Edges.Keys.Take(3).ToList();

        sb.AppendLine("## Call graph");
        sb.AppendLine();

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var maxDepth = 5;

        sb.AppendLine("```text");
        foreach (var callerKey in callerKeys)
        {
            if (!options.CallGraph.Edges.ContainsKey(callerKey)) continue;

            var parts = callerKey.Split('.');
            var callerType = parts.Length > 1 ? string.Join('.', parts[..^1]) : callerKey;
            var callerMethod = parts.Length > 0 ? parts[^1] : callerKey;

            sb.AppendLine($"**{callerType}.{callerMethod}**");
            visited.Clear();
            visited.Add(callerKey);
            RenderCallGraphNode(sb, options.CallGraph, callerKey, "", visited, 0, maxDepth);
            sb.AppendLine();
        }
        sb.AppendLine("```");
    }

    private static void RenderCallGraphNode(StringBuilder sb, CallGraph graph, string callerKey,
        string indent, HashSet<string> visited, int depth, int maxDepth)
    {
        if (depth >= maxDepth) return;
        if (!graph.Edges.TryGetValue(callerKey, out var edges)) return;

        var edgeList = edges.Take(12).ToList();
        for (int i = 0; i < edgeList.Count; i++)
        {
            var edge = edgeList[i];
            var isLast = i == edgeList.Count - 1;
            var connector = isLast ? "└─ " : "├─ ";
            var location = edge.CallSiteLocation is not null ? $" `({edge.CallSiteLocation})`" : "";
            sb.AppendLine($"{indent}{connector}`{edge.CalleeType}.{edge.CalleeMethod}`{location}");

            var calleeKey = $"{edge.CalleeType}.{edge.CalleeMethod}";
            if (visited.Add(calleeKey))
            {
                var childIndent = indent + (isLast ? "   " : "│  ");
                RenderCallGraphNode(sb, graph, calleeKey, childIndent, visited, depth + 1, maxDepth);
            }
        }
    }

    private static void AppendMediatRHandlers(StringBuilder sb, DiscoveryModel model)
    {
        var handlers = model.Detections.OfType<MediatRHandlerDetection>().ToList();
        if (handlers.Count == 0) return;

        sb.AppendLine($"## {SectionNames.MediatRHandlers}");
        sb.AppendLine();

        sb.AppendLine("| Kind | Request | Response | Handler |");
        sb.AppendLine("|------|---------|----------|---------|");
        foreach (var h in handlers)
        {
            sb.AppendLine($"| {h.Kind} | {h.RequestType} | {h.ResponseType} | {h.HandlerType} |");
        }

        sb.AppendLine();
    }

    private static void AppendEfEntities(StringBuilder sb, DiscoveryModel model)
    {
        var entities = model.Detections.OfType<EfEntityDetection>()
            .Where(e => !string.Equals(e.DbContextType, "Migrations", StringComparison.Ordinal) && !string.Equals(e.EntityType, "<OnModelCreating>", StringComparison.Ordinal))
            .ToList();
        var migrationCount = model.Detections.OfType<EfEntityDetection>()
            .Count(e => string.Equals(e.DbContextType, "Migrations", StringComparison.Ordinal));

        if (entities.Count == 0 && migrationCount == 0) return;

        sb.AppendLine($"## {SectionNames.DataModel} (EF Core)");
        sb.AppendLine();

        if (entities.Count > 0)
        {
            var byContext = entities.GroupBy(e => e.DbContextType, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal);
            foreach (var group in byContext)
            {
                sb.AppendLine($"### `{group.Key}`");
                sb.AppendLine();
                sb.AppendLine("| Entity | Aggregate root | Key properties |");
                sb.AppendLine("|--------|---------------|----------------|");
                foreach (var e in group.OrderBy(e => e.EntityType, StringComparer.Ordinal))
                {
                    var keys = e.KeyProperties.Length > 0 ? string.Join(", ", e.KeyProperties) : "—";
                    var agg = e.IsAggregate ? "✓" : "—";
                    sb.AppendLine($"| `{e.EntityType}` | {agg} | {keys} |");
                }
                sb.AppendLine();
            }
        }

        if (migrationCount > 0)
        {
            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"**{migrationCount} EF Core migrations found.**");
            sb.AppendLine();
        }
    }

    private static void AppendMessageConsumers(StringBuilder sb, DiscoveryModel model)
    {
        var consumers = model.Detections.OfType<MessageConsumerDetection>().ToList();
        if (consumers.Count == 0) return;

        sb.AppendLine($"## {SectionNames.MessageConsumers}");
        sb.AppendLine();
        sb.AppendLine("| Bus | Message type | Consumer |");
        sb.AppendLine("|-----|-------------|---------|");
        foreach (var c in consumers.OrderBy(c => c.BusKind, StringComparer.Ordinal).ThenBy(c => c.MessageType, StringComparer.Ordinal))
            sb.AppendLine($"| {c.BusKind} | `{c.MessageType}` | `{c.ConsumerType}` |");
        sb.AppendLine();
    }

    private static void RenderWiringSections(StringBuilder sb, DiscoveryModel model, RenderOptions options, List<SectionTokenRecord> sectionTokens)
    {
        if (ShouldRender(SectionNames.NonObviousWiring, options)
            || ShouldRender(SectionNames.IndirectWiring, options))
        {
            var preLen = sb.Length;
            var rendered = AppendIndirectWiring(sb, model);
            if (rendered) TrackSection(sectionTokens, "Indirect wiring", preLen, sb.Length);
        }

        if (ShouldRender(SectionNames.NonObviousWiring, options)
            || ShouldRender(SectionNames.BackgroundWorkers, options))
        {
            var preLen = sb.Length;
            var rendered = AppendBackgroundWorkers(sb, model);
            if (rendered) TrackSection(sectionTokens, "Background workers", preLen, sb.Length);
        }

        if (ShouldRender(SectionNames.NonObviousWiring, options)
            || ShouldRender(SectionNames.MiddlewarePipeline, options))
        {
            var preLen = sb.Length;
            var rendered = AppendMiddlewarePipeline(sb, model);
            if (rendered) TrackSection(sectionTokens, "Middleware pipeline", preLen, sb.Length);
        }

        if (ShouldRender(SectionNames.NonObviousWiring, options)
            || ShouldRender(SectionNames.DiRegistrations, options))
        {
            var preLen = sb.Length;
            var rendered = AppendDiRegistrations(sb, model);
            if (rendered) TrackSection(sectionTokens, "DI registrations", preLen, sb.Length);
        }
    }

    private static bool AppendIndirectWiring(StringBuilder sb, DiscoveryModel model)
    {
        var wiring = model.Detections.OfType<IndirectWiringDetection>().ToList();
        if (wiring.Count == 0) return false;

        sb.AppendLine($"## {SectionNames.IndirectWiring}");
        sb.AppendLine();
        sb.AppendLine("| Kind | Caller | Target |");
        sb.AppendLine("|------|--------|--------|");
        foreach (var w in wiring)
            sb.AppendLine($"| {w.Kind} | {w.CallerType}.{w.CallerMethod} | {w.TargetType ?? "unknown"} |");
        sb.AppendLine();
        return true;
    }

    private static bool AppendBackgroundWorkers(StringBuilder sb, DiscoveryModel model)
    {
        var workers = model.Detections.OfType<BackgroundWorkerDetection>().ToList();
        if (workers.Count == 0) return false;

        sb.AppendLine($"## {SectionNames.BackgroundWorkers}");
        sb.AppendLine();
        foreach (var w in workers)
            sb.AppendLine($"- {w.ImplementationType} ({w.Kind})");
        sb.AppendLine();
        return true;
    }

    private static bool AppendMiddlewarePipeline(StringBuilder sb, DiscoveryModel model)
    {
        var middleware = model.Detections.OfType<MiddlewareDetection>().ToList();
        if (middleware.Count == 0) return false;

        sb.AppendLine($"## {SectionNames.MiddlewarePipeline}");
        sb.AppendLine();

        var grouped = middleware
            .GroupBy(m => m.MiddlewareType, StringComparer.Ordinal)
            .Select(g => new
            {
                Type = g.Key,
                Count = g.Count(),
                Kind = g.First().Kind,
                Sources = g.Select(m => Path.GetFileName(m.SourceFile)).Distinct(StringComparer.Ordinal)
            })
            .OrderBy(m => m.Count)
            .ToList();

        sb.AppendLine("| Type | Kind | Count | Sources |");
        sb.AppendLine("|------|------|-------|---------|");
        foreach (var m in grouped)
        {
            var sources = string.Join(", ", m.Sources);
            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"| {m.Type} | {m.Kind} | {m.Count} | {sources} |");
        }
        sb.AppendLine();
        return true;
    }

    private static bool AppendDiRegistrations(StringBuilder sb, DiscoveryModel model)
    {
        var diRegs = model.Detections.OfType<DiRegistrationDetection>().ToList();
        if (diRegs.Count == 0) return false;

        sb.AppendLine($"## {SectionNames.DiRegistrations}");
        sb.AppendLine();
        sb.AppendLine("| Lifetime | Service | Implementation | Source |");
        sb.AppendLine("|----------|---------|----------------|--------|");
        foreach (var d in diRegs)
        {
            var impl = FormatDiShape(d);
            var source = $"{Path.GetFileName(d.SourceFile)}:{d.LineNumber}";
            sb.AppendLine($"| {d.Lifetime} | {d.ServiceType} | {impl} | {source} |");
        }
        sb.AppendLine();
        return true;
    }

    private static void RenderAnalysisSections(StringBuilder sb, DiscoveryModel model, List<SectionTokenRecord> sectionTokens)
    {
        var preLen = sb.Length;
        AppendAntiPatterns(sb, model);
        TrackSection(sectionTokens, "Anti-patterns", preLen, sb.Length);

        preLen = sb.Length;
        AppendEventFlow(sb, model);
        TrackSection(sectionTokens, "Event flow", preLen, sb.Length);
    }

    private static void AppendAntiPatterns(StringBuilder sb, DiscoveryModel model)
    {
        var patterns = model.Detections.OfType<AntiPatternDetection>().ToList();
        if (patterns.Count == 0) return;

        sb.AppendLine($"## {SectionNames.AntiPatterns}");
        sb.AppendLine();

        // Group by source file for compact readability
        var byFile = patterns
            .GroupBy(p => Path.GetFileName(p.SourceFile), StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ToList();

        if (byFile.Count == 1)
        {
            // Single file — flat table
            AppendAntiPatternTable(sb, patterns);
        }
        else
        {
            foreach (var fileGroup in byFile)
            {
                var grouped = fileGroup
                    .OrderBy(p => p.Severity switch { "high" => 0, "medium" => 1, _ => 2 })
                    .ToList();

                sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"### {fileGroup.Key} ({grouped.Count})");
                sb.AppendLine();
                AppendAntiPatternTable(sb, grouped);
            }
        }
    }

    private static void AppendAntiPatternTable(StringBuilder sb, List<AntiPatternDetection> patterns)
    {
        sb.AppendLine("| Severity | Pattern | Description |");
        sb.AppendLine("|----------|---------|-------------|");

        foreach (var p in patterns.OrderBy(p => p.Severity switch { "high" => 0, "medium" => 1, _ => 2 }).Take(40))
            sb.AppendLine($"| {p.Severity} | {p.Pattern} | {p.Description} |");

        sb.AppendLine();
    }

    private static void AppendEventFlow(StringBuilder sb, DiscoveryModel model)
    {
        var flows = model.Detections.OfType<EventFlowDetection>().ToList();
        if (flows.Count == 0) return;

        var busKind = flows[0].BusKind;

        sb.AppendLine(string.Equals(busKind, "in-memory"
, StringComparison.Ordinal) ? "## Event flow (in-memory bus)"
            : "## Event flow");
        sb.AppendLine();

        // Group by event type
        var byEvent = flows.Where(f => !string.Equals(f.Kind, "Handler", StringComparison.Ordinal))
            .GroupBy(f => f.EventType, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        if (byEvent.Count > 0)
        {
            sb.AppendLine("| Event | Direction | Target | Source |");
            sb.AppendLine("|-------|-----------|--------|--------|");
            foreach (var group in byEvent)
            {
                foreach (var flow in group)
                {
                    var direction = string.Equals(flow.Kind, "Subscribe", StringComparison.Ordinal) ? "← subscribed" : "→ published";
                    var source = flow.SourceFile is not null && flow.LineNumber > 0
                        ? $"{Path.GetFileName(flow.SourceFile)}:{flow.LineNumber}"
                        : "-";
                    sb.AppendLine($"| `{flow.EventType}` | {direction} | `{flow.Target}` | {source} |");
                }
            }
            sb.AppendLine();
        }

        // Show handler implementations via IEventHandler<T>
        var handlers = flows.Where(f => string.Equals(f.Kind, "Handler", StringComparison.Ordinal)).ToList();
        if (handlers.Count > 0)
        {
            sb.AppendLine("### IEventHandler implementations");
            sb.AppendLine();
            foreach (var h in handlers)
                sb.AppendLine($"- `{h.EventType}` → `{h.Target}`");
            sb.AppendLine();
        }
    }

    private static void RenderTypeSection(StringBuilder sb, DiscoveryModel model, RenderOptions options, HashSet<string>? includedIds, List<SectionTokenRecord> sectionTokens)
    {
        var preLen = sb.Length;
        AppendRelatedTypesByLayer(sb, model, includedIds, options);
        TrackSection(sectionTokens, "Related types", preLen, sb.Length);
    }

    private static void AppendRelatedTypesByLayer(StringBuilder sb, DiscoveryModel model, HashSet<string>? includedIds, RenderOptions options)
    {
        var hasDetections = !model.Detections.IsEmpty;

        if (hasDetections)
            AppendRelatedTypesWithDetections(sb, model, includedIds, options);
        else
            AppendRelatedTypesByNamespaceView(sb, model, includedIds);
    }

    private static void AppendRelatedTypesWithDetections(StringBuilder sb, DiscoveryModel model, HashSet<string>? includedIds, RenderOptions options)
    {
        sb.AppendLine($"## {SectionNames.RelatedTypes}");
        sb.AppendLine();

        IEnumerable<TypeDiscovery> surviving = includedIds is not null
            ? model.Types.Values.Where(t => includedIds.Contains(t.Id))
            : model.Types.Values.Where(t => !t.IsHardExcluded);

        // Build rank lookup from plan order; lower index = higher rank
        var rankByType = new Dictionary<string, int>(StringComparer.Ordinal);
        if (options.Plan is { } plan)
        {
            for (var i = 0; i < plan.IncludedTypeIds.Length; i++)
                rankByType[plan.IncludedTypeIds[i]] = i;
        }

        var typedTypes = surviving
            .GroupBy(t => t.Layer)
            .OrderBy(g => g.Key.ToString(), StringComparer.Ordinal);

        var hasContent = false;

        foreach (var group in typedTypes)
        {
            var ordered = rankByType.Count > 0
                ? group.OrderBy(t => rankByType.TryGetValue(t.Id, out var r) ? r : int.MaxValue).ThenBy(t => t.Name, StringComparer.Ordinal)
                : group.OrderBy(t => t.Name, StringComparer.Ordinal);
            var typeList = string.Join(", ", ordered.Select(t => t.Name));
            sb.AppendLine($"- **{group.Key}**: {typeList}");
            hasContent = true;
        }

        if (!hasContent)
            sb.AppendLine("No types discovered.");

        sb.AppendLine();
    }

    private static void AppendRelatedTypesByNamespaceView(StringBuilder sb, DiscoveryModel model, HashSet<string>? includedIds)
    {
        sb.AppendLine("## Types by namespace");
        sb.AppendLine();

        var hasContent = false;
        var survivingTypes = includedIds is not null
            ? model.Types.Values.Where(t => includedIds.Contains(t.Id)).ToList()
            : model.Types.Values.Where(t => !t.IsHardExcluded).ToList();

        foreach (var nsGroup in survivingTypes
            .GroupBy(t => t.Namespace, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var types = nsGroup.ToList();
            var publicCount = types.Count(t => t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public);
            var totalCount = types.Count;
            var publicTypes = types
                .Where(t => t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
                .Select(t => t.Name)
                .ToList();

            sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"- **{nsGroup.Key}** — {totalCount} types");
            if (publicCount > 0 && publicCount < totalCount)
                sb.Append(System.Globalization.CultureInfo.InvariantCulture, $" ({publicCount} public)");
            sb.AppendLine();

            if (publicCount > 0 && publicTypes.Count <= 10)
            {
                sb.AppendLine($"  Public: {string.Join(", ", publicTypes)}");
            }
            else if (publicCount > 10)
            {
                var sample = string.Join(", ", publicTypes.Take(10));
                sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"  Public ({publicCount}): {sample} ...");
            }
            hasContent = true;
        }

        if (!hasContent)
            sb.AppendLine("No types discovered.");

        sb.AppendLine();
    }

    private static void RenderDiagnosticSection(StringBuilder sb, DiscoveryModel model, RenderOptions options, List<SectionTokenRecord> sectionTokens)
    {
        var preLen = sb.Length;
        AppendDiagnostics(sb, model, options);
        TrackSection(sectionTokens, "Diagnostics", preLen, sb.Length);
    }

    private static void AppendDiagnostics(StringBuilder sb, DiscoveryModel model, RenderOptions options)
    {
        sb.AppendLine("## Diagnostics");
        sb.AppendLine();

        var diagnostics = model.Diagnostics.ToList();
        if (diagnostics.Count == 0 && model.PruningNotes.Count == 0 && options.Plan?.Excluded.IsDefaultOrEmpty != false)
        {
            sb.AppendLine("No diagnostics recorded.");
            sb.AppendLine();
            return;
        }

        if (diagnostics.Count > 0)
        {
            sb.AppendLine("| Level | Source | Message |");
            sb.AppendLine("|-------|--------|---------|");
            foreach (var diag in diagnostics)
                sb.AppendLine($"| {diag.Level} | {diag.Source} | {diag.Message} |");

            sb.AppendLine();
        }

        if (options.Plan is { Excluded.Length: > 0 } plan)
        {
            sb.AppendLine("### Budget cuts (what almost made it)");
            sb.AppendLine();
            sb.AppendLine("| Type | Score | Reason |");
            sb.AppendLine("|------|-------|--------|");
            foreach (var ex in plan.Excluded.OrderByDescending(e => e.Score).Take(20))
                sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"| `{ex.TypeName}` | {ex.Score:F3} | {ex.Reason} |");
            sb.AppendLine();
        }

        if (model.PruningNotes.Count > 0)
        {
            sb.AppendLine("### Pruning notes");
            sb.AppendLine();
            foreach (var note in model.PruningNotes)
                sb.AppendLine($"- {note}");
            sb.AppendLine();
        }
    }

    private static void AppendFooter(StringBuilder sb, DiscoveryModel model,
        RenderOptions options, System.Diagnostics.Stopwatch sw, HashSet<string>? includedIds)
    {
        var typesTotal = model.Types.Count;
        var typesSurviving = includedIds is not null
            ? includedIds.Count
            : model.Types.Values.Count(t => !t.IsHardExcluded);
        var prunedCount = typesTotal - typesSurviving;

        var compressionSummary = model.AppliedCompressions
            .Where(r => r.TokensBefore != r.TokensAfter)
            .Select(r =>
            {
                var pct = r.TokensBefore > 0
                    ? (r.TokensBefore - r.TokensAfter) * 100 / r.TokensBefore
                    : 0;
                return $"{r.StrategyName}(−{pct}%)";
            })
            .ToList();

        var compressionText = compressionSummary.Count > 0
            ? $" | Compression: {string.Join(" · ", compressionSummary)}"
            : "";

        sb.AppendLine("---");
        sb.Append("*Generated in ").Append(sw.Elapsed.TotalMilliseconds.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)).Append("ms | ");
        sb.Append(typesTotal).Append(" types (").Append(typesSurviving).Append(" active, ").Append(prunedCount).Append(" pruned)");
        sb.Append(compressionText);
        sb.AppendLine(" | Schema v1.1*");

        // Usage hints (only when output is broad and no focus points)
        if (typesSurviving > 50 && options.FocusPoints.IsDefaultOrEmpty)
        {
            sb.AppendLine();
            sb.AppendLine("> 💡 Narrow this output with `--around TypeName` or `--around TypeName:MethodName` for focused context.");
        }
    }

    private static string TruncateBody(string body, int charCap)
    {
        var lines = body.Split('\n');
        var result = new List<string>(lines.Length);
        var charCount = 0;

        foreach (var line in lines)
        {
            var lineLength = line.Length + 1;

            if (charCount + lineLength <= charCap)
            {
                result.Add(line);
                charCount += lineLength;
                continue;
            }

            var remaining = charCap - charCount;
            if (remaining > 3)
            {
                result.Add(line[..Math.Min(remaining - 3, line.Length)]);
            }

            // Count remaining non-empty lines
            var remainingLines = 0;
            for (var i = lines.Length - 1; i >= result.Count; i--)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    remainingLines++;
            }

            result.Add($"// ... [{remainingLines} lines]");
            break;
        }

        return string.Join('\n', result);
    }
}
