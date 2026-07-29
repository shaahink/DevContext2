# G5 s18 verification: the same two fast gates s17 ran, on the cache-root-injection tree.
# Exit codes captured explicitly (a gate that dies mid-stream prints no GATE: line — bug #15).
$root = "C:\code\DevContext2"
$out  = "$root\eval-results\2026-07-29\G5-s18"
Set-Location $root

Write-Host "########## GATE 1/2: fast-engine ##########"
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\eval\gates.ps1" -SkipEval -Scope engine
$g1 = $LASTEXITCODE
Write-Host "########## fast-engine EXIT = $g1 ##########"
Set-Content -Path "$out\verify-fast-engine.exit.txt" -Value $g1 -Encoding utf8

Write-Host ""
Write-Host "########## GATE 2/2: guards (now carrying Rule 7) ##########"
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\scripts\loom-guards.ps1"
$g2 = $LASTEXITCODE
Write-Host "########## guards EXIT = $g2 ##########"
Set-Content -Path "$out\verify-guards.exit.txt" -Value $g2 -Encoding utf8

Write-Host ""
Write-Host "########## VERDICT: fast-engine=$g1 guards=$g2 ##########"
