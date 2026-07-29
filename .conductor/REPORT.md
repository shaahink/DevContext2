# Conductor — DevContext graph-v2 — autonomous remainder run report

_Updated 2026-07-29 06:07 UTC · branch `feat/graph-v2` · HEAD `efe70fb`_

**Status:** Idle
**Stage:** G2 — R4 menu hygiene + one trace default (items 11-12) · attempts used 0
**Checkpoints:** 6/22 done · **Sessions run:** 6 · **Cost:** $96.4393 (agent $96.3401 + gates $0.0993) · **Tokens:** 1,110,589 in / 487,926 out
**Confirmed phases:** G1
**Pending:** full-battery phase gate for G2

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| G1 | R4 MCP correctness + honesty fixes (R4 §1 items 1-7) | ██████████ 4/4 | confirmed ✓ |
| G2 | R4 menu hygiene + one trace default (items 11-12) | ██████████ 2/2 | gating… |
| G3 | R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) | ░░░░░░░░░░ 0/3 | todo |
| G4 | R4 dogfood drive — is the MCP a proper tool? | ░░░░░░░░░░ 0/3 | todo |
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
| G2.1 | Tool menu folded (`flow`→`trace(compact)`, `insights`→`stats`, `interesting_points`→`overview`) and the did-you-mean handler reads the real tool list instead of a second hand-maintained one | ✅ DONE | - |
| G2.2 | One trace budget default across MCP / CLI / server, read from `TracePolicy` (Batch E's single source) | ✅ DONE | - |

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
| 3 | G1 | Fix | 2 | 07-29 03:04 | 0:06 | Interrupted |  | 0 |  |  |  |  |
| 4 | G1 | Resume | 2r1 | 07-29 03:12 | 1:13 | Advanced | G1.3 | 4 | fast-engine:OK · guards:OK | $25.4288 | $0.0146 | 173,665/115,497 |
| 5 | G1 | Deliver | 1 | 07-29 04:28 | 0:24 | Advanced | G1.4 | 3 | fast-engine:OK · guards:OK | $16.5483 | $0.0156 | 225,857/83,097 |
| 6 | G2 | Deliver | 1 | 07-29 05:11 | 0:52 | Advanced | G2.1 G2.2 | 4 | fast-engine:OK · guards:OK | $25.1946 | $0.0223 | 308,321/130,080 |

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
07-29 03:36:37  ◆ run resumed · DevContext graph-v2 — autonomous remainder
07-29 04:04:17  • session #3 G1 Fix started (attempt 2/6)
07-29 04:11:14  ◆ run resumed · DevContext graph-v2 — autonomous remainder
07-29 04:12:15  • session #4 G1 Resume started (attempt 2/6)
07-29 05:28:29  ▪ gate fast-engine pass [session]  (1m38s)
07-29 05:28:29  ▪ gate guards pass [session]  (48.0s)
07-29 05:28:32  • session #4 G1 → Advanced · done G1.3 · 4 commit(s)  (1h16m17s)
07-29 05:28:32  • session #5 G1 Deliver started (attempt 1/6)
07-29 05:55:31  ▪ gate fast-engine pass [session]  (1m46s)
07-29 05:55:31  ▪ gate guards pass [session]  (49.7s)
07-29 05:55:34  • session #5 G1 → Advanced · done G1.4 · 3 commit(s)  (27m02s)
07-29 06:11:38  ▪ gate fast-engine pass [phase]  (1m50s)
07-29 06:11:38  ▪ gate guards pass [phase]  (50.2s)
07-29 06:11:38  ▪ gate battery pass [phase]  (13m22s)
07-29 06:11:38  ▸ stage G1 confirmed  (4h43m26s)
07-29 06:11:43  ▸ stage G2 entered — R4 menu hygiene + one trace default (items 11-12)
07-29 06:11:43  • session #6 G2 Deliver started (attempt 1/2)
07-29 07:07:33  ▪ gate fast-engine pass [session]  (1m57s)
07-29 07:07:33  ▪ gate guards pass [session]  (1m45s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 6 · retries 2 (33 %) · overall Warn
⚠ [context-saturation] session #2: 23,739,978 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #4: 41,612,852 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #5: 24,423,544 context tokens (≥ 20,000,000)
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/graph-v2
working tree: M eval-results/2026-07-29/mcp-qa.md, ?? eval-results/2026-07-19/, ?? eval-results/2026-07-27/batchA-tests.txt, ?? eval-results/2026-07-27/bench-s2-close.txt, ?? eval-results/2026-07-27/bench-s2-dntsite.txt, ?? eval-results/2026-07-27/gates-s1-close.err.txt, ?? eval-results/2026-07-27/gates-s1-close.txt, ?? eval-results/2026-07-27/gates-s2-close.err.txt (+81 more)
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

## Last gate run

fast-engine:OK · guards:OK

## Last session result

> SESSION-RESULT: Stage G2 is complete — G2.1 (tool menu folded 24→21, did-you-mean now reads the SDK's real tool collection) and G2.2 (one trace budget/depth policy across MCP/CLI/server, read from `TracePolicy`) are both landed, claimed via `conductor task --done`, and pushed to `feat/graph-v2` (`35eea1e`, `b579030`, `7d42c08`, `efe70fb`); R4 §1 items 1–7 + 11–12 are now all done. Evidence: `eval-results/2026-07-29/G2.1-EVIDENCE.md` and `G2.2-EVIDENCE.md`, each with a before/red/after driver run (new `menu` and `trace-budget` cases in `eval/mcp-qa/drive-r4.js`). Nothing is red: build 0w/0e, Core 672 passed/2 skipped/674, Server 76, contract sweep PASS, loom-guards PASS, app lint + 120 tests …

## Tracker handoff

```
**STAGE G2 COMPLETE.** G2.1 @ 35eea1e (+ b579030) · G2.2 @ 7d42c08 — evidence
`eval-results/2026-07-29/G2.{1,2}-EVIDENCE.md`. R4 §1 items 1-7 + 11-12 all landed. Next: **G3.1**
(`seam(from,to)` at proto + GraphQuery + tool). Test counts: **Core 674 / Server 76** (G1.4 was
674/58) — a passing test is NOT named in the log, so the count delta is your only proof.
**Verify with the battery's OWN commands**, never the bare `--filter "Category!=Eval"`:
`"Category!=Eval&Category!=CliSmoke&Category!=McpQa"`, then `"Category=McpQa"` ALONE (gates.ps1:136).
**MIRRORING A CONSTANT DOES NOT KEEP TWO SURFACES TOGETHER — not restating it does.** Both G2 fixes
are that one lesson: a hand-kept tool list that was CORRECT, and MCP dials that mirrored TracePolicy.
A C# parameter default is not an unset field — assigning a proto3 `optional` sets its presence bit,
which had silently disabled `TracePolicy.ElasticDepth` on every request the product ever served.
G3 opens the proto: fold in `TraceResponse.applied_budget_tokens` (G2.2 §6) while you are there.
**Watch your driver check go red before you trust it** — 4-for-4 now. G2.1's equality PASSED on the
before-state (the stale list was still correct); G2.2's budget check would have been theatre (the
4000 default cuts NOTHING on eShop at any depth — the observable half was the depth rule).
Traps re-paid: the MCP spawns a DevContext.Server that outlives the driver and locks
Core/Cli/Contracts.dll — `dotnet build` then reports 6 errors + 30 warnings that read as a code
break; kill it BY PID. `InternalsVisibleTo` is unusable for DevContext.Mcp (its top-level `Program`
collides with the Server's → CS0433). Run `pnpm gen:proto` after any proto edit.
```
