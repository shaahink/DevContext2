param(
  [switch]$Kill,
  [switch]$Status
)

$AppDir = Split-Path -Parent $PSScriptRoot
$ServerDll = Join-Path $AppDir "..\DevContext.Server\bin\Debug\net10.0\DevContext.Server.dll"
$ServerUrl = "http://127.0.0.1:5179"
$WebPort = 4200
# T0.1c: job output goes to real log files (was "NUL") so a timeout is diagnosable.
$LogDir = Join-Path $AppDir ".dev-logs"
New-Item -ItemType Directory -Path $LogDir -Force | Out-Null

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
  # Kill orphan server/web processes. NOTE (T0.1c): Windows PowerShell 5.1's Get-Process does NOT
  # expose CommandLine, so the old `Get-Process | Where { $_.CommandLine -match ... }` filter
  # silently matched nothing — every Angular-wait timeout leaked the dotnet server it had just
  # started, and the next build failed with a locked bin/. Query CIM/WMI, which does carry
  # CommandLine, and match on the server dll / ng-serve command line instead.
  $procs = Get-CimInstance Win32_Process `
    -Filter "Name='dotnet.exe' OR Name='node.exe' OR Name='DevContext.Server.exe'" `
    -ErrorAction SilentlyContinue
  foreach ($proc in $procs) {
    $cmd = $proc.CommandLine
    if (-not $cmd) { continue }
    if (($cmd -match 'DevContext\.Server') -and ($cmd -notmatch 'DevContext\.Server\.Tests')) {
      try { Stop-Process -Id $proc.ProcessId -Force -ErrorAction Stop; Write-Host "[kill] server pid $($proc.ProcessId)" } catch {}
    }
    elseif (($cmd -match 'ng serve') -or ($cmd -match '@angular')) {
      try { Stop-Process -Id $proc.ProcessId -Force -ErrorAction Stop; Write-Host "[kill] web pid $($proc.ProcessId)" } catch {}
    }
  }
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

Write-Host "[start] Launching .NET server in background job... (logs: $LogDir\server.*.log)"
$serverJob = Start-Job -Name 'devcontext-server' -ScriptBlock {
  param($dll, $url, $outLog, $errLog)
  $env:ASPNETCORE_ENVIRONMENT = "Development"
  Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList @($dll, "--urls", $url) -RedirectStandardOutput $outLog -RedirectStandardError $errLog
  # Keep job alive briefly to detect early exit
  Start-Sleep -Seconds 3
  $p = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -match "DevContext.Server" }
  if (-not $p) { throw "Server process exited early" }
  # Stay alive so job stays Running
  while ($true) { Start-Sleep -Seconds 10 }
} -ArgumentList @( (Resolve-Path $ServerDll).Path, $ServerUrl, (Join-Path $LogDir "server.out.log"), (Join-Path $LogDir "server.err.log") )

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
  Write-Error "Server failed to start within 90s (see $LogDir\server.err.log)"
  Kill-All
  exit 1
}
Write-Host "[ready] .NET server is up"

Write-Host "[start] Launching Angular dev server in background job... (logs: $LogDir\web.*.log)"
$webJob = Start-Job -Name 'devcontext-web' -ScriptBlock {
  param($cwd, $port, $outLog, $errLog)
  Set-Location $cwd
  Start-Process -NoNewWindow -FilePath "pnpm" -ArgumentList @("ng", "serve", "--port", "$port") -RedirectStandardOutput $outLog -RedirectStandardError $errLog
  Start-Sleep -Seconds 5
  $p = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -match "ng serve" -or $_.CommandLine -match "@angular" }
  if (-not $p) { throw "Angular dev server exited early" }
  while ($true) { Start-Sleep -Seconds 10 }
} -ArgumentList @($AppDir, $WebPort, (Join-Path $LogDir "web.out.log"), (Join-Path $LogDir "web.err.log"))

# Wait for web. T0.1c: raised 120s -> 240s (a cold ng build routinely exceeds 120s), and on timeout
# Kill-All now actually kills the server it started (see the CIM fix above) instead of leaking it.
Write-Host "[wait] Waiting for Angular dev server (up to 240s)..."
$deadline = (Get-Date).AddSeconds(240)
$webUp = $false
while ((Get-Date) -lt $deadline) {
  try {
    $r = Invoke-WebRequest -Uri "http://localhost:$WebPort" -UseBasicParsing -ErrorAction Stop
    if ($r.StatusCode -eq 200) { $webUp = $true; break }
  } catch {}
  Start-Sleep -Milliseconds 800
}
if (-not $webUp) {
  Write-Error "Angular dev server failed to start within 240s (see $LogDir\web.err.log)"
  Kill-All
  exit 1
}
Write-Host "[ready] Angular dev server is up"
Write-Host ""
Write-Host "  Server: $ServerUrl"
Write-Host "  Web:    http://localhost:$WebPort"
Write-Host "  Logs:   $LogDir"
Write-Host ""
Write-Host "Use './scripts/start-dev-bg.ps1 -Kill' to stop all"
