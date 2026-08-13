# Owner gate-harness chore — `eval/gates.ps1` (evidence)

Injected into session #7 (killed by a 429 before it could apply anything) and carried into session
#13's queued instructions. **Verified unapplied before doing any of it**: the file had no `trap`, no
`node_modules` preflight, and the banner order was still `0, 1, 1a, 2, 2b, 3, 4, 4b, 5`.

## 1. An ABORT is no longer read as a suite verdict

Under `$ErrorActionPreference = 'Stop'`, any terminating error *outside* `Invoke-NativeCapture` — a
missing tool, a vanished path, a typo in a step — unwound the script with no `GATE:` line at all.
Conductor then had to guess from a truncated log whether the battery failed or the host died.

A script-scope `trap` now names the error and its file:line and exits **9**, a code no step uses.

**Measured, not assumed** (`.conductor/tmp-trap-probe.ps1`, a probe carrying the same trap):

| input | exit | meaning |
| --- | --- | --- |
| terminating error (`Get-Content` on a missing path) | **9** | trap fires, error named |
| a step's own `exit 3` | **3** | `exit` is flow control, NOT a terminating error — the trap does not hijack it |
| native tool exiting 7 inside a capture | **0** | unchanged; the step's own `$LASTEXITCODE` check still owns the verdict |

That middle row is the one that mattered: had the trap intercepted `exit`, every real gate failure
would have been relabelled 9 and the battery would have stopped being able to say what broke.

## 2. Step 5 preflight — a missing install stops reading as a broken app

A worktree that never had `pnpm install` run in it fails `pnpm check` with a module-resolution stack
trace that reads like a broken app. Every fresh worktree this program makes starts that way, and one
was hit this session (the `corepack/dist/pnpm.js` MODULE_NOT_FOUND in
`.conductor/bg-logs/n22-appcheck-20260813-204830948.log`). Step 5 now checks for `node_modules`
first and fails with the one command that fixes it, labelled a **SETUP** failure. `Test-Path` is
`True` in this worktree, so the guard is correctly silent here.

## 3. Fail-fast order: 0, 1, 1a, 5, 2, 2b, 4, 4b, 3

Cheapest and most-likely-to-break first; the ~11-minute eval suite last. Before this, an app-only
typo waited out the whole engine cohort to be told, and a red eval hid whatever else was red behind
it. Step NUMBERS are stable names — a step keeps its number and its exit code forever, so
`GATE: FAIL (step 4)` means the same thing across the program's history — while the ORDER is a
scheduling decision. Documented in the script's `.DESCRIPTION`.

Executed as a pure block move (`$A + $F + $B + $D + $C + $E + $G`) with three guards: markers
asserted in order before the move, line count asserted identical after it, and — the one that
mattered — the variables assigned inside the Step 3 block were checked against every variable read
by Steps 4/4b/5 first. **No overlap**, so moving Step 3 after them cannot starve anything.

Verified:

```
PARSE OK                                   # [Parser]::ParseFile, 0 errors
Step 0: Clear orphaned build-locking processes
Step 1: Build solution
Step 1a: Contract sweep (dead proto fields)
Step 5: App check (pnpm check)             # + its -Scope engine skip banner
Steps 2-4b: engine suites - SKIPPED (-Scope app)
Step 2: Fast unit tests
Step 2b: MCP QA gate (serial)
Step 4: CLI --strict matrix
Step 4b: CLI query ops (T3.7)
Step 3: Eval expectation tests             # + its -SkipEval skip banner
```

**Not run here**: the battery itself, in any scope — this session is under a standing instruction not
to, and Conductor runs the full form immediately after it exits. That run is the live proof of this
change; the checks above are what can honestly be claimed without it.
