# Conductor — DevContext graph-v2 — autonomous remainder run report

_Updated 2026-08-02 11:29 UTC · branch `feat/graph-v2` · HEAD `a8af4bd`_

**Status:** Idle
**Stage:** G9 — R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro) · attempts used 0
**Checkpoints:** 21/22 done · **Sessions run:** 27 · **Cost:** $347.0772 (agent $346.7374 + gates $0.3398) · **Tokens:** 4,868,373 in / 1,872,465 out
**Confirmed phases:** G1, G2, G3, G4, G5, G6, G7, G8
**Pending:** full-battery phase gate for G9

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| G1 | R4 MCP correctness + honesty fixes (R4 §1 items 1-7) | ██████████ 4/4 | confirmed ✓ |
| G2 | R4 menu hygiene + one trace default (items 11-12) | ██████████ 2/2 | confirmed ✓ |
| G3 | R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) | ██████████ 3/3 | confirmed ✓ |
| G4 | R4 dogfood drive — is the MCP a proper tool? | ██████████ 3/3 | confirmed ✓ |
| G5 | D-3 — a CLI verb reaches its handler | ██████████ 2/2 | confirmed ✓ |
| G6 | D-4 — one vocabulary for "service" on Atlas | ██████████ 2/2 | confirmed ✓ |
| G7 | C-2 / C-3 — a library's empty sections fill or withhold with a reason | ██████████ 2/2 | confirmed ✓ |
| G8 | R1 scale wall — profile HotChocolate, do not raise the timeout | ██████████ 2/2 | confirmed ✓ |
| G9 | R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro) | ██████████ 1/1 | gating… |
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

<details> ✅<summary>G7 — C-2 / C-3 — a library's empty sections fill or withhold with a reason (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G7.1 | C-2 — Atlas's five empty sections on a library either fill or withhold themselves with a stated reason | ✅ DONE | [`55af763`](https://github.com/shaahink/DevContext2/commit/55af763) |
| G7.2 | C-3 — the withhold-don't-suppress rule applied consistently wherever a surface has no entries | ✅ DONE | [`55af763`](https://github.com/shaahink/DevContext2/commit/55af763) |

</details>

<details> ✅<summary>G8 — R1 scale wall — profile HotChocolate, do not raise the timeout (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G8.1 | HotChocolate profiled: the phase that does not terminate inside the 600s budget is NAMED, with per-phase timings as evidence | ✅ DONE | [`59b17aa`](https://github.com/shaahink/DevContext2/commit/59b17aa) |
| G8.2 | Fixed, or recorded as an accepted limitation with the defect class named — R1's exit criterion answered either way. **Not by raising the timeout.** | ✅ DONE | [`59b17aa`](https://github.com/shaahink/DevContext2/commit/59b17aa) |

</details>

<details> ✅<summary>G9 — R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro) (1/1)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G9.1 | An auxiliary/demo executable stops deciding a packable library's archetype: `CLI` and `MahApps.Metro` read Library, canary poles unmoved | ✅ DONE | - |

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
| 23 | G7 | Deliver | 1 | 07-29 17:17 | 0:42 | Advanced | G7.1 G7.2 | 5 | fast-app:OK · guards:OK | $21.2053 | $0.0209 | 245,917/106,260 |
| 24 | G8 | Deliver | 1 | 07-29 18:16 | 1:13 | Advanced | G8.1 G8.2 | 4 | fast-engine:OK · guards:OK | $22.1017 | $0.0120 | 262,531/119,875 |
| 25 | G9 | Deliver | 1 | 07-29 19:43 | 0:15 | LimitBackoff |  | 0 |  | $2.5773 |  | 85,046/14,993 |
| 26 | G9 | Resume | 1 | 07-29 20:29 | 0:03 | LimitBackoff |  | 0 |  | $0.0000 |  |  |
| 27 | G9 | Resume | 1 | 08-02 10:50 | 0:34 | Advanced | G9.1 | 3 | fast-engine:OK · guards:OK | $12.2255 | $0.0219 | 215,763/73,372 |

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
07-29 18:07:15  ▪ gate fast-app pass [session]  (1m27s)
07-29 18:07:15  ▪ gate guards pass [session]  (45.3s)
07-29 18:07:19  • session #22 G6 → Progress · 1 commit(s)  (36m31s)
07-29 18:17:05  ▪ gate fast-app pass [phase]  (1m19s)
07-29 18:17:05  ▪ gate guards pass [phase]  (40.0s)
07-29 18:17:05  ▪ gate battery pass [phase]  (7m46s)
07-29 18:17:05  ▸ stage G6 confirmed  (2h45m15s)
07-29 18:17:09  ▸ stage G7 entered — C-2 / C-3 — a library's empty sections fill or withhold with a reason
07-29 18:17:09  • session #23 G7 Deliver started (attempt 1/4)
07-29 19:03:36  ▪ gate fast-app pass [session]  (1m46s)
07-29 19:03:36  ▪ gate guards pass [session]  (1m41s)
07-29 19:03:40  • session #23 G7 → Advanced · done G7.1,G7.2 · 5 commit(s)  (46m30s)
07-29 19:03:40  § owner approval requested — G7
07-29 19:03:43  ◆ plan reloaded — v5 · 10 stages · 4 gates
07-29 19:05:21  ◆ plan reloaded — v6 · 10 stages · 4 gates
07-29 19:05:39  § owner approval granted — G7
07-29 19:16:50  ▪ gate fast-app pass [phase]  (1m37s)
07-29 19:16:50  ▪ gate guards pass [phase]  (41.0s)
07-29 19:16:50  ▪ gate battery pass [phase]  (8m52s)
07-29 19:16:50  ▸ stage G7 confirmed  (59m40s)
07-29 19:16:54  ▸ stage G8 entered — R1 scale wall — profile HotChocolate, do not raise the timeout
07-29 19:16:55  • session #24 G8 Deliver started (attempt 1/4)
07-29 20:32:39  ▪ gate fast-engine pass [session]  (1m21s)
07-29 20:32:39  ▪ gate guards pass [session]  (38.9s)
07-29 20:32:42  • session #24 G8 → Advanced · done G8.1,G8.2 · 4 commit(s)  (1h15m47s)
07-29 20:43:06  ▪ gate fast-engine pass [phase]  (1m24s)
07-29 20:43:06  ▪ gate guards pass [phase]  (39.9s)
07-29 20:43:06  ▪ gate battery pass [phase]  (8m19s)
07-29 20:43:06  ▸ stage G8 confirmed  (1h26m11s)
07-29 20:43:11  ▸ stage G9 entered — R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro)
07-29 20:43:11  • session #25 G9 Deliver started (attempt 1/2)
07-29 20:59:12  • session #25 G9 → LimitBackoff  (16m00s)
07-29 21:29:10  • session #26 G9 Resume started (attempt 1/2)
07-29 21:32:36  • session #26 G9 → LimitBackoff  (3m26s)
08-02 11:38:44  ◆ run resumed · DevContext graph-v2 — autonomous remainder
08-02 11:50:24  ◆ plan reloaded — v7 · 10 stages · 4 gates
08-02 11:50:29  ◆ plan reloaded — v8 · 10 stages · 4 gates
08-02 11:50:35  • session #27 G9 Resume started (attempt 1/2)
08-02 12:29:07  ▪ gate fast-engine pass [session]  (2m06s)
08-02 12:29:07  ▪ gate guards pass [session]  (1m33s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 27 · retries 9 (33 %) · overall Alert
⛔ [gate-repetition] gate 'fast-engine' failed 3x in a row
⛔ [same-failure-loop] stage G3: 3 consecutive sessions made no progress
⚠ [context-saturation] session #12: 24,741,432 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #13: 20,653,481 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #19: 26,816,442 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #20: 51,631,186 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #21: 30,558,526 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #23: 32,174,554 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #24: 32,989,754 context tokens (≥ 20,000,000)
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
working tree: M .conductor/REPORT.md, M conductor.plan.json, ?? eval-results/2026-07-19/, ?? eval-results/2026-07-27/batchA-tests.txt, ?? eval-results/2026-07-27/bench-s2-close.txt, ?? eval-results/2026-07-27/bench-s2-dntsite.txt, ?? eval-results/2026-07-27/gates-s1-close.err.txt, ?? eval-results/2026-07-27/gates-s1-close.txt (+84 more)
vs upstream: up to date
```

### Commits by session

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
- **s23 (G7 Deliver)** — 5 commit(s):
  - [`ff64251`](https://github.com/shaahink/DevContext2/commit/ff64251) chore(tracker): stage G7 complete — C-2 + C-3 landed, C-3 boundary named
  - [`5ba0ec7`](https://github.com/shaahink/DevContext2/commit/5ba0ec7) G7.2 (R3 C-3): zero entries is not "no analysis"
  - [`1518807`](https://github.com/shaahink/DevContext2/commit/1518807) conductor: the orchestrator plan edits from this run (planVersion 5, run budget)
  - [`d6ef0b0`](https://github.com/shaahink/DevContext2/commit/d6ef0b0) chore(tracker): G7.1 handoff — C-2 landed, C-3 instrument named
  - [`55af763`](https://github.com/shaahink/DevContext2/commit/55af763) G7.1 (R3 C-2): a library's empty Atlas sections say why
- **s24 (G8 Deliver)** — 4 commit(s):
  - [`2fb2cc2`](https://github.com/shaahink/DevContext2/commit/2fb2cc2) chore(conductor): commit orchestrator plan state + refreshed mcp-qa artifact
  - [`73a03c2`](https://github.com/shaahink/DevContext2/commit/73a03c2) chore(conductor): s24 G8 handoff — scale wall profiled + fixed
  - [`d6be215`](https://github.com/shaahink/DevContext2/commit/d6be215) perf(G8.2): one per-file index kills the R1 scale wall — HotChocolate 1275s -> 64s
  - [`59b17aa`](https://github.com/shaahink/DevContext2/commit/59b17aa) perf(G8.1): profile HotChocolate — the scale wall is one file meeting a per-file quadratic
- **s27 (G9 Resume)** — 3 commit(s):
  - [`a8af4bd`](https://github.com/shaahink/DevContext2/commit/a8af4bd) docs(tracker): G9.1 handoff
  - [`7d82108`](https://github.com/shaahink/DevContext2/commit/7d82108) docs(eval): G9.1 evidence - archetype vs auxiliary executable
  - [`ea0dc3f`](https://github.com/shaahink/DevContext2/commit/ea0dc3f) fix(archetype): an auxiliary executable stops deciding a packable library's archetype (G9.1)

## Last gate run

fast-engine:OK · guards:OK

## Last session result

> SESSION-RESULT: **G9.1 landed and claimed** — fix @ `ea0dc3f`, evidence @ `7d82108`, handoff @ `a8af4bd`, all pushed to `feat/graph-v2`; tree clean. `CLI` (dotnet/command-line-api) and `MahApps.Metro` now read **Library / NotApplicable** with `graph-truth style=PASS` on both, and **no expectation was touched** — the poles were pinned RED and the engine moved to meet them. The stage note's framing turned out to be wrong: it is **two root causes, not one**, and "a demo exe outside `samples/`" describes neither precisely. MahApps was a *path shape* — all 25 of its entry points came from `src/MahApps.Metro.Samples/` and none from the library, but `IsSamplePath` matched `samples` only as a slash-…

## Tracker handoff

```
**STAGE G9 COMPLETE — G9.1 fix @ ea0dc3f, evidence @ 7d82108** (`eval-results/2026-07-29/G9/G9.1-EVIDENCE.md`).
`CLI` and `MahApps.Metro` both read **Library / NotApplicable**; graph-truth `style=PASS` on both; expectations
untouched. **It was TWO root causes, not one, and the stage note's "a demo exe outside samples/" was wrong for
the pole it mattered on.** (1) MahApps = PATH SHAPE: all 25 entries came from `src/MahApps.Metro.Samples/` and
NONE from the library, but `IsSamplePath` matched `samples` only as a slash-delimited segment — the char before
`Samples` is `.`. It now also reads the dotted-compound **collection** (`*.Samples/.Examples/.Snippets/.Demos`);
**plural only** — `OrchardCore.Demo` is a shipped module. (2) CLI = LADDER ORDER, no sample path: `dotnet-suggest`
is a real `Exe`+`PackAsTool` under a production path, so the CliTool rung returned before the auxiliary-exe test
the detector **already owned** was consulted. That test now runs once above the CliTool AND entries rungs
(`DescribeLibraryShape`) and overrules them only on **symmetric declaration evidence**: `PackAsTool` on the exe
loses to `<IsPackable>true</IsPackable>` on the library it references. `hasPublicSurface` would flip GitVersion —
measured; that is the canary. **Dead end, do not retry:** filtering entries by auxiliary-project provenance
FAILS here — CLI's 2nd entry is declared inside the library itself (`System.CommandLine/RootCommand.cs:21`).
**Instrument: `g9-archetype-sweep.ps1`** (37 poles, `--no-cache` both sides, run BEFORE against a `bin/` copy so
you can rebuild underneath it). 27/37 finished both sides; only the 2 targets moved archetype (`Desktop` pole's
STYLE moved Unknown→SampleCollection — undeclared by design, it IS avalonia-samples). **TRAP: never overlap
`dotnet test` with a bg analyze sweep — that is the McpQa load flake (bug #3/#1); green alone, 2/2 in 12s.**
**Next = G10.1** (sweep for thresholds calibrated on pre-Batch-A starved-graph data). Bug #20 still open
(`RunnableProjects` counts an auxiliary demo exe as a SERVICE — the render half of this same idea).
```
