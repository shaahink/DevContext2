# Proposal T — Tapestry: Real-Repo Truth, Agent Ergonomics, Verified Context

> The post-Loom mega phase. Written 2026-07-15 from a live audit session that drove the engine,
> MCP, and UI against `C:\code\shamshir` (a 14-project Aspire/worker/SignalR trading engine the
> tool had never seen) plus the eval fleet. Evidence for every stage:
> `eval-results/2026-07-15/wrapup-drive/` (maps v1→v3, MCP transcript, cold QA),
> `eval-results/2026-07-11/ui-context-studio-audit.md` (R1–R16),
> `docs/dev/NOTABLE-FINDINGS.md`, `docs/dev/go-to-program/HANDOVER-2026-07-15.md` §4.
> Tracker: `TAPESTRY-START.md` (repo root). Branch scheme: `feat/tapestry-t<stage>` off `develop`.
> Dogfood stays `run-aspnetcore-microservices/src`; **shamshir-class truth** is the new second pole.
> **Audit weave 2026-07-15:** the live blind-drive audit (`eval-results/2026-07-15/feature-design-audit.md`,
> eShop GUI + 8-library CLI) added checkpoints T1.7–T1.9, T2.5–T2.8, T3.7–T3.8, T4.6, T5.6,
> T6.7–T6.11, T7.4 plus gate riders — full spec per checkpoint (evidence, verified loci, traps):
> `docs/dev/briefs/proposal-tapestry-audit-addendum.md`. eShop joins shamshir as an audit pole.

## 0. What Tapestry is

Loom made the graph honest *on the repos we test*. The 2026-07-15 wrap-up session proved the
promise breaks on the first real repo outside the fleet: five hosted services, two SignalR hubs and
an Aspire AppHost were **invisible** (package-gated signals, Program.cs-only walks), every
controller action collapsed onto a duplicate truncated route with a shared wrong target, the style
verdict flipped between two wrong answers, hub traces were depth-1 while worker traces were 13.6k
tokens, MCP tools disagree about how to name a symbol, and the context pack renders `file:` with no
line number. Eight of those bugs are already fixed and pinned (commits `99acf40`, `202c593`).
Tapestry's one sentence: **make the whole surface — detection, graph, MCP, context packs, every UI
page — hold on repos we did not write, and make an agent's first hour cheap.**

Three product outcomes:
1. **Real-repo detection truth** — a shamshir-class repo (startup composed in extension methods,
   shared-framework SignalR, SDK-style Aspire, factory-lambda workers) renders complete entries,
   honest style, and per-service reality — from a fresh clone, cold.
2. **An MCP an agent can drive without reading our docs** — one symbol-addressing convention,
   token-bounded defaults, repo-relative provenance, self-describing heuristics.
3. **Context packs that can be verified** — every section carries file:line provenance, budget is
   actually used, staleness is detectable, and the Studio shows what was cut and why.

Non-goals: no LLM in the core; no new node kinds; no rewrite of Graph2 (the identity spine held —
extend it, don't reopen it).

## 1. Rules of engagement (every session, no exceptions)

**Pre-session ritual** (≤10 min):
1. Read `TAPESTRY-START.md` handoff block + your stage section here + the evidence files your
   stage cites.
2. **Orphan kill is now automatic** (T0.1 landed): `eval/gates.ps1` Step 0 clears leaked
   `DevContext.Server`/`testhost` (and dotnet hosts running the server dll) before building — the
   wrap-up session lost four builds to leaked servers locking `bin/`. Manual form if ever needed:
   `Get-Process DevContext.Server,testhost -ErrorAction SilentlyContinue | Stop-Process -Force`.
3. Run the gate battery (below). Red before you start → fix or record, never build on red.
4. State in the tracker, in one line, what artifact proves your stage done.

**Gate battery** (green before every commit):
```powershell
dotnet build DevContext.slnx                              # 0w/0e — warnings are errors
dotnet test  DevContext.slnx --filter "Category!=Eval"    # fast suite
dotnet test  DevContext.slnx --filter "Category=Truth"    # ratchet (skips = pending)
powershell -File scripts/loom-guards.ps1                  # banned patterns
powershell -File eval/gates.ps1                           # build→tests→eval→CLI --strict
cd src/DevContext.App; pnpm check                         # app stages only
```

**Post-session ritual** (≤15 min): re-run the battery + your stage's truth gates; produce the
evidence artifact under `eval-results/<date>/`; overwrite the tracker handoff block (≤12 lines);
commit per checkpoint (`feat(t<stage>): …`); push; **never merge unasked**.

**Hard-won lessons — these are rules now, each one cost the audit session real time:**
- **R-T1 Detect ≠ render ≠ serve.** A new detection lands in the SAME checkpoint as (a) its Map
  render, (b) its MCP visibility, (c) an eval expectation pinning it. The worker/SignalR blindness
  survived for weeks because detection shipped alone.
- **R-T2 Self-source guard.** Any syntax-pattern signal must respect
  `EntrySurfaceCatalog.SelfNamePatterns` — analyzing the framework's own repo must not flip its
  archetype (the `signalr` eval catch, fixed in `202c593`).
- **R-T3 Fixture realism.** Mirror the real repo shape (ProjectReferences, file layout,
  registration style) before writing a detector test. A bare fixture green-washes — the
  style-detector test passed for weeks while counting phantom projects.
- **R-T4 Doc claims are hypotheses.** Verify DONE/DEFERRED against `src/` before building on them
  (the tracker said "I6 MCP DEFERRED" while 23 tools were live in `src/DevContext.Mcp`).
- **R-T5 One battery at a time.** Never run two dotnet build/test batteries concurrently;
  background CLI drives are fine (they don't build). Stale-binary races produce unreadable results.
- **R-T6 `pnpm install` after any merge that touches `package.json`** — a stale `node_modules`
  fails `ng build` with misleading module-not-found errors while vitest stays green.
- **R-T7 Truth ratchet only** (unchanged from Loom): goldens/truth loosen never; tighten with a
  fresh-run diff pasted in the commit. Truth files are written from reading the TARGET repo's
  source, never from DevContext output.
- **R-T8 Baseline drift table.** Every stage end records dogfood + shamshir
  `nodes/edges/entries(kind counts)/style/verified%` in the tracker; unexplained drift >5% blocks.

## 2. Stage map

| Stage | Theme | Sessions | Blocked by |
|---|---|---|---|
| T0 | Harness & hygiene — orphan-proof gates, shamshir-class fixture, truth re-baseline | 1–2 | — |
| T1 | Detection strength — entry surfaces complete, style/runnables honest | 2–3 | T0 |
| T2 | Graph quality — provenance an agent can trust | 2 | T0 (T1 helps) |
| T3 | MCP v3 — one addressing model, token-bounded defaults | 2 | T2 |
| T4 | Context generation v2 — verifiable packs | 2 | T2 |
| T5 | Context Studio v2 — trust surfaced in the UI | 2–3 | T4 |
| T6 | Workbench & pages revamp — screenshot-driven | 2–3 | T1 (audit can start earlier) |
| T7 | Multi-surface bench + perf honesty | 1–2 | T1, T3 |
| T8 | Close-out | 1 | all |

One agent session lands one checkpoint battery with gates green. Land LESS with proof over MORE
without.

Audit weave adds ≈9–12 sessions: T1 +3 checkpoints · T2 +4 · T3 +2 · T4 +1 · T5 +1 · T6 +5 ·
T7 +1, plus gate riders on T1.4/T1.5/T2.2/T6.3/T6.4. Insertion order: T1.7/T1.8 with T1.1
(same territory); **T2.5 first when T2 opens** (the value unlock); T5.6 folds into the T5.1
session; T6.7–T6.11 join the T6 batch after T6.0's shamshir half.

---

## T0 — Harness & hygiene

**Goal:** the gates can be trusted cold, and this session's eight fixes are pinned forever.

Checkpoints:
- **T0.1 Orphan-proof tooling.** (a) `Server.Tests` fixture kills its spawned `DevContext.Server`
  on teardown; (b) `eval/gates.ps1` step 0 kills orphaned `DevContext.Server`/`testhost`
  processes; (c) `start-dev-bg.ps1`: Kill-All must also kill the server it just started when the
  Angular wait times out (today it leaks it → the next build fails), raise the Angular wait to
  240s, and log job output to files instead of NUL (the audit could not diagnose the timeout).
  **Recommended approach:** reproduce each leak first (run Server.Tests, observe the process
  survive), then fix, then prove: `eval/gates.ps1` twice consecutively from cold, no manual kills.
- **T0.2 CompositionApp fixture.** New `tests/fixtures/CompositionApp` mirroring shamshir's shape:
  `Program.cs` that only calls extension methods; `Configuration/ServiceRegistration.cs` with
  `AddHostedService<T>`, `AddHostedService(sp => sp.GetRequiredService<T>())`, `AddSignalR`;
  `Configuration/MiddlewarePipeline.cs` with `MapHub<T>("/hubs/x")`; a `: Hub` class; controllers
  with `[HttpGet("packs/{id}")]`-style verb-attribute routes; an `Aspire.AppHost.Sdk/x.y.z`
  AppHost with 2 ProjectReferences. Eval expectation pins: Background + SignalR entry sections
  present, composed routes, style NOT Microservices, aspire signal present.
- **T0.3 Truth re-baseline.** Record current dogfood and shamshir numbers in the tracker as the
  drift baseline (dogfood was 436→432 nodes across July snapshots with no record of why — that
  ends here). Extend `bench.ps1 -Truth` to print the per-kind entry counts.

**What the agent will get wrong:** fixing the launcher by increasing timeouts only — the leak on
the timeout path is the actual bug; and writing CompositionApp expectations by copying current
output (violates R-T7 — write them from the fixture's source).

**Gate:** gates.ps1 green twice from cold; CompositionApp eval green; tracker baseline table filled.

---

## T1 — Detection strength

**Goal:** entry surfaces complete and the architecture verdict honest on real repos. Evidence:
shamshir map v1 vs v3 (`wrapup-drive/shamshir-map*.md`).

Checkpoints:
- **T1.1 Catalog-driven entry seeds.** `CallGraphExtractor.EntrySeedFiles` hardcodes 4 detection
  types (endpoints, MediatR, workers, + hubs since `202c593`); gRPC services, Functions, Orleans
  grains, GraphQL resolvers still get no call edges in Map mode → their traces will be as shallow
  as hubs were. **Recommended approach:** give entry-producing detections a common
  source-file-contributing contract (or a catalog-driven registry) so every `SurfaceRole.AppEntry`
  surface feeds the seed set; verify per-kind with a trace ≥2 deep on one repo per surface.
- **T1.2 Gateway archetype rung.** `yarp` eval expects `Gateway`, detector returns `App` (red on
  develop since before this session). The Gateway signal already exists in the catalog — add the
  archetype rung + evidence (AddReverseProxy/ocelot.json), flip the eval green.
- **T1.3 dntsite target gap.** `target-feednews`/`target-feedcomments` red: controller actions
  calling an injected `FeedsService` resolve no target. Reproduce with
  `--focus "GET /Feed/News"`, find where `ResolvePrimaryCall` loses the callee, fix, flip green.
- **T1.4 Runnable & per-service inference.** Shamshir's per-service list shows 2 of 5 runnables
  (Host, CTraderRunner, AppHost missing); `ResearchCli` renders "Unknown" instead of a CLI
  archetype. Use OutputType=Exe + AppHost ProjectReferences (already parsed) for runnable
  detection; per-service style strings for worker hosts ("Worker Service") and CLIs.
- **T1.5 Style-ladder honesty.** A controllers-heavy layered web app must not read
  "MinimalApi (moderate)" (shamshir does). Arbitrate Controllers-vs-MinimalApi by endpoint counts,
  not signal presence; a monolith+worker+Aspire repo should read as what it is.
- **T1.6 Feature-area derivation v2.** Shamshir's module map is `Api (122 entries)` — useless.
  Derive HTTP feature areas from route prefix segments (`/api/addons/*` → addons), non-HTTP from
  namespace/folder.
- **T1.7 Entry taxonomy hygiene** *(audit A2–A5)*. gRPC entries = proto RPC overrides only (eShop
  Basket = 3 real RPCs, not 7 incl. private helpers); `DesktopEntryExtractor` stops emitting plain
  ViewModel methods/animation classes and dedupes `[RelayCommand]` twins (96/181 eShop entries are
  MAUI noise today); Blazor page routes are UI entries, never `kind:HttpEndpoint` (fixes the
  "49/56 anonymous endpoints" insight); title collisions get a version/action/file:line
  disambiguator (kills NG0955 dup-keys in 4 components). Gate: eshop per-kind counts pinned from
  source (R-T7); zero NG0955 in the UI drive.
- **T1.8 Kind single-sourcing** *(audit "gRPC 75")*. `EntryTableProjection` joins
  `snapshot.Entries` (builders carry the true `EntryPointKind`); delete `DeriveEntryKind`
  tag-parsing + its silent `PublicApi` default; App chips map 1:1 to `EntryPointKind`.
  Gate: facet counts == table rows == deck chips on eShop + dogfood + shamshir.
- **T1.9 Topology noise** *(audit A16/D)*. Tests/samples/benchmarks out of the service diagram,
  services count, most-depended-upon, dead-code (project-level classification, not path regex);
  project-vs-package rows deduped. Gate: eShop 14 services; MediatR most-depended = MediatR,
  not MediatR.Examples.

**What the agent will get wrong:** adding new extractors (banned — reform in place, see
`docs/product/DETECTION-GUIDE.md`); and fixing T1.5 by special-casing shamshir — the arbitration
rule must be stated in evidence terms and hold across the 22-repo bench.

**Gate:** shamshir map shows ≥5 per-service rows with honest styles; yarp + dntsite evals flip
green; 22-repo bench presence run has zero regressions; CompositionApp still green.

---

## T2 — Graph quality

**Goal:** every edge and location in the graph is something an agent can act on without checking.

Checkpoints:
- **T2.1 Production-first DI provenance.** Shamshir trace resolved `ITradeRepository` →
  `SqliteTradeRepository` via a registration in `tests/.../InProcessEngineSmokeTests.cs:89`.
  Resolves edges must prefer production registrations; a test-only registration is last-resort and
  tagged as such (render: `di SqliteTradeRepository [test-only registration]`).
- **T2.2 Member line numbers.** Member nodes created by entry builders and the call graph carry no
  `LineNumber` → context packs render `RunsController.cs:` (trailing colon, Q7 transcript). Stamp
  decl lines from BodyFacts/StructureFacts at node creation. This is the enabler for T4/T5
  verification. *Audit riders:* Razor entries must not all stamp `:1`; Call Stack rows carry the
  member's own decl line (not the entry's); Inspector Code / `read_source` MEMBER mode returns the
  member's span — today the pane shows a raw file window that drifts into the NEXT handler (A13).
- **T2.3 Target quality.** Targets render bare member names (`RunAsync`, `GetAllAsync` in
  top_flows) — always `Type.Method`; direct-EF actions currently target `TradingDbContext` —
  label them `direct data access (TradingDbContext)` so a reader knows there is no service layer;
  audit the wrong-primary class (`POST /api/system/reset → BacktestOrchestrator.GetAll`): a
  mutating verb must not pick a getter when a mutating callee exists on the same member.
- **T2.4 Type-focus trace shaping.** A Type focus (`BacktestOrchestrator`) opens with
  "(106 more branches omitted)" before any content. Group a Type entry's members, walk top-N by
  out-degree, name the omission per group. (The MCP token cap rides in T3.3.)
- **T2.5 Param-passed dispatch seam** *(audit A1 — the flagship-flow killer)*. eShop
  `POST /api/orders/draft` traces exactly 2 nodes: `ResolveArgTarget` only correlates `Send(x)`
  args with locals; a method-parameter command resolves to null → no seam. Fix in Graph2:
  `BodyFacts` gains parameters (`BodyFactExtractor`); `ResolveArgTarget` falls back to the named
  parameter's declared type (approx tier); `MediatRDispatchDetector` normalizes member-access
  receivers (`services.Mediator`). Fixture: param-passed-command endpoint in CompositionApp.
  Gate: eShop `/draft` traces ≥3 hops (entry → handler → domain callee); Sends counts only rise.
- **T2.6 One event join** *(audit A10)*. eShop's RabbitMQ backbone is invisible: event board = 1
  approx domain-event row, one-pager says "0 cross-service", the pack flow shows CrossService hops
  under the wrong node. Join publisher→event→consumer ONCE from Graph2 seams (BusPublish +
  IntegrationEventCreation + consumer entries); board, one-pager, and flow markers render from
  that one projection; delete the legacy project-name joins (`GraphBuilder.cs:~2099-2350`).
  Gate: eShop board ≥8 integration-event rows pinned from source; three surfaces agree.
- **T2.7 `global` never rendered** *(audit A7)*. Display fallback namespace → project → folder
  (eShop's `OrdersApi` is genuinely namespace-less — it's a display concern, don't fake the
  extraction). Gate: zero standalone "global" group labels across the fleet's rendered output.
- **T2.8 Old-graph retirement cleanup** *(audit §0b verdict)*. Graph2 is already the substrate and
  the regex body-scans are gone; finish it: kind tags retired (T1.8), stale `AddSends` comments
  removed, `GraphBuilder.cs` (121 KB) split into assembler modules. Gate: byte-identical dogfood
  drift row on the split commit (`analyze --no-cache`).

**What the agent will get wrong:** T2.1 by filtering `tests/` paths with a regex — use
`NoiseFilter.IsProductionEntrySource` (exists, already encodes this); T2.2 by re-parsing files —
BodyFacts already knows every member's line.

**Gate:** CompositionApp + dogfood context packs show `file:line` on every node; shamshir DI wiring
cites production files only (or tags test-only); truth bench green.

---

## T3 — MCP v3: one addressing model, bounded output

**Goal:** an agent's first hour costs 10× fewer tokens. Evidence:
`wrapup-drive/mcp-shamshir-transcript.md` (Q1–Q12) + `mcp-cold-qa.md` (11/11 actionable).

Checkpoints:
- **T3.1 Unified symbol addressing.** `impact`/`tests_for`/`read_source` demand `nodeId` while
  `trace`/`get_context` take `focus` and `find`/`resolve` take `query` — three naive calls died on
  parameter names (Q8/Q11/Q12). Every symbol-taking tool accepts `query`, resolved through
  `graph.Find` ranking; `nodeId` stays as the precise form. Error envelopes remain for true misses.
  **Budget: error+hint+example ≤80 tokens (unchanged from Loom L5).**
- **T3.2 `entrypoints` summary default.** 128 entries = ~10k tokens (Q2). Default: counts per
  kind + top-N by score + `full:true` escape hatch; ≤1.5k tokens on shamshir.
- **T3.3 `trace` token budget.** `trace("EngineWorker")` = ~13.6k tokens (Q4). Add
  `budgetTokens` (default ~4k): depth/fan-out adapt, omissions named per subtree, deep-link hint
  to `read_source`/`flow` for the rest.
- **T3.4 `config` latency.** 10.5s on shamshir (Q10) vs ≤100ms for every other query — profile
  (likely an on-demand file scan), precompute at analyze or cache per session.
- **T3.5 Repo-relative paths + clean identity.** Absolute `C:\code\shamshir\…` paths in packs and
  `read_source` waste tokens and leak machine layout; `overview`'s "Start here" lists `List` and
  `TradingDbContext` (framework/store noise) — filter by the same noise rules as targets.
- **T3.6 Self-describing heuristics.** `tests_for`/`config` responses carry a one-line `method`
  note (what was scanned, what 0 means) — Bucket C #1; document `flow` vs `trace` in both tools'
  descriptions — Bucket C #2.
- **T3.7 CLI query parity** *(audit A15)*. `devcontext query entrypoints|stats|trace` are stubs —
  all three fall through to the overview render (verified in `QueryCommand.cs`; `trace` ignores
  `--focus`). Implement against the snapshot with the MCP/kernel JSON envelope (one shape, two
  transports): `entrypoints` = entry list + per-kind counts, `stats` = GraphStats + per-kind,
  `trace` = `GraphQuery.Trace(focus, depth)`. Gate: gates.ps1 CLI matrix asserts each op.
- **T3.8 Report hygiene** *(audit C5/D)*. Run-report telemetry only under `--stats`; LIBRARY
  PUBLIC SURFACE capped (top-N + "…N more, use `--format json`" — MassTransit's report is 476 KB
  today); footer drill-in example derived from the repo's own entries (not an eShop route);
  reconcile the two "public types" numbers. Gate: MassTransit report <40 KB; footer example
  resolves on all 8 library repos.

**What the agent will get wrong:** breaking the 23-tool contract — additive params and behavioral
fixes only, rename nothing; and letting summary modes hide honesty (the summary must state what it
omitted and how to get it).

**Gate:** cold QA ≥90% actionable (regression); dogfood checkout ≤3 calls/≤2k tok (regression);
NEW: shamshir "what runs in the background?" answered ≤3 calls/≤2k tok cold; `entrypoints` ≤1.5k
tok and `trace` ≤4k tok defaults on shamshir, transcript re-run committed as evidence.

---

## T4 — Context generation v2: verifiable packs

**Goal:** a pack a human or agent can check against source. Evidence: Q7 transcript
(612/4000 tokens used, `file:` without line, absolute paths, empty `# ` title) + the Context
Studio audit's GAP-1 (no verification mechanism at all).

Checkpoints:
- **T4.1 Pack identity + locations.** Header: repo name, analyzed-at timestamp, git HEAD, entry
  focus; all locations repo-relative `file:line` (rides on T2.2).
- **T4.2 Budget utilization.** 612/4000 is under-filling — expand salient→full bodies (spine-first)
  until ~85% of budget; every truncated body carries `… (+N lines)`.
- **T4.3 `config` + `tests` sections for real** (audit R9 — today they are dead client stubs that
  spin and vanish): config keys touched by the traced spine; `tests_for` hits for spine members.
- **T4.4 Per-section provenance + confidence** (audit R10): each section lists the source
  `file:line` set it derived from + its resolution-tier mix (`5 verified · 2 approx`).
- **T4.5 Staleness verification API** (audit R6, engine half): `VerifyContextPack` compares
  snapshot file hashes vs disk → per-section `stale` flags + changed-line counts. Proto + server +
  MCP exposure (`verify_context`).
- **T4.6 Pack assembly correctness** *(2026-07-15 audit C2)*: the contracts card is a verbatim
  duplicate of signatures (the UI even labels it "signatures: 597 tok") — give it its own section
  (interfaces/DTOs/message contracts from the spine); empty sections omitted AND recorded in
  `omitted[]` (today "Entities — 0 tok" ships); archetype header filled (today `_Archetype: _`);
  HTML comment markers out of the human copy. Gate: CompositionApp pack golden asserts
  signatures ≠ contracts, no empty sections, non-empty archetype.

**What the agent will get wrong:** building T4.5 as a diff engine — v1 is hash + line-count deltas
per file, nothing more; and spending the budget on prose — sections stay structural, bodies are
where tokens go.

**Gate:** packs on dogfood + shamshir: ≥85% budget use, every node `file:line`, `verify_context`
flips stale when a spine file is edited mid-session; MCP `get_context` and Studio Copy produce the
identical pack (Trap-A regression).

---

## T5 — Context Studio v2: trust in the UI

**Goal:** the Studio stops silently lying. Source: `ui-context-studio-audit.md` §9 (R1–R16).

Checkpoints:
- **T5.1 Quick wins** (≤1 session): R1 `omitted[]` rendered in BudgetPanel; R4 error state +
  retry in CompositionView (today a failed RPC = spinners stop, nothing said); R5 correct save
  extension for plain format.
- **T5.2 Verification panel** (R6 UI over T4.5): per-section accuracy/staleness ledger per the
  audit's §5.3 sketch; [Re-analyze] and [Refresh verification] actions.
- **T5.3 Provenance & export round-out:** R7 per-card copy; R8 JSON export; provenance chips show
  `file:line` (not filename-only, audit §4.2) and click through to the code pane.
- **T5.4 Entry-kind presets.** "I'm changing this endpoint" exists; add worker/hub presets
  ("I'm changing this hub method" → hub method + orchestrator spine + consumers + tests) — the
  anchors exist since `202c593`.
- **T5.5 Card content honesty** (deep-QA findings 40–42, 50): card previews must show actual
  section content, not a title echo ("Flow: /ProductList"); provenance chips must cite each
  card's OWN source set, not the entry's line five times; one unit everywhere (server tokens —
  the `~10L` line-estimates disappear when T4.3 makes tests/config real); scope-picker error
  badges (red icon on `DiscountProtoServic…`) get a tooltip saying what is wrong.
- **T5.6 Recompute-on-change** *(2026-07-15 audit C1 — captured live: compose at 4k → slider to 1k
  (meter goes red "over budget") → Copy/Save/plain: all four exports byte-identical, header still
  "Budget: 4000")*. Verified: `context-studio.ts` calls `getContextPack` only when cards are
  added; nothing observes `budgetTokens` for re-fetch; Copy serves the frozen `serverPackMarkdown`.
  Fix: budget/intent changes re-pack (debounced) — or flip a visible "stale — Re-pack" state that
  disables Copy/Save; assert plain ≠ markdown bytes; save name `${repo}-context-${date}.{md|txt}`;
  preset "I'm changing this endpoint" disabled at 0 selection with a hint (today it silently
  no-ops); the select→"Add to context" two-step gets a count-badged primary affordance. Gate:
  ui-audit-drive asserts copy@4k ≠ copy@1k, plain ≠ markdown, header budget matches the slider.

**What the agent will get wrong:** fixing R4 with a toast only — the cards themselves must show
failed state; testing only in `ng serve` (verify one Tauri smoke per session, Loom L6 rule).

**Gate:** `ui-audit-drive.mjs` extended: tiny-budget run shows omitted list; server-kill mid-load
shows error state; verification panel renders on a stale pack. Before/after screenshots committed.
`pnpm check` green.

---

## T6 — Workbench & pages revamp (screenshot-driven)

**Goal:** every page audited against BOTH poles (dogfood microservices + shamshir monolith), then
fixed. The wrap-up session launched the app but did not finish the page-by-page audit — that audit
is T6.0, not optional context.

Checkpoints:
- **T6.0 Full UI audit artifact.** Drive all 7 pages against dogfood AND shamshir with
  `audit-screenshots.mts` + manual Playwright passes; write
  `eval-results/<date>/ui-pages-audit.md` with per-page: data shown, quality, gaps, screenshots.
  Rigor bar: `ui-context-studio-audit.md`. *(2026-07-15: the eShop/microservices pole is DONE —
  `eval-results/2026-07-15/feature-design-audit.md`, drive scripts `audit-drive*.mts`; the
  shamshir pole remains.)*
- **T6.1 Home/Atlas on non-microservice repos.** Service-map hero and per-service cards must
  degrade honestly for a monolith+workers+hubs repo (T1.4 data): Web + worker + hub lanes, no
  empty microservice scaffolding.
- **T6.2 Graph canvas revamp.** Worker/hub/CLI entries first-class in the Service lens; entry-kind
  glyphs; edge styling by resolution tier (verified solid / approx dashed) — honesty visible in
  the picture; lens defaults per archetype.
- **T6.3 Insights honesty pass.** Noise: "ViewModel-View: 0 VMs" on non-MVVM apps, dead-code
  chips naming framework-shaped types (finding 45). Copy bugs: "every WRITE endpoint needs a
  validator" evidenced by GET endpoints; "Desktop apps are organised in feature areas" rendered
  on a web microservices repo (finding 44 — templated copy must be archetype-aware). "Module map:
  1 feature areas" suppressed below thresholds (T1.6 fixes the data). *Audit riders (A11/D):*
  confidence percentages become tier words or disappear, and ranking is tier-first (the audit saw
  a "12% conf" Warning ranked #1); "missing validation" counts write endpoints only; dead-code
  suppresses convention-instantiated shapes (EF `IEntityTypeConfiguration`, DI extension classes);
  "internal hubs" gets a ≥3-refs floor (today "(1 refs)" is called heavily-referenced);
  ViewModel-View self-suppresses when either side is 0 or edges are 0 (fires on Polly/Hangfire).
- **T6.4 MCP + Settings truth.** MCP page reflects live sessions (verify multi-session); Settings
  storage shows real cache paths/sizes. *Audit rider (B12):* Settings→Server shows the LIVE
  `serverBaseUrl()` + health target, not the 5179 constant (verified wrong under an injected URL).
- **T6.5 Keyboard reality** (finding 37): the activity bar declares single-key shortcuts
  (h/e/a/i/m/c/s) that do not navigate — wire them globally (capture phase, not while an input is
  focused) or remove the affordance; verify the `/` route cannot get stuck rendering Settings
  after a route-restore (finding 49); extend `ui-audit-drive` with a keyboard assertion battery
  (`?`, Ctrl+K, Shift+E, single-key nav).
- **T6.6 Theme parity** (findings 38–39): light mode currently themes the routed page while the
  titlebar/tab-strip/activity-rail stay dark — the shell must follow the mode; the 3 vibes
  (Modern/Terminal/Hacker) × 3 modes get a 6-screenshot matrix in the drive gate so unthemed
  surfaces can't ship.
- **T6.7 Hero graphs draw edges** *(audit B1)*: home "How services connect" and Atlas "Service
  diagram · 36 dependency edges" both render an edge-less single-column card stack while the
  Service lens proves the cytoscape renderer works — `service-map-hero` reuses it (compact,
  non-interactive); tests collapse into a lane (T1.9 data); the Atlas MAP header becomes
  structured chips (TFMs deduped/humanized, no raw text wall). Gate: eShop hero screenshot shows
  edges; no `;`-joined TFM strings in the DOM.
- **T6.8 Names, paths, metric meaning** *(audit A8/A14/B5/B6)*: kill `split('.').pop()` display
  names (`atlas-page.ts:224` corrupts the one-pager into "API"×5; also `service-cards.ts:47`,
  `home-page.ts:131`) — strip only the common solution prefix; repo-relative paths in the Details
  rail / Call Stack / Table RESOLUTION (UI half of T3.5) with a copy-absolute affordance; deck
  middle-ellipsis keeps the distinguishing route tail (15 identical "GET /api/c…" rows today);
  every metric chip ("N% verified", "atlas N/100", SHARED, flow "0%") gets a one-source definition
  + tooltip, or is dropped; reconcile CLI-vs-server graph counts (1137/886 vs 1156/904) or label
  them as different graphs. Gate: one-pager full names; grep gate on `split('.').pop()`; drive
  dumps contain zero `C:\` paths.
- **T6.9 First-run & session** *(audit B2–B4)*: deck default sort = wired-and-deep first (today
  it opens on an unwired Blazor `GET /` with a one-node trace); "Trace checkout" resolves to the
  deepest matching flow, never an unwired UI route; START-HERE tiles persist across revisits
  (verified: present right after analysis, gone on return); the agent tile links to the MCP page
  (no `pnpm dev:web` developer leakage); the client reattaches to the server's latest session for
  the repo on boot (server half already exists — the MCP page lists sessions). Gate: drive —
  fresh context reattaches without re-analyze; tiles on revisit; Trace-checkout lands ≥3 hops.
- **T6.10 MCP page ergonomics** *(audit B9/A15)*: the page truncates handles (`slice(0,8)`,
  `mcp-page.ts:142`) that its own TRY-A-TOOL then rejects (`[not_found] Unknown session handle`) —
  full-handle copy + a "use this session" prefill button (zero typing); live feed tags rows by
  origin (UI vs agent) and defaults to agents (one page render logged 163 UI calls / ~99k tok);
  validate the host-config `command` resolves on the packaged install. Gate: try-a-tool succeeds
  via the button; feed default-filtered screenshot.
- **T6.11 One-pager fidelity** *(audit C3 — rides T2.6 + T6.8)*: clipboard export stays, file
  download added; content inherits full names + the event join + honest cross-service counts.
  Gate: eShop one-pager golden — full service names, ≥8 event rows, counts consistent with flows.

**What the agent will get wrong:** restyling instead of fixing data (T6.1 needs T1.4's runnables,
not CSS); auditing only dogfood (the whole point is the second pole); asserting keyboard shortcuts
by dispatching synthetic events at the handler (drive real `page.keyboard.press`).

**Gate:** ui-audit-drive all green incl. the keyboard battery + theme matrix; the T6.0 audit doc
exists with screenshot pairs for every changed page; `pnpm check` green.

---

## T7 — Multi-surface bench + perf honesty

Checkpoints:
- **T7.1 Bench extension.** Add CompositionApp + one real gRPC service app + aspire-samples to the
  bench; refresh per-repo verdicts (Loom L7.4 table).
- **T7.2 Perf regression check.** `devcontext-bench` baseline vs the `project-perf-regression-v2`
  memory's Map call-graph edge-explosion note; measure the T0–T2 additions (content probe over
  cached text must stay <100ms warm).
- **T7.3 Stage-waterfall honesty.** Shamshir: stages sum to ~25s of a 51s wall — semantic-lite,
  graph assembly, flows, and insights are invisible in the report. Every second lands in a named
  row; `--stats` waterfall sums to ≥95% of wall time.
- **T7.4 Page-render RPC budget** *(audit B11)*. Rendering home+atlas fired ~150 `GetTrace` +
  dozens of `GetNode` in ~2s (flows/hub-radar recomputed client-side per visit) — add a
  server-side session memo for flows/facets (or a shared `top_flows` RPC) and a drive assertion:
  ≤15 RPCs per page navigation. Gate: drive RPC counter green; a fresh page load shows <20
  UI-origin calls in the MCP feed.

**Gate:** bench within budget, no verdict regressions, waterfall accounts for ≥95% wall.

---

## T8 — Close-out

Full battery + truth bench + cold QA + UI drive on a clean clone; `docs/dev/HANDOVER-TAPESTRY.md`
with per-claim artifact links; AGENTS.md (root + App) updated with the T-rules; memory update;
tracker closed.

---

## 3. Standing risk table

| Risk | Mitigation |
|---|---|
| Stage lands detection without render/serve/eval | R-T1 is a review-blocking rule; the gate lists all three |
| Truth files copied from output | R-T7 + truth-file changes only in dedicated commits citing target-repo sources |
| Orphaned processes poison results | T0.1 first; until then the pre-session kill ritual |
| Token budgets regress silently | T3 gates are numeric (≤1.5k/≤4k) and re-run in T8 |
| UI fixes regress Tauri | one Tauri smoke per T5/T6 session |
| Numbers drift during ports | R-T8 drift table every stage end |
| Proto changes break TS | additive fields only; regen + `pnpm check` in the same commit |
