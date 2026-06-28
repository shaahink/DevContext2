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

        // RESULT — the entry's expected outcome
        var resultText = ResultForEntry(trace.Entry);
        if (resultText is not null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"RESULT   {resultText}");
            sections.Add(new NarrativeSection("Result", sb.ToString()));
        }

        // NEXT — lifecycle hints from emitted events suggest what follows
        if (trace.EmittedEvents.Length > 0)
        {
            var lifecycleHints = new List<string>();
            foreach (var evt in trace.EmittedEvents)
            {
                if (evt.Contains("Started", StringComparison.OrdinalIgnoreCase))
                    lifecycleHints.Add("initial state");
                if (evt.Contains("Confirmed", StringComparison.OrdinalIgnoreCase))
                    lifecycleHints.Add("status transition");
                if (evt.Contains("Paid", StringComparison.OrdinalIgnoreCase))
                    lifecycleHints.Add("payment processing");
                if (evt.Contains("Shipped", StringComparison.OrdinalIgnoreCase))
                    lifecycleHints.Add("fulfillment");
                if (evt.Contains("Cancelled", StringComparison.OrdinalIgnoreCase))
                    lifecycleHints.Add("cancellation");
            }
            var hints = lifecycleHints.Distinct().ToList();
            if (hints.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"NEXT     {string.Join(" → ", hints)}");
                sections.Add(new NarrativeSection("Next", sb.ToString()));
            }
        }

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

        if (step.Resolution is Resolution.Syntactic)
            sb.Append(" [approx]");
        else if (step.Resolution is Resolution.Join or Resolution.Semantic)
        {
            // verified — no label needed, or label "verified" for semantic
            if (step.Resolution == Resolution.Semantic)
                sb.Append(" [verified]");
        }

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
            var marker = step.Children.Length == 0
                ? $"(stopped at depth {step.Depth}; {n} {branches} omitted)"
                : $"({n} more {branches} omitted beyond fan-out)";
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
        _ => "?",
    };

    private static string? ResultForEntry(EntryPoint entry) => entry.Kind switch
    {
        EntryPointKind.HttpEndpoint => entry.HttpMethod switch
        {
            "GET" => "200 OK · failure → 404 Not Found",
            "POST" => "200 OK / 201 Created · failure → 400 Bad Request",
            "PUT" => "200 OK / 204 No Content · failure → 400 Bad Request",
            "DELETE" => "200 OK / 204 No Content · failure → 404 Not Found",
            "PATCH" => "200 OK · failure → 400 Bad Request",
            _ => null,
        },
        _ => null,
    };
}
