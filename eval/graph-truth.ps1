# eval/graph-truth.ps1 -- R1 graph-truth matrix harness (feat/graph-v2 S1, PLAN S1 step 3).
# Runs the built CLI over each repo and scores the 7 truth checks of R1 sec 2.1:
#   transport      DC3    ServiceLink edge counts by tag (bus/grpc/http-gateway/refit) vs ground truth
#   handler-join   DC1/2  every Sends edge target must have an outgoing Handles edge
#   hub-sanity     DC7    top-5 degree hubs must contain no test/fixture types
#   entry-target   DC4    share of entries whose target is a bare DI interface name (proxy metric)
#   style          DC7    archetype (+ optional style list) vs ground truth
#   sln-scope      DC6    >1 sln on disk silently scoped to one analysis => FAIL
#   dup-name       DC2n   same-short-name types spanning >1 project touched by cross-project edges
# Ground truth lives in eval/expectations/graph-truth/<repo>.json (one file per matrix repo; the
# matrix set = the files that exist). Sections absent from an expectation file fall back to the
# universal defaults below; transport/style need hand-written truth and SKIP when absent.
# Usage:
#   powershell -File eval/graph-truth.ps1 matrix               # every repo with an expectation file
#   powershell -File eval/graph-truth.ps1 eShop [-SkipBuild]   # one repo (case-insensitive)
# Output under <OutRoot> (default eval-results/<date>/graph-truth):
#   raw/<repo>/            kernel.json, entries.json, stats.json, graph.json, analyze-stdout.txt
#   verdicts/<repo>.json   machine-readable per-check verdicts + metrics
#   MATRIX.md              human grid, rebuilt from ALL verdict files between GRID markers only
#                          (probe answers / acceptance sections outside the markers are preserved)
# Exit code: 0 when every selected repo produced a verdict (FAIL cells are the documented baseline
# R2 batches flip -- they do NOT fail the run); 1 only on harness errors (ERR cells / bad setup).
# Keep this file ASCII (PS 5.1 detached-run encoding gotcha).

param(
    [Parameter(Mandatory = $true, Position = 0)][string]$Target,
    [switch]$SkipBuild,
    [string]$OutRoot,
    # S5 instrument change (declared in the S5 MATRIX): per-pole analyze budget. The widen set added
    # repos an order of magnitude larger than any previous pole (graphql-platform, PowerToys,
    # bitwarden/server); the first of them ran 28+ minutes without producing a dump, which is not a
    # verdict the harness could ever report -- it just stalled the matrix. A pole that cannot be
    # analyzed inside a usable budget is a FINDING (recorded as a TIMEOUT cell), not a reason to wait.
    [int]$TimeoutSec = 900
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$CliExe = Join-Path $RepoRoot "src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.exe"
$ExpectDir = Join-Path $RepoRoot "eval\expectations\graph-truth"

# JavaScriptSerializer parses the multi-MB graphdump far faster than ConvertFrom-Json in PS 5.1,
# and returns plain dictionaries (fast indexing in the edge loops below).
Add-Type -AssemblyName System.Web.Extensions
$Serializer = New-Object System.Web.Script.Serialization.JavaScriptSerializer
$Serializer.MaxJsonLength = [int]::MaxValue
$Serializer.RecursionLimit = 512

# S4 instrument fix 2: the old deny pattern was a case-insensitive SUBSTRING match, so
# "CreateStream" (crea-teSt-ream) reported MediatR's production Mediator.CreateStream as a fixture
# hub. Fixture-ness is a WORD in an identifier, so split on camel humps / separators and compare
# whole words.
$FixtureWords = @("test", "tests", "testing", "fixture", "fixtures", "mock", "mocks", "fake", "fakes", "stub", "stubs")
# S4 instrument fix 1: applied with -cmatch. PowerShell's -match ignores case, so "^I[A-Z]..."
# matched ItemPage and IdentifiedCommand -- every type starting with I counted as a DI interface,
# which is most of eShop's 12.1% entry-target FAIL.
$DiInterfacePattern = "^I[A-Z][A-Za-z0-9_]*$"
# S4 instrument fix 3: on a small graph a degree-1 node lands in the "top 5". A node that hubs
# nothing carries no signal about hubs, so hub-sanity ignores anything below this floor.
$HubMinDegree = 3

function Test-FixtureName([string]$Text) {
    if (-not $Text) { return $false }
    # Split identifiers into words: camel humps and any non-alphanumeric separator.
    $spaced = [regex]::Replace($Text, "([a-z0-9])([A-Z])", '$1 $2')
    $spaced = [regex]::Replace($spaced, "([A-Z]+)([A-Z][a-z])", '$1 $2')
    foreach ($w in ([regex]::Split($spaced, "[^A-Za-z0-9]+"))) {
        if ($w -and $FixtureWords -contains $w.ToLowerInvariant()) { return $true }
    }
    return $false
}

function Get-Prop($Obj, [string]$Name, $Default) {
    if ($null -ne $Obj -and $Obj.PSObject.Properties[$Name]) { return $Obj.$Name }
    return $Default
}

function Test-Range([int]$Value, $Range, [string]$Label, [System.Collections.Generic.List[string]]$Fails) {
    # $Range: expectation object with optional min/max; $null Range => key absent => must be 0
    # (a present transports section states the FULL transport truth for the repo).
    $min = 0; $max = 0
    if ($null -ne $Range) {
        $min = [int](Get-Prop $Range "min" 0)
        $max = [int](Get-Prop $Range "max" 2147483647)
    }
    if ($Value -lt $min) { $Fails.Add("$Label=$Value below min $min") }
    if ($Value -gt $max) { $Fails.Add("$Label=$Value above max $max") }
}

if (-not (Test-Path $ExpectDir)) { Write-Host "ERR expectations dir missing: $ExpectDir"; exit 1 }
$ExpectFiles = @(Get-ChildItem $ExpectDir -Filter *.json | Sort-Object Name)
if ($ExpectFiles.Count -eq 0) { Write-Host "ERR no expectation files in $ExpectDir"; exit 1 }

$Selected = @()
foreach ($f in $ExpectFiles) {
    $exp = Get-Content $f.FullName -Raw | ConvertFrom-Json
    # S4: a pole may declare its own id, so the SAME repo can appear twice under different scopes
    # (gitversion = read through --sln, gitversion-default = the flag-less read).
    $leaf = [string](Get-Prop $exp "id" "")
    if (-not $leaf) { $leaf = Split-Path ([string]$exp.repo) -Leaf }
    $Selected += ,@{ File = $f.FullName; Expect = $exp; Name = $leaf }
}
if ($Target -ne "matrix") {
    $Selected = @($Selected | Where-Object { $_.Name -ieq $Target })
    if ($Selected.Count -eq 0) {
        Write-Host "Unknown repo '$Target'. Valid: matrix, $((Get-ChildItem $ExpectDir -Filter *.json | ForEach-Object { $_.BaseName }) -join ', ')"
        exit 1
    }
}

if (-not $OutRoot) {
    $OutRoot = Join-Path $RepoRoot ("eval-results\{0}\graph-truth" -f (Get-Date -Format "yyyy-MM-dd"))
}
$RawRoot = Join-Path $OutRoot "raw"
$VerdictDir = Join-Path $OutRoot "verdicts"
New-Item -ItemType Directory -Force -Path $RawRoot, $VerdictDir | Out-Null

if (-not $SkipBuild) {
    Write-Host "== build CLI (stale-dll guard) =="
    & dotnet build (Join-Path $RepoRoot "src\DevContext.Cli") -v q --nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Host "CLI build FAILED"; exit 1 }
}
if (-not (Test-Path $CliExe)) { Write-Host "CLI exe not found: $CliExe"; exit 1 }

$HarnessErrors = 0

foreach ($Sel in $Selected) {
    $Name = $Sel.Name
    $Expect = $Sel.Expect
    $RepoPath = Join-Path $RepoRoot ([string]$Expect.repo -replace "/", "\")
    $Raw = Join-Path $RawRoot $Name
    New-Item -ItemType Directory -Force -Path $Raw | Out-Null

    Write-Host ""
    Write-Host "== $Name =="
    $Checks = [ordered]@{}
    foreach ($id in "transport", "handler-join", "hub-sanity", "entry-target", "style", "sln-scope", "dup-name") {
        $Checks[$id] = [ordered]@{ verdict = "ERR"; detail = "not evaluated"; metrics = [ordered]@{} }
    }

    if (-not (Test-Path $RepoPath)) {
        Write-Host "  ERR missing repo: $RepoPath"
        $HarnessErrors++
        foreach ($id in @($Checks.Keys)) { $Checks[$id].detail = "repo missing" }
        [ordered]@{ repo = $Name; error = "repo missing: $RepoPath" } | ConvertTo-Json |
            Out-File (Join-Path $VerdictDir "$Name.json") -Encoding utf8
        continue
    }

    # -- drive the CLI: analyze (timed) then cache-riding query ops --
    # S4: a pole may name the solution to analyze (Batch C's --sln). This is how the flag gets measured
    # the way a user meets it -- from the repo root -- instead of by pointing the tool at a subdirectory.
    $SlnArg = [string](Get-Prop $Expect "sln" "")
    $SlnOpt = @()
    if ($SlnArg) { $SlnOpt = @("--sln", $SlnArg); Write-Host "  --sln $SlnArg" }

    $KernelPath = Join-Path $Raw "kernel.json"
    $StdOutPath = Join-Path $Raw "analyze-stdout.txt"
    $Sw = [System.Diagnostics.Stopwatch]::StartNew()
    # Timeout-capped analyze (S5): run it as a child process so it can be KILLED at the budget.
    # `& $CliExe` cannot be interrupted from here, which is how one pole stalled the whole matrix.
    $AnalyzeArgs = @("analyze", $RepoPath) + $SlnOpt + @("-o", $KernelPath, "--format", "json")
    $Proc = Start-Process -FilePath $CliExe -ArgumentList $AnalyzeArgs -PassThru -NoNewWindow `
        -RedirectStandardOutput $StdOutPath -RedirectStandardError (Join-Path $Raw "analyze-stderr.txt")
    $TimedOut = $false
    if (-not $Proc.WaitForExit($TimeoutSec * 1000)) {
        $TimedOut = $true
        try { $Proc.Kill($true) } catch { }
        try { $Proc.WaitForExit(30000) | Out-Null } catch { }
    }
    $AnalyzeExit = if ($TimedOut) { -1 } else { $Proc.ExitCode }
    $Sw.Stop()
    $WallSec = [Math]::Round($Sw.Elapsed.TotalSeconds, 1)

    if ($TimedOut) {
        Write-Host "  TIMEOUT: analyze exceeded ${TimeoutSec}s -- pole recorded as TIMEOUT, not scored"
        $HarnessErrors++
        foreach ($id in @($Checks.Keys)) { $Checks[$id].verdict = "TIMEOUT"; $Checks[$id].detail = "analyze exceeded ${TimeoutSec}s" }
        [ordered]@{ repo = $Name; wallSec = $WallSec; error = "timeout"; checks = $Checks } |
            ConvertTo-Json -Depth 8 | Out-File (Join-Path $VerdictDir "$Name.json") -Encoding utf8
        continue
    }

    $EntriesText = (& $CliExe query entrypoints --path $RepoPath @SlnOpt 2>$null) -join "`n"
    $EntriesText | Out-File (Join-Path $Raw "entries.json") -Encoding utf8
    $StatsText = (& $CliExe query stats --path $RepoPath @SlnOpt 2>$null) -join "`n"
    $StatsText | Out-File (Join-Path $Raw "stats.json") -Encoding utf8
    $GraphText = (& $CliExe query graphdump --path $RepoPath @SlnOpt 2>$null) -join "`n"
    [System.IO.File]::WriteAllText((Join-Path $Raw "graph.json"), $GraphText)

    $Kernel = $null; $Entries = $null; $Stats = $null; $Graph = $null
    try {
        if (Test-Path $KernelPath) { $Kernel = Get-Content $KernelPath -Raw | ConvertFrom-Json }
        if ($EntriesText.Trim()) { $Entries = $EntriesText | ConvertFrom-Json }
        if ($StatsText.Trim()) { $Stats = $StatsText | ConvertFrom-Json }
        if ($GraphText.Trim()) { $Graph = $Serializer.DeserializeObject($GraphText) }
    } catch {
        Write-Host "  ERR parse: $($_.Exception.Message)"
    }
    if ($AnalyzeExit -ne 0 -or $null -eq $Kernel -or $null -eq $Graph -or $null -eq $Entries) {
        Write-Host "  ERR drive: analyze exit=$AnalyzeExit kernel=$([bool]$Kernel) graph=$([bool]$Graph) entries=$([bool]$Entries)"
        $HarnessErrors++
        foreach ($id in @($Checks.Keys)) { $Checks[$id].detail = "CLI drive failed (analyze exit $AnalyzeExit)" }
        [ordered]@{ repo = $Name; wallSec = $WallSec; error = "drive failed"; checks = $Checks } |
            ConvertTo-Json -Depth 8 | Out-File (Join-Path $VerdictDir "$Name.json") -Encoding utf8
        continue
    }
    $CacheStatus = if ($Stats) { [string](Get-Prop $Stats "snapshotCache" "?") } else { "?" }

    # -- single pass over the graph dump: node maps + edge aggregations --
    $NodeProj = @{}; $NodeTitle = @{}; $NodeKind = @{}
    $TitleProjects = @{}   # Type short name -> HashSet[project]
    foreach ($n in $Graph["nodes"]) {
        $id = [string]$n["id"]
        $NodeProj[$id] = $n["project"]
        $NodeTitle[$id] = [string]$n["title"]
        $NodeKind[$id] = [string]$n["kind"]
        if ($n["kind"] -eq "Type" -and $n["project"]) {
            $t = [string]$n["title"]
            if (-not $TitleProjects.ContainsKey($t)) {
                $TitleProjects[$t] = New-Object "System.Collections.Generic.HashSet[string]"
            }
            [void]$TitleProjects[$t].Add([string]$n["project"])
        }
    }

    # HandledFrom: a dispatched request joins its handler via Handles (commands/queries) OR
    # Consumes (published events) -- either outgoing edge on the send target counts as joined.
    $HandledFrom = New-Object "System.Collections.Generic.HashSet[string]"
    $SendsEdges = New-Object "System.Collections.Generic.List[object]"
    $CrossEdges = New-Object "System.Collections.Generic.List[object]"
    $Degree = @{}
    $Transports = [ordered]@{ bus = 0; grpc = 0; http = 0; refit = 0; aspire = 0; other = 0 }
    $HandlesCount = 0; $ConsumesCount = 0
    foreach ($e in $Graph["edges"]) {
        $from = [string]$e["from"]; $to = [string]$e["to"]; $kind = [string]$e["kind"]
        if ($Degree.ContainsKey($from)) { $Degree[$from]++ } else { $Degree[$from] = 1 }
        if ($Degree.ContainsKey($to)) { $Degree[$to]++ } else { $Degree[$to] = 1 }
        switch ($kind) {
            "Handles" { [void]$HandledFrom.Add($from); $HandlesCount++ }
            "Sends" { $SendsEdges.Add($e) }
            "Consumes" { [void]$HandledFrom.Add($from); $ConsumesCount++ }
            "ServiceLink" {
                $tag = ""
                if ($e.ContainsKey("tags") -and $e["tags"]) { $tag = [string]$e["tags"][0] }
                if ($tag -like "bus-publish*") { $Transports.bus++ }
                elseif ($tag -eq "grpc") { $Transports.grpc++ }
                # S3/Batch B: a typed client registered straight at a service address is HTTP too --
                # "via gateway" was never the only way to speak it, just the only way we could see.
                elseif ($tag -eq "http-via-gateway" -or $tag -eq "http-direct") { $Transports.http++ }
                elseif ($tag -eq "refit-direct") { $Transports.refit++ }
                elseif ($tag -eq "aspire-reference") { $Transports.aspire++ }
                else { $Transports.other++ }
            }
        }
        $fp = $NodeProj[$from]; $tp = $NodeProj[$to]
        if ($fp -and $tp -and $fp -ne $tp) { $CrossEdges.Add($e) }
    }

    # -- C1 transport (DC3) --
    $c = $Checks["transport"]
    $c.metrics = [ordered]@{
        bus = $Transports.bus; grpc = $Transports.grpc; http = $Transports.http
        refit = $Transports.refit; aspire = $Transports.aspire; untagged = $Transports.other
        eventWiringIntegration = [int](Get-Prop (Get-Prop $Kernel "eventWiring" $null) "integration" 0)
        eventWiringCrossService = [int](Get-Prop (Get-Prop $Kernel "eventWiring" $null) "crossService" 0)
    }
    $ExpTransports = Get-Prop $Expect "transports" $null
    if ($null -eq $ExpTransports) {
        $c.verdict = "SKIP"; $c.detail = "no transport truth declared"
    } else {
        $fails = New-Object "System.Collections.Generic.List[string]"
        Test-Range $Transports.bus (Get-Prop $ExpTransports "bus" $null) "bus" $fails
        Test-Range $Transports.grpc (Get-Prop $ExpTransports "grpc" $null) "grpc" $fails
        Test-Range $Transports.http (Get-Prop $ExpTransports "http" $null) "http" $fails
        Test-Range $Transports.refit (Get-Prop $ExpTransports "refit" $null) "refit" $fails
        Test-Range $Transports.aspire (Get-Prop $ExpTransports "aspire" $null) "aspire" $fails
        if ($fails.Count -eq 0) { $c.verdict = "PASS"; $c.detail = "bus=$($Transports.bus) grpc=$($Transports.grpc) http=$($Transports.http) refit=$($Transports.refit) aspire=$($Transports.aspire)" }
        else { $c.verdict = "FAIL"; $c.detail = $fails -join "; " }
    }

    # -- C2 handler-join (DC1/DC2) --
    $c = $Checks["handler-join"]
    $Unhandled = New-Object "System.Collections.Generic.HashSet[string]"
    foreach ($e in $SendsEdges) {
        $to = [string]$e["to"]
        if (-not $HandledFrom.Contains($to)) {
            $t = $NodeTitle[$to]; if (-not $t) { $t = $to }
            [void]$Unhandled.Add([string]$t)
        }
    }
    $MaxUnhandled = [int](Get-Prop (Get-Prop $Expect "handlerJoin" $null) "maxUnhandled" 0)
    $UnhandledSample = @($Unhandled | Sort-Object | Select-Object -First 5)
    $c.metrics = [ordered]@{
        sendsTotal = $SendsEdges.Count; handlesTotal = $HandlesCount; consumesTotal = $ConsumesCount
        unhandledTargets = $Unhandled.Count; sample = $UnhandledSample
    }
    if ($SendsEdges.Count -eq 0 -and $HandledFrom.Count -eq 0) {
        $c.verdict = "SKIP"; $c.detail = "no Sends/Handles wiring in graph"
    } elseif ($Unhandled.Count -le $MaxUnhandled) {
        $c.verdict = "PASS"; $c.detail = "$($SendsEdges.Count) sends, $($Unhandled.Count) unhandled (max $MaxUnhandled)"
    } else {
        $c.verdict = "FAIL"; $c.detail = "$($Unhandled.Count) of $($SendsEdges.Count) send targets have no Handles edge: $($UnhandledSample -join ', ')"
    }

    # -- C3 hub-sanity (DC7) --
    $c = $Checks["hub-sanity"]
    $Top5 = @($Degree.GetEnumerator() | Sort-Object -Property @{Expression="Value";Descending=$true}, @{Expression="Key";Descending=$false} | Select-Object -First 5)
    $Hubs = @($Top5 | Where-Object { $_.Value -ge $HubMinDegree })
    $Allow = @(Get-Prop (Get-Prop $Expect "hubs" $null) "allow" @())
    $BadHubs = @()
    foreach ($h in $Hubs) {
        $title = $NodeTitle[$h.Key]; if (-not $title) { $title = $h.Key }
        $proj = [string]$NodeProj[$h.Key]
        if (((Test-FixtureName $title) -or (Test-FixtureName $proj)) -and ($Allow -notcontains $title)) {
            $BadHubs += "$title($($h.Value))"
        }
    }
    $c.metrics = [ordered]@{
        minDegree = $HubMinDegree
        hubsScored = $Hubs.Count
        top5 = @($Top5 | ForEach-Object { [ordered]@{ title = $NodeTitle[$_.Key]; id = $_.Key; degree = $_.Value } })
    }
    if ($Hubs.Count -eq 0) {
        $c.verdict = "SKIP"; $c.detail = "no node reaches degree $HubMinDegree -- graph too small to have hubs"
    } elseif ($BadHubs.Count -eq 0) {
        $c.verdict = "PASS"; $c.detail = "top hubs clean: $(@($Hubs | ForEach-Object { ""$($NodeTitle[$_.Key])($($_.Value))"" }) -join ', ')"
    } else {
        $c.verdict = "FAIL"; $c.detail = "test/fixture hubs in top-5: $($BadHubs -join ', ')"
    }

    # -- C4 entry-target (DC4) --
    $c = $Checks["entry-target"]
    $EntryList = @(Get-Prop $Entries "entries" @())
    $WithTarget = 0; $DiTargets = 0; $DiSamples = New-Object "System.Collections.Generic.HashSet[string]"
    foreach ($en in $EntryList) {
        $target = [string](Get-Prop $en "target" "")
        if (-not $target) { continue }
        $WithTarget++
        $leaf = ($target -split "[.:]")[-1]
        if ($leaf -cmatch $DiInterfacePattern) {
            $DiTargets++
            if ($DiSamples.Count -lt 5) { [void]$DiSamples.Add("$(Get-Prop $en 'title' '?') -> $leaf") }
        }
    }
    $DiPct = if ($WithTarget -gt 0) { [Math]::Round(100.0 * $DiTargets / $WithTarget, 1) } else { 0 }
    $MaxDiPct = [double](Get-Prop (Get-Prop $Expect "entryTargets" $null) "maxDiInterfacePct" 25)
    # S3: an entry SURFACE that should exist but does not is a DC8 defect the quality ratio cannot
    # see -- a tool with one entry scores a perfect 0% DI-interface targets. Declared per repo.
    $MinEntries = [int](Get-Prop (Get-Prop $Expect "entryTargets" $null) "minEntries" 0)
    $c.metrics = [ordered]@{
        entries = $EntryList.Count; withTarget = $WithTarget; diInterfaceTargets = $DiTargets
        diInterfacePct = $DiPct; minEntries = $MinEntries; sample = @($DiSamples)
    }
    if ($EntryList.Count -lt $MinEntries) {
        $c.verdict = "FAIL"; $c.detail = "$($EntryList.Count) entries detected, expected at least $MinEntries (entry surface missing -- DC8)"
    } elseif ($EntryList.Count -eq 0) {
        $c.verdict = "SKIP"; $c.detail = "no entries detected (see style/DC8)"
    } elseif ($DiPct -le $MaxDiPct) {
        $c.verdict = "PASS"; $c.detail = "$DiTargets/$WithTarget targets are bare DI interfaces ($DiPct% <= $MaxDiPct%)"
    } else {
        $c.verdict = "FAIL"; $c.detail = "$DiPct% of entry targets are bare DI interfaces (max $MaxDiPct%): $(@($DiSamples) -join '; ')"
    }

    # -- C5 style (DC7) --
    $c = $Checks["style"]
    $GotArch = [string](Get-Prop $Kernel "archetype" "?")
    $GotStyle = [string](Get-Prop $Kernel "architectureStyle" "?")
    $c.metrics = [ordered]@{ archetype = $GotArch; style = $GotStyle; confidence = Get-Prop $Kernel "styleConfidence" 0 }
    $ExpStyle = Get-Prop $Expect "style" $null
    if ($null -eq $ExpStyle) {
        $c.verdict = "SKIP"; $c.detail = "no style truth declared (got $GotArch/$GotStyle)"
    } else {
        $ExpArchs = @(Get-Prop $ExpStyle "archetypes" @())
        if ($ExpArchs.Count -eq 0) { $a = Get-Prop $ExpStyle "archetype" $null; if ($a) { $ExpArchs = @($a) } }
        $ExpStyles = @(Get-Prop $ExpStyle "styles" @())
        $fails = @()
        if ($ExpArchs.Count -gt 0 -and $ExpArchs -notcontains $GotArch) { $fails += "archetype $GotArch not in [$($ExpArchs -join '|')]" }
        if ($ExpStyles.Count -gt 0 -and $ExpStyles -notcontains $GotStyle) { $fails += "style $GotStyle not in [$($ExpStyles -join '|')]" }
        if ($fails.Count -eq 0) { $c.verdict = "PASS"; $c.detail = "$GotArch/$GotStyle" }
        else { $c.verdict = "FAIL"; $c.detail = $fails -join "; " }
    }

    # -- C6 sln-scope (DC6) --
    # S4 instrument fix 4: the check used to be a pure disk count -- >1 sln = FAIL, unconditionally.
    # That was the right RED for a SILENT pick, but the fix Batch C builds is not "analyze everything":
    # it is "say which system you analyzed, and let the caller pick another". So the check now reads the
    # engine's own scope note out of kernel.json and passes when the note exists and its count agrees
    # with what is on disk. No note, or a disagreeing count, is still FAIL. The disk filter mirrors
    # SolutionCatalog.IsIgnoredPath (ANY dot-segment, bin, obj, node_modules, artifacts) so both sides
    # count the same set -- aspire-samples' .github decoy would otherwise break the match.
    $c = $Checks["sln-scope"]
    $Slns = @(Get-ChildItem -Path $RepoPath -Recurse -File -Include *.sln, *.slnx -ErrorAction SilentlyContinue |
        Where-Object {
            $rel = $_.FullName.Substring($RepoPath.Length)
            $bad = @($rel.Split([char[]]@([char]92, [char]47)) | Where-Object { $_ -and ($_.StartsWith(".") -or @("bin","obj","node_modules","artifacts") -contains $_) })
            $bad.Count -eq 0
        })
    $Scope = Get-Prop $Kernel "scope" $null
    $NoteCount = [int](Get-Prop $Scope "solutionsOnDisk" 0)
    $NoteAnalyzed = [string](Get-Prop $Scope "analyzed" "")
    $c.metrics = [ordered]@{
        slnOnDisk = $Slns.Count
        slnNames = @($Slns | Select-Object -First 10 | ForEach-Object { $_.FullName.Substring($RepoPath.Length + 1) })
        analyzedProjects = [int](Get-Prop $Kernel "projectCount" 0)
        noteSolutionsOnDisk = $NoteCount
        noteAnalyzed = $NoteAnalyzed
        noteRequested = [bool](Get-Prop $Scope "requested" $false)
        note = [string](Get-Prop $Scope "note" "")
    }
    $ExpAnalyzed = [string](Get-Prop (Get-Prop $Expect "slnScope" $null) "analyzed" "")
    if ($Slns.Count -eq 0) {
        # No solution on disk (a bare csproj repo) -- there is no choice to disclose, so no note is owed.
        $c.verdict = "PASS"; $c.detail = "no solution file on disk (folder-mode analysis)"
    } elseif ($null -eq $Scope) {
        $c.verdict = "FAIL"; $c.detail = "$($Slns.Count) sln(s) on disk but the analysis emits no scope note"
    } elseif ($Slns.Count -gt 1 -and $NoteCount -ne $Slns.Count) {
        $c.verdict = "FAIL"; $c.detail = "scope note counts $NoteCount solutions, $($Slns.Count) on disk"
    } elseif ($ExpAnalyzed -and $NoteAnalyzed -ne $ExpAnalyzed) {
        $c.verdict = "FAIL"; $c.detail = "analyzed '$NoteAnalyzed', expected '$ExpAnalyzed'"
    } elseif ($Slns.Count -le 1) {
        $c.verdict = "PASS"; $c.detail = "single solution ($NoteAnalyzed)"
    } else {
        $c.verdict = "PASS"; $c.detail = "$([string](Get-Prop $Scope 'note' ''))"
    }

    # -- C7 dup-name (DC2 noise) --
    $c = $Checks["dup-name"]
    $DupTouched = New-Object "System.Collections.Generic.HashSet[string]"
    foreach ($e in $CrossEdges) {
        foreach ($endId in @([string]$e["from"], [string]$e["to"])) {
            if ($NodeKind[$endId] -ne "Type") { continue }
            $t = $NodeTitle[$endId]
            if ($t -and $TitleProjects.ContainsKey($t) -and $TitleProjects[$t].Count -gt 1) {
                [void]$DupTouched.Add($endId)
            }
        }
    }
    $MaxDup = [int](Get-Prop (Get-Prop $Expect "dupNames" $null) "maxCrossTypes" 0)
    $DupSample = @($DupTouched | ForEach-Object { "$($NodeTitle[$_]) [$($NodeProj[$_])]" } | Sort-Object | Select-Object -First 5)
    $c.metrics = [ordered]@{
        crossProjectEdges = $CrossEdges.Count; dupNameTypesTouched = $DupTouched.Count; sample = $DupSample
    }
    if ($DupTouched.Count -le $MaxDup) {
        $c.verdict = "PASS"; $c.detail = "$($DupTouched.Count) dup-name types on cross-project edges (max $MaxDup)"
    } else {
        $c.verdict = "FAIL"; $c.detail = "$($DupTouched.Count) dup-name types touched by cross-project edges: $($DupSample -join ', ')"
    }

    # -- verdict file + console line --
    $Verdict = [ordered]@{
        repo = $Name
        run = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
        wallSec = $WallSec
        snapshotCache = $CacheStatus
        nodeCount = [int]$Graph["nodeCount"]
        edgeCount = [int]$Graph["edgeCount"]
        checks = $Checks
    }
    $Verdict | ConvertTo-Json -Depth 8 | Out-File (Join-Path $VerdictDir "$Name.json") -Encoding utf8

    $CellLine = @($Checks.Keys | ForEach-Object { "$($_)=$($Checks[$_].verdict)" }) -join " "
    Write-Host "  wall=${WallSec}s nodes=$($Graph["nodeCount"]) edges=$($Graph["edgeCount"])"
    Write-Host "  $CellLine"
    foreach ($id in $Checks.Keys) {
        if ($Checks[$id].verdict -eq "FAIL") { Write-Host "    FAIL $($id): $($Checks[$id].detail)" }
    }
}

# -- rebuild MATRIX.md grid from ALL verdict files (marker-managed; rest of file preserved) --
$MatrixPath = Join-Path $OutRoot "MATRIX.md"
$CheckIds = "transport", "handler-join", "hub-sanity", "entry-target", "style", "sln-scope", "dup-name"
$Grid = New-Object "System.Collections.Generic.List[string]"
$Grid.Add("Generated by eval/graph-truth.ps1 on $(Get-Date -Format 'yyyy-MM-dd HH:mm'). Cells: PASS / FAIL(detail in verdicts/<repo>.json) / SKIP (no truth declared) / TIMEOUT (analyze exceeded the per-pole budget -- see -TimeoutSec) / ERR.")
$Grid.Add("")
$Grid.Add("| repo | transport (DC3) | handler-join (DC1/2) | hub-sanity (DC7) | entry-target (DC4) | style (DC7) | sln-scope (DC6) | dup-name (DC2n) | wall s |")
$Grid.Add("|---|---|---|---|---|---|---|---|---|")
foreach ($vf in @(Get-ChildItem $VerdictDir -Filter *.json | Sort-Object Name)) {
    $v = Get-Content $vf.FullName -Raw | ConvertFrom-Json
    $err = [string](Get-Prop $v "error" "")
    if ($err) {
        # A TIMEOUT is a measurement, not a harness failure: it says the analysis did not finish inside
        # the budget, which is exactly what the widen set was added to find out. Keep it distinct from ERR.
        $cell = if ($err -eq "timeout") { "**TIMEOUT**" } else { "ERR" }
        $wall = [string](Get-Prop $v "wallSec" "-")
        $Grid.Add("| $($v.repo) | $cell | $cell | $cell | $cell | $cell | $cell | $cell | $wall |")
        continue
    }
    $cells = @($CheckIds | ForEach-Object {
        $ck = $v.checks.$_
        if ($ck.verdict -eq "FAIL") { "**FAIL**" } else { $ck.verdict }
    })
    $Grid.Add("| $($v.repo) | $($cells -join ' | ') | $($v.wallSec) |")
}
$GridText = $Grid -join "`r`n"

$BeginMark = "<!-- GRID:BEGIN -->"
$EndMark = "<!-- GRID:END -->"
if (Test-Path $MatrixPath) {
    $Existing = [System.IO.File]::ReadAllText($MatrixPath)
    $bi = $Existing.IndexOf($BeginMark); $ei = $Existing.IndexOf($EndMark)
    if ($bi -ge 0 -and $ei -gt $bi) {
        $NewText = $Existing.Substring(0, $bi + $BeginMark.Length) + "`r`n" + $GridText + "`r`n" + $Existing.Substring($ei)
        [System.IO.File]::WriteAllText($MatrixPath, $NewText, (New-Object System.Text.UTF8Encoding $false))
    } else {
        Write-Host "WARN: MATRIX.md exists without GRID markers -- grid not injected"
    }
} else {
    $Skeleton = @(
        "# Graph-truth matrix -- R1 instrument baseline"
        ""
        "Defect-class x repo grid (R1 sec 2.2/2.3). FAIL cells are the DOCUMENTED baseline that R2"
        "batches must flip -- see the acceptance section below. Raw evidence: raw/<repo>/, verdicts/<repo>.json."
        ""
        $BeginMark
        $GridText
        $EndMark
        ""
        "## Probe answers (S1 step 5)"
        ""
        "### DC3 -- eShop gRPC client registration: which detection fires?"
        ""
        "TODO"
        ""
        "### DC8 -- GitVersion command framework: why does F10 see 1 command?"
        ""
        "TODO"
        ""
        "## Batch A acceptance (declared before Batch A coding starts)"
        ""
        "TODO"
        ""
    ) -join "`r`n"
    [System.IO.File]::WriteAllText($MatrixPath, $Skeleton, (New-Object System.Text.UTF8Encoding $false))
}
Write-Host ""
Write-Host "MATRIX: $MatrixPath"
if ($HarnessErrors -gt 0) { Write-Host "GRAPH-TRUTH: ERR ($HarnessErrors harness error(s))"; exit 1 }
Write-Host "GRAPH-TRUTH: OK ($($Selected.Count) repo(s) scored)"
exit 0
