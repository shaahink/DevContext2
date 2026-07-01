namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects AWS Lambda function handlers — classes with <c>[LambdaFunction]</c>
/// attribute or implementing <c>ILambdaFunction</c>. Produces <see cref="FunctionEntryDetection"/> entries.</summary>
[ExtractorOrder(63)]
public sealed class AwsLambdaExtractor : IDiscoveryExtractor
{
    public string Name => "AwsLambdaExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.AwsLambda],
        ["aws-lambda-detections"],
        ["model.Detections"],
        "Scans for ILambdaFunction implementations and [LambdaFunction] attributes");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.AwsLambda);

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

                // Check for [LambdaFunction] or [Function] attribute
                var triggers = new List<string>();
                foreach (var attrList in classDecl.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        var attrName = attr.Name.ToString();
                        var dot = attrName.LastIndexOf('.');
                        var simple = dot >= 0 ? attrName[(dot + 1)..] : attrName;
                        if (simple is "LambdaFunction" or "FunctionHandler")
                            triggers.Add(simple);
                    }
                }

                // Check for ILambdaFunction base
                var baseList = classDecl.BaseList;
                var hasLambdaBase = false;
                if (baseList is not null)
                {
                    foreach (var bt in baseList.Types)
                    {
                        if (bt.Type.ToString().Contains("ILambdaFunction"))
                        {
                            hasLambdaBase = true;
                            triggers.Add("ILambdaFunction");
                        }
                    }
                }

                if (triggers.Count == 0 && !hasLambdaBase) continue;

                // Find public method (FunctionHandler or Handle)
                var handlerMethod = "FunctionHandler";
                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    if (method.Identifier.ValueText is "FunctionHandler" or "Handle" or "Run")
                    {
                        handlerMethod = method.Identifier.ValueText;
                        break;
                    }
                }

                model.Detections.Add(new FunctionEntryDetection(
                    className, handlerMethod, triggers.ToImmutableArray())
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.85f,
                });
            }
        }
    }
}
