# G6.1 RED-THEN-GREEN PROOF. Restores ONLY ArchitectureStyleDetector.cs to its pre-G6.1 state,
# rebuilds, runs the new invariant tests (must FAIL), then puts the fixed file back and re-runs
# (must PASS). ASCII only; absolute paths only.
$ErrorActionPreference = 'Continue'
$repo = 'C:\code\DevContext2'
$src  = 'src/DevContext.Core/Extractors/Generic/ArchitectureStyleDetector.cs'
$abs  = Join-Path $repo $src
$keep = 'C:\Users\shahi\AppData\Local\Temp\g6-detector-fixed.cs'
$filter = 'FullyQualifiedName~ServiceVocabularyTests'

Copy-Item $abs $keep -Force
Write-Host "=== RED: restoring the pre-G6.1 detector ==="
& git -C $repo checkout HEAD -- $src
& dotnet build (Join-Path $repo 'tests\DevContext.Core.Tests') -clp:ErrorsOnly
& dotnet test (Join-Path $repo 'tests\DevContext.Core.Tests') --no-build --filter $filter
Write-Host "RED exit=$LASTEXITCODE   (non-zero is the point)"

Write-Host "=== GREEN: putting the G6.1 detector back ==="
Copy-Item $keep $abs -Force
& dotnet build (Join-Path $repo 'tests\DevContext.Core.Tests') -clp:ErrorsOnly
& dotnet test (Join-Path $repo 'tests\DevContext.Core.Tests') --no-build --filter $filter
Write-Host "GREEN exit=$LASTEXITCODE   (zero is the point)"
Write-Host "DONE"
