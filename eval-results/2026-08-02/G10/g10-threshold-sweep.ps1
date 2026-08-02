# g10-threshold-sweep.ps1 -- G10.1 threshold-input instrument.
#
# Every threshold this checkpoint re-measures reads ONE of these quantities off a real repo:
#   nodeCount / edgeCount / edge-per-node ratio      (GraphBuilder.Seams AddHubScopeEdges sparseness)
#   Calls edge count + Semantic share                (GraphOrphansSource coverage floor)
#   Handles / Sends edge counts                      (GraphOrphansSource wiring floor)
#   entries + entriesWithTarget                      (home-page unwired severity, orphans wiredRatio)
#   flows with >= 2 steps / total entries            (GraphStats deep-spine ratio)
#   nodes with >= 2 outgoing Resolves edges          (GraphQuery interesting-point "seats")
# So one cold analysis per pole, dumped twice, answers all of them. Aggregation is in
# g10-aggregate.mjs (node) because a 100k-edge graphdump is not a thing to ConvertFrom-Json.
#
# DEVCONTEXT_CACHE_ROOT is redirected to a private temp dir: the FIRST query per pole analyses cold
# against THIS build (no stale snapshot from an older build -- G8/G9's trap), writes its snapshot
# there, and the SECOND query hits it. One analysis per pole instead of three, and the user's real
# cache is never touched.
#
# Keep this file ASCII (PS 5.1 detached-run encoding gotcha).

param(
    [string]$Cli = "C:\code\DevContext2\src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe",
    [Parameter(Mandatory = $true)][string]$OutDir,
    [int]$TimeoutSec = 1200
)

$ErrorActionPreference = "Stop"
$Repos = "C:\code\DevContext2\eval-repos"

# Poles chosen to span the starved..rich axis the thresholds were calibrated on, and to INCLUDE the
# two repos the GraphOrphansSource comment names as its own justification (dotnet-podcasts, wolverine).
$Poles = @(
    @("MediatR",           (Join-Path $Repos "MediatR")),
    @("Serilog",           (Join-Path $Repos "Serilog")),
    @("Dapper",            (Join-Path $Repos "Dapper")),
    @("wolverine",         (Join-Path $Repos "wolverine")),
    @("MahApps.Metro",     (Join-Path $Repos "MahApps.Metro")),
    @("GitVersion",        (Join-Path $Repos "GitVersion")),
    @("CleanArchitecture", (Join-Path $Repos "CleanArchitecture")),
    @("dotnet-podcasts",   (Join-Path $Repos "dotnet-podcasts")),
    @("DntSite",           (Join-Path $Repos "DntSite")),
    @("eShop",             (Join-Path $Repos "eShop")),
    @("self",              "C:\code\DevContext2")
)

New-Item -ItemType Directory -Force $OutDir | Out-Null
$Raw = Join-Path $OutDir "raw"
New-Item -ItemType Directory -Force $Raw | Out-Null
$CacheRoot = Join-Path $env:TEMP ("g10-cache-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force $CacheRoot | Out-Null
$env:DEVCONTEXT_CACHE_ROOT = $CacheRoot
Write-Host "CACHE_ROOT $CacheRoot"

function Invoke-Op($exe, $opArgs, $outFile, $errFile, $timeoutSec) {
    $p = Start-Process -FilePath $exe -ArgumentList $opArgs -PassThru -NoNewWindow `
        -RedirectStandardOutput $outFile -RedirectStandardError $errFile
    $null = $p.Handle
    if (-not $p.WaitForExit($timeoutSec * 1000)) {
        & taskkill /PID $p.Id /T /F 2>&1 | Out-Null
        return -1
    }
    return $p.ExitCode
}

foreach ($pole in $Poles) {
    $name = $pole[0]; $path = $pole[1]
    if (-not (Test-Path $path)) { Write-Host "$name MISSING $path"; continue }

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $statsOut = Join-Path $Raw "$name.stats.json"
    $rc = Invoke-Op $Cli @("query", "stats", "--path", $path, "--format", "json") `
        $statsOut (Join-Path $Raw "$name.stats.err") $TimeoutSec
    if ($rc -ne 0) { Write-Host ("{0} STATS-FAIL rc={1} [{2:N0}s]" -f $name, $rc, $sw.Elapsed.TotalSeconds); continue }

    $dumpOut = Join-Path $Raw "$name.graphdump.json"
    $rc = Invoke-Op $Cli @("query", "graphdump", "--path", $path, "--format", "json") `
        $dumpOut (Join-Path $Raw "$name.graphdump.err") $TimeoutSec
    $sw.Stop()
    if ($rc -ne 0) { Write-Host ("{0} DUMP-FAIL rc={1} [{2:N0}s]" -f $name, $rc, $sw.Elapsed.TotalSeconds); continue }

    $mb = [math]::Round((Get-Item $dumpOut).Length / 1MB, 1)
    Write-Host ("{0} OK dump={1}MB [{2:N0}s]" -f $name, $mb, $sw.Elapsed.TotalSeconds)
}

Write-Host "SWEEP DONE -> $Raw"
