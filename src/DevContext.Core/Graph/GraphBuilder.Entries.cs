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
    /// <summary>After the graph is assembled, resolve each entry's dispatch target (the command it
    /// sends or the handler it invokes) so the Map and the desktop picker can show "route → Target".
    /// Uses the entry's <see cref="EntryPoint.HandlerNode"/> (set during graph construction) to find
    /// the connected Type/Member node and its Sends edges.</summary>
    private static ImmutableArray<EntryPoint> EnrichEntryTargets(CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (entries.IsDefaultOrEmpty) return entries;
        var b = ImmutableArray.CreateBuilder<EntryPoint>(entries.Length);
        foreach (var e in entries)
        {
            var target = ResolveEntryTarget(graph, e)
                ?? ResolveOwningTypeFallback(graph, e);
            b.Add(e with { Target = target });
        }
        return b.ToImmutable();
    }

    /// <summary>When <see cref="ResolveEntryTarget"/> finds no dispatch target (e.g. a view-returning
    /// controller action with no service call and no MediatR send), fall back to the owning controller
    /// type — honest (it's the declaring type) and more useful than a blank drill-in hint (W8). E6: a
    /// minimal-API lambda with real work inside but no single named collaborator (every call was a
    /// data-access noise verb, or several tied on out-degree) says so — "inline (N calls)" — rather than
    /// naming the whole registration type, which the reader would mistake for a real handler.</summary>
    private static string? ResolveOwningTypeFallback(CodeGraph graph, EntryPoint entry)
    {
        if (entry.HandlerNode is not { } hn) return null;
        var handler = graph.Node(hn);
        if (handler is null) return null;

        if (handler.Kind == NodeKind.Type)
            // Batch C (DC4): a target that repeats what the entry title already says is not a target.
            // eShop's `GET /item/{itemId:int} [ItemPage] → ItemPage` spent a target slot telling the
            // reader the page is the page. Better an honest blank than a tautology.
            return IsSelfEvident(entry, handler.Title) ? null : handler.Title;

        if (handler.Kind == NodeKind.Member)
        {
            if (handler.Title.StartsWith("<lambda>", StringComparison.Ordinal))
            {
                var callCount = graph.OutEdges(handler.Id, EdgeKind.Calls).Length;
                if (callCount > 0) return $"inline ({callCount} call{(callCount == 1 ? "" : "s")})";
            }

            var typeKey = ExtractTypeKey(handler.Id.Key);
            var owner = graph.Node(NodeId.ForType(typeKey))?.Title;
            return owner is not null && IsSelfEvident(entry, owner) ? null : owner;
        }

        return null;
    }

    /// <summary>True when the proposed target adds nothing the entry's own title didn't already carry —
    /// the declaring type of a page/component/handler whose title names it. Whole-word containment, so
    /// "ItemPage" in "GET /item/{itemId:int} [ItemPage]" counts but a genuine collaborator does not.</summary>
    private static bool IsSelfEvident(EntryPoint entry, string target)
    {
        if (string.IsNullOrEmpty(target) || entry.Title is not { Length: > 0 } title) return false;
        var idx = title.IndexOf(target, StringComparison.Ordinal);
        while (idx >= 0)
        {
            var beforeOk = idx == 0 || !char.IsLetterOrDigit(title[idx - 1]);
            var end = idx + target.Length;
            var afterOk = end >= title.Length || !char.IsLetterOrDigit(title[end]);
            if (beforeOk && afterOk) return true;
            idx = title.IndexOf(target, idx + 1, StringComparison.Ordinal);
        }
        return false;
    }

    /// <summary>Resolves an entry's primary target by following the entry's Calls edge to the
    /// target node, then checking that node's Sends edges — same traversal the TraceBuilder uses.</summary>
    internal static string? ResolveEntryTarget(CodeGraph graph, EntryPoint entry)
    {
        if (entry.Node.Key.Contains("<dynamic>", StringComparison.Ordinal)) return null;

        // T2.3 mutating-verb guard: a write endpoint should not resolve to a getter collaborator.
        var isMutating = entry.HttpMethod is "POST" or "PUT" or "DELETE" or "PATCH";

        foreach (var call in graph.OutEdges(entry.Node, EdgeKind.Calls))
        {
            var node = graph.Node(call.To);
            if (node is null) continue;

            switch (node.Kind)
            {
                case NodeKind.Member:
                    // 1. CQRS dispatch (MediatR Send/Publish) — try FIRST so eShop entry→target is unchanged.
                    var msends = graph.OutEdges(node.Id, EdgeKind.Sends)
                        .Select(s => s.To).Distinct().ToList();
                    if (msends.Count == 1) return graph.Node(msends[0])?.Title;
                    if (msends.Count > 1 && entry.Title is { } mroute)
                        return MatchRouteToSend(mroute, msends, graph);
                    // 2. Primary service call — a handler that dispatches no request (a plain controller
                    //    action) resolves to the dominant in-scope service it calls. The action member's
                    //    own Calls edges are precise post member-origin (Iteration 1), so this takes
                    //    controllers from 0 → target without guessing via the whole class.
                    return ResolvePrimaryCall(graph, node, isMutating);
                case NodeKind.Type:
                    var sends = graph.OutEdges(node.Id, EdgeKind.Sends)
                        .Select(s => s.To).Distinct().ToList();
                    if (sends.Count == 1)
                        return graph.Node(sends[0])?.Title;
                    if (sends.Count > 1 && entry.Title is { } route)
                        return MatchRouteToSend(route, sends, graph);
                    return null;
            }
        }
        return null;
    }

    /// <summary>Resolves an entry whose handler dispatches no MediatR request (e.g. a plain controller
    /// action or a minimal-API lambda) to the primary service it calls: the dominant in-scope callee of
    /// the action <b>member</b>. Prefers a DI-resolved <c>service</c>-tagged callee, else the in-scope,
    /// non-self, non-framework callee with the most outgoing calls of its own (E6: a real collaborator
    /// keeps working, a data-access leaf doesn't). Returns its title (member form, e.g.
    /// "ProductService.GetByIdAsync"), or null when the action calls nothing meaningful — honest, never
    /// guessed via the whole class (member-origin made the action's own Calls edges precise, so the old
    /// <c>ResolveViaParentType</c> whole-type crutch is retired).</summary>
    private static string? ResolvePrimaryCall(CodeGraph graph, GraphNode member, bool isMutating)
    {
        var ownerTypeKey = ExtractTypeKey(member.Id.Key);
        GraphNode? serviceCallee = null;
        GraphNode? serviceCalleeType = null;
        string? serviceMember = null;
        GraphNode? bestFallback = null;
        GraphNode? bestFallbackType = null;
        var bestOutDegree = -1;
        string? skippedDataStore = null;
        // Batch E: when a call lands on a TYPE (a DI interface has no member nodes to land on), the
        // member the call site named rides on the edge. Kept beside the chosen callee so the target can
        // say WHICH method — the Orleans Dashboard cell inherited from S4.
        string? serviceEdgeMember = null;
        string? fallbackEdgeMember = null;
        var bestServiceOutDegree = -1;
        foreach (var call in graph.OutEdges(member.Id, EdgeKind.Calls))
        {
            var callee = graph.Node(call.To);
            if (callee is null) continue;

            var calleeTypeKey = callee.Kind == NodeKind.Member ? ExtractTypeKey(callee.Id.Key) : callee.Id.Key;
            // Skip self-calls (a controller action calling ControllerBase helpers like Ok()/NotFound(),
            // which the syntactic resolver attributes to `this`).
            if (string.Equals(calleeTypeKey, ownerTypeKey, StringComparison.Ordinal)) continue;

            // In-scope only: the callee's owning Type must be a declared type we own (non-null FilePath),
            // which excludes framework leaves.
            var calleeType = graph.Node(NodeId.ForType(calleeTypeKey));
            if (calleeType?.FilePath is null) continue;

            var calleeMemberName = callee.Kind == NodeKind.Member ? ExtractMemberName(callee.Id.Key) : null;

            // E6 / T2.3: a raw data-access call is an implementation detail, not the endpoint's meaning — skip
            // a DataStore-tagged callee (a DbContext) and bare EF/LINQ/object verbs. But remember the store:
            // if it is the ONLY meaningful collaborator, the endpoint has no service layer and we say so
            // ("direct data access (X)") rather than resolving to nothing or to a noise verb.
            if (calleeType.Tags.Contains(RoleTags.DataStore))
            {
                skippedDataStore ??= calleeType.Title;
                continue;
            }
            if (IsDataAccessNoiseMethod(calleeMemberName) || IsObjectNoiseMethod(calleeMemberName))
                continue;

            // Prefer a DI-resolved service (the action's real collaborator). T2.3 mutating-verb guard: a
            // mutating entry (POST/PUT/DELETE/PATCH) must not pick a getter (GetAll) when a non-getter service
            // callee exists on the same member — keep the first service callee, but upgrade a getter to a
            // non-getter when the verb mutates. Batch A: a TYPE-kind service callee (a PlainCall seam edge)
            // upgrades to a MEMBER-kind callee of the SAME type — the member names the actual method, and
            // seam edges land before call edges in insertion order, so without the upgrade the bare type
            // shadows "Service.Method" as the target.
            if (calleeType.Tags.Contains(RoleTags.Service))
            {
                var upgradesToMember = serviceCallee is { Kind: NodeKind.Type }
                    && callee.Kind == NodeKind.Member
                    && string.Equals(calleeTypeKey, serviceCalleeType!.Id.Key, StringComparison.Ordinal);
                // Batch E — the ORDERING residue inherited from S4: with two service collaborators on one
                // member, FIRST-WINS decided, and first is edge insertion order, which is not evidence.
                // eShop's CheckoutViewModel.CheckoutAsync named DialogService.ShowAlertAsync — a real call,
                // and the weaker of the two. The out-degree rule that already ranks non-service callees
                // ("a real collaborator keeps working, a leaf call doesn't") now ranks these too, so there
                // is ONE strength rule instead of a strength rule and an accident.
                var serviceOutDegree = graph.OutEdges(callee.Id, EdgeKind.Calls).Length;
                var strongerService = serviceCallee is not null
                    && !upgradesToMember
                    && serviceOutDegree > bestServiceOutDegree
                    // T2.3 still binds: on a mutating entry a getter never displaces a non-getter,
                    // however busy the getter is. Strength breaks ties; it does not overrule the verb.
                    && !(isMutating && IsGetter(calleeMemberName) && !IsGetter(serviceMember));
                if (serviceCallee is null
                    || upgradesToMember
                    || strongerService
                    || (isMutating && IsGetter(serviceMember) && !IsGetter(calleeMemberName)))
                {
                    bestServiceOutDegree = Math.Max(bestServiceOutDegree, serviceOutDegree);
                    serviceCallee = callee;
                    serviceCalleeType = calleeType;
                    serviceMember = calleeMemberName;
                    serviceEdgeMember = call.TargetMember;
                }
                continue;
            }

            // Non-service meaningful callee — remember the one with the highest out-degree of its own
            // (a real handler keeps working, a leaf call doesn't). Batch A: a MEMBER-kind callee of the
            // same type upgrades a TYPE-kind best (the seam edge lands first; the member names the method).
            var outDegree = graph.OutEdges(callee.Id, EdgeKind.Calls).Length;
            var upgradesFallback = bestFallback is { Kind: NodeKind.Type }
                && callee.Kind == NodeKind.Member
                && string.Equals(calleeTypeKey, bestFallbackType!.Id.Key, StringComparison.Ordinal);
            if (outDegree > bestOutDegree || upgradesFallback)
            {
                bestOutDegree = Math.Max(bestOutDegree, outDegree);
                bestFallback = callee;
                bestFallbackType = calleeType;
                fallbackEdgeMember = call.TargetMember;
            }
        }

        if (serviceCallee is not null)
            return TargetTitle(
                ResolveTypedClientTarget(graph, serviceCallee), serviceCalleeType!, serviceMember,
                serviceEdgeMember);
        if (bestFallback is not null)
            return TargetTitle(ResolveTypedClientTarget(graph, bestFallback), bestFallbackType!,
                bestFallback.Kind == NodeKind.Member ? ExtractMemberName(bestFallback.Id.Key) : null,
                fallbackEdgeMember);
        // No service/handler call — the endpoint accesses the data store directly; label it as such.
        return skippedDataStore is { } ds ? $"direct data access ({ds})" : null;
    }

    /// <summary>C6 (Prism D1.2f): a typed-HttpClient interface is plumbing, not a target — when a
    /// Type-kind callee has a single <see cref="EdgeKind.Resolves"/> edge tagged
    /// <see cref="RoleTags.HttpClientBinding"/>, name the implementation the route actually calls
    /// (podcasts' <c>PUT /feeds/{id}</c> read a bare "IFeedClient"). Domain-port interfaces
    /// (untagged AddScoped bindings, e.g. eShop's IOrderQueries) keep their contract display.</summary>
    private static GraphNode ResolveTypedClientTarget(CodeGraph graph, GraphNode callee)
    {
        if (callee.Kind != NodeKind.Type) return callee;
        var resolves = graph.OutEdges(callee.Id, EdgeKind.Resolves);
        if (resolves.Length != 1 || resolves[0].Tags.IsDefaultOrEmpty
            || !resolves[0].Tags.Contains(RoleTags.HttpClientBinding)) return callee;
        return graph.Node(resolves[0].To) ?? callee;
    }

    /// <summary>A read-only accessor name (<c>Get…</c>). A mutating endpoint should prefer a non-getter
    /// collaborator over one of these when both are called on the same member (T2.3 — POST /reset must not
    /// resolve to <c>Orchestrator.GetAll</c>).</summary>
    private static bool IsGetter(string? methodName)
        => methodName is { Length: > 0 } && methodName.StartsWith("Get", StringComparison.Ordinal);

    /// <summary>An entry target is always rendered <c>Type.Method</c> for a member callee. The semantic
    /// body-scan seams sometimes create a member node with a BARE method-name title (e.g. DntSite's
    /// auto-registered <c>FeedsService</c>, whose target read as an ownerless "GetNewsAsync"); its NodeId
    /// still encodes the owning type, so reconstruct the qualified name from the resolved type node so
    /// "FeedsService.GetNewsAsync" survives (T1.3). A callee whose title is already qualified — or a Type
    /// callee — keeps its own title.</summary>
    /// <param name="edgeMember">Batch E — the member the CALL SITE named, carried on the edge for a call
    /// that landed on a Type. A DI interface has no member nodes (its methods have no bodies), so
    /// without this the target is the bare interface: true, and the least useful true thing available.
    /// Only applied to a Type callee whose title doesn't already carry a member.</param>
    private static string TargetTitle(GraphNode callee, GraphNode calleeType, string? memberName,
        string? edgeMember = null)
    {
        if (callee.Kind == NodeKind.Member
            && memberName is { Length: > 0 }
            && !callee.Title.Contains('.', StringComparison.Ordinal))
            return $"{calleeType.Title}.{memberName}";

        // The member is only worth naming if it MEANS something. `context.CatalogTypes.OrderBy(...)
        // .ToListAsync()` names ToListAsync at the call site, and "CatalogContext.ToListAsync" is a
        // worse target than "CatalogContext" — which is exactly why the data-access deny-list exists
        // and why S4's redundancy probe answered KEEP BOTH. Same predicate, same judgement.
        if (callee.Kind == NodeKind.Type
            && edgeMember is { Length: > 0 }
            && !IsDataAccessNoiseMethod(edgeMember)
            && !IsObjectNoiseMethod(edgeMember)
            && !callee.Title.Contains('.', StringComparison.Ordinal))
            return $"{callee.Title}.{edgeMember}";

        return callee.Title;
    }

    /// <summary>"TypeFqn::MethodName" → "MethodName" (the inverse of <see cref="ExtractTypeKey"/>).</summary>
    private static string ExtractMemberName(string memberKey) => SymbolCanon.MemberNameOf(memberKey);

    /// <summary>E6: bare EF Core / LINQ verbs — never a meaningful entry target on their own, whichever
    /// type the syntactic resolver happened to attribute the call to.</summary>
    private static readonly HashSet<string> _dataAccessNoiseMethods = new(StringComparer.Ordinal)
    {
        "Where", "Select", "SelectMany", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending",
        "Include", "ThenInclude", "Skip", "Take", "GroupBy", "Distinct",
        "Any", "AnyAsync", "All", "Count", "CountAsync", "Sum", "SumAsync", "Average",
        "First", "FirstAsync", "FirstOrDefault", "FirstOrDefaultAsync",
        "Single", "SingleAsync", "SingleOrDefault", "SingleOrDefaultAsync",
        "ToList", "ToListAsync", "ToArray", "ToArrayAsync", "ToDictionary", "ToDictionaryAsync",
        "Find", "FindAsync", "Add", "AddAsync", "AddRange", "AddRangeAsync",
        "Remove", "RemoveRange", "Update", "UpdateRange", "SaveChanges", "SaveChangesAsync",
        "Attach", "AsNoTracking", "AsQueryable", "AsEnumerable",
    };

    private static bool IsDataAccessNoiseMethod(string? methodName)
        => methodName is not null && _dataAccessNoiseMethods.Contains(methodName);

    /// <summary>System.Object/lifetime plumbing — calling <c>service.ToString()</c> must never make
    /// that service the entry's target (seen live: "GET /api/ctrader/listen → CTraderListenService.ToString").</summary>
    private static bool IsObjectNoiseMethod(string? methodName)
        => methodName is "ToString" or "GetHashCode" or "Equals" or "GetType" or "Dispose" or "DisposeAsync";

    /// <summary>"TypeFqn::MethodName" → "TypeFqn" (strips the member segment from a Member key).</summary>
    private static string ExtractTypeKey(string memberKey) => SymbolCanon.OwnerTypeOf(memberKey);

    /// <summary>When a registration type dispatches many commands (minimal APIs), match an entry's
    /// route to the most likely request by extracting the last significant route segment and finding
    /// the Send target whose request name contains it.</summary>
    private static string? MatchRouteToSend(string route, List<NodeId> sendTargets, CodeGraph graph)
    {
        // Extract the last significant segment: "POST /api/orders/" → "orders"
        var segment = route.TrimEnd('/');
        var lastSlash = segment.LastIndexOf('/');
        if (lastSlash >= 0)
            segment = segment[(lastSlash + 1)..];
        // Strip {params}: "orders/{orderId:int}" → "orders"
        var brace = segment.IndexOf('{');
        if (brace > 0) segment = segment[..brace];
        if (segment.Length < 2) return null;

        // Also try singular form (routes are often plural, type names singular)
        var singular = segment.EndsWith("s", StringComparison.OrdinalIgnoreCase)
            ? segment[..^1] : null;
        // HTTP-verb prefix hints: POST→Create, GET→Get/List, PUT→Update, DELETE→Delete
        var verb = route.AsSpan().TrimStart();
        var space = verb.IndexOf(' ');
        var httpVerb = space > 0 ? verb[..space].ToString() : "";

        string? best = null;
        foreach (var targetId in sendTargets)
        {
            var name = graph.Node(targetId)?.Title;
            if (name is null) continue;
            if (!name.Contains(segment, StringComparison.OrdinalIgnoreCase)
                && (singular is null || !name.Contains(singular, StringComparison.OrdinalIgnoreCase)))
                continue;

            // Prefer targets whose verb-derived prefix matches
            if (MatchesVerbPrefix(name, httpVerb))
                return name;
            best ??= name;
        }
        return best;
    }

    private static bool MatchesVerbPrefix(string name, string httpVerb) => httpVerb switch
    {
        "POST" => name.StartsWith("Create", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("Add", StringComparison.OrdinalIgnoreCase),
        "GET" => name.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
              || name.StartsWith("List", StringComparison.OrdinalIgnoreCase)
              || name.StartsWith("Find", StringComparison.OrdinalIgnoreCase),
        "PUT" => name.StartsWith("Update", StringComparison.OrdinalIgnoreCase),
        "DELETE" => name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Remove", StringComparison.OrdinalIgnoreCase),
        "PATCH" => name.StartsWith("Update", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Patch", StringComparison.OrdinalIgnoreCase),
        _ => false,
    };

    /// <summary>L3.6 — Derives a project-relative GroupPath for each entry. Uses the handler type's
    /// namespace (resolved via NameResolver) and strips the project's typical root-namespace prefix
    /// to produce a grouping key like "Controllers/Orders" or "Services/Ordering".</summary>
    private static ImmutableArray<EntryPoint> EnrichEntryGroupPaths(
        ImmutableArray<EntryPoint> entries, SymbolTable names, SolutionScope scope)
    {
        if (entries.IsDefaultOrEmpty) return entries;
        var b = ImmutableArray.CreateBuilder<EntryPoint>(entries.Length);
        foreach (var e in entries)
        {
            var gp = DeriveGroupPath(e, names, scope);
            b.Add(e with { GroupPath = gp });
        }
        return b.ToImmutable();
    }

    private static string? DeriveGroupPath(EntryPoint entry, SymbolTable names, SolutionScope scope)
    {
        // 1. Resolve the handler type's FQN (via HandlerNode or by parsing Provenance)
        string? ns = null;
        string? project = null;

        // Extract project name from provenance (file:line string)
        if (entry.Provenance is { } provenance)
        {
            var colon = provenance.LastIndexOf(':');
            var filePath = colon > 0 ? provenance[..colon] : provenance;
            project = scope.ProjectForFile(filePath);
        }

        // T1.6 — HTTP feature areas come from the ROUTE first, not the handler namespace. Grouping every
        // endpoint under its shared "…Api" namespace collapsed 128 shamshir endpoints into one useless
        // "Api (128 entries)" module row; the route's first meaningful segment is the real feature
        // (/api/addons/* → addons, /api/orders/* → orders). Namespace/folder still groups non-HTTP entries.
        if (entry.Kind == EntryPointKind.HttpEndpoint && entry.Route is { } route
            && HttpRouteGroupPath(route) is { } routeGroup)
            return routeGroup;

        if (entry.HandlerNode is { } hn)
        {
            var fqn = ExtractTypeKey(hn.Key);
            ns = names.GetNamespace(fqn);
        }

        if (ns is null) return project;

        // Derive GroupPath from namespace, stripping project-root prefix
        return NamespaceGroupPath(ns, project);
    }

    /// <summary>Derives a GroupPath from the last 1-2 meaningful namespace segments, stripping
    /// common project/root prefixes (e.g. "MyApp.Api.Controllers.Orders" → "Controllers/Orders"
    /// when project is "MyApp.Api").</summary>
    private static string? NamespaceGroupPath(string ns, string? project)
    {
        var parts = ns.Split('.');
        if (parts.Length <= 1) return ns;

        // Find where the namespace diverges from the project (typically namespaces mirror projects)
        var start = 0;
        if (project is not null)
        {
            var projParts = project.Split('.');
            for (var i = 0; i < Math.Min(parts.Length, projParts.Length); i++)
            {
                if (string.Equals(parts[i], projParts[i], StringComparison.OrdinalIgnoreCase))
                    start = i + 1;
                else break;
            }
        }

        // Take the remaining meaningful segments, skip "Controllers"/"Endpoints" as redundant
        var remaining = parts[start..];
        if (remaining.Length == 0) return project;
        if (remaining.Length == 1) return remaining[0];

        // Skip the ubiquitous first segment if it's a well-known structural layer marker
        if (remaining.Length >= 2
            && (remaining[0] == "Controllers" || remaining[0] == "Endpoints"
                || remaining[0] == "Handlers" || remaining[0] == "Services"
                || remaining[0] == "Consumers" || remaining[0] == "Hubs"))
            return string.Join("/", remaining[1..]);

        return string.Join("/", remaining);
    }

    /// <summary>T1.6 — Derives the FEATURE-AREA GroupPath from an HTTP route: the first meaningful path
    /// segment, skipping the "api" prefix, version segments (v1, v2.0), and route parameters
    /// (e.g. "GET /api/orders/{id}" → "orders", "POST /api/v2/addons" → "addons"). Returns null for a
    /// route with no meaningful segment (e.g. "/") so the caller can fall back to namespace/project.</summary>
    private static string? HttpRouteGroupPath(string route)
    {
        var space = route.IndexOf(' ');
        var path = space > 0 ? route[(space + 1)..] : route;
        foreach (var seg in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (seg.StartsWith('{')) continue;                                   // route parameter
            if (seg.Equals("api", StringComparison.OrdinalIgnoreCase)) continue; // ubiquitous api prefix
            if (IsRouteVersionSegment(seg)) continue;                            // v1, v2, v2.0
            return seg.ToLowerInvariant();                                       // first meaningful segment = feature
        }
        return null;
    }

    /// <summary>True for an API-version route segment like "v1" or "v2.0" (letter v + digit).</summary>
    private static bool IsRouteVersionSegment(string s)
        => s.Length >= 2 && s[0] is 'v' or 'V' && char.IsDigit(s[1]);

    /// <summary>L3.2 — Computes graph-aware scores for each entry: BFS from the entry's node outward
    /// through Calls/Sends edges to count reach, seam richness, entity touches, and cross-project depth.
    /// Produces a composite 0..1 score for ranking.</summary>
    private static ImmutableArray<EntryPoint> EnrichEntryScores(
        ImmutableArray<EntryPoint> entries, CodeGraph graph, SolutionScope scope)
    {
        if (entries.IsDefaultOrEmpty) return entries;

        var maxReach = 0d;
        var maxSeam = 0d;
        var maxEntity = 0d;
        var (reach, seam, ent, xProjects) = ScoreEntries(entries, graph, scope);

        if (reach.Length > 0) { maxReach = reach.Max(); maxSeam = Math.Max(maxSeam, seam.Max()); maxEntity = Math.Max(maxEntity, ent.Max()); }

        var b = ImmutableArray.CreateBuilder<EntryPoint>(entries.Length);
        for (var i = 0; i < entries.Length; i++)
        {
            var normReach = maxReach > 0 ? reach[i] / maxReach : 0;
            var normSeam = maxSeam > 0 ? seam[i] / maxSeam : 0;
            var normEntity = maxEntity > 0 ? ent[i] / maxEntity : 0;
            var normProj = reach.Length > 0 ? xProjects[i] / Math.Max(xProjects.Max(), 1) : 0;

            var score = normReach * 0.4 + normSeam * 0.3 + normEntity * 0.2 + normProj * 0.1;
            b.Add(entries[i] with
            {
                Score = Math.Round(score, 3),
                Reach = reach[i],
                SeamRichness = seam[i],
                EntityTouches = ent[i],
                CrossProjects = xProjects[i],
            });
        }
        return b.ToImmutable();
    }

    private static (int[] Reach, int[] Seam, int[] Entity, int[] XProj) ScoreEntries(
        ImmutableArray<EntryPoint> entries, CodeGraph graph, SolutionScope scope)
    {
        var n = entries.Length;
        var reach = new int[n];
        var seam = new int[n];
        var entity = new int[n];
        var xProj = new int[n];

        for (var i = 0; i < n; i++)
        {
            var (r, s, e, x) = BfsEntryScore(graph, entries[i], scope);
            reach[i] = r;
            seam[i] = s;
            entity[i] = e;
            xProj[i] = x;
        }
        return (reach, seam, entity, xProj);
    }

    private static (int Reach, int Seam, int Entity, int XProj) BfsEntryScore(CodeGraph graph, EntryPoint entry, SolutionScope scope)
    {
        var visited = new HashSet<NodeId>();
        var queue = new Queue<(NodeId, int)>();
        queue.Enqueue((entry.Node, 0));
        visited.Add(entry.Node);

        var reach = 0;
        var seam = 0;
        var entity = 0;
        var projects = new HashSet<string>();

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth > 6) continue;
            if (current != entry.Node) reach++;

            foreach (var edge in graph.OutEdges(current))
            {
                // Batch D (R2 §2.D): ONE node lookup per edge. This loop used to call graph.Node(edge.To)
                // twice for the same edge — once for the entity check, once for the project check —
                // which is a frozen-dictionary probe per entry per edge, per BFS.
                var target = graph.Node(edge.To);

                // Track seam richness
                if (edge.Kind is EdgeKind.Sends or EdgeKind.Raises or EdgeKind.Consumes)
                    seam++;
                if (edge.Kind == EdgeKind.ReadsWrites
                    && target is not null
                    && (target.Tags.Contains(RoleTags.Entity) || target.Tags.Contains(RoleTags.Aggregate)))
                    entity++;
                // Track cross-project: resolve the owning project from the target node's file path.
                if (target?.FilePath is { } fp)
                {
                    var proj = scope.ProjectForFile(fp) ?? target.Project ?? Path.GetFileNameWithoutExtension(fp);
                    if (proj is not null) projects.Add(proj);
                }

                if (visited.Add(edge.To) && depth < 6)
                    queue.Enqueue((edge.To, depth + 1));
            }
        }

        return (reach, seam, entity, projects.Count);
    }

}
