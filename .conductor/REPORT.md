# Conductor — DevContext graph-v2 — autonomous remainder run report

_Updated 2026-07-29 02:27 UTC · branch `feat/graph-v2` · HEAD `ccd8a41`_

**Status:** Paused
**Stage:** G1 — R4 MCP correctness + honesty fixes (R4 §1 items 1-7) · attempts used 1 · working ▸ G1.3
**Checkpoints:** 2/22 done · **Sessions run:** 2 · **Cost:** $29.2151 (agent $29.1684 + gates $0.0467) · **Tokens:** 402,746 in / 159,252 out

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| G1 | R4 MCP correctness + honesty fixes (R4 §1 items 1-7) | █████░░░░░ 2/4 | **← active** |
| G2 | R4 menu hygiene + one trace default (items 11-12) | ░░░░░░░░░░ 0/2 | todo |
| G3 | R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) | ░░░░░░░░░░ 0/3 | todo |
| G4 | R4 dogfood drive — is the MCP a proper tool? | ░░░░░░░░░░ 0/3 | todo |
| G5 | D-3 — a CLI verb reaches its handler | ░░░░░░░░░░ 0/2 | todo |
| G6 | D-4 — one vocabulary for "service" on Atlas | ░░░░░░░░░░ 0/2 | todo |
| G7 | C-2 / C-3 — a library's empty sections fill or withhold with a reason | ░░░░░░░░░░ 0/2 | todo |
| G8 | R1 scale wall — profile HotChocolate, do not raise the timeout | ░░░░░░░░░░ 0/2 | todo |
| G9 | R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro) | ░░░░░░░░░░ 0/1 | todo |
| G10 | Sweep for thresholds calibrated on pre-Batch-A data | ░░░░░░░░░░ 0/1 | todo |

<details><summary>G1 — R4 MCP correctness + honesty fixes (R4 §1 items 1-7) (2/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G1.1 | `map` returns the structured Map surface (library surface, packages, aggregates, service styles); its markdown stops advertising CLI flags that don't exist over MCP | ✅ DONE | [`cf1a822`](https://github.com/shaahink/DevContext2/commit/cf1a822) |
| G1.2 | `get_context` accepts type/symbol roots — a library gets a pack instead of nothing | ✅ DONE | [`79743b0`](https://github.com/shaahink/DevContext2/commit/79743b0) |
| G1.3 | Seam glyphs match the proto (singular/plural), handle-less calls stop retargeting across repos, RpcException stops leaking past the error envelope on all five tools | ⬜ TODO | - |
| G1.4 | `find(kind:)` filters server-side so total/hasMore are true; `analyze` returns an honest long-run note + a `cached` flag | ⬜ TODO | - |

</details>

<details><summary>G2 — R4 menu hygiene + one trace default (items 11-12) (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G2.1 | Tool menu folded (`flow`→`trace(compact)`, `insights`→`stats`, `interesting_points`→`overview`) and the did-you-mean handler reads the real tool list instead of a second hand-maintained one | ⬜ TODO | - |
| G2.2 | One trace budget default across MCP / CLI / server, read from `TracePolicy` (Batch E's single source) | ⬜ TODO | - |

</details>

<details><summary>G3 — R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) (0/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G3.1 | `seam(from,to)` path-between primitive exists at proto + GraphQuery + tool | ⬜ TODO | - |
| G3.2 | Kind-filtered `neighbors` ("who WRITES this table", "who SENDS this command") exposed | ⬜ TODO | - |
| G3.3 | Snapshot-cache truth (`from_cache` / `analyzed_at` / `git_head`) on AnalysisSummary + SessionInfo | ⬜ TODO | - |

</details>

<details><summary>G4 — R4 dogfood drive — is the MCP a proper tool? (0/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G4.1 | Dogfood Task 1 — 10 real architecture questions on an unseen repo, MCP tools only, every call logged and graded HELPED / NEUTRAL / HURT | ⬜ TODO | - |
| G4.2 | Dogfood Tasks 2+3 — a real change made through MCP orientation, and DevContext used on itself | ⬜ TODO | - |
| G4.3 | `eval-results/<date>/mcp-dogfood/REPORT.md` — call log, grades, ranked "what it lacks", judged against R4 §3's success bar | ⬜ TODO | - |

</details>

<details><summary>G5 — D-3 — a CLI verb reaches its handler (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G5.1 | Root cause named, per verb with evidence: why GitVersion's five `ICommand<TSettings>` verbs join no handler | ⬜ TODO | - |
| G5.2 | The join lands — a CLI verb reaches its handler on the gitversion pole, with the CleanArchitecture canary unmoved | ⬜ TODO | - |

</details>

<details><summary>G6 — D-4 — one vocabulary for "service" on Atlas (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G6.1 | One vocabulary for "service" on Atlas — the canvas, the per-service breakdown and Hub radar stop disagreeing about what a service is | ⬜ TODO | - |
| G6.2 | Raw metadata arity never reaches the UI (no `` Logging.ILogger`1 `` in a rendered surface) | ⬜ TODO | - |

</details>

<details><summary>G7 — C-2 / C-3 — a library's empty sections fill or withhold with a reason (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G7.1 | C-2 — Atlas's five empty sections on a library either fill or withhold themselves with a stated reason | ⬜ TODO | - |
| G7.2 | C-3 — the withhold-don't-suppress rule applied consistently wherever a surface has no entries | ⬜ TODO | - |

</details>

<details><summary>G8 — R1 scale wall — profile HotChocolate, do not raise the timeout (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G8.1 | HotChocolate profiled: the phase that does not terminate inside the 600s budget is NAMED, with per-phase timings as evidence | ⬜ TODO | - |
| G8.2 | Fixed, or recorded as an accepted limitation with the defect class named — R1's exit criterion answered either way. **Not by raising the timeout.** | ⬜ TODO | - |

</details>

<details><summary>G9 — R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro) (0/1)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G9.1 | An auxiliary/demo executable stops deciding a packable library's archetype: `CLI` and `MahApps.Metro` read Library, canary poles unmoved | ⬜ TODO | - |

</details>

<details><summary>G10 — Sweep for thresholds calibrated on pre-Batch-A data (0/1)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G10.1 | Sweep for thresholds calibrated on pre-Batch-A (starved-graph) data; each one re-measured on current data and corrected or justified in a comment that states the measurement | ⬜ TODO | - |

</details>

## Sessions

| # | Stage | Kind | Att | Started (UTC) | Dur | Outcome | New DONE | Commits | Gates | Cost | Overhead | Tokens |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | G1 | Deliver | 1 | 07-29 00:28 | 0:56 | Advanced | G1.1 | 2 | fast-engine:OK · guards:OK | $12.6728 | $0.0305 | 172,654/66,379 |
| 2 | G1 | Deliver | 1 | 07-29 01:29 | 0:55 | GatesRed | G1.2 | 3 | fast-engine:FAIL · guards:OK | $16.4956 | $0.0163 | 230,092/92,873 |

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
07-29 01:28:11  ◆ run started · DevContext graph-v2 — autonomous remainder
07-29 01:28:12  ▸ stage G1 entered — R4 MCP correctness + honesty fixes (R4 §1 items 1-7)
07-29 01:28:12  • session #1 G1 Deliver started (attempt 1/6)
07-29 02:29:42  ▪ gate fast-engine pass [session]  (4m09s)
07-29 02:29:42  ▪ gate guards pass [session]  (55.6s)
07-29 02:29:48  • session #1 G1 → Advanced · done G1.1 · 2 commit(s)  (1h01m35s)
07-29 02:29:48  • session #2 G1 Deliver started (attempt 1/6)
07-29 03:27:42  ▪ gate fast-engine FAIL [session]  (59.4s)
07-29 03:27:42  ▪ gate guards pass [session]  (1m43s)
07-29 03:27:47  • session #2 G1 → GatesRed · done G1.2 · 3 commit(s)  (57m58s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 2 · retries 0 (0 %) · overall Warn
⚠ [context-saturation] session #2: 23,739,978 context tokens (≥ 20,000,000)
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/graph-v2
working tree: M GRAPH-V2-START.md, ?? eval-results/2026-07-19/, ?? eval-results/2026-07-27/batchA-tests.txt, ?? eval-results/2026-07-27/bench-s2-close.txt, ?? eval-results/2026-07-27/bench-s2-dntsite.txt, ?? eval-results/2026-07-27/gates-s1-close.err.txt, ?? eval-results/2026-07-27/gates-s1-close.txt, ?? eval-results/2026-07-27/gates-s2-close.err.txt (+81 more)
vs upstream: up to date
```

### Commits by session

- **s1 (G1 Deliver)** — 2 commit(s):
  - [`0f1a5fc`](https://github.com/shaahink/DevContext2/commit/0f1a5fc) conductor: orchestrator's own template + plan edits from this run
  - [`cf1a822`](https://github.com/shaahink/DevContext2/commit/cf1a822) G1.1: the structured map the MCP dropped, and the flags it told agents to run
- **s2 (G1 Deliver)** — 3 commit(s):
  - [`27de288`](https://github.com/shaahink/DevContext2/commit/27de288) conductor: G1.2 handoff + the orchestrator's own plan edits from this run
  - [`7c6eb5e`](https://github.com/shaahink/DevContext2/commit/7c6eb5e) G1.2: a library symbol gets a pack — member roots resolve, and the pack carries who uses it
  - [`79743b0`](https://github.com/shaahink/DevContext2/commit/79743b0) conductor: telegram gets the one field without which it is a silent no-op

## Last gate run

fast-engine:FAIL · guards:OK

<details><summary>fast-engine — exit 1</summary>

```
--- Step 0: Clear orphaned build-locking processes ---
  PASS  Cleared 0 orphaned process(es)

--- Step 1: Build solution ---
  PASS  Build succeeded

--- Step 1a: Contract sweep (dead proto fields) ---
  PASS  Contract sweep clean (every response field read or allow-listed with a reason)

--- Step 2: Fast unit tests ---
```
</details>

## Last session result

> SESSION-RESULT:** G1.2 landed and is claimed (`task_update(G1.2, done)`, commit `7c6eb5e`, pushed as `6c6eccb..27de288`), evidence at `eval-results/2026-07-29/G1.2-EVIDENCE.md`. The item-2 `[audit]` premise turned out **partly stale** and re-measuring it first was the whole session's leverage: a TYPE focus already resolved (`AbstractValidator` filled 43%), so "libraries get nothing" was false — the two real defects were that a bare **member** name resolved to nothing (`get_context(focus:"RuleFor")` returned an envelope while `resolve("RuleFor")` on the same handle listed that member), and that every pack section is built from a trace, which walks OUT-edges — so `IValidator`, with 9 in-edges …

## Tracker handoff

```
G1.2 CLAIMED @ 7c6eb5e — evidence `eval-results/2026-07-29/G1.2-EVIDENCE.md`. Next: **G1.3**.
RUN ITS MEASUREMENT FIRST: `node eval/mcp-qa/drive-r4.js glyphs|retarget|envelope <outDir>` — the
driver already has a case per remaining G1 checkpoint and each measures the before-state.
**Re-verify every [audit] ref before editing.** Item 1's was wider than written; item 2's was partly
STALE (type roots already resolved — "libraries get nothing" was false). Assume item 3-7's are too.
Make the evidence print the IDENTITY a tool resolved, not just PASS/FAIL — that is the only reason
G1.2 caught its second defect. And a negative assertion needs a positive precondition: G1.2's canary
first passed on a pack with ZERO sections.
Traps paid for: the MCP SPAWNS a DevContext.Server that outlives the driver and locks Core/Cli/
Contracts DLLs — kill it BY PID before any build (`start-dev-bg.ps1 -Kill` does not know it), and
pin `DEVCONTEXT_SERVER` to the fresh exe or an installed copy shadows your build.
RED, not mine and not new: `McpQaGateTests` fails 0/12 on the FIRST run after any Core edit
(open bug #1, MVID snapshot invalidation). Warm re-run = 12/12. Do not weaken it. Bug #2 also filed.
```
