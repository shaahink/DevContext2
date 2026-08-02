# S5 / Batch D close verification -- battery THEN the full 47-pole matrix, sequentially.
# Sequential on purpose: gates.ps1 BUILDS, and the matrix drives the built CLI; running them at the
# same time races on the binary (the session-killer pre-empted in R2 sec 1).
# Detached (S3 lesson: the agent harness kills background commands at 10 min).
# ASCII only (PS 5.1 detached-run encoding gotcha).
$ErrorActionPreference = "Continue"
$RepoRoot = "C:\code\DevContext2"
$Stamp = "2026-07-28"
$GatesLog = Join-Path $RepoRoot "eval-results\$Stamp\gates-s5-batchD.txt"
$MatrixLog = Join-Path $RepoRoot "eval-results\$Stamp\matrix-s5-batchD.txt"
$OutRoot = Join-Path $RepoRoot "eval-results\$Stamp\graph-truth-s5"

"=== gates (full) started $(Get-Date -Format 'HH:mm:ss') ===" | Out-File $GatesLog -Encoding ascii
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot "eval\gates.ps1") 2>&1 |
    Out-File $GatesLog -Append -Encoding ascii
$GatesExit = $LASTEXITCODE
"=== gates finished $(Get-Date -Format 'HH:mm:ss') exit=$GatesExit ===" | Out-File $GatesLog -Append -Encoding ascii
"$GatesExit" | Out-File (Join-Path $RepoRoot "eval-results\$Stamp\gates-s5-batchD.exit.txt") -Encoding ascii

"=== matrix (47 poles, post-Batch-D) started $(Get-Date -Format 'HH:mm:ss') ===" | Out-File $MatrixLog -Encoding ascii
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot "eval\graph-truth.ps1") matrix -SkipBuild -OutRoot $OutRoot 2>&1 |
    Out-File $MatrixLog -Append -Encoding ascii
"=== matrix finished $(Get-Date -Format 'HH:mm:ss') exit=$LASTEXITCODE ===" | Out-File $MatrixLog -Append -Encoding ascii
