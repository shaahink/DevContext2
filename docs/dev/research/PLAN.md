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
- [ ] S4 — Batch C landed · battery green · matrix widened to 32 repos
- [ ] S5 — Batch D landed · Batch E landed · battery green ×2 · matrix = full 47 · loop-until-dry
      confirmed (no new DC class in last 10 repos)
- [ ] S6 — R3 decision session held · DECISIONS.md written · implementation per decisions + render kernel
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
- Close: E battery + matrix = full 47 (remainder incl. desktop pole PowerToys/ScreenToGif/MahApps/
  CommunityToolkit.Mvvm/Desktop, StackExchange.Redis, Dapper, xUnit, CLI, blazor-samples,
  razorpages-app, company-functions, bitwarden-server, DntSite, HotChocolate, MassTransit, MediatR…).
  R1 exit: no new DC class in the last 10 repos; every DC has fix-or-accepted-limitation noted.

### S6 — R3 decision session (owner-interactive; after S3 at earliest)
- Run per R3 §1: mock-ups per decision area (D-A first), owner decides, record DECISIONS.md,
  implement only complete pages. Render kernel built AFTER decisions, serving app/CLI/MCP as
  projections. Re-point `screenshot-gate.mts` as pages land.

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
