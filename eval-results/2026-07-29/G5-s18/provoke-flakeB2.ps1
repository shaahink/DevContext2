# G5 s18 — provocation, take 2. Take 1 used Start-Job and produced 0/8; since the direct key probe
# (probe-versionkey.ps1) PROVED an unrelated repo write moves the key, the likely fault was the job
# never running. This version spawns a real churn PROCESS and proves it churned by counting how many
# times the file's mtime moved during the run.
param([int]$Iterations = 6)

$ErrorActionPreference = 'Continue'
$root  = 'C:\code\DevContext2'
$churn = Join-Path $root 'eval-results\2026-07-29\G5-s18\churn2.tmp'
$logDir = Join-Path $env:TEMP ('g5s18-provoke2-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$churnScript = "for (`$n=0; `$n -lt 4000; `$n++) { Set-Content -LiteralPath '$churn' -Value `$n; Start-Sleep -Milliseconds 200 }"
$proc = Start-Process powershell -ArgumentList '-NoProfile', '-Command', $churnScript -PassThru -WindowStyle Hidden
"CHURN PROCESS PID $($proc.Id)"
Start-Sleep -Seconds 2
"churn file exists after 2s: $(Test-Path $churn)  (content: $(if (Test-Path $churn) { Get-Content $churn -Raw } else { 'n/a' }))"

$fails = 0
for ($i = 1; $i -le $Iterations; $i++) {
    $mtimeBefore = (Get-Item $churn).LastWriteTimeUtc
    $log = Join-Path $logDir "p2-$i.txt"
    & dotnet test "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" --no-build --nologo `
        --filter "FullyQualifiedName~AnalyzeCacheTruthTests" > $log 2>&1
    $code = $LASTEXITCODE
    $mtimeAfter = (Get-Item $churn).LastWriteTimeUtc
    $churned = $mtimeAfter -ne $mtimeBefore
    if ($code -ne 0) {
        $fails++
        $names = (Select-String -Path $log -Pattern '\[FAIL\]' | ForEach-Object { ($_.Line -split '\s+')[-2] }) -join ', '
        "P2 ITER $i EXIT $code churned=$churned FAIL: $names  ($log)"
        Copy-Item $log (Join-Path $root "eval-results\2026-07-29\G5-s18\provoke2-FAIL-$i.txt") -Force
    } else {
        "P2 ITER $i EXIT 0 churned=$churned pass"
    }
}
Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $churn -ErrorAction SilentlyContinue
"P2 TOTAL FAILURES: $fails / $Iterations  (logs: $logDir)"
