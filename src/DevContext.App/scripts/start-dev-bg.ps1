param(
  [switch]$Kill,
  [switch]$Status
)

$AppDir = Split-Path -Parent $PSScriptRoot
$ServerDll = Join-Path $AppDir "..\DevContext.Server\bin\Debug\net10.0\DevContext.Server.dll"
$ServerUrl = "http://127.0.0.1:5179"
$WebPort = 4200

function Get-JobByName($name) {
  Get-Job | Where-Object { $_.Name -eq $name -and $_.State -eq 'Running' }
}

function Kill-All {
  $names = @('devcontext-server', 'devcontext-web')
  foreach ($n in $names) {
    $j = Get-JobByName $n
    if ($j) {
      Stop-Job $j -ErrorAction SilentlyContinue
      Remove-Job $j -Force -ErrorAction SilentlyContinue
      Write-Host "[kill] Stopped job: $n"
    }
  }
  # Kill orphan dotnet/ng processes on our ports
  Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
    $_.CommandLine -match "DevContext.Server"
  } | Stop-Process -Force -ErrorAction SilentlyContinue
  $nodeProc = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
    ($_.CommandLine -match "ng serve") -or ($_.CommandLine -match "@angular")
  }
  if ($nodeProc) { $nodeProc | Stop-Process -Force -ErrorAction SilentlyContinue }
  Write-Host "[kill] Cleaned up orphan processes"
}

if ($Kill) {
  Kill-All
  exit 0
}

if ($Status) {
  $server = Get-JobByName 'devcontext-server'
  $web = Get-JobByName 'devcontext-web'
  $sOk = $server -and $server.State -eq 'Running'
  $wOk = $web -and $web.State -eq 'Running'
  if ($sOk -or $wOk) {
    Write-Host "server=$($sOk) web=$($wOk)"
  } else {
    Write-Host "No running jobs"
  }
  exit 0
}

# Kill any leftovers first
Kill-All

Write-Host "[start] Building server..."
& "dotnet" "build" (Join-Path $AppDir "..\DevContext.Server") "-clp:ErrorsOnly" "--nologo" | Out-Null
if ($LASTEXITCODE -ne 0) {
  Write-Error "Server build failed"
  exit 1
}

Write-Host "[start] Launching .NET server in background job..."
$serverJob = Start-Job -Name 'devcontext-server' -ScriptBlock {
  param($dll, $url)
  $env:ASPNETCORE_ENVIRONMENT = "Development"
  Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList @($dll, "--urls", $url) -RedirectStandardOutput "NUL" -RedirectStandardError "NUL"
  # Keep job alive briefly to detect early exit
  Start-Sleep -Seconds 3
  $p = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -match "DevContext.Server" }
  if (-not $p) { throw "Server process exited early" }
  # Stay alive so job stays Running
  while ($true) { Start-Sleep -Seconds 10 }
} -ArgumentList @( (Resolve-Path $ServerDll).Path, $ServerUrl )

# Wait for server health
Write-Host "[wait] Waiting for server /health..."
$deadline = (Get-Date).AddSeconds(90)
$serverUp = $false
while ((Get-Date) -lt $deadline) {
  try {
    $r = Invoke-WebRequest -Uri "$ServerUrl/health" -UseBasicParsing -ErrorAction Stop
    if ($r.StatusCode -eq 200) { $serverUp = $true; break }
  } catch {}
  Start-Sleep -Milliseconds 800
}
if (-not $serverUp) {
  Write-Error "Server failed to start within 90s"
  Kill-All
  exit 1
}
Write-Host "[ready] .NET server is up"

Write-Host "[start] Launching Angular dev server in background job..."
$webJob = Start-Job -Name 'devcontext-web' -ScriptBlock {
  param($cwd, $port)
  Set-Location $cwd
  Start-Process -NoNewWindow -FilePath "pnpm" -ArgumentList @("ng", "serve", "--port", "$port") -RedirectStandardOutput "NUL" -RedirectStandardError "NUL"
  Start-Sleep -Seconds 5
  $p = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -match "ng serve" -or $_.CommandLine -match "@angular" }
  if (-not $p) { throw "Angular dev server exited early" }
  while ($true) { Start-Sleep -Seconds 10 }
} -ArgumentList @($AppDir, $WebPort)

# Wait for web
Write-Host "[wait] Waiting for Angular dev server..."
$deadline = (Get-Date).AddSeconds(120)
$webUp = $false
while ((Get-Date) -lt $deadline) {
  try {
    $r = Invoke-WebRequest -Uri "http://localhost:$WebPort" -UseBasicParsing -ErrorAction Stop
    if ($r.StatusCode -eq 200) { $webUp = $true; break }
  } catch {}
  Start-Sleep -Milliseconds 800
}
if (-not $webUp) {
  Write-Error "Angular dev server failed to start within 120s"
  Kill-All
  exit 1
}
Write-Host "[ready] Angular dev server is up"
Write-Host ""
Write-Host "  Server: $ServerUrl"
Write-Host "  Web:    http://localhost:$WebPort"
Write-Host ""
Write-Host "Use './scripts/start-dev-bg.ps1 -Kill' to stop all"
