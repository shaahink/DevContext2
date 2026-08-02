# S5 close (Batch D + Batch E): full battery -> macro bench -> full 47-pole matrix, SEQUENTIALLY.
# Sequential is load-bearing three times over: gates BUILDS (racing the matrix's CLI), the bench needs a
# quiet machine to be comparable to PERF-2026-07-28-0429, and gates' Step 0 kills orphaned processes --
# which would kill a matrix run sharing the box.
# Detached (S3 lesson: the agent harness kills background commands at 10 min).
# ASCII only (PS 5.1 detached-run encoding gotcha).
$ErrorActionPreference = "Continue"
$RepoRoot = "C:\code\DevContext2"
$Stamp = "2026-07-28"
$Dir = Join-Path $RepoRoot "eval-results\$Stamp"
$GatesLog = Join-Path $Dir "gates-s5-close.txt"
$BenchLog = Join-Path $Dir "bench-s5-close.txt"
$MatrixLog = Join-Path $Dir "matrix-s5-close.txt"
$OutRoot = Join-Path $Dir "graph-truth-s5-close"

Set-Location $RepoRoot

"=== gates (full) started $(Get-Date -Format 'HH:mm:ss') ===" | Out-File $GatesLog -Encoding ascii
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot "eval\gates.ps1") 2>&1 |
    Out-File $GatesLog -Append -Encoding ascii
$GatesExit = $LASTEXITCODE
"=== gates finished $(Get-Date -Format 'HH:mm:ss') exit=$GatesExit ===" | Out-File $GatesLog -Append -Encoding ascii
"$GatesExit" | Out-File (Join-Path $Dir "gates-s5-close.exit.txt") -Encoding ascii

"=== bench started $(Get-Date -Format 'HH:mm:ss') ===" | Out-File $BenchLog -Encoding ascii
& dotnet run --project (Join-Path $RepoRoot "benchmarks\DevContext.Benchmarks") -c Release -- repos 2>&1 |
    Out-File $BenchLog -Append -Encoding ascii
"=== bench finished $(Get-Date -Format 'HH:mm:ss') exit=$LASTEXITCODE ===" | Out-File $BenchLog -Append -Encoding ascii

"=== matrix (47 poles, post-Batch-E, timeout 600s) started $(Get-Date -Format 'HH:mm:ss') ===" | Out-File $MatrixLog -Encoding ascii
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot "eval\graph-truth.ps1") matrix -SkipBuild -OutRoot $OutRoot -TimeoutSec 600 2>&1 |
    Out-File $MatrixLog -Append -Encoding ascii
"=== matrix finished $(Get-Date -Format 'HH:mm:ss') exit=$LASTEXITCODE ===" | Out-File $MatrixLog -Append -Encoding ascii
