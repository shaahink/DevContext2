using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>An Aspire resource declared in an AppHost. <see cref="ResourceName"/> is the runtime name
/// ("basket-api") — the name service-discovery addresses use. <see cref="VariableName"/> is the local
/// the resource builder was assigned to, which is how the AppHost refers to it in later
/// <c>WithReference</c> calls. <see cref="ProjectRef"/> carries the <c>AddProject&lt;Projects.X&gt;</c>
/// type argument (Batch B: the bridge from "basket-api" to the analyzed project Basket.API).</summary>
internal sealed record AspireResourceDetection(
    string ResourceType,
    string ResourceName,
    string? Relationship,
    string? VariableName = null,
    string? ProjectRef = null
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
         "AddKafka", "AddMilvus", "AddQdrant", "AddWeaviate", "AddNeo4j",
         // Batch B: databases hang off a server resource (postgres.AddDatabase("catalogdb")) and are
         // what services actually reference; the cache/storage aliases are the same shape.
         "AddDatabase", "AddValkey", "AddGarnet", "AddNats", "AddAzureStorage"];

    /// <summary>Resource methods that name an analyzed PROJECT rather than infrastructure.</summary>
    private const string ProjectResourceMethod = "AddProject";

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
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            // Batch B pass 1 — locals that stand for a resource. `var identityEndpoint =
            // identityApi.GetEndpoint(profile)` makes identityEndpoint mean identityApi at every later
            // reference; without this the AppHost's WithEnvironment wiring resolves to nothing.
            var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var declarator in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                if (declarator.Initializer?.Value is not { } initializer) continue;
                if (ResolveChainRoot(initializer) is not { } chainRoot) continue;
                var local = declarator.Identifier.ValueText;
                if (!string.Equals(chainRoot, local, StringComparison.Ordinal))
                    aliases[local] = chainRoot;
            }

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                ct.ThrowIfCancellationRequested();

                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess == null) continue;

                var methodName = memberAccess.Name.Identifier.ValueText;

                if (ResourceMethods.Contains(methodName))
                {
                    var resourceType = methodName[3..];
                    var resourceName = ExtractResourceName(invocation);
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    model.Detections.Add(new AspireResourceDetection(
                        ResourceType: resourceType,
                        ResourceName: resourceName,
                        // A nested resource records its host: postgres.AddDatabase("catalogdb").
                        // For a top-level resource this is just the builder local — the graph joins
                        // only names it already knows as resources, so that resolves to nothing.
                        Relationship: Dealias(ResolveChainRoot(memberAccess.Expression), aliases),
                        VariableName: FindAssignedVariable(invocation),
                        ProjectRef: methodName == ProjectResourceMethod
                            ? ExtractProjectRef(memberAccess, invocation)
                            : null)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }

                if (RelationshipMethods.Contains(methodName))
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    var args = invocation.ArgumentList.Arguments;

                    // The SOURCE is the resource the call is chained onto, never an argument:
                    // `basketApi.WithReference(redis)` names only the target in its arguments. The old
                    // reading took args[0] as the source and args[1] as the target, so every
                    // relationship pointed at "?" — which is why this vocabulary stayed dead.
                    var source = Dealias(ResolveChainRoot(memberAccess.Expression), aliases);
                    var targetExpression = methodName == "WithEnvironment" && args.Count > 1
                        ? args[1].Expression                      // WithEnvironment("Identity__Url", identityEndpoint)
                        : args.Count > 0 ? args[0].Expression : null;
                    var target = targetExpression is null
                        ? null
                        : Dealias(ResolveChainRoot(targetExpression), aliases);

                    if (source is null || target is null
                        || string.Equals(source, target, StringComparison.Ordinal)) continue;

                    model.Detections.Add(new AspireRelationshipDetection(
                        SourceResource: source,
                        TargetResource: target,
                        RelationshipType: methodName)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
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

    /// <summary>Walks a fluent chain back to the resource it started from, returning that resource's
    /// name (<c>builder.AddProject&lt;T&gt;("basket-api").WithReference(x)</c> → "basket-api") or the
    /// local it is rooted at (<c>basketApi.WithReference(x)</c> → "basketApi"). Null when the chain
    /// starts at something that is neither.</summary>
    private static string? ResolveChainRoot(ExpressionSyntax? expression)
    {
        var current = expression;
        while (current is not null)
        {
            switch (current)
            {
                case InvocationExpressionSyntax invocation:
                    if (invocation.Expression is not MemberAccessExpressionSyntax invocationAccess) return null;
                    if (ResourceMethods.Contains(invocationAccess.Name.Identifier.ValueText))
                        return ExtractResourceName(invocation);
                    current = invocationAccess.Expression;
                    continue;
                case MemberAccessExpressionSyntax access:
                    current = access.Expression;
                    continue;
                case IdentifierNameSyntax identifier:
                    return identifier.Identifier.ValueText;
                default:
                    return null;
            }
        }
        return null;
    }

    /// <summary>Follows local aliases to the underlying resource, with a hop cap so a self-referential
    /// chain can never spin.</summary>
    private static string? Dealias(string? name, Dictionary<string, string> aliases)
    {
        var current = name;
        for (var hops = 0; current is not null && hops < 4; hops++)
        {
            if (!aliases.TryGetValue(current, out var next)) return current;
            current = next;
        }
        return current;
    }

    /// <summary>The local a resource builder was assigned to, which is how later AppHost lines name it.</summary>
    private static string? FindAssignedVariable(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            switch (current)
            {
                case VariableDeclaratorSyntax declarator: return declarator.Identifier.ValueText;
                case AssignmentExpressionSyntax { Left: IdentifierNameSyntax left }: return left.Identifier.ValueText;
                case LambdaExpressionSyntax: return null;
                case StatementSyntax: return null;
            }
        }
        return null;
    }

    /// <summary>The project an <c>AddProject</c> resource stands for. Either the
    /// <c>AddProject&lt;Projects.Basket_API&gt;</c> type argument stripped to its leaf ("Basket_API" --
    /// the generated Projects.* names substitute '_' for the real separators, so consumers match it
    /// normalized rather than literally), or the csproj path of the non-generic
    /// <c>AddProject("web", "../Web/Web.csproj")</c> overload.</summary>
    private static string? ExtractProjectRef(MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation)
    {
        if (memberAccess.Name is GenericNameSyntax generic
            && generic.TypeArgumentList.Arguments.Count > 0)
        {
            var typeArgument = generic.TypeArgumentList.Arguments[0].ToString();
            var lastDot = typeArgument.LastIndexOf('.');
            return lastDot >= 0 ? typeArgument[(lastDot + 1)..] : typeArgument;
        }

        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count >= 2 && arguments[1].Expression is LiteralExpressionSyntax literal
            && literal.Token.Value is string path
            && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        return null;
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
