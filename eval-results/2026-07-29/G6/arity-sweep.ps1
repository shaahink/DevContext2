<#
G6.2 — arity sweep. "Raw metadata arity never reaches the UI."

MEASURES, per pole, every surface the engine renders, for a raw metadata arity marker (backtick + a
digit) in HUMAN-FACING text:

  [graphdump]      node .title      — the field every app surface renders (Atlas, inspector, radar)
  [*.md]           query <op> --format md — the markdown Core/CLI/MCP/server SHARE
  [analyze]        the markdown context pack

TWO MEASUREMENT TRAPS THIS SCRIPT EXISTS TO AVOID (both cost the first attempt a false CLEAN):
  1. System.Text.Json escapes the backtick as \u0060 — a JSON surface contains NO literal 0x60 at all.
     JSON is therefore swept with '\\u0060', text with the literal character.
  2. `query map` defaults to JSON. The rendered markdown is `query <op> --format md`.

CONTROL: node IDs are swept too and their arity count is REQUIRED to be non-zero. Arity is part of
canonical identity and must stay in ids; a zero there means the sweep is broken (trap 1) or the fix
went too far and erased identity. Either way the script says FAIL, not CLEAN.

Usage:  powershell -File eval-results/2026-07-29/G6/arity-sweep.ps1 -Out <file> [-Poles a,b,c]
#>
param(
    [string]$Out = "",
    [string[]]$Poles = @("eShop", "FluentValidation", "AutoMapper", "MediatR", "CleanArchitecture")
)

$ErrorActionPreference = "Continue"
$RepoRoot = "C:\code\DevContext2"
$CliExe   = Join-Path $RepoRoot "src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe"

# Single-quoted so the backtick stays literal.
$TextArityRx  = '`\d'
$JsonTitleRx  = '"title":"[^"]*\\u0060\d[^"]*"'
$JsonIdRx     = '"id":"[^"]*\\u0060\d[^"]*"'

$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

Emit "G6.2 ARITY SWEEP"
Emit "cli      : $CliExe"
Emit "cli mtime: $((Get-Item $CliExe).LastWriteTime.ToString('s'))"
Emit "poles    : $($Poles -join ', ')"
Emit ""

$grandTitleHits = 0
$grandTextHits  = 0
$grandIdHits    = 0

foreach ($pole in $Poles) {
    $repo = Join-Path $RepoRoot "eval-repos\$pole"
    if (-not (Test-Path $repo)) { Emit "[$pole] SKIP - not on disk"; continue }

    Emit "==================================================================="
    Emit "POLE $pole"
    Emit "==================================================================="

    # ---- graphdump (JSON): titles are the claim, ids are the control ----------------
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $dump = (& $CliExe query graphdump --path $repo 2>$null) -join "`n"
    $sw.Stop()
    if (-not $dump) { Emit "  [graphdump] EMPTY - analysis failed"; continue }

    $titleHits = [regex]::Matches($dump, $JsonTitleRx)
    $idHits    = [regex]::Matches($dump, $JsonIdRx)
    $grandTitleHits += $titleHits.Count
    $grandIdHits    += $idHits.Count
    Emit ("  [graphdump] {0:n0} chars / {1:n1}s - node TITLES with arity: {2}   [control] node IDS with arity: {3}" -f $dump.Length, $sw.Elapsed.TotalSeconds, $titleHits.Count, $idHits.Count)
    foreach ($d in ($titleHits | ForEach-Object { $_.Value } | Sort-Object -Unique | Select-Object -First 25)) { Emit "      TITLE-HIT $d" }

    # ---- the RENDERED markdown surfaces --------------------------------------------
    foreach ($op in @("map", "entrypoints", "stats")) {
        $txt = (& $CliExe query $op --path $repo --format md 2>$null) -join "`n"
        if (-not $txt) { Emit "  [$op.md] EMPTY"; continue }
        $hits = [regex]::Matches($txt, $TextArityRx)
        $grandTextHits += $hits.Count
        Emit ("  [{0}.md] {1:n0} chars - arity markers: {2}" -f $op, $txt.Length, $hits.Count)
        if ($hits.Count -gt 0) {
            foreach ($b in ($txt -split "`n" | Where-Object { $_ -match $TextArityRx } | Select-Object -Unique -First 15)) { Emit ("      TEXT-HIT " + $b.Trim()) }
        }
    }

    # ---- the markdown context pack --------------------------------------------------
    $pack = (& $CliExe analyze $repo 2>$null) -join "`n"
    if ($pack) {
        $hits = [regex]::Matches($pack, $TextArityRx)
        $grandTextHits += $hits.Count
        Emit ("  [analyze pack] {0:n0} chars - arity markers: {1}" -f $pack.Length, $hits.Count)
        if ($hits.Count -gt 0) {
            foreach ($b in ($pack -split "`n" | Where-Object { $_ -match $TextArityRx } | Select-Object -Unique -First 15)) { Emit ("      PACK-HIT " + $b.Trim()) }
        }
    } else { Emit "  [analyze pack] EMPTY" }
    Emit ""
}

Emit "==================================================================="
Emit "node TITLES carrying arity      : $grandTitleHits   (must be 0)"
Emit "rendered-text arity markers     : $grandTextHits    (must be 0)"
Emit "node IDS carrying arity [CONTROL]: $grandIdHits     (must be > 0 - identity keeps its arity)"
$verdict = if ($grandIdHits -eq 0) { "SWEEP BROKEN OR IDENTITY ERASED - the control measured 0" }
           elseif ($grandTitleHits -eq 0 -and $grandTextHits -eq 0) { "CLEAN - no raw metadata arity reaches a rendered surface" }
           else { "ARITY REACHES THE UI" }
Emit "VERDICT: $verdict"

if ($Out) {
    $dir = Split-Path -Parent $Out
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    $lines | Set-Content -Path $Out -Encoding utf8
    Write-Host "`nwritten: $Out"
}
