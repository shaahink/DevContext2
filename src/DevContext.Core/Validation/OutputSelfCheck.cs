using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using DevContext.Core.Constants;
using DevContext.Core.Contracts;
using DevContext.Core.Extractors.Specific;
using DevContext.Core.Models;

namespace DevContext.Core.Validation;

/// <summary>Records the result of a single self-check invariant.</summary>
public sealed record SelfCheckResult(string CheckId, bool Passed, string Detail);

/// <summary>
/// Pure static validator that inspects rendered output against invariants.
/// Called at the end of every pipeline run. Checks are cheap by design;
/// <c>deterministic</c> is the only expensive check and is test-only.
/// </summary>
public static partial class OutputSelfCheck
{
    private static readonly FrozenSet<string> KnownSectionNames = BuildKnownSectionNames();

    /// <summary>Safety margin (tokens) applied to the budget-respected check.</summary>
    public const int TokenBudgetSafetyMargin = 500;

    /// <summary>Runs all enabled self-check invariants and returns results.</summary>
    public static ImmutableArray<SelfCheckResult> Check(
        RenderedContext rendered,
        DiscoveryModel model,
        RenderOptions renderOptions,
        bool includeTestOnly = false)
    {
        var results = ImmutableArray.CreateBuilder<SelfCheckResult>(12);

        results.Add(BudgetRespected(rendered, renderOptions));
        results.Add(NoEmptySections(rendered));
        results.Add(SectionsKnown(rendered));
        results.Add(DetectionsSourced(model));
        results.Add(NoDuplicateDetections(model));
        results.Add(NoDynamicRoutes(rendered));
        results.Add(JsonSchemaValid(rendered));
        results.Add(HtmlWellFormed(rendered));
        results.Add(FunnelConsistent(model));

        if (includeTestOnly)
            results.Add(Deterministic(rendered));

        return results.ToImmutable();
    }

    // ── helpers ────────────────────────────────────────────────────────

    private static SelfCheckResult Pass(string checkId, string detail = "")
        => new(checkId, true, detail);

    private static SelfCheckResult Fail(string checkId, string detail)
        => new(checkId, false, detail);

    private static SelfCheckResult NotApplicable(string checkId, string reason)
        => new(checkId, true, $"N/A: {reason}");

    private static FrozenSet<string> BuildKnownSectionNames()
    {
        var names = typeof(SectionNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        return names;
    }

    // ── checks ─────────────────────────────────────────────────────────

    /// <summary>budget‑respected — estimated tokens must not exceed the cap plus safety margin.</summary>
    private static SelfCheckResult BudgetRespected(RenderedContext rendered, RenderOptions renderOptions)
    {
        var max = renderOptions.EstimatedTokens;
        var actual = rendered.EstimatedTokens;
        var effective = max + TokenBudgetSafetyMargin;
        if (actual <= effective)
            return Pass("budget-respected", $"Estimate {actual} <= budget {max} (+{TokenBudgetSafetyMargin})");
        return Fail("budget-respected",
            $"Estimate {actual} exceeds budget {max} by {actual - max} tokens (margin {TokenBudgetSafetyMargin})");
    }

    /// <summary>no-empty‑sections — no rendered section header must have zero content beneath it.</summary>
    private static SelfCheckResult NoEmptySections(RenderedContext rendered)
    {
        var content = rendered.Content;
        // Only meaningful for markdown — JSON and HTML have different structure
        if (content.Length == 0)
            return Pass("no-empty-sections", "Empty output");

        if (!LooksLikeMarkdown(content))
            return NotApplicable("no-empty-sections", "Output does not appear to be markdown");

        // Find all ## headers and check the gap to the next header or EOF
        var headerMatches = MarkdownH2Regex().Matches(content);
        var emptyHeaders = new List<string>();

        for (int i = 0; i < headerMatches.Count; i++)
        {
            var currentMatch = headerMatches[i];
            var headerName = currentMatch.Groups[1].Value.Trim();
            var bodyStart = currentMatch.Index + currentMatch.Length;
            var bodyEnd = (i + 1 < headerMatches.Count)
                ? headerMatches[i + 1].Index
                : content.Length;

            var body = content[bodyStart..bodyEnd];
            var hasContent = HasNonTrivialContent(body);
            if (!hasContent)
                emptyHeaders.Add($"\"{headerName}\"");
        }

        if (emptyHeaders.Count == 0)
            return Pass("no-empty-sections");
        return Fail("no-empty-sections",
            $"Empty section(s): {string.Join(", ", emptyHeaders)}");
    }

    /// <summary>sections‑known — every rendered section name must be a known SectionName constant.</summary>
    private static SelfCheckResult SectionsKnown(RenderedContext rendered)
    {
        var content = rendered.Content;
        if (content.Length == 0)
            return Pass("sections-known", "Empty output");

        if (!LooksLikeMarkdown(content))
            return NotApplicable("sections-known", "Output does not appear to be markdown");

        var headers = MarkdownH2Regex().Matches(content)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        // Exclude the document title line (e.g. "DevContext — overview on X")
        var unknown = headers
            .Where(h => !h.StartsWith("DevContext ", StringComparison.OrdinalIgnoreCase)
                        && !h.StartsWith("DevContext —", StringComparison.OrdinalIgnoreCase))
            // Strip trailing parentheticals like "(EF Core)", "(in-memory bus)"
            .Select(NormaliseSectionName)
            .Where(normalised => !KnownSectionNames.Contains(normalised))
            .ToList();

        if (unknown.Count == 0)
            return Pass("sections-known");
        return Fail("sections-known",
            $"Unknown section header(s): {string.Join(", ", unknown.Distinct(StringComparer.Ordinal))}");
    }

    /// <summary>detections‑sourced — every detection must have a non‑empty SourceFile and LineNumber > 0.</summary>
    private static SelfCheckResult DetectionsSourced(DiscoveryModel model)
    {
        var violations = model.Detections
            .Where(d => string.IsNullOrWhiteSpace(d.SourceFile) || d.LineNumber <= 0)
            .Select(d => $"{d.GetType().Name}(file='{d.SourceFile ?? "<null>"}', line={d.LineNumber})")
            .Take(10)
            .ToList();

        if (violations.Count == 0)
            return Pass("detections-sourced", $"All {model.Detections.Count} detections sourced");
        return Fail("detections-sourced",
            $"Unsourced detection(s): {string.Join("; ", violations)}");
    }

    /// <summary>no‑duplicate‑detections — no two detections share the same stable key.</summary>
    private static SelfCheckResult NoDuplicateDetections(DiscoveryModel model)
    {
        var seen = new Dictionary<string, Detection>(StringComparer.Ordinal);
        var duplicates = new List<string>();

        foreach (var d in model.Detections)
        {
            var key = BuildStableKey(d);
            if (seen.TryGetValue(key, out var existing))
                duplicates.Add($"{d.GetType().Name} key='{key}' (duplicate of #{model.Detections.IndexOf(existing)})");
            else
                seen[key] = d;
        }

        if (duplicates.Count == 0)
            return Pass("no-duplicate-detections", $"No duplicates in {model.Detections.Count} detections");
        return Fail("no-duplicate-detections",
            $"Duplicate detection(s): {string.Join("; ", duplicates)}");
    }

    /// <summary>no‑dynamic‑routes — output must not contain the literal string &lt;dynamic&gt;.</summary>
    private static SelfCheckResult NoDynamicRoutes(RenderedContext rendered)
    {
        if (!rendered.Content.Contains("<dynamic>", StringComparison.Ordinal))
            return Pass("no-dynamic-routes");
        var count = CountOccurrences(rendered.Content, "<dynamic>");
        return Fail("no-dynamic-routes", $"Found {count} '<dynamic>' placeholder(s) in output");
    }

    /// <summary>json‑schema‑valid — JSON output must deserialize to DevContextOutput round‑trip.</summary>
    private static SelfCheckResult JsonSchemaValid(RenderedContext rendered)
    {
        var content = rendered.Content.TrimStart();
        if (content.Length == 0 || content[0] != '{')
            return NotApplicable("json-schema-valid", "Output is not JSON");

        try
        {
            var deserialized = JsonSerializer.Deserialize<DevContextOutput>(content);
            if (deserialized is null)
                return Fail("json-schema-valid", "Deserialized to null");
            return Pass("json-schema-valid");
        }
        catch (JsonException ex)
        {
            return Fail("json-schema-valid", $"JSON deserialization failed: {ex.Message}");
        }
    }

    /// <summary>html‑well‑formed — HTML output must parse without fatal errors.</summary>
    private static SelfCheckResult HtmlWellFormed(RenderedContext rendered)
    {
        var content = rendered.Content.TrimStart();
        if (content.Length == 0)
            return NotApplicable("html-well-formed", "Empty output");

        // Quick heuristic: HTML content starts with <!DOCTYPE, <html, or <article
        if (!LooksLikeHtml(content))
            return NotApplicable("html-well-formed", "Output does not appear to be HTML");

        try
        {
            // XDocument.Parse requires a single root element; wrap if needed
            var toParse = content;
            if (!content.StartsWith("<article", StringComparison.OrdinalIgnoreCase)
                && !content.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
                && !content.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
            {
                toParse = $"<root>{content}</root>";
            }

            var doc = XDocument.Parse(toParse);
            return Pass("html-well-formed");
        }
        catch (System.Xml.XmlException ex)
        {
            return Fail("html-well-formed", $"HTML/Xml parse error: {ex.Message}");
        }
    }

    /// <summary>funnel‑consistent — types discovered = included + excluded.</summary>
    private static SelfCheckResult FunnelConsistent(DiscoveryModel model)
    {
        if (model.Types.IsEmpty && model.PrunedTypeIds.Count == 0)
            return NotApplicable("funnel-consistent", "No types discovered");

        var total = model.Types.Count;
        var pruned = model.PrunedTypeIds.Count;
        var included = model.Types.Values.Count(t => !t.IsHardExcluded);

        // TODO(Plan 3): once explicit included/excluded tracking lands, tighten this check.
        // Currently pre-Plan-1/3: total should equal included + pruned (all pruned IDs reference known types).
        if (total == included + pruned)
            return Pass("funnel-consistent",
                $"Types: {total} total = {included} included + {pruned} pruned");

        return Fail("funnel-consistent",
            $"Funnel mismatch: {total} total != {included} included + {pruned} pruned (off by {total - included - pruned})");
    }

    /// <summary>deterministic — same input rendered twice must be byte‑identical (test‑only, expensive).</summary>
    internal static SelfCheckResult Deterministic(RenderedContext rendered)
    {
        // This is a test-only check; in production the pipeline only renders once.
        // The test calls this by rendering twice and comparing byte-identical.
        return NotApplicable("deterministic",
            "Expensive — run only in tests by rendering the same model twice and comparing outputs");
    }

    // ── private helpers ─────────────────────────────────────────────────

    private static bool LooksLikeMarkdown(string content)
    {
        // Markdown typically starts without HTML/XML/JSON preamble
        var trimmed = content.TrimStart();
        return !trimmed.StartsWith('{')
               && !trimmed.StartsWith('<')
               && (trimmed.StartsWith('#') || trimmed.StartsWith("DevContext", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeHtml(string content)
    {
        var trimmed = content.TrimStart();
        return trimmed.StartsWith("<article", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Checks whether a markdown section body has non‑trivial content.</summary>
    private static bool HasNonTrivialContent(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return false;

        // Strip horizontal rules (---), blank lines, and inline separators
        var cleaned = HorizontalRuleRegex().Replace(body, "");

        // If only whitespace remains, the section is effectively empty
        return cleaned.Any(c => !char.IsWhiteSpace(c));
    }

    /// <summary>Strips trailing parenthetical annotations from section names.</summary>
    private static string NormaliseSectionName(string header)
    {
        // "Data model (EF Core)" → "Data model"
        // "Event flow (in-memory bus)" → "Event flow"
        var paren = header.IndexOf('(');
        return paren > 0
            ? header[..paren].TrimEnd()
            : header;
    }

    /// <summary>
    /// Builds a stable deduplication key for a detection.
    /// Type name + SourceFile + LineNumber + type‑specific distinguishing fields.
    /// </summary>
    internal static string BuildStableKey(Detection d)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(d.GetType().Name);
        sb.Append('|');
        sb.Append(d.SourceFile);
        sb.Append('|');
        sb.Append(d.LineNumber);
        sb.Append('|');

        AppendDetectionFields(sb, d);

        return sb.ToString();
    }

    private static void AppendDetectionFields(System.Text.StringBuilder sb, Detection d)
    {
        d.AppendSelfCheckFields(sb);
    }

    internal static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var idx = 0;
        while ((idx = text.IndexOf(value, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += value.Length;
        }
        return count;
    }

    // IndexOf extension helper for List<Detection> (used in no-duplicate-detections detail message)
    private static int IndexOf(this ConcurrentBag<Detection> bag, Detection target)
    {
        // ConcurrentBag doesn't provide indexed access; snapshot it
        var snapshot = bag.ToList();
        for (int i = 0; i < snapshot.Count; i++)
            if (ReferenceEquals(snapshot[i], target))
                return i;
        return -1;
    }

    [GeneratedRegex(@"^## (.+)$", RegexOptions.Multiline)]
    private static partial Regex MarkdownH2Regex();

    [GeneratedRegex(@"^-{3,}\s*$", RegexOptions.Multiline)]
    private static partial Regex HorizontalRuleRegex();
}
