# G6.1 — is aspire-samples' EMPTY per-service block a regression from the one-predicate change,
# or was it already empty? Measure both binaries. Uses `git stash push -- <file>` so the restored
# file gets a FRESH timestamp (Copy-Item preserves the old one and MSBuild then skips the compile).
$ErrorActionPreference = 'Continue'
$repoRoot = 'C:\code\DevContext2'
$src  = 'src/DevContext.Core/Extractors/Generic/ArchitectureStyleDetector.cs'
$cli  = 'C:\code\DevContext2\src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe'
$out  = 'C:\code\DevContext2\eval-results\2026-07-29\G6'
$repo = 'C:\code\DevContext2\eval-repos\aspire-samples'

function Show([string]$label, [string]$file) {
    $lines = Get-Content $file
    $rows = @(); $inBlock = $false
    foreach ($line in $lines) {
        if ($line -match 'per service:') { $inBlock = $true; continue }
        if ($inBlock) {
            if ($line -match '^\s{6,}([^:]+):\s') { $rows += $Matches[1].Trim() }
            elseif ($line.Trim() -eq '') { $inBlock = $false }
        }
    }
    Write-Host ("  " + $label + " per-service rows = " + $rows.Count + "  [" + ($rows -join ', ') + "]")
}

Write-Host '== BEFORE (pre-G6.1 detector) =='
& git -C $repoRoot stash push -- $src
& dotnet build C:\code\DevContext2\src\DevContext.Cli -clp:ErrorsOnly --no-incremental
& $cli analyze $repo *> (Join-Path $out 'aspire-before-analyze.txt')
& $cli query map --path $repo --format md *> (Join-Path $out 'aspire-before-map.txt')
Show 'BEFORE' (Join-Path $out 'aspire-before-map.txt')

Write-Host '== AFTER (G6.1 detector) =='
& git -C $repoRoot stash pop
& dotnet build C:\code\DevContext2\src\DevContext.Cli -clp:ErrorsOnly --no-incremental
& $cli analyze $repo *> (Join-Path $out 'aspire-after-analyze.txt')
& $cli query map --path $repo --format md *> (Join-Path $out 'aspire-after-map.txt')
Show 'AFTER ' (Join-Path $out 'aspire-after-map.txt')

Write-Host 'DONE-ASPIRE'
