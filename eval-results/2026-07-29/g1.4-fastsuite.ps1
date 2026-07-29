# G1.4 verification, using the BATTERY'S OWN commands (eval/gates.ps1 Step 2 then Step 2b).
# The bare `Category!=Eval` filter is NOT this: it drags the 3-minute external node MCP drive into
# the parallel unit run and produces a phantom red that cost three sessions. ASCII only.
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
$out = "C:/code/DevContext2/eval-results/2026-07-29"

Write-Host "=== Step 2: fast unit tests (Category!=Eval&Category!=CliSmoke&Category!=McpQa) ==="
dotnet test DevContext.slnx --filter "Category!=Eval&Category!=CliSmoke&Category!=McpQa" --no-build 2>&1 |
  Tee-Object -FilePath "$out/g1.4-step2-unit.txt"
Write-Host "STEP2-EXIT=$LASTEXITCODE"

Write-Host "=== Step 2b: MCP QA gate, alone and serial (Category=McpQa) ==="
dotnet test DevContext.slnx --filter "Category=McpQa" --no-build 2>&1 |
  Tee-Object -FilePath "$out/g1.4-step2b-mcpqa.txt"
Write-Host "STEP2B-EXIT=$LASTEXITCODE"

Write-Host "=== Step 1a: contract sweep (a new proto field with no reader fails here) ==="
powershell -File eval/contract-sweep.ps1 2>&1 | Tee-Object -FilePath "$out/g1.4-contract-sweep.txt"
Write-Host "SWEEP-EXIT=$LASTEXITCODE"

Write-Host "=== loom guards ==="
powershell -File scripts/loom-guards.ps1 2>&1 | Tee-Object -FilePath "$out/g1.4-guards.txt"
Write-Host "GUARDS-EXIT=$LASTEXITCODE"
Write-Host "=== done ==="
