<#
E1.3 (#7) - the invariant-refusal sweep.

V1.3 made a malformed node a REFUSAL at CodeGraphBuilder.AddNode; E1.3 made the refusal COUNTED.
This runs `analyze --include-diagnostics` on each pole and prints the GraphInvariants line, which
tallies the DISTINCT node keys refused:

  INV-A   kind Type carrying a member id  (backlog #7's shape: Type:Ns.T::Type(1))
  INV-B   expression / lambda TEXT as a node key (backlog #18)

A refusal also drops every edge that wanted the node, so a NON-EMPTY tally names a producer still
minting the shape and the edges lost with it. An EMPTY tally (no GraphInvariants line at all) is the
healthy state - and is the measurement that lets #7 be closed rather than assumed.

Poles are "name=ABSOLUTE_PATH" (house trap 1/4: absolute paths only; `analyze` takes a POSITIONAL
path - `analyze --path` silently analyses the working directory instead).
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
)

$lines = New-Object System.Collections.Generic.List[string]
function Emit($s) { $lines.Add($s) | Out-Null; Write-Host $s }

Emit "E1.3 invariant-refusal sweep - label=$Label"
Emit ("cli-core-dll-written=" + (Get-Item (Join-Path (Split-Path $Cli) "DevContext.Core.dll")).LastWriteTime.ToString("s"))
Emit ""

foreach ($p in $Poles) {
    $name = $p.Substring(0, $p.IndexOf('='))
    $path = $p.Substring($p.IndexOf('=') + 1)
    if (-not (Test-Path $path)) { Emit ("{0,-20} MISSING {1}" -f $name, $path); continue }

    $raw = (& $Cli analyze $path --include-diagnostics --no-cache 2>&1) -join "`n"
    $hits = @([regex]::Matches($raw, 'GraphInvariants[^\r\n]*'))
    if ($hits.Count -eq 0) {
        Emit ("{0,-20} refusals=0 (no GraphInvariants diagnostic)" -f $name)
    } else {
        foreach ($h in $hits) { Emit ("{0,-20} {1}" -f $name, $h.Value.Trim()) }
    }
}

if ($Out -ne "") {
    $dir = Split-Path $Out
    if ($dir -ne "" -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
    $lines | Out-File -Encoding utf8 $Out
    Write-Host "wrote $Out"
}
