# G5.2 — measure the two poles with the CLI. Run BEFORE and AFTER the engine change.
#   powershell -File eval-results/2026-07-29/G5.2/measure-poles.ps1 -Tag before
# Every run uses its OWN cache root, so every analyze is cold and no snapshot from a previous
# engine build can be served (bug #13: `analyze --no-cache` leaves a stale snapshot behind).
param([Parameter(Mandatory)][string]$Tag)

$ErrorActionPreference = 'Stop'
$repo = 'C:\code\DevContext2'
$cli  = Join-Path $repo 'src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe'
$out  = Join-Path $repo "eval-results\2026-07-29\G5.2\$Tag"
New-Item -ItemType Directory -Force -Path $out | Out-Null

# Poles: name, absolute repo path, --sln (empty = let it pick)
$poles = @(
    @{ name = 'gitversion';        path = "$repo\eval-repos\GitVersion";        sln = 'new-cli/GitVersion.slnx' },
    @{ name = 'cleanarchitecture'; path = "$repo\eval-repos\CleanArchitecture"; sln = '' }
)

foreach ($p in $poles) {
    $env:DEVCONTEXT_CACHE_ROOT = Join-Path $env:TEMP "dc-g52-$Tag-$($p.name)"
    if (Test-Path $env:DEVCONTEXT_CACHE_ROOT) { Remove-Item -Recurse -Force $env:DEVCONTEXT_CACHE_ROOT }

    $slnArgs = @()
    if ($p.sln) { $slnArgs = @('--sln', $p.sln) }

    Write-Host "=== $($p.name) [$Tag] : analyze ==="
    # TRAP: `analyze` takes a POSITIONAL path; `query` takes --path.
    & $cli analyze $p.path @slnArgs *> (Join-Path $out "$($p.name)-analyze.txt")
    Write-Host "    analyze exit=$LASTEXITCODE"

    foreach ($q in @('stats', 'entrypoints', 'graphdump')) {
        & $cli query $q --path $p.path @slnArgs *> (Join-Path $out "$($p.name)-$q.json")
        Write-Host "    $q exit=$LASTEXITCODE"
    }
    Remove-Item Env:\DEVCONTEXT_CACHE_ROOT
}
Write-Host "wrote $out"
