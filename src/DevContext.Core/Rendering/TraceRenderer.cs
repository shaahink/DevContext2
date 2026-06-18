using System.Text;

using DevContext.Core.Graph;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Rendering;

public static class TraceRenderer
{
    /// <summary>Renders the whole trace as one string (CLI / file output). Byte-identical to the
    /// concatenation of <see cref="RenderSections"/>.</summary>
    public static string Render(Trace trace, TraceDetail detail)
    {
        var sb = new StringBuilder();
        foreach (var s in RenderSections(trace, detail))
            sb.Append(s.Text);
        return sb.ToString();
    }

    /// <summary>Renders the trace as ordered, toggleable fragments — the entry+tree ("Trace"), the
    /// touched-entities summary ("Touches"), and the emitted-events summary ("Emits") — so the
    /// desktop can show/hide each in both the Human and LLM views.</summary>
    public static IReadOnlyList<NarrativeSection> RenderSections(Trace trace, TraceDetail detail)
    {
        var sections = new List<NarrativeSection>();
        var entry = trace.Entry;

        var head = new StringBuilder();
        head.AppendLine($"TRACE  {entry.Title}");
        if (entry.Provenance is { } p)
            head.AppendLine($"       {p}");
        if (entry.Project is { } proj)
            head.Append("       " + proj);
        head.AppendLine();
        RenderStep(head, trace.Root, "", detail, isLast: true, isRoot: true);
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
            sb.AppendLine($"EMITS    {string.Join(", ", trace.EmittedEvents)}");
            sections.Add(new NarrativeSection("Emits", sb.ToString()));
        }

        return sections;
    }

    private static void RenderStep(StringBuilder sb, TraceStep step, string indent, TraceDetail detail,
        bool isLast, bool isRoot)
    {
        var prefix = isRoot ? "\u25B8 ENTRY  " : indent + (isLast ? "\u2514\u2500 " : "\u251C\u2500 ")
            + SeamLabel(step.Seam) + " ";

        sb.Append(prefix);
        sb.Append(step.Node.Title);

        if (step.Provenance is { } p)
            sb.Append($"  ({p})");

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

        if (step.Truncated)
        {
            sb.AppendLine(indent + (isLast ? "   " : "\u2502  ") + "(truncated — more edges beyond depth/fan-out)");
        }

        if (step.Children.Length == 0) return;

        var childIndent = indent + (isLast ? "   " : "\u2502  ");
        for (var i = 0; i < step.Children.Length; i++)
        {
            var child = step.Children[i];
            var childIsLast = i == step.Children.Length - 1;
            RenderStep(sb, child, childIndent, detail, childIsLast, false);
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
}
