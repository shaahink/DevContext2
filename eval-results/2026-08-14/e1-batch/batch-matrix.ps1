<#
E1.4 - the BATCH acceptance matrix.

Same seven poles and the SAME per-column computation as
eval-results/2026-08-13/e1-edges/edge-matrix.ps1 (E1.1), so the rows are directly comparable to
matrix-before.txt (pre-#11 baseline) and matrix-after-e13.txt. Two additions, both append-only:

  phantomT   Calls edges whose TYPE target is NOT a declared node in this graph. This is the
             E1.2 phantom-node gate, which E1.2 asserted on TodoApi and MediatR only; the batch
             acceptance declares it on all seven poles. Must be 0.
  the raw graphdump JSON of each pole is saved next to the matrix, so every later measurement
  (class-C impact set, bug #6 sizing) reads the SAME dump instead of re-analysing.

Poles are "name=ABSOLUTE_PATH" (house trap 1/4). CLI paths absolute (house trap 4).
#>
param(
    [string]$Label = "run",
    [string]$Out = "",
    [string]$DumpDir = "",
    [string]$Cli = "C:\Code\DevContext2-engine\src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe",
    [string[]]$Poles = @(
        "eShop=C:\Code\DevContext2-engine\eval-repos\eShop",
        "TodoApi=C:\Code\DevContext2-engine\eval-repos\TodoApi",
        "CleanArchitecture=C:\Code\DevContext2-engine\eval-repos\VerticalSlice",
        "MediatR=C:\Code\eval-poles\MediatR",
        "Hangfire=C:\Code\eval-poles\Hangfire",
        "eshop-microservices=C:\Users\shahi\source\repos\run-aspnetcore-microservices\src",
        "DevContext=C:\Code\DevContext2-engine"
    )
)

$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

Emit "E1.4 batch matrix - label=$Label"
Emit "cli=$Cli"
Emit ("cli-core-dll-written=" + (Get-Item (Join-Path (Split-Path $Cli) "DevContext.Core.dll")).LastWriteTime.ToString("s"))
Emit ""
Emit ("{0,-18} {1,7} {2,7} {3,7} {4,7} {5,7} {6,9} {7,8} {8,9}" -f "pole", "nodes", "edges", "calls", "m2m", "m2t", "verified", "approx", "phantomT")

if ($DumpDir -ne "" -and -not (Test-Path $DumpDir)) { New-Item -ItemType Directory -Force $DumpDir | Out-Null }

foreach ($p in $Poles) {
    $name = $p.Substring(0, $p.IndexOf('='))
    $path = $p.Substring($p.IndexOf('=') + 1)
    if (-not (Test-Path $path)) { Emit ("{0,-18} MISSING {1}" -f $name, $path); continue }

    $raw = (& $Cli query graphdump --path $path 2>$null) -join "`n"
    if ([string]::IsNullOrWhiteSpace($raw)) { Emit ("{0,-18} NO-OUTPUT" -f $name); continue }
    if ($DumpDir -ne "") { $raw | Out-File -Encoding utf8 (Join-Path $DumpDir ($name + ".json")) }
    try { $j = $raw | ConvertFrom-Json } catch { Emit ("{0,-18} PARSE-FAIL" -f $name); continue }

    $calls = @($j.edges | Where-Object { $_.kind -eq 'Calls' })
    $m2m = @($calls | Where-Object { $_.from -like 'Member:*' -and $_.to -like 'Member:*' })
    $m2t = @($calls | Where-Object { $_.from -like 'Member:*' -and $_.to -like 'Type:*' })
    $ver = @($calls | Where-Object { $_.resolution -eq 'Semantic' })
    $apx = $calls.Count - $ver.Count

    $declared = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($n in $j.nodes) { $declared.Add($n.id) | Out-Null }
    $phantom = @($m2t | Where-Object { -not $declared.Contains($_.to) })

    Emit ("{0,-18} {1,7} {2,7} {3,7} {4,7} {5,7} {6,9} {7,8} {8,9}" -f `
        $name, $j.nodes.Count, $j.edges.Count, $calls.Count, $m2m.Count, $m2t.Count, $ver.Count, $apx, $phantom.Count)

    if ($phantom.Count -gt 0) {
        foreach ($t in ($phantom | Select-Object -ExpandProperty to -Unique | Select-Object -First 12)) {
            Emit ("    PHANTOM-TYPE-TARGET {0}" -f $t)
        }
    }

    if ($name -eq "DevContext") {
        foreach ($h in @('DevContext.Core.Graph2.BodyFactExtractor',
                         'DevContext.Core.Utilities.RazorCodeVirtualizer',
                         'DevContext.Core.Utilities.ExtractorHelpers')) {
            $tin = @($j.edges | Where-Object { $_.to -eq ("Type:" + $h) })
            $min = @($j.edges | Where-Object { $_.to -like ("Member:" + $h + "::*") })
            Emit ("    dogfood {0,-46} typeIn={1,3} memberIn={2,3}" -f $h, $tin.Count, $min.Count)
        }
    }
}

if ($Out -ne "") {
    $dir = Split-Path $Out
    if ($dir -ne "" -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
    $lines | Out-File -Encoding utf8 $Out
    Write-Host "wrote $Out"
}
