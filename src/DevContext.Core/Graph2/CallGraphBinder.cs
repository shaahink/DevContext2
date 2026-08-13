using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Graph2;

/// <summary>Produces <see cref="CallEdge"/>s from <see cref="BodyFacts"/> invocations resolved
/// through the <see cref="SymbolTable"/> (Batch A / R2 §2.A steps 2+3). This replaces
/// <c>CallGraphExtractor</c>'s private resolution universe — the second Roslyn compilation, the
/// short-name DI/fqn maps and their silent winners are gone. Resolution honesty:
/// <list type="bullet">
/// <item>Receiver types resolve through the one arity-aware, project-scoped SymbolTable;
/// Ambiguous → the edge is SKIPPED, never first-matched (Law R1). Semantic tiers ride in from the
/// Tier-B upgrade (the ONE compilation, with NuGet refs) — no second bind pass.</item>
/// <item>Interface receivers route to their implementation only on honest evidence: an unambiguous
/// production DI DirectBinding, else a sole in-solution implementor; otherwise the edge lands on
/// the interface itself.</item>
/// <item>Bare-identifier invocations join the caller type only when the type DECLARES the method —
/// inherited framework helpers (<c>Ok()</c>) and pseudo-calls (<c>nameof</c>) fail structurally,
/// which is what retired the IsSelfCallNoise deny-list.</item>
/// </list>
/// The entry-seeded closure scoping (D3 perf win) and the focus-BFS depth cap are KEPT: edges are
/// produced only for files reachable from the entry/focus seed, and a focused run walks the same
/// depth-limited BFS as before.</summary>
public static class CallGraphBinder
{
    private const int SeedlessBindFileCap = 100;
    private const int MaxClosureRounds = 16;

    public static void Bind(
        DiscoveryContext context,
        DiscoveryModel model,
        SymbolTable symbols,
        IReadOnlyList<BodyFacts> bodyFacts,
        NoiseFilter noise,
        CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var maxDepth = context.ActiveScenario.Pruning.MaxCallDepth;

        var typeByFqn = new Dictionary<string, TypeDiscovery>(StringComparer.Ordinal);
        foreach (var t in model.OrderedTypes) typeByFqn.TryAdd(t.Id, t);

        var diImpl = BuildDiImplMap(model, symbols, noise);
        var soleImpl = BuildSoleImplMap(model, symbols, noise);

        var bodiesByFile = new Dictionary<string, List<BodyFacts>>(StringComparer.OrdinalIgnoreCase);
        foreach (var body in bodyFacts)
        {
            if (string.IsNullOrEmpty(body.File)) continue;
            if (!bodiesByFile.TryGetValue(body.File, out var list))
                bodiesByFile[body.File] = list = [];
            list.Add(body);
        }

        var typeToFile = BuildTypeToFile(model);

        // ── Seed selection (KEEP: D3 entry-seeded closure) ─────────────────────────────────────
        var seedFiles = ResolveFocusSeedFiles(context, model);
        if (context.Analysis.FocusPoints.Count == 0)
        {
            seedFiles = EntrySeedFiles(model);
            if (seedFiles.Count == 0) seedFiles = ResolveFocusSeedFiles(context, model);
        }

        var allEdges = new List<CallEdge>();
        var bound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void BindFile(string file, HashSet<string>? calleeTypesOut)
        {
            if (!bodiesByFile.TryGetValue(file, out var bodies)) return;
            foreach (var body in bodies)
            {
                ct.ThrowIfCancellationRequested();
                var callerType = SymbolCanon.OwnerTypeOf(body.Member.Canonical);

                // Batch C (DC7) — call edges obey the SAME production rule as nodes. Without this, a type
                // excluded from the graph still emitted its calls, and the callee came back as a bare
                // ghost node: FluentValidation's ITestValidationContinuation was the graph's top hub with
                // nine edges, all of them from its own TestHelper extension methods. An edge whose origin
                // is not modelled cannot be evidence about the system.
                if (typeByFqn.TryGetValue(callerType, out var callerDecl) && !noise.IsProductionCode(callerDecl))
                    continue;
                var callerMethod = SymbolCanon.MemberNameOf(SymbolCanon.MemberKeyFromSymbolId(body.Member.Canonical));

                foreach (var op in body.Ops)
                {
                    if (op is not InvocationOp inv) continue;

                    var callee = ResolveCallee(inv, callerType, body.File, symbols, diImpl, soleImpl);
                    if (callee is not { } c) continue;

                    allEdges.Add(new CallEdge(callerType, callerMethod, c.Type, inv.MethodName,
                        $"{body.File}:{inv.Line}")
                    {
                        Resolution = c.Resolution,
                    });
                    calleeTypesOut?.Add(c.Type);
                }
            }
        }

        if (seedFiles.Count > 0)
        {
            // Seed the closure with seam-landing files too (MediatR handlers, workers): a trace
            // crosses Send→Handler seams, so the handler's call edges must exist or it truncates.
            var frontier = new HashSet<string>(seedFiles, StringComparer.OrdinalIgnoreCase);
            foreach (var f in SeamLandingFiles(model, typeToFile)) frontier.Add(f);

            for (var round = 0; round < MaxClosureRounds && frontier.Count > 0; round++)
            {
                var toBind = frontier.Where(f => !bound.Contains(f) && bodiesByFile.ContainsKey(f)).ToList();
                foreach (var f in toBind) bound.Add(f);

                var callees = new HashSet<string>(StringComparer.Ordinal);
                foreach (var f in toBind) BindFile(f, callees);

                var next = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in callees)
                {
                    if (typeByFqn.TryGetValue(c, out var td) && !string.IsNullOrEmpty(td.FilePath)
                        && !bound.Contains(td.FilePath))
                        next.Add(td.FilePath);
                    else if (typeToFile.TryGetValue(c, out var cf) && !bound.Contains(cf))
                        next.Add(cf);
                }
                frontier = next;
            }

            model.AddDiagnostic(DiagnosticLevel.Info, "CallGraphBinder",
                $"Entry-scoped call graph: bound {bound.Count} of {bodiesByFile.Count} files from the handler seed.");
        }
        else if (bodiesByFile.Count is > 0 and <= SeedlessBindFileCap)
        {
            foreach (var f in bodiesByFile.Keys) { bound.Add(f); BindFile(f, null); }
            model.AddDiagnostic(DiagnosticLevel.Info, "CallGraphBinder",
                $"No entry/focus seed — bound all {bodiesByFile.Count} files (seedless fallback).");
        }

        // ── BFS depth cap from the start keys (unchanged contract) ─────────────────────────────
        var adjacency = new Dictionary<string, List<CallEdge>>(StringComparer.Ordinal);
        foreach (var edge in allEdges)
        {
            var key = SymbolCanon.MemberKey(edge.CallerType, edge.CallerMethod);
            if (!adjacency.TryGetValue(key, out var list))
                adjacency[key] = list = [];
            list.Add(edge);
        }

        var startKeys = GetStartKeys(context, model);
        var bfsDepth = new Dictionary<string, int>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        var includedEdges = new List<CallEdge>();

        foreach (var key in startKeys)
            if (bfsDepth.TryAdd(key, 0))
                queue.Enqueue(key);

        while (queue.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var currentKey = queue.Dequeue();
            var depth = bfsDepth[currentKey];
            if (depth >= maxDepth) continue;
            if (!adjacency.TryGetValue(currentKey, out var edges)) continue;

            foreach (var edge in edges)
            {
                includedEdges.Add(edge);
                var calleeKey = SymbolCanon.MemberKey(edge.CalleeType, edge.CalleeMethod);
                if (bfsDepth.TryAdd(calleeKey, depth + 1))
                    queue.Enqueue(calleeKey);
            }
        }

        model.CallEdges.Clear();
        foreach (var edge in includedEdges)
            model.CallEdges.Add(edge);
        model.SealDeterministicOrder(); // canonical call-site order (D5.3 — source order is semantic)

        var callGraphAdj = new Dictionary<string, ImmutableArray<CallEdge>>();
        foreach (var edge in model.CallEdges)
        {
            // Legacy CallGraph keys stay "Type.Method" — display-only consumers split on '.'.
            var key = $"{edge.CallerType}.{edge.CallerMethod}";
            callGraphAdj[key] = callGraphAdj.TryGetValue(key, out var existing)
                ? existing.Add(edge)
                : [edge];
        }
        context.Analysis.CallGraph = new CallGraph(callGraphAdj);

        sw.Stop();
        var semanticCount = includedEdges.Count(e => Graph.EdgeConfidence.IsVerified(e.Resolution)); // V1.1 (#25)
        model.AddDiagnostic(DiagnosticLevel.Info, "CallGraphBinder",
            $"Built call graph: {includedEdges.Count} edges at depth ≤ {maxDepth} "
            + $"({semanticCount} semantic) from BodyFacts through SymbolTable in {sw.ElapsedMilliseconds}ms.");
    }

    /// <summary>Resolves one invocation to its callee type, honestly or not at all.</summary>
    private static (string Type, Graph.Resolution Resolution)? ResolveCallee(
        InvocationOp inv, string callerType, string file, SymbolTable symbols,
        Dictionary<string, string?> diImpl, Dictionary<string, string> soleImpl)
    {
        if (inv.ReceiverType is { } recv)
        {
            var resolved = symbols.Resolve(recv);
            if (resolved.Tier is ResolutionTier.Ambiguous or ResolutionTier.Unresolved)
                return null;                                   // Law R1: skip, never silent-win
            if (resolved.Resolved is not { Kind: SymbolKind.Type } sym)
                return null;                                   // member-canonical collision — not a type

            var calleeType = sym.Canonical;

            // Batch C (DC4) — receiver CHAIN hop. `a.B.C()` was bound to a's type, so an aggregator that
            // merely HOLDS the collaborator swallowed the call: eShop's
            // `_appEnvironmentService.OrderService.CreateOrderAsync(order)` bound to IAppEnvironmentService,
            // and the checkout command's target read as a bare DI interface. When the receiver's trailing
            // segment names a PROPERTY of the resolved receiver type, the call lands on that property's
            // type. Same honesty rule as everywhere else: the hop happens only when the property's declared
            // type resolves unambiguously, otherwise the receiver type stands.
            if (symbols.HopThroughProperty(calleeType, inv.ReceiverMember, recv.Site) is { } hopped)
                calleeType = hopped;
            if (symbols.IsInterface(calleeType))
            {
                if (diImpl.TryGetValue(calleeType, out var impl))
                {
                    // Batch C: conflicting registrations mean we cannot name the IMPLEMENTATION — they
                    // never meant we cannot name the call. Dropping the edge cost eShop's ClientApp its
                    // whole member-level call spine (every service is registered twice, real and mock,
                    // behind a UseMocks switch), which left its [RelayCommand] entries with nothing but
                    // a bare type seam to point at. Landing on the interface is the honest middle: the
                    // call really is to IOrderService.CreateOrderAsync, and the graph's Resolves edges
                    // still carry the candidate implementations.
                    if (impl is not null) calleeType = impl;
                }
                else if (soleImpl.TryGetValue(calleeType, out var sole))
                {
                    calleeType = sole;
                }
                // else: keep the interface — the graph's Resolves edges carry the candidates
            }

            var resolution = resolved.Tier == ResolutionTier.Semantic
                ? Graph.Resolution.Semantic
                : Graph.Resolution.Syntactic;
            return (calleeType, resolution);
        }

        // Bare-identifier / this-call: joins the caller type only when the method is DECLARED on it.
        if (inv.ReceiverText is null or "this")
        {
            return symbols.TypeDeclaresMember(callerType, inv.MethodName)
                ? (callerType, Graph.Resolution.Syntactic)
                : null;
        }

        // E1.1 (#11) — STATIC call through a TYPE-NAME receiver: `BodyFactExtractor.Extract(root, …)`
        // resolves from no local scope, so it used to fall off the end here with no edge at all and the
        // whole static-utility layer of a repo had no in-edges. One shared rule with the seam producer
        // (SymbolTable.ResolveStaticReceiverType) — unambiguous, in-solution, declares-the-method.
        if (symbols.ResolveStaticReceiverType(inv, file) is { } staticType)
            return (staticType, Graph.Resolution.Syntactic);

        return null; // receiver exists but its type never resolved from scope — unknown, skip
    }

    /// <summary>Honest DI routing map: interface canonical → implementation canonical, or null when
    /// registrations conflict. Same DirectBinding + production-wins + deterministic-order discipline
    /// as <c>GraphBuilder.AddDiResolves</c>; both texts resolve through the SymbolTable with the
    /// registration site's project context (no cross-project short-name capture).</summary>
    private static Dictionary<string, string?> BuildDiImplMap(
        DiscoveryModel model, SymbolTable symbols, NoiseFilter noise)
    {
        var productionRegistered = new HashSet<string>(StringComparer.Ordinal);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (di.Shape != DiRegistrationShape.DirectBinding) continue;
            if (!noise.IsProductionEntrySource(di.SourceFile)) continue;
            productionRegistered.Add(symbols.ResolveName(di.ServiceType, di.SourceFile));
        }

        var map = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>()
            .Where(d => d.Shape == DiRegistrationShape.DirectBinding
                && !string.IsNullOrEmpty(d.ImplementationType)
                && d.ImplementationType != "?"
                && !d.ImplementationType.StartsWith("sp =>")
                && !d.ImplementationType.StartsWith("_ =>")
                && !d.ImplementationType.StartsWith("(")
                && !d.ImplementationType.Contains("GetRequiredService"))
            .OrderBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber))
        {
            var svc = symbols.ResolveName(di.ServiceType, di.SourceFile);
            if (!symbols.IsKnownFqn(svc)) continue;            // service didn't resolve — can't attribute
            if (!noise.IsProductionEntrySource(di.SourceFile) && productionRegistered.Contains(svc))
                continue;                                      // production wins over test-only bindings

            var impl = symbols.ResolveName(di.ImplementationType, di.SourceFile);
            if (!symbols.IsKnownFqn(impl)) continue;

            if (map.TryGetValue(svc, out var existing))
            {
                if (existing is not null && !string.Equals(existing, impl, StringComparison.Ordinal))
                    map[svc] = null;                           // conflicting bindings — mark ambiguous
            }
            else
            {
                map[svc] = impl;
            }
        }
        return map;
    }

    /// <summary>Sole-implementor fallback: interface canonical → the single production in-scope type
    /// implementing it. Multiple implementors → absent (never guessed).</summary>
    private static Dictionary<string, string> BuildSoleImplMap(
        DiscoveryModel model, SymbolTable symbols, NoiseFilter noise)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        var ambiguous = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in model.OrderedTypes)
        {
            if (!noise.IsProductionCode(type)) continue;
            if (type.ImplementedInterfaces.IsDefaultOrEmpty) continue;
            foreach (var iface in type.ImplementedInterfaces)
            {
                var ifaceFqn = symbols.ResolveName(iface, type.FilePath);
                if (!symbols.IsKnownFqn(ifaceFqn)) continue;
                if (ambiguous.Contains(ifaceFqn)) continue;
                if (!map.TryAdd(ifaceFqn, type.Id))
                {
                    map.Remove(ifaceFqn);
                    ambiguous.Add(ifaceFqn);
                }
            }
        }
        return map;
    }

    /// <summary>Maps every type's canonical id and short name to its source file, for closure frontier
    /// expansion. Stable order so short-name first-wins is deterministic.</summary>
    private static Dictionary<string, string> BuildTypeToFile(DiscoveryModel model)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var t in model.OrderedTypes)
        {
            if (string.IsNullOrEmpty(t.FilePath)) continue;
            map[t.Id] = t.FilePath;
            map.TryAdd(t.Name, t.FilePath);
        }
        return map;
    }

    /// <summary>Files the focus points to — the seed for focus-scoped binding. Type/Method focus →
    /// the declaring type's file(s); Endpoint focus → the endpoint's file + handler type file.
    /// Empty when the focus can't be tied to a source file (→ seedless fallback).</summary>
    private static HashSet<string> ResolveFocusSeedFiles(DiscoveryContext context, DiscoveryModel model)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fp in context.Analysis.FocusPoints)
        {
            if (!string.IsNullOrEmpty(fp.TypeName))
            {
                foreach (var t in model.OrderedTypes)
                    if ((string.Equals(t.Name, fp.TypeName, StringComparison.OrdinalIgnoreCase)
                            || SymbolCanon.TypeIdMatches(t.Id, fp.TypeName, StringComparison.OrdinalIgnoreCase))
                        && !string.IsNullOrEmpty(t.FilePath))
                        files.Add(t.FilePath);
            }

            if (fp.Kind == FocusKind.Endpoint && !string.IsNullOrEmpty(fp.Route))
            {
                foreach (var ep in model.Detections.OfType<EndpointDetection>())
                {
                    if (!RouteMatches(ep, fp)) continue;
                    if (!string.IsNullOrEmpty(ep.SourceFile)) files.Add(ep.SourceFile);
                    foreach (var t in model.OrderedTypes)
                        if (string.Equals(t.Name, ep.HandlerType, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(t.FilePath))
                            files.Add(t.FilePath);
                }
            }
        }
        return files;
    }

    private static bool RouteMatches(EndpointDetection ep, FocusPoint fp)
    {
        static string Norm(string r) => "/" + r.Trim('/');
        var routeOk = string.Equals(Norm(ep.RouteTemplate), Norm(fp.Route!), StringComparison.OrdinalIgnoreCase);
        var verbOk = string.IsNullOrEmpty(fp.HttpMethod)
            || string.Equals(ep.HttpMethod, fp.HttpMethod, StringComparison.OrdinalIgnoreCase);
        return routeOk && verbOk;
    }

    /// <summary>All source files that declare an application entry surface — the seed for entry-scoped
    /// binding in Map mode. Catalog-driven: every <see cref="IEntrySurfaceDetection"/> contributes its
    /// file (a new AppEntry surface needs no edit here).</summary>
    private static HashSet<string> EntrySeedFiles(DiscoveryModel model)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in model.Detections)
            if (d is IEntrySurfaceDetection && !string.IsNullOrEmpty(d.SourceFile))
                files.Add(d.SourceFile);
        return files;
    }

    /// <summary>Files of types a trace can jump to across a non-call seam (MediatR handlers,
    /// scheduled jobs / hosted services).</summary>
    private static HashSet<string> SeamLandingFiles(DiscoveryModel model, Dictionary<string, string> typeToFile)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Add(string? typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return;
            var (baseName, _) = SymbolCanon.SplitGenericText(typeName);
            if (typeToFile.TryGetValue(baseName, out var f)) files.Add(f);
        }
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>()) Add(h.HandlerType);
        foreach (var w in model.Detections.OfType<BackgroundWorkerDetection>()) Add(w.ImplementationType);
        return files;
    }

    private static HashSet<string> GetStartKeys(DiscoveryContext context, DiscoveryModel model)
    {
        var startKeys = new HashSet<string>(StringComparer.Ordinal);

        if (context.Analysis.FocusPoints.Count > 0)
        {
            var focusFiles = context.Analysis.FocusPoints
                .Select(f => f.FilePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var type in model.OrderedTypes)
            {
                if (focusFiles.Contains(type.FilePath))
                    foreach (var method in type.Methods)
                        startKeys.Add(SymbolCanon.MemberKey(type.Id, method.Name));
            }
        }

        if (startKeys.Count == 0)
        {
            foreach (var type in model.OrderedTypes)
                foreach (var method in type.Methods)
                    startKeys.Add(SymbolCanon.MemberKey(type.Id, method.Name));
        }

        return startKeys;
    }
}
