<#
G6.2 — does GraphQuery's otherTitle fallback fire, and does it leak arity?

GraphQuery.NeighborsView does:   var otherTitle = _graph.Node(otherId)?.Title ?? otherId.Key;
i.e. when an edge points at a node that is NOT in the graph, the wire field the app renders as a
NAME carries the RAW CANONICAL KEY instead -- arity marker and all. This measures, per pole:

  - how many edge endpoints have no node (the fallback's blast radius)
  - how many of those endpoints carry a metadata arity marker (the actual leak)
  - a sample of the strings a user would see

Node ids come out of graphdump JSON-escaped (backtick = \u0060), so both sides are unescaped before
comparing. See the ledger trap for why that matters.
#>
param([string]$Out = "", [string[]]$Poles = @("eShop", "FluentValidation", "AutoMapper", "MediatR", "CleanArchitecture", "dotnet-podcasts"))

$RepoRoot = "C:\code\DevContext2"
$CliExe   = Join-Path $RepoRoot "src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe"
$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

Emit "G6.2 DANGLING-EDGE PROBE (GraphQuery.NeighborsView otherTitle fallback)"
Emit "cli mtime: $((Get-Item $CliExe).LastWriteTime.ToString('s'))"
Emit ""

$grandDangling = 0; $grandArity = 0
foreach ($pole in $Poles) {
    $repo = Join-Path $RepoRoot "eval-repos\$pole"
    if (-not (Test-Path $repo)) { Emit "[$pole] SKIP"; continue }
    $d = (& $CliExe query graphdump --path $repo 2>$null) -join "`n"
    if (-not $d) { Emit "[$pole] EMPTY"; continue }

    $nodeIds = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($m in [regex]::Matches($d, '\{"id":"([^"]+)","kind":"')) { $nodeIds.Add([regex]::Unescape($m.Groups[1].Value)) | Out-Null }

    $dangling = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($m in [regex]::Matches($d, '\{"from":"([^"]+)","to":"([^"]+)"')) {
        foreach ($g in @(1, 2)) {
            $ep = [regex]::Unescape($m.Groups[$g].Value)
            if (-not $nodeIds.Contains($ep)) { $dangling.Add($ep) | Out-Null }
        }
    }
    $withArity = @($dangling | Where-Object { $_ -match '`\d' })
    $grandDangling += $dangling.Count; $grandArity += $withArity.Count

    Emit ("POLE {0,-20} nodes {1,6}   endpoints with NO node: {2,5}   of those carrying arity: {3,4}" -f $pole, $nodeIds.Count, $dangling.Count, $withArity.Count)
    foreach ($x in ($withArity | Sort-Object | Select-Object -First 6)) { Emit "      LEAKS-AS-A-NAME  $x" }
    foreach ($x in ($dangling | Sort-Object | Select-Object -First 4)) { Emit "      dangling         $x" }
}

Emit ""
Emit "TOTAL dangling endpoints: $grandDangling   of which arity-bearing: $grandArity"

if ($Out) {
    $dir = Split-Path -Parent $Out
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    $lines | Set-Content -Path $Out -Encoding utf8
    Write-Host "`nwritten: $Out"
}
