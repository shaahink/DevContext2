# V1.1 before/after driver. Builds a PRE-CHANGE CLI from a worktree at HEAD, then runs the same
# four reads with each binary against ONE pristine target tree (the worktree itself, whose content
# neither run modifies). Long child - launch through `conductor bg`.
$ErrorActionPreference = 'Continue'
$repo = 'C:\Code\DevContext2-engine'
$wt   = 'C:\Code\DevContext2-v11-before'
$cap  = Join-Path $repo 'eval-results\2026-08-13\v1-vocabulary\capture.ps1'

if (-not (Test-Path $wt)) {
    Write-Host "== creating pre-change worktree at HEAD =="
    git -C $repo worktree add --detach $wt HEAD
    Write-Host "worktree exit $LASTEXITCODE"
}

Write-Host "== building PRE-CHANGE cli in the worktree =="
dotnet build (Join-Path $wt 'src\DevContext.Cli') -clp:ErrorsOnly
Write-Host "build exit $LASTEXITCODE"

$beforeCli = Join-Path $wt 'src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe'
$afterCli  = Join-Path $repo 'src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe'

& powershell -NoProfile -ExecutionPolicy Bypass -File $cap -Tag before -Cli $beforeCli -Target $wt
& powershell -NoProfile -ExecutionPolicy Bypass -File $cap -Tag after  -Cli $afterCli  -Target $wt
Write-Host "ALL DONE"
