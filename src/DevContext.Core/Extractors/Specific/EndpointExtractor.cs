using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects minimal API endpoint registrations (MapGet, MapPost, etc.) via syntax tree analysis.</summary>
[ExtractorOrder(10)]
public sealed class EndpointExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> MapMethods = ["MapGet", "MapPost", "MapPut", "MapDelete", "MapPatch"];

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "EndpointExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MinimalApis], ["endpoint-detections"],
        ["model.Detections"],
        "Walks syntax trees to detect minimal API endpoint registrations");
    /// <summary>Only runs when the MinimalApis signal has been detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MinimalApis);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var programFiles = context.Analysis.AllSourceFiles
            .Where(f => Path.GetFileName(f).Equals("Program.cs", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var filePath in programFiles)
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
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess == null) continue;

                var methodName = memberAccess.Name.Identifier.ValueText;
                if (!MapMethods.Contains(methodName)) continue;

                var httpMethod = methodName switch
                {
                    "MapGet" => "GET",
                    "MapPost" => "POST",
                    "MapPut" => "PUT",
                    "MapDelete" => "DELETE",
                    "MapPatch" => "PATCH",
                    _ => "UNKNOWN",
                };

                var routeArg = invocation.ArgumentList.Arguments
                    .FirstOrDefault(a => a.Expression is LiteralExpressionSyntax)
                    ?.Expression as LiteralExpressionSyntax;
                var routeTemplate = routeArg?.Token.ValueText ?? "/";

                var handlerArg = invocation.ArgumentList.Arguments
                    .LastOrDefault()
                    ?.Expression;
                var handlerInfo = handlerArg?.ToString() ?? "?";

                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                model.Detections.Add(new EndpointDetection(
                    httpMethod,
                    routeTemplate,
                    handlerInfo,
                    "<lambda>",
                    [],
                    [])
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                });
            }
        }

        if (programFiles.Count == 0)
        {
            await foreach (var filePath in EnumerateSourceFilesAsync(context, ct))
            {
                ct.ThrowIfCancellationRequested();

                SyntaxTree syntaxTree;
                try
                {
                    syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
                }
                catch
                {
                    continue;
                }

                var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
                var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

                foreach (var invocation in invocations)
                {
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (memberAccess == null) continue;

                    var methodName = memberAccess.Name.Identifier.ValueText;
                    if (!MapMethods.Contains(methodName)) continue;

                    var httpMethod = methodName switch
                    {
                        "MapGet" => "GET",
                        "MapPost" => "POST",
                        "MapPut" => "PUT",
                        "MapDelete" => "DELETE",
                        "MapPatch" => "PATCH",
                        _ => "UNKNOWN",
                    };

                    var routeArg = invocation.ArgumentList.Arguments
                        .FirstOrDefault(a => a.Expression is LiteralExpressionSyntax)
                        ?.Expression as LiteralExpressionSyntax;
                    var routeTemplate = routeArg?.Token.ValueText ?? "/";

                    var handlerArg = invocation.ArgumentList.Arguments
                        .LastOrDefault()
                        ?.Expression;
                    var handlerInfo = handlerArg?.ToString() ?? "?";

                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    model.Detections.Add(new EndpointDetection(
                        httpMethod,
                        routeTemplate,
                        handlerInfo,
                        "<lambda>",
                        [],
                        [])
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }
            }
        }
    }

    private static async IAsyncEnumerable<string> EnumerateSourceFilesAsync(
        DiscoveryContext context, [EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var file in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            yield return file;
        }
    }
}
