# G5.2 -- blast radius of the Defect-1 normalisation, stated in numbers.
# Counts `this.<member>.<call>(` sites per eval repo: the shape whose receiver type was previously
# always null and now resolves. A repo with 0 sites cannot move because of this change.
# ASCII only.
$ErrorActionPreference = 'Continue'
$repo = 'C:\code\DevContext2\eval-repos'
$pattern = 'this\.[A-Za-z_][A-Za-z0-9_]*\.[A-Za-z_][A-Za-z0-9_]*\s*\('
$rows = @()
foreach ($d in Get-ChildItem $repo -Directory) {
    $files = 0; $sites = 0
    foreach ($f in Get-ChildItem $d.FullName -Recurse -Filter *.cs -File -ErrorAction SilentlyContinue) {
        $m = @(Select-String -Path $f.FullName -Pattern $pattern -AllMatches -ErrorAction SilentlyContinue |
               ForEach-Object { $_.Matches.Count })
        if ($m.Count -gt 0) { $files++; $sites += ($m | Measure-Object -Sum).Sum }
    }
    $rows += [pscustomobject]@{ repo = $d.Name; filesWithShape = $files; sites = $sites }
    Write-Host ("{0,-26} {1,6} {2,8}" -f $d.Name, $files, $sites)
}
$rows | Sort-Object -Property sites -Descending |
    Export-Csv -Path 'C:\code\DevContext2\eval-results\2026-07-29\G5.2\blast-radius.csv' -NoTypeInformation
Write-Host 'BLASTDONE'
