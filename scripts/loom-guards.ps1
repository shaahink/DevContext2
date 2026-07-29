# Loom guards -- ban-list pattern checker for the Loom identity spine + regex funeral.
# Run from the repo root. Exits 0 when clean, non-zero when banned patterns found.
# Guards tighten progressively: new patterns added each stage, exceptions removed
# as the old paths die. CI hook: `powershell -File scripts/loom-guards.ps1`.
#
# L1 rules (enforced where applicable):
#   1. No Regex usage in src/DevContext.Core/Graph/ (all body-scan regexes died in L2.3)
#   2. No `SymbolId(` construction outside Graph2/ or test code (enforced)
#   3. No `fqns[0]` in Graph2/ (enforced -- SymbolTable uses Candidates, never picks)
# L2.3 rules:
#   4. No `using System.Text.RegularExpressions` in Core/Graph (all regexes banished)
# L3+ will additionally enforce:
#   5. No `NodeId.ForType(` in Graph/ (the old path dies)
# D5 (L0.4): Truth gate wired into battery. Runs Category=Truth tests and
#   fails on actual failures (skips are the pending ratchet — green).
#   This prevents silent truth regressions from reaching the branch.
# Prism D1.0d rule:
#   6. No NEW bare `catch` in src/DevContext.Core (empty or comment-only body -- a swallow).
#      Existing swallows are grandfathered per-file below until Prism D2 (silent-failure
#      amnesty) converts them to catch-log-count and drives every allowance to 0.
#      The table is a MAX per file: adding one anywhere fails the gate.
# graph-v2 G5 s18 rule:
#   7. No test writes the process-wide DEVCONTEXT_CACHE_ROOT (a shared variable cannot isolate
#      concurrent xUnit collections — pass ServerOptions.SnapshotCacheRoot instead).

param(
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$failures = 0

function Check-Pattern {
    param([string]$Pattern, [string]$Label, [string]$Path)
    $files = Get-ChildItem -LiteralPath $Path -Recurse -Filter *.cs -ErrorAction SilentlyContinue
    if (-not $files) { return }
    $matches = Select-String -LiteralPath $files.FullName -Pattern $Pattern -SimpleMatch -CaseSensitive -ErrorAction SilentlyContinue
    foreach ($m in $matches) {
        $rel = $m.Path.Substring($repoRoot.ToString().Length + 1)
        if (-not $Quiet) { Write-Host "  BANNED: $Label in ${rel}:$($m.LineNumber)" }
        $script:failures++
    }
}

Write-Host "== loom-guards -- L2.3 ban check ==" -ForegroundColor Gray

$graphDir = Join-Path $repoRoot 'src/DevContext.Core/Graph'
$graph2Dir = Join-Path $repoRoot 'src/DevContext.Core/Graph2'
$coreDir = Join-Path $repoRoot 'src/DevContext.Core'
$testsDir = Join-Path $repoRoot 'tests'

# Rule 1: No `using System.Text.RegularExpressions` in Core/Graph/ (L2.3 — the regex funeral)
Check-Pattern -Pattern 'System.Text.RegularExpressions' -Label 'Regex import in Core/Graph' -Path $graphDir

# Rule 2: No `SymbolId(` construction outside Graph2/ or test code
$allSrcFiles = Get-ChildItem -LiteralPath $coreDir -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\Graph2\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
foreach ($f in $allSrcFiles) {
    $lines = Select-String -LiteralPath $f.FullName -Pattern 'new SymbolId(' -SimpleMatch -CaseSensitive 2>$null
    foreach ($m in $lines) {
        $rel = $m.Path.Substring($repoRoot.ToString().Length + 1)
        if (-not $Quiet) { Write-Host "  BANNED: new SymbolId in ${rel}:$($m.LineNumber) (only allowed in Graph2/ or test code)" }
        $script:failures++
    }
}

# Rule 3: No `fqns[0]` in Graph2/ (SymbolTable uses Candidates, never picks)
Check-Pattern -Pattern 'fqns[0]' -Label 'fqns[0] in Graph2/' -Path $graph2Dir

# Rule 6 (Prism D1.0d): no bare catch in Core — empty or comment-only body, across lines.
# J1 (Prism D2) drove the grandfathered 2026-07-17 census (30) to ZERO: every former swallow now
# reports through PipelineDiagnostics.Swallowed(source, category, ex). The allowance stays empty
# forever — new swallows must count themselves the same way.
$bareCatchAllowance = @{}
$bareCatchPattern = [regex]::new(
    'catch(\s*\([^)]*\))?\s*\{\s*(?:(?://[^\r\n]*|/\*.*?\*/)\s*)*\}',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
$coreFiles = Get-ChildItem -LiteralPath $coreDir -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
foreach ($f in $coreFiles) {
    $raw = [System.IO.File]::ReadAllText($f.FullName)
    $count = $bareCatchPattern.Matches($raw).Count
    if ($count -eq 0) { continue }
    $relCore = $f.FullName.Substring($coreDir.Length + 1)
    $allowed = 0
    if ($bareCatchAllowance.ContainsKey($relCore)) { $allowed = $bareCatchAllowance[$relCore] }
    if ($count -gt $allowed) {
        $extra = $count - $allowed
        if (-not $Quiet) { Write-Host "  BANNED: $extra new bare catch in src/DevContext.Core/${relCore} ($count found, $allowed grandfathered until D2)" }
        $script:failures += $extra
    }
}

# Rule 7 (graph-v2 G5 s18): no test may redirect the snapshot cache by writing the process-wide
# DEVCONTEXT_CACHE_ROOT. The variable is process-scoped and the server test assembly runs five xUnit
# collections concurrently, so five owners of one variable meant whoever constructed last decided
# where everyone's snapshots went — measured as a 5-in-15 red on AnalyzeCacheTruthTests, and a gate
# that is a coin flip is worse than a gate that is missing. Hand the root to the host instead:
# `new ServerOptions { SnapshotCacheRoot = <dir> }`. The census after the fix is ZERO, so this is a
# flat ban with no allowance. (Reading the variable is fine; only writing it from a test is banned,
# and separate PROCESSES — eval/mcp-qa/drive-r4.js — are unaffected.)
$testFiles = Get-ChildItem -LiteralPath $testsDir -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
foreach ($f in $testFiles) {
    $hits = Select-String -LiteralPath $f.FullName -Pattern 'SetEnvironmentVariable("DEVCONTEXT_CACHE_ROOT"' -SimpleMatch -CaseSensitive 2>$null
    foreach ($m in $hits) {
        $rel = $m.Path.Substring($repoRoot.ToString().Length + 1)
        if (-not $Quiet) { Write-Host "  BANNED: test writes the process-wide DEVCONTEXT_CACHE_ROOT in ${rel}:$($m.LineNumber) (use ServerOptions.SnapshotCacheRoot)" }
        $script:failures++
    }
}

# Advisory: count remaining banned patterns in Graph/ (fixed by later stages)
$nodeIdCount = (Select-String -Path "$graphDir\**\*.cs" -Pattern 'NodeId.ForType(' -SimpleMatch -CaseSensitive 2>$null).Count
$fqns0Count = (Select-String -Path "$graphDir\**\*.cs" -Pattern 'fqns[0]' -SimpleMatch 2>$null).Count
if (-not $Quiet -and ($nodeIdCount -gt 0 -or $fqns0Count -gt 0)) {
    Write-Host "  Advisory: $nodeIdCount NodeId.ForType( (fixed by L3) + $fqns0Count fqns[0] (fixed by L3)" -ForegroundColor Yellow
}

# D5 (L0.4): Truth gate — runs Category=Truth tests; failures are banned, skips are ratchets.
Write-Host "== truth gate -- dotnet test Category=Truth ==" -ForegroundColor Gray
$testProject = Join-Path $repoRoot 'tests\DevContext.Core.Tests\DevContext.Core.Tests.csproj'
# Same hazard as eval/gates.ps1's Invoke-NativeCapture (measured 2026-07-29, G5): under
# $ErrorActionPreference = 'Stop', PS 5.1 turns any stderr line this native call writes into a
# TERMINATING NativeCommandError, killing the script with exit 1 before $truthExit is ever read -
# so a green truth suite reads as a dead gate, and a red one loses its failing test names. One
# capture here, so the relaxation is scoped inline rather than via a duplicated helper.
$oldEap = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
try { $truthOutput = & dotnet test $testProject --filter "Category=Truth" --no-build --verbosity normal 2>&1 }
finally { $ErrorActionPreference = $oldEap }
$truthExit = $LASTEXITCODE

# Parse: count actual Failures (not skips). Skipped tests are the pending ratchet.
$failedLines = $truthOutput | Select-String -Pattern 'Failed!|Failed\s*:\s*(?!\s*0)' | Select-Object -First 1
$failedCount = 0
if ($failedLines) {
    $failedLine = $failedLines.Line
    if ($failedLine -match 'Failed:\s*(\d+)') { $failedCount = [int]$matches[1] }
    elseif ($failedLine -match 'Failed!') { $failedCount = 1 }
}

if ($truthExit -ne 0 -or $failedCount -gt 0) {
    Write-Host "  FAILED: $failedCount truth test(s) red. Truth regressions must be fixed before proceeding." -ForegroundColor Red
    $script:failures += $failedCount
} else {
    Write-Host "  PASSED: 0 truth failures (skips are the pending ratchet)" -ForegroundColor Green
}

if ($failures -gt 0) {
    Write-Host "`n== FAILED: $failures banned patterns found ==" -ForegroundColor Red
    exit 1
}

Write-Host "`n== PASSED: no banned patterns ==" -ForegroundColor Green
exit 0
