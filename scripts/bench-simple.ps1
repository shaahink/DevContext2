param([string]$ResultsDir = "eval-results")

$ErrorActionPreference = "Stop"
$rootDir = Get-Location
$reposPath = Join-Path $rootDir "eval-repos.json"
$resultsRoot = Join-Path $rootDir $ResultsDir
$cliPath = Join-Path $rootDir "src\DevContext.Cli"
$dateStamp = Get-Date -Format "yyyy-MM-dd"
$runDir = Join-Path $resultsRoot $dateStamp
New-Item -ItemType Directory -Path $runDir -Force | Out-Null

$config = Get-Content $reposPath -Raw | ConvertFrom-Json
$total = ($config.repos | Measure-Object).Count
$index = 0
$summary = @()

Write-Host "BENCH: $total repos -> $runDir" -ForegroundColor Yellow

foreach ($repo in $config.repos) {
    $index++
    $name = $repo.name
    Write-Host "[$index/$total] $name" -ForegroundColor Cyan

    $repoPath = if ($repo.localPath) {
        $path = $repo.localPath
        if (-not [System.IO.Path]::IsPathRooted($path)) { $path = Join-Path $rootDir $path }
        $path
    } else {
        Join-Path (Join-Path $rootDir "eval-repos") $name
    }

    if (-not (Test-Path $repoPath)) {
        Write-Host "  SKIP: path not found" -ForegroundColor Yellow
        $summary += "$name|SKIP|not found|0|0|0s"
        continue
    }

    $outFile = Join-Path $runDir "$name-report.md"
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $result = & dotnet run --project "$cliPath" --no-build -- report $repoPath --no-cache -o $outFile 2>&1
    $sw.Stop()
    $time = $sw.Elapsed.TotalSeconds

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FAIL: exit $LASTEXITCODE" -ForegroundColor Red
        $result | Out-File (Join-Path $runDir "$name-error.txt")
        $summary += "$name|FAIL|exit $LASTEXITCODE|0|0|$([math]::Round($time,1))s"
        continue
    }

    $reportContent = Get-Content $outFile -Raw -ErrorAction SilentlyContinue
    $isStub = $reportContent -match '_No analysis data available_'
    $hasStats = $reportContent -match '## Stats'

    if ($isStub -or -not $hasStats) {
        Write-Host "  STUB: no analysis data" -ForegroundColor Red
        $summary += "$name|STUB|empty|0|0|$([math]::Round($time,1))s"
        continue
    }

    $nodes = if ($reportContent -match '\| Nodes \| (\d+)') { $Matches[1] } else { "?" }
    $edges = if ($reportContent -match '\| Edges \| (\d+)') { $Matches[1] } else { "?" }
    $style = if ($reportContent -match 'Style: (\S+)') { $Matches[1] } else { "?" }

    Write-Host "  OK: $nodes nodes, $edges edges, style=$style, $([math]::Round($time,1))s" -ForegroundColor Green
    $summary += "$name|OK|$style|$nodes|$edges|$([math]::Round($time,1))s"
}

$summaryFile = Join-Path $runDir "bench-summary.txt"
$summary -join "`n" | Out-File $summaryFile -Encoding utf8
Write-Host "`nSummary -> $summaryFile" -ForegroundColor Cyan
Write-Host "Done: $(($summary | Where-Object { $_ -match '\|OK\|' }).Count) OK, $(($summary | Where-Object { $_ -match '\|FAIL\||\|STUB\|' }).Count) failed/stub" -ForegroundColor Yellow
