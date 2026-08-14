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

    STEP ORDER IS FAIL-FAST, NOT NUMERICAL (owner gate chore, 2026-08-13). The battery runs
    0, 1, 1a, 5, 2, 2b, 4, 4b, 3 — cheapest and most-likely-to-break first, the ~11-minute
    eval suite last. The numbers are STABLE NAMES (a step keeps its number and its exit code
    forever, so "GATE: FAIL (step 4)" means the same thing across the program's history); the
    ORDER is a scheduling decision. Before this, an app-only typo waited out the whole engine
    cohort to be told, and a red eval hid whatever else was red behind it.

    An ABORT is not a step verdict: any terminating error unwinds through the trap at the top
    and exits 9 with the error named, so a battery that DIED is never read as a suite that
    FAILED. Steps use 1-5; 9 is reserved for "did not finish".

    Step 3 is ENGINE-STAMP CACHED (T7.0): before running, a SHA256 over engine sources,
    Core tests, expectations, and fixtures is compared to eval/.eval-stamp.json (written on
    the last green eval). Identical stamp = the previous verdict transfers - the step is
    skipped and labeled "eval cached". App-only batteries pay ~0 for the eval suite without
    losing full-gate citability. Delete eval/.eval-stamp.json to force a re-run.

    Step 3 runs the expectation theory SPLIT ACROSS TWO test hosts by default (~halves its
    wall time; repos are independent). -SerialEval restores the single-process form.

    -EvalTier (Prism D2.0, 2026-07-17 - owner pipeline-speed directive): 'quick' excludes the
    five HEAVY repos (bitwarden-server, screentogif, wolverine, newtonsoft-json,
    stackexchange-redis) from Step 3 so a mid-delivery checkpoint sweep lands in <5 min split.
    BOTH poles (dogfood-microservices, shamshir-pole) always ride. A quick-tier green NEVER
    writes the eval stamp (a partial verdict must not transfer) and the final line self-labels
    "not a merge gate" - delivery-close and boundary sweeps stay full-cohort (the default).

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
    # D2.0: 'quick' = exclude the heavy repos from Step 3 (mid-delivery cadence; not a merge gate).
    [ValidateSet('quick', 'full')]
    [string]$EvalTier = 'full',
    [switch]$SerialEval,
    # The MCP QA drive (step 2b) targets a machine-local dogfood repo (eval/mcp-qa/run.js) that
    # can't exist on a hosted runner — CI (.github/workflows/eval.yml) passes this. Local runs don't.
    [switch]$SkipMcpQa
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

$exitCode = 0

# ABORT = FAIL, WITH A NAME (owner gate chore, ported from the engine branch). Under
# $ErrorActionPreference = 'Stop' any terminating error outside Invoke-NativeCapture — a missing
# tool, a path that vanished, a typo in a step — unwinds the script silently: PowerShell prints the
# record to stderr and exits, so the battery ends with no "GATE:" line at all. Conductor then reads
# a truncated log and has to GUESS whether the battery failed or the host died.
#
# Exit 9 is reserved for exactly that: the battery ABORTED rather than any step failing. It is a
# distinct code from every step's (1,2,3,4,5), so it can never be mistaken for a suite verdict, and
# the error and the step banner it died under are printed rather than swallowed.
trap {
    Write-Host ""
    Write-Host "  ABORT  $($_.Exception.Message)" -ForegroundColor Red
    if ($_.InvocationInfo) { Write-Host "         at $($_.InvocationInfo.ScriptName):$($_.InvocationInfo.ScriptLineNumber)" -ForegroundColor Red }
    Write-Host ""
    Write-Host "GATE: ABORTED (exit 9 - the battery did not finish; this is NOT a step verdict)" -ForegroundColor Red
    exit 9
}

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

# EVERY native command whose output this script captures runs through here. Windows PowerShell 5.1
# wraps each stderr line a native tool writes into a NativeCommandError ErrorRecord, and under
# $ErrorActionPreference = 'Stop' that record is TERMINATING: `$r = dotnet test ... 2>&1` aborts the
# whole script the instant the tool writes one line to stderr - BEFORE the $LASTEXITCODE check that
# decides pass or fail, and before the Write-Host that prints why.
#
# Measured 2026-07-29 (G5, evidence in eval-results/2026-07-29/G5/): this killed the battery twice
# with exit 1 and an empty diagnostic - run.db gates rows 3 (s2) and 39 (s16), both truncated at the
# "Step 2: Fast unit tests" banner - while the suite underneath was GREEN. The reverse is worse: on a
# genuinely failing suite the same abort fires before `Write-Host $testResult`, so the failing test
# names are discarded too. Step 5 already carried this workaround inline for `pnpm check`; the other
# eleven captures did not.
#
# This STRENGTHENS the gate rather than relaxing it: the relaxation is scoped to the native call, it
# restores the previous preference in a finally, and its whole effect is that the $LASTEXITCODE check
# now actually runs. A non-zero exit still fails the step, and now keeps its output.
function Invoke-NativeCapture {
    param([Parameter(Mandatory)][scriptblock]$Command)
    $oldEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { & $Command 2>&1 } finally { $ErrorActionPreference = $oldEap }
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
$buildResult = Invoke-NativeCapture { dotnet build $sln }
if ($LASTEXITCODE -ne 0) {
    Write-Host $buildResult
    Write-Fail "Build failed" -Step 1
    Write-Host ""
    Write-Host "GATE: FAIL (step 1 - build)" -ForegroundColor Red
    exit 1
}
Write-Pass "Build succeeded"

# Step 1a: contract sweep (S9). Cheap (no build, ~2s), and it guards a defect this program hit
# FOUR times before it was a check: a field the engine computes, the wire carries, and no client
# reads. Runs in EVERY scope, before the slow steps: it is not an engine suite, and an app-only
# change is one of the ways a field loses its last reader.
Write-Step "Step 1a: Contract sweep (dead proto fields)"
$sweepResult = Invoke-NativeCapture { & powershell -NoProfile -File (Join-Path $PSScriptRoot "contract-sweep.ps1") }
if ($LASTEXITCODE -ne 0) {
    Write-Host $sweepResult
    Write-Fail "Contract sweep found a field no client reads" -Step 1
    Write-Host ""
    Write-Host "GATE: FAIL (step 1a - contract sweep)" -ForegroundColor Red
    exit 2
}
Write-Pass "Contract sweep clean (every response field read or allow-listed with a reason)"

# Step 5: App check (T7.0) — pnpm check (lint + vitest + build) folds the desktop app into
# the battery; before this the full gate never covered the app at all.
if ($Scope -eq 'engine') {
    Write-Step "Step 5: App check - SKIPPED (-Scope engine)"
} else {
    Write-Step "Step 5: App check (pnpm check)"
    # PREFLIGHT (owner gate chore). A worktree that has never had `pnpm install` run in it fails
    # `pnpm check` with a module-resolution error that reads like a broken app, not a missing
    # install — and every fresh worktree this program creates starts that way. Say which it is
    # BEFORE spending the run, and name the one command that fixes it.
    $appDir = Join-Path $repoRoot "src\DevContext.App"
    if (-not (Test-Path (Join-Path $appDir "node_modules"))) {
        Write-Fail "node_modules is missing in $appDir - run 'pnpm install' there first" -Step 5
        Write-Host ""
        Write-Host "GATE: FAIL (step 5 - app dependencies not installed; this is a SETUP failure, not an app failure)" -ForegroundColor Red
        exit 5
    }
    Push-Location $appDir
    try {
        # This step's inline workaround is where the hazard was first found; it is now the shared
        # Invoke-NativeCapture every step uses (see its comment for what it costs to leave one out).
        $appResult = Invoke-NativeCapture { & pnpm check }
        $appExit = $LASTEXITCODE
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
$testResult = Invoke-NativeCapture { dotnet test $sln --filter $filterArg --no-build }
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
    $mcpResult = Invoke-NativeCapture { dotnet test $sln --filter "Category=McpQa" --no-build }
    if ($LASTEXITCODE -ne 0) {
        Write-Host $mcpResult
        Write-Fail "MCP QA gate failed" -Step 2
        Write-Host ""
        Write-Host "GATE: FAIL (step 2b - MCP QA)" -ForegroundColor Red
        exit 2
    }
    Write-Pass "MCP QA gate passed"
}

# Step 2c: Wire-truth gate (pre-release T1.4) — what an AGENT actually receives.
#
# Every other step in this battery reads the engine from the inside: C# tests, expectation JSON,
# the CLI. None of them speaks MCP, and that hole is exactly where bug #5 lived for a year — the
# C# source carried 26 XML doc summaries while `tools/list` on the wire carried 22 empty strings,
# through every green gate. So these probes drive a REAL MCP handshake over stdio against the
# binary step 1 just built and judge the REPLY, never the source.
#
#   wire-truth.js    (~15s, no analyze) every tool AND parameter carries a description; the menu is
#                    the curated one; an unknown name gets the did-you-mean envelope naming the
#                    unlisted specialists; all five enum dials reject an out-of-range value while a
#                    valid one still reaches the real code path.
#   partial-truth.js (~2-4min, analyzes TodoApi) the confident-partial-truth family: every
#                    entrypoints title round-trips through get_context AND trace; trace(nodeId) is
#                    never weaker than trace(bare name) and never renders a phantom; a found:true
#                    trace with 0 steps says why; an elided pack names the elision and the
#                    budgetTokens lever (and the probe proves it actually found an elided pack, so
#                    a green here can't be vacuous).
#   deep-link-truth.js (~1-2min, analyzes TodoApi) N4.3: the desktop's MCP feed rows navigate off
#                    two ToolCallEvent fields, so it subscribes to ObserveToolCalls over gRPC-web
#                    and reads back what a real trace/get_context call streamed — the RPC name the
#                    page routes on, the subject it opens, the repo it belongs to. Also the one
#                    live drive of ListMcpTools, which serves that page's catalog.
#
# Both print PASS/FAIL per bar and exit non-zero on any FAIL. Proven RED first, on a real pre-fix
# binary built from 5853ac0 (the commit before the T1 fixes): eval-results/2026-08-13/t1-wire-truth-gate/.
Write-Step "Step 2c: Wire-truth gate (real MCP handshake)"
$node = Get-Command node -ErrorAction SilentlyContinue
$mcpExe = Join-Path $repoRoot "src\DevContext.Mcp\bin\Debug\net10.0\devcontext-mcp.exe"
# TEMP, not eval-results\: these probes dump ~800KB of raw wire transcript per run, and the battery
# runs after every session. The path is printed on failure, which is when anyone wants it.
$wireOut = Join-Path $env:TEMP "devcontext-gate-wire-truth"
if (-not $node) {
    Write-Fail "node not on PATH - the wire-truth probes cannot run (this step is not optional)" -Step 2
    Write-Host ""
    Write-Host "GATE: FAIL (step 2c - node missing)" -ForegroundColor Red
    exit 2
}
if (-not (Test-Path $mcpExe)) {
    Write-Fail "devcontext-mcp.exe not built at $mcpExe" -Step 2
    Write-Host ""
    Write-Host "GATE: FAIL (step 2c - MCP exe missing)" -ForegroundColor Red
    exit 2
}
$wireResult = Invoke-NativeCapture { & node (Join-Path $repoRoot "eval\mcp-qa\wire-truth.js") $wireOut }
$wireExit = $LASTEXITCODE
Write-Host ($wireResult -join "`n")
if ($wireExit -ne 0) {
    Write-Fail "wire-truth probe failed (exit $wireExit) - see $wireOut" -Step 2
    Write-Host ""
    Write-Host "GATE: FAIL (step 2c - wire truth)" -ForegroundColor Red
    exit 2
}
Write-Pass "wire truth: every tool + parameter described, menu curated, enum dials reject"

# partial-truth analyzes a repo, so it needs the eval-repos submodule. Absent = a HOLE in this
# verdict, printed the way step 3 prints its skipped repos - never a silent pass.
# The marker is a REAL file inside the pin (TodoApp.sln), not the directory: an uninitialised
# submodule leaves the directory there and empty, so Test-Path on the dir alone would report
# covered and skip forever. Measured 2026-08-13 - the first draft of this check tested for a
# TodoApi.csproj that does not exist in the pin, and would have skipped on every machine.
$ptRepo = Join-Path $repoRoot "eval-repos\TodoApi"
if (-not (Test-Path (Join-Path $ptRepo "TodoApp.sln"))) {
    Write-Host "  SKIP  partial-truth: eval-repos\TodoApi absent (git submodule update --init)." -ForegroundColor Yellow
    Write-Host "        NOT COVERED by this verdict: entry round-trip, trace-by-nodeId, elision honesty," -ForegroundColor Yellow
    Write-Host "        and the MCP feed's deep links (deep-link-truth analyzes the same repo)." -ForegroundColor Yellow
} else {
    $ptResult = Invoke-NativeCapture { & node (Join-Path $repoRoot "eval\mcp-qa\partial-truth.js") $wireOut $ptRepo }
    $ptExit = $LASTEXITCODE
    Write-Host (($ptResult | Select-String "PASS|FAIL|GREEN|RED|handle:") -join "`n")
    if ($ptExit -ne 0) {
        Write-Host ($ptResult -join "`n")
        Write-Fail "partial-truth probe failed (exit $ptExit) - see $wireOut" -Step 2
        Write-Host ""
        Write-Host "GATE: FAIL (step 2c - partial truth)" -ForegroundColor Red
        exit 2
    }
    Write-Pass "partial truth: entry names round-trip, nodeId traces, every elision named"

    # N4.3 — the MCP page's feed rows are navigable, and both halves of that decision are made
    # from ToolCallEvent: `tool` (the gRPC method the server recorded) and `primary_arg` (the
    # subject, stamped by the sidecar). If either stops arriving, or the RPC is spelled
    # differently than mcp-page.ts routes on, the row silently loses its link - nothing errors and
    # nothing logs. So it is measured on the real wire, through the same gRPC-web transport the
    # desktop uses. Shares the TodoApi guard above: same repo, same submodule.
    $dlResult = Invoke-NativeCapture { & node (Join-Path $repoRoot "eval\mcp-qa\deep-link-truth.js") (Join-Path $wireOut "deep-link") $ptRepo }
    $dlExit = $LASTEXITCODE
    Write-Host (($dlResult | Select-String "PASS|FAIL|focus:|handle:") -join "`n")
    if ($dlExit -ne 0) {
        Write-Host ($dlResult -join "`n")
        Write-Fail "deep-link-truth probe failed (exit $dlExit) - see $wireOut\deep-link" -Step 2
        Write-Host ""
        Write-Host "GATE: FAIL (step 2c - deep-link truth)" -ForegroundColor Red
        exit 2
    }
    Write-Pass "deep-link truth: feed rows carry the RPC and the subject the desktop navigates on"
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
    $cliOutput = Invoke-NativeCapture { & dotnet run --no-build --project $cliProject -- $entry.Args }
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

$epJson = Invoke-NativeCapture { & dotnet run --no-build --project $cliProject -- query entrypoints --path $testDir --format json } | Out-String
try { $ep = $epJson | ConvertFrom-Json } catch { $ep = $null }
if ($ep -and $ep.count -gt 0 -and $ep.byKind -and ($ep.byKind.PSObject.Properties.Count -gt 0)) {
    Write-Host "    entrypoints: $($ep.count) entries across $($ep.byKind.PSObject.Properties.Count) kind(s)" -ForegroundColor Green
} else {
    Write-Host "    entrypoints: expected >0 entries with per-kind counts" -ForegroundColor Red; $queryFailed++
}

$stJson = Invoke-NativeCapture { & dotnet run --no-build --project $cliProject -- query stats --path $testDir --format json } | Out-String
try { $st = $stJson | ConvertFrom-Json } catch { $st = $null }
# K2 (Prism D3.3): stats must carry the analyze-time stage timeline (fresh fixture => never empty).
if ($st -and $st.nodeCount -gt 0 -and $st.entriesByKind -and $st.stages.Count -gt 0) {
    Write-Host "    stats: $($st.nodeCount) nodes, $($st.entryCount) entries, $($st.stages.Count) waterfall stages" -ForegroundColor Green
} else {
    Write-Host "    stats: expected node counts + entriesByKind + non-empty stages timeline" -ForegroundColor Red; $queryFailed++
}

# trace must honor --focus (the render fallback ignored it): no focus => exit 1 guard; real focus => found.
Invoke-NativeCapture { & dotnet run --no-build --project $cliProject -- query trace --path $testDir } | Out-Null
if ($LASTEXITCODE -eq 1) {
    Write-Host "    trace (no focus): required-focus guard fired (exit 1)" -ForegroundColor Green
} else {
    Write-Host "    trace (no focus): expected exit 1, got $LASTEXITCODE" -ForegroundColor Red; $queryFailed++
}
$focus = $null
if ($ep -and $ep.entries -and $ep.entries.Count -gt 0) { $focus = $ep.entries[0].title }
if ($focus) {
    $trJson = Invoke-NativeCapture { & dotnet run --no-build --project $cliProject -- query trace --path $testDir --focus $focus --format json } | Out-String
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
# Step 3: Eval tests.
# T7.0 engine stamp: a content hash over everything that can change an eval verdict. If it
# matches the stamp written by the last GREEN eval run, that verdict transfers and the step
# is skipped — an app-only battery stays a citable full gate at ~zero eval cost.
#
# Hashing goes through the .NET type, NOT Get-FileHash (2026-08-13, N0 battery RED). In Windows
# PowerShell 5.1 Get-FileHash is a FUNCTION exported by the Microsoft.PowerShell.Utility MODULE,
# resolved by PSModulePath autoloading — not a built-in cmdlet. Launched from a pwsh-7 parent (how
# Conductor and any PS7 shell run this script) the 5.1 child inherits a PSModulePath with PowerShell
# 7's module dirs PREPENDED, autoloads PS7's Utility module, and Get-FileHash resolves to nothing:
# CommandNotFound, and with ErrorActionPreference=Stop the whole battery dies one line into Step 3.
# It looked like a mystery red because -Scope app skips Step 3, so only the full form ever reached
# here. SHA256 over the same bytes, same uppercase-hex format — the measurement is unchanged.
function Get-Sha256Hex([byte[]]$bytes) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('X2') }) -join '' }
    finally { $sha.Dispose() }
}
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
                $fileHash = Get-Sha256Hex ([System.IO.File]::ReadAllBytes($_.FullName))
                [void]$sb.AppendLine($_.FullName.Substring($repoRoot.Length) + ':' + $fileHash)
            }
    }
    Get-Sha256Hex ([System.Text.Encoding]::UTF8.GetBytes($sb.ToString()))
}
$stampFile = Join-Path $repoRoot "eval\.eval-stamp.json"

# D2.0: the QUICK tier's exclusion list. These five dominate the ~16-min serial cohort wall
# (bitwarden alone ~4-5 min); excluding them targets <5 min split for mid-delivery sweeps.
# Exact expectation basenames - poles are NOT here on purpose (they always ride).
$HeavyRepos = @('bitwarden-server', 'screentogif', 'wolverine', 'newtonsoft-json', 'stackexchange-redis')

if ($SkipEval) {
    Write-Step "Step 3: Eval expectation tests - SKIPPED (-SkipEval; mid-stage fast run)"
    Write-Host "  the full battery must still run at the stage/push/merge boundary" -ForegroundColor Yellow
} else {
    Write-Step "Step 3: Eval expectation tests"
    if ($EvalTier -eq 'quick') {
        # No silent caps: a tiered verdict says out loud what it did not cover.
        Write-Host "  QUICK tier - $($HeavyRepos.Count) heavy repos EXCLUDED from this verdict: $($HeavyRepos -join ', ')" -ForegroundColor Yellow
    }
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
            $serialFilter = "Category=Eval"
            if ($EvalTier -eq 'quick') {
                $serialFilter += (($HeavyRepos | ForEach-Object { "&DisplayName!~$_" }) -join '')
            }
            $evalResult = Invoke-NativeCapture { dotnet test $sln --filter $serialFilter --no-build }
            $evalExit = $LASTEXITCODE
            Write-Host $evalResult
        } else {
            $names = Get-ChildItem (Join-Path $repoRoot 'eval\expectations\*.json') -ErrorAction SilentlyContinue |
                Where-Object { $_.BaseName -notmatch '-output$' } | ForEach-Object { $_.BaseName } | Sort-Object
            if ($EvalTier -eq 'quick') {
                $names = @($names | Where-Object { $HeavyRepos -notcontains $_ })
            }
            # D2.0: known-slow repos ride FIRST, heaviest first, so the round-robin below spreads
            # them across the hosts (LPT-style). Plain alphabetical alternation stacked
            # shamshir-pole + dntsite + dotnet-podcasts in one bucket - 8m28 vs 3m56 halves
            # (prism-d2/gates-d20-quick.txt). Names absent from the cohort (quick tier) drop out.
            $slowFirst = @('bitwarden-server', 'shamshir-pole', 'dntsite', 'screentogif', 'wolverine',
                           'newtonsoft-json', 'dotnet-podcasts', 'stackexchange-redis', 'eshop', 'dogfood-microservices')
            $names = @($slowFirst | Where-Object { $names -contains $_ }) + @($names | Where-Object { $slowFirst -notcontains $_ })
            $bucketA = New-Object System.Collections.Generic.List[string]
            $bucketB = New-Object System.Collections.Generic.List[string]
            # Start the alternation at B: host A additionally carries every non-expectation eval
            # class (filterA's FullyQualifiedName!~ arm), so the single heaviest repo goes opposite.
            $i = 1
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
                $evalResult = Invoke-NativeCapture { dotnet test $sln --filter "Category=Eval" --no-build }
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
        if ($EvalTier -eq 'quick') {
            # A quick-tier green is a partial verdict - it must not transfer to later full gates.
            Write-Pass "Eval tests passed (QUICK tier - stamp NOT written; heavy repos not covered)"
        } else {
            @{ stamp = $engineStamp; date = (Get-Date -Format 'yyyy-MM-dd HH:mm') } | ConvertTo-Json |
                Out-File $stampFile -Encoding utf8
            Write-Pass "Eval tests passed (stamp written)"
        }
    }
}

} # end engine suites ($Scope -ne 'app')

# Final — non-full runs label themselves so they can never be cited as the boundary gate.
Write-Host ""
if ($Scope -eq 'app') {
    Write-Host "GATE: PASS (APP scope - engine suites skipped; not a merge gate)" -ForegroundColor Yellow
} elseif ($Scope -eq 'engine') {
    Write-Host "GATE: PASS (ENGINE scope - app check skipped; not a merge gate)" -ForegroundColor Yellow
} elseif ($SkipEval) {
    Write-Host "GATE: PASS (FAST - eval skipped; not a merge gate)" -ForegroundColor Yellow
} elseif ($EvalTier -eq 'quick') {
    Write-Host "GATE: PASS (QUICK eval tier - heavy repos excluded; not a merge gate)" -ForegroundColor Yellow
} else {
    Write-Host "GATE: PASS" -ForegroundColor Green
}
exit 0
