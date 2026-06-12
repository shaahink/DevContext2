<#
.SYNOPSIS
    DevContext gate script - self-validation invariants, unit tests, eval suite.
.DESCRIPTION
    Windows PowerShell 5.1 compatible. Run before finishing any session.
    Exits with 0 on PASS, non-zero on FAIL.
#>

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

$exitCode = 0

function Write-Step {
    param([string]$Text)
    Write-Host ""
    Write-Host "--- $Text ---" -ForegroundColor Cyan
}

function Write-Pass {
    param([string]$Text)
    Write-Host "  PASS  $Text" -ForegroundColor Green
}

function Write-Fail {
    param([string]$Text, [int]$Step)
    Write-Host "  FAIL  $Text" -ForegroundColor Red
    $script:exitCode = $Step
}

# Step 1: Build
Write-Step "Step 1: Build solution"
$sln = Join-Path $repoRoot "DevContext.slnx"
$buildResult = dotnet build $sln 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host $buildResult
    Write-Fail "Build failed" -Step 1
    Write-Host ""
    Write-Host "GATE: FAIL (step 1 - build)" -ForegroundColor Red
    exit 1
}
Write-Pass "Build succeeded"

# Step 2: Fast unit tests (exclude Eval, CliSmoke)
Write-Step "Step 2: Fast unit tests"
$filterArg = "Category!=Eval&Category!=CliSmoke"
$testResult = dotnet test $sln --filter $filterArg --no-build 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host $testResult
    Write-Fail "Fast tests failed" -Step 2
    Write-Host ""
    Write-Host "GATE: FAIL (step 2 - fast tests)" -ForegroundColor Red
    exit 2
}
Write-Pass "Fast tests passed"

# Step 3: Eval tests
Write-Step "Step 3: Eval expectation tests"
$evalResult = dotnet test $sln --filter "Category=Eval" --no-build 2>&1
$evalExit = $LASTEXITCODE
Write-Host $evalResult

$aspirationalLines = $evalResult | Select-String "ASPIRATIONAL-FAIL" | ForEach-Object { $_.Line }
if ($aspirationalLines) {
    Write-Host ""
    Write-Host "Aspirational failures (non-blocking):" -ForegroundColor Yellow
    foreach ($line in $aspirationalLines) {
        Write-Host "  $line" -ForegroundColor Yellow
    }
}

if ($evalExit -ne 0) {
    Write-Fail "Eval tests failed" -Step 3
    Write-Host ""
    Write-Host "GATE: FAIL (step 3 - eval)" -ForegroundColor Red
    exit 3
}
Write-Pass "Eval tests passed"

# Step 4: CLI strict-mode matrix
Write-Step "Step 4: CLI --strict matrix"
$cliProject = Join-Path $repoRoot "src\DevContext.Cli"
$testDir = Join-Path $repoRoot "tests\fixtures\MinimalApiProject"

if (-not (Test-Path $testDir)) {
    $testDir = $repoRoot
}

$cliMatrix = @(
    @{ Name = "analyze . --strict";              Args = @("analyze", $testDir, "--strict") }
    @{ Name = "analyze --format json --strict";  Args = @("analyze", $testDir, "--format", "json", "--strict") }
    @{ Name = "analyze --format html --strict";  Args = @("analyze", $testDir, "--format", "html", "--strict") }
    @{ Name = "analyze --dry-run";               Args = @("analyze", $testDir, "--dry-run") }
    @{ Name = "analyze --max-tokens 2000 --strict"; Args = @("analyze", $testDir, "--max-tokens", "2000", "--strict") }
)

$cliFailed = 0
foreach ($entry in $cliMatrix) {
    Write-Host "  Running: $($entry.Name)..."
    $cliOutput = & dotnet run --no-build --project $cliProject -- $entry.Args 2>&1
    $cliExit = $LASTEXITCODE

    if ($cliExit -eq 0) {
        Write-Host "    exit 0 (clean)" -ForegroundColor Green
    } elseif ($cliExit -eq 2 -and $entry.Name.Contains("--strict")) {
        Write-Host "    exit 2 (self-check failures)" -ForegroundColor Yellow
    } elseif ($entry.Name -eq "analyze --dry-run") {
        if ($cliExit -eq 0) {
            Write-Host "    exit 0 (dry-run)" -ForegroundColor Green
        } else {
            Write-Host "    exit $cliExit (unexpected)" -ForegroundColor Red
            $cliFailed++
        }
    } else {
        Write-Host "    exit $cliExit" -ForegroundColor Red
        $cliFailed++
    }
}

if ($cliFailed -gt 0) {
    Write-Fail "$cliFailed CLI command(s) failed" -Step 4
    Write-Host ""
    Write-Host "GATE: FAIL (step 4 - CLI matrix)" -ForegroundColor Red
    exit 4
}
Write-Pass "CLI matrix: all commands ran successfully"

# Final
Write-Host ""
Write-Host "GATE: PASS" -ForegroundColor Green
exit 0
