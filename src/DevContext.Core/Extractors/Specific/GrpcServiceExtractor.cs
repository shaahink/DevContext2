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
                    // T1.7 — a gRPC generated service base is ALWAYS the nested `Service.ServiceBase`
                    // form protoc emits (Greeter.GreeterBase, Basket.BasketBase,
                    // DiscountProtoService.DiscountProtoServiceBase). The old rule — any type ending in
                    // "Base" — matched MAUI/MVVM `ViewModelBase`, so every eShop ClientApp ViewModel was
                    // catalogued as a gRPC service (7 `InitializeAsync` RPCs in the deck). Require the
                    // `X.XBase` nesting: the last segment must equal the outer type's simple name + "Base".
                    if (!baseName.EndsWith("Base", StringComparison.Ordinal)) continue;
                    var dot = baseName.LastIndexOf('.');
                    if (dot < 0) continue;                       // not nested → not a generated gRPC base
                    var baseSimple = baseName[(dot + 1)..];      // "GreeterBase"
                    var outer = baseName[..dot];                 // "Greeter" (possibly namespace-qualified)
                    var outerDot = outer.LastIndexOf('.');
                    var outerSimple = outerDot >= 0 ? outer[(outerDot + 1)..] : outer;  // "Greeter"
                    if (!baseSimple.Equals(outerSimple + "Base", StringComparison.Ordinal)) continue;

                    var className = classDecl.Identifier.ValueText;
                    var serviceName = outerSimple;

                    // T1.7 — gRPC entries are proto RPCs only: the generated XxxBase declares each RPC
                    // as `public virtual`, so an implementation overrides it with `public override`.
                    // Private helpers (ThrowNotAuthenticated, MapToCustomerBasket*) are NOT RPCs — they
                    // were inflating the "gRPC 7" facet with 4 BasketService helpers. Filter to overrides.
                    var methods = new List<string>();
                    foreach (var method in classDecl.Members
                        .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                    {
                        var mods = method.Modifiers;
                        var isRpcOverride =
                            mods.Any(m => m.ValueText == "override") &&
                            mods.Any(m => m.ValueText == "public");
                        if (isRpcOverride)
                            methods.Add(method.Identifier.ValueText);
                    }

                    model.Detections.Add(new GrpcServiceDetection(
                        serviceName, className, methods.ToImmutableArray())
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    });
                    break; // one base match per class
                }
            }
        }
    }
}
