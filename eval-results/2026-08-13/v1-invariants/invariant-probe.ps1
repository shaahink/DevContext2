<#
V1.3 - the two standing invariants (pre-release, backlog #7 rider and #18).

Reads `query graphdump` for each pole and counts VIOLATIONS of two invariants that must hold of
any graph the engine produces:

  INV-A (#7 rider)  no node carries kind Type with a MEMBER id.
                    A member key is "Owner::Name" (SymbolCanon's member separator). Hangfire's
                    explicit interface implementation `string IStackTraceFormatter<string>.Type(
                    string)` shipped as  Type:Hangfire.StackTraceHtmlFragments::Type(1)  -- kind
                    Type, member id -- and 26 BCL System.Type references bound onto it, making a
                    dashboard formatter fragment the repo's #5 "wiring hub".

  INV-B (#18)       lambda / expression TEXT never becomes a node key or title.
                    Real shipped Type nodes: an 8-line rate-limiter lambda (dotnet-podcasts), a
                    20-line DI lambda with its comments (CleanArchitecture), `new
                    System.IO.StringWriter()` (MediatR), `opt => opt.AddMaps(typeof(Source))`
                    (AutoMapper), the string literal `"self"` (TodoApi).

TWO RULES ARE REPORTED, and the difference between them is the point.

  ENFORCED  "expression syntax": the text contains any of  =>  (  )  {  }  ;  =  "  '  or a
            newline/tab. This is the rule the engine enforces (SymbolCanon.IsExpressionText).
            It is exactly what #18 names -- the audit's own words are that GraphBuilder.Seams
            "already filters some of these shapes ... the filter is incomplete and is not applied
            on every path". This completes it and moves it to the one choke point.

  WIDER     "not type-name shaped": the text does not parse as a C# type reference (dotted
            identifier segments, optional `N arity, optional generic list, optional array or
            nullable suffix). Reported for HONESTY, NOT enforced -- a queue channel node is
            deliberately keyed by its display text ("feed-queue [AzureStorageQueue]", B5/Prism
            D1.2d), so a whitelist rule would delete real event wiring. The gap between the two
            columns is the residue, and the evidence names it rather than hiding it.

SCOPE NOTE: both rules are checked on Type nodes only. EntryPoint / Service / Store titles are
prose by design ("GET /todos"), so a syntax rule would be a category error there.

Poles are "name=ABSOLUTE_PATH" so nothing depends on a working directory (house trap 1/4).
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
        "Hangfire=C:\Code\eval-poles\Hangfire",
        "TodoApi=C:\Code\DevContext2-engine\eval-repos\TodoApi",
        "DevContext=C:\Code\DevContext2-engine"
    )
)

$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

# ---- ENFORCED rule -------------------------------------------------------------------------
$exprChars = @([char]'=', [char]'(', [char]')', [char]'{', [char]'}', [char]';',
               [char]'"', [char]"'", [char]"`n", [char]"`r", [char]"`t")
function HasExpressionSyntax([string]$t) {
    if ([string]::IsNullOrWhiteSpace($t)) { return $true }
    foreach ($c in $t.ToCharArray()) { if ($exprChars -contains $c) { return $true } }
    return $false
}

# ---- WIDER diagnostic ----------------------------------------------------------------------
function IsIdentSegment([string]$s) {
    if ([string]::IsNullOrEmpty($s)) { return $false }
    $tick = $s.IndexOf([char]96)
    if ($tick -ge 0) {
        $n = $s.Substring($tick + 1)
        if ($n.Length -eq 0) { return $false }
        foreach ($c in $n.ToCharArray()) { if (-not [char]::IsDigit($c)) { return $false } }
        $s = $s.Substring(0, $tick)
        if ($s.Length -eq 0) { return $false }
    }
    if ($s.StartsWith('@')) { $s = $s.Substring(1) }
    if ($s.Length -eq 0) { return $false }
    if (-not ([char]::IsLetter($s[0]) -or $s[0] -eq '_')) { return $false }
    foreach ($c in $s.ToCharArray()) {
        if (-not ([char]::IsLetterOrDigit($c) -or $c -eq '_')) { return $false }
    }
    return $true
}

function SplitTopLevel([string]$s) {
    $parts = New-Object System.Collections.Generic.List[string]
    $depth = 0; $start = 0
    for ($i = 0; $i -lt $s.Length; $i++) {
        $c = $s[$i]
        if ($c -eq '<') { $depth++ }
        elseif ($c -eq '>') { $depth-- }
        elseif ($c -eq ',' -and $depth -eq 0) { $parts.Add($s.Substring($start, $i - $start)) | Out-Null; $start = $i + 1 }
    }
    $parts.Add($s.Substring($start)) | Out-Null
    return $parts
}

function ShapeOk([string]$t) {
    if ($null -eq $t) { return $false }
    $t = $t.Trim()
    if ($t.Length -eq 0) { return $false }
    if ($t.Contains('::')) { return $false }
    while ($true) {
        if ($t.EndsWith('?')) { $t = $t.Substring(0, $t.Length - 1).TrimEnd(); if ($t.Length -eq 0) { return $false }; continue }
        if ($t.EndsWith(']')) {
            $ob = $t.LastIndexOf('[')
            if ($ob -lt 0) { return $false }
            $inner = $t.Substring($ob + 1, $t.Length - $ob - 2)
            if (($inner -replace ',', '').Trim().Length -ne 0) { return $false }
            $t = $t.Substring(0, $ob).TrimEnd(); if ($t.Length -eq 0) { return $false }; continue
        }
        break
    }
    $lt = $t.IndexOf('<')
    if ($lt -ge 0) {
        if (-not $t.EndsWith('>')) { return $false }
        if ($lt -eq 0) { return $false }
        $inner = $t.Substring($lt + 1, $t.Length - $lt - 2)
        $t = $t.Substring(0, $lt)
        foreach ($a in (SplitTopLevel $inner)) {
            $a = $a.Trim()
            if ($a.Length -eq 0) { continue }
            if (-not (ShapeOk $a)) { return $false }
        }
    }
    foreach ($seg in $t.Split('.')) { if (-not (IsIdentSegment $seg)) { return $false } }
    return $true
}

function OneLine([string]$s) {
    $one = ($s -replace "`r", '' -replace "`n", ' \n ')
    if ($one.Length -gt 110) { $one = $one.Substring(0, 110) + ' ...' }
    return $one
}

Emit "V1.3 GRAPH INVARIANT PROBE"
Emit "  INV-A    no node has kind Type with a MEMBER id (key contains '::')"
Emit "  INV-B    no Type node key/title is expression text  [ENFORCED rule: expression syntax]"
Emit "  (wider)  Type key/title not type-name shaped        [DIAGNOSTIC only - channels live here]"
Emit "cli: $Cli"
Emit "cli mtime: $((Get-Item $Cli).LastWriteTime.ToString('s'))"
Emit ""

$tA = 0; $tB = 0; $tW = 0; $tType = 0; $tAll = 0
foreach ($pole in $Poles) {
    $eq = $pole.IndexOf('=')
    $name = $pole.Substring(0, $eq)
    $repo = $pole.Substring($eq + 1)
    if (-not (Test-Path $repo)) { Emit "[$name] SKIP (no repo at $repo)"; continue }
    $d = (& $Cli query graphdump --path $repo 2>$null) -join "`n"
    if (-not $d) { Emit "[$name] EMPTY"; continue }

    $vA = New-Object System.Collections.Generic.List[string]
    $vB = New-Object System.Collections.Generic.List[string]
    $vW = New-Object System.Collections.Generic.List[string]
    $nType = 0; $nAll = 0
    foreach ($m in [regex]::Matches($d, '\{"id":"([^"]+)","kind":"([^"]+)","title":"([^"]*)"')) {
        $id    = [regex]::Unescape($m.Groups[1].Value)
        $kind  = $m.Groups[2].Value
        $title = [regex]::Unescape($m.Groups[3].Value)
        $key   = $id.Substring($id.IndexOf(':') + 1)
        $nAll++
        if ($kind -ne 'Type') { continue }
        $nType++
        if ($key.Contains('::')) { $vA.Add($key) | Out-Null; continue }
        if ((HasExpressionSyntax $key) -or (HasExpressionSyntax $title)) { $vB.Add((OneLine $key)) | Out-Null; continue }
        if (-not ((ShapeOk $key) -and (ShapeOk $title))) { $vW.Add((OneLine "$key | title='$title'")) | Out-Null }
    }
    $tA += $vA.Count; $tB += $vB.Count; $tW += $vW.Count; $tType += $nType; $tAll += $nAll
    Emit ("POLE {0,-18} nodes {1,6}  Type {2,6}   INV-A {3,4}   INV-B {4,4}   (wider {5,4})" -f $name, $nAll, $nType, $vA.Count, $vB.Count, $vW.Count)
    foreach ($x in $vA) { Emit "      A: $x" }
    foreach ($x in $vB) { Emit "      B: $x" }
    foreach ($x in $vW) { Emit "      w: $x" }
    Emit ""
}

Emit "=== TOTALS ==="
Emit ("  nodes {0}   Type nodes {1}   INV-A {2}   INV-B {3}   (wider {4})" -f $tAll, $tType, $tA, $tB, $tW)

if ($Out) {
    $dir = Split-Path -Parent $Out
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    $lines | Set-Content -Path $Out -Encoding utf8
    Write-Host "`nwritten: $Out"
}
