# g9-archetype-sweep.ps1 -- G9.1 archetype differential instrument.
# Runs `analyze <ABS repo path> [--sln X] --no-cache --format json` over the archetype-bearing poles and
# writes one "<pole>,<archetype>,<style>" line per repo. Two runs (before/after a Core change) diff to
# exactly the set of poles whose archetype moved.
#
# Why --no-cache on EVERY pole (G8's trap, paid for once already): SnapshotCacheService keys on the TARGET
# repo's git HEAD, NOT on the DevContext build, so without it the "after" run is served the "before" answer.
# Why -Cli is a parameter: take the BEFORE side against a COPY of bin/ so the tree can be edited and rebuilt
# while the sweep is still running.
# Keep this file ASCII (PS 5.1 detached-run encoding gotcha).

param(
    [Parameter(Mandatory = $true)][string]$Cli,
    [Parameter(Mandatory = $true)][string]$Out,
    [int]$TimeoutSec = 900
)

$ErrorActionPreference = "Stop"
$RepoRoot = "C:\code\DevContext2"
$Repos = Join-Path $RepoRoot "eval-repos"
$Tmp = Join-Path $env:TEMP ("g9-sweep-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force $Tmp | Out-Null

# pole name -> @(repo dir, sln arg or ""). The sln values mirror eval/expectations/graph-truth/<pole>.json
# so this instrument measures the same scope the matrix does.
$Poles = @(
    @("CLI",                    "CLI",                  ""),
    @("MahApps.Metro",          "MahApps.Metro",        ""),
    @("gitversion",             "GitVersion",           "new-cli/GitVersion.slnx"),
    @("gitversion-default",     "GitVersion",           ""),
    @("ScreenToGif",            "ScreenToGif",          ""),
    @("PowerToys",              "PowerToys",            ""),
    @("Desktop",                "Desktop",              ""),
    @("AutoMapper",             "AutoMapper",           ""),
    @("Newtonsoft.Json",        "Newtonsoft.Json",      ""),
    @("StackExchange.Redis",    "StackExchange.Redis",  ""),
    @("Spectre.Console",        "Spectre.Console",      ""),
    @("xUnit",                  "xUnit",                ""),
    @("Dapper",                 "Dapper",               ""),
    @("Serilog",                "Serilog",              ""),
    @("Polly",                  "Polly",                ""),
    @("refit",                  "refit",                ""),
    @("FluentValidation",       "FluentValidation",     ""),
    @("CommunityToolkit.Mvvm",  "CommunityToolkit.Mvvm",""),
    @("Quartz.NET",             "Quartz.NET",           ""),
    @("Hangfire",               "Hangfire",             ""),
    @("RestSharp",              "RestSharp",            ""),
    @("MassTransit",            "MassTransit",          ""),
    @("MediatR",                "MediatR",              ""),
    @("gRPC",                   "gRPC",                 ""),
    @("HotChocolate",           "HotChocolate",         ""),
    @("Orleans",                "Orleans",              ""),
    @("CleanArchitecture",      "CleanArchitecture",    ""),
    @("DntSite",                "DntSite",              ""),
    @("aspire-samples",         "aspire-samples",       ""),
    @("razorpages-app",         "razorpages-app",       ""),
    @("dotnet-podcasts",        "dotnet-podcasts",      ""),
    @("OrchardCore",            "OrchardCore",          ""),
    @("bitwarden-server",       "bitwarden-server",     ""),
    @("MassTransit-Sample",     "MassTransit-Sample",   ""),
    @("eShop",                  "eShop",                ""),
    @("SignalR",                "SignalR",              ""),
    @("wolverine",              "wolverine",            "")
)

$Lines = New-Object System.Collections.Generic.List[string]
foreach ($p in $Poles) {
    $name = $p[0]; $dir = $p[1]; $sln = $p[2]
    $path = Join-Path $Repos $dir
    if (-not (Test-Path $path)) { $Lines.Add("$name,MISSING,MISSING"); Write-Host "$name MISSING"; continue }
    $kernel = Join-Path $Tmp "$name.json"
    $args = @("analyze", $path)
    if ($sln) { $args += @("--sln", $sln) }
    $args += @("--no-cache", "-o", $kernel, "--format", "json", "--quiet")
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $proc = Start-Process -FilePath $Cli -ArgumentList $args -PassThru -NoNewWindow `
        -RedirectStandardOutput (Join-Path $Tmp "$name.out") -RedirectStandardError (Join-Path $Tmp "$name.err")
    $null = $proc.Handle   # PS 5.1: caching the handle is what makes .ExitCode readable later
    if (-not $proc.WaitForExit($TimeoutSec * 1000)) {
        & taskkill /PID $proc.Id /T /F 2>&1 | Out-Null
        $Lines.Add("$name,TIMEOUT,TIMEOUT"); Write-Host "$name TIMEOUT"; continue
    }
    $sw.Stop()
    if (-not (Test-Path $kernel)) { $Lines.Add("$name,ERR,ERR"); Write-Host "$name ERR (no kernel)"; continue }
    $j = Get-Content $kernel -Raw | ConvertFrom-Json
    $line = "$name,$($j.archetype),$($j.architectureStyle)"
    $Lines.Add($line)
    Write-Host ("{0}  [{1:N1}s]" -f $line, $sw.Elapsed.TotalSeconds)
}

Set-Content -Path $Out -Value $Lines -Encoding ASCII
Write-Host "WROTE $Out"
