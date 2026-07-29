# G6.1 engine verification. ASCII only, absolute paths only.
$ErrorActionPreference = 'Continue'
Set-Location C:\code\DevContext2

Write-Host '== contract sweep =='
& powershell -NoProfile -ExecutionPolicy Bypass -File C:\code\DevContext2\eval\contract-sweep.ps1
Write-Host ('sweep exit=' + $LASTEXITCODE)

Write-Host '== loom guards =='
& powershell -NoProfile -ExecutionPolicy Bypass -File C:\code\DevContext2\scripts\loom-guards.ps1
Write-Host ('guards exit=' + $LASTEXITCODE)

Write-Host '== fast unit+integration (Category!=Eval) =='
& dotnet test C:\code\DevContext2\DevContext.slnx --no-build --filter 'Category!=Eval'
Write-Host ('tests exit=' + $LASTEXITCODE)
Write-Host 'DONE-VERIFY'
