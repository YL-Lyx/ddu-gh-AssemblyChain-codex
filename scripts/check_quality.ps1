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

Write-Host "[quality] Auditing repository metrics"
python repo_audit.py src --output reports | Out-Null

$audit = Get-Content 'reports/audit_report.json' | ConvertFrom-Json
$files = @($audit.data.files)
$methods = @()
foreach ($file in $files) {
    foreach ($method in @($file.methods)) {
        $method | Add-Member -NotePropertyName FilePath -NotePropertyValue $file.path -Force
        $methods += $method
    }
}

$errors = @()
$cycleCount = @($audit.data.dependency_cycles).Count
if ($cycleCount -gt 0) {
    $cyclePreview = ($audit.data.dependency_cycles | Select-Object -First 1) -join ' -> '
    $errors += "Dependency cycles detected (limit 0). Example: $cyclePreview"
}

$duplicateCount = @($audit.data.duplicates).Count
if ($duplicateCount -gt 20) {
    $errors += "Duplicate fragments found: $duplicateCount (limit 20)"
}

$complexityBreaches = @($methods | Where-Object { $_.complexity -gt 15 })
if ($complexityBreaches.Count -gt 0) {
    $topComplexity = $complexityBreaches | Sort-Object -Property complexity -Descending | Select-Object -First 3
    foreach ($breach in $topComplexity) {
        $errors += "Cyclomatic complexity ${($breach.complexity)} exceeds 15 in $($breach.FilePath):$($breach.start_line) ($($breach.name))"
    }
}

$lengthBreaches = @($methods | Where-Object { $_.length -gt 120 })
if ($lengthBreaches.Count -gt 0) {
    $topLength = $lengthBreaches | Sort-Object -Property length -Descending | Select-Object -First 3
    foreach ($breach in $topLength) {
        $errors += "Method length ${($breach.length)} lines exceeds 120 in $($breach.FilePath):$($breach.start_line) ($($breach.name))"
    }
}

$publicMethods = @($methods | Where-Object { $_.is_public -eq $true })
$documentedPublic = @($publicMethods | Where-Object { $_.doc_present -eq $true })
$docCoverage = if ($publicMethods.Count -eq 0) { 1.0 } else { [double]$documentedPublic.Count / [double]$publicMethods.Count }
if ($docCoverage -lt 0.25) {
    $percentage = [math]::Round($docCoverage * 100, 2)
    $errors += "Public API documentation coverage $percentage% is below required 25%"
}

Write-Host ("[quality] Duplication: {0} fragments" -f $duplicateCount)
Write-Host ("[quality] Max complexity: {0}" -f (($methods | Measure-Object -Property complexity -Maximum).Maximum))
Write-Host ("[quality] Max method length: {0}" -f (($methods | Measure-Object -Property length -Maximum).Maximum))
Write-Host ("[quality] Public API doc coverage: {0:P2}" -f $docCoverage)

if ($errors.Count -gt 0) {
    foreach ($error in $errors) {
        Write-Error $error
    }
    throw "Quality gate failed."
}

Write-Host "[quality] Running smoke benchmark (short job)"
dotnet run --project tests/AssemblyChain.Benchmarks/AssemblyChain.Benchmarks.csproj --configuration Release -- --filter * --job short --warmupCount 1 --iterationCount 1 --out build/logs/benchmarks

Write-Host "[quality] Completed"
