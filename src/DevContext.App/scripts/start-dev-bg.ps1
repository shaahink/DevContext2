param(
  [switch]$Kill,
  [switch]$Status
)

# T7.0 rewrite (wall-speed rider): the old form nested Start-Process inside Start-Job. Two
# session-killers hid in that: (1) Start-Process with -RedirectStandardOutput uses
# UseShellExecute=false, which CANNOT launch a .cmd shim (pnpm.cmd) — the web "job" died
# before writing a byte, logs stayed empty, and the 240s wait always timed out; (2) job
# state lived in the launching PowerShell session, so -Status/-Kill from a NEW shell saw
# nothing. Now: direct process starts (cmd /d /c for the pnpm shim), pid files under
# .dev-pids/, and taskkill /T tree-kills — any shell can start/status/kill.

$AppDir = Split-Path -Parent $PSScriptRoot
$ServerDll = Join-Path $AppDir "..\DevContext.Server\bin\Debug\net10.0\DevContext.Server.dll"
$ServerUrl = "http://127.0.0.1:5179"
$WebPort = 4200
$LogDir = Join-Path $AppDir ".dev-logs"
$PidDir = Join-Path $AppDir ".dev-pids"
New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
New-Item -ItemType Directory -Path $PidDir -Force | Out-Null
$ServerPidFile = Join-Path $PidDir "server.pid"
$WebPidFile = Join-Path $PidDir "web.pid"

function Get-PidFromFile($file) {
  if (-not (Test-Path $file)) { return $null }
  $procId = (Get-Content $file -ErrorAction SilentlyContinue | Select-Object -First 1) -as [int]
  if ($procId -and (Get-Process -Id $procId -ErrorAction SilentlyContinue)) { return $procId }
  return $null
}

function Kill-Tree($file, $label) {
  $procId = Get-PidFromFile $file
  if ($procId) {
    # /T kills the whole descendant tree (cmd -> node, dotnet -> children).
    & taskkill /PID $procId /T /F 2>&1 | Out-Null
    Write-Host "[kill] $label tree (pid $procId)"
  }
  Remove-Item $file -Force -ErrorAction SilentlyContinue
}

function Kill-All {
  Kill-Tree $ServerPidFile 'server'
  Kill-Tree $WebPidFile 'web'
  # Orphan sweep stays as backup (T0.1c): PS 5.1 Get-Process has no CommandLine — use CIM.
  $procs = Get-CimInstance Win32_Process `
    -Filter "Name='dotnet.exe' OR Name='node.exe' OR Name='DevContext.Server.exe'" `
    -ErrorAction SilentlyContinue
  foreach ($proc in $procs) {
    $cmd = $proc.CommandLine
    if (-not $cmd) { continue }
    if (($cmd -match 'DevContext\.Server') -and ($cmd -notmatch 'DevContext\.Server\.Tests')) {
      try { Stop-Process -Id $proc.ProcessId -Force -ErrorAction Stop; Write-Host "[kill] orphan server pid $($proc.ProcessId)" } catch {}
    }
    elseif (($cmd -match 'ng serve') -or ($cmd -match '@angular')) {
      try { Stop-Process -Id $proc.ProcessId -Force -ErrorAction Stop; Write-Host "[kill] orphan web pid $($proc.ProcessId)" } catch {}
    }
  }
  Write-Host "[kill] Done"
}

function Test-Url($url) {
  try {
    $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
    return $r.StatusCode -eq 200
  } catch { return $false }
}

if ($Kill) {
  Kill-All
  exit 0
}

if ($Status) {
  $sPid = Get-PidFromFile $ServerPidFile
  $wPid = Get-PidFromFile $WebPidFile
  $sUp = Test-Url "$ServerUrl/health"
  $wUp = Test-Url "http://localhost:$WebPort"
  Write-Host "server: pid=$(if ($sPid) { $sPid } else { '-' }) health=$sUp"
  Write-Host "web:    pid=$(if ($wPid) { $wPid } else { '-' }) http=$wUp"
  if ($sUp -and $wUp) { exit 0 } else { exit 1 }
}

# Idempotent start: if both ends already answer, do nothing (drives can call this blindly).
if ((Test-Url "$ServerUrl/health") -and (Test-Url "http://localhost:$WebPort")) {
  Write-Host "[ready] server + web already up (idempotent start)"
  exit 0
}

Kill-All

Write-Host "[start] Building server..."
& "dotnet" "build" (Join-Path $AppDir "..\DevContext.Server") "-clp:ErrorsOnly" "--nologo" | Out-Null
if ($LASTEXITCODE -ne 0) {
  Write-Error "Server build failed"
  exit 1
}

Write-Host "[start] Launching .NET server... (logs: $LogDir\server.*.log)"
# Logs are redirected BY cmd, not by PowerShell: PS-level -RedirectStandardOutput forces
# CreateProcess with handle inheritance, and the daemon child then holds a copy of the
# caller's stdout pipe — an agent/CI shell piping this script never sees EOF and "hangs"
# on a healthy start. The ShellExecute path (no PS redirect) inherits nothing.
$serverCmd = "set ASPNETCORE_ENVIRONMENT=Development&& dotnet ""$((Resolve-Path $ServerDll).Path)"" --urls $ServerUrl > ""$(Join-Path $LogDir 'server.out.log')"" 2> ""$(Join-Path $LogDir 'server.err.log')"""
$serverProc = Start-Process cmd -ArgumentList @('/d', '/c', $serverCmd) -WindowStyle Hidden -PassThru
Set-Content $ServerPidFile $serverProc.Id

Write-Host "[wait] Waiting for server /health..."
$deadline = (Get-Date).AddSeconds(90)
$serverUp = $false
while ((Get-Date) -lt $deadline) {
  if ($serverProc.HasExited) { break }
  if (Test-Url "$ServerUrl/health") { $serverUp = $true; break }
  Start-Sleep -Milliseconds 800
}
if (-not $serverUp) {
  Write-Error "Server failed to start within 90s (see $LogDir\server.err.log)"
  Kill-All
  exit 1
}
Write-Host "[ready] .NET server is up"

# pnpm is a .cmd shim (CreateProcess can't exec it directly) — cmd /d /c runs it AND owns
# the log redirection (same no-inheritance rationale as the server above). The saved pid is
# cmd's; taskkill /T reaches the node tree beneath it.
Write-Host "[start] Launching Angular dev server... (logs: $LogDir\web.*.log)"
$webCmd = "pnpm ng serve --port $WebPort > ""$(Join-Path $LogDir 'web.out.log')"" 2> ""$(Join-Path $LogDir 'web.err.log')"""
$webProc = Start-Process cmd -ArgumentList @('/d', '/c', $webCmd) `
  -WorkingDirectory $AppDir -WindowStyle Hidden -PassThru
Set-Content $WebPidFile $webProc.Id

Write-Host "[wait] Waiting for Angular dev server (up to 240s)..."
$deadline = (Get-Date).AddSeconds(240)
$webUp = $false
while ((Get-Date) -lt $deadline) {
  if ($webProc.HasExited) {
    Write-Error "Angular dev server exited early (see $LogDir\web.err.log / web.out.log)"
    Kill-All
    exit 1
  }
  if (Test-Url "http://localhost:$WebPort") { $webUp = $true; break }
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
Write-Host "  Pids:   $PidDir"
Write-Host ""
Write-Host "Use './scripts/start-dev-bg.ps1 -Kill' to stop all"
