# G5 s18 — direct probe of the claim "an unrelated repo write changes a fixture's snapshot version key".
# No tests involved: analyse the fixture through the CLI into a PRIVATE cache root, touch an untracked
# file elsewhere in the repo, analyse again, and count the snapshot files that appear.
#   2 files with different -dirty- suffixes -> the key really does track the whole working tree.
#   1 file                                  -> it does not, and flake B has some other cause.
# Traps obeyed: `analyze` takes a POSITIONAL path (--path would silently analyse the CWD) and the path
# must be ABSOLUTE (a relative path is parsed as a GitHub repo and cloned).
$ErrorActionPreference = 'Continue'
$root    = 'C:\code\DevContext2'
$fixture = Join-Path $root 'tests\fixtures\ControllerApp'
$cli     = Join-Path $root 'src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe'
$cache   = Join-Path $env:TEMP ('g5s18-keyprobe-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
$churn   = Join-Path $root 'eval-results\2026-07-29\G5-s18\keyprobe-churn.tmp'
New-Item -ItemType Directory -Force -Path $cache | Out-Null
$env:DEVCONTEXT_CACHE_ROOT = $cache
"CLI exists: $(Test-Path $cli)  |  cache root: $cache"

"--- analyze #1 ---"
& $cli analyze $fixture --format json > (Join-Path $env:TEMP 'g5s18-a1.json') 2>&1
"exit=$LASTEXITCODE"
$after1 = Get-ChildItem $cache -Recurse -Filter *.snap.json.gz | ForEach-Object { $_.Name }
"snapshots after #1: $($after1.Count)"; $after1

"--- touching an untracked repo file (nothing to do with the fixture) ---"
Set-Content -LiteralPath $churn -Value ("churn " + [Guid]::NewGuid())
Start-Sleep -Milliseconds 400
"git sees it: " + ((& git -C $root status --porcelain -uall | Select-String 'keyprobe-churn') -join '')

"--- analyze #2 ---"
& $cli analyze $fixture --format json > (Join-Path $env:TEMP 'g5s18-a2.json') 2>&1
"exit=$LASTEXITCODE"
$after2 = Get-ChildItem $cache -Recurse -Filter *.snap.json.gz | ForEach-Object { $_.Name }
"snapshots after #2: $($after2.Count)"; $after2

Remove-Item -LiteralPath $churn -ErrorAction SilentlyContinue
"VERDICT: $(if ($after2.Count -gt $after1.Count) { 'KEY MOVED - an unrelated repo write invalidates the fixture snapshot' } else { 'KEY STABLE - the repo write does NOT change the key' })"
