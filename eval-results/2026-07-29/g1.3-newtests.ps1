# G1.3 — the three new cross-surface contract test classes.
$ErrorActionPreference = "Continue"
Set-Location C:/code/DevContext2
$filter = "FullyQualifiedName~McpErrorEnvelopeTests|FullyQualifiedName~McpHandleScopeTests|FullyQualifiedName~SeamVocabularyTests"
dotnet test tests/DevContext.Server.Tests --no-build --filter $filter 2>&1 |
  Tee-Object -FilePath C:/code/DevContext2/eval-results/2026-07-29/g1.3-newtests.txt
Write-Host "EXIT=$LASTEXITCODE"
