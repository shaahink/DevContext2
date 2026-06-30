param(
    [switch]$Fast,
    [switch]$OnlyNew
)
$ErrorActionPreference = 'Continue'
$repoRoot = $PSScriptRoot
$cliProject = Join-Path $repoRoot "..\..\src\DevContext.Cli"
$evalRoot = Join-Path $repoRoot "..\..\eval-repos"

# Build
Write-Host "=== Building CLI ==="
dotnet build $cliProject -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }

# Repo definitions
$repos = @(
    @{Name="TodoApi";           SubPath="TodoApi"},
    @{Name="DntSite";           SubPath="DntSite"},
    @{Name="eShop";             SubPath="eShop"; Huge=$true},
    @{Name="eShop-Ordering";    SubPath="eShop\src\Ordering.API"},
    @{Name="VerticalSlice";     SubPath="VerticalSlice"},
    @{Name="OrchardCore";       SubPath="OrchardCore"; Huge=$true},
    @{Name="AutoMapper";        SubPath="AutoMapper"},
    @{Name="FluentValidation";  SubPath="FluentValidation"},
    @{Name="Polly";             SubPath="Polly"},
    @{Name="CommunityToolkit";  SubPath="CommunityToolkit.Mvvm"},
    @{Name="MediatR";           SubPath="MediatR"},
    @{Name="RestSharp";         SubPath="RestSharp"},
    @{Name="Serilog";           SubPath="Serilog"},
    @{Name="Hangfire";          SubPath="Hangfire"},
    @{Name="Dapper";            SubPath="Dapper"},
    @{Name="xUnit";             SubPath="xUnit"},
    @{Name="Quartz.NET";        SubPath="Quartz.NET"},
    @{Name="YARP";              SubPath="YARP"},
    @{Name="CLI-cmdline";       SubPath="CLI"},
    @{Name="gRPC";              SubPath="gRPC"},
    @{Name="Functions";         SubPath="Functions"},
    @{Name="MahApps.Metro";     SubPath="MahApps.Metro\src\MahApps.Metro"},
    @{Name="MassTransit";       SubPath="MassTransit\src"; Huge=$true},
    @{Name="SignalR-Server";    SubPath="SignalR\src\SignalR"; Huge=$true},
    @{Name="HotChocolate";      SubPath="HotChocolate\src"; Huge=$true},
    @{Name="Orleans";           SubPath="Orleans"; Huge=$true}
)

$oldNames = @('TodoApi','DntSite','eShop','eShop-Ordering','VerticalSlice','OrchardCore','AutoMapper','FluentValidation','Polly','CommunityToolkit','MediatR')
$total = 0; $passed = 0; $failed = 0; $skipped = 0
$results = @()

foreach ($repo in $repos) {
    if ($OnlyNew -and ($repo.Name -in $oldNames)) { continue }
    if ($Fast -and $repo.Huge) { 
        Write-Host "SKIP $($repo.Name) --fast" -ForegroundColor Yellow; $skipped++; continue 
    }
    
    $absPath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($evalRoot, $repo.SubPath))
    if (-not (Test-Path $absPath)) {
        Write-Host "MISS $($repo.Name): $absPath" -ForegroundColor Red; $skipped++; continue
    }
    
    $total++
    Write-Host "ANALYZE $($repo.Name)" -ForegroundColor Cyan
    
    $outJson = Join-Path $repoRoot "$($repo.Name).json"
    $outMd = Join-Path $repoRoot "$($repo.Name)-map.md"
    
    $sw = [Diagnostics.Stopwatch]::StartNew()
    
    # Run JSON analysis
    $jsonResult = & dotnet run --no-build --project $cliProject -- analyze $absPath --format json -o $outJson --stats 2>&1
    $exitCode = $LASTEXITCODE
    $sw.Stop()
    $elapsed = [math]::Round($sw.Elapsed.TotalSeconds, 1)
    
    # Get markdown output too
    $mdResult = & dotnet run --no-build --project $cliProject -- analyze $absPath -o $outMd 2>&1
    
    # Parse entries from JSON
    $entryCount = 0
    $archStyle = "?"
    $detectionCount = 0
    if ((Test-Path $outJson) -and $exitCode -eq 0) {
        try {
            $json = Get-Content $outJson -Raw | ConvertFrom-Json
            $entryCount = @($json.entries).Count
            $archStyle = $json.architecture.style
            $detectionCount = @($json.detections).Count
        } catch { }
    }
    
    # Extract stats
    $statsLine = ""
    $statsMatch = ($jsonResult | Select-String 'nodes.*edges' -CaseSensitive:$false | Select-Object -First 1)
    if ($statsMatch) { $statsLine = $statsMatch.Line }
    
    $status = if ($exitCode -eq 0) { "PASS" } else { "FAIL($exitCode)" }
    if ($exitCode -eq 0) { $passed++ } else { $failed++ }
    
    $row = [PSCustomObject]@{Name=$repo.Name; Elapsed=$elapsed; Status=$status; Style=$archStyle; Entries=$entryCount; Detections=$detectionCount}
    $results += $row
    Write-Host "  $status  ${elapsed}s  style=$archStyle  entries=$entryCount  det=$detectionCount" -ForegroundColor $(if ($exitCode -eq 0) { 'Green' } else { 'Red' })
}

Write-Host "`n=== RESULTS ===" -ForegroundColor Cyan
$results | Format-Table -AutoSize
Write-Host "Total: $total | Pass: $passed | Fail: $failed | Skip: $skipped"
Write-Host "Output: $repoRoot"
