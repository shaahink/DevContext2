# DevContext graph-v2 — autonomous remainder Phase Tracker

**Plan:** DevContext graph-v2 — autonomous remainder | **Branch:** `feat/graph-v2` | **Design doc:** docs/dev/research/PLAN.md

## Handoff (overwrite this block, ≤12 lines, no history)

**STAGE G10 COMPLETE — G10.1 @ 06fcae5 + 55293b5**, evidence `eval-results/2026-08-02/G10/G10.1-EVIDENCE.md`.
Ten candidates found by two regex sweeps + **`git blame` against batchA `4e1292d` as the discriminator** — every
one blames to 07-02..07-18. Re-measured with `g10-threshold-sweep.ps1`: 11 poles, one COLD analysis each.
**The tool note that makes this cheap: `DEVCONTEXT_CACHE_ROOT` redirects the snapshot cache** — point it at a
temp dir and the first `query` per pole runs cold against YOUR build, the second is a hit. 11 poles, 2 ops, 4 min.
**Four of five stale thresholds no longer mean what their comment says.** CORRECTED: (1) START HERE filtered
`nodeCount >= 4` **before** D-E's band rule, so it could delete the whole request-shaped band — red-then-green
proof reproduces E-2 **verbatim** (`expected 'CheckoutViewModel.CheckoutAsync' to be 'POST /api/orders/draft'`)
without any checkout title; `flow-ranking.ts` shipped with no spec, it has one now. (2) home-page read
`unwired/entries > 0.2` as a rate at n=1 — GitVersion and MediatR each shipped a WARNING saying "1 of 1".
JUSTIFIED + bug, measurement now stated at each site: **`graph.orphans` has NEVER fired** (Semantic share
0.010–0.259 on every app; floor 0.5 unreachable — left there on purpose, see ledger CALL 1); **L3.4's sparse
broadening never fires** on 11/11 incl. its own trigger repos, so identity-strip's hub-scope line has never
rendered (cause is between line 199 and `k<5` — Dapper's Calls edges span 32 types, so k=16; **start there**);
**deep-spine ratio is saturated** (1.000 on 5/11). Also: the engine ships **two definitions of a verified edge**
(GraphStats approx = Syntactic only, so Join counts; GraphOrphansSource counts Semantic only; Join is the enum
DEFAULT). Gates: Cli 0w/0e · loom-guards PASSED incl. Truth · app 159/159 · `ng build` EXIT 0. 4 bugs filed.

## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 22 |
| Done | 0 |
| Claimed (unconfirmed) | 21 |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · DONE ✓ (confirmed) · BLOCKED · SKIPPED. Evidence = artifact path produced by a run this
phase (a code path is not evidence). Agent claims are marked DONE; engine confirms as DONE ✓.

### G1 — R4 MCP correctness + honesty fixes (R4 §1 items 1-7)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G1.1 | `map` returns the structured Map surface (library surface, packages, aggregates, service styles); its markdown stops advertising CLI flags that don't exist over MCP | DONE | cf1a822 | fast-engine:OK · guards:OK |
| G1.2 | `get_context` accepts type/symbol roots — a library gets a pack instead of nothing | DONE | 79743b0 | fast-engine:FAIL · guards:OK |
| G1.3 | Seam glyphs match the proto (singular/plural), handle-less calls stop retargeting across repos, RpcException stops leaking past the error envelope on all five tools | DONE | 75704f2 | fast-engine:OK · guards:OK |
| G1.4 | `find(kind:)` filters server-side so total/hasMore are true; `analyze` returns an honest long-run note + a `cached` flag | DONE | a09c456 | fast-engine:OK · guards:OK |

### G2 — R4 menu hygiene + one trace default (items 11-12)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G2.1 | Tool menu folded (`flow`→`trace(compact)`, `insights`→`stats`, `interesting_points`→`overview`) and the did-you-mean handler reads the real tool list instead of a second hand-maintained one | DONE | 35eea1e | fast-engine:OK · guards:OK |
| G2.2 | One trace budget default across MCP / CLI / server, read from `TracePolicy` (Batch E's single source) | DONE | 35eea1e | fast-engine:OK · guards:OK |

### G3 — R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G3.1 | `seam(from,to)` path-between primitive exists at proto + GraphQuery + tool | DONE | baa5ffd | fast-engine:OK · guards:OK |
| G3.2 | Kind-filtered `neighbors` ("who WRITES this table", "who SENDS this command") exposed | DONE | d82d074 | fast-engine:OK · guards:OK |
| G3.3 | Snapshot-cache truth (`from_cache` / `analyzed_at` / `git_head`) on AnalysisSummary + SessionInfo | DONE | cf0fa62 | fast-engine:OK · guards:OK |

### G4 — R4 dogfood drive — is the MCP a proper tool?

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G4.1 | Dogfood Task 1 — 10 real architecture questions on an unseen repo, MCP tools only, every call logged and graded HELPED / NEUTRAL / HURT | DONE | 254fd36 | fast-engine:OK · guards:OK |
| G4.2 | Dogfood Tasks 2+3 — a real change made through MCP orientation, and DevContext used on itself | DONE | 546fb32 | fast-engine:OK · guards:OK |
| G4.3 | `eval-results/<date>/mcp-dogfood/REPORT.md` — call log, grades, ranked "what it lacks", judged against R4 §3's success bar | DONE | 546fb32 | fast-engine:OK · guards:OK |

### G5 — D-3 — a CLI verb reaches its handler

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G5.1 | Root cause named, per verb with evidence: why GitVersion's five `ICommand<TSettings>` verbs join no handler | DONE | d21e72b | fast-engine:FAIL · guards:OK |
| G5.2 | The join lands — a CLI verb reaches its handler on the gitversion pole, with the CleanArchitecture canary unmoved | DONE | 11ebe20 | fast-engine:OK · guards:OK |

### G6 — D-4 — one vocabulary for "service" on Atlas

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G6.1 | One vocabulary for "service" on Atlas — the canvas, the per-service breakdown and Hub radar stop disagreeing about what a service is | DONE | 2fdd4cf | eval-results/2026-07-29/G6/G6.3-EVIDENCE.md |
| G6.2 | Raw metadata arity never reaches the UI (no `` Logging.ILogger`1 `` in a rendered surface) | DONE | 051386c | fast-app:OK · guards:OK |

### G7 — C-2 / C-3 — a library's empty sections fill or withhold with a reason

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G7.1 | C-2 — Atlas's five empty sections on a library either fill or withhold themselves with a stated reason | DONE | 55af763 | fast-app:OK · guards:OK |
| G7.2 | C-3 — the withhold-don't-suppress rule applied consistently wherever a surface has no entries | DONE | 55af763 | fast-app:OK · guards:OK |

### G8 — R1 scale wall — profile HotChocolate, do not raise the timeout

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G8.1 | HotChocolate profiled: the phase that does not terminate inside the 600s budget is NAMED, with per-phase timings as evidence | DONE | 59b17aa | fast-engine:OK · guards:OK |
| G8.2 | Fixed, or recorded as an accepted limitation with the defect class named — R1's exit criterion answered either way. **Not by raising the timeout.** | DONE | 59b17aa | fast-engine:OK · guards:OK |

### G9 — R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G9.1 | An auxiliary/demo executable stops deciding a packable library's archetype: `CLI` and `MahApps.Metro` read Library, canary poles unmoved | DONE | ea0dc3f | eval-results/2026-07-29/G9/G9.1-EVIDENCE.md |

### G10 — Sweep for thresholds calibrated on pre-Batch-A data

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G10.1 | Sweep for thresholds calibrated on pre-Batch-A (starved-graph) data; each one re-measured on current data and corrected or justified in a comment that states the measurement | TODO | - | - |

## Dependencies

```
(none — stages run sequentially by plan order)
```
