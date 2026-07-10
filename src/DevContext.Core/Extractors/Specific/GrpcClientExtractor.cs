using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects gRPC generated client type usage — constructor-injected types whose name
/// ends with <c>Client</c> and whose namespace matches a gRPC service namespace. Produces
/// <see cref="GrpcClientDetection"/> entries for cross-project ServiceLink joins (M1.7).</summary>
[ExtractorOrder(59)]
public sealed class GrpcClientExtractor : IDiscoveryExtractor
{
    public string Name => "GrpcClientExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Grpc],
        ["grpc-client-detections"],
        ["model.Detections"],
        "Scans syntax trees for gRPC generated client types (XxxClient pattern)");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Grpc)
            || currentModel.Architecture.Has(ArchitectureSignals.Keys.Gateway);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

            SyntaxTree syntaxTree;
            try { syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct); }
            catch { continue; }

            var root = await syntaxTree.GetRootAsync(ct);

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();

                // Collect constructor parameter lists: explicit ctors + primary constructor
                var allParams = new List<(ParameterSyntax param, string Origin)>();
                foreach (var ctor in classDecl.Members.OfType<ConstructorDeclarationSyntax>())
                    foreach (var p in ctor.ParameterList.Parameters)
                        allParams.Add((p, "ctor"));
                if (classDecl.ParameterList is not null)
                    foreach (var p in classDecl.ParameterList.Parameters)
                        allParams.Add((p, "primary"));

                foreach (var (param, origin) in allParams)
                {
                    var paramType = param.Type?.ToString();
                    if (paramType is null) continue;

                    // gRPC generated client: X.XClient (e.g. DiscountProtoService.DiscountProtoServiceClient)
                    if (!IsGrpcClientType(paramType)) continue;

                    var serviceName = ExtractServiceName(paramType);
                    var methods = CollectClientMethodCalls(root, param.Identifier.ValueText);

                    model.Detections.Add(new GrpcClientDetection(
                        paramType, serviceName, classDecl.Identifier.ValueText,
                        methods.ToImmutableArray())
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = param.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        Confidence = 0.85f,
                    });
                }
            }
        }
    }

    private static bool IsGrpcClientType(string typeName)
    {
        // gRPC generated client: X.XClient (e.g. DiscountProtoService.DiscountProtoServiceClient)
        if (typeName.EndsWith("Client", StringComparison.Ordinal) && typeName != "HttpClient")
        {
            var dot = typeName.LastIndexOf('.');
            var shortName = dot >= 0 ? typeName[(dot + 1)..] : typeName;
            // Must be a qualified name with a service prefix (not bare HttpClient)
            if (dot >= 0 && shortName.EndsWith("Client", StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static string ExtractServiceName(string fullTypeName)
    {
        // DiscountProtoService.DiscountProtoServiceClient → DiscountProtoService
        var dot = fullTypeName.LastIndexOf('.');
        if (dot < 0) return fullTypeName[..^6]; // strip "Client"
        var beforeClient = dot >= 0 ? fullTypeName[..dot] : fullTypeName;
        // If the name ends with "Service", the service name is that part
        var lastDot = beforeClient.LastIndexOf('.');
        var shortName = lastDot >= 0 ? beforeClient[(lastDot + 1)..] : beforeClient;
        return shortName;
    }

    private static List<string> CollectClientMethodCalls(SyntaxNode root, string paramName)
    {
        var methods = new List<string>();
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var expr = inv.Expression;
            if (expr is MemberAccessExpressionSyntax memberAccess)
            {
                var obj = memberAccess.Expression.ToString();
                if (obj == paramName || obj.StartsWith(paramName + ".", StringComparison.Ordinal))
                {
                    var method = memberAccess.Name.Identifier.ValueText;
                    if (!methods.Contains(method))
                        methods.Add(method);
                }
            }
        }
        return methods;
    }
}
