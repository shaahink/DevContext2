using DevContext.Core.Extractors.Specific;
using DevContext.Core.Graph.Seams;
using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;
using DevContext.Core.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph;

public sealed partial class GraphBuilder
{
    /// <summary>WORKED EXAMPLE — every in-scope production type becomes a TypeNode (noise filtered structurally).</summary>
    private void AddTypeNodes(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope, ArchitectureArchetype archetype)
    {
        foreach (var type in model.OrderedTypes)
        {
            if (!_noise.IsProductionCode(type) || !scope.Contains(type.FilePath)) continue;
            var feature = DeriveFeature(type, model);
            var project = scope.ProjectForFile(type.FilePath);
            g.AddNode(new GraphNode(NodeId.ForType(type.Id), type.Name, NodeKind.Type)
            {
                FilePath = type.FilePath,
                SourceBody = type.SourceBody,
                LineNumber = type.StartLine,
                Layer = type.Layer != ArchitectureLayer.Unknown ? type.Layer.ToLabel(archetype) : null,
                Feature = feature,
                Project = project,
            });
        }
    }

    private static void AddServiceNodes(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope)
    {
        var runnable = ServiceBoundaryInference.RunnableProjects(scope, model.SamplesAreTheProduct);
        foreach (var proj in runnable)
        {
            g.AddNode(new GraphNode(NodeId.ForService(proj.Name), proj.Name, NodeKind.Service)
            {
                Project = proj.Name,
                Tags = [RoleTags.Runnable],
            });
        }
    }

    /// <summary>D9 — derives the feature label from namespace, stripping project and known layer prefixes.
    /// Returns the first meaningful segment after removing project-root namespace segments and layer-ish segments.</summary>
    private static string? DeriveFeature(TypeDiscovery type, DiscoveryModel model)
    {
        var ns = type.Namespace;
        if (string.IsNullOrWhiteSpace(ns)) return null;

        if (type.FilePath is not { } fp) return CarveFeature(ns);

        var matchedProject = model.Projects.FirstOrDefault(p =>
            p.FilePath is { } pp && fp.StartsWith(Path.GetDirectoryName(pp) ?? "", StringComparison.OrdinalIgnoreCase));
        if (matchedProject is not null)
        {
            var prefix = matchedProject.Name.Replace("-", "").Replace("_", "");
            if (ns.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                ns = ns[prefix.Length..].TrimStart('.');
        }
        if (ns.StartsWith("Services.", StringComparison.OrdinalIgnoreCase))
            ns = ns["Services.".Length..];

        return CarveFeature(ns);
    }

    private static readonly HashSet<string> LayerSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Api", "Controllers", "Endpoints", "Presentation", "UI",
        "Application", "UseCases", "Services", "Handlers", "Behaviors", "Validators",
        "Domain", "Entities", "Aggregates", "ValueObjects", "Events",
        "Infrastructure", "Persistence", "Data", "Repositories", "External",
        "Contracts", "Dto", "Messages", "Requests", "Responses",
        "Extensions", "Filters", "Middleware", "Mapping", "Configuration",
        "Pages", "Components", "Views", "ViewModels", "Platform", "Core", "Internals",
    };

    private static string? CarveFeature(string ns)
    {
        var segments = ns.Split('.');
        var meaningful = segments
            .Where(s => !string.IsNullOrWhiteSpace(s) && !LayerSegments.Contains(s))
            .ToArray();
        return meaningful.Length > 0 ? meaningful[0] : null;
    }


    /// <summary>Creates Handles edges from MediatRHandlerDetection detections AND from
    /// TypeDiscovery objects that transitively implement known handler interfaces (M1.1 closure).
    /// Transitive detection catches classes that inherit from a handler base class (not common
    /// but required for the "match handlers transitively" golden).</summary>
    private static void AddHandlerJoins(CodeGraphBuilder g, DiscoveryModel model, SymbolTable names, SolutionScope scope, NoiseFilter noise)
    {
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (!scope.Contains(h.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(h.SourceFile)) continue;
            EmitHandlerJoin(g, model, names, h.RequestType, h.HandlerType, h.Kind, h.SourceFile, h.LineNumber);
        }

        // M1.1 transitive: scan model types for classes whose BaseTypes transitively
        // implement handler interfaces but weren't picked up by the syntax-level extractor.
        var handlerByShortName = new Dictionary<string, List<TypeDiscovery>>(StringComparer.Ordinal);
        foreach (var t in model.OrderedTypes)
        {
            var sn = StripGenerics(t.Name);
            if (!handlerByShortName.TryGetValue(sn, out var list))
                handlerByShortName[sn] = list = [];
            list.Add(t);
        }

        var knownHandlerTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
            knownHandlerTypes.Add(names.ResolveName(h.HandlerType, h.SourceFile));

        foreach (var type in model.OrderedTypes)
        {
            if (type.Kind != Models.TypeKind.Class) continue;
            if (!scope.Contains(type.FilePath)) continue;
            if (!noise.IsProductionCode(type)) continue;
            if (knownHandlerTypes.Contains(type.Id)) continue;

            // Check if any BaseType transitively reaches a known handler type
            var reached = FindHandlerBaseType(type, handlerByShortName, knownHandlerTypes, []);
            if (reached is null) continue;

            // Find the most-specific handler interface in the chain
            var handlerIfaces = reached.ImplementedInterfaces
                .Where(i => IsHandlerInterface(i, handlerByShortName, []))
                .ToArray();
            if (handlerIfaces.Length == 0) continue;

            var handlerInterface = handlerIfaces[0];
            var args = ExtractGenericArgs(handlerInterface);
            if (args.Length < 1) continue;

            var requestType = args[0];
            var responseType = args.Length >= 2 ? args[1] : "Unit";
            var kind = handlerInterface.Contains("Notification", StringComparison.Ordinal)
                ? MediatRKind.Notification
                : MediatRKind.Command;

            EmitHandlerJoin(g, model, names, requestType, type.Name, kind, type.FilePath, type.StartLine ?? 1);
        }
    }

    private static void EmitHandlerJoin(CodeGraphBuilder g, DiscoveryModel model, SymbolTable names,
        string requestType, string handlerShortName, MediatRKind kind, string sourceFile, int lineNumber)
    {
        var requestId = NodeId.ForType(names.ResolveName(requestType, sourceFile));
        var handlerId = NodeId.ForType(names.ResolveName(handlerShortName, sourceFile));

        g.AddNode(new GraphNode(requestId, requestType, NodeKind.Type)
        {
            Tags = [kind.ToString().ToLowerInvariant()],
            Layer = "Application",
        });
        g.AddNode(new GraphNode(handlerId, handlerShortName, NodeKind.Type)
        {
            FilePath = sourceFile,
            Tags = [RoleTags.Handler],
            Layer = "Application",
            SourceBody = model.OrderedTypes
                .FirstOrDefault(t => t.Id == names.ResolveName(handlerShortName, sourceFile))
                ?.SourceBody,
        });
        g.AddEdge(new GraphEdge(requestId, handlerId, EdgeKind.Handles)
        {
            Provenance = $"{sourceFile}:{lineNumber}",
            Resolution = Resolution.Join,
        });
    }

    private static TypeDiscovery? FindHandlerBaseType(TypeDiscovery type,
        Dictionary<string, List<TypeDiscovery>> byShortName,
        HashSet<string> knownHandlers,
        HashSet<string> visited)
    {
        if (knownHandlers.Contains(type.Id)) return type;
        foreach (var bt in type.BaseTypes)
        {
            var stripped = StripGenerics(bt);
            if (!visited.Add(stripped)) continue;
            if (byShortName.TryGetValue(stripped, out var bases))
            {
                foreach (var baseType in bases)
                {
                    var result = FindHandlerBaseType(baseType, byShortName, knownHandlers, visited);
                    if (result is not null) return result;
                }
            }
        }
        return null;
    }

    private static bool IsHandlerInterface(string ifaceName,
        Dictionary<string, List<TypeDiscovery>> byShortName,
        HashSet<string> visited)
    {
        var stripped = StripGenerics(ifaceName);
        if (stripped is "IRequestHandler" or "INotificationHandler" or "IStreamRequestHandler")
            return true;
        if (!visited.Add(stripped)) return false;
        if (byShortName.TryGetValue(stripped, out var matches))
        {
            foreach (var match in matches)
            {
                if (match.Kind != Models.TypeKind.Interface) continue;
                foreach (var parent in match.ImplementedInterfaces)
                {
                    if (IsHandlerInterface(parent, byShortName, visited))
                        return true;
                }
            }
        }
        return false;
    }

    private static string[] ExtractGenericArgs(string typeName)
    {
        var open = typeName.IndexOf('<');
        if (open < 0) return [];
        var close = typeName.LastIndexOf('>');
        if (close <= open) return [];
        var inner = typeName.Substring(open + 1, close - open - 1);
        return SplitGenericCsv(inner);
    }

    private static string[] SplitGenericCsv(string args)
    {
        var depth = 0;
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        foreach (var ch in args)
        {
            switch (ch)
            {
                case '<': depth++; current.Append(ch); break;
                case '>': depth--; current.Append(ch); break;
                case ',' when depth == 0: parts.Add(current.ToString().Trim()); current.Clear(); break;
                default: current.Append(ch); break;
            }
        }
        if (current.Length > 0) parts.Add(current.ToString().Trim());
        return parts.ToArray();
    }

    /// <summary>B3: Detects IPipelineBehavior registrations from DI detections and creates
    /// WrappedBy edges from every Request node to each pipeline behavior. The trace renders
    /// pipeline behaviors as a "pipeline" seam under the first send that reaches a Request.</summary>
    private static void AddPipelineBehaviors(CodeGraphBuilder g, DiscoveryModel model, SymbolTable names, SolutionScope scope, NoiseFilter noise)
    {
        var behaviors = new HashSet<(string BehaviorType, string? SourceFile, int? LineNumber)>();

        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!scope.Contains(di.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(di.SourceFile)) continue;

            // Direct registration: services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            if (di.ServiceType.Contains("IPipelineBehavior", StringComparison.Ordinal))
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add((impl, di.SourceFile, di.LineNumber));
            }
            // MediatR extension: services.AddMediatR(cfg => { cfg.AddOpenBehavior(typeof(LoggingBehavior<,>)); })
            if (di.ExtensionsUsed.Contains("AddOpenBehavior") || di.ServiceType == "AddOpenBehavior")
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add((impl, di.SourceFile, di.LineNumber));
            }
            // Fluent config packed in lambda body: scan for AddOpenBehavior(typeof(X)) patterns
            if (di.ImplementationType is { Length: > 0 } body
                && body.Contains("AddOpenBehavior", StringComparison.Ordinal))
            {
                // Scan for AddOpenBehavior(typeof(X)) patterns — manual string scan (L2.3: no Regex here)
                var pos = 0;
                while ((pos = body.IndexOf("AddOpenBehavior", pos, StringComparison.Ordinal)) >= 0)
                {
                    pos += "AddOpenBehavior".Length;
                    var rest = body[pos..];
                    var bp = 0;
                    while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                    if (bp < rest.Length && rest[bp] == '(') bp++;
                    while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                    if (bp + "typeof".Length <= rest.Length
                        && rest.AsSpan(bp, "typeof".Length).SequenceEqual("typeof"))
                    {
                        bp += "typeof".Length;
                        while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                        if (bp < rest.Length && rest[bp] == '(') bp++;
                        while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                        var start = bp;
                        while (bp < rest.Length && (char.IsLetterOrDigit(rest[bp]) || rest[bp] == '_')) bp++;
                        if (bp > start)
                        {
                            var name = rest[start..bp];
                            if (name.Length > 0 && name != "?")
                                behaviors.Add((name, di.SourceFile, di.LineNumber));
                        }
                    }
                }
            }
        }

        foreach (var (behaviorType, file, line) in behaviors)
        {
            var behaviorFqn = names.ResolveName(behaviorType, file);
            var behaviorNodeId = NodeId.ForType(behaviorFqn);
            g.AddNode(new GraphNode(behaviorNodeId, behaviorType, NodeKind.Type)
            {
                FilePath = file,
                Tags = [RoleTags.Service, RoleTags.Pipeline],
                Layer = "Infrastructure",
                SourceBody = model.OrderedTypes
                    .FirstOrDefault(t => t.Id == behaviorFqn)?.SourceBody,
            });

            // WrappedBy edge from every request node (a Type tagged command/query/notification) to
            // this pipeline behavior.
            foreach (var node in g.Nodes.Where(IsRequestNode))
            {
                g.AddEdge(new GraphEdge(node.Id, behaviorNodeId, EdgeKind.WrappedBy)
                {
                    Provenance = file is not null && line is not null ? $"{file}:{line}" : null,
                    Resolution = Resolution.Join,
                });
            }
        }
    }

    /// <summary>Strips typeof(…) / nameof(…) / generics to get a raw type name.</summary>
    private static string CleanTypeRef(string expr)
    {
        var s = expr.AsSpan().Trim();
        // typeof(X) → X
        if (s.StartsWith("typeof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        // nameof(X) → X
        else if (s.StartsWith("nameof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        // Strip generic arity suffix: LoggingBehavior<,> → LoggingBehavior
        var generic = s.IndexOf('<');
        if (generic > 0) s = s.Slice(0, generic);
        return s.ToString().Trim();
    }

    // ── P1 Map-facing seams (B1) — JOIN detections into graph nodes/edges ────────────────────────

    /// <summary>B1: EfEntityDetection → Entity nodes + aggregate tags PLUS subtypes of detected entity
    /// bases so entities registered via reflection (e.g. DntSite's RegisterAllDerivedEntities) are also
    /// tagged — Iteration 6 deferred / DntSite TOUCHES gap.</summary>
    private static void AddEntityNodes(CodeGraphBuilder g, DiscoveryModel model, SymbolTable names, SolutionScope scope, NoiseFilter noise)
    {
        var knownEntityFqns = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            if (!scope.Contains(e.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(e.SourceFile)) continue;
            var entityId = NodeId.ForType(names.ResolveName(e.EntityType, e.SourceFile));
            var tags = e.IsAggregate
                ? ImmutableArray.Create(RoleTags.Entity, RoleTags.Aggregate)
                : ImmutableArray.Create(RoleTags.Entity);
            g.AddNode(new GraphNode(entityId, e.EntityType, NodeKind.Type)
            {
                FilePath = e.SourceFile,
                Tags = tags,
                Layer = "Domain",
            });
            knownEntityFqns.Add(names.ResolveName(e.EntityType, e.SourceFile));
        }

        // Iteration 6 deferred: when a base entity is detected but its subtypes aren't (because they were
        // registered via reflection — DntSite's RegisterAllDerivedEntities from BaseEntity), create
        // entity-tagged nodes for every in-scope production type whose base resolves to a known entity.
        foreach (var type in model.OrderedTypes)
        {
            if (!scope.Contains(type.FilePath) || type.IsHardExcluded) continue;
            if (type.BaseTypes.IsDefaultOrEmpty) continue;
            foreach (var bt in type.BaseTypes)
            {
                if (knownEntityFqns.Contains(names.ResolveName(bt, type.FilePath)))
                {
                    g.AddNode(new GraphNode(NodeId.ForType(type.Id), type.Name, NodeKind.Type)
                    {
                        FilePath = type.FilePath,
                        Tags = [RoleTags.Entity],
                        Layer = "Domain",
                    });
                    break;
                }
            }
        }
    }

    /// <summary>A-F14: Creates EntityRelation edges between entity type nodes by inspecting each entity's
    /// declared navigation properties. Creates edges in the BelongsTo direction (child entity → parent
    /// aggregate/entity) for depth-from-aggregate-root traversal. For reference properties (OrderItem.Order),
    /// the child entity owns the property → edge goes child→parent. For collection properties
    /// (Order.ICollection&lt;OrderItem&gt;), the parent owns the property → edge is reversed to child→parent.
    /// Honesty note: declared-shape only; fluent-API <c>HasMany</c> mappings are not parsed in v1.</summary>
    private static void AddEntityNavigationEdges(CodeGraphBuilder g, DiscoveryModel model,
        SymbolTable names, SolutionScope scope)
    {
        // Build a set of known entity short names from detections + already entity-tagged graph nodes
        var entityShortNames = model.Detections.OfType<EfEntityDetection>()
            .Select(d => d.EntityType)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var node in g.Nodes)
        {
            if (node.Kind != NodeKind.Type || !node.Tags.Contains(RoleTags.Entity))
                continue;
            entityShortNames.Add(node.Title);
        }

        foreach (var type in model.OrderedTypes)
        {
            if (!scope.Contains(type.FilePath)) continue;
            if (!entityShortNames.Contains(type.Name)) continue;
            if (type.Properties.IsDefaultOrEmpty) continue;

            var entityId = NodeId.ForType(type.Id);

            foreach (var prop in type.Properties)
            {
                var (targetName, isCollection) = ExtractInnerEntityNameWithDir(prop.PropertyType);
                if (targetName is null || targetName == type.Name) continue;
                if (!entityShortNames.Contains(targetName)) continue;

                var targetFqn = names.ResolveName(targetName, type.FilePath);
                var targetId = NodeId.ForType(targetFqn);

                // BelongsTo direction: edge from child → parent.
                // For collection properties (e.g. Order has ICollection<OrderItem>), the owning type
                // is the parent; edge direction is reversed so OrderItem → Order.
                // For reference properties (e.g. OrderItem has Order Order), the owning type IS the
                // child, so edge direction is already child→parent.
                if (isCollection)
                    g.AddEdge(new GraphEdge(targetId, entityId, EdgeKind.EntityRelation)
                    {
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.6f,
                    });
                else
                    g.AddEdge(new GraphEdge(entityId, targetId, EdgeKind.EntityRelation)
                    {
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.6f,
                    });
            }
        }
    }

    /// <summary>Extracts the inner entity name and collection-direction flag from a property type string.
    /// Returns (name, isCollection) where isCollection is true for <c>ICollection&lt;T&gt;</c>,
    /// <c>List&lt;T&gt;</c>, <c>IEnumerable&lt;T&gt;</c>, <c>T[]</c> patterns.
    /// Returns null for non-entity property types like <c>string</c>, <c>int</c>, <c>DateTime</c>.</summary>
    private static (string? Name, bool IsCollection) ExtractInnerEntityNameWithDir(string propertyType)
    {
        if (string.IsNullOrEmpty(propertyType)) return (null, false);
        var type = propertyType.AsSpan().Trim();

        // Array: OrderItem[] → collection
        if (type.EndsWith("[]"))
        {
            var inner = type[..^2].Trim();
            return inner.IsEmpty ? (null, false) : (inner.ToString(), true);
        }

        // Generic collection: ICollection<OrderItem>, List<Product>, IEnumerable<Entity>, etc.
        var open = type.IndexOf('<');
        var close = type.LastIndexOf('>');
        if (open >= 0 && close > open)
        {
            var inner = type[(open + 1)..close].Trim();
            if (inner.EndsWith("?"))
                inner = inner[..^1];
            return inner.IsEmpty ? (null, false) : (inner.ToString(), true);
        }

        // Nullable reference: Order?
        if (type.EndsWith("?"))
            type = type[..^1];

        // Skip primitives and framework types
        if (type is "string" or "int" or "long" or "short" or "byte" or "float" or "double"
            or "bool" or "char" or "decimal" or "DateTime" or "Guid" or "TimeSpan" or "DateTimeOffset"
            or "Uri" or "object" or "String")
            return (null, false);

        return (type.ToString(), false);
    }

    /// <summary>Extracts the inner entity name from a property type string like
    /// <c>ICollection&lt;OrderItem&gt;</c> → "OrderItem",
    /// <c>List&lt;Product&gt;</c> → "Product",
    /// <c>Order</c> → "Order".
    /// Returns null for non-entity property types like <c>string</c>, <c>int</c>, <c>DateTime</c>.</summary>
    private static string? ExtractInnerEntityName(string propertyType)
    {
        var (name, _) = ExtractInnerEntityNameWithDir(propertyType);
        return name;
    }

    /// <summary>B1: MediatR notification handlers + message bus consumers → Event nodes + Consumes edges.
    /// Domain events (INotificationHandler) and integration events (MessageConsumer) are unified as
    /// Event nodes; both feed into Handler nodes via Consumes edges.</summary>
    private static void AddEventConsumers(CodeGraphBuilder g, DiscoveryModel model, SymbolTable names, SolutionScope scope, NoiseFilter noise)
    {
        // Notification handlers (domain events via MediatR)
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (h.Kind != MediatRKind.Notification) continue;
            if (!scope.Contains(h.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(h.SourceFile)) continue;
            var eventId = NodeId.ForType(names.ResolveName(h.RequestType, h.SourceFile));
            var handlerId = NodeId.ForType(names.ResolveName(h.HandlerType, h.SourceFile));

            g.AddNode(new GraphNode(eventId, h.RequestType, NodeKind.Type)
            {
                Tags = [RoleTags.DomainEvent],
                Layer = "Domain",
            });
            g.AddNode(new GraphNode(handlerId, h.HandlerType, NodeKind.Type)
            {
                FilePath = h.SourceFile,
                Project = scope.ProjectForFile(h.SourceFile),
                Tags = [RoleTags.Handler],
                Layer = "Application",
            });
            g.AddEdge(new GraphEdge(eventId, handlerId, EdgeKind.Consumes)
            {
                Provenance = $"{h.SourceFile}:{h.LineNumber}",
                Resolution = Resolution.Join,
            });
        }

        // Message bus consumers (integration events via RabbitMQ/MassTransit/etc.)
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
        {
            if (!scope.Contains(mc.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(mc.SourceFile)) continue;
            // B5 (Prism D1.2d): a queue CHANNEL is not a type — its display title doubles as the node
            // key (never resolved as a type name) so the publisher half (below) lands on the same node
            // and the event wiring joins them, and the wire renders "feed-queue [AzureStorageQueue]".
            var isChannel = mc.MessageType.StartsWith("queue:", StringComparison.Ordinal);
            var eventId = isChannel
                ? NodeId.ForType(ChannelTitle(mc.MessageType))
                : NodeId.ForType(names.ResolveName(mc.MessageType, mc.SourceFile));
            var consumerType = names.ResolveName(mc.ConsumerType, mc.SourceFile);
            var handlerId = NodeId.ForType(consumerType);

            g.AddNode(new GraphNode(eventId, isChannel ? ChannelTitle(mc.MessageType) : mc.MessageType, NodeKind.Type)
            {
                Tags = [RoleTags.IntegrationEvent, mc.BusKind],
                Layer = "Contracts",
            });
            g.AddNode(new GraphNode(handlerId, mc.ConsumerType, NodeKind.Type)
            {
                FilePath = mc.SourceFile,
                Project = scope.ProjectForFile(mc.SourceFile),
                Tags = [RoleTags.Consumer],
                Layer = "Infrastructure",
            });
            g.AddEdge(new GraphEdge(eventId, handlerId, EdgeKind.Consumes)
            {
                Provenance = $"{mc.SourceFile}:{mc.LineNumber}",
                Resolution = Resolution.Join,
            });
        }

        // B5 (Prism D1.2d): queue-channel PUBLISHERS (EventBusExtractor queue seams). The Raises edge
        // onto the shared channel node completes the wire — FeedsApi → [feed-queue] → Worker renders
        // on the event board and as a cross-service bus link. Syntactic resolution → [approx].
        foreach (var ef in model.Detections.OfType<EventFlowDetection>())
        {
            if (ef.Kind != "Publish" || !ef.EventType.StartsWith("queue:", StringComparison.Ordinal)) continue;
            if (!scope.Contains(ef.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(ef.SourceFile)) continue;

            var channelId = NodeId.ForType(ChannelTitle(ef.EventType));
            var publisherId = NodeId.ForType(names.ResolveName(ef.Target, ef.SourceFile));

            g.AddNode(new GraphNode(channelId, ChannelTitle(ef.EventType), NodeKind.Type)
            {
                Tags = [RoleTags.IntegrationEvent, ef.BusKind],
                Layer = "Contracts",
            });
            g.AddNode(new GraphNode(publisherId, ef.Target, NodeKind.Type)
            {
                FilePath = ef.SourceFile,
                Project = scope.ProjectForFile(ef.SourceFile),
            });
            g.AddEdge(new GraphEdge(publisherId, channelId, EdgeKind.Raises)
            {
                Provenance = $"{ef.SourceFile}:{ef.LineNumber}",
                Resolution = Resolution.Syntactic,
                Confidence = ef.Confidence,
            });
        }
    }

    /// <summary>"queue:AzureStorageQueue:feed-queue" → "feed-queue [AzureStorageQueue]";
    /// an unresolved channel shows the transport alone.</summary>
    private static string ChannelTitle(string channelKey)
    {
        var parts = channelKey.Split(':');
        if (parts.Length < 3) return channelKey;
        var transport = parts[1];
        var name = parts[2];
        return name == "unresolved" ? $"{transport} queue" : $"{name} [{transport}]";
    }

}
