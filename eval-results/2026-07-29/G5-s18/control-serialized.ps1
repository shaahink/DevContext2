# G5 s18 — CONTROL for the repro. Identical command to repro-race.ps1 with ONE variable changed:
# xUnit's cross-collection parallelism is turned off. If the flake is cross-collection contention on
# the process-wide DEVCONTEXT_CACHE_ROOT, this run is 10/10 green; if it is anything else, it still flakes.
param([int]$Iterations = 10)

$ErrorActionPreference = 'Continue'
$root = 'C:\code\DevContext2'
$out  = Join-Path $root 'eval-results\2026-07-29\G5-s18'

$fails = 0
for ($i = 1; $i -le $Iterations; $i++) {
    $log = Join-Path $out "control-$i.txt"
    & dotnet test "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" --no-build --nologo `
        --filter "FullyQualifiedName~AnalyzeCacheTruthTests|FullyQualifiedName~HostReleaseTests|FullyQualifiedName~AnalyzeFlowTests" `
        -- xunit.parallelizeTestCollections=false > $log 2>&1
    $code = $LASTEXITCODE
    if ($code -ne 0) { $fails++ }
    "CONTROL ITER $i EXIT $code $(if ($code -ne 0) { 'FAIL -> ' + $log } else { 'pass' })"
}
"CONTROL TOTAL FAILURES: $fails / $Iterations"
