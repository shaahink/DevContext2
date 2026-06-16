using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

internal sealed record AspireResourceDetection(
    string ResourceType,
    string ResourceName,
    string? Relationship
) : Detection;

internal sealed record AspireRelationshipDetection(
    string SourceResource,
    string TargetResource,
    string RelationshipType
) : Detection;

/// <summary>Detects .NET Aspire resource patterns (AddProject, AddRedis, etc.) and service relationships in AppHost projects.</summary>
[ExtractorOrder(60)]
public sealed class AspireExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> ResourceMethods =
        ["AddProject", "AddRedis", "AddPostgres", "AddSqlServer", "AddRabbitMQ",
         "AddAzureServiceBus", "AddCosmosDB", "AddMongoDB", "AddElasticsearch",
         "AddSeq", "AddKeycloak", "AddMySql", "AddMariaDB", "AddOracle",
         "AddKafka", "AddMilvus", "AddQdrant", "AddWeaviate", "AddNeo4j"];

    private static readonly ImmutableArray<string> RelationshipMethods =
        ["WithReference", "WithEnvironment", "DependsOn"];

    private static readonly ImmutableArray<string> AspireProjectFiles =
        ["AppHost", "Aspire"];

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "AspireExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Aspire], ["aspire-resource-detections"],
        ["model.Detections"],
        "Walks AppHost project files to detect Aspire resource patterns and service relationships");
    /// <summary>Only runs when the Aspire signal has been detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Aspire);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var appHostFiles = context.Analysis.AllSourceFiles
            .Where(f => AspireProjectFiles.Any(p =>
                f.Contains(p, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var filePath in appHostFiles)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                ct.ThrowIfCancellationRequested();

                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess == null) continue;

                var methodName = memberAccess.Name.Identifier.ValueText;

                if (ResourceMethods.Contains(methodName, StringComparer.Ordinal))
                {
                    var resourceType = methodName[3..];
                    var resourceName = ExtractResourceName(invocation);
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    model.Detections.Add(new AspireResourceDetection(
                        ResourceType: resourceType,
                        ResourceName: resourceName,
                        Relationship: null)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }

                if (RelationshipMethods.Contains(methodName, StringComparer.Ordinal))
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    var args = invocation.ArgumentList.Arguments;
                    var source = args.Count > 0 ? args[0].Expression.ToString() : "?";
                    var target = args.Count > 1 ? args[1].Expression.ToString() : "?";

                    model.Detections.Add(new AspireRelationshipDetection(
                        SourceResource: source,
                        TargetResource: target,
                        RelationshipType: methodName)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                        Confidence = 0.8f,
                    });
                }
            }
        }

        if (appHostFiles.Count == 0)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, Name,
                "No AppHost or Aspire project files found despite Aspire signal being set");
        }
    }

    private static string ExtractResourceName(InvocationExpressionSyntax invocation)
    {
        if (invocation.ArgumentList.Arguments.Count == 0) return "?";
        var firstArg = invocation.ArgumentList.Arguments[0].Expression;

        if (firstArg is LiteralExpressionSyntax lit)
            return lit.Token.ValueText;

        if (firstArg is IdentifierNameSyntax ins)
            return ins.Identifier.ValueText;

        return firstArg.ToString();
    }
}
