# G1.3 / bug #1 — two runs of the FIXED harness.
#   1. dogfood, as the gate runs it: must still score 12/12 (no regression from the analyze rewrite).
#   2. a repo the server has never analysed in this process, whose cold analyse takes far longer than
#      the old shared 45s timeout. The QA questions are eShop-specific so the SCORE is meaningless
#      here — what is being measured is the analyze step: does the call return its own handle, and
#      how long did it take. That is the mechanism the 45s cap used to break.
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
$env:DEVCONTEXT_SERVER = "C:/code/DevContext2/src/DevContext.Server/bin/Debug/net10.0/DevContext.Server.exe"

Write-Host "=== 1. dogfood (gate repo, --quiet as the test runs it) ==="
node C:/code/DevContext2/eval/mcp-qa/run.js --quiet 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-mcpqa-dogfood.txt
Write-Host "EXIT=$LASTEXITCODE"

Write-Host "=== 2. cold analyse of an unseen repo (analyze step only) ==="
node C:/code/DevContext2/eval/mcp-qa/run.js --repo C:/code/DevContext2/eval-repos/bitwarden-server 2>&1 |
  Select-String -Pattern "Analyzing|still analyzing|Analyzed in|analyze call did not return|FAILED" |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-mcpqa-coldpath.txt
Write-Host "=== done ==="
