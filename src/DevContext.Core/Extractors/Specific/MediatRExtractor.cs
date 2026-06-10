using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects MediatR handlers and marker interfaces (IRequest, ICommand, IQuery) via syntax tree analysis.</summary>
[ExtractorOrder(20)]
public sealed class MediatRExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> RequestMarkers =
        ["IRequest", "ICommand", "IQuery"];

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "MediatRExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MediatR], ["mediatr-handler-detections"],
        ["model.Detections"],
        "Walks syntax trees to detect MediatR handlers and marker interfaces");
    /// <summary>Only runs when the MediatR signal has been detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MediatR);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
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
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classes)
            {
                var handlerType = classDecl.Identifier.ValueText;
                var baseTypes = classDecl.BaseList?.Types;

                if (baseTypes == null) continue;

                foreach (var baseType in baseTypes)
                {
                    var typeName = baseType.Type.ToString();

                    var match = TryParseHandlerType(typeName);
                    if (match == null) continue;

                    var lineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    var (requestType, responseType, kind) = match.Value;

                    var detection = new MediatRHandlerDetection(
                        RequestType: requestType,
                        ResponseType: responseType,
                        HandlerType: handlerType,
                        Kind: kind)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    };

                    model.Detections.Add(detection);
                }
            }
        }
    }

    private static (string RequestType, string ResponseType, MediatRKind Kind)? TryParseHandlerType(string typeName)
    {
        if (typeName.StartsWith("IRequestHandler<"))
        {
            var args = ExtractGenericArguments(typeName);
            if (args.Length >= 2)
            {
                return (args[0], args[1], MediatRKind.Command);
            }
        }

        if (typeName.StartsWith("INotificationHandler<"))
        {
            var args = ExtractGenericArguments(typeName);
            if (args.Length >= 1)
            {
                return (args[0], "Unit", MediatRKind.Notification);
            }
        }

        if (typeName is "IRequest" or "ICommand" or "IQuery")
        {
            return ("<self>", "Unit", MediatRKind.Command);
        }

        var baseName = ExtractGenericBaseName(typeName);
        if (baseName != null && RequestMarkers.Contains(baseName))
        {
            var args = ExtractGenericArguments(typeName);
            if (args.Length == 1)
            {
                var kind = baseName switch
                {
                    "ICommand" => MediatRKind.Command,
                    "IQuery" => MediatRKind.Query,
                    _ => MediatRKind.Command,
                };
                return (args[0], "Unit", kind);
            }
            if (args.Length == 2)
            {
                var kind = baseName switch
                {
                    "ICommand" => MediatRKind.Command,
                    "IQuery" => MediatRKind.Query,
                    _ => MediatRKind.Command,
                };
                return (args[0], args[1], kind);
            }
        }

        return null;
    }

    private static string[] ExtractGenericArguments(string typeName)
    {
        var openBracket = typeName.IndexOf('<');
        if (openBracket < 0) return [];

        var closeBracket = typeName.LastIndexOf('>');
        if (closeBracket <= openBracket) return [];

        var inner = typeName.Substring(openBracket + 1, closeBracket - openBracket - 1);
        return SplitGenericArgs(inner);
    }

    private static string? ExtractGenericBaseName(string typeName)
    {
        var openBracket = typeName.IndexOf('<');
        return openBracket < 0 ? typeName : typeName[..openBracket];
    }

    private static string[] SplitGenericArgs(string args)
    {
        var depth = 0;
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var ch in args)
        {
            switch (ch)
            {
                case '<':
                    depth++;
                    current.Append(ch);
                    break;
                case '>':
                    depth--;
                    current.Append(ch);
                    break;
                case ',' when depth == 0:
                    parts.Add(current.ToString().Trim());
                    current.Clear();
                    break;
                default:
                    current.Append(ch);
                    break;
            }
        }

        if (current.Length > 0)
            parts.Add(current.ToString().Trim());

        return [.. parts];
    }
}
