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
        var (sections, omitted) = BuildSections(focus, budgetTokens, intent);
        if (sections.Length == 0)
        {
            var empty = new ContextPack("", 0, [], [.. omitted]) { Found = false };
            return empty;
        }

        var sb = new StringBuilder();
        foreach (var sa in sections)
        {
            sb.AppendLine($"## {sa.Section}");
            sb.AppendLine(sa.Content);
        }

        var totalTokens = sections.Sum(s => s.Tokens);
        return new ContextPack(sb.ToString(), totalTokens, sections, [.. omitted]) { Found = true };
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

    private string BuildCalleeSignatures(Trace trace)
    {
        var sb = new StringBuilder();
        var seen = new HashSet<NodeId>();
        CollectSignatures(trace.Root, sb, seen);
        return sb.ToString();
    }

    private void CollectSignatures(TraceStep step, StringBuilder sb, HashSet<NodeId> seen)
    {
        if (seen.Add(step.Node.Id))
        {
            sb.AppendLine($"- `{step.Node.Kind}:{step.Node.Id.Key}` — {step.Node.Title}");
            if (step.Node.FilePath is { } fp)
                sb.AppendLine($"  Location: {RelPath(fp)}:{step.Node.LineNumber}");
        }
        foreach (var child in step.Children)
            CollectSignatures(child, sb, seen);
    }

    /// <summary>T3.5 — pack locations are repo-relative, never absolute machine paths (they waste
    /// tokens and leak layout). Falls back to the raw path when it isn't under the analysis root.</summary>
    private string RelPath(string filePath)
    {
        var root = _snapshot.RootPath;
        if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(filePath)) return filePath;
        var abs = filePath.Replace('\\', '/');
        var rooted = root.Replace('\\', '/').TrimEnd('/') + "/";
        return abs.StartsWith(rooted, StringComparison.OrdinalIgnoreCase) ? abs[rooted.Length..] : abs;
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
    /// sections by type, assemble the full markdown pack. Closes Meridian Trap A.
    /// Each card aggregates sections from ALL its referenced entries (not just the first
    /// one traced), so a card with entries [A, B] gets trace content from both.</summary>
    public MultiContextPack BuildMulti(
        IReadOnlyList<ContextCardSpec> cards,
        int totalBudget = 8000,
        string? intent = null)
    {
        // Collect unique entry focuses with their reach counts for proportional budget
        var uniqueFocuses = new List<(string Focus, int Reach)>();
        var seen = new HashSet<string>();
        foreach (var card in cards)
        {
            foreach (var eid in card.EntryIds)
            {
                var (focus, reach) = ResolveFocusWithReach(eid);
                if (focus is not null && seen.Add(focus))
                    uniqueFocuses.Add((focus, reach));
            }
        }

        // L4.5 — proportional budget: each entry gets budget weighted by its reach count
        // (complexity proxy). Minimum floor of 200 tokens per entry so tiny entries still
        // get meaningful sections.
        const int minEntryBudget = 200;
        var focusBudgets = AllocateProportionalBudgets(uniqueFocuses, totalBudget, minEntryBudget);
        // totalBudget updated to reflect what proportionally-allocated budgets sum to

        // Trace each unique entry once, build ALL sections.
        // Sections are stored per-focus so each card can aggregate from its own entries.
        var entrySections = new Dictionary<string, ImmutableArray<SectionAllocation>>();
        foreach (var (focus, _) in uniqueFocuses)
        {
            var budget = focusBudgets.GetValueOrDefault(focus, minEntryBudget);
            var (allSections, _) = BuildSections(focus, budget, intent);
            if (allSections.Length > 0)
                entrySections[focus] = allSections;
        }

        // Build per-card items — each card aggregates sections from ALL its entries
        var cardItems = ImmutableArray.CreateBuilder<ContextCardPack>();
        var allTokens = 0;

        foreach (var card in cards)
        {
            var wanted = CardTypeSections.GetValueOrDefault(card.Type, []);
            if (wanted.Count == 0) continue; // tests/config not traced

            var pickedBySection = new Dictionary<string, SectionAllocation>(StringComparer.OrdinalIgnoreCase);
            foreach (var eid in card.EntryIds)
            {
                var focus = ResolveFocus(eid);
                if (focus is null || !entrySections.TryGetValue(focus, out var es)) continue;
                foreach (var sa in es)
                {
                    if (!wanted.Contains(sa.Section)) continue;
                    if (pickedBySection.TryGetValue(sa.Section, out var existing))
                    {
                        // Same section from another entry — concatenate content
                        pickedBySection[sa.Section] = new SectionAllocation(
                            sa.Section,
                            existing.Tokens + sa.Tokens,
                            existing.Content + "\n" + sa.Content);
                    }
                    else
                    {
                        pickedBySection[sa.Section] = sa;
                    }
                }
            }

            var picked = pickedBySection.Values.OrderBy(x => x.Section).ToImmutableArray();
            var cardTokens = picked.Sum(s => s.Tokens);
            allTokens += cardTokens;

            cardItems.Add(new ContextCardPack(card.Type, card.Title, picked, cardTokens));
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
            totalBudget,    // AllocatedTokens = the budget ceiling, not actual usage
            [.. omitted]);
    }

    /// <summary>Returns all sections for a single focus + omitted reasons (no delimiting headers — raw content per section).</summary>
    internal (ImmutableArray<SectionAllocation> Sections, ImmutableArray<string> Omitted) BuildSections(string focus, int budgetTokens, string? intent)
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
            return ([.. sections], [.. omitted]);

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
                return ([.. sections], [.. omitted]);

            var sigs = BuildCalleeSignatures(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", sigs, ref budgetTokens))
                return ([.. sections], [.. omitted]);

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
                return ([.. sections], [.. omitted]);

            var sigs = BuildCalleeSignatures(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", sigs, ref budgetTokens))
                return ([.. sections], [.. omitted]);

            var bodies = BuildSalientBodies(trace);
            if (!tokensAddSection(sections, omitted, budgetTokens, "bodies", bodies, ref budgetTokens))
                return ([.. sections], [.. omitted]);

            var regs = BuildDiRegistrations(trace);
            if (regs.Length > 0)
                tokensAddSection(sections, omitted, budgetTokens, "di_wiring", regs, ref budgetTokens);

            if (!trace.TouchedEntities.IsDefaultOrEmpty)
            {
                var entities = "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
                tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            }
        }

        return ([.. sections], [.. omitted]);
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

    private string? ResolveFocus(string entryId) => FindEntry(entryId) is { } e
        ? (e.HttpMethod is { } m && e.Route is { } r ? $"{m} {r}" : e.Title)
        : null;

    private (string? Focus, int Reach) ResolveFocusWithReach(string entryId)
    {
        if (FindEntry(entryId) is not { } e) return (null, 0);
        var focus = e.HttpMethod is { } m && e.Route is { } r ? $"{m} {r}" : e.Title;
        return (focus, e.Reach);
    }

    // GAP 4 (UI Context Studio audit) — same bare-route gap as EntryPointResolver: a card seeded with
    // a bare route id ("/products") must resolve, not just "GET /products" or the raw nodeId.
    private EntryPoint? FindEntry(string entryId)
    {
        foreach (var entry in _snapshot.Entries)
        {
            var nid = entry.Node.ToString();
            if (nid == entryId || entry.Title == entryId ||
                (entry.HttpMethod is { } hm && entry.Route is { } rt && $"{hm} {rt}" == entryId))
                return entry;
        }

        if (entryId.StartsWith('/'))
        {
            var routeMatches = _snapshot.Entries.Where(e =>
                e.Kind == EntryPointKind.HttpEndpoint && e.Route is { } r &&
                string.Equals(GraphBuilder.NormalizeRoute(r), GraphBuilder.NormalizeRoute(entryId), StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (routeMatches.Count == 1) return routeMatches[0];
            if (routeMatches.Count > 1)
                return routeMatches.FirstOrDefault(e =>
                    string.Equals(e.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    ?? routeMatches[0];
        }

        return null;
    }

    private static Dictionary<string, int> AllocateProportionalBudgets(
        List<(string Focus, int Reach)> focuses, int totalBudget, int minPerEntry)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        if (focuses.Count == 0) return result;

        var sumReach = focuses.Sum(f => Math.Max(f.Reach, 1)); // at least 1 to avoid div-by-zero
        if (sumReach <= 0) sumReach = focuses.Count;

        var remaining = totalBudget;
        var allocated = 0;
        for (var i = 0; i < focuses.Count; i++)
        {
            var (focus, reach) = focuses[i];
            var isLast = i == focuses.Count - 1;
            var share = isLast
                ? remaining
                : Math.Max(minPerEntry, (int)((double)Math.Max(reach, 1) / sumReach * totalBudget));
            result[focus] = share;
            remaining -= share;
            allocated += share;
        }

        return result;
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
