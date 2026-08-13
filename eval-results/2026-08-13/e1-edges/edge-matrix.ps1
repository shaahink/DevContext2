<#
E1.1 (#11) - the static-type-name-receiver edge matrix.

Runs `query graphdump` on each pole and reports the numbers the fix is supposed to move, plus the
ones it must NOT move. One row per pole; run it once on the pre-fix build and once on the post-fix
build and diff the rows.

  nodes / edges       total graph size (must not collapse)
  calls               edges of kind Calls
  m2m / m2t           Calls split by endpoint shape - member->member (CallGraphBinder spine, entry
                      closure scoped) vs member->TYPE (PlainCallDetector seam, all in-scope bodies).
                      #11 lives in BOTH producers; a fix in one moves only one column.
  verified            Calls edges whose resolution is Semantic (V1.1 / backlog #25 definition)
  approx              Calls edges whose resolution is anything else

Poles are "name=ABSOLUTE_PATH" so nothing depends on a working directory (house trap 1/4).
CLI paths must be absolute or the CLI parses them as a GitHub repo (house trap 4).
#>
param(
    [string]$Label = "run",
    [string]$Out = "",
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
    # NOTE: pass -Poles only with a comma-separated array. `-Poles "a" "b"` binds "b" POSITIONALLY
    # to -Cli (PowerShell does not treat space-separated strings as one array), which silently runs
    # the sweep with a pole string as the CLI path. Prefer editing this default list.
)

$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

Emit "E1.1 edge matrix - label=$Label"
Emit "cli=$Cli"
Emit ("cli-core-dll-written=" + (Get-Item (Join-Path (Split-Path $Cli) "DevContext.Core.dll")).LastWriteTime.ToString("s"))
Emit ""
Emit ("{0,-18} {1,7} {2,7} {3,7} {4,7} {5,7} {6,9} {7,8}" -f "pole", "nodes", "edges", "calls", "m2m", "m2t", "verified", "approx")

foreach ($p in $Poles) {
    $name = $p.Substring(0, $p.IndexOf('='))
    $path = $p.Substring($p.IndexOf('=') + 1)
    if (-not (Test-Path $path)) { Emit ("{0,-18} MISSING {1}" -f $name, $path); continue }

    $raw = (& $Cli query graphdump --path $path 2>$null) -join "`n"
    if ([string]::IsNullOrWhiteSpace($raw)) { Emit ("{0,-18} NO-OUTPUT" -f $name); continue }
    try { $j = $raw | ConvertFrom-Json } catch { Emit ("{0,-18} PARSE-FAIL" -f $name); continue }

    $calls = @($j.edges | Where-Object { $_.kind -eq 'Calls' })
    $m2m = @($calls | Where-Object { $_.from -like 'Member:*' -and $_.to -like 'Member:*' })
    $m2t = @($calls | Where-Object { $_.from -like 'Member:*' -and $_.to -like 'Type:*' })
    $ver = @($calls | Where-Object { $_.resolution -eq 'Semantic' })
    $apx = $calls.Count - $ver.Count

    Emit ("{0,-18} {1,7} {2,7} {3,7} {4,7} {5,7} {6,9} {7,8}" -f `
        $name, $j.nodes.Count, $j.edges.Count, $calls.Count, $m2m.Count, $m2t.Count, $ver.Count, $apx)

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
