# Re-measure after the fix run — Book2Course, post F1/F3/F4/F5

2026-08-27. This is the dated addendum to `../DRIVE.md`: the same recorded call batches, re-driven
against the fixed build. Build under test: `fix/mcp-drive-integration` @ `9cfefc2`, built in the
integration worktree (`dotnet build DevContext.slnx` 0w/0e, CLI rebuilt per house rule). Target:
`C:/Code/BookToCourse` @ `c14da997` — same HEAD as the original drive, untouched. Analysis was
**fresh, not cached**: `"cached":false, "fromCache":false`, 23.3s
(`analyze-out.txt`).

Driver: byte-identical to the recorded `../mcp.js` except `MCP_EXE` points at the worktree build
(the copy used is in this directory). Batches: the recorded `../calls-q.json` / `../calls-q2.json`,
unmodified, transcripts `q-out.txt` / `q2-out.txt`. One extra diagnostic batch was added for this
re-measure — `calls-diag.json` / `diag-out.txt` — to localize F4 and to size the F3 catalog; it
drove nothing the recorded calls depend on.

Defects fixed this run and judged here: **F1, F3, F4, F5**. F2 was fixed and re-measured in the
original record; everything else is unmeasured.

## Verdicts

| defect | bar | verdict |
|---|---|---|
| F1 | `startHere` holds no member of a type that does not declare it | **PASS** |
| F3 | `Pipeline:Queue:Drain` resolves with provenance; catalog well above 1; insight says what it cannot see | **PASS** |
| F4 | `seam(BuildCoordinator → IngestStage)` found:true through the `IJobQueue` port with a joined hop | **FAIL** — the port now bridges (proven below), but the recorded call still returns `found:false`: the walk dies one hop later, at a *different* missing-edge class |
| F4 (truthfulness half) | `seam(SourceUploadEndpoints → IngestStage)` stays honest | **PASS** — still `found:false` with the honest note, no fabricated path |
| F5 | `usages(JobRunner)` reports one call site once | **PASS** |

## F1 · PASS — startHere is real again

`overview.startHere` (`q-out.txt`, Q1):

```
Start here: ApiProblems, Course, Run, AppDbContext
```

The full 21-entry `startHere` list is `ApiProblems`, `Course`, `Run`, `AppDbContext`,
`Planner.Compute`, `BuildCoordinator.AdvanceAsync`, `JobRunner.RunAsync`, `ApiProblems.NotFound`,
`JobSettlement.FailAsync`, … — every member is declared by its receiver (spot-checked in target
source: `Planner.Compute` at `Pipeline/Workflow/Planning/Planner.cs:24`, `JobSettlement.FailAsync`
at `Pipeline/Workflow/JobSettlement.cs:60`). `AppDbContext.ConfigureAwait`, `.Where` and
`.IgnoreQueryFilters` are gone; grep over **all four transcripts** finds zero occurrences of
`ConfigureAwait`, `::Where` or `IgnoreQueryFilters` anywhere — including the Q2 trace
(`POST /source-uploads`, 29 steps), which in the original carried
`→ Member: SourceUploads.ConfigureAwait [approx]` hops and now carries none. The trace kept its DI
hop (`IObjectStore ◇ S3ObjectStore`, registration `StorageModule.cs:27`), its `[approx]` markers
and its declared `4 omitted`.

The invariant from the original record — *no node may be a member of a type that does not declare
it* — held everywhere this re-drive looked.

## F3 · PASS — the catalog learned the Options pattern

The recorded Q5 call (`q-out.txt`):

```json
{"key":"Pipeline:Queue:Drain","totalKeys":1,
 "keys":{"Pipeline:Queue:Drain":[{"filePath":"C:\\Code\\BookToCourse\\apps\\app\\Pipeline\\DependencyInjection.cs",
 "lineNumber":73,"patternType":"OptionsBinding","service":"Book2Course.Pipeline"}]}}
```

The key resolves, with provenance at exactly `Pipeline/DependencyInjection.cs:73` — the
`AddOptions<QueueDrainOptions>().BindConfiguration(...)` site the original drive quoted.
(`totalKeys` in a keyed response counts the *matched* keys; the original's "1 keys exist" was the
whole catalog.) The whole catalog now (`diag-out.txt`, D6, no key):

```
"totalKeys":14 — OTEL_EXPORTER_OTLP_ENDPOINT, Storage, Database, Admin, Authentication:Google,
WebPush, Review, Pipeline:Queue, Pipeline:Stages, Pipeline:Estimation, Pipeline:Queue:Drain,
Pipeline:Agent, Pipeline:SourceAttachments, Pipeline:SourceFetch
```

14 keys against the original's 1 — well above the bar. The `method` string and the insight headline
both now state the blind spot out loud — `config.missing-defaults` (`q2-out.txt`, stats):

> **Config without defaults: 11 consumed keys have no appsettings default (counts literal +
> Options-bound keys; computed keys are invisible here)**

Observation, not a verdict: the `Storage` key carries a second provenance row from
`C:\Code\BookToCourse\.conductor\tmp-q14\StorageModule.cs` — a stray temp copy inside the target
repo that the scan swept in. Harmless here, but the config scan does not confine itself to solution
sources. Worth a backlog note.

## F4 · FAIL on the stated bar — the port bridges, and the walk now dies one hop later

The recorded Q4b call (`q2-out.txt`) still returns the miss, in the same honest words:

```json
{"found":false, "direction":"none",
 "from":{"nodeId":"Type:Book2Course.Pipeline.Workflow.BuildCoordinator"},
 "to":{"nodeId":"Type:Book2Course.Pipeline.Stages.IngestStage"},
 "note":"'BuildCoordinator' and 'IngestStage' are unconnected: the walk exhausted everything
         reachable from each end within 8 hops and neither reached the other."}
```

That fails the bar as written. But the diagnosis moved. The transport-port join the fix promised
**exists and routes** — `seam(BuildCoordinator → JobRunner)` (`diag-out.txt`, D1) is `found:true`,
2 paths, and the crossing hop is exactly the shape the bar asked for:

```json
{"from":"BuildCoordinator.AdvanceAsync","to":"IJobQueue","kind":"Calls","resolution":"Semantic",
 "site":"...\\Pipeline\\Workflow\\BuildCoordinator.cs:27"},
{"from":"IJobQueue","to":"JobRunner.RunNextAsync","kind":"Consumes","resolution":"Join",
 "site":"...\\Pipeline\\Workflow\\IJobQueue.cs:16"}
```

`IJobQueue` is no longer a sink: `neighbors(IJobQueue, out)` (D3) shows the `Consumes` edge to
`JobRunner.RunNextAsync` (Join) beside the DI `Resolves` to `PostgresJobQueue`. The stats ledger
carries it: `Consumes: total 1, joined 1`.

What still breaks the recorded call is downstream and **different in kind**:
`neighbors(IngestStage, in)` (D4) is `count: 0` — *nothing in the graph lands on IngestStage at
all*. In the target, stages are registered by reflection —
`Pipeline/DependencyInjection.cs:206-214`, `AddStages()` scans the assembly for `IStage`
implementors and calls `services.AddSingleton(typeof(IStage), stage)` — and `JobRunner` dispatches
to them dynamically through `IStageRegistry`. No static edge reaches any stage type, so the walk
from the now-bridged queue exhausts honestly before `IngestStage`. This is a reflection-based
registration blind spot, not the handler-join cell; it was not in the fixed set and deserves its
own backlog entry. Until it lands, the F4 bar on this repo cannot pass end-to-end — the fix's own
commit (`b731281`) predicted the coupling in the other direction and this is its remaining half.

The truthfulness half of the bar holds: the recorded Q4
`seam(SourceUploadEndpoints → IngestStage)` (`q-out.txt`) still reports `found:false` with the
same honest note — correct, upload only stages the file — and no path was fabricated anywhere.

## F5 · PASS — one call site, once

`usages(JobRunner)` (`q2-out.txt`, Q2b) — was `count: 3`, three identical rows. Now:

```json
{"nodeId":"Type:Book2Course.Pipeline.Workflow.JobRunner","count":2,"usages":[
 {"caller":"Member:...QueueDrainService::TurnAsync","kind":"Calls",
  "provenance":"...\\Pipeline\\Workflow\\QueueDrainService.cs:95"},
 {"caller":"Type:...IJobQueue","kind":"Consumes","provenance":"...\\Pipeline\\Workflow\\JobRunner.cs:82"}]}
```

The `QueueDrainService.TurnAsync` call site appears exactly once, at `QueueDrainService.cs:95`.
The second row is not a duplicate: it is the new F4 port-bridge in-edge (`Consumes`, distinct
caller, distinct kind, distinct provenance) — visibly distinct rows, honest count. The tool's own
description now reads "One row per distinct call…".

## The honesty ledger moved

`stats` seam breakdown for `Calls` (`q2-out.txt` vs `../q2-out.txt`):

| | total | verified | joined | approx | approx share |
|---|---|---|---|---|---|
| before (DRIVE.md) | 1594 | 650 | 44 | 900 | **56%** |
| now | 932 | 609 | 44 | 279 | **30%** |

The graph itself shrank with F1 — 1315 → 1172 nodes, 1739 → 1078 edges — because the minted
undeclared members and their edges are gone; most of what vanished was approx noise (900 → 279),
while verified dropped only 41 (650 → 609). Plus the new `Consumes: 1 joined` row that did not
exist before. Fresh analyze wall time 23.3s against the original 44.7s cold.

Unchanged and consistent with the original record's F2 re-measure: `auth.anonymous` 6/39
(confidence 0.85), `web.auth-surface` 33 protected / 5 unannotated of 39.

## Unmeasured

F2 (measured in the original record, observed consistent here), and everything the original drive
did not question. The two observations above — reflection-registered stages leaving `IngestStage`
at in-degree 0, and the config scan sweeping `.conductor` temp copies — are new, unfixed, and
should be filed, not inferred fixed from this record.

## Reproducing

`node mcp.js ../calls-analyze.json analyze-out.txt` then `../calls-q.json` → `q-out.txt`,
`../calls-q2.json` → `q2-out.txt`, `calls-diag.json` → `diag-out.txt`, with `mcp.js` here pointing
`MCP_EXE` at `src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe` in the integration worktree.
