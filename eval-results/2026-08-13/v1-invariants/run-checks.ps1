<#
V1.3 gate checks, SERIAL on purpose (R-T5 / the handoff's trap: two `dotnet test` hosts at once
print "Test host process crashed" next to two "Passed!" lines -- a false red).
#>
$ErrorActionPreference = 'Continue'
$repo = 'C:/Code/DevContext2-engine'

Write-Host "=== 1/4 CORE UNIT+INTEGRATION (Category!=Eval) ==="
& dotnet test "$repo/tests/DevContext.Core.Tests" --filter "Category!=Eval" --no-build 2>&1 | Select-Object -Last 6
Write-Host "CORE_EXIT=$LASTEXITCODE"

Write-Host "=== 2/4 SERVER (Category!=Eval) ==="
& dotnet test "$repo/tests/DevContext.Server.Tests" --filter "Category!=Eval" --no-build 2>&1 | Select-Object -Last 6
Write-Host "SERVER_EXIT=$LASTEXITCODE"

Write-Host "=== 3/4 EVAL MATRIX (Category=Eval) ==="
& dotnet test "$repo/tests/DevContext.Core.Tests" --filter "Category=Eval" --no-build 2>&1 | Select-Object -Last 20
Write-Host "EVAL_EXIT=$LASTEXITCODE"

Write-Host "=== 4/4 LOOM GUARDS ==="
& powershell -NoProfile -ExecutionPolicy Bypass -File "$repo/scripts/loom-guards.ps1" 2>&1 | Select-Object -Last 30
Write-Host "GUARDS_EXIT=$LASTEXITCODE"

Write-Host "CHECKS DONE"
