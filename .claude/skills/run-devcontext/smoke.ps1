#!/usr/bin/env pwsh
# DevContext CLI smoke driver — builds the CLI and drives the two real output
# artifacts (Map + Trace) plus the JSON lens against an in-repo fixture, then
# asserts on exit codes, file content, and the console stats line. This is the
# programmatic handle on the app: a future agent runs THIS, not `dotnet run` by hand.
#
#   powershell -File .claude/skills/run-devcontext/smoke.ps1                 # bundled fixture
#   powershell -File .claude/skills/run-devcontext/smoke.ps1 <path> <focus>  # any local path
#
# Notes that matter (learned the hard way):
#  - Use `-o <file>` for content assertions. The CLI also prints an explanation
#    line ("Overview map (no focus).") and the stats summary to STDOUT, so parsing
#    captured stdout as JSON fails — but the -o file holds ONLY result.Content.
#  - The stats line ("… N nodes · M edges · depth D …") is STDOUT-only (not in the
#    -o file), so capture stdout separately for that check.
#  - Never pipe the CLI to `Select-Object -First N` then read $LASTEXITCODE: the
#    truncated pipe kills dotnet early and reports -1 on success.
#  - Use an ABSOLUTE path. `RepoUrl.Parse` treats a relative "a/b" as a GitHub
#    owner/repo shorthand and tries to clone it.
#  - A TYPE/handler focus (CreateOrderHandler) reliably traces; a minimal-API route
#    focus may fall back to the Map without Roslyn. `--no-roslyn` keeps it fast.

$ErrorActionPreference = 'Stop'
$repo   = (Resolve-Path "$PSScriptRoot/../../..").Path
$target = if ($args.Count -ge 1) { (Resolve-Path $args[0]).Path } else { Join-Path $repo 'tests/fixtures/MinimalApiProject' }
$focus  = if ($args.Count -ge 2) { $args[1] } else { 'CreateOrderHandler' }
$cli    = Join-Path $repo 'src/DevContext.Cli'
$out    = Join-Path ([System.IO.Path]::GetTempPath()) 'dc-smoke'
New-Item -ItemType Directory -Force -Path $out | Out-Null

$fails = 0
function Check([string]$name, [bool]$ok, [string]$detail = '') {
    if ($ok) { Write-Host "  PASS  $name" -ForegroundColor Green }
    else { Write-Host "  FAIL  $name  $detail" -ForegroundColor Red; $script:fails++ }
}

Write-Host "== build CLI ==" -ForegroundColor Cyan
dotnet build $cli -clp:ErrorsOnly | Out-Null
Check 'build' ($LASTEXITCODE -eq 0) "exit $LASTEXITCODE"
$dll = (Get-ChildItem -Path (Join-Path $cli 'bin/Debug') -Recurse -Filter 'DevContext.Cli.dll' | Select-Object -First 1)
if (-not $dll) { Write-Host 'CLI dll not found after build' -ForegroundColor Red; exit 1 }

# Runs the CLI: content -> -o file, stats/explanation -> captured stdout string.
function Run([string]$file, [string[]]$cliArgs) {
    $path = Join-Path $out $file
    $console = (& dotnet $dll.FullName @cliArgs '-o' $path 2>$null | Out-String)
    $code = $LASTEXITCODE
    $content = if (Test-Path $path) { Get-Content $path -Raw -Encoding UTF8 } else { '' }
    return @{ Code = $code; Console = $console; Content = $content; Path = $path }
}

Write-Host "== Map (no focus) ==" -ForegroundColor Cyan
$map = Run 'map.md' @('analyze', $target, '--no-roslyn')
Check 'map exit 0'      ($map.Code -eq 0) "exit $($map.Code)"
Check 'map MAP header'  ($map.Content -match 'MAP\s')
Check 'map STYLE'       ($map.Content -match 'STYLE\s')
Check 'map stats line'  (($map.Console -match '\d+ nodes') -and ($map.Console -match '\d+ edges'))

Write-Host "== Trace (--focus $focus) ==" -ForegroundColor Cyan
$trace = Run 'trace.md' @('analyze', $target, '--focus', $focus, '--no-roslyn')
Check 'trace exit 0'    ($trace.Code -eq 0) "exit $($trace.Code)"
Check 'trace TRACE hdr' ($trace.Content -match 'TRACE\s')
Check 'trace depth'     ($trace.Console -match 'depth \d+')

Write-Host "== JSON lens ==" -ForegroundColor Cyan
$json = Run 'out.json' @('analyze', $target, '--format', 'json', '--no-roslyn')
Check 'json exit 0'     ($json.Code -eq 0) "exit $($json.Code)"
try { $null = $json.Content | ConvertFrom-Json; Check 'json parses' $true } catch { Check 'json parses' $false $_.Exception.Message }

Write-Host ''
if ($fails -eq 0) { Write-Host "ALL PASS  (artifacts in $out)" -ForegroundColor Green; exit 0 }
else { Write-Host "$fails CHECK(S) FAILED" -ForegroundColor Red; exit 1 }
