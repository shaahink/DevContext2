using DevContext.Core.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph;

/// <summary>One config key-usage site: where a literal configuration key is read in source.</summary>
public readonly record struct ConfigBindingInfo(
    string Key, string FilePath, int LineNumber, string NodeId, string PatternType, string Service);

/// <summary>
/// T3.4 — scans the source files behind graph nodes for literal configuration-key usage
/// (IConfiguration indexer / GetValue / GetSection / GetConnectionString / GetRequiredSection).
/// Extracted from the gRPC ConfigLookup handler so the result can be computed ONCE and cached per
/// session: the scan reads and syntax-parses every node-bearing file (10.5s on a 2.8k-node repo),
/// so re-running it on every config() call was the latency bug. The scan is pure and deterministic.
/// The match walks the Roslyn syntax tree (no regex — the Graph/ regex funeral, L2.3), so keys inside
/// comments or non-literal arguments are never mistaken for real bindings.
/// F3 (BUG-BACKLOG #34): the model-aware overload additionally merges Options-pattern bindings
/// (<c>AddOptions&lt;T&gt;().BindConfiguration</c> / <c>.Bind</c> / <c>Configure&lt;T&gt;</c>) detected at
/// extraction time, where const section names resolve project-wide — the syntax scan here cannot see
/// those (it visits only node-bearing files and reads only literals).
/// </summary>
public static class ConfigScanner
{
    // Receiver identifiers we treat as an IConfiguration instance. Compared against the receiver's
    // trailing simple name, so both `_config["x"]` and `builder.Configuration["x"]` qualify.
    private static readonly HashSet<string> ConfigReceiverNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "configuration", "iconfiguration", "config", "_config", "_configuration",
        "_cfg", "_conf", "cfg", "conf", "_c",
    };

    // config.<method>("key") calls we recognise → the PatternType label they carry.
    private static readonly Dictionary<string, string> ConfigMethods = new(StringComparer.Ordinal)
    {
        ["GetValue"] = "GetValue",
        ["GetSection"] = "GetSection",
        ["GetConnectionString"] = "GetConnectionString",
        ["GetRequiredSection"] = "GetRequiredSection",
    };

    /// <summary>The PatternType label carried by catalog rows merged in from extraction-time
    /// Options-pattern detections (F3, BUG-BACKLOG #34). Part of the documented closed set on
    /// <c>ConfigBinding.pattern_type</c> in the proto.</summary>
    public const string OptionsBindingPatternType = "OptionsBinding";

    /// <summary>F3 — the full catalog: the syntax scan below PLUS the Options-pattern bindings the
    /// extractors detected (<see cref="OptionsBindingDetection"/>). The syntax scan alone cannot see
    /// them: it only visits files that own a graph node (a composition-root DependencyInjection.cs
    /// can be skipped wholesale) and it cannot resolve a const section name across files. Every
    /// catalog consumer — the server's session catalog, the pack's config section — reads THIS
    /// overload so the numbers agree with <c>ConfigDefaultsSource</c>, which counts the same
    /// detections.</summary>
    public static IReadOnlyList<ConfigBindingInfo> Scan(
        CodeGraph graph, DiscoveryModel model, ISet<string>? onlyFiles = null)
    {
        var syntaxRows = Scan(graph, onlyFiles);
        var optionsRows = OptionsBindings(model, graph, onlyFiles);
        if (optionsRows.Count == 0) return syntaxRows;

        // One binding, one row: Configure<T>(cfg.GetSection("X")) is caught by BOTH paths at the
        // same site — the options row (which knows the bound type's pattern) wins the collision.
        static string Site(ConfigBindingInfo b) => $"{b.Key}\n{b.FilePath}\n{b.LineNumber}";
        var optionsSites = new HashSet<string>(optionsRows.Select(Site), StringComparer.OrdinalIgnoreCase);

        var merged = new List<ConfigBindingInfo>(syntaxRows.Count + optionsRows.Count);
        merged.AddRange(syntaxRows.Where(row => !optionsSites.Contains(Site(row))));
        merged.AddRange(optionsRows);
        return merged;
    }

    /// <summary>F3 — projects <see cref="OptionsBindingDetection"/> rows into catalog form. The ONE
    /// source for Options-pattern section keys: the catalog overload above merges these rows, and
    /// <c>ConfigDefaultsSource</c> counts their keys, so the two surfaces cannot disagree.</summary>
    public static IReadOnlyList<ConfigBindingInfo> OptionsBindings(
        DiscoveryModel model, CodeGraph? graph = null, ISet<string>? onlyFiles = null)
    {
        var rows = new List<ConfigBindingInfo>();

        // Best-effort service attribution: the project of any graph node in the binding's file.
        Dictionary<string, string>? projectByFile = null;
        if (graph is not null)
        {
            projectByFile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in graph.Nodes)
                if (node.FilePath is { } fp && node.Project is { } project)
                    projectByFile.TryAdd(fp, project);
        }

        foreach (var detection in model.Detections.OfType<OptionsBindingDetection>())
        {
            if (onlyFiles is not null && !onlyFiles.Contains(detection.SourceFile)) continue;
            var service = projectByFile?.GetValueOrDefault(detection.SourceFile) ?? "";
            rows.Add(new ConfigBindingInfo(
                detection.SectionKey, detection.SourceFile, detection.LineNumber,
                NodeId: "", OptionsBindingPatternType, service));
        }

        // model.Detections is a ConcurrentBag — order it so the catalog stays deterministic.
        return rows
            .OrderBy(r => r.FilePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.LineNumber)
            .ThenBy(r => r.Key, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>The syntax half of the catalog: a full scan over every file that owns a graph node.
    /// Callers filter by key in-memory afterward — this is the expensive part worth caching. Pass
    /// <paramref name="onlyFiles"/> to scan a file subset instead (T4.3: the pack's config section
    /// scans just the spine's files). Prefer the model-aware overload — this one cannot see
    /// Options-pattern bindings.</summary>
    public static IReadOnlyList<ConfigBindingInfo> Scan(CodeGraph graph, ISet<string>? onlyFiles = null)
    {
        var result = new List<ConfigBindingInfo>();

        var filesByPath = graph.Nodes
            .Where(n => n.FilePath is not null && (onlyFiles is null || onlyFiles.Contains(n.FilePath!)))
            .GroupBy(n => n.FilePath!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var (filePath, nodes) in filesByPath)
        {
            if (!File.Exists(filePath)) continue;
            string text;
            try { text = File.ReadAllText(filePath); }
            catch { continue; }

            SyntaxNode root;
            try { root = CSharpSyntaxTree.ParseText(text, path: filePath).GetRoot(); }
            catch { continue; }

            var service = nodes.Count > 0 ? nodes[0].Project ?? "" : "";

            foreach (var syntax in root.DescendantNodes())
            {
                var (key, patternType, line) = MatchConfigKey(syntax);
                if (key is null) continue;

                string? nodeId = null;
                foreach (var node in nodes)
                    if (node.LineNumber is { } ln && ln == line)
                    {
                        nodeId = node.Id.ToString();
                        break;
                    }

                result.Add(new ConfigBindingInfo(key, filePath, line, nodeId ?? "", patternType, service));
            }
        }

        return result;
    }

    /// <summary>Recognises a single config-key access expression; returns null Key otherwise.</summary>
    private static (string? Key, string PatternType, int Line) MatchConfigKey(SyntaxNode node)
    {
        switch (node)
        {
            // config["Key"] — indexer access with a single string-literal argument.
            case ElementAccessExpressionSyntax indexer
                when IsConfigReceiver(indexer.Expression)
                     && indexer.ArgumentList.Arguments.Count == 1
                     && LiteralArg(indexer.ArgumentList.Arguments[0].Expression) is { } indexerKey:
                return (indexerKey, "Indexer", LineOf(indexer));

            // config.GetValue<T>("Key") / GetSection / GetConnectionString / GetRequiredSection.
            case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax member } call
                when ConfigMethods.TryGetValue(member.Name.Identifier.ValueText, out var pattern)
                     && IsConfigReceiver(member.Expression)
                     && call.ArgumentList.Arguments.Count >= 1
                     && LiteralArg(call.ArgumentList.Arguments[0].Expression) is { } methodKey:
                return (methodKey, pattern, LineOf(call));

            default:
                return (null, "", 0);
        }
    }

    private static bool IsConfigReceiver(ExpressionSyntax expr) =>
        ConfigReceiverNames.Contains(TrailingName(expr));

    // The rightmost simple name of a receiver: `builder.Configuration` → "Configuration", `_config` → "_config".
    private static string TrailingName(ExpressionSyntax expr) => expr switch
    {
        IdentifierNameSyntax id => id.Identifier.ValueText,
        MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
        _ => "",
    };

    private static string? LiteralArg(ExpressionSyntax expr) =>
        expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression)
            ? lit.Token.ValueText
            : null;

    private static int LineOf(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
}
