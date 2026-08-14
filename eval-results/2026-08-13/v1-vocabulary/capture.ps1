# V1.1 evidence capture - the SAME four CLI reads, run by the PRE-CHANGE binary and the POST-CHANGE
# binary against ONE pristine target tree, so any difference is attributable to the change alone.
#
#   capture.ps1 -Tag before -Cli <path to pre-change DevContext.Cli.exe>  -Target <repo>
#   capture.ps1 -Tag after  -Cli <path to post-change DevContext.Cli.exe> -Target <repo>
#
# NOTE: `query` takes its OP positionally (`query stats --path X`); `--op stats` is not a thing.
param(
    [Parameter(Mandatory)][string]$Tag,
    [Parameter(Mandatory)][string]$Cli,
    [Parameter(Mandatory)][string]$Target)

$out = 'C:\Code\DevContext2-engine\eval-results\2026-08-13\v1-vocabulary'

function Run-Cli {
    param([string]$File, [string[]]$CliArgs)
    $path = Join-Path $out $File
    # Out-File -Encoding utf8 (not '>') - PS 5.1 redirection writes UTF-16, which no JSON parser reads.
    & $Cli @CliArgs 2>&1 | Out-File -FilePath $path -Encoding utf8
    Write-Host "$File exit $LASTEXITCODE"
}

Write-Host "== $Tag : cli=$Cli target=$Target =="
Run-Cli "$Tag-report.md"     @('analyze', $Target)
Run-Cli "$Tag-analyze.json"  @('analyze', $Target, '--format', 'json')
Run-Cli "$Tag-stats.json"    @('query', 'stats', '--path', $Target)
Run-Cli "$Tag-graphdump.json" @('query', 'graphdump', '--path', $Target)
Write-Host "DONE $Tag"
