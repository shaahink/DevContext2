# Engine Value Audit — "Go-To for Every .NET Repo"

> Authored 2026-07-02 · Base: `develop` @ `7228d1e` · Branch: `docs/go-to-engine-audit`
> Companion: [`PROGRAM-PLAN.md`](PROGRAM-PLAN.md) (the phased plan derived from this audit, with votes).
> Scope: the engine + CLI/desktop surfaces, assessed against the north star in
> `docs/product/PRODUCT-DIRECTION.md` — *the first thing you run on any .NET repo*.
> All `file:line` citations verified against `develop` @ `7228d1e` on 2026-07-02.

---

## 1. Executive summary

The engine has crossed the credibility line: member-origin edges (no fabrication), 13 entry kinds,
provenance-gated archetypes, a real query API (`GraphQuery`), and an eval harness with 28 expectation
files across genuinely diverse repos. The 27-repo matrix that was 8/27 FAIL in June is now structurally
green.

The next distance to "go-to" is **not more entry kinds**. It is three things:

1. **Trust at breadth** — the wiring seams still lean on name-shape regexes tuned on eShop
   (`Send|Publish` verb sets, `*IntegrationEvent*` name matching, whole-body variable resolution).
   These will misfire quietly on the long tail of real .NET code. One is a live bug (§5.1).
2. **Answering the questions devs actually ask, per repo type** — we render *entries + topology +
   packages* everywhere, but the data already sitting in `Detections` (auth attributes, middleware
   order, DI shapes, message flows, EF entities) is not yet composed into the per-archetype answers
   that would make someone *bother to run this* (§4).
3. **The kernel as the product** — `GraphQuery` is MCP-shaped already; the legacy catalog/token
   machinery (W9) is the last big hygiene debt, and JSON output still serializes the dead catalog
   instead of the kernel (§6).

Everything proposed here composes into the existing 5-artifact discipline (entry points · topology ·
trace · stats · browse). No new output shapes — new **facets** of the same artifacts, gated per
archetype.

---

## 2. Current support matrix (what a repo gets today)

Grades are against the "ripgrep test" bar: zero-config, seconds, genuinely useful.

| Repo shape | Exemplar | Archetype | What renders today | Grade | Biggest missing value |
|---|---|---|---|---|---|
| Controller web app | DntSite | App ✓ | HTTP entries w/ `→ target` + file:line, Scheduled jobs, topology, packages | **B+** | auth surface, data map; TOUCHES gap (EfCore detection) |
| Minimal API | TodoApi | App ✓ | entries, trace w/ TOUCHES | **A−** | per-endpoint auth |
| FastEndpoints / VSA | VerticalSlice | App ✓ | entries incl. const-routes | **B+** | slice-level grouping |
| Razor Pages LOB | (E1, fixture) | App ✓ | page routes as HTTP entries | **B−** | handler→service targets unproven on a real LOB repo |
| Blazor app | blazor-samples | App ✓ | `@page` routes | **C+** | component→service wiring, event handlers as sub-entries |
| CQRS / DDD / events | eShop Ordering | App ✓ | the flagship: send→handler→raises→consumer trace, aggregates, behaviors | **A−** | per-aggregate lens; param-passed commands (semantic) |
| Microservices | eShop whole | App ✓ | 43 HTTP + 13 Bus, style=Microservices | **B** | cross-service message matrix, Aspire topology render |
| Modular monolith | OrchardCore | App ✓ ModularMonolith | entries, scope stamp | **B** | module-level topology; bind cost (~4s fixed) |
| Messaging / worker | MassTransit consumers, NServiceBus, Wolverine | App ✓ | Bus entries | **C+** | publish↔consume matrix — *the* question in these repos |
| gRPC / SignalR consumer | eShop Basket | App ✓ | entries w/ target (WS-A/F1) | **B−** | proto/hub method detail, client side ("talks to") |
| Serverless | Functions/Lambda fixtures | App ✓ | trigger entries | **C+** | trigger kind + binding detail (queue/timer/http) on the entry line |
| Desktop | Files (WinUI), WPF/WinForms/MAUI | App ✓ | UiEntry windows/pages | **C+** | VM↔View↔Service wiring, navigation graph |
| CLI tool | cmdline repos | App ✓ (E7) | CliCommand entries | **C+** | command tree w/ options/args (data is in settings-class attributes) |
| Runtime library | Serilog, Polly, Hangfire | Library ✓ | ENTRY API, ABSTRACTIONS, surface, consumer paths, type-rooted traces (W3) | **B** | contributor view: internal hubs by centrality; surface wall-of-types for humans |
| Framework repo | xUnit, MassTransit, SignalR | Library ✓ | correct archetype + surface, correct *absence* of app entries | **B−** | same as library + honest "this is the framework" one-liner |
| Source-gen library | CommunityToolkit | Library ✓ | GENERATORS section, marker attributes | **B** | generated-output examples |
| Gateway | YARP, Ocelot | Gateway ✓ | ROUTES from config | **B−** | route→cluster→downstream map; per-route middleware/auth |
| Megarepo / mixed | aspnetcore, HotChocolate | Library/timeout | W1 keeps entries honest (518→10) | **C** | sub-solution scoping; perf wall (whole-solution compilation bind) |

**Read of the matrix:** the *coverage* dimension (does anything sensible render?) is essentially won.
The *value* dimension (would a dev on that repo learn something they'd otherwise spend 30 minutes on?)
is won only for App/HTTP shapes and partially for libraries. §4 is the plan for the rest.

---

## 3. What "entry / common / interesting points" means per archetype

The user's framing: for HTTP and CLI, entry points are obvious. For libraries and other shapes they are
not. Proposal — make this an explicit, kernel-level concept with three tiers per archetype, so every
face (CLI, desktop, MCP) can ask for them uniformly:

| Archetype | Entry points (how execution enters) | Common points (what most work flows through) | Interesting points (what a dev studies first) |
|---|---|---|---|
| Web app | routes (have) | the service layer targets (have, `→ target`) | auth boundaries, middleware order, data touchpoints |
| CQRS service | routes + commands (have) | handlers, pipeline behaviors (have) | aggregates + their event fan-out |
| Messaging app | consumers (have) | the bus seam | the publish↔consume matrix; retry/outbox wiring |
| Library | ENTRY API: register/build/implement seats (have) | the **internal hubs** — highest in-degree types (data exists: `CodeGraph.InEdges`, unused) | the pipeline spine, e.g. Serilog's `Logger→MessageTemplate→Sink` chain |
| CLI tool | commands (have) | settings/options classes | command tree + side effects (files/processes touched) |
| Desktop | windows/pages (have) | ViewModels + `[RelayCommand]`s | View↔VM↔Service wiring, navigation graph |
| Gateway | config routes (have) | middleware chain | route→cluster→downstream targets |
| Framework repo | public seats (have) | extension points | "where do I add a provider/sink/formatter" — the seat + its implementors (ABSTRACTIONS already ranks this!) |
| Anything else (fallback) | `Main` / hosted | composition root(s) | **graph centrality** — the honest universal answer |

**Key insight:** the universal fallback for "interesting points" is already computable — in/out degree
over the graph we build (`GraphQuery.Node` returns degrees today; nothing ranks them). Centrality gives
every repo shape a non-empty, honest "start here" list *without* a per-shape extractor. That is the
cheapest high-leverage feature in this audit (§4, F7).

---

## 4. Per-type value packs — features we (mostly) already have data for

The discipline from PRODUCT-DIRECTION §2 stands: **no new artifact shapes**. Every item below is a
*facet* of Map / Trace / Stats, rendered **only when the archetype/signal warrants it**, and exposed as
a `GraphQuery` operation so CLI, desktop, and MCP get it for free. Ordered by (value × data-readiness):

| # | Facet | Question it answers | Data already captured | Work remaining |
|---|---|---|---|---|
| **F1** | **Auth surface** | "What's exposed anonymously? What guards what?" | `EndpointDetection.AuthAttributes` (`Detections.cs:55`) — captured for every endpoint, never rendered | render-only: entry-line annotation (`[anon]` / `[Authorize(policy)]`) + a one-line security digest in Overview ("70 endpoints · 12 anonymous") |
| **F2** | **Middleware pipeline** | "What does a request pass through, in order?" | `MiddlewareDetection(MiddlewareType, PipelineOrder, Kind)` (`Detections.cs:86`) + `ProgramCsFlowExtractor` | render-only: a `PIPELINE` line in Cross-cutting for web apps |
| **F3** | **Message flow matrix** | "Who publishes what; who consumes it?" (the #1 question in messaging repos) | `Sends`/`Raises` edges (member-origin) × `Handles`/`Consumes` edges × `MessageConsumerDetection` — the join IS the graph | pure graph query + render: per message type, producers → consumers, `?` for consumed-but-never-produced (honest partial-view marker). Gate on Bus entries present |
| **F4** | **Data map** | "What entities exist and which entries touch them?" | `EfEntityDetection` (aggregate flag, keys) + `ReadsWrites` edges | aggregate the per-trace TOUCHES to Map level: entity ← entry-kinds that reach it; DntSite TOUCHES gap is an EfCore detection fix first (known, Iteration-5 note) |
| **F5** | **Outbound dependencies ("talks to")** | "What does this service call?" | partial: DI `AddHttpClient<T>` in `DiRegistrationDetection.ExtensionsUsed`, `refit` signal, gRPC client types | small extractor delta: typed-client base addresses (config), gRPC/`ServiceReference` clients, named clients → a `TALKS TO` group in Topology. Microservices repos get service→service edges |
| **F6** | **Config surface** | "What configuration does this need to run?" | partial: `Configure<T>` registrations in DI detections; `IOptions<T>` ctor params in call-graph data | join + render: list option types (+ their appsettings section name when the `Configure<T>(config.GetSection("X"))` literal is present) |
| **F7** | **Hotspots / centrality** | "Where's the heart of this code?" | `CodeGraph.InEdges`/`OutEdges` — fully built, only per-node degrees surfaced | pure query: top-N by degree (production nodes only, NoiseFilter applied), tagged by role. Feeds the Library contributor view AND the universal fallback (§3) |
| **F8** | **DI health** | "What's registered, with what lifetime; what looks wrong?" | `DiRegistrationDetection(Shape, Lifetime, FactorySummary)` (`Detections.cs:120`) — rich and unrendered in the narrative | Stats facet: lifetime histogram, self-vs-interface, forwarding aliases; *(stretch: captive-dependency check = singleton whose impl ctor takes a scoped service — needs ctor-param join, data partially present)* |
| **F9** | **Aspire / orchestration topology** | "What services exist and how are they wired?" | `AspireExtractor` detections | render: a `SERVICES` grouping in Topology for Aspire/microservices repos |
| **F10** | **CLI command tree** | "What commands/options does this tool expose?" | `CliCommandDetection` (entries only) | extractor delta: read `[CommandOption]`/`[CommandArgument]` from settings classes (syntax we already parse); render tree under the CLI entry group |
| **F11** | **Serverless trigger detail** | "What fires this function?" | trigger attributes seen by extractors but only the kind survives | carry binding kind (Queue/Timer/Http + route/schedule literal) onto the entry line |
| **F12** | **Desktop wiring** | "Which VM drives this window; what does it touch?" | UiEntry + `[RelayCommand]` detection exists; DataContext binding not yet | extractor delta: `DataContext`/`x:DataType`/DI-registered VM join → UiEntry `→ target` = the VM; VM commands as sub-entries |

**Renderer growth control:** F1–F12 must not recreate the 15-catalog soup. Rule: each facet renders
(a) at most one *line group* inside an existing section, (b) only when its gating signal/archetype
holds, (c) with a structural cap + `… and N more`. The catalog pattern (`EntrySurfaceCatalog`) shows
how to keep this declarative — §6 proposes the same move for facets.

---

## 5. Wiring intelligence & hardening (the anti-overfit audit)

The engine's seam detection is a two-tier system: Roslyn-semantic for `Calls` (receiver resolution via
a source-based `CSharpCompilation`), regex-syntactic for `Sends`/`Raises`/`ReadsWrites` body scans.
The syntactic tier is where overfit lives. Verified hot spots:

### 5.1 CONFIRMED BUG — variable resolution crosses method boundaries

`GraphBuilder.AddSends` (`GraphBuilder.cs:836-845`): for `.Send(cmd)` where `cmd` is a variable, it
regex-searches `body[..pos]` — the **whole type body** before the call — for the last `new T(...)`.
Member-origin anchoring (via `methodSpans`) fixed where the edge *starts*, but the variable search is
**not span-bounded**: a `new SomethingCommand()` in an earlier *sibling method* will be attributed to a
later method's `Send(cmd)` whenever the sending method itself has no `new` (e.g. the command arrives as
a **parameter** — precisely the eShop `/draft` case). Same pattern in `ResolveVariableNewType`
(`:1043-1045`) used by `AddRaises`. **Fix:** bound the search to the enclosing method's span (the spans
are already computed and passed in); if nothing matches in-span, emit *no* edge (honest) rather than a
cross-method guess. This is a sibling-fabrication class bug — the same family Iteration 1 eliminated —
just one level deeper.

### 5.2 Verb-set overfit — dispatch matched by method name only

`AddSends`/`AddDispatchEdgesFromBody` match `\.(Send|SendAsync|Publish|PublishAsync)\s*\(`
(`GraphBuilder.cs:824-825, 781-782`) on **any receiver**. Two failure directions:

- **False positives:** any unrelated `.Send(...)` (an `SmtpClient`, a `Channel writer`, a domain method
  named Send) mints a `Sends` edge with a `Type` node for its argument.
- **False negatives:** Wolverine (`InvokeAsync`/`SendAsync` on `IMessageBus`), MassTransit
  (`Publish` on `IPublishEndpoint`, but also `Request<T>`), NServiceBus (`session.Send`), Rebus, CAP,
  Brighter — verbs and receivers we don't know.

**Hardening direction (vote in PLAN §V1):** make dispatch matching **receiver-typed and
catalog-driven**. We already resolve receivers semantically in `CallGraphExtractor`; gate body-scan
matches on the receiver being a known dispatch interface (`ISender`/`IMediator`/`IPublisher`/
`IPublishEndpoint`/`IMessageBus`/…), and move the (package → interfaces → verbs → EdgeKind) tuples into
a declarative **DispatchSeamCatalog** next to `EntrySurfaceCatalog` — new bus support becomes data, not
code. Keep the bare-verb match only as a `Confidence`-lowered fallback when no semantic receiver is
available.

### 5.3 Name-shape event matching

`AddRaises` detects integration events by `new\s+(\w*IntegrationEvent\w*)` (`GraphBuilder.cs:754-755`)
— pure name convention. Repos that call them `*Message`, `*Envelope`, or nothing-in-particular are
invisible; a DTO named `PaymentIntegrationEventLog` would false-positive. **Fix:** we already know the
real event types — `TypeDiscovery.BaseTypes`/`ImplementedInterfaces` gives every type deriving
`IntegrationEvent`/implementing `INotification`/`IDomainEvent` etc. Build the event-type **set from the
model**, then match `new X(...)` against the set. Same move for the `{AddDomainEvent, RaiseDomainEvent,
AddEvent}` method list (`:718`) — keep the verbs as a *seed*, but also accept any method whose parameter
type is in the event set. Replacing name-regex with model-derived type sets is the general anti-overfit
pattern; it reuses data we already extract.

### 5.4 Where string heuristics are *fine* (don't gold-plate)

- `WolverineExtractor` `*Handler` + `Handle/Consume` (`WolverineExtractor.cs:38-44`) — that IS the
  Wolverine convention, and it's signal-gated on the `WolverineFx` package. Acceptable; add a negative
  fixture (an `EventHandler`-suffixed non-handler) to pin behavior.
- `ProjectClassifier` path conventions (`/samples/`, `/test/`…) — structural, root-relative (W1),
  well-tested. Fine.
- `LibrarySurfaceBuilder.IsMarkerAttribute` (any `*Attribute` class) — over-broad but only fires for
  source-gen libraries and only ranks tier 0. Acceptable.
- FastEndpoints const-route index, Razor `@page` scans — narrow, gated. Fine.

### 5.5 The systematic hardening instrument: a pattern-zoo corpus

We currently prove correctness on ~30 real repos + fixtures — great for archetypes, weak for **syntax
breadth**. Missing: a deliberate corpus fixture (`tests/fixtures/PatternZoo/`) exercising modern C#
shapes through every seam scanner: primary constructors, expression-bodied members, top-level
statements, records with bodies, local functions, generic math, partial classes/methods, `required`
members, collection expressions, global usings, `#if` blocks, raw string literals *containing
code-looking text* (a known regex trap: a raw string containing `.Send(new X())` currently mints an
edge). Two assertions per shape: (a) no crash / no junk edges (negative), (b) the seam still resolves
when wrapped in the new syntax (positive). This is cheap to build incrementally and is the harness that
keeps every future regex honest. `TraceQualityTests`' divergence guards showed the pattern —
negative tests are what caught the class-scoped bug family.

### 5.6 Cleverness upgrades worth having (beyond hardening)

Ranked by value ÷ cost:

1. **In-span dataflow-lite** (with 5.1): resolve `Send(cmd)` through *param type* and *field type*
   when no local `new` exists — parameter/field types are in the syntax we already parse. Closes eShop
   `/draft` without full SemanticModel.
2. **Multi-implementation honesty:** when DI has 2+ impls for a resolved interface, the trace prints
   only one today. Annotate: `→ IFoo → FooA [registered ×2: FooA, FooB]`. Data exists in
   `DiRegistrationDetection`; trust feature, cheap.
3. **Channel/queue in-proc seam:** `Channel<T>.Writer.Write` → the reader loop — the modern in-process
   indirection idiom (background-queue pattern). One catalog entry + body scan, same machinery as
   Sends/Consumes.
4. **Semantic Sends/Raises tier** (existing backlog #11): bind body-scan seams through the
   `CallGraphExtractor` compilation per-member instead of regex. This is the endgame for 5.2/5.3;
   the catalog + type-set moves above are its stepping stones and stay useful as the fallback tier.

---

## 6. Kernel hygiene as we grow

**What's healthy:** `EntrySurfaceCatalog` (37 descriptors, one-file data; adding a shape = descriptor +
extractor + builder + detection); `IEntryPointBuilder` registry; auto-discovered extractors; signal
gating; `NoiseFilter`/`ProjectClassifier` structural filtering; `CodeGraph` immutable + serialization
clean; `GraphQuery` face-agnostic; `BudgetIndependenceTests` locking the narrative path; the eval
ratchet.

**Debts, in priority order:**

1. **W9 — the legacy catalog/token machinery is still alive** (`Pruning/TokenBudgetEnforcer`,
   `PatternRelevancePruner`, `RenderPlanBuilder` funnel) feeding only `--format json|html`. It confuses
   every external analysis (the DeepSeek misdiagnosis), keeps ~2k LOC of dead-but-not-dead code, and
   makes JSON lie about what the product is. **Decision needed** (options + vote in PLAN §V4): my vote
   is retire — JSON becomes a serialization of `MapModel` + `CodeGraph` + `GraphQuery` results, the
   same contract the MCP server will speak. One kernel, one wire format.
2. **Facet discipline needs a home.** F1–F12 (§4) threaten renderer sprawl if each lands as bespoke
   `MapRenderer` edits. Mirror the catalog move: a **facet descriptor** (gating signal/archetype →
   section key → cap → query op) so `MapRenderer`/desktop/MCP enumerate facets instead of hard-coding
   them. One new facet = descriptor + query function.
3. **Extractor-convention enforcement:** the `.cshtml`-as-C# regression happened because the
   `AllSourceFiles` invariant lived in a doc comment. Now that `AllContentFiles` exists, add a
   convention test: every Stage-3 extractor is signal-gated (`ShouldRun` ≠ constant true), touches the
   parse cache (not raw `ParseText`), and only reads `.cs` from `AllSourceFiles`. Cheap insurance that
   scales with extractor count (23 and growing).
4. **Default excludes footgun (open perf item #2):** `ExtractionOptions.cs:18` still lacks
   `eval-repos`/`analysis-repos`; analyzing DevContext2 itself walks 30 vendored repos (>600s). One-line
   fix + a guard test.
5. **Dispatch/event knowledge as data** (§5.2/5.3) — the DispatchSeamCatalog keeps seam growth from
   re-scattering tables, same reasoning as WS-D.

**Perf posture** (from the 2026-07-01 profiling): entry-scoped binding won the big one (41s→10s
DntSite); the remaining wall is the whole-solution `CSharpCompilation` + BCL reference bind (~3-4s
fixed, grows with repo). The correct fix is the **persistent, content-keyed compilation/graph index**
(the long-deferred P4) — it also unlocks the GitHub-URL path and instant desktop re-opens. Scheduled as
its own phase; do not micro-optimize the bind before it.

---

## 7. Faces: CLI, desktop, MCP, (web)

Confirmed direction (user, 2026-07-02): the graph/engine kernel will also be exposed through **MCP**, so
LLMs/agents can navigate a repo through us; desktop + CLI first, possibly web later — same engine.

- `GraphQuery` (`Graph/GraphQuery.cs`) already covers `entrypoints · trace · map · stats · node ·
  neighbors · find_usages · ResolveNodeId`. That is ~the MCP tool list from PRODUCT-DIRECTION §7
  verbatim. Missing for MCP v1: `search` (name/route/tag filter over nodes+entries — trivial over the
  graph), a `facet` op (once §4 lands), and **JSON serialization of results** — which is exactly what
  the W9 retirement produces. MCP server = a thin stdio host over `GraphQuery` + the analyze handle
  cache. The desktop's `DevContext.Server` already proves the hosting pattern.
- CLI today: `analyze | init | scenarios | version`. After W9, `analyze --format json` returns the
  kernel contract; consider `devcontext query <op>` as the CLI twin of MCP tools (same handle cache) —
  it makes the CLI scriptable for CI (e.g. "fail if anonymous endpoints grew").
- Web face: nothing to build now; the Server + JSON contract is the seam it would ride on.

---

## 8. Eval & acceptance posture

28 expectation files; render-level `entry-kind-present` checks closed the detect≠render blind spot;
`TraceQualityTests` has divergence (negative) guards; F3 seam-count goldens guard wiring regressions;
`BudgetIndependenceTests` locks narrative independence. **Gaps:** (a) no syntax-breadth corpus (§5.5);
(b) no per-facet render checks yet (each F# must ship with one, same as entry kinds); (c) the probe
verdict ("primer, not accelerator") is stale — it predates member-origin + honest traces + controller
targets; re-run it once V1 hardening lands, on eShop + DntSite, before investing further in
trace-shaping (PLAN §V5 gate).
