# G5.2 -- watch the NEW gate fail on demand. A check nobody has seen go red is not a check.
# Temporarily raises gitversion's entryTargets.minWithTarget above what the pole can produce, runs
# graph-truth, and restores the file in a finally block. Expected: entry-target=FAIL, then PASS again.
# ASCII ONLY (PS 5.1 detached-run encoding gotcha -- same rule as eval/graph-truth.ps1).
$ErrorActionPreference = 'Stop'
$repo = 'C:\code\DevContext2'
$f    = Join-Path $repo 'eval\expectations\graph-truth\gitversion.json'
$out  = Join-Path $repo 'eval-results\2026-07-29\G5.2\graph-truth-canary'
$real = Join-Path $repo 'eval-results\2026-07-29\G5.2\graph-truth-after'
$gt   = Join-Path $repo 'eval\graph-truth.ps1'
$orig = Get-Content $f -Raw

function Show-Verdict($root, $label) {
    $v = Get-Content (Join-Path $root 'verdicts\GitVersion.json') -Raw | ConvertFrom-Json
    $et = $v.checks.'entry-target'
    Write-Host "### $label entry-target=$($et.verdict) : $($et.detail)"
    Write-Host "###   metrics: entries=$($et.metrics.entries) withTarget=$($et.metrics.withTarget) minWithTarget=$($et.metrics.minWithTarget)"
}

try {
    Write-Host '### CANARY: minWithTarget 4 -> 6 (more than the pole even has entries). Expect FAIL.'
    $bumped = $orig.Replace('"minWithTarget":  4', '"minWithTarget":  6')
    if ($bumped -eq $orig) { throw 'canary edit did not apply -- the token moved' }
    # WriteAllText with an explicit no-BOM encoder: PS 5.1's `Set-Content -Encoding utf8` writes a BOM,
    # which shows up as a spurious first-line diff on a file this script is supposed to leave untouched.
    [IO.File]::WriteAllText($f, $bumped, (New-Object Text.UTF8Encoding $false))
    & powershell -File $gt gitversion -SkipBuild -OutRoot $out
    Show-Verdict $out 'CANARY'
}
finally {
    [IO.File]::WriteAllText($f, $orig, (New-Object Text.UTF8Encoding $false))
    Write-Host '### restored:'
    Select-String -Path $f -Pattern 'minWithTarget' | ForEach-Object { $_.Line.Trim() }
}

Write-Host '### RE-RUN at the real floor of 4. Expect PASS.'
& powershell -File $gt gitversion -SkipBuild -OutRoot $real
Show-Verdict $real 'REAL'
Write-Host 'CANARYDONE'
