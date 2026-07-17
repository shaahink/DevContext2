<#
.SYNOPSIS
    DevContext gate script - self-validation invariants, unit tests, eval suite.
.DESCRIPTION
    Windows PowerShell 5.1 compatible. Run before finishing any session.
    Exits with 0 on PASS, non-zero on FAIL.

    -SkipEval (T5 battery-cadence directive, 2026-07-16): skips Step 3 (the ~11-minute eval
    expectation suite) for MID-STAGE dotnet checkpoints. The verdict line then reads
    "GATE: PASS (FAST - eval skipped; not a merge gate)" so a fast run can never be cited
    as the boundary gate. Stage close-out, pre-push, and pre-merge ALWAYS run the full form.

    -Scope (T7.0 wall-speed rider, 2026-07-16) encodes the battery cadence in the script:
      full   (default) every step INCLUDING the app check (pnpm check) — the only form
             citable at a stage/push/merge boundary.
      engine skips the app check; for engine-only mid-stage checkpoints.
      app    build + app check only (engine suites skipped); for app-only checkpoints.
    Non-full verdicts self-label "not a merge gate", same contract as -SkipEval.

    Step 3 is ENGINE-STAMP CACHED (T7.0): before running, a SHA256 over engine sources,
    Core tests, expectations, and fixtures is compared to eval/.eval-stamp.json (written on
    the last green eval). Identical stamp = the previous verdict transfers - the step is
    skipped and labeled "eval cached". App-only batteries pay ~0 for the eval suite without
    losing full-gate citability. Delete eval/.eval-stamp.json to force a re-run.

    Step 3 runs the expectation theory SPLIT ACROSS TWO test hosts by default (~halves its
    wall time; repos are independent). -SerialEval restores the single-process form.

    POLES ride in Step 3 (Prism D1.2-fix2, 2026-07-17). dogfood-microservices.json and
    shamshir-pole.json are ordinary expectation files pointing at MACHINE-LOCAL absolute
    paths, so the battery guards the poles instead of leaving them to a hand-checked prose
    table. Why they exist: D1.1c silently flipped the dogfood's style Microservices ->
    CleanArchitecture and deleted its ROUTES block + 4 cross-service edges, and it survived
    FOUR checkpoints of green gates — because no pole was in the cohort, a 42/42 eval said
    nothing about them. On a machine without those repos (CI, a clean clone) the harness
    SKIPs the rows; Step 3 now PRINTS every skip, because a silent skip is how a gate lies
    about what it covered. Poles pin SEMANTICS (style, signals, rendered sections), never
    node/edge counts — a count row on a live repo flakes, and a flaky row gets muted.
#>
param(
    [switch]$SkipEval,
    [ValidateSet('full', 'engine', 'app')]
    [string]$Scope = 'full',
    [switch]$SerialEval,
    # The MCP QA drive (step 2b) targets a machine-local dogfood repo (eval/mcp-qa/run.js) that
    # can't exist on a hosted runner — CI (.github/workflows/eval.yml) passes this. Local runs don't.
    [switch]$SkipMcpQa
)

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

# Step 0: Clear orphaned build-locking processes (Tapestry T0.1b).
# The wrap-up audit lost four builds to leaked DevContext.Server processes holding bin/. Clearing
# them here makes the gate trustworthy from cold, replacing the manual pre-session kill ritual.
Write-Step "Step 0: Clear orphaned build-locking processes"
$orphansKilled = 0
Get-Process DevContext.Server, testhost -ErrorAction SilentlyContinue | ForEach-Object {
    try { $_.Kill(); $script:orphansKilled++ } catch {}
}
# Windows PowerShell 5.1's Get-Process has no CommandLine, so a dotnet host running our server dll
# is found via CIM. The regex matches the server dll but not DevContext.Server.Tests.dll.
Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -match 'DevContext\.Server\.dll' } |
    ForEach-Object { try { Stop-Process -Id $_.ProcessId -Force -ErrorAction Stop; $script:orphansKilled++ } catch {} }
Write-Pass "Cleared $orphansKilled orphaned process(es)"

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

# Step 2: Fast unit tests (exclude Eval, CliSmoke, and McpQa).
# McpQa is a 3-minute external `node` MCP drive against the dogfood repo (its own Category, meant to
# be independently selectable). Run inside the parallel unit suite it loses a shared-state race and
# fails intermittently, yet passes reliably on its own — so it runs serially as Step 2b below. This
# keeps the fast suite fast and deterministic (Tapestry T0.1: gates trusted cold).
if ($Scope -eq 'app') {
    Write-Step "Steps 2-4b: engine suites - SKIPPED (-Scope app)"
    Write-Host "  fast tests / MCP QA / eval / CLI matrix skipped; the full battery must still run at the boundary" -ForegroundColor Yellow
}
if ($Scope -ne 'app') {
Write-Step "Step 2: Fast unit tests"
$filterArg = "Category!=Eval&Category!=CliSmoke&Category!=McpQa"
$testResult = dotnet test $sln --filter $filterArg --no-build 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host $testResult
    Write-Fail "Fast tests failed" -Step 2
    Write-Host ""
    Write-Host "GATE: FAIL (step 2 - fast tests)" -ForegroundColor Red
    exit 2
}
Write-Pass "Fast tests passed"

# Step 2b: MCP QA gate (serial — see note above).
Write-Step "Step 2b: MCP QA gate (serial)"
if ($SkipMcpQa) {
    Write-Host "  SKIP  -SkipMcpQa (dogfood repo is machine-local; not a pass)" -ForegroundColor Yellow
} else {
    $mcpResult = dotnet test $sln --filter "Category=McpQa" --no-build 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host $mcpResult
        Write-Fail "MCP QA gate failed" -Step 2
        Write-Host ""
        Write-Host "GATE: FAIL (step 2b - MCP QA)" -ForegroundColor Red
        exit 2
    }
    Write-Pass "MCP QA gate passed"
}

# Step 3: Eval tests.
# T7.0 engine stamp: a content hash over everything that can change an eval verdict. If it
# matches the stamp written by the last GREEN eval run, that verdict transfers and the step
# is skipped — an app-only battery stays a citable full gate at ~zero eval cost.
function Get-EngineStamp {
    $stampPaths = @('src\DevContext.Core', 'src\DevContext.Cli', 'tests\DevContext.Core.Tests',
                    'eval\expectations', 'eval\fixtures', 'tests\fixtures')
    $exts = @('.cs', '.csproj', '.json', '.razor', '.props', '.proto', '.slnx', '.cshtml')
    $sb = New-Object System.Text.StringBuilder
    foreach ($p in $stampPaths) {
        $dir = Join-Path $repoRoot $p
        if (-not (Test-Path $dir)) { continue }
        Get-ChildItem $dir -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $exts -contains $_.Extension } | Sort-Object FullName | ForEach-Object {
                [void]$sb.AppendLine($_.FullName.Substring($repoRoot.Length) + ':' + (Get-FileHash $_.FullName -Algorithm SHA256).Hash)
            }
    }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($sb.ToString())
    $stream = New-Object System.IO.MemoryStream(,$bytes)
    (Get-FileHash -InputStream $stream -Algorithm SHA256).Hash
}
$stampFile = Join-Path $repoRoot "eval\.eval-stamp.json"

if ($SkipEval) {
    Write-Step "Step 3: Eval expectation tests - SKIPPED (-SkipEval; mid-stage fast run)"
    Write-Host "  the full battery must still run at the stage/push/merge boundary" -ForegroundColor Yellow
} else {
    Write-Step "Step 3: Eval expectation tests"
    $engineStamp = Get-EngineStamp
    $lastGreen = $null
    if (Test-Path $stampFile) {
        try { $lastGreen = Get-Content $stampFile -Raw | ConvertFrom-Json } catch { $lastGreen = $null }
    }
    if ($lastGreen -and $lastGreen.stamp -eq $engineStamp) {
        Write-Pass "Eval cached - engine inputs unchanged since last green ($($lastGreen.date)); verdict transfers"
    } else {
        # T7.0: the expectation theory runs its repos split across two test hosts (repos are
        # independent in-process analyzes; names that are substrings of other names share a
        # bucket so no repo ever runs in both). Other Category=Eval classes ride bucket A.
        $evalExit = 0
        $evalResult = @()
        if ($SerialEval) {
            $evalResult = dotnet test $sln --filter "Category=Eval" --no-build 2>&1
            $evalExit = $LASTEXITCODE
            Write-Host $evalResult
        } else {
            $names = Get-ChildItem (Join-Path $repoRoot 'eval\expectations\*.json') -ErrorAction SilentlyContinue |
                Where-Object { $_.BaseName -notmatch '-output$' } | ForEach-Object { $_.BaseName } | Sort-Object
            $bucketA = New-Object System.Collections.Generic.List[string]
            $bucketB = New-Object System.Collections.Generic.List[string]
            $i = 0
            foreach ($n in $names) {
                $sub = $null
                foreach ($seen in ($bucketA + $bucketB)) {
                    if ($n.Contains($seen) -or $seen.Contains($n)) { $sub = $seen; break }
                }
                if ($sub -and $bucketA.Contains($sub)) { $bucketA.Add($n) }
                elseif ($sub) { $bucketB.Add($n) }
                elseif ($i++ % 2 -eq 0) { $bucketA.Add($n) }
                else { $bucketB.Add($n) }
            }
            if ($bucketB.Count -eq 0) {
                # Degenerate split (tiny expectation set) — fall back to one host.
                $evalResult = dotnet test $sln --filter "Category=Eval" --no-build 2>&1
                $evalExit = $LASTEXITCODE
                Write-Host $evalResult
            } else {
            $filterA = "Category=Eval&(" + ((($bucketA | ForEach-Object { "DisplayName~$_" }) + 'FullyQualifiedName!~EvalExpectationTests') -join '|') + ")"
            $filterB = "Category=Eval&(" + (($bucketB | ForEach-Object { "DisplayName~$_" }) -join '|') + ")"
            Write-Host "  split: A=$($bucketA.Count)+other eval classes, B=$($bucketB.Count) repos, two test hosts" -ForegroundColor Gray
            $logA = Join-Path $env:TEMP 'gates-eval-A.log'; $logB = Join-Path $env:TEMP 'gates-eval-B.log'
            $errA = Join-Path $env:TEMP 'gates-eval-A.err'; $errB = Join-Path $env:TEMP 'gates-eval-B.err'
            $procA = Start-Process dotnet -ArgumentList @('test', $sln, '--filter', $filterA, '--no-build', '--results-directory', (Join-Path $env:TEMP 'gates-eval-A')) -NoNewWindow -PassThru -RedirectStandardOutput $logA -RedirectStandardError $errA
            $procB = Start-Process dotnet -ArgumentList @('test', $sln, '--filter', $filterB, '--no-build', '--results-directory', (Join-Path $env:TEMP 'gates-eval-B')) -NoNewWindow -PassThru -RedirectStandardOutput $logB -RedirectStandardError $errB
            # PS 5.1: cache .Handle NOW or .ExitCode reads $null after exit (and $null -ne 0
            # is $true — a green run would be scored as a failure; bitten 2026-07-16).
            $null = $procA.Handle; $null = $procB.Handle
            $procA.WaitForExit(); $procB.WaitForExit()
            $evalResult = @(Get-Content $logA -ErrorAction SilentlyContinue) + @(Get-Content $logB -ErrorAction SilentlyContinue)
            Write-Host ($evalResult -join "`n")
            Write-Host "  host exit codes: A=$($procA.ExitCode) B=$($procB.ExitCode)" -ForegroundColor Gray
            if ($null -eq $procA.ExitCode -or $procA.ExitCode -ne 0) { $evalExit = 1 }
            elseif ($null -eq $procB.ExitCode -or $procB.ExitCode -ne 0) { $evalExit = 1 }
            }
        }

        $aspirationalLines = $evalResult | Select-String "ASPIRATIONAL-FAIL" | ForEach-Object { $_.Line }
        if ($aspirationalLines) {
            Write-Host ""
            Write-Host "Aspirational failures (non-blocking):" -ForegroundColor Yellow
            foreach ($line in $aspirationalLines) {
                Write-Host "  $line" -ForegroundColor Yellow
            }
        }

        # D1.2-fix2: a SKIPPED repo is a hole in the verdict, so say so out loud. The poles
        # (dogfood, shamshir) are machine-local by nature — on CI they skip, and a gate that
        # printed nothing would let "42/42 green" imply pole coverage it never had.
        $skipLines = $evalResult | Select-String "^\s*SKIP " | ForEach-Object { $_.Line.Trim() }
        if ($skipLines) {
            Write-Host ""
            Write-Host "Repos SKIPPED - not covered by this verdict:" -ForegroundColor Yellow
            foreach ($line in $skipLines) {
                Write-Host "  $line" -ForegroundColor Yellow
            }
        }

        if ($evalExit -ne 0) {
            Write-Fail "Eval tests failed" -Step 3
            Write-Host ""
            Write-Host "GATE: FAIL (step 3 - eval)" -ForegroundColor Red
            exit 3
        }
        @{ stamp = $engineStamp; date = (Get-Date -Format 'yyyy-MM-dd HH:mm') } | ConvertTo-Json |
            Out-File $stampFile -Encoding utf8
        Write-Pass "Eval tests passed (stamp written)"
    }
}

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

# Step 4b: CLI query ops (Tapestry T3.7).
# entrypoints/stats/trace now run against the snapshot's GraphQuery (one JSON shape shared with MCP),
# not the overview render. Assert each op returns real graph JSON on the fixture.
Write-Step "Step 4b: CLI query ops (T3.7)"
$queryFailed = 0

$epJson = & dotnet run --no-build --project $cliProject -- query entrypoints --path $testDir --format json 2>&1 | Out-String
try { $ep = $epJson | ConvertFrom-Json } catch { $ep = $null }
if ($ep -and $ep.count -gt 0 -and $ep.byKind -and ($ep.byKind.PSObject.Properties.Count -gt 0)) {
    Write-Host "    entrypoints: $($ep.count) entries across $($ep.byKind.PSObject.Properties.Count) kind(s)" -ForegroundColor Green
} else {
    Write-Host "    entrypoints: expected >0 entries with per-kind counts" -ForegroundColor Red; $queryFailed++
}

$stJson = & dotnet run --no-build --project $cliProject -- query stats --path $testDir --format json 2>&1 | Out-String
try { $st = $stJson | ConvertFrom-Json } catch { $st = $null }
if ($st -and $st.nodeCount -gt 0 -and $st.entriesByKind) {
    Write-Host "    stats: $($st.nodeCount) nodes, $($st.entryCount) entries, per-kind counts present" -ForegroundColor Green
} else {
    Write-Host "    stats: expected node counts + entriesByKind" -ForegroundColor Red; $queryFailed++
}

# trace must honor --focus (the render fallback ignored it): no focus => exit 1 guard; real focus => found.
& dotnet run --no-build --project $cliProject -- query trace --path $testDir 2>&1 | Out-Null
if ($LASTEXITCODE -eq 1) {
    Write-Host "    trace (no focus): required-focus guard fired (exit 1)" -ForegroundColor Green
} else {
    Write-Host "    trace (no focus): expected exit 1, got $LASTEXITCODE" -ForegroundColor Red; $queryFailed++
}
$focus = $null
if ($ep -and $ep.entries -and $ep.entries.Count -gt 0) { $focus = $ep.entries[0].title }
if ($focus) {
    $trJson = & dotnet run --no-build --project $cliProject -- query trace --path $testDir --focus $focus --format json 2>&1 | Out-String
    try { $tr = $trJson | ConvertFrom-Json } catch { $tr = $null }
    if ($tr -and $tr.found -eq $true -and $tr.root) {
        Write-Host "    trace('$focus'): found, root=$($tr.root.title)" -ForegroundColor Green
    } else {
        Write-Host "    trace('$focus'): expected found=true with a root (focus not honored)" -ForegroundColor Red; $queryFailed++
    }
}

if ($queryFailed -gt 0) {
    Write-Fail "$queryFailed CLI query op(s) failed" -Step 4
    Write-Host ""
    Write-Host "GATE: FAIL (step 4b - CLI query ops)" -ForegroundColor Red
    exit 4
}
Write-Pass "CLI query ops: entrypoints/stats/trace return graph JSON"
} # end engine suites ($Scope -ne 'app')

# Step 5: App check (T7.0) — pnpm check (lint + vitest + build) folds the desktop app into
# the battery; before this the full gate never covered the app at all.
if ($Scope -eq 'engine') {
    Write-Step "Step 5: App check - SKIPPED (-Scope engine)"
} else {
    Write-Step "Step 5: App check (pnpm check)"
    Push-Location (Join-Path $repoRoot "src\DevContext.App")
    try {
        # PS 5.1: native stderr under 2>&1 + EAP=Stop can throw mid-stream — relax around the call.
        $oldEap = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
        $appResult = & pnpm check 2>&1
        $appExit = $LASTEXITCODE
        $ErrorActionPreference = $oldEap
    } finally { Pop-Location }
    if ($appExit -ne 0) {
        Write-Host ($appResult | Select-Object -Last 40)
        Write-Fail "pnpm check failed" -Step 5
        Write-Host ""
        Write-Host "GATE: FAIL (step 5 - app check)" -ForegroundColor Red
        exit 5
    }
    Write-Pass "pnpm check passed"
}

# Final — non-full runs label themselves so they can never be cited as the boundary gate.
Write-Host ""
if ($Scope -eq 'app') {
    Write-Host "GATE: PASS (APP scope - engine suites skipped; not a merge gate)" -ForegroundColor Yellow
} elseif ($Scope -eq 'engine') {
    Write-Host "GATE: PASS (ENGINE scope - app check skipped; not a merge gate)" -ForegroundColor Yellow
} elseif ($SkipEval) {
    Write-Host "GATE: PASS (FAST - eval skipped; not a merge gate)" -ForegroundColor Yellow
} else {
    Write-Host "GATE: PASS" -ForegroundColor Green
}
exit 0
