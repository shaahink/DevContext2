using System.Text;
using System.Web;

namespace DevContext.Core.Rendering;

/// <summary>Converts Map/Trace narrative text into simple styled HTML for the Desktop Human view.</summary>
public static class NarrativeHtmlConverter
{
    public static string Convert(string narrativeText)
    {
        if (string.IsNullOrWhiteSpace(narrativeText))
            return "<p>No content available.</p>";

        var sb = new StringBuilder();
        sb.AppendLine("<div class='narrative'>");

        var lines = narrativeText.Split('\n');
        var inTraceTree = false;
        var inCodeBody = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');

            // Empty line — end code body or trace tree if followed by section
            if (string.IsNullOrWhiteSpace(line) || line.Trim() == "")
            {
                if (inCodeBody && i + 1 < lines.Length &&
                    (lines[i + 1].TrimStart().StartsWith("MAP ") || lines[i + 1].TrimStart().StartsWith("TRACE ")
                     || IsSectionHeader(lines[i + 1])))
                {
                    sb.AppendLine("</pre>");
                    inCodeBody = false;
                }
                if (inTraceTree && i + 1 < lines.Length && IsSectionHeader(lines[i + 1]))
                {
                    sb.AppendLine("</pre>");
                    inTraceTree = false;
                }
                if (inTraceTree) sb.AppendLine();
                continue;
            }

            var trimmed = line.TrimStart();
            var indent = line.Length - trimmed.Length;

            // MAP / TRACE header
            if (trimmed.StartsWith("MAP ") || trimmed.StartsWith("TRACE "))
            {
                if (inTraceTree) { sb.AppendLine("</pre>"); inTraceTree = false; }
                if (inCodeBody) { sb.AppendLine("</pre>"); inCodeBody = false; }

                var parts = trimmed.Split("  ", 2, StringSplitOptions.RemoveEmptyEntries);
                var title = HttpUtility.HtmlEncode(trimmed);
                var kind = trimmed.StartsWith("MAP ") ? "Map" : "Trace";
                sb.AppendLine($"<h2 class='narrative-title narrative-{kind.ToLowerInvariant()}'>{title}</h2>");
                continue;
            }

            // Source provenance line after MAP/TRACE header (indented file:line)
            if ((i > 0 && (lines[i - 1].TrimStart().StartsWith("MAP ") || lines[i - 1].TrimStart().StartsWith("TRACE "))) &&
                indent >= 4 && !trimmed.StartsWith("STACK") && !trimmed.StartsWith("STYLE") && !IsSectionHeader(line))
            {
                sb.AppendLine($"<p class='narrative-src'>{HttpUtility.HtmlEncode(trimmed)}</p>");
                continue;
            }

            // STACK / STYLE lines
            if (trimmed.StartsWith("STACK ") || trimmed.StartsWith("STYLE "))
            {
                if (inTraceTree) { sb.AppendLine("</pre>"); inTraceTree = false; }
                if (inCodeBody) { sb.AppendLine("</pre>"); inCodeBody = false; }
                sb.AppendLine($"<p class='narrative-prop'>{HttpUtility.HtmlEncode(trimmed)}</p>");
                continue;
            }

            // Section headers (TOPOLOGY, ENTRY POINTS, CROSS-CUTTING, PACKAGES, TOUCHES, EMITS)
            if (IsSectionHeader(line))
            {
                if (inTraceTree) { sb.AppendLine("</pre>"); inTraceTree = false; }
                if (inCodeBody) { sb.AppendLine("</pre>"); inCodeBody = false; }
                sb.AppendLine($"<h3 class='narrative-section-title'>{HttpUtility.HtmlEncode(trimmed)}</h3>");
                continue;
            }

            // Trace tree nodes (▸, ├─, └─, │)
            if (trimmed.StartsWith("\u25B8") || trimmed.StartsWith("\u251C\u2500") ||
                trimmed.StartsWith("\u2514\u2500") || trimmed.StartsWith("\u2502") ||
                trimmed.StartsWith("\u2502  ") || line.Contains('\u2502'))
            {
                if (!inTraceTree)
                {
                    sb.AppendLine("<pre class='narrative-trace-tree'>");
                    inTraceTree = true;
                }

                // Classify: entry node, branch, or leaf
                var cssClass = trimmed.StartsWith("\u25B8") ? "trace-entry"
                    : trimmed.StartsWith("\u2502") ? "trace-branch"
                    : "trace-leaf";
                sb.AppendLine($"<span class='{cssClass}'>{HttpUtility.HtmlEncode(line)}</span>");
                continue;
            }

            // Indented content in trace (salient body lines, continuation)
            if (inTraceTree && indent >= 4)
            {
                // Check if this is a code body line (has common code patterns)
                var isCode = trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("/*")
                    || trimmed.Contains('{') || trimmed.Contains('}') || trimmed.Contains('(')
                    || trimmed.Contains(';') || trimmed.Contains("=>") || trimmed.Contains("var ");

                if (isCode && !inCodeBody)
                {
                    sb.AppendLine("</pre><pre class='narrative-code'>");
                    inCodeBody = true;
                }
                else if (!isCode && inCodeBody)
                {
                    sb.AppendLine("</pre>");
                    inCodeBody = false;
                }

                sb.AppendLine(HttpUtility.HtmlEncode(line));
                continue;
            }

            // Topology entries (indented project lines)
            if (trimmed.EndsWith("──") || (trimmed.Contains('─') && indent >= 3 && !trimmed.StartsWith("─")))
            {
                if (!inTraceTree && !inCodeBody)
                {
                    inTraceTree = true;
                    sb.AppendLine("<pre class='narrative-topology'>");
                }
                sb.AppendLine(HttpUtility.HtmlEncode(line));
                continue;
            }

            // Entry point list items (indented with path:line)
            if (indent >= 4 && (trimmed.StartsWith("GET ") || trimmed.StartsWith("POST ") ||
                trimmed.StartsWith("PUT ") || trimmed.StartsWith("DELETE ") || trimmed.StartsWith("PATCH ")))
            {
                if (!inCodeBody) { inCodeBody = true; sb.AppendLine("<pre class='narrative-entries'>"); }
                sb.AppendLine(HttpUtility.HtmlEncode(line));
                continue;
            }

            if (inCodeBody && indent >= 2)
            {
                sb.AppendLine(HttpUtility.HtmlEncode(line));
                continue;
            }

            // Package / aggregate / pipeline entries
            if (indent >= 3 && !trimmed.StartsWith("→"))
            {
                if (!inCodeBody) { inCodeBody = true; sb.AppendLine("<pre class='narrative-items'>"); }
                sb.AppendLine(HttpUtility.HtmlEncode(line));
                continue;
            }

            // Footer / hint lines
            if (trimmed.StartsWith("→"))
            {
                if (inTraceTree) { sb.AppendLine("</pre>"); inTraceTree = false; }
                if (inCodeBody) { sb.AppendLine("</pre>"); inCodeBody = false; }
                sb.AppendLine($"<p class='narrative-footer'>{HttpUtility.HtmlEncode(trimmed)}</p>");
                continue;
            }

            // Evidence lines
            if (trimmed.StartsWith("evidence:"))
            {
                if (inCodeBody) { sb.AppendLine("</pre>"); inCodeBody = false; }
                sb.AppendLine($"<p class='narrative-evidence'>{HttpUtility.HtmlEncode(trimmed)}</p>");
                continue;
            }

            // Default: plain text
            if (inTraceTree) { sb.AppendLine("</pre>"); inTraceTree = false; }
            if (inCodeBody) { sb.AppendLine("</pre>"); inCodeBody = false; }
            sb.AppendLine($"<p>{HttpUtility.HtmlEncode(trimmed)}</p>");
        }

        // Close any open tags
        if (inTraceTree) sb.AppendLine("</pre>");
        if (inCodeBody) sb.AppendLine("</pre>");

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private static bool IsSectionHeader(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("TOPOLOGY") || trimmed.StartsWith("ENTRY POINTS")
            || trimmed.StartsWith("CROSS-CUTTING") || trimmed.StartsWith("PACKAGES")
            || trimmed.StartsWith("TOUCHES ") || trimmed.StartsWith("EMITS ");
    }
}
