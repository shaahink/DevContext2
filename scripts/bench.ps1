param(
    [string]$ReposFile = "eval-repos.json",
    [string]$ResultsDir = "eval-results",
    [string]$CliProject = "src\DevContext.Cli",
    [switch]$SkipClone,
    [switch]$DiffOnly,
    [switch]$Truth
)

$ErrorActionPreference = "Stop"
$rootDir = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path
$reposPath = Join-Path $rootDir $ReposFile
$resultsRoot = Join-Path $rootDir $ResultsDir
$cliPath = Join-Path $rootDir $CliProject

if (-not (Test-Path $reposPath)) {
    Write-Error "Repos file not found: $reposPath"
    exit 1
}

$config = Get-Content $reposPath -Raw | ConvertFrom-Json
$dateStamp = Get-Date -Format "yyyy-MM-dd"
$runDir = Join-Path $resultsRoot $dateStamp
New-Item -ItemType Directory -Path $runDir -Force | Out-Null

# Build CLI
Write-Host "Building DevContext CLI..." -ForegroundColor Yellow
$buildResult = & dotnet build "$cliPath" --nologo 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "CLI build failed"
    exit 1
}

$script:stubFailures = 0

if ($DiffOnly) {
    Write-Host "Diff-only mode -- skipping analyze runs." -ForegroundColor Yellow
}
else {
    $total = ($config.repos | Measure-Object).Count
    $index = 0
    foreach ($repo in $config.repos) {
        $index++
        $name = $repo.name
        Write-Host "`n===== [$index/$total] $name =====" -ForegroundColor Cyan

        # Resolve repo path: localPath > eval-repos/<name>
        $repoPath = if ($repo.localPath) {
            $path = $repo.localPath
            if (-not [System.IO.Path]::IsPathRooted($path)) {
                $path = Join-Path $rootDir $path
            }
            $path
        }
        else {
            $evalDir = Join-Path (Join-Path $rootDir "eval-repos") $name
            $dirEmpty = $false
            if (Test-Path $evalDir) {
                $hasContent = Get-ChildItem $evalDir -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
                if (-not $hasContent) {
                    Write-Host "  Empty clone detected - removing and re-cloning..." -ForegroundColor Yellow
                    Remove-Item $evalDir -Recurse -Force -ErrorAction SilentlyContinue
                    $dirEmpty = $true
                }
            }
            if ((-not (Test-Path $evalDir)) -or $dirEmpty) {
                Write-Host "  Cloning $($repo.url)..." -ForegroundColor Yellow
                New-Item -ItemType Directory -Path $evalDir -Force | Out-Null
                $isSha = $repo.ref -match '^[0-9a-fA-F]{40}$'
                $previousEAP = $ErrorActionPreference
                $ErrorActionPreference = "Continue"
                if ($isSha) {
                    git clone --no-checkout $repo.url $evalDir 2>&1 | Out-Null
                    if ($LASTEXITCODE -eq 0) {
                        Push-Location $evalDir
                        git checkout $repo.ref 2>&1 | Out-Null
                        Pop-Location
                    }
                }
                else {
                    $refArg = if ($repo.ref) { @("--branch", $repo.ref) } else { @() }
                    git clone --depth 1 $refArg $repo.url $evalDir 2>&1 | Out-Null
                }
                $ErrorActionPreference = $previousEAP
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "  ## Clone failed, skipping." -ForegroundColor Red
                    continue
                }
            }
            $evalDir
        }

        if (-not (Test-Path $repoPath)) {
            Write-Host "  ## Path not found: $repoPath, skipping." -ForegroundColor Red
            continue
        }

        # Run report
        $outFile = Join-Path $runDir "$name-report.md"
        Write-Host "  Running devcontext report $repoPath ..." -ForegroundColor Gray

        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $result = & dotnet run --project "$cliPath" --no-build -- report $repoPath --no-cache -o $outFile 2>&1
        $sw.Stop()

        if ($LASTEXITCODE -ne 0) {
            Write-Host "  Failed (exit $LASTEXITCODE)" -ForegroundColor Red
            $result | Out-File (Join-Path $runDir "$name-error.txt")
            $script:stubFailures++
            continue
        }

        # Content-assert: a stub report (no analysis data) is a hard failure
        $reportContent = Get-Content $outFile -Raw -ErrorAction SilentlyContinue
        $isStub = $reportContent -match '_No analysis data available\._'
        $hasStats = $reportContent -match '## Stats'
        $hasTopFlows = $reportContent -match '## Top Flows'

        if ($isStub -or -not $hasStats -or -not $hasTopFlows) {
            Write-Host "  ASSERTION FAILED: stub report or missing structural sections (Stats=$hasStats, TopFlows=$hasTopFlows, Stub=$isStub)" -ForegroundColor Red
            $script:stubFailures++
            continue
        }

        Write-Host "  Done in $([math]::Round($sw.Elapsed.TotalSeconds, 1))s -> $outFile (Stats ok, TopFlows ok)" -ForegroundColor Green
    }

    if ($script:stubFailures -gt 0) {
        Write-Error "$($script:stubFailures) report(s) failed content assertion - stub reports or missing structural sections"
        exit 1
    }

    Write-Host "`n===== Reports saved to $runDir =====" -ForegroundColor Cyan
}

# ---- Diff vs previous run ----
$runs = Get-ChildItem $resultsRoot -Directory | Sort-Object Name -Descending
if ($runs.Count -ge 2) {
    $currentRun = $runs[0].Name
    $previousRun = $runs[1].Name
    Write-Host "`n===== Diff: $previousRun -> $currentRun =====" -ForegroundColor Cyan

    $diffReport = @()
    foreach ($repo in $config.repos) {
        $name = $repo.name
        $curFile = Join-Path (Join-Path $resultsRoot $currentRun) "$name-report.md"
        $prevFile = Join-Path (Join-Path $resultsRoot $previousRun) "$name-report.md"

        if (-not (Test-Path $curFile)) { continue }

        if (-not (Test-Path $prevFile)) {
            $diffReport += "## NEW: $name"
            $diffReport += "> First run -- no previous report to diff against."
            $diffReport += ""
            continue
        }

        $cur = Get-Content $curFile -Raw
        $prev = Get-Content $prevFile -Raw

        # Strip non-deterministic sections for diff purposes
        $strip = {
            param($txt)
            # Strip wall-time sections (Run Report stages, extractor timing, analyzed-in stat)
            $txt = $txt -replace '\| Analyzed in \| [\d.]+s \|', '| Analyzed in | (stripped) |'
            $txt = $txt -replace '\| .+ \| \d+ms \|', '| (stage) | (stripped) |'
            $txt = $txt -replace '\| .+ \| \d+ms \| \d+ \| \d+ \|', '| (extractor) | (stripped) | (stripped) | (stripped) |'
            return $txt
        }

        $curStripped = & $strip $cur
        $prevStripped = & $strip $prev

        if ($curStripped -eq $prevStripped) {
            $diffReport += "## UNCHANGED: $name"
            $diffReport += "> No structural changes detected."
            $diffReport += ""
            continue
        }

        # Structural diff: count entry changes, insight changes
        $curEntries = ($cur -split "`n" | Select-String -Pattern '^\d+\. \*\*').Count
        $prevEntries = ($prev -split "`n" | Select-String -Pattern '^\d+\. \*\*').Count
        $curInsights = ($cur -split "`n" | Select-String -Pattern '^### \*\*').Count
        $prevInsights = ($prev -split "`n" | Select-String -Pattern '^### \*\*').Count
        $curNodes = if ($cur -match '\| Nodes \| (\d+) \|') { $Matches[1] } else { "?" }
        $prevNodes = if ($prev -match '\| Nodes \| (\d+) \|') { $Matches[1] } else { "?" }
        $curEdges = if ($cur -match '\| Edges \| (\d+) \|') { $Matches[1] } else { "?" }
        $prevEdges = if ($prev -match '\| Edges \| (\d+) \|') { $Matches[1] } else { "?" }

        $diffReport += "## CHANGED: $name"
        $diffReport += ""
        $diffReport += "| Metric | Before | After | Delta |"
        $diffReport += "|--------|--------|-------|-------|"
        $diffReport += "| Nodes | $prevNodes | $curNodes | $(if ($curNodes -ne $prevNodes) {'*'} else {'='}) |"
        $diffReport += "| Edges | $prevEdges | $curEdges | $(if ($curEdges -ne $prevEdges) {'*'} else {'='}) |"
        $diffReport += "| Top Flows | $prevEntries | $curEntries | $(if ($curEntries -ne $prevEntries) { "$($curEntries - $prevEntries)" } else {'='}) |"
        $diffReport += "| Insights | $prevInsights | $curInsights | $(if ($curInsights -ne $prevInsights) { "$($curInsights - $prevInsights)" } else {'='}) |"
        $diffReport += ""

        # Section-level diff
        $curSections = ($cur -split "`n" | Select-String -Pattern '^## ' | ForEach-Object { $_.Line.Trim() })
        $prevSections = ($prev -split "`n" | Select-String -Pattern '^## ' | ForEach-Object { $_.Line.Trim() })
        $added = $curSections | Where-Object { $_ -notin $prevSections }
        $removed = $prevSections | Where-Object { $_ -notin $curSections }

        if ($added.Count -gt 0) {
            $diffReport += "**Added sections:**"
            foreach ($s in $added) { $diffReport += "  + $s" }
            $diffReport += ""
        }
        if ($removed.Count -gt 0) {
            $diffReport += "**Removed sections:**"
            foreach ($s in $removed) { $diffReport += "  - $s" }
            $diffReport += ""
        }
    }

    if ($diffReport.Count -gt 0) {
        $diffFile = Join-Path $runDir "DIFF-vs-$previousRun.md"
        $diffReport -join "`n" | Out-File $diffFile -Encoding utf8
        Write-Host "Diff written to $diffFile" -ForegroundColor Green
    }
    else {
        Write-Host "(No diff report generated -- all identical.)" -ForegroundColor Gray
    }
}

# ---- Truth gate (L0+) ----
if ($Truth) {
    Write-Host "`n===== Running truth gate (dotnet test --filter Category=Truth) =====" -ForegroundColor Cyan

    $testProject = Join-Path $rootDir "tests\DevContext.Core.Tests\DevContext.Core.Tests.csproj"
    $truthLog = Join-Path $runDir "truth-gate.txt"

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $testOutput = & dotnet test $testProject --filter "Category=Truth" --no-build --verbosity normal 2>&1
    $testExit = $LASTEXITCODE
    $sw.Stop()

    # Save raw output
    $testOutput | Out-File $truthLog -Encoding utf8

    # Summarise
    $passed = ($testOutput | Select-String -Pattern 'Passed:' | ForEach-Object { $_.Line }).Trim()
    $failed = ($testOutput | Select-String -Pattern 'Failed:' | ForEach-Object { $_.Line }).Trim()
    $skipped = ($testOutput | Select-String -Pattern 'Skipped:' | ForEach-Object { $_.Line }).Trim()

    Write-Host "  $passed" -ForegroundColor Green
    if ($failed) { Write-Host "  $failed" -ForegroundColor Red }
    if ($skipped) { Write-Host "  $skipped (truth ratchets pending)" -ForegroundColor Yellow }

    Write-Host "  Truth gate $($sw.Elapsed.TotalSeconds.ToString('F1'))s -> $truthLog" -ForegroundColor $(if ($testExit -eq 0) { 'Green' } else { 'Red' })

    if ($testExit -ne 0) {
        Write-Error "Truth gate FAILED — one or more truth checks are red. Fix or record before proceeding."
        exit 1
    }

    Write-Host "`n===== Truth gate complete =====" -ForegroundColor Green
}

Write-Host "`n===== Bench complete =====" -ForegroundColor Cyan
