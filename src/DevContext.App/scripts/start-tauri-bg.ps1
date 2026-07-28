# Launches the Tauri desktop shell against an ALREADY-RUNNING dev stack.
#
# `pnpm dev` / `tauri dev` normally runs beforeDevCommand ("pnpm start"), which spawns a second
# Angular dev server and collides with the one start-dev-bg.ps1 already has on 4200. This blanks
# that hook so the shell just points at the running http://localhost:4200 and the running .NET
# server on 5179 — no rebuild of either.
#
# Run start-dev-bg.ps1 FIRST. Logs land next to the other dev logs.
# Keep this file ASCII (PS 5.1 detached-run encoding gotcha).
$ErrorActionPreference = 'Stop'
$appDir = Split-Path -Parent $PSScriptRoot
Set-Location $appDir

$logDir = Join-Path $appDir '.dev-logs'
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }

Write-Host '[tauri] compiling the Rust shell (first run takes several minutes)...'
Write-Host '[tauri] the web app stays usable at http://localhost:4200 while this builds'

# The override goes through a FILE, not an inline JSON argument: PowerShell -> pnpm -> tauri strips
# the double quotes on the way, and tauri rejects `{build:{beforeDevCommand:}}` as invalid JSON.
$overridePath = Join-Path $logDir 'tauri.no-before-dev.json'
'{"build":{"beforeDevCommand":""}}' | Out-File -FilePath $overridePath -Encoding utf8 -NoNewline
& pnpm exec tauri dev --config $overridePath
