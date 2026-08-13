<#
V1.3 RED-FIRST. Runs the NEW invariant tests against the PRE-FIX ENGINE.

Only the three BEHAVIOURAL files are stashed:
    CodeGraph.cs             (the choke point that refuses)
    GraphBuilder.Seams.cs    (two Kind-gated resolutions)
    PlainCallDetector.cs     (the third)
SymbolCanon.cs stays, because its V1.3 addition is two PURE predicates called from nowhere in the
stashed set -- inert on the pre-fix build. Stashing it too only produces a COMPILE error, which
proves the helper is new and proves nothing about the graph; leaving it in lets the behavioural
assertions actually run and FAIL, which is the point of a red-first.

A backup patch is written first, so the fix survives even if the stash round-trip goes wrong.
#>
$ErrorActionPreference = 'Continue'
$repo = 'C:/Code/DevContext2-engine'
$src  = @(
    'src/DevContext.Core/Graph/CodeGraph.cs',
    'src/DevContext.Core/Graph/GraphBuilder.Seams.cs',
    'src/DevContext.Core/Graph2/Seams/PlainCallDetector.cs'
)
$filter = 'FullyQualifiedName~GraphInvariantTests'

git -C $repo diff -- $src 'src/DevContext.Core/Graph2/SymbolCanon.cs' |
    Out-File -Encoding utf8 "$repo/eval-results/2026-08-13/v1-invariants/v13-fix.patch"
Write-Host "backup patch written"

Write-Host "=== STASHING THE BEHAVIOURAL FIX (tests + SymbolCanon predicates stay) ==="
git -C $repo stash push -m 'v13-redfirst' -- $src

Write-Host "=== PRE-FIX RUN (must FAIL) ==="
& dotnet test "$repo/tests/DevContext.Core.Tests" --filter $filter -clp:ErrorsOnly 2>&1 | Select-Object -Last 45
Write-Host "PREFIX_EXIT=$LASTEXITCODE"

Write-Host "=== RESTORING THE FIX ==="
git -C $repo stash pop
git -C $repo status --short

Write-Host "=== POST-FIX RUN (must PASS) ==="
& dotnet test "$repo/tests/DevContext.Core.Tests" --filter $filter -clp:ErrorsOnly 2>&1 | Select-Object -Last 12
Write-Host "POSTFIX_EXIT=$LASTEXITCODE"
Write-Host "RED-FIRST DONE"
