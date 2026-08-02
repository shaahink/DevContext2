# S5 -- full 47-pole matrix on the POST-BATCH-D binary (commit 4a90b98), no build.
# -SkipBuild is load-bearing: Batch E is being written in the working tree while this runs, and a
# build here would silently measure D+E and call it D.
# Timeout 600s per pole: the three giants (HotChocolate, bitwarden-server, PowerToys) are being asked
# a yes/no question -- does analysis finish in a usable budget - and 28 minutes already answered no.
$ErrorActionPreference = "Continue"
$RepoRoot = "C:\code\DevContext2"
$Log = Join-Path $RepoRoot "eval-results\2026-07-28\matrix-s5-batchD.txt"
$OutRoot = Join-Path $RepoRoot "eval-results\2026-07-28\graph-truth-s5"

"=== matrix (47 poles, post-Batch-D, timeout 600s) started $(Get-Date -Format 'HH:mm:ss') ===" | Out-File $Log -Encoding ascii
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot "eval\graph-truth.ps1") matrix -SkipBuild -OutRoot $OutRoot -TimeoutSec 600 2>&1 |
    Out-File $Log -Append -Encoding ascii
"=== matrix finished $(Get-Date -Format 'HH:mm:ss') exit=$LASTEXITCODE ===" | Out-File $Log -Append -Encoding ascii
