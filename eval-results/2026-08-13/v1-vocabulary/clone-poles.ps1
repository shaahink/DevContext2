$ErrorActionPreference = 'Continue'
New-Item -ItemType Directory -Force -Path C:/Code/eval-poles | Out-Null
$poles = @(
  @{ n = 'FluentValidation'; u = 'https://github.com/FluentValidation/FluentValidation.git' },
  @{ n = 'AutoMapper';       u = 'https://github.com/AutoMapper/AutoMapper.git' },
  @{ n = 'MediatR';          u = 'https://github.com/jbogard/MediatR.git' },
  @{ n = 'dotnet-podcasts';  u = 'https://github.com/microsoft/dotnet-podcasts.git' }
)
foreach ($p in $poles) {
  $d = 'C:/Code/eval-poles/' + $p.n
  if (Test-Path $d) { Write-Host ("exists: " + $d); continue }
  Write-Host ("cloning " + $p.n)
  git clone --depth 1 $p.u $d
}
Write-Host "CLONE SWEEP DONE"
