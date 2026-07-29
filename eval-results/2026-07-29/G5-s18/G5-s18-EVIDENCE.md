# G5 s18 — the `fast-engine` red was a **flaky gate**, and it is now deterministic

**Verdict: fixed at the root. `fast-engine` = 0, `guards` = 0. No test, expectation, golden or gate bar
was weakened; one gate got STRICTER (loom-guards Rule 7).** No checkpoint is claimed — G5.2 is untouched.

---

## 1. The red

Conductor's own battery, after s17:

```
--- Step 2: Fast unit tests ---
DevContext.Server.Tests.AnalyzeCacheTruthTests.A_rehydrate_reports_the_originals_instant_not_its_own [FAIL]
System.IO.DirectoryNotFoundException : Could not find a part of the path
  'C:\Users\shahi\AppData\Local\Temp\devcontext-cachetruth-tests\9b9e2174dea7400188465d4c6c02369e'
  at ...AnalyzeCacheTruthTests.BackdatePersistedSnapshot(...) AnalyzeCacheTruthTests.cs:line 197
```

The class's own private cache root did not exist — so the analysis it had just run had persisted its
snapshot somewhere else.

## 2. It is flaky, at ~1 run in 3

`repro-race.ps1` — build once, then run the three cache-touching classes `--no-build` 15 times.

| iteration | 1 | 2 | 3 | 4 | **5** | 6 | 7 | **8** | **9** | 10 | **11** | 12 | **13** | 14 | 15 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| exit | 0 | 0 | 0 | 0 | **1** | 0 | 0 | **1** | **1** | 0 | **1** | 0 | **1** | 0 | 0 |

**5 failures / 15**, every one the identical exception with a *different* GUID each time
(`run-5.txt`, `run-8.txt`, `run-9.txt`, `run-11.txt`, `run-13.txt`; summary in `loop-A-repro-multiclass.txt`).

This matters beyond the fix: **`fast-engine` has been a coin flip since G3.3 added this test.** s17 saw it
green and s18 saw it red on the same tree, and both observations were honest. A single green `fast-engine`
run inside that window is weaker evidence than it looked.

## 3. Root cause

`SnapshotCacheRoot.DefaultPath` (`SnapshotCacheService.cs:58`) reads the **process-wide** env var
`DEVCONTEXT_CACHE_ROOT`. Five xUnit collections in `DevContext.Server.Tests` were each writing it in their
constructor and restoring it on dispose, and xUnit v2 runs collections **concurrently in one process**:

| owner | writes | restores |
|---|---|---|
| `ServerTestFactory` ×3 instances (`AnalyzeFlowTests`, `SessionManagementTests`, `NeighborKindEndToEndTests`) | ctor | **`null`**, not the prior value |
| `HostReleaseTests` | ctor | prior |
| `AnalyzeCacheTruthTests` | ctor | prior |

`EngineRunner` resolved the root once, when its `SnapshotCacheService` field was constructed. A neighbour
writing the variable in the window between the test class's constructor and that line sent the analysis into
the neighbour's root — and the class's own root was then never created at all.

The `null` restore is the same defect pointing at the developer's **real** `%LOCALAPPDATA%\DevContext\cache`,
which would also destroy the "cold cache" precondition this test class documents at its top.

## 4. The fix — hand the root to the host instead of shouting it at the process

| file | change |
|---|---|
| `src/DevContext.Server/Sessions/ServerOptions.cs` | new `SnapshotCacheRoot` (null = old default; the env override still works for real users and the CLI) |
| `src/DevContext.Server/Sessions/EngineRunner.cs` | takes `ServerOptions`; `new SnapshotCacheService(options.SnapshotCacheRoot)` |
| `tests/.../ServerTestFactory.cs` | overrides `ServerOptions` in `ConfigureWebHost` via `prior with { ... }`; both env writes deleted |
| `tests/.../HostReleaseTests.cs`, `AnalyzeCacheTruthTests.cs` | pass their root to their own `EngineRunner`; no env at all |
| `scripts/loom-guards.ps1` | **Rule 7**: a test that writes `DEVCONTEXT_CACHE_ROOT` fails the guards gate |

`Program.cs` is untouched — `ServerOptions` was already a registered singleton.

One line of test legibility (not a bar change): `BackdatePersistedSnapshot` now treats a missing root as zero
snapshots and lets `Assert.Single` report it, instead of throwing `DirectoryNotFoundException`. Same bar,
named failure — the opaque form is what cost two sessions of diagnosis.

## 5. Verification

| what | result | artifact |
|---|---|---|
| Full `DevContext.Server.Tests` (104 tests, all five collections) ×25, logs written **outside** the repo | **0 failures / 25** | `loop-D-verify-25x-afterfix.txt` |
| `eval/gates.ps1 -SkipEval -Scope engine` | **PASS, exit 0** | `verify-gates-OUTPUT.txt`, `verify-fast-engine.exit.txt` |
| `scripts/loom-guards.ps1` | **PASS, exit 0** | `verify-gates-OUTPUT.txt`, `verify-guards.exit.txt` |
| `dotnet build DevContext.slnx` | 0 warnings / 0 errors | above |
| **Rule 7 watched going red on demand** | armed=1, disarmed=0 → "THE GUARD REALLY FIRES" | `canary-rule7-OUTPUT.txt` |

## 6. Three experiments that refuted my own theories — kept deliberately

Each of these looked like confirmation and was not. They are in the folder so nobody repeats them.

1. **`control-serialized.ps1` — VOID, not a refutation.** `dotnet test -- xunit.parallelizeTestCollections=false`
   "still failed 1/10", which reads as killing the contention theory. The setting never reached the runner:
   every supposedly serialised iteration reported the *same* 4–6 s duration as the parallel ones. A
   `-- key=value` passthrough is silent when ignored. The experiment that actually worked needed no
   passthrough — narrow `--filter` to ONE class, since one class is one collection.
2. **`discriminator-single-class.ps1` — the real control.** That class alone: **0/20** of flake A. Contention
   confirmed; the theory finally had an experiment under it rather than a code read.
3. **`provoke-flakeB*.ps1` — mechanism true, causation false.** The solo run surfaced a *second*, rarer red
   (`All_three_paths_report_themselves`, `third.Cached` false, `solo-12.txt`). I attributed it to the cache
   key tracking the whole working tree. `probe-versionkey.ps1` **proves that half**: creating one untracked
   file elsewhere in the repo moved the fixture's key from `-dirty-FC670061FC38EC7B` to `-dirty-F8AA6627EFE09FE5`.
   But driving the tests while a churn process rewrote an untracked repo file every 200 ms — churn *proven*
   to have run (`churned=True` each iteration) — gave **0 failures in 14 iterations**. Mechanism real,
   causal link absent. Filed as **bug #16**, open, with the next suspect named (`SaveAsync` swallows failures
   into a result `EngineRunner` only logs, to a logger factory the tests give no sinks).

Harness rule learned the hard way: **verification loops for this class must write their logs outside the
repo.** `eval-results/` is untracked-but-not-ignored, so a growing log inside it is itself a working-tree
change; `.conductor/bg-logs/` is ignored and is safe.
