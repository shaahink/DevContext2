# Probe: prove the new .NET-SHA256 Get-EngineStamp digest is IDENTICAL to the Get-FileHash form it
# replaced. Runs both over the same tree in the same process, so no file churn can separate them.
# Must be run in a shell where Get-FileHash IS available (pwsh, or a clean Windows PowerShell).
$ErrorActionPreference = 'Stop'
$gates = 'C:\Code\DevContext2-engine\eval\gates.ps1'
$repoRoot = 'C:\Code\DevContext2-engine'

$ast = [System.Management.Automation.Language.Parser]::ParseFile($gates, [ref]$null, [ref]$null)
$fn = $ast.FindAll({
    param($n)
    $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Get-EngineStamp'
}, $true) | Select-Object -First 1
Invoke-Expression $fn.Extent.Text

function Get-EngineStampOld {
    $stampPaths = @('src\DevContext.Core', 'src\DevContext.Cli', 'tests\DevContext.Core.Tests',
                    'eval\expectations', 'eval\fixtures', 'tests\fixtures')
    $exts = @('.cs', '.csproj', '.json', '.razor', '.props', '.proto', '.slnx', '.cshtml')
    $sb = New-Object System.Text.StringBuilder
    foreach ($p in $stampPaths) {
        $dir = Join-Path $repoRoot $p
        if (-not (Test-Path $dir)) { continue }
        Get-ChildItem $dir -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $exts -contains $_.Extension } | Sort-Object FullName | ForEach-Object {
                [void]$sb.AppendLine($_.FullName.Substring($repoRoot.Length) + ':' + (Get-FileHash $_.FullName -Algorithm SHA256).Hash)
            }
    }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($sb.ToString())
    $stream = New-Object System.IO.MemoryStream(,$bytes)
    (Get-FileHash -InputStream $stream -Algorithm SHA256).Hash
}

Write-Output ("GetFileHashAvailable=" + [string]([bool](Get-Command Get-FileHash -ErrorAction SilentlyContinue)))
$new = Get-EngineStamp
$old = Get-EngineStampOld
Write-Output ("NEW=" + $new)
Write-Output ("OLD=" + $old)
Write-Output ("IDENTICAL=" + [string]($new -eq $old))
