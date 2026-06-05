using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects message bus consumers and bus registration patterns (MassTransit, NServiceBus).</summary>
[ExtractorOrder(40)]
public sealed class EventBusExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "EventBusExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MassTransit, "nservicebus"],
        ["message-consumer-detections"],
        ["model.Detections"],
        "Walks syntax trees to detect message bus consumers and bus registrations");
    /// <summary>Only runs when MassTransit or NServiceBus signals are detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MassTransit)
            || currentModel.Architecture.Has("nservicebus");

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var busKind = DetectBusKind(model);

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
                ct.ThrowIfCancellationRequested();

                var consumerType = classDecl.Identifier.ValueText;
                var baseTypes = classDecl.BaseList?.Types;

                if (baseTypes == null) continue;

                foreach (var baseType in baseTypes)
                {
                    var typeName = baseType.Type.ToString();

                    if (typeName.StartsWith("IConsumer<"))
                    {
                        var args = ExtractGenericArguments(typeName);
                        if (args.Length == 0) continue;

                        var messageType = args[0];
                        var lineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                        model.Detections.Add(new MessageConsumerDetection(
                            MessageType: messageType,
                            ConsumerType: consumerType,
                            BusKind: busKind)
                        {
                            ExtractorName = Name,
                            SourceFile = filePath,
                            LineNumber = lineNumber,
                        });
                    }
                }
            }
        }

        await DetectBusRegistrationPatterns(context, model, busKind, Name, ct);
    }

    private static string DetectBusKind(DiscoveryModel model)
    {
        if (model.Architecture.Has(ArchitectureSignals.Keys.MassTransit))
            return "MassTransit";

        if (model.Architecture.Has("nservicebus"))
            return "NServiceBus";

        return "Unknown";
    }

    private static async ValueTask DetectBusRegistrationPatterns(
        DiscoveryContext context,
        DiscoveryModel model,
        string busKind,
        string extractorName,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

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
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess == null) continue;

                var methodName = memberAccess.Name.Identifier.ValueText;

                var isBusRegistration = busKind == "MassTransit"
                    ? methodName is "AddMassTransit" or "UsingRabbitMq" or "UsingAzureServiceBus" or "AddConsumer" or "AddMediator"
                    : methodName is "AddNServiceBus" or "ConfigureEndpoint" or "AddEndpoint";

                if (!isBusRegistration) continue;

                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                model.Detections.Add(new MessageConsumerDetection(
                    MessageType: "<registration>",
                    ConsumerType: methodName,
                    BusKind: busKind)
                {
                    ExtractorName = extractorName,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                    Confidence = 0.7f,
                });
            }
        }
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
