# eval/contract-sweep.ps1 -- S9 contract sweep: find fields that are DEAD BY CONSTRUCTION.
#
# Why this exists: three separate times in this program (S7 kind glyph, S8 MapResponse.scope_note,
# S8 ArchetypeView) the same defect was found BY ACCIDENT while doing something else -- the engine
# computes a fact, it rides the proto, the CLI renders it, and the app's generated copy of the
# contract is never read. Each was worth a feature; each cost a session to notice. This turns that
# accident into a check.
#
# Two directions, because the defect has two shapes:
#   unread   a response field NO client reads (app TS/HTML, MCP C#, CLI C#)  -- computed for nobody
#   unwritten a response field the server never assigns                      -- always default on the wire
#
# Expected-unread fields live in eval/expectations/contract-sweep-allow.txt, one per line as
#   Message.field_name = reason
# A field is only allowed to be unread WITH A STATED REASON: "no client reads it" is the finding,
# so the allow-list is where you say why that is correct (MCP-only RPC, superseded by a better
# source, derivable duplicate). Anything unread and unlisted is a NEW dead field and fails the run.
#
# Usage:
#   powershell -File eval/contract-sweep.ps1              # gate mode: exit 1 on a new dead field
#   powershell -File eval/contract-sweep.ps1 -All         # list every unread field, allow-listed too
#   powershell -File eval/contract-sweep.ps1 -UpdateAllow # rewrite the allow-list from current state
#                                                         # (reasons for existing entries preserved)
# Keep this file ASCII (PS 5.1 detached-run encoding gotcha).

param(
    [switch]$All,
    [switch]$UpdateAllow
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$protoPath = Join-Path $repoRoot 'proto/devcontext/v1/devcontext.proto'
$allowPath = Join-Path $repoRoot 'eval/expectations/contract-sweep-allow.txt'

if (-not (Test-Path $protoPath)) { Write-Error "proto not found: $protoPath"; exit 1 }

# ---------------------------------------------------------------------------
# 1. Parse the proto into message -> fields
# ---------------------------------------------------------------------------
$messages = [ordered]@{}
$current = $null
foreach ($line in (Get-Content $protoPath)) {
    if ($line -match '^message\s+(\w+)') { $current = $Matches[1]; $messages[$current] = @(); continue }
    if ($null -eq $current) { continue }
    if ($line -match '^\}') { $current = $null; continue }
    if ($line -match '^\s*(?:(optional|repeated)\s+)?([\w\.]+)\s+(\w+)\s*=\s*\d+\s*;') {
        $messages[$current] += [pscustomobject]@{
            Name = $Matches[3]; Type = $Matches[2]; Card = $Matches[1]
        }
    }
}

function ConvertTo-Camel([string]$s) {
    $parts = $s -split '_'
    $out = $parts[0]
    for ($i = 1; $i -lt $parts.Count; $i++) {
        if ($parts[$i].Length -gt 0) { $out += $parts[$i].Substring(0, 1).ToUpper() + $parts[$i].Substring(1) }
    }
    return $out
}

function ConvertTo-Pascal([string]$s) {
    $c = ConvertTo-Camel $s
    if ($c.Length -eq 0) { return $c }
    return $c.Substring(0, 1).ToUpper() + $c.Substring(1)
}

# ---------------------------------------------------------------------------
# 2. Load consumer sources. The generated copies of the contract are NOT consumers --
#    devcontext_pb.ts and obj/**/Devcontext.cs mention every field by construction, which is
#    exactly the illusion this sweep exists to see through.
# ---------------------------------------------------------------------------
function Get-SourceBlob([string]$root, [string[]]$patterns) {
    if (-not (Test-Path $root)) { return '' }
    $sb = New-Object System.Text.StringBuilder
    foreach ($p in $patterns) {
        Get-ChildItem -Path $root -Filter $p -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object {
                $f = $_.FullName -replace '\\', '/'
                $f -notmatch '/core/grpc/gen/' -and $f -notmatch '/obj/' -and
                $f -notmatch '/bin/' -and $f -notmatch '/node_modules/' -and $f -notmatch '/dist/'
            } |
            ForEach-Object { [void]$sb.AppendLine((Get-Content $_.FullName -Raw)) }
    }
    return $sb.ToString()
}

Write-Host '[sweep] reading consumers...' -ForegroundColor DarkGray
$tsBlob = Get-SourceBlob (Join-Path $repoRoot 'src/DevContext.App/src') @('*.ts', '*.html')
$csBlob = ''
foreach ($proj in @('src/DevContext.Mcp', 'src/DevContext.Cli', 'src/DevContext.Server')) {
    $csBlob += Get-SourceBlob (Join-Path $repoRoot $proj) @('*.cs')
}
# The server is a WRITER, not a reader -- kept separate so a field the server merely maps outward
# is never mistaken for a field someone consumes.
$serverBlob = Get-SourceBlob (Join-Path $repoRoot 'src/DevContext.Server') @('*.cs')
$clientCsBlob = ''
foreach ($proj in @('src/DevContext.Mcp', 'src/DevContext.Cli')) {
    $clientCsBlob += Get-SourceBlob (Join-Path $repoRoot $proj) @('*.cs')
}

# ---------------------------------------------------------------------------
# 3. Allow-list
# ---------------------------------------------------------------------------
$allow = [ordered]@{}
if (Test-Path $allowPath) {
    foreach ($line in (Get-Content $allowPath)) {
        $t = $line.Trim()
        if ($t.Length -eq 0 -or $t.StartsWith('#')) { continue }
        $kv = $t -split '\s*=\s*', 2
        $allow[$kv[0]] = if ($kv.Count -gt 1) { $kv[1] } else { '(no reason given)' }
    }
}

# ---------------------------------------------------------------------------
# 4. Sweep
# ---------------------------------------------------------------------------
$unread = @()
$unwritten = @()
foreach ($msg in $messages.Keys) {
    foreach ($f in $messages[$msg]) {
        $key = "$msg.$($f.Name)"
        $camel = ConvertTo-Camel $f.Name
        $pascal = ConvertTo-Pascal $f.Name
        $readByApp = $tsBlob -match "(?m)\b$([regex]::Escape($camel))\b"
        $readByCsClient = $clientCsBlob -match "(?m)\b$([regex]::Escape($pascal))\b"
        if (-not $readByApp -and -not $readByCsClient) {
            $unread += [pscustomobject]@{
                Key = $key; Card = $f.Card; Type = $f.Type
                Allowed = $allow.Contains($key); Reason = $allow[$key]
            }
        }
        # A request message is written by clients, not the server -- only response-side fields
        # can be "the server never fills this in".
        #
        # MENTION, not assignment, is the test. Matching `X =` / `X.Add` looked tighter and was
        # simply wrong twice: protobuf-csharp renames a colliding field (SurfaceGroup.Types ->
        # `Types_`), and positional record construction assigns nothing (`new ContextCardSpec(a, b,
        # c)`). A field the writing side never NAMES cannot be filled in; anything else is noise
        # in a check whose job is to be believed.
        if ($msg -notmatch 'Request$') {
            if ($serverBlob -notmatch "(?m)\b$([regex]::Escape($pascal))_?\b") { $unwritten += $key }
        }
    }
}

# ---------------------------------------------------------------------------
# 5. Report
# ---------------------------------------------------------------------------
if ($UpdateAllow) {
    $lines = @(
        '# Fields no client reads, and why that is CORRECT. Generated by eval/contract-sweep.ps1',
        '# -UpdateAllow; reasons are hand-written and preserved across regeneration. A field here',
        '# is a deliberate contract asymmetry. A field NOT here that no client reads is a defect.',
        ''
    )
    foreach ($u in ($unread | Sort-Object Key)) {
        $reason = if ($u.Reason) { $u.Reason } else { 'TODO: state why no client reads this' }
        $lines += "$($u.Key) = $reason"
    }
    Set-Content -Path $allowPath -Value $lines -Encoding ASCII
    Write-Host "[sweep] wrote $($unread.Count) entries to $allowPath" -ForegroundColor Cyan
    exit 0
}

$new = @($unread | Where-Object { -not $_.Allowed })
$known = @($unread | Where-Object { $_.Allowed })

Write-Host ''
Write-Host "[sweep] messages $($messages.Count) - fields $(($messages.Values | ForEach-Object { $_.Count } | Measure-Object -Sum).Sum)"
Write-Host "[sweep] unread by every client: $($unread.Count) ($($known.Count) allow-listed, $($new.Count) NEW)"

if ($All -and $known.Count -gt 0) {
    Write-Host ''
    Write-Host 'Allow-listed (unread on purpose):' -ForegroundColor DarkGray
    foreach ($u in ($known | Sort-Object Key)) { Write-Host ("  {0,-46} {1}" -f $u.Key, $u.Reason) -ForegroundColor DarkGray }
}

if ($unwritten.Count -gt 0) {
    Write-Host ''
    Write-Host "Response fields the SERVER never assigns ($($unwritten.Count)) -- always default on the wire:" -ForegroundColor Yellow
    foreach ($k in ($unwritten | Sort-Object)) { Write-Host "  $k" -ForegroundColor Yellow }
}

if ($new.Count -gt 0) {
    Write-Host ''
    Write-Host "NEW dead fields ($($new.Count)) -- computed and shipped, read by nothing:" -ForegroundColor Red
    foreach ($u in ($new | Sort-Object Key)) { Write-Host "  $($u.Key)  ($($u.Card) $($u.Type))" -ForegroundColor Red }
    Write-Host ''
    Write-Host 'Either wire it into a surface, delete it from the contract, or add it to' -ForegroundColor Red
    Write-Host "eval/expectations/contract-sweep-allow.txt WITH the reason it is correct." -ForegroundColor Red
    exit 1
}

Write-Host ''
Write-Host 'GATE: PASS - every response field is read by a client or allow-listed with a reason.' -ForegroundColor Green
exit 0
