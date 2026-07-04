# L — Lighthouse: Repo Intelligence Iteration (Mega Plan)

> Authored 2026-07-04 · Base: `develop` @ `cae5e1e` (Fable W0–W7 closed, see
> `docs/dev/HANDOVER-FABLE-FINAL.md`). Written for agent-per-stage execution, same convention as the
> Fable proposal (§ stages, checkpoints, gates) and the go-to program (evidence → remedy → ratchet).
>
> **Why "Lighthouse":** the tool's job is no longer to *render the graph* — Fable finished that shell.
> The job now is to *point*: at what matters in an unfamiliar .NET repo, for a human onboarding or an
> agent working on a budget. Every stage below either makes the pointing trustworthy (L0–L2), makes
> the pointing smart (L3–L4), or hands the pointer to new consumers (L5 MCP, L6 UI, L7 audit).

---

## 1. Vision (restated from the user's brief, sharpened)

**North star:** the first thing you run on any .NET repo — library, web app, microservices, desktop,
CLI — and within a minute you know: *what is this, where does execution enter, what's the heart of
it, what's risky, and where do I start reading.* Then, when you (or your agent) dig in, the tool
builds **precise, budget-priced context** instead of you grepping and dumping files.

Three consumers, one kernel, in priority order for this iteration:
1. **The desktop app** — the human onboarding/browsing surface (exists, needs answer-reframing).
2. **MCP** — the agent surface (doesn't exist; the biggest strategic lever; an agent with our graph
   spends 10× fewer tokens than grep-scan-read loops and gets confidence labels for free).
3. **The CLI** — the *iteration harness*: one command that renders everything the app knows, so
   benchmark → judge → fix engine → re-run becomes the project's operating loop. (Public CLI polish
   catches up later, as the user said.)

**The moat is the engine.** Both external LLM reviews and our own audit agree: extraction quality and
honesty (verified/approx) are the differentiator; visualization variety is not. So the waterfall
front-loads engine truth, then builds product value on top of it.

---

## 2. Evidence — benchmark round (2026-07-04, this session)

Fresh CLI runs over five repo shapes + the user's TradingEngine screenshots. Raw outputs preserved in
`eval-results/lighthouse-baseline/`. Every finding below was verified against source, with the
responsible code located. **These are the fix list for L0–L2 — cite them in commits.**

### 2.1 Trust-breakers (wrong facts, shown prominently)

| # | Finding | Proof | Cause (file:line) |
|---|---------|-------|-------------------|
| E1 | **False top-of-Home security warning.** "11/12 endpoints anonymous incl. 6 POST/PUT/DELETE" on davidfowl/TodoApi — but `/todos` is protected by `group.RequireAuthorization(pb => pb.RequireCurrentUser())`. Same class of claim on TradingEngine ("85/85 anonymous"). | `TodoApi-map.md` vs `todoapi-src/Todo.Api/Todos/TodoApi.cs:16` | `EndpointExtractor.cs:217` only sees *chained* `RequireAuthorization` on the endpoint itself; group-level (`MapGroup(...)` conventions) and global fallback-policy auth are invisible. `AnonymousEndpointsSource.cs:18-20` also dedups detections by route only (verb collisions). |
| E2 | **"Multi-implementation interfaces" insight is doubly wrong.** Reports "? (16 impls)", "TodoDbContext (2 impls)", "`ApplicationDiscriminator = "TodoApp"` (2 impls)" — unresolved names leak as literal `?`, and non-interfaces are listed. | `TodoApi-map.md`; TradingEngine screenshot ("? (9 impls) · options (7 impls)") | `MultiImplSource.cs:20` groups by `ImplementationType ?? ServiceType` — i.e. by *implementation*, while the headline claims *interfaces*. Correct: group by `ServiceType`, count **distinct** implementations, drop unresolved/`?` names, require the service to be an interface/abstract. |
| E3 | **Trace snippets show the wrong code.** Under `handler CreateOrderCommandHandler` the snippet is the command DTO's `[DataMember] private readonly List<OrderItemDTO> _orderItems;`. Stray `}` lines, unrelated comments everywhere. TradingEngine screenshot: a `GET /api/system` trace shows `[HttpPost("reset")] ... Reset(...)` code. | `eShop-trace-orders.md`; screenshots | `TraceBuilder.cs:329-337` — "salient" lines are taken from the **caller's** body (falling back to the whole parent *type's* body) around the provenance line, then rendered under the **callee** node. Type-body fallback also misaligns file line numbers vs body-substring offsets (`ExtractSalient`, `TraceBuilder.cs:453`). |
| E4 | **Sibling-member contamination in data edges.** POST /api/orders trace reports `data CardType [approx]` and `TOUCHES CardType` — CardType belongs to the sibling `GET /api/orders/cardtypes` endpoint. The V1.1 span-bounding fixed Sends/Raises; ReadsWrites/data edges still leak across members. | `eShop-trace-orders.md` | `GraphBuilder` ReadsWrites body-scan not span-bounded per member (same family as the fixed `AddSends` bug — check `GraphBuilder.cs` `AddReadsWrites`/data-edge path). |
| E5 | **Commands reported as raised events.** `raises CreateOrderCommand [approx]` — a `new CreateOrderCommand(...)` constructor read as an event raise, duplicating the `send` seam in the same trace. | `eShop-trace-orders.md` | `AddRaises` verb/ctor matching not gated by the model-derived *event* type set (V1.4 built the set; the Raises path doesn't fully enforce "must derive from a known event base"). |
| E6 | **Minimal-API entry "targets" are arbitrary internal calls.** `DELETE /api/catalog/items/{id}` → `CatalogContext.SaveChangesAsync`; `GET items/by` → `CatalogServices.Where`; `GET .../pic` → `CatalogContext.FindAsync`. The single most-read column in the product points at noise. | `eShop-map.md` | Lambda-endpoint target selection picks a deepest/first interesting call. Correct preference order: method-group ref → named local function → the service/handler call with the highest out-degree ownership — else honestly render `inline (N calls)`. |
| E7 | **WPF `ICommand`s misclassified as CLI commands.** PowerToys "CLI (20)": `ButtonClickCommand -settings object`, `AsyncCommand`, palette-SDK `Command`. | `PowerToys-map.md` | `CliCommandExtractor` gate matches name/ctor shape without requiring a CLI-framework base (Spectre `Command<TSettings>`/System.CommandLine/Cocona) in the type's bases. |
| E8 | **Architecture style overclaims, confidently.** DevContext itself: `STYLE ModularMonolith (confidence high) — evidence: 9 module-like sub-projects: devcontext.cli, devcontext.core.tests…` (dot-named projects ≠ modules; test projects counted as modules). STACK claims `MediatR (CQRS)` from a stray package ref. | `DevContext-self-map.md` | `ArchitectureStyleDetector` module heuristic = name-dot pattern; stack chips are package-presence, not usage-evidence. |
| E9 | **Silent garbage on bad input.** `analyze <empty-dir>` resolved an *ancestor/CWD solution* (`DevContext.slnx`), rendered a **legacy pre-kernel format** ("Architecture: Not detected", advice to use the *removed* `--profile debug` flag), exit 0. And any one-slash relative path (`eval-repos/eShop`) is hijacked as a GitHub `owner/repo` shorthand → "Repository not found". | session transcript; `eShop-map.md` (first run) | `ProjectRootResolver` fallback + a legacy renderer path still alive; `RepoUrl.Parse` wins over local-path existence check. |

### 2.2 Performance / responsiveness (the user's live pain)

| # | Finding | Cause |
|---|---------|-------|
| P1 | **Every GitHub analyze re-clones from scratch.** The 24 h clone memo is an instance field (`GitCloneService.cs:24`) but `EngineRunner.PrepareSourceAsync` constructs a fresh `GitCloneService` per analyze (`EngineRunner.cs:108`) — so the memo is always empty, `Directory.Exists → DeleteDirectoryRobust → full re-clone` (`GitCloneService.cs:138-139`). Reopening the app with a GitHub tab = re-clone GBs + re-analyze. | Design bug, small fix, huge payoff. |
| P2 | **Primary clone path is a FULL clone.** LibGit2Sharp is tried first and cannot shallow-clone; the `--depth 1` git-CLI path is only the fallback (`GitCloneService.cs:145-155, 207`). PowerToys ⇒ full history download. | Flip preference: git CLI shallow first when available. |
| P3 | **Clone progress is per-object; analysis progress is a 5-step jump.** Clone streams `Receiving: 12345/280000 objects` (`GitCloneService.cs:190`); analysis maps 7 stages to fixed percents (10→25→…, `StreamingProgressObserver.cs:11-23`) so "Deep analysis 50%" sits frozen for minutes on big repos. | No file/project-weighted progress; no event throttling. |
| P4 | **Snapshot cache is checked *after* clone.** `PrepareSourceAsync` runs before `SnapshotCacheService` lookup (`EngineRunner.cs:14` vs `:35`), so even a fully cached GitHub repo pays validate+clone before rendering. | Reorder: URL→(clone registry)→snapshot key first; freshness check in background. |
| P5 | **Analysis itself is NOT the bottleneck.** PowerToys cold: 120 projects, 3 637 files, **23 s** total (`PowerToys-map.md`). The app-level unresponsiveness comes from P1/P2 clone + post-ready client work (Atlas indexer fires up to 100 sequential GetTrace calls; progress event flood through Angular change detection). | Server-side Top Flows/Impact (L2) + event throttling (L1). |
| P6 | Sync-over-async disposal on the server session paths (`AnalysisSessionManager.cs:40,70,77`) — latent stalls under eviction. | Cleanup item, L1. |

### 2.3 Value gaps (right data, wrong altitude — per repo shape)

- **Desktop megarepo (PowerToys):** "UI (237)" = flat alphabetical `[RelayCommand]` dump across 120
  projects. The repo is really ~30 utilities + CmdPal; nothing surfaces that. No module/app grouping,
  no per-module entry rollup. Grade for the shape the user cares about: **D** — technically covered,
  practically useless.
- **Library (Serilog):** ENTRY API / ABSTRACTIONS (implementor counts!) / PUBLIC SURFACE are genuinely
  good bones. But PUBLIC SURFACE is a 109-type wall including vendored `JetBrains.Annotations` and
  internal plumbing namespaces; XML-doc summaries get mangled by stripped `<see cref>` ("Configuration
  object for creating instances **of**…" → dangling). No "internal hubs / where's the heart"
  (V3.2 contributor view, still unbuilt).
- **Tool/library-shaped graphs are nearly edgeless.** DevContext self-analysis: 331 nodes, **25
  edges** — the focused profile binds call edges only around entries, so Node/Neighbors/System views
  are ghosts on exactly the repos where browsing matters most. (`eShop`: 1 734 nodes/1 011 edges —
  fine, because 121 entries anchor it.)
- **gRPC surface collapsed:** self-analysis shows ONE entry `DevContextGrpcService (20 methods:
  Analyze, RunAnalysisAsync, CloseSession)` — method-level entries exist for HTTP but not gRPC.
- **Controller routes without action names:** eShop `GET /Account` ×3 rows, indistinguishable.
- **Duplicated-looking deck rows** (TradingEngine screenshots: a dozen `GET /api/runs`): route templates
  truncated/undifferentiated in the deck — needs identity subtitle (action/target), and engine-side
  disambiguation where routes genuinely collide.

### 2.4 UI/UX judgment — screenshot by screenshot (TradingEngine session)

**Home digest.** The stat strip leads with *our* vocabulary — `2898 nodes · 1323 edges · 85 entries ·
85 wired · 0 unwired · 100% coverage · 50% confidence`. A developer opening a strange repo cannot act
on any of those numbers, and "50% confidence" reads as "half wrong" with no explanation. The identity
line `App MinimalApi · 65%` is a classifier chip, not a sentence a person would say. TOP FLOWS is
seven visually identical route rows (three of them `/api/strategies`) with an "HTTP" tag and no hint
of *why* they're top or *what they do*. Below the fold, RUN REPORT / STAGES / FUNNEL expose pipeline
telemetry as primary content — and the funnel is a dead legacy organ (`Types 0 → 0`, `Tokens 0 / 8.0K
budget` — W9 was retired; this UI wasn't). **Verdict: the first screen answers "what did the engine
do" instead of "what is this repo".**

**Workbench, Flow tree.** The strongest surface. Entry/Call chips, verified/approx badges, deck →
stage → inspector flow all read well. Three failures: (1) the code snippets under call nodes are
wrong code (E3) — in a *trust-themed* product that's fatal; (2) the deck is a wall of near-identical
rows (dup `GET /api/runs`) with no subtitle to tell them apart; (3) the LLM context pane renders the
export as raw wrapped ASCII — it's the product's crown jewel and it looks like an accident.

**Flow graph.** Four nodes floating in a huge empty canvas, hairline arrows, generic always-on legend,
no fit-to-view, no fullscreen. At realistic flow sizes this becomes the "clutter" the user reports.
The legend advertises 9 edge colors — visual encoding is carrying semantics the layout doesn't.
**Verdict: below the bar of the rest of the app; needs a real layout engine + zen mode, not more
tweaks.**

**Insights page.** Correct instinct (severity groups: RISK/WIRING/TOPOLOGY/COVERAGE), flat execution:
facts without consequence or action ("Most depended-upon: TradingEngine.Domain (10 dependents)" — so
what?), a false headline warning at the top (E1), `?` leaking into card titles (E2), evidence chips
duplicated (`GET /api/trades` ×3), the COVERAGE section rendered twice, and CLI copy leaking into a
desktop product ("use `--focus` for deeper traces"). No card offers a next step (trace it, see
usages, export pack).

**Statusbar** cycles raw insight text + node/edge counts — plumbing again.

**What's genuinely good and must not be lost in the reframe:** one-workbench + trail model, keyboard
sweep, verified/approx honesty chips, altitude switcher, the export drawer concept, light/dark polish.

---

## 3. Decision log (votes recorded, carry on)

| Q | Options | VOTE + why |
|---|---------|-----------|
| **"Skip Roslyn" option** — keep? | (a) keep user-facing toggle; (b) remove from UI + Start flow, keep `--no-roslyn` as hidden CLI/debug escape; (c) remove everywhere | **(b).** The Deep tier (source-based `CSharpCompilation`, no MSBuild) is what produces `[verified]` edges, real targets, TOUCHES — i.e. the product. A first-run user unchecking "Use Roslyn" gets the untrustworthy tool and doesn't know why. The option predates snapshot caching + Phase-6 perf (~10 s DntSite, 23 s PowerToys) — its reason to exist is gone. Keep the *internal* knob (fixture tests, bisecting extractor bugs). **Note:** the strategic Roslyn question is the opposite direction — an *optional* MSBuildWorkspace "high-fidelity" tier (semantic Sends/Raises, cross-assembly types) for repos that build. Recorded as LATER; body-scan + catalogs stay the default tier. |
| Graph rendering | (a) keep hand-rolled SVG; (b) adopt a bundled layout/render lib (Cytoscape.js + fcose, or sigma.js/elkjs) inside the existing Angular stage | **(b)** — layout quality is a solved problem we keep re-losing; CSP allows bundled (not CDN) libs. elkjs (layered) for Flow altitude — call chains are DAG-ish and want ranked layers; fcose/Cytoscape for System altitude clustering. Keep our node/edge *styling* tokens. |
| GitHub clones | (a) fix memo + keep delete/re-clone; (b) persistent on-disk clone registry + fetch-to-freshen, never delete implicitly | **(b)** — clones are cache assets like snapshots; Settings→Storage already shows/clears them. Shallow (`--depth 1`) by default via git CLI; LibGit2Sharp becomes fallback. |
| Insight engine | (a) more sources; (b) reframe envelope (impact/action/confidence) + per-archetype composition, then add sources | **(b)** — the external reviews and our screenshots agree: count of insights isn't the problem, consequence is. |
| MCP shape | (a) thin GraphQuery mirror (I6 as written); (b) I6 + `get_context` budgeted packs + `read_source` | **(b)** — for an agent, the killer feature vs grep is *pre-resolved wiring + honest confidence + token-priced context*. Tools without a budgeted context assembler still force the agent to re-read files. |
| CLI as harness | (a) enrich `analyze`; (b) new `devcontext report` (digest+flows+traces+insights in one deterministic doc) + `bench` diff loop | **(b)** — `analyze` stays the composable primitive; `report` is the app-mirror the user asked for ("with one call see what we see in the app"). |

---

## 4. The waterfall — stages L0 → L7

Rules carried from the Fable/go-to conventions, binding on every stage:
- One stage ≈ one agent session (L6 is two). Do checkpoints in order; each lands as **one commit**
  (`type(scope): L<stage>.<n> — what`), tracker table in this doc updated in the same commit.
- **Gate before DONE**, and *verify live* — CLI stages: re-run the benchmark set and diff; UI stages:
  Playwright against a real analyzed repo with **client-side navigation** (a `page.goto()` hard reload
  drops session state and fakes "everything broken" — documented trap), plus screenshots.
- Eval goldens are **ratcheted, never silently re-baselined** — before/after numbers in the commit message.
- Every detection change ships a positive AND a negative fixture (absent gate ⇒ absent output).
- Docs move with code in the same commit (`docs/product/cli-reference.md`, MCP reference, this tracker).

| Stage | Theme | Status |
|-------|-------|--------|
| L0 | Truth pass — fix the nine trust-breakers | IN PROGRESS (5/9: L0.1, L0.2, L0.3, L0.4, L0.9 done) |
| L1 | Open fast, reopen instantly, stay responsive (GitHub/persistence/progress) | NOT STARTED |
| L2 | CLI `report` + bench loop (the iteration harness) | NOT STARTED |
| L3 | Kernel answers: Impact RPC, Top Flows, InterestingPoints, graph completeness | NOT STARTED |
| L4 | Insight engine v2 + archetype lenses (facets that matter) | NOT STARTED |
| L5 | MCP server + context packs (the agent surface) | NOT STARTED |
| L6 | UI/UX round: answers-first Home, insight cards with actions, readable graphs | NOT STARTED |
| L7 | Benchmark audit + close-out gate | NOT STARTED |

### L0 — Truth pass (engine correctness; nothing else ships on lies)

Checkpoints (each = evidence item → fix → fixtures → ratchet):
1. **Auth truth (E1).** Propagate `MapGroup` conventions (`RequireAuthorization`/`AllowAnonymous`/
   rate-limit) to member endpoints; detect `AddAuthorization` fallback/default-policy and
   `app.MapGroup().RequireAuthorization()` composition roots. When global-auth signals exist but
   per-endpoint status is unknown, the insight must say "auth present, N endpoints not individually
   verifiable" — never "anonymous". Fix verb-collision dedup in `AnonymousEndpointsSource`.
   *Gate:* TodoApi reports only the genuinely anonymous auth endpoints; eShop/DntSite counts re-ratcheted.
2. **Multi-impl semantics (E2)** + a global **"no `?` in output" invariant**: add a render-layer
   assertion (strict mode) that no insight/entry/target ever prints an unresolved placeholder.
3. **Salient snippet correctness (E3).** Show the *callee's* signature + first salient body lines from
   the **callee's own** span; label caller-side context explicitly ("at call site:"); fix
   file-line↔body-offset mapping. This single fix upgrades every trace, the Inspector, and every LLM
   export at once — highest leverage checkpoint in L0.
4. **Span-bound data edges (E4)** — same treatment `AddSends` got; no in-span match ⇒ no edge.
5. **Raises gating (E5)** — `Raises` only for types in the model-derived event set (derive from known
   event bases/interfaces); `new SomeCommand(...)` never raises.
6. **Minimal-API target selection (E6)** — preference order: method group → named local function →
   owning service call; else `inline (N calls)`. *Gate:* zero `DbContext.*`/LINQ-operator targets
   across the benchmark set.
7. **CLI-command gating (E7)** — require CLI-framework base types; PowerToys "CLI" group drops to 1
   (`PowerToys.DSC` if it truly is Spectre-based) or 0.
8. **Style/stack honesty (E8)** — ModularMonolith needs module *behavior* evidence (module manifests,
   per-module composition roots, area folders), not dot-named projects; test/bench projects never count;
   stack chips require usage evidence (MediatR chip ⇔ `IMediator`/`Send` usages exist).
9. **Input honesty (E9)** — local-path existence beats `owner/repo` shorthand; empty/invalid dir = hard
   error with guidance (exit 2, no output document); delete the legacy renderer fallback path entirely.

**Agent guidance / tricky parts:** build with `dotnet build DevContext.slnx` (warnings are errors);
`eval-repos/` in this checkout are **empty placeholders** — clone real fixtures or use
`eval-results/lighthouse-baseline/` inputs (TodoApi/eShop/Serilog clones under scratchpad were
session-local; re-clone shallow). Always pass **absolute paths** to the CLI until checkpoint 9 lands.
The 5 pre-existing `TraceQualityTests` failures are stale goldens vs a drifted eShop checkout — re-ratchet
them while you're here (close-out item from the Fable handover). Console output mojibake under PS 5.1
is cosmetic (`[Console]::OutputEncoding` UTF-8 per the eval-audit skill).

### L1 — Open fast, reopen instantly, stay responsive

1. **Persistent clone registry** — `%LOCALAPPDATA%/DevContext/repos/registry.json`
   (url+ref → path, HEAD, clonedAt); `GitCloneService` becomes a singleton fed by the registry;
   freshness = `git fetch` + HEAD compare (never implicit delete); shallow clone via git CLI preferred,
   LibGit2Sharp fallback; explicit refresh/delete only from Settings→Storage or a UI "refresh" action.
2. **Snapshot-first open** — resolve URL→registry→snapshot key *before* any network/clone
   (`EngineRunner` reorder); on hit: render instantly, kick a background staleness probe, surface a
   "repo moved ahead — Re-analyze?" chip instead of blocking. App restart with N tabs = N instant
   rehydrations, zero re-analysis (kills the "reopening re-analyzes the previous GitHub tab" pain).
3. **Progress v2 (logical, not per-object)** — clone: map git phases (counting/receiving/resolving/
   checkout) to a weighted 0-100 with repo-size-aware messaging; analysis: percent = files-parsed/total
   within extraction stages + per-project bind counts in the deep stage (the pipeline already knows
   both; `StreamingProgressObserver` gets real signals instead of stage constants). Server throttles
   progress events to ≤4/s; client renders a phase checklist with live counts, not a jumping bar.
4. **Responsiveness under load** — Angular: progress stream handled outside change detection (zoneless
   signals batch); statusbar/ticker updates coalesced. Sidecar: below-normal process priority +
   `MaxDegreeOfParallelism` setting. Server: fix sync-over-async disposals (P6).
   *Gate (live, Playwright + stopwatch):* analyze PowerToys-from-URL in tab A — tab B stays fully
   interactive (keyboard nav < 100 ms); close app mid-everything, reopen → all tabs interactive < 3 s.

**Tricky parts:** don't break CLI one-shot semantics (CLI keeps `AnalysisCache` per-run; registry is
shared). Registry writes need file locking (two tabs cloning the same URL — reuse `_cloneLock` pattern
at registry level). The Tauri sidecar spawns per app run; registry must be process-independent state.

### L2 — CLI `report` + bench loop (build the harness before the features it will judge)

1. `devcontext report <path|url>` — one deterministic markdown/JSON doc mirroring the app: identity
   sentence, stat digest, **Top Flows with targets**, top-3 compact traces, insights (v1 for now),
   archetype lens section (library surface / module map when present), run report. Stable ordering,
   `--format json` = kernel JSON (this becomes the MCP/status-quo contract input).
2. `scripts/bench.ps1` (or `devcontext bench`) — run `report` across the registered benchmark set into
   `eval-results/<date>/`, then emit a **diff vs the previous run** (entries gained/lost, target
   changes, insight text changes). The reinforcement loop the user asked for: *analyze → read reports →
   judge → fix engine → re-run → diff*.
3. Benchmark set v2 (register in `eval-repos.json`): keep DntSite/TodoApi/eShop/VerticalSlice; add
   **PowerToys** (desktop megarepo), **Serilog** (library), **Spectre.Console** (CLI framework),
   **MassTransit sample** (messaging), **DevContext itself** (dogfood), + the user's TradingEngine
   when local. Aim: every §2.3 shape has an exemplar.
4. Query surface parity: `query` grows `insights`, `node`, `neighbors`, `usages`, `search` ops
   (`QueryCommand.cs:17-20` currently stops at 4) — the CLI must be able to see anything the app sees.

**Tricky parts:** determinism is the point — sort everything, strip wall-times into a sidecar section
the differ ignores. Don't build a second renderer: `report` composes existing `MapRenderer`/
`TraceRenderer`/insight renderers; if a section can't be composed, that's a kernel-face smell to fix,
not to work around.

### L3 — Kernel answers, not client reconstructions

1. **`GetImpact` RPC** on the already-correct `GraphQuery.BlastRadius` (dead code today — handover
   §4.3); Inspector Impact lens + omnibox verb consume it; Atlas brute-force indexer demoted to
   optional prefetch. (Directly fixes P5's post-ready GetTrace storm.)
2. **Server-side Top Flows** — rank entries by reach breadth × seam richness (send/raise/consume
   crossings) × entity touches × cross-project depth; return *with* target + one-line "why" (e.g.
   "touches 4 entities, crosses 3 services"). Home/deck ordering consumes it.
3. **`InterestingPoints(archetype)`** (V3.1) — per-archetype composition with **centrality fallback**
   for every shape (the honest universal "start here"); powers Home "Start here", deck grouping, MCP.
4. **Graph completeness for browse** — when entries are sparse (< N entries or Library/Tool archetype),
   bind call edges hub-scoped (top-K central types) so Node/Neighbors/System are never ghosts
   (fixes 331-nodes/25-edges). Budget-bounded; report the chosen scope honestly in Stats.
5. **Node line numbers** (S2) + **auth-on-entries** (S1, from L0.1) onto `NodeResponse`/entry payloads —
   unblocks editor jump + Entries auth column.
6. **Module/feature grouping** — entries carry `GroupPath` (project → module/area/feature folder) so
   PowerToys renders "CmdPal › Gallery (9 commands)" instead of 237 flat rows; gRPC services expand to
   method-level entries like HTTP.

### L4 — Insight engine v2 (answers with consequences) + archetype lenses

1. **New envelope:** `claim · evidence[] (node ids, not strings) · confidence + basis · why-it-matters ·
   suggested action (trace/usages/export target)`. Renderers (CLI/desktop/MCP) get action affordances
   for free. No insight renders without its gate signal (negative expectation each).
2. **Composition per archetype** (uses L3.3): web → auth surface + data map + middleware; messaging →
   publish↔consume matrix with `consumed-but-never-produced ⇒ external contract` marks; library →
   public-surface size/compression, internal hubs ("the heart"), seat implementors, dead public API;
   desktop → module map + VM↔View wiring; CLI → command tree (real one, post-E7).
   This absorbs the F1–F12 facet backlog *selectively* — F1/F3/F4/F7/F10/F12 land here as insight+facet
   pairs; F2/F5/F6/F8/F9/F11 stay backlog unless a benchmark shows demand.
3. **Confidence Ledger honesty** — "50% confidence" (screenshot) must decompose: what % verified edges,
   what's approx and *why that's okay/not*; ledger copy written for humans.
4. Doc-summary hygiene for library surface (dangling `<see cref>` strips, vendored-namespace exclusion).

### L5 — MCP server + context packs (the agent surface)

Extends `ITERATION-I6-mcp.md` (still sound): stdio server, official C# SDK, tools = kernel ops,
non-blocking `analyze` returning `{handle,status}` + `status(handle)`.
1. Tools v1: `analyze`, `status`, `map`, `entrypoints`, `top_flows`, `interesting_points`, `trace`,
   `node`, `neighbors`, `usages`, `search`, `impact`, `insights` — every response envelope carries
   `scope + coverage + confidence` (the calibration edge over grep).
2. **`get_context(focus|question, budget_tokens, intent)`** — the LLM-exporter, weaponized: kernel
   `ContextPackBuilder` assembles trace skeleton + callee signatures + salient bodies (correct after
   L0.3) + DI wiring + config + touched entities, ranked by graph distance/centrality, cut to budget
   with per-section token attribution. Same builder backs the desktop export drawer presets and a CLI
   `context` op — one implementation, three faces. This is the user's "trace a service and build
   context instead of dumping files".
3. **`read_source(node_id, span?)`** — precise file:line-anchored reads so the agent never greps blind.
4. Deliverable proof: committed transcript — agent on eShop answers "how does order placement reach the
   basket?" in ≤ N tool calls / ≤ M tokens, vs a recorded grep-baseline attempt. That comparison is the
   README pitch for MCP.

**Tricky parts:** stdio server must not write logs to stdout (protocol corruption — Serilog to file
only). Session handles map to the same `SnapshotCacheService`; re-`analyze` of a cached repo returns
ready immediately. Token counting: reuse the existing estimator; never hard-fail on budget — trim and
report what was cut (`omitted:` list), the agent needs to know what it *didn't* get.

### L6 — UI/UX round (two sessions: reframe, then graphs)

**Session A — answers-first reframe** (judged against §2.4):
1. **Home rewrite:** identity *sentence* ("ASP.NET Core web API · 85 endpoints across 3 services ·
   EF Core + RabbitMQ"), then *Start here* (L3.3), *Top Flows with targets + why* (L3.2), *What needs
   attention* (top insights v2 with action buttons). Engine telemetry (stages/cache/funnel) moves to a
   collapsed "Engine details" disclosure — and the dead token-funnel UI is **deleted** (W9 remnant).
   Stat strip becomes human: "85 endpoints · 12 services · 340 types · analyzed in 11 s ·
   94% verified" with hover explainers.
2. **Insight cards v2:** severity → *impact grouping* ("Act on this" / "Know this"), evidence chips
   dedup + link into Workbench, action buttons (Trace / Usages / Export pack), no CLI copy anywhere
   (audit all strings for `--focus`-style leaks), fix duplicated COVERAGE section.
3. **Deck identity:** subtitle = action/target ("GET /api/runs — RunsController.List"), group by
   resource/module (L3.6), dedupe visual noise; count badge per group.
4. **LLM context pane:** styled pack preview (sections, token meter per section) backed by
   `ContextPackBuilder`, copy + "open in export drawer".
5. Purge remaining plumbing leaks: statusbar ticker shows insight *headlines* not node counts;
   "confidence" chip opens the Ledger explainer.

**Session B — graph readability** (the user's explicit complaint):
6. Adopt the voted layout lib; Flow altitude = layered DAG (elkjs), System = clustered (project/module
   compound nodes, expand/collapse), node size by centrality, edge color by seam (keep tokens).
7. **Zen mode:** maximize stage to full window (`F` / double-click header), esc-ladder aware; fit-to-view
   on load; zoom controls that were previously decorative become real.
8. Focus dimming (hover/select dims non-neighbors), legend collapses to a popover, label
   density rules (hide labels < zoom threshold), minimap only in zen mode on large graphs.
9. *Gate:* screenshots on TradingEngine + eShop + PowerToys System view — a stranger can name the main
   clusters unprompted; no overlapping labels at default zoom; zen mode keyboard-complete.

**Tricky parts:** bundle the graph lib locally (CSP forbids CDN); keep the existing node styling
tokens so light/dark/reduced-motion survive; virtualize the deck (85+ rows already jank with peek
hovers). Re-run the W7 reduced-motion audit on new graph animations.

### L7 — Benchmark audit + close-out (the honesty gate)

1. Re-run `bench` across the full set; write `eval-results/<date>/AUDIT.md` scoring every §2 finding
   FIXED/IMPROVED/OPEN with output diffs as proof.
2. Fix the top-3 worst regressions found (there will be some — budget a session).
3. Ratchet all eval expectations; tracker tables in this doc → DONE with commit refs; handover doc
   (`HANDOVER-LIGHTHOUSE.md`) in the Fable-handover style: what a cold session must know, what's
   deliberately deferred (MSBuild fidelity tier, remaining facets, web face, CLI public polish).

---

## 5. Success criteria (the user-visible version)

1. **Trust:** zero known-false claims in the default output across the benchmark set (E1–E9 all gated
   by fixtures). The top insight on Home is never wrong about the flagship samples.
2. **Cold GitHub repo (PowerToys-class):** paste URL → honest phased progress → interactive digest;
   **reopen app → < 3 s to interactive** with no re-clone/re-analysis; other tabs never freeze.
3. **Any shape gets a real answer:** library → surface + heart; desktop → module map; CLI → command
   tree; messaging → produce/consume matrix; fallback → centrality "start here". No more ghost graphs.
4. **Agent leverage:** MCP transcript proves an agent answers a cross-service wiring question in fewer
   tokens than grep-baseline, with confidence labels; `get_context` returns a budgeted pack, not a dump.
5. **The loop exists:** `bench` diff runs in CI-able form; the next quality iteration starts from
   reports, not vibes.

---

## 6. Tracker

| Stage.checkpoint | Commit | Status |
|---|---|---|
| L0.1 Auth truth (E1) | 829417e | DONE |
| L0.2 Multi-impl semantics + no-`?` (E2) | fa4618d | DONE |
| L0.3 Salient snippet correctness (E3) | 4bfd388 | DONE |
| L0.4 Span-bound data edges (E4) | this commit | DONE |
| L0.5 Raises gating (E5) | — | NOT STARTED |
| L0.6 Minimal-API target selection (E6) | — | NOT STARTED |
| L0.7 CLI-command gating (E7) | — | NOT STARTED |
| L0.8 Style/stack honesty (E8) | — | NOT STARTED |
| L0.9 Input honesty (E9) | 2ddb83f | DONE (partial — see note) |
| L1.1 … L1.4 | — | NOT STARTED |
| L2.1 … L2.4 | — | NOT STARTED |
| L3.1 … L3.6 | — | NOT STARTED |
| L4.1 … L4.4 | — | NOT STARTED |
| L5.1 … L5.4 | — | NOT STARTED |
| L6.1 … L6.9 | — | NOT STARTED |
| L7.1 … L7.3 | — | NOT STARTED |

*Maintain per the conventions in §4's preamble: status + commit hash in the same commit as the work.*

**L0.9 partial-scope note:** fixed (a) `RepoUrl.Parse` hijacking any one-slash local path as GitHub
shorthand — now excludes drive letters/backslashes/leading `.`or`/`, and `AnalyzeCommand` additionally
prefers local-path existence over shorthand parsing for the positional arg; (b) a genuinely empty/
invalid directory (no `.sln`/`.slnx`/`.csproj` reachable at all) now hard-errors with guidance at exit
2 instead of silently analyzing nothing at exit 0; (c) the stale `--profile debug` (removed flag)
reference in `MarkdownRenderer.AppendCallGraphAvailability` is gone. **Deferred:** the specific
`ProjectRootResolver` `WalkedUp`/`WalkedDown` case — pointing at an empty subfolder that has an
unrelated ancestor/descendant solution — still resolves that ancestor and renders "No types discovered"
at exit 0 rather than hard-erroring or scoping to the ancestor's real content; needs a `ScopeResolver`-
level decision (next session: does an empty resolved scope under a found-but-irrelevant solution count
as "invalid input" or "valid input, empty result"?). "Delete the legacy renderer fallback path entirely"
was also **not done wholesale**: `MarkdownRenderer`/`RenderPlanBuilder` are still the live path for
`--format json`/`html` and for markdown when a host registers its own renderer (verified this is a
real extensibility contract — `GoldenTests.BasicProject_Analysis` exercises it directly) — only the one
stale message was fixed. Re-scope this as its own L0.9b or fold into L1 if a deeper fix is wanted.
