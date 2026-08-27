# Re-measure after the residuals run — Book2Course, post #37/#38/#39

2026-08-27. The second dated addendum to `../DRIVE.md`, following `../remeasure-post-fix/REMEASURE.md`
(the F1/F3/F4/F5 re-measure that filed #37–#39). Same recorded call batches, re-driven against the
build that closes the residuals. Build under test: `fix/drive-residuals` @ `843d961`
(develop `07af4b6` + #37 scan-registration joins, #38 `.conductor` exclusion, #39 harness identity),
`dotnet build DevContext.slnx` 0w/0e. Target: `C:/Code/BookToCourse` @ `c14da997` — same HEAD as the
original drive, untouched, and `.conductor/tmp-q14/` still present (required: it is #38's live
repro). Analysis was **fresh** (24.5s, `analyze-out.txt`) — the exclusion-list change alone re-keys
the snapshot flavor.

Driver: byte-identical to `../remeasure-post-fix/mcp.js` except `MCP_EXE` points at this checkout's
build and the spawn is **endpoint-isolated** (`DEVCONTEXT_ENDPOINT=http://127.0.0.1:5391` — the #39
rule, applied to the measurement that lands it). Batches: the recorded `../calls-q.json` /
`../calls-q2.json` unmodified (transcripts `q-out.txt` / `q2-out.txt`), plus the previous
re-measure's own `../remeasure-post-fix/calls-diag.json` unmodified (`diag-out.txt`) so every
diagnostic is measured against the same questions that filed the defects.

## Verdicts

| defect | bar | verdict |
|---|---|---|
| #37 | recorded Q4b `seam(BuildCoordinator → IngestStage)` found:true with a joined hop; `neighbors(IngestStage, in)` > 0 | **PASS** |
| #37 (truthfulness half) | the joined hop says `Join`, never verified; `seam(SourceUploadEndpoints → IngestStage)` stays honest | **PASS** |
| #38 | the `Storage` key carries exactly ONE provenance row; catalog breadth kept | **PASS** |
| #39 | the QA drive spawns isolated, verifies server identity, and still clears its gates | **PASS** — 12/12, both gates |
| regressions | F1 / F3 / F5 bars and the F4 port bridge re-checked on the same transcripts | **NONE** |

With Q4b passing, **every bar the unseen-repo drive filed (#33–#39) now measures closed** on this
repo — the drive's Q-batteries run end-to-end with no fabricated hop anywhere.

## #37 · PASS — the reflection-registered stage has in-edges, and the seam routes

The recorded Q4b call (`q2-out.txt`), which the previous re-measure still failed:

```json
{"found":true,"direction":"forward","hops":3,"totalPaths":1,
 "paths":[[
  {"from":"BuildCoordinator.AdvanceAsync","to":"Planner","kind":"Calls","resolution":"Semantic",
   "site":"...\\Pipeline\\Workflow\\BuildCoordinator.cs:27"},
  {"from":"Planner.CollectInputs","to":"IStage","kind":"Calls","resolution":"Semantic",
   "site":"...\\Pipeline\\Workflow\\Planning\\Planner.cs:130"},
  {"from":"IStage","to":"IngestStage","kind":"Resolves","resolution":"Join",
   "site":"...\\Pipeline\\Stages\\IStage.cs:20"}]]}
```

The closing hop is exactly the fix's shape: interface → implementor, `Resolves`, **`Join`** — a
classification, never a verified call. The localizing diagnostic flipped with it — `neighbors(
IngestStage, direction:in)` (`diag-out.txt`, D4), the call that measured **0** when #37 was filed:

```json
{"count":1,"edges":[{"from":"Type:...IStage","to":"Type:...IngestStage","kind":"Resolves",
 "resolution":"Join","provenance":"C:\\Code\\BookToCourse\\apps\\app\\Pipeline\\DependencyInjection.cs:214"}]}
```

The provenance is the SCAN SITE — `AddStages()`'s `services.AddSingleton(typeof(IStage), stage)`
at `DependencyInjection.cs:214` — so an agent asking "why does this edge exist" is pointed at the
registration loop, not at a guess. D2 (`seam(JobRunner → IngestStage)`) moved from exhausted-honest
to `found:true`, 4 hops, 2 paths. The graph grew by exactly the candidate set: stats
(`q2-out.txt`) show **1078 → 1087 edges (+9), 1172 nodes unchanged, 41 entries unchanged** —
the scan's in-solution implementors, nothing else.

Truthfulness held on both halves. The joined hop reaches the wire saying `Join`; and the drive's
correctly-false first case (`q-out.txt`, Q4) is STILL false in the same honest words:

```json
{"found":false,"direction":"none",
 "note":"'SourceUploadEndpoints' and 'IngestStage' are unconnected: the walk exhausted everything
         reachable from each end within 8 hops and neither reached the other."}
```

Observation, not a verdict: a seam hop's `site` for a joined Resolves edge renders the interface's
declaration line (`IStage.cs:20`) while the edge's true provenance rides the neighbors surface —
the same rendering choice the F4 port-bridge hops made (`IJobQueue.cs:16` in the previous
re-measure's D1). Consistent, pre-existing, noted only so nobody re-discovers it as a surprise.

## #38 · PASS — one Storage key, one row, and the catalog kept its breadth

The full catalog (`diag-out.txt`, D6, no key): `totalKeys: 14` — identical breadth to the previous
re-measure. The `Storage` key now carries exactly **one** provenance row:

```
C:\Code\BookToCourse\apps\app\Api\Storage\StorageModule.cs:18
```

The `.conductor\tmp-q14\StorageModule.cs` row is gone — the temp copy never enters the file
inventory, so the DI extractor never sees its Options binding and the catalog never mints the
phantom row. The recorded Q5 (`q-out.txt`) still resolves `Pipeline:Queue:Drain` with provenance at
`Pipeline/DependencyInjection.cs:73`, unchanged.

## #39 · PASS — the QA drive is isolated, attributed, and still green

`node eval/mcp-qa/run.js` (dogfood repo): **12/12 passing, checkout gate PASS (2 calls, 1598 tok),
actionable gate PASS**. The drive now spawns its MCP with `probeEnv()` (own endpoint, 5279) and
refuses to measure a server whose `/health` it cannot attribute to this checkout's fresh build —
the same two defences the other four probes adopted in T1.4. The other half moved with it:
`ServerTestFactory.Dispose` kills only a `DevContext.Server` whose main module lives under THIS
checkout (walked to the `DevContext.slnx` marker), never the bare process name — so a concurrent
test dispose can no longer kill another checkout's (or this drive's isolated) server
mid-conversation. Measured incidentally during this session: a leaked server from an earlier test
run locked `bin/` and failed the build with MSB3027 — the exact orphan class the scoped sweep
still catches, verified by attributing and killing it by path before rebuilding.

## Regressions — none

- **F1**: grep over all four transcripts finds zero `ConfigureAwait` / `::Where` /
  `IgnoreQueryFilters`; `startHere` unchanged (`ApiProblems`, `Course`, `Run`, `AppDbContext`, …).
- **F3**: `Pipeline:Queue:Drain` resolves with provenance (Q5); catalog 14 keys (D6).
- **F4**: the port bridge still routes — D1 `seam(BuildCoordinator → JobRunner)` `found:true`,
  2 paths, crossing hop `IJobQueue —Consumes/Join→ JobRunner.RunNextAsync`.
- **F5**: `usages(JobRunner)` (Q2b) — 2 distinct callers, each reported once.
