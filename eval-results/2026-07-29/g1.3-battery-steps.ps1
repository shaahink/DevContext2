# G1.3 close — run the tests THE WAY THE BATTERY RUNS THEM (eval/gates.ps1 Step 2 + Step 2b).
# Step 2 excludes Category=McpQa on purpose: that one is a 3-minute external `node` MCP drive and
# gets its own step, alone. Verifying with a bare "Category!=Eval" pulls it into a 674-test
# parallel run, and under that load the server the MCP spawns dies before it can bind.
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2

Write-Host "=== Step 2: fast unit tests (battery filter) ==="
dotnet test DevContext.slnx --no-build --filter "Category!=Eval&Category!=CliSmoke&Category!=McpQa" 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-step2-unit.txt
Write-Host "STEP2_EXIT=$LASTEXITCODE"

Write-Host "=== Step 2b: MCP QA drive (alone, as the battery runs it) ==="
dotnet test DevContext.slnx --no-build --filter "Category=McpQa" 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-step2b-mcpqa.txt
Write-Host "STEP2B_EXIT=$LASTEXITCODE"

Write-Host "=== Step 1a: contract sweep ==="
powershell -NoProfile -ExecutionPolicy Bypass -File C:/code/DevContext2/eval/contract-sweep.ps1 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-contract-sweep.txt
Write-Host "SWEEP_EXIT=$LASTEXITCODE"

Write-Host "=== loom guards ==="
powershell -NoProfile -ExecutionPolicy Bypass -File C:/code/DevContext2/scripts/loom-guards.ps1 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-guards.txt
Write-Host "GUARDS_EXIT=$LASTEXITCODE"
