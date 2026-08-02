# S5 widen baseline -- measures the 15 NEW matrix poles on PRE-Batch-D binaries.
# Detached by design (S3 lesson: the agent harness kills background commands at 10 min; the big
# poles here -- PowerToys, bitwarden-server, HotChocolate, xUnit -- run far longer than that).
# ASCII only (PS 5.1 detached-run encoding gotcha).
$ErrorActionPreference = "Continue"
$RepoRoot = "C:\code\DevContext2"
$OutRoot = Join-Path $RepoRoot "eval-results\2026-07-28\graph-truth-s5-baseline"
$Log = Join-Path $RepoRoot "eval-results\2026-07-28\s5-widen-baseline.txt"

$poles = @(
    "razorpages-app", "Desktop", "CLI", "Dapper", "StackExchange.Redis",
    "CommunityToolkit.Mvvm", "MahApps.Metro", "MassTransit", "ScreenToGif",
    "blazor-samples", "DntSite", "xUnit", "HotChocolate", "bitwarden-server", "PowerToys"
)

"=== S5 widen baseline (pre-Batch-D) started $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') ===" | Out-File $Log -Encoding ascii
foreach ($p in $poles) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    "--- $p ---" | Out-File $Log -Append -Encoding ascii
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot "eval\graph-truth.ps1") $p -SkipBuild -OutRoot $OutRoot 2>&1 |
        Out-File $Log -Append -Encoding ascii
    $sw.Stop()
    "--- $p done in $([int]$sw.Elapsed.TotalSeconds)s (exit $LASTEXITCODE) ---" | Out-File $Log -Append -Encoding ascii
}
"=== S5 widen baseline finished $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') ===" | Out-File $Log -Append -Encoding ascii
