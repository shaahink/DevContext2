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
    /// <summary>Runs for MassTransit/NServiceBus, or self-activates when any discovered type implements
    /// an integration-event handler interface (eShop's custom RabbitMQ IEventBus pattern, which carries
    /// no MassTransit/NServiceBus signal) — so its Bus consumers reach the Map's Bus group (G3).
    /// B5 (Prism D1.2d): also runs when a raw queue-transport package is referenced (Azure Storage
    /// Queues / Azure Service Bus / RabbitMQ.Client) so queue seams become channel edges even in repos
    /// with no bus framework at all (podcasts' FeedsApi → feed-queue → Ingestion.Worker).</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MassTransit)
            || currentModel.Architecture.Has(ArchitectureSignals.Keys.NServiceBus)
            || currentModel.Types.Values.Any(ImplementsIntegrationEventHandler)
            || HasQueueTransportPackage(currentModel);

    /// <summary>True when any project references a raw queue-transport client package.</summary>
    private static bool HasQueueTransportPackage(DiscoveryModel model)
        => model.Projects.Any(p => p.PackageReferences.Any(pr =>
            pr.Name.Equals("Azure.Storage.Queues", StringComparison.OrdinalIgnoreCase)
            || pr.Name.Equals("Azure.Messaging.ServiceBus", StringComparison.OrdinalIgnoreCase)
            || pr.Name.Equals("RabbitMQ.Client", StringComparison.OrdinalIgnoreCase)));

    /// <summary>True when a type implements <c>IIntegrationEventHandler&lt;T&gt;</c> (eShop / generic
    /// integration-event bus pattern).</summary>
    private static bool ImplementsIntegrationEventHandler(TypeDiscovery type)
        => type.ImplementedInterfaces.Any(i =>
            i.StartsWith("IIntegrationEventHandler<", StringComparison.Ordinal)
            || i.Contains(".IIntegrationEventHandler<", StringComparison.Ordinal));

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

                    // MassTransit IConsumer<T> or a generic/eShop IIntegrationEventHandler<T> — both name
                    // the message in the first type argument and the handler as the declaring class.
                    var isConsumer = typeName.StartsWith("IConsumer<", StringComparison.Ordinal);
                    var isIntegrationHandler = typeName.StartsWith("IIntegrationEventHandler<", StringComparison.Ordinal);
                    if (isConsumer || isIntegrationHandler)
                    {
                        var args = ExtractGenericArguments(typeName);
                        if (args.Length == 0) continue;

                        var messageType = args[0];
                        var lineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                        model.Detections.Add(new MessageConsumerDetection(
                            MessageType: messageType,
                            ConsumerType: consumerType,
                            BusKind: isIntegrationHandler && busKind == "Unknown" ? "RabbitMQ" : busKind)
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
        await DetectQueueSeams(context, model, ct);
    }

    // ── B5 (Prism D1.2d): raw queue transports as [approx] channel seams ─────────────────────────
    //
    // Storage-Queue/Service-Bus/RabbitMQ senders and their hosted consumers carry a repo's real
    // cross-process path with NO bus framework to detect (podcasts: FeedsApi.CreateFeed →
    // Azure Storage Queue "feed-queue" → Ingestion.Worker; the audit's event board said "No events
    // detected"). Syntax-only: a send site publishes to a channel, a receive site consumes it, and
    // the channel name joins them. A type with BOTH directions on one transport is a bus LIBRARY
    // implementation (eShop's EventBusRabbitMQ), not an application seam — dropped whole, which is
    // what keeps eShop byte-identical.

    private sealed record QueueSite(string Transport, string ClassName, string File, string? Channel, int Line, bool IsSend);

    private static readonly ImmutableArray<string> StorageQueueSendVerbs = ["SendMessageAsync", "SendMessagesAsync"];
    private static readonly ImmutableArray<string> StorageQueueReceiveVerbs = ["ReceiveMessagesAsync", "ReceiveMessageAsync"];
    private static readonly ImmutableArray<string> ServiceBusReceiveVerbs = ["StartProcessingAsync"];
    private static readonly ImmutableArray<string> RabbitSendVerbs = ["BasicPublish", "BasicPublishAsync"];
    private static readonly ImmutableArray<string> RabbitReceiveVerbs = ["BasicConsume", "BasicConsumeAsync"];

    private static async ValueTask DetectQueueSeams(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        if (!HasQueueTransportPackage(model)) return;

        var sites = new List<QueueSite>();
        // Repo-wide `new QueueClient(conn, "name")` literals — when one distinct queue name exists,
        // it labels storage-queue sites whose own call carries no literal (podcasts registers the
        // client in DI; the send site never repeats the name).
        var storageQueueNames = new HashSet<string>(StringComparer.Ordinal);

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
                continue; // already logged by the consumer scan
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var fileText = root.ToString();
            var hasStorageQueue = fileText.Contains("QueueClient", StringComparison.Ordinal)
                && !fileText.Contains("ServiceBus", StringComparison.Ordinal);
            var hasServiceBus = fileText.Contains("ServiceBusSender", StringComparison.Ordinal)
                || fileText.Contains("ServiceBusProcessor", StringComparison.Ordinal)
                || fileText.Contains("ServiceBusClient", StringComparison.Ordinal);
            var hasRabbit = fileText.Contains("BasicPublish", StringComparison.Ordinal)
                || fileText.Contains("BasicConsume", StringComparison.Ordinal);
            if (!hasStorageQueue && !hasServiceBus && !hasRabbit) continue;

            foreach (var creation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                if (creation.Type.ToString() is not ("QueueClient")) continue;
                var nameArg = creation.ArgumentList?.Arguments.Skip(1).FirstOrDefault();
                if (nameArg?.Expression is LiteralExpressionSyntax lit)
                    storageQueueNames.Add(lit.Token.ValueText);
            }

            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var className = typeDecl.Identifier.ValueText;

                foreach (var inv in typeDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (inv.Expression is not MemberAccessExpressionSyntax ma) continue;
                    var verb = ma.Name.Identifier.ValueText;
                    var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    if (hasRabbit && RabbitSendVerbs.Contains(verb))
                        sites.Add(new QueueSite("RabbitMQ", className, filePath, LiteralArg(inv, 1), line, IsSend: true));
                    else if (hasRabbit && RabbitReceiveVerbs.Contains(verb))
                        sites.Add(new QueueSite("RabbitMQ", className, filePath, LiteralArg(inv, 0), line, IsSend: false));
                    else if (hasStorageQueue && StorageQueueSendVerbs.Contains(verb))
                        sites.Add(new QueueSite("AzureStorageQueue", className, filePath, null, line, IsSend: true));
                    else if (hasStorageQueue && StorageQueueReceiveVerbs.Contains(verb))
                        sites.Add(new QueueSite("AzureStorageQueue", className, filePath, null, line, IsSend: false));
                    else if (hasServiceBus && !hasStorageQueue && StorageQueueSendVerbs.Contains(verb))
                        sites.Add(new QueueSite("AzureServiceBus", className, filePath, null, line, IsSend: true));
                    else if (hasServiceBus && ServiceBusReceiveVerbs.Contains(verb))
                        sites.Add(new QueueSite("AzureServiceBus", className, filePath, null, line, IsSend: false));
                }

                // `processor.ProcessMessageAsync += Handler` — the Service Bus consumer wiring shape.
                if (hasServiceBus)
                {
                    foreach (var assign in typeDecl.DescendantNodes().OfType<AssignmentExpressionSyntax>())
                    {
                        if (assign.OperatorToken.ValueText == "+="
                            && assign.Left is MemberAccessExpressionSyntax pm
                            && pm.Name.Identifier.ValueText == "ProcessMessageAsync")
                        {
                            var line = assign.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                            sites.Add(new QueueSite("AzureServiceBus", className, filePath, null, line, IsSend: false));
                        }
                    }
                }
            }
        }

        EmitQueueSeams(model, sites, storageQueueNames);
    }

    /// <summary>Nth argument when it is a string literal, else null.</summary>
    private static string? LiteralArg(InvocationExpressionSyntax inv, int index)
        => inv.ArgumentList.Arguments.Count > index
            && inv.ArgumentList.Arguments[index].Expression is LiteralExpressionSyntax lit
            && lit.Token.Value is string s
            ? s : null;

    private static void EmitQueueSeams(DiscoveryModel model, List<QueueSite> sites, HashSet<string> storageQueueNames)
    {
        if (sites.Count == 0) return;

        // A type doing BOTH directions on one transport is the bus implementation itself, not a seam.
        var infraTypes = sites.GroupBy(s => (s.Transport, s.ClassName))
            .Where(g => g.Any(s => s.IsSend) && g.Any(s => !s.IsSend))
            .Select(g => g.Key)
            .ToHashSet();
        var seams = sites.Where(s => !infraTypes.Contains((s.Transport, s.ClassName))).ToList();

        // One repo-wide storage-queue name labels the channel when the site itself carries none.
        var storageChannel = storageQueueNames.Count == 1 ? storageQueueNames.First() : null;

        foreach (var site in seams)
        {
            var channel = site.Channel
                ?? (site.Transport == "AzureStorageQueue" ? storageChannel : null);
            var channelKey = $"queue:{site.Transport}:{channel ?? "unresolved"}";

            if (site.IsSend)
            {
                model.Detections.Add(new EventFlowDetection(channelKey, site.ClassName, "Publish", site.Transport)
                {
                    ExtractorName = "EventBusExtractor",
                    SourceFile = site.File,
                    LineNumber = site.Line,
                    Confidence = 0.6f, // syntax-only channel join — renders [approx]
                });
            }
            else
            {
                model.Detections.Add(new MessageConsumerDetection(channelKey, site.ClassName, site.Transport)
                {
                    ExtractorName = "EventBusExtractor",
                    SourceFile = site.File,
                    LineNumber = site.Line,
                    Confidence = 0.6f,
                });
            }
        }
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
