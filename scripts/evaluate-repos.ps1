param(
    [string]$ReposFile = "eval-repos.json",
    [string]$OutputDir = "eval-results",
    [string]$CliProject = "src\DevContext.Cli"
)

$ErrorActionPreference = "Stop"
$rootDir = Split-Path -Parent $PSScriptRoot
$reposPath = Join-Path $rootDir $ReposFile
$outputRoot = Join-Path $rootDir $OutputDir

if (-not (Test-Path $reposPath)) {
    Write-Error "Repos file not found: $reposPath"
    exit 1
}

$config = Get-Content $reposPath -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

foreach ($repo in $config.repos) {
    $repoDir = Join-Path $rootDir "eval-repos" $repo.name
    $repoOutput = Join-Path $outputRoot $repo.name
    New-Item -ItemType Directory -Path $repoOutput -Force | Out-Null

    Write-Host "=== Processing $($repo.name) ===" -ForegroundColor Cyan

    # Clone or update repo
    if (-not (Test-Path (Join-Path $repoDir ".git"))) {
        Write-Host "Cloning $($repo.name)..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $repoDir -Force | Out-Null
        git clone --depth 1 $repo.url $repoDir 2>&1 | Out-Null
    }
    else {
        Write-Host "Repo exists, fetching..." -ForegroundColor Yellow
        Push-Location $repoDir
        git fetch --depth 1 origin 2>&1 | Out-Null
        Pop-Location
    }

    # Run scenarios
    foreach ($scenario in $repo.scenarios) {
        foreach ($format in @("json", "markdown")) {
            $outputFile = Join-Path $repoOutput "$($repo.profile)-$scenario.$format"
            $metricsFile = Join-Path $repoOutput "$($repo.profile)-$scenario-metrics.txt"

            Write-Host "  Running $($repo.profile)/$scenario [$format]..." -ForegroundColor Gray

            $args = @(
                "analyze", $repoDir,
                "--profile", $repo.profile,
                "--scenario", $scenario,
                "--format", $format,
                "--max-tokens", "8000",
                "--metrics",
                "-o", $outputFile
            )

            $result = & dotnet run --project (Join-Path $rootDir $CliProject) -- $args 2>&1
            $exitCode = $LASTEXITCODE

            # Save metrics (everything before the summary box)
            $result | Out-File -FilePath $metricsFile -Encoding utf8

            if ($exitCode -eq 0) {
                Write-Host "    ✓ Completed" -ForegroundColor Green
            }
            else {
                Write-Host "    ✗ Failed (exit $exitCode)" -ForegroundColor Red
            }
        }
    }

    # Dry-run with metrics
    $dryRunOutput = Join-Path $repoOutput "dry-run.txt"
    $dryArgs = @("analyze", $repoDir, "--dry-run", "--metrics")
    $dryResult = & dotnet run --project (Join-Path $rootDir $CliProject) -- $dryArgs 2>&1
    $dryResult | Out-File -FilePath $dryRunOutput -Encoding utf8
    Write-Host "  ✓ Dry-run complete" -ForegroundColor Green

    Write-Host ""
}

Write-Host "=== Evaluation complete ===" -ForegroundColor Cyan
Write-Host "Results in: $outputRoot"
