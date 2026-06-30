namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects gRPC service implementations — classes extending gRPC-generated
/// <c>XxxBase</c> base classes. Produces <see cref="GrpcServiceDetection"/> entries.</summary>
[ExtractorOrder(58)]
public sealed class GrpcServiceExtractor : IDiscoveryExtractor
{
    public string Name => "GrpcServiceExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Grpc],
        ["grpc-service-detections"],
        ["model.Detections"],
        "Scans syntax trees for gRPC service base subclasses (XxxBase pattern)");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Grpc);

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

                foreach (var bt in baseList.Types)
                {
                    var baseName = bt.Type.ToString();
                    // gRPC generated base: Xxx.XxxBase
                    if (!baseName.EndsWith("Base", StringComparison.Ordinal)) continue;

                    var className = classDecl.Identifier.ValueText;
                    var dot = baseName.LastIndexOf('.');
                    var serviceName = dot >= 0 ? baseName[..dot] : baseName[..^4];

                    // Collect methods as gRPC service methods
                    var methods = new List<string>();
                    foreach (var method in classDecl.Members
                        .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                    {
                        methods.Add(method.Identifier.ValueText);
                    }

                    model.Detections.Add(new GrpcServiceDetection(
                        serviceName, className, methods.ToImmutableArray())
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        Confidence = 0.85f,
                    });
                    break; // one base match per class
                }
            }
        }
    }
}
