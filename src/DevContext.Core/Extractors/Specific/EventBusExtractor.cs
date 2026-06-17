using Microsoft.CodeAnalysis;
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
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MassTransit, ArchitectureSignals.Keys.NServiceBus],
        ["message-consumer-detections"],
        ["model.Detections"],
        "Walks syntax trees to detect message bus consumers and bus registrations");
    /// <summary>Only runs when MassTransit or NServiceBus signals are detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MassTransit)
            || currentModel.Architecture.Has(ArchitectureSignals.Keys.NServiceBus);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var busKind = DetectBusKind(model);

        foreach (var filePath in context.Analysis.AllSourceFiles)
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

                    if (typeName.StartsWith("IConsumer<", StringComparison.Ordinal))
                    {
                        var args = GenericArgumentParser.ExtractGenericArguments(typeName);
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

        await DetectBusRegistrationPatterns(context, model, busKind, Name, ct).ConfigureAwait(false);
    }

    private static string DetectBusKind(DiscoveryModel model)
    {
        if (model.Architecture.Has(ArchitectureSignals.Keys.MassTransit))
            return "MassTransit";

        if (model.Architecture.Has(ArchitectureSignals.Keys.NServiceBus))
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
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
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

                var isBusRegistration = string.Equals(busKind, "MassTransit"
, StringComparison.Ordinal) ? methodName is "AddMassTransit" or "UsingRabbitMq" or "UsingAzureServiceBus" or "AddConsumer" or "AddMediator"
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
}
