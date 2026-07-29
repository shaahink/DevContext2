# G1.3 after-state evidence — the same three cases the before-state ran, against the fixed build.
# The server exe is PINNED so an installed copy cannot shadow the build (ServerShim priority trap).
$ErrorActionPreference = "Continue"
$repo = "C:/code/DevContext2"
$env:DEVCONTEXT_SERVER = "$repo/src/DevContext.Server/bin/Debug/net10.0/DevContext.Server.exe"
$out = "$repo/eval-results/2026-07-29/mcp-r4-g13-after"
New-Item -ItemType Directory -Force -Path $out | Out-Null

foreach ($case in @("envelope", "retarget", "glyphs")) {
  Write-Host "=== case: $case ==="
  Set-Location $repo
  & node "$repo/eval/mcp-qa/drive-r4.js" $case $out 2>&1 | Tee-Object -FilePath "$out/$case.txt"
  Write-Host "exit=$LASTEXITCODE"
}
Write-Host "=== done ==="
