# G5 s18 — PROVOCATION for flake B. Watch it go red on demand before believing the mechanism.
#
# Claim under test: AnalyzeCacheTruthTests' hit/miss assertions are coupled to the repo's working
# tree, because SnapshotCacheService.ComputeKeys appends GitHeadReader.ReadDirtyFingerprint, which
# hashes the mtime+length of every file `git status --porcelain -uall` lists — repo-wide, untracked
# included — and tests/fixtures/* live inside this repo.
#
# Experiment: run the two cache-path tests N times while a churn loop rewrites ONE untracked file in
# the repo every 250ms. That file has nothing to do with the fixture. Test logs go to $env:TEMP so the
# harness itself is not a second churn source.
#   near-100% failure -> mechanism proven, and this is reachable during the real battery (measured:
#                        eval-results/<date>/mcp-qa.md was rewritten at 13:39:31 by s17's own fast gate).
#   ~5% failure       -> the churn is irrelevant and the mechanism is something else.
param([int]$Iterations = 8, [switch]$NoChurn)

$ErrorActionPreference = 'Continue'
$root = 'C:\code\DevContext2'
$logDir = Join-Path $env:TEMP ('g5s18-provoke-' + [Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$churnFile = Join-Path $root 'eval-results\2026-07-29\G5-s18\churn.tmp'

$job = $null
if (-not $NoChurn) {
    $job = Start-Job -ScriptBlock {
        param($f)
        for ($n = 0; $n -lt 100000; $n++) {
            Set-Content -LiteralPath $f -Value ("churn $n") -ErrorAction SilentlyContinue
            Start-Sleep -Milliseconds 250
        }
    } -ArgumentList $churnFile
    "CHURN JOB $($job.Id) rewriting $churnFile every 250ms"
} else {
    "NO CHURN (control arm)"
}

$fails = 0
for ($i = 1; $i -le $Iterations; $i++) {
    $log = Join-Path $logDir "provoke-$i.txt"
    & dotnet test "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" --no-build --nologo `
        --filter "FullyQualifiedName~AnalyzeCacheTruthTests" > $log 2>&1
    $code = $LASTEXITCODE
    if ($code -ne 0) {
        $fails++
        $names = (Select-String -Path $log -Pattern '\[FAIL\]' | ForEach-Object { ($_.Line -split '\s+')[-2] }) -join ', '
        "PROVOKE ITER $i EXIT $code FAIL: $names"
    } else {
        "PROVOKE ITER $i EXIT 0 pass"
    }
}
if ($job) { Stop-Job $job -ErrorAction SilentlyContinue; Remove-Job $job -Force -ErrorAction SilentlyContinue }
Remove-Item -LiteralPath $churnFile -ErrorAction SilentlyContinue
"PROVOKE TOTAL FAILURES: $fails / $Iterations   (logs: $logDir)"
