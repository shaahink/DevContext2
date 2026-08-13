# App gate for the V1.1 app-side edits. `pnpm` on this machine only resolves through its PowerShell
# shim, so it must be invoked with & from a .ps1 - never as a bare exe from a spawned process.
Set-Location 'C:\Code\DevContext2-engine\src\DevContext.App'
& pnpm check
Write-Host "pnpm check exit $LASTEXITCODE"
