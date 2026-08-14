<#
E1.2 - explain, edge by edge, the Calls pairs that TodoApi (-2) and MediatR (-5) lost against the
E1.1 baseline. Builds the CLI twice from the same worktree: once with src/DevContext.Core taken from
the pre-E1.2 commit, once from HEAD. Core edits change the assembly MVID and DevContext's snapshots
are MVID-keyed, so each build cold-starts its own analysis - no stale-snapshot rescue needed.

Pure ASCII (PowerShell 5.1 reads unmarked UTF-8 as cp1252).
#>
param(
    [string]$Base = "795ad1b",
    [string]$Out  = "C:\Code\DevContext2-engine\eval-results\2026-08-13\e1-span\lost-edge-diff.txt",
    [string]$Cli  = "C:\Code\DevContext2-engine\src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe",
    [string[]]$Poles = @(
        "TodoApi=C:\Code\DevContext2-engine\eval-repos\TodoApi",
        "MediatR=C:\Code\eval-poles\MediatR"
    )
)

$repo = "C:\Code\DevContext2-engine"
$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

function Dump($path) {
    $raw = (& $Cli query graphdump --path $path 2>$null) -join "`n"
    if ([string]::IsNullOrWhiteSpace($raw)) { return $null }
    return $raw | ConvertFrom-Json
}

function CallPairs($j) {
    $set = @{}
    foreach ($e in $j.edges) { if ($e.kind -eq 'Calls') { $set["$($e.from) -> $($e.to)"] = $e.resolution } }
    return $set
}

Emit "E1.2 lost-edge diff - base=$Base vs HEAD"

# --- side A: pre-E1.2 Core ---
& git -C $repo checkout $Base -- src/DevContext.Core
if ($LASTEXITCODE -ne 0) { Emit "CHECKOUT-FAILED"; exit 1 }
& dotnet build $repo\src\DevContext.Cli -clp:ErrorsOnly | Out-Null
Emit ("base-core-dll=" + (Get-Item (Join-Path (Split-Path $Cli) "DevContext.Core.dll")).LastWriteTime.ToString("s"))
$before = @{}
foreach ($p in $Poles) {
    $name = $p.Substring(0, $p.IndexOf('=')); $path = $p.Substring($p.IndexOf('=') + 1)
    $j = Dump $path
    if ($null -eq $j) { Emit "$name BASE NO-OUTPUT"; continue }
    $before[$name] = CallPairs $j
    Emit ("{0,-10} base calls={1}" -f $name, $before[$name].Count)
}

# --- side B: HEAD Core ---
& git -C $repo checkout HEAD -- src/DevContext.Core
& dotnet build $repo\src\DevContext.Cli -clp:ErrorsOnly | Out-Null
Emit ("head-core-dll=" + (Get-Item (Join-Path (Split-Path $Cli) "DevContext.Core.dll")).LastWriteTime.ToString("s"))
foreach ($p in $Poles) {
    $name = $p.Substring(0, $p.IndexOf('=')); $path = $p.Substring($p.IndexOf('=') + 1)
    $j = Dump $path
    if ($null -eq $j) { Emit "$name HEAD NO-OUTPUT"; continue }
    $after = CallPairs $j
    $declared = @{}
    foreach ($n in $j.nodes) { if ($n.kind -eq 'Type') { $declared[$n.id] = $true } }
    Emit ""
    Emit ("{0,-10} head calls={1}" -f $name, $after.Count)

    $lost = @($before[$name].Keys | Where-Object { -not $after.ContainsKey($_) })
    $gained = @($after.Keys | Where-Object { -not $before[$name].ContainsKey($_) })
    Emit ("  LOST={0}  GAINED={1}" -f $lost.Count, $gained.Count)
    foreach ($k in ($lost | Sort-Object)) { Emit ("    LOST   {0}   [was {1}]" -f $k, $before[$name][$k]) }
    foreach ($k in ($gained | Sort-Object)) { Emit ("    GAINED {0}   [{1}]" -f $k, $after[$k]) }

    # Every Calls target on the HEAD build must be a type this solution declares.
    $bad = @($after.Keys | Where-Object { $_ -match '-> (Type:.+)$' -and -not $declared[$Matches[1]] })
    Emit ("  head Calls edges whose TYPE target is not a declared node: {0}" -f $bad.Count)
    foreach ($k in ($bad | Select-Object -First 10)) { Emit ("    UNDECLARED {0}" -f $k) }
}

$lines | Out-File -Encoding utf8 $Out
Write-Host "wrote $Out"
