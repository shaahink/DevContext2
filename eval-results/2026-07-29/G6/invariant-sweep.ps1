# G6.1 — prove the ONE-VOCABULARY invariant on REAL repos, not just fixtures:
# the per-service breakdown rows (map "per service:" block) must be exactly the graph's Service
# nodes (graphdump, kind=Service) — the set the Atlas canvas draws.
# ASCII only. Absolute paths only. `query` takes a POSITIONAL op and --path.
$ErrorActionPreference = 'Continue'
$cli = 'C:\code\DevContext2\src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe'
$out = 'C:\code\DevContext2\eval-results\2026-07-29\G6'
$repos = @('eShop', 'aspire-samples', 'CleanArchitecture', 'dotnet-podcasts', 'bitwarden-server')

foreach ($name in $repos) {
    $repo = "C:\code\DevContext2\eval-repos\$name"
    Write-Host "===== $name ====="
    & $cli analyze $repo *> (Join-Path $out "sweep-$name-analyze.txt")
    & $cli query map --path $repo --format md *> (Join-Path $out "sweep-$name-map.txt")
    & $cli query graphdump --path $repo *> (Join-Path $out "sweep-$name-graphdump.json")

    # per-service rows: the indented "  Name: Style" lines under "per service:"
    $mapLines = Get-Content (Join-Path $out "sweep-$name-map.txt")
    $rows = @()
    $inBlock = $false
    foreach ($line in $mapLines) {
        if ($line -match 'per service:') { $inBlock = $true; continue }
        if ($inBlock) {
            if ($line -match '^\s{6,}([^:]+):\s') { $rows += $Matches[1].Trim() }
            elseif ($line.Trim() -eq '') { $inBlock = $false }
        }
    }

    $dump = Get-Content (Join-Path $out "sweep-$name-graphdump.json") -Raw | ConvertFrom-Json
    $svc = @($dump.nodes | Where-Object { $_.kind -eq 'Service' } | ForEach-Object { $_.project })

    $rowsSorted = @($rows | Sort-Object -Unique)
    $svcSorted  = @($svc  | Sort-Object -Unique)
    $onlyRows = @($rowsSorted | Where-Object { $svcSorted -notcontains $_ })
    $onlySvc  = @($svcSorted  | Where-Object { $rowsSorted -notcontains $_ })

    Write-Host ("  breakdown rows : " + $rowsSorted.Count)
    Write-Host ("  Service nodes  : " + $svcSorted.Count)
    Write-Host ("  breakdown-only : " + $(if ($onlyRows.Count) { $onlyRows -join ', ' } else { '(none)' }))
    Write-Host ("  canvas-only    : " + $(if ($onlySvc.Count)  { $onlySvc  -join ', ' } else { '(none)' }))
    if ($onlyRows.Count -eq 0 -and $onlySvc.Count -eq 0) { Write-Host "  VERDICT: SAME SET" }
    else { Write-Host "  VERDICT: DIVERGED" }
}
Write-Host 'DONE-SWEEP'
