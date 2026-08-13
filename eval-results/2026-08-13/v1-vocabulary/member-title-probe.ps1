<#
V1.2 (#17) - ONE member-title vocabulary.

Re-runs the instrument that FILED bug #17 (eval-results/2026-07-29/G6/label-mirror-fidelity.ps1):
for every graphdump node it derives a title from the node's ID with ONE rule and compares it to the
engine's own GraphNode.Title. A Member node whose engine title is bare ("Send") mismatches the rule
("Mediator.Send"); a node titled by the rule matches. So:

    Member MISMATCH count = the size of the second vocabulary.

THE RULE (identical to the 2026-07-29 probe, character for character):
    Member  "Ns.Owner`N::M"  -> shortName(Ns.Owner`N) + "." + M
    Type    "Ns.Sub.Name`N"  -> shortName(Ns.Sub.Name`N)
    other                    -> the key verbatim
    shortName(x) = drop everything from the last backtick, then keep the last dotted segment

Poles are passed as "name=ABSOLUTE_PATH" so nothing depends on a working directory (house trap 1/4).
#>
param(
    [string]$Out = "",
    [string]$Cli = "C:\Code\DevContext2-engine\src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe",
    [string[]]$Poles = @(
        "eShop=C:\Code\DevContext2-engine\eval-repos\eShop",
        "FluentValidation=C:\Code\eval-poles\FluentValidation",
        "AutoMapper=C:\Code\eval-poles\AutoMapper",
        "MediatR=C:\Code\eval-poles\MediatR",
        "CleanArchitecture=C:\Code\DevContext2-engine\eval-repos\VerticalSlice",
        "dotnet-podcasts=C:\Code\eval-poles\dotnet-podcasts",
        "DevContext=C:\Code\DevContext2-engine"
    )
)

$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

function ShortName([string]$x) {
    $tick = $x.LastIndexOf([char]96)
    $span = if ($tick -gt 0) { $x.Substring(0, $tick) } else { $x }
    $dot  = $span.LastIndexOf('.')
    if ($dot -ge 0) { return $span.Substring($dot + 1) } else { return $span }
}

function Mirror([string]$kind, [string]$key) {
    $sep = $key.IndexOf('::')
    if ($sep -gt 0) { return (ShortName $key.Substring(0, $sep)) + "." + $key.Substring($sep + 2) }
    if ($kind -eq 'Type') { return (ShortName $key) }
    return $key
}

Emit "V1.2 MEMBER-TITLE VOCABULARY PROBE (one-rule mirror vs engine GraphNode.Title)"
Emit "cli: $Cli"
Emit "cli mtime: $((Get-Item $Cli).LastWriteTime.ToString('s'))"
Emit ""

$totals = @{}
foreach ($pole in $Poles) {
    $eq = $pole.IndexOf('=')
    $name = $pole.Substring(0, $eq)
    $repo = $pole.Substring($eq + 1)
    if (-not (Test-Path $repo)) { Emit "[$name] SKIP (no repo at $repo)"; continue }
    $d = (& $Cli query graphdump --path $repo 2>$null) -join "`n"
    if (-not $d) { Emit "[$name] EMPTY"; continue }

    $per = @{}
    $mismatches = New-Object System.Collections.Generic.List[string]
    foreach ($m in [regex]::Matches($d, '\{"id":"([^"]+)","kind":"([^"]+)","title":"([^"]*)"')) {
        $id    = [regex]::Unescape($m.Groups[1].Value)
        $kind  = $m.Groups[2].Value
        $title = [regex]::Unescape($m.Groups[3].Value)
        $key   = $id.Substring($id.IndexOf(':') + 1)
        $mirror = Mirror $kind $key
        if (-not $per.ContainsKey($kind)) { $per[$kind] = @{ ok = 0; bad = 0 } }
        if (-not $totals.ContainsKey($kind)) { $totals[$kind] = @{ ok = 0; bad = 0 } }
        if ($mirror -ceq $title) { $per[$kind].ok++; $totals[$kind].ok++ } else {
            $per[$kind].bad++; $totals[$kind].bad++
            if ($mismatches.Count -lt 8) { $mismatches.Add("      $kind id=$id`n         engine='$title'  mirror='$mirror'") | Out-Null }
        }
    }
    Emit "POLE $name"
    foreach ($k in ($per.Keys | Sort-Object)) { Emit ("  {0,-12} match {1,6}   MISMATCH {2,6}" -f $k, $per[$k].ok, $per[$k].bad) }
    foreach ($x in $mismatches) { Emit $x }
    Emit ""
}

Emit "=== TOTALS ==="
foreach ($k in ($totals.Keys | Sort-Object)) { Emit ("  {0,-12} match {1,6}   MISMATCH {2,6}" -f $k, $totals[$k].ok, $totals[$k].bad) }

if ($Out) {
    $dir = Split-Path -Parent $Out
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    $lines | Set-Content -Path $Out -Encoding utf8
    Write-Host "`nwritten: $Out"
}
