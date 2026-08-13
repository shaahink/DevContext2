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
        // T5.2 — unified addressing (T3.1 rule): a nodeId or bare route resolves like it does
        // in BuildMulti before tracing. Without this, VerifyContext(focus=nodeId) traced null
        // and returned identity-only sections — 0 files checked, always "fresh" while the disk
        // drifted underneath.
        focus = ResolveFocus(focus) ?? focus;
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
    private string BuildCalleeSignatures(Trace trace, int tokenBudget, SectionProvenance prov)
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
            prov.Tally(step.Resolution);
            if (step.Node.FilePath is { } pf)
                prov.Locations.Add(Location(pf, step.Node.LineNumber));
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
    private (string Text, int OmittedBodies) BuildBodiesToFill(Trace trace, int tokenBudget, SectionProvenance prov)
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
                    TallyBody(prov, step);
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
                    TallyBody(prov, step);
                    continue;
                }
            }

            omitted++;
        }

        return (sb.ToString(), omitted);

        void TallyBody(SectionProvenance p, TraceStep step)
        {
            p.Tally(step.Resolution);
            if (step.Node.FilePath is { } fp)
                p.Locations.Add(Location(fp, step.Node.LineNumber));
        }
    }

    /// <summary>The fullest body text available for a node: its own SourceBody, else its declaration
    /// text found in the parent type's body — the same lookup the trace's salient snippet uses.</summary>
    private string? FullBodyText(GraphNode node)
    {
        if (node.SourceBody is { Length: > 0 } own) return own;
        if (node.Kind != NodeKind.Member) return null;

        var key = node.Id.Key;
        if (!key.Contains("::", StringComparison.Ordinal)) return null;
        var owner = _query.Graph.Node(new NodeId(NodeKind.Type, Graph2.SymbolCanon.OwnerTypeOf(key)));
        if (owner?.SourceBody is not { Length: > 0 } typeBody) return null;
        return TraceBuilder.FindMemberDeclarationText(typeBody, Graph2.SymbolCanon.MemberNameOf(key));
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
    private string BuildContracts(Trace trace, SectionProvenance prov)
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
            {
                sb.Append($" — {Location(fp, node.LineNumber)}");
                prov.Locations.Add(Location(fp, node.LineNumber));
            }
            sb.AppendLine();
            if (DeclarationLine(node.SourceBody) is { } decl)
                sb.AppendLine($"  `{decl}`");
            prov.Tally(Resolution.Semantic); // contract shapes come from the node's own declaration
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

    /// <summary>T4.3 (R9) — the config keys the traced spine actually reads, each with the
    /// file:line of the binding site. Scans only the spine's own files (a handful), so this is
    /// cheap at pack time; the session-wide scan cache (T3.4) is a server concern, not ours.</summary>
    private string BuildConfigSection(Trace trace, SectionProvenance prov)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var step in WalkSteps(trace.Root))
            if (step.Node.FilePath is { } fp)
                files.Add(fp);
        if (files.Count == 0) return "";

        var bindings = ConfigScanner.Scan(_query.Graph, files);
        if (bindings.Count == 0) return "";

        var sb = new StringBuilder();
        foreach (var group in bindings
            .GroupBy(b => b.Key, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var first = group.OrderBy(b => b.FilePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(b => b.LineNumber).First();
            sb.Append($"- `{group.Key}` ({first.PatternType}) — {Location(first.FilePath, first.LineNumber)}");
            var more = group.Count() - 1;
            if (more > 0) sb.Append($" (+{more} more sites)");
            sb.AppendLine();
            prov.Tally(Resolution.Semantic); // a literal key in the syntax tree is a certain binding
            prov.Locations.Add(Location(first.FilePath, first.LineNumber));
        }
        return sb.ToString();
    }

    /// <summary>T4.3 (R9) — tests whose call closure reaches a spine member, by the same
    /// heuristic the tests_for tool uses (one source: <see cref="TestHeuristics"/>). Best-effort:
    /// an empty section means no test REACHED the spine by these signals, never "untested".</summary>
    private string BuildTestsSection(Trace trace, SectionProvenance prov)
    {
        const int maxProbes = 10;
        const int maxRows = 12;
        var sb = new StringBuilder();
        var seenTests = new HashSet<NodeId>();
        var probed = 0;
        var capped = false;

        foreach (var step in WalkStepsBreadthFirst(trace.Root))
        {
            if (capped) break;
            var node = step.Node;
            if (node.Kind is not (NodeKind.Member or NodeKind.Type)) continue;
            if (++probed > maxProbes) break;

            foreach (var (callerId, title, filePath, lineNumber, project, distance) in _query.FindCallers(node.Id, maxDepth: 6))
            {
                if (!TestHeuristics.IsLikelyTestMethod(title, filePath, project, _snapshot.RootPath)) continue;
                if (!seenTests.Add(callerId)) continue;

                sb.Append($"- `{title}` — reaches `{node.Title}` (distance {distance})");
                if (filePath is not null)
                {
                    sb.Append($" — {Location(filePath, lineNumber)}");
                    prov.Locations.Add(Location(filePath, lineNumber));
                }
                sb.AppendLine();
                prov.Tally(Resolution.Syntactic); // name/path heuristic — approximate by design

                if (seenTests.Count >= maxRows)
                {
                    sb.AppendLine($"- … (list capped at {maxRows} — tests_for(nodeId) for the rest)");
                    capped = true;
                    break;
                }
            }
        }

        if (sb.Length == 0) return "";
        return "_best-effort: name/path/project heuristic — no rows ≠ untested_\n" + sb;
    }

    /// <summary>G1.2 (R4 item 2) — the INBOUND half of a symbol-rooted pack: who calls, sends to,
    /// resolves or otherwise references this symbol, each with the call site's repo-relative file:line.
    /// <para>Every other section is built from a trace, and a trace walks OUT-edges — which is the
    /// right direction for an HTTP endpoint and the wrong one for a library symbol. MEASURED on
    /// FluentValidation before this section existed: <c>InlineValidator</c> filled 8% of a 4000-token
    /// budget with a one-line trace, while the graph knew who used it; <c>IValidator</c>, the library's
    /// central abstraction, has 9 in-edges and 0 out-edges, so its pack was structurally empty.</para>
    /// <para>In-edges roll up through <see cref="GraphQuery.Neighbors"/>, so a Type root answers with
    /// the members that carry the collaboration, not with the bare type.</para></summary>
    private string BuildUsageSection(NodeId rootId, SectionProvenance prov)
    {
        const int maxRows = 14;
        var usages = _query.FindUsages(rootId);
        if (usages.Length == 0) return "";

        var sb = new StringBuilder();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var shown = 0;
        foreach (var u in usages)
        {
            var callerKey = u.From.Key;
            if (!seen.Add(callerKey + "|" + u.Kind)) continue;
            if (shown >= maxRows) continue;

            sb.Append($"- `{u.OtherTitle}` {UsageVerb(u.Kind)} — `{callerKey}`");
            // The edge's provenance is the CALL SITE. When an edge carries none, the caller's own
            // declaration line is the honest second-best — and it is labelled differently, because
            // "where the caller is declared" is not "where it calls me".
            if (u.Provenance is { Length: > 0 } site)
            {
                var rel = RelPath(site);
                sb.Append($" at {rel}");
                prov.Locations.Add(rel);
            }
            else if (_query.Graph.Node(u.From) is { FilePath: { Length: > 0 } fp } caller)
            {
                var rel = Location(fp, caller.LineNumber);
                sb.Append($" declared in {rel}");
                prov.Locations.Add(rel);
            }
            if (u.Resolution == Resolution.Syntactic) sb.Append(" [approx]");
            sb.AppendLine();
            prov.Tally(u.Resolution);
            shown++;
        }

        var total = seen.Count;
        if (total > shown)
            sb.AppendLine($"- … (+{total - shown} more references — usages(nodeId) for the full list)");
        return sb.ToString();
    }

    /// <summary>The edge kind read from the CALLER's side ("X calls me", "X sends me").</summary>
    private static string UsageVerb(EdgeKind kind) => kind switch
    {
        EdgeKind.Calls => "calls it",
        EdgeKind.Sends => "sends it",
        EdgeKind.Handles => "is handled by it",
        EdgeKind.Raises => "raises it",
        EdgeKind.Consumes => "consumes it",
        EdgeKind.ReadsWrites => "reads/writes it",
        EdgeKind.Resolves => "resolves to it",
        EdgeKind.WrappedBy => "is wrapped by it",
        EdgeKind.EntityRelation => "relates to it",
        EdgeKind.ServiceLink => "links to it",
        EdgeKind.Exposes => "exposes it",
        EdgeKind.DependsOn => "depends on it",
        _ => "references it",
    };

    private string BuildDiRegistrations(Trace? trace, SectionProvenance prov)
    {
        var types = new HashSet<NodeId>();
        if (trace is not null)
            foreach (var step in WalkSteps(trace.Root))
            {
                if (step.Node.Kind == NodeKind.Type)
                    types.Add(step.Node.Id);
                else if (step.Node.Kind == NodeKind.Member
                    && step.Node.Id.Key.Contains("::", StringComparison.Ordinal))
                {
                    types.Add(new NodeId(NodeKind.Type, Graph2.SymbolCanon.OwnerTypeOf(step.Node.Id.Key)));
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
            {
                sb.AppendLine($"- `{r.OtherTitle}` → {nodeId.Key} ({r.Resolution})");
                prov.Tally(r.Resolution);
            }
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
        ["config"]     = ["config"],      // T4.3 (R9) — real: spine config keys, no longer a client stub
        ["entities"]   = ["entities"],
        ["contracts"]  = ["contracts"],   // T4.6 — own section, no longer a signatures alias
        ["tests"]      = ["tests"],       // T4.3 (R9) — real: tests reaching the spine
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

        // Trace each unique entry once, build ALL sections.
        // Sections are stored per-focus so each card can aggregate from its own entries.
        var entrySections = new Dictionary<string, ImmutableArray<SectionAllocation>>();
        var sectionOmissions = new List<string>();
        // N0.1 (audit §3.F.4) — what "allocated" actually means: the share of the ceiling that
        // reached an entry which produced sections. Previously AllocatedTokens echoed the budget
        // ceiling verbatim, so the Studio header printed one number under two labels (and claimed
        // a full allocation even for a pack where nothing resolved).
        var allocatedTokens = 0;
        foreach (var (focus, _) in uniqueFocuses)
        {
            var budget = focusBudgets.GetValueOrDefault(focus, minEntryBudget);
            var (allSections, focusOmitted) = BuildSections(focus, budget, intent);
            if (allSections.Length > 0)
            {
                entrySections[focus] = allSections;
                allocatedTokens += Math.Max(0, budget);
            }
            // T5.1 (audit R1) — these omission reasons were built and discarded here, so
            // GetContextPack always reported an empty omitted[] while silently cutting
            // sections. Attribute per focus when the pack spans more than one entry.
            foreach (var line in focusOmitted)
                sectionOmissions.Add(uniqueFocuses.Count > 1 ? $"{focus} — {line}" : line);
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
                        // Same section from another entry — concatenate content AND carry the
                        // provenance across (N0.1 / audit §3.F.3: the merge used to keep only
                        // the first entry's SourceLocations/Verified/Approx, so every card on a
                        // multi-entry pack lost the provenance the single-entry path renders).
                        pickedBySection[sa.Section] = new SectionAllocation(
                            sa.Section,
                            existing.Tokens + sa.Tokens,
                            existing.Content + "\n" + sa.Content)
                        {
                            SourceLocations = [.. existing.SourceLocations
                                .Concat(sa.SourceLocations)
                                .Distinct(StringComparer.Ordinal)
                                .OrderBy(l => l, StringComparer.Ordinal)
                                .Take(20)],
                            Verified = existing.Verified + sa.Verified,
                            Approx = existing.Approx + sa.Approx,
                        };
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

        // T5.1 (audit R1) — section-level omissions ride after the card-level ones,
        // deduped (the same section line repeats across cards sharing a focus) and capped.
        const int omissionCap = 12;
        var distinctOmissions = sectionOmissions.Distinct().ToList();
        omitted.AddRange(distinctOmissions.Take(omissionCap));
        if (distinctOmissions.Count > omissionCap)
            omitted.Add($"… +{distinctOmissions.Count - omissionCap} more omissions");

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
            allocatedTokens,    // budget handed to entries that produced sections (≤ totalBudget)
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

        // G1.2 — resolve ONCE, here, so the pack knows what its root IS. A focus that resolves to a
        // declared entry point builds the pack it always has; a focus that resolves to a Type or
        // Member the entry inventory never listed is SYMBOL-ROOTED, and a symbol-rooted pack owes the
        // reader the inbound direction as well as the outbound one (see BuildUsageSection).
        var rootEntry = _query.ResolveEntry(focus);
        var symbolRooted = rootEntry is not null
            && !_snapshot.Entries.Any(e => e.Node == rootEntry.Node);

        // Build identity
        var identity = BuildIdentitySection(focus, symbolRooted ? rootEntry : null);
        tokensAddSection(sections, omitted, budgetTokens, "identity", identity, ref budgetTokens);

        // T4.2 — the spine scales with the budget: a bigger budget buys a DEEPER walk (more
        // members to sign and embody), never more prose. Depth 4 starved the fill — dogfood's
        // checkout spine is 46 steps at depth 6 but ~12 at depth 4.
        var depth = mode == "review" || budgetTokens >= 3000 ? 6 : 4;
        var fanOut = budgetTokens >= 6000 ? 16 : 12;
        var trace = rootEntry is null ? null : _query.Trace(rootEntry, depth, fanOut);
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
        var shapedTrace = EstimateTokens(skeletonFull) <= skeletonCap
            ? trace
            : TraceBuilder.ShapeToBudget(trace, skeletonCap);
        var skeleton = ReferenceEquals(shapedTrace, trace) ? skeletonFull : BuildTraceSkeleton(shapedTrace);
        var skeletonProv = new SectionProvenance();
        foreach (var step in WalkSteps(shapedTrace.Root))
        {
            skeletonProv.Tally(step.Resolution);
            if (step.Node.FilePath is { } fp)
                skeletonProv.Locations.Add(Location(fp, step.Node.LineNumber));
        }
        var sigProv = new SectionProvenance();
        var signatures = BuildCalleeSignatures(trace, signaturesCap, sigProv);
        var contractsProv = new SectionProvenance();
        var contracts = BuildContracts(trace, contractsProv);
        var diProv = new SectionProvenance();
        var di = BuildDiRegistrations(trace, diProv);
        var configProv = new SectionProvenance();
        var config = BuildConfigSection(trace, configProv);
        var testsProv = new SectionProvenance();
        var tests = BuildTestsSection(trace, testsProv);
        // G1.2 — symbol-rooted only. A declared entry has no meaningful inbound direction (nothing in
        // the repo calls an HTTP endpoint), so building this for one would only add an
        // "usage: empty — omitted" line to every entry pack in the product.
        var usageProv = new SectionProvenance();
        var usage = symbolRooted ? BuildUsageSection(rootEntry!.Node, usageProv) : "";

        if (mode == "explain")
        {
            tokensAddSection(sections, omitted, budgetTokens, "di_wiring", di, ref budgetTokens, diProv);
            tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "config", config, ref budgetTokens, configProv);
            tokensAddSection(sections, omitted, budgetTokens, "tests", tests, ref budgetTokens, testsProv);
            tokensAddSection(sections, omitted, budgetTokens, "signatures", signatures, ref budgetTokens, sigProv);
            if (symbolRooted)
                tokensAddSection(sections, omitted, budgetTokens, "usage", usage, ref budgetTokens, usageProv);
            tokensAddSection(sections, omitted, budgetTokens, "contracts", contracts, ref budgetTokens, contractsProv);
            tokensAddSection(sections, omitted, budgetTokens, "trace", skeleton, ref budgetTokens, skeletonProv);
        }
        else if (mode == "review")
        {
            if (!tokensAddSection(sections, omitted, budgetTokens, "trace", skeleton, ref budgetTokens, skeletonProv))
                return ([.. sections], [.. omitted]);

            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", signatures, ref budgetTokens, sigProv))
                return ([.. sections], [.. omitted]);

            if (symbolRooted)
                tokensAddSection(sections, omitted, budgetTokens, "usage", usage, ref budgetTokens, usageProv);
            tokensAddSection(sections, omitted, budgetTokens, "contracts", contracts, ref budgetTokens, contractsProv);
            tokensAddSection(sections, omitted, budgetTokens, "di_wiring", di, ref budgetTokens, diProv);
            tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "config", config, ref budgetTokens, configProv);
            tokensAddSection(sections, omitted, budgetTokens, "tests", tests, ref budgetTokens, testsProv);
        }
        else
        {
            if (!tokensAddSection(sections, omitted, budgetTokens, "trace", skeleton, ref budgetTokens, skeletonProv))
                return ([.. sections], [.. omitted]);

            if (!tokensAddSection(sections, omitted, budgetTokens, "signatures", signatures, ref budgetTokens, sigProv))
                return ([.. sections], [.. omitted]);

            if (symbolRooted)
                tokensAddSection(sections, omitted, budgetTokens, "usage", usage, ref budgetTokens, usageProv);
            tokensAddSection(sections, omitted, budgetTokens, "contracts", contracts, ref budgetTokens, contractsProv);
            tokensAddSection(sections, omitted, budgetTokens, "di_wiring", di, ref budgetTokens, diProv);
            tokensAddSection(sections, omitted, budgetTokens, "entities", entities, ref budgetTokens);
            tokensAddSection(sections, omitted, budgetTokens, "config", config, ref budgetTokens, configProv);
            tokensAddSection(sections, omitted, budgetTokens, "tests", tests, ref budgetTokens, testsProv);
        }

        var bodiesProv = new SectionProvenance();
        // −30: headroom for the provenance footer tokensAddSection appends, so an exact fill
        // can't tip the section over the remaining budget.
        var (bodies, omittedBodies) = BuildBodiesToFill(trace, budgetTokens - 30, bodiesProv);
        tokensAddSection(sections, omitted, budgetTokens, "bodies", bodies, ref budgetTokens, bodiesProv);
        if (omittedBodies > 0)
            omitted.Add($"bodies: {omittedBodies} member bodies omitted (budget) — raise budgetTokens or read_source the member");

        return ([.. sections], [.. omitted]);
    }

    private static bool tokensAddSection(
        List<SectionAllocation> sections, List<string> omitted,
        int totalBudget, string sectionName, string text, ref int remainingBudget,
        SectionProvenance? prov = null)
    {
        // T4.6 — an empty section never ships (audit C2's "Entities — 0 tok"); it is recorded
        // in omitted[] so the reader knows the pack looked and found nothing.
        if (string.IsNullOrWhiteSpace(text))
        {
            omitted.Add($"{sectionName}: empty — omitted");
            return true;
        }

        // T4.4 (R10) — every section says where it came from and how sure it is.
        if (prov is { IsEmpty: false })
        {
            if (!text.EndsWith('\n')) text += "\n";
            text += $"_provenance: {prov.Locations.Count} source sites · {prov.Verified} verified · {prov.Approx} approx_\n";
        }

        var tokens = EstimateTokens(text);
        if (tokens > remainingBudget)
        {
            if (sections.Sum(s => s.Tokens) < totalBudget * 0.6 && remainingBudget > 100)
            {
                var trimmed = TruncateToBudget(text, remainingBudget - 50);
                var trimTokens = EstimateTokens(trimmed);
                sections.Add(Allocate(sectionName, trimTokens, trimmed, prov));
                omitted.Add($"{sectionName}: trimmed from {tokens} to {trimTokens} tokens");
                remainingBudget -= trimTokens;
                return true;
            }
            omitted.Add($"{sectionName}: omitted ({tokens} tokens, budget exhausted)");
            return false;
        }
        sections.Add(Allocate(sectionName, tokens, text, prov));
        remainingBudget -= tokens;
        return true;

        static SectionAllocation Allocate(string name, int tokens, string content, SectionProvenance? prov)
            => new(name, tokens, content)
            {
                SourceLocations = prov is null ? [] : [.. prov.Locations.OrderBy(l => l, StringComparer.Ordinal).Take(20)],
                Verified = prov?.Verified ?? 0,
                Approx = prov?.Approx ?? 0,
            };
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

    private string BuildIdentitySection(string focus, EntryPoint? symbolRoot = null)
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

        // G1.2 — a symbol focus is a NAME, and a name can match several symbols. Say which one this
        // pack is rooted on, and say when the name was ambiguous, so the reader is never left
        // guessing which of five same-named members they are reading.
        if (symbolRoot is not null)
        {
            sb.Append("Rooted on symbol: ").Append(symbolRoot.Node.ToString());
            if (symbolRoot.Provenance is { Length: > 0 } file)
                sb.Append(" — ").Append(RelPath(file));
            sb.AppendLine(" (not a declared entry point)");
            var sameName = CountSymbolsNamed(focus);
            if (sameName > 1)
                sb.Append(sameName).AppendLine($" symbols share the name '{focus}' — this is the most connected one; resolve(query) lists the rest.");
        }

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

    /// <summary>How many Type/Member nodes carry this exact short name — the honesty number behind
    /// "N symbols share this name". Counts titles and member names, the two things the resolver matches.</summary>
    private int CountSymbolsNamed(string name)
    {
        var n = 0;
        foreach (var node in _query.Graph.Nodes)
        {
            if (node.Kind is not (NodeKind.Type or NodeKind.Member)) continue;
            if (string.Equals(node.Title, name, StringComparison.OrdinalIgnoreCase)
                || (node.Kind == NodeKind.Member
                    && string.Equals(Graph2.SymbolCanon.MemberNameOf(node.Id.Key), name, StringComparison.OrdinalIgnoreCase)))
                n++;
        }
        return n;
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

public sealed record SectionAllocation(string Section, int Tokens, string Content)
{
    /// <summary>T4.4 (R10) — the repo-relative file:line set this section derived from (deduped, capped).</summary>
    public ImmutableArray<string> SourceLocations { get; init; } = [];
    /// <summary>T4.4 (R10) — items resolved semantically or by detection join (trustworthy).</summary>
    public int Verified { get; init; }
    /// <summary>T4.4 (R10) — items resolved by syntax/string heuristics (approximate).</summary>
    public int Approx { get; init; }
}

/// <summary>T4.4 (R10) — accumulates a section's provenance while it renders: every source site
/// it drew from plus the resolution-tier mix (Semantic/Join = verified, Syntactic = approx).</summary>
internal sealed class SectionProvenance
{
    public HashSet<string> Locations { get; } = new(StringComparer.Ordinal);
    public int Verified { get; private set; }
    public int Approx { get; private set; }
    public bool IsEmpty => Locations.Count == 0 && Verified == 0 && Approx == 0;

    public void Tally(Resolution resolution)
    {
        if (resolution == Resolution.Syntactic) Approx++;
        else Verified++;
    }
}

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
