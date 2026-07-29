# Conductor — DevContext graph-v2 — autonomous remainder run report

_Updated 2026-07-29 01:29 UTC · branch `feat/graph-v2` · HEAD `0f1a5fc`_

**Status:** Idle
**Stage:** G1 — R4 MCP correctness + honesty fixes (R4 §1 items 1-7) · attempts used 0 · working ▸ G1.2
**Checkpoints:** 1/22 done · **Sessions run:** 1 · **Cost:** $12.7032 (agent $12.6728 + gates $0.0305) · **Tokens:** 172,654 in / 66,379 out

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| G1 | R4 MCP correctness + honesty fixes (R4 §1 items 1-7) | ██░░░░░░░░ 1/4 | **← active** |
| G2 | R4 menu hygiene + one trace default (items 11-12) | ░░░░░░░░░░ 0/2 | todo |
| G3 | R4 missing primitives: seam / kind-filtered neighbours / cache truth (8-10) | ░░░░░░░░░░ 0/3 | todo |
| G4 | R4 dogfood drive — is the MCP a proper tool? | ░░░░░░░░░░ 0/3 | todo |
| G5 | D-3 — a CLI verb reaches its handler | ░░░░░░░░░░ 0/2 | todo |
| G6 | D-4 — one vocabulary for "service" on Atlas | ░░░░░░░░░░ 0/2 | todo |
| G7 | C-2 / C-3 — a library's empty sections fill or withhold with a reason | ░░░░░░░░░░ 0/2 | todo |
| G8 | R1 scale wall — profile HotChocolate, do not raise the timeout | ░░░░░░░░░░ 0/2 | todo |
| G9 | R1 archetype loses to an auxiliary executable (CLI, MahApps.Metro) | ░░░░░░░░░░ 0/1 | todo |
| G10 | Sweep for thresholds calibrated on pre-Batch-A data | ░░░░░░░░░░ 0/1 | todo |

<details><summary>G1 — R4 MCP correctness + honesty fixes (R4 §1 items 1-7) (1/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| G1.1 | `map` returns the structured Map surface (library surface, packages, aggregates, service styles); its markdown stops advertising CLI flags that don't exist over MCP | ✅ DONE | - |
| G1.2 | `get_context` accepts type/symbol roots — a library gets a pack instead of nothing | ⬜ TODO | - |
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

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
07-29 01:28:11  ◆ run started · DevContext graph-v2 — autonomous remainder
07-29 01:28:12  ▸ stage G1 entered — R4 MCP correctness + honesty fixes (R4 §1 items 1-7)
07-29 01:28:12  • session #1 G1 Deliver started (attempt 1/6)
07-29 02:29:42  ▪ gate fast-engine pass [session]  (4m09s)
07-29 02:29:42  ▪ gate guards pass [session]  (55.6s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 1 · retries 0 (0 %) · overall Ok
✓ no health concerns detected
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/graph-v2
working tree: ?? eval-results/2026-07-19/, ?? eval-results/2026-07-27/batchA-tests.txt, ?? eval-results/2026-07-27/bench-s2-close.txt, ?? eval-results/2026-07-27/bench-s2-dntsite.txt, ?? eval-results/2026-07-27/gates-s1-close.err.txt, ?? eval-results/2026-07-27/gates-s1-close.txt, ?? eval-results/2026-07-27/gates-s2-close.err.txt, ?? eval-results/2026-07-27/gates-s2-close.txt (+80 more)
vs upstream: up to date
```

### Commits by session

- **s1 (G1 Deliver)** — 2 commit(s):
  - [`0f1a5fc`](https://github.com/shaahink/DevContext2/commit/0f1a5fc) conductor: orchestrator's own template + plan edits from this run
  - [`cf1a822`](https://github.com/shaahink/DevContext2/commit/cf1a822) G1.1: the structured map the MCP dropped, and the flags it told agents to run

## Last gate run

fast-engine:OK · guards:OK

## Last session result

> SESSION-RESULT: G1.1 is DONE and claimed with evidence at `eval-results/2026-07-29/G1.1-EVIDENCE.md` — `map` now returns the structured Map surface and no shared-render markdown advertises a CLI flag, proven by real MCP calls on a library pole (FluentValidation, 6/6 PASS) and a multi-solution pole (GitVersion, `solutionScope` 1-of-3, 120s cold analyse confirming the render came from the new Core); commits cf1a822 + 0f1a5fc pushed to `feat/graph-v2`, tracked tree clean, port 5179 free. Green: build 0w/0e, contract-sweep PASS (0 new), loom-guards PASS, slnx `Category!=Eval` 689 pass / 0 fail once the single red was run down and acquitted — nothing is red. The next session should take **G1.2** …

## Tracker handoff

```
last: **G1.1 CLAIMED** — `map` now returns the structured surface (FluentValidation: entryApi 10 ·
  abstractions 10 · groups 5 · internals 1 · extensionPoints 68 · consumerPaths 6 · surface packages)
  and no shared-render markdown names a CLI flag. Evidence = real MCP calls, not a diff:
  `eval-results/2026-07-29/G1.1-EVIDENCE.md`. The aborted session's tree was kept where right and
  corrected where not: it MISSED `LibrarySurfaceRenderer.cs:122,125` ("use --format json"), which fire
  on exactly the library archetype item 1 is about. Its six goldens were reverted and REGENERATED from
  a fresh run — the diff came back byte-identical (footer line only), so they were right, just unproven.
stage: **G1 IN PROGRESS** — G1.1 done, G1.2 next.
gate: build 0w/0e · contract-sweep PASS (0 new) · loom-guards PASS · slnx `Category!=Eval` = 689 pass,
  0 fail after the one red was run down. **READ THIS BEFORE PANICKING AT THE BATTERY:** `McpQaGateTests`
  scores a false **0/12** on the first run after ANY Core edit — MVID-keyed snapshots invalidate, the
  harness takes a session before its graph exists. Cold 0/12 -> warm 12/12, same binaries. Tracked bug #1.
next: **G1.2** — `get_context` accepts type/symbol roots so a library gets a pack instead of nothing.
  `drive-r4.js getctx-library` already exists and MEASURES the before-state — run it first.
trap: the MCP driver leaves a spawned DevContext.Server holding bin DLL locks — kill it by PID before
  any build, `start-dev-bg.ps1 -Kill` does NOT get it. Pin `DEVCONTEXT_SERVER` or a stale/installed
  server serves your "evidence". Find flag leaks by sweeping Core renderers, not by reading the [audit] line.
```
