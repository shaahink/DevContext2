# Conductor — DevContext graph-v2 — autonomous remainder run report

_Updated 2026-07-29 11:17 UTC · branch `feat/graph-v2` · HEAD `fa8ce07`_

**Status:** Idle — advisor: human intervention required
**Stage:** G4 — R4 dogfood drive — is the MCP a proper tool? · attempts used 0 · working ▸ G4.2
**Checkpoints:** 10/22 done · **Sessions run:** 14 · **Cost:** $157.7821 (agent $157.6155 + gates $0.1666) · **Tokens:** 2,227,915 in / 838,436 out
**Confirmed phases:** G1, G2, G3

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| G1 | R4 MCP correctness + honesty fixes (R4 §1 items 1-7) | ██████████ 4/4 | confirmed ✓ |
| G2 | R4 menu hygiene + one trace default (items 11-12) | ██████████ 2/2 | confirmed ✓ |
| G3 | R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) | ██████████ 3/3 | confirmed ✓ |
| G4 | R4 dogfood drive — is the MCP a proper tool? | ███░░░░░░░ 1/3 | **← active** |
| G5 | D-3 — a CLI verb reaches its handler | ░░░░░░░░░░ 0/2 | todo |
| G6 | D-4 — one vocabulary for "service" on Atlas | ░░░░░░░░░░ 0/2 | todo |
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

<details><summary>G4 — R4 dogfood drive — is the MCP a proper tool? (1/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G4.1 | Dogfood Task 1 — 10 real architecture questions on an unseen repo, MCP tools only, every call logged and graded HELPED / NEUTRAL / HURT | ✅ DONE | - |
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

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
07-29 07:23:03  ▪ gate fast-engine pass [phase]  (1m39s)
07-29 07:23:03  ▪ gate guards pass [phase]  (46.5s)
07-29 07:23:03  ▪ gate battery pass [phase]  (12m59s)
07-29 07:23:03  ▸ stage G2 confirmed  (1h11m20s)
07-29 07:23:06  ▸ stage G3 entered — R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10)
07-29 07:23:06  • session #7 G3 Deliver started (attempt 1/4)
07-29 07:54:04  ▪ gate fast-engine pass [session]  (1m29s)
07-29 07:54:04  ▪ gate guards pass [session]  (42.9s)
07-29 07:54:07  • session #7 G3 → Advanced · done G3.1 · 1 commit(s)  (31m00s)
07-29 07:54:07  • session #8 G3 Deliver started (attempt 1/4)
07-29 08:05:07  ▪ gate fast-engine FAIL [session]  (0.2s)
07-29 08:05:07  ▪ gate guards FAIL [session]  (0.0s)
07-29 08:05:08  • session #8 G3 → AgentError  (11m00s)
07-29 08:05:08  • session #9 G3 Fix started (attempt 2/4)
07-29 08:05:09  ▪ gate fast-engine FAIL [session]  (0.0s)
07-29 08:05:09  ▪ gate guards FAIL [session]  (0.0s)
07-29 08:05:09  ■ needs human — advisor: human intervention required
07-29 08:05:09  • session #9 G3 → AgentError  (0.9s)
07-29 08:05:09  • session #10 G3 Deliver started (attempt 3/4)
07-29 10:27:54  ◆ run resumed · DevContext graph-v2 — autonomous remainder
07-29 10:27:54  • session #11 G3 Resume started (attempt 3/4)
07-29 10:29:32  ▪ gate fast-engine FAIL [session]  (8.2s)
07-29 10:29:32  ▪ gate guards pass [session]  (1m26s)
07-29 10:29:36  • session #11 G3 → AgentError  (1m41s)
07-29 10:29:58  • session #12 G3 Deliver started (attempt 1/4)
07-29 11:03:24  ▪ gate fast-engine pass [session]  (1m17s)
07-29 11:03:24  ▪ gate guards pass [session]  (55.5s)
07-29 11:03:28  • session #12 G3 → Advanced · done G3.2 · 1 commit(s)  (33m29s)
07-29 11:03:28  • session #13 G3 Deliver started (attempt 1/4)
07-29 11:35:08  ▪ gate fast-engine pass [session]  (1m40s)
07-29 11:35:08  ▪ gate guards pass [session]  (39.7s)
07-29 11:35:11  • session #13 G3 → Advanced · done G3.3 · 3 commit(s)  (31m42s)
07-29 11:46:50  ▪ gate fast-engine pass [phase]  (1m26s)
07-29 11:46:50  ▪ gate guards pass [phase]  (41.0s)
07-29 11:46:50  ▪ gate battery pass [phase]  (9m30s)
07-29 11:46:50  ▸ stage G3 confirmed  (4h23m43s)
07-29 11:46:55  ▸ stage G4 entered — R4 dogfood drive — is the MCP a proper tool?
07-29 11:46:55  • session #14 G4 Deliver started (attempt 1/4)
07-29 12:17:55  ▪ gate fast-engine pass [session]  (1m36s)
07-29 12:17:55  ▪ gate guards pass [session]  (1m16s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 14 · retries 5 (36 %) · overall Alert
⛔ [gate-repetition] gate 'fast-engine' failed 3x in a row
⛔ [same-failure-loop] stage G3: 3 consecutive sessions made no progress
⚠ [context-saturation] session #12: 24,741,432 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #13: 20,653,481 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #2: 23,739,978 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #4: 41,612,852 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #5: 24,423,544 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #6: 37,715,259 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #7: 24,222,376 context tokens (≥ 20,000,000)
⚠ [gate-oscillation] gate 'fast-engine' flipped pass/fail 4x
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/graph-v2
working tree: M eval-results/2026-07-29/mcp-qa.md, ?? eval-results/2026-07-19/, ?? eval-results/2026-07-27/batchA-tests.txt, ?? eval-results/2026-07-27/bench-s2-close.txt, ?? eval-results/2026-07-27/bench-s2-dntsite.txt, ?? eval-results/2026-07-27/gates-s1-close.err.txt, ?? eval-results/2026-07-27/gates-s1-close.txt, ?? eval-results/2026-07-27/gates-s2-close.err.txt (+81 more)
vs upstream: up to date
```

### Commits by session

- **s2 (G1 Deliver)** — 3 commit(s):
  - [`27de288`](https://github.com/shaahink/DevContext2/commit/27de288) conductor: G1.2 handoff + the orchestrator's own plan edits from this run
  - [`7c6eb5e`](https://github.com/shaahink/DevContext2/commit/7c6eb5e) G1.2: a library symbol gets a pack — member roots resolve, and the pack carries who uses it
  - [`79743b0`](https://github.com/shaahink/DevContext2/commit/79743b0) conductor: telegram gets the one field without which it is a silent no-op
- **s4 (G1 Resume)** — 4 commit(s):
  - [`56f2bcf`](https://github.com/shaahink/DevContext2/commit/56f2bcf) PLAN §2: R4 status through G1.3, and the verification-command trap that produced three sessions of phantom red
  - [`611dae3`](https://github.com/shaahink/DevContext2/commit/611dae3) conductor: G1.3 handoff + the orchestrator's own plan edits from this run
  - [`2d27a6c`](https://github.com/shaahink/DevContext2/commit/2d27a6c) G1.3 evidence: before/after MCP responses, battery-style gate logs, and the driver runners
  - [`75704f2`](https://github.com/shaahink/DevContext2/commit/75704f2) G1.3: a seam says what kind it is, a handle-less call stays in its own repo, and no tool throws a stack trace at an agent
- **s5 (G1 Deliver)** — 3 commit(s):
  - [`53e79aa`](https://github.com/shaahink/DevContext2/commit/53e79aa) PLAN §2 + handoff: R4 §1 items 1-7 complete, and the vacuous-check lesson recorded
  - [`74a1d73`](https://github.com/shaahink/DevContext2/commit/74a1d73) G1.4 evidence: before/after MCP responses, the tightened driver cases, and the gate logs
  - [`a09c456`](https://github.com/shaahink/DevContext2/commit/a09c456) G1.4: a total that is a fact, and an analyze that says what it just did
- **s6 (G2 Deliver)** — 4 commit(s):
  - [`efe70fb`](https://github.com/shaahink/DevContext2/commit/efe70fb) G2 handoff: stage complete, next session opens G3.1
  - [`7d42c08`](https://github.com/shaahink/DevContext2/commit/7d42c08) G2.2: one trace policy, and the rule that had never run
  - [`b579030`](https://github.com/shaahink/DevContext2/commit/b579030) G2.1 evidence: the menu in three states, with the red in the middle
  - [`35eea1e`](https://github.com/shaahink/DevContext2/commit/35eea1e) G2.1: 24 tools become 21, and the menu stops being told twice
- **s7 (G3 Deliver)** — 1 commit(s):
  - [`baa5ffd`](https://github.com/shaahink/DevContext2/commit/baa5ffd) G3.1: the path between two symbols, and the roll-up that makes it real
- **s12 (G3 Deliver)** — 1 commit(s):
  - [`d82d074`](https://github.com/shaahink/DevContext2/commit/d82d074) G3.2: who writes this table, and the roll-up that makes it answerable
- **s13 (G3 Deliver)** — 3 commit(s):
  - [`1b3f7b4`](https://github.com/shaahink/DevContext2/commit/1b3f7b4) chore: mcp-qa report timing, rewritten by the McpQa gate run
  - [`9beb733`](https://github.com/shaahink/DevContext2/commit/9beb733) chore(G3.3): handoff for the next session
  - [`cf0fa62`](https://github.com/shaahink/DevContext2/commit/cf0fa62) G3.3: how old is this answer, and which commit is it about
- **s14 (G4 Deliver)** — 3 commit(s):
  - [`fa8ce07`](https://github.com/shaahink/DevContext2/commit/fa8ce07) G4.1: PLAN status + handoff for the next session
  - [`55e7cec`](https://github.com/shaahink/DevContext2/commit/55e7cec) G4.1: the R4 dogfood drive - 43 MCP calls on Hangfire, every one graded
  - [`254fd36`](https://github.com/shaahink/DevContext2/commit/254fd36) G4.1: ten dogfood questions, committed BEFORE the drive

## Last gate run

fast-engine:OK · guards:OK

## Last session result

> SESSION-RESULT:** G4.1 is claimed and pushed (`conductor task --done G4.1 --evidence eval-results/2026-07-29/mcp-dogfood/G4.1-EVIDENCE.md`, board verified DONE; commits `254fd36` → `55e7cec` → `fa8ce07` on `feat/graph-v2`). The R4 §2 Task 1 dogfood drive ran: ten architecture questions committed *before* the driver existed, then 43 MCP calls against `eval-repos/Hangfire` (unseen, archetype Library, 0 entries) with no grep and `read_source` never called — 44,712 response tokens, 12.9 s of tool wall time, graded HELPED 28 / NEUTRAL 7 / HURT 8, every grade re-checkable against the 43 raw responses the driver wrote. **The verdict is 8/10 against §3's bar, and 6/10 under the stricter reading wher…

## Tracker handoff

```
**G4.1 CLAIMED — the dogfood drive is RUN and graded.** 43 MCP calls on Hangfire (unseen, Library,
0 entries), questions committed BEFORE the drive (254fd36), no grep and `read_source` never called.
**8/10 on §3's bar — 6/10 if "answered" must mean a TOOL asserted it** (Q4/Q5 fell to inference: the
graph has NO inheritance edge kind, so "who implements this" cannot be asked). 44,712 tokens, of which
`map` alone is 17,105. Evidence `eval-results/2026-07-29/mcp-dogfood/G4.1-EVIDENCE.md` + CALL-GRADES.md,
commit 55e7cec. Four bugs filed: **#5** every tool ships `description:""` (MEASURED: the fix is
`[System.ComponentModel.Description]`, NOT GenerateDocumentationFile — that changes nothing);
**#6** `trace` + a nodeId → `found:true`, 0 steps, "Type: Type" (its focus resolver matches the token
BEFORE the first colon and ignores the rest); **#7** a method registered as a Type node, 26 BCL
`System.Type` refs bound to it, 5th wiring hub; **#8** calls inside a LAMBDA ARGUMENT make no edge, so
the enqueue path's actual storage write is invisible while the trace looks complete.
Next: **G4.2** (Tasks 2+3). Drive with `node eval/mcp-qa/dogfood.js <batch.json>`; START THE SERVER
FIRST (the MCP kills a server it spawned, so handles die with each driver run). Use BARE NAMES in
`query` — never a nodeId on `trace`. Before Task 3, R4 §2 warns the server ignores devcontext.json,
so CLI and MCP see different file sets on this repo: measure that first, don't inherit the claim.
```
