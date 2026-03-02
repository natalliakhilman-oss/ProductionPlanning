# Update DB (migrations) and rebuild project
$ErrorActionPreference = "Stop"
$projDir = Join-Path $PSScriptRoot "ProductionPlanning"
$env:ASPNETCORE_ENVIRONMENT = "Development"

$dotnet = $null
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $dotnet = "dotnet"
} elseif (Test-Path "C:\Program Files\dotnet\dotnet.exe") {
    $dotnet = "C:\Program Files\dotnet\dotnet.exe"
} else {
    Write-Host "dotnet SDK not found. Install .NET 8 SDK."
    exit 1
}

Set-Location $projDir

Write-Host "Applying migrations..."
& $dotnet ef database update
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Building project..."
& $dotnet build --no-incremental
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Done: database updated, project built."
