# Faithful repro of the McpQaGateTests red: EXACTLY what the test runs — no DEVCONTEXT_SERVER
# pinned, repo root as the working directory, --quiet, stdout and stderr both captured.
# (The first repro set DEVCONTEXT_SERVER and therefore was NOT the same command; it passed.)
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
Remove-Item Env:DEVCONTEXT_SERVER -ErrorAction SilentlyContinue
node C:/code/DevContext2/eval/mcp-qa/run.js --quiet 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-mcpqa-repro2.txt
Write-Host "EXIT=$LASTEXITCODE"
