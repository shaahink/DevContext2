# eval/lens-audit.ps1 -- the octet lens harness (Prism D1.0c).
# Drives the built CLI (+ optionally MCP) over one octet repo or all eight, then runs FAIL probes:
#   P1 dead-map          map tokens << repo size (<400 tokens on a >100-cs-file repo)  [audit A5]
#   P2 archetype         JSON archetype != the intended verdict pinned below           [audit A1-A4]
#   P3 unknown+0entries  App-rendered map with style Unknown and no ENTRY POINTS       [audit A5/C4]
#   P4 sample-rows       per-service rows that are samples/demos/toys                  [audit A4]
#   P5 wall-time         first analyze wall > 2.5x the audit baseline                  [audit D2]
#   P6 mcp-drive         generic MCP smoke (analyze/overview/map/entrypoints all OK)   [audit G]
# Usage:
#   powershell -File eval/lens-audit.ps1 octet                # all 8, the D1 DoD proof form
#   powershell -File eval/lens-audit.ps1 dotnet-podcasts      # one repo
#   powershell -File eval/lens-audit.ps1 wolverine -SkipMcp   # CLI-only (faster)
# Output: eval-results/<date>/lens-run/<repo>/ (map.md, out.json, analyze-stdout.txt,
# mcp-transcript.txt, probes.txt) + summary.txt at the run root. Exit 0 only if all probes pass.
# Keep this file ASCII (PS 5.1 detached-run encoding gotcha).

param(
    [Parameter(Mandatory = $true, Position = 0)][string]$Target,
    [switch]$SkipMcp,
    [switch]$SkipBuild,
    [string]$OutRoot
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$CliExe = Join-Path $RepoRoot "src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe"
$DriveJs = Join-Path $RepoRoot "eval\mcp-qa\lens-drive.js"

# The octet: name -> intended archetype (aspirational until Prism D1 lands) + audit baseline
# wall seconds (2026-07-17, AUDIT.md method table). Baselines ratchet with the pinned SHAs.
$Octet = @(
    @{ Name = "Newtonsoft.Json";     Archetype = "Library"; BaselineSec = 30  }
    @{ Name = "refit";               Archetype = "Library"; BaselineSec = 25  }
    @{ Name = "StackExchange.Redis"; Archetype = "Library"; BaselineSec = 30  }
    @{ Name = "wolverine";           Archetype = "Library"; BaselineSec = 58  }
    @{ Name = "GitVersion";          Archetype = "CliTool"; BaselineSec = 18  }
    @{ Name = "dotnet-podcasts";     Archetype = "App";     BaselineSec = 8   }
    @{ Name = "ScreenToGif";         Archetype = "Desktop"; BaselineSec = 20  }
    @{ Name = "bitwarden-server";    Archetype = "App";     BaselineSec = 209 }
)

if ($Target -eq "octet") {
    $Selected = $Octet
} else {
    $Selected = @($Octet | Where-Object { $_.Name -eq $Target })
    if ($Selected.Count -eq 0) {
        Write-Host "Unknown repo '$Target'. Valid: octet, $(($Octet | ForEach-Object { $_.Name }) -join ', ')"
        exit 1
    }
}

if (-not $OutRoot) {
    $OutRoot = Join-Path $RepoRoot ("eval-results\{0}\lens-run" -f (Get-Date -Format "yyyy-MM-dd"))
}
New-Item -ItemType Directory -Force -Path $OutRoot | Out-Null

if (-not $SkipBuild) {
    Write-Host "== build CLI (stale-dll guard) =="
    & dotnet build (Join-Path $RepoRoot "src\DevContext.Cli") -v q --nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Host "CLI build FAILED"; exit 1 }
    if (-not $SkipMcp) {
        & dotnet build (Join-Path $RepoRoot "src\DevContext.Mcp") -v q --nologo | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Host "MCP build FAILED"; exit 1 }
    }
}
if (-not (Test-Path $CliExe)) { Write-Host "CLI exe not found: $CliExe"; exit 1 }

$Summary = @()
$TotalFails = 0

foreach ($Repo in $Selected) {
    $Name = $Repo.Name
    $RepoPath = Join-Path $RepoRoot ("eval-repos\" + $Name)
    $Out = Join-Path $OutRoot $Name
    New-Item -ItemType Directory -Force -Path $Out | Out-Null
    $Probes = @()

    Write-Host ""
    Write-Host "== $Name =="
    if (-not (Test-Path $RepoPath)) {
        $Probes += "FAIL missing-repo: $RepoPath not found (see eval/README.md Octet section)"
        $TotalFails++
        $Probes | Out-File (Join-Path $Out "probes.txt") -Encoding utf8
        $Summary += "$Name  MISSING"
        continue
    }

    $CsFiles = (Get-ChildItem -Path $RepoPath -Recurse -Filter *.cs -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '\\\.git\\' }).Count

    # -- timed analyze (markdown), then json --
    $MapPath = Join-Path $Out "map.md"
    $JsonPath = Join-Path $Out "out.json"
    $Sw = [System.Diagnostics.Stopwatch]::StartNew()
    $StdOut = & $CliExe analyze $RepoPath -o $MapPath --format markdown 2>$null
    $MdExit = $LASTEXITCODE
    $Sw.Stop()
    $WallSec = [Math]::Round($Sw.Elapsed.TotalSeconds, 1)
    $StdOut | Out-File (Join-Path $Out "analyze-stdout.txt") -Encoding utf8
    if ($MdExit -ne 0) { $Probes += "FAIL analyze-exit: markdown analyze exited $MdExit" }

    & $CliExe analyze $RepoPath -o $JsonPath --format json 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) { $Probes += "FAIL analyze-exit: json analyze exited $LASTEXITCODE" }

    $MapText = ""
    $MapTokens = 0
    if (Test-Path $MapPath) {
        $MapText = [System.IO.File]::ReadAllText($MapPath)
        $MapTokens = [Math]::Ceiling($MapText.Length / 4)
    }
    $Json = $null
    if (Test-Path $JsonPath) {
        try { $Json = Get-Content $JsonPath -Raw | ConvertFrom-Json } catch { $Probes += "FAIL json-parse: $($_.Exception.Message)" }
    }
    # CLI JSON is the kernel schema (devcontext/v1): flat archetype/architectureStyle.
    $Archetype = if ($Json) { [string]$Json.archetype } else { "?" }
    $Style = if ($Json) { [string]$Json.architectureStyle } else { "?" }

    # -- P1 dead-map --
    if ($CsFiles -gt 100 -and $MapTokens -lt 400) {
        $Probes += "FAIL dead-map: ~$MapTokens map tokens on a $CsFiles-cs-file repo (audit A5)"
    }
    # -- P2 archetype vs intended --
    if ($Archetype -ne $Repo.Archetype) {
        $Probes += "FAIL archetype: got '$Archetype', intended '$($Repo.Archetype)' (audit A1-A4)"
    }
    # -- P3 Unknown + 0 entries on an App-rendered map --
    if ($MapText.StartsWith("MAP ") -and $Style -eq "Unknown" -and $MapText -notmatch "ENTRY POINTS") {
        $Probes += "FAIL unknown-zero-entries: App map, style Unknown, no ENTRY POINTS section (audit A5/C4)"
    }
    # -- P4 sample rows in per-service --
    $SampleRows = @()
    if ($MapText -match "(?s)per service:(.*?)(\r?\n\r?\n|TOPOLOGY)") {
        $Block = $Matches[1]
        $SampleRows = @($Block -split "\r?\n" | Where-Object { $_ -match "^\s+\S" } |
            ForEach-Object { ($_ -split ":")[0].Trim() } |
            Where-Object { $_ -match "(?i)(sample|demo|chaos|playground|toy)" })
    }
    if ($SampleRows.Count -gt 0) {
        $Probes += "FAIL sample-rows: per-service table contains sample/demo hosts: $($SampleRows -join ', ') (audit A4)"
    }
    # -- P5 wall-time vs baseline --
    $Cap = [Math]::Round($Repo.BaselineSec * 2.5, 0)
    if ($WallSec -gt $Cap) {
        $Probes += "FAIL wall-time: ${WallSec}s > ${Cap}s (2.5x audit baseline $($Repo.BaselineSec)s)"
    }
    # -- P6 MCP drive --
    if (-not $SkipMcp) {
        & node $DriveJs $RepoPath (Join-Path $Out "mcp-transcript.txt") | Out-Null
        if ($LASTEXITCODE -ne 0) {
            $Probes += "FAIL mcp-drive: lens-drive.js exited $LASTEXITCODE (see mcp-transcript.txt)"
        }
    }
    # -- P7 insight validity (I2, Prism D2) --
    # Machine-checkable claims get checks: every graph.orphans evidence type is greped for liveness
    # (constructed, implemented/extended, or EF-indexed in the repo source). A hit = the insight
    # accused live code. All insights are RECORDED per repo (insights-recorded.json) so the
    # non-machine-checkable ones can carry manual verdicts.
    # --no-cache (D3.1): P7's claims-check must recompute insights independently, not validate the
    # very snapshot the analyze leg just cached.
    $StatsJson = & $CliExe query stats --path $RepoPath --no-cache 2>$null
    if ($LASTEXITCODE -eq 0 -and $StatsJson) {
        try {
            $Stats = ($StatsJson -join "`n") | ConvertFrom-Json
            if ($null -ne $Stats.insights) {
                $Stats.insights | ConvertTo-Json -Depth 6 |
                    Out-File (Join-Path $Out "insights-recorded.json") -Encoding utf8
                $Orphans = @($Stats.insights | Where-Object { $_.id -eq "graph.orphans" })
                foreach ($Card in $Orphans) {
                    $Types = @($Card.evidence | Where-Object { $_ -match '^[A-Za-z_][A-Za-z0-9_]*$' })
                    if ($Types.Count -eq 0) { continue }
                    $Alt = ($Types -join "|")
                    $LivePattern = "new\s+($Alt)\s*[({]|:\s*[^{;]*\b($Alt)\b|DbSet<($Alt)>"
                    $Hits = Get-ChildItem -Path $RepoPath -Recurse -Filter *.cs -File -ErrorAction SilentlyContinue |
                        Where-Object { $_.FullName -notmatch '\\\.git\\' } |
                        Select-String -Pattern $LivePattern -List
                    $LiveTypes = @()
                    foreach ($T in $Types) {
                        $TPattern = "new\s+$T\s*[({]|:\s*[^{;]*\b$T\b|DbSet<$T>"
                        if ($Hits | Where-Object { $_.Line -match $TPattern } | Select-Object -First 1) {
                            $LiveTypes += $T
                        }
                    }
                    if ($LiveTypes.Count -gt 0) {
                        $Probes += "FAIL orphans-validity: provably-live types in dead-code claim: $($LiveTypes -join ', ') (audit I1/I2)"
                    }
                }
            }
        } catch { $Probes += "FAIL insight-validity-probe: $($_.Exception.Message)" }
    }

    $FailCount = @($Probes | Where-Object { $_ -like "FAIL*" }).Count
    $TotalFails += $FailCount
    $Verdict = if ($FailCount -eq 0) { "PASS" } else { "FAIL ($FailCount)" }
    $Line = "$Name  wall=${WallSec}s  tokens=$MapTokens  cs=$CsFiles  archetype=$Archetype/$($Repo.Archetype)  style=$Style  => $Verdict"
    Write-Host $Line
    $Probes | ForEach-Object { Write-Host "    $_" }
    $Summary += $Line
    ("run: " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss")) | Out-File (Join-Path $Out "probes.txt") -Encoding utf8
    $Line | Out-File (Join-Path $Out "probes.txt") -Encoding utf8 -Append
    $Probes | Out-File (Join-Path $Out "probes.txt") -Encoding utf8 -Append
}

Write-Host ""
$Summary | Out-File (Join-Path $OutRoot "summary.txt") -Encoding utf8
if ($TotalFails -eq 0) {
    Write-Host "LENS-AUDIT: PASS ($($Selected.Count) repo(s))"
    "LENS-AUDIT: PASS" | Out-File (Join-Path $OutRoot "summary.txt") -Encoding utf8 -Append
    exit 0
} else {
    Write-Host "LENS-AUDIT: FAIL ($TotalFails probe failure(s) across $($Selected.Count) repo(s))"
    "LENS-AUDIT: FAIL ($TotalFails)" | Out-File (Join-Path $OutRoot "summary.txt") -Encoding utf8 -Append
    exit 1
}
