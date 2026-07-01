namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects HotChocolate GraphQL resolver types — classes decorated with
/// <c>[QueryType]</c>/<c>[MutationType]</c>/<c>[SubscriptionType]</c> or implementing <c>ObjectType&lt;T&gt;</c>.
/// Produces <see cref="GraphQlFieldDetection"/> entries.</summary>
[ExtractorOrder(64)]
public sealed class GraphQlResolverExtractor : IDiscoveryExtractor
{
    public string Name => "GraphQlResolverExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.GraphQL],
        ["graphql-resolver-detections"],
        ["model.Detections"],
        "Scans for HotChocolate [QueryType]/[MutationType] and ObjectType<T>");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.GraphQL);

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
                var className = classDecl.Identifier.ValueText;

                // Check for [QueryType] / [MutationType] / [SubscriptionType]
                string? opType = null;
                foreach (var attrList in classDecl.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        var name = attr.Name.ToString();
                        var dot = name.LastIndexOf('.');
                        var simple = dot >= 0 ? name[(dot + 1)..] : name;
                        if (simple is "QueryType" or "MutationType" or "SubscriptionType"
                            or "ExtendObjectType" or "Query" or "Mutation")
                        {
                            opType = simple.Replace("Type", "").Replace("ExtendObject", "");
                        }
                    }
                }

                // Check for ObjectType<T> base
                var baseList = classDecl.BaseList;
                if (baseList is not null)
                {
                    foreach (var bt in baseList.Types)
                    {
                        if (bt.Type.ToString().StartsWith("ObjectType<"))
                        {
                            opType ??= "Object";
                            break;
                        }
                    }
                }

                if (opType is null) continue;

                // Collect public resolver methods
                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    var methodName = method.Identifier.ValueText;
                    if (methodName == "Configure") continue;
                    model.Detections.Add(new GraphQlFieldDetection(
                        className, methodName, opType)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        Confidence = 0.85f,
                    });
                }

                if (!classDecl.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                    .Any(m => m.Identifier.ValueText != "Configure"))
                {
                    model.Detections.Add(new GraphQlFieldDetection(
                        className, className, opType)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        Confidence = 0.8f,
                    });
                }
            }
        }
    }
}
