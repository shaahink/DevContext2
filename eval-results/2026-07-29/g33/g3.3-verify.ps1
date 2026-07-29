# G3.3 verification: build, the battery's OWN Step 2 filter, guards, contract sweep.
# NOT `--filter "Category!=Eval"` -- that drags the McpQa node drive into a 674-test parallel
# run and produces the false red three G1 sessions chased (PLAN.md S2 note, gates.ps1:136).
$ErrorActionPreference = 'Continue'
Set-Location C:\code\DevContext2

Write-Host '=== build ==='
dotnet build DevContext.slnx -clp:ErrorsOnly
Write-Host "BUILD EXIT=$LASTEXITCODE"

Write-Host '=== step 2: unit + integration (battery filter) ==='
dotnet test DevContext.slnx --no-build --filter 'Category!=Eval&Category!=CliSmoke&Category!=McpQa' -clp:ErrorsOnly
Write-Host "STEP2 EXIT=$LASTEXITCODE"

Write-Host '=== step 2b: MCP QA drive, ALONE ==='
dotnet test DevContext.slnx --no-build --filter 'Category=McpQa' -clp:ErrorsOnly
Write-Host "STEP2B EXIT=$LASTEXITCODE"

Write-Host '=== guards ==='
powershell -File scripts/loom-guards.ps1
Write-Host "GUARDS EXIT=$LASTEXITCODE"

Write-Host '=== contract sweep ==='
powershell -File eval/contract-sweep.ps1
Write-Host "SWEEP EXIT=$LASTEXITCODE"
