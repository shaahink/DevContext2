using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects in-memory event bus patterns (IEventBus.Subscribe/PublishAsync) beyond MassTransit/NServiceBus.</summary>
[ExtractorOrder(46)]
public sealed class InMemoryEventBusExtractor : IDiscoveryExtractor
{
    public string Name => "InMemoryEventBusExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [], ["event-flow"],
        ["model.Detections"],
        "Detects in-memory event bus wiring: IEventBus.Subscribe, IEventBus.PublishAsync, and IEventHandler implementations");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var eventHandlers = DiscoverEventHandlers(model);
        var implicitPublishCount = await ScanFilesForEventBusCallsAsync(context, model, ct).ConfigureAwait(false);

        EmitHandlerDetections(model, eventHandlers);

        EmitEventBusDiagnostic(model, eventHandlers, implicitPublishCount);
    }

    private static Dictionary<string, string> DiscoverEventHandlers(DiscoveryModel model)
    {
        var eventHandlers = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            foreach (var iface in type.ImplementedInterfaces)
            {
                var handlerMatch = MatchEventHandlerGeneric(iface);
                if (handlerMatch is not null)
                    eventHandlers[handlerMatch] = type.Name;
            }
        }
        return eventHandlers;
    }

    private async ValueTask<int> ScanFilesForEventBusCallsAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var implicitPublishCount = 0;

        await foreach (var filePath in ExtractorHelpers.EnumerateSourceFilesAsync(context, ct).ConfigureAwait(false))
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

            foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (inv.Expression is not MemberAccessExpressionSyntax ma) continue;

                var methodName = ma.Name.Identifier.ValueText;

                if (methodName is "Subscribe" or "SubscribeAsync" && ma.Name is GenericNameSyntax subGeneric)
                {
                    var eventTypeArgs = subGeneric.TypeArgumentList.Arguments;
                    if (eventTypeArgs.Count >= 1)
                    {
                        var eventType = eventTypeArgs[0].ToString();
                        var handlerName = ExtractHandlerArg(inv);
                        var resolved = handlerName ?? "?";
                        var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                        model.Detections.Add(new EventFlowDetection(
                            eventType, resolved, "Subscribe", "in-memory")
                        {
                            ExtractorName = Name,
                            SourceFile = filePath,
                            LineNumber = line
                        });
                    }
                }
                else if (methodName is "PublishAsync" or "Publish")
                {
                    var eventType = ExtractEventType(ma, inv);
                    if (eventType is not null)
                    {
                        var callerType = CallingType(root, inv);
                        var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                        model.Detections.Add(new EventFlowDetection(
                            eventType, callerType ?? "?", "Publish", "in-memory")
                        {
                            ExtractorName = Name,
                            SourceFile = filePath,
                            LineNumber = line
                        });
                    }
                    else
                    {
                        implicitPublishCount++;
                    }
                }
            }
        }

        return implicitPublishCount;
    }

    private void EmitHandlerDetections(DiscoveryModel model, Dictionary<string, string> eventHandlers)
    {
        foreach (var kv in eventHandlers)
        {
            model.Detections.Add(new EventFlowDetection(
                kv.Key, kv.Value, "Handler", "in-memory")
            {
                ExtractorName = Name,
                SourceFile = "",
                LineNumber = 0
            });
        }
    }

    private void EmitEventBusDiagnostic(DiscoveryModel model, Dictionary<string, string> eventHandlers, int implicitPublishCount)
    {
        if (eventHandlers.Count == 0 && !model.Detections.OfType<EventFlowDetection>().Any() && implicitPublishCount == 0)
            return;

        var pubCount = model.Detections.OfType<EventFlowDetection>().Count(d => string.Equals(d.Kind, "Publish", StringComparison.Ordinal));
        var subCount = model.Detections.OfType<EventFlowDetection>().Count(d => string.Equals(d.Kind, "Subscribe", StringComparison.Ordinal));
        model.AddDiagnostic(DiagnosticLevel.Info, Name,
            $"Found {eventHandlers.Count} handlers, {subCount} subscriptions, {pubCount} explicit publications{(implicitPublishCount > 0 ? $", {implicitPublishCount} implicit (unresolved)" : "")}");
    }

    private static string? ExtractEventType(MemberAccessExpressionSyntax ma, InvocationExpressionSyntax inv)
    {
        if (ma.Name is GenericNameSyntax pubGeneric)
        {
            var evtArgs = pubGeneric.TypeArgumentList.Arguments;
            if (evtArgs.Count >= 1)
                return evtArgs[0].ToString();
        }
        return null;
    }

    private static string? ExtractHandlerArg(InvocationExpressionSyntax inv)
    {
        var args = inv.ArgumentList.Arguments;
        if (args.Count >= 1)
        {
            var arg = args[0].Expression;
            // Handler is a simple identifier like a field name
            if (arg is IdentifierNameSyntax id)
                return id.Identifier.ValueText;
            // Handler is a lambda like sp => sp.GetRequiredService<Handler>()
            // or an object creation like new Handler()
            return arg.ToString();
        }
        return null;
    }

    private static string? CallingType(SyntaxNode root, InvocationExpressionSyntax inv)
    {
        var method = inv.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (method is null) return null;

        var type = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (type is null) return null;

        var ns = RoslynSyntaxHelpers.GetNamespace(type);
        return !string.Equals(ns, "global", StringComparison.Ordinal) ? $"{ns}.{type.Identifier.ValueText}.{method.Identifier.ValueText}" : null;
    }

    private static string? MatchEventHandlerGeneric(string ifaceFull)
    {
        // Match IEventHandler<T> — check for IEventHandler< prefix
        var i = ifaceFull.IndexOf("IEventHandler<", StringComparison.Ordinal);
        if (i < 0) return null;

        var rest = ifaceFull[(i + "IEventHandler<".Length)..];
        // Extract the generic argument (T in IEventHandler<T>)
        // Handle nested generics by counting angle brackets
        var depth = 0;
        for (var j = 0; j < rest.Length; j++)
        {
            if (rest[j] == '<') depth++;
            else if (rest[j] == '>')
            {
                if (depth == 0)
                    return rest[..j].Trim();
                depth--;
            }
        }
        return null;
    }
}

/// <summary>Detection for an event flow relationship (publisher → event → handler).</summary>
public sealed record EventFlowDetection(
    string EventType,
    string Target,
    string Kind,       // "Subscribe" | "Publish" | "Handler"
    string BusKind
) : Detection;
