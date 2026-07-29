# G8.2 differential capture -- full graph dump per pole from a FRESH analysis.
# --no-cache is REQUIRED: SnapshotCacheService.ComputeKeys keys on the TARGET repo's git HEAD, not on
# the DevContext build, so without it an engine change is served the PRE-change snapshot.
# Usage: powershell -File g82-capture.ps1 -Out <dir>
param([Parameter(Mandatory = $true)][string]$Out)

$ErrorActionPreference = "Continue"
$Cli = "C:/code/DevContext2/src/DevContext.Cli/bin/Debug/net10.0/DevContext.Cli.exe"
$Repos = @("TodoApi", "VerticalSlice", "eShop", "FluentValidation", "CleanArchitecture",
           "GitVersion", "MediatR", "AutoMapper", "Serilog", "Polly")

New-Item -ItemType Directory -Force -Path $Out | Out-Null
foreach ($r in $Repos) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $path = "C:/code/DevContext2/eval-repos/$r"
    $dest = Join-Path $Out "$r.graphdump.txt"
    & $Cli query graphdump --path $path --no-cache 2>$null | Out-File -Encoding utf8 $dest
    $len = if (Test-Path $dest) { (Get-Item $dest).Length } else { -1 }
    Write-Host ("{0,-18} {1,5}s  bytes={2}" -f $r, [int]$sw.Elapsed.TotalSeconds, $len)
}
Write-Host "CAPTURE-COMPLETE $Out"
