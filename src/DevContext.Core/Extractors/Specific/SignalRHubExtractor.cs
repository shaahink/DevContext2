namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects SignalR hub classes — classes extending <c>Hub</c> or <c>Hub&lt;T&gt;</c>.
/// Collects public hub methods as entry-point methods. Produces <see cref="SignalRHubDetection"/> entries.</summary>
[ExtractorOrder(57)]
public sealed class SignalRHubExtractor : IDiscoveryExtractor
{
    public string Name => "SignalRHubExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.SignalR],
        ["signalr-hub-detections"],
        ["model.Detections"],
        "Scans syntax trees for Hub/Hub<T> subclasses and their public methods");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.SignalR);

    /// <summary>B1 (Prism D1.2a): matches SignalR hub base names AS WRITTEN — bare ("Hub", "Hub&lt;T&gt;")
    /// or namespace-qualified ("Microsoft.AspNetCore.SignalR.Hub[&lt;T&gt;]", the form BOTH dotnet-podcasts'
    /// ListenTogetherHub and bitwarden's NotificationsHub use; the audit's in-framework hubs were missed
    /// on exactly this). A qualified base is trusted only when the qualifier ends with "SignalR" — an
    /// arbitrary Foo.Hub type is not SignalR.</summary>
    internal static bool IsHubBaseName(string name)
    {
        if (name == "Hub" || name.StartsWith("Hub<", StringComparison.Ordinal)) return true;
        var idx = name.LastIndexOf(".Hub", StringComparison.Ordinal);
        if (idx < 0) return false;
        var after = idx + 4;
        if (after != name.Length && name[after] != '<') return false;
        return name.AsSpan(0, idx).EndsWith("SignalR", StringComparison.Ordinal);
    }

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

            Microsoft.CodeAnalysis.SyntaxTree syntaxTree;
            try { syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct); }
            catch { continue; }

            var root = await syntaxTree.GetRootAsync(ct);

            foreach (var classDecl in root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();
                var baseList = classDecl.BaseList;
                if (baseList is null) continue;

                // Check for a Hub base — bare or SignalR-qualified (B1, Prism D1.2a).
                var isHub = false;
                foreach (var bt in baseList.Types)
                {
                    if (IsHubBaseName(bt.Type.ToString()))
                    {
                        isHub = true;
                        break;
                    }
                }
                if (!isHub) continue;

                var className = classDecl.Identifier.ValueText;
                var methods = new List<string>();
                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    methods.Add(method.Identifier.ValueText);
                }

                model.Detections.Add(new SignalRHubDetection(className, methods.ToImmutableArray())
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.9f,
                });
            }
        }
    }
}
