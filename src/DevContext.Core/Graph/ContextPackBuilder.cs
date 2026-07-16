using System.Collections.Immutable;
using System.Text;
using DevContext.Core.Insights;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Graph;

/// <summary>
/// L5.4 — Assembles a budget-priced context pack for an agent from a trace focus.
/// One implementation behind both consumers: MCP get_context (server GetContext RPC)
/// and the desktop Context Studio (GetContextPack RPC). Ranks content by graph
/// distance and centrality, cuts to budget with per-section attribution, and reports
/// what was omitted so the agent knows what it didn't get.
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

    /// <summary>T4.2 — signatures stay structural: spine-first (BFS) up to a token cap so a deep
    /// trace can't starve the bodies section (shamshir at depth 6 grew this to 2.5k of a 4k pack).
    /// The cut is named — the reader knows how many members the list left out.</summary>
    private string BuildCalleeSignatures(Trace trace, int tokenBudget)
    {
        var sb = new StringBuilder();
        var seen = new HashSet<NodeId>();
        var used = 0;
        var omittedCount = 0;
        foreach (var step in WalkStepsBreadthFirst(trace.Root))
        {
            if (!seen.Add(step.Node.Id)) continue;

            var entry = new StringBuilder();
            entry.AppendLine($"- `{step.Node.Kind}:{step.Node.Id.Key}` — {step.Node.Title}");
            if (step.Node.FilePath is { } fp)
                entry.AppendLine($"  Location: {Location(fp, step.Node.LineNumber)}");

            var tokens = EstimateTokens(entry.ToString());
            if (used + tokens > tokenBudget)
            {
                omittedCount++;
                continue;
            }
            sb.Append(entry);
            used += tokens;
        }
        if (omittedCount > 0)
            sb.AppendLine($"- … (+{omittedCount} more members — raise budgetTokens for the full list)");
        return sb.ToString();
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

    /// <summary>T4.1 — one location format everywhere: repo-relative `file:line`; the `:line`
    /// suffix only when a declaration line is known (never a trailing colon).</summary>
    private string Location(string filePath, int? line)
        => line is { } ln && ln > 0 ? $"{RelPath(filePath)}:{ln}" : RelPath(filePath);

    /// <summary>T4.2 — bodies are where the tokens go. Fills up to <paramref name="tokenBudget"/>
    /// spine-first (breadth-first, closest to the entry first): each step gets its FULL declaration
    /// text when that fits (capped per body so one god-class can't eat the pack), else its salient
    /// snippet with a visible `… (+N lines)` truncation marker, else it is counted omitted.</summary>
    private (string Text, int OmittedBodies) BuildBodiesToFill(Trace trace, int tokenBudget)
    {
        var sb = new StringBuilder();
        var seen = new HashSet<NodeId>();
        var remaining = tokenBudget;
        var perBodyCap = Math.Max(150, tokenBudget * 2 / 5);
        var omitted = 0;

        foreach (var step in WalkStepsBreadthFirst(trace.Root))
        {
            var node = step.Node;
            if (!seen.Add(node.Id)) continue;

            var full = FullBodyText(node);
            var salient = step.Salient;
            if (full is null && salient.IsDefaultOrEmpty) continue;

            var heading = new StringBuilder($"### {node.Title}");
            if (node.FilePath is { } fp)
                heading.Append($" — {Location(fp, node.LineNumber)}");

            if (full is not null)
            {
                var fullBlock = $"{heading}\n```csharp\n{full.TrimEnd()}\n```\n";
                var fullTokens = EstimateTokens(fullBlock);
                if (fullTokens <= Math.Min(remaining, perBodyCap))
                {
                    sb.Append(fullBlock);
                    remaining -= fullTokens;
                    continue;
                }
            }

            if (salient.Length > 0)
            {
                var moreLines = full is null ? 0 : Math.Max(0, CountBodyLines(full) - salient.Length);
                var marker = moreLines > 0 ? $"\n… (+{moreLines} lines)" : "";
                var snippetBlock = $"{heading}\n```csharp\n{string.Join("\n", salient)}{marker}\n```\n";
                var snippetTokens = EstimateTokens(snippetBlock);
                if (snippetTokens <= remaining)
                {
                    sb.Append(snippetBlock);
                    remaining -= snippetTokens;
                    continue;
                }
            }

            omitted++;
        }

        return (sb.ToString(), omitted);
    }

    /// <summary>The fullest body text available for a node: its own SourceBody, else its declaration
    /// text found in the parent type's body — the same lookup the trace's salient snippet uses.</summary>
    private string? FullBodyText(GraphNode node)
    {
        if (node.SourceBody is { Length: > 0 } own) return own;
        if (node.Kind != NodeKind.Member) return null;

        var key = node.Id.Key;
        var lastDot = key.LastIndexOf('.');
        if (lastDot <= 0) return null;
        var owner = _query.Graph.Node(new NodeId(NodeKind.Type, key[..lastDot]));
        if (owner?.SourceBody is not { Length: > 0 } typeBody) return null;
        return TraceBuilder.FindMemberDeclarationText(typeBody, key[(lastDot + 1)..]);
    }

    /// <summary>Body lines the reader would actually see — blank and lone-brace lines don't count,
    /// matching how the salient snippet counts its lines (so `+N lines` is honest).</summary>
    private static int CountBodyLines(string text)
    {
        var n = 0;
        foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line is "{" or "}") continue;
            n++;
        }
        return n;
    }

    private static IEnumerable<TraceStep> WalkStepsBreadthFirst(TraceStep root)
    {
        var queue = new Queue<TraceStep>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var step = queue.Dequeue();
            yield return step;
            foreach (var child in step.Children)
                queue.Enqueue(child);
        }
    }

    /// <summary>T4.6 — the contracts a change must honour: interfaces, DTOs, and message contracts
    /// (commands/queries/events) on the traced spine. Audit C2: the contracts card was a verbatim
    /// duplicate of signatures — this section selects only contract-shaped types, with the
    /// declaration line as the payload. Entities are excluded (they have their own section).</summary>
    private string BuildContracts(Trace trace)
    {
        var sb = new StringBuilder();
        var seen = new HashSet<NodeId>();
        foreach (var step in WalkSteps(trace.Root))
        {
            var node = step.Node;
            if (node.Kind != NodeKind.Type || !seen.Add(node.Id)) continue;
            if (ContractRole(node) is not { } role) continue;

            sb.Append($"- `{node.Title}` ({role})");
            if (node.FilePath is { } fp)
                sb.Append($" — {Location(fp, node.LineNumber)}");
            sb.AppendLine();
            if (DeclarationLine(node.SourceBody) is { } decl)
                sb.AppendLine($"  `{decl}`");
        }
        return sb.ToString();
    }

    private static string? ContractRole(GraphNode node)
    {
        if (node.Tags.Contains(RoleTags.Entity) || node.Tags.Contains(RoleTags.Aggregate)) return null;
        if (node.Tags.Contains(RoleTags.Command)) return "command";
        if (node.Tags.Contains(RoleTags.Query)) return "query";
        if (node.Tags.Contains(RoleTags.Notification)) return "notification";
        if (node.Tags.Contains(RoleTags.IntegrationEvent)) return "integration event";
        if (node.Tags.Contains(RoleTags.DomainEvent)) return "domain event";

        var decl = DeclarationLine(node.SourceBody);
        if (decl is null) return null;
        if (decl.Contains("interface ", StringComparison.Ordinal)) return "interface";
        if (decl.Contains("record ", StringComparison.Ordinal)
            || node.Title.EndsWith("Dto", StringComparison.Ordinal)
            || node.Title.EndsWith("Request", StringComparison.Ordinal)
            || node.Title.EndsWith("Response", StringComparison.Ordinal))
            return "dto";
        return null;
    }

    /// <summary>The first line of the type's own declaration (skipping attributes), cut at the
    /// opening brace — the one-line shape of the contract.</summary>
    private static string? DeclarationLine(string? sourceBody)
    {
        if (string.IsNullOrWhiteSpace(sourceBody)) return null;
        foreach (var raw in sourceBody.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('[') || line.StartsWith("//", StringComparison.Ordinal)) continue;
            var brace = line.IndexOf('{');
            if (brace >= 0) line = line[..brace].TrimEnd();
            return line.Length > 0 ? line : null;
        }
        return null;
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
        ["contracts"]  = ["contracts"],   // T4.6 — own section, no longer a signatures alias
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
        var omitted = new List<string>();

        foreach (var card in cards)
        {
            var wanted = CardTypeSections.GetValueOrDefault(card.Type, []);
            if (wanted.Count == 0)
            {
                omitted.Add($"{card.Type}: client-only type, no server section");
                continue;
            }

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
            // T4.6 — a card with no content is dropped from the pack and named in omitted[],
            // never rendered as an empty "0 tok" heading.
            if (picked.Length == 0)
            {
                omitted.Add($"{card.Type} ({card.Title}): no content for its entries — omitted");
                continue;
            }

            var cardTokens = picked.Sum(s => s.Tokens);
            allTokens += cardTokens;

            cardItems.Add(new ContextCardPack(card.Type, card.Title, picked, cardTokens));
        }

        // Assemble full markdown pack. T4.1: header names the repo + when/what was analyzed;
        // the archetype comes from the Map (snapshot.Explanation is never populated — audit C2's
        // `_Archetype: _`).
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {RepoName()} — Context Pack");
        sb.AppendLine();
        if (IdentityLine() is { Length: > 0 } identity)
            sb.AppendLine($"_{identity}_");
        var archetype = _query.Map()?.Archetype.ToString().ToLowerInvariant() ?? "unknown";
        sb.AppendLine($"_Archetype: {archetype}_");
        sb.AppendLine($"_Intent: {intent ?? "trace"} · Budget: {totalBudget} tokens_");
        sb.AppendLine();

        // T4.6 — no HTML comment markers in the human copy (card boundaries live in the
        // structured Cards[]; machine markers belong to the JSON export, T5.3).
        foreach (var cp in cardItems)
        {
            sb.AppendLine($"## {cp.Title}");
            sb.AppendLine($"_type: {cp.Type}, {cp.TotalTokens} tok_");
            sb.AppendLine();

            foreach (var sa in cp.Sections)
                sb.AppendLine(sa.Content);

            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine($"_Generated by DevContext Context Studio — {DateTime.UtcNow:O}_");

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
        var totalBudget = budgetTokens;
        // T4.2 — structural caps: the trace skeleton and the signature list are orientation,
        // not payload; capping them is what leaves the budget for bodies.
        var skeletonCap = Math.Max(300, totalBudget * 3 / 20);
        var signaturesCap = Math.Max(400, totalBudget / 4);
        var mode = intent?.ToLowerInvariant() switch
        {
            "explain" => "explain",
            "review" => "review",
            _ => "trace",
        };

        // Build identity
        var identity = BuildIdentitySection(focus);
        tokensAddSection(sections, omitted, budgetTokens, "identity", identity, ref budgetTokens);

        // T4.2 — the spine scales with the budget: a bigger budget buys a DEEPER walk (more
        // members to sign and embody), never more prose. Depth 4 starved the fill — dogfood's
        // checkout spine is 46 steps at depth 6 but ~12 at depth 4.
        var depth = mode == "review" || budgetTokens >= 3000 ? 6 : 4;
        var fanOut = budgetTokens >= 6000 ? 16 : 12;
        var trace = _query.Trace(focus, depth, fanOut);
        if (trace is null)
            return ([.. sections], [.. omitted]);

        // T4.6 — every section goes through tokensAddSection, which drops empty content and
        // records it in omitted[] (no more "Entities — 0 tok" shipping in a pack).
        // T4.2 — bodies always come LAST and fill whatever budget the structural sections left
        // (the audit's 612/4000 under-fill): sections stay structural, bodies are where tokens go.
        var entities = trace.TouchedEntities.IsDefaultOrEmpty
            ? ""
            : "## Touched entities\n" + string.Join("\n", trace.TouchedEntities.Select(e => $"- `{e}`")) + "\n";
        // Shape the skeleton only when it actually exceeds its cap — ShapeToBudget estimates
        // trace cost WITH salient text, so shaping an already-fitting skeleton over-cuts it.
        var skeletonFull = BuildTraceSkeleton(trace);
        var skeleton = EstimateTokens(skeletonFull) <= skeletonCap
            ? skeletonFull
            : BuildTraceSkeleton(TraceBuilder.ShapeToBudget(trace, skeletonCap));
        var signatures = BuildCalleeSignatures(trace, signaturesCap);

        if (mode == "explain")
        {
            tokensAddSection(sections, omitted, budgetTokens, "di_wiring", BuildDiRegistrations(trace), ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "signatures", signatures, ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "contracts", BuildContracts(trace), ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "trace", skeleton, ref budgetTokens);
        }
        else if (mode == "review")
        {
            if (!tokensAddSection(sections, omitted, budgetTokens, "trace", skeleton, ref budgetTokens))
                return ([.. sections], [.. omitted]);

            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", signatures, ref budgetTokens))
                return ([.. sections], [.. omitted]);

            tokensAddSection(sections, omitted, budgetTokens, "contracts", BuildContracts(trace), ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "di_wiring", BuildDiRegistrations(trace), ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
        }
        else
        {
            if (!tokensAddSection(sections, omitted, budgetTokens, "trace", skeleton, ref budgetTokens))
                return ([.. sections], [.. omitted]);

            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", signatures, ref budgetTokens))
                return ([.. sections], [.. omitted]);

            tokensAddSection(sections, omitted, budgetTokens, "contracts", BuildContracts(trace), ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "di_wiring", BuildDiRegistrations(trace), ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
        }

        var (bodies, omittedBodies) = BuildBodiesToFill(trace, budgetTokens);
        tokensAddSection(sections, omitted, budgetTokens, "bodies", bodies, ref budgetTokens);
        if (omittedBodies > 0)
            omitted.Add($"bodies: {omittedBodies} member bodies omitted (budget) — raise budgetTokens or read_source the member");

        return ([.. sections], [.. omitted]);
    }

    private static bool tokensAddSection(
        List<SectionAllocation> sections, List<string> omitted,
        int totalBudget, string sectionName, string text, ref int remainingBudget)
    {
        // T4.6 — an empty section never ships (audit C2's "Entities — 0 tok"); it is recorded
        // in omitted[] so the reader knows the pack looked and found nothing.
        if (string.IsNullOrWhiteSpace(text))
        {
            omitted.Add($"{sectionName}: empty — omitted");
            return true;
        }

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

    /// <summary>T4.1 — repo display name: the solution name when there is one (an analysis root
    /// like `…/repo/src` would otherwise title the pack "src"), else the root folder. The audit's
    /// `# ` empty title came from snapshot.Explanation, which the pipeline never populates —
    /// don't read it here.</summary>
    private string RepoName()
    {
        if (_snapshot.Model.Solution?.Name is { Length: > 0 } solution) return solution;
        var root = (_snapshot.RootPath ?? "").Replace('\\', '/').TrimEnd('/');
        var slash = root.LastIndexOf('/');
        var name = slash >= 0 ? root[(slash + 1)..] : root;
        return name.Length > 0 ? name : "repository";
    }

    /// <summary>T4.1 — the pack identity line: when the analysis ran + which commit it saw.
    /// Only claims what the snapshot actually knows (no HEAD chunk outside a git checkout).</summary>
    private string IdentityLine()
    {
        var parts = new List<string>();
        if (_snapshot.AnalyzedAtUtc is { } at)
            parts.Add($"analyzed {at.UtcDateTime.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture)} UTC");
        if (_snapshot.GitHead is { Length: >= 7 } head)
            parts.Add($"HEAD {head[..7]}");
        return string.Join(" · ", parts);
    }

    private string BuildIdentitySection(string focus)
    {
        var map = _query.Map();
        var archetype = map?.Archetype.ToString().ToLowerInvariant() ?? "unknown";
        var sb = new System.Text.StringBuilder();
        sb.Append("# ").Append(RepoName()).Append(" — ").AppendLine(focus);
        if (IdentityLine() is { Length: > 0 } identity)
            sb.AppendLine(identity);
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
