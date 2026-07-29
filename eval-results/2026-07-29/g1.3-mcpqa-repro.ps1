# Narrowest repro of the McpQaGateTests red: the exact command the test runs.
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
$env:DEVCONTEXT_SERVER = "C:/code/DevContext2/src/DevContext.Server/bin/Debug/net10.0/DevContext.Server.exe"
node C:/code/DevContext2/eval/mcp-qa/run.js --quiet 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-mcpqa-repro.txt
Write-Host "EXIT=$LASTEXITCODE"
