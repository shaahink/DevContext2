<#
G6.2 — is a CLIENT-SIDE id->label derivation faithful to the ENGINE's own title?

The app has to label a node when the wire gives it no title (an in-flight GetNode, an edge whose
otherTitle is empty). Today it invents a different rule at each site. Before replacing them with ONE
rule, this MEASURES whether that rule can reproduce the engine's own GraphNode.Title exactly, per
kind, on real repos -- so the fallback is indistinguishable from the real thing instead of a second
vocabulary (which is the whole point of D-4).

THE RULE UNDER TEST (the client mirror of SymbolCanon.ShortNameOf + GraphBuilder's title choices):
    Member  "Ns.Owner`N::M"  -> shortName(Ns.Owner`N) + "." + M
    Type    "Ns.Sub.Name`N"  -> shortName(Ns.Sub.Name`N)
    other                    -> the key verbatim
    shortName(x) = drop everything from the last backtick, then the last dotted segment

Output: per kind, MATCH / MISMATCH counts and the first mismatches, so a divergence is a fact I have
to design around rather than a surprise after the fact.
#>
param([string]$Out = "", [string[]]$Poles = @("eShop", "FluentValidation", "AutoMapper", "MediatR", "CleanArchitecture", "dotnet-podcasts"))

$RepoRoot = "C:\code\DevContext2"
$CliExe   = Join-Path $RepoRoot "src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe"
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

Emit "G6.2 LABEL-MIRROR FIDELITY  (client rule vs engine GraphNode.Title)"
Emit "cli mtime: $((Get-Item $CliExe).LastWriteTime.ToString('s'))"
Emit ""

$totals = @{}
foreach ($pole in $Poles) {
    $repo = Join-Path $RepoRoot "eval-repos\$pole"
    if (-not (Test-Path $repo)) { Emit "[$pole] SKIP"; continue }
    $d = (& $CliExe query graphdump --path $repo 2>$null) -join "`n"
    if (-not $d) { Emit "[$pole] EMPTY"; continue }

    $per = @{}
    $mismatches = New-Object System.Collections.Generic.List[string]
    foreach ($m in [regex]::Matches($d, '\{"id":"([^"]+)","kind":"([^"]+)","title":"([^"]*)"')) {
        # JSON-unescape the two fields we compare (System.Text.Json escapes ` as \u0060 and more).
        $id    = [regex]::Unescape($m.Groups[1].Value)
        $kind  = $m.Groups[2].Value
        $title = [regex]::Unescape($m.Groups[3].Value)
        $key   = $id.Substring($id.IndexOf(':') + 1)
        $mirror = Mirror $kind $key
        if (-not $per.ContainsKey($kind)) { $per[$kind] = @{ ok = 0; bad = 0 } }
        if ($mirror -ceq $title) { $per[$kind].ok++ } else {
            $per[$kind].bad++
            if ($mismatches.Count -lt 8) { $mismatches.Add("      $kind id=$id`n         engine='$title'  mirror='$mirror'") | Out-Null }
        }
        if (-not $totals.ContainsKey($kind)) { $totals[$kind] = @{ ok = 0; bad = 0 } }
        if ($mirror -ceq $title) { $totals[$kind].ok++ } else { $totals[$kind].bad++ }
    }
    Emit "POLE $pole"
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
