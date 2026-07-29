# Conductor — DevContext graph-v2 — autonomous remainder run report

_Updated 2026-07-29 17:17 UTC · branch `feat/graph-v2` · HEAD `5940292`_

**Status:** Idle — advisor: human intervention required
**Stage:** G6 — D-4 — one vocabulary for "service" on Atlas · attempts used 0
**Checkpoints:** 16/22 done · **Sessions run:** 22 · **Cost:** $288.9126 (agent $288.6276 + gates $0.2850) · **Tokens:** 4,059,116 in / 1,557,965 out
**Confirmed phases:** G1, G2, G3, G4, G5, G6

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| G1 | R4 MCP correctness + honesty fixes (R4 §1 items 1-7) | ██████████ 4/4 | confirmed ✓ |
| G2 | R4 menu hygiene + one trace default (items 11-12) | ██████████ 2/2 | confirmed ✓ |
| G3 | R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) | ██████████ 3/3 | confirmed ✓ |
| G4 | R4 dogfood drive — is the MCP a proper tool? | ██████████ 3/3 | confirmed ✓ |
| G5 | D-3 — a CLI verb reaches its handler | ██████████ 2/2 | confirmed ✓ |
| G6 | D-4 — one vocabulary for "service" on Atlas | ██████████ 2/2 | confirmed ✓ |
| G7 | C-2 / C-3 — a library's empty sections fill or withhold with a reason | ░░░░░░░░░░ 0/2 | todo |
| G8 | R1 scale wall — profile HotChocolate, do not raise the timeout | ░░░░░░░░░░ 0/2 | todo |
| G9 | R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro) | ░░░░░░░░░░ 0/1 | todo |
| G10 | Sweep for thresholds calibrated on pre-Batch-A data | ░░░░░░░░░░ 0/1 | todo |

<details> ✅<summary>G1 — R4 MCP correctness + honesty fixes (R4 §1 items 1-7) (4/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G1.1 | `map` returns the structured Map surface (library surface, packages, aggregates, service styles); its markdown stops advertising CLI flags that don't exist over MCP | ✅ DONE | [`cf1a822`](https://github.com/shaahink/DevContext2/commit/cf1a822) |
| G1.2 | `get_context` accepts type/symbol roots — a library gets a pack instead of nothing | ✅ DONE | [`79743b0`](https://github.com/shaahink/DevContext2/commit/79743b0) |
| G1.3 | Seam glyphs match the proto (singular/plural), handle-less calls stop retargeting across repos, RpcException stops leaking past the error envelope on all five tools | ✅ DONE | [`75704f2`](https://github.com/shaahink/DevContext2/commit/75704f2) |
| G1.4 | `find(kind:)` filters server-side so total/hasMore are true; `analyze` returns an honest long-run note + a `cached` flag | ✅ DONE | [`a09c456`](https://github.com/shaahink/DevContext2/commit/a09c456) |

</details>

<details> ✅<summary>G2 — R4 menu hygiene + one trace default (items 11-12) (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G2.1 | Tool menu folded (`flow`→`trace(compact)`, `insights`→`stats`, `interesting_points`→`overview`) and the did-you-mean handler reads the real tool list instead of a second hand-maintained one | ✅ DONE | [`35eea1e`](https://github.com/shaahink/DevContext2/commit/35eea1e) |
| G2.2 | One trace budget default across MCP / CLI / server, read from `TracePolicy` (Batch E's single source) | ✅ DONE | [`35eea1e`](https://github.com/shaahink/DevContext2/commit/35eea1e) |

</details>

<details> ✅<summary>G3 — R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) (3/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G3.1 | `seam(from,to)` path-between primitive exists at proto + GraphQuery + tool | ✅ DONE | [`baa5ffd`](https://github.com/shaahink/DevContext2/commit/baa5ffd) |
| G3.2 | Kind-filtered `neighbors` ("who WRITES this table", "who SENDS this command") exposed | ✅ DONE | [`d82d074`](https://github.com/shaahink/DevContext2/commit/d82d074) |
| G3.3 | Snapshot-cache truth (`from_cache` / `analyzed_at` / `git_head`) on AnalysisSummary + SessionInfo | ✅ DONE | [`cf0fa62`](https://github.com/shaahink/DevContext2/commit/cf0fa62) |

</details>

<details> ✅<summary>G4 — R4 dogfood drive — is the MCP a proper tool? (3/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G4.1 | Dogfood Task 1 — 10 real architecture questions on an unseen repo, MCP tools only, every call logged and graded HELPED / NEUTRAL / HURT | ✅ DONE | [`254fd36`](https://github.com/shaahink/DevContext2/commit/254fd36) |
| G4.2 | Dogfood Tasks 2+3 — a real change made through MCP orientation, and DevContext used on itself | ✅ DONE | [`546fb32`](https://github.com/shaahink/DevContext2/commit/546fb32) |
| G4.3 | `eval-results/<date>/mcp-dogfood/REPORT.md` — call log, grades, ranked "what it lacks", judged against R4 §3's success bar | ✅ DONE | [`546fb32`](https://github.com/shaahink/DevContext2/commit/546fb32) |

</details>

<details> ✅<summary>G5 — D-3 — a CLI verb reaches its handler (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G5.1 | Root cause named, per verb with evidence: why GitVersion's five `ICommand<TSettings>` verbs join no handler | ✅ DONE | [`d21e72b`](https://github.com/shaahink/DevContext2/commit/d21e72b) |
| G5.2 | The join lands — a CLI verb reaches its handler on the gitversion pole, with the CleanArchitecture canary unmoved | ✅ DONE | [`11ebe20`](https://github.com/shaahink/DevContext2/commit/11ebe20) |

</details>

<details> ✅<summary>G6 — D-4 — one vocabulary for "service" on Atlas (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G6.1 | One vocabulary for "service" on Atlas — the canvas, the per-service breakdown and Hub radar stop disagreeing about what a service is | ✅ DONE | [`2fdd4cf`](https://github.com/shaahink/DevContext2/commit/2fdd4cf) |
| G6.2 | Raw metadata arity never reaches the UI (no `` Logging.ILogger`1 `` in a rendered surface) | ✅ DONE | [`051386c`](https://github.com/shaahink/DevContext2/commit/051386c) |

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
| 3 | G1 | Fix | 2 | 07-29 03:04 | 0:06 | Interrupted |  | 0 |  |  |  |  |
| 4 | G1 | Resume | 2r1 | 07-29 03:12 | 1:13 | Advanced | G1.3 | 4 | fast-engine:OK · guards:OK | $25.4288 | $0.0146 | 173,665/115,497 |
| 5 | G1 | Deliver | 1 | 07-29 04:28 | 0:24 | Advanced | G1.4 | 3 | fast-engine:OK · guards:OK | $16.5483 | $0.0156 | 225,857/83,097 |
| 6 | G2 | Deliver | 1 | 07-29 05:11 | 0:52 | Advanced | G2.1 G2.2 | 4 | fast-engine:OK · guards:OK | $25.1946 | $0.0223 | 308,321/130,080 |
| 7 | G3 | Deliver | 1 | 07-29 06:23 | 0:28 | Advanced | G3.1 | 1 | fast-engine:OK · guards:OK | $16.9647 | $0.0133 | 262,171/89,155 |
| 8 | G3 | Deliver | 1 | 07-29 06:54 | 0:10 | AgentError |  | 0 | fast-engine:FAIL · guards:FAIL |  | $0.0000 |  |
| 9 | G3 | Fix | 2 | 07-29 07:05 | 0:00 | AgentError |  | 0 | fast-engine:FAIL · guards:FAIL |  | $0.0000 |  |
| 10 | G3 | Deliver | 3 | 07-29 07:05 | 2:22 | Interrupted |  | 0 |  |  |  |  |
| 11 | G3 | Resume | 3r1 | 07-29 09:27 | 0:00 | AgentError |  | 0 | fast-engine:FAIL · guards:OK | $0.0000 | $0.0095 |  |
| 12 | G3 | Deliver | 1 | 07-29 09:29 | 0:31 | Advanced | G3.2 | 1 | fast-engine:OK · guards:OK | $16.9310 | $0.0132 | 240,450/86,120 |
| 13 | G3 | Deliver | 1 | 07-29 10:03 | 0:29 | Advanced | G3.3 | 3 | fast-engine:OK · guards:OK | $14.4742 | $0.0140 | 210,692/81,557 |
| 14 | G4 | Deliver | 1 | 07-29 10:46 | 0:28 | Advanced | G4.1 | 3 | fast-engine:OK · guards:OK | $12.9056 | $0.0173 | 404,013/93,678 |
| 15 | G4 | Deliver | 1 | 07-29 11:17 | 0:28 | Advanced | G4.2 G4.3 | 4 | fast-engine:OK · guards:OK | $14.4902 | $0.0155 | 249,634/100,477 |
| 16 | G5 | Deliver | 1 | 07-29 12:02 | 0:25 | GatesRed | G5.1 | 2 | fast-engine:FAIL · guards:OK | $10.9637 | $0.0100 | 198,784/74,494 |
| 17 | G5 | Fix | 2 | 07-29 12:29 | 0:15 | GatesRed |  | 2 | fast-engine:FAIL · guards:OK | $8.4244 | $0.0089 | 164,208/46,667 |
| 18 | G5 | Fix | 3 | 07-29 12:46 | 0:42 | Progress |  | 2 | fast-engine:OK · guards:OK | $13.0171 | $0.0110 | 221,201/103,688 |
| 19 | G5 | Deliver | 3 | 07-29 13:31 | 0:46 | Advanced | G5.2 | 4 | fast-engine:OK · guards:OK | $18.0011 | $0.0177 | 226,885/92,868 |
| 20 | G6 | Deliver | 1 | 07-29 14:31 | 0:54 | Advanced | G6.1 | 2 | fast-app:OK · guards:OK | $31.7807 | $0.0260 | 303,408/117,180 |
| 21 | G6 | Deliver | 1 | 07-29 15:30 | 0:43 | Advanced | G6.2 | 1 | fast-app:OK · guards:OK | $20.6312 | $0.0160 | 257,618/110,930 |
| 22 | G6 | Fix | 2 | 07-29 16:30 | 0:34 | Progress |  | 1 | fast-app:OK · guards:OK | $13.7036 | $0.0133 | 209,463/73,225 |

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
07-29 13:02:13  • session #16 G5 Deliver started (attempt 1/4)
07-29 13:29:27  ▪ gate fast-engine FAIL [session]  (52.7s)
07-29 13:29:27  ▪ gate guards pass [session]  (47.4s)
07-29 13:29:30  • session #16 G5 → GatesRed · done G5.1 · 2 commit(s)  (27m16s)
07-29 13:29:30  • session #17 G5 Fix started (attempt 2/4)
07-29 13:46:46  ▪ gate fast-engine FAIL [session]  (48.8s)
07-29 13:46:46  ▪ gate guards pass [session]  (39.8s)
07-29 13:46:48  • session #17 G5 → GatesRed · 2 commit(s)  (17m17s)
07-29 13:46:49  • session #18 G5 Fix started (attempt 3/4)
07-29 14:31:07  ▪ gate fast-engine pass [session]  (1m14s)
07-29 14:31:07  ▪ gate guards pass [session]  (35.6s)
07-29 14:31:10  • session #18 G5 → Progress · 2 commit(s)  (44m20s)
07-29 14:31:10  • session #19 G5 Deliver started (attempt 3/4)
07-29 15:20:37  ▪ gate fast-engine pass [session]  (1m36s)
07-29 15:20:37  ▪ gate guards pass [session]  (1m20s)
07-29 15:20:40  • session #19 G5 → Advanced · done G5.2 · 4 commit(s)  (49m30s)
07-29 15:31:45  ▪ gate fast-engine pass [phase]  (1m18s)
07-29 15:31:45  ▪ gate guards pass [phase]  (41.2s)
07-29 15:31:45  ▪ gate battery pass [phase]  (9m04s)
07-29 15:31:45  ▸ stage G5 confirmed  (2h29m31s)
07-29 15:31:49  ▸ stage G6 entered — D-4 — one vocabulary for "service" on Atlas
07-29 15:31:49  • session #20 G6 Deliver started (attempt 1/2)
07-29 16:30:47  ▪ gate fast-app pass [session]  (3m36s)
07-29 16:30:47  ▪ gate guards pass [session]  (43.2s)
07-29 16:30:50  • session #20 G6 → Advanced · done G6.1 · 2 commit(s)  (59m00s)
07-29 16:30:50  • session #21 G6 Deliver started (attempt 1/2)
07-29 17:17:17  ▪ gate fast-app pass [session]  (1m38s)
07-29 17:17:17  ▪ gate guards pass [session]  (1m00s)
07-29 17:17:20  • session #21 G6 → Advanced · done G6.2 · 1 commit(s)  (46m29s)
07-29 17:30:44  ▪ gate fast-app pass [phase]  (1m26s)
07-29 17:30:44  ▪ gate guards pass [phase]  (44.1s)
07-29 17:30:44  ▪ gate battery FAIL [phase]  (11m12s)
07-29 17:30:47  • session #22 G6 Fix started (attempt 2/2)
07-29 18:07:15  ▪ gate fast-app pass [session]  (1m27s)
07-29 18:07:15  ▪ gate guards pass [session]  (45.3s)
07-29 18:07:19  • session #22 G6 → Progress · 1 commit(s)  (36m31s)
07-29 18:17:05  ▪ gate fast-app pass [phase]  (1m19s)
07-29 18:17:05  ▪ gate guards pass [phase]  (40.0s)
07-29 18:17:05  ▪ gate battery pass [phase]  (7m46s)
07-29 18:17:05  ▸ stage G6 confirmed  (2h45m15s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 22 · retries 9 (41 %) · overall Alert
⛔ [gate-repetition] gate 'fast-engine' failed 3x in a row
⛔ [same-failure-loop] stage G3: 3 consecutive sessions made no progress
⚠ [context-saturation] session #12: 24,741,432 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #13: 20,653,481 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #19: 26,816,442 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #20: 51,631,186 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #21: 30,558,526 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #2: 23,739,978 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #4: 41,612,852 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #5: 24,423,544 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #6: 37,715,259 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #7: 24,222,376 context tokens (≥ 20,000,000)
⚠ [gate-oscillation] gate 'fast-engine' flipped pass/fail 6x
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/graph-v2
working tree: M GRAPH-V2-START.md, ?? eval-results/2026-07-19/, ?? eval-results/2026-07-27/batchA-tests.txt, ?? eval-results/2026-07-27/bench-s2-close.txt, ?? eval-results/2026-07-27/bench-s2-dntsite.txt, ?? eval-results/2026-07-27/gates-s1-close.err.txt, ?? eval-results/2026-07-27/gates-s1-close.txt, ?? eval-results/2026-07-27/gates-s2-close.err.txt (+81 more)
vs upstream: up to date
```

### Commits by session

- **s15 (G4 Deliver)** — 4 commit(s):
  - [`ebf5b3a`](https://github.com/shaahink/DevContext2/commit/ebf5b3a) G4 close: PLAN status + handoff for the next session
  - [`8541958`](https://github.com/shaahink/DevContext2/commit/8541958) G4.3: the R4 dogfood REPORT - 81 calls graded, judged against the success bar
  - [`e954c79`](https://github.com/shaahink/DevContext2/commit/e954c79) G4.2: dogfood Tasks 2+3 - a real change through MCP orientation, and DevContext on itself
  - [`546fb32`](https://github.com/shaahink/DevContext2/commit/546fb32) G4.2: the Task 2 change spec, committed BEFORE the drive
- **s16 (G5 Deliver)** — 2 commit(s):
  - [`961f239`](https://github.com/shaahink/DevContext2/commit/961f239) chore(conductor): s16 G5.1 handoff - D-3 root-caused, G5.2 scoped
  - [`d21e72b`](https://github.com/shaahink/DevContext2/commit/d21e72b) G5.1: root cause per verb - why GitVersion's five verbs join no handler
- **s17 (G5 Fix)** — 2 commit(s):
  - [`45454e8`](https://github.com/shaahink/DevContext2/commit/45454e8) chore: MCP QA report regenerated by the s17 verification run (12/12, timing line only)
  - [`68ad4ff`](https://github.com/shaahink/DevContext2/commit/68ad4ff) G5: the gate script was dying mid-step and reporting it as a test failure
- **s18 (G5 Fix)** — 2 commit(s):
  - [`c1949ef`](https://github.com/shaahink/DevContext2/commit/c1949ef) chore: MCP QA report regenerated by the s18 verification run (12/12, timing line only)
  - [`5b401dd`](https://github.com/shaahink/DevContext2/commit/5b401dd) G5: fast-engine was a coin flip, not a red — five test classes owned one env var
- **s19 (G5 Deliver)** — 4 commit(s):
  - [`e819b7d`](https://github.com/shaahink/DevContext2/commit/e819b7d) chore: s19 handoff - G5.2 claimed, G5 complete, blast radius corrected for G9.1
  - [`5805713`](https://github.com/shaahink/DevContext2/commit/5805713) chore: G5.2 evidence - measurement scripts, before/after dumps, red-then-green logs
  - [`a637b54`](https://github.com/shaahink/DevContext2/commit/a637b54) G5.2 truth: a quality RATIO cannot see a pole where nothing resolves
  - [`11ebe20`](https://github.com/shaahink/DevContext2/commit/11ebe20) G5.2: a CLI verb reaches its handler - one call spelled two ways was two facts
- **s20 (G6 Deliver)** — 2 commit(s):
  - [`850d7c4`](https://github.com/shaahink/DevContext2/commit/850d7c4) chore(conductor): G6.1 handoff for session #21
  - [`2fdd4cf`](https://github.com/shaahink/DevContext2/commit/2fdd4cf) G6.1 (R3 D-4): one vocabulary for "service" on Atlas
- **s21 (G6 Deliver)** — 1 commit(s):
  - [`051386c`](https://github.com/shaahink/DevContext2/commit/051386c) G6.2 (R3 D-4): a metadata arity marker is identity, never a name
- **s22 (G6 Fix)** — 1 commit(s):
  - [`7c65b3e`](https://github.com/shaahink/DevContext2/commit/7c65b3e) G6.3 (R3 D-4): the scope boundary names what it drops

## Last gate run

fast-app:OK · guards:OK · battery:OK

## Last session result

> SESSION-RESULT: The battery's one red (dotnet-podcasts `maui-present`) is fixed at root and the FULL `eval/gates.ps1` is now `GATE: PASS` / `BATTERY-EXIT=0` — build 0w/0e, contract sweep 0 new dead fields, fast tests, MCP QA, eval over 24 repos on both hosts (A=0 B=0), CLI matrix, `pnpm check` — committed as `7c65b3e` and pushed to `feat/graph-v2`, with G6.1 re-claimed against `eval-results/2026-07-29/G6/G6.3-EVIDENCE.md`; the cause was G6.1 silently swapping a *population* (`model.Projects` → the analysed `.sln`'s projects) inside what read as a *predicate* change, so podcasts' two MAUI clients — which live in sibling solutions — lost the only surface that had ever named them, and the fix n…

## Tracker handoff

```
**FIX SESSION — the battery's one red was a real G6.1 regression; FULL battery now `GATE: PASS` (`BATTERY-EXIT=0`).**
Evidence `eval-results/2026-07-29/G6/G6.3-EVIDENCE.md` + `G6.3-battery.log`. Red: dotnet-podcasts
`maui-present`. G6.1 swapped a PREDICATE (`model.Projects` + a home-grown skip list → `RunnableProjects(scope,…)`)
and, invisibly, a **POPULATION** — `SolutionScope` keeps only the resolved `.sln`'s csprojs, and podcasts holds
its two MAUI clients in sibling solutions, so the rollup that was the only surface ever naming them went silent.
**The lesson is bigger than the bug: G6.1's own 5-pole invariant was blind BY CONSTRUCTION.** "Breakdown rows ==
Service nodes" now reads BOTH sides from the same narrowed list, so a shared shrink measures as agreement —
podcasts read 5=5 before AND after; the 5 used to be 7. **Pair every A==B invariant with a content ratchet.**
Fix NAMES the boundary rather than re-admitting non-services: `outside_scope_apps` (proto 19; MCP `map` + Atlas
read it) renders under SCOPE as "not analyzed — 2 runnable apps outside this solution". D-4's "service" is
untouched, and the live DOM probe (`scripts/g63-outside-scope-dom.mts`) proves BOTH halves: block present, and
0 of its names leaked onto a service card. Still open, filed: **bug #19** — the same page states a FOURTH count
(STYLE evidence "6 runnable web services" vs 5 rows / 5 boxes); the STACK line still reads every discovered
project (podcasts prints `net7.0-android` from the out-of-scope csprojs). **Next = G7.1 (C-2).**
```
