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
    /// <summary>B1: DiRegistrationDetection → Resolves (interface → impl) edges.
    /// Only DirectBinding registrations (explicit interface-to-implementation). Uses ISymbolResolver
    /// for single-implementor fallback. Creates Resolves edges from interface TypeNode to impl TypeNode.</summary>
    private void AddDiResolves(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope)
    {
        // Pre-compute single-implementor map for fallback when no DI registration
        var singleImplMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var implCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            if (!scope.Contains(type.FilePath) || !_noise.IsProductionCode(type)) continue;
            foreach (var iface in type.ImplementedInterfaces)
            {
                var ifaceShort = StripGenerics(iface);
                if (!implCounts.TryGetValue(ifaceShort, out var count))
                {
                    count = 0;
                }
                implCounts[ifaceShort] = count + 1;
                if (count == 0)
                    singleImplMap[ifaceShort] = type.Id;
                else
                    singleImplMap.Remove(ifaceShort); // multiple impls → ambiguous
            }
        }

        // T2.1: which services have a PRODUCTION DI registration? A test-project registration is a
        // last-resort binding (shamshir wired ITradeRepository→SqliteTradeRepository only from an
        // InProcessEngineSmokeTests file) — prefer production, and tag a test-only edge so the render is
        // honest. Uses NoiseFilter.IsProductionEntrySource (which already encodes the test/sample rules),
        // never a path regex.
        var productionRegisteredSvc = new HashSet<string>(StringComparer.Ordinal);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (di.Shape != DiRegistrationShape.DirectBinding) continue;
            if (!scope.Contains(di.SourceFile) || !_noise.IsProductionEntrySource(di.SourceFile)) continue;
            productionRegisteredSvc.Add(StripGenerics(di.ServiceType));
        }

        // C5: N hosts each registering the same service→impl used to race for the one deduped edge —
        // model.Detections is an unordered bag, so the cited registration was arbitrary AND flapped
        // run-to-run. Group per (svc, impl) pair instead: ALL sites ride the edge (deterministically
        // ordered), Provenance is the first, and the trace ranks by focus-host proximity at walk time.
        var bindings = model.Detections.OfType<DiRegistrationDetection>()
            .Where(di => scope.Contains(di.SourceFile)
                && di.Shape == DiRegistrationShape.DirectBinding
                && !string.IsNullOrEmpty(di.ImplementationType)
                && di.ImplementationType != "?"
                && !di.ImplementationType.StartsWith("sp =>")
                && !di.ImplementationType.StartsWith("_ =>")
                && !di.ImplementationType.StartsWith("(")
                && !di.ImplementationType.Contains("GetRequiredService"))
            .Where(di => _noise.IsProductionEntrySource(di.SourceFile)
                // Production wins: skip a test-only registration when a production one exists for the same service.
                || !productionRegisteredSvc.Contains(StripGenerics(di.ServiceType)))
            .OrderBy(di => di.SourceFile, StringComparer.Ordinal).ThenBy(di => di.LineNumber)
            .GroupBy(di => (
                Svc: NodeId.ForType(names.Resolve(di.ServiceType, di.SourceFile)),
                Impl: NodeId.ForType(names.Resolve(di.ImplementationType, di.SourceFile))));

        foreach (var pair in bindings)
        {
            var first = pair.First();
            var svcShort = StripGenerics(first.ServiceType);
            var (svcNodeId, implNodeId) = (pair.Key.Svc, pair.Key.Impl);

            // Ensure both nodes exist
            if (!g.HasNode(svcNodeId))
                g.AddNode(new GraphNode(svcNodeId, first.ServiceType, NodeKind.Type)
                {
                    Layer = "Infrastructure", // DI extension methods (AddMediatR, AddDbContext, etc.)
                });
            g.AddNode(new GraphNode(implNodeId, first.ImplementationType, NodeKind.Type)
            {
                Tags = [RoleTags.Service],
                Layer = "Infrastructure", // DI-registered implementations
            });

            var sites = pair
                .Select(di => (Site: $"{di.SourceFile}:{di.LineNumber}",
                    Project: scope.ProjectForFile(di.SourceFile) ?? ""))
                .DistinctBy(x => x.Site)
                .ToList();

            // I1.6 — tag Resolves edges with multi-impl count for render annotation
            var multiCount = implCounts.TryGetValue(svcShort, out var c) && c > 1 ? c : 0;
            var edgeTags = ImmutableArray.CreateBuilder<string>();
            // T2.1: last-resort test binding — the production-wins filter above already dropped test
            // sites when a production one exists, so a surviving non-prod site means test-only.
            if (!_noise.IsProductionEntrySource(first.SourceFile)) edgeTags.Add(RoleTags.TestOnlyDi);
            if (pair.Any(di => di.Lifetime == "HttpClient")) edgeTags.Add(RoleTags.HttpClientBinding); // C6 (D1.2f)
            g.AddEdge(new GraphEdge(svcNodeId, implNodeId, EdgeKind.Resolves)
            {
                Provenance = sites[0].Site,
                Resolution = Resolution.Join,
                MultiImplCount = multiCount,
                Tags = edgeTags.ToImmutable(),
                RegistrationSites = sites.Count > 1 ? [.. sites.Select(x => x.Site)] : [],
                RegistrationProjects = sites.Count > 1 ? [.. sites.Select(x => x.Project)] : [],
            });
        }

        // Fallback: single-implementor interfaces not covered by DI registrations
        var diResolvedSvcIds = new HashSet<NodeId>();
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!scope.Contains(di.SourceFile)) continue;
            if (di.Shape != DiRegistrationShape.DirectBinding) continue;
            var svcFqn = names.Resolve(di.ServiceType, di.SourceFile);
            diResolvedSvcIds.Add(NodeId.ForType(svcFqn));
        }

        foreach (var (ifaceShort, implFqn) in singleImplMap)
        {
            var ifaceFqn = names.Resolve(ifaceShort);
            var svcNodeId = NodeId.ForType(ifaceFqn);
            var implNodeId = NodeId.ForType(implFqn);
            if (!g.HasNode(svcNodeId) || !g.HasNode(implNodeId)) continue;
            if (diResolvedSvcIds.Contains(svcNodeId)) continue; // already resolved via DI

            var fallbackMultiCount = implCounts.TryGetValue(ifaceShort, out var fc) && fc > 1 ? fc : 0;
            g.AddEdge(new GraphEdge(svcNodeId, implNodeId, EdgeKind.Resolves)
            {
                Resolution = Resolution.Syntactic,
                Confidence = 0.7f,
                MultiImplCount = fallbackMultiCount,
            });
        }
    }

    // ── P2 Trace-facing seams (C1) — joins that complete the indirection-bridged trace ─────────

    /// <summary>C1: model.CallEdges → <b>member→member</b> Calls edges, but ONLY between types that are
    /// real nodes in the graph (in-scope solution types). The syntactic call graph emits a callee per
    /// invocation, many of which are local variables, fluent-chain fragments, or framework methods (e.g.
    /// "group", "pb", "AsNoTracking()"); materializing those as phantom nodes floods the trace with noise.
    /// By requiring both endpoints to already exist as declared Type nodes (non-null FilePath), the trace
    /// keeps only edges to types we actually know. Origin is the caller <b>method</b> and target the callee
    /// <b>method</b> (both carried on <see cref="CallEdge"/>), so a focused trace descends method-to-method
    /// — the spine — instead of inheriting every sibling method's edges. Member nodes carry their owning
    /// Type's FilePath (salient lines fall back to the Type body in <see cref="TraceBuilder"/>).
    /// Resolution flows through from the edge (semantic → [verified], syntactic → [approx]).</summary>
    private static void AddCallEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        IReadOnlyList<BodyFacts>? bodyFacts = null)
    {
        // L3.3 — build semantic index from upgraded BodyFacts for Call edge verification.
        var semanticLocs = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        if (bodyFacts is { Count: > 0 })
        {
            foreach (var body in bodyFacts)
            {
                foreach (var op in body.Ops)
                {
                    var prov = $"{body.File}:{op.Line}";
                    switch (op)
                    {
                        case CreationOp c when c.Type is { Tier: ResolutionTier.Semantic } s:
                            AddToIndex(semanticLocs, prov, s.Text);
                            break;
                        case LocalDeclOp l when l.InferredFrom is { Tier: ResolutionTier.Semantic } s:
                            AddToIndex(semanticLocs, prov, s.Text);
                            break;
                        case InvocationOp i when i.ReceiverType is { Tier: ResolutionTier.Semantic } s:
                            AddToIndex(semanticLocs, prov, s.Text);
                            break;
                        case InvocationOp i:
                            foreach (var ga in i.GenericArgs)
                                if (ga is { Tier: ResolutionTier.Semantic } s)
                                    AddToIndex(semanticLocs, prov, s.Text);
                            break;
                    }
                }
            }
        }

        foreach (var ce in model.CallEdges)
        {
            var callerFqn = names.Resolve(ce.CallerType, ce.CallSiteLocation);
            var calleeFqn = names.Resolve(ce.CalleeType, ce.CallSiteLocation);

            // Filter self-calls to known noise targets — syntactic-resolver mis-attributions (nameof,
            // controller result-helpers) that member-origin precision surfaced (Iteration 4 noise polish).
            if (callerFqn == calleeFqn && IsSelfCallNoise(ce.CalleeMethod)) continue;

            // Declared in-scope types only. After the Type+tags collapse, requests/events/handlers that
            // live in referenced projects also exist as Type nodes (name-only, added by joins) — gating
            // on a non-null FilePath (set only by AddTypeNodes) keeps Calls restricted to types we
            // actually declared, exactly as before the collapse, so no phantom call edges appear.
            var callerType = g.GetNode(NodeId.ForType(callerFqn));
            var calleeType = g.GetNode(NodeId.ForType(calleeFqn));
            if (callerType?.FilePath is null || calleeType?.FilePath is null) continue;

            var callerId = NodeId.ForMember(callerFqn, ce.CallerMethod);
            var calleeId = NodeId.ForMember(calleeFqn, ce.CalleeMethod);
            if (callerId == calleeId) continue;                              // skip direct self-recursion

            // L3.3 — check if this call site was semantically verified via Tier B body facts.
            var resolution = ce.Resolution;
            if (resolution == Resolution.Syntactic
                && ce.CallSiteLocation is { } loc
                && semanticLocs.TryGetValue(loc, out var semTargets)
                && IsAnyShortMatch(ce.CalleeType, semTargets))
            {
                resolution = Resolution.Semantic;
            }

            // Member nodes for both endpoints, carrying the owning Type's file (body filled — when at all —
            // by the body-scan seams / HTTP entry; salient otherwise falls back to the parent Type body).
            g.AddNode(new GraphNode(callerId, $"{callerType.Title}.{ce.CallerMethod}", NodeKind.Member)
            {
                FilePath = callerType.FilePath,
            });
            g.AddNode(new GraphNode(calleeId, $"{calleeType.Title}.{ce.CalleeMethod}", NodeKind.Member)
            {
                FilePath = calleeType.FilePath,
            });

            g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls)
            {
                Provenance = ce.CallSiteLocation,
                Resolution = resolution,
                Confidence = resolution == Resolution.Semantic ? 0.95f : 0.6f,
            });
        }
    }

    /// <summary>L3.4 — Broadens call-edge binding for sparse graphs (library/tool archetypes where
    /// normal CallEdges produce very few edges because one or both endpoints lack a FilePath).
    /// Detects sparseness (entries &lt; 5 or edge/node ratio &lt; 0.1), identifies top-K central
    /// type nodes by degree, and binds their inter-type call edges from the model's CallEdges.
    /// Budget-capped at 500 additional edges; honest scope reported in Stats.</summary>
    private static (bool IsSparse, int HubCount) AddHubScopeEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        ImmutableArray<EntryPoint> entries)
    {
        var nodeCount = g.NodeCount;
        var edgeCount = g.EdgeCount;
        var ratio = nodeCount > 0 ? (double)edgeCount / nodeCount : 0;

        if (entries.Length >= 5 && ratio >= 0.1) return (false, 0);

        // Compute degree centrality for all types with a FilePath (in-scope, production code)
        var typeDegrees = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var ce in model.CallEdges)
        {
            var cfqn = names.Resolve(ce.CallerType);
            var dfqn = names.Resolve(ce.CalleeType);
            if (cfqn != ce.CallerType) typeDegrees[cfqn] = typeDegrees.GetValueOrDefault(cfqn) + 1;
            if (dfqn != ce.CalleeType) typeDegrees[dfqn] = typeDegrees.GetValueOrDefault(dfqn) + 1;
        }

        // Build a set of type nodes already present with FilePath (production code)
        var existingTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in g.Nodes)
            if (node.Kind == NodeKind.Type && node.FilePath is not null)
                existingTypes.Add(node.Id.Key);

        // Top-K hubs
        var k = Math.Min(50, Math.Min(nodeCount / 4, typeDegrees.Count / 2));
        if (k < 5) return (false, 0);

        var hubs = typeDegrees
            .Where(kv => existingTypes.Contains(kv.Key))
            .OrderByDescending(kv => kv.Value)
            .Take(k)
            .Select(kv => kv.Key)
            .ToHashSet(StringComparer.Ordinal);

        var added = 0;
        foreach (var ce in model.CallEdges)
        {
            if (added >= 500) break;

            var cfqn = names.Resolve(ce.CallerType);
            var dfqn = names.Resolve(ce.CalleeType);
            if (cfqn == dfqn) continue;

            // At least one endpoint must be a hub
            if (!hubs.Contains(cfqn) && !hubs.Contains(dfqn)) continue;

            var callerId = NodeId.ForMember(cfqn, ce.CallerMethod);
            var calleeId = NodeId.ForMember(dfqn, ce.CalleeMethod);
            if (callerId == calleeId) continue;

            var callerNode = g.GetNode(NodeId.ForType(cfqn));
            var calleeNode = g.GetNode(NodeId.ForType(dfqn));

            g.AddNode(new GraphNode(callerId, $"{callerNode?.Title ?? cfqn}.{ce.CallerMethod}", NodeKind.Member)
            {
                FilePath = callerNode?.FilePath,
            });
            g.AddNode(new GraphNode(calleeId, $"{calleeNode?.Title ?? dfqn}.{ce.CalleeMethod}", NodeKind.Member)
            {
                FilePath = calleeNode?.FilePath,
            });

            if (g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls)
            {
                Provenance = ce.CallSiteLocation,
                Resolution = ce.Resolution,
                Confidence = (ce.Resolution == Resolution.Semantic ? 0.95f : 0.6f) * 0.8f,
            }))
                added++;
        }
        return (true, hubs.Count);
    }

    // ── L2.3: Seam detectors over BodyFacts (replaces the old regex body-scan methods) ────────────

    /// <summary>L2.3 — Runs structured seam detectors (<see cref="ISeamDetector"/>) over the pre-extracted
    /// <see cref="BodyFacts"/>, which replaced the retired regex body-scan passes. Detectors are pure
    /// (facts in, seams out); here we resolve targets via the
    /// <see cref="SymbolTable"/> and materialise graph nodes/edges. Ambiguous targets are skipped per
    /// Law R1 (no silent winners); unresolved (external) types use the short name as-is. Edge provenance
    /// comes from the body-fact line number, anchored on the correct Member node by construction — never a
    /// char-offset estimate. The Raises edges written here are the publisher half of the T2.6
    /// <see cref="EventWiringProjection"/>.</summary>
    private static void AddSeamsFromDetectors(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        SolutionScope scope, IReadOnlyList<BodyFacts>? allBodyFacts)
    {
        // Auto-extract BodyFacts from model TypeDiscovery SourceBodies when the pipeline hasn't
        // pre-extracted them (backward compatibility for tests that build directly from model).
        if (allBodyFacts is null || allBodyFacts.Count == 0)
        {
            var facts = new List<BodyFacts>();
            foreach (var type in model.Types.Values)
            {
                if (type.SourceBody is not { Length: > 0 } sb) continue;
                try
                {
                    var hasTypeDecl = sb.Contains("class ", StringComparison.Ordinal)
                        || sb.Contains("struct ", StringComparison.Ordinal)
                        || sb.Contains("record ", StringComparison.Ordinal);
                    var fullSource = sb;
                    if (!hasTypeDecl)
                    {
                        var nsDecl = !string.IsNullOrEmpty(type.Namespace) && !sb.Contains("namespace ", StringComparison.Ordinal)
                            ? $"namespace {type.Namespace} {{ "
                            : (sb.Contains("namespace ", StringComparison.Ordinal) ? "" : $"namespace {type.Name} {{ ");
                        var closings = nsDecl.Length > 0 ? " }}" : " }";
                        fullSource = $"{nsDecl}public class {type.Name} {{ {sb}{closings}";
                    }
                    else if (!string.IsNullOrEmpty(type.Namespace) && !sb.Contains("namespace ", StringComparison.Ordinal))
                    {
                        fullSource = $"namespace {type.Namespace} {{ {sb} }}";
                    }
                    var parseOpts = CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG");
                    var tree = CSharpSyntaxTree.ParseText(fullSource, parseOpts, path: type.FilePath);
                    var project = scope.ProjectForFile(type.FilePath) ?? "";
                    facts.AddRange(BodyFactExtractor.Extract(tree, type.FilePath, project));
                }
                catch { /* parse failure → skip */ }
            }
            allBodyFacts = facts;
        }
        if (allBodyFacts.Count == 0) return;

        // Build SeamContext from model detections + type base/interface data
        var (integrationTypes, domainTypes) = BuildTypeEventSets(model);
        var knownEntities = new HashSet<string>(StringComparer.Ordinal);

        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            knownEntities.Add(e.EntityType);
            knownEntities.Add(names.Resolve(e.EntityType, e.SourceFile));
        }
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
            integrationTypes.Add(mc.MessageType);
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (h.Kind == MediatRKind.Notification)
                domainTypes.Add(h.RequestType);
        }

        // Entity and event names that are also FQNs
        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            var entityFqn = names.Resolve(e.EntityType, e.SourceFile);
            if (!string.IsNullOrEmpty(entityFqn) && entityFqn != "?" && entityFqn != e.EntityType)
                knownEntities.Add(entityFqn);
        }
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
        {
            var msgFqn = names.Resolve(mc.MessageType, mc.SourceFile);
            if (!string.IsNullOrEmpty(msgFqn) && msgFqn != "?" && msgFqn != mc.MessageType)
                integrationTypes.Add(msgFqn);
        }
        foreach (var node in g.Nodes)
        {
            if (node.Kind == NodeKind.Type && node.Tags.Contains(RoleTags.Entity))
                knownEntities.Add(node.Title);
        }

        var ctx = BuildSeamContext(model, scope, integrationTypes, domainTypes, knownEntities, allBodyFacts);

        var detectors = new ISeamDetector[]
        {
            new MediatRDispatchDetector(),
            new BusPublishDetector(),
            new IntegrationEventCreationDetector(),
            new DomainEventRaiseDetector(),
            new EntityTouchDetector(),
            new PlainCallDetector(),
        };

        // L3.3 — build a quick index of (provenance → semantic-short-names) from upgraded BodyFacts.
        // Both provenance-level (file:line) and body-level keys are stored.
        var semanticLocs = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var body in allBodyFacts)
        {
            foreach (var op in body.Ops)
            {
                var prov = $"{body.File}:{op.Line}";
                switch (op)
                {
                    case CreationOp c when c.Type is { Tier: ResolutionTier.Semantic } s:
                        AddToIndex(semanticLocs, prov, s.Text);
                        break;
                    case LocalDeclOp l when l.InferredFrom is { Tier: ResolutionTier.Semantic } s:
                        AddToIndex(semanticLocs, prov, s.Text);
                        break;
                    case InvocationOp i when i.ReceiverType is { Tier: ResolutionTier.Semantic } s:
                        AddToIndex(semanticLocs, prov, s.Text);
                        break;
                    case InvocationOp i:
                        foreach (var ga in i.GenericArgs)
                            if (ga is { Tier: ResolutionTier.Semantic } s)
                                AddToIndex(semanticLocs, prov, s.Text);
                        break;
                }
            }
        }

        foreach (var body in allBodyFacts)
        {
            foreach (var detector in detectors)
            {
                try
                {
                    foreach (var match in detector.Detect(body, ctx))
                    {
                        var originId = ToMemberNodeId(match.Origin);
                        EnsureMemberId(g, originId, body.MemberName, body.File, body.Project, body.DeclLine);

                        var resolved = ctx.Symbols!.Resolve(match.Target);
                        if (resolved.Tier == ResolutionTier.Ambiguous)
                            continue; // Law R1: no silent winners

                        NodeId targetId;
                        string targetDisplayName = match.Target.Text;

                        if (resolved.Resolved is { } symId)
                            targetId = NodeId.ForType(symId.Canonical);
                        else
                            targetId = NodeId.ForType(match.Target.Text);

                        if (!g.HasNode(targetId))
                        {
                            var tags = match.Kind switch
                            {
                                EdgeKind.ReadsWrites => ImmutableArray.Create(RoleTags.Entity),
                                EdgeKind.Raises => match.DetectorId switch
                                {
                                    "IntegrationEventCreation" or "BusPublish" => ImmutableArray.Create(RoleTags.IntegrationEvent),
                                    "DomainEventRaise" => ImmutableArray.Create(RoleTags.DomainEvent),
                                    _ => ImmutableArray.Create(RoleTags.DomainEvent),
                                },
                                EdgeKind.Sends => ImmutableArray.Create(RoleTags.Command),
                                _ => ImmutableArray<string>.Empty,
                            };
                            g.AddNode(new GraphNode(targetId, targetDisplayName, NodeKind.Type)
                            {
                                Tags = tags,
                                Layer = match.Kind switch
                                {
                                    EdgeKind.ReadsWrites => "Domain",
                                    EdgeKind.Raises => "Domain",
                                    EdgeKind.Sends => "Application",
                                    _ => null,
                                },
                            });
                        }

                        var isSemantic = resolved.Tier == ResolutionTier.Semantic
                            || match.Target.Tier == ResolutionTier.Semantic
                            || (match.Provenance is { } p && semanticLocs.TryGetValue(p, out var semTargets)
                                && semTargets.Contains(match.Target.Text))
                            || IsTargetSemanticInBody(body, match.Target.Text);

                        g.AddEdge(new GraphEdge(originId, targetId, match.Kind)
                        {
                            Provenance = match.Provenance,
                            Resolution = isSemantic ? Resolution.Semantic : Resolution.Syntactic,
                            Confidence = match.Confidence,
                        });
                    }
                }
                catch { /* detector failure → skip its matches, continue with others */ }
            }
        }
    }

    private static void AddToIndex(Dictionary<string, HashSet<string>> map, string prov, string text)
    {
        if (!map.TryGetValue(prov, out var set)) map[prov] = set = [];
        set.Add(text);
    }

    private static bool IsTargetSemanticInBody(BodyFacts body, string targetShort)
    {
        foreach (var op in body.Ops)
        {
            if (op is LocalDeclOp l && l.InferredFrom is { Tier: ResolutionTier.Semantic } s
                && string.Equals(s.Text, targetShort, StringComparison.Ordinal))
                return true;
            if (op is CreationOp c && c.Type is { Tier: ResolutionTier.Semantic } cs
                && string.Equals(cs.Text, targetShort, StringComparison.Ordinal))
                return true;
            if (op is InvocationOp i && i.ReceiverType is { Tier: ResolutionTier.Semantic } rs
                && string.Equals(rs.Text, targetShort, StringComparison.Ordinal))
                return true;
            if (op is InvocationOp ig)
                foreach (var ga in ig.GenericArgs)
                    if (ga is { Tier: ResolutionTier.Semantic } gs
                        && string.Equals(gs.Text, targetShort, StringComparison.Ordinal))
                        return true;
        }
        return false;
    }

    private static bool IsAnyShortMatch(string fqn, HashSet<string> shortNames)
    {
        // The short name matches if it equals the last segment of the FQN or the full FQN.
        foreach (var sn in shortNames)
            if (string.Equals(fqn, sn, StringComparison.Ordinal)
                || fqn.EndsWith("." + sn, StringComparison.Ordinal))
                return true;
        return false;
    }

    /// <summary>Converts a BodyFacts <see cref="SymbolId"/> (format <c>TypeFqn::MethodName(N)</c>) to the
    /// <see cref="NodeId"/> format used by the graph (<c>TypeFqn.MethodName</c>).</summary>
    private static NodeId ToMemberNodeId(SymbolId memberId)
    {
        var canonical = memberId.Canonical;
        var sep = canonical.IndexOf("::", StringComparison.Ordinal);
        if (sep < 0) return NodeId.ForMember(canonical, canonical);
        var typeFqn = canonical[..sep];
        var after = canonical[(sep + 2)..];
        var paren = after.IndexOf('(');
        var methodName = paren > 0 ? after[..paren] : after;
        return NodeId.ForMember(typeFqn, methodName);
    }

    /// <summary>L2.4 — Runs seam detectors on lambda entry-handler member nodes that carry a SourceBody
    /// (populated by <see cref="HttpEntryPointBuilder"/>). Lambdas live inside the enclosing method's
    /// BodyFacts, so the main pass attributes edges to the enclosing method. This post-pass extracts
    /// per-lambda facts and attributes edges to the lambda member node so entry→lambda→dispatch traces
    /// work correctly for the checkout flow.</summary>
    private static void AddLambdaSeams(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope,
        IReadOnlyList<BodyFacts>? upgradedFacts)
    {
        var (integrationTypes, domainTypes) = BuildTypeEventSets(model);
        var ctx = BuildSeamContext(model, scope, integrationTypes, domainTypes, ImmutableHashSet<string>.Empty, upgradedFacts);

        // L3.2/L3.3 — semantic overlay: the lambda body is re-parsed in isolation (a synthetic tree not in
        // the Tier-B compilation), so its ops carry only syntactic types. Re-attach the semantic tier that the
        // whole-file pass already established, matched by (file, expression text, short type) — no line
        // dependency, so the synthetic tree's shifted lines don't matter. Covers dispatch via a `var` local
        // (LocalDecl.InferredFrom), inline argument (`sender.Send(new XCommand(..))` — ArgFact.Type), and
        // generic type arguments (`Adapt<T>`). Law R2: upgrade only.
        var semanticRefs = new Dictionary<(string File, string Text, string Type), Graph2.SymbolRef>();
        if (upgradedFacts is not null)
        {
            foreach (var body in upgradedFacts)
            {
                foreach (var op in body.Ops)
                {
                    switch (op)
                    {
                        case LocalDeclOp { InferredFrom: { Tier: ResolutionTier.Semantic } sem } local:
                            semanticRefs[(body.File, local.Name, sem.Text)] = sem;
                            break;
                        case InvocationOp inv:
                            foreach (var arg in inv.Args)
                                if (arg.Type is { Tier: ResolutionTier.Semantic } at)
                                    semanticRefs[(body.File, arg.Text, at.Text)] = at;
                            foreach (var ga in inv.GenericArgs)
                                if (ga is { Tier: ResolutionTier.Semantic } gat)
                                    semanticRefs[(body.File, gat.Text, gat.Text)] = gat;
                            break;
                    }
                }
            }
        }

        var detectors = new ISeamDetector[]
        {
            new MediatRDispatchDetector(),
            new BusPublishDetector(),
            new IntegrationEventCreationDetector(),
            new DomainEventRaiseDetector(),
            new EntityTouchDetector(),
            new PlainCallDetector(),
        };

        foreach (var node in g.Nodes.Where(n => n.Kind == NodeKind.Member
            && n.SourceBody is { Length: > 0 }
            && (n.Id.Key.Contains("<lambda>", StringComparison.Ordinal)
                || n.Id.Key.Contains("<anonymous>", StringComparison.Ordinal))).ToList())
        {
            try
            {
                // Wrap the lambda body in a synthetic method + class so the extractor finds it
                var body = node.SourceBody!;
                var filePath = node.FilePath ?? "";
                var project = node.Project ?? scope.ProjectForFile(filePath) ?? "";
                var wrapped = $"namespace _ {{ public class _ {{ public void _() {{ ({body})(); }} }} }}";
                var tree = CSharpSyntaxTree.ParseText(wrapped, path: filePath);
                var facts = OverlaySemanticLocals(
                    BodyFactExtractor.Extract(tree, filePath, project), filePath, semanticRefs);

                foreach (var bodyFacts in facts)
                {
                    foreach (var detector in detectors)
                    {
                        try
                        {
                            foreach (var match in detector.Detect(bodyFacts, ctx))
                            {
                                var resolved = ctx.Symbols!.Resolve(match.Target);
                                if (resolved.Tier == ResolutionTier.Ambiguous) continue;

                                NodeId targetId;
                                if (resolved.Resolved is { } symId)
                                    targetId = NodeId.ForType(symId.Canonical);
                                else
                                    targetId = NodeId.ForType(match.Target.Text);

                                EnsureMemberId(g, node.Id, node.Title, node.FilePath, node.Project);

                                if (!g.HasNode(targetId))
                                {
                                    g.AddNode(new GraphNode(targetId, match.Target.Text, NodeKind.Type)
                                    {
                                        Tags = match.Kind switch
                                        {
                                            EdgeKind.Sends => ImmutableArray.Create(RoleTags.Command),
                                            EdgeKind.Raises => ImmutableArray.Create(RoleTags.DomainEvent),
                                            _ => ImmutableArray<string>.Empty,
                                        },
                                        Layer = match.Kind switch
                                        {
                                            EdgeKind.Sends => "Application",
                                            EdgeKind.Raises => "Domain",
                                            _ => null,
                                        },
                                    });
                                }

                                g.AddEdge(new GraphEdge(node.Id, targetId, match.Kind)
                                {
                                    Provenance = match.Provenance,
                                    Resolution = resolved.Tier == ResolutionTier.Semantic
                                        ? Resolution.Semantic
                                        : Resolution.Syntactic,
                                    Confidence = match.Confidence,
                                });
                            }
                        }
                        catch { /* detector failure → skip its matches for this lambda */ }
                    }
                }
            }
            catch { /* parse failure → skip */ }
        }
    }

    /// <summary>Re-attaches semantic types (Tier B) onto facts re-parsed from a lambda body, matched by
    /// (file, expression text, short type). Upgrade-only (Law R2): a syntactic ref whose text+type matches a
    /// whole-file semantic bind is lifted to Semantic; everything else is untouched. Covers dispatch via a
    /// <c>var</c> local (<see cref="LocalDeclOp.InferredFrom"/>), inline argument (<see cref="ArgFact.Type"/>),
    /// and generic type arguments (<see cref="InvocationOp.GenericArgs"/>).</summary>
    private static ImmutableArray<BodyFacts> OverlaySemanticLocals(
        ImmutableArray<BodyFacts> facts, string filePath,
        Dictionary<(string File, string Text, string Type), Graph2.SymbolRef> semanticRefs)
    {
        if (semanticRefs.Count == 0) return facts;

        var result = ImmutableArray.CreateBuilder<BodyFacts>(facts.Length);
        foreach (var body in facts)
        {
            var ops = body.Ops;
            var changed = false;
            for (var i = 0; i < ops.Length; i++)
            {
                switch (ops[i])
                {
                    case LocalDeclOp { InferredFrom: { Tier: not ResolutionTier.Semantic } inf } local
                        when semanticRefs.TryGetValue((filePath, local.Name, inf.Text), out var sem):
                        ops = ops.SetItem(i, local with { InferredFrom = sem });
                        changed = true;
                        break;

                    case InvocationOp inv:
                    {
                        var newInv = inv;
                        var invChanged = false;

                        if (!inv.Args.IsDefaultOrEmpty)
                        {
                            var args = inv.Args;
                            var argsChanged = false;
                            for (var ai = 0; ai < args.Length; ai++)
                            {
                                if (args[ai].Type is { Tier: not ResolutionTier.Semantic } at
                                    && semanticRefs.TryGetValue((filePath, args[ai].Text, at.Text), out var semArg))
                                { args = args.SetItem(ai, args[ai] with { Type = semArg }); argsChanged = true; }
                            }
                            if (argsChanged) { newInv = newInv with { Args = args }; invChanged = true; }
                        }

                        if (inv.GenericArgs.Length > 0)
                        {
                            var gargs = inv.GenericArgs;
                            var gargsChanged = false;
                            for (var gi = 0; gi < gargs.Length; gi++)
                            {
                                if (gargs[gi].Tier != ResolutionTier.Semantic
                                    && semanticRefs.TryGetValue((filePath, gargs[gi].Text, gargs[gi].Text), out var semG))
                                { gargs = gargs.SetItem(gi, semG); gargsChanged = true; }
                            }
                            if (gargsChanged) { newInv = newInv with { GenericArgs = gargs }; invChanged = true; }
                        }

                        if (invChanged) { ops = ops.SetItem(i, newInv); changed = true; }
                        break;
                    }
                }
            }
            result.Add(changed ? body with { Ops = ops } : body);
        }
        return result.ToImmutable();
    }

    /// <summary>Ensures a Member node exists in the graph for the given id (first-write wins).</summary>
    private static void EnsureMemberId(CodeGraphBuilder g, NodeId id, string? memberName, string? file, string? project, int? line = null)
    {
        if (g.HasNode(id)) return;
        g.AddNode(new GraphNode(id, memberName ?? id.Key, NodeKind.Member)
        {
            FilePath = file,
            Project = project,
            LineNumber = line is > 0 ? line : null,
        });
    }


    /// <summary>True for a self-call target that is syntactic-resolver noise, not real wiring: the
    /// <c>nameof</c> pseudo-call, and the common ASP.NET <c>ControllerBase</c> result helpers (inherited,
    /// not declared on the controller) that resolve to <c>this</c> (Iteration 4 noise polish).</summary>
    private static bool IsSelfCallNoise(string method)
        => method is "nameof"
            or "Ok" or "NotFound" or "BadRequest" or "NoContent" or "Created" or "CreatedAtAction"
            or "CreatedAtRoute" or "Accepted" or "Unauthorized" or "Forbid" or "StatusCode"
            or "Content" or "Json" or "Redirect" or "RedirectToAction" or "File" or "ValidationProblem"
            or "Problem" or "Conflict" or "UnprocessableEntity";

    /// <summary>True for a node that represents a MediatR request (a Type tagged command/query/
    /// notification) � the targets a pipeline behavior wraps. Replaces the old NodeKind.Request check.</summary>
    private static bool IsRequestNode(GraphNode n)
        => n.Kind == NodeKind.Type
            && (n.Tags.Contains(RoleTags.Command)
                || n.Tags.Contains(RoleTags.Query)
                || n.Tags.Contains(RoleTags.Notification));

    /// <summary>Strips generic type arguments: <c>List&lt;int&gt;</c>?"List". Used in handler-joins, DI resolution, and type-name heuristics.</summary>
    private static string StripGenerics(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx > 0 ? typeName[..idx].TrimEnd() : typeName.TrimEnd();
    }

    /// <summary>True for an EndpointDetection that is a framework/infrastructure pseudo-entry � OpenAPI/Scalar
    /// root routes registered in ServiceDefaults or extension files � not genuine application surface. The
    /// guard matches on both source and route, not just <c>"/"</c>, so a real root route isn't falsely dropped.</summary>
    internal static bool IsInfrastructureEntry(EndpointDetection ep)
    {
        if (ep.RouteTemplate is "/" or "" or "/index.html" or "/openapi" or "/scalar")
        {
            var f = ep.SourceFile.AsSpan();
            if (f.Contains("ServiceDefaults", StringComparison.OrdinalIgnoreCase)
                || f.Contains("OpenApi", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Normalizes a route template for dedup comparison.</summary>
    internal static string NormalizeRoute(string route) => route.TrimStart('/').TrimEnd('/');


}
