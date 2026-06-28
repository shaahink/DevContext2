# Product Direction — The Universal .NET Repo Lens

> North-star product definition (2026-06-28). Supersedes the earlier "narrow to a
> MediatR/indirection niche" positioning. Companion to `DESIGN-PHILOSOPHY.md` (principles) and
> `IDEAL-OUTPUT-TARGET.md` (the artifact shapes). This doc is what every fix and feature gets
> sequenced against. It is a living draft — edit freely.

## 1. The vision (one line)

**The first thing you run on any .NET repo.** Point it at anything — a clone, a `.sln`, or a GitHub
URL — and instantly *browse* it, *trace* any flow down the wiring, and pull *entry points + stats*.
For humans first, agents next. Think `ripgrep`/`ctags` for .NET: zero-config, fast, always works —
but **semantic, trace-aware, and LLM-ready**.

The differentiator is not a feature; it's the *slot*: nobody owns "the universal .NET orientation
tool." The smart trace-through-indirection is the **capability** that makes the lens better than
`grep`, better than the IDE's go-to-definition, and better than an agent re-exploring from cold every
session. MediatR/CQRS is the *hardest proof* the engine is smart — never the identity. The same trace
must work on plain controllers and DI.

## 2. Principles (the discipline that keeps "universal" from rotting into the old catalog soup)

1. **Universalize coverage, keep the artifact set tight.** Breadth of *repos* — yes, every shape.
   Breadth of *output shapes* — no. We escaped "show any info / 15 disconnected catalogs" once;
   universal must not drag us back.
2. **Never show what the kernel can't honestly answer.** No fabricated edges, no confident-but-wrong
   `[verified]`. A universal tool that lies loses trust *faster* (more people, more shapes).
3. **Honest scope.** Always say *what was analyzed* (whole solution vs one project closure) and *how
   confident*. A default tool you reach for blindly must be self-aware about its blind spots.
4. **Bounding is structural, not budget-driven.** A trace is bounded by depth · fan-out · framework
   boundary — not by a token guillotine. (Token budgeting is deferred — see §8.)
5. **One kernel, many faces.** CLI, browse UI, and MCP are *query clients* over one queryable graph.
   Build the query layer once; the faces are thin.

## 3. The locked artifact set

Everything the product produces is one of these five. Each must answer a question a developer
actually asks on a fresh repo. Anything else does not ship.

| Artifact | Question it answers |
|---|---|
| **Entry points** | "How does control get *into* this system?" (the spine of everything else) |
| **Topology / architecture** | "What is this and where do things live?" |
| **Trace** | "How does *this* operation actually work, down the wiring?" |
| **Stats / metadata** | "Give me the numbers — sizes, counts, hotspots — in one shot." |
| **Browse** | "Let me navigate the structure and jump around." |

**Explicitly out (for now):** anything that re-grows the old per-concern catalog tables as a
standalone output; token-budget trimming (§8); whole-program data-flow/taint; non-.NET languages.

## 4. Entry-point coverage ladder (this *is* the product, not a side feature)

Universal = works on every common .NET shape. Build coverage in this order; the "go-to" promise dies
on the common case first.

1. **Controllers** (ASP.NET MVC/Web API) — the most common shape, currently **0/94** entry→target
   resolution. **Highest priority.**
2. **Minimal APIs** — works partially today.
3. **MediatR / CQRS dispatch** — works today (the showcase). Keep it; don't lead with it.
4. **Hosted services / background workers / scheduled jobs** — detected; surface as entries.
5. **gRPC services, SignalR hubs.**
6. **Blazor components** (lifecycle + event handlers).
7. **Azure Functions / AWS Lambda triggers; console `Main`.**

Each rung ships with negative tests and an honest "covered / not covered" signal in output.

## 5. One kernel, three faces

Human browse UI **first**, designed so MCP and CLI are the same queries over the same graph.

| Face | Order | Access pattern | Notes |
|---|---|---|---|
| **Browse UI (human)** | 1st | Interactive: analyze once, query many | The redo. Navigate entries → topology → trace → node detail → stats. |
| **MCP / agent** | 2nd (designed-in now) | Programmatic: `list_entrypoints`, `trace`, `map`, `stats`, `find_usages`, `get_node` | Same query layer. The agent calls it precisely for what it can't compute itself. |
| **CLI** | exists | One-shot | Keep working; it's the smoke test for the query layer. |

**GitHub-URL path is in the v1 vision** ("paste any repo, get a map") — it's a strong discovery
surface. It rides on kernel v2 (the persistent index, §6) for fetch/clone/scale; it does **not** block
the human UI on that index.

## 6. The kernel = a queryable graph store ("analyze once, query many")

The kernel is **not a renderer** — it's a query layer over one immutable `CodeGraph`. Build it once,
against the faces' needs (§7), and the faces become thin. The good news: `CodeGraph` is already
immutable and serialization-clean, and the desktop already proves analyze-once/render-many.

Four requirements:

1. **Correct** — *member-origin edges*. Today call/raise/send/data edges attach to the **Type** node
   (one-node-per-class collapse), so a trace anchored on one method inherits every sibling method's
   edges — fabricated `[verified]` edges, wrong EMITS/TOUCHES, and (proven in the probe) *missing the
   real path*. Fix: edges originate from the **Member** that contains them; a controlled member bridge
   replaces the inherit-everything `OutEdgesWithTwin`; focus anchors on the Member node.
   (Locus: `GraphBuilder.AddCallEdges/AddRaises/AddSends/AddDataEdges`, `TraceBuilder.OutEdgesWithTwin`,
   entry resolution.)
2. **Covered** — the entry-point ladder (§4), controllers first.
3. **Bidirectional / navigable** — store **inverse edges**. Browse = "go to neighbors / find usages";
   MCP = `find_usages`. Today the graph only stores out-edges, so "who calls this" needs a full scan.
4. **Persistent + queryable** — staged:
   - **v1 (in-memory):** the graph made correct/covered/bidirectional, behind a clean query API.
     Serves the human UI and a local MCP.
   - **v2 (disk index):** content-keyed persistent index (the deferred P4) for the GitHub-URL path and
     instant re-opens. *Additive* — the graph is already serialization-clean, so this is not a rewrite.

### Query API sketch (what the faces call)

```
analyze(target, scope) -> AnalysisHandle            // once; cached
entrypoints(handle, filter?) -> EntryPoint[]        // grouped, filterable
trace(handle, entry, depth, detail) -> Trace        // cheap re-query; structural bound only
map(handle, facet?) -> Map                          // topology / stack / packages
stats(handle) -> Stats                              // counts, sizes, per-seam edge coverage
node(handle, id) -> NodeDetail                      // declaration, tags, file:line
neighbors(handle, id, direction) -> Edge[]          // browse navigation (needs inverse edges)
find_usages(handle, id) -> Edge[]                   // inverse query
search(handle, query) -> Hit[]                      // filter-as-search over the model (later)
```

## 7. Faces' data-needs spec (the design the kernel serves)

**Browse UI screens** (each maps to query-API calls above):
- Entry list — grouped by kind (HTTP / Bus / Domain / Worker…), filterable → `entrypoints`
- Topology / architecture overview → `map`
- Trace view — pick an entry or any node, set depth/detail, follow the wiring → `trace`
- Node detail / navigate — declaration, tags, neighbors both directions → `node` + `neighbors`/`find_usages`
- Stats panel → `stats`

**MCP tool list** (same operations, agent-shaped): `list_entrypoints`, `trace`, `map`, `stats`,
`get_node`, `find_usages`. JSON is a first-class serialization of the same graph.

## 8. Token budgeting — DEFERRED (decision 2026-06-28)

Pulled out of the kernel. **Reason:** as built it acts as a guillotine that fires before/independently
of structural traversal and **distorts pruning and graph building** (the audit shows Map cutting
97–98% of types while leaving most of the budget unused, and traces both over-budget and truncated by
fan-out before the budget ever engages). The trace is already bounded structurally (depth · fan-out ·
framework stop), so budget does not belong in graph assembly.

**Rule for if/when it returns:** it is a **pure render-time post-filter** over an already-built,
already-bounded graph — it must **not** touch pruning, edge selection, or graph construction. Until
then, no token-budget logic participates in the kernel.

## 9. Acceptance bar — the "ripgrep test"

The product is "go-to" when, with **zero config**, in **a few seconds** (warm), it produces something
genuinely useful on each of these *differently-shaped* real repos:

| Shape | Eval repo | Must produce |
|---|---|---|
| Controller Web API | DntSite | entries **with** dispatch targets, a correct trace, stats |
| Minimal API | TodoApi | correct entries + trace + TOUCHES |
| Blazor / front-end | (add one) | component entries, topology |
| CQRS / DDD / events | eShop Ordering.API | the correct `POST /api/orders` trace (matches IDEAL-OUTPUT-TARGET §2), no sibling-edge fabrication |
| Library | AutoMapper | a useful surface map (not a call-stack dump) |

Plus the **re-probe**: rerun the LLM before/after (the §7 validation probe) on the *fixed* trace, on a
controller repo and a 2nd task, and measure whether the trace now reduces a tool-using agent's cost
(the current answer is "primer, not accelerator" — the target is "accelerator").

## 10. Sequenced backlog (each line serves a §6 requirement)

1. **Member-origin edge correctness** (req. 1) — removes fabrication; the probe showed it also recovers
   *missed* paths. Gate: Catalog `CreateItem` ≠ `UpdateItem`; `POST /api/orders` shows no sibling sends/raises.
2. **Controller entry→target resolution** (req. 2) — 0/94 → most resolve. The common case.
3. **Inverse edges + `neighbors`/`find_usages`** (req. 3) — unlocks browse navigation + MCP usages.
4. **Query API + analyze-once/query-many in the kernel** (req. 4 v1) — the layer the faces sit on.
5. **Perf / caching** — "just run it" needs speed; 41s cold is disqualifying.
6. **Browse UI (redo)** — thin client over the query API.
7. **MCP server** — second thin client.
8. **Remaining entry kinds** (§4 rungs 4–7) — widen universality.
9. **Persistent disk index + GitHub-URL path** (req. 4 v2) — discovery surface + scale.

## 11. Deferred / out of scope (named, not forgotten)

- Token budgeting (§8).
- Desktop polish beyond the browse redo.
- Whole-program data-flow / taint analysis.
- Non-.NET languages.
- Filter-as-search (`search` in the API) — after the five artifacts are solid.
