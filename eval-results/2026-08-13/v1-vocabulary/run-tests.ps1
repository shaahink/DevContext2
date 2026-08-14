$core = "C:/Code/DevContext2-engine/tests/DevContext.Core.Tests"
$srv  = "C:/Code/DevContext2-engine/tests/DevContext.Server.Tests"
dotnet test $core --filter "Category!=Eval" --no-build
Write-Host ("CORE EXIT=" + $LASTEXITCODE)
dotnet test $srv --filter "Category!=Eval" --no-build
Write-Host ("SERVER EXIT=" + $LASTEXITCODE)
