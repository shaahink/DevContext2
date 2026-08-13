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

## Full-battery result

See `## Full battery, post-fix` appended below.
