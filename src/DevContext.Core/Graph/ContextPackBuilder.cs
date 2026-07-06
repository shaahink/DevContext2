using System.Collections.Immutable;
using System.Text;
using DevContext.Core.Insights;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Graph;

/// <summary>
/// L5.4 — Assembles a budget-priced context pack for an agent from a trace focus.
/// One implementation, used by MCP (get_context), CLI (devcontext context), and
/// desktop export drawer (From Trail preset). Ranks content by graph distance and
/// centrality, cuts to budget with per-section attribution, and reports what was
/// omitted so the agent knows what it didn't get.
/// </summary>
public sealed class ContextPackBuilder
{
    private readonly GraphQuery _query;
    private readonly AnalysisSnapshot _snapshot;

    public ContextPackBuilder(GraphQuery query, AnalysisSnapshot snapshot)
    {
        _query = query;
        _snapshot = snapshot;
    }

    public ContextPack Build(string focus, int budgetTokens = 8000, string? intent = null)
    {
        var sb = new StringBuilder();
        var sections = new List<SectionAllocation>();
        var omitted = new List<string>();
        var mode = intent?.ToLowerInvariant() switch
        {
            "explain" => "explain",
            "review" => "review",
            _ => "trace",
        };

        // M4.8 — richer identity with service info + conventions
        var map = _query.Map();
        var archetype = map?.Archetype.ToString().ToLowerInvariant() ?? "unknown";
        var sbId = new StringBuilder();
        sbId.Append("# ").AppendLine(_snapshot.Explanation);
        sbId.Append("Archetype: ").Append(archetype);
        sbId.Append(" | ").Append(_snapshot.Entries.Length).Append(" entries");
        sbId.Append(" | ").Append(_snapshot.Graph?.NodeCount ?? 0).AppendLine(" nodes");

        if (map?.ServiceStyles is { Length: > 0 })
        {
            sbId.Append("Services: ");
            sbId.AppendJoin(", ", map.ServiceStyles.Select(s => $"{s.ProjectName} ({s.Style})"));
            sbId.AppendLine();
        }

        if (map?.PipelineBehaviors is { Length: > 0 })
        {
            sbId.Append("Behaviors: ");
            sbId.AppendJoin(", ", map.PipelineBehaviors);
            sbId.AppendLine();
        }

        // M4.8 — cross-service ServiceLinks from graph edges
        var crossServiceEdges = _query.Graph.AllEdges
            .Where(e => e.Kind == EdgeKind.ServiceLink).ToArray();
        if (crossServiceEdges.Length > 0)
        {
            sbId.Append("Cross-service: ");
            sbId.AppendJoin(" | ", crossServiceEdges.Take(6).Select(e =>
                $"{_query.Graph.Node(e.From)?.Title ?? e.From.Key} → {_query.Graph.Node(e.To)?.Title ?? e.To.Key} ({e.Tags.FirstOrDefault()})"));
            sbId.AppendLine();
        }

        var identity = sbId.ToString();
        AppendSection(sb, sections, omitted, budgetTokens, "identity", identity);

        var trace = _query.Trace(focus, depth: mode == "review" ? 6 : 4);
        if (trace is null)
            return Finalize(sb, sections, omitted);

        // ── Explain mode: prioritize concepts over code ──
        if (mode == "explain")
        {
            // DI wiring first (architecture understanding)
            var regs = BuildDiRegistrations(trace);
            if (regs.Length > 0)
                AppendSection(sb, sections, omitted, budgetTokens, "di_wiring", regs);

            // Entities
            if (!trace.TouchedEntities.IsDefaultOrEmpty)
            {
                var entities = "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
                AppendSection(sb, sections, omitted, budgetTokens, "entities", entities);
            }

            // Signatures (conceptual, not full bodies)
            var sigs = BuildCalleeSignatures(trace);
            AppendSection(sb, sections, omitted, budgetTokens, "signatures", sigs);

            // Bodies trimmed — conceptual docs only, skip if tight
            var bodies = BuildSalientBodies(trace);
            AppendSection(sb, sections, omitted, budgetTokens, "bodies", bodies);

            // Trace skeleton last (least important for explain)
            var traceText = BuildTraceSkeleton(trace);
            AppendSection(sb, sections, omitted, budgetTokens, "trace", traceText);
        }
        // ── Review mode: prioritize code depth ──
        else if (mode == "review")
        {
            // Trace skeleton
            var traceText = BuildTraceSkeleton(trace);
            if (!AppendSection(sb, sections, omitted, budgetTokens, "trace", traceText))
                return Finalize(sb, sections, omitted);

            // Signatures
            var sigs = BuildCalleeSignatures(trace);
            if (!AppendSection(sb, sections, omitted, budgetTokens, "signatures", sigs))
                return Finalize(sb, sections, omitted);

            // Full bodies (primary value for review)
            var bodies = BuildSalientBodies(trace);
            AppendSection(sb, sections, omitted, budgetTokens, "bodies", bodies);

            // DI wiring (helpful for review)
            var regs = BuildDiRegistrations(trace);
            if (regs.Length > 0)
                AppendSection(sb, sections, omitted, budgetTokens, "di_wiring", regs);

            // Entities (secondary)
            if (!trace.TouchedEntities.IsDefaultOrEmpty)
            {
                var entities = "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
                AppendSection(sb, sections, omitted, budgetTokens, "entities", entities);
            }
        }
        // ── Trace mode (default): balanced ──
        else
        {
            // Trace skeleton
            var traceText = BuildTraceSkeleton(trace);
            if (!AppendSection(sb, sections, omitted, budgetTokens, "trace", traceText))
                return Finalize(sb, sections, omitted);

            // Signatures
            var sigs = BuildCalleeSignatures(trace);
            if (!AppendSection(sb, sections, omitted, budgetTokens, "signatures", sigs))
                return Finalize(sb, sections, omitted);

            // Bodies
            var bodies = BuildSalientBodies(trace);
            if (!AppendSection(sb, sections, omitted, budgetTokens, "bodies", bodies))
                return Finalize(sb, sections, omitted);

            // DI wiring
            var regs = BuildDiRegistrations(trace);
            if (regs.Length > 0)
                AppendSection(sb, sections, omitted, budgetTokens, "di_wiring", regs);

            // Entities
            if (!trace.TouchedEntities.IsDefaultOrEmpty)
            {
                var entities = "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
                AppendSection(sb, sections, omitted, budgetTokens, "entities", entities);
            }
        }

        return Finalize(sb, sections, omitted);
    }

    private static bool AppendSection(StringBuilder sb, List<SectionAllocation> sections, List<string> omitted,
        int budget, string sectionName, string text)
    {
        var tokens = EstimateTokens(text);
        var currentTotal = sections.Sum(s => s.Tokens);
        if (currentTotal + tokens > budget)
        {
            if (currentTotal < budget * 0.6)
            {
                // Trim: include what fits
                var available = budget - currentTotal - 200; // reserve for headers
                if (available > 100)
                {
                    var trimmed = TruncateToBudget(text, available);
                    var trimTokens = EstimateTokens(trimmed);
                    sb.AppendLine($"## {sectionName} (trimmed)");
                    sb.AppendLine(trimmed);
                    sections.Add(new SectionAllocation(sectionName, trimTokens, trimmed));
                    omitted.Add($"{sectionName}: trimmed from {tokens} to {trimTokens} tokens");
                }
                else
                {
                    omitted.Add($"{sectionName}: omitted ({tokens} tokens, budget exhausted)");
                }
            }
            else
            {
                omitted.Add($"{sectionName}: omitted ({tokens} tokens, budget low)");
            }
            return false;
        }

        sb.AppendLine($"## {sectionName}");
        sb.AppendLine(text);
        sections.Add(new SectionAllocation(sectionName, tokens, text));
        return true;
    }

    private static ContextPack Finalize(StringBuilder sb, List<SectionAllocation> sections, List<string> omitted)
    {
        var totalTokens = sections.Sum(s => s.Tokens);
        var content = sb.ToString();
        return new ContextPack(content, totalTokens, [.. sections], [.. omitted]);
    }

    private static string BuildTraceSkeleton(Trace trace)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Focus: {trace.Entry.Title} ({trace.Entry.Kind})");
        BuildTraceSkeletonRecursive(sb, trace.Root, 0);
        return sb.ToString();
    }

    private static void BuildTraceSkeletonRecursive(StringBuilder sb, TraceStep step, int indent)
    {
        var prefix = new string(' ', indent * 2);
        sb.Append($"{prefix}- [{step.Seam}] {step.Node.Title}");
        if (step.Resolution == Resolution.Syntactic)
            sb.Append(" [approx]");
        if (step.Truncated)
            sb.Append($" (truncated, {step.Omitted} omitted)");
        sb.AppendLine();
        foreach (var child in step.Children)
            BuildTraceSkeletonRecursive(sb, child, indent + 1);
    }

    private static string BuildCalleeSignatures(Trace trace)
    {
        var sb = new StringBuilder();
        var seen = new HashSet<NodeId>();
        CollectSignatures(trace.Root, sb, seen);
        return sb.ToString();
    }

    private static void CollectSignatures(TraceStep step, StringBuilder sb, HashSet<NodeId> seen)
    {
        if (seen.Add(step.Node.Id))
        {
            sb.AppendLine($"- `{step.Node.Kind}:{step.Node.Id.Key}` — {step.Node.Title}");
            if (step.Node.FilePath is { } fp)
                sb.AppendLine($"  Location: {fp}:{step.Node.LineNumber}");
        }
        foreach (var child in step.Children)
            CollectSignatures(child, sb, seen);
    }

    private static string BuildSalientBodies(Trace trace)
    {
        var sb = new StringBuilder();
        foreach (var step in WalkSteps(trace.Root))
        {
            if (step.Salient.Length > 0)
            {
                sb.AppendLine($"### {step.Node.Title}");
                sb.AppendLine("```csharp");
                foreach (var line in step.Salient)
                    sb.AppendLine(line);
                sb.AppendLine("```");
            }
        }
        return sb.ToString();
    }

    private string BuildDiRegistrations(Trace? trace)
    {
        var types = new HashSet<NodeId>();
        if (trace is not null)
            foreach (var step in WalkSteps(trace.Root))
            {
                if (step.Node.Kind == NodeKind.Type)
                    types.Add(step.Node.Id);
                else if (step.Node.Kind == NodeKind.Member)
                {
                    var lastDot = step.Node.Id.Key.LastIndexOf('.');
                    if (lastDot > 0)
                    {
                        var typeKey = step.Node.Id.Key[..lastDot];
                        types.Add(new NodeId(NodeKind.Type, typeKey));
                    }
                }
            }

        if (types.Count == 0) return "";

        var sb = new StringBuilder();
        sb.AppendLine("## DI Registrations");
        var found = false;
        foreach (var nodeId in types)
        {
            var resolvers = _query.Neighbors(nodeId, EdgeDirection.In, EdgeKind.Resolves);
            if (resolvers.Length == 0) continue;
            found = true;
            foreach (var r in resolvers)
                sb.AppendLine($"- `{r.OtherTitle}` → {nodeId.Key} ({r.Resolution})");
        }
        return found ? sb.ToString() : "";
    }

    private static IEnumerable<TraceStep> WalkSteps(TraceStep root)
    {
        yield return root;
        foreach (var child in root.Children)
        foreach (var s in WalkSteps(child))
            yield return s;
    }

    /// <summary>Token estimator: ~1 token per 4 characters (GPT-style).</summary>
    private static int EstimateTokens(string text) => (text.Length + 3) / 4;

    private static string TruncateToBudget(string text, int maxTokens)
    {
        var maxChars = maxTokens * 4;
        if (text.Length <= maxChars) return text;
        return text[..maxChars] + "\n... (truncated)";
    }
}

public sealed record ContextPack(
    string Content,
    int TotalTokens,
    ImmutableArray<SectionAllocation> Sections,
    ImmutableArray<string> Omitted);

public sealed record SectionAllocation(string Section, int Tokens, string Content);
