namespace DevContext.Core.Graph;

/// <summary>
/// Batch E (R2 §2.E item 1) — THE trace contract. One seam pick order, one framework-leaf filter, one
/// set of dials, one budget rule, read by every surface that answers "what happens when this entry
/// runs": the precomputed <see cref="Flow"/> spine built during assembly, the render-time
/// <see cref="TraceBuilder"/> tree, and the CLI / gRPC / MCP callers of <see cref="GraphQuery.Trace"/>.
/// <para>Before this module those answers came from two INDEPENDENT tables. The spine ranked
/// <see cref="EdgeKind.ServiceLink"/> third — a cross-service hop is the whole point of a distributed
/// trace — while the tree left it in the catch-all bucket beside <see cref="EdgeKind.Calls"/>, the
/// lowest rank there is. So on a repo like eShop the map's flow could cross into another service while
/// the trace of the SAME entry, at a fan-out boundary, silently dropped that hop. Two tables, two
/// stories, one entry. That is the defect this module exists to make impossible.</para>
/// <para>The unified order is the spine's, because it is the one that was reasoned about
/// (dispatch → handler → cross-service → events → data → wiring). Adopting it changes the TREE only:
/// ServiceLink is promoted out of the catch-all. Relative order is otherwise preserved on both sides.</para>
/// </summary>
public static class TracePolicy
{
    /// <summary>Hops from the entry when the caller doesn't say. Every surface defaults here.</summary>
    public const int DefaultDepth = 6;

    /// <summary>Children expanded per node when the caller doesn't say.</summary>
    public const int DefaultFanOut = 12;

    /// <summary>The document budget a trace is shaped to when the caller names none. 0 means the
    /// caller asked for the full tree, and is honoured; this is only what "unspecified" resolves to.
    /// <para>R4 §1 item 12 — before this constant existed, "unspecified" meant three different
    /// things: the MCP tool hard-coded 4000, <c>query --op trace</c> ran unbudgeted, and the gRPC
    /// server treated an absent field as unlimited. Same focus, same engine, three sizes of answer.
    /// The value is 4000 because that is the only trace-SPECIFIC default the product has ever
    /// shipped — the CLI's 8000 is a whole-DOCUMENT budget (<c>RenderRequest.MaxTokens</c>) a caller
    /// passes explicitly, out of which <see cref="TreeBudget"/> already carves the tree's share.</para></summary>
    public const int DefaultBudgetTokens = 4000;

    /// <summary>Ceiling for budget-elastic deepening (see <see cref="ElasticDepth"/>). A trace deeper
    /// than this stops being a story and becomes a dump, whatever the budget allows.</summary>
    public const int MaxElasticDepth = 12;

    /// <summary>Tokens the rendered document spends AROUND the tree — TOUCHES/EMITS, hints, the
    /// diagnostics tail. Measured ~1.2k on the bitwarden exemplar (Prism D3). Reserved out of the
    /// caller's budget, or the shaped tree fits and the document still overshoots.</summary>
    public const int RenderReserveTokens = 1200;

    /// <summary>Floor for the tree's share of a budget — below this the shaping cuts everything and the
    /// answer is an empty trace, which helps nobody.</summary>
    public const int MinTreeBudget = 1000;

    /// <summary>The tree's token budget given the document's. 0 (or less) means unlimited: shaping off.</summary>
    public static int TreeBudget(int documentBudgetTokens)
        => documentBudgetTokens <= 0 ? 0 : Math.Max(MinTreeBudget, documentBudgetTokens - RenderReserveTokens);

    /// <summary>THE seam pick order — lower wins. Used both to rank a trace node's branches and to pick
    /// the single next step of a flow spine.</summary>
    public static int SeamPriority(EdgeKind kind) => kind switch
    {
        EdgeKind.Sends => 0,        // dispatch is the core story
        EdgeKind.Handles => 1,      // the handler is the response
        EdgeKind.ServiceLink => 2,  // crossing a service boundary outranks anything in-process
        EdgeKind.Raises => 3,       // events
        EdgeKind.Consumes => 4,
        EdgeKind.ReadsWrites => 5,  // data access
        EdgeKind.Resolves => 6,     // DI wiring
        EdgeKind.WrappedBy => 7,    // pipeline wrappers (never on the spine, see NotOnSpine)
        _ => 8,                     // Calls — lowest: most likely to be framework noise
    };

    /// <summary>Edge kinds a SPINE never follows. They are real edges the tree still shows (a pipeline
    /// behavior, an entity relation), but they are not "what happens next" — following one would make
    /// the flow's single path stop being a path through the dispatch.</summary>
    public static bool IsOnSpine(EdgeKind kind)
        => kind is not (EdgeKind.WrappedBy or EdgeKind.EntityRelation or EdgeKind.DependsOn or EdgeKind.Exposes);

    /// <summary>THE framework-boundary stop. A step onto one of these is rendered but never descended:
    /// walking into Microsoft.*/System.* internals produces depth without meaning.
    /// <para>Batch A removed the old <c>*Mediator*</c> Contains-overfit — the honest binder never
    /// produces edges onto out-of-solution mediator types — so only literal framework names remain.</para>
    /// A null node counts as a leaf: an edge whose target is not in the graph cannot be walked.</summary>
    public static bool IsFrameworkLeaf(GraphNode? node)
    {
        if (node is null) return true;
        var title = node.Title;
        return title.StartsWith("Microsoft.", StringComparison.Ordinal)
            || title.StartsWith("System.", StringComparison.Ordinal)
            || title == "DbContext"
            || title is "ILogger" or "IMediator" or "ISender" or "IPublisher";
    }

    /// <summary>Budget-elastic depth: how deep to walk when the caller gave a budget but no explicit
    /// depth. A fixed default is wrong in both directions — it truncates a small entry that had room to
    /// spare, and it over-walks a hub whose tree will be cut back anyway.
    /// <para>Elasticity only ever DEEPENS, and only when the first walk both hit the depth limit and
    /// used a small share of the budget; the shaper still trims whatever comes back. Callers that name a
    /// depth get exactly that depth — an explicit dial is not a suggestion.</para></summary>
    public static int ElasticDepth(int builtDepth, int usedTokens, int treeBudget, bool hitDepthLimit)
    {
        if (treeBudget <= 0 || !hitDepthLimit) return builtDepth;
        if (usedTokens * 2 > treeBudget) return builtDepth;      // already using half the budget
        return Math.Min(MaxElasticDepth, builtDepth + 3);
    }
}
