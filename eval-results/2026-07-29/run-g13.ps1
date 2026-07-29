# G1.3 evidence runner — runs the three drive-r4 cases sequentially against a PINNED server exe.
# Usage: powershell -File eval-results/2026-07-29/run-g13.ps1 <outDirName>
param([string]$OutName = "mcp-r4-g13-before")

$ErrorActionPreference = "Continue"
$repo = "C:/code/DevContext2"
$env:DEVCONTEXT_SERVER = "$repo/src/DevContext.Server/bin/Debug/net10.0/DevContext.Server.exe"
$out = "$repo/eval-results/2026-07-29/$OutName"
New-Item -ItemType Directory -Force -Path $out | Out-Null

foreach ($case in @("envelope", "retarget", "glyphs")) {
  Write-Host "=== case: $case ==="
  Push-Location $repo
  & node "$repo/eval/mcp-qa/drive-r4.js" $case $out 2>&1 | Tee-Object -FilePath "$out/$case.txt"
  Write-Host "exit=$LASTEXITCODE"
  Pop-Location
}
Write-Host "=== done ==="
