# T1 phase-gate RED — root cause and fix (session #4, 2026-08-13)

The T1 phase gate ran the full battery twice and both runs came back **exit 1 in 83–85s**, with
the transcript stopping dead at the `--- Step 3: Eval expectation tests ---` banner and nothing
after it. Every measurement underneath was green, including all thirteen bars of the wire-truth
step T1.4 had just added.

## 1. The failure was NOT a step verdict

`eval/gates.ps1` gives every step its own exit code — step 3 fails with `exit 3`, not `exit 1`.
Exit 1 with no diagnostic is a PowerShell process that died on an unhandled terminating error.
Two facts pinned it inside step 3 before the step ran anything:

| Fact | Measurement |
|---|---|
| step 2c finished, step 3 started | `%TEMP%\devcontext-gate-wire-truth\*` last write **18:59:39**; the battery exited **18:59:42** |
| the two eval test hosts never launched | `%TEMP%\gates-eval-A.log` LastWriteTime **18:45** — an earlier, agent-launched run |

So the abort happened in the ~3 seconds between the step-3 banner and the first `Start-Process`,
which is `Get-EngineStamp`.

## 2. Reproduced

`pre-fix-battery-RED.log` — the full battery re-run in the same environment conductor uses
(launched through `conductor bg`, so it inherits conductor's process environment):

```
--- Step 3: Eval expectation tests ---
ForEach-Object : The term 'Get-FileHash' is not recognized as the name of a cmdlet, function,
script file, or operable program.
At C:\Code\DevContext2-engine\eval\gates.ps1:273 char:84
EXITCODE=1
```

## 3. Root cause

**In Windows PowerShell 5.1 `Get-FileHash` is not a binary cmdlet — it is a script function
inside the 5.1 copy of `Microsoft.PowerShell.Utility`.** PowerShell 7 ships its own module of the
same name, and on this machine PSModulePath puts PS7's directory *ahead* of System32's, so 5.1
binds the PS7 manifest — which exports none of the 5.1 script functions.

`psmodulepath-probe.log`, measured inside a conductor-spawned `powershell -NoProfile` child:

```
PSVersion=5.1.26100.8972
PSModulePath=...\OneDrive\Documents\PowerShell\Modules;C:\Program Files\PowerShell\Modules;
             c:\users\shahi\scoop\apps\pwsh\current\Modules;   <-- PS7, and it has its own
             ...;C:\WINDOWS\system32\WindowsPowerShell\v1.0\Modules;...   <-- 5.1's, behind it
GetFileHash=MISSING
System32ModulesOnPath=True          <-- present, but too late to win
```

Every *binary* cmdlet (`Get-ChildItem`, `Write-Host`, `ConvertFrom-Json`) still resolves, which is
why the first 200 lines of the battery look perfectly healthy.

**Why it only bit now.** Every earlier conductor gate in this run used `-SkipEval` or `-Scope`, so
18:58 was the first *full-form* battery conductor itself launched. A battery launched by an agent
inherits a clean environment and passes — session #3's own full run at 18:42 got through step 3.
That is a gate lying about the machine that actually runs it, not a T1 regression: none of
T1.1–T1.4's bars are involved, and all of them are green in both RED runs.

## 4. Fix (`fa7d4b6`)

`Get-EngineStamp` hashes with `[System.Security.Cryptography.SHA256]`, which is always available.
Plus a script-level `trap` so this whole class is never silent again: it names the exception type,
message and line, and exits **9** — a code no step uses, so an abort can never be read as a verdict.

## 5. The fix is proven equivalent, not merely present

`stamp-equivalence-probe.ps1` extracts the **real** `Get-EngineStamp` text out of `gates.ps1` with
the PowerShell AST (`Parser::ParseFile` → `FunctionDefinitionAst` → `Invoke-Expression Extent.Text`),
so there is no copied body that can drift, and runs it beside the old `Get-FileHash` form in one
process over the same tree:

```
GetFileHashAvailable=True
NEW=526E7B59C07978DC3BF40AB8AE77E75A3D18D587B0F5CF8612D3ED4525ACD22E
OLD=526E7B59C07978DC3BF40AB8AE77E75A3D18D587B0F5CF8612D3ED4525ACD22E
IDENTICAL=True
```

Same digest, same case — an eval stamp written before this change still transfers, so nothing is
invalidated and no eval verdict is re-declared.

`post-fix-stamp-in-broken-shell.log` — the same function in the shell that could not run the old one:

```
PSVersion=5.1.26100.8972
GetFileHashAvailable=False
STAMP=...            (1.9s)
```

## 6. Nothing was weakened

No test deleted, skipped or filtered; no expectation relaxed; no golden re-declared; no gate
command softened. The battery's step list, exit codes and bars are unchanged — the only difference
is that step 3 now reaches the tests it was always supposed to run, from any shell.

## Files

| File | What it is |
|---|---|
| `pre-fix-battery-RED.log` | the reproduction: full battery, conductor environment, dies at step 3 |
| `psmodulepath-probe.log` | the mechanism: `Get-FileHash=MISSING` in a conductor-spawned 5.1 |
| `stamp-equivalence-probe.ps1` | the equivalence proof (run it anywhere `Get-FileHash` works) |
| `post-fix-stamp-in-broken-shell.log` | the new form running where the old one could not |
| `post-fix-battery.log` | the full battery after the fix |
