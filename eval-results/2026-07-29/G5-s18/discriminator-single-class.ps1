# G5 s18 — DISCRIMINATOR. The control that tried to switch xUnit parallelism off via
# `-- xunit.parallelizeTestCollections=false` was VOID: run durations were identical to the parallel
# runs (5s), i.e. the setting never reached the runner, so its 1/10 failure refutes nothing.
#
# This experiment changes the same variable WITHOUT depending on a runsettings passthrough: run ONLY
# AnalyzeCacheTruthTests. One class = one xUnit collection = strictly sequential, and no other class
# in the assembly is alive to touch the process-wide DEVCONTEXT_CACHE_ROOT.
#   0 failures  -> the flake needs a CONCURRENT NEIGHBOUR: cross-class contention on that variable.
#   >0 failures -> the cause is inside the class itself and the env-var theory is wrong.
param([int]$Iterations = 20)

$ErrorActionPreference = 'Continue'
$root = 'C:\code\DevContext2'
$out  = Join-Path $root 'eval-results\2026-07-29\G5-s18'

$fails = 0
for ($i = 1; $i -le $Iterations; $i++) {
    $log = Join-Path $out "solo-$i.txt"
    & dotnet test "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" --no-build --nologo `
        --filter "FullyQualifiedName~AnalyzeCacheTruthTests" > $log 2>&1
    $code = $LASTEXITCODE
    if ($code -ne 0) { $fails++ }
    "SOLO ITER $i EXIT $code $(if ($code -ne 0) { 'FAIL -> ' + $log } else { 'pass' })"
}
"SOLO TOTAL FAILURES: $fails / $Iterations"
