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

    /// <summary>Full unfiltered scan over every file that owns a graph node. Callers filter by key
    /// in-memory afterward — this is the expensive part worth caching.</summary>
    public static IReadOnlyList<ConfigBindingInfo> Scan(CodeGraph graph)
    {
        var result = new List<ConfigBindingInfo>();

        var filesByPath = graph.Nodes
            .Where(n => n.FilePath is not null)
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
