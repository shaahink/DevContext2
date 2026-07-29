# G5 s18 — repro harness for the AnalyzeCacheTruthTests DirectoryNotFoundException.
# Runs the Server.Tests assembly N times (parallel collections intact) and records each outcome.
param([int]$Iterations = 15)

$ErrorActionPreference = 'Continue'
$root = 'C:\code\DevContext2'
$out  = Join-Path $root 'eval-results\2026-07-29\G5-s18'
New-Item -ItemType Directory -Force -Path $out | Out-Null

& dotnet build "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" -v q --nologo
"BUILD EXIT: $LASTEXITCODE"

$fails = 0
for ($i = 1; $i -le $Iterations; $i++) {
    $log = Join-Path $out "run-$i.txt"
    & dotnet test "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" --no-build --nologo `
        --filter "FullyQualifiedName~AnalyzeCacheTruthTests|FullyQualifiedName~HostReleaseTests|FullyQualifiedName~AnalyzeFlowTests" `
        > $log 2>&1
    $code = $LASTEXITCODE
    if ($code -ne 0) { $fails++ }
    "ITER $i EXIT $code $(if ($code -ne 0) { 'FAIL -> ' + $log } else { 'pass' })"
}
"TOTAL FAILURES: $fails / $Iterations"
