<#
G6.2 - RED-then-GREEN for loom-guards rule 8 ("the app may not derive a label from a node id").

A guard that has never been watched go red is a guard you are guessing about. This reverts ONLY the
app-side G6.2 changes (rule 8 itself stays in place), runs the guard, restores, and runs it again.

Uses `git stash push -- <paths>` rather than Copy-Item on purpose: Copy-Item preserves LastWriteTime,
which is what made a previous session's "green" leg silently re-run a stale binary (ledger trap, G6
s20). Nothing is compiled here - the guard reads source text - but the habit is the rule.

SAFETY: this repo carries three OLD stashes from other branches. The pop is gated on stash@{0}
actually carrying THIS script's message, so a failed push can never pop someone else's work.

ASCII ONLY. PowerShell 5.1 reads a .ps1 as ANSI; a UTF-8 em dash inside a double-quoted string
becomes a quote-like byte sequence and the file fails to PARSE (cost one run of this script).
#>
param([string]$Out = "eval-results/2026-07-29/G6/g62-red-proof.txt")

$ErrorActionPreference = 'Continue'
Set-Location C:\code\DevContext2

$stashMsg = "g62-red-proof"
$targets = @(
    'src/DevContext.App/src/app/core/format.ts',
    'src/DevContext.App/src/app/state/trace.store.ts',
    'src/DevContext.App/src/app/features/pages/workbench-page.ts',
    'src/DevContext.App/src/app/features/explorer/stage.ts',
    'src/DevContext.App/src/app/features/peek/node-peek.ts',
    'src/DevContext.App/src/app/features/node-card/node-card.ts',
    'src/DevContext.App/src/app/ui/graph-canvas/graph-canvas.ts'
)

$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

function Rule8Hits($output) {
    return @($output | Select-String -Pattern 'BANNED: .*node id|BANNED: .*node-id')
}

Emit "G6.2 RED-THEN-GREEN - loom-guards rule 8"
Emit "files under test: $($targets.Count)"
Emit ""

$redCount = -1
$mine = $false
try {
    Emit "--- git stash push (revert the app-side fix, keep rule 8) ---"
    $push = (& git stash push -m $stashMsg -- $targets 2>&1) -join "`n"
    Emit $push
    $top = (& git stash list 2>&1 | Select-Object -First 1) -join ""
    $mine = $top -match [regex]::Escape($stashMsg)
    Emit "stash@{0} is mine: $mine"

    if ($mine) {
        Emit ""
        Emit "=== LEG 1 - PRE-FIX SOURCES (expect BANNED lines) ==="
        $red = & powershell -NoProfile -File scripts/loom-guards.ps1 2>&1
        $redHits = Rule8Hits $red
        foreach ($l in $redHits) { Emit ("  " + $l.Line.Trim()) }
        $redCount = $redHits.Count
        Emit "  rule-8 violations: $redCount"
    }
}
finally {
    if ($mine) {
        Emit ""
        Emit "--- git stash pop (restore the fix) ---"
        Emit ((& git stash pop 2>&1) -join "`n")
    }
}

Emit ""
Emit "=== LEG 2 - FIXED SOURCES (expect zero) ==="
$green = & powershell -NoProfile -File scripts/loom-guards.ps1 2>&1
$greenHits = Rule8Hits $green
foreach ($l in $greenHits) { Emit ("  " + $l.Line.Trim()) }
Emit "  rule-8 violations: $($greenHits.Count)"
Emit ""
foreach ($l in ($green | Select-String -Pattern 'PASSED|FAILED')) { Emit ("  " + $l.Line.Trim()) }

Emit ""
if ($redCount -lt 0) { $verdict = "DID NOT RUN - the stash did not take" }
elseif ($redCount -eq 0) { $verdict = "VACUOUS - the guard did NOT go red on the pre-fix code" }
elseif ($greenHits.Count -gt 0) { $verdict = "STILL RED after the fix" }
else { $verdict = "RED THEN GREEN - the guard catches the pre-fix code and passes the fixed code" }
Emit "VERDICT: $verdict"

$dir = Split-Path -Parent $Out
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
$lines | Set-Content -Path $Out -Encoding utf8
Write-Host "written: $Out"
