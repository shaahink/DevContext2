# r1-metric-sweep.ps1 -- R1.1 metric recalibration instrument (post-E1).
#
# Successor to eval-results/2026-08-02/G10/g10-threshold-sweep.ps1. Same idea, three differences:
#   1. The G10 pole set no longer exists on this machine (wolverine / DntSite / GitVersion /
#      MahApps.Metro / Dapper / Serilog were never submodules and their clones are gone). Five of the
#      eleven survive -- eShop, dotnet-podcasts, CleanArchitecture, MediatR, self -- and those five
#      carry the before/after comparison. The rest of the set is refilled from what IS on disk,
#      keeping the starved..rich axis and the library shapes #23 exists for.
#   2. `query stats --format json` now carries the whole grid (sparseGraph, hubScopeNodes,
#      entriesWithTarget, entriesWithDeepSpine, deepSpineRatio, per-kind seams with the V1.1 tier
#      split, AND the insight list) -- so one call per pole answers #22, #23 and #24, and no
#      graphdump is needed for any of the three.
#   3. It captures the map markdown too, for the archetype line (the Library exclusion is one of
#      graph.orphans' five clauses and stats does not report it).
#
# DEVCONTEXT_CACHE_ROOT is redirected to a private temp dir so the FIRST query per pole analyses cold
# against THIS build (G8/G9's stale-snapshot trap, and bug #13: --no-cache does not invalidate what a
# later query reads). The user's real cache is untouched.
#
# Keep this file ASCII (PS 5.1 detached-run encoding gotcha).

param(
    [string]$Cli = "C:\Code\DevContext2-engine\src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe",
    [Parameter(Mandatory = $true)][string]$OutDir,
    [int]$TimeoutSec = 1200
)

$ErrorActionPreference = "Stop"

# name=ABSOLUTE_PATH (house trap 4: a relative path is parsed as a GitHub repo and cloned).
# [G10] marks a pole that was in the 2026-08-02 grid, so its row is directly comparable.
$Poles = @(
    @("MediatR",             "C:\Code\eval-poles\MediatR"),                                  # [G10] library
    @("AutoMapper",          "C:\Code\eval-poles\AutoMapper"),                               # library
    @("FluentValidation",    "C:\Code\eval-poles\FluentValidation"),                         # library
    @("Hangfire",            "C:\Code\eval-poles\Hangfire"),                                 # library + samples
    @("FastEndpoints",       "C:\Code\DevContext\.test-repos\FastEndpoints"),                # library
    @("CleanArchitecture",   "C:\Code\DevContext\.test-repos\CleanArchitecture"),            # [G10] app
    @("VerticalSlice",       "C:\Code\DevContext2-engine\eval-repos\VerticalSlice"),         # app
    @("TodoApi",             "C:\Code\DevContext2-engine\eval-repos\TodoApi"),               # app
    @("dotnet-podcasts",     "C:\Code\eval-poles\dotnet-podcasts"),                          # [G10] app
    @("eshop-microservices", "C:\Users\shahi\source\repos\run-aspnetcore-microservices\src"),# app
    @("eShop",               "C:\Code\DevContext2-engine\eval-repos\eShop"),                 # [G10] app
    @("self",                "C:\Code\DevContext2-engine")                                   # [G10] app
)

New-Item -ItemType Directory -Force $OutDir | Out-Null
$Raw = Join-Path $OutDir "raw"
New-Item -ItemType Directory -Force $Raw | Out-Null
$CacheRoot = Join-Path $env:TEMP ("r1-cache-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force $CacheRoot | Out-Null
$env:DEVCONTEXT_CACHE_ROOT = $CacheRoot
Write-Host "CACHE_ROOT $CacheRoot"
Write-Host ("CLI       " + $Cli)
Write-Host ("core-dll  " + (Get-Item (Join-Path (Split-Path $Cli) "DevContext.Core.dll")).LastWriteTime.ToString("s"))

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

    # Second call is a snapshot HIT (same private cache root) -- the markdown map, for the archetype.
    $mapOut = Join-Path $Raw "$name.map.md"
    $rc = Invoke-Op $Cli @("query", "map", "--path", $path, "--format", "md") `
        $mapOut (Join-Path $Raw "$name.map.err") $TimeoutSec
    $sw.Stop()
    Write-Host ("{0} OK map-rc={1} [{2:N0}s]" -f $name, $rc, $sw.Elapsed.TotalSeconds)
}

Write-Host "SWEEP DONE -> $Raw"
