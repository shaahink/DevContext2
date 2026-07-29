# G1.4 — build gate. ASCII only (PS 5.1 detached-run encoding rule).
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
dotnet build DevContext.slnx -clp:ErrorsOnly
Write-Host "BUILD-EXIT=$LASTEXITCODE"
