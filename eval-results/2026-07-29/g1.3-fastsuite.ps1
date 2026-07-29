# G1.3 — build, then the full fast suite (unit + integration, Category!=Eval).
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
dotnet build DevContext.slnx -clp:ErrorsOnly 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-build2.txt
Write-Host "BUILD_EXIT=$LASTEXITCODE"
dotnet test DevContext.slnx --no-build --filter "Category!=Eval" 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-unit-tests.txt
Write-Host "TEST_EXIT=$LASTEXITCODE"
