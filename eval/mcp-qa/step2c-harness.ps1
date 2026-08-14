<#
.SYNOPSIS
    Runs gates.ps1's Step 2c block, VERBATIM, against any repo root.
.DESCRIPTION
    T1.4. The battery is 10-15 minutes and Conductor runs it after a session exits, so a session
    that wants to prove one new step works cannot afford to run the whole thing twice - and a step
    proven by reading it is not proven at all.

    This harness EXTRACTS the text of Step 2c from eval/gates.ps1 (between its banner comment and
    the Step 3 banner) and executes that text unchanged, with the same $repoRoot / $Scope / helper
    contract the real script gives it. Same text, same exit codes: `exit 2` on either probe failing,
    fallthrough on green. Point -RepoRoot at a pre-fix worktree to prove the step goes RED, at this
    checkout to prove it goes green.

    It is a proof tool, not a gate. The gate is Step 2c inside gates.ps1.
.EXAMPLE
    powershell -File eval/mcp-qa/step2c-harness.ps1 -RepoRoot C:/Code/DevContext2-prefix-t1
#>
param(
    [Parameter(Mandatory)][string]$RepoRoot,
    [string]$GatesScript
)

$ErrorActionPreference = "Stop"
if (-not $GatesScript) { $GatesScript = Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) "eval\gates.ps1" }

# The contract Step 2c is written against, copied from gates.ps1 (same behaviour, same names).
$exitCode = 0
function Write-Step { param([string]$Text) Write-Host ""; Write-Host "--- $Text ---" -ForegroundColor Cyan }
function Write-Pass { param([string]$Text) Write-Host "  PASS  $Text" -ForegroundColor Green }
function Write-Fail { param([string]$Text, [int]$Step) Write-Host "  FAIL  $Text" -ForegroundColor Red; $script:exitCode = $Step }
function Invoke-NativeCapture {
    param([Parameter(Mandatory)][scriptblock]$Command)
    $oldEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { & $Command 2>&1 } finally { $ErrorActionPreference = $oldEap }
}

$repoRoot = (Resolve-Path $RepoRoot).Path
$lines = Get-Content $GatesScript
$start = ($lines | Select-String -Pattern '^# Step 2c: Wire-truth gate' | Select-Object -First 1).LineNumber
$stop = ($lines | Select-String -Pattern '^# Step 3: Eval tests\.' | Select-Object -First 1).LineNumber
if (-not $start -or -not $stop) { throw "Could not find the Step 2c block in $GatesScript" }
$block = ($lines[($start - 1)..($stop - 2)]) -join "`n"

Write-Host "harness: gates.ps1 lines $start..$($stop - 1) against repoRoot=$repoRoot" -ForegroundColor Gray
& ([scriptblock]::Create($block))
Write-Host ""
Write-Host "step 2c fell through (no exit) - PASS path, harness exitCode=$exitCode" -ForegroundColor Green
exit $exitCode
