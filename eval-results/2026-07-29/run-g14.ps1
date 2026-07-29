# G1.4 evidence runner (R4 §1 items 6+7) — the two cases the checkpoint is judged on.
# Usage: powershell -File run-g14.ps1 <outDirName>     e.g. mcp-r4-g14-before | mcp-r4-g14
# The server exe is PINNED so an installed copy cannot shadow the build (ServerShim priority trap).
param([Parameter(Mandatory = $true)][string]$OutName)

$ErrorActionPreference = "Continue"
$repo = "C:/code/DevContext2"
$env:DEVCONTEXT_SERVER = "$repo/src/DevContext.Server/bin/Debug/net10.0/DevContext.Server.exe"
$out = "$repo/eval-results/2026-07-29/$OutName"
New-Item -ItemType Directory -Force -Path $out | Out-Null

foreach ($case in @("find-kind", "analyze-honesty")) {
  Write-Host "=== case: $case ==="
  Set-Location $repo
  & node "$repo/eval/mcp-qa/drive-r4.js" $case $out 2>&1 | Tee-Object -FilePath "$out/$case.txt"
  Write-Host "exit=$LASTEXITCODE"
}
Write-Host "=== done ==="
