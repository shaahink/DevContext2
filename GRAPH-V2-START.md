# DevContext graph-v2 — autonomous remainder Phase Tracker

**Plan:** DevContext graph-v2 — autonomous remainder | **Branch:** `feat/graph-v2` | **Design doc:** docs/dev/research/PLAN.md

## Handoff (overwrite this block, ≤12 lines, no history)

**G3.1 CLAIMED** — `seam(from,to)` at `GraphQuery` + proto (`GetSeam`, RPC 26) + tool (menu 21→**22**).
Evidence `eval-results/2026-07-29/G3.1-EVIDENCE.md`. Test counts **Core 683 / Server 85** (was
674/76, **+9 each**) — a passing test is not named in the log, so the delta is your only proof.
Verify with `eval-results/2026-07-29/g3.1-verify.ps1`: the battery's OWN Step 2 filter
(`Category!=Eval&Category!=CliSmoke&Category!=McpQa`) then `Category=McpQa` **alone** — never the
bare `Category!=Eval`. Next: **G3.2** — and it is smaller than it looks: `GraphQuery.Neighbors`
ALREADY takes an `EdgeKind?`; only `NeighborsRequest` and the tool don't expose it. The real work is
the honesty half — what a kind matching nothing says, and whether the roll-up drops it. Then G3.3.
**THE ROLL-UP IS WHAT A NEW TRAVERSAL GETS WRONG, and it is invisible until you test for it**: after
Batch A a Type node carries almost no edges of its own, so anything written against
`_graph.OutEdges` reports two types that collaborate every request as UNCONNECTED. Watched red
3-of-9 — and **6 of the 9 passed on that broken state**, so a suite of the wrong 6 ships it.
Traps re-paid: the MCP leaves a `DevContext.Server` alive that locks Core/Cli/Contracts.dll — kill it
BY PID before the next build. `pnpm gen:proto` after any proto edit. My driver's ground truth was
wrong once more (seam-vs-impact hop equality is a wrong premise on a Type target) — measure, 4-for-4.
Still open from G2.2 §6: fold `TraceResponse.applied_budget_tokens` in on the next proto edit.


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 22 |
| Done | 0 |
| Claimed (unconfirmed) | 6 |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · DONE ✓ (confirmed) · BLOCKED. Evidence = artifact path produced by a run this
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
| G3.1 | `seam(from,to)` path-between primitive exists at proto + GraphQuery + tool | TODO | - | - |
| G3.2 | Kind-filtered `neighbors` ("who WRITES this table", "who SENDS this command") exposed | TODO | - | - |
| G3.3 | Snapshot-cache truth (`from_cache` / `analyzed_at` / `git_head`) on AnalysisSummary + SessionInfo | TODO | - | - |

### G4 — R4 dogfood drive — is the MCP a proper tool?

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G4.1 | Dogfood Task 1 — 10 real architecture questions on an unseen repo, MCP tools only, every call logged and graded HELPED / NEUTRAL / HURT | TODO | - | - |
| G4.2 | Dogfood Tasks 2+3 — a real change made through MCP orientation, and DevContext used on itself | TODO | - | - |
| G4.3 | `eval-results/<date>/mcp-dogfood/REPORT.md` — call log, grades, ranked "what it lacks", judged against R4 §3's success bar | TODO | - | - |

### G5 — D-3 — a CLI verb reaches its handler

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G5.1 | Root cause named, per verb with evidence: why GitVersion's five `ICommand<TSettings>` verbs join no handler | TODO | - | - |
| G5.2 | The join lands — a CLI verb reaches its handler on the gitversion pole, with the CleanArchitecture canary unmoved | TODO | - | - |

### G6 — D-4 — one vocabulary for "service" on Atlas

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G6.1 | One vocabulary for "service" on Atlas — the canvas, the per-service breakdown and Hub radar stop disagreeing about what a service is | TODO | - | - |
| G6.2 | Raw metadata arity never reaches the UI (no `` Logging.ILogger`1 `` in a rendered surface) | TODO | - | - |

### G7 — C-2 / C-3 — a library's empty sections fill or withhold with a reason

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G7.1 | C-2 — Atlas's five empty sections on a library either fill or withhold themselves with a stated reason | TODO | - | - |
| G7.2 | C-3 — the withhold-don't-suppress rule applied consistently wherever a surface has no entries | TODO | - | - |

### G8 — R1 scale wall — profile HotChocolate, do not raise the timeout

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G8.1 | HotChocolate profiled: the phase that does not terminate inside the 600s budget is NAMED, with per-phase timings as evidence | TODO | - | - |
| G8.2 | Fixed, or recorded as an accepted limitation with the defect class named — R1's exit criterion answered either way. **Not by raising the timeout.** | TODO | - | - |

### G9 — R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G9.1 | An auxiliary/demo executable stops deciding a packable library's archetype: `CLI` and `MahApps.Metro` read Library, canary poles unmoved | TODO | - | - |

### G10 — Sweep for thresholds calibrated on pre-Batch-A data

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G10.1 | Sweep for thresholds calibrated on pre-Batch-A (starved-graph) data; each one re-measured on current data and corrected or justified in a comment that states the measurement | TODO | - | - |

## Dependencies

```
(none — stages run sequentially by plan order)
```
