# G6.3 - run the FULL gate battery and record its verdict. ASCII only (PS 5.1 codepage trap).
$ErrorActionPreference = 'Continue'
Set-Location C:/code/DevContext2
$log = 'C:/code/DevContext2/eval-results/2026-07-29/G6/G6.3-battery.log'
powershell -NoProfile -ExecutionPolicy Bypass -File C:/code/DevContext2/eval/gates.ps1 *> $log
$code = $LASTEXITCODE
Add-Content -Path $log -Value ("BATTERY-EXIT=" + $code)
Write-Output ("BATTERY-EXIT=" + $code)
exit $code
