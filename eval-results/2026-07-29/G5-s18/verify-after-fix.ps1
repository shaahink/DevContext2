# G5 s18 — verification of the cache-root injection, plus one instrument.
#
# Every iteration log goes to $env:TEMP, NOT into the repo: eval-results is untracked-but-not-ignored,
# so a log file growing inside the repo changes the snapshot version key mid-test (flake B) and the
# harness manufactures its own red. That is what polluted the first discriminator run.
#
# INSTRUMENT: around each iteration, fingerprint every file `git status --porcelain -uall` lists
# (path + mtime + length — the same inputs GitHeadReader.ReadDirtyFingerprint hashes). If the fingerprint
# moves across a test run, then the fast suite mutates its own repo while running and flake B is
# reachable in the real battery, not just under this harness.
param([int]$Iterations = 25, [string]$Filter = '')

$ErrorActionPreference = 'Continue'
$root = 'C:\code\DevContext2'
$out  = Join-Path $root 'eval-results\2026-07-29\G5-s18'
$logDir = Join-Path $env:TEMP ('g5s18-verify-' + [Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
"LOG DIR (outside the repo): $logDir"

function Get-TreeFingerprint {
    Push-Location $root
    $lines = & git status --porcelain -uall 2>$null
    Pop-Location
    $sb = New-Object Text.StringBuilder
    foreach ($l in $lines) {
        if ($l.Length -lt 4) { continue }
        $rel = $l.Substring(3).Trim('"', ' ')
        $full = Join-Path $root $rel
        [void]$sb.Append($rel).Append('|')
        if (Test-Path -LiteralPath $full -PathType Leaf) {
            $fi = Get-Item -LiteralPath $full
            [void]$sb.Append($fi.LastWriteTimeUtc.Ticks).Append(':').Append($fi.Length)
        }
        [void]$sb.Append("`n")
    }
    $md5 = [Security.Cryptography.MD5]::Create()
    return [Convert]::ToHexString($md5.ComputeHash([Text.Encoding]::UTF8.GetBytes($sb.ToString()))).Substring(0, 12)
}

$fails = 0
$treeMoved = 0
for ($i = 1; $i -le $Iterations; $i++) {
    $before = Get-TreeFingerprint
    $log = Join-Path $logDir "verify-$i.txt"
    if ($Filter) {
        & dotnet test "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" --no-build --nologo --filter $Filter > $log 2>&1
    } else {
        & dotnet test "$root\tests\DevContext.Server.Tests\DevContext.Server.Tests.csproj" --no-build --nologo > $log 2>&1
    }
    $code = $LASTEXITCODE
    $after = Get-TreeFingerprint
    if ($before -ne $after) { $treeMoved++ }
    if ($code -ne 0) {
        $fails++
        Copy-Item $log (Join-Path $out "verify-FAIL-$i.txt") -Force
    }
    "VERIFY ITER $i EXIT $code tree $before -> $after $(if ($before -ne $after) { 'MOVED' } else { 'stable' })"
}
"VERIFY TOTAL FAILURES: $fails / $Iterations"
"TREE MOVED DURING A TEST RUN: $treeMoved / $Iterations"
