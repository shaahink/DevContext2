# PLAN — R1→R2→R3 execution plan (authored 2026-07-27)

> Owner decision: run R1 and R2 first (interleaved, token-optimized batching), do R3 on the cleaner
> engine. R4 is a parallel lane for a spare session. Claude's sequencing votes were adopted (§1).
> A fresh session starts here: read §2 STATUS, then ONLY the strand doc + files your session needs
> (§5 token rules). R1–R4 docs hold the detail; this doc is the execution overlay — do not duplicate
> their content here, reference it.

## 1. Decisions adopted (don't re-litigate)

- **Interleave R1/R2, don't run full breadth first.** Build the truth-matrix instrument + run it
  over a 12-repo pole set (matrix v1), then start Batch A against those cells. Breadth widens
  incrementally: +10 repos at each batch close, full 47 + loop-until-dry by Batch E close.
  The instrument matters more than the exhaustive first run.
- **Base: ask the owner to sign the Prism merge (`feat/prism-d1…d5` → `develop`) at S1 open.**
  If signed → base = fresh `develop`. If not → base = d5 tip (fa9d706). NEVER merge unasked.
- **One program branch `feat/graph-v2`** off the base. Commit prefixes `r1:` / `batchA:` … `batchE:`.
  Batches are sequential on this one branch (no per-batch branch juggling). Owner-signed merge to
  develop at program close (or a mid-point checkpoint after Batch B if the owner wants one).
- **Batch order A→B→C→D+E** as in R2. D and E share one session (D is mechanical); still two batch
  closes — launch D's full battery DETACHED and start E while it runs.
- **R3 waits for Batch B** (canvas/workspace mocks need true edges + transports). R3 is
  owner-interactive; schedule it after S3, overlapping S4/S5 if convenient.
- **R4 dogfood grading is fairest after Batch B** (judge usefulness on a true graph). R4 *fixes*
  (items 1–7, 11–12) are server-local and can run any time in a parallel session/worktree;
  primitives 8–10 + trace defaults coordinate with Batch E (one-trace-contract).

## 2. STATUS (update at every session close — this is the cold-start entry point)

- [x] S1 — CLOSED 2026-07-27: base decided (develop @ 8dbb510) · program assets committed ·
      `eval/graph-truth.ps1` built (7 checks + `query graphdump` CLI op; expectations in
      `eval/expectations/graph-truth/`) · matrix v1 (12 poles) at
      `eval-results/2026-07-27/graph-truth/MATRIX.md` · DC3/DC8 probes ANSWERED (in MATRIX.md) ·
      Batch A acceptance cells DECLARED (in MATRIX.md) · Batch A [audit] refs re-verified exact
- [x] S2 — CLOSED 2026-07-27: Batch A landed (SymbolCanon structural ids · CallGraphBinder through
      SymbolTable · one compilation · NameResolver collapsed · seam production gate) · matrix cells
      flipped as declared + 4 hypothesis corrections (MATRIX.md §Batch A CLOSE) · battery+bench
      verdicts recorded in session log below
- [x] S3 — CLOSED 2026-07-28: Batch B landed (transport client registrations · Aspire topology ·
      [Command] verb detection · external-target policy) · matrix widened to **23** poles (2 PLAN-listed
      repos were duplicates on disk — swapped, see MATRIX §Widen-set deviations; +1 `gitversion-new-cli`
      pole) · battery + bench verdicts in the session log below
- [x] S4 — CLOSED 2026-07-28: Batch C landed (all 4 R2 §2.C items — multi-solution scope + `--sln` +
      scope note · style suppressed at the detector for Library/CliTool + 2 accuracy root-causes ·
      test-support classification · entry-target quality) · matrix widened to **32** poles
      (`gitversion-new-cli` retired for a repo-root `--sln` read + a new `gitversion-default` pole) ·
      every declared cell flipped · deny-list redundancy probe answered (KEEP BOTH) · battery + bench
      verdicts in the session log below
- [x] S5 — CLOSED 2026-07-28: Batch D landed (hygiene + perf riders; acceptance was "nothing moves" and
      **0 cells moved** across 44 pole comparisons) · Batch E landed (one `TracePolicy`, one trace build
      per request, RESULT/NEXT retired, three number pairs reconciled, all four inherited cells
      root-caused with per-edge evidence) · matrix = **full 47** · **R1 loop-until-dry NOT confirmed —
      a new class appeared in the widen set** (see the session log: `HotChocolate` cannot be analysed
      inside a usable budget). Verdicts in the session log below.
- [~] S6 — R3 decision session OPENED 2026-07-28. **D-A DECIDED + LANDED** (owner picked the A1+A2
      hybrid: centre-pane altitude follows focus); `docs/dev/research/DECISIONS.md` written and is now
      the record for this strand. **D-B … D-H remain OPEN** — the next R3 session continues there,
      starting with D-B (canvas semantic language), which D-A promoted in importance by making the
      canvas the landing surface. Verdicts in the session log below.
- [ ] S7 — R3 continued: D-B (canvas language) → D-C/D-D (library + CliTool archetypes) → D-E…D-H ·
      render kernel built AFTER the decisions it must serve · re-point `screenshot-gate.mts` as pages land
- [ ] R4 (parallel lane) — fixes landed · dogfood run · REPORT.md graded

Session log (one line each: date · what closed · surprises):
- 2026-07-27 · S1 steps 1-2 done ahead of session: Prism merge signed+landed (develop 8dbb510,
  pushed), `feat/graph-v2` created off it, program assets committed. Next session starts at S1
  step 3 (build `eval/graph-truth.ps1`). eval-results/2026-07-19/ left untracked (clutter = open
  owner call).
- 2026-07-27 · S1 CLOSED: `query graphdump` op + graph-truth harness + 12-pole matrix v1 + probes +
  Batch A acceptance. Surprises: functions-app graph is 3 nodes (Functions triggers invisible —
  DC8 breadth confirmed at pole level); GitVersion fixture-hub lives in the INSIGHTS surface, graph
  top-5 is clean; GitVersion analysis swallows both sln trees incl. name-colliding projects
  (GitVersion.Core ×2 — DC6-adjacent identity hazard); eShop handler-join unhandled = exactly
  IdentifiedCommand (DC1 verified case isolated); CleanArchitecture repo is 4-sln so its
  handler-join FAIL is DC6-contaminated (canary = declared cells only). Next: S2 Batch A
  (R2 §2.A), acceptance = MATRIX.md declaration.
- 2026-07-27 · S2 CLOSED (Batch A): all 5 R2 §2.A steps landed on one branch commit — SymbolCanon
  (nested+arity type ids, `Type::Member` node keys, use-site arity on SymbolRef), CallGraphExtractor
  DELETED (CallGraphBinder produces edges from BodyFacts through SymbolTable; Ambiguous→skip; DI
  iface→impl only on unambiguous evidence; entry-seeded closure kept), UpgradeCallEdges deleted
  (edges born against THE SemanticLite compilation; razor @code rides BodyFacts + the one
  compilation), NameResolver deleted (SymbolTable.ResolveName, pass-through on ambiguous),
  IsSelfCallNoise + Mediator-Contains deny-lists deleted (data-access noise list KEPT — not yet
  redundant, note in MATRIX). Must-flip cells ALL landed; CA handler-join 19→5. Surprises: (1) the
  biggest defect found mid-batch was NEW — member canonicals in the type short-name index made every
  explicitly-constructed class "ambiguous" (blanked entry targets repo-wide; fixed by separate
  member tier, pinned by test); (2) seam origins were the ONE ungated join path — test bodies
  (Moq chains) minted Sends→System.Boolean + test→prod call seams that honest resolution EXPOSED
  (baseline ambiguity had hidden them); production+scope gate added, which also flipped Polly+
  Spectre hub-sanity to PASS early; (3) two S1 acceptance hypotheses were WRONG on evidence —
  eShop ProductImageUrlProvider + all 10 OrchardCore dup-names are REAL cross-project DI wiring
  (expectations corrected with edge lists); eShop bus 5→9 links is resolver-COMPLETED truth (all
  verified real). Member keys carry no method-arity (documented deviation — producers can't know
  it). Snapshot schema v6. CLOSE VERDICTS: full battery GATE: PASS unqualified (all 8 steps, eval
  re-ran fresh post-expectation-corrections — log gates-s2-close2.txt; the first run's only red was
  an app graph-layout spec timing out 5052ms vs its 5000ms cap under vitest load, passed clean on a
  quiet machine) · bench PERF-2026-07-27-2348: DntSite Map 34.5s→24.9s (−28%), Trace 30.2→26.7s;
  PERF-2026-07-27-2343: OrchardCore Map 30.8s→14.1s (−54%) — the deleted second compilation +
  UpgradeCallEdges pass show up directly; perf rider PASSED with headroom (2343's DntSite 54s row is
  vitest-contention noise, superseded by 2348). Next: S3 Batch B (R2 §2.B) transports+joins,
  informed by the S1 DC3/DC8 probe answers; declare Batch B acceptance at open + widen matrix to 22.

- 2026-07-28 · S3 CLOSED (Batch B): three of R2 §2.B's four items built, the fourth deliberately not.
  **Transport client links** — `AddGrpcClient<T>` / typed `AddHttpClient<T>` / `AddRefitClient<T>` now
  mint a `TransportClientDetection` carrying the type argument AND the configured address (the DC3
  root cause: the generic `Add*` branch read args[0], the config lambda, and threw the type argument
  away). New `ServiceAddressBook` resolves an address host to a project: AppHost resource table first
  (`http://basket-api` → `Projects.Basket_API` → Basket.API), then normalized project-name match
  (`todoapi` ≡ `Todo.Api`), ambiguity → nothing (Batch A's rule). Injection site fixed too: file-local
  `using` aliases expanded, which is what hid eShop's `GrpcBasketClient`. **Aspire topology** —
  resources were detected and thrown away, and `WithReference`'s direction was read BACKWARDS (args[0]
  as source), so every relationship pointed at "?"; now project→project ServiceLinks + Store nodes
  with DependsOn edges. **CLI** — `[Command("verb")]` attribute detection (E7 stands: the verb string
  is the evidence, never the interface name), verb-first entry titles. **Channel<T> NOT built** —
  R2 conditions it on "if cheap"; naming a channel's producer/consumer is BodyFacts trace-seam work.
  Results: eShop transport FAIL→PASS (grpc 1, http 3, aspire 10, **bus held at exactly 9**), TodoApi
  SKIP→PASS (http 1, aspire 0 — the tag-precedence proof), canary + all must-not-worsen numbers exact,
  both single-project false-positive guards clean. **GitVersion's declared entries≥5 cell did NOT
  flip** — the detection is right (the new `new-cli` pole reads all 5 verbs) but the whole-repo
  analysis drops every new-cli type: DC6, so it becomes a Batch C acceptance cell. Surprises: (1) a
  self-inflicted perf regression caught by the matrix's big poles — the alias lookup walked every
  node of every file in any gRPC-package repo (RazorPages 62→119s); made lazy, restored to 63.5s, and
  the same pre-existing waste in `CliCommandExtractor` fixed with it; (2) eShop's AppHost declares a
  deliberate Identity↔apps reference CYCLE (its own comment says so) — 10 aspire links is truth, not
  a direction bug; (3) two PLAN-listed widen repos were duplicates already in the matrix
  (VerticalSlice ≡ CleanArchitecture, Functions ≡ AzureFunctions) — swapped for wolverine +
  company-functions so 23 rows mean 23 repos. CLOSE VERDICTS: full battery **GATE: PASS unqualified**
  (gates-s3-close3.txt, exit 0, all 8 steps — needed a detached run because the agent harness kills
  background commands at 10 min and the battery runs longer; no step ever failed, and the
  `--format html --strict` exit 2 inside Step 4 is pre-existing, byte-identical in the S2 log) ·
  bench PERF-2026-07-28-0141 DntSite Map
  26.3s / OrchardCore 15.0s — 24% and 51% under the PERF-2026-07-18-1346 baseline, within noise of
  Batch A's numbers (GraphAssembly 266ms). Full grid + acceptance diff + scope decisions:
  `eval-results/2026-07-28/graph-truth/MATRIX.md`. Next: S4 Batch C (R2 §2.C) — declare acceptance at
  open, widen matrix to ~32.

- 2026-07-28 · S4 CLOSED (Batch C): all four R2 §2.C items, matrix at **32** poles, every declared
  cell flipped. **Scope (item 2)**: one `SolutionCatalog` now enumerates + scores solutions, so the
  two pickers that could disagree (`ProjectRootResolver` took the first file in enumeration order —
  GitVersion's Cake build tree — while `SolutionDiscoveryExtractor` scored) make one pick; a
  `SolutionScopeNote` rides kernel JSON + Map + proto ("analyzed src/GitVersion.slnx — 1 of 3
  solutions in this repo"); `--sln` picks another from the repo ROOT (GitVersion's 5 verbs, the
  inherited S3 cell, with no detection change); the SymbolTable is solution-scoped so a repo's second
  `GitVersion.Core` stops making its own names ambiguous. 14 sln-scope FAILs → PASS. **Style (item
  4)**: Library/CliTool ⇒ `NotApplicable`, applied BEFORE the map is built (the map snapshots the
  style into its header — suppressing after would have left surfaces disagreeing). 9 library poles
  green. **Classification (item 3)**: `*.Testing`/`*.TestSupport`/`*.Fixtures`/`TestGrains` projects
  and TestHelper/Testing folders are test support; call edges now obey the same production rule as
  nodes. **Targets (item 1)**: receiver-CHAIN hop in ONE place used by both producers, DI-conflict
  lands on the interface instead of dropping the edge, self-targets suppressed. Surprises: (1) the
  CleanArchitecture style FAIL was THREE stacked defects, and the deepest was DC6 at the style surface
  — the detector read all 22 projects across the repo's FOUR solutions and found three AppHosts, i.e.
  three single-app solutions counted as one constellation; scoping the style verdict fixed it. (2)
  `DesktopEntryDetection` was the ONE entry surface not implementing `IEntrySurfaceDetection`, whose
  single consumer is the call-graph seed — so desktop/MAUI had no call spine at all, and that (not the
  hop) is what finally flipped eShop's entry-target cell. (3) OrchardCore's ModularMonolith verdict
  was riding on five scaffolding NAMES; solution scoping dropped them, and the ~150 real modules carry
  no "Module" name segment — they live under `src/OrchardCore.Modules/`, so the rule now reads the
  directory. (4) THREE instrument defects found while declaring acceptance: the DI-interface regex ran
  case-INSENSITIVE (every type starting with I counted — most of eShop's RED), the fixture deny pattern
  matched substrings across camel humps ("Crea-teSt-ream" flagged MediatR's production `CreateStream`),
  and hub-sanity had no minimum-degree floor. (5) Orleans' entry-target went RED with its DI-interface
  count UNCHANGED at 12 — the denominator shed 25 self-targets and 16 test-grain entries; not relaxed,
  carried to S5. CLOSE VERDICTS: full battery **GATE: PASS unqualified** (gates-s4-close2.txt, exit 0,
  all 8 steps; the first run failed Step 3 on three eval expectations that encoded the fixed defects —
  including `verticalslice`, which is ardalis/CleanArchitecture under a directory-derived name — each
  corrected with evidence) · bench PERF-2026-07-28-0429: DntSite Map 26.0s, OrchardCore Map 13.5s —
  25% and 56% under the PERF-2026-07-18-1346 baseline, and OrchardCore 10% FASTER than Batch B ·
  deny-list probe answered: **KEEP BOTH** (without them 21 entry targets degrade to LINQ/EF verbs;
  the DI-ratio metric cannot see it). Full grid + acceptance diff + probe:
  `eval-results/2026-07-28/graph-truth-s4/MATRIX.md`. Next: S5 Batch D + Batch E (R2 §2.D/§2.E) —
  declare acceptance at open, widen matrix to the full 47.

- 2026-07-28 · S5 CLOSED (Batch D + Batch E), matrix at the full **47**. **Batch D** (hygiene/perf) was
  the one batch whose acceptance was "nothing moves", and it was checked mechanically rather than by eye
  (`eval-results/2026-07-28/compare-verdicts.py`): **0 moved cells** over 32 carried poles and 12
  pre-D-baselined widen poles. Landed: SDK evidence parsed once onto `ProjectInfo` (`Sdks` +
  `UsesWpf/UsesWinForms`; the scalar `Sdk` DELETED so two SDK fields can never disagree) which removed
  four `File.ReadAllText(csproj).Contains(marker)` probes and made `ArchetypeDetector` pure; ONE
  `Detection` polymorphism scheme (the hand-maintained `[JsonDerivedType]` list is gone, wire
  discriminator unified on `type` so JSON output is byte-identical); the dead `IPruner` strand +
  `OnPrunerCompleted` + `ScorerStat` + `src/DevContext.Roslyn/`; and the perf riders — draft graphs for
  the two intermediate assembly views (only the final graph freezes), lazy in-edge adjacency, indexes
  for four O(items x model) scans, a cached `IsProductionEntrySource`, and the gateway config scan moved
  out of the GraphAssembly clock with its cross-OS separator bug fixed (`file.Contains("\\bin\\")`
  matched NOTHING off Windows, so bin/obj/.git were read and JSON-parsed). Snapshot schema v7.
  **Batch E**: `TracePolicy` is now the single seam order + framework stop + dial set + budget rule,
  read by the flow spine, the trace tree, and every caller — the two tables it replaced disagreed about
  `ServiceLink` (third on the spine, catch-all in the tree), so the map's flow could cross a service
  boundary the trace silently dropped; gRPC `GetTrace` built the trace twice under two different budgets
  and now builds once; RESULT/NEXT (invented HTTP statuses, eShop lifecycle vocabulary) retired and the
  tokens spent on NAMING omitted branches; three number pairs reconciled to one counting function each,
  tested (the `OverallConfidence` 0.7/0.3 blend is deleted, proto field reserved). All four inherited
  cells were root-caused from the raw dumps BEFORE any code moved — Orleans' bare-interface target
  (the called member was known at the call site and dropped; now on the edge), eShop's primary-call
  ordering (first-wins was edge insertion order, not evidence), CleanArchitecture's 5 unhandled (the
  handler interface lives in a NuGet package, so the transitive walk stops at the boundary; a
  shape-based fallback needs TWO structural facts, no name list), and wolverine's `Envelope`
  (ACCEPTED LIMITATION with all six origins named — in the repo that implements a bus, `SendAsync(env)`
  is the wire, not dispatch). **Surprises:** (1) the SDK probes read the REAL filesystem while fixtures
  live in a `FakeFileSystem`, so every fixture test was blind to SDK evidence — six goldens encoded "no
  runnable web service" for a fixture whose csproj says `Sdk="Microsoft.NET.Sdk.Web"`; (2) the dup-name
  check's premise died in Batch A — its flagged pairs are provably distinct ids (bitwarden's 99 are its
  Dapper/EntityFramework repository pairs, one SignalR pair is a NESTED type), so it measured how many
  homonyms a repo HAS; re-pointed at cross-SERVICE links, which is what DC2n actually complained about;
  (3) a harness change fired ONCE without a smoke test and cost ~35 minutes — `Start-Process -PassThru`
  needs `$proc.Handle` touched or `.ExitCode` throws, and under `ErrorActionPreference=Stop` that killed
  the whole matrix silently after one pole; (4) `HotChocolate` (graphql-platform) produced no dump in 28
  minutes and TIMED OUT at the 600s budget — **a defect class DC1-DC10 does not name, so R1 does NOT
  exit at S5**; (5) two widen poles surfaced one shared archetype defect — `CLI`
  (dotnet/command-line-api) reads CliTool and `MahApps.Metro` reads Desktop, because an auxiliary
  executable outranks the packable library it demos. CLOSE VERDICTS: see
  `eval-results/2026-07-28/graph-truth-s5/MATRIX.md` (Batch D close + root causes) and
  `graph-truth-s5-close/MATRIX.md` (post-Batch-E grid), with the battery/bench logs beside them.
  CLOSE VERDICTS: battery **Steps 0–4b PASS** (fresh, non-cached eval suite) with Step 5 failing on a
  1-millisecond timestamp flake in an app spec — fixed, app suite re-run green (15 files / 92 tests);
  bench **PERF-2026-07-28-1238** DntSite Map 26.6s · OrchardCore Map 13.6s (**−23% / −56%** vs the
  PERF-2026-07-18-1346 baseline, +2.3% / +0.7% vs Batch C — inside noise on the stricter bar), DntSite
  Trace 27.9→22.1s; post-Batch-E matrix moved **9 cells, all accounted for** (CleanArchitecture +
  OrchardCore handler-join from the Batch E fix, 6 dup-name + 1 hub-sanity from the two declared
  instrument changes), and `Orleans` entry-target flipped **12 bare interfaces → 0 of 37** on the
  re-measure after two follow-up defects were found and fixed. Grids:
  `eval-results/2026-07-28/graph-truth-s5/MATRIX.md` (post-D + root causes) and
  `graph-truth-s5-close/MATRIX.md` (post-E + close verdict).
  Next: S6 = R3 decision session (owner-interactive) — and the two R1 items S5 leaves open: the scale
  wall, and the archetype-vs-auxiliary-executable defect (both pinned RED, neither re-declared).

- 2026-07-28 · S6 OPENED (R3 decision session): **D-A decided and landed; D-B…D-H still open.**
  First move was prep the owner never had to wait on — a re-drive of the app on the post-Batch-E
  engine (`src/DevContext.App/scripts/r3-current-state.mts` → `eval-results/2026-07-28/r3-current-state/`),
  because every screenshot in the 07-27 audit predates every R2 fix and would have made the session
  decide against stale evidence. That re-drive is the session's most reusable output: eShop's Atlas
  now reads **23 transport links** (was 5, bus-only — E1) with queue/HTTP/gRPC drawn distinctly, and
  `POST /api/orders/` reaches `IdentifiedCommandHandler` **verified** (was In:1/Out:0 — E2). The
  engine chain that blocked R3 is closed; what was left on the workspace really is IA.
  **Owner decision: A1+A2 hybrid** — the centre pane's altitude FOLLOWS FOCUS (no focus → the
  topology canvas, focus → the trace tree, canvas still a toggle of the focused state). Implemented
  in `stage.ts`'s lens→altitude effect; it was a ~10-line change because `system` altitude already
  WAS the topology canvas — the W1 void existed only because the default `flow` lens was sent to the
  `flow` altitude unconditionally, with nothing to draw.
  **Owner delegated sub-decision A-2** (the CrossService wall) with criteria rather than an option —
  "enjoyable, informative, quick" — so it was decided on those and recorded as delegated:
  collapse each run of sibling cross-service hops to ONE expandable row naming the services
  (`groupServiceHops`, 7 new unit tests). eShop's order trace went from **33 CrossService rows to 1**
  ("crosses 8 services · 15 hops · 37 omitted"; the 37 reconciles exactly against the old per-row
  counts), and — the real win — the trace now CONTINUES into the handler's actual idempotency logic
  (`IRequestManager` → `RequestManager` → `ExistAsync` → `OrderingContext.FindAsync`), which the mesh
  had been burying.
  Surprises: (1) **a decision had to be corrected mid-implementation on honesty grounds** — the first
  draft of A-2 promised the collapsed row would name transport kinds and crossing events, but
  `TraceNode` (proto:323) carries no transport kind (it lives on the ServiceLink EDGE), so rendering
  it would have meant inventing it; DECISIONS.md now records the scope correction plus the engine
  follow-up it implies. Check the proto before promising a label. (2) `GraphCanvas` hard-codes its
  host height (500px / 280 compact) — invisible while it was embedded in scrolling pages, glaring the
  moment D-A made it the landing surface; added an explicit `fill` input rather than changing the
  default under the other three call sites. (3) Running `ng build` against the workspace while
  `start-dev-bg` is up leaves a stale `vite-error-overlay` that silently eats Playwright clicks —
  cost two failed captures; restart the dev stack before any capture that follows a build.
  (4) A verification script that navigates straight to `/explore` gets an unanalysed app — bootstrap
  the session through Home first, the way the capture driver does.
  Verdicts: app suite **15 files / 99 tests green** (was 92 — +7), `ng lint` clean, `ng build` clean,
  disclosure toggle verified in the real app (27→35→27 nodes, `r3-verify-hopgroup.mts`).

## 3. Session map

### S1 — Instrument + matrix v1 + Batch A prep (R1 doc)
1. ~~Open: ask owner for the signed Prism merge; set base per §1; create `feat/graph-v2`.~~
   DONE 2026-07-27: merge signed, develop @ 8dbb510, `feat/graph-v2` created.
2. ~~First commit: the program assets currently untracked on the d5 working tree.~~ DONE 2026-07-27.
3. Build `eval/graph-truth.ps1` implementing the 7 checks in R1 §2.1 (transport counts vs
   expectation, handler-join reachability, hub sanity, entry-target sanity, style vs expectation,
   sln scope, dup-name cross-service proxy). Per-repo expectations live in `eval/expectations/`
   (extend the existing scheme). Output: machine-readable per-repo verdicts + a human MATRIX.md grid.
4. Run matrix v1 over the 12 poles: eShop, dotnet-podcasts, CleanArchitecture, aspire-samples,
   GitVersion, Spectre.Console, FluentValidation, Polly, gRPC, MassTransit-Sample, functions-app,
   OrchardCore. (Names = `eval-repos/` dir names, verified.) Fan out per-repo runs to subagents;
   only verdicts return to context (§5).
5. Answer the two R1 probes: (a) eShop gRPC client registration — which detection fires (DC3);
   (b) GitVersion command framework — why F10 sees 1 command (DC8). Record answers in MATRIX.md;
   they shape Batch B items 1+3.
6. Batch A prep: re-verify the [audit] file:line refs for Batch A files ONLY (R2 §2.A list);
   declare Batch A acceptance in MATRIX.md — which cells must flip, which must not move
   (CleanArchitecture is the healthy-baseline canary: its cells must NOT move in any batch).
- Done when: STATUS S1 line all true. If S1 runs long, Batch A prep (step 6) may slip to S2 open.

### S2 — Batch A: identity + resolution (R2 §2.A — the deep cut)
- Steps 1–5 as written in R2 §2.A (structural NodeId converging on SymbolId; call edges through
  SymbolTable, Ambiguous→skip; one Roslyn compilation; delete compensating deny-lists after proving
  redundancy; collapse NameResolver into SymbolTable).
- Keep: entry-seeded closure scoping (D3 perf win) · determinism seals (`DeterministicOrderTests`
  green — HANDOVER-PRISM §4).
- Close: full battery detached + matrix v1 rerun + bench vs PERF-2026-07-18-1346 (DntSite 34.5s /
  OrchardCore 30.8s, no >10% regression). Acceptance = the cells declared in S1.

### S3 — Batch B: transports + joins (R2 §2.B)
- Items 1–4 as written, informed by the S1 probe answers. External targets render as dashed
  external nodes (drop both-ends-in-solution gate).
- Close: battery + matrix widened to 22 (add: SignalR, signalr-app, Blazor, RazorPages,
  VerticalSlice, TodoApi, Functions, AzureFunctions, YARP, Ocelot). Declare acceptance at open.

### S4 — Batch C: entry quality + classification + scope (R2 §2.C)
- Items 1–4 as written (primary-call pick, multi-sln explicit scope + `--sln` flag, ProjectClassifier
  fixtures, style verdicts suppressed at detector).
- **Inherited acceptance cells from S3 (already declared, do not re-litigate):**
  - `GitVersion` entry-target must reach ≥5 entries **with no engine change** — item 2's explicit sln
    scope is the whole fix; the `[Command]` detection already works (see the `gitversion-new-cli` pole).
  - `CleanArchitecture` handler-join residue (5 unhandled: the template's own Create/Delete/Update/
    Get/List Contributor set) — root-cause once the 4-sln swallow stops contaminating handler detection.
  - `wolverine` handler-join (`Envelope` read as a dispatched request — non-MediatR dispatcher) and the
    SignalR/YARP/wolverine dup-name residues need per-edge verification before any expectation relaxes.
  - `_dataAccessNoiseMethods` + `IsObjectNoiseMethod` deny-lists: Batch A kept them pending item 1's
    primary-call work — re-test redundancy here.
- Instrument note carried from S3: hub-sanity has no signal on small graphs (a degree-1 node lands in
  the "top 5" of a 67-node graph) — a minimum-degree floor is the fix.
- Close: battery + matrix widened to ~32 (wolverine is already in; add: Hangfire, Quartz.NET, Orleans,
  MediatR, Serilog, AutoMapper, Newtonsoft.Json, refit, RestSharp + one replacement for wolverine).
  Verify candidate repos are not duplicates of existing poles before declaring them. Declare
  acceptance at open.

### S5 — Batch D then Batch E (R2 §2.D + §2.E)
- Batch D (mechanical hygiene/perf): land, launch full battery DETACHED, immediately start Batch E
  (one trace contract, number reconciliation, retire eShop string tables).
- **Inherited from S4 (declared, do not re-litigate):**
  - `Orleans` entry-target is RED and the expectation was NOT relaxed: its 12 Dashboard minimal-API
    endpoints resolve to the bare `IDashboardClient` interface. The absolute count did not move in
    Batch C (12 → 12); the ratio crossed the line only because the denominator shed 25 tautological
    self-targets and 16 test-grain entries. This is the type-level seam limitation — a minimal-API
    lambda's call lands on the receiver TYPE, not the member — and it belongs to Batch E's one-trace
    contract, together with the primary-call ORDERING residue (eShop's `CheckoutViewModel.CheckoutAsync`
    resolves to `DialogService.ShowAlertAsync`, a real call but the weaker of two collaborators).
  - `wolverine` handler-join (`Envelope` read as a dispatched request) and the `SignalR` (12) /
    `wolverine` (4) dup-name residues were **not root-caused in S4** — Batch C's four items plus the
    scope work filled the session. Nothing was relaxed: every one of those numbers is still pinned
    EXACT in the matrix, and the per-edge verification requirement still stands before any change.
  - Batch D's perf sweep should re-check what Batch C added per-invocation: the receiver-chain hop
    (`SymbolTable.HopThroughProperty`) and the call-edge production gate both run in the hot loop.
- Close: E battery + matrix = full 47 (remainder incl. desktop pole PowerToys/ScreenToGif/MahApps/
  CommunityToolkit.Mvvm/Desktop, StackExchange.Redis, Dapper, xUnit, CLI, blazor-samples,
  razorpages-app, company-functions, bitwarden-server, DntSite, HotChocolate, MassTransit, MediatR…).
  R1 exit: no new DC class in the last 10 repos; every DC has fix-or-accepted-limitation noted.

### Open R1 items S5 leaves on the table (read before declaring R1 done)

- **Scale wall (new class, not in DC1–DC10).** `HotChocolate` (ChilliCream/graphql-platform) produced
  no dump in 28 minutes and TIMED OUT at the harness's new 600s per-pole budget. Every other pole
  finishes — including PowerToys (83s) and bitwarden-server (214s) — so this is one repo an order of
  magnitude past the rest, not a general regression. R1's exit criterion ("no new DC class in the last
  10 repos") is therefore NOT met; it needs a profile of that repo, not a bigger timeout.
- **Archetype loses to an auxiliary executable.** `CLI` (dotnet/command-line-api) reads CliTool and
  `MahApps.Metro` reads Desktop. Both are LIBRARIES whose exe is a demo/sample that isn't under a
  `samples/` path, so it decides the archetype. One root cause, two poles, both pinned RED with the
  truth declared — do not re-declare the expectations to match the engine.
- **`wolverine` `Envelope`** — accepted limitation, evidence in the S5 MATRIX. Still pinned at 1.
- **`gRPC` transport** — unchanged since S3: its examples are outside the analysed solution.

### S6 — R3 decision session (owner-interactive; after S3 at earliest)
- Run per R3 §1: mock-ups per decision area (D-A first), owner decides, record DECISIONS.md,
  implement only complete pages. Render kernel built AFTER decisions, serving app/CLI/MCP as
  projections. Re-point `screenshot-gate.mts` as pages land.
- ~~D-A: workspace default + IA~~ **DECIDED + LANDED 2026-07-28** — A1+A2 hybrid (altitude follows
  focus) + sub-decision A-2 (cross-service collapse, owner-delegated on criteria). See DECISIONS.md.
- **Before ANY further R3 work, re-drive the app first.** The 07-27 `ui-feature-audit/` PNGs predate
  every R2 batch and are now wrong about the product; `scripts/r3-current-state.mts <repo> <name>
  [focus]` regenerates honest current-state frames into `eval-results/<date>/r3-current-state/`.
  Restart `start-dev-bg.ps1` before capturing if anything ran `ng build` since it started.

### S7 — R3 continued (owner-interactive)
- **D-B first** (canvas semantic language) — D-A promoted it: the canvas is now what Explore opens on,
  so its grammar is the product's first impression. Open items already visible in
  `eval-results/2026-07-28/r3-current-state/eshop-after2/10-explore-default.png`: `eShop.AppHost` /
  `HybridApp` / `ClientApp` render as floating peers (AppHost should be an orchestrator FRAME per
  R3 §2 D-B), no kind glyphs, no store cylinders drawn, and `apphost` repeats as an edge label ~9
  times where a grouping frame would say it once.
- Then D-C/D-D (library + CliTool archetypes — both changed materially in Batch C), then D-E…D-H.
  **Their current-state evidence was NOT gathered in S6** — the FluentValidation capture was launched
  from a drifted working directory and died on module resolution, and re-running it against a live
  eval battery risked the CPU-contention flake S2 already paid for. Capture FluentValidation (library)
  and GitVersion (CliTool) at the top of S7.
- D-A sub-decisions NOT yet implemented (decided, still owed): A-1 dock the node card / kill the
  modal · A-3 budget-elastic + labelled depth (still depth-1 + an unexplained 7% meter) · A-4
  middle-ellipsis entry truncation · A-5 promote the Trail to a real section.
- Engine follow-up A-2 implies: project the crossing edge's transport kind (and event name where
  known) onto `TraceNode` so the collapsed row can name seams instead of only counting them.

### Parallel lane — R4 (separate session/worktree, any time)
- Fixes 1–7 + 11–12 per R4 §1; primitives 8–10 wait for/coordinate with Batch E.
- Dogfood protocol per R4 §2 AFTER Batch B is in. Output: `eval-results/<date>/mcp-dogfood/REPORT.md`.

## 4. Batch discipline (from R2 §1 — the contract every session follows)

- Inside a batch: `dotnet build src/DevContext.Cli -clp:ErrorsOnly` + `--filter`ed unit tests only.
  NO full gate mid-batch.
- Batch close: full `eval/gates.ps1` once (DETACHED, overlap next work) + matrix run once.
  Acceptance cells declared BEFORE coding starts.
- Session-killers, pre-empted: `start-dev-bg.ps1 -Kill` FIRST always · rebuild Cli after Core edits ·
  ASCII only in detached PS 5.1 scripts · capture CLI output to file, never `Select-Object -First`.
- Snapshot cache: every batch invalidates all snapshots (MVID keying) — cold re-analyzes during
  verification are EXPECTED, not regressions.

## 5. Token economy (why this plan is shaped this way — follow strictly)

1. **Cold-start reading list per session**: this PLAN (§2 STATUS first) + the ONE strand doc for the
   session + HANDOVER-PRISM §4 if touching resolver/determinism code. Do NOT re-read FINDINGS.md,
   the audits, or other strand docs wholesale — R1's DC list is the condensed form of all of them.
2. **[audit] refs are re-verified lazily**: only for files the current batch touches, immediately
   before editing. Never sweep-verify the whole inventory.
3. **Matrix runs happen in subagents / detached scripts**: per-repo analyze+query output goes to
   files under `eval-results/<date>/graph-truth/raw/`; only the verdict grid enters context.
   Read MATRIX.md, never the raw dumps.
4. **One build/test cycle per batch** (§4). The matrix answers "did it regress" wholesale — do not
   run per-fix verification loops.
5. **Declare acceptance up front, in writing** (MATRIX.md). This converts verification from
   open-ended exploration into a checklist diff.
6. **Update §2 STATUS + session log at close** — the next session cold-starts from it instead of
   re-deriving state from git archaeology.

## 6. Standing constraints (inherited — see research/README.md)

- NEVER merge to develop unasked; the Prism merge and the graph-v2 merge are owner-signed events.
- Determinism seals stay green through all surgery (SealableBag/OrderedTypes/insertion-order/
  call-site edge canon).
- PRODUCT-DIRECTION.md §3 five-artifact contract binds R3; a sixth artifact needs owner override.
