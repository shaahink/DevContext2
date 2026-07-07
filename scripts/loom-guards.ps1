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

# Advisory: count remaining banned patterns in Graph/ (fixed by later stages)
$nodeIdCount = (Select-String -Path "$graphDir\**\*.cs" -Pattern 'NodeId.ForType(' -SimpleMatch -CaseSensitive 2>$null).Count
$fqns0Count = (Select-String -Path "$graphDir\**\*.cs" -Pattern 'fqns[0]' -SimpleMatch 2>$null).Count
if (-not $Quiet -and ($nodeIdCount -gt 0 -or $fqns0Count -gt 0)) {
    Write-Host "  Advisory: $nodeIdCount NodeId.ForType( (fixed by L3) + $fqns0Count fqns[0] (fixed by L3)" -ForegroundColor Yellow
}

if ($failures -gt 0) {
    Write-Host "`n== FAILED: $failures banned patterns found ==" -ForegroundColor Red
    exit 1
}

Write-Host "`n== PASSED: no banned patterns ==" -ForegroundColor Green
exit 0
