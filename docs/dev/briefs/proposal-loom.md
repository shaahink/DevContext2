# Proposal L — Loom: Truthful Graph, Honest Surfaces, Cold-Agent MCP

> The post-Meridian mega phase. Design authority: `docs/dev/briefs/loom-graph-design.md`
> (read it FIRST, it is not optional). Audit that motivates every line here:
> `eval-results/2026-07-07/SESSION-AUDIT.md`. Tracker: `LOOM-START.md` (repo root).
> Branch scheme: `feat/loom-l<stage>` off `develop` after Meridian merges, or off
> `feat/meridian-m0` until then. Dogfood: `run-aspnetcore-microservices/src`.

## 0. What Loom is

Meridian built the machinery (graph, 20 RPCs, 22 MCP tools, 7 UI surfaces) and proved it
green against scripted gates. The audit showed the gates measure presence, not truth:
the flagship checkout trace is depth-1, name collisions fabricate wiring, services render
as "API"×3, a cold agent fails 15/15 MCP calls, and Context Studio produces zero cards on
first drive. Loom's one sentence: **make what exists true, then make it enjoyable.**

Three product outcomes (re-stated from Meridian, now with truth-gates):
1. **Wiring truth v2** — the checkout flow traces across three services *from a fresh
   clone, cold, via CLI, MCP and UI*, and repos outside the CQRS sweet spot get honest,
   useful graphs instead of near-empty ones.
2. **A .NET lens devs enjoy** — tabs, code pane, context studio, and the service map
   behave like a tool you'd keep open next to your IDE.
3. **An agent surface that works cold** — an agent with zero prior knowledge answers
   real questions in ≤3 calls, and failures teach instead of stonewalling.

## 1. Rules of engagement (every session, no exceptions)

**Pre-session ritual** (≤10 min):
1. Read `LOOM-START.md` handoff block + your stage section here + the design doc
   sections your stage cites.
2. Run the gate battery: `dotnet build` (0w/0e) · `dotnet test --filter Category!=Eval`
   · `pnpm check` · `node eval/mcp-qa/run.js --quiet` (until L5 replaces it).
   **If anything is red before you start, fix or record — never build on red.**
3. State in the tracker, in one line, what artifact will prove your stage done.

**Post-session ritual** (≤15 min):
1. Re-run the gate battery + the truth gates your stage touches.
2. Produce the evidence artifact (fresh run output under `eval-results/<date>/`).
3. Update `LOOM-START.md`: handoff block (overwrite, ≤10 lines), checkpoint row with
   commit hash + artifact path. A code path is not evidence; a green run is.
4. Commit per checkpoint (`feat(l<stage>): …`), push. Never merge unasked.

**Discipline invariants:**
- The design doc's §9 prohibitions are hard rules. The build gate
  `scripts/loom-guards.ps1` (created in L1) greps for banned patterns
  (`new Regex` in Graph/, `NodeId.ForType(`, `fqns[0]`-style first-match picks) —
  keep it green, don't creatively bypass it.
- Every claim in the tracker names a fresh artifact. If you can't produce the artifact,
  the checkpoint is not DONE — write BLOCKED with what's missing.
- Scope changes get a `> scope change:` line under the checkpoint row, never silent
  renumbering.
- **Tests policy:** unit tests that pin *internal string mechanics* of the old builder
  may be deleted with a one-line note when their subject dies (expected around L2–L3).
  Truth gates and goldens may only be *ratcheted* (loosened never; tightened with a
  fresh-run diff pasted in the commit). If a golden changes because output got MORE
  true, update it with `$env:UPDATE_GOLDENS=1` and paste before/after in the commit body.

## 2. Stage map

| Stage | Theme | Sessions | Blocked by |
|---|---|---|---|
| L0 | Truth harness — gates that measure truth, cold | 1–2 | — |
| L1 | Identity spine — SymbolTable, SymbolRef, tiers, Service nodes | 2 | L0 |
| L2 | BodyFacts + seam detectors — the regex funeral | 2–3 | L1 |
| L3 | Semantic-lite tier (assets.json compilations) | 2 | L2 |
| L4 | Flows + projections — one truth, three renderers | 2 | L2 (L3 improves it) |
| L5 | MCP v2 — cold-agent ergonomics | 1–2 | L4 |
| L6 | Workbench repair — tabs, code pane, context studio | 2–3 | L4 (U-fixes can start earlier) |
| L7 | Repo-shape coverage — non-CQRS honesty | 2 | L2, L4 |
| L8 | Close-out — full truth bench, audit, handover | 1 | all |

Sessions are sized so one agent session lands one checkpoint battery with gates green.
When in doubt, land *less* with proof over *more* without.

---

## L0 — Truth harness (do this before touching the engine)

**Goal:** replace presence-gates with truth-gates so every later stage has a fixed star.

Checkpoints:
- **L0.1 Truth expectations.** New `eval/expectations/<repo>.truth.json` for 6 repos
  (dogfood, eShop, TodoApi, CleanArchitecture, DntSite, RazorPages) asserting *named
  facts*: dogfood must contain flow `POST /basket/checkout` with steps
  `CheckoutBasketCommand → CheckoutBasketCommandHandler → BasketCheckoutEvent(bus) →
  Ordering consumer → CreateOrder…` and hop transport `bus`; service set = exactly the 6
  runnables with full names; RazorPages must contain **zero** edges whose endpoints
  live in different sample roots (the fabrication regression test); Blazor archetype
  must NOT be Microservices. Wire into `EvalExpectationTests` (Category=Eval) + a
  `bench.ps1 -Truth` mode. **These will be RED for dogfood/checkout and RazorPages —
  commit them red-but-skipped-with-reason (`[TruthPending("L3")]` attribute) so the
  ratchet exists from day one.**
- **L0.2 Cold-agent MCP QA.** New `eval/mcp-qa/run-cold.js`: no handles, no exact
  routes fed in — the script plays a naive agent (calls tools with plausible-but-wrong
  args first, then discovers via error messages). Gate: every failure response must
  contain (a) what was wrong, (b) a copyable next step. Today this is 0% — record the
  baseline, gate activates in L5.
- **L0.3 UI drive gate.** Promote `src/DevContext.App/scripts/ui-audit-drive.mjs`
  (created in the audit session) to the visual gate: runs headless, asserts tab-strip
  height ≥ 30px (red until L6), New-button preserves other tabs (red until L6), code
  pane non-empty on entry selection (red until L6), context studio produces ≥1 card
  from preset (red until L6). Screenshots land in `eval-results/<date>/ui/`.

**What the agent will get wrong here:** writing expectations that mirror today's broken
output (copying actuals into the truth file). Rule: truth files are written from
*reading the target repo's source*, not from DevContext output. For dogfood checkout,
the truth is in `CheckoutBasketEndpoints.cs` → `CheckoutBasketHandler.cs` →
`BasketCheckoutEvent` → Ordering's `BasketCheckoutEventHandler` — read those files.

**Gate:** truth harness runs; red items enumerated with owning stage; no green-washing.

---

## L1 — Identity spine

**Goal:** typed identity end-to-end; Service nodes; ambiguity as data. Design §1, §2.2.

Checkpoints:
- **L1.1** `SymbolId`, `SymbolRef`, `ResolutionTier`, `SymbolTable` (+ AmbiguityReport)
  in `Graph2/`; unit tests incl. same-short-name-different-project (the RazorPages
  scenario reduced to a fixture).
- **L1.2** `Service`/`Message`/`Store` node kinds; ServiceBoundaryInference (runnable
  detection); Project+Service stamped on every declared node; proto + ProtoMapper +
  TS regen ride along (additive fields only).
- **L1.3** Port ServiceLinks to Service→Service edges (delete `_eventPublishers`
  static; publishers flow through assembler-local state). Dogfood still: 6 ServiceLinks,
  now between Service nodes with full names.
- **L1.4** `scripts/loom-guards.ps1` + CI hook: bans `new Regex` under `Core/Graph`,
  `NodeId.ForType(`, raw first-candidate picks (reviewer greps listed in the script).
- **L1.5** MapBuilder/impact/scope-picker read Service identity (fixes "(unknown)"
  impact buckets + "Default" scope groups + hero card names — UI label fix itself
  lands in L6, but the DATA is right after this).

**What the agent will get wrong:** trying to migrate all of GraphBuilder at once. Don't.
L1 only introduces the spine + services + delete-the-static; body-scan seams still run
on the old path until L2. Also: don't "fix" ambiguity by picking by-project-then-first —
the fix is *keeping* it ambiguous.

**Gate:** build battery green; dogfood numbers within ±5% (493/316 baseline — small
drift from Service nodes is fine, document exact delta); RazorPages fabricated-edge
truth test flips GREEN here (ambiguous edges excluded from traversal); guards script green.

---

## L2 — BodyFacts + seam detectors (the regex funeral)

**Goal:** one structured body pass; detectors replace the 18 regex sites. Design §2.1.

Checkpoints:
- **L2.1** `BodyFacts`/`BodyOp` extraction inside the existing per-file parse (extend
  `SyntaxStructureExtractor`/`CallGraphExtractor` — do NOT add another parse). Content-
  keyed cache entry versioned (`facts-v1`).
- **L2.2** Detectors: MediatRDispatch, DomainEventRaise, IntegrationEventCreation,
  EntityTouch, BusPublish. Each with a fixture test using real dogfood snippets
  (including the `request.Adapt<CheckoutBasketCommand>()` + `sender.Send(command)`
  pattern — the audit's E1 flow, verbatim).
- **L2.3** Assembler consumes SeamMatches; delete `AddSends/AddRaises/AddDataEdges`
  body-regex paths + `StripStringLiterals` + `EstimateProvenance` + `ResolveVariable*`
  + `BuildAllMethodSpans`. GraphBuilder shrinks to the assembler shell (design §2.3);
  rename to `CodeGraphAssembler` when it's under ~400 lines.
- **L2.4** **Flagship fix validated:** checkout Sends edge anchors on the lambda member
  the trace actually walks. `POST /basket/checkout` trace depth ≥5 through CLI. The L0
  `[TruthPending]` on the checkout flow flips to enforced — **this is the stage's
  headline artifact.**

**What the agent will get wrong:** (1) porting regexes 1:1 into syntax walks that
replicate the same wrong anchoring — the fix for E1 is that the *op already knows its
enclosing member* (BodyFacts.Member), so edges anchor correctly by construction; if you
find yourself computing char offsets, stop. (2) Deleting tests wholesale — delete only
tests of dead internals; port the *behaviors* (worked examples in old tests) to detector
fixtures first. (3) Letting WrappedBy stay cartesian — while here, scope WrappedBy to
requests actually dispatched in the behavior's project group.

**Gate:** dogfood Sends ≥ 26 (Meridian's M1.2 number) with equal-or-better tier mix;
checkout truth test GREEN end-to-end (CLI + MCP trace); zero `Regex` references under
`Core/Graph` (guards); bench wall-time within budget (design §8).

---

## L3 — Semantic-lite tier

**Goal:** real symbol binding without MSBuild. Design §6 Tier B.

Checkpoints:
- **L3.1** `SemanticLitePopulator`: per-project `CSharpCompilation` from our trees +
  project refs + bundled framework ref assemblies + NuGet dlls via
  `obj/project.assets.json` → `~/.nuget/packages` paths. Per-project degrade to Tier A
  with Coverage note when assets missing. Stats records tier routing.
- **L3.2** Targeted semantic upgrades (Law R2: upgrade only): receiver types for
  dispatch gating, LocalDecl inferred types, handler interface closure, ambiguous-ref
  arbitration (a real bind can move Ambiguous→Semantic).
- **L3.3** Ratchet: dogfood verified-edge share from 59% to ≥80%; DntSite controllers
  target resolution (34/94 baseline) — measure and record improvement; re-run full
  truth bench.

**What the agent will get wrong:** building compilations eagerly for all projects and
blowing the time budget — bind lazily, only for members that own seam matches or
ambiguous refs (the demand set is small). Also: assuming assets.json exists — every
path must have the degrade branch, tested with a fixture repo without obj/.

**Gate:** truth bench green; tier routing visible in Stats page + CLI stats line;
perf within design §8 budget (record actuals in tracker).

---

## L4 — Flows first-class + projections

**Goal:** compute flows once; render everywhere; per-node lens data. Design §1.4, §3.

Checkpoints:
- **L4.1** `Flow` store on CodeGraph; spine-only Touches/Emits (fixes audit E5
  over-claiming); ServiceHops with transport + provenance.
- **L4.2** Projections: ServiceMap (runnables only, full names, gateway/bus lanes),
  FlowList, EntryTable, LayerBand (per-node) + new `GetGraphFacets` RPC (Meridian P1)
  so the UI lens gets every node's layer/feature in one call.
- **L4.3** Consumers switch: Home hero + Atlas diagram + MCP `overview`/`top_flows` +
  stepper strips all read projections. Delete their ad-hoc walks.
- **L4.4** `ContextPackProjection` server-side round-trip (closes Meridian Trap A);
  Copy/Save = exactly the server pack.

**What the agent will get wrong:** UI still deriving card labels client-side
(`name.split('.').pop()`) — the projection carries `DisplayName`; the UI renders it
verbatim. Also flows recomputed per RPC call — they're computed at assembly and cached
on the graph.

**Gate:** UI drive gate: hero shows `Basket.API`, `Catalog.API`, `Ordering.API`,
`Discount.Grpc`, `Shopping.Web`, `YarpApiGateway` and NO library cards (screenshot
diff); layer lens renders per-node bands on dogfood; `trace`/`flow` MCP outputs match
CLI for the same focus (same Flow object).

---

## L5 — MCP v2: cold-agent ergonomics

**Goal:** an agent with no docs succeeds or learns from every response. Audit §4.

Checkpoints:
- **L5.1** Session ergonomics: single-session default (omitting `handle` uses the only/
  most-recent session; multi-session keeps explicit handles); `analyze` idempotent by
  repo+HEAD.
- **L5.2** Error envelope: every tool failure returns `{error, hint, example}`;
  parameter-binding failures list expected schema; unknown tool name returns the tool
  list. Unknown symbol returns candidates ("did you mean") — never zero-shaped success
  (kills the `impact TotallyMadeUpType → totalAffected:0` trap).
- **L5.3** One resolution path: `resolve`/`find`/`usages`/`impact`/`read_source`/
  `tests_for` all use `graph.Find` ranking (exact > prefix > word-boundary; Types over
  Members; degree tiebreak). `resolve "Order"` must return the Order aggregate #1.
- **L5.4** `flow` tool for real (compact Flow rendering ≤150 tok typical; deep-link to
  `trace` for detail); `get_context`/`config` fuzzy focus with suggestions on miss.
- **L5.5** QA v2: `run-cold.js` becomes the gate (L0.2 baseline → now enforced);
  scripted `run.js` kept as regression harness. Multi-repo ratchet re-recorded.

**What the agent will get wrong:** making error envelopes verbose (hundreds of tokens).
Budget: error+hint+example ≤ 80 tokens. And don't break the existing 22-tool contract —
additive + behavioral fixes only; rename nothing without a deprecation alias.

**Gate:** cold-agent QA: ≥90% of naive-arg calls produce actionable guidance; checkout
question answered cold in ≤3 calls/≤2k tok **without the harness knowing the route**;
tools/list envelope ≤1.5k tok.

---

## L6 — Workbench repair (the "enjoy browsing" stage)

**Goal:** fix every confirmed UI defect; make Context Studio meaningful. Audit §5.
Design-review each fix against `docs/dev/briefs/*fable*` tokens (existing design system).

Checkpoints:
- **L6.1 Tabs.** Strip 32–36px tall, 13px labels, VS Code-style (close on hover,
  active underline, middle-click close — keep), repo favicon-ish glyph per archetype.
  Titlebar **New = `createTab()`** (never closes/cancels others); "open recent"
  replaces-current only via explicit dropdown choice. Closing a cloning/analyzing tab
  shows one-line confirm ("Cancel analysis of X?") — cancellation already plumbs
  through gRPC, verify server logs stop.
- **L6.2 Code pane.** Entry selection shows the endpoint's source immediately
  (read_source on the entry's file:line — data exists); node selection unchanged;
  loading + error states visible (no more silent null). PrismJS verified in web AND
  Tauri.
- **L6.3 Inspector truth.** Insights section filters by graph adjacency (insight
  evidence nodeIds ∩ selected node's 1-hop), chip counts only that; empty state says
  "none reference this node (10 repo-wide)".
- **L6.4 Context Studio v2.** Scope tree grouped by Service nodes (L1 data); preset
  "I'm changing this endpoint" immediately scaffolds cards (endpoint + handler + message
  + consumers + tests-for + config) via ContextPackProjection; card list shows why each
  card is there (provenance chip); Copy produces the server pack (L4.4). Table-stakes
  UX: clicking a scope row adds/removes it, running total updates live.
- **L6.5 Table lens.** Toolbar button next to lens switcher (Shift+E stays as
  shortcut); global shortcut works regardless of focus (capture phase).
- **L6.6 Chrome polish batch.** MCP page status reflects live sessions (no
  "Stopped + refused" while sessions listed); confidence stat renamed/tooltipped
  ("wiring confidence" with formula link) or demoted from headline; icon-size sweep at
  125%/150% DPI (Meridian P5) — screenshot evidence.

**What the agent will get wrong:** restyling instead of fixing behavior (tabs need
*semantics* changes: createTab); fixing Context Studio by adding more empty-state text
instead of making the preset scaffold real cards; testing only in `ng serve` (verify
Tauri build for code pane + clipboard paths).

**Gate:** UI drive gate all-green (L0.3 red items flip); before/after screenshot pairs
committed; `pnpm check` green.

---

## L7 — Repo-shape coverage (beyond the CQRS sweet spot)

**Goal:** honest, useful graphs for plain MVC/Razor, Blazor, WPF/desktop, workers,
Azure Functions — the ".NET repo lens" promise. Audit E3/E4.

Checkpoints:
- **L7.1** Call-spine completion: controller/page/component action → service → EF/HTTP
  leaf via BodyFacts invocations (no MediatR needed). Measure on RazorPages-class and
  CleanArchitecture repos: entries with ≥2-deep spine ≥70%.
- **L7.2** Archetype-shaped projections: Desktop (window/command tree), Worker
  (schedule/queue view), Library (public surface — exists, verify), Blazor (route/
  component tree). Each gets ONE projection + report section, not a new universe.
- **L7.3** Style detection guardrails: multi-sample/docs repos (no unifying solution,
  >N unrelated roots) report "sample collection", never Microservices (E4). Scope
  clarity: root-vs-subfolder closure behavior documented + E9 partial-scope fix.
- **L7.4** Truth files extended to one repo per archetype; full 22-repo bench re-run
  with per-repo verdicts (not just presence).

**What the agent will get wrong:** inventing new node kinds per archetype. The graph
model does NOT change here — archetypes differ in *entry builders + projections* only.

**Gate:** truth bench green incl. new archetype repos; no repo reports an empty-ish
graph without Coverage saying why.

---

## L8 — Close-out

Full gate battery + truth bench + cold-agent QA + UI drive on a clean clone; refresh
`docs/dev/HANDOVER-LOOM.md` (same skeleton as Meridian's but every claim carries its
artifact link); update AGENTS.md files (root + App) with the Loom invariants (§1) so
the *next* phase's sessions inherit the rituals; memory update; tracker closed.

---

## 3. Standing risk table (read before every stage)

| Risk | Mitigation |
|---|---|
| Migration stalls mid-organ (old+new paths both live) | Every stage ends with the old path *deleted* for its organ; compatibility shims listed in tracker with removal stage |
| Numbers drift silently during ports | Every stage records dogfood `nodes/edges/entries/SL/verified%` in the tracker; unexplained drift >5% blocks |
| Agent green-washes truth files | Truth files only change in dedicated commits with source-file citations in the body |
| Perf regression from Tier B | Lazy demand-set binding; bench budget is a gate, not a hope |
| UI fixes regress Tauri | L6 checkpoints require one Tauri smoke per session (`pnpm dev`, manual screenshot) |
| Proto changes break TS | Additive fields only; regen + `pnpm check` in the same commit |
