#!/usr/bin/env pwsh
# Build the WPF/BlazorWebView desktop app, launch it, wait for its window to
# render, screenshot JUST the app window (not the whole desktop), then close it.
# This is the agent handle for "screenshot the desktop" — driving the WebView2
# DOM itself is out of scope; this proves the app builds and renders.
#
#   powershell -File .claude/skills/run-devcontext/desktop-shot.ps1
#   -> writes .claude/skills/run-devcontext/desktop-launch.png

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path "$PSScriptRoot/../../..").Path
$proj = Join-Path $repo 'src/DevContext.Desktop'

Write-Host "== build desktop ==" -ForegroundColor Cyan
dotnet build $proj -clp:ErrorsOnly | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Host "build failed ($LASTEXITCODE)" -ForegroundColor Red; exit 1 }
$exe = (Get-ChildItem (Join-Path $proj 'bin/Debug') -Recurse -Filter 'DevContext.Desktop.exe' | Select-Object -First 1).FullName
if (-not $exe) { Write-Host 'DevContext.Desktop.exe not found' -ForegroundColor Red; exit 1 }

Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public struct RECT { public int Left, Top, Right, Bottom; }
public static class Win32 {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
}
'@

Write-Host "== launch ==" -ForegroundColor Cyan
$p = Start-Process $exe -PassThru

# Poll for the main window handle instead of a blind sleep; then let WebView2 settle.
$deadline = (Get-Date).AddSeconds(40)
while ($p.MainWindowHandle -eq 0 -and (Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 400; $p.Refresh()
}
if ($p.MainWindowHandle -eq 0) {
    Write-Host 'window never appeared' -ForegroundColor Red
    if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }
    exit 1
}
[Win32]::ShowWindow($p.MainWindowHandle, 9) | Out-Null   # SW_RESTORE
[Win32]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
Start-Sleep -Seconds 6                                   # WebView2 DOM paint

$r = New-Object RECT
[Win32]::GetWindowRect($p.MainWindowHandle, [ref]$r) | Out-Null
$w = $r.Right - $r.Left; $h = $r.Bottom - $r.Top
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.Left, $r.Top, 0, 0, (New-Object System.Drawing.Size $w, $h))
$shot = Join-Path $PSScriptRoot 'desktop-launch.png'
$bmp.Save($shot, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }
Write-Host "saved $shot ($w x $h)" -ForegroundColor Green
