# G8.2 differential capture, second batch -- see g82-capture.ps1 for why --no-cache is mandatory.
# VerticalSlice turned out to be a byte-identical second checkout of CleanArchitecture, so this batch
# widens the set to keep the pole count honest.
param([Parameter(Mandatory = $true)][string]$Out)

$ErrorActionPreference = "Continue"
$Cli = "C:/code/DevContext2/src/DevContext.Cli/bin/Debug/net10.0/DevContext.Cli.exe"
$Repos = @("SignalR", "Ocelot", "Dapper", "Spectre.Console", "Hangfire")

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
