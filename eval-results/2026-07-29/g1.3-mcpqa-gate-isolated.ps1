# Is the McpQaGateTests red a load artefact? Run the SAME test alone, with no other test class
# competing for the machine. Same binaries, same command shape as the full suite.
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
dotnet test tests/DevContext.Core.Tests --no-build --filter "FullyQualifiedName~McpQaGateTests" 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-mcpqa-gate-isolated.txt
Write-Host "EXIT=$LASTEXITCODE"
