# G5 / session 17 — the `fast-engine` red was a harness abort, and the harness is now fixed

**Date:** 2026-07-29 · **Branch:** `feat/graph-v2` · **HEAD at diagnosis:** `a76ffa7`

Conductor's independent battery after session 16 returned:

```
### Gate `fast-engine` FAILED (exit 1, 53s)
  PASS  Cleared 0 orphaned process(es)
  PASS  Build succeeded
  PASS  Contract sweep clean (every response field read or allow-listed with a reason)
--- Step 2: Fast unit tests ---            <-- output ends here
```

No test output. No failing test name. No `GATE: FAIL` line. **That shape is not something
`eval/gates.ps1` can produce**, and the tests underneath were green the whole time.

---

## 1. The red does not reproduce — same command, same tree

`git diff` on `src/` is empty; `HEAD` is `a76ffa7`. The byte-identical command conductor ran:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File eval\gates.ps1 -SkipEval -Scope engine
```

→ `GATE: PASS (ENGINE scope - app check skipped; not a merge gate)`, with
`PASS  Fast tests passed` and `PASS  MCP QA gate passed`.

Raw log: `repro-01-prefix-gate-passes-unchanged-tree.txt`.

A non-reproducing red is normally where a session declares "flake" and moves on. That is the move
this evidence file exists to avoid.

## 2. `exit 1` is not a code Step 2 can return

Every exit path in `eval/gates.ps1`:

| exit | step |
|---|---|
| 1 | **step 1, build failure only** — and it prints the build log + `GATE: FAIL (step 1 - build)` |
| 2 | steps 1a / 2 / 2b — each prints its captured output first |
| 3 / 4 / 5 | eval / CLI matrix / app check |
| 0 | pass |

A real step-2 test failure exits **2**, and `Write-Host $testResult` runs *before* it. The observed
run printed `PASS Build succeeded`, then the Step 2 banner, then nothing, and exited **1**. No path
in the script does that — so the script did not choose that exit. It was killed mid-statement.

## 3. The mechanism, reproduced in six lines

`probe-eap-abort.ps1.txt` — a script with `$ErrorActionPreference = "Stop"` (gates.ps1:60) running a
native command under `2>&1` where that command writes **one** stderr line and **exits 0**:

```
--- Step 2: Fast unit tests ---
<exit code 1>
```

The `$LASTEXITCODE` check line never runs. Windows PowerShell 5.1 (measured: `5.1.26100.8972`)
wraps every native stderr line in a `NativeCommandError` `ErrorRecord`; under `EAP=Stop` that record
is **terminating**. The native command in the probe **exits 0** — so the suite can be entirely green
and the gate still reports FAIL, with the diagnostic going to a stderr stream nobody kept.

**It cuts both ways, and the second direction is worse.** On a genuinely red suite the same abort
fires *before* `Write-Host $testResult`, so the failing test names are discarded too. This gate could
not print why it failed in either direction.

## 4. It had already happened once, and the tracker still carries it as evidence

`run.db` `gates` rows, every `fast-engine` failure this run:

| row | session | exit | tail ends at |
|---|---|---|---|
| 3 | s2 (G1) | 1 | `--- Step 2: Fast unit tests ---` — **same silent abort** |
| 19 / 21 | s8 / s9 | -1073741502 | `0xC0000142`, the known OS-spawn failure (different bug) |
| 23 | s11 (G3) | 1 | full build log + `GATE: FAIL (step 1 - build)` — **a real red, correctly reported** |
| 39 | s16 (G5) | 1 | `--- Step 2: Fast unit tests ---` — this one |

Row 23 is the control: `exit 1` *with* output is a true build failure. Rows 3 and 39 are
byte-identical truncations at the same banner. Row 3 is what `GRAPH-V2-START.md` still records as
checkpoint **G1.2**'s evidence (`fast-engine:FAIL · guards:OK`) — a red that was never a red.

## 5. The fix — and why it strengthens the gate rather than relaxing it

`eval/gates.ps1:442` **already carried this exact workaround**, inline, for `pnpm check`:

```powershell
# PS 5.1: native stderr under 2>&1 + EAP=Stop can throw mid-stream — relax around the call.
```

So the hazard was diagnosed in this repo once, patched at the single site where it bit, and the
other **eleven** native captures were left exposed. The fix promotes that in-tree, already-blessed
remedy to a shared helper and applies it everywhere:

```powershell
function Invoke-NativeCapture {
    param([Parameter(Mandatory)][scriptblock]$Command)
    $oldEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { & $Command 2>&1 } finally { $ErrorActionPreference = $oldEap }
}
```

- `eval/gates.ps1` — 12 native captures (build, contract sweep, fast tests, MCP QA, both eval
  paths, CLI matrix, 4× query ops, app check) now route through it. No raw native `2>&1` remains.
- `scripts/loom-guards.ps1:102` — the truth-gate `dotnet test` had the identical hole under its own
  `EAP='Stop'`; scoped inline (one capture, no duplicated helper).

**This is not a softened bar.** Nothing was deleted, skipped, relaxed, or re-declared. The
relaxation is scoped to the native call, restored in a `finally`, and its entire effect is that the
`$LASTEXITCODE` check *now actually runs*. Verified in `probe-helper-verification.ps1.txt`:

| probe | native command | result |
|---|---|---|
| A | stderr + **exit 0** | no abort; check reached; `LASTEXITCODE=0`; output captured → passes, correctly |
| B | stderr + **exit 2** | no abort; check reached; `LASTEXITCODE=2`; **full output preserved** → fails, correctly, and now legibly |
| C | after the call | `ErrorActionPreference=Stop` restored; a cmdlet error is still terminating |

Probe B is the one that matters: a real failure still fails, and it keeps the diagnostic it used to
throw away.

## 6. What is NOT known, and why it cannot be recovered

The specific stderr line `dotnet test` wrote at 13:27:46 is **gone**. Conductor's `gates` row stores
only the stdout `tail`; the terminating error went to stderr and nothing kept it. Filed as a bug so
the next aborted gate leaves a trace. The mechanism is proven independently of that line — probe A
shows a single stderr byte is sufficient, regardless of its content.

## 7. Verification on the fixed harness

`verify.ps1` ran both fast gates end to end and captured each exit code explicitly:

- `verify-fast-engine.txt` / `verify-fast-engine.exit.txt`
- `verify-guards.txt` / `verify-guards.exit.txt`
