param()

$ErrorActionPreference = 'Stop'

Write-Host "[quality] Restoring tools"
dotnet tool restore | Out-Null

if (-not (Test-Path 'build/logs')) {
    New-Item -ItemType Directory -Path 'build/logs' | Out-Null
}

Write-Host "[quality] Building solution"
dotnet build AssemblyChain-Core.sln --configuration Release --nologo

Write-Host "[quality] Running unit tests"
dotnet test AssemblyChain-Core.sln --configuration Release --no-build --logger "trx;LogFileName=build/logs/test-results.trx"

Write-Host "[quality] Running smoke benchmark (short job)"
dotnet run --project tests/AssemblyChain.Benchmarks/AssemblyChain.Benchmarks.csproj --configuration Release -- --filter * --job short --warmupCount 1 --iterationCount 1 --out build/logs/benchmarks

Write-Host "[quality] Completed"
