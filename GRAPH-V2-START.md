# DevContext graph-v2 — autonomous remainder Phase Tracker

**Plan:** DevContext graph-v2 — autonomous remainder | **Branch:** `feat/graph-v2` | **Design doc:** docs/dev/research/PLAN.md

## Handoff (overwrite this block, ≤12 lines, no history)

**G5.2 CLAIMED — the join lands: gitversion `entriesWithTarget` 0/5 → 4/5, CleanArchitecture canary
bit-identical** (139 nodes / 64 edges / 7 entries / 5 targets both sides). Writeup + every raw dump:
`eval-results/2026-07-29/G5.2/G5.2-EVIDENCE.md`. Commits `11ebe20` (fix+tests) · `a637b54` (truth
ratchet) · `5805713` (evidence). **G5 is now fully claimed; the next session starts G6.1.**
**G5.1 §5 named the WRONG LOCUS for Defect 1 and its own §3 disproves it** — `HopThroughProperty` is
properties-only and GitVersion's collaborators are FIELDS (`TypeDiscovery` has no `Fields` at all).
Real fix was upstream in `BodyFactExtractor` (new `SplitReceiver`): a receiver chain rooted at `this`
reports the member declared on `this`, so `this.x.M()` and `x.M()` are one fact. `BuildTypeScope`
already held every field/property/primary-ctor param. Plus the entry builder joins the execute MEMBER,
by SHAPE (the method taking the detected settings type), not by `ExecuteMethod`'s name list.
**BLAST RADIUS IS WIDER THAN G5.1 §4b SAID** (it sampled 7 repos): Orleans **1603** sites, GitVersion
878, PowerToys 440, MahApps.Metro 266, CommunityToolkit.Mvvm 186 (`blast-radius.csv`, all 47 repos).
All four unmeasured movers were graph-truth'd and diffed vs their 07-28 verdicts — **identical**,
including the two pre-existing FAILs (Orleans handler-join; MahApps `style`, which IS G9.1's subject).
**G9.1 and any R1 scale work must RE-MEASURE, not inherit a pre-G5.2 number.**
Gates at close: build 0w/0e · loom-guards PASS (incl. `Category=Truth`) · contract-sweep PASS ·
Server 104/104 · Core 713 pass, 1 fail = **bug #1** (cold-snapshot MCP QA race; warm re-run passes).
**NEW TRAP, may hit the battery:** the MCP QA gate can leave a `DevContext.Server.exe` alive holding
`DevContext.Core.dll`; the next build then fails MSB3027 with "10 Warning(s)" that are copy retries,
not code. Read the pid out of the error, stop THAT pid only. Zero were alive at close, verified.
Still open and untouched: bug #12 (Defect 2), #14 (generic `[Command<Parent>]` sub-commands), #16.


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 22 |
| Done | 0 |
| Claimed (unconfirmed) | 13 |

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
