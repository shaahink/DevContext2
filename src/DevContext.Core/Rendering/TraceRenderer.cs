using System.Text;

using DevContext.Core.Graph;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Rendering;

public static class TraceRenderer
{
    /// <summary>Renders the whole trace as one string (CLI / file output). Byte-identical to the
    /// concatenation of <see cref="RenderSections"/>.</summary>
    public static string Render(Trace trace, TraceDetail detail, string? basePath = null)
    {
        var sb = new StringBuilder();
        foreach (var s in RenderSections(trace, detail, basePath))
            sb.Append(s.Text);
        return sb.ToString();
    }

    /// <summary>Renders the trace as ordered, toggleable fragments — the entry+tree ("Trace"), the
    /// touched-entities summary ("Touches"), and the emitted-events summary ("Emits") — so the
    /// desktop can show/hide each in both the Human and LLM views. Source locations are rendered
    /// relative to <paramref name="basePath"/> (the analysis root) when provided.</summary>
    public static IReadOnlyList<NarrativeSection> RenderSections(Trace trace, TraceDetail detail, string? basePath = null)
    {
        var sections = new List<NarrativeSection>();
        var entry = trace.Entry;

        var head = new StringBuilder();
        head.AppendLine($"TRACE  {entry.Title}");
        if (entry.Provenance is { } p)
            head.AppendLine($"       {PathDisplay.RelativeProvenance(basePath, p)}");
        if (entry.Project is { } proj)
            head.Append("       " + proj);
        head.AppendLine();
        RenderStep(head, trace.Root, "", detail, basePath, isLast: true, isRoot: true);
        sections.Add(new NarrativeSection("Trace", head.ToString()));

        // Summary pass — kept as separate fragments, each retaining its original spacing.
        if (trace.TouchedEntities.Length > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"TOUCHES  {string.Join(", ", trace.TouchedEntities)}");
            sections.Add(new NarrativeSection("Touches", sb.ToString()));
        }

        if (trace.EmittedEvents.Length > 0)
        {
            var sb = new StringBuilder();
            var deduped = trace.EmittedEvents.Distinct().ToImmutableArray();
            sb.AppendLine($"EMITS    {string.Join(", ", deduped)}");
            sections.Add(new NarrativeSection("Emits", sb.ToString()));
        }

        // Batch E (R2 §2.E item 3): the RESULT and NEXT blocks are RETIRED.
        //   RESULT printed "200 OK · failure → 404 Not Found" from a verb→status lookup table. The engine
        //   never read the handler's return type, its ProducesResponseType attributes, or a single throw
        //   site — it printed what a GET usually does. On an endpoint that returns 204, or 202, or throws
        //   409 on conflict, the line was simply wrong, and it was wrong in the authoritative voice the
        //   rest of the trace earns by citing file:line for everything it says.
        //   NEXT mapped event-NAME substrings ("Started" → "initial state", "Paid" → "payment
        //   processing") — an eShop vocabulary that says nothing about the repo in front of the reader.
        // The tokens they cost now go to naming the omitted branches (see TraceStep.OmittedNames), which
        // is evidence the engine actually holds.
        return sections;
    }

    private static void RenderStep(StringBuilder sb, TraceStep step, string indent, TraceDetail detail,
        string? basePath, bool isLast, bool isRoot)
    {
        var prefix = isRoot ? "\u25B8 ENTRY  " : indent + (isLast ? "\u2514\u2500 " : "\u251C\u2500 ")
            + SeamLabel(step.Seam) + " ";

        sb.Append(prefix);
        sb.Append(step.Node.Title);

        if (step.Provenance is { } p)
            sb.Append($"  ({PathDisplay.RelativeProvenance(basePath, p)})");

        // V1.1 (#25) — the tier comes from EdgeConfidence, the one definition. A Join step stays
        // unlabelled here (it is neither Roslyn-verified nor a string guess); only the two ends of
        // the scale carry a marker.
        switch (EdgeConfidence.TierOf(step.Resolution))
        {
            case EdgeTier.Approximate: sb.Append(" [approx]"); break;
            case EdgeTier.Verified: sb.Append(" [verified]"); break;
            default: break;
        }

        // I1.6 — multi-impl honesty: when DI has >1 impl for this Resolve
        if (step.MultiImplCount > 1)
            sb.Append($" [×{step.MultiImplCount} impls]");

        // C5 — N hosts register this binding and none of them is the focus host: the cited site is
        // the deterministic first, not "the" registration.
        if (step.DiHostCount > 1)
            sb.Append($" [×{step.DiHostCount} hosts]");

        // T2.1 — the binding comes only from a test project (last-resort, not the production wiring).
        if (step.TestOnly)
            sb.Append(" [test-only registration]");

        sb.AppendLine();

        // Salient body lines at --detail salient or full
        if (detail >= TraceDetail.Salient && step.Salient.Length > 0)
        {
            var bodyIndent = indent + (isLast ? "       " : "\u2502      ");
            foreach (var line in step.Salient)
                sb.AppendLine(bodyIndent + line);
        }

        // Pipeline behaviors wrapping the request, rendered once under the send (Iteration 3 Step 3).
        if (step.Pipeline.Length > 0)
        {
            var pipeIndent = indent + (isLast ? "       " : "\u2502      ");
            sb.AppendLine(pipeIndent + "pipeline \u25B8 " + string.Join(" \u2192 ", step.Pipeline));
        }

        if (step.Truncated)
        {
            var n = step.Omitted;
            var branches = n == 1 ? "branch" : "branches";
            // Batch E (R2 \u00a72.E item 3): NAME the omitted branches. A count says something was cut; the
            // names say whether it mattered and where to point the next --focus.
            var who = "";
            if (!step.OmittedNames.IsDefaultOrEmpty)
            {
                var shown = string.Join(", ", step.OmittedNames);
                who = step.OmittedNames.Length < n ? $": {shown}, \u2026" : $": {shown}";
            }
            var marker = step.Children.Length == 0
                ? $"(stopped at depth {step.Depth}; {n} {branches} omitted{who})"
                : $"({n} more {branches} omitted beyond fan-out{who})";
            sb.AppendLine(indent + (isLast ? "   " : "\u2502  ") + marker);
        }

        if (step.Children.Length == 0) return;

        var childIndent = indent + (isLast ? "   " : "\u2502  ");
        for (var i = 0; i < step.Children.Length; i++)
        {
            var child = step.Children[i];
            var childIsLast = i == step.Children.Length - 1;
            RenderStep(sb, child, childIndent, detail, basePath, childIsLast, false);
        }
    }

    private static string SeamLabel(SeamKind kind) => kind switch
    {
        SeamKind.Entry => "",
        SeamKind.Call => "call",
        SeamKind.Send => "send",
        SeamKind.Handle => "handler",
        SeamKind.Raise => "raises",
        SeamKind.Consume => "consumes",
        SeamKind.Data => "data",
        SeamKind.Resolve => "di",
        SeamKind.Pipeline => "pipeline",
        // G1.3 — the only member of the enum this switch had no arm for, so every hop between two
        // services rendered a bare "?" in the tree. Found while fixing the same class of hole in the
        // MCP's glyph map; SeamVocabularyTests now walks the enum for both.
        SeamKind.CrossService => "cross-service",
        _ => "?",
    };

}
