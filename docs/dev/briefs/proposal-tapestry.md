# Proposal T — Tapestry: Real-Repo Truth, Agent Ergonomics, Verified Context

> The post-Loom mega phase. Written 2026-07-15 from a live audit session that drove the engine,
> MCP, and UI against `C:\code\shamshir` (a 14-project Aspire/worker/SignalR trading engine the
> tool had never seen) plus the eval fleet. Evidence for every stage:
> `eval-results/2026-07-15/wrapup-drive/` (maps v1→v3, MCP transcript, cold QA),
> `eval-results/2026-07-11/ui-context-studio-audit.md` (R1–R16),
> `docs/dev/NOTABLE-FINDINGS.md`, `docs/dev/go-to-program/HANDOVER-2026-07-15.md` §4.
> Tracker: `TAPESTRY-START.md` (repo root). Branch scheme: `feat/tapestry-t<stage>` off `develop`.
> Dogfood stays `run-aspnetcore-microservices/src`; **shamshir-class truth** is the new second pole.

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
2. **Kill orphans first** (until T0.1 makes this automatic):
   `Get-Process DevContext.Server,testhost -ErrorAction SilentlyContinue | Stop-Process -Force`
   — the wrap-up session lost four builds to leaked servers locking `bin/`.
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
  verification.
- **T2.3 Target quality.** Targets render bare member names (`RunAsync`, `GetAllAsync` in
  top_flows) — always `Type.Method`; direct-EF actions currently target `TradingDbContext` —
  label them `direct data access (TradingDbContext)` so a reader knows there is no service layer;
  audit the wrong-primary class (`POST /api/system/reset → BacktestOrchestrator.GetAll`): a
  mutating verb must not pick a getter when a mutating callee exists on the same member.
- **T2.4 Type-focus trace shaping.** A Type focus (`BacktestOrchestrator`) opens with
  "(106 more branches omitted)" before any content. Group a Type entry's members, walk top-N by
  out-degree, name the omission per group. (The MCP token cap rides in T3.3.)

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
  Rigor bar: `ui-context-studio-audit.md`.
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
  1 feature areas" suppressed below thresholds (T1.6 fixes the data).
- **T6.4 MCP + Settings truth.** MCP page reflects live sessions (verify multi-session); Settings
  storage shows real cache paths/sizes.
- **T6.5 Keyboard reality** (finding 37): the activity bar declares single-key shortcuts
  (h/e/a/i/m/c/s) that do not navigate — wire them globally (capture phase, not while an input is
  focused) or remove the affordance; verify the `/` route cannot get stuck rendering Settings
  after a route-restore (finding 49); extend `ui-audit-drive` with a keyboard assertion battery
  (`?`, Ctrl+K, Shift+E, single-key nav).
- **T6.6 Theme parity** (findings 38–39): light mode currently themes the routed page while the
  titlebar/tab-strip/activity-rail stay dark — the shell must follow the mode; the 3 vibes
  (Modern/Terminal/Hacker) × 3 modes get a 6-screenshot matrix in the drive gate so unthemed
  surfaces can't ship.

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
