using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Utilities;

/// <summary>
/// C1 (Prism D2): turns a Blazor component's <c>@code</c>/<c>@functions</c> blocks and
/// <c>@inject</c> directives into a small VIRTUAL C# compilation unit — a partial class named after
/// the component, in the component's real namespace, containing ONLY the extracted C#. The Razor
/// MARKUP is never parsed as C# (the T-era perf trap: whole-file Razor-as-C# parses produced garbage
/// trees and a huge semantic-compilation hit on Razor-heavy repos). <c>#line</c> directives map every
/// extracted region back to its true <c>.razor</c> line, so call-site provenance stays honest.
/// <para>The virtual namespace follows Blazor's own default: an explicit <c>@namespace</c> directive
/// (own file, else nearest ancestor <c>_Imports.razor</c>), else the owning project's RootNamespace
/// (csproj file name when unset) plus the folder path — which is exactly what a code-behind
/// <c>.razor.cs</c> partial declares, so the virtual class and a real code-behind MERGE into one
/// type.</para>
/// </summary>
public static class RazorCodeVirtualizer
{
    private static readonly Regex InjectDirective = new(
        @"^\s*@inject\s+(?<type>[A-Za-z_][A-Za-z0-9_.<>,?\[\] ]*?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex NamespaceDirective = new(
        @"^\s*@namespace\s+(?<ns>[A-Za-z_][A-Za-z0-9_.]*)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex UsingDirective = new(
        @"^\s*@using\s+(?<u>[A-Za-z_][^\r\n;]*?)\s*;?\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex InheritsDirective = new(
        @"^\s*@inherits\s+(?<base>[A-Za-z_][A-Za-z0-9_.<>, ]*)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex RootNamespaceProperty = new(
        @"<RootNamespace>\s*([A-Za-z_][A-Za-z0-9_.]*)\s*</RootNamespace>",
        RegexOptions.Compiled);

    /// <summary>Enumerates the virtual syntax trees for every in-scope <c>.razor</c> component that
    /// carries extractable C# (<c>@code</c>/<c>@functions</c>/<c>@inject</c>). Trees are built once per
    /// analysis via <see cref="SharedAnalysisContext.RazorVirtualTrees"/> — the second consumer
    /// (structure vs call graph) gets the memoised parse. Read failures surface as model diagnostics
    /// at the consumer, not here.</summary>
    public static async IAsyncEnumerable<(string Path, SyntaxTree Tree)> EnumerateVirtualTreesAsync(
        DiscoveryContext context, [EnumeratorCancellation] CancellationToken ct)
    {
        var razorFiles = context.Analysis.AllContentFiles
            .Where(f => f.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
                && !Path.GetFileNameWithoutExtension(f).StartsWith('_'))
            .ToList();
        if (razorFiles.Count == 0) yield break;

        var namespaces = await NamespaceIndex.BuildAsync(context, ct);

        foreach (var filePath in razorFiles)
        {
            ct.ThrowIfCancellationRequested();
            SyntaxTree? tree;
            try
            {
                tree = await context.Analysis.GetOrBuildRazorVirtualTreeAsync(filePath, async () =>
                {
                    var text = await context.Cache.GetTextAsync(filePath, ct);
                    var source = BuildVirtualSource(filePath, text,
                        namespaces.Resolve(filePath, text), namespaces.InheritedUsings(filePath));
                    return source is null
                        ? null
                        : CSharpSyntaxTree.ParseText(source, path: filePath, cancellationToken: ct);
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                context.Logger.LogWarning(ex, "Razor @code virtualization failed for {Path}", filePath);
                continue;
            }
            if (tree is not null) yield return (filePath, tree);
        }
    }

    /// <summary>Builds the virtual C# source for one component, or null when the file carries no
    /// extractable C# (a markup-only component) or its name is not a valid identifier. The file's own
    /// <c>@using</c> directives plus <paramref name="inheritedUsings"/> (from ancestor
    /// <c>_Imports.razor</c>, exactly Blazor's own scoping) are emitted so the semantic compilation
    /// binds injected-service calls for real ([verified] edges, not [approx]).</summary>
    public static string? BuildVirtualSource(string razorPath, string razorText, string? namespaceName,
        IReadOnlyList<string>? inheritedUsings = null)
    {
        var componentName = Path.GetFileNameWithoutExtension(razorPath);
        if (!SyntaxFacts.IsValidIdentifier(componentName)) return null;

        var blocks = ExtractCodeBlocks(razorText);
        var injects = InjectDirective.Matches(razorText);
        if (blocks.Count == 0 && injects.Count == 0) return null;

        var inheritsMatch = InheritsDirective.Match(razorText);
        var baseType = inheritsMatch.Success
            ? inheritsMatch.Groups["base"].Value.Trim()
            : "Microsoft.AspNetCore.Components.ComponentBase";

        var sb = new StringBuilder(256 + razorText.Length / 2);
        sb.AppendLine($"// <virtual> @code of {Path.GetFileName(razorPath)} — markup never parsed (C1)");
        if (namespaceName is not null)
            sb.AppendLine($"namespace {namespaceName};");
        foreach (var u in EnumerateUsings(razorText, inheritedUsings))
            sb.AppendLine($"using {u};");
        sb.AppendLine($"public partial class {componentName} : {baseType}");
        sb.AppendLine("{");
        foreach (Match m in injects)
        {
            // Blazor generates an [Inject]-attributed property per @inject — a property, so the
            // call graph's field map resolves "PodcastService.GetShows(...)" to the injected type.
            sb.AppendLine($"    public {m.Groups["type"].Value.Trim()} {m.Groups["name"].Value} {{ get; set; }} = default!;");
        }
        foreach (var block in blocks)
        {
            sb.AppendLine($"#line {block.StartLine} \"{razorPath}\"");
            sb.AppendLine(razorText[block.ContentStart..block.ContentEnd]);
            sb.AppendLine("#line default");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>The file's own <c>@using</c> targets plus the inherited ones, de-duplicated in order.</summary>
    private static IEnumerable<string> EnumerateUsings(string razorText, IReadOnlyList<string>? inherited)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var u in inherited ?? [])
            if (seen.Add(u)) yield return u;
        foreach (Match m in UsingDirective.Matches(razorText))
            if (seen.Add(m.Groups["u"].Value)) yield return m.Groups["u"].Value;
    }

    /// <summary>One extracted <c>@code</c>/<c>@functions</c> block: the content span between (not
    /// including) its braces, and the 1-based razor line the content starts on (for <c>#line</c>).</summary>
    internal readonly record struct CodeBlock(int ContentStart, int ContentEnd, int StartLine);

    /// <summary>Finds every <c>@code {…}</c> / <c>@functions {…}</c> block by balanced-brace scan.
    /// The scanner is string/char/comment-aware so a <c>}</c> inside a literal doesn't end the block;
    /// an unbalanced block (scanner reached EOF) is skipped whole — honest omission over swallowing
    /// trailing markup as C#.</summary>
    internal static List<CodeBlock> ExtractCodeBlocks(string text)
    {
        var blocks = new List<CodeBlock>();
        var i = 0;
        while (i < text.Length)
        {
            var at = text.IndexOf('@', i);
            if (at < 0) break;

            var keywordEnd = MatchDirectiveKeyword(text, at);
            if (keywordEnd < 0) { i = at + 1; continue; }

            var brace = SkipWhitespace(text, keywordEnd);
            if (brace >= text.Length || text[brace] != '{') { i = keywordEnd; continue; }

            var contentStart = brace + 1;
            var contentEnd = FindBlockEnd(text, contentStart);
            if (contentEnd < 0) { i = keywordEnd; continue; } // unbalanced — skip this block

            blocks.Add(new CodeBlock(contentStart, contentEnd, CountLines(text, contentStart)));
            i = contentEnd + 1;
        }
        return blocks;
    }

    /// <summary>When <paramref name="at"/> starts an <c>@code</c>/<c>@functions</c> directive (not an
    /// email-like <c>x@code</c>), returns the index after the keyword; else -1.</summary>
    private static int MatchDirectiveKeyword(string text, int at)
    {
        if (at > 0 && (char.IsLetterOrDigit(text[at - 1]) || text[at - 1] == '_')) return -1;
        foreach (var keyword in (ReadOnlySpan<string>)["code", "functions"])
        {
            var end = at + 1 + keyword.Length;
            if (end <= text.Length
                && text.AsSpan(at + 1, keyword.Length).SequenceEqual(keyword)
                && (end == text.Length || !char.IsLetterOrDigit(text[end])))
                return end;
        }
        return -1;
    }

    private static int SkipWhitespace(string text, int i)
    {
        while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
        return i;
    }

    /// <summary>Scans from just inside the opening brace to the matching close brace, skipping
    /// string/char literals (incl. verbatim, interpolated-as-plain, and raw <c>"""</c>) and comments.
    /// Returns the index of the closing brace, or -1 when unbalanced.</summary>
    private static int FindBlockEnd(string text, int contentStart)
    {
        var depth = 1;
        var i = contentStart;
        while (i < text.Length)
        {
            var c = text[i];
            switch (c)
            {
                case '{': depth++; i++; break;
                case '}':
                    depth--;
                    if (depth == 0) return i;
                    i++;
                    break;
                case '/':
                    if (i + 1 < text.Length && text[i + 1] == '/') i = SkipLineComment(text, i);
                    else if (i + 1 < text.Length && text[i + 1] == '*') i = SkipBlockComment(text, i);
                    else i++;
                    break;
                case '\'': i = SkipCharLiteral(text, i); break;
                case '"': i = SkipStringLiteral(text, i, verbatim: false); break;
                case '@':
                    if (i + 1 < text.Length && text[i + 1] == '"') i = SkipStringLiteral(text, i + 1, verbatim: true);
                    else i++;
                    break;
                case '$':
                    // Interpolated strings scanned as plain strings: their {expr} holes are balanced,
                    // so ignoring the braces inside keeps the GLOBAL count balanced. ($@" / @$" land
                    // here or at '@' above; both skip to the terminating quote.)
                    if (i + 1 < text.Length && text[i + 1] == '"') i = SkipStringLiteral(text, i + 1, verbatim: false);
                    else if (i + 2 < text.Length && text[i + 1] == '@' && text[i + 2] == '"') i = SkipStringLiteral(text, i + 2, verbatim: true);
                    else i++;
                    break;
                default: i++; break;
            }
        }
        return -1;
    }

    private static int SkipLineComment(string text, int i)
    {
        while (i < text.Length && text[i] != '\n') i++;
        return i;
    }

    private static int SkipBlockComment(string text, int i)
    {
        i += 2;
        while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/')) i++;
        return Math.Min(i + 2, text.Length);
    }

    private static int SkipCharLiteral(string text, int i)
    {
        i++; // opening quote
        while (i < text.Length && text[i] != '\'')
            i += text[i] == '\\' ? 2 : 1;
        return Math.Min(i + 1, text.Length);
    }

    private static int SkipStringLiteral(string text, int i, bool verbatim)
    {
        // Raw string literal ("""…"""): skip to the matching run of quotes.
        if (!verbatim && i + 2 < text.Length && text[i + 1] == '"' && text[i + 2] == '"')
        {
            var open = 0;
            while (i + open < text.Length && text[i + open] == '"') open++;
            var close = text.IndexOf(new string('"', open), i + open, StringComparison.Ordinal);
            return close < 0 ? text.Length : close + open;
        }

        i++; // opening quote
        while (i < text.Length)
        {
            if (verbatim)
            {
                if (text[i] == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { i += 2; continue; } // "" escape
                    return i + 1;
                }
                i++;
            }
            else
            {
                if (text[i] == '\\') { i += 2; continue; }
                if (text[i] == '"' || text[i] == '\n') return i + 1; // non-verbatim can't span lines
                i++;
            }
        }
        return text.Length;
    }

    /// <summary>1-based line number of <paramref name="index"/> in <paramref name="text"/>.</summary>
    private static int CountLines(string text, int index)
    {
        var line = 1;
        for (var i = 0; i < index && i < text.Length; i++)
            if (text[i] == '\n') line++;
        return line;
    }

    /// <summary>Resolves a component's namespace the way Blazor does: own <c>@namespace</c> →
    /// nearest ancestor <c>_Imports.razor</c> <c>@namespace</c> (plus folders below it) → owning
    /// project's RootNamespace (csproj name default) plus folders below the project dir.</summary>
    internal sealed class NamespaceIndex
    {
        private readonly List<(string Dir, string RootNs)> _projectDirs;   // longest dir first
        private readonly Dictionary<string, string> _importsNsByDir;
        private readonly Dictionary<string, List<string>> _importsUsingsByDir;

        private NamespaceIndex(List<(string, string)> projectDirs, Dictionary<string, string> importsNsByDir,
            Dictionary<string, List<string>> importsUsingsByDir)
        {
            _projectDirs = projectDirs;
            _importsNsByDir = importsNsByDir;
            _importsUsingsByDir = importsUsingsByDir;
        }

        public static async Task<NamespaceIndex> BuildAsync(DiscoveryContext context, CancellationToken ct)
        {
            var projectDirs = new List<(string, string)>();
            foreach (var csproj in context.Analysis.AllProjectFiles)
            {
                ct.ThrowIfCancellationRequested();
                var dir = Path.GetDirectoryName(csproj);
                if (dir is null) continue;
                var rootNs = Path.GetFileNameWithoutExtension(csproj);
                var m = RootNamespaceProperty.Match(await context.Cache.GetTextAsync(csproj, ct));
                if (m.Success) rootNs = m.Groups[1].Value;
                projectDirs.Add((dir, rootNs));
            }
            projectDirs.Sort((a, b) => b.Item1.Length.CompareTo(a.Item1.Length));

            var importsNs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var importsUsings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in context.Analysis.AllContentFiles)
            {
                if (!Path.GetFileName(file).Equals("_Imports.razor", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (Path.GetDirectoryName(file) is not { } dir) continue;
                var text = await context.Cache.GetTextAsync(file, ct);
                var m = NamespaceDirective.Match(text);
                if (m.Success) importsNs[dir] = m.Groups["ns"].Value;
                var usings = UsingDirective.Matches(text).Select(u => u.Groups["u"].Value).ToList();
                if (usings.Count > 0) importsUsings[dir] = usings;
            }
            return new NamespaceIndex(projectDirs, importsNs, importsUsings);
        }

        /// <summary>All <c>@using</c> targets inherited from ancestor <c>_Imports.razor</c> files
        /// (outermost first — Blazor's cumulative import scoping).</summary>
        public IReadOnlyList<string> InheritedUsings(string razorPath)
        {
            var chain = new List<string>();
            for (var dir = Path.GetDirectoryName(razorPath); dir is not null; dir = Path.GetDirectoryName(dir))
            {
                if (_importsUsingsByDir.ContainsKey(dir)) chain.Add(dir);
                if (IsProjectDir(dir)) break;
            }
            var result = new List<string>();
            for (var i = chain.Count - 1; i >= 0; i--)
                result.AddRange(_importsUsingsByDir[chain[i]]);
            return result;
        }

        private bool IsProjectDir(string dir)
            => _projectDirs.Any(p => p.Dir.Equals(dir, StringComparison.OrdinalIgnoreCase));

        public string? Resolve(string razorPath, string razorText)
        {
            var own = NamespaceDirective.Match(razorText);
            if (own.Success) return own.Groups["ns"].Value;

            var fileDir = Path.GetDirectoryName(razorPath);
            if (fileDir is null) return null;

            var (projectDir, rootNs) = _projectDirs.FirstOrDefault(p =>
                fileDir.StartsWith(p.Dir, StringComparison.OrdinalIgnoreCase)
                && (fileDir.Length == p.Dir.Length || fileDir[p.Dir.Length] is '\\' or '/'));

            // Nearest ancestor _Imports @namespace wins over the project root, folders below it appended.
            for (var dir = fileDir; dir is not null; dir = Path.GetDirectoryName(dir))
            {
                if (_importsNsByDir.TryGetValue(dir, out var ns))
                    return AppendFolders(ns, dir, fileDir);
                if (projectDir is not null && dir.Equals(projectDir, StringComparison.OrdinalIgnoreCase))
                    break;
            }

            return projectDir is null ? null : AppendFolders(rootNs, projectDir, fileDir);
        }

        private static string AppendFolders(string baseNs, string baseDir, string fileDir)
        {
            if (fileDir.Length <= baseDir.Length) return baseNs;
            var sb = new StringBuilder(baseNs);
            foreach (var segment in fileDir[baseDir.Length..].Split('\\', '/', StringSplitOptions.RemoveEmptyEntries))
                sb.Append('.').Append(SanitizeSegment(segment));
            return sb.ToString();
        }

        /// <summary>Folder → namespace segment, Blazor-style: invalid identifier chars become '_',
        /// a leading digit gets a '_' prefix.</summary>
        private static string SanitizeSegment(string segment)
        {
            var sb = new StringBuilder(segment.Length + 1);
            if (segment.Length > 0 && char.IsDigit(segment[0])) sb.Append('_');
            foreach (var c in segment)
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
            return sb.ToString();
        }
    }
}
