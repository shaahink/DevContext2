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
- [~] S7 — R3 continued 2026-07-28. **D-B DECIDED + LANDED** (owner: B2 the language · B1's frame
      where an orchestrator is declared · B3's lanes for the picture surfaces). Transports are now the
      only labelled edge layer, parallel links collapse with a count, a declared AppHost draws a
      containment frame instead of nine identical labels, stores are drawn for the first time, and
      the kind glyph is alive after being dead-by-construction on every repo. Three facet fields +
      one membership edge added engine-side. **A-4 landed; A-3/A-5 were already built; A-1 deferred
      with a dependency (Inspector needs a Neighbours section first).** Verdicts in the session log.
- [~] S8 — R3 continued 2026-07-28. **D-B's lane tail LANDED · D-C and D-D DECIDED + LANDED.**
      Lane views inherit the kind glyph and gain a `declared resources` lane (17ad1d1). Owner picked
      **C2** (a library's front doors open onto the real call path) and **D2 → D1** (say which
      solution was analyzed and let the reader switch, then make the verb list the CLI's centre) —
      d1ddda4 · 81292d8 · 44be0db. Two more dead-by-construction fields revived
      (`MapResponse.scope_note`, `ArchetypeView`) and one silent refusal fixed (the session manager
      answered a different-solution request from cache). **D-E…D-H remain open**, plus the
      sub-decisions listed in DECISIONS.md. Verdicts in the session log below.
- [~] S9 — R3 continued 2026-07-28. **The dead-field sweep is DONE and is now a gate**
      (`eval/contract-sweep.ps1` + `eval/expectations/contract-sweep-allow.txt`): four more dead
      fields found, all four removed from the contract (`entry_table`, `layer_band`,
      `args_digest`, `total_files`), and two real losses revived — swallowed extraction failures
      in the app's run report, and L3.4's sparse-graph caveat in BOTH the ledger and the CLI's
      `--stats`. Fixing the second uncovered a suppression: the whole Confidence Ledger was hidden
      on any entry-less repo, i.e. every library. **D-E…D-H and the C-1/C-2/C-3 + D-2/D-3/D-4
      sub-decisions remain OPEN** — they are the next session's work.
- [~] S10 — R3 continued 2026-07-28. **D-E DECIDED + LANDED (E3 — the front page asks the
      archetype's question), absorbing C-1 and D-2.** A library draws no canvas and leads with its
      front doors; a CliTool leads with the command surface; a service keeps the canvas it earns.
      E1's rule rides inside it (every fact stated once — eShop printed three of five headline
      numbers twice), **E-1** moved `% verified` into the Confidence Ledger, and **E-2** put Home,
      Atlas and START HERE on ONE ranking rule (`core/flow-ranking.ts`). Brief:
      <https://claude.ai/code/artifact/973a85a7-5ac5-43e4-8446-05762d0bbe82>.
      **Four defects fixed first, none needing a decision** — see the session log. **D-F/D-G/D-H
      remain open**, with their evidence captured (owner: implement Home first).
- [ ] S11 — R3 continued: D-F · D-G · D-H · C-2/C-3 + D-3/D-4 · render kernel built AFTER the
      decisions it serves · re-point `screenshot-gate.mts` as pages land
- [~] R4 (parallel lane) — now driven by conductor as stage **G1** (`GRAPH-V2-START.md`).
      **§1 item 1 LANDED 2026-07-29**: `map` returns the structured surface (it was dropping the
      library surface, packages, aggregates, service styles and the archetype view *after* the server
      had computed them), and no shared-render markdown names a CLI-only flag. The leak was wider
      than the [audit] ref — it lives in the **Core renderers all three surfaces share**, so it is
      found by sweeping Core, not by reading the MCP tool: `LibrarySurfaceRenderer.cs:122,125` were
      missed on the first pass. `MarkdownRenderer`'s `--around` is correctly left alone (CLI-only
      registration). Evidence: `eval-results/2026-07-29/G1.1-EVIDENCE.md`.
      **§1 item 2 LANDED 2026-07-29** — and its [audit] premise was partly STALE: a TYPE focus already
      resolved (`AbstractValidator` filled 43%), so "libraries get nothing" was wrong. The two real
      defects, both measured: a bare MEMBER name resolved to nothing (`focus:"RuleFor"` → envelope,
      while `resolve("RuleFor")` on the same handle listed the member), and every section was built
      from a trace, which walks OUT-edges — so `IValidator` (9 in-edges, 0 out) got a structurally
      empty pack. Now: a member tier that runs LAST (additive by construction), a `usage` section on
      symbol-rooted packs only (entry packs unmoved — that is the canary), and an identity line naming
      the resolved symbol. The evidence run then found a **second** defect nothing had read:
      `EntryPointResolver` ranked candidates on a type's OWN out-edges while `GraphQuery.ResolveNodeId`
      has ranked on ROLLED degree since C3, so on a library — where front doors are interfaces with no
      out-edges — every candidate tied at 0 and enumeration order won. One rolled ranking rule now.
      Evidence: `eval-results/2026-07-29/G1.2-EVIDENCE.md`.
      **§1 items 3+4+5 LANDED 2026-07-29** (75704f2). Item 3 was understated: Core's `SeamKind` has
      never produced the plurals the MCP matched, AND `Data`/`Resolve`/`Pipeline` had no arm at all,
      so **seven of ten seam kinds rendered the same mute dot** — on eShop's `POST /api/orders/` the
      two mute rows were the MediatR dispatch and its handler. Compact traces now carry a `legend`,
      and Core's own `TraceRenderer` gained the `CrossService` label it never had. Item 4's root
      cause is two server lines (`Get()` bumps `LastAccess` on every access, `ListSessions()` orders
      by it), so `Sessions[0]` meant "whatever repo someone else touched last"; the MCP now prefers
      the session it analyzed, and names the open ones rather than guessing. Item 5's [audit] ref
      named 5 tools; a sweep of all 24 found **8**, so the fix is a test that walks every
      `[McpServerTool]` method against a failing client. Evidence:
      `eval-results/2026-07-29/G1.3-EVIDENCE.md`.
      **§1 items 6+7 LANDED 2026-07-29** (a09c456) — **§1 items 1-7 are now complete.** Item 6 was
      understated by the [audit] ref: the kind filter running below the truncation made BOTH totals
      artefacts of the page size, not just the kind-filtered one. On eShop, `find("Order",
      limit:100)` reported `total=120` — which is `limit+20`, the MCP's own fetch window — when the
      answer is **354**, and `kind:"Type"` reported 22 when the answer is **174**. The kind now
      rides on `SearchRequest` and `GraphQuery.FindPage` applies it above the limit, returning the
      uncapped count on `SearchResponse.total_matches`; `Find()` is that method's page, so
      resolve/usages/impact are provably unmoved. Item 7: `analyze` read the event stream for one
      field and dropped the rest — same eleven words for an eight-minute analysis and a 12ms session
      reuse. It now returns the `AnalysisSummary` the server had already computed plus
      `AnalyzeResult.cached`, whose truth had to be built: three paths return an analysis without
      analysing and only ONE said so (to a progress event nobody read) — the runner's two
      snapshot-cache branches returned an `EngineResult` indistinguishable from a full run.
      Two riders, both from re-verifying rather than reading the refs: **`AnalysisSummary.archetype`
      was assigned nowhere** (every analyze the server has ever answered carried `""`, while
      `ToMapResponse` filled the same fact from the same source), and the MCP dropped the
      `AnalyzeEvent.Error` arm, so a failed analysis lost the server's own reason. Evidence:
      `eval-results/2026-07-29/G1.4-EVIDENCE.md`. Items 11-12 (G2) open; dogfood + REPORT.md
      ungraded.
      **§1 items 11-12 LANDED 2026-07-29 (G2)** — the menu is folded and the did-you-mean handler is
      seeded from the SDK's own tool collection (35eea1e), and one `TracePolicy` budget serves MCP /
      CLI / server (7d42c08). Evidence: `eval-results/2026-07-29/G2.{1,2}-EVIDENCE.md`.
      **§1 item 8 LANDED 2026-07-29 (G3.1)** — `seam(from, to)`, the path BETWEEN two symbols, at
      `GraphQuery` + proto (`GetSeam`, RPC 26) + tool (menu 21 → 22). It existed at no layer: every
      other graph query is single-source, so "how does A reach B" had no answer. Shortest paths only,
      with `totalPaths` counted EXACTLY over the predecessor DAG rather than by enumeration; the
      reverse direction is searched before answering "no"; a search the hop budget ended says so and
      names the retry. **The discriminating check was watched going RED**: run with direct edges
      instead of C3's rolled edges, 3 of 9 tests fail with `Expected: Forward / Actual: None` — a
      Type→Type seam over direct edges calls two types that collaborate on every request
      "unconnected", because after Batch A the wiring hangs off members, not Type nodes. **Six of the
      nine passed on that broken state.** On eShop the drive is cross-checked by two tools that
      predate it: the pair is chosen by `impact` (which already knows the distance) and every hop is
      confirmed against `neighbors`. Evidence: `eval-results/2026-07-29/G3.1-EVIDENCE.md`.
      Rider: my own driver was wrong once and the engine right — an exact seam-vs-impact hop equality
      is a wrong premise on a TYPE target, because seam's roll-up counts arrival at a member of the
      type while impact reports the distance to the bare Type node (3 vs 4, measured from the raw
      response). Fourth occurrence of that pattern in this program.
      **§1 items 9-10 LANDED 2026-07-29 (G3.2, G3.3)** — kind-filtered `neighbors` (d82d074) and
      snapshot-cache truth `from_cache`/`analyzed_at`/`git_head` on AnalysisSummary + SessionInfo
      (cf0fa62). **§1 IS NOW COMPLETE (items 1-12).**
      **§2 Task 1 RUN 2026-07-29 (G4.1) — the first honest answer to "is this a proper tool".**
      43 MCP calls on `eval-repos/Hangfire` (unseen, non-octet, archetype Library, 946 nodes / 615
      edges / **0 entries**), ten architecture questions written and committed BEFORE the drive
      (254fd36), MCP tools only: **no grep, and `read_source` never called**. 44,712 response tokens,
      12.9s of tool wall time, graded HELPED 28 / NEUTRAL 7 / HURT 8. Verdict against §3's bar:
      **8/10 answered correctly — 6/10 if "answered" has to mean a TOOL ASSERTED the fact rather than
      the agent inferring it from names** (Q4 and Q5 are the two that fall, both because the graph has
      no inheritance edge). On tokens the honest comparison is the other way round: the post-drive
      grep phase verified all ten answers in three shell calls and ~4k tokens. The defensible claim is
      not economy but that the MCP produced the right symbol names with file:line provenance from zero
      prior knowledge — grep needs the name first. Evidence:
      `eval-results/2026-07-29/mcp-dogfood/G4.1-EVIDENCE.md` (+ `CALL-GRADES.md`, `call-log.jsonl`,
      43 raw responses). Driver: `eval/mcp-qa/dogfood.js`.
      **Four defects filed, three of them silent-wrong-answer class.** (5) All 22 tools ship
      `description: ""` — 31 written `///` summaries never reach the wire; MEASURED both ways:
      `GenerateDocumentationFile` does NOT carry them (byte-identical response), a
      `[System.ComponentModel.Description]` attribute does. (6) `trace` handed a nodeId returns
      `found:true` with an EMPTY tree titled "Type: Type" — its focus resolver matches the prefix
      token before the first colon and ignores the rest, proved by `Type:ZzzNoSuchSymbolAnywhere`
      returning the identical answer; `get_context` resolves the same string correctly, and trace's
      own envelope tells the agent to pass a nodeId. (7) An explicit interface METHOD is registered
      as a Type node with an empty filePath, and 26 BCL `System.Type` references bind to it —
      4.2% of the repo's edges, and it ranks 5th in `stats`' wiring hubs. (8) Calls inside a LAMBDA
      ARGUMENT produce no edge, so `CoreBackgroundJobFactory.cs:89`'s
      `ctx.Context.Connection.CreateExpiredJob(...)` — the actual persistence write of the whole
      enqueue path — is invisible, and the trace of that type looks *complete* without it.
      **Structural gaps, not bugs:** `SeamKind` has no inheritance kind at all, so "who implements
      this" cannot be asked on a library (biggest gap for the archetype S10 made first-class);
      `seam` reports two genuinely-connected symbols as "unconnected" because a library's call graph
      fragments at every interface (19 `Resolves` for 51 interfaces); `map` is 17,105 tokens with one
      parameter; and dead ends still do not name a working next step outside G3.2's kind-filter path —
      measured at its sharpest when `usages`' envelope recommended a retry that returned STRICTLY LESS
      than the reply suggesting it.
      **A driver check is vacuous until you have watched it go red** — G1.4's `find-kind` case
      PASSED on the broken before-state (`total >= page length` = 22 >= 5), the same way G1.3's
      glyphs case did. The check that discriminates is the INVARIANT: a true total does not move
      when the page size moves. Third occurrence in this program.
      **Verification-command trap, worth more than any of the above**: `dotnet test --filter
      "Category!=Eval"` is NOT `eval/gates.ps1` Step 2 (`:136` also excludes `CliSmoke` and
      `McpQa`, and runs the MCP QA drive alone as Step 2b). Verifying with it drags a 3-minute
      external node drive into a 674-test parallel run, where the server the MCP spawns exits
      before binding — `FATAL: Timeout: initialize`, which reads as an engine collapse and is not
      one. That is the `McpQaGateTests` red three G1 sessions have chased.
      **§2 Tasks 2+3 RUN 2026-07-29 (G4.2) and §2 IS NOW COMPLETE; REPORT.md written (G4.3), so
      stage G4 is closed.** Task 2 made a real change in Hangfire oriented by MCP only — six facts
      declared and committed BEFORE the drive (`task2/CHANGE-SPEC.md` @546fb32), **6/6 came back
      TOOL-asserted, 0 inferred**, and `LogJobDurationAttribute` compiles. That 6/6 against Task 1's
      8/10-or-6/10 is the strand's most useful single observation: **the MCP is markedly better at
      "what does this look like" than at "what happens next"** — Task 1 asked for behaviour, which
      lives in edges; Task 2 asked for declarations, which come back verbatim in
      `signatures`/`bodies`. On tokens grep won 6:1 (14,424 vs ~2,362) and it is recorded as a FAIL,
      but the circularity is now nameable rather than asserted: the winning grep was
      `rg ": JobFilterAttribute"`, **a query that presupposes fact F3, one of the six being sought**.
      Reusable trick, no engine change needed: an interface's implementors are found by asking about
      the type its METHOD TAKES AS A PARAMETER — `IServerFilter` has inDegree 0 AND outDegree 0, but
      `get_context(focus:"PerformingContext")`'s usage section named three implementors with
      `file:line`. Task 3 **corrected two inherited claims**. R4 §2's "server ignores devcontext.json
      → different file sets" is true in mechanism (`DevContextConfig` is a CLI-project type; the
      Server references it nowhere) but moves **no nodes and no edges**: three runs give CLI-with-config
      1254/1383, CLI-without-config 1254/1383 (file/project inventory 385→442, 8→29), MCP 1260/1398 —
      the default patterns already exclude `eval-repos`, the config only adds `fixtures`/`goldens`, and
      solution scoping already excluded those projects. **So the real +6/+15 CLI↔MCP divergence has a
      different cause, still open.** Rider: `DevContextConfig.DefaultPath` reads the config from the
      WORKING DIRECTORY, not the analysed repo root. And the maintenance question — bug #8's own
      "what to measure first" — **refuted bug #8's stated cause** (filed as bug #11): in
      `BodyFactsExtractor.ExtractAsync`, calls at :51/:54/:62 inside a lambda (one nested two deep)
      DO bind, while :56/:74/:80 do not — and :74 is in no lambda at all. What the three missing
      edges share is a STATIC call with a TYPE-NAME receiver; `neighbors(…, in)` is **0 for
      `BodyFactExtractor`, `RazorCodeVirtualizer` and `ExtractorHelpers` alike**, all live nodes with
      out-edges. `stats` reads Calls verified 280 / approx 1103 — 80% approximate on our own repo, and
      the engine's own body walker is called by nobody according to its own graph. My first hypothesis
      (the callee types are missing nodes) was wrong and `find` killed it — fifth occurrence of
      agent-premise-wrong / engine-right in this program.
      Three more defects filed, all the same family: **#9** `get_context`'s fillNote asserts "the pack
      already contains everything reachable" while eliding the body you asked for (one focus, two
      budgets, same sentence; `fill %` is tokens/budget so it FELL 42%→18% as content rose); **#10**
      `read_source` silently accepts an invalid `mode` (`DevContextTools.cs:1756` has an unguarded
      else) and returned 20 of 147 lines with `found:true`; **#11** above. **The strand's signature
      defect is now named: not emptiness and not an error, but a reply shaped exactly like a complete
      answer, with nothing on the wire saying it is partial** — and `contract-sweep.ps1` cannot catch
      it, because every field involved IS read. Evidence:
      `eval-results/2026-07-29/mcp-dogfood/{REPORT.md, G4.2-EVIDENCE.md, CALL-GRADES-G4.2.md,
      task3/DEVCONTEXT-JSON-SCOPE.md}`. Totals across the protocol: **81 calls · 72,664 tokens ·
      HELPED 55 / NEUTRAL 13 / HURT 13.** Verdict: not yet a proper tool, and the gap is not the one
      R4 assumed — the two blockers are whole missing edge classes (inheritance, static calls) and
      replies that cannot distinguish "no" from "I did not look".

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
- 2026-07-28 · S7 (R3 continued): **D-B decided and landed.** Owner picked **B2 as the base language
  (transports are the edge layer), B1's containment frame where an orchestrator is declared, and
  B3's lane grammar for the surfaces where the canvas is a picture** — one grammar, one stated
  condition, one placement. Landed: parallel links collapse per pair per transport with a count
  (23 links → a readable handful); deployment references keep their edge but lose their label and
  their weight (the word `apphost` appeared ~9 times and was the loudest thing on the canvas);
  `eShop.AppHost` draws a dashed frame around the 9 projects and 6 stores it orchestrates, which
  also took it out of the "in no relationship" tray it had been stranded in; declared infrastructure
  is drawn for the first time; the legend became an always-visible strip listing only what the
  canvas actually drew; services in no relationship at all are named in a tray instead of floating.
  Engine: three facet fields (`ServiceCard.Stores`, `ServiceCard.Orchestrates`,
  `TransportLink.Resolution`) + one membership edge (AppHost → project, tagged `orchestrates` —
  `WithReference` runs project→project, so no edge had ever expressed the one thing an AppHost is
  for). **The kind glyph was dead by construction on every repo since D4.2**: `ClassifyService` read
  a `Layer` that `AddServiceNodes` never sets, so every service classified "Service" and every glyph
  rendered empty; it now derives from the entry surfaces a service owns, and eShop reads
  `[UI] WebApp` / `[RPC] Basket.API` / `[JOB] PaymentProcessor` correctly on first run.
  Surprises: (1) **two scope corrections, both recorded rather than quietly shrunk** — B-3 blamed the
  fit CLAMP for the empty pane, but `fitAndCenter` centres correctly and the real cause is layout
  ASPECT (a layered graph flows RIGHT and fits to width in a portrait pane; the topology now lays
  out DOWN); and the lane views cannot honestly inherit transport-coloured edges, because lanes live
  at the all-projects level whose edges are csproj references while transports are a service-level
  fact. (2) **Three of the four "owed" D-A sub-decisions were not what S6 recorded** — A-3's depth
  select was already labelled and already defaults to 3, A-5's Trail has been a first-class inspector
  section since `f81a31f`, and A-4's middle-ellipsis existed but never fired because its 48-character
  threshold sat above the ~34 the column shows, so CSS truncation always cut first. Check the code
  before building to a note about the code. (3) **A-1 is bigger than "kill the modal"** — the
  Inspector renders everything the node card does except its neighbour lists (Call Stack is the path
  from the ENTRY, not the node's Called-by/Calls), so deleting the modal first would delete the only
  home those lists have. (4) `barrel` was the wrong way to say "store": at a node's width-to-height
  ratio it is indistinguishable from a round-rectangle, and the product already says kind in text
  everywhere else — `[db]` reads instantly where the shape did not. (5) The S6 vite-overlay trap
  fired again, from `pnpm test` this time rather than `ng build`; any command that rebuilds needs the
  dev stack restarted before the next capture.

- 2026-07-28 · S8 (R3 continued): **D-B's lane tail landed; D-C and D-D decided and landed.**
  Order was deliberate — code first, then ONE capture pass, because every capture needs the dev
  stack restarted after a build (the S6/S7 trap) and the D-C/D-D briefs had to be made against the
  product as it actually stands.
  **Lane tail** (17ad1d1): the all-projects view inherits the two facts that survive the altitude
  change — a project that IS a service wears its kind glyph, and declared resources get a lane of
  their own (named "declared resources", not "infrastructure", because the DDD lane beside it can
  carry that name and one of them means C# projects). eShop reads `[UI] HybridApp` / `[RPC]
  Basket.API` / `[JOB] PaymentProcessor` across four DDD lanes with its six stores in a footer band.
  Transport-coloured edges stayed out, per the S7 correction.
  **D-D = D2 → D1** (d1ddda4, 81292d8). D2: `MapResponse.scope_note` reached the app's generated
  types and **no component read it**, and the app had no `--sln`, so GitVersion showed one of three
  solutions as the whole repo in silence. The proto now carries the scope FACTS (not the CLI's
  sentence, which ends by naming a flag no GUI has) and the identity strip renders a picker.
  **The bug this uncovered is the session manager**: idempotent by repo+HEAD, it answered the first
  switch in 2.3ms with the analysis already in hand — the sln reached the wire and the server
  declined to notice. Idempotence had become a refusal; the session key now carries the solution,
  which is the rule the snapshot cache learned in Batch C. Verified end to end: 1 entry → 5 entries.
  D1: `ArchetypeView` has projected a COMMAND SURFACE since L7.2 and nothing read that either, so a
  CliTool landed on a canvas that can only draw disconnected boxes; it is now the landing state,
  narrowly (Flow lens, no focus — the topology lenses still draw the topology).
  **D-C = C2** (44be0db): the Library surface was the best archetype page in the product and a dead
  end — list items, not buttons, so Inspector/Trail/pin/export were unreachable from it. Front doors
  now open onto the REAL call path (`derive AbstractValidator` → Include → RuleFor → RuleForEach →
  RuleSet).
  **Surprises:** (1) **`consumerPaths` is a template sentence per entry kind**, not a traversal —
  shipping it as "the consumer path" would have dressed a label as evidence; checked before building
  (the S7 rule), and the trace is what opens instead. (2) **Three dead-by-construction fields in one
  strand now** — the kind glyph (S7), `scope_note` and `ArchetypeView` (S8). The pattern is always
  the same: the engine computes it, a renderer somewhere consumes it, and the app's copy of the
  contract is never read. Worth a sweep. (3) **My own brief was wrong about D-3** — `0/5 wired` is
  NOT a meaningless metric for a CLI the way `0 entries` is for a library: a verb should reach its
  handler, and GitVersion's five reach none (they are `ICommand<TSettings>` classes whose execute
  member never joined). Corrected in DECISIONS.md rather than quietly implemented; it is real engine
  work for a later batch. (4) A backtick inside an HTML comment terminated an Angular template
  literal — `ng lint` passed and the dev server died on it; lint is not a compile. (5) `.list-row`
  is a flex ROW, so a stacked doc line became a squeezed column the moment the path panel took half
  the width — invisible at full width, which is why the capture and not the build caught it.
  **Verdicts:** app suite **120 green** (was 104 at S7 close: +3 lane tail, +9 CLI landing rule, +4
  focus token), server suite **27 green** (+2), lint clean, build clean, three end-to-end drivers
  PASS (`r3-verify-scope.mts`, `r3-verify-cli-surface.mts`, `r3-verify-consumer-path.mts`).
  Full battery **GATE: PASS unqualified** (`gates-s8-close.txt`, exit 0, all 8 steps; eval re-ran
  fresh at 12m14s because the Core/Server edits invalidated the stamp, and Step 4's
  `--format html --strict` exit 2 is the pre-existing one, byte-identical since S2).
  Next: S9 = the dead-field sweep, D-E…D-H, plus the sub-decisions DECISIONS.md leaves open.

- 2026-07-28 · S9 (R3 continued): **the dead-field sweep ran, and it is now an instrument.**
  `eval/contract-sweep.ps1` parses the proto, reads every consumer (app TS/HTML, MCP, CLI) and fails
  on any response field no client reads unless `eval/expectations/contract-sweep-allow.txt` states
  why that is correct. It found **four more dead fields on top of the three this program had already
  found by accident** — and the accident rate is the point: the same defect had shown up in S7, S8
  and S8 again, one session apart each time.
  **Out of the contract** (all four were computed and shipped to nobody): `GraphFacetsResponse`'s
  `entry_table` and `layer_band` — two projections built on EVERY GetGraphFacets call, one of which
  walks every node in the graph, read by neither client (the app takes entries from GetEntryPoints;
  MCP only ever touched ServiceMap and FlowList); `ToolCallEvent.args_digest`, never assigned and
  never read; and `CorpusStat.total_files`, whose only producer passes a literal `0`
  (`SetCorpusFileCounts(0, csharpFiles, …)`), so every response ever sent said the corpus held zero
  files. Field numbers reserved, messages deleted, Core projections left in place as tested
  capability with nothing calling them.
  **Revived, because these were real losses:** (1) **swallowed extraction failures** — J1/J3's
  counters ride the stats payload the app already renders section by section, the CLI prints a table
  of them, and the app dropped exactly that one section, so a desktop reader was the only reader who
  could not tell a clean run from a lossy one (14 of the 47 matrix poles have failures; eShop 2,
  SignalR 14). (2) **the sparse-graph verdict** — L3.4 BROADENS call-edge binding when a repo is
  entry-poor, and nothing said so on either surface, so the confidence panel quoted an edge
  percentage without saying those edges were found under a looser rule. Now in the ledger AND in the
  CLI's `--stats` output.
  **The suppression the sparse fix uncovered:** the Confidence Ledger was gated on
  `!entries.IsDefaultOrEmpty`, so it vanished entirely on any repo with no entry points — every
  library. FluentValidation has 169 edges whose verified/approximate split is computed and was
  unreachable, because no ledger meant no "verified" chip and the chip is what opens the panel. The
  gate is now `graph is not null`; the two entry-dependent ROWS withhold themselves instead, which
  is C-3's rule. This is a scope correction made while implementing — recorded, per the A-2/B-3
  precedent, not quietly restated.
  **Surprises:** (1) **the backtick trap fired again, immediately** — a backtick inside an HTML
  comment terminated the Angular template literal, exactly as S8 recorded it. It is now two for two;
  write field names in comments without backticks. (2) **My driver's ground truth was wrong and the
  app was right**: L3.4's doc comment says sparse means "entries < 5 or ratio < 0.1", so I asserted
  FluentValidation (0 entries) must be sparse. It is not — the rule ALSO needs enough central types
  to broaden over, and most entry-poor libraries fail that second test. Measured the verdict across
  poles (`query stats`) and rewrote the expectations from measurement: eShop dense + 2 failures, CLI
  sparse over 9 hubs + clean, each the other's negative control. Checking what a field CONTAINS is
  now three-for-three as the most valuable habit in this program. (3) **`analyze` takes a POSITIONAL
  path; `query` takes `--path`** — `analyze --path <repo>` silently analyzed the working directory
  instead, and three different repos returning identical node counts was the only tell. (4) The
  ledger's own two rows still overlap: `verified` counts `Semantic` while `approx` counts
  `Syntactic OR confidence < 1`, so the CLI pole reads "27% of 173" and "99% of 173" — a reader is
  invited to add them. Left alone: what "approximate" means is a definition call, and Batch E's
  number-pair reconciliation is the precedent for making it deliberately.
  **Verdicts:** app suite 120 green, server suite 27 green, contract sweep GATE: PASS (12 fields
  allow-listed with reasons, 0 unexplained), both honesty poles PASS end-to-end
  (`s9-verify-honesty.mts eshop|cli`). Full battery **GATE: PASS unqualified** — run TWICE, and the
  second run is the citable one: the first (`gates-s9-close.txt`) was launched before the sweep was
  wired in, so its log has no Step 1a and a verdict that does not cover the change it is cited for
  is not a verdict. `gates-s9-close2.txt` runs **Step 1a: Contract sweep — PASS** after the build,
  then all remaining steps green (eval 27 passed / 1 skipped in 9m53s, stamp written; Step 4's
  `--format html --strict` exit 2 is the pre-existing one, unchanged since S2).
  Next: D-E…D-H and the open sub-decisions (C-1/C-2/C-3, D-2/D-3/D-4).

- 2026-07-28 · S10 CLOSED (R3 D-E): **owner picked E3 — the front page asks the archetype's
  question** — after a capture of all three archetype poles on the post-S9 engine. Brief:
  <https://claude.ai/code/artifact/973a85a7-5ac5-43e4-8446-05762d0bbe82>. Landed: a library draws
  no canvas and leads with front doors + namespaces; a CliTool leads with the command surface and
  loses the Services toggle; a service keeps the canvas it earns. **E1's rule rides inside it** —
  eShop printed three of five headline numbers twice forty pixels apart, the wiring fact three times
  in two notations, types+projects a third time in the freshness tile; each fact now has one owner.
  **E-1** moved `% verified` into the Confidence Ledger (the chip stays as the opener — S9's lesson
  that no chip means no panel). **E-2** put Home, Atlas and START HERE on one rule
  (`core/flow-ranking.ts`). Absorbs **C-1** whole and **D-2** (no canvas below three connected
  boxes).
  **Four defects were fixed BEFORE the brief, none needing a decision** (e0a4cac), and one of them
  is the biggest single find of the strand: **`Insight.Severity` reached the wire as `"Warning"`
  while the app keyed on `"warning"` and the MCP filtered on `"WARNING"`.** One field, three
  spellings, two broken clients, silent in both — the Insights page had NEVER rendered its "Act on
  this" group on any repo, security warnings drew the info-blue border, Home's triage list showed
  only the row the app synthesises itself (hiding 18/18 unvalidated write endpoints and 36/43
  anonymous endpoints behind a link), and `mcp stats.warnings` returned `[]` for every repo ever
  analysed. **This is the variant S9's contract sweep cannot catch**: the field IS read, with the
  wrong key. Wire spelling is now lowercase, pinned by `ProtoMapperSeverityTests` (one test runs the
  MCP's own predicate). Also fixed: the `NotApplicable` sentinel printing as GitVersion's style
  chip; A-4's middle-ellipsis never having reached the Studio picker (ELEVEN rows read
  `/api/catalog/i…`), now a shared helper that takes a bias because a route distinguishes itself at
  the TAIL and a type name at the HEAD; and a selected picker row carrying `bg-hover`, the same
  class hover sets.
  **The regression E-2 uncovered:** START HERE offered `Trace [RelayCommand]
  CheckoutViewModel.CheckoutAsync` on eShop — a MAUI mobile command as the way into a twelve-service
  backend. Its own comment records why the checkout special case was safe when written (every
  checkout-titled entry was then a 1-hop client command that could not clear the ≥4-node gate) and
  states the intent (`"Trace POST /api/orders/draft" is the story a first visit should open on`).
  **Batches A–E made the comment false** and the special case began beating the preference it
  existed to protect. Now reads `Trace POST /api/orders/`. **Look for other thresholds calibrated on
  pre-Batch-A data.**
  **Two stale premises died in the capture** before costing anything: rail badges have not replaced
  their icons since M7.4 (D-H), and a picker row click has always called `toggleEntry` (D-G).
  **Traps:** (1) **the backtick-in-an-HTML-comment trap fired a THIRD time**, and this time it was
  invisible — `pnpm ng build` piped through `grep -E` for error markers looked clean because esbuild
  emits ANSI colour codes before the `✘`, so a dead dev server was the only symptom. **Check `$?`.**
  (2) the drifted-working-directory trap (S7's) fired twice; every `node scripts/…` needs an
  absolute `cd` in the same command. (3) my own driver was wrong twice and the app right both times
  — "no warnings" is not "no Act-on-this group" (notable belongs there too), and comparing a picker
  row's label span alone reported GET/PUT/DELETE on one route as a collision because the method is a
  sibling span. Measure the thing the reader sees.
  **Verdicts:** app 120 · server 30 (3 new) · `s10-verify-home.mts` PASS on all three archetypes ·
  `s10-verify-triage.mts` PASS on both poles · `s9-verify-honesty.mts` still PASS after its chip
  rename (it opened the ledger by clicking a chip that read "verified"). Full battery
  **GATE: PASS** — `gates-s10-close.txt`, exit 0, every step green including **Step 1a contract
  sweep** and **Step 2b MCP QA**; Step 4's `--format html --strict` exit 2 is the pre-existing one,
  unchanged since S2. **Cited with a second log on purpose:** five computeds died with the tile
  footers that used to restate the strip's counts, and that deletion landed AFTER the full run's
  Step 2 — so `gates-s10-close-app.txt` (`-Scope app`, exit 0) covers the final tree's app state,
  and the engine was untouched by it. S9's rule, applied to my own change: a verdict that does not
  cover the change it is cited for is not a verdict.
  Next: S11 — D-F (insight dedup, engine-side so CLI+MCP benefit) · D-G (Studio default state + the
  picker-label design call) · D-H/D-4 (three "service" vocabularies on the Atlas page) · C-2/C-3/D-3.

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

- ~~**Scale wall (new class, not in DC1–DC10).**~~ **PROFILED + FIXED 2026-07-29 (G8.1 / G8.2)** —
  `eval-results/2026-07-29/G8/`. It was never "one repo an order of magnitude past the rest":
  `SyntaxStructureExtractor.ResolveTypeDeclaration` walked the whole file's syntax tree once per
  base-list entry, so cost is `baseEntries × nodes` **inside a single file**. HotChocolate carries one
  11.3 MB generated GraphQL client with 4,598 base lists → **1,216,998 ms of a 1,275 s analysis**.
  One per-file index later: **11,830 ms**, whole analysis **64.3 s**, `types`/`detections` identical,
  and 15 poles byte-identical by SHA-256 (`G8.2-DIFF-VERDICT.txt`). The 600s budget was NOT raised.
  The class to remember is **large AND base-list dense**, not "large": SignalR's 3.0 MB generated file
  has one base list and never engaged the quadratic.
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

### S7 — R3 continued (owner-interactive) — ~~D-B~~ **DECIDED + LANDED 2026-07-28**
- ~~D-B (canvas semantic language)~~ **DECIDED + LANDED.** Owner picked **B2 as the language, B1's
  frame as a conditional enrichment, B3's lane grammar for the picture surfaces**. Brief:
  <https://claude.ai/code/artifact/25a53935-e9a2-49da-9bd9-486381d7db25>. See DECISIONS.md for the
  entry, its five sub-decisions, and the two scope corrections made while implementing.
  Before/after: `eval-results/2026-07-28/r3-current-state/eshop-after2/10-explore-default.png` vs
  `.../eshop-db3-zoom/canvas.png` (close-up; `r3-canvas-zoom.mts` is the new driver — full-page
  frames are too small to judge a node shape or an edge weight, which is most of a canvas decision).
- **Left of D-B for S8:** the lane views' narrowed inheritance (kind glyph + store lane on Home's
  *What runs* and Atlas — NOT transport-coloured edges, see the correction in DECISIONS.md).
- Then D-C/D-D (library + CliTool archetypes — both changed materially in Batch C), then D-E…D-H.
  **Their current-state evidence was NOT gathered in S6 or S7** — the FluentValidation capture was
  launched from a drifted working directory and died on module resolution, and re-running it against
  a live eval battery risked the CPU-contention flake S2 already paid for. Capture FluentValidation
  (library) and GitVersion (CliTool) at the top of S8.
- D-A sub-decisions: **A-4 landed in S7** · **A-3 and A-5 were already built** (the S6 note was
  stale — see the status table in DECISIONS.md; only A-3's budget-elastic half is open) · **A-1
  remains, and needs an Inspector Neighbours section first** — killing the modal today would delete
  the only surface the node card's Called-by/Calls lists have.
- Engine follow-up A-2 implies: project the crossing edge's transport kind (and event name where
  known) onto `TraceNode` so the collapsed row can name seams instead of only counting them.

### S8 — R3 continued (owner-interactive) — ~~D-C~~ ~~D-D~~ **DECIDED + LANDED 2026-07-28**
- ~~D-B's lane-view tail~~ **LANDED** (kind glyph + `declared resources` lane; no transport edges,
  per the S7 correction).
- ~~D-C (library)~~ **DECIDED: C2** — front doors are the spine and each opens its real call path.
- ~~D-D (CliTool)~~ **DECIDED: D2 → D1** — the scope is said and switchable, then the verb list is
  the centre. Brief: <https://claude.ai/code/artifact/4b09714f-db18-4823-9d4b-931f058d2b6e>
- **Left for S9**, all recorded in DECISIONS.md: C-1 (Home's "What runs" on a library still asks the
  wrong question), C-2 (Atlas's five empty sections), C-3 (`0 entries` suppression), D-2 (the fit
  clamp on a two-node graph), D-3 (**engine work** — a CLI verb should reach its handler; GitVersion's
  five reach none), D-4 (two "service" vocabularies on one Atlas page).
- **Worth a sweep before D-E**: three fields in this strand were dead by construction (the kind
  glyph, `scope_note`, `ArchetypeView`) — the engine computes, the CLI renders, the app never reads.
  Check what else `MapResponse` carries that no component consumes.

### S10 — R3 continued (owner-interactive) — ~~D-E~~ **DECIDED + LANDED 2026-07-28**
- ~~D-E (Home)~~ **DECIDED: E3** — the body is chosen by archetype. Absorbs **C-1** (a library is no
  longer asked what runs) and **D-2** (below three connected boxes, no canvas at all — a clamp only
  frames emptiness better). Sub-decisions **E-1** (`% verified` → the ledger) and **E-2** (one
  ranking rule) decided with it; **E-3** answered by E3 itself. See DECISIONS.md.
- **Left for S11**: D-F (insight dedup — three overlapping auth findings on eShop, and the fix
  belongs at the engine so CLI+MCP get it too), D-G (the Studio's default state, and picker rows
  that are still not unique — a design call, see below), D-H (largely stale; the live find is D-4),
  plus C-2 (Atlas's empty sections on a library), C-3, D-3 (**engine work**).
- **The D-G finding S10 could not fix without a decision**: after the truncation fix, eShop's picker
  still shows five `OrderStatusChangedTo*EventHandler`s that agree on their last 18 characters, and
  three `GET /Account` actions the engine DOES disambiguate (`[Logout]`, `[AccessDenied]`) in data
  the row drops. No truncation setting separates them — the answers are showing the target member,
  widening the column, or two-line rows. `s10-verify-triage.mts` prints them as `D-G evidence` on
  every run and hard-fails only on ROUTE collisions, which are zero.
- **Standing trap, now 3-for-3**: a backtick inside an HTML comment terminates the Angular template
  literal. It fired again in `home-tiles.ts`. Worse, it was invisible: `pnpm ng build` piped through
  a `grep -E` for error markers looked CLEAN, because esbuild prints ANSI colour codes before the
  `✘`. **Check `$?`, never the filtered output.**

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
