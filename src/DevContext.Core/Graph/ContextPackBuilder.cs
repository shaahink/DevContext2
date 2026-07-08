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
            return new ContextPack(sb.ToString(), EstimateTokens(sb.ToString()), [], []) { Found = false };

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
        return new ContextPack(content, totalTokens, [.. sections], [.. omitted]) { Found = sections.Count > 0 };
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
    public static int EstimateTokens(string text) => (text.Length + 3) / 4;

    private static string TruncateToBudget(string text, int maxTokens)
    {
        var maxChars = maxTokens * 4;
        if (text.Length <= maxChars) return text;
        return text[..maxChars] + "\n... (truncated)";
    }

    // ── L4.4 multi-card assembly ──────────────────────────────────────────

    private static readonly Dictionary<string, IReadOnlyList<string>> CardTypeSections = new()
    {
        ["flow"]       = ["trace"],
        ["signatures"] = ["signatures"],
        ["bodies"]     = ["bodies"],
        ["di_wiring"]  = ["di_wiring"],
        ["config"]     = [],   // config is not traced — handled separately
        ["entities"]   = ["entities"],
        ["contracts"]  = ["signatures"],
        ["tests"]      = [],   // tests — handled separately
        ["identity"]   = ["identity"],
    };

    /// <summary>L4.4 — Build multi-card pack: trace each unique entry once, pick per-card
    /// sections by type, assemble the full markdown pack. Closes Meridian Trap A.</summary>
    public MultiContextPack BuildMulti(
        IReadOnlyList<ContextCardSpec> cards,
        int totalBudget = 8000,
        string? intent = null)
    {
        // Collect unique entry focuses
        var uniqueFocuses = new List<string>();
        var seen = new HashSet<string>();
        foreach (var card in cards)
        {
            foreach (var eid in card.EntryIds)
            {
                var focus = ResolveFocus(eid);
                if (focus is not null && seen.Add(focus))
                    uniqueFocuses.Add(focus);
            }
        }

        var perEntryBudget = uniqueFocuses.Count > 0
            ? totalBudget / uniqueFocuses.Count
            : totalBudget;

        // Trace each unique entry once, build ALL sections
        var entrySections = new Dictionary<string, ImmutableArray<SectionAllocation>>();
        foreach (var focus in uniqueFocuses)
        {
            var allSections = BuildSections(focus, perEntryBudget, intent);
            if (allSections.Length > 0)
                entrySections[focus] = allSections;
        }

        var sectionMap = new Dictionary<string, SectionAllocation>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in entrySections)
            foreach (var sa in kv.Value)
                if (!sectionMap.ContainsKey(sa.Section))
                    sectionMap[sa.Section] = sa;

        // Build per-card items
        var cardItems = ImmutableArray.CreateBuilder<ContextCardPack>();
        var allTokens = 0;

        foreach (var card in cards)
        {
            var wanted = CardTypeSections.GetValueOrDefault(card.Type, []);
            if (wanted.Count == 0) continue; // tests/config not traced

            var picked = ImmutableArray.CreateBuilder<SectionAllocation>();
            foreach (var sectionKey in wanted)
            {
                if (sectionMap.TryGetValue(sectionKey, out var sa))
                    picked.Add(sa);
            }

            var cardTokens = picked.Sum(s => s.Tokens);
            allTokens += cardTokens;

            cardItems.Add(new ContextCardPack(card.Type, card.Title, picked.ToImmutable(), cardTokens));
        }

        // Assemble full markdown pack
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# DevContext — Context Pack");
        sb.AppendLine();
        sb.AppendLine($"_Archetype: {_snapshot.Explanation}_");
        sb.AppendLine($"_Intent: {intent ?? "trace"} · Budget: {totalBudget} tokens_");
        sb.AppendLine();

        foreach (var cp in cardItems)
        {
            sb.AppendLine($"## {cp.Title}");
            sb.AppendLine($"_type: {cp.Type}, {cp.TotalTokens} tok_");
            sb.AppendLine();

            foreach (var sa in cp.Sections)
                sb.AppendLine(sa.Content);

            sb.AppendLine();
            sb.AppendLine($"<!-- context card: {cp.Type} -->");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine($"_Generated by DevContext Context Studio — {DateTime.UtcNow:O}_");

        var omitted = new List<string>();
        foreach (var card in cards)
        {
            if (CardTypeSections.GetValueOrDefault(card.Type, []).Count == 0)
                omitted.Add($"{card.Type}: client-only type, no server section");
        }

        return new MultiContextPack(
            cardItems.ToImmutable(),
            sb.ToString(),
            allTokens,
            allTokens,
            [.. omitted]);
    }

    /// <summary>Returns all sections for a single focus (no delimiting headers — raw content per section).</summary>
    internal ImmutableArray<SectionAllocation> BuildSections(string focus, int budgetTokens, string? intent)
    {
        var sections = new List<SectionAllocation>();
        var omitted = new List<string>();
        var mode = intent?.ToLowerInvariant() switch
        {
            "explain" => "explain",
            "review" => "review",
            _ => "trace",
        };

        // Build identity
        var identity = BuildIdentitySection();
        tokensAddSection(sections, omitted, budgetTokens, "identity", identity, ref budgetTokens);

        var trace = _query.Trace(focus, depth: mode == "review" ? 6 : 4);
        if (trace is null)
            return [];

        if (mode == "explain")
        {
            var regs = BuildDiRegistrations(trace);
            if (regs.Length > 0)
                tokensAddSection(sections, omitted, budgetTokens, "di_wiring", regs, ref budgetTokens);

            if (!trace.TouchedEntities.IsDefaultOrEmpty)
            {
                var entities = "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
                tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            }

            var sigs = BuildCalleeSignatures(trace);
            tokensAddSection(sections, omitted, budgetTokens, "signatures", sigs, ref budgetTokens);

            var bodies = BuildSalientBodies(trace);
            tokensAddSection(sections, omitted, budgetTokens, "bodies", bodies, ref budgetTokens);

            var traceText = BuildTraceSkeleton(trace);
            tokensAddSection(sections, omitted, budgetTokens, "trace", traceText, ref budgetTokens);
        }
        else if (mode == "review")
        {
            var traceText = BuildTraceSkeleton(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "trace", traceText, ref budgetTokens))
                return [.. sections];

            var sigs = BuildCalleeSignatures(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", sigs, ref budgetTokens))
                return [.. sections];

            var bodies = BuildSalientBodies(trace);
            tokensAddSection(sections, omitted, budgetTokens, "bodies", bodies, ref budgetTokens);

            var regs = BuildDiRegistrations(trace);
            if (regs.Length > 0)
                tokensAddSection(sections, omitted, budgetTokens, "di_wiring", regs, ref budgetTokens);

            if (!trace.TouchedEntities.IsDefaultOrEmpty)
            {
                var entities = "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
                tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            }
        }
        else
        {
            var traceText = BuildTraceSkeleton(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "trace", traceText, ref budgetTokens))
                return [.. sections];

            var sigs = BuildCalleeSignatures(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", sigs, ref budgetTokens))
                return [.. sections];

            var bodies = BuildSalientBodies(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "bodies", bodies, ref budgetTokens))
                return [.. sections];

            var regs = BuildDiRegistrations(trace);
            if (regs.Length > 0)
                tokensAddSection(sections, omitted, budgetTokens, "di_wiring", regs, ref budgetTokens);

            if (!trace.TouchedEntities.IsDefaultOrEmpty)
            {
                var entities = "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
                tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            }
        }

        return [.. sections];
    }

    private static bool tokensAddSection(
        List<SectionAllocation> sections, List<string> omitted,
        int totalBudget, string sectionName, string text, ref int remainingBudget)
    {
        var tokens = EstimateTokens(text);
        if (tokens > remainingBudget)
        {
            if (sections.Sum(s => s.Tokens) < totalBudget * 0.6 && remainingBudget > 100)
            {
                var trimmed = TruncateToBudget(text, remainingBudget - 50);
                var trimTokens = EstimateTokens(trimmed);
                sections.Add(new SectionAllocation(sectionName, trimTokens, trimmed));
                omitted.Add($"{sectionName}: trimmed from {tokens} to {trimTokens} tokens");
                remainingBudget -= trimTokens;
                return true;
            }
            omitted.Add($"{sectionName}: omitted ({tokens} tokens, budget exhausted)");
            return false;
        }
        sections.Add(new SectionAllocation(sectionName, tokens, text));
        remainingBudget -= tokens;
        return true;
    }

    private string BuildIdentitySection()
    {
        var map = _query.Map();
        var archetype = map?.Archetype.ToString().ToLowerInvariant() ?? "unknown";
        var sb = new System.Text.StringBuilder();
        sb.Append("# ").AppendLine(_snapshot.Explanation);
        sb.Append("Archetype: ").Append(archetype);
        sb.Append(" | ").Append(_snapshot.Entries.Length).Append(" entries");
        sb.Append(" | ").Append(_snapshot.Graph?.NodeCount ?? 0).AppendLine(" nodes");

        if (map?.ServiceStyles is { Length: > 0 })
        {
            sb.Append("Services: ");
            sb.AppendJoin(", ", map.ServiceStyles.Select(s => $"{s.ProjectName} ({s.Style})"));
            sb.AppendLine();
        }

        if (map?.PipelineBehaviors is { Length: > 0 })
        {
            sb.Append("Behaviors: ");
            sb.AppendJoin(", ", map.PipelineBehaviors);
            sb.AppendLine();
        }

        var crossServiceEdges = _query.Graph.AllEdges
            .Where(e => e.Kind == EdgeKind.ServiceLink).ToArray();
        if (crossServiceEdges.Length > 0)
        {
            sb.Append("Cross-service: ");
            sb.AppendJoin(" | ", crossServiceEdges.Take(6).Select(e =>
                $"{_query.Graph.Node(e.From)?.Title ?? e.From.Key} → {_query.Graph.Node(e.To)?.Title ?? e.To.Key} ({e.Tags.FirstOrDefault()})"));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string? ResolveFocus(string entryId)
    {
        foreach (var entry in _snapshot.Entries)
        {
            var nid = entry.Node.ToString();
            if (nid == entryId || entry.Title == entryId ||
                (entry.HttpMethod is { } hm && entry.Route is { } rt && $"{hm} {rt}" == entryId))
                return entry.HttpMethod is { } m && entry.Route is { } r
                    ? $"{m} {r}"
                    : entry.Title;
        }
        return null;
    }
}

public sealed record ContextPack(
    string Content,
    int TotalTokens,
    ImmutableArray<SectionAllocation> Sections,
    ImmutableArray<string> Omitted)
{
    public bool Found { get; init; } = true;
}

public sealed record SectionAllocation(string Section, int Tokens, string Content);

/// <summary>L4.4 — One card spec the UI sends to the server.</summary>
public sealed record ContextCardSpec(string Type, string Title, ImmutableArray<string> EntryIds);

/// <summary>L4.4 — One assembled card from the server.</summary>
public sealed record ContextCardPack(
    string Type,
    string Title,
    ImmutableArray<SectionAllocation> Sections,
    int TotalTokens);

/// <summary>L4.4 — Multi-card context pack assembled server-side.</summary>
public sealed record MultiContextPack(
    ImmutableArray<ContextCardPack> Cards,
    string AssembledMarkdown,
    int TotalTokens,
    int AllocatedTokens,
    ImmutableArray<string> Omitted);
