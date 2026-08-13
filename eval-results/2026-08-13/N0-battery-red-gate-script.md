# N0 phase gate RED — root cause and fix (session #3, 2026-08-13)

## Verdict

The red was **not** N0 work. It is a **gate-script defect that predates this program by a month**
(`Get-EngineStamp` landed 2026-07-16 in `1654a49`, T7.0). N0.1 / N0.2 / N0.3 stand as claimed — no
checkpoint was downgraded, because none of them caused this.

## Symptom the conductor saw

```
gate battery: FAIL (exit 1) in 69s   → retried → FAIL (exit 1) in 41s
```

with the captured output stopping dead one line into Step 3:

```
--- Step 2b: MCP QA gate (serial) ---
  PASS  MCP QA gate passed

--- Step 3: Eval expectation tests ---
```

No failing test, no assertion, no eval output. The conductor's capture keeps stdout, and the actual
error went to **stderr** — which is why the failure text was missing from `REPORT.md`.

## The stderr the capture dropped

Re-ran the identical command through `conductor bg` with stderr captured
(`.conductor/bg-logs/battery-repro-20260813-172431149.log`):

```
ForEach-Object : The term 'Get-FileHash' is not recognized as the name of a cmdlet, function,
script file, or operable program.
At C:\Code\DevContext2-desktop\eval\gates.ps1:199 char:84
    + CategoryInfo          : ObjectNotFound: (Get-FileHash:String) [ForEach-Object], CommandNotFoundException
```

`$ErrorActionPreference = "Stop"` turns that into a terminating error, so the battery exits 1 roughly
one second into Step 3 — which is exactly what the 69s/41s wall times imply (all of Steps 0–2b, then
nothing).

## Root cause

In **Windows PowerShell 5.1**, `Get-FileHash` is not a built-in cmdlet. Measured:

```
powershell -NoProfile -Command '(Get-Command Get-FileHash).CommandType; (Get-Command Get-FileHash).ModuleName'
Function
Microsoft.PowerShell.Utility
```

It is a **function exported by a module**, found via `PSModulePath` autoloading. Conductor launches the
gate from a **pwsh 7** parent, so the 5.1 child inherits a `PSModulePath` with PowerShell 7's module
directories **prepended**:

| launched from | `PSModulePath` begins with | `Get-FileHash` |
|---|---|---|
| interactive shell (foreground) | `…\WindowsPowerShell\Modules; …\system32\WindowsPowerShell\v1.0\Modules` | **visible** |
| `conductor bg` (pwsh 7 parent) | `…\Documents\PowerShell\Modules; …\Program Files\PowerShell\Modules; …\scoop\apps\pwsh\current\Modules; …` | **missing** |

5.1 autoloads *PowerShell 7's* `Microsoft.PowerShell.Utility`, which does not export `Get-FileHash` as a
function it can use — so the name resolves to nothing.

This never fired before because **`-Scope app` skips Step 3**. `Get-EngineStamp` is only reached by the
full form, and the conductor log confirms the full battery had *no passing run on record* in this
worktree (`gate battery: FAIL after retry (41s — no passing run of this gate on record)`).

## Fix — `eval/gates.ps1`

Hash through the .NET type, which needs no module resolution. Same algorithm, same uppercase-hex
format, same bytes:

```powershell
function Get-Sha256Hex([byte[]]$bytes) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('X2') }) -join '' }
    finally { $sha.Dispose() }
}
```

**Nothing was weakened.** No test deleted, no expectation relaxed, no step skipped, no golden
re-declared. The stamp still hashes the same six trees, the same eight extensions, in the same sorted
order, and Step 3 still runs the full 44-expectation cohort split over two hosts.

## Proof the fix is equivalent, not a workaround

A probe running the *verbatim* new `Get-EngineStamp`, executed in both environments:

```
# foreground (Get-FileHash available — equivalence check)
host   = 5.1.26100.8972
getfilehash-visible = True
dotnet = 4C0B4D9C7C5E84F494DDC23FEA617A976D7D5A71415CB69689DAC47A5D03DDF6
cmdlet = 4C0B4D9C7C5E84F494DDC23FEA617A976D7D5A71415CB69689DAC47A5D03DDF6
MATCH  = True
stamp  = 0E56E012F51D906DE625759A22B3C2E307A006F7C9FDFF97B9B277F525D94C76

# conductor bg (the environment that was failing)
host   = 5.1.26100.8972
getfilehash-visible = False
cmdlet = <unavailable in this host>
dotnet = 4C0B4D9C7C5E84F494DDC23FEA617A976D7D5A71415CB69689DAC47A5D03DDF6
stamp  = 0E56E012F51D906DE625759A22B3C2E307A006F7C9FDFF97B9B277F525D94C76
```

Byte-identical to `Get-FileHash` where `Get-FileHash` exists, and it still produces the identical stamp
where `Get-FileHash` does not. The probe script was deleted after measuring.

## Proof Step 3 now proceeds

`.conductor/bg-logs/battery-verify-20260813-172935867.log` — the line that had never been reached:

```
--- Step 3: Eval expectation tests ---
  split: A=21+other eval classes, B=24 repos, two test hosts
```

## Second red, found only because the first was fixed

With Step 3 unblocked the battery ran to the end and stopped at **Step 5 (app check)**:

```
FAIL src/app/ui/graph-canvas/graph-layout.spec.ts > graph-layout (D4.1 determinism + no-clip contract)
     > same input twice → identical geometry
Error: Test timed out in 5000ms.
  Test Files  1 failed | 20 passed (21)
       Tests  1 failed | 172 passed (173)
    Duration  89.94s
```

Measured, not assumed:

| run | wall | that test |
|---|---|---|
| file alone, idle machine | 8 passed, `tests 1.28s` | passes |
| full 173-test suite, idle machine | 21 files / 173 passed, `Duration 11.18s` | passes |
| full suite inside the battery, straight after the eval cohort + CLI matrix | `Duration 89.94s` | **5249ms — lost by 249ms** |

Root cause: `graph-layout.ts` loads elkjs lazily —

```ts
async function getElk(): Promise<ELK> {
  if (!elkInstance) {
    const mod = await import('elkjs/lib/elk.bundled.js');   // ~1.4 MB GWT-compiled bundle
```

so whichever `it` ran first in the process was billed the whole module load against vitest's
**5000ms default**. On an 8×-loaded machine that is not enough. The assertion was never the problem.

Fix (`32639c6`): pay the bootstrap once in a `beforeAll` that asserts nothing. `layoutGraph` returns
early on an empty node list, so the warm-up uses a one-node graph to actually reach ELK. **Every test
keeps the strict 5000ms default** — no timeout was raised on anything that measures something, no test
deleted or skipped, every assertion byte-for-byte unchanged.

## Full battery, post-fix — GREEN

`.conductor/bg-logs/battery-final-20260813-174142*.log`:

```
--- Step 0: Clear orphaned build-locking processes ---   PASS  Cleared 0 orphaned process(es)
--- Step 1: Build solution ---                           PASS  Build succeeded
--- Step 1a: Contract sweep (dead proto fields) ---      PASS  Contract sweep clean
--- Step 2: Fast unit tests ---                          PASS  Fast tests passed
--- Step 2b: MCP QA gate (serial) ---                    PASS  MCP QA gate passed
--- Step 3: Eval expectation tests ---
Passed!  - Failed: 0, Passed: 50, Skipped: 9, Total: 59, Duration: 2 m 57 s
Passed!  - Failed: 0, Passed: 27, Skipped: 1, Total: 28, Duration: 2 m 29 s
                                                         PASS  Eval tests passed (stamp written)
--- Step 4: CLI --strict matrix ---                      PASS  CLI matrix: all commands ran successfully
--- Step 4b: CLI query ops (T3.7) ---                    PASS  CLI query ops: entrypoints/stats/trace return graph JSON
--- Step 5: App check (pnpm check) ---                   PASS  pnpm check passed

GATE: PASS
```

Step 3 ran **uncached** (the full 44-expectation cohort, both hosts, 77 tests passed / 10 skipped) — the
skips are the harness printing the rows for repos that are not on this machine, which is its designed
and self-announcing behaviour.

## Filed, not fixed

`conductor bug #2` — the T7.0 eval stamp cache never actually hits. `Get-EngineStamp` recurses into
`bin/` and `obj/` and hashes `project.assets.json` / `*.deps.json`, which Step 1 rewrites on every run,
so the stamp never matches the one the last green run wrote. Two consecutive full batteries six minutes
apart with no engine source touched between them both re-ran the eval and neither printed
`Eval cached`. Not fixed here — out of this session's scope, and the failure mode is conservative
(re-run when unsure) rather than wrong.
