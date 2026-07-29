# G8.2 differential verdict: every pole's graph dump must be byte-identical before and after.
# An empty diff is the acceptance for the fix; "the tests still pass" is not.
param(
    [string]$Before = "C:/code/DevContext2/eval-results/2026-07-29/G8/diff-before",
    [string]$After  = "C:/code/DevContext2/eval-results/2026-07-29/G8/diff-after",
    # The script writes its own verdict file. Piping it through Tee-Object does NOT work: the
    # `exit` below tears the pipeline down before Tee flushes, and you get no artifact at all.
    [string]$VerdictFile = "C:/code/DevContext2/eval-results/2026-07-29/G8/G8.2-DIFF-VERDICT.txt"
)

$ErrorActionPreference = "Stop"
$fail = 0
$rows = @()
foreach ($b in (Get-ChildItem $Before -Filter *.graphdump.txt | Sort-Object Name)) {
    $a = Join-Path $After $b.Name
    $pole = $b.Name -replace '\.graphdump\.txt$', ''
    if (-not (Test-Path $a)) { $rows += "MISSING  $pole (no AFTER capture)"; $fail++; continue }
    $hb = (Get-FileHash $b.FullName -Algorithm SHA256).Hash
    $ha = (Get-FileHash $a -Algorithm SHA256).Hash
    $len = (Get-Item $a).Length
    if ($hb -eq $ha) {
        $rows += ("IDENTICAL {0,-18} bytes={1,-8} sha256={2}" -f $pole, $len, $hb.Substring(0, 16))
    } else {
        $fail++
        $rows += ("DIFFERS   {0,-18} before={1} after={2}" -f $pole, $hb.Substring(0, 16), $ha.Substring(0, 16))
        # Show the first few differing lines so a real regression is readable, not just flagged.
        $d = Compare-Object (Get-Content $b.FullName) (Get-Content $a) | Select-Object -First 5
        foreach ($x in $d) { $rows += ("          {0} {1}" -f $x.SideIndicator, $x.InputObject.Substring(0, [Math]::Min(200, $x.InputObject.Length))) }
    }
}
$rows += ""
$rows += ("DIFF-VERDICT: {0} poles compared, {1} differ" -f (Get-ChildItem $Before -Filter *.graphdump.txt).Count, $fail)
$rows | ForEach-Object { Write-Host $_ }
$rows | Out-File -Encoding utf8 $VerdictFile
if ($fail -gt 0) { exit 1 } else { exit 0 }
