$ErrorActionPreference = 'Continue'
$dir = 'C:/Code/DevContext2-engine/eval-results/2026-08-13'

Write-Host "=== 1/2 INVARIANT PROBE (after) ==="
& powershell -NoProfile -ExecutionPolicy Bypass -File "$dir/v1-invariants/invariant-probe.ps1" `
    -Out "$dir/v1-invariants/invariants-after.txt"

Write-Host ""
Write-Host "=== 2/2 V1.2 MEMBER-TITLE PROBE (regression check: Member must stay 1716/0) ==="
& powershell -NoProfile -ExecutionPolicy Bypass -File "$dir/v1-vocabulary/member-title-probe.ps1" `
    -Out "$dir/v1-invariants/member-title-after-v13.txt"

Write-Host "AFTER RUN DONE"
