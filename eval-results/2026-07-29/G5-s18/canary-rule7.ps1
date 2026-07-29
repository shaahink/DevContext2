# G5 s18 — a guard nobody has watched fail is not a guard. Plant the exact pattern Rule 7 bans in a
# throwaway .cs under tests/ (tests/ root has no csproj, so nothing compiles it), run loom-guards, and
# require it to REPORT the file and exit non-zero. Then delete the canary and require green again.
$root   = 'C:\code\DevContext2'
$canary = Join-Path $root 'tests\_rule7canary.cs'

@'
// G5 s18 Rule 7 canary — deleted by canary-rule7.ps1.
namespace Canary;
internal static class Rule7Canary
{
    public static void Redirect(string dir) =>
        System.Environment.SetEnvironmentVariable("DEVCONTEXT_CACHE_ROOT", dir);
}
'@ | Set-Content -LiteralPath $canary -Encoding utf8

Write-Host "########## ARM: canary planted at tests/_rule7canary.cs ##########"
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\scripts\loom-guards.ps1"
$armed = $LASTEXITCODE
Write-Host "########## guards WITH canary EXIT = $armed  (must be non-zero) ##########"

Remove-Item -LiteralPath $canary -Force
Write-Host ""
Write-Host "########## DISARM: canary removed ##########"
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\scripts\loom-guards.ps1"
$clean = $LASTEXITCODE
Write-Host "########## guards WITHOUT canary EXIT = $clean  (must be 0) ##########"

Write-Host ""
Write-Host "########## RULE 7 CANARY: armed=$armed clean=$clean -> $(if ($armed -ne 0 -and $clean -eq 0) { 'THE GUARD REALLY FIRES' } else { 'VACUOUS GUARD - FIX IT' }) ##########"
